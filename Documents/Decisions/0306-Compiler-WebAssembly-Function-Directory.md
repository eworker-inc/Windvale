# Decision 0306: Compiler WebAssembly function directory

- Date: 2026-08-06
- Status: Implemented with focused Windows-local reference evidence
- Advances: [Decision 0298](0298-Compiler-Scale-WebAssembly-Code-Inventory.md)
- Target: `wasm32-browser-v1-experimental`

## Context

Decision 0298 proves that the direct WebAssembly backend can walk all 417
functions and 157,844 instructions in the exact portable compiler. Its
inventory is intentionally part of the established lowering core and answers
whether an input is within that backend's bounds. The next executable slices
need a different object: immutable, constant-time function metadata that can be
reused at each of the compiler's 2,991 direct calls without rescanning the
variable-width function section.

The established core also carries mature small-profile emitters. A reusable
compiler execution representation should develop independently until it can
replace a complete path; a partial compiler emitter must not perturb or be
published through those existing selectors.

## Decision

- Add a focused portable `WebAssembly-Function-Directory.wv` module rather
  than adding another representation to `WebAssembly-Core.wv`.
- Admit one through 512 functions, at most 64 parameters, 8,191 locals,
  131,072 code bytes per function, one MiB of aggregate code, declared stack
  depth one through 256, and bounded valid WVB shapes and names.
- Encode exactly one 32-byte little-endian entry per function. Its eight `u32`
  fields are parameter count, parameter-shape offset, result-shape offset,
  local count, local-shape offset, code offset, code length, and maximum stack.
- Require function code extents to be contiguous and to consume the complete
  code section. A function index then resolves to its metadata with one bounded
  multiply and fixed-offset reads.
- Retain a complete opcode-width scan beside the directory. It validates local
  indices, function-relative control extents, Boolean constants, and direct-call
  target indices while recording exact aggregate instruction and call counts.
- Add a dedicated hosted directory tool and project manifest. The tool reads
  one WVB, emits only the raw directory, and reports exact inventory evidence.
  It is development evidence, not a browser or server compilation service.
- Do not yet claim typed operand-stack validation, call parameter/result
  agreement, instruction-boundary control proof, executable WebAssembly, or a
  browser performance improvement.

## Exact evidence

The focused retained test composes the directory tool independently, verifies
its pinned WVB digest, and runs it twice over the exact portable compiler. Both
runs produce the same 13,344-byte directory for 417 functions and report
157,844 instructions plus 2,991 direct calls.

A generated 512-function boundary produces the exact 16,384-byte directory,
2,051 instructions, and one call. A 513-function module, hostile name extent,
invalid shape, inconsistent code range, and declared stack depth 257 fail the
function-inventory boundary. An unknown opcode and an out-of-range call fail
the call-inventory boundary without publishing output. The focused test passed
in 21.042 seconds of execution on the measured Windows host.

`WebAssembly-Core.wv`, the pinned browser backend, the static website, and the
normal browser worker are unchanged by this slice. The existing exact backend
still stops without output at its retained executable selector.

## Consequences

Compiler-scale call validation and eventual emission can now resolve any
target's signature and code range in constant time. The directory is immutable
evidence derived from the already bounded WVB sections, so later phases do not
need hidden mutation or repeated section scans.

This slice does not reduce the current 1.4-billion-operation interpreted
compiler run. The immediate next gate is typed operand-stack simulation and
exact parameter/result agreement for every direct call, including rejection of
an in-range target whose signature is incompatible.

## Reconsider when

- the serialized function declaration or WVB shape encoding changes;
- the exact compiler exceeds a retained directory bound;
- indirect function values require a separate typed table;
- a compact typed executable representation needs additional immutable fields;
  or
- a complete direct compiler Wasm supersedes the development directory tool.
