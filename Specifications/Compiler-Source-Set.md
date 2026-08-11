# Windvale compiler source set

## Status and purpose

Windvale Source Set 1 (`WVSS 1`) is the canonical packed input collection for the portable Windvale-written semantic pipeline. The implementation is cross-host qualified at commit `00ef0b1` under Decision 0029.

WVSS carries source bytes, not paths, timestamps, encodings, decoded syntax objects, or host handles. Entry zero is the root source. Entries one onward are supplied dependencies ordered strictly by the ordinal UTF-8 bytes of their declared module names.

## Binary format

All integers are unsigned little-endian. Offsets are from the beginning of the WVSS value.

| Offset | Size | Field | Requirement |
| ---: | ---: | --- | --- |
| 0 | 4 | Magic | ASCII `WVSS` (`57 56 53 53`) |
| 4 | 2 | Major version | `1` |
| 6 | 2 | Minor version | `0` |
| 8 | 4 | Module count | `1..64` |
| 12 | 4 | Directory length | Exactly `module count * 8` |
| 16 | variable | Directory | One `(source offset: u32, source length: u32)` entry per module |
| after directory | variable | Source payloads | Strict UTF-8 Windvale sources in directory order |

The first source offset must equal `16 + directory length`. Every later offset must equal the preceding offset plus length. Lengths are nonzero. The last source must end exactly at the WVSS length. Gaps, overlap, padding, reordered payloads, and trailing bytes are noncanonical and rejected.

The complete WVSS value is currently limited to 4,194,304 bytes by the Seed immutable-`bytes` contract. This means directory bytes reduce the maximum aggregate source payload. This first version does not claim parity with Stage 0's 16 MiB aggregate character limit. The current complete 12-module compiler closure occupies 1,090,127 source bytes and proves that the present envelope is sufficient for bytecode self-hosting; envelope parity remains a separate future contract decision rather than a blocker to that achieved closure.

## Portable contracts

```text
Compilerˉscanˉsourceˉset(Input: bytes)
    -> Compilerˉsourceˉsetˉscan

Compilerˉsourceˉsetˉmodule(
    Input: bytes,
    Scan: Compilerˉsourceˉsetˉscan,
    Index: u32
) -> Compilerˉsourceˉsetˉview

Compilerˉvalidateˉsourceˉset(Input: bytes)
    -> Compilerˉsourceˉsetˉsummary
```

The scanner validates the entire envelope before any view is trusted. A successful view contains an immutable source slice; an invalid scan or out-of-range index returns a failure view with an empty byte value.

The validator runs the qualified declaration and body passes over every source, passing the accepted declaration summary into the body pass so declarations are not parsed twice. It rejects duplicate declared module names and requires dependencies to be in strict declared-name order. Each dependency must use `profile portable`, contain no capability or data declaration, and mark every function `export`. The summary reports module/source-byte totals plus aggregate import, record, enum, and function counts and deterministic first-failure evidence.

`Compilerˉsourceˉspansˉcompare` compares two already validated spans from possibly different immutable byte values using unsigned ordinal byte order. Callers own both range checks.

`Compilerˉsourceˉspansˉequal` compares two already validated spans for exact byte equality. It rejects unequal lengths immediately and scans equal-length spans from the end so the compiler's long shared identifier prefixes do not force a complete forward ordinal comparison when suffixes differ. Callers own both range checks.

## Boundary and ownership

WVSS is a compiler input container, not a source package, archive, module-distribution format, general collection API, or runtime linker. It does not resolve imports, prove reachability or acyclicity, bind symbols, decode strings, retain syntax nodes or diagnostics, construct WIR, or emit WVB.

The hosted source-set tool accepts explicit root/dependency resource names, constructs WVSS in argument order, and passes it to the portable validator. Dependency order is therefore checked rather than inherited from host enumeration. Repeated reads of the same exact resource use the hosted first-successful-read snapshot.

## Current deterministic artifacts and retained evidence

- `Source-Set-Core.wvb`: 257,873 bytes, SHA-256 `1121320e20d83f685c559ea2d0cff8b8e57583d047a3c6aaf9f5c1fdc9423acb`.
- `Source-Set-Demo.wvb`: 267,203 bytes, SHA-256 `ac7fb0e04cf042ab9f9f3bfc8f344f0fdbcdc4198189b65f152eaead84b07742`.
- `Source-Set-Tool.wvb`: 261,726 bytes, SHA-256 `6e8b8c8aaa6fe2c5735719a9b317e8897cf70f87828ea1be5d26d670bc2ed30f`.

These local candidate identities include the new lexical, declaration, and body-expression surface and require cross-host requalification. The current local hosted report is:

Decision 0517 reproduces all three identities through the current-Windows
native Project front door and natively inspects the core portable type/export
surface. Independent Linux execution and native demo/tool execution remain
pending.

```text
source set status=Valid modules=5 source-bytes=297051 imports=6 records=18 enums=11 functions=110
```

The source-set contract was originally cross-host qualified at `00ef0b1`. Decision 0042's artifacts and aggregate source-byte report were requalified byte for byte with the role-based compiler layout at `4fdc6bf`. Decision 0055's declaration-summary reuse implementation is cross-host qualified at `1a4fca7`. Decision 0058 adds the equality-only span helper and uses the unchanged WVSS 1 format for exact Stage 1 to Stage 2 compiler convergence, cross-host qualified at `5c16547`.
