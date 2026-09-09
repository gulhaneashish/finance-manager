using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManager.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionIdToLoan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "Loans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TransactionId",
                table: "Loans",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Loans_TransactionId",
                table: "Loans",
                column: "TransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Transactions_TransactionId",
                table: "Loans",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Transactions_TransactionId",
                table: "Loans");

            migrationBuilder.DropIndex(
                name: "IX_Loans_TransactionId",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "Loans");
        }
    }
}
