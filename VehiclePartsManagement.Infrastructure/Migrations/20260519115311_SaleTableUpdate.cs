using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehiclePartsManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SaleTableUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "VehicleId",
                table: "Sale",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "ProcessedByApplicationUserId",
                table: "Sale",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(450)",
                oldMaxLength: 450);

            migrationBuilder.Sql("""
                ALTER TABLE "Sale"
                ADD COLUMN IF NOT EXISTS "SaleChannel" character varying(30);

                UPDATE "Sale"
                SET "SaleChannel" = 'Workshop'
                WHERE "SaleChannel" IS NULL OR btrim("SaleChannel") = '';

                ALTER TABLE "Sale"
                ALTER COLUMN "SaleChannel" SET DEFAULT 'Workshop';

                ALTER TABLE "Sale"
                ALTER COLUMN "SaleChannel" SET NOT NULL;
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conname = 'CK_Sale_SaleChannel_Valid'
                    ) THEN
                        ALTER TABLE "Sale"
                        ADD CONSTRAINT "CK_Sale_SaleChannel_Valid"
                        CHECK ("SaleChannel" IN ('Workshop','OnlineParts'));
                    END IF;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Sale_SaleChannel_Valid",
                table: "Sale");

            migrationBuilder.DropColumn(
                name: "SaleChannel",
                table: "Sale");

            migrationBuilder.AlterColumn<int>(
                name: "VehicleId",
                table: "Sale",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProcessedByApplicationUserId",
                table: "Sale",
                type: "character varying(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(450)",
                oldMaxLength: 450,
                oldNullable: true);
        }
    }
}
