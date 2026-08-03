# Decision 0120: Bounded WebAssembly call graph

- Date: 2026-08-02
- Status: Implemented with local Windows, Node.js, and Chromium evidence; cross-host and cross-browser qualification pending
- Extends: [Decision 0116](0116-Sequential-WebAssembly-Control-Regions.md)
- Target: `wasm32-browser-v1-experimental`

## Context

Profile 5 proves direct structured WebAssembly for several sequential loops and conditionals in one function. Useful Windvale programs also need source functions with parameters, private reusable computation, and nested dynamic calls. Adding unrestricted calls would admit recursion and unbounded target stack growth without a call-depth resource contract. Combining calls and control flow immediately would also multiply the independent proof obligations before either boundary was qualified alone.

Canonical WVB already stores functions in a stable order, records their exact signatures and code ranges, and encodes direct calls by function ordinal. A strictly decreasing call ordinal provides a small acyclic profile whose maximum dynamic depth is statically bounded by the function-count limit.

## Decision

- Add experimental profile 6 for one portable module containing two through eight `i32`-returning functions and one final exported `Main() -> i32`.
- Admit zero through two `i32` parameters per function and up to 256 combined parameters and locals. Admit only profile-3 scalar instructions plus direct `call`; control-flow instructions remain outside this profile.
- Require every call target to have a lower canonical function ordinal than its caller and require the operand stack to supply exactly the target's declared arguments. Reject forward calls, self-calls, cycles, recursion, and invalid arity before publication.
- Retain the 16,384-byte and 4,096-instruction per-function limits, add 32,768-byte and 8,192-instruction aggregate limits, require stack depth one or two, and require at least one call.
- Emit one exported ABI-2 wrapper followed by one private WebAssembly function for each WVB function. Lower WVB calls to real direct WebAssembly `call` instructions.
- Share one instruction count, caller-supplied instruction limit, and status across every generated function. Charge each dynamic WVB instruction before execution. Propagate `WVR3007` and `WVR3011` through callers as status values, never WebAssembly engine traps, and publish `Main`'s result only on status zero.
- Advance the .NET-free static page to the retained three-function artifact and require exact success at budget 66 and `WVR3011` exhaustion at budget 65.

## Consequences

The Windvale-authored backend now lowers ordinary direct source calls with parameters and shared instruction accounting. The retained program exercises a depth-three path and repeated calls while the browser-facing execution ABI and its four exports stay unchanged.

The decreasing-ordinal rule is deliberately narrower than the source language. It makes the call graph acyclic by construction and caps dynamic depth at eight, but it rejects otherwise valid forward references and all recursion. Profile 6 does not yet compose calls with loops or conditionals. The Stage 0 compiler, mandatory WVB verifier, and hosted execution of the `.wv` lowerer remain part of artifact production.

## Local evidence

The retained source defines `Add(i32, i32)`, `Double(i32)`, and exported `Main()`. It compiles to 399-byte WVB SHA-256 `502f5e9394248db4e21b49a3a98173917c2ff6f9a8252bef606a7a6c845d6482`. The Windvale-authored selector emits a deterministic 1,185-byte import-free Wasm module with SHA-256 `d92667752762a992bdb626e34b83b78ee9c531f167b911737dfbf5f6443f3518`.

The reference runtime and Node.js agree on `0/42/66` at budget 66 and `3011/0/65` at budget 65; a repeated success run resets and reproduces the same tuple. A separate two-function fixture overflows inside `Calculate`: its 301-byte WVB SHA-256 is `9e2b2a747287ff49ffce4d34f888b557a48064062e75ff5147bfc0224b54dca2`, its 737-byte Wasm SHA-256 is `4e936e5c4b077d1bce8719f5cc5c974961088f1171ed00158f9ac251f7652bd7`, and both engines report `3007/0/14` under budget 100 across repeated runs. The independent C# decoder reconstructs every target type, function declaration, five-global layout, four exports, wrapper instruction, private function body, meter, scalar operation, local mapping, direct call target, post-call status branch, and return from verified WVB. Zero-, one-, and two-parameter calls are accepted. An eight-function chain is accepted and reconstructed; nine functions, three parameters, a renamed final function, forward or self-directed targets, and arity mismatches fail as `Unsupportedˉcode` with no output publication.

`Tools/Verify/Verify-WebAssembly.ps1` retains all earlier artifact identities and executes all thirteen artifacts under Node.js 24. The standalone-page verifier reconstructs and executes the embedded profile-6 artifact without a .NET asset. A Chromium-based in-app browser loads the updated ordinary HTML page and reports the exact SHA-256, ABI `2`, tuples `0/42/66` and `3011/0/65`, and zero .NET/Blazor resource requests. This is local Windows and one-browser-engine evidence, not cross-host or cross-browser qualification.

## Rejected alternatives

Unrestricted call targets were rejected because self-calls and cycles would need an explicit dynamic depth contract and additional contained-exhaustion evidence.

Inlining callees was rejected because it would avoid proving the target call ABI, duplicate generated code, and hide the program's real call structure.

Combining calls and structured control in the same profile was deferred so call signatures, target ordering, shared metering, and failure propagation could be reconstructed and tested independently first.

Treating compiler output as trusted was rejected because canonical WVB remains an untrusted serialized input at the backend boundary.

## Reconsider when

- Calls and profile-5 control regions can share one selector with independent reconstruction of both call and branch structure.
- A dynamic call-depth counter can admit recursion or less restrictive call ordering without relying only on worker termination.
- More than two scalar parameters or additional scalar families are required by a representative portable program.
- A Windvale-native verifier or browser compiler can replace another Stage 0 artifact-production component.
