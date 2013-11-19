using System;
using System.Collections.Generic;
using System.Net.Sockets;
using RazChat.Shared.Network;
using RazChat.Shared.Utility;
using System.Net;
using System.Threading;
using RazChat.Shared;

namespace RazChat.MacClient.Network
{
	public sealed class Server : NetworkHost
	{
		private static Dictionary<EOpcode, PacketHandlerAttribute> sHandlers = new Dictionary<EOpcode, PacketHandlerAttribute>();

		[Initializer(1)]
		public static bool InitializeHandlers()
		{
			List<Tuple<PacketHandlerAttribute, PacketProcessor>> handlers = Reflector.FindAllMethods<PacketHandlerAttribute, PacketProcessor>();
			handlers.ForEach(d => { d.Item1.Processor = d.Item2; sHandlers.Add(d.Item1.Opcode, d.Item1); });
			Log.WriteLine(ELogLevel.Info, "[Client] Initialized {0} Server Packet Handlers", sHandlers.Count);
			return true;
		}

		private string mWelcomeMessage = "";

		public Server(Socket pSocket) : base(pSocket)
		{

		}

		public string WelcomeMessage { get { return mWelcomeMessage; } set { mWelcomeMessage = value; } }


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
				Client.AddCallback (() => handler.Processor (pPacket));
			else {
				Log.WriteLine (ELogLevel.Debug, "[{0}] Receiving 0x{1}, {2} Bytes", Host, ((ushort)pPacket.Opcode).ToString ("X4"), pPacket.Length);
				pPacket.Dump ();
			}
		}
	}
}