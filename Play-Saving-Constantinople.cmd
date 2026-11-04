@echo off
cd /d "%~dp0"
if exist "Builds\Version-39.1\Saving Constantinople.exe" (
    start "Against Fate: Saving Constantinople" "Builds\Version-39.1\Saving Constantinople.exe" %*
) else (
    start "Saving Constantinople - legacy Version 39" "Builds\Version-39\Besieged.exe" %*
)
