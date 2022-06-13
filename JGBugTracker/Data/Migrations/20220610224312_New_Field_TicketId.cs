using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JGBugTracker.Data.Migrations
{
    public partial class New_Field_TicketId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_InviteeId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_InvitorId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_InviteeId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_InvitorId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "InviteeId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "InvitorId",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "Sender",
                table: "Notifications",
                newName: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientId",
                table: "Notifications",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_SenderId",
                table: "Notifications",
                column: "SenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_RecipientId",
                table: "Notifications",
                column: "RecipientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_SenderId",
                table: "Notifications",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_RecipientId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_SenderId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_RecipientId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_SenderId",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "SenderId",
                table: "Notifications",
                newName: "Sender");

            migrationBuilder.AddColumn<string>(
                name: "InviteeId",
                table: "Notifications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvitorId",
                table: "Notifications",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_InviteeId",
                table: "Notifications",
                column: "InviteeId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_InvitorId",
                table: "Notifications",
                column: "InvitorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_InviteeId",
                table: "Notifications",
                column: "InviteeId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_InvitorId",
                table: "Notifications",
                column: "InvitorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
