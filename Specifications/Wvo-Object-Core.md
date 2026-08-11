# Windvale WVO object core

## Status and purpose

`Object-Model/Windvale/Wvo-Object-Core.wv` owns the Windvale-written WVO 1.0 read-only command and report shell. `Object-Model/Windvale/Wvo-Object-Verification.wv` owns its shared portable reader and complete bounded validator. The shell constructs the canonical representative object for an internal deterministic self-test and emits the same successful verification and inspection reports as the independent C# Stage 0 oracle.

`Windvale-Wvo-Object.wvproj` composes both object modules with `Foundationˉbyteˉordering` and `Foundationˉsha256`. The current candidate WVB is 61,008 bytes with SHA-256 `a630d49f0549c865644d8052fbff7e8bf2b6a6dcd013e1187d4356d49cd188db`. It is a source candidate pending the final grouped Windows/Linux retirement gate; this identity is not a cross-host qualification claim.

Decision 0519 makes that manifest the normal broad-script build contract and
requires independent native verification plus exact inspection of its
42-function, 51,298-code-byte product. Inspection binds the five read-only
capabilities and rejects `file.write_bytes`. The old duplicated managed source
list omitted `Wvo-Object-Verification.wv` and was no longer a current-product
contract. Hosted verification/inspection behavior, malformed inputs, and WVO
sample construction remain separate execution evidence.

Decision 0520 transfers successful hosted verification and inspection of the
canonical 218-byte assembler object to the digest-bound native applications.
The paired helper invokes the qualified native WVA assembler, requires object
SHA-256 `992c298a4f9b68dec27b7203a2770f2a37ef2016ea45e88d33ee21994060fe85`,
checks the exact native reports, and proves the object unchanged. The pinned
application's no-argument self-test currently returns 1, and native empty or
missing resource failures do not yet reproduce the reference-runtime reports;
those calls and the independent Stage 0 object-report oracles remain explicit.

This module is an object verifier and inspector, not an assembler, linker, or object-file writer. Moving the source from `Examples/Foundation` into `Object-Model/Windvale` makes its ownership explicit without changing WVO 1.0.

## Pure boundaries

The encoder remains available only to the module's no-argument self-test:

```text
Buildˉsampleˉobject() -> bytes
```

The structural verifier accepts arbitrary bounded bytes and returns a nominal result rather than allowing malformed input to escape into a runtime range failure:

```text
Inspectˉobject(Input: bytes) -> Wvoˉinspection
Wvoˉobjectˉisˉvalid(Input: bytes) -> bool
```

The verification module checks the WVO magic, version, architecture, reserved fields, limits, section invariants, strict bounded UTF-8 names, duplicate and canonical name ordering, symbol ranges and ownership, relocation references, four-byte zero placeholders, relocation ordering/non-overlap, and complete input consumption. Successful reporting reads only the verified offsets and counts. The native WVO publisher imports the boolean admission surface from this module, so inspection and mutation do not carry parallel object parsers.

The C# `Objectˉverifier` and `Objectˉinspector` remain independent Stage 0 recovery/differential oracles during candidate qualification. After native promotion, ordinary coverage uses stable WVO vectors, structural assertions, deterministic digests, and malformed-input outcomes rather than generating every expected result through C# at test time.

## Representative object

The internal canonical object remains 189 bytes with SHA-256:

```text
006fd80183da7fbc71d3c6d63b65e6f3551765508fe9dba6f38ba80e002eb28a
```

It contains:

- `.text`, alignment 16, bytes `E8 00 00 00 00 C3`;
- `.rodata`, alignment 1, bytes `48 69 0A`;
- local data symbol `Message`;
- exported function symbol `Main`;
- imported function symbol `Console_write`; and
- one `relative-i32` relocation at `.text` offset 1 targeting `Console_write` with addend `-4`.

The zero relocation field is intentional. Layout and relocation application belong to the separately owned linker.

## Read-only hosted shell

With no arguments, `Main` runs deterministic construction, validation, formatting, quoting, and SHA-256 self-tests without reading or writing a file. The ordinary forms are:

```text
wvo-object-core verify <object.wvo>
wvo-object-core inspect <object.wvo>
```

Both forms read one bounded object snapshot and completely verify it before success output. `verify` prints the architecture and SHA-256 digest. `inspect` additionally prints every section, symbol, and relocation in canonical order. Invalid WVO returns 2 with a deterministic structural status; invalid command or argument count returns 64.

The module declares exactly `console.write_line`, `diagnostic.write_line`, `file.read_bytes`, `process.argument`, and `process.argument_count`. It has no file-write capability. Path interpretation and native file failures remain in the host adapter; WVO meaning remains host-independent.

## Ownership boundary

The assembler owns WVA parsing and WVO production. The linker owns symbol resolution, layout, relocation, image construction, and maps. These object modules own only WVO validation and human-readable inspection. The paired read-only package and promotion gate are specified by [Windvale native WVO inspector](Windvale-Native-Wvo-Inspector.md); atomic whole-object replacement is separately specified by the [native WVO publisher](Windvale-Native-Wvo-Publisher.md).

Decision 0522 makes the paired digest-bound applications own the no-argument
self-test on Windows and Linux. Reconstruction derives the module-specific
enum metadata through the native hosted-enum processes and packages the
complete leaf-plus-metadata service, so deterministic construction, formatting,
quoting, and enum-name checks execute without Stage 0. Capability refusal and
invalid or missing hosted resources remain adapter-boundary contracts rather
than WVO-core semantics.
