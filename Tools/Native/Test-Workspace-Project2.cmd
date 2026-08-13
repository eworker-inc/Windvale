@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "BuildDriver=%RepositoryRoot%\Artifacts\Native-Front-Door\windows-x64\wvbuild.exe"
set "Workspace=%RepositoryRoot%\Windvale.wvws"
set "Fixtures=%RepositoryRoot%\Tests\Fixtures\Project"

:allocate
set "TestDirectory=%TEMP%\windvale-workspace-project2-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TestDirectory%" goto :allocate
mkdir "%TestDirectory%" || exit /b 1
set "Result=1"
set "Candidate=%TestDirectory%\Candidate.wvb"

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%Fixtures%\Workspace-Project2-Build.wvproj" "%Candidate%" ^
    >"%TestDirectory%\Valid.out" 2>"%TestDirectory%\Valid.err"
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Run-Wvb.cmd" "%Candidate%" --report-steps ^
    >"%TestDirectory%\Run.out" 2>"%TestDirectory%\Run.err"
if errorlevel 1 goto :cleanup
findstr /c:"Result: 42" "%TestDirectory%\Run.out" >nul || goto :cleanup

call :reject_project "Legacy-Project1.wvproj" "WVP1001" || goto :cleanup
call :reject_project "Parent-Escape-Project2.wvproj" "WVP1006" || goto :cleanup
call :reject_project "Absolute-Path-Project2.wvproj" "WVP1006" || goto :cleanup
call :reject_project "Duplicate-Path-Project2.wvproj" "WVP1007" || goto :cleanup
call :reject_workspace "Invalid-Header.wvws" "Workspace-Project2-Build.wvproj" "WVW1001" || goto :cleanup
call :reject_workspace "Trailing-Data.wvws" "Workspace-Project2-Build.wvproj" "WVW1002" || goto :cleanup
call :reject_workspace "Nested\Windvale.wvws" "Workspace-Project2-Build.wvproj" "WVW1003" || goto :cleanup

echo native workspace/project test status=Complete cases=8
set "Result=0"

:cleanup
if exist "%TestDirectory%\." rmdir /s /q "%TestDirectory%"
exit /b %Result%

:reject_project
set "CaseWorkspace=%Workspace%"
set "CaseProject=%Fixtures%\%~1"
set "CaseCode=%~2"
call :run_rejection
exit /b %ERRORLEVEL%

:reject_workspace
set "CaseWorkspace=%Fixtures%\%~1"
set "CaseProject=%Fixtures%\%~2"
set "CaseCode=%~3"
call :run_rejection
exit /b %ERRORLEVEL%

:run_rejection
set "CaseWorkspace=%CaseWorkspace:\=/%"
set "CaseProject=%CaseProject:\=/%"
set "CaseCandidate=%Candidate:\=/%"
"%BuildDriver%" --workspace "%CaseWorkspace%" --project "%CaseProject%" "%CaseCandidate%" ^
    >"%TestDirectory%\Reject.out" 2>"%TestDirectory%\Reject.err"
if not "%ERRORLEVEL%"=="1" exit /b 1
findstr /c:"code=%CaseCode%" "%TestDirectory%\Reject.err" >nul
exit /b %ERRORLEVEL%
