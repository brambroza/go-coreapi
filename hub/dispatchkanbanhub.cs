using Microsoft.AspNetCore.SignalR;

namespace goalongapi.Hubs
{
    public class DispatchKanbanHub : Hub
    {
        public async Task JoinBoard(string cmpId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"kanban-{cmpId}");
        }

        public async Task LeaveBoard(string cmpId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"kanban-{cmpId}");
        }

        public async Task BroadcastBoardChanged(string cmpId, string eventType)
        {
            await Clients.Group($"kanban-{cmpId}")
                .SendAsync("KanbanBoardChanged", new { eventType, cmpId, ts = DateTimeOffset.UtcNow });
        }
    }
}
