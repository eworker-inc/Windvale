import { readFile, writeFile } from 'node:fs/promises';
import { join } from 'node:path';

// Extends the existing native lowering owner; no compiler reconstruction is
// needed for this runtime-private assembly leaf's focused selection.
export async function Runˉownedˉstorageˉcases(Context) {
    const { Repository, Work, Target, Requireˉsuccess, Runˉprocess } = Context;
    const Extension = Target === 'windows' ? 'cmd' : 'sh';
    const Tool = Name => join(Repository, 'Tools', 'Native', `${Name}.${Extension}`);
    const Objects = [];
    for (const [Name, Source] of [
        ['Owned', 'Runtime/Native/X64-Owned-Storage.wva'],
        ['Allocator', 'Compiler/Native/Allocator/Descriptor-Allocator.wva'],
    ]) {
        process.stdout.write(`native owned storage step=assemble leaf=${Name}\n`);
        const Object = join(Work, `${Name}.wvo`);
        const Repeat = join(Work, `${Name}-repeat.wvo`);
        await Requireˉsuccess(Tool('Assemble-Wva'), [join(Repository, Source), Object], `owned-${Name}-assemble`);
        await Requireˉsuccess(Tool('Assemble-Wva'), [join(Repository, Source), Repeat], `owned-${Name}-repeat`);
        if (!(await readFile(Object)).equals(await readFile(Repeat))) {
            throw new Error(`Owned storage ${Name} assembly is not deterministic.`);
        }
        await Requireˉsuccess(Tool('Check-Wvo'), [Object], `owned-${Name}-validate`);
        Objects.push(Object);
    }
    const Cases = Buildˉcases();
    for (const [Index, Case] of Cases.entries()) {
        process.stdout.write(`native owned storage step=execute item=${Index + 1}/${Cases.length} case=${Case.Name}\n`);
        const Prefix = join(Work, `Owned-${Case.Name}`);
        if (Buffer.byteLength(Case.Source) > 131_072) throw new Error('Owned storage fixture exceeds its source limit.');
        const Source = Prefix + '.wva', Object = Prefix + '.wvo', Image = Prefix + '.bin';
        const Application = Prefix + (Target === 'windows' ? '.exe' : '.elf');
        await writeFile(Source, Case.Source);
        await Requireˉsuccess(Tool('Assemble-Wva'), [Source, Object], `owned-${Case.Name}-assemble`);
        const Linked = await Requireˉsuccess(Tool('Link-Wvo'), ['0', 'Main', Image, Object, ...Objects], `owned-${Case.Name}-link`);
        const Entry = /^entry name=Main address=([0-9]+)$/mu.exec(Linked.Output);
        if (Entry === null) throw new Error('Owned storage test entry point is missing.');
        await Requireˉsuccess(Tool('Package-Console'), [`${Target}-x64-console-v1`, Image, Entry[1], Application], `owned-${Case.Name}-package`);
        const Start = performance.now();
        const Result = await Runˉprocess(Application, [], 30_000, `owned-${Case.Name}-execute`);
        if (Result.Code !== 42 || Result.Exceeded || Result.Timedˉout || Result.Output !== '') {
            throw new Error(`Owned storage ${Case.Name} failed: code=${Result.Code}, output=${Result.Output}.`);
        }
        process.stdout.write(`native owned storage case=${Case.Name} status=Passed elapsed-ms=${Math.round(performance.now() - Start)}\n`);
    }
    process.stdout.write(`native owned storage status=Passed cases=${Cases.length} slots=64 state-bytes=2112 stress-iterations=32768 stress-arena=64 stress-peak-charge=48\n`);
    return Cases.length;
}

function Buildˉcases() {
    const Cases = [];
    function Case(Name, Capacity, Body) {
        const Lines = ['windvale-assembly 1',
            'symbol local function Request_reset in .text',
            'symbol local function Snapshot_compare in .text',
            'symbol local function Snapshot_save in .text',
            'symbol export function Main in .text',
            'symbol import function Windvale_owned_storage',
            'section code .text align 16', 'define Main'];
        const Emit = (...Text) => Lines.push(...Text);
        for (const Register of ['rbx', 'rbp', 'rsi', 'rdi', 'r12', 'r13', 'r14', 'r15']) Emit(`push ${Register}`);
        // Touch each stack page before descending across Windows guard pages.
        Emit('move_u32 ebp 1', 'move_u32 ebx 5151', 'move_u32 esi 6161', 'move_u32 r15d 7171', 'xor eax eax');
        for (let Page = 0; Page < 4; Page++) Emit('subtract_i32 rsp 4096', 'store_memory_u64 rsp none 1 0 rax');
        Emit('subtract_i32 rsp 8', 'move_u32 ecx 32', 'label Frame_zero',
            'store_memory_u64 rsp rcx 1 0 rax', 'add_i32 ecx 8', 'compare_i32 ecx 16384',
            'branch below Frame_zero', 'move r12 rsp', 'add_i32 r12 32',
            'move r13 rsp', 'add_i32 r13 2144', 'move r14 rsp', 'add_i32 r14 2208');
        let Checkˉindex = 0;
        function Set(Base, Offset, Value) {
            Emit(`move_u32 eax ${Value}`, `store_memory_u32 ${Base} none 1 ${Offset} eax`);
        }
        function Check(Base, Offset, Value) {
            Emit(`move_u32 ebp ${1 + (++Checkˉindex % 40)}`, `load_memory_u32 eax ${Base} none 1 ${Offset}`,
                `compare_i32 eax ${Value}`, 'branch not_equal Failed');
        }
        function Equal(Left, Right) { Emit(`compare ${Left} ${Right}`, 'branch not_equal Failed'); }
        function Save(Requestˉoffset, Slot) {
            Emit(`load_memory_u64 rax r13 none 1 ${Requestˉoffset}`, `store_memory_u64 rsp none 1 ${12800 + Slot * 8} rax`);
        }
        function Load(Slot, Register = 'rax') { Emit(`load_memory_u64 ${Register} rsp none 1 ${12800 + Slot * 8}`); }
        function Request(Operation, { Capacity: Bytes = 0, Length = 0, Alignment = 0, Budget = 0, Handle = null } = {}) {
            Emit('call Request_reset');
            for (const [Offset, Value] of [[8, Operation], [12, Bytes], [32, Alignment], [36, Length], [56, Budget]]) {
                if (Value !== 0) Set('r13', Offset, Value);
            }
            if (Handle !== null) { Load(Handle); Emit('store_memory_u64 r13 none 1 24 rax'); }
        }
        function Snapshot(Compare) {
            Emit(`call Snapshot_${Compare ? 'compare' : 'save'}`, 'test eax eax', 'branch not_equal Failed');
        }
        function Snapshotˉbody(Compare) {
            const Label = `Snapshot_${Compare ? 'compare' : 'save'}`;
            Emit(`define ${Label}`);
            // State and the whole backing extent; rejected requests must change neither.
            for (const [Base, Bytes, Saved] of [['r12', 2112, 6400], ['r14', 4096, 8512], ['r13', 64, 12608]]) {
                Emit('xor ecx ecx', `label ${Label}_${Base}`);
                if (Compare && Base === 'r13') Emit('compare_i32 ecx 40', `branch equal ${Label}_skip_status`);
                Emit(`load_memory_u32 eax ${Base} rcx 1 0`);
                if (Compare) Emit(`load_memory_u32 edx rsp rcx 1 ${Saved + 8}`, 'compare eax edx', 'branch not_equal Mismatch');
                else Emit(`store_memory_u32 rsp rcx 1 ${Saved + 8} eax`);
                if (Compare && Base === 'r13') Emit(`label ${Label}_skip_status`);
                Emit('add_i32 ecx 4', `compare_i32 ecx ${Bytes}`, `branch below ${Label}_${Base}`);
            }
            Emit('xor eax eax', 'return');
            if (Compare) Emit('label Mismatch', 'move_u32 eax 1', 'return');
            Emit('end define');
        }
        function Call(Status = 0, Atomic = Status !== 0) {
            if (Atomic) Snapshot(false);
            Emit('store_memory_u64 rsp none 1 13100 rdi', 'store_memory_u64 rsp none 1 13108 rbp');
            Emit('move r8 r12', 'move r9 r13', 'move_u32 r10d 12345', 'move_u32 r11d 67890',
                'call Windvale_owned_storage', `compare_i32 eax ${Status}`, 'branch not_equal Failed',
                'compare_i32 r10d 12345', 'branch not_equal Failed', 'compare_i32 r11d 67890', 'branch not_equal Failed',
                'compare_i32 ebx 5151', 'branch not_equal Failed', 'compare_i32 esi 6161', 'branch not_equal Failed',
                'compare_i32 r15d 7171', 'branch not_equal Failed',
                'load_memory_u64 rax rsp none 1 13100', 'compare rax rdi', 'branch not_equal Failed',
                'load_memory_u64 rax rsp none 1 13108', 'compare rax rbp', 'branch not_equal Failed');
            Check('r13', 40, Status);
            if (Atomic) Snapshot(true);
        }
        function Reserve(Bytes = 32, Handle = 0, Pointer = 1) {
            Request(1, { Capacity: Bytes, Length: Bytes, Alignment: 16, Budget: (Bytes + 31) & ~15 }); Call();
            Save(24, Handle); Save(48, Pointer);
        }
        function Release(Handle = 0) { Request(3, { Handle }); Call(); }
        Set('r12', 4, 1); Set('r12', 8, 2112); Set('r12', 12, 64); Set('r12', 16, 71);
        Emit('store_memory_u64 r12 none 1 32 r14'); Set('r12', 40, Capacity);
        Request(0); Call();
        Body({ Emit, Set, Check, Equal, Save, Load, Request, Call, Reserve, Release, Snapshot });
        Emit('move_u32 eax 42', 'jump_label Finished', 'label Failed', 'move eax ebp', 'label Finished', 'add_i32 rsp 16392');
        for (const Register of ['r15', 'r14', 'r13', 'r12', 'rdi', 'rsi', 'rbp', 'rbx']) Emit(`pop ${Register}`);
        Emit('return', 'end define', 'define Request_reset', 'xor eax eax');
        for (let Offset = 0; Offset < 64; Offset += 8) Emit(`store_memory_u64 r13 none 1 ${Offset} rax`);
        Set('r13', 0, 1); Set('r13', 4, 64); Set('r13', 16, 71);
        Emit('return', 'end define');
        Snapshotˉbody(false); Snapshotˉbody(true);
        Emit('end section', '');
        Cases.push({ Name, Source: Lines.join('\n') });
    }

    Case('resize-and-zero', 144, ({ Emit, Check, Load, Request, Call, Reserve, Release }) => {
        Reserve(33); Check('r13', 44, 64); Check('r12', 56, 64);
        Load(1, 'rdi'); Check('rdi', 0, 0); Check('rdi', 28, 0);
        Emit('move_u32 eax 97', 'store_memory_u8 rdi none 1 0 al', 'store_memory_u8 rdi none 1 2 al', 'store_memory_u8 rdi none 1 32 al');
        Request(2, { Handle: 0, Length: 2 }); Call(); Check('r12', 76, 2); Check('r12', 56, 64);
        Request(2, { Handle: 0, Length: 33 }); Call(); Check('rdi', 0, 97); Check('rdi', 28, 0);
        Emit('xor eax eax', 'load_memory_u8 al rdi none 1 32', 'test eax eax', 'branch not_equal Failed');
        Request(2, { Handle: 0, Length: 34 }); Call(7);
        Request(4, { Handle: 0 }); Call(); Release();
        Check('r12', 52, 0); Check('r12', 56, 0); Check('r12', 24, 64);
        Request(4, { Handle: 0 }); Call(4);
        Reserve(33, 2, 3); Load(1, 'rcx'); Load(3); Emit('compare rax rcx', 'branch not_equal Failed');
        Load(0, 'rcx'); Load(2); Emit('compare rax rcx', 'branch equal Failed');
        Load(3, 'rdi'); Check('rdi', 0, 0); Check('rdi', 28, 0);
        Request(3, { Handle: 0 }); Call(4); Release(2); Request(3, { Handle: 2 }); Call(4);
    });
    Case('non-tail-reuse-and-teardown', 144, ({ Emit, Check, Load, Request, Call, Reserve, Release }) => {
        Reserve(32, 0, 1); Reserve(32, 2, 3); Reserve(32, 4, 5);
        Load(3, 'rdi'); Emit('move_u32 eax 99', 'store_memory_u32 rdi none 1 0 eax');
        Release(0); Reserve(32, 6, 7); Load(1, 'rcx'); Load(7); Emit('compare rax rcx', 'branch not_equal Failed');
        Load(3, 'rdi'); Check('rdi', 0, 99); Check('r12', 56, 144);
        Request(5); Call(); Check('r12', 52, 0); Check('r12', 56, 0); Check('r12', 28, 1);
        Request(4, { Handle: 2 }); Call(9); Request(5); Call(9);
        Request(1, { Capacity: 32, Alignment: 16, Budget: 48 }); Call(9);
    });
    for (const Order of [[0, 2], [2, 0]]) Case(`coalesce-${Order.join('-')}`, 144, ({ Emit, Check, Load, Reserve, Release }) => {
        Reserve(32, 0, 1); Reserve(32, 2, 3); Reserve(32, 4, 5);
        Release(Order[0]); Release(Order[1]); Reserve(80, 6, 7);
        Load(1, 'rcx'); Load(7); Emit('compare rax rcx', 'branch not_equal Failed');
        Check('r12', 56, 144); Release(4); Release(6); Reserve(128); Check('r13', 44, 144);
    });
    Case('fragmentation', 192, ({ Check, Request, Call, Reserve, Release }) => {
        for (let Index = 0; Index < 4; Index++) Reserve(32, Index * 2, Index * 2 + 1);
        Release(0); Release(4); Check('r12', 56, 96);
        Request(1, { Capacity: 80, Alignment: 16, Budget: 96 }); Call(3);
        Release(2); Reserve(128); Check('r13', 44, 144);
    });
    Case('request-refusals', 64, ({ Emit, Set, Check, Request, Call, Reserve, Snapshot }) => {
        for (const [Offset, Value] of [[0, 2], [4, 63], [8, 6], [60, 1], [40, 1], [44, 1], [48, 1]]) {
            Request(1, { Capacity: 1, Alignment: 1, Budget: 32 }); Set('r13', Offset, Value); Call(1);
        }
        Request(0); Call(1);
        for (const [Options, Status] of [
            [{ Capacity: 0, Alignment: 16, Budget: 32 }, 1],
            [{ Capacity: 4194305, Alignment: 16, Budget: 4194336 }, 8],
            [{ Capacity: 4194304, Alignment: 16, Budget: 4194320 }, 3],
            [{ Capacity: 32, Alignment: 0, Budget: 48 }, 1],
            [{ Capacity: 32, Alignment: 3, Budget: 48 }, 1],
            [{ Capacity: 32, Alignment: 8192, Budget: 48 }, 1],
            [{ Capacity: 32, Alignment: 32, Budget: 48 }, 8],
            [{ Capacity: 32, Alignment: 16, Budget: 47 }, 2],
            [{ Capacity: 32, Alignment: 16, Budget: 48, Length: 33 }, 7],
        ]) { Request(1, Options); Call(Status); }
        Request(1, { Capacity: 1, Alignment: 1, Budget: 32 }); Set('r13', 24, 1); Call(1);
        Reserve(32); Request(1, { Capacity: 1, Alignment: 1, Budget: 32 }); Call(3);
        Request(4, { Handle: 0 }); Set('r13', 16, 72); Call(4);
        Request(4); Call(4); Request(4); Set('r13', 24, 65); Call(4);
        for (const Pointerˉsetup of [
            ['xor r8d r8d'], ['add_i32 r8 1'], ['add_i32 r9 1'],
            ['move r9 r12', 'add_i32 r9 80'], ['move r9 r14'],
        ]) {
            Request(4, { Handle: 0 }); Snapshot(false);
            Emit('move r8 r12', 'move r9 r13', ...Pointerˉsetup, 'call Windvale_owned_storage',
                'compare_i32 eax 1', 'branch not_equal Failed');
            Snapshot(true); Check('r13', 40, 0);
        }
    });
    Case('slots-and-teardown', 4096, ({ Emit, Check, Request, Call, Reserve }) => {
        Emit('move_u32 eax 64', 'store_memory_u32 rsp none 1 13000 eax', 'label Fill_slots');
        Reserve(1);
        Emit('load_memory_u32 eax rsp none 1 13000', 'subtract_i32 eax 1',
            'store_memory_u32 rsp none 1 13000 eax', 'branch not_equal Fill_slots');
        Check('r12', 52, 64); Check('r12', 56, 2048);
        Request(1, { Capacity: 1, Alignment: 1, Budget: 32 }); Call(6);
        Request(5); Call(); Check('r12', 56, 0); Check('r14', 0, 4096);
    });
    Case('generation-retirement', 64, ({ Set, Check, Request, Call, Reserve, Release }) => {
        Set('r12', 64, 4294967294); Reserve(1); Check('r13', 28, -1); Release();
        Reserve(1, 2, 3); Check('r13', 24, 2); Check('r13', 28, 1);
        Request(4, { Handle: 0 }); Call(4); Release(2);
        for (let Index = 1; Index < 64; Index++) Set('r12', 64 + Index * 32, 4294967295);
        Request(1, { Capacity: 1, Alignment: 1, Budget: 32 }); Call(6);
    });
    Case('corrupt-state', 144, ({ Set, Request, Call, Reserve, Release }) => {
        Reserve(32); Reserve(32, 2, 3); Release(0);
        // Two free blocks surround the remaining live one. Corruption of even
        // the later free node must be found before first-fit can mutate anything.
        for (const [Base, Offset, Value, Original] of [
            ['r12', 52, 2, 1], ['r12', 56, 1, 48], ['r12', 24, 0, 96],
            ['r12', 96, 0, 1], ['r12', 120, 1, 0], ['r12', 104, 0, 32],
            ['r14', 52, 2, 1], ['r14', 108, 0, 1380341335],
            ['r14', 104, 1, 0], ['r14', 8, 49, 97], ['r14', 96, 32, 48],
        ]) {
            Set(Base, Offset, Value); Request(1, { Capacity: 1, Alignment: 16, Budget: 32 }); Call(5); Set(Base, Offset, Original);
        }
        Request(5); Call();
    });
    Case('repeated-reuse', 64, ({ Emit, Check, Request, Call, Reserve, Release }) => {
        Emit('move_u32 eax 32768', 'store_memory_u32 rsp none 1 13000 eax', 'label Repeat');
        Reserve(17); Check('r13', 44, 48); Release();
        Emit('load_memory_u32 eax rsp none 1 13000', 'subtract_i32 eax 1',
            'store_memory_u32 rsp none 1 13000 eax', 'branch not_equal Repeat');
        Check('r12', 64, 32768); Check('r12', 56, 0); Check('r12', 24, 48);
        Request(5); Call();
    });
    return Cases;
}
