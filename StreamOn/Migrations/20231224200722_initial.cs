using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Netflix.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    email = table.Column<string>(type: "longtext", nullable: false),
                    password = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false),
                    isPublisher = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    isAdmin = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "movies",
                columns: table => new
                {
                    MovieId = table.Column<Guid>(type: "char(36)", nullable: false),
                    PublisherId = table.Column<Guid>(type: "char(36)", nullable: false),
                    MovieName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false),
                    MovieDescription = table.Column<string>(type: "varchar(600)", maxLength: 600, nullable: false),
                    views = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    like = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    dislike = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ReleaseDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PublishedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValue: new DateTime(2023, 12, 24, 23, 7, 22, 687, DateTimeKind.Local).AddTicks(1660)),
                    MovieLink = table.Column<string>(type: "longtext", nullable: false),
                    ImageLink = table.Column<string>(type: "longtext", nullable: false),
                    rating = table.Column<float>(type: "float", nullable: false, defaultValue: 0f),
                    approved = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    genre = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movies", x => x.MovieId);
                    table.ForeignKey(
                        name: "FK_movies_accounts_PublisherId",
                        column: x => x.PublisherId,
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "movieWatched",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    MovieId = table.Column<Guid>(type: "char(36)", nullable: false),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false),
                    liked = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    disliked = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movieWatched", x => x.id);
                    table.ForeignKey(
                        name: "FK_movieWatched_accounts_UserId",
                        column: x => x.UserId,
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.InsertData(
                table: "accounts",
                columns: new[] { "Id", "email", "isAdmin", "password" },
                values: new object[] { new Guid("90db3ce9-0631-4bd4-af9c-f9a6f8d7498e"), "admin@email.com", true, "XaXMmpjDe5X6M9J/v8xWKGpsn+AdYVx0dhH744pDzPm15Aa6" });

            migrationBuilder.CreateIndex(
                name: "IX_movies_PublisherId",
                table: "movies",
                column: "PublisherId");

            migrationBuilder.CreateIndex(
                name: "IX_movieWatched_UserId",
                table: "movieWatched",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "movies");

            migrationBuilder.DropTable(
                name: "movieWatched");

            migrationBuilder.DropTable(
                name: "accounts");
        }
    }
}
