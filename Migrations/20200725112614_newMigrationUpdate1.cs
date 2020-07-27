using Microsoft.EntityFrameworkCore.Migrations;

namespace SIMS.Migrations
{
    public partial class newMigrationUpdate1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_refreshTokens_StudentId",
                table: "refreshTokens",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_refreshTokens_Students_StudentId",
                table: "refreshTokens",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_refreshTokens_Students_StudentId",
                table: "refreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_refreshTokens_StudentId",
                table: "refreshTokens");
        }
    }
}
