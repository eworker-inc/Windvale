# Windvale native WVB publisher

## Status and scope

This contract constructs the general WVB publisher as exact Windows and Linux
native applications without a target-specific managed writer. The publisher
admits one immutable candidate snapshot with the Windvale semantic WVB
verifier, then reuses the qualified publication adapter for file-identity
checks, sibling creation, reread verification, atomic replacement, and
durability reporting.

The candidate applications are not yet promoted over the retained
`Artifacts/Native-Front-Door` recovery publisher. The grouped retirement gate
and native compiler/build-driver self-convergence remain before that cutover.
Decision 0706 supplies local Debian WSL2 execution for the refreshed candidate;
it is not release qualification or front-door promotion.

## Candidate construction geometry

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| WVB publisher WVB | 181,772 | `c90f5325ea409d0710254812e1d434cce712de68385dec74d23eef5a475cf3c4` |
| Native-lowered WVO | 1,523,708 | `c1ce50f68e12dc94e56fa848c6f09f707ad117294af5e19f15659b7901c0bf35` |
| Linked fragment | 1,520,746 | `98aba65ccfdb0455f9fcb78ad3ffa0ecbe7aa942fcbf9064d179018dec12178a` |

The WVO has two sections, 78 symbols, 15 relocations, 1,520,336 code
bytes, and 410 read-only-data bytes. `Main` is at offset 0. The private
transaction-begin bridge is `$function_0002` at offset 5,475 with 389 bytes;
the transaction-apply bridge is `$function_0001` at offset 4,686 with 789
bytes. These roles are discovered from the canonical WVB declarations and
admitted explicitly; symbol numbering alone is not semantic authority.

This candidate routes its immutable input through the shared
independent-metadata verifier adapter before semantic verification. It retains
absent-form acceptance and rejects invalid replacement metadata before the
publication transaction begins. Construction variant 2, its structure and
identity records, target tables, object and import instantiation, PE/ELF
materializers, and final application digests all bind this exact geometry.

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
| Windows x64 | 1,537,024 / `6385eac0d7c326f9dbded708a064eecb113fcf41c036b59b519938ee1a5b5e8c` | 1,544,192 / `0fdb432aa54cc7b9cc4a1d42a438d2b56a29695e06b2369540dac845989751c1` |
| Linux x64 | 1,536,000 / `1e3049360820c321df5489e2df6f2cbb748565f20e95e130c1ff08edbe7622c4` | 1,541,109 / `7bf4593566401853ab7f551ca5d45125ac0ea3a6c4e34315703785ed7d6cdfb6` |

The paired final applications live in
`Artifacts/Native-Wvb-Publisher-Candidate`. Their WVB and WVO live in the
shared publisher-construction candidate so all three exact overlay roles use
one inventory and one process toolset.

## Evidence and remaining boundary

The focused native owner rebuilds, lowers, and links the metadata-aware source,
verifies the refreshed construction inventory, reconstructs both exact target
applications, and executes the current-host candidate on canonical portable
and independent-metadata WVB inputs. Windows and Debian WSL2 locally agree on
the source WVB, WVO, fragment, construction records, and paired final
applications. The immutable recovery release retains historical differential
evidence; it is not part of the normal publisher build or focused verification
path.

The retained front-door publisher still rejects replacement metadata. The
candidate now has refreshed exact construction geometry and paired application
identities, but front-door cutover and a package such as Echo migrating its
source header remain separate follow-up changes.
