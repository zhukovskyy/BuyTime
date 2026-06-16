using BuyTime_Application.Dto;
using BuyTime_Application.User.Command.TelegramLogin;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BuyTime_Api.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController(ISender mediatr) : ApiController
{
    [HttpPost("telegram")]
    public async Task<IActionResult> TelegramLogin([FromBody] TelegramAuthRequest request)
    {
        var command = new TelegramLoginCommand(request.InitData);
        var result = await mediatr.Send(command);

        if (result.IsError)
        {
            return Problem(result.Errors);
        }

        return Ok(new
        {
            token = result.Value.Token,
            isExpert = result.Value.IsExpert
        });
    }
}