# Windvale native hosted-verifier service bundle

## Status and scope

This contract transfers construction of the compiler verifier's exact
six-service `WVSQ 2` request into portable Windvale. It accepts one verified
native fragment and the six already selected platform service leaves, constructs
and validates their canonical `WVPQ 1` publication request, and supplies one
bounded request to the shared Windvale service-bundle materializer.

This is a fixed verifier profile, not a general service selector. It does not
authorize capabilities, choose a target, generate machine code, calculate
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

Every input is nonempty. Their resource names are pairwise distinct and the
output name does not alias an input. Platform-specific leaves are selected and
verified before invocation; this process preserves their bytes and order.

Windvale constructs the exact 96-byte `WVPQ 1` request with six 12-byte service
records, then requires the shared publication planner to return a successful
104-byte layout for the same fragment and service count. The complete image must
fit the canonical 4,194,104-byte service-bundle segment. The emitted `WVSQ 2`
has segment offset zero, the complete planned image extent, and a payload
containing the fragment followed by the six raw leaves. Alignment fill remains
omitted and is reconstructed only by the shared
[`WVSQ 2` materializer](Windvale-Native-Service-Bundle-Materialization.md).

## Command contract

```text
wvhostverifierbundle <fragment> <console> <argument-count> <argument> <file-input> <utf8> <diagnostic> <request.wvsq>
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
| Request WVB | 13,993 | `b23655332f5525fd411cb3a0a1f815af49f97d743156dfd4d0ae7549fab586f4` |
| Windows application | 160,256 | `b4902fc6554f6e8bd52c83b870d9cf6b6e179c3207a34037e6f58e44d657d18b` |
| Linux application | 159,744 | `e8deec17224202394f828db219734fd2e31c266819fc62c616a82bb1db495353` |

The WVB builds through the native Project 1 front door. Both applications
reconstruct byte-for-byte through the shared native hosted-container packager;
no managed product writer or target registration is added. Focused evidence
constructs the Windows and Linux requests, compares them with the frozen
reference bytes, and makes the existing Windvale materializer reproduce both
complete service bundles.

Independent Linux process execution, verifier startup and final-container
composition, promotion, and the grouped retirement gate remain.
