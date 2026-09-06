import Assert from 'node:assert/strict';
import { Acquireˉcurrentˉsplitˉcompiler, Constructˉcurrentˉsplitˉcompiler } from './Current-Split-Compiler-Cache-Core.mjs';
import { createHash } from 'node:crypto';
import { spawnSync } from 'node:child_process';
import { realpathSync } from 'node:fs';
import {
    chmod,
    mkdtemp,
    mkdir,
    open,
    readFile,
    readdir,
    realpath,
    rename,
    rm,
    symlink,
    writeFile,
} from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import {
    Orderˉsplitˉprojectˉsourceˉpayloads,
} from './Split-Project-Source-Ordering-Core.mjs';

const SCRIPT_DIRECTORY = path.dirname(fileURLToPath(import.meta.url));
const REPOSITORY_ROOT = path.resolve(SCRIPT_DIRECTORY, '..', '..');
const CACHE_SCRIPT = path.join(
    SCRIPT_DIRECTORY,
    'Build-Cached-Split-Project-Wvb.mjs',
);
const IDENTITY_WRITER = path.join(
    SCRIPT_DIRECTORY,
    'Write-Split-Compiler-Producer-Identity.mjs',
);
const PROJECT = path.join(
    REPOSITORY_ROOT,
    'Projects',
    'Tests',
    'Windvale-Native-Test-Source-Descriptor.wvproj',
);
const HOST = `${process.platform}-${process.arch}`;
const TEMPORARY_PREFIX = 'windvale-split-cache-test-';
const MAXIMUM_DIAGNOSTIC_BYTES = 65_536;
const MAXIMUM_REPORTED_DIAGNOSTIC_CHARACTERS = 4_096;
const FAILURE_TIMEOUT_MILLISECONDS = 30_000;

if (process.arch !== 'x64' ||
    (process.platform !== 'win32' && process.platform !== 'linux')) {
    Reject(`The split cache test does not support ${HOST}.`);
}

const Temporaryˉroot = realpathSync.native(os.tmpdir());
const Allocatedˉtestˉroot = await mkdtemp(
    path.join(Temporaryˉroot, TEMPORARY_PREFIX),
);
let Testˉroot;
try {
    Testˉroot = realpathSync.native(Allocatedˉtestˉroot);
} catch (Error) {
    await rm(Allocatedˉtestˉroot, { recursive: true, force: true });
    throw Error;
}
try {
    const Root = Buffer.from('module Root;\n', 'utf8');
    const Mainˉfile = Buffer.from(
        'module WebAssemblyˉinterpreter;\n',
        'utf8',
    );
    const Envelopeˉfile = Buffer.from(
        'module WebAssemblyˉinterpreterˉenvelope;\n',
        'utf8',
    );
    const Ordered = Orderˉsplitˉprojectˉsourceˉpayloads([
        Root,
        Envelopeˉfile,
        Mainˉfile,
    ]);
    if (Ordered.length !== 3 || Ordered[0] !== Root ||
        Ordered[1] !== Mainˉfile || Ordered[2] !== Envelopeˉfile) {
        Reject('Declared module identities did not determine source order.');
    }
    const Cacheˉroot = path.join(Testˉroot, 'cache');
    const Outputˉroot = path.join(Testˉroot, 'output');
    await mkdir(Outputˉroot);
    const Cleanupˉtestˉpreload = await Writeˉcleanupˉtestˉpreload(Testˉroot);
    const Producer = path.join(Testˉroot, 'producer.bin');
    const Analyzerˉidentity = path.join(Testˉroot, 'analyzer.identity');
    const Emitterˉidentity = path.join(Testˉroot, 'emitter.identity');
    await writeFile(Producer, Buffer.from([0x57]));
    const Producerˉsha256 = createHash('sha256')
        .update(Buffer.from([0x57])).digest('hex');
    const Identityˉresult = spawnSync(process.execPath, [
        IDENTITY_WRITER,
        'analyzer',
        Producer,
        Analyzerˉidentity,
    ], {
        cwd: REPOSITORY_ROOT,
        encoding: 'utf8',
        windowsHide: true,
    });
    if (Identityˉresult.status !== 0 || Identityˉresult.stderr !== '' ||
        !(await readFile(Analyzerˉidentity)).equals(Buffer.from(
            Identity('analyzer', 1, Producerˉsha256), 'ascii'
        ))) {
        Reject(
            'The split producer identity was not published through the ' +
            'temporary directory path.',
        );
    }
    await writeFile(
        Analyzerˉidentity,
        Identity('analyzer', 2, '0'.repeat(64)),
        'ascii',
    );
    await writeFile(
        Emitterˉidentity,
        Identity('emitter', 1, '0'.repeat(64)),
        'ascii',
    );

    const Result = spawnSync(process.execPath, [
        CACHE_SCRIPT,
        PROJECT,
        path.join(Outputˉroot, 'Product.wvb'),
        Producer,
        Analyzerˉidentity,
        Producer,
        Emitterˉidentity,
    ], {
        cwd: REPOSITORY_ROOT,
        encoding: 'utf8',
        env: {
            ...process.env,
            WINDVALE_NATIVE_CACHE_ROOT: Cacheˉroot,
        },
        maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
        timeout: FAILURE_TIMEOUT_MILLISECONDS,
        windowsHide: true,
    });
    if (Result.status === 0 || typeof Result.stderr !== 'string' ||
        !Result.stderr.includes('analyzer producer does not match its identity')) {
        Reject(
            'The forced producer-identity failure was not observed: ' +
            Childˉdiagnostic(Result),
        );
    }
    const Debris = await Findˉtemporaryˉdirectories(Cacheˉroot);
    if (Debris.length !== 0) {
        Reject(`The failed cache publication retained debris: ${Debris[0]}`);
    }

    const Replacementˉrecord = path.join(Testˉroot, 'replacement-path.txt');
    const Replacementˉanalyzer = await Writeˉtestˉproducer(
        Testˉroot,
        'Replacement-Analyzer',
        `import { mkdir, rename, rm, writeFile } from 'node:fs/promises';\n` +
        `import path from 'node:path';\n` +
        `const args=process.argv.slice(2);\n` +
        `const temporary=path.dirname(args.at(-4));\n` +
        `const displaced=temporary+'.allocated';\n` +
        `const record=process.env.WINDVALE_TEST_REPLACEMENT_RECORD;\n` +
        `if(typeof record!=='string'||record.length===0)throw new Error('missing replacement record');\n` +
        `await rename(temporary,displaced);\n` +
        `await mkdir(temporary);\n` +
        `await writeFile(path.join(temporary,'Replacement.marker'),'replacement\\n',{encoding:'ascii',flag:'wx'});\n` +
        `await writeFile(record,temporary,{encoding:'utf8',flag:'wx'});\n` +
        `await rm(displaced,{recursive:true,force:true});\n` +
        `throw new Error('forced temporary-directory replacement');\n`,
    );
    const Replacementˉanalyzerˉidentity = path.join(
        Testˉroot,
        'replacement-analyzer.identity',
    );
    const Replacementˉemitterˉidentity = path.join(
        Testˉroot,
        'replacement-emitter.identity',
    );
    Writeˉidentity(
        'analyzer',
        Replacementˉanalyzer,
        Replacementˉanalyzerˉidentity,
    );
    Writeˉidentity(
        'emitter',
        Replacementˉanalyzer,
        Replacementˉemitterˉidentity,
    );
    const Replacementˉcache = path.join(Testˉroot, 'replacement-cache');
    const Replacementˉresult = spawnSync(process.execPath, [
        CACHE_SCRIPT,
        PROJECT,
        path.join(Outputˉroot, 'Replacement-Product.wvb'),
        Replacementˉanalyzer,
        Replacementˉanalyzerˉidentity,
        Replacementˉanalyzer,
        Replacementˉemitterˉidentity,
    ], {
        cwd: REPOSITORY_ROOT,
        encoding: 'utf8',
        env: {
            ...process.env,
            WINDVALE_NATIVE_CACHE_ROOT: Replacementˉcache,
            WINDVALE_TEST_REPLACEMENT_RECORD: Replacementˉrecord,
        },
        maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
        timeout: FAILURE_TIMEOUT_MILLISECONDS,
        windowsHide: true,
    });
    if (Replacementˉresult.status === 0 ||
        !Replacementˉresult.stderr.includes(
            'split compiler analysis producer exited with status'
        )) {
        Reject(
            'The forced temporary-directory replacement did not fail in ' +
            'the producer: ' + Childˉdiagnostic(Replacementˉresult),
        );
    }
    const Replacement = await readFile(Replacementˉrecord, 'utf8');
    const Replacementˉmarker = await readFile(
        path.join(Replacement, 'Replacement.marker'),
        'ascii',
    ).catch(() => '');
    const Replacementˉdebris = await Findˉtemporaryˉdirectories(
        Replacementˉcache,
    );
    if (Replacementˉmarker !== 'replacement\n' ||
        Replacementˉdebris.length !== 1 ||
        !Sameˉpath(Replacementˉdebris[0], Replacement)) {
        Reject(
            'Cleanup removed or altered the replacement temporary directory.',
        );
    }
    const Replacementˉrelative = path.relative(
        Replacementˉcache,
        Replacement,
    );
    if (Replacementˉrelative.startsWith('..') ||
        path.isAbsolute(Replacementˉrelative) ||
        !path.basename(Replacement).startsWith('.new-')) {
        Reject('Refusing to remove an unexpected replacement fixture.');
    }
    await rm(Replacement, { recursive: true, force: true });
    if ((await Findˉtemporaryˉdirectories(Replacementˉcache)).length !== 0) {
        Reject('The replacement-race fixture retained owned debris.');
    }

    const Cleanupˉrecord = path.join(Testˉroot, 'cleanup-failure-paths.json');
    const Cleanupˉfailureˉanalyzer = await Writeˉtestˉproducer(
        Testˉroot,
        'Cleanup-Failure-Analyzer',
        `import { rename, writeFile } from 'node:fs/promises';\n` +
        `import path from 'node:path';\n` +
        `const args=process.argv.slice(2);\n` +
        `const temporary=path.dirname(args.at(-4));\n` +
        `const family=path.dirname(temporary);\n` +
        `const displaced=family+'.displaced';\n` +
        `const record=process.env.WINDVALE_TEST_CLEANUP_FAILURE_RECORD;\n` +
        `if(typeof record!=='string'||record.length===0)throw new Error('missing cleanup record');\n` +
        `await rename(family,displaced);\n` +
        `await writeFile(record,JSON.stringify({family,displaced}),{encoding:'utf8',flag:'wx'});\n` +
        `throw new Error('forced primary producer failure');\n`,
    );
    const Cleanupˉanalyzerˉidentity = path.join(
        Testˉroot,
        'cleanup-failure-analyzer.identity',
    );
    const Cleanupˉemitterˉidentity = path.join(
        Testˉroot,
        'cleanup-failure-emitter.identity',
    );
    Writeˉidentity(
        'analyzer',
        Cleanupˉfailureˉanalyzer,
        Cleanupˉanalyzerˉidentity,
    );
    Writeˉidentity(
        'emitter',
        Cleanupˉfailureˉanalyzer,
        Cleanupˉemitterˉidentity,
    );
    const Cleanupˉfailureˉcache = path.join(
        Testˉroot,
        'cleanup-failure-cache',
    );
    const Cleanupˉfailureˉresult = spawnSync(process.execPath, [
        CACHE_SCRIPT,
        PROJECT,
        path.join(Outputˉroot, 'Cleanup-Failure-Product.wvb'),
        Cleanupˉfailureˉanalyzer,
        Cleanupˉanalyzerˉidentity,
        Cleanupˉfailureˉanalyzer,
        Cleanupˉemitterˉidentity,
    ], {
        cwd: REPOSITORY_ROOT,
        encoding: 'utf8',
        env: {
            ...process.env,
            WINDVALE_NATIVE_CACHE_ROOT: Cleanupˉfailureˉcache,
            WINDVALE_TEST_CLEANUP_FAILURE_RECORD: Cleanupˉrecord,
        },
        maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
        timeout: FAILURE_TIMEOUT_MILLISECONDS,
        windowsHide: true,
    });
    const Primaryˉdiagnostic =
        'The split compiler analysis producer exited with status';
    const Cleanupˉdiagnostic =
        'Cache temporary cleanup also failed:';
    const Primaryˉoffset = Cleanupˉfailureˉresult.stderr.indexOf(
        Primaryˉdiagnostic,
    );
    const Cleanupˉoffset = Cleanupˉfailureˉresult.stderr.indexOf(
        Cleanupˉdiagnostic,
    );
    if (Cleanupˉfailureˉresult.status === 0 || Primaryˉoffset < 0 ||
        Cleanupˉoffset <= Primaryˉoffset) {
        Reject(
            'Cleanup failure replaced or preceded the primary diagnostic: ' +
            Childˉdiagnostic(Cleanupˉfailureˉresult),
        );
    }
    const Cleanupˉpaths = JSON.parse(
        await readFile(Cleanupˉrecord, 'utf8'),
    );
    for (const Candidate of [Cleanupˉpaths.family, Cleanupˉpaths.displaced]) {
        const Relative = path.relative(Cleanupˉfailureˉcache, Candidate);
        if (Relative.startsWith('..') || path.isAbsolute(Relative)) {
            Reject('Refusing to remove an unexpected cleanup-failure fixture.');
        }
        await rm(Candidate, { recursive: true, force: true });
    }
    if ((await Findˉtemporaryˉdirectories(Cleanupˉfailureˉcache)).length !== 0) {
        Reject('The cleanup-failure fixture retained owned debris.');
    }

    const Boundaryˉrecord = path.join(
        Testˉroot,
        'quarantine-boundary-race.json',
    );
    const Boundaryˉcache = path.join(Testˉroot, 'quarantine-boundary-cache');
    const Boundaryˉresult = spawnSync(process.execPath, [
        '--import', pathToFileURL(Cleanupˉtestˉpreload).href,
        CACHE_SCRIPT,
        PROJECT,
        path.join(Outputˉroot, 'Quarantine-Boundary-Product.wvb'),
        Producer,
        Analyzerˉidentity,
        Producer,
        Emitterˉidentity,
    ], {
        cwd: REPOSITORY_ROOT,
        encoding: 'utf8',
        env: {
            ...process.env,
            WINDVALE_NATIVE_CACHE_ROOT: Boundaryˉcache,
            WINDVALE_SPLIT_CACHE_IMPORTED_TEST_HOOKS: JSON.stringify({
                mode: 'replacement-race',
                record: Boundaryˉrecord,
            }),
        },
        maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
        timeout: FAILURE_TIMEOUT_MILLISECONDS,
        windowsHide: true,
    });
    if (Boundaryˉresult.status === 0 ||
        !Boundaryˉresult.stderr.includes(
            'analyzer producer does not match its identity'
        )) {
        Reject(
            'The quarantine-boundary replacement did not retain the primary ' +
            `failure: ${Childˉdiagnostic(Boundaryˉresult)}`,
        );
    }
    const Boundaryˉpaths = JSON.parse(await readFile(Boundaryˉrecord, 'utf8'));
    const Boundaryˉmarker = await readFile(
        path.join(Boundaryˉpaths.quarantine, 'Replacement.marker'),
        'ascii',
    ).catch(() => '');
    if (Boundaryˉmarker !== 'replacement\n') {
        Reject('Recursive cleanup deleted the quarantine-boundary replacement.');
    }
    for (const Candidate of [
        Boundaryˉpaths.displaced,
        Boundaryˉpaths.quarantine,
    ]) {
        const Relative = path.relative(Boundaryˉcache, Candidate);
        if (Relative.startsWith('..') || path.isAbsolute(Relative) ||
            !(path.basename(Candidate).startsWith('.new-') ||
                path.basename(Candidate).startsWith('.remove-'))) {
            Reject('Refusing to remove an unexpected quarantine-race fixture.');
        }
        await rm(Candidate, { recursive: true, force: true });
    }
    if ((await Findˉtemporaryˉdirectories(Boundaryˉcache)).length !== 0) {
        Reject('The quarantine-boundary fixture retained owned debris.');
    }

    const Nonˉerrorˉrecord = path.join(Testˉroot, 'non-error-primary-path.txt');
    const Nonˉerrorˉcache = path.join(Testˉroot, 'non-error-primary-cache');
    const Nonˉerrorˉresult = spawnSync(process.execPath, [
        '--import', pathToFileURL(Cleanupˉtestˉpreload).href,
        CACHE_SCRIPT,
        PROJECT,
        path.join(Outputˉroot, 'Non-Error-Primary-Product.wvb'),
        Producer,
        Analyzerˉidentity,
        Producer,
        Emitterˉidentity,
    ], {
        cwd: REPOSITORY_ROOT,
        encoding: 'utf8',
        env: {
            ...process.env,
            WINDVALE_NATIVE_CACHE_ROOT: Nonˉerrorˉcache,
            WINDVALE_SPLIT_CACHE_IMPORTED_TEST_HOOKS: JSON.stringify({
                mode: 'non-error-primary',
                record: Nonˉerrorˉrecord,
            }),
        },
        maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
        timeout: FAILURE_TIMEOUT_MILLISECONDS,
        windowsHide: true,
    });
    const Nonˉerrorˉprimary = Nonˉerrorˉresult.stderr.indexOf(
        'forced non-error primary'
    );
    const Nonˉerrorˉcleanup = Nonˉerrorˉresult.stderr.indexOf(
        'Cache temporary cleanup also failed: Error: ' +
        'forced quarantine cleanup failure'
    );
    if (Nonˉerrorˉresult.status === 0 || Nonˉerrorˉprimary < 0 ||
        Nonˉerrorˉcleanup <= Nonˉerrorˉprimary) {
        Reject(
            'The non-Error primary was not preserved before cleanup: ' +
            Childˉdiagnostic(Nonˉerrorˉresult),
        );
    }
    const Nonˉerrorˉtemporary = await readFile(Nonˉerrorˉrecord, 'utf8');
    const Nonˉerrorˉrelative = path.relative(
        Nonˉerrorˉcache,
        Nonˉerrorˉtemporary,
    );
    if (Nonˉerrorˉrelative.startsWith('..') ||
        path.isAbsolute(Nonˉerrorˉrelative) ||
        !path.basename(Nonˉerrorˉtemporary).startsWith('.new-')) {
        Reject('Refusing to remove an unexpected non-Error fixture.');
    }
    await rm(Nonˉerrorˉtemporary, { recursive: true, force: true });
    if ((await Findˉtemporaryˉdirectories(Nonˉerrorˉcache)).length !== 0) {
        Reject('The non-Error cleanup-failure fixture retained owned debris.');
    }

    const Rawˉanalyzer = await Writeˉtestˉproducer(
        Testˉroot,
        'Raw-Analyzer',
        `import { writeFile } from 'node:fs/promises';\n` +
        `import path from 'node:path';\n` +
        `const args=process.argv.slice(2);\n` +
        `const expected=JSON.parse(process.env.WINDVALE_TEST_EXPECTED_ANALYZER_INPUTS ?? '[]');\n` +
        `const key=value=>process.platform==='win32'?path.resolve(value).toLowerCase():path.resolve(value);\n` +
        `if(args.length!==expected.length+4)throw new Error('unexpected analyzer argument count');\n` +
        `for(let index=0;index<expected.length;index+=1){if(key(args[index])!==key(expected[index]))throw new Error('unexpected analyzer source order');}\n` +
        `if(args[0]==='--admitted-source-set')throw new Error('removed admitted route was used');\n` +
        `const outputs=args.slice(expected.length);\n` +
        `if(outputs.map(value=>path.extname(value)).join(',')!=='.wvss,.wvca,.wvlb,.wvir')throw new Error('unexpected analyzer outputs');\n` +
        `await writeFile(outputs[0],Buffer.from('WVSS'));\n` +
        `await writeFile(outputs[1],Buffer.alloc(104,0x41));\n` +
        `await writeFile(outputs[2],Buffer.from('WVLB'));\n` +
        `await writeFile(outputs[3],Buffer.from('WVIR'));\n` +
        `process.stdout.write('test analyzer status=Passed\\n');\n`,
    );
    const Rawˉemitter = await Writeˉtestˉproducer(
        Testˉroot,
        'Raw-Emitter',
        `import { writeFile } from 'node:fs/promises';\n` +
        `import path from 'node:path';\n` +
        `const args=process.argv.slice(2);\n` +
        `if(args.length!==5)throw new Error('unexpected emitter argument count');\n` +
        `if(args.slice(0,4).map(value=>path.extname(value)).join(',')!=='.wvss,.wvca,.wvlb,.wvir')throw new Error('unexpected emitter inputs');\n` +
        `await writeFile(args[4],Buffer.from([0x57]));\n` +
        `process.stdout.write('test emitter status=Passed\\n');\n`,
    );
    const Rawˉanalyzerˉidentity = path.join(
        Testˉroot, 'raw-analyzer.identity'
    );
    const Rawˉemitterˉidentity = path.join(Testˉroot, 'raw-emitter.identity');
    Writeˉidentity('analyzer', Rawˉanalyzer, Rawˉanalyzerˉidentity);
    Writeˉidentity('emitter', Rawˉemitter, Rawˉemitterˉidentity);
    const Rawˉoutput = path.join(Outputˉroot, 'Raw-Product.wvb');
    const Rawˉresult = spawnSync(process.execPath, [
        CACHE_SCRIPT,
        PROJECT,
        Rawˉoutput,
        Rawˉanalyzer,
        Rawˉanalyzerˉidentity,
        Rawˉemitter,
        Rawˉemitterˉidentity,
    ], {
        cwd: REPOSITORY_ROOT,
        encoding: 'utf8',
        env: {
            ...process.env,
            WINDVALE_NATIVE_CACHE_ROOT: path.join(Testˉroot, 'raw-cache'),
            WINDVALE_TEST_EXPECTED_ANALYZER_INPUTS: JSON.stringify([
                path.join(
                    REPOSITORY_ROOT,
                    'Tests', 'Fixtures', 'Language-1.0',
                    'Source-Descriptor-Self-Test.wv',
                ),
                path.join(
                    REPOSITORY_ROOT,
                    'Compiler', 'Windvale', 'Source-Descriptor-Core.wv',
                ),
            ]),
        },
        maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
        timeout: FAILURE_TIMEOUT_MILLISECONDS,
        windowsHide: true,
    });
    if (Rawˉresult.status !== 0 || Rawˉresult.stderr !== '' ||
        !(await readFile(Rawˉoutput)).equals(Buffer.from([0x57]))) {
        Reject(
            'The cache did not use the retained raw Project 2 Analyzer route: ' +
            Childˉdiagnostic(Rawˉresult),
        );
    }
    await Verifyˉsymbolˉcheckpointˉresume(Testˉroot, Outputˉroot);
    await Verifyˉcurrentˉcompilerˉcheckpoint(Testˉroot);
    await Verifyˉcompilerˉconstructionˉbranches(Testˉroot);
    console.log(
        'split project cache test cases=28 status=Passed current-compiler-pair=Verified ' +
        'module-order=Passed identity-publication=Passed ' +
        'forced-failure-cleanup=Passed replacement-race=Passed ' +
        'primary-cleanup-diagnostics=Passed ' +
        'quarantine-boundary-race=Passed non-error-primary=Passed ' +
        'raw-project2-route=Passed symbol-resume=Passed ' +
        'symbol-corruption=Rejected final-product-reuse=Passed ' +
        'final-product-corruption=Rejected analysis-key-corruption=Rejected ' +
        'producer-change=Rejected',
    );
} finally {
    const Resolved = path.resolve(Testˉroot);
    if (!Sameˉpath(path.dirname(Resolved), Temporaryˉroot) ||
        !path.basename(Resolved).startsWith(TEMPORARY_PREFIX)) {
        Reject('Refusing to remove an unexpected split cache test directory.');
    }
    await rm(Resolved, { recursive: true, force: true });
}

function Childˉdiagnostic(Result) {
    const Status = Result.status === null ? 'null' : String(Result.status);
    const Signal = Result.signal === null ? 'none' : String(Result.signal);
    const Spawnˉerror = Result.error === undefined
        ? 'none'
        : Diagnosticˉtext(`${Result.error.name}:${Result.error.message}`);
    return `status=${Status} signal=${Signal} ` +
        `spawn-error=${JSON.stringify(Spawnˉerror)} ` +
        `stdout=${JSON.stringify(Diagnosticˉtext(Result.stdout))} ` +
        `stderr=${JSON.stringify(Diagnosticˉtext(Result.stderr))}`;
}

function Diagnosticˉtext(Value) {
    const Text = typeof Value === 'string' ? Value : String(Value ?? '');
    if (Text.length <= MAXIMUM_REPORTED_DIAGNOSTIC_CHARACTERS) {
        return Text;
    }
    return Text.slice(0, MAXIMUM_REPORTED_DIAGNOSTIC_CHARACTERS) +
        `...[truncated characters=${Text.length}]`;
}

function Identity(Role, Bytes, Sha256) {
    const Target = Role === 'analyzer'
        ? 'source-analysis-v1'
        : 'portable-wvb-optimized-v1';
    return `windvale-split-compiler-producer 2\n` +
        `role ${Role}\n` +
        `target ${Target}\n` +
        `host ${HOST}\n` +
        `bytes ${Bytes}\n` +
        `sha256 ${Sha256}\n`;
}

function Writeˉidentity(Role, Producer, Destination) {
    const Result = spawnSync(process.execPath, [
        IDENTITY_WRITER,
        Role,
        Producer,
        Destination,
    ], {
        cwd: REPOSITORY_ROOT,
        encoding: 'utf8',
        windowsHide: true,
    });
    if (Result.status !== 0 || Result.stderr !== '') {
        Reject(
            `The ${Role} test identity could not be written: ` +
            Childˉdiagnostic(Result),
        );
    }
}

async function Writeˉtestˉproducer(Directory, Stem, Program) {
    const Module = path.join(Directory, `${Stem}.mjs`);
    await writeFile(Module, Program, { encoding: 'ascii', flag: 'wx' });
    const Extension = process.platform === 'win32' ? '.cmd' : '.sh';
    const Producer = path.join(Directory, `${Stem}${Extension}`);
    const Wrapper = process.platform === 'win32'
        ? `@echo off\r\nnode "%~dp0${Stem}.mjs" %*\r\n`
        : `#!/usr/bin/env sh\nexec node "$(dirname "$0")/${Stem}.mjs" "$@"\n`;
    await writeFile(Producer, Wrapper, { encoding: 'ascii', flag: 'wx' });
    if (process.platform !== 'win32') {
        await chmod(Producer, 0o755);
    }
    return Producer;
}

async function Verifyˉsymbolˉcheckpointˉresume(Testˉroot, Outputˉroot) {
    const Record = path.join(Testˉroot, 'symbol-checkpoint-phases.txt');
    const Failureˉmarker = path.join(
        Testˉroot,
        'symbol-checkpoint-analysis-failed.txt',
    );
    const Analyzer = await Writeˉtestˉproducer(
        Testˉroot,
        'Symbol-Checkpoint-Analyzer',
        `import { appendFile, writeFile } from 'node:fs/promises';\n` +
        `const args=process.argv.slice(2);\n` +
        `const record=process.env.WINDVALE_TEST_PHASE_RECORD;\n` +
        `const marker=process.env.WINDVALE_TEST_FAIL_ANALYSIS_ONCE;\n` +
        `if(typeof record!=='string'||record.length===0)throw new Error('missing phase record');\n` +
        `if(args[0]==='--internal-symbol-checkpoint'){\n` +
        `if(args.length<4)throw new Error('unexpected symbol argument count');\n` +
        `await appendFile(record,'symbols\\n');\n` +
        `await writeFile(args.at(-2),Buffer.from('WVSS'));\n` +
        `await writeFile(args.at(-1),Buffer.from('WVSY'));\n` +
        `process.stdout.write('test symbols status=Passed\\n');}\n` +
        `else if(args[0]==='--internal-analysis-checkpoint'){\n` +
        `if(args.length!==6)throw new Error('unexpected analysis argument count');\n` +
        `await appendFile(record,'analysis\\n');\n` +
        `let fail=false;try{await writeFile(marker,'failed\\n',{flag:'wx'});fail=true;}` +
        `catch(error){if(error?.code!=='EEXIST')throw error;}\n` +
        `if(fail)throw new Error('forced analysis-wir failure');\n` +
        `await writeFile(args[3],Buffer.alloc(104,0x41));\n` +
        `await writeFile(args[4],Buffer.from('WVLB'));\n` +
        `await writeFile(args[5],Buffer.from('WVIR'));\n` +
        `process.stdout.write('test analysis status=Passed\\n');}\n` +
        `else throw new Error('unexpected analyzer mode');\n`,
    );
    const Emitter = await Writeˉtestˉproducer(
        Testˉroot,
        'Symbol-Checkpoint-Emitter',
        `import { appendFile, writeFile } from 'node:fs/promises';\n` +
        `const args=process.argv.slice(2);\n` +
        `const record=process.env.WINDVALE_TEST_PHASE_RECORD;\n` +
        `if(args.length!==5)throw new Error('unexpected emitter argument count');\n` +
        `await appendFile(record,'emission\\n');\n` +
        `await writeFile(args[4],Buffer.from([0x57]));\n` +
        `process.stdout.write('test emitter status=Passed\\n');\n`,
    );
    const Analyzerˉidentity = path.join(
        Testˉroot,
        'symbol-checkpoint-analyzer.identity',
    );
    const Emitterˉidentity = path.join(
        Testˉroot,
        'symbol-checkpoint-emitter.identity',
    );
    Writeˉidentity('analyzer', Analyzer, Analyzerˉidentity);
    Writeˉidentity('emitter', Emitter, Emitterˉidentity);
    const Cacheˉroot = path.join(Testˉroot, 'symbol-checkpoint-cache');
    const Output = path.join(Outputˉroot, 'Symbol-Checkpoint-Product.wvb');
    const Arguments = [
        CACHE_SCRIPT,
        PROJECT,
        Output,
        Analyzer,
        Analyzerˉidentity,
        Emitter,
        Emitterˉidentity,
        '--symbol-checkpoint',
    ];
    const Environment = {
        ...process.env,
        WINDVALE_NATIVE_CACHE_ROOT: Cacheˉroot,
        WINDVALE_TEST_PHASE_RECORD: Record,
        WINDVALE_TEST_FAIL_ANALYSIS_ONCE: Failureˉmarker,
    };
    const First = spawnSync(process.execPath, Arguments, {
        cwd: REPOSITORY_ROOT,
        encoding: 'utf8',
        env: Environment,
        maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
        timeout: FAILURE_TIMEOUT_MILLISECONDS,
        windowsHide: true,
    });
    if (First.status === 0 || !First.stderr.includes(
        'split compiler analysis-wir producer exited with status'
    ) || await readFile(Record, 'ascii') !== 'symbols\nanalysis\n') {
        Reject(
            'The later analysis failure did not preserve one completed ' +
            `symbol phase: ${Childˉdiagnostic(First)}`,
        );
    }
    const Symbolˉfamily = path.join(
        Cacheˉroot,
        'project-symbols-wvsy-v1',
        HOST,
    );
    const Symbolˉentries = (await readdir(Symbolˉfamily, {
        withFileTypes: true,
    })).filter(Entry => Entry.isDirectory() && !Entry.name.startsWith('.'));
    if (Symbolˉentries.length !== 1) {
        Reject('The completed symbol phase did not publish one checkpoint.');
    }
    const Symbols = path.join(
        Symbolˉfamily,
        Symbolˉentries[0].name,
        'Symbols.wvsy',
    );
    await writeFile(Symbols, Buffer.from('BAD!'));
    const Corrupt = spawnSync(process.execPath, Arguments, {
        cwd: REPOSITORY_ROOT,
        encoding: 'utf8',
        env: Environment,
        maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
        timeout: FAILURE_TIMEOUT_MILLISECONDS,
        windowsHide: true,
    });
    if (Corrupt.status === 0 || !Corrupt.stderr.includes(
        'The symbol checkpoint manifest is invalid.'
    ) || await readFile(Record, 'ascii') !== 'symbols\nanalysis\n') {
        Reject(
            'A corrupted symbol checkpoint was executed or accepted: ' +
            Childˉdiagnostic(Corrupt),
        );
    }
    await writeFile(Symbols, Buffer.from('WVSY'));
    const Retry = spawnSync(process.execPath, Arguments, {
        cwd: REPOSITORY_ROOT,
        encoding: 'utf8',
        env: Environment,
        maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
        timeout: FAILURE_TIMEOUT_MILLISECONDS,
        windowsHide: true,
    });
    if (Retry.status !== 0 || Retry.stderr !== '' ||
        !Retry.stdout.includes('step=analysis-symbols cache=Hit') ||
        await readFile(Record, 'ascii') !==
            'symbols\nanalysis\nanalysis\nemission\n' ||
        !(await readFile(Output)).equals(Buffer.from([0x57])) ||
        (await Findˉtemporaryˉdirectories(Cacheˉroot)).length !== 0) {
        Reject(
            'The retry did not reuse the symbol checkpoint and complete: ' +
            Childˉdiagnostic(Retry),
        );
    }
    // A finished product is independent of evicted intermediate checkpoints.
    for (const Namespace of ['project-analysis-wvca-v3', 'project-symbols-wvsy-v1']) {
        await rename(path.join(Cacheˉroot, Namespace),
            path.join(Cacheˉroot, `${Namespace}.evicted`));
    }
    const Completedˉphases = await readFile(Record, 'ascii');
    const Runˉagain = () => spawnSync(process.execPath, Arguments, {
        cwd: REPOSITORY_ROOT, encoding: 'utf8', env: Environment,
        maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
        timeout: FAILURE_TIMEOUT_MILLISECONDS, windowsHide: true,
    });
    const Hit = Runˉagain();
    if (Hit.status !== 0 || Hit.stderr !== '' ||
        !Hit.stdout.includes('step=emission cache=Hit') ||
        Hit.stdout.includes('step=analysis') ||
        await readFile(Record, 'ascii') !== Completedˉphases ||
        !(await readFile(Output)).equals(Buffer.from([0x57])) ||
        (await readdir(Cacheˉroot)).some(Name =>
            Name === 'project-analysis-wvca-v3' || Name === 'project-symbols-wvsy-v1')) {
        Reject('The finished product rebuilt evicted analysis: ' + Childˉdiagnostic(Hit));
    }
    const Family = path.join(Cacheˉroot, 'project-split-wvb-optimized-v3', HOST);
    const Entries = await readdir(Family);
    if (Entries.length !== 1 || !/^[0-9a-f]{64}$/u.test(Entries[0])) {
        Reject('The final product did not publish exactly one checkpoint.');
    }
    const Product = path.join(Family, Entries[0], 'Product.wvb');
    const Manifest = path.join(Family, Entries[0], 'Checkpoint.txt');
    const Originalˉmanifest = await readFile(Manifest, 'ascii');
    for (const Mutation of ['product', 'analysis-key']) {
        if (Mutation === 'product') {
            await writeFile(Product, Buffer.from([0x58]));
        } else {
            await writeFile(Manifest, Originalˉmanifest.replace(
                /^analysis-key [0-9a-f]{64}$/mu, `analysis-key ${'0'.repeat(64)}`), 'ascii');
        }
        const Corruption = Runˉagain();
        if (Corruption.status === 0 || !Corruption.stderr.includes(
            'The emission checkpoint manifest is invalid.') ||
            await readFile(Record, 'ascii') !== Completedˉphases) {
            Reject(`The invalid ${Mutation} was accepted or rebuilt analysis: ` +
                Childˉdiagnostic(Corruption));
        }
        await writeFile(Product, Buffer.from([0x57]));
        await writeFile(Manifest, Originalˉmanifest, 'ascii');
    }
    await writeFile(Analyzerˉidentity, Identity('analyzer', 2, '0'.repeat(64)), 'ascii');
    const Changed = Runˉagain();
    if (Changed.status === 0 || !Changed.stderr.includes(
        'analyzer producer does not match its identity') ||
        Changed.stdout.includes('step=emission cache=Hit') ||
        await readFile(Record, 'ascii') !== Completedˉphases) {
        Reject('A changed producer reused the prior finished product: ' +
            Childˉdiagnostic(Changed));
    }
}

async function Writeˉcleanupˉtestˉpreload(Directory) {
    const Candidate = path.join(Directory, 'Split-Cache-Test-Hooks.mjs');
    await writeFile(
        Candidate,
        `import { mkdir, rename, writeFile } from 'node:fs/promises';\n` +
        `const name='WINDVALE_SPLIT_CACHE_IMPORTED_TEST_HOOKS';\n` +
        `const raw=process.env[name];\n` +
        `if(raw===undefined)throw new Error('missing split-cache test hooks');\n` +
        `delete process.env[name];\n` +
        `const config=JSON.parse(raw);\n` +
        `let after=null;let before=null;\n` +
        `if(config.mode==='replacement-race'){\n` +
        `before=async value=>{const displaced=value.candidate+'.owned';` +
        `await rename(value.candidate,displaced);await mkdir(value.candidate);` +
        `await writeFile(value.candidate+'/Replacement.marker','replacement\\n',` +
        `{encoding:'ascii',flag:'wx'});await writeFile(config.record,` +
        `JSON.stringify({...value,displaced}),{encoding:'utf8',flag:'wx'});};}\n` +
        `else if(config.mode==='non-error-primary'){\n` +
        `after=async value=>{await writeFile(config.record,value.path,` +
        `{encoding:'utf8',flag:'wx'});throw 'forced non-error primary';};` +
        `before=async()=>{throw new Error('forced quarantine cleanup failure');};}\n` +
        `else throw new Error('invalid split-cache test hook mode');\n` +
        `globalThis[Symbol.for('windvale.split-cache.test-hooks.v1')]=` +
        `Object.freeze({afterTemporaryIdentified:after,` +
        `beforeQuarantineRename:before});\n`,
        { encoding: 'ascii', flag: 'wx' },
    );
    return Candidate;
}

async function Findˉtemporaryˉdirectories(Root) {
    const Found = [];
    await Visit(Root);
    return Found;

    async function Visit(Directory) {
        const Entries = await readdir(Directory, {
            withFileTypes: true,
        }).catch(error => {
            if (error?.code === 'ENOENT') {
                return [];
            }
            throw error;
        });
        for (const Entry of Entries) {
            if (!Entry.isDirectory()) {
                continue;
            }
            const Candidate = path.join(Directory, Entry.name);
            if (Entry.name.startsWith('.new-')) {
                Found.push(Candidate);
            }
            await Visit(Candidate);
        }
    }
}

function Sameˉpath(Left, Right) {
    return process.platform === 'win32'
        ? Left.toLowerCase() === Right.toLowerCase()
        : Left === Right;
}

function Reject(Message) {
    throw new Error(Message);
}

async function Verifyˉcurrentˉcompilerˉcheckpoint(Testˉroot) {
    const Family = path.join(Testˉroot, 'current-compiler-pair');
    await mkdir(Family);
    const Key = '1'.repeat(64);
    const Suffix = process.platform === 'win32' ? 'exe' : 'elf';
    let Constructions = 0;
    let Inputˉchecks = 0;
    const Unchanged = async () => { Inputˉchecks += 1; };
    const Produce = async Place => {
        Constructions += 1;
        for (const [Name, Role] of [['Analyzer', 'analyzer'], ['Emitter', 'emitter']]) {
            const Bytes = Buffer.from(`bounded ${Role} fixture`);
            const Digest = createHash('sha256').update(Bytes).digest('hex');
            await writeFile(path.join(Place, `${Name}.${Suffix}`), Bytes, { mode: 0o755 });
            await writeFile(path.join(Place, `${Name}.identity`), Identity(Role, Bytes.length, Digest));
        }
    };
    const Neverˉproduce = async () => Reject('Unexpected current compiler reconstruction.');
    const Cold = await Acquireˉcurrentˉsplitˉcompiler(Family, Key, Produce, Unchanged);
    const Warm = await Acquireˉcurrentˉsplitˉcompiler(Family, Key, Neverˉproduce, Unchanged);
    Assert.equal(Cold.status, 'Created');
    Assert.equal(Warm.status, 'Hit');
    Assert.equal(Cold.directory, Warm.directory);
    Assert.equal(Constructions, 1);
    Assert.equal(Inputˉchecks, 3);

    const Product = path.join(Cold.directory, `Analyzer.${Suffix}`);
    const Original = await readFile(Product);
    await writeFile(Product, Buffer.from('corrupt product'));
    await Assert.rejects(() => Acquireˉcurrentˉsplitˉcompiler(
        Family, Key, Neverˉproduce, Unchanged), /analyzer identity differs/);
    await writeFile(Product, Original);

    const Record = path.join(Cold.directory, 'Checkpoint.json');
    const Originalˉrecord = await readFile(Record);
    await writeFile(Record, Buffer.from('{}\n'));
    await Assert.rejects(() => Acquireˉcurrentˉsplitˉcompiler(
        Family, Key, Neverˉproduce, Unchanged), /checkpoint record differs/);
    await writeFile(Record, Originalˉrecord);

    const Productˉidentity = path.join(Cold.directory, 'Analyzer.identity');
    const Originalˉidentity = await readFile(Productˉidentity);
    await writeFile(Productˉidentity, Originalˉidentity.toString().replace(
        'source-analysis-v1', 'portable-wvb-optimized-v1'));
    await Assert.rejects(() => Acquireˉcurrentˉsplitˉcompiler(
        Family, Key, Neverˉproduce, Unchanged), /analyzer identity differs/);
    await writeFile(Productˉidentity, Originalˉidentity);

    const Handle = await open(Product, 'r+');
    try { await Handle.truncate(67_108_865); }
    finally { await Handle.close(); }
    await Assert.rejects(() => Acquireˉcurrentˉsplitˉcompiler(
        Family, Key, Neverˉproduce, Unchanged), /bounded ordinary file/);
    await writeFile(Product, Original);
    if (process.platform === 'linux') {
        await chmod(Product, 0o644);
        try {
            await Assert.rejects(() => Acquireˉcurrentˉsplitˉcompiler(
                Family, Key, Neverˉproduce, Unchanged), /bounded ordinary file/);
        } finally { await chmod(Product, 0o755); }
    }
    const Holding = path.join(Family, 'held-valid-pair');
    await rename(Cold.directory, Holding);
    try {
        await symlink(Holding, Cold.directory, process.platform === 'win32' ? 'junction' : 'dir');
        await Assert.rejects(() => Acquireˉcurrentˉsplitˉcompiler(
            Family, Key, Neverˉproduce, Unchanged), /link or non-directory/);
    } finally {
        // The link and its resolved target are both inside this private test family.
        const Resolved = await realpath(Cold.directory);
        Assert.equal(Resolved, Holding);
        await rm(Cold.directory, { recursive: true, force: true });
        await rename(Holding, Cold.directory);
    }

    const Changed = async () => Reject('Compiler inputs changed.');
    await Assert.rejects(() => Acquireˉcurrentˉsplitˉcompiler(
        Family, Key, Neverˉproduce, Changed), /Compiler inputs changed/);
    const Changedˉkey = '2'.repeat(64);
    await Assert.rejects(() => Acquireˉcurrentˉsplitˉcompiler(
        Family, Changedˉkey, Produce, Changed), /Compiler inputs changed/);
    Assert.equal((await readdir(Family)).includes(Changedˉkey), false);
    const Changedˉproduct = await Acquireˉcurrentˉsplitˉcompiler(
        Family, Changedˉkey, Produce, Unchanged);
    Assert.equal(Changedˉproduct.status, 'Created');

    const Interruptedˉkey = '3'.repeat(64);
    await Assert.rejects(() => Acquireˉcurrentˉsplitˉcompiler(
        Family, Interruptedˉkey, async Place => {
            await writeFile(path.join(Place, `Analyzer.${Suffix}`), Original);
            Reject('Compiler construction interrupted.');
        }, Unchanged), /construction interrupted/);
    Assert.equal((await readdir(Family)).some(Name => Name.startsWith('.new-')), false);
    Assert.equal((await readdir(Family)).includes(Interruptedˉkey), false);

    const Raceˉkey = '4'.repeat(64);
    let Arrivals = 0;
    let Release;
    const Ready = new Promise(Resolve => { Release = Resolve; });
    const Racingˉproducer = async Place => {
        await Produce(Place);
        Arrivals += 1;
        if (Arrivals === 2) Release();
        await Ready;
    };
    const Race = await Promise.all([
        Acquireˉcurrentˉsplitˉcompiler(Family, Raceˉkey, Racingˉproducer, Unchanged),
        Acquireˉcurrentˉsplitˉcompiler(Family, Raceˉkey, Racingˉproducer, Unchanged),
    ]);
    Assert.deepEqual(Race.map(Value => Value.status).sort(), ['Created', 'Hit']);
    Assert.equal((await readdir(Family)).some(Name => Name.startsWith('.new-')), false);

    const Extra = path.join(Cold.directory, 'Unexpected');
    await writeFile(Extra, Buffer.from('extra'));
    await Assert.rejects(() => Acquireˉcurrentˉsplitˉcompiler(
        Family, Key, Neverˉproduce, Unchanged), /inventory bound/);
    await rm(Extra);
    Assert.equal((await Acquireˉcurrentˉsplitˉcompiler(
        Family, Key, Neverˉproduce, Unchanged)).status, 'Hit');
}

async function Verifyˉcompilerˉconstructionˉbranches(Testˉroot) {
    const Suffix = process.platform === 'win32' ? '.exe' : '.elf';
    for (const Mode of ['complete', 'analyzer-failure', 'emitter-failure', 'both-failures', 'preparation-failure']) {
        const Work = path.join(Testˉroot, Mode);
        const Candidate = path.join(Work, 'Pair');
        await mkdir(Candidate, { recursive: true });
        const Calls = [];
        const Completed = new Set();
        let Active = 0;
        let Maximumˉactive = 0;
        let Arrivals = 0;
        let Settled = false;
        let Release;
        let Announce;
        const Ready = new Promise(Resolve => { Announce = Resolve; });
        const Peer = new Promise(Resolve => { Release = Resolve; });
        const Timer = setTimeout(() => { Announce(); Release(); }, 2_000);
        const Run = async (Label, Name, Arguments) => {
            Assert.equal(Calls.includes(Label), false);
            Calls.push(Label);
            Active += 1;
            Maximumˉactive = Math.max(Active, Maximumˉactive);
            try {
                if (Label === 'stage1-analyzer-build' && Mode === 'preparation-failure') {
                    throw new Error('preparation-failure');
                }
                const Branch = Label === 'stage1-analyzer-package' ? 'analyzer' :
                    Label === 'stage1-emitter-build' ? 'emitter' : null;
                if (Branch !== null) {
                    Assert.ok(Completed.has('stage1-checkpoint-analyzer-identity'));
                    Arrivals += 1;
                    if (Arrivals === 2) Announce();
                    if (Mode === Branch + '-failure' || Mode === 'both-failures') {
                        throw new Error(Branch + '-failure');
                    }
                    await Peer;
                }
                if (Name === 'Package-Segmented-Compiler-Wvb') {
                    Assert.equal(Arguments.length, 4);
                    Assert.equal(Arguments[3], '--development-cache');
                    Assert.equal(Arguments[0], Label === 'pinned-analyzer-package' ||
                        Label === 'stage1-analyzer-package' ? '7' : '8');
                    await readFile(Arguments[1]);
                    await writeFile(Arguments[2], Buffer.from(Label), { mode: 0o755 });
                } else if (Name === 'Write-Split-Compiler-Producer-Identity.mjs') {
                    Assert.equal(Arguments.length, 3);
                    await readFile(Arguments[1]);
                    Assert.equal(Arguments[0], Label.includes('emitter') ? 'emitter' : 'analyzer');
                    await writeFile(Arguments[2], Buffer.from(Label));
                } else {
                    Assert.equal(Name, 'Build-Cached-Split-Project-Wvb.mjs');
                    Assert.equal(Arguments.length, Label === 'stage1-emitter-build' ? 7 : 6);
                    if (Arguments.length === 7) {
                        Assert.equal(Arguments[6], '--symbol-checkpoint');
                        Assert.equal(path.basename(Arguments[2]), 'Checkpoint-Analyzer' + Suffix);
                    }
                    for (const Input of Arguments.slice(0, 6).filter((_, Index) => Index !== 1)) {
                        await readFile(Input);
                    }
                    await writeFile(Arguments[1], Buffer.from(Label));
                }
                Completed.add(Label);
            } finally { Active -= 1; }
        };
        const Construction = Constructˉcurrentˉsplitˉcompiler(Work, Candidate, Run, Run)
            .then(() => { Settled = true; return null; }, Error => { Settled = true; return Error; });
        try {
            if (Mode !== 'preparation-failure') {
                await Ready;
                Assert.equal(Arrivals, 2, 'The independent compiler branches were serialized.');
                await new Promise(Resolve => setImmediate(Resolve));
                if (Mode !== 'both-failures') {
                    Assert.equal(Settled, false, 'Construction abandoned its live peer.');
                    Assert.deepEqual(await readdir(Candidate), []);
                }
                Release();
            }
            const Failure = await Construction;
            Assert.equal(Active, 0);
            Assert.ok(Maximumˉactive <= 2);
            if (Mode === 'complete') {
                Assert.equal(Failure, null);
                Assert.equal(Maximumˉactive, 2);
                Assert.equal(Calls.length, 12);
                const Products = ['Analyzer' + Suffix, 'Analyzer.identity', 'Emitter' + Suffix, 'Emitter.identity'];
                Assert.deepEqual((await readdir(Candidate)).sort(), Products.sort());
                for (const Product of Products) {
                    Assert.deepEqual(await readFile(path.join(Candidate, Product)),
                        await readFile(path.join(Work, Product)));
                }
            } else {
                Assert.deepEqual(await readdir(Candidate), []);
                if (Mode === 'preparation-failure') {
                    Assert.equal(Failure.message, Mode);
                    Assert.equal(Arrivals, 0);
                    Assert.equal(Calls.length, 5);
                } else {
                    Assert.ok(Failure instanceof AggregateError);
                    const Expected = Mode === 'both-failures'
                        ? ['analyzer-failure', 'emitter-failure'] : [Mode];
                    Assert.deepEqual(Failure.errors.map(Error => Error.message), Expected);
                    for (const Failed of Expected) {
                        Assert.equal(Calls.includes('stage1-' + Failed.split('-')[0] + '-identity'), false);
                    }
                }
            }
        } finally { clearTimeout(Timer); Release(); await Construction; }
    }
}
