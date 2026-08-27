import { spawnSync } from 'node:child_process';
import { mkdtempSync, realpathSync, rmSync, statSync, writeFileSync } from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import {
    CALLABLE_WVB_BASE64,
    CALLABLE_WVB_SHA256,
    CLOSURE_WVB_BASE64,
    CLOSURE_WVB_SHA256,
} from './Language-1.0-Callable-Wvb-Fixtures.mjs';
import { createHash } from 'node:crypto';

const MAXIMUM_OUTPUT_BYTES = 65_536;

if (process.argv.length !== 3) {
    Fail('Usage: node Tools/Native/Verify-Language-1.0-Callable-Runner.mjs <runner>');
}

const Runner = realpathSync(path.resolve(process.argv[2]));
const Runnerˉstatus = statSync(Runner);
if (!Runnerˉstatus.isFile() || Runnerˉstatus.size < 1 ||
    Runnerˉstatus.size > 67_108_864) {
    Fail('The callable runner must be one bounded ordinary file.');
}

const Temporaryˉroot = realpathSync(os.tmpdir());
const Work = mkdtempSync(path.join(Temporaryˉroot, 'windvale-callable-runner-'));
try {
    const Cases = [
        [
            'callable', CALLABLE_WVB_BASE64, CALLABLE_WVB_SHA256,
            'Result: 42\nInstructions: 24\n',
        ],
        [
            'closure', CLOSURE_WVB_BASE64, CLOSURE_WVB_SHA256,
            'Result: 42\nInstructions: 11\n',
        ],
    ];
    var Index = 0;
    for (const [Name, Encoded, Expectedˉhash, Expectedˉoutput] of Cases) {
        Index += 1;
        process.stdout.write(
            `START language 1 callable runner compatibility item=${Index}/2 ` +
            `case=${Name}\n`,
        );
        const Bytes = Buffer.from(Encoded, 'base64');
        const Actualˉhash = createHash('sha256').update(Bytes).digest('hex');
        if (Actualˉhash !== Expectedˉhash) {
            Fail(`The ${Name} callable fixture identity differs.`);
        }
        const Module = path.join(Work, `${Name}.wvb`);
        writeFileSync(Module, Bytes, { flag: 'wx' });
        const Result = spawnSync(Runner, [Module, '--report-steps'], {
            encoding: 'utf8',
            windowsHide: true,
            maxBuffer: MAXIMUM_OUTPUT_BYTES,
            timeout: 60_000,
        });
        const Output = Result.stdout.replaceAll('\r\n', '\n');
        if (Result.error !== undefined || Result.status !== 0 ||
            Result.stderr.length !== 0 || Output !== Expectedˉoutput) {
            Fail(
                `The ${Name} callable runner case differed: ` +
                `status=${Result.status} error=${Result.error?.message ?? 'none'}\n` +
                `stdout=${Output}\nstderr=${Result.stderr.replaceAll('\r\n', '\n')}`,
            );
        }
        process.stdout.write(
            `PASS  language 1 callable runner compatibility item=${Index}/2 ` +
            `case=${Name}\n`,
        );
    }
} finally {
    const Resolved = path.resolve(Work);
    if (path.dirname(Resolved) !== Temporaryˉroot ||
        !path.basename(Resolved).startsWith('windvale-callable-runner-')) {
        Fail('The callable runner temporary directory escaped its exact root.');
    }
    rmSync(Resolved, { recursive: true, force: true, maxRetries: 2 });
}

process.stdout.write(
    'native language 1 callable runner compatibility status=Passed ' +
    'cases=2 result=42\n',
);

function Fail(Message) {
    throw new Error(Message);
}
