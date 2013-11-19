using System;
using System.Collections.Generic;
using System.Net.Sockets;
using RazChat.Shared.Network;
using RazChat.Shared.Utility;
using System.Net;
using System.Threading;
using RazChat.Shared;

namespace RazChat.Server.Network
{
	public sealed class Client : NetworkHost
	{
		private static Dictionary<EOpcode, PacketHandlerAttribute> sHandlers = new Dictionary<EOpcode, PacketHandlerAttribute>();

		[Initializer(1)]
		public static bool InitializeHandlers()
		{
			List<Tuple<PacketHandlerAttribute, PacketProcessor>> handlers = Reflector.FindAllMethods<PacketHandlerAttribute, PacketProcessor>();
			handlers.ForEach(d => { d.Item1.Processor = d.Item2; sHandlers.Add(d.Item1.Opcode, d.Item1); });
			Log.WriteLine(ELogLevel.Info, "[Server] Initialized {0} Client Packet Handlers", sHandlers.Count);
			return true;
		}

		private int mIdentifier = 0;
		private string mUsername = null;

		public Client(Socket pSocket, int pIdentifier) : base(pSocket)
		{
			mIdentifier = pIdentifier;
			mUsername = "user_" + pIdentifier;
		}

		public int Identifier { get { return mIdentifier; } }
		public string Username { get { return mUsername; } set { mUsername = value; } }

		public override void OnReceiveHandshakePacket (Packet pPacket)
		{
			ushort build;
			pPacket.ReadUShort (out build);

			if (build != Config.Instance.Build) {
				Log.WriteLine (ELogLevel.Warn, "[Client] Build version mismatch. Disconnecting from server");
				Disconnect ();
			}
		}

		public override void OnReceivePacket (Packet pPacket)
		{
			PacketHandlerAttribute handler = sHandlers.GetOrDefault (pPacket.Opcode, null);
			if (handler != null)
				Server.AddCallback (() => handler.Processor (this, pPacket));
			else {
				Log.WriteLine (ELogLevel.Debug, "[{0}] Receiving 0x{1}, {2} Bytes", Host, ((ushort)pPacket.Opcode).ToString ("X4"), pPacket.Length);
				pPacket.Dump ();
			}
		}
	}
}