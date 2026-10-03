import { Buildˉstorageˉfixture } from './Native-Storage-Fixture.mjs';

// Generated fixtures extend the existing native storage owner and its complete
// physical/accounting snapshots. Shared state and request remain fixed-size.
const SHARED = 6400, REQUEST = 8512, SAVED = 9000, INPUT = 13440;
const ACCOUNTING = 17504;

function Fixture(Name, Arena, Body) {
    return Buildˉstorageˉfixture(`shared-${Name}`, Arena, Base => {
        const { Emit, Set, Load, Snapshot } = Base;
        function Check(Region, Offset, Value) { Base.Check(Region, Offset, Value | 0); }
        // Buildˉstorageˉfixture has already initialized the same canonical domain.
        Set('rsp', SHARED + 4, 1); Set('rsp', SHARED + 8, 2112);
        Set('rsp', SHARED + 12, 64); Set('rsp', SHARED + 24, 71);
        Emit(`store_memory_u64 rsp none 1 ${SHARED + 16} r12`);
        let Serial = 0;
        function Word(Offset, Value) { Set('rsp', REQUEST + Offset, Value); }
        function Wide(Offset, Value) {
            const Integer = BigInt(Value);
            Word(Offset, Number(Integer & 0xffffffffn)); Word(Offset + 4, Number(Integer >> 32n));
        }
        function Request(Operation, { Handle = null, Budget = 0n, Value = 0n, Length = 0n, Source = null } = {}) {
            Emit('call Shared_request_reset');
            if (Operation !== 0) Word(8, Operation);
            if (BigInt(Budget) !== 0n) Wide(32, Budget);
            if (BigInt(Value) !== 0n) Wide(40, Value);
            if (BigInt(Length) !== 0n) Wide(48, Length);
            if (Handle !== null) { Load(Handle); Emit(`store_memory_u64 rsp none 1 ${REQUEST + 24} rax`); }
            if (Source !== null) Emit('move rax rsp', `add_i32 rax ${Source}`, `store_memory_u64 rsp none 1 ${REQUEST + 56} rax`);
        }
        function Sharedˉsnapshot(Compare) {
            const Label = `Shared_snapshot_${++Serial}`;
            Emit('xor ecx ecx', `label ${Label}`);
            if (Compare) Emit(`compare_i32 ecx ${2112 + 64}`, `branch equal ${Label}_next`);
            Emit(`load_memory_u32 eax rsp rcx 1 ${SHARED}`);
            if (Compare) Emit(`load_memory_u32 edx rsp rcx 1 ${SAVED}`, 'compare eax edx', 'branch not_equal Failed');
            else Emit(`store_memory_u32 rsp rcx 1 ${SAVED} eax`);
            if (Compare) Emit(`label ${Label}_next`);
            Emit('add_i32 ecx 4', 'compare_i32 ecx 2240', `branch below ${Label}`);
        }
        function Call(Status = 0, Atomic = Status !== 0, Setup = [], Pointerˉfailure = false) {
            if (Atomic) { Snapshot(false); Sharedˉsnapshot(false); }
            Emit('store_memory_u64 rsp none 1 13100 rdi', 'store_memory_u64 rsp none 1 13108 rbp',
                'move r8 rsp', `add_i32 r8 ${SHARED}`, 'move r9 rsp', `add_i32 r9 ${REQUEST}`,
                'move_u32 r10d 12345', 'move_u32 r11d 67890', ...Setup, 'call Windvale_shared_storage',
                `compare_i32 eax ${Status}`, 'branch not_equal Failed',
                'compare_i32 r10d 12345', 'branch not_equal Failed', 'compare_i32 r11d 67890', 'branch not_equal Failed',
                'compare_i32 ebx 5151', 'branch not_equal Failed', 'compare_i32 esi 6161', 'branch not_equal Failed',
                'compare_i32 r15d 7171', 'branch not_equal Failed',
                'load_memory_u64 rax rsp none 1 13100', 'compare rax rdi', 'branch not_equal Failed',
                'load_memory_u64 rax rsp none 1 13108', 'compare rax rbp', 'branch not_equal Failed');
            Check('rsp', REQUEST + 64, Pointerˉfailure ? 0 : Status);
            if (Atomic) { Snapshot(true); Sharedˉsnapshot(true); }
        }
        function Save(Offset, Slot) {
            Emit(`load_memory_u64 rax rsp none 1 ${REQUEST + Offset}`, `store_memory_u64 rsp none 1 ${12800 + Slot * 8} rax`);
        }
        function Reserve(Maximum = 17, Slot = 0, Budget = 4294967297n, Status = 0) {
            Request(1, { Value: Maximum, Budget }); Call(Status, false);
            if (Status === 0) { Save(24, Slot); Save(72, Slot + 1); }
        }
        function Append(Operation, Value, Slot = 0, Status = 0) { Request(Operation, { Handle: Slot, Value }); Call(Status); }
        function Freeze(Slot = 0) { Request(7, { Handle: Slot }); Call(); }
        function Release(Slot = 0) { Request(9, { Handle: Slot }); Call(); }
        function Sharedˉappend(Target, Source, Start, Length, Status = 0) {
            Request(13, { Handle: Target, Length: Start }); Load(Source);
            Emit(`store_memory_u64 rsp none 1 ${REQUEST + 40} rax`); Wide(56, Length); Call(Status);
        }
        function Split(Maximum = 48) {
            Base.Request(6); Set('r13', 64, 1); Set('r13', 68, 1); Set('r13', 72, Maximum); Base.Call();
            Emit('load_memory_u64 rax r13 none 1 64', 'store_memory_u64 rsp none 1 13320 rax');
        }
        function Childˉreserve(Maximum = 17, Status = 0) {
            Request(1, { Value: Maximum }); Emit('load_memory_u64 rax rsp none 1 13320',
                `store_memory_u64 rsp none 1 ${REQUEST + 32} rax`); Call(Status, false);
            if (Status === 0) { Save(24, 0); Save(72, 1); }
        }
        Request(0); Call();
        Body({ ...Base, Check, Word, Wide, Request, Call, Save, Reserve, Append, Freeze, Release,
            Sharedˉappend, Sharedˉsnapshot, Split, Childˉreserve });
    }, true).Source.replace('symbol local function Snapshot_compare',
        'symbol local function Shared_request_reset in .text\nsymbol local function Snapshot_compare')
        .replace('symbol import function Windvale_owned_storage',
            'symbol import function Windvale_owned_storage\nsymbol import function Windvale_shared_storage')
        .replace('\nend section\n', '\ndefine Shared_request_reset\nmove r8 rsp\n' +
            `add_i32 r8 ${REQUEST + 8}\nxor eax eax\nxor ecx ecx\nlabel Clear\n` +
            'store_memory_u64 r8 rcx 1 0 rax\nadd_i32 ecx 8\ncompare_i32 ecx 128\nbranch below Clear\n' +
            'move_u32 eax 1\nstore_memory_u32 r8 none 1 0 eax\n' +
            'move_u32 eax 128\nstore_memory_u32 r8 none 1 4 eax\n' +
            'move_u32 eax 71\nstore_memory_u32 r8 none 1 16 eax\nreturn\nend define\nend section\n');
}

export function Buildˉsharedˉstorageˉcases() {
    const Cases = [];
    const Case = (Name, Arena, Body) => Cases.push({ Name: `shared-${Name}`, Source: Fixture(Name, Arena, Body) });
    Case('freeze-share-append-and-credit', 112, ({ Reserve, Append, Freeze, Release, Request, Call, Check, Load, Sharedˉappend, Emit }) => {
        Reserve(64); Append(2, 97); Append(3, 0x44332211); Append(4, 0xffeeddccbbaa9988n);
        Append(5, 18446744073709551615n); Check('rsp', REQUEST + 80, 33); Check('rsp', 88, 80);
        Load(1, 'rdi'); Check('rdi', 0, 0x33221161); Check('rdi', 5, 0xbbaa9988);
        Check('rdi', 13, 0x34343831); Check('rdi', 29, 0x35313631);
        Freeze(); Check('rsp', SHARED + 76, 2); Check('rsp', REQUEST + 88, 64);
        Request(8, { Handle: 0 }); Call(); Check('rsp', REQUEST + 96, 2);
        Release(); Check('rsp', 88, 80); Request(10, { Handle: 0, Value: 1, Length: 4 }); Call();
        Emit(`load_memory_u64 rdi rsp none 1 ${REQUEST + 72}`); Check('rdi', 0, 0x44332211);
        // The source survives after its mutable owner and one alias disappear.
        // A separate borrowed root budget funds the second full backing.
        Request(1, { Value: 4, Budget: 4294967297n }); Call(4); // consumed root is a lease
        Release(); Check('rsp', 88, 0); Request(11); Call();
    });
    // Independent child budgets permit two allocations without reusing an owner.
    Case('borrowed-shared-source', 112, ({ Split, Childˉreserve, Append, Freeze, Sharedˉappend, Release, Save, Request, Call, Check, Load, Emit, Set }) => {
        Split(80); Childˉreserve(64); Append(3, 0x44332211); Freeze();
        Split(32); Request(1, { Value: 4 }); Emit('load_memory_u64 rax rsp none 1 13320',
            `store_memory_u64 rsp none 1 ${REQUEST + 32} rax`); Call(); Save(24, 2); Save(72, 3);
        Sharedˉappend(2, 0, 0, 4); Check('rsp', 88, 112); Release(0); Check('rsp', 88, 32);
        Check('rsp', ACCOUNTING + 48, 32); Freeze(2); Request(10, { Handle: 2, Length: 4 }); Call();
        Emit(`load_memory_u64 rdi rsp none 1 ${REQUEST + 72}`); Check('rdi', 0, 0x44332211);
        // Reuse the first non-tail block while the second immutable owner lives.
        Split(80); Request(1, { Value: 64 }); Emit('load_memory_u64 rax rsp none 1 13320',
            `store_memory_u64 rsp none 1 ${REQUEST + 32} rax`); Call(); Save(24, 4); Save(72, 5);
        Load(1, 'rcx'); Load(5); Emit('compare rax rcx', 'branch not_equal Failed');
        Check('rsp', 88, 112); Append(4, 0x8877665544332211n, 4);
        Request(10, { Handle: 2, Length: 4 }); Call();
        Emit(`load_memory_u64 rdi rsp none 1 ${REQUEST + 72}`); Check('rdi', 0, 0x44332211);
        Release(4); Check('rsp', 88, 32); Check('rsp', ACCOUNTING + 48, 32);
        Release(2); Check('rsp', 88, 0); Check('rsp', ACCOUNTING + 48, 0);
        Request(11); Call(); Check('r12', 40, 1); Check('rsp', SHARED + 32, 1);
    });
    Case('empty-maximum-and-reuse', 32, ({ Split, Childˉreserve, Append, Freeze, Request, Call, Release, Load, Check, Emit }) => {
        Split(32); Childˉreserve(0); Check('rsp', REQUEST + 68, 32); Check('rsp', REQUEST + 88, 0);
        Append(2, 1, 0, 7); Freeze(); Request(10, { Handle: 0 }); Call();
        Check('rsp', REQUEST + 80, 0); Release(); Check('rsp', 88, 0);
        Load(1); Emit('store_memory_u64 rsp none 1 12832 rax');
        Split(32); Childˉreserve(16); Load(4, 'rcx'); Load(1); Emit('compare rax rcx', 'branch not_equal Failed');
        Release(); Check('rsp', 88, 0);
        Request(12); Call(); Request(11); Call(); Check('rsp', SHARED + 32, 1);
    });
    Case('external-span-and-atomic-appends', 48, ({ Split, Childˉreserve, Append, Request, Call, Word, Freeze, Release, Check, Load, Set }) => {
        Split(); Childˉreserve(17); Set('rsp', INPUT, 0x04030201);
        Request(6, { Handle: 0, Length: 4, Source: INPUT }); Call();
        Check('rsp', INPUT, 0x04030201);
        Append(5, 0); Check('rsp', REQUEST + 80, 5); Load(1, 'rdi'); Check('rdi', 0, 0x04030201);
        Append(5, 18446744073709551615n, 0, 7); Check('rsp', SHARED + 80, 5);
        Request(6, { Handle: 0 }); Call(); Freeze(); Append(2, 1, 0, 4);
        Request(13, { Handle: 0 }); Call(4); Release(); Check('rsp', 88, 0);
    });
    Case('geometry-stale-and-corrupt', 48, ({ Split, Childˉreserve, Append, Freeze, Release, Request, Call, Word, Wide, Set, Check, Save }) => {
        Split(); Childˉreserve(17); Append(3, 0x44332211); Freeze();
        for (const [Start, Length] of [[4n, 1n], [5n, 0n], [18446744073709551615n, 1n]]) {
            Request(10, { Handle: 0, Value: Start, Length }); Call(7);
        }
        Request(10, { Handle: 0, Value: 4 }); Call(); Check('rsp', REQUEST + 80, 0);
        for (const [Offset, Value, Original] of [[64 + 4, 2, 1], [64 + 8, 0, 1], [64 + 12, 3, 2],
            [64 + 16, 18, 4], [64 + 20, 16, 17], [64 + 24, 1, 0], [96 + 8, 1, 0], [36, 1, 0]]) {
            Set('rsp', SHARED + Offset, Value); Request(12); Call(5); Set('rsp', SHARED + Offset, Original);
        }
        Set('rsp', SHARED + 72, 4294967295); Request(8, { Handle: 0 }); Call(6);
        Set('rsp', SHARED + 72, 1);
        Request(8, { Handle: 0 }); Word(16, 72); Call(4);
        Request(8, { Handle: 0 }); Word(24, 65); Call(4);
        Release(); Request(9, { Handle: 0 }); Call(4);
        Split(); Childˉreserve(17); Request(9, { Handle: 0 }); Word(28, 1); Call(4);
        Release(); Check('rsp', 88, 0);
    });
    Case('malformed-and-overlapping-extents', 48, ({ Split, Childˉreserve, Request, Call, Word, Wide, Set, Emit }) => {
        Split(); Childˉreserve(17);
        for (const [Offset, Value] of [[0, 2], [4, 127], [8, 14], [12, 1], [32, 1], [40, 256],
            [48, 1], [56, 1], [64, 1], [100, 1], [120, 1]]) {
            Request(2, { Handle: 0, Value: 1 }); Word(Offset, Value); Call(1);
        }
        Request(3, { Handle: 0, Value: 4294967296n }); Call(1);
        for (const Setup of [['xor r8d r8d'], ['add_i32 r8 1'], ['add_i32 r9 1'],
            ['move r9 r8'], ['move r9 r12'], ['move r9 r14'], ['move r9 rsp', 'subtract_i32 r9 128']]) {
            Request(12); Call(1, true, Setup, true);
        }
        Request(6, { Handle: 0, Length: 1 }); Emit(`store_memory_u64 rsp none 1 ${REQUEST + 56} r14`);
        Call(1, true, [], true);
        Request(6, { Handle: 0, Length: 2 }); Wide(56, 18446744073709551615n); Call(1, true, [], true);
        Request(6, { Handle: 0, Length: 4194305, Source: INPUT }); Call(1, true, [], true);
        Request(6, { Handle: 0, Length: 1 }); Call(1, true, [], true);
        Request(6, { Handle: 0, Source: INPUT }); Call(1);
        Request(6, { Handle: 0, Length: 1 }); Emit('move rax rsp', 'subtract_i32 rax 128',
            `store_memory_u64 rsp none 1 ${REQUEST + 56} rax`); Call(1, true, [], true);
    });
    Case('constructor-refusal-consumes-budget', 48, ({ Split, Childˉreserve, Check, Request, Call }) => {
        Split(31); Childˉreserve(17, 2); Check('rsp', ACCOUNTING + 48, 0);
        Check('rsp', REQUEST + 104, 48); Check('rsp', REQUEST + 112, 31);
        Split(); Childˉreserve(4194305, 8); Check('rsp', ACCOUNTING + 48, 0);
        Check('rsp', REQUEST + 104, 4194336);
        Split(); Childˉreserve(18446744073709551615n, 8);
        Check('rsp', REQUEST + 104, -1); Check('rsp', REQUEST + 108, -1);
        Request(11); Call();
    });
    Case('physical-refusal-and-ownerless-validation', 16, ({ Reserve, Request, Call, Check }) => {
        Reserve(1, 0, 4294967297n, 3); Check('rsp', ACCOUNTING + 24, 0);
        Request(12); Call(); Request(11); Call(); Check('r12', 40, 1);
    });
    Case('retired-budget-generation', 48, ({ Set, Reserve, Request, Call, Check }) => {
        Set('rsp', ACCOUNTING + 16, 4294967295);
        Reserve(17, 0, 18446744069414584321n, 6);
        Check('rsp', ACCOUNTING + 16, -1); Check('rsp', ACCOUNTING + 24, 0);
        Request(12); Call(); Request(11); Call();
    });
    Case('physical-slot-exhaustion-consumes-budget', 2048,
        ({ Split, Childˉreserve, Append, Freeze, Request, Call, Emit, Set, Check, Snapshot, Sharedˉsnapshot }) => {
            // One runtime loop funds and constructs all64 live immutable backings.
            Set('rsp', 13328, 64); Emit('label Fill_shared_slots');
            Split(32); Childˉreserve(1); Append(2, 42); Freeze();
            Emit('load_memory_u32 eax rsp none 1 13328', 'subtract_i32 eax 1',
                'store_memory_u32 rsp none 1 13328 eax', 'branch not_equal Fill_shared_slots');
            Check('rsp', 84, 64); Check('rsp', 88, 2048); Check('rsp', 56, 2048);
            Check('rsp', ACCOUNTING + 36, 64); Check('rsp', ACCOUNTING + 48, 2048);
            Request(1, { Value: 1, Budget: 4294967297n }); Snapshot(false); Sharedˉsnapshot(false);
            Call(6, false); Check('rsp', REQUEST + 104, 32); Check('rsp', REQUEST + 112, 2048);
            Check('rsp', ACCOUNTING + 24, 1); Check('rsp', ACCOUNTING + 28, 0);
            Check('rsp', ACCOUNTING + 36, 64); Check('rsp', ACCOUNTING + 48, 2048);
            // Only the consumed root owner and declared typed-refusal fields may differ.
            Set('rsp', 29296 + 28, 0); Set('rsp', SAVED + 2112 + 104, 32);
            Set('rsp', SAVED + 2112 + 112, 2048); Snapshot(true); Sharedˉsnapshot(true);
            Request(12); Call(); Request(11); Call();
            Check('rsp', 84, 0); Check('rsp', 88, 0); Check('rsp', 60, 1);
            Check('rsp', ACCOUNTING + 24, 0); Check('rsp', ACCOUNTING + 48, 0);
            Check('rsp', SHARED + 32, 1);
            Emit('move_u32 ecx 64', 'label Shared_entries_zero',
                `load_memory_u64 rax rsp rcx 1 ${SHARED}`, 'test rax rax', 'branch not_equal Failed',
                'add_i32 ecx 8', 'compare_i32 ecx 2112', 'branch below Shared_entries_zero');
            // Logical teardown preserves every retired accounting generation.
            Emit(`move_u32 ecx ${ACCOUNTING + 56}`, 'label Shared_child_generations',
                'load_memory_u32 eax rsp rcx 1 0', 'compare_i32 eax 2', 'branch not_equal Failed',
                'load_memory_u32 eax rsp rcx 1 8', 'test eax eax', 'branch not_equal Failed',
                'add_i32 ecx 40', `compare_i32 ecx ${ACCOUNTING + 2616}`, 'branch below Shared_child_generations');
        });
    Case('teardown-outstanding-aliases', 96, ({ Split, Childˉreserve, Freeze, Request, Call, Save, Check, Emit }) => {
        Split(); Childˉreserve(17); Freeze(); Request(8, { Handle: 0 }); Call(); Request(8, { Handle: 0 }); Call();
        Split(); Request(1, { Value: 17 }); Emit('load_memory_u64 rax rsp none 1 13320',
            `store_memory_u64 rsp none 1 ${REQUEST + 32} rax`); Call(); Save(24, 2);
        Request(11); Call(); Check('rsp', 88, 0); Check('rsp', SHARED + 32, 1);
        Check('rsp', SHARED + 64, 0); Check('rsp', SHARED + 96, 0); Check('r12', 40, 1);
        Request(9, { Handle: 0 }); Call(9); Request(12); Call(9);
    });
    Case('decimal-boundaries', 80, ({ Reserve, Append, Freeze, Load, Check, Emit, Release }) => {
        const Values = [0n, 9n, 10n, 99n, 100n, 18446744073709551615n, 10000000000000000000n];
        const Expected = Buffer.from(Values.map(Value => Value.toString()).join(''), 'ascii');
        Reserve(Expected.length);
        for (const Value of Values) Append(5, Value);
        Check('rsp', REQUEST + 80, Expected.length); Append(2, 1, 0, 7); Freeze(); Load(1, 'rdi');
        for (let Offset = 0; Offset + 4 <= Expected.length; Offset += 4) Check('rdi', Offset, Expected.readUInt32LE(Offset));
        for (let Offset = Expected.length & ~3; Offset < Expected.length; Offset++) {
            Emit('xor eax eax', `load_memory_u8 al rdi none 1 ${Offset}`, `compare_i32 eax ${Expected[Offset]}`,
                'branch not_equal Failed');
        }
        Release(); Check('rsp', 88, 0);
    });
    Case('shared-source-range-refusals', 96, ({ Split, Childˉreserve, Append, Freeze, Request, Call, Emit, Save, Sharedˉappend, Check, Release }) => {
        Split(); Childˉreserve(17); Append(3, 0x44332211); Freeze();
        Split(); Request(1, { Value: 17 }); Emit('load_memory_u64 rax rsp none 1 13320',
            `store_memory_u64 rsp none 1 ${REQUEST + 32} rax`); Call(); Save(24, 2);
        Sharedˉappend(2, 0, 4, 1, 7); Sharedˉappend(2, 0, 5, 0, 7);
        Sharedˉappend(2, 0, 18446744073709551615n, 1, 7); Sharedˉappend(2, 2, 0, 1, 4);
        Sharedˉappend(2, 0, 4, 0); Check('rsp', REQUEST + 80, 0);
        Sharedˉappend(2, 0, 0, 4); Check('rsp', REQUEST + 80, 4);
        Release(0); Sharedˉappend(2, 0, 0, 1, 4); Release(2); Check('rsp', 88, 0);
    });
    Case('mixed-owner-domain', 96, ({ Split, Childˉreserve, Request, Call, Emit, Set, Check, Freeze }) => {
        // The lower domain may contain scalar/other owners outside this adapter.
        Split();
        Emit('call Request_reset'); Set('r13', 8, 1); Set('r13', 12, 17); Set('r13', 32, 16); Set('r13', 36, 17);
        Emit('load_memory_u64 rax rsp none 1 13320', 'store_memory_u64 r13 none 1 64 rax',
            'move r8 r12', 'move r9 r13', 'call Windvale_budgeted_storage', 'test eax eax', 'branch not_equal Failed');
        Request(12); Call(); Check('rsp', SHARED + 64, 0); Check('rsp', 88, 48);
        Split(); Childˉreserve(17); Freeze(); Check('rsp', SHARED + 64, 0); Check('rsp', SHARED + 96, 2);
        Request(11); Call(); Check('rsp', 88, 0); Check('rsp', ACCOUNTING + 48, 0);
    });
    for (const Iterations of [1, 1000, 32768]) Case(`fixed-live-${Iterations}`, 64,
        ({ Split, Childˉreserve, Append, Freeze, Release, Request, Call, Check, Emit, Set, Load }) => {
            Set('rsp', 13328, Iterations); Emit('label Shared_repeat');
            Split(); Childˉreserve(); Append(2, 42); Freeze(); Request(8, { Handle: 0 }); Call();
            Release(); Request(10, { Handle: 0, Length: 1 }); Call();
            Emit(`load_memory_u64 rdi rsp none 1 ${REQUEST + 72}`, 'xor eax eax', 'load_memory_u8 al rdi none 1 0',
                'compare_i32 eax 42', 'branch not_equal Failed');
            Release(); Check('rsp', 88, 0); Check('rsp', ACCOUNTING + 48, 0);
            Emit('load_memory_u32 eax rsp none 1 13328', 'subtract_i32 eax 1',
                'store_memory_u32 rsp none 1 13328 eax', 'branch not_equal Shared_repeat');
            Check('rsp', 56, 48); Check('rsp', 96, Iterations); Request(11); Call();
        });
    return Cases;
}
