# Decision 0723: Compose segmented hosted storage

## Status

Accepted on 2026-08-16.

## Context

The completed transaction writer produces more than 4 MiB of native code. The
segmented compiler path could lower, stage, verify, transport, and package that
application, but it owned only the application's exported `Main`. The native
random-access-storage provider imports `Main` and exports
`Storage_host_entry`, so packaging the application alone entered generated code
without a populated provider table and faulted before useful storage work.

Joining the large application WVO with the provider through the ordinary Wv
linker was not possible: its strict whole-value input remains capped at 4 MiB.
A general streamed multi-object linker would be substantially larger than this
database milestone requires.

The investigation also found one segmented-linker rejection error. A valid
compiler WVO may retain its second `.rodata` section header while producing
zero read-only bytes and therefore zero read-only chunks. The segmented linker
incorrectly required a read-only chunk whenever the header was present.

## Decision

Windvale adopts the narrow hosted overlay contract in
`Specifications/Windvale-Segmented-Hosted-Overlay.md`.

The canonical large application image remains unchanged. The ordinary Wv
linker lays out and validates the common storage provider, one platform
provider, and one five-byte exported `Main` trampoline at the next 16-byte
boundary. The composer verifies that exact trampoline and patches only its
signed relative displacement back to the application's verified `Main`. The
provider must fit in the remaining part of the final 4 MiB fragment and the
complete image must remain within 32 MiB.

The segmented compiler linker now requires read-only chunks only when the
admitted envelope declares nonzero read-only bytes. Nonempty read-only data
still requires complete chunk coverage.

Because that correction changes the native compiler-image staging product, the
candidate toolset and its digest-bound launchers advance to these exact
identities:

- compiler-image staging WVB: 75,666 bytes, SHA-256
  `1a1614c4010baf47f5f1766de5f71806356ec14fa8f5bc67a62b5b2342269edd`;
- Windows application: 854,016 bytes, SHA-256
  `e467d211d141ab75b838ece9b3c4625b6b5b2768b63dcacadd040368844e18db`;
- Linux application: 856,064 bytes, SHA-256
  `7ef825a8054cb8f63c10c957b234f9c371fe1507d7ee20f3e6dbabf73e550cb2`.

## Evidence

The corrected Windows staging application directly accepted the previously
rejected empty-read-only diagnostic object. It reported a 4,211,776-byte image,
entry offset 5,357, six output chunks, and a 100-byte manifest.

The first database overlay contained:

- 4,195,836 unchanged application bytes with `Main` at 4,477;
- 4 alignment bytes and a 4,208-byte provider image;
- provider entry 4,196,656 and trampoline 4,199,952; and
- 4,200,048 final image bytes in two canonical fragments.

The Windows `host-tree-writer` development owner passed in 481,610 ms. Its
135,620 ms writer phase performed a real commit, reopen and idempotence checks,
five interruption/recovery cases, and logical put/get through the composed
provider. Tool construction took 282,040 ms because this run recreated the
cache; host-storage and tree-reader prerequisites took 35,780 ms and 27,860 ms.
A cache-hit reporting rerun passed in 128,120 ms, including 63,350 ms for the
segmented writer and logical put/get phase; its project checkpoints explicitly
reported `Segmented`, `Segmented`, and `Hit`, and all three hosted application
checkpoints reported `Hit`.

This is current-Windows-host evidence. The Linux candidate was constructed,
but Linux execution and independent dual-host reconstruction remain pending.
The native segmented-toolset reconstruction owner then passed all four checks:
WVO staging producer, compiler-image staging, compiler-image transport, and a
compiler-scale WVB staging flow.

The complete database development owner passed 47 cases in 1,475,810 ms. This
included every portable tree, transaction, query, SQL, JSON, catalog, and
bootstrap fixture plus real hosted storage, create/fill/split, local put/get,
reopen, delete, scan, recovery, and the segmented writer path. A focused
root-writer reporting rerun passed in 114,490 ms and reported cache hits for
all create, fill, split, and get projects and applications.

Nine other shared owners selected because the digest-bound staging launcher
changed also pass: WVB-to-WVO, Wv linker, WVO inspector, console verifier, WVO
publisher, linker rejections, 200 hostile linker inputs, linker map limit, and
hosted-container packager reconstruction.

The complete changed-file wrapper cannot make a clean all-owner claim at this
revision. It stops in the older compiler-reconstruction owner before invoking
the changed segmented linker: the current compiler source closure emits a
947,975-byte WVB while that owner still pins 929,711 bytes. None of this
milestone's changed paths is in that 13-file source closure. That existing
compiler-product drift requires a separate candidate refresh rather than an
unrelated pin change in this database milestone.

## Consequences

Large generated database applications can use the real hosted storage provider
without teaching the ordinary linker to load values above its qualified limit.
Provider imports and relocations retain the mature linker checks, while one
small explicit patch joins the two already verified images.

The composer streams application fragments and bounds the provider image, so it
does not add a full-image memory copy to an already expensive development
path. The direct segmented build remains slow and is intentionally used only
for applications that exceed the ordinary lowerer's output limit.

The overlay is storage-specific and private to build/verification. It is not a
general dynamic-linking design, a durable image publisher, or a capability
grant. A later persistent database server may reuse the resulting hosted
provider contract, while a general streamed multi-object linker remains a
separate milestone justified only by broader demand.
