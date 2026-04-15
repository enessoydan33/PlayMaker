using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayMaker.Migrations
{
    /// <inheritdoc />
    public partial class mig_8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PollComment_Comments_Id",
                table: "PollComment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PollComment",
                table: "PollComment");

            migrationBuilder.RenameTable(
                name: "PollComment",
                newName: "TeamComment");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TeamComment",
                table: "TeamComment",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamComment_Comments_Id",
                table: "TeamComment",
                column: "Id",
                principalTable: "Comments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeamComment_Comments_Id",
                table: "TeamComment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TeamComment",
                table: "TeamComment");

            migrationBuilder.RenameTable(
                name: "TeamComment",
                newName: "PollComment");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PollComment",
                table: "PollComment",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PollComment_Comments_Id",
                table: "PollComment",
                column: "Id",
                principalTable: "Comments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
