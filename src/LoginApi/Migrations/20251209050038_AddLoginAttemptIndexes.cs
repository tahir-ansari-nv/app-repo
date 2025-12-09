using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoginApi.Migrations
{
    /// <inheritdoc />
    public partial class AddLoginAttemptIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "IPAddress",
                table: "LoginAttempts",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "EmailAttempted",
                table: "LoginAttempts",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_LoginAttempt_Email_IP_Timestamp_Success",
                table: "LoginAttempts",
                columns: new[] { "EmailAttempted", "IPAddress", "Timestamp", "Success" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LoginAttempt_Email_IP_Timestamp_Success",
                table: "LoginAttempts");

            migrationBuilder.AlterColumn<string>(
                name: "IPAddress",
                table: "LoginAttempts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "EmailAttempted",
                table: "LoginAttempts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
