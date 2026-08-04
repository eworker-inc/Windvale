# Windvale WVO object core

## Status and purpose

`Examples/Foundation/Wvo-Object-Core.wv` is the first Windvale-written native-tool foundation. It constructs the canonical WVO 1.0 representative object entirely from immutable Windvale values, validates the complete result with Windvale source, and can persist it through an explicitly granted hosted capability. The independent C# object model decodes and verifies the produced bytes again.

The current module imports `Foundationˉbyteˉordering` through its required alias for canonical section and symbol name comparison. Its locally verified composed WVB 1.11 module is 29,840 bytes with SHA-256 `15b6de9e90cd3eb3288f106bd272fac5b4b25280f972c393e8d0f0b34ecf5d07`; the earlier artifact is cross-host qualified at `4fdea22`. The representative WVO bytes remain unchanged.

This is an object producer and structural inspector, not an assembler or linker. Its role is to prove that verified Windvale bytecode can create a useful deterministic binary interchange artifact without borrowing Windows or Linux object semantics.

## Pure boundaries

The encoder builds the header and record groups from nominal Windvale records:

```text
Buildˉsampleˉobject() -> bytes
```

The structural inspector accepts arbitrary bounded bytes and returns a nominal result rather than allowing malformed input to escape into a runtime range failure:

```text
Inspectˉobject(Input: bytes) -> Wvoˉinspection
```

It checks the WVO magic, version, architecture, reserved fields, limits, section invariants, ASCII machine names, canonical ordering, symbol ranges and section kinds, relocation references, four-byte zero placeholders, non-overlap, and complete input consumption. The C# `Objectˉverifier` remains the authoritative Stage 0 oracle and additionally supplies canonical object contracts to the CLI.

## Representative object

The canonical object is 189 bytes with SHA-256:

```text
006fd80183da7fbc71d3c6d63b65e6f3551765508fe9dba6f38ba80e002eb28a
```

It contains:

- `.text`, alignment 16, bytes `E8 00 00 00 00 C3`
- `.rodata`, alignment 1, bytes `48 69 0A`
- local data symbol `Message`
- exported function symbol `Main`
- imported function symbol `Console_write`
- one `relative-i32` relocation at `.text` offset 1 targeting `Console_write` with addend `-4`

The zero relocation field is intentional. Layout and relocation application belong to the separately owned linker.

## Hosted shell

With no arguments, `Main` runs deterministic construction and adversarial self-tests without writing a file. With one argument, it validates the generated object before calling `file.write_bytes`, then reports `Wrote WVO 1.0 bytes=189`. Other argument counts report usage and return `64`.

The module must declare and receive grants for `console.write_line`, `diagnostic.write_line`, `file.write_bytes`, `process.argument`, and `process.argument_count`. Path interpretation and native errors remain in the host adapter; the encoder and inspector are host-independent.

## Ownership boundary

The now-qualified assembler reuses the WVO contracts while replacing the fixed representative records with parsed instructions, sections, definitions, exports, and imports. The linker remains the separate owner of layout, symbol resolution, relocation evaluation, and final image adapters.
