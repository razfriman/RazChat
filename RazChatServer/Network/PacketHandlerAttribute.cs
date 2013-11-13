using System;
using RazChat.Shared.Network;

namespace RazChat.Server.Network
{
	public delegate void PacketProcessor(Client pClient, Packet pPacket);

	public sealed class PacketHandlerAttribute : Attribute
	{
		public readonly EOpcode Opcode;
		public PacketProcessor Processor;

		public PacketHandlerAttribute(EOpcode pOpcode) { Opcode = pOpcode; }
	}
}

