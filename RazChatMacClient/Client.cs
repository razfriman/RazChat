using System;
using RazChat.Shared.Utility;
using System.Net.Sockets;
using System.Reflection;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using RazChat.MacClient.Network;
using RazChat.Shared;
using RazChat.Shared.Network;
using System.Threading.Tasks;
using System.Linq;
using RazChatMacClient;

namespace RazChat.MacClient
{
	internal static class Client
	{
		private static LockFreeQueue<Callback> sCallbacks = new LockFreeQueue<Callback>();
		private static Socket sServerSocket;
		private static int sRetryCount = 0;

		internal static MainWindowController window;
		internal static Server sServer;

		private static Callback callback;

		public static void Load ()
		{

			Config.Load ();

			if (!Initialize()) return;

			Task.Factory.StartNew (HandleCallbacksAsync);
		}

		public static void HandleCallbacksAsync() {



			while (true)
			{
				while (sCallbacks.Dequeue (out callback)) {
					Log.WriteLine (ELogLevel.Info, "Checking callback2");
					callback ();

				}
				Thread.Sleep(1);
			}
		}

		internal static string Version { get { return Assembly.GetEntryAssembly().GetName().Version.ToString(); } }

		public static void SendMessage(string pMessage) {
			Packet packet = new Packet (EOpcode.CMSG_CHAT_MESSAGE);
			packet.WriteString (pMessage);
			sServer.SendPacket (packet);
		}

		public static void AddCallback(Callback pCallback) { sCallbacks.Enqueue(pCallback); }

		private static bool Initialize()
		{

			List<Tuple<InitializerAttribute, InitializerCallback>> initializers = Reflector.FindAllMethods<InitializerAttribute, InitializerCallback>();
			initializers.Sort((p1, p2) => p1.Item1.Stage.CompareTo(p2.Item1.Stage));
			if (!initializers.TrueForAll(p => p.Item2())) return false;

			sServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			Log.WriteLine(ELogLevel.Info, "[Client] Connecting to Server");

			sServerSocket.BeginConnect("localhost",8484, new AsyncCallback(ConnectCallback), null);

			return true;
		}

		private static void ConnectCallback(IAsyncResult ar) {
			try {
				sServerSocket.EndConnect (ar);
				Log.WriteLine(ELogLevel.Info, "[Client] Connected to Server");

				sServer = new Server(sServerSocket);

				Packet p = new Packet(EOpcode.CMSG_CHAT_MESSAGE);
				p.WriteString("Hello");
				sServer.SendPacket(p);

			} catch (Exception e) {
				sRetryCount++;
				Log.WriteLine(ELogLevel.Info, "[Client] Could not connect to server: {0}", e.Message);

				if (sRetryCount < 3) {
					Log.WriteLine (ELogLevel.Info, "[Client] Reattempting to connect to server");
					Thread.Sleep (1000);
					sServerSocket.BeginConnect ("localhost", 8484, new AsyncCallback (ConnectCallback), null);
				} else {
					Log.WriteLine (ELogLevel.Error, "[Client] Cannot connect to server");
				}
			}
		}

		public delegate void Callback();
	}
}