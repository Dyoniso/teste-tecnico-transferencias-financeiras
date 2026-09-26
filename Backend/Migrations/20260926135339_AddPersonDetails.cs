using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "birth_date",
                table: "persons",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "city",
                table: "persons",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "complement",
                table: "persons",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "document",
                table: "persons",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "persons",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "neighborhood",
                table: "persons",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "number",
                table: "persons",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "phone",
                table: "persons",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "state",
                table: "persons",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "street",
                table: "persons",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "zip_code",
                table: "persons",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "birth_date",
                table: "persons");

            migrationBuilder.DropColumn(
                name: "city",
                table: "persons");

            migrationBuilder.DropColumn(
                name: "complement",
                table: "persons");

            migrationBuilder.DropColumn(
                name: "document",
                table: "persons");

            migrationBuilder.DropColumn(
                name: "email",
                table: "persons");

            migrationBuilder.DropColumn(
                name: "neighborhood",
                table: "persons");

            migrationBuilder.DropColumn(
                name: "number",
                table: "persons");

            migrationBuilder.DropColumn(
                name: "phone",
                table: "persons");

            migrationBuilder.DropColumn(
                name: "state",
                table: "persons");

            migrationBuilder.DropColumn(
                name: "street",
                table: "persons");

            migrationBuilder.DropColumn(
                name: "zip_code",
                table: "persons");
        }
    }
}
