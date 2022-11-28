$wshshell = New-Object -ComObject WScript.Shell

$intemp = Join-Path $env:TEMP tempFile.txt
"Hello temp" | Out-File -FilePath $intemp
