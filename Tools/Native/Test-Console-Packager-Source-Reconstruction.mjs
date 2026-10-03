import { mkdtemp, realpath, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { basename, dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { Runˉownedˉconsoleˉcases } from './Native-Owned-Console-Cases.mjs';

const Arguments = process.argv.slice(2);
const Prepareˉonly = Arguments[0] === '--prepare-only';
const Preparedˉonly = Arguments[0] === '--prepared-products-only';
if (Arguments.length !== 3 || !['--owned-current','--prepare-only','--prepared-products-only'].includes(Arguments[0]) ||
    Arguments[1] !== '--maximum-seconds' || !/^[1-9][0-9]*$/u.test(Arguments[2]) ||
    Number(Arguments[2]) < 60 || Number(Arguments[2]) > (Prepareˉonly ? 5400 : Preparedˉonly ? 600 : 1800) ||
    (Prepareˉonly && process.env.WINDVALE_PREPARED_PRODUCTS_ONLY !== undefined)) {
    process.stderr.write('Usage: Test-Console-Packager-Source-Reconstruction.mjs <--owned-current|--prepare-only|--prepared-products-only> --maximum-seconds <60..5400; behavior maximum 1800; prepared maximum 600>\n');
    process.exit(64);
}
if (Preparedˉonly) process.env.WINDVALE_PREPARED_PRODUCTS_ONLY = '1';
const Repository = resolve(dirname(fileURLToPath(import.meta.url)), '../..');
const Temporary = await realpath(tmpdir());
const Work = await realpath(await mkdtemp(join(Temporary,'windvale-owned-console-test-')));
try {
    await Runˉownedˉconsoleˉcases(Repository,Work,Date.now() + Number(Arguments[2]) * 1000 - 10_000,Prepareˉonly);
} catch (Error) {
    process.stderr.write(`${Error.message}\n`);
    process.exitCode = Error.exitCode ?? 1;
} finally {
    if (dirname(Work) !== Temporary || !basename(Work).startsWith('windvale-owned-console-test-')) {
        throw new Error('Refusing to remove an unowned console test directory.');
    }
    await rm(Work,{ recursive:true,force:false });
}
