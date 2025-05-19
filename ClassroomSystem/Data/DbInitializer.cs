using ClassroomSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ClassroomSystem.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // Ensure database exists and apply pending migrations
            await context.Database.MigrateAsync();

            // Create Identity tables if they don't exist
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (!pendingMigrations.Any())
            {
                await context.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetRoles' and xtype='U')
                    BEGIN
                        CREATE TABLE [dbo].[AspNetRoles] (
                            [Id] nvarchar(450) NOT NULL,
                            [Name] nvarchar(256) NULL,
                            [NormalizedName] nvarchar(256) NULL,
                            [ConcurrencyStamp] nvarchar(max) NULL,
                            CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
                        );

                        CREATE TABLE [dbo].[AspNetRoleClaims] (
                            [Id] int NOT NULL IDENTITY(1,1),
                            [RoleId] nvarchar(450) NOT NULL,
                            [ClaimType] nvarchar(max) NULL,
                            [ClaimValue] nvarchar(max) NULL,
                            CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
                            CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
                        );

                        CREATE TABLE [dbo].[AspNetUsers] (
                            [Id] nvarchar(450) NOT NULL,
                            [UserName] nvarchar(256) NULL,
                            [NormalizedUserName] nvarchar(256) NULL,
                            [Email] nvarchar(256) NULL,
                            [NormalizedEmail] nvarchar(256) NULL,
                            [EmailConfirmed] bit NOT NULL,
                            [PasswordHash] nvarchar(max) NULL,
                            [SecurityStamp] nvarchar(max) NULL,
                            [ConcurrencyStamp] nvarchar(max) NULL,
                            [PhoneNumber] nvarchar(max) NULL,
                            [PhoneNumberConfirmed] bit NOT NULL,
                            [TwoFactorEnabled] bit NOT NULL,
                            [LockoutEnd] datetimeoffset NULL,
                            [LockoutEnabled] bit NOT NULL,
                            [AccessFailedCount] int NOT NULL,
                            [FirstName] nvarchar(50) NOT NULL,
                            [LastName] nvarchar(50) NOT NULL,
                            [Role] nvarchar(20) NOT NULL,
                            [IsActive] bit NOT NULL,
                            [CreatedAt] datetime2 NOT NULL,
                            [LastLoginAt] datetime2 NULL,
                            [Title] nvarchar(100) NULL,
                            [Department] nvarchar(max) NOT NULL,
                            CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
                        );

                        CREATE TABLE [dbo].[AspNetUserClaims] (
                            [Id] int NOT NULL IDENTITY(1,1),
                            [UserId] nvarchar(450) NOT NULL,
                            [ClaimType] nvarchar(max) NULL,
                            [ClaimValue] nvarchar(max) NULL,
                            CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
                            CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
                        );

                        CREATE TABLE [dbo].[AspNetUserLogins] (
                            [LoginProvider] nvarchar(450) NOT NULL,
                            [ProviderKey] nvarchar(450) NOT NULL,
                            [ProviderDisplayName] nvarchar(max) NULL,
                            [UserId] nvarchar(450) NOT NULL,
                            CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
                            CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
                        );

                        CREATE TABLE [dbo].[AspNetUserRoles] (
                            [UserId] nvarchar(450) NOT NULL,
                            [RoleId] nvarchar(450) NOT NULL,
                            CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
                            CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
                            CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
                        );

                        CREATE TABLE [dbo].[AspNetUserTokens] (
                            [UserId] nvarchar(450) NOT NULL,
                            [LoginProvider] nvarchar(450) NOT NULL,
                            [Name] nvarchar(450) NOT NULL,
                            [Value] nvarchar(max) NULL,
                            CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
                            CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
                        );

                        CREATE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
                        CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
                        CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
                        CREATE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
                        CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
                        CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
                        CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
                    END
                ");
            }

            // Create roles if they don't exist
            string[] roles = { "Admin", "Instructor" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Check if we already have users
            if (await userManager.Users.AnyAsync())
            {
                return; // DB has been seeded
            }

            // Create default admin user
            var adminUser = new ApplicationUser
            {
                UserName = "admin@classroom.com",
                Email = "admin@classroom.com",
                FirstName = "Admin",
                LastName = "User",
                Role = "Admin",
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                Department = "Administration",
                Title = "System Administrator"
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Create default instructor user
            var instructorUser = new ApplicationUser
            {
                UserName = "instructor@classroom.com",
                Email = "instructor@classroom.com",
                FirstName = "Demo",
                LastName = "Instructor",
                Role = "Instructor",
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                Department = "Computer Science",
                Title = "Professor"
            };

            result = await userManager.CreateAsync(instructorUser, "Instructor123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(instructorUser, "Instructor");
            }
        }
    }
} 