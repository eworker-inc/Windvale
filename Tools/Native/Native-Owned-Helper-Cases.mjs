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
    return readFile(Path);
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
        if ([7, 8, 11, 23, 28, 29, 35].includes(Kind)) Cursor += 4;
        else if (Kind === 37) Shape();
        return { Start, Length: Cursor - Start, Kind };
    }
    const Count = Input.readUInt32LE(Table.Start);
    if (Count > 65) throw new Error('Helper function count exceeds fixture bound.');
    const Sizes = new Map([[1,5],[2,2],[4,5],[5,5],[8,2],[9,5],[48,5],[49,5],[64,5],
        [104,5],[105,5],[106,9],[128,9],[129,9],[151,9],[152,9],[153,9],
        [196,9],[202,5],[205,5],[206,9],[207,9],[208,9],[209,13],[227,9]]);
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

function Appendˉhelper(Input, Parameter, Return, Body) {
    const { Sections, Functions } = Inspect(Input), Table = Sections.get(4), Code = Sections.get(5);
    const Name = Buffer.from('Transfer');
    const Entry = Buffer.concat([Word(Name.length), Name, Word(1), Parameter, Return,
        Word(0), Word(Code.Length), Word(Body.length), Word(1)]);
    return Section(Section(Input, 5, Buffer.concat([Input.subarray(Code.Start, Code.Start + Code.Length), Body])), 4,
        Buffer.concat([Word(Functions.length + 1), Input.subarray(Table.Start + 4, Table.Start + Table.Length), Entry]));
}

export async function Runˉownedˉhelpers(Context, Lowerer) {
    const { Repository, Work, Target, Requireˉsuccess, Runˉprocess } = Context;
    const Input = await Prepareˉownedˉhelpers(Context), { Sections, Functions } = Inspect(Input);
    if (Input.readUInt16LE(6) !== 42 || Functions.map(Item => Item.Name).join(',') !== 'Construct,Consume,Forward,Main') {
        throw new Error('Owned helper source workload differs.');
    }
    const [Construct, Consume, Forward, Main] = Functions;
    const Capacity = Construct.Operations.find(([Opcode]) => Opcode === 129)[1] + 1;
    const Budget = Main.Operations.find(([Opcode]) => Opcode === 129)[1] + 1;
    const Constructorˉcall = Main.Operations.find(([Opcode, At]) => Opcode === 64 && Input.readUInt32LE(At + 1) === Construct.Index)[1];
    const Recursiveˉcall = Forward.Operations.find(([Opcode]) => Opcode === 64)[1];
    const Drop = Consume.Operations.find(([Opcode]) => Opcode === 80)[1];
    const Resultˉshape = Input.subarray(Construct.Return.Start, Construct.Return.Start + Construct.Return.Length);
    const Cases = [
        { Name: 'nested-recursive-transfer', Result: 42, Peak: 32, Generation: 2000 },
        { Name: 'result-parameter-and-return', Result: 42, Peak: 32, Change: Value => {
            const Extended = Appendˉhelper(Value, Resultˉshape, Resultˉshape, Buffer.from([205, 0, 0, 0, 0, 81]));
            const { Sections: Current, Functions: Entries } = Inspect(Extended), First = Entries[0], Code = Current.get(5);
            return Replaceˉbody(Extended, First, Buffer.concat([
                Extended.subarray(Code.Start + First.Offset, Code.Start + First.Offset + First.Length - 1),
                Buffer.from([64, 4, 0, 0, 0, 81])]));
        } },
        { Name: 'maximum-function-count', Result: 42, Peak: 32, Change: Value => Extraˉfunctions(Value, 64) },
        { Name: 'constructor-refusal', Result: 2, Peak: 0, Change: Value => { Value.writeBigUInt64LE(16n, Budget); } },
        { Name: 'physical-refusal', Result: 2, Peak: 0, Arena: 16 },
        { Name: 'invalid-constructor-cleanup', Status: 5, Peak: 0, Change: Value => { Value.writeBigUInt64LE(0n, Capacity); } },
        { Name: 'recursive-depth-cleanup', Status: 3, Peak: 32, Depth: 3 },
        { Name: 'nested-fuel-cleanup', Status: 2, Peak: 32, Fuel: 100 },
    ];
    const Objects = ['Owned', 'Allocator', 'Budget-Validation', 'Budgeted'].map(Name => join(Work, `${Name}.wvo`));
    const Tool = Name => join(Repository, `Tools/Native/${Name}.${Target === 'windows' ? 'cmd' : 'sh'}`);
    for (const [Index, Case] of Cases.entries()) {
        process.stdout.write(`native owned helpers step=execute item=${Index + 1}/${Cases.length} case=${Case.Name}\n`);
        const Prefix = join(Work, `Helper-${Case.Name}`), Value = Buffer.from(Input), Changed = Case.Change?.(Value);
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
        ['call-entry', Value => { Value.writeUInt32LE(Main.Index, Constructorˉcall + 1); }],
        ['missing-target', Value => { Value.writeUInt32LE(4, Recursiveˉcall + 1); }],
        ['wrong-parameter', Value => { Value[Forward.Locals[0].Start] = 2; }],
        ['copy-vector-parameter', Value => { Value[Forward.Operations.find(([Opcode]) => Opcode === 205)[1]] = 4; }],
        ['copy-result-return', Value => { Value[Construct.Operations.findLast(([Opcode]) => Opcode === 205)[1]] = 4; }],
        ['helper-retains-budget', Value => Appendˉhelper(Value, Buffer.from([25]), Buffer.from([1]), Buffer.from([1, 42, 0, 0, 0, 81]))],
        ['helper-retains-result', Value => Appendˉhelper(Value, Resultˉshape, Buffer.from([1]), Buffer.from([1, 42, 0, 0, 0, 81]))],
        ['helper-bytes-parameter', Value => Appendˉhelper(Value, Buffer.from([6]), Buffer.from([6]), Buffer.from([4, 0, 0, 0, 0, 81]))],
        ['helper-borrow-parameter', Value => Appendˉhelper(Value, Buffer.from([37, 1]), Buffer.from([1]), Buffer.from([4, 0, 0, 0, 0, 81]))],
        ['helper-retains-vector', Value => {
            const Code = Sections.get(5), Start = Code.Start + Consume.Offset, End = Start + Consume.Length;
            const Body = Buffer.concat([Value.subarray(Start, Drop - 5), Value.subarray(Drop + 1, End)]);
            for (const [Opcode, At] of Consume.Operations) {
                if (Opcode !== 48 && Opcode !== 49) continue;
                const Target = Value.readUInt32LE(At + 1), Current = At - Start - (At > Drop ? 6 : 0);
                Body.writeUInt32LE(Target - (Target > Drop - Start ? 6 : 0), Current + 1);
            }
            return Replaceˉbody(Value, Consume, Body);
        }],
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
    process.stdout.write('native owned helpers status=Passed cases=20 executions=8 rejections=12 iterations=1000 arena-bytes=64 peak-charge=32 metadata-bytes=5816 abi=24\n');
    return { Cases: 20, Valid: 8, Malformed: 12, Executions: 8 };
}
