using BuyTime_Application.Wallet.Command.AddWallet;
using BuyTime_Application.Wallet.Command.RemoveWallet;
using BuyTime_Application.Wallet.Query.GetUserWallets;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuyTime_Api.Controllers;

[Route("api/wallet")]
[Authorize]
[ApiController]
public class WalletController(ISender mediatr) : ApiController
{
    public record AddWalletRequest(string Network, string Address);

    [HttpGet("get-all-by-user")]
    public async Task<IActionResult> GetWallets()
    {
        var query = new GetUserWalletsQuery(CurrentUserId);
        var result = await mediatr.Send(query);

        if (result.IsError) return Problem(result.Errors);
        return Ok(result.Value);
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddWallet([FromBody] AddWalletRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var command = new AddWalletCommand(CurrentUserId, request.Network, request.Address);
        var result = await mediatr.Send(command);

        if (result.IsError) return Problem(result.Errors);
        return Ok(result.Value);
    }

    [HttpDelete("remove")]
    public async Task<IActionResult> RemoveWallet([FromQuery] Guid walletId)
    {
        var command = new RemoveWalletCommand(CurrentUserId, walletId);
        var result = await mediatr.Send(command);

        if (result.IsError) return Problem(result.Errors);
        return Ok(new { message = "Wallet disconnected successfully." });
    }
}