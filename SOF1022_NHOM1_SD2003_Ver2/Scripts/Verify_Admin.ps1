$OutputEncoding = [Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$cookieJar = New-Object System.Net.CookieContainer

# 1. GET Login to get token
$getReq = [System.Net.HttpWebRequest]::Create("http://localhost:5000/Account/Login")
$getReq.CookieContainer = $cookieJar
$getRes = $getReq.GetResponse()
$sr = New-Object System.IO.StreamReader($getRes.GetResponseStream(), [System.Text.Encoding]::UTF8)
$loginHtml = $sr.ReadToEnd()
$getRes.Close()

$token = ""
if ($loginHtml -match '__RequestVerificationToken"[^>]*value="([^"]+)"') {
    $token = $matches[1]
}

# 2. POST Login with Token
$loginUrl = "http://localhost:5000/Account/Login"
$body = "Email=" + [System.Uri]::EscapeDataString("admin@library.com") + "&Password=" + [System.Uri]::EscapeDataString("Admin@123") + "&__RequestVerificationToken=" + [System.Uri]::EscapeDataString($token)
$postData = [System.Text.Encoding]::UTF8.GetBytes($body)

$postReq = [System.Net.HttpWebRequest]::Create($loginUrl)
$postReq.Method = "POST"
$postReq.ContentType = "application/x-www-form-urlencoded"
$postReq.CookieContainer = $cookieJar
$postReq.AllowAutoRedirect = $true
$stream = $postReq.GetRequestStream()
$stream.Write($postData, 0, $postData.Length)
$stream.Close()

$postRes = $postReq.GetResponse()
$sr2 = New-Object System.IO.StreamReader($postRes.GetResponseStream(), [System.Text.Encoding]::UTF8)
$afterLoginHtml = $sr2.ReadToEnd()
$postRes.Close()

# 3. Request Readers Index
$req = [System.Net.HttpWebRequest]::Create("http://localhost:5000/Readers")
$req.CookieContainer = $cookieJar
$req.AllowAutoRedirect = $true
$res = $req.GetResponse()
$sr = New-Object System.IO.StreamReader($res.GetResponseStream(), [System.Text.Encoding]::UTF8)
$readersHtml = $sr.ReadToEnd()
$res.Close()

# Find row containing sinhvien02
$idx = $readersHtml.IndexOf("sinhvien02@fpt.edu.vn")
if ($idx -ge 0) {
    $start = [Math]::Max(0, $idx - 300)
    $len = [Math]::Min($readersHtml.Length - $start, 600)
    Write-Host "Snippet around sinhvien02@fpt.edu.vn:"
    Write-Host $readersHtml.Substring($start, $len)
} else {
    Write-Host "sinhvien02@fpt.edu.vn NOT FOUND in /Readers"
}

# Check SystemSettings in sidebar
$idxSettings = $readersHtml.IndexOf("SystemSettings")
if ($idxSettings -ge 0) {
    Write-Host "SystemSettings LINK FOUND in /Readers HTML!"
} else {
    Write-Host "SystemSettings LINK NOT FOUND"
}
