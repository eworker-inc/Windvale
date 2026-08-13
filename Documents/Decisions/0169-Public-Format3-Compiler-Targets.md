# Decision 0169: Public format-3 compiler targets

- Date: 2026-08-03
- Status: Cross-host qualified at exact commit `57d154c` in GitHub Verify run 30819768981
- Adds: public `windows-x64-console-v3` and `linux-x64-console-v3` compiler targets through the existing atomic artifact publisher
- Retains: ABI 22, exact format-3 bytes, ordinary 4 MiB values, version-1/version-2 application bytes, and source-visible file-output semantics

## Context

Decisions 0164, 0167, and 0168 establish deterministic independently verified Linux and Windows format-3 containers and prove direct canonical Stage 2 reproduction on both permanent hosts. Those artifacts were still internal test constructions. A normal user needs one supported CLI route that selects the exact container contract, verifies it before exposure, gives it the right platform extension and Linux mode, and cannot leave a partially written executable at the requested destination.

The repository already owns a qualified artifact publisher for native console targets. It writes a unique sibling with create-new and write-through behavior, flushes it to durable storage, optionally prepares the Linux executable mode, and only then replaces the destination. Reusing that boundary avoids defining a second publication mechanism or changing the compiler program's existing `file.write_bytes` capability, which remains durable but non-atomic.

## Decision

- Expose format 3 as `Windowsˉconsoleˉapplicationˉcontract.COMPILER_TARGET_NAME` / `windows-x64-console-v3` and `Linuxˉconsoleˉapplicationˉcontract.COMPILER_TARGET_NAME` / `linux-x64-console-v3`. Preserve every existing target name and default artifact byte.
- Add public paired hosted-compiler writer methods. Each accepts the verified native fragment plus its WVB capability declarations, requires exactly one exported `Main`, constructs the named platform service bundle, builds the complete format-3 image, and independently parses the image back against its entry and complete bundle before returning success.
- Route the CLI's verified WVB capability declarations and one native fragment into the selected writer. Source `compile` accepts the target names directly; `aot` accepts an already verified WVB so project-manifest recovery can build the canonical source inventory once and package that exact module for both hosts. Default Windows format-3 output to `.exe`, Linux to `.elf`, and prepare Linux format-3 temporary files with executable permissions on a Linux host.
- Publish both format-3 targets through the existing unique-sibling atomic replacement path. Any construction, independent verification, write, flush, permission, or pre-move failure leaves an existing destination unchanged and removes the temporary sibling when possible.
- Keep the exact-compiler test as one compiler construction. Ask each public writer for its first image and compare it with the existing internal builder over the already measured bundle; do not add another CLI compilation or child compiler run merely to test routing.
- Keep atomic publication evidence in the shared publisher case rather than cloning its injected-failure and mode corpus for each target version.

## Local evidence

The focused Release exact-compiler case passes with a zero-warning build. The public writers reproduce the pinned 17,157,120-byte Windows PE at SHA-256 `356bd9c6be1a927017e987728b479d105f9852c0c7aad1b8b9e93202ba64010f` and 17,158,144-byte Linux ELF at `42f3f947cccca8e44c279afce1b6e944682dc440e0e9cda6546883898d951f31`. The same Windows image directly recompiles the twelve canonical sources to the exact 599,868-byte Stage 2 WVB without loading a named CLR or .NET host/runtime module.

The shared atomic-publication case already injects a prepublication failure, proves an existing destination remains byte-identical, proves no sibling remains, then proves complete replacement; on Linux it also verifies exact executable mode. The format-3 CLI routes reach that same code path after their independent verifier succeeds.

The clean project-manifest recovery route is also executed locally against `Projects/Examples/Windvale-Compiler.wvproj`: `build` emits the canonical 599,868-byte WVB once, then paired `aot` commands consume that exact verified module and reproduce the 17,157,120-byte PE and 17,158,144-byte ELF at their pinned SHA-256 values. Target extension selection is shared between default naming and final validation and covered for all seven source targets, closing an initial split in which Windows format 3 defaulted to `.exe` but the later validator treated it as `.elf`.

## Cross-host qualification

Exact commit `57d154c1f6758315692e35a47939d51702d5c96b` passes GitHub [Verify run 30819768981](https://github.com/eworker-inc/Windvale/actions/runs/30819768981) from the isolated `codex/compiler-package-qualification` evidence ref. Windows and digest-pinned Debian 12 each complete a zero-warning Release build, all 87 Seed tests, all 38 OS tests, the golden compiler contract, and the native CLI gate; the verification gate succeeds.

The shared AOT/publication case passes in 137 ms on Windows and 63 ms on Debian. It packages a separately verified WVB through the public `aot` command, checks its exact native container, rejects an invalid extension without output, then proves injected prepublication failure, sibling cleanup, complete replacement, and Linux mode. The exact-compiler case passes in 20.194 and 16.878 seconds, respectively. Both hosts reproduce every pinned native, WVO, link, service, metadata, runtime, PE, ELF, and canonical Stage 2 WVB identity unchanged; each current-host raw compiler again runs without a CLR/.NET host or runtime mapping.

## Consequences

The exact compiler containers are normal deterministic CLI artifacts rather than test-only images. Artifact publication is recoverable against interruption before replacement without silently strengthening the authority-bearing file capability seen by Windvale programs.

The underlying containers, public writers, `compile`/`aot` routing, atomic publication boundary, and direct Stage 2 behavior are paired-host qualified. Stage 0 remains the independent construction and recovery oracle until the much broader native-retirement gate is documented and qualified; this decision does not imply that the other native tools or normal automation are .NET-free.

## Reconsider when

- A platform cannot provide same-directory atomic replacement with the required durability or executable-mode preparation.
- Format 3 becomes a general hosted application profile rather than the exact compiler profile.
- A source-visible atomic output capability is specified and deliberately adopted by the compiler.
- The native backend gains a qualified Windows stack-probe contract that permits a smaller initial stack commit.
