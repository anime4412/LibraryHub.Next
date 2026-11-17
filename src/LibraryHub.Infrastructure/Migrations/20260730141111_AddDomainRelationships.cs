using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDomainRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_PermissionId",
                table: "UserPermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_AssignedStaffAccountId",
                table: "SupportTickets",
                column: "AssignedStaffAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_ReaderId",
                table: "SupportTickets",
                column: "ReaderId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportMessages_SenderAccountId",
                table: "SupportMessages",
                column: "SenderAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_BookId",
                table: "Reservations",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_ReadingHistories_ReaderId",
                table: "ReadingHistories",
                column: "ReaderId");

            migrationBuilder.CreateIndex(
                name: "IX_ReaderFeaturedBooks_BookId",
                table: "ReaderFeaturedBooks",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileTagPins_AuthorId",
                table: "ProfileTagPins",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileTagPins_ProfileOwnerId",
                table: "ProfileTagPins",
                column: "ProfileOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileComments_AuthorId",
                table: "ProfileComments",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileComments_ProfileOwnerId",
                table: "ProfileComments",
                column: "ProfileOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedByAccountId",
                table: "Notifications",
                column: "CreatedByAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Fines_BorrowSlipId",
                table: "Fines",
                column: "BorrowSlipId");

            migrationBuilder.CreateIndex(
                name: "IX_Fines_ReaderId",
                table: "Fines",
                column: "ReaderId");

            migrationBuilder.CreateIndex(
                name: "IX_FineDetails_BookId",
                table: "FineDetails",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_FineDetails_BorrowDetailId",
                table: "FineDetails",
                column: "BorrowDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_RoleId",
                table: "Employees",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_StatusOptionId",
                table: "Employees",
                column: "StatusOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentReports_BookCommentId",
                table: "CommentReports",
                column: "BookCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentReports_ProfileCommentId",
                table: "CommentReports",
                column: "ProfileCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentReports_ReportedUserId",
                table: "CommentReports",
                column: "ReportedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentReports_ReporterId",
                table: "CommentReports",
                column: "ReporterId");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowReturns_BorrowDetailId",
                table: "BorrowReturns",
                column: "BorrowDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_BookViewHistories_BookId",
                table: "BookViewHistories",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_BookViewHistories_ReaderId",
                table: "BookViewHistories",
                column: "ReaderId");

            migrationBuilder.CreateIndex(
                name: "IX_BookRatings_ReaderId",
                table: "BookRatings",
                column: "ReaderId");

            migrationBuilder.CreateIndex(
                name: "IX_BookFavorites_ReaderId",
                table: "BookFavorites",
                column: "ReaderId");

            migrationBuilder.CreateIndex(
                name: "IX_BookComments_BookId",
                table: "BookComments",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_BookComments_ParentCommentId",
                table: "BookComments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_BookComments_ReaderId",
                table: "BookComments",
                column: "ReaderId");

            migrationBuilder.CreateIndex(
                name: "IX_BookCommentLikes_ReaderId",
                table: "BookCommentLikes",
                column: "ReaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookCommentLikes_BookComments_BookCommentId",
                table: "BookCommentLikes",
                column: "BookCommentId",
                principalTable: "BookComments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BookCommentLikes_Readers_ReaderId",
                table: "BookCommentLikes",
                column: "ReaderId",
                principalTable: "Readers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BookComments_BookComments_ParentCommentId",
                table: "BookComments",
                column: "ParentCommentId",
                principalTable: "BookComments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BookComments_Books_BookId",
                table: "BookComments",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BookComments_Readers_ReaderId",
                table: "BookComments",
                column: "ReaderId",
                principalTable: "Readers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BookFavorites_Books_BookId",
                table: "BookFavorites",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BookFavorites_Readers_ReaderId",
                table: "BookFavorites",
                column: "ReaderId",
                principalTable: "Readers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BookRatings_Books_BookId",
                table: "BookRatings",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BookRatings_Readers_ReaderId",
                table: "BookRatings",
                column: "ReaderId",
                principalTable: "Readers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BookViewHistories_Books_BookId",
                table: "BookViewHistories",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BookViewHistories_Readers_ReaderId",
                table: "BookViewHistories",
                column: "ReaderId",
                principalTable: "Readers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BorrowReturns_BorrowDetails_BorrowDetailId",
                table: "BorrowReturns",
                column: "BorrowDetailId",
                principalTable: "BorrowDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CommentReports_BookComments_BookCommentId",
                table: "CommentReports",
                column: "BookCommentId",
                principalTable: "BookComments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CommentReports_ProfileComments_ProfileCommentId",
                table: "CommentReports",
                column: "ProfileCommentId",
                principalTable: "ProfileComments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CommentReports_Readers_ReportedUserId",
                table: "CommentReports",
                column: "ReportedUserId",
                principalTable: "Readers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CommentReports_Readers_ReporterId",
                table: "CommentReports",
                column: "ReporterId",
                principalTable: "Readers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_EmployeeStatusOptions_StatusOptionId",
                table: "Employees",
                column: "StatusOptionId",
                principalTable: "EmployeeStatusOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_SecurityRoles_RoleId",
                table: "Employees",
                column: "RoleId",
                principalTable: "SecurityRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FineDetails_Books_BookId",
                table: "FineDetails",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FineDetails_BorrowDetails_BorrowDetailId",
                table: "FineDetails",
                column: "BorrowDetailId",
                principalTable: "BorrowDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Fines_BorrowSlips_BorrowSlipId",
                table: "Fines",
                column: "BorrowSlipId",
                principalTable: "BorrowSlips",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Fines_Readers_ReaderId",
                table: "Fines",
                column: "ReaderId",
                principalTable: "Readers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_CreatedByAccountId",
                table: "Notifications",
                column: "CreatedByAccountId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_RecipientAccountId",
                table: "Notifications",
                column: "RecipientAccountId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfileComments_Readers_AuthorId",
                table: "ProfileComments",
                column: "AuthorId",
                principalTable: "Readers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProfileComments_Readers_ProfileOwnerId",
                table: "ProfileComments",
                column: "ProfileOwnerId",
                principalTable: "Readers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProfileTagPins_Readers_AuthorId",
                table: "ProfileTagPins",
                column: "AuthorId",
                principalTable: "Readers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProfileTagPins_Readers_ProfileOwnerId",
                table: "ProfileTagPins",
                column: "ProfileOwnerId",
                principalTable: "Readers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReaderFeaturedBooks_Books_BookId",
                table: "ReaderFeaturedBooks",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReaderFeaturedBooks_Readers_ReaderId",
                table: "ReaderFeaturedBooks",
                column: "ReaderId",
                principalTable: "Readers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReadingHistories_Books_BookId",
                table: "ReadingHistories",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReadingHistories_Readers_ReaderId",
                table: "ReadingHistories",
                column: "ReaderId",
                principalTable: "Readers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Books_BookId",
                table: "Reservations",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Readers_ReaderId",
                table: "Reservations",
                column: "ReaderId",
                principalTable: "Readers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Permissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId",
                principalTable: "Permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_SecurityRoles_RoleId",
                table: "RolePermissions",
                column: "RoleId",
                principalTable: "SecurityRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StaffAccounts_AspNetUsers_UserAccountId",
                table: "StaffAccounts",
                column: "UserAccountId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StaffAccounts_Employees_EmployeeId",
                table: "StaffAccounts",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SupportMessages_AspNetUsers_SenderAccountId",
                table: "SupportMessages",
                column: "SenderAccountId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTickets_AspNetUsers_AssignedStaffAccountId",
                table: "SupportTickets",
                column: "AssignedStaffAccountId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTickets_Readers_ReaderId",
                table: "SupportTickets",
                column: "ReaderId",
                principalTable: "Readers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermissions_Employees_EmployeeId",
                table: "UserPermissions",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermissions_Permissions_PermissionId",
                table: "UserPermissions",
                column: "PermissionId",
                principalTable: "Permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookCommentLikes_BookComments_BookCommentId",
                table: "BookCommentLikes");

            migrationBuilder.DropForeignKey(
                name: "FK_BookCommentLikes_Readers_ReaderId",
                table: "BookCommentLikes");

            migrationBuilder.DropForeignKey(
                name: "FK_BookComments_BookComments_ParentCommentId",
                table: "BookComments");

            migrationBuilder.DropForeignKey(
                name: "FK_BookComments_Books_BookId",
                table: "BookComments");

            migrationBuilder.DropForeignKey(
                name: "FK_BookComments_Readers_ReaderId",
                table: "BookComments");

            migrationBuilder.DropForeignKey(
                name: "FK_BookFavorites_Books_BookId",
                table: "BookFavorites");

            migrationBuilder.DropForeignKey(
                name: "FK_BookFavorites_Readers_ReaderId",
                table: "BookFavorites");

            migrationBuilder.DropForeignKey(
                name: "FK_BookRatings_Books_BookId",
                table: "BookRatings");

            migrationBuilder.DropForeignKey(
                name: "FK_BookRatings_Readers_ReaderId",
                table: "BookRatings");

            migrationBuilder.DropForeignKey(
                name: "FK_BookViewHistories_Books_BookId",
                table: "BookViewHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_BookViewHistories_Readers_ReaderId",
                table: "BookViewHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_BorrowReturns_BorrowDetails_BorrowDetailId",
                table: "BorrowReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_CommentReports_BookComments_BookCommentId",
                table: "CommentReports");

            migrationBuilder.DropForeignKey(
                name: "FK_CommentReports_ProfileComments_ProfileCommentId",
                table: "CommentReports");

            migrationBuilder.DropForeignKey(
                name: "FK_CommentReports_Readers_ReportedUserId",
                table: "CommentReports");

            migrationBuilder.DropForeignKey(
                name: "FK_CommentReports_Readers_ReporterId",
                table: "CommentReports");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_EmployeeStatusOptions_StatusOptionId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_SecurityRoles_RoleId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_FineDetails_Books_BookId",
                table: "FineDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_FineDetails_BorrowDetails_BorrowDetailId",
                table: "FineDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Fines_BorrowSlips_BorrowSlipId",
                table: "Fines");

            migrationBuilder.DropForeignKey(
                name: "FK_Fines_Readers_ReaderId",
                table: "Fines");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_CreatedByAccountId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_RecipientAccountId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfileComments_Readers_AuthorId",
                table: "ProfileComments");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfileComments_Readers_ProfileOwnerId",
                table: "ProfileComments");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfileTagPins_Readers_AuthorId",
                table: "ProfileTagPins");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfileTagPins_Readers_ProfileOwnerId",
                table: "ProfileTagPins");

            migrationBuilder.DropForeignKey(
                name: "FK_ReaderFeaturedBooks_Books_BookId",
                table: "ReaderFeaturedBooks");

            migrationBuilder.DropForeignKey(
                name: "FK_ReaderFeaturedBooks_Readers_ReaderId",
                table: "ReaderFeaturedBooks");

            migrationBuilder.DropForeignKey(
                name: "FK_ReadingHistories_Books_BookId",
                table: "ReadingHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_ReadingHistories_Readers_ReaderId",
                table: "ReadingHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Books_BookId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Readers_ReaderId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Permissions_PermissionId",
                table: "RolePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_SecurityRoles_RoleId",
                table: "RolePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_StaffAccounts_AspNetUsers_UserAccountId",
                table: "StaffAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_StaffAccounts_Employees_EmployeeId",
                table: "StaffAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportMessages_AspNetUsers_SenderAccountId",
                table: "SupportMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportTickets_AspNetUsers_AssignedStaffAccountId",
                table: "SupportTickets");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportTickets_Readers_ReaderId",
                table: "SupportTickets");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPermissions_Employees_EmployeeId",
                table: "UserPermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPermissions_Permissions_PermissionId",
                table: "UserPermissions");

            migrationBuilder.DropIndex(
                name: "IX_UserPermissions_PermissionId",
                table: "UserPermissions");

            migrationBuilder.DropIndex(
                name: "IX_SupportTickets_AssignedStaffAccountId",
                table: "SupportTickets");

            migrationBuilder.DropIndex(
                name: "IX_SupportTickets_ReaderId",
                table: "SupportTickets");

            migrationBuilder.DropIndex(
                name: "IX_SupportMessages_SenderAccountId",
                table: "SupportMessages");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_BookId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_ReadingHistories_ReaderId",
                table: "ReadingHistories");

            migrationBuilder.DropIndex(
                name: "IX_ReaderFeaturedBooks_BookId",
                table: "ReaderFeaturedBooks");

            migrationBuilder.DropIndex(
                name: "IX_ProfileTagPins_AuthorId",
                table: "ProfileTagPins");

            migrationBuilder.DropIndex(
                name: "IX_ProfileTagPins_ProfileOwnerId",
                table: "ProfileTagPins");

            migrationBuilder.DropIndex(
                name: "IX_ProfileComments_AuthorId",
                table: "ProfileComments");

            migrationBuilder.DropIndex(
                name: "IX_ProfileComments_ProfileOwnerId",
                table: "ProfileComments");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_CreatedByAccountId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Fines_BorrowSlipId",
                table: "Fines");

            migrationBuilder.DropIndex(
                name: "IX_Fines_ReaderId",
                table: "Fines");

            migrationBuilder.DropIndex(
                name: "IX_FineDetails_BookId",
                table: "FineDetails");

            migrationBuilder.DropIndex(
                name: "IX_FineDetails_BorrowDetailId",
                table: "FineDetails");

            migrationBuilder.DropIndex(
                name: "IX_Employees_RoleId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_StatusOptionId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_CommentReports_BookCommentId",
                table: "CommentReports");

            migrationBuilder.DropIndex(
                name: "IX_CommentReports_ProfileCommentId",
                table: "CommentReports");

            migrationBuilder.DropIndex(
                name: "IX_CommentReports_ReportedUserId",
                table: "CommentReports");

            migrationBuilder.DropIndex(
                name: "IX_CommentReports_ReporterId",
                table: "CommentReports");

            migrationBuilder.DropIndex(
                name: "IX_BorrowReturns_BorrowDetailId",
                table: "BorrowReturns");

            migrationBuilder.DropIndex(
                name: "IX_BookViewHistories_BookId",
                table: "BookViewHistories");

            migrationBuilder.DropIndex(
                name: "IX_BookViewHistories_ReaderId",
                table: "BookViewHistories");

            migrationBuilder.DropIndex(
                name: "IX_BookRatings_ReaderId",
                table: "BookRatings");

            migrationBuilder.DropIndex(
                name: "IX_BookFavorites_ReaderId",
                table: "BookFavorites");

            migrationBuilder.DropIndex(
                name: "IX_BookComments_BookId",
                table: "BookComments");

            migrationBuilder.DropIndex(
                name: "IX_BookComments_ParentCommentId",
                table: "BookComments");

            migrationBuilder.DropIndex(
                name: "IX_BookComments_ReaderId",
                table: "BookComments");

            migrationBuilder.DropIndex(
                name: "IX_BookCommentLikes_ReaderId",
                table: "BookCommentLikes");
        }
    }
}
