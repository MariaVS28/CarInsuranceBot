using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarInsuranceBot.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddedAdminFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDocumentDataMocked",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "FaildStep",
                table: "Errors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDocumentDataMocked",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FaildStep",
                table: "Errors");
        }
    }
}
