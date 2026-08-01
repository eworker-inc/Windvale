# Windvale native execution context

## Status and scope

Execution-context version 4 is the implemented qualification candidate for target `x86-64-wvb-baseline-v12`. It appends one execution-owned immutable argument table while retaining the qualified text and record arenas, per-run resource limits, and platform-neutral runtime-service boundary. Decisions 0065 through 0068 qualify the seam through ABI 9. [Decision 0069](../Documents/Decisions/0069-Dynamic-Native-Text-And-Complete-Wvdump.md) through [Decision 0072](../Documents/Decisions/0072-Final-Pure-Runtime-Native-Services.md) cross-host qualify ABI 10 and ABI 11, context 3, and all six pure-service leaves. [Decision 0073](../Documents/Decisions/0073-Native-Argument-Table-And-Process-Input-Services.md) defines the ABI-12/context-4 argument-table candidate.

This is an experimental native ABI, not a stable public foreign-function interface. ABI 12 replaces ABI 11 in the current implementation. Qualified older artifacts remain historical evidence and are not accepted by the ABI-12 fragment verifier.

## Entry convention

The exported native `Main() -> i32` receives a pointer to one execution context in `RDX`. The Windows executor duplicates that pointer into its second and third bridge arguments so both Windows x64 and System V x86-64 place the same value in `RDX`. Generated `Main` preserves `R15`, copies the context pointer into `R15`, and loads the instruction and call-depth budgets into the shared `R11` and `R10` counters.

Internal functions accept as many as four parameters. `i32`, `bool`, `u8`, `u32`, enums, and record-arena offsets use `R8D`, `R9D`, `ECX`, and `EDX`. A borrowed `text` or `bytes` parameter uses the corresponding 64-bit register as a pointer to the caller's verified 16-byte descriptor; the callee copies the descriptor into its own frame before the call can return. Packed scalar/enum/record value and status returns use `RAX`, and every internal call preserves the shared resource counters.

For a `text` or `bytes` return, the caller places its verified result-cell address in `RAX` after loading explicit arguments. The callee saves that hidden pointer in one dedicated final frame cell before clearing ordinary locals and temporaries. A successful return copies both descriptor words to the hidden result and returns zero in `RAX`; traps retain their packed nonzero status. A void call uses the same status path but has no hidden result or stored scalar. The independent decoder cross-checks every call shape against the callee's single decoded return kind.

## Value-slot and borrowed-descriptor layout

Each native local and temporary owns one zero-initialized 16-byte frame slot. Scalars use the low four bytes. The wider slot is a universal representation boundary for values that require more than one machine word; it is not a heap object or a claim that every future Windvale value will use this exact shape.

An immutable borrowed `text` or `bytes` value occupies one slot:

| Offset | Bytes | Field | ABI-12 rule |
| ---: | ---: | --- | --- |
| 0 | 8 | data pointer | Points into verified fragment data or one execution-owned immutable host buffer |
| 8 | 4 | byte length | Exact remaining span length |
| 12 | 4 | reserved | Zero |

Static text and byte constants create descriptors through verified RIP-relative data references. A native service may return a descriptor backed by its execution-owned immutable buffer; that borrow expires when the native call returns. `Bytesˉslice` produces another borrowed descriptor only after unsigned offset/length bounds checks. `Bytesˉreadˉu8`, `Bytesˉreadˉu16ˉlittle`, `Bytesˉreadˉu32ˉlittle`, and `Bytesˉreadˉi32ˉlittle` check the complete fixed-width range before reading. A failed slice or read returns packed status 6 and becomes `WVR3008`; no host signal or unchecked pointer access is the language-level failure path.

An enum occupies the low four bytes and retains its signed member value plus compile-time nominal identity. A record occupies the low four bytes as an offset into the current execution's record arena. Each immutable record field consumes one complete 16-byte arena cell. Record construction checks offset addition and arena capacity before copying typed cells; field access proves the complete selected cell is below the committed used boundary. Packed status 7 becomes `WVR3017` on arena exhaustion.

ABI 12 retains ABI 11's return shapes and arenas, ABI 9's nominal values, and ABI 8's borrowed values. Unsigned arithmetic overflow retains packed status 1 / `WVR3007`.

## Execution-context memory layout

All integer fields are little-endian. The context is exactly 88 bytes:

| Offset | Bytes | Field | Version-4 rule |
| ---: | ---: | --- | --- |
| 0 | 4 | format version | `4` |
| 4 | 4 | structure bytes | `88` |
| 8 | 8 | instruction budget | Positive maximum charged with WVB instruction semantics |
| 16 | 8 | call-depth budget | Positive maximum active native call depth |
| 24 | 8 | service-table pointer | Zero when no runtime service is required; otherwise points to the exact table below |
| 32 | 8 | record-arena pointer | Execution-owned base; may be zero only when arena length is zero and the module performs no record construction |
| 40 | 4 | record-arena bytes | At most 1 MiB in the current host executor |
| 44 | 4 | record-arena used bytes | Starts at zero; generated checked construction advances it in 16-byte cells |
| 48 | 8 | text-arena pointer | Execution-owned base; may be zero only when arena length is zero and no text allocation occurs |
| 56 | 4 | text-arena bytes | At most 16 MiB in the current host executor |
| 60 | 4 | text-arena used bytes | Starts at zero; every managed or native text allocator advances the same checked cursor |
| 64 | 4 | service-failure detail | Starts at zero; exact native-service failure detail described below |
| 68 | 4 | reserved | Required zero |
| 72 | 8 | argument-table pointer | Zero when the captured count is zero; otherwise points to the exact immutable descriptor array below |
| 80 | 4 | argument count | Prevalidated snapshot count from 0 through 67 |
| 84 | 4 | argument reserved | Required zero |

The platform executor or verified OS bridge owns this memory for the complete call. Ordinary generated code does not retain the pointer after return. The current adapters construct the exact version and size; the exact generated prologue and every use are independently decoded before publication.

## Runtime-service table

Service-table version 4 is exactly 96 bytes:

| Offset | Bytes | Field | Version-4 rule |
| ---: | ---: | --- | --- |
| 0 | 4 | format version | `4` |
| 4 | 4 | structure bytes | `96` |
| 8 | 8 | `console.write_line` entry | Pointer to the runtime-owned adapter thunk |
| 16 | 8 | `process.argument_count` entry | Pointer to the exact runtime-native context-count leaf |
| 24 | 8 | `process.argument` entry | Pointer to the exact runtime-native checked descriptor-copy leaf |
| 32 | 8 | `file.read_bytes` entry | Pointer to the runtime-owned adapter thunk |
| 40 | 8 | `Textˉutf8ˉisˉvalid` entry | Pointer to the runtime-owned pure native validation leaf |
| 48 | 8 | `diagnostic.write_line` entry | Pointer to the authorized diagnostic adapter thunk |
| 56 | 8 | `Enumˉname` entry | Pointer to the exact runtime-native nominal-name leaf and its adjacent verified metadata |
| 64 | 8 | `Textˉconcat` entry | Pointer to the exact runtime-native concatenation leaf |
| 72 | 8 | `Textˉquote` entry | Pointer to the exact runtime-native deterministic-quote leaf |
| 80 | 8 | `I32ˉformat` entry | Pointer to the exact runtime-native invariant signed-format leaf |
| 88 | 8 | `U32ˉformat` entry | Pointer to the exact runtime-native invariant unsigned-format leaf; `u8` is zero-extended |

The table is deliberately closed. A fragment may require any distinct canonical-order subset of these eleven services; any unknown, duplicate, or noncanonical service list fails verification. Console, process, file, and diagnostic entries are capability services and retain explicit authorization. UTF-8 validation, enum naming, concatenation, quoting, and integer formatting are deterministic runtime support with no ambient authority. All six pure operations and both immutable process-input operations use exact platform-neutral native leaves. Only console output, diagnostic output, and file-byte input use managed adapters during Stage 0. A later extension requires a new accepted contract, bounds/version handling, and cross-host evidence.

Every status-bearing service returns zero in `EAX` on success and nonzero on service failure; `process.argument_count` returns its `u32` value directly. Managed adapters retain their captured `WVR` diagnostic. A native leaf clears context service-failure detail on entry, writes `1` for the 1 MiB text-value limit, `2` for 16 MiB text-arena exhaustion, or `3` for an argument index outside the snapshot, and then returns one. Generated code retains packed status 5; the executor translates the exact details to `WVR3012`, `WVR3018`, or `WVR3020`, and treats an absent or unknown detail as `WVR3013`. Native services do not unwind host exceptions through generated code.

## `console.write_line` service

The baseline accepts `console.write_line(text) -> void` and `diagnostic.write_line(text) -> void` as separate exact hosted capabilities. Their arguments are borrowed-text descriptors backed by strict UTF-8 fragment data or execution-owned immutable buffers. Text parameters, locals, calls, and returns copy complete descriptors.

Generated code calls the service-table entry with this Windvale-owned internal convention:

- `R8` is the address of verified immutable UTF-8 bytes;
- `R9D` is their bounded byte length; and
- `EAX` is zero on success and nonzero on service failure.

The runtime owns one exact platform thunk per required service. For console, Windows maps `R8`/`R9D` to `RCX`/`EDX`; Linux maps them to `RDI`/`ESI`. Every variant preserves `R10`, `R11`, and `R15`, aligns the native stack, calls a bounded managed adapter during Stage 0, restores the original stack, and returns the adapter result. Generated fragment bytes remain identical across hosts.

Before allocating executable memory, the runtime requires explicit authorization for `console.write_line` (`WVR3010`) and an actual output implementation (`WVR3001`). The adapter revalidates that the complete byte range lies inside verified fragment data or an execution-owned buffer, enforces the WVB UTF-8 byte limit, decodes strict UTF-8, writes the text followed by LF, and converts adapter exceptions to packed status 5 / `WVR3013`. No managed exception unwinds through generated machine code.

## Hosted input services

`process.argument_count` has no generated-code arguments and returns the prevalidated `u32` count in `EAX`. Its exact 5-byte native leaf reads context offset 80 and returns. `process.argument` receives the index in `R8D`, receives a verified output-descriptor address in `R9`, and returns status in `EAX`. Its exact 70-byte native leaf clears failure detail, checks unsigned index against the context count before reading the table, and copies one complete descriptor. An out-of-range index writes detail 3 and returns status one without loading the table. Both leaves preserve `R10`, `R11`, and `R15`; neither uses a Windows/System V adapter.

One `Nativeˉexecutionˉbuffers` owner exists for one native run. When either argument service is required, it eagerly encodes the already validated snapshot into one packed immutable allocation and one contiguous descriptor table. Before publishing the context, it independently rereads every table field and byte sequence, checks all ranges and zero reserved fields, and requires exact agreement with the resource snapshot. Zero arguments publish zero pointer and count. The existing limits remain 67 arguments, 4 KiB per argument, and 64 KiB total. All argument allocations expire after native return and no process address enters portable code, WVB, or WVO.

`file.read_bytes` receives resource-name pointer/length in `R8`/`R9D`, receives a verified output-descriptor address in `RCX`, and returns status in `EAX`. Its remaining platform thunk translates those internal registers to the Windows or System V callback convention. File reads use `Hostedˉresourceˉcontext.Readˉfileˉbytes`, so reference and native execution share the same bounded 64-name immutable snapshot cache and hosted adapter error mapping; each returned snapshot is copied once into execution-owned unmanaged storage. Repeated reads reuse the descriptor.

Services write pointer/length/reserved descriptors only into independently verified frame slots. Resource-name and console input ranges must lie wholly inside fragment data or one registered execution allocation. Runtime resource errors retain their exact `WVR302x` code through packed status 5; unexpected adapter failures use `WVR3013`.

## Pure UTF-8 validation service

`Textˉutf8ˉisˉvalid(bytes) -> bool` passes one proven borrowed-byte pointer/length in `R8`/`R9D` and a verified bool output-cell address in `RCX`. [Decision 0070](../Documents/Decisions/0070-First-Runtime-Native-Utf8-Service.md) replaces its managed callback and platform adapters with one exact 800-byte runtime-native x86-64 leaf shared by Windows and Linux. The leaf writes normalized zero or one, returns status zero in `EAX`, and preserves `R10`, `R11`, and `R15`. Invalid encoding writes false; it does not allocate text or gain a capability. Fragment verification plus immutable execution-allocation ownership proves the range before the call; no arbitrary native pointer is accepted.

`Textˉfromˉutf8(bytes) -> text` uses that service as a proof step. False branches to packed status 8 / `WVR3014`; true copies the already-bounded borrowed descriptor as text. It does not allocate or silently replace malformed input.

## Dynamic text services and arena

One native run owns one fixed 16 MiB monotonic text arena. Context version 4 retains its base, capacity, and single allocation cursor at the version-3 offsets. `Enumˉname`, integer formatting, `Textˉconcat`, and `Textˉquote` allocate strict UTF-8 results by reading and advancing that cursor. Each result is independently limited to the WVB 1 MiB text bound (`WVR3012`). Checked aggregate exhaustion becomes `WVR3018`. Arena descriptors are accepted by the same range validator as immutable argument and file buffers and expire when `Main` returns.

Enum naming receives a verified nominal type index and signed enum value and returns the exact declared member name. Its fixed native leaf reads an adjacent canonical runtime-private `WVEN` version-1 block derived from the fragment's verified nominal declarations. The 24-byte header records magic, version, total bytes, nominal type count, enum member count, and the type-directory offset. Each type directory entry is 8 bytes; records have zero members. Each 16-byte member entry records the signed value, absolute metadata-relative strict-UTF-8 name offset and length, and a zero reserved field. Concatenated names follow the member entries. The complete block is limited to 32 MiB, independently parsed against the verified declarations, deterministically reconstructed, and required to match byte for byte before publication.

Exact native `I32ˉformat` and `U32ˉformat` leaves use invariant decimal with no grouping and cover signed minimum, unsigned maximum, and zero directly. Exact native concatenation checks the combined encoded length before reserving and copying bytes. Exact native quoting makes two passes over strict UTF-8: it first validates and measures the complete UTF-16-code-unit escape representation and proves both limits, then writes it. Printable ASCII is preserved; quote, reverse solidus, and the five short control escapes use their short forms; other ASCII controls and non-ASCII BMP values use uppercase `\uXXXX`; supplementary scalars use two uppercase surrogate escapes.

Before W^X publication, the runtime requires exact [Decision 0071](../Documents/Decisions/0071-Native-Text-Arena-And-Core-Text-Services.md) identities for the 249-byte concatenation leaf, 225-byte signed formatter, and 191-byte unsigned formatter. [Decision 0072](../Documents/Decisions/0072-Final-Pure-Runtime-Native-Services.md) additionally requires the exact 323-byte enum-name leaf and its independently verified `WVEN` block plus the exact 1,165-byte quote leaf. All preserve `R10`, `R11`, and `R15`. Their service inputs and output-cell addresses remain compiler-generated and independently verified; context state is runtime-owned for the call.

## Verification and publication

The native fragment verifier independently decodes the exact context prologue, `R15` restoration on every exit, typed 16-byte frame access, hidden descriptor-result cells, scalar/descriptor/void call and return kinds, descriptor construction/copy and provenance, enum operations, record arena allocation/field copies, unsigned bounds branches, fixed-width reads, all service-table loads and argument forms, UTF-8 and runtime failure edges, immutable data targets, relocations, and packed statuses. Corrupt instruction bytes, descriptor fields, hidden results, arena sizes or offsets, argument forms, displacement bytes, service metadata, or immutable data fail before WVO serialization or writable-to-executable publication.

The current `Nativeˉfragment` carries its required-service list beside code, symbols, and patches. WVO 1.0 does not serialize that list. A service-bearing linked image may therefore execute only while paired with its original verified fragment metadata; it is not yet a standalone native application. A future PE, ELF, or Windvale-native container must preserve and verify capability/service requirements before it can publish independently loadable hosted AOT modules.

## Windvale OS use

The version-7 native kernel bridge constructs context version 4, while the portable kernel probe supplies exact budgets `271` and `2`, a zero service-table pointer, zero-length record and text arenas, zero argument table/count, and zero failure/reserved fields. The ordinary portable module loops over immutable i32 data, passes borrowed bytes through an internal function, slices and reads them, and checks `u8`/`u32` results. Firmware probe version 14 identifies the ABI-12 rebuild and emits `native-context=pass` only after that service-free path and the special-kernel path both succeed.

This does not give Windvale OS a runtime service table, record or text allocator, WVB loader, verifier, JIT, or hosted capability implementation. It proves that the ABI-12 compiler still supplies service-free generated code through one explicit versioned context and the borrowed-byte representation in the current AOT OS candidate.
