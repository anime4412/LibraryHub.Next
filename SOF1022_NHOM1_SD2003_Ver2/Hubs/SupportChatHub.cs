using Microsoft.AspNetCore.SignalR;
using SOF1022_NHOM1_SD2003_Ver2.Datas;
using SOF1022_NHOM1_SD2003_Ver2.Models;
using SOF1022_NHOM1_SD2003_Ver2.Models.Enums;

namespace SOF1022_NHOM1_SD2003_Ver2.Hubs
{
    public class SupportChatHub : Hub
    {
        private readonly LibraryDbContext _context;
        private static System.Collections.Concurrent.ConcurrentDictionary<string, int?> _onlineEmployees = new();

        public SupportChatHub(LibraryDbContext context)
        {
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext != null)
            {
                var role = httpContext.Session.GetString("Role");
                var userIdString = httpContext.Session.GetString("UserId");

                // Track online employees
                if (role == "Admin" || role == "Librarian")
                {
                    if (int.TryParse(userIdString, out int employeeId))
                    {
                        _onlineEmployees[Context.ConnectionId] = employeeId;
                    }
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _onlineEmployees.TryRemove(Context.ConnectionId, out _);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinTicketGroup(int ticketId)
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext != null)
            {
                var userIdString = httpContext.Session.GetString("UserId");
                var role = httpContext.Session.GetString("Role");

                // Validate access to this ticket
                var ticket = await _context.SupportTickets.FindAsync(ticketId);
                if (ticket == null)
                {
                    throw new HubException("Ticket not found");
                }

                if (role == "Reader")
                {
                    if (int.TryParse(userIdString, out int readerId) && ticket.ReaderId != readerId)
                    {
                        throw new HubException("Access denied");
                    }
                }
                // Admin/Librarian can join any ticket

                await Groups.AddToGroupAsync(Context.ConnectionId, $"ticket-{ticketId}");
            }
        }

        public async Task LeaveTicketGroup(int ticketId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"ticket-{ticketId}");
        }

        public async Task JoinStaffQueue()
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext != null)
            {
                var role = httpContext.Session.GetString("Role");
                if (role != "Admin" && role != "Librarian")
                {
                    throw new HubException("Access denied");
                }

                await Groups.AddToGroupAsync(Context.ConnectionId, "staff-queue");
            }
        }

        public async Task LeaveStaffQueue()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "staff-queue");
        }

        public async Task BroadcastNewTicket(int ticketId)
        {
            await Clients.Group("staff-queue").SendAsync("NewTicketCreated", ticketId);
        }

        public async Task BroadcastTicketClaimed(int ticketId, int employeeId, string employeeName)
        {
            // Notify staff queue
            await Clients.Group("staff-queue").SendAsync("TicketClaimed", ticketId, employeeId, employeeName);
            
            // Notify ticket group
            await Clients.Group($"ticket-{ticketId}").SendAsync("TicketClaimed", ticketId, employeeId, employeeName);
        }

        public async Task BroadcastNewMessage(int ticketId, object messageData)
        {
            await Clients.Group($"ticket-{ticketId}").SendAsync("NewMessage", ticketId, messageData);
        }

        public async Task BroadcastTicketClosed(int ticketId)
        {
            await Clients.Group($"ticket-{ticketId}").SendAsync("TicketClosed", ticketId);
            await Clients.Group("staff-queue").SendAsync("TicketClosed", ticketId);
        }

        public static int GetOnlineEmployeeCount()
        {
            return _onlineEmployees.Count;
        }
    }
}
