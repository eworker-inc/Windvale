# Windvale native hosted-verifier publisher base construction

## Status and scope

This contract closes the final managed input in hosted-verifier publisher image
construction. Two focused hosted Windvale tools derive exact verifier metadata
and runtime headers from canonical six-service or explicit profile-6/profile-7
eleven-service `WVSQ 2` requests.
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
wvhostverifierpublisherbasemetadata wvo-inspector <target:1|2> <native-entry> <request.wvsq> <metadata.wvhv>
wvhostverifierpublisherbasemetadata console-verifier <target:1|2> <native-entry> <request.wvsq> <metadata.wvhv>
wvhostverifierpublisherbaseruntime <metadata.wvhv> <runtime.wvhr>
```

The metadata tool admits one exact `WVSQ 2` envelope with its embedded `WVPQ 1`
plan, hashes the linked fragment and six ordered service leaves, constructs the
private `WVVE` evidence, and invokes the existing `WVVE -> WVVR -> WVHV` cores.
It writes only the admitted raw 1,024-byte `WVHV` payload. The runtime tool
requires that exact payload length before any fixed read, forms the existing
1,048-byte runtime request, invokes the shared constructor, and writes only the
raw 4,096-byte `WVHR` result.

The explicit `wvo-inspector` and `console-verifier` forms require the 156-byte
eleven-service plan, hash the fragment and all eleven ordered leaves, and carry
profile 6 or profile 7 through the 572-byte evidence and 624-byte request. The
legacy form and `publisher-admission` selector retain their profile-2/profile-8
bytes.

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

The base-metadata and base-runtime rows are the retained packaged identities
from the construction-toolset refresh that consumes the profile-7-capable
sources.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Base-metadata WVB | 78,245 | `a96d2bcae53793f95102a45278a229bf4615109e3df670aa44db02d8945e0703` |
| Base-runtime WVB | 22,465 | `39fc7a256118c41b360a1e0bcf1903798ecc8f1f4155cc787d59e6b92dd5e084` |
| Windows base-metadata application | 1,003,008 | `b23f04e029730edde01a8c3942d6b8771cfe65a7d8c56f6eac7f29c8371be1df` |
| Linux base-metadata application | 1,003,520 | `9ade312396293afa20ad5e409e10366c54b9c4616615ccb9cbcf0a4a4432101e` |
| Windows base-runtime application | 236,032 | `65e94ce354feae8121d8868a27308ba3ae1a993baf3313b8d847d1fb732102e2` |
| Linux base-runtime application | 237,568 | `db5fc8da164bb72da1c379b582c6bbffa64e8ecd1b6f6f4dd807a7e95f726055` |
| Publisher-application admission WVB | 30,778 | `b4e0a2ee04de6cfff0efc723c57031bf5cfcd6706e3156525ce2157c5f287d07` |
| Publisher-application admission WVO | 555,690 | `88cc97665cfd0de14f2c9ac6c80dfd985edc508fccdc3d9b887da740cd034e23` |
| Publisher-promoter WVB | 41,268 | `c0c7c88996ef837bc5a2ec3ceb1de61254b025fbd6504e4f3d7dc055c4140672` |
| Publisher-promoter WVO | 660,123 | `ba5d9c5afde115fede472369d24c3d1fe466806de523773d2e445e6a9e004667` |
| Linked publisher-promoter fragment | 658,339 | `e06189a37c038a5237787ffd16fb53466df3d10519efd4129b219bd814f4def2` |
| WVB publisher WVB | 159,770 | `8247539e0f4a5436b3902ec1fef33c6c39c231703de7bf505a6c65d66a764f96` |
| WVB publisher WVO | 1,319,377 | `edc49bbae0bfd16a38db4a08d9a6e636edfac35828e1c6b050c45d85d5e1f9e3` |
| Linked WVB publisher fragment | 1,317,613 | `9003479563a043bb69113be43100289f653f6772356c48a17098c1c6700f5271` |
| Linked publisher fragment | 232,736 | `260e9f4f23c99dab13145ceb98724a4c74157fc579c5685194b7312c1a5cb115` |
| Windows base | 248,832 | `2afd9d92422b063abd3cd20d8da6056efbbbff9e7ac8baeef9c8b60b391686c5` |
| Linux base | 249,856 | `687338281ca78c9d3a4d08b601c1efbcc198ec3c8fcc96fbf34f5dc349cafae2` |
| Windows publisher | 256,000 | `17cb5c4228e8448693b17f1b73695fd0ecfd03d7ada922794a5bf3bd7594fc96` |
| Linux publisher | 254,917 | `babe721a573e29f89ec095c35677880077ff465d4e2129063f6742cd47591a97` |
| Windows promoter base | 674,816 | `927476ca389c7449fb0c72341f26d68577a6a9e0c0ed02fa45ac8c4af935c77f` |
| Linux promoter base | 675,840 | `768ca223c99e901d17a1c5d86744515e4b571a6feae329fb6fc3cf225215a133` |
| Windows promoter | 681,472 | `598bd2de8247abd19d931efa1edcc8323adef7f56da51da1d41256933667eb23` |
| Linux promoter | 680,901 | `422332fb4f2824ae558bf93adadb6470597399d07810f5428f71aa4d971a4f58` |
| Windows WVB-publisher base | 1,333,760 | `8fcdcfc755439ebae5086c72d88113fb52f397ba0687c785af247230a7732fff` |
| Linux WVB-publisher base | 1,335,296 | `f53a4c8c5d292e999735cf5fd337b7c6997c0a8e6d2ba316ec94cd6b0838b090` |
| Windows WVB publisher | 1,340,928 | `71794a6a254ccfd652ffe3bad556c32f86e2d9210a5a3099bad576f97476a8f3` |
| Linux WVB publisher | 1,340,357 | `7f2dbfaecf2734c5afdbd6e2e54263a5a74038b8a498eeb1e155ee71788b630c` |
| Windows WVO-publisher base | 422,912 | `1f9361126c368f133693222cbaa4c21e2d0948e79df7bf945b7b037ac815e884` |
| Linux WVO-publisher base | 421,888 | `af61a601f4cd8e7fb81704353160a518d2e4f199084fde4b29518d27c89774f7` |
| Windows WVO publisher | 430,080 | `ad4c2a05115b2acdb074c0f53b6d7470c8bcacfdfea86583043bdd0ff511188a` |
| Linux WVO publisher | 426,949 | `4b0ce2d332648e3dd572596db4490748bf62ee4448a9550d83c152de60f7e51d` |

Version 17 of the construction candidate contains 27 canonical WVB/WVO
artifacts and 22 paired host applications. Its 49-entry `SHA256SUMS` is 5,064
LF-only bytes with SHA-256
`12b7cafbfeafcf1fc667e074ea0670f353bc883131d8a2f180008019f07d03d5`.

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

The separate `wvo-publisher-reconstruction` lane owns the role-3 candidate
inventory and exact native WVB plus paired-application reconstruction. It uses
the raw lowerer directly so the WVO publisher does not participate in
publishing its own construction object.

The frozen managed file-pipeline test remains recovery/differential evidence.
Independent Linux execution, grouped qualification, candidate promotion, and
release integration remain open.
