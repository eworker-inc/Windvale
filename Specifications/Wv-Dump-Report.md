# Windvale `wvdump` line report

## Purpose

This document defines the deterministic report emitted by `Wv-Dump-Core.wv` for a structurally valid WVB 1.5 module. The format is intentionally line-oriented, ASCII-only, stable across Windows and Linux, and simple enough for both humans and early Windvale tools. It is a Seed development contract, not yet a long-term public interchange format.

## Encoding and safety

- Output uses LF line endings.
- Every line contains one record. Fields are separated by one ASCII space and appear in the order defined here.
- Decimal integers have no sign unless the field is an `i32`, no grouping, and no leading padding.
- Declaration names use `Textˉquote`. The result includes double quotes, escapes quote, reverse solidus, and controls, preserves printable ASCII, and represents every non-ASCII UTF-16 code unit as uppercase `\uXXXX`.
- No untrusted module text is written outside this quoted form. A module cannot inject a control sequence or a second report line through its metadata.
- The implementation validates the complete module before emitting the first standard-output line.

## Record order

The report begins with:

```text
wvdump 1
module version=1.5 profile=<portable|hosted|system> name=<quoted-name>
```

It then emits seven section records in physical WVB order:

```text
section name=<kind> offset=<payload-offset> bytes=<payload-length> count=<items>
```

For Code, `count` is the total structurally decoded instruction count. For other sections it is the declaration count; Module uses one.

Declaration records follow in this order: capabilities and their parameters, data, functions and their parameters/locals/instructions, exports, then nominal types and their fields or members.

```text
capability index=<n> name=<quoted-name> parameters=<n> result=<shape>
capability_parameter capability=<n> index=<n> type=<shape>

data index=<n> name=<quoted-name> type=text bytes=<n>
data index=<n> name=<quoted-name> type=i32_array elements=<n>
data index=<n> name=<quoted-name> type=bytes bytes=<n>

function index=<n> name=<quoted-name> parameters=<n> result=<shape> locals=<n> code_offset=<n> code_bytes=<n> max_stack=<n>
function_parameter function=<n> index=<n> type=<shape>
function_local function=<n> index=<n> type=<shape>
instruction function=<n> offset=<n> opcode=<name> [operand=<value>] [operand2=<value>]

export index=<n> name=<quoted-name> kind=function target=<function-index>

type index=<n> name=<quoted-name> kind=record fields=<n>
record_field type=<n> index=<n> name=<quoted-name> value_type=<shape>
type index=<n> name=<quoted-name> kind=enum members=<n>
enum_member type=<n> index=<n> name=<quoted-name> value=<i32>
```

A primitive shape is `void`, `i32`, `bool`, `text`, `u8`, `u32`, or `bytes`. Nominal shapes are `record[<type-index>]` and `enum[<type-index>]`. Instruction offsets are relative to the beginning of their function, matching branch operands. `i32.const` uses signed decimal; Boolean constants use `true` or `false`; `enum.const` emits both operands; other encoded operands use unsigned decimal.

## Diagnostics

Envelope failures use:

```text
<status> sections=<completed> offset=<absolute-byte-offset>
```

Payload failures use:

```text
<status> declarations=<completed> instructions=<completed> offset=<absolute-byte-offset>
```

Diagnostics go only to the diagnostic sink. They produce process result `2` and no partial standard-output report. Hosted argument or file-adapter failures remain runtime diagnostics with their stable `WVR` codes because Seed does not yet have catchable result values.
