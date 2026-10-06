import { Buildˉownedˉdomainˉfixture } from './Native-Owned-Domain-Cases.mjs';
import { createHash } from 'node:crypto';
import { lstat, mkdir, open, realpath, writeFile } from 'node:fs/promises';
import { basename, dirname, join, resolve } from 'node:path';
import { Prepareˉassemblyˉobjectˉcache, Acquireˉassemblyˉobject } from './Native-Assembly-Object-Cache-Core.mjs';
import { Buildˉsharedˉnativeˉconsumer, Parseˉstagedˉnativeˉmanifest } from './Build-Shared-Compiler-Host.mjs';

export async function Readˉsharedˉvalueˉtemplate(Context, Valueˉobject) {
    const { Repository, Work, Target, Requireˉsuccess } = Context;
    const Tool = Name => join(Repository, 'Tools/Native', Name + (Target === 'windows' ? '.cmd' : '.sh'));
    const Stub = join(Work, 'Shared-template-sha.wva'), Object = join(Work, 'Shared-template-sha.wvo');
    const Image = join(Work, 'Shared-template.bin');
    // Resolve the sole external call at the same aligned offset as the real SHA
    // suffix. The stub is never executed; the checked native linker owns rel32.
    await writeFile(Stub, 'windvale-assembly 1\nsymbol export function Windvale_shared_sha256 in .text\n' +
        'section code .text align 16\ndefine Windvale_shared_sha256\nreturn\nend define\nend section\n');
    if (Context.Valuesˉonly) {
        const Cache = await Prepareˉassemblyˉobjectˉcache(Context.Deadline);
        await Acquireˉassemblyˉobject(Cache, Stub, Object);
        await Cache.Requireˉunchanged();
    } else {
        await Requireˉsuccess(Tool('Assemble-Wva'), [Stub, Object], 'shared-template-stub');
    }
    const Linked = await Requireˉsuccess(Tool('Link-Wvo'),
        ['0', 'Windvale_shared_value_operation', Image, Valueˉobject, Object], 'shared-template-link');
    const Section = /^section index=0 input=0 source-index=0 kind=code name=\.text image-offset=0 address=0 memory-bytes=(\d+) data-bytes=(\d+) alignment=16$/mu.exec(Linked.Output);
    const Suffix = /^symbol .* binding=export kind=function name=Windvale_shared_sha256 address=(\d+) size=1$/mu.exec(Linked.Output);
    const Length = Number(Section?.[1]), Offset = Number(Suffix?.[1]);
    Sourceˉrequire(Section !== null && Section[1] === Section[2] && Length > 0 && Length < 65_536 &&
        Offset === Math.ceil(Length / 16) * 16 && /^entry name=Windvale_shared_value_operation address=0$/mu.test(Linked.Output),
    'Shared value template link geometry differs.');
    const Bytes = await Sourceˉread(Image, 65_537);
    Sourceˉrequire(Bytes.length === Offset + 1 && Bytes[Offset] === 195, 'Shared value SHA stub layout differs.');
    return { Value: Bytes.subarray(0, Length), Padding: Buffer.alloc(Offset - Length, 144), Offset };
}

export async function Checkˉsharedˉvalueˉtemplate(Context, Valueˉobject) {
    const Actual = await Readˉsharedˉvalueˉtemplate(Context, Valueˉobject);
    const Source = (await Sourceˉread(join(Context.Repository,
        'Compiler/Windvale/Native-X64-Lowering-Shared-Templates.wv'), 262_144)).toString('utf8');
    for (const [Name, Expected] of [['Valueˉtemplate', Actual.Value], ['Valueˉpadding', Actual.Padding]]) {
        const Matches = [...Source.matchAll(new RegExp('data ' + Name + ': bytes = \\[([\\s\\S]*?)\\];', 'gu'))];
        Sourceˉrequire(Matches.length === 1 && /^[\d,\s]*$/u.test(Matches[0][1]), 'Shared template literal differs.');
        const Values = Matches[0][1].split(',').map(Value => Value.trim()).filter(Boolean).map(Number);
        Sourceˉrequire(Values.every(Value => Number.isInteger(Value) && Value >= 0 && Value <= 255) &&
            Buffer.from(Values).equals(Expected), 'Shared value template differs from assembled runtime source: ' + Name);
    }
    const Total = Actual.Offset + 1640;
    for (const Text of [`Bytesˉlength(Input) != ${Total}u32`, `while Index < ${Actual.Value.length}u32`,
        `while Index < ${Actual.Offset}u32`, `Bytesˉslice(Input, ${Actual.Offset}u32, 1640u32)`,
        `export fn Helperˉbytes() -> u32 { return ${Total}u32; }`]) {
        Sourceˉrequire(Source.includes(Text), 'Shared template extent differs: ' + Text);
    }
    process.stdout.write(`native shared value template status=Passed value-bytes=${Actual.Value.length} helper-bytes=${Total}\n`);
}

export function Checkˉsharedˉstagingˉmanifest() {
    let Cases = 0;
    function Manifest(Lengths) {
        const Bytes = Buffer.alloc(24 + Lengths.length * 12);
        Bytes.write('WVOP', 0, 'ascii');
        [1, Bytes.length, Lengths.reduce((Total, Length) => Total + Length, 0), Lengths.length, 4_194_304]
            .forEach((Value, Index) => Bytes.writeUInt32LE(Value, 4 + Index * 4));
        let Position = 0;
        Lengths.forEach((Length, Index) => {
            const At = 24 + Index * 12;
            Bytes.writeUInt32LE(Index, At); Bytes.writeUInt32LE(Position, At + 4);
            Bytes.writeUInt32LE(Length, At + 8); Position += Length;
        });
        return Bytes;
    }
    function Accept(Lengths) {
        const Actual = Parseˉstagedˉnativeˉmanifest(Manifest(Lengths));
        Sourceˉrequire(JSON.stringify(Actual) === JSON.stringify(Lengths), 'Staged host manifest geometry differs.');
        Cases++;
    }
    function Refuse(Bytes) {
        let Rejected = false;
        try { Parseˉstagedˉnativeˉmanifest(Bytes); } catch { Rejected = true; }
        Sourceˉrequire(Rejected, 'Malformed staged host manifest was admitted.'); Cases++;
    }
    for (const Lengths of [[1], [4_194_304], Array(8).fill(4_194_304),
        [...Array(8).fill(4_194_304), 1], Array(16).fill(4_194_304), Array(63).fill(1), Array(518).fill(1)])
        Accept(Lengths);
    Refuse(Manifest([...Array(16).fill(4_194_304), 1]));
    Refuse(Manifest([])); Refuse(Manifest(Array(519).fill(1)));
    Refuse(Manifest([0])); Refuse(Manifest([4_194_305]));
    const Valid = Manifest([2, 3]);
    for (const Length of [0, 3, 23, 24, Valid.length - 1]) Refuse(Valid.subarray(0, Length));
    Refuse(Buffer.concat([Valid, Buffer.alloc(1)]));
    for (const [At, Value] of [[0, 0], [4, 2], [8, 0], [12, 0], [12, 4], [12, 6],
        [16, 1], [20, 1], [24, 1], [28, 1], [36, 0], [40, 3], [44, 0], [44, 0xffff_ffff]]) {
        const Changed = Buffer.from(Valid); Changed.writeUInt32LE(Value, At); Refuse(Changed);
    }
    return Cases;
}

// Exercise the production value bridge inside the existing storage owner.
// These cases do not select SHA; a poison import makes accidental selection fail.
const VALUE_REQUEST = 9600;
const PARENT = 9696;
const CHILD = 9712;
const BUILDER = 9728;
const BYTES = 9744;
const ALIAS = 9760;
const VIEW = 9776;
const SCALAR = 9792;
const RESULT = 9808;
const COUNTER = 9920;

function Fixture(Name, Body) {
    return Buildˉownedˉdomainˉfixture('shared-value-' + Name, Context => {
        const { Emit, Set, Check, Initialize, Shared, CONTEXT, PHYSICAL, ACCOUNTING, ADAPTER } = Context;
        Initialize();
        Emit(`load_memory_u64 rax rsp none 1 ${CONTEXT + 184}`,
            `store_memory_u64 rsp none 1 ${PARENT} rax`, 'xor eax eax',
            `store_memory_u64 rsp none 1 ${CONTEXT + 184} rax`);
        Set(PARENT + 8, 1);
        let Sequence = 0;
        function Value(Action, Destination = null, Source = null, Argument = null,
            Numberˉzero = 0n, Numberˉone = 0n, Status = 0) {
            const Label = 'Clear_value_' + Sequence++;
            Emit('xor eax eax', `move_u32 ecx ${VALUE_REQUEST}`, `label ${Label}`,
                'store_memory_u64 rsp rcx 1 0 rax', 'add_i32 ecx 8',
                `compare_i32 ecx ${VALUE_REQUEST + 64}`, `branch below ${Label}`);
            for (const [Field, Number] of [[0, 1], [4, 64], [8, Action]]) Set(VALUE_REQUEST + Field, Number);
            for (const [Field, Cell] of [[16, Destination], [24, Source], [32, Argument]]) {
                if (Cell !== null) Emit('move rax rsp', `add_i32 rax ${Cell}`,
                    `store_memory_u64 rsp none 1 ${VALUE_REQUEST + Field} rax`);
            }
            for (const [Field, Operand] of [[40, Numberˉzero], [48, Numberˉone]]) {
                const Wide = BigInt(Operand);
                Set(VALUE_REQUEST + Field, Number(Wide & 0xffff_ffffn));
                Set(VALUE_REQUEST + Field + 4, Number(Wide >> 32n));
            }
            Emit('move r8 rsp', `add_i32 r8 ${CONTEXT}`, 'move r9 rsp',
                `add_i32 r9 ${VALUE_REQUEST}`, 'call Windvale_shared_value_operation');
            if (Status === null) Emit('test eax eax', 'branch equal Failed');
            else Emit(`compare_i32 eax ${Status}`, 'branch not_equal Failed');
        }
        function Reserve(Maximum = 8, Charge = 32) {
            Value(6, RESULT, PARENT, null, BigInt(Charge), 17n);
            Check(RESULT, 0); Value(17, CHILD, RESULT + 16);
            Value(229, RESULT, CHILD, null, BigInt(Maximum));
            Check(RESULT, 0); Check(CHILD, 0); Check(CHILD + 8, 0);
            Value(9, BUILDER, RESULT + 16); Check(RESULT + 16, 0); Check(RESULT + 24, 0);
        }
        function Freeze() {
            Value(236, BYTES, BUILDER); Check(BUILDER, 0); Check(BUILDER + 8, 0);
        }
        Body({ ...Context, Check: (Offset, Expected) => Check(Offset, Expected | 0), Value, Reserve, Freeze });
        Value(8, null, PARENT); Check(PARENT, 0); Check(PARENT + 8, 0);
        Shared(11);
        Check(PHYSICAL + 56, 0); Check(ACCOUNTING + 24, 0); Check(ADAPTER + 40, 1);
    }, {
        Symbols: ['symbol export function Windvale_shared_sha256 in .text',
            'symbol import function Windvale_shared_value_operation'],
        Definitions: ['define Windvale_shared_sha256', 'trap', 'end define'],
        ExtraRuntime: 'Value',
    });
}

export function Buildˉsharedˉvalueˉcases() {
    const Cases = [];
    const Case = (Name, Body) => Cases.push(Fixture(Name, Body));
    Case('builder-and-scalar', ({ Value, Reserve, Freeze, Check, PHYSICAL }) => {
        Value(120, BYTES, null, null, 255n);
        Value(13, SCALAR, BYTES); Check(SCALAR, 255); Value(1, null, BYTES);
        Value(121, BYTES, null, null, 65535n);
        Value(14, SCALAR, BYTES); Check(SCALAR, 65535);
        Value(121, BYTES, null, null, 65536n, 0n, 7);
        Value(14, SCALAR, BYTES); Check(SCALAR, 65535); Value(1, null, BYTES);
        Value(122, BYTES, null, null, 0xffff_fffen);
        Value(15, SCALAR, BYTES); Check(SCALAR, -2); Value(1, null, BYTES);
        for (const Word of [0n, 0x8000_0000n, 0xffff_ffffn, 0x8877_6655_4433_2211n]) {
            Value(123, BYTES, null, null, Word); Check(BYTES + 8, 4);
            Value(15, SCALAR, BYTES); Check(SCALAR, Number(Word & 0xffff_ffffn));
            Value(1, null, BYTES); Check(PHYSICAL + 56, 0);
        }
        for (const Wide of [0n, 0x8877_6655_4433_2211n, 0xffff_ffff_ffff_ffffn]) {
            Value(190, BYTES, null, null, Wide); Check(BYTES + 8, 8);
            Value(189, SCALAR, BYTES);
            Check(SCALAR, Number(Wide & 0xffff_ffffn)); Check(SCALAR + 4, Number(Wide >> 32n));
            Value(1, null, BYTES); Check(PHYSICAL + 56, 0);
        }
        // Immutable module mappings use the same generation/range path as backing.
        Value(4, BYTES, null, null, 0n); Check(BYTES + 8, 4);
        Value(2, ALIAS, BYTES); Value(1, null, BYTES);
        Value(13, SCALAR, ALIAS, null, 3n); Check(SCALAR, 100);
        Value(13, SCALAR, ALIAS, null, 4n, 0n, 7); Check(SCALAR, 100);
        Value(1, null, ALIAS); Check(PHYSICAL + 56, 0);
        Reserve(0, 32); Freeze(); Check(BYTES + 8, 0); Check(BYTES + 12, 0);
        Check(PHYSICAL + 56, 32); Value(1, null, BYTES); Check(PHYSICAL + 56, 0);
        Reserve(16, 32);
        Value(230, RESULT, BUILDER, null, 65n); Check(RESULT, 0);
        Value(231, RESULT, BUILDER, null, 0x4433_2211n); Check(RESULT, 0);
        Value(232, RESULT, BUILDER, null, 0x8877_6655_4433_2211n); Check(RESULT, 0);
        Freeze(); Check(BYTES + 8, 13); Check(BYTES + 12, 16);
        Value(237, SCALAR, BYTES); Check(SCALAR, 13);
        Value(13, SCALAR, BYTES, null, 0n); Check(SCALAR, 65);
        Value(15, SCALAR, BYTES, null, 1n); Check(SCALAR, 0x4433_2211);
        Value(189, SCALAR, BYTES, null, 5n); Check(SCALAR, 0x4433_2211); Check(SCALAR + 4, 0x8877_6655);
        Value(1, null, BYTES); Check(PHYSICAL + 56, 0);
    });
    Case('aliases-and-owning-range', ({ Value, Reserve, Freeze, Check, PHYSICAL }) => {
        Reserve(); Value(232, RESULT, BUILDER, null, 0x8877_6655_4433_2211n); Freeze();
        Value(2, ALIAS, BYTES); Value(12, VIEW, ALIAS, null, 1n, 2n);
        Value(1, null, BYTES); Value(1, null, ALIAS); Check(PHYSICAL + 56, 32);
        Value(13, SCALAR, VIEW, null, 0n); Check(SCALAR, 0x22);
        Value(3, ALIAS, VIEW); Check(VIEW, 0); Check(VIEW + 8, 0); Check(VIEW + 12, 0);
        Value(13, SCALAR, ALIAS, null, 1n); Check(SCALAR, 0x33);
        Value(1, null, ALIAS); Check(PHYSICAL + 56, 0);
    });
    Case('append-refusal-preserves-builder', ({ Value, Reserve, Freeze, Check }) => {
        Reserve(); Value(232, RESULT, BUILDER, null, 7n);
        Value(230, RESULT, BUILDER, null, 9n); Check(RESULT, 1);
        Check(RESULT + 32, 0); Check(RESULT + 64, 9); Check(RESULT + 80, 8);
        Freeze(); Value(237, SCALAR, BYTES); Check(SCALAR, 8);
        Value(13, SCALAR, BYTES, null, 0n); Check(SCALAR, 7);
        Value(1, null, BYTES);
    });
    Case('constructor-refusal-credits-parent', ({ Value, Check, PHYSICAL }) => {
        Value(6, RESULT, PARENT, null, 16n, 17n); Check(RESULT, 0);
        Value(17, CHILD, RESULT + 16);
        Value(229, RESULT, CHILD, null, 8n); Check(RESULT, 1);
        Check(RESULT + 32, 1); Check(RESULT + 48, 32); Check(RESULT + 64, 16);
        Check(CHILD, 0); Check(CHILD + 8, 0); Check(PHYSICAL + 56, 0);
        Value(16, SCALAR, PARENT); Check(SCALAR, 48);
    });
    Case('borrowed-builder-cell', ({ Value, Reserve, Freeze, Check, PHYSICAL }) => {
        Reserve(); Value(10, ALIAS, BUILDER); Check(ALIAS + 8, 0); Check(BUILDER + 8, 1);
        Value(230, RESULT, ALIAS, null, 41n); Check(RESULT, 0);
        Value(236, BYTES, ALIAS, null, 0n, 0n, 1); Check(BYTES, 0);
        Value(7, null, ALIAS); Check(ALIAS, 0); Check(PHYSICAL + 56, 32);
        Freeze(); Value(237, SCALAR, BYTES); Check(SCALAR, 1);
        Value(1, null, BYTES); Check(PHYSICAL + 56, 0);
    });
    Case('slice-generation', ({ Value, Reserve, Freeze, Check }) => {
        Reserve(); Value(230, RESULT, BUILDER, null, 19n); Freeze();
        Value(239, VIEW, BYTES, null, 0n, 1n);
        Value(240, SCALAR, VIEW, null, 0n); Check(SCALAR, 19);
        Value(241, SCALAR, VIEW); Check(SCALAR, 1);
        Value(1, null, BYTES);
        Value(240, SCALAR, VIEW, null, 0n, 0n, null); Check(SCALAR, 1);
        Reserve(); Value(230, RESULT, BUILDER, null, 29n); Freeze();
        Value(240, SCALAR, VIEW, null, 0n, 0n, null); Check(SCALAR, 1);
        Value(13, SCALAR, BYTES, null, 0n); Check(SCALAR, 29);
        Value(1, null, BYTES);
    });
    Case('malformed-cells-preserve-owner', ({ Value, Reserve, Freeze, Check, Set, Emit, PHYSICAL, SHARED }) => {
        Reserve(); Value(230, RESULT, BUILDER, null, 23n); Freeze(); Value(2, ALIAS, BYTES);
        Set(BYTES + 12, 0); // length exceeds maximum: reject before dropping destination.
        Value(2, ALIAS, BYTES, null, 0n, 0n, 1); Check(PHYSICAL + 56, 32);
        Set(BYTES + 12, 8); Value(13, SCALAR, ALIAS, null, 0n); Check(SCALAR, 23);
        function Shares(Count) {
            Emit(`load_memory_u32 ecx rsp none 1 ${BYTES}`, 'and_i32 ecx 127',
                'subtract_i32 ecx 1', 'shift_left ecx 5', `add_i32 ecx ${SHARED + 64 + 8}`,
                `move_u32 eax ${Count}`, 'store_memory_u32 rsp rcx 1 0 eax');
        }
        Shares(0xffff_ffff);
        Value(2, VIEW, BYTES, null, 0n, 0n, 6); Check(VIEW, 0); Check(PHYSICAL + 56, 32);
        Shares(2);
        Value(1, null, BYTES); Value(1, null, ALIAS);
        Emit('xor eax eax', `store_memory_u64 rsp none 1 ${CHILD} rax`);
        Set(CHILD + 8, 2);
        Value(229, RESULT, CHILD, null, 8n, 0n, 1); Check(CHILD + 8, 2);
        Set(CHILD + 8, 0);
    });
    for (const Iterations of [1, 1000, 32768]) {
        Case('fixed-live-state-' + Iterations, ({ Emit, Set, Check, Value, Reserve, Freeze, PHYSICAL }) => {
            Set(COUNTER, Iterations); Emit('label Reuse');
            Reserve(); Value(232, RESULT, BUILDER, null, 11n); Freeze();
            Value(2, ALIAS, BYTES); Value(1, null, BYTES);
            Value(13, SCALAR, ALIAS, null, 0n); Check(SCALAR, 11); Check(PHYSICAL + 56, 32);
            Value(1, null, ALIAS); Check(PHYSICAL + 56, 0);
            Value(16, SCALAR, PARENT); Check(SCALAR, 48);
            Value(123, BYTES, null, null, 0x4433_2211n);
            Value(2, ALIAS, BYTES); Value(1, null, BYTES);
            Value(15, SCALAR, ALIAS); Check(SCALAR, 0x4433_2211);
            Check(PHYSICAL + 56, 32); Value(1, null, ALIAS); Check(PHYSICAL + 56, 0);
            Value(190, BYTES, null, null, 0x8877_6655_4433_2211n);
            Value(189, SCALAR, BYTES); Check(SCALAR, 0x4433_2211); Check(SCALAR + 4, 0x8877_6655);
            Value(1, null, BYTES); Check(PHYSICAL + 56, 0);
            Emit(`load_memory_u32 eax rsp none 1 ${COUNTER}`, 'subtract_i32 eax 1',
                `store_memory_u32 rsp none 1 ${COUNTER} eax`, 'test eax eax', 'branch not_equal Reuse');
        });
    }
    return Cases;
}

const SOURCE_INPUT = Buffer.from([6, 7, 8, 9]);
const SOURCE_MODULES = Object.freeze([
    'Libraries/Foundation/Bytes/Bytes.wv', 'Libraries/Foundation/Memory/Memory.wv',
    'Libraries/Foundation/Collections/Collections.wv', 'Libraries/Foundation/Values/Result.wv',
]);
const SOURCE_LOCK = 'Documents/Project/Language-1.0-Localization-Workloads/01-Source-Profile-Admission/Reference-Artifacts/Source-Inputs.wvlock';
const SOURCE_PROFILE = 'Documents/Project/Language-1.0-Localization-Workloads/01-Source-Profile-Admission/Reference-Artifacts/En-Source-Profile.wvsp';
const SOURCE_LOCK_SHA256 = '9e2ca572552ed52ed496142d18539f2f55fed2bbdfb1ec602f283b5d72386f3e';
const SOURCE_PREFIX = `#!wv/1 en@1
module Nativeˉsharedˉsourceˉfixture;
profile core;
platform linux, windows, windvale;
authority application;
import Foundationˉbytes as Bytes;
import Foundationˉmemory as Memory;
import Foundationˉcollections as Collections;
import Foundationˉresult as Results;
`;

export function Buildˉsharedˉsourceˉcases() {
    const Cases = [];
    function Case(Name, Body, Expected, Maximum, Physicalˉhighˉwater, Iterations = 1,
        Returnedˉphysicalˉcharge = Physicalˉhighˉwater, Trapˉstatus = 0,
        Expectedˉphysicalˉreservations = null, Wvbˉminor = 44) {
        Cases.push(Object.freeze({ Name, Source: SOURCE_PREFIX + Body + '\n',
            Expected: Buffer.from(Expected), Maximum, Physicalˉhighˉwater,
            Returnedˉphysicalˉcharge, Iterations, Trapˉstatus, Expectedˉphysicalˉreservations, Wvbˉminor }));
    }
    const Word = Buffer.alloc(4); Word.writeUInt32LE(0x44332211);
    const Wide = Buffer.alloc(8); Wide.writeBigUInt64LE(0x8877665544332211n);
    Case('byte-family', `
fn Markˉbyte(Builder: borrow mut Bytes.Bytesˉbuilder, Value: u8) -> bool {
    let Added = Bytes.Appendˉu8(borrow mut Builder, Value);
    return match Added {
        case Results.Result.Valid { Value: Accepted } { true }
        case Results.Result.Failure { Error: Refused } { false }
    };
}
fn Markˉword(Builder: borrow mut Bytes.Bytesˉbuilder, Value: u32) -> bool {
    let Added = Bytes.Appendˉu32ˉlittle(borrow mut Builder, Value);
    return match Added {
        case Results.Result.Valid { Value: Accepted } { true }
        case Results.Result.Failure { Error: Refused } { false }
    };
}
export fn Main(Input: borrow bytes, Budget: Memory.Memoryˉbudget) -> bytes effects(memory.allocate) {
    let Created = Bytes.Constructˉreserved(Budget, 48u64);
    return match Created {
        case Results.Result.Valid { Value: Initial } {
            var Builder = Initial;
            let Message: text = "wv1";
            if (true || Markˉbyte(borrow mut Builder, 0u8)) &&
                (Markˉbyte(borrow mut Builder, 42u8) &&
                    (false || Markˉword(borrow mut Builder, 1144201745u32))) {
            } else { return Input; }
            let C = Bytes.Appendˉu64ˉlittle(borrow mut Builder, 9833440827789222417u64);
            let D = Bytes.Appendˉu64ˉdecimal(borrow mut Builder, 18446744073709551615u64);
            let E = Bytes.Appendˉbytes(borrow mut Builder, borrow Input);
            let F = Bytes.Appendˉutf8(borrow mut Builder, borrow Message);
            Bytes.Freeze(Builder)
        }
        case Results.Result.Failure { Error: Allocationˉerror } { Input }
    };
}`, Buffer.concat([Buffer.from([42]), Word, Wide,
        Buffer.from('18446744073709551615'), SOURCE_INPUT, Buffer.from('wv1')]), 48, 64);
    Case('helper-transfers-and-borrowed-returns', `
data Copyˉfailed: bytes = [0];
fn Supply(Value: Memory.Memoryˉbudget) -> Memory.Memoryˉbudget { return Value; }
fn Forwardˉbuilder(Value: Bytes.Bytesˉbuilder) -> Bytes.Bytesˉbuilder { return Value; }
fn Observe(Value: borrow Bytes.Bytesˉbuilder) -> unit { return (); }
fn Fill(Value: borrow mut Bytes.Bytesˉbuilder) -> unit {
    let Added = Bytes.Appendˉu64ˉlittle(borrow mut Value, 42u64); return ();
}
fn Finish(Value: Bytes.Bytesˉbuilder) -> bytes { return Bytes.Freeze(Value); }
fn Range(Value: borrow bytes) -> Collections.Slice<u8> {
    return Bytes.Borrowˉrange(borrow Value, 0u64, 1u64);
}
fn Forwardˉslice(Value: Collections.Slice<u8>) -> Collections.Slice<u8> { return Value; }
fn Readˉnarrow(Value: bytes, First: bool) -> u32 {
    if First { return U32ˉfromˉu8(Bytes.At(borrow Value, 0u64)); }
    return U32ˉfromˉu8(Bytes.At(borrow Value, 1u64));
}
fn Readˉwide(Value: bytes, First: bool) -> u64 {
    if First { return Bytes.Length(borrow Value); }
    if Bytes.Length(borrow Value) == 8u64 { return 18446744073709551615u64; }
    return 0u64;
}
record Borrowˉpair { First: bytes; Second: bytes; }
fn Observeˉexpressions(First: borrow bytes, Second: borrow bytes) -> bool {
    return Bytes.Length(borrow First) == 8u64 && Bytes.At(borrow First, 0u64) == 42u8 &&
        Bytes.Length(borrow Second) == 1u64 && Bytes.At(borrow Second, 0u64) == 42u8;
}
fn Six(A: bytes, B: borrow bytes, C: Collections.Slice<u8>, D: u32, E: bytes, F: borrow bytes) -> bytes {
    let First = Bytes.At(borrow B, 0u64);
    let Width = Bytes.Length(borrow B);
    let Element = C[0u64];
    let Last = Bytes.At(borrow F, 0u64);
    var Count: u32 = 0u32;
    for Item in C { if Item == 42u8 { Count = Count + 1u32; } }
    if First == 42u8 && Width == 8u64 && Element == 42u8 && Count == 1u32 &&
        D == 7u32 && Last == 6u8 { return A; }
    return E;
}
fn Copy(Source: bytes, Budget: Memory.Memoryˉbudget) -> bytes effects(memory.allocate) {
    let Created = Bytes.Constructˉreserved(Budget, 8u64);
    return match Created {
        case Results.Result.Valid { Value: Initial } {
            var Builder = Initial;
            let Added = Bytes.Appendˉbytes(borrow mut Builder, borrow Source);
            match Added {
                case Results.Result.Valid { Value: Accepted } {
                    let Copied = Bytes.Freeze(Builder);
                    if Bytes.At(borrow Source, 0u64) != 42u8 { return Copyˉfailed; }
                    Copied
                }
                case Results.Result.Failure { Error: Limitˉerror } { Copyˉfailed }
            }
        }
        case Results.Result.Failure { Error: Allocationˉerror } { Copyˉfailed }
    };
}
fn Buildˉandˉcopy(Parent: borrow mut Memory.Memoryˉbudget, Input: borrow bytes) -> bytes effects(memory.allocate) {
    let Split = Memory.Split(borrow mut Parent, 32u64, 17u32);
    return match Split {
        case Results.Result.Valid { Value: Child } {
            let Created = Bytes.Constructˉreserved(Supply(Child), 8u64);
            match Created {
                case Results.Result.Valid { Value: Initial } {
                    var Builder = Forwardˉbuilder(Initial);
                    Observe(borrow Builder);
                    Fill(borrow mut Builder);
                    let Frozen = Finish(Builder);
                    if Readˉnarrow(Frozen, true) != 42u32 || Readˉnarrow(Frozen, false) != 0u32 ||
                        Readˉwide(Frozen, true) != 8u64 ||
                        Readˉwide(Frozen, false) != 18446744073709551615u64 { return Copyˉfailed; }
                    let Pair = Borrowˉpair { First: Frozen, Second: Frozen };
                    if !Observeˉexpressions(borrow Pair.First, borrow Bytesˉslice(Pair.Second, 0u32, 1u32)) {
                        return Copyˉfailed;
                    }
                    let View = Forwardˉslice(Range(borrow Frozen));
                    let Checked = Six(Frozen, borrow Frozen, View, 7u32, Input, borrow Input);
                    if Bytes.Length(borrow Checked) != 8u64 { return Copyˉfailed; }
                    let Copyˉsplit = Memory.Split(borrow mut Parent, 32u64, 0u32);
                    match Copyˉsplit {
                        case Results.Result.Valid { Value: Copyˉchild } {
                            let Copied = Copy(Checked, Copyˉchild);
                            if Bytes.Length(borrow Copied) != 8u64 ||
                                Bytes.At(borrow Frozen, 0u64) != 42u8 ||
                                Bytes.At(borrow Checked, 0u64) != 42u8 || View[0u64] != 42u8 {
                                return Copyˉfailed;
                            }
                            // Only B escapes. Every owner and borrowed alias of A
                            // is local to this function and closes on its return.
                            Copied
                        }
                        case Results.Result.Failure { Error: Copyˉerror } { Copyˉfailed }
                    }
                }
                case Results.Result.Failure { Error: Allocationˉerror } { Copyˉfailed }
            }
        }
        case Results.Result.Failure { Error: Splitˉerror } { Copyˉfailed }
    };
}
fn Forwardˉbudget(Parent: borrow mut Memory.Memoryˉbudget, Input: borrow bytes) -> bytes effects(memory.allocate) {
    return Buildˉandˉcopy(borrow mut Parent, borrow Input);
}
fn Reuse(Budget: Memory.Memoryˉbudget, Value: borrow bytes) -> bool effects(memory.allocate) {
    let Created = Bytes.Constructˉreserved(Budget, 8u64);
    return match Created {
        case Results.Result.Valid { Value: Initial } {
            var Builder = Initial;
            let Added = Bytes.Appendˉu8(borrow mut Builder, 99u8);
            let Reused = Bytes.Freeze(Builder);
            Bytes.Length(borrow Reused) == 1u64 && Bytes.At(borrow Reused, 0u64) == 99u8 &&
                Bytes.At(borrow Value, 0u64) == 42u8
        }
        case Results.Result.Failure { Error: Allocationˉerror } { false }
    };
}
export fn Main(Input: borrow bytes, Budget: Memory.Memoryˉbudget) -> bytes effects(memory.allocate) {
    var Parent = Budget;
    let Copied = Forwardˉbudget(borrow mut Parent, borrow Input);
    if Bytes.Length(borrow Copied) != 8u64 { return Copyˉfailed; }
    let Reuseˉsplit = Memory.Split(borrow mut Parent, 32u64, 0u32);
    return match Reuseˉsplit {
        case Results.Result.Valid { Value: Reuseˉchild } {
            if !Reuse(Reuseˉchild, borrow Copied) { return Copyˉfailed; }
            Copied
        }
        case Results.Result.Failure { Error: Reuseˉerror } { Copyˉfailed }
    };
}`, [42, 0, 0, 0, 0, 0, 0, 0], 8, 64, 1, 32, 0, null, 45);
    Case('record-and-variant-aliases', `
data Textˉfailed: bytes = [0];
${Array.from({ length: 80 }, (_, Index) => `record Padding${Index} { Value: i32; }`).join('\n')}
record Pair { Left: bytes; Right: bytes; Message: text; }
variant Choice { Full(Value: Pair, Shadow: bytes); Empty; }
fn Message(Prefix: text) -> text { return Textˉconcat(Prefix, "v1"); }
fn Forwardˉtext(Value: text) -> text { return Value; }
fn Box(Value: bytes, Label: text) -> Pair {
    return Pair { Left: Value, Right: Value, Message: Label };
}
fn Wrap(Value: Pair) -> Choice {
    let Shadow = Value.Left;
    return Choice.Full { Value: Value, Shadow: Shadow };
}
fn Empty(Value: Choice) -> Choice { return Choice.Empty {}; }
fn Aliasˉbytes(Value: bytes) -> bytes {
    let Active = Wrap(Box(Value, "wv1"));
    let Selected = match Active {
        case Choice.Full { Value: Remaining, Shadow: Shadow } { Shadow }
        case Choice.Empty { Value }
    };
    let Inactive = Empty(Active);
    return match Inactive {
        case Choice.Empty { let Lastˉpair = Box(Selected, "wv1"); Lastˉpair.Right }
        case Choice.Full { Value: Otherˉpair, Shadow: Otherˉshadow } { Otherˉpair.Left }
    };
}
fn Encodedˉaliases(Value: bytes) -> bytes {
    let Label = Message("w");
    let Active = Wrap(Box(Value, Label));
    let Inactive = Empty(Active);
    return match Inactive {
        case Choice.Empty {
            let Lastˉpair = Box(Value, Label);
            let Lastˉbytes = Lastˉpair.Right;
            let Forwarded = Forwardˉtext(Lastˉpair.Message);
            let Encoded = Textˉtoˉutf8(Forwarded);
            if Bytes.At(borrow Lastˉbytes, 0u64) != 42u8 { return Textˉfailed; }
            // Only the UTF8 byte share escapes. All text and aggregate aliases
            // are local to this function and close on its return.
            Encoded
        }
        case Choice.Full { Value: Remaining, Shadow: Shadow } { Textˉfailed }
    };
}
export fn Main(Input: borrow bytes, Budget: Memory.Memoryˉbudget) -> bytes effects(memory.allocate) {
    let Created = Bytes.Constructˉreserved(Budget, 8u64);
    return match Created {
        case Results.Result.Valid { Value: Initial } {
            var Builder = Initial;
            let Added = Bytes.Appendˉu64ˉlittle(borrow mut Builder, 42u64);
            let Frozen = Bytes.Freeze(Builder);
            let Lastˉbytes = Aliasˉbytes(Frozen);
            let Encoded = Encodedˉaliases(Lastˉbytes);
            if Bytes.Length(borrow Encoded) != 3u64 ||
                Bytes.At(borrow Encoded, 0u64) != 119u8 ||
                Bytes.At(borrow Encoded, 1u64) != 118u8 ||
                Bytes.At(borrow Encoded, 2u64) != 49u8 { return Input; }
            Lastˉbytes
        }
        case Results.Result.Failure { Error: Allocationˉerror } { Input }
    };
}`, [42, 0, 0, 0, 0, 0, 0, 0], 8, 64, 1, 32);
    Case('branch-release-before-reallocation', `
data Branchˉfailed: bytes = [0];
fn Build(Parent: borrow mut Memory.Memoryˉbudget, Value: u8) -> bytes effects(memory.allocate) {
    let Split = Memory.Split(borrow mut Parent, 64u64, 17u32);
    return match Split {
        case Results.Result.Valid { Value: Child } {
            let Created = Bytes.Constructˉreserved(Child, 40u64);
            match Created {
                case Results.Result.Valid { Value: Initial } {
                    var Builder = Initial;
                    let Added = Bytes.Appendˉu8(borrow mut Builder, Value);
                    Bytes.Freeze(Builder)
                }
                case Results.Result.Failure { Error: Allocationˉerror } { Branchˉfailed }
            }
        }
        case Results.Result.Failure { Error: Splitˉerror } { Branchˉfailed }
    };
}
fn Falseˉedge(Value: bytes, Keep: bool, Parent: borrow mut Memory.Memoryˉbudget) -> bytes effects(memory.allocate) {
    if Keep { return Value; }
    return Build(borrow mut Parent, 43u8);
}
fn Trueˉedge(Value: bytes, Replace: bool, Parent: borrow mut Memory.Memoryˉbudget) -> bytes effects(memory.allocate) {
    if Replace { return Build(borrow mut Parent, 44u8); }
    return Value;
}
export fn Main(Input: borrow bytes, Budget: Memory.Memoryˉbudget) -> bytes effects(memory.allocate) {
    var Parent = Budget;
    let First = Build(borrow mut Parent, 42u8);
    let Second = Falseˉedge(First, Bytes.Length(borrow Input) == 0u64, borrow mut Parent);
    if Bytes.At(borrow Second, 0u64) != 43u8 { return Branchˉfailed; }
    return Trueˉedge(Second, Bytes.Length(borrow Input) != 0u64, borrow mut Parent);
}`, [44], 40, 64, 1, 64, 0, null, 45);
    Case('typed-append-refusal', `
export fn Main(Input: borrow bytes, Budget: Memory.Memoryˉbudget) -> bytes effects(memory.allocate) {
    let Created = Bytes.Constructˉreserved(Budget, 8u64);
    return match Created {
        case Results.Result.Valid { Value: Initial } {
            var Builder = Initial;
            let Added = Bytes.Appendˉu64ˉlittle(borrow mut Builder, 42u64);
            let Refused = Bytes.Appendˉu8(borrow mut Builder, 9u8);
            match Refused {
                case Results.Result.Valid { Value: Accepted } { Input }
                case Results.Result.Failure { Error: Limitˉerror } {
                    match Limitˉerror {
                        case Memory.Limitˉfailure.Maximumˉexceeded { Requested: Requested, Maximum: Maximum } {
                            if Requested == 9u64 && Maximum == 8u64 { Bytes.Freeze(Builder) }
                            else { Input }
                        }
                        case Memory.Limitˉfailure.Arithmeticˉoverflow { Input }
                    }
                }
            }
        }
        case Results.Result.Failure { Error: Allocationˉerror } { Input }
    };
}`, [42, 0, 0, 0, 0, 0, 0, 0], 8, 32);
    Case('typed-constructor-refusal', `
export fn Main(Input: borrow bytes, Budget: Memory.Memoryˉbudget) -> bytes effects(memory.allocate) {
    var Parent = Budget;
    let Split = Memory.Split(borrow mut Parent, 16u64, 17u32);
    return match Split {
        case Results.Result.Valid { Value: Child } {
            let Created = Bytes.Constructˉreserved(Child, 8u64);
            match Created {
                case Results.Result.Valid { Value: Unexpected } { Bytes.Freeze(Unexpected) }
                case Results.Result.Failure { Error: Allocationˉerror } {
                    if Allocationˉerror.Reason == Memory.Allocationˉreason.Budgetˉexhausted &&
                        Allocationˉerror.Requestedˉbytes == 32u64 && Allocationˉerror.Availableˉbytes == 16u64 {
                        Input
                    } else { Bytesˉfromˉu8(255u8) }
                }
            }
        }
        case Results.Result.Failure { Error: Splitˉerror } { Bytesˉfromˉu8(254u8) }
    };
}`, SOURCE_INPUT, 4, 0);
    for (const Iterations of [1, 1000, 32768]) {
        Case('fixed-live-source-' + Iterations, `
data Scalingˉfailed: bytes = [0];
export fn Main(Input: borrow bytes, Budget: Memory.Memoryˉbudget) -> bytes effects(memory.allocate) {
    var Parent = Budget;
    var Current: bytes = Input;
    var Index: u32 = 0u32;
    while Index < ${Iterations}u32 && Bytes.Length(borrow Current) > 0u64 {
        let Split = Memory.Split(borrow mut Parent, 32u64, 17u32);
        let Next = match Split {
            case Results.Result.Valid { Value: Child } {
                let Created = Bytes.Constructˉreserved(Child, 8u64);
                match Created {
                    case Results.Result.Valid { Value: Initial } {
                        var Builder = Initial;
                        // Keep the preceding share live through new reservation.
                        let Previous = Bytes.At(borrow Current, 0u64);
                        let Added = Bytes.Appendˉu8(borrow mut Builder, Previous);
                        Bytes.Freeze(Builder)
                    }
                    case Results.Result.Failure { Error: Allocationˉerror } { Scalingˉfailed }
                }
            }
            case Results.Result.Failure { Error: Splitˉerror } { Scalingˉfailed }
        };
        if Bytes.Length(borrow Next) != 1u64 || Bytes.At(borrow Next, 0u64) != 6u8 {
            return Scalingˉfailed;
        }
        Current = Next;
        Index = Index + 1u32;
    }
    if Index != ${Iterations}u32 { return Scalingˉfailed; }
    return Current;
}`, [6], 8, Iterations === 1 ? 32 : 64, Iterations, 32, 0, Iterations);
    }
    Case('checked-overflow-closes-domain', `
export fn Main(Input: borrow bytes, Budget: Memory.Memoryˉbudget) -> bytes effects(memory.allocate) {
    let Created = Bytes.Constructˉreserved(Budget, 8u64);
    return match Created {
        case Results.Result.Valid { Value: Initial } {
            var Builder = Initial;
            let Overflow = 18446744073709551615u64 + Bytes.Length(borrow Input);
            let Added = Bytes.Appendˉu64ˉlittle(borrow mut Builder, Overflow);
            Bytes.Freeze(Builder)
        }
        case Results.Result.Failure { Error: Allocationˉerror } { Input }
    };
}`, [], 0, 32, 1, 0, 1);
    return Object.freeze(Cases);
}

function Sourceˉrequire(Condition, Message) { if (!Condition) throw new Error(Message); }
function Sourceˉhash(Value) { return createHash('sha256').update(Value).digest('hex'); }
async function Sourceˉrequireˉabsent(Path) {
    try { await lstat(Path); }
    catch (Error) { if (Error.code === 'ENOENT') return; throw Error; }
    throw new Error('A refused compiler result produced an output path: ' + Path);
}
async function Sourceˉread(Path, Maximum) {
    const Place = resolve(Path), Information = await lstat(Place);
    Sourceˉrequire(Information.isFile() && !Information.isSymbolicLink() && Information.nlink === 1 &&
        Information.size > 0 && Information.size <= Maximum &&
        (process.platform === 'win32' ? (await realpath(Place)).toLowerCase() === Place.toLowerCase() :
            await realpath(Place) === Place), 'Nonordinary or oversized source execution input: ' + Place);
    const File = await open(Place, 'r');
    try {
        const Before = await File.stat();
        const Sameˉidentity = Other => Other.isFile() && Other.nlink === 1 &&
            Other.dev === Information.dev && Other.ino === Information.ino &&
            Other.size === Information.size && Other.mtimeMs === Information.mtimeMs &&
            Other.ctimeMs === Information.ctimeMs;
        Sourceˉrequire(Sameˉidentity(Before), 'Source execution input changed before bounded read.');
        // One extra byte detects growth without allowing readFile to expand its allocation.
        const Value = Buffer.alloc(Information.size + 1);
        let Offset = 0;
        while (Offset < Value.length) {
            const Part = await File.read(Value, Offset, Value.length - Offset, Offset);
            if (Part.bytesRead === 0) break;
            Offset += Part.bytesRead;
        }
        Sourceˉrequire(Offset === Information.size && Sameˉidentity(await File.stat()) &&
            Sameˉidentity(await lstat(Place)), 'Source execution input changed while bounded read.');
        return Value.subarray(0, Offset);
    } finally { await File.close(); }
}

function Sourceˉhostˉsuccess(Report, Object, Inspection, Abi = 25) {
    Sourceˉrequire(Abi === 22 || Abi === 25, 'Unexpected source compiler ABI.');
    const Code = /^  \[0\] \.text kind=Code align=16 memory=([0-9]+) data=([0-9]+)$/mu.exec(Inspection);
    const Codeˉbytes = Code === null ? 0 : Number(Code[2]);
    const Objectˉhash = Sourceˉhash(Object);
    Sourceˉrequire(Code !== null && Code[1] === Code[2] && Number.isSafeInteger(Codeˉbytes) &&
        Codeˉbytes > 0 && Codeˉbytes < Object.length &&
        Inspection.split(/\r?\n/u).filter(Line => Line === 'SHA-256: ' + Objectˉhash).length === 1,
    'Compiler host metadata has no matching admitted physical text section.');
    Sourceˉrequire(Report === `native x64 status=Valid abi=${Abi} code-bytes=${Codeˉbytes} object-bytes=${Object.length}\n`,
        'Compiler host reported a different status, source ABI or emitted object geometry.');
}

function Sourceˉexports(Report) {
    const Names = new Map();
    for (const Line of Report.split(/\r?\n/u)) {
        const Match = /^  \[[0-9]+\] ([^ ]+) binding=(Export|Import) kind=(Function|Data) section=([0-9]+|undefined) offset=([0-9]+) size=([0-9]+)$/u.exec(Line);
        if (!Match) continue;
        Sourceˉrequire(!Names.has(Match[1]), 'Duplicate generated source symbol.');
        Names.set(Match[1], { Binding: Match[2], Kind: Match[3], Section: Match[4],
            Offset: Number(Match[5]), Bytes: Number(Match[6]) });
    }
    for (const [Name, Kind] of [['Main', 'Function'], ['Windvale_shared_result_close', 'Function'],
        ['Windvale_shared_data_blob', 'Data'], ['Windvale_shared_data_directory', 'Data']]) {
        const Symbol = Names.get(Name);
        Sourceˉrequire(Symbol?.Binding === 'Export' && Symbol.Kind === Kind && Symbol.Section !== 'undefined' &&
            Number.isSafeInteger(Symbol.Offset) && Symbol.Offset <= 4_194_304 &&
            Number.isSafeInteger(Symbol.Bytes) && Symbol.Bytes <= 4_194_304 - Symbol.Offset &&
            (Kind === 'Data' || Symbol.Bytes > 0), 'Missing ABI25 source export: ' + Name);
    }
    const Imports = [...Names].filter(([, Symbol]) => Symbol.Binding === 'Import');
    Sourceˉrequire(Imports.length === 2 && ['Windvale_budgeted_storage', 'Windvale_shared_storage']
        .every(Name => {
            const Symbol = Names.get(Name);
            return Symbol?.Binding === 'Import' && Symbol.Kind === 'Function' &&
                Symbol.Section === 'undefined' && Symbol.Offset === 0 && Symbol.Bytes === 0;
        }), 'Generated source imports differ from ABI25 leaves.');
    Sourceˉrequire([...Names.values()].filter(Symbol => Symbol.Binding === 'Export').length === 4,
        'Generated source has unexpected public exports.');
    const Main = Names.get('Main'), Close = Names.get('Windvale_shared_result_close');
    Sourceˉrequire(Close.Section === Main.Section && Close.Offset === Main.Offset + 32 &&
        Close.Bytes === 17 && Main.Bytes >= 49, 'Generated result-close alias differs from ABI25.');
    const Blob = Names.get('Windvale_shared_data_blob'), Directory = Names.get('Windvale_shared_data_directory');
    Sourceˉrequire(Blob.Section === Directory.Section && Blob.Section !== Main.Section && Blob.Offset === 0 &&
        Directory.Offset === Math.ceil(Blob.Bytes / 4) * 4 &&
        Directory.Bytes % 16 === 0 && Directory.Bytes <= 64 * 16,
        'Generated source static directory exceeds its admitted profile.');
    return Names;
}

function Sourceˉharness(Case, Symbols) {
    const Reservations = Case.Expectedˉphysicalˉreservations;
    Sourceˉrequire(Reservations === null ||
        ([1, 1000, 32768].includes(Reservations) && Case.Name === 'fixed-live-source-' + Reservations &&
            Case.Iterations === Reservations && Case.Trapˉstatus === 0 && Case.Maximum === 8 &&
            Case.Expected.equals(Buffer.from([6])) && Case.Returnedˉphysicalˉcharge === 32 &&
            Case.Physicalˉhighˉwater === (Reservations === 1 ? 32 : 64)),
    'Physical generation observation requires the exact fixed-live source profile.');
    const Blob = Symbols.get('Windvale_shared_data_blob').Bytes;
    const Directory = Symbols.get('Windvale_shared_data_directory').Bytes;
    const Runtimeˉmaximum = 44, Applicationˉmaximum = 64;
    const Rootˉmaximum = Blob + SOURCE_INPUT.length + Runtimeˉmaximum + Applicationˉmaximum;
    Sourceˉrequire(Number.isSafeInteger(Rootˉmaximum) && Rootˉmaximum <= 4_194_304 + 112,
        'Source fixture root reservations exceed the bounded mapped-data profile.');
    const Bridge = 12000;
    const Fixture = Buildˉownedˉdomainˉfixture('source-' + Case.Name, Context => {
        const { Emit, Set, Check, Initialize, REQUEST: R, CONTEXT: C, SHARED: S,
            PHYSICAL: P, ACCOUNTING: B, ADAPTER: A, Sharedˉrequest: Q } = Context;
        Set(R + 72, Rootˉmaximum); Set(R + 160, Runtimeˉmaximum);
        Set(R + 168, Applicationˉmaximum); Set(R + 88, 50_000_000);
        for (const [Field, Bytes, Symbol] of [[112, Directory, 'Windvale_shared_data_directory'],
            [128, Blob, 'Windvale_shared_data_blob']]) {
            if (Bytes === 0) Emit('xor eax eax');
            else Emit('load_address rax ' + Symbol);
            Emit(`store_memory_u64 rsp none 1 ${R + Field} rax`); Set(R + Field + 8, Bytes);
        }
        Initialize();
        let Generationˉcheck = 0;
        function Physicalˉreservations(Expected) {
            if (Reservations === null) return;
            // Fresh physical slots start at zero; only successful reserve increments generation.
            // Release and teardown preserve it. These three workloads reserve exactly 32 bytes
            // per iteration; mapped anchors do not use these 64 physical slots. This is not a
            // general allocation statistic. The u64 sum and per-slot bound prevent u32 masking.
            const Label = 'Source_physical_generations_' + Generationˉcheck++;
            Emit('xor edx edx', 'xor ecx ecx', 'label ' + Label,
                `load_memory_u32 eax rsp rcx 1 ${P + 64}`,
                `compare_i32 eax ${Reservations}`, 'branch above Failed',
                'add rdx rax', 'add_i32 ecx 32', 'compare_i32 ecx 2048', 'branch below ' + Label,
                `move_u32 eax ${Expected}`, 'compare rdx rax', 'branch not_equal Failed');
        }
        Physicalˉreservations(0);
        Emit(`load_memory_u64 rax rsp none 1 ${C + 168}`,
            `store_memory_u64 rsp none 1 ${Bridge + 16} rax`);
        Set(Bridge + 24, SOURCE_INPUT.length); Set(Bridge + 28, SOURCE_INPUT.length);
        function Closed() {
            for (const [At, Value] of [[P + 28, 1], [P + 56, 0], [P + 52, 0], [A + 40, 1],
                [S + 32, 1], [S + 2120, 0], [S + 2152, 0], [B + 24, 0], [B + 48, 0],
                [A + 1092, 0], [A + 1156, 0]]) Check(At, Value);
        }
        Emit('move rcx rsp', `add_i32 rcx ${Bridge}`, 'move rdx rsp', `add_i32 rdx ${C}`,
            'call Main');
        if (Case.Trapˉstatus !== 0) {
            // The generated byte entry closes the domain and preserves overflow high1/low0.
            Emit('move rdx rax', 'shift_right rdx 32', `compare_i32 edx ${Case.Trapˉstatus}`,
                'branch not_equal Failed', 'test eax eax', 'branch not_equal Failed');
            Check(C + 184, 0); Check(C + 188, 0); Check(P + 24, Case.Physicalˉhighˉwater);
            Closed();
            return;
        }
        Emit('test rax rax', 'branch not_equal Failed');
        Check(C + 184, 0); Check(C + 188, 0);
        Check(Bridge + 8, Case.Expected.length); Check(Bridge + 12, Case.Maximum);
        Check(P + 24, Case.Physicalˉhighˉwater); Check(P + 56, Case.Returnedˉphysicalˉcharge);
        function Query(Selector) {
            const Label = 'Clear_source_query_' + Selector;
            Emit('xor eax eax', `move_u32 ecx ${Q}`, `label ${Label}`,
                'store_memory_u64 rsp rcx 1 0 rax', 'add_i32 ecx 8',
                `compare_i32 ecx ${Q + 128}`, `branch below ${Label}`);
            Set(Q, 2); Set(Q + 4, 128); Set(Q + 8, Selector); Set(Q + 16, 71);
            Emit(`load_memory_u64 rax rsp none 1 ${Bridge}`, 'move edx eax', 'and_i32 edx 127',
                'shift_right rax 32', 'shift_left rax 32', 'or rax rdx',
                `store_memory_u64 rsp none 1 ${Q + 24} rax`);
            if (Selector === 10) Emit(`load_memory_u32 eax rsp none 1 ${Bridge}`, 'shift_right eax 7',
                `store_memory_u64 rsp none 1 ${Q + 40} rax`,
                `load_memory_u32 eax rsp none 1 ${Bridge + 8}`,
                `store_memory_u64 rsp none 1 ${Q + 48} rax`);
            Emit('move r8 rsp', `add_i32 r8 ${S}`, 'move r9 rsp', `add_i32 r9 ${Q}`,
                'call Windvale_shared_storage', 'test eax eax', 'branch not_equal Failed');
        }
        Query(10);
        Physicalˉreservations(Reservations);
        if (Reservations !== null) Check(P + 52, 1);
        Emit(`load_memory_u64 rsi rsp none 1 ${Q + 72}`, 'test rsi rsi', 'branch equal Failed');
        for (const [Index, Value] of Case.Expected.entries()) Emit('xor eax eax',
            `load_memory_u8 al rsi none 1 ${Index}`, `compare_i32 eax ${Value}`, 'branch not_equal Failed');
        Query(9);
        Physicalˉreservations(Reservations);
        if (Reservations !== null) { Check(P + 52, 0); Check(P + 56, 0); }
        Emit('xor eax eax', `store_memory_u64 rsp none 1 ${Bridge} rax`,
            `store_memory_u64 rsp none 1 ${Bridge + 8} rax`, 'xor ecx ecx',
            'move rdx rsp', `add_i32 rdx ${C}`, 'call Windvale_shared_result_close',
            'test rax rax', 'branch not_equal Failed');
        Closed();
        Physicalˉreservations(Reservations);
    }, { Symbols: ['symbol import function Main', 'symbol import function Windvale_shared_result_close',
        'symbol import data Windvale_shared_data_blob', 'symbol import data Windvale_shared_data_directory'] });
    Sourceˉrequire(Fixture.Source.split('symbol export function Main in .text').length === 2 &&
        Fixture.Source.split('define Main\n').length === 2, 'Domain wrapper entry identity differs.');
    const Source = Fixture.Source.replace('symbol export function Main in .text', 'symbol export function TestMain in .text')
        .replace('define Main\n', 'define TestMain\n');
    const Bindings = { local: 0, export: 1, import: 2 };
    const Names = Source.split('\n').filter(Line => Line.startsWith('symbol ')).map(Line => Line.split(' '));
    Sourceˉrequire(Names.every((Name, Index) => Index === 0 ||
        Bindings[Names[Index - 1][1]] < Bindings[Name[1]] ||
        (Bindings[Names[Index - 1][1]] === Bindings[Name[1]] && Names[Index - 1][3] < Name[3])),
    'Renamed source wrapper symbols are not in canonical order.');
    return Source;
}

function Sourceˉproductˉcontext(Context) {
    const { Target, Deadline, Compilerˉcheckpoint: Compiler, Requireˉsuccess } = Context;
    Sourceˉrequire(['windows', 'linux'].includes(Target) &&
        Target === (process.platform === 'win32' ? 'windows' : 'linux') &&
        Number.isSafeInteger(Deadline) && Deadline > Date.now() &&
        typeof Compiler?.directory === 'string' && typeof Compiler.Requireˉunchanged === 'function' &&
        typeof Requireˉsuccess === 'function',
    'Shared source products require the admitted existing frontend and a finite deadline.');
}

async function Sourceˉworkspace(Context) {
    const { Repository, Work, Target } = Context;
    const Common = join(Work, 'Shared-Source-Workspace'); await mkdir(Common);
    const Targetˉdescriptor = 'Projects/Targets/' + (Target === 'windows' ? 'Windows' : 'Linux') + '-X64-No-Foreign.wvtd';
    const Snapshots = [];
    for (const Relative of ['Windvale.wvws', SOURCE_LOCK, SOURCE_PROFILE, Targetˉdescriptor, ...SOURCE_MODULES]) {
        const Input = join(Repository, Relative), Destination = join(Common, Relative);
        const Value = await Sourceˉread(Input, 1_048_576);
        if (Relative === SOURCE_LOCK) Sourceˉrequire(Sourceˉhash(Value) === SOURCE_LOCK_SHA256, 'Source input lock differs.');
        await mkdir(dirname(Destination), { recursive: true }); await writeFile(Destination, Value, { flag: 'wx' });
        Snapshots.push({ Path: Input, Value });
    }
    return { Common, Targetˉdescriptor, Snapshots };
}

async function Sourceˉcompileˉproduct(Context, Common, Project, Wvb, Label, Maximum, Expectedˉminor = 44) {
    Sourceˉproductˉcontext(Context);
    Sourceˉrequire(Expectedˉminor === 44 || Expectedˉminor === 45, 'Invalid shared source product version expectation.');
    const { Repository, Target, Requireˉsuccess, Compilerˉcheckpoint: Compiler } = Context;
    const Suffix = Target === 'windows' ? '.exe' : '.elf';
    await Compiler.Requireˉunchanged();
    const Report = await Requireˉsuccess(process.execPath, [join(Repository, 'Tools', 'Native', 'Run-Split-Compiler.mjs'),
        ...['Admitter', 'Authenticator', 'Analyzer', 'Emitter'].map(Name => join(Compiler.directory, Name + Suffix)),
        '--foreign-binder', join(Compiler.directory, 'Binder' + Suffix),
        '--workspace', join(Common, 'Windvale.wvws'), '--project', join(Common, Project),
        '--manifest-reader', join(Compiler.directory, 'Reader' + Suffix), Wvb], Label);
    const Bytecode = await Sourceˉread(Wvb, Maximum);
    Sourceˉrequire(Bytecode.length >= 24 && Bytecode.readUInt32LE(0) === 0x31425657 &&
        Bytecode.readUInt16LE(4) === 1 && Bytecode.readUInt16LE(6) === Expectedˉminor,
    `Current shared source product did not publish WVB1.${Expectedˉminor}.`);
    const Analysis = [...Report.Output.matchAll(/^split compiler step=authenticated-analysis cache=(Created|Hit) key=([0-9a-f]{64})$/gmu)];
    Sourceˉrequire(Analysis.length === 1, 'Shared source product omitted its exact authenticated analysis checkpoint.');
    await Compiler.Requireˉunchanged();
    return { Wvb, Bytecode, Analysisˉstatus: Analysis[0][1], Analysisˉkey: Analysis[0][2] };
}

// Compilation and header checks do not replace complete bytecode admission.
// Every native use admits the exact bytes afresh through the existing read-only CLI.
async function Sourceˉadmitˉproduct(Context, Product, Object, Label) {
    Sourceˉproductˉcontext(Context);
    const { Repository, Deadline, Requireˉsuccess, Compilerˉcheckpoint: Compiler } = Context;
    Sourceˉrequire(Buffer.isBuffer(Product.Bytecode) && Product.Bytecode.length <= 4_194_304,
        'Shared source admission needs the bounded compiled bytecode.');
    await Compiler.Requireˉunchanged();
    Sourceˉrequire((await Sourceˉread(Product.Wvb, 4_194_304)).equals(Product.Bytecode),
        'Shared source bytecode changed before complete admission.');
    await Sourceˉrequireˉabsent(Object);
    Sourceˉrequire(Date.now() < Deadline, 'Shared source deadline reached before complete admission.');
    let Report;
    try {
        Report = await Requireˉsuccess(process.execPath,
            [join(Repository, 'Tools/Native/Verify-Wvb.mjs'), '--current', Product.Wvb], Label);
    } finally {
        Sourceˉrequire((await Sourceˉread(Product.Wvb, 4_194_304)).equals(Product.Bytecode),
            'Shared source bytecode changed during complete admission.');
        await Sourceˉrequireˉabsent(Object);
        await Compiler.Requireˉunchanged();
    }
    Sourceˉrequire(Report.Code === 0 && !Report.Exceeded && !Report.Timedˉout &&
        Report.Output.replaceAll('\r\n', '\n') === 'wvb status=Valid profile=compiler-aligned\n',
    'Shared source complete bytecode admission did not return the exact Valid report.');
    Sourceˉrequire(Date.now() < Deadline, 'Shared source deadline reached after complete admission.');
}

async function Sourceˉcaseˉproduct(Context, Workspace, Case) {
    const { Work } = Context, { Common, Targetˉdescriptor } = Workspace;
    const Root = 'Tests/Fixtures/Native-X64/Shared-Source-' + Case.Name + '.wv';
    const Project = 'Projects/Tests/Shared-Source-' + Case.Name + '.wvproj';
    await mkdir(dirname(join(Common, Root)), { recursive: true });
    await writeFile(join(Common, Root), Case.Source, { flag: 'wx' });
    await mkdir(dirname(join(Common, Project)), { recursive: true });
    await writeFile(join(Common, Project), ['windvale-project 4', `root "${Root}"`,
        ...SOURCE_MODULES.map(Path => `source "${Path}"`), 'emit wvb',
        `source-input-lock "${SOURCE_LOCK}"`, 'source-input-lock-sha256 ' + SOURCE_LOCK_SHA256,
        `source-profile "${SOURCE_PROFILE}"`, `target-descriptor "${Targetˉdescriptor}"`, ''].join('\n'), { flag: 'wx' });
    const Prefix = join(Work, 'Shared-Source-' + Case.Name);
    return { Prefix, ...await Sourceˉcompileˉproduct(Context, Common, Project, Prefix + '.wvb',
        'shared-source-' + Case.Name + '-compile', 4_194_304, Case.Wvbˉminor) };
}

async function Sourceˉsnapshotsˉunchanged(Snapshots, Maximum) {
    for (const Snapshot of Snapshots) Sourceˉrequire((await Sourceˉread(Snapshot.Path, Maximum)).equals(Snapshot.Value),
        'A canonical shared source product input changed during the owner run.');
}

// Preparation constructs only required products; it never runs their Main.
const STAGING_NATIVE_CASES = Object.freeze([
    Object.freeze({ Name: 'staging-relocations', Project: 'Staging-Wvo-Relocations-Native', Assertions: 6, Instructions: 1_000_000 }),
    Object.freeze({ Name: 'staging-symbols', Project: 'Staging-Wvo-Symbols-Native', Assertions: 7, Instructions: 6_000_000 }),
]);
const RETIRED_SOURCE_ENTRIES = Object.freeze([
    "Compiler/Windvale/Source-Wvb-Memory-Adapter.wv",
    "Projects/Compiler/Windvale-Compiler-Memory.wvproj",
    "Tests/Fixtures/Native-X64/Wvo-Staging-Relocations-Adapter.wv",
    "Projects/Tests/Windvale-Native-Test-Staging-Wvo-Relocations.wvproj",
    "Tests/Fixtures/Native-X64/Wvo-Staging-Symbols-Adapter.wv",
    "Projects/Tests/Windvale-Native-Test-Staging-Wvo-Symbols.wvproj"
]);

export async function Checkˉsharedˉretirements(Repository) {
    for (const Relative of RETIRED_SOURCE_ENTRIES) await Sourceˉrequireˉabsent(join(Repository, Relative));
    return RETIRED_SOURCE_ENTRIES.length;
}

async function Sourceˉprepareˉretainedˉproducts(Context) {
    const { Repository, Work, Target, Deadline, Requireˉsuccess,
        Compilerˉcheckpoint: Compiler, Compilerˉkey: Key } = Context;
    Sourceˉrequire(/^[0-9a-f]{64}$/u.test(Key), 'Retained preparation needs the exact current compiler key.');
    const Specs = [
        ['Budget-Oracle', 'Budgeted-Storage-Accounting-Oracle', 2],
        ['Owned-Vector', 'Owned-Vector-Scope', 0],
        ['Owned-Vector-Growth', 'Owned-Vector-Growth', 0],
        ['Owned-Vector-Append-Refusal', 'Owned-Vector-Append-Refusal', 0],
        ['Owned-Vector-Helpers', 'Owned-Vector-Helpers', 0],
        ['Owned-Vector-Scalar-Mutation', 'Owned-Vector-Scalar-Mutation', 0],
        ['Owned-Helper-Cleanup', 'Owned-Helper-Cleanup', 0],
        ['Borrow-Probe', 'X64-Foundation-Borrow-Machine-Probe', 6],
        ...STAGING_NATIVE_CASES.map(Case => [Case.Name, Case.Project, 0]),
    ];
    const Arguments = [join(Repository, 'Tools/Native/Build-Current-Split-Project-Wvb.mjs'),
        '--prepared-compiler-only', '--compiler-checkpoint', Key, '--deadline-ms', String(Deadline)];
    for (const [Name, Project] of Specs) {
        Sourceˉrequire(Date.now() < Deadline, 'Retained WVB preparation deadline reached.');
        await Compiler.Requireˉunchanged();
        await Requireˉsuccess(process.execPath, [...Arguments,
            join(Repository, 'Projects/Tests/Windvale-Native-Test-' + Project + '.wvproj'),
            join(Work, Name + '.wvb')], 'shared-retained-wvb-' + Name);
    }
    let Images = 0;
    for (const [Name, , Profile] of Specs) {
        const Wvb = join(Work, Name + '.wvb');
        await Sourceˉread(Wvb, 4_194_304);
        if (Profile === 0) continue;
        Sourceˉrequire(Date.now() < Deadline, 'Retained image preparation deadline reached.');
        await Requireˉsuccess(process.execPath,
            [join(Repository, 'Tools/Native/Build-Cached-Segmented-Hosted-Wvb.mjs'),
                '--deadline-ms', String(Deadline), String(Profile), Wvb,
                join(Work, Name + (Target === 'windows' ? '.exe' : '.elf'))],
            'shared-retained-image-' + Name);
        Images += 1;
    }
    await Compiler.Requireˉunchanged();
    return { Wvbs: Specs.length, Images };
}

// This executes the admitted lowerer to construct an object. It does not execute
// the generated application; actual export lengths determine the wrapper bytes.
async function Sourceˉwarmˉsourceˉwrapper(Context, Cache, Case, Product) {
    const { Repository, Requireˉsuccess, Preparedˉsharedˉhost: Host } = Context;
    const Suffix = Context.Target === 'windows' ? 'cmd' : 'sh';
    const Tool = Name => join(Repository, 'Tools/Native', Name + '.' + Suffix);
    await Host.Requireˉunchanged();
    const Object = Product.Prefix + '.wvo';
    await Sourceˉadmitˉproduct(Context, Product, Object, 'shared-source-' + Case.Name + '-prepare-wvb-admission');
    await Host.Requireˉunchanged();
    const Lowered = await Requireˉsuccess(Host.Path, [Product.Wvb, Object],
        'shared-source-' + Case.Name + '-prepare-native-object');
    const Bytes = await Sourceˉread(Object, 4_194_304 - 32);
    await Requireˉsuccess(Tool('Check-Wvo'), [Object], 'shared-source-' + Case.Name + '-prepare-object-check');
    const Inspected = await Requireˉsuccess(Tool('Inspect-Wvo'), [Object],
        'shared-source-' + Case.Name + '-prepare-object-inspect');
    Sourceˉhostˉsuccess(Lowered.Output, Bytes, Inspected.Output);
    const Source = Product.Prefix + '-wrapper.wva', Output = Product.Prefix + '-wrapper.wvo';
    await writeFile(Source, Sourceˉharness(Case, Sourceˉexports(Inspected.Output)), { flag: 'wx' });
    await Acquireˉassemblyˉobject(Cache, Source, Output);
    await Host.Requireˉunchanged();
}

export async function Prepareˉsharedˉsourceˉproducts(Context) {
    Sourceˉproductˉcontext(Context);
    const { Selection, Deadline, Compilerˉcheckpoint: Compiler, Preparedˉsharedˉhost: Host } = Context;
    Sourceˉrequire(['source', 'plan', 'retained', 'all'].includes(Selection) &&
        process.env.WINDVALE_PREPARED_PRODUCTS_ONLY === undefined,
    'Shared source preparation requires an explicit selection and a preparation environment.');
    await Compiler.Requireˉunchanged();
    let Sourceˉproducts = 0, Planˉproducts = 0, Wrapperˉobjects = 0, Runtimeˉobjects = 0;
    let Cache = null;
    if (Host && ['source', 'all'].includes(Selection)) {
        await Host.Requireˉunchanged();
        Sourceˉrequire(typeof Context.Prepareˉruntimeˉobjects === 'function',
            'Shared assembly preparation needs the existing runtime object owner.');
        Runtimeˉobjects = (await Context.Prepareˉruntimeˉobjects()).length;
        Cache = await Prepareˉassemblyˉobjectˉcache(Deadline);
    }
    if (['source', 'all'].includes(Selection)) {
        const Workspace = await Sourceˉworkspace(Context), Cases = Buildˉsharedˉsourceˉcases();
        for (const [Index, Case] of Cases.entries()) {
            Sourceˉrequire(Date.now() < Deadline, 'Shared source preparation deadline reached.');
            process.stdout.write(`native shared source preparation item=${Index + 1}/${Cases.length} case=${Case.Name} status=Started behavior-cases=0\n`);
            const Product = await Sourceˉcaseˉproduct(Context, Workspace, Case);
            Sourceˉproducts += 1;
            if (Host) {
                await Sourceˉwarmˉsourceˉwrapper(Context, Cache, Case, Product);
                Wrapperˉobjects += 1;
            }
            process.stdout.write(`native shared source preparation case=${Case.Name} status=Prepared ` +
                `analysis-cache=${Product.Analysisˉstatus} analysis-key=${Product.Analysisˉkey} ` +
                `wvb-bytes=${Product.Bytecode.length} behavior-cases=0\n`);
        }
        await Sourceˉsnapshotsˉunchanged(Workspace.Snapshots, 1_048_576);
    }
    if (['plan', 'all'].includes(Selection)) {
        Sourceˉrequire(Date.now() < Deadline, 'Shared plan preparation deadline reached.');
        process.stdout.write('native shared source preparation case=plan-consumer status=Started behavior-cases=0\n');
        const Product = await Sourceˉplanˉproduct(Context);
        await Sourceˉsnapshotsˉunchanged(Product.Snapshots, 4_194_304);
        Planˉproducts = 1;
        process.stdout.write('native shared source preparation case=plan-consumer status=Prepared ' +
            `analysis-cache=${Product.Analysisˉstatus} analysis-key=${Product.Analysisˉkey} ` +
            `wvb-bytes=${Product.Bytecode.length} behavior-cases=0\n`);
    }
    await Compiler.Requireˉunchanged();
    Sourceˉrequire(Date.now() < Deadline, 'Shared source preparation deadline reached before completion.');
    const Retained = ['retained', 'all'].includes(Selection) ? await Sourceˉprepareˉretainedˉproducts(Context) : { Wvbs: 0, Images: 0 };
    if (Host) await Host.Requireˉunchanged();
    Sourceˉrequire(Date.now() < Deadline, 'Shared prerequisite preparation deadline reached.');
    return { Sourceˉproducts, Planˉproducts, Products: Sourceˉproducts + Planˉproducts,
        Retainedˉproducts: Retained.Wvbs, Retainedˉimages: Retained.Images,
        Runtimeˉobjects, Wrapperˉobjects, Lowererˉexecutions: Wrapperˉobjects,
        Verifierˉexecutions: Wrapperˉobjects, Executions: 0,
        Sourceˉwrappersˉprepared: Sourceˉproducts === 0 || Wrapperˉobjects === Sourceˉproducts };
}

async function Sourceˉstagingˉinputs(Context) {
    const { Repository, Deadline } = Context;
    const Snapshots = [], Names = new Set(['Windvale.wvws']);
    for (const Case of STAGING_NATIVE_CASES) {
        Sourceˉrequire(Date.now() < Deadline, 'Staging source snapshot deadline reached.');
        const Project = 'Projects/Tests/Windvale-Native-Test-' + Case.Project + '.wvproj';
        const Manifest = await Sourceˉread(join(Repository, Project), 65_536);
        const Text = Manifest.toString('utf8');
        Sourceˉrequire(Text.startsWith('windvale-project 4\n') && !Text.includes('\r'),
            'Staging source project is not canonical Project4.');
        const Roots = [...Text.matchAll(/^root "([^"]+)"$/gmu)];
        const Sources = [...Text.matchAll(/^source "([^"]+)"$/gmu)];
        Sourceˉrequire(Roots.length === 1 && Roots[0][1] ===
            'Tests/Fixtures/Native-X64/Wvo-Staging-' + Case.Project.slice(12) + '-Adapter.wv' &&
            Sources.length >= 1 && Sources.length <= 15,
            'Staging fixture root or source count differs from its bounded profile.');
        const Projectˉnames = new Set([Roots[0][1]]);
        Snapshots.push({ Path: join(Repository, Project), Value: Manifest });
        Names.add(Roots[0][1]);
        for (const Source of Sources) {
            Sourceˉrequire(/^Compiler\/Windvale\/Native-X64-Lowering-(?:Sha256|Shared-Templates|Staging-Manifest|Staging-Wvo-(?:Envelope|Symbols|Relocations)(?:-Native-Bridge)?)\.wv$/u.test(Source[1]),
                'Unknown source in the retained staging fixture profile.');
            Sourceˉrequire(!Projectˉnames.has(Source[1]), 'Repeated source in a staging project.');
            Projectˉnames.add(Source[1]);
            Names.add(Source[1]);
        }
        for (const [Field, Expected] of [['source-input-lock', SOURCE_LOCK], ['source-profile', SOURCE_PROFILE]]) {
            const Matches = [...Text.matchAll(new RegExp('^' + Field + ' "([^"\\n]+)"$', 'gmu'))];
            Sourceˉrequire(Matches.length === 1 && Matches[0][1] === Expected,
                'Staging project admission input differs: ' + Field);
            Names.add(Expected);
        }
        const Targets = [...Text.matchAll(/^target-descriptor "([^"\n]+)"$/gmu)];
        Sourceˉrequire(Targets.length === 1 && /^Projects\/Targets\/(?:Windows|Linux)-X64-No-Foreign\.wvtd$/u.test(Targets[0][1]),
            'Staging project target descriptor differs.');
        Names.add(Targets[0][1]);
        const Locks = [...Text.matchAll(/^source-input-lock-sha256 ([0-9a-f]{64})$/gmu)];
        Sourceˉrequire(Locks.length === 1 && Locks[0][1] === SOURCE_LOCK_SHA256 &&
            [...Text.matchAll(/^emit wvb$/gmu)].length === 1 &&
            Text.split('\n').filter(Line => Line !== '').length === Sources.length + 7,
            'Unknown or inconsistent staging project field.');
    }
    Sourceˉrequire(Names.size <= 32, 'Staging source snapshot exceeds its bounded profile.');
    for (const Relative of Names) {
        Sourceˉrequire(Date.now() < Deadline, 'Staging source snapshot deadline reached.');
        Snapshots.push({ Path: join(Repository, Relative), Value: await Sourceˉread(join(Repository, Relative), 1_048_576) });
    }
    return Snapshots;
}


export function Buildˉstagingˉentry(Instructions) {
    Sourceˉrequire([1_000_000, 6_000_000].includes(Instructions), 'Unknown staging fixture instruction profile.');
    // The large symbol boundary uses about 5.31 million metered instructions.
    // This isolated fixture grants its own finite budget before entering Main.
    return ['windvale-assembly 1', 'symbol export function TestMain in .text',
        'symbol import function Main', 'section code .text align 16', 'define TestMain',
        'push r15', 'move r15 rdx', 'subtract_i32 rsp 32', `move_u32 eax ${Instructions}`,
        'store_memory_u64 r15 none 1 8 rax', 'call Main', 'add_i32 rsp 32', 'pop r15',
        'return', 'end define', 'end section', ''].join('\n');
}

// These retained scalar fixtures exercise the live Project4 directories through
// the same true compiler host and ordinary console path used by native lowering.
export async function Runˉsharedˉstagingˉcases(Context) {
    Sourceˉproductˉcontext(Context);
    const { Repository, Work, Target, Deadline, Requireˉsuccess, Runˉprocess,
        Compilerˉcheckpoint: Compiler, Compilerˉkey: Key,
        Sharedˉcompilerˉhost: Host, Preparedˉsharedˉhost: Prepared } = Context;
    Sourceˉrequire(/^[0-9a-f]{64}$/u.test(Key) && typeof Runˉprocess === 'function' &&
        typeof Prepared?.Requireˉunchanged === 'function' && Prepared.Compilerˉkey === Key &&
        Prepared.Path === Host?.Path && Prepared.Sha256 === Host?.Sha256 &&
        process.env.WINDVALE_PREPARED_PRODUCTS_ONLY === '1',
    'Staging behavior needs the exact prepared compiler host and prepared products.');
    const Ending = Target === 'windows' ? 'cmd' : 'sh';
    const Tool = Name => join(Repository, 'Tools/Native', Name + '.' + Ending);
    const Snapshots = await Sourceˉstagingˉinputs(Context);
    await Sourceˉsnapshotsˉunchanged(Snapshots, 1_048_576);
    let Assertions = 0;
    for (const Case of STAGING_NATIVE_CASES) {
        Sourceˉrequire(Date.now() < Deadline, 'Staging native behavior deadline reached.');
        await Compiler.Requireˉunchanged(); await Prepared.Requireˉunchanged();
        process.stdout.write('native shared staging directories case=' + Case.Name + ' status=Started\n');
        const Prefix = join(Work, Case.Name), Wvb = Prefix + '.wvb', Object = Prefix + '.wvo';
        await Requireˉsuccess(process.execPath, [join(Repository, 'Tools/Native/Build-Current-Split-Project-Wvb.mjs'),
            '--prepared-compiler-only', '--compiler-checkpoint', Key, '--deadline-ms', String(Deadline),
            join(Repository, 'Projects/Tests/Windvale-Native-Test-' + Case.Project + '.wvproj'), Wvb],
        'shared-' + Case.Name + '-prepared-wvb');
        const Input = await Sourceˉread(Wvb, 4_194_304);
        Sourceˉrequire(Input.length >= 24 && Input.toString('ascii', 0, 4) === 'WVB1' &&
            Input.readUInt16LE(4) === 1 && Input.readUInt16LE(6) >= 11 && Input.readUInt16LE(6) <= 43,
        'Retained staging source published a different WVB profile.');
        const Lowered = await Requireˉsuccess(Host.Path, [Wvb, Object], 'shared-' + Case.Name + '-lower');
        const Objectˉbytes = await Sourceˉread(Object, 4_194_304 - 32);
        await Requireˉsuccess(Tool('Check-Wvo'), [Object], 'shared-' + Case.Name + '-object-check');
        const Inspection = await Requireˉsuccess(Tool('Inspect-Wvo'), [Object], 'shared-' + Case.Name + '-object-inspect');
        Sourceˉhostˉsuccess(Lowered.Output, Objectˉbytes, Inspection.Output, 22);
        const Wrapperˉsource = Prefix + '-entry.wva', Wrapper = Prefix + '-entry.wvo';
        await writeFile(Wrapperˉsource, Buildˉstagingˉentry(Case.Instructions), { flag: 'wx' });
        await Requireˉsuccess(Tool('Assemble-Wva'), [Wrapperˉsource, Wrapper], 'shared-' + Case.Name + '-entry');
        const Image = Prefix + '.bin';
        const Linked = await Requireˉsuccess(Tool('Link-Wvo'), ['0', 'TestMain', Image, Wrapper, Object],
            'shared-' + Case.Name + '-link');
        const Entry = /^entry name=TestMain address=([0-9]+)$/mu.exec(Linked.Output);
        Sourceˉrequire(Entry !== null && Number.isSafeInteger(Number(Entry[1])), 'Staging native Main entry differs.');
        const Application = Prefix + (Target === 'windows' ? '.exe' : '.elf');
        await Requireˉsuccess(Tool('Package-Console'), [Target + '-x64-console-v1', Image, Entry[1], Application],
            'shared-' + Case.Name + '-package');
        const Result = await Runˉprocess(Application, [], Math.min(30_000, Deadline - Date.now()),
            'shared-' + Case.Name + '-execute');
        Sourceˉrequire(Result.Code === 42 && !Result.Exceeded && !Result.Timedˉout && Result.Output === '',
            'Staging native fixture assertions failed: ' + Case.Name);
        Sourceˉrequire((await Sourceˉread(Wvb, 4_194_304)).equals(Input) &&
            (await Sourceˉread(Object, 4_194_304 - 32)).equals(Objectˉbytes),
        'Staging source or native object changed during behavior.');
        await Sourceˉsnapshotsˉunchanged(Snapshots, 1_048_576);
        await Compiler.Requireˉunchanged(); await Prepared.Requireˉunchanged();
        Sourceˉrequire(Date.now() < Deadline, 'Staging native behavior exceeded its deadline.');
        Assertions += Case.Assertions;
        process.stdout.write('native shared staging directories case=' + Case.Name +
            ' status=Passed assertions=' + Case.Assertions + ' source-abi=22 instruction-maximum=' + Case.Instructions + '\n');
    }
    return { Cases: Assertions, Products: STAGING_NATIVE_CASES.length, Executions: STAGING_NATIVE_CASES.length };
}

export async function Runˉadmissionˉproviderˉcase(Context) {
    const { Repository, Work, Target, Requireˉsuccess, Runˉprocess, Deadline } = Context;
    const Owner = await Sourceˉread(join(Repository, 'Linker/Startup/X64-Shared-Compiler-Host.wva'), 1_048_576);
    const Definitions = ['Admission_read', 'Admission_write'].map(Name => {
        const Matches = [...Owner.toString('utf8').matchAll(new RegExp('^define ' + Name + '\\n[\\s\\S]*?^end define$', 'gmu'))];
        Sourceˉrequire(Matches.length === 1, 'Missing exact admission provider definition: ' + Name);
        return Matches[0][0];
    });
    const Lines = ['windvale-assembly 1',
        'symbol local function Admission_read in .text', 'symbol local function Admission_write in .text',
        ...['Bad_request', 'Bad_response', 'Request', 'Response'].map(Name => 'symbol local data ' + Name + ' in .rodata'),
        'symbol export function Main in .text',
        'section code .text align 16', 'define Main', 'push r12', 'push r15', 'subtract_i32 rsp 40',
        'move r15 rdx', 'load_memory_u64 r12 r15 none 1 48',
        'load_memory_u32 eax r15 none 1 56', 'compare_i32 eax 20983872', 'branch below Failed'];
    const Emit = (...Values) => Lines.push(...Values);
    const Set = (Offset, Value) => Emit('move_u32 eax ' + Value, 'store_memory_u32 r12 none 1 ' + Offset + ' eax');
    const Check = (Offset, Value) => Emit('load_memory_u32 eax r12 none 1 ' + Offset,
        'compare_i32 eax ' + Value, 'branch not_equal Failed');
    let Cases = 0;
    function Reset() {
        Set(11384, 1); Set(11388, 0); Set(11392, 0); Set(8712, 48);
        Set(8688, 0); Set(8692, 0); Set(8696, 0); Set(8700, 0);
        Set(11600, 123); Set(11604, 0); Set(11608, 0); Set(11612, 0);
        for (let At = 0; At < 64; At += 4) { Set(20983808 + At, 100 + At); Set(11312 + At, 17); }
        Emit('move_u32 eax 20983872', 'store_memory_u32 r15 none 1 60 eax',
            'move_u32 r10d 291', 'move_u32 r11d 1110');
    }
    function Status(Value) {
        Emit('compare_i32 eax ' + Value, 'branch not_equal Failed',
            'compare_i32 r10d 291', 'branch not_equal Failed',
            'compare_i32 r11d 1110', 'branch not_equal Failed'); Cases++;
    }
    function Read(Name = 'Request', Length = 7) {
        Emit('move rcx r12', 'add_i32 rcx 11600', 'load_address r8 ' + Name,
            'move_u32 r9d ' + Length, 'call Admission_read');
    }
    function Write(Length = 64, Offset = 20983808, Name = 'Response') {
        Emit('move rcx r12', 'add_i32 rcx ' + Offset, 'move_u32 edx ' + Length,
            'load_address r8 ' + Name, 'move_u32 r9d 8', 'call Admission_write');
    }
    for (const Kind of ['inactive', 'name', 'name-length', 'short-request', 'large-request']) {
        Reset();
        if (Kind === 'inactive') Set(11384, 0);
        if (Kind === 'short-request') Set(8712, 47);
        if (Kind === 'large-request') Set(8712, 2161);
        Read(Kind === 'name' ? 'Bad_request' : 'Request', Kind === 'name-length' ? 6 : 7);
        Status(1); Check(11388, 0); Check(11600, 123); Check(11608, 0);
    }
    for (const Length of [48, 2160]) {
        Reset(); Set(8712, Length); Read(); Status(0); Check(11388, 1);
        Check(11608, Length); Check(11612, 0);
        Emit('load_memory_u64 rax r12 none 1 11600', 'move rcx r12', 'add_i32 rcx 8720',
            'compare rax rcx', 'branch not_equal Failed');
        Read(); Status(1); Check(11388, 1); Check(11608, Length);
    }
    for (const Kind of ['inactive', 'before-read', 'name', 'short', 'middle', 'large', 'below-arena',
        'past-live', 'invalid-live', 'repeated']) {
        Reset(); Set(11388, 1);
        if (Kind === 'inactive') Set(11384, 0);
        if (Kind === 'before-read') Set(11388, 0);
        if (Kind === 'repeated') Set(11392, 1);
        if (Kind === 'invalid-live') Emit('load_memory_u32 eax r15 none 1 56', 'add_i32 eax 1',
            'store_memory_u32 r15 none 1 60 eax');
        const Length = Kind === 'short' ? 31 : Kind === 'middle' ? 33 : Kind === 'large' ? 65 : 64;
        Write(Length, Kind === 'below-arena' ? 20983807 : Kind === 'past-live' ? 20983809 : 20983808,
            Kind === 'name' ? 'Bad_response' : 'Response');
        Status(1); Check(11392, Kind === 'repeated' ? 1 : 0); Check(8696, 0); Check(11312, 17);
    }
    for (const Length of [32, 64]) {
        Reset(); Set(11388, 1); Write(Length); Status(0); Check(11392, 1);
        Check(8696, Length); Check(8700, 0);
        for (let At = 0; At < 64; At += 4) Check(11312 + At, At < Length ? 100 + At : 17);
        Emit('load_memory_u64 rax r12 none 1 8688', 'move rcx r12', 'add_i32 rcx 11312',
            'compare rax rcx', 'branch not_equal Failed');
    }
    Emit('move_u32 eax 42', 'jump_label Complete', 'label Failed', 'move_u32 eax 1', 'label Complete',
        'add_i32 rsp 40', 'pop r15', 'pop r12', 'return', 'end define', ...Definitions, 'end section',
        'section rodata .rodata align 16');
    for (const [Name, Value] of [['Request', 'request'], ['Response', 'response'],
        ['Bad_request', 'requesx'], ['Bad_response', 'responsx']])
        Emit('define ' + Name, 'bytes ' + [...Buffer.from(Value)].join(' '), 'end define');
    Emit('end section', '');
    const Prefix = join(Work, 'shared-admission-providers'), Source = Prefix + '.wva', Object = Prefix + '.wvo',
        Image = Prefix + '.chunk-0', Application = Prefix + (Target === 'windows' ? '.exe' : '.elf');
    const Tool = Name => join(Repository, 'Tools/Native', Name + (Target === 'windows' ? '.cmd' : '.sh'));
    await writeFile(Source, Lines.join('\n'), { flag: 'wx' });
    await Requireˉsuccess(Tool('Assemble-Wva'), [Source, Object], 'shared-admission-providers-assemble');
    const Report = await Requireˉsuccess(Tool('Link-Wvo'), ['0', 'Main', Image, Object], 'shared-admission-providers-link');
    const Entry = /^entry name=Main address=([0-9]+)$/mu.exec(Report.Output);
    Sourceˉrequire(Entry !== null, 'Admission provider probe has no native entry.');
    await Requireˉsuccess(Tool('Package-Hosted-Wvb'), ['image', '1',
        Context.Preparedˉsharedˉhost.Segmentedˉconsumer.Carrier.Path, Prefix, '1', Entry[1], Application, Target],
    'shared-admission-providers-package');
    const Result = await Runˉprocess(Application, [], Math.min(30_000, Deadline - Date.now()), 'shared-admission-providers-execute');
    Sourceˉrequire(Result.Code === 42 && !Result.Exceeded && !Result.Timedˉout && Result.Output === '',
        'Admission provider bounds, one-shot state, output copy or counter preservation failed.');
    Sourceˉrequire((await Sourceˉread(join(Repository, 'Linker/Startup/X64-Shared-Compiler-Host.wva'), 1_048_576)).equals(Owner),
        'Admission providers changed during execution.');
    process.stdout.write('native shared admission providers status=Passed cases=' + Cases + ' executions=1\n');
}

export async function Runˉsharedˉsourceˉcases(Context) {
    const Available = Buildˉsharedˉsourceˉcases(), Selection = Context.Sourceˉcases;
    Sourceˉrequire(Selection === undefined || (Array.isArray(Selection) && Selection.length > 0 &&
        Selection.length <= Available.length && new Set(Selection).size === Selection.length &&
        Selection.every(Name => Available.some(Case => Case.Name === Name))),
    'Shared source selection must contain distinct existing case names.');
    const Cases = Selection === undefined ? Available : Available.filter(Case => Selection.includes(Case.Name));
    const Iterations = [...new Set(Cases.map(Case => Case.Iterations))].sort((Left, Right) => Left - Right);
    process.stdout.write('native shared source selection=' + Cases.map(Case => Case.Name).join(',') +
        ' selected=' + Cases.length + '/' + Available.length + '\n');
    process.stdout.write('native shared staging manifest cases=' + Checkˉsharedˉstagingˉmanifest() + ' status=Passed\n');
    await Runˉadmissionˉproviderˉcase(Context);
    const { Repository, Work, Target, Requireˉsuccess, Runˉprocess, Deadline,
        Compilerˉcheckpoint: Compiler, Sharedˉcompilerˉhost: Host, Runtimeˉobjects } = Context;
    Sourceˉrequire(['windows', 'linux'].includes(Target) &&
        Target === (process.platform === 'win32' ? 'windows' : 'linux') &&
        Number.isSafeInteger(Deadline) && Deadline > Date.now() &&
        typeof Compiler?.directory === 'string' && typeof Compiler.Requireˉunchanged === 'function' &&
        typeof Host?.Path === 'string' && /^[0-9a-f]{64}$/u.test(Host.Sha256) && Array.isArray(Runtimeˉobjects),
    'Actual source execution requires admitted prepared frontend, compiler host and deadline.');
    const Suffix = Target === 'windows' ? '.exe' : '.elf', Extension = Target === 'windows' ? 'cmd' : 'sh';
    const Tool = Name => join(Repository, 'Tools', 'Native', Name + '.' + Extension);
    const Providers = Runtimeˉobjects.filter(Path => ['Owned.wvo', 'Allocator.wvo', 'Budget-Validation.wvo',
        'Budgeted.wvo', 'Domain.wvo', 'Shared.wvo'].includes(basename(Path)));
    Sourceˉrequire(Providers.length === 6 && new Set(Providers.map(Path => basename(Path))).size === 6,
        'Source execution requires exactly the existing six runtime leaves.');
    const Workspace = await Sourceˉworkspace(Context);
    const Cache = await Prepareˉassemblyˉobjectˉcache(Deadline);
    const Fatalˉtraps = Cases.filter(Case => Case.Trapˉstatus !== 0).length;
    let Hostˉrefusals = 0;
    for (const [Index, Case] of Cases.entries()) {
        Sourceˉrequire(Date.now() < Deadline, 'Source execution owner deadline reached.');
        await Compiler.Requireˉunchanged();
        process.stdout.write(`native shared source step=compile item=${Index + 1}/${Cases.length} case=${Case.Name} status=Started\n`);
        const { Prefix, Wvb, Bytecode } = await Sourceˉcaseˉproduct(Context, Workspace, Case);
        const Object = Prefix + '.wvo';
        await Sourceˉadmitˉproduct(Context, { Wvb, Bytecode }, Object,
            'shared-source-' + Case.Name + '-wvb-admission');
        await Context.Preparedˉsharedˉhost.Requireˉunchanged();
        const Compiled = await Requireˉsuccess(Host.Path, [Wvb, Object], 'shared-source-' + Case.Name + '-native-compiler');
        const Nativeˉobject = await Sourceˉread(Object, 4_194_304 - 32);
        await Requireˉsuccess(Tool('Check-Wvo'), [Object], 'shared-source-' + Case.Name + '-object-check');
        const Report = await Requireˉsuccess(Tool('Inspect-Wvo'), [Object], 'shared-source-' + Case.Name + '-object-inspect');
        Sourceˉhostˉsuccess(Compiled.Output, Nativeˉobject, Report.Output);
        if (Case.Name === 'byte-family') {
            const Invalidˉinput = Buffer.from(Bytecode); Invalidˉinput[0] ^= 1;
            const Invalidˉpath = Prefix + '-invalid-magic.wvb', Refusedˉobject = Prefix + '-refused.wvo';
            await writeFile(Invalidˉpath, Invalidˉinput, { flag: 'wx' });
            await Sourceˉrequireˉabsent(Refusedˉobject);
            Sourceˉrequire(Date.now() < Deadline, 'Source execution owner deadline reached before semantic refusal.');
            const Refused = await Runˉprocess(Host.Path, [Invalidˉpath, Refusedˉobject],
                Math.min(30_000, Deadline - Date.now()), 'shared-source-host-invalid-wvb');
            Sourceˉrequire(Refused.Code === 1 && !Refused.Exceeded && !Refused.Timedˉout &&
                Refused.Output === 'native x64 status=Invalidˉwvb abi=22 code-bytes=0 object-bytes=0\n',
            'Compiler host did not return the exact typed malformed-WVB refusal.');
            await Sourceˉrequireˉabsent(Refusedˉobject);
            Sourceˉrequire((await Sourceˉread(Invalidˉpath, 4_194_304)).equals(Invalidˉinput),
                'The malformed-WVB refusal input changed during compilation.');
            await Compiler.Requireˉunchanged();
            Hostˉrefusals += 1;
            process.stdout.write('native shared source host-refusal status=Passed code=1 ' +
                'native-status=Invalidˉwvb abi=22 code-bytes=0 object-bytes=0 output-absent=true\n');
        }
        const Symbols = Sourceˉexports(Report.Output), Wrapper = Prefix + '-wrapper.wva', Wrapperˉobject = Prefix + '-wrapper.wvo';
        await writeFile(Wrapper, Sourceˉharness(Case, Symbols), { flag: 'wx' });
        await Acquireˉassemblyˉobject(Cache, Wrapper, Wrapperˉobject);
        const Image = Prefix + '.bin';
        const Linked = await Requireˉsuccess(Tool('Link-Wvo'), ['0', 'TestMain', Image, Wrapperˉobject,
            Object, ...Providers], 'shared-source-' + Case.Name + '-link');
        const Entry = /^entry name=TestMain address=([0-9]+)$/mu.exec(Linked.Output);
        Sourceˉrequire(Entry !== null, 'Generated source wrapper entry is missing.');
        const Application = Prefix + Suffix;
        await Requireˉsuccess(Tool('Package-Console'), [Target + '-x64-console-v1', Image, Entry[1], Application],
            'shared-source-' + Case.Name + '-package');
        Sourceˉrequire(Date.now() < Deadline, 'Source execution owner deadline reached before startup.');
        const Started = performance.now(), Execution = await Runˉprocess(Application, [],
            Math.min(30_000, Deadline - Date.now()), 'shared-source-' + Case.Name + '-execute');
        Sourceˉrequire(Execution.Code === 42 && !Execution.Exceeded && !Execution.Timedˉout && Execution.Output === '',
            `Generated source ${Case.Name} failed: code=${Execution.Code}, output=${Execution.Output}.`);
        await Compiler.Requireˉunchanged();
        process.stdout.write(`native shared source case=${Case.Name} status=Passed ` +
            `source-outcome=${Case.Trapˉstatus === 0 ? 'returned-bytes' : 'checked-overflow-domain-closed'} ` +
            `iterations=${Case.Iterations} ` +
            `physical-high-water=${Case.Physicalˉhighˉwater} returned-physical-charge=${Case.Returnedˉphysicalˉcharge} ` +
            'final-physical-charge=0 final-budget-charge=0 ' +
            (Case.Expectedˉphysicalˉreservations === null ? '' :
                `observed-successful-physical-reservations=${Case.Expectedˉphysicalˉreservations} ` +
                `cumulative-physical-reserve-charge=${BigInt(Case.Expectedˉphysicalˉreservations) * 32n} ` +
                'physical-generation-baseline=0 observation=preserved-generation-sum ') +
            `elapsed-ms=${Math.round(performance.now() - Started)}\n`);
    }
    await Sourceˉsnapshotsˉunchanged(Workspace.Snapshots, 1_048_576);
    process.stdout.write(`native shared source status=Passed cases=${Cases.length} executions=${Cases.length} ` +
        `successes=${Cases.length - Fatalˉtraps} fatal-traps=${Fatalˉtraps} host-refusals=${Hostˉrefusals} ` +
        `iterations=${Iterations.join(',')} selected=${Cases.length}/${Available.length} ` +
        'arena-bytes=64 application-budget=64 physical-high-water<=64 ' +
        'final-physical-charge=0 final-budget-charge=0 ' +
        `verifier-executions=${Cases.length} ` +
        'generated-source=true compiler=WVB44/45-native25 serializer=current qualification=false\n');
    return { Cases: Cases.length, Executions: Cases.length, Successes: Cases.length - Fatalˉtraps,
        Fatalˉtraps, Hostˉrefusals, Verifierˉexecutions: Cases.length, Iterations,
        Selectedˉcases: Cases.map(Case => Case.Name), Availableˉcases: Available.length };
}

const PLAN_PROJECT = 'Projects/Tests/Windvale-Native-Test-Staging-Content-Native.wvproj';
const PLAN_RUNTIME_MAXIMUM = 65_536;
const PLAN_APPLICATION_MAXIMUM = 192;
const PLAN_ARENA_BYTES = PLAN_RUNTIME_MAXIMUM + PLAN_APPLICATION_MAXIMUM;

// The companion uses the existing admitted WVSC offsets. Its configuration's
// provider-entry address is fixed by the same two-pass linker used by BuildHost.
export function Buildˉsharedˉplanˉcompanion(Configuration, Iterations) {
    Sourceˉrequire(Buffer.isBuffer(Configuration) && Configuration.length === 64 &&
        Configuration.toString('ascii', 0, 4) === 'WVSC' && Configuration.readUInt32LE(4) === 1 &&
        Configuration.readUInt32LE(8) === 64 && [0, 1, 1000].includes(Iterations),
    'Plan consumer needs the admitted ABI25 configuration and finite workload.');
    const Blob = Configuration.readUInt32LE(28), Directory = Configuration.readUInt32LE(36);
    Sourceˉrequire(Blob <= 4_194_304 && Directory <= 8192 && Directory % 16 === 0 &&
        Configuration.readUInt32LE(20) >= 49 &&
        Configuration.readUInt32LE(40) === Configuration.readUInt32LE(16) + 32 &&
        Configuration.readUInt32LE(44) === 17,
    'Plan consumer module geometry differs from the existing ABI25 entry.');
    const Rootˉmaximum = Blob + 4 + PLAN_RUNTIME_MAXIMUM + PLAN_APPLICATION_MAXIMUM;
    const Bridge = 12000, Arena = 16384, Report = 12400;
    const Prefix = Buffer.from('plan-physical-high-water=', 'ascii');
    const Fixture = Buildˉownedˉdomainˉfixture('plan-' + Iterations, Context => {
        const { Emit, Set, Check, Initialize, REQUEST: R, CONTEXT: C, SHARED: S,
            PHYSICAL: P, ACCOUNTING: B, ADAPTER: A, INPUT: Input,
            Lowerˉrequest: L, Sharedˉrequest: Q } = Context;
        Emit(`store_memory_u64 rsp none 1 ${Bridge + 32} rdx`,
            'load_address r14 TestMain', 'load_address rbp Windvale_shared_compiler_configuration',
            'load_memory_u32 eax rbp none 1 56', 'subtract r14 rax', 'branch below Failed',
            `store_memory_u64 rsp none 1 ${Bridge + 40} r14`);
        Set(R + 56, PLAN_ARENA_BYTES); Set(R + 72, Rootˉmaximum);
        Set(R + 160, PLAN_RUNTIME_MAXIMUM); Set(R + 168, PLAN_APPLICATION_MAXIMUM);
        Set(R + 88, 500_000_000); Set(Input, Iterations);
        Emit('move rax rsp', `add_i32 rax ${Arena}`, `store_memory_u64 rsp none 1 ${R + 48} rax`);
        for (const [Field, Offset, Bytes] of [[112, 32, Directory], [128, 24, Blob]]) {
            if (Bytes === 0) Emit('xor eax eax');
            else Emit(`load_memory_u32 eax rbp none 1 ${Offset}`, 'add rax r14');
            Emit(`store_memory_u64 rsp none 1 ${R + Field} rax`); Set(R + Field + 8, Bytes);
        }
        Initialize();
        Emit(`load_memory_u64 rax rsp none 1 ${C + 168}`,
            `store_memory_u64 rsp none 1 ${Bridge + 16} rax`);
        Set(Bridge + 24, 4); Set(Bridge + 28, 4);
        function Entry(Offset) {
            Emit(`load_memory_u64 r14 rsp none 1 ${Bridge + 40}`,
                'load_address rbp Windvale_shared_compiler_configuration',
                `load_memory_u32 eax rbp none 1 ${Offset}`, 'add rax r14', 'call_register rax',
                'test rax rax', 'branch not_equal Failed');
        }
        Emit('move rcx rsp', `add_i32 rcx ${Bridge}`, 'move rdx rsp', `add_i32 rdx ${C}`);
        Entry(16);
        Check(C + 184, 0); Check(C + 188, 0); Check(Bridge + 8, 4); Check(Bridge + 12, 4);
        // Check the entire fixed-live state while the returned mapped share is
        // still live. Teardown cannot conceal retained planning scratch.
        Check(P + 56, 0); Check(P + 52, 0);
        Emit(`load_memory_u32 eax rsp none 1 ${P + 24}`, 'compare_i32 eax 192',
            'branch below Failed', `compare_i32 eax ${PLAN_ARENA_BYTES}`, 'branch above Failed',
            `store_memory_u32 rsp none 1 ${Bridge + 48} eax`);
        Emit('xor eax eax', `move_u32 ecx ${L}`, 'label Clear_runtime_query',
            'store_memory_u64 rsp rcx 1 0 rax', 'add_i32 ecx 8', `compare_i32 ecx ${L + 112}`,
            'branch below Clear_runtime_query');
        Set(L, 2); Set(L + 4, 112); Set(L + 8, 8); Set(L + 16, 71);
        Emit(`load_memory_u64 rax rsp none 1 ${C + 176}`, `store_memory_u64 rsp none 1 ${L + 64} rax`,
            'move r8 rsp', `add_i32 r8 ${A}`, 'move r9 rsp', `add_i32 r9 ${L}`,
            'call Windvale_budgeted_storage', 'test eax eax', 'branch not_equal Failed');
        Check(L + 80, PLAN_RUNTIME_MAXIMUM); Check(L + 84, 0);
        function Query(Selector) {
            Emit('xor eax eax', `move_u32 ecx ${Q}`, `label Clear_plan_query_${Selector}`,
                'store_memory_u64 rsp rcx 1 0 rax', 'add_i32 ecx 8', `compare_i32 ecx ${Q + 128}`,
                `branch below Clear_plan_query_${Selector}`);
            Set(Q, 2); Set(Q + 4, 128); Set(Q + 8, Selector); Set(Q + 16, 71);
            Emit(`load_memory_u64 rax rsp none 1 ${Bridge}`, 'move edx eax', 'and_i32 edx 127',
                'shift_right rax 32', 'shift_left rax 32', 'or rax rdx',
                `store_memory_u64 rsp none 1 ${Q + 24} rax`);
            if (Selector === 10) Emit(`load_memory_u32 eax rsp none 1 ${Bridge}`, 'shift_right eax 7',
                `store_memory_u64 rsp none 1 ${Q + 40} rax`, `move_u32 eax 4`,
                `store_memory_u64 rsp none 1 ${Q + 48} rax`);
            Emit('move r8 rsp', `add_i32 r8 ${S}`, 'move r9 rsp', `add_i32 r9 ${Q}`,
                'call Windvale_shared_storage', 'test eax eax', 'branch not_equal Failed');
        }
        Query(10);
        Emit(`load_memory_u64 rsi rsp none 1 ${Q + 72}`, 'test rsi rsi', 'branch equal Failed',
            'load_memory_u32 eax rsi none 1 0', `compare_i32 eax ${Iterations}`, 'branch not_equal Failed');
        Query(9);
        Emit('xor eax eax', `store_memory_u64 rsp none 1 ${Bridge} rax`,
            `store_memory_u64 rsp none 1 ${Bridge + 8} rax`, 'xor ecx ecx',
            'move rdx rsp', `add_i32 rdx ${C}`);
        Entry(40);
        for (const [At, Value] of [[P + 28, 1], [P + 56, 0], [P + 52, 0], [A + 40, 1],
            [S + 32, 1], [S + 2120, 0], [S + 2152, 0], [B + 24, 0], [B + 48, 0],
            [A + 1092, 0], [A + 1156, 0]]) Check(At, Value);
        // Print one bounded host metric only after closure. This outer ABI22
        // console carrier shares no accounting state with the source domain.
        for (const [Index, Byte] of Prefix.entries()) Emit(`move_u32 eax ${Byte}`,
            `store_memory_u8 rsp none 1 ${Report + Index} al`);
        Emit(`load_memory_u32 edx rsp none 1 ${Bridge + 48}`);
        for (let Index = 0; Index < 8; Index++) {
            Emit('move eax edx', `shift_right eax ${(7 - Index) * 4}`, 'and_i32 eax 15',
                'compare_i32 eax 10', `branch below Plan_digit_${Index}`, 'add_i32 eax 39',
                `label Plan_digit_${Index}`, 'add_i32 eax 48',
                `store_memory_u8 rsp none 1 ${Report + Prefix.length + Index} al`);
        }
        Emit(`load_memory_u64 r15 rsp none 1 ${Bridge + 32}`, 'test r15 r15', 'branch equal Failed',
            'test_i32 r15 7', 'branch not_equal Failed', 'load_memory_u32 eax r15 none 1 0',
            'compare_i32 eax 7', 'branch not_equal Failed', 'load_memory_u32 eax r15 none 1 4',
            'compare_i32 eax 112', 'branch not_equal Failed', 'load_memory_u64 rax r15 none 1 24',
            'test rax rax', 'branch equal Failed', 'load_memory_u64 rax rax none 1 8',
            'test rax rax', 'branch equal Failed', 'move r8 rsp', `add_i32 r8 ${Report}`,
            `move_u32 r9d ${Prefix.length + 8}`, 'call_register rax', 'test eax eax', 'branch not_equal Failed');
    }, { Frame: 131072, Symbols: ['symbol import data Windvale_shared_compiler_configuration'] });
    return { Source: Fixture.Source.replace('symbol export function Main in .text', 'symbol export function TestMain in .text')
        .replace('define Main\n', 'define TestMain\n'), Entry: 'TestMain' };
}

async function Sourceˉplanˉproduct(Context) {
    Sourceˉproductˉcontext(Context);
    const { Repository, Work, Target } = Context;
    const Manifest = await Sourceˉread(join(Repository, PLAN_PROJECT), 65_536);
    const Text = Manifest.toString('utf8');
    const Paths = [...Text.matchAll(/^(?:root|source) "([^"]+)"$/gmu)].map(Match => Match[1]);
    Sourceˉrequire(Paths.length > 0 && new Set(Paths).size === Paths.length && Paths.length <= 128 &&
        Paths[0] === 'Tests/Fixtures/Native-X64/Wvo-Staging-Content-Native-Adapter.wv' &&
        Paths.every(Path => !Path.startsWith('/') && !Path.includes('..') && /^[A-Za-z0-9/_.-]+$/u.test(Path)),
    'Plan consumer project source closure differs.');
    const Targetˉdescriptor = 'Projects/Targets/' + (Target === 'windows' ? 'Windows' : 'Linux') + '-X64-No-Foreign.wvtd';
    const Common = join(Work, 'Shared-Plan-Workspace'); await mkdir(Common);
    const Snapshots = [];
    let Total = 0;
    for (const Relative of ['Windvale.wvws', PLAN_PROJECT, SOURCE_LOCK, SOURCE_PROFILE, Targetˉdescriptor, ...Paths]) {
        const Value = await Sourceˉread(join(Repository, Relative), 4_194_304); Total += Value.length;
        Sourceˉrequire(Total <= 16_777_216, 'Plan consumer source closure exceeds16MiB.');
        if (Relative === SOURCE_LOCK) Sourceˉrequire(Sourceˉhash(Value) === SOURCE_LOCK_SHA256, 'Source input lock differs.');
        const Destination = join(Common, Relative); await mkdir(dirname(Destination), { recursive: true });
        const Output = Relative === PLAN_PROJECT ? Buffer.from(Text.replace(
            /^target-descriptor "[^"]+"$/gmu, 'target-descriptor "' + Targetˉdescriptor + '"'), 'utf8') : Value;
        await writeFile(Destination, Output, { flag: 'wx' }); Snapshots.push({ Path: join(Repository, Relative), Value });
    }
    const Wvb = join(Work, 'Shared-Plan-Consumer.wvb');
    return { Snapshots, ...await Sourceˉcompileˉproduct(Context, Common, PLAN_PROJECT, Wvb,
        'shared-plan-source', 16_777_216, 45) };
}

export async function Runˉsharedˉplanˉconsumer(Context) {
    const { Work, Target, Runˉprocess, Deadline,
        Compilerˉcheckpoint: Compiler, Preparedˉsharedˉhost: Prepared } = Context;
    Sourceˉproductˉcontext(Context);
    Sourceˉrequire(typeof Prepared?.Requireˉunchanged === 'function',
        'Plan consumer requires the admitted segmented construction checkpoint.');
    await Prepared.Requireˉunchanged(); await Compiler.Requireˉunchanged();
    process.stdout.write('native shared plan step=source status=Started construction=segmented\n');
    const { Snapshots, Wvb, Bytecode } = await Sourceˉplanˉproduct(Context);
    const Peaks = new Map();
    let Stagedˉmodule;
    for (const Iterations of [0, 1, 1000]) {
        Sourceˉrequire(Date.now() < Deadline, 'Plan consumer owner deadline reached.');
        await Compiler.Requireˉunchanged(); await Prepared.Requireˉunchanged();
        const Product = await Buildˉsharedˉnativeˉconsumer({ Work, Deadline,
            Input: { Path: Wvb, Sha256: Sourceˉhash(Bytecode) }, Preparedˉsharedˉhost: Prepared,
            Companion: Configuration => Buildˉsharedˉplanˉcompanion(Configuration, Iterations), Stagedˉmodule });
        Stagedˉmodule = Product.Stagedˉmodule;
        Sourceˉrequire(Date.now() < Deadline, 'Plan consumer owner deadline reached before execution.');
        const Started = performance.now();
        const Result = await Runˉprocess(Product.Path, [], Math.min(120_000, Deadline - Date.now()),
            'shared-plan-execute-' + Iterations);
        const Match = /^plan-physical-high-water=([0-9a-f]{8})\n$/u.exec(Result.Output);
        const Peak = Match === null ? 0 : Number.parseInt(Match[1], 16);
        Sourceˉrequire(Result.Code === 42 && !Result.Exceeded && !Result.Timedˉout && Match !== null &&
            Peak >= PLAN_APPLICATION_MAXIMUM && Peak <= PLAN_ARENA_BYTES,
        'Actual plan consumer result, live cleanup or bounded physical metric differs.');
        Peaks.set(Iterations, Peak);
        process.stdout.write(`native shared plan iterations=${Iterations} status=Passed physical-high-water=${Peak} ` +
            `runtime-scratch-bound=${PLAN_RUNTIME_MAXIMUM} runtime-scratch-live=0 physical-live-before-close=0 ` +
            (Iterations === 0 ? 'staging-assertions=11 ' :
                'serializer-reservation-bytes=192 reservation-refusals=95,96 late-refusal=Unsupportedˉmodule ') +
            `elapsed-ms=${Math.round(performance.now() - Started)}\n`);
    }
    Sourceˉrequire(Peaks.get(1) === Peaks.get(1000), 'Fixed-live PlanBuild high-water changed with1000iterations.');
    await Sourceˉsnapshotsˉunchanged(Snapshots, 4_194_304);
    await Compiler.Requireˉunchanged(); await Prepared.Requireˉunchanged();
    process.stdout.write('native shared plan status=Passed cases=3 executions=3 iterations=0,1,1000 ' +
        'source-consumer=actual-plan-build serializer-entry-bytes=76 physical-high-water=' + Peaks.get(1000) +
        ' final-physical-charge=0 final-budget-charge=0 construction=segmented qualification=false\n');
    return { Cases: 3, Executions: 3, Iterations: [0, 1, 1000], Physicalˉhighˉwater: Peaks.get(1000) };
}
