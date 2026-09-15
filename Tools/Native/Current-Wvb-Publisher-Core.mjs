import { createHash } from 'node:crypto';
import { lstat, mkdir, mkdtemp, realpath, rm } from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';
import {
    Addˉhostedˉkeyˉfield,
    HOSTED_REPOSITORY_ROOT,
    Isˉsameˉhostedˉpath,
    Prepareˉhostedˉapplicationˉcontext,
    Readˉboundedˉhostedˉfile,
} from './Native-Hosted-Application-Cache-Core.mjs';
import {
    Createˉsegmentedˉhostedˉcheckpoint,
    Materializeˉsegmentedˉhostedˉcheckpoint,
    Requireˉloadedˉsegmentedˉhostedˉproducersˉunchanged,
    Validateˉsegmentedˉhostedˉcheckpoint,
} from './Build-Cached-Segmented-Hosted-Wvb.mjs';

const NAMESPACE = 'current-transactional-wvb-publisher-v1';
const WINDOWS = process.platform === 'win32';
const HOST = WINDOWS ? 'windows-x64' : 'linux-x64';
const EXTENSION = WINDOWS ? 'exe' : 'elf';
const WRAPPER = WINDOWS ? 'cmd' : 'sh';
const PROFILE = '2';
const MAXIMUM_WVB_BYTES = 16_777_216;
const OBJECTS = Object.freeze([
    'Windows-X64-Wvb-Publisher.wvo', 'Linux-X64-Wvb-Publisher.wvo',
    'Windows-X64-Wvb-Publication-Adapter.wvo', 'Linux-X64-Wvb-Publication-Adapter.wvo',
    'X64-Wvb-Publication-Sha256.wvo', 'X64-Publication-Transaction-State.wvo',
]);
const LOADED_PRODUCERS = await Promise.all([
    'Tools/Native/Current-Wvb-Publisher-Core.mjs',
    'Tools/Native/Build-Wvb-Project4.mjs',
    'Tools/Native/Development-Command-Core.mjs',
].map(async Relative => ({ Relative, Bytes: await Readˉboundedˉhostedˉfile(
    path.join(HOSTED_REPOSITORY_ROOT, Relative), 'loaded publisher producer',
) })));

function Reject(Message) { throw new Error(Message); }

async function Directory(Candidate, Create = false) {
    const Resolved = path.resolve(Candidate);
    const Root = path.parse(Resolved).root;
    let Current = Root;
    for (const Part of Resolved.slice(Root.length).split(path.sep).filter(Boolean)) {
        Current = path.join(Current, Part);
        if (Create) await mkdir(Current).catch(Error => {
            if (Error.code !== 'EEXIST') throw Error;
        });
        const Information = await lstat(Current);
        if (!Information.isDirectory() || Information.isSymbolicLink()) {
            Reject(`The publisher directory contains a link or non-directory: ${Current}`);
        }
    }
    if (!Isˉsameˉhostedˉpath(await realpath(Resolved), Resolved)) {
        Reject('The publisher directory must use its canonical path.');
    }
    return Resolved;
}

async function Cacheˉkey(Input, Compilerˉkey) {
    for (const Producer of LOADED_PRODUCERS) {
        const Current = await Readˉboundedˉhostedˉfile(
            path.join(HOSTED_REPOSITORY_ROOT, Producer.Relative), 'loaded publisher producer',
        );
        if (!Current.equals(Producer.Bytes)) Reject('A loaded publisher producer changed.');
    }
    const Hash = createHash('sha256');
    const Field = (Name, Value) => Addˉhostedˉkeyˉfield(Hash, Name, Value);
    for (const [Name, Value] of [
        ['namespace', NAMESPACE], ['host', HOST], ['node', process.version],
        ['compiler', Compilerˉkey],
    ]) Field(Name, Buffer.from(Value, 'utf8'));
    Field('input', Input.payload);
    // The source compiler identity is separate from the native/container producers.
    for (const Relative of [
        'Tools/Native/Current-Wvb-Publisher-Core.mjs',
        'Tools/Native/Build-Wvb-Project4.mjs',
        'Tools/Native/Development-Command-Core.mjs',
        'Tools/Native/Build-Cached-Segmented-Hosted-Wvb.mjs',
        'Tools/Native/Build-Cached-Segmented-Project.mjs',
        'Tools/Native/Build-Current-Publisher-Binding.mjs',
        'Tools/Native/Plan-Current-Publisher-Linkage.mjs',
        'Tools/Native/Bind-Current-Publisher-Host-Imports.mjs',
        'Tools/Native/Materialize-Current-Publisher-Executable.mjs',
        `Tools/Native/Stage-Compiler-Wvb.${WRAPPER}`,
        `Tools/Native/Link-Staged-Compiler-Wvo.${WRAPPER}`,
        `Artifacts/Native-Segmented-Compiler-Toolset-Candidate/${HOST}-wvstage.${EXTENSION}`,
        `Artifacts/Native-Segmented-Compiler-Toolset-Candidate/${HOST}-wvlinkstage.${EXTENSION}`,
        ...OBJECTS.map(Leaf => `Linker/Reference/Consumers/${Leaf}`),
    ]) {
        const Bytes = await Readˉboundedˉhostedˉfile(
            path.join(HOSTED_REPOSITORY_ROOT, Relative), 'publisher producer', 134_217_728,
        );
        Field(Relative, createHash('sha256').update(Bytes).digest());
    }
    const Node = await Readˉboundedˉhostedˉfile(process.execPath, 'Node runtime', 134_217_728);
    Field('node-executable', createHash('sha256').update(Node).digest());
    const Hosted = await Prepareˉhostedˉapplicationˉcontext(
        WINDOWS ? 'windows' : 'linux',
        path.join(HOSTED_REPOSITORY_ROOT, 'Tools', 'Native', `Package-Hosted-Wvb.${WRAPPER}`),
    );
    for (const Producer of Hosted.producerFields) Field(Producer.label, Producer.bytes);
    return Hash.digest('hex');
}

// Construction only: destination mutation is performed by the returned native tool.
export async function Acquireˉcurrentˉwvbˉpublisher(
    Wvb, Output, Compilerˉkey, Runˉnative, Runˉnode,
) {
    if (process.arch !== 'x64' || !['win32', 'linux'].includes(process.platform) ||
        !/^[0-9a-f]{64}$/u.test(Compilerˉkey) ||
        typeof Runˉnative !== 'function' || typeof Runˉnode !== 'function') {
        Reject('Invalid current publisher construction request.');
    }
    Wvb = path.resolve(Wvb);
    Output = path.resolve(Output);
    await Directory(path.dirname(Output));
    if (path.extname(Output) !== `.${EXTENSION}` ||
        await lstat(Output).catch(Error => {
            if (Error.code === 'ENOENT') return null;
            throw Error;
        }) !== null) Reject('The current publisher output must be a new executable path.');
    const Payload = await Readˉboundedˉhostedˉfile(Wvb, 'current publisher WVB', MAXIMUM_WVB_BYTES);
    const Input = { path: Wvb, payload: Payload, bytes: Payload.length,
        sha256: createHash('sha256').update(Payload).digest('hex') };
    const Key = await Cacheˉkey(Input, Compilerˉkey);
    if (WINDOWS && !process.env.WINDVALE_NATIVE_CACHE_ROOT && !process.env.LOCALAPPDATA) {
        Reject('A local application-data or explicit native cache directory is required.');
    }
    const Cacheˉroot = process.env.WINDVALE_NATIVE_CACHE_ROOT ?? (WINDOWS
        ? path.join(process.env.LOCALAPPDATA, 'Windvale', 'Native-Tool-Cache')
        : path.join(process.env.XDG_CACHE_HOME ?? path.join(os.homedir(), '.cache'),
            'windvale', 'native-tool-cache'));
    const Family = await Directory(path.join(Cacheˉroot, NAMESPACE, HOST), true);
    const Destination = path.join(Family, Key);
    const Admit = async () => {
        const Current = await Readˉboundedˉhostedˉfile(Wvb, 'current publisher WVB', MAXIMUM_WVB_BYTES);
        await Requireˉloadedˉsegmentedˉhostedˉproducersˉunchanged();
        if (!Current.equals(Payload) || await Cacheˉkey(Input, Compilerˉkey) !== Key) {
            Reject('Current publisher inputs or producers changed during construction.');
        }
    };
    const Existing = await lstat(Destination).catch(Error => {
        if (Error.code === 'ENOENT') return null;
        throw Error;
    });
    let Status = 'Hit';
    if (Existing === null) {
        Status = await Createˉsegmentedˉhostedˉcheckpoint(
            Family, Destination, Key, PROFILE, Input,
            async Candidate => {
                const Root = await realpath(os.tmpdir());
                const Work = await mkdtemp(path.join(Root, 'windvale-current-publisher-build-'));
                try {
                    const Object = path.join(Work, 'Object');
                    const Image = path.join(Work, 'Image');
                    const Binding = path.join(Work, 'Binding.wvcp');
                    const Linkage = path.join(Work, 'Linkage.wvcl');
                    const References = path.join(HOSTED_REPOSITORY_ROOT, 'Linker', 'Reference', 'Consumers');
                    await Runˉnative('publisher-stage', 'Stage-Compiler-Wvb', [Wvb, Object, `${Object}.wvop`]);
                    await Runˉnative('publisher-link', 'Link-Staged-Compiler-Wvo',
                        [Object, `${Object}.wvop`, Image, `${Image}.wvli`]);
                    await Runˉnode('publisher-bind', 'Build-Current-Publisher-Binding.mjs',
                        [Wvb, Object, `${Object}.wvop`, Image, `${Image}.wvli`, References, Binding]);
                    await Runˉnode('publisher-plan', 'Plan-Current-Publisher-Linkage.mjs',
                        [Binding, Image, `${Image}.wvli`, References, Linkage]);
                    await Runˉnode('publisher-materialize', 'Materialize-Current-Publisher-Executable.mjs',
                        [Linkage, HOST, Image, `${Image}.wvli`, References, Wvb,
                            path.join(Candidate, `Product.${EXTENSION}`), path.join(Work, 'Imports.wvci')]);
                } finally {
                    if (path.dirname(Work) !== Root ||
                        !path.basename(Work).startsWith('windvale-current-publisher-build-')) {
                        Reject('Refusing to remove an unowned publisher build directory.');
                    }
                    await Directory(Work);
                    await rm(Work, { recursive: true, force: false });
                }
            }, Admit,
        );
    }
    const Checkpoint = await Validateˉsegmentedˉhostedˉcheckpoint(Destination, Key, PROFILE, Input);
    await Admit();
    await Materializeˉsegmentedˉhostedˉcheckpoint(Checkpoint, Output);
    process.stdout.write(`current publisher cache status=${Status} key=${Key} host=${HOST}\n`);
    return { Path: Output, Key, Status };
}
