# Windvale native hosted-verifier publisher current object admission

## Status and scope

Current development contract under
[current-source publisher construction](../Documents/Decisions/0962-Construct-Current-Source-Wvb-Publishers.md).
This is the read-only native-object admission stage, not a completed current-source
publisher constructor or installation qualification. Frozen `WVPI 1`, `WVPS 1`,
`WVCR 1`, and `WVIX 1` retain their exact release contracts.

The stage checks the small native startup, host adapter, SHA-256/reporting, and
transaction-state objects. The large compiled verifier continues through existing
segmented WVO/image admission. An admitted object is structurally suitable for
linkage; it is not authenticated implementation code. Construction must separately
bind exact source/module/object identities and trusted producer inputs, resolve
all named imports, and check relative-address ranges before execution.

## Input and bounds

`Publisherˉcurrentˉobjectˉinspect(Input: bytes, Role: u32)` consumes canonical
little-endian WVO 1.0 and a role from the following closed set:

| Role | Object | Function entry | Required imports / exports |
| --- | --- | --- | --- |
| 1 | Windows startup | `Windows_wvb_publisher_startup` | 1 / 1 |
| 2 | Linux startup | `Linux_wvb_publisher_startup` | 1 / 1 |
| 3 | Windows adapter | `Windows_wvb_publisher_run` | 43 / 1 |
| 4 | Linux adapter | `Linux_wvb_publisher_run` | 26 / 1 |
| 5 | SHA-256 and reporting | `X64_wvb_publication_sha256_hex` | 0 / 5 |
| 6 | Shared transaction state | `Native_publication_begin` | 0 / 2 |

Reject inputs outside 24..65,536 bytes, more than 64 symbols, more than 256
relocations, or a wrong section count before invoking the shared WVO reader.
This limits its duplicate-name scan work independently of the general object
format's larger limits. Do not enlarge these ceilings to admit a compiled
verifier; that input belongs to the segmented path.

Reuse `Object-Model/Windvale/Wvo-Object-Verification.wv` for all general WVO
validation: canonical ordering, unique names, complete records, flags, section and
symbol bounds and kinds, relocation indices, non-overlapping fields, zero patch
placeholders, and exact input consumption. Additional admission requires one
nonempty `.text` code section aligned to 16. Role 5 additionally requires one
nonempty `.rodata` read-only section aligned to 4. No writable or zero-fill
section is allowed. Every relocation must be relative-i32, target a field in
`.text`, and have addend -4. The final linker still checks resolved displacement
range; this stage does not apply relocations.

## Named bindings

Startup roles import their target's `*_wvb_publisher_run` function. Adapters
require the exact common import names/kinds declared by the maintained
`Linker/Startup/Linux-X64-Wvb-Publication-Adapter.wva`; the Windows role also
requires the 17 Windows IAT data imports declared by
`Linker/Startup/Windows-X64-Wvb-Publication-Adapter.wva`. These existing native ABI
sets are closed: an unknown, missing, duplicate, or wrong-kind binding is rejected.
The portable admission owner enumerates these sets independently of input bytes.

Role 5 exports the two functions `X64_wvb_publication_sha256_hex` and
`X64_wvb_publication_u32_hex8`, plus the three data symbols
`X64_wvb_publication_report_newline`, `X64_wvb_publication_report_prefix`, and
`X64_wvb_publication_report_separator`. Role 6 exports the two functions
`Native_publication_begin` and `Native_publication_apply`. Export sizes must be
nonzero; other exports are rejected. Canonical local symbols are permitted within
the count bound. Function offsets, local symbol counts, and code bytes need not
match an old release. A valid entry moved within its code section remains valid.

## Result and read-only command

The in-memory result contains `Valid`, `Codeˉbytes`, `Readˉonlyˉbytes`,
`Entryˉoffset`, `Secondaryˉoffset`, and `Imports`. Offsets are relative to `.text`,
not absolute executable addresses. The secondary entry is the formatter for role
5, apply for role 6, and zero for other roles. Failure returns false and five
zero fields; no partially discovered offsets may escape as valid evidence.

The maintained structure tool adds:

```text
wvhostverifierstructure --current-object <role-1-through-6> <object.wvo>
```

The role argument is exactly one ASCII digit. Exit 64 denotes usage rejection;
exit 2 denotes object admission rejection. Success exits 0 and emits one line,
in the following field order, with canonical unsigned decimal values:

```text
publisher current object status=Valid format=1 role=R code-bytes=C read-only-bytes=D entry-offset=E secondary-offset=S imports=I
```

The host supplies its normal line ending. Rejection emits no success record and
reports `publisher current object status=Rejected` on diagnostic output. This
mode writes no file. The original two-argument frozen-envelope mode remains.

## Current-source binding record

The current construction path also has a host-side binding command:

```text
node Tools/Native/Build-Current-Publisher-Binding.mjs <publisher.wvb> <object-chunk-prefix> <publisher.wvop> <image-chunk-prefix> <publisher.wvli> <reference-object-directory> <output.wvcp>
```

It consumes one already-built current publisher WVB, its strict `WVOP 1.0`
object-staging manifest and chunks, its strict `WVLI 1.0` linked-image manifest
and chunks, and the six maintained native publisher objects named in this
specification. The output path must be absent and distinct from every direct
input. Exact output/input aliasing exits 64 before any write, preserving the
subject bytes.

The record is ASCII with LF line endings and begins:

```text
windvale-current-source-wvb-publisher-binding 1
host <windows-x64-or-linux-x64>
binding-sha256 <lowercase-hex-sha256>
```

The remaining fields record byte counts and SHA-256 values for the WVB, `WVOP`,
every object chunk, `WVLI`, every image chunk, and each fixed native object
role. The `WVOP` and `WVLI` readers enforce their magic, version, manifest size,
nonzero payload size, one-through-518 chunk count, 4 MiB chunk ceiling,
contiguous positions, exact chunk byte lengths, and absence of a trailing
`chunk-<count>` resource. The image entry offset is recorded and must be inside
the declared image; the current focused owner still expects entry offset zero
from the linker report.

This binding record is construction input evidence. It does not authenticate
machine code, resolve adapter imports, compute relative relocation values,
materialize PE/ELF bytes, install a publisher, or prove transactional
publication behavior. Those remain later current-source construction gates.

## Verification

The existing `hosted-verifier-publisher-files` owner includes the focused
`--current-objects` selection. It builds the current admission self-test and
structure tool, checks all six maintained WVO roles, wrong roles, truncated and
oversized records, invalid names/counts/kinds/offsets/relocations, moved startup
entries, the inclusive byte ceiling, and command success/rejection. It does not
substitute for module identity, complete linkage, or native transaction tests.

The same owner includes the focused `--current-source` selection. It rebuilds
the current publisher source twice, compares exact WVB, `WVOP`, `WVLI`, and
chunk bytes, emits the binding record for one reproduced set, checks the shared
transaction-state object appears as role 6, and checks exact alias rejection
preserves the source WVB. Development construction requires the existing
validated current split-compiler cache. A missing cache is an explicit setup
failure, not permission to start a cold compiler reconstruction inside this
check. The focused current-object tool builder has a four-minute total
construction deadline and reuses existing exact-input caches.
