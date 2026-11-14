$connStr = "Server=.;Database=SOF1022_NHOM1_SD2003_Ver2;Trusted_Connection=True;TrustServerCertificate=True;"
$sqlFile = "d:\test\SOF1022_NHOM1_SD2003_Ver2\SOF1022_NHOM1_SD2003_Ver2\Scripts\SOF1022_ASM_Database.sql"
$sqlText = [System.IO.File]::ReadAllText($sqlFile, [System.Text.Encoding]::UTF8)

# Split by GO commands (case-insensitive regex)
$batches = [regex]::Split($sqlText, "^\s*GO\s*$", [System.Text.RegularExpressions.RegexOptions]::Multiline -bor [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)

$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$conn.Open()

$success = 0
$errorCount = 0

foreach ($batch in $batches) {
    $trimmed = $batch.Trim()
    if ([string]::IsNullOrWhiteSpace($trimmed)) {
        continue
    }
    try {
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = $trimmed
        $cmd.CommandTimeout = 120
        $cmd.ExecuteNonQuery() | Out-Null
        $success++
    } catch {
        Write-Warning "Batch error: $($_.Exception.Message)"
        $errorCount++
    }
}

$conn.Close()
Write-Output "Execution completed: $success batches succeeded, $errorCount errors."
