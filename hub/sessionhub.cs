using System.Data;
using System.Threading.Tasks;
using goalongapi.Models;
using goalongapi.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace goalongapi.Hubs
{
    public class SessionHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var sid = Context.User?.FindFirst("sid")?.Value;
            if (!string.IsNullOrEmpty(sid))
                await Groups.AddToGroupAsync(Context.ConnectionId, $"session:{sid}");

            await base.OnConnectedAsync();



        }

        public async Task JoinSession(string sessionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"session:{sessionId}");

            await Clients.Caller.SendAsync("session_joined", new
            {
                sessionId,
                group = $"session:{sessionId}",
                connectionId = Context.ConnectionId
            });
        }
    }
}
