# Decision 0788: Make split compiler emission target-aware

## Status

Accepted on 2026-08-20.

## Context

Decision 0779 deliberately published the first split emitter in complete,
unoptimized mode. That was the safest initial equality oracle, but it made every
ordinary cached Project 2 product retain functions and data unreachable from
the selected exports. As the Language 1.0 compiler closure gained generic
lowering, its complete WVB reached 1,268,289 bytes and its selected native image
reached 37,097,130 bytes. The latter exceeds the unchanged 32 MiB native-image
limit even though a large part of the image is not reachable by the hosted
emission tool.

The split development owner also rebuilt three large compiler products for
every coherent adapter or cache change. The Language 1.0 front door already
owns full compiler-core reconstruction, so that work duplicated semantic
evidence and dominated feedback time.

## Decision

1. Give the hosted split emitter one fixed product target,
   `portable-wvb-optimized-v1`, and pass `Optimize = true` to the shared
   prepared source backend.
2. Keep complete emission in the one-shot compiler's explicit `--complete`
   mode. The split cache has no implicit or unkeyed mode switch.
3. Advance split producer identities to version 2. Analyzer identities name
   `source-analysis-v1`; emitter identities name
   `portable-wvb-optimized-v1`.
4. Use `project-analysis-wvca-v2` and
   `project-split-wvb-optimized-v2` cache families. Producer identity bytes,
   project inputs, host family, and analysis identity remain exact key inputs.
5. Require Project 2 dependency paths in canonical ordinal order at the focused
   adapter boundary.
6. Replace the duplicate large-product development check with four focused
   cases: adapter routing, the exact optimized pruning oracle, its exact
   complete counterpart, and forced-failure cache cleanup. Compiler analysis
   and emission core changes select the Language 1.0 front door once; adapter,
   identity, and cache changes select the focused split owner.
7. Do not raise compiler, object, native-image, fragment, byte-value,
   diagnostic, instruction, or timeout limits for this change.
8. Retain one portable bootstrap pair: a 949,355-byte analyzer WVB and a
   746,557-byte target-aware emitter WVB, each with exact source, project,
   producer, size, and digest provenance. Do not retain another native analyzer
   or emitter executable. The Language 1.0 front door packages the pair for its
   active host only after validating both exact identities.

## Evidence

The current optimized split emitter contains 402 functions and 687,924 code
bytes in an 833,126-byte WVB at SHA-256
`be4a063cafe5b905ea2457e1c3c2ead36af2ecd4f9dd76a8a68a905dbf90a111`.
It packages through the unchanged profile-2 path in five fragments as a
19,005,440-byte Windows x64 application at SHA-256
`e85f4af225255dcf6fc369cd2b794bc6af9459ba996fc6b0f70a79f992c5c842`.
Compared with the 1,268,289-byte complete WVB, reachable emission removes
435,163 bytes, or about 34.3 percent.

The packaged current emitter emits its own 833,126-byte WVB byte-for-byte. It
also emits the direct bounded-generic collection fixture and its monomorphic
oracle to the same 466-byte WVB at SHA-256
`2d59187da5f16a3b275a6bbe96502ce1309f0ba8348e8a22da02097808c8b0c6`.
The small pruning fixture remains 308 bytes in optimized mode and 395 bytes in
complete mode with their previously specified exact digests.
The three-module Foundation generic fixture falls from its historical
3,383-byte complete product to a 3,236-byte optimized product at SHA-256
`78ca3b22958e87b2717c1b94d83205e2d18bc96b9e546192d323f45c8279bc5f`;
the same typed `Option`/`Result` behavior remains reachable.

The bootstrap analyzer contains 390 functions and 783,293 code bytes in a
949,355-byte WVB at SHA-256
`bd8541fc51d87e12265055786df656048510102ced86c6672cabe6ba45bb27cb`.
The bootstrap emitter contains 353 functions and 616,798 code bytes in a
746,557-byte WVB at SHA-256
`a0fe54283ed51e1940bae837eb11bfb2d72f16dd91d7eb7022e51730eb0c5805`.
Their manifest binds source base commit
`49717f4dda27db2235033827ac164fac080ca623`, the single adapter Boolean overlay,
both exact Project 2 identities, and the digest-pinned producer. The current
analyzer still reconstructs normally and packages in eight fragments, but its
hosted execution envelope does not admit the 1.57 MiB current compiler closure.
Ordinary Language 1.0 programs use that current analyzer; only the oversized
self-analysis step uses the bootstrap pair.

On the Windows x64 development host, the revised four-case split owner completed
in under one second. The removed form launched three large source compilations
and reported 30-second heartbeats while each was active. Full current compiler
reconstruction remains in the Language 1.0 front door rather than being run a
second time by this owner.

## Consequences

Ordinary split and cached builds now carry only the functions and data reachable
from the selected exports. This restores native-package headroom for the
remaining Language 1.0 collection work and reduces downstream lowering,
linking, hashing, copying, and cache storage without weakening malformed-input
validation or deterministic output.

Existing version-1 split caches are intentionally cold after this change. They
remain isolated from version-2 families and cannot be mistaken for optimized
products. Tool packagers must publish new role-specific version-2 producer
identities.

This decision does not claim that every library type is pruned, add Project 3
profile inputs, replace final paired-host qualification, or make the split cache
a release boundary. Nominal type pruning and target-aware library closure
refinement remain later optimization work.

## Reconsideration triggers

Reconsider the fixed emitter target when Project 3 profile inputs enter the
split front door, when more than one optimized publication target is required,
or when a persistent compiler service can retain equally exact analysis and
producer generations with lower process-start and hashing cost.
