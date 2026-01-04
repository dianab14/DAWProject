using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MicroSocialPlatform.Migrations
{
    /// <inheritdoc />
    public partial class CurrentState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupMemberships_AspNetUsers_UserId",
                table: "GroupMemberships");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupMessages_AspNetUsers_UserId",
                table: "GroupMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_AspNetUsers_UserId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Follows_FollowerId",
                table: "Follows");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "GroupMessages",
                newName: "SentAt");

            migrationBuilder.RenameColumn(
                name: "Text",
                table: "Comments",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "isDeleted",
                table: "AspNetUsers",
                newName: "IsDeleted");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Posts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "GroupMessages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "JoinedAt",
                table: "GroupMemberships",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RequestedAt",
                table: "GroupMemberships",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptedAt",
                table: "Follows",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Comments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Follows_FollowerId_FollowedId",
                table: "Follows",
                columns: new[] { "FollowerId", "FollowedId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMemberships_AspNetUsers_UserId",
                table: "GroupMemberships",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMessages_AspNetUsers_UserId",
                table: "GroupMessages",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_AspNetUsers_UserId",
                table: "Posts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupMemberships_AspNetUsers_UserId",
                table: "GroupMemberships");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupMessages_AspNetUsers_UserId",
                table: "GroupMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_AspNetUsers_UserId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Follows_FollowerId_FollowedId",
                table: "Follows");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "GroupMessages");

            migrationBuilder.DropColumn(
                name: "JoinedAt",
                table: "GroupMemberships");

            migrationBuilder.DropColumn(
                name: "RequestedAt",
                table: "GroupMemberships");

            migrationBuilder.DropColumn(
                name: "AcceptedAt",
                table: "Follows");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "SentAt",
                table: "GroupMessages",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "Comments",
                newName: "Text");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "AspNetUsers",
                newName: "isDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Follows_FollowerId",
                table: "Follows",
                column: "FollowerId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMemberships_AspNetUsers_UserId",
                table: "GroupMemberships",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMessages_AspNetUsers_UserId",
                table: "GroupMessages",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_AspNetUsers_UserId",
                table: "Posts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
