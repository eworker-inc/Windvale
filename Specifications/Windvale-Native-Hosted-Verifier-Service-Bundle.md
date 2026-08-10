# Windvale native hosted-verifier service bundle

## Status and scope

This contract transfers construction of the fixed hosted-verifier `WVSQ 2`
requests into portable Windvale. The legacy command accepts one verified native
fragment and the compiler verifier's six already selected platform service
leaves. The explicit `wvo-inspector` and `console-verifier` commands accept the
same prefix plus the five pure report-service leaves required by profiles 6 and
7. Each semantic selector has its own source entry while both construct the
same canonical eleven-service `WVPQ 1` publication request and supply one
bounded request to the shared Windvale service-bundle materializer.

These are two fixed verifier profiles, not a general service selector. The
contract does not authorize capabilities, choose a target, generate machine code, calculate
digests, or construct an outer PE/ELF container. Those boundaries precede or
follow this process explicitly.

## Input and request contract

Inputs occur in this exact order:

1. verifier native fragment;
2. service 1, `console.write_line`;
3. service 2, `process.argument_count`;
4. service 3, `process.argument`;
5. service 4, `file.read_bytes`;
6. service 5, startup-internal `text.utf8_is_valid`;
7. service 6, `diagnostic.write_line`.

The explicit `wvo-inspector` and `console-verifier` forms then require, in
order, service 7 `enum.name`, service 8 `text.concat`, service 9 `text.quote`,
service 10 `i32.format`, and service 11 `u32.format`.

Every input is nonempty. Their resource names are pairwise distinct and the
output name does not alias an input. Platform-specific leaves are selected and
verified before invocation; this process preserves their bytes and order.

Windvale constructs the exact 96-byte `WVPQ 1` request with six 12-byte service
records for the legacy path, or the exact 156-byte request with eleven records
for either explicit selector. It then requires the shared publication planner to return
a successful 104-byte or 164-byte layout for the same fragment and service
count. The complete image must
fit the canonical 4,194,104-byte service-bundle segment. The emitted `WVSQ 2`
has segment offset zero, the complete planned image extent, and a payload
containing the fragment followed by the six or eleven raw leaves. Alignment
fill remains
omitted and is reconstructed only by the shared
[`WVSQ 2` materializer](Windvale-Native-Service-Bundle-Materialization.md).

## Command contract

```text
wvhostverifierbundle <fragment> <console> <argument-count> <argument> <file-input> <utf8> <diagnostic> <request.wvsq>
wvhostverifierbundle wvo-inspector <fragment> <console> <argument-count> <argument> <file-input> <utf8> <diagnostic> <enum-name> <text-concat> <text-quote> <i32-format> <u32-format> <request.wvsq>
wvhostverifierbundle console-verifier <fragment> <console> <argument-count> <argument> <file-input> <utf8> <diagnostic> <enum-name> <text-concat> <text-quote> <i32-format> <u32-format> <request.wvsq>
```

Success writes one exact request, reports
`verifier service-bundle request status=Valid bytes=N`, and returns zero.
Invalid input or duplicate input names report `Rejected`, return 2, and preserve
an existing output. An invalid invocation or input/output alias reports the
usage line, returns 64, and preserves every input.

The application declares exactly `console.write_line`,
`diagnostic.write_line`, `file.read_bytes`, `file.write_bytes`,
`process.argument`, and `process.argument_count`.

## Exact identities and retirement boundary

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Request WVB | 22,699 | `e5499702b0440434aa6776e2664900f9158c226613c716670c66f5de44b32982` |
| Windows application | 264,192 | `eb3ec1d1236b73d67c5682d45fa591e881212f3791e3e22b692a38f43f51d346` |
| Linux application | 266,240 | `b4f5d9dc5a2fb38806ed3b7a1ad2bf2d5362d755033bb3c84bbc1ace7560f2d3` |

These rows are the retained packaged products from the hosted-toolset refresh
that consumes the profile-7-capable source. The WVB builds through the native
Project 1 front door. Both applications
reconstruct byte-for-byte through the shared native hosted-container packager;
no managed product writer or target registration is added. Focused evidence
constructs the Windows and Linux requests, compares them with the frozen
reference bytes, and makes the existing Windvale materializer reproduce both
complete service bundles.

Independent Linux process execution, verifier startup and final-container
composition, promotion, and the grouped retirement gate remain.
