using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace HospitalManagement.Presentation.Hubs;

[Authorize]
public class AppointmentHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        // Add to general updates group or specific department groups based on role
        if (Context.User?.IsInRole("Receptionist") == true || Context.User?.IsInRole("Admin") == true)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "ReceptionStaff");
        }
        await base.OnConnectedAsync();
    }
}
