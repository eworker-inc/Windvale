import { createHash, randomBytes } from 'node:crypto';
import { spawn } from 'node:child_process';
import {
    chmod,
    link,
    lstat,
    mkdtemp,
    open,
    realpath,
    rm,
    rmdir,
    unlink,
} from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';

const WINDOWS = process.platform === 'win32';
const TEMPORARY_PREFIX = 'windvale-split-compiler-';
const PUBLICATION_PREFIX = '.new-windvale-split-compiler-';
const MAXIMUM_PRODUCT_COMMAND_BYTES = 134_217_728;
const MAXIMUM_WVB_BYTES = 16_777_216;
const MAXIMUM_PHASE_VALUE_BYTES = 4_194_304;
const MAXIMUM_LOCK_BYTES = 1_048_576;
const MAXIMUM_PROFILE_BYTES = 65_536;
const MAXIMUM_DIAGNOSTIC_BYTES = 65_536;
const MAXIMUM_SOURCE_MODULES = 64;
const WVTD_MINIMUM_BYTES = 64;
const WVTD_MAXIMUM_BYTES = 320;
const WVAE_BYTES = 224;
const WVFC_MINIMUM_BYTES = 48;
const PRODUCER_TIMEOUT_MILLISECONDS = Readˉproducerˉtimeout();
const TEST_HOOKS = Readˉtestˉhooks();
const HEARTBEAT_INTERVAL_MILLISECONDS = 30_000;
const TASKKILL_TIMEOUT_MILLISECONDS = 2_000;
const TERMINATION_SETTLE_MILLISECONDS = 5_000;

if (process.argv.length < 7) Usage();

const Hasˉforeignˉbinder = process.argv[6] === '--foreign-binder';
const Authenticatedˉargumentˉoffset = Hasˉforeignˉbinder ? 8 : 6;
const Authenticated =
    process.argv[Authenticatedˉargumentˉoffset] === '--source-input-lock';
const Admitter = path.resolve(process.argv[2]);
let Validator = null;
let Foreignˉbinder = null;
let Analyzer;
let Emitter;
let Arguments;
if (Authenticated) {
    Validator = path.resolve(process.argv[3]);
    Analyzer = path.resolve(process.argv[4]);
    Emitter = path.resolve(process.argv[5]);
    if (Hasˉforeignˉbinder) {
        if (process.argv.length < 10) Usage();
        Foreignˉbinder = path.resolve(process.argv[7]);
    }
    Arguments = process.argv.slice(Authenticatedˉargumentˉoffset);
} else {
    if (Hasˉforeignˉbinder) Usage();
    Analyzer = path.resolve(process.argv[3]);
    Emitter = path.resolve(process.argv[4]);
    Arguments = process.argv.slice(5);
    if (Arguments[0] === '--source-input-lock') {
        Reject(
            'The targetless Language 1.0 split-compiler route was removed; ' +
            'supply the validator product and exact target descriptor.',
        );
    }
}

const Coordinatorˉstarted = Date.now();
let Activeˉstep = 'input-validation';
const Activityˉenabled =
    process.env.WINDVALE_SPLIT_COMPILER_ACTIVITY !== '0';
const Coordinatorˉheartbeat = Activityˉenabled
    ? setInterval(() => {
        process.stdout.write(
            `INFO  split compiler active step=${Activeˉstep} ` +
            `elapsed-ms=${Date.now() - Coordinatorˉstarted}\n`
        );
    }, HEARTBEAT_INTERVAL_MILLISECONDS)
    : null;
Coordinatorˉheartbeat?.unref();

await Requireˉordinaryˉfile(
    Admitter, 1, MAXIMUM_PRODUCT_COMMAND_BYTES, 'source admission product'
);
if (Validator !== null) {
    await Requireˉordinaryˉfile(
        Validator, 1, MAXIMUM_PRODUCT_COMMAND_BYTES,
        'source admission validator product'
    );
}
if (Foreignˉbinder !== null) {
    await Requireˉordinaryˉfile(
        Foreignˉbinder, 1, MAXIMUM_PRODUCT_COMMAND_BYTES,
        'source foreign binder product'
    );
}
await Requireˉordinaryˉfile(
    Analyzer, 1, MAXIMUM_PRODUCT_COMMAND_BYTES, 'source analyzer product'
);
await Requireˉordinaryˉfile(
    Emitter, 1, MAXIMUM_PRODUCT_COMMAND_BYTES, 'source emitter product'
);

if (Authenticated) {
    if (Arguments.length < 8 ||
        Arguments[0] !== '--source-input-lock' ||
        Arguments[3] !== '--source-profile' ||
        Arguments[5] !== '--target-descriptor') {
        Usage();
    }
} else if (Arguments.length < 2) {
    Usage();
}

const Output = path.resolve(Arguments.at(-1));
if (path.extname(Output).toLowerCase() !== '.wvb' || await Exists(Output)) {
    Reject('The split compiler output must be a new .wvb path.');
}
const Outputˉparent = await Requireˉordinaryˉdirectory(
    path.dirname(Output), 'output parent'
);

let Inputˉsnapshots = null;
if (Authenticated) {
    Reportˉactivity('input-snapshot');
    const Sourceˉpaths = Arguments.slice(7, -1);
    if (Sourceˉpaths.length < 1 || Sourceˉpaths.length > MAXIMUM_SOURCE_MODULES) {
        Reject('The authenticated source closure must contain 1 through 64 modules.');
    }
    if (!/^[0-9a-f]{64}$/u.test(Arguments[2])) {
        Reject('The source-input lock digest must be canonical lowercase SHA-256.');
    }
    const Lock = await Readˉordinaryˉsnapshot(
        Arguments[1], 1, MAXIMUM_LOCK_BYTES, 'source-input lock'
    );
    const Profile = await Readˉordinaryˉsnapshot(
        Arguments[4], 1, MAXIMUM_PROFILE_BYTES, 'source profile'
    );
    const Target = await Readˉordinaryˉsnapshot(
        Arguments[6], WVTD_MINIMUM_BYTES, WVTD_MAXIMUM_BYTES,
        'target descriptor'
    );
    const Sources = [];
    const Sourceˉpayloadˉbudget = MAXIMUM_PHASE_VALUE_BYTES -
        (16 + Sourceˉpaths.length * 8);
    let Sourceˉpayloadˉbytes = 0;
    for (let Index = 0; Index < Sourceˉpaths.length; Index += 1) {
        const Source = await Readˉordinaryˉsnapshot(
            Sourceˉpaths[Index], 1, MAXIMUM_PHASE_VALUE_BYTES,
            `source module ${Index}`
        );
        if (Sourceˉpayloadˉbytes > Sourceˉpayloadˉbudget ||
            Source.bytes.length > Sourceˉpayloadˉbudget - Sourceˉpayloadˉbytes) {
            Reject('The source closure exceeds the 4 MiB canonical WVSS bound.');
        }
        Sourceˉpayloadˉbytes += Source.bytes.length;
        Sources.push(Source);
    }
    const Canonicalˉinputs = [
        Lock.path, Profile.path, Target.path,
        ...Sources.map(Source => Source.path),
    ];
    Requireˉdistinctˉpaths(Canonicalˉinputs, 'authenticated input');
    Requireˉdistinctˉidentities(
        [Lock.identity, Profile.identity, Target.identity,
            ...Sources.map(Source => Source.identity)],
        'authenticated input'
    );
    Inputˉsnapshots = {
        lock: Lock.bytes,
        lockDigest: Arguments[2],
        profile: Profile.bytes,
        sources: Sources.map(Source => Source.bytes),
        target: Target.bytes,
    };
}

class Splitˉcompilerˉfailure extends Error {
    constructor(Status, Diagnostics) {
        super('A split compiler phase rejected its input.');
        this.status = Status === null || Status === 0 ? 1 : Status;
        this.diagnostics = Diagnostics;
    }
}

const Temporaryˉroot = await realpath(os.tmpdir());
const Temporaryˉallocation = await Allocateˉtemporary(Temporaryˉroot);
const Temporary = Temporaryˉallocation.path;
const Temporaryˉidentity = Temporaryˉallocation.identity;

let Failure = null;
let Published = false;
let Publishedˉidentity = null;
let Publicationˉcandidate = null;
let Publicationˉcandidateˉidentity = null;
let Privateˉtreeˉcleanupˉsafe = true;
try {
    const Sourceˉset = path.join(Temporary, 'Admitted.wvss');
    const Analyzedˉsourceˉset = path.join(Temporary, 'Analyzed.wvss');
    const Manifest = path.join(Temporary, 'Manifest.wvca');
    const Bindings = path.join(Temporary, 'Bindings.wvlb');
    const Wir = path.join(Temporary, 'Wir.wvir');
    const Product = path.join(Temporary, 'Product.wvb');
    const Reports = [];

    if (Authenticated) {
        const Inputˉsourceˉset = path.join(Temporary, 'Input.wvss');
        const Lock = path.join(Temporary, 'Source-Inputs.wvlock');
        const Profile = path.join(Temporary, 'Source-Profile.wvsp');
        const Target = path.join(Temporary, 'Input.wvtd');
        const Admittedˉtarget = path.join(Temporary, 'Admitted.wvtd');
        const Catalog = path.join(Temporary, 'Catalog.wvfc');
        const Evidence = path.join(Temporary, 'Evidence.wvae');

        const Inputˉsourceˉbytes = Buildˉwvss1(Inputˉsnapshots.sources);
        await Writeˉprivateˉsnapshot(
            Inputˉsourceˉset, Inputˉsourceˉbytes, Temporary
        );
        Inputˉsnapshots.sources = [];
        const Lockˉidentity = await Writeˉprivateˉsnapshot(
            Lock, Inputˉsnapshots.lock, Temporary
        );
        const Profileˉidentity = await Writeˉprivateˉsnapshot(
            Profile, Inputˉsnapshots.profile, Temporary
        );
        await Writeˉprivateˉsnapshot(Target, Inputˉsnapshots.target, Temporary);
        const Retainedˉinputs = {
            lock: { bytes: Inputˉsnapshots.lock, identity: Lockˉidentity },
            profile: { bytes: Inputˉsnapshots.profile, identity: Profileˉidentity },
        };
        await Requireˉretainedˉsnapshot(
            Lock, Retainedˉinputs.lock, 1, MAXIMUM_LOCK_BYTES,
            'source-input lock'
        );
        await Requireˉretainedˉsnapshot(
            Profile, Retainedˉinputs.profile, 1, MAXIMUM_PROFILE_BYTES,
            'source profile'
        );

        Reports.push(await Runˉrequired(
            Admitter,
            [
                '--source-input-lock', Lock, Inputˉsnapshots.lockDigest,
                '--source-profile', Profile,
                '--target-descriptor', Target,
                '--source-set', Inputˉsourceˉset,
                Sourceˉset, Admittedˉtarget, Catalog, Evidence,
            ],
            'source-admission',
        ));
        await Requireˉprivateˉphaseˉfile(
            Sourceˉset, 37, MAXIMUM_PHASE_VALUE_BYTES,
            Temporary, 'admitted source set'
        );
        await Requireˉprivateˉphaseˉfile(
            Admittedˉtarget, WVTD_MINIMUM_BYTES, WVTD_MAXIMUM_BYTES,
            Temporary, 'admitted target descriptor'
        );
        await Requireˉprivateˉphaseˉfile(
            Catalog, WVFC_MINIMUM_BYTES, MAXIMUM_PHASE_VALUE_BYTES,
            Temporary, 'foreign catalog'
        );
        await Requireˉprivateˉphaseˉfile(
            Evidence, WVAE_BYTES, WVAE_BYTES,
            Temporary, 'admission evidence'
        );
        const Retained = {
            catalog: await Readˉordinaryˉsnapshot(
                Catalog, WVFC_MINIMUM_BYTES, MAXIMUM_PHASE_VALUE_BYTES,
                'foreign catalog'
            ),
            evidence: await Readˉordinaryˉsnapshot(
                Evidence, WVAE_BYTES, WVAE_BYTES, 'admission evidence'
            ),
            sourceSet: await Readˉordinaryˉsnapshot(
                Sourceˉset, 37, MAXIMUM_PHASE_VALUE_BYTES,
                'admitted source set'
            ),
            target: await Readˉordinaryˉsnapshot(
                Admittedˉtarget, WVTD_MINIMUM_BYTES, WVTD_MAXIMUM_BYTES,
                'admitted target descriptor'
            ),
        };
        if (!Retained.target.bytes.equals(Inputˉsnapshots.target)) {
            Reject('The admitted target descriptor differs from its private input snapshot.');
        }
        await Requireˉretainedˉsnapshot(
            Lock, Retainedˉinputs.lock, 1, MAXIMUM_LOCK_BYTES,
            'source-input lock'
        );
        await Requireˉretainedˉsnapshot(
            Profile, Retainedˉinputs.profile, 1, MAXIMUM_PROFILE_BYTES,
            'source profile'
        );

        Reports.push(await Runˉrequired(
            Validator,
            [Evidence, Sourceˉset, Admittedˉtarget, Catalog, Lock, Profile],
            'source-authentication',
        ));
        await Requireˉretainedˉsnapshot(
            Evidence, Retained.evidence, WVAE_BYTES, WVAE_BYTES,
            'admission evidence'
        );
        await Requireˉretainedˉsnapshot(
            Sourceˉset, Retained.sourceSet, 37, MAXIMUM_PHASE_VALUE_BYTES,
            'admitted source set'
        );
        await Requireˉretainedˉsnapshot(
            Admittedˉtarget, Retained.target,
            WVTD_MINIMUM_BYTES, WVTD_MAXIMUM_BYTES,
            'admitted target descriptor'
        );
        await Requireˉretainedˉsnapshot(
            Catalog, Retained.catalog,
            WVFC_MINIMUM_BYTES, MAXIMUM_PHASE_VALUE_BYTES,
            'foreign catalog'
        );
        await Requireˉretainedˉsnapshot(
            Lock, Retainedˉinputs.lock, 1, MAXIMUM_LOCK_BYTES,
            'source-input lock'
        );
        await Requireˉretainedˉsnapshot(
            Profile, Retainedˉinputs.profile, 1, MAXIMUM_PROFILE_BYTES,
            'source profile'
        );
        if (Retained.catalog.bytes.readUInt32LE(12) !== 0) {
            if (Foreignˉbinder === null) {
                Reject(
                    'The authenticated foreign catalog requires ' +
                    '--foreign-binder <wvbind>.'
                );
            }
            const Bindingˉevidence = await Runˉrequired(
                Foreignˉbinder,
                [Sourceˉset, Admittedˉtarget, Catalog],
                'source-foreign-binding',
            );
            const Expectedˉbindingˉevidence =
                Buildˉforeignˉbindingˉevidence(Retained);
            if (!Bindingˉevidence.equals(Expectedˉbindingˉevidence)) {
                Reject(
                    'The foreign-binder evidence does not exactly match the ' +
                    'retained authenticated inputs.'
                );
            }
            await Requireˉretainedˉsnapshot(
                Evidence, Retained.evidence, WVAE_BYTES, WVAE_BYTES,
                'admission evidence'
            );
            await Requireˉretainedˉsnapshot(
                Sourceˉset, Retained.sourceSet, 37,
                MAXIMUM_PHASE_VALUE_BYTES, 'admitted source set'
            );
            await Requireˉretainedˉsnapshot(
                Admittedˉtarget, Retained.target,
                WVTD_MINIMUM_BYTES, WVTD_MAXIMUM_BYTES,
                'admitted target descriptor'
            );
            await Requireˉretainedˉsnapshot(
                Catalog, Retained.catalog,
                WVFC_MINIMUM_BYTES, MAXIMUM_PHASE_VALUE_BYTES,
                'foreign catalog'
            );
            await Requireˉretainedˉsnapshot(
                Lock, Retainedˉinputs.lock, 1, MAXIMUM_LOCK_BYTES,
                'source-input lock'
            );
            await Requireˉretainedˉsnapshot(
                Profile, Retainedˉinputs.profile, 1, MAXIMUM_PROFILE_BYTES,
                'source profile'
            );
            throw new Splitˉcompilerˉfailure(
                1,
                Buffer.from('source analysis status=Foreignˉloweringˉpending\n'),
            );
        }
        Reports.push(await Runˉrequired(
            Analyzer,
            [
                '--internal-source-set', Sourceˉset, Analyzedˉsourceˉset,
                Manifest, Bindings, Wir,
            ],
            'source-analysis',
        ));
        await Requireˉprivateˉphaseˉfile(
            Analyzedˉsourceˉset, 37, MAXIMUM_PHASE_VALUE_BYTES,
            Temporary, 'analyzed source set'
        );
        await Requireˉretainedˉsnapshot(
            Sourceˉset, Retained.sourceSet, 37, MAXIMUM_PHASE_VALUE_BYTES,
            'admitted source set'
        );
        const Analyzedˉsnapshot = await Readˉordinaryˉsnapshot(
            Analyzedˉsourceˉset, 37, MAXIMUM_PHASE_VALUE_BYTES,
            'analyzed source set'
        );
        if (!Analyzedˉsnapshot.bytes.equals(Retained.sourceSet.bytes)) {
            Reject('The Analyzer republished a different admitted source set.');
        }
    } else {
        Reports.push(await Runˉrequired(
            Analyzer,
            [...Arguments.slice(0, -1), Sourceˉset, Manifest, Bindings, Wir],
            'project-2-source-analysis',
        ));
        await Requireˉprivateˉphaseˉfile(
            Sourceˉset, 25, MAXIMUM_PHASE_VALUE_BYTES,
            Temporary, 'project 2 source set'
        );
    }

    await Requireˉprivateˉphaseˉfile(
        Manifest, 104, 104, Temporary, 'source analysis manifest'
    );
    await Requireˉprivateˉphaseˉfile(
        Bindings, 1, MAXIMUM_PHASE_VALUE_BYTES,
        Temporary, 'source binding evidence'
    );
    await Requireˉprivateˉphaseˉfile(
        Wir, 1, MAXIMUM_PHASE_VALUE_BYTES, Temporary, 'source WIR'
    );
    Reports.push(await Runˉrequired(
        Emitter, [Sourceˉset, Manifest, Bindings, Wir, Product], 'source-emission'
    ));
    await Requireˉprivateˉphaseˉfile(
        Product, 1, MAXIMUM_WVB_BYTES, Temporary, 'split compiler product'
    );

    Publicationˉcandidate = path.join(
        Outputˉparent,
        `${PUBLICATION_PREFIX}${process.pid}-` +
        randomBytes(12).toString('hex'),
    );
    Reportˉactivity('publication');
    const Productˉsnapshot = await Readˉordinaryˉsnapshot(
        Product, 1, MAXIMUM_WVB_BYTES, 'split compiler product'
    );
    Publicationˉcandidateˉidentity =
        await Writeˉpublicationˉcandidate(
            Publicationˉcandidate, Productˉsnapshot.bytes,
            Outputˉparent
        );
    await Requireˉordinaryˉfile(
        Publicationˉcandidate, 1, MAXIMUM_WVB_BYTES,
        'split compiler publication candidate'
    );
    await Requireˉidentity(
        Publicationˉcandidate, Publicationˉcandidateˉidentity,
        'split compiler publication candidate'
    );
    await link(Publicationˉcandidate, Output);
    Published = true;
    Publishedˉidentity = Publicationˉcandidateˉidentity;
    await Requireˉordinaryˉfile(
        Output, 1, MAXIMUM_WVB_BYTES, 'published split compiler product'
    );
    await Requireˉidentity(
        Output, Publishedˉidentity, 'published split compiler product'
    );
    await Applyˉpostˉlinkˉcandidateˉtestˉhook(Publicationˉcandidate);
    await Removeˉpublicationˉcandidate(
        Publicationˉcandidate, Outputˉparent,
        Publicationˉcandidateˉidentity
    );
    Publicationˉcandidate = null;
    Publicationˉcandidateˉidentity = null;
    await Applyˉpostˉpublicationˉtestˉhook(
        Output, TEST_HOOKS.postPublicationCleanup
    );
    for (const Report of Reports) process.stdout.write(Report);
} catch (Error) {
    Failure = Error;
} finally {
    Reportˉactivity('cleanup');
    const Cleanupˉerrors = [];
    if (Failure !== null && Published) {
        await Captureˉcleanupˉerror(Cleanupˉerrors, async () => {
            await Removeˉpublishedˉoutput(Output, Publishedˉidentity);
        });
    }
    if (Publicationˉcandidate !== null) {
        await Captureˉcleanupˉerror(Cleanupˉerrors, async () => {
            if (await Exists(Publicationˉcandidate)) {
                await Removeˉpublicationˉcandidate(
                    Publicationˉcandidate, Outputˉparent,
                    Publicationˉcandidateˉidentity
                );
            }
        });
    }
    if (Privateˉtreeˉcleanupˉsafe) {
        await Captureˉcleanupˉerror(Cleanupˉerrors, async () => {
            await Removeˉtemporary(
                Temporary, Temporaryˉroot, Temporaryˉidentity
            );
            if (TEST_HOOKS.cleanupFailureAfterRemoval) {
                Reject('forced cleanup failure after private-directory removal');
            }
        });
    }
    if (Cleanupˉerrors.length !== 0) {
        if (Failure === null && Published) {
            await Captureˉcleanupˉerror(Cleanupˉerrors, async () => {
                await Removeˉpublishedˉoutput(Output, Publishedˉidentity);
            });
        }
        const Detail = Cleanupˉerrors.join('; ').slice(
            0, MAXIMUM_DIAGNOSTIC_BYTES - 64
        );
        Failure = Appendˉcleanupˉfailure(Failure, Detail);
    }
}

if (Coordinatorˉheartbeat !== null) clearInterval(Coordinatorˉheartbeat);

if (Failure !== null) {
    if (Failure instanceof Splitˉcompilerˉfailure) {
        if (Failure.diagnostics.length !== 0) {
            process.stderr.write(Failure.diagnostics);
        }
        process.exitCode = Failure.status;
    } else {
        throw Failure;
    }
}

function Buildˉwvss1(Sources) {
    const Headerˉbytes = 16 + Sources.length * 8;
    let Payloadˉbytes = 0;
    for (const Source of Sources) {
        if (Source.length < 1 ||
            Payloadˉbytes > MAXIMUM_PHASE_VALUE_BYTES - Headerˉbytes ||
            Source.length >
                MAXIMUM_PHASE_VALUE_BYTES - Headerˉbytes - Payloadˉbytes) {
            Reject('The private source closure exceeds the 4 MiB WVSS bound.');
        }
        Payloadˉbytes += Source.length;
    }
    const Result = Buffer.alloc(Headerˉbytes + Payloadˉbytes);
    Result.write('WVSS', 0, 4, 'ascii');
    Result.writeUInt16LE(1, 4);
    Result.writeUInt16LE(0, 6);
    Result.writeUInt32LE(Sources.length, 8);
    Result.writeUInt32LE(Sources.length * 8, 12);
    let Payloadˉoffset = Headerˉbytes;
    for (let Index = 0; Index < Sources.length; Index += 1) {
        Result.writeUInt32LE(Payloadˉoffset, 16 + Index * 8);
        Result.writeUInt32LE(Sources[Index].length, 20 + Index * 8);
        Sources[Index].copy(Result, Payloadˉoffset);
        Payloadˉoffset += Sources[Index].length;
    }
    return Result;
}

function Buildˉforeignˉbindingˉevidence(Retained) {
    const Digest = Value =>
        createHash('sha256').update(Value).digest('hex');
    return Buffer.from(
        'foreign binding status=Published ' +
        `source-bytes=${Retained.sourceSet.bytes.length} ` +
        `source-sha256=${Digest(Retained.sourceSet.bytes)} ` +
        `target-bytes=${Retained.target.bytes.length} ` +
        `target-sha256=${Digest(Retained.target.bytes)} ` +
        `catalog-bytes=${Retained.catalog.bytes.length} ` +
        `catalog-sha256=${Digest(Retained.catalog.bytes)} ` +
        `foreign-count=${Retained.catalog.bytes.readUInt32LE(12)}\n`,
        'utf8'
    );
}

async function Runˉrequired(Command, Commandˉarguments, Step) {
    const Result = await Runˉbounded(Command, Commandˉarguments, Step);
    if (Result.cleanupFailure !== null) {
        Privateˉtreeˉcleanupˉsafe = false;
        throw new Splitˉcompilerˉfailure(
            1,
            Buffer.from(
                `split compiler status=Terminationˉfailure step=${Step} ` +
                `detail=${Result.cleanupFailure}\n`
            ),
        );
    }
    if (Result.timedOut) {
        throw new Splitˉcompilerˉfailure(
            1, Buffer.from(`split compiler status=Timeout step=${Step}\n`)
        );
    }
    if (Result.exceeded) {
        throw new Splitˉcompilerˉfailure(
            1, Buffer.from(`split compiler status=Outputˉlimit step=${Step}\n`)
        );
    }
    if (Result.status !== 0) {
        const Diagnostics = Result.stderr.length === 0
            ? Buffer.from(
                `split compiler status=Phaseˉrejected step=${Step} ` +
                `exit=${Result.status ?? 1}\n`
            )
            : Result.stderr;
        throw new Splitˉcompilerˉfailure(Result.status, Diagnostics);
    }
    if (Result.stderr.length !== 0 || Result.stdout.length === 0) {
        Reject(`The ${Step} produced invalid success diagnostics.`);
    }
    return Result.stdout;
}

function Runˉbounded(Command, Commandˉarguments, Step) {
    return new Promise((Resolve, Rejectˉpromise) => {
        Reportˉactivity(Step);
        const Isˉcommand = WINDOWS && Command.toLowerCase().endsWith('.cmd');
        if (Isˉcommand && [Command, ...Commandˉarguments].some(
            Argument => /[\r\n&|<>^%!"]/u.test(Argument)
        )) {
            Rejectˉpromise(new Error(
                'A split compiler command contains shell metacharacters.'
            ));
            return;
        }
        const Executable = Isˉcommand
            ? process.env.ComSpec ?? 'cmd.exe'
            : Command;
        const Spawnˉarguments = Isˉcommand
            ? ['/d', '/v:off', '/s', '/c', `"${[Command, ...Commandˉarguments]
                .map(Argument => `"${Argument}"`).join(' ')}"`]
            : Commandˉarguments;
        const Child = spawn(Executable, Spawnˉarguments, {
            detached: !WINDOWS,
            windowsHide: true,
            windowsVerbatimArguments: Isˉcommand,
            stdio: ['ignore', 'pipe', 'pipe'],
        });
        const Stdout = [];
        const Stderr = [];
        let Capturedˉbytes = 0;
        let Exceeded = false;
        let Timedˉout = false;
        let Settled = false;
        let Closeˉreceived = false;
        let Closeˉstatus = null;
        let Terminationˉpromise = null;
        let Cleanupˉfailure = null;
        let Settleˉtimer;

        const Result = Forced => ({
            cleanupFailure: Cleanupˉfailure,
            exceeded: Exceeded,
            forced: Forced,
            status: Closeˉstatus,
            stderr: Buffer.concat(Stderr),
            stdout: Buffer.concat(Stdout),
            timedOut: Timedˉout,
        });
        const Complete = Forced => {
            if (Settled) return;
            Settled = true;
            clearTimeout(Timer);
            if (Settleˉtimer !== undefined) clearTimeout(Settleˉtimer);
            Resolve(Result(Forced));
        };
        const Terminateˉandˉsettle = () => {
            if (Terminationˉpromise !== null) return;
            Settleˉtimer = setTimeout(() => {
                try {
                    Child.kill('SIGKILL');
                } catch {
                    // The bounded termination result below remains authoritative.
                }
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
                if (TEST_HOOKS.terminationSettleFailure) {
                    // Withhold confirmation to exercise the bounded forced-settle path.
                    await new Promise(() => {});
                }
                if (Closeˉreceived) Complete(false);
            })().catch(Error => {
                Cleanupˉfailure = `tree termination error: ${Error.message}`;
                if (Closeˉreceived) Complete(false);
            });
        };
        const Append = (Destination, Chunk) => {
            Capturedˉbytes += Chunk.length;
            if (Capturedˉbytes <= MAXIMUM_DIAGNOSTIC_BYTES) {
                Destination.push(Chunk);
            }
            else if (!Exceeded) {
                Exceeded = true;
                Terminateˉandˉsettle();
            }
        };
        const Timer = setTimeout(() => {
            Timedˉout = true;
            Terminateˉandˉsettle();
        }, PRODUCER_TIMEOUT_MILLISECONDS);
        Child.stdout.on('data', Chunk => {
            Append(Stdout, Chunk);
        });
        Child.stderr.on('data', Chunk => {
            Append(Stderr, Chunk);
        });
        Child.once('error', Error => {
            if (Timedˉout || Exceeded || Settled) return;
            clearTimeout(Timer);
            Rejectˉpromise(Error);
        });
        Child.once('close', Status => {
            Closeˉreceived = true;
            Closeˉstatus = Status;
            if (Terminationˉpromise === null) Complete(false);
            else void Terminationˉpromise.then(() => Complete(false));
        });
    });
}

function Reportˉactivity(Step) {
    Activeˉstep = Step;
    if (!Activityˉenabled) return;
    process.stdout.write(
        `INFO  split compiler active step=${Step} ` +
        `elapsed-ms=${Date.now() - Coordinatorˉstarted}\n`
    );
}

function Readˉproducerˉtimeout() {
    const Override = process.env.WINDVALE_SPLIT_COMPILER_TEST_TIMEOUT_MILLISECONDS;
    if (Override === undefined) return 300_000;
    if (!/^[1-9][0-9]{0,5}$/u.test(Override)) {
        Reject('The split compiler test timeout is invalid.');
    }
    const Value = Number.parseInt(Override, 10);
    if (Value > 300_000) {
        Reject('The split compiler test timeout exceeds the production ceiling.');
    }
    return Value;
}

function Readˉtestˉhooks() {
    const Key = Symbol.for('windvale.split-compiler.test-hooks.v1');
    const Hooks = globalThis[Key];
    delete globalThis[Key];
    if (Hooks === undefined) {
        return Object.freeze({
            candidateFailure: null,
            cleanupFailureAfterRemoval: false,
            postLinkCandidateRemoval: false,
            postPublicationCleanup: null,
            temporaryIdentityFailure: false,
            terminationSettleFailure: false,
        });
    }
    if (Hooks === null || typeof Hooks !== 'object' ||
        !Object.isFrozen(Hooks) ||
        !['before-identity', 'after-write', 'after-sync', null].includes(
            Hooks.candidateFailure
        ) ||
        typeof Hooks.cleanupFailureAfterRemoval !== 'boolean' ||
        typeof Hooks.postLinkCandidateRemoval !== 'boolean' ||
        typeof Hooks.temporaryIdentityFailure !== 'boolean' ||
        typeof Hooks.terminationSettleFailure !== 'boolean' ||
        !['original', 'replacement', null].includes(
            Hooks.postPublicationCleanup
        )) {
        Reject('The explicitly imported split-compiler test hooks are invalid.');
    }
    return Object.freeze({
        candidateFailure: Hooks.candidateFailure,
        cleanupFailureAfterRemoval: Hooks.cleanupFailureAfterRemoval,
        postLinkCandidateRemoval: Hooks.postLinkCandidateRemoval,
        postPublicationCleanup: Hooks.postPublicationCleanup,
        temporaryIdentityFailure: Hooks.temporaryIdentityFailure,
        terminationSettleFailure: Hooks.terminationSettleFailure,
    });
}

async function Applyˉpostˉlinkˉcandidateˉtestˉhook(Candidate) {
    if (TEST_HOOKS.postLinkCandidateRemoval) {
        await unlink(Candidate);
    }
}

async function Applyˉpostˉpublicationˉtestˉhook(Output, Mode) {
    if (Mode !== 'replacement') return;
    await unlink(Output);
    const Handle = await open(Output, 'wx', 0o600);
    try {
        await Handle.writeFile(Buffer.from('attacker-replacement'));
        await Handle.sync();
        await Handle.chmod(0o400);
    } finally {
        await Handle.close();
    }
}

function Applyˉcandidateˉtestˉhook(Point) {
    if (TEST_HOOKS.candidateFailure === Point) {
        Reject(`forced publication-candidate ${Point} failure`);
    }
}

function Processˉisˉlive(Child) {
    return Child.pid !== undefined &&
        Child.exitCode === null && Child.signalCode === null;
}

async function Terminateˉprocessˉtree(Child) {
    if (Child.pid === undefined) return null;
    let Diagnostic = null;
    if (WINDOWS) {
        Diagnostic = await Runˉboundedˉtaskkill(Child.pid);
        if (!Processˉisˉlive(Child) &&
            Diagnostic !== null &&
            Diagnostic.startsWith('taskkill exited ')) {
            Diagnostic = null;
        }
    }
    else {
        try {
            process.kill(-Child.pid, 'SIGKILL');
        } catch (Error) {
            if (Error.code !== 'ESRCH') {
                Diagnostic = `process-group kill error: ${Error.message}`;
            }
        }
    }
    if (Processˉisˉlive(Child)) {
        try {
            Child.kill('SIGKILL');
        } catch (Error) {
            Diagnostic = Diagnostic === null
                ? `direct child kill error: ${Error.message}`
                : `${Diagnostic}; direct child kill error: ${Error.message}`;
        }
    }
    return Diagnostic;
}

function Runˉboundedˉtaskkill(Processˉidentifier) {
    return new Promise(Resolve => {
        const Killer = spawn(
            'taskkill.exe',
            ['/pid', String(Processˉidentifier), '/t', '/f'],
            { stdio: 'ignore', windowsHide: true },
        );
        let Settled = false;
        const Timer = setTimeout(() => {
            if (Settled) return;
            Settled = true;
            Killer.kill('SIGKILL');
            Killer.unref();
            Resolve('taskkill did not settle within 2000 ms');
        }, TASKKILL_TIMEOUT_MILLISECONDS);
        Killer.once('error', Error => {
            if (Settled) return;
            Settled = true;
            clearTimeout(Timer);
            Resolve(`taskkill error: ${Error.message}`);
        });
        Killer.once('close', Code => {
            if (Settled) return;
            Settled = true;
            clearTimeout(Timer);
            Resolve(Code === 0 ? null : `taskkill exited ${Code}`);
        });
    });
}

async function Readˉordinaryˉsnapshot(Candidate, Minimum, Maximum, Label) {
    const Resolved = path.resolve(Candidate);
    const Canonical = await Requireˉordinaryˉfile(
        Resolved, Minimum, Maximum, Label
    );
    const Handle = await open(Canonical, 'r');
    let Before;
    let After;
    let Bytes;
    try {
        Before = await Handle.stat({ bigint: true });
        if (!Before.isFile() || Before.size < BigInt(Minimum) ||
            Before.size > BigInt(Maximum)) {
            Reject(`The ${Label} changed outside its accepted byte bounds before read.`);
        }
        Bytes = await Handle.readFile();
        After = await Handle.stat({ bigint: true });
    } finally {
        await Handle.close();
    }
    if (!Sameˉidentity(Before, After) || Before.size !== After.size ||
        BigInt(Bytes.length) !== After.size) {
        Reject(`The ${Label} changed while its snapshot was read.`);
    }
    return { bytes: Bytes, identity: After, path: Canonical };
}

async function Requireˉretainedˉsnapshot(
    Candidate, Retained, Minimum, Maximum, Label
) {
    const Current = await Readˉordinaryˉsnapshot(
        Candidate, Minimum, Maximum, Label
    );
    if (!Sameˉidentity(Current.identity, Retained.identity) ||
        !Current.bytes.equals(Retained.bytes)) {
        Reject(`The retained ${Label} changed between compiler phases.`);
    }
}

async function Requireˉordinaryˉfile(Candidate, Minimum, Maximum, Label) {
    const Information = await lstat(Candidate).catch(() => null);
    if (Information === null || !Information.isFile() ||
        Information.isSymbolicLink() || Information.size < Minimum ||
        Information.size > Maximum) {
        Reject(`The ${Label} is not a bounded ordinary file: ${Candidate}`);
    }
    const Canonical = await realpath(Candidate);
    if (!Pathsˉequal(Canonical, path.resolve(Candidate))) {
        Reject(`The ${Label} path is not canonical: ${Candidate}`);
    }
    return Canonical;
}

async function Requireˉordinaryˉdirectory(Candidate, Label) {
    const Resolved = path.resolve(Candidate);
    const Information = await lstat(Resolved).catch(() => null);
    if (Information === null || !Information.isDirectory() ||
        Information.isSymbolicLink()) {
        Reject(`The ${Label} is not an ordinary directory: ${Candidate}`);
    }
    const Canonical = await realpath(Resolved);
    if (!Pathsˉequal(Canonical, Resolved)) {
        Reject(`The ${Label} path is not canonical: ${Candidate}`);
    }
    return Canonical;
}

async function Requireˉprivateˉphaseˉfile(
    Candidate, Minimum, Maximum, Temporaryˉdirectory, Label
) {
    const Resolved = path.resolve(Candidate);
    if (!Pathsˉequal(path.dirname(Resolved), Temporaryˉdirectory)) {
        Reject(`The ${Label} escaped the private phase directory.`);
    }
    await Requireˉordinaryˉfile(Resolved, Minimum, Maximum, Label);
    const Information = await lstat(Resolved);
    if (Information.nlink !== 1) {
        Reject(`The ${Label} is not privately owned by the phase directory.`);
    }
    await chmod(Resolved, 0o400);
}

async function Writeˉprivateˉsnapshot(Candidate, Bytes, Temporaryˉdirectory) {
    const Resolved = path.resolve(Candidate);
    if (!Pathsˉequal(path.dirname(Resolved), Temporaryˉdirectory)) {
        Reject('A private snapshot escaped the phase directory.');
    }
    const Handle = await open(Resolved, 'wx', 0o600);
    let Identity;
    try {
        await Handle.writeFile(Bytes);
        await Handle.sync();
        await Handle.chmod(0o400);
        Identity = await Handle.stat({ bigint: true });
    } finally {
        await Handle.close();
    }
    return Identity;
}

async function Writeˉpublicationˉcandidate(
    Candidate, Bytes, Expectedˉparent
) {
    const Resolved = path.resolve(Candidate);
    if (!Pathsˉequal(path.dirname(Resolved), Expectedˉparent) ||
        !path.basename(Resolved).startsWith(PUBLICATION_PREFIX)) {
        Reject('A publication candidate escaped its exact output directory.');
    }
    const Handle = await open(Resolved, 'wx', 0o600);
    let Identity = null;
    let Failure = null;
    const Cleanupˉerrors = [];
    try {
        Applyˉcandidateˉtestˉhook('before-identity');
        Identity = await Handle.stat({ bigint: true });
        if (!Identity.isFile() || Identity.isSymbolicLink() ||
            Identity.nlink !== 1n || Identity.size !== 0n) {
            Reject('The allocated publication candidate is not a private empty file.');
        }
        await Handle.writeFile(Bytes);
        Applyˉcandidateˉtestˉhook('after-write');
        await Handle.sync();
        Applyˉcandidateˉtestˉhook('after-sync');
        await Handle.chmod(0o400);
        const Finalˉidentity = await Handle.stat({ bigint: true });
        if (!Sameˉidentity(Finalˉidentity, Identity) ||
            Finalˉidentity.size !== BigInt(Bytes.length)) {
            Reject('The publication candidate changed while it was populated.');
        }
    } catch (Error) {
        Failure = Error;
        if (Identity === null) {
            try {
                const Retriedˉidentity = await Handle.stat({ bigint: true });
                if (!Retriedˉidentity.isFile() ||
                    Retriedˉidentity.isSymbolicLink() ||
                    Retriedˉidentity.nlink !== 1n ||
                    Retriedˉidentity.size !== 0n) {
                    Reject(
                        'The publication candidate identity retry did not ' +
                        'identify a private file.'
                    );
                }
                Identity = Retriedˉidentity;
            } catch (Identityˉerror) {
                Cleanupˉerrors.push(
                    'publication-candidate identity retry: ' +
                    (Identityˉerror instanceof Error
                        ? Identityˉerror.message
                        : String(Identityˉerror))
                );
            }
        }
    } finally {
        try {
            await Handle.close();
        } catch (Error) {
            Cleanupˉerrors.push(`publication-candidate close: ${Error.message}`);
        }
    }
    if (Failure !== null || Cleanupˉerrors.length !== 0) {
        if (Identity !== null) {
            await Captureˉcleanupˉerror(Cleanupˉerrors, async () => {
                await Removeˉpublicationˉcandidate(
                    Resolved, Expectedˉparent, Identity
                );
            });
        }
        if (Cleanupˉerrors.length !== 0) {
            Failure = Appendˉcleanupˉfailure(
                Failure, Cleanupˉerrors.join('; ')
            );
        }
        throw Failure;
    }
    return Identity;
}

async function Allocateˉtemporary(Expectedˉroot) {
    const Candidate = await mkdtemp(
        path.join(Expectedˉroot, TEMPORARY_PREFIX)
    );
    const Resolved = path.resolve(Candidate);
    let Identity = null;
    let Failure = null;
    try {
        if (!Pathsˉequal(path.dirname(Resolved), Expectedˉroot) ||
            !path.basename(Resolved).startsWith(TEMPORARY_PREFIX)) {
            Reject('The allocated split-compiler directory escaped its root.');
        }
        if (TEST_HOOKS.temporaryIdentityFailure) {
            Reject('forced temporary-allocation identity failure');
        }
        Identity = await lstat(Resolved, { bigint: true });
        if (!Identity.isDirectory() || Identity.isSymbolicLink()) {
            Reject('The allocated split-compiler path is not an ordinary directory.');
        }
        const Canonical = await realpath(Resolved);
        const Confirmed = await lstat(Canonical, { bigint: true });
        if (!Pathsˉequal(Canonical, Resolved) ||
            !Sameˉidentity(Confirmed, Identity)) {
            Reject('The allocated split-compiler directory is not canonical.');
        }
        return Object.freeze({ path: Canonical, identity: Identity });
    } catch (Error) {
        Failure = Error;
    }
    const Cleanupˉerrors = [];
    await Captureˉcleanupˉerror(Cleanupˉerrors, async () => {
        await Removeˉemptyˉallocatedˉdirectory(
            Resolved, Expectedˉroot, Identity
        );
    });
    if (Cleanupˉerrors.length !== 0) {
        Failure = Appendˉcleanupˉfailure(
            Failure, Cleanupˉerrors.join('; ')
        );
    }
    throw Failure;
}

async function Removeˉemptyˉallocatedˉdirectory(
    Candidate, Expectedˉroot, Expectedˉidentity
) {
    const Resolved = path.resolve(Candidate);
    if (!Pathsˉequal(path.dirname(Resolved), Expectedˉroot) ||
        !path.basename(Resolved).startsWith(TEMPORARY_PREFIX)) {
        Reject('Refusing to remove an unexpected split-compiler allocation.');
    }
    if (Expectedˉidentity === null) {
        try {
            await rmdir(Resolved);
        } catch (Error) {
            if (Error?.code !== 'ENOENT') throw Error;
        }
        return;
    }
    const Information = await Lstatˉorˉabsent(Resolved, { bigint: true });
    if (Information === null) return;
    const Canonical = await realpath(Resolved);
    if (!Information.isDirectory() || Information.isSymbolicLink() ||
        !Sameˉidentity(Information, Expectedˉidentity) ||
        !Pathsˉequal(Canonical, Resolved)) {
        Reject('Refusing to remove a replaced split-compiler allocation.');
    }
    await rmdir(Resolved);
}

async function Removeˉtemporary(Candidate, Expectedˉroot, Expectedˉidentity) {
    const Information = await Lstatˉorˉabsent(Candidate, { bigint: true });
    if (Information === null) return;
    const Canonical = await realpath(Candidate);
    if (!Information.isDirectory() || Information.isSymbolicLink() ||
        !Pathsˉequal(Canonical, path.resolve(Candidate)) ||
        !Pathsˉequal(path.dirname(Canonical), Expectedˉroot) ||
        !path.basename(Canonical).startsWith(TEMPORARY_PREFIX) ||
        !Sameˉidentity(Information, Expectedˉidentity)) {
        Reject(
            `Refusing to remove an unexpected split compiler directory: ${Candidate}`
        );
    }
    await rm(Canonical, { recursive: true, force: true, maxRetries: 2 });
}

async function Removeˉpublicationˉcandidate(
    Candidate, Expectedˉparent, Expectedˉidentity
) {
    const Resolved = path.resolve(Candidate);
    const Information = await Lstatˉorˉabsent(Resolved, { bigint: true });
    if (Information === null) return;
    if (!Information.isFile() || Information.isSymbolicLink() ||
        !Pathsˉequal(path.dirname(Resolved), Expectedˉparent) ||
        !path.basename(Resolved).startsWith(PUBLICATION_PREFIX) ||
        !Sameˉidentity(Information, Expectedˉidentity)) {
        Reject(`Refusing to remove an unexpected publication candidate: ${Candidate}`);
    }
    await unlink(Resolved);
}

async function Removeˉpublishedˉoutput(Candidate, Expectedˉidentity) {
    const Information = await Lstatˉorˉabsent(Candidate, { bigint: true });
    if (Information === null) return;
    if (!Information.isFile() || Information.isSymbolicLink() ||
        !Sameˉidentity(Information, Expectedˉidentity)) {
        return;
    }
    await unlink(Candidate);
}

async function Captureˉcleanupˉerror(Errors, Action) {
    try {
        await Action();
    } catch (Cleanupˉerror) {
        Errors.push(
            Cleanupˉerror instanceof Error
                ? Cleanupˉerror.message
                : String(Cleanupˉerror)
        );
    }
}

async function Lstatˉorˉabsent(Candidate, Options = undefined) {
    try {
        return await lstat(Candidate, Options);
    } catch (Error) {
        if (Error?.code === 'ENOENT') return null;
        throw Error;
    }
}

function Appendˉcleanupˉfailure(Failure, Detail) {
    if (Failure === null) {
        const Diagnostic = Cleanupˉdiagnostic(
            Detail, MAXIMUM_DIAGNOSTIC_BYTES
        );
        return new Splitˉcompilerˉfailure(1, Diagnostic);
    }
    if (Failure instanceof Splitˉcompilerˉfailure) {
        const Remaining = Math.max(
            0, MAXIMUM_DIAGNOSTIC_BYTES - Failure.diagnostics.length
        );
        const Diagnostic = Cleanupˉdiagnostic(Detail, Remaining);
        return new Splitˉcompilerˉfailure(
            Failure.status,
            Buffer.concat([Failure.diagnostics, Diagnostic]),
        );
    }
    const Diagnostic = Cleanupˉdiagnostic(
        Detail, MAXIMUM_DIAGNOSTIC_BYTES
    );
    const Primaryˉmessage = Failure instanceof Error
        ? Failure.message
        : `Non-Error primary failure: ${String(Failure)}`;
    const Primaryˉstack = Failure instanceof Error
        ? Failure.stack ?? Failure.message
        : Primaryˉmessage;
    const Combined = new Error(
        `${Primaryˉmessage}\n${Diagnostic.toString('utf8').trimEnd()}`,
        { cause: Failure },
    );
    Combined.stack = `${Primaryˉstack}\n` +
        Diagnostic.toString('utf8').trimEnd();
    return Combined;
}

function Cleanupˉdiagnostic(Detail, Maximumˉbytes) {
    const Prefix = Buffer.from(
        'split compiler status=Cleanupˉfailure detail='
    );
    const Newline = Buffer.from('\n');
    if (Maximumˉbytes < Prefix.length + Newline.length) {
        return Buffer.alloc(0);
    }
    const Source = Buffer.from(String(Detail));
    let Length = Math.min(
        Source.length,
        Maximumˉbytes - Prefix.length - Newline.length,
    );
    while (Length > 0) {
        const Candidate = Source.subarray(0, Length);
        if (Buffer.from(Candidate.toString('utf8')).equals(Candidate)) break;
        Length -= 1;
    }
    return Buffer.concat([Prefix, Source.subarray(0, Length), Newline]);
}

async function Readˉidentity(Candidate) {
    return await lstat(Candidate, { bigint: true });
}

async function Requireˉidentity(Candidate, Expectedˉidentity, Label) {
    const Information = await Readˉidentity(Candidate);
    if (!Sameˉidentity(Information, Expectedˉidentity)) {
        Reject(`The ${Label} identity changed while retained.`);
    }
}

function Sameˉidentity(Left, Right) {
    return Right !== null &&
        BigInt(Left.dev) === BigInt(Right.dev) &&
        BigInt(Left.ino) === BigInt(Right.ino);
}

function Requireˉdistinctˉpaths(Paths, Label) {
    const Keys = Paths.map(Candidate => Pathˉkey(Candidate));
    if (new Set(Keys).size !== Keys.length) {
        Reject(`The ${Label} paths must be distinct.`);
    }
}

function Requireˉdistinctˉidentities(Identities, Label) {
    const Keys = Identities.map(Identity =>
        `${Identity.dev.toString()}:${Identity.ino.toString()}`
    );
    if (new Set(Keys).size !== Keys.length) {
        Reject(`The ${Label} files must not be hard-link aliases.`);
    }
}

function Pathˉkey(Candidate) {
    const Resolved = path.resolve(Candidate);
    return WINDOWS ? Resolved.toLowerCase() : Resolved;
}

function Pathsˉequal(Left, Right) {
    return Pathˉkey(Left) === Pathˉkey(Right);
}

async function Exists(Candidate) {
    return await Lstatˉorˉabsent(Candidate) !== null;
}

function Usage() {
    Reject(
        'Usage: node Run-Split-Compiler.mjs <admitter> <validator> <analyzer> ' +
        '<emitter> [--foreign-binder <wvbind>] --source-input-lock <lock> ' +
        '<sha256> --source-profile ' +
        '<profile> --target-descriptor <target.wvtd> <root.wv> ' +
        '[dependency.wv ...] <output.wvb>; or the retained Project 2 form ' +
        '<admitter> <analyzer> <emitter> <root.wv> [dependency.wv ...] ' +
        '<output.wvb>',
    );
}

function Reject(Message) {
    throw new Error(Message);
}
