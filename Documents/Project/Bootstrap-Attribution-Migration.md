# Bootstrap attribution migration

## Purpose

Before public visibility, Windvale performed the one-time identity normalization authorized by [Decision 0032](../Decisions/0032-Public-Contribution-And-Governance-Foundation.md). The migration associates the bootstrap commits with the verified `EWorkerAI` GitHub project account without erasing their descriptive `Codex` author and committer names.

The pre-normalization history remains reachable through the annotated tag `evidence/pre-eworkerai-linkage`. Its tip is `1e94e9a0abd6029c986f69b31c2a7f461ac5eb10`. The corresponding normalized tip is `f2323f909865e8a26209d55856c448e5cddd15f7`.

## Transformation

For every author or committer email exactly equal to `codex@codex-dev-vm.local`, the migration substituted `246088022+EWorkerAI@users.noreply.github.com`. It made no other intentional commit-object change. In particular, it preserved every source tree, author name, committer name, author timestamp, committer timestamp, commit message, and parent order.

This changes commit identifiers because Git commit identities are part of the hashed commit object. It does not add a retroactive DCO sign-off, claim that historical verification was rerun, or change the artifact and source bytes qualified from the tagged history.

## Verification

The migration verifier established:

- Both histories contain 74 commits.
- Every mapped pair has the same tree identifier.
- Every mapped pair has identical author and committer names and timestamps.
- Every mapped pair has an identical commit message.
- Every mapped parent points to the corresponding mapped parent in the same order.
- Each changed email field had the exact authorized old value and exact authorized replacement.
- The two tip trees are byte-for-byte identical.
- Normalized `main` contains no `codex@codex-dev-vm.local` author or committer field.
- The mapped normalized history records 71 `Codex` author commits and three `E-Worker AI` author commits, all using the verified `EWorkerAI` address.

Historical qualification documents continue to name the pre-normalization identifiers and archive paths that were actually verified. Those identifiers resolve through the evidence tag; the table below gives the tree-identical normalized identifier used by `main`.

## Commit mapping

| Pre-normalization commit | Normalized commit | Subject |
| --- | --- | --- |
| `a908ae6c9a623e358a8ddccce881b33566c2944f` | `12764fe818793e76cc5e519872aece057fef8e96` | Establish Windvale project foundation |
| `5d3800b27ca580939389add177cb896a27ec0aab` | `0033a8a444adef51488d3460122fe5f8e1d21703` | Implement Windvale Seed toolchain |
| `e240655aa3611407bf3b09e94ff502f5677e4a14` | `066f40c478f8d19211255c470511dd217f79a52d` | Record Seed Linux conformance |
| `abb763001510d4776185be7f1ffd2a47606bd1e0` | `e0ff441324e685fd111d6b2a0717a58f2bb3ad70` | Adopt Windvale source naming and mutability |
| `e1c5333b98d2a7e0753f47baae912aa80101e964` | `b000eccfacc83a3b4a5406c196dbdc3dad1837b6` | Record naming contract conformance |
| `3f79530d97487c243528aa57576a321d440bffe7` | `e28a3d150f7da56a81a3d79dfa01cc787470a1b3` | Implement Foundation byte primitives |
| `98cf19fa262d9eb30ef1f7621d3da9dfd669c0ef` | `8468e200ce2065eabed4545fa3991833f4e5e0ae` | Record Foundation cross-host verification |
| `60fd261ac4a92e7338c05f35c994e9772fb5d4de` | `f807cc18da9f1bf688def699683ad5eb0c11afe9` | Implement Windvale wvdump section core |
| `f3c279a30e03631fca554586243b18e57f45c653` | `db6e4dc09764a703b28e1528ffbe0b400dffac03` | Record wvdump core cross-host qualification |
| `a4b0f5d54ecb74d0385a9cbca2fb259aa6135183` | `8789113743a42f2ba0943341687d466b73833a52` | Implement immutable Seed records |
| `bba4744a016af8e57ce8403c5ea865ac1e91f3b4` | `578b7bc9f59330e1af4fd8ebd8e496e1e1ab7b6d` | Record Seed records cross-host qualification |
| `e6c51c6b0be6a0d20fa1d62605ce50a1573f0e7f` | `4225aa567bf2ee35c0735788ce5d01585594eb6f` | Implement nominal enums and bounded formatting |
| `4a0f865547fbebe1a2512584fe5ff0dcb1b312e0` | `91e3047ee5f633b87ca646d043778377af09f604` | Record Phase 2 cross-host qualification |
| `1f4b48af19cc190b6dc943b37ed6dc28747d0a60` | `41344b865d2e6b1159cb4f9d506c72312ba7b904` | Implement explicit hosted resource boundary |
| `7ec4fa1ca30382a12b636b450eb49ca0c9a9786a` | `b246724ae141a93b9357ad62c9c21a4badc45ba9` | Record Phase 3 cross-host qualification |
| `a829fc8c9b3ba276335f30fd5fc2671a771cbdbc` | `c09bca941b7bdeddf19f8895652117af2ee71462` | Implement useful Windvale wvdump reports |
| `7a296bf4e1209d2b4f85cbf14283a6453198d3ca` | `467ee46dfae6364b93368f20a7d632fad661894a` | Record Phase 4 cross-host qualification |
| `f87a5fad42fe72ff06067195357002fb5d2b7bbe` | `1670ea7b1845f2c1df13629dc1c2515994a7d5a1` | Implement the Windvale object foundation |
| `72ea1f80e4c3629f3ff99916323b9857ec265e67` | `8a39bc5c98aeb804f6035d3bcbe28bca8b995053` | Record Phase 5 cross-host qualification |
| `3bfc6bbf5a8bb5640ba75a9a4047b3fa7bae4bc0` | `30238f424fe783853f505467abcb248ebd35a082` | Implement the WVA Stage 0 assembler |
| `3c9baaf32145579da8c0de46971b5193867efcfd` | `798bfd5aaf3237219e1a2efa275754bbe6198cc4` | Record WVA Stage 0 cross-host qualification |
| `284fdfd535ec12246d81ae285f3a6b4decc1a140` | `10cf49c7594960a77e52964f26182e5e9a1a7c64` | Expand the Windvale execution roadmap |
| `57f2544809a79555d4496f7ce706769086260515` | `90a7a7b21a32fbfaaae43df80d71697b372e2750` | Implement the Windvale WVA scanner |
| `e5fd109f0f4fcf77bf9e509db27a37834bb3e820` | `1045df0da3e46b86b09417d6b2d6b7f461fdb802` | Pin WVA sources to LF in archives |
| `a84852173400723c3641adf0dfc91930e13c8489` | `384108409ed14c10c303938d750b8fd50051b45c` | Record WVA scanner cross-host qualification |
| `cc57bf9c50080b05d52fde8559362f3d2d006fc3` | `4ab2b0c121352f7d91ec833a469de4eff9bfeb2b` | Implement Windvale WVA semantic inspector |
| `775f1964afd450e351383194dc415b1c221b02a8` | `2cb90ffde7e501e8da2c935ec6a71ee2cae9b2b6` | Record WVA semantic inspector qualification |
| `a68961742fd5dccaa51da13f54b7924c12ce9f55` | `1e539ac25d943bf9fef329cad5b30b4bf709fc99` | Implement Windvale WVA object encoder |
| `69af320e862f36d01ed3d5df069299b32dc42d63` | `c058da6ada8eb6f84e3c540a86f73ec86df3992d` | Record Windvale assembler qualification |
| `9c4b9f533e4e83873893d6e4fc2e53f49899762a` | `445d6b18e21a81afaf02f51cddcf8194a07d7834` | Implement deterministic Stage 0 linker |
| `a73264d9e64f51c00d26261fdb49946ccb70926d` | `0914227fff8f73759577169127f76b6aadec9d67` | Record Stage 0 linker qualification |
| `348c82a0c4bb0031dbfa815dd66203b6c6d543f7` | `f50baa836c742d74de47e9ca29061ecc95b16b8b` | Add deterministic linker bootstrap primitives |
| `c34de9934e0f19f3f7a15cfce46f791bccabd704` | `50bd342f18f3de2699270b8fa71e4be447adce86` | Record linker prerequisite qualification |
| `89ce80b009db338a4d0bdb6b2d8903655450a4ec` | `62be8c392a00b1cf36b957307c473a20edd8e9d6` | Use balanced persistent runtime bytes |
| `7fe0612e04be4076e0d4f7be31cbaf7d48d5ee0a` | `14909a397a5d6e06bf76ef52edaec1d8c9804eef` | Record persistent byte qualification |
| `3eb331aba3fc84f54a350d1eb08df62f7bcc5863` | `ec1a5ecba1a0abb65a092a44c7553b77b32d4188` | Add Windvale linker object scanner |
| `46dc4e039687f7f1e3b968ea069b52fdf5d930b2` | `59523c71ce7c918806a8fbdf7eb1593355fb8a47` | Record linker scanner qualification |
| `709ccb352592ade1e796ed090d89d36c5f4544ef` | `bb2e0d022dfb401f9add751567e59c34a121aa1f` | Add Windvale linker resolution and layout |
| `745d832e3bd3fec8dfc9e23c759a96d8ab569341` | `01f2e34bb2b56c53774437781c481d03d8ac274e` | Record linker layout qualification |
| `ec9c98001527bc9fc841d7e5a4012d4c5aa6aeb6` | `50ff2b916120815c76beffc5545e089bde62bf9e` | Build Windvale linker image and relocations |
| `0ae98db8c85833efcf1610da63b6c0110d9afb23` | `a3a1ce98891944981421df83f7a130cc31019e69` | Record Windvale image qualification |
| `d8008e377e7b060fcc8ba9334ce61b75f7c9107a` | `e7d45ba2833468706fed86afd58d99268fa31b94` | Add independent Windvale image verification |
| `c4770a763d86701bf5c76cfaab523c1f64314d3a` | `45a45091f6078b8cfb453c20e13b82ea0a2b6ff9` | Record Windvale reconstruction qualification |
| `40ac57d1b2ff2a4c32294bf62dd6da3da1d33766` | `cfe6f5e56d6c16f278bb42cca1fef990f612c38a` | Complete Windvale linker publication |
| `6b135557f59fe256c5fd05f62e2bbc3fe81a8193` | `453647c45304c23243c550e99e96e90192d2d03c` | Record complete Windvale linker qualification |
| `df80f9154c45eff66c687ce4427f0f08b6e37382` | `142d7d25149ac9f5a5dc6dd36ac217b3d351f131` | Add bounded source module composition |
| `d7506d607439e1bc5d3c6fb56531794cea3a3b0e` | `da22a1a8a92d7fbba3aabae6c3d4fec3e6c77310` | Record source module qualification |
| `d46af863420eb854de4d3054f3464688a918f4ca` | `67a5d2847f20c4437e4d2f1c9d959e9990df4305` | Extract shared Foundation machine contracts |
| `6bb8f844e6aa68c2dfb1e029779e722994abfb14` | `fd932b0c0e97b77fac16da1e4f617c1ba1f0b69f` | Record first Foundation module qualification |
| `4fdea22b1208d9bf18fed8d55dcf29adf6080683` | `8ab6789c232503388945fe653230c3e0121a5d1d` | Extract shared ordinal byte ordering |
| `07287b19905ec5df272440ae7390f0c232c791d0` | `7c4fd7f65e1abf4ca0d21b5cc7b2cd5e287634b7` | Record ordinal byte ordering qualification |
| `6d2a3515105a2a8d651e9aed75fba9c25b4949c9` | `c45fa10542e2606f73f9a12ddea4ad1f40767b48` | Add nominal Foundation decimal parsing |
| `37c40f6086e71a5f2a039cdb9b027859a895a4dc` | `f60928f96fbb33591265cc2cadceed5a3a1c30fa` | Record decimal Foundation qualification |
| `26e2fd19eecc14866a318177423c928a452dc7f0` | `5e69f121bf14222d96992ef85e1dcf7e99973131` | Add bounded Foundation byte construction |
| `f99107412b1afd07cefb96e2e31c2cf8402a1ef3` | `90b444dd74c597c242f60afa669ea65174a652e3` | Record byte construction qualification |
| `d91dbfb05a8b096d6b9778f0c4e4da855ca96ed3` | `b1dc7d719509884279be21e5db44ffa3c4e0d588` | Implement Windvale bootstrap source lexer |
| `65419d41440afb0100f5f6aa0bf05a17949007a8` | `518414757efeb59e3b955744bdc6669ae8e7a771` | Record source lexer cross-host qualification |
| `fc87a3e69b413bab06c5cb739f83823afc7775b3` | `a3ecbaa752f00f680decbd13019ec48980e1413d` | Implement Windvale declaration parser |
| `27da5133916f22c92034ab0a2079ed6a2619b1f3` | `8bfadfa9a064f545aeb66d351c308044961b2dc4` | Record declaration parser qualification |
| `ddfa9e379cc7df82ebac14b8d2120b9afdc26e4d` | `d43389da497b338b901741d463fb683e8c6cbce4` | Implement Windvale body parser |
| `d7ed6336c6a473a80ffe068925e4cf198921f8d0` | `3dce75572bfae535b4e6c0adcdfebe294b18812c` | Record body parser qualification |
| `f9fab08c88b1a1006e9b2e56ba914ffc7f2a723a` | `884d20970ea69683177d38f7af12c4bacd9d8b80` | Adopt MIT license and E-Worker stewardship |
| `00ef0b10747937b50980fa9477dcdeb94d719048` | `91511bb900955597b618350ce35646f967346e1b` | Implement canonical compiler source sets |
| `ec88d5396e7e4c77de3a921aae0d8b58301bbbf6` | `a88abedab5f5a8e5a371b1891923c8372ddbca69` | Record source set qualification |
| `09c6f54399774423ed8b5ee1bacb6a3a8803014b` | `5627c4abe5ed224f1e1796c838f03950df32c37e` | Implement portable compiler import graphs |
| `b178c6451f6663883971ca845289c7dcc6106684` | `71ae9ad2375a9d0427c63eafcb5a48bd6838c7c2` | Record import graph qualification |
| `ef6d9efdd712625597823b8ef1dc1253019e8f28` | `fddc532e49146bd07ac15fb667c9617bd6f4a07e` | Prepare public repository foundation |
| `b1b5bdddd5a9b7f5444d58dc5e8e67ef4081fada` | `fe20a721f9fa1ccfade49b04efc3943b0534557f` | Implement portable declaration and signature binding |
| `d57a6d8009d2d6d73e189a560628e2096e75eced` | `f5adbb45ad185bc780fa1e01ec6b49f452a5c52c` | Resolve compiler decision numbering |
| `f180ed10fed87c98229a5998f0c2ee54815fc15f` | `970dd0840c7879302ae65456610da4bd32827268` | Prepare private-first GitHub publication |
| `5bda4c2586d7f21558f840c3d8a1923a9e47b305` | `404ad5e4a06b804bec8f2ed43c183fbb20a1f284` | Record declaration and signature binding qualification |
| `223cf125d97f80d45fe019d2bc14efd1bd904008` | `61b856ce53d00c6930ec8fb4816e1c4f583836df` | Adopt E-Worker AI commit identity |
| `de88007b4716c88604321baaad4c4d5c417d317e` | `00466fd9e9feaac4655cdf9748ac1dc56b586a84` | Speed up Seed verification feedback |
| `1e94e9a0abd6029c986f69b31c2a7f461ac5eb10` | `f2323f909865e8a26209d55856c448e5cddd15f7` | Authorize pre-public account attribution migration |
