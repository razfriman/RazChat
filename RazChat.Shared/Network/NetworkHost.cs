using System;
using System.Collections.Generic;
using System.Net.Sockets;
using RazChat.Shared.Network;
using RazChat.Shared.Utility;
using System.Net;
using System.Threading;
using RazChat.Shared;

namespace RazChat.Shared.Network
{
	public class NetworkHost
	{
		private const int MAX_RECEIVE_BUFFER = 16384;

		private Socket mSocket = null;
		private int mDisconnected = 0;

		private byte[] mReceiveBuffer = null;
		private int mReceiveStart = 0;
		private int mReceiveLength = 0;
		private DateTime mReceiveLast = DateTime.Now;
		private LockFreeQueue<ByteArraySegment> mSendSegments = new LockFreeQueue<ByteArraySegment>();
		private int mSending = 0;
		private ushort mReceivingPacketLength = 0;
		private bool mReceivedHandshakePacket = false;

		private string mHost = null;
		private string mWelcomeMessage = "";

		public NetworkHost(Socket pSocket)
		{
			mSocket = pSocket;
			mReceiveBuffer = new byte[MAX_RECEIVE_BUFFER];
			mHost = ((IPEndPoint)mSocket.RemoteEndPoint).Address.ToString();
			Log.WriteLine(ELogLevel.Debug, "[{0}] Connected", Host);
			BeginReceive();
		}

		public string Host { get { return mHost; } }

		public void Disconnect()
		{
			if (Interlocked.CompareExchange(ref mDisconnected, 1, 0) == 0)
			{
				try {
					mSocket.Shutdown(SocketShutdown.Both);
					mSocket.Close();
					Log.WriteLine(ELogLevel.Debug, "[{0}] Disconnected", Host);
				} catch (SocketException se) {
					Log.WriteLine(ELogLevel.Debug, "[{0}] Disconnected", Host);
				}
			}
		}

		public virtual void OnReceiveHandshakePacket(Packet pPacket) {

		}

		public virtual void OnReceivePacket(Packet pPacket) {

		}

		private void EndReceive(SocketAsyncEventArgs pArguments)
		{
			if (mDisconnected != 0) return;
			if (pArguments.BytesTransferred <= 0)
			{
				if (pArguments.SocketError != SocketError.Success && pArguments.SocketError != SocketError.ConnectionReset) Log.WriteLine(ELogLevel.Error, "[{0}] Receive Error: {1}", Host, pArguments.SocketError);
				Disconnect();
				return;
			}
			mReceiveLength += pArguments.BytesTransferred;

			while (mReceiveLength > 4)
			{
				if (mReceivingPacketLength == 0)
				{
					mReceivingPacketLength = GetHeaderLength(mReceiveBuffer, mReceiveStart);
				}
				if (mReceivingPacketLength > 0 && mReceiveLength >= mReceivingPacketLength + 4)
				{
					if (!mReceivedHandshakePacket && mReceiveStart == 0) {
						// Handshake packet
						mReceivedHandshakePacket = true;
						Packet packet = new Packet(mReceiveBuffer, mReceiveStart + 4, mReceivingPacketLength, false);
						OnReceiveHandshakePacket (packet);

					} else {
						Packet packet = new Packet(mReceiveBuffer, mReceiveStart + 4, mReceivingPacketLength);

						OnReceivePacket (packet);
					}

					mReceiveStart += mReceivingPacketLength + 4;
					mReceiveLength -= mReceivingPacketLength + 4;
					mReceivingPacketLength = 0;
					mReceiveLast = DateTime.Now;
				}
			}

			if (mReceiveLength == 0) mReceiveStart = 0;
			else if (mReceiveStart > 0 && (mReceiveStart + mReceiveLength) >= mReceiveBuffer.Length)
			{
				Buffer.BlockCopy(mReceiveBuffer, mReceiveStart, mReceiveBuffer, 0, mReceiveLength);
				mReceiveStart = 0;
			}
			if (mReceiveLength == mReceiveBuffer.Length)
			{
				Log.WriteLine(ELogLevel.Error, "[{0}] Receive Overflow", Host);
				Disconnect();
			}
			else BeginReceive();
		}

		private void BeginReceive()
		{
			if (mDisconnected != 0) return;
			SocketAsyncEventArgs args = new SocketAsyncEventArgs();
			args.Completed += (s, a) => EndReceive(a);
			args.SetBuffer(mReceiveBuffer, mReceiveStart, mReceiveBuffer.Length - (mReceiveStart + mReceiveLength));
			try { if (!mSocket.ReceiveAsync(args)) EndReceive(args); }
			catch (ObjectDisposedException) { }
		}

		private ushort GetHeaderLength(byte[] pBuffer, int pStart)
		{
			int length = (int)pBuffer[pStart] |
			             (int)(pBuffer[pStart + 1] << 8) |
			             (int)(pBuffer[pStart + 2] << 16) |
			             (int)(pBuffer[pStart + 3] << 24);
			length = (length >> 16) ^ (length & 0xFFFF);
			return (ushort)length;
		}

		public void SendHandshake(ushort pBuild)
		{
			byte[] buffer = new byte[6];
			buffer [0] = (byte)0x02;
			buffer [1] = 0;
			buffer [2] = 0;
			buffer [3] = 0;

			buffer [4] = (byte)pBuild;
			buffer [5] = (byte)(pBuild >> 8);

			Send(buffer);
		}

		private void Send(byte[] pBuffer)
		{
			if (mDisconnected != 0) return;
			mSendSegments.Enqueue(new ByteArraySegment(pBuffer));
			if (Interlocked.CompareExchange(ref mSending, 1, 0) == 0) BeginSend();
		}

		public void GenerateHeader(byte[] pBuffer, int pLength)
		{
			pBuffer[0] = (byte)pLength;
			pBuffer[1] = (byte)(pLength >> 8);
			pBuffer[2] = (byte)(pLength >> 16);
			pBuffer[3] = (byte)(pLength >> 24);
		}

		public void SendPacket(Packet pPacket)
		{
			if (mDisconnected != 0) return;
			byte[] buffer = new byte[pPacket.Length + 4];
			GenerateHeader (buffer, pPacket.Length);
			Buffer.BlockCopy(pPacket.InnerBuffer, 0, buffer, 4, pPacket.Length);
			Send(buffer);
		}

		private void BeginSend()
		{
			SocketAsyncEventArgs args = new SocketAsyncEventArgs();
			args.Completed += (s, a) => EndSend(a);
			ByteArraySegment segment = mSendSegments.Next;
			args.SetBuffer(segment.Buffer, segment.Start, segment.Length);
			try { if (!mSocket.SendAsync(args)) EndSend(args); }
			catch (ObjectDisposedException) { }
		}
		private void EndSend(SocketAsyncEventArgs pArguments)
		{
			if (mDisconnected != 0)
				return;
			if (pArguments.BytesTransferred <= 0) {
				if (pArguments.SocketError != SocketError.Success)
					Log.WriteLine (ELogLevel.Error, "[{0}] Send Error: {1}", Host, pArguments.SocketError);
				Disconnect ();
				return;
			}
			if (mSendSegments.Next.Advance (pArguments.BytesTransferred))
				mSendSegments.Dequeue ();
			if (mSendSegments.Next != null)
				BeginSend ();
			else
				mSending = 0;
		}
	}
}