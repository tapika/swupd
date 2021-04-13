param ( 
    # operations to execute, like 'build', 'pack', 'push'...
    [string]$operations,

    [Alias('c')]
    $configuration = 'Release',

    [Alias('clean')]
    [switch]$rebuild=$false,

    [Alias('noop')]
    [switch]$dontexecute=$false,

    [Alias('v')]
    [switch]$verbose=$false,

    [Alias('servers')]
    $serversConf,

    [Alias('b')]
    [string]$branch,

    # Can add operations using simple command line like this: 
    #   build a -add_operations c=true,d=true,e=false -v
    # => 
    #   a c d
    #
    [string] $addoperations = ''
)

$ErrorActionPreference = "Stop"
$scriptDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
if( $env:APPVEYOR -eq 'true' )
{
    $verbose = $true
}

if($operations -eq '')
{
    $operations = "build";
}

$nugetExe = [System.Environment]::ExpandEnvironmentVariables("%USERPROFILE%\.nuget\cli\5.8.0\nuget.exe")

function download_nuget
{
    $cliVersionPath = split-path $nugetExe -Parent
    if ((test-path $cliVersionPath) -eq $false) {
        New-Item -Type Directory -Path $cliVersionPath | out-null
    }
    # Determine nuget version from path
    $nugetVersion = [System.IO.Path]::GetFileName($cliVersionPath)

    if ((test-path $nugetExe) -eq $false) {
        Write-Host ("Downloading NuGet version " + $nugetVersion)
            [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
            Invoke-WebRequest ("https://dist.nuget.org/win-x86-commandline/v" + $nugetVersion + "/nuget.exe") -OutFile $nugetExe
    }
}

# Determine what needs to be done
$operationsToPerform = $operations.Split(";,")

foreach ($operationToAdd in $addoperations.Split(";,"))
{
    if($operationToAdd.Length -eq 0)
    {
        continue
    }

    $keyValue = $operationToAdd.Split("=")

    if($keyValue.Length -ne 2)
    {
        "Ignoring command line parameter '$operationToAdd'"
        continue
    }

    if([System.Convert]::ToBoolean($keyValue[1]))
    {
        $operationsToPerform = $operationsToPerform + $keyValue[0];
    }
}

if($verbose)
{
    "Will perform following operations: $operationsToPerform"
}

$packageOutputFolder = [System.IO.Path]::Combine($scriptDir, 'build_output')

if( $operationsToPerform.Contains("build") )
{
    $operationsToPerform = ,"nuget" + $operationsToPerform
}

$sln = 'src\chocolatey.sln'
foreach ($operation in $operationsToPerform)
{
    "- $operation"

    $cmdArgs = @( )
    $cmd = 'dotnet'

    if($operation -eq 'nuget')
    {
        download_nuget

        $cmd = $nugetExe
        $cmdArgs = @( 'restore', $sln)
    }

    if($operation -eq 'build')
    {
        $cmdArgs = @( 'build', $sln, '-c', $configuration )
        if($rebuild)
        {
            $cmdArgs += '--no-incremental'
        }
    }

    if($operation -eq 'pack')
    {
    }

    if($cmdArgs.Count -ne 0)
    {
        "> $cmd $cmdArgs"

        if($dontexecute -ne $true)
        {
            & "$cmd" $cmdArgs

            if ($LASTEXITCODE -ne 0)
            {
                "Command failed: Exit code: $LASTEXITCODE ('$cmd $cmdArgs')"
                exit $LASTEXITCODE
            }
        }
    }
}

