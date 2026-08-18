# Windvale Language 1.0 Foundation signature registry

## Status and ownership

This is the normative-candidate, machine-extractable signature registry for the
[Language 1.0 Foundation specification](Windvale-Language-1.0-Foundation.md).
Its complete-suite role is accepted by
[Decision 0765](../Documents/Decisions/0765-Complete-Language-1.0-Source-Freeze-Candidate.md).
Its localized-source catalog binding role is accepted by
[Decision 0766](../Documents/Decisions/0766-Complete-Language-1.0-Localized-Source-Reconciliation.md).
It fixes the complete edition-1 public declaration surface without claiming that
the current Seed Foundation implements it. The Foundation specification owns
behavior, failure ordering, bounds, ownership, and effects; this registry owns
canonical module names, major versions, declaration spelling, and signature-set
identities.

The owner-accepted replacement-candidate
[localized-source specification](Windvale-Language-1.0-Localized-Source.md)
uses each exact signature-set hash as the binding anchor for separately shipped
source-vocabulary catalogs. Localized module, declaration, field, case, and
parameter labels do not enter the canonical signature identity below and do not
create additional registry declarations. Every complete catalog must target the
exact canonical identity set and becomes stale when that set or its signatures
change.

Each `windvale-foundation-signatures` block is one complete module signature
set. Its identity input is the strict UTF-8 byte sequence beginning with the
block's `module` line and ending with the LF immediately after its `end module;`
line. Markdown fences and surrounding prose are excluded. Every line ending in
the identity input is LF, there is no byte-order mark, and no Unicode or
whitespace normalization is performed. The identity is lowercase hexadecimal
SHA-256 of that exact byte sequence.

The registry notation uses edition-1 declaration syntax plus three
signature-only forms:

- `opaque <class> type Name;` declares representation-hidden public types;
- `intrinsic type Name;` identifies a primitive type supplied by the language;
  and
- `family Name { ... }` defines a finite deterministic signature expansion.

The admitted classes are `copy`, `shared`, `owned`, and `borrowed`. A borrowed
opaque type carries one source-owner lifetime and cannot be stored independently
of that owner. A `family` is part of
the signature set exactly as written. Its expansion rule is normative and
cannot gain another generated declaration without changing the module identity.
There are no declarations outside the listed blocks in Foundation major 1.

## Candidate module identities

| Module | Major | Minimum profile | Signature-set SHA-256 |
| --- | ---: | --- | --- |
| `Foundationˉoption` | 1 | Core | `1fe70000bf7a33c035dcc163bbbab4c299e4a8301a806e28592dbb569e8011e4` |
| `Foundationˉresult` | 1 | Core | `aabe8b02f6e239a198d21780704679936bbf8125cdd5ea55431d66a650590691` |
| `Foundationˉnumeric` | 1 | Core | `4ca030c3c5d7dd53575094dc6c176de7a60ec14991e5b70cfb8cf772da9638aa` |
| `Foundationˉordering` | 1 | Core | `6194f9674195aa96d84d85b017ec86e6d7548ae3d184b2cbbac590820e129a4d` |
| `Foundationˉmemory` | 1 | Core | `aa9537df1ec92a8a6fae0e5a7517ac23589c4aa71e6e130dfdf87019d18a187f` |
| `Foundationˉcollections` | 1 | Core | `3cbcc436bc30c774db1b1214b3e82629bf20a67f05bd09fed7d2ec1b2f742234` |
| `Foundationˉbytes` | 1 | Core | `8dd5cd3b1bc4cd5c877ab6c2b73b3ca4c67740b9ce8e1ea13772cd4975fff64b` |
| `Foundationˉtext` | 1 | Core | `67b0329dc3242c245a0c3be9dccbe1c73263745173d7eced450ee8f5018e753f` |
| `Foundationˉresource` | 1 | Core | `74a7e0a64b9df8a43bd6c9fea23d7bd8ed6ed8222a630b6e323ea2f99c27bc8a` |
| `Foundationˉtask` | 1 | Hosted | `453090ee631482c1b83c2016a32726f79abff79f936484e4315969f29ca9af84` |
| `Foundationˉunsafe` | 1 | System | `461263f6041122ff7dca5401f1e778382c0699c35fc557c8c27565a5192ce8df` |

The hashes are candidate identities until the explicit Language 1.0
source-freeze decision accepts them. They may change during owner review; a
change to one block changes only that module's signature-set identity unless a
dependent signature also changes.

## Foundation option

~~~windvale-foundation-signatures
module Foundationˉoption major 1;
export variant Option<T> { Present(Value: T); Absent; }
export fn Isˉpresent<T>(Value: borrow Option<T>) -> bool effects();
export fn Borrow<T>(Value: borrow Option<T>) -> Option<borrow T> effects();
export fn Borrowˉmut<T>(Value: borrow mut Option<T>) -> Option<borrow mut T> effects();
export fn Take<T>(Value: borrow mut Option<T>) -> Option<T> effects();
export fn Map<T, U>(Value: Option<T>, Transform: fn(T) -> U effects()) -> Option<U> effects();
end module;
~~~

## Foundation result

~~~windvale-foundation-signatures
module Foundationˉresult major 1;
export variant Result<T, E> { Valid(Value: T); Failure(Error: E); }
export fn Isˉvalid<T, E>(Value: borrow Result<T, E>) -> bool effects();
export fn Isˉfailure<T, E>(Value: borrow Result<T, E>) -> bool effects();
export fn Borrowˉvalid<T, E>(Value: borrow Result<T, E>) -> Foundationˉoption.Option<borrow T> effects();
export fn Borrowˉfailure<T, E>(Value: borrow Result<T, E>) -> Foundationˉoption.Option<borrow E> effects();
export fn Borrowˉvalidˉmut<T, E>(Value: borrow mut Result<T, E>) -> Foundationˉoption.Option<borrow mut T> effects();
export fn Borrowˉfailureˉmut<T, E>(Value: borrow mut Result<T, E>) -> Foundationˉoption.Option<borrow mut E> effects();
export fn Mapˉvalid<T, E, U>(Value: Result<T, E>, Transform: fn(T) -> U effects()) -> Result<U, E> effects();
export fn Mapˉfailure<T, E, F>(Value: Result<T, E>, Transform: fn(E) -> F effects()) -> Result<T, F> effects();
end module;
~~~

## Foundation numeric

~~~windvale-foundation-signatures
module Foundationˉnumeric major 1;
export variant Numericˉconversionˉfailure { Belowˉminimum; Aboveˉmaximum; }
export variant Floatingˉconversionˉfailure { Notˉaˉnumber; Infinite; Belowˉminimum; Aboveˉmaximum; Inexact; }
export enum Floatingˉclass: u8 { Negativeˉinfinity = 1u8; Negativeˉnormal = 2u8; Negativeˉsubnormal = 3u8; Negativeˉzero = 4u8; Positiveˉzero = 5u8; Positiveˉsubnormal = 6u8; Positiveˉnormal = 7u8; Positiveˉinfinity = 8u8; Notˉaˉnumber = 9u8; }
export variant Numericˉparseˉfailure { Empty; Invalidˉdigit(Offset: u64); Invalidˉsign(Offset: u64); Invalidˉseparator(Offset: u64); Trailingˉinput(Offset: u64); Belowˉminimum; Aboveˉmaximum; Limitˉexceeded; }
family Integerˉconversionˉmatrix {
    Types: i8, i16, i32, i64, u8, u16, u32, u64;
    For each ordered distinct pair S, D, emit exactly one declaration in source-type order then destination-type order;
    If every mathematical S value is representable by D: export fn WidenˉSˉtoˉD(Value: S) -> D effects();
    Otherwise: export fn ConvertˉSˉtoˉD(Value: S) -> Foundationˉresult.Result<D, Numericˉconversionˉfailure> effects();
}
family Integerˉbitˉreinterpretationˉmatrix {
    Widths: 8, 16, 32, 64;
    For each width W in listed order emit: export fn BitsˉiWˉtoˉuW(Value: iW) -> uW effects();
    Then emit: export fn BitsˉuWˉtoˉiW(Value: uW) -> iW effects();
}
family Integerˉfloatingˉconversionˉmatrix {
    Integerˉtypes: i8, i16, i32, i64, u8, u16, u32, u64;
    Floatingˉtypes: f32, f64;
    For each S then D in listed order emit: export fn ConvertˉSˉtoˉDˉnearest(Value: S) -> D effects();
    Then emit: export fn ConvertˉSˉtoˉDˉexact(Value: S) -> Foundationˉresult.Result<D, Floatingˉconversionˉfailure> effects();
    For each S in f32, f64 then D in integer-type order emit: export fn ConvertˉSˉtoˉDˉtruncate(Value: S) -> Foundationˉresult.Result<D, Floatingˉconversionˉfailure> effects();
}
export fn Bitsˉf32ˉtoˉu32(Value: f32) -> u32 effects();
export fn Bitsˉu32ˉtoˉf32(Value: u32) -> f32 effects();
export fn Bitsˉf64ˉtoˉu64(Value: f64) -> u64 effects();
export fn Bitsˉu64ˉtoˉf64(Value: u64) -> f64 effects();
export fn Classifyˉf32(Value: f32) -> Floatingˉclass effects();
export fn Classifyˉf64(Value: f64) -> Floatingˉclass effects();
export fn Bitwiseˉequalˉf32(Left: f32, Right: f32) -> bool effects();
export fn Bitwiseˉequalˉf64(Left: f64, Right: f64) -> bool effects();
export fn Totalˉcompareˉf32(Left: f32, Right: f32) -> Foundationˉordering.Orderingˉresult effects();
export fn Totalˉcompareˉf64(Left: f64, Right: f64) -> Foundationˉordering.Orderingˉresult effects();
export fn Fusedˉmultiplyˉaddˉf32(Left: f32, Right: f32, Addend: f32) -> f32 effects();
export fn Fusedˉmultiplyˉaddˉf64(Left: f64, Right: f64, Addend: f64) -> f64 effects();
export fn Widenˉf32ˉtoˉf64(Value: f32) -> f64 effects();
export fn Narrowˉf64ˉtoˉf32ˉnearest(Value: f64) -> f32 effects();
export fn Narrowˉf64ˉtoˉf32ˉexact(Value: f64) -> Foundationˉresult.Result<f32, Floatingˉconversionˉfailure> effects();
export fn Parseˉu64ˉdecimalˉwhole(Value: borrow text, Maximumˉinputˉbytes: u64) -> Foundationˉresult.Result<u64, Numericˉparseˉfailure> effects();
end module;
~~~

## Foundation ordering

~~~windvale-foundation-signatures
module Foundationˉordering major 1;
export protocol Equality<T> { fn Equal(Left: borrow T, Right: borrow T) -> bool effects(); }
export enum Orderingˉresult: i8 { Less = -1i8; Equal = 0i8; Greater = 1i8; }
export protocol Ordering<T> { fn Compare(Left: borrow T, Right: borrow T) -> Orderingˉresult effects(); }
end module;
~~~

## Foundation memory

~~~windvale-foundation-signatures
module Foundationˉmemory major 1;
export variant Limitˉfailure { Maximumˉexceeded(Requested: u64, Maximum: u64); Arithmeticˉoverflow; }
export enum Allocationˉreason: u8 { Budgetˉexhausted = 1u8; Targetˉunaddressable = 2u8; Providerˉunavailable = 3u8; Fragmented = 4u8; }
export record Allocationˉfailure { Reason: Allocationˉreason; Requestedˉbytes: u64; Availableˉbytes: u64; }
export variant Capacityˉfailure<T> { Rejected(Requestedˉitems: u64, Remainingˉitems: u64, Value: T); Acceptedˉprefix(Acceptedˉitems: u64, Remaining: T); }
opaque owned type Memoryˉbudget;
opaque owned type Allocationˉlease;
export fn Split(Parent: borrow mut Memoryˉbudget, Maximumˉbytes: u64, Maximumˉchildren: u32) -> Foundationˉresult.Result<Memoryˉbudget, Allocationˉfailure> effects(memory.allocate);
end module;
~~~

## Foundation collections

~~~windvale-foundation-signatures
module Foundationˉcollections major 1;
opaque owned type Vector<T>;
opaque shared type Sequence<T>;
opaque borrowed type Slice<T>;
opaque borrowed type Mutableˉslice<T>;
opaque owned type Map<K, V>;
opaque shared type Immutableˉmap<K, V>;
opaque owned type Set<T>;
opaque shared type Immutableˉset<T>;
opaque owned type Arena<T>;
opaque shared type Immutableˉarena<T>;
opaque copy type Handle<T>;
export record Vectorˉappendˉfailure<T> { Error: Collectionˉfailure; Value: T; }
export variant Sliceˉfailure { Rangeˉoverflow(Start: u64, Length: u64); Outˉofˉrange(Start: u64, Length: u64, Ownerˉlength: u64); }
export variant Collectionˉfailure { Invalidˉlimit(Field: u32, Observed: u64, Minimum: u64, Maximum: u64); Allocation(Error: Foundationˉmemory.Allocationˉfailure); Capacityˉexhausted(Maximum: u64); Duplicate; Comparisonˉlimit(Maximum: u64); Wrongˉarena; Slotˉoutˉofˉrange(Slot: u64, Maximum: u64); Vacant(Slot: u64); Staleˉgeneration(Expected: u64, Observed: u64); Retired(Slot: u64); }
export record Mapˉinsertˉfailure<K, V> { Error: Collectionˉfailure; Key: K; Value: V; }
export record Mapˉentry<K, V> { Key: K; Value: V; }
export variant Mapˉreplaceˉoutcome<V> { Replaced(Previous: V); Absent(Replacement: V); Rejected(Error: Collectionˉfailure, Replacement: V); }
export record Setˉinsertˉfailure<T> { Error: Collectionˉfailure; Value: T; }
export record Arenaˉseed<T> { Owner: Arena<T>; First: Handle<T>; }
export record Arenaˉinsertˉfailure<T> { Error: Collectionˉfailure; Value: T; }
export record Arenaˉreplaceˉfailure<T> { Error: Collectionˉfailure; Value: T; }
export protocol Iterator<Self, T> { fn Maximumˉremaining(Value: borrow Self) -> u64 effects(); fn Next(Value: borrow mut Self) -> Foundationˉoption.Option<T> effects(); }
export fn Vectorˉconstructˉreserved<T>(Budget: Foundationˉmemory.Memoryˉbudget, Maximumˉitems: u64) -> Foundationˉresult.Result<Vector<T>, Foundationˉmemory.Allocationˉfailure> effects(memory.allocate);
export fn Vectorˉappend<T>(Vector: borrow mut Vector<T>, Value: T) -> Foundationˉresult.Result<unit, Vectorˉappendˉfailure<T>> effects();
export fn Vectorˉlength<T>(Vector: borrow Vector<T>) -> u64 effects();
export fn Vectorˉborrowˉat<T>(Vector: borrow Vector<T>, Index: u64) -> borrow T effects();
export fn Vectorˉborrowˉatˉmut<T>(Vector: borrow mut Vector<T>, Index: u64) -> borrow mut T effects();
export fn Vectorˉreplace<T>(Vector: borrow mut Vector<T>, Index: u64, Replacement: T) -> T effects();
export fn Vectorˉremove<T>(Vector: borrow mut Vector<T>, Index: u64) -> T effects();
export fn Vectorˉfreeze<T>(Vector: Vector<T>) -> Sequence<T> effects();
export fn Sequenceˉlength<T>(Value: borrow Sequence<T>) -> u64 effects();
export fn Sequenceˉat<T>(Value: borrow Sequence<T>, Index: u64) -> borrow T effects();
export fn Sliceˉlength<T>(Value: Slice<T>) -> u64 effects();
export fn Sliceˉat<T>(Value: Slice<T>, Index: u64) -> borrow T effects();
export fn Arrayˉslice<T, const N: u64>(Value: borrow Array<T, N>, Start: u64, Length: u64) -> Foundationˉresult.Result<Slice<T>, Sliceˉfailure> effects();
export fn Vectorˉslice<T>(Value: borrow Vector<T>, Start: u64, Length: u64) -> Foundationˉresult.Result<Slice<T>, Sliceˉfailure> effects();
export fn Vectorˉsliceˉmut<T>(Value: borrow mut Vector<T>, Start: u64, Length: u64) -> Foundationˉresult.Result<Mutableˉslice<T>, Sliceˉfailure> effects();
export fn Mutableˉsliceˉlength<T>(Value: Mutableˉslice<T>) -> u64 effects();
export fn Mutableˉsliceˉreplace<T>(Value: Mutableˉslice<T>, Index: u64, Replacement: T) -> T effects();
export fn Mapˉconstruct<K, V>(Budget: Foundationˉmemory.Memoryˉbudget, Maximumˉitems: u64) -> Foundationˉresult.Result<Map<K, V>, Collectionˉfailure> effects(memory.allocate) where K: Foundationˉordering.Ordering<K>;
export fn Mapˉconstructˉwithˉfirst<K, V>(Budget: Foundationˉmemory.Memoryˉbudget, Maximumˉitems: u64, Key: K, Value: V) -> Foundationˉresult.Result<Map<K, V>, Mapˉinsertˉfailure<K, V>> effects(memory.allocate) where K: Foundationˉordering.Ordering<K>;
export fn Mapˉinsert<K, V>(Map: borrow mut Map<K, V>, Key: K, Value: V) -> Foundationˉresult.Result<unit, Mapˉinsertˉfailure<K, V>> effects() where K: Foundationˉordering.Ordering<K>;
export fn Mapˉlength<K, V>(Map: borrow Map<K, V>) -> u64 effects();
export fn Mapˉcontains<K, V>(Map: borrow Map<K, V>, Key: borrow K) -> bool effects() where K: Foundationˉordering.Ordering<K>;
export fn Mapˉfindˉrank<K, V>(Map: borrow Map<K, V>, Key: borrow K) -> Foundationˉoption.Option<u64> effects() where K: Foundationˉordering.Ordering<K>;
export fn Mapˉborrowˉat<K, V>(Map: borrow Map<K, V>, Index: u64) -> borrow V effects();
export fn Mapˉkeyˉat<K, V>(Map: borrow Map<K, V>, Index: u64) -> borrow K effects();
export fn Mapˉreplace<K, V>(Map: borrow mut Map<K, V>, Key: borrow K, Replacement: V) -> Mapˉreplaceˉoutcome<V> effects() where K: Foundationˉordering.Ordering<K>;
export fn Mapˉremove<K, V>(Map: borrow mut Map<K, V>, Key: borrow K) -> Foundationˉresult.Result<Foundationˉoption.Option<Mapˉentry<K, V>>, Collectionˉfailure> effects() where K: Foundationˉordering.Ordering<K>;
export fn Mapˉfreeze<K, V>(Map: Map<K, V>) -> Immutableˉmap<K, V> effects();
export fn Immutableˉmapˉlength<K, V>(Map: borrow Immutableˉmap<K, V>) -> u64 effects();
export fn Immutableˉmapˉcontains<K, V>(Map: borrow Immutableˉmap<K, V>, Key: borrow K) -> bool effects() where K: Foundationˉordering.Ordering<K>;
export fn Immutableˉmapˉfindˉrank<K, V>(Map: borrow Immutableˉmap<K, V>, Key: borrow K) -> Foundationˉoption.Option<u64> effects() where K: Foundationˉordering.Ordering<K>;
export fn Immutableˉmapˉborrowˉat<K, V>(Map: borrow Immutableˉmap<K, V>, Index: u64) -> borrow V effects();
export fn Immutableˉmapˉkeyˉat<K, V>(Map: borrow Immutableˉmap<K, V>, Index: u64) -> borrow K effects();
export fn Setˉconstruct<T>(Budget: Foundationˉmemory.Memoryˉbudget, Maximumˉitems: u64) -> Foundationˉresult.Result<Set<T>, Collectionˉfailure> effects(memory.allocate) where T: Foundationˉordering.Ordering<T>;
export fn Setˉconstructˉwithˉfirst<T>(Budget: Foundationˉmemory.Memoryˉbudget, Maximumˉitems: u64, Value: T) -> Foundationˉresult.Result<Set<T>, Setˉinsertˉfailure<T>> effects(memory.allocate) where T: Foundationˉordering.Ordering<T>;
export fn Setˉinsert<T>(Set: borrow mut Set<T>, Value: T) -> Foundationˉresult.Result<unit, Setˉinsertˉfailure<T>> effects() where T: Foundationˉordering.Ordering<T>;
export fn Setˉlength<T>(Set: borrow Set<T>) -> u64 effects();
export fn Setˉcontains<T>(Set: borrow Set<T>, Value: borrow T) -> bool effects() where T: Foundationˉordering.Ordering<T>;
export fn Setˉfindˉrank<T>(Set: borrow Set<T>, Value: borrow T) -> Foundationˉoption.Option<u64> effects() where T: Foundationˉordering.Ordering<T>;
export fn Setˉborrowˉat<T>(Set: borrow Set<T>, Index: u64) -> borrow T effects();
export fn Setˉremove<T>(Set: borrow mut Set<T>, Value: borrow T) -> Foundationˉresult.Result<Foundationˉoption.Option<T>, Collectionˉfailure> effects() where T: Foundationˉordering.Ordering<T>;
export fn Setˉfreeze<T>(Set: Set<T>) -> Immutableˉset<T> effects();
export fn Immutableˉsetˉlength<T>(Set: borrow Immutableˉset<T>) -> u64 effects();
export fn Immutableˉsetˉcontains<T>(Set: borrow Immutableˉset<T>, Value: borrow T) -> bool effects() where T: Foundationˉordering.Ordering<T>;
export fn Immutableˉsetˉfindˉrank<T>(Set: borrow Immutableˉset<T>, Value: borrow T) -> Foundationˉoption.Option<u64> effects() where T: Foundationˉordering.Ordering<T>;
export fn Immutableˉsetˉborrowˉat<T>(Set: borrow Immutableˉset<T>, Index: u64) -> borrow T effects();
export fn Arenaˉconstruct<T>(Budget: Foundationˉmemory.Memoryˉbudget, Maximumˉnodes: u64) -> Foundationˉresult.Result<Arena<T>, Collectionˉfailure> effects(memory.allocate);
export fn Arenaˉconstructˉwithˉfirst<T>(Budget: Foundationˉmemory.Memoryˉbudget, Maximumˉnodes: u64, First: T) -> Foundationˉresult.Result<Arenaˉseed<T>, Arenaˉinsertˉfailure<T>> effects(memory.allocate);
export fn Arenaˉinsert<T>(Arena: borrow mut Arena<T>, Value: T) -> Foundationˉresult.Result<Handle<T>, Arenaˉinsertˉfailure<T>> effects();
export fn Arenaˉreplace<T>(Arena: borrow mut Arena<T>, Handle: borrow Handle<T>, Value: T) -> Foundationˉresult.Result<T, Arenaˉreplaceˉfailure<T>> effects();
export fn Arenaˉremove<T>(Arena: borrow mut Arena<T>, Handle: borrow Handle<T>) -> Foundationˉresult.Result<T, Collectionˉfailure> effects();
export fn Arenaˉvalidate<T>(Arena: borrow Arena<T>, Handle: borrow Handle<T>) -> Foundationˉresult.Result<unit, Collectionˉfailure> effects();
export fn Arenaˉborrowˉvalidated<T>(Arena: borrow Arena<T>, Handle: Handle<T>) -> borrow T effects();
export fn Arenaˉlength<T>(Arena: borrow Arena<T>) -> u64 effects();
export fn Arenaˉfreeze<T>(Arena: Arena<T>) -> Immutableˉarena<T> effects();
export fn Immutableˉarenaˉvalidate<T>(Arena: borrow Immutableˉarena<T>, Handle: borrow Handle<T>) -> Foundationˉresult.Result<unit, Collectionˉfailure> effects();
export fn Immutableˉarenaˉborrowˉvalidated<T>(Arena: borrow Immutableˉarena<T>, Handle: Handle<T>) -> borrow T effects();
export fn Immutableˉarenaˉlength<T>(Arena: borrow Immutableˉarena<T>) -> u64 effects();
end module;
~~~

## Foundation bytes

~~~windvale-foundation-signatures
module Foundationˉbytes major 1;
intrinsic type bytes;
opaque owned type Byteˉbuffer;
opaque owned type Bytesˉbuilder;
export fn Length(Value: borrow bytes) -> u64 effects();
export fn At(Value: borrow bytes, Index: u64) -> u8 effects();
export fn Borrowˉrange(Value: borrow bytes, Start: u64, Length: u64) -> Foundationˉcollections.Slice<u8> effects();
export fn Constructˉbuffer(Budget: Foundationˉmemory.Memoryˉbudget, Length: u64) -> Foundationˉresult.Result<Byteˉbuffer, Foundationˉmemory.Allocationˉfailure> effects(memory.allocate);
export fn Bufferˉlength(Buffer: borrow Byteˉbuffer) -> u64 effects();
export fn Borrowˉslice(Buffer: borrow Byteˉbuffer, Start: u64, Length: u64) -> Foundationˉcollections.Slice<u8> effects();
export fn Borrowˉsliceˉmut(Buffer: borrow mut Byteˉbuffer, Start: u64, Length: u64) -> Foundationˉcollections.Mutableˉslice<u8> effects();
export fn Constructˉreserved(Budget: Foundationˉmemory.Memoryˉbudget, Maximumˉoutputˉbytes: u64) -> Foundationˉresult.Result<Bytesˉbuilder, Foundationˉmemory.Allocationˉfailure> effects(memory.allocate);
export fn Appendˉbytes(Builder: borrow mut Bytesˉbuilder, Value: borrow bytes) -> Foundationˉresult.Result<unit, Foundationˉmemory.Limitˉfailure> effects();
export fn Appendˉutf8(Builder: borrow mut Bytesˉbuilder, Value: borrow text) -> Foundationˉresult.Result<unit, Foundationˉmemory.Limitˉfailure> effects();
export fn Appendˉu8(Builder: borrow mut Bytesˉbuilder, Value: u8) -> Foundationˉresult.Result<unit, Foundationˉmemory.Limitˉfailure> effects();
export fn Appendˉu32ˉlittle(Builder: borrow mut Bytesˉbuilder, Value: u32) -> Foundationˉresult.Result<unit, Foundationˉmemory.Limitˉfailure> effects();
export fn Appendˉu64ˉlittle(Builder: borrow mut Bytesˉbuilder, Value: u64) -> Foundationˉresult.Result<unit, Foundationˉmemory.Limitˉfailure> effects();
export fn Appendˉu64ˉdecimal(Builder: borrow mut Bytesˉbuilder, Value: u64) -> Foundationˉresult.Result<unit, Foundationˉmemory.Limitˉfailure> effects();
export fn Freeze(Builder: Bytesˉbuilder) -> bytes effects();
end module;
~~~

## Foundation text

~~~windvale-foundation-signatures
module Foundationˉtext major 1;
intrinsic type text;
opaque owned type Textˉbuilder;
export variant Decodeˉfailure { Inputˉlimit(Byteˉoffset: u64, Observed: u64, Maximum: u64); Runeˉlimit(Byteˉoffset: u64, Observed: u64, Maximum: u64); Invalidˉlead(Byteˉoffset: u64); Invalidˉcontinuation(Byteˉoffset: u64); Truncated(Byteˉoffset: u64); Overlong(Byteˉoffset: u64); Surrogate(Byteˉoffset: u64); Outˉofˉrange(Byteˉoffset: u64); }
export variant Decodeˉutf8ˉfailure { Allocation(Error: Foundationˉmemory.Allocationˉfailure); Source(Error: Decodeˉfailure); }
export protocol Formatting<T> { fn Maximumˉutf8ˉbytes(Value: borrow T) -> Foundationˉresult.Result<u64, Foundationˉmemory.Limitˉfailure> effects(); fn Append(Builder: borrow mut Textˉbuilder, Value: borrow T) -> Foundationˉresult.Result<unit, Foundationˉmemory.Limitˉfailure> effects(); }
export fn Byteˉlength(Value: borrow text) -> u64 effects();
export fn Runeˉcount(Value: borrow text) -> u64 effects();
export fn Decodeˉutf8ˉreserved(Budget: Foundationˉmemory.Memoryˉbudget, Value: borrow bytes, Maximumˉbytes: u64, Maximumˉrunes: u64) -> Foundationˉresult.Result<text, Decodeˉutf8ˉfailure> effects(memory.allocate);
export fn Decodeˉutf8ˉsliceˉreserved(Budget: Foundationˉmemory.Memoryˉbudget, Value: Foundationˉcollections.Slice<u8>, Maximumˉbytes: u64, Maximumˉrunes: u64) -> Foundationˉresult.Result<text, Decodeˉutf8ˉfailure> effects(memory.allocate);
export fn Decodeˉfailureˉbyteˉoffset(Error: borrow Decodeˉfailure) -> u64 effects();
export fn Runeˉat(Value: borrow text, Index: u64) -> rune effects();
export fn Runeˉutf8ˉwidth(Value: rune) -> u64 effects();
export fn Shareˉrange(Value: borrow text, Startˉrune: u64, Runeˉcount: u64) -> text effects();
export fn Constructˉreserved(Budget: Foundationˉmemory.Memoryˉbudget, Maximumˉoutputˉbytes: u64) -> Foundationˉresult.Result<Textˉbuilder, Foundationˉmemory.Allocationˉfailure> effects(memory.allocate);
export fn Appendˉrune(Builder: borrow mut Textˉbuilder, Value: rune) -> Foundationˉresult.Result<unit, Foundationˉmemory.Limitˉfailure> effects();
export fn Appendˉtext(Builder: borrow mut Textˉbuilder, Value: borrow text) -> Foundationˉresult.Result<unit, Foundationˉmemory.Limitˉfailure> effects();
export fn Appendˉu64ˉdecimal(Builder: borrow mut Textˉbuilder, Value: u64) -> Foundationˉresult.Result<unit, Foundationˉmemory.Limitˉfailure> effects();
export fn Appendˉu32ˉhexˉfixed(Builder: borrow mut Textˉbuilder, Value: u32) -> Foundationˉresult.Result<unit, Foundationˉmemory.Limitˉfailure> effects();
export fn Appendˉf32ˉcanonical(Builder: borrow mut Textˉbuilder, Value: f32) -> Foundationˉresult.Result<unit, Foundationˉmemory.Limitˉfailure> effects();
export fn Appendˉf64ˉcanonical(Builder: borrow mut Textˉbuilder, Value: f64) -> Foundationˉresult.Result<unit, Foundationˉmemory.Limitˉfailure> effects();
export fn Freeze(Builder: Textˉbuilder) -> text effects();
end module;
~~~

## Foundation resource

~~~windvale-foundation-signatures
module Foundationˉresource major 1;
export protocol Localˉrelease<Self> { fn Release(Value: Self) -> unit effects(resource.release); }
export variant Mutationˉoutcome<E> { Rejected(Error: E); Acceptedˉpartial(Completed: u64, Error: E); Completed(Completed: u64); Indeterminate(Error: E); }
end module;
~~~

## Foundation task

~~~windvale-foundation-signatures
module Foundationˉtask major 1;
opaque copy type Operationˉcontext;
opaque owned type Taskˉscope;
opaque owned type Task<T, E>;
export record Taskˉlimits { Maximumˉchildren: u32; Maximumˉrunnable: u32; Maximumˉcompleted: u32; Maximumˉretainedˉbytes: u64; Maximumˉworkˉunits: u64; Maximumˉcallˉdepth: u32; Maximumˉtimers: u32; Maximumˉdiagnostics: u32; }
export variant Taskˉscopeˉfailure { Invalidˉlimits(Field: u32, Observed: u64, Minimum: u64, Maximum: u64); Allocation(Error: Foundationˉmemory.Allocationˉfailure); Parentˉcontextˉstale(Expectedˉgeneration: u64, Observedˉgeneration: u64); Runtimeˉunavailable(Expectedˉgeneration: u64, Observedˉgeneration: u64); }
export variant Taskˉoutcome<T, E> { Valid(Value: T); Failure(Error: E); Cancelled; Deadlineˉreached; Runtimeˉlost(Expectedˉgeneration: u64, Observedˉgeneration: u64); Runtimeˉrestarted(Expectedˉgeneration: u64, Observedˉgeneration: u64); Trapped(Identity: u32); }
export variant Spawnˉfailure<W> { Scopeˉclosing(Work: W); Taskˉlimit(Work: W); Queueˉlimit(Work: W); Memoryˉfailure(Error: Foundationˉmemory.Allocationˉfailure, Work: W); }
export variant Cancelˉrequestˉoutcome { Requested(Liveˉchildren: u32); Alreadyˉrequested(Liveˉchildren: u32); }
export fn Construct(Budget: Foundationˉmemory.Memoryˉbudget, Limits: Taskˉlimits, Parentˉcontext: borrow Operationˉcontext) -> Foundationˉresult.Result<Taskˉscope, Taskˉscopeˉfailure> effects(memory.allocate, resource.acquire);
export fn Operationˉcontext(Scope: borrow Taskˉscope) -> Operationˉcontext effects();
family Spawnˉclosureˉrelation {
    For exact W = async fn() -> Foundationˉresult.Result<T, E> effects(F):
    export fn Spawn(Scope: borrow mut Taskˉscope, Work: W) -> Foundationˉresult.Result<Task<T, E>, Spawnˉfailure<W>> effects(memory.allocate, task.spawn);
    Solve W only from Work; decompose T, E, and finite F structurally from W; require caller and module to admit F; never use result context or overload search;
}
export async fn Await<T, E>(Handle: Task<T, E>) -> Taskˉoutcome<T, E> effects(task.suspend);
export fn Requestˉcancel(Scope: borrow mut Taskˉscope) -> Cancelˉrequestˉoutcome effects(task.cancel);
end module;
~~~

## Foundation unsafe

~~~windvale-foundation-signatures
module Foundationˉunsafe major 1;
opaque copy type Rawˉaddress<Scope>;
opaque borrowed type Foreignˉpointer<T, Abi>;
opaque borrowed type Nullableˉforeignˉpointer<T, Abi>;
opaque copy type Volatileˉpointer<T, Scope>;
opaque owned type Dmaˉregion<Device, Generation>;
opaque owned type Foreignˉscratch<Abi>;
opaque borrowed type Foreignˉwriteˉregion<Abi>;
export variant Foreignˉmemoryˉfailure { Invalidˉlength(Observed: u64, Maximum: u64); Invalidˉalignment(Observed: u64); Allocation(Error: Foundationˉmemory.Allocationˉfailure); Unsupportedˉabi; }
export variant Foreignˉpointerˉfailure { Null; Addressˉoverflow(Start: u64, Length: u64, Addressˉbits: u32); Outˉofˉrange(Start: u64, Length: u64, Ownerˉlength: u64); Misaligned(Start: u64, Requiredˉalignment: u64); Aliasing; Lifetimeˉended; Unsupportedˉabi; }
export unsafe fn Requireˉnonˉnull<T, Abi>(Value: Nullableˉforeignˉpointer<T, Abi>) -> Foundationˉresult.Result<Foreignˉpointer<T, Abi>, Foreignˉpointerˉfailure> effects(unsafe.address);
export fn Constructˉscratch<Abi>(Budget: Foundationˉmemory.Memoryˉbudget, Length: u64, Alignment: u64) -> Foundationˉresult.Result<Foreignˉscratch<Abi>, Foreignˉmemoryˉfailure> effects(memory.allocate);
export fn Scratchˉlength<Abi>(Scratch: borrow Foreignˉscratch<Abi>) -> u64 effects();
export unsafe fn Borrowˉwriteˉregion<Abi>(Scratch: borrow mut Foreignˉscratch<Abi>, Start: u64, Length: u64, Requiredˉalignment: u64) -> Foundationˉresult.Result<Foreignˉwriteˉregion<Abi>, Foreignˉpointerˉfailure> effects(unsafe.address);
export unsafe fn Writeˉpointer<Abi>(Region: borrow Foreignˉwriteˉregion<Abi>) -> Foreignˉpointer<u8, Abi> effects(unsafe.address);
export fn Regionˉlength<Abi>(Region: borrow Foreignˉwriteˉregion<Abi>) -> u64 effects();
export fn Borrowˉscratchˉslice<Abi>(Scratch: borrow Foreignˉscratch<Abi>, Start: u64, Length: u64) -> Foundationˉcollections.Slice<u8> effects();
end module;
~~~

## Compatibility rule

A consumer records the module name, major `1`, and exact signature-set identity.
A later implementation is compatible only when it supplies every declaration
in the selected block with identical resolved types, ownership, effects,
generic relations, and failure types. Adding, removing, or changing a public
declaration produces a different identity even when the major remains `1`.
Semantic corrections that alter observable behavior require a new identity and,
when incompatible, a new major contract.
