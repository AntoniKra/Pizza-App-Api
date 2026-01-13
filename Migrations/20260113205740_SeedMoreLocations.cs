using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PizzaApp.Migrations
{
    /// <inheritdoc />
    public partial class SeedMoreLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "IsoCode", "Name", "PhonePrefix" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), "IT", "Włochy", "+39" },
                    { new Guid("20000000-0000-0000-0000-000000000002"), "US", "Stany Zjednoczone", "+1" },
                    { new Guid("30000000-0000-0000-0000-000000000003"), "DE", "Niemcy", "+49" },
                    { new Guid("d28888e9-2ba9-473a-a40f-e38cb54f9b35"), "PL", "Polska", "+48" }
                });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Name", "Region" },
                values: new object[,]
                {
                    { new Guid("2902b665-1190-4c70-9915-b9c2d7680450"), new Guid("d28888e9-2ba9-473a-a40f-e38cb54f9b35"), "Kraków", "Małopolskie" },
                    { new Guid("44444444-4444-4444-4444-444444444444"), new Guid("d28888e9-2ba9-473a-a40f-e38cb54f9b35"), "Gdańsk", "Pomorskie" },
                    { new Guid("55555555-5555-5555-5555-555555555555"), new Guid("d28888e9-2ba9-473a-a40f-e38cb54f9b35"), "Wrocław", "Dolnośląskie" },
                    { new Guid("66666666-6666-6666-6666-666666666666"), new Guid("d28888e9-2ba9-473a-a40f-e38cb54f9b35"), "Poznań", "Wielkopolskie" },
                    { new Guid("77777777-7777-7777-7777-777777777777"), new Guid("10000000-0000-0000-0000-000000000001"), "Rzym", "Lacjum" },
                    { new Guid("88888888-8888-8888-8888-888888888888"), new Guid("10000000-0000-0000-0000-000000000001"), "Neapol", "Kampania" },
                    { new Guid("99999999-9999-9999-9999-999999999999"), new Guid("10000000-0000-0000-0000-000000000001"), "Mediolan", "Lombardia" },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new Guid("20000000-0000-0000-0000-000000000002"), "Nowy Jork", "Nowy Jork" },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), new Guid("20000000-0000-0000-0000-000000000002"), "Chicago", "Illinois" },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), new Guid("30000000-0000-0000-0000-000000000003"), "Berlin", "Berlin" },
                    { new Guid("da2fd609-d754-4feb-8acd-c4f9ff13ba96"), new Guid("d28888e9-2ba9-473a-a40f-e38cb54f9b35"), "Warszawa", "Mazowieckie" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: new Guid("2902b665-1190-4c70-9915-b9c2d7680450"));

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"));

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"));

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"));

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"));

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"));

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"));

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: new Guid("da2fd609-d754-4feb-8acd-c4f9ff13ba96"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("d28888e9-2ba9-473a-a40f-e38cb54f9b35"));
        }
    }
}
