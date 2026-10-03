import { Buildˉstorageˉfixture } from './Native-Storage-Fixture.mjs';
import { createHash } from 'node:crypto';
import { existsSync } from 'node:fs';
import { readFile, stat, writeFile } from 'node:fs/promises';
import { join } from 'node:path';

export async function Prepareˉownedˉhelpers(Context) {
    const Path = Context.Helperˉproduct?.Path ?? join(Context.Work, 'Owned-Vector-Helpers.wvb');
    if (Context.Helperˉproduct) {
        if ((await stat(Path)).size > 1_048_576 || createHash('sha256').update(await readFile(Path)).digest('hex') !== Context.Helperˉproduct.Sha256) {
            throw new Error('Supplied owned helper identity differs.');
        }
    } else {
        await Context.Requireˉsuccess(process.execPath,
            [join(Context.Repository, 'Tools/Native/Build-Current-Split-Project-Wvb.mjs'), '--prepared-compiler-only',
                join(Context.Repository, 'Projects/Tests/Windvale-Native-Test-Owned-Vector-Helpers.wvproj'), Path], 'owned-helper-build');
    }
    const Scalarˉpath = Context.Scalarˉhelperˉproduct?.Path ?? join(Context.Work, 'Owned-Vector-Scalar-Mutation.wvb');
    if (Context.Scalarˉhelperˉproduct) {
        if ((await stat(Scalarˉpath)).size > 1_048_576 ||
            createHash('sha256').update(await readFile(Scalarˉpath)).digest('hex') !== Context.Scalarˉhelperˉproduct.Sha256) {
            throw new Error('Supplied scalar mutation identity differs.');
        }
    } else {
        await Context.Requireˉsuccess(process.execPath,
            [join(Context.Repository, 'Tools/Native/Build-Current-Split-Project-Wvb.mjs'), '--prepared-compiler-only',
                join(Context.Repository, 'Projects/Tests/Windvale-Native-Test-Owned-Vector-Scalar-Mutation.wvproj'), Scalarˉpath],
            'owned-scalar-mutation-build');
    }
    const Scalar = await readFile(Scalarˉpath);
    if (Scalar.readUInt16LE(6) !== 43) throw new Error('Scalar mutation did not select WVB 1.43.');
    const Cleanupˉpath = join(Context.Work, 'Owned-Helper-Cleanup.wvb');
    await Context.Requireˉsuccess(process.execPath,
        [join(Context.Repository, 'Tools/Native/Build-Current-Split-Project-Wvb.mjs'), '--prepared-compiler-only',
            join(Context.Repository, 'Projects/Tests/Windvale-Native-Test-Owned-Helper-Cleanup.wvproj'), Cleanupˉpath],
        'owned-helper-cleanup-build');
    return { Input: await readFile(Path), Scalar, Cleanup: await readFile(Cleanupˉpath) };
}

function Word(Value) { const Result = Buffer.alloc(4); Result.writeUInt32LE(Value); return Result; }

function Inspect(Input) {
    const Sections = new Map();
    for (let Cursor = 12; Cursor < Input.length;) {
        const Kind = Input.readUInt32LE(Cursor), Length = Input.readUInt32LE(Cursor + 4);
        if (Length > Input.length - Cursor - 8) throw new Error('Helper section exceeds input.');
        Sections.set(Kind, { Start: Cursor + 8, Length }); Cursor += 8 + Length;
    }
    const Table = Sections.get(4), Code = Sections.get(5), Functions = [];
    let Cursor = Table.Start + 4;
    function Shape() {
        const Start = Cursor, Kind = Input[Cursor++];
        if ([7, 8, 11, 23, 26, 27, 28, 29, 35].includes(Kind)) Cursor += 4;
        else if (Kind === 37) Shape();
        return { Start, Length: Cursor - Start, Kind };
    }
    const Count = Input.readUInt32LE(Table.Start);
    if (Count > 65) throw new Error('Helper function count exceeds fixture bound.');
    const Sizes = new Map([[1,5],[2,2],[4,5],[5,5],[8,2],[9,5],[48,5],[49,5],[64,5],
        [104,5],[105,5],[106,9],[128,9],[129,9],[151,9],[152,9],[153,9],
        [196,9],[202,5],[205,5],[206,9],[207,9],[208,9],[209,13],[226,9],[227,9],[228,9]]);
    for (let Index = 0; Index < Count; Index++) {
        const Start = Cursor, Nameˉlength = Input.readUInt32LE(Cursor); Cursor += 4;
        const Name = Input.subarray(Cursor, Cursor + Nameˉlength).toString('utf8'); Cursor += Nameˉlength;
        const Parameters = Input.readUInt32LE(Cursor); Cursor += 4;
        if (Parameters > 64) throw new Error('Helper parameter bound differs.');
        const Parameterˉshapes = Array.from({ length: Parameters }, Shape), Return = Shape();
        const Localˉcount = Input.readUInt32LE(Cursor); Cursor += 4;
        if (Localˉcount > 128) throw new Error('Helper local bound differs.');
        const Locals = [...Parameterˉshapes, ...Array.from({ length: Localˉcount }, Shape)];
        const Metadata = Cursor, Offset = Input.readUInt32LE(Cursor), Length = Input.readUInt32LE(Cursor + 4);
        const Operations = []; Cursor += 12;
        for (let At = Code.Start + Offset; At < Code.Start + Offset + Length;) {
            const Opcode = Input[At]; Operations.push([Opcode, At]); At += Sizes.get(Opcode) ?? 1;
        }
        Functions.push({ Index, Name, Start, End: Cursor, Metadata, Offset, Length, Return, Locals, Operations });
    }
    if (Cursor !== Table.Start + Table.Length) throw new Error('Helper function directory differs.');
    return { Sections, Functions };
}

function Section(Input, Kind, Body) {
    const { Start, Length } = Inspect(Input).Sections.get(Kind);
    return Buffer.concat([Input.subarray(0, Start - 4), Word(Body.length), Body, Input.subarray(Start + Length)]);
}

function Replaceˉbody(Input, Function, Body) {
    const Value = Buffer.from(Input), { Sections, Functions } = Inspect(Value), Code = Sections.get(5);
    const Difference = Body.length - Function.Length;
    for (const Item of Functions) {
        if (Item.Index === Function.Index) Value.writeUInt32LE(Body.length, Item.Metadata + 4);
        else if (Item.Offset > Function.Offset) Value.writeUInt32LE(Item.Offset + Difference, Item.Metadata);
    }
    return Section(Value, 5, Buffer.concat([Value.subarray(Code.Start, Code.Start + Function.Offset), Body,
        Value.subarray(Code.Start + Function.Offset + Function.Length, Code.Start + Code.Length)]));
}

function Extraˉfunctions(Input, Count) {
    const { Sections, Functions } = Inspect(Input), Table = Sections.get(4), Code = Sections.get(5);
    const Entries = [Word(Count), Input.subarray(Table.Start + 4, Table.Start + Table.Length)];
    const Bodies = [Input.subarray(Code.Start, Code.Start + Code.Length)];
    for (let Index = Functions.length; Index < Count; Index++) {
        const Name = Buffer.from(`Spare${String(Index).padStart(2, '0')}`), Body = Buffer.from([1, 42, 0, 0, 0, 81]);
        Entries.push(Word(Name.length), Name, Word(0), Buffer.from([1]), Word(0),
            Word(Code.Length + (Index - Functions.length) * Body.length), Word(Body.length), Word(1));
        Bodies.push(Body);
    }
    return Section(Section(Input, 5, Buffer.concat(Bodies)), 4, Buffer.concat(Entries));
}

function Appendˉhelper(Input, Parameter, Return, Body, Maximumˉstack = 1) {
    const { Sections, Functions } = Inspect(Input), Table = Sections.get(4), Code = Sections.get(5);
    const Name = Buffer.from('Transfer');
    const Entry = Buffer.concat([Word(Name.length), Name, Word(1), Parameter, Return,
        Word(0), Word(Code.Length), Word(Body.length), Word(Maximumˉstack)]);
    return Section(Section(Input, 5, Buffer.concat([Input.subarray(Code.Start, Code.Start + Code.Length), Body])), 4,
        Buffer.concat([Word(Functions.length + 1), Input.subarray(Table.Start + 4, Table.Start + Table.Length), Entry]));
}

export function Buildˉvectorˉmutationˉrejections(Input) {
    const { Sections, Functions } = Inspect(Input);
    const Mutate = Functions.find(Item => Item.Name === 'Mutateˉvalue');
    const Append = Functions.find(Item => Item.Name === 'Append');
    const Length = Functions.find(Item => Item.Name === 'Borrowˉlength');
    const Replacement = Mutate?.Operations.find(([Opcode]) => Opcode === 228)?.[1];
    const Parameterˉlength = Length?.Operations.find(([Opcode]) => Opcode === 226)?.[1];
    if (Replacement === undefined || Parameterˉlength === undefined || Append === undefined) {
        throw new Error('Mutation rejection fixture controls differ.');
    }
    return [
        ['old-mutation-minor', Value => { Value.writeUInt16LE(42, 6); }],
        ['immutable-parameter-replace', Value => { Value[Mutate.Locals[0].Start] = 26; }],
        ['immutable-parameter-append', Value => { Value[Append.Locals[0].Start] = 26; }],
        ['replace-target-out-of-range', Value => { Value.writeUInt32LE(Mutate.Locals.length, Replacement + 1); }],
        ['replace-wrong-vector-type', Value => { Value.writeUInt32LE(Input.readUInt32LE(Sections.get(7).Start), Replacement + 5); }],
        ['replace-wrong-index', Value => { Value[Mutate.Locals[1].Start] = 1; }],
        ['replace-wrong-element', Value => { Value[Mutate.Locals[2].Start] = 2; }],
        ['parameter-length-target', Value => { Value.writeUInt32LE(Length.Locals.length, Parameterˉlength + 1); }],
        ['replacement-invalidates-held-loan', Value => {
            const Shape = Mutate.Locals[0];
            const Mutableˉshape = Value.subarray(Shape.Start, Shape.Start + Shape.Length);
            const Vectorˉidentity = Value.subarray(Replacement + 5, Replacement + 9);
            return Appendˉhelper(Value, Mutableˉshape, Buffer.from([1]), Buffer.concat([
                Buffer.from([129]), Buffer.alloc(8), Buffer.from([227]), Word(0), Vectorˉidentity,
                Buffer.from([129]), Buffer.alloc(8), Buffer.from([1, 44, 0, 0, 0, 228]),
                Word(0), Vectorˉidentity, Buffer.from([16, 81]),
            ]), 3);
        }],
        ['truncated-replacement', Value => {
            const Start = Sections.get(5).Start + Mutate.Offset;
            return Replaceˉbody(Value, Mutate, Value.subarray(Start, Replacement + 8));
        }],
    ];
}

async function Runˉownedˉframeˉcases(Context) {
    const { Repository, Work, Target, Requireˉsuccess, Runˉprocess } = Context;
    const Tool = Name => join(Repository, `Tools/Native/${Name}.${Target === 'windows' ? 'cmd' : 'sh'}`);
    const Prefix = join(Work, 'Owned-Frames');
    await Requireˉsuccess(Tool('Assemble-Wva'),
        [join(Repository, 'Runtime/Native/X64-Owned-Frame-Cleanup.wva'), Prefix + '.wvo'], 'owned-frame-template-assemble');
    const Linked = await Requireˉsuccess(Tool('Link-Wvo'),
        ['0', 'Windvale_owned_frame_track', Prefix + '.bin', Prefix + '.wvo'], 'owned-frame-template-link');
    for (const [Name, Offset, Length] of [['track', 0, 304], ['drop', 304, 152], ['cleanup', 456, 264]]) {
        if (!Linked.Output.split(/\r?\n/u).some(Line => Line.endsWith(
            `name=Windvale_owned_frame_${Name} address=${Offset} size=${Length}`))) {
            throw new Error(`Owned frame ${Name} entry differs.`);
        }
    }
    const Source = await readFile(join(Repository, 'Compiler/Windvale/Native-X64-Lowering-Owned-Frames.wv'), 'utf8');
    const Template = /data Template: bytes = \[([\d,\s]+)\];/u.exec(Source)?.[1];
    const Templateˉbytes = Template?.split(',').map(Value => Value.trim()).filter(Boolean).map(Number);
    if (Templateˉbytes?.length !== 720 || !Templateˉbytes.every(Value => Number.isInteger(Value) && Value >= 0 && Value <= 255) ||
        !(await readFile(Prefix + '.bin')).equals(Buffer.from(Templateˉbytes))) {
        throw new Error('Owned frame compiler template differs from linked source.');
    }
    const Cases = [
        { Name: 'reverse-acquisition', Order: [2, 0, 3, 1], Kinds: [7, 9, 17, 19], Expected: [[202, 9], [404, 9], [101, 7], [303, 7]] },
        { Name: 'failed-result', Order: [0, 1], Kinds: [17, 19], Failed: 1, Expected: [[101, 7]] },
        { Name: 'empty', Order: [], Kinds: [], Expected: [] },
        { Name: 'maximum-ranks', Order: Array.from({ length: 64 }, (_, Index) => Index),
            Kinds: Array(64).fill(9), Expected: Array.from({ length: 64 }, (_, Index) => [(64 - Index) * 101, 9]) },
        { Name: 'duplicate-rank', Order: [0, 0], Kinds: [9, 9], Status: 5 },
        { Name: 'rank-bound', Order: [2], Kinds: [9, 9], Status: 5 },
        { Name: 'count-bound', Order: [0], Kinds: [9], Count: 2, Status: 5 },
        { Name: 'capacity-bound', Order: [], Kinds: [], Capacity: 65, Status: 5 },
        { Name: 'invalid-owner-kind', Order: [0], Kinds: [8], Status: 5, Popped: true },
        { Name: 'invalid-result-tag', Order: [0], Kinds: [19], Tag: 2, Status: 5, Popped: true },
    ];
    const Fixture = Buildˉstorageˉfixture('owned-frame-ledger', 64, ({ Emit, Set, Check }) => {
        Emit('move r15 rsp', 'add_i32 r15 25000');
        function Invoke(Name, Status = 0) {
            Emit('move_u32 r10d 12345', 'move_u32 r11d 67890', `call Windvale_owned_frame_${Name}`,
                `compare_i32 eax ${Status}`, 'branch not_equal Failed',
                'compare_i32 r10d 12345', 'branch not_equal Failed', 'compare_i32 r11d 67890', 'branch not_equal Failed');
        }
        function Cleanup(Status = 0) {
            Emit('move rax rsp', 'add_i32 rax 20000', 'move rdx rsp', 'add_i32 rdx 21000',
                'move r8 rsp', 'add_i32 r8 22000', 'load_address r9 Record_release');
            Invoke('cleanup', Status);
        }
        function Track(Rank, Operation, Status = 0) {
            Emit('move rax rsp', 'add_i32 rax 20000', `move_u32 ecx ${Rank}`, `move_u32 edx ${Operation}`);
            Invoke('track', Status);
        }
        for (const Case of Cases) {
            Set('rsp', 25000, 0);
            Set('rsp', 20000, Case.Count ?? Case.Order.length);
            Set('rsp', 20004, Case.Capacity ?? Case.Kinds.length);
            if (Case.Name === 'maximum-ranks') {
                Emit('xor ecx ecx', 'move_u32 edx 101', 'move_u32 r9d 9', 'label Maximum_initialize',
                    'store_memory_u32 rsp rcx 4 20008 ecx', 'move r8d ecx', 'shift_left r8 4',
                    'store_memory_u32 rsp rcx 8 21000 r8d', 'store_memory_u32 rsp rcx 8 21004 r9d',
                    'store_memory_u64 rsp r8 1 22000 rdx', 'add_i32 ecx 1', 'add_i32 edx 101',
                    'compare_i32 ecx 64', 'branch below Maximum_initialize');
            } else {
            for (const [Index, Rank] of Case.Order.entries()) Set('rsp', 20008 + Index * 4, Rank);
            for (const [Index, Kind] of Case.Kinds.entries()) {
                Set('rsp', 21000 + Index * 8, Index * 16);
                Set('rsp', 21004 + Index * 8, Kind);
                if (Kind === 17 || Kind === 19) {
                    const Result = 23500 + Index * 80;
                    Emit('move rax rsp', `add_i32 rax ${Result}`, `store_memory_u64 rsp none 1 ${22000 + Index * 16} rax`);
                    Set('rsp', Result, Case.Tag ?? (Case.Failed === Index ? 1 : 0));
                    Set('rsp', Result + 16, (Index + 1) * 101);
                } else {
                    Set('rsp', 22000 + Index * 16, (Index + 1) * 101);
                    Set('rsp', 22004 + Index * 16, 0);
                }
            }
            }
            Cleanup(Case.Status ?? 0);
            Check('rsp', 20000, Case.Status && !Case.Popped ? Case.Count ?? Case.Order.length : 0);
            Check('rsp', 25000, Case.Expected?.length ?? 0);
            if (Case.Name === 'maximum-ranks') {
                Emit('xor ecx ecx', 'move_u32 edx 6464', 'label Maximum_check', 'move ebp ecx', 'add_i32 ebp 64',
                    'move r9d ecx', 'shift_left r9 4', 'load_memory_u64 r8 rsp r9 1 25008',
                    'compare r8 rdx', 'branch not_equal Failed', 'load_memory_u32 r8d rsp r9 1 25016',
                    'compare_i32 r8d 9', 'branch not_equal Failed', 'add_i32 ecx 1', 'subtract_i32 edx 101',
                    'compare_i32 ecx 64', 'branch below Maximum_check');
            } else {
            for (const [Index, [Handle, Kind]] of (Case.Expected ?? []).entries()) {
                Check('rsp', 25008 + Index * 16, Handle);
                Check('rsp', 25012 + Index * 16, 0);
                Check('rsp', 25016 + Index * 16, Kind);
            }
            }
        }
        Set('rsp', 25000, 0); Set('rsp', 20000, 0); Set('rsp', 20004, 3);
        for (let Index = 0; Index < 3; Index++) {
            Set('rsp', 21000 + Index * 8, Index * 16); Set('rsp', 21004 + Index * 8, 9);
            Set('rsp', 22000 + Index * 16, (Index + 1) * 101); Set('rsp', 22004 + Index * 16, 0);
        }
        Track(0, 0); Track(2, 0); Track(1, 0); Track(0, 1); Track(0, 0);
        Check('rsp', 20000, 3); Check('rsp', 20008, 2); Check('rsp', 20012, 1); Check('rsp', 20016, 0);
        Track(0, 0, 5); Track(3, 1, 5); Track(2, 2, 5);
        Check('rsp', 20000, 3); Check('rsp', 20008, 2); Check('rsp', 20012, 1); Check('rsp', 20016, 0);
        Cleanup(); Check('rsp', 25000, 3);
        Check('rsp', 25008, 101); Check('rsp', 25024, 202); Check('rsp', 25040, 303);
        Track(0, 1, 5); Check('rsp', 20000, 0);
        Emit('move_u32 r15d 7171');
    }, true);
    const Callback = ['define Record_release', 'load_memory_u32 r8d r15 none 1 0', 'move r9d r8d',
        'shift_left r9 4', 'store_memory_u64 r15 r9 1 8 rax', 'store_memory_u32 r15 r9 1 16 ecx',
        'add_i32 r8d 1', 'store_memory_u32 r15 none 1 0 r8d', 'xor eax eax', 'return', 'end define', 'end section'].join('\n');
    await writeFile(Prefix + '-harness.wva', Fixture.Source
        .replace('symbol local function Request_reset', 'symbol local function Record_release in .text\nsymbol local function Request_reset')
        .replace('symbol import function Windvale_owned_storage', ['cleanup', 'track'].map(Name =>
            `symbol import function Windvale_owned_frame_${Name}`).join('\n') + '\nsymbol import function Windvale_owned_storage')
        .replace('end section', Callback));
    await Requireˉsuccess(Tool('Assemble-Wva'), [Prefix + '-harness.wva', Prefix + '-harness.wvo'], 'owned-frame-harness');
    const Objects = ['Owned', 'Allocator', 'Budget-Validation', 'Budgeted'].map(Name => join(Work, `${Name}.wvo`));
    const Harness = await Requireˉsuccess(Tool('Link-Wvo'),
        ['0', 'Main', Prefix + '-harness.bin', Prefix + '-harness.wvo', Prefix + '.wvo', ...Objects], 'owned-frame-link');
    const Address = /^entry name=Main address=(\d+)$/mu.exec(Harness.Output)?.[1];
    if (Address === undefined) throw new Error('Owned frame harness entry missing.');
    const Application = Prefix + (Target === 'windows' ? '.exe' : '.elf');
    await Requireˉsuccess(Tool('Package-Console'),
        [`${Target}-x64-console-v1`, Prefix + '-harness.bin', Address, Application], 'owned-frame-package');
    const Result = await Runˉprocess(Application, [], 30000, 'owned-frame-execute');
    if (Result.Code !== 42 || Result.Timedˉout || Result.Exceeded || Result.Output !== '') {
        throw new Error(`Owned frame ledger failed: ${Result.Code} ${Result.Output}`);
    }
    process.stdout.write('native owned frames status=Passed cases=16 executions=1 maximum-ranks=64 template-bytes=720\n');
    return { Cases: 16, Valid: 6, Malformed: 10, Executions: 1 };
}

export async function Runˉownedˉhelpers(Context, Lowerer) {
    const { Repository, Work, Target, Requireˉsuccess, Runˉprocess } = Context;
    const { Input, Scalar, Cleanup } = await Prepareˉownedˉhelpers(Context), { Sections, Functions } = Inspect(Input);
    const Frameˉcases = await Runˉownedˉframeˉcases(Context);
    if (Input.readUInt16LE(6) !== 43 || Functions.map(Item => Item.Name).join(',') !==
        'Append,Borrow,Borrowˉlength,Borrowˉvalue,Construct,Consume,Forward,Main,Mutate,Mutateˉvalue') {
        throw new Error('Owned helper source workload differs.');
    }
    const [Append, Borrow, Borrowˉlength, Borrowˉvalue, Construct, Consume, Forward, Main, Mutate, Mutateˉvalue] = Functions;
    const Capacity = Construct.Operations.find(([Opcode]) => Opcode === 129)[1] + 1;
    const Budget = Main.Operations.find(([Opcode]) => Opcode === 129)[1] + 1;
    const Constructorˉcall = Main.Operations.find(([Opcode, At]) => Opcode === 64 && Input.readUInt32LE(At + 1) === Construct.Index)[1];
    const Recursiveˉcall = Forward.Operations.find(([Opcode]) => Opcode === 64)[1];
    const Borrowˉindex = Consume.Operations.find(([Opcode, At]) => Opcode === 129 && Input.readBigUInt64LE(At + 1) === 0n)[1] + 1;
    const Replaceˉindex = Consume.Operations.find(([Opcode]) => Opcode === 129)[1] + 1;
    const Replacement = Mutateˉvalue.Operations.find(([Opcode]) => Opcode === 228)?.[1];
    const Parameterˉlength = Borrowˉlength.Operations.find(([Opcode]) => Opcode === 226)?.[1];
    const Parameterˉappend = Append.Operations.find(([Opcode]) => Opcode === 208)?.[1];
    if (Replacement === undefined || Parameterˉlength === undefined || Parameterˉappend === undefined) {
        throw new Error('Owned helper mutation operations differ.');
    }
    const Iterations = Main.Operations.find(([Opcode, At]) => Opcode === 1 && Input.readInt32LE(At + 1) === 1000)?.[1];
    if (Iterations === undefined) throw new Error('Helper iteration limit differs.');
    const Borrowˉshape = Input.subarray(Borrow.Locals[2].Start, Borrow.Locals[2].Start + Borrow.Locals[2].Length);
    const Appendˉresult = Input.readUInt32LE(Consume.Operations.find(([Opcode]) => Opcode === 208)[1] + 5);
    const Vectorˉshape = Buffer.from(Borrowˉshape);
    Vectorˉshape[0] = 23;
    const Resultˉshape = Input.subarray(Construct.Return.Start, Construct.Return.Start + Construct.Return.Length);
    const Cleanupˉfunctions = Inspect(Cleanup).Functions;
    if (Cleanupˉfunctions.map(Item => Item.Name).join(',') !== 'Construct,Discardˉbudget,Discardˉresult,Discardˉsplit,Discardˉvector,Main') {
        throw new Error('Implicit helper cleanup source workload differs.');
    }
    const Cleanupˉmain = Cleanupˉfunctions[5], Cleanupˉconstruct = Cleanupˉfunctions[0];
    const Cleanupˉiterations = Cleanupˉmain.Operations.find(([Opcode, At]) => Opcode === 1 && Cleanup.readInt32LE(At + 1) === 1000)?.[1];
    const Cleanupˉthreshold = Cleanupˉmain.Operations.find(([Opcode, At]) => Opcode === 1 && Cleanup.readInt32LE(At + 1) === 500)?.[1];
    const Vectorˉthreshold = Cleanupˉmain.Operations.find(([Opcode, At]) => Opcode === 1 && Cleanup.readInt32LE(At + 1) === 750)?.[1];
    const Splitˉthreshold = Cleanupˉmain.Operations.find(([Opcode, At]) => Opcode === 1 && Cleanup.readInt32LE(At + 1) === 600)?.[1];
    if (Cleanupˉiterations === undefined || Cleanupˉthreshold === undefined || Vectorˉthreshold === undefined || Splitˉthreshold === undefined) {
        throw new Error('Implicit cleanup iteration controls differ.');
    }
    function Cleanupˉmode(Value, Iterations, Budget, Vector = false, Split = false) {
        Value.writeInt32LE(Iterations, Cleanupˉiterations + 1);
        Value.writeInt32LE(Budget ? Iterations : 0, Cleanupˉthreshold + 1);
        Value.writeInt32LE(Vector ? 0 : Iterations, Vectorˉthreshold + 1);
        Value.writeInt32LE(Split ? Iterations : 0, Splitˉthreshold + 1);
    }
    const Cases = [
        { Name: 'single-mutation-and-release', Result: 42, Peak: 48, Generation: 2,
            Change: Value => { Value.writeInt32LE(1, Iterations + 1); } },
        { Name: 'nested-mutation-and-transfer', Result: 42, Peak: 48, Generation: 2000 },
        { Name: 'sustained-mutation-and-release', Result: 42, Peak: 48, Generation: 65536, Fuel: 100000000,
            Change: Value => { Value.writeInt32LE(32768, Iterations + 1); } },
        { Name: 'implicit-vector-single', Result: 42, Peak: 48, Generation: 1, Input: Cleanup,
            Change: Value => Cleanupˉmode(Value, 1, false, true) },
        { Name: 'implicit-vector-reuse', Result: 42, Peak: 48, Generation: 1999, Input: Cleanup,
            Change: Value => Cleanupˉmode(Value, 1000, false, true) },
        { Name: 'implicit-vector-sustained', Result: 42, Peak: 48, Generation: 65535, Fuel: 100000000, Input: Cleanup,
            Change: Value => Cleanupˉmode(Value, 32768, false, true) },
        { Name: 'implicit-budget-reuse', Result: 42, Peak: 0, Generation: 65535, Fuel: 100000000, Input: Cleanup,
            Change: Value => Cleanupˉmode(Value, 32768, true) },
        { Name: 'implicit-budget-result-reuse', Result: 42, Peak: 0, Generation: 65535, Fuel: 100000000, Input: Cleanup,
            Change: Value => Cleanupˉmode(Value, 32768, false, false, true) },
        { Name: 'implicit-result-single', Result: 42, Peak: 48, Generation: 1, Input: Cleanup,
            Change: Value => Cleanupˉmode(Value, 1, false) },
        { Name: 'implicit-result-reuse', Result: 42, Peak: 48, Generation: 1999, Input: Cleanup,
            Change: Value => Cleanupˉmode(Value, 1000, false) },
        { Name: 'implicit-result-sustained', Result: 42, Peak: 48, Generation: 65535, Fuel: 100000000, Input: Cleanup,
            Change: Value => Cleanupˉmode(Value, 32768, false) },
        { Name: 'implicit-result-refusal-reuse', Result: 42, Peak: 0, Generation: 65535, Fuel: 100000000, Arena: 16, Input: Cleanup,
            Change: Value => Cleanupˉmode(Value, 32768, false) },
        { Name: 'implicit-result-invalid-constructor-cleanup', Status: 5, Peak: 0, Input: Cleanup, Change: Value => {
            Cleanupˉmode(Value, 1, false);
            for (const [Opcode, At] of Cleanupˉconstruct.Operations) {
                if (Opcode === 129 && Value.readBigUInt64LE(At + 1) === 2n) Value.writeBigUInt64LE(0n, At + 1);
            }
        } },
        { Name: 'mutable-borrowed-read', Result: 42, Peak: 48, Change: Value => {
            Value[Borrow.Locals[2].Start] = 27;
            Value[Borrowˉvalue.Locals[0].Start] = 27;
        } },
        { Name: 'result-parameter-and-return', Result: 42, Peak: 48, Change: Value => {
            const Extended = Appendˉhelper(Value, Resultˉshape, Resultˉshape, Buffer.from([205, 0, 0, 0, 0, 81]));
            const { Sections: Current, Functions: Entries } = Inspect(Extended), First = Entries[Construct.Index], Code = Current.get(5);
            return Replaceˉbody(Extended, First, Buffer.concat([
                Extended.subarray(Code.Start + First.Offset, Code.Start + First.Offset + First.Length - 1),
                Buffer.concat([Buffer.from([64]), Word(Functions.length), Buffer.from([81])])]));
        } },
        { Name: 'maximum-function-count', Result: 42, Peak: 48, Change: Value => Extraˉfunctions(Value, 64) },
        { Name: 'six-scalar-mutations', Result: 42, Peak: 32, Generation: 12, Input: Scalar },
        { Name: 'constructor-refusal', Result: 2, Peak: 0, Change: Value => { Value.writeBigUInt64LE(16n, Budget); } },
        { Name: 'physical-refusal', Result: 2, Peak: 0, Arena: 16 },
        { Name: 'invalid-constructor-cleanup', Status: 5, Peak: 0, Change: Value => { Value.writeBigUInt64LE(0n, Capacity); } },
        { Name: 'recursive-depth-cleanup', Status: 3, Peak: 48, Depth: 3 },
        { Name: 'nested-fuel-cleanup', Status: 2, Peak: 48, Fuel: 100 },
        { Name: 'borrowed-read-bounds-cleanup', Status: 5, Peak: 48,
            Change: Value => { Value.writeBigUInt64LE(2n, Borrowˉindex); } },
        { Name: 'replacement-bounds-cleanup', Status: 5, Peak: 48,
            Change: Value => { Value.writeBigUInt64LE(2n, Replaceˉindex); } },
        { Name: 'replacement-wide-index-cleanup', Status: 5, Peak: 48,
            Change: Value => { Value.writeBigUInt64LE(4294967296n, Replaceˉindex); } },
        { Name: 'empty-replacement-cleanup', Status: 5, Peak: 48,
            Change: Value => {
                Value.writeUInt32LE(2, Consume.Metadata + 8);
                return Replaceˉbody(Value, Consume, Buffer.concat([
                Buffer.from([129]), Buffer.alloc(8), Buffer.from([1, 44, 0, 0, 0, 228]),
                Word(2), Value.subarray(Replacement + 5, Replacement + 9),
                Buffer.from([205, 2, 0, 0, 0, 80, 81]),
                ]));
            } },
    ];
    const Objects = ['Owned', 'Allocator', 'Budget-Validation', 'Budgeted'].map(Name => join(Work, `${Name}.wvo`));
    const Tool = Name => join(Repository, `Tools/Native/${Name}.${Target === 'windows' ? 'cmd' : 'sh'}`);
    for (const [Index, Case] of Cases.entries()) {
        process.stdout.write(`native owned helpers step=execute item=${Index + 1}/${Cases.length} case=${Case.Name}\n`);
        const Prefix = join(Work, `Helper-${Case.Name}`), Value = Buffer.from(Case.Input ?? Input), Changed = Case.Change?.(Value);
        await writeFile(Prefix + '.wvb', Buffer.isBuffer(Changed) ? Changed : Value);
        const Lowered = await Requireˉsuccess(Lowerer, [Prefix + '.wvb', Prefix + '.wvo'], `helper-${Case.Name}-lower`);
        if (!/^native x64 status=Valid abi=24 /u.test(Lowered.Output)) throw new Error(`Helper lowering differs: ${Lowered.Output}`);
        if (Index === 0) {
            await Requireˉsuccess(Lowerer, [Prefix + '.wvb', Prefix + '-repeat.wvo'], 'helper-repeat');
            if (!(await readFile(Prefix + '.wvo')).equals(await readFile(Prefix + '-repeat.wvo'))) throw new Error('Helper lowering is not deterministic.');
        }
        const Fixture = Buildˉstorageˉfixture(Case.Name, Case.Arena ?? 64, ({ Emit, Set, Check }) => {
            Set('rsp', 14000, 10); Set('rsp', 14004, 136);
            Set('rsp', 14008, Case.Fuel ?? 1000000); Set('rsp', 14016, Case.Depth ?? 64);
            Emit('store_memory_u64 rsp none 1 14112 r12', 'load_address rax Windvale_budgeted_storage',
                'store_memory_u64 rsp none 1 14120 rax', 'move rdx rsp', 'add_i32 rdx 14000', 'call Main');
            if (Case.Status) Emit('shift_right rax 32', `compare_i32 eax ${Case.Status}`, 'branch not_equal Failed');
            else Emit(`compare_i32 rax ${Case.Result}`, 'branch not_equal Failed');
            Check('r12', 40, 1); Check('rsp', 60, 1); Check('rsp', 88, 0); Check('rsp', 17528, 0);
            Check('rsp', 56, Case.Peak);
            if (Case.Generation !== undefined) Check('rsp', 17560, Case.Generation);
            Emit('compare_i32 r15d 7171', 'branch not_equal Failed');
        }, true);
        await writeFile(Prefix + '.wva', Fixture.Source.replace('symbol export function Main in .text',
            'symbol export function Harness in .text\nsymbol import function Main').replace('define Main\n', 'define Harness\n'));
        await Requireˉsuccess(Tool('Assemble-Wva'), [Prefix + '.wva', Prefix + '-harness.wvo'], 'helper-harness');
        const Linked = await Requireˉsuccess(Tool('Link-Wvo'), ['0', 'Harness', Prefix + '.bin', Prefix + '-harness.wvo', Prefix + '.wvo', ...Objects], 'helper-link');
        const Address = /^entry name=Harness address=(\d+)$/mu.exec(Linked.Output)?.[1];
        if (Address === undefined) throw new Error('Helper harness entry missing.');
        const Application = Prefix + (Target === 'windows' ? '.exe' : '.elf');
        await Requireˉsuccess(Tool('Package-Console'), [`${Target}-x64-console-v1`, Prefix + '.bin', Address, Application], 'helper-package');
        const Start = performance.now(), Result = await Runˉprocess(Application, [], 30000, `helper-${Case.Name}`);
        if (Result.Code !== 42 || Result.Timedˉout || Result.Exceeded || Result.Output !== '') throw new Error(`Helper ${Case.Name} failed: ${Result.Code} ${Result.Output}`);
        process.stdout.write(`native owned helpers case=${Case.Name} status=Passed elapsed-ms=${Math.round(performance.now() - Start)}\n`);
    }
    const Rejections = [
        ...Buildˉvectorˉmutationˉrejections(Input),
        ['call-entry', Value => { Value.writeUInt32LE(Main.Index, Constructorˉcall + 1); }],
        ['missing-target', Value => { Value.writeUInt32LE(Functions.length, Recursiveˉcall + 1); }],
        ['wrong-parameter', Value => { Value[Forward.Locals[0].Start] = 2; }],
        ['copy-vector-parameter', Value => { Value[Forward.Operations.find(([Opcode]) => Opcode === 205)[1]] = 4; }],
        ['copy-result-return', Value => { Value[Construct.Operations.findLast(([Opcode]) => Opcode === 205)[1]] = 4; }],
        ['helper-bytes-parameter', Value => Appendˉhelper(Value, Buffer.from([6]), Buffer.from([6]), Buffer.from([4, 0, 0, 0, 0, 81]))],
        ['helper-borrow-parameter', Value => Appendˉhelper(Value, Buffer.from([37, 1]), Buffer.from([1]), Buffer.from([4, 0, 0, 0, 0, 81]))],
        ['immutable-vector-mutation', Value => Appendˉhelper(Value, Borrowˉshape, Buffer.from([1]),
            Buffer.concat([Buffer.from([1, 42, 0, 0, 0, 208]), Word(0), Word(Appendˉresult),
                Buffer.from([80, 1, 42, 0, 0, 0, 81])]))],
        ['borrowed-vector-return', Value => Appendˉhelper(Value, Borrowˉshape, Vectorˉshape,
            Buffer.from([4, 0, 0, 0, 0, 81]))],
        ['borrowed-vector-release', Value => Appendˉhelper(Value, Borrowˉshape, Buffer.from([1]),
            Buffer.from([205, 0, 0, 0, 0, 80, 1, 42, 0, 0, 0, 81]))],
        ['function-count-overflow', Value => Extraˉfunctions(Value, 65)],
        ['helper-code-limit', Value => { Value.writeUInt32LE(4097, Forward.Metadata + 4); }],
    ];
    for (const [Name, Change] of Rejections) {
        const Prefix = join(Work, `Helper-reject-${Name}`), Value = Buffer.from(Input), Changed = Change(Value);
        await writeFile(Prefix + '.wvb', Buffer.isBuffer(Changed) ? Changed : Value);
        const Result = await Runˉprocess(Lowerer, [Prefix + '.wvb', Prefix + '.wvo'], 30000, `helper-reject-${Name}`);
        if (Result.Code !== 1 || Result.Timedˉout || Result.Exceeded || existsSync(Prefix + '.wvo') ||
            !/^native x64 status=(Invalidˉwvb|Unsupportedˉmodule|Unsupportedˉfunction|Unsupportedˉcode) /u.test(Result.Output)) {
            throw new Error(`Owned helper rejection ${Name} differs: ${Result.Output}`);
        }
    }
    const Count = Frameˉcases.Cases + Cases.length + Rejections.length;
    process.stdout.write(`native owned helpers status=Passed cases=${Count} executions=${Cases.length + Frameˉcases.Executions} ` +
        `malformed=${Rejections.length + Frameˉcases.Malformed} iterations=1,1000,32768 arena-bytes=64 peak-charge=48 metadata-bytes=5816 abi=24\n`);
    return { Cases: Count, Valid: Frameˉcases.Valid + Cases.length, Malformed: Frameˉcases.Malformed + Rejections.length,
        Executions: Cases.length + Frameˉcases.Executions };
}
