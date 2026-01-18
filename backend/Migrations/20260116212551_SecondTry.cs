using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiChat.Backend.Migrations
{
    /// <inheritdoc />
    public partial class SecondTry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Message_chat_ChatId",
                table: "Message");

            migrationBuilder.DropPrimaryKey(
                name: "PK_chat",
                table: "chat");

            migrationBuilder.RenameTable(
                name: "chat",
                newName: "Chat");

            migrationBuilder.RenameIndex(
                name: "IX_chat_UserId",
                table: "Chat",
                newName: "IX_Chat_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat",
                table: "Chat",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Message_Chat_ChatId",
                table: "Message",
                column: "ChatId",
                principalTable: "Chat",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Message_Chat_ChatId",
                table: "Message");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat",
                table: "Chat");

            migrationBuilder.RenameTable(
                name: "Chat",
                newName: "chat");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_UserId",
                table: "chat",
                newName: "IX_chat_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_chat",
                table: "chat",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Message_chat_ChatId",
                table: "Message",
                column: "ChatId",
                principalTable: "chat",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
