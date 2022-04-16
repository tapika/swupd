$wshshell = New-Object -ComObject WScript.Shell

$startMenu = $([System.Environment]::GetFolderPath([System.Environment+SpecialFolder]::CommonStartMenu))
$shortcutPath = "$startMenu\Programs\msgbox.lnk"

$exePath = "$packageFolder\msgbox.exe"

if (!(Test-Path($exePath)))
{
  # powershell script executed locally
  $scriptDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
  $exePath = "$scriptDir\..\msgbox.exe"
}

$lnk = $wshshell.CreateShortcut($shortcutPath)
$lnk.TargetPath = $exePath
$lnk.Save()

