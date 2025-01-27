using System.Data;
using System.Threading.Tasks;
using goalongapi.Models;
using goalongapi.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace goalongapi.Hubs
{
    public class TicketCommentHub : Hub
    {
        public async Task SendMessage(TicketCommentMessage mt)
        {
            await Clients.All.SendAsync($"ReceiveCommentTicket{mt.cmpId}{mt.ticketId}", mt);
        } 

        public async Task JoinTicketGroup(string ticketId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, ticketId);
        }

        public async Task LeaveTicketGroup(string ticketId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, ticketId);
        }
    }
}
