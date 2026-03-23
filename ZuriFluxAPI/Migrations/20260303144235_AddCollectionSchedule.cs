using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZuriFluxAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddCollectionSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CollectionSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BinId = table.Column<int>(type: "int", nullable: false),
                    RequestedByUserId = table.Column<int>(type: "int", nullable: false),
                    AssignedCollectorId = table.Column<int>(type: "int", nullable: true),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeSlot = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionSchedules_Bins_BinId",
                        column: x => x.BinId,
                        principalTable: "Bins",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CollectionSchedules_Users_AssignedCollectorId",
                        column: x => x.AssignedCollectorId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CollectionSchedules_Users_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionSchedules_AssignedCollectorId",
                table: "CollectionSchedules",
                column: "AssignedCollectorId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionSchedules_BinId",
                table: "CollectionSchedules",
                column: "BinId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionSchedules_RequestedByUserId",
                table: "CollectionSchedules",
                column: "RequestedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CollectionSchedules");
        }
    }
}
