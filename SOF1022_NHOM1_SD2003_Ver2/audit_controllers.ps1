$controllers = Get-ChildItem "d:\test\SOF1022_NHOM1_SD2003_Ver2\SOF1022_NHOM1_SD2003_Ver2\Controllers\*.cs"
foreach ($c in $controllers) {
    $text = Get-Content $c.FullName -Raw
    $hasSession = $text -match "\[SessionAuthorize\]"
    $hasPerm = $text -match "\[PermissionAuthorize"
    Write-Output "$($c.Name): SessionAuth=$hasSession, PermAuth=$hasPerm"
}
