// Cases belong to the existing native storage owner; no extra verifier gate.
const REQUEST = 32;
const CONTEXT = 144;
const PHYSICAL = 288;
const ACCOUNTING = 2400;
const ADAPTER = 5024;
const ARENA = 6112;
const END = 6176;
const REGIONS = [[16, CONTEXT, 136], [24, PHYSICAL, 2112],
    [32, ACCOUNTING, 2616], [40, ADAPTER, 1088], [48, ARENA, 64]];
const PRESERVED = ['rbx', 'rbp', 'rsi', 'rdi', 'r12', 'r13', 'r14', 'r15', 'r10', 'r11'];

function Fixture(Name, Body, Substituteˉbudgeted = false, Version = 1, Substituteˉshared = false, Options = {}) {
    const CONTEXT = Version === 2 ? 224 : 144, PHYSICAL = Version === 2 ? 416 : 288;
    const ACCOUNTING = Version === 2 ? 2528 : 2400, ADAPTER = Version === 2 ? 5152 : 5024;
    const SHARED = 6368, ARENA = Version === 2 ? 8544 : 6112, DIRECTORY = 8640, MODULE = 8704, INPUT = 8736;
    const END = Version === 2 ? 9040 : 6176, Frame = Options.Frame ?? (Version === 2 ? 32768 : 16384);
    const Lowerˉrequest = Version === 2 ? 8768 : 6208, Sharedˉrequest = 8880, Savedˉhandle = Version === 2 ? 9024 : 6304;
    const REGIONS = [[16, CONTEXT, Version === 2 ? 192 : 136], [24, PHYSICAL, 2112],
        [32, ACCOUNTING, 2616], [40, ADAPTER, Version === 2 ? 1216 : 1088], [48, ARENA, 64],
        ...(Version === 2 ? [[104, SHARED, 2176], [112, DIRECTORY, 48], [128, MODULE, 16], [144, INPUT, 4]] : [])];
    const Lines = ['windvale-assembly 1',
        'symbol local function Snapshot_compare in .text', 'symbol local function Snapshot_save in .text',
        'symbol export function Main in .text',
        ...(Substituteˉshared ? ['symbol export function Windvale_shared_storage in .text'] : []),
        `symbol ${Substituteˉbudgeted ? 'export function Windvale_budgeted_storage in .text' : 'import function Windvale_budgeted_storage'}`,
        'symbol import function Windvale_owned_domain_initialize',
        ...(Version === 2 && !Substituteˉshared ? ['symbol import function Windvale_shared_storage'] : []),
        ...(Options.Symbols ?? []),
        'section code .text align 16', 'define Main'];
    if (Options.Symbols?.length) {
        const Positions = Lines.flatMap((Line, Index) => Line.startsWith('symbol ') ? [Index] : []);
        const Bindings = { local: 0, export: 1, import: 2 };
        const Sorted = Positions.map(Index => Lines[Index]).sort((Left, Right) => {
            const A = Left.split(' '), B = Right.split(' ');
            return Bindings[A[1]] - Bindings[B[1]] || (A[3] < B[3] ? -1 : A[3] > B[3] ? 1 : 0);
        });
        for (let Index = 0; Index < Positions.length; Index++) Lines[Positions[Index]] = Sorted[Index];
    }
    const Emit = (...Value) => Lines.push(...Value);
    for (const Register of PRESERVED.slice(0, 8)) Emit(`push ${Register}`);
    Emit('xor eax eax');
    for (let Page = 0; Page < Frame / 4096; Page++) Emit('subtract_i32 rsp 4096', 'store_memory_u64 rsp none 1 0 rax');
    Emit('subtract_i32 rsp 8', 'move_u32 ecx 32', 'label Clear',
        'store_memory_u64 rsp rcx 1 0 rax', 'add_i32 ecx 8', `compare_i32 ecx ${Frame}`, 'branch below Clear');
    function Set(Offset, Value) {
        Emit(`move_u32 eax ${Value}`, `store_memory_u32 rsp none 1 ${Offset} eax`);
    }
    function Pointer(Field, Offset) {
        Emit('move rax rsp', `add_i32 rax ${Offset}`, `store_memory_u64 rsp none 1 ${REQUEST + Field} rax`);
    }
    function Check(Offset, Value) {
        Emit(`load_memory_u32 eax rsp none 1 ${Offset}`, `compare_i32 eax ${Value}`, 'branch not_equal Failed');
    }
    function Initialize(Status = 0, Atomic = Status !== 0, Address = REQUEST) {
        if (Atomic) Emit('call Snapshot_save');
        if (Address === null) Emit('xor r8d r8d');
        else if (Address === 'wrap') Emit('xor r8d r8d', 'subtract_i32 r8 16');
        else Emit('move r8 rsp', `add_i32 r8 ${Address}`);
        for (const [Index, Register] of PRESERVED.entries()) Emit(`move_u32 ${Register === 'rbp' ? 'ebp' : Register === 'rbx' ? 'ebx' : Register === 'rsi' ? 'esi' : Register === 'rdi' ? 'edi' : Register + 'd'} ${10000 + Index}`);
        Emit('call Windvale_owned_domain_initialize', `compare_i32 eax ${Status}`, 'branch not_equal Failed');
        for (const [Index, Register] of PRESERVED.entries()) Emit(`compare_i32 ${Register === 'rbp' ? 'ebp' : Register === 'rbx' ? 'ebx' : Register === 'rsi' ? 'esi' : Register === 'rdi' ? 'edi' : Register + 'd'} ${10000 + Index}`, 'branch not_equal Failed');
        if (Atomic) Emit('call Snapshot_compare', 'test eax eax', 'branch not_equal Failed');
    }
    let Operationˉindex = 0;
    function Operation(Selector, Identity = 1, Generation = 1) {
        const Label = 'Clear_operation_' + Operationˉindex++;
        Emit('xor eax eax', `move_u32 ecx ${Lowerˉrequest}`, 'label ' + Label,
            'store_memory_u64 rsp rcx 1 0 rax', 'add_i32 ecx 8',
            `compare_i32 ecx ${Lowerˉrequest + (Version === 2 ? 112 : 96)}`, 'branch below ' + Label);
        Set(Lowerˉrequest, Version); Set(Lowerˉrequest + 4, Version === 2 ? 112 : 96);
        Set(Lowerˉrequest + 8, Selector); Set(Lowerˉrequest + 16, 71);
        if (Selector === 8) { Set(Lowerˉrequest + 64, Identity); Set(Lowerˉrequest + 68, Generation); }
        if (Selector === 6) { Set(Lowerˉrequest + 64, 1); Set(Lowerˉrequest + 68, 1);
            Set(Lowerˉrequest + 72, 48); Set(Lowerˉrequest + 88, 0); }
        if (Selector === 1) {
            Set(Lowerˉrequest + 12, 17); Set(Lowerˉrequest + 32, 16); Set(Lowerˉrequest + 36, 17);
            Set(Lowerˉrequest + 64, 2); Set(Lowerˉrequest + 68, 1);
        }
        if (Selector === 9) Emit(`load_memory_u64 rax rsp none 1 ${Savedˉhandle}`,
            `store_memory_u64 rsp none 1 ${Lowerˉrequest + 24} rax`);
        Emit('move r8 rsp', `add_i32 r8 ${ADAPTER}`, 'move r9 rsp', `add_i32 r9 ${Lowerˉrequest}`,
            'call Windvale_budgeted_storage', 'test eax eax', 'branch not_equal Failed');
    }
    function Shared(Operation, Contextˉhandle = 160, Start = 0, Length = 0) {
        const Label = 'Clear_shared_' + Operationˉindex++;
        Emit('xor eax eax', `move_u32 ecx ${Sharedˉrequest}`, `label ${Label}`,
            'store_memory_u64 rsp rcx 1 0 rax', 'add_i32 ecx 8', `compare_i32 ecx ${Sharedˉrequest + 128}`,
            `branch below ${Label}`);
        for (const [Field, Value] of [[0, 2], [4, 128], [8, Operation], [16, 71], [40, Start], [48, Length]]) {
            if (Value !== 0) Set(Sharedˉrequest + Field, Value);
        }
        if (Operation !== 11 && Operation !== 12) Emit(`load_memory_u64 rax rsp none 1 ${CONTEXT + Contextˉhandle}`,
            `store_memory_u64 rsp none 1 ${Sharedˉrequest + 24} rax`);
        Emit('move r8 rsp', `add_i32 r8 ${SHARED}`, 'move r9 rsp', `add_i32 r9 ${Sharedˉrequest}`,
            'call Windvale_shared_storage', 'test eax eax', 'branch not_equal Failed');
    }
    for (const [Offset, Value] of [[0, Version], [4, Version === 2 ? 192 : 112], [56, 64], [64, 71],
        [72, Version === 2 ? 112 : 64], [80, 64], [88, 1000000], [96, Version === 2 ? 64 : 1024],
        ...(Version === 2 ? [[120, 48], [136, 16], [152, 4], [160, 44], [168, 48]] : [])]) Set(REQUEST + Offset, Value);
    for (const [Field, Offset] of REGIONS) Pointer(Field, Offset);
    // Nonzero arena contents are permitted; reserve zeroes exposed backing.
    Set(ARENA, 0x12345678); Set(ARENA + 12, 0x23456789);
    if (Version === 2) {
        for (const [Offset, Length, Kind] of [[0, 4, 3], [4, 8, 4], [12, 4, 5]]) {
            const Row = DIRECTORY + (Kind === 3 ? 0 : Kind === 4 ? 16 : 32);
            Set(Row, Offset); Set(Row + 4, Length); Set(Row + 8, Length); Set(Row + 12, Kind);
        }
        Set(MODULE, 0x64636261); Set(MODULE + 4, 0x44332211); Set(MODULE + 8, 0x88776655);
        Set(MODULE + 12, 0xccbbaa99); Set(INPUT, 0x09080706);
    }
    Body({ Emit, Set, Pointer, Check, Initialize, Operation, Shared, REGIONS, REQUEST, CONTEXT, PHYSICAL,
        ACCOUNTING, ADAPTER, SHARED, ARENA, DIRECTORY, MODULE, INPUT, Lowerˉrequest, Sharedˉrequest });
    Emit('move_u32 eax 42', 'jump_label Finish', 'label Failed', 'move_u32 eax 1',
        'label Finish', `add_i32 rsp ${Frame + 8}`);
    for (const Register of PRESERVED.slice(0, 8).reverse()) Emit(`pop ${Register}`);
    Emit('return', 'end define');
    for (const Compare of [false, true]) {
        const Label = Compare ? 'Snapshot_compare' : 'Snapshot_save';
        Emit('define ' + Label, 'move_u32 ecx 32', 'label Cell');
        Emit('load_memory_u64 rax rsp rcx 1 8');
        const Saved = Version === 2 ? 16392 : 8168;
        if (Compare) Emit(`load_memory_u64 rdx rsp rcx 1 ${Saved}`, 'compare rax rdx', 'branch not_equal Mismatch');
        else Emit(`store_memory_u64 rsp rcx 1 ${Saved} rax`);
        Emit('add_i32 ecx 8', `compare_i32 ecx ${END}`, 'branch below Cell', 'xor eax eax', 'return');
        if (Compare) Emit('label Mismatch', 'move_u32 eax 1', 'return');
        Emit('end define');
    }
    if (Substituteˉbudgeted) Emit('define Windvale_budgeted_storage', 'move_u32 eax 1', 'return', 'end define');
    if (Substituteˉshared) Emit('define Windvale_shared_storage', 'move_u32 eax 123', 'store_memory_u32 r8 none 1 64 eax',
        'move_u32 eax 1', 'return', 'end define');
    Emit(...(Options.Definitions ?? []));
    Emit('end section', '');
    return { Name: 'domain-' + Name, Source: Lines.join('\n'), Substituteˉbudgeted, Substituteˉshared,
        ...(Options.ExtraRuntime ? { ExtraRuntime: Options.ExtraRuntime } : {}) };
}

export function Buildˉownedˉdomainˉfixture(Name, Body, Options = {}) {
    return Fixture(Name, Body, false, 2, false, Options);
}

export function Buildˉownedˉdomainˉcases() {
    const Cases = [];
    const Case = (Name, Body) => Cases.push(Fixture(Name, Body));
    Case('initialize-and-teardown', ({ Initialize, Check, Operation, Emit }) => {
        Initialize();
        for (const [Offset, Value] of [[CONTEXT, 10], [CONTEXT + 4, 136],
            [CONTEXT + 8, 1000000], [CONTEXT + 16, 1024],
            [PHYSICAL + 4, 1], [PHYSICAL + 8, 2112], [PHYSICAL + 12, 64],
            [ACCOUNTING, 1112364631], [ACCOUNTING + 16, 1],
            [ACCOUNTING + 24, 1], [ACCOUNTING + 28, 1], [ACCOUNTING + 40, 64]]) Check(Offset, Value);
        Emit('load_memory_u64 rax rsp none 1 ' + (CONTEXT + 112), 'move rcx rsp',
            'add_i32 rcx ' + ADAPTER, 'compare rax rcx', 'branch not_equal Failed',
            'load_memory_u64 rax rsp none 1 ' + (CONTEXT + 120),
            'load_address rcx Windvale_budgeted_storage', 'compare rax rcx', 'branch not_equal Failed');
        Operation(8); Check(6288, 64);
        // Allocation consumes a child budget; the retained root stays observable.
        Operation(6); Check(6272, 2); Check(6276, 1);
        Operation(1); Check(6252, 48); Check(PHYSICAL + 56, 48);
        Emit('load_memory_u64 rax rsp none 1 6232', 'store_memory_u64 rsp none 1 6304 rax',
            'load_memory_u64 rdx rsp none 1 6256', 'load_memory_u64 rax rdx none 1 0',
            'load_memory_u64 rcx rdx none 1 8', 'or rax rcx', 'test rax rax', 'branch not_equal Failed',
            'xor eax eax', 'load_memory_u8 al rdx none 1 16', 'test eax eax', 'branch not_equal Failed');
        Initialize(3); // Never reset a domain while an owner is live.
        Operation(8); Check(6288, 16);
        Operation(9); Check(PHYSICAL + 56, 0);
        Operation(8); Check(6288, 64);
        Operation(5); Check(ADAPTER + 40, 1); Check(ACCOUNTING + 24, 0);
        Initialize(3); // Nor a retired domain, even after terminal teardown.
    });
    Case('empty-authority', ({ Set, Initialize, Operation, Check }) => {
        Set(REQUEST + 72, 0); Set(REQUEST + 80, 0);
        Initialize(); Operation(8); Check(6288, 0); Operation(5);
    });
    Case('minimum-arena', ({ Set, Initialize, Operation }) => {
        Set(REQUEST + 56, 16); Set(REQUEST + 72, 16);
        Initialize(); Operation(5);
    });
    Case('request-rejections', ({ Set, Initialize }) => {
        for (const [Field, Value, Original] of [[0, 2, 1], [4, 111, 112],
            [8, 1, 0], [104, 1, 0], [84, 1, 0], [80, 65, 64],
            [56, 0, 64], [56, 15, 64], [56, 65, 64], [56, 16777232, 64],
            [60, 1, 0], [72, 65, 64], [76, 1, 0], [64, 0, 71], [88, 0, 1000000], [96, 0, 1024]]) {
            Set(REQUEST + Field, Value); Initialize(1); Set(REQUEST + Field, Original);
        }
        Initialize(1, true, null); Initialize(1, true, REQUEST + 1);
        Initialize(2, true, 'wrap'); Initialize(2, true, -32);
    });
    Case('extent-rejections', ({ Pointer, Set, Initialize, Emit }) => {
        for (const [Field, Offset] of REGIONS) {
            Emit('xor eax eax', 'store_memory_u64 rsp none 1 ' + (REQUEST + Field) + ' rax');
            Initialize(2); Pointer(Field, Offset);
            Pointer(Field, Offset + 1); Initialize(2); Pointer(Field, Offset);
            Emit('xor eax eax', 'subtract_i32 rax 16', 'store_memory_u64 rsp none 1 ' + (REQUEST + Field) + ' rax');
            Initialize(2); Pointer(Field, Offset);
            Pointer(Field, -1024); Initialize(2); Pointer(Field, Offset);
        }
        // Every pair among the request and five supplied regions.
        const All = [[0, REQUEST, 112], ...REGIONS];
        for (let Left = 0; Left < All.length; Left++) for (let Right = Left + 1; Right < All.length; Right++) {
            const [Field, Offset] = All[Right];
            Pointer(Field, All[Left][1]); Initialize(2); Pointer(Field, Offset);
        }
    });
    Case('nonzero-metadata', ({ Set, Initialize }) => {
        for (const [, Offset, Length] of REGIONS.slice(0, 4)) {
            for (const At of [Offset, Offset + Length - 4]) {
                Set(At, 1); Initialize(3); Set(At, 0);
            }
        }
    });
    Cases.push(Fixture('provider-failure-rollback', ({ Initialize }) => Initialize(4), true));
    const V2 = (Name, Body) => Cases.push(Fixture('v2-' + Name, Body, false, 2));
    V2('initialize-anchors-budgets-and-teardown', ({ Initialize, Check, Shared, Operation, Emit,
        CONTEXT: C, PHYSICAL: P, ACCOUNTING: B, ADAPTER: A, SHARED: S, MODULE: M, INPUT: I,
        Lowerˉrequest: L, Sharedˉrequest: Q }) => {
        Initialize();
        for (const [Offset, Value] of [[C, 11], [C + 4, 192], [C + 8, 1000000], [C + 16, 64],
            [C + 160, 65], [C + 164, 1], [C + 168, 66], [C + 172, 1],
            [C + 176, 4], [C + 180, 1], [C + 184, 5], [C + 188, 1],
            [A + 4, 2], [A + 8, 1216], [A + 1092, 1], [A + 1112, 16],
            [A + 1156, 1], [A + 1176, 4], [B + 36, 4], [B + 48, 112],
            [B + 56, 2], [B + 96, 2], [B + 136, 1], [B + 160, 44], [B + 176, 1], [B + 200, 48],
            [S + 4, 2], [S + 8, 2176], [S + 12, 66], [S + 2112, 65], [S + 2120, 1],
            [S + 2124, 2], [S + 2128, 16], [S + 2132, 16], [S + 2144, 66], [P + 56, 0]]) Check(Offset, Value);
        for (const [Offset, Symbol] of [[120, 'Windvale_budgeted_storage'], [136, 'Windvale_shared_storage']]) Emit(
            `load_memory_u64 rax rsp none 1 ${C + Offset}`, `load_address rcx ${Symbol}`,
            'compare rax rcx', 'branch not_equal Failed');
        Operation(8); Check(L + 80, 0); Operation(8, 4); Check(L + 80, 44); Operation(8, 5); Check(L + 80, 48);
        Shared(8); Check(Q + 96, 2); Shared(10, 160, 4, 8);
        Emit(`load_memory_u64 rax rsp none 1 ${Q + 72}`, 'move rcx rsp', `add_i32 rcx ${M + 4}`,
            'compare rax rcx', 'branch not_equal Failed');
        Shared(9); Check(S + 2120, 1); Shared(8, 168); Shared(9, 168);
        Initialize(3); Shared(11); Check(P + 28, 1); Check(A + 40, 1); Check(S + 32, 1);
        Check(B + 24, 0); Check(B + 48, 0); Check(A + 1092, 0); Check(A + 1156, 0);
        Check(A + 1088, 1); Check(A + 1152, 1); Check(M, 0x64636261); Check(I, 0x09080706);
        Initialize(3);
    });
    V2('empty-charged-anchors', ({ Set, Initialize, Check, Shared, CONTEXT: C, ACCOUNTING: B,
        ADAPTER: A, SHARED: S }) => {
        for (const Field of [72, 112, 116, 120, 128, 132, 136, 144, 148, 152, 160, 168]) Set(REQUEST + Field, 0);
        Set(REQUEST + 80, 4); Initialize();
        for (const [Offset, Value] of [[C + 160, 65], [C + 164, 1], [C + 168, 66], [C + 172, 1],
            [C + 176, 4], [C + 184, 5], [B + 36, 4], [B + 48, 0], [B + 64, 1], [B + 68, 1],
            [B + 104, 1], [B + 108, 1], [A + 1092, 1], [A + 1156, 1], [A + 1096, 0],
            [A + 1160, 0], [S + 2128, 0], [S + 2132, 0]]) Check(Offset, Value);
        Shared(10); Shared(10, 168); Shared(11); Check(B + 24, 0); Check(B + 48, 0);
        Check(A + 1088, 1); Check(A + 1152, 1);
    });
    V2('request-and-sum-rejections', ({ Set, Initialize }) => {
        for (const [Field, Value, Original] of [[0, 3, 2], [4, 191, 192], [8, 1, 0], [176, 1, 0],
            [184, 1, 0], [80, 3, 64], [168, 49, 48], [120, 17, 48], [120, 8208, 48],
            [136, 4194305, 16], [152, 4194305, 4], [72, 111, 112]]) {
            Set(REQUEST + Field, Value); Initialize(1); Set(REQUEST + Field, Original);
        }
        Set(REQUEST + 160, 4294967295); Set(REQUEST + 164, 4294967295);
        Initialize(1); Set(REQUEST + 160, 44); Set(REQUEST + 164, 0);
        Initialize(1, true, null); Initialize(1, true, REQUEST + 1);
        Initialize(2, true, 'wrap'); Initialize(2, true, -32);
    });
    V2('directory-rejections', ({ Set, Initialize, Pointer, Emit, Shared, DIRECTORY: D }) => {
        for (const [Offset, Value, Original] of [[D + 12, 2, 3], [D + 28, 6, 4], [D + 4, 5, 4],
            [D, 16, 0], [D + 8, 17, 4], [D + 16, 5, 4], [D + 20, 6, 8], [D + 32, 4294967295, 12]]) {
            Set(Offset, Value); Initialize(1); Set(Offset, Original);
        }
        // Admit all512 rows with a bounded generated loop, including repeated
        // views into one immutable backing. This span is disjoint from metadata.
        Pointer(112, 16896); Set(REQUEST + 120, 8192); Emit('xor ecx ecx', 'label Full_directory');
        for (const [Offset, Value] of [[0, 0], [4, 4], [8, 4], [12, 5]]) Emit(`move_u32 eax ${Value}`,
            `store_memory_u32 rsp rcx 1 ${16896 + Offset} eax`);
        Emit('add_i32 ecx 16', 'compare_i32 ecx 8192', 'branch below Full_directory');
        Initialize(); Shared(11);
    });
    V2('complete-region-pairs', ({ Set, Emit, Initialize, REGIONS: Regions }) => {
        const All = [[0, REQUEST, 192], ...Regions], Table = 26400, Left = 26600, Right = 26604;
        for (const [Index, [Field, Offset, Length]] of All.entries()) {
            Set(Table + Index * 16, Field); Set(Table + Index * 16 + 4, Offset); Set(Table + Index * 16 + 8, Length);
        }
        Set(Left, 0); Set(Right, 1); Emit('label Pair');
        Emit(`load_memory_u32 ecx rsp none 1 ${Left}`, 'shift_left ecx 4',
            `load_memory_u32 eax rsp rcx 1 ${Table + 4}`, 'add rax rsp',
            `load_memory_u32 ecx rsp none 1 ${Right}`, 'shift_left ecx 4',
            `load_memory_u32 ecx rsp rcx 1 ${Table}`, `store_memory_u64 rsp rcx 1 ${REQUEST} rax`);
        Initialize(2);
        Emit(`load_memory_u32 ecx rsp none 1 ${Right}`, 'shift_left ecx 4',
            `load_memory_u32 eax rsp rcx 1 ${Table + 4}`, 'add rax rsp',
            `load_memory_u32 ecx rsp rcx 1 ${Table}`, `store_memory_u64 rsp rcx 1 ${REQUEST} rax`,
            `load_memory_u32 eax rsp none 1 ${Right}`, 'add_i32 eax 1', `store_memory_u32 rsp none 1 ${Right} eax`,
            'compare_i32 eax 10', 'branch below Pair', `load_memory_u32 eax rsp none 1 ${Left}`,
            'add_i32 eax 1', `store_memory_u32 rsp none 1 ${Left} eax`, 'compare_i32 eax 9',
            'branch above_equal Pairs_done', 'add_i32 eax 1', `store_memory_u32 rsp none 1 ${Right} eax`,
            'jump_label Pair', 'label Pairs_done');
    });
    V2('extent-guards', ({ Emit, Pointer, Initialize, REGIONS: Regions }) => {
        for (const [Field, Offset] of Regions) {
            Emit('xor eax eax', `store_memory_u64 rsp none 1 ${REQUEST + Field} rax`);
            Initialize(2); Pointer(Field, Offset);
            if (![128, 144].includes(Field)) { Pointer(Field, Offset + 1); Initialize(2); Pointer(Field, Offset); }
            Emit('xor eax eax', 'subtract_i32 rax 2', `store_memory_u64 rsp none 1 ${REQUEST + Field} rax`);
            Initialize(2); Pointer(Field, Offset);
            Pointer(Field, -1024); Initialize(2); Pointer(Field, Offset);
        }
    });
    V2('fresh-metadata', ({ Set, Initialize, REGIONS: Regions }) => {
        for (const [, Offset, Length] of Regions.filter(([Field]) => ![48, 112, 128, 144].includes(Field))) {
            for (const At of [Offset, Offset + Length - 4]) { Set(At, 1); Initialize(3); Set(At, 0); }
        }
    });
    Cases.push(Fixture('v2-budget-provider-rollback', ({ Initialize }) => Initialize(4), true, 2));
    Cases.push(Fixture('v2-shared-provider-rollback', ({ Initialize }) => Initialize(4), false, 2, true));
    return Cases;
}

// ABI25 entry evidence composes the existing version-two constructor fixture.
// Each fixture supplies only private body stubs; production entry code is linked
// by the existing owner through ExtraRuntime, never copied into test assembly.
function Entryˉbodyˉdefinitions({ Trap = false, Corrupt = false } = {}) {
    const Lines = ['define Windvale_owned_body', 'xor eax eax', 'return', 'end define'];
    for (const Bytes of [false, true]) {
        const Symbol = Bytes ? 'Windvale_shared_bytes_body' : 'Windvale_shared_scalar_body';
        const Emit = (...Value) => Lines.push(...Value);
        Emit(`define ${Symbol}`, 'push r12', 'push r13', 'push r14', 'subtract_i32 rsp 160',
            'move r12 rdx', 'move r13 rcx', `move r14 ${Bytes ? 'r9' : 'r8'}`,
            'store_memory_u64 r12 none 1 8832 r14');
        function Query(Shared) {
            Emit('xor eax eax', 'move_u32 ecx 32', `label Clear_${Shared ? 'share' : 'budget'}`,
                'store_memory_u64 rsp rcx 1 0 rax', 'add_i32 ecx 8', 'compare_i32 ecx 160',
                `branch below Clear_${Shared ? 'share' : 'budget'}`, 'move_u32 eax 2',
                'store_memory_u32 rsp none 1 32 eax', `move_u32 eax ${Shared ? 128 : 112}`,
                'store_memory_u32 rsp none 1 36 eax', `move_u32 eax ${Shared ? 8 : 7}`,
                'store_memory_u32 rsp none 1 40 eax', 'load_memory_u64 r8 r12 none 1 128',
                'load_memory_u64 rax r8 none 1 24', 'store_memory_u64 rsp none 1 48 rax');
            if (Shared) Emit('load_memory_u64 rax r13 none 1 16', 'store_memory_u64 rsp none 1 56 rax');
            else Emit('store_memory_u64 rsp none 1 96 r14', 'load_memory_u64 r8 r12 none 1 112');
            Emit('move r9 rsp', 'add_i32 r9 32', `call Windvale_${Shared ? 'shared' : 'budgeted'}_storage`,
                'test eax eax', 'branch not_equal Invalid');
        }
        if (Bytes && !Trap) Query(true);
        Query(false);
        if (Corrupt) Emit('load_memory_u64 rax r12 none 1 128', 'move_u32 ecx 1',
            'store_memory_u32 rax none 1 36 ecx');
        if (Trap) Emit('move_u32 eax 2', 'shift_left rax 32', 'add_i32 rax 77');
        else if (Bytes) Emit('load_memory_u64 rax r13 none 1 16', 'store_memory_u64 r13 none 1 0 rax',
            'load_memory_u64 rax r13 none 1 24', 'store_memory_u64 r13 none 1 8 rax', 'xor eax eax');
        else Emit('move_u32 eax 42');
        Emit('jump_label Finish', 'label Invalid', 'move_u32 eax 9', 'shift_left rax 32',
            'label Finish', 'add_i32 rsp 160', 'pop r14', 'pop r13', 'pop r12', 'return', 'end define');
    }
    return Lines;
}

export function Buildˉownedˉentryˉcases() {
    const Cases = [];
    const Symbols = ['symbol import function Windvale_shared_bytes_entry',
        'symbol import function Windvale_shared_scalar_entry', 'symbol import function Windvale_shared_result_close',
        'symbol export function Windvale_owned_body in .text',
        'symbol export function Windvale_shared_bytes_body in .text',
        'symbol export function Windvale_shared_scalar_body in .text'];
    function Case(Name, Body, Stub = {}, Frame = 32768) {
        Cases.push(Buildˉownedˉdomainˉfixture('entry-' + Name, Context => {
            const { Emit, Check, Set, Initialize, CONTEXT: C, PHYSICAL: P, ADAPTER: A, SHARED: S, ACCOUNTING: B } = Context;
            const Bridge = 12000;
            function Input() {
                Emit('xor eax eax', `store_memory_u64 rsp none 1 ${Bridge} rax`,
                    `store_memory_u64 rsp none 1 ${Bridge + 8} rax`,
                    `load_memory_u64 rax rsp none 1 ${C + 168}`, `store_memory_u64 rsp none 1 ${Bridge + 16} rax`,
                    `load_memory_u64 rax rsp none 1 ${S + 2160}`, `store_memory_u64 rsp none 1 ${Bridge + 24} rax`);
            }
            function Call(Kind, Expected = Kind === 'scalar' ? 42 : 0, Atomic = Expected === 9 || (Kind === 'close' && Expected === 1), Address = C, Cells = Bridge) {
                if (Atomic) {
                    Emit('call Snapshot_save');
                    for (let Cell = 0; Cell < 4; Cell++) Emit(`load_memory_u64 rax rsp none 1 ${Bridge + Cell * 8}`,
                        `store_memory_u64 rsp none 1 ${27700 + Cell * 8} rax`);
                }
                for (const [Index, Register] of PRESERVED.slice(0, 8).entries()) {
                    const Low = Register === 'rbp' ? 'ebp' : Register === 'rbx' ? 'ebx' : Register === 'rsi' ? 'esi' : Register === 'rdi' ? 'edi' : Register + 'd';
                    Emit(`move_u32 ${Low} ${11000 + Index}`);
                }
                Emit('move rdx rsp', `add_i32 rdx ${Address}`);
                if (Kind === 'bytes') Emit('move rcx rsp', `add_i32 rcx ${Cells}`);
                else Emit('xor ecx ecx');
                Emit(`call Windvale_shared_${Kind === 'close' ? 'result_close' : Kind + '_entry'}`);
                if (Expected === 9) Emit('shift_right rax 32', 'compare_i32 eax 9', 'branch not_equal Failed');
                else if (Expected === 'trap') Emit('move rdx rax', 'shift_right rdx 32', 'compare_i32 edx 2',
                    'branch not_equal Failed', 'compare_i32 eax 77', 'branch not_equal Failed');
                else Emit(`compare_i32 rax ${Expected}`, 'branch not_equal Failed');
                for (const [Index, Register] of PRESERVED.slice(0, 8).entries()) {
                    const Low = Register === 'rbp' ? 'ebp' : Register === 'rbx' ? 'ebx' : Register === 'rsi' ? 'esi' : Register === 'rdi' ? 'edi' : Register + 'd';
                    Emit(`compare_i32 ${Low} ${11000 + Index}`, 'branch not_equal Failed');
                }
                if (Atomic) {
                    Emit('call Snapshot_compare', 'test eax eax', 'branch not_equal Failed');
                    for (let Cell = 0; Cell < 4; Cell++) Emit(`load_memory_u64 rax rsp none 1 ${Bridge + Cell * 8}`,
                        `load_memory_u64 rdx rsp none 1 ${27700 + Cell * 8}`, 'compare rax rdx', 'branch not_equal Failed');
                }
            }
            function Closed(Base = 0) {
                for (const [At, Value] of [[P + 28, 1], [P + 56, 0], [A + 40, 1], [S + 32, 1],
                    [B + 24, 0], [B + 48, 0], [A + 1092, 0], [A + 1156, 0], [S + 2120, 0], [S + 2152, 0]]) Check(At + Base, Value);
            }
            function Second() {
                const Base = 32768;
                for (const [Field, Offset] of Context.REGIONS) Context.Pointer(Field, Offset + Base);
                Set(Context.REQUEST + 64, 72);
                for (const [At, Value] of [[Context.DIRECTORY, 0], [Context.DIRECTORY + 4, 4],
                    [Context.DIRECTORY + 8, 4], [Context.DIRECTORY + 12, 5]]) Set(At + Base, Value);
                for (const At of [Context.DIRECTORY + 16, Context.DIRECTORY + 32]) {
                    Set(At + Base, 0); Set(At + Base + 4, 0); Set(At + Base + 8, 0); Set(At + Base + 12, 5);
                }
                Initialize();
                for (const [Field, Offset] of Context.REGIONS) Context.Pointer(Field, Offset);
                Set(Context.REQUEST + 64, 71);
                return Base;
            }
            Body({ ...Context, Input, Call, Closed, Second, Bridge });
        }, { Symbols, Definitions: Entryˉbodyˉdefinitions(Stub), ExtraRuntime: 'Entry', Frame }));
    }
    Case('bytes-alias-survival-and-close', ({ Initialize, Input, Call, Closed, Check, CONTEXT: C, SHARED: S, ACCOUNTING: B }) => {
        Initialize(); Input(); Call('close', 1); Call('bytes');
        for (const [At, Value] of [[C + 184, 0], [C + 188, 0], [S + 32, 0], [S + 2152, 2],
            [B + 24, 1], [B + 48, 64], [9056, 5], [9060, 1], [12000, 66], [12004, 1], [12008, 4], [12012, 4]]) Check(At, Value);
        Call('bytes', 9); Call('close'); Closed(); Call('close', 1);
    });
    Case('scalar-return-closes-domain', ({ Initialize, Call, Closed, Check, CONTEXT: C }) => {
        Initialize(); Call('scalar'); Closed(); Check(C + 184, 0); Check(9056, 5); Check(9060, 1); Call('scalar', 9);
    });
    Case('byte-trap-closes-domain', ({ Initialize, Input, Call, Closed }) => {
        Initialize(); Input(); Call('bytes', 'trap'); Closed();
    }, { Trap: true });
    Case('earlier-trap-survives-cleanup-refusal', ({ Initialize, Input, Call, Check, PHYSICAL: P, SHARED: S, ACCOUNTING: B }) => {
        Initialize(); Input(); Call('bytes', 'trap'); Check(P + 28, 0); Check(S + 32, 0); Check(S + 36, 1); Check(B + 24, 1);
        Call('close', 1);
    }, { Trap: true, Corrupt: true });
    Case('charged-empty-input-and-result', ({ Set, Initialize, Input, Call, Closed, Check, REQUEST: R, SHARED: S, ACCOUNTING: B }) => {
        for (const Field of [112, 116, 120, 128, 132, 136, 144, 148, 152]) Set(R + Field, 0);
        Set(R + 72, 92); Initialize(); Input(); Call('bytes');
        Check(12000, 66); Check(12004, 1); Check(12008, 0); Check(12012, 0); Check(S + 2152, 2); Check(B + 48, 44);
        Call('close'); Closed();
    });
    Case('context-owner-and-input-refusals', ({ Initialize, Input, Call, Set, Emit, CONTEXT: C, SHARED: S, ADAPTER: A, Bridge }) => {
        Initialize(); Input();
        for (const [At, Value, Original] of [[C, 10, 11], [C + 4, 191, 192], [C + 16, 65, 64], [C + 24, 1, 0],
            [C + 160, 193, 65], [C + 164, 2, 1], [C + 168, 65, 66], [C + 172, 2, 1],
            [C + 176, 1, 4], [C + 176, 5, 4], [C + 188, 2, 1], [S + 24, 72, 71], [A + 32, 72, 71],
            [Bridge, 1, 0], [Bridge + 16, 65, 66], [Bridge + 20, 2, 1], [Bridge + 24, 5, 4], [Bridge + 28, 5, 4]]) {
            Set(At, Value); Call('bytes', 9); Set(At, Original);
        }
        for (const Field of [120, 136]) {
            Emit('load_address rax Windvale_owned_body', `store_memory_u64 rsp none 1 ${C + Field} rax`);
            Call('bytes', 9);
            Emit(`load_address rax Windvale_${Field === 120 ? 'budgeted' : 'shared'}_storage`,
                `store_memory_u64 rsp none 1 ${C + Field} rax`);
        }
        Call('bytes'); Call('close');
    });
    Case('directory-geometry-and-row-refusals', ({ Initialize, Input, Call, Set, Emit, CONTEXT: C, DIRECTORY: D,
        PHYSICAL: P, ACCOUNTING: B, ADAPTER: A, SHARED: S, ARENA: N, MODULE: M, INPUT: I }) => {
        Initialize(); Input();
        for (const [At, Value, Original] of [[C + 152, 47, 48], [C + 152, 8208, 48], [C + 156, 1, 0],
            [D + 12, 2, 3], [D + 4, 5, 4], [D + 8, 17, 4], [D, 4294967295, 0],
            [D + 16, 5, 4], [D + 20, 7, 8], [D + 28, 6, 4]]) {
            Set(At, Value); Call('bytes', 9); Set(At, Original);
        }
        for (const At of [C, P, B, A, S, N, M, I, D + 1]) {
            Emit('move rax rsp', `add_i32 rax ${At}`, `store_memory_u64 rsp none 1 ${C + 144} rax`); Call('bytes', 9);
        }
        Emit('move rax rsp', `add_i32 rax ${D}`, `store_memory_u64 rsp none 1 ${C + 144} rax`);
        // u32-array maximum is a physical byte bound and need not be divisible by4.
        Set(D + 24, 9); Call('bytes'); Call('close');
    });
    Case('complete-entry-scratch-refusals', ({ Initialize, Input, Call, Set, Emit, CONTEXT: C, ADAPTER: A, PHYSICAL: P,
        SHARED: S, ACCOUNTING: B, ARENA: N, DIRECTORY: D, MODULE: M, INPUT: I }) => {
        Initialize(); Input();
        const Table = [[C + 128, S], [C + 112, A], [A + 16, P], [A + 24, B], [P + 32, N],
            [C + 144, D], [A + 1096, M], [A + 1160, I]];
        for (const [Index, [Field, Original]] of Table.entries()) {
            Set(26400 + Index * 8, Field); Set(26404 + Index * 8, Original);
        }
        Set(9072, 0); Emit('label Scratch_next', 'load_memory_u32 ecx rsp none 1 9072',
            'load_memory_u32 edx rsp rcx 8 26400', 'move rax rsp', 'subtract_i32 rax 256',
            'store_memory_u64 rsp rdx 1 0 rax');
        // Save the complete pre-push write window, excluding the CALL return slot.
        Emit('move_u32 ecx 512', 'label Scratch_save', 'move rdx rsp', 'subtract rdx rcx',
            'move_u32 eax 123456789', 'store_memory_u64 rdx none 1 0 rax',
            'store_memory_u64 rsp rcx 1 28000 rax', 'subtract_i32 ecx 8', 'compare_i32 ecx 8', 'branch above Scratch_save');
        Call('bytes', 9);
        Emit('move_u32 ecx 512', 'label Scratch_compare', 'move rdx rsp', 'subtract rdx rcx',
            'load_memory_u64 rax rdx none 1 0', 'load_memory_u64 rdx rsp rcx 1 28000', 'compare rax rdx',
            'branch not_equal Failed', 'subtract_i32 ecx 8', 'compare_i32 ecx 8', 'branch above Scratch_compare',
            'load_memory_u32 ecx rsp none 1 9072', 'load_memory_u32 edx rsp rcx 8 26400',
            'load_memory_u32 eax rsp rcx 8 26404', 'add rax rsp', 'store_memory_u64 rsp rdx 1 0 rax',
            'add_i32 ecx 1', 'store_memory_u32 rsp none 1 9072 ecx', 'compare_i32 ecx 8', 'branch below Scratch_next');
        Call('bytes', 9, true, -128); Call('bytes', 9, true, C, -32); Call('bytes'); Call('close');
    });
    Case('finalizer-refuses-another-valid-domain', ({ Initialize, Input, Call, Second, Check, Closed, Emit,
        CONTEXT: C, SHARED: S, ADAPTER: A, PHYSICAL: P, ACCOUNTING: B }) => {
        Initialize(); Input(); const Base = Second(); Call('bytes');
        Emit('move rax rsp', `add_i32 rax ${S + Base}`, `store_memory_u64 rsp none 1 ${C + 128} rax`);
        Call('close', 1); Check(P + Base + 28, 0); Check(A + Base + 40, 0); Check(S + Base + 32, 0); Check(B + Base + 24, 1);
        Emit('move rax rsp', `add_i32 rax ${S}`, `store_memory_u64 rsp none 1 ${C + 128} rax`);
        Call('close'); Closed();
        Emit('xor eax eax', `store_memory_u64 rsp none 1 ${C + Base + 184} rax`);
        Call('close', 0, false, C + Base); Closed(Base);
    }, {}, 65536);
    return Cases;
}
