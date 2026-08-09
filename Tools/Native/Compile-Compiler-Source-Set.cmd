@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~3"=="" goto :usage
if not "%~4"=="" goto :usage
if /I not "%~x1"==".exe" goto :usage
if /I not "%~x3"==".wvb" goto :usage

set "Compiler=%~f1"
set "SourceRoot=%~f2"
set "Output=%~f3"
if not exist "%Compiler%" goto :usage
if not exist "%SourceRoot%\." goto :usage

"%Compiler%" ^
    "%SourceRoot%\Examples\Compiler\Source-Wvb-Tool.wv" ^
    "%SourceRoot%\Compiler\Windvale\Source-Bindings-Core.wv" ^
    "%SourceRoot%\Compiler\Windvale\Source-Body-Parser.wv" ^
    "%SourceRoot%\Compiler\Windvale\Source-Declaration-Parser.wv" ^
    "%SourceRoot%\Compiler\Windvale\Source-Graph-Core.wv" ^
    "%SourceRoot%\Compiler\Windvale\Source-Lexer-Core.wv" ^
    "%SourceRoot%\Compiler\Windvale\Source-Set-Core.wv" ^
    "%SourceRoot%\Compiler\Windvale\Source-Symbols-Core.wv" ^
    "%SourceRoot%\Compiler\Windvale\Source-Wir-Core.wv" ^
    "%SourceRoot%\Compiler\Windvale\Source-Wvb-Core.wv" ^
    "%SourceRoot%\Compiler\Windvale\Source-Wvb-Temporary-Slots.wv" ^
    "%SourceRoot%\Foundation\Byte-Construction.wv" ^
    "%SourceRoot%\Foundation\Decimal-Parsing.wv" ^
    "%Output%"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Compile-Compiler-Source-Set.cmd ^<compiler.exe^> ^<source-root^> ^<output.wvb^>
exit /b 64
