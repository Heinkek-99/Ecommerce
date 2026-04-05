using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Promotions.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "promotions");

            migrationBuilder.CreateTable(
                name: "campaigns",
                schema: "promotions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false, defaultValue: "seasonal"),
                    starts_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ends_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_campaigns", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "flash_sales",
                schema: "promotions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    campaign_id = table.Column<Guid>(type: "uuid", nullable: true),
                    variant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sale_price = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    stock_limit = table.Column<int>(type: "integer", nullable: true),
                    sold_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    starts_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ends_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_flash_sales", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "promotions",
                schema: "promotions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    campaign_id = table.Column<Guid>(type: "uuid", nullable: true),
                    code = table.Column<string>(type: "text", nullable: true),
                    discount_type = table.Column<string>(type: "text", nullable: false),
                    discount_value = table.Column<decimal>(type: "numeric(10,4)", nullable: false, defaultValue: 0m),
                    min_order_amount = table.Column<decimal>(type: "numeric(12,2)", nullable: false, defaultValue: 0m),
                    max_uses = table.Column<int>(type: "integer", nullable: true),
                    uses_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    max_uses_per_user = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    stackable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_promotions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "promotion_uses",
                schema: "promotions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    promotion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    discount_applied = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_promotion_uses", x => x.id);
                    table.ForeignKey(
                        name: "fk_promotion_uses_promotions_promotion_id",
                        column: x => x.promotion_id,
                        principalSchema: "promotions",
                        principalTable: "promotions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_promotion_uses_promotion_id_order_id",
                schema: "promotions",
                table: "promotion_uses",
                columns: new[] { "promotion_id", "order_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_promotions_code",
                schema: "promotions",
                table: "promotions",
                column: "code",
                unique: true,
                filter: "code IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "campaigns",
                schema: "promotions");

            migrationBuilder.DropTable(
                name: "flash_sales",
                schema: "promotions");

            migrationBuilder.DropTable(
                name: "promotion_uses",
                schema: "promotions");

            migrationBuilder.DropTable(
                name: "promotions",
                schema: "promotions");
        }
    }
}
