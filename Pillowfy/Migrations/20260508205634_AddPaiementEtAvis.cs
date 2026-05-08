using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pillowfy.Migrations
{
    /// <inheritdoc />
    public partial class AddPaiementEtAvis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Commentaire",
                table: "Avis",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateAvis",
                table: "Avis",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "HotelId",
                table: "Avis",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Note",
                table: "Avis",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Avis",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Paiements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Montant = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DatePaiement = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Methode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReservationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paiements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Paiements_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Avis_HotelId",
                table: "Avis",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_Avis_UserId_HotelId",
                table: "Avis",
                columns: new[] { "UserId", "HotelId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Paiements_ReservationId",
                table: "Paiements",
                column: "ReservationId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Avis_AspNetUsers_UserId",
                table: "Avis",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Avis_Hotels_HotelId",
                table: "Avis",
                column: "HotelId",
                principalTable: "Hotels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Avis_AspNetUsers_UserId",
                table: "Avis");

            migrationBuilder.DropForeignKey(
                name: "FK_Avis_Hotels_HotelId",
                table: "Avis");

            migrationBuilder.DropTable(
                name: "Paiements");

            migrationBuilder.DropIndex(
                name: "IX_Avis_HotelId",
                table: "Avis");

            migrationBuilder.DropIndex(
                name: "IX_Avis_UserId_HotelId",
                table: "Avis");

            migrationBuilder.DropColumn(
                name: "Commentaire",
                table: "Avis");

            migrationBuilder.DropColumn(
                name: "DateAvis",
                table: "Avis");

            migrationBuilder.DropColumn(
                name: "HotelId",
                table: "Avis");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "Avis");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Avis");
        }
    }
}
