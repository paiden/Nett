$configuration = 'Release'
$pkgbase = Join-Path $PSScriptRoot ..\..\Solutions\Nett\packages
$prjDir = $PSScriptRoot
$runner = (Get-ChildItem "$pkgbase\*\NBench.Runner.exe" -Recurse).FullName
$asm = Join-Path $prjDir "bin\$configuration\Nett.PerfTests.dll"
$perfres = Join-Path $prjDir PerfTestResults

& $runner $asm output-directory=$perfres