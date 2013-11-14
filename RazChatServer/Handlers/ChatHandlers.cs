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

			Log.WriteLine (ELogLevel.Info, "CHAT: {0}", message);

			Packet packet = new Packet(EOpcode.SMSG_CHAT_MESSAGE);
			packet.WriteString (pClient.Username);
			packet.WriteString(message);
			Server.SendPacketToAllExcept (packet, pClient);


		}
	}
}

