using System;
using RazChat.Shared;
using RazChat.Shared.Network;

namespace RazChat.ConsoleClient.Network
{
	public delegate void PacketProcessor(Packet pPacket);

	public sealed class PacketHandlerAttribute : Attribute
	{
		public readonly EOpcode Opcode;
		public PacketProcessor Processor;

		public PacketHandlerAttribute(EOpcode pOpcode) { Opcode = pOpcode; }
	}
}

