using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFullClientIsolation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClientId",
                table: "TaskDependency",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ClientId",
                table: "Task",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ClientId",
                table: "Status",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ClientId",
                table: "Initiative",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ClientId",
                table: "Image",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ClientId",
                table: "DependencyType",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ClientId",
                table: "Comment",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql("""
                DECLARE @DefaultClientId uniqueidentifier = (SELECT TOP (1) [Id] FROM [Client] ORDER BY [CreatedAt]);
                IF @DefaultClientId IS NULL
                    THROW 50001, 'A client must exist before client isolation can be applied.', 1;

                UPDATE i SET [ClientId] = COALESCE(u.[ClientId], @DefaultClientId)
                FROM [Initiative] i LEFT JOIN [User] u ON u.[Id] = i.[AssignedToId];

                UPDATE t SET [ClientId] = COALESCE(i.[ClientId], u.[ClientId], @DefaultClientId)
                FROM [Task] t
                LEFT JOIN [Initiative] i ON i.[Id] = t.[InitiativeId]
                LEFT JOIN [User] u ON u.[Id] = t.[AssignedToId];

                UPDATE c SET [ClientId] = COALESCE(t.[ClientId], u.[ClientId], @DefaultClientId)
                FROM [Comment] c
                LEFT JOIN [Task] t ON t.[Id] = c.[TaskId]
                LEFT JOIN [User] u ON u.[Id] = c.[UserId];

                UPDATE d SET [ClientId] = COALESCE(p.[ClientId], s.[ClientId], @DefaultClientId)
                FROM [TaskDependency] d
                LEFT JOIN [Task] p ON p.[Id] = d.[PredecessorId]
                LEFT JOIN [Task] s ON s.[Id] = d.[SuccessorId];

                UPDATE img SET [ClientId] = COALESCE(u.[ClientId], ownerUser.[ClientId], @DefaultClientId)
                FROM [Image] img
                LEFT JOIN [User] u ON u.[Id] = img.[UploadedById]
                OUTER APPLY (SELECT TOP (1) usr.[ClientId] FROM [User] usr WHERE usr.[ImageId] = img.[Id]) ownerUser;

                UPDATE [Status] SET [ClientId] = @DefaultClientId;
                UPDATE [DependencyType] SET [ClientId] = @DefaultClientId;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_TaskDependency_ClientId",
                table: "TaskDependency",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Task_ClientId",
                table: "Task",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Status_ClientId",
                table: "Status",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Initiative_ClientId",
                table: "Initiative",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Image_ClientId",
                table: "Image",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_DependencyType_ClientId",
                table: "DependencyType",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_ClientId",
                table: "Comment",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Client_ClientId",
                table: "Comment",
                column: "ClientId",
                principalTable: "Client",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DependencyType_Client_ClientId",
                table: "DependencyType",
                column: "ClientId",
                principalTable: "Client",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Image_Client_ClientId",
                table: "Image",
                column: "ClientId",
                principalTable: "Client",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Initiative_Client_ClientId",
                table: "Initiative",
                column: "ClientId",
                principalTable: "Client",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Status_Client_ClientId",
                table: "Status",
                column: "ClientId",
                principalTable: "Client",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Task_Client_ClientId",
                table: "Task",
                column: "ClientId",
                principalTable: "Client",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskDependency_Client_ClientId",
                table: "TaskDependency",
                column: "ClientId",
                principalTable: "Client",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comment_Client_ClientId",
                table: "Comment");

            migrationBuilder.DropForeignKey(
                name: "FK_DependencyType_Client_ClientId",
                table: "DependencyType");

            migrationBuilder.DropForeignKey(
                name: "FK_Image_Client_ClientId",
                table: "Image");

            migrationBuilder.DropForeignKey(
                name: "FK_Initiative_Client_ClientId",
                table: "Initiative");

            migrationBuilder.DropForeignKey(
                name: "FK_Status_Client_ClientId",
                table: "Status");

            migrationBuilder.DropForeignKey(
                name: "FK_Task_Client_ClientId",
                table: "Task");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskDependency_Client_ClientId",
                table: "TaskDependency");

            migrationBuilder.DropIndex(
                name: "IX_TaskDependency_ClientId",
                table: "TaskDependency");

            migrationBuilder.DropIndex(
                name: "IX_Task_ClientId",
                table: "Task");

            migrationBuilder.DropIndex(
                name: "IX_Status_ClientId",
                table: "Status");

            migrationBuilder.DropIndex(
                name: "IX_Initiative_ClientId",
                table: "Initiative");

            migrationBuilder.DropIndex(
                name: "IX_Image_ClientId",
                table: "Image");

            migrationBuilder.DropIndex(
                name: "IX_DependencyType_ClientId",
                table: "DependencyType");

            migrationBuilder.DropIndex(
                name: "IX_Comment_ClientId",
                table: "Comment");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "TaskDependency");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Status");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Initiative");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Image");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "DependencyType");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Comment");
        }
    }
}
