using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;

namespace RazChat.MacClient
{
	public sealed class Config
	{
		internal static Config Instance { get; private set; }

		internal static void Load()
		{

			string currentDir = Path.GetDirectoryName (Assembly.GetEntryAssembly ().Location);
			using (XmlReader reader = XmlReader.Create (Path.Combine(currentDir, "Config.xml"))) {
				Instance = (Config)(new XmlSerializer (typeof(Config))).Deserialize (reader);
			}
		}

		public ushort Build;
		public ushort Port;
		public string ExternalAddress;
	}
}