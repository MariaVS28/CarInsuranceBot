using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using CarInsuranceBot.BLL.Services;
using CarInsuranceBot.BLL.Services.Interfaces;
using Telegram.Bot;

namespace CarInsuranceBot.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TelegramController(IFlowService _flowService, IDuplicateRequestDetectorService _duplicateRequestDetectorService,
        ITelegramBotClient _botClient) : ControllerBase
    {
        [HttpPost("update")]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            

            if (update.Message != null)
            {
                var chatId = update.Message.Chat.Id;
                var text = update.Message.Text;
                string? fileId = null;

                if (text != null && _duplicateRequestDetectorService.IsDuplicate(chatId, text))
                {
                    var msg = "Duplicate message detected — skipping.";
                    await _botClient.SendMessage(chatId, msg);
                    return Ok();
                }

                if (update.Message.Photo != null)
                {
                    var largestPhoto = update.Message.Photo
                        .OrderByDescending(p => p.FileSize)
                        .First();

                    fileId = largestPhoto.FileId;
                }
                else if (update.Message.Document != null)
                {
                    fileId = update.Message.Document.FileId;
                }

                if (fileId != null)
                    await _flowService.ProcessTelegramFileAsync(chatId, fileId);
                else
                    await _flowService.ProcessTelegramCommandAsync(chatId, text, update.Message.From);
            }

            return Ok();
        }
    
    }
}
