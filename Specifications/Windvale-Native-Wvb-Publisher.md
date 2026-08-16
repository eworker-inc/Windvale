# Windvale native WVB publisher

## Status and scope

This contract constructs the general WVB publisher as exact Windows and Linux
native applications without a target-specific managed writer. The publisher
admits one immutable candidate snapshot with the Windvale semantic WVB
verifier, then reuses the qualified publication adapter for file-identity
checks, sibling creation, reread verification, atomic replacement, and
durability reporting.

The candidate applications are not yet promoted over the retained
`Artifacts/Native-Front-Door` recovery publisher. Independent Linux execution,
the grouped retirement gate, and native compiler/build-driver self-convergence
remain before that cutover.

## Retained construction geometry

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| WVB publisher WVB | 163,300 | `9ebfe92eef070dfdcf18c4d176b5f32f64ad3f80751340b8a59ab2f1d567ec2a` |
| Native-lowered WVO | 1,349,361 | `43a594776b4e280575ac14e2866b4708961dd1290d643b41779a4933a8ba5991` |
| Linked fragment | 1,347,597 | `3d419d28b606408e7b2430cceacf4c0b7b109bcd511df4e98ca0d41b871f1c2d` |

The WVO has two sections, 47 symbols, six relocations, 1,347,360 code
bytes, and 237 read-only-data bytes. `Main` is at offset 0. The private
transaction-begin bridge is `$function_0002` at offset 5,475 with 389 bytes;
the transaction-apply bridge is `$function_0001` at offset 4,686 with 789
bytes. These roles are discovered from the canonical WVB declarations and
admitted explicitly; symbol numbering alone is not semantic authority.

The current source candidate instead routes its immutable input through the
shared independent-metadata verifier adapter before semantic verification. It
retains absent-form acceptance and rejects invalid replacement metadata before
the publication transaction begins. The current candidate identities are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Metadata-aware WVB publisher WVB | 181,772 | `c90f5325ea409d0710254812e1d434cce712de68385dec74d23eef5a475cf3c4` |
| Native-lowered WVO | 1,523,708 | `c1ce50f68e12dc94e56fa848c6f09f707ad117294af5e19f15659b7901c0bf35` |
| Linked fragment | 1,520,746 | `98aba65ccfdb0455f9fcb78ad3ffa0ecbe7aa942fcbf9064d179018dec12178a` |

The current candidate is not yet an input to construction variant 2. The
retained records below continue to admit only the preceding exact geometry
until the construction, admission, promoter, candidate, and front-door family
is refreshed coherently.

## Native construction

The shared publisher-overlay record family uses exact role 2. `WVPM 1`
selects role 2, the constructor emits distinct `WVPB 1` metadata, and `WVPS`
and `WVCR` retain the explicit role. The same startup, publication adapter,
SHA-256 object, six-service base constructor, target instantiator, and PE/ELF
materializers are reused with role-specific exact geometry.

```text
Tools\Native\Construct-Wvb-Publisher.cmd <windows|linux> <output.exe|output.elf>
./Tools/Native/Construct-Wvb-Publisher.sh <windows|linux> <output.exe|output.elf>
```

| Target | Base bytes / SHA-256 | Final bytes / SHA-256 |
| --- | --- | --- |
| Windows x64 | 1,363,968 / `243b763d8b49b34108585c56f46c90190eac085a80c59873c8a2cb3e88d16102` | 1,371,136 / `b9fd1b11bc1e4a726e4a43b16830a9351fe573b30e547ba8d8f6660f688ed421` |
| Linux x64 | 1,363,968 / `2fc0332887c96ad0fa34d1987091d60ddbbe61f019739d41734cd491b8ca4b64` | 1,369,077 / `b8efb90f7d7c4eae99de01df6c0a3c24a7396d9b9e717ff69d005282ed3d63af` |

The paired final applications live in
`Artifacts/Native-Wvb-Publisher-Candidate`. Their WVB and WVO live in the
shared publisher-construction candidate so all three exact overlay roles use
one inventory and one process toolset.

## Evidence and remaining boundary

The focused native owner rebuilds, lowers, and links the current metadata-aware
source candidate with exact identities on both hosts. Separately, it verifies
the retained construction inventory, reconstructs both retained applications,
and executes the current-host retained candidate on a canonical portable WVB.
This separation keeps source progress visible without misrepresenting the old
variant-2 artifact as metadata-aware. The immutable recovery release retains
historical differential evidence; it is not part of the normal publisher build
or focused verification path.

The retained front-door publisher still rejects replacement metadata. Promotion
requires refreshed exact construction geometry and paired application identities
before a package such as Echo can migrate its source header through the ordinary
build and publication path.
