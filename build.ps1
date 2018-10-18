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

    [Alias("ngp")]
    [parameter(ParameterSetName="pack")]
    [string] $pack,

    [Alias("up")]
    [parameter(ParameterSetName="pack")]
    [switch]$push,

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

$basePath = & .\Infrastructure\vswhere.exe -latest -property installationPath
$msbuild = Join-Path $basePath  ".\MSBuild\15.0\bin\msbuild.exe"
$buildItem = Join-Path -Path $PSScriptRoot -ChildPath Solutions\Nett\Nett.sln

if (!(Test-Path $msbuild)) {
    write-error "ERROR: Could not find MSBuild."
    return 1
}
else {
    $msbuild = "`"$msbuild`"";
}

if(-not $nobuild) {
    $msBuildOptions = "/p:Configuration=$configuration", "/m", "/nologo"
    $msBuildOptions += $rest
    Invoke-ExpandedChecked { & $msbuild $buildItem $msBuildOptions }
}

if($pack) {
    $v = $pack.ToString()
    $nuspecNett = "`"$(Join-Path -Path $PSScriptRoot -ChildPath Nett.nuspec)`""
    $nuspecComa = "`"$(Join-Path -Path $PSScriptRoot -ChildPath Coma.nuspec)`""
    $aspNettComa = "`"$(Join-Path -Path $PSScriptRoot -ChildPath Nett.AspNet.nuspec)`""
    $props = "`"configuration=$configuration`""

    if($configuration -eq "Debug") { $v += '-debug' }

    New-Item -ItemType Directory -Path ngp -Force
    Invoke-ExpandedChecked { & nuget.exe pack -symbols $nuspecNett -Version $v -Properties $props -OutputDirectory ngp}
    Invoke-ExpandedChecked { & nuget.exe pack -symbols $nuspecComa -Version $v -Properties $props -OutputDirectory ngp}
    Invoke-ExpandedChecked { & nuget.exe pack -symbols $aspNettComa -Version $v -Properties $props -OutputDirectory ngp}
}

if($push) {
    Invoke-ExpandedChecked { & nuget.exe push ngp\Nett.$v.nupkg -Source 'https://www.nuget.org/api/v2/package' }
    Invoke-ExpandedChecked { & nuget.exe push ngp\Nett.Coma.$v.nupkg -Source 'https://www.nuget.org/api/v2/package' }
    Invoke-ExpandedChecked { & nuget.exe push ngp\Nett.AspNet.$v.nupkg -Source 'https://www.nuget.org/api/v2/package' }
}
