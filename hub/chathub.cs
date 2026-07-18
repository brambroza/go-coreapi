using System.Data;
using System.Threading.Tasks;
using goalongapi.Models;
using goalongapi.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace goalongapi.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessageChat", message);
        }

    }
}
