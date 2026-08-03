# Decision 0164: First exact-compiler Linux executable container

- Date: 2026-08-03
- Status: Implemented with focused local Windows construction evidence; Linux execution and cross-host qualification pending
- Adds: canonical WVA startup and deterministic independently verified format-3 Linux ELF candidate
- Retains: ABI 22, the exact compiler and service bytes, the bounded runtime-data plan, ordinary 4 MiB behavior, and all existing application bytes

## Context

Decisions 0161 and 0163 fix the exact compiler's service image, application manifest, and bounded initial runtime state. The next uncertainty is the real loader boundary: whether those contracts can inhabit one import-free executable without silently inheriting .NET, libc, a dynamic linker, executable data, or host-defined argument semantics.

The container must remain inspectable before it is executable. A writer alone could produce internally consistent but incorrectly bound bytes, while a second exact-compiler test would unnecessarily repeat the expensive 17 MiB native compilation. The narrow milestone therefore extends the existing exact-compiler AOT case with a separately assembled startup oracle and an independent final-image parser.

## Decision

- Define `Linux-X64-Hosted-Compiler.wva` as the canonical x86-64 startup source. Its one 765-byte code section has 25 imports and 31 typed relative-i32 relocations. The retained unpatched template is SHA-256 `8302bbd7a4c89f70e8dd24a69dd345fc8273995cb59d09cd8c85ee3bf61c3c33`; the encoded WVO is SHA-256 `0df0525b35bbeb63492929d974326f328c247ce9313111ee6a8c1e321a2c22ff`.
- Preserve the kernel's initial argument stack, reserve a private 64 MiB RW stack through the Linux `mmap` syscall, bind all fixed RW/NX runtime regions and exactly ten service leaves, and never introduce a dynamic import.
- Exclude `argv[0]`, admit at most 67 arguments, scan at most 4,096 bytes per argument, admit at most 65,536 aggregate bytes, and call the exact UTF-8 service before publishing immutable descriptors. Any mapping, limit, UTF-8, native-status, or nonportable-result failure exits with status one.
- Emit one sectionless x86-64 `ET_DYN` image with five program headers: a read-only header page, one read/execute text load, one read/write non-executable data load, the Windvale format-3 note, and a 64 MiB RW/NX GNU stack declaration. The image has no interpreter, dynamic table, imports, or loader relocations.
- Place startup at virtual/file address 4,096, the exact compiler/service bundle at relative text offset 4,096, and the 4,096-byte runtime header at the next page. Extend that data load to the fixed 406,929,408-byte demand-paged runtime extent.
- Keep construction and verification separate. The verifier parses every ELF and program-header field, the Windvale note, all reserved zeros and file boundaries, every resolved startup target, exact bundle bytes, exact runtime metadata, truncation, and trailing data. The separately assembled WVA object must reproduce the instantiated startup bytes exactly.
- Extend the existing exact-compiler AOT transport test so the compiler, native fragment, and platform bundle are each built once. Reuse its malformed-input loop for header, note, startup, padding, bundle, runtime, truncation, and trailing-byte mutations.

## Local evidence

The focused Release test passes with zero warnings on Windows. Two independent constructions produce identical 17,158,144-byte ELF files with SHA-256 `42f3f947cccca8e44c279afce1b6e944682dc440e0e9cda6546883898d951f31`.

| Region | File placement/bytes | Virtual bytes |
| --- | ---: | ---: |
| Header | 0 / 4,096 | 4,096 |
| Text | 4,096 / 17,147,447 | 17,147,447 |
| Runtime data | 17,154,048 / 4,096 | 406,929,408 |
| Complete image | 17,158,144 bytes | 424,083,456 |

The verifier recovers the exact native entry and complete service bundle, and all eight malformed classes fail closed. The roughly 404 MiB gap between data file size and data virtual size is zero-initialized, demand-paged RW/NX capacity governed by Decision 0163 rather than bytes stored in the executable.

This is construction and parser evidence produced on Windows. It does not claim that the Linux kernel has loaded the file, that the compiler has run outside .NET, or that canonical Stage 2 has been reproduced.

## Consequences

The exact compiler now crosses the Linux executable-format boundary as deterministic bytes. The startup's machine code is owned as WVA, while the bootstrap C# encoder remains a recovery implementation whose result is checked against that source. No existing console target or ordinary admission limit changes.

The next Linux milestone is operational: publish this candidate atomically with executable permissions, run it on the qualified Debian host against a small source and then the canonical compiler source set, and compare produced WVB bytes. Windows still needs its equivalent format-3 startup, PE imports/relocations, container writer, and independent verifier before paired direct reproduction can begin.

## Reconsider when

- Linux rejects the sectionless static PIE layout or reserves the RW/NX extent differently from the verified contract.
- Live arguments, file snapshots, output publication, or service calls expose a startup ABI mismatch.
- A smaller compatible runtime allocation plan replaces the fixed snapshot strides.
- Windows construction reveals a contract that should be shared rather than host-specific.
