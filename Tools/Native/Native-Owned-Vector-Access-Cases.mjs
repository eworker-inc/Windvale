import { Buildˉstorageˉfixture } from './Native-Storage-Fixture.mjs';
import { createHash } from 'node:crypto';
import { existsSync } from 'node:fs';
import { readFile, stat, writeFile } from 'node:fs/promises';
import { join } from 'node:path';

// Source construction, bytecode mutations and native execution stay in the
// existing lowering owner; this module adds no independent verifier entry.
export async function Prepareˉvectorˉaccess(Context) {
    const Result = [];
    for (const [Name, Product] of [['Growth', Context.Growthˉproduct], ['Append-Refusal', Context.Appendˉproduct]]) {
        const Path = Product?.Path ?? join(Context.Work, `Owned-Vector-${Name}.wvb`);
        if (Product) {
            if ((await stat(Path)).size > 1_048_576 ||
                createHash('sha256').update(await readFile(Path)).digest('hex') !== Product.Sha256) {
                throw new Error(`Supplied Vector ${Name} identity differs.`);
            }
        } else {
            await Context.Requireˉsuccess(process.execPath,
                [join(Context.Repository, 'Tools/Native/Build-Current-Split-Project-Wvb.mjs'), '--prepared-compiler-only',
                    join(Context.Repository, `Projects/Tests/Windvale-Native-Test-Owned-Vector-${Name}.wvproj`), Path],
                `vector-${Name}-build`);
        }
        Result.push(await readFile(Path));
    }
    return Result;
}

function Inspect(Input) {
    if (Input.readUInt16LE(6) !== 41) throw new Error('Vector indexed-access version differs.');
    const Sections = new Map();
    for (let Cursor = 12; Cursor < Input.length;) {
        const Kind = Input.readUInt32LE(Cursor), Length = Input.readUInt32LE(Cursor + 4);
        if (Length > Input.length - Cursor - 8) throw new Error('Vector section exceeds input.');
        Sections.set(Kind, { Start: Cursor + 8, Length }); Cursor += 8 + Length;
    }
    const Code = Sections.get(5), Operations = [], Constants = [];
    const Sizes = new Map([[1,5],[2,2],[4,5],[5,5],[8,2],[9,5],[48,5],[49,5],
        [104,5],[105,5],[106,9],[128,9],[129,9],[151,9],[152,9],[153,9],
        [196,9],[202,5],[205,5],[206,9],[207,9],[208,9],[209,13],[227,9]]);
    for (let Cursor = Code.Start; Cursor < Code.Start + Code.Length;) {
        const Opcode = Input[Cursor]; Operations.push([Opcode, Cursor]);
        if (Opcode === 129) Constants.push(Cursor + 1);
        Cursor += Sizes.get(Opcode) ?? 1;
    }
    return { Sections, Operations, Constants };
}

export async function Runˉvectorˉaccess(Context, Lowerer) {
    const { Repository, Work, Target, Requireˉsuccess, Runˉprocess } = Context;
    const Tool = Name => join(Repository, `Tools/Native/${Name}.${Target === 'windows' ? 'cmd' : 'sh'}`);
    const [Growth, Append] = await Prepareˉvectorˉaccess(Context);
    const { Sections, Operations, Constants } = Inspect(Growth);
    if (Constants.length !== 6 || [32n,1n,2n,2n,0n,1n].some((Value, Index) =>
        Growth.readBigUInt64LE(Constants[Index]) !== Value)) throw new Error('Vector growth constants differ.');
    const Grow = Operations.find(([Opcode]) => Opcode === 209)?.[1];
    const Read = Operations.find(([Opcode]) => Opcode === 227)?.[1];
    const Appendˉoperation = Operations.find(([Opcode]) => Opcode === 208)?.[1];
    if (!Grow || !Read || !Appendˉoperation) throw new Error('Vector access operations missing.');
    Inspect(Append);
    const Cases = [
        { Name: 'growth-values-and-cleanup', Input: Growth, Result: 42, Peak: 80 },
        { Name: 'append-refusal-retains-item', Input: Append, Result: 42, Peak: 32 },
        { Name: 'growth-budget-refusal', Input: Growth, Result: 4, Peak: 32, Budget: 64 },
        { Name: 'growth-physical-refusal', Input: Growth, Result: 4, Peak: 32, Arena: 64 },
        { Name: 'growth-target-refusal', Input: Growth, Result: 4, Peak: 32, Change: Value => Value.writeBigUInt64LE(2048n, Constants[2]) },
        { Name: 'growth-overflow-refusal', Input: Growth, Result: 4, Peak: 32, Change: Value => Value.writeBigUInt64LE(18446744073709551615n, Constants[2]) },
        { Name: 'growth-invalid-limit', Input: Growth, Status: 5, Peak: 32, Change: Value => Value.writeBigUInt64LE(1n, Constants[2]) },
        { Name: 'indexed-read-boundary', Input: Growth, Status: 5, Peak: 80, Change: Value => Value.writeBigUInt64LE(2n, Constants[5]) },
        { Name: 'indexed-read-overflow', Input: Growth, Status: 5, Peak: 80, Change: Value => Value.writeBigUInt64LE(18446744073709551615n, Constants[5]) },
    ];
    const Objects = ['Owned', 'Allocator', 'Budget-Validation', 'Budgeted'].map(Name => join(Work, `${Name}.wvo`));
    for (const [Index, Case] of Cases.entries()) {
        process.stdout.write(`native vector access step=execute item=${Index + 1}/${Cases.length} case=${Case.Name}\n`);
        const Prefix = join(Work, `Access-${Case.Name}`), Candidate = Buffer.from(Case.Input);
        Case.Change?.(Candidate); await writeFile(Prefix + '.wvb', Candidate);
        const Lowered = await Requireˉsuccess(Lowerer, [Prefix + '.wvb', Prefix + '.wvo'], `access-${Case.Name}-lower`);
        if (!/^native x64 status=Valid abi=24 /u.test(Lowered.Output)) throw new Error(`Vector access lowering differs: ${Lowered.Output}`);
        if (Index < 2) {
            await Requireˉsuccess(Lowerer, [Prefix + '.wvb', Prefix + '-repeat.wvo'], 'access-repeat');
            if (!(await readFile(Prefix + '.wvo')).equals(await readFile(Prefix + '-repeat.wvo'))) throw new Error('Vector access is not deterministic.');
        }
        const Fixture = Buildˉstorageˉfixture(Case.Name, Case.Arena ?? 80, ({ Emit, Set, Check }) => {
            if (Case.Budget) Set('rsp', 17544, Case.Budget);
            Set('rsp', 14000, 10); Set('rsp', 14004, 136); Set('rsp', 14008, 1000000); Set('rsp', 14016, 64);
            Emit('store_memory_u64 rsp none 1 14112 r12', 'load_address rax Windvale_budgeted_storage',
                'store_memory_u64 rsp none 1 14120 rax', 'move rdx rsp', 'add_i32 rdx 14000', 'call Main');
            if (Case.Status) Emit('shift_right rax 32', `compare_i32 eax ${Case.Status}`, 'branch not_equal Failed');
            else Emit(`compare_i32 rax ${Case.Result}`, 'branch not_equal Failed');
            Check('r12', 40, 1); Check('rsp', 60, 1); Check('rsp', 88, 0); Check('rsp', 17528, 0);
            Check('rsp', 56, Case.Peak); Emit('compare_i32 r15d 7171', 'branch not_equal Failed');
        }, true);
        const Harness = Fixture.Source.replace('symbol export function Main in .text',
            'symbol export function Harness in .text\nsymbol import function Main').replace('define Main\n', 'define Harness\n');
        await writeFile(Prefix + '.wva', Harness);
        await Requireˉsuccess(Tool('Assemble-Wva'), [Prefix + '.wva', Prefix + '-harness.wvo'], 'access-harness');
        const Linked = await Requireˉsuccess(Tool('Link-Wvo'), ['0', 'Harness', Prefix + '.bin', Prefix + '-harness.wvo', Prefix + '.wvo', ...Objects], 'access-link');
        const Entry = /^entry name=Harness address=(\d+)$/mu.exec(Linked.Output)?.[1];
        if (Entry === undefined) throw new Error('Vector access entry missing.');
        const Application = Prefix + (Target === 'windows' ? '.exe' : '.elf');
        await Requireˉsuccess(Tool('Package-Console'), [`${Target}-x64-console-v1`, Prefix + '.bin', Entry, Application], 'access-package');
        const Start = performance.now();
        const Result = await Runˉprocess(Application, [], 30000, `access-${Case.Name}`);
        if (Result.Code !== 42 || Result.Timedˉout || Result.Exceeded || Result.Output !== '') {
            throw new Error(`Vector access ${Case.Name} failed: code=${Result.Code} output=${Result.Output}`);
        }
        process.stdout.write(`native vector access case=${Case.Name} status=Passed elapsed-ms=${Math.round(performance.now() - Start)}\n`);
    }
    function Extraˉlocals(Value, Total) {
        const Functions = Sections.get(4);
        // One Main, one scalar budget parameter and an i32 return.
        const Count = Functions.Start + 18;
        const Existing = Value.readUInt32LE(Count), Declared = Total - 1, Extra = Declared - Existing;
        if (Extra <= 0) throw new Error('Vector local-limit fixture differs.');
        const Metadata = Functions.Start + Functions.Length - 12;
        Value.writeUInt32LE(Declared, Count);
        Value.writeUInt32LE(Functions.Length + Extra, Functions.Start - 4);
        return Buffer.concat([Value.subarray(0, Metadata), Buffer.alloc(Extra, 1), Value.subarray(Metadata)]);
    }
    // A unit constant in WVB 1.11 must fail version admission, before it can
    // reach the historical budget-split emitter through the shared dispatcher.
    const Baseline = await readFile(join(Repository, 'Artifacts/Native-Wvb-To-Wvo-Candidate/Return-42.wvb'));
    if (createHash('sha256').update(Baseline).digest('hex') !==
        '7933c4ba0cb854477a95750966f9532c2b9eb5888e55ec9ae64ebdf552a08f31') {
        throw new Error('Vector old-version baseline identity differs.');
    }
    const Oldˉparts = [Baseline.subarray(0, 12)];
    for (let Cursor = 12; Cursor < Baseline.length;) {
        const Kind = Baseline.readUInt32LE(Cursor), Length = Baseline.readUInt32LE(Cursor + 4);
        let Payload = Buffer.from(Baseline.subarray(Cursor + 8, Cursor + 8 + Length));
        if (Kind === 4) {
            Payload.writeUInt32LE(2, 17); Payload.writeUInt32LE(22, 26);
            Payload = Buffer.concat([Payload.subarray(0, 22), Buffer.from([5]), Payload.subarray(22)]);
        }
        if (Kind === 5) Payload = Buffer.concat([Buffer.from([195, 5, 1, 0, 0, 0]), Payload]);
        const Header = Buffer.alloc(8); Header.writeUInt32LE(Kind); Header.writeUInt32LE(Payload.length, 4);
        Oldˉparts.push(Header, Payload); Cursor += 8 + Length;
    }
    const Rejections = [
        ['wrong-growth-owner', Value => Value.writeUInt32LE(0, Grow + 1)],
        ['wrong-growth-budget', Value => Value.writeUInt32LE(Value.readUInt32LE(Grow + 1), Grow + 5)],
        ['wrong-growth-result', Value => Value.writeUInt32LE(0, Grow + 9)],
        ['wrong-append-result', Value => Value.writeUInt32LE(0, Appendˉoperation + 5)],
        ['wrong-read-owner', Value => Value.writeUInt32LE(0, Read + 1)],
        ['wrong-read-type', Value => Value.writeUInt32LE(0, Read + 5)],
        ['earlier-version', Value => Value.writeUInt16LE(27, 6)],
        ['truncated-growth', Value => Value.subarray(0, Grow + 12)],
        ['local-limit', Value => Extraˉlocals(Value, 129)],
        ['unit-in-old-version', () => Buffer.concat(Oldˉparts)],
    ];
    for (const [Name, Change] of Rejections) {
        const Input = Buffer.from(Growth), Changed = Change(Input), Prefix = join(Work, `Access-reject-${Name}`);
        await writeFile(Prefix + '.wvb', Buffer.isBuffer(Changed) ? Changed : Input);
        const Result = await Runˉprocess(Lowerer, [Prefix + '.wvb', Prefix + '.wvo'], 30000, `access-reject-${Name}`);
        if (Result.Code !== 1 || Result.Timedˉout || Result.Exceeded || existsSync(Prefix + '.wvo') ||
            !/^native x64 status=(Invalidˉwvb|Unsupportedˉmodule|Unsupportedˉfunction|Unsupportedˉcode) /u.test(Result.Output)) {
            throw new Error(`Vector access rejection ${Name} differed: ${Result.Output}`);
        }
    }
    return { Cases: Cases.length + Rejections.length, Valid: Cases.length, Malformed: Rejections.length, Executions: Cases.length };
}
