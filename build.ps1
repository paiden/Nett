<#
.SYNOPSIS
Executes various advanced Nett build scenarios.

.DESCRIPTION
Without options executes a build that simply builds the Nett Main Solution
with Debug configuration.

Quick-Targets:
  -ngp, -nuGetPackage

  Please note that quick targets are mutually exclusive and only one can
  be selected and determines the parameter set.

Additional flags/params (availability depends on param set):
    -release

Fine grained build configuration: Pass options to MSBuild via
/p:OptionName=SomeValue.

.PARAMETER nuGetPackage
Exclusive Quick Target.
Make a build that after the default builds also creates the final NuGet
package

.PARAMETER nugetPackageVersion
Version the generated NuGet package should have. Required for the
nuGetPackage quick target.

.PARAMETER release
Build with release configuration

#>

[CmdletBinding(DefaultParameterSetName="none", SupportsShouldProcess=$true)]
Param(
    # mutually exclusive quick targets


    [alias("dp")]
    [parameter(ParameterSetName="doc")]
    [switch]$docpub,

    [Alias("ngp")]
    [parameter(ParameterSetName="pack")]
    [switch]$pack,

    [Alias("up")]
    [parameter(ParameterSetName="pack")]
    [switch]$pub,

    # Multi set parameters

    # Set independent parameters
    [ValidateSet("Debug", "Release")]
    [string]$configuration = "Release",
    [switch]$nobuild,

    [parameter(Position=0, ValueFromRemainingArguments=$true, DontShow)] $rest
)


#Helper to support whatif for native invocations
function Invoke-ExpandedChecked {
[CmdletBinding(
    SupportsShouldProcess = $true,
    ConfirmImpact = 'Medium')]
    param([ScriptBlock]$ScriptBlock)

    $expanded = $ExecutionContext.InvokeCommand.ExpandString($ScriptBlock)
    $script = [scriptblock]::Create($expanded)
    if ($PSCmdlet.ShouldProcess($script.ToString(), "Invoke External Command"))
    {
        & $script
    }
}

. ./tags.ps1

$basePath = & .\Infrastructure\vswhere.exe -latest -property installationPath
$msbuild = Join-Path $basePath  ".\MSBuild\15.0\bin\msbuild.exe"
$buildItem = Join-Path -Path $PSScriptRoot -ChildPath Nett.sln

if (!(Test-Path $msbuild)) {
    write-error "ERROR: Could not find MSBuild."
    return 1
}
else {
    $msbuild = "`"$msbuild`"";
}

Invoke-Expression ".\tagger.ps1"

if (-not $nobuild) {
    $msBuildOptions = "/p:Configuration=$configuration", "/m", "/nologo"
    $msBuildOptions += $rest
    Invoke-ExpandedChecked { & $msbuild $buildItem $msBuildOptions }
}

if($docpub) {
    & docfx dfx\docfx.json
    Remove-Item -Path docs\* -Recurse -Force
    Copy-Item -Path .\dfx\_site\* -Destination docs -Recurse -Container -Force
}

if($pub) {
    # Try to validate release notes are up to date before pushing new packages
    $rnf = "RELEASENOTES.md"
    $rn = Get-Content $rnf
    $nrvc = ($rn | Select-String -pattern $NETT_VERSION | Measure-Object).Count
    $crvc = ($rn | Select-String -Pattern $COMA_VERSION | Measure-Object).Count
    $arvc = ($rn | Select-String -Pattern $ASPNETT_VERSION | Measure-Object).Count

    if ($nrvc -le 0) { Write-Host "Failed to find 'Nett' v$NETT_VERSION release notes." -ForegroundColor Red }
    if ($crvc -le 0) { Write-Host "Failed to find 'Nett.Coma' v$COMA_VERSION release notes." -ForegroundColor Red }
    if ($arvc -le 0) { Write-Host "Failed to find 'Nett.AspNet' v$ASPNETT_VERSION release notes." -ForegroundColor Red }

    # Some version number checking
    $np = "Source\Nett\bin\Release\Nett.$NETT_VERSION.nupkg"
    $cp = "Source\Nett.Coma\bin\Release\Nett.Coma.$COMA_VERSION.nupkg"
    $ap = "Source\Nett.AspNet\bin\Release\Nett.AspNet.$ASPNETT_VERSION.nupkg"

    if (-not (Test-Path $np)) { throw "Nuget package $np does not exist on disk. Aborting push." }
    if (-not (Test-Path $cp)) { throw "Nuget package $cp does not exist on disk. Aborting push." }
    if (-not (Test-Path $ap)) { throw "Nuget pakcage $ap does not eixst on disk. Aborting push." }

    Get-ChildItem Source\Nett\bin\$configuration\netstandard2.0\Nett.dll | ForEach-Object { "Nett.dll: $($_.VersionInfo.FileVersion)" } |  Write-Host
    Get-ChildItem Source\Nett.Coma\bin\$configuration\netstandard2.0\Nett.Coma.dll | ForEach-Object { "Nett.Coma.dll: $($_.VersionInfo.FileVersion)" } | Write-Host
    Get-ChildItem Source\Nett.AspNet\bin\$configuration\netstandard2.0\Nett.AspNet.dll | ForEach-Object { "Nett.AspNet.dll: $($_.VersionInfo.FileVersion)" } | Write-Host

    Write-Host $np
    Write-Host $cp
    Write-Host $ap

    $confirm = Read-Host "Please check for errors and correct version numbers. Continue to publish these '$configuration' packages? [y/n]"
    if ($confirm -match "[yY]") {
       Invoke-ExpandedChecked { & nuget.exe push $np -Source 'https://www.nuget.org/api/v2/package' }
       Invoke-ExpandedChecked { & nuget.exe push $cp -Source 'https://www.nuget.org/api/v2/package' }
       Invoke-ExpandedChecked { & nuget.exe push $ap -Source 'https://www.nuget.org/api/v2/package' }
    }
    else {
        Write-Output "Push was aborted by user."
    }
}
