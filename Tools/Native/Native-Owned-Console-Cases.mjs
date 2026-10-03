import { Acquireˉcurrentˉwvbˉpublisher } from './Current-Wvb-Publisher-Core.mjs';
import { Runˉdevelopmentˉcommand } from './Development-Command-Core.mjs';
import { Getˉcurrentˉsplitˉcompilerˉkey } from './Current-Split-Compiler-Cache-Core.mjs';
import { Readˉboundedˉhostedˉfile } from './Native-Hosted-Application-Cache-Core.mjs';
import { createHash } from 'node:crypto';
import { chmod, lstat, readFile, writeFile } from 'node:fs/promises';
import { basename, join } from 'node:path';

const RUNTIME = [
    'Linker/Startup/X64-Owned-Console-Entry.wva', 'Runtime/Native/X64-Owned-Domain.wva',
    'Runtime/Native/X64-Owned-Storage.wva', 'Runtime/Native/X64-Budgeted-Storage.wva',
    'Runtime/Native/X64-Memory-Budget-Validation.wva', 'Compiler/Native/Allocator/Descriptor-Allocator.wva',
];

function Require(Condition, Message) { if (!Condition) throw new Error(Message); }

function Template(Object) {
    Require(Object.length >= 49 && Object.toString('ascii', 0, 4) === 'WVO1' &&
        Object.readUInt32LE(12) === 1, 'Startup object shape differs.');
    const Length = Object.readUInt32LE(36), Nameˉlength = Object.readUInt32LE(40);
    const Start = 44 + Nameˉlength;
    Require(Start <= Object.length && Length <= Object.length - Start, 'Startup object extent differs.');
    return Object.subarray(Start, Start + Length);
}

function Scopeˉchanges(Input) {
    const Codeˉsize = new Map([[1,5],[2,2],[4,5],[5,5],[8,2],[9,5],[48,5],[49,5],
        [128,9],[129,9],[151,9],[152,9],[153,9],[196,9],[205,5],[206,9],[207,9]]);
    let Code = null;
    for (let Offset = 12; Offset < Input.length;) {
        const Length = Input.readUInt32LE(Offset + 4);
        Require(Length <= Input.length - Offset - 8, 'Scope section extent differs.');
        if (Input.readUInt32LE(Offset) === 5) Code = { Start: Offset + 8, Length };
        Offset += 8 + Length;
    }
    Require(Code !== null, 'Scope code is missing.');
    let Loop = null;
    const Limits = [];
    for (let Offset = Code.Start; Offset < Code.Start + Code.Length;) {
        if (Input[Offset] === 1 && Input.readUInt32LE(Offset + 1) === 1000) Loop = Offset + 1;
        if (Input[Offset] === 129) Limits.push(Offset + 1);
        Offset += Codeˉsize.get(Input[Offset]) ?? 1;
    }
    Require(Loop !== null && Limits.length === 2 && Input.readBigUInt64LE(Limits[0]) === 48n &&
        Input.readBigUInt64LE(Limits[1]) === 1n, 'Scope workload constants differ.');
    return { Loop, Budget: Limits[0], Capacity: Limits[1] };
}

function Audit(Status, Result) {
    return `windvale-assembly 1
symbol export function Native_main in .text
symbol import function Main
symbol import function Windvale_memory_budget_validate
section code .text align 16
define Native_main
push r12
push r13
subtract_i32 rsp 40
move r12 rdx
call Main
move r13 rax
move rax r13
shift_right rax 32
compare_i32 eax ${Status}
branch not_equal Failed
${Status === 0 ? `compare_i32 r13d ${Result}\nbranch not_equal Failed\n` : ''}load_memory_u64 rdx r12 none 1 112
load_memory_u32 eax rdx none 1 40
compare_i32 eax 1
branch not_equal Failed
load_memory_u64 r8 rdx none 1 16
load_memory_u32 eax r8 none 1 28
compare_i32 eax 1
branch not_equal Failed
load_memory_u64 rax r8 none 1 52
test rax rax
branch not_equal Failed
load_memory_u64 r8 rdx none 1 24
call Windvale_memory_budget_validate
test eax eax
branch not_equal Failed
load_memory_u64 rdx r12 none 1 112
load_memory_u64 rdx rdx none 1 24
move_u32 ecx 24
label Check_records
load_memory_u64 rax rdx rcx 1 0
test rax rax
branch not_equal Failed
add_i32 ecx 40
compare_i32 ecx 2616
branch below Check_records
move_u32 eax 42
jump_label Finish
label Failed
move_u32 eax 99
label Finish
add_i32 rsp 40
pop r13
pop r12
return
end define
end section
`;
}

function Fallback(Corrupt) {
    return `windvale-assembly 1
symbol export function Native_main in .text
symbol import function Windvale_budgeted_storage
section code .text align 16
define Native_main
push r12
subtract_i32 rsp 128
move r12 rdx
xor eax eax
move_u32 ecx 32
label Clear_request
store_memory_u64 rsp rcx 1 0 rax
add_i32 ecx 8
compare_i32 ecx 128
branch below Clear_request
move_u32 eax 1
store_memory_u32 rsp none 1 32 eax
store_memory_u32 rsp none 1 40 eax
store_memory_u64 rsp none 1 48 rax
move_u32 eax 96
store_memory_u32 rsp none 1 36 eax
move_u32 eax 17
store_memory_u32 rsp none 1 44 eax
store_memory_u32 rsp none 1 68 eax
move_u32 eax 16
store_memory_u32 rsp none 1 64 eax
move_u32 eax 1
shift_left rax 32
add_i32 rax 1
store_memory_u64 rsp none 1 96 rax
load_memory_u64 r8 r12 none 1 112
move r9 rsp
add_i32 r9 32
call Windvale_budgeted_storage
test eax eax
branch not_equal Failed
${Corrupt ? 'load_memory_u64 rdx r12 none 1 112\nmove_u32 eax 1\nstore_memory_u32 rdx none 1 40 eax\n' : ''}move_u32 eax 42
jump_label Finish
label Failed
move_u32 eax 99
label Finish
add_i32 rsp 128
pop r12
return
end define
end section
`;
}

function Mutations(Input, Windows) {
    const Startup = Windows ? 512 : 4096;
    const Native = Windows ? 144 : 208;
    const Delta = Windows ? 41 : 44;
    const Fields = Windows ? [10,17,32,47,62,77,99] : [71,78,93,108,123,138,160];
    const Data = Windows ? Input.readUInt32LE(452) : Number(Input.readBigUInt64LE(184));
    const Text = Windows ? 400 : 152;
    const Cases = [];
    const Byte = (Name, Offset) => Cases.push([Name, Value => { Value[Offset] ^= 1; return Value; }]);
    const Word = (Name, Offset, Number) => Cases.push([Name, Value => { Value.writeUInt32LE(Number, Offset); return Value; }]);
    Byte('magic', 1);
    Word('unknown-format', Windows ? 152 : 408, Windows ? 0x0004020b : 4);
    Byte('header-policy', Windows ? 428 : 124);
    Word('zero-native', Text, Native);
    Word('oversized-native', Text, Native + 4_194_305);
    Word('malicious-native', Text, 0xffffffff);
    Word('entry-before-image', Startup + Fields.at(-1), Delta - 1);
    Word('entry-at-end', Startup + Fields.at(-1), Delta + Input.readUInt32LE(Text) - Native);
    Byte('startup-opcode', Startup);
    for (const Field of Fields.slice(0,-1)) Byte('startup-address-' + Field, Startup + Field);
    Byte('startup-padding', Startup + Native - 1);
    for (const Field of [0,4,8,16,24,32,40,48,56,64,72,80,84,88,96,104]) Byte('request-' + Field, Data + Field);
    if (Windows) Byte('data-padding', Data + 112);
    for (const Length of [0,3,...(Windows ? [154] : [409,410,411]),Startup + Fields.at(-1) + 3,Data + 111,Input.length - 1]) {
        Cases.push(['truncate-' + Length, Value => Value.subarray(0, Length)]);
    }
    Cases.push(['trailing-byte', Value => Buffer.concat([Value, Buffer.from([0])])]);
    Cases.push(['oversized-container', Value => Buffer.concat([Value,
        Buffer.alloc((Windows ? 4_196_353 : 4_202_609) - Value.length)])]);
    return { Cases, Data, Fields, Native, Startup };
}

export async function Runˉownedˉconsoleˉcases(Repository, Work, Deadline, Prepareˉonly = false) {
    const Windows = process.platform === 'win32';
    const Extension = Windows ? '.exe' : '.elf';
    const Wrapper = Windows ? '.cmd' : '.sh';
    const Native = join(Repository, 'Tools/Native');
    const Target = Windows ? 'windows' : 'linux';
    let Cases = 0;
    const Pass = Name => { Cases += 1; process.stdout.write(`owned console item=${Cases} case=${Name} status=Passed\n`); };
    async function Run(Name, Tool, Arguments, Expected = 0) {
        if (process.env.WINDVALE_PREPARED_PRODUCTS_ONLY === '1' && Name.startsWith('publisher-')) {
            throw new Error('Prepared console publisher checkpoint missing; construction is forbidden.');
        }
        process.stdout.write(`owned console step=${Name} status=Started\n`);
        const Result = await Runˉdevelopmentˉcommand(Tool, Arguments, Deadline, Prepareˉonly,
            Name === 'compiler-prepare' ? 262_144 : 65_536);
        Require(Result.Code === Expected && Result.Error === '', `${Name} failed (${Result.Code}): ${Result.Error || Result.Output}`);
        return Result;
    }
    const Node = (Name, Leaf, Arguments) => Run(Name, process.execPath, [join(Native, Leaf), ...Arguments]);
    const Tool = (Name, Leaf, Arguments) => Run(Name, Windows ? join(Native, Leaf + Wrapper) : 'bash',
        Windows ? Arguments : [join(Native, Leaf + Wrapper), ...Arguments]);
    const Executeˉconstructed = async (Name, Path, Expected) => {
        // Construction writes bytes; publication supplies execution permissions.
        if (!Windows) await chmod(Path, 0o700);
        return Run(Name, Path, [], Expected);
    };
    if (Prepareˉonly) {
        await Node('compiler-prepare','Build-Current-Split-Project-Wvb.mjs',
            ['--prepare-only','--deadline-ms',String(Deadline)]);
    } else {
        const Invalidˉarguments = ['--owned-current','--maximum-seconds','0'];
        const Wrapperˉfailure = await Runˉdevelopmentˉcommand(Windows ?
            join(Native,'Test-Console-Packager-Source-Reconstruction.cmd') : 'bash',Windows ? Invalidˉarguments :
            [join(Native,'Test-Console-Packager-Source-Reconstruction.sh'),...Invalidˉarguments],Deadline,false);
        Require(Wrapperˉfailure.Code === 64 && Wrapperˉfailure.Error.startsWith('Usage: '),
            'The owner wrapper hid an argument failure.');
        Pass('wrapper-failure-propagation');
    }
    const Paths = ['Packager','Verifier','Publisher','Scope','Growth','Mutation','Fuel','Depth'].map(Name => join(Work, Name + '.wvb'));
    const Projects = [
        'Projects/Linker/Windvale-Console-Application-Packager.wvproj',
        'Projects/Tools/Windvale-Console-Application-Verifier.wvproj',
        'Projects/Tools/Windvale-Console-Application-Publisher.wvproj',
        'Projects/Tests/Windvale-Native-Test-Owned-Vector-Scope.wvproj',
        'Projects/Tests/Windvale-Native-Test-Owned-Vector-Growth.wvproj',
        'Projects/Tests/Windvale-Native-Test-Owned-Vector-Scalar-Mutation.wvproj',
        'Projects/Tests/Windvale-Native-Test-Owned-Console-Fuel-Trap.wvproj',
        'Projects/Tests/Windvale-Native-Test-Owned-Console-Depth-Trap.wvproj',
    ];
    await Node('current-source-build', 'Build-Current-Split-Project-Wvb.mjs', ['--prepared-compiler-only',
        '--deadline-ms', String(Deadline), ...Projects.flatMap((Project, Index) => [join(Repository, Project), Paths[Index]])]);
    const Packager = join(Work, 'Packager' + Extension), Verifier = join(Work, 'Verifier' + Extension);
    await Node('packager-materialize', 'Build-Cached-Segmented-Hosted-Wvb.mjs', ['--deadline-ms', String(Deadline),'6',Paths[0],Packager]);
    await Node('verifier-materialize', 'Build-Cached-Segmented-Hosted-Wvb.mjs', ['--deadline-ms', String(Deadline),'2',Paths[1],Verifier]);
    const Publisher = await Acquireˉcurrentˉwvbˉpublisher(Paths[2], join(Work, 'Publisher' + Extension),
        await Getˉcurrentˉsplitˉcompilerˉkey(), Tool, Node);
    if (Prepareˉonly) {
        const Lowerer = join(Work,'Lowerer.wvb');
        await Node('lowerer-source-build','Build-Current-Split-Project-Wvb.mjs',
            ['--prepared-compiler-only','--deadline-ms',String(Deadline),join(Repository,
                'Projects/Compiler/Windvale-Native-X64-Lowering-Tool.wvproj'),Lowerer]);
        await Node('lowerer-materialize','Build-Cached-Segmented-Hosted-Wvb.mjs',
            ['--deadline-ms',String(Deadline),'7',Lowerer,join(Work,'Lowerer' + Extension)]);
        process.stdout.write('owned console preparation status=Prepared behavior-cases=0\n');
        return 0;
    }
    const Cacheˉroot = process.env.WINDVALE_NATIVE_CACHE_ROOT;
    let Missingˉcheckpoint;
    try {
        process.env.WINDVALE_NATIVE_CACHE_ROOT = join(Work,'Missing-Checkpoint');
        Missingˉcheckpoint = await Runˉdevelopmentˉcommand(process.execPath,
            [join(Native,'Test-Console-Packager-Source-Reconstruction.mjs'),
                '--prepared-products-only','--maximum-seconds','60'],Deadline,false);
    } finally {
        if (Cacheˉroot === undefined) delete process.env.WINDVALE_NATIVE_CACHE_ROOT;
        else process.env.WINDVALE_NATIVE_CACHE_ROOT = Cacheˉroot;
    }
    Require(Missingˉcheckpoint.Code === 1 &&
        Missingˉcheckpoint.Error.includes('Current compiler checkpoint missing') &&
        !Missingˉcheckpoint.Output.includes('compiler-prepare'),
        'Prepared behavior reconstructed a missing compiler checkpoint.');
    Pass('prepared-checkpoint-miss');
    const Productˉmode = process.env.WINDVALE_PREPARED_PRODUCTS_ONLY;
    const Missingˉpublisher = join(Work,'Missing-Publisher' + Extension);
    let Publisherˉrefused = false;
    try {
        process.env.WINDVALE_NATIVE_CACHE_ROOT = join(Work,'Missing-Checkpoint');
        process.env.WINDVALE_PREPARED_PRODUCTS_ONLY = '1';
        await Acquireˉcurrentˉwvbˉpublisher(Paths[2],Missingˉpublisher,
            await Getˉcurrentˉsplitˉcompilerˉkey(),Tool,Node);
    } catch (Error) {
        Require(Error.message === 'Prepared console publisher checkpoint missing; construction is forbidden.',
            'Prepared publisher refusal differs: ' + Error.message);
        Publisherˉrefused = true;
    } finally {
        if (Cacheˉroot === undefined) delete process.env.WINDVALE_NATIVE_CACHE_ROOT;
        else process.env.WINDVALE_NATIVE_CACHE_ROOT = Cacheˉroot;
        if (Productˉmode === undefined) delete process.env.WINDVALE_PREPARED_PRODUCTS_ONLY;
        else process.env.WINDVALE_PREPARED_PRODUCTS_ONLY = Productˉmode;
    }
    Require(Publisherˉrefused && await lstat(Missingˉpublisher).catch(Error => {
        if (Error.code === 'ENOENT') return null;
        throw Error;
    }) === null, 'Prepared publisher refusal constructed an executable.');
    Pass('prepared-publisher-miss');
    const Empty = join(Work, 'Empty.bin');
    await writeFile(Empty, Buffer.alloc(0), { flag: 'wx' });
    const Objects = [];
    for (const [Index, Source] of RUNTIME.entries()) {
        const Object = join(Work, 'Runtime-' + Index + '.wvo');
        await Tool('assemble-' + basename(Source), 'Assemble-Wva', [join(Repository, Source), Object]);
        Objects.push(Object);
    }
    const Programs = [];
    for (const [Index, Name] of ['Scope','Growth','Mutation'].entries()) {
        const Object = join(Work, Name + '.wvo');
        await Node('lower-' + Name, 'Lower-Wvb-To-Wvo.mjs', ['--current',Paths[Index + 3],Object]);
        const Renamed = join(Work, Name + '-body.wvo');
        await Tool('rename-' + Name, 'Rename-Wvo-Export', [Object,'Main','Native_main',Renamed]);
        const Image = join(Work, Name + '.bin');
        const Map = await Tool('link-' + Name, 'Link-Wvo', ['0','Windvale_owned_console_entry',Image,...Objects,Renamed]);
        Require(/^entry name=Windvale_owned_console_entry address=0$/mu.test(Map.Output), 'Shared console entry moved.');
        Programs.push({ Name, Object, Image });
    }
    for (const Platform of ['windows','linux']) {
        const Output = join(Work, 'Scope-' + Platform + (Platform === 'windows' ? '.exe' : '.elf'));
        await Run('construct-' + Platform, Packager, [Platform + '-x64-console-v3',Programs[0].Image,'0',Output]);
        const Admitted = await Run('admit-' + Platform, Verifier, [Output,Empty]);
        Require(Admitted.Output.startsWith('console application status=Valid '), 'Valid owned console refused.');
        Pass('construct-admit-' + Platform);
        const Input = await readFile(Output);
        const Shape = Mutations(Input, Platform === 'windows');
        const Startupˉobject = join(Work, Platform + '-startup.wvo');
        await Tool('startup-source-' + Platform, 'Assemble-Wva', [join(Repository,'Linker/Startup',
            (Platform === 'windows' ? 'Windows' : 'Linux') + '-X64-Owned-Console.wva'),Startupˉobject]);
        const Source = Template(await readFile(Startupˉobject));
        const Encoded = Buffer.from(Input.subarray(Shape.Startup,Shape.Startup + Source.length));
        const Dataˉaddress = Platform === 'windows' ? Input.readUInt32LE(444) : Number(Input.readBigUInt64LE(192));
        for (const [Index, Field] of Shape.Fields.entries()) {
            const Expected = Index === 6 ? 4096 + Shape.Native : Dataˉaddress + [0,112,256,2368,4992,6080][Index];
            Require(4096 + Field + 4 + Encoded.readInt32LE(Field) === Expected, 'Owned startup pointer placement differs.');
            Encoded.fill(0,Field,Field + 4);
        }
        Require(Encoded.equals(Source), 'Owned startup differs from its assembled source.');
        Pass('startup-source-and-addresses-' + Platform);
        const Repeat = Output + '.repeat';
        await Run('determinism-' + Platform, Packager, [Platform + '-x64-console-v3',Programs[0].Image,'0',Repeat]);
        Require(Input.equals(await readFile(Repeat)), 'Owned container construction is not deterministic.');
        Pass('determinism-' + Platform);
        const Alternate = Buffer.from(Input);
        Alternate.writeUInt32LE((Platform === 'windows' ? 41 : 44) + 1, Shape.Startup + Shape.Fields.at(-1));
        await writeFile(Repeat,Alternate);
        const Alternateˉresult = await Run('alternate-entry-' + Platform,Verifier,[Repeat,Empty]);
        Require(Alternateˉresult.Output.includes(' entry=1'), 'In-range opaque native entry was refused.');
        Pass('alternate-entry-' + Platform);
        for (const [Name, Change] of Shape.Cases) {
            const Changed = Change(Buffer.from(Input));
            const First = join(Work, 'Mutation-first.bin'), Second = join(Work, 'Mutation-second.bin');
            await writeFile(First, Changed.subarray(0,4_194_304));
            await writeFile(Second, Changed.subarray(4_194_304));
            const Result = await Runˉdevelopmentˉcommand(Verifier,[First,Second],Deadline,false);
            Require(Result.Code === 1 && Result.Error.startsWith('console application status=Rejected '),
                `Owned ${Platform} malformed ${Name} was not rejected: ${Result.Output}${Result.Error}`);
            Pass('reject-' + Platform + '-' + Name);
        }
        const Bad = join(Work,'Bad-' + Platform + (Platform === 'windows' ? '.exe' : '.elf'));
        const Corrupt = Buffer.from(Input); Corrupt[Shape.Startup] ^= 1;
        await writeFile(Bad,Corrupt);
        const Destination = join(Work,'Sentinel-' + Platform + (Platform === 'windows' ? '.exe' : '.elf'));
        const Sentinel = Buffer.from([1,7,3,9]); await writeFile(Destination,Sentinel);
        const Rejected = await Runˉdevelopmentˉcommand(Publisher.Path,[Bad,Destination],Deadline,false);
        Require(Rejected.Code !== 0 && Sentinel.equals(await readFile(Destination)), 'Rejected publication changed the destination.');
        Pass('publication-refusal-' + Platform);
        await Run('publication-' + Platform,Publisher.Path,[Output,Destination]);
        Require(Input.equals(await readFile(Destination)), 'Valid publication bytes differ.');
        Pass('publication-complete-' + Platform);
    }
    const Ordinary = join(Work,'Ordinary' + Extension);
    await Tool('normal-command','Package-Console',['--owned',Target,Programs[0].Object,Ordinary]);
    await Run('normal-execution',Ordinary,[],42); Pass('normal-command-execution');
    for (const Program of Programs.slice(1)) {
        const Output = join(Work,Program.Name + Extension);
        await Run('construct-' + Program.Name,Packager,[Target + '-x64-console-v3',Program.Image,'0',Output]);
        await Executeˉconstructed('execute-' + Program.Name,Output,42); Pass('compiled-' + Program.Name);
    }
    const Scope = await readFile(Paths[3]), Changes = Scopeˉchanges(Scope);
    const Auditˉcases = [
        { Name:'success-release',Status:0,Result:42 },
        { Name:'allocation-refusal',Status:0,Result:1,Change:Value => Value.writeBigUInt64LE(16n,Changes.Budget) },
        { Name:'invalid-limit-trap',Status:5,Change:Value => Value.writeBigUInt64LE(0n,Changes.Capacity) },
        { Name:'fuel-trap',Status:2,Path:Paths[6] },
        { Name:'depth-trap',Status:3,Path:Paths[7] },
    ];
    for (const Case of Auditˉcases) {
        const Prefix = join(Work,Case.Name), Value = Buffer.from(Case.Path ? await readFile(Case.Path) : Scope); Case.Change?.(Value);
        await writeFile(Prefix + '.wvb',Value);
        await Node('lower-' + Case.Name,'Lower-Wvb-To-Wvo.mjs',['--current',Prefix + '.wvb',Prefix + '.wvo']);
        await writeFile(Prefix + '.wva',Audit(Case.Status,Case.Result));
        await Tool('audit-' + Case.Name,'Assemble-Wva',[Prefix + '.wva',Prefix + '-audit.wvo']);
        await Tool('link-' + Case.Name,'Link-Wvo',['0','Windvale_owned_console_entry',Prefix + '.bin',...Objects,Prefix + '-audit.wvo',Prefix + '.wvo']);
        await Run('construct-' + Case.Name,Packager,[Target + '-x64-console-v3',Prefix + '.bin','0',Prefix + Extension]);
        await Executeˉconstructed('execute-' + Case.Name,Prefix + Extension,42); Pass('audit-' + Case.Name);
    }
    for (const Corrupt of [false,true]) {
        const Name = Corrupt ? 'cleanup-refusal' : 'outer-domain-release', Prefix = join(Work,Name);
        await writeFile(Prefix + '.wva',Fallback(Corrupt));
        await Tool('assemble-' + Name,'Assemble-Wva',[Prefix + '.wva',Prefix + '.wvo']);
        await Tool('link-' + Name,'Link-Wvo',['0','Windvale_owned_console_entry',Prefix + '.bin',...Objects,Prefix + '.wvo']);
        await Run('construct-' + Name,Packager,[Target + '-x64-console-v3',Prefix + '.bin','0',Prefix + Extension]);
        await Executeˉconstructed('execute-' + Name,Prefix + Extension,Corrupt ? 1 : 42); Pass(Name);
    }
    const Tiny = join(Work,'Return-42.bin'); await writeFile(Tiny,Buffer.from([0xb8,42,0,0,0,0xc3]));
    const Legacy = join(Repository,'Artifacts/Native-Console-Packager-Candidate/Console-Packager' + Extension);
    for (const Platform of ['windows','linux']) {
        const Old = join(Work,'Old-' + Platform + '.bin'), Current = join(Work,'Current-' + Platform + '.bin');
        await Run('legacy-' + Platform,Legacy,[Platform + '-x64-console-v1',Tiny,'0',Old]);
        await Run('current-legacy-' + Platform,Packager,[Platform + '-x64-console-v1',Tiny,'0',Current]);
        Require((await readFile(Old)).equals(await readFile(Current)), 'Legacy container bytes changed.');
        await Run('admit-legacy-' + Platform,Verifier,[Current,Empty]); Pass('legacy-bytes-' + Platform);
    }
    const Encoded = await Readˉboundedˉhostedˉfile(join(Repository,
        'Tests/Native/Hosted-Console-Container-Mutations/Corpus.tar.gz.b64'),'hosted console corpus',1_048_576);
    const Archive = Buffer.from(Encoded.toString('ascii').replaceAll('\n',''),'base64');
    Require(createHash('sha256').update(Archive).digest('hex') ===
        'a8027a9d4238767ae9b7ab18e3d0114da4e4fdf3edcbbc044d4358f2ce1fd055', 'Hosted corpus identity differs.');
    const Archiveˉpath = join(Work,'Hosted-Corpus.tar.gz'); await writeFile(Archiveˉpath,Archive,{ flag:'wx' });
    await Run('hosted-positive-fixtures','tar',['-xzf',Archiveˉpath,'-C',Work,'Windows-Valid.exe','Linux-Valid.elf']);
    for (const [Name,Digest] of [
        ['Windows-Valid.exe','0f59222c33828d65a086de9f2b3eb22f00fc3b8c69cf7262a19b9e8df8b4f4e0'],
        ['Linux-Valid.elf','7ad022f26e24949ddb7a4b1cb7681e7edc24c573e501c64d92a8f0c9b4bca1fd'],
    ]) {
        const Path = join(Work,Name);
        Require(createHash('sha256').update(await Readˉboundedˉhostedˉfile(Path,'hosted positive fixture',10_000))
            .digest('hex') === Digest, 'Hosted positive fixture identity differs.');
        const Admitted = await Run('admit-hosted-' + Name,Verifier,[Path,Empty]);
        Require(Admitted.Output.startsWith('console application status=Valid '), 'Hosted format 2 admission changed.');
        Pass('hosted-format-2-' + Name);
    }
    process.stdout.write(`owned console source reconstruction status=Passed cases=${Cases} host=${Target} profile=3 abi=24\n`);
    return Cases;
}
