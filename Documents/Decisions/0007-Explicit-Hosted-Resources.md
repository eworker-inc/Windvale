# Decision 0007: Explicit hosted resources

- Date: 2026-07-29
- Status: Accepted and qualified on Windows and Debian Linux

## Context

The portable `Inspectˉwvbˉenvelope(Input: bytes)` function can validate supplied data, but a useful `wvdump` must receive a filename, read bytes, and route normal and diagnostic output. Passing native process objects, paths, streams, exceptions, or handles directly into Windvale would make Windows and Linux behavior accidental language semantics. Embedding all I/O in the C# CLI would leave the Windvale-written tool unable to control its own workflow.

The existing WVB capability import already expresses primitive signatures and exact module declarations. A new bytecode format or general foreign-function interface is unnecessary for this boundary.

## Decision

- Keep portable parsing as ordinary Windvale functions over immutable values.
- Add explicit hosted capabilities for an ordered argument snapshot, bounded file-byte input, exact standard output, line output, and a separate diagnostic line sink.
- Pass program arguments after the launcher's `--` separator; do not expose the ambient process command line or environment.
- Treat hosted resource names as opaque text. Native adapters own path parsing, current-directory resolution, permissions, and platform error translation.
- Limit arguments to 64 entries, 4 KiB each, and 64 KiB total in strict UTF-8.
- Limit `file.read_bytes` to the existing 4 MiB immutable byte-value bound and require adapters to enforce the bound while reading.
- Require both module declaration and launcher authorization. Preflight host support before executing instructions.
- Validate capability return types and resource sizes again at the runtime boundary.
- Define output line termination as LF and keep diagnostic output separate from normal output.
- Keep WVB at 1.3 because the existing capability section and call instruction encode the complete contract.
- Co-locate the first hosted WvDump shell with its pure envelope functions until Windvale has a real module/package composition mechanism; do not copy the parser into a parallel host implementation.

## Consequences

- The same Windvale workflow can inspect a real file on Windows and Linux while all WVB parsing remains Windvale code.
- Capability grants are intentionally verbose and inspectable. A future package policy may group grants without weakening module-level declarations.
- Native path spelling and permission behavior can differ, but successful byte content and portable inspection results remain host-independent.
- File failures currently trap through stable runtime diagnostics because Seed has no catchable result/error type.
- LF output makes captured program output deterministic instead of inheriting `TextWriter.WriteLine` host conventions.
- The host interface becomes a typed security boundary rather than a trusted callback: unsupported services and malformed results fail before they contaminate runtime state.
- The hosted shell is not yet a complete `wvdump`; it validates the WVB envelope and reports its status but does not decode declaration payloads or disassemble instructions.

## Reconsider when

- A tool needs recoverable file errors, multiple open resources, streaming input, files larger than one immutable byte value, or directory traversal.
- Package manifests need resource scopes narrower than one capability name, such as a selected file or directory grant.
- Standard input, structured diagnostics, or machine-readable output becomes necessary.
- Module composition exists and can separate the hosted WvDump entry point from its portable inspection library without duplication.
