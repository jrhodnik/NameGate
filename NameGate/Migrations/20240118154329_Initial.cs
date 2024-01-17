using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NameGate.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DomainStatistics",
                columns: table => new
                {
                    Domain = table.Column<string>(type: "TEXT", nullable: false),
                    AllowedCount = table.Column<ulong>(type: "INTEGER", nullable: false),
                    BlockedCount = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainStatistics", x => x.Domain);
                });

            migrationBuilder.CreateTable(
                name: "IpStatistics",
                columns: table => new
                {
                    IpAddress = table.Column<string>(type: "TEXT", nullable: false),
                    AllowedCount = table.Column<ulong>(type: "INTEGER", nullable: false),
                    BlockedCount = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IpStatistics", x => x.IpAddress);
                });

            migrationBuilder.CreateTable(
                name: "WhiteListEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DomainGlob = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhiteListEntries", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DomainStatistics");

            migrationBuilder.DropTable(
                name: "IpStatistics");

            migrationBuilder.DropTable(
                name: "WhiteListEntries");
        }
    }
}
