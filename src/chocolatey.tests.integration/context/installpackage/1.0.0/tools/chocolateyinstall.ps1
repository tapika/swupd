$toolsDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
"simple file" | Out-File "$toolsDir\simplefile.txt" -force

Write-Output "This is $packageName v$packageVersion being installed"
Write-Host "Ya!"
Write-Debug "A debug message"
Write-Verbose "Yo!"
Write-Warning "A warning!"

Write-Output "$packageName v$packageVersion has been installed"