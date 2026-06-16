using BuyTime_Application.Settings.Command.UpdateUserSettings;
using BuyTime_Application.Settings.Query.GetUserSettings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BuyTime_Api.Controllers;

[Route("api/settings")]
[Authorize]
[ApiController]
public class SettingsController(ISender mediatr) : ApiController
{
    public record UpdateSettingsRequest(string Theme, string Language, bool ShowCurrencyEquivalent, 
                                        string Currency, bool NotifyInTelegram, bool NotifyOnBooking, 
                                        bool NotifyOnFinance, bool NotifyReminders, bool NotifyOnNewFeedback);

    [HttpGet("get")]
    public async Task<IActionResult> GetSettings()
    {
        var query = new GetUserSettingsQuery(CurrentUserId);
        var result = await mediatr.Send(query);

        if (result.IsError) return Problem(result.Errors);
        return Ok(result.Value);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateSettingsRequest request)
    {
        var command = new UpdateUserSettingsCommand(
            CurrentUserId, request.Theme, request.Language, request.ShowCurrencyEquivalent,
            request.Currency, request.NotifyInTelegram, request.NotifyOnBooking,
            request.NotifyOnFinance, request.NotifyReminders, request.NotifyOnNewFeedback);

        var result = await mediatr.Send(command);

        if (result.IsError) return Problem(result.Errors);
        return Ok(result.Value);
    }
}