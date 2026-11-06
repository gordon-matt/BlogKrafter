using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BlogKrafter.Data.Npgsql.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "bkraft");

            migrationBuilder.CreateTable(
                name: "Categories",
                schema: "bkraft",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<string>(type: "character varying(64)", unicode: false, maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Slug = table.Column<string>(type: "character varying(200)", unicode: false, maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                schema: "bkraft",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<string>(type: "character varying(64)", unicode: false, maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Slug = table.Column<string>(type: "character varying(200)", unicode: false, maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Posts",
                schema: "bkraft",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<string>(type: "character varying(64)", unicode: false, maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Slug = table.Column<string>(type: "character varying(200)", unicode: false, maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Excerpt = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ContentFormat = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    ContentHtml = table.Column<string>(type: "text", nullable: false),
                    CoverImageUrl = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CoverImageAlt = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    MetaTitle = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    MetaDescription = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    CanonicalUrl = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                    CommentsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ReadingTimeMinutes = table.Column<int>(type: "integer", nullable: false),
                    AuthorUserId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AuthorName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Posts_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "bkraft",
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PostTags",
                schema: "bkraft",
                columns: table => new
                {
                    PostId = table.Column<int>(type: "integer", nullable: false),
                    TagId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostTags", x => new { x.PostId, x.TagId });
                    table.ForeignKey(
                        name: "FK_PostTags_Posts_PostId",
                        column: x => x.PostId,
                        principalSchema: "bkraft",
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PostTags_Tags_TagId",
                        column: x => x.TagId,
                        principalSchema: "bkraft",
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SlugRedirects",
                schema: "bkraft",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<string>(type: "character varying(64)", unicode: false, maxLength: 64, nullable: false),
                    Slug = table.Column<string>(type: "character varying(200)", unicode: false, maxLength: 200, nullable: false),
                    PostId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlugRedirects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SlugRedirects_Posts_PostId",
                        column: x => x.PostId,
                        principalSchema: "bkraft",
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_TenantId_Slug",
                schema: "bkraft",
                table: "Categories",
                columns: new[] { "TenantId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Posts_CategoryId",
                schema: "bkraft",
                table: "Posts",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_TenantId_Slug",
                schema: "bkraft",
                table: "Posts",
                columns: new[] { "TenantId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Posts_TenantId_Status_PublishedAt",
                schema: "bkraft",
                table: "Posts",
                columns: new[] { "TenantId", "Status", "PublishedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PostTags_TagId",
                schema: "bkraft",
                table: "PostTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_SlugRedirects_PostId",
                schema: "bkraft",
                table: "SlugRedirects",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_SlugRedirects_TenantId_Slug",
                schema: "bkraft",
                table: "SlugRedirects",
                columns: new[] { "TenantId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_TenantId_Slug",
                schema: "bkraft",
                table: "Tags",
                columns: new[] { "TenantId", "Slug" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostTags",
                schema: "bkraft");

            migrationBuilder.DropTable(
                name: "SlugRedirects",
                schema: "bkraft");

            migrationBuilder.DropTable(
                name: "Tags",
                schema: "bkraft");

            migrationBuilder.DropTable(
                name: "Posts",
                schema: "bkraft");

            migrationBuilder.DropTable(
                name: "Categories",
                schema: "bkraft");
        }
    }
}
