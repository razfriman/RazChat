// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;
using System.CodeDom.Compiler;

namespace RazChatMacClient
{
	[Register ("MainWindowController")]
	partial class MainWindowController
	{
		[Outlet]
		MonoMac.AppKit.NSTextField inputText { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextView textView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (textView != null) {
				textView.Dispose ();
				textView = null;
			}

			if (inputText != null) {
				inputText.Dispose ();
				inputText = null;
			}
		}
	}

	[Register ("MainWindow")]
	partial class MainWindow
	{
		[Outlet]
		MonoMac.AppKit.NSTextField inputText { get; set; }

		[Outlet]
		MonoMac.AppKit.NSClipView textView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (inputText != null) {
				inputText.Dispose ();
				inputText = null;
			}

			if (textView != null) {
				textView.Dispose ();
				textView = null;
			}
		}
	}
}
