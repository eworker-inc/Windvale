# Decision 0829: Keep profile-admission SHA-256 native-lowerable

## Status

Accepted on 2026-08-22.

## Context

The Language 1.0 profile-admission product validates the external source-input
lock and composite source profile by SHA-256 before it publishes WVSS. Its
original closure called the portable `Foundationˉsha256ˉhex` implementation.
The WVB 1.18 type checkpoint replaced that call with the
`Bytesˉsha256ˉhex` VM intrinsic and removed the Foundation source module from
the compiler project closures.

The intrinsic is valid portable WVB, but the current native x64 backend does
not lower opcode `7D bytes.sha256_hex`. As a result, the optimized admission
WVB remained executable in the WVB runtime while a freshly reconstructed
`wvadmit` stopped at native staging with `Unsupportedˉcode function=19
detail=1`. Reusing the older packaged admission tool would hide this compiler
product regression.

## Decision

1. Keep profile-admission hashing expressed through the portable
   `Foundationˉsha256ˉhex(bytes) -> bytes` function until every publication
   path that consumes the product lowers the VM intrinsic.
2. Restore `Foundation/Sha256.wv` to every maintained compiler project closure
   that contains `Compilerˉsourceˉprofile` directly or transitively. Do not
   duplicate SHA-256 inside the admission driver or native backend.
3. Keep `Bytesˉsha256ˉhex` in the language and WVB contracts. It remains a
   permitted optimization only for a target that declares and verifies its
   implementation; portable source must not be replaced with it merely because
   the first output is WVB when that WVB is subsequently native-staged.
4. Do not expand native opcode support, image limits, fragment limits, or the
   source-profile format in this correction.

## Evidence

The current split compiler publishes the restored 40-function admission
product as 82,924 WVB bytes at SHA-256
`7a7da249ff51647e2c279a9d06c05897f071683991aca0748ad6f40e02887512`.
Native profile 2 stages and packages it in one fragment as a
797,184-byte Windows x64 application at SHA-256
`8307a87aa7f70cc9519ade98140554db9e5b6de834d39c86149ec8441624b8d6`.
That fresh application admits the two-module Sequence-read fixture, after
which the current analyzer and emitter publish the unchanged 472-byte WVB and
its focused six-case verifier/runtime oracle passes.

## Consequences

The admission WVB retains the bounded portable SHA-256 function and is larger
than the temporarily intrinsic-only product. In return, the same reconstructed
product remains valid both as WVB and as input to the current native packaging
path. A future backend-specific intrinsic substitution should be selected by
an explicit final execution target, not inferred from an intermediate format.

## Reconsideration triggers

Reconsider when the native x64 backend lowers `bytes.sha256_hex`, or when the
compiler carries an explicit final-target contract that can safely choose the
intrinsic for WVB runtimes while retaining the portable source function for
native staging.
