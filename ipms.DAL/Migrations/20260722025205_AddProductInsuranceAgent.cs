using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ipms.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddProductInsuranceAgent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InsuranceAgentId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InsuranceAgentId",
                table: "Products");
        }
    }
}
