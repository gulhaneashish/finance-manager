using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManager.API.Migrations
{
    /// <inheritdoc />
    public partial class LinkInvestmentTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TransactionId",
                table: "Investments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Investments_TransactionId",
                table: "Investments",
                column: "TransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Investments_Transactions_TransactionId",
                table: "Investments",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Investments_Transactions_TransactionId",
                table: "Investments");

            migrationBuilder.DropIndex(
                name: "IX_Investments_TransactionId",
                table: "Investments");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "Investments");
        }
    }
}
