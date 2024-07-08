using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using PMTool.Authorization;
using PMTool.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PMTool.Hubs
{
    [Authorize(Policy = "ActiveUserPolicy")]
    public class NotificationHub : Hub
    {
        public NotificationHub(IUserService _userService)
        {
            userService = _userService;
        }

        public static Dictionary<string, List<string>> Connections = new Dictionary<string, List<string>>();
        public static List<string> ConnectionIds = new List<string>();
        private readonly IUserService userService;

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public override Task OnConnectedAsync()
        {
            var userId = Context.User.Claims.FirstOrDefault(s => s.Type == ClaimTypes.NameIdentifier);
            if (userId != null)
            {

                if (Connections.ContainsKey(userId.Value))
                    Connections[userId.Value].Add(Context.ConnectionId);
                else
                    Connections[userId.Value] = new List<string>() { Context.ConnectionId };
            }
            return Task.CompletedTask;
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User.Claims.FirstOrDefault(s => s.Type == ClaimTypes.NameIdentifier);
            if (userId != null)
                Connections[userId.Value].Remove(Context.ConnectionId);
            return Task.CompletedTask;
        }
    }
}
