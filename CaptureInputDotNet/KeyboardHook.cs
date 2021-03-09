using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace CaptureInputDotNet
{
    public enum KeyboardEvents
    {
        KeyDown       = 0x0100,
        KeyUp         = 0x0101,
        SystemKeyDown = 0x0104,
        SystemKeyUp   = 0x0105
    }

	public enum KeyboardLayout
    {
		US,
		DVORAK
    }

    public class KeyboardHook : SystemHook
    {
        public delegate void KeyboardEventHandler(KeyboardEvents kEvent, Keys key);
		public event KeyboardEventHandler KeyboardEvent;

		KeyboardLayout _keyboardLayout;

		public KeyboardLayout keyboardLayout
		{
			get
            {
				return _keyboardLayout;
            }
			set
			{
				_keyboardLayout = value;
				generateDictionary();
			}
		}

		private Dictionary<VirtualKeys, Keys> keyMap;

        public KeyboardHook() : base(HookTypes.KeyboardLL) 
		{
			generateDictionary();
		}

		public KeyboardHook(KeyboardLayout keyboardLayout) : base(HookTypes.KeyboardLL)
		{
			this.keyboardLayout = keyboardLayout; 
		}

		protected void generateDictionary()
        {
			keyMap = new Dictionary<VirtualKeys, Keys>();

			keyMap.Add(VirtualKeys.LWindows, Keys.LWin);
			keyMap.Add(VirtualKeys.ShiftLeft, Keys.Shift);
			keyMap.Add(VirtualKeys.ControlLeft, Keys.Control);
			keyMap.Add(VirtualKeys.AltLeft, Keys.Menu);
			keyMap.Add(VirtualKeys.Menu, Keys.Menu);
			keyMap.Add(VirtualKeys.Tab, Keys.Tab);
			keyMap.Add(VirtualKeys.Capital, Keys.Capital);
			keyMap.Add(VirtualKeys.Escape, Keys.Escape);
			keyMap.Add(VirtualKeys.Space, Keys.Space);
			keyMap.Add(VirtualKeys.D0, Keys.D0);
			keyMap.Add(VirtualKeys.D1, Keys.D1);
			keyMap.Add(VirtualKeys.D2, Keys.D2);
			keyMap.Add(VirtualKeys.D3, Keys.D3);
			keyMap.Add(VirtualKeys.D4, Keys.D4);
			keyMap.Add(VirtualKeys.D5, Keys.D5);

			if (_keyboardLayout == KeyboardLayout.US)
            {
				keyMap.Add(VirtualKeys.Q, Keys.Q);
				keyMap.Add(VirtualKeys.W, Keys.W); 
				keyMap.Add(VirtualKeys.E, Keys.E);
				keyMap.Add(VirtualKeys.R, Keys.R);
				keyMap.Add(VirtualKeys.T, Keys.T);

				keyMap.Add(VirtualKeys.A, Keys.A);
				keyMap.Add(VirtualKeys.S, Keys.S);
				keyMap.Add(VirtualKeys.D, Keys.D);
				keyMap.Add(VirtualKeys.F, Keys.F);
				keyMap.Add(VirtualKeys.G, Keys.G);

				keyMap.Add(VirtualKeys.Z, Keys.Z);
				keyMap.Add(VirtualKeys.X, Keys.X);
				keyMap.Add(VirtualKeys.C, Keys.C);
				keyMap.Add(VirtualKeys.V, Keys.V);
			} 
			else if (keyboardLayout == KeyboardLayout.DVORAK)
            {
				keyMap.Add(VirtualKeys.OEMQuote, Keys.Q);
				keyMap.Add(VirtualKeys.OEMComma, Keys.W);
				keyMap.Add(VirtualKeys.OEMPeriod, Keys.E);
				keyMap.Add(VirtualKeys.P, Keys.R);
				keyMap.Add(VirtualKeys.Y, Keys.T);

				keyMap.Add(VirtualKeys.A, Keys.A);
				keyMap.Add(VirtualKeys.O, Keys.S);
				keyMap.Add(VirtualKeys.E, Keys.D);
				keyMap.Add(VirtualKeys.U, Keys.F);
				keyMap.Add(VirtualKeys.I, Keys.G);

				keyMap.Add(VirtualKeys.OEMColon, Keys.Z);
				keyMap.Add(VirtualKeys.Q, Keys.X);
				keyMap.Add(VirtualKeys.J, Keys.C);
				keyMap.Add(VirtualKeys.K, Keys.V);
			}
		}

		protected override void HookCallback(int code, UIntPtr wparam, IntPtr lparam)
		{
			if (KeyboardEvent == null)
			{
				return;
			}

			int vkCode = 0;
			KeyboardEvents kEvent = (KeyboardEvents)wparam.ToUInt32();

			if (kEvent != KeyboardEvents.KeyDown &&
				kEvent != KeyboardEvents.KeyUp &&
				kEvent != KeyboardEvents.SystemKeyDown &&
				kEvent != KeyboardEvents.SystemKeyUp)
			{
				return;
			}

			GetKeyboardReading(wparam, lparam, ref vkCode);
			VirtualKeys vk = (VirtualKeys)vkCode;

			Keys key;

			if (keyMap.TryGetValue(vk, out key))
            {
				KeyboardEvent(kEvent, key);
			}
		}

		public void FilterMessage(KeyboardEvents eventType)
		{
			base.FilterMessage(this.HookType, (int)eventType);
		}

	}
}
