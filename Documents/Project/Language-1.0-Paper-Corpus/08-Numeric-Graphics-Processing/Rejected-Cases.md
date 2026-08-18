# Workload 8 rejected and boundary cases

## Compile-time and typing

| Case | Input | Required outcome |
| ---: | --- | --- |
| 1 | Array literal without expected `Array<T,N>`. | Diagnostic: exact expected array type required. |
| 2 | Seven or nine elements for `Array<u32,8>`. | Exact expected/observed count diagnostic. |
| 3 | One i32 element among u32 elements. | Exact element-type diagnostic; no conversion. |
| 4 | `[]` under nonzero N. | Count diagnostic. |
| 5 | `[Value; 8]`. | Grammar rejection; no 1.0 repetition form. |
| 6 | Unsuffixed bit literal without exact context. | Literal-type diagnostic. |
| 7 | Add f32 and f64. | Exact operand-type diagnostic. |
| 8 | Assign u32 directly to f32. | Missing named-conversion diagnostic. |
| 9 | Call a generic `cast::<f32>`. | Unknown declaration; no general cast. |
| 10 | Overload `+` for a vector. | Operator implementation rejection. |
| 11 | Return a slice from two possible owners. | Borrow provenance diagnostic. |
| 12 | Freeze vector while its mutable slice is live. | Move/borrow conflict diagnostic. |
| 13 | Create a second read or write view during exclusive slice. | Alias conflict diagnostic. |
| 14 | Store mutable slice in a record/task/module value. | Escaping-borrow diagnostic. |
| 15 | Capture mutable slice in spawned work. | Task-boundary borrow diagnostic. |
| 16 | Use `parallel for`. | Grammar rejection; no such statement. |

## Slice and collection boundaries

| Case | Input | Required outcome |
| ---: | --- | --- |
| 17 | `Start + Length` overflows u64. | `Rangeˉoverflow`; no view. |
| 18 | Start one beyond owner. | `Outˉofˉrange`; no view. |
| 19 | End one beyond owner. | `Outˉofˉrange`; no view. |
| 20 | Empty slice at owner end. | Valid zero-length view. |
| 21 | Empty slice beyond owner end. | `Outˉofˉrange`. |
| 22 | Read at slice length. | Terminal precondition trap before access. |
| 23 | Replace at mutable-slice length. | Terminal precondition trap before mutation. |
| 24 | Output vector maximum zero. | Invalid-limit failure before allocation. |
| 25 | Ninth append to maximum eight. | Capacity failure; vector and item unchanged. |
| 26 | Physical reserve failure. | Allocation failure; no vector. |
| 27 | Freeze with active view. | Compile-time rejection, not runtime race. |
| 28 | Overlapping split mutable ranges. | Rejection before either conflicting view. |

## Floating special values and operations

| Case | Input | Required outcome |
| ---: | --- | --- |
| 29 | Payload NaN bits observed before arithmetic. | Original bits preserved. |
| 30 | Same NaN plus zero. | Canonical `7fc00000`. |
| 31 | Positive zero times one. | `00000000`. |
| 32 | Negative zero times one. | `80000000`. |
| 33 | Compare signed zeros with `==`. | Equal. |
| 34 | Bitwise-compare signed zeros. | Not equal. |
| 35 | Total-order signed zeros. | Negative zero Less. |
| 36 | Minimum subnormal times one. | `00000001`; no flush. |
| 37 | Maximum finite times two. | Positive infinity. |
| 38 | Infinity times zero plus one in FMA. | Canonical NaN. |
| 39 | Lane zero through named FMA. | `a8800000`. |
| 40 | Lane zero through separate operators. | `00000000`. |
| 41 | Contract ordinary expression into FMA. | Cross-target bit failure. |
| 42 | Implement named FMA as multiply then add. | Cross-target bit failure. |
| 43 | Reassociate ordinary operations. | Semantic/conformance failure. |
| 44 | Evaluate through wider hidden accumulator. | Semantic/conformance failure. |

## Conversion and formatting

| Case | Input | Required outcome |
| ---: | --- | --- |
| 45 | Exact u32→f32 of 16,777,217. | `Inexact`. |
| 46 | Nearest u32→f32 of 16,777,217. | `4b800000`. |
| 47 | Narrow halfway f64 nearest-even. | f32 `3f800000`. |
| 48 | Narrow same f64 exact. | `Inexact`. |
| 49 | NaN to i32 truncate. | `Notˉaˉnumber`. |
| 50 | Infinity to i32 truncate. | `Infinite`. |
| 51 | Finite below/above i32. | Exact range failure; no clamp/wrap. |
| 52 | `-42.75f32` truncate. | `-42i32`. |
| 53 | Widen `1.5f32` to f64. | Exact `3ff8000000000000`. |
| 54 | Format arbitrary NaN payload. | `nan`, no payload text. |
| 55 | Format negative zero. | `-0`. |
| 56 | Format minimum subnormal. | `1e-45`. |
| 57 | Use uppercase hex/exponent or locale comma. | Canonical-output mismatch. |
| 58 | Builder has 327 bytes for report. | Limit failure; no truncated text. |
| 59 | Builder allocation fails. | Allocation failure; no text. |
| 60 | Host formatter chooses a longer/other spelling. | Exact report/hash failure. |

## Parallel and cross-target

| Case | Input | Required outcome |
| ---: | --- | --- |
| 61 | Process disjoint lanes in another schedule. | Same ordered bits/report or rejection. |
| 62 | Publish completion order instead of lane order. | Determinism failure. |
| 63 | Parallel reduction with unspecified grouping. | API/source rejection. |
| 64 | Target lacks strict subnormal/FMA behavior. | Unsupported strict profile; no substitute. |
| 65 | Backend uses host rounding/fast-math flag. | Qualification failure. |
| 66 | Windows/Linux result bit differs. | Cross-host conformance failure. |

Implementation coverage should pair these with accepted finite normals,
positive/negative infinity, both zeros, every class, conversion endpoints,
slice start/end boundaries, exact builder capacities, and deterministic repeats.
