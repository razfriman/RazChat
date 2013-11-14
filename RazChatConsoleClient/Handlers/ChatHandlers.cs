using System;
using RazChat.ConsoleClient.Network;
using RazChat.Shared.Network;
using RazChat.Shared;

namespace RazChat.ConsoleClient
{
	internal sealed class ChatHandlers
	{
		[PacketHandler(EOpcode.SMSG_WELCOME_MESSAGE)]
		public static void WelcomeMessage(Packet pPacket)
		{
			string message;
			pPacket.ReadString (out message);

			Log.WriteLine (ELogLevel.Info, "Welcome Message: {0}", message);
		}

		[PacketHandler(EOpcode.SMSG_CHAT_MESSAGE)]
		public static void ChatMessage(Packet pPacket)
		{
			string senderName;
			string message;

			pPacket.ReadString (out senderName);
			pPacket.ReadString (out message);

			Log.WriteLine (ELogLevel.Info, "Chat Message: {0} - {1}", senderName, message);
		}

		[PacketHandler(EOpcode.SMSG_UPDATE_USERNAME)]
		public static void UpdateUsername(Packet pPacket)
		{
			string username;

			pPacket.ReadString (out username);

			Log.WriteLine (ELogLevel.Info, "Username: {0}", username);
		}
	}
}

