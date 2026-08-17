# Workload 1 implementation responsibilities

## Rule

This matrix assigns every paper dependency to one repository owner. It prevents
ordinary command convenience from becoming ambient language behavior and
prevents missing Foundation operations from being hidden inside a launcher or
provider. No row authorizes implementation before Language 1.0 source freeze.

## Responsibility matrix

| Boundary | Existing owner | Work proven necessary by this bundle | Core source change? |
| --- | --- | --- | --- |
| Edition/module/profile/platform/authority/capability parsing | Language grammar and compiler front end | Parse five modules and preserve the exact three-capability closure. | No new syntax. |
| Arguments as immutable ordinary entry data | Package/launcher profile, Foundation collections | Construct and bind one bounded `Sequence<text>` excluding executable identity. | No. Decision 0754 already assigns ordinary arguments to launcher metadata. |
| Exact sequence length and access | Foundation collections, type checker, borrow checker | Publish generic `Sequenceˉlength` and checked borrowed `Sequenceˉat`; solve `T` from the explicit sequence argument. | No new syntax; exact Foundation signatures are accepted. |
| Strict decimal `u64` parsing | Foundation numeric, runtimes/backends | Implement whole-input ASCII decimal parsing with exact failure offsets, byte maximum, and overflow behavior. | No; the exact Foundation signature is accepted. |
| Option, Result, `try`, records, enums, variants, and value match | Language semantic owner and compiler | Type the parser and preserve exact failure propagation without exceptions or overload selection. | No. |
| Text byte/rune observations | Foundation text, runtimes/backends | Return canonical UTF-8 byte length and Unicode scalar count identically on all targets. | No; exact Foundation names are accepted. |
| Reserved text/byte builders | Foundation memory/bytes/text, ownership lowering | Commit maxima at construction; append atomically; consume on freeze; release on every early return. | No new grammar; exact Foundation signatures are accepted. |
| Package data | Parser, source graph, package/build plan | Bind one 73-byte object by type, maximum, length, and digest with no filesystem grant. | No. |
| Package/WVB representation | Package formats, WVB owner, loader, publisher | Store one typed content reference and one distinct payload object; charge retained bytes once. | No source change; likely a versioned content-reference table. |
| Standard input | Hosted capability catalog and provider adapters | Read one bounded strict UTF-8 value into a caller budget and preserve limit, invalid UTF-8, rejection, and loss. | No; capability/library contract only. |
| Standard output and diagnostic output | Hosted capability catalog and provider adapters | Preserve rejected, exact partial, complete, inconsistent, and indeterminate acceptance without retry. | No; capability/library contract only. |
| Process status mapping | Command launcher profile | Bind the enum result and map its six members to exact process statuses. | No special `Main` or process intrinsic. |
| Memory accounting | Launcher/runtime resource domain | Admit 98,304 bytes, charge arguments/package/provider state, and enforce child splits and teardown reserve. | No. |
| Diagnostics | Compiler, launcher, input/output providers | Retain phase, stable identity, argument/source position, expected/observed state, and applicable maximum within bounded records. | No. |
| Editor tooling | Windvale editor/formatter | Classify edition-1 headers, package data, named calls, variants, borrows, builders, effects, and capability roots. | Uses candidate grammar. |
| Verification | Focused Language 1.0 command owners | Turn valid, rejected, limit, UTF-8, mutation, cleanup, deterministic, and malformed cases into bounded fixtures. | No. |

## Accepted Foundation signatures

The complete exact set selected by this source is recorded in
[Command-Contract.md](Command-Contract.md). The four source-freeze groups are:

1. `Sequenceˉlength<T>` and borrowed checked `Sequenceˉat<T>`;
2. `Parseˉu64ˉdecimalˉwhole`;
3. `Byteˉlength` and `Runeˉcount` for text; and
4. reserved text/byte builder construction, atomic append, UTF-8 append, decimal
   append, and consuming freeze.

These are normative-candidate additions to the Foundation companion under
[Decision 0755](../../../Decisions/0755-Resolve-Language-1.0-Command-Workload-Findings.md),
not a second command library. A later mandatory workload may revise one only through a named
reconsideration that updates all paper source coherently.

## Representation planning

### Front end and WIR

The host source lowers through ordinary typed WIR: immutable/shared values,
records, enums, variants, exact generic calls, checked loops and arithmetic,
borrows, owned budgets/builders, cleanup edges, capability calls, and closed
matches. No command-specific WIR instruction is required.

### Launcher arguments

The launcher may receive native arguments differently on Windows, Linux, and
Windvale OS, but it must normalize them into the same strict bounded semantic
sequence before source begins. Native pointer arrays, quoting rules, executable
paths, and environment blocks do not enter WIR or WVB.

### Input and output

Capability calls retain exact signature-set, limit-profile, progress, and
provider-generation identities. Host adapters translate platform results to the
closed semantic values. A native write return code cannot collapse partial and
indeterminate progress into one Boolean.

### Builders

An implementation may use inline or heap-backed storage, but reserved
construction must prove the full maximum before append. Freeze may reuse backing
or copy only when accounting, ownership, bytes, and failure behavior remain
identical.

## Planned verification owners

| Owner | Initial cases |
| --- | --- |
| `language-1-paper-command` | Candidate parse/name/type/effect/ownership cases and stable paper identities. |
| `language-1-foundation-command` | Sequence access, strict numeric parsing, text counts, reserved builders, limits, freeze, and cleanup. |
| `package-data` | Usage binding, type, maximum, length, digest, missing/duplicate content, and retained-byte accounting. |
| `hosted-standard-streams` | Strict bounded input plus output/diagnostic progress, loss, malformed provider values, and no retry. |
| `command-launcher` | Argument maxima, exact entry binding, root budget, capability closure, target selection, and status translation. |
| `command-differential` | Identical output/status/capability transcript across every admitted backend and host. |

These are planning labels, not current registry additions. Implementation adds a
focused owner only when its executable boundary exists; paper-document changes
must not trigger an unfiltered native qualification gate.

## Implementation order after source freeze

1. Freeze the general Language 1.0 and Foundation clarifications exposed by all
   eleven workloads.
2. Implement the ordinary Core values used by argument parsing.
3. Implement sequence access, strict numeric parsing, text observations, and
   reserved builders with simple reference oracles.
4. Implement package-data references and the command launcher argument/budget
   binding.
5. Implement software standard-input/output/diagnostic providers on Windows and
   Linux with the exact progress model.
6. Compile and execute this application through the shared interpreter and
   native backend, then add cross-host differential evidence.
7. Bind the same semantic providers in Windvale OS without changing source.

Shell parsing, environment snapshots, filesystem redirection, pipelines,
interactive terminals, asynchronous streams, and locale formatting remain
separate later contracts.
