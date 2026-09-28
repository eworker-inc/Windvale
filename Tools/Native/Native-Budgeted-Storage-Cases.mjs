import { Buildˉstorageˉfixture } from './Native-Storage-Fixture.mjs';
import { createHash } from 'node:crypto';
import { readFile, stat } from 'node:fs/promises';
import { join } from 'node:path';

const BUDGET_BASE = 17504;

export async function Prepareˉbudgetˉoracle(Context) {
    const { Repository, Work, Target, Requireˉsuccess, Oracleˉproduct } = Context;
    const Wvb = Oracleˉproduct?.Path ?? join(Work, 'Budget-Oracle.wvb');
    process.stdout.write('native budgeted storage step=accounting-oracle-prepare\n');
    if (Oracleˉproduct) {
        if ((await stat(Wvb)).size > 1_048_576 ||
            createHash('sha256').update(await readFile(Wvb)).digest('hex') !== Oracleˉproduct.Sha256) {
            throw new Error('Supplied budget oracle identity differs.');
        }
    } else {
        await Requireˉsuccess(process.execPath, [join(Repository, 'Tools/Native/Build-Current-Split-Project-Wvb.mjs'),
            '--prepared-compiler-only', join(Repository, 'Projects/Tests/Windvale-Native-Test-Budgeted-Storage-Accounting-Oracle.wvproj'), Wvb],
        'budget-oracle-build');
    }
    const Application = join(Work, Target === 'windows' ? 'Budget-Oracle.exe' : 'Budget-Oracle.elf');
    await Requireˉsuccess(join(Repository, `Tools/Native/Package-Segmented-Compiler-Wvb.${Target === 'windows' ? 'cmd' : 'sh'}`),
        ['2', Wvb, Application, '--development-cache'], 'budget-oracle-package');
    return Application;
}

export async function Readˉbudgetˉoracle(Context) {
    const Application = await Prepareˉbudgetˉoracle(Context);
    const Output = join(Context.Work, 'Budget-States.bin');
    await Context.Requireˉsuccess(Application, [Output], 'budget-oracle-execute');
    if ((await stat(Output)).size !== 36624) throw new Error('Budget oracle checkpoint length differs.');
    const Bytes = await readFile(Output);
    process.stdout.write('native budgeted storage accounting-oracle status=Passed states=14 bytes=36624\n');
    return Array.from({ length: 14 }, (_, Index) => Bytes.subarray(Index * 2616, (Index + 1) * 2616));
}

export function Buildˉbudgetedˉstorageˉcases(Oracle) {
    const Cases = [];
    function Case(Name, Arena, Body) {
        Cases.push(Buildˉstorageˉfixture(`budget-${Name}`, Arena, Fixture => {
            const { Emit, Set, Check, Request, Call, Save, Load } = Fixture;
            const Offset = Identity => BUDGET_BASE + 16 + (Identity - 1) * 40;
            function Entry(Identity, Generation, Parent, Maximum, Children = 0, Reserved = 0, Owner = 1) {
                const Base = Offset(Identity);
                for (let Index = 0; Index < 40; Index += 4) Set('rsp', Base + Index, 0);
                for (const [Index, Value] of [[0, Generation], [4, Parent], [8, 1], [12, Owner],
                    [16, 64], [20, Children], [24, Maximum], [32, Reserved]]) Set('rsp', Base + Index, Value);
            }
            function Field(Identity, Index, Value) { Check('rsp', Offset(Identity) + Index, Value); }
            function Allocate(Identity, Generation, Bytes = 17, Slot = 0, Status = 0) {
                Request(1, { Capacity: Bytes, Length: Bytes, Alignment: 16 });
                Set('r13', 64, Identity); Set('r13', 68, Generation); Call(Status);
                if (Status === 0) {
                    Save(24, Slot); Save(48, Slot + 1);
                    for (let Index = 0; Index < 4; Index++) Save(64 + Index * 8, Slot + 2 + Index);
                }
            }
            function Begin(Operation, Slot = 0, Length = 0) {
                Request(Operation, { Handle: Slot, Length });
                for (let Index = 0; Index < 4; Index++) {
                    Load(Slot + 2 + Index); Emit(`store_memory_u64 r13 none 1 ${64 + Index * 8} rax`);
                }
            }
            function Release(Slot = 0) { Begin(3, Slot); Call(); }
            function Replace(Bytes = 33, Prefix = 17, Slot = 0, Budget = 1) {
                Request(11, { Capacity: Bytes, Length: Bytes, Alignment: 16, Handle: Slot });
                Set('r13', 64, Budget); Set('r13', 68, 1); Set('r13', 88, Prefix);
            }
            Body({ ...Fixture, Offset, Entry, Field, Allocate, Begin, Release, Replace });
        }, true, Oracle));
    }
    Case('commit-and-release', 64, ({ Entry, Field, Allocate, Begin, Release, Check, Call, Emit, Load, Set, Oracleˉstate }) => {
        Oracleˉstate(0);
        Entry(1, 1, 0, 4096, 1, 48); Entry(2, 1, 1, 48);
        Oracleˉstate(1);
        Allocate(2, 1); Field(1, 32, 48); Field(2, 0, 2); Check('rsp', 88, 48);
        Oracleˉstate(2);
        Check('r13', 72, 48); Check('r13', 80, 48); Check('r13', 88, 16);
        Begin(2, 0, 0); Call(); Field(1, 32, 48); Check('rsp', 88, 48);
        Load(1, 'rdi'); Set('rdi', 0, 97); Begin(2, 0, 17); Call(); Check('rdi', 0, 0);
        Begin(4); Call(); Begin(4); Set('r13', 80, 47); Call(4);
        Release(); Field(1, 32, 0); Field(1, 20, 0); Field(2, 8, 0); Field(2, 0, 2); Check('rsp', 88, 0);
        Oracleˉstate(3);
        Begin(3); Call(4);
        Entry(1, 1, 0, 4096, 1, 48); Entry(2, 3, 1, 48);
        Allocate(2, 3, 17, 8); Load(1, 'rcx'); Load(9); Emit('compare rax rcx', 'branch not_equal Failed');
        Begin(3, 0); Call(4); Release(8); Field(1, 32, 0);
    });
    Case('atomic-refusal', 32, ({ Emit, Entry, Field, Allocate, Request, Set, Call, Snapshot, Check }) => {
        Entry(1, 1, 0, 4096, 1, 47); Entry(2, 1, 1, 47);
        Allocate(2, 1, 17, 0, 2); Field(2, 0, 1);
        Entry(1, 1, 0, 4096, 1, 48); Entry(2, 1, 1, 48);
        Allocate(2, 1, 17, 0, 3); Field(2, 0, 1);
        Allocate(2, 3, 1, 0, 4);
        Entry(2, 4294967295, 1, 48); Allocate(2, 4294967295, 1, 0, 6);
        Entry(2, 1, 1, 48);
        for (const [Index, Value, Status] of [[32, 3, 1], [32, 32, 8], [36, 2, 7], [56, 32, 1], [72, 32, 1], [92, 1, 1]]) {
            Request(1, { Capacity: 1, Length: 1, Alignment: 16 });
            Set('r13', 64, 2); Set('r13', 68, 1); Set('r13', Index, Value); Call(Status);
        }
        for (const Address of [16416, 32, BUDGET_BASE, 2240]) {
            Request(0); Snapshot(false);
            Emit('move r8 r12', 'move r9 rsp', `add_i32 r9 ${Address}`, 'call Windvale_budgeted_storage',
                'compare_i32 eax 1', 'branch not_equal Failed');
            Snapshot(true); Check('r13', 40, 0);
        }
    });
    Case('retained-parent', 144, ({ Entry, Field, Allocate, Release, Check, Oracleˉstate }) => {
        Entry(1, 1, 0, 4096, 1, 96); Entry(2, 1, 1, 96, 1, 48); Entry(3, 1, 2, 48);
        Oracleˉstate(4); Allocate(2, 1); Oracleˉstate(5); Release(); Oracleˉstate(6);
        Field(1, 32, 96); Field(2, 8, 1); Field(2, 12, 0); Field(2, 32, 48);
        Check('rsp', 88, 0);
        Allocate(3, 1, 17, 8); Oracleˉstate(7); Release(8); Oracleˉstate(8);
        Field(1, 32, 0); Field(2, 8, 0); Field(3, 8, 0);
    });
    Case('released-ancestor-chain', 64, ({ Entry, Field, Allocate, Release, Oracleˉstate }) => {
        Entry(1, 1, 0, 4096, 1, 96, 0); Entry(2, 1, 1, 96, 1, 48, 0); Entry(3, 1, 2, 48);
        Oracleˉstate(9); Allocate(3, 1); Release(); Oracleˉstate(10);
        for (let Identity = 1; Identity <= 3; Identity++) { Field(Identity, 8, 0); Field(Identity, 32, 0); }
    });
    Case('oracle-teardown', 144, ({ Entry, Allocate, Release, Request, Call, Oracleˉstate }) => {
        Entry(1, 1, 0, 4096, 1, 96); Entry(2, 1, 1, 96, 1, 48); Entry(3, 1, 2, 48);
        Allocate(2, 1); Release(); Allocate(3, 1); Oracleˉstate(7);
        Request(5); Call(); Oracleˉstate(11);
    });
    Case('binding-corruption', 144, ({ Entry, Allocate, Begin, Set, Call, Offset, Request }) => {
        Entry(1, 1, 0, 4096, 2, 96); Entry(2, 1, 1, 48); Entry(3, 1, 1, 48);
        Allocate(2, 1); Allocate(3, 1, 17, 8);
        for (const [Base, Index, Value, Original] of [
            ['r12', 64, 2, 1], ['r12', 76, 1, 2], ['r12', 72, 3, 2],
            ['rsp', Offset(2) + 0, 1, 2], ['rsp', Offset(2) + 24, 47, 48],
        ]) { Set(Base, Index, Value); Begin(3); Call(5); Set(Base, Index, Original); }
        Request(5); Call();
    });
    Case('budget-corruption', 64, ({ Entry, Allocate, Offset, Set }) => {
        Entry(1, 1, 0, 4096, 1, 48); Entry(2, 1, 1, 48);
        for (const [Index, Value, Original] of [
            [BUDGET_BASE, 0, 1112364631], [Offset(1) + 32, 49, 48],
            [Offset(2) + 8, 2, 1], [Offset(2) + 12, 2, 1],
            [Offset(2) + 4, 66, 1], [Offset(2) + 4, 2, 1],
            [Offset(2) + 0, 0, 1], [Offset(2) + 32, 49, 0],
        ]) { Set('rsp', Index, Value); Allocate(2, 1, 1, 0, 5); Set('rsp', Index, Original); }
        Entry(1, 1, 0, 4294967295, 2, 47); Set('rsp', Offset(1) + 28, 4294967295);
        Entry(2, 1, 1, 4294967295); Set('rsp', Offset(2) + 28, 4294967295); Entry(3, 1, 1, 48);
        Allocate(2, 1, 1, 0, 5); // Wrapped sum would falsely equal the parent's 47-byte reservation.
        Entry(1, 1, 0, 4096); Entry(2, 1, 3, 48, 1, 48); Entry(3, 1, 2, 48, 1, 48);
        Allocate(2, 1, 1, 0, 5);
    });
    Case('teardown-and-slot-refusal', 4096, ({ Emit, Entry, Field, Request, Set, Call, Check }) => {
        Entry(1, 1, 0, 4096, 64, 2048);
        // Build the fixture's already-split canonical domain table in a loop.
        Emit('move_u32 ecx 17560', 'label Seed_children', 'move_u32 eax 1',
            'store_memory_u32 rsp rcx 1 0 eax', 'store_memory_u32 rsp rcx 1 4 eax',
            'store_memory_u32 rsp rcx 1 8 eax', 'store_memory_u32 rsp rcx 1 12 eax',
            'move_u32 eax 32', 'store_memory_u32 rsp rcx 1 24 eax',
            'add_i32 ecx 40', 'compare_i32 ecx 20120', 'branch below Seed_children');
        Set('rsp', 13000, 2); Emit('label Fill_slots');
        Request(1, { Capacity: 1, Length: 1, Alignment: 16 });
        Emit('load_memory_u32 eax rsp none 1 13000', 'store_memory_u32 r13 none 1 64 eax'); Set('r13', 68, 1); Call();
        Emit('load_memory_u32 eax rsp none 1 13000', 'add_i32 eax 1', 'store_memory_u32 rsp none 1 13000 eax',
            'compare_i32 eax 66', 'branch below Fill_slots');
        Request(1, { Capacity: 1, Length: 1, Alignment: 16 }); Set('r13', 64, 1); Set('r13', 68, 1); Call(6);
        Field(1, 0, 1); Check('rsp', 88, 2048);
        Request(5); Call(); Check('rsp', 88, 0); Check('r12', 40, 1); Field(1, 8, 0); Field(65, 8, 0);
        Request(5); Call(9);
    });
    Case('repeated-credit-and-reuse', 64, ({ Emit, Field, Request, Set, Call, Save, Release, Check }) => {
        Set('rsp', 13000, 32768); Emit('label Repeat_budget');
        Request(6); Set('r13', 64, 1); Set('r13', 68, 1); Set('r13', 72, 48); Call();
        Emit('load_memory_u32 eax r13 none 1 68', 'store_memory_u32 rsp none 1 13004 eax');
        Request(1, { Capacity: 17, Length: 17, Alignment: 16 }); Set('r13', 64, 2);
        Emit('load_memory_u32 eax rsp none 1 13004', 'store_memory_u32 r13 none 1 68 eax'); Call();
        Save(24, 0); for (let Index = 0; Index < 4; Index++) Save(64 + Index * 8, 2 + Index);
        Release(); Field(1, 32, 0);
        Emit('load_memory_u32 eax rsp none 1 13000', 'subtract_i32 eax 1', 'store_memory_u32 rsp none 1 13000 eax', 'branch not_equal Repeat_budget');
        Field(2, 0, 65536); Field(1, 32, 0); Check('rsp', 88, 0); Check('rsp', 56, 48);
    });
    Case('split-query-and-release', 64, ({ Request, Set, Call, Check, Field, Allocate, Oracleˉstate }) => {
        Request(8); Set('r13', 64, 1); Set('r13', 68, 1); Call(); Check('r13', 80, 4096); Oracleˉstate(0);
        Request(6); Set('r13', 64, 1); Set('r13', 68, 1); Set('r13', 72, 48); Set('r13', 88, 64); Call();
        Check('r13', 64, 2); Check('r13', 68, 1); Check('r13', 80, 4096); Oracleˉstate(1);
        Request(8); Set('r13', 64, 1); Set('r13', 68, 1); Call(); Check('r13', 80, 4048);
        Allocate(2, 1); Oracleˉstate(2);
        Request(10, { Handle: 0 }); Call(); Check('r13', 44, 48); Check('r13', 64, 0);
        Request(9, { Handle: 0 }); Call(); Oracleˉstate(3);
        Request(9, { Handle: 0 }); Call(4);
        Request(6); Set('r13', 64, 1); Set('r13', 68, 1); Set('r13', 72, 0); Call();
        Check('r13', 64, 2); Check('r13', 68, 3); Field(1, 20, 1); Field(1, 32, 0);
        Request(7); Set('r13', 64, 2); Set('r13', 68, 3); Call(); Field(1, 20, 0); Field(2, 0, 3);
        Request(7); Set('r13', 64, 1); Set('r13', 68, 1); Call(); Field(1, 8, 0);
        Request(8); Set('r13', 64, 1); Set('r13', 68, 1); Call(4);
    });
    Case('budget-operation-refusals', 64, ({ Request, Set, Call, Entry, Offset, Check }) => {
        for (const Operation of [6, 7, 8, 9, 10]) {
            Request(Operation); Set('r13', 64, 1); Set('r13', 68, 1); Set('r13', 92, 1); Call(1);
        }
        for (const [Operation, Field, Value, Status] of [
            [6, 72, 4097, 2], [6, 88, 65, 1], [6, 68, 2, 4], [6, 64, 66, 4],
            [6, 12, 1, 1], [6, 24, 1, 1], [6, 32, 1, 1], [6, 36, 1, 1], [6, 80, 1, 1],
            [7, 72, 1, 1], [7, 88, 1, 1], [8, 72, 1, 1], [8, 80, 1, 1], [8, 88, 1, 1],
        ]) {
            Request(Operation); Set('r13', 64, 1); Set('r13', 68, 1); Set('r13', Field, Value); Call(Status);
        }
        Entry(1, 1, 0, 4096); Set('rsp', Offset(1) + 16, 0);
        Request(6); Set('r13', 64, 1); Set('r13', 68, 1); Call(2);
        Set('rsp', Offset(1) + 16, 64); Set('rsp', Offset(2), 4294967295);
        Request(6); Set('r13', 64, 1); Set('r13', 68, 1); Call(); Check('r13', 64, 3);
        Request(7); Set('r13', 64, 3); Set('r13', 68, 1); Call();
        for (let Identity = 3; Identity <= 65; Identity++) Set('rsp', Offset(Identity), 4294967295);
        Request(6); Set('r13', 64, 1); Set('r13', 68, 1); Call(6);
    });
    Case('budget-drop-with-live-descendants', 64, ({ Request, Set, Call, Oracleˉstate, Allocate }) => {
        for (const [Parent, Maximum] of [[1, 96], [2, 48]]) {
            Request(6); Set('r13', 64, Parent); Set('r13', 68, 1); Set('r13', 72, Maximum); Set('r13', 88, 64); Call();
        }
        Oracleˉstate(4);
        for (const Identity of [1, 2]) { Request(7); Set('r13', 64, Identity); Set('r13', 68, 1); Call(); }
        Oracleˉstate(9);
        Allocate(3, 1);
        for (const Operation of [9, 10]) {
            for (const Field of [64, 72, 80, 88]) { Request(Operation, { Handle: 0 }); Set('r13', Field, 1); Call(1); }
        }
        Request(9, { Handle: 0 }); Call(); Oracleˉstate(10);
    });
    Case('replacement-copy-and-credit', 112, ({ Emit, Entry, Allocate, Replace, Call, Load, Save, Set, Check, Field, Request, Oracleˉstate }) => {
        Entry(1, 1, 0, 4096, 1, 48); Entry(2, 1, 1, 48);
        Allocate(2, 1); Load(1, 'rdi'); Set('rdi', 0, 123456); Set('rdi', 8, 765432);
        Emit('move_u32 eax 91', 'store_memory_u8 rdi none 1 16 al'); Replace(); Call(); Oracleˉstate(12);
        Save(24, 8); Save(48, 9); Check('r13', 64, 1); Check('r13', 68, 1);
        Check('r13', 44, 64); Check('rsp', 56, 112); Check('rsp', 88, 64);
        Load(9, 'rdi'); Check('rdi', 0, 123456); Check('rdi', 8, 765432);
        Check('rdi', 16, 91); Check('rdi', 20, 0); Check('rdi', 28, 0);
        Field(1, 20, 1); Field(1, 32, 64); Field(2, 8, 0); Field(3, 0, 2);
        Request(10, { Handle: 0 }); Call(4);
        Request(9, { Handle: 8 }); Call(); Oracleˉstate(13); Check('rsp', 88, 0);
    });
    Case('replacement-refusals', 112, ({ Entry, Allocate, Replace, Set, Call, Field, Offset, Request }) => {
        Entry(1, 1, 0, 4096, 1, 48); Entry(2, 1, 1, 48); Allocate(2, 1);
        for (const [Index, Value, Status] of [
            [12, 17, 1], [12, 0, 1], [12, 4194305, 8], [24, 65, 4],
            [28, 2, 4], [64, 2, 4], [68, 3, 4], [32, 3, 1], [32, 32, 8],
            [36, 34, 7], [36, 16, 1], [72, 1, 1], [80, 1, 1], [88, 18, 1], [92, 1, 1],
        ]) { Replace(); Set('r13', Index, Value); Call(Status); }
        Set('rsp', Offset(1) + 24, 111); Replace(); Call(2);
        Set('rsp', Offset(1) + 24, 4096); Set('rsp', Offset(1) + 16, 1); Replace(); Call(2);
        Set('rsp', Offset(1) + 16, 64);
        for (let Identity = 3; Identity <= 65; Identity++) Set('rsp', Offset(Identity), 4294967294);
        Replace(); Call(6); Field(2, 0, 2);
        Request(9, { Handle: 0 }); Call();
    });
    Case('replacement-physical-refusal', 96, ({ Entry, Allocate, Replace, Call, Request, Check }) => {
        Entry(1, 1, 0, 4096, 1, 48); Entry(2, 1, 1, 48); Allocate(2, 1);
        Replace(); Call(3); Check('rsp', 56, 48);
        Request(9, { Handle: 0 }); Call(); Check('rsp', 88, 0);
    });
    Case('replacement-fragmentation', 192, ({ Entry, Allocate, Replace, Release, Call, Request }) => {
        Entry(1, 1, 0, 4096, 4, 192);
        for (let Identity = 2; Identity <= 5; Identity++) Entry(Identity, 1, 1, 48);
        for (let Identity = 2; Identity <= 5; Identity++) {
            Allocate(Identity, 1, 17, (Identity - 2) * 8);
        }
        Release(8); Release(24); Replace(); Call(3); Request(5); Call();
    });
    Case('replacement-generation-selection', 112, ({ Entry, Allocate, Replace, Offset, Set, Call, Field, Request }) => {
        Entry(1, 1, 0, 4096, 1, 48); Entry(2, 1, 1, 48); Allocate(2, 1);
        Set('rsp', Offset(3), 4294967293); Set('rsp', Offset(4), 3);
        Replace(); Call(); Field(3, 0, -3); Field(4, 0, 6); Field(4, 8, 1);
        Request(5); Call();
    });
    Case('replacement-repeated-reuse', 112, ({ Emit, Request, Set, Call, Save, Replace, Field, Check }) => {
        Set('rsp', 13000, 1000); Emit('label Repeat_replacement');
        Request(6); Set('r13', 64, 1); Set('r13', 68, 1); Set('r13', 72, 48); Call();
        Save(64, 2);
        Request(1, { Capacity: 17, Length: 17, Alignment: 16 });
        Emit('load_memory_u64 rax rsp none 1 12816', 'store_memory_u64 r13 none 1 64 rax'); Call(); Save(24, 0);
        Replace(); Call(); Save(24, 8); Request(9, { Handle: 8 }); Call();
        Field(1, 20, 0); Field(1, 32, 0); Check('rsp', 88, 0);
        Emit('load_memory_u32 eax rsp none 1 13000', 'subtract_i32 eax 1',
            'store_memory_u32 rsp none 1 13000 eax', 'branch not_equal Repeat_replacement');
        Field(2, 0, 2000); Field(3, 0, 2000); Check('rsp', 56, 112);
    });
    return Cases;
}
