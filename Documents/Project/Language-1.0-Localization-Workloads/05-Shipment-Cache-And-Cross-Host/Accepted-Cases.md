# Accepted shipment, cache, and cross-host cases

These cases define future implementation/qualification expectations; paper
acceptance does not claim they have executed.

| # | Case | Expected result |
| ---: | --- | --- |
| 1 | Runtime-only installation | No source-profile, diagnostic, documentation, or generated source-table object is selected. |
| 2 | Minimal developer installation | Shared edition plus exact `en@1` objects are selected. |
| 3 | Developer adds Chinese source | Exact `zh-Hans@1` objects are added; compiler/runtime/library executables are not duplicated. |
| 4 | Developer adds only Chinese diagnostics/docs after source support | Non-semantic objects change; compiler build identity/result does not. |
| 5 | Both profiles reference shared Unicode/token objects | Store retains one physical object per shared SHA-256. |
| 6 | Two catalogs in packages have byte-identical content | One immutable content object is reused while both logical item references remain. |
| 7 | Existing matching store object is encountered | Manager rereads/verifies according to store policy and completes idempotently. |
| 8 | Installer UI locale suggests Chinese | Final visible selection is explicit; host locale never changes source semantics. |
| 9 | Offline English-only release selection is complete | Installation/build resolution succeeds without network access. |
| 10 | Offline release includes optional Chinese selection | Portable object hashes/logical package selection match connected acquisition. |
| 11 | Same connected/offline request occurs on one host | Immutable generation bytes match when every generation input is equal. |
| 12 | Build descriptor/lock selects an installed exact profile | Resolver supplies exact immutable bytes/handles to the compiler. |
| 13 | Multiple installed profiles exist | The explicit descriptor/lock alone selects the build profile. |
| 14 | Terminology update publishes `zh-Hans@2` | Version 1 remains immutable/usable; adopting version 2 requires explicit source/lock conversion. |
| 15 | Installer updates while a build is running | Running request pins old generation; later request sees complete new generation. |
| 16 | Update succeeds | New generation activates atomically and prior generation remains rollback-reachable. |
| 17 | User rolls back | Old exact objects/profile work again without rewriting source or decreasing signed security state. |
| 18 | Optional Chinese selection is removed | New generation omits it; active/rollback/pinned objects remain reachable until GC permits removal. |
| 19 | GC dry run follows active/rollback/recovery/pin/audit roots | Only unreachable localization objects are proposed for deletion. |
| 20 | First content-cache request sees one artifact | It hashes/parses/validates once and publishes one immutable entry. |
| 21 | Later module uses the same artifact in one service generation | It performs a cache hit with zero artifact-byte read/hash/parse work. |
| 22 | English and Chinese profiles share edition inputs | Both reuse one published Unicode/token entry. |
| 23 | Eight concurrent requests miss the same content key | Single-flight or equivalent race handling publishes one entry and releases duplicates. |
| 24 | Cache budget cannot retain a valid new entry | Request may complete privately; cache pressure does not alter semantic validity. |
| 25 | Compiler/package update starts a new service generation | New requests bind it while old requests finish against old immutable state. |
| 26 | Old generation loses its last request/reference | Private entries retire and retained-byte report reaches the defined shared-state floor. |
| 27 | Same raw source compiles under different diagnostic locales | Semantic/cache result is unchanged; diagnostics remain request-owned. |
| 28 | Paired English/Chinese sources compile on Windows/Linux | Canonical projection and portable semantic outputs match exactly. |
| 29 | Cross-host installer contains different native target objects | Report marks target generations host-specific while portable localization hashes remain equal. |
| 30 | Benchmark runs process-cold and service-warm states separately | Report exposes the avoided hash/parse work rather than blending states. |
| 31 | Reference benchmark completes 5 warmups and 30 measurements | Median/p95 plus raw bounded results are reported. |
| 32 | Maximum-size/concurrency workload completes 5 measurements | Time, memory, failure cleanup, and retained state stay within accepted implementation ceilings. |
| 33 | Implementation sets host thresholds after representative measurements | Reviewed versioned ceilings become release/regression gates. |
| 34 | Exact Release 1 fixture inventory is recomputed | Unique totals remain 3,998 shared, 4,101 English, 4,189 Chinese, and 12,288 combined bytes. |
