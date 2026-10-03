import { Buildˉstorageˉfixture } from './Native-Storage-Fixture.mjs';
import { createHash } from 'node:crypto';
import { existsSync } from 'node:fs';
import { readFile, stat, writeFile } from 'node:fs/promises';
import { join } from 'node:path';

export async function Prepareˉownedˉvector(Context) {
    const { Repository, Work, Requireˉsuccess, Vectorˉproduct } = Context;
    const Wvb = Vectorˉproduct?.Path ?? join(Work, 'Owned-Vector.wvb');
    if (Vectorˉproduct) {
        if ((await stat(Wvb)).size > 1_048_576 ||
            createHash('sha256').update(await readFile(Wvb)).digest('hex') !== Vectorˉproduct.Sha256) {
            throw new Error('Supplied owned Vector fixture identity differs.');
        }
    } else {
        await Requireˉsuccess(process.execPath, [join(Repository, 'Tools/Native/Build-Current-Split-Project-Wvb.mjs'),
            '--prepared-compiler-only', join(Repository, 'Projects/Tests/Windvale-Native-Test-Owned-Vector-Scope.wvproj'), Wvb],
        'owned-vector-source-build');
    }
    return Wvb;
}

export async function Runˉownedˉvectorˉcases(Context, Lowerer) {
    const { Repository, Work, Target, Requireˉsuccess, Runˉprocess } = Context;
    const Tool = Name => join(Repository, 'Tools/Native', `${Name}.${Target === 'windows' ? 'cmd' : 'sh'}`);
    const Wvb = await Prepareˉownedˉvector(Context);
    const Input = await readFile(Wvb);
    if (Input.length > 1_048_576 || Input.readUInt16LE(6) !== 24) throw new Error('Owned Vector fixture version differs.');
    const Sections = new Map();
    for (let Cursor = 12; Cursor < Input.length;) {
        const Kind = Input.readUInt32LE(Cursor), Length = Input.readUInt32LE(Cursor + 4);
        if (Length > Input.length - Cursor - 8) throw new Error('Owned Vector section exceeds input.');
        Sections.set(Kind, { Start: Cursor + 8, Length }); Cursor += Length + 8;
    }
    const Code = Sections.get(5), Constants = [], Operations = [];
    const Sizes = new Map([[1,5],[2,2],[4,5],[5,5],[8,2],[9,5],[48,5],[49,5],
        [128,9],[129,9],[151,9],[152,9],[153,9],[196,9],[205,5],[206,9],[207,9]]);
    for (let Cursor = Code.Start; Cursor < Code.Start + Code.Length;) {
        const Opcode = Input[Cursor];
        Operations.push([Opcode, Cursor]);
        if (Opcode === 129) Constants.push(Cursor + 1);
        Cursor += Sizes.get(Opcode) ?? 1;
    }
    if (Constants.length !== 2 || Input.readBigUInt64LE(Constants[0]) !== 48n ||
        Input.readBigUInt64LE(Constants[1]) !== 1n) throw new Error('Owned Vector workload constants differ.');
    const Construct = Operations.find(([Opcode]) => Opcode === 207)?.[1];
    const Pop = Operations.find(([Opcode]) => Opcode === 80)?.[1];
    if (!Construct || !Pop || Input[Pop - 5] !== 205) throw new Error('Owned Vector scope operations differ.');

    const Entry = join(Work, 'Owned-Entry.wvo');
    const Stub = join(Work, 'Owned-Body.wvo');
    await Requireˉsuccess(Tool('Assemble-Wva'), [join(Repository, 'Runtime/Native/X64-Owned-Entry.wva'), Entry], 'owned-entry-assemble');
    const Stubˉsource = join(Work, 'Owned-Body.wva');
    await writeFile(Stubˉsource, 'windvale-assembly 1\nsymbol export function Windvale_owned_body in .text\n' +
        'section code .text align 16\ndefine Windvale_owned_body\nreturn\nend define\nend section\n');
    await Requireˉsuccess(Tool('Assemble-Wva'), [Stubˉsource, Stub], 'owned-entry-stub');
    const Template = join(Work, 'Owned-Template.bin');
    const Templateˉmap = await Requireˉsuccess(Tool('Link-Wvo'), ['0', 'Windvale_owned_body', Template, Entry, Stub], 'owned-entry-link');
    const Body = Number(/^entry name=Windvale_owned_body address=(\d+)$/mu.exec(Templateˉmap.Output)?.[1]);
    const Source = await readFile(join(Repository, 'Compiler/Windvale/Native-X64-Lowering-Owned-Collections.wv'), 'utf8');
    const Literal = /data Entryˉtemplate: bytes = \[([\s\S]*?)\];/u.exec(Source)?.[1];
    const Bytes = Buffer.from(Literal?.split(',').map(Value => Value.trim()).filter(Boolean).map(Number) ?? []);
    if (Body !== Bytes.length || !(await readFile(Template)).subarray(0, Body).equals(Bytes)) {
        throw new Error('Owned entry template differs from assembled runtime source.');
    }
    for (const [Name, Offset] of [['split', 442], ['vector', 705], ['release', 1151], ['access', 1451], ['grow', 1743]]) {
        if (!Templateˉmap.Output.includes(`name=Windvale_owned_${Name} address=${Offset} `)) {
            throw new Error(`Owned ${Name} helper offset differs.`);
        }
    }
    const Objects = ['Owned', 'Allocator', 'Budget-Validation', 'Budgeted'].map(Name => join(Work, `${Name}.wvo`));
    const Cases = [
        { Name: 'repeat-scope', Result: 42, Peak: 32, Generation: 2000 },
        { Name: 'budget-refusal', Result: 2, Peak: 0, Change: Value => Value.writeBigUInt64LE(4097n, Constants[0]) },
        { Name: 'constructor-budget-refusal', Result: 1, Peak: 0, Change: Value => Value.writeBigUInt64LE(16n, Constants[0]) },
        { Name: 'physical-refusal', Result: 1, Peak: 0, Arena: 16 },
        { Name: 'target-refusal', Result: 1, Peak: 0, Change: Value => Value.writeBigUInt64LE(2048n, Constants[1]) },
        { Name: 'overflow-refusal', Result: 1, Peak: 0, Change: Value => Value.writeBigUInt64LE(18446744073709551615n, Constants[1]) },
        { Name: 'invalid-limit-cleanup', Status: 5, Peak: 0, Change: Value => Value.writeBigUInt64LE(0n, Constants[1]) },
        { Name: 'fuel-cleanup', Status: 2, Fuel: 60, Peak: 32, Generation: 2 },
        { Name: 'depth-cleanup', Status: 3, Depth: 0, Peak: 0 },
        { Name: 'old-context', Status: 9, Version: 7, Closed: 0, Peak: 0 },
        { Name: 'context-length', Status: 9, Size: 112, Closed: 0, Peak: 0 },
    ];
    for (const [Index, Case] of Cases.entries()) {
        process.stdout.write(`native owned vector step=execute item=${Index + 1}/${Cases.length} case=${Case.Name}\n`);
        const Prefix = join(Work, `Vector-${Case.Name}`), Candidate = Buffer.from(Input);
        Case.Change?.(Candidate);
        await writeFile(Prefix + '.wvb', Candidate);
        const Lowered = await Requireˉsuccess(Lowerer, [Prefix + '.wvb', Prefix + '.wvo'], `vector-${Case.Name}-lower`);
        if (!/^native x64 status=Valid abi=24 code-bytes=\d+ object-bytes=\d+\r?\n$/u.test(Lowered.Output)) {
            throw new Error(`Owned Vector lowering differed: ${Lowered.Output}`);
        }
        if (Index === 0) {
            await Requireˉsuccess(Lowerer, [Prefix + '.wvb', Prefix + '-repeat.wvo'], 'vector-repeat-lower');
            if (!(await readFile(Prefix + '.wvo')).equals(await readFile(Prefix + '-repeat.wvo'))) {
                throw new Error('Owned Vector lowering is not deterministic.');
            }
        }
        const Fixture = Buildˉstorageˉfixture(Case.Name, Case.Arena ?? 64, ({ Emit, Set, Check }) => {
            Set('rsp', 14000, Case.Version ?? 10); Set('rsp', 14004, Case.Size ?? 136);
            Set('rsp', 14008, Case.Fuel ?? 1000000); Set('rsp', 14016, Case.Depth ?? 64);
            Emit('store_memory_u64 rsp none 1 14112 r12', 'load_address rax Windvale_budgeted_storage',
                'store_memory_u64 rsp none 1 14120 rax', 'move rdx rsp', 'add_i32 rdx 14000', 'call Main');
            if (Case.Status) Emit('shift_right rax 32', `compare_i32 eax ${Case.Status}`, 'branch not_equal Failed');
            else Emit(`compare_i32 rax ${Case.Result}`, 'branch not_equal Failed');
            Check('r12', 40, Case.Closed ?? 1); Check('rsp', 60, Case.Closed ?? 1);
            Check('rsp', 88, 0); Check('rsp', 17528, Case.Closed === 0 ? 1 : 0);
            if (Case.Peak !== undefined) Check('rsp', 56, Case.Peak);
            if (Case.Generation !== undefined) Check('rsp', 17560, Case.Generation);
            Emit('compare_i32 r15d 7171', 'branch not_equal Failed');
        }, true);
        const Harness = Fixture.Source.replace('symbol export function Main in .text',
            'symbol export function Harness in .text\nsymbol import function Main').replace('define Main\n', 'define Harness\n');
        await writeFile(Prefix + '.wva', Harness);
        await Requireˉsuccess(Tool('Assemble-Wva'), [Prefix + '.wva', Prefix + '-harness.wvo'], 'vector-harness');
        const Linked = await Requireˉsuccess(Tool('Link-Wvo'), ['0', 'Harness', Prefix + '.bin', Prefix + '-harness.wvo', Prefix + '.wvo', ...Objects], 'vector-link');
        const Address = /^entry name=Harness address=(\d+)$/mu.exec(Linked.Output)?.[1];
        if (Address === undefined) throw new Error('Owned Vector entry missing.');
        const Application = Prefix + (Target === 'windows' ? '.exe' : '.elf');
        await Requireˉsuccess(Tool('Package-Console'), [`${Target}-x64-console-v1`, Prefix + '.bin', Address, Application], 'vector-package');
        const Start = performance.now();
        const Result = await Runˉprocess(Application, [], 30000, `vector-${Case.Name}`);
        if (Result.Code !== 42 || Result.Timedˉout || Result.Exceeded || Result.Output !== '') {
            throw new Error(`Owned Vector ${Case.Name} failed: code=${Result.Code} output=${Result.Output}`);
        }
        process.stdout.write(`native owned vector case=${Case.Name} status=Passed elapsed-ms=${Math.round(performance.now() - Start)}\n`);
    }
    const Rejections = [
        ['copy-owner', Value => { Value[Code.Start] = 4; }],
        ['copy-before-release', Value => { Value[Pop - 5] = 4; }],
        ['consumed-budget', Value => Value.writeUInt32LE(0, Construct + 1)],
        ['wrong-result', Value => Value.writeUInt32LE(0, Construct + 5)],
        ['unknown-vector', Value => Value.writeUInt32LE(65, Construct + 5)],
        ['unsupported-vector-operation', Value => { Value[Construct] = 200; }],
        ['owner-retained-at-join', Value => {
            // Keep the Vector alive after its using block. The forward join
            // must reject mismatched owners rather than intersect them away.
            const Body = Buffer.concat([Value.subarray(Code.Start, Pop),
                Buffer.from([5, ...Value.subarray(Pop - 4, Pop)]),
                Value.subarray(Pop + 1, Code.Start + Code.Length)]);
            for (const [Opcode, Offset] of Operations) {
                if (Opcode !== 48 && Opcode !== 49) continue;
                const Prior = Offset - Code.Start, Current = Prior + (Offset > Pop ? 4 : 0);
                const Target = Value.readUInt32LE(Offset + 1);
                Body.writeUInt32LE(Target + (Target > Pop - Code.Start ? 4 : 0), Current + 1);
            }
            const Functions = Sections.get(4);
            Value.writeUInt32LE(Code.Length + 4, Functions.Start + Functions.Length - 8);
            Value.writeUInt32LE(Code.Length + 4, Code.Start - 4);
            return Buffer.concat([Value.subarray(0, Code.Start), Body, Value.subarray(Code.Start + Code.Length)]);
        }],
        ['truncated', Value => Value.subarray(0, Value.length - 1)],
        ['oversized', Value => Buffer.concat([Value, Buffer.alloc(1_048_577 - Value.length)])],
    ];
    for (const [Name, Change] of Rejections) {
        const Candidate = Buffer.from(Input), Changed = Change(Candidate);
        const Prefix = join(Work, `Vector-reject-${Name}`);
        await writeFile(Prefix + '.wvb', Buffer.isBuffer(Changed) ? Changed : Candidate);
        const Result = await Runˉprocess(Lowerer, [Prefix + '.wvb', Prefix + '.wvo'], 30000, `vector-reject-${Name}`);
        if (Result.Code !== 1 || Result.Timedˉout || Result.Exceeded || existsSync(Prefix + '.wvo') ||
            !/^native x64 status=(Invalidˉwvb|Unsupportedˉmodule|Unsupportedˉfunction|Unsupportedˉcode) /u.test(Result.Output)) {
            throw new Error(`Owned Vector rejection ${Name} differed: ${Result.Output}`);
        }
    }
    process.stdout.write('native owned vector status=Passed cases=20 executions=11 rejections=9 iterations=1000 arena-bytes=64 peak-charge=32 metadata-bytes=5816 abi=24\n');
    return { Cases: 20, Valid: 11, Malformed: 9, Executions: 11 };
}
