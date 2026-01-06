using BuyTime_Application.Wallet.Command.AddWallet;
using BuyTime_Application.Wallet.Command.RemoveWallet;
using BuyTime_Application.Wallet.Query.GetUserWallets;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BuyTime_Api.Controllers;

[Route("api/wallet")]
[ApiController]
public class WalletController(ISender mediatr) : ApiController
{
    [HttpGet("get-all-by-user")]
    public async Task<IActionResult> GetWallets([FromQuery] Guid userId)
    {
        var query = new GetUserWalletsQuery(userId);
        var result = await mediatr.Send(query);

        if (result.IsError) return Problem(result.Errors);
        return Ok(result.Value);
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddWallet([FromBody] AddWalletCommand command)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await mediatr.Send(command);

        if (result.IsError) return Problem(result.Errors);

        return Ok(result.Value);
    }

    [HttpDelete("remove")]
    public async Task<IActionResult> RemoveWallet([FromQuery] Guid userId, [FromQuery] Guid walletId)
    {
        var command = new RemoveWalletCommand(UserId: userId, WalletId: walletId);
        var result = await mediatr.Send(command);

        if (result.IsError) return Problem(result.Errors);

        return Ok("Wallet disconnected successfully.");
    }
}