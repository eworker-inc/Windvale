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
set "FailureStep=project2-build"
set "Candidate=%TestDirectory%\Candidate.wvb"

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%Fixtures%\Workspace-Project2-Build.wvproj" "%Candidate%" ^
    >"%TestDirectory%\Valid.out" 2>"%TestDirectory%\Valid.err"
if errorlevel 1 goto :cleanup
set "FailureStep=project2-run"
call "%RepositoryRoot%\Tools\Native\Run-Wvb.cmd" "%Candidate%" --report-steps ^
    >"%TestDirectory%\Run.out" 2>"%TestDirectory%\Run.err"
if errorlevel 1 goto :cleanup
set "FailureStep=project2-report"
findstr /c:"Result: 42" "%TestDirectory%\Run.out" >nul || goto :cleanup

set "FailureStep=project3-build"
set "Project3Candidate=%TestDirectory%\Project3-Candidate.wvb"
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%Fixtures%\Project3-Manifest-Self-Test.wvproj" "%Project3Candidate%" ^
    >"%TestDirectory%\Project3.out" 2>"%TestDirectory%\Project3.err"
if errorlevel 1 goto :cleanup
set "FailureStep=project3-run"
call "%RepositoryRoot%\Tools\Native\Run-Wvb.cmd" "%Project3Candidate%" ^
    >"%TestDirectory%\Project3-Run.out" 2>"%TestDirectory%\Project3-Run.err"
if errorlevel 1 goto :cleanup
set "FailureStep=project3-report"
findstr /c:"Result: 42" "%TestDirectory%\Project3-Run.out" >nul || goto :cleanup

set "FailureStep=project-rejections"
call :reject_project "Legacy-Project1.wvproj" "WVP1001" || goto :cleanup
call :reject_project "Parent-Escape-Project2.wvproj" "WVP1006" || goto :cleanup
call :reject_project "Absolute-Path-Project2.wvproj" "WVP1006" || goto :cleanup
call :reject_project "Duplicate-Path-Project2.wvproj" "WVP1007" || goto :cleanup
call :reject_project "Project2-Profile-Directive.wvproj" "WVP1003" || goto :cleanup
call :reject_workspace "Invalid-Header.wvws" "Workspace-Project2-Build.wvproj" "WVW1001" || goto :cleanup
call :reject_workspace "Trailing-Data.wvws" "Workspace-Project2-Build.wvproj" "WVW1002" || goto :cleanup
call :reject_workspace "Nested\Windvale.wvws" "Workspace-Project2-Build.wvproj" "WVW1003" || goto :cleanup

echo native workspace/project test status=Complete cases=10
set "Result=0"

:cleanup
if not "%Result%"=="0" (
    >&2 echo FAIL  native workspace/project test step=%FailureStep%
    for %%F in (
        "%TestDirectory%\Valid.out"
        "%TestDirectory%\Valid.err"
        "%TestDirectory%\Run.out"
        "%TestDirectory%\Run.err"
        "%TestDirectory%\Project3.out"
        "%TestDirectory%\Project3.err"
        "%TestDirectory%\Project3-Run.out"
        "%TestDirectory%\Project3-Run.err"
        "%TestDirectory%\Reject.out"
        "%TestDirectory%\Reject.err"
    ) do if exist "%%~F" type "%%~F" >&2
)
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
