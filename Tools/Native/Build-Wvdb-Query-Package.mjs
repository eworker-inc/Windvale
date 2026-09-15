import { createHash } from 'node:crypto';
import { lstat, mkdtemp, realpath, rm, writeFile } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { basename, dirname, extname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { SOURCE_EDITION_PREDECESSOR } from './Source-Edition-Predecessor-Core.mjs';
import { Runˉdevelopmentˉcommand } from './Development-Command-Core.mjs';
import { Readˉboundedˉhostedˉfile } from './Native-Hosted-Application-Cache-Core.mjs';

const REPOSITORY = resolve(dirname(fileURLToPath(import.meta.url)), '..', '..');
const WINDOWS = process.platform === 'win32';
const HOST = WINDOWS ? 'windows-x64' : 'linux-x64';
const SUFFIX = WINDOWS ? '.exe' : '.elf';
const OUTPUT_SHA256 = '77cb6034402942734be316b9a135d6c1b46ace5cb43a198b2aafe2d1b098027b';
const SOURCES = [
    ['Projects/Applications/Windvale-Wvdb-Query.wvproj', 270, '86570daa0dac6410dc8a64947901a3fc955db24afe3589bc70986f96abb8f49a'],
    ['Applications/Database/Wvdb-Query.wv', 3168, '22d1fb0b883383fd51cd103d9b831d500178bbedc3d79df12dc86af74070c2d8'],
    ['Foundation/Decimal-Parsing.wv', 1276, '797eb31da7e7a8c93e0d082bf910bc6d8e7988bcfad757a87c979075912e668a'],
    ['Libraries/Platform/Filesystem/Read-Only-Directory.wv', 6847, '7b8600bebb590d8c9d9ce9c8b7318374d4ee0501d7e32dddeded3fcf672c3f28'],
    ['Libraries/Platform/Database/Read-Only-Wvdb.wv', 9084, '7b3bd45397878e5468d979a2fb437feb4d72d5d8bbad21c832bcf3f280c018cb'],
    ['Libraries/Database/Wvdb-Reader.wv', 10700, '665f805dc32d08bc6bd45f4f3ee67bb853dd2a622ac3b1edcafc22b532d9ac2b'],
];
const Hash = Bytes => createHash('sha256').update(Bytes).digest('hex');
let Publicationˉstarted = false;
function Require(Condition, Message) { if (!Condition) throw new Error(Message); }
function Invocation(Condition, Message) {
    if (!Condition) throw Object.assign(new Error(Message), { exitCode: 64 });
}
function Exact(Bytes, Size, Digest, Label) {
    Require(Bytes.length === Size && Hash(Bytes) === Digest, `Locked ${Label} identity differs.`);
}

async function Main() {
    Invocation(process.argv.length === 5 && process.arch === 'x64' && ['win32', 'linux'].includes(process.platform),
        'Usage: Build-Wvdb-Query-Package.mjs <manifest.wvpack> <lock.wvlock> <output.wvb>');
    const [Manifest, Lock, Output] = process.argv.slice(2).map(Value => resolve(Value));
    Invocation((WINDOWS ? Manifest.toLowerCase() : Manifest) ===
        (WINDOWS ? join(REPOSITORY, 'Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvpack').toLowerCase() :
            join(REPOSITORY, 'Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvpack')) &&
        extname(Lock) === '.wvlock' && extname(Output) === '.wvb', 'Invalid locked package invocation.');
    const Compiler = join(REPOSITORY, 'Artifacts/Native-Compiler-Reconstruction-Candidate', HOST, 'wvcompiler' + SUFFIX);
    const Publisher = join(REPOSITORY, 'Artifacts/Native-Front-Door', HOST, 'wvpublish' + SUFFIX);
    const Inputs = [
        [Lock, 1770, '7fe9552317e0845b693b8a4ade1882c4a492cecf46c1bfcaaf26b45ed067be50'],
        [Manifest, 866, '835f573302377fdd38e4c3d51fa9106397beba0b9813f99bfc3143d08a156406'],
        [join(REPOSITORY, 'Windvale.wvws'), 21, '5cb4f5f771ffd5a9f443ca993fd66f53109cd5862f7c268f1f3958a36b8f4199'],
        [join(REPOSITORY, 'Artifacts/Native-Compiler-Reconstruction-Candidate/Wvb/Windvale-Compiler.wvb'),
            935163, 'a7d47b2de29faee089c7a22ef23eac4657f719331dc02044eb2d818457dac5b6'],
        [Compiler, WINDOWS ? 28172800 : 28172288, WINDOWS ?
            'a5db938a814471fdacda75efcf57d28934ae52b3b2290732627c14ba173fd70d' :
            'da11ab3b70b428087cbcb9de5614a2dbdccd31afc6861cc15881fd65c12ff19b'],
        [Publisher, WINDOWS ? 1371136 : 1369077, WINDOWS ?
            'b9fd1b11bc1e4a726e4a43b16830a9351fe573b30e547ba8d8f6660f688ed421' :
            'b8efb90f7d7c4eae99de01df6c0a3c24a7396d9b9e717ff69d005282ed3d63af'],
    ];
    async function Checkˉinputs() {
        for (const [File, Size, Digest] of Inputs) Exact(
            await Readˉboundedˉhostedˉfile(File, 'locked package input', Size), Size, Digest, File);
        const Destination = await lstat(Output).catch(Error => {
            if (Error.code === 'ENOENT') return null;
            throw Error;
        });
        if (Destination !== null) {
            Invocation(Destination.isFile() && !Destination.isSymbolicLink(), 'Output must be an ordinary file.');
            for (const [File] of Inputs) {
                const Input = await lstat(File);
                Invocation(Destination.dev !== Input.dev || Destination.ino !== Input.ino,
                    'Output aliases a locked package input.');
            }
        }
    }
    await Checkˉinputs();
    const Temporary = await realpath(tmpdir());
    const Work = await realpath(await mkdtemp(join(Temporary, 'windvale-wvdb-locked-source-')));
    const Deadline = Date.now() + 120_000;
    async function Run(Command, Arguments, Relay = false) {
        const Result = await Runˉdevelopmentˉcommand(Command, Arguments, Deadline, Relay);
        Require(Result.Code === 0 && Result.Error === '', `Locked package producer failed: ${Result.Output}${Result.Error}`);
        return Result.Output;
    }
    try {
        const Paths = [];
        for (const [Index, [File, Size, Digest]] of SOURCES.entries()) {
            const Bytes = Buffer.from(await Run('git', ['-C', REPOSITORY, 'show', `${SOURCE_EDITION_PREDECESSOR}:${File}`]), 'utf8');
            Exact(Bytes, Size, Digest, File);
            // Only this historical lock consumes these exact private snapshots.
            if (Index > 0) {
                const Destination = join(Work, `${Index}.wv`);
                await writeFile(Destination, Bytes, { flag: 'wx' });
                Paths.push(Destination);
            }
        }
        const Candidate = join(Work, 'Candidate.wvb');
        await Run(Compiler, [...Paths, Candidate], true);
        Exact(await Readˉboundedˉhostedˉfile(Candidate, 'locked output', 26145), 26145, OUTPUT_SHA256, 'output');
        await Checkˉinputs();
        Publicationˉstarted = true;
        await Run(Publisher, [Candidate, Output], true);
        process.stdout.write(`package status=Published root=windvale.wvdb-query target=hosted-wvb-v1 bytes=26145 sha256=${OUTPUT_SHA256} source=frozen-historical-input\n`);
    } finally {
        Require(dirname(Work) === Temporary && basename(Work).startsWith('windvale-wvdb-locked-source-'), 'Unowned package temporary directory.');
        await rm(Work, { recursive: true, force: false });
    }
}

try { await Main(); }
catch (Error) {
    process.stderr.write(Publicationˉstarted
        ? `package status=Publication_failed completion=Inspect-native-status do-not-retry ${Error.message}\n`
        : `package status=Lock_rejected reason=identity-or-resource ${Error.message}\n`);
    process.exitCode = Error.exitCode ?? 1;
}
