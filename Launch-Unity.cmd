@echo off
setlocal
set "BESIEGED_EDITOR=%ProgramFiles%\Unity\Hub\Editor\2022.3.15f1\Editor\Unity.exe"
if not exist "%BESIEGED_EDITOR%" (
    echo Unity 2022.3.15f1 was not found in its default installation folder.
    echo Open Unity Hub and add the folder containing this launcher as a project.
    echo See README.md for instructions.
    pause
    exit /b 1
)
start "" "%BESIEGED_EDITOR%" -projectPath "%~dp0."
endlocal
