using CarInsuranceBot.BLL.Commands;
using CarInsuranceBot.BLL.Helpers;
using CarInsuranceBot.BLL.Services;
using CarInsuranceBot.DAL.Models;
using CarInsuranceBot.DAL.Repositories;
using Moq;
using Telegram.Bot;

namespace CarInsuranceBot.BLL.Tests.Services
{
    [TestFixture]
    public class FlowServiceTests
    {
        Mock<ITelegramBotClient> _botClient;
        Mock<ITelegramService> _telegramService;
        Mock<IAIChatService> _aIChatService;
        Mock<ITelegramFileLoaderService> _telegramFileLoaderService;
        Mock<IMindeeService> _mindeeService;
        Mock<IUserRepository> _userRepository;
        Mock<IAuditLogRepository> _auditLogRepository;
        Mock<IErrorRepository> _errorRepository;
        Mock<ICommandHandlerResolver> _handlerResolver;
        Mock<IDateTimeHelper> _dateTimeHelper;

        Mock<IProcessUnknown> _processUnknown;
        Mock<IProcessStart> _processStart;
        Mock<IProcessStatus> _processStatus;
        Mock<IProcessHelp> _processHelp;
        Mock<IProcessGiveAdmin> _processGiveAdmin;

        IFlowService _flowService;

        [SetUp]
        public void SetUp()
        {
            _botClient = new(MockBehavior.Strict);
            _telegramService = new(MockBehavior.Strict);
            _aIChatService = new(MockBehavior.Strict);
            _telegramFileLoaderService = new(MockBehavior.Strict);
            _mindeeService = new(MockBehavior.Strict);
            _userRepository = new(MockBehavior.Strict);
            _auditLogRepository = new(MockBehavior.Strict);
            _errorRepository = new(MockBehavior.Strict);
            _handlerResolver = new(MockBehavior.Strict);
            _dateTimeHelper = new(MockBehavior.Strict);

            _processUnknown = new(MockBehavior.Strict);
            _processStart = new(MockBehavior.Strict);
            _processStatus = new(MockBehavior.Strict);
            _processHelp = new(MockBehavior.Strict);
            _processGiveAdmin = new(MockBehavior.Strict);

            _flowService = new FlowService(_botClient.Object, _telegramService.Object, _aIChatService.Object, _telegramFileLoaderService.Object,
                _mindeeService.Object, _userRepository.Object, _auditLogRepository.Object, _errorRepository.Object,
                _handlerResolver.Object, _dateTimeHelper.Object);
        }

        private void VerifyAll()
        {
            _botClient.VerifyAll();
            _telegramService.VerifyAll();
            _aIChatService.VerifyAll();
            _telegramFileLoaderService.VerifyAll();
            _mindeeService.VerifyAll();
            _userRepository.VerifyAll();
            _auditLogRepository.VerifyAll();
            _errorRepository.VerifyAll();
            _handlerResolver.VerifyAll();
            _processUnknown.VerifyAll();
            _processStart.VerifyAll();
            _processStatus.VerifyAll();
            _processHelp.VerifyAll();
            _processGiveAdmin.VerifyAll();
        }

        [Test]
        public async Task ProcessTelegramCommandAsync_WhenUserNullInNonStartingFlow_ThenCallProcessUnknownCommand()
        {
            //Arrange
            long chatId = 1234;
            string text = "/yes";
            Telegram.Bot.Types.User telegramUser = new();

            User? user = null;
            _userRepository.Setup(x => x.GetUserAsync(chatId)).ReturnsAsync(user);

            _handlerResolver.Setup(x => x.Resolve<IProcessUnknown>()).Returns(_processUnknown.Object);
            _processUnknown.Setup(x => x.ProcessAsync(chatId)).Returns(Task.CompletedTask);

            //Act
            await _flowService.ProcessTelegramCommandAsync(chatId, text, telegramUser);

            //Assert
            VerifyAll();
        }

        [Test]
        public async Task ProcessTelegramCommandAsync_WhenUserNullInStartingFlow_ThenCallProcessUnknownCommand()
        {
            //Arrange
            long chatId = 1234;
            string text = "/start";
            Telegram.Bot.Types.User telegramUser = new();

            User? user = null;
            _userRepository.Setup(x => x.GetUserAsync(chatId)).ReturnsAsync(user);

            _handlerResolver.Setup(x => x.Resolve<IProcessStart>()).Returns(_processStart.Object);
            _processStart.Setup(x => x.ProcessAsync(chatId)).Returns(Task.CompletedTask);

            //Act
            await _flowService.ProcessTelegramCommandAsync(chatId, text, telegramUser);

            //Assert
            VerifyAll();
        }

        [Test]
        public async Task ProcessTelegramCommandAsync_WhenUserNotNullInStartingFlow_ThenCallProcessUnknownCommand()
        {
            //Arrange
            long chatId = 1234;
            string text = "/status";
            Telegram.Bot.Types.User telegramUser = new();

            User? user = new User();
            _userRepository.Setup(x => x.GetUserAsync(chatId)).ReturnsAsync(user);

            _handlerResolver.Setup(x => x.Resolve<IProcessStatus>()).Returns(_processStatus.Object);
            _processStatus.Setup(x => x.ProcessAsync(chatId, user)).Returns(Task.CompletedTask);

            //Act
            await _flowService.ProcessTelegramCommandAsync(chatId, text, telegramUser);

            //Assert
            VerifyAll();
        }

        [Test]
        public async Task ProcessTelegramCommandAsync_WhenHelpCommand_ThenCallProcessHelpCommand()
        {
            //Arrange
            long chatId = 1234;
            string text = "/help";
            Telegram.Bot.Types.User telegramUser = new();

            User? user = new User();
            _userRepository.Setup(x => x.GetUserAsync(chatId)).ReturnsAsync(user);

            _handlerResolver.Setup(x => x.Resolve<IProcessHelp>()).Returns(_processHelp.Object);
            _processHelp.Setup(x => x.ProcessAsync(chatId)).Returns(Task.CompletedTask);

            //Act
            await _flowService.ProcessTelegramCommandAsync(chatId, text, telegramUser);

            //Assert
            VerifyAll();
        }

        [Test]
        public async Task ProcessTelegramCommandAsync_WhenGiveAdminCommandAndTargetIdExists_ThenCallProcessGiveAdminCommand()
        {
            //Arrange
            long chatId = 1234;
            string text = "/giveadmin 45895625";
            long targetId = 45895625;
            Telegram.Bot.Types.User telegramUser = new();

            User? user = new User();
            _userRepository.Setup(x => x.GetUserAsync(chatId)).ReturnsAsync(user);

            _handlerResolver.Setup(x => x.Resolve<IProcessGiveAdmin>()).Returns(_processGiveAdmin.Object);
            _processGiveAdmin.Setup(x => x.ProcessAsync(chatId, user, targetId)).Returns(Task.CompletedTask);

            //Act
            await _flowService.ProcessTelegramCommandAsync(chatId, text, telegramUser);

            //Assert
            VerifyAll();
        }
    }
}
