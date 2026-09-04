import assert from 'node:assert/strict';
import { Runˉdevelopmentˉcommand as Runˉcommand } from './Development-Command-Core.mjs';
import { createHash } from 'node:crypto';
import { lstat, mkdtemp, readFile, realpath, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { basename, dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const SCRIPT_PATH = fileURLToPath(import.meta.url);
const NATIVE = dirname(SCRIPT_PATH);
const REPOSITORY = resolve(NATIVE, '..', '..');
const WINDOWS = process.platform === 'win32';
const MAXIMUM_OUTPUT_BYTES = 65_536;
const MAXIMUM_PRODUCT_BYTES = 67_108_864;
const MAXIMUM_RUN_MILLISECONDS = 600_000;
const PRODUCTS = Object.freeze([
    ['descriptor', 'Source-Descriptor', 33, 'paired-interpreter', 20],
    ['value-front-end', 'Language-1-Value-Front-End', 39, 'interpreter', 40],
    ['generic-declarations', 'Language-1-Generic-Declarations', 3, 'interpreter', 20],
    ['generic-calls', 'Language-1-Generic-Calls', 1, 'native', 150],
    ['generic-resolution', 'Language-1-Generic-Resolution', 1, 'native', 50],
    ['generic-type-catalog', 'Language-1-Generic-Type-Catalog', 1, 'native', 50],
].map(([Name, Project, Cases, Mode, ExpectedSeconds]) => Object.freeze({
    Name, Project: `Projects/Tests/Windvale-Native-Test-${Project}.wvproj`,
    Cases, Mode, ExpectedSeconds,
})));

export function Selectˉproducts(Selection = 'all') {
    if (typeof Selection !== 'string' || Selection.length > 160) {
        throw new Error('Invalid front-end development selection.');
    }
    if (Selection === 'all') return [...PRODUCTS];
    const Names = Selection.split('+');
    if (new Set(Names).size !== Names.length ||
        Names.some(Name => !PRODUCTS.some(Product => Product.Name === Name))) {
        throw new Error('Unknown or duplicate front-end development product.');
    }
    return PRODUCTS.filter(Product => Names.includes(Product.Name));
}

export function Requireˉexecution(Result, Mode) {
    if (!['native', 'interpreter'].includes(Mode)) throw new Error('Unknown execution mode.');
    const Expected = Mode === 'native' ? 42 : 0;
    if (Result.Code !== Expected || Result.Error.length !== 0 ||
        (Mode === 'native' ? Result.Output.length !== 0 :
            !/^Result: 42\r?\n$/u.test(Result.Output))) {
        throw new Error(`Front-end behavior failed: mode=${Mode} exit=${Result.Code}.`);
    }
}

async function Readˉordinary(Path, Maximum) {
    const Information = await lstat(Path);
    if (!Information.isFile() || Information.isSymbolicLink() ||
        Information.size < 1 || Information.size > Maximum ||
        (await realpath(Path)) !== Path) {
        throw new Error(`Expected bounded ordinary file: ${Path}`);
    }
    const Bytes = await readFile(Path);
    if (Bytes.length !== Information.size) throw new Error('Input changed while reading.');
    return Bytes;
}

export async function Readˉplan(Selection = 'all') {
    const Products = [];
    for (const Product of Selectˉproducts(Selection)) {
        const Bytes = await Readˉordinary(join(REPOSITORY, Product.Project), 65_536);
        const Lines = new TextDecoder('utf-8', { fatal: true }).decode(Bytes)
            .trimEnd().split(/\r?\n/u);
        if (Lines.shift() !== 'windvale-project 2' || Lines.pop() !== 'emit wvb') {
            throw new Error(`Unexpected front-end project format: ${Product.Project}`);
        }
        const Inputs = [];
        var Roots = 0;
        for (const Line of Lines) {
            const Match = /^(root|source) "([A-Za-z0-9][A-Za-z0-9./-]*\.wv)"$/u.exec(Line);
            if (!Match || Match[2].split('/').some(Part => !Part || Part === '..' || Part === '.')) {
                throw new Error(`Malformed front-end project declaration: ${Product.Project}`);
            }
            Roots += Number(Match[1] === 'root');
            Inputs.push(Match[2]);
        }
        if (Roots !== 1 || Inputs.length > 64 || new Set(Inputs).size !== Inputs.length) {
            throw new Error('Invalid front-end project source inventory.');
        }
        Products.push({ ...Product, Inputs: [Product.Project, ...Inputs] });
    }
    return {
        Format: 'windvale-front-end-development-plan-1',
        Qualification: false,
        FrozenCases: 251,
        Cases: 251 + Products.reduce((Count, Product) => Count + Product.Cases, 0),
        Products,
    };
}


export async function Executeˉplan(Plan, Operations) {
    if (!Array.isArray(Plan.Products) || Plan.Products.length < 1 ||
        Plan.Products.length > PRODUCTS.length ||
        new Set(Plan.Products.map(Product => Product.Name)).size !== Plan.Products.length ||
        Plan.Products.some(Product => !PRODUCTS.some(Owned =>
            Owned.Name === Product.Name && Owned.Project === Product.Project &&
            Owned.Cases === Product.Cases && Owned.Mode === Product.Mode)) ||
        Plan.Cases !== 251 + Plan.Products.reduce((Count, Product) => Count + Product.Cases, 0)) {
        throw new Error('Incomplete front-end development coverage.');
    }
    await Operations.Frozen();
    const Completed = new Set();
    for (const Product of Plan.Products) {
        if (Completed.has(Product.Name)) throw new Error('Duplicate behavior execution.');
        await Operations.Product(Product);
        Completed.add(Product.Name);
    }
    if (Completed.size !== Plan.Products.length ||
        Plan.Cases !== 251 + Plan.Products.reduce((Count, Product) => Count + Product.Cases, 0)) {
        throw new Error('Incomplete front-end development coverage.');
    }
}

async function Run(Selection) {
    if (!['win32', 'linux'].includes(process.platform)) throw new Error('Unsupported development host.');
    const Started = Date.now();
    const Deadline = Started + MAXIMUM_RUN_MILLISECONDS;
    const Plan = await Readˉplan(Selection);
    const Temporary = await realpath(tmpdir());
    const Work = await realpath(await mkdtemp(join(Temporary, 'windvale-front-end-development-')));
    const Extension = WINDOWS ? 'cmd' : 'sh';
    var CatalogBytes = 0;
    async function Command(Step, Tool, Arguments, Expected = 0) {
        process.stdout.write(`START front-end development step=${Step}\n`);
        const Start = Date.now();
        const Result = await Runˉcommand(Tool, Arguments, Deadline, true);
        if (Result.Code !== Expected || Result.Error.length !== 0) {
            throw new Error(`${Step} failed (${Result.Code}): ${Result.Output}${Result.Error}`);
        }
        process.stdout.write(`PASS front-end development step=${Step} elapsed-ms=${Date.now() - Start}\n`);
        return Result;
    }
    try {
        await Executeˉplan(Plan, {
            Frozen: async () => {
                const Result = await Command('frozen-inputs', process.execPath,
                    [join(NATIVE, 'Verify-Language-1.0-Migration-Fixtures.mjs')]);
                if (!/^language 1 migration fixture identity status=Passed freeze-bytes=\d+ inputs=251 source-fixtures=72\r?\n$/u.test(Result.Output)) {
                    throw new Error('Frozen input coverage differs.');
                }
            },
            Product: async Product => {
                const Start = Date.now();
                const Wvb = join(Work, `${Product.Name}.wvb`);
                const Paired = Product.Mode === 'paired-interpreter';
                const Builder = join(NATIVE, `${Paired ? 'Build-Wvb' : 'Build-Cached-Project-Wvb'}.${Extension}`);
                await Command(`${Product.Name}-build`, Builder, [join(REPOSITORY, Product.Project), Wvb]);
                const Bytes = await Readˉordinary(Wvb, MAXIMUM_PRODUCT_BYTES);
                if (Paired) {
                    const Second = join(Work, `${Product.Name}-independent.wvb`);
                    await Command(`${Product.Name}-independent-build`, Builder, [join(REPOSITORY, Product.Project), Second]);
                    if (!Bytes.equals(await Readˉordinary(Second, MAXIMUM_PRODUCT_BYTES))) {
                        throw new Error('Independent descriptor constructions differ.');
                    }
                }
                const Native = Product.Mode === 'native';
                const Application = join(Work, `${Product.Name}.${WINDOWS ? 'exe' : 'elf'}`);
                if (Native) await Command(`${Product.Name}-package`,
                    join(NATIVE, `Package-Segmented-Compiler-Wvb.${Extension}`),
                    ['1', Wvb, Application, '--development-cache']);
                const Execution = await Runˉcommand(Native ? Application :
                    join(NATIVE, `Run-Wvb.${Extension}`), Native ? [] : [Wvb], Deadline);
                Requireˉexecution(Execution, Native ? 'native' : 'interpreter');
                if (Product.Name === 'generic-type-catalog') CatalogBytes = Bytes.length;
                process.stdout.write(`PASS front-end development product=${Product.Name} cases=${Product.Cases} ` +
                    `execution=fresh wvb-sha256=${createHash('sha256').update(Bytes).digest('hex')} ` +
                    `elapsed-ms=${Date.now() - Start}\n`);
            },
        });
    } finally {
        if (dirname(Work) !== Temporary || !basename(Work).startsWith('windvale-front-end-development-')) {
            throw new Error('Unexpected development temporary directory.');
        }
        await rm(Work, { recursive: true, force: false, maxRetries: 3, retryDelay: 100 });
    }
    if (Date.now() > Deadline) throw Object.assign(
        new Error('Front-end development deadline exceeded during cleanup.'), { exitCode: 124 });
    if (Selection === 'all') process.stdout.write(
        'native language 1 front door development status=Passed cases=329 frozen-inputs=251 source-fixtures=72 ' +
        'descriptor-cases=33 value-front-end-cases=39 generic-front-end-cases=4 generic-resolution-cases=1 ' +
        `generic-type-catalog-cases=1 generic-type-catalog-wvb-bytes=${CatalogBytes}\n`);
    else process.stdout.write(`native language 1 front door development status=Passed cases=${Plan.Cases} ` +
        `selection=${Selection} qualification=false elapsed-ms=${Date.now() - Started}\n`);
}

async function Checkˉrunner() {
    assert.equal((await Readˉplan()).Cases, 329);
    assert.equal((await Readˉplan('generic-declarations')).Cases, 254);
    // Exhaustive bounded oracle: every nonempty subset, independent of input order.
    for (var Mask = 1; Mask < 64; Mask++) {
        const Expected = PRODUCTS.filter((Product, Index) => (Mask & (1 << Index)) !== 0);
        const Selection = [...Expected].reverse().map(Product => Product.Name).join('+');
        assert.deepEqual(Selectˉproducts(Selection), Expected);
    }
    for (const Invalid of ['', 'all+descriptor', 'descriptor+descriptor', '../descriptor', 'x'.repeat(161)]) {
        assert.throws(() => Selectˉproducts(Invalid));
    }
    for (const Mode of ['native', 'interpreter']) {
        const Good = { Code: Mode === 'native' ? 42 : 0,
            Output: Mode === 'native' ? '' : 'Result: 42\n', Error: '' };
        Requireˉexecution(Good, Mode);
        for (const Bad of [{ ...Good, Code: 1 }, { ...Good, Code: null },
            { ...Good, Output: `${Good.Output}unexpected` }, { ...Good, Error: 'diagnostic' }]) {
            assert.throws(() => Requireˉexecution(Bad, Mode));
        }
    }
    const Plan = await Readˉplan();
    var Runs = 0;
    const Operations = { Frozen: async () => {}, Product: async () => { Runs++; } };
    await Executeˉplan(Plan, Operations);
    await Executeˉplan(Plan, Operations);
    assert.equal(Runs, 12, 'Product reuse must not skip behavior execution.');
    await assert.rejects(() => Executeˉplan({ ...Plan, Products: Plan.Products.slice(1) }, Operations));
    await assert.rejects(() => Executeˉplan({ ...Plan, Products: [Plan.Products[0], Plan.Products[0]] }, Operations));
    for (const Failed of Plan.Products) {
        const Seen = [];
        await assert.rejects(() => Executeˉplan(Plan, { Frozen: async () => {}, Product: async Product => {
            Seen.push(Product.Name);
            if (Product.Name === Failed.Name) throw new Error('Seeded behavior failure');
        } }));
        assert.equal(Seen.at(-1), Failed.Name, 'Failure must stop the plan.');
    }
    await assert.rejects(() => Runˉcommand(process.execPath,
        ['-e', 'setInterval(() => {}, 1000)'], Date.now() + 150), Error => Error.exitCode === 124);
    await assert.rejects(() => Runˉcommand(process.execPath,
        ['-e', 'process.stdout.write("x".repeat(70000))'], Date.now() + 5_000), Error => Error.exitCode === 2);
    const Wrapper = await Runˉcommand(join(NATIVE,
        `Test-Language-1.0-Front-Door.${WINDOWS ? 'cmd' : 'sh'}`),
        ['--development-target', 'unknown-product'], Date.now() + 5_000);
    assert.equal(Wrapper.Code, 1, 'The host wrapper must propagate child rejection.');
    process.stdout.write('front-end development runner checks status=Passed products=6 subsets=63\n');
}

if (process.argv[1] && resolve(process.argv[1]) === SCRIPT_PATH) {
    try {
        const Arguments = process.argv.slice(2);
        if (Arguments.length === 1 && Arguments[0] === '--plan') {
            process.stdout.write(`${JSON.stringify(await Readˉplan())}\n`);
        } else if (Arguments.length === 1 && Arguments[0] === '--check-runner') {
            await Checkˉrunner();
        } else if (Arguments.length === 0) await Run('all');
        else if (Arguments.length === 2 && Arguments[0] === '--target') {
            Selectˉproducts(Arguments[1]);
            await Run(Arguments[1]);
        } else throw Object.assign(new Error('Usage: Test-Language-1.0-Front-Door-Development.mjs [--plan|--check-runner|--target <product+product>]'), { exitCode: 64 });
    } catch (Error) {
        process.stderr.write(`${Error.message.slice(0, MAXIMUM_OUTPUT_BYTES)}\n`);
        process.exitCode = Error.exitCode ?? 1;
    }
}
