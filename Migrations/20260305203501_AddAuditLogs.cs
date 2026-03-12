using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnifleqSolutions_IMS.Migrations
{
    public partial class AddAuditLogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$AlA7DJJBgAWjzn1BWRwtVuPcqR7MatiDuXMi75x13hF4HAYnO7TUG");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$tKIw.EgByfPam.WOpI8Q/uLy/oRVRRmBy3yLoBFtOp7tBvpbmdw7G");
        }
    }
}
