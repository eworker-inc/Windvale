# Decision 0789: Publish multiple concrete generic specializations

- Status: Accepted
- Date: 2026-08-20

## Context

Decision 0786 connected direct generic functions to concrete bindings, WIR,
and WVB, but limited each declaration to one distinct specialization. That
made an inferred and explicit `Identity<i32>` reusable, while a second
`Identity<u32>` failed even though WVGC already admitted a bounded ordered
catalog of distinct instances.

The existing WVLB/WVIR 1.1 directory position was also the WVSD declaration
position. Reusing it for a second concrete body would either overwrite the
first body or make calls depend on hidden mutable state. Counting only source
function declarations was not sufficient because WVSD also contains modules,
capabilities, data, records, enums, variants, fields, members, and cases.

## Decision

1. Remove the one-instance-per-declaration rejection. Retain the shared bound
   of at most 256 WVGC instances per compilation, the 32-pass specialization
   fixed point, the 1 MiB catalog bound, and the 16 MiB estimated-code bound.
2. Keep non-specialized analysis byte-for-byte on WVLB/WVIR 1.1. When the
   catalog is non-empty, publish WVLB 1.2 and WVIR 1.2 as an inseparable
   validated pair.
3. Retain every WVSD position in both directories. A generic declaration's
   ordinary position is an all-zero placeholder. Append one concrete range and
   one concrete function body per WVGC instance in catalog order. The stable
   specialized identity is `WvsdEntryCount + Instance`.
4. Give each WVLB 1.2 range its source declaration and WVGC instance. Embed the
   exact WVGC 1.0 evidence once between the range section and binding entries.
   Individual entries retain only concrete shapes.
5. Give WVIR 1.2 an explicit specialization count and layout version. Keep all
   function, block, operation, temporary, and operand entry layouts unchanged.
6. Validate the embedded catalog as the canonical solved substitution, then
   validate each concrete binding and WIR signature against its source
   declaration. Map specialized call targets back through WVLB before checking
   declaration kind, arity, parameter shapes, and result shape.
7. Emit only reachable concrete functions. Omit generic placeholders, append
   concrete instances deterministically with private synthetic names, translate
   calls through the ordinary rank table, and do not export a specialization
   merely because its source declaration is visible.
8. Keep WVGS/WVGC and specialization identity out of WVB and the runtime. This
   remains monomorphization through the shared WIR/WVB backend, not a runtime
   generic service or a second compiler.

## Evidence

`Generic-Multiple-Specializations.wv` deliberately declares a record before
the generic function, then infers `Identity<i32>` and `Identity<u32>` and
explicitly reuses `Identity<i32>`. This catches the incorrect function-count
base as well as distinct-instance publication and reuse.

The current Windows split compiler produces identical output on two builds:

| Evidence | Exact value |
| --- | ---: |
| WVB bytes | 498 |
| WVB SHA-256 | `d2054fc0a60dca7d48aa2427efb608b10d2198425960bc54381babc5824b7d01` |
| Reachable WVB functions | 3 |
| Native scalar result | 42 |

The compiler-aligned strict verifier accepts the product. The three functions
are `Main` plus two concrete specializations; the source generic placeholder is
not shipped. The second build is served from the target-aware analysis and
emission cache with the same bytes.

The final local analyzer WVB is 1,070,851 bytes with SHA-256
`7720b36a5c1f336ab26db4bc9a8e7eb1d3f0f686945f4d5f6627a5ad80d6f26c`.
Its segmented Windows executable is 33,527,296 bytes, 27,136 bytes below the
unchanged 33,554,432-byte limit. No compiler, artifact, catalog, or native-image
limit was raised.

The current analyzer plus the retained target-aware bootstrap emitter produces
the current optimized emitter as 419 functions, 692,991 code bytes, and an
838,414-byte WVB with SHA-256
`5d5ad052d052b5c1e507c920d42e516388ea59d1bc4488ca544f291cfbb00159`.
The historical bootstrap analyzer reaches its bounded instruction budget on
this enlarged source closure. The Language 1.0 front door therefore reuses the
current analyzer it already reconstructed and no longer packages or executes
the obsolete analyzer for this step. This removes one redundant large native
package from each focused run.

The focused Language 1.0 owner now builds the specialization fixture twice,
compares its diagnostics and WVB bytes, performs strict compiler-aligned
verification, and executes it on both maintained host scripts. Storage, OS,
paired-host equality, and complete Qualification remain deferred to the final
seven-slice integration gate.

## Consequences

One generic function can now serve multiple concrete argument identities in a
single source closure while equal inferred and explicit calls still reuse one
body. Cached analysis is self-describing enough for an independent emitter to
reject a corrupted catalog, mapping, signature, or call target before WVB
publication.

Specialization increases analysis artifacts and generated code only for
distinct reachable instances and remains bounded. Unused generic templates,
generic records and variants, nested generic type expressions, and constant
generics outside the accepted collection maximum remain unsupported.

## Reconsideration triggers

Replace linear catalog lookup only if representative workloads show it
dominating compilation under the 256-instance bound. Add a separate template
artifact only if uninstantiated public generic declarations must cross package
boundaries. Change private WVB naming only with an equally deterministic,
collision-free mapping and cross-host evidence.
