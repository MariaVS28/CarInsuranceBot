using CarInsuranceBot.BLL.Models;
using CarInsuranceBot.BLL.Services.Interfaces;
using QuestPDF.Fluent;

namespace CarInsuranceBot.BLL.Services
{
    public class PolicyGenerationService : IPolicyGenerationService
    {
        public byte[] GeneratePdf(ExtractedFields extractedFields)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);

                    page.Header().Text("Insurance Policy").FontSize(20).Bold().AlignCenter();
                    var issueDate = DateTime.UtcNow;
                    var price = "100";

                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Policy Number: {extractedFields.PolicyNumber}");
                        col.Item().Text($"Customer Name: {extractedFields.Surname } + {extractedFields.GivenNames}");
                        col.Item().Text($"Customer Passport Number: {extractedFields.PassportNumber}");
                        col.Item().Text($"Customer Date of Birth: {extractedFields.BirthDate}");
                        col.Item().Text($"The Vehicle's License Plate Number: {extractedFields.VehiclesLicensePlateNumber}");
                        col.Item().Text($"The Vehicle First Release Date: {extractedFields.VehicleFirstReleaseDate}");
                        col.Item().Text($"The Vehicle Owner's Full Name: {extractedFields.VehicleOwnersFullName}");
                        col.Item().Text($"The Vehicle's Brand: {extractedFields.VehiclesBrand}");
                        col.Item().Text($"The Vehicle Identification Number (VIN): {extractedFields.VIN}");
                        col.Item().Text($"Issue Date: {issueDate:yyyy-MM-dd}");
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
    }
}
