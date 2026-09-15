import { mkdtemp, realpath, rm, writeFile } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { basename, dirname, extname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { Runˉdevelopmentˉcommand } from './Development-Command-Core.mjs';
import { Readˉboundedˉhostedˉfile } from './Native-Hosted-Application-Cache-Core.mjs';
import {
    Prepareˉnativeˉprojectˉcacheˉcontext, Getˉnativeˉprojectˉcacheˉrequest,
    Requireˉnativeˉprojectˉcacheˉrequestˉunchanged,
} from './Native-Project-Cache-Key-Core.mjs';
import {
    Acquireˉcurrentˉsplitˉcompiler,
    Getˉcurrentˉsplitˉcompilerˉfamily,
    Getˉcurrentˉsplitˉcompilerˉkey,
} from './Current-Split-Compiler-Cache-Core.mjs';

const NATIVE = dirname(fileURLToPath(import.meta.url));
const REPOSITORY = resolve(NATIVE, '..', '..');
const WINDOWS = process.platform === 'win32';
const SUFFIX = WINDOWS ? '.exe' : '.elf';
const WRAPPER = WINDOWS ? '.cmd' : '.sh';
const DEADLINE_MILLISECONDS = 600_000;

// The pinned two-argument host wrappers remain bootstrap entry points. This
// explicit development mode consumes a prepared current compiler checkpoint.
async function Main() {
    if (process.argv.length !== 5 || process.argv[2] !== '--current' ||
        extname(process.argv[3]).toLowerCase() !== '.wvb' ||
        extname(process.argv[4]).toLowerCase() !== '.wvo' || process.arch !== 'x64' ||
        !['win32', 'linux'].includes(process.platform)) {
        throw Object.assign(new Error('Usage: Lower-Wvb-To-Wvo.mjs --current <input.wvb> <output.wvo>'), { exitCode: 64 });
    }
    const Input = resolve(process.argv[3]);
    const Output = resolve(process.argv[4]);
    const Payload = await Readˉboundedˉhostedˉfile(Input, 'current lowerer input', 16_777_216);
    const Project = join(REPOSITORY, 'Projects/Compiler/Windvale-Native-X64-Lowering-Tool.wvproj');
    const Context = await Prepareˉnativeˉprojectˉcacheˉcontext('current-native-lowering-v1', [
        fileURLToPath(import.meta.url), join(NATIVE, 'Publish-Wvo' + WRAPPER),
        join(NATIVE, 'Development-Command-Core.mjs'),
    ]);
    const Request = await Getˉnativeˉprojectˉcacheˉrequest(Context, Project);
    const Key = await Getˉcurrentˉsplitˉcompilerˉkey();
    const Requireˉunchanged = async () => {
        await Requireˉnativeˉprojectˉcacheˉrequestˉunchanged(Request);
        if (await Getˉcurrentˉsplitˉcompilerˉkey() !== Key ||
            !Payload.equals(await Readˉboundedˉhostedˉfile(Input, 'current lowerer input', 16_777_216))) {
            throw new Error('Current lowerer construction inputs changed.');
        }
    };
    const Compiler = await Acquireˉcurrentˉsplitˉcompiler(
        await Getˉcurrentˉsplitˉcompilerˉfamily(), Key,
        () => { throw new Error('Current compiler checkpoint absent; prepare it in a separately selected construction run.'); },
        Requireˉunchanged,
    );
    const Deadline = Date.now() + DEADLINE_MILLISECONDS;
    const Temporary = await realpath(tmpdir());
    const Work = await realpath(await mkdtemp(join(Temporary, 'windvale-current-lower-')));
    async function Run(Step, Command, Arguments) {
        process.stdout.write(`current lowerer step=${Step} status=Started\n`);
        const Result = await Runˉdevelopmentˉcommand(Command, Arguments, Deadline, true);
        if (Result.Code !== 0 || Result.Error !== '') {
            throw new Error(`Current lowering ${Step} failed (${Result.Code}): ${Result.Error}`);
        }
        process.stdout.write(`current lowerer step=${Step} status=Complete\n`);
    }
    let Publicationˉstarted = false;
    try {
        const Wvb = join(Work, 'Lowerer.wvb');
        const Lowerer = join(Work, 'Lowerer' + SUFFIX);
        const Candidate = join(Work, 'Candidate.wvo');
        const Privateˉinput = join(Work, 'Input.wvb');
        await writeFile(Privateˉinput, Payload, { flag: 'wx' });
        await Run('source-build', process.execPath, [join(NATIVE, 'Build-Cached-Split-Project-Wvb.mjs'),
            Project, Wvb,
            join(Compiler.directory, 'Analyzer' + SUFFIX), join(Compiler.directory, 'Analyzer.identity'),
            join(Compiler.directory, 'Emitter' + SUFFIX), join(Compiler.directory, 'Emitter.identity'),
            '--authenticated-project4', ...['Admitter', 'Authenticator', 'Reader', 'Binder'].map(Name =>
                join(Compiler.directory, Name + SUFFIX))]);
        await Run('native-package', process.execPath, [join(NATIVE, 'Build-Cached-Segmented-Hosted-Wvb.mjs'),
            '7', Wvb, Lowerer]);
        await Run('lower', Lowerer, [Privateˉinput, Candidate]);
        await Requireˉunchanged();
        if (!Payload.equals(await Readˉboundedˉhostedˉfile(Privateˉinput, 'private lowerer input', 16_777_216))) {
            throw new Error('Private lowerer input changed.');
        }
        // Object validation and destination publication stay in the native owner.
        const Publisher = join(NATIVE, 'Publish-Wvo' + WRAPPER);
        Publicationˉstarted = true;
        await Run('native-publication', WINDOWS ? Publisher : 'bash',
            WINDOWS ? [Candidate, Output] : [Publisher, Candidate, Output]);
    } catch (Error) {
        if (Publicationˉstarted) process.stderr.write('Publication attempted: inspect native status; do not retry an indeterminate mutation.\n');
        throw Error;
    } finally {
        if (dirname(Work) !== Temporary || !basename(Work).startsWith('windvale-current-lower-')) {
            throw new Error('Refusing to remove an unowned current-lowerer directory.');
        }
        await rm(Work, { recursive: true, force: false });
    }
}

try { await Main(); }
catch (Error) {
    process.stderr.write(`${Error.message}\n`);
    process.exitCode = Error.exitCode ?? 1;
}
