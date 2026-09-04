import { createHash } from 'node:crypto';
import { spawn } from 'node:child_process';
import { lstat, mkdtemp, readFile, realpath, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { basename, dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const WINDOWS = process.platform === 'win32';
const MAXIMUM_OUTPUT_BYTES = 128 * 1024;
const MAXIMUM_WVB_BYTES = 1 * 1024 * 1024;
const HEARTBEAT_INTERVAL_MILLISECONDS = 30_000;
const BUILD_TIMEOUT_MILLISECONDS = 55 * 60_000;
const PACKAGE_TIMEOUT_MILLISECONDS = 30 * 60_000;
const CASE_TIMEOUT_MILLISECONDS = 120_000;
const MAXIMUM_APPLICATION_BYTES = 134_217_728;
const TASKKILL_TIMEOUT_MILLISECONDS = 2_000;
const TERMINATION_SETTLE_MILLISECONDS = 5_000;
const FIXTURES = Object.freeze([
    Object.freeze({
        name: 'core',
        application: 'combined',
        selectors: Object.freeze([...'abcdefghijklmn']),
    }),
    Object.freeze({
        name: 'portable',
        application: 'combined',
        selectors: Object.freeze([...'opqrstuvwx']),
    }),
    Object.freeze({
        name: 'typed-wir',
        application: 'typed-wir',
        selectors: Object.freeze(['y']),
    }),
    Object.freeze({
        name: 'typed-wir-validation',
        application: 'combined',
        selectors: Object.freeze(['z']),
    }),
    Object.freeze({
        name: 'pairing',
        application: 'pairing',
        selectors: Object.freeze(['A']),
    }),
]);
const SELECTORS = FIXTURES.flatMap(Fixture =>
    Fixture.selectors.map(Selector => ({ Fixture, Selector }))
);
const SCRIPT_DIRECTORY = dirname(fileURLToPath(import.meta.url));
const REPOSITORY_ROOT = resolve(SCRIPT_DIRECTORY, '..', '..');

function Reject(Message) {
    throw new Error(Message);
}

function Processˉisˉlive(Child) {
    return Child.pid !== undefined &&
        Child.exitCode === null && Child.signalCode === null;
}

function Runˉboundedˉtaskkill(Processˉidentifier) {
    return new Promise(Resolveˉresult => {
        const Killer = spawn(
            'taskkill.exe',
            ['/pid', String(Processˉidentifier), '/t', '/f'],
            { stdio: 'ignore', windowsHide: true }
        );
        var Settled = false;
        const Timer = setTimeout(() => {
            if (Settled) return;
            Settled = true;
            Killer.kill('SIGKILL');
            Killer.unref();
            Resolveˉresult('taskkill did not settle within 2000 ms');
        }, TASKKILL_TIMEOUT_MILLISECONDS);
        Killer.once('error', Errorˉvalue => {
            if (Settled) return;
            Settled = true;
            clearTimeout(Timer);
            Resolveˉresult(`taskkill error: ${Errorˉvalue.message}`);
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
    var Diagnostic = null;
    if (WINDOWS) {
        Diagnostic = await Runˉboundedˉtaskkill(Child.pid);
    }
    else {
        try {
            process.kill(-Child.pid, 'SIGKILL');
        } catch (Errorˉvalue) {
            Diagnostic = `process-group kill error: ${Errorˉvalue.message}`;
        }
    }
    if (Processˉisˉlive(Child)) {
        try {
            if (!Child.kill('SIGKILL') && Diagnostic !== null) {
                Diagnostic += '; direct child kill returned false';
            }
        } catch (Errorˉvalue) {
            if (Diagnostic !== null) {
                Diagnostic +=
                    `; direct child kill error: ${Errorˉvalue.message}`;
            }
        }
    }
    return Diagnostic;
}

function Runˉcommand(
    Tool,
    Argumentsˉvalue,
    Timeoutˉmilliseconds,
    Relayˉstdout = false,
    Activity = 'command'
) {
    return new Promise((Resolveˉresult, Rejectˉpromise) => {
        const Isˉcommand = WINDOWS && Tool.toLowerCase().endsWith('.cmd');
        if (Isˉcommand && [Tool, ...Argumentsˉvalue].some(
            Argument => /[\r\n&|<>^%!"]/u.test(Argument)
        )) {
            Rejectˉpromise(new Error(
                'A Windows foreign-binding owner argument contains shell metacharacters.'
            ));
            return;
        }
        const Executable = Isˉcommand
            ? process.env.ComSpec ?? 'cmd.exe'
            : Tool;
        const Commandˉarguments = Isˉcommand
            ? [
                '/d', '/v:off', '/s', '/c',
                `"${[Tool, ...Argumentsˉvalue]
                    .map(Argument => `"${Argument}"`)
                    .join(' ')}"`,
            ]
            : Argumentsˉvalue;
        const Started = Date.now();
        const Child = spawn(Executable, Commandˉarguments, {
            cwd: REPOSITORY_ROOT,
            detached: !WINDOWS,
            stdio: ['ignore', 'pipe', 'pipe'],
            windowsHide: true,
            windowsVerbatimArguments: Isˉcommand,
        });
        const Heartbeat = setInterval(() => {
            process.stdout.write(
                `INFO  language 1 authenticated foreign binding active ` +
                `step=${Activity} elapsed-ms=${Date.now() - Started}\n`
            );
        }, HEARTBEAT_INTERVAL_MILLISECONDS);
        Heartbeat.unref();
        const Output = [];
        const Errorˉoutput = [];
        var Outputˉbytes = 0;
        var Errorˉbytes = 0;
        var Exceeded = false;
        var Timedˉout = false;
        var Settled = false;
        var Cleanupˉfailure = null;
        var Terminationˉdiagnostic = null;
        var Terminationˉpromise = null;
        var Closeˉcode = null;
        var Closeˉreceived = false;
        var Settleˉtimer;
        function Result(Code, Forced) {
            return {
                Code,
                Elapsed: Date.now() - Started,
                Error: Buffer.concat(Errorˉoutput),
                Cleanupˉfailure,
                Exceeded,
                Forced,
                Output: Buffer.concat(Output),
                Timedˉout,
            };
        }
        function Complete(Code, Forced) {
            if (Settled) return;
            Settled = true;
            clearTimeout(Timer);
            clearInterval(Heartbeat);
            if (Settleˉtimer !== undefined) clearTimeout(Settleˉtimer);
            Resolveˉresult(Result(Code, Forced));
        }
        function Terminateˉandˉsettle() {
            if (Terminationˉpromise !== null) return;
            Settleˉtimer = setTimeout(() => {
                try {
                    Child.kill('SIGKILL');
                } catch (Errorˉvalue) {
                    Terminationˉdiagnostic = Terminationˉdiagnostic ??
                        `final direct child kill error: ${Errorˉvalue.message}`;
                }
                const Settleˉdiagnostic =
                    `process tree did not close within ` +
                    `${TERMINATION_SETTLE_MILLISECONDS} ms`;
                Cleanupˉfailure = Terminationˉdiagnostic === null
                    ? Settleˉdiagnostic
                    : `${Terminationˉdiagnostic}; ${Settleˉdiagnostic}`;
                Child.stdout.destroy();
                Child.stderr.destroy();
                Child.unref();
                Complete(null, true);
            }, TERMINATION_SETTLE_MILLISECONDS);
            Terminationˉpromise = (async () => {
                try {
                    Terminationˉdiagnostic =
                        await Terminateˉprocessˉtree(Child);
                } catch (Errorˉvalue) {
                    Terminationˉdiagnostic =
                        `tree termination error: ${Errorˉvalue.message}`;
                    try {
                        Child.kill('SIGKILL');
                    } catch {
                        // The bounded settle path records the diagnostic.
                    }
                }
                Cleanupˉfailure = Terminationˉdiagnostic;
                if (Closeˉreceived) Complete(Closeˉcode, false);
            })();
        }
        const Timer = setTimeout(() => {
            Timedˉout = true;
            Terminateˉandˉsettle();
        }, Timeoutˉmilliseconds);
        Child.stdout.on('data', Chunk => {
            Outputˉbytes += Chunk.length;
            if (Outputˉbytes <= MAXIMUM_OUTPUT_BYTES) {
                Output.push(Chunk);
                if (Relayˉstdout) process.stdout.write(Chunk);
            }
            else if (!Exceeded) {
                Exceeded = true;
                Terminateˉandˉsettle();
            }
        });
        Child.stderr.on('data', Chunk => {
            Errorˉbytes += Chunk.length;
            if (Errorˉbytes <= MAXIMUM_OUTPUT_BYTES) {
                Errorˉoutput.push(Chunk);
            }
            else if (!Exceeded) {
                Exceeded = true;
                Terminateˉandˉsettle();
            }
        });
        Child.once('error', Errorˉvalue => {
            if (Timedˉout || Exceeded || Settled) return;
            Settled = true;
            clearTimeout(Timer);
            clearInterval(Heartbeat);
            if (Settleˉtimer !== undefined) clearTimeout(Settleˉtimer);
            Rejectˉpromise(Errorˉvalue);
        });
        Child.once('close', Code => {
            Closeˉreceived = true;
            Closeˉcode = Code;
            if (Terminationˉpromise === null) {
                Complete(Code, false);
                return;
            }
            void Terminationˉpromise.then(() => Complete(Code, false));
        });
    });
}

function Requireˉcleanˉresult(Result, Label, Expectedˉcode) {
    if (Result.Cleanupˉfailure !== null) {
        Reject(`${Label} cleanup failed: ${Result.Cleanupˉfailure}.`);
    }
    if (Result.Timedˉout) Reject(`${Label} exceeded its time bound.`);
    if (Result.Exceeded) Reject(`${Label} exceeded its output bound.`);
    if (Result.Code !== Expectedˉcode) {
        Reject(
            `${Label} returned ${Result.Code}; expected ${Expectedˉcode}.\n` +
            Result.Error.toString('utf8') + Result.Output.toString('utf8')
        );
    }
}

async function Fileˉidentity(
    Pathˉvalue,
    Label,
    Maximumˉbytes = MAXIMUM_WVB_BYTES
) {
    const Information = await lstat(Pathˉvalue);
    if (!Information.isFile() || Information.isSymbolicLink() ||
        Information.size < 12 || Information.size > Maximumˉbytes) {
        Reject(`The ${Label} is not a bounded ordinary WVB file.`);
    }
    const Bytes = await readFile(Pathˉvalue);
    return {
        bytes: Bytes.length,
        sha256: createHash('sha256').update(Bytes).digest('hex'),
        value: Bytes,
    };
}

async function Processˉidentifierˉisˉlive(Processˉidentifier) {
    try {
        process.kill(Processˉidentifier, 0);
    } catch (Errorˉvalue) {
        if (Errorˉvalue.code === 'ESRCH') return false;
        throw Errorˉvalue;
    }
    if (WINDOWS) return true;
    const Record = await readFile(
        `/proc/${Processˉidentifier}/stat`, 'ascii'
    ).catch(Errorˉvalue => {
        if (Errorˉvalue.code === 'ENOENT') return null;
        throw Errorˉvalue;
    });
    if (Record === null) return false;
    const Commandˉend = Record.lastIndexOf(') ');
    if (Commandˉend < 0 || Commandˉend + 2 >= Record.length) {
        Reject(`The termination probe process state is malformed: ${Processˉidentifier}.`);
    }
    const State = Record[Commandˉend + 2];
    return State !== 'Z' && State !== 'X';
}

async function Waitˉforˉprocessˉexit(Processˉidentifier) {
    const Deadline = Date.now() + 1_000;
    while (await Processˉidentifierˉisˉlive(Processˉidentifier)) {
        if (Date.now() >= Deadline) return false;
        await new Promise(Resolveˉwait => setTimeout(Resolveˉwait, 25));
    }
    return true;
}

async function Runˉterminationˉprobe() {
    const Descendantˉsource = 'setInterval(()=>{},1000)';
    const Source =
        "const{spawn}=require('node:child_process');" +
        `const c=spawn(process.execPath,['-e',${JSON.stringify(
            Descendantˉsource
        )}],{stdio:'ignore'});` +
        'process.stdout.write(String(c.pid));setInterval(()=>{},1000)';
    const Result = await Runˉcommand(
        process.execPath, ['-e', Source], 500, false, 'termination-probe'
    );
    if (!Result.Timedˉout || Result.Exceeded || Result.Forced ||
        Result.Cleanupˉfailure !== null) {
        Reject(
            `The termination probe returned an invalid result: ` +
            `timed-out=${Result.Timedˉout} exceeded=${Result.Exceeded} ` +
            `forced=${Result.Forced} cleanup=${Result.Cleanupˉfailure}.`
        );
    }
    const Descendantˉtext = Result.Output.toString('utf8');
    if (!/^[1-9][0-9]*$/u.test(Descendantˉtext)) {
        Reject('The termination probe descendant identity is invalid.');
    }
    const Descendantˉidentifier = Number.parseInt(Descendantˉtext, 10);
    if (!await Waitˉforˉprocessˉexit(Descendantˉidentifier)) {
        Reject('The termination probe left its descendant running.');
    }
    process.stdout.write(
        'authenticated foreign binding process termination probe status=Passed\n'
    );
}

async function Removeˉwork(Work, Temporaryˉroot) {
    const Realˉroot = await realpath(Temporaryˉroot);
    const Realˉparent = await realpath(dirname(Work));
    if (Realˉparent !== Realˉroot ||
        !basename(Work).startsWith(
            'windvale-authenticated-foreign-binding-'
        )) {
        Reject(`Refusing to remove unexpected temporary path: ${Work}`);
    }
    await rm(Work, { recursive: true, force: false, maxRetries: 2 });
}

async function Main() {
    const Probeˉonly = process.argv.length === 3 &&
        process.argv[2] === '--termination-probe';
    if (!Probeˉonly && process.argv.length !== 2) {
        Reject('The authenticated foreign-binding owner accepts no arguments.');
    }
    await Runˉterminationˉprobe();
    if (Probeˉonly) return;
    const Selectorˉvalues = SELECTORS.map(Item => Item.Selector);
    if (FIXTURES.length !== 5 || SELECTORS.length !== 27 ||
        new Set(Selectorˉvalues).size !== SELECTORS.length ||
        Selectorˉvalues.some(Selector => !/^[A-Za-z]$/u.test(Selector))) {
        Reject('The authenticated foreign-binding selector inventory is invalid.');
    }

    const Temporaryˉroot = resolve(tmpdir());
    const Work = await realpath(await mkdtemp(join(
        Temporaryˉroot,
        'windvale-authenticated-foreign-binding-'
    )));
    var Passed = false;
    try {
        const Build = join(
            SCRIPT_DIRECTORY,
            'Build-Current-Split-Project-Wvb.mjs'
        );
        const Package = join(
            SCRIPT_DIRECTORY,
            'Build-Cached-Segmented-Hosted-Wvb.mjs'
        );
        const Combined = {
            name: 'combined',
            Application: join(
                Work,
                WINDOWS
                    ? 'Authenticated-Foreign-Binding-Combined.exe'
                    : 'Authenticated-Foreign-Binding-Combined.elf'
            ),
            Product: join(
                Work,
                'Authenticated-Foreign-Binding-Combined.wvb'
            ),
            Project: join(
                REPOSITORY_ROOT,
                'Projects', 'Tests',
                'Windvale-Native-Test-Language-1-Authenticated-Foreign-Binding-Combined.wvproj'
            ),
        };
        const Typedˉwir = {
            name: 'typed-wir',
            Application: join(
                Work,
                WINDOWS ? 'Typed-Foreign-Call-Wir.exe' : 'Typed-Foreign-Call-Wir.elf'
            ),
            Product: join(Work, 'Typed-Foreign-Call-Wir.wvb'),
            Project: join(
                REPOSITORY_ROOT,
                'Projects', 'Tests',
                'Windvale-Native-Test-Language-1-Typed-Foreign-Call-Wir.wvproj'
            ),
        };
        // The production-admission owner independently reconstructs wvbind.
        // Pairing is a small ordinary module in the combined product. Typed WIR
        // retains its separate product because merging its large WIR compiler
        // module would exceed the analyzer's bounded working set. Independent
        // native packages run concurrently after both products are complete.
        const Buildˉproducts = [Combined, Typedˉwir];
        const Packageˉproducts = [Combined, Typedˉwir];
        const Applicationˉbyˉfixture = new Map([
            ['combined', Combined.Application],
            ['typed-wir', Typedˉwir.Application],
            ['pairing', Combined.Application],
        ]);

        process.stdout.write(
            'START language 1 authenticated foreign binding ' +
            'phase=build item=1/3 fixtures=5 products=2\n'
        );
        const Buildˉstarted = Date.now();
        const Buildˉarguments = [Build];
        for (const Item of Buildˉproducts) {
            Buildˉarguments.push(Item.Project, Item.Product);
        }
        const Buildˉresult = await Runˉcommand(
            process.execPath,
            Buildˉarguments,
            BUILD_TIMEOUT_MILLISECONDS,
            true,
            'build-current-products'
        );
        Requireˉcleanˉresult(
            Buildˉresult,
            'foreign-binding current products build',
            0
        );
        if (Buildˉresult.Error.length !== 0) {
            Reject(
                'The foreign-binding current products build wrote to stderr.\n' +
                Buildˉresult.Error.toString('utf8')
            );
        }
        for (const [Index, Item] of Buildˉproducts.entries()) {
            Item.Productˉidentity = await Fileˉidentity(
                Item.Product,
                `authenticated foreign-binding ${Item.name} product`,
                2 * MAXIMUM_WVB_BYTES
            );
            process.stdout.write(
                `INFO  language 1 authenticated foreign binding phase=build ` +
                `product=${Index + 1}/${Buildˉproducts.length} name=${Item.name} ` +
                `wvb-bytes=${Item.Productˉidentity.bytes} ` +
                `sha256=${Item.Productˉidentity.sha256}\n`
            );
        }
        process.stdout.write(
            `PASS  language 1 authenticated foreign binding phase=build ` +
            `item=1/3 elapsed-ms=${Date.now() - Buildˉstarted} ` +
            `fixtures=5 products=2\n`
        );

        process.stdout.write(
            'START language 1 authenticated foreign binding ' +
            'phase=package item=2/3 fixtures=5 applications=2\n'
        );
        const Packageˉstarted = Date.now();
        const Packageˉresults = await Promise.all(Packageˉproducts.map(async Item => {
            const Packageˉresult = await Runˉcommand(
                process.execPath,
                [Package, '7', Item.Product, Item.Application],
                PACKAGE_TIMEOUT_MILLISECONDS,
                true,
                `package-${Item.name}`
            );
            Requireˉcleanˉresult(
                Packageˉresult,
                `foreign-binding ${Item.name} package`,
                0
            );
            if (Packageˉresult.Error.length !== 0) {
                Reject(
                    `The foreign-binding ${Item.name} package wrote to stderr.\n` +
                    Packageˉresult.Error.toString('utf8')
                );
            }
            const Applicationˉinformation = await lstat(Item.Application);
            if (!Applicationˉinformation.isFile() ||
                Applicationˉinformation.isSymbolicLink() ||
                Applicationˉinformation.size < 1 ||
                Applicationˉinformation.size > MAXIMUM_APPLICATION_BYTES) {
                Reject(
                    `The packaged foreign-binding ${Item.name} application is invalid.`
                );
            }
            return Applicationˉinformation.size;
        }));
        const Applicationˉbytes = Packageˉresults.reduce(
            (Total, Bytes) => Total + Bytes,
            0
        );
        process.stdout.write(
            'PASS  language 1 authenticated foreign binding ' +
            `phase=package item=2/3 elapsed-ms=${Date.now() - Packageˉstarted} ` +
            `fixtures=5 applications=2 application-bytes=${Applicationˉbytes}\n`
        );

        process.stdout.write(
            'START language 1 authenticated foreign binding ' +
            'phase=execute item=3/3 cases=27 fixtures=5\n'
        );
        for (const [Index, Item] of SELECTORS.entries()) {
            process.stdout.write(
                `START language 1 authenticated foreign binding ` +
                `phase=execute case=${Index + 1}/${SELECTORS.length} ` +
                `fixture=${Item.Fixture.name} selector=${Item.Selector}\n`
            );
            const Application = Applicationˉbyˉfixture.get(
                Item.Fixture.application
            );
            if (Application === undefined) {
                Reject(`Unknown fixture application: ${Item.Fixture.application}.`);
            }
            const Result = await Runˉcommand(
                Application,
                [Item.Selector],
                CASE_TIMEOUT_MILLISECONDS,
                false,
                `execute-${Index + 1}`
            );
            Requireˉcleanˉresult(
                Result,
                `authenticated foreign-binding case ${Index + 1}`,
                42
            );
            if (Result.Output.length !== 0 || Result.Error.length !== 0) {
                Reject(
                    `Authenticated foreign-binding case ${Index + 1} wrote output.\n` +
                    Result.Output.toString('utf8') +
                    Result.Error.toString('utf8')
                );
            }
            process.stdout.write(
                `PASS  language 1 authenticated foreign binding ` +
                `phase=execute case=${Index + 1}/${SELECTORS.length} ` +
                `fixture=${Item.Fixture.name} selector=${Item.Selector} ` +
                `elapsed-ms=${Result.Elapsed}\n`
            );
        }
        for (const Item of Buildˉproducts) {
            const Finalˉidentity = await Fileˉidentity(
                Item.Product,
                `verified authenticated foreign-binding ${Item.name} product`,
                2 * MAXIMUM_WVB_BYTES
            );
            if (!Finalˉidentity.value.equals(Item.Productˉidentity.value)) {
                Reject(
                    `The authenticated foreign-binding ${Item.name} WVB ` +
                    'changed during execution.'
                );
            }
        }
        Passed = true;
    } finally {
        await Removeˉwork(Work, Temporaryˉroot);
    }

    if (Passed) {
        process.stdout.write(
            'native language 1 authenticated foreign binding status=Passed ' +
            'cases=27 fixtures=5 result=42 execution=native-profile-7 ' +
            'isolated-executions=27\n'
        );
    }
}

await Main();
