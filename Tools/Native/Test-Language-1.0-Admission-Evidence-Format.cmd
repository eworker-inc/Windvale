@echo off
setlocal
set "ScriptDirectory=%~dp0"
node "%ScriptDirectory%Test-Language-1.0-Admission-Evidence-Format.mjs"
exit /b %errorlevel%
