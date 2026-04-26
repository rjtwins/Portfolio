using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BCT.EF.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BoolValue",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<bool>(type: "INTEGER", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoolValue", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DoubleValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<double>(type: "REAL", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Year = table.Column<int>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoubleValues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectGridWizards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    Interest = table.Column<double>(type: "REAL", nullable: false),
                    Horizon = table.Column<int>(type: "INTEGER", nullable: false),
                    StartYear = table.Column<int>(type: "INTEGER", nullable: false),
                    InterestEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    NewInvestment = table.Column<double>(type: "REAL", nullable: false),
                    NewInvestmentYear = table.Column<int>(type: "INTEGER", nullable: false),
                    NewInvestmentDescription = table.Column<string>(type: "TEXT", nullable: false),
                    AvoidedInvestmentEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AvoidedInvestment = table.Column<double>(type: "REAL", nullable: false),
                    AvoidedInvestmentYear = table.Column<int>(type: "INTEGER", nullable: false),
                    AvoidedInvestmentDescription = table.Column<string>(type: "TEXT", nullable: false),
                    ICTHardware = table.Column<double>(type: "REAL", nullable: false),
                    ICTHardwareYear = table.Column<int>(type: "INTEGER", nullable: false),
                    ICTHardwareDescription = table.Column<string>(type: "TEXT", nullable: false),
                    ICTSoftware = table.Column<double>(type: "REAL", nullable: false),
                    ICTSoftwareYear = table.Column<int>(type: "INTEGER", nullable: false),
                    ICTSoftwareDescription = table.Column<string>(type: "TEXT", nullable: false),
                    Equipment = table.Column<double>(type: "REAL", nullable: false),
                    EquipmentYear = table.Column<int>(type: "INTEGER", nullable: false),
                    EquipmentDescription = table.Column<string>(type: "TEXT", nullable: false),
                    EquipmentUsage = table.Column<double>(type: "REAL", nullable: false),
                    EquipmentUsageYear = table.Column<int>(type: "INTEGER", nullable: false),
                    EquipmentUsageDescription = table.Column<string>(type: "TEXT", nullable: false),
                    Personnel = table.Column<double>(type: "REAL", nullable: false),
                    PersonnelYear = table.Column<int>(type: "INTEGER", nullable: false),
                    PersonnelDescription = table.Column<string>(type: "TEXT", nullable: false),
                    Energy = table.Column<double>(type: "REAL", nullable: false),
                    EnergyYear = table.Column<int>(type: "INTEGER", nullable: false),
                    EnergyDescription = table.Column<string>(type: "TEXT", nullable: false),
                    Other = table.Column<double>(type: "REAL", nullable: false),
                    OtherYear = table.Column<int>(type: "INTEGER", nullable: false),
                    OtherDescription = table.Column<string>(type: "TEXT", nullable: false),
                    AvoidedCostEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    EquipmentOvoided = table.Column<double>(type: "REAL", nullable: false),
                    EquipmentOvoidedYear = table.Column<int>(type: "INTEGER", nullable: false),
                    EquipmentOvoidedDescription = table.Column<string>(type: "TEXT", nullable: false),
                    EquipmentUsageOvoided = table.Column<double>(type: "REAL", nullable: false),
                    EquipmentUsageOvoidedDescription = table.Column<string>(type: "TEXT", nullable: false),
                    EquipmentUsageOvoidedYear = table.Column<int>(type: "INTEGER", nullable: false),
                    PersonnelOvoided = table.Column<double>(type: "REAL", nullable: false),
                    PersonnelOvoidedYear = table.Column<int>(type: "INTEGER", nullable: false),
                    PersonnelOvoidedDescription = table.Column<string>(type: "TEXT", nullable: false),
                    EnergyOvoided = table.Column<double>(type: "REAL", nullable: false),
                    EnergyOvoidedYear = table.Column<int>(type: "INTEGER", nullable: false),
                    EnergyOvoidedDescription = table.Column<string>(type: "TEXT", nullable: false),
                    OtherOvoided = table.Column<double>(type: "REAL", nullable: false),
                    OtherOvoidedYear = table.Column<int>(type: "INTEGER", nullable: false),
                    OtherOvoidedDescription = table.Column<string>(type: "TEXT", nullable: false),
                    ExtraIncome = table.Column<double>(type: "REAL", nullable: false),
                    ExtraIncomeYear = table.Column<int>(type: "INTEGER", nullable: false),
                    ExtraIncomeDescription = table.Column<string>(type: "TEXT", nullable: false),
                    ExtraIncomeEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LostIncome = table.Column<double>(type: "REAL", nullable: false),
                    LostIncomeYear = table.Column<int>(type: "INTEGER", nullable: false),
                    LostIncomeDescription = table.Column<string>(type: "TEXT", nullable: false),
                    LostIncomeEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    ResidualValueEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    ResidualValue = table.Column<double>(type: "REAL", nullable: false),
                    ResidualValueDescription = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectGridWizards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SensitivityScenarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    InvestmentMod = table.Column<double>(type: "REAL", nullable: false),
                    AvoidedInvestmentMod = table.Column<double>(type: "REAL", nullable: false),
                    CostMod = table.Column<double>(type: "REAL", nullable: false),
                    AvoidedCostMod = table.Column<double>(type: "REAL", nullable: false),
                    IncomeMod = table.Column<double>(type: "REAL", nullable: false),
                    LostIncomeMod = table.Column<double>(type: "REAL", nullable: false),
                    InvestmentDif = table.Column<double>(type: "REAL", nullable: false),
                    AvoidedInvestmentDif = table.Column<double>(type: "REAL", nullable: false),
                    CostDif = table.Column<double>(type: "REAL", nullable: false),
                    AvoidedCostDif = table.Column<double>(type: "REAL", nullable: false),
                    IncomeDif = table.Column<double>(type: "REAL", nullable: false),
                    LostIncomeDif = table.Column<double>(type: "REAL", nullable: false),
                    Mode = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensitivityScenarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StringValue",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StringValue", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AuthId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    LastProjectId = table.Column<int>(type: "INTEGER", nullable: true),
                    LastCompanyId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatorId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    BtwNumber = table.Column<string>(type: "TEXT", nullable: false),
                    Adres = table.Column<string>(type: "TEXT", nullable: false),
                    City = table.Column<string>(type: "TEXT", nullable: false),
                    PostalCode = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Companies_Users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CompanyUser",
                columns: table => new
                {
                    CompanyId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyUser", x => new { x.CompanyId, x.UserId });
                    table.ForeignKey(
                        name: "FK_CompanyUser_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanyUser_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CompanyId = table.Column<int>(type: "INTEGER", nullable: false),
                    StartYear = table.Column<int>(type: "INTEGER", nullable: false),
                    Horizon = table.Column<int>(type: "INTEGER", nullable: false),
                    InterestEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    ChosenGridMethod = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Text = table.Column<string>(type: "TEXT", nullable: false),
                    CompanyId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tags_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectTag",
                columns: table => new
                {
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    TagId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectTag", x => new { x.ProjectId, x.TagId });
                    table.ForeignKey(
                        name: "FK_ProjectTag_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectTag_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Companies_CreatorId",
                table: "Companies",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyUser_UserId",
                table: "CompanyUser",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CompanyId",
                table: "Projects",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTag_TagId",
                table: "ProjectTag",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_CompanyId_Text",
                table: "Tags",
                columns: new[] { "CompanyId", "Text" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_AuthId",
                table: "Users",
                column: "AuthId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BoolValue");

            migrationBuilder.DropTable(
                name: "CompanyUser");

            migrationBuilder.DropTable(
                name: "DoubleValues");

            migrationBuilder.DropTable(
                name: "ProjectGridWizards");

            migrationBuilder.DropTable(
                name: "ProjectTag");

            migrationBuilder.DropTable(
                name: "SensitivityScenarios");

            migrationBuilder.DropTable(
                name: "StringValue");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
