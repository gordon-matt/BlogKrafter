using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogKrafter.Data.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlogKrafter_Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenantId = table.Column<string>(type: "TEXT", unicode: false, maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", unicode: false, maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogKrafter_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BlogKrafter_Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenantId = table.Column<string>(type: "TEXT", unicode: false, maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", unicode: false, maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogKrafter_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BlogKrafter_Posts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenantId = table.Column<string>(type: "TEXT", unicode: false, maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", unicode: false, maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Excerpt = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    ContentFormat = table.Column<int>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    ContentHtml = table.Column<string>(type: "TEXT", nullable: false),
                    CoverImageUrl = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    CoverImageAlt = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    MetaTitle = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    MetaDescription = table.Column<string>(type: "TEXT", maxLength: 320, nullable: true),
                    CanonicalUrl = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsFeatured = table.Column<bool>(type: "INTEGER", nullable: false),
                    CommentsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    ReadingTimeMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    AuthorUserId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    AuthorName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogKrafter_Posts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlogKrafter_Posts_BlogKrafter_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "BlogKrafter_Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "BlogKrafter_PostTags",
                columns: table => new
                {
                    PostId = table.Column<int>(type: "INTEGER", nullable: false),
                    TagId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogKrafter_PostTags", x => new { x.PostId, x.TagId });
                    table.ForeignKey(
                        name: "FK_BlogKrafter_PostTags_BlogKrafter_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "BlogKrafter_Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlogKrafter_PostTags_BlogKrafter_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "BlogKrafter_Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlogKrafter_SlugRedirects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenantId = table.Column<string>(type: "TEXT", unicode: false, maxLength: 64, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", unicode: false, maxLength: 200, nullable: false),
                    PostId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogKrafter_SlugRedirects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlogKrafter_SlugRedirects_BlogKrafter_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "BlogKrafter_Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlogKrafter_Categories_TenantId_Slug",
                table: "BlogKrafter_Categories",
                columns: new[] { "TenantId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlogKrafter_Posts_CategoryId",
                table: "BlogKrafter_Posts",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogKrafter_Posts_TenantId_Slug",
                table: "BlogKrafter_Posts",
                columns: new[] { "TenantId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlogKrafter_Posts_TenantId_Status_PublishedAt",
                table: "BlogKrafter_Posts",
                columns: new[] { "TenantId", "Status", "PublishedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BlogKrafter_PostTags_TagId",
                table: "BlogKrafter_PostTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogKrafter_SlugRedirects_PostId",
                table: "BlogKrafter_SlugRedirects",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogKrafter_SlugRedirects_TenantId_Slug",
                table: "BlogKrafter_SlugRedirects",
                columns: new[] { "TenantId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlogKrafter_Tags_TenantId_Slug",
                table: "BlogKrafter_Tags",
                columns: new[] { "TenantId", "Slug" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlogKrafter_PostTags");

            migrationBuilder.DropTable(
                name: "BlogKrafter_SlugRedirects");

            migrationBuilder.DropTable(
                name: "BlogKrafter_Tags");

            migrationBuilder.DropTable(
                name: "BlogKrafter_Posts");

            migrationBuilder.DropTable(
                name: "BlogKrafter_Categories");
        }
    }
}
