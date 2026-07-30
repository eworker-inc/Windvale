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

The complete WVSS value is currently limited to 4,194,304 bytes by the Seed immutable-`bytes` contract. This means directory bytes reduce the maximum aggregate source payload. This first version does not claim parity with Stage 0's 16 MiB aggregate character limit; that gap must close before complete bootstrap qualification.

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

The validator runs the qualified declaration and body passes over every source, rejects duplicate declared module names, and requires dependencies to be in strict declared-name order. Each dependency must use `profile portable`, contain no capability or data declaration, and mark every function `export`. The summary reports module/source-byte totals plus aggregate import, record, enum, and function counts and deterministic first-failure evidence.

`Compilerˉsourceˉspansˉcompare` compares two already validated spans from possibly different immutable byte values using unsigned ordinal byte order. Callers own both range checks.

## Boundary and ownership

WVSS is a compiler input container, not a source package, archive, module-distribution format, general collection API, or runtime linker. It does not resolve imports, prove reachability or acyclicity, bind symbols, decode strings, retain syntax nodes or diagnostics, construct WIR, or emit WVB.

The hosted source-set tool accepts explicit root/dependency resource names, constructs WVSS in argument order, and passes it to the portable validator. Dependency order is therefore checked rather than inherited from host enumeration. Repeated reads of the same exact resource use the hosted first-successful-read snapshot.

## Qualified artifacts and evidence

- `Source-Set-Core.wvb`: 177,057 bytes, SHA-256 `c03b3e9daa5b20fc2f77a0d1dd15cb1fdc1728e2a6eda021aa766b19b1bfa2b8`.
- `Source-Set-Demo.wvb`: 186,030 bytes, SHA-256 `0054138c6e39f3c99e5cd4751c796cd599b495880d7db174323342fb7b687488`.
- `Source-Set-Tool.wvb`: 181,286 bytes, SHA-256 `dc290826985f66f80d469b99235ca290dc617997edee0aab2ea0d4227984aab6`.

The Windows x64 and Debian Linux x64 conformance runners each pass all 43 tests with zero build warnings/errors. The hosted tool validates the real source-set core plus body parser, declaration parser, lexer, and decimal parser as:

```text
source set status=Valid modules=5 source-bytes=192171 imports=4 records=16 enums=11 functions=86
```

The exact `00ef0b1` archive passed the complete native CLI verifier on both hosts. Their normalized reports matched, the three source-set artifacts above were byte-identical, and 14 directly retrieved dependency/downstream compiler, assembler, linker, object, image, and map artifacts also retained exact cross-host identity.
