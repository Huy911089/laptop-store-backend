using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaptopStore.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class ChangeRoleKeyToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Users_Roles_RoleId')
BEGIN
    ALTER TABLE [Users] DROP CONSTRAINT [FK_Users_Roles_RoleId];
END

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Users_RoleId' AND object_id = OBJECT_ID(N'[Users]'))
BEGIN
    DROP INDEX [IX_Users_RoleId] ON [Users];
END

IF EXISTS (
    SELECT 1
    FROM sys.columns c
    JOIN sys.types t ON c.user_type_id = t.user_type_id
    WHERE c.object_id = OBJECT_ID(N'[Roles]') AND c.name = N'Id' AND t.name = N'uniqueidentifier'
)
BEGIN
    CREATE TABLE [Roles_Temp]
    (
        [Id] int IDENTITY(1,1) NOT NULL,
        [Name] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_Roles_Temp] PRIMARY KEY ([Id])
    );

    INSERT INTO [Roles_Temp] ([Name])
    SELECT DISTINCT [Name]
    FROM [Roles];

    IF COL_LENGTH('Users', 'RoleIdInt') IS NULL
    BEGIN
        ALTER TABLE [Users] ADD [RoleIdInt] int NULL;
    END

    EXEC(N'
        UPDATE u
        SET u.[RoleIdInt] = rt.[Id]
        FROM [Users] u
        INNER JOIN [Roles] r ON u.[RoleId] = r.[Id]
        INNER JOIN [Roles_Temp] rt ON rt.[Name] = r.[Name];

        DECLARE @defaultRoleId int = (SELECT TOP 1 [Id] FROM [Roles_Temp] ORDER BY [Id]);
        UPDATE [Users] SET [RoleIdInt] = @defaultRoleId WHERE [RoleIdInt] IS NULL;

        ALTER TABLE [Users] ALTER COLUMN [RoleIdInt] int NOT NULL;
        ALTER TABLE [Users] DROP COLUMN [RoleId];
        EXEC sp_rename ''Users.RoleIdInt'', ''RoleId'', ''COLUMN'';

        DROP TABLE [Roles];
        EXEC sp_rename ''Roles_Temp'', ''Roles'';

        CREATE INDEX [IX_Users_RoleId] ON [Users] ([RoleId]);
        ALTER TABLE [Users]
            ADD CONSTRAINT [FK_Users_Roles_RoleId]
            FOREIGN KEY ([RoleId]) REFERENCES [Roles]([Id]) ON DELETE NO ACTION;
    ');
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Users_Roles_RoleId')
BEGIN
    ALTER TABLE [Users] DROP CONSTRAINT [FK_Users_Roles_RoleId];
END

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Users_RoleId' AND object_id = OBJECT_ID(N'[Users]'))
BEGIN
    DROP INDEX [IX_Users_RoleId] ON [Users];
END

IF EXISTS (
    SELECT 1
    FROM sys.columns c
    JOIN sys.types t ON c.user_type_id = t.user_type_id
    WHERE c.object_id = OBJECT_ID(N'[Roles]') AND c.name = N'Id' AND t.name = N'int'
)
BEGIN
    CREATE TABLE [Roles_Temp]
    (
        [Id] uniqueidentifier NOT NULL CONSTRAINT [DF_Roles_Temp_Id] DEFAULT NEWSEQUENTIALID(),
        [Name] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_Roles_Temp] PRIMARY KEY ([Id])
    );

    INSERT INTO [Roles_Temp] ([Name])
    SELECT DISTINCT [Name]
    FROM [Roles];

    IF COL_LENGTH('Users', 'RoleIdGuid') IS NULL
    BEGIN
        ALTER TABLE [Users] ADD [RoleIdGuid] uniqueidentifier NULL;
    END

    EXEC(N'
        UPDATE u
        SET u.[RoleIdGuid] = rt.[Id]
        FROM [Users] u
        INNER JOIN [Roles] r ON u.[RoleId] = r.[Id]
        INNER JOIN [Roles_Temp] rt ON rt.[Name] = r.[Name];

        DECLARE @defaultRoleId uniqueidentifier = (SELECT TOP 1 [Id] FROM [Roles_Temp] ORDER BY [Name]);
        UPDATE [Users] SET [RoleIdGuid] = @defaultRoleId WHERE [RoleIdGuid] IS NULL;

        ALTER TABLE [Users] ALTER COLUMN [RoleIdGuid] uniqueidentifier NOT NULL;
        ALTER TABLE [Users] DROP COLUMN [RoleId];
        EXEC sp_rename ''Users.RoleIdGuid'', ''RoleId'', ''COLUMN'';

        DROP TABLE [Roles];
        EXEC sp_rename ''Roles_Temp'', ''Roles'';

        CREATE INDEX [IX_Users_RoleId] ON [Users] ([RoleId]);
        ALTER TABLE [Users]
            ADD CONSTRAINT [FK_Users_Roles_RoleId]
            FOREIGN KEY ([RoleId]) REFERENCES [Roles]([Id]) ON DELETE NO ACTION;
    ');
END
");
        }
    }
}
