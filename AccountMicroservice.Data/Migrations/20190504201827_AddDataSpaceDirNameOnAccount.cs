using Microsoft.EntityFrameworkCore.Migrations;

namespace AccountMicroservice.Data.Migrations
{
    public partial class AddDataSpaceDirNameOnAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataSpaceDirName",
                table: "Accounts",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataSpaceDirName",
                table: "Accounts");
        }
    }
}
