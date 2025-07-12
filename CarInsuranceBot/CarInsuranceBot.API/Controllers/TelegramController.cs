using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Telegram.Bot;
using CarInsuranceBot.BLL.Services;

namespace CarInsuranceBot.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TelegramController(IFlowService _flowService) : ControllerBase
    {
        [HttpPost("update")]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            if (update.Message != null)
            {
                var chatId = update.Message.Chat.Id;
                var text = update.Message.Text;

                await _flowService.ProcessTelegramCommand(chatId, text);
            }

            return Ok();
        }
    
    }
}
