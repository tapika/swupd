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

    [Alias('b')]
    [string]$branch,

    # or newer 'net5.0' / 'net6.0'
    [Alias('net')]
    [string]$netPlatform = 'netcoreapp3.1',

    # Can add operations using simple command line like this: 
    #   build a -add_operations c=true,d=true,e=false -v
    # => 
    #   a c d
    #
    [string] $addoperations = ''
)

$ErrorActionPreference = "Stop"
$scriptDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$isBuildMachine = $env:APPVEYOR -eq 'true' -or $env:GITHUB_ACTIONS -eq 'true'
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
    if(!$operationsToPerform.Contains("nuget"))
    {
      $operationsToPerform = ,"nuget" + $operationsToPerform
    }

    if(!$operationsToPerform.Contains("env"))
    {
      $operationsToPerform = ,"env" + $operationsToPerform
    }
}

if( $operationsToPerform -match "buildexe_?.*" )
{
    if(!$operationsToPerform.Contains("env"))
    {
      $operationsToPerform = ,"env" + $operationsToPerform
    }
}

if( $operationsToPerform.Contains("all") )
{
    $operationsToPerform = @(
        'nuget', 'env', 'build', 
        # Comment for faster testing
        'integration', 

        # 'coverage' runs same tests as 'test1' & 'test2' only with code coverage enabled. 
        # If you comment one out - enable other ones.
        #'test1',
        #'test2',
        'coverage', 

        'coveragehtml',
        'buildexe_choco_win7',
        'buildexe_choco_linux',
        'github_publishrelease'
    )
}

# Testing locally:
# > set GITHUB_REF=refs/heads/develop
# > set GITHUB_RUN_ID=1538394100
#
# > build coverageupload
if(![string]::IsNullOrEmpty($env:GITHUB_RUN_ID))
{
    $env:APPVEYOR_JOB_ID = $env:GITHUB_RUN_ID
    $env:APPVEYOR = 'true'
}

if([string]::IsNullOrEmpty($env:APPVEYOR_REPO_BRANCH) -and ![string]::IsNullOrEmpty($env:GITHUB_REF))
{
    if ($env:GITHUB_REF -match '.*/(.*)') {
        $env:APPVEYOR_REPO_BRANCH = $matches[1]
    }
}

#----------------------------------------------------------
# Determine commit id
#----------------------------------------------------------
$commitId = $env:APPVEYOR_REPO_COMMIT

if([string]::IsNullOrEmpty($commitId))
{
    $commitId = $env:GITHUB_SHA
}

if([string]::IsNullOrEmpty($commitId))
{
    $commitId = git rev-parse HEAD
}

if([string]::IsNullOrEmpty($env:APPVEYOR_REPO_COMMIT)) { $env:APPVEYOR_REPO_COMMIT = $commitId }
if([string]::IsNullOrEmpty($env:GITHUB_SHA)) { $env:GITHUB_SHA = $commitId }


#----------------------------------------------------------
# Determine commit message (used by coverageupload)
#----------------------------------------------------------
$commitMessage = $env:APPVEYOR_REPO_COMMIT_MESSAGE

if([string]::IsNullOrEmpty($commitMessage))
{
    $commitMessage = git log -1 --oneline --pretty=%B
}

if([string]::IsNullOrEmpty($env:APPVEYOR_REPO_COMMIT_MESSAGE))
{
    $env:APPVEYOR_REPO_COMMIT_MESSAGE = $commitMessage
}

#if(![string]::IsNullOrEmpty($env:APPVEYOR_JOB_ID) -and $isBuildMachine )
#{
#    $operationsToPerform = $operationsToPerform + 'testresultupload'
#}

if( $operationsToPerform.Contains("coverage") -and $isBuildMachine )
{
    $operationsToPerform = $operationsToPerform + 'coverageupload'
}

if($verbose)
{
    "Will perform following operations: $operationsToPerform"
}

$buildPlatform = 'net48'
$nunitConsole = [System.IO.Path]::Combine($scriptDir, 'src\packages\nunit.consolerunner\3.13.0\tools\nunit3-console.exe')
$chocolateyTestsDir = [System.IO.Path]::Combine($scriptDir, "src\bin\$buildPlatform-$configuration")
#$chocolateyIntegrationTestsDir = [System.IO.Path]::Combine($scriptDir, "src\chocolatey.tests.integration\bin\$configuration")
$chocolateyIntegrationTestsDir = [System.IO.Path]::Combine($scriptDir, "src\bin\$buildPlatform-$configuration")
$chocolateyTestsDll = [System.IO.Path]::Combine($chocolateyTestsDir, 'chocolatey.tests.dll')
#$chocolateyTests2Dll = [System.IO.Path]::Combine($scriptDir, "src\chocolatey.tests.integration\bin\$configuration\chocolatey.tests.integration.dll")
$chocolateyTests2Dll = [System.IO.Path]::Combine($chocolateyIntegrationTestsDir, 'chocolatey.tests.integration.dll')
$sln = 'src\chocolatey.sln'
$sln2 = "src\chocolatey_$netPlatform.sln"
$coverageOutDir = [System.IO.Path]::Combine($scriptDir, 'build_output\build_artifacts\codecoverage')
$coverageXml = [System.IO.Path]::Combine($coverageOutDir, 'coverage.xml')
$testResultsXmlDir = [System.IO.Path]::Combine($scriptDir, 'build_output\build_artifacts\tests')
$testResultsXml = [System.IO.Path]::Combine($testResultsXmlDir, 'test-results.xml')

$dlls2test = @( 'chocolatey.tests.dll' )

foreach ($operation in $operationsToPerform)
{
    "- $operation"

    $cmdArgs = @( )
    $cmd = 'dotnet'
    $curDir = ""

    if($operation -eq 'integration')
    {
        $dlls2test = $dlls2test + 'chocolatey.tests.integration.dll'
        continue
    }

    if($operation -eq 'nuget')
    {
        download_nuget

        $cmd = $nugetExe
        $cmdArgs = @( 'restore', $sln)
    }

    if($operation -eq 'env')
    {
        foreach ($vsvariant in "Enterprise,Community".split(","))
        {
          if($netPlatform -eq 'net6.0')
          {
              $batch = "C:\Program Files\Microsoft Visual Studio\2022\Preview\Common7\Tools\VsDevCmd.bat"
              if ((test-path $batch) -eq $true)
              {
                 break
              }
          }

          $batch = "C:\Program Files (x86)\Microsoft Visual Studio\2019\$vsvariant\VC\Auxiliary\Build\vcvars64.bat"
          if ((test-path $batch) -eq $true)
          {
             break
          }
        }

        "Importing environment variables from " + [System.IO.Path]::GetFileName($batch)
        $cmd = "`"$batch`""
        cmd /c "$cmd > nul 2>&1 && set" | . { process {
            if ($_ -match '^([^=]+)=(.*)') {
                #"-set env variable: " + $matches[1] + "=" + $matches[2]
                [System.Environment]::SetEnvironmentVariable($matches[1], $matches[2])
            }
        }}
    }

    if($operation -eq 'build')
    {
        [System.Environment]::SetEnvironmentVariable('BuildingFromCli', 'true')
        $cmd = 'cmd.exe'
        $cmdArgs = @( '/c', 'devenv', $sln, '/build', '"Release|AnyCPU"' )

        #"dotnet build" does not work with StrongNamer + net48
        #$cmdArgs = @( 'build', $sln, '-c', $configuration )
        if($rebuild)
        {
            $cmdArgs += '--no-incremental'
        }
    }

    #------------------------------------------------------------------------------------
    # Build ReadyToRun executable, for example:
    # buildexe_<target>_win7     => Windows
    # buildexe_<target>_linux    => Linux
    # buildexe_<target>_osx      => MacOS
    # Full list of runtime identifier catalog is in here:
    #------------------------------------------------------------------------------------
    # .NET Runtime Identifier (RID) Catalog
    #
    # Documentation:
    #   https://docs.microsoft.com/en-us/dotnet/core/rid-catalog
    # Same in git:
    #   https://github.com/jesulink2514/docs-1/blob/master/docs/core/rid-catalog.md
    #------------------------------------------------------------------------------------
    if($operation -match 'buildexe_?(.*)_(.*)')
    {
        $cmd = 'cmd.exe'
        $buildTarget = $matches.1
        $publishPlatform = $matches.2
        $runtimeIdentifier = $publishPlatform + '-x64'
        #$runtimeIdentifier = $publishPlatform + '-arm64'
        $publishDirectory = 'bin\publish_' + $runtimeIdentifier + '_' + $netPlatform

        $publishTrimmed = 'true'
        # If published trimmed does not work for some reason
        if($netPlatform -eq 'net6.0')
        {
            $publishTrimmed = 'false'
        }

        $cmdArgs = @( '/c', 'msbuild', $sln2,
          '/p:DeployOnBuild=true',
          "/p:Configuration=$configuration",
          '/p:Platform="Any CPU"',
          '/t:restore;build;publish',
          ('/p:PublishDir=' + $publishDirectory + '\'),
          '/p:PublishProtocol=FileSystem',
          # For example:
          #'/p:RuntimeIdentifier=win7-x64',
          #'/p:RuntimeIdentifier=linux-x64',
          ('/p:RuntimeIdentifier=' + $runtimeIdentifier),
          '/p:SelfContained=true',
          '/p:PublishSingleFile=true',
          '/p:PublishReadyToRun=false',
          # works only in .net 6.0, older platforms ignore this parameter
          '/p:EnableCompressionInSingleFile=true',
          '/p:IncludeNativeLibrariesForSelfExtract=true',
          '/p:IncludeAllContentForSelfExtract=true',
          ('/p:PublishTrimmed=' + $publishTrimmed),
          #'buildexe_choco_win7' => 'choco' => 'PUBLISH_CHOCO'
          ('/p:PUBLISH_' + $buildTarget.ToUpper() + '=true')
          #'/p:UseAppHost=true'
        )
    }

    if($operation -eq 'test1')
    {
        $cmd = $nunitConsole
        $dll = $chocolateyTestsDll
        $outDir = [System.IO.Path]::Combine($scriptDir, 'build_output\build_artifacts\tests')
        $out = [System.IO.Path]::Combine($outDir, 'test-results.xml')
        
        if ((test-path $outDir) -eq $false) { New-Item -Type Directory -Path $outDir | out-null }

        $cmdArgs = @( $dll, "/result=$out", '--workers=1', '--agents=1')
    }

    if($operation -eq 'test2')
    {
        $cmd = $nunitConsole
        $dll = $chocolateyTests2Dll
        $outDir = [System.IO.Path]::Combine($scriptDir, 'build_output\build_artifacts\tests')
        $out = [System.IO.Path]::Combine($outDir, 'test-results-2.xml')
        
        if ((test-path $outDir) -eq $false) { New-Item -Type Directory -Path $outDir | out-null }

        $cmdArgs = @( $dll, "/result=$out", '--workers=1', '--agents=1' )
        $curDir = $chocolateyIntegrationTestsDir
    }

    if($operation -eq 'coverage')
    {
        if ((test-path $testResultsXmlDir) -eq $false) { New-Item -Type Directory -Path $testResultsXmlDir | out-null }
        $cmd = [System.IO.Path]::Combine($scriptDir, 'src\packages\opencover\4.7.1221\tools\OpenCover.Console.exe')

        $dlls = ($dlls2test -join " ")
        $cmdArgs = @( "-target:""$nunitConsole""", "-targetdir:""$chocolateyIntegrationTestsDir"" ", "-targetargs:"" $dlls /result=$testResultsXml --workers=1 --agents=1 """)
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
        
        $curDir = $chocolateyIntegrationTestsDir
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
        if($dontexecute)
        {
            "Test results upload skipped - --noop option"
            continue
        }

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
    
        $cmd = [System.IO.Path]::Combine($scriptDir, 'src\packages\coveralls.io.1.1.86\tools\coveralls.net.exe')
        $cmdArgs = @( '--opencover', """$coverageXml""")
    }

    if($operation -eq 'github_publishrelease')
    {
        $privateApiKeyPath = 'c:\Private\github_apikey.txt'
        if ((test-path $privateApiKeyPath) -eq $true)
        {
            $gitHubApiKey = Get-Content $privateApiKeyPath
        }

        if([string]::IsNullOrEmpty($gitHubApiKey))
        {
            $gitHubApiKey = $env:GITHUB_APIKEY
        }
        
        if([string]::IsNullOrEmpty($gitHubApiKey))
        {
            $gitHubApiKey = $env:API_KEY
        }

        if([string]::IsNullOrEmpty($gitHubApiKey))
        {
            "GITHUB_APIKEY/API_KEY environment not set, and not saved in $privateApiKeyPath"
            exit 2
        }
        
        $versionNumber = '1.0.0'
        #$versionNumber = git describe --abbrev=0
        $commitId = $env:APPVEYOR_REPO_COMMIT

        if([string]::IsNullOrEmpty($commitId))
        {
            $commitId = git rev-parse HEAD
        }

        $releaseNotes = $env:APPVEYOR_REPO_COMMIT_MESSAGE

        if([string]::IsNullOrEmpty($releaseNotes))
        {
            $releaseNotes = git log -1 --oneline --pretty=%B
        }

        $branchName = $env:APPVEYOR_REPO_BRANCH
        if([string]::IsNullOrEmpty($branchName))
        {
            $branchName = git branch --show-current
        }

        if($branchName -eq 'master')
        {
            $preRelease = $FALSE
        }
        else
        {
            $preRelease = $TRUE
        }

        "Creating github release:"
        "Release tag name: $versionNumber"
        "Commit id: $commitId"
        "Release notes: '$releaseNotes'"
        "Branch name: $branchName"

        if($dontexecute)
        {
            continue
        }

        # See also https://blog.peterritchie.com/Resetting-Build-Number-in-Appveyor/

        $releaseData = @{
            tag_name = $versionNumber;
            target_commitish = $commitId;
            name = $versionNumber;
            body = "$releaseNotes";
            # Set to true to mark this as a draft release (not visible to users)
            draft = $FALSE;
            prerelease = $preRelease;
         }

        $authHeaders = @{ Authorization = 'Basic ' + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes($gitHubApiKey + ":x-oauth-basic")); } 
        $releaseParams = @{
            Uri = "https://api.github.com/repos/tapika/swupd/releases";
            Method = 'POST';
            Headers = $authHeaders;
            ContentType = 'application/json';
            Body = (ConvertTo-Json $releaseData -Compress)
        }

        $canRetry = $true

        while($true)
        {
            try{
              $result = Invoke-RestMethod @releaseParams
              break
            }catch
            {
               try{ $jsonObj = ConvertFrom-Json $_ } catch {}
               if($jsonObj -ne $null -and $jsonObj.errors.code -ne 'already_exists')
               {
                   $_
                   exit 3
               }

               if(!$canRetry) { break }
               $canRetry = $false
        
               "Release already exists, deleting one..."
               # https://docs.github.com/en/rest/reference/repos#get-a-release-by-tag-name
               # If already exists => get asset url
               $req = @{ Uri = "https://api.github.com/repos/tapika/swupd/releases/tags/$versionNumber"; Method = 'GET'; Headers = $authHeaders; } 
               $result = Invoke-RestMethod @req
               $url = $result.url

               # and delete it
               #https://docs.github.com/en/rest/reference/repos#delete-a-release-asset
               $req = @{ Uri = $url; Method = 'DELETE'; Headers = $authHeaders; } 
               $result = Invoke-RestMethod @req
            }
        }
        
        $uploadUri = $result | Select -ExpandProperty upload_url
        

        $platforms = @("win7", "linux")
        foreach ($publishPlatform in $platforms)
        {
            $runtimeIdentifier = $publishPlatform + '-x64'
            $publishDirectory = 'bin\publish_' + $runtimeIdentifier + '_' + $netPlatform
            $executableName = 'choco'

            if($publishPlatform -eq 'win7')
            {
                $executableName = 'choco.exe'
            }

            $uploadFilePath = [System.IO.Path]::Combine($scriptDir, 'src\chocolatey.console', $publishDirectory, $executableName)

            $uploadFilename = [System.IO.Path]::GetFileName($uploadFilePath)
            $uploadFileUri = $uploadUri -replace '\{\?name.*\}', "?name=$uploadFilename"

            "Uploading file:"
            " - " + $uploadFilePath
            "to: $uploadFileUri"

            $uploadParams = @{
                Uri = $uploadFileUri;
                Method = 'POST';
                Headers = $authHeaders;
                ContentType = 'application/octet-stream';
                InFile = $uploadFilePath
            }

            $result = Invoke-RestMethod @uploadParams
        }
    }

    if($cmdArgs.Count -ne 0)
    {
        if($curDir -ne "")
        {
            "> cd $curDir"
            Set-Location -Path $curDir
        }

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
        
        Set-Location -Path $scriptDir
    }
}

