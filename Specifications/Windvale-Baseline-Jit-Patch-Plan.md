# Windvale baseline-JIT patch plan

Status: implemented candidate

## Purpose

`WVJP 1` is the first Windvale-owned, serialized baseline-JIT lowering
boundary. It separates a source-side patch-plan producer from a runtime-side
verifier and materializer. The plan is ordinary portable data; it is never an
executable-memory container and it grants no authority to publish or invoke
native code.

The first accepted profile is deliberately closed. It admits the canonical
174-byte WVB 1.11 portable module produced from
`Tests/Fixtures/Native-X64/Wvb-To-Wvo-Return-42.wv`, with only the four-byte
little-endian operand of its first `i32.const` allowed to vary. The module has
one exported `Main() -> i32`, one `i32` local, no capabilities, no data, no
nominal types, and this exact instruction sequence:

1. `i32.const <value>`
2. `local.store 0`
3. `local.load 0`
4. `return`

General WVB verification remains a required admission step before a future
public JIT route calls the producer. The producer also compares every accepted
byte other than the four literal bytes, so this bounded profile fails closed
when invoked directly.

## Format

All integers use little-endian encoding. The only architecture value is `1`
for x86-64, and the only patch-kind value is `1` for an `i32` immediate.

| Offset | Bytes | Field | Required value |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVJP` |
| 4 | 2 | version | `1` |
| 6 | 2 | architecture | `1` (`x86-64`) |
| 8 | 4 | plan bytes | `54` |
| 12 | 4 | admitted WVB bytes | `174` |
| 16 | 4 | materialized code bytes | `6` |
| 20 | 4 | patch records | `1` |
| 24 | 4 | code offset | `1` |
| 28 | 4 | patch width | `4` |
| 32 | 4 | patch kind | `1` (`i32-immediate`) |
| 36 | 4 | source function index | `0` |
| 40 | 4 | source instruction offset | `0` |
| 44 | 4 | immediate bits | copied from WVB offsets 122 through 125 |
| 48 | 1 | template opcode | `0xB8` (`mov eax, imm32`) |
| 49 | 4 | template patch hole | all zero |
| 53 | 1 | template return | `0xC3` (`ret`) |

The runtime verifier requires exactly 54 bytes, checks every fixed field and
template byte, and constructs exactly these six bytes:

```text
B8 <four immediate bytes> C3
```

It does not accept extra records, trailing bytes, alternate instruction
encodings, nonzero template holes, or architecture aliases.

## Results

The producer returns a status, a byte value, and a failure offset. Its statuses
are `Valid`, `Invalid_size`, `Invalid_wvb`, `Invalid_shape`, and
`Construction_failed`. Only `Valid` carries a 54-byte plan.

The independent runtime verifier returns the same result shape with statuses
`Valid`, `Invalid_size`, `Invalid_header`, `Invalid_record`,
`Invalid_template`, and `Construction_failed`. Only `Valid` carries six
materialized code bytes.

Both components return an empty value on rejection. Size arithmetic is fixed
and bounded; neither component reads a variable offset before the exact input
length has been established.

## Ownership and non-claims

- `Compiler/Windvale/Baseline-Jit-Patch-Plan-Core.wv` owns production from the
  closed WVB profile.
- `Runtime/Windvale/Baseline-Jit-Patch-Plan-Verifier-Core.wv` independently
  owns plan admission and materialization.
- The two components share the serialized contract, not mutable state or a
  hidden registration mechanism.
- This version proves deterministic typed lowering and hostile-input rejection.
- This version does not allocate executable memory, change page permissions,
  invoke generated code, provide a general baseline JIT, or satisfy the .NET
  retirement gate by itself.

The next contract must publish verified materialized bytes through an explicit
write-then-execute lifetime: writable while copying, executable only after a
successful permission transition, never writable and executable at the same
time, and torn down on every completion or failure path. Windows and Linux
must qualify that boundary independently.

## Verification

`Windvale-Native-Baseline-Jit-Patch-Plan-Self-Test.wvproj` builds the producer,
verifier, and capability-free self-test through the native source front door.
The self-test covers deterministic repeated output, `42`, `-1`, WVB truncation,
altered WVB magic, altered bytecode shape, plan truncation, altered plan magic,
an invalid patch width, and a nonzero template hole. The paired native host
launchers then lower, verify, link, package, and execute that test without
loading .NET.
