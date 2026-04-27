using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BCT.EF.Migrations
{
    /// <inheritdoc />
    public partial class create_update_date : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RecordCreatedAt",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "RecordUpdatedAt",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "RecordCreatedAt",
                table: "Tags",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "RecordUpdatedAt",
                table: "Tags",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "RecordCreatedAt",
                table: "StringValue",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "RecordUpdatedAt",
                table: "StringValue",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "RecordCreatedAt",
                table: "SensitivityScenarios",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "RecordUpdatedAt",
                table: "SensitivityScenarios",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "RecordCreatedAt",
                table: "Projects",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "RecordUpdatedAt",
                table: "Projects",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "RecordCreatedAt",
                table: "ProjectGridWizards",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "RecordUpdatedAt",
                table: "ProjectGridWizards",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "RecordCreatedAt",
                table: "DoubleValues",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "RecordUpdatedAt",
                table: "DoubleValues",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "RecordCreatedAt",
                table: "Companies",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "RecordUpdatedAt",
                table: "Companies",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "RecordCreatedAt",
                table: "BoolValue",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "RecordUpdatedAt",
                table: "BoolValue",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecordCreatedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RecordUpdatedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RecordCreatedAt",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "RecordUpdatedAt",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "RecordCreatedAt",
                table: "StringValue");

            migrationBuilder.DropColumn(
                name: "RecordUpdatedAt",
                table: "StringValue");

            migrationBuilder.DropColumn(
                name: "RecordCreatedAt",
                table: "SensitivityScenarios");

            migrationBuilder.DropColumn(
                name: "RecordUpdatedAt",
                table: "SensitivityScenarios");

            migrationBuilder.DropColumn(
                name: "RecordCreatedAt",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "RecordUpdatedAt",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "RecordCreatedAt",
                table: "ProjectGridWizards");

            migrationBuilder.DropColumn(
                name: "RecordUpdatedAt",
                table: "ProjectGridWizards");

            migrationBuilder.DropColumn(
                name: "RecordCreatedAt",
                table: "DoubleValues");

            migrationBuilder.DropColumn(
                name: "RecordUpdatedAt",
                table: "DoubleValues");

            migrationBuilder.DropColumn(
                name: "RecordCreatedAt",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "RecordUpdatedAt",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "RecordCreatedAt",
                table: "BoolValue");

            migrationBuilder.DropColumn(
                name: "RecordUpdatedAt",
                table: "BoolValue");
        }
    }
}
