using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogKrafter.Data.Sql.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlogKrafterCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Slug = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogKrafterCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BlogKrafterTags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Slug = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogKrafterTags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BlogKrafterPosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Slug = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Excerpt = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ContentFormat = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentHtml = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CoverImageUrl = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    CoverImageAlt = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    MetaTitle = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    MetaDescription = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    CanonicalUrl = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    CommentsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ReadingTimeMinutes = table.Column<int>(type: "int", nullable: false),
                    AuthorUserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    AuthorName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogKrafterPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlogKrafterPosts_BlogKrafterCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "BlogKrafterCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "BlogKrafterPostTags",
                columns: table => new
                {
                    PostId = table.Column<int>(type: "int", nullable: false),
                    TagId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogKrafterPostTags", x => new { x.PostId, x.TagId });
                    table.ForeignKey(
                        name: "FK_BlogKrafterPostTags_BlogKrafterPosts_PostId",
                        column: x => x.PostId,
                        principalTable: "BlogKrafterPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlogKrafterPostTags_BlogKrafterTags_TagId",
                        column: x => x.TagId,
                        principalTable: "BlogKrafterTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlogKrafterSlugRedirects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    Slug = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogKrafterSlugRedirects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlogKrafterSlugRedirects_BlogKrafterPosts_PostId",
                        column: x => x.PostId,
                        principalTable: "BlogKrafterPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlogKrafterCategories_TenantId_Slug",
                table: "BlogKrafterCategories",
                columns: new[] { "TenantId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlogKrafterPosts_CategoryId",
                table: "BlogKrafterPosts",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogKrafterPosts_TenantId_Slug",
                table: "BlogKrafterPosts",
                columns: new[] { "TenantId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlogKrafterPosts_TenantId_Status_PublishedAt",
                table: "BlogKrafterPosts",
                columns: new[] { "TenantId", "Status", "PublishedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BlogKrafterPostTags_TagId",
                table: "BlogKrafterPostTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogKrafterSlugRedirects_PostId",
                table: "BlogKrafterSlugRedirects",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogKrafterSlugRedirects_TenantId_Slug",
                table: "BlogKrafterSlugRedirects",
                columns: new[] { "TenantId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlogKrafterTags_TenantId_Slug",
                table: "BlogKrafterTags",
                columns: new[] { "TenantId", "Slug" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlogKrafterPostTags");

            migrationBuilder.DropTable(
                name: "BlogKrafterSlugRedirects");

            migrationBuilder.DropTable(
                name: "BlogKrafterTags");

            migrationBuilder.DropTable(
                name: "BlogKrafterPosts");

            migrationBuilder.DropTable(
                name: "BlogKrafterCategories");
        }
    }
}
