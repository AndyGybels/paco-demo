using Microsoft.AspNetCore.SignalR;

namespace PacoDemo.Api.Hubs;

public class DocumentHub : Hub
{
    // Clients subscribe and receive "DocumentStatusChanged" pushes.
    // No server-callable methods needed — the hub is receive-only for clients.
}
