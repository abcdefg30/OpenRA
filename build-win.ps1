(Get-Content "OpenRA.WindowsLauncher\ModConfig.cs.in").replace('DISPLAY_NAME', 'Red Alert').replace('MOD_ID', 'ra').replace('FAQ_URL', 'http://wiki.openra.net/FAQ') | Set-Content "OpenRA.WindowsLauncher\ModConfig.cs"
dotnet build OpenRA.WindowsLauncher\OpenRA.WindowsLauncher.csproj -p:icon=RedAlert.ico --no-incremental
Move-Item -Path OpenRA.WindowsLauncher.exe -Destination RedAlert.exe -Force

dotnet build OpenRA.WindowsLauncher\OpenRA.WindowsLauncher.csproj -p:icon=TiberianDawn.ico --force --no-incremental
Move-Item -Path OpenRA.WindowsLauncher.exe -Destination TiberianDawn.exe -Force

dotnet build OpenRA.WindowsLauncher\OpenRA.WindowsLauncher.csproj -p:icon=Dune2000.ico --force --no-incremental
Move-Item -Path OpenRA.WindowsLauncher.exe -Destination Dune2000.exe -Force
ie4uinit.exe -show
