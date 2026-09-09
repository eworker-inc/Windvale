# Construct current-source WVB publishers

## Status

Accepted on 2026-09-09 by maintainer approval. Implementation and Windows/Linux
qualification remain separate gates.

## Decision

Provide a bounded construction path for the maintained WVB publisher source.
Identify its exact source closure, compiler and native producers, module bytes,
and target-specific inputs. Validate the module and native object structure;
resolve required exports and imports by name and kind, and apply only checked
typed relocations. Do not infer a current publisher from a release byte length,
private function number, fixed function offset, or a caller-supplied digest alone.

Reuse the existing native lowering, linker, container, and publication owners.
Keep their input, fragment, allocation, and relative-address bounds explicit.
The current verifier exceeds the old monolithic construction envelope: use the
existing segmented representation rather than silently raising that envelope.
Any changed serialized request needs an explicit version and malformed-input
coverage; existing frozen requests keep their exact meaning.

Reuse the shared native publication-state object where the adapter requires
`Native_publication_begin` and `Native_publication_apply`. As established by
[the pruned staged-publisher bridge decision](0394-Pruned-Staged-Publisher-Bridge-Closure.md),
do not retain dormant private source functions solely to expose package offsets.
The portable transaction implementation remains the semantic/recovery oracle.

Preserve native candidate validation, sibling-file creation, flush, revalidation,
replacement, directory-flush, cleanup, and indeterminate-completion behavior.
Construction failure must not change a previously published destination. Generic
validation-only executables are not publishers and must never be installed as
such. No shell or JavaScript copying fallback may replace the native transaction.

Keep the frozen release artifacts, their pins, and their reconstruction route
unchanged. A current-source development product is not a release or installation
promotion. Reusable build evidence must bind the complete inputs and producers;
a hash beside mutable code proves identity, not independent trust or correctness.

## Rationale and next gate

The current verifier accepts migrated package bytecode that both frozen
publishers reject. Existing current-source verification proves reproducible
native images but does not attach a transactional publisher. Exact evidence is
in the [publisher validation boundary record](../Evidence/2026-09-09-Current-Publisher-Validation-Boundary.json).

The next gate is a current-source publishing executable with checked linkage and
focused success, malformed-input, alias, and destination-preservation evidence on
Windows and Debian. Extend existing verification owners rather than replaying
them through a new test coordinator. Project 4 launcher integration and installed
toolchain qualification follow that gate; neither is established by this decision.
