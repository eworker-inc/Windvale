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

function Fixture(Name, Body, Substituteˉbudgeted = false) {
    const Lines = ['windvale-assembly 1',
        'symbol local function Snapshot_compare in .text', 'symbol local function Snapshot_save in .text',
        'symbol export function Main in .text',
        `symbol ${Substituteˉbudgeted ? 'export function Windvale_budgeted_storage in .text' : 'import function Windvale_budgeted_storage'}`,
        'symbol import function Windvale_owned_domain_initialize',
        'section code .text align 16', 'define Main'];
    const Emit = (...Value) => Lines.push(...Value);
    for (const Register of PRESERVED.slice(0, 8)) Emit(`push ${Register}`);
    Emit('xor eax eax');
    for (let Page = 0; Page < 4; Page++) Emit('subtract_i32 rsp 4096', 'store_memory_u64 rsp none 1 0 rax');
    Emit('subtract_i32 rsp 8', 'move_u32 ecx 32', 'label Clear',
        'store_memory_u64 rsp rcx 1 0 rax', 'add_i32 ecx 8', 'compare_i32 ecx 16384', 'branch below Clear');
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
    function Operation(Selector) {
        const Label = 'Clear_operation_' + Operationˉindex++;
        Emit('xor eax eax', 'move_u32 ecx 6208', 'label ' + Label,
            'store_memory_u64 rsp rcx 1 0 rax', 'add_i32 ecx 8',
            'compare_i32 ecx 6304', 'branch below ' + Label);
        Set(6208, 1); Set(6212, 96); Set(6216, Selector); Set(6224, 71);
        if (Selector === 8) { Set(6272, 1); Set(6276, 1); }
        if (Selector === 6) { Set(6272, 1); Set(6276, 1); Set(6280, 48); Set(6296, 0); }
        if (Selector === 1) {
            Set(6220, 17); Set(6240, 16); Set(6244, 17); Set(6272, 2); Set(6276, 1);
        }
        if (Selector === 9) Emit('load_memory_u64 rax rsp none 1 6304', 'store_memory_u64 rsp none 1 6232 rax');
        Emit('move r8 rsp', `add_i32 r8 ${ADAPTER}`, 'move r9 rsp', 'add_i32 r9 6208',
            'call Windvale_budgeted_storage', 'test eax eax', 'branch not_equal Failed');
    }
    for (const [Offset, Value] of [[0, 1], [4, 112], [56, 64], [64, 71], [72, 64],
        [80, 64], [88, 1000000], [96, 1024]]) Set(REQUEST + Offset, Value);
    for (const [Field, Offset] of REGIONS) Pointer(Field, Offset);
    // Nonzero arena contents are permitted; reserve zeroes exposed backing.
    Set(ARENA, 0x12345678); Set(ARENA + 12, 0x23456789);
    Body({ Emit, Set, Pointer, Check, Initialize, Operation });
    Emit('move_u32 eax 42', 'jump_label Finish', 'label Failed', 'move_u32 eax 1',
        'label Finish', 'add_i32 rsp 16392');
    for (const Register of PRESERVED.slice(0, 8).reverse()) Emit(`pop ${Register}`);
    Emit('return', 'end define');
    for (const Compare of [false, true]) {
        const Label = Compare ? 'Snapshot_compare' : 'Snapshot_save';
        Emit('define ' + Label, 'move_u32 ecx 32', 'label Cell');
        Emit('load_memory_u64 rax rsp rcx 1 8');
        if (Compare) Emit('load_memory_u64 rdx rsp rcx 1 8168', 'compare rax rdx', 'branch not_equal Mismatch');
        else Emit('store_memory_u64 rsp rcx 1 8168 rax');
        Emit('add_i32 ecx 8', `compare_i32 ecx ${END}`, 'branch below Cell', 'xor eax eax', 'return');
        if (Compare) Emit('label Mismatch', 'move_u32 eax 1', 'return');
        Emit('end define');
    }
    if (Substituteˉbudgeted) Emit('define Windvale_budgeted_storage', 'move_u32 eax 1', 'return', 'end define');
    Emit('end section', '');
    return { Name: 'domain-' + Name, Source: Lines.join('\n'), Substituteˉbudgeted };
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
    return Cases;
}
