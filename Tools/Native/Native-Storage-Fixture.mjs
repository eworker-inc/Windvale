export function Buildˉstorageˉfixture(Name, Capacity, Body, Budgeted = false, Oracle = []) {
    const Oracleˉused = new globalThis.Set();
    const Lines = ['windvale-assembly 1',
        'symbol local function Request_reset in .text',
        'symbol local function Snapshot_compare in .text',
        'symbol local function Snapshot_save in .text',
        'symbol export function Main in .text',
        ...(Budgeted ? ['symbol import function Windvale_budgeted_storage'] : []),
        'symbol import function Windvale_owned_storage',
        'section code .text align 16', 'define Main'];
    const Emit = (...Text) => Lines.push(...Text);
    for (const Register of ['rbx', 'rbp', 'rsi', 'rdi', 'r12', 'r13', 'r14', 'r15']) Emit(`push ${Register}`);
    // Touch each stack page before descending across Windows guard pages.
    Emit('move_u32 ebp 1', 'move_u32 ebx 5151', 'move_u32 esi 6161', 'move_u32 r15d 7171', 'xor eax eax');
    const Frame = Budgeted ? 32768 : 16384;
    for (let Page = 0; Page < Frame / 4096; Page++) Emit('subtract_i32 rsp 4096', 'store_memory_u64 rsp none 1 0 rax');
    Emit('subtract_i32 rsp 8', 'move_u32 ecx 32', 'label Frame_zero',
        'store_memory_u64 rsp rcx 1 0 rax', 'add_i32 ecx 8', `compare_i32 ecx ${Frame}`,
        'branch below Frame_zero', 'move r12 rsp', 'add_i32 r12 32',
        'move r13 rsp', 'add_i32 r13 2144', 'move r14 rsp', `add_i32 r14 ${Budgeted ? 2240 : 2208}`);
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
        if (Budgeted) Emit('move r8 rsp', 'add_i32 r8 40', 'move r9 rsp', 'add_i32 r9 17512');
        const Spans = Budgeted
            ? [['r12', 1088, 22000], ['r8', 2112, 23088], ['r14', 4096, 25200], ['r9', 2616, 29296], ['r13', 96, 31912]]
            : [['r12', 2112, 6400], ['r14', 4096, 8512], ['r13', 64, 12608]];
        for (const [Base, Bytes, Saved] of Spans) {
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
    function Call(Status = 0, Atomic = Status !== 0, Physical = false) {
        if (Atomic) Snapshot(false);
        Emit('store_memory_u64 rsp none 1 13100 rdi', 'store_memory_u64 rsp none 1 13108 rbp');
        Emit('move r8 r12', 'move r9 r13', 'move_u32 r10d 12345', 'move_u32 r11d 67890',
            `call Windvale_${Budgeted && !Physical ? 'budgeted' : 'owned'}_storage`, `compare_i32 eax ${Status}`, 'branch not_equal Failed',
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
    function Oracleˉstate(Index) {
        if (!Budgeted || Oracle[Index]?.length !== 2616) throw new Error('Missing accounting oracle checkpoint.');
        Oracleˉused.add(Index);
        const Label = `Oracle_${Index}`;
        Emit(`load_address r8 Expected_${Index}`, 'xor ecx ecx', `label ${Label}`,
            'load_memory_u32 eax r8 rcx 1 0', 'load_memory_u32 edx rsp rcx 1 17504',
            'compare eax edx', 'branch not_equal Failed', 'add_i32 ecx 4',
            'compare_i32 ecx 2616', `branch below ${Label}`);
    }
    Set('r12', 4, 1); Set('r12', 8, 2112); Set('r12', 12, 64); Set('r12', 16, 71);
    Emit('store_memory_u64 r12 none 1 32 r14'); Set('r12', 40, Capacity);
    Request(0); Set('r13', 4, 64); Call(0, false, true);
    if (Budgeted) {
        Emit('move rax r12', 'move r12 rsp', 'add_i32 r12 16416', 'store_memory_u64 r12 none 1 16 rax',
            'move rax rsp', 'add_i32 rax 17504', 'store_memory_u64 r12 none 1 24 rax');
        Set('r12', 4, 1); Set('r12', 8, 1088); Set('r12', 32, 71);
        Set('rsp', 17504, 1112364631); Set('rsp', 17508, 1); Set('rsp', 17512, 65); Set('rsp', 17516, 1);
        Set('rsp', 17520, 1); Set('rsp', 17528, 1); Set('rsp', 17532, 1); Set('rsp', 17536, 64); Set('rsp', 17544, 4096);
        Request(0); Call();
    }
    Body({ Emit, Set, Check, Equal, Save, Load, Request, Call, Reserve, Release, Snapshot, Oracleˉstate });
    Emit('move_u32 eax 42', 'jump_label Finished', 'label Failed', 'move eax ebp', 'label Finished', `add_i32 rsp ${Frame + 8}`);
    for (const Register of ['r15', 'r14', 'r13', 'r12', 'rdi', 'rsi', 'rbp', 'rbx']) Emit(`pop ${Register}`);
    Emit('return', 'end define', 'define Request_reset', 'xor eax eax');
    for (let Offset = 0; Offset < (Budgeted ? 96 : 64); Offset += 8) Emit(`store_memory_u64 r13 none 1 ${Offset} rax`);
    Set('r13', 0, 1); Set('r13', 4, Budgeted ? 96 : 64); Set('r13', 16, 71);
    Emit('return', 'end define');
    Snapshotˉbody(false); Snapshotˉbody(true);
    Emit('end section');
    if (Oracleˉused.size > 0) {
        const Indices = [...Oracleˉused].sort((Left, Right) => `Expected_${Left}` < `Expected_${Right}` ? -1 : 1);
        Lines.splice(1, 0, ...Indices.map(Index => `symbol local data Expected_${Index} in .rodata`));
        Emit('section rodata .rodata align 16');
        for (const Index of Indices) {
            Emit(`define Expected_${Index}`);
            for (let Offset = 0; Offset < 2616; Offset += 32) Emit(`bytes ${[...Oracle[Index].subarray(Offset, Offset + 32)].join(' ')}`);
            Emit('end define');
        }
        Emit('end section');
    }
    Emit('');
    return { Name, Source: Lines.join('\n') };
}
