$connStr = "Server=.;Database=SOF1022_NHOM1_SD2003_Ver2;Trusted_Connection=True;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT Id, Name FROM Authors ORDER BY Id"
$r = $cmd.ExecuteReader()
Write-Output "=== AUTHORS ==="
while ($r.Read()) {
    Write-Output "$($r['Id']): $($r['Name'])"
}
$r.Close()

$cmd.CommandText = "SELECT Id, Name FROM Publishers ORDER BY Id"
$r = $cmd.ExecuteReader()
Write-Output "=== PUBLISHERS ==="
while ($r.Read()) {
    Write-Output "$($r['Id']): $($r['Name'])"
}
$r.Close()
$conn.Close()
