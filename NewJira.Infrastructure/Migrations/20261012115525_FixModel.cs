using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewJira.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PermissionRoles_Roles_RoleId1",
                table: "PermissionRoles");

            migrationBuilder.DropIndex(
                name: "IX_PermissionRoles_RoleId1",
                table: "PermissionRoles");

            migrationBuilder.DropColumn(
                name: "RoleId1",
                table: "PermissionRoles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RoleId1",
                table: "PermissionRoles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PermissionRoles_RoleId1",
                table: "PermissionRoles",
                column: "RoleId1");

            migrationBuilder.AddForeignKey(
                name: "FK_PermissionRoles_Roles_RoleId1",
                table: "PermissionRoles",
                column: "RoleId1",
                principalTable: "Roles",
                principalColumn: "Id");
        }
    }
}
