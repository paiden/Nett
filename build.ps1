<#
.SYNOPSIS
Executes the Ranorex Dex build.

.DESCRIPTION
Without options executes a build that simply builds the Ranorex.Dex Solution
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

[CmdletBinding(DefaultParameterSetName="none")]
Param(
    # mutually exclusive quick targets

    [Alias("ngp")]
    [parameter(ParameterSetName="NuGetPackage")]
    [switch] $NuGetPackage,

    [Alias("npv")]
    [parameter(ParameterSetName="NuGetPackage", Mandatory=$true)]
    [Version]$nugetPackageVersion,

    # Multi set parameters

    # Set independent parameters
    [switch]$release,
    [switch]$disableStdBuild,


    [parameter(Position=0, ValueFromRemainingArguments=$true, DontShow)] $rest
)

$configuration = if($release) { "Debug" } else { "Release" }

$msbuild = Join-Path ${env:ProgramFiles(x86)} ".\MSBuild\14.0\bin\msbuild.exe"
$buildItem = Join-Path -Path $PSScriptRoot -ChildPath Solutions\Nett\Nett.sln

if (!(Test-Path $msbuild)) {
    write-error "ERROR: Could not find MSBuild."
    return 1
}

if(-not $disableStdBuild) {
    $msBuildOptions = "/p:Configuration=$configuration", "/m", "/nologo"
    $msBuildOptions += $rest
    & $msbuild $buildItem $msBuildOptions
}

if($NuGetPackage) {

    $nuget = Join-Path -Path $PSScriptRoot -ChildPath Tools\NuGet.exe
    $nuspec = Join-Path -Path $PSScriptRoot -ChildPath Nett.nuspec

    # Build second time with DoMerge set to create merged libs
    & $msbuild $buildItem $msBuildOptions
    & $nuget pack $nuspec -Version $nugetPackageVersion.ToString() -Properties configuration=$configuration
}

