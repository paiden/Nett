
$templext = ".ttempl"
$tagsep = "$";

function Get-Version([string]$ver) {
   $index = $ver.IndexOf("-");
   $parse = if($index -le 0) { $ver } else { $ver.Substring(0, $index) }
   $pv = [Version]::Parse($parse)
   return [Version]::Parse("$($pv.ToString(3)).0") # ensure version contains a revision of 0
}

function Add-Versions([hashtable]$dst, [string]$key, [string]$setver) {
    [Version]$ver = Get-Version $setver
    $dst.Add($key, "$setver");
    $dst.Add("$($key)_SEM", "$setver+$githash")
    $dst.Add("$($key)_3D", $ver.ToString(3))
    $dst.Add("$($key)_4D", $ver.ToString(4))
    $dst.Add("$($key)_2DXX", "$($ver.ToString(2)).0.0")
}

function New-TargetFile([hashtable]$t) {
    Process {
        $template = $_.Path
        $content = Get-Content $template -Raw

        foreach($e in $t.GetEnumerator()) {
            $content = $content.Replace("$tagsep$($e.Name)$tagsep", $e.Value)
        }

        $tgt = $template.Replace($templext, "")
        if((Test-Path $tgt) -and ((Get-Content $tgt -Raw).Trim()) -eq ($content.Trim())) {
            return
        }

        Set-Content $tgt $content
   }
}

. ./tags.ps1 # Tags.ps1 defines tags and version numbers

$githash = &git rev-parse --short HEAD

Add-Versions $tags "NETT_VERSION" $NETT_VERSION
Add-Versions $tags "NETTEXP_VERSION" $NETTEXP_VERSION
Add-Versions $tags "COMA_VERSION" $COMA_VERSION
Add-Versions $tags "ASPNETT_VERSION" $ASPNETT_VERSION

Resolve-Path "*$templext" | New-TargetFile $tags

# foreach($e in $tags.GetEnumerator()) { Write-Output "$($e.Name): $($e.Value)" }

