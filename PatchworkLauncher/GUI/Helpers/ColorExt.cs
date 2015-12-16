using System;
using System.Drawing;

namespace PatchworkLauncher {
	public static class ColorExt {
		public static Color Transform(this Color color, Func<ColorChannel, byte, byte> selector) {
			return Color.FromArgb(selector(ColorChannel.A, color.A), selector(ColorChannel.R, color.R),
				selector(ColorChannel.G, color.G),
				selector(ColorChannel.B, color.B));
		}

		public static Color Lighten(this Color color, int increase) {
			return color.Transform((chan, val) => chan == ColorChannel.A ? val : (byte)Math.Min(255, val + increase));
		}
	}

}