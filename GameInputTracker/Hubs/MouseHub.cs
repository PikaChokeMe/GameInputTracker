using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GameInputTracker.Hubs
{
    public class MouseHub : Hub
    {
        private static IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<KeyboardHub>();

        public void Hello()
        {
            Clients.All.hello();
        }

        public void HighlightKey(String keyName, bool isActive)
        {
            Clients.All.HighlightKey(keyName, isActive);
        }

        public void Heartbeat()
        {
            Clients.All.Heartbeat();
        }
    }
}