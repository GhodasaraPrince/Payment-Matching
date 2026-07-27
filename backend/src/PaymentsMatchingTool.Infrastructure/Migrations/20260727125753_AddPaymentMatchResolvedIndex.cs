using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentsMatchingTool.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentMatchResolvedIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PaymentMatches_BatchId_Resolved",
                table: "PaymentMatches",
                columns: new[] { "BatchId", "Resolved" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentMatches_BatchId_Resolved",
                table: "PaymentMatches");
        }
    }
}
