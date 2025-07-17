using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarInsuranceBot.DAL.Migrations
{
    /// <inheritdoc />
    public partial class addedFileUploadAttempt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileUploadAttempts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PassportAttemps = table.Column<int>(type: "int", nullable: false),
                    VRCAttemps = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileUploadAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileUploadAttempts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileUploadAttempts_UserId",
                table: "FileUploadAttempts",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileUploadAttempts");
        }
    }
}
