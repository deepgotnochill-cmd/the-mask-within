param(
    [Parameter(Mandatory)][string]$BaseZip,
    [Parameter(Mandatory)][string]$OutputDirectory,
    [string]$DotNet = "dotnet"
)
$ErrorActionPreference = "Stop"
$expected = "132ecc0ad2c84387c6a9c35e4fe225b30353493b5d6313f1756d84134a1c23f6"
if ((Get-FileHash -LiteralPath $BaseZip -Algorithm SHA256).Hash -ne $expected) {
    throw "Expected the original The.Mask.Within.zip from release v1.0.0."
}
$target = [IO.Path]::GetFullPath($OutputDirectory)
if ((Test-Path -LiteralPath $target) -and (Get-ChildItem -LiteralPath $target -Force | Select-Object -First 1)) {
    throw "Choose an empty output folder."
}
$sdks = @(& $DotNet --list-sdks)
if (!$sdks -or $sdks[-1] -notmatch '^([0-9.\w-]+) \[(.+)\]$') {
    throw "Install a .NET SDK or pass its dotnet.exe using -DotNet."
}
$compiler = Join-Path $Matches[2] "$($Matches[1])/Roslyn/bincore/csc.dll"
$source = Join-Path $PSScriptRoot '../Gameplay'
New-Item -ItemType Directory -Force $target | Out-Null
Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [IO.Compression.ZipFile]::OpenRead((Resolve-Path -LiteralPath $BaseZip).Path)
try {
    foreach ($entry in $archive.Entries) {
        $path = $entry.FullName -replace '^the mask within \(school fair project\)/',''
        if (!$entry.Name -or $path -notmatch '^(MonoBleedingEdge/|The Mask Within_Data/|The Mask Within\.exe$|UnityCrashHandler64\.exe$|UnityPlayer\.dll$)') { continue }
        $file = Join-Path $target $path
        [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($file)) | Out-Null
        [IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $file)
    }
} finally {
    $archive.Dispose()
}
$managed = Join-Path $target 'The Mask Within_Data/Managed'
$refs = Get-ChildItem -LiteralPath $managed -Filter '*.dll' | Where-Object Name -ne 'Assembly-CSharp.dll' | ForEach-Object { "-reference:$($_.FullName)" }
$files = Get-ChildItem -LiteralPath $source -Filter '*.cs' | Sort-Object Name | ForEach-Object FullName
& $DotNet $compiler /nologo /nostdlib+ /target:library /optimize+ /deterministic+ "-out:$managed/Assembly-CSharp.dll" @refs @files
if ($LASTEXITCODE -ne 0) { throw "Gameplay compilation failed." }
Write-Output "Built $target/The Mask Within.exe"
