using System;
using System.Runtime.InteropServices;
using System.Threading;
using GameInputTracker.Hubs;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Globalization;

[assembly: OwinStartup(typeof(GameInputTracker.Startup1))]

namespace GameInputTracker
{
    public class Startup1
    {
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(System.Int32 vKey);

        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888

            app.MapSignalR();

            IHubContext keyboardHub = GlobalHost.ConnectionManager.GetHubContext<KeyboardHub>();
            Thread keyThread = new Thread(() => processKeys(keyboardHub));
            keyThread.Start();
        }

        public void processKeys(IHubContext keyboardHub)
        {
            Dictionary<string, short> keyMap = new Dictionary<string, short>();

            while (true)
            {
                keyboardHub.Clients.All.Heartbeat();

                foreach (System.Int32 keyCode in Enum.GetValues(typeof(Keys)))
                {
                    var keyState = GetAsyncKeyState(keyCode);
                    var keyName = getReferenceKeyName(keyCode);

                    if (keyMap.ContainsKey(keyName) && keyState != keyMap[keyName])
                        keyboardHub.Clients.All.HighlightKey(keyName, keyState != 0);

                    keyMap[keyName] = keyState;
                }

                Thread.Sleep(25);
            }
        }

        public string getReferenceKeyName(int keyCode)
        {
            var keyName = Enum.GetName(typeof(Keys), keyCode);

            switch (InputLanguage.CurrentInputLanguage.LayoutName)
            {
                case "United States-Dvorak":
                    switch (keyName)
                    {
                        case "OemQuotes":
                            return "KeyQ";
                        case "Oemcomma":
                            return "KeyW";
                        case "OemPeriod":
                            return "KeyE";
                        case "P":
                            return "KeyR";
                        case "A":
                            return "KeyA";
                        case "O":
                            return "KeyS";
                        case "E":
                            return "KeyD";
                        case "U":
                            return "KeyF";
                        case "OemSemicolon":
                            return "KeyZ";
                        case "Q":
                            return "KeyX";
                        case "J":
                            return "KeyC";
                    }
                    goto default;

                default:
                    switch(keyName)
                    {
                        case "LShiftKey":
                            return "ShiftLeft";
                        case "LControlKey":
                            return "ControlLeft";
                        case "LMenu":
                            return "AltLeft";
                        case "LWin":
                            return "MetaLeft";
                    }
                    break;
            }

            return keyName;
        }
    }
}
