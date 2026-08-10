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

## Exact source and native geometry

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| WVB publisher WVB | 159,328 | `5da26ddb18cdb6511cb6c28b9603e79c7d318696a5371ca4410db47be7bcb219` |
| Native-lowered WVO | 1,292,411 | `90b309f903219edb4db02cb3c7a909e173505f4c459376e473bf9f8c1cbd9493` |
| Linked fragment | 1,290,749 | `8426d7a2c22ec6aeec642b55c0144c6f5532929a8c29200fe38298326511b5e5` |

The WVO has two sections, 44 symbols, six relocations, 1,290,512 code
bytes, and 237 read-only-data bytes. `Main` is at offset 0. The private
transaction-begin bridge is `$function_0002` at offset 5,475 with 389 bytes;
the transaction-apply bridge is `$function_0001` at offset 4,686 with 789
bytes. These roles are discovered from the canonical WVB declarations and
admitted explicitly; symbol numbering alone is not semantic authority.

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
| Windows x64 | 1,307,136 / `146149052209fcb9ef054c80c05dd315e197290f48142c057161b0e9c154e9d6` | 1,313,792 / `e95676eabf80e5230d39241a9967b47bf61b4c96bddca0280ff0abb772bae1d1` |
| Linux x64 | 1,306,624 / `c2f710921da8b2f39a8f927b0054a59f00957b9cfc449a687dd600eb9e508427` | 1,311,685 / `3bb76b7ab4f5f5a00d9f949e70a65d49aac7b0973856e6a6148f2a9a5ca38c72` |

The paired final applications live in
`Artifacts/Native-Wvb-Publisher-Candidate`. Their WVB and WVO live in the
shared publisher-construction candidate so all three exact overlay roles use
one inventory and one process toolset.

## Evidence and remaining boundary

The focused native owner rebuilds and lowers the WVB publisher, requires exact
WVB/WVO/fragment equality, reconstructs both applications, and executes the
current-host candidate on a canonical portable WVB. The focused managed test
remains independent Stage 0 differential evidence and matches both application
identities.

The current compiler and compiler-build-driver WVB candidates still encounter
the same semantic-verifier rejection through both the retained front door and
this candidate. That existing verifier/self-convergence boundary is not
bypassed by publication and remains the next compiler bootstrap seam.
