using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SurveyBasket.Migrations
{
    /// <inheritdoc />
    public partial class AddDashboardAuditAndAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PollId",
                table: "Attachments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_PollId",
                table: "Attachments",
                column: "PollId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Polls_PollId",
                table: "Attachments",
                column: "PollId",
                principalTable: "Polls",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Polls_PollId",
                table: "Attachments");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_PollId",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "PollId",
                table: "Attachments");
        }
    }
}
