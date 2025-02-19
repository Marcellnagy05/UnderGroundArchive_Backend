using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnderGroundArchive_Backend.Migrations
{
    /// <inheritdoc />
    public partial class fixFavourites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
     name: "Chapters",
     columns: table => new
     {
         ChapterId = table.Column<int>(type: "int", nullable: false)
             .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
         BookId = table.Column<int>(type: "int", nullable: false),
         ChapterNumber = table.Column<int>(type: "int", nullable: false),
         ChapterTitle = table.Column<string>(type: "varchar(255)", nullable: false)
             .Annotation("MySql:CharSet", "utf8mb4"),
         ChapterContent = table.Column<string>(type: "text", nullable: false)
             .Annotation("MySql:CharSet", "utf8mb4")
     },
     constraints: table =>
     {
         table.PrimaryKey("PK_Chapters", x => x.ChapterId);
         table.ForeignKey(
             name: "FK_Chapters_Books_BookId",
             column: x => x.BookId,
             principalTable: "Books",
             principalColumn: "BookId",
             onDelete: ReferentialAction.Cascade);
     })
     .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Chapters_BookId",
                table: "Chapters",
                column: "BookId");

            migrationBuilder.CreateTable(
                name: "Favourites",
                columns: table => new
                {
                    FavouriteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BookId = table.Column<int>(type: "int", nullable: false),
                    ChapterId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favourites", x => x.FavouriteId);
                    table.ForeignKey(
                        name: "FK_Favourites_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Favourites_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Favourites_Chapters_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "Chapters",
                        principalColumn: "ChapterId",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Favourites_BookId",
                table: "Favourites",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_Favourites_ChapterId",
                table: "Favourites",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_Favourites_UserId",
                table: "Favourites",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Chapters");
            migrationBuilder.DropTable(
                name: "Favourites");

            migrationBuilder.AddColumn<string>(
                name: "Favourites",
                table: "AspNetUsers",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
