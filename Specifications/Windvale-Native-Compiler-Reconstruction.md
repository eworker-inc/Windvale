# Windvale native compiler reconstruction inventory

## Status and scope

This contract owns the retained pre-Language-1.0 compiler reconstruction
inventory. It is historical bootstrap evidence, a small-source differential
oracle, and a compiler-scale WebAssembly workload. It is not the current
compiler, is not reconstructed from the current source tree, and does not
define current language semantics.

The active current compiler and its exact fixed-point proof are specified by
[Windvale native compiler bootstrap and convergence](Windvale-Native-Compiler-Seed-Bootstrap.md).

## Retained inventory

The retained compiler came from the former 649-byte
`Projects/Examples/Windvale-Compiler.wvproj` identity at SHA-256
`a180b171446a6b047b737913ead74fb77a2ecb8d5eedcef833e881dc93ec9b05`.
Its six digest-bound artifacts remain:

| Product | Bytes | SHA-256 |
| --- | ---: | --- |
| canonical WVB 1.11 | 935,163 | `a7d47b2de29faee089c7a22ef23eac4657f719331dc02044eb2d818457dac5b6` |
| Windows x64 compiler | 28,172,800 | `a5db938a814471fdacda75efcf57d28934ae52b3b2290732627c14ba173fd70d` |
| Linux x64 compiler | 28,172,288 | `da11ab3b70b428087cbcb9de5614a2dbdccd31afc6861cc15881fd65c12ff19b` |
| build-driver WVB 1.11 | 1,142,818 | `125d2b4080889615877d843a36b2f9f6b50d049d011cc06fa8ab426ab83c0574` |
| Windows x64 build driver | 30,071,296 | `f556f0e2c794d9424cbcd9f5e3f8e5aee54f49373c7c18ea1d4829facea7dc6f` |
| Linux x64 build driver | 30,072,832 | `628fd60ea702c4a3b3ffb01d32cba7ba9708477acccf190cc6506a56f159d7a9` |

`Artifacts/Native-Compiler-Reconstruction-Candidate/Manifest.json` binds these
identities. They remain distinct from `Artifacts/Native-Compiler-Seed` and the
current split bootstrap products.

## Focused owner

The `compiler-reconstruction` owner has three bounded cases on both hosts:

1. admit the exact six-artifact retained inventory;
2. reject invalid current convergence-wrapper usage; and
3. compare one small program compiled by the retained compiler with the same
   Project 2 program compiled by the current split path.

Both paths must publish the exact 816-byte Function-Only WVB at SHA-256
`28d215b982a7b7185cfa80c4cc5346666bd0181582fe80bec8b7035d514da936`,
compare byte for byte, and pass independent WVB verification. The third case is
a differential smoke, not proof that the retained compiler implements current
Language 1.0.

The accepted `--development` option selects the same bounded cases so changed
verification retains its stable command contract. Cold current compiler
convergence runs once in the separate bootstrap qualification job rather than
being duplicated inside this owner.

## WebAssembly use

The WebAssembly verification gate consumes the retained 935,163-byte WVB
directly as a fixed compiler-scale interpreter workload. Its established step,
allocation, reclamation, and exhaustion observations depend on that exact input.
Rebuilding a different current compiler in the WebAssembly gate would test two
moving boundaries at once and duplicate bootstrap work.

## Retirement boundary

Decision 0876 removes the old monolithic reconstruction launchers. Recreating
this inventory, if ever required for historical investigation, begins from its
recorded source commit or recovery release in a separate workspace. Current
development does not update these six artifacts when language source changes.
