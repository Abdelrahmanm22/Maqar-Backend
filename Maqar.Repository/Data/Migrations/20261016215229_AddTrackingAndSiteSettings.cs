using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maqar.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTrackingAndSiteSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WhatsAppNumber",
                table: "AspNetUsers",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PropertyEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    VisitorHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyEvents_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SiteSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PhoneDisplay = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    WhatsApp = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    AddressAr = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    AddressEn = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    FacebookUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    InstagramUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SiteSettings",
                columns: new[] { "Id", "AddressAr", "AddressEn", "Email", "FacebookUrl", "InstagramUrl", "Phone", "PhoneDisplay", "UpdatedAt", "WhatsApp" },
                values: new object[] { 1, null, null, "hello@maqar.eg", null, null, "+201000000000", "+20 100 000 0000", null, "201000000000" });

            migrationBuilder.CreateIndex(
                name: "IX_PropertyEvents_PropertyId_Type_CreatedAt",
                table: "PropertyEvents",
                columns: new[] { "PropertyId", "Type", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PropertyEvents_VisitorHash_PropertyId_Type_CreatedAt",
                table: "PropertyEvents",
                columns: new[] { "VisitorHash", "PropertyId", "Type", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PropertyEvents");

            migrationBuilder.DropTable(
                name: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "WhatsAppNumber",
                table: "AspNetUsers");
        }
    }
}
