using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductSlugSellerNameImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "main_image_url",
                schema: "catalog",
                table: "products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "seller_name",
                schema: "catalog",
                table: "products",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "slug",
                schema: "catalog",
                table: "products",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_products_slug",
                schema: "catalog",
                table: "products",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_categories_parent_id",
                schema: "catalog",
                table: "categories",
                column: "parent_id");

            migrationBuilder.AddForeignKey(
                name: "fk_categories_categories_parent_id",
                schema: "catalog",
                table: "categories",
                column: "parent_id",
                principalSchema: "catalog",
                principalTable: "categories",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_categories_categories_parent_id",
                schema: "catalog",
                table: "categories");

            migrationBuilder.DropIndex(
                name: "ix_products_slug",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropIndex(
                name: "ix_categories_parent_id",
                schema: "catalog",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "main_image_url",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropColumn(
                name: "seller_name",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropColumn(
                name: "slug",
                schema: "catalog",
                table: "products");
        }
    }
}
