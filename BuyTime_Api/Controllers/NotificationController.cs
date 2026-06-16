using BuyTime_Application.Notifications.Command.MarkAsRead;
using BuyTime_Application.Notifications.Query.GetByUserId;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuyTime_Api.Controllers;

[Route("api/notification")]
[Authorize]
[ApiController]
public class NotificationController(ISender mediatr) : ApiController
{
    [HttpGet("get-by-user")]
    public async Task<IActionResult> GetUserNotifications()
    {
        var query = new GetNotificationsQuery(CurrentUserId);
        var result = await mediatr.Send(query);

        if (result.IsError) return Problem(result.Errors);
        return Ok(result.Value);
    }

    [HttpPost("mark-as-read")]
    public async Task<IActionResult> MarkAsRead([FromQuery] Guid notificationId)
    {
        var command = new MarkNotificationAsReadCommand(notificationId, CurrentUserId);
        var result = await mediatr.Send(command);

        if (result.IsError) return Problem(result.Errors);
        return Ok();
    }
}