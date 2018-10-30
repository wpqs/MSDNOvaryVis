using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OvaryVisWebApp.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OvaryVis",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    D1mm = table.Column<int>(nullable: false),
                    D2mm = table.Column<int>(nullable: false),
                    D3mm = table.Column<int>(nullable: false),
                    JobSubmitted = table.Column<DateTime>(nullable: false),
                    ResultVis = table.Column<int>(nullable: false),
                    StatusMsg = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OvaryVis", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OvaryVis");
        }
    }
}
