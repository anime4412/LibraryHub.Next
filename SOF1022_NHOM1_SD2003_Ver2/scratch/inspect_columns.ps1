$connStr = "Server=.;Database=SOF1022_NHOM1_SD2003_Ver2;Trusted_Connection=True;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$conn.Open()
$tables = @('Employees', 'Readers', 'Authors', 'Books', 'BorrowDetails', 'BorrowSlips', 'Fines', 'Reservations')

foreach ($t in $tables) {
    Write-Output "=== TABLE: $t ==="
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '$t' ORDER BY ORDINAL_POSITION"
    $r = $cmd.ExecuteReader()
    while ($r.Read()) {
        Write-Output ("  {0,-25} {1,-15} Nullable:{2,-5} Def:{3}" -f $r["COLUMN_NAME"], $r["DATA_TYPE"], $r["IS_NULLABLE"], $r["COLUMN_DEFAULT"])
    }
    $r.Close()
}
$conn.Close()
