# Windvale native compiler publication

## Status

Candidate retained-plan compiler-host integration under the existing native
compiler bootstrap direction. Small Windows and real Debian execution have
focused development evidence; compiler-scale self-lowering remains open.

## Contract

The candidate compiler host also accepts `--staged <input.wvb> <output-prefix>`.
The ordinary two-argument byte-result entry retains its 4 MiB response limit,
including the 32-byte header. The staged entry uses the existing WVO 1.0 and
WVOP 1 publication contracts: at most 64 MiB per complete object, 518 chunks,
and 4 MiB per chunk. This is private compiler-host integration under the
[temporary bootstrap direction](../Documents/Decisions/0981-Bootstrap-The-Current-Native-Compiler-With-A-Temporary-Serializer-Projection.md),
not a new source-language, WVB or runtime value encoding.

[`Native-X64-Lowering-Publication-Session.wv`](../Compiler/Windvale/Native-X64-Lowering-Publication-Session.wv)
normalizes admitted metadata once, builds one plan with the borrowed application
budget, and retains the source, plan and publication regions in an owned Session.
Open rejects an invalid complete-object maximum and the measured code/helper
lower bound before emitting batches. It checks the complete region size before
any output-file call. Begin and Next borrow that retained Session; Next takes a
value Cursor and returns an owned Step. Ordinary source cleanup releases each
Step and the final Session. The single-result adapter uses this same session
and one reserved byte builder within its unchanged response ceiling.

Publication validates helper bytes against the existing SHA-256 template, the
owned-frame cleanup template for ABI 24, or the shared-storage template for
ABI 25. Automatic budget, vector and allocation-result cleanup therefore uses
the same exact owned-frame helper in ordinary and staged output. An ABI-specific
helper under another ABI, or an inexact template, is refused before that piece
is published.

[`Build-Shared-Compiler-Host.mjs`](../Tools/Native/Build-Shared-Compiler-Host.mjs)
first obtains independent segmented WVO admission. Its bounded binding reader
checks WVB 1.45 root-function signatures, nominal identities, field kinds,
nested record backing and enum values, then binds the five source roots to
their exact admitted local-function symbols. It emits a data-only private
`WVPC` table; the ordinary Windvale assembler/linker produce the caller and
apply relocations. Host JavaScript supplies no compiler machine instructions.
The table contains sixteen little-endian u32 words:

| Offset | Field |
| --- | --- |
| 0, 4, 8, 12 | `WVPC` magic, version 1, size 64, admitted module image bytes |
| 16, 20, 24, 28, 32 | Open, Begin, Next, Session release and Step release function offsets |
| 36, 40, 44, 48, 52 | Session status, Plan ABI, function count, machine-code bytes and helper bytes field offsets |
| 56, 60 | Regions valid and complete-object-byte field offsets |

[`X64-Shared-Compiler-Publication.wva`](../Linker/Startup/X64-Shared-Compiler-Publication.wva)
checks that table and calls the admitted roots in the existing fresh ABI 25 /
context 11 domain. Its current Session, Cursor and Step backing occupies 51,
4 and 8 sixteen-byte cells. The Cursor is outgoing value storage and may be
consumed by Next; checks after that call use saved scalar expectations and the
returned Step. Fuel and call-depth counters survive every source call.

The caller prepays 64 KiB of metadata, a 4 MiB copy buffer and 16 MiB of physical
storage in the outer arena. The physical region starts at byte 4,259,840 and
ends at 21,037,056. Independent-reader temporary writes must start at or above
that end. Runtime/application budgets each retain their 16 MiB maximum; the
40 MiB root also funds the two immutable mappings. These are this tool profile's
bounds, not portable source-language memory limits.

Each Step must have the expected kind and object position, an admitted shared
byte extent, equal length/capacity, a consistent next cursor and a bounded
function index. At most function-count plus eight steps are accepted. Only
code pieces are coalesced, up to 1,310,720 bytes when a buffer is already live;
individual pieces retain the 4 MiB limit. Prefix, padding, read-only header/data,
symbols and relocations retain separate chunk resources required by the
independent staged reader. The prefix is at most 4,078 UTF-8 bytes.
The successful code-byte report counts machine code, helper bytes and only the
padding actually emitted. ABI 24 output without read-only data is not padded;
its staged and ordinary reports retain the same exact code length.

Use a fresh private output prefix. Each file write is attempted once. A generated
output path equal to the exact input argument is rejected before writing;
this does not establish normalized-path or alias equivalence. Failure may leave
completed chunks, but never publishes a successful manifest. Success releases
the Session, checks zero physical charge and full runtime/application credit,
releases the application budget, closes and checks the domain, then writes the
final WVOP manifest. Typed source refusal makes no output-file call. Host I/O,
trap, progression and cleanup failures remain distinct nonzero host outcomes.

The existing shared-source owner adds ordinary/staged byte identity, independent
staged admission, typed malformed-input refusal, failed-write cleanup and exact
input-path collision cases. Small Windows and real Debian execution establish
those focused contracts. Compiler-scale self-lowering, broader memory integration,
qualification and installed-toolchain promotion remain separate open gates.
