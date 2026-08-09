# Decision 0476: Native WVHV publisher construction requests

- Status: Implemented current-host candidate; final materialization pending
- Date: 2026-08-09
- Advances: [Decision 0475](0475-Native-WVVP-Metadata-Ownership.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [native hosted-verifier publisher construction requests](../../Specifications/Windvale-Native-Hosted-Verifier-Publisher-Construction-Requests.md)

## Context

Decision 0475 moved `WVVP` construction into Windvale, but the frozen C#
publisher builder still discovered `Main` and two private transaction
functions, loaded five embedded WVO assets, calculated target placements, and
resolved every external relocation target. Rewriting PE/ELF bytes before
making those inputs explicit would preserve hidden C# semantics.

The first combined Windvale request project also exposed a real compiler
limit: SHA-256 admission, generic WVO verification, layout, and target arrays
together exceed the 4,096-binding source-WIR ceiling.

## Decision

- Admit exact inputs in `WVPI 1` using portable streaming SHA-256.
- Discover and record exact WVO structure and `Main`/begin/apply offsets in
  the separate `WVPS 1` stage.
- Emit exact target geometry plus all seven input/output digests in `WVCR 1`.
- Emit startup and adapter external addresses in exact WVO symbol order in
  `WVPT 1`; do not duplicate every relocation occurrence.
- Keep portable byte bridges separate from hosted file tools. Each pair shares
  one core and has an independently pinned WVB identity.
- Preserve focused source ownership rather than combining these invariants in
  one very large file or splitting them into arbitrary numbered fragments.

## Evidence and consequences

The native source front door builds the four service-free bridges at 39,914,
18,050, 14,045, and 7,960 bytes, and the four hosted tools at 42,707, 19,420,
15,554, and 9,390 bytes. The native lowerer produces the exact 233,804-byte
publisher WVO with SHA-256
`ef0f5e49a07450e3d957e5576f819201849b705097bfbf75432c76d2c438ec23`.

The reviewed focused test passes 1/1 in 10.413 seconds after the incremental
build. It covers both targets, all pinned WVB identities, native large-envelope
identity admission, interpreter/native equality for the bounded downstream
records, exact private-function offsets, layouts, raw digests, every ordered
external target, and malformed rejection. The large identity stage is not run
through the reference interpreter because its SHA work exceeds that
interpreter's one-million-instruction test ceiling; independent .NET hashing
checks all six inputs and all seven emitted digests.

This removes C# resource loading, private-symbol discovery, and target-layout
semantics from the remaining design gap. It does not claim that native object
instantiation or final PE/ELF materialization is complete. The C# builder is
still frozen Stage 0 recovery/differential evidence. No broad Seed, OS,
Standard, Qualification, WebAssembly, QEMU, or Linux process gate ran.

## Reconsideration triggers

Version these records when any pinned module/object identity, symbol order,
target address, PE import profile, ELF segment topology, or final application
identity changes. Reconsider the staged split only after a measured compiler
ceiling change; do not merge it merely to reduce the number of files.
