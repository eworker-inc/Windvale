# Database storage geometry

## Status and purpose

`Windvaleˉdatabaseˉstorageˉgeometry` is the implemented portable arithmetic boundary for future database storage. It computes one complete zero-based page byte range using `u64` page identity, offset, length, and exclusive-end values. It performs no I/O, grants no authority, selects no durable format, and does not change the experimental `WVDB 1` reader.

The implementation is [`Libraries/Database/Storage-Geometry.wv`](../Libraries/Database/Storage-Geometry.wv).

## Public contract

```text
Databaseˉstorageˉpageˉrange(
    Headerˉsize: u64,
    Pageˉsize: u32,
    Pageˉidentifier: u64,
    Storageˉlength: u64
) -> Databaseˉpageˉrangeˉresult
```

A valid result carries:

- the unchanged zero-based `Pageˉidentifier`;
- `Offset = Headerˉsize + Pageˉidentifier * Pageˉsize`;
- the losslessly widened page `Length`; and
- `Endˉexclusive = Offset + Length`.

The complete half-open range `[Offset, Endˉexclusive)` must fit in `Storageˉlength`. The operation returns `Invalidˉpageˉsize` for zero, `Arithmeticˉoverflow` when a required `u64` product or sum cannot be represented, and `Outsideˉstorage` when the representable range exceeds the supplied storage length. These are typed results, not traps.

The implementation preflights multiplication and both additions before evaluating them. An exclusive end makes exact range length and bounds comparison unambiguous. A requested range whose final exclusive end would be `2^64` is rejected because that value cannot be represented by the selected `u64` storage domain.

## Portability and authority boundary

The module is portable and capability-free. `Storageˉlength` is evidence supplied by a caller, not proof that a file, device, or provider exists. A future pre-opened storage resource must separately define provider generation, lifetime, revocation, read and mutation progress, writer fencing, flush classes, publication, and recovery behavior. Windows, Linux, Windvale OS, and WebAssembly hosts may bind different providers without changing this arithmetic contract.

The canonical source intrinsic `U64ˉfromˉu32(u32) -> u64` and WVB 1.11 opcode `0xBF` supply the explicit lossless width transition used for page size. Narrow native, WebAssembly, and Windvale OS consumers may continue to reject this otherwise valid WVB 1.11 operation until their named subsets adopt `u64` values.
