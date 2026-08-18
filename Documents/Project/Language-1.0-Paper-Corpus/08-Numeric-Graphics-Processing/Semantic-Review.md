# Workload 8 semantic review

## Profile and authority

All six modules are Core. The only mutable owner is the local output vector; the
only allocations are its reserved backing and one reserved text builder. Fixed
arrays and scalar records are values, not implicit heap objects. The final
sequence and text are shared immutable publications.

## Type and conversion review

Every literal has an explicit suffix or exact expected field/array type. Array
literals receive `Array<u32, 8u64>` from module-data declarations and supply
exactly eight u32 expressions. No lane finds a common numeric type.

All bit reinterpretation and numeric conversion is named. Nearest, exact,
truncate, widen, and narrow remain distinct. `try` propagates only the exact
workload failure after explicit adapters.

## Ownership and slices

The output vector is reserved before mutation and contains eight initialized
f32 values. `Fill` borrows three module arrays immutably and the vector
exclusively. Four checked slices are lexical and cannot escape, overlap mutably,
resize/freeze their owner, cross a task, or expose an address.

`Mutableˉsliceˉreplace` returns each prior Copy f32 value; discarding that Copy
does not lose an owner. The mutable view ends when `Fill` returns, before
`Vectorˉfreeze` consumes the vector. Failure before view creation changes
nothing; the admitted loop has only proved in-range indices.

## Floating behavior

Each lane calls the explicit fused operation once. There is no operator
overload, contraction, reassociation, intermediate f64, host extended precision,
flush-to-zero, or host rounding mode. Arithmetic NaN is canonical, but raw
payload bits remain observable until arithmetic.

The audit distinguishes IEEE equality from bitwise equality and total order:
`-0 == +0`, `Bitwiseˉequalˉf32(-0,+0) = false`, and total order reports Less.
The minimum subnormal survives multiplication by one. Maximum finite times two
becomes infinity.

## Determinism and parallelism

Iteration is index 0 through 7. Output freeze preserves that order. Formatting
uses fixed lowercase hex and canonical f32 decimal calls, never interpolation or
host locale.

The workload does not spawn tasks. A future parallel library has a proof-friendly
shape because lanes are independent and output ranges can be split disjointly,
but it must compare with this exact sequential sequence/report. Reductions or
changed grouping are outside the evidence.

## Bounds

All collection lengths, ranges, allocation budgets, loop counts, formatting
bytes, and diagnostics are finite. The application validates limits before
splitting. Array ranges validate before borrow publication. Vector capacity is
fully reserved. The report builder reserves its complete maximum.

There is no recursion, capability call, I/O, retry, scheduler queue, provider
state, hidden collection growth, or retained diagnostic list.

## Failure atomicity

| Boundary | Failure | Observable state |
| --- | --- | --- |
| configuration | exact field/min/max | no child budget split |
| split/reserve | allocation evidence | no output publication |
| slice range | overflow/out-of-range geometry | no view published |
| append | collection/capacity evidence | vector/item unchanged |
| bit oracle | index/expected/observed | no successful result |
| conversion | exact named reason | input unchanged |
| text append | exact limit | builder unchanged for failing append; no text published |

## Acceptance matrix

| Pressure | Evidence | Standing |
| --- | --- | :---: |
| arrays/vectors/slices/generic algorithms | four fixed arrays, reserved vector, four views, const-generic functions | Pass with accepted completion |
| strict f32/f64 | fused/separate distinction, classification, special values | Pass |
| explicit conversions | nearest/exact/truncate/widen/narrow | Pass |
| deterministic formatting | exact hex + canonical f32, 328-byte hash | Pass |
| no overload/fast math | one named FMA, fixed operators/signatures | Pass |
| bounded parallelism | sequential oracle; disjoint-lane equivalence rule | Pass without parallel source |
