using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq; 

namespace BuyTime_Api.Controllers
{
    [Route("api/telegram")]
    [ApiController]
    public class TelegramWebhookController : ControllerBase
    {
        [HttpPost("webhook")]
        public IActionResult ReceiveUpdate([FromBody] object update)
        {
            try
            {
                var updateData = JObject.Parse(update.ToString());

                if (updateData["message"] != null)
                {
                    var chatId = updateData["message"]["chat"]["id"].ToString();
                    Console.WriteLine($"Received message from chat ID: {chatId}");
                }
                
                Console.WriteLine($"Received update: {updateData}");

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing update: {ex.Message}");
                return BadRequest();
            }
        }
    }
}