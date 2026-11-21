param(
    [Parameter(Mandatory)][string]$BaseZip,
    [Parameter(Mandatory)][string]$OutputDirectory,
    [string]$DotNet='dotnet'
)
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath($OutputDirectory)
if ((Test-Path -LiteralPath $root) -and (Get-ChildItem -LiteralPath $root -Force | Select-Object -First 1)) { throw 'Choose an empty test directory.' }
$player=Join-Path $root 'Player'
$source=Join-Path $root 'TestSource'
& "$PSScriptRoot/Build-Windows.ps1" -BaseZip $BaseZip -OutputDirectory $player -DotNet $DotNet
New-Item -ItemType Directory -Force $source | Out-Null
Copy-Item "$PSScriptRoot/../Gameplay/*.cs" $source
Copy-Item "$PSScriptRoot/../Tests/AtmosphereCheck.cs" $source
$p=Join-Path $source 'CursorScript.cs'
[IO.File]::WriteAllText($p,([IO.File]::ReadAllText($p).Replace('Cursor.visible = true;','Cursor.visible = true; AtmosphereCheck.Boot();')))
foreach($name in 'Crank','KeyPickup') {
    $p=Join-Path $source "$name.cs"
    [IO.File]::WriteAllText($p,([IO.File]::ReadAllText($p).Replace('Input.GetKey(KeyCode.E)','AtmosphereCheck.Interact').Replace('Input.GetKeyDown(KeyCode.E)','AtmosphereCheck.Interact')))
}
$sdks=@(& $DotNet --list-sdks)
if(!$sdks -or $sdks[-1] -notmatch '^([0-9.\w-]+) \[(.+)\]$') { throw 'A .NET SDK is required.' }
$compiler=Join-Path $Matches[2] "$($Matches[1])/Roslyn/bincore/csc.dll"
$managed=Join-Path $player 'The Mask Within_Data/Managed'
$refs=Get-ChildItem -LiteralPath $managed -Filter '*.dll' | Where-Object Name -ne 'Assembly-CSharp.dll' | ForEach-Object {"-reference:$($_.FullName)"}
$files=(Get-ChildItem -LiteralPath $source -Filter '*.cs' | Sort-Object Name).FullName
& $DotNet $compiler /nologo /target:library /nostdlib+ /optimize+ "-out:$managed/Assembly-CSharp.dll" @refs @files
if($LASTEXITCODE -ne 0) { throw 'Test compilation failed.' }
$log=Join-Path $root 'test.log'
$p=Start-Process -FilePath "$player/The Mask Within.exe" -ArgumentList @('-screen-fullscreen','0','-screen-width','1280','-screen-height','720','-logFile',('"'+$log+'"')) -WindowStyle Hidden -PassThru
if(!$p.WaitForExit(150000)) { Stop-Process -Id $p.Id; throw 'Test timed out.' }
Get-Content $log | Select-String 'PASS:|FAIL:|Exception|TIMING|LEGACY|CHECK PASSED'
if($p.ExitCode -ne 0 -or !(Select-String -Path $log -SimpleMatch 'ATMOSPHERE CHECK PASSED' -Quiet)) { throw 'Verification failed; see test.log.' }
