import Assert from 'node:assert/strict';
import { Acquireˉcallableˉproducts, Parseˉcallableˉarguments, Withˉcallableˉenvironment }
    from './Test-Language-1.0-Callable-Semantics.mjs';
import { Acquireˉauthenticatedˉanalysis } from './Authenticated-Analysis-Cache-Core.mjs';
import { Acquireˉcurrentˉwvbˉpublisher } from './Current-Wvb-Publisher-Core.mjs';
import { Createˉsegmentedˉhostedˉcheckpoint } from './Build-Cached-Segmented-Hosted-Wvb.mjs';
import { Buildˉcachedˉprojectˉwvb } from './Build-Cached-Project-Wvb.mjs';
import { Acquireˉfoundationˉborrowˉtestˉproducts, Parseˉfoundationˉownerˉarguments,
    Withˉfoundationˉownerˉenvironment } from './Foundation-Borrow-Test-Products-Core.mjs';
import {
    Constructˉsourceˉeditionˉpredecessor,
    Hasˉuncertainˉconstructionˉcleanup,
    Runˉcompilerˉconstructionˉcommand,
} from './Source-Edition-Predecessor-Core.mjs';
import {
    Requireˉwindowsˉterminationˉresult,
    Runˉdevelopmentˉcommand,
} from './Development-Command-Core.mjs';
import {
    Acquireˉcurrentˉsplitˉcompiler,
    Constructˉcurrentˉsplitˉcompiler,
    Getˉcurrentˉsplitˉcompilerˉkey,
    Readˉpreparedˉsplitˉcompiler,
    CURRENT_ADMISSION_PROJECTS,
} from './Current-Split-Compiler-Cache-Core.mjs';
import {
    Acquireˉdirectˉconditionˉanalyzer,
    DIRECT_CONDITION_SELECTORS,
    DIRECT_CONDITION_SELECTOR_VERSION,
    Projectˉdirectˉconditionˉanalyzer,
    Requireˉdirectˉconstruction,
} from './Direct-Condition-Analyzer-Intermediate-Core.mjs';
import { createHash } from 'node:crypto';
import { spawnSync } from 'node:child_process';
import { realpathSync } from 'node:fs';
import {
    chmod,
    lstat,
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
let PROJECT;
const HOST = `${process.platform}-${process.arch}`;
const TEMPORARY_PREFIX = 'windvale-split-cache-test-';
const MAXIMUM_DIAGNOSTIC_BYTES = 65_536;
const MAXIMUM_REPORTED_DIAGNOSTIC_CHARACTERS = 4_096;
const FAILURE_TIMEOUT_MILLISECONDS = 30_000;

if (process.argv[2] === '--construction-readiness') {
    if (process.argv.length !== 3) Reject('Usage: --construction-readiness');
    await Verifyˉconstructionˉreadiness();
    process.exit(0);
}

if (process.argv[2] === '--project4') {
    if (process.argv.length !== 11) Reject('Expected --project4 analyzer analyzer.identity emitter emitter.identity admit auth reader binder.');
    await Verifyˉauthenticatedˉprojectˉcache(process.argv.slice(3).map(Value => path.resolve(Value)));
    process.exit(0);
}

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
const Projectˉparentˉpath = path.join(REPOSITORY_ROOT, 'Artifacts', 'Work');
await mkdir(Projectˉparentˉpath, { recursive: true });
const Projectˉparent = realpathSync.native(Projectˉparentˉpath);
const Projectˉroot = await mkdtemp(path.join(Projectˉparent, TEMPORARY_PREFIX));
try {
    if (process.argv[2] === '--publisher-cache') {
        if (process.argv.length !== 3) Reject('Usage: --publisher-cache');
        await Verifyˉpreparedˉpublisher(Testˉroot);
    } else {
    await Verifyˉauthenticatedˉanalysisˉcache(Testˉroot);
    if (process.argv[2] === '--analysis-cache') process.exitCode = 0;
    else {
    PROJECT = path.join(Projectˉroot, 'Legacy.wvproj');
    const Legacyˉsource = path.join(Projectˉroot, 'Legacy.wv');
    await writeFile(Legacyˉsource, 'module Legacy profile portable;\nexport fn Main() -> i32 { return 42; }\n');
    await writeFile(PROJECT, 'windvale-project 2\nroot "' +
        path.relative(REPOSITORY_ROOT, Legacyˉsource).replaceAll('\\', '/') + '"\nemit wvb\n');
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
            WINDVALE_TEST_EXPECTED_ANALYZER_INPUTS: JSON.stringify([Legacyˉsource]),
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
    const Artifactˉcases = await Verifyˉartifactˉreaderˉprojections();
    const Directˉcases = await Verifyˉcompilerˉconstructionˉcontrols(Testˉroot);
    const Foundationˉcases = await Verifyˉfoundationˉtestˉproducts(Testˉroot);
    const Foundationˉownerˉcases = await Verifyˉfoundationˉownerˉpreparation(Testˉroot);
    const Callableˉphaseˉcases = await Verifyˉcallableˉphases(Testˉroot);
    const Deadlineˉcases = await Verifyˉconstructionˉdeadlineˉadmission();
    const Preparationˉcases = await Verifyˉcompilerˉpreparationˉcli(Testˉroot);
    const Processˉcases = await Verifyˉconstructionˉprocessˉstatuses(Testˉroot);
    const Inspectionˉcases = await Verifyˉfunctionˉlimitˉdiagnostics(Testˉroot);
    const Legacyˉcases = await Verifyˉlegacyˉprojectˉcheckpoint(Testˉroot);
    const Publisherˉcases = await Verifyˉpreparedˉpublisher(Testˉroot);
    console.log(
        `split project cache test cases=${41 + Artifactˉcases + Directˉcases + Foundationˉcases + Foundationˉownerˉcases + Callableˉphaseˉcases + Deadlineˉcases + Preparationˉcases + Processˉcases + Inspectionˉcases + Legacyˉcases + Publisherˉcases} status=Passed current-compiler-pair=Verified ` +
        'module-order=Passed identity-publication=Passed ' +
        'forced-failure-cleanup=Passed replacement-race=Passed ' +
        'primary-cleanup-diagnostics=Passed ' +
        'quarantine-boundary-race=Passed non-error-primary=Passed ' +
        'raw-project2-route=Passed symbol-resume=Passed ' +
        'symbol-corruption=Rejected final-product-reuse=Passed ' +
        'final-product-corruption=Rejected analysis-key-corruption=Rejected ' +
        `producer-change=Rejected foundation-test-products=${Foundationˉcases} foundation-owner-preparation=${Foundationˉownerˉcases} callable-phases=${Callableˉphaseˉcases} construction-deadlines=${Deadlineˉcases} direct-intermediate=${Directˉcases} artifact-readers=${Artifactˉcases} compiler-preparation=${Preparationˉcases} construction-statuses=${Processˉcases} function-limit-diagnostics=${Inspectionˉcases} legacy-project-cache=${Legacyˉcases} prepared-publisher=${Publisherˉcases}`,
    );
    }
    }
} finally {
    const Resolved = path.resolve(Testˉroot);
    if (!Sameˉpath(path.dirname(Resolved), Temporaryˉroot) ||
        !path.basename(Resolved).startsWith(TEMPORARY_PREFIX)) {
        Reject('Refusing to remove an unexpected split cache test directory.');
    }
    await rm(Resolved, { recursive: true, force: true });
    if (!Sameˉpath(path.dirname(Projectˉroot), Projectˉparent) ||
        !path.basename(Projectˉroot).startsWith(TEMPORARY_PREFIX)) {
        Reject('Refusing to remove an unexpected legacy project fixture directory.');
    }
    await rm(Projectˉroot, { recursive: true, force: false });
}

async function Verifyˉlegacyˉprojectˉcheckpoint(Root) {
    const Previousˉcache = process.env.WINDVALE_NATIVE_CACHE_ROOT;
    const Previousˉprepared = process.env.WINDVALE_PREPARED_COMPILER_ONLY;
    const Cache = path.join(Root, 'legacy-checkpoints');
    const Output = path.join(Root, 'Legacy-Output.wvb');
    const Windows = process.platform === 'win32';
    const Family = path.join(Cache, 'project-wvb-v2', Windows ? 'windows-x64' : 'linux-x64');
    const Wrapper = path.join(SCRIPT_DIRECTORY, `Build-Cached-Project-Wvb.${Windows ? 'cmd' : 'sh'}`);
    let Cases = 0;
    function Pass(Name) {
        Cases += 1;
        console.log(`legacy project cache item=${Cases} case=${Name} status=Passed`);
    }
    function Record(Key, Product) {
        return Buffer.from(['windvale-native-project-wvb-checkpoint 1', `key ${Key}`,
            `wvb-bytes ${Product.length}`,
            `wvb-sha256 ${createHash('sha256').update(Product).digest('hex')}`, '']
            .join(Windows ? '\r\n' : '\n'));
    }
    async function Launch(Project = PROJECT) {
        return Runˉdevelopmentˉcommand(Windows ? Wrapper : 'bash',
            Windows ? [Project, Output] : [Wrapper, Project, Output], Date.now() + 30_000,
            false, MAXIMUM_DIAGNOSTIC_BYTES);
    }
    try {
        process.env.WINDVALE_NATIVE_CACHE_ROOT = Cache;
        delete process.env.WINDVALE_PREPARED_COMPILER_ONLY;
        let Result = await Launch();
        Assert.equal(Result.Code, 0, Result.Error);
        Assert.match(Result.Output, /status=Created key=[0-9a-f]{64}/u);
        const Product = await readFile(Output);
        Assert.equal(Product.subarray(0, 3).toString('ascii'), 'WVB');
        const Key = /key=([0-9a-f]{64})/u.exec(Result.Output)[1];
        const Place = path.join(Family, Key);
        const Recordˉpath = path.join(Place, 'Checkpoint.txt');
        const Productˉpath = path.join(Place, 'Product.wvb');
        const Manifest = Record(Key, Product);
        Assert.deepEqual(await readFile(Recordˉpath), Manifest);
        Pass('cold-build-legacy-record');
        await writeFile(Output, Buffer.alloc(0));
        Result = await Launch();
        Assert.equal(Result.Code, 0, Result.Error);
        Assert.match(Result.Output, /status=Hit/u);
        Assert.deepEqual(await readFile(Output), Product);
        Pass('warm-reuse-empty-output');

        async function Rejected(Name) {
            const Result = await Launch();
            Assert.equal(Result.Code, 1, Result.Error);
            Assert.deepEqual(await readFile(Output), Product, 'Rejection replaced existing output.');
            Pass(Name);
        }
        for (const [Name, Bytes] of [['truncated-record', Manifest.subarray(0, 25)],
            ['oversized-record', Buffer.alloc(1_025)],
            ['wrong-key-record', Buffer.from(Manifest.toString().replace(Key, '0'.repeat(64)))]]) {
            await writeFile(Recordˉpath, Bytes);
            await Rejected(Name);
        }
        await writeFile(Recordˉpath, Manifest);
        const Changed = Buffer.from(Product);
        Changed[Changed.length - 1] ^= 1;
        await writeFile(Productˉpath, Changed);
        await Rejected('corrupt-product');
        await writeFile(Productˉpath, Buffer.alloc(0));
        await Rejected('empty-product');
        const Large = await open(Productˉpath, 'r+');
        try { await Large.truncate(67_108_865); } finally { await Large.close(); }
        await Rejected('oversized-product');
        await writeFile(Productˉpath, Product);
        if (!Windows) {
            const Link = path.join(Root, 'Legacy-Link.wvb');
            await symlink(Output, Link);
            await Assert.rejects(Buildˉcachedˉprojectˉwvb(PROJECT, Link), /non-link file/u);
            Assert.deepEqual(await readFile(Output), Product);
            Pass('linked-output-rejected');
        }
        const Linkedˉcache = path.join(Root, 'Legacy-Linked-Cache');
        await symlink(Cache, Linkedˉcache, Windows ? 'junction' : 'dir');
        process.env.WINDVALE_NATIVE_CACHE_ROOT = Linkedˉcache;
        await Assert.rejects(Buildˉcachedˉprojectˉwvb(PROJECT, Output), /link or non-directory/u);
        Assert.deepEqual(await readFile(Output), Product);
        Pass('linked-cache-rejected');

        async function Constructionˉcase(Name, Execute, Expected) {
            const Private = path.join(Root, `legacy-${Name}`);
            process.env.WINDVALE_NATIVE_CACHE_ROOT = Private;
            if (Expected) await Assert.rejects(Buildˉcachedˉprojectˉwvb(PROJECT, Output, Execute), Expected);
            else await Buildˉcachedˉprojectˉwvb(PROJECT, Output, Execute);
            Assert.deepEqual(await readFile(Output), Product);
            const Entries = await readdir(path.join(Private, 'project-wvb-v2', path.basename(Family)));
            Pass(Name);
            return Entries;
        }
        Assert.deepEqual(await Constructionˉcase('failed-build-cleanup', async () =>
            ({ Code: 1, Output: '', Error: 'deliberate construction failure' }), /deliberate construction failure/u), []);
        const Uncertainˉroot = path.join(Root, 'legacy-uncertain');
        process.env.WINDVALE_NATIVE_CACHE_ROOT = Uncertainˉroot;
        const Probe = `import { Buildˉcachedˉprojectˉwvb as Build } from ${JSON.stringify(
            pathToFileURL(path.join(SCRIPT_DIRECTORY, 'Build-Cached-Project-Wvb.mjs')).href)};
            const Failure = Object.assign(new Error('deliberate uncertain termination'), { cleanupUncertain: true });
            try {
                await Build(${JSON.stringify(PROJECT)}, ${JSON.stringify(Output)}, async () => { throw Failure; });
                throw new Error('The uncertain termination was not reported.');
            } catch (Error) { if (Error !== Failure) throw Error; }`;
        const Uncertainˉresult = await Runˉdevelopmentˉcommand(process.execPath,
            ['--input-type=module', '--eval', Probe], Date.now() + 30_000, false, MAXIMUM_DIAGNOSTIC_BYTES);
        Assert.equal(Uncertainˉresult.Code, 0, Uncertainˉresult.Error);
        Assert.match(Uncertainˉresult.Error, /Preserved project-WVB work after uncertain process termination/u);
        const Retained = await readdir(path.join(Uncertainˉroot, 'project-wvb-v2', path.basename(Family)));
        Assert.equal(Retained.length, 1);
        Assert.ok(Retained[0].startsWith(`.new-${Key}-`));
        Assert.deepEqual(await readFile(Output), Product);
        Pass('uncertain-termination-preserves-work');

        for (const Divergent of [false, true]) {
            const Entries = await Constructionˉcase(Divergent ? 'divergent-race' : 'identical-race',
                async (Command, Arguments) => {
                    const Temporaryˉproduct = Arguments.at(-1);
                    await writeFile(Temporaryˉproduct, Product);
                    const Winner = path.join(path.dirname(path.dirname(Temporaryˉproduct)), Key);
                    await mkdir(Winner);
                    const Winnerˉproduct = Divergent ? Changed : Product;
                    await writeFile(path.join(Winner, 'Product.wvb'), Winnerˉproduct);
                    await writeFile(path.join(Winner, 'Checkpoint.txt'), Record(Key, Winnerˉproduct));
                    return { Code: 0, Output: '', Error: '' };
                }, Divergent ? /Concurrent project-WVB construction differs/u : null);
            Assert.deepEqual(Entries, [Key]);
        }
        const Source = path.join(path.dirname(PROJECT), 'Legacy.wv');
        const Original = await readFile(Source);
        try {
            Assert.deepEqual(await Constructionˉcase('changed-construction-input', async (Command, Arguments) => {
                await writeFile(Arguments.at(-1), Product);
                await writeFile(Source, Buffer.concat([Original, Buffer.from('\n')]));
                return { Code: 0, Output: '', Error: '' };
            }, /construction inputs changed/u), []);
        } finally { await writeFile(Source, Original); }

        const Current = path.join(Root, 'Prepared-Only.wvproj');
        await writeFile(Current, 'windvale-project 4\n');
        process.env.WINDVALE_NATIVE_CACHE_ROOT = path.join(Root, 'legacy-current-missing');
        process.env.WINDVALE_PREPARED_COMPILER_ONLY = '1';
        Result = await Launch(Current);
        Assert.equal(Result.Code, 64, Result.Error);
        Assert.match(Result.Error, /prepar/iu);
        Assert.deepEqual(await readFile(Output), Product);
        Pass('project4-preparation-miss-preserves-output');
        return Cases;
    } finally {
        if (Previousˉcache === undefined) delete process.env.WINDVALE_NATIVE_CACHE_ROOT;
        else process.env.WINDVALE_NATIVE_CACHE_ROOT = Previousˉcache;
        if (Previousˉprepared === undefined) delete process.env.WINDVALE_PREPARED_COMPILER_ONLY;
        else process.env.WINDVALE_PREPARED_COMPILER_ONLY = Previousˉprepared;
    }
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

async function Verifyˉauthenticatedˉanalysisˉcache(Testˉroot) {
    const Previousˉroot = process.env.WINDVALE_NATIVE_CACHE_ROOT;
    const Previousˉmode = process.env.WINDVALE_PREPARED_PRODUCTS_ONLY;
    const Root = path.join(Testˉroot, 'authenticated-analysis');
    await mkdir(Root);
    const Analyzer = path.join(Root, 'Analyzer');
    await writeFile(Analyzer, 'analyzer-v1');
    const Inputs = Array.from({ length: 6 }, (_, Index) => Index === 0 ? Buffer.alloc(37, 1) : Buffer.from('input-' + Index));
    const Values = [Buffer.alloc(37, 1), Buffer.alloc(104, 2), Buffer.from('bindings'), Buffer.from('wir')];
    let Calls = 0, Requests = 0, Cases = 0;
    process.env.WINDVALE_NATIVE_CACHE_ROOT = path.join(Root, 'cache');
    delete process.env.WINDVALE_PREPARED_PRODUCTS_ONLY;
    async function Request(Overrides = {}) {
        const Output = path.join(Root, 'output-' + Requests++);
        await mkdir(Output);
        const Outputs = Values.map((_, Index) => path.join(Output, String(Index)));
        const Options = { Analyzer, Mode: '--internal-source-set', Inputs, Outputs,
            Coordinator: Buffer.from('coordinator-v1'),
            Writeˉprivate: (Name, Bytes) => writeFile(Name, Bytes, { flag: 'wx' }),
            Produce: async () => {
                Calls += 1;
                for (let Index = 0; Index < Values.length; Index += 1) await writeFile(Outputs[Index], Index === 0 ? Options.Inputs[0] : Values[Index], { flag: 'wx' });
                return Buffer.from('source analysis status=Published\n');
            }, ...Overrides };
        const Report = await Acquireˉauthenticatedˉanalysis(Options);
        Assert.equal(Report.toString(), 'source analysis status=Published\n');
        for (let Index = 0; Index < 4; Index += 1) Assert.deepEqual(await readFile(Outputs[Index]), Index === 0 ? Options.Inputs[0] : Values[Index]);
    }
    function Pass(Name) { Cases += 1; console.log(`authenticated analysis cache item=${Cases} case=${Name} status=Passed`); }
    try {
        await Request(); Assert.equal(Calls, 1); Pass('cold-analysis');
        await Request(); Assert.equal(Calls, 1); Pass('warm-private-copy');
        // Simulate an emission failure after a completed analysis: its retry
        // must retain the analyzer product without retaining emitter state.
        await Assert.rejects(async () => { await Request(); throw new Error('later-emission-failure'); }, /later-emission-failure/);
        await Request(); Assert.equal(Calls, 1); Pass('later-failure-reuses-analysis');
        for (let Index = 0; Index < Inputs.length; Index += 1) {
            const Changed = Inputs.map(Bytes => Buffer.from(Bytes)); Changed[Index][0] ^= 1;
            const Before = Calls; await Request({ Inputs: Changed }); Assert.equal(Calls, Before + 1);
            Pass('changed-authenticated-input-' + Index);
        }
        const Beforeˉmode = Calls;
        await Request({ Mode: '--internal-foreign-source-set' }); Assert.equal(Calls, Beforeˉmode + 1); Pass('foreign-mode-separated');
        await Request({ Coordinator: Buffer.from('coordinator-v2') }); Assert.equal(Calls, Beforeˉmode + 2); Pass('coordinator-invalidates');
        await writeFile(Analyzer, 'analyzer-v2'); await Request(); Assert.equal(Calls, Beforeˉmode + 3); Pass('analyzer-invalidates');
        await writeFile(Analyzer, 'analyzer-v1');
        process.env.WINDVALE_PREPARED_PRODUCTS_ONLY = '1';
        const Beforeˉprepared = Calls;
        await Request(); Assert.equal(Calls, Beforeˉprepared); Pass('prepared-hit');
        await Assert.rejects(Request({ Coordinator: Buffer.from('missing') }), /not prepared/);
        Assert.equal(Calls, Beforeˉprepared); Pass('prepared-miss-stops');
        process.env.WINDVALE_PREPARED_PRODUCTS_ONLY = 'invalid';
        await Assert.rejects(Request(), /absent or 1/); Pass('invalid-mode-rejected');
        delete process.env.WINDVALE_PREPARED_PRODUCTS_ONLY;
        await Assert.rejects(Request({ Coordinator: Buffer.from('failure'), Produce: async () => { throw new Error('producer-failed'); } }), /producer-failed/);
        Pass('failed-analysis-not-published');
        await Assert.rejects(Request({ Inputs: [Buffer.alloc(4_194_305), ...Inputs.slice(1)] }), /Invalid authenticated/); Pass('oversized-input-rejected');
        const Family = path.join(Root, 'cache', 'authenticated-analysis-v1', HOST);
        const Directories = await readdir(Family);
        Assert.equal(Directories.some(Name => Name.startsWith('.new-')), false);
        // Locate the original request by its output record and test every
        // retained result under the same request instead of relying on order.
        const Changedˉfiles = [];
        for (const Name of Directories) {
            const File = path.join(Family, Name, 'Wir.wvir');
            const Original = await readFile(File);
            await writeFile(File, Buffer.from('corrupt'));
            Changedˉfiles.push([File, Original]);
        }
        const Beforeˉcorrupt = Calls;
        await Assert.rejects(Request(), /checkpoint record differs/); Assert.equal(Calls, Beforeˉcorrupt); Pass('corrupt-output-rejected');
        for (const [File, Original] of Changedˉfiles) await writeFile(File, Original);
        for (const Name of Directories) await writeFile(path.join(Family, Name, 'Extra'), 'unexpected');
        await Assert.rejects(Request(), /inventory (differs|exceeds)/); Assert.equal(Calls, Beforeˉcorrupt); Pass('unexpected-inventory-rejected');
        for (const Name of Directories) await rm(path.join(Family, Name, 'Extra'));
        const Beforeˉrace = Calls;
        await Promise.all([Request({ Coordinator: Buffer.from('concurrent') }), Request({ Coordinator: Buffer.from('concurrent') })]);
        Assert.ok(Calls >= Beforeˉrace + 1 && Calls <= Beforeˉrace + 2); Pass('concurrent-publication');
        Assert.equal((await readdir(Family)).some(Name => Name.startsWith('.new-')), false); Pass('temporary-cleanup');
        const Mutationˉoutput = path.join(Root, 'mutated-output');
        await mkdir(Mutationˉoutput);
        const Mutationˉpaths = Values.map((_, Index) => path.join(Mutationˉoutput, String(Index)));
        await Assert.rejects(Request({ Coordinator: Buffer.from('mutation'), Outputs: Mutationˉpaths,
            Produce: async () => {
                for (let Index = 0; Index < 4; Index += 1) await writeFile(Mutationˉpaths[Index], Values[Index]);
                await writeFile(Analyzer, 'changed-during-analysis');
                return Buffer.from('source analysis status=Published\n');
            } }), /producer or cache implementation changed/);
        await writeFile(Analyzer, 'analyzer-v1'); Pass('producer-mutation-rejected');
        const Invalidˉsource = path.join(Root, 'invalid-source'); await mkdir(Invalidˉsource);
        const Invalidˉpaths = Values.map((_, Index) => path.join(Invalidˉsource, String(Index)));
        await Assert.rejects(Request({ Coordinator: Buffer.from('source-mismatch'), Outputs: Invalidˉpaths,
            Produce: async () => {
                for (let Index = 0; Index < 4; Index += 1) await writeFile(Invalidˉpaths[Index], Index === 0 ? Buffer.alloc(37, 9) : Values[Index]);
                return Buffer.from('source analysis status=Published\n');
            } }), /different admitted source set/);
        Pass('source-mismatch-not-cached');
        for (const Name of await readdir(Family)) await writeFile(path.join(Family, Name, 'Checkpoint.json'), '{');
        await Assert.rejects(Request(), /checkpoint record differs/); Pass('truncated-record-rejected');
        console.log(`authenticated analysis cache status=Passed cases=${Cases}`);
    } finally {
        if (Previousˉroot === undefined) delete process.env.WINDVALE_NATIVE_CACHE_ROOT;
        else process.env.WINDVALE_NATIVE_CACHE_ROOT = Previousˉroot;
        if (Previousˉmode === undefined) delete process.env.WINDVALE_PREPARED_PRODUCTS_ONLY;
        else process.env.WINDVALE_PREPARED_PRODUCTS_ONLY = Previousˉmode;
    }
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
    const Runˉagain = (Mode = '1') => spawnSync(process.execPath, Arguments, {
        cwd: REPOSITORY_ROOT, encoding: 'utf8',
        env: { ...Environment,
            ...(Mode === null ? {} : { WINDVALE_PREPARED_PRODUCTS_ONLY: Mode }) },
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
    const Savedˉcheckpoint = path.join(Family, `${Entries[0]}.saved`);
    await rename(path.join(Family, Entries[0]), Savedˉcheckpoint);
    const Missing = Runˉagain();
    Assert.equal(Missing.status, 64, Childˉdiagnostic(Missing));
    Assert.match(Missing.stderr, /Prepared split-project product missing/u);
    Assert.equal(await readFile(Record, 'ascii'), Completedˉphases);
    Assert.deepEqual(await readFile(Output), Buffer.from([0x57]));
    Assert.deepEqual(await readdir(Family), [`${Entries[0]}.saved`]);
    Assert.deepEqual(await Findˉtemporaryˉdirectories(Cacheˉroot), []);
    await rename(Savedˉcheckpoint, path.join(Family, Entries[0]));
    const Invalidˉmode = Runˉagain('0');
    Assert.equal(Invalidˉmode.status, 1, Childˉdiagnostic(Invalidˉmode));
    Assert.match(Invalidˉmode.stderr, /WINDVALE_PREPARED_PRODUCTS_ONLY must be absent or 1/u);
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
    const Changed = Runˉagain(null);
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

async function Verifyˉfunctionˉlimitˉdiagnostics(Testˉroot) {
    const Owner = path.join(SCRIPT_DIRECTORY, 'Test-Language-1.0-Memory-Budget-Split-Execution.mjs');
    function U32(Value) { const Bytes = Buffer.alloc(4); Bytes.writeUInt32LE(Value); return Bytes; }
    function Entry(Name, Locals, Code, Stack) {
        const Text = Buffer.from(Name);
        return Buffer.concat([U32(Text.length), Text, U32(0), Buffer.from([5]),
            U32(Locals), Buffer.alloc(Locals, 5), U32(0), U32(Code), U32(Stack)]);
    }
    const Cases = [
        ['valid', 2047, 100, 1, 0, /largest-name=LargeCode.*most-slots-name=LargeSlots most-slots=2047/u],
        ['locals', 2048, 100, 1, 1, /LargeSlots.*total-slots=2048/u],
        ['code', 1, 131073, 1, 1, /LargeCode.*code-bytes=131073/u],
        ['stack', 1, 100, 1025, 1, /LargeSlots.*maximum-stack=1025/u],
    ];
    for (const [Name, Locals, Code, Stack, Status, Diagnostic] of Cases) {
        // Directory-only diagnostic input: deliberately not an executable module.
        const Sections = [];
        for (let Kind = 1; Kind <= 7; Kind += 1) {
            const Body = Kind === 4 ? Buffer.concat([U32(2),
                Entry('LargeCode', 0, Code, 1), Entry('LargeSlots', Locals, 1, Stack)]) : Buffer.alloc(0);
            Sections.push(Buffer.from([Kind, 0, 0, 0]), U32(Body.length), Body);
        }
        const Input = path.join(Testˉroot, `Limit-${Name}.wvb`);
        await writeFile(Input, Buffer.concat([Buffer.alloc(12), ...Sections]), { flag: 'wx' });
        const Result = spawnSync(process.execPath, [Owner, '--inspect-function-limits', Input], {
            cwd: REPOSITORY_ROOT, encoding: 'utf8', windowsHide: true,
            timeout: 5_000, maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
        });
        Assert.equal(Result.error, undefined);
        Assert.equal(Result.status, Status, Name);
        Assert.match(Result.stdout + Result.stderr, Diagnostic);
        if (Status !== 0) Assert.doesNotMatch(Result.stdout, /status=Valid/u);
    }
    return Cases.length;
}

async function Verifyˉconstructionˉdeadlineˉadmission() {
    const Builder = path.join(SCRIPT_DIRECTORY, 'Build-Current-Split-Project-Wvb.mjs');
    // Expiry must precede reading these deliberately nonexistent target paths.
    const Pair = ['missing-deadline-project.wvproj', 'missing-deadline-output.wvb'];
    const Future = String(Date.now() + 120_000);
    const Cases = [
        [['--deadline-ms', '0', ...Pair], 1, /Invalid current split-project deadline/u],
        [['--deadline-ms', '9007199254740992', ...Pair], 1, /Invalid current split-project deadline/u],
        [['--deadline-ms', Future, '--deadline-ms', Future, ...Pair], 64, /Usage:/u],
        [[...Pair, '--deadline-ms'], 64, /Usage:/u],
        [['--deadline-ms', '1', ...Pair], 124, /deadline expired before construction/u],
        [['--deadline-ms', String(Date.now() + 1_000), ...Pair], 124, /deadline expired before construction/u],
        [['--prepare-only'], 64, /Usage:/u],
        [['--prepare-only', '--prepare-only', '--deadline-ms', Future], 64, /Usage:/u],
        [['--prepare-only', '--deadline-ms', Future, ...Pair], 64, /Usage:/u],
        [['--prepared-compiler-only'], 64, /Usage:/u],
        [['--prepared-compiler-only', '--prepared-compiler-only', ...Pair], 64, /Usage:/u],
        [['--prepare-only', '--deadline-ms', '1'], 124, /deadline expired before construction/u],
        [['--prepare-compiler', ...Pair], 64, /Usage:/u],
        [['--prepare-compiler', '--prepare-only', '--deadline-ms', Future], 64, /Usage:/u],
        [['--prepare-compiler', '--prepare-compiler', '--deadline-ms', Future, ...Pair], 64, /Usage:/u],
        [['--compiler-checkpoint', 'bad', ...Pair], 64, /Usage:/u],
        [['--compiler-checkpoint', '../' + 'a'.repeat(64), ...Pair], 64, /Usage:/u],
        [['--compiler-checkpoint', 'a'.repeat(64), '--compiler-checkpoint', 'b'.repeat(64), ...Pair], 64, /Usage:/u],
        [['--compiler-checkpoint', 'a'.repeat(64), '--prepare-only', '--deadline-ms', Future], 64, /Usage:/u],
        [['--compiler-checkpoint', 'a'.repeat(64), '--prepare-compiler', '--deadline-ms', Future, ...Pair], 64, /Usage:/u],
    ];
    for (const [Arguments, Status, Diagnostic] of Cases) {
        const Result = spawnSync(process.execPath, [Builder, ...Arguments], {
            cwd: REPOSITORY_ROOT, encoding: 'utf8', windowsHide: true,
            timeout: 3_000, maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
        });
        Assert.equal(Result.error, undefined);
        Assert.equal(Result.status, Status);
        Assert.equal(Result.stdout, '');
        Assert.match(Result.stderr, Diagnostic);
    }
    for (const Deadline of [-1, Infinity]) {
        await Assert.rejects(() => Constructˉsourceˉeditionˉpredecessor(undefined, Deadline),
            /Invalid source-edition predecessor deadline/u);
    }
    await Assert.rejects(() => Constructˉsourceˉeditionˉpredecessor(undefined, 1), Error =>
        Error.exitCode === 124 && /deadline expired before construction/u.test(Error.message));
    return Cases.length + 3;
}

async function Verifyˉconstructionˉprocessˉstatuses(Testˉroot, Actionsˉonly = false) {
    const Builder = pathToFileURL(path.join(SCRIPT_DIRECTORY, 'Build-Current-Split-Project-Wvb.mjs')).href;
    const Predecessor = pathToFileURL(path.join(SCRIPT_DIRECTORY, 'Source-Edition-Predecessor-Core.mjs')).href;
    const Prelude = `import { Runˉcurrentˉsplitˉprojectˉcli as Cli } from ${JSON.stringify(Builder)};\n` +
        `import { Runˉcompilerˉconstructionˉcommand as Run } from ${JSON.stringify(Predecessor)};\n`;
    const { Currentˉsplitˉcommandˉdeadline: Commandˉdeadline } = await import(Builder);
    const Started = 1_000_000;
    const Workˉdeadline = Started + 1_800_000;
    for (const Name of ['Package-Segmented-Compiler-Wvb', 'Build-Cached-Segmented-Hosted-Wvb.mjs']) {
        Assert.equal(Commandˉdeadline(Name, true, Workˉdeadline, Started), Started + 1_050_000);
        Assert.equal(Commandˉdeadline(Name, false, Workˉdeadline, Started), Started + 600_000);
        Assert.equal(Commandˉdeadline(Name, true, Started + 50_000, Started), Started + 50_000);
    }
    for (const Name of ['Package-Segmented-Compiler-Wvb.cmd', 'path/Package-Segmented-Compiler-Wvb',
        'Build-Cached-Segmented-Hosted-Wvb.mjs.old', 'Build-Cached-Split-Project-Wvb.mjs']) {
        Assert.equal(Commandˉdeadline(Name, true, Workˉdeadline, Started), Started + 600_000);
    }
    Assert.equal(Commandˉdeadline('Package-Segmented-Compiler-Wvb', true, Workˉdeadline, Started + 1_500_000), Workˉdeadline);
    Assert.equal(Commandˉdeadline('Package-Segmented-Compiler-Wvb', false, null, Started), Started + 600_000);
    const Child = Text => `Run('probe', process.execPath, ['-e', ${JSON.stringify(Text)}], Date.now() + 3000, false)`;
    const Primaryˉdetails = {
        code: 'E'.repeat(80), syscall: 'open\t\r\n' + 's'.repeat(80),
        path: 'selected\t\r\n' + 'p'.repeat(1_100),
        stack: 'Error: primary-marker\n' + Array.from({ length: 10 }, (_, Index) =>
            '    at primary-frame-' + Index + ' ' + 'x'.repeat(300)).join('\n'),
    };
    const Windowˉstack = 'Error: primary-timeout\n    at within-window-marker (input:1:1)\n' +
        'x'.repeat(16_384) + '\n    at beyond-window-marker (input:2:2)';
    const Cases = [
        [Child("process.stderr.write('child-timeout-marker'); process.exit(124);"), 124, /child-timeout-marker/u],
        [Child("process.stderr.write('child-framework-marker'); process.exit(2);"), 2, /child-framework-marker/u],
        [Child("process.stderr.write('unexpected-stderr');"), 2, /unexpected-stderr/u],
        ["Run('git-progress', process.execPath, ['-e', \"process.stderr.write('allowed-git-progress')\"], Date.now()+3000, false, undefined, true)", 0, /^$/u],
        [Child("process.exit(7);"), 7, /status=7/u],
        ["Run('probe', 'windvale-deliberately-missing-construction-tool', [], Date.now()+3000, false)", 2, /ENOENT/u, Diagnostic => {
            Assert.match(Diagnostic, /^Details: code=ENOENT syscall=spawn windvale-deliberately-missing-construction-tool path=windvale-deliberately-missing-construction-tool$/mu);
        }],
        ["Run('probe', process.execPath, ['-e', 'setInterval(()=>{},1000)'], Date.now()+100, false)", 124, /Development command timed out/u],
        ["Promise.reject(new AggregateError([new Error('ordinary-marker'), Object.assign(new Error('timeout-marker'), {exitCode:124})], 'joined-marker'))", 124, /joined-marker[\s\S]*ordinary-marker[\s\S]*timeout-marker/u],
        ["(()=>{const Failure=Object.assign(new Error('primary-marker'), " + JSON.stringify(Primaryˉdetails) +
            ", {exitCode:1, cleanupFailure:Object.assign(new Error('cleanup-marker'), {exitCode:124, code:37, syscall:'unlink', path:77, stack:''})});Failure.cause=Failure;return Promise.reject(Failure);})()",
            1, /primary-marker[\s\S]*Cleanup: cleanup-marker/u, Diagnostic => {
                Assert.ok(Diagnostic.includes('\nDetails: code=' + 'E'.repeat(64) +
                    ' syscall=open   ' + 's'.repeat(57) + ' path=selected   ' + 'p'.repeat(1_013) + '\n'));
                Assert.match(Diagnostic, /^Details: code=37 syscall=unlink path=77$/mu);
                Assert.equal(Diagnostic.split('\n').filter(Line => Line === 'primary-marker').length, 1);
                Assert.doesNotMatch(Diagnostic, /[\r\t]/u);
                const Frames = Diagnostic.split('\n').filter(Line => /^\s+at /u.test(Line));
                Assert.equal(Frames.length, 8);
                for (let Index = 0; Index < Frames.length; Index += 1) {
                    Assert.ok(Frames[Index].startsWith('    at primary-frame-' + Index + ' '));
                    Assert.equal(Frames[Index].length, 256);
                }
                Assert.doesNotMatch(Diagnostic, /primary-frame-[89]/u);
            }],
        ["Promise.reject(Object.assign(new Error('primary-timeout'), {exitCode:124, code:'ETIMEDOUT', syscall:'waitpid', path:'selected-timeout', stack:" +
            JSON.stringify(Windowˉstack) + ", cleanupFailure:Object.assign(new Error('cleanup-framework'), {exitCode:2, stack:''}), cause:Object.assign(new Error('cause-marker'), {code:'ENOENT',syscall:'stat',path:'/portable/missing',stack:''})}))",
            124, /primary-timeout[\s\S]*Cleanup: cleanup-framework/u, Diagnostic => {
                Assert.match(Diagnostic, /^Details: code=ETIMEDOUT syscall=waitpid path=selected-timeout$/mu);
                Assert.deepEqual(Diagnostic.split('\n').filter(Line => /^\s+at /u.test(Line)),
                    ['    at within-window-marker (input:1:1)']);
                Assert.doesNotMatch(Diagnostic, /beyond-window-marker/u);
                Assert.match(Diagnostic, /Cleanup: cleanup-framework[\s\S]*Cause: cause-marker\nDetails: code=ENOENT syscall=stat path=\/portable\/missing/u);
            }],
        ["Promise.reject(new AggregateError([Object.assign(new Error('timeout-marker'), {exitCode:124}), Object.assign(new Error('uncertain-marker'), {exitCode:2,cleanupUncertain:true})], 'uncertain-join'))", 2, /uncertain-join[\s\S]*timeout-marker[\s\S]*uncertain-marker/u],
        ["Promise.reject(Object.assign(new Error('primary-timeout'), {exitCode:124, cleanupFailure:Object.assign(new Error('cleanup-unproven'), {exitCode:2,cleanupUncertain:true})}))", 2, /primary-timeout[\s\S]*Cleanup: cleanup-unproven/u],
        ["Run('probe', 'ignored', [], Date.now()+3000, false, async()=>({Code:null,Output:'',Error:''}))", 2, /invalid construction process result/u],
    ];
    for (const [Action, Status, Diagnostic, Checkˉdetails] of Cases) {
        const Result = spawnSync(process.execPath, ['--input-type=module', '-e',
            Prelude + `await Cli(() => ${Action});`], {
            cwd: REPOSITORY_ROOT, encoding: 'utf8', windowsHide: true,
            timeout: 10_000, maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
        });
        Assert.equal(Result.error, undefined);
        Assert.equal(Result.status, Status);
        Assert.equal(Result.stdout, '');
        Assert.match(Result.stderr, Diagnostic);
        if (Checkˉdetails !== undefined) Checkˉdetails(Result.stderr);
    }
    if (Actionsˉonly) return Cases.length;
    const Owner = path.join(SCRIPT_DIRECTORY, 'Test-Language-1.0-Memory-Budget-Split-Execution.mjs');
    for (const [Seconds, Status, Diagnostic] of [
        ['1', 124, /deadline expired|timed out/u],
        ['0', 64, /The explicit development maximum must be 1 through 3600 seconds/u],
    ]) {
        const Result = spawnSync(process.execPath, [Owner, '--vector-borrow-integration', '--maximum-seconds', Seconds], {
            cwd: REPOSITORY_ROOT, encoding: 'utf8', windowsHide: true,
            timeout: 10_000, maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
        });
        Assert.equal(Result.error, undefined);
        Assert.equal(Result.status, Status);
        Assert.match(Result.stderr, Diagnostic);
        Assert.doesNotMatch(Result.stdout, /stage1-analyzer-build|source-edition predecessor|target-project-build/u);
    }
    const Framework = await Runˉcompilerˉconstructionˉcommand('framework', 'ignored', [], Date.now() + 1_000,
        false, async () => ({ Code: 2, Output: '', Error: 'framework marker' })).catch(Error => Error);
    Assert.equal(Framework.exitCode, 2);
    Assert.equal(Framework.cleanupUncertain, true);
    Assert.equal(Hasˉuncertainˉconstructionˉcleanup(new AggregateError([new Error('ordinary'),
        new AggregateError([Framework])])), true);
    const Cycle = new Error('cycle');
    Cycle.cause = Cycle;
    Assert.equal(Hasˉuncertainˉconstructionˉcleanup(Cycle), false);
    Assert.equal(Hasˉuncertainˉconstructionˉcleanup(new AggregateError(Array(9).fill(Cycle))), true);
    Assert.doesNotThrow(() => Requireˉwindowsˉterminationˉresult({ status: 0, error: undefined, signal: null }));
    const Windowsˉfailures = [null, { status: 1 }, { status: null, signal: 'SIGTERM' },
        { status: 0, error: new Error('bounded\nerror\t'.repeat(100)) }];
    for (const Result of Windowsˉfailures) {
        Assert.throws(() => Requireˉwindowsˉterminationˉresult(Result), Error =>
            Error.exitCode === 2 && Error.cleanupUncertain === true &&
            Error.message.length <= 320 && !/[\r\n\t]/u.test(Error.message));
    }
    const Detachedˉcases = process.platform === 'linux'
        ? await Verifyˉdetachedˉconstructionˉtimeout(Testˉroot) +
            await Verifyˉdetachedˉconstructionˉtimeout(Testˉroot, true) : 0;
    return Cases.length + 4 + Windowsˉfailures.length + Detachedˉcases;
}

async function Verifyˉdetachedˉconstructionˉtimeout(Testˉroot, Orphaned = false) {
    const Root = path.join(Testˉroot, Orphaned ? 'orphaned-construction-timeout' : 'detached-construction-timeout');
    await mkdir(Root);
    const Leaves = Orphaned ? ['Parent', 'Child'] : ['Parent', 'Child', 'Grandchild'];
    const Records = Leaves.map(Name => path.join(Root, Name + '.json'));
    const Scripts = Leaves.map(Name => path.join(Root, Name + '.mjs'));
    for (let Index = Leaves.length - 1; Index >= 0; Index -= 1) {
        const Program = `import {writeFileSync,readFileSync,renameSync} from 'node:fs';\n` +
            `import {spawn} from 'node:child_process';\n` +
            `const stat=readFileSync('/proc/self/stat','utf8');\n` +
            `const start=stat.slice(stat.lastIndexOf(') ')+2).split(' ')[19];\n` +
            `writeFileSync(${JSON.stringify(Records[Index] + '.tmp')},JSON.stringify({Pid:process.pid,Start:start}),{flag:'wx'});\n` +
            `renameSync(${JSON.stringify(Records[Index] + '.tmp')},${JSON.stringify(Records[Index])});\n` +
            (Index + 1 < Leaves.length ? `spawn(process.execPath,[${JSON.stringify(Scripts[Index + 1])}],` +
                `{detached:true,stdio:${Orphaned ? "['ignore','inherit','inherit']" : "'ignore'"}});\n` : '') +
            (Orphaned && Index === 0 ? 'process.exit(0);\n' : 'setInterval(()=>{},1000);\n');
        await writeFile(Scripts[Index], Program, { flag: 'wx' });
    }
    async function Current(Pid) {
        const Stat = await readFile('/proc/' + Pid + '/stat', 'utf8').catch(Error => {
            if (Error.code === 'ENOENT' || Error.code === 'ESRCH') return null;
            throw Error;
        });
        if (Stat === null) return null;
        const Fields = Stat.slice(Stat.lastIndexOf(') ') + 2).split(' ');
        return { State: Fields[0], Start: Fields[19] };
    }
    async function Readˉrecords() {
        const Found = [];
        for (const Record of Records) {
            const Bytes = await readFile(Record, 'utf8').catch(Error => {
                if (Error.code === 'ENOENT') return null;
                throw Error;
            });
            if (Bytes === null) continue;
            const Value = JSON.parse(Bytes);
            Assert.ok(Number.isSafeInteger(Value.Pid) && Value.Pid > 1);
            Assert.match(Value.Start, /^[0-9]+$/u);
            Found.push(Value);
        }
        return Found;
    }
    const Deadline = Date.now() + 3_000;
    const Command = Runˉdevelopmentˉcommand(process.execPath, [Scripts[0]], Deadline, false,
        MAXIMUM_DIAGNOSTIC_BYTES).then(Result => ({ Result }), Error => ({ Error }));
    try {
        let Identities = [];
        while (Date.now() < Deadline - 500) {
            Identities = await Readˉrecords();
            if (Identities.length === Leaves.length) break;
            await new Promise(Resolve => setTimeout(Resolve, 20));
        }
        Assert.equal(Identities.length, Leaves.length, 'The detached process chain did not announce readiness.');
        const Outcome = await Command;
        Assert.equal(Outcome.Error?.exitCode, Orphaned ? 2 : 124);
        if (Orphaned) Assert.equal(Outcome.Error.cleanupUncertain, true);
        for (const Identity of Orphaned ? Identities.slice(0, 1) : Identities) {
            const State = await Current(Identity.Pid);
            Assert.ok(State === null || State.Start !== Identity.Start || ['Z', 'X'].includes(State.State),
                'A detached construction descendant still executes after timeout.');
        }
    } finally {
        // Never signal a reused PID. These exact test-owned identities are the
        // only cleanup targets, even when the process-tree assertion fails.
        for (const Identity of (await Readˉrecords()).reverse()) {
            const State = await Current(Identity.Pid);
            if (State !== null && State.Start === Identity.Start && !['Z', 'X'].includes(State.State)) {
                try { process.kill(Identity.Pid, 'SIGKILL'); }
                catch (Error) { if (Error.code !== 'ESRCH') throw Error; }
            }
        }
        await Command;
    }
    return 1;
}

async function Verifyˉfoundationˉtestˉproducts(Testˉroot) {
    const Suffix = process.platform === 'win32' ? '.exe' : '.elf';
    const Key = '5'.repeat(64);
    const Root = path.join(Testˉroot, 'foundation-test-products');
    await mkdir(Root);
    let Cases = 0;
    const Never = async () => Assert.fail('Invalid acquisition invoked a callback.');
    for (const Request of [
        { Work: Root, Deadline: Date.now() - 1 },
        { Work: 'relative-work', Deadline: Date.now() + 30_000 },
        { Work: Root, Deadline: Number.MAX_SAFE_INTEGER + 1 },
    ]) {
        await Assert.rejects(() => Acquireˉfoundationˉborrowˉtestˉproducts({
            ...Request, Run: Never, Getˉkey: Never, Getˉfamily: Never, Acquire: Never,
            Snapshot: Never, Requireˉunchanged: Never,
        }), /deadline expired|Invalid Foundation test product/);
        Cases += 1;
    }
    for (const Mode of ['complete', 'explicit-construction', 'build-failure', 'missing-cache', 'key-change',
        'source-change', 'verifier-failure', 'runner-failure', 'both-failures', 'verifier-timeout', 'mixed-failures',
        'components-failure', 'product-change', 'compiler-product-change',
        'late-input-change', 'oversized-wvb', 'work-replacement']) {
        const Work = path.join(Root, Mode);
        const Family = path.join(Work, 'Cache');
        await mkdir(Family, { recursive: true });
        const Produce = async Place => {
            for (const [Name, Role] of [['Analyzer', 'analyzer'], ['Emitter', 'emitter']]) {
                const Bytes = Buffer.from(Name);
                await writeFile(path.join(Place, Name + Suffix), Bytes, { mode: 0o755 });
                await writeFile(path.join(Place, Name + '.identity'), Identity(
                    Role, Bytes.length, createHash('sha256').update(Bytes).digest('hex')));
            }
            for (const [Name] of CURRENT_ADMISSION_PROJECTS) {
                await writeFile(path.join(Place, Name + Suffix), Buffer.from(Name), { mode: 0o755 });
            }
        };
        if (Mode !== 'missing-cache') {
            await Acquireˉcurrentˉsplitˉcompiler(Family, Key, Produce, async () => {});
        }
        const Calls = [];
        let Keyˉreads = 0;
        let Checks = 0;
        let Acquisitions = 0;
        let Active = 0;
        let Maximumˉactive = 0;
        let Arrivals = 0;
        let Settled = false;
        let Announce;
        let Announceˉcomponent;
        let Release;
        const Ready = new Promise(Resolve => { Announce = Resolve; });
        const Componentˉready = new Promise(Resolve => { Announceˉcomponent = Resolve; });
        const Peer = new Promise(Resolve => { Release = Resolve; });
        const Packagingˉfailure = ['verifier-failure', 'runner-failure', 'both-failures', 'verifier-timeout', 'mixed-failures',
            'components-failure'].includes(Mode);
        let Barrierˉexpired = false;
        // Filesystem preparation shares the acquisition deadline; it must not
        // exhaust a shorter timer before the packaging workers can arrive.
        const Deadline = Date.now() + FAILURE_TIMEOUT_MILLISECONDS;
        const Timer = setTimeout(() => {
            Barrierˉexpired = true;
            Announce(); Announceˉcomponent(); Release();
        }, FAILURE_TIMEOUT_MILLISECONDS);
        let Outputˉdirectory;
        const Run = async (Label, Command, Arguments, Boundary) => {
            Assert.equal(Command, process.execPath);
            Assert.equal(Boundary, Deadline);
            Assert.equal(Calls.some(Call => Call.Label === Label), false);
            Calls.push({ Label, Arguments });
            Active += 1;
            Maximumˉactive = Math.max(Maximumˉactive, Active);
            try {
                if (Label === 'foundation-products-build') {
                    Assert.equal(Mode, 'explicit-construction');
                    Assert.equal(path.basename(Arguments[0]), 'Build-Current-Split-Project-Wvb.mjs');
                    Assert.deepEqual(Arguments.slice(1, 4), ['--prepare-compiler', '--deadline-ms', String(Deadline)]);
                    Assert.equal(Arguments.length, 10);
                    for (let Index = 4; Index < Arguments.length; Index += 2) {
                        Assert.equal(path.extname(Arguments[Index]), '.wvproj');
                        Outputˉdirectory = path.dirname(Arguments[Index + 1]);
                        Assert.equal(path.dirname(Outputˉdirectory), Work);
                        await writeFile(Arguments[Index + 1], Buffer.from('bounded WVB'), { flag: 'wx' });
                    }
                    return;
                }
                if (Label.startsWith('foundation-products-build-')) {
                    Assert.equal(path.basename(Arguments[0]), 'Build-Cached-Split-Project-Wvb.mjs');
                    Assert.equal(Arguments.length, 12);
                    Assert.equal(Arguments[7], '--authenticated-project4');
                    Assert.deepEqual(Arguments.slice(3, 7), [
                        path.join(Family, Key, 'Analyzer' + Suffix), path.join(Family, Key, 'Analyzer.identity'),
                        path.join(Family, Key, 'Emitter' + Suffix), path.join(Family, Key, 'Emitter.identity'),
                    ]);
                    Assert.deepEqual(Arguments.slice(8), ['Admitter', 'Authenticator', 'Reader', 'Binder']
                        .map(Name => path.join(Family, Key, Name + Suffix)));
                    if (Mode === 'build-failure') throw new Error(Mode);
                    Assert.equal(path.extname(Arguments[1]), '.wvproj');
                    Outputˉdirectory = path.dirname(Arguments[2]);
                    Assert.equal(path.dirname(Outputˉdirectory), Work);
                    await writeFile(Arguments[2], Buffer.from('bounded WVB'), { flag: 'wx' });
                    if (Mode === 'oversized-wvb') {
                        const Handle = await open(Arguments[2], 'r+');
                        try { await Handle.truncate(16_777_217); }
                        finally { await Handle.close(); }
                    }
                    if (Mode === 'work-replacement') {
                        await rename(Outputˉdirectory, Outputˉdirectory + '-original');
                        await mkdir(Outputˉdirectory);
                        for (const Name of ['Verifier', 'Runner', 'Components']) {
                            await writeFile(path.join(Outputˉdirectory, Name + '.wvb'), Buffer.from('replacement'));
                        }
                    }
                    return;
                }
                Assert.equal(path.basename(Arguments[0]), 'Build-Cached-Segmented-Hosted-Wvb.mjs');
                Assert.equal(Arguments.length, 6);
                Assert.deepEqual(Arguments.slice(1, 3), ['--deadline-ms', String(Deadline)]);
                const Name = path.basename(Arguments[4], '.wvb');
                Assert.equal(Arguments[3], { Verifier: '7', Runner: '5', Components: '1' }[Name]);
                Assert.equal(path.basename(Arguments[5]), Name + Suffix);
                if (Name !== 'Components') {
                    Arrivals += 1;
                    if (Arrivals === 2) Announce();
                } else { Announceˉcomponent(); }
                if (Packagingˉfailure) {
                    await Ready;
                    if (Mode === 'verifier-timeout' && Name === 'Verifier') {
                        throw Object.assign(new Error('Verifier-timeout'), { exitCode: 124 });
                    }
                    if (Mode === 'mixed-failures') {
                        throw Object.assign(new Error(Name + '-mixed-failure'), Name === 'Verifier'
                            ? { exitCode: 2, cleanupUncertain: true } : { exitCode: 124 });
                    }
                    if (Mode === Name.toLowerCase() + '-failure' || Mode === 'both-failures') {
                        throw new Error(Name + '-failure');
                    }
                    if (Name !== 'Verifier' || Mode !== 'components-failure') await Peer;
                } else {
                    await new Promise(Resolve => setImmediate(Resolve));
                }
                await writeFile(Arguments[5], Buffer.from(Name), { flag: 'wx', mode: 0o755 });
            } finally { Active -= 1; }
        };
        const Acquisition = Acquireˉfoundationˉborrowˉtestˉproducts({
            Work, Deadline, Run, Prepareˉcompiler: Mode === 'explicit-construction',
            Getˉkey: async () => {
                Keyˉreads += 1;
                return Mode === 'key-change' && Keyˉreads > 1 ? '6'.repeat(64) : Key;
            },
            Getˉfamily: async () => Family,
            Acquire: async (...Arguments) => {
                Acquisitions += 1;
                return Acquireˉcurrentˉsplitˉcompiler(...Arguments);
            },
            Snapshot: async Projects => {
                Assert.deepEqual(Projects.map(Project => path.basename(Project)), [
                    'Windvale-Compiler-Wvb-Verifier.wvproj', 'Windvale-Wvb-Runner.wvproj',
                    'Windvale-Native-Test-Foundation-Borrow-Components.wvproj',
                ]);
                return 'snapshot';
            },
            Requireˉunchanged: async Snapshot => {
                Assert.equal(Snapshot, 'snapshot');
                Checks += 1;
                if (Mode === 'source-change' || (Mode === 'late-input-change' && Checks === 2)) {
                    throw new Error('source changed');
                }
                if (Mode === 'product-change' && Checks === 2) {
                    await writeFile(path.join(Outputˉdirectory, 'Verifier' + Suffix), Buffer.from('changed'));
                }
                if (Mode === 'compiler-product-change' && Checks === 2) {
                    await writeFile(path.join(Family, Key, 'Admitter' + Suffix), Buffer.from('changed'));
                }
            },
        }).then(Value => { Settled = true; return { Value }; }, Error => { Settled = true; return { Error }; });
        try {
            if (Packagingˉfailure) {
                await Promise.race([Ready, Acquisition.then(Result => {
                    if (Result.Error) throw Result.Error;
                })]);
                Assert.equal(Barrierˉexpired, false, 'Foundation packaging arrival deadline expired.');
                Assert.equal(Arrivals, 2, 'Foundation packaging branches were serialized.');
                if (Mode === 'components-failure') {
                    await Componentˉready;
                    Assert.equal(Barrierˉexpired, false, 'Foundation component arrival deadline expired.');
                    Assert.equal(Calls.length, 6, 'The third package did not reuse an available leaf.');
                }
                // Allow the failing branch to report without releasing its live peer.
                for (let Iteration = 0; Iteration < 8; Iteration += 1) {
                    await new Promise(Resolve => setImmediate(Resolve));
                }
                if (!['both-failures', 'mixed-failures'].includes(Mode)) {
                    Assert.equal(Settled, false, 'Foundation acquisition abandoned a live peer.');
                }
                Release();
            }
            const Result = await Acquisition;
            Assert.equal(Active, 0);
            Assert.ok(Maximumˉactive <= 2);
            if (Mode === 'complete' || Mode === 'explicit-construction') {
                Assert.equal(Result.Error, undefined);
                Assert.equal(Maximumˉactive, 2);
                Assert.equal(Calls.length, Mode === 'complete' ? 6 : 4);
                Assert.equal(Acquisitions, 2);
                Assert.equal(Checks, 2);
                Assert.equal(Result.Value.Compilerˉkey, Key);
                Assert.equal(Result.Value.Evidence.length, 14);
                Assert.equal(Object.isFrozen(Result.Value), true);
                for (const Name of ['Analyzer', 'Emitter', 'Admitter', 'Authenticator', 'Reader', 'Binder']) {
                    Assert.equal(Result.Value[Name], path.join(Family, Key, Name + Suffix));
                }
                for (const Name of ['Verifier', 'Runner', 'Components']) {
                    Assert.equal(Result.Value[Name], path.join(Outputˉdirectory, Name + Suffix));
                }
            } else {
                Assert.ok(Result.Error instanceof Error, Mode + ' did not reject.');
                if (Packagingˉfailure) {
                    Assert.ok(Result.Error instanceof AggregateError);
                    Assert.equal(Result.Error.errors.length, ['both-failures', 'mixed-failures'].includes(Mode) ? 2 : 1);
                    if (Mode === 'verifier-timeout') Assert.equal(Result.Error.exitCode, 124);
                    if (Mode === 'mixed-failures') {
                        Assert.equal(Result.Error.exitCode, 2);
                        Assert.equal(Result.Error.cleanupUncertain, true);
                    }
                    Assert.equal(Calls.length, Mode === 'components-failure' ? 6 : 5,
                        'Foundation acquisition started queued commands after a branch failed.');
                } else if (['product-change', 'compiler-product-change', 'late-input-change'].includes(Mode)) {
                    Assert.equal(Calls.length, 6);
                    Assert.match(Result.Error.message, /product changed|checkpoint record differs|source changed/);
                } else {
                    Assert.equal(Calls.length, ['missing-cache', 'key-change'].includes(Mode) ? 0 :
                        Mode === 'source-change' ? 3 : 1, Mode + ': ' + Result.Error.message);
                    const Diagnostic = Result.Error instanceof AggregateError
                        ? Result.Error.errors.map(Error => Error.message).join('\n') : Result.Error.message;
                    Assert.match(Diagnostic, /build-failure|checkpoint missing|inputs changed|source changed|bounded ordinary file|directory changed/);
                }
            }
            Assert.equal((await readdir(Family)).some(Name => Name.startsWith('.new-')), false);
            Cases += 1;
        } finally {
            clearTimeout(Timer);
            Release();
            await Acquisition;
        }
    }
    return Cases;
}

async function Writeˉcompilerˉcheckpointˉfixture(Place) {
    const Suffix = process.platform === 'win32' ? 'exe' : 'elf';
    for (const [Name, Role] of [['Analyzer', 'analyzer'], ['Emitter', 'emitter']]) {
        const Bytes = Buffer.from(`bounded ${Role} fixture`);
        const Digest = createHash('sha256').update(Bytes).digest('hex');
        await writeFile(path.join(Place, `${Name}.${Suffix}`), Bytes, { mode: 0o755 });
        await writeFile(path.join(Place, `${Name}.identity`), Identity(Role, Bytes.length, Digest));
    }
    for (const [Name] of CURRENT_ADMISSION_PROJECTS) {
        await writeFile(path.join(Place, `${Name}.${Suffix}`), Buffer.from(Name), { mode: 0o755 });
    }
}

async function Verifyˉcompilerˉpreparationˉcli(Testˉroot) {
    const Root = path.join(Testˉroot, 'preparation-cli');
    const Family = path.join(Root, 'current-split-compiler-v2', HOST);
    await mkdir(Family, { recursive: true });
    const Builder = path.join(SCRIPT_DIRECTORY, 'Build-Current-Split-Project-Wvb.mjs');
    const Key = await Getˉcurrentˉsplitˉcompilerˉkey();
    const Wrongˉkey = Key === '0'.repeat(64) ? '1'.repeat(64) : '0'.repeat(64);
    await Acquireˉcurrentˉsplitˉcompiler(Family, Wrongˉkey,
        Writeˉcompilerˉcheckpointˉfixture, async () => {});
    let Cases = 0;
    const Probe = (Mode, Status, Diagnostic, Arguments = [], Prepare = true) => {
        const Environment = { ...process.env, WINDVALE_NATIVE_CACHE_ROOT: Root };
        delete Environment.WINDVALE_PREPARED_COMPILER_ONLY;
        delete Environment.WINDVALE_PREPARED_PRODUCTS_ONLY;
        if (Mode.startsWith('products-')) Environment.WINDVALE_PREPARED_PRODUCTS_ONLY = Mode.slice(9);
        else if (Mode !== 'flag' && Mode !== 'normal') Environment.WINDVALE_PREPARED_COMPILER_ONLY = Mode;
        const Result = spawnSync(process.execPath, [Builder,
            ...(Prepare ? ['--prepare-only'] : []), '--deadline-ms', String(Date.now() + 60_000),
            ...(Mode === 'flag' ? ['--prepared-compiler-only'] : []), ...Arguments], {
            cwd: REPOSITORY_ROOT, env: Environment, encoding: 'utf8', windowsHide: true,
            timeout: 15_000, maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
        });
        Assert.equal(Result.error, undefined);
        Assert.equal(Result.status, Status, Childˉdiagnostic(Result));
        Assert.match(Result.stdout + Result.stderr, Diagnostic);
        Assert.doesNotMatch(Result.stdout, /START current split project/u);
        if (Status !== 0) Assert.doesNotMatch(Result.stdout, /status=Complete/u);
        else Assert.equal(Result.stderr, '');
        Cases += 1;
    };
    // No native product may execute: fixtures only exercise cache identity and
    // CLI phase selection. A different input key is not a prepared checkpoint.
    Probe('flag', 64, /Current compiler checkpoint missing/u);
    Probe('1', 64, /Current compiler checkpoint missing/u);
    Probe('products-1', 64, /Current compiler checkpoint missing/u);
    Probe('products-0', 1, /WINDVALE_PREPARED_PRODUCTS_ONLY must be absent or 1/u);
    const Output = path.join(Testˉroot, 'Prepared-only.wvb');
    const Sentinel = Buffer.from('preserve the previous output');
    await writeFile(Output, Sentinel);
    Probe('flag', 64, /Current compiler checkpoint missing/u, [PROJECT, Output], false);
    Probe('1', 64, /Current compiler checkpoint missing/u, [PROJECT, Output], false);
    Probe('normal', 64, /Project builds do not reconstruct compiler tools/u, [PROJECT, Output], false);
    Probe('normal', 64, /checkpoint missing/u, ['--compiler-checkpoint', 'f'.repeat(64), PROJECT, Output], false);
    Probe('1', 64, /Usage:/u, ['--prepare-compiler', PROJECT, Output], false);
    const Selected = await Readˉpreparedˉsplitˉcompiler(Family, Wrongˉkey);
    Assert.equal(Selected.status, 'Hit');
    await Selected.Requireˉunchanged();
    Cases += 1;
    const Explicit = spawnSync(process.execPath, [Builder, '--compiler-checkpoint', Wrongˉkey, PROJECT, Output], {
        cwd: REPOSITORY_ROOT, encoding: 'utf8', windowsHide: true, timeout: 15_000,
        env: { ...process.env, WINDVALE_NATIVE_CACHE_ROOT: Root, WINDVALE_PREPARED_PRODUCTS_ONLY: '1' },
        maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
    });
    Assert.equal(Explicit.error, undefined);
    Assert.equal(Explicit.status, 64, Childˉdiagnostic(Explicit));
    Assert.match(Explicit.stdout, /selection=explicit-checkpoint construction=disabled/u);
    Assert.match(Explicit.stderr, /Prepared split-project product missing/u);
    Assert.doesNotMatch(Explicit.stdout, /stage1-|predecessor|pinned-analyzer-package/u);
    Cases += 1;
    // Both public and development entry points reject missing compiler tools
    // before any native producer starts, and preserve the previous destination.
    const Modern = path.join(Projectˉroot, 'Missing-Compiler.wvproj');
    await writeFile(Modern, 'windvale-project 4\nroot "missing.wv"\nemit wvb\n');
    const Public = path.join(SCRIPT_DIRECTORY, 'Build-Wvb-Project4.mjs');
    for (const Options of [[], ['--compiler-checkpoint', 'f'.repeat(64)]]) {
        const Missing = spawnSync(process.execPath, [Public, ...Options, Modern, Output], {
            cwd: REPOSITORY_ROOT, encoding: 'utf8', windowsHide: true, timeout: 15_000,
            env: { ...process.env, WINDVALE_NATIVE_CACHE_ROOT: Root }, maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
        });
        Assert.equal(Missing.error, undefined);
        Assert.equal(Missing.status, 64, Childˉdiagnostic(Missing));
        Assert.match(Missing.stderr, /Project builds do not reconstruct compiler tools/u);
        Assert.equal(Missing.stdout, '');
        Cases += 1;
    }
    Assert.deepEqual(await readFile(Output), Sentinel);
    Assert.deepEqual((await readdir(Family)).sort(), [Wrongˉkey]);
    Probe('0', 1, /must be absent or 1/u);
    const Ready = await Acquireˉcurrentˉsplitˉcompiler(Family, Key,
        Writeˉcompilerˉcheckpointˉfixture, async () => {});
    Probe('flag', 0, /preparation status=Complete steps=0 key=[a-f0-9]{64}/u);
    Probe('1', 0, /cache status=Hit/u);
    Probe('products-1', 0, /cache status=Hit/u);
    Probe('normal', 0, /cache status=Hit/u);
    // A later invocation failure must leave the completed checkpoint reusable.
    Probe('flag', 64, /Usage:/u, ['unexpected.wvproj', 'unexpected.wvb']);
    Probe('flag', 0, /cache status=Hit/u);
    const Product = path.join(Ready.directory, `Emitter.${process.platform === 'win32' ? 'exe' : 'elf'}`);
    await writeFile(Product, Buffer.from('corrupt emitter'));
    Probe('flag', 1, /emitter identity differs/u);
    await Assert.rejects(Readˉpreparedˉsplitˉcompiler(Family, Key), /emitter identity differs/u);
    await writeFile(path.join(Selected.directory, 'Reader.' + (process.platform === 'win32' ? 'exe' : 'elf')), Buffer.from('changed'));
    await Assert.rejects(Selected.Requireˉunchanged(), /checkpoint record differs/u);
    Cases += 2;
    return Cases;
}

async function Verifyˉpreparedˉpublisher(Testˉroot) {
    const Previousˉroot = process.env.WINDVALE_NATIVE_CACHE_ROOT;
    const Previousˉmode = process.env.WINDVALE_PREPARED_PRODUCTS_ONLY;
    const Root = path.join(Testˉroot, 'prepared-publisher');
    await mkdir(Root);
    const Cache = path.join(Root, 'cache');
    const Host = process.platform === 'win32' ? 'windows-x64' : 'linux-x64';
    const Extension = process.platform === 'win32' ? 'exe' : 'elf';
    const Family = path.join(Cache, 'current-transactional-wvb-publisher-v1', Host);
    const Wvb = path.join(Root, 'Publisher.wvb');
    const Payload = Buffer.from('publisher input fixture\n');
    const Product = Buffer.from('prepared publisher product fixture\n');
    await writeFile(Wvb, Payload);
    let Constructions = 0;
    const Construct = async () => {
        Constructions += 1;
        throw new Error('publisher-staging-reached');
    };
    const Acquire = (Name, Compilerˉkey = 'a'.repeat(64)) => Acquireˉcurrentˉwvbˉpublisher(
        Wvb, path.join(Root, `${Name}.${Extension}`), Compilerˉkey, Construct, Construct,
    );
    try {
        process.env.WINDVALE_NATIVE_CACHE_ROOT = Cache;
        process.env.WINDVALE_PREPARED_PRODUCTS_ONLY = 'invalid';
        await Assert.rejects(Acquire('Invalid'), /must be absent or 1/u);
        process.env.WINDVALE_PREPARED_PRODUCTS_ONLY = '1';
        let Key;
        await Assert.rejects(Acquire('Missing'), Error => {
            Assert.equal(Error.exitCode, 64);
            const Match = /Prepared segmented hosted product missing key=([0-9a-f]{64})/u.exec(Error.message);
            Assert.ok(Match);
            Key = Match[1];
            return true;
        });
        Assert.equal(Constructions, 0);
        Assert.deepEqual(await readdir(Family), []);
        Assert.deepEqual((await readdir(Root)).sort(), ['Publisher.wvb', 'cache']);
        const Destination = path.join(Family, Key);
        const Input = { path: Wvb, payload: Payload, bytes: Payload.length,
            sha256: createHash('sha256').update(Payload).digest('hex') };
        // Exercise cache control with an opaque product; this fixture is never executed.
        await Createˉsegmentedˉhostedˉcheckpoint(Family, Destination, Key, '2', Input,
            Place => writeFile(path.join(Place, `Product.${Extension}`), Product));
        const Hit = await Acquire('Hit');
        Assert.equal(Hit.Status, 'Hit');
        Assert.deepEqual(await readFile(Hit.Path), Product);
        Assert.equal(Constructions, 0);
        await writeFile(path.join(Destination, `Product.${Extension}`), 'corrupt');
        await Assert.rejects(Acquire('Corrupt'), /manifest differs/u);
        Assert.equal(Constructions, 0);
        delete process.env.WINDVALE_PREPARED_PRODUCTS_ONLY;
        await Assert.rejects(Acquire('Construction', 'b'.repeat(64)), /publisher-staging-reached/u);
        Assert.equal(Constructions, 1);
        Assert.deepEqual(await readdir(Family), [Key]);
        Assert.deepEqual(await readFile(Hit.Path), Product);
        console.log('prepared publisher cache cases=5 status=Passed construction-on-miss=Disabled');
        return 5;
    } finally {
        if (Previousˉroot === undefined) delete process.env.WINDVALE_NATIVE_CACHE_ROOT;
        else process.env.WINDVALE_NATIVE_CACHE_ROOT = Previousˉroot;
        if (Previousˉmode === undefined) delete process.env.WINDVALE_PREPARED_PRODUCTS_ONLY;
        else process.env.WINDVALE_PREPARED_PRODUCTS_ONLY = Previousˉmode;
    }
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
        await Writeˉcompilerˉcheckpointˉfixture(Place);
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
    for (const [Name] of CURRENT_ADMISSION_PROJECTS) {
        const Admission = path.join(Cold.directory, `${Name}.${Suffix}`);
        const Originalˉadmission = await readFile(Admission);
        await writeFile(Admission, Buffer.from('corrupt admission producer'));
        await Assert.rejects(() => Acquireˉcurrentˉsplitˉcompiler(
            Family, Key, Neverˉproduce, Unchanged), /checkpoint record differs/);
        await writeFile(Admission, Originalˉadmission);
    }

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
    for (const Edition of [2, 4]) {
    for (const Mode of ['complete', 'analyzer-failure', 'emitter-failure', 'both-failures', 'preparation-failure', 'admission-failure']) {
        const Work = path.join(Testˉroot, `${Edition}-${Mode}`);
        const Candidate = path.join(Work, 'Pair');
        await mkdir(Candidate, { recursive: true });
        const Projects = path.join(Work, 'Projects');
        await mkdir(Projects);
        for (const Name of ['Windvale-Compiler-Analysis-Driver.wvproj',
            'Windvale-Compiler-Emission-Driver.wvproj', ...CURRENT_ADMISSION_PROJECTS.map(([, Name]) => Name)]) {
            await writeFile(path.join(Projects, Name), Edition === 4 && Name === 'Windvale-Compiler-Analysis-Driver.wvproj'
                ? await readFile(path.join(REPOSITORY_ROOT, 'Projects', 'Tools', Name))
                : `windvale-project ${Edition}\n`);
        }
        const Predecessor = {};
        for (const Name of ['Analyzer', 'Emitter', 'Admitter', 'Authenticator', 'Reader', 'Binder']) {
            Predecessor[Name] = path.join(Work, 'Predecessor-' + Name + Suffix);
            await writeFile(Predecessor[Name], Buffer.from(Name), { mode: 0o755 });
        }
        for (const Name of ['Analyzer', 'Emitter']) {
            Predecessor[Name + 'ˉidentity'] = path.join(Work, 'Predecessor-' + Name + '.identity');
            const Product = await readFile(Predecessor[Name]);
            await writeFile(Predecessor[Name + 'ˉidentity'], Buffer.from(
                `windvale-split-compiler-producer 2\nrole ${Name.toLowerCase()}\n` +
                `target ${Name === 'Analyzer' ? 'source-analysis-v1' : 'portable-wvb-optimized-v1'}\n` +
                `host ${process.platform}-${process.arch}\nbytes ${Product.length}\n` +
                `sha256 ${createHash('sha256').update(Product).digest('hex')}\n`));
        }
        Predecessor.Construction = { kind: 'source-edition-predecessor-1',
            revision: '992ba7bbebbbf3356966bdcc0ea1d2b0e5923720',
            tree: 'c6231310843c24a03b0253f6f5a062d77db9f73f', key: '1'.repeat(64), host: HOST };
        const Family = path.join(Work, 'Cache', 'current-split-compiler-v2', HOST);
        await mkdir(Family, { recursive: true });
        const Constructionˉrequest = { Mode: 'prepare', Key: '2'.repeat(64), Family,
            Deadline: Date.now() + 120_000, Requireˉunchanged: async () => {} };
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
        let Barrierˉexpired = false;
        const Timer = setTimeout(() => {
            Barrierˉexpired = true;
            Announce(); Release();
        }, FAILURE_TIMEOUT_MILLISECONDS);
        const Run = async (Label, Name, Arguments) => {
            Assert.equal(Calls.includes(Label), false);
            Calls.push(Label);
            Active += 1;
            Maximumˉactive = Math.max(Active, Maximumˉactive);
            try {
                if (Label === 'stage1-analyzer-build' && Mode === 'preparation-failure') {
                    throw new Error('preparation-failure');
                }
                if (Label === 'current-Admitter-build' && Mode === 'admission-failure') {
                    throw new Error('admission-failure');
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
                if (Name === 'Run-Split-Compiler.mjs') {
                    Assert.equal(Label, 'direct-condition-analyzer-source');
                    Assert.equal(Arguments.length, 13);
                    Assert.deepEqual(Arguments.slice(0, 4), [Predecessor.Admitter, Predecessor.Authenticator,
                        Predecessor.Analyzer, Predecessor.Emitter]);
                    Assert.deepEqual(Arguments.slice(4, 6), ['--foreign-binder', Predecessor.Binder]);
                    Assert.equal(Arguments[6], '--workspace');
                    Assert.equal(Arguments[8], '--project');
                    Assert.deepEqual(Arguments.slice(10, 12), ['--manifest-reader', Predecessor.Reader]);
                    const Snapshotˉmanifest = await readFile(Arguments[9], 'utf8');
                    Assert.equal([...Snapshotˉmanifest.matchAll(/^(?:root|source) /gmu)].length, 23);
                    Assert.ok(!Snapshotˉmanifest.includes('Source-Wir-Scalar-Constants.wv'));
                    const Wvb = Buffer.alloc(8);
                    Wvb.write('WVB1'); Wvb.writeUInt16LE(1, 4); Wvb.writeUInt16LE(11, 6);
                    await writeFile(Arguments[12], Wvb);
                } else if (Name === 'Package-Segmented-Compiler-Wvb') {
                    Assert.equal(Arguments.length, 4);
                    Assert.equal(Arguments[3], '--development-cache');
                    Assert.equal(Arguments[0], Label === 'pinned-analyzer-package' ? '7' : '8');
                    await readFile(Arguments[1]);
                    await writeFile(Arguments[2], Buffer.from(Label), { mode: 0o755 });
                } else if (Name === 'Write-Split-Compiler-Producer-Identity.mjs') {
                    Assert.equal(Arguments.length, 3);
                    await readFile(Arguments[1]);
                    Assert.equal(Arguments[0], Label.includes('emitter') ? 'emitter' : 'analyzer');
                    if (Label === 'direct-condition-analyzer-identity') {
                        const Product = await readFile(Arguments[1]);
                        await writeFile(Arguments[2], Buffer.from(`windvale-split-compiler-producer 2\nrole analyzer\n` +
                            `target source-analysis-v1\nhost ${HOST}\nbytes ${Product.length}\n` +
                            `sha256 ${createHash('sha256').update(Product).digest('hex')}\n`));
                    } else await writeFile(Arguments[2], Buffer.from(Label));
                } else if (Name === 'Build-Cached-Segmented-Hosted-Wvb.mjs') {
                    Assert.equal(Arguments.length, 3);
                    Assert.equal(Arguments[0], '7');
                    await readFile(Arguments[1]);
                    await writeFile(Arguments[2], Buffer.from(Label), { mode: 0o755 });
                } else {
                    Assert.equal(Name, 'Build-Cached-Split-Project-Wvb.mjs');
                    Assert.equal(Arguments.length, Edition === 4 ? 11 : Label === 'stage1-analyzer-build' ? 6 : 7);
                    if (Edition === 4) {
                        Assert.deepEqual(Arguments.slice(6), ['--authenticated-project4',
                            Predecessor.Admitter, Predecessor.Authenticator, Predecessor.Reader, Predecessor.Binder]);
                    }
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
        const Construction = Constructˉcurrentˉsplitˉcompiler(Work, Candidate, Run, Run,
            async (_, Deadline) => { Assert.equal(Edition, 4);
                Assert.equal(Deadline, Constructionˉrequest.Deadline); return Predecessor; }, Projects,
            Edition === 4 ? Constructionˉrequest : null)
            .then(() => { Settled = true; return null; }, Error => { Settled = true; return Error; });
        try {
            if (Mode !== 'preparation-failure') {
                await Promise.race([Ready, Construction.then(Failure => {
                    if (Failure) throw Failure;
                })]);
                Assert.equal(Barrierˉexpired, false, 'Compiler branch arrival deadline expired.');
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
                Assert.equal(Calls.length, Edition === 4 ? 19 : 20);
                if (Edition === 4) Assert.deepEqual(Calls.slice(0, 3), ['direct-condition-analyzer-source',
                    'direct-condition-analyzer-package', 'direct-condition-analyzer-identity']);
                const Products = ['Analyzer' + Suffix, 'Analyzer.identity', 'Emitter' + Suffix, 'Emitter.identity',
                    ...CURRENT_ADMISSION_PROJECTS.map(([Name]) => Name + Suffix)];
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
                    Assert.equal(Calls.length, Edition === 4 ? 4 : 5);
                } else if (Mode === 'admission-failure') {
                    Assert.equal(Failure.message, Mode);
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
}

async function Verifyˉartifactˉreaderˉprojections() {
    // No generated file is written and no compiler or image executes.
    const Retired = 'Compiler/Windvale/Source-Bindings-Generic-Types-Artifact-Core.wv';
    await Assert.rejects(() => lstat(path.join(REPOSITORY_ROOT, Retired)), Error => Error.code === 'ENOENT');
    const Project = await readFile(path.join(REPOSITORY_ROOT,
        'Projects/Tools/Windvale-Compiler-Emission-Driver.wvproj'));
    Assert.ok(Project.length > 0 && Project.length <= 65_536);
    Assert.ok(!Project.toString('utf8').includes('"' + Retired + '"'));
    console.log('artifact reader control item=1/2 case=named-retired-reader-absent status=Passed');
    const Result = await Runˉdevelopmentˉcommand(process.execPath,
        [path.join(SCRIPT_DIRECTORY, 'Generate-Compiler-Artifact-Readers.mjs'), '--check'],
        Date.now() + 30_000, false, MAXIMUM_DIAGNOSTIC_BYTES);
    Assert.equal(Result.Code, 0, Result.Error);
    Assert.equal(Result.Error, '');
    Assert.equal(Result.Output.replaceAll('\r\n', '\n'), 'artifact-reader status=Passed files=17\n');
    console.log('artifact reader control item=2/2 case=canonical-projections-check status=Passed');
    return 2;
}

async function Verifyˉcompilerˉconstructionˉcontrols(Testˉroot) {
    const Names = ['WINDVALE_PREPARED_PRODUCTS_ONLY', 'WINDVALE_PREPARED_COMPILER_ONLY'];
    const Previous = Names.map(Name => process.env[Name]);
    try {
        // These callbacks write control data only; no compiler or image executes.
        for (const Name of Names) delete process.env[Name];
        await Verifyˉcompilerˉconstructionˉbranches(Testˉroot);
        return await Verifyˉdirectˉconditionˉintermediate(Testˉroot);
    } finally {
        for (const [Index, Name] of Names.entries()) {
            if (Previous[Index] === undefined) delete process.env[Name];
            else process.env[Name] = Previous[Index];
        }
    }
}

async function Verifyˉdirectˉconditionˉintermediate(Testˉroot) {
    const Deadline = Date.now() + 120_000;
    let Cases = 0;
    function Time() { Assert.ok(Date.now() < Deadline, 'Direct intermediate control deadline expired.'); }
    function Pass(Name) {
        Time(); Cases += 1;
        console.log('direct intermediate cache item=' + Cases + ' case=' + Name + ' status=Passed');
    }
    const Core = await readFile(path.join(REPOSITORY_ROOT, 'Compiler/Windvale/Source-Wir-Core.wv'));
    const Manifest = await readFile(path.join(REPOSITORY_ROOT,
        'Projects/Tools/Windvale-Compiler-Analysis-Driver.wvproj'));
    const Originalˉcore = Buffer.from(Core);
    const Originalˉmanifest = Buffer.from(Manifest);
    const Projected = Projectˉdirectˉconditionˉanalyzer(Core, Manifest);
    Assert.equal(Projected.Inverseˉexact, true);
    Assert.ok(!Projected.Core.includes(Buffer.from('Scalarˉconstants')));
    Assert.ok(Projected.Core.includes(Buffer.from('export fn Compilerˉsourceˉwirˉappendˉfunctionˉentry(')));
    Assert.equal([...Projected.Project.toString('utf8').matchAll(/^(?:root|source) /gmu)].length, 23);
    Assert.ok(!Projected.Project.includes(Buffer.from('Source-Wir-Scalar-Constants.wv')));
    Assert.ok(Core.equals(Originalˉcore) && Manifest.equals(Originalˉmanifest));
    Pass('selected-source-geometry-and-exact-inverse');
    Assert.equal(DIRECT_CONDITION_SELECTORS.length, 3);
    Assert.ok(Object.isFrozen(DIRECT_CONDITION_SELECTORS));
    Assert.deepEqual(Projected.Selectors, DIRECT_CONDITION_SELECTORS);
    for (const Selector of DIRECT_CONDITION_SELECTORS) {
        Assert.ok(Object.isFrozen(Selector));
        Assert.ok(Number.isSafeInteger(Selector.bytes) && Selector.bytes > 0);
        Assert.match(Selector.sha256, /^[0-9a-f]{64}$/u);
    }
    Assert.equal(Core.length - Projected.Core.length,
        DIRECT_CONDITION_SELECTORS[0].bytes + DIRECT_CONDITION_SELECTORS[1].bytes -
        DIRECT_CONDITION_SELECTORS[1].replacementBytes);
    Assert.equal(Manifest.length - Projected.Project.length, DIRECT_CONDITION_SELECTORS[2].bytes);
    Pass('bounded-selector-identities-and-payload-extents');
    const Suffixˉbytes = Buffer.from('// unrelated preserved input\n');
    const Outside = Projectˉdirectˉconditionˉanalyzer(Buffer.concat([Core, Suffixˉbytes]), Manifest);
    Assert.ok(Outside.Core.equals(Buffer.concat([Projected.Core, Suffixˉbytes])));
    Assert.ok(Outside.Project.equals(Projected.Project));
    Pass('unselected-source-preservation');
    for (const [Source, Project] of [
        [Buffer.from(Core.toString('utf8').replace('import Compilerˉsourceˉwirˉscalarˉconstants',
            '//missing Compilerˉsourceˉwirˉscalarˉconstants')), Manifest],
        [Core, Buffer.concat([Manifest, Buffer.from('source "Compiler/Windvale/Source-Wir-Scalar-Constants.wv"\n')])],
        [Buffer.from(Core.toString('utf8').replace('fn Compilerˉsourceˉwirˉpoolˉfunction(', 'fn Changed(')), Manifest],
    ]) Assert.throws(() => Projectˉdirectˉconditionˉanalyzer(Source, Project));
    Pass('selector-drift-and-repetition-refusal');
    for (const Value of [null, { Mode: 'behavior' }, { Mode: 'prepare', Key: 'wrong' }]) {
        Assert.throws(() => Requireˉdirectˉconstruction(Value), /explicit preparation/u);
    }
    Pass('explicit-preparation-context-refusal');
    const Caseˉwork = path.join(Testˉroot, '4-complete');
    const Family = path.join(Caseˉwork, 'Cache', 'current-split-compiler-v2', HOST);
    const Construction = { Mode: 'prepare', Key: '2'.repeat(64), Family, Deadline,
        Requireˉunchanged: async () => {} };
    Assert.throws(() => Requireˉdirectˉconstruction({ ...Construction, Deadline: 1 }),
        Failure => Failure.exitCode === 124 && /deadline expired/u.test(Failure.message));
    Pass('inherited-absolute-deadline-refusal');
    // Reuse the existing successful mocked construction; none of these products executes.
    const Suffix = process.platform === 'win32' ? '.exe' : '.elf';
    const Predecessor = {};
    for (const Name of ['Analyzer', 'Emitter', 'Reader', 'Admitter', 'Authenticator', 'Binder']) {
        Predecessor[Name] = path.join(Caseˉwork, 'Predecessor-' + Name + Suffix);
    }
    for (const Name of ['Analyzer', 'Emitter']) {
        Predecessor[Name + 'ˉidentity'] = path.join(Caseˉwork, 'Predecessor-' + Name + '.identity');
    }
    Predecessor.Construction = { kind: 'source-edition-predecessor-1',
        revision: '992ba7bbebbbf3356966bdcc0ea1d2b0e5923720',
        tree: 'c6231310843c24a03b0253f6f5a062d77db9f73f', key: '1'.repeat(64), host: HOST };
    let Callbackˉcalls = 0;
    const Neverˉproduce = async () => {
        Callbackˉcalls += 1;
        throw new Error('Unexpected direct intermediate producer callback.');
    };
    const Options = { Work: Caseˉwork, Predecessor, Construction,
        Analysisˉproject: path.join(Caseˉwork, 'Projects', 'Windvale-Compiler-Analysis-Driver.wvproj'),
        Runˉnative: Neverˉproduce, Runˉnode: Neverˉproduce };
    Time();
    const Hit = await Acquireˉdirectˉconditionˉanalyzer(Options);
    Assert.equal(Callbackˉcalls, 0);
    Assert.equal(Hit.request.currentKey, Construction.Key);
    Assert.equal(Hit.request.selectorVersion, DIRECT_CONDITION_SELECTOR_VERSION);
    const Owner = await readFile(path.join(SCRIPT_DIRECTORY, 'Direct-Condition-Analyzer-Intermediate-Core.mjs'));
    Assert.deepEqual(Hit.request.owner, { bytes: Owner.length,
        sha256: createHash('sha256').update(Owner).digest('hex') });
    Pass('intermediate-cache-hit-zero-producer-callbacks');
    async function Rejected(Change, Pattern) {
        Time();
        await Assert.rejects(() => Acquireˉdirectˉconditionˉanalyzer(Change), Pattern);
        Assert.equal(Callbackˉcalls, 0);
        Time();
    }
    const Selectedˉmanifest = await readFile(Options.Analysisˉproject);
    try {
        await writeFile(Options.Analysisˉproject, Buffer.concat([Selectedˉmanifest, Buffer.from('\n')]));
        await Rejected(Options, /differs from the selected current repository project/u);
    } finally { await writeFile(Options.Analysisˉproject, Selectedˉmanifest); }
    Pass('alternate-manifest-refuses-before-cache-or-producer');
    const Checkpoint = path.join(path.dirname(Hit.Analyzer), 'Checkpoint.json');
    const Checkpointˉbytes = await readFile(Checkpoint);
    try {
        await writeFile(Checkpoint, Buffer.from('{}\n'));
        await Rejected(Options, /checkpoint request or product record differs/u);
    } finally { await writeFile(Checkpoint, Checkpointˉbytes); }
    Pass('corrupt-intermediate-refuses-without-fallback');
    const Cachedˉwvb = path.join(path.dirname(Hit.Analyzer), 'Analyzer.wvb');
    const Wvbˉbytes = await readFile(Cachedˉwvb);
    const Wrongˉversion = Buffer.from(Wvbˉbytes);
    Wrongˉversion.writeUInt16LE(10, 6);
    try {
        await writeFile(Cachedˉwvb, Wrongˉversion);
        await Rejected(Options, /exact WVB 1.11/u);
    } finally { await writeFile(Cachedˉwvb, Wvbˉbytes); }
    Pass('wrong-source-epoch-refuses-without-fallback');
    for (const Name of ['WINDVALE_PREPARED_PRODUCTS_ONLY', 'WINDVALE_PREPARED_COMPILER_ONLY']) {
        const Previous = process.env[Name];
        try {
            process.env[Name] = '1';
            await Rejected(Options, new RegExp('refuses inherited ' + Name, 'u'));
        } finally {
            if (Previous === undefined) delete process.env[Name];
            else process.env[Name] = Previous;
        }
        Pass(Name === 'WINDVALE_PREPARED_PRODUCTS_ONLY'
            ? 'prepared-products-only-refuses-before-callback'
            : 'prepared-compiler-only-refuses-before-callback');
    }
    await Rejected({ ...Options, Construction: { ...Construction,
        Requireˉunchanged: async () => { throw new Error('Direct unchanged guard failed.'); } } },
    /Direct unchanged guard failed/u);
    Pass('identity-guard-failure-is-not-success');
    let Predecessors = 0;
    let Commands = 0;
    const Nullˉwork = path.join(Testˉroot, 'Direct-Null-Context');
    const Candidate = path.join(Nullˉwork, 'Candidate');
    await mkdir(Candidate, { recursive: true });
    await Assert.rejects(() => Constructˉcurrentˉsplitˉcompiler(Nullˉwork, Candidate,
        async () => { Commands += 1; }, async () => { Commands += 1; },
        async () => { Predecessors += 1; }, path.join(REPOSITORY_ROOT, 'Projects', 'Tools')),
    /explicit preparation/u);
    Assert.equal(Predecessors, 0);
    Assert.equal(Commands, 0);
    Assert.equal(Callbackˉcalls, 0);
    Assert.deepEqual(await readdir(Candidate), []);
    Pass('missing-context-before-predecessor-or-command');
    Assert.ok((await readFile(path.join(REPOSITORY_ROOT, 'Compiler/Windvale/Source-Wir-Core.wv'))).equals(Core));
    Assert.ok((await readFile(path.join(REPOSITORY_ROOT,
        'Projects/Tools/Windvale-Compiler-Analysis-Driver.wvproj'))).equals(Manifest));
    Assert.equal(Cases, 14);
    Time();
    return Cases;
}

async function Verifyˉauthenticatedˉprojectˉcache(Producers) {
    const Parent = await realpath(path.join(REPOSITORY_ROOT, 'Artifacts', 'Work'));
    const Work = await mkdtemp(path.join(Parent, 'Authenticated-Split-Cache-Test-'));
    let Cases = 0;
    try {
        const Relative = path.relative(REPOSITORY_ROOT, Work).split(path.sep).join('/');
        const Project = path.join(Work, 'Test.wvproj');
        const Output = path.join(Work, 'Test.wvb');
        const Cache = path.join(Work, 'Cache');
        let Manifest = await readFile(path.join(REPOSITORY_ROOT,
            'Projects/Tests/Windvale-Native-Test-Canonical-Package-Text.wvproj'), 'utf8');
        const Metadata = [];
        for (const Kind of ['source-input-lock', 'source-profile', 'target-descriptor']) {
            const Match = new RegExp(`^${Kind} "([^"]+)"$`, 'm').exec(Manifest);
            Assert.ok(Match, `Missing ${Kind}`);
            const Name = path.posix.basename(Match[1]);
            const File = path.join(Work, Name);
            const Bytes = await readFile(path.join(REPOSITORY_ROOT, Match[1]));
            await writeFile(File, Bytes, { flag: 'wx' });
            Metadata.push({ Kind, File, Bytes });
            Manifest = Manifest.replace(Match[0], `${Kind} "${Relative}/${Name}"`);
        }
        await writeFile(Project, Manifest, { flag: 'wx' });
        const Arguments = [CACHE_SCRIPT, Project, Output, ...Producers.slice(0, 4),
            '--authenticated-project4', ...Producers.slice(4)];
        function Run(Values = Arguments, Cacheˉpath = Cache) {
            return spawnSync(process.execPath, Values, { cwd: REPOSITORY_ROOT,
                env: { ...process.env, WINDVALE_NATIVE_CACHE_ROOT: Cacheˉpath },
                encoding: 'utf8', windowsHide: true, timeout: 45_000,
                maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES });
        }
        function Pass(Name) { Cases += 1; console.log(`split project4 cache item=${Cases} case=${Name} status=Passed`); }
        function Successful(Result, State) {
            Assert.equal(Result.status, 0, Childˉdiagnostic(Result));
            Assert.equal(Result.stderr, '', Childˉdiagnostic(Result));
            Assert.ok(Result.stdout.includes(`cache=${State}`), Childˉdiagnostic(Result));
        }
        Successful(Run(), 'Created');
        const Product = await readFile(Output);
        Assert.equal(Product.subarray(0, 3).toString('ascii'), 'WVB');
        Pass('cold-authenticated-construction');
        Successful(Run(), 'Hit');
        Assert.deepEqual(await readFile(Output), Product);
        Pass('authenticated-cache-reuse');
        Successful(Run(Arguments, path.join(Work, 'Independent-Cache')), 'Created');
        Assert.deepEqual(await readFile(Output), Product);
        Pass('independent-cache-determinism');
        async function Rejected(Name, Values = Arguments) {
            const Result = Run(Values);
            Assert.notEqual(Result.status, 0, Childˉdiagnostic(Result));
            Assert.equal(Result.error, undefined, Childˉdiagnostic(Result));
            Assert.ok(!Result.stdout.includes('cache=Hit'), Childˉdiagnostic(Result));
            Assert.deepEqual(await readFile(Output), Product, 'Rejection changed the previous output.');
            Pass(Name);
        }
        await Rejected('project4-needs-explicit-producers', Arguments.slice(0, 7));
        await writeFile(Project, Manifest.replace(/source-input-lock-sha256 [0-9a-f]{64}/u,
            'source-input-lock-sha256 ' + '0'.repeat(64)));
        await Rejected('wrong-lock-digest');
        await writeFile(Project, Manifest);
        for (const Item of Metadata) {
            const Changed = Buffer.from(Item.Bytes);
            Changed[0] ^= 1;
            await writeFile(Item.File, Changed);
            await Rejected(`${Item.Kind}-content-invalidation`);
            await writeFile(Item.File, Item.Bytes);
        }
        const Invalidˉidentity = path.join(Work, 'Invalid.identity');
        const Originalˉidentity = await readFile(Producers[1], 'ascii');
        await writeFile(Invalidˉidentity, Originalˉidentity.replace(/sha256 [0-9a-f]{64}/u,
            'sha256 ' + '0'.repeat(64)));
        const Wrongˉproducer = [...Arguments];
        Wrongˉproducer[4] = Invalidˉidentity;
        await Rejected('wrong-analyzer-identity', Wrongˉproducer);
        const Missingˉproducer = [...Arguments];
        Missingˉproducer[8] = path.join(Work, 'Missing-Admitter');
        await Rejected('missing-admission-producer', Missingˉproducer);
        Successful(Run(), 'Hit');
        Assert.deepEqual(await readFile(Output), Product);
        Pass('restored-inputs-reuse-evidence');
        await writeFile(path.join(Work, 'Hosted.wv'),
            '#!wv/1 en@1\nmodule Hostedˉcacheˉfixture;\nprofile hosted;\n' +
            'platform linux, windows;\nauthority application;\n' +
            'requires capability console.write_line version 1;\n' +
            'import Foundationˉoption as Option;\n' +
            'export fn Main() -> i32 { console.write_line("metadata"); return 0; }\n');
        const Hostedˉmanifest = Manifest.replace(/^root "[^"]+"$/mu, `root "${Relative}/Hosted.wv"`)
            .replace(/^source "Libraries\/Package\/Canonical-Package-Text.wv"\r?\n/mu, '');
        await writeFile(Project, Hostedˉmanifest);
        Successful(Run(), 'Created');
        const Hosted = await readFile(Output);
        Assert.equal(Hosted[20], 2, 'The hosted profile was lost.');
        let Cursor = 25 + Hosted.readUInt32LE(21);
        Assert.equal(Hosted[Cursor++], 1);
        Assert.equal(Hosted[Cursor++], 1);
        Assert.equal(Hosted[Cursor++], 2, 'Application authority was lost.');
        function Number() { const Value = Hosted.readUInt32LE(Cursor); Cursor += 4; return Value; }
        function Text() { const Length = Number(); const Value = Hosted.subarray(Cursor, Cursor + Length).toString('utf8'); Cursor += Length; return Value; }
        Assert.equal(Number(), 2);
        Assert.deepEqual([Text(), Text()], ['linux', 'windows']);
        Assert.equal(Number(), 1, 'The versioned capability requirement was lost.');
        Assert.equal(Text(), 'console.write_line');
        Assert.equal(Number(), 1);
        Assert.equal(Number(), 0);
        Assert.equal(Cursor, 20 + Hosted.readUInt32LE(16));
        Pass('hosted-header-metadata-preserved');
        Successful(Run(), 'Hit');
        Assert.deepEqual(await readFile(Output), Hosted);
        Pass('hosted-metadata-cache-reuse');
        console.log(`split project4 cache cases=${Cases} status=Passed wvb-sha256=` +
            createHash('sha256').update(Product).digest('hex'));
    } finally {
        Assert.equal(path.dirname(path.resolve(Work)), Parent);
        Assert.ok(path.basename(Work).startsWith('Authenticated-Split-Cache-Test-'));
        Assert.equal(await realpath(Work), Work);
        await rm(Work, { recursive: true, force: true });
    }
}

async function Verifyˉfoundationˉownerˉpreparation(Testˉroot) {
    let Cases = 0;
    Assert.equal(Parseˉfoundationˉownerˉarguments(['--foundation-borrow']), null);
    Assert.deepEqual(Parseˉfoundationˉownerˉarguments(['--foundation-borrow-owners']),
        { Prepare: false, Prepared: false, Maximum: 600 });
    Assert.deepEqual(Parseˉfoundationˉownerˉarguments(['--foundation-borrow-owners', '--prepare-only', '--maximum-seconds', '4500']),
        { Prepare: true, Prepared: false, Maximum: 4500 });
    Assert.deepEqual(Parseˉfoundationˉownerˉarguments(['--foundation-borrow-owners', '--prepared-products-only', '--maximum-seconds', '600']),
        { Prepare: false, Prepared: true, Maximum: 600 });
    Cases += 4;
    for (const Tail of [['--prepare-only'], ['--prepared-products-only'], ['--maximum-seconds', '0'],
        ['--prepare-only', '--maximum-seconds', '4501'], ['--prepared-products-only', '--maximum-seconds', '601'],
        ['--prepared-products-only', '--prepare-only', '--maximum-seconds', '600'], ['--unknown']]) {
        Assert.throws(() => Parseˉfoundationˉownerˉarguments(['--foundation-borrow-owners', ...Tail]),
            Error => Error.exitCode === 64);
        Cases += 1;
    }
    const Names = ['WINDVALE_PREPARED_COMPILER_ONLY', 'WINDVALE_PREPARED_PRODUCTS_ONLY'];
    const Original = Names.map(Name => process.env[Name]);
    try {
        for (const State of [undefined, '', 'before']) {
            for (const Name of Names) {
                if (State === undefined) delete process.env[Name]; else process.env[Name] = State;
            }
            await Assert.rejects(() => Withˉfoundationˉownerˉenvironment({Prepare:false, Prepared:true}, async () => {
                Assert.deepEqual(Names.map(Name => process.env[Name]), ['1', '1']);
                throw Object.assign(new Error('child refused'), {exitCode:64});
            }), Error => Error.exitCode === 64);
            Assert.deepEqual(Names.map(Name => process.env[Name]), [State, State]);
            Cases += 1;
        }
        await Assert.rejects(() => Withˉfoundationˉownerˉenvironment({Prepare:true, Prepared:false},
            async () => Assert.fail('Prepared construction invoked a child.')), Error => Error.exitCode === 64);
        Cases += 1;
        for (const Name of Names) delete process.env[Name];
        const Key = '9'.repeat(64), Suffix = process.platform === 'win32' ? '.exe' : '.elf';
        for (const Mode of ['warm', 'prepared-hit', 'compiler-miss', 'source-miss', 'image-miss',
            'late-source-change', 'late-product-change', 'changed-key']) {
            const Work = path.join(Testˉroot, 'foundation-owner-' + Mode), Family = path.join(Work, 'Cache');
            await mkdir(path.join(Family, Key), {recursive:true});
            for (const Name of ['Analyzer', 'Emitter', 'Admitter', 'Authenticator', 'Reader', 'Binder']) {
                await writeFile(path.join(Family, Key, Name + Suffix), Buffer.from(Name), {mode:0o755});
                if (Name === 'Analyzer' || Name === 'Emitter') await writeFile(path.join(Family, Key, Name + '.identity'), Name);
            }
            const Calls = [], Deadline = Date.now() + 30_000;
            let Changed = false, Products;
            const Request = {Prepare:Mode === 'warm', Prepared:Mode !== 'warm'};
            const Action = () => Withˉfoundationˉownerˉenvironment(Request, async () => {
                Products = await Acquireˉfoundationˉborrowˉtestˉproducts({Work, Deadline, Selection:'owners',
                    Prepareˉcompiler:Request.Prepare,
                    Getˉkey:async () => Changed && Mode === 'changed-key' ? '8'.repeat(64) : Key,
                    Getˉfamily:async () => Family,
                    Acquire:async (Givenˉfamily, Givenˉkey, Produce, Requireˉcompilerˉunchanged) => {
                        Assert.equal(Givenˉfamily, Family); Assert.equal(Givenˉkey, Key);
                        if (Mode === 'compiler-miss') await Produce(path.join(Family, Key));
                        await Requireˉcompilerˉunchanged();
                        return {status:'Hit', directory:path.join(Family, Key)};
                    },
                    Snapshot:async Projects => {
                        Assert.deepEqual(Projects.map(Project => path.basename(Project)), ['Windvale-Native-Test-Foundation-Owner-Flow.wvproj']);
                        return 'exact-owner-closure';
                    },
                    Requireˉunchanged:async Snapshot => {
                        Assert.equal(Snapshot, 'exact-owner-closure');
                        if (Changed && Mode === 'late-source-change') throw new Error('owner source changed');
                    },
                    Run:async (Label, Command, Arguments, Boundary) => {
                        Calls.push(Label); Assert.equal(Command, process.execPath); Assert.equal(Boundary, Deadline);
                        Assert.deepEqual(Names.map(Name => process.env[Name]), Request.Prepare ? [undefined, undefined] : ['1', '1']);
                        if (Label === 'foundation-products-build') {
                            Assert.equal(Request.Prepare, true);
                            Assert.deepEqual(Arguments.slice(1, 4), ['--prepare-compiler','--deadline-ms',String(Deadline)]);
                            Assert.equal(Arguments.length, 6);
                            Assert.equal(path.basename(Arguments[4]), 'Windvale-Native-Test-Foundation-Owner-Flow.wvproj');
                            await writeFile(Arguments[5], 'owner-wvb'); return;
                        }
                        if (Label === 'foundation-products-build-owners') {
                            Assert.equal(path.basename(Arguments[0]), 'Build-Cached-Split-Project-Wvb.mjs');
                            Assert.equal(Arguments[7], '--authenticated-project4'); Assert.equal(Arguments.length, 12);
                            if (Mode === 'source-miss') throw Object.assign(new Error('prepared source cache miss'), {exitCode:64});
                            await writeFile(Arguments[2], 'owner-wvb'); return;
                        }
                        Assert.equal(Label, 'foundation-products-package-owners');
                        Assert.equal(path.basename(Arguments[0]), 'Build-Cached-Segmented-Hosted-Wvb.mjs');
                        Assert.deepEqual(Arguments.slice(1, 4), ['--deadline-ms', String(Deadline), '1']);
                        if (Mode === 'image-miss') throw Object.assign(new Error('prepared image cache miss'), {exitCode:64});
                        await writeFile(Arguments[5], 'owner-native', {mode:0o755});
                    },
                });
                Assert.equal(Products.Evidence.length, 10);
                Changed = true;
                if (Mode === 'late-product-change') await writeFile(Products.Owners, 'changed-native');
                await Products.Requireˉunchanged();
                return Products;
            });
            if (Mode === 'warm' || Mode === 'prepared-hit') {
                await Action(); Assert.equal(Calls.length, 2);
            } else {
                await Assert.rejects(Action, /checkpoint missing|prepared source cache miss|packaging failed|source changed|product changed|inputs changed/);
                Assert.equal(Calls.length, Mode === 'compiler-miss' ? 0 : Mode === 'source-miss' ? 1 : 2);
            }
            Assert.equal(Calls.some(Label => Label.includes('execute')), false);
            Assert.deepEqual(Names.map(Name => process.env[Name]), [undefined, undefined]);
            Cases += 1;
        }
    } finally {
        for (let Index = 0; Index < Names.length; Index += 1) {
            if (Original[Index] === undefined) delete process.env[Names[Index]];
            else process.env[Names[Index]] = Original[Index];
        }
    }
    return Cases;
}

async function Verifyˉcallableˉphases(Testˉroot) {
    let Cases = 0;
    Assert.deepEqual(Parseˉcallableˉarguments([]), { Prepare: false, Prepared: false, Maximum: 3600 }); Cases++;
    Assert.deepEqual(Parseˉcallableˉarguments(['--prepare-only', '--maximum-seconds', '4500']),
        { Prepare: true, Prepared: false, Maximum: 4500 }); Cases++;
    Assert.deepEqual(Parseˉcallableˉarguments(['--prepare-only', '--maximum-seconds', '60',
        '--deadline-ms', '61000'], 1000), { Prepare: true, Prepared: false, Maximum: 60, Deadline: 61000 });
    Assert.deepEqual(Parseˉcallableˉarguments(['--prepared-products-only', '--maximum-seconds', '3600']),
        { Prepare: false, Prepared: true, Maximum: 3600 }); Cases++;
    for (const Arguments of [['--prepare-only'], ['--prepared-products-only'],
        ['--prepare-only', '--maximum-seconds', '4501'], ['--prepared-products-only', '--maximum-seconds', '3601'],
        ['--prepare-only', '--maximum-seconds', '0'], ['--prepare-only', '--maximum-seconds', '01'],
        ['--prepare-only', '--prepared-products-only', '--maximum-seconds', '600'],
        ['--prepare-only', '--maximum-seconds', '60', '--deadline-ms', '1000'],
        ['--prepare-only', '--maximum-seconds', '60', '--deadline-ms', '61001'],
        ['--prepare-only', '--maximum-seconds', '60', '--deadline-ms', '9007199254740992']])
        Assert.throws(() => Parseˉcallableˉarguments(Arguments, 1000), Error => Error.exitCode === 64);
    Cases++;
    const Names = ['WINDVALE_PREPARED_COMPILER_ONLY', 'WINDVALE_PREPARED_PRODUCTS_ONLY'];
    const Original = Names.map(Name => process.env[Name]);
    try {
        for (const Value of [undefined, '', 'kept']) {
            for (const Name of Names) { if (Value === undefined) delete process.env[Name]; else process.env[Name] = Value; }
            await Withˉcallableˉenvironment({ Prepare: false, Prepared: true }, async () => {
                for (const Name of Names) Assert.equal(process.env[Name], '1');
            });
            for (const Name of Names) Assert.equal(process.env[Name], Value);
        }
        Cases++;
        const Failure = Object.assign(new Error('seeded child failure'), { exitCode: 23 });
        await Assert.rejects(() => Withˉcallableˉenvironment({ Prepare: false, Prepared: true }, async () => {
            throw Failure;
        }), Error => Error === Failure);
        for (const Name of Names) Assert.equal(process.env[Name], 'kept'); Cases++;
        for (const Name of Names) {
            for (const Other of Names) delete process.env[Other];
            process.env[Name] = '1';
            let Calls = 0;
            await Assert.rejects(() => Withˉcallableˉenvironment({ Prepare: true, Prepared: false }, async () => { Calls++; }),
                Error => Error.exitCode === 64);
            Assert.equal(Calls, 0);
        }
        Cases++;
        const Work = await mkdtemp(path.join(Testˉroot, 'callable-phases-'));
        const Key = 'a'.repeat(64), Commands = [];
        let Keyˉreads = 0, Drifting = false, Refusal = null, Requireˉprepared = false;
        const Options = {
            Work, Deadline: Date.now() + 60_000,
            Getˉkey: async () => { Keyˉreads++; return Drifting && Keyˉreads > 1 ? 'b'.repeat(64) : Key; },
            Getˉfamily: async () => Work,
            Readˉcompiler: async () => ({ status: 'Hit', Requireˉunchanged: async () => {} }),
            Snapshot: async Projects => { Assert.equal(Projects.length, 8); return { marker: 'source-inputs' }; },
            Requireˉinputs: async Inputs => { Assert.equal(Inputs.marker, 'source-inputs'); },
            Evidence: async Path => ({ Path, Bytes: 1, Sha256: Key }),
            Run: async (Step, Tool, Arguments) => {
                if (Requireˉprepared) for (const Name of Names) Assert.equal(process.env[Name], '1');
                Commands.push({ Step, Tool, Arguments });
                if (Refusal && Step.startsWith(Refusal)) throw Object.assign(new Error('cache miss'), { exitCode: 64 });
            },
        };
        for (const Name of Names) delete process.env[Name];
        const Prepared = await Acquireˉcallableˉproducts({ ...Options, Prepareˉproducts: true });
        Assert.equal(Commands.length, 16);
        for (const Command of Commands) {
            Assert(!Command.Arguments.includes('--prepare-compiler'));
            if (Command.Step.startsWith('source-')) {
                Assert(Command.Arguments.includes('--prepared-compiler-only'));
                Assert.equal(Command.Arguments[Command.Arguments.indexOf('--compiler-checkpoint') + 1], Key);
            }
        }
        Assert.equal(Prepared.Products.length, 7); Assert.equal(Prepared.Verifier.Profile, '2');
        Assert.equal(Commands.filter(Command => Command.Step.startsWith('package-')).length, 8); Cases++;
        Commands.length = 0; Keyˉreads = 0; Requireˉprepared = true;
        await Withˉcallableˉenvironment({ Prepare: false, Prepared: true }, async () => {
            await Acquireˉcallableˉproducts(Options);
            Assert.equal(Commands.length, 16);
            for (const Command of Commands) {
                Assert(!Command.Arguments.includes('--prepare-compiler'));
                if (Command.Step.startsWith('source-')) Assert(Command.Arguments.includes('--prepared-compiler-only'));
            }
        }); Cases++;
        for (const Kind of ['source-', 'package-']) {
            Refusal = Kind; Commands.length = 0; Keyˉreads = 0;
            await Withˉcallableˉenvironment({ Prepare: false, Prepared: true }, async () => {
                await Assert.rejects(() => Acquireˉcallableˉproducts(Options), Error => Error.exitCode === 64);
            });
            Assert.equal(Commands.length, Kind === 'source-' ? 1 : 2);
            Assert(Commands.every(Command => !Command.Arguments.includes('--prepare-compiler')));
            Cases++;
        }
        Refusal = null; Commands.length = 0; Requireˉprepared = false;
        await Assert.rejects(() => Acquireˉcallableˉproducts({ ...Options, Deadline: Date.now() - 1 }), Error => Error.exitCode === 124);
        Assert.equal(Commands.length, 0);
        await Assert.rejects(() => Acquireˉcallableˉproducts({ ...Options,
            Readˉcompiler: async () => ({ status: 'Miss' }) }), Error => Error.exitCode === 64);
        Assert.equal(Commands.length, 0); Cases++;
        Commands.length = 0; Keyˉreads = 0; Drifting = true;
        await Assert.rejects(() => Acquireˉcallableˉproducts(Options), /compiler inputs changed/u); Cases++;
        Commands.length = 0;
        await Withˉcallableˉenvironment({ Prepare: false, Prepared: true }, async () => {
            await Assert.rejects(() => Acquireˉcallableˉproducts({ ...Options, Prepareˉproducts: true }), Error => Error.exitCode === 64);
        });
        Assert.equal(Commands.length, 0); Cases++;
        Assert.equal(Cases, 14);
    } finally {
        for (const [Index, Name] of Names.entries()) {
            if (Original[Index] === undefined) delete process.env[Name]; else process.env[Name] = Original[Index];
        }
    }
    return Cases;
}

async function Verifyˉconstructionˉreadiness() {
    const fs = await import('node:fs/promises');
    const Temporary = realpathSync.native(os.tmpdir());
    const Root = await fs.mkdtemp(path.join(Temporary, 'windvale-construction-readiness-'));
    try {
    const Work = path.join(Root, 'Control-Scaffold');
    const Tooling = path.join(Work, 'Tools', 'Native');
    const Started = Date.now();
    const Deadline = Started + 30_000;
    const Cases = [];
    const Digest = Value => createHash('sha256').update(Value).digest('hex');
    await fs.mkdir(Tooling, { recursive: true });
    await fs.mkdir(path.join(Work, 'Inputs'), { recursive: true });
    await fs.mkdir(path.join(Work, 'Output'), { recursive: true });
    const Source = path.join(Work, 'Inputs', 'Source.wv');
    const Project = path.join(Work, 'Inputs', 'Project.wvproj');
    await fs.writeFile(Source, '#!wv/1/core\nmodule Control profile portable;\nexport fn Main() -> i32 { return 42; }\n');
    await fs.writeFile(Project, 'windvale-project 4\nroot "Inputs/Source.wv"\nemit wvb\n');
    await fs.copyFile(path.join(SCRIPT_DIRECTORY, 'Project-Construction-Readiness-Core.mjs'),
        path.join(Tooling, 'Project-Construction-Readiness-Core.mjs'));
    const Actualˉmodule = await fs.readFile(path.join(Tooling, 'Project-Construction-Readiness-Core.mjs'),'utf8');
    const Filesystemˉimport = "from 'node:fs/promises';";
    Assert.equal(Actualˉmodule.split(Filesystemˉimport).length,2);
    await fs.writeFile(path.join(Tooling,'Project-Construction-Readiness-Core.mjs'),
        Actualˉmodule.replace(Filesystemˉimport,"from './Filesystem-Control.mjs';"));
    await fs.writeFile(path.join(Tooling,'Filesystem-Control.mjs'),`
    import * as fs from 'node:fs/promises'; import path from 'node:path';
    export { access,lstat,realpath,unlink } from 'node:fs/promises';
    export const Control={Cache:null,CacheWrites:0};
    export async function open(P,...A){if(Control.Cache!==null&&path.dirname(P)===Control.Cache&&path.basename(P).startsWith('.windvale-readiness-')){
     Control.CacheWrites++;throw Object.assign(Error('Read-only cache'),{code:'EACCES'});}return fs.open(P,...A);}
    export async function statfs(P,...A){return P===Control.Cache?{bsize:4096n,bavail:0n}:fs.statfs(P,...A);}
    `);
    await fs.copyFile(path.join(SCRIPT_DIRECTORY, 'Build-Current-Split-Project-Wvb.mjs'),
        path.join(Tooling, 'Build-Current-Split-Project-Wvb.mjs'));
    await fs.writeFile(path.join(Tooling, 'Generate-Compiler-Artifact-Readers.mjs'), '// generator command is mocked; not executed\n');
    await fs.writeFile(path.join(Tooling, 'Native-Project-Cache-Key-Core.mjs'), `
    import fs from 'node:fs/promises'; import path from 'node:path'; import { createHash } from 'node:crypto';
    export const Control = { Requests: 0, Checks: 0, Slow: false, Source: null };
    const Hash = B => createHash('sha256').update(B).digest('hex');
    async function Evidence(P) { const B=await fs.readFile(P); return { path:P,bytes:B.length,sha256:Hash(B) }; }
    export async function Prepareˉnativeˉprojectˉcacheˉcontext(N,P) { return {producerEvidence:await Promise.all(P.map(Evidence))}; }
    export async function Getˉnativeˉprojectˉcacheˉrequest(C,P) { Control.Requests++; if(Control.Slow) await new Promise(R=>setTimeout(R,30));
     const S=Control.Source??path.join(path.dirname(P),'Source.wv'), inputEvidence=await Promise.all([P,S].map(Evidence));
     return { context:C,projectPath:P,inputEvidence,key:Hash(Buffer.from(JSON.stringify(inputEvidence))) }; }
    export async function Requireˉnativeˉprojectˉcacheˉrequestˉunchanged(R) { Control.Checks++; for(const E of [...R.inputEvidence,...R.context.producerEvidence]) {
     if((await Evidence(E.path)).sha256!==E.sha256) throw Error('Input changed'); } }
    `);
    await fs.writeFile(path.join(Tooling, 'Development-Command-Core.mjs'), `
    import fs from 'node:fs/promises';
    export const Control = { Calls: [], Mode: 'Pass', Source: null };
    export async function Runˉdevelopmentˉcommand(T,A,D,S,M) { Control.Calls.push({T,A,D,S,M});
     if(Control.Mode==='Timeout') throw Object.assign(Error('mock child timeout'),{exitCode:124});
     if(Control.Mode==='Change') await fs.appendFile(Control.Source,'// changed\\n');
     if(Control.Mode==='Fail') return {Code:1,Output:'Mismatch',Error:'Mismatch'};
     if(Control.Mode==='Fatal') return {Code:2,Output:'Mismatch',Error:'Mismatch'};
     return {Code:0,Output:T==='git'?'git version 2.50.0\\n':'artifact-reader checks=17\\n',Error:''}; }
    `);
    await fs.writeFile(path.join(Tooling, 'Current-Split-Compiler-Cache-Core.mjs'), `
    export const Control = { Key:0,Family:0,Acquire:0,Construct:0,Read:0 };
    export const CURRENT_COMPILER_PROJECTS = ['Project.wvproj'];
    export function Getˉcurrentˉsplitˉcompilerˉcacheˉroot() { return process.env.WINDVALE_NATIVE_CACHE_ROOT; }
    export async function Getˉcurrentˉsplitˉcompilerˉkey() {Control.Key++;throw Error('Unexpected key');}
    export async function Getˉcurrentˉsplitˉcompilerˉfamily() {Control.Family++;throw Error('Unexpected family');}
    export async function Acquireˉcurrentˉsplitˉcompiler() {Control.Acquire++;throw Error('Unexpected acquisition');}
    export async function Constructˉcurrentˉsplitˉcompiler() {Control.Construct++;throw Error('Unexpected construction');}
    export async function Readˉpreparedˉsplitˉcompiler() {Control.Read++;throw Error('Unexpected admission');}
    `);
    await fs.writeFile(path.join(Tooling, 'Source-Edition-Predecessor-Core.mjs'), `
    export function Hasˉuncertainˉconstructionˉcleanup(E) { return E?.cleanupUncertain===true; }
    export async function Constructˉsourceˉeditionˉpredecessor() {throw Error('Unexpected predecessor');}
    export async function Runˉcompilerˉconstructionˉcommand() {throw Error('Unexpected compiler');}
    `);
    const Module = await import(pathToFileURL(path.join(Tooling, 'Project-Construction-Readiness-Core.mjs')).href);
    const Requests = (await import(pathToFileURL(path.join(Tooling, 'Native-Project-Cache-Key-Core.mjs')).href)).Control;
    const Commands = (await import(pathToFileURL(path.join(Tooling, 'Development-Command-Core.mjs')).href)).Control;
    const Compiler = (await import(pathToFileURL(path.join(Tooling, 'Current-Split-Compiler-Cache-Core.mjs')).href)).Control;
    const Filesystem = (await import(pathToFileURL(path.join(Tooling,'Filesystem-Control.mjs')).href)).Control;
    const Configuration = () => ({ Repository: Work, Projects: [Project], Temporary: Work,
        Cache: path.join(Work, 'Not-Created', 'Cache'), Outputs: [path.join(Work, 'Output', 'Output.wvb')],
        Prepare: false, Preparedˉonly: true, Deadline });
    function Done(Name) { Cases.push(Name); }
    Assert.equal(Module.Classifyˉreadinessˉspace(1n, 2n), 'Warning'); Done('Positive-low-space-warns');
    Assert.equal(Module.Classifyˉreadinessˉspace(2n, 2n), 'Observed'); Done('Advisory-equality');
    Assert.equal(Module.Classifyˉreadinessˉspace(2n**70n, 2n), 'Observed'); Done('Bigint-space-does-not-wrap');
    Assert.throws(()=>Module.Classifyˉreadinessˉspace(0n, 0n)); Done('Zero-space-refuses');
    Assert.throws(()=>Module.Classifyˉreadinessˉspace(-1n, 0n)); Done('Negative-space-refuses');
    Assert.throws(()=>Module.Classifyˉreadinessˉspace(undefined, 0n)); Done('Unavailable-space-refuses');
    Assert.throws(()=>Module.Requireˉreadinessˉtime(Date.now()-1), E=>E.exitCode===124); Done('Expired-deadline');
    const Plain = await Module.Checkˉprojectˉconstructionˉreadiness(Configuration());
    Assert.equal(Plain.Semanticˉvalidation,'NotRun'); Assert.equal(Plain.Compilerˉadmission,'OrdinaryPathPending');
    Assert.equal(Plain.Generator,'NoDeclaredGeneratedReader'); Assert.equal(Commands.Calls.length,0);
    Assert.equal(Plain.Node.Bytes, (await fs.stat(process.execPath)).size); Done('Plain-profile-and-actual-node-metadata');
    Assert.equal(await fs.lstat(path.join(Work,'Not-Created')).then(()=>true,()=>false),false);
    Assert.equal((await fs.readdir(Work)).filter(V=>V.startsWith('.windvale-readiness-')).length,0);
    Done('No-cache-creation-and-probes-removed');
    const Readˉonlyˉcache = path.join(Work,'Read-Only-Cache');
    await fs.mkdir(Readˉonlyˉcache,{recursive:true});
    Filesystem.Cache=Readˉonlyˉcache;
    const Reused = await Module.Checkˉprojectˉconstructionˉreadiness({...Configuration(),Cache:Readˉonlyˉcache});
    const Cacheˉobservation=Reused.Resources.find(Value=>Value.Role==='Cache');
    Assert.equal(Cacheˉobservation.Availableˉbytes,'0');
    Assert.equal(Cacheˉobservation.Access,'ReadOnlyObservation');Assert.equal(Filesystem.CacheWrites,0);
    await Assert.rejects(Module.Checkˉprojectˉconstructionˉreadiness({...Configuration(),Cache:Readˉonlyˉcache,Preparedˉonly:false}),/no available bytes/u);
    Assert.equal(Filesystem.CacheWrites,0);Filesystem.Cache=null;
    Done('Prepared-read-only-full-cache-reuse-without-write-probe');
    const Artifact = path.join(Work, 'Compiler', 'Windvale', 'Source-Missing-Artifact-Core.wv');
    await fs.mkdir(path.dirname(Artifact), { recursive:true });
    await fs.writeFile(Artifact,'module Missing_marker profile portable;\n');
    Requests.Source=Artifact;
    const Missingˉmarker = await Module.Checkˉprojectˉconstructionˉreadiness(Configuration());
    Assert.equal(Missingˉmarker.Generator,'CanonicalCheckPassed');
    Assert.equal(Module.Isˉmaintainedˉartifactˉinput(Work, Source),false);
    Requests.Source=null; Commands.Calls=[];
    Done('Maintained-artifact-with-deleted-marker-still-checks-generator');
    const Generated = '// Generated by Tools/Native/Generate-Compiler-Artifact-Readers.mjs.\nmodule Control;\n';
    await fs.writeFile(Source,Generated);
    const Checked = await Module.Checkˉprojectˉconstructionˉreadiness(Configuration());
    Assert.equal(Checked.Generator,'CanonicalCheckPassed');
    Assert.deepEqual(Commands.Calls.at(-1).A,[path.join(Tooling,'Generate-Compiler-Artifact-Readers.mjs'),'--check']);
    Assert.ok(Commands.Calls.at(-1).D<=Deadline); Assert.equal(Commands.Calls.at(-1).S,false);
    Done('Only-canonical-generator-check-with-inherited-deadline');
    Commands.Mode='Fail'; await Assert.rejects(Module.Checkˉprojectˉconstructionˉreadiness(Configuration()),/artifact check failed/u);
    Done('Generator-mismatch-refuses');
    Commands.Mode='Fatal'; await Assert.rejects(Module.Checkˉprojectˉconstructionˉreadiness(Configuration()),E=>E.exitCode===2);
    Done('Generator-fatal-child-status-retained');
    await Assert.rejects(Module.Checkˉprojectˉconstructionˉreadiness({...Configuration(),Prepare:true,Preparedˉonly:false}),E=>E.exitCode===2);
    Done('Git-fatal-child-status-retained');
    Commands.Mode='Timeout'; await Assert.rejects(Module.Checkˉprojectˉconstructionˉreadiness(Configuration()),E=>E.exitCode===124);
    Done('Generator-timeout-retains-124');
    Commands.Mode='Change'; Commands.Source=Source;
    await Assert.rejects(Module.Checkˉprojectˉconstructionˉreadiness(Configuration()),/Input changed/u);
    Done('Changed-declared-input-refuses-after-child');
    Commands.Mode='Pass'; Requests.Slow=true;
    await Assert.rejects(Module.Checkˉprojectˉconstructionˉreadiness({...Configuration(),Deadline:Date.now()+20}),E=>E.exitCode===124);
    Requests.Slow=false; Done('Slow-request-cannot-renew-deadline');

    const Saved = { argv:process.argv, exit:process.exitCode, stderr:process.stderr.write, stdout:process.stdout.write,
        compiler:process.env.WINDVALE_PREPARED_COMPILER_ONLY, products:process.env.WINDVALE_PREPARED_PRODUCTS_ONLY,
        cache:process.env.WINDVALE_NATIVE_CACHE_ROOT };
    const Cli = await import(pathToFileURL(path.join(Tooling,'Build-Current-Split-Project-Wvb.mjs')).href);
    let Errorˉtext='', Outputˉtext='';
    try {
        delete process.env.WINDVALE_PREPARED_COMPILER_ONLY; delete process.env.WINDVALE_PREPARED_PRODUCTS_ONLY;
        process.env.WINDVALE_NATIVE_CACHE_ROOT=path.join(Work,'Not-Created','Cache');
        process.stderr.write=Value=>{Errorˉtext+=String(Value);return true;};
        process.stdout.write=Value=>{Outputˉtext+=String(Value);return true;};
        async function Cliˉrun(Arguments, Action) {
            Errorˉtext='';Outputˉtext='';process.exitCode=undefined;
            process.argv=[process.execPath,path.join(Tooling,'Build-Current-Split-Project-Wvb.mjs'),...Arguments];
            await Cli.Runˉcurrentˉsplitˉprojectˉcli(Action);
            return { Code:process.exitCode??0,Error:Errorˉtext,Output:Outputˉtext };
        }
        const Base=['--preflight-only','--deadline-ms',String(Date.now()+60_000),Project,path.join(Work,'Output','Output.wvb')];
        Assert.equal((await Cliˉrun(Base)).Code,0); Done('CLI-preflight-only-returns-before-work-key-or-admission');
        Assert.equal((await Cliˉrun([...Base,'--preflight-only'])).Code,64); Done('CLI-duplicate-flag');
        process.env.WINDVALE_PREPARED_COMPILER_ONLY='';
        Assert.equal((await Cliˉrun(Base)).Code,1); delete process.env.WINDVALE_PREPARED_COMPILER_ONLY;
        Done('CLI-invalid-prepared-environment');
        Assert.equal((await Cliˉrun(['--preflight-only','--deadline-ms',String(Date.now()-1),Project,path.join(Work,'Output','Output.wvb')])).Code,124);
        Done('CLI-expired-parent-deadline');
        const Selected=['--compiler-checkpoint','a'.repeat(64),...Base];
        Assert.equal((await Cliˉrun(Selected)).Code,0); Done('CLI-preflight-does-not-admit-selected-key');
        Assert.deepEqual(Compiler,{Key:0,Family:0,Acquire:0,Construct:0,Read:0});
    } finally {
        process.argv=Saved.argv; process.exitCode=Saved.exit; process.stderr.write=Saved.stderr;process.stdout.write=Saved.stdout;
        for(const [Name,Value] of [['WINDVALE_PREPARED_COMPILER_ONLY',Saved.compiler],['WINDVALE_PREPARED_PRODUCTS_ONLY',Saved.products],['WINDVALE_NATIVE_CACHE_ROOT',Saved.cache]]) {
            if(Value===undefined) delete process.env[Name];else process.env[Name]=Value;
        }
    }
    if(Date.now()>Deadline) throw Error('Control deadline exceeded');
    const Readinessˉelapsed = Date.now()-Started;
    const Processˉactions = await Verifyˉconstructionˉprocessˉstatuses(Work, true);
    Assert.equal(Processˉactions,13);
    const Report={status:'Pass',cases:Cases.length,names:Cases,elapsedMilliseconds:Readinessˉelapsed,
        processActions:Processˉactions,packageDeadlineAssertions:12,totalElapsedMilliseconds:Date.now()-Started,
        readinessControlMaximumMilliseconds:30_000,processActionTimeoutMilliseconds:10_000,
        actualModuleSha256:Digest(Buffer.from(Actualˉmodule)),
        filesystemBoundaryAdaptation:'One import specifier; exact body inverse to actual module; modeled full read-only cache',
        actualCliSha256:Digest(await fs.readFile(path.join(Tooling,'Build-Current-Split-Project-Wvb.mjs'))),
        mockedDependencies:['NativeProjectRequest','DevelopmentCommand','CurrentCompiler','SourceEditionPredecessor','ReadOnlyFullCacheFilesystem'],
        actualChecks:['Module code','CLI sequencing','private filesystem write/cleanup','actual Node metadata'],
        constructors:0,compilerExecutions:0,generatorExecutions:0,nativeExecutions:0,qualified:false};
    await fs.writeFile(path.join(Root,'Readiness-Controls.json'),JSON.stringify(Report,null,2)+'\n');
    console.log(JSON.stringify(Report));

    } finally {
        if (path.dirname(path.resolve(Root)) !== Temporary ||
            !path.basename(Root).startsWith('windvale-construction-readiness-')) {
            throw new Error('Readiness control cleanup escaped its allocated parent.');
        }
        await fs.rm(Root, { recursive: true, force: true });
    }

}
