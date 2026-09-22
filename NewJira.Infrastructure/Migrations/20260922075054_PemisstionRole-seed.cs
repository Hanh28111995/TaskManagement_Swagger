using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewJira.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PemisstionRoleseed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Code", "Description" },
                values: new object[,]
                {
            { 1, "project.view", "Xem dự án" },
            { 2, "project.manage", "Tạo/sửa/xóa dự án" },
            { 3, "task.view", "Xem task" },
            { 4, "task.create", "Tạo task" },
            { 5, "task.update.own", "Cập nhật task được giao" },
            { 6, "task.update.all", "Cập nhật mọi task" },
            { 7, "user.manage", "Quản lý người dùng" },
            { 8, "chat.group.create", "Tạo group chat" },
            { 9, "role.assign", "Phân quyền vai trò" },
                });

            // LƯU Ý: bảng có cột Id PK nên phải gán Id tường minh (1..12)
            migrationBuilder.InsertData(
                table: "PermissionRoles",
                columns: new[] { "Id", "RoleId", "PermissionId" },
                values: new object[,]
                {
            // Member (3)
            { 1, 3, 1 }, { 2, 3, 3 }, { 3, 3, 4 }, { 4, 3, 5 },
            // Manager (2)
            { 5, 2, 1 }, { 6, 2, 2 }, { 7, 2, 3 }, { 8, 2, 4 },
            { 9, 2, 5 }, { 10, 2, 6 }, { 11, 2, 8 },
            // Admin (1)
            { 12, 1, 1 }, { 13, 1, 2 }, { 14, 1, 3 }, { 15, 1, 4 },
            { 16, 1, 5 }, { 17, 1, 6 }, { 18, 1, 7 }, { 19, 1, 8 }, { 20, 1, 9 },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
