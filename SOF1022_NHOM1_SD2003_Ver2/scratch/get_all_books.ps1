$connStr = "Server=.;Database=SOF1022_NHOM1_SD2003_Ver2;Trusted_Connection=True;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = @"
SELECT b.Id, b.ISBN, b.Title, a.Name AS Author, p.Name AS Publisher, c.Name AS Category, b.PublishYear, b.PageCount, b.Price, b.CoverImage
FROM Books b
LEFT JOIN Authors a ON b.AuthorsId = a.Id
LEFT JOIN Publishers p ON b.PublishersId = p.Id
LEFT JOIN Categories c ON b.CategoriesId = c.Id
ORDER BY b.Id
"@
$r = $cmd.ExecuteReader()
while ($r.Read()) {
    Write-Output ("ID: {0,3} | ISBN: {1,-14} | Title: {2,-35} | Author: {3,-22} | Pub: {4,-18} | Cat: {5,-20} | Year: {6} | Pages: {7} | Price: {8:N0} VNĐ" -f $r["Id"], $r["ISBN"], $r["Title"], $r["Author"], $r["Publisher"], $r["Category"], $r["PublishYear"], $r["PageCount"], $r["Price"])
}
$conn.Close()
