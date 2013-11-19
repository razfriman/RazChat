using System;

namespace RazChat.Shared.Network
{
	public enum EOpcode : ushort
	{
		SMSG_CHAT_MESSAGE               = 0x0001,
		SMSG_WELCOME_MESSAGE            = 0x0002,
		SMSG_UPDATE_USERNAME            = 0x0003,

		CMSG_CHAT_MESSAGE               = 0x0001,
		CMSG_UPDATE_USERNAME            = 0x0002,


		MSG_NONE                        = 0xFFFF
	}
}