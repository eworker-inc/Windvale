# Windvale native WVO read-only rejection tests

## Status and scope

This fixed contract exercises every stable WVO 1.0 rejection family through
all three digest-bound read-only launchers. It transfers deterministic malformed-input
reports without adding another object parser or deriving expectations from the
managed implementation at test time.

## Exact inputs and reports

The matrix reuses the existing bad-magic, truncated/out-of-bounds, and trailing
fixtures under `Tests/Native/Wvo/`. Ten additional compact base64 fixtures under
`Tests/Native/Wvo-Rejections/` are exact mutations of the canonical 189-byte
sample object, except the one-byte short-header case.

| Case | Decoded bytes | Input SHA-256 | Report SHA-256 |
| --- | ---: | --- | --- |
| `short-header` | 1 | `6e340b9cffb37a989ca544e6bb780a2c78901d3fb33738768511a30617afa01d` | `97779c19c3b55c92f53faa567de292403493fbff7180cfb6e2bade8991ef63aa` |
| `bad-magic` | 479 | `0369f8b34765adb08799e6b852e9d1e249c40d1049976b01ff59355dd111f288` | `2e53f573d1e94159c58368c4d9ebcba284d6c13f63a286bd75264bc837a162e4` |
| `bad-version` | 189 | `3c724339c2a6fe6d41c07a461907e5bbee7abc95cf899b0605e77f744f0c6081` | `bce421b96f8ee4ce19c322eba64a71bcefa3539640b41ecca2a5cd70bab4055e` |
| `bad-architecture` | 189 | `7ff46081c9b5f3d50d0a499f74d665bb9b474e308432ddcf484079a6f434db3d` | `8f6a586a1323284e6aeb9522fc292b266e4368f44d5e022b87fab28632a2da97` |
| `unsupported-flags` | 189 | `b1b581c75901f1bba0dfeb37fb888342d32c6f6eff565165277d051d7ae0f4c7` | `3eab07bbffa763acfd259b4e3b0b09206098c61f625bf23e202dc16fb19cc11c` |
| `limit-exceeded` | 189 | `6e191db4e2ce6107493baed610e9d116018ae887972d94d3df5969d3d405c0a8` | `d502b71111e5f7557fff108bc740b558d6d15acdc5eb22ada9f8cfe2dca0a46e` |
| `out-of-bounds` | 478 | `6f120ce6b833f781ab014844af535b25fe28eb2d565afa2b2f4360c7a0c99371` | `9b45f12022ab0ba549e6c2ffa49cb15673d96c8f58efd5d6d9c2def87097aedb` |
| `invalid-name` | 189 | `2cf0c91c9e6df189f2a79214bc5b5a3690e3b0140e41eae2683efd817bf9d067` | `bf35958972ccf812961fd52b92b1ebeb6f5e9b7e87a77c7083064de590c548cb` |
| `invalid-section` | 189 | `d0a93c19fceb58070797c893f3ba5eb3ebae60e380a85d5fd84cf037995702e8` | `430a541121485335be6635ec6277141489dafb4b73ec47dcfb1ddc72a32e649d` |
| `invalid-symbol` | 189 | `9ba10fcccc2e6d4b9a9fef8343dacb1743a2c2e1f0c1795ef0b97a3b50f655a5` | `b3dd9e318a471bf1f8f5e589d1c119f4b89b02f69d3956f42a51bde5afc1875e` |
| `invalid-relocation` | 189 | `b36011ba5615c228dcf6c4d389c7c50f24b25934b47d01bbbc701c9bf02b2736` | `b6b147e8141a3de78ab59b3af4d04081c37d77b8124ccdbefffc94645ab18995` |
| `noncanonical-order` | 189 | `443499e89326160f6172be9dd0be918935373e1c862d2192570cc922471026a7` | `2012a1501f7861708c992f61dfe308bc8ef217781b5e92bd2ca67fc56d6e31d8` |
| `trailing-bytes` | 480 | `3ca5e84240e8f12be84fdb957df37f8162e74415417cd7009f92698e683ee981` | `3cdcb2fa62f4fc698e9624e68dc10dbf95e7363cf0332b280066083cc1783711` |

Each complete report is one LF-terminated
`object status=<status> sections=<u32> symbols=<u32> relocations=<u32> offset=<u32>`
line. The report hash captures the exact status, counters, failure offset, UTF-8
encoding, and terminator.

## Rejection contract

For each case the coordinator must:

1. decode and verify the complete fixed input identity;
2. invoke `Check-Wvo.cmd` / `.sh`, `Verify-Wvo.cmd` / `.sh`, and
   `Inspect-Wvo.cmd` / `.sh` independently;
3. require exit `2`, empty standard output, and the same complete report identity
   from all three public launchers;
4. recheck that each read-only command left the input byte-for-byte unchanged;
5. remove only its named decoded input, reports, and temporary directory.

Success prints `PASS` for the thirteen cases in status order, followed by:

```text
Tests: 13, Passed: 13, Failed: 0
```

This family matrix does not replace the outer
[hostile-size object contract](Windvale-Native-Wvo-Hostile-Size-Tests.md). The
separate [native WVO differential contract](Windvale-Native-Wvo-Differential-Tests.md)
owns 128 valid-shaped mutations and 128 arbitrary values while this matrix owns
exact inner reports for all thirteen stable status families. The complete
retirement gate must still qualify all three contracts on Windows and Linux.
