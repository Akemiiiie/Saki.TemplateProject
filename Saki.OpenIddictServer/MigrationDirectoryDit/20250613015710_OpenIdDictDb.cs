#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Saki.OpenIddictServer.MigrationDirectoryDit;

/// <inheritdoc />
public partial class OpenIdDictDb : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            "OpenIddictApplications",
            table => new
            {
                Id = table.Column<string>("nvarchar(450)", nullable: false),
                ApplicationType = table.Column<string>("nvarchar(50)", maxLength: 50, nullable: true),
                ClientId = table.Column<string>("nvarchar(100)", maxLength: 100, nullable: true),
                ClientSecret = table.Column<string>("nvarchar(max)", nullable: true),
                ClientType = table.Column<string>("nvarchar(50)", maxLength: 50, nullable: true),
                ConcurrencyToken = table.Column<string>("nvarchar(50)", maxLength: 50, nullable: true),
                ConsentType = table.Column<string>("nvarchar(50)", maxLength: 50, nullable: true),
                DisplayName = table.Column<string>("nvarchar(max)", nullable: true),
                DisplayNames = table.Column<string>("nvarchar(max)", nullable: true),
                JsonWebKeySet = table.Column<string>("nvarchar(max)", nullable: true),
                Permissions = table.Column<string>("nvarchar(max)", nullable: true),
                PostLogoutRedirectUris = table.Column<string>("nvarchar(max)", nullable: true),
                Properties = table.Column<string>("nvarchar(max)", nullable: true),
                RedirectUris = table.Column<string>("nvarchar(max)", nullable: true),
                Requirements = table.Column<string>("nvarchar(max)", nullable: true),
                Settings = table.Column<string>("nvarchar(max)", nullable: true)
            },
            constraints: table => { table.PrimaryKey("PK_OpenIddictApplications", x => x.Id); });

        migrationBuilder.CreateTable(
            "OpenIddictScopes",
            table => new
            {
                Id = table.Column<string>("nvarchar(450)", nullable: false),
                ConcurrencyToken = table.Column<string>("nvarchar(50)", maxLength: 50, nullable: true),
                Description = table.Column<string>("nvarchar(max)", nullable: true),
                Descriptions = table.Column<string>("nvarchar(max)", nullable: true),
                DisplayName = table.Column<string>("nvarchar(max)", nullable: true),
                DisplayNames = table.Column<string>("nvarchar(max)", nullable: true),
                Name = table.Column<string>("nvarchar(200)", maxLength: 200, nullable: true),
                Properties = table.Column<string>("nvarchar(max)", nullable: true),
                Resources = table.Column<string>("nvarchar(max)", nullable: true)
            },
            constraints: table => { table.PrimaryKey("PK_OpenIddictScopes", x => x.Id); });

        migrationBuilder.CreateTable(
            "OpenIddictAuthorizations",
            table => new
            {
                Id = table.Column<string>("nvarchar(450)", nullable: false),
                ApplicationId = table.Column<string>("nvarchar(450)", nullable: true),
                ConcurrencyToken = table.Column<string>("nvarchar(50)", maxLength: 50, nullable: true),
                CreationDate = table.Column<DateTime>("datetime2", nullable: true),
                Properties = table.Column<string>("nvarchar(max)", nullable: true),
                Scopes = table.Column<string>("nvarchar(max)", nullable: true),
                Status = table.Column<string>("nvarchar(50)", maxLength: 50, nullable: true),
                Subject = table.Column<string>("nvarchar(400)", maxLength: 400, nullable: true),
                Type = table.Column<string>("nvarchar(50)", maxLength: 50, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OpenIddictAuthorizations", x => x.Id);
                table.ForeignKey(
                    "FK_OpenIddictAuthorizations_OpenIddictApplications_ApplicationId",
                    x => x.ApplicationId,
                    "OpenIddictApplications",
                    "Id");
            });

        migrationBuilder.CreateTable(
            "OpenIddictTokens",
            table => new
            {
                Id = table.Column<string>("nvarchar(450)", nullable: false),
                ApplicationId = table.Column<string>("nvarchar(450)", nullable: true),
                AuthorizationId = table.Column<string>("nvarchar(450)", nullable: true),
                ConcurrencyToken = table.Column<string>("nvarchar(50)", maxLength: 50, nullable: true),
                CreationDate = table.Column<DateTime>("datetime2", nullable: true),
                ExpirationDate = table.Column<DateTime>("datetime2", nullable: true),
                Payload = table.Column<string>("nvarchar(max)", nullable: true),
                Properties = table.Column<string>("nvarchar(max)", nullable: true),
                RedemptionDate = table.Column<DateTime>("datetime2", nullable: true),
                ReferenceId = table.Column<string>("nvarchar(100)", maxLength: 100, nullable: true),
                Status = table.Column<string>("nvarchar(50)", maxLength: 50, nullable: true),
                Subject = table.Column<string>("nvarchar(400)", maxLength: 400, nullable: true),
                Type = table.Column<string>("nvarchar(50)", maxLength: 50, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OpenIddictTokens", x => x.Id);
                table.ForeignKey(
                    "FK_OpenIddictTokens_OpenIddictApplications_ApplicationId",
                    x => x.ApplicationId,
                    "OpenIddictApplications",
                    "Id");
                table.ForeignKey(
                    "FK_OpenIddictTokens_OpenIddictAuthorizations_AuthorizationId",
                    x => x.AuthorizationId,
                    "OpenIddictAuthorizations",
                    "Id");
            });

        migrationBuilder.CreateIndex(
            "IX_OpenIddictApplications_ClientId",
            "OpenIddictApplications",
            "ClientId",
            unique: true,
            filter: "[ClientId] IS NOT NULL");

        migrationBuilder.CreateIndex(
            "IX_OpenIddictAuthorizations_ApplicationId_Status_Subject_Type",
            "OpenIddictAuthorizations",
            new[] { "ApplicationId", "Status", "Subject", "Type" });

        migrationBuilder.CreateIndex(
            "IX_OpenIddictScopes_Name",
            "OpenIddictScopes",
            "Name",
            unique: true,
            filter: "[Name] IS NOT NULL");

        migrationBuilder.CreateIndex(
            "IX_OpenIddictTokens_ApplicationId_Status_Subject_Type",
            "OpenIddictTokens",
            new[] { "ApplicationId", "Status", "Subject", "Type" });

        migrationBuilder.CreateIndex(
            "IX_OpenIddictTokens_AuthorizationId",
            "OpenIddictTokens",
            "AuthorizationId");

        migrationBuilder.CreateIndex(
            "IX_OpenIddictTokens_ReferenceId",
            "OpenIddictTokens",
            "ReferenceId",
            unique: true,
            filter: "[ReferenceId] IS NOT NULL");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            "OpenIddictScopes");

        migrationBuilder.DropTable(
            "OpenIddictTokens");

        migrationBuilder.DropTable(
            "OpenIddictAuthorizations");

        migrationBuilder.DropTable(
            "OpenIddictApplications");
    }
}