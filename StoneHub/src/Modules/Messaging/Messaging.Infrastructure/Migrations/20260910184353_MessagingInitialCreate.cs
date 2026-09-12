using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Messaging.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MessagingInitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "messaging");

            migrationBuilder.CreateTable(
                name: "Conversations",
                schema: "messaging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParticipantAId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParticipantAName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ParticipantAAvatarUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ParticipantBId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParticipantBName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ParticipantBAvatarUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LastMessagePreview = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LastMessageAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ParticipantALastReadAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ParticipantBLastReadAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                schema: "messaging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Body = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    MediaUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SentAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_ParticipantAId",
                schema: "messaging",
                table: "Conversations",
                column: "ParticipantAId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_ParticipantAId_ParticipantBId",
                schema: "messaging",
                table: "Conversations",
                columns: new[] { "ParticipantAId", "ParticipantBId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_ParticipantBId",
                schema: "messaging",
                table: "Conversations",
                column: "ParticipantBId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ConversationId_SentAt",
                schema: "messaging",
                table: "Messages",
                columns: new[] { "ConversationId", "SentAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Conversations",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "Messages",
                schema: "messaging");
        }
    }
}
