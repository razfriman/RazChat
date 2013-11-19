using System;
using RazChat.Shared.Network;
using RazChat.Server.Network;
using RazChat.Shared;

namespace RazChat.Server.Handlers
{
	internal sealed class ChatHandlers
	{
		[PacketHandler(EOpcode.CMSG_CHAT_MESSAGE)]
		public static void ChatMessage(Client pClient, Packet pPacket)
		{
			string message;
			pPacket.ReadString (out message);

			Log.WriteLine (ELogLevel.Info, "[Server] Received Chat Message: {0}", message);

			Packet packet = new Packet(EOpcode.SMSG_CHAT_MESSAGE);
			packet.WriteString (pClient.Username);
			packet.WriteString(message);
			Server.SendPacketToAllExcept (packet, pClient);
		}

		[PacketHandler(EOpcode.CMSG_UPDATE_USERNAME)]
		public static void UsernameChange(Client pClient, Packet pPacket) {
			string username;
			pPacket.ReadString (out username);

			Log.WriteLine (ELogLevel.Info, "[Server] Username change request recevied: {0} -> {1}", pClient.Username, username);

			Server.UpdateUsername (pClient, username);
		}
	}
}

