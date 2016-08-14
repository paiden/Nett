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
    [parameter(ParameterSetName="NuGetPackage")]
    [switch] $NuGetPackage,

    [Alias("npv")]
    [parameter(ParameterSetName="NuGetPackage", Mandatory=$true)]
    [Version]$nugetPackageVersion,

    [parameter(ParameterSetName="NuGetPackage")]
    [switch]$strongName,

    # Multi set parameters

    # Set independent parameters
    [ValidateSet("Debug", "Release")]
    [string]$configuration = "Debug",
    [switch]$disableStdBuild,


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

$msbuild = Join-Path ${env:ProgramFiles(x86)} ".\MSBuild\14.0\bin\msbuild.exe"
$buildItem = Join-Path -Path $PSScriptRoot -ChildPath Solutions\Nett\Nett.sln

if (!(Test-Path $msbuild)) {
    write-error "ERROR: Could not find MSBuild."
    return 1
}
else {
    $msbuild = "`"$msbuild`"";
}

if(-not $disableStdBuild) {
    $msBuildOptions = "/p:Configuration=$configuration", "/m", "/nologo"
    $msBuildOptions += $rest
    Invoke-ExpandedChecked { & $msbuild $buildItem $msBuildOptions }
}

if($NuGetPackage) {
    $packagesDir = Join-Path $PSScriptRoot -ChildPath .\Solutions\nett\packages
    $snTool = (Get-ChildItem "$packagesDir\Brutal.Dev.StrongNameSigner*\**\StrongNameSigner.Console.Exe")[0].FullName
    $src = "$(Join-Path $PSScriptRoot -ChildPath "Source\Nett\bin\$configuration\Nett.dll")"
    $dst = "$(Join-Path $PSScriptRoot -ChildPath "Source\Nett\bin\$configuration.StrongNamed\Nett.dll")"
    $sne=""

    if($strongName) {
        $sne = '.StrongNamed'
        New-Item -ItemType File -Path $dst -Force
        Copy-Item $src -Destination $dst
        $qdst = "`"$dst`""
        $devsnk = "`"$env:DEVSNK`""
        Invoke-ExpandedChecked { & $snTool -AssemblyFile $qdst -KeyFile $devsnk }
    }

    $v = $nugetPackageVersion.ToString()
    $nuspec = "`"$(Join-Path -Path $PSScriptRoot -ChildPath Nett.nuspec)`""
    $props = "`"$configuration;SNE=$sne`""
    if($configuration -eq "Debug") { $v += '-debug' }

    New-Item -ItemType Directory -Path ngp -Force
    Invoke-ExpandedChecked { & nuget.exe pack -symbols $nuspec -Version $v -Properties configuration=$props -OutputDirectory ngp}
}

