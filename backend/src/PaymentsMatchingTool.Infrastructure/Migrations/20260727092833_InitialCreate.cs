using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentsMatchingTool.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MatchBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SystemFileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    ProviderFileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    TotalCount = table.Column<int>(type: "integer", nullable: false),
                    MatchedCount = table.Column<int>(type: "integer", nullable: false),
                    OnlySystemCount = table.Column<int>(type: "integer", nullable: false),
                    OnlyProviderCount = table.Column<int>(type: "integer", nullable: false),
                    AmountMismatchCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchBatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    SystemAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ProviderAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Resolved = table.Column<bool>(type: "boolean", nullable: false),
                    ResolutionSide = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ResolvedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentMatches_MatchBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "MatchBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMatches_BatchId_OrderId_Currency",
                table: "PaymentMatches",
                columns: new[] { "BatchId", "OrderId", "Currency" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentMatches");

            migrationBuilder.DropTable(
                name: "MatchBatches");
        }
    }
}
