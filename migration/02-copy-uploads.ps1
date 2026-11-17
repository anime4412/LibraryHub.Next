param(
    [Parameter(Mandatory=$true)][string]$Source,
    [Parameter(Mandatory=$true)][string]$Destination,
    [switch]$WhatIf
)

$ErrorActionPreference = 'Stop'
$sourcePath = [System.IO.Path]::GetFullPath($Source)
$destinationPath = [System.IO.Path]::GetFullPath($Destination)
if (-not (Test-Path -LiteralPath $sourcePath -PathType Container)) {
    throw "Không tìm thấy thư mục nguồn: $sourcePath"
}
if ($sourcePath -eq $destinationPath) { throw 'Nguồn và đích không được trùng nhau.' }

$allowed = @('.jpg','.jpeg','.png','.gif','.webp','.mp4')
$files = Get-ChildItem -LiteralPath $sourcePath -Recurse -File |
    Where-Object { $allowed -contains $_.Extension.ToLowerInvariant() }

Write-Host "Nguồn: $sourcePath"
Write-Host "Đích:  $destinationPath"
Write-Host "Số tệp hợp lệ: $($files.Count)"

foreach ($file in $files) {
    $relative = [System.IO.Path]::GetRelativePath($sourcePath, $file.FullName)
    $target = Join-Path $destinationPath $relative
    $targetDirectory = Split-Path -Parent $target
    if ($WhatIf) {
        Write-Host "[WhatIf] $relative"
        continue
    }
    if (-not (Test-Path -LiteralPath $targetDirectory)) {
        New-Item -ItemType Directory -Path $targetDirectory | Out-Null
    }
    if (Test-Path -LiteralPath $target) {
        $sourceHash = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
        $targetHash = (Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash
        if ($sourceHash -ne $targetHash) {
            throw "Tệp đích đã tồn tại nhưng khác nội dung: $relative"
        }
        continue
    }
    Copy-Item -LiteralPath $file.FullName -Destination $target
    $copiedHash = (Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash
    $sourceHash = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
    if ($copiedHash -ne $sourceHash) { throw "Checksum không khớp: $relative" }
}

Write-Host 'Hoàn tất. Không có tệp nào bị ghi đè.'
