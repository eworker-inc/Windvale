# Rejected shipment, cache, and cross-host cases

Every mutating failure preserves the old active generation and removes only its
exact private candidates under the existing installation transaction rules.

| # | Case | Required rejection |
| ---: | --- | --- |
| 1 | Installer silently selects a pack from host locale | Reject hidden product/semantic selection. |
| 2 | Draft Chinese pack is labeled officially qualified | Reject unsupported release status. |
| 3 | Runtime package embeds source packs without development-tool intent | Reject shipment-scope expansion. |
| 4 | Optional pack duplicates compiler/runtime/library executable blobs | Reject package graph as unnecessary duplication. |
| 5 | Shared Unicode/token bytes are stored once per profile/generation | Reject failed content-addressed deduplication. |
| 6 | Deduplication trusts filename, size, locale tag, hard link, or unverified digest | Reject object reuse. |
| 7 | Existing digest path contains different bytes | Report store corruption; never replace under the same digest. |
| 8 | Package exposes a writable alias to an immutable object | Reject store isolation violation. |
| 9 | Compiler searches installed packs or chooses newest/first | Reject ambient resolution. |
| 10 | Missing selected pack falls back to English or another locale | Reject build before source parsing/publication. |
| 11 | Offline missing optional pack triggers an unrequested network call | Reject authority/transport expansion. |
| 12 | Connected/offline portable objects differ for the same release selection | Reject release/repository evidence. |
| 13 | Transport name changes semantic package/profile identity | Reject acquisition leakage. |
| 14 | Update mutates `zh-Hans@1` bytes or an existing source descriptor | Reject immutable-version violation. |
| 15 | New Foundation interface is paired with a stale old catalog | Reject exact interface binding. |
| 16 | Activation occurs before all new objects/generation checks pass | Reject partial generation publication. |
| 17 | In-flight build observes a mixture of old/new generations | Reject snapshot isolation failure. |
| 18 | Rollback decrements signed high-water state or bypasses revoked/minimum policy | Reject unsafe rollback. |
| 19 | Removal deletes an object reachable from rollback/recovery/pin/audit | Reject reachability proof. |
| 20 | GC runs without explicit authorization and dry-run inventory | Reject destructive maintenance. |
| 21 | Cache key omits format, hash, compiler/table identity, interface, option, dependency, or target input that affects its layer | Reject cache identity. |
| 22 | Cache key uses path, mtime, host locale, installation order, or display locale | Reject nondeterministic/stale key. |
| 23 | Cache publishes before full cross-reference/collision validation | Reject partial immutable state. |
| 24 | Same-hash race leaves multiple retained entries or private debris | Reject publication cleanup. |
| 25 | Failure becomes a durable shared negative cache | Reject stale failure retention. |
| 26 | Cache reuses another request's raw spans, diagnostics, source map, or debug provenance | Reject request isolation breach. |
| 27 | Cache budget makes otherwise valid source fail solely because entry cannot be retained | Reject semantic dependence on optimization policy. |
| 28 | Retiring generation remains retained without an active/shared reference | Reject memory leak. |
| 29 | Persistent cross-process cache is added without measured need and corruption/schema/locking qualification | Reject unevidenced complexity. |
| 30 | Sidecar “already hashed” metadata substitutes for immutable store evidence or first-use hashing | Reject trust shortcut. |
| 31 | Warm-cache report still rereads/rehashes/reparses unchanged artifact payloads | Reject warm-cache claim. |
| 32 | Collision/completeness validation performs pairwise scans at maximum bounds | Reject superlinear implementation. |
| 33 | Windows/Linux use different Unicode/pack bytes but are reported conformant | Reject cross-host equality claim. |
| 34 | Host-native generation bytes are falsely required/reported equal across different targets | Reject incorrect comparison boundary. |
| 35 | Timing report mixes startup, process-cold, service-warm, and storage-cold states | Reject uninterpretable benchmark. |
| 36 | Benchmark reports only best run or omits input hashes/host/profile | Reject performance evidence. |
| 37 | Threshold is guessed before implementation or automatically reset after regression | Reject unevidenced gate. |
| 38 | Sub-millisecond relative noise alone fails a build without absolute floor | Reject unstable regression policy. |
| 39 | Maximum-size fixture allocates from declared maxima before validated lengths/counts | Reject memory-bound violation. |
| 40 | Diagnostic/failure path retains unbounded candidates, messages, or cache state | Reject boundedness failure. |
| 41 | Paper protocol is called measured Windows/Linux qualification | Reject evidence-status inflation. |
| 42 | Fixture's 12,288 bytes are presented as the complete future Foundation/docs/compiler shipment | Reject misleading size claim. |
