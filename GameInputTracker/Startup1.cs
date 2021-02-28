using System;
using System.Runtime.InteropServices;
using System.Threading;
using GameInputTracker.Hubs;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

[assembly: OwinStartup(typeof(GameInputTracker.Startup1))]

namespace GameInputTracker
{
    public class Startup1
    {
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(System.Int32 vKey);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        public static Point GetCursorPosition()
        {
            POINT lpPoint;
            GetCursorPos(out lpPoint);
            // NOTE: If you need error handling
            // bool success = GetCursorPos(out lpPoint);
            // if (!success)

            return lpPoint;
        }

        private enum KeyboardLayout {
            QWERTY,
            DVORAK
        }

        KeyboardLayout trackedLayout = KeyboardLayout.DVORAK;

        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888

            app.MapSignalR();

            IHubContext keyboardHub = GlobalHost.ConnectionManager.GetHubContext<KeyboardHub>();
            IHubContext mouseHub = GlobalHost.ConnectionManager.GetHubContext<MouseHub>();
            Thread inputThread = new Thread(() => processInput(keyboardHub, mouseHub));
            inputThread.Start();
        }

        public void processInput(IHubContext keyboardHub, IHubContext mouseHub)
        {
            Dictionary<string, short> keyMap = new Dictionary<string, short>();
            Point previousCursorPosition = GetCursorPosition();

            while (true)
            {
                keyboardHub.Clients.All.Heartbeat();
                mouseHub.Clients.All.Heartbeat();

                foreach (System.Int32 keyCode in Enum.GetValues(typeof(Keys)))
                {
                    var keyState = GetAsyncKeyState(keyCode);
                    var keyName = getReferenceKeyName(keyCode);

                    if (keyMap.ContainsKey(keyName) && keyState != keyMap[keyName])
                        keyboardHub.Clients.All.HighlightKey(keyName, keyState != 0);

                    if (keyMap.ContainsKey(keyName) && keyState != keyMap[keyName])
                        mouseHub.Clients.All.HighlightKey(keyName, keyState != 0);

                    keyMap[keyName] = keyState;
                }

                //if (previousCursorPosition != Cursor.Position)
                //{

                //    Point currentCursorPosition = GetCursorPosition();

                //    double deltaX = currentCursorPosition.X - previousCursorPosition.X;
                //    double deltaY = currentCursorPosition.Y - previousCursorPosition.Y;
                //    double radians = Math.Atan2(deltaY, deltaX);
                //    double degrees = radians * 180 / Math.PI;
                //    double facingAngle = (degrees - 360.0 * Math.Floor(degrees / 360.0));

                //    previousCursorPosition = currentCursorPosition;

                //    mouseHub.Clients.All.setAngle(facingAngle);
                //}
                
                

                Thread.Sleep(50);
            }
        }

        public string getReferenceKeyName(int keyCode)
        {
            var keyName = Enum.GetName(typeof(Keys), keyCode);

            if (trackedLayout == KeyboardLayout.DVORAK) 
            {
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
            }

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

            if (trackedLayout == KeyboardLayout.QWERTY)
            {
                return $"Key{keyName}";
            } 
            else
            {
                return keyName;
            }
        }
    }
}
