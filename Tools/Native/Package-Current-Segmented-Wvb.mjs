import { createHash } from 'node:crypto';
import { copyFile, lstat, mkdir, mkdtemp, realpath, rm, writeFile } from 'node:fs/promises';
import { constants as FS_CONSTANTS } from 'node:fs';
import { tmpdir } from 'node:os';
import { basename, dirname, extname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { Runˉdevelopmentˉcommand } from './Development-Command-Core.mjs';
import { Readˉboundedˉhostedˉfile } from './Native-Hosted-Application-Cache-Core.mjs';
import { Acquireˉsegmentedˉimageˉcheckpoint, Validateˉsegmentedˉimageˉcheckpoint } from './Build-Cached-Segmented-Hosted-Wvb.mjs';
import {
    Prepareˉnativeˉprojectˉcacheˉcontext, Getˉnativeˉprojectˉcacheˉrequest,
    Requireˉnativeˉprojectˉcacheˉrequestˉunchanged,
} from './Native-Project-Cache-Key-Core.mjs';
import {
    Acquireˉcurrentˉsplitˉcompiler, Getˉcurrentˉsplitˉcompilerˉfamily,
    Getˉcurrentˉsplitˉcompilerˉkey,
} from './Current-Split-Compiler-Cache-Core.mjs';

const NATIVE = dirname(fileURLToPath(import.meta.url));
const REPOSITORY = resolve(NATIVE, '..', '..');
const WINDOWS = process.platform === 'win32';
const HOST = WINDOWS ? 'windows-x64' : 'linux-x64';
const SUFFIX = WINDOWS ? '.exe' : '.elf';
const WRAPPER = WINDOWS ? '.cmd' : '.sh';
const NAMESPACE = 'current-segmented-native-image-v1';
const Hash = Bytes => createHash('sha256').update(Bytes).digest('hex');
function Require(Condition, Message) { if (!Condition) throw new Error(Message); }
function Invocation(Condition, Message) {
    if (!Condition) throw Object.assign(new Error(Message), { exitCode: 64 });
}

async function Main() {
    const [Profile, Inputˉargument, Outputˉargument, Target = WINDOWS ? 'windows' : 'linux'] = process.argv.slice(2);
    Invocation([5, 6].includes(process.argv.length) && /^[1-8]$/u.test(Profile) &&
        extname(Inputˉargument).toLowerCase() === '.wvb' && ['windows', 'linux'].includes(Target) &&
        extname(Outputˉargument).toLowerCase() === (Target === 'windows' ? '.exe' : '.elf') &&
        process.arch === 'x64' && ['win32', 'linux'].includes(process.platform),
    'Usage: Package-Current-Segmented-Wvb.mjs <profile-1-through-8> <input.wvb> <output.exe-or-elf> [windows|linux]');
    const Input = resolve(Inputˉargument);
    const Output = resolve(Outputˉargument);
    const Payload = await Readˉboundedˉhostedˉfile(Input, 'current segmented input', 16_777_216);
    const Snapshot = { path: Input, payload: Payload, bytes: Payload.length, sha256: Hash(Payload) };
    const Key = await Getˉcurrentˉsplitˉcompilerˉkey();
    const Unchanged = async () => {
        Require(await Getˉcurrentˉsplitˉcompilerˉkey() === Key &&
            Payload.equals(await Readˉboundedˉhostedˉfile(Input, 'current segmented input', 16_777_216)),
        'Current segmented construction inputs changed.');
    };
    const Compilerˉfamily = await Getˉcurrentˉsplitˉcompilerˉfamily();
    const Compiler = await Acquireˉcurrentˉsplitˉcompiler(Compilerˉfamily, Key,
        () => { throw new Error('Prepare the current compiler checkpoint in a separately selected construction run.'); }, Unchanged);
    const Deadline = Date.now() + 1_200_000;
    const Temporary = await realpath(tmpdir());
    const Work = await realpath(await mkdtemp(join(Temporary, 'windvale-current-segmented-')));
    async function Run(Step, Command, Arguments) {
        process.stdout.write(`current segmented package step=${Step} status=Started\n`);
        const Result = await Runˉdevelopmentˉcommand(Command, Arguments, Deadline, true, 1_048_576);
        Require(Result.Code === 0 && Result.Error === '', `${Step} failed exit=${Result.Code}: ${Result.Output}${Result.Error}`);
        process.stdout.write(`current segmented package step=${Step} status=Complete\n`);
    }
    const Native = (Step, Name, Arguments) => {
        const Script = join(NATIVE, Name + WRAPPER);
        return Run(Step, WINDOWS ? Script : 'bash', WINDOWS ? Arguments : [Script, ...Arguments]);
    };
    let Publicationˉstarted = false;
    try {
        const Project = join(REPOSITORY, 'Projects/Compiler/Windvale-Native-X64-Lowering-Staging-Tool.wvproj');
        const Stagerˉwvb = join(Work, 'Stager.wvb');
        const Stager = join(Work, 'Stager' + SUFFIX);
        const Inputs = [fileURLToPath(import.meta.url),
            ...['Development-Command-Core.mjs', 'Build-Cached-Segmented-Hosted-Wvb.mjs',
                'Build-Cached-Segmented-Project.mjs', 'Native-Hosted-Application-Cache-Core.mjs']
                .map(Name => join(NATIVE, Name)),
            ...['Link-Staged-Compiler-Wvo', 'Transport-Compiler-Image'].map(Name => join(NATIVE, Name + WRAPPER)),
            ...['wvlinkstage', 'wvimagetransport'].map(Name => join(REPOSITORY,
                'Artifacts/Native-Segmented-Compiler-Toolset-Candidate', HOST + '-' + Name + SUFFIX))];
        const Context = await Prepareˉnativeˉprojectˉcacheˉcontext(NAMESPACE, Inputs);
        const Request = await Getˉnativeˉprojectˉcacheˉrequest(Context, Project);
        await Run('stager-source', process.execPath, [join(NATIVE, 'Build-Cached-Split-Project-Wvb.mjs'),
            Project, Stagerˉwvb, join(Compiler.directory, 'Analyzer' + SUFFIX), join(Compiler.directory, 'Analyzer.identity'),
            join(Compiler.directory, 'Emitter' + SUFFIX), join(Compiler.directory, 'Emitter.identity'),
            '--authenticated-project4', ...['Admitter', 'Authenticator', 'Reader', 'Binder']
                .map(Name => join(Compiler.directory, Name + SUFFIX))]);
        await Run('stager-package', process.execPath, [join(NATIVE, 'Build-Cached-Segmented-Hosted-Wvb.mjs'),
            '7', Stagerˉwvb, Stager]);
        const Stagerˉbytes = await Readˉboundedˉhostedˉfile(Stager, 'current native stager', 67_108_864);
        const Imageˉkey = Hash(Buffer.from([NAMESPACE, HOST, process.version, Key, Request.key,
            Snapshot.sha256, Hash(Stagerˉbytes)].join('\n'), 'utf8'));
        const Admit = async () => {
            await Unchanged();
            await Requireˉnativeˉprojectˉcacheˉrequestˉunchanged(Request);
            Require(Stagerˉbytes.equals(await Readˉboundedˉhostedˉfile(Stager, 'current native stager', 67_108_864)),
                'Current native stager changed.');
        };
        let Family = dirname(dirname(Compilerˉfamily));
        for (const Part of [NAMESPACE, HOST]) {
            Family = join(Family, Part);
            await mkdir(Family).catch(Error => { if (Error.code !== 'EEXIST') throw Error; });
            const Information = await lstat(Family);
            const Canonical = await realpath(Family);
            Require(Information.isDirectory() && !Information.isSymbolicLink() &&
                (WINDOWS ? Canonical.toLowerCase() === Family.toLowerCase() : Canonical === Family),
            'Non-canonical current image cache.');
        }
        const Privateˉinput = join(Work, 'Input.wvb');
        await writeFile(Privateˉinput, Payload, { flag: 'wx' });
        const Image = await Acquireˉsegmentedˉimageˉcheckpoint(Family, Imageˉkey, Snapshot, async Candidate => {
            const Object = join(Work, 'Object');
            const Linked = join(Work, 'Linked');
            await Run('stage', Stager, [Privateˉinput, Object, Object + '.wvop']);
            await Native('link', 'Link-Staged-Compiler-Wvo', [Object, Object + '.wvop', Linked, Linked + '.wvli']);
            await Native('transport', 'Transport-Compiler-Image', [Linked, Linked + '.wvli',
                join(Candidate, 'Image'), join(Candidate, 'Image.wvli')]);
        }, Admit);
        process.stdout.write(`current segmented package image-cache=${Image.status} key=${Imageˉkey}\n`);
        const Privateˉimage = join(Work, 'Image');
        await mkdir(Privateˉimage);
        for (const File of [join(dirname(Image.manifestPath), 'Checkpoint.txt'), Image.manifestPath, ...Image.fragmentPaths]) {
            await copyFile(File, join(Privateˉimage, basename(File)), FS_CONSTANTS.COPYFILE_EXCL);
        }
        await Validateˉsegmentedˉimageˉcheckpoint(Privateˉimage, Imageˉkey, Snapshot);
        await Admit();
        Require(Payload.equals(await Readˉboundedˉhostedˉfile(Privateˉinput, 'private package input', 16_777_216)),
            'Private package input changed.');
        // The existing native image packager owns validation and final publication.
        Publicationˉstarted = true;
        await Native('native-container-publication', 'Package-Hosted-Wvb', ['image', Profile, Privateˉinput,
            join(Privateˉimage, 'Image'), String(Image.fragments.length), String(Image.entryOffset), Output, Target]);
        process.stdout.write(`current segmented package status=Published target=${Target} profile=${Profile} input-sha256=${Snapshot.sha256}\n`);
    } catch (Error) {
        if (Publicationˉstarted) process.stderr.write('Publication attempted: inspect native status; do not retry an indeterminate mutation.\n');
        throw Error;
    } finally {
        Require(dirname(Work) === Temporary && basename(Work).startsWith('windvale-current-segmented-'), 'Unowned segmented package work.');
        await rm(Work, { recursive: true, force: false });
    }
}

try { await Main(); }
catch (Error) { process.stderr.write(Error.message + '\n'); process.exitCode = Error.exitCode ?? 1; }
