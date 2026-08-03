# Decision 0169: Public format-3 compiler targets

- Date: 2026-08-03
- Status: Implemented and locally verified on Windows; Linux construction is verified and direct execution still awaits a Linux host report
- Adds: public `windows-x64-console-v3` and `linux-x64-console-v3` compiler targets through the existing atomic artifact publisher
- Retains: ABI 22, exact format-3 bytes, ordinary 4 MiB values, version-1/version-2 application bytes, and source-visible file-output semantics

## Context

Decisions 0164, 0167, and 0168 establish deterministic independently verified Linux and Windows format-3 containers and prove direct canonical Stage 2 reproduction on Windows. Those artifacts were still internal test constructions. A normal user needs one supported `windvale compile` route that selects the exact container contract, verifies it before exposure, gives it the right platform extension and Linux mode, and cannot leave a partially written executable at the requested destination.

The repository already owns a qualified artifact publisher for native console targets. It writes a unique sibling with create-new and write-through behavior, flushes it to durable storage, optionally prepares the Linux executable mode, and only then replaces the destination. Reusing that boundary avoids defining a second publication mechanism or changing the compiler program's existing `file.write_bytes` capability, which remains durable but non-atomic.

## Decision

- Expose format 3 as `Windowsˉconsoleˉapplicationˉcontract.COMPILER_TARGET_NAME` / `windows-x64-console-v3` and `Linuxˉconsoleˉapplicationˉcontract.COMPILER_TARGET_NAME` / `linux-x64-console-v3`. Preserve every existing target name and default artifact byte.
- Add public paired hosted-compiler writer methods. Each accepts the verified native fragment plus its WVB capability declarations, requires exactly one exported `Main`, constructs the named platform service bundle, builds the complete format-3 image, and independently parses the image back against its entry and complete bundle before returning success.
- Route the CLI's verified WVB capability declarations and one native fragment into the selected writer. Default Windows format-3 output to `.exe`, Linux to `.elf`, and prepare Linux format-3 temporary files with executable permissions on a Linux host.
- Publish both format-3 targets through the existing unique-sibling atomic replacement path. Any construction, independent verification, write, flush, permission, or pre-move failure leaves an existing destination unchanged and removes the temporary sibling when possible.
- Keep the exact-compiler test as one compiler construction. Ask each public writer for its first image and compare it with the existing internal builder over the already measured bundle; do not add another CLI compilation or child compiler run merely to test routing.
- Keep atomic publication evidence in the shared publisher case rather than cloning its injected-failure and mode corpus for each target version.

## Local evidence

The focused Release exact-compiler case passes with a zero-warning build. The public writers reproduce the pinned 17,157,120-byte Windows PE at SHA-256 `356bd9c6be1a927017e987728b479d105f9852c0c7aad1b8b9e93202ba64010f` and 17,158,144-byte Linux ELF at `42f3f947cccca8e44c279afce1b6e944682dc440e0e9cda6546883898d951f31`. The same Windows image directly recompiles the twelve canonical sources to the exact 599,868-byte Stage 2 WVB without loading a named CLR or .NET host/runtime module.

The shared atomic-publication case already injects a prepublication failure, proves an existing destination remains byte-identical, proves no sibling remains, then proves complete replacement; on Linux it also verifies exact executable mode. The format-3 CLI routes reach that same code path after their independent verifier succeeds.

## Consequences

The exact compiler containers are normal deterministic CLI artifacts rather than test-only images. Artifact publication is recoverable against interruption before replacement without silently strengthening the authority-bearing file capability seen by Windvale programs.

This is not paired host qualification. Linux still needs direct canonical Stage 2 execution on a real Linux kernel, followed by cross-host comparison of the public executable and reproduced WVB identities. Stage 0 remains the independent construction and recovery oracle until the full native-retirement gate is documented and qualified.

## Reconsider when

- A platform cannot provide same-directory atomic replacement with the required durability or executable-mode preparation.
- Format 3 becomes a general hosted application profile rather than the exact compiler profile.
- A source-visible atomic output capability is specified and deliberately adopted by the compiler.
- The native backend gains a qualified Windows stack-probe contract that permits a smaller initial stack commit.
