using CarInsuranceBot.DAL.Models;

namespace CarInsuranceBot.BLL.Commands
{
    public interface IProcessReady
    {
        Task ProcessAsync(long chatId, User? user, Telegram.Bot.Types.User telegramUser);
    }
}
