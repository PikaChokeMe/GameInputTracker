using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InputTracker.Hubs
{
    public class KeyboardHub : Hub
    {
        public async Task SendKeyState(Keys keyName, bool isActive)
        {
            await Clients.All.SendAsync("RecieveKeyState", keyName.ToString(), isActive);
        }
    }
}
