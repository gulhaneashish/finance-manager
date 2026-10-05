using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManager.API.Migrations
{
    /// <inheritdoc />
    public partial class FixTransactionType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Purpose",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Normal",
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Purpose",
                table: "Transactions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValue: "Normal");
        }
    }
}
