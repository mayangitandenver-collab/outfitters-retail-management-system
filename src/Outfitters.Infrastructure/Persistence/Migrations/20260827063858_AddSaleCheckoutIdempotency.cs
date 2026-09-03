using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Outfitters.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSaleCheckoutIdempotency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sales_StoreId",
                table: "Sales");

            migrationBuilder.AddColumn<Guid>(
                name: "CheckoutId",
                table: "Sales",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_StoreId_CheckoutId",
                table: "Sales",
                columns: new[] { "StoreId", "CheckoutId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sales_StoreId_CheckoutId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "CheckoutId",
                table: "Sales");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_StoreId",
                table: "Sales",
                column: "StoreId");
        }
    }
}
