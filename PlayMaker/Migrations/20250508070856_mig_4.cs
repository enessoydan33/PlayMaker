using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayMaker.Migrations
{
    /// <inheritdoc />
    public partial class mig_4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PollComment_Polls_PollId",
                table: "PollComment");

            migrationBuilder.DropForeignKey(
                name: "FK_UserVotes_Options_OptionId",
                table: "UserVotes");

            migrationBuilder.DropIndex(
                name: "IX_UserVotes_OptionId",
                table: "UserVotes");

            migrationBuilder.DropIndex(
                name: "IX_PollComment_PollId",
                table: "PollComment");

            migrationBuilder.DropColumn(
                name: "OptionId",
                table: "UserVotes");

            migrationBuilder.DropColumn(
                name: "PollId",
                table: "PollComment");

            migrationBuilder.RenameColumn(
                name: "VoteId",
                table: "UserVotes",
                newName: "SelectedOption");

            migrationBuilder.AddColumn<DateTime>(
                name: "Time",
                table: "Polls",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "TeamName",
                table: "PollComment",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Time",
                table: "Polls");

            migrationBuilder.DropColumn(
                name: "TeamName",
                table: "PollComment");

            migrationBuilder.RenameColumn(
                name: "SelectedOption",
                table: "UserVotes",
                newName: "VoteId");

            migrationBuilder.AddColumn<int>(
                name: "OptionId",
                table: "UserVotes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PollId",
                table: "PollComment",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserVotes_OptionId",
                table: "UserVotes",
                column: "OptionId");

            migrationBuilder.CreateIndex(
                name: "IX_PollComment_PollId",
                table: "PollComment",
                column: "PollId");

            migrationBuilder.AddForeignKey(
                name: "FK_PollComment_Polls_PollId",
                table: "PollComment",
                column: "PollId",
                principalTable: "Polls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserVotes_Options_OptionId",
                table: "UserVotes",
                column: "OptionId",
                principalTable: "Options",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
