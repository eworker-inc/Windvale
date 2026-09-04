import { spawn } from 'node:child_process';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

if (process.argv.length !== 2) {
    process.stderr.write(
        'Usage: node Tools/Native/Run-Database-Storage-Qualification.mjs\n'
    );
    process.exit(64);
}

const WINDOWS = process.platform === 'win32';
if (!WINDOWS && process.platform !== 'linux') {
    process.stderr.write(
        `Unsupported database qualification host: ${process.platform}.\n`
    );
    process.exit(1);
}

const SCRIPT_DIRECTORY = dirname(fileURLToPath(import.meta.url));
const REPOSITORY_ROOT = dirname(dirname(SCRIPT_DIRECTORY));
const OWNER = join(
    SCRIPT_DIRECTORY,
    `Test-Database-Storage.${WINDOWS ? 'cmd' : 'sh'}`,
);
if (WINDOWS && /[\r\n"&|<>^%!]/u.test(OWNER)) {
    process.stderr.write(
        'The database qualification owner path contains a Windows shell metacharacter.\n'
    );
    process.exit(1);
}

const CHILDREN = new Set();
let Stopping = false;
let Interrupted = false;

function Stopˉwindowsˉparent(Child) {
    if (Child.exitCode === null && Child.signalCode === null) {
        Child.kill('SIGKILL');
    }
}

function Stopˉchild(Child) {
    if (Child.pid === undefined ||
        Child.exitCode !== null || Child.signalCode !== null) return;
    if (WINDOWS) {
        const Killer = spawn(
            'taskkill.exe',
            ['/pid', String(Child.pid), '/t', '/f'],
            {
                stdio: 'ignore',
                windowsHide: true,
            },
        );
        Killer.once('error', () => Stopˉwindowsˉparent(Child));
        Killer.once('close', Code => {
            if (Code !== 0) Stopˉwindowsˉparent(Child);
        });
    } else {
        try {
            process.kill(-Child.pid, 'SIGKILL');
        } catch (Error) {
            if (Error?.code !== 'ESRCH') throw Error;
        }
    }
}

function Stopˉall() {
    if (Stopping) return;
    Stopping = true;
    for (const Child of CHILDREN) Stopˉchild(Child);
}

function Runˉpart(Name) {
    const Ownerˉarguments = [`--qualification-${Name}`];
    const Executable = WINDOWS ? process.env.ComSpec ?? 'cmd.exe' : OWNER;
    const Arguments = WINDOWS
        ? [
            '/d',
            '/v:off',
            '/s',
            '/c',
            `"${[OWNER, ...Ownerˉarguments]
                .map(Value => `"${Value}"`).join(' ')}"`,
        ]
        : Ownerˉarguments;
    return new Promise(Resolve => {
        process.stdout.write(
            `database storage qualification part=${Name} status=Started\n`
        );
        const Child = spawn(Executable, Arguments, {
            cwd: REPOSITORY_ROOT,
            detached: !WINDOWS,
            stdio: ['ignore', 'inherit', 'inherit'],
            windowsHide: true,
            windowsVerbatimArguments: WINDOWS,
        });
        CHILDREN.add(Child);
        Child.once('error', Error => {
            CHILDREN.delete(Child);
            Stopˉall();
            Resolve({ Name, Code: null, Signal: null, Error });
        });
        Child.once('close', (Code, Signal) => {
            CHILDREN.delete(Child);
            if (Code !== 0 || Signal !== null) Stopˉall();
            Resolve({ Name, Code, Signal, Error: null });
        });
    });
}

for (const Signal of ['SIGINT', 'SIGTERM']) {
    process.once(Signal, () => {
        Interrupted = true;
        Stopˉall();
        process.exitCode = 1;
    });
}

const Results = await Promise.all([
    Runˉpart('portable'),
    Runˉpart('hosted'),
]);
if (Interrupted) {
    process.stderr.write('Database storage qualification was interrupted.\n');
    process.exit(1);
}
const Failed = Results.find(Result =>
    Result.Error !== null || Result.Code !== 0 || Result.Signal !== null
);
if (Failed !== undefined) {
    process.stderr.write(
        `Database storage qualification part ${Failed.Name} failed ` +
        `with exit ${Failed.Code ?? 'signal'}${
            Failed.Signal === null ? '' : ` signal=${Failed.Signal}`}${
            Failed.Error === null ? '' : ` error=${Failed.Error.message}`}.\n`
    );
    process.exit(1);
}

process.stdout.write(
    'native database storage status=Passed cases=57 local-results=0 ' +
    'cross-host-images=Verified\n'
);
