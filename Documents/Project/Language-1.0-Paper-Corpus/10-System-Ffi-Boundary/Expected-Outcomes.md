# Workload 10 expected semantic outcomes

## Valid completion

The isolated foreign shim receives one non-null 8-byte-aligned pointer, capacity
64, and expected generation 42. It writes the exact 24-byte record from the
format contract and returns `24i64`.

The adapter:

1. proves the status nonnegative and converts it exactly to u64;
2. proves 24 is within scratch and configured record maxima;
3. ends the exclusive foreign region;
4. lends a 24-byte immutable ordinary slice to Core;
5. publishes kind `Delta`, enabled true, generation 42, and independently owned
   payload `de ad be ef`; and
6. publishes the exact report:

~~~text
WVFFI1
kind=delta
enabled=true
generation=42
payload=deadbeef
~~~

The report is 62 UTF-8 bytes, 5 LF-terminated lines, with SHA-256
`c0a915258a1d23e50599c51f208465768368683158b8d9a17af2b981999961cd`.

## Stale generation

The shim writes observed generation 43 as eight little-endian bytes and returns
`-3i64`. The adapter returns
`Staleˉgeneration(Expected: 42, Observed: 43)`. It does not decode a record,
copy a payload, render a report, or retry.

## Ownership outcomes

- successful scratch construction consumes one child budget and owns one lease;
- region failure publishes no pointer and leaves scratch unchanged;
- a live region exclusively borrows scratch;
- the foreign pointer cannot outlive or be stored beyond that region;
- rejected/failed/invalid status releases scratch without safe publication;
- successful decode owns a new immutable payload independent of scratch; and
- lexical teardown releases scratch exactly once on every ordinary path.

## Compiler planning ceilings

The future executable fixture admits at most 32 generic instances, 256 WIR
blocks, 2,048 WIR operations, 20 call-depth units, 16 diagnostics, and 512 KiB
retained compiler/unsafe proof evidence. These are admission ceilings, not
measurements. Excess rejects before artifact publication.
