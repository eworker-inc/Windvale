# Windvale native source-to-AOT composition

## Status and scope

This is the implemented candidate composition proof for the accepted metered scalar, structured-control, loop, and direct-call native subset. Decision 0305 exposes the proof as digest-bound `Test-Aot-Chain.cmd` and `.sh` coordinators over the pinned candidate tools. It does not define a new serialized format or claim the complete backend, an ordinary AOT launcher, artifact promotion, or .NET retirement.

```text
Project 1 source
  -> qualified native source builder and publisher
  -> canonical verified WVB 1.11
  -> WVHN 1 native WVB-to-WVO tool
  -> canonical WVO 1.0
  -> native standard flat linker
  -> flat x86-64 image plus canonical link map
  -> WVHP 1 native console packager
  -> version-1 PE32+ or ELF64 application
  -> direct host execution
```

Every arrow is a process or file boundary already owned by its linked contract. The composition adds no hidden in-process adapter and gives no tool authority beyond its existing capability profile.

## Fixed fixture and products

`Windvale-Native-Test-Wvb-To-Wvo-Return-42.wvproj` selects one portable `Main() -> i32` fixture. Its current deterministic products are:

| Product | Bytes | SHA-256 |
| --- | ---: | --- |
| WVB | 174 | `7933c4ba0cb854477a95750966f9532c2b9eb5888e55ec9ae64ebdf552a08f31` |
| WVO | 479 | `0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5` |
| UTF-8 link map | 630 | `857710249807d2fed4da847729d0244f08ccdc70156c043fdaa0516de394e2dc` |
| Flat image | 406 | `7c05565142850adab1d63d999479977a23ef50c7264c03ee55ce5b323df26408` |
| Windows x64 PE | 2,560 | `8f2c3389dafa40c0231a0f5aeead3db5570697d54874f324a81f84a2d5b16eb6` |
| Linux x64 ELF | 8,304 | `fe525b84b9bf902677a5c7beb36872dfd72e7d6d0f12bfb5c95d491c4e1cd3f7` |

The WVO contains one 406-byte code section, one exported function symbol named `Main` at offset zero, and no relocations. Standard linking at base address 1,048,576 preserves that entry at image offset zero. The console packager therefore receives entry offset zero. Direct execution returns 42.

## Verification boundary

The focused test must:

1. build the fixture through the pinned native source-to-WVB front door;
2. require the fixed WVB identity and structural module properties;
3. execute the current-host lowerer and require the fixed WVO identity and object structure;
4. execute the current-host linker and require the exact canonical map and flat-image identities;
5. execute the current-host packager and independently recover the exact image and entry from the resulting container;
6. execute that container and require process result 42; and
7. observe no named CLR, .NET host, or runtime mapping in the lowerer, linker, packager, or result process.

The retained C# differential harness still reconstructs the candidates as explicit Stage 0 evidence. The Decision 0305 coordinators instead consume the pinned digest-bound applications directly, check every fixed product identity, and require result 42 without a managed process or live C# expected-result generator. They are the permanent fixed-vector route intended to survive Stage 0 archival.

## Promotion and retirement boundary

This composition remains candidate evidence until the accumulated source state passes the grouped Windows/Linux gate. Decisions 0301 through 0304 add the exact candidate tools and digest-bound launchers; Decision 0305 composes them without promoting them. Complete native lowering, unsupported module routing, native host-container construction and publication, release construction, broader native test ownership, clean previous-native-release bootstrap, and the final archived Stage 0 release remain mandatory under [Decision 0057](../Documents/Decisions/0057-Windvale-Native-Execution-And-Dotnet-Retirement.md).

Related contracts:

- [Native source-to-WVB front door](Windvale-Native-Source-To-Wvb-Front-Door.md)
- [Native WVB-to-WVO application](Windvale-Native-Wvb-To-Wvo.md)
- [Native linker application](Windvale-Native-Wv-Linker.md)
- [Native console packager](Windvale-Native-Console-Packager.md)
