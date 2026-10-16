@echo off
rem Builds fake_steam_api.dll for tests/steam-native.cjs with the MSVC x64 tools. Usage: build.cmd <outDir>
setlocal
set "OUT=%~1"
if "%OUT%"=="" set "OUT=%~dp0"
set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
for /f "usebackq delims=" %%i in (`"%VSWHERE%" -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath`) do set "VS=%%i"
if not defined VS (echo MSVC x64 tools not found & exit /b 2)
call "%VS%\VC\Auxiliary\Build\vcvars64.bat" >nul || exit /b 2
cd /d "%OUT%"
cl /nologo /EHsc /O1 /LD "%~dp0fake_steam_api.cpp" /Fe:fake_steam_api.dll /link /NOLOGO >build.log 2>&1 || (type build.log & exit /b 1)
exit /b 0
