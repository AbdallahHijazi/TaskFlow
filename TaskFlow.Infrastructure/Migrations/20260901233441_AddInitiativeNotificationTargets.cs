using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInitiativeNotificationTargets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InitiativeId",
                table: "Notification",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notification_InitiativeId",
                table: "Notification",
                column: "InitiativeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_Initiative_InitiativeId",
                table: "Notification",
                column: "InitiativeId",
                principalTable: "Initiative",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notification_Initiative_InitiativeId",
                table: "Notification");

            migrationBuilder.DropIndex(
                name: "IX_Notification_InitiativeId",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "InitiativeId",
                table: "Notification");
        }
    }
}
