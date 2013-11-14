using System;
using System.Xml;
using System.Xml.Serialization;

namespace RazChat.ConsoleClient
{
	public sealed class Config
	{
		internal static Config Instance { get; private set; }

		internal static void Load()
		{
			using (XmlReader reader = XmlReader.Create ("Config.xml")) {
				Instance = (Config)(new XmlSerializer (typeof(Config))).Deserialize (reader);
			}
		}

		public ushort Build;
		public ushort Port;
		public string ExternalAddress;
	}
}