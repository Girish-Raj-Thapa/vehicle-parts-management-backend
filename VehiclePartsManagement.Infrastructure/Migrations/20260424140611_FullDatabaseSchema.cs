using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VehiclePartsManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FullDatabaseSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PartCategory",
                columns: table => new
                {
                    PartCategoryId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CategoryDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartCategory", x => x.PartCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "VehicleBrand",
                columns: table => new
                {
                    VehicleBrandId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BrandName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleBrand", x => x.VehicleBrandId);
                });

            migrationBuilder.CreateTable(
                name: "Vendor",
                columns: table => new
                {
                    VendorId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VendorName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ContactPerson = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    VendorEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    VendorPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VendorAddress = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendor", x => x.VendorId);
                });

            migrationBuilder.CreateTable(
                name: "Part",
                columns: table => new
                {
                    PartId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartCategoryId = table.Column<int>(type: "integer", nullable: false),
                    Sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PartName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PartDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SellingPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    CurrentStock = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LowStockThreshold = table.Column<int>(type: "integer", nullable: false, defaultValue: 10),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Part", x => x.PartId);
                    table.CheckConstraint("CK_Part_CurrentStock_NonNegative", "\"CurrentStock\" >= 0");
                    table.CheckConstraint("CK_Part_LowStockThreshold_NonNegative", "\"LowStockThreshold\" >= 0");
                    table.CheckConstraint("CK_Part_SellingPrice_NonNegative", "\"SellingPrice\" >= 0");
                    table.ForeignKey(
                        name: "FK_Part_PartCategory_PartCategoryId",
                        column: x => x.PartCategoryId,
                        principalTable: "PartCategory",
                        principalColumn: "PartCategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VehicleModel",
                columns: table => new
                {
                    VehicleModelId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ModelName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    VehicleBrandId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleModel", x => x.VehicleModelId);
                    table.ForeignKey(
                        name: "FK_VehicleModel_VehicleBrand_VehicleBrandId",
                        column: x => x.VehicleBrandId,
                        principalTable: "VehicleBrand",
                        principalColumn: "VehicleBrandId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Purchase",
                columns: table => new
                {
                    PurchaseId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VendorId = table.Column<int>(type: "integer", nullable: false),
                    CreatedByApplicationUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    PurchaseInvoiceNo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    PurchaseStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purchase", x => x.PurchaseId);
                    table.CheckConstraint("CK_Purchase_PurchaseStatus_Valid", "\"PurchaseStatus\" IN ('Pending','Ordered','Received','Completed','Cancelled')");
                    table.CheckConstraint("CK_Purchase_TotalAmount_NonNegative", "\"TotalAmount\" >= 0");
                    table.ForeignKey(
                        name: "FK_Purchase_AspNetUsers_CreatedByApplicationUserId",
                        column: x => x.CreatedByApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Purchase_Vendor_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendor",
                        principalColumn: "VendorId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockMovement",
                columns: table => new
                {
                    StockMovementId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartId = table.Column<int>(type: "integer", nullable: false),
                    MovementType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    MovementQuantity = table.Column<int>(type: "integer", nullable: false),
                    MovementDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ReferenceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReferenceId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovement", x => x.StockMovementId);
                    table.CheckConstraint("CK_StockMovement_MovementQuantity_Positive", "\"MovementQuantity\" > 0");
                    table.CheckConstraint("CK_StockMovement_MovementType_Valid", "\"MovementType\" IN ('In','Out','Adjustment')");
                    table.CheckConstraint("CK_StockMovement_ReferenceType_Valid", "\"ReferenceType\" IN ('Purchase','Sale','ManualAdjustment','Return')");
                    table.ForeignKey(
                        name: "FK_StockMovement_Part_PartId",
                        column: x => x.PartId,
                        principalTable: "Part",
                        principalColumn: "PartId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PartCompatibility",
                columns: table => new
                {
                    PartId = table.Column<int>(type: "integer", nullable: false),
                    VehicleModelId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartCompatibility", x => new { x.PartId, x.VehicleModelId });
                    table.ForeignKey(
                        name: "FK_PartCompatibility_Part_PartId",
                        column: x => x.PartId,
                        principalTable: "Part",
                        principalColumn: "PartId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartCompatibility_VehicleModel_VehicleModelId",
                        column: x => x.VehicleModelId,
                        principalTable: "VehicleModel",
                        principalColumn: "VehicleModelId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Vehicle",
                columns: table => new
                {
                    VehicleId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApplicationUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    VehicleModelId = table.Column<int>(type: "integer", nullable: false),
                    VehicleNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ManufactureYear = table.Column<int>(type: "integer", nullable: true),
                    EngineNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ChassisNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FuelType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Color = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CurrentMileage = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    VehicleRegisteredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicle", x => x.VehicleId);
                    table.CheckConstraint("CK_Vehicle_CurrentMileage_NonNegative", "\"CurrentMileage\" >= 0");
                    table.CheckConstraint("CK_Vehicle_ManufactureYear_Range", "\"ManufactureYear\" IS NULL OR (\"ManufactureYear\" >= 1900 AND \"ManufactureYear\" <= 2100)");
                    table.ForeignKey(
                        name: "FK_Vehicle_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vehicle_VehicleModel_VehicleModelId",
                        column: x => x.VehicleModelId,
                        principalTable: "VehicleModel",
                        principalColumn: "VehicleModelId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseItem",
                columns: table => new
                {
                    PurchaseItemId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PurchaseId = table.Column<int>(type: "integer", nullable: false),
                    PartId = table.Column<int>(type: "integer", nullable: false),
                    QuantityPurchased = table.Column<int>(type: "integer", nullable: false),
                    UnitPriceAtPurchase = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PurchaseLineTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseItem", x => x.PurchaseItemId);
                    table.CheckConstraint("CK_PurchaseItem_PurchaseLineTotal_NonNegative", "\"PurchaseLineTotal\" >= 0");
                    table.CheckConstraint("CK_PurchaseItem_QuantityPurchased_Positive", "\"QuantityPurchased\" > 0");
                    table.CheckConstraint("CK_PurchaseItem_UnitPriceAtPurchase_Positive", "\"UnitPriceAtPurchase\" > 0");
                    table.ForeignKey(
                        name: "FK_PurchaseItem_Part_PartId",
                        column: x => x.PartId,
                        principalTable: "Part",
                        principalColumn: "PartId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseItem_Purchase_PurchaseId",
                        column: x => x.PurchaseId,
                        principalTable: "Purchase",
                        principalColumn: "PurchaseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PartRequest",
                columns: table => new
                {
                    PartRequestId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApplicationUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    VehicleId = table.Column<int>(type: "integer", nullable: true),
                    FulfilledPartId = table.Column<int>(type: "integer", nullable: true),
                    RequestedPartName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    RequestDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    RequestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    RequestStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartRequest", x => x.PartRequestId);
                    table.CheckConstraint("CK_PartRequest_RequestStatus_Valid", "\"RequestStatus\" IN ('Pending','Ordered','Received','Completed','Cancelled')");
                    table.ForeignKey(
                        name: "FK_PartRequest_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartRequest_Part_FulfilledPartId",
                        column: x => x.FulfilledPartId,
                        principalTable: "Part",
                        principalColumn: "PartId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartRequest_Vehicle_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicle",
                        principalColumn: "VehicleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Sale",
                columns: table => new
                {
                    SaleId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerApplicationUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    VehicleId = table.Column<int>(type: "integer", nullable: false),
                    ProcessedByApplicationUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    SaleInvoiceNo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SaleDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    SaleStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SubTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    LoyaltyDiscountPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    BalanceDue = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    PaymentStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sale", x => x.SaleId);
                    table.CheckConstraint("CK_Sale_BalanceDue_NonNegative", "\"BalanceDue\" >= 0");
                    table.CheckConstraint("CK_Sale_DiscountAmount_NonNegative", "\"DiscountAmount\" >= 0");
                    table.CheckConstraint("CK_Sale_LoyaltyDiscountPercent_Valid", "\"LoyaltyDiscountPercent\" >= 0 AND \"LoyaltyDiscountPercent\" <= 100");
                    table.CheckConstraint("CK_Sale_PaymentStatus_Valid", "\"PaymentStatus\" IN ('Unpaid','PartiallyPaid','Paid','Overdue')");
                    table.CheckConstraint("CK_Sale_SaleStatus_Valid", "\"SaleStatus\" IN ('Draft','Confirmed','Completed','Cancelled')");
                    table.CheckConstraint("CK_Sale_SubTotal_NonNegative", "\"SubTotal\" >= 0");
                    table.CheckConstraint("CK_Sale_TotalAmount_NonNegative", "\"TotalAmount\" >= 0");
                    table.ForeignKey(
                        name: "FK_Sale_AspNetUsers_CustomerApplicationUserId",
                        column: x => x.CustomerApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sale_AspNetUsers_ProcessedByApplicationUserId",
                        column: x => x.ProcessedByApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sale_Vehicle_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicle",
                        principalColumn: "VehicleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceAppointment",
                columns: table => new
                {
                    ServiceAppointmentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApplicationUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    VehicleId = table.Column<int>(type: "integer", nullable: false),
                    AppointmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AppointmentTime = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AppointmentStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AppointmentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AppointmentNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceAppointment", x => x.ServiceAppointmentId);
                    table.CheckConstraint("CK_ServiceAppointment_AppointmentStatus_Valid", "\"AppointmentStatus\" IN ('Pending','Confirmed','InProgress','Completed','Cancelled','NoShow')");
                    table.CheckConstraint("CK_ServiceAppointment_AppointmentType_Valid", "\"AppointmentType\" IN ('GeneralService','Repair','Inspection','Maintenance','Other')");
                    table.ForeignKey(
                        name: "FK_ServiceAppointment_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceAppointment_Vehicle_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicle",
                        principalColumn: "VehicleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payment",
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SaleId = table.Column<int>(type: "integer", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    PaymentMethod = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PaymentAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PaymentReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PaymentNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payment", x => x.PaymentId);
                    table.CheckConstraint("CK_Payment_PaymentAmount_Positive", "\"PaymentAmount\" > 0");
                    table.CheckConstraint("CK_Payment_PaymentMethod_Valid", "\"PaymentMethod\" IN ('Cash','Card','BankTransfer','MobileWallet','Cheque','Other')");
                    table.ForeignKey(
                        name: "FK_Payment_Sale_SaleId",
                        column: x => x.SaleId,
                        principalTable: "Sale",
                        principalColumn: "SaleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SaleItem",
                columns: table => new
                {
                    SaleItemId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SaleId = table.Column<int>(type: "integer", nullable: false),
                    PartId = table.Column<int>(type: "integer", nullable: false),
                    QuantitySold = table.Column<int>(type: "integer", nullable: false),
                    UnitPriceAtSale = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleItem", x => x.SaleItemId);
                    table.CheckConstraint("CK_SaleItem_LineTotal_NonNegative", "\"LineTotal\" >= 0");
                    table.CheckConstraint("CK_SaleItem_QuantitySold_Positive", "\"QuantitySold\" > 0");
                    table.CheckConstraint("CK_SaleItem_UnitPriceAtSale_Positive", "\"UnitPriceAtSale\" > 0");
                    table.ForeignKey(
                        name: "FK_SaleItem_Part_PartId",
                        column: x => x.PartId,
                        principalTable: "Part",
                        principalColumn: "PartId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SaleItem_Sale_SaleId",
                        column: x => x.SaleId,
                        principalTable: "Sale",
                        principalColumn: "SaleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceRecord",
                columns: table => new
                {
                    ServiceRecordId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServiceAppointmentId = table.Column<int>(type: "integer", nullable: false),
                    VehicleId = table.Column<int>(type: "integer", nullable: false),
                    ServiceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ServiceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Diagnosis = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    WorkDone = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    MileageAtService = table.Column<int>(type: "integer", nullable: false),
                    LaborCharge = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    NextServiceDueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRecord", x => x.ServiceRecordId);
                    table.CheckConstraint("CK_ServiceRecord_LaborCharge_NonNegative", "\"LaborCharge\" >= 0");
                    table.CheckConstraint("CK_ServiceRecord_MileageAtService_NonNegative", "\"MileageAtService\" >= 0");
                    table.ForeignKey(
                        name: "FK_ServiceRecord_ServiceAppointment_ServiceAppointmentId",
                        column: x => x.ServiceAppointmentId,
                        principalTable: "ServiceAppointment",
                        principalColumn: "ServiceAppointmentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceRecord_Vehicle_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicle",
                        principalColumn: "VehicleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Review",
                columns: table => new
                {
                    ReviewId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServiceRecordId = table.Column<int>(type: "integer", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    ReviewComment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ReviewDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Review", x => x.ReviewId);
                    table.CheckConstraint("CK_Review_Rating_Range", "\"Rating\" >= 1 AND \"Rating\" <= 5");
                    table.ForeignKey(
                        name: "FK_Review_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Review_ServiceRecord_ServiceRecordId",
                        column: x => x.ServiceRecordId,
                        principalTable: "ServiceRecord",
                        principalColumn: "ServiceRecordId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Part_PartCategoryId",
                table: "Part",
                column: "PartCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Part_Sku",
                table: "Part",
                column: "Sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartCategory_CategoryName",
                table: "PartCategory",
                column: "CategoryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartCompatibility_VehicleModelId",
                table: "PartCompatibility",
                column: "VehicleModelId");

            migrationBuilder.CreateIndex(
                name: "IX_PartRequest_ApplicationUserId",
                table: "PartRequest",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PartRequest_FulfilledPartId",
                table: "PartRequest",
                column: "FulfilledPartId");

            migrationBuilder.CreateIndex(
                name: "IX_PartRequest_RequestDate",
                table: "PartRequest",
                column: "RequestDate");

            migrationBuilder.CreateIndex(
                name: "IX_PartRequest_VehicleId",
                table: "PartRequest",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_PaymentDate",
                table: "Payment",
                column: "PaymentDate");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_SaleId",
                table: "Payment",
                column: "SaleId");

            migrationBuilder.CreateIndex(
                name: "IX_Purchase_CreatedByApplicationUserId",
                table: "Purchase",
                column: "CreatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Purchase_PurchaseInvoiceNo",
                table: "Purchase",
                column: "PurchaseInvoiceNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Purchase_VendorId",
                table: "Purchase",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseItem_PartId",
                table: "PurchaseItem",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseItem_PurchaseId",
                table: "PurchaseItem",
                column: "PurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Review_ApplicationUserId",
                table: "Review",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Review_ServiceRecordId",
                table: "Review",
                column: "ServiceRecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sale_CustomerApplicationUserId",
                table: "Sale",
                column: "CustomerApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Sale_ProcessedByApplicationUserId",
                table: "Sale",
                column: "ProcessedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Sale_SaleInvoiceNo",
                table: "Sale",
                column: "SaleInvoiceNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sale_VehicleId",
                table: "Sale",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleItem_PartId",
                table: "SaleItem",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleItem_SaleId",
                table: "SaleItem",
                column: "SaleId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceAppointment_ApplicationUserId",
                table: "ServiceAppointment",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceAppointment_VehicleId",
                table: "ServiceAppointment",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRecord_ServiceAppointmentId",
                table: "ServiceRecord",
                column: "ServiceAppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRecord_VehicleId",
                table: "ServiceRecord",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovement_MovementDate",
                table: "StockMovement",
                column: "MovementDate");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovement_PartId",
                table: "StockMovement",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_ApplicationUserId",
                table: "Vehicle",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_VehicleModelId",
                table: "Vehicle",
                column: "VehicleModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_VehicleNumber",
                table: "Vehicle",
                column: "VehicleNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleBrand_BrandName",
                table: "VehicleBrand",
                column: "BrandName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleModel_VehicleBrandId_ModelName",
                table: "VehicleModel",
                columns: new[] { "VehicleBrandId", "ModelName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartCompatibility");

            migrationBuilder.DropTable(
                name: "PartRequest");

            migrationBuilder.DropTable(
                name: "Payment");

            migrationBuilder.DropTable(
                name: "PurchaseItem");

            migrationBuilder.DropTable(
                name: "Review");

            migrationBuilder.DropTable(
                name: "SaleItem");

            migrationBuilder.DropTable(
                name: "StockMovement");

            migrationBuilder.DropTable(
                name: "Purchase");

            migrationBuilder.DropTable(
                name: "ServiceRecord");

            migrationBuilder.DropTable(
                name: "Sale");

            migrationBuilder.DropTable(
                name: "Part");

            migrationBuilder.DropTable(
                name: "Vendor");

            migrationBuilder.DropTable(
                name: "ServiceAppointment");

            migrationBuilder.DropTable(
                name: "PartCategory");

            migrationBuilder.DropTable(
                name: "Vehicle");

            migrationBuilder.DropTable(
                name: "VehicleModel");

            migrationBuilder.DropTable(
                name: "VehicleBrand");
        }
    }
}
