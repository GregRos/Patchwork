using Serilog;

namespace Patchwork.Engine.Utility {
	internal static class LogHelper {
		public static void Header(this ILogger logger, string headerTemplate, params object[] args) {
			logger.Information("=====" + headerTemplate + "=====", args);
		}
	}
}