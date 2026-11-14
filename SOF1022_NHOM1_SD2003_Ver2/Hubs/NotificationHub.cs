using Microsoft.AspNetCore.SignalR;

namespace SOF1022_NHOM1_SD2003_Ver2.Hubs
{
    public class NotificationHub : Hub
    {
        // Join a user-specific notification group based on their user ID
        public async Task JoinUserGroup(int userId)
        {
            var groupName = $"user-{userId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        // Leave a user-specific notification group
        public async Task LeaveUserGroup(int userId)
        {
            var groupName = $"user-{userId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        // Join admin group for receiving all admin notifications
        public async Task JoinAdminGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
        }

        // Leave admin group
        public async Task LeaveAdminGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "admins");
        }

        // Join librarian group for receiving all librarian notifications
        public async Task JoinLibrarianGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "librarians");
        }

        // Leave librarian group
        public async Task LeaveLibrarianGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "librarians");
        }

        // Join staff group (both admins and librarians)
        public async Task JoinStaffGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "staff");
        }

        // Leave staff group
        public async Task LeaveStaffGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "staff");
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
