# Decision 0018: Canonical Windvale map and publication

- Date: 2026-07-30
- Status: Accepted, implemented, and cross-host qualified at `40ac57d`

## Context

The qualified Windvale linker already produced and independently reconstructed the complete flat image, but publication remained disabled. Windvale Linking 1 requires the path-free canonical map to succeed before the host sees an image write. The map also needs every validated input digest, placement, definition, import provider, relocation value, and image digest in exact Stage 0 order.

Repeatedly running the complete hostile-input validator while formatting thousands of already accepted symbol records made the 1 MiB map-limit case impractical. The hosted boundary already guarantees one immutable first-read snapshot for each input, so later passes can derive read-only offsets from the exact value accepted by the complete WVO scanner without weakening initial validation.

## Decision

- Keep `Inspectˉobject` as the only structural acceptance boundary for each link input. No resolution, image, verifier, or map pass runs unless every snapshot passed that complete scan.
- Derive `Acceptedˉobjectˉview` records only from those same immutable snapshots. These views read already validated counts, binding ranges, and record offsets; they do not accept a new value or replace the WVO validator.
- Construct canonical map version 1 in Windvale source as bounded ASCII/LF bytes, in the specified input/layout/source order, with invariant integer formatting and exact SHA-256 identities.
- Reject a definition set whose provable minimum record size already exceeds 1 MiB, then enforce the exact 1 MiB limit again before every line append. Either path returns `WVL1012` with no map bytes.
- Run map construction only after independent complete-image reconstruction succeeds.
- Invoke `file.write_bytes` exactly once only after the image and complete map are both accepted. Emit the already constructed map only after the host write succeeds.
- Keep deterministic link failures before publication: they invoke no writer, create no image, and preserve an existing image. Native write failures remain explicit hosted-resource failures.

## Consequences

- The Windvale-written linker now produces the same 24-byte canonical image and 1,721-byte map as Stage 0 without a C# link callback.
- The accepted-view optimization removes redundant validation loops while preserving one complete validation of every exact input snapshot.
- The maximum 4 MiB valid image and a 16,384-definition map-limit rejection both remain within explicit 200,000,000-instruction conformance ceilings.
- The first target remains `flat-x86-64-v1`; this decision does not add PE, ELF, UEFI, ABI, loader, or operating-system policy.

## Reconsider when

- A reusable Foundation object-view module can replace the internal accepted-view code without changing the initial validation boundary or serialized output.
- A new target needs different evidence. Add a target-owned map/publication adapter rather than inserting host conditionals into this flat-image implementation.
