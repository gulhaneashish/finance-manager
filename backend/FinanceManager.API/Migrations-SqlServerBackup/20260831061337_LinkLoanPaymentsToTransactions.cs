using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManager.API.Migrations
{
    /// <inheritdoc />
    public partial class LinkLoanPaymentsToTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TransactionId",
                table: "LoanPayments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_LoanPayments_TransactionId",
                table: "LoanPayments",
                column: "TransactionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LoanPayments_Transactions_TransactionId",
                table: "LoanPayments",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoanPayments_Transactions_TransactionId",
                table: "LoanPayments");

            migrationBuilder.DropIndex(
                name: "IX_LoanPayments_TransactionId",
                table: "LoanPayments");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "LoanPayments");
        }
    }
}
