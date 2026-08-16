@echo off
setlocal EnableExtensions DisableDelayedExpansion
if not "%~1"=="" exit /b 64
set "RepositoryRoot=%~dp0\..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
goto :allocate
:verify
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /i /x /c:"%~3" >nul
exit /b %ERRORLEVEL%
:allocate
set "Work=%TEMP%\windvale-os-x64-code-emission-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "Status=1"
echo step=code-emission item=1/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Code-Emission.wvproj" "%Work%\Test.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Test.wvb" 13597 3bdfd99bb37c4ff037a2d57bfdd89e67a2f190df77f113b50effba1f9c6bd24f
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\Test.wvb" "%Work%\Test.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Test.wvo" 187279 00dd63a5703136ed0ebe06e55b6e6907f0394fde3b27935ffdcaa60d18f8c4c9
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\Test.bin" "%Work%\Test.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Test.bin" 186526 3c1bac2e475b55721a65da9a3d39fefbfe442c3d50ddd75ea166b80fa65a77d2
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\Test.bin" 0 "%Work%\Test.exe" >nul
if errorlevel 1 goto :cleanup
"%Work%\Test.exe" >nul
if not "%ERRORLEVEL%"=="50" goto :cleanup
call :verify "%Work%\Test.exe" 188416 2904c72b25c1d827b0547e839bc5b237694db3e6b4b52de182d774ec83853bec
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\Test.bin" 0 "%Work%\Test.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Test.elf" 192624 6ed4a40989e0f33e6461d36f2dc8402894ff9e3d9b8417978159cc40df89b300
if errorlevel 1 goto :cleanup
echo step=process-entry item=2/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Entry-Emission.wvproj" "%Work%\Entry.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Entry.wvb" 18819 3d830d8788372bfb35e59f86f1cd2fce4bcbab38536d3e1da287f4cac4d15749
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\Entry.wvb" "%Work%\Entry.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Entry.wvo" 293142 503d0a912e6299c6ee2ae2a2d441c3d3efe2d99bdf463ea9929662c022ba9c36
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\Entry.bin" "%Work%\Entry.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Entry.bin" 291060 45d89b13ccdc220951e1b949920de44fc9e2948f3d30e58e7f57dbe415b9c15a
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\Entry.bin" 0 "%Work%\Entry.exe" >nul
if errorlevel 1 goto :cleanup
"%Work%\Entry.exe" >nul
if not "%ERRORLEVEL%"=="51" goto :cleanup
call :verify "%Work%\Entry.exe" 292864 89e0cb8b18666d51ebf7176a913523927f91b1e816a489ad755d891cdc394f30
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\Entry.bin" 0 "%Work%\Entry.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Entry.elf" 299120 81733b1d317d4df43f3b46a1ebdb4e620d5fd38771a734834efc7c3ae1d4dd61
if errorlevel 1 goto :cleanup
echo step=process-coordinator item=3/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Coordinator-Emission.wvproj" "%Work%\Coordinator.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Coordinator.wvb" 17360 da3d04e734f6057ce9665e1e1c48d6c9dfcdbe0a9396cd1a94397ac4d284a203
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\Coordinator.wvb" "%Work%\Coordinator.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Coordinator.wvo" 252088 aee75cbbb20681001780422c024d058281d89b1e90d1b62405c9edce186c6b77
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\Coordinator.bin" "%Work%\Coordinator.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Coordinator.bin" 249692 a8d608a9940f68b8de11988efa3749a6153a75292f38cd145c02ae1000a16732
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\Coordinator.bin" 0 "%Work%\Coordinator.exe" >nul
if errorlevel 1 goto :cleanup
"%Work%\Coordinator.exe" >nul
if not "%ERRORLEVEL%"=="52" goto :cleanup
call :verify "%Work%\Coordinator.exe" 251392 128269a0d5cedd8e2eed4ab4a569b355a1811b2300fd7b16a36078f3eee15c36
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\Coordinator.bin" 0 "%Work%\Coordinator.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Coordinator.elf" 254064 8c160bc19330784ca82ca837d5a33fe93fe44fc5701cf491c661b1d06e728318
if errorlevel 1 goto :cleanup
echo step=process-endpoint item=4/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Endpoint-Emission.wvproj" "%Work%\Endpoint.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Endpoint.wvb" 14386 2d9bdb6b1705bdc0e2e2f3a9b5e5e98224545abc1730ced3c5f55ec0a5cd1391
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\Endpoint.wvb" "%Work%\Endpoint.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Endpoint.wvo" 213163 e5a62845e56b9c77b7adccdb8853c1a089f088b62b01088a2814de52d746df66
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\Endpoint.bin" "%Work%\Endpoint.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Endpoint.bin" 211641 93ba7b48ba58558471fc678e74a5bab841fd45268f289e1a759c912551e8b796
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\Endpoint.bin" 0 "%Work%\Endpoint.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\Endpoint.exe" >nul
if not "%ERRORLEVEL%"=="53" goto :cleanup
call :verify "%Work%\Endpoint.exe" 213504 bb53be86bb8351e805fd0919c6b0836efb483894c36568a6b38dde039a369b20
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\Endpoint.bin" 0 "%Work%\Endpoint.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Endpoint.elf" 217200 b649ba1abe8db582942085afc90b14ad8d9cd44b542d232df3b7ea19f8a7eb2f
if errorlevel 1 goto :cleanup
echo step=init-allocation item=5/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Memory-Allocation-Emission.wvproj" "%Work%\Allocation.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Allocation.wvb" 14586 1baa66d77b35db8c2629c0cc2478e29b716739b5ad2c3a2a9096ad9439011112
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\Allocation.wvb" "%Work%\Allocation.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Allocation.wvo" 205076 382ffcd386872dd42126c2e85a84746b17df4489ecbbb8c63597c1d710572c79
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\Allocation.bin" "%Work%\Allocation.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Allocation.bin" 203252 f17991141c256c3428221465ead0afb13b90787dfda8ae77509f325ebd222008
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\Allocation.bin" 0 "%Work%\Allocation.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\Allocation.exe" >nul
if not "%ERRORLEVEL%"=="54" goto :cleanup
call :verify "%Work%\Allocation.exe" 205312 fe1aa700ae411cc3f02277bc13cc8980721fe62aa03f08b0862d81f5bf9e6270
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\Allocation.bin" 0 "%Work%\Allocation.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Allocation.elf" 209008 197947667b10fc4bb9a4df15117a0f34f9ff1237a950408679cf9c729fb008c8
if errorlevel 1 goto :cleanup
echo step=init-record item=6/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Record-Emission.wvproj" "%Work%\Record.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Record.wvb" 16069 be44b1d300abd532a5689755f9ab9ed75b49e7e4954395d3626ee175b9b97e13
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\Record.wvb" "%Work%\Record.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Record.wvo" 235764 b013b39333881cfe78f7c1915388ae663a912377b003aa80348993ca876513ee
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\Record.bin" "%Work%\Record.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Record.bin" 233968 157e355ac94220da5f1b6df2cfc5d51a54b21506f675b06dc3aff0b328371e9b
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\Record.bin" 0 "%Work%\Record.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\Record.exe" >nul
if not "%ERRORLEVEL%"=="55" goto :cleanup
call :verify "%Work%\Record.exe" 236032 693ce53db751bd537ade2933adc8f688ff42492aad6091e005ea9b6391d7ff16
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\Record.bin" 0 "%Work%\Record.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Record.elf" 241776 1ecaa2ac3dda959a632b88c753d4189ecd3213a2f04c69c886f5bc0f11db23c0
if errorlevel 1 goto :cleanup
echo step=init-paging item=7/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Paging-Emission.wvproj" "%Work%\Paging.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Paging.wvb" 14379 e2f712fb99ecc186211c957a4bdf9f9b0991ad7c735dcb8d47c643e85f9fd50d
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\Paging.wvb" "%Work%\Paging.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Paging.wvo" 206912 59dc7bcdd1a0ae0b74aa71a85f4330cc28a2f7d3e9d3d4cddce367885d9a6534
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\Paging.bin" "%Work%\Paging.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Paging.bin" 205144 eb4ffec315e7ed51c2de630f789cf3aadc44b5ff2ed81131f9b32b39af53608b
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\Paging.bin" 0 "%Work%\Paging.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\Paging.exe" >nul
if not "%ERRORLEVEL%"=="56" goto :cleanup
call :verify "%Work%\Paging.exe" 206848 857d384d8e62ccfb435986c4b607d8a7615b9d9bc8c78d1bd73efa38f0dc832e
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\Paging.bin" 0 "%Work%\Paging.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Paging.elf" 213104 fd20a386a8a0e03a9efce86444498e119f7dffbd67263c3845659d1a7f949ef2
if errorlevel 1 goto :cleanup
echo step=init-image item=8/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Image-Emission.wvproj" "%Work%\Image.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Image.wvb" 16434 3207175a3928407f8b0fb1976e8f55c3643ffa5f0555a46fa9379354d90c0ae1
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\Image.wvb" "%Work%\Image.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Image.wvo" 212268 32133cb740952b9193017defc27d10937eabbae42bfa31138eddef259c6147aa
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\Image.bin" "%Work%\Image.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Image.bin" 210560 a9c40a6cc7de6c3468efc70a675ec2681130344c0cf7e1da8d868bfab998008a
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\Image.bin" 0 "%Work%\Image.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\Image.exe" >nul
if not "%ERRORLEVEL%"=="57" goto :cleanup
call :verify "%Work%\Image.exe" 212480 722e4d867408a750d534ddd2ca55b43512ef934d68fd66aaf8e8ba1411d6c8e7
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\Image.bin" 0 "%Work%\Image.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Image.elf" 217200 58b42db3daa211c10f79426dae970fb635233ec19e7f135a1e54ed963e526a87
if errorlevel 1 goto :cleanup
echo step=client-reservation item=9/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Client-Reservation-Emission.wvproj" "%Work%\ClientReservation.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientReservation.wvb" 14957 bd9bd8bb378642e707e5a328a783dd42df20457aa04c967fcbf63cf8845678b4
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\ClientReservation.wvb" "%Work%\ClientReservation.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientReservation.wvo" 212197 1d6543a6f9eeb86a0a0e7e64a39b4af66181ea28e88e02ae132b3150ca236391
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\ClientReservation.bin" "%Work%\ClientReservation.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientReservation.bin" 210223 5b1e36c70a22eab46561f58f73e18ea3241de6afe9ae01d5e2d41c6279c6afdb
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\ClientReservation.bin" 0 "%Work%\ClientReservation.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\ClientReservation.exe" >nul
if not "%ERRORLEVEL%"=="58" goto :cleanup
call :verify "%Work%\ClientReservation.exe" 211968 b98c4e3351ea369e6eb70fb8476b03d61300065ae0b57e0d860de458a955196f
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\ClientReservation.bin" 0 "%Work%\ClientReservation.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientReservation.elf" 217200 547f5351c84530e41436b51f03b25680f1added815d6238998dc5fe7915e0684
if errorlevel 1 goto :cleanup
echo step=directory-allocation item=10/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Directory-Allocation-Emission.wvproj" "%Work%\DirectoryAllocation.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\DirectoryAllocation.wvb" 14733 c75790ba9823172830b6da72f83a77ce9de2014e0ac9ce4730283a21e261d76f
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\DirectoryAllocation.wvb" "%Work%\DirectoryAllocation.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\DirectoryAllocation.wvo" 207898 0f56e3c872b673a3df7a6aa32aa5c0872588e5d8a867917ae31802818f766cf5
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\DirectoryAllocation.bin" "%Work%\DirectoryAllocation.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\DirectoryAllocation.bin" 206024 01930821a30da4f113f69dbf7b71937d980a62bc9d8d1684e59df545b65462fa
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\DirectoryAllocation.bin" 0 "%Work%\DirectoryAllocation.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\DirectoryAllocation.exe" >nul
if not "%ERRORLEVEL%"=="59" goto :cleanup
call :verify "%Work%\DirectoryAllocation.exe" 207872 45d79cbb35032809d41adb4711803772dad0f07a8696674614e832c651748d75
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\DirectoryAllocation.bin" 0 "%Work%\DirectoryAllocation.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\DirectoryAllocation.elf" 213104 551df680881fb91b911caa77f92cb60e02e5f68c11544ea24ffe9b3b634486a3
if errorlevel 1 goto :cleanup
echo step=directory-record item=11/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Directory-Record-Emission.wvproj" "%Work%\DirectoryRecord.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\DirectoryRecord.wvb" 16076 b549bbb7566023e09cb8dfa65ad774c6c99a6d4cb4b5f7239d0be317833d40b3
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\DirectoryRecord.wvb" "%Work%\DirectoryRecord.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\DirectoryRecord.wvo" 235780 b7c699def26f9e8b8967142a8ad0fe975ae04c9fa60b5b16e7fcf6a03373682c
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\DirectoryRecord.bin" "%Work%\DirectoryRecord.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\DirectoryRecord.bin" 233984 544f5033bbcf0c9f781bdbf493e34cfd4b6816b53fac64ff0d34f58a3d801bd8
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\DirectoryRecord.bin" 0 "%Work%\DirectoryRecord.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\DirectoryRecord.exe" >nul
if not "%ERRORLEVEL%"=="60" goto :cleanup
call :verify "%Work%\DirectoryRecord.exe" 236032 865f82f369212f100f46d8e630bfef5a1aa5468e211ac8e15258bfe7c95f4b19
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\DirectoryRecord.bin" 0 "%Work%\DirectoryRecord.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\DirectoryRecord.elf" 241776 b4c32f4820655131c2ba596f8003d78c3ffd16179a599c4f4fe77c9e36267e23
if errorlevel 1 goto :cleanup
echo step=directory-paging item=12/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Directory-Paging-Emission.wvproj" "%Work%\DirectoryPaging.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\DirectoryPaging.wvb" 14228 caba027a75434fc07c2f44cafead16f595e7ce4fc13a84864041204d24cd5c17
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\DirectoryPaging.wvb" "%Work%\DirectoryPaging.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\DirectoryPaging.wvo" 203856 cefb806e6d0af4d7cb531d0f7e4579397a9367e1b96a83e82ed04cccd29764c8
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\DirectoryPaging.bin" "%Work%\DirectoryPaging.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\DirectoryPaging.bin" 202088 be7476e644e81062f637395946d8b3e8188c3d61b0b6b5e0854dc838c649b3d1
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\DirectoryPaging.bin" 0 "%Work%\DirectoryPaging.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\DirectoryPaging.exe" >nul
if not "%ERRORLEVEL%"=="61" goto :cleanup
call :verify "%Work%\DirectoryPaging.exe" 203776 0308cf1a5d01eeb2d463f43bc4ea3b3993f4922b5732cee7e8b23964e2d001c0
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\DirectoryPaging.bin" 0 "%Work%\DirectoryPaging.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\DirectoryPaging.elf" 209008 303eada707e4868fba8406ccc304e5764ce069d156808d6f44245e98629fb0d9
if errorlevel 1 goto :cleanup
echo step=directory-image item=13/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Directory-Image-Emission.wvproj" "%Work%\DirectoryImage.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\DirectoryImage.wvb" 15098 589034ed2ae906ba8c96ebedb3e583decb9d9181527b70b389d64296f66a4171
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\DirectoryImage.wvb" "%Work%\DirectoryImage.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\DirectoryImage.wvo" 204016 59380cede0d6d500f554dbbffa8bd8a98bd3cb3e68361c34a32a58fae8642e78
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\DirectoryImage.bin" "%Work%\DirectoryImage.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\DirectoryImage.bin" 202288 5222772acdcf41ec237179a72083725d9fd5bec8e83324096a5ccba961bbe246
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\DirectoryImage.bin" 0 "%Work%\DirectoryImage.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\DirectoryImage.exe" >nul
if not "%ERRORLEVEL%"=="62" goto :cleanup
call :verify "%Work%\DirectoryImage.exe" 204288 b20d649b83c3b3ca54550118f77c7775a4937d789f0c08832c03444861c68fbd
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\DirectoryImage.bin" 0 "%Work%\DirectoryImage.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\DirectoryImage.elf" 209008 4c66120f10ba53e10cf1e7e31ca600eef51d47874b5f629aec0f8c46091bef98
if errorlevel 1 goto :cleanup
echo step=client-record item=14/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Client-Record-Emission.wvproj" "%Work%\ClientRecord.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientRecord.wvb" 16843 6182088b7f1ae89766d2a8cb20b2b022a4ca54571ba63312c7111379c1b15ef3
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\ClientRecord.wvb" "%Work%\ClientRecord.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientRecord.wvo" 251549 a618e37ff642f693a0e80c46307a595e7634041026c6130a82b2673850c66b79
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\ClientRecord.bin" "%Work%\ClientRecord.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientRecord.bin" 249563 e64360935ccd01c7146f3ef5890313d3afa180941e39dd22f8c9d6aa7a6fa4d5
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\ClientRecord.bin" 0 "%Work%\ClientRecord.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\ClientRecord.exe" >nul
if not "%ERRORLEVEL%"=="63" goto :cleanup
call :verify "%Work%\ClientRecord.exe" 251392 2cbedd60fd226415ba274cffb121b7c39505fa74a6ed854fa628770d844d406b
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\ClientRecord.bin" 0 "%Work%\ClientRecord.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientRecord.elf" 254064 08911fe6297712035388dd9ae1baaa9e03ddb6d905fd82aba485a33dc192f484
if errorlevel 1 goto :cleanup
echo step=client-paging item=15/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Client-Paging-Emission.wvproj" "%Work%\ClientPaging.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientPaging.wvb" 14563 b848688f23ff1e1750044eaec3b4f1837454f7a0c73938699435ce56f81b8fe9
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\ClientPaging.wvb" "%Work%\ClientPaging.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientPaging.wvo" 206507 fdc55615cdcc3dfe88a0efc281e750074842e95a0661a2b419add0e2df3163c8
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\ClientPaging.bin" "%Work%\ClientPaging.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientPaging.bin" 204635 dfdc768e8d583879c116b2edb29a9a1740d4c38846d8bf93391ef026ca6511d8
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\ClientPaging.bin" 0 "%Work%\ClientPaging.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\ClientPaging.exe" >nul
if not "%ERRORLEVEL%"=="64" goto :cleanup
call :verify "%Work%\ClientPaging.exe" 206336 5e67969e9047f8b5d71ec79d0de6c86bfdaa77905fac314d12a6ab9d8e7cced7
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\ClientPaging.bin" 0 "%Work%\ClientPaging.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientPaging.elf" 209008 bd58157bc0b8023ea2a413c50a5b275bf958b256d08fcbb310a8abb96cca740e
if errorlevel 1 goto :cleanup
echo step=client-image item=16/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Client-Image-Emission.wvproj" "%Work%\ClientImage.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientImage.wvb" 13798 e45446f9c0aa6d8806c3427d2aa3900266067112ff90c29b8d0dea2ea4f4aafd
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\ClientImage.wvb" "%Work%\ClientImage.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientImage.wvo" 187723 7491dcf347e3ce772b9f18d1678191cba4624862575133886e63bbc4e545f88c
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\ClientImage.bin" "%Work%\ClientImage.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientImage.bin" 186049 05acfd5941a598b6c8c43f0aa7406cdcb41da8e5e923a22f489d945c1e2b1a60
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\ClientImage.bin" 0 "%Work%\ClientImage.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\ClientImage.exe" >nul
if not "%ERRORLEVEL%"=="65" goto :cleanup
call :verify "%Work%\ClientImage.exe" 187904 741049bdb17717f89fc617322a5aa07fe94a4e2c2e3e1286a5a83d62b285067f
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\ClientImage.bin" 0 "%Work%\ClientImage.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientImage.elf" 192624 a2b3880da1d0bdefaf491717d180bb638118d9b706f550f2100b7e596382c1fe
if errorlevel 1 goto :cleanup
echo step=client-program-resource item=17/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Client-Program-Resource-Emission.wvproj" "%Work%\ClientProgramResource.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientProgramResource.wvb" 12763 d0c7e8f7890e6cbc0168dfe122564b48f03a2c4d5bfb658e4e20a9c4ec4e85a1
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\ClientProgramResource.wvb" "%Work%\ClientProgramResource.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientProgramResource.wvo" 168553 8a05720a44e34a749829ccd421a6dcce3ba9e867f029f245a4f543c93f666bfa
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\ClientProgramResource.bin" "%Work%\ClientProgramResource.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientProgramResource.bin" 167129 b1fd079dde392a72817d30ca9e9a68ab81d926db54bb40c2c1aed76a380e94a3
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\ClientProgramResource.bin" 0 "%Work%\ClientProgramResource.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\ClientProgramResource.exe" >nul
if not "%ERRORLEVEL%"=="66" goto :cleanup
call :verify "%Work%\ClientProgramResource.exe" 168960 ac00e3dc1267d2c1c5ce11e389ea93711297930a7b99c1fb061d148b3c001f49
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\ClientProgramResource.bin" 0 "%Work%\ClientProgramResource.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientProgramResource.elf" 172144 d8b7bf66d482a976a7ecec2b3c0d408c52d942e0b0c75360883cf117aab3d72f
if errorlevel 1 goto :cleanup
echo step=client-budget-resource item=18/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Client-Budget-Resource-Emission.wvproj" "%Work%\ClientBudgetResource.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientBudgetResource.wvb" 12586 080eec8cd90b5364bc374eed8fdd3dae520ce7ee9bfb48c0ff30e08aa7150939
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\ClientBudgetResource.wvb" "%Work%\ClientBudgetResource.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientBudgetResource.wvo" 165922 6b289d4388ef5f2b79d0e344cc2424b0fb823715167309b299347653d7a6f80c
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\ClientBudgetResource.bin" "%Work%\ClientBudgetResource.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientBudgetResource.bin" 164598 ad05831ffbde9abed9e6d8f58bc7c2fb064ed23b28b0ab49e67a478b1e6acd8e
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\ClientBudgetResource.bin" 0 "%Work%\ClientBudgetResource.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\ClientBudgetResource.exe" >nul
if not "%ERRORLEVEL%"=="67" goto :cleanup
call :verify "%Work%\ClientBudgetResource.exe" 166400 577eb58b87816cc15004096f1a20b5e042e3196b5ee3b71af691e91f89f92725
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\ClientBudgetResource.bin" 0 "%Work%\ClientBudgetResource.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientBudgetResource.elf" 172144 7461b66d1b74e3dbab07f682d00627c16b32bd1a15a99beef52a8da2aeeb288f
if errorlevel 1 goto :cleanup
echo step=client-store-resource item=19/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Client-Store-Resource-Emission.wvproj" "%Work%\ClientStoreResource.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientStoreResource.wvb" 12594 e367cd4e99c842b1e18e9eba459ce034263b3cd6add89ee5d15153015e10dde6
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\ClientStoreResource.wvb" "%Work%\ClientStoreResource.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientStoreResource.wvo" 165932 ae32cd44aafa4c5f03766c40788d2340622c02846e801c5cc9e87bab945603fa
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\ClientStoreResource.bin" "%Work%\ClientStoreResource.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientStoreResource.bin" 164608 7d797d3765d66df301d2f38e7eff6a17612988664405228b739e2a7081427848
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\ClientStoreResource.bin" 0 "%Work%\ClientStoreResource.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\ClientStoreResource.exe" >nul
if not "%ERRORLEVEL%"=="68" goto :cleanup
call :verify "%Work%\ClientStoreResource.exe" 166400 28d3812b8a5a627eda4a4c8eeb854a4ca266da49c35eef07997affcb05edc9ec
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\ClientStoreResource.bin" 0 "%Work%\ClientStoreResource.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientStoreResource.elf" 172144 96d9d990d5500af4975c54083e76a4837b2915cb873c390d0e32c7650bcb1987
if errorlevel 1 goto :cleanup
echo step=client-directory-resource item=20/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Client-Directory-Resource-Emission.wvproj" "%Work%\ClientDirectoryResource.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientDirectoryResource.wvb" 12601 64cf8e6b7241e7fab1aa79d32977bdeb52efd72a376ac96a978093599a7c1c5e
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\ClientDirectoryResource.wvb" "%Work%\ClientDirectoryResource.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientDirectoryResource.wvo" 165932 4cf991e23cfe523d61146fa97e8c66685fe040003ed4a72b7b1c898c73514655
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\ClientDirectoryResource.bin" "%Work%\ClientDirectoryResource.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientDirectoryResource.bin" 164608 03e9b50e55bae6206f2e58648d55d653ed10b9a10a85ec32a5efffe163daab23
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\ClientDirectoryResource.bin" 0 "%Work%\ClientDirectoryResource.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\ClientDirectoryResource.exe" >nul
if not "%ERRORLEVEL%"=="69" goto :cleanup
call :verify "%Work%\ClientDirectoryResource.exe" 166400 8d6d31d9d7ba4f221fe331274e926c41310492f3b94eceb7dac9030c47df365f
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\ClientDirectoryResource.bin" 0 "%Work%\ClientDirectoryResource.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientDirectoryResource.elf" 172144 d7addd829407b5ef37500c9f54ee08d194858c5bef28497b94e0cca2769fc1aa
if errorlevel 1 goto :cleanup
echo step=client-store-validation item=21/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Client-Store-Validation-Emission.wvproj" "%Work%\ClientStoreValidation.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientStoreValidation.wvb" 4504 8e0e5c8b0dcc5d58c6f89a517af6ae1bcc30fcf99da2e63fef09892d67c81ead
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\ClientStoreValidation.wvb" "%Work%\ClientStoreValidation.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientStoreValidation.wvo" 62214 e0a46fb18221c75467a6e7d7b6d0c541bce1b9f3275ba389f6c555352ef753ef
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\ClientStoreValidation.bin" "%Work%\ClientStoreValidation.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientStoreValidation.bin" 61794 a04b67dd98d2d0fb6ea60291466ff0adea2333e93c35f766e8033071142eeee3
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\ClientStoreValidation.bin" 0 "%Work%\ClientStoreValidation.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\ClientStoreValidation.exe" >nul
if not "%ERRORLEVEL%"=="70" goto :cleanup
call :verify "%Work%\ClientStoreValidation.exe" 63488 ec3353bc21a776fdb2970e709cf9ba1282e33d3f42e086c27054d925b2cf105f
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\ClientStoreValidation.bin" 0 "%Work%\ClientStoreValidation.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientStoreValidation.elf" 69744 96f3b1fb420ac01b38c553051c88a2d9fca453d11cd417e99e8c8ae1aff6a699
if errorlevel 1 goto :cleanup
echo step=client-directory-validation item=22/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Client-Directory-Validation-Emission.wvproj" "%Work%\ClientDirectoryValidation.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientDirectoryValidation.wvb" 4544 9d04682e657cb5f3dbf2c1ce505e144458c2348c9248cb0862393b4ae143c23a
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\ClientDirectoryValidation.wvb" "%Work%\ClientDirectoryValidation.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientDirectoryValidation.wvo" 62648 c0179f7de61f6c615756534d486f379954d0e011b3f3d54e7f08e5f266fca9b4
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\ClientDirectoryValidation.bin" "%Work%\ClientDirectoryValidation.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientDirectoryValidation.bin" 62278 2fda1d7de6488a445e8419c58e890f332a7be727881c7c8ab09f723b5e95d4b8
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\ClientDirectoryValidation.bin" 0 "%Work%\ClientDirectoryValidation.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\ClientDirectoryValidation.exe" >nul
if not "%ERRORLEVEL%"=="71" goto :cleanup
call :verify "%Work%\ClientDirectoryValidation.exe" 64000 7a0b611673c9d8aeea54a3e78ea8030f67d99cfce2de4c54cb0f99fe238c30d7
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\ClientDirectoryValidation.bin" 0 "%Work%\ClientDirectoryValidation.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientDirectoryValidation.elf" 69744 60b016054f3205ad2726548b7ddc17d463179a9b7d204066addf63b1dd9c8d51
if errorlevel 1 goto :cleanup
echo step=privileged-entry item=23/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Privileged-Entry-Emission.wvproj" "%Work%\PrivilegedEntry.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\PrivilegedEntry.wvb" 5205 ea4cd3684fc0a0cc87957bbed1a57d4e8e83848182b48d113d6ebbe230c133a5
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\PrivilegedEntry.wvb" "%Work%\PrivilegedEntry.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\PrivilegedEntry.wvo" 51429 344ce7077348450390ed73fce32c44c8027c7e3c742356f33e36bd8cad4ec78c
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\PrivilegedEntry.bin" "%Work%\PrivilegedEntry.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\PrivilegedEntry.bin" 51041 f46e755ed2a76bf2a5a65b41e22edf1eef80e66653336f391f6a8bd4f6d2dbdd
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\PrivilegedEntry.bin" 0 "%Work%\PrivilegedEntry.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\PrivilegedEntry.exe" >nul
if not "%ERRORLEVEL%"=="72" goto :cleanup
call :verify "%Work%\PrivilegedEntry.exe" 52736 0361f3a1d4be66ca32455a4fc3b103bbd0453380c8d26713cefc7cc37aadc901
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\PrivilegedEntry.bin" 0 "%Work%\PrivilegedEntry.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\PrivilegedEntry.elf" 57456 4b1a3da08c2a9cd0c21d56ff44f1288dc5e92010191fdd6c53c83536b81bc6ed
if errorlevel 1 goto :cleanup
echo step=thread-timer-state item=24/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Thread-Timer-State-Emission.wvproj" "%Work%\ThreadTimer.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ThreadTimer.wvb" 2526 5341e329f3df812aa7ea81cd8505c95ddc27e3531cbda6a65b6bb3fbf0235d70
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\ThreadTimer.wvb" "%Work%\ThreadTimer.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ThreadTimer.wvo" 14482 2ef3ae4096144bc7dd45dfd1f6aecbd23ee7b45b4941bf02add3b33693c586bf
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\ThreadTimer.bin" "%Work%\ThreadTimer.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ThreadTimer.bin" 14230 97349536af08373fe7a29ebf6ef19a4a238b37f7af10fddd4cccc61861558baa
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\ThreadTimer.bin" 0 "%Work%\ThreadTimer.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\ThreadTimer.exe" >nul
if not "%ERRORLEVEL%"=="73" goto :cleanup
call :verify "%Work%\ThreadTimer.exe" 16384 7327ad985bd44588276c526ba2aac21336df53d5484be3309f48a2deb7d3ddf7
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\ThreadTimer.bin" 0 "%Work%\ThreadTimer.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ThreadTimer.elf" 20592 bc928927aa085143e3c021941edaa88e4017d801d2ee492e67a9b87d5aab87b3
if errorlevel 1 goto :cleanup
echo step=timer-activation item=25/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Timer-Activation-Emission.wvproj" "%Work%\TimerActivation.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\TimerActivation.wvb" 4446 0b95cf7586b996922129d2199bec80051253c14e15f2c263c19a65c07547fc09
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\TimerActivation.wvb" "%Work%\TimerActivation.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\TimerActivation.wvo" 46353 adabb32058a5943b0103e902a2adafc759c26e59c73d2d27df9ca35767430fb3
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\TimerActivation.bin" "%Work%\TimerActivation.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\TimerActivation.bin" 45965 6ec505bc781ab84bbcb458c24f03e5467a84d87cbf340c1d49bcf6ca1125d850
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\TimerActivation.bin" 0 "%Work%\TimerActivation.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\TimerActivation.exe" >nul
if not "%ERRORLEVEL%"=="74" goto :cleanup
call :verify "%Work%\TimerActivation.exe" 47616 c906ae32935fe03af5670398fb52e61284d13981b67635790ab6073edaaf725d
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\TimerActivation.bin" 0 "%Work%\TimerActivation.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\TimerActivation.elf" 53360 35f3ece863faf0eb9d93c29b8d98dfc19f209637bb0c35f5519506e6c88c6e08
if errorlevel 1 goto :cleanup
echo step=provider-user-transfer item=26/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Provider-User-Transfer-Emission.wvproj" "%Work%\ProviderTransfer.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ProviderTransfer.wvb" 3860 afc6b4cf959b85feba02abf7f4ade0dc264a7626d6330cfb5eb53ae682e09c28
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\ProviderTransfer.wvb" "%Work%\ProviderTransfer.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ProviderTransfer.wvo" 35801 3ae69310cbd48a0fd407646a22b94eaaad0268ef788262ba9c4a2f04052f98fc
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\ProviderTransfer.bin" "%Work%\ProviderTransfer.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ProviderTransfer.bin" 35363 7da4dbf2f9d6ab02665db4178b4bbdaa2e14133bd627a4d1e696c73ac521896c
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\ProviderTransfer.bin" 0 "%Work%\ProviderTransfer.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\ProviderTransfer.exe" >nul
if not "%ERRORLEVEL%"=="75" goto :cleanup
call :verify "%Work%\ProviderTransfer.exe" 37376 df51b14ee5f4845a3962333235abba10ea59f64fd21bb3df98965c932538d768
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\ProviderTransfer.bin" 0 "%Work%\ProviderTransfer.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ProviderTransfer.elf" 41072 408af2b3e7d77567424a5aafe93eb0c2b185b13d60071e36e6181bb2e125f007
if errorlevel 1 goto :cleanup
echo step=provider-return-init-transfer item=27/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Provider-Return-Init-Transfer-Emission.wvproj" "%Work%\ProviderReturn.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ProviderReturn.wvb" 3645 4f7ba1ef897096f9ae461539edde3f67f5fc2754fc2068533796ed35b6d72e18
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\ProviderReturn.wvb" "%Work%\ProviderReturn.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ProviderReturn.wvo" 26095 c0b76893649bdbb48145160e05cdf830606583bf27b45f6cb7e25a60f9ccd893
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\ProviderReturn.bin" "%Work%\ProviderReturn.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ProviderReturn.bin" 25607 a1d715cd3e8dd3c74305aaeb1f7a3465b5e1178161665a6664a9acc8523c255f
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\ProviderReturn.bin" 0 "%Work%\ProviderReturn.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\ProviderReturn.exe" >nul
if not "%ERRORLEVEL%"=="76" goto :cleanup
call :verify "%Work%\ProviderReturn.exe" 27648 36ba9a985fd48c19dcca036d88ee0dde1c8dde33b426c8582e8d7788a817931c
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\ProviderReturn.bin" 0 "%Work%\ProviderReturn.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ProviderReturn.elf" 32880 e6ad54583fbcdb6f5c020a1748caa02dd037af37522b46ea7fe149620662130f
if errorlevel 1 goto :cleanup
echo step=init-return-program-validation item=28/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Init-Return-Program-Validation-Emission.wvproj" "%Work%\InitReturn.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\InitReturn.wvb" 3198 6c2bf662aa5156f525b21a011753174816c63526db82894032cec825cca0155f
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\InitReturn.wvb" "%Work%\InitReturn.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\InitReturn.wvo" 21983 5e5aa891bec04624daed3562bfc37095611b1cd8054ecf0271e62b572f207e38
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\InitReturn.bin" "%Work%\InitReturn.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\InitReturn.bin" 21563 dda94557c1ed9f192d207c0e2cde4f7178115f809fc8f450f5c357397ef04a32
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\InitReturn.bin" 0 "%Work%\InitReturn.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\InitReturn.exe" >nul
if not "%ERRORLEVEL%"=="77" goto :cleanup
call :verify "%Work%\InitReturn.exe" 23552 e72ef5b51f5b2adc6e3b26cd1983953ad96f28a4eeb9bc2490fe540106e8dff9
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\InitReturn.bin" 0 "%Work%\InitReturn.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\InitReturn.elf" 28784 d044ea33bb2a76f23ff8e621bd4ce5004afd53970d3b9cc07b96008afb566a93
if errorlevel 1 goto :cleanup
echo step=init-return-budget-validation item=29/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Init-Return-Budget-Validation-Emission.wvproj" "%Work%\BudgetValidation.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\BudgetValidation.wvb" 3019 947de07e02a5abeb8424f71ddb32188f1b1698a60a9a0f69eafdf05bb60e6940
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\BudgetValidation.wvb" "%Work%\BudgetValidation.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\BudgetValidation.wvo" 21805 f5105e4ff72996d94ffff9ab89ebd1b75a1568f61f1b1bcbb7cf39a0d8240c7f
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\BudgetValidation.bin" "%Work%\BudgetValidation.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\BudgetValidation.bin" 21385 f6894fd05f1a4a18e3312e5d31568c14ff3e718e911969148944adf6f5bc2f00
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\BudgetValidation.bin" 0 "%Work%\BudgetValidation.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\BudgetValidation.exe" >nul
if not "%ERRORLEVEL%"=="78" goto :cleanup
call :verify "%Work%\BudgetValidation.exe" 23040 ea17483f317867ed5162ca463ff54622f169a331efa4028fb00eff47a2693e9e
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\BudgetValidation.bin" 0 "%Work%\BudgetValidation.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\BudgetValidation.elf" 28784 eadb2564cbeebee10cfd58cca34378b65adaeb9ea6f8cf5832abfbf02d23bc9c
if errorlevel 1 goto :cleanup
echo step=init-return-store-directory-validation item=30/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Init-Return-Store-Directory-Validation-Emission.wvproj" "%Work%\StoreDirectoryValidation.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\StoreDirectoryValidation.wvb" 3267 296e72a6601c2364b1ad6215f69a7a3ac9f69b85ebcedc458e4524c7decaa05e
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\StoreDirectoryValidation.wvb" "%Work%\StoreDirectoryValidation.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\StoreDirectoryValidation.wvo" 22043 238b934e9c701cac294edbfefef99595e6222212959b09efee6712e63c2b74f2
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\StoreDirectoryValidation.bin" "%Work%\StoreDirectoryValidation.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\StoreDirectoryValidation.bin" 21623 f9a6bbacfe87f5169f174dcb6d7ccc33e4a64c432f3e68aba622313c27d05436
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\StoreDirectoryValidation.bin" 0 "%Work%\StoreDirectoryValidation.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\StoreDirectoryValidation.exe" >nul
if not "%ERRORLEVEL%"=="79" goto :cleanup
call :verify "%Work%\StoreDirectoryValidation.exe" 23552 ce5ebb7565f1bd26a3648f8d735d0646b3d9450b8ff890b322f5eaebc0710fe5
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\StoreDirectoryValidation.bin" 0 "%Work%\StoreDirectoryValidation.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\StoreDirectoryValidation.elf" 28784 04c59fa6379fe7f833450b1d56fff045ca5fbc7a2f62f8f319a2eccde5d89c2c
if errorlevel 1 goto :cleanup
echo step=client-user-transfer item=31/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Client-User-Transfer-Emission.wvproj" "%Work%\ClientTransfer.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientTransfer.wvb" 3861 396c95aacd156af86f6b56d2461a255de115cd5292267035a7d4e5ae4f2ea8a1
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\ClientTransfer.wvb" "%Work%\ClientTransfer.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientTransfer.wvo" 35804 dd434a07b5c4bae410c80789be9d1e29010c7575b8544ef3329e35501d46e73e
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\ClientTransfer.bin" "%Work%\ClientTransfer.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientTransfer.bin" 35366 f02d2b9b7d84e8571917d2f4d0bdf7145a73e55c8886937702aacc8862482b54
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\ClientTransfer.bin" 0 "%Work%\ClientTransfer.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\ClientTransfer.exe" >nul
if not "%ERRORLEVEL%"=="80" goto :cleanup
call :verify "%Work%\ClientTransfer.exe" 37376 67eb2ded3d5b168c75b6b8300b30e026f5bf46110fed521f517277e69522effc
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\ClientTransfer.bin" 0 "%Work%\ClientTransfer.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientTransfer.elf" 41072 4ad53750962c1363f1c1460253c0d917139e581efac4df436c4979c4f60e82d4
if errorlevel 1 goto :cleanup
echo step=client-return-init-transfer item=32/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Client-Return-Init-Transfer-Emission.wvproj" "%Work%\ClientReturn.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientReturn.wvb" 3813 96456642337d7eaf7ef4c8c497eb3f262fd6722ac71f0efe18b6ee9e12f84950
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\ClientReturn.wvb" "%Work%\ClientReturn.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientReturn.wvo" 26675 f3717d0358c12a4db8c325df347f0d260135145b64bac7372ae21c4f75713756
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\ClientReturn.bin" "%Work%\ClientReturn.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientReturn.bin" 26187 197aebc42566bec11f51059100feabd063ada5e7fc5df4b5d53bdf45ba3d3749
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\ClientReturn.bin" 0 "%Work%\ClientReturn.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\ClientReturn.exe" >nul
if not "%ERRORLEVEL%"=="81" goto :cleanup
call :verify "%Work%\ClientReturn.exe" 28160 cf8fe7a00c3b5bd183bedb6a9878d8f1d2aad32f83d98fecf7700b3a3d10c553
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\ClientReturn.bin" 0 "%Work%\ClientReturn.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientReturn.elf" 32880 b2a09c2ee5aaa2255866247c6ca5ba22ca5526f3dd2dedce9c67832cc89f2213
if errorlevel 1 goto :cleanup
echo step=init-reply-publish-resume item=33/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Init-Reply-Publish-Resume-Emission.wvproj" "%Work%\InitReply.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\InitReply.wvb" 3816 8f23f7f711f25908c4910ed5de9b2c4097d28d0ae6c1fdc57a0cbffb6cf5c92b
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\InitReply.wvb" "%Work%\InitReply.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\InitReply.wvo" 26680 c0c2538008b7b168488c72be3017369f14f818eb76d1d5d0ca23b71a82b2b9f7
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\InitReply.bin" "%Work%\InitReply.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\InitReply.bin" 26192 9eb958555632b5cc9fdb178a9013c8f73bf3e3c8563a9fea3c21dbbecbabddeb
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\InitReply.bin" 0 "%Work%\InitReply.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\InitReply.exe" >nul
if not "%ERRORLEVEL%"=="82" goto :cleanup
call :verify "%Work%\InitReply.exe" 28160 68370c479947101120bc84ef6e910aa9b1b8d3f74f42d201934a8144c25f38d2
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\InitReply.bin" 0 "%Work%\InitReply.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\InitReply.elf" 32880 72993c6a4b09d484d56d414b8d41b0ec900c748ce20314eb4a57bee8763c1f4c
if errorlevel 1 goto :cleanup
echo step=client-reply-delivery item=34/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Client-Reply-Delivery-Emission.wvproj" "%Work%\ReplyDelivery.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ReplyDelivery.wvb" 3806 668972466f58918a5d13930fdce2ff160d56d25d45907cd0d17214b2689cf44f
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\ReplyDelivery.wvb" "%Work%\ReplyDelivery.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ReplyDelivery.wvo" 26675 49e5010ba3301cb265b8dccc2bdae2527e2037229fb349c7e4ca13f7b26592a1
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\ReplyDelivery.bin" "%Work%\ReplyDelivery.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ReplyDelivery.bin" 26187 304559d8922d82f3544a5c446e6d5a14f160a12f3951166948992f0935e71b50
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\ReplyDelivery.bin" 0 "%Work%\ReplyDelivery.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\ReplyDelivery.exe" >nul
if not "%ERRORLEVEL%"=="83" goto :cleanup
call :verify "%Work%\ReplyDelivery.exe" 28160 11d820588e286c52ea4c6374a5e99c80c5803ed1840a9bce9833053fd9b4baa5
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\ReplyDelivery.bin" 0 "%Work%\ReplyDelivery.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ReplyDelivery.elf" 32880 1bce868f6b93bfafc8433cf501678736bef090a2511dd7c835eef9f8be6733c2
if errorlevel 1 goto :cleanup
echo step=client-directory-request-delivery item=35/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Client-Directory-Request-Delivery-Emission.wvproj" "%Work%\DirectoryRequest.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\DirectoryRequest.wvb" 3819 a7df225a45ad90cd6667ddbcea1c8005fb0f9f66fab8b48d5eb5b33d60be1a66
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\DirectoryRequest.wvb" "%Work%\DirectoryRequest.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\DirectoryRequest.wvo" 26675 9cd740d089580da8598c067dad3808c1705c4b35675e87bd7febd168191e04f4
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\DirectoryRequest.bin" "%Work%\DirectoryRequest.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\DirectoryRequest.bin" 26187 4c4ab3e2b288a9e9fcf40e20f616b4582b31ff63bcc2cc8b5163bd5b5cd67621
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\DirectoryRequest.bin" 0 "%Work%\DirectoryRequest.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\DirectoryRequest.exe" >nul
if not "%ERRORLEVEL%"=="84" goto :cleanup
call :verify "%Work%\DirectoryRequest.exe" 28160 5ccda5639fa6b8350bcac8b64cd1c54f144463962932f16198ea8f31c6c4da88
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\DirectoryRequest.bin" 0 "%Work%\DirectoryRequest.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\DirectoryRequest.elf" 32880 cc01218d71949cf91c0088b584e5ea931e656fe8fce71307fc2c5394e2802f46
if errorlevel 1 goto :cleanup
echo step=directory-reply-publish-resume item=36/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Directory-Reply-Publish-Resume-Emission.wvproj" "%Work%\DirectoryReply.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\DirectoryReply.wvb" 3821 025112005cf1f4be915800b7ca53852ffbee06ee3abeb7d58b5143ecfdc55976
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\DirectoryReply.wvb" "%Work%\DirectoryReply.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\DirectoryReply.wvo" 26680 9460d10ed4dc0ac79608626f3e5dc9ecc77baed737497ac1485645be3b187500
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\DirectoryReply.bin" "%Work%\DirectoryReply.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\DirectoryReply.bin" 26192 ee71fee9030f70de743a14eaed01c25284816c43b1ce44181ad16c4a4988feb6
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\DirectoryReply.bin" 0 "%Work%\DirectoryReply.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\DirectoryReply.exe" >nul
if not "%ERRORLEVEL%"=="85" goto :cleanup
call :verify "%Work%\DirectoryReply.exe" 28160 7f6bf894790451deaa44c6da79431164b7263e129d615f2cf6a6ebad0daff22c
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\DirectoryReply.bin" 0 "%Work%\DirectoryReply.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\DirectoryReply.elf" 32880 b59095e5d2aeecb446bc702960a6996bfa14f9844e5fed112d962b72aa7d68f9
if errorlevel 1 goto :cleanup
echo step=client-directory-reply-delivery item=37/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Client-Directory-Reply-Delivery-Emission.wvproj" "%Work%\ClientDirectoryReply.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientDirectoryReply.wvb" 3817 c4a7768ab78055fb79a299e4208da0e98c64513f29888308bf9a84e6cfef8bfa
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\ClientDirectoryReply.wvb" "%Work%\ClientDirectoryReply.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientDirectoryReply.wvo" 26675 67e95c51c89e487e4b9f19c1608f803f50d766cf2ef0d7fee848393141fcb800
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\ClientDirectoryReply.bin" "%Work%\ClientDirectoryReply.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientDirectoryReply.bin" 26187 e6ca3c27c2812103e84c689ee01975cdda5f68109d42a448e54312b93b378c36
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\ClientDirectoryReply.bin" 0 "%Work%\ClientDirectoryReply.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\ClientDirectoryReply.exe" >nul
if not "%ERRORLEVEL%"=="86" goto :cleanup
call :verify "%Work%\ClientDirectoryReply.exe" 28160 c48b5fa577d9f715be43e69d4d22fa7374700e602867636ffd29c0663b3d32a3
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\ClientDirectoryReply.bin" 0 "%Work%\ClientDirectoryReply.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientDirectoryReply.elf" 32880 8007f75463e70308075dd76ae5fdfd0adee0d4a2a30118e8978ec22cc432fce6
if errorlevel 1 goto :cleanup
echo step=client-completion-cleanup item=38/38
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Client-Completion-Cleanup-Emission.wvproj" "%Work%\ClientCleanup.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientCleanup.wvb" 4541 36b58e50809e26264419c1fca7e429b337fb08f71f4da91fc5a9887cb05306e2
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\ClientCleanup.wvb" "%Work%\ClientCleanup.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientCleanup.wvo" 23395 eee4dad46947de922bfd45b514a2eba5c256222278ed37b975d2847abdaad1c0
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\ClientCleanup.bin" "%Work%\ClientCleanup.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientCleanup.bin" 22975 5cb1bb0098b987d31867d4af7990b8dfc9bec7bdbd2ce8d6e05538394e95a91d
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\ClientCleanup.bin" 0 "%Work%\ClientCleanup.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\ClientCleanup.exe" >nul
if not "%ERRORLEVEL%"=="87" goto :cleanup
call :verify "%Work%\ClientCleanup.exe" 25088 d231f32c41c0ef2d7493180edfeb3edb8f04aed8b1ccb1d5711b8772b0fc28eb
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\ClientCleanup.bin" 0 "%Work%\ClientCleanup.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientCleanup.elf" 28784 78130c20bab66eb81a42700f6a0c77c89db51457e56c808d674e7f7b1a9e495a
if errorlevel 1 goto :cleanup
echo step=client-reclamation-preflight item=39/39
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Client-Reclamation-Preflight-Emission.wvproj" "%Work%\ClientReclamationPreflight.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientReclamationPreflight.wvb" 5489 de9965e67eb1a0607567d4506ca8569083ef025244501b855a786cee37d781c2
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\ClientReclamationPreflight.wvb" "%Work%\ClientReclamationPreflight.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientReclamationPreflight.wvo" 26770 0a8c3132e27a04d24eb4611cbc6850f3e5706d24f4f8d0b5e63637a56ba367df
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\ClientReclamationPreflight.bin" "%Work%\ClientReclamationPreflight.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientReclamationPreflight.bin" 26282 710c3e5954f04ad8b344b8828c6b3e5cf37cc0c3e947e966df8ec55e9fca75ef
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\ClientReclamationPreflight.bin" 0 "%Work%\ClientReclamationPreflight.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\ClientReclamationPreflight.exe" >nul
if not "%ERRORLEVEL%"=="88" goto :cleanup
call :verify "%Work%\ClientReclamationPreflight.exe" 28160 ec6e05d9b84fa9364a18c6e423a9966eb961fcddfb8252fbaeab9b36ddfb2859
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\ClientReclamationPreflight.bin" 0 "%Work%\ClientReclamationPreflight.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientReclamationPreflight.elf" 32880 2098fc92bf5a58448256beefb52207fda7aa974941b26567ee9941df496e0ed1
if errorlevel 1 goto :cleanup
echo step=client-memory-recycle item=40/40
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Client-Memory-Recycle-Emission.wvproj" "%Work%\ClientMemoryRecycle.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientMemoryRecycle.wvb" 4205 6d43607fde70e4debb388d504d5197f5810377958917ca49c10d31bf3988907d
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\ClientMemoryRecycle.wvb" "%Work%\ClientMemoryRecycle.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientMemoryRecycle.wvo" 34800 a4a98e1a839f6423f7bfb8b37a9a419725b91a7f5523f080dc46defb98d9ec8b
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\ClientMemoryRecycle.bin" "%Work%\ClientMemoryRecycle.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientMemoryRecycle.bin" 34312 159a99df77bee31b100b2aacfd5e659b24aa1134a7a239f5d615ecb3af01b310
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\ClientMemoryRecycle.bin" 0 "%Work%\ClientMemoryRecycle.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\ClientMemoryRecycle.exe" >nul
if not "%ERRORLEVEL%"=="89" goto :cleanup
call :verify "%Work%\ClientMemoryRecycle.exe" 36352 bd784204bcb993dd642d1122038af4add3efaf0b68dd695c57cf5be5b7bc402c
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\ClientMemoryRecycle.bin" 0 "%Work%\ClientMemoryRecycle.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientMemoryRecycle.elf" 41072 5215b6db9ae314e336946db9af0be94a1b83be919a6adde2404e064053cdb315
if errorlevel 1 goto :cleanup
echo step=client-generation-two-record item=41/41
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Client-Generation-Two-Record-Emission.wvproj" "%Work%\ClientGenerationTwoRecord.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientGenerationTwoRecord.wvb" 2246 408a51f39da581efc0ece5c54ba34207c553c82186cc218ae98c64e2a3b30030
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\ClientGenerationTwoRecord.wvb" "%Work%\ClientGenerationTwoRecord.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientGenerationTwoRecord.wvo" 14191 7355afeb166ae3502a7f6f33cb213060b09568b1e79fddd390757e3ac8f118c6
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\ClientGenerationTwoRecord.bin" "%Work%\ClientGenerationTwoRecord.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientGenerationTwoRecord.bin" 13939 64ed64bf5378380c8300f3577031f212602b10bbfd88fbec79f9054a12289241
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\ClientGenerationTwoRecord.bin" 0 "%Work%\ClientGenerationTwoRecord.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\ClientGenerationTwoRecord.exe" >nul
if not "%ERRORLEVEL%"=="90" goto :cleanup
call :verify "%Work%\ClientGenerationTwoRecord.exe" 15872 9ad20271c35b181ead51fd1ff3d84e3d8f83cf44183092c601c72f81a644b85c
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\ClientGenerationTwoRecord.bin" 0 "%Work%\ClientGenerationTwoRecord.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientGenerationTwoRecord.elf" 20592 aa3700448dfb9d450afc48c64ed20d49f19ad780efe8ecd29031ecbdbba2c7b2
if errorlevel 1 goto :cleanup
echo step=client-generation-two-paging item=42/42
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Client-Generation-Two-Paging-Emission.wvproj" "%Work%\ClientGenerationTwoPaging.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientGenerationTwoPaging.wvb" 14544 f7e189d04bdf740c5c1b2224c5872a2e3c0159e6408dd4de69dbd6ab3a1db9f2
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\ClientGenerationTwoPaging.wvb" "%Work%\ClientGenerationTwoPaging.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientGenerationTwoPaging.wvo" 206347 1b56fd5301900cdcc756da7c84af1bec5ff4f509363806d4c9c58ad3e2b1448d
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\ClientGenerationTwoPaging.bin" "%Work%\ClientGenerationTwoPaging.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientGenerationTwoPaging.bin" 204475 443cd01a8a604ff67e41d52a7f52fe8821dd8ae9dbe54708756e98e0c362a087
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\ClientGenerationTwoPaging.bin" 0 "%Work%\ClientGenerationTwoPaging.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\ClientGenerationTwoPaging.exe" >nul
if not "%ERRORLEVEL%"=="91" goto :cleanup
call :verify "%Work%\ClientGenerationTwoPaging.exe" 206336 62c74695812eec852cf3dddee37cec39596da28397e0f2abdbaf28e8475119c2
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\ClientGenerationTwoPaging.bin" 0 "%Work%\ClientGenerationTwoPaging.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientGenerationTwoPaging.elf" 209008 bae80f3e33d79d1446cd54af10c066da7d2bcdb2e95f74ddd4b32eab2d9a1511
if errorlevel 1 goto :cleanup
echo step=client-generation-two-image item=43/43
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-X64-Process-Client-Generation-Two-Image-Emission.wvproj" "%Work%\ClientGenerationTwoImage.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientGenerationTwoImage.wvb" 13762 8758de24cc2954212d55bedab76d3746cfb584313bd455e11f2a0461fba40b1e
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\ClientGenerationTwoImage.wvb" "%Work%\ClientGenerationTwoImage.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientGenerationTwoImage.wvo" 187483 1847ff8d4263ca70bf6d8e165fb1e09bf7f9621a58ef94c8d238b3b1f759d436
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\ClientGenerationTwoImage.bin" "%Work%\ClientGenerationTwoImage.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientGenerationTwoImage.bin" 185809 1e9357d1626073ab7b2148c5c0368cc977449b691f09180339058c923be86a95
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\ClientGenerationTwoImage.bin" 0 "%Work%\ClientGenerationTwoImage.exe" >nul
if errorlevel 1 goto :cleanup
call "%Work%\ClientGenerationTwoImage.exe" >nul
if not "%ERRORLEVEL%"=="92" goto :cleanup
call :verify "%Work%\ClientGenerationTwoImage.exe" 187904 a78e65b9424e4a10dc65cbf0f5cf5268a3ec04b39bdac854023b9fe35fb46386
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\ClientGenerationTwoImage.bin" 0 "%Work%\ClientGenerationTwoImage.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\ClientGenerationTwoImage.elf" 192624 fb2d1a9b64f06b8068602e05fe23d710c0e8e45ae1ff06803489c8f31bc6ad4e
if errorlevel 1 goto :cleanup
echo native os x64 code emission status=Passed projects=43 cases=258 local-results=50/51/52/53/54/55/56/57/58/59/60/61/62/63/64/65/66/67/68/69/70/71/72/73/74/75/76/77/78/79/80/81/82/83/84/85/86/87/88/89/90/91/92 cross-host-images=Verified source-owned-bytes=25065 relocation-fields=121
set "Status=0"
:cleanup
if exist "%Work%\." rmdir /s /q "%Work%"
exit /b %Status%
