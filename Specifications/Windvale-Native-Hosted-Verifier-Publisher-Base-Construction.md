# Windvale native hosted-verifier publisher base construction

## Status and scope

This contract closes the final managed input in hosted-verifier publisher image
construction. Two focused hosted Windvale tools derive the exact verifier
metadata and runtime header from one canonical six-service `WVSQ 2` request.
The ordinary construction command then connects those values to the existing
native bundle, startup, platform, container, publisher-record, object, import,
and materialization processes.

The command is an exact candidate constructor, not yet a durable publisher. It
refuses an existing destination, constructs privately, admits the final length
and digest, and copies a new result only after all stages succeed. Promotion
still requires a completed-publisher admission and durable replacement boundary.

## Focused commands

```text
wvhostverifierpublisherbasemetadata <target:1|2> <native-entry> <request.wvsq> <metadata.wvhv>
wvhostverifierpublisherbaseruntime <metadata.wvhv> <runtime.wvhr>
```

The metadata tool admits one exact `WVSQ 2` envelope with its embedded `WVPQ 1`
plan, hashes the linked fragment and six ordered service leaves, constructs the
private `WVVE` evidence, and invokes the existing `WVVE -> WVVR -> WVHV` cores.
It writes only the admitted raw 1,024-byte `WVHV` payload. The runtime tool
requires that exact payload length before any fixed read, forms the existing
1,048-byte runtime request, invokes the shared constructor, and writes only the
raw 4,096-byte `WVHR` result.

Both commands reject an exact path-text input/output alias with status 64.
Malformed records return 2 without changing an existing destination. Filesystem
aliases remain excluded by the private orchestration boundary.

The paired ordinary host commands are:

```text
Tools\Native\Construct-Hosted-Verifier-Publisher.cmd <windows|linux> <output.exe|output.elf>
./Tools/Native/Construct-Hosted-Verifier-Publisher.sh <windows|linux> <output.exe|output.elf>
```

They verify the complete hosted-container and publisher-construction candidate
inventories, natively lower and link the canonical publisher WVB, require
`Main` at offset 3,001, construct the target base and publisher records, and
require the final exact application identity. They never invoke .NET.

## Exact identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Base-metadata WVB | 70,166 | `6c79bd1b190e7d28190d1481a4ea1c602c227274806a885062af29d06c972630` |
| Base-runtime WVB | 20,387 | `56d329c2710322b4a3f74565dd5fb862be8ac4c26c7fb76f7c2bfafc66a9f8e2` |
| Publisher-application admission WVB | 30,837 | `f1e7497dc1acba1a08190021d4dac83ec65c3e6b58f80edb3bfcd62eeda55ed3` |
| Publisher-application admission WVO | 556,273 | `ac5972e8de83ad962874217ed6e0fba49586096df4c3b69d61abdf7509e2dff5` |
| Linked publisher fragment | 232,736 | `260e9f4f23c99dab13145ceb98724a4c74157fc579c5685194b7312c1a5cb115` |
| Windows base | 248,832 | `cf204201e5c26d71e78da1112de2bc724d389a5222cc835d48dbe8cd8bbc5988` |
| Linux base | 249,856 | `0bdeee07a49f75781767934884cbbc7dd085abff4507e2f78210fa225638539a` |
| Windows publisher | 256,000 | `735320b5ff33419d685925044add6f254bf402c0d49fc575c77f6110fac705f6` |
| Linux publisher | 254,917 | `de4f06f6d837eb58457a31b4757c3410e389ecc3c11fd79daf229dbdeb23e02a` |

Version 9 of the construction candidate contains 22 canonical WVB/WVO
artifacts and 22 paired host applications. Its 44-entry `SHA256SUMS` is 4,634
LF-only bytes with SHA-256
`83df3a245217c20bd704685e79d296c03bbdd85ee0377cd046a38f995735e273`.

## Native owner and remaining gate

The `hosted-verifier-publisher-files` retirement lane owns six fixed cases:
the complete inventory, exact Windows and Linux construction, malformed base
record preservation, exact alias preservation, and execution of the constructed
current-host publisher to install the exact verifier candidate. Its inventory
case also rebuilds and natively lowers the publisher-application admission WVB
and requires byte equality with both pinned artifacts.

The frozen managed file-pipeline test remains recovery/differential evidence.
Independent Linux execution, grouped qualification, completed-publisher durable
publication, candidate promotion, and release integration remain open.
