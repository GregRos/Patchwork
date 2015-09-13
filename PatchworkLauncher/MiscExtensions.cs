using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace PatchworkLauncher {
	internal static class MiscExtensions {
		public static T Deserialize<T>(this XmlSerializer serializer, string path, T defaultValue) {
			using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
				if (true || serializer.CanDeserialize(new XmlTextReader(stream))) {
					var result = serializer.Deserialize(stream);
					if (result is T) {
						return (T) result;
					}
				}
				return defaultValue;
			}
		}
	}
}
