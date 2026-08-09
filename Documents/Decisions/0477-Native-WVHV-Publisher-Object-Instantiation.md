# Decision 0477: Native WVHV publisher object instantiation

- Status: Implemented current-host candidate; final PE/ELF materialization pending
- Date: 2026-08-09
- Advances: [Decision 0476](0476-Native-WVHV-Publisher-Construction-Requests.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [native hosted-verifier publisher object instantiation](../../Specifications/Windvale-Native-Hosted-Verifier-Publisher-Object-Instantiation.md)

## Context

Decision 0476 transferred exact object identities, layout, private-function
offsets, and ordered external targets from the frozen C# publisher builder to
Windvale. C# still instantiated the startup, publication adapter, and shared
SHA-256 object before writing the final application.

The publisher objects differ from the earlier hosted-startup case: the
adapter has 111 Windows or 49 Linux relocations, targets are assigned once per
external symbol rather than repeated per relocation, and the SHA object has
aligned code plus read-only-data sections with two internal relocations.

## Decision

- Add service-free `WVIX 1` and `WVIO 1` contracts for publisher object
  instantiation after the `WVPI`/`WVPS`/`WVCR`/`WVPT` admission stages.
- Resolve adapter imports in exact admitted WVO symbol order, while resolving
  adapter locals and both SHA relocations from the object layout itself.
- Apply only admitted relative-i32 relocations with addend `-4`, checked signed
  range, and zero placeholders.
- Preserve the two-byte alignment gap between SHA code and read-only data.
- Keep this constructor focused on object bytes. PE/ELF section, segment,
  import-page, note, metadata, and outer-image mutation remain the next seam.

## Evidence and consequences

The digest-bound native front door builds a service-free 16,091-byte WVB with
SHA-256
`dd589e731e2253fc020ca70ea1ba379da287948f90323d8df8447689bc9d6a78`.
The reviewed focused test passes 1/1 in 2.637 seconds. For both targets it
proves interpreter/native equality and exact equality with all six component
slices already present in the canonical publisher applications.

The Windows response contains 5 startup, 5,286 adapter, and 1,685 SHA bytes;
the Linux response contains 5, 3,363, and 1,685. Their raw payload SHA-256
values are respectively
`41591b9e04457c46aa449fb1a2ab8415a29e9146bdb775f46dcf6f9c38a94a16`
and
`c19e7f510d8a05554a94e55e53edc32118f2fc6bcd38e6ef42fb96727feb225a`.
Malformed headers, placements, external targets, and object/relocation shapes
reject; exact byte identity remains owned by the preceding `WVPI` stage.

This removes object instantiation from the remaining C# semantic gap. It does
not yet remove the frozen C# PE/ELF writer or claim independent Linux
qualification. No broad Seed, OS, Standard, Qualification, WebAssembly, QEMU,
or Linux process gate ran.

## Reconsideration triggers

Version the request or response if an object identity, section geometry,
symbol order, relocation kind/addend, target placement, or output identity
changes. A future general WVO instantiator may replace this exact constructor
only after it preserves the same admission and deterministic-byte evidence.
