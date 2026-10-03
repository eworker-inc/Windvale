import { mkdtemp, realpath, rm, writeFile } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { basename, dirname, extname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { Runˉdevelopmentˉcommand } from './Development-Command-Core.mjs';
import { Acquireˉcurrentˉwvbˉpublisher } from './Current-Wvb-Publisher-Core.mjs';
import { Prepareˉassemblyˉobjectˉcache, Acquireˉassemblyˉobject } from './Native-Assembly-Object-Cache-Core.mjs';
import { Isˉsameˉhostedˉpath, Readˉboundedˉhostedˉfile } from './Native-Hosted-Application-Cache-Core.mjs';
import {
    Getˉcurrentˉsplitˉcompilerˉfamily, Getˉcurrentˉsplitˉcompilerˉkey,
    Readˉpreparedˉsplitˉcompiler,
} from './Current-Split-Compiler-Cache-Core.mjs';
import {
    Prepareˉnativeˉprojectˉcacheˉcontext, Getˉnativeˉprojectˉcacheˉrequest,
    Requireˉnativeˉprojectˉcacheˉrequestˉunchanged,
} from './Native-Project-Cache-Key-Core.mjs';

const NATIVE = dirname(fileURLToPath(import.meta.url));
const REPOSITORY = resolve(NATIVE, '..', '..');
const WINDOWS = process.platform === 'win32';
const WRAPPER = WINDOWS ? '.cmd' : '.sh';
const HOST_EXTENSION = WINDOWS ? '.exe' : '.elf';
const MAXIMUM_NATIVE_BYTES = 4_194_304;
const CLEANUP_MILLISECONDS = 30_000;
const RUNTIME_SOURCES = Object.freeze([
    'Linker/Startup/X64-Owned-Console-Entry.wva',
    'Runtime/Native/X64-Owned-Domain.wva',
    'Runtime/Native/X64-Owned-Storage.wva',
    'Runtime/Native/X64-Budgeted-Storage.wva',
    'Runtime/Native/X64-Memory-Budget-Validation.wva',
    'Compiler/Native/Allocator/Descriptor-Allocator.wva',
]);
const PACKAGER_PROJECT = join(REPOSITORY, 'Projects/Linker/Windvale-Console-Application-Packager.wvproj');
const PUBLISHER_PROJECT = join(REPOSITORY, 'Projects/Tools/Windvale-Console-Application-Publisher.wvproj');

function Usage() {
    throw Object.assign(new Error('Usage: Package-Console.mjs [--maximum-seconds <60..1800>] ' +
        '--owned <windows|linux> <abi-24-main.wvo> <output.exe|elf>\n' +
        '       Package-Console.mjs [--maximum-seconds <60..1800>] --current ' +
        '<windows-x64-console-v1|linux-x64-console-v1|windows-x64-console-v3|linux-x64-console-v3> ' +
        '<native-image.bin> <entry-offset> <output.exe|elf>'), { exitCode: 64 });
}

// Host orchestration only. Windvale owns the executable bytes and admission;
// the existing native transaction owns destination mutation.
async function Main() {
    const Arguments = process.argv.slice(2);
    let Maximumˉseconds = 600;
    if (Arguments[0] === '--maximum-seconds') {
        if (!/^[1-9][0-9]*$/u.test(Arguments[1] ?? '')) Usage();
        Maximumˉseconds = Number(Arguments[1]);
        if (!Number.isSafeInteger(Maximumˉseconds) || Maximumˉseconds < 60 || Maximumˉseconds > 1800) Usage();
        Arguments.splice(0, 2);
    }
    const Owned = Arguments[0] === '--owned';
    if ((!Owned && Arguments[0] !== '--current') || Arguments.length !== (Owned ? 4 : 5) ||
        process.arch !== 'x64' || !['win32', 'linux'].includes(process.platform)) Usage();
    let Target = Arguments[1];
    let Entry = Owned ? '0' : Arguments[3];
    if (Owned) {
        if (!['windows', 'linux'].includes(Target)) Usage();
        Target += '-x64-console-v3';
    }
    if (!/^(windows|linux)-x64-console-v[13]$/u.test(Target) ||
        !/^(0|[1-9][0-9]*)$/u.test(Entry) || !Number.isSafeInteger(Number(Entry))) Usage();
    const Input = resolve(Arguments[2]);
    const Output = resolve(Arguments[Owned ? 3 : 4]);
    const Outputˉextension = Target.startsWith('windows-') ? '.exe' : '.elf';
    if (extname(Input).toLowerCase() !== (Owned ? '.wvo' : '.bin') ||
        extname(Output) !== Outputˉextension || Isˉsameˉhostedˉpath(Input, Output)) Usage();
    const Parent = dirname(Output);
    if (!Isˉsameˉhostedˉpath(await realpath(Parent), Parent)) {
        throw new Error('The console output parent must use its canonical path.');
    }
    const Payload = await Readˉboundedˉhostedˉfile(Input, 'console native input', MAXIMUM_NATIVE_BYTES);
    if (!Owned && Number(Entry) >= Payload.length) throw new Error('The console entry exceeds the native image.');
    const Deadline = Date.now() + Maximumˉseconds * 1000;
    const Workˉdeadline = Deadline - CLEANUP_MILLISECONDS;
    const Key = await Getˉcurrentˉsplitˉcompilerˉkey();
    const Compiler = await Readˉpreparedˉsplitˉcompiler(await Getˉcurrentˉsplitˉcompilerˉfamily(), Key);
    const Context = await Prepareˉnativeˉprojectˉcacheˉcontext('current-owned-console-v1', [
        fileURLToPath(import.meta.url),
        ...RUNTIME_SOURCES.map(Name => join(REPOSITORY, Name)),
        ...['Windows-X64-Owned-Console', 'Linux-X64-Owned-Console'].map(Name =>
            join(REPOSITORY, 'Linker/Startup', Name + '.wva')),
        ...['Assemble-Wva', 'Rename-Wvo-Export', 'Link-Wvo'].map(Name => join(NATIVE, Name + WRAPPER)),
        join(NATIVE, 'Current-Wvb-Publisher-Core.mjs'),
        join(NATIVE, 'Native-Assembly-Object-Cache-Core.mjs'),
        join(NATIVE, 'Build-Cached-Segmented-Hosted-Wvb.mjs'),
    ]);
    const Requests = await Promise.all([PACKAGER_PROJECT, PUBLISHER_PROJECT].map(Project =>
        Getˉnativeˉprojectˉcacheˉrequest(Context, Project)));
    const Assemblyˉcontext = Owned ? await Prepareˉassemblyˉobjectˉcache(Workˉdeadline) : null;
    const Assemblyˉrequests = [];
    const Requireˉunchanged = async () => {
        await Promise.all(Requests.map(Requireˉnativeˉprojectˉcacheˉrequestˉunchanged));
        await Promise.all(Assemblyˉrequests.map(Request => Request.Requireˉunchanged()));
        if (Assemblyˉcontext !== null) await Assemblyˉcontext.Requireˉunchanged();
        await Compiler.Requireˉunchanged();
        if (await Getˉcurrentˉsplitˉcompilerˉkey() !== Key ||
            !Payload.equals(await Readˉboundedˉhostedˉfile(Input, 'console native input', MAXIMUM_NATIVE_BYTES))) {
            throw new Error('Console construction inputs changed.');
        }
    };
    const Temporary = await realpath(tmpdir());
    const Work = await realpath(await mkdtemp(join(Temporary, 'windvale-current-console-')));
    async function Run(Step, Tool, Parameters) {
        if (process.env.WINDVALE_PREPARED_PRODUCTS_ONLY === '1' && Step.startsWith('publisher-')) {
            throw new Error('Prepared console publisher checkpoint missing; construction is forbidden.');
        }
        process.stdout.write(`current console step=${Step} status=Started\n`);
        const Result = await Runˉdevelopmentˉcommand(Tool, Parameters, Workˉdeadline, true);
        if (Result.Code !== 0 || Result.Error !== '') {
            throw new Error(`Console ${Step} failed (${Result.Code}): ${Result.Error}`);
        }
        process.stdout.write(`current console step=${Step} status=Complete\n`);
        return Result;
    }
    const Runˉnode = (Step, Name, Parameters) => Run(Step, process.execPath, [join(NATIVE, Name), ...Parameters]);
    const Runˉnative = (Step, Name, Parameters) => {
        const Tool = join(NATIVE, Name + WRAPPER);
        return Run(Step, WINDOWS ? Tool : 'bash', WINDOWS ? Parameters : [Tool, ...Parameters]);
    };
    let Publicationˉstarted = false;
    let Publicationˉcomplete = false;
    try {
        const Packagerˉwvb = join(Work, 'Packager.wvb');
        const Publisherˉwvb = join(Work, 'Publisher.wvb');
        await Runˉnode('source-build', 'Build-Current-Split-Project-Wvb.mjs', [
            '--prepared-compiler-only', '--deadline-ms', String(Workˉdeadline),
            PACKAGER_PROJECT, Packagerˉwvb, PUBLISHER_PROJECT, Publisherˉwvb,
        ]);
        const Packager = join(Work, 'Packager' + HOST_EXTENSION);
        await Runˉnode('packager-materialize', 'Build-Cached-Segmented-Hosted-Wvb.mjs', [
            '--deadline-ms', String(Workˉdeadline), '6', Packagerˉwvb, Packager,
        ]);
        const Publisher = await Acquireˉcurrentˉwvbˉpublisher(Publisherˉwvb,
            join(Work, 'Publisher' + HOST_EXTENSION), Key, Runˉnative, Runˉnode);
        let Image = join(Work, 'Input.bin');
        if (Owned) {
            const Object = join(Work, 'Input.wvo');
            const Renamed = join(Work, 'Body.wvo');
            await writeFile(Object, Payload, { flag: 'wx' });
            await Runˉnative('rename-main', 'Rename-Wvo-Export', [Object, 'Main', 'Native_main', Renamed]);
            const Objects = [];
            for (const [Index, Source] of RUNTIME_SOURCES.entries()) {
                const Leaf = join(Work, `Runtime-${Index}.wvo`);
                Assemblyˉrequests.push(await Acquireˉassemblyˉobject(Assemblyˉcontext, join(REPOSITORY, Source), Leaf));
                Objects.push(Leaf);
            }
            const Linked = await Runˉnative('link-owned-entry', 'Link-Wvo', [
                '0', 'Windvale_owned_console_entry', Image, ...Objects, Renamed,
            ]);
            Entry = /^entry name=Windvale_owned_console_entry address=([0-9]+)$/mu.exec(Linked.Output)?.[1];
            if (Entry === undefined) throw new Error('The owned console entry is missing.');
            await Readˉboundedˉhostedˉfile(Image, 'linked owned console image', MAXIMUM_NATIVE_BYTES);
        } else await writeFile(Image, Payload, { flag: 'wx' });
        const Candidate = join(Work, 'Candidate' + Outputˉextension);
        await Run('construct-container', Packager, [Target, Image, Entry, Candidate]);
        await Requireˉunchanged();
        if (!Payload.equals(await Readˉboundedˉhostedˉfile(join(Work, Owned ? 'Input.wvo' : 'Input.bin'),
            'private console input', MAXIMUM_NATIVE_BYTES))) throw new Error('Private console input changed.');
        Publicationˉstarted = true;
        await Run('native-publication', Publisher.Path, [Candidate, Output]);
        Publicationˉcomplete = true;
        process.stdout.write(`current console status=Published target=${Target} output=${Output}\n`);
    } catch (Error) {
        if (Publicationˉstarted && !Publicationˉcomplete) {
            process.stderr.write('Publication attempted: inspect native status before retrying an indeterminate mutation.\n');
        }
        throw Error;
    } finally {
        if (dirname(Work) !== Temporary || !basename(Work).startsWith('windvale-current-console-')) {
            throw new Error('Refusing to remove an unowned console construction directory.');
        }
        await rm(Work, { recursive: true, force: false });
    }
}

try { await Main(); }
catch (Error) {
    process.stderr.write(`${Error.message}\n`);
    process.exitCode = Error.exitCode ?? 1;
}
