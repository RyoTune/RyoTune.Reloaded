# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/RyoTune.Reloaded.Template/*" -Force -Recurse
dotnet publish "./RyoTune.Reloaded.Template.csproj" -c Release -o "$env:RELOADEDIIMODS/RyoTune.Reloaded.Template" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location