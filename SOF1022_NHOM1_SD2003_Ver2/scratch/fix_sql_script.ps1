$filePath = "d:\test\SOF1022_NHOM1_SD2003_Ver2\SOF1022_NHOM1_SD2003_Ver2\Scripts\SOF1022_ASM_Database.sql"
$content = [System.IO.File]::ReadAllText($filePath, [System.Text.Encoding]::UTF8)

# 1. Fix Authors column Bio -> Biography
$content = $content.Replace("[Name], [Bio], [Nationality], [IsDeleted]", "[Name], [Biography], [Nationality], [IsDeleted]")

# 2. Fix Employees insert to include UnexcusedDaysCount
$oldEmpCols = "([Id], [EmployeeId], [FullName], [Email], [Phone], [Address], [CCCD], [Gender], [DateOfBirth], [HireDate], [PositionId], [Salary], [StatusId], [IsDeleted], [RoleId]) VALUES"
$newEmpCols = "([Id], [EmployeeId], [FullName], [Email], [Phone], [Address], [CCCD], [Gender], [DateOfBirth], [HireDate], [PositionId], [Salary], [StatusId], [UnexcusedDaysCount], [IsDeleted], [RoleId]) VALUES"
$content = $content.Replace($oldEmpCols, $newEmpCols)
$content = $content.Replace("1, 28000000, 1, 0, 1)", "1, 28000000, 1, 0, 0, 1)")
$content = $content.Replace("2, 18500000, 1, 0, 2)", "2, 18500000, 1, 0, 0, 2)")
$content = $content.Replace("3, 12000000, 1, 0, 3)", "3, 12000000, 1, 0, 0, 3)")
$content = $content.Replace("4, 10500000, 1, 0, 4)", "4, 10500000, 1, 0, 0, 4)")
$content = $content.Replace("5, 13500000, 1, 0, 5)", "5, 13500000, 1, 0, 0, 5)")
$content = $content.Replace("4, 9500000, 1, 0, 6)", "4, 9500000, 1, 0, 0, 6)")

# 3. Fix Readers insert to include ConsecutiveLoginDays
$oldRdrCols = "([ReaderCode], [FullName], [DateOfBirth], [Phone], [Email], [Address], [IdentityNumber], [RegisterDate], [ExpirationDate], [Password], [Role], [IsActive], [EmailConfirmed], [IsVerified], [MaxBorrowBooks])"
$newRdrCols = "([ReaderCode], [FullName], [DateOfBirth], [Phone], [Email], [Address], [IdentityNumber], [RegisterDate], [ExpirationDate], [Password], [Role], [IsActive], [EmailConfirmed], [IsVerified], [MaxBorrowBooks], [ConsecutiveLoginDays])"
$content = $content.Replace($oldRdrCols, $newRdrCols)
$content = $content.Replace("'Admin', 1, 1, 1, 20);", "'Admin', 1, 1, 1, 20, 1);")
$content = $content.Replace("'Reader', 1, 1, 1, 5);", "'Reader', 1, 1, 1, 5, 1);")
$content = $content.Replace("'Reader', 1, 1, 1, 5),", "'Reader', 1, 1, 1, 5, 1),")

# 4. Fix BorrowDetails insert to include CompensationAmount
$oldBdCols = "INSERT INTO BorrowDetails ([BorrowSlipId], [BookId], [Quantity], [BorrowPrice], [IsReturned], [ConditionBefore], [Note])"
$newBdCols = "INSERT INTO BorrowDetails ([BorrowSlipId], [BookId], [Quantity], [BorrowPrice], [IsReturned], [ConditionBefore], [Note], [CompensationAmount])"
$content = $content.Replace($oldBdCols, $newBdCols)
$content = $content.Replace("N'Mượn đọc chuyên đề kỹ năng sống'),", "N'Mượn đọc chuyên đề kỹ năng sống', 0),")
$content = $content.Replace("N'Mượn tuần 1');", "N'Mượn tuần 1', 0);")
$content = $content.Replace("N'Chưa mang sách trả thư viện');", "N'Chưa mang sách trả thư viện', 0);")

$oldBdCols2 = "INSERT INTO BorrowDetails ([BorrowSlipId], [BookId], [Quantity], [BorrowPrice], [IsReturned], [ReturnedDate], [ConditionBefore], [ConditionAfter], [Note])"
$newBdCols2 = "INSERT INTO BorrowDetails ([BorrowSlipId], [BookId], [Quantity], [BorrowPrice], [IsReturned], [ReturnedDate], [ConditionBefore], [ConditionAfter], [Note], [CompensationAmount])"
$content = $content.Replace($oldBdCols2, $newBdCols2)
$content = $content.Replace("N'Trả đúng hạn');", "N'Trả đúng hạn', 0);")

# 5. Fix variable declarations in section 12 by replacing repeated DECLARE @shelf...
$content = $content.Replace("DECLARE @shelfA2 INT = (SELECT TOP 1 Id FROM Shelves WHERE Code = 'A2');`r`n    INSERT INTO Books", "SET @shelfA2 = (SELECT TOP 1 Id FROM Shelves WHERE Code = 'A2');`r`n    INSERT INTO Books")
$content = $content.Replace("DECLARE @shelfA3 INT = (SELECT TOP 1 Id FROM Shelves WHERE Code = 'A3');`r`n    INSERT INTO Books", "SET @shelfA3 = (SELECT TOP 1 Id FROM Shelves WHERE Code = 'A3');`r`n    INSERT INTO Books")

[System.IO.File]::WriteAllText($filePath, $content, [System.Text.Encoding]::UTF8)
Write-Output "Fixed SOF1022_ASM_Database.sql"
