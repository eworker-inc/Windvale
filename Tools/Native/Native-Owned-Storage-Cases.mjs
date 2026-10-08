import { Buildˉstorageˉfixture } from './Native-Storage-Fixture.mjs';
import { Buildˉownedˉdomainˉcases, Buildˉownedˉentryˉcases } from './Native-Owned-Domain-Cases.mjs';
import { Buildˉbudgetedˉstorageˉcases, Readˉbudgetˉoracle } from './Native-Budgeted-Storage-Cases.mjs';
import { Buildˉsharedˉstorageˉcases } from './Native-Shared-Storage-Cases.mjs';
import { Buildˉsharedˉvalueˉcases, Checkˉsharedˉvalueˉtemplate } from './Native-Shared-Value-Cases.mjs';
import { Prepareˉassemblyˉobjectˉcache, Acquireˉassemblyˉobject } from './Native-Assembly-Object-Cache-Core.mjs';
import { readFile, writeFile } from 'node:fs/promises';
import { join } from 'node:path';

// Extends the existing native owner. The accounting oracle uses a prepared
// compiler or an explicitly supplied, digest-checked WVB product.
export async function Prepareˉownedˉstorageˉobjects(Context) {
    const { Repository, Work, Target, Requireˉsuccess } = Context;
    const Extension = Target === 'windows' ? 'cmd' : 'sh';
    const Tool = Name => join(Repository, 'Tools', 'Native', `${Name}.${Extension}`);
    // Focused runtime checks reuse admitted objects. The complete owner still
    // runs its independent repeated assembly and byte comparison below.
    const Cache = Context.Assemblyˉcache ??
        (Context.Valuesˉonly ? await Prepareˉassemblyˉobjectˉcache(Context.Deadline) : null);
    const Objects = [];
    for (const [Name, Source] of [
        ['Owned', 'Runtime/Native/X64-Owned-Storage.wva'],
        ['Allocator', 'Compiler/Native/Allocator/Descriptor-Allocator.wva'],
        ['Budget-Validation', 'Runtime/Native/X64-Memory-Budget-Validation.wva'],
        ['Budgeted', 'Runtime/Native/X64-Budgeted-Storage.wva'],
        ...(Context.Sharedˉonly ? [] : [['Domain', 'Runtime/Native/X64-Owned-Domain.wva']]),
        ['Shared', 'Runtime/Native/X64-Shared-Storage.wva'],
        ...(!Context.Sharedˉonly && !Context.Domainˉonly ? [['Value', 'Runtime/Native/X64-Shared-Value-Operations.wva']] : []),
        ...(Context.Sharedˉonly || Context.Valuesˉonly ? [] : [['Entry', 'Runtime/Native/X64-Owned-Entry.wva']]),
    ]) {
        process.stdout.write(`native owned storage step=assemble leaf=${Name}\n`);
        const Object = join(Work, `${Name}.wvo`);
        if (Cache !== null) {
            await Acquireˉassemblyˉobject(Cache, join(Repository, Source), Object);
            Objects.push(Object);
            continue;
        }
        const Repeat = join(Work, `${Name}-repeat.wvo`);
        await Requireˉsuccess(Tool('Assemble-Wva'), [join(Repository, Source), Object], `owned-${Name}-assemble`);
        await Requireˉsuccess(Tool('Assemble-Wva'), [join(Repository, Source), Repeat], `owned-${Name}-repeat`);
        if (!(await readFile(Object)).equals(await readFile(Repeat))) {
            throw new Error(`Owned storage ${Name} assembly is not deterministic.`);
        }
        await Requireˉsuccess(Tool('Check-Wvo'), [Object], `owned-${Name}-validate`);
        Objects.push(Object);
    }
    if (Objects.includes(join(Work, 'Value.wvo'))) {
        await Checkˉsharedˉvalueˉtemplate(Context, join(Work, 'Value.wvo'));
    }
    return Objects;
}

export async function Runˉownedˉstorageˉcases(Context) {
    const { Repository, Work, Target, Requireˉsuccess, Runˉprocess } = Context;
    const Extension = Target === 'windows' ? 'cmd' : 'sh';
    const Tool = Name => join(Repository, 'Tools', 'Native', `${Name}.${Extension}`);
    const Cache = Context.Storageˉonly || Context.Valuesˉonly
        ? await Prepareˉassemblyˉobjectˉcache(Context.Deadline) : null;
    const Objects = await Prepareˉownedˉstorageˉobjects({ ...Context,
        Assemblyˉcache: Cache, Includeˉshared: !Context.Domainˉonly });
    const Oracle = Context.Domainˉonly || Context.Sharedˉonly || Context.Valuesˉonly ? null : await Readˉbudgetˉoracle(Context);
    const Sharedˉcases = Context.Domainˉonly ? [] : Buildˉsharedˉstorageˉcases();
    const Budgetedˉcases = Context.Domainˉonly || Context.Sharedˉonly || Context.Valuesˉonly
        ? [] : Buildˉbudgetedˉstorageˉcases(Oracle);
    const Mappedˉcases = Sharedˉcases.filter(Case => Case.Name.startsWith('shared-mapped-')).length;
    const Cases = Context.Valuesˉonly ? Buildˉsharedˉvalueˉcases() : Context.Sharedˉonly ? Sharedˉcases :
        [...(Context.Domainˉonly ? [] : [...Buildˉcases(), ...Budgetedˉcases,
            ...Sharedˉcases, ...Buildˉsharedˉvalueˉcases()]), ...Buildˉownedˉdomainˉcases(), ...Buildˉownedˉentryˉcases()];
    // Two independent fixtures bound concurrent tool memory. Drain both workers
    // on failure before the owner removes their shared temporary directory.
    let Next = 0;
    let Failure = null;
    let Packageˉtail = Promise.resolve();
    async function Package(Case, Image, Entry, Application) {
        const Produce = () => Requireˉsuccess(Tool('Package-Console'),
            [`${Target}-x64-console-v1`, Image, Entry, Application], `owned-${Case.Name}-package`);
        if (Target !== 'windows') return Produce();
        // The pinned Windows wrapper seeds its private path from CMD RANDOM.
        // Concurrent invocations in one clock tick can choose the same path.
        const Result = Packageˉtail.then(Produce);
        Packageˉtail = Result.catch(() => {});
        return Result;
    }
    async function Worker() {
        while (Failure === null && Next < Cases.length) {
            const Index = Next++, Case = Cases[Index];
            try {
                process.stdout.write(`native owned storage step=${Context.Prepareˉstorage ? 'prepare' : 'execute'} item=${Index + 1}/${Cases.length} case=${Case.Name}\n`);
                const Prefix = join(Work, `Owned-${Case.Name}`);
                if (Buffer.byteLength(Case.Source) > 131_072) throw new Error('Owned storage fixture exceeds its source limit.');
                const Source = Prefix + '.wva', Object = Prefix + '.wvo', Image = Prefix + '.bin';
                const Application = Prefix + (Target === 'windows' ? '.exe' : '.elf');
                await writeFile(Source, Case.Source);
                if (Cache !== null) {
                    await Acquireˉassemblyˉobject(Cache, Source, Object);
                } else {
                    await Requireˉsuccess(Tool('Assemble-Wva'), [Source, Object], `owned-${Case.Name}-assemble`);
                }
                if (Context.Prepareˉstorage) {
                    process.stdout.write(`native storage products step=prepared item=${Index + 1}/${Cases.length} case=${Case.Name}\n`);
                    continue;
                }
                const Providers = Objects.filter(Path =>
                    (Path !== join(Work, 'Entry.wvo') || Case.ExtraRuntime === 'Entry') &&
                    (Path !== join(Work, 'Value.wvo') || Case.ExtraRuntime === 'Value') &&
                    !(Case.Substituteˉbudgeted && Path === join(Work, 'Budgeted.wvo')) &&
                    !(Case.Substituteˉshared && Path === join(Work, 'Shared.wvo')));
                const Linked = await Requireˉsuccess(Tool('Link-Wvo'), ['0', 'Main', Image, Object, ...Providers], `owned-${Case.Name}-link`);
                const Entry = /^entry name=Main address=([0-9]+)$/mu.exec(Linked.Output);
                if (Entry === null) throw new Error('Owned storage test entry point is missing.');
                await Package(Case, Image, Entry[1], Application);
                const Start = performance.now();
                const Result = await Runˉprocess(Application, [], 30_000, `owned-${Case.Name}-execute`);
                if (Result.Code !== 42 || Result.Exceeded || Result.Timedˉout || Result.Output !== '') {
                    throw new Error(`Owned storage ${Case.Name} failed: code=${Result.Code}, output=${Result.Output}.`);
                }
                process.stdout.write(`native owned storage case=${Case.Name} status=Passed elapsed-ms=${Math.round(performance.now() - Start)}\n`);
            } catch (Error) { Failure ??= Error; }
        }
    }
    await Promise.all([Worker(), Worker()]);
    if (Failure !== null) throw Failure;
    if (Cache !== null) await Cache.Requireˉunchanged();
    if (Context.Prepareˉstorage) {
        process.stdout.write(`native storage products status=Prepared target=${Context.Sharedˉonly ? 'shared' : 'owned'} runtime-objects=${Objects.length} fixture-objects=${Cases.length} behavior-cases=0 workers=2 qualification=false\n`);
    } else if (Context.Valuesˉonly) {
        process.stdout.write(`native shared values status=Passed cases=${Cases.length} iterations=1,1000,32768 arena-bytes=64 peak-physical-charge=32 final-physical-charge=0 final-parent-available=48 metadata-bytes=8120 workers=2 generated-source=false qualification=false\n`);
    } else if (Context.Sharedˉonly) {
        process.stdout.write(`native shared storage status=Passed cases=${Cases.length} slots=64 state-bytes=2112 request-bytes=128 iterations=1,1000,32768 stress-arena=64 stress-peak-charge=48 metadata-bytes=7928 mapped-v2-cases=${Mappedˉcases} mapped-slots=2 v2-state-bytes=2176 v2-metadata-bytes=8120 workers=2 qualification=false\n`);
    } else if (Context.Domainˉonly) {
        process.stdout.write(`native owned domain status=Passed cases=${Cases.length} v1-cases=7 metadata-bytes=5952 request-bytes=112 pairs=15 snapshot-bytes=6144 v2-cases=9 v2-entry-cases=9 v2-metadata-bytes=8120 v2-request-bytes=192 v2-context-bytes=192 v2-pairs=45 workers=2 qualification=false\n`);
    } else {
        process.stdout.write(`native owned storage status=Passed cases=${Cases.length} slots=64 state-bytes=2112 budgeted-cases=${Budgetedˉcases.length} domain-cases=25 domain-v2-cases=9 domain-v2-entry-cases=9 shared-storage-cases=${Sharedˉcases.length} shared-value-cases=10 mapped-v2-cases=${Mappedˉcases} accounting-states=14 budgeted-metadata-bytes=5816 shared-metadata-bytes=7928 v2-metadata-bytes=8120 stress-iterations=32768 stress-arena=64 stress-peak-charge=48 replacement-iterations=1000 replacement-arena=112 replacement-peak-charge=112 workers=2\n`);
    }
    return Cases.length;
}

function Buildˉcases() {
    const Cases = [];
    const Case = (...Arguments) => Cases.push(Buildˉstorageˉfixture(...Arguments));

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
    Case('live-interval-order', 4096, ({ Emit, Set, Check, Request, Call, Reserve }) => {
        Emit('move_u32 eax 64', 'store_memory_u32 rsp none 1 13000 eax', 'label Fill_intervals');
        Reserve(1);
        Emit('load_memory_u32 eax rsp none 1 13000', 'subtract_i32 eax 1',
            'store_memory_u32 rsp none 1 13000 eax', 'branch not_equal Fill_intervals');
        // Slot order is independent of physical address order. Equal capacities
        // let these private valid-state fixtures isolate the interval proof.
        Request(6); Call();
        for (const Address of [Index => (Index * 17 + 31) % 64, Index => 63 - Index]) {
            for (let Index = 0; Index < 64; Index++) Set('r12', 68 + Index * 32, Address(Index) * 32 + 1);
            Request(6); Call();
        }
        Set('r12', 2084, 2017); Request(6); Call(5); Set('r12', 2084, 1);
        Request(5); Call(); Check('r12', 52, 0); Check('r12', 56, 0); Check('r14', 0, 4096);
    });
    Case('interleaved-free-intervals', 3088, ({ Set, Check, Request, Call }) => {
        // The maximum 65 free nodes alternate with 64 live intervals; all
        // touching boundaries are valid and their total covers the whole arena.
        for (let Index = 0; Index < 64; Index++) {
            const Slot = 64 + Index * 32, Header = 16 + Index * 48;
            for (const [Offset, Value] of [[0, 1], [4, Header + 1], [8, 1], [12, 1], [16, 32], [20, 16]])
                Set('r12', Slot + Offset, Value);
            for (const [Offset, Value] of [[0, 32], [4, 1], [8, 0], [12, 1279350359]])
                Set('r14', Header + Offset, Value);
        }
        for (let Index = 0; Index <= 64; Index++) {
            for (const [Offset, Value] of [[0, 16], [4, 0], [8, Index < 64 ? (Index + 1) * 48 + 1 : 0], [12, 1380341335]])
                Set('r14', Index * 48 + Offset, Value);
        }
        Set('r12', 24, 2048); Set('r12', 52, 64); Set('r12', 56, 2048);
        Request(6); Call();
        Set('r14', 0, 64); Request(6); Call(5); Set('r14', 0, 16);
        Set('r14', 8, 97); Request(6); Call(5); Set('r14', 8, 49);
        Request(5); Call(); Check('r12', 52, 0); Check('r12', 56, 0); Check('r14', 0, 3088);
    });
    Case('forged-live-header', 144, ({ Set, Request, Call, Reserve }) => {
        Reserve(64); Reserve(32, 2, 3);
        // A valid-looking second header inside the first live payload must
        // still fail, without changing the corrupt state or any arena byte.
        for (const [Offset, Value] of [[0, 48], [4, 1], [8, 0], [12, 1279350359]])
            Set('r14', 32 + Offset, Value);
        Set('r12', 100, 33); Request(6); Call(5); Set('r12', 100, 81);
        Request(6); Call(); Request(5); Call();
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
