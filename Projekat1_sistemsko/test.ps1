#skripta za testiranje stampeda

$url1 = "http://localhost:8080/Nis"
$url2 = "http://localhost:8080/Belgrade"
$url3 = "http://localhost:8080/Novi%20Sad"

Write-Host "Salju se zahtevi" -ForegroundColor Cyan

    1..100 | ForEach-Object {
        Invoke-WebRequest -Uri $url1 -UseBasicParsing | Out-Null
        Invoke-WebRequest -Uri $url2 -UseBasicParsing | Out-Null
        Invoke-WebRequest -Uri $url3 -UseBasicParsing | Out-Null
    }
    Write-Host "Poslato 300"
    Start-Sleep -Seconds 5