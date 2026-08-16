[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$OutputDirectory
)

$ErrorActionPreference = 'Stop'
$RepositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$OutputRoot = (Resolve-Path -LiteralPath $OutputDirectory).Path
if (!(Test-Path -LiteralPath $OutputRoot -PathType Container)) {
    throw 'The native Seed front-door output directory must already exist.'
}

$NativeBuild = Join-Path $RepositoryRoot 'Tools/Native/Build-Wvb.cmd'
$NativeSourceCompilerBuild = Join-Path $RepositoryRoot 'Tools/Native/Build-Source-Compiler-Product.cmd'
$NativeVerify = Join-Path $RepositoryRoot 'Tools/Native/Verify-Wvb.cmd'
$NativeInspect = Join-Path $RepositoryRoot 'Tools/Native/Inspect-Wvb.cmd'
$NativeRun = Join-Path $RepositoryRoot 'Tools/Native/Run-Wvb.cmd'
$NativeAssembler = Join-Path $RepositoryRoot 'Tools/Native/Assemble-Wva.cmd'
$NativeWvoVerify = Join-Path $RepositoryRoot 'Tools/Native/Verify-Wvo.cmd'
$NativeWvoInspect = Join-Path $RepositoryRoot 'Tools/Native/Inspect-Wvo.cmd'
$NativeWvoApplication = Join-Path $RepositoryRoot 'Artifacts/Native-Wvo-Object-Candidate/Wvo-Object.exe'
$NativeWvDumpApplication = Join-Path $RepositoryRoot 'Artifacts/Native-Front-Door/windows-x64/wvdump.exe'
$NativeWvaApplication = Join-Path $RepositoryRoot 'Artifacts/Native-Front-Door/windows-x64/wvasm.exe'
$NativeLinker = Join-Path $RepositoryRoot 'Tools/Native/Link-Wvo.cmd'
$NativeLinkerApplication = Join-Path $RepositoryRoot 'Artifacts/Native-Wv-Linker-Candidate/Wv-Linker.exe'

function Invoke-ExactBuild(
    [string]$ProjectPath,
    [string]$OutputPath,
    [long]$ExpectedBytes,
    [string]$ExpectedSha256,
    [string]$ExpectedBuildReport
) {
    $BuildOutput = @(& $NativeBuild $ProjectPath $OutputPath 2>&1)
    $BuildExit = $LASTEXITCODE
    $ExpectedPublicationReport =
        "publication status=Complete bytes=0x$($ExpectedBytes.ToString('x8')) sha256=$ExpectedSha256"
    if (
        $BuildExit -ne 0 -or
        $BuildOutput.Count -ne 2 -or
        $BuildOutput[0] -ne $ExpectedBuildReport -or
        $BuildOutput[1] -ne $ExpectedPublicationReport
    ) {
        throw "The native Seed project build failed: $ProjectPath"
    }

    $Information = Get-Item -LiteralPath $OutputPath
    $Digest = (Get-FileHash -Algorithm SHA256 -LiteralPath $OutputPath).Hash.ToLowerInvariant()
    if ($Information.Length -ne $ExpectedBytes -or $Digest -ne $ExpectedSha256) {
        throw "The native Seed project build produced an unexpected module: $OutputPath"
    }
}

function Invoke-ExactSourceCompilerBuild(
    [string]$Product,
    [string]$OutputPath,
    [long]$ExpectedBytes,
    [string]$ExpectedSha256,
    [string]$ExpectedCompilerReport
) {
    $BuildOutput = @(& $NativeSourceCompilerBuild $Product $OutputPath 2>&1)
    $BuildExit = $LASTEXITCODE
    $ExpectedPublicationReport =
        "publication status=Complete bytes=0x$($ExpectedBytes.ToString('x8')) sha256=$ExpectedSha256"
    if (
        $BuildExit -ne 0 -or
        $BuildOutput.Count -ne 2 -or
        $BuildOutput[0] -ne $ExpectedCompilerReport -or
        $BuildOutput[1] -ne $ExpectedPublicationReport
    ) {
        throw "The native source compiler $Product product did not reproduce its exact report."
    }

    $Information = Get-Item -LiteralPath $OutputPath
    $Digest = (Get-FileHash -Algorithm SHA256 -LiteralPath $OutputPath).Hash.ToLowerInvariant()
    if ($Information.Length -ne $ExpectedBytes -or $Digest -ne $ExpectedSha256) {
        throw "The native source compiler $Product product has an unexpected identity."
    }
}

function Invoke-ExactVerify([string]$ModulePath) {
    $VerifyOutput = @(& $NativeVerify $ModulePath 2>&1)
    if (
        $LASTEXITCODE -ne 0 -or
        $VerifyOutput.Count -ne 1 -or
        $VerifyOutput[0] -ne 'wvb status=Valid profile=compiler-aligned'
    ) {
        throw "The native Seed verifier rejected or misreported: $ModulePath"
    }
}

function Invoke-ExactInspect(
    [string]$ModulePath,
    [string[]]$RequiredPatterns,
    [string[]]$ForbiddenPatterns = @()
) {
    $InspectOutput = @(& $NativeInspect $ModulePath 2>&1)
    if ($LASTEXITCODE -ne 0) {
        throw "The native Seed inspector omitted required evidence: $ModulePath"
    }
    $Inspection = $InspectOutput -join "`n"
    foreach ($RequiredPattern in $RequiredPatterns) {
        if ($Inspection -notmatch $RequiredPattern) {
            throw "The native Seed inspector omitted required evidence: $ModulePath"
        }
    }
    foreach ($ForbiddenPattern in $ForbiddenPatterns) {
        if ($Inspection -match $ForbiddenPattern) {
            throw "The native Seed inspector exposed forbidden evidence: $ModulePath"
        }
    }
}

function Invoke-ExactRun(
    [string]$ModulePath,
    [int]$ExpectedResult,
    [long]$ExpectedBytes,
    [string]$ExpectedSha256
) {
    $RunOutput = @(& $NativeRun $ModulePath 2>&1)
    if (
        $LASTEXITCODE -ne 0 -or
        $RunOutput.Count -ne 1 -or
        $RunOutput[0].ToString() -ne "Result: $ExpectedResult"
    ) {
        throw "The native Seed runner rejected or misreported: $ModulePath"
    }
    $Information = Get-Item -LiteralPath $ModulePath
    $Digest = (Get-FileHash -Algorithm SHA256 -LiteralPath $ModulePath).Hash.ToLowerInvariant()
    if ($Information.Length -ne $ExpectedBytes -or $Digest -ne $ExpectedSha256) {
        throw "The native Seed runner modified its input module: $ModulePath"
    }
}

function Invoke-ExactInstructionReport(
    [string]$ModulePath,
    [int]$ExpectedResult,
    [int]$ExpectedInstructions,
    [long]$ExpectedBytes,
    [string]$ExpectedSha256
) {
    $RunOutput = @(& $NativeRun $ModulePath --report-steps 2>&1)
    if (
        $LASTEXITCODE -ne 0 -or
        $RunOutput.Count -ne 2 -or
        $RunOutput[0].ToString() -ne "Result: $ExpectedResult" -or
        $RunOutput[1].ToString() -ne "Instructions: $ExpectedInstructions"
    ) {
        throw "The native Seed runner instruction report is invalid: $ModulePath"
    }
    $Information = Get-Item -LiteralPath $ModulePath
    $Digest = (Get-FileHash -Algorithm SHA256 -LiteralPath $ModulePath).Hash.ToLowerInvariant()
    if ($Information.Length -ne $ExpectedBytes -or $Digest -ne $ExpectedSha256) {
        throw "The native Seed runner modified its reported input module: $ModulePath"
    }
}

function Invoke-ExactWvDumpExecution(
    [string]$SumPath,
    [string]$InvalidPath
) {
    $ApplicationInformation = Get-Item -LiteralPath $NativeWvDumpApplication
    $ApplicationDigest = (
        Get-FileHash -Algorithm SHA256 -LiteralPath $NativeWvDumpApplication
    ).Hash.ToLowerInvariant()
    if (
        $ApplicationInformation.Length -ne 795136 -or
        $ApplicationDigest -ne '61512dae2941607b93da7d29dd59f973c690f0fec3ba24f772f2101c87ed5381'
    ) {
        throw 'The Windows native WvDump application identity is invalid.'
    }
    $SelfTestOutput = @(& $NativeWvDumpApplication 2>&1)
    if ($LASTEXITCODE -ne 0 -or $SelfTestOutput.Count -ne 0) {
        throw 'The digest-bound native WvDump self-test failed.'
    }

    $SumDigest = (Get-FileHash -Algorithm SHA256 -LiteralPath $SumPath).Hash
    $ReportOutput = @(& $NativeWvDumpApplication $SumPath 2>&1)
    $ReportExit = $LASTEXITCODE
    $Report = $ReportOutput -join "`n"
    foreach ($Pattern in @(
        '(?m)^wvdump 1$',
        'module version=1\.11 profile=portable name="Sum\\u02C9data"',
        'data index=0 name="Values" type=i32_array elements=4',
        'instruction function=1 offset=141 opcode=call operand=0',
        'export index=0 name="Main" kind=function target=1'
    )) {
        if ($Report -notmatch $Pattern) {
            throw 'The digest-bound native WvDump report omitted required evidence.'
        }
    }
    if (
        $ReportExit -ne 0 -or
        (Get-FileHash -Algorithm SHA256 -LiteralPath $SumPath).Hash -ne $SumDigest
    ) {
        throw 'The digest-bound native WvDump rejected or modified the canonical module.'
    }

    $InvalidDigest = (Get-FileHash -Algorithm SHA256 -LiteralPath $InvalidPath).Hash
    $InvalidOutput = @(& $NativeWvDumpApplication $InvalidPath 2>&1)
    $InvalidExit = $LASTEXITCODE
    if (
        $InvalidExit -ne 2 -or
        $InvalidOutput.Count -ne 1 -or
        $InvalidOutput[0].ToString() -ne 'Badˉmagic sections=0 offset=0' -or
        (Get-FileHash -Algorithm SHA256 -LiteralPath $InvalidPath).Hash -ne $InvalidDigest
    ) {
        throw 'The digest-bound native WvDump invalid-file contract failed.'
    }
}

function Invoke-ExactWvoReadOnlyExecution([string]$ObjectPath) {
    $ApplicationInformation = Get-Item -LiteralPath $NativeWvoApplication
    $ApplicationDigest = (
        Get-FileHash -Algorithm SHA256 -LiteralPath $NativeWvoApplication
    ).Hash.ToLowerInvariant()
    if (
        $ApplicationInformation.Length -ne 1037312 -or
        $ApplicationDigest -ne '5362372e826958470eee7d90eb01938de5b91dcb3e1b0f952722e00578a82d03'
    ) {
        throw 'The Windows native WVO inspector application identity is invalid.'
    }
    $SelfTest = @(& $NativeWvoApplication 2>&1)
    if ($LASTEXITCODE -ne 0 -or $SelfTest.Count -ne 0) {
        throw 'The digest-bound native WVO inspector self-test failed.'
    }

    $AssemblySource = Join-Path $RepositoryRoot 'Examples/Assembler/Hello-Object.wva'
    $AssemblyOutput = @(& $NativeAssembler $AssemblySource $ObjectPath 2>&1)
    if (
        $LASTEXITCODE -ne 0 -or
        $AssemblyOutput.Count -ne 2 -or
        $AssemblyOutput[0].ToString() -ne 'wvasm 1' -or
        $AssemblyOutput[1].ToString() -ne 'assembly status=valid object-bytes=218 sections=2 symbols=3 relocations=2 offset=403 line=22 column=1'
    ) {
        throw 'The digest-bound native WVA assembler did not construct the WVO read-only fixture.'
    }
    $ObjectInformation = Get-Item -LiteralPath $ObjectPath
    $ObjectDigest = (Get-FileHash -Algorithm SHA256 -LiteralPath $ObjectPath).Hash.ToLowerInvariant()
    if (
        $ObjectInformation.Length -ne 218 -or
        $ObjectDigest -ne '992c298a4f9b68dec27b7203a2770f2a37ef2016ea45e88d33ee21994060fe85'
    ) {
        throw 'The native WVO read-only fixture has an unexpected identity.'
    }

    $VerifyOutput = @(& $NativeWvoVerify $ObjectPath 2>&1)
    if (
        $LASTEXITCODE -ne 0 -or
        $VerifyOutput.Count -ne 2 -or
        $VerifyOutput[0].ToString() -ne 'Verified object: X86ˉ64' -or
        $VerifyOutput[1].ToString() -ne "SHA-256: $ObjectDigest"
    ) {
        throw 'The digest-bound native WVO verifier report is invalid.'
    }

    $InspectOutput = @(& $NativeWvoInspect $ObjectPath 2>&1)
    $InspectExit = $LASTEXITCODE
    $Inspection = $InspectOutput -join "`n"
    foreach ($Pattern in @(
        '(?m)^Windvale object 1\.0$',
        '(?m)^Architecture: X86ˉ64$',
        'Sections \(2\)',
        'Console_write binding=Import',
        'kind=Relativeˉi32 section=0 offset=6 symbol=2 addend=-4'
    )) {
        if ($Inspection -notmatch $Pattern) {
            throw 'The digest-bound native WVO inspection omitted required evidence.'
        }
    }
    if (
        $InspectExit -ne 0 -or
        (Get-FileHash -Algorithm SHA256 -LiteralPath $ObjectPath).Hash.ToLowerInvariant() -ne $ObjectDigest
    ) {
        throw 'The digest-bound native WVO inspector rejected or modified its input.'
    }
}

function Invoke-ExactWvaAndLinkerExecution(
    [string]$ObjectPath,
    [string]$InvalidSourcePath,
    [string]$ProviderObjectPath,
    [string]$LinkedImagePath,
    [string]$LinkMapPath,
    [string]$InvalidAssemblyPath,
    [string]$InvalidLinkPath
) {
    $AssemblerInformation = Get-Item -LiteralPath $NativeWvaApplication
    $AssemblerDigest = (
        Get-FileHash -Algorithm SHA256 -LiteralPath $NativeWvaApplication
    ).Hash.ToLowerInvariant()
    if (
        $AssemblerInformation.Length -ne 2895360 -or
        $AssemblerDigest -ne '40a35687fb052dcd4f6d3a767436f4024d91bd5f03890b30fa4f0300184a35ed'
    ) {
        throw 'The Windows native WVA assembler application identity is invalid.'
    }
    $AssemblerSelfTest = @(& $NativeWvaApplication 2>&1)
    if ($LASTEXITCODE -ne 0 -or $AssemblerSelfTest.Count -ne 0) {
        throw 'The digest-bound native WVA assembler self-test failed.'
    }

    $LinkerInformation = Get-Item -LiteralPath $NativeLinkerApplication
    $LinkerDigest = (
        Get-FileHash -Algorithm SHA256 -LiteralPath $NativeLinkerApplication
    ).Hash.ToLowerInvariant()
    if (
        $LinkerInformation.Length -ne 1796608 -or
        $LinkerDigest -ne 'f47a952867203fbff53abb131ea155b4fe9e14a8be153cc61c0ca5fd8e4a74e0'
    ) {
        throw 'The Windows native WVO linker application identity is invalid.'
    }
    $LinkerSelfTest = @(& $NativeLinkerApplication 2>&1)
    if ($LASTEXITCODE -ne 0 -or $LinkerSelfTest.Count -ne 0) {
        throw 'The digest-bound native WVO linker self-test failed.'
    }

    $ObjectDigest = (Get-FileHash -Algorithm SHA256 -LiteralPath $ObjectPath).Hash.ToLowerInvariant()
    $ScannerOutput = @(& $NativeLinkerApplication $ObjectPath 2>&1)
    if (
        $LASTEXITCODE -ne 0 -or
        $ScannerOutput.Count -ne 1 -or
        $ScannerOutput[0].ToString() -ne 'object status=Valid sections=2 symbols=3 relocations=2 offset=218' -or
        (Get-FileHash -Algorithm SHA256 -LiteralPath $ObjectPath).Hash.ToLowerInvariant() -ne $ObjectDigest
    ) {
        throw 'The digest-bound native WVO linker scanner rejected or modified the canonical object.'
    }
    $InvalidSourceDigest = (
        Get-FileHash -Algorithm SHA256 -LiteralPath $InvalidSourcePath
    ).Hash.ToLowerInvariant()
    $InvalidScannerOutput = @(& $NativeLinkerApplication $InvalidSourcePath 2>&1)
    if (
        $LASTEXITCODE -ne 2 -or
        $InvalidScannerOutput.Count -ne 1 -or
        $InvalidScannerOutput[0].ToString() -ne 'object status=Badˉmagic sections=0 symbols=0 relocations=0 offset=0' -or
        (Get-FileHash -Algorithm SHA256 -LiteralPath $InvalidSourcePath).Hash.ToLowerInvariant() -ne $InvalidSourceDigest
    ) {
        throw 'The digest-bound native WVO linker scanner invalid-file contract failed.'
    }

    if (Test-Path -LiteralPath $InvalidAssemblyPath) {
        throw "The invalid native assembly output unexpectedly exists: $InvalidAssemblyPath"
    }
    $InvalidAssemblyOutput = @(
        & $NativeWvaApplication $InvalidSourcePath $InvalidAssemblyPath 2>&1
    )
    if (
        $LASTEXITCODE -ne 2 -or
        $InvalidAssemblyOutput.Count -ne 1 -or
        $InvalidAssemblyOutput[0].ToString() -ne 'assembly status=WVA1001 object-bytes=0 sections=0 symbols=0 relocations=0 offset=0 line=1 column=1' -or
        (Test-Path -LiteralPath $InvalidAssemblyPath)
    ) {
        throw 'The digest-bound native WVA assembler created output for rejected source.'
    }
    $ExistingAssemblyOutput = @(
        & $NativeWvaApplication $InvalidSourcePath $ObjectPath 2>&1
    )
    if (
        $LASTEXITCODE -ne 2 -or
        $ExistingAssemblyOutput.Count -ne 1 -or
        $ExistingAssemblyOutput[0].ToString() -ne 'assembly status=WVA1001 object-bytes=0 sections=0 symbols=0 relocations=0 offset=0 line=1 column=1' -or
        (Get-FileHash -Algorithm SHA256 -LiteralPath $ObjectPath).Hash.ToLowerInvariant() -ne $ObjectDigest
    ) {
        throw 'Rejected native assembly modified the canonical object.'
    }

    $ProviderSource = Join-Path $RepositoryRoot 'Examples/Linker/Console-Provider.wva'
    $ProviderOutput = @(& $NativeAssembler $ProviderSource $ProviderObjectPath 2>&1)
    if (
        $LASTEXITCODE -ne 0 -or
        $ProviderOutput.Count -ne 2 -or
        $ProviderOutput[0].ToString() -ne 'wvasm 1' -or
        $ProviderOutput[1].ToString() -ne 'assembly status=valid object-bytes=91 sections=1 symbols=1 relocations=0 offset=163 line=10 column=1'
    ) {
        throw 'The digest-bound native WVA assembler did not construct the linker provider.'
    }
    $ProviderInformation = Get-Item -LiteralPath $ProviderObjectPath
    $ProviderDigest = (
        Get-FileHash -Algorithm SHA256 -LiteralPath $ProviderObjectPath
    ).Hash.ToLowerInvariant()
    if (
        $ProviderInformation.Length -ne 91 -or
        $ProviderDigest -ne '486134e34bb32abadd233d1c3303acd9c313aa69d3874cafdce0fcb61b6e72ab'
    ) {
        throw 'The native linker provider has an unexpected identity.'
    }

    $LinkOutput = @(
        & $NativeLinker 1048576 Main $LinkedImagePath $ObjectPath $ProviderObjectPath 2>&1
    )
    $LinkExit = $LASTEXITCODE
    $LinkReport = $LinkOutput -join "`n"
    foreach ($Pattern in @(
        '(?m)^windvale-link-map 1$',
        '(?m)^target name=flat-x86-64-v1 architecture=x86-64 base-address=1048576 image-bytes=24$',
        '(?m)^entry name=Main address=1048576$',
        '(?m)^image sha256=0e02d447ec379e8bc8be373694d6ca14fdde0125550cbd34ee05b3ecc63ffe9a$',
        '(?m)^import index=0 input=0 source-index=2 kind=function name=Console_write provider-input=1 provider-source-index=0 address=1048592$',
        '(?m)^relocation index=1 input=0 source-index=1 kind=absolute-u32 patch-offset=20 patch-address=1048596 target=Main target-input=0 target-source-index=1 target-address=1048576 addend=0 value=1048576$'
    )) {
        if ($LinkReport -notmatch $Pattern) {
            throw 'The digest-bound native WVO linker map omitted required evidence.'
        }
    }
    if ($LinkExit -ne 0 -or $LinkReport -match [regex]::Escape($RepositoryRoot)) {
        throw 'The digest-bound native WVO linker did not produce a path-free canonical map.'
    }
    $LinkedInformation = Get-Item -LiteralPath $LinkedImagePath
    $LinkedDigest = (
        Get-FileHash -Algorithm SHA256 -LiteralPath $LinkedImagePath
    ).Hash.ToLowerInvariant()
    if (
        $LinkedInformation.Length -ne 24 -or
        $LinkedDigest -ne '0e02d447ec379e8bc8be373694d6ca14fdde0125550cbd34ee05b3ecc63ffe9a'
    ) {
        throw 'The digest-bound native WVO linker wrote unexpected image bytes.'
    }
    [IO.File]::WriteAllText(
        $LinkMapPath,
        "$LinkReport`n",
        [Text.UTF8Encoding]::new($false))
    $LinkMapInformation = Get-Item -LiteralPath $LinkMapPath
    $LinkMapDigest = (
        Get-FileHash -Algorithm SHA256 -LiteralPath $LinkMapPath
    ).Hash.ToLowerInvariant()
    if (
        $LinkMapInformation.Length -ne 1721 -or
        $LinkMapDigest -ne '31bc6a8e90d5f3049ae3e2eb0735a901923186d6a03ed40f22762b557b2ba5f4'
    ) {
        throw 'The digest-bound native WVO linker wrote an unexpected canonical map.'
    }

    if (Test-Path -LiteralPath $InvalidLinkPath) {
        throw "The invalid native link output unexpectedly exists: $InvalidLinkPath"
    }
    $UndefinedOutput = @(
        & $NativeLinker 1048576 Main $InvalidLinkPath $ObjectPath 2>&1
    )
    if (
        $LASTEXITCODE -ne 2 -or
        $UndefinedOutput.Count -ne 1 -or
        $UndefinedOutput[0].ToString() -ne 'link status=WVL1005 inputs=1 sections=2 symbols=3 relocations=2 image-bytes=0 entry-address=0 input=0' -or
        (Test-Path -LiteralPath $InvalidLinkPath)
    ) {
        throw 'The digest-bound native WVO linker created output for an undefined import.'
    }
    $ExistingLinkOutput = @(
        & $NativeLinker 1048576 Main $LinkedImagePath $ObjectPath 2>&1
    )
    if (
        $LASTEXITCODE -ne 2 -or
        $ExistingLinkOutput.Count -ne 1 -or
        $ExistingLinkOutput[0].ToString() -ne 'link status=WVL1005 inputs=1 sections=2 symbols=3 relocations=2 image-bytes=0 entry-address=0 input=0' -or
        (Get-FileHash -Algorithm SHA256 -LiteralPath $LinkedImagePath).Hash.ToLowerInvariant() -ne $LinkedDigest
    ) {
        throw 'A rejected native WVO link modified the existing image.'
    }
}

$SumModule = Join-Path $OutputRoot 'Sum-Data.wvb'
$HelloModule = Join-Path $OutputRoot 'Hello-Windvale.wvb'
$FoundationModule = Join-Path $OutputRoot 'Read-Wvb-Header.wvb'
$CompositionModule = Join-Path $OutputRoot 'Module-Composition-Demo-Project.wvb'
$MachineContractsModule = Join-Path $OutputRoot 'Machine-Contracts.wvb'
$MachineContractsDemoModule = Join-Path $OutputRoot 'Machine-Contracts-Demo.wvb'
$ByteOrderingModule = Join-Path $OutputRoot 'Byte-Ordering.wvb'
$ByteOrderingDemoModule = Join-Path $OutputRoot 'Byte-Ordering-Demo.wvb'
$DecimalParsingModule = Join-Path $OutputRoot 'Decimal-Parsing.wvb'
$DecimalParsingDemoModule = Join-Path $OutputRoot 'Decimal-Parsing-Demo.wvb'
$ByteConstructionModule = Join-Path $OutputRoot 'Byte-Construction.wvb'
$ByteConstructionDemoModule = Join-Path $OutputRoot 'Byte-Construction-Demo.wvb'
$NativeStencilModule = Join-Path $OutputRoot 'Native-Stencil-Core.wvb'
$NativeStencilDemoModule = Join-Path $OutputRoot 'Native-Stencil-Demo.wvb'
$NativeStencilBridgeModule = Join-Path $OutputRoot 'Native-Stencil-Bridge.wvb'
$NativeUtf8CoreModule = Join-Path $OutputRoot 'Native-X64-Utf8-Service.wvb'
$NativeUtf8BridgeModule = Join-Path $OutputRoot 'Native-X64-Utf8-Service-Bridge.wvb'
$NativeIntegerFormatCoreModule = Join-Path $OutputRoot 'Native-X64-Integer-Format-Services.wvb'
$NativeIntegerFormatBridgeModule = Join-Path $OutputRoot 'Native-X64-Integer-Format-Services-Bridge.wvb'
$NativeServiceCodeBuilderModule = Join-Path $OutputRoot 'Native-X64-Service-Code-Builder.wvb'
$NativeWindowsOutputCoreModule = Join-Path $OutputRoot 'Native-X64-Output-Service-Windows.wvb'
$NativeLinuxOutputCoreModule = Join-Path $OutputRoot 'Native-X64-Output-Service-Linux.wvb'
$NativeOutputBridgeModule = Join-Path $OutputRoot 'Native-X64-Output-Services-Bridge.wvb'
$NativeFileOutputCodeModule = Join-Path $OutputRoot 'Native-X64-File-Output-Service-Code.wvb'
$NativeWindowsFileOutputCoreModule = Join-Path $OutputRoot 'Native-X64-File-Output-Service-Windows.wvb'
$NativeLinuxFileOutputCoreModule = Join-Path $OutputRoot 'Native-X64-File-Output-Service-Linux.wvb'
$NativeFileOutputBridgeModule = Join-Path $OutputRoot 'Native-X64-File-Output-Services-Bridge.wvb'
$NativeFileInputCodeModule = Join-Path $OutputRoot 'Native-X64-File-Input-Service-Code.wvb'
$NativeWindowsFileInputCoreModule = Join-Path $OutputRoot 'Native-X64-File-Input-Service-Windows.wvb'
$NativeLinuxFileInputCoreModule = Join-Path $OutputRoot 'Native-X64-File-Input-Service-Linux.wvb'
$NativeFileInputBridgeModule = Join-Path $OutputRoot 'Native-X64-File-Input-Services-Bridge.wvb'
$NativeTextConcatCoreModule = Join-Path $OutputRoot 'Native-X64-Text-Concat-Service.wvb'
$NativeTextConcatBridgeModule = Join-Path $OutputRoot 'Native-X64-Text-Concat-Service-Bridge.wvb'
$NativeTextQuoteCoreModule = Join-Path $OutputRoot 'Native-X64-Text-Quote-Service.wvb'
$NativeTextQuoteBridgeModule = Join-Path $OutputRoot 'Native-X64-Text-Quote-Service-Bridge.wvb'
$NativeEnumNameCoreModule = Join-Path $OutputRoot 'Native-X64-Enum-Name-Service.wvb'
$NativeEnumNameBridgeModule = Join-Path $OutputRoot 'Native-X64-Enum-Name-Service-Bridge.wvb'
$NativeEnumMetadataCoreModule = Join-Path $OutputRoot 'Native-Enum-Metadata-Core.wvb'
$NativeEnumMetadataBridgeModule = Join-Path $OutputRoot 'Native-Enum-Metadata-Bridge.wvb'
$NativePublicationModule = Join-Path $OutputRoot 'Native-Publication-Core.wvb'
$NativePublicationBridgeModule = Join-Path $OutputRoot 'Native-Publication-Bridge.wvb'
$NativeServiceBundleMaterializationCoreModule = Join-Path $OutputRoot 'Native-Service-Bundle-Materialization-Core.wvb'
$NativeServiceBundleMaterializationBridgeModule = Join-Path $OutputRoot 'Native-Service-Bundle-Materialization-Bridge.wvb'
$NativeOutputTableCoreModule = Join-Path $OutputRoot 'Native-Output-Table-Core.wvb'
$NativeOutputTableBridgeModule = Join-Path $OutputRoot 'Native-Output-Table-Bridge.wvb'
$NativeFileOutputTableCoreModule = Join-Path $OutputRoot 'Native-File-Output-Table-Core.wvb'
$NativeFileOutputTableBridgeModule = Join-Path $OutputRoot 'Native-File-Output-Table-Bridge.wvb'
$NativeFileInputTableCoreModule = Join-Path $OutputRoot 'Native-File-Input-Table-Core.wvb'
$NativeFileInputTableBridgeModule = Join-Path $OutputRoot 'Native-File-Input-Table-Bridge.wvb'
$NativeServiceTableCoreModule = Join-Path $OutputRoot 'Native-Service-Table-Core.wvb'
$NativeServiceTableBridgeModule = Join-Path $OutputRoot 'Native-Service-Table-Bridge.wvb'
$NativeExecutionContextCoreModule = Join-Path $OutputRoot 'Native-Execution-Context-Core.wvb'
$NativeExecutionContextBridgeModule = Join-Path $OutputRoot 'Native-Execution-Context-Bridge.wvb'
$NativeArgumentTableCoreModule = Join-Path $OutputRoot 'Native-Argument-Table-Core.wvb'
$NativeArgumentTableBridgeModule = Join-Path $OutputRoot 'Native-Argument-Table-Bridge.wvb'
$NativeEntryBridgeCoreModule = Join-Path $OutputRoot 'Native-Entry-Bridge-Core.wvb'
$NativeEntryBridgeBridgeModule = Join-Path $OutputRoot 'Native-Entry-Bridge-Bridge.wvb'
$NativeByteResultAdmissionCoreModule = Join-Path $OutputRoot 'Native-Byte-Result-Admission-Core.wvb'
$NativeByteResultAdmissionBridgeModule = Join-Path $OutputRoot 'Native-Byte-Result-Admission-Bridge.wvb'
$NativeHostedToolMetadataAdmissionModule = Join-Path $OutputRoot 'Native-Hosted-Tool-Metadata-Admission.wvb'
$NativeHostedToolMetadataConstructionCoreModule = Join-Path $OutputRoot 'Native-Hosted-Tool-Metadata-Construction-Core.wvb'
$NativeHostedToolMetadataConstructionBridgeModule = Join-Path $OutputRoot 'Native-Hosted-Tool-Metadata-Construction-Bridge.wvb'
$NativeHostedStartupInstantiationModule = Join-Path $OutputRoot 'Native-Hosted-Startup-Instantiation.wvb'
$NativeHostedContainerPlanModule = Join-Path $OutputRoot 'Native-Hosted-Container-Construction.wvb'
$NativeHostedContainerWindowsModule = Join-Path $OutputRoot 'Native-Hosted-Container-Windows.wvb'
$NativeHostedContainerLinuxModule = Join-Path $OutputRoot 'Native-Hosted-Container-Linux.wvb'
$NativeHostedContainerSegmentationModule = Join-Path $OutputRoot 'Native-Hosted-Container-Segmentation.wvb'
$NativeHostedToolRuntimeHeaderCoreModule = Join-Path $OutputRoot 'Native-Hosted-Tool-Runtime-Header-Core.wvb'
$NativeHostedToolRuntimeHeaderBridgeModule = Join-Path $OutputRoot 'Native-Hosted-Tool-Runtime-Header-Bridge.wvb'
$NativePublicationLifetimeCoreModule = Join-Path $OutputRoot 'Native-Publication-Lifetime-Core.wvb'
$NativePublicationLifetimeBridgeModule = Join-Path $OutputRoot 'Native-Publication-Lifetime-Bridge.wvb'
$SourceLexerModule = Join-Path $OutputRoot 'Source-Lexer-Core.wvb'
$SourceLexerDemoModule = Join-Path $OutputRoot 'Source-Lexer-Demo.wvb'
$SourceDeclarationParserModule = Join-Path $OutputRoot 'Source-Declaration-Parser.wvb'
$SourceDeclarationParserDemoModule = Join-Path $OutputRoot 'Source-Declaration-Parser-Demo.wvb'
$SourceDeclarationParserToolModule = Join-Path $OutputRoot 'Source-Declaration-Parser-Tool.wvb'
$SourceBodyParserModule = Join-Path $OutputRoot 'Source-Body-Parser.wvb'
$SourceBodyParserDemoModule = Join-Path $OutputRoot 'Source-Body-Parser-Demo.wvb'
$SourceBodyParserToolModule = Join-Path $OutputRoot 'Source-Body-Parser-Tool.wvb'
$SourceSetModule = Join-Path $OutputRoot 'Source-Set-Core.wvb'
$SourceSetDemoModule = Join-Path $OutputRoot 'Source-Set-Demo.wvb'
$SourceSetToolModule = Join-Path $OutputRoot 'Source-Set-Tool.wvb'
$SourceGraphModule = Join-Path $OutputRoot 'Source-Graph-Core.wvb'
$SourceGraphDemoModule = Join-Path $OutputRoot 'Source-Graph-Demo.wvb'
$SourceGraphToolModule = Join-Path $OutputRoot 'Source-Graph-Tool.wvb'
$SourceSymbolsModule = Join-Path $OutputRoot 'Source-Symbols-Core.wvb'
$SourceSymbolsDemoModule = Join-Path $OutputRoot 'Source-Symbols-Demo.wvb'
$SourceSymbolsToolModule = Join-Path $OutputRoot 'Source-Symbols-Tool.wvb'
$SourceBindingsModule = Join-Path $OutputRoot 'Source-Bindings-Core.wvb'
$SourceBindingsDemoModule = Join-Path $OutputRoot 'Source-Bindings-Demo.wvb'
$SourceBindingsToolModule = Join-Path $OutputRoot 'Source-Bindings-Tool.wvb'
$SourceWirModule = Join-Path $OutputRoot 'Source-Wir-Core.wvb'
$SourceWirDemoModule = Join-Path $OutputRoot 'Source-Wir-Demo.wvb'
$SourceWirToolModule = Join-Path $OutputRoot 'Source-Wir-Tool.wvb'
$SourceWvbModule = Join-Path $OutputRoot 'Source-Wvb-Core.wvb'
$SourceWvbDemoModule = Join-Path $OutputRoot 'Source-Wvb-Demo.wvb'
$SourceWvbToolModule = Join-Path $OutputRoot 'Source-Wvb-Tool.wvb'
$WvDumpCoreModule = Join-Path $OutputRoot 'Wv-Dump-Core.wvb'
$WvoCoreModule = Join-Path $OutputRoot 'Wvo-Object-Core.wvb'
$WvaAssemblerModule = Join-Path $OutputRoot 'Wva-Assembler-Core.wvb'
$WvLinkerCoreModule = Join-Path $OutputRoot 'Wv-Linker-Core.wvb'
$WvoSample = Join-Path $OutputRoot 'Sample.wvo'
$LinkProviderObject = Join-Path $OutputRoot 'Console-Provider.wvo'
$WindvaleLinkedImage = Join-Path $OutputRoot 'Hello-Linked-Windvale.bin'
$WindvaleLinkMap = Join-Path $OutputRoot 'Hello-Linked-Windvale.wvmap'
$InvalidWindvaleAssemblyObject = Join-Path $OutputRoot '__windvale_invalid_assembly_output__.wvo'
$InvalidWindvaleLinkedImage = Join-Path $OutputRoot '__windvale_invalid_wvlink_output__.bin'

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Examples/Seed/Sum-Data.wvproj') `
    $SumModule `
    494 `
    '76b4fa3c4c0cc37e6f1350e8191ccd78c6272224f146ef9816b5f987114c15df' `
    'build status=Published verification=compiler-aligned functions=2 code-bytes=270 module-bytes=494'
Invoke-ExactVerify $SumModule
Invoke-ExactInspect $SumModule 'opcode=data\.load\.i32 operand=0'
Invoke-ExactRun `
    $SumModule `
    29 `
    494 `
    '76b4fa3c4c0cc37e6f1350e8191ccd78c6272224f146ef9816b5f987114c15df'
Invoke-ExactInstructionReport `
    $SumModule `
    29 `
    203 `
    494 `
    '76b4fa3c4c0cc37e6f1350e8191ccd78c6272224f146ef9816b5f987114c15df'

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Examples/Seed/Hello-Windvale.wvproj') `
    $HelloModule `
    253 `
    '0a9230e700a10d14e718340e49562e5b0184a3c3a71b5cd29915126a6b28c28f' `
    'build status=Published verification=compiler-aligned functions=1 code-bytes=36 module-bytes=253'

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Examples/Foundation/Read-Wvb-Header.wvproj') `
    $FoundationModule `
    1701 `
    'c13efd14485afa1bf7fa418b54cea2fdd234fe34fdc824ae52346ce062be7793' `
    'build status=Published verification=compiler-aligned functions=2 code-bytes=1379 module-bytes=1701'
Invoke-ExactVerify $FoundationModule
Invoke-ExactInspect $FoundationModule 'opcode=bytes\.read_u32_little(?:\r?$|\s)'
Invoke-ExactRun `
    $FoundationModule `
    1 `
    1701 `
    'c13efd14485afa1bf7fa418b54cea2fdd234fe34fdc824ae52346ce062be7793'

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Examples/Foundation/Module-Composition-Demo.wvproj') `
    $CompositionModule `
    660 `
    '030ce3f627e7bdeb8ff8a3432f01e94920c93551fd58d982bdafe9f9a5d24607' `
    'build status=Published verification=compiler-aligned functions=4 code-bytes=280 module-bytes=660'
Invoke-ExactRun `
    $CompositionModule `
    42 `
    660 `
    '030ce3f627e7bdeb8ff8a3432f01e94920c93551fd58d982bdafe9f9a5d24607'

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Foundation/Machine-Contracts.wvproj') `
    $MachineContractsModule `
    2466 `
    'f624739461dea01862121daf234b3a838dfcafd73753e3124a038b7efa8b4fa3' `
    'build status=Published verification=compiler-aligned functions=2 code-bytes=2019 module-bytes=2466'
Invoke-ExactInspect `
    $MachineContractsModule `
    @('Foundation\\u02C9alignment\\u02C9is\\u02C9valid', 'Foundation\\u02C9machine\\u02C9name\\u02C9is\\u02C9valid', 'section name=exports .* count=2')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Examples/Foundation-Machine-Contracts-Demo.wvproj') `
    $MachineContractsDemoModule `
    3487 `
    '69106233197b3dbc33f23184eaa443505e8595aa056e9e2e10659a33eeefeea3' `
    'build status=Published verification=compiler-aligned functions=3 code-bytes=2899 module-bytes=3487'
Invoke-ExactRun `
    $MachineContractsDemoModule `
    0 `
    3487 `
    '69106233197b3dbc33f23184eaa443505e8595aa056e9e2e10659a33eeefeea3'

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Foundation/Byte-Ordering.wvproj') `
    $ByteOrderingModule `
    990 `
    '27a3c24b5cc358a4f67e2e1959b5e80559918f0176c52e08648e638212e6dece' `
    'build status=Published verification=compiler-aligned functions=1 code-bytes=720 module-bytes=990'
Invoke-ExactInspect `
    $ByteOrderingModule `
    @('Foundation\\u02C9byte\\u02C9spans\\u02C9compare', 'section name=exports .* count=1')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Examples/Foundation-Byte-Ordering-Demo.wvproj') `
    $ByteOrderingDemoModule `
    2422 `
    'fbaf423b6e4eac5c18b644dc27f1fa20fca8798519596485cd7497b44979533f' `
    'build status=Published verification=compiler-aligned functions=2 code-bytes=2059 module-bytes=2422'
Invoke-ExactRun `
    $ByteOrderingDemoModule `
    0 `
    2422 `
    'fbaf423b6e4eac5c18b644dc27f1fa20fca8798519596485cd7497b44979533f'

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Foundation/Decimal-Parsing.wvproj') `
    $DecimalParsingModule `
    1698 `
    'bb120d1098855b8b4adced6bcd1b1ab695f115e76bebdacb19a2b07b798cad37' `
    'build status=Published verification=compiler-aligned functions=1 code-bytes=1301 module-bytes=1698'
Invoke-ExactInspect `
    $DecimalParsingModule `
    @('Foundation\\u02C9u32\\u02C9parse', 'Foundation\\u02C9u32\\u02C9decimal\\u02C9parse', 'section name=exports .* count=1')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Examples/Foundation-Decimal-Parsing-Demo.wvproj') `
    $DecimalParsingDemoModule `
    3742 `
    'd323f8fa9178583990394a37872a8ee522320084ef4741eac26cb0f86c21b453' `
    'build status=Published verification=compiler-aligned functions=2 code-bytes=2969 module-bytes=3742'
Invoke-ExactRun `
    $DecimalParsingDemoModule `
    0 `
    3742 `
    'd323f8fa9178583990394a37872a8ee522320084ef4741eac26cb0f86c21b453'

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Foundation/Byte-Construction.wvproj') `
    $ByteConstructionModule `
    2001 `
    '3be0d06b8f4e7745dd9ffd9f325804d69ce524ac7ff6341b1e7b38037f6dd6f8' `
    'build status=Published verification=compiler-aligned functions=2 code-bytes=1503 module-bytes=2001'
Invoke-ExactInspect `
    $ByteConstructionModule `
    @('Foundation\\u02C9bytes\\u02C9result', 'Foundation\\u02C9bytes\\u02C9repeat', 'Foundation\\u02C9bytes\\u02C9replace', 'section name=exports .* count=2')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Examples/Foundation-Byte-Construction-Demo.wvproj') `
    $ByteConstructionDemoModule `
    5017 `
    'ab594976ced7a84573ade0aa50fb4370d96b8004c8b9a5ec1e888968c7b3bf8f' `
    'build status=Published verification=compiler-aligned functions=3 code-bytes=4194 module-bytes=5017'

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Compiler/Windvale/Native-Stencil-Core.wvproj') `
    $NativeStencilModule `
    21296 `
    '6df3c524d0f9bec79cd2516a758985c487cc237c6f94bc5b80e015975d50cca3' `
    'build status=Published verification=compiler-aligned functions=20 code-bytes=16427 module-bytes=21296'
Invoke-ExactInspect `
    $NativeStencilModule `
    @('Native\\u02C9stencil\\u02C9result', 'Native\\u02C9stencil\\u02C9patch\\u02C9kind', 'Native\\u02C9stencil\\u02C9process\\u02C9argument\\u02C9count', 'Native\\u02C9stencil\\u02C9process\\u02C9argument', 'section name=exports .* count=20')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Examples/Native-Stencil-Demo.wvproj') `
    $NativeStencilDemoModule `
    25683 `
    '6b27fbd10d5f06855354f433ec0b8c9b1af1761ef04458817931e675c26e0da8' `
    'build status=Published verification=compiler-aligned functions=24 code-bytes=21063 module-bytes=25683'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Compiler/Windvale/Native-Stencil-Bridge.wvproj') `
    $NativeStencilBridgeModule `
    20800 `
    '0a4387f12674f08d91682898a27bf84494cbdf886c34542beeb52fd9c4a538da' `
    'build status=Published verification=compiler-aligned functions=21 code-bytes=16833 module-bytes=20800'
Invoke-ExactInspect $NativeStencilBridgeModule @('name="Main" parameters=0 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Utf8-Service-Core.wvproj') `
    $NativeUtf8CoreModule `
    11577 `
    'adbd4843f3c0aaf003dc6118461278fc903fd2264be6e3b90835af49eb3cb2c7' `
    'build status=Published verification=compiler-aligned functions=18 code-bytes=9098 module-bytes=11577'
Invoke-ExactInspect $NativeUtf8CoreModule @('profile=portable', 'Native\\u02C9x64\\u02C9utf8\\u02C9service\\u02C9build', 'section name=exports .* count=1')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Utf8-Service.wvproj') `
    $NativeUtf8BridgeModule `
    11511 `
    '4d3c8d50d371147d687163c6d7ab761d32445719789f1f62f1f116f2bf268c4f' `
    'build status=Published verification=compiler-aligned functions=19 code-bytes=9114 module-bytes=11511'
Invoke-ExactInspect $NativeUtf8BridgeModule @('profile=portable', 'name="Main" parameters=0 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Integer-Format-Services-Core.wvproj') `
    $NativeIntegerFormatCoreModule `
    11611 `
    '6b5b5660392a9f927d046eff41aa3470bdbc616970a0e297c2c467b53d3f1fa2' `
    'build status=Published verification=compiler-aligned functions=11 code-bytes=9588 module-bytes=11611'
Invoke-ExactInspect $NativeIntegerFormatCoreModule @('profile=portable', 'Native\\u02C9x64\\u02C9integer\\u02C9format\\u02C9service\\u02C9build', 'section name=exports .* count=1')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Integer-Format-Services.wvproj') `
    $NativeIntegerFormatBridgeModule `
    11598 `
    '851f6d8e01b62106763af518c15dc163a9af9ea30c14cdb01d62adf1538ae7f9' `
    'build status=Published verification=compiler-aligned functions=12 code-bytes=9654 module-bytes=11598'
Invoke-ExactInspect $NativeIntegerFormatBridgeModule @('profile=portable', 'name="Main" parameters=0 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Service-Code-Builder.wvproj') `
    $NativeServiceCodeBuilderModule `
    4135 `
    'adfb19e5a0668d06d40e0d6cadfadb34a729a0b0d1c12a11d03af722bd53cb06' `
    'build status=Published verification=compiler-aligned functions=12 code-bytes=2440 module-bytes=4135'
Invoke-ExactInspect `
    $NativeServiceCodeBuilderModule `
    @('profile=portable', 'Native\\u02C9x64\\u02C9service\\u02C9builder', 'Native\\u02C9x64\\u02C9service\\u02C9finish', 'section name=exports .* count=10')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Output-Service-Windows.wvproj') `
    $NativeWindowsOutputCoreModule `
    9435 `
    'a072c3dc92b9675d00ac833860c0c7ef7b44cf98d15a3fead38955921d321983' `
    'build status=Published verification=compiler-aligned functions=15 code-bytes=7347 module-bytes=9435'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Output-Service-Linux.wvproj') `
    $NativeLinuxOutputCoreModule `
    8908 `
    'd3d8c8b660694af7aed52b3f78a650fc6030bfe4ad6d8adc25396ee64ed608ad' `
    'build status=Published verification=compiler-aligned functions=14 code-bytes=6941 module-bytes=8908'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Output-Services.wvproj') `
    $NativeOutputBridgeModule `
    14930 `
    '209b3fad1d03c6f9d08a20e4cfce2511c3af3ed894e1e70e3b32f05ad067ceed' `
    'build status=Published verification=compiler-aligned functions=18 code-bytes=12050 module-bytes=14930'
Invoke-ExactInspect $NativeOutputBridgeModule @('profile=portable', 'name="Main" parameters=0 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-File-Output-Service-Code.wvproj') `
    $NativeFileOutputCodeModule `
    6576 `
    '7ed9baf3a21912933045b99cb82d22d73620a318a716931db86670e5ea2212c6' `
    'build status=Published verification=compiler-aligned functions=18 code-bytes=4463 module-bytes=6576'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-File-Output-Service-Linux.wvproj') `
    $NativeLinuxFileOutputCoreModule `
    18658 `
    '834d0c45b85b26ffd3ee43e49a85c8c4ffa08f36581c02785729b276eeccdb48' `
    'build status=Published verification=compiler-aligned functions=21 code-bytes=14933 module-bytes=18658'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-File-Output-Service-Windows.wvproj') `
    $NativeWindowsFileOutputCoreModule `
    21129 `
    '9ca03bf6f5b8678389c81e281438160ff4c96c86f11a048aba90238fdc81a45d' `
    'build status=Published verification=compiler-aligned functions=22 code-bytes=16956 module-bytes=21129'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-File-Output-Services.wvproj') `
    $NativeFileOutputBridgeModule `
    33437 `
    '441db0e0e5a90f98c7e4b12b17086f56487e7d754d7b6378a0eb2972591e64f6' `
    'build status=Published verification=compiler-aligned functions=26 code-bytes=27468 module-bytes=33437'
Invoke-ExactInspect $NativeFileOutputBridgeModule @('profile=portable', 'name="Main" parameters=0 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-File-Input-Service-Code.wvproj') `
    $NativeFileInputCodeModule `
    7869 `
    'e2bfd4521b8f22529f3747eef196bdf7fa7aa0e97644db23ed45939aa10a1a7a' `
    'build status=Published verification=compiler-aligned functions=20 code-bytes=5317 module-bytes=7869'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-File-Input-Service-Linux.wvproj') `
    $NativeLinuxFileInputCoreModule `
    26718 `
    '04533e8ecade1f29e0b706c75ec949f5b4c300074cfd65feacb86f5107dcaeba' `
    'build status=Published verification=compiler-aligned functions=26 code-bytes=21582 module-bytes=26718'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-File-Input-Service-Windows.wvproj') `
    $NativeWindowsFileInputCoreModule `
    32085 `
    '6155c4ebb8f4ea76a5d1f22c1bb788aec51e731ceb4a1c5a4ceb7551ba8f409a' `
    'build status=Published verification=compiler-aligned functions=28 code-bytes=25972 module-bytes=32085'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-File-Input-Services.wvproj') `
    $NativeFileInputBridgeModule `
    51341 `
    '09f73787a909ae35ebc1aefb05bd88e4282ff8db7152d196f83b2798ea7c2234' `
    'build status=Published verification=compiler-aligned functions=35 code-bytes=42279 module-bytes=51341'
Invoke-ExactInspect $NativeFileInputBridgeModule @('profile=portable', 'name="Main" parameters=0 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Text-Concat-Service-Core.wvproj') `
    $NativeTextConcatCoreModule `
    10253 `
    '6b03161b9b3f112c6641474e321b2764522eb57a949d1b6bfc3d7b73ac91cc73' `
    'build status=Published verification=compiler-aligned functions=14 code-bytes=8082 module-bytes=10253'
Invoke-ExactInspect $NativeTextConcatCoreModule @('profile=portable', 'Native\\u02C9x64\\u02C9text\\u02C9concat\\u02C9service\\u02C9build', 'section name=exports .* count=1')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Text-Concat-Service.wvproj') `
    $NativeTextConcatBridgeModule `
    10232 `
    '87bd2e3489d3a5e4b31002858f37a5f2547706fdecc9b5f9292c736c331b9a08' `
    'build status=Published verification=compiler-aligned functions=15 code-bytes=8098 module-bytes=10232'
Invoke-ExactInspect $NativeTextConcatBridgeModule @('profile=portable', 'name="Main" parameters=0 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Text-Quote-Service-Core.wvproj') `
    $NativeTextQuoteCoreModule `
    1471 `
    'b23c077329de43fcc307f7e7f564aefe318ca1dd7dc6543bfa10160ab724c453' `
    'build status=Published verification=compiler-aligned functions=1 code-bytes=16 module-bytes=1471'
Invoke-ExactInspect $NativeTextQuoteCoreModule @('profile=portable', 'data index=0 name="Native\\u02C9x64\\u02C9text\\u02C9quote\\u02C9leaf" type=bytes bytes=1165', 'Native\\u02C9x64\\u02C9text\\u02C9quote\\u02C9service\\u02C9build', 'section name=exports .* count=1')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Text-Quote-Service.wvproj') `
    $NativeTextQuoteBridgeModule `
    1435 `
    '306b76bcf7e6b3252ce0f9509664acc5ee5a2bcc8fa411e8fdcf2c6a1fb4b631' `
    'build status=Published verification=compiler-aligned functions=2 code-bytes=32 module-bytes=1435'
Invoke-ExactInspect $NativeTextQuoteBridgeModule @('profile=portable', 'name="Main" parameters=0 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Enum-Name-Service-Core.wvproj') `
    $NativeEnumNameCoreModule `
    625 `
    'b404104b8e5ca174841b47d02ea45f197599179e0cb23ba778d6a2cdf7846948' `
    'build status=Published verification=compiler-aligned functions=1 code-bytes=16 module-bytes=625'
Invoke-ExactInspect $NativeEnumNameCoreModule @('profile=portable', 'data index=0 name="Native\\u02C9x64\\u02C9enum\\u02C9name\\u02C9leaf" type=bytes bytes=323', 'Native\\u02C9x64\\u02C9enum\\u02C9name\\u02C9service\\u02C9build', 'section name=exports .* count=1')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-X64-Enum-Name-Service.wvproj') `
    $NativeEnumNameBridgeModule `
    592 `
    '46d806adcceee597a139976748c2e1d5a25dbf57a3fba61c6836b6cf3ce1f76c' `
    'build status=Published verification=compiler-aligned functions=2 code-bytes=32 module-bytes=592'
Invoke-ExactInspect $NativeEnumNameBridgeModule @('profile=portable', 'name="Main" parameters=0 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Compiler/Windvale/Native-Enum-Metadata-Core.wvproj') `
    $NativeEnumMetadataCoreModule `
    15414 `
    '8f22e1ba56985fc5a330fcb73cda84456ecc3ef51f9ddffd6bc2edd740f73659' `
    'build status=Published verification=compiler-aligned functions=17 code-bytes=13480 module-bytes=15414'
Invoke-ExactInspect $NativeEnumMetadataCoreModule @('profile=portable', 'Native\\u02C9enum\\u02C9metadata\\u02C9build', 'section name=exports .* count=1')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Compiler/Windvale/Native-Enum-Metadata.wvproj') `
    $NativeEnumMetadataBridgeModule `
    15292 `
    '052be4402df26ed542107d666ed894cadb04a46ba6b2428bafc9f1879e38a072' `
    'build status=Published verification=compiler-aligned functions=18 code-bytes=13511 module-bytes=15292'
Invoke-ExactInspect $NativeEnumMetadataBridgeModule @('profile=portable', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Compiler/Windvale/Native-Publication-Core.wvproj') `
    $NativePublicationModule `
    7190 `
    '3048902ce708d6e640d484507efc1d567399bcafed6e2c133ca2827aff83189f' `
    'build status=Published verification=compiler-aligned functions=8 code-bytes=5333 module-bytes=7190'
Invoke-ExactInspect $NativePublicationModule @('profile=portable', 'Native\\u02C9publication\\u02C9result', 'Native\\u02C9publication\\u02C9status', 'Native\\u02C9publication\\u02C9plan', 'section name=exports .* count=8')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Compiler/Windvale/Native-Publication.wvproj') `
    $NativePublicationBridgeModule `
    6758 `
    '111608af768b18adb9be8b531214aeb14c472efef482fad507224aaa1b18909c' `
    'build status=Published verification=compiler-aligned functions=9 code-bytes=5399 module-bytes=6758'
Invoke-ExactInspect $NativePublicationBridgeModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Runtime/Windvale-Native-Service-Bundle-Materialization-Core.wvproj') `
    $NativeServiceBundleMaterializationCoreModule `
    17185 `
    '97063c0c3d264d9b9ede73cc316c68798c66d61732c5b115f71a33e486ee7008' `
    'build status=Published verification=compiler-aligned functions=19 code-bytes=14253 module-bytes=17185'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Runtime/Windvale-Native-Service-Bundle-Materialization.wvproj') `
    $NativeServiceBundleMaterializationBridgeModule `
    17150 `
    '327b753062d46755b934cfe6e6bc16550ec711c8b7d2aff46eac4bf0d8d9d902' `
    'build status=Published verification=compiler-aligned functions=20 code-bytes=14319 module-bytes=17150'
Invoke-ExactInspect $NativeServiceBundleMaterializationBridgeModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-Output-Table-Core.wvproj') `
    $NativeOutputTableCoreModule `
    4710 `
    'ab51993aea2370d84b8fe116634e3da71882756bfa87822f1bce180bb01b04a8' `
    'build status=Published verification=compiler-aligned functions=7 code-bytes=4002 module-bytes=4710'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-Output-Table.wvproj') `
    $NativeOutputTableBridgeModule `
    4714 `
    'b5b20dc0213e55790e4f39e8a512a17e2a0304b0202d488a9342905ee35e80a8' `
    'build status=Published verification=compiler-aligned functions=8 code-bytes=4033 module-bytes=4714'
Invoke-ExactInspect $NativeOutputTableBridgeModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-File-Output-Table-Core.wvproj') `
    $NativeFileOutputTableCoreModule `
    3926 `
    'fb6fd67339561f517967b326cc4299132699dc6f098a38595bbb3aabbf1fbc7f' `
    'build status=Published verification=compiler-aligned functions=6 code-bytes=3293 module-bytes=3926'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-File-Output-Table.wvproj') `
    $NativeFileOutputTableBridgeModule `
    3930 `
    '94cc057b655c58be3ccd2db333cff4e7a755482c52983c4031196ab060a89e06' `
    'build status=Published verification=compiler-aligned functions=7 code-bytes=3324 module-bytes=3930'
Invoke-ExactInspect $NativeFileOutputTableBridgeModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-File-Input-Table-Core.wvproj') `
    $NativeFileInputTableCoreModule `
    5078 `
    '0c6b66ae7fcef5a0b73df1d56bbfd0a5376ae2978f6ae762470abcf544b6a438' `
    'build status=Published verification=compiler-aligned functions=6 code-bytes=4381 module-bytes=5078'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-File-Input-Table.wvproj') `
    $NativeFileInputTableBridgeModule `
    5084 `
    'e7d33fc579c0bc2d001a3e7e2ad68e6403091cae6bda270e51578e10f04c4bd9' `
    'build status=Published verification=compiler-aligned functions=7 code-bytes=4412 module-bytes=5084'
Invoke-ExactInspect $NativeFileInputTableBridgeModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-Service-Table-Core.wvproj') `
    $NativeServiceTableCoreModule `
    3065 `
    'ca7388bf816e7d23d5a4cd3cb7cff488ba2cb3d96c0c1a0f511ced54b4296c26' `
    'build status=Published verification=compiler-aligned functions=6 code-bytes=2492 module-bytes=3065'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-Service-Table.wvproj') `
    $NativeServiceTableBridgeModule `
    3079 `
    '04c87116f12097c6efaeddc471c06ce831f6146c94b4cae0205a635f31bcd50b' `
    'build status=Published verification=compiler-aligned functions=7 code-bytes=2523 module-bytes=3079'
Invoke-ExactInspect $NativeServiceTableBridgeModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-Execution-Context-Core.wvproj') `
    $NativeExecutionContextCoreModule `
    5530 `
    'dda77e9fd637746bf5b1179136deee0bbae2d8d6b57982323b868b98a8daa29b' `
    'build status=Published verification=compiler-aligned functions=7 code-bytes=4767 module-bytes=5530'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-Execution-Context.wvproj') `
    $NativeExecutionContextBridgeModule `
    5531 `
    '86b9a139a387eb3c4fb86f43731e442a62af8ce3c7289cf914b31a9256d21a68' `
    'build status=Published verification=compiler-aligned functions=8 code-bytes=4798 module-bytes=5531'
Invoke-ExactInspect $NativeExecutionContextBridgeModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-Argument-Table-Core.wvproj') `
    $NativeArgumentTableCoreModule `
    4362 `
    '08df8569d091fc0c860988dceff1320d7a8e407b54ce571515af601c10120d75' `
    'build status=Published verification=compiler-aligned functions=6 code-bytes=3707 module-bytes=4362'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-Argument-Table.wvproj') `
    $NativeArgumentTableBridgeModule `
    4374 `
    '080be2dea127948697222c23efe4be828410450b602dee5cf2a63abc11627788' `
    'build status=Published verification=compiler-aligned functions=7 code-bytes=3738 module-bytes=4374'
Invoke-ExactInspect $NativeArgumentTableBridgeModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-Entry-Bridge-Core.wvproj') `
    $NativeEntryBridgeCoreModule `
    3385 `
    '8eab863c7b214e559c48c822381b822eef22bd852ce16252bb392ebdfbcefdae' `
    'build status=Published verification=compiler-aligned functions=6 code-bytes=2799 module-bytes=3385'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-Entry-Bridge.wvproj') `
    $NativeEntryBridgeBridgeModule `
    3401 `
    'd66a34430da6db3271103cfb9c2064a3a5a9de455c564ed87144cf4a0a4994c1' `
    'build status=Published verification=compiler-aligned functions=7 code-bytes=2830 module-bytes=3401'
Invoke-ExactInspect $NativeEntryBridgeBridgeModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-Byte-Result-Admission-Core.wvproj') `
    $NativeByteResultAdmissionCoreModule `
    7078 `
    'eacc3c6bce78f9b07d11b13a46059e92cf8a34fc1f659b896d444e7e3c937c04' `
    'build status=Published verification=compiler-aligned functions=10 code-bytes=6085 module-bytes=7078'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-Byte-Result-Admission.wvproj') `
    $NativeByteResultAdmissionBridgeModule `
    7057 `
    '9106356cf441c995b7c8478b3a5a779628328cd82acac87621de9a45bbb2becf' `
    'build status=Published verification=compiler-aligned functions=11 code-bytes=6116 module-bytes=7057'
Invoke-ExactInspect $NativeByteResultAdmissionBridgeModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Runtime/Windvale/Native-Hosted-Tool-Metadata-Admission.wvproj') `
    $NativeHostedToolMetadataAdmissionModule `
    10872 `
    'd7b0084ed2c69ee03ad65ee4bfffa72550fd8d9ef2889efa0be116350b80b8b5' `
    'build status=Published verification=compiler-aligned functions=13 code-bytes=9503 module-bytes=10872'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Runtime/Windvale-Native-Hosted-Tool-Metadata-Construction-Core.wvproj') `
    $NativeHostedToolMetadataConstructionCoreModule `
    24360 `
    '5808f778eb21c1214b581f0ce03958a74173a801b886aec7ed32124d7446abcd' `
    'build status=Published verification=compiler-aligned functions=35 code-bytes=21363 module-bytes=24360'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Runtime/Windvale-Native-Hosted-Tool-Metadata.wvproj') `
    $NativeHostedToolMetadataConstructionBridgeModule `
    24252 `
    'b5e9397326d3106b22ce735369ef8202ff6bb4c8e14f6069a0c467b4266c8208' `
    'build status=Published verification=compiler-aligned functions=36 code-bytes=21394 module-bytes=24252'
Invoke-ExactInspect $NativeHostedToolMetadataConstructionBridgeModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Linker/Windvale/Native-Hosted-Startup-Instantiation.wvproj') `
    $NativeHostedStartupInstantiationModule `
    21329 `
    '8fb31dbbbb70f094da1e5104d9edd49dd9690bc386541e1d19a75a0fd03ae445' `
    'build status=Published verification=compiler-aligned functions=15 code-bytes=18984 module-bytes=21329'
Invoke-ExactInspect $NativeHostedStartupInstantiationModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Linker/Windvale-Native-Hosted-Container-Construction.wvproj') `
    $NativeHostedContainerPlanModule `
    36010 `
    'e7c92413c31571e8af3dd4ed93664faee5e08716c6241d320b1377c681a254cf' `
    'build status=Published verification=compiler-aligned functions=41 code-bytes=31286 module-bytes=36010'
Invoke-ExactInspect $NativeHostedContainerPlanModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Linker/Windvale-Native-Hosted-Container-Windows.wvproj') `
    $NativeHostedContainerWindowsModule `
    17813 `
    'f7a8d3e69b347a3deddf81b5eea09ef929c9798081a6743e7d9aa94262db6de0' `
    'build status=Published verification=compiler-aligned functions=22 code-bytes=15136 module-bytes=17813'
Invoke-ExactInspect $NativeHostedContainerWindowsModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Linker/Windvale-Native-Hosted-Container-Linux.wvproj') `
    $NativeHostedContainerLinuxModule `
    12328 `
    'dac93155c68ba18f6cbe3af2d301a4c4171b9a9c05841057ea57398536fa8b42' `
    'build status=Published verification=compiler-aligned functions=19 code-bytes=10674 module-bytes=12328'
Invoke-ExactInspect $NativeHostedContainerLinuxModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Linker/Windvale-Native-Hosted-Container-Segmentation.wvproj') `
    $NativeHostedContainerSegmentationModule `
    22584 `
    '488e6d26e4d4ff459ea602fa5cd13b6270486332a4eab64796a29391271c2604' `
    'build status=Published verification=compiler-aligned functions=28 code-bytes=19181 module-bytes=22584'
Invoke-ExactInspect $NativeHostedContainerSegmentationModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Runtime/Windvale-Native-Hosted-Tool-Runtime-Header-Core.wvproj') `
    $NativeHostedToolRuntimeHeaderCoreModule `
    19516 `
    'f1c156def9fa6f00bb0401097435bb1d1429d9d4be247b8d11f0de0b5ea51be2' `
    'build status=Published verification=compiler-aligned functions=29 code-bytes=17050 module-bytes=19516'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Runtime/Windvale-Native-Hosted-Tool-Runtime-Header.wvproj') `
    $NativeHostedToolRuntimeHeaderBridgeModule `
    19459 `
    '3cc8d0850b888911ee3338600bc7699578b163e7400c2b3631ef14649b9a3f18' `
    'build status=Published verification=compiler-aligned functions=30 code-bytes=17081 module-bytes=19459'
Invoke-ExactInspect $NativeHostedToolRuntimeHeaderBridgeModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Compiler/Windvale/Native-Publication-Lifetime-Core.wvproj') `
    $NativePublicationLifetimeCoreModule `
    4955 `
    'a9e540c5c9ddaaeb4f45ab08a902a0a9019ce8155d544e319485c023b7d485d3' `
    'build status=Published verification=compiler-aligned functions=7 code-bytes=3358 module-bytes=4955'
Invoke-ExactInspect $NativePublicationLifetimeCoreModule @('profile=portable', 'Native\\u02C9publication\\u02C9lifetime\\u02C9result', 'Native\\u02C9publication\\u02C9lifetime\\u02C9status', 'Native\\u02C9publication\\u02C9lifetime\\u02C9plan', 'section name=exports .* count=7')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Compiler/Windvale/Native-Publication-Lifetime.wvproj') `
    $NativePublicationLifetimeBridgeModule `
    4442 `
    'f966e7f7553def7f3d57be0d3bed67b1b010f0e2cd4907c4ef78760a140fd554' `
    'build status=Published verification=compiler-aligned functions=8 code-bytes=3424 module-bytes=4442'
Invoke-ExactInspect $NativePublicationLifetimeBridgeModule @('profile=portable', 'section name=capabilities .* count=0', 'name="Main" parameters=1 result=bytes', 'section name=exports .* count=1')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Compiler/Windvale-Source-Lexer-Core.wvproj') `
    $SourceLexerModule `
    49470 `
    '411c7d9679fc53a600c15d2d132b4ac62aa410e45a67f63f76e08efb89da6b3e' `
    'build status=Published verification=compiler-aligned functions=20 code-bytes=40152 module-bytes=49470'
Invoke-ExactInspect $SourceLexerModule @('profile=portable', 'section name=exports offset=46433 bytes=715 count=17', 'section name=types offset=47156 bytes=2314 count=7', 'Compiler\\u02C9source\\u02C9token', 'Compiler\\u02C9token\\u02C9kind', 'Compiler\\u02C9lex\\u02C9source\\u02C9bounded')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Examples/Windvale-Source-Lexer-Demo.wvproj') `
    $SourceLexerDemoModule `
    56674 `
    'f83ff53dd2ffa1808bbf5c9ca2056f8dbb386308d52142f720ddf26420a6c2db' `
    'build status=Published verification=compiler-aligned functions=21 code-bytes=46427 module-bytes=56674'

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Compiler/Windvale-Source-Declaration-Parser.wvproj') `
    $SourceDeclarationParserModule `
    151197 `
    '8a0bafe3b0faebfd20e882be59a37af659158fb674cf58aba5adf2284050c6eb' `
    'build status=Published verification=compiler-aligned functions=52 code-bytes=120804 module-bytes=151197'
Invoke-ExactInspect $SourceDeclarationParserModule @('profile=portable', 'section name=exports offset=145507 bytes=1417 count=32', 'section name=types offset=146932 bytes=4265 count=15', 'Compiler\\u02C9source\\u02C9declaration', 'Compiler\\u02C9source\\u02C9module\\u02C9summary', 'Compiler\\u02C9parse\\u02C9next\\u02C9declaration\\u02C9validated')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Examples/Windvale-Source-Declaration-Parser-Demo.wvproj') `
    $SourceDeclarationParserDemoModule `
    154365 `
    '9e7ff36a3aa8b0a1cf5b4698ef6ab14f8be40f59fd4dffc4ab327813028e8fbf' `
    'build status=Published verification=compiler-aligned functions=53 code-bytes=124556 module-bytes=154365'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Examples/Windvale-Source-Declaration-Parser-Tool.wvproj') `
    $SourceDeclarationParserToolModule `
    151731 `
    'ad07772ae002683c58899e09e4a323b594ca4957b9f526fca5dc6f4340fd85f0' `
    'build status=Published verification=compiler-aligned functions=55 code-bytes=122750 module-bytes=151731'

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Compiler/Windvale-Source-Body-Parser.wvproj') `
    $SourceBodyParserModule `
    248663 `
    '68a340644274f220224a0c2c08058c78c82bcb0d3edff71402cfce5071121589' `
    'build status=Published verification=compiler-aligned functions=100 code-bytes=197096 module-bytes=248663'
Invoke-ExactInspect $SourceBodyParserModule @('profile=portable', 'section name=exports offset=239096 bytes=2112 count=47', 'section name=types offset=241216 bytes=7447 count=25', 'Compiler\\u02C9source\\u02C9expression', 'Compiler\\u02C9source\\u02C9statement', 'Compiler\\u02C9parse\\u02C9expression\\u02C9validated', 'Compiler\\u02C9parse\\u02C9source\\u02C9bodies')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Examples/Windvale-Source-Body-Parser-Demo.wvproj') `
    $SourceBodyParserDemoModule `
    254805 `
    '2a4e44f3c652e9c91ed2dd5c6b3eb1f30f580d937953dd99b26b0eba535a738f' `
    'build status=Published verification=compiler-aligned functions=101 code-bytes=204515 module-bytes=254805'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Examples/Windvale-Source-Body-Parser-Tool.wvproj') `
    $SourceBodyParserToolModule `
    247844 `
    '0a69617d83408b8cf0c99b0efa0e83b24357f36f1de72729c5c513736607ec4f' `
    'build status=Published verification=compiler-aligned functions=103 code-bytes=198924 module-bytes=247844'

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Compiler/Windvale-Source-Set-Core.wvproj') `
    $SourceSetModule `
    257061 `
    '2daf59f6863a39c662e282cfc272a0203cff9fc0440e033774b40c8b44354d35' `
    'build status=Published verification=compiler-aligned functions=110 code-bytes=205855 module-bytes=257061'
Invoke-ExactInspect $SourceSetModule @('profile=portable', 'section name=exports offset=248458 bytes=430 count=10', 'section name=types offset=248896 bytes=8165 count=29', 'Compiler\\u02C9source\\u02C9set\\u02C9scan', 'Compiler\\u02C9source\\u02C9set\\u02C9summary', 'Compiler\\u02C9scan\\u02C9source\\u02C9set', 'Compiler\\u02C9validate\\u02C9source\\u02C9set')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Examples/Windvale-Source-Set-Demo.wvproj') `
    $SourceSetDemoModule `
    266391 `
    'de6e86890e54a47a2dba9a821c4cb279c8c02468cbd78c8f57df95c6e399f50e' `
    'build status=Published verification=compiler-aligned functions=116 code-bytes=213351 module-bytes=266391'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Examples/Windvale-Source-Set-Tool.wvproj') `
    $SourceSetToolModule `
    260914 `
    '132e2a7817c704afa4d6ef9f9a33e21ddbd704cc0bd6139e205a0a3048c65fa1' `
    'build status=Published verification=compiler-aligned functions=115 code-bytes=209119 module-bytes=260914'

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Compiler/Windvale-Source-Graph-Core.wvproj') `
    $SourceGraphModule `
    281381 `
    'f29b234fc07bc4b1e0b01587b28cd6aa422dd61a68fa310b032b3fc3be5c8a68' `
    'build status=Published verification=compiler-aligned functions=126 code-bytes=225553 module-bytes=281381'
Invoke-ExactInspect $SourceGraphModule @('profile=portable', 'section name=exports offset=271979 bytes=549 count=12', 'section name=types offset=272536 bytes=8845 count=34', 'Compiler\\u02C9source\\u02C9graph\\u02C9status', 'Compiler\\u02C9source\\u02C9graph\\u02C9summary', 'Compiler\\u02C9validate\\u02C9source\\u02C9graph')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Examples/Windvale-Source-Graph-Demo.wvproj') `
    $SourceGraphDemoModule `
    287335 `
    '5e8c4add278609866b952bd0a18dcb7e0e9b05ac04e7e7a5a6fec1e5655ad468' `
    'build status=Published verification=compiler-aligned functions=131 code-bytes=230448 module-bytes=287335'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Examples/Windvale-Source-Graph-Tool.wvproj') `
    $SourceGraphToolModule `
    284522 `
    '1e0494b7e49f0d14a0508367dcb68d054b69faf501b3ef60ca6f14d48998f7f4' `
    'build status=Published verification=compiler-aligned functions=131 code-bytes=228463 module-bytes=284522'

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Compiler/Windvale-Source-Symbols-Core.wvproj') `
    $SourceSymbolsModule `
    445357 `
    '64fcf13cc05969ee8021448ffa660d463e0ef7b17dba31849a792635aa798bb0' `
    'build status=Published verification=compiler-aligned functions=205 code-bytes=356648 module-bytes=445357'
Invoke-ExactInspect $SourceSymbolsModule @('profile=portable', 'section name=exports offset=430346 bytes=3665 count=67', 'section name=types offset=434019 bytes=11338 count=45', 'Compiler\\u02C9source\\u02C9symbol\\u02C9status', 'Compiler\\u02C9source\\u02C9symbol\\u02C9summary', 'Compiler\\u02C9source\\u02C9symbols\\u02C9directory\\u02C9is\\u02C9valid', 'Compiler\\u02C9validate\\u02C9source\\u02C9symbols')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Examples/Windvale-Source-Symbols-Demo.wvproj') `
    $SourceSymbolsDemoModule `
    457363 `
    'dc357f3e35f0dd154b7b0979e6ddad0fa19c8eab382bf39d10b67d41d83cfb69' `
    'build status=Published verification=compiler-aligned functions=214 code-bytes=367136 module-bytes=457363'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Examples/Windvale-Source-Symbols-Tool.wvproj') `
    $SourceSymbolsToolModule `
    444094 `
    '1f2d0e0df52425b36eef50b1db05ed36cbe504cf2254a4d7333be36c0a5fcdbb' `
    'build status=Published verification=compiler-aligned functions=210 code-bytes=360642 module-bytes=444094'

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Compiler/Windvale-Source-Bindings-Core.wvproj') `
    $SourceBindingsModule `
    551200 `
    '54be5f326b982fdd33e56cb96f8b0fa21f2dcbcc6d371d57c597d7db6cd002e2' `
    'build status=Published verification=compiler-aligned functions=265 code-bytes=444696 module-bytes=551200'
Invoke-ExactInspect $SourceBindingsModule @('profile=portable', 'section name=exports offset=534813 bytes=3056 count=60', 'section name=types offset=537877 bytes=13323 count=55', 'Compiler\\u02C9source\\u02C9binding\\u02C9status', 'Compiler\\u02C9source\\u02C9binding\\u02C9summary', 'Compiler\\u02C9source\\u02C9bindings\\u02C9directory\\u02C9is\\u02C9valid', 'Compiler\\u02C9validate\\u02C9source\\u02C9bindings')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Examples/Windvale-Source-Bindings-Demo.wvproj') `
    $SourceBindingsDemoModule `
    557834 `
    '73af35691ea355eb49b06ee2ff6905c9293115591548e94f36ef38f0a55a8604' `
    'build status=Published verification=compiler-aligned functions=273 code-bytes=451323 module-bytes=557834'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Examples/Windvale-Source-Bindings-Tool.wvproj') `
    $SourceBindingsToolModule `
    551123 `
    '2088cb97a2614dbccb6e7504707c8584431e3e1ccd4bd98797b1d65398fdaaef' `
    'build status=Published verification=compiler-aligned functions=270 code-bytes=448326 module-bytes=551123'

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Compiler/Windvale-Source-Wir-Core.wvproj') `
    $SourceWirModule `
    831890 `
    '221702d78eea74babbe3762f59da7f5445920cd093e8a42859ed2b5ce009d8e9' `
    'build status=Published verification=compiler-aligned functions=349 code-bytes=677715 module-bytes=831890'
Invoke-ExactInspect $SourceWirModule @('profile=portable', 'section name=exports offset=808405 bytes=3755 count=75', 'section name=types offset=812168 bytes=19722 count=66', 'Compiler\\u02C9source\\u02C9wir\\u02C9operation', 'Compiler\\u02C9source\\u02C9wir\\u02C9summary', 'Compiler\\u02C9source\\u02C9wir\\u02C9directory\\u02C9is\\u02C9valid', 'Compiler\\u02C9validate\\u02C9source\\u02C9wir')
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Examples/Windvale-Source-Wir-Demo.wvproj') `
    $SourceWirDemoModule `
    837598 `
    '67de0b84e904b4ac3b412bb6204157fb390b8a768b277522d7e78de378800a38' `
    'build status=Published verification=compiler-aligned functions=355 code-bytes=684412 module-bytes=837598'
Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Examples/Windvale-Source-Wir-Tool.wvproj') `
    $SourceWirToolModule `
    830836 `
    '385c54a5bb36c8b5b7db000123c5355939b6a51597d015dc6fa90bfc6da74927' `
    'build status=Published verification=compiler-aligned functions=354 code-bytes=681817 module-bytes=830836'

Invoke-ExactSourceCompilerBuild `
    'core' `
    $SourceWvbModule `
    956733 `
    'ef6b18cefb40527944da34103c891754207fad6aafea81f4168f33b3cb93a374' `
    'source wvb status=Valid functions=436 code-bytes=784254 module-bytes=956733'
Invoke-ExactInspect $SourceWvbModule @('profile=portable', 'section name=exports offset=931463 bytes=3740 count=78', 'section name=types offset=935211 bytes=21522 count=82', 'Compiler\\u02C9source\\u02C9wvb\\u02C9summary', 'Compiler\\u02C9compile\\u02C9source\\u02C9wvb')
Invoke-ExactSourceCompilerBuild `
    'demo' `
    $SourceWvbDemoModule `
    962310 `
    'ed063a966acdd189718a3f9fd022e1a7e7a85dc4a04a713307c9d91fe6947efe' `
    'source wvb status=Valid functions=441 code-bytes=791815 module-bytes=962310'
Invoke-ExactSourceCompilerBuild `
    'tool' `
    $SourceWvbToolModule `
    955192 `
    'f547a925b49082b4cdbca3979b1889ea154178984a0e761b3a3ad649890d4cd4' `
    'source wvb status=Valid functions=443 code-bytes=787817 module-bytes=955192'

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Examples/Windvale-Wvb-Inspector.wvproj') `
    $WvDumpCoreModule `
    94327 `
    '3d407d210a1de517746b46cbd389fdf8c4efe9cc266f3db342fe369ccd06bfff' `
    'build status=Published verification=compiler-aligned functions=61 code-bytes=74726 module-bytes=94327'
Invoke-ExactVerify $WvDumpCoreModule
Invoke-ExactInspect $WvDumpCoreModule @('profile=hosted', 'section name=capabilities offset=48 bytes=145 count=5', 'section name=exports offset=93435 bytes=17 count=1', 'section name=types offset=93460 bytes=867 count=5', 'Inspect\\u02C9wvb\\u02C9envelope', 'opcode=record\.create', 'opcode=record\.field', 'opcode=enum\.name', 'opcode=u32\.format', 'opcode=text\.concat', 'opcode=bytes\.read_i32_little', 'opcode=text\.utf8_is_valid', 'opcode=text\.from_utf8', 'opcode=text\.quote', 'opcode=u32\.from_u8')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Object-Model/Windvale-Wvo-Object.wvproj') `
    $WvoCoreModule `
    73322 `
    '40f7b7efcff5b6e5bbc3c878cf5f0147ee92af208d43d54ab8a04f87ec1e9070' `
    'build status=Published verification=compiler-aligned functions=64 code-bytes=60229 module-bytes=73322'
Invoke-ExactVerify $WvoCoreModule
Invoke-ExactInspect $WvoCoreModule @('profile=hosted', 'section name=capabilities offset=51 bytes=145 count=5', 'section name=exports offset=71410 bytes=17 count=1', 'section name=types offset=71435 bytes=1887 count=17', 'opcode=bytes\.concat', 'opcode=bytes\.from_u16_little', 'opcode=bytes\.from_i32_little', 'opcode=text\.to_utf8', '__WvM1F0', '__WvM2F0', '__WvM3F0', '__WvM4F0', '__WvM5F0', 'file\.read_bytes', 'Object\\u02C9sha256') @('file\.write_bytes')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Assembler/Windvale-Wva-Assembler.wvproj') `
    $WvaAssemblerModule `
    180071 `
    'a50e261fb690b1b2836b7b05da2d94ec7f023ef531ddd2432fc6a9001ae7049c' `
    'build status=Published verification=compiler-aligned functions=101 code-bytes=145748 module-bytes=180071'
Invoke-ExactVerify $WvaAssemblerModule
Invoke-ExactInspect $WvaAssemblerModule @('profile=hosted', 'section name=capabilities offset=54 bytes=172 count=6', 'section name=exports offset=177876 bytes=17 count=1', 'section name=types offset=177901 bytes=2170 count=19', 'Scan\\u02C9wva', 'Inspect\\u02C9wva\\u02C9semantics', 'Encode\\u02C9wva', 'Encode\\u02C9sections', 'Encode\\u02C9symbols', 'Encode\\u02C9relocations', '__WvM4F1', '__WvM2F0', '__WvM3F0', '__WvM1F0', 'opcode=bytes\.concat', 'opcode=bytes\.from_u32_little', 'file\.read_bytes', 'file\.write_bytes')

Invoke-ExactBuild `
    (Join-Path $RepositoryRoot 'Projects/Linker/Windvale-Wv-Linker.wvproj') `
    $WvLinkerCoreModule `
    135740 `
    '02f727a8ce2d6826c8414cada0933c7d5a54893ea061621d08147984c3d6f874' `
    'build status=Published verification=compiler-aligned functions=96 code-bytes=112099 module-bytes=135740'
Invoke-ExactVerify $WvLinkerCoreModule
Invoke-ExactInspect $WvLinkerCoreModule @('profile=hosted', 'section name=capabilities offset=50 bytes=172 count=6', 'section name=exports offset=133297 bytes=17 count=1', 'section name=types offset=133322 bytes=2418 count=20', 'Inspect\\u02C9object', 'Find\\u02C9section', 'Find\\u02C9symbol', 'Find\\u02C9relocation', 'Validate\\u02C9export\\u02C9uniqueness', 'Validate\\u02C9imports', 'Measure\\u02C9layout', 'Validate\\u02C9definitions', 'Build\\u02C9unrelocated\\u02C9image', 'Apply\\u02C9relocations', 'Verifier\\u02C9place\\u02C9section', 'Verifier\\u02C9find\\u02C9export', 'Verifier\\u02C9apply\\u02C9relocations\\u02C9reverse', 'Accept\\u02C9reconstructed\\u02C9image', 'Accepted\\u02C9object\\u02C9view', 'Definition\\u02C9map\\u02C9minimum\\u02C9exceeds\\u02C9limit', 'Build\\u02C9canonical\\u02C9map', '__WvM4F0', '__WvM2F0', '__WvM3F0', '__WvM1F0', '__WvM1F1', 'name="__WvM5F0" parameters=1 result=bytes locals=903', 'opcode=bytes\.read_i32_little', 'file\.read_bytes', 'file\.write_bytes')

Invoke-ExactWvDumpExecution `
    $SumModule `
    (Join-Path $RepositoryRoot 'Examples/Seed/Sum-Data.wv')
Invoke-ExactWvoReadOnlyExecution $WvoSample
Invoke-ExactWvaAndLinkerExecution `
    $WvoSample `
    (Join-Path $RepositoryRoot 'Examples/Seed/Sum-Data.wv') `
    $LinkProviderObject `
    $WindvaleLinkedImage `
    $WindvaleLinkMap `
    $InvalidWindvaleAssemblyObject `
    $InvalidWindvaleLinkedImage

$TemporaryRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$TemporaryDirectory = Join-Path `
    $TemporaryRoot `
    "windvale-seed-front-door-$PID-$([guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path $TemporaryDirectory | Out-Null
try {
    $LegacyProject = Join-Path $RepositoryRoot 'Tests/Fixtures/Project/Legacy-Project1.wvproj'
    $ExistingOutput = Join-Path $TemporaryDirectory 'Existing.wvb'
    [IO.File]::WriteAllBytes($ExistingOutput, [byte[]](9, 8, 7))
    $InvalidOutput = @(& $NativeBuild $LegacyProject $ExistingOutput 2>&1)
    if (
        $LASTEXITCODE -ne 1 -or
        $InvalidOutput.Count -ne 1 -or
        $InvalidOutput[0].ToString() -ne 'build status=Projectˉrejected code=WVP1001 line=1 column=1' -or
        [Convert]::ToHexString([IO.File]::ReadAllBytes($ExistingOutput)) -ne '090807'
    ) {
        throw 'The native Seed project rejection or output preservation contract failed.'
    }
} finally {
    $ResolvedTemporary = [IO.Path]::GetFullPath($TemporaryDirectory)
    if (!$ResolvedTemporary.StartsWith($TemporaryRoot, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Refusing to remove an unexpected native Seed temporary directory.'
    }
    Remove-Item -LiteralPath $ResolvedTemporary -Recurse -Force
}

$global:LASTEXITCODE = 0
Write-Output 'native Seed front-door reconstruction status=Complete artifacts=105 cases=185'
