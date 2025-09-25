USE superchat;
GO
 
-- 1. DECLARE VARIABLES for User IDs, Vendor IDs, and Role IDs
DECLARE @AdminUserId INT, @LeaderUserId INT, @VendorUserId INT, @Admin2UserId INT;
DECLARE @SampleVendorId INT;
DECLARE @RoleAdminId INT = 1;
DECLARE @RoleLeaderId INT = 2;
DECLARE @RoleVendorId INT = 3;
 
 
-- 2. CREATE SAMPLE USERS AND CAPTURE THEIR NEW IDs
 
-- ADMIN USER 1 (Password: hlo123)
INSERT INTO Users (Email, PasswordHash, FirstName, LastName, CreatedAt, PublicId)
VALUES ('admin@example.com', '$2a$11$2HWtCEy9K7FrQ8OhbHVSROTysfkZeIVJhj33XNnycVsLU.OZAJb3a', 'Admin', 'User', GETUTCDATE(), NEWID());
SET @AdminUserId = SCOPE_IDENTITY();
 
-- LEADER USER (Password: hii12345)
INSERT INTO Users (Email, PasswordHash, FirstName, LastName, CreatedAt, PublicId)
VALUES ('leader@example.com', '$2a$11$cGlnksQmEeWoIhatkVp0qOWZQwWDVqMliz.ojsDahTMcwJZFXqMmO', 'Lead', 'User', GETUTCDATE(), NEWID());
SET @LeaderUserId = SCOPE_IDENTITY();
 
-- VENDOR USER (Password: 12345)
INSERT INTO Users (Email, PasswordHash, FirstName, LastName, CreatedAt, PublicId)
VALUES ('vendor@example.com', '$2a$11$MzIx4iNXZcvMwI1aXiWZYOqu9.3vldQMttpVzGvBbJu0dY9jTkEx2', 'Vendor', 'User', GETUTCDATE(), NEWID());
SET @VendorUserId = SCOPE_IDENTITY();
 
 
-- 3. LINK USERS TO ROLES (The Role IDs 1, 2, 3 are already in the DB from the migration)
INSERT INTO UserRoles (UsersId, RolesId) VALUES (@AdminUserId, @RoleAdminId);     -- Admin User
INSERT INTO UserRoles (UsersId, RolesId) VALUES (@LeaderUserId, @RoleLeaderId);   -- Leader User
INSERT INTO UserRoles (UsersId, RolesId) VALUES (@VendorUserId, @RoleVendorId);   -- Vendor User
 
 
-- 4. CREATE A SAMPLE VENDOR RECORD
-- This links the Vendor's User account (@VendorUserId) and sets the Leader who added them (@LeaderUserId).
INSERT INTO Vendors (CompanyName, ContactEmail, Country, Status, UserId, AddedByLeaderId, CreatedAt, PublicId)
VALUES ('Sample Vendor Co', 'vendor@example.com', 'USA', 'Active', @VendorUserId, @LeaderUserId, GETUTCDATE(), NEWID());
SET @SampleVendorId = SCOPE_IDENTITY();
 
 
-- 5. VERIFY THE FINAL RESULT
SELECT u.Email, r.Name AS RoleName
FROM Users u
JOIN UserRoles ur ON u.Id = ur.UsersId
JOIN Roles r ON ur.RolesId = r.Id;
 
SELECT CompanyName, ContactEmail, Country FROM Vendors;
GO