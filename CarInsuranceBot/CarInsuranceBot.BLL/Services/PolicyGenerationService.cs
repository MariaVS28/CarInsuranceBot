using CarInsuranceBot.BLL.Helpers;
using CarInsuranceBot.DAL.Models;
using CarInsuranceBot.DAL.Repositories;
using QuestPDF.Fluent;

namespace CarInsuranceBot.BLL.Services
{
    public class PolicyGenerationService(IAIChatService _aIChatService, 
        IErrorRepository _errorRepository, IDateTimeHelper _dateTimeHelper) : IPolicyGenerationService
    {
        public async Task<byte[]> GeneratePdfAsync(ExtractedFields extractedFields)
        {
            try
            {
                var issueDate = _dateTimeHelper.UtcNow();
                var price = "100";
                var expirationDate = issueDate.AddDays(7);
                var random = new Random();
                var policyNumber = random.Next(100000, 1000000).ToString();

                string prompt = @$"Generate a short insurance policy introduction paragraph addressed to the customer. Be friendly and professional.";
                var msg = await _aIChatService.GetChatCompletionAsync(prompt);
                var fullCustomerName = string.Concat(extractedFields.Surname, " ", extractedFields.GivenNames);
                var document = QuestPDF.Fluent.Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(50);

                        page.Header().Text("Insurance Policy").FontSize(20).Bold().AlignCenter();

                        page.Content().Column(col =>
                        {
                            col.Item().Text(msg);
                            col.Item().Text($"Policy Number: {policyNumber}");
                            col.Item().Text($"Customer Name: {fullCustomerName}");
                            col.Item().Text($"Customer Passport Number: {extractedFields.PassportNumber}");
                            col.Item().Text($"Customer Date of Birth: {extractedFields.BirthDate}");
                            col.Item().Text($"The Vehicle Identification Number: {extractedFields.VehicleIdentificationNumber}");
                            col.Item().Text($"The Vehicle's Registration Date: {extractedFields.VehiclesRegistrationDate}");
                            col.Item().Text($"The Vehicle Owner's Full Name: {extractedFields.VehicleOwnersFullName}");
                            col.Item().Text($"The Vehicle Make: {extractedFields.VehicleMake}");
                            col.Item().Text($"The Vehicle Model: {extractedFields.VehicleModel}");
                            col.Item().Text($"Issue Date: {issueDate:yyyy-MM-dd}");
                            col.Item().Text($"Expiration Date: {expirationDate:yyyy-MM-dd}");
                            col.Item().Text($"Total Price: ${price}");

                            col.Item().PaddingVertical(20);
                            col.Item().Text("Thank you for choosing our insurance services.")
                                      .Italic();
                        });

                        page.Footer().AlignCenter().Text(txt =>
                        {
                            txt.Span("Generated on ");
                            txt.Span($"{DateTime.Now:yyyy-MM-dd HH:mm}");
                        });
                    });
                });

                using var ms = new MemoryStream();
                document.GeneratePdf(ms);
                return ms.ToArray();
            }
            catch (Exception ex)
            {
                var error = new Error
                {
                    StackTrace = ex.StackTrace,
                    Message = ex.Message,
                    Date = _dateTimeHelper.UtcNow()
                };

                await _errorRepository.AddErrorAsync(error);
                throw ex;
            }
        }
    }
}
