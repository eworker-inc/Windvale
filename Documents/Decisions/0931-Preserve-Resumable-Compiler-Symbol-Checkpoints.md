# Decision 0931: preserve resumable compiler symbol checkpoints

## Status

Accepted and implemented locally on Windows on 2026-09-03. This decision
changes only the private development cache. It does not change source
semantics, compiler output, release reconstruction, or qualification evidence.

## Context

The split-project compiler can publish an internal `WVSY 1.0` symbol checkpoint
before its more expensive analysis and emission work. The earlier cache route
kept that checkpoint only in the private in-progress directory. If a later
analysis phase failed or timed out, cleanup correctly removed the whole
directory, but a retry then repeated unchanged source scanning and symbol
construction.

That behavior made failures safe but unnecessarily expensive. The symbol result
already has an independent validator and a complete dependency identity, so it
can be retained separately without treating partial analysis as a successful
final cache entry.

## Decision

1. Store completed `WVSS` and `WVSY 1.0` values in the separate private
   `project-symbols-wvsy-v1` cache family.
2. Key that family by the exact Analyzer identity, project and closure inputs,
   source-set request, cache implementation, and format version. Do not include
   the emitter identity because emission cannot affect source scanning or
   symbols.
3. Record and revalidate the exact size and SHA-256 identity of both values
   before reuse. Missing, malformed, mismatched, or corrupted entries fail
   closed and cannot seed later analysis.
4. Copy validated values into a private candidate directory. The second
   Analyzer invocation independently validates `WVSY` against the unchanged
   `WVSS`, and the coordinator compares both values again after consumption.
5. Keep the final `project-analysis-wvca-v3` cache unchanged. It publishes only
   complete `WVSS`, `WVCA`, `WVLB`, and `WVIR` products; `WVSY` remains private
   resumable evidence and is never a distributable compiler artifact.
6. Preserve a completed symbol entry when later analysis fails or is
   interrupted. Preserve existing atomic publication, quarantine, replacement,
   and cleanup behavior for both cache families.
7. Keep uncached paired-host reconstruction as the release and qualification
   oracle. A development cache hit is not reconstruction evidence.

## Verification

The existing `compiler-split-development` owner now runs a ten-case cache
sentinel. It proves ordinary identity and ordering behavior, failure cleanup,
replacement and quarantine races, combined diagnostics, reuse after an injected
later-analysis failure, and rejection of corrupted symbol bytes. The focused
owner passed four external cases in 6.240 seconds on the Windows development
host.

## Consequences

A compiler retry can resume after the completed symbol boundary instead of
repeating that work. The optimization does not weaken the final analysis cache,
reuse evidence across undeclared inputs, or make a partial compile look
successful. Cold analysis and emission remain performance targets, and release
qualification still starts from fresh inputs.

## Reconsideration triggers

Remove or revise the separate family if symbol construction gains an undeclared
emitter dependency, if its independent validator no longer reconstructs the
complete source relationship, if cache corruption can reach analysis, or if a
development result is used as release reconstruction evidence.
