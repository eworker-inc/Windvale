# Decision 0781: Separate the Language 1.0 profile-admission product

- Status: Accepted
- Date: 2026-08-20

## Context

Decision 0779 separated source analysis from WVB emission, but Project 3 and
descriptor-bearing Language 1.0 input still entered through the retained
one-shot compiler. Slice 3 generic Option and Result increased that compiler's
native image beyond the unchanged 32 MiB staging envelope. Raising the envelope
would hide phase ownership, while reproducing source-profile rules in a test
script would create a second admission contract.

The analyzer already occupies all eight allowed native fragments. Adding the
profile parser and SHA-256 admission logic to it would merge unrelated work and
remove the remaining product boundary.

## Decision

1. Move complete multi-module source-profile admission from the WVB emitter
   module into `Compilerˉsourceˉprofile` without changing its status, offset,
   descriptor, lock, hash, profile, or WVSS 2 behavior.
2. Publish `wvadmit` as a small hosted product. It accepts the exact lock path,
   expected lock hash, profile path, ordered source closure, and one WVSS output.
   It publishes nothing unless every source agrees with the admitted edition
   and profile.
3. Let `wvanalyze --admitted-source-set` consume that WVSS 2 value. Keep
   descriptorless Project 2 analysis as its existing direct mode. `wvemit`
   continues to distrust and validate WVSS, WVCA, WVLB, and WVIR before WVB
   publication.
4. Use one bounded coordinator for the three processes. It writes private phase
   values under one exact temporary directory, publishes the WVB only after all
   phases succeed, and removes that directory in `finally` after success or
   failure.
5. Retain the complete current compiler WVB rebuild as self-hosting evidence.
   Do not package its oversized monolithic native image and do not increase any
   object, image, fragment, verifier, runtime, or diagnostic limit.

## Evidence

The admission product contains 40 functions, 61,760 code bytes, and 82,781 WVB
bytes with SHA-256
`7ff3fcb5e4d07ebcc3abe325a501cdfc09983990f9d600b228b71abb040037d8`.
It packages as a 797,184-byte Windows x64 executable in one fragment. The
analyzer remains 976,748 WVB bytes / eight fragments. The emitter is 368
functions and 775,522 WVB bytes / five fragments. No existing bound changed.

The complete 524-function compiler rebuild is 1,165,567 WVB bytes. The focused
Language 1.0 owner then uses the current three-stage path for all compilation
cases and passes 146 cases. Its generic fixture admits 3,169 source bytes,
publishes 104 WVCA bytes, 900 WVLB bytes and 5,792 WVIR bytes, emits the same
3,383-byte WVB twice, rejects four malformed inputs without a WVB, passes the
compiler-aligned verifier, and executes to `Result: 42`.

## Consequences

Language 1.0 profile admission remains Windvale-authored and independently
packageable. Analyzer and emitter capacity stays bounded, and the focused gate
no longer needs a monolithic current native compiler merely to test language
semantics. Three cold process starts cost more for tiny inputs; later
profile-aware caching or a retained compiler service may remove that overhead
without merging semantic phases.

## Non-decision

This decision does not cache Project 3 inputs, remove the one-shot WVB oracle,
change source-profile formats, change generic semantics, qualify Linux, or run
storage and OS qualification per slice.

## Reconsideration triggers

Reconsider the process boundary when representative profile-aware projects have
an exact cache key or when a retained compiler service can preserve the same
phase validation with lower startup cost. Reconsider product geometry before
any product reaches its existing fragment ceiling.
