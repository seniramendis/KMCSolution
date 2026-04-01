using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KMC.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistrationDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RegisteredAt",
                table: "Registrations",
                newName: "RegistrationDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RegistrationDate",
                table: "Registrations",
                newName: "RegisteredAt");
        }
    }
}
