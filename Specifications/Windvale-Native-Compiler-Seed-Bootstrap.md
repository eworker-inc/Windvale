# Windvale native compiler bootstrap and convergence

## Status and scope

This document defines the active no-.NET bootstrap proof for the current
Windvale compiler. The current compiler is one pipeline implemented by two
separately packaged products:

- the analyzer parses, validates, and publishes canonical source-analysis
  evidence and Windvale IR; and
- the emitter consumes that admitted evidence and publishes canonical WVB.

The split is an implementation and resource boundary, not two source
languages, two semantic definitions, or two competing compilers. The checked-in
native Seed remains an immutable recovery and differential oracle. Current
language work does not modify Seed merely to make ordinary development advance.

## Bootstrap inputs

The active bootstrap begins with three digest-bound portable WVB products under
`Artifacts/Language-1.0-Target-Aware-Emission-Bootstrap/Wvb/`:

| Product | Bytes | SHA-256 |
| --- | ---: | --- |
| bootstrap analyzer | 992,412 | `26ea9bccfe8c2763fb887a5a14c2f0a086a27265523c3df84187b361616f9120` |
| bootstrap emitter | 895,787 | `ea8ade4774236a84208242a6e17d271077b9a4a94fb40c47ec487d43a97b2b94` |
| WVIR 1.9 bridge emitter | 1,146,083 | `0d838b6d983320cf22b9094ef5a4692d6833f1834292863789577e034f6febdb` |

These products are packaged for the current host through the pinned segmented
compiler-container path. Their producer identities include role, target, host,
byte length, and SHA-256. The identities are inputs to the content-addressed
project cache; they are not inferred from filenames or ambient tools.

`Artifacts/Native-Compiler-Seed` remains the last promoted semantic-freeze Seed
and `stage0-recovery-e5a1a7473c57` remains the managed recovery release. Neither
is silently repinned to the current source tree.

## Exact current products

The fixed-point compiler products are:

| Product | Functions | Bytes | SHA-256 |
| --- | ---: | ---: | --- |
| current analyzer | 758 | 1,515,372 | `9876f178f4ac06872a44f44085de5d72f17777abf462985300f6e453e4b625d9` |
| current emitter | 738 | 1,523,605 | `a0beb624dcc225b0ccdac848d808af1faef63cdb66eb650faf0bb9216e0815c9` |
| current compiler-aligned verifier | 111 | 399,387 | `7da624b070b69c3a720a00df12b753ed28276b7909c48ec5e6c349bd15ed9800` |

The WVB identities are host-independent. PE and ELF container identities may
differ because their startup and platform-service materializations are explicit
host products.

## Convergence procedure

`Tools/Native/Verify-Compiler-Convergence.cmd` and `.sh` are the public native
entry points. They accept no arguments and invoke the bounded coordinator
`Verify-Current-Split-Compiler-Convergence.mjs` over the current checkout.

The coordinator performs these steps in one private temporary directory:

1. admit the three exact bootstrap WVB products;
2. create a private empty native cache;
3. package the bootstrap analyzer, emitter, and bridge for the current host;
4. publish exact producer identities for those three applications;
5. build the current analyzer Stage 1 with the bootstrap analyzer and emitter;
6. package the Stage 1 analyzer and publish its producer identity;
7. build the current emitter Stage 1 with the current analyzer and bridge
   emitter;
8. package the Stage 1 emitter and publish its producer identity;
9. rebuild both compiler halves with that current analyzer/emitter pair;
10. build and package the compiler-aligned verifier from the same current pair;
11. verify both Stage 2 WVB products with that current verifier; and
12. require exact Stage 1/Stage 2 byte equality for both compiler halves.

Success reports two converged compiler products and their exact identities. A
generation, verification, identity, timeout, diagnostic-bound, or publication
failure returns nonzero.

Node.js coordinates processes, validates bounded files, emits progress, and
performs exact byte comparisons. It does not parse Windvale source, construct
semantic models, emit WVB, or define compiler behavior. All semantic work is
performed by the Windvale-built analyzer, emitter, and verifier.

## Bounds and cleanup

Every child has a 15-minute timeout and emits a progress line at least every 30
seconds. Combined child diagnostics are limited to 1 MiB, and admitted products
are limited to 16 MiB. The private cache makes the qualification result
independent of prior development state.

The coordinator removes only the exact process-created directory whose resolved
parent is the operating-system temporary directory and whose basename has the
owned convergence prefix. It removes that directory on success or failure.

The cold proof intentionally rebuilds native host containers because it is a
bootstrap qualification boundary. Ordinary development uses the split
content-addressed cache and focused owners instead. A future optimization may
reuse independently admitted fixed runtime/service container segments, but must
not skip current compiler generation, verification, or exact comparison.

## Retired monolithic route

The former `Bootstrap-Compiler`, `Compile-Compiler-Source-Set`, and
`Construct-Compiler-Reconstruction` launchers pinned a 649-byte monolithic
project manifest. That manifest no longer represents the current compiler and
the route could neither consume the current 1,712-byte project nor express the
analyzer/emitter ownership boundary. Decision 0876 retires those launchers from
`main` rather than maintaining an obsolete second compiler path.

The retained 935,163-byte compiler candidate remains useful historical,
differential, and WebAssembly stress evidence. It is not the current compiler
identity and is not rebuilt from the current source tree.

## Bounded Seed products

`Build-Source-Compiler-Product.cmd` and `.sh` retain only the exact `core` and
`demo` Seed products. The former `tool` selector used the obsolete monolithic
compiler project as an identity sentinel and is retired. Current compiler
products are built through the split Project 2 path and proved by convergence.

## Qualification

`Tools/Verify/Verify-Bootstrap.cmd` and `.sh` run this cold convergence proof.
Cross-host convergence is claimed only when the same source commit passes on
both permanent hosts. A local pass establishes current-host evidence only and
does not promote Seed or replace the recovery release.
