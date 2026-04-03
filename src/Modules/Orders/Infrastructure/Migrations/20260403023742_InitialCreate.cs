using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Orders.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "orders");

            migrationBuilder.CreateTable(
                name: "orders",
                schema: "orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    address_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "pending"),
                    total_amount = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    currency_code = table.Column<string>(type: "text", nullable: false, defaultValue: "EUR"),
                    total_in_base = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    base_currency = table.Column<string>(type: "text", nullable: false, defaultValue: "EUR"),
                    fx_rate_snapshot = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    ordered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                schema: "orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    method = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "pending"),
                    amount = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    transaction_ref = table.Column<string>(type: "text", nullable: true),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    currency_code = table.Column<string>(type: "text", nullable: true),
                    amount_in_base = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    fx_rate = table.Column<decimal>(type: "numeric(18,8)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "order_items",
                schema: "orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    variant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_name = table.Column<string>(type: "text", nullable: false),
                    product_sku = table.Column<string>(type: "text", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(12,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_items_orders_order_id",
                        column: x => x.order_id,
                        principalSchema: "orders",
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_order_items_order_id",
                schema: "orders",
                table: "order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_order_id",
                schema: "orders",
                table: "payments",
                column: "order_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "order_items",
                schema: "orders");

            migrationBuilder.DropTable(
                name: "payments",
                schema: "orders");

            migrationBuilder.DropTable(
                name: "orders",
                schema: "orders");
        }
    }
}
