using BuyTime_Application.Notifications.Command.MarkAsRead;
using BuyTime_Application.Notifications.Query.GetByUserId;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BuyTime_Api.Controllers;

[Route("api/notification")]
[ApiController]
public class NotificationController(ISender mediatr) : ApiController
{
    [HttpGet("get-by-user")]
    public async Task<IActionResult> GetUserNotifications([FromQuery] Guid userId)
    {
        var query = new GetNotificationsQuery(userId);
        var result = await mediatr.Send(query);

        if (result.IsError) return Problem(result.Errors);
        return Ok(result.Value);
    }

    [HttpPost("mark-as-read")]
    public async Task<IActionResult> MarkAsRead([FromQuery] Guid notificationId, [FromQuery] Guid userId)
    {
        var command = new MarkNotificationAsReadCommand(notificationId, userId);
        var result = await mediatr.Send(command);

        if (result.IsError) return Problem(result.Errors);
        return Ok();
    }
}