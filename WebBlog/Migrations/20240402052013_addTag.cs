using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBlog.Migrations
{
    /// <inheritdoc />
    public partial class addTag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Tags_TagId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_TagId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "TagId",
                table: "Posts");

            migrationBuilder.AddColumn<string>(
                name: "slug",
                table: "Tags",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "slug",
                table: "Tags");

            migrationBuilder.AddColumn<int>(
                name: "TagId",
                table: "Posts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Posts_TagId",
                table: "Posts",
                column: "TagId");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Tags_TagId",
                table: "Posts",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id");
        }
    }
}
