using System;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;
using System.IO;
using RazChat.Server.Utility;
using RazChat.Server.Network;
using RazChat.Shared.Network;
using RazChat.Shared.Utility;
using System.Net;
using System.Reflection;
using RazChat.Shared;

namespace RazChat.Server
{
	internal static class Server
	{
		private static LockFreeQueue<Callback> sCallbacks = new LockFreeQueue<Callback>();
		private static Socket sListener;
		private static List<Client> sClients = new List<Client>();
		private static int sClientIdCounter = 1;

		public static void Main (string[] args)
		{
			Console.Title = "Raz Chat Server " + Version;
			Console.SetWindowSize(128, 64);

			Config.Load();

			if (!Initialize()) return;

			Callback callback;

			while (true)
			{
				while (sCallbacks.Dequeue(out callback)) callback();
				Thread.Sleep(1);
			}
		}

		internal static string Version { get { return Assembly.GetEntryAssembly().GetName().Version.ToString(); } }

		public static void AddCallback(Callback pCallback) { sCallbacks.Enqueue(pCallback); }

		private static bool Initialize()
		{

			List<Tuple<InitializerAttribute, InitializerCallback>> initializers = Reflector.FindAllMethods<InitializerAttribute, InitializerCallback>();
			initializers.Sort((p1, p2) => p1.Item1.Stage.CompareTo(p2.Item1.Stage));
			if (!initializers.TrueForAll(p => p.Item2())) return false;

			sListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			sListener.Bind(new IPEndPoint(IPAddress.Any, Config.Instance.Port));
			sListener.Listen(Config.Instance.Backlog);
			Log.WriteLine(ELogLevel.Info, "[Server] Initialized Listener");

			BeginListenerAccept(null);

			return true;
		}

		internal static void SendPacketToAllExcept(Packet pPacket, Client pExcept) { sClients.ForEach(p => { if (p != pExcept) p.SendPacket(pPacket); }); }
		internal static void SendPacketToAll(Packet pPacket) { sClients.ForEach(p => p.SendPacket(pPacket)); }

		internal static void SendUniqueName(Client pClient) {
			Packet packet = new Packet(EOpcode.SMSG_UPDATE_USERNAME);
			packet.WriteString ("user_" + pClient.Identifier);
			pClient.SendPacket (packet);
		}

		internal static void SendWelcomeMessage(Client pClient) {
			Packet packet = new Packet(EOpcode.SMSG_WELCOME_MESSAGE);
			packet.WriteString (Config.Instance.WelcomeMessage);
			pClient.SendPacket (packet);
		}

		internal static void ClientConnected(Client pClient) { 
			SendUniqueName (pClient);
			SendWelcomeMessage (pClient);
		}

		internal static void ClientDisconnected(Client pClient) { lock (sClients) sClients.Remove(pClient); }

		private static void BeginListenerAccept(SocketAsyncEventArgs pArgs)
		{
			if (pArgs == null)
			{
				pArgs = new SocketAsyncEventArgs();
				pArgs.Completed += (s, a) => EndListenerAccept(a);
			}
			pArgs.AcceptSocket = null;
			if (!sListener.AcceptAsync(pArgs)) EndListenerAccept(pArgs);
		}


		private static void EndListenerAccept(SocketAsyncEventArgs pArguments)
		{
			try {
				if (pArguments.SocketError == SocketError.Success) {
					Client client = new Client (pArguments.AcceptSocket, sClientIdCounter);
					sClientIdCounter++;

					lock (sClients) {
						sClients.Add (client);
					}

					client.SendHandshake(Config.Instance.Build);

					ClientConnected(client);

					BeginListenerAccept (pArguments);
				} else if (pArguments.SocketError != SocketError.OperationAborted)
					Log.WriteLine (ELogLevel.Error, "[Server] Listener Error: {0}", pArguments.SocketError);
			} catch (ObjectDisposedException) {
			} catch (Exception exc) {
				Log.WriteLine (ELogLevel.Exception, "[Server] Listener Exception: {0}", exc.Message);
			}
		}

		public delegate void Callback();
	}
}