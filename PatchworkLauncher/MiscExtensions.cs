using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Patchwork.Utility;
using Serilog;

namespace PatchworkLauncher {
	internal static class MiscExtensions {



		public static T Deserialize<T>(this XmlSerializer serializer, string path, T defaultValue) {
			if (!File.Exists(path)) {
				return defaultValue;
			}
			using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
				var reader = new XmlTextReader(stream);
				if (!serializer.CanDeserialize(reader)) {
					return defaultValue;
				}
				var result = serializer.Deserialize(reader);
				result = result ?? defaultValue;
				if (result is T) {
					return (T) result;
				} else {
					throw new ArgumentException("Invalid type.");
				}
			}

		}

		public static void Serialize(this XmlSerializer serializer, object o,string path) {
			using (var stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read)) {
				serializer.Serialize(stream, o);
			}
		}
	}
}
