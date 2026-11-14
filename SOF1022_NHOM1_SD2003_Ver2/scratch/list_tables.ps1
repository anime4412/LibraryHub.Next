$connStr = "Server=.;Database=SOF1022_NHOM1_SD2003_Ver2;Trusted_Connection=True;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = @"
SELECT t.name AS TableName, p.rows AS [RowCount]
FROM sys.tables t
INNER JOIN sys.partitions p ON t.object_id = p.object_id AND p.index_id IN (0, 1)
ORDER BY t.name
"@
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Output ("{0,-30} : {1} rows" -f $reader["TableName"], $reader["RowCount"])
}
$conn.Close()
