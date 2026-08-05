# Windvale WVO object core

## Status and purpose

`Object-Model/Windvale/Wvo-Object-Core.wv` owns the Windvale-written WVO 1.0 read-only core. It constructs the canonical representative object for an internal deterministic self-test, completely validates bounded WVO input, and emits the same successful verification and inspection reports as the independent C# Stage 0 oracle.

`Windvale-Wvo-Object.wvproj` composes the core with `Foundationˉbyteˉordering` and `Foundationˉsha256`. The current candidate WVB is 57,297 bytes with SHA-256 `3940e5aebb8dc25581080e5af3a73eb81eec5b7144c34fb2b7f4014e155b73a7`. It is a source candidate pending the final grouped Windows/Linux retirement gate; this identity is not a cross-host qualification claim.

This module is an object verifier and inspector, not an assembler, linker, or object-file writer. Moving the source from `Examples/Foundation` into `Object-Model/Windvale` makes its ownership explicit without changing WVO 1.0.

## Pure boundaries

The encoder remains available only to the module's no-argument self-test:

```text
Buildˉsampleˉobject() -> bytes
```

The structural verifier accepts arbitrary bounded bytes and returns a nominal result rather than allowing malformed input to escape into a runtime range failure:

```text
Inspectˉobject(Input: bytes) -> Wvoˉinspection
```

It checks the WVO magic, version, architecture, reserved fields, limits, section invariants, ASCII machine names, duplicate and canonical name ordering, symbol ranges and ownership, relocation references, four-byte zero placeholders, relocation ordering/non-overlap, and complete input consumption. Successful reporting reads only the verified offsets and counts.

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

The assembler owns WVA parsing and WVO production. The linker owns symbol resolution, layout, relocation, image construction, and maps. This object core owns only WVO validation and human-readable inspection. The paired native package and promotion gate are specified by [Windvale native WVO inspector](Windvale-Native-Wvo-Inspector.md).
