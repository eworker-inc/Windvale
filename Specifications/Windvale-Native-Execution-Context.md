# Windvale native execution context

## Status and scope

Execution-context version 7 and ABI-22 target `x86-64-wvb-baseline-v22` are current and cross-host qualified under [Decision 0150](../Documents/Decisions/0150-Bounded-Native-Dynamic-Value-Lifetimes.md) at exact descendant `2591cd5` in GitHub Verify run 30797770080. ABI 22 retains ABI 21's frame-owned direct records and adds generation-owned dynamic byte buffers plus verified function-entry arena checkpoints without changing this context's serialized layout. [Decision 0099](../Documents/Decisions/0099-Bounded-Native-Frame-Admission.md) established the retained 2,048-cell envelope, and [Decision 0105](../Documents/Decisions/0105-Typed-Block-Scoped-Native-Value-Slots.md) established the typed block-scoped physical map. The 2 MiB host record-arena fields qualified by [Decision 0112](../Documents/Decisions/0112-Bounded-Exact-Compiler-Record-Arena.md) and Probe 32's 1,024-byte in-page extent remain dormant compatibility seams.

This is an experimental native ABI, not a stable public foreign-function interface. ABI 22 replaces ABI 21 in the current implementation. Qualified older artifacts remain historical evidence and are not accepted by the ABI-22 fragment verifier.

## Entry convention

The exported native `Main() -> i32`, capability-free portable `Main() -> bytes`, or capability-free portable `Main(bytes) -> bytes`, receives a pointer to one execution context in `RDX`. Hosted byte entries are not admitted because their argument/file borrows require additional owner validation. The executor duplicates the context pointer into its second Windows and third System V bridge arguments so both Windows x64 and System V x86-64 place the same value in `RDX`. Generated `Main` preserves `R15`, copies the context pointer into `R15`, and loads the instruction and call-depth budgets into the shared `R11` and `R10` counters. Scalar and parameterless byte entry code and invocation remain unchanged.

For `Main() -> bytes`, the executor also duplicates one verified 16-byte result-cell pointer into its first Windows and fourth System V bridge arguments; both host conventions therefore place the pointer in physical `RCX`. The generated entry copies `RCX` to `RAX` immediately after frame allocation and then uses the ordinary hidden-result convention below. The independent decoder requires that exact copy when and only when exported `Main` has a descriptor result. The executor classifies the decoded result before W^X publication, so the scalar and byte APIs cannot invoke the wrong entry shape.

For `Main(bytes) -> bytes`, that same allocation is exactly 32 bytes. Its first
cell remains the zeroed result descriptor and its second cell is one verified
immutable host-input descriptor. After ordinary frame initialization, the
entry derives `R8 = RCX + 16` with the exact four-byte bridge instruction and
then copies the descriptor through the existing first-parameter convention.
The input is initialized, limited to 4 MiB, copied into execution-owned
immutable storage, and valid only for the call. The decoder admits exactly one
borrowed descriptor parameter only when that bridge is present and the result
is also a descriptor. Parameterless callers and byte-input callers are distinct
executor shapes and a mismatch fails as `WVN4011` before publication. Invalid
or oversized host input fails as `WVN4020` before publication. This added entry
shape was previously rejected, so ABI 22 and all existing entry bytes remain
unchanged.

Internal functions accept at most 64 parameters, matching the source-language declaration limit. The first four positions retain `R8`, `R9`, `RCX`, and `RDX`; `i32`, `bool`, `u8`, `u32`, and enums use the low dword. A record uses the complete register as a pointer to verified caller-owned backing, while borrowed `text` or `bytes` uses the complete register as a pointer to the caller's verified 16-byte descriptor. For positions 4 through 63, the caller reserves exactly one 16-byte outgoing cell per parameter. Scalars occupy the low dword, a record occupies the low machine word, and borrowed descriptors copy both machine words. After allocating its own frame, the callee copies each later cell from `RSP + frame-bytes + 8 + (position - 4) * 16`; the eight-byte term is the internal return address. The caller releases the exact reservation before testing the packed status. The maximum outgoing reservation is 960 bytes and the fragment verifier reconstructs its size, cell offsets and types, hidden-result adjustment, call target, release, and callee agreement. Packed scalar/enum values and statuses return in `RAX`; records and descriptors use the caller-owned result conventions below. Every internal call preserves the shared resource counters.

For a `text` or `bytes` return, the caller places its verified result-cell address in `RAX` after loading explicit arguments. For a record return, the caller instead passes the exact planned backing address in `RAX`. The callee saves either hidden pointer before clearing ordinary cells. A successful descriptor return copies both descriptor words to the hidden result. A successful record return copies every direct 16-byte field to the hidden destination. Both return zero status in `RAX`; traps retain their packed nonzero status. After a record call succeeds, the caller publishes its destination address in the result handle. A void call uses the same status path but has no hidden result or stored scalar. The independent decoder cross-checks every call shape against the callee's single decoded return kind.

After a successful exported byte return, the host requires a zero reserved word, a length no greater than 4 MiB, and a complete pointer range inside one exact immutable fragment-data symbol, the committed used prefix of the execution arena, or the current verified immutable entry-input buffer. It copies the accepted result before releasing the fragment, context, bridge cell, input, and arenas. A null pointer is accepted only for an empty result. Every other descriptor fails as `WVN4012`; a descriptor cannot escape its run. The parameterless exported byte-result contract is cross-host qualified under Decision 0080 at exact commit `f547af8dcf8e257ab8ad8a76a49bbdd1b9136677`; the input extension is implemented under [Decision 0360](../Documents/Decisions/0360-Native-Bounded-Byte-Entry-Input.md) with grouped cross-host qualification deferred.

## Value-slot and borrowed-descriptor layout

Each native local and physical temporary owns one zero-initialized 16-byte frame slot. ABI 22 retains deterministic persistent record-local backing, block-reused record-result backing, and an optional record-return pointer cell in the same projected frame. The complete projected frame remains limited to 2,048 cells, or 32 KiB, before any separate outgoing-call reservation. Scalars use the low four bytes. Descriptors and direct record fields use complete cells. The wider cell is an internal representation boundary, not a host object or a claim that every future Windvale value will use this exact shape.

An immutable borrowed `text` or `bytes` value occupies one slot:

| Offset | Bytes | Field | ABI-22 rule |
| ---: | ---: | --- | --- |
| 0 | 8 | data pointer | Points into verified fragment data or one execution-owned immutable host buffer |
| 8 | 4 | byte length | Exact remaining span length |
| 12 | 4 | ownership generation | Zero for static, host-borrowed, sliced, and public results; nonzero only for a verified generated dynamic-byte owner |

Static text and byte constants create descriptors through verified RIP-relative data references. A native service may return a descriptor backed by its execution-owned immutable buffer; that borrow expires when the native call returns. `Textˉtoˉutf8` copies the already-valid text descriptor as borrowed bytes without allocation. `Bytesˉfromˉu8`, `Bytesˉfromˉu16ˉlittle`, and `Bytesˉfromˉu32ˉlittle` allocate and write exactly one, two, and four bytes in the shared execution arena. The two-byte form proves its `u32` input is at most 65,535 before allocation and becomes `WVR3016` otherwise. ABI 22 gives generated dynamic byte values of at least 64 bytes an eight-byte capacity/generation header and lets `Bytesˉconcat` reuse only a valid current owner at the arena tail. Capacity doubles through 2 MiB and is capped at the 4 MiB byte-value bound; stale aliases, non-tail values, and insufficient owners take a checked allocation path. Generation advancement preserves older immutable aliases. Aggregate arena exhaustion remains `WVR3018`. `Bytesˉslice` produces another borrowed descriptor only after unsigned offset/length bounds checks and never conveys ownership. `Bytesˉreadˉu8`, `Bytesˉreadˉu16ˉlittle`, `Bytesˉreadˉu32ˉlittle`, and `Bytesˉreadˉi32ˉlittle` check the complete fixed-width range before reading. A failed slice or read returns packed status 6 and becomes `WVR3008`; no host signal or unchecked pointer access is the language-level failure path.

An enum occupies the low four bytes and retains its signed member value plus compile-time nominal identity. An ABI-22 record handle occupies one machine word and points to direct frame-owned backing. Each immutable direct field consumes one complete 16-byte cell, including borrowed text or byte descriptors. Construction copies fields into a planned scratch range; local load and store preserve value semantics by copying into planned scratch or persistent ranges. An unassigned record parameter may borrow caller backing, while an assigned parameter first receives an owned copy. Record-valued fields are rejected in this ABI.

Every record construction, copy, field access, record-returning call, and record return carries an independently decoded nominal tag. The verifier reconstructs complete field widths, backing ranges, pointer publication, caller-owned return destinations, and record call-graph agreement. Generated ABI-22 record operations do not access or advance the context record arena, and successful execution reports zero record-arena use. The retained packed status 7 / `WVR3017` suffix is decoded for context-version compatibility but is not targeted by admitted ABI-22 record operations.

ABI 22 retains ABI 21's frame-owned direct records, ABI 20's fixed-width byte construction, ABI 18's typed physical map, ABI 16's bounded 64-parameter envelope, ABI 15's native file output, ABI 14's native file input, ABI 13's native output, ABI 12's immutable argument table, ABI 11's descriptor return shapes and dynamic-value arena, ABI 9's nominal values, and ABI 8's borrowed values. Unsigned arithmetic overflow retains packed status 1 / `WVR3007`.

## Execution-context memory layout

All integer fields are little-endian. The context is exactly 112 bytes:

| Offset | Bytes | Field | Version-7 rule |
| ---: | ---: | --- | --- |
| 0 | 4 | format version | `7` |
| 4 | 4 | structure bytes | `112` |
| 8 | 8 | instruction budget | Positive maximum charged with WVB instruction semantics |
| 16 | 8 | call-depth budget | Positive maximum active native call depth |
| 24 | 8 | service-table pointer | Zero when no runtime service is required; otherwise points to the exact table below |
| 32 | 8 | record-arena pointer | Retained context-7 compatibility field; ABI-22 generated direct-record code does not read it |
| 40 | 4 | record-arena bytes | Retained bounded capacity; the current host may still supply at most 2 MiB |
| 44 | 4 | record-arena used bytes | Starts at zero and remains zero for admitted ABI-22 generated record operations |
| 48 | 8 | text-arena pointer | Execution-owned dynamic text/byte base; may be zero only when no admitted dynamic value allocates |
| 56 | 4 | text-arena bytes | Exact per-run bound; 128 MiB in the host executor and version-2/3 hosted containers, 16 MiB in the narrow version-1 console containers |
| 60 | 4 | text-arena used bytes | Starts at zero; every admitted dynamic text or byte allocator advances the same checked cursor |
| 64 | 4 | service-failure detail | Starts at zero; exact native-service failure detail described below |
| 68 | 4 | reserved | Required zero |
| 72 | 8 | argument-table pointer | Zero when the captured count is zero; otherwise points to the exact immutable descriptor array below |
| 80 | 4 | argument count | Prevalidated snapshot count from 0 through 67 |
| 84 | 4 | argument reserved | Required zero |
| 88 | 8 | output-table pointer | Zero when no output service is required; otherwise points to the exact runtime-private table below |
| 96 | 8 | file-input-table pointer | Zero when file input is not required; otherwise points to the exact runtime-private table below |
| 104 | 8 | file-output-table pointer | Zero when file output is not required; otherwise points to the exact runtime-private table below |

The platform executor or verified OS bridge owns this memory for the complete call. Ordinary generated code does not retain the pointer after return. The current adapters construct the exact version and size; the exact generated prologue and every use are independently decoded before publication.

## Runtime-service table

Service-table version 5 is exactly 104 bytes:

| Offset | Bytes | Field | Version-5 rule |
| ---: | ---: | --- | --- |
| 0 | 4 | format version | `5` |
| 4 | 4 | structure bytes | `104` |
| 8 | 8 | `console.write_line` entry | Pointer to the exact platform-specific native output leaf |
| 16 | 8 | `process.argument_count` entry | Pointer to the exact runtime-native context-count leaf |
| 24 | 8 | `process.argument` entry | Pointer to the exact runtime-native checked descriptor-copy leaf |
| 32 | 8 | `file.read_bytes` entry | Pointer to the exact platform-specific native file-input leaf |
| 40 | 8 | `Textˉutf8ˉisˉvalid` entry | Pointer to the runtime-owned pure native validation leaf |
| 48 | 8 | `diagnostic.write_line` entry | Pointer to the exact platform-specific native output leaf |
| 56 | 8 | `Enumˉname` entry | Pointer to the exact runtime-native nominal-name leaf and its adjacent verified metadata |
| 64 | 8 | `Textˉconcat` entry | Pointer to the exact runtime-native concatenation leaf |
| 72 | 8 | `Textˉquote` entry | Pointer to the exact runtime-native deterministic-quote leaf |
| 80 | 8 | `I32ˉformat` entry | Pointer to the exact runtime-native invariant signed-format leaf |
| 88 | 8 | `U32ˉformat` entry | Pointer to the exact runtime-native invariant unsigned-format leaf; `u8` is zero-extended |
| 96 | 8 | `file.write_bytes` entry | Pointer to the exact platform-specific native whole-file output leaf |

The table is deliberately closed. A fragment may require any distinct canonical-order subset of these twelve services; any unknown, duplicate, or noncanonical service list fails verification. Console, process, file, and diagnostic entries are capability services and retain explicit authorization. UTF-8 validation, enum naming, concatenation, quoting, and integer formatting are deterministic runtime support with no ambient authority. All twelve slots use exact native leaves: eight platform-neutral leaves plus platform-specific console/diagnostic, file-input, and file-output leaves. A later extension requires a new accepted contract, bounds/version handling, and cross-host evidence.

Every status-bearing service returns zero in `EAX` on success and nonzero on service failure; `process.argument_count` returns its `u32` value directly. A native leaf clears context service-failure detail on entry. Details `1` through `4` retain text-value, text-arena, argument-index, and output-write failures. Details `5` through `10` identify invalid file name, not found, permission denied, unavailable, too large, and snapshot limit. Detail `11` identifies a generated `Bytesˉconcat` result above the 4 MiB value bound. Detail `12` identifies a generated `Bytesˉfromˉu16ˉlittle` input above 65,535; its guard runs before arena mutation. Generated code retains packed status 5; the executor translates those details to the stable `WVR3012`, `WVR3018`, `WVR3020`, `WVR3029`, `WVR3021` through `WVR3025`, `WVR3028`, `WVR3015`, and `WVR3016` codes. An absent or unknown detail remains `WVR3013`. Native services and generated allocation failures do not unwind host exceptions through generated code.

## Runtime-private output table

Output-table version 1 is exactly 48 bytes. It is host state for one native execution and is never serialized into WVB or WVO:

| Offset | Bytes | Field | Version-1 rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | Little-endian `WVIO` |
| 4 | 4 | format version | `1` |
| 8 | 4 | structure bytes | `48` |
| 12 | 4 | platform | `1` for Windows or `2` for Linux |
| 16 | 4 | flags | Bit 0 console present; bit 1 diagnostic present; all other bits zero |
| 20 | 4 | reserved | Required zero |
| 24 | 8 | console target | Windows file handle or zero-extended Linux file descriptor; zero when absent |
| 32 | 8 | diagnostic target | Windows file handle or zero-extended Linux file descriptor; zero when absent |
| 40 | 8 | Windows writer | Exact `WriteFile` address on Windows; required zero on Linux |

The [Windvale output-table constructor](Windvale-Native-Output-Table-Construction.md) owns the exact table bytes. The C# Stage 0 adapter projects its bounded request, pins the supplied safe handles for the complete native call, resolves the Windows writer, independently checks and copies the response, rereads every byte and host input, and releases the table afterward. Each required channel must belong to the current platform and pass preflight before executable publication. The caller owns externally supplied handles; the execution context owns only a bounded lifetime reference. Standard-output helpers expose process stdout and stderr without transferring ownership.

## `console.write_line` service

The baseline accepts `console.write_line(text) -> void` and `diagnostic.write_line(text) -> void` as separate exact hosted capabilities. Their arguments are borrowed-text descriptors backed by strict UTF-8 fragment data or execution-owned immutable buffers. Text parameters, locals, calls, and returns copy complete descriptors.

Generated code calls the service-table entry with this Windvale-owned internal convention:

- `R8` is the address of verified immutable UTF-8 bytes;
- `R9D` is their bounded byte length; and
- `EAX` is zero on success and nonzero on service failure.

The runtime owns one exact platform leaf per required output service. Windows uses a 258-byte leaf that calls the table's verified `WriteFile` pointer with the selected handle. Linux uses a 213-byte leaf and direct x86-64 `write` syscalls. Both preserve `R10`, `R11`, and `R15`, write the complete strict-UTF-8 byte span with checked partial-write loops, then write one LF byte. Linux retries `EINTR`; zero, oversized, or failed writes become detail 4 / `WVR3029`. Empty text still emits one LF. Console and diagnostic leaves differ only in the verified table-field displacement. Generated fragment bytes remain identical across hosts; the runtime-selected leaf is outside the WVO fragment.

The canonical output leaves are generated by the focused portable Windvale
Windows and Linux modules and the shared native service-code builder. The
retained bridge concatenates Windows console, Windows diagnostic, Linux
console, and Linux diagnostic leaves in that order. Normal runtime loading
uses the four separate exact-length and SHA-256-bound `.bin` artifacts; it does
not execute or embed the generator WVB.

Before allocating executable memory, the runtime requires explicit authorization for each output service (`WVR3010`) and an available channel (`WVR3001`). The fragment verifier proves that each service input is a complete borrowed-text descriptor backed by immutable fragment data or an execution-owned allocation and already bounded by the WVB UTF-8 limit. No text decoding or managed callback occurs on the native output path. The Windows console's display encoding is process policy; Windvale always supplies strict UTF-8 bytes to the selected handle.

## Hosted input services

`process.argument_count` has no generated-code arguments and returns the prevalidated `u32` count in `EAX`. Its exact 5-byte native leaf reads context offset 80 and returns. `process.argument` receives the index in `R8D`, receives a verified output-descriptor address in `R9`, and returns status in `EAX`. Its exact 70-byte native leaf clears failure detail, checks unsigned index against the context count before reading the table, and copies one complete descriptor. An out-of-range index writes detail 3 and returns status one without loading the table. Both leaves preserve `R10`, `R11`, and `R15`; neither uses a Windows/System V adapter. Their live machine bytes are instantiated from the exact WVA-authored `WVSP 1` and `WVSP 2` objects defined by the [native-stencil contract](Wva-Native-Stencil.md).

One `Nativeˉexecutionˉbuffers` owner exists for one native run. When either argument service is required, it eagerly encodes the already validated snapshot into one packed immutable allocation and one contiguous descriptor table. Before publishing the context, it independently rereads every table field and byte sequence, checks all ranges and zero reserved fields, and requires exact agreement with the resource snapshot. Zero arguments publish zero pointer and count. The existing limits remain 67 arguments, 4 KiB per argument, and 64 KiB total. All argument allocations expire after native return and no process address enters portable code, WVB, or WVO.

`file.read_bytes` receives resource-name pointer/length in `R8`/`R9D`, receives a verified output-descriptor address in `RCX`, and returns status in `EAX`. The exact Windows leaf validates and converts the path, then calls verified Win32 file functions. The exact Linux leaf validates and copies the path, then issues direct `openat`, `read`, and `close` syscalls. Neither path uses a managed callback or platform argument thunk.

Each successful exact ordinal name publishes one immutable execution-owned snapshot. Repeated reads return the first descriptor without reopening the file. Failed reads publish nothing. The native contract retains the reference boundary of 64 distinct snapshots and 4 MiB per result; invalid name, not found, denied, unavailable, oversized, and 65th-name failures retain `WVR3021` through `WVR3025` and `WVR3028`.

Services write pointer/length/reserved descriptors only into independently verified frame slots. Resource-name and output input ranges have compiler-verified provenance. Published file records and all table state are independently reread after execution; unknown native service detail uses `WVR3013`.

The canonical file-input leaves are generated by shared and platform-focused
portable Windvale modules through the native service-code builder. The retained
bridge concatenates the Windows and Linux leaves in that order. Normal runtime
loading uses the two separate exact-length and SHA-256-bound `.bin` artifacts;
it does not execute or embed the generator WVB.

## Runtime-private file-input table

File-input-table version 1 is exactly 136 bytes and is never serialized into WVB or WVO:

| Offset | Bytes | Field | Version-1 rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | Little-endian `WVFI` |
| 4 | 4 | format version | `1` |
| 8 | 4 | structure bytes | `136` |
| 12 | 4 | platform | `1` Windows; `2` Linux |
| 16 | 8 | snapshot-table pointer | 64 execution-owned 32-byte records |
| 24 | 4 | snapshot capacity | `64` |
| 28 | 4 | snapshot count | Starts at zero; native success advances it after publishing a complete record |
| 32 | 8 | name-arena pointer | Canonical 64-slot arena |
| 40 | 4 | name stride | 1 MiB |
| 44 | 4 | name reserved | Zero |
| 48 | 8 | data-arena pointer | Canonical 64-slot arena |
| 56 | 4 | data stride | 4 MiB |
| 60 | 4 | maximum data bytes | 4 MiB |
| 64 | 8 | path scratch pointer | Execution-owned UTF-8 or UTF-16 path scratch |
| 72 | 4 | path scratch bytes | Exact platform capacity |
| 76 | 4 | reserved | Zero |
| 80 | 56 | Windows function pointers | UTF-8 conversion, open, size, read, close, commit, and last-error; all zero on Linux |

Each snapshot record contains name pointer/length/reserved at offsets 0/8/12 and data pointer/length/reserved at 16/24/28. Record `i` must point to canonical name slot `i * 1 MiB` and data slot `i * 4 MiB`. After native return, the owner verifies count, every pointer and bound, zero reserved fields, strict UTF-8 names, and ordinal uniqueness before releasing the execution.

## Runtime-private file-output table

File-output-table version 1 is exactly 80 bytes and is never serialized into WVB or WVO:

| Offset | Bytes | Field | Version-1 rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | Little-endian `WVFO` |
| 4 | 4 | format version | `1` |
| 8 | 4 | structure bytes | `80` |
| 12 | 4 | platform | `1` Windows; `2` Linux |
| 16 | 8 | path scratch pointer | Execution-owned UTF-8 or UTF-16 path scratch |
| 24 | 4 | path scratch bytes | Exact platform capacity |
| 28 | 4 | reserved | Zero |
| 32 | 48 | Windows function pointers | UTF-8 conversion, create/replace, write, durable flush, close, and last-error; all zero on Linux |

`file.write_bytes` receives byte pointer/length in `RCX`/`EDX` and resource-name pointer/length in `R8`/`R9D`. Its generated descriptors retain compiler-verified provenance and the existing 4 MiB/1 MiB value limits. The exact 787-byte Windows leaf has SHA-256 `a331248b12fc5830587f6fd8ddf06a546859b8f57366e205032aa2c37db48bb1`; it converts strict UTF-8 and calls the table's verified file functions. The exact 823-byte Linux leaf has SHA-256 `fc688f2a84936dc1082fcb5654667a8a60b0581bff29b1868d48ef2d4af77422`; it uses direct `openat`, `write`, `fsync`, and `close` system calls.

Both leaves reject empty, embedded-NUL, or oversized inputs, complete partial writes, durably flush before success, and contain expected platform errors as existing file details 5 through 9. The operation creates or truncates one file and is deliberately non-atomic; a failure may leave a created, truncated, or partial file. The [Windvale file-output-table constructor](Windvale-Native-File-Output-Table-Construction.md) owns the exact `WVFO` bytes from already allocated scratch and resolved opaque functions. The host independently verifies every static table byte before publication and after return. [Windvale-Native-File-Output.md](Windvale-Native-File-Output.md) defines the complete boundary and deliberate omissions.

The canonical file-output leaves are generated by shared and platform-focused
portable Windvale modules through the native service-code builder. The retained
bridge concatenates the Windows and Linux leaves in that order. Normal runtime
loading uses the two separate exact-length and SHA-256-bound `.bin` artifacts;
it does not execute or embed the generator WVB.

## Pure UTF-8 validation service

`Textˉutf8ˉisˉvalid(bytes) -> bool` passes one proven borrowed-byte pointer/length in `R8`/`R9D` and a verified bool output-cell address in `RCX`. [Decision 0070](../Documents/Decisions/0070-First-Runtime-Native-Utf8-Service.md) replaces its managed callback and platform adapters with one exact 800-byte runtime-native x86-64 leaf shared by Windows and Linux. [Decision 0355](../Documents/Decisions/0355-Windvale-Owned-Native-Utf8-Service-Construction.md) transfers exact construction of that unchanged leaf to focused Windvale source and an import-free byte-result bridge. [Decision 0364](../Documents/Decisions/0364-Direct-Fixed-Native-Service-Leaf-Consumption.md) makes the exact generated artifact, rather than live generator-WVB execution, the normal runtime input while retaining source and WVB reproduction evidence. The leaf writes normalized zero or one, returns status zero in `EAX`, and preserves `R10`, `R11`, and `R15`. Invalid encoding writes false; it does not allocate text or gain a capability. Fragment verification plus immutable execution-allocation ownership proves the range before the call; no arbitrary native pointer is accepted.

`Textˉfromˉutf8(bytes) -> text` uses that service as a proof step. False branches to packed status 8 / `WVR3014`; true copies the already-bounded borrowed descriptor as text. It does not allocate or silently replace malformed input.

## Dynamic text and byte arena

One native run owns one bounded dynamic-value arena retained at the context's historical text-arena offsets: 128 MiB in the ordinary host executor and version-2/3 hosted containers, and 16 MiB in each narrow version-1 console container. `Enumˉname`, integer formatting, `Textˉconcat`, and `Textˉquote` allocate strict UTF-8 results by reading and advancing that cursor. Generated fixed-width byte constructors and `Bytesˉconcat` share the cursor; owned byte headers and function-return compaction may reuse only independently verified regions. Each non-entry descriptor-returning function saves the entry cursor in its hidden result cell. Returning pre-existing storage restores the cursor, returning wholly internal storage compacts it to the checkpoint, and a boundary-spanning result preserves the arena. Scalar-only direct-record returns restore their checkpoint; descriptor-bearing aggregates do not relocate without caller-liveness evidence. Text values retain the WVB 1 MiB bound (`WVR3012`), byte concatenation retains the 4 MiB bound (`WVR3015`), invalid two-byte encoder inputs retain `WVR3016`, and checked aggregate exhaustion becomes `WVR3018`. Public and host descriptors carry generation zero, are accepted by the same range validator as immutable argument and file buffers, and expire when `Main` returns.

Enum naming receives a verified nominal type index and signed enum value and returns the exact declared member name. Its fixed native leaf reads an adjacent canonical runtime-private `WVEN` version-1 block derived from the fragment's verified nominal declarations. The 24-byte header records magic, version, total bytes, nominal type count, enum member count, and the type-directory offset. Each type directory entry is 8 bytes; records have zero members. Each 16-byte member entry records the signed value, absolute metadata-relative strict-UTF-8 name offset and length, and a zero reserved field. Concatenated names follow the member entries. The complete block is limited to 32 MiB, independently parsed against the verified declarations, deterministically reconstructed, and required to match byte for byte before publication. [Decision 0359](../Documents/Decisions/0359-Windvale-Owned-Native-Enum-Name-Leaf.md) transfers the unchanged 323-byte executable leaf into one compact Windvale machine-template source and import-free byte-result bridge. [Decision 0363](../Documents/Decisions/0363-Direct-Native-Enum-Name-Leaf-Consumption.md) makes the exact generated leaf, rather than live generator-WVB execution, the normal runtime input while retaining the source and WVB for reproduction and recovery. [Decision 0361](../Documents/Decisions/0361-Windvale-Owned-Bounded-Native-Enum-Metadata.md) first transfers canonical `WVEN` construction through 4 MiB. [Decision 0362](../Documents/Decisions/0362-Windvale-Owned-Segmented-Native-Enum-Metadata.md) completes the 32 MiB constructor transfer with strict runtime-private `WVEQ` version-2 group requests and `WVEC` version-1 response envelopes. Groups contain complete nominal types, at most 2,048 members, and independently fit the 4 MiB input and result bounds. The temporary managed session projects requests, validates envelopes, concatenates already constructed header/directory/member/name sections, and independently validates the complete result; it contains no `WVEN` field writer.

Exact native `I32ˉformat` and `U32ˉformat` leaves use invariant decimal with no grouping and cover signed minimum, unsigned maximum, and zero directly. [Decision 0356](../Documents/Decisions/0356-Windvale-Owned-Native-Integer-Format-Construction.md) transfers construction of both unchanged leaves to one focused Windvale generator and import-free paired byte-result bridge. Exact native concatenation checks the combined encoded length before reserving and copying bytes. [Decision 0357](../Documents/Decisions/0357-Windvale-Owned-Native-Text-Concatenation-Construction.md) transfers construction of that unchanged leaf to a focused Windvale generator, shared service-code builder, and import-free byte-result bridge. Exact native quoting makes two passes over strict UTF-8: it first validates and measures the complete UTF-16-code-unit escape representation and proves both limits, then writes it. Printable ASCII is preserved; quote, reverse solidus, and the five short control escapes use their short forms; other ASCII controls and non-ASCII BMP values use uppercase `\uXXXX`; supplementary scalars use two uppercase surrogate escapes. [Decision 0358](../Documents/Decisions/0358-Windvale-Owned-Native-Text-Quote-Leaf.md) transfers the unchanged x64 implementation into one compact Windvale machine-template source and import-free byte-result bridge while leaving the semantic contract and behavioral oracle independent of that template. [Decision 0364](../Documents/Decisions/0364-Direct-Fixed-Native-Service-Leaf-Consumption.md) makes all five exact generated leaves the normal runtime inputs and retains their generator WVBs only for reproduction, qualification, and recovery evidence.

Before W^X publication, the runtime requires exact [Decision 0071](../Documents/Decisions/0071-Native-Text-Arena-And-Core-Text-Services.md) identities for the 249-byte concatenation leaf, 225-byte signed formatter, and 191-byte unsigned formatter. [Decision 0072](../Documents/Decisions/0072-Final-Pure-Runtime-Native-Services.md) additionally requires the exact 323-byte enum-name leaf and its independently verified `WVEN` block plus the exact 1,165-byte quote leaf. All preserve `R10`, `R11`, and `R15`. Their service inputs and output-cell addresses remain compiler-generated and independently verified; context state is runtime-owned for the call.

## Verification and publication

The native fragment verifier independently decodes the exact context prologue, `R15` restoration on every exit, typed 16-byte frame access, hidden descriptor and record-result cells, scalar/descriptor/record/void call and return kinds, complete descriptor construction/copy/provenance, function-entry arena checkpoints, descriptor compaction returns, scalar-record rollback, owned-buffer header/generation/capacity checks, enum operations, typed frame-owned record construction/copy/field/return shapes, bounded dynamic-byte allocation/copy loops, fixed-width byte construction and range guards, unsigned bounds branches, fixed-width reads, all service-table loads and argument forms, UTF-8 and runtime failure edges, immutable data targets, relocations, and packed statuses. Record and descriptor stack forms are distinguished by decoding the complete two-word descriptor before the one-word record pointer. The runtime loads exact output, file-input, and file-output artifacts, checks lengths, SHA-256 identities, and complete bytes, and rejects corrupted leaves or tables before writable-to-executable publication. Corrupt fragment instructions, descriptor fields, ownership thresholds, checkpoints, record tags, frame ranges, encoder limits or widths, hidden results, arena sizes or offsets, argument forms, displacement bytes, service metadata, or immutable data fail before WVO serialization or publication.

The current `Nativeˉfragment` carries its required-service list beside code, symbols, and patches. WVO 1.0 does not serialize that list. A service-bearing linked image may therefore execute only while paired with its original verified fragment metadata; it is not yet a standalone native application. `windows-x64-console-v1` and `linux-x64-console-v1` close only the service-free scalar case by requiring an empty list before WVO production. A future hosted PE, ELF, or Windvale-native container must preserve and verify capability/service requirements before it can publish independently loadable hosted AOT modules.

## Windvale OS use

The retained version-10 native kernel bridge constructs context version 7 with exact budgets `271` and `2`, a zero service-table pointer, zero-length record and text arenas, zero argument table/count, zero output, file-input, and file-output table pointers, and zero failure/reserved fields. The ordinary portable module loops over immutable i32 data, passes borrowed bytes through an internal function, slices and reads them, and checks `u8`/`u32` results. The pre-paging firmware probe-20 baseline cross-host qualifies that service-free ABI-15 path at exact commit `12e9e2e` while retaining normalized invalid-opcode/general-protection entries and WVA-owned Q35 shutdown. Exact commit `860c69c` retains the context contract and identical probe machine bytes under ABI 16 plus kernel-owned paging.

Decision 0090 adds a separate ABI-16 context instance with exact budgets `8948` and `2`. The AOT Windvale admission module consumes 8,944 instructions while validating one embedded canonical WVB; its admitted AOT program consumes the remaining four instructions. Both are capability-free and use the same all-zero service/resource fields. Only accepted token 73 followed by program result 29 permits a tail transfer to the retained 271/2 bridge. Qualified probe 21 emits `native-context=pass` only after both contexts and special-kernel Main succeed under the new root.

[Decision 0095](../Documents/Decisions/0095-First-Runtime-Supplied-Wvb-Boot-Resource.md) adds the first narrow Windvale OS service-bearing context without changing ABI 16, context 7, or service-table version 5. Process `2` publishes only the `file.read_bytes` slot and uses context offset 96 for one OS-private 32-byte `WVBR` version-1 table rather than the Windows/Linux `WVFI` table. Its exact WVA-owned 199-byte leaf accepts only `boot:main.wvb` and returns a borrowed descriptor into one RO/NX page. This is a platform adapter for the same generated call convention; `WVBR` is not added to the Windows/Linux host contract.

[Decision 0096](../Documents/Decisions/0096-First-Windvale-Init-Owned-Boot-Resource-Grant.md) preserves those exact ABI bytes but changes publication timing. Process `2` begins with context offsets 24 and 96 zero and without its resource PTE. Windvale init selects resource `1`; one checked kernel grant then installs the RO/NX alias, materializes service-table version 5 and `WVBR` version 1 in the client data page, and publishes both context pointers before client entry. Init remains owner and process `2` is the fixed borrower. The generated call and its platform-private table are unchanged.

[Decision 0098](../Documents/Decisions/0098-First-Typed-Two-Resource-Lookup.md) retains ABI 16, context 7, and service-table 5 while replacing the OS-private table with `WVBR002`. One atomic grant publishes two ordered entries and two RO/NX aliases for `boot:main.wvb` and `boot:main.budget`. The exact 347-byte WVA leaf performs typed name lookup and returns either borrowed descriptor through the unchanged `file.read_bytes` generated-call convention. Both context pointers still transition from zero to their final values only after the complete set validates; terminal cleanup returns them to zero and clears the complete 80-byte directory.

[Decision 0100](../Documents/Decisions/0100-First-Reclaimed-And-Reused-Process-Root.md) advances the composed OS to ABI 17 without changing context 7, service-table 5, `WVBR002`, or hosted-call semantics. Probe 30 constructs the same exact table twice for generation-stamped clients at one reused physical root; cleanup clears the complete table before tail release and rebuild.

[Decision 0101](../Documents/Decisions/0101-First-Exact-Wvb-Across-Three-Environments.md) replaces the fixed guest sample with the exact canonical `Sum-Data.wv` compiler output in Probe 31. [Decision 0103](../Documents/Decisions/0103-Second-Exact-Wvb-And-Broader-Scalar-Control-Flow.md) advances Probe 32 to the exact `Function-Only.wv` WVB, four functions, four scalar families, and a section-derived 199-instruction execution proof. Both execute in two protected generations through the same context/resource boundary.

[Decision 0105](../Documents/Decisions/0105-Typed-Block-Scoped-Native-Value-Slots.md) advances the shared backend and Probe 32 to ABI 18 while retaining context 7 and all guest-visible service shapes. Typed block-scoped reuse reduces the exact generated frames and permits `WVKMEM10` to compact the client root and kernel arena. [Decision 0108](../Documents/Decisions/0108-Native-One-Byte-Construction.md) cross-host qualifies ABI 19 without changing any operation reachable by Probe 32. Qualified [Decision 0109](../Documents/Decisions/0109-Native-Two-Byte-Little-Endian-Construction.md) similarly advances the shared implementation to ABI 20 without changing the retained guest bytes; all four pinned Windows QEMU scenarios retain their exact identities.

[Decision 0150](../Documents/Decisions/0150-Bounded-Native-Dynamic-Value-Lifetimes.md) advances the shared host/OS selector and verifier to ABI 22 while retaining context 7 and ABI 21's frame plan. Rebuilt Probe 34 still needs 755 cells and a deepest 24,240-byte call path in the retained six-page client stack; generated record operations leave the 1,024-byte in-page record arena unused. The 447,757-byte normal client needs 110 RX pages; `WVKMEM13` composes its 121-page root with Probe 34's retained 11-page init/resource extent inside a 144-page kernel arena. The exact guest WVB, `WVPROC13`, resource behavior, and firmware/serial formats do not change.

Windvale OS still has no general runtime service registry, output filesystem, general native text/byte allocator, general WVB loader/verifier, JIT, or dynamic hosted-capability implementation. The rebuilt Probe 32 candidate preserves generated ABI-22 hosted code that consumes two typed Windvale-selected immutable OS resources across one generation-safe reuse cycle and executes two exact compiler-produced programs without changing guest semantics. Stage 0 still writes the table and PTEs after independently checking Windvale-owned policy and WVA-owned leaf bytes; those are named replacement seams rather than portable semantics.
