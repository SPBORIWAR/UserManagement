using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagement.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Addingcolumninusertable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfilePicturePath",
                schema: "UserManagement",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePicturePath",
                schema: "UserManagement",
                table: "Users");
        }
    }
}
