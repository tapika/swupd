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
$isBuildMachine = $env:APPVEYOR -eq 'true'
if( $isBuildMachine )
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

$packageOutputFolder = [System.IO.Path]::Combine($scriptDir, 'build_output')

if( $operationsToPerform.Contains("build") )
{
    $operationsToPerform = ,"nuget" + $operationsToPerform
}

if( $operationsToPerform.Contains("all") )
{
    $operationsToPerform = @('nuget', 'build', 'coverage', 'coveragehtml')
}

if( $operationsToPerform.Contains("coverage") -and $isBuildMachine )
{
    $operationsToPerform = $operationsToPerform + 'testresultupload', 'coverageupload'
}

if($verbose)
{
    "Will perform following operations: $operationsToPerform"
}

$nunitConsole = [System.IO.Path]::Combine($scriptDir, 'src\packages\NUnit.Runners.2.6.4\tools\nunit-console.exe')
$chocolateyTestsDir = [System.IO.Path]::Combine($scriptDir, "src\chocolatey.tests\bin\$configuration")
$chocolateyTestsDll = [System.IO.Path]::Combine($chocolateyTestsDir, 'chocolatey.tests.dll')
$chocolateyTests2Dll = [System.IO.Path]::Combine($scriptDir, "src\chocolatey.tests.integration\bin\$configuration\chocolatey.tests.integration.dll")
$sln = 'src\chocolatey.sln'
$coverageOutDir = [System.IO.Path]::Combine($scriptDir, 'build_output\build_artifacts\codecoverage')
$coverageXml = [System.IO.Path]::Combine($coverageOutDir, 'coverage.xml')
$testResultsXmlDir = [System.IO.Path]::Combine($scriptDir, 'build_output\build_artifacts\tests')
$testResultsXml = [System.IO.Path]::Combine($testResultsXmlDir, 'test-results.xml')

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

    if($operation -eq 'test1')
    {
        $cmd = $nunitConsole
        $dll = $chocolateyTestsDll
        $outDir = [System.IO.Path]::Combine($scriptDir, 'build_output\build_artifacts\tests')
        $out = [System.IO.Path]::Combine($outDir, 'test-results.xml')
        
        if ((test-path $outDir) -eq $false) { New-Item -Type Directory -Path $outDir | out-null }

        $cmdArgs = @( $dll, "/xml=$out", '/nologo', '/framework=net-4.0', '/exclude="Database,Integration,Slow,NotWorking,Ignore,database,integration,slow,notworking,ignore"' )
    }

    if($operation -eq 'coverage')
    {
        if ((test-path $testResultsXmlDir) -eq $false) { New-Item -Type Directory -Path $testResultsXmlDir | out-null }
        $cmd = [System.IO.Path]::Combine($scriptDir, 'lib\OpenCover\OpenCover.Console.exe')
        $cmdArgs = @( "-target:""$nunitConsole""", "-targetdir:""$chocolateyTestsDir"" ", "-targetargs:"" chocolatey.tests.dll /xml=$testResultsXml """)
        $filters = '+[chocolatey*]*'
        $filters = "$filters -[chocolatey*test*]*"
        $filters = "$filters -[chocolatey]*adapters.*"
        $filters = "$filters -[chocolatey]*infrastructure.app.configuration.*Setting*"
        $filters = "$filters -[chocolatey]*app.configuration.*Configuration"
        $filters = "$filters -[chocolatey]*app.domain.*"
        $filters = "$filters -[chocolatey]*app.messages.*"
        $filters = "$filters -[chocolatey]*.registration.*"
        $filters = "$filters -[chocolatey]*app.templates.*"
        $filters = "$filters -[chocolatey]*commandline.Option*"
        $filters = "$filters -[chocolatey]*licensing.*"
        $filters = "$filters -[chocolatey]*infrastructure.results.*"
        $cmdArgs = $cmdArgs + "-filter:""$filters"""
        if ((test-path $coverageOutDir) -eq $false) { New-Item -Type Directory -Path $coverageOutDir | out-null }
        $cmdArgs = $cmdArgs + "-output:""$coverageXml"""
        $cmdArgs = $cmdArgs + '-log:All'
        $cmdArgs = $cmdArgs + '-skipautoprops'
        $cmdArgs = $cmdArgs + '-register:administrator'
    }

    if($operation -eq 'coveragehtml')
    {
        $cmd = [System.IO.Path]::Combine($scriptDir, 'lib\ReportGenerator\ReportGenerator.exe')
        $outDir = [System.IO.Path]::Combine($scriptDir, 'build_output\build_artifacts\codecoverage\Html')
        if ((test-path $outDir) -eq $false) { New-Item -Type Directory -Path $outDir | out-null }
        $cmdArgs = @( """$coverageXml""",  """$outDir""", 'HtmlSummary')
    }

    if($operation -eq 'testresultupload')
    {
        $wc = New-Object 'System.Net.WebClient'
        $wc.UploadFile("https://ci.appveyor.com/api/testresults/nunit/$($env:APPVEYOR_JOB_ID)", (Resolve-Path $testResultsXml) )
        continue
    }

    if($operation -eq 'coverageupload')
    {
        if($env:COVERALLS_REPO_TOKEN -eq $null)
        {
            "COVERALLS_REPO_TOKEN environment variable not set, not uploading coverage report"
            continue
        }
    
        #$cmd = [System.IO.Path]::Combine($scriptDir, 'src\packages\coveralls.io.1.4.2\tools\coveralls.net.exe')
        $cmd = [System.IO.Path]::Combine($scriptDir, 'src\packages\coveralls.io.1.1.86\tools\coveralls.net.exe')
        #$cmdArgs = @( '--opencover', """$coverageXml""", '-r', $env:COVERALLS_REPO_TOKEN)
        $cmdArgs = @( '--opencover', """$coverageXml""")
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

