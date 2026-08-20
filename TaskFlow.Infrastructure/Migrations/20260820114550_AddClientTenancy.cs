using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClientTenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClientId",
                table: "User",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Client",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Client", x => x.Id);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO [Client] ([Id], [Name], [ContactEmail], [IsActive], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted])
                VALUES ('00000000-0000-0000-0000-000000000000', 'Default Client', NULL, 1, SYSUTCDATETIME(), NULL, NULL, NULL, 0)
                """);

            migrationBuilder.CreateIndex(
                name: "IX_User_ClientId",
                table: "User",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Client_Name",
                table: "Client",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_User_Client_ClientId",
                table: "User",
                column: "ClientId",
                principalTable: "Client",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Client_ClientId",
                table: "User");

            migrationBuilder.DropTable(
                name: "Client");

            migrationBuilder.DropIndex(
                name: "IX_User_ClientId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "User");
        }
    }
}
