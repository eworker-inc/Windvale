# Windvale native execution context

## Status and scope

Execution-context version 7 and ABI-16 target `x86-64-wvb-baseline-v16` are cross-host qualified at exact commit `860c69c`. [Decision 0087](../Documents/Decisions/0087-Native-Windows-And-Linux-File-Output.md) first qualified context version 7 under ABI 15 with one runtime-private file-output-table pointer and exact native Windows/Linux `file.write_bytes` leaves. [Decision 0089](../Documents/Decisions/0089-Bounded-Native-Stack-Arguments.md) advances only bounded internal calls to ABI 16 while retaining every context and service-table field. Execution-context version 6 and ABI 14 remain cross-host-qualified historical evidence at exact commit `ef08619`; [Decision 0076](../Documents/Decisions/0076-Native-Windows-And-Linux-File-Input.md) records that transition. [Decision 0080](../Documents/Decisions/0080-Native-Byte-Result-And-Live-Stencil-Consumption.md) adds the bounded exported byte-result contract without advancing version 6, and [Decision 0082](../Documents/Decisions/0082-Windvale-Owned-Native-Publication-Layout.md) moves executable-image layout into Windvale without changing ABI 14.

This is an experimental native ABI, not a stable public foreign-function interface. ABI 16 replaces ABI 15 in the current implementation. Qualified older artifacts remain historical evidence and are not accepted by the ABI-16 fragment verifier.

## Entry convention

The exported native `Main() -> i32`, or capability-free portable `Main() -> bytes`, receives a pointer to one execution context in `RDX`. Hosted byte entries are not admitted in this slice because their argument/file borrows require additional result-owner validation. The executor duplicates the context pointer into its second Windows and third System V bridge arguments so both Windows x64 and System V x86-64 place the same value in `RDX`. Generated `Main` preserves `R15`, copies the context pointer into `R15`, and loads the instruction and call-depth budgets into the shared `R11` and `R10` counters. Scalar entry code and invocation remain unchanged.

For `Main() -> bytes`, the executor also duplicates one verified 16-byte result-cell pointer into its first Windows and fourth System V bridge arguments; both host conventions therefore place the pointer in physical `RCX`. The generated entry copies `RCX` to `RAX` immediately after frame allocation and then uses the ordinary hidden-result convention below. The independent decoder requires that exact copy when and only when exported `Main` has a descriptor result. The executor classifies the decoded result before W^X publication, so the scalar and byte APIs cannot invoke the wrong entry shape.

Internal functions accept at most 64 parameters, matching the source-language declaration limit. The first four positions retain `R8`, `R9`, `RCX`, and `RDX`; `i32`, `bool`, `u8`, `u32`, enums, and record-arena offsets use the low dword, while borrowed `text` or `bytes` uses the complete register as a pointer to the caller's verified 16-byte descriptor. For positions 4 through 63, the caller reserves exactly one 16-byte outgoing cell per parameter. Scalars occupy the low dword and borrowed descriptors copy both machine words. After allocating its own frame, the callee copies each later cell from `RSP + frame-bytes + 8 + (position - 4) * 16`; the eight-byte term is the internal return address. The caller releases the exact reservation before testing the packed status. The maximum outgoing reservation is 960 bytes and the fragment verifier reconstructs its size, cell offsets and types, hidden descriptor-result adjustment, call target, release, and callee agreement. Packed scalar/enum/record value and status returns use `RAX`, and every internal call preserves the shared resource counters.

For a `text` or `bytes` return, the caller places its verified result-cell address in `RAX` after loading explicit arguments. The callee saves that hidden pointer in one dedicated final frame cell before clearing ordinary locals and temporaries. A successful return copies both descriptor words to the hidden result and returns zero in `RAX`; traps retain their packed nonzero status. A void call uses the same status path but has no hidden result or stored scalar. The independent decoder cross-checks every call shape against the callee's single decoded return kind.

After a successful exported byte return, the host requires a zero reserved word, a length no greater than 4 MiB, and a complete pointer range inside either one exact immutable fragment-data symbol or the committed used prefix of the execution arena. It copies the accepted result before releasing the fragment, context, cell, and arena. A null pointer is accepted only for an empty result. Every other descriptor fails as `WVN4012`; a descriptor cannot escape its run. This exported byte-result contract is cross-host qualified under Decision 0080 at exact commit `f547af8dcf8e257ab8ad8a76a49bbdd1b9136677`.

## Value-slot and borrowed-descriptor layout

Each native local and temporary owns one zero-initialized 16-byte frame slot. Scalars use the low four bytes. The wider slot is a universal representation boundary for values that require more than one machine word; it is not a heap object or a claim that every future Windvale value will use this exact shape.

An immutable borrowed `text` or `bytes` value occupies one slot:

| Offset | Bytes | Field | ABI-16 rule |
| ---: | ---: | --- | --- |
| 0 | 8 | data pointer | Points into verified fragment data or one execution-owned immutable host buffer |
| 8 | 4 | byte length | Exact remaining span length |
| 12 | 4 | reserved | Zero |

Static text and byte constants create descriptors through verified RIP-relative data references. A native service may return a descriptor backed by its execution-owned immutable buffer; that borrow expires when the native call returns. `Textˉtoˉutf8` copies the already-valid text descriptor as borrowed bytes without allocation. `Bytesˉfromˉu32ˉlittle` writes four bytes and `Bytesˉconcat` copies both inputs into the shared execution arena; concatenation preserves the 4 MiB WVB value bound as `WVR3015`, while aggregate arena exhaustion remains `WVR3018`. `Bytesˉslice` produces another borrowed descriptor only after unsigned offset/length bounds checks. `Bytesˉreadˉu8`, `Bytesˉreadˉu16ˉlittle`, `Bytesˉreadˉu32ˉlittle`, and `Bytesˉreadˉi32ˉlittle` check the complete fixed-width range before reading. A failed slice or read returns packed status 6 and becomes `WVR3008`; no host signal or unchecked pointer access is the language-level failure path.

An enum occupies the low four bytes and retains its signed member value plus compile-time nominal identity. A record occupies the low four bytes as an offset into the current execution's record arena. Each immutable record field consumes one complete 16-byte arena cell, including borrowed text or byte descriptors. Record construction checks offset addition and arena capacity before copying typed cells; field access proves the complete selected cell is below the committed used boundary. A fragment containing descriptor-bearing records uses independently decoded nominal-type tags on all record construction and field-access shapes so descriptor provenance cannot be confused with a scalar cell. Packed status 7 becomes `WVR3017` on arena exhaustion.

ABI 16 retains ABI 15's native file output, ABI 14's native file input, ABI 13's native output, ABI 12's immutable argument table, ABI 11's return shapes and arenas, ABI 9's nominal values, and ABI 8's borrowed values. Unsigned arithmetic overflow retains packed status 1 / `WVR3007`.

## Execution-context memory layout

All integer fields are little-endian. The context is exactly 112 bytes:

| Offset | Bytes | Field | Version-7 rule |
| ---: | ---: | --- | --- |
| 0 | 4 | format version | `7` |
| 4 | 4 | structure bytes | `112` |
| 8 | 8 | instruction budget | Positive maximum charged with WVB instruction semantics |
| 16 | 8 | call-depth budget | Positive maximum active native call depth |
| 24 | 8 | service-table pointer | Zero when no runtime service is required; otherwise points to the exact table below |
| 32 | 8 | record-arena pointer | Execution-owned base; may be zero only when arena length is zero and the module performs no record construction |
| 40 | 4 | record-arena bytes | At most 1 MiB in the current host executor |
| 44 | 4 | record-arena used bytes | Starts at zero; generated checked construction advances it in 16-byte cells |
| 48 | 8 | text-arena pointer | Execution-owned dynamic text/byte base; may be zero only when no admitted dynamic value allocates |
| 56 | 4 | text-arena bytes | At most 16 MiB in the current host executor |
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

Every status-bearing service returns zero in `EAX` on success and nonzero on service failure; `process.argument_count` returns its `u32` value directly. A native leaf clears context service-failure detail on entry. Details `1` through `4` retain text-value, text-arena, argument-index, and output-write failures. Details `5` through `10` identify invalid file name, not found, permission denied, unavailable, too large, and snapshot limit. Detail `11` identifies a generated `Bytesˉconcat` result above the 4 MiB value bound. Generated code retains packed status 5; the executor translates those details to the stable `WVR3012`, `WVR3018`, `WVR3020`, `WVR3029`, `WVR3021` through `WVR3025`, `WVR3028`, and `WVR3015` codes. An absent or unknown detail remains `WVR3013`. Native services and generated allocation failures do not unwind host exceptions through generated code.

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

The C# Stage 0 executor still constructs this table, pins the supplied safe handles for the complete native call, independently rereads every byte and host input, and releases the table afterward. Each required channel must belong to the current platform and pass preflight before executable publication. The caller owns externally supplied handles; the execution context owns only a bounded lifetime reference. Standard-output helpers expose process stdout and stderr without transferring ownership.

## `console.write_line` service

The baseline accepts `console.write_line(text) -> void` and `diagnostic.write_line(text) -> void` as separate exact hosted capabilities. Their arguments are borrowed-text descriptors backed by strict UTF-8 fragment data or execution-owned immutable buffers. Text parameters, locals, calls, and returns copy complete descriptors.

Generated code calls the service-table entry with this Windvale-owned internal convention:

- `R8` is the address of verified immutable UTF-8 bytes;
- `R9D` is their bounded byte length; and
- `EAX` is zero on success and nonzero on service failure.

The runtime owns one exact platform leaf per required output service. Windows uses a 258-byte leaf that calls the table's verified `WriteFile` pointer with the selected handle. Linux uses a 213-byte leaf and direct x86-64 `write` syscalls. Both preserve `R10`, `R11`, and `R15`, write the complete strict-UTF-8 byte span with checked partial-write loops, then write one LF byte. Linux retries `EINTR`; zero, oversized, or failed writes become detail 4 / `WVR3029`. Empty text still emits one LF. Console and diagnostic leaves differ only in the verified table-field displacement. Generated fragment bytes remain identical across hosts; the runtime-selected leaf is outside the WVO fragment.

Before allocating executable memory, the runtime requires explicit authorization for each output service (`WVR3010`) and an available channel (`WVR3001`). The fragment verifier proves that each service input is a complete borrowed-text descriptor backed by immutable fragment data or an execution-owned allocation and already bounded by the WVB UTF-8 limit. No text decoding or managed callback occurs on the native output path. The Windows console's display encoding is process policy; Windvale always supplies strict UTF-8 bytes to the selected handle.

## Hosted input services

`process.argument_count` has no generated-code arguments and returns the prevalidated `u32` count in `EAX`. Its exact 5-byte native leaf reads context offset 80 and returns. `process.argument` receives the index in `R8D`, receives a verified output-descriptor address in `R9`, and returns status in `EAX`. Its exact 70-byte native leaf clears failure detail, checks unsigned index against the context count before reading the table, and copies one complete descriptor. An out-of-range index writes detail 3 and returns status one without loading the table. Both leaves preserve `R10`, `R11`, and `R15`; neither uses a Windows/System V adapter. Their live machine bytes are instantiated from the exact WVA-authored `WVSP 1` and `WVSP 2` objects defined by the [native-stencil contract](Wva-Native-Stencil.md).

One `Nativeˉexecutionˉbuffers` owner exists for one native run. When either argument service is required, it eagerly encodes the already validated snapshot into one packed immutable allocation and one contiguous descriptor table. Before publishing the context, it independently rereads every table field and byte sequence, checks all ranges and zero reserved fields, and requires exact agreement with the resource snapshot. Zero arguments publish zero pointer and count. The existing limits remain 67 arguments, 4 KiB per argument, and 64 KiB total. All argument allocations expire after native return and no process address enters portable code, WVB, or WVO.

`file.read_bytes` receives resource-name pointer/length in `R8`/`R9D`, receives a verified output-descriptor address in `RCX`, and returns status in `EAX`. The exact Windows leaf validates and converts the path, then calls verified Win32 file functions. The exact Linux leaf validates and copies the path, then issues direct `openat`, `read`, and `close` syscalls. Neither path uses a managed callback or platform argument thunk.

Each successful exact ordinal name publishes one immutable execution-owned snapshot. Repeated reads return the first descriptor without reopening the file. Failed reads publish nothing. The native contract retains the reference boundary of 64 distinct snapshots and 4 MiB per result; invalid name, not found, denied, unavailable, oversized, and 65th-name failures retain `WVR3021` through `WVR3025` and `WVR3028`.

Services write pointer/length/reserved descriptors only into independently verified frame slots. Resource-name and output input ranges have compiler-verified provenance. Published file records and all table state are independently reread after execution; unknown native service detail uses `WVR3013`.

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

Both leaves reject empty, embedded-NUL, or oversized inputs, complete partial writes, durably flush before success, and contain expected platform errors as existing file details 5 through 9. The operation creates or truncates one file and is deliberately non-atomic; a failure may leave a created, truncated, or partial file. The owner independently verifies every static table byte before publication and after return. [Windvale-Native-File-Output.md](Windvale-Native-File-Output.md) defines the complete boundary and deliberate omissions.

## Pure UTF-8 validation service

`Textˉutf8ˉisˉvalid(bytes) -> bool` passes one proven borrowed-byte pointer/length in `R8`/`R9D` and a verified bool output-cell address in `RCX`. [Decision 0070](../Documents/Decisions/0070-First-Runtime-Native-Utf8-Service.md) replaces its managed callback and platform adapters with one exact 800-byte runtime-native x86-64 leaf shared by Windows and Linux. The leaf writes normalized zero or one, returns status zero in `EAX`, and preserves `R10`, `R11`, and `R15`. Invalid encoding writes false; it does not allocate text or gain a capability. Fragment verification plus immutable execution-allocation ownership proves the range before the call; no arbitrary native pointer is accepted.

`Textˉfromˉutf8(bytes) -> text` uses that service as a proof step. False branches to packed status 8 / `WVR3014`; true copies the already-bounded borrowed descriptor as text. It does not allocate or silently replace malformed input.

## Dynamic text and byte arena

One native run owns one fixed 16 MiB monotonic dynamic-value arena retained at the context's historical text-arena offsets. `Enumˉname`, integer formatting, `Textˉconcat`, and `Textˉquote` allocate strict UTF-8 results by reading and advancing that cursor. Generated `Bytesˉfromˉu32ˉlittle` and `Bytesˉconcat` use the same cursor for immutable byte results. Text values retain the WVB 1 MiB bound (`WVR3012`), byte concatenation retains the 4 MiB bound (`WVR3015`), and checked aggregate exhaustion becomes `WVR3018`. Arena descriptors are accepted by the same range validator as immutable argument and file buffers and expire when `Main` returns.

Enum naming receives a verified nominal type index and signed enum value and returns the exact declared member name. Its fixed native leaf reads an adjacent canonical runtime-private `WVEN` version-1 block derived from the fragment's verified nominal declarations. The 24-byte header records magic, version, total bytes, nominal type count, enum member count, and the type-directory offset. Each type directory entry is 8 bytes; records have zero members. Each 16-byte member entry records the signed value, absolute metadata-relative strict-UTF-8 name offset and length, and a zero reserved field. Concatenated names follow the member entries. The complete block is limited to 32 MiB, independently parsed against the verified declarations, deterministically reconstructed, and required to match byte for byte before publication.

Exact native `I32ˉformat` and `U32ˉformat` leaves use invariant decimal with no grouping and cover signed minimum, unsigned maximum, and zero directly. Exact native concatenation checks the combined encoded length before reserving and copying bytes. Exact native quoting makes two passes over strict UTF-8: it first validates and measures the complete UTF-16-code-unit escape representation and proves both limits, then writes it. Printable ASCII is preserved; quote, reverse solidus, and the five short control escapes use their short forms; other ASCII controls and non-ASCII BMP values use uppercase `\uXXXX`; supplementary scalars use two uppercase surrogate escapes.

Before W^X publication, the runtime requires exact [Decision 0071](../Documents/Decisions/0071-Native-Text-Arena-And-Core-Text-Services.md) identities for the 249-byte concatenation leaf, 225-byte signed formatter, and 191-byte unsigned formatter. [Decision 0072](../Documents/Decisions/0072-Final-Pure-Runtime-Native-Services.md) additionally requires the exact 323-byte enum-name leaf and its independently verified `WVEN` block plus the exact 1,165-byte quote leaf. All preserve `R10`, `R11`, and `R15`. Their service inputs and output-cell addresses remain compiler-generated and independently verified; context state is runtime-owned for the call.

## Verification and publication

The native fragment verifier independently decodes the exact context prologue, `R15` restoration on every exit, typed 16-byte frame access, hidden descriptor-result cells, scalar/descriptor/void call and return kinds, descriptor construction/copy and provenance, enum operations, typed record tags and arena cell copies, bounded dynamic-byte allocation/copy loops, unsigned bounds branches, fixed-width reads, all service-table loads and argument forms, UTF-8 and runtime failure edges, immutable data targets, relocations, and packed statuses. The runtime separately reconstructs output, file-input, and file-output leaves, checks exact lengths and SHA-256 identities, and rejects corrupted bytes or tables before writable-to-executable publication. Corrupt fragment instructions, descriptor fields, record tags, byte-value limits, hidden results, arena sizes or offsets, argument forms, displacement bytes, service metadata, or immutable data fail before WVO serialization or publication.

The current `Nativeˉfragment` carries its required-service list beside code, symbols, and patches. WVO 1.0 does not serialize that list. A service-bearing linked image may therefore execute only while paired with its original verified fragment metadata; it is not yet a standalone native application. A future PE, ELF, or Windvale-native container must preserve and verify capability/service requirements before it can publish independently loadable hosted AOT modules.

## Windvale OS use

The retained version-10 native kernel bridge constructs context version 7 with exact budgets `271` and `2`, a zero service-table pointer, zero-length record and text arenas, zero argument table/count, zero output, file-input, and file-output table pointers, and zero failure/reserved fields. The ordinary portable module loops over immutable i32 data, passes borrowed bytes through an internal function, slices and reads them, and checks `u8`/`u32` results. The pre-paging firmware probe-20 baseline cross-host qualifies that service-free ABI-15 path at exact commit `12e9e2e` while retaining normalized invalid-opcode/general-protection entries and WVA-owned Q35 shutdown. Exact commit `860c69c` retains the context contract and identical probe machine bytes under ABI 16 plus kernel-owned paging.

Decision 0090 adds a separate ABI-16 context instance with exact budgets `8948` and `2`. The AOT Windvale admission module consumes 8,944 instructions while validating one embedded canonical WVB; its admitted AOT program consumes the remaining four instructions. Both are capability-free and use the same all-zero service/resource fields. Only accepted token 73 followed by program result 29 permits a tail transfer to the retained 271/2 bridge. Qualified probe 21 emits `native-context=pass` only after both contexts and special-kernel Main succeed under the new root.

[Decision 0095](../Documents/Decisions/0095-First-Runtime-Supplied-Wvb-Boot-Resource.md) adds the first narrow Windvale OS service-bearing context without changing ABI 16, context 7, or service-table version 5. Process `2` publishes only the `file.read_bytes` slot and uses context offset 96 for one OS-private 32-byte `WVBR` version-1 table rather than the Windows/Linux `WVFI` table. Its exact WVA-owned 199-byte leaf accepts only `boot:main.wvb` and returns a borrowed descriptor into one RO/NX page. This is a platform adapter for the same generated call convention; `WVBR` is not added to the Windows/Linux host contract.

[Decision 0096](../Documents/Decisions/0096-First-Windvale-Init-Owned-Boot-Resource-Grant.md) preserves those exact ABI bytes but changes publication timing. Process `2` begins with context offsets 24 and 96 zero and without its resource PTE. Windvale init selects resource `1`; one checked kernel grant then installs the RO/NX alias, materializes service-table version 5 and `WVBR` version 1 in the client data page, and publishes both context pointers before client entry. Init remains owner and process `2` is the fixed borrower. The generated call and its platform-private table are unchanged.

[Decision 0098](../Documents/Decisions/0098-First-Typed-Two-Resource-Lookup.md) retains ABI 16, context 7, and service-table 5 while replacing the OS-private table with `WVBR002`. One atomic grant publishes two ordered entries and two RO/NX aliases for `boot:main.wvb` and `boot:main.budget`. The exact 347-byte WVA leaf performs typed name lookup and returns either borrowed descriptor through the unchanged `file.read_bytes` generated-call convention. Both context pointers still transition from zero to their final values only after the complete set validates; terminal cleanup returns them to zero and clears the complete 80-byte directory.

Windvale OS still has no general runtime service registry, output filesystem, record or text allocator, general WVB loader/verifier, JIT, or dynamic hosted-capability implementation. Qualified Probe 29 proves that ABI-16 generated hosted code can consume two typed Windvale-selected immutable OS resources without changing host ABI semantics. Stage 0 still writes the table and PTEs after independently checking Windvale-owned policy and WVA-owned leaf bytes; those are named replacement seams rather than portable semantics.
