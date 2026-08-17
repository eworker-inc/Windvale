# Workload 1 rejected and boundary cases

## Rule

These cases are mandatory future fixtures. Compile, build, launcher, and runtime
rejections must retain phase, stable identity, relevant source or argument
position, expected/observed state, and applicable maximum without an unbounded
diagnostic cascade.

## Compile-time source rejections

### 1. Calling an undeclared capability

```windvale
profile hosted;
// No standard.output requirement.
standard.output.Write(Value: borrow Output);
```

Reject during capability/effect checking. Importing `Platformˉstream` is not a
grant and cannot create the module-bound root.

### 2. Calling an optional-only capability

```windvale
optional capability standard.output version 1;
standard.output.Write(Value: borrow Output);
```

Reject because optional metadata supplies no callable root.

### 3. Hiding a capability effect

```windvale
fn Write(Value: borrow bytes) -> unit effects() {
    standard.output.Write(Value: Value);
}
```

Reject because the declared empty effect set omits `standard.output`.

### 4. Using result context to select numeric parsing

```windvale
let Maximum: u64 = try Numeric.Parse(
    Value: Argument,
    Maximumˉinputˉbytes: 20u64,
);
```

Reject: edition 1 has no result-context generic destination selection or
overload search. The exact name is `Parseˉu64ˉdecimalˉwhole`.

### 5. Implicit numeric conversion

```windvale
let Maximum: u32 = Parsedˉu64;
```

Reject. Narrowing requires a named checked conversion and this application keeps
the maximum as `u64`.

### 6. Reading a sequence without proving its index

```windvale
let Value = Collections.Sequenceˉat(
    Value: Arguments,
    Index: Count,
);
```

The code type-checks only as a checked access and traps if executed. A future
static range proof may diagnose the always-invalid call earlier. No unchecked
Core or Hosted operation is admitted.

### 7. Reusing a consumed builder

```windvale
let Output = Bytes.Freeze(Builder: Builder);
Bytes.Appendˉbytes(Builder: borrow mut Builder, Value: More);
```

Reject during ownership analysis because `Freeze` consumed `Builder`.

### 8. Retrying indeterminate output

```windvale
case Resource.Mutationˉoutcome.Indeterminate { Error: _ } {
    standard.output.Write(Value: Value);
}
```

Reject under the application/capability conformance owner because the operation
has no idempotency contract and the retry can duplicate externally visible
bytes. General type checking alone need not infer application intent.

### 9. Treating an outcome as Boolean

```windvale
if standard.output.Write(Value: Value) {
    return Types.Processˉstatus.Success;
}
```

Reject because `Mutationˉoutcome<Writeˉfailure>` is not truthy and has no
implicit Boolean conversion.

### 10. Ambient argument or environment access

```windvale
let Mode = Environment.Arguments[0u64];
```

Reject name resolution. Language 1.0 has no ambient prelude, `Environment`
global, implicit process arguments, or environment table.

## Build and launcher rejection

| Case | Required result |
| --- | --- |
| Missing usage-resource binding | Reject package construction before publication or launch. |
| Usage bytes length 74 against maximum 73 | Reject before retaining or mapping content. |
| Usage digest mismatch | Reject before entry or capability binding. |
| Usage bound as `text` rather than declared `bytes` | Reject type mismatch before publication. |
| Unknown or unsupported platform registry key | Reject target admission. |
| Missing, duplicate, stale, or wrong-major capability root | Reject atomic launcher binding; run no source. |
| More than 16 arguments | Reject before constructing the entry argument sequence. |
| One 257-byte argument | Reject before source; do not truncate. |
| Aggregate argument bytes 2,049 | Reject before source; do not drop later arguments. |
| Argument contains malformed UTF-8 | Reject launcher decoding before constructing `text`. |
| Root memory budget below the launcher profile | Reject before capability binding and entry. |

Exactly 16 arguments, exactly 256 bytes in one argument, and exactly 2,048
aggregate bytes are admitted by the launcher, though the application may then
reject their option meaning.

## Argument behavior boundaries

| Arguments | Result |
| --- | --- |
| none | `Noˉarguments`, diagnostic, status 2 if completely accepted |
| `--help` | usage on normal output, no input call, status 0 |
| `--help --operation bytes` | `Helpˉwithˉotherˉarguments`, status 2 after diagnostic |
| `--unknown` | `Unknownˉoption`, status 2 after diagnostic |
| `--operation` | `Missingˉvalue`, status 2 after diagnostic |
| `--operation words` | `Invalidˉoperation`, status 2 after diagnostic |
| `--operation bytes --operation runes` | `Repeatedˉoperation`, status 2 after diagnostic |
| `--operation bytes --maximum-bytes 1 --maximum-bytes 2` | `Repeatedˉmaximum`, status 2 after diagnostic |
| `--maximum-bytes 4` | `Operationˉmissing`, status 2 after diagnostic |
| `--operation bytes --maximum-bytes 0` | admitted; only empty input can succeed |
| `--operation bytes --maximum-bytes 65_536` | numeric rejection because source option text admits digits only |
| `--operation bytes --maximum-bytes +1` | `Invalidˉsign` numeric failure |
| `--operation bytes --maximum-bytes 1 ` | trailing-input numeric failure; no trimming |
| `--operation bytes --maximum-bytes 18446744073709551616` | `Aboveˉmaximum`; no wrapping |
| `--operation bytes --maximum-bytes 65537` | application maximum rejection |

Option order is otherwise irrelevant. Adding a future distinct option cannot
change which declaration any existing call resolves to because the parser uses
fixed names and no overload selection.

## Input boundaries

| Provider observation | Required result |
| --- | --- |
| Empty input with maximum 0 | Success; byte or rune count zero |
| Exactly the configured byte maximum and EOF | Success |
| One byte beyond configured maximum | `Maximumˉexceeded`; no prefix exposed |
| `C3 28` | `Invalidˉutf8` at byte offset 0 |
| Truncated `F0 9F 92` at EOF | `Invalidˉutf8` at byte offset 0 |
| Overlong, surrogate, or out-of-range UTF-8 | `Invalidˉutf8` at first invalid sequence |
| Provider rejects before data | `Rejected`; no text value |
| Provider generation disappears during read | `Providerˉlost`; no text value |

## Output and diagnostic boundaries

For either output capability:

| Outcome | Status | Retry |
| --- | ---: | --- |
| `Rejected` | 4 | Never |
| `Acceptedˉpartial(Completed: N)` | 4 | Never |
| `Completed(Completed: exact length)` | Requested terminal status | Not applicable |
| `Completed(Completed: different length)` | 4 | Never |
| `Indeterminate` | 5 | Never |

A failed normal write does not fall back to diagnostics. A failed diagnostic
does not fall back to normal output. This prevents duplicate or cross-channel
publication after uncertain progress.

## Resource and diagnostic boundaries

- Failure to reserve the diagnostic child attempts one allocation-free static
  diagnostic at most once.
- Failure to reserve input or either output child happens before input and uses
  the already reserved diagnostic child.
- Builder construction failure returns status 6 after at most one diagnostic.
- Append limit failure preserves the builder and then releases it; no truncated
  value is exposed.
- Diagnostic construction failure returns status 6 without recursive reporting.
- Every diagnostic is at most 256 bytes and every path attempts at most one
  diagnostic mutation.
