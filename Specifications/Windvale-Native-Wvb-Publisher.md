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
| WVB publisher WVB | 159,770 | `8247539e0f4a5436b3902ec1fef33c6c39c231703de7bf505a6c65d66a764f96` |
| Native-lowered WVO | 1,319,377 | `edc49bbae0bfd16a38db4a08d9a6e636edfac35828e1c6b050c45d85d5e1f9e3` |
| Linked fragment | 1,317,613 | `9003479563a043bb69113be43100289f653f6772356c48a17098c1c6700f5271` |

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
| Windows x64 | 1,333,760 / `a06095df9ab46b3816c376c2bedc6b07c8e6aff0eaf6c92ff2c2a47d9b210466` | 1,340,928 / `9ee91e3044193e2e90461ecf4e7ddefa4b5583f55b041b31911044c6d65b92c7` |
| Linux x64 | 1,335,296 / `57cac655719571d20922bf6b3db33ec77781201ccd4dbd45fc41e14c651eb6ab` | 1,340,357 / `2ade91f624609c93a3b80a0802679bef79832c0a63db7996c889794d365f1188` |

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
