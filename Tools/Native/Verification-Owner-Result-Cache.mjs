import {
    access,
    lstat,
    mkdir,
    mkdtemp,
    open,
    readFile,
    readdir,
    readlink,
    realpath,
    rename,
    rm,
    stat,
    utimes,
} from 'node:fs/promises';
import { constants as Fileˉconstants, createReadStream } from 'node:fs';
import { execFile } from 'node:child_process';
import { createHash, randomUUID } from 'node:crypto';
import { promisify, TextDecoder } from 'node:util';
import {
    arch,
    homedir,
    hostname,
    platform,
    release,
    tmpdir,
    type,
    uptime,
} from 'node:os';
import {
    delimiter,
    dirname,
    extname,
    isAbsolute,
    join,
    parse,
    relative,
    resolve,
    sep,
} from 'node:path';
import { fileURLToPath } from 'node:url';

const Executeˉfile = promisify(execFile);
const CACHE_FAMILY = 'owner-result-v1';
const STATE_FORMAT = 'windvale-verification-owner-state-1';
const RESULT_FORMAT = 'windvale-verification-owner-result-1';
const STATE_RECORD_FORMAT = 'windvale-verification-owner-state-record-1';
const CANDIDATE_FORMAT = 'windvale-verification-owner-candidates-1';
const CHANGED_PATH_FORMAT = 'windvale-verification-owner-changed-paths-1';
const STATE_RECORD_NAME = 'State.json';
const MAX_STATE_RECORD_BYTES = 4 * 1024;
const MAX_CHANGED_PATHS = 65_536;
const MAX_CACHE_FAMILY_ENTRIES = 2_048;
const MAX_STATE_DIRECTORIES = 16;
const MAX_COMPATIBLE_CANDIDATES = 15;
const MAX_RESULTS_PER_STATE = 512;
const MAX_RESULT_BYTES = 16 * 1024;
const MAX_STATE_AGE_MS = 7 * 24 * 60 * 60 * 1000;
const MAX_TEMPORARY_AGE_MS = 60 * 60 * 1000;
const HEX_DIGEST = /^[0-9a-f]{64}$/;
const GIT_TREE = /^[0-9a-f]{40}(?:[0-9a-f]{24})?$/;
const OWNER_NAME = /^[a-z0-9]+(?:-[a-z0-9]+)*$/;
const RESULT_NAME = /^[0-9a-f]{64}\.json$/;
const TEMPORARY_NAME = /^\.new-[0-9a-f]{64}-[0-9]+-[0-9a-f-]{36}$/;
const STATE_TEMPORARY_NAME = /^\.state-new-[0-9]+-[0-9a-f-]{36}$/;
const WINDOWS = platform() === 'win32';

let Hostˉidentityˉpromise;

function Digest(Value) {
    return createHash('sha256').update(Value).digest('hex');
}

async function Digestˉfile(Path) {
    const Hash = createHash('sha256');
    for await (const Chunk of createReadStream(Path)) {
        Hash.update(Chunk);
    }
    return Hash.digest('hex');
}

function Requireˉdigest(Value, Name) {
    if (!HEX_DIGEST.test(Value)) {
        throw new Error(`${Name} must be one lowercase SHA-256 digest.`);
    }
    return Value;
}

function Requireˉowner(Value) {
    if (Value.length > 128 || !OWNER_NAME.test(Value)) {
        throw new Error('Verification owner name is invalid.');
    }
    return Value;
}

function Requireˉaction(Value) {
    if (Value.length === 0 || Value.length > 4096 || Value.includes('\0')) {
        throw new Error('Verification owner action is invalid.');
    }
    return Value;
}

function Requireˉcontained(Path, Parent) {
    const Resolvedˉpath = resolve(Path);
    const Resolvedˉparent = resolve(Parent);
    const Relative = relative(Resolvedˉparent, Resolvedˉpath);
    if (Relative.length === 0 || Relative === '..' ||
        Relative.startsWith(`..${sep}`) || resolve(Resolvedˉparent, Relative) !== Resolvedˉpath) {
        throw new Error(`Verification result cache path escaped its family: ${Resolvedˉpath}`);
    }
    return Resolvedˉpath;
}

function Isˉinsideˉorˉequal(Path, Parent) {
    const Relative = relative(resolve(Parent), resolve(Path));
    return Relative.length === 0 ||
        (Relative !== '..' &&
            !Relative.startsWith(`..${sep}`) &&
            !isAbsolute(Relative));
}

async function Ensureˉordinaryˉdirectory(Path) {
    const Resolved = resolve(Path);
    const Root = parse(Resolved).root;
    const Components = relative(Root, Resolved)
        .split(sep)
        .filter(Component => Component.length !== 0);
    let Current = Root;
    for (const Component of Components) {
        Current = join(Current, Component);
        try {
            const Metadata = await lstat(Current);
            if (!Metadata.isDirectory() || Metadata.isSymbolicLink()) {
                throw new Error(
                    `Verification result cache path is linked or not a directory: ${Current}`
                );
            }
        } catch (Errorˉvalue) {
            if (Errorˉvalue?.code !== 'ENOENT') {
                throw Errorˉvalue;
            }
            try {
                await mkdir(Current, { mode: 0o700 });
            } catch (Createˉerror) {
                if (Createˉerror?.code !== 'EEXIST') {
                    throw Createˉerror;
                }
            }
            const Metadata = await lstat(Current);
            if (!Metadata.isDirectory() || Metadata.isSymbolicLink()) {
                throw new Error(
                    `Verification result cache path is linked or not a directory: ${Current}`
                );
            }
        }
    }
    return Resolved;
}

function Defaultˉcacheˉroot() {
    if (process.env.WINDVALE_VERIFICATION_RESULT_CACHE_ROOT !== undefined) {
        if (process.env.WINDVALE_VERIFICATION_RESULT_CACHE_ROOT.length === 0) {
            throw new Error(
                'WINDVALE_VERIFICATION_RESULT_CACHE_ROOT must not be empty.'
            );
        }
        return process.env.WINDVALE_VERIFICATION_RESULT_CACHE_ROOT;
    }
    if (WINDOWS) {
        if (process.env.LOCALAPPDATA === undefined) {
            throw new Error('LOCALAPPDATA is unavailable for the verification cache.');
        }
        return join(
            process.env.LOCALAPPDATA,
            'Windvale',
            'Verification-Result-Cache',
        );
    }
    return join(
        process.env.XDG_CACHE_HOME ?? join(homedir(), '.cache'),
        'windvale',
        'verification-result-cache',
    );
}

async function Runˉgit(Repository, Arguments, Environment = {}) {
    const Result = await Executeˉfile(
        'git',
        ['-C', Repository, ...Arguments],
        {
            encoding: 'utf8',
            env: { ...process.env, ...Environment },
            maxBuffer: 1024 * 1024,
            windowsHide: true,
        },
    );
    return Result.stdout.trim();
}

async function Runˉgitˉbytes(Repository, Arguments) {
    const Result = await Executeˉfile(
        'git',
        ['-C', Repository, ...Arguments],
        {
            encoding: 'buffer',
            env: process.env,
            maxBuffer: 64 * 1024 * 1024,
            windowsHide: true,
        },
    );
    return Result.stdout;
}

async function Measureˉsourceˉtree(Repository) {
    const Work = await mkdtemp(join(
        tmpdir(),
        'windvale-verification-source-state-',
    ));
    const Index = join(Work, 'Index');
    const Environment = { GIT_INDEX_FILE: Index };
    try {
        let Sparse = 'false';
        try {
            Sparse = await Runˉgit(
                Repository,
                ['config', '--bool', 'core.sparseCheckout'],
            );
        } catch (Errorˉvalue) {
            if (Errorˉvalue?.code !== 1) {
                throw Errorˉvalue;
            }
        }
        if (Sparse === 'true') {
            throw new Error(
                'Persistent verification resume is unavailable in a sparse checkout.'
            );
        }
        await Runˉgit(Repository, ['read-tree', 'HEAD'], Environment);
        await Runˉgit(Repository, ['add', '-A', '--', '.'], Environment);
        const Tree = await Runˉgit(Repository, ['write-tree'], Environment);
        if (!GIT_TREE.test(Tree)) {
            throw new Error('Git returned an invalid source-tree identity.');
        }
        return Tree;
    } finally {
        await rm(Work, { recursive: true, force: true });
    }
}

async function Measureˉsourceˉsentinel(Repository) {
    const Hash = createHash('sha256');
    Hash.update('windvale-verification-source-sentinel-1\0');
    Hash.update(await Runˉgitˉbytes(Repository, [
        'diff',
        '--binary',
        '--full-index',
        '--no-ext-diff',
        '--no-textconv',
        '--no-renames',
        'HEAD',
        '--',
    ]));
    Hash.update('\0untracked\0');
    const Untracked = await Runˉgitˉbytes(
        Repository,
        ['ls-files', '-z', '--others', '--exclude-standard'],
    );
    for (const Pathˉbytes of Splitˉnulˉpaths(Untracked)) {
        const Relativeˉpath = Pathˉbytes.toString('utf8');
        const Path = Requireˉcontained(
            join(Repository, Relativeˉpath),
            Repository,
        );
        const Metadata = await lstat(Path);
        Hash.update(Pathˉbytes);
        Hash.update('\0');
        Hash.update(String(Metadata.mode & 0o111));
        Hash.update('\0');
        if (Metadata.isSymbolicLink()) {
            Hash.update('link\0');
            Hash.update(await readlink(Path, { encoding: 'buffer' }));
        } else if (Metadata.isFile()) {
            Hash.update('file\0');
            Hash.update(String(Metadata.size));
            Hash.update('\0');
            Hash.update(await Digestˉfile(Path));
        } else {
            throw new Error(
                `Untracked source input is not a file or symbolic link: ${Path}`
            );
        }
        Hash.update('\0');
    }
    return Hash.digest('hex');
}

function Splitˉnulˉpaths(Value) {
    const Paths = [];
    let Start = 0;
    for (let Index = 0; Index < Value.length; Index += 1) {
        if (Value[Index] === 0) {
            if (Index > Start) {
                Paths.push(Value.subarray(Start, Index));
            }
            Start = Index + 1;
        }
    }
    if (Start !== Value.length) {
        throw new Error('Git returned an unterminated untracked-path list.');
    }
    return Paths;
}

async function Findˉexecutable(Name) {
    if (Name === 'node') {
        return process.execPath;
    }
    if (Name === 'cmd' && process.env.ComSpec !== undefined) {
        return process.env.ComSpec;
    }
    const Hasˉextension = extname(Name).length !== 0;
    const Extensions = WINDOWS && !Hasˉextension
        ? (process.env.PATHEXT ?? '.COM;.EXE;.BAT;.CMD')
            .split(';')
            .filter(Boolean)
        : [''];
    for (const Directory of (process.env.PATH ?? '').split(delimiter)) {
        if (Directory.length === 0) {
            continue;
        }
        for (const Extension of Extensions) {
            const Candidate = join(Directory, Name + Extension);
            try {
                const Metadata = await stat(Candidate);
                if (!Metadata.isFile()) {
                    continue;
                }
                if (!WINDOWS) {
                    await access(Candidate, Fileˉconstants.X_OK);
                }
                return Candidate;
            } catch (Errorˉvalue) {
                if (Errorˉvalue?.code !== 'ENOENT' &&
                    Errorˉvalue?.code !== 'EACCES') {
                    throw Errorˉvalue;
                }
            }
        }
    }
    return null;
}

async function Measureˉtool(Name) {
    const Candidate = await Findˉexecutable(Name);
    if (Candidate === null) {
        return { name: Name, status: 'absent' };
    }
    const Resolved = await realpath(Candidate);
    const Metadata = await stat(Resolved);
    return {
        name: Name,
        status: 'present',
        path: Resolved,
        bytes: Metadata.size,
        sha256: await Digestˉfile(Resolved),
    };
}

function Measureˉenvironment() {
    const Names = new Set([
        'AR',
        'CC',
        'CXX',
        'LANG',
        'LC_ALL',
        'LD',
        'NODE_OPTIONS',
        'OS',
        'PATH',
        'PATHEXT',
        'PROCESSOR_ARCHITECTURE',
        'PROCESSOR_IDENTIFIER',
        'TZ',
    ]);
    for (const Name of Object.keys(process.env)) {
        if (Name.toUpperCase().startsWith('WINDVALE_') &&
            Name.toUpperCase() !==
                'WINDVALE_VERIFICATION_RESULT_CACHE_ROOT') {
            Names.add(Name);
        }
    }
    return [...Names]
        .sort()
        .map(Name => ({
            name: Name,
            value: process.env[Name] ?? null,
        }));
}

async function Measureˉhostˉidentity() {
    if (Hostˉidentityˉpromise === undefined) {
        Hostˉidentityˉpromise = (async () => {
            const Toolˉnames = WINDOWS
                ? [
                    'node', 'pwsh', 'git', 'cmd', 'certutil', 'where',
                    'wsl', 'qemu-system-x86_64', 'nasm', 'clang',
                    'lld-link', 'python',
                ]
                : [
                    'node', 'bash', 'git', 'sha256sum', 'awk', 'sed',
                    'grep', 'cc', 'clang', 'ld', 'nasm',
                    'qemu-system-x86_64', 'python3',
                ];
            const Tools = [];
            for (const Name of Toolˉnames) {
                Tools.push(await Measureˉtool(Name));
            }
            return {
                platform: platform(),
                type: type(),
                release: release(),
                architecture: arch(),
                host: hostname(),
                bootEpochMinute: Math.round(
                    ((Date.now() / 1000) - uptime()) / 60
                ),
                nodeVersion: process.version,
                tools: Tools,
                environment: Measureˉenvironment(),
            };
        })();
    }
    return Hostˉidentityˉpromise;
}

async function Removeˉordinaryˉdirectory(Path, Family) {
    Requireˉcontained(Path, Family);
    const Metadata = await lstat(Path);
    if (!Metadata.isDirectory() || Metadata.isSymbolicLink()) {
        throw new Error(`Refusing to remove an unsafe cache directory: ${Path}`);
    }
    await rm(Path, { recursive: true, force: false });
}

async function Pruneˉstateˉcontents(Stateˉdirectory, Now) {
    const Entries = await readdir(Stateˉdirectory, { withFileTypes: true });
    const Results = [];
    for (const Entry of Entries) {
        const Path = join(Stateˉdirectory, Entry.name);
        if (Entry.isSymbolicLink()) {
            continue;
        }
        if (Entry.isFile() && RESULT_NAME.test(Entry.name)) {
            const Metadata = await stat(Path);
            Results.push({ Path, Time: Metadata.mtimeMs });
        } else if (Entry.isFile() && TEMPORARY_NAME.test(Entry.name)) {
            const Metadata = await stat(Path);
            if (Now - Metadata.mtimeMs > MAX_TEMPORARY_AGE_MS) {
                await rm(Path, { force: true });
            }
        } else if (Entry.isFile() && STATE_TEMPORARY_NAME.test(Entry.name)) {
            const Metadata = await stat(Path);
            if (Now - Metadata.mtimeMs > MAX_TEMPORARY_AGE_MS) {
                await rm(Path, { force: true });
            }
        }
    }
    Results.sort((Left, Right) => Right.Time - Left.Time);
    for (const Result of Results.slice(MAX_RESULTS_PER_STATE)) {
        Requireˉcontained(Result.Path, Stateˉdirectory);
        await rm(Result.Path, { force: true });
    }
}

async function Pruneˉfamily(Family, Currentˉstate) {
    const Now = Date.now();
    const States = [];
    for (const Entry of await readdir(Family, { withFileTypes: true })) {
        if (!HEX_DIGEST.test(Entry.name) || Entry.isSymbolicLink()) {
            continue;
        }
        const Path = join(Family, Entry.name);
        if (!Entry.isDirectory()) {
            continue;
        }
        const Metadata = await stat(Path);
        States.push({
            Name: Entry.name,
            Path,
            Time: Metadata.mtimeMs,
        });
    }
    States.sort((Left, Right) => Right.Time - Left.Time);
    const Retained = [];
    for (const State of States) {
        if (State.Name !== Currentˉstate &&
            (Now - State.Time > MAX_STATE_AGE_MS ||
                Retained.length >= MAX_STATE_DIRECTORIES - 1)) {
            await Removeˉordinaryˉdirectory(State.Path, Family);
        } else {
            Retained.push(State);
            await Pruneˉstateˉcontents(State.Path, Now);
        }
    }
}

function Resultˉpath(Root, Stateˉkey, Owner, Action) {
    const Family = join(resolve(Root), CACHE_FAMILY);
    const Stateˉdirectory = join(Family, Requireˉdigest(Stateˉkey, 'State key'));
    const Actionˉdigest = Digest(
        `${Requireˉowner(Owner)}\0${Requireˉaction(Action)}`
    );
    return {
        Actionˉdigest,
        Family,
        Stateˉdirectory,
        Result: join(Stateˉdirectory, `${Actionˉdigest}.json`),
    };
}

function Stateˉrecordˉpath(Stateˉdirectory) {
    return join(Stateˉdirectory, STATE_RECORD_NAME);
}

async function Readˉstateˉrecord(Stateˉdirectory) {
    const Path = Stateˉrecordˉpath(Stateˉdirectory);
    let Metadata;
    try {
        Metadata = await lstat(Path);
    } catch (Errorˉvalue) {
        if (Errorˉvalue?.code === 'ENOENT') return null;
        throw Errorˉvalue;
    }
    if (!Metadata.isFile() || Metadata.isSymbolicLink() ||
        Metadata.size < 1 || Metadata.size > MAX_STATE_RECORD_BYTES) {
        return null;
    }
    let Record;
    try {
        Record = JSON.parse(await readFile(Path, 'utf8'));
    } catch {
        return null;
    }
    if (Record === null || typeof Record !== 'object' ||
        Object.keys(Record).sort().join(',') !==
            'format,hostKey,repositoryKey,sourceTree,stateKey' ||
        Record.format !== STATE_RECORD_FORMAT ||
        !HEX_DIGEST.test(Record.hostKey) ||
        !HEX_DIGEST.test(Record.repositoryKey) ||
        !GIT_TREE.test(Record.sourceTree) ||
        !HEX_DIGEST.test(Record.stateKey)) {
        return null;
    }
    return Record;
}

async function Ensureˉstateˉrecord(Stateˉdirectory, Expected) {
    const Path = Stateˉrecordˉpath(Stateˉdirectory);
    const Payload = Buffer.from(`${JSON.stringify(Expected)}\n`, 'utf8');
    if (Payload.length > MAX_STATE_RECORD_BYTES) {
        throw new Error('Verification result-cache state record exceeds its bound.');
    }
    const Existing = await Readˉstateˉrecord(Stateˉdirectory);
    if (Existing !== null) {
        if (JSON.stringify(Existing) !== JSON.stringify(Expected)) {
            throw new Error('Verification result-cache state record differs.');
        }
        return;
    }
    const Temporary = join(
        Stateˉdirectory,
        `.state-new-${process.pid}-${randomUUID()}`,
    );
    let File;
    try {
        File = await open(Temporary, 'wx', 0o600);
        await File.writeFile(Payload);
        await File.sync();
        await File.close();
        File = undefined;
        try {
            await rename(Temporary, Path);
        } catch (Errorˉvalue) {
            if (!['EEXIST', 'ENOTEMPTY', 'EPERM', 'EACCES'].includes(
                Errorˉvalue?.code
            )) {
                throw Errorˉvalue;
            }
        }
    } finally {
        await File?.close();
        await rm(Temporary, { force: true });
    }
    const Actual = await Readˉstateˉrecord(Stateˉdirectory);
    if (Actual === null || JSON.stringify(Actual) !== JSON.stringify(Expected)) {
        throw new Error('Verification result-cache state record differs.');
    }
}

export async function Prepareˉverificationˉresultˉcache(
    Repositoryˉinput,
    Cacheˉrootˉinput,
) {
    const Repositoryˉlexical = resolve(Repositoryˉinput);
    const Repository = await realpath(Repositoryˉlexical);
    const Top = await realpath(await Runˉgit(
        Repository,
        ['rev-parse', '--show-toplevel'],
    ));
    if (Top !== Repository) {
        throw new Error('Verification cache repository root is not the Git root.');
    }
    const Sentinelˉbefore = await Measureˉsourceˉsentinel(Repository);
    const Sourceˉtree = await Measureˉsourceˉtree(Repository);
    const Sourceˉsentinel = await Measureˉsourceˉsentinel(Repository);
    if (Sourceˉsentinel !== Sentinelˉbefore) {
        throw new Error(
            'Repository inputs changed while the verification state was measured.'
        );
    }
    const Hostˉidentity = await Measureˉhostˉidentity();
    const Hostˉkey = Digest(JSON.stringify(Hostˉidentity));
    const Repositoryˉkey = Digest(Repository);
    const Stateˉdescriptor = {
        format: STATE_FORMAT,
        repository: Repository,
        sourceTree: Sourceˉtree,
        host: Hostˉidentity,
    };
    const Stateˉkey = Digest(JSON.stringify(Stateˉdescriptor));
    const Rootˉinput = resolve(Cacheˉrootˉinput ?? Defaultˉcacheˉroot());
    if (Isˉinsideˉorˉequal(Rootˉinput, Repository) ||
        Isˉinsideˉorˉequal(Rootˉinput, Repositoryˉlexical)) {
        throw new Error(
            'Verification result cache root must remain outside the repository.'
        );
    }
    const Root = await Ensureˉordinaryˉdirectory(Rootˉinput);
    const Family = await Ensureˉordinaryˉdirectory(join(Root, CACHE_FAMILY));
    await Pruneˉfamily(Family, Stateˉkey);
    const Stateˉdirectory = await Ensureˉordinaryˉdirectory(
        join(Family, Stateˉkey)
    );
    await Ensureˉstateˉrecord(Stateˉdirectory, {
        format: STATE_RECORD_FORMAT,
        stateKey: Stateˉkey,
        sourceTree: Sourceˉtree,
        repositoryKey: Repositoryˉkey,
        hostKey: Hostˉkey,
    });
    const Now = new Date();
    await utimes(Stateˉdirectory, Now, Now);
    return {
        format: STATE_FORMAT,
        root: Root,
        stateKey: Stateˉkey,
        sourceTree: Sourceˉtree,
        sourceSentinel: Sourceˉsentinel,
        repositoryKey: Repositoryˉkey,
        hostKey: Hostˉkey,
    };
}

export async function Probeˉverificationˉresult(
    Root,
    Stateˉkey,
    Owner,
    Action,
) {
    const Paths = Resultˉpath(Root, Stateˉkey, Owner, Action);
    Requireˉcontained(Paths.Stateˉdirectory, Paths.Family);
    Requireˉcontained(Paths.Result, Paths.Stateˉdirectory);
    let Metadata;
    try {
        Metadata = await lstat(Paths.Result);
    } catch (Errorˉvalue) {
        if (Errorˉvalue?.code === 'ENOENT') {
            return false;
        }
        throw Errorˉvalue;
    }
    if (!Metadata.isFile() || Metadata.isSymbolicLink()) {
        throw new Error('Verification result cache entry is not an ordinary file.');
    }
    let Valid = Metadata.size > 0 && Metadata.size <= MAX_RESULT_BYTES;
    let Record;
    if (Valid) {
        try {
            Record = JSON.parse(await readFile(Paths.Result, 'utf8'));
        } catch {
            Valid = false;
        }
    }
    const Keys = Valid && Record !== null && typeof Record === 'object'
        ? Object.keys(Record).sort().join(',')
        : '';
    Valid = Valid &&
        Keys === 'actionDigest,completedUtc,format,owner,stateKey' &&
        Record.format === RESULT_FORMAT &&
        Record.stateKey === Stateˉkey &&
        Record.owner === Owner &&
        Record.actionDigest === Paths.Actionˉdigest &&
        typeof Record.completedUtc === 'string' &&
        Number.isFinite(Date.parse(Record.completedUtc));
    if (!Valid) {
        await rm(Paths.Result, { force: true });
        return false;
    }
    return true;
}

export async function Listˉverificationˉresultˉcandidates(
    Root,
    Currentˉstateˉkey,
    Repositoryˉkey,
    Hostˉkey,
    Owner,
    Action,
) {
    Requireˉdigest(Currentˉstateˉkey, 'Current state key');
    Requireˉdigest(Repositoryˉkey, 'Repository key');
    Requireˉdigest(Hostˉkey, 'Host key');
    Requireˉowner(Owner);
    Requireˉaction(Action);
    const Cacheˉroot = await Ensureˉordinaryˉdirectory(resolve(Root));
    const Family = await Ensureˉordinaryˉdirectory(
        join(Cacheˉroot, CACHE_FAMILY),
    );
    const Entries = await readdir(Family, { withFileTypes: true });
    if (Entries.length > MAX_CACHE_FAMILY_ENTRIES) {
        throw new Error('Verification result-cache family exceeds its entry bound.');
    }
    const States = [];
    for (const Entry of Entries) {
        if (Entry.name === Currentˉstateˉkey ||
            !HEX_DIGEST.test(Entry.name) || Entry.isSymbolicLink() ||
            !Entry.isDirectory()) {
            continue;
        }
        const Stateˉdirectory = join(Family, Entry.name);
        const Record = await Readˉstateˉrecord(Stateˉdirectory);
        if (Record === null || Record.stateKey !== Entry.name ||
            Record.repositoryKey !== Repositoryˉkey ||
            Record.hostKey !== Hostˉkey ||
            !(await Probeˉverificationˉresult(
                Root, Entry.name, Owner, Action
            ))) {
            continue;
        }
        const Metadata = await stat(Stateˉdirectory);
        States.push({
            stateKey: Record.stateKey,
            sourceTree: Record.sourceTree,
            modifiedMilliseconds: Math.trunc(Metadata.mtimeMs),
        });
    }
    States.sort((Left, Right) =>
        Right.modifiedMilliseconds - Left.modifiedMilliseconds ||
        Left.stateKey.localeCompare(Right.stateKey));
    return {
        format: CANDIDATE_FORMAT,
        candidates: States.slice(0, MAX_COMPATIBLE_CANDIDATES),
    };
}

export async function Getˉverificationˉchangedˉpaths(
    Repositoryˉinput,
    Fromˉtree,
    Toˉtree,
) {
    if (!GIT_TREE.test(Fromˉtree) || !GIT_TREE.test(Toˉtree)) {
        throw new Error('Verification compatibility requires two Git tree identities.');
    }
    const Repository = await realpath(resolve(Repositoryˉinput));
    const Top = await realpath(await Runˉgit(
        Repository,
        ['rev-parse', '--show-toplevel'],
    ));
    if (Top !== Repository) {
        throw new Error('Verification compatibility repository root is not the Git root.');
    }
    const Output = await Runˉgitˉbytes(Repository, [
        'diff',
        '--name-only',
        '-z',
        '--no-renames',
        '--diff-filter=ACDMRTUXB',
        Fromˉtree,
        Toˉtree,
        '--',
    ]);
    const Rawˉpaths = Splitˉnulˉpaths(Output);
    if (Rawˉpaths.length > MAX_CHANGED_PATHS) {
        throw new Error('Verification compatibility changed-path count exceeds its bound.');
    }
    const Decoder = new TextDecoder('utf-8', { fatal: true });
    const Paths = Rawˉpaths.map(Raw => {
        const Path = Decoder.decode(Raw);
        if (Path.length === 0 || Path.includes('\\') || Path.startsWith('/') ||
            Path.split('/').some(Part =>
                Part.length === 0 || Part === '.' || Part === '..')) {
            throw new Error('Git returned an invalid verification changed path.');
        }
        return Path;
    });
    return { format: CHANGED_PATH_FORMAT, paths: Paths };
}

export async function Confirmˉverificationˉsourceˉstate(
    Repositoryˉinput,
    Sourceˉsentinel,
) {
    Requireˉdigest(Sourceˉsentinel, 'Source sentinel');
    const Repository = await realpath(resolve(Repositoryˉinput));
    const Top = await realpath(await Runˉgit(
        Repository,
        ['rev-parse', '--show-toplevel'],
    ));
    if (Top !== Repository) {
        throw new Error('Verification confirmation repository root is not the Git root.');
    }
    return (await Measureˉsourceˉsentinel(Repository)) === Sourceˉsentinel;
}

export async function Publishˉverificationˉresult(
    Repositoryˉinput,
    Root,
    Stateˉkey,
    Sourceˉtree,
    Sourceˉsentinel,
    Owner,
    Action,
) {
    Requireˉdigest(Stateˉkey, 'State key');
    if (!GIT_TREE.test(Sourceˉtree)) {
        throw new Error('Source tree must be one Git tree identity.');
    }
    Requireˉdigest(Sourceˉsentinel, 'Source sentinel');
    if ((await Measureˉsourceˉsentinel(resolve(Repositoryˉinput))) !==
        Sourceˉsentinel) {
        return 'StateChanged';
    }
    const Paths = Resultˉpath(Root, Stateˉkey, Owner, Action);
    await Ensureˉordinaryˉdirectory(Paths.Family);
    await Ensureˉordinaryˉdirectory(Paths.Stateˉdirectory);
    const Record = Buffer.from(JSON.stringify({
        format: RESULT_FORMAT,
        stateKey: Stateˉkey,
        owner: Owner,
        actionDigest: Paths.Actionˉdigest,
        completedUtc: new Date().toISOString(),
    }) + '\n');
    if (Record.length > MAX_RESULT_BYTES) {
        throw new Error('Verification result cache record exceeds its bound.');
    }
    const Temporary = join(
        Paths.Stateˉdirectory,
        `.new-${Paths.Actionˉdigest}-${process.pid}-${randomUUID()}`,
    );
    Requireˉcontained(Temporary, Paths.Stateˉdirectory);
    let File;
    try {
        File = await open(Temporary, 'wx', 0o600);
        await File.writeFile(Record);
        await File.sync();
        await File.close();
        File = undefined;
        try {
            await rename(Temporary, Paths.Result);
        } catch (Errorˉvalue) {
            if (Errorˉvalue?.code !== 'EEXIST' &&
                Errorˉvalue?.code !== 'ENOTEMPTY') {
                throw Errorˉvalue;
            }
            if (!(await Probeˉverificationˉresult(
                Root, Stateˉkey, Owner, Action
            ))) {
                throw new Error('A competing verification result was invalid.');
            }
        }
        return 'Stored';
    } finally {
        await File?.close();
        Requireˉcontained(Temporary, Paths.Stateˉdirectory);
        await rm(Temporary, { force: true });
    }
}

export function Getˉverificationˉresultˉpath(
    Root,
    Stateˉkey,
    Owner,
    Action,
) {
    return Resultˉpath(Root, Stateˉkey, Owner, Action).Result;
}

function Failˉusage() {
    process.stderr.write(
        'Usage: node Tools/Native/Verification-Owner-Result-Cache.mjs ' +
        'prepare <repository-root> [cache-root] | ' +
        'probe <cache-root> <state-key> <owner> <action> | ' +
        'candidates <cache-root> <current-state-key> <repository-key> ' +
        '<host-key> <owner> <action> | ' +
        'changes <repository-root> <from-tree> <to-tree> | ' +
        'confirm <repository-root> <source-sentinel> | ' +
        'publish <repository-root> <cache-root> <state-key> ' +
        '<source-tree> <source-sentinel> <owner> <action>\n'
    );
    process.exitCode = 64;
}

async function Main() {
    const Operation = process.argv[2];
    if (Operation === 'prepare' &&
        (process.argv.length === 4 || process.argv.length === 5)) {
        process.stdout.write(JSON.stringify(
            await Prepareˉverificationˉresultˉcache(
                process.argv[3],
                process.argv[4],
            )
        ) + '\n');
    } else if (Operation === 'probe' && process.argv.length === 7) {
        process.stdout.write(
            (await Probeˉverificationˉresult(
                process.argv[3],
                process.argv[4],
                process.argv[5],
                process.argv[6],
            ) ? 'Hit' : 'Miss') + '\n'
        );
    } else if (Operation === 'candidates' && process.argv.length === 9) {
        process.stdout.write(JSON.stringify(
            await Listˉverificationˉresultˉcandidates(
                process.argv[3],
                process.argv[4],
                process.argv[5],
                process.argv[6],
                process.argv[7],
                process.argv[8],
            )
        ) + '\n');
    } else if (Operation === 'changes' && process.argv.length === 6) {
        process.stdout.write(JSON.stringify(
            await Getˉverificationˉchangedˉpaths(
                process.argv[3],
                process.argv[4],
                process.argv[5],
            )
        ) + '\n');
    } else if (Operation === 'confirm' && process.argv.length === 5) {
        process.stdout.write(
            (await Confirmˉverificationˉsourceˉstate(
                process.argv[3],
                process.argv[4],
            ) ? 'Unchanged' : 'Changed') + '\n'
        );
    } else if (Operation === 'publish' && process.argv.length === 10) {
        process.stdout.write(
            await Publishˉverificationˉresult(
                process.argv[3],
                process.argv[4],
                process.argv[5],
                process.argv[6],
                process.argv[7],
                process.argv[8],
                process.argv[9],
            ) + '\n'
        );
    } else {
        Failˉusage();
    }
}

if (process.argv[1] !== undefined &&
    resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
    try {
        await Main();
    } catch (Errorˉvalue) {
        process.stderr.write(
            `Verification result cache failure: ${Errorˉvalue.message}\n`
        );
        process.exitCode = process.exitCode || 1;
    }
}
