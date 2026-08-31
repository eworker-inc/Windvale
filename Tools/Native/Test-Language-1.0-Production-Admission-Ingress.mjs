import { createHash } from 'node:crypto';
import { spawn } from 'node:child_process';
import {
    chmod,
    link,
    lstat,
    mkdir,
    mkdtemp,
    readdir,
    readFile,
    realpath,
    rm,
    stat,
    writeFile,
} from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { basename, dirname, join, resolve } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';

const WINDOWS = process.platform === 'win32';
const MAXIMUM_OUTPUT_BYTES = 65_536;
const HEARTBEAT_MILLISECONDS = 30_000;
const TASKKILL_TIMEOUT_MILLISECONDS = 2_000;
const TERMINATION_SETTLE_MILLISECONDS = 5_000;
const BUILD_TIMEOUT_MILLISECONDS = 600_000;
const PACKAGE_TIMEOUT_MILLISECONDS = 1_200_000;
const CASE_TIMEOUT_MILLISECONDS = 30_000;
const SCRIPT_DIRECTORY = dirname(fileURLToPath(import.meta.url));
const REPOSITORY_ROOT = resolve(SCRIPT_DIRECTORY, '..', '..');
const SPLIT_COMPILER = join(SCRIPT_DIRECTORY, 'Run-Split-Compiler.mjs');
const TEMPORARY_PREFIX = 'windvale-production-admission-ingress-';
const COLD_DOUBLE_BUILD_ENVIRONMENT =
    'WINDVALE_PRODUCTION_ADMISSION_INGRESS_COLD_DOUBLE_BUILD';
const EXPECTED_COORDINATOR = Object.freeze({
    bytes: 48_199,
    sha256: '0133ccbd14f0cdc7e7998c996830424ac3d0f2bec9d38df0d7bfaacb97b69634',
});

const PINNED_COMPILER = Object.freeze({
    analyzer: Object.freeze({
        file: 'wvanalyze.wvb',
        role: 'analyzer',
        bytes: 1_552_090,
        sha256: '5baba39b96932eca26d694b537d380f9ee6dcd4683afc81c09a99ab3c3cb9c77',
    }),
    emitter: Object.freeze({
        file: 'wvemit.wvb',
        role: 'emitter',
        bytes: 1_556_434,
        sha256: 'd16cc44f65a788a8c2dc45d423686dde095cac63e8f2fd8305d1246b29c168f9',
    }),
});

// Every run requires the exact recorded portable WVB identities. Native
// application identities are measured and reported, but remain unpinned until
// paired Windows/Linux evidence supplies both host values. Development uses the
// validated shared cache once per product. Explicit qualification builds twice
// through isolated cold caches and additionally requires WVB byte identity.
const EXPECTED_PRODUCTS = Object.freeze({
    wvadmit: Object.freeze({
        bytes: 572_966,
        sha256: 'e9d202c4b6b3f6b90fba3db9462ab9ba7f6d0e76be58884f56f54e80efba749e',
    }),
    wvauth: Object.freeze({
        bytes: 93_436,
        sha256: '6d536c93df19b14ea1c03134614e7889d1b440536e45aa0460f4c1780fe37612',
    }),
    wvanalyze: Object.freeze({
        bytes: 1_573_433,
        sha256: '23d9ec0c223d214a69fcb4179abec5b3b9a6d579d8557f3ccf4248c2904267b6',
    }),
    wvbind: Object.freeze({
        bytes: 980_285,
        sha256: '33ad319280dad9aa7c9ba7888f2c2c7d089b23433db8def9ba7308767d14eb07',
    }),
    wvemit: Object.freeze({
        bytes: 1_575_647,
        sha256: '0972defc2debdad47cd36268516c15d947a364b93aede84f0b55cf17ad061d77',
    }),
});

const CASES = Object.freeze([
    'valid-empty-catalog-end-to-end',
    'foreign-lowering-pending-no-publication',
    'deterministic-snapshots-and-products',
    'removed-admitted-source-set-route',
    'raw-project2-system-rejected',
    'raw-project2-platform-rejected',
    'raw-project2-foreign-rejected',
    'fake-token-certificate-cache-and-path-rejected',
    'wvae-mutation-prevents-analyzer-launch',
    'truncated-and-trailing-evidence-rejected',
    'rehashed-empty-and-remapped-catalog-rejected',
    'wvss-catalog-and-wvtd-target-mismatch-rejected',
    'oversized-source-rejected-before-scan',
    'wvadmit-failure-preserves-destination-and-cleans-private-tree',
    'validator-failure-preserves-destination-and-cleans-private-tree',
    'analyzer-failure-preserves-destination-and-cleans-private-tree',
    'alias-and-preexisting-output-rejected',
    'child-timeout-kills-descendant-tree',
    'output-overflow-kills-child',
    'heartbeat-precedes-bounded-termination',
    'invalid-production-preflight-rejected-before-product-launch',
]);

function Reject(Message) { throw new Error(Message); }

function Require(Condition, Message) {
    if (!Condition) Reject(Message);
}

function Sha256(Value) {
    return createHash('sha256').update(Value).digest('hex');
}

function Processˉisˉlive(Child) {
    return Child.pid !== undefined && Child.exitCode === null &&
        Child.signalCode === null;
}

function Processˉidentifierˉisˉlive(Identifier) {
    try {
        process.kill(Identifier, 0);
        return true;
    } catch (Error) {
        if (Error.code === 'ESRCH') return false;
        return false;
    }
}

function Runˉboundedˉtaskkill(Identifier) {
    return new Promise(Resolveˉresult => {
        const Killer = spawn(
            'taskkill.exe', ['/pid', String(Identifier), '/t', '/f'],
            { stdio: 'ignore', windowsHide: true }
        );
        let Settled = false;
        const Timer = setTimeout(() => {
            if (Settled) return;
            Settled = true;
            Killer.kill('SIGKILL');
            Killer.unref();
            Resolveˉresult('taskkill did not settle within 2000 ms');
        }, TASKKILL_TIMEOUT_MILLISECONDS);
        Killer.once('error', Error => {
            if (Settled) return;
            Settled = true;
            clearTimeout(Timer);
            Resolveˉresult(`taskkill error: ${Error.message}`);
        });
        Killer.once('close', Code => {
            if (Settled) return;
            Settled = true;
            clearTimeout(Timer);
            Resolveˉresult(Code === 0 ? null : `taskkill exited ${Code}`);
        });
    });
}

async function Terminateˉprocessˉtree(Child) {
    if (!Processˉisˉlive(Child)) return null;
    let Diagnostic = null;
    if (WINDOWS) Diagnostic = await Runˉboundedˉtaskkill(Child.pid);
    else {
        try { process.kill(-Child.pid, 'SIGKILL'); }
        catch (Error) { Diagnostic = `process-group kill error: ${Error.message}`; }
    }
    if (Processˉisˉlive(Child)) {
        try { Child.kill('SIGKILL'); }
        catch (Error) {
            Diagnostic = Diagnostic === null
                ? `direct child kill error: ${Error.message}`
                : `${Diagnostic}; direct child kill error: ${Error.message}`;
        }
    }
    return Diagnostic;
}

function Runˉbounded(
    Tool,
    Arguments,
    Activity,
    Timeout = CASE_TIMEOUT_MILLISECONDS,
    Environment = process.env,
    Relay = false,
    Maximumˉoutput = MAXIMUM_OUTPUT_BYTES
) {
    return new Promise((Resolveˉresult, Rejectˉpromise) => {
        const Isˉcommand = WINDOWS && Tool.toLowerCase().endsWith('.cmd');
        if (Isˉcommand && [Tool, ...Arguments].some(
            Argument => /[\r\n&|<>^%!"]/u.test(Argument)
        )) {
            Rejectˉpromise(new Error(
                'A production-ingress owner command contains shell metacharacters.'
            ));
            return;
        }
        const Executable = Isˉcommand ? process.env.ComSpec ?? 'cmd.exe' : Tool;
        const Commandˉarguments = Isˉcommand
            ? ['/d', '/v:off', '/s', '/c', `"${[Tool, ...Arguments]
                .map(Argument => `"${Argument}"`).join(' ')}"`]
            : Arguments;
        const Started = Date.now();
        const Child = spawn(Executable, Commandˉarguments, {
            cwd: REPOSITORY_ROOT,
            detached: !WINDOWS,
            env: Environment,
            stdio: ['ignore', 'pipe', 'pipe'],
            windowsHide: true,
            windowsVerbatimArguments: Isˉcommand,
        });
        const Output = [];
        const Errorˉoutput = [];
        const Heartbeats = [];
        let Captured = 0;
        let Exceeded = false;
        let Timedˉout = false;
        let Settled = false;
        let Closeˉreceived = false;
        let Closeˉcode = null;
        let Cleanupˉfailure = null;
        let Terminationˉpromise = null;
        let Settleˉtimer;
        const Heartbeat = setInterval(() => {
            const Elapsed = Date.now() - Started;
            Heartbeats.push(Elapsed);
            process.stdout.write(
                `INFO  production admission ingress active step=${Activity} ` +
                `elapsed-ms=${Elapsed}\n`
            );
        }, HEARTBEAT_MILLISECONDS);
        Heartbeat.unref();
        const Result = Forced => ({
            cleanupFailure: Cleanupˉfailure,
            code: Closeˉcode,
            elapsed: Date.now() - Started,
            error: Buffer.concat(Errorˉoutput),
            exceeded: Exceeded,
            forced: Forced,
            heartbeats: Heartbeats,
            output: Buffer.concat(Output),
            timedOut: Timedˉout,
        });
        const Complete = Forced => {
            if (Settled) return;
            Settled = true;
            clearTimeout(Timer);
            clearInterval(Heartbeat);
            if (Settleˉtimer !== undefined) clearTimeout(Settleˉtimer);
            Resolveˉresult(Result(Forced));
        };
        const Terminateˉandˉsettle = () => {
            if (Terminationˉpromise !== null) return;
            Settleˉtimer = setTimeout(() => {
                try { Child.kill('SIGKILL'); } catch { /* recorded below */ }
                Cleanupˉfailure = Cleanupˉfailure ??
                    `process tree did not close within ` +
                    `${TERMINATION_SETTLE_MILLISECONDS} ms`;
                Child.stdout.destroy();
                Child.stderr.destroy();
                Child.unref();
                Complete(true);
            }, TERMINATION_SETTLE_MILLISECONDS);
            Terminationˉpromise = (async () => {
                Cleanupˉfailure = await Terminateˉprocessˉtree(Child);
                if (Closeˉreceived) Complete(false);
            })().catch(Error => {
                Cleanupˉfailure = `tree termination error: ${Error.message}`;
                if (Closeˉreceived) Complete(false);
            });
        };
        const Append = (Destination, Chunk) => {
            Captured += Chunk.length;
            if (Captured <= Maximumˉoutput) {
                Destination.push(Chunk);
                if (Relay) process.stdout.write(Chunk);
            } else if (!Exceeded) {
                Exceeded = true;
                Terminateˉandˉsettle();
            }
        };
        const Timer = setTimeout(() => {
            Timedˉout = true;
            Terminateˉandˉsettle();
        }, Timeout);
        Child.stdout.on('data', Chunk => Append(Output, Chunk));
        Child.stderr.on('data', Chunk => Append(Errorˉoutput, Chunk));
        Child.once('error', Error => {
            if (Timedˉout || Exceeded || Settled) return;
            clearTimeout(Timer);
            clearInterval(Heartbeat);
            Rejectˉpromise(Error);
        });
        Child.once('close', Code => {
            Closeˉreceived = true;
            Closeˉcode = Code;
            if (Terminationˉpromise === null) Complete(false);
            else void Terminationˉpromise.then(() => Complete(false));
        });
    });
}

function Requireˉcleanˉtermination(Result, Label) {
    if (Result.cleanupFailure !== null) {
        Reject(`${Label} cleanup failed: ${Result.cleanupFailure}.`);
    }
    if (Result.forced) Reject(`${Label} required the forced-settle fallback.`);
}

async function Requireˉsuccess(
    Label,
    Tool,
    Arguments,
    Timeout,
    Relay = false,
    Environment = process.env
) {
    const Result = await Runˉbounded(
        Tool, Arguments, Label, Timeout, Environment, Relay
    );
    Requireˉcleanˉtermination(Result, Label);
    if (Result.timedOut || Result.exceeded || Result.code !== 0 ||
        Result.error.length !== 0) {
        Reject(
            `${Label} failed exit=${Result.code} timeout=${Result.timedOut} ` +
            `overflow=${Result.exceeded}.\n` + Result.error.toString('utf8') +
            Result.output.toString('utf8')
        );
    }
    return Result;
}

async function Evidence(Path) {
    const Information = await stat(Path);
    Require(Information.isFile() && Information.size > 0 &&
        Information.size <= 67_108_864, `Invalid bounded product: ${Path}`);
    const Value = await readFile(Path);
    return { bytes: Value.length, sha256: Sha256(Value), value: Value };
}

function Pinsˉareˉcomplete() {
    return Object.values(EXPECTED_PRODUCTS).every(Product =>
        Number.isSafeInteger(Product.bytes) && Product.bytes > 0 &&
        /^[0-9a-f]{64}$/u.test(Product.sha256)
    );
}

function Requireˉcompleteˉpins() {
    if (Pinsˉareˉcomplete()) return;
    Reject(
        'production admission ingress pins status=Missing ' +
        'products=wvadmit,wvauth,wvanalyze,wvbind,wvemit ' +
        'reason=incomplete-product-identity'
    );
}

function Getˉproductˉbuildˉmode() {
    const Requested = process.env[COLD_DOUBLE_BUILD_ENVIRONMENT];
    Require(Requested === undefined || Requested === '' || Requested === '0' ||
        Requested === '1',
    `${COLD_DOUBLE_BUILD_ENVIRONMENT} must be 0 or 1 when set.`);
    const Coldˉdoubleˉbuild = Requested === '1';
    return Object.freeze({
        coldDoubleBuild: Coldˉdoubleˉbuild,
        name: Coldˉdoubleˉbuild
            ? 'qualification-cold-double-build'
            : 'development-cache',
        buildsPerProduct: Coldˉdoubleˉbuild ? 2 : 1,
    });
}

async function Verifyˉcontracts() {
    Require(CASES.length === 21 && new Set(CASES).size === 21,
        'The production-ingress case inventory is not exactly 21 unique cases.');
    const Splitˉbytes = await readFile(SPLIT_COMPILER);
    Require(Splitˉbytes.length === EXPECTED_COORDINATOR.bytes &&
        Sha256(Splitˉbytes) === EXPECTED_COORDINATOR.sha256,
    `Run-Split-Compiler identity differs: bytes=${Splitˉbytes.length} ` +
        `sha256=${Sha256(Splitˉbytes)}.`);
    const Split = Splitˉbytes.toString('utf8');
    const Admission = await readFile(join(
        REPOSITORY_ROOT, 'Tools', 'Windvale.Build',
        'Compiler-Admission-Driver.wv'
    ), 'utf8');
    const Authentication = await readFile(join(
        REPOSITORY_ROOT, 'Tools', 'Windvale.Build',
        'Compiler-Source-Authenticator-Driver.wv'
    ), 'utf8');
    const Analyzer = await readFile(join(
        REPOSITORY_ROOT, 'Tools', 'Windvale.Build',
        'Compiler-Analysis-Driver.wv'
    ), 'utf8');
    const Binder = await readFile(join(
        REPOSITORY_ROOT, 'Tools', 'Windvale.Build',
        'Compiler-Foreign-Binding-Driver.wv'
    ), 'utf8');
    for (const Required of [
        "'--target-descriptor'", "'--internal-source-set'",
        "'--foreign-binder'", "'source-authentication'",
        "'source-foreign-binding'", "'source-analysis'",
        'Foreignˉloweringˉpending', 'Analyzed.wvss',
        'Buildˉforeignˉbindingˉevidence',
        'foreign-binder evidence does not exactly match',
        'retained authenticated inputs',
        'authenticated foreign catalog requires',
        'The split compiler output must be a new .wvb path.',
        'paths must be distinct.',
        'PUBLICATION_PREFIX', 'Terminateˉprocessˉtree',
        'HEARTBEAT_INTERVAL_MILLISECONDS', 'MAXIMUM_DIAGNOSTIC_BYTES',
    ]) Require(Split.includes(Required), `Run-Split-Compiler lacks ${Required}.`);
    Require(Split.indexOf("'source-authentication'") <
        Split.indexOf("'source-analysis'"),
    'The split compiler does not authenticate before Analyzer launch.');
    Require(Split.indexOf("'source-authentication'") <
        Split.indexOf("'source-foreign-binding'"),
    'The split compiler does not authenticate before foreign binding.');
    Require(!Split.includes('--admission-token') &&
        !Split.includes('--admission-certificate') &&
        !Split.includes('--admission-cache-key'),
    'The public coordinator recognizes forgeable admission material.');
    Require(Admission.includes('--target-descriptor') &&
        Admission.includes('<output.wvae>'),
    'wvadmit does not expose the four-value target-aware contract.');
    Require(Authentication.includes('Usage: wvauth ') &&
        Authentication.includes('Source-Catalog'),
    'wvauth lacks the source/catalog authentication boundary.');
    for (const Required of [
        'admitted-source-set-route-removed', '--internal-source-set',
        'Foreignˉrequiresˉauthenticatedˉbinding',
        'RawˉSystemˉprofile', 'Rawˉplatform', 'Rawˉforeign',
    ]) Require(Analyzer.includes(Required), `Analyzer ingress lacks ${Required}.`);
    for (const Required of [
        'Usage: wvbind ', 'Compilerˉbindˉsourceˉforeignˉdeclarations',
    ]) Require(Binder.includes(Required), `Binder ingress lacks ${Required}.`);
}

async function Removeˉwork(Work, Temporaryˉroot) {
    const Realˉroot = await realpath(Temporaryˉroot);
    const Information = await lstat(Work);
    const Realˉwork = await realpath(Work);
    Require(Information.isDirectory() && !Information.isSymbolicLink() &&
        dirname(Realˉwork) === Realˉroot &&
        basename(Realˉwork).startsWith(TEMPORARY_PREFIX),
    `Refusing to remove unexpected temporary path: ${Work}`);
    await rm(Realˉwork, { recursive: true, force: false, maxRetries: 2 });
}

async function Writeˉsentinel(Work, Name, Source) {
    const Path = join(Work, `${Name}.mjs`);
    await writeFile(Path, Source, { flag: 'wx', mode: 0o700 });
    if (!WINDOWS) await chmod(Path, 0o700);
    return Path;
}

async function Writeˉproductˉsentinel(Work, Name, Source) {
    const Script = await Writeˉsentinel(Work, Name, Source);
    const Command = join(Work, `${Name}.${WINDOWS ? 'cmd' : 'sh'}`);
    const Quoteˉshell = Value => `'${Value.replaceAll("'", "'\"'\"'")}'`;
    const Wrapper = WINDOWS
        ? `@echo off\r\n"${process.execPath}" "${Script}" %*\r\n`
        : `#!/bin/sh\nexec ${Quoteˉshell(process.execPath)} ` +
            `${Quoteˉshell(Script)} "$@"\n`;
    await writeFile(Command, Wrapper, { flag: 'wx', mode: 0o700 });
    if (!WINDOWS) await chmod(Command, 0o700);
    return Command;
}

async function Writeˉsentinelˉinputs(Work) {
    const Paths = {
        lock: join(Work, 'Sentinel.wvlock'),
        profile: join(Work, 'Sentinel.wvsp'),
        target: join(Work, 'Sentinel.wvtd'),
        source: join(Work, 'Sentinel.wv'),
    };
    await Promise.all([
        writeFile(Paths.lock, 'lock', { flag: 'wx' }),
        writeFile(Paths.profile, 'profile', { flag: 'wx' }),
        writeFile(Paths.target, Buffer.alloc(64), { flag: 'wx' }),
        writeFile(Paths.source, 'module Sentinel;', { flag: 'wx' }),
    ]);
    return { ...Paths, lockDigest: '0'.repeat(64) };
}

async function Writeˉsentinelˉpipeline(
    Work, Failureˉrole, Destination, Catalogˉrecords = 0
) {
    const Marker = Role => join(Work, `${Role}-launched`);
    const Fail = Role => Failureˉrole === Role
        ? `await writeFile(${JSON.stringify(Destination)},` +
            `Buffer.from('preserved-${Role}'),{flag:'wx'});` +
            `process.stderr.write('${Role} rejected\\n');process.exitCode=17;`
        : null;
    const Admitterˉfailure = Fail('wvadmit');
    const Validatorˉfailure = Fail('validator');
    const Analyzerˉfailure = Fail('Analyzer');
    const Admitter = await Writeˉproductˉsentinel(Work, 'Sentinel-admitter',
        `import{copyFile,writeFile}from'node:fs/promises';` +
        `await writeFile(${JSON.stringify(Marker('wvadmit'))},Buffer.alloc(0),` +
        `{flag:'a'});${Admitterˉfailure ??
            "const a=process.argv.slice(2);await copyFile(a[8],a[9]);" +
            "await copyFile(a[6],a[10]);const c=Buffer.alloc(48);" +
            `c.writeUInt32LE(${Catalogˉrecords},12);await writeFile(a[11],c,` +
            "{flag:'wx'});await writeFile(a[12],Buffer.alloc(224),{flag:'wx'});" +
            "process.stdout.write('admission ok\\n');"}\n`);
    const Validator = await Writeˉproductˉsentinel(Work, 'Sentinel-validator',
        `import{writeFile}from'node:fs/promises';` +
        `await writeFile(${JSON.stringify(Marker('validator'))},Buffer.alloc(0),` +
        `{flag:'a'});${Validatorˉfailure ??
            "process.stdout.write('authentication ok\\n');"}\n`);
    const Analyzer = await Writeˉproductˉsentinel(Work, 'Sentinel-analyzer',
        `import{copyFile,writeFile}from'node:fs/promises';` +
        `const a=process.argv.slice(2);await writeFile(` +
        `${JSON.stringify(Marker('Analyzer'))},Buffer.alloc(0),{flag:'a'});` +
        `${Analyzerˉfailure ??
            "await copyFile(a[1],a[2]);" +
            "await writeFile(a[3],Buffer.alloc(104),{flag:'wx'});" +
            "await writeFile(a[4],Buffer.from([1]),{flag:'wx'});" +
            "await writeFile(a[5],Buffer.from([1]),{flag:'wx'});" +
            "process.stdout.write('analysis ok\\n');"}\n`);
    const Binder = await Writeˉproductˉsentinel(Work, 'Sentinel-binder',
        `import{createHash}from'node:crypto';` +
        `import{readFile,writeFile}from'node:fs/promises';` +
        `const a=process.argv.slice(2);await writeFile(` +
        `${JSON.stringify(Marker('foreignBinding'))},Buffer.alloc(0),{flag:'a'});` +
        `const[s,t,c]=await Promise.all(a.map(p=>readFile(p)));` +
        `const h=b=>createHash('sha256').update(b).digest('hex');` +
        "process.stdout.write('foreign binding status=Published '+" +
        "`source-bytes=${s.length} source-sha256=${h(s)} `+" +
        "`target-bytes=${t.length} target-sha256=${h(t)} `+" +
        "`catalog-bytes=${c.length} catalog-sha256=${h(c)} `+" +
        "`foreign-count=${c.readUInt32LE(12)}\\n`);\n"
    );
    const Emitter = await Writeˉproductˉsentinel(Work, 'Sentinel-emitter',
        `import{writeFile}from'node:fs/promises';` +
        `await writeFile(${JSON.stringify(Marker('emitter'))},Buffer.alloc(0),` +
        `{flag:'a'});const a=process.argv.slice(2);` +
        `await writeFile(a[4],Buffer.from('P'),{flag:'wx'});` +
        `process.stdout.write('emission ok\\n');\n`);
    return {
        products: { wvadmit: Admitter, wvauth: Validator,
            wvanalyze: Analyzer, wvbind: Binder, wvemit: Emitter },
        markers: ['wvadmit', 'validator', 'Analyzer', 'foreignBinding',
            'emitter'].reduce(
            (Result, Role) => ({ ...Result, [Role]: Marker(Role) }), {}
        ),
    };
}

async function Runˉsentinelˉcoordinator(
    Work, Products, Inputs, Output, Activity, Timeout,
    Outerˉtimeout = 10_000, Checkˉcandidate = true,
    Coordinatorˉarguments = null, Environmentˉoverrides = null,
    Importedˉtestˉhooks = null,
    Captureˉlimit = MAXIMUM_OUTPUT_BYTES,
    Expectˉretainedˉprivateˉtree = false
) {
    const Temporary = join(Work, `Runner-Temporary-${Activity.replace(
        /[^A-Za-z0-9-]/gu, '-'
    )}`);
    await mkdir(Temporary);
    const Environment = {
        ...process.env,
        TEMP: Temporary,
        TMP: Temporary,
        TMPDIR: Temporary,
        WINDVALE_SPLIT_COMPILER_TEST_TIMEOUT_MILLISECONDS: String(Timeout),
        ...(Environmentˉoverrides ?? {}),
    };
    const Coordinatorˉinvocation = Coordinatorˉarguments ??
        Authenticatedˉarguments(Products, Inputs, Inputs.source, Output);
    let Invocation = Coordinatorˉinvocation;
    if (Importedˉtestˉhooks !== null) {
        Environment.WINDVALE_SPLIT_COMPILER_IMPORTED_TEST_HOOKS =
            JSON.stringify(Importedˉtestˉhooks.hooks);
        Invocation = [
            '--import', pathToFileURL(Importedˉtestˉhooks.preload).href,
            ...Coordinatorˉinvocation,
        ];
    }
    const Result = await Runˉbounded(
        process.execPath,
        Invocation,
        Activity, Outerˉtimeout, Environment, false, Captureˉlimit
    );
    Requireˉcleanˉtermination(Result, Activity);
    const Temporaryˉentries = await readdir(Temporary);
    if (Expectˉretainedˉprivateˉtree) {
        Require(Temporaryˉentries.length === 1 &&
            Temporaryˉentries[0].startsWith('windvale-split-compiler-'),
        `${Activity} did not retain exactly one private split-compiler tree.`);
    } else {
        Require(Temporaryˉentries.length === 0,
            `${Activity} left private split-compiler entries=` +
            `${Temporaryˉentries.join(',')} stdout=` +
            `${Result.output.toString('utf8')} stderr=${Result.error.toString('utf8')}`);
    }
    if (Checkˉcandidate) {
        Require(!(await readdir(dirname(Output))).some(Name =>
            Name.startsWith('.new-windvale-split-compiler-')),
        `${Activity} left a publication candidate.`);
    }
    return Result;
}

async function Writeˉsplitˉcompilerˉtestˉpreload(Work) {
    const Candidate = join(Work, 'Split-Compiler-Test-Hooks.mjs');
    await writeFile(
        Candidate,
        "const n='WINDVALE_SPLIT_COMPILER_IMPORTED_TEST_HOOKS';\n" +
        "const r=process.env[n];\n" +
        "if(r===undefined)throw new Error('missing imported test hooks');\n" +
        "delete process.env[n];\n" +
        "const h=JSON.parse(r);\n" +
        "globalThis[Symbol.for('windvale.split-compiler.test-hooks.v1')]=\n" +
        "Object.freeze(h);\n",
        { encoding: 'ascii', flag: 'wx' },
    );
    return Candidate;
}

async function Caseˉpreflightˉrejections(Work) {
    const Definitions = [
        {
            name: 'zero-sources',
            diagnostic: 'authenticated source closure must contain 1 through 64 modules.',
            prepare: async () => ({ sources: [] }),
        },
        {
            name: 'sixty-five-sources',
            diagnostic: 'authenticated source closure must contain 1 through 64 modules.',
            prepare: async Inputs => ({ sources: Array(65).fill(Inputs.source) }),
        },
        {
            name: 'noncanonical-digest',
            diagnostic: 'source-input lock digest must be canonical lowercase SHA-256.',
            prepare: async () => ({ digest: 'A'.repeat(64) }),
        },
        {
            name: 'empty-lock',
            diagnostic: 'source-input lock is not a bounded ordinary file',
            prepare: async Inputs => {
                await writeFile(Inputs.lock, Buffer.alloc(0));
                return {};
            },
        },
        {
            name: 'oversized-lock',
            diagnostic: 'source-input lock is not a bounded ordinary file',
            prepare: async Inputs => {
                await writeFile(Inputs.lock, Buffer.alloc(1_048_577));
                return {};
            },
        },
        {
            name: 'empty-profile',
            diagnostic: 'source profile is not a bounded ordinary file',
            prepare: async Inputs => {
                await writeFile(Inputs.profile, Buffer.alloc(0));
                return {};
            },
        },
        {
            name: 'oversized-profile',
            diagnostic: 'source profile is not a bounded ordinary file',
            prepare: async Inputs => {
                await writeFile(Inputs.profile, Buffer.alloc(65_537));
                return {};
            },
        },
        {
            name: 'short-target',
            diagnostic: 'target descriptor is not a bounded ordinary file',
            prepare: async Inputs => {
                await writeFile(Inputs.target, Buffer.alloc(63));
                return {};
            },
        },
        {
            name: 'oversized-target',
            diagnostic: 'target descriptor is not a bounded ordinary file',
            prepare: async Inputs => {
                await writeFile(Inputs.target, Buffer.alloc(321));
                return {};
            },
        },
        {
            name: 'empty-source',
            diagnostic: 'source module 0 is not a bounded ordinary file',
            prepare: async Inputs => {
                await writeFile(Inputs.source, Buffer.alloc(0));
                return {};
            },
        },
        {
            name: 'aggregate-wvss-overflow',
            diagnostic: 'source closure exceeds the 4 MiB canonical WVSS bound.',
            prepare: async Inputs => {
                // The source itself is exactly at the per-file maximum. Its
                // one-entry WVSS directory makes the aggregate one-past.
                await writeFile(Inputs.source, Buffer.alloc(4_194_304, 32));
                return {};
            },
        },
        {
            name: 'duplicate-input-path',
            diagnostic: 'authenticated input paths must be distinct.',
            prepare: async Inputs => ({ profile: Inputs.lock }),
        },
        {
            name: 'hard-link-input-alias',
            diagnostic: 'authenticated input files must not be hard-link aliases.',
            prepare: async (Inputs, Caseˉwork) => {
                const Alias = join(Caseˉwork, 'Sentinel-Alias.wv');
                await link(Inputs.source, Alias);
                return { sources: [Inputs.source, Alias] };
            },
        },
    ];
    for (const Definition of Definitions) {
        const Caseˉwork = join(Work, `Sentinel-preflight-${Definition.name}`);
        await mkdir(Caseˉwork);
        const Inputs = await Writeˉsentinelˉinputs(Caseˉwork);
        const Pipeline = await Writeˉsentinelˉpipeline(
            Caseˉwork, null, join(Caseˉwork, 'unused.wvb')
        );
        const Override = await Definition.prepare(Inputs, Caseˉwork);
        const Lock = Override.lock ?? Inputs.lock;
        const Digest = Override.digest ?? Inputs.lockDigest;
        const Profile = Override.profile ?? Inputs.profile;
        const Target = Override.target ?? Inputs.target;
        const Sources = Override.sources ?? [Inputs.source];
        const Output = join(Caseˉwork, 'Rejected.wvb');
        const Arguments = [
            SPLIT_COMPILER,
            Pipeline.products.wvadmit, Pipeline.products.wvauth,
            Pipeline.products.wvanalyze, Pipeline.products.wvemit,
            '--source-input-lock', Lock, Digest,
            '--source-profile', Profile,
            '--target-descriptor', Target,
            ...Sources, Output,
        ];
        const Protected = [...new Set([Lock, Profile, Target, ...Sources])];
        const Before = new Map(await Promise.all(Protected.map(async Path =>
            [Path, await readFile(Path)]
        )));
        const Result = await Runˉsentinelˉcoordinator(
            Caseˉwork, Pipeline.products, Inputs, Output,
            `${CASES[20]}-${Definition.name}`, 5_000, 10_000, true, Arguments
        );
        const Diagnostic = Buffer.concat([Result.output, Result.error])
            .toString('utf8');
        Require(Result.code !== 0 && Diagnostic.includes(Definition.diagnostic),
            `${Definition.name} did not take its exact preflight rejection.`);
        Require(!await lstat(Output).then(() => true, () => false),
            `${Definition.name} created a public destination.`);
        for (const Role of [
            'wvadmit', 'validator', 'foreignBinding', 'Analyzer', 'emitter',
        ]) {
            Require(!await lstat(Pipeline.markers[Role]).then(
                () => true, () => false
            ), `${Definition.name} launched ${Role}.`);
        }
        for (const Path of Protected) {
            Require((await readFile(Path)).equals(Before.get(Path)),
                `${Definition.name} changed authenticated input ${Path}.`);
        }
    }
}

async function Caseˉpreflightˉmaxima(Work) {
    const Caseˉwork = join(Work, 'Sentinel-preflight-exact-maxima');
    await mkdir(Caseˉwork);
    const Inputs = await Writeˉsentinelˉinputs(Caseˉwork);
    const Pipeline = await Writeˉsentinelˉpipeline(
        Caseˉwork, null, join(Caseˉwork, 'unused.wvb')
    );
    await Promise.all([
        writeFile(Inputs.lock, Buffer.alloc(1_048_576, 76)),
        writeFile(Inputs.profile, Buffer.alloc(65_536, 80)),
        writeFile(Inputs.target, Buffer.alloc(320)),
    ]);

    const Sourceˉcount = 64;
    const Sourceˉpayloadˉbudget = 4_194_304 -
        (16 + Sourceˉcount * 8);
    const Sources = Array.from(
        { length: Sourceˉcount },
        (_, Index) => join(Caseˉwork, `Maximum-${Index}.wv`)
    );
    await Promise.all(Sources.map((Path, Index) => writeFile(
        Path,
        Buffer.alloc(
            Index + 1 === Sourceˉcount
                ? Sourceˉpayloadˉbudget - (Sourceˉcount - 1)
                : 1,
            32
        ),
        { flag: 'wx' }
    )));

    const Output = join(Caseˉwork, 'Maximum.wvb');
    const Arguments = [
        SPLIT_COMPILER,
        Pipeline.products.wvadmit, Pipeline.products.wvauth,
        Pipeline.products.wvanalyze, Pipeline.products.wvemit,
        '--source-input-lock', Inputs.lock, Inputs.lockDigest,
        '--source-profile', Inputs.profile,
        '--target-descriptor', Inputs.target,
        ...Sources, Output,
    ];
    const Protected = [Inputs.lock, Inputs.profile, Inputs.target, ...Sources];
    const Before = new Map(await Promise.all(Protected.map(async Path =>
        [Path, Sha256(await readFile(Path))]
    )));
    const Result = await Runˉsentinelˉcoordinator(
        Caseˉwork, Pipeline.products, Inputs, Output,
        `${CASES[20]}-exact-maxima`, 5_000, 15_000, true, Arguments
    );
    Requireˉcleanˉtermination(Result, `${CASES[20]}-exact-maxima`);
    Require(Result.code === 0 &&
        (await readFile(Output)).equals(Buffer.from('P')),
    'The exact preflight maxima did not reach one successful publication.');
    for (const Role of ['wvadmit', 'validator', 'Analyzer', 'emitter']) {
        Require(await lstat(Pipeline.markers[Role]).then(
            () => true, () => false
        ), `The exact preflight maxima did not launch ${Role}.`);
    }
    for (const Path of Protected) {
        Require(Sha256(await readFile(Path)) === Before.get(Path),
            `The exact preflight maxima changed authenticated input ${Path}.`);
    }
}

async function Waitˉforˉexit(Identifier) {
    const Deadline = Date.now() + 2_000;
    while (Processˉidentifierˉisˉlive(Identifier)) {
        if (Date.now() >= Deadline) return false;
        await new Promise(Resolveˉwait => setTimeout(Resolveˉwait, 25));
    }
    return true;
}

async function Caseˉtimeoutˉtree(Work) {
    const Caseˉwork = join(Work, 'Sentinel-timeout-tree');
    await mkdir(Caseˉwork);
    const Inputs = await Writeˉsentinelˉinputs(Caseˉwork);
    const Pipeline = await Writeˉsentinelˉpipeline(
        Caseˉwork, null, join(Caseˉwork, 'unused.wvb')
    );
    const Descendant = join(Caseˉwork, 'Descendant.pid');
    Pipeline.products.wvadmit = await Writeˉproductˉsentinel(
        Caseˉwork, 'Timeout-admitter',
        "import{spawn}from'node:child_process';import{writeFile}from'node:fs/promises';" +
        "const c=spawn(process.execPath,['-e','setInterval(()=>{},1000)']," +
        `{stdio:'ignore'});await writeFile(${JSON.stringify(Descendant)},` +
        "String(c.pid),{flag:'wx'});setInterval(()=>{},1000);\n"
    );
    const Output = join(Caseˉwork, 'Timeout.wvb');
    const Result = await Runˉsentinelˉcoordinator(
        Caseˉwork, Pipeline.products, Inputs, Output, CASES[17], 500
    );
    Require(Result.code !== 0 && !Result.timedOut && !Result.exceeded &&
        Result.error.toString('utf8').includes('status=Timeout'),
        'The timeout sentinel did not take the timeout path.');
    const Identifier = Number.parseInt(await readFile(Descendant, 'utf8'), 10);
    Require(Number.isSafeInteger(Identifier) && Identifier > 0,
        'The timeout sentinel did not report a descendant identifier.');
    Require(await Waitˉforˉexit(Identifier),
        'The timeout path left its descendant running.');
}

async function Caseˉterminationˉfailureˉretainsˉtree(Work) {
    const Caseˉwork = join(Work, 'Sentinel-termination-failure');
    await mkdir(Caseˉwork);
    const Inputs = await Writeˉsentinelˉinputs(Caseˉwork);
    const Pipeline = await Writeˉsentinelˉpipeline(
        Caseˉwork, null, join(Caseˉwork, 'unused.wvb')
    );
    const Descendant = join(Caseˉwork, 'Descendant.pid');
    Pipeline.products.wvadmit = await Writeˉproductˉsentinel(
        Caseˉwork, 'Termination-failure-admitter',
        "import{spawn}from'node:child_process';import{writeFile}from'node:fs/promises';" +
        "const c=spawn(process.execPath,['-e','setInterval(()=>{},1000)']," +
        `{stdio:'ignore'});await writeFile(${JSON.stringify(Descendant)},` +
        "String(c.pid),{flag:'wx'});setInterval(()=>{},1000);\n"
    );
    const Testˉpreload = await Writeˉsplitˉcompilerˉtestˉpreload(
        Caseˉwork
    );
    const Output = join(Caseˉwork, 'Termination-failure.wvb');
    const Result = await Runˉsentinelˉcoordinator(
        Caseˉwork, Pipeline.products, Inputs, Output,
        'forced-settle-termination-failure', 500, 12_000, true, null, null,
        {
            preload: Testˉpreload,
            hooks: {
                candidateFailure: null,
                cleanupFailureAfterRemoval: false,
                postLinkCandidateRemoval: false,
                postPublicationCleanup: null,
                temporaryIdentityFailure: false,
                terminationSettleFailure: true,
            },
        },
        MAXIMUM_OUTPUT_BYTES, true
    );
    const Diagnostic = Result.error.toString('utf8');
    Require(Result.code !== 0 && !Result.timedOut && !Result.exceeded &&
        Diagnostic.includes('status=Terminationˉfailure') &&
        !Diagnostic.includes('status=Timeout') &&
        !Diagnostic.includes('status=Outputˉlimit') &&
        !await lstat(Output).then(() => true, () => false),
    'Forced settle masked termination failure or removed the private tree.');
    const Identifier = Number.parseInt(await readFile(Descendant, 'utf8'), 10);
    Require(Number.isSafeInteger(Identifier) && Identifier > 0 &&
        await Waitˉforˉexit(Identifier),
    'The forced-settle sentinel left its controlled descendant running.');
}

async function Caseˉoverflow(Work) {
    const Caseˉwork = join(Work, 'Sentinel-overflow');
    await mkdir(Caseˉwork);
    const Inputs = await Writeˉsentinelˉinputs(Caseˉwork);
    const Pipeline = await Writeˉsentinelˉpipeline(
        Caseˉwork, null, join(Caseˉwork, 'unused.wvb')
    );
    const Descendant = join(Caseˉwork, 'Descendant.pid');
    Pipeline.products.wvadmit = await Writeˉproductˉsentinel(
        Caseˉwork, 'Overflow-admitter',
        "import{spawn}from'node:child_process';import{writeFile}from'node:fs/promises';" +
        "const c=spawn(process.execPath,['-e','setInterval(()=>{},1000)']," +
        `{stdio:'ignore'});await writeFile(${JSON.stringify(Descendant)},` +
        "String(c.pid),{flag:'wx'});const b=Buffer.alloc(8192,120);" +
        "setInterval(()=>process.stdout.write(b),1);\n"
    );
    const Output = join(Caseˉwork, 'Overflow.wvb');
    const Result = await Runˉsentinelˉcoordinator(
        Caseˉwork, Pipeline.products, Inputs, Output, CASES[18], 5_000
    );
    Require(Result.code !== 0 && !Result.timedOut && !Result.exceeded &&
        Result.error.toString('utf8').includes('status=Outputˉlimit'),
        'The overflow sentinel did not take the output-limit path.');
    Require(Result.elapsed < 5_000,
        'The output-limit sentinel was not terminated promptly.');
    const Identifier = Number.parseInt(await readFile(Descendant, 'utf8'), 10);
    Require(await Waitˉforˉexit(Identifier),
        'The output-limit path left its descendant running.');
}

async function Caseˉheartbeat(Work) {
    const Caseˉwork = join(Work, 'Sentinel-heartbeat');
    await mkdir(Caseˉwork);
    const Inputs = await Writeˉsentinelˉinputs(Caseˉwork);
    const Pipeline = await Writeˉsentinelˉpipeline(
        Caseˉwork, null, join(Caseˉwork, 'unused.wvb')
    );
    Pipeline.products.wvadmit = await Writeˉproductˉsentinel(
        Caseˉwork, 'Heartbeat-admitter', 'setInterval(()=>{},1000);\n'
    );
    const Output = join(Caseˉwork, 'Heartbeat.wvb');
    const Result = await Runˉsentinelˉcoordinator(
        Caseˉwork, Pipeline.products, Inputs, Output, CASES[19],
        HEARTBEAT_MILLISECONDS + 1_500, HEARTBEAT_MILLISECONDS + 8_000
    );
    const Activity = [...Result.output.toString('utf8').matchAll(
        /INFO  split compiler active step=source-admission elapsed-ms=([0-9]+)/gu
    )].map(Match => Number.parseInt(Match[1], 10));
    Require(Result.code !== 0 && Activity.some(Elapsed =>
        Elapsed >= HEARTBEAT_MILLISECONDS - 500 &&
        Elapsed <= HEARTBEAT_MILLISECONDS + 500),
    'The real coordinator emitted no bounded 30-second heartbeat.');
    Require(Result.elapsed <= HEARTBEAT_MILLISECONDS + 7_000,
        'The heartbeat sentinel did not terminate within its settle ceiling.');
}

async function Caseˉphaseˉlifecycle(Work, Index, Step) {
    const Caseˉwork = join(Work, `Sentinel-phase-${Step}`);
    await mkdir(Caseˉwork);
    const Destination = join(Caseˉwork, `Competing-${Step}.wvb`);
    const Original = Buffer.from(`preserved-${Step}`, 'utf8');
    const Inputs = await Writeˉsentinelˉinputs(Caseˉwork);
    const Pipeline = await Writeˉsentinelˉpipeline(
        Caseˉwork, Step, Destination
    );
    const Result = await Runˉsentinelˉcoordinator(
        Caseˉwork, Pipeline.products, Inputs, Destination, CASES[Index], 5_000
    );
    Require(Result.code === 17 && !Result.timedOut && !Result.exceeded,
        `${Step} sentinel did not reject exactly.`);
    Require((await readFile(Destination)).equals(Original),
        `${Step} failure changed the existing destination.`);
    const Expected = Step === 'wvadmit' ? ['wvadmit']
        : Step === 'validator' ? ['wvadmit', 'validator']
            : ['wvadmit', 'validator', 'Analyzer'];
    for (const Role of [
        'wvadmit', 'validator', 'foreignBinding', 'Analyzer', 'emitter',
    ]) {
        Require(await lstat(Pipeline.markers[Role]).then(() => true, () => false) ===
            Expected.includes(Role),
        `${Step} failure violated phase sequencing at ${Role}.`);
    }
}

async function Caseˉpublicationˉlifecycle(Work) {
    const Caseˉwork = join(Work, 'Sentinel-publication');
    await mkdir(Caseˉwork);
    const Testˉpreload = await Writeˉsplitˉcompilerˉtestˉpreload(Caseˉwork);
    const Importedˉhooks = Overrides => ({
        preload: Testˉpreload,
        hooks: {
            candidateFailure: null,
            cleanupFailureAfterRemoval: false,
            postLinkCandidateRemoval: false,
            postPublicationCleanup: null,
            temporaryIdentityFailure: false,
            terminationSettleFailure: false,
            ...Overrides,
        },
    });
    const Inputs = await Writeˉsentinelˉinputs(Caseˉwork);
    const Pipeline = await Writeˉsentinelˉpipeline(
        Caseˉwork, null, join(Caseˉwork, 'unused.wvb')
    );
    const Published = join(Caseˉwork, 'Published.wvb');
    const First = await Runˉsentinelˉcoordinator(
        Caseˉwork, Pipeline.products, Inputs, Published, CASES[16], 5_000
    );
    Require(First.code === 0 && (await readFile(Published)).equals(Buffer.from('P')),
        'The sentinel coordinator did not publish its bounded product: ' +
        `exit=${First.code} stdout=${First.output.toString('utf8')} ` +
        `stderr=${First.error.toString('utf8')}`);
    const Existing = await Runˉsentinelˉcoordinator(
        Caseˉwork, Pipeline.products, Inputs, Published,
        `${CASES[16]}-preexisting`, 5_000
    );
    Require(Existing.code !== 0 &&
        (await readFile(Published)).equals(Buffer.from('P')),
    'The pre-existing publication was not preserved.');

    const Raceˉoutput = join(Caseˉwork, 'Race.wvb');
    const Race = await Promise.all([
        Runˉsentinelˉcoordinator(
            Caseˉwork, Pipeline.products, Inputs, Raceˉoutput,
            `${CASES[16]}-race-a`, 5_000, 10_000, false
        ),
        Runˉsentinelˉcoordinator(
            Caseˉwork, Pipeline.products, Inputs, Raceˉoutput,
            `${CASES[16]}-race-b`, 5_000, 10_000, false
        ),
    ]);
    Require(Race.filter(Result => Result.code === 0).length === 1 &&
        Race.filter(Result => Result.code !== 0).length === 1 &&
        (await readFile(Raceˉoutput)).equals(Buffer.from('P')),
    'The sentinel publication race did not preserve exactly one winner.');
    Require(!(await readdir(Caseˉwork)).some(Name =>
        Name.startsWith('.new-windvale-split-compiler-')),
    'The lost publication race left a publication candidate.');

    const Missingˉcandidateˉoutput = join(
        Caseˉwork, 'Missing-publication-candidate.wvb'
    );
    const Missingˉcandidate = await Runˉsentinelˉcoordinator(
        Caseˉwork, Pipeline.products, Inputs, Missingˉcandidateˉoutput,
        `${CASES[16]}-missing-candidate`, 5_000, 10_000, true, null,
        null, Importedˉhooks({ postLinkCandidateRemoval: true })
    );
    Require(Missingˉcandidate.code === 0 &&
        Missingˉcandidate.error.length === 0 &&
        (await readFile(Missingˉcandidateˉoutput)).equals(Buffer.from('P')),
    'An already absent publication candidate rolled back the valid output.');

    const Alias = join(Caseˉwork, 'Alias.wvb');
    await writeFile(Alias, 'module Alias;', { flag: 'wx' });
    const Aliasˉinputs = { ...Inputs, source: Alias };
    const Aliasˉresult = await Runˉsentinelˉcoordinator(
        Caseˉwork, Pipeline.products, Aliasˉinputs, Alias,
        `${CASES[16]}-alias`, 5_000
    );
    Require(Aliasˉresult.code !== 0 &&
        (await readFile(Alias, 'utf8')) === 'module Alias;',
    'The input/output alias was not preserved and rejected.');

    const Candidateˉfailureˉoutput = join(
        Caseˉwork, 'Candidate-finalization-failure.wvb'
    );
    const Candidateˉfailure = await Runˉsentinelˉcoordinator(
        Caseˉwork, Pipeline.products, Inputs, Candidateˉfailureˉoutput,
        `${CASES[16]}-candidate-finalization`, 5_000, 10_000, true, null,
        null, Importedˉhooks({ candidateFailure: 'after-write' })
    );
    Require(Candidateˉfailure.code !== 0 &&
        Candidateˉfailure.error.toString('utf8').includes(
            'forced publication-candidate after-write failure'
        ) &&
        !await lstat(Candidateˉfailureˉoutput).then(() => true, () => false),
    'A mid-finalization failure retained a publication or its candidate.');

    const Candidateˉidentityˉoutput = join(
        Caseˉwork, 'Candidate-identity-failure.wvb'
    );
    const Candidateˉidentityˉfailure = await Runˉsentinelˉcoordinator(
        Caseˉwork, Pipeline.products, Inputs, Candidateˉidentityˉoutput,
        `${CASES[16]}-candidate-identity`, 5_000, 10_000, true, null,
        null, Importedˉhooks({ candidateFailure: 'before-identity' })
    );
    Require(Candidateˉidentityˉfailure.code !== 0 &&
        Candidateˉidentityˉfailure.error.toString('utf8').includes(
            'forced publication-candidate before-identity failure'
        ) &&
        !await lstat(Candidateˉidentityˉoutput).then(() => true, () => false),
    'An initial candidate-identity failure retained its exclusive empty file.');

    const Temporaryˉidentityˉoutput = join(
        Caseˉwork, 'Temporary-identity-failure.wvb'
    );
    const Temporaryˉidentityˉfailure = await Runˉsentinelˉcoordinator(
        Caseˉwork, Pipeline.products, Inputs, Temporaryˉidentityˉoutput,
        `${CASES[16]}-temporary-identity`, 5_000, 10_000, true, null,
        null, Importedˉhooks({ temporaryIdentityFailure: true })
    );
    Require(Temporaryˉidentityˉfailure.code !== 0 &&
        Temporaryˉidentityˉfailure.error.toString('utf8').includes(
            'forced temporary-allocation identity failure'
        ) &&
        !await lstat(Temporaryˉidentityˉoutput).then(() => true, () => false),
    'An unidentified temporary allocation was not removed non-recursively.');

    const Cleanupˉdiagnostic =
        'split compiler status=Cleanupˉfailure detail=' +
        'forced cleanup failure after private-directory removal\n';
    const Cleanupˉoutput = join(Caseˉwork, 'Cleanup-failure.wvb');
    const Cleanupˉfailure = await Runˉsentinelˉcoordinator(
        Caseˉwork, Pipeline.products, Inputs, Cleanupˉoutput,
        `${CASES[16]}-post-publication-cleanup`, 5_000, 10_000, true, null,
        null, Importedˉhooks({
            cleanupFailureAfterRemoval: true,
            postPublicationCleanup: 'original',
        })
    );
    Require(Cleanupˉfailure.code !== 0 &&
        Cleanupˉfailure.error.toString('utf8') === Cleanupˉdiagnostic &&
        !await lstat(Cleanupˉoutput).then(() => true, () => false),
    'A post-publication cleanup failure retained the published original.');

    const Replacementˉoutput = join(Caseˉwork, 'Cleanup-replacement.wvb');
    const Replacementˉfailure = await Runˉsentinelˉcoordinator(
        Caseˉwork, Pipeline.products, Inputs, Replacementˉoutput,
        `${CASES[16]}-post-publication-replacement`, 5_000, 10_000, true, null,
        null, Importedˉhooks({
            cleanupFailureAfterRemoval: true,
            postPublicationCleanup: 'replacement',
        })
    );
    Require(Replacementˉfailure.code !== 0 &&
        Replacementˉfailure.error.toString('utf8') === Cleanupˉdiagnostic &&
        (await readFile(Replacementˉoutput)).equals(
            Buffer.from('attacker-replacement')
        ),
    'A post-publication cleanup failure removed an identity-mismatched replacement.');

    const Primaryˉwork = join(Caseˉwork, 'Primary-and-cleanup-failure');
    await mkdir(Primaryˉwork);
    const Primaryˉinputs = await Writeˉsentinelˉinputs(Primaryˉwork);
    const Primaryˉpipeline = await Writeˉsentinelˉpipeline(
        Primaryˉwork, 'Analyzer', join(Primaryˉwork, 'Primary-marker')
    );
    const Primaryˉoutput = join(Primaryˉwork, 'Unpublished.wvb');
    const Primaryˉfailure = await Runˉsentinelˉcoordinator(
        Primaryˉwork, Primaryˉpipeline.products, Primaryˉinputs, Primaryˉoutput,
        `${CASES[16]}-primary-plus-cleanup`, 5_000, 10_000, true, null,
        null, Importedˉhooks({ cleanupFailureAfterRemoval: true })
    );
    Require(Primaryˉfailure.code === 17 &&
        Primaryˉfailure.error.toString('utf8') ===
            `Analyzer rejected\n${Cleanupˉdiagnostic}` &&
        !await lstat(Primaryˉoutput).then(() => true, () => false),
    'Cleanup failure masked the primary compiler diagnostic or status.');

    const Nearˉceilingˉwork = join(Caseˉwork, 'Near-ceiling-diagnostic');
    await mkdir(Nearˉceilingˉwork);
    const Nearˉceilingˉinputs = await Writeˉsentinelˉinputs(
        Nearˉceilingˉwork
    );
    const Nearˉceilingˉpipeline = await Writeˉsentinelˉpipeline(
        Nearˉceilingˉwork, null, join(Nearˉceilingˉwork, 'unused.wvb')
    );
    const Primaryˉbytes = Buffer.alloc(65_470, 0x50);
    Nearˉceilingˉpipeline.products.wvanalyze =
        await Writeˉproductˉsentinel(
            Nearˉceilingˉwork,
            'Near-Ceiling-Analyzer',
            `process.stderr.write('P'.repeat(${Primaryˉbytes.length}));` +
            'process.exitCode=17;\n'
        );
    const Nearˉceilingˉoutput = join(
        Nearˉceilingˉwork, 'Unpublished.wvb'
    );
    const Nearˉceilingˉfailure = await Runˉsentinelˉcoordinator(
        Nearˉceilingˉwork, Nearˉceilingˉpipeline.products,
        Nearˉceilingˉinputs, Nearˉceilingˉoutput,
        `${CASES[16]}-near-ceiling-primary`, 5_000, 10_000, true, null,
        null, Importedˉhooks({ cleanupFailureAfterRemoval: true }),
        MAXIMUM_OUTPUT_BYTES * 2
    );
    const Nearˉceilingˉdiagnostic = Nearˉceilingˉfailure.error;
    const Appended = Nearˉceilingˉdiagnostic.subarray(Primaryˉbytes.length);
    Require(Nearˉceilingˉfailure.code === 17 &&
        Nearˉceilingˉdiagnostic.length === MAXIMUM_OUTPUT_BYTES &&
        Nearˉceilingˉdiagnostic.subarray(0, Primaryˉbytes.length)
            .equals(Primaryˉbytes) &&
        Appended.toString('utf8').startsWith(
            'split compiler status=Cleanupˉfailure detail='
        ) && Appended.at(-1) === 10 &&
        !await lstat(Nearˉceilingˉoutput).then(() => true, () => false),
    'Near-ceiling cleanup evidence changed or overflowed the primary diagnostic.');
}

async function Caseˉanalyzerˉsourceˉmismatch(Work) {
    const Caseˉwork = join(Work, 'Sentinel-analyzer-source-mismatch');
    await mkdir(Caseˉwork);
    const Inputs = await Writeˉsentinelˉinputs(Caseˉwork);
    const Pipeline = await Writeˉsentinelˉpipeline(
        Caseˉwork, null, join(Caseˉwork, 'unused.wvb')
    );
    Pipeline.products.wvanalyze = await Writeˉproductˉsentinel(
        Caseˉwork, 'Mismatching-analyzer',
        "import{readFile,writeFile}from'node:fs/promises';" +
        "const a=process.argv.slice(2);const s=await readFile(a[1]);" +
        "await writeFile(a[2],Buffer.concat([s,Buffer.from([32])]),{flag:'wx'});" +
        "await writeFile(a[3],Buffer.alloc(104),{flag:'wx'});" +
        "await writeFile(a[4],Buffer.from([1]),{flag:'wx'});" +
        "await writeFile(a[5],Buffer.from([1]),{flag:'wx'});" +
        "process.stdout.write('analysis mismatch\\n');\n"
    );
    const Output = join(Caseˉwork, 'Mismatch.wvb');
    const Result = await Runˉsentinelˉcoordinator(
        Caseˉwork, Pipeline.products, Inputs, Output,
        `${CASES[15]}-source-mismatch`, 5_000
    );
    Require(Result.code !== 0 &&
        !await lstat(Output).then(() => true, () => false) &&
        !await lstat(Pipeline.markers.emitter).then(() => true, () => false),
    'The Analyzer source-set mismatch reached emitter or publication.');
}

async function Caseˉvalidatorˉsnapshotˉmutation(Work) {
    for (const [Name, Argument] of [
        ['evidence', 0],
        ['source-set', 1],
        ['target', 2],
        ['catalog', 3],
        ['lock', 4],
        ['profile', 5],
    ]) {
        const Caseˉwork = join(
            Work, `Sentinel-validator-snapshot-mutation-${Name}`
        );
        await mkdir(Caseˉwork);
        const Inputs = await Writeˉsentinelˉinputs(Caseˉwork);
        const Pipeline = await Writeˉsentinelˉpipeline(
            Caseˉwork, null, join(Caseˉwork, 'unused.wvb')
        );
        Pipeline.products.wvauth = await Writeˉproductˉsentinel(
            Caseˉwork, `Mutating-validator-${Name}`,
            "import{chmod,readFile,writeFile}from'node:fs/promises';" +
            `const a=process.argv.slice(2);const s=await readFile(a[${Argument}]);` +
            `await chmod(a[${Argument}],0o600);` +
            `await writeFile(a[${Argument}],Buffer.concat([s,Buffer.from([32])]));` +
            "process.stdout.write('authentication mutation\\n');\n"
        );
        const Output = join(Caseˉwork, 'Mutation.wvb');
        const Result = await Runˉsentinelˉcoordinator(
            Caseˉwork, Pipeline.products, Inputs, Output,
            `${CASES[14]}-${Name}`, 5_000
        );
        Require(Result.code !== 0 &&
            !await lstat(Output).then(() => true, () => false) &&
            !await lstat(Pipeline.markers.foreignBinding).then(
                () => true, () => false
            ) &&
            !await lstat(Pipeline.markers.Analyzer).then(() => true, () => false) &&
            !await lstat(Pipeline.markers.emitter).then(() => true, () => false),
        `A validator ${Name} mutation reached Analyzer, emitter, or publication.`);
    }
}

async function Caseˉauthenticatedˉforeignˉbindingˉboundary(Work) {
    const Exactˉwork = join(Work, 'Sentinel-foreign-binding-exact');
    await mkdir(Exactˉwork);
    const Exactˉinputs = await Writeˉsentinelˉinputs(Exactˉwork);
    const Exactˉoutput = join(Exactˉwork, 'Foreign.wvb');
    const Exactˉpipeline = await Writeˉsentinelˉpipeline(
        Exactˉwork, null, Exactˉoutput, 1
    );
    const Exact = await Runˉsentinelˉcoordinator(
        Exactˉwork, Exactˉpipeline.products, Exactˉinputs, Exactˉoutput,
        `${CASES[1]}-exact-evidence`, 5_000
    );
    const Exactˉactivity = Exact.output.toString('utf8');
    const Exactˉauthenticationˉactivity =
        Exactˉactivity.indexOf('step=source-authentication');
    const Exactˉbindingˉactivity =
        Exactˉactivity.indexOf('step=source-foreign-binding');
    Require(Exact.code !== 0 &&
        Exact.error.toString('utf8').includes('Foreignˉloweringˉpending') &&
        Exactˉauthenticationˉactivity >= 0 &&
        Exactˉbindingˉactivity > Exactˉauthenticationˉactivity &&
        !Exactˉactivity.includes('step=source-analysis') &&
        !Exactˉactivity.includes('step=source-emission') &&
        await lstat(Exactˉpipeline.markers.foreignBinding).then(
            () => true, () => false
        ) &&
        !await lstat(Exactˉpipeline.markers.Analyzer).then(
            () => true, () => false
        ) &&
        !await lstat(Exactˉpipeline.markers.emitter).then(
            () => true, () => false
        ) &&
        !await lstat(Exactˉoutput).then(() => true, () => false),
    'Exact foreign-binding evidence did not stop at the lowering boundary.');

    const Missingˉbinderˉwork = join(
        Work, 'Sentinel-foreign-binding-missing-product'
    );
    await mkdir(Missingˉbinderˉwork);
    const Missingˉbinderˉinputs = await Writeˉsentinelˉinputs(
        Missingˉbinderˉwork
    );
    const Missingˉbinderˉoutput = join(Missingˉbinderˉwork, 'Foreign.wvb');
    const Missingˉbinderˉpipeline = await Writeˉsentinelˉpipeline(
        Missingˉbinderˉwork, null, Missingˉbinderˉoutput, 1
    );
    const Missingˉbinderˉarguments = [
        SPLIT_COMPILER,
        Missingˉbinderˉpipeline.products.wvadmit,
        Missingˉbinderˉpipeline.products.wvauth,
        Missingˉbinderˉpipeline.products.wvanalyze,
        Missingˉbinderˉpipeline.products.wvemit,
        '--source-input-lock', Missingˉbinderˉinputs.lock,
        Missingˉbinderˉinputs.lockDigest,
        '--source-profile', Missingˉbinderˉinputs.profile,
        '--target-descriptor', Missingˉbinderˉinputs.target,
        Missingˉbinderˉinputs.source, Missingˉbinderˉoutput,
    ];
    const Missingˉbinder = await Runˉsentinelˉcoordinator(
        Missingˉbinderˉwork, Missingˉbinderˉpipeline.products,
        Missingˉbinderˉinputs, Missingˉbinderˉoutput,
        `${CASES[1]}-missing-binder`, 5_000, 10_000, true,
        Missingˉbinderˉarguments
    );
    const Missingˉbinderˉdiagnostic = Buffer.concat([
        Missingˉbinder.output, Missingˉbinder.error,
    ]).toString('utf8');
    Require(Missingˉbinder.code !== 0 && Missingˉbinderˉdiagnostic.includes(
        'The authenticated foreign catalog requires --foreign-binder <wvbind>.'
    ) && await lstat(Missingˉbinderˉpipeline.markers.wvadmit).then(
        () => true, () => false
    ) && await lstat(Missingˉbinderˉpipeline.markers.validator).then(
        () => true, () => false
    ) && !await lstat(Missingˉbinderˉpipeline.markers.foreignBinding).then(
        () => true, () => false
    ) && !await lstat(Missingˉbinderˉpipeline.markers.Analyzer).then(
        () => true, () => false
    ) && !await lstat(Missingˉbinderˉpipeline.markers.emitter).then(
        () => true, () => false
    ) && !await lstat(Missingˉbinderˉoutput).then(() => true, () => false),
    'A nonempty catalog without wvbind crossed the authenticated boundary.');

    const Exactˉevidenceˉsource =
        "const[s,t,c]=await Promise.all(a.map(p=>readFile(p)));" +
        "const h=b=>createHash('sha256').update(b).digest('hex');" +
        "const e='foreign binding status=Published '+" +
        "`source-bytes=${s.length} source-sha256=${h(s)} `+" +
        "`target-bytes=${t.length} target-sha256=${h(t)} `+" +
        "`catalog-bytes=${c.length} catalog-sha256=${h(c)} `+" +
        "`foreign-count=${c.readUInt32LE(12)}\\n`;";
    for (const [Name, Body, Expectedˉdiagnostic] of [
        [
            'missing-evidence', '',
            'The source-foreign-binding produced invalid success diagnostics.',
        ],
        [
            'malformed-evidence',
            "process.stdout.write('foreign binding status=Published\\n');",
            'The foreign-binder evidence does not exactly match the retained ' +
                'authenticated inputs.',
        ],
        [
            'mismatched-evidence',
            "process.stdout.write('foreign binding status=Published source-bytes=0 '+" +
                "'source-sha256='+('0'.repeat(64))+' target-bytes=0 '+" +
                "'target-sha256='+('0'.repeat(64))+' catalog-bytes=0 '+" +
                "'catalog-sha256='+('0'.repeat(64))+' foreign-count=0\\n');",
            'The foreign-binder evidence does not exactly match the retained ' +
                'authenticated inputs.',
        ],
        [
            'partial-evidence', Exactˉevidenceˉsource +
                'process.stdout.write(e.slice(0,-1));',
            'The foreign-binder evidence does not exactly match the retained ' +
                'authenticated inputs.',
        ],
        [
            'duplicated-evidence', Exactˉevidenceˉsource +
                'process.stdout.write(e+e);',
            'The foreign-binder evidence does not exactly match the retained ' +
                'authenticated inputs.',
        ],
        [
            'extra-evidence', Exactˉevidenceˉsource +
                "process.stdout.write(e+'extra\\n');",
            'The foreign-binder evidence does not exactly match the retained ' +
                'authenticated inputs.',
        ],
    ]) {
        const Caseˉwork = join(Work, `Sentinel-foreign-binding-${Name}`);
        await mkdir(Caseˉwork);
        const Inputs = await Writeˉsentinelˉinputs(Caseˉwork);
        const Output = join(Caseˉwork, 'Foreign.wvb');
        const Pipeline = await Writeˉsentinelˉpipeline(
            Caseˉwork, null, Output, 1
        );
        Pipeline.products.wvbind = await Writeˉproductˉsentinel(
            Caseˉwork, `Invalid-foreign-binding-${Name}`,
            "import{createHash}from'node:crypto';" +
            "import{readFile,writeFile}from'node:fs/promises';" +
            `const a=process.argv.slice(2);await writeFile(` +
            `${JSON.stringify(Pipeline.markers.foreignBinding)},Buffer.alloc(0),` +
            `{flag:'a'});${Body}\n`
        );
        const Result = await Runˉsentinelˉcoordinator(
            Caseˉwork, Pipeline.products, Inputs, Output,
            `${CASES[1]}-${Name}`, 5_000
        );
        const Diagnostic = Buffer.concat([Result.output, Result.error])
            .toString('utf8');
        Require(Result.code !== 0 && Diagnostic.includes(Expectedˉdiagnostic) &&
            await lstat(Pipeline.markers.foreignBinding).then(
            () => true, () => false
        ) && !await lstat(Pipeline.markers.Analyzer).then(
            () => true, () => false
        ) && !await lstat(Pipeline.markers.emitter).then(
            () => true, () => false
        ) && !await lstat(Output).then(() => true, () => false),
        `Invalid foreign-binding ${Name} crossed the boundary: ${Diagnostic}`);
    }

    for (const [Name, Pathˉexpression, Label] of [
        ['evidence', "join(dirname(a[0]),'Evidence.wvae')", 'admission evidence'],
        ['source-set', 'a[0]', 'admitted source set'],
        ['target', 'a[1]', 'admitted target descriptor'],
        ['catalog', 'a[2]', 'foreign catalog'],
        ['lock', "join(dirname(a[0]),'Source-Inputs.wvlock')", 'source-input lock'],
        ['profile', "join(dirname(a[0]),'Source-Profile.wvsp')", 'source profile'],
    ]) {
        const Caseˉwork = join(
            Work, `Sentinel-foreign-binding-retained-${Name}`
        );
        await mkdir(Caseˉwork);
        const Inputs = await Writeˉsentinelˉinputs(Caseˉwork);
        const Output = join(Caseˉwork, 'Foreign.wvb');
        const Pipeline = await Writeˉsentinelˉpipeline(
            Caseˉwork, null, Output, 1
        );
        Pipeline.products.wvbind = await Writeˉproductˉsentinel(
            Caseˉwork, `Mutating-foreign-binding-${Name}`,
            "import{createHash}from'node:crypto';" +
            "import{chmod,readFile,writeFile}from'node:fs/promises';" +
            "import{dirname,join}from'node:path';" +
            `const a=process.argv.slice(2);await writeFile(` +
            `${JSON.stringify(Pipeline.markers.foreignBinding)},Buffer.alloc(0),` +
            "{flag:'a'});const[s,t,c]=await Promise.all(a.map(p=>readFile(p)));" +
            "const h=b=>createHash('sha256').update(b).digest('hex');" +
            `const p=${Pathˉexpression};const b=await readFile(p);` +
            "b[b.length-1]^=1;await chmod(p,0o600);await writeFile(p,b);" +
            "process.stdout.write('foreign binding status=Published '+" +
            "`source-bytes=${s.length} source-sha256=${h(s)} `+" +
            "`target-bytes=${t.length} target-sha256=${h(t)} `+" +
            "`catalog-bytes=${c.length} catalog-sha256=${h(c)} `+" +
            "`foreign-count=${c.readUInt32LE(12)}\\n`);\n"
        );
        const Result = await Runˉsentinelˉcoordinator(
            Caseˉwork, Pipeline.products, Inputs, Output,
            `${CASES[1]}-${Name}-retained-mutation`, 5_000
        );
        const Diagnostic = Buffer.concat([Result.output, Result.error])
            .toString('utf8');
        Require(Result.code !== 0 && Diagnostic.includes(
            `The retained ${Label} changed between compiler phases.`
        ) && await lstat(Pipeline.markers.foreignBinding).then(
            () => true, () => false
        ) && !await lstat(Pipeline.markers.Analyzer).then(
            () => true, () => false
        ) && !await lstat(Pipeline.markers.emitter).then(
            () => true, () => false
        ) && !await lstat(Output).then(() => true, () => false),
        `A post-binding ${Name} mutation escaped its retained-snapshot recheck: ` +
            Diagnostic);
    }

    const Failureˉwork = join(Work, 'Sentinel-foreign-binding-failure');
    await mkdir(Failureˉwork);
    const Failureˉinputs = await Writeˉsentinelˉinputs(Failureˉwork);
    const Failureˉoutput = join(Failureˉwork, 'Foreign.wvb');
    const Failureˉpipeline = await Writeˉsentinelˉpipeline(
        Failureˉwork, null, Failureˉoutput, 1
    );
    Failureˉpipeline.products.wvbind = await Writeˉproductˉsentinel(
        Failureˉwork, 'Rejecting-foreign-binding',
        `import{writeFile}from'node:fs/promises';await writeFile(` +
        `${JSON.stringify(Failureˉpipeline.markers.foreignBinding)},` +
        "Buffer.alloc(0),{flag:'a'});process.stderr.write('binder rejected\\n');" +
        'process.exitCode=17;\n'
    );
    const Failure = await Runˉsentinelˉcoordinator(
        Failureˉwork, Failureˉpipeline.products, Failureˉinputs,
        Failureˉoutput, `${CASES[1]}-binder-failure`, 5_000
    );
    Require(Failure.code === 17 &&
        Failure.error.toString('utf8') === 'binder rejected\n' &&
        await lstat(Failureˉpipeline.markers.wvadmit).then(
            () => true, () => false
        ) && await lstat(Failureˉpipeline.markers.validator).then(
            () => true, () => false
        ) && await lstat(Failureˉpipeline.markers.foreignBinding).then(
            () => true, () => false
        ) && !await lstat(Failureˉpipeline.markers.Analyzer).then(
            () => true, () => false
        ) && !await lstat(Failureˉpipeline.markers.emitter).then(
            () => true, () => false
        ) && !await lstat(Failureˉoutput).then(() => true, () => false),
    'A rejecting foreign binder violated phase order or reached publication.');

    for (const Mode of ['timeout', 'overflow']) {
        const Caseˉwork = join(Work, `Sentinel-foreign-binding-${Mode}`);
        await mkdir(Caseˉwork);
        const Inputs = await Writeˉsentinelˉinputs(Caseˉwork);
        const Output = join(Caseˉwork, 'Foreign.wvb');
        const Pipeline = await Writeˉsentinelˉpipeline(
            Caseˉwork, null, Output, 1
        );
        const Descendant = join(Caseˉwork, 'Descendant.pid');
        Pipeline.products.wvbind = await Writeˉproductˉsentinel(
            Caseˉwork, `Bounded-foreign-binding-${Mode}`,
            "import{spawn}from'node:child_process';" +
            "import{writeFile}from'node:fs/promises';" +
            `await writeFile(${JSON.stringify(Pipeline.markers.foreignBinding)},` +
            "Buffer.alloc(0),{flag:'a'});" +
            "const c=spawn(process.execPath,['-e','setInterval(()=>{},1000)']," +
            "{stdio:'ignore'});" +
            `await writeFile(${JSON.stringify(Descendant)},String(c.pid),` +
            "{flag:'wx'});" +
            (Mode === 'timeout'
                ? 'setInterval(()=>{},1000);\n'
                : "const b=Buffer.alloc(8192,120);" +
                    'setInterval(()=>process.stdout.write(b),1);\n')
        );
        const Result = await Runˉsentinelˉcoordinator(
            Caseˉwork, Pipeline.products, Inputs, Output,
            `${CASES[1]}-binder-${Mode}`, Mode === 'timeout' ? 500 : 5_000
        );
        const Expectedˉstatus = Mode === 'timeout' ? 'Timeout' : 'Outputˉlimit';
        const Diagnostic = Result.error.toString('utf8');
        const Identifier = Number.parseInt(await readFile(Descendant, 'utf8'), 10);
        Require(Result.code !== 0 &&
            Diagnostic.includes(`status=${Expectedˉstatus}`) &&
            Diagnostic.includes('step=source-foreign-binding') &&
            Number.isSafeInteger(Identifier) && Identifier > 0 &&
            await Waitˉforˉexit(Identifier) &&
            await lstat(Pipeline.markers.wvadmit).then(
                () => true, () => false
            ) && await lstat(Pipeline.markers.validator).then(
                () => true, () => false
            ) && await lstat(Pipeline.markers.foreignBinding).then(
                () => true, () => false
            ) && !await lstat(Pipeline.markers.Analyzer).then(
                () => true, () => false
            ) && !await lstat(Pipeline.markers.emitter).then(
                () => true, () => false
            ) && !await lstat(Output).then(() => true, () => false),
        `The foreign-binder ${Mode} path violated its bounded phase contract.`);
    }
}

async function Runˉsentinelˉinfrastructure(Work) {
    await Caseˉpreflightˉrejections(Work);
    await Caseˉpreflightˉmaxima(Work);
    await Caseˉphaseˉlifecycle(Work, 13, 'wvadmit');
    await Caseˉphaseˉlifecycle(Work, 14, 'validator');
    await Caseˉvalidatorˉsnapshotˉmutation(Work);
    await Caseˉauthenticatedˉforeignˉbindingˉboundary(Work);
    await Caseˉphaseˉlifecycle(Work, 15, 'Analyzer');
    await Caseˉanalyzerˉsourceˉmismatch(Work);
    await Caseˉpublicationˉlifecycle(Work);
    await Caseˉtimeoutˉtree(Work);
    await Caseˉterminationˉfailureˉretainsˉtree(Work);
    await Caseˉoverflow(Work);
    await Caseˉheartbeat(Work);
}

function Constructˉwvtd(Build = 4) {
    const Result = Buffer.alloc(64);
    Result.write('WVTD', 0, 4, 'ascii');
    Result.writeUInt16LE(1, 4);
    Result.writeUInt32LE(64, 8);
    const Values = Build === 4
        ? [4, 2, 1, 2, 64, 1, 1]
        : Build === 2
            ? [2, 2, 1, 1, 64, 1, 0]
            : Build === 1
                ? [1, 1, 1, 1, 64, 1, 0]
                : null;
    Require(Values !== null, `Unsupported test WVTD build identity ${Build}.`);
    for (let Index = 0; Index < Values.length; Index += 1) {
        Result.writeUInt32LE(Values[Index], 12 + Index * 4);
    }
    return Result;
}

function Constructˉwvss1(Sources) {
    const Header = 16 + Sources.length * 8;
    const Result = Buffer.alloc(Header + Sources.reduce(
        (Total, Source) => Total + Source.length, 0
    ));
    Result.write('WVSS', 0, 4, 'ascii');
    Result.writeUInt16LE(1, 4);
    Result.writeUInt32LE(Sources.length, 8);
    Result.writeUInt32LE(Sources.length * 8, 12);
    let Offset = Header;
    Sources.forEach((Source, Index) => {
        Result.writeUInt32LE(Offset, 16 + Index * 8);
        Result.writeUInt32LE(Source.length, 20 + Index * 8);
        Source.copy(Result, Offset);
        Offset += Source.length;
    });
    return Result;
}

async function Buildˉandˉpackageˉproducts(Work, Buildˉmode) {
    Requireˉcompleteˉpins();
    const Extension = WINDOWS ? 'cmd' : 'sh';
    const Executableˉextension = WINDOWS ? 'exe' : 'elf';
    const Lowˉlevelˉpackage = join(
        SCRIPT_DIRECTORY,
        `Package-Segmented-Compiler-Wvb.${Extension}`
    );
    const Cachedˉpackage = join(
        SCRIPT_DIRECTORY,
        'Build-Cached-Segmented-Hosted-Wvb.mjs'
    );
    const Build = join(SCRIPT_DIRECTORY, 'Build-Cached-Split-Project-Wvb.mjs');
    const Writeˉidentity = join(
        SCRIPT_DIRECTORY, 'Write-Split-Compiler-Producer-Identity.mjs'
    );
    const Pinnedˉroot = join(
        REPOSITORY_ROOT, 'Artifacts',
        'Language-1.0-Target-Aware-Emission-Bootstrap', 'Wvb'
    );
    const Cacheˉroots = Buildˉmode.coldDoubleBuild
        ? [join(Work, 'Cache-A'), join(Work, 'Cache-B')]
        : [];
    await Promise.all(Cacheˉroots.map(Cache => mkdir(Cache)));
    const Buildˉenvironments = Buildˉmode.coldDoubleBuild
        ? Cacheˉroots.map(Cache => ({
            ...process.env,
            WINDVALE_NATIVE_CACHE_ROOT: Cache,
        }))
        : [process.env];
    process.stdout.write(
        `production admission products build-mode=${Buildˉmode.name} ` +
        `builds-per-product=${Buildˉmode.buildsPerProduct} ` +
        `cache=${Buildˉmode.coldDoubleBuild ? 'isolated-cold' : 'shared'}\n`
    );
    const Pinned = {};
    for (const [Name, Expected] of Object.entries(PINNED_COMPILER)) {
        const Wvb = join(Pinnedˉroot, Expected.file);
        const Actual = await Evidence(Wvb);
        Require(Actual.bytes === Expected.bytes &&
            Actual.sha256 === Expected.sha256,
        `pinned ${Name} identity differs: bytes=${Actual.bytes} ` +
            `sha256=${Actual.sha256}.`);
        const Application = join(
            Work, `Pinned-${Name}.${Executableˉextension}`
        );
        await Requireˉsuccess(
            `pinned-${Name}-profile-7-package`, Lowˉlevelˉpackage,
            ['7', Wvb, Application, '--development-cache'],
            PACKAGE_TIMEOUT_MILLISECONDS, true);
        const Identity = join(Work, `Pinned-${Name}.identity`);
        await Requireˉsuccess(`pinned-${Name}-identity`, process.execPath,
            [Writeˉidentity, Expected.role, Application, Identity],
            BUILD_TIMEOUT_MILLISECONDS);
        Pinned[Name] = { application: Application, identity: Identity };
    }
    const Projects = {
        wvadmit: join(REPOSITORY_ROOT, 'Projects', 'Tools',
            'Windvale-Compiler-Admission-Driver.wvproj'),
        wvauth: join(REPOSITORY_ROOT, 'Projects', 'Tools',
            'Windvale-Compiler-Source-Authenticator.wvproj'),
        wvanalyze: join(REPOSITORY_ROOT, 'Projects', 'Tools',
            'Windvale-Compiler-Analysis-Driver.wvproj'),
        wvbind: join(REPOSITORY_ROOT, 'Projects', 'Tools',
            'Windvale-Compiler-Foreign-Binding-Driver.wvproj'),
        wvemit: join(REPOSITORY_ROOT, 'Projects', 'Tools',
            'Windvale-Compiler-Emission-Driver.wvproj'),
    };
    const Applications = {};
    let Productˉindex = 0;
    const Buildˉproduct = async (
        Name, Project, Analyzer, Emitter
    ) => {
        Productˉindex += 1;
        const First = join(Work, `${Name}-A.wvb`);
        const Producerˉarguments = [
            Analyzer.application, Analyzer.identity,
            Emitter.application, Emitter.identity,
        ];
        process.stdout.write(
            `START production admission product=${Name} ` +
            `item=${Productˉindex}/5 build=1/${Buildˉmode.buildsPerProduct} ` +
            `build-mode=${Buildˉmode.name}\n`
        );
        await Requireˉsuccess(`${Name}-build-1`, process.execPath,
            [Build, Project, First, ...Producerˉarguments],
            BUILD_TIMEOUT_MILLISECONDS, false, Buildˉenvironments[0]);
        const Firstˉidentity = await Evidence(First);
        if (Buildˉmode.coldDoubleBuild) {
            const Second = join(Work, `${Name}-B.wvb`);
            process.stdout.write(
                `START production admission product=${Name} ` +
                `item=${Productˉindex}/5 build=2/2 ` +
                `build-mode=${Buildˉmode.name}\n`
            );
            await Requireˉsuccess(`${Name}-build-2`, process.execPath,
                [Build, Project, Second, ...Producerˉarguments],
                BUILD_TIMEOUT_MILLISECONDS, false, Buildˉenvironments[1]);
            const Secondˉidentity = await Evidence(Second);
            Require(Firstˉidentity.value.equals(Secondˉidentity.value),
                `${Name} cold double build is not byte-identical.`);
        }
        const Expected = EXPECTED_PRODUCTS[Name];
        Require(Firstˉidentity.bytes === Expected.bytes &&
            Firstˉidentity.sha256 === Expected.sha256,
        `${Name} identity differs: bytes=${Firstˉidentity.bytes} ` +
            `sha256=${Firstˉidentity.sha256}.`);
        const Application = join(Work, `${Name}.${Executableˉextension}`);
        if (Name === 'wvanalyze' || Name === 'wvemit') {
            await Requireˉsuccess(
                `${Name}-profile-7-package`, Lowˉlevelˉpackage,
                ['7', First, Application, '--development-cache'],
                PACKAGE_TIMEOUT_MILLISECONDS, true);
        } else {
            await Requireˉsuccess(
                `${Name}-profile-7-package`, process.execPath,
                [Cachedˉpackage, '7', First, Application],
                PACKAGE_TIMEOUT_MILLISECONDS, true);
        }
        const Applicationˉidentity = await Evidence(Application);
        process.stdout.write(
            `production admission product=${Name} ` +
            `wvb-bytes=${Firstˉidentity.bytes} ` +
            `wvb-sha256=${Firstˉidentity.sha256} ` +
            `application-bytes=${Applicationˉidentity.bytes} ` +
            `application-sha256=${Applicationˉidentity.sha256} ` +
            `build-mode=${Buildˉmode.name} ` +
            `wvb-identity=Recorded-candidate-match ` +
            `application-identity=Measured-not-pinned ` +
            `cold-double-build=${Buildˉmode.coldDoubleBuild
                ? 'Verified'
                : 'Not-requested'}\n`
        );
        Applications[Name] = Application;
    };
    for (const Name of ['wvadmit', 'wvauth', 'wvanalyze', 'wvbind']) {
        await Buildˉproduct(
            Name, Projects[Name], Pinned.analyzer, Pinned.emitter
        );
    }
    const Currentˉanalyzerˉidentity = join(Work, 'wvanalyze.identity');
    await Requireˉsuccess('wvanalyze-current-identity', process.execPath,
        [Writeˉidentity, 'analyzer', Applications.wvanalyze,
            Currentˉanalyzerˉidentity],
        BUILD_TIMEOUT_MILLISECONDS);
    await Buildˉproduct(
        'wvemit', Projects.wvemit,
        {
            application: Applications.wvanalyze,
            identity: Currentˉanalyzerˉidentity,
        },
        Pinned.emitter
    );
    return Applications;
}

async function Writeˉproductionˉinputs(Work) {
    const Lock = await readFile(join(
        REPOSITORY_ROOT, 'Documents', 'Project',
        'Language-1.0-Localization-Workloads', '01-Source-Profile-Admission',
        'Reference-Artifacts', 'Source-Inputs.wvlock'
    ));
    const Profile = await readFile(join(
        REPOSITORY_ROOT, 'Documents', 'Project',
        'Language-1.0-Localization-Workloads', '01-Source-Profile-Admission',
        'Reference-Artifacts', 'En-Source-Profile.wvsp'
    ));
    const Coreˉsource = Buffer.from(
        '#!wv/1 en@1\nmodule Root; profile core; platform linux; ' +
        'authority application;', 'utf8'
    );
    const Foreignˉsource = Buffer.from(
        '#!wv/1 en@1\nmodule Root; profile system; ' +
        'platform linux.x86_64.sysv_amd64_c_v1; authority application; ' +
        'unsafe foreign "windvale.paper.buffer_source.sysv_amd64_c_v1" ' +
        'fn Readˉforeignˉrecord(Destination: ' +
        'Unsafe.Foreignˉpointer<u8, Bufferˉsourceˉabi>, Capacity: u64, ' +
        'Expectedˉgeneration: u64) -> i64 effects(ffi.call) as ' +
        '"wv_paper_buffer_source_read_v1";', 'utf8'
    );
    const Paths = {
        lock: join(Work, 'Source-Inputs.wvlock'),
        profile: join(Work, 'Source-Profile.wvsp'),
        target: join(Work, 'Target.wvtd'),
        core: join(Work, 'Core.wv'),
        foreign: join(Work, 'Foreign.wv'),
    };
    await Promise.all([
        writeFile(Paths.lock, Lock, { flag: 'wx' }),
        writeFile(Paths.profile, Profile, { flag: 'wx' }),
        writeFile(Paths.target, Constructˉwvtd(), { flag: 'wx' }),
        writeFile(Paths.core, Coreˉsource, { flag: 'wx' }),
        writeFile(Paths.foreign, Foreignˉsource, { flag: 'wx' }),
    ]);
    return { ...Paths, lockDigest: Sha256(Lock), Coreˉsource, Foreignˉsource };
}

function Authenticatedˉarguments(Products, Inputs, Source, Output) {
    return [
        SPLIT_COMPILER,
        Products.wvadmit, Products.wvauth, Products.wvanalyze, Products.wvemit,
        '--foreign-binder', Products.wvbind,
        '--source-input-lock', Inputs.lock, Inputs.lockDigest,
        '--source-profile', Inputs.profile,
        '--target-descriptor', Inputs.target,
        Source, Output,
    ];
}

async function Writeˉadmissionˉsnapshotˉmutator(
    Work, Name, Admitter, Mode, Mixedˉcatalog = null
) {
    let Mutation;
    if (Mode === 'missing') Mutation = 'await rm(a[11]);';
    else if (Mode === 'mixed') {
        Mutation = `await rm(a[11]);await copyFile(${JSON.stringify(Mixedˉcatalog)},` +
            'a[11]);';
    } else {
        Mutation = 'const e=await readFile(a[12]);e[64]^=1;' +
            'await writeFile(a[12],e);';
    }
    return await Writeˉproductˉsentinel(
        Work, Name,
        "import{spawnSync}from'node:child_process';" +
        "import{copyFile,readFile,rm,writeFile}from'node:fs/promises';" +
        `const a=process.argv.slice(2);const r=spawnSync(${JSON.stringify(Admitter)},a,` +
        "{encoding:null,maxBuffer:65536,timeout:300000,windowsHide:true});" +
        "if(r.stdout?.length)process.stdout.write(r.stdout);" +
        "if(r.stderr?.length)process.stderr.write(r.stderr);" +
        "if(r.error)throw r.error;if(r.status!==0){process.exitCode=r.status??1;}" +
        `else{${Mutation}}\n`
    );
}

async function Writeˉlaunchˉguard(Work, Name, Marker) {
    return await Writeˉproductˉsentinel(
        Work, Name,
        `import{writeFile}from'node:fs/promises';` +
        `await writeFile(${JSON.stringify(Marker)},Buffer.alloc(0),{flag:'wx'});` +
        `process.stderr.write('${Name} must not launch\\n');process.exitCode=99;\n`
    );
}

async function Runˉproductionˉcases(Work, Products, Inputs) {
    // Cases 1-13 and 17 are production-only. They intentionally remain behind
    // the complete five-product recorded-identity pin gate above.
    // The two valid outputs prove current-run final-WVB determinism only. They
    // are not the absent Decision 0893 WVSS/WVCA/WVLB/WVIR/WVB baseline corpus.
    const Runnerˉtemporary = join(Work, 'Runner-Temporary');
    await mkdir(Runnerˉtemporary);
    const Runnerˉenvironment = {
        ...process.env,
        TEMP: Runnerˉtemporary,
        TMP: Runnerˉtemporary,
        TMPDIR: Runnerˉtemporary,
    };
    const Runˉcoordinator = async (Arguments, Activity) =>
        await Runˉbounded(
            process.execPath, Arguments, Activity,
            BUILD_TIMEOUT_MILLISECONDS, Runnerˉenvironment
        );
    const Requireˉcoordinatorˉcleanup = async () => {
        Require((await readdir(Runnerˉtemporary)).length === 0,
            'The coordinator left a private split-compiler directory.');
        Require(!(await readdir(Work)).some(Name =>
            Name.startsWith('.new-windvale-split-compiler-')),
        'The coordinator left a publication candidate.');
    };

    const Output = join(Work, 'Valid.wvb');
    const Valid = await Runˉcoordinator(
        Authenticatedˉarguments(Products, Inputs, Inputs.core, Output), CASES[0]);
    Requireˉcleanˉtermination(Valid, CASES[0]);
    Require(Valid.code === 0 && await lstat(Output).then(() => true, () => false),
        'The valid empty-catalog route did not publish one WVB.');
    await Requireˉcoordinatorˉcleanup();

    const ForeignOutput = join(Work, 'Foreign.wvb');
    const Foreign = await Runˉcoordinator(
        Authenticatedˉarguments(Products, Inputs, Inputs.foreign, ForeignOutput),
        CASES[1]);
    Requireˉcleanˉtermination(Foreign, CASES[1]);
    Require(Foreign.code !== 0 &&
        Foreign.error.toString('utf8').includes('Foreignˉloweringˉpending') &&
        Foreign.output.toString('utf8').includes('step=source-authentication') &&
        Foreign.output.toString('utf8').includes('step=source-foreign-binding') &&
        !Foreign.output.toString('utf8').includes('step=source-analysis') &&
        !Foreign.output.toString('utf8').includes('step=source-emission') &&
        !await lstat(ForeignOutput).then(() => true, () => false),
    'The exact foreign route did not stop at named pending lowering.');
    await Requireˉcoordinatorˉcleanup();

    const SecondOutput = join(Work, 'Valid-Second.wvb');
    const Second = await Runˉcoordinator(
        Authenticatedˉarguments(Products, Inputs, Inputs.core, SecondOutput),
        CASES[2]);
    Requireˉcleanˉtermination(Second, CASES[2]);
    Require(Second.code === 0 &&
        (await readFile(Output)).equals(await readFile(SecondOutput)),
    'Two authenticated builds did not publish byte-identical WVB.');

    await Requireˉcoordinatorˉcleanup();

    const Dummy = [join(Work, 'D.wvss'), join(Work, 'D.wvca'),
        join(Work, 'D.wvlb'), join(Work, 'D.wvir')];
    const Removed = await Runˉbounded(Products.wvanalyze,
        ['--admitted-source-set', Inputs.core, ...Dummy], CASES[3]);
    Require(Removed.code === 64 && Removed.error.toString('utf8').includes(
        'admitted-source-set-route-removed'), 'The old Analyzer route was not rejected.');
    const Removedˉauthenticated = await Runˉbounded(
        Products.wvanalyze, ['--internal-authenticated-admission'], CASES[3]
    );
    Require(Removedˉauthenticated.code === 64 &&
        Removedˉauthenticated.error.toString('utf8').includes(
            'authenticated-admission-route-removed'
        ),
    'The obsolete six-snapshot Analyzer route was not rejected.');

    const RawSources = [
        Buffer.from('#!wv/1 en@1\nmodule R; profile system; authority application;'),
        Buffer.from('#!wv/1 en@1\nmodule R; profile core; platform linux; authority application;'),
        Buffer.from('#!wv/1 en@1\nmodule R; profile core; authority application; unsafe foreign "x" fn F() -> i32 effects(ffi.call) as "x";'),
    ];
    for (let Index = 0; Index < RawSources.length; Index += 1) {
        const Source = join(Work, `Raw-${Index}.wv`);
        await writeFile(Source, RawSources[Index], { flag: 'wx' });
        const Outputs = ['wvss', 'wvca', 'wvlb', 'wvir'].map(
            Extension => join(Work, `Raw-${Index}.${Extension}`)
        );
        const Result = await Runˉbounded(Products.wvanalyze,
            [Source, ...Outputs], CASES[4 + Index]);
        Require(Result.code !== 0 && !await lstat(Outputs[3]).then(() => true, () => false),
            `${CASES[4 + Index]} did not reject without WIR.`);
    }

    for (const Fake of ['--admission-token', '--admission-certificate',
        '--admission-cache-key', '--authenticated-private-path']) {
        const Candidate = join(Work, `Fake-${Fake.slice(2)}.wvb`);
        const Result = await Runˉcoordinator(
            [SPLIT_COMPILER, Products.wvadmit, Products.wvauth,
                Products.wvanalyze, Products.wvemit,
                '--foreign-binder', Products.wvbind, Fake, 'forged',
                Inputs.core, Candidate], CASES[7]);
        Require(Result.code !== 0 && !await lstat(Candidate).then(() => true, () => false),
            `The public coordinator accepted ${Fake}.`);
    }
    await Requireˉcoordinatorˉcleanup();

    // Direct hostile snapshot cases start from exact wvadmit output. Host SHA-256
    // is used only to construct attacker-controlled rehashed WVAE test inputs.
    const Wvss1 = join(Work, 'Attack-Input.wvss');
    await writeFile(Wvss1, Constructˉwvss1([Inputs.Foreignˉsource]), { flag: 'wx' });
    const Snapshots = ['Admitted.wvss', 'Admitted.wvtd', 'Catalog.wvfc', 'Evidence.wvae']
        .map(Name => join(Work, Name));
    const Admit = await Runˉbounded(Products.wvadmit, [
        '--source-input-lock', Inputs.lock, Inputs.lockDigest,
        '--source-profile', Inputs.profile, '--target-descriptor', Inputs.target,
        '--source-set', Wvss1, ...Snapshots,
    ], 'hostile-snapshot-base', BUILD_TIMEOUT_MILLISECONDS);
    Require(Admit.code === 0, 'Could not create the hostile snapshot base.');
    const Base = await Promise.all(Snapshots.map(Path => readFile(Path)));

    // Build a separately valid empty-catalog Windows admission, then pair its
    // admitted source set with the Linux target and rehash only the WVAE target
    // field. This proves wvauth checks every module header even when no foreign
    // declaration causes catalog traversal.
    const Windowsˉwvss1 = join(Work, 'Attack-Empty-Windows-Input.wvss');
    const Windowsˉsource = Buffer.from(
        Inputs.Coreˉsource.toString('utf8').replace(
            'platform linux;', 'platform windows;'
        ),
        'utf8'
    );
    await writeFile(
        Windowsˉwvss1, Constructˉwvss1([Windowsˉsource]), { flag: 'wx' }
    );
    const Windowsˉtarget = join(Work, 'Attack-Windows-Target.wvtd');
    await writeFile(Windowsˉtarget, Constructˉwvtd(1), { flag: 'wx' });
    const Windowsˉsnapshots = [
        'Windows-Admitted.wvss', 'Windows-Admitted.wvtd',
        'Windows-Catalog.wvfc', 'Windows-Evidence.wvae',
    ].map(Name => join(Work, Name));
    const Windowsˉadmit = await Runˉbounded(Products.wvadmit, [
        '--source-input-lock', Inputs.lock, Inputs.lockDigest,
        '--source-profile', Inputs.profile,
        '--target-descriptor', Windowsˉtarget,
        '--source-set', Windowsˉwvss1, ...Windowsˉsnapshots,
    ], 'empty-catalog-target-mismatch-base', BUILD_TIMEOUT_MILLISECONDS);
    Require(Windowsˉadmit.code === 0,
        'Could not create the empty-catalog target-mismatch base.');
    const Windowsˉbase = await Promise.all(
        Windowsˉsnapshots.map(Path => readFile(Path))
    );
    Require(Windowsˉbase[2].readUInt32LE(12) === 0,
        'The target-mismatch base unexpectedly contains foreign records.');

    const Mixedˉcatalog = join(Work, 'Mixed-Catalog.wvfc');
    await writeFile(Mixedˉcatalog, Base[2], { flag: 'wx' });
    const Guardˉanalyzerˉmarker = join(Work, 'Hostile-Analyzer-launched');
    const Guardˉemitterˉmarker = join(Work, 'Hostile-Emitter-launched');
    const Guardˉanalyzer = await Writeˉlaunchˉguard(
        Work, 'Hostile-analyzer', Guardˉanalyzerˉmarker
    );
    const Guardˉemitter = await Writeˉlaunchˉguard(
        Work, 'Hostile-emitter', Guardˉemitterˉmarker
    );
    for (const [Name, Mode] of [
        ['missing-snapshot', 'missing'],
        ['mixed-snapshot', 'mixed'],
        ['wvae-mutation-coordinator', 'evidence'],
    ]) {
        const Mutatingˉadmitter = await Writeˉadmissionˉsnapshotˉmutator(
            Work, `${Name}-admitter`, Products.wvadmit, Mode, Mixedˉcatalog
        );
        const Hostileˉproducts = {
            wvadmit: Mutatingˉadmitter,
            wvauth: Products.wvauth,
            wvanalyze: Guardˉanalyzer,
            wvbind: Products.wvbind,
            wvemit: Guardˉemitter,
        };
        const Candidate = join(Work, `${Name}.wvb`);
        const Result = await Runˉcoordinator(
            Authenticatedˉarguments(
                Hostileˉproducts, Inputs, Inputs.core, Candidate
            ),
            Name
        );
        const Activity = Result.output.toString('utf8');
        Require(Result.code !== 0 &&
            !await lstat(Candidate).then(() => true, () => false) &&
            !await lstat(Guardˉanalyzerˉmarker).then(() => true, () => false) &&
            !await lstat(Guardˉemitterˉmarker).then(() => true, () => false) &&
            !Activity.includes('step=source-analysis') &&
            !Activity.includes('step=source-emission'),
        `${Name} reached Analyzer, emitter, or public destination.`);
        if (Mode !== 'missing') {
            Require(Activity.includes('step=source-authentication'),
                `${Name} did not reach independent wvauth rejection.`);
        }
        await Requireˉcoordinatorˉcleanup();
    }

    const Rehash = (Envelope, Offset, Value) => {
        const Result = Buffer.from(Envelope);
        Buffer.from(Sha256(Value), 'hex').copy(Result, Offset);
        return Result;
    };
    const Mutations = [];
    Mutations.push(['wvae-mutation', Buffer.from(Base[3]), Base[0], Base[1], Base[2]]);
    Mutations[0][1][64] ^= 1;
    Mutations.push(['truncated-wvae', Base[3].subarray(0, 223), Base[0], Base[1], Base[2]]);
    Mutations.push(['trailing-wvae', Buffer.concat([Base[3], Buffer.from([0])]), Base[0], Base[1], Base[2]]);
    const Empty = Buffer.alloc(48); Empty.write('WVFC'); Empty.writeUInt16LE(1, 4);
    Empty.writeUInt32LE(48, 8); Empty.writeUInt32LE(96, 16);
    Empty.writeUInt32LE(48, 20); Empty.writeUInt32LE(1, 24);
    Mutations.push(['rehashed-empty-catalog', Rehash(Base[3], 128, Empty), Base[0], Base[1], Empty]);
    const Remapped = Buffer.from(Base[2]); Remapped.writeUInt32LE(1, 52);
    Mutations.push(['rehashed-remapped-catalog', Rehash(Base[3], 128, Remapped), Base[0], Base[1], Remapped]);
    const CountMismatch = Buffer.from(Base[2]); CountMismatch.writeUInt32LE(2, 24);
    Mutations.push(['wvss-catalog-mismatch', Rehash(Base[3], 128, CountMismatch), Base[0], Base[1], CountMismatch]);
    const WrongTarget = Constructˉwvtd(2);
    Mutations.push(['wvtd-target-mismatch', Rehash(Base[3], 96, WrongTarget), Base[0], WrongTarget, Base[2]]);
    Mutations.push([
        'empty-catalog-wvtd-target-mismatch',
        Rehash(Windowsˉbase[3], 96, Base[1]),
        Windowsˉbase[0], Base[1], Windowsˉbase[2],
    ]);
    for (const [Name, Wvae, Wvss, Wvtd, Wvfc] of Mutations) {
        const Directory = join(Work, `Mutation-${Name}`);
        await mkdir(Directory);
        const Values = [Wvae, Wvss, Wvtd, Wvfc];
        const Names = ['Evidence.wvae', 'Admitted.wvss', 'Target.wvtd', 'Catalog.wvfc'];
        await Promise.all(Values.map((Value, Index) =>
            writeFile(join(Directory, Names[Index]), Value, { flag: 'wx' })));
        const Result = await Runˉbounded(Products.wvauth, [
            join(Directory, Names[0]), join(Directory, Names[1]),
            join(Directory, Names[2]), join(Directory, Names[3]),
            Inputs.lock, Inputs.profile,
        ], Name);
        const Analyzerˉmarker = join(Directory, 'Analyzer-launched');
        if (Result.code === 0) {
            await writeFile(Analyzerˉmarker, Buffer.alloc(0), { flag: 'wx' });
        }
        Require(Result.code !== 0, `wvauth accepted ${Name}.`);
        if (Name === 'empty-catalog-wvtd-target-mismatch') {
            const Diagnostic = Result.error.toString('utf8');
            Require(
                /^wvauth status=Rejected phase=Source-Catalog authentication-status=17 target-status=4 module=0 declaration=0 offset=[0-9]+\r?\n$/u
                    .test(Diagnostic),
                'The empty-catalog target mismatch did not reach the ' +
                    `independent all-module platform check: ${Diagnostic}`
            );
        }
        if (Name === 'wvae-mutation') {
            Require(!await lstat(Analyzerˉmarker).then(() => true, () => false),
                'Analyzer launch was permitted after WVAE mutation.');
        }
    }

    const Oversized = join(Work, 'Oversized.wv');
    await writeFile(Oversized, Buffer.alloc(4_194_305, 32), { flag: 'wx' });
    const OversizedOutput = join(Work, 'Oversized.wvb');
    const Oversizedˉadmitterˉmarker = join(Work, 'Oversized-admitter-launched');
    const Oversizedˉadmitter = await Writeˉlaunchˉguard(
        Work, 'Oversized-admitter', Oversizedˉadmitterˉmarker
    );
    const Oversizedˉproducts = {
        ...Products,
        wvadmit: Oversizedˉadmitter,
        wvanalyze: Guardˉanalyzer,
        wvbind: Products.wvbind,
        wvemit: Guardˉemitter,
    };
    const OversizedResult = await Runˉcoordinator(
        Authenticatedˉarguments(
            Oversizedˉproducts, Inputs, Oversized, OversizedOutput
        ),
        CASES[12]);
    Require(OversizedResult.code !== 0 && !await lstat(OversizedOutput).then(
        () => true, () => false) &&
        !await lstat(Oversizedˉadmitterˉmarker).then(() => true, () => false),
    'Oversized input reached admission or publication.');
    await Requireˉcoordinatorˉcleanup();

    const Existing = join(Work, 'Existing.wvb');
    const Preserved = Buffer.from('preserve');
    await writeFile(Existing, Preserved, { flag: 'wx' });
    const Alias = join(Work, 'Alias-Input.wvb');
    await writeFile(Alias, Inputs.Coreˉsource, { flag: 'wx' });
    for (const Candidate of [Alias, Existing]) {
        const Source = Candidate === Alias ? Alias : Inputs.core;
        const Original = await readFile(Candidate);
        const Result = await Runˉcoordinator(
            Authenticatedˉarguments(Products, Inputs, Source, Candidate), CASES[16]);
        Require(Result.code !== 0, `The coordinator accepted output ${Candidate}.`);
        Require((await readFile(Candidate)).equals(Original),
            `The coordinator changed preserved output ${Candidate}.`);
    }
    Require((await readFile(Existing)).equals(Preserved),
        'The coordinator changed the pre-existing destination.');
    await Requireˉcoordinatorˉcleanup();
}

async function Main() {
    if (process.argv.length !== 2) {
        Reject('The production-admission-ingress owner accepts no arguments.');
    }
    const Buildˉmode = Getˉproductˉbuildˉmode();
    await Verifyˉcontracts();
    const Temporaryˉroot = await realpath(resolve(tmpdir()));
    const Work = await mkdtemp(join(Temporaryˉroot, TEMPORARY_PREFIX));
    let Passed = false;
    try {
        process.stdout.write('START production admission ingress phase=sentinels item=1/3\n');
        await Runˉsentinelˉinfrastructure(Work);
        process.stdout.write('PASS  production admission ingress phase=sentinels item=1/3 cases=13\n');
        if (process.env.WINDVALE_PRODUCTION_ADMISSION_INGRESS_SENTINELS_ONLY ===
            '1') {
            Passed = true;
            return;
        }
        process.stdout.write('START production admission ingress phase=products item=2/3\n');
        const Products = await Buildˉandˉpackageˉproducts(Work, Buildˉmode);
        process.stdout.write(
            'PASS  production admission ingress phase=products item=2/3 ' +
            `build-mode=${Buildˉmode.name} ` +
            'wvb-identity=Recorded-candidate-match ' +
            `cold-double-build=${Buildˉmode.coldDoubleBuild
                ? 'Verified'
                : 'Not-requested'} profile=7\n`
        );
        const Inputs = await Writeˉproductionˉinputs(Work);
        process.stdout.write('START production admission ingress phase=execute item=3/3 cases=21\n');
        await Runˉproductionˉcases(Work, Products, Inputs);
        Passed = true;
    } finally {
        await Removeˉwork(Work, Temporaryˉroot);
    }
    if (Passed) {
        process.stdout.write(
            'native language 1 production admission ingress status=Passed ' +
            'cases=21 deterministic=Verified profile=7 ' +
            'final-publication-owner=split-compiler\n'
        );
    }
}

await Main();
