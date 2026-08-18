# Workload 8 expected semantic outcomes

## Reference bindings

The launcher supplies:

- `Maximumˉlanes = 8`;
- `Outputˉbytes = 4096`;
- `Reportˉbytes = 4096`; and
- one 8,192-byte root memory budget.

No capability, operation context, package object, task runtime, or unsafe target
is bound.

## Exact values

The frozen output sequence contains these bits and canonical text in order:

| Index | Bits | Canonical f32 text |
| ---: | ---: | --- |
| 0 | `a8800000` | `-1.4210855e-14` |
| 1 | `41200000` | `10` |
| 2 | `c0000000` | `-2` |
| 3 | `00000001` | `1e-45` |
| 4 | `7f800000` | `inf` |
| 5 | `7fc00000` | `nan` |
| 6 | `80000000` | `-0` |
| 7 | `00000000` | `0` |

The audit record contains:

```text
Laneˉcount = 8
Canonicalˉnanˉbits = 7fc00000
Positiveˉzeroˉbits = 00000000
Negativeˉzeroˉbits = 80000000
Minimumˉsubnormalˉbits = 00000001
Overflowˉbits = 7f800000
Nearestˉintegerˉbits = 4b800000
Narrowˉnearestˉbits = 3f800000
Separateˉlane0ˉbits = 00000000
```

## Exact report

The final LF is part of this 328-byte UTF-8 text:

```text
WVNUM1
lane=0 bits=a8800000 value=-1.4210855e-14
lane=1 bits=41200000 value=10
lane=2 bits=c0000000 value=-2
lane=3 bits=00000001 value=1e-45
lane=4 bits=7f800000 value=inf
lane=5 bits=7fc00000 value=nan
lane=6 bits=80000000 value=-0
lane=7 bits=00000000 value=0
nearest_u32=4b800000
narrow_f64=3f800000
separate_lane0=00000000
```

SHA-256:
`25f308384b0a6ad088039cb3a65f5cf6eb928148b0f2cc9b18b2e2ca7c6ead2a`.

Windows, Linux, Windvale OS, interpreter, JIT, AOT, scalar, and any admitted
parallel implementation must produce these same bytes before claiming the
strict profile.

## Failure outcomes

- A wrong lane limit fails before memory split.
- Range overflow or out-of-range slice fails before publishing any view.
- Output capacity failure leaves the vector and rejected item unchanged.
- Any lane mismatch reports exact index/expected/observed bits.
- Exact conversion of `16777217u32` reports `Inexact`; it does not return the
  nearest value.
- NaN/infinity to i32 report distinct failures.
- A report maximum of 327 rejects before the append that would exceed it and
  publishes no text.
- A backend that contracts the ordinary lane-zero expression or implements the
  fused call as separate operations fails the exact bit oracle.
