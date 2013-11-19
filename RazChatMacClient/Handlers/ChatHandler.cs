using System;
using RazChat.MacClient.Network;
using RazChat.Shared.Network;
using RazChat.Shared;

namespace RazChat.MacClient.Handlers
{
	internal sealed class ChatHandlers
	{
		[PacketHandler(EOpcode.SMSG_WELCOME_MESSAGE)]
		public static void WelcomeMessage(Packet pPacket)
		{
			string message;
			pPacket.ReadString (out message);


			Client.sServer.WelcomeMessage = message;

			Client.window.InvokeOnMainThread (() => {
				Client.window.AddLineToChatHistory (string.Format ("Welcome Message: {0}", message));
			});
		}

		[PacketHandler(EOpcode.SMSG_CHAT_MESSAGE)]
		public static void ChatMessage(Packet pPacket)
		{
			string senderName;
			string message;

			pPacket.ReadString (out senderName);
			pPacket.ReadString (out message);

			Client.window.InvokeOnMainThread (() => {
				Client.window.AddLineToChatHistory (string.Format ("{0}: {1}", senderName, message));
			});
		}

		[PacketHandler(EOpcode.SMSG_UPDATE_USERNAME)]
		public static void UpdateUsername(Packet pPacket)
		{
			bool success = false;
			string username;

			pPacket.ReadBool (out success);

			if (success) {
				pPacket.ReadString (out username);
				Client.sUsername = username;

				Client.window.InvokeOnMainThread (() => {
					Client.window.AddLineToChatHistory (string.Format ("Username: {0}", username));
				});
			} else {
				Client.window.InvokeOnMainThread (() => {
					Client.window.AddLineToChatHistory (string.Format ("Cannot update username"));
				});
			}
		}
	}
}

