-- =====================================================================
-- PURE ROLE SEPARATION SCRIPT
-- =====================================================================

USE [SOF1022_NHOM1_SD2003_Ver2];
GO

-- 1. Reset permissions for Roles 2 to 6
DELETE FROM [RolePermissions] WHERE [RoleId] IN (2, 3, 4, 5, 6);
GO

-- 2. Admin (RoleId = 1): Full 60 permissions
INSERT INTO [RolePermissions] ([RoleId], [PermissionId])
SELECT 1, p.[Id]
FROM [Permissions] p
WHERE NOT EXISTS (
    SELECT 1 FROM [RolePermissions] rp WHERE rp.[RoleId] = 1 AND rp.[PermissionId] = p.[Id]
);
GO

-- 3. Role 2: Quản lý thư viện (Manager) - Oversight & Monitoring
INSERT INTO [RolePermissions] ([RoleId], [PermissionId])
SELECT 2, p.[Id]
FROM [Permissions] p
WHERE p.[Code] IN (
    'dashboard.view',
    'book.view',
    'category.view',
    'author.view',
    'publisher.view',
    'shelf.view',
    'reader.view',
    'employee.view',
    'borrow.view',
    'reservation.view',
    'fine.view',
    'notification.view',
    'notification.send',
    'support.view',
    'reader_page.view',
    'reader_page.borrow',
    'reader_page.comment',
    'reader_page.favorite'
);
GO

-- 4. Role 3: Thủ thư (Librarian) - Books, Cataloging & Circulation Counter ONLY
INSERT INTO [RolePermissions] ([RoleId], [PermissionId])
SELECT 3, p.[Id]
FROM [Permissions] p
WHERE p.[Code] IN (
    'dashboard.view',
    -- Books
    'book.view', 'book.create', 'book.edit',
    -- Categories
    'category.view', 'category.create', 'category.edit',
    -- Authors
    'author.view', 'author.create', 'author.edit',
    -- Publishers
    'publisher.view', 'publisher.create', 'publisher.edit',
    -- Circulation loans
    'borrow.view', 'borrow.create', 'borrow.edit', 'borrow.confirm_return',
    -- Reservations
    'reservation.view', 'reservation.create', 'reservation.edit', 'reservation.confirm', 'reservation.cancel',
    -- Reader lookup for loans
    'reader.view',
    -- Reader page
    'reader_page.view', 'reader_page.borrow', 'reader_page.comment', 'reader_page.favorite'
);
GO

-- 5. Role 4: Nhân viên kho (Warehouse) - Shelves & Book Stock Assignment ONLY
INSERT INTO [RolePermissions] ([RoleId], [PermissionId])
SELECT 4, p.[Id]
FROM [Permissions] p
WHERE p.[Code] IN (
    'dashboard.view',
    -- Shelves management
    'shelf.view', 'shelf.create', 'shelf.edit', 'shelf.delete',
    -- Books stock & shelf location
    'book.view', 'book.edit',
    -- Reader page
    'reader_page.view', 'reader_page.borrow', 'reader_page.comment', 'reader_page.favorite'
);
GO

-- 6. Role 5: Kế toán (Accountant) - Fines & Cashflow ONLY
INSERT INTO [RolePermissions] ([RoleId], [PermissionId])
SELECT 5, p.[Id]
FROM [Permissions] p
WHERE p.[Code] IN (
    'dashboard.view',
    -- Fines collection & processing
    'fine.view', 'fine.create', 'fine.edit', 'fine.delete', 'fine.process_payment',
    -- Reader page
    'reader_page.view', 'reader_page.borrow', 'reader_page.comment', 'reader_page.favorite'
);
GO

-- 7. Role 6: Chăm sóc khách hàng (CSKH) - Support, Notifications & Readers ONLY
INSERT INTO [RolePermissions] ([RoleId], [PermissionId])
SELECT 6, p.[Id]
FROM [Permissions] p
WHERE p.[Code] IN (
    'dashboard.view',
    -- Support tickets & live chat
    'support.view', 'support.create', 'support.edit', 'support.delete', 'support.reply',
    -- Notifications broadcast
    'notification.view', 'notification.create', 'notification.edit', 'notification.delete', 'notification.send',
    -- Reader account care
    'reader.view', 'reader.create', 'reader.edit',
    -- Reader page
    'reader_page.view', 'reader_page.borrow', 'reader_page.comment', 'reader_page.favorite'
);
GO

SELECT r.[Id] AS RoleId, r.[Name] AS RoleName, COUNT(rp.[PermissionId]) AS PermCount
FROM [Roles] r
LEFT JOIN [RolePermissions] rp ON r.[Id] = rp.[RoleId]
GROUP BY r.[Id], r.[Name]
ORDER BY r.[Id];
GO
