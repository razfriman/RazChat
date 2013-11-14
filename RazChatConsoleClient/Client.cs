using System;
using RazChat.Shared.Utility;
using System.Net.Sockets;
using System.Reflection;
using System.Collections.Generic;
using System.Net;
using RazChat.Shared;
using RazChat.Shared.Network;
using System.Threading;

namespace RazChat.ConsoleClient
{
	internal static class Client
	{
		private static LockFreeQueue<Callback> sCallbacks = new LockFreeQueue<Callback>();
		private static Socket sListener;
		private static RazChat.ConsoleClient.Network.Server sServer;

		public static void Main (string[] args)
		{
			Console.Title = "Raz Chat Console Client " + Version;
			Console.SetWindowSize(128, 64);

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

			sListener.BeginConnect("localhost",8484, new AsyncCallback(ConnectCallback), null);

			Log.WriteLine(ELogLevel.Info, "[Client] Connecting to Server");

			return true;
		}

		private static void ConnectCallback(IAsyncResult ar) {
			try {
				sListener.EndConnect (ar);
				Console.WriteLine ("Socket connected");
			} catch (Exception e) {
				Console.WriteLine (e.ToString ());
			}
		}

		public delegate void Callback();
	}
}