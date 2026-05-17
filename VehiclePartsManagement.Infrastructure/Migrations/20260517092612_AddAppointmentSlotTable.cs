using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VehiclePartsManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentSlotTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppointmentSlot",
                columns: table => new
                {
                    AppointmentSlotId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SlotDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SlotTime = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MaxCapacity = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentSlot", x => x.AppointmentSlotId);
                    table.CheckConstraint("CK_AppointmentSlot_MaxCapacity_Positive", "\"MaxCapacity\" > 0");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentSlot_IsActive",
                table: "AppointmentSlot",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentSlot_SlotDate_SlotTime",
                table: "AppointmentSlot",
                columns: new[] { "SlotDate", "SlotTime" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppointmentSlot");
        }
    }
}
