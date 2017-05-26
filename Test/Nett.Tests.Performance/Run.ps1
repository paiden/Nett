[CmdletBinding()]
Param(
    [Parameter(ParameterSetName='Clean')]
    [switch]$clean,
    [Parameter(ParameterSetName='Clean')]
    [switch]$publish
)

$configuration = 'Release'
$pkgbase = Join-Path $PSScriptRoot ..\..\Solutions\Nett\packages
$prjDir = $PSScriptRoot
$runner = (Get-ChildItem "$pkgbase\*\NBench.Runner.exe" -Recurse).FullName
$asm = Join-Path $prjDir "bin\$configuration\Nett.PerfTests.dll"
$perfres = Join-Path $prjDir PerfTestResults
$report = Join-Path $prjDir Results.md

if($clean) {
    Remove-Item $perfres -Recurse -Force -ErrorAction Ignore
}

& $runner $asm output-directory=$perfres

if($publish) {
    Clear-Content $report
    $files = Get-ChildItem -Path $perfres -Filter *.md
    # Trim to get rid of leading whitespaces that break pipe tables in VS markdown addin
    $content = Get-Content $files.FullName
    ForEach($l in $content) {
        Add-Content $report $l.TrimStart()
    }
}