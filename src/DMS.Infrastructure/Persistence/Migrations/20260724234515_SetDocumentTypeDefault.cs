using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SetDocumentTypeDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "document_type",
                table: "documents",
                type: "text",
                nullable: false,
                defaultValue: "Ostalo",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.Sql("""
                UPDATE documents
                SET document_type = 'Ostalo'
                WHERE document_type IS NULL OR document_type = '';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE documents
                SET document_type = ''
                WHERE document_type = 'Ostalo';
                """);

            migrationBuilder.AlterColumn<string>(
                name: "document_type",
                table: "documents",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Ostalo");
        }
    }
}
