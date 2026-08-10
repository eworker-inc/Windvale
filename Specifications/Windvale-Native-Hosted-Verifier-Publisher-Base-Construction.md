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
Tools\Native\Construct-Wvb-Publisher.cmd <windows|linux> <output.exe|output.elf>
./Tools/Native/Construct-Wvb-Publisher.sh <windows|linux> <output.exe|output.elf>
```

They verify the complete hosted-container and publisher-construction candidate
inventories, natively lower and link the canonical publisher WVB, require
`Main` at offset 3,001, construct the target base and publisher records, and
require the final exact application identity. They never invoke .NET.
The WVB-publisher wrapper selects role 2 while preserving the original two
roles and their exact bytes.

## Exact identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Base-metadata WVB | 72,025 | `1396b613b18586cb68cdd010adb7864a70ac43d1cadca3f7c474ddd46a207fd2` |
| Base-runtime WVB | 20,850 | `aa354586184de25e0873c65d16fa35819eb9e4c50111b32ba4fd51d69327b806` |
| Publisher-application admission WVB | 30,778 | `c6ba933fa0ea1068f02235f75ed251655b10b43d64f8984d22b548f01608af0d` |
| Publisher-application admission WVO | 555,690 | `722d819152d8415487c1cf111474fd11dd0ab89a863e33ab84c865a2e3e13771` |
| Publisher-promoter WVB | 41,268 | `30eb1e8c93b01266592b322b9c5154b27782ea6c7cd2b6522a10781bf935bec9` |
| Publisher-promoter WVO | 660,123 | `6f20c95c4c09958dcc09ee35b8f7a3a0330d67f26446206be5bdd85cd8cb042d` |
| Linked publisher-promoter fragment | 658,339 | `a7c0ef19de332e00dcae74c9ab8c25b16b1e1ca73169d4485c85575412a28ed8` |
| WVB publisher WVB | 159,770 | `8247539e0f4a5436b3902ec1fef33c6c39c231703de7bf505a6c65d66a764f96` |
| WVB publisher WVO | 1,319,377 | `edc49bbae0bfd16a38db4a08d9a6e636edfac35828e1c6b050c45d85d5e1f9e3` |
| Linked WVB publisher fragment | 1,317,613 | `9003479563a043bb69113be43100289f653f6772356c48a17098c1c6700f5271` |
| Linked publisher fragment | 232,736 | `260e9f4f23c99dab13145ceb98724a4c74157fc579c5685194b7312c1a5cb115` |
| Windows base | 248,832 | `cf204201e5c26d71e78da1112de2bc724d389a5222cc835d48dbe8cd8bbc5988` |
| Linux base | 249,856 | `0bdeee07a49f75781767934884cbbc7dd085abff4507e2f78210fa225638539a` |
| Windows publisher | 256,000 | `735320b5ff33419d685925044add6f254bf402c0d49fc575c77f6110fac705f6` |
| Linux publisher | 254,917 | `de4f06f6d837eb58457a31b4757c3410e389ecc3c11fd79daf229dbdeb23e02a` |
| Windows WVB-publisher base | 1,333,760 | `a06095df9ab46b3816c376c2bedc6b07c8e6aff0eaf6c92ff2c2a47d9b210466` |
| Linux WVB-publisher base | 1,335,296 | `57cac655719571d20922bf6b3db33ec77781201ccd4dbd45fc41e14c651eb6ab` |
| Windows WVB publisher | 1,340,928 | `9ee91e3044193e2e90461ecf4e7ddefa4b5583f55b041b31911044c6d65b92c7` |
| Linux WVB publisher | 1,340,357 | `2ade91f624609c93a3b80a0802679bef79832c0a63db7996c889794d365f1188` |

Version 14 of the construction candidate contains 26 canonical WVB/WVO
artifacts and 22 paired host applications. Its 48-entry `SHA256SUMS` is 4,980
LF-only bytes with SHA-256
`217c33c4163719f998a3cfbe6694a5f42d07d78e7c50c31fa0358d95f4bad11a`.

## Native owner and remaining gate

The `hosted-verifier-publisher-files` retirement lane owns fifteen fixed cases:
the complete inventory, exact Windows and Linux publisher construction, exact
Windows and Linux profile-8 admitter construction, a current-host read-only
admission matrix, malformed base-record preservation, exact alias preservation,
and execution of the constructed current-host publisher to install the exact
verifier candidate. Its inventory case also rebuilds and natively lowers the
publisher-application admission WVB and requires byte equality with both pinned
artifacts. It also rebuilds and lowers the distinct publisher promoter, links
`Main` at 1,178, requires the exact 658,339-byte fragment, constructs both exact
promoter applications, installs both publisher subjects through the promoter,
and runs the installed current-host publisher through one verifier installation.
It additionally rebuilds and lowers the WVB publisher, constructs both exact
role-2 applications, and executes the current-host candidate on a canonical
portable WVB without loading .NET.

The frozen managed file-pipeline test remains recovery/differential evidence.
Independent Linux execution, grouped qualification, candidate promotion, and
release integration remain open.
