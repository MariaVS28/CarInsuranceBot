using CarInsuranceBot.BLL.Commands;
using CarInsuranceBot.BLL.Helpers;
using CarInsuranceBot.BLL.Services;
using CarInsuranceBot.DAL.Models;
using CarInsuranceBot.DAL.Repositories;
using Moq;

namespace CarInsuranceBot.BLL.Tests.Commands
{
    [TestFixture]
    public class ProcessGiveAdminTests
    {
        Mock<ITelegramService> _telegramService;
        Mock<IUserRepository> _userRepository;
        Mock<IAuditLogRepository> _auditLogRepository;
        Mock<IProcessUnknown> _processUnknown;
        Mock<IDateTimeHelper> _dateTimeHelper;

        IProcessGiveAdmin _processGiveAdmin;

        [SetUp]
        public void SetUp()
        {
            _telegramService = new(MockBehavior.Strict);
            _userRepository = new(MockBehavior.Strict);
            _auditLogRepository = new(MockBehavior.Strict);
            _processUnknown = new(MockBehavior.Strict);
            _dateTimeHelper = new(MockBehavior.Strict);

            _processGiveAdmin = new ProcessGiveAdmin(_telegramService.Object, _userRepository.Object, _auditLogRepository.Object, 
                _processUnknown.Object, _dateTimeHelper.Object);
        }

        private void VerifyAll()
        {
            _telegramService.VerifyAll();
            _userRepository.VerifyAll();
            _auditLogRepository.VerifyAll();
            _processUnknown.VerifyAll();
        }

        [Test]
        public async Task ProcessAsync_WhenUserIsNotAdmin_ThenProcessUnknownCommand()
        {
            //Arrange
            long chatId = 1234;
            long targetId = 45895625;
            User? user = new User();

            _processUnknown.Setup(x => x.ProcessAsync(chatId)).Returns(Task.CompletedTask);

            //Act
            await _processGiveAdmin.ProcessAsync(chatId, user, targetId);

            //Assert
            VerifyAll();
        }

        [Test]
        public async Task ProcessAsync_WhenUserIsAdminAndUserIdNotExist_ThenSendMessageUserNotExist()
        {
            //Arrange
            long chatId = 1234;
            long targetId = 45895625;
            bool isUerIdExist = false;
            var message = $"The user {targetId} doesn't exist.";
            User? user = new User
            {
                IsAdmin = true
            };

            _userRepository.Setup(x => x.IsUserIdExistAsync(targetId)).ReturnsAsync(isUerIdExist);
            _telegramService.Setup(x => x.SendMessage(chatId, message)).ReturnsAsync(new Telegram.Bot.Types.Message());

            //Act
            await _processGiveAdmin.ProcessAsync(chatId, user, targetId);

            //Assert
            VerifyAll();
        }

        [Test]
        public async Task ProcessAsync_WhenUserIsAdminAndUserIdExists_ThenCallProcessAsync()
        {
            //Arrange
            long chatId = 1234;
            long targetId = 45895625;
            bool isUerIdExist = true;
            var message = $"Access granted successfully to {targetId} user!";
            var logMessage = $"The User {targetId} got access to admin right.";
            var isAdmin = true;
            User? user = new User
            {
                IsAdmin = isAdmin
            };

            var utcNow = DateTime.UtcNow;

            var auditLog = new AuditLog
            { 
                Message = logMessage,
                Date = utcNow
            };

            _userRepository.Setup(x => x.IsUserIdExistAsync(targetId)).ReturnsAsync(isUerIdExist);
            _userRepository.Setup(x => x.SetAdminAsync(targetId, isAdmin)).Returns(Task.CompletedTask);
            _telegramService.Setup(x => x.SendMessage(chatId, message)).ReturnsAsync(new Telegram.Bot.Types.Message());
            _dateTimeHelper.Setup(x => x.UtcNow()).Returns(utcNow);
            _auditLogRepository.Setup(x => x.AddAuditLogAsync(It.IsAny<AuditLog>())).Callback<AuditLog>(x =>
            {
                Assert.That(x.Message, Is.EqualTo(logMessage));
                Assert.That(x.Date, Is.EqualTo(utcNow));
            }).Returns(Task.CompletedTask);

            //Act
            await _processGiveAdmin.ProcessAsync(chatId, user, targetId);

            //Assert
            VerifyAll();
        }
    }
}
