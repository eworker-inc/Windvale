# Decision 0907: observe immutable borrowed unsafe scratch in WVB 1.35

## Status

Accepted and implemented locally on Windows on 2026-09-01.

This decision adds the first public observation over an opaque
`Foreignˉscratch<Abi>` owner. It does not add mutable scratch borrowing,
address or pointer exposure, write-region construction, authenticated Foreign
calls, Linux evidence, or paired-host qualification.

## Context

[Decision 0904](0904-Execute-Wvb-1.33-Unsafe-Scratch-In-A-Bounded-Scalar-Provider.md)
made bounded scratch construction executable, and
[Decision 0906](0906-Represent-Immutable-Borrowed-Memory-Budget-Calls-In-Wvb-1.34.md)
proved a first non-owning call boundary. The frozen Foundation surface already
defined `Scratchˉlength<Abi>(Scratch: borrow Foreignˉscratch<Abi>) -> u64`, but
the compiler could neither lower that exact call nor let a borrowed scratch
parameter perform any operation. Code therefore had no safe way to inspect the
extent chosen at construction without opening the later pointer boundary.

The one-word scratch record remains opaque in source and bytecode. Its current
provider-private field carries the requested length, not an address. A length
query can therefore be constant-time and non-consuming while preserving the
owner and keeping backing, lease, and provider state hidden.

## Decision

1. Bind only the exact canonical generic call
   `Foundationˉunsafe.Scratchˉlength::<Abi>(Scratch: borrow
   Foreignˉscratch<Abi>) -> u64 effects()`. Require one explicit ABI enum, the
   named immutable-borrow argument, and an exact matching scratch identity.
2. Add typed WVIR operation `187`. It has exact result `u64`, zero stack
   operands, `Target` equal to the scratch parameter or local slot, and
   `Auxiliary` equal to the matching ABI-enum shape. WVIR 1.25 carries the
   operation without generic instances and WVIR 1.26 carries it with them.
3. Reserve WVB minor `1.35` and opcode `DD` (`221`). Its nine-byte encoding is
   `DD`, a little-endian `u32` scratch-local index, and a little-endian `u32`
   ABI-type index. The instruction consumes no operand-stack value, produces
   one `u64`, and does not consume the scratch owner.
4. Reuse shape `28` as the immutable borrowed record view and retain its
   nominal scratch type index. WVB 1.35 alone may use an exact scratch shape-28
   helper parameter. A compiler-generated shape-28 local must be created from
   an available matching owned scratch and remain confined to the adjacent
   borrowed direct-call sequence. It is never an owner, result, field, payload,
   collection element, callable shape, indirect-call argument, or `local.take`
   source.
5. Require every opcode `DD` to name an exact canonical scratch owner or
   immutable view and a declared enum ABI. Every scratch/ABI pair observed by
   `DD` must be covered by the module's exact construction relation. Limit one
   module to 4,096 length instructions and 256 distinct length relations.
6. The scalar provider stores the exact accepted construction length in the
   private scratch record and returns it in constant time. The native x86-64
   backend dereferences private field zero through the validated record handle
   and stores the complete `u64`; it never returns the handle or backing
   address. Affine validation proves the owner is still available.
7. Reject the old minor, unknown or missing opcode, invalid or wrong-shape
   local, invalid or non-enum ABI, unrelated ABI, malformed WVIR version,
   operation, result, operand count, local, ABI shape, and temporary-result
   relation before publication or execution.

## Implementation standing

The focused local Windows matrix passes four valid and eight rejected source
or WVIR programs, sixteen malformed WVIR mutations, fifteen malformed WVB
mutations, twenty compiler-verifier cases, nine scalar-runtime cases, and all
nine native x86-64 reconstruction and execution cases. The length program
constructs 64 bytes, passes an immutable scratch view to a helper, observes
`64u64`, and returns `42` without consuming the owner.

The final current-source native lowerer contains 620 functions in 678,601 WVB
bytes at SHA-256
`c552c6ca542a60de8140c78e4d978be75a70f8baf50cf7ae5661008c9259b823`.
The same WVB reconstructs the 9,754,112-byte Windows hosted tool at SHA-256
`606486f4e800df858a74245596e87d58ebf0e169f9e9288be7d2f4208afd77e6`
and the 9,752,576-byte Linux hosted tool at SHA-256
`377675961465fbfa2b2038ed5cf301ef483907d642355a6b6ebf42d23fa29703`.
The exact WVB-to-WVO reconstruction owner passes all six inventory, metadata,
paired-tool, and retained-fixture cases.
The rebuilt scalar runner contains 235 functions in 505,705 WVB bytes at
SHA-256 `d18b2ce1f802b5bcfdf95c8a6524b5a2ec6dfd6c1e84ae298daf73b362b599c2`.
The final current-source compiler-aligned verifier contains 127 functions in
443,840 WVB bytes at SHA-256
`5a9409437d0a58f1a5fe314ab16ec905b6ffd958938d8981e5f652d83a12110c`.

## Consequences

- A program can validate a scratch extent without receiving an address or a
  second owner.
- The first borrowed record parameter becomes executable only for one exact
  opaque Foundation identity and one exact non-mutating operation.
- Scalar and native execution remain representation-private and constant-time;
  neither path scans or copies the scratch backing to answer the query.
- WVB 1.35 inherits WVB 1.34 budget borrowing and WVB 1.33 construction when
  present, but a valid 1.35 module must contain at least one opcode `DD`.
- Mutable scratch/write-region borrowing, pointer derivation, authenticated
  Foreign calls, a migrated real boundary, Linux reproduction, and paired-host
  qualification remain explicit Slice 8 work.

## Reconsideration triggers

Add another scratch observation only with an exact result, complexity, owner,
and revocation contract. Add mutable borrowing only with a write-through alias
model and verifier/runtime/native evidence. Do not generalize shape `28` into
an arbitrary borrowed-record parameter or expose the private carrier as a
public ABI value.
