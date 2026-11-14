$connStr = "Server=.;Database=SOF1022_NHOM1_SD2003_Ver2;Trusted_Connection=True;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT Id, ReaderCode, FullName, Email, Role, IsActive, IsVerified FROM Readers"
$r = $cmd.ExecuteReader()
Write-Output "=== READERS ==="
while ($r.Read()) {
    Write-Output "ID: $($r['Id']) | Code: $($r['ReaderCode']) | Name: $($r['FullName']) | Email: $($r['Email']) | Role: $($r['Role']) | Active: $($r['IsActive']) | Verified: $($r['IsVerified'])"
}
$r.Close()

$cmd.CommandText = "SELECT Id, EmployeeCode, FullName, Email, RoleId, StatusId FROM Employees"
$r = $cmd.ExecuteReader()
Write-Output "=== EMPLOYEES ==="
while ($r.Read()) {
    Write-Output "ID: $($r['Id']) | Code: $($r['EmployeeCode']) | Name: $($r['FullName']) | Email: $($r['Email']) | RoleId: $($r['RoleId']) | StatusId: $($r['StatusId'])"
}
$r.Close()
$conn.Close()
