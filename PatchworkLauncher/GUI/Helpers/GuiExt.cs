using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Windows.Forms;
using Patchwork.Utility;
using Patchwork.Utility.Binding;

namespace PatchworkLauncher {
	public static class GuiExt {
		public static void ShowOrFocus(this Form form) {
			if (form.Visible) {
				form.Focus();
			} else {
				form.Show();
			}
		}

		public static IList<Control> CastList(this Control.ControlCollection collection) {
			return collection.CastList<Control>();
		}

		public static IList<Control> CastList(this Form.ControlCollection collection) {
			return collection.CastList<Control>();
		} 
	}

	public static class GuiBindings {

		public static IBindable<TValue> Bind<TControl, TValue>(this TControl control, Expression<Func<TControl, TValue>> memberAccess, string refreshEvent = null)
			where TControl : Control
		{
			Action<Action> dispatcher = act => {
				if (control?.InvokeRequired == true) {
					control.Invoke(act);
				} else {
					act();
				}
			};
			var notification = refreshEvent == null ? null : new EventRaised(refreshEvent);
			return control.Bind(memberAccess, notification).WithDispatcher(dispatcher);
		}
	}
}