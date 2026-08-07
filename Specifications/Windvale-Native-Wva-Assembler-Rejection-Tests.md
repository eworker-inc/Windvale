# Windvale native WVA assembler rejection tests

## Status and scope

This fixed contract exercises one representative input for every stable WVA
diagnostic family through the ordinary digest-bound native assembler launcher.
It transfers deterministic rejection and output-preservation evidence without
rebuilding the assembler, generating expectations from Stage 0, or duplicating
every malformed spelling in the managed differential suite.

## Fixed inputs

Ten compact LF-terminated fixtures live under `Tests/Native/Wva-Rejections/`:

| Case | Fixture | Bytes | SHA-256 |
| --- | --- | ---: | --- |
| `wva1001` | `Bad-Header.wva` | 28 | `a0c401f0ff8df946469bc46a2a8e6aeeea17ac1335267d377c5636f2ada31376` |
| `wva1002` | `Late-Symbol.wva` | 97 | `e80f74ddb1daa2e52b731d70f01c2bef21910b70a5a5b3a83baafbf290bb35dd` |
| `wva1003` | `Short-Symbol.wva` | 33 | `05db14bde97f50b4373bac9d1d4432aceb84d67cd040f059a6ff275ace41de88` |
| `wva1004` | `Bad-Machine-Name.wva` | 56 | `13dcbcc9a1882d238c220f5ce91a9407e86e5ab558b2742a5454accd596cf694` |
| `wva1005` | `Bad-Alignment.wva` | 59 | `f7e2b5e7adc5e782289ba6d9e5f2f1505d7352ea2d79e8ce30af44d677633bdc` |
| `wva1006` | `Noncanonical-Symbols.wva` | 89 | `ddce08026e091ef40e43f770557e510660c99a3beba4eea4d840d66c5616c9e8` |
| `wva1007` | `Wrong-Symbol-Section.wva` | 133 | `490ee170d0899f724f2c51c326ebfb6b90b540d95f4663813d20ef2969fae9ac` |
| `wva1008` | `Wrong-Statement-Section.wva` | 128 | `e10d6cfc9568bfb13cc3281953eff68d9fc988521b5ca726adff59eb8e63a267` |
| `wva1009` | `Missing-Call-Target.wva` | 133 | `d5f86e0b5c975edaff2b82bfd7b48c5f8fcb1fd7a0ac49ea2601ca41a4f7d1ec` |
| `wva1010` | `Unclosed-Definition.wva` | 104 | `bfefa2b17caad9c1966854ff0f23dc0e73647db9ad8cbea9c3f1c882002c6030` |

The `wva1011` case creates a temporary 1,048,577-byte zero-filled input at the
one-byte-over-limit boundary. Its SHA-256 is
`2cb74edba754a81d121c9db6833704a8e7d417e5b13d1a19f4a52f007d644264`.
The generated boundary avoids adding a very large source fixture to the
repository. Every case starts with the canonical 479-byte return-42 WVO as its
destination sentinel.

## Rejection contract

The complete LF-terminated report identities are:

| Case | Report SHA-256 |
| --- | --- |
| `wva1001` | `4cfa4a4e82f3f03d8447865354e4c6f4d433680dadf3ce5c074e708c79a4de31` |
| `wva1002` | `8642b0a6d4d2ac84a8e5be5d8d6009bdbc945082c954c5a8e15359494c212d58` |
| `wva1003` | `b627119175c5b48c0ea1e7ad8566e61df57c467ebe2de91d92cf456131f8a53a` |
| `wva1004` | `909f464d645ede6ec49f119c933573638eff80525d1b1c49738b90c96cfcc27c` |
| `wva1005` | `d46c03051b79c8af12274df50737cc7963b77a6aa4404282558c052b07e94b65` |
| `wva1006` | `9c2270a866c3383ea43020bea7693d8d0ae87aae06fe86d46a35d30146e1a4ec` |
| `wva1007` | `440fb3e5eaf8153ee771d926274393e58a4689a14824c5a1846317bf819053d1` |
| `wva1008` | `9715af284f22626fd002ea4465185bfecf609be0fe378febbb93765d9736344a` |
| `wva1009` | `d0d7a09622b8cc73cf2a4b87863f1b0cfe7c20f3da343f01764092472f6f1fd8` |
| `wva1010` | `ce6ea19735ebbbfa18725b7b600c12be4a240e68b7f6e5aec061722278969af4` |
| `wva1011` | `0637a77d191b3e749c5779bcd069859f330314be167647d6db05bb96eb8d483c` |

For each case the coordinator must verify the complete input identity, invoke
only `Assemble-Wva.cmd` / `.sh`, require exit `2` and empty standard output,
verify the complete report identity, and preserve the complete destination
sentinel. It removes only its named temporary files and directory.

Success prints `PASS  wva1001` through `PASS  wva1011` in order, followed by:

```text
Tests: 11, Passed: 11, Failed: 0
```

The fixed matrix represents all stable diagnostic families, not every malformed
source spelling. The separate
[native WVA differential contract](Windvale-Native-Wva-Differential-Tests.md)
now owns the exact 200-case seeded mutation loop and agrees on every Stage 0
diagnostic code. Remaining representative valid vectors and arbitrary-source
containment still require equivalent independent evidence before the final
retirement gate.
