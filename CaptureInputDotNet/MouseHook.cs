using System;

namespace CaptureInputDotNet
{
	public enum MouseEvents
	{
		LeftButtonDown = 0x0201,
		LeftButtonUp = 0x0202,
		Move = 0x0200,
		MouseWheel = 0x020A,
		RightButtonDown = 0x0204,
		RightButtonUp = 0x0205
	}

	public class MouseHook : SystemHook
    {
		public delegate void MouseButtonEventHandler(MouseEvents mouseEvents);
		public event MouseButtonEventHandler MouseLeftDownEvent;
		public event MouseButtonEventHandler MouseLeftUpEvent;
		public event MouseButtonEventHandler MouseRightDownEvent;
		public event MouseButtonEventHandler MouseRightUpEvent;

		public delegate void MouseScrollEventHandler(MouseEvents mouseEvent, int delta);
		public event MouseScrollEventHandler MouseScrollEvent;

		public MouseHook(): base(HookTypes.MouseLL)
		{
		}

		protected override void HookCallback(int code, UIntPtr wparam, IntPtr lparam)
		{
			MouseEvents mEvent = (MouseEvents)wparam.ToUInt32();

			switch (mEvent)
			{
				case MouseEvents.LeftButtonDown:
					MouseLeftDownEvent(mEvent);
					break;
				case MouseEvents.LeftButtonUp:
					MouseLeftUpEvent(mEvent);
					break;
				case MouseEvents.MouseWheel:
					int delta = 0;

					GetScrollState(wparam, lparam, ref delta);
					MouseScrollEvent(mEvent, delta);

					break;
				case MouseEvents.Move:
					//GetMousePosition(wparam, lparam, ref x, ref y);
					break;
				case MouseEvents.RightButtonDown:
					MouseRightDownEvent(mEvent);
					break;
				case MouseEvents.RightButtonUp:
					MouseRightUpEvent(mEvent);
					break;
				default:
					break;
			}

			
		}

		public void FilterMessage(MouseEvents eventType)
		{
			base.FilterMessage(this.HookType, (int)eventType);
		}
	}
}
