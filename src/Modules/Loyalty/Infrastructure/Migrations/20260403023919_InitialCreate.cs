using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Loyalty.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "loyalty");

            migrationBuilder.CreateTable(
                name: "loyalty_programs",
                schema: "loyalty",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    points_per_euro = table.Column<decimal>(type: "numeric(10,4)", nullable: false, defaultValue: 10m),
                    euro_per_point = table.Column<decimal>(type: "numeric(10,6)", nullable: false, defaultValue: 0.01m),
                    expiry_days = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_loyalty_programs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "loyalty_redemptions",
                schema: "loyalty",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reward_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    points_spent = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "pending"),
                    redeemed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_loyalty_redemptions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "loyalty_rewards",
                schema: "loyalty",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    program_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    reward_type = table.Column<string>(type: "text", nullable: false),
                    points_cost = table.Column<int>(type: "integer", nullable: false),
                    discount_value = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    stock = table.Column<int>(type: "integer", nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_loyalty_rewards", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "loyalty_tiers",
                schema: "loyalty",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    program_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    min_points = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    bonus_multiplier = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 1.0m),
                    perks = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'"),
                    rank = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_loyalty_tiers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "loyalty_accounts",
                schema: "loyalty",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    program_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_tier_id = table.Column<Guid>(type: "uuid", nullable: true),
                    points_balance = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    points_lifetime = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    tier_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_loyalty_accounts", x => x.id);
                    table.ForeignKey(
                        name: "fk_loyalty_accounts_loyalty_tiers_current_tier_id",
                        column: x => x.current_tier_id,
                        principalSchema: "loyalty",
                        principalTable: "loyalty_tiers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "loyalty_transactions",
                schema: "loyalty",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reward_id = table.Column<Guid>(type: "uuid", nullable: true),
                    type = table.Column<string>(type: "text", nullable: false),
                    points = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_loyalty_transactions", x => x.id);
                    table.ForeignKey(
                        name: "fk_loyalty_transactions_loyalty_accounts_account_id",
                        column: x => x.account_id,
                        principalSchema: "loyalty",
                        principalTable: "loyalty_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_loyalty_accounts_current_tier_id",
                schema: "loyalty",
                table: "loyalty_accounts",
                column: "current_tier_id");

            migrationBuilder.CreateIndex(
                name: "ix_loyalty_accounts_user_id_program_id",
                schema: "loyalty",
                table: "loyalty_accounts",
                columns: new[] { "user_id", "program_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_loyalty_tiers_program_id_name",
                schema: "loyalty",
                table: "loyalty_tiers",
                columns: new[] { "program_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_loyalty_tiers_program_id_rank",
                schema: "loyalty",
                table: "loyalty_tiers",
                columns: new[] { "program_id", "rank" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_loyalty_transactions_account_id",
                schema: "loyalty",
                table: "loyalty_transactions",
                column: "account_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "loyalty_programs",
                schema: "loyalty");

            migrationBuilder.DropTable(
                name: "loyalty_redemptions",
                schema: "loyalty");

            migrationBuilder.DropTable(
                name: "loyalty_rewards",
                schema: "loyalty");

            migrationBuilder.DropTable(
                name: "loyalty_transactions",
                schema: "loyalty");

            migrationBuilder.DropTable(
                name: "loyalty_accounts",
                schema: "loyalty");

            migrationBuilder.DropTable(
                name: "loyalty_tiers",
                schema: "loyalty");
        }
    }
}
