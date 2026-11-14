$connStr = "Server=.;Database=SOF1022_NHOM1_SD2003_Ver2;Trusted_Connection=True;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT Id, Name FROM Categories ORDER BY Id"
$r = $cmd.ExecuteReader()
Write-Output "=== CATEGORIES ==="
while ($r.Read()) {
    Write-Output "$($r['Id']): $($r['Name'])"
}
$r.Close()
$conn.Close()
