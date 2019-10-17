using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;
using System.Windows.Interop;

namespace FeserWard.Controls
{
    public class PopupTopmostBehavior: Behavior<Popup>
	{
		public bool Topmost
		{
			get { return (bool)GetValue(TopmostProperty); }
			set { SetValue(TopmostProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Topmost.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty TopmostProperty =
			DependencyProperty.Register("Topmost", typeof(bool), typeof(PopupTopmostBehavior), new PropertyMetadata(false, OnTopmostChanged));

		private static void OnTopmostChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			(obj as PopupTopmostBehavior).UpdateWindow();
		}

		protected override void OnAttached()
		{
			Popup popup = AssociatedObject as Popup;
			if (popup == null)
			{
				throw new ArgumentException("PopupTopmostBehavior can only be used with a Popup.");
			}
			popup.Opened += Popup_Opened;
		}

		private void Popup_Opened(object sender, EventArgs e)
		{
			UpdateWindow();
		}

		protected override void OnDetaching()
		{
			Popup popup = AssociatedObject as Popup;
			if (popup != null)
			{
				popup.Opened -= Popup_Opened;
			}
		}

		private void UpdateWindow()
		{
			Popup pop = AssociatedObject as Popup;
			var hwnd = ((HwndSource)PresentationSource.FromVisual(pop.Child)).Handle;
			RECT rect;

			if (GetWindowRect(hwnd, out rect))
			{
				SetWindowPos(hwnd, Topmost ? -1 : -2, rect.Left, rect.Top, (int)pop.Width, (int)pop.Height, 0x0010);
			}
		}

		#region P/Invoke imports & definitions

		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

		[DllImport("user32", EntryPoint = "SetWindowPos")]
		private static extern int SetWindowPos(IntPtr hWnd, int hwndInsertAfter, int x, int y, int cx, int cy, int wFlags);

		#endregion
	}
}
