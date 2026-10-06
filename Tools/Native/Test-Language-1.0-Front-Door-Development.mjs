import assert from 'node:assert/strict';
import { Runˉdevelopmentˉcommand as Runˉcommand } from './Development-Command-Core.mjs';
import { createHash } from 'node:crypto';
import { lstat, mkdtemp, readFile, realpath, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { basename, dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { Getˉcurrentˉsplitˉcompilerˉfamily, Getˉcurrentˉsplitˉcompilerˉkey,
    Readˉpreparedˉsplitˉcompiler } from './Current-Split-Compiler-Cache-Core.mjs';

const SCRIPT_PATH = fileURLToPath(import.meta.url);
const NATIVE = dirname(SCRIPT_PATH);
const REPOSITORY = resolve(NATIVE, '..', '..');
const WINDOWS = process.platform === 'win32';
const MAXIMUM_OUTPUT_BYTES = 65_536;
const MAXIMUM_PRODUCT_BYTES = 67_108_864;
const MAXIMUM_RUN_MILLISECONDS = 600_000;
const MAXIMUM_PREPARATION_SECONDS = 5_400;
const PRODUCTS = Object.freeze([
    ['descriptor', 'Source-Descriptor', 33, 'paired-interpreter', 20],
    ['value-front-end', 'Language-1-Value-Front-End', 39, 'interpreter', 40],
    ['generic-declarations', 'Language-1-Generic-Declarations', 3, 'interpreter', 20],
    ['generic-calls', 'Language-1-Generic-Calls', 1, 'native', 150],
    ['generic-resolution', 'Language-1-Generic-Resolution', 1, 'native', 50],
    ['generic-type-catalog', 'Language-1-Generic-Type-Catalog', 1, 'native', 50],
    ['bytes-source', 'Language-1-Reserved-Byte-Construction', 107, 'publication', 180, 42, 8, 1, [44, null, null, null]],
    ['source-analysis', 'Projects/Tests/Language-1.0-Source-Analysis-Self-Test.wvproj', 16, 'publication', 20, 0, 8, 1, []],
    ['generic-wir', 'Language-1-Generic-Wir', 14, 'publication', 20, 42, 8, 1, []],
    ['generic-analysis-publication', 'Language-1-Generic-Analysis-Publication', 9, 'publication', 20, 42, 8, 1],
    ['generic-collection-publication', 'Language-1-Generic-Collection-Analysis-Publication', 6, 'publication', 20, 42, 8, 1, [11]],
].map(([Name, Project, Cases, Mode, ExpectedSeconds, ExpectedExit, PackageProfile, ArgumentGuardCases, Publicationˉminors]) => Object.freeze({
    Name, Project: Project.startsWith('Projects/') ? Project : `Projects/Tests/Windvale-Native-Test-${Project}.wvproj`,
    Cases, Mode, ExpectedSeconds, ExpectedExit: ExpectedExit ?? (Mode === 'native' ? 42 : 0),
    PackageProfile: PackageProfile ?? (Mode === 'native' ? 1 : 0),
    ArgumentGuardCases: ArgumentGuardCases ?? 0,
    SemanticCases: Cases - (ArgumentGuardCases ?? 0),
    Publicationˉminors: Object.freeze(Publicationˉminors ?? (Mode === 'publication' ? [null] : [])),
    Validatorˉproject: Name === 'bytes-source' ?
        'Projects/Tests/Windvale-Native-Test-Language-1-Reserved-Byte-Validation.wvproj' :
        Name === 'source-analysis' ? 'Projects/Tests/Language-1.0-Source-Analysis-Validation-Self-Test.wvproj' :
        Name === 'generic-wir' ? 'Projects/Tests/Windvale-Native-Test-Language-1-Generic-Wir-Validation.wvproj' :
        Name === 'generic-analysis-publication' ? 'Projects/Tests/Windvale-Native-Test-Language-1-Generic-Analysis-Validation.wvproj' : null,
})));
const COMPILER_ANALYSIS_PRODUCTS = Object.freeze(['source-analysis', 'generic-wir',
    'generic-analysis-publication', 'generic-collection-publication']);
const BYTE_VALIDATION_CASES = Object.freeze([0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
    10, 11, 14, 15, 30, 32, 33, 34, 37]);

export function Selectˉproducts(Selection = 'all') {
    if (typeof Selection !== 'string' || Selection.length > 512) {
        throw new Error('Invalid front-end development selection.');
    }
    if (Selection === 'all') return [...PRODUCTS];
    const Names = Selection.split('+').flatMap(Name =>
        Name === 'compiler-analysis' ? COMPILER_ANALYSIS_PRODUCTS : [Name]);
    if (new Set(Names).size !== Names.length ||
        Names.some(Name => !PRODUCTS.some(Product => Product.Name === Name))) {
        throw new Error('Unknown or duplicate front-end development product.');
    }
    return PRODUCTS.filter(Product => Names.includes(Product.Name));
}

export function Requireˉexecution(Result, Mode, Expected = Mode === 'interpreter' ? 0 : 42) {
    if (!['native', 'publication', 'interpreter'].includes(Mode) ||
        !Number.isInteger(Expected) || Expected < 0 || Expected > 255 ||
        (Mode === 'interpreter' && Expected !== 0)) throw new Error('Unknown execution contract.');
    if (Result.Code !== Expected || Result.Error.length !== 0 ||
        (Mode !== 'interpreter' ? Result.Output.length !== 0 :
            !/^Result: 42\r?\n$/u.test(Result.Output))) {
        throw Object.assign(new Error(`Front-end behavior failed: mode=${Mode} exit=${Result.Code}.`),
            { exitCode: Number.isInteger(Result.Code) && Result.Code >= 1 && Result.Code <= 255 &&
                Result.Code !== Expected ? Result.Code : 1 });
    }
}


// File/header agreement is checked here; the existing Emitter admits source,
// binding and WIR semantics before any published WVB is accepted.
function Publicationˉmoduleˉcount(Source) {
    if (Source.length < 16 || Source.toString('ascii', 0, 4) !== 'WVSS' ||
        ![1, 2].includes(Source.readUInt16LE(4)) || Source.readUInt16LE(6) !== 0) {
        throw new Error('Publication source header differs.');
    }
    const Version = Source.readUInt16LE(4), Modules = Source.readUInt32LE(8);
    const Entryˉbytes = Version === 2 ? 20 : 8;
    if (Modules < 1 || Modules > 64 || Source.readUInt32LE(12) !== Modules * Entryˉbytes ||
        Modules * Entryˉbytes > Source.length - 16) {
        throw new Error('Publication source directory differs.');
    }
    var Expectedˉoffset = 16 + Modules * Entryˉbytes;
    for (var Index = 0; Index < Modules; Index++) {
        const Entry = 16 + Index * Entryˉbytes;
        const Offset = Source.readUInt32LE(Entry), Length = Source.readUInt32LE(Entry + 4);
        if (Offset !== Expectedˉoffset || Length < 1 || Length > Source.length - Offset ||
            (Version === 2 && (Source.readUInt32LE(Entry + 8) !== 1 ||
                Source.readUInt32LE(Entry + 12) !== 1 || Source.readUInt32LE(Entry + 16) < 1 ||
                Source.readUInt32LE(Entry + 16) > 129))) {
            throw new Error('Publication source entry differs.');
        }
        Expectedˉoffset = Offset + Length;
    }
    if (Expectedˉoffset !== Source.length) throw new Error('Publication source extent differs.');
    return Modules;
}

export function Requireˉpublication(Values) {
    if (!Array.isArray(Values) || Values.length !== 4 || Values.some(Value =>
        !Buffer.isBuffer(Value) || Value.length < 1 || Value.length > 4_194_304)) {
        throw new Error('Publication file extent differs.');
    }
    const [Source, Manifest, Bindings, Wir] = Values;
    const Modules = Publicationˉmoduleˉcount(Source);
    if (Manifest.length !== 104 || Manifest.toString('ascii', 0, 4) !== 'WVCA' ||
        Manifest.readUInt16LE(4) !== 1 || Manifest.readUInt16LE(6) !== 0 ||
        Manifest.readUInt32LE(8) !== 104 || Manifest.readUInt32LE(100) !== 0 ||
        Manifest.readUInt32LE(12) !== Source.length ||
        Manifest.readUInt32LE(16) !== Bindings.length ||
        Manifest.readUInt32LE(20) !== Wir.length || Manifest.readUInt32LE(24) !== Modules ||
        Bindings.length < 32 || Bindings.toString('ascii', 0, 4) !== 'WVLB' ||
        Bindings.readUInt16LE(4) !== 1 || Wir.length < 48 ||
        Wir.toString('ascii', 0, 4) !== 'WVIR' || Wir.readUInt16LE(4) !== 1 ||
        Manifest.readUInt32LE(68) !== Bindings.readUInt32LE(8) ||
        [[80, 8], [84, 16], [88, 24], [92, 32], [96, 40]].some(([Left, Right]) =>
            Manifest.readUInt32LE(Left) !== Wir.readUInt32LE(Right))) {
        throw new Error('Publication header or manifest agreement differs.');
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

async function Readˉproject(Project) {
    const Bytes = await Readˉordinary(join(REPOSITORY, Project), 65_536);
    const Lines = new TextDecoder('utf-8', { fatal: true }).decode(Bytes)
        .trimEnd().split(/\r?\n/u);
    const Header = Lines.shift();
    const Admission = [];
    if (Header === 'windvale-project 4') {
        const Directives = Lines.splice(-4);
        const Patterns = [/^source-input-lock "([A-Za-z0-9][A-Za-z0-9./-]*\.wvlock)"$/u,
            /^source-input-lock-sha256 [0-9a-f]{64}$/u,
            /^source-profile "([A-Za-z0-9][A-Za-z0-9./-]*\.wvsp)"$/u,
            /^target-descriptor "([A-Za-z0-9][A-Za-z0-9./-]*\.wvtd)"$/u];
        for (const [Index, Pattern] of Patterns.entries()) {
            const Match = Pattern.exec(Directives[Index] ?? '');
            if (!Match || Match[1]?.split('/').some(Part => !Part || Part === '..' || Part === '.')) {
                throw new Error(`Malformed front-end admission input: ${Project}`);
            }
            if (Match[1]) Admission.push(Match[1]);
        }
    }
    if (!['windvale-project 2', 'windvale-project 4'].includes(Header) || Lines.pop() !== 'emit wvb') {
        throw new Error(`Unexpected front-end project format: ${Project}`);
    }
    const Inputs = [];
    var Roots = 0;
    for (const Line of Lines) {
        const Match = /^(root|source) "([A-Za-z0-9][A-Za-z0-9./-]*\.wv)"$/u.exec(Line);
        if (!Match || Match[2].split('/').some(Part => !Part || Part === '..' || Part === '.')) {
            throw new Error(`Malformed front-end project declaration: ${Project}`);
        }
        Roots += Number(Match[1] === 'root');
        Inputs.push(Match[2]);
    }
    if (Roots !== 1 || Inputs.length > 64 || new Set(Inputs).size !== Inputs.length) {
        throw new Error('Invalid front-end project source inventory.');
    }
    return { RequiresCurrentCompiler: Header === 'windvale-project 4',
        Inputs: [Project, ...Inputs, ...Admission] };
}

export async function Readˉplan(Selection = 'all') {
    const Products = [];
    for (const Product of Selectˉproducts(Selection)) {
        const Primary = await Readˉproject(Product.Project);
        const Validator = Product.Validatorˉproject === null ? null : await Readˉproject(Product.Validatorˉproject);
        if (Validator !== null && (!Primary.RequiresCurrentCompiler || !Validator.RequiresCurrentCompiler)) {
            throw new Error('Split validation requires current Project4 programs.');
        }
        Products.push({ ...Product, RequiresCurrentCompiler: Primary.RequiresCurrentCompiler,
            Inputs: [...new Set([...Primary.Inputs, ...(Validator?.Inputs ?? [])])] });
    }
    return {
        Format: 'windvale-front-end-development-plan-1',
        Qualification: false,
        FrozenCases: 251,
        Cases: 251 + Products.reduce((Count, Product) => Count + Product.Cases, 0),
        Products,
    };
}


async function Requireˉplan(Plan) {
    if (!Array.isArray(Plan.Products) || Plan.Products.length < 1 ||
        Plan.Products.length > PRODUCTS.length ||
        new Set(Plan.Products.map(Product => Product.Name)).size !== Plan.Products.length ||
        Plan.Products.some(Product => !PRODUCTS.some(Owned =>
            Owned.Name === Product.Name && Owned.Project === Product.Project &&
            Owned.Validatorˉproject === Product.Validatorˉproject &&
            Owned.Cases === Product.Cases && Owned.Mode === Product.Mode &&
            Owned.ExpectedSeconds === Product.ExpectedSeconds && Owned.ExpectedExit === Product.ExpectedExit &&
            Owned.PackageProfile === Product.PackageProfile &&
            Owned.ArgumentGuardCases === Product.ArgumentGuardCases && Owned.SemanticCases === Product.SemanticCases &&
            JSON.stringify(Owned.Publicationˉminors) === JSON.stringify(Product.Publicationˉminors))) ||
        Plan.Products.some(Product => typeof Product.RequiresCurrentCompiler !== 'boolean') ||
        Plan.Cases !== 251 + Plan.Products.reduce((Count, Product) => Count + Product.Cases, 0)) {
        throw new Error('Incomplete front-end development coverage.');
    }
    const Authentic = await Readˉplan(Plan.Products.map(Product => Product.Name).join('+'));
    if (Plan.Products.some(Product => Authentic.Products.find(Owned =>
        Owned.Name === Product.Name).RequiresCurrentCompiler !== Product.RequiresCurrentCompiler ||
        JSON.stringify(Authentic.Products.find(Owned => Owned.Name === Product.Name).Inputs) !==
            JSON.stringify(Product.Inputs))) {
        throw new Error('Front-end compiler requirement or input inventory differs from the admitted project manifest.');
    }
}

export async function Executeˉplan(Plan, Operations) {
    await Requireˉplan(Plan);
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

export async function Prepareˉplan(Plan, Operations) {
    await Requireˉplan(Plan);
    if (Plan.Products.some(Product => Product.RequiresCurrentCompiler)) await Operations.Compiler();
    for (const Product of Plan.Products) await Operations.Product(Product);
}

function Publicationˉsets(Product, Outputs) {
    return Product.Publicationˉminors.map((Minor, Index) => ({
        Minor, Suffix: Index === 0 ? '' : `-${Index}`,
        Outputs: Outputs.map(Output => Index === 0 ? Output : `${Output}-${Index}`),
    }));
}

function Producedˉoutputs(Product, Outputs) {
    const Sets = Publicationˉsets(Product, Outputs);
    return [...(Sets.length === 0 ? Outputs : Sets.flatMap(Set => Set.Outputs)),
        ...(Product.Name === 'generic-wir' ? ['-predicates', '-payload', '-view-storage', '-copy', '-mutation'] :
            Product.Name === 'generic-analysis-publication' ? ['-distinct', '-result'] : [])
            .flatMap(Suffix => Outputs.map(Output => `${Output}${Suffix}`)),
        ...(Product.Name !== 'bytes-source' ? [] : [
            ...Outputs.map(Output => `${Output}-borrow`),
            ...[1, 2, 3, 4, 5, 6, 7, 9, 10, 11, 14, 15, 30, 32, 33, 34, 37]
                .flatMap(Index => Outputs.map(Output => `${Output}-source-${Index}`)),
        ])];
}

export function Parseˉarguments(Arguments) {
    var Selection = 'all';
    var Selected = false;
    var PrepareOnly = false;
    var PreparedOnly = false;
    var MaximumSeconds = null;
    for (var Index = 0; Index < Arguments.length; Index++) {
        const Argument = Arguments[Index];
        if (Argument === '--target' && !Selected && Index + 1 < Arguments.length) {
            Selection = Arguments[++Index]; Selected = true; Selectˉproducts(Selection);
        } else if (Argument === '--prepare-only' && !PrepareOnly) PrepareOnly = true;
        else if (Argument === '--prepared-products-only' && !PreparedOnly) PreparedOnly = true;
        else if (Argument === '--maximum-seconds' && MaximumSeconds === null && Index + 1 < Arguments.length) {
            const Value = Arguments[++Index];
            if (!/^[1-9][0-9]*$/u.test(Value) || !Number.isSafeInteger(Number(Value))) Usage();
            MaximumSeconds = Number(Value);
        } else Usage();
    }
    if (PrepareOnly && (PreparedOnly || MaximumSeconds === null)) Usage();
    MaximumSeconds ??= MAXIMUM_RUN_MILLISECONDS / 1_000;
    if (MaximumSeconds < 60 || MaximumSeconds > (PrepareOnly ? MAXIMUM_PREPARATION_SECONDS :
        MAXIMUM_RUN_MILLISECONDS / 1_000)) Usage();
    return { Selection, PrepareOnly, PreparedOnly, MaximumSeconds };
}

function Usage() {
    throw Object.assign(new Error('Usage: Test-Language-1.0-Front-Door-Development.mjs ' +
        '[--plan|--check-runner] or [--target <product+product|compiler-analysis>] [--prepared-products-only] ' +
        '[--maximum-seconds <60..600>] or --prepare-only --maximum-seconds <60..5400> ' +
        '[--target <product+product>]'), { exitCode: 64 });
}

async function Run(Options) {
    const { Selection, PrepareOnly, PreparedOnly, MaximumSeconds } = Options;
    if (!['win32', 'linux'].includes(process.platform)) throw new Error('Unsupported development host.');
    for (const Name of ['WINDVALE_PREPARED_COMPILER_ONLY', 'WINDVALE_PREPARED_PRODUCTS_ONLY']) {
        if (process.env[Name] !== undefined && process.env[Name] !== '1') Usage();
        if (PrepareOnly && process.env[Name] !== undefined) Usage();
        if (PreparedOnly) process.env[Name] = '1';
    }
    const Started = Date.now();
    const Deadline = Started + MaximumSeconds * 1_000;
    const Plan = await Readˉplan(Selection);
    if (process.env.WINDVALE_PREPARED_PRODUCTS_ONLY === '1' &&
        Plan.Products.some(Product => !Product.RequiresCurrentCompiler)) {
        throw Object.assign(new Error('Prepared-only Front Door execution requires Project4 products. ' +
            'The legacy project cache does not refuse construction on a miss.'), { exitCode: 64 });
    }
    const Temporary = await realpath(tmpdir());
    const Work = await realpath(await mkdtemp(join(Temporary, 'windvale-front-end-development-')));
    const Extension = WINDOWS ? 'cmd' : 'sh';
    var CatalogBytes = 0;
    var PublicationCompiler = null;
    var PublicationCompilerKey = null;
    async function Requireˉpublicationˉcompiler() {
        if (Date.now() >= Deadline) throw Object.assign(new Error('Publication admission deadline reached.'), { exitCode: 124 });
        const Key = await Getˉcurrentˉsplitˉcompilerˉkey();
        if (PublicationCompiler === null) {
            PublicationCompilerKey = Key;
            PublicationCompiler = await Readˉpreparedˉsplitˉcompiler(
                await Getˉcurrentˉsplitˉcompilerˉfamily(), Key);
        }
        if (Key !== PublicationCompilerKey) throw new Error('Publication compiler selection changed.');
        await PublicationCompiler.Requireˉunchanged();
        if (Date.now() >= Deadline) throw Object.assign(new Error('Publication admission deadline reached.'), { exitCode: 124 });
        return PublicationCompiler;
    }
    async function Command(Step, Tool, Arguments, Expected = 0, Expectedˉerror = '') {
        process.stdout.write(`START front-end development step=${Step}\n`);
        const Start = Date.now();
        const Activity = setInterval(() => {
            if (Date.now() < Deadline) process.stdout.write('INFO front-end development ' +
                `step=${Step} elapsed-ms=${Date.now() - Start} remaining-ms=${Math.max(0, Deadline - Date.now())}\n`);
        }, 30_000);
        try {
            const Result = await Runˉcommand(Tool, Arguments, Deadline, true);
            if (Result.Code !== Expected || Result.Error.replaceAll('\r\n', '\n') !== Expectedˉerror) {
                throw Object.assign(new Error(`${Step} failed (${Result.Code}): ${Result.Output}${Result.Error}`),
                    { exitCode: Number.isInteger(Result.Code) && Result.Code >= 1 && Result.Code <= 255 &&
                        Result.Code !== Expected ? Result.Code : 1 });
            }
            process.stdout.write(`PASS front-end development step=${Step} elapsed-ms=${Date.now() - Start}\n`);
            return Result;
        } finally {
            clearInterval(Activity);
        }
    }
    try {
        const Operations = {
            Compiler: async () => {
                await Command('current-compiler-preparation', process.execPath,
                    [join(NATIVE, 'Build-Current-Split-Project-Wvb.mjs'),
                        '--prepare-only', '--deadline-ms', String(Deadline)]);
            },
            Frozen: async () => {
                const Result = await Command('frozen-inputs', process.execPath,
                    [join(NATIVE, 'Verify-Language-1.0-Migration-Fixtures.mjs')]);
                if (!/^language 1 migration fixture identity status=Passed freeze-bytes=\d+ inputs=251 source-fixtures=72\r?\n$/u.test(Result.Output)) {
                    throw new Error('Frozen input coverage differs.');
                }
            },
            Product: async Product => {
                const Start = Date.now();
                const Validatorˉapplication = join(Work, `${Product.Name}-Validator.${WINDOWS ? 'exe' : 'elf'}`);
                if (Product.Validatorˉproject !== null) {
                    const Validatorˉwvb = join(Work, `${Product.Name}-Validator.wvb`);
                    await Command(`${Product.Name}-validator-build`, process.execPath,
                        [join(NATIVE, 'Build-Current-Split-Project-Wvb.mjs'), '--deadline-ms',
                            String(Deadline), join(REPOSITORY, Product.Validatorˉproject), Validatorˉwvb]);
                    await Command(`${Product.Name}-validator-package`, process.execPath,
                        [join(NATIVE, 'Build-Cached-Segmented-Hosted-Wvb.mjs'), '--deadline-ms',
                            String(Deadline), String(Product.PackageProfile), Validatorˉwvb, Validatorˉapplication]);
                }
                const Wvb = join(Work, `${Product.Name}.wvb`);
                const Paired = Product.Mode === 'paired-interpreter';
                const Builder = join(NATIVE, `${Paired ? 'Build-Wvb' : 'Build-Cached-Project-Wvb'}.${Extension}`);
                if (!Paired && Product.RequiresCurrentCompiler) {
                    await Command(`${Product.Name}-build`, process.execPath,
                        [join(NATIVE, 'Build-Current-Split-Project-Wvb.mjs'), '--deadline-ms',
                            String(Deadline), join(REPOSITORY, Product.Project), Wvb]);
                } else await Command(`${Product.Name}-build`, Builder, [join(REPOSITORY, Product.Project), Wvb]);
                const Bytes = await Readˉordinary(Wvb, MAXIMUM_PRODUCT_BYTES);
                if (Paired && !PrepareOnly) {
                    const Second = join(Work, `${Product.Name}-independent.wvb`);
                    await Command(`${Product.Name}-independent-build`, Builder, [join(REPOSITORY, Product.Project), Second]);
                    if (!Bytes.equals(await Readˉordinary(Second, MAXIMUM_PRODUCT_BYTES))) {
                        throw new Error('Independent descriptor constructions differ.');
                    }
                }
                const Native = ['native', 'publication'].includes(Product.Mode);
                const Application = join(Work, `${Product.Name}.${WINDOWS ? 'exe' : 'elf'}`);
                if (Native) await Command(`${Product.Name}-package`, process.execPath,
                    [join(NATIVE, 'Build-Cached-Segmented-Hosted-Wvb.mjs'), '--deadline-ms',
                        String(Deadline), String(Product.PackageProfile), Wvb, Application]);
                if (!PrepareOnly) {
                    const Outputs = Product.Mode === 'publication' ?
                        ['Source.wvss', 'Analysis.wvca', 'Bindings.wvlb', 'Wir.wvir']
                            .map(Name => join(Work, `${Product.Name}-${Name}`)) : [];
                    if (Product.Mode === 'publication') {
                        const Guard = await Command(`${Product.Name}-argument-guard`, Application, [], 64);
                        Requireˉexecution(Guard, 'publication', 64);
                        if (Product.Validatorˉproject !== null) {
                            Requireˉexecution(await Command(`${Product.Name}-validator-argument-guard`,
                                Validatorˉapplication, [], 64), 'publication', 64);
                            for (const Selector of Product.Name === 'bytes-source' ? ['00', '12', '38'] : []) {
                                Requireˉexecution(await Command(`${Product.Name}-validator-selection-guard-${Selector}`,
                                    Validatorˉapplication, [...Outputs, Selector], 64), 'publication', 64);
                            }
                        }
                        for (const Output of Producedˉoutputs(Product, Outputs)) {
                            const Information = await lstat(Output).catch(Error => {
                                if (Error.code === 'ENOENT') return null;
                                throw Error;
                            });
                            if (Information !== null) throw new Error('Refused publication wrote an output file.');
                        }
                    }
                    const Execution = await Command(`${Product.Name}-execute`, Native ? Application :
                        join(NATIVE, `Run-Wvb.${Extension}`), Native ? Outputs : [Wvb], Product.ExpectedExit);
                    Requireˉexecution(Execution, Native ? Product.Mode : 'interpreter', Product.ExpectedExit);
                    if (Product.Validatorˉproject !== null) {
                        if (Product.Name === 'bytes-source') {
                            for (const Index of BYTE_VALIDATION_CASES) {
                                Requireˉexecution(await Command(`${Product.Name}-validation-execute-${Index}`,
                                    Validatorˉapplication, [...Outputs, String(Index)], 42), 'native', 42);
                            }
                        } else {
                            Requireˉexecution(await Command(`${Product.Name}-validation-execute`,
                                Validatorˉapplication, Outputs, Product.ExpectedExit), 'native', Product.ExpectedExit);
                        }
                    }
                    for (const Publication of Publicationˉsets(Product, Outputs)) {
                        const Values = await Promise.all(Publication.Outputs.map(Output => Readˉordinary(Output, 4_194_304)));
                        Requireˉpublication(Values);
                        const Compiler = await Requireˉpublicationˉcompiler();
                        const Published = join(Work, `${Product.Name}-Published${Publication.Suffix}.wvb`);
                        const Admission = await Command(`${Product.Name}-publication-admission${Publication.Suffix}`,
                            join(Compiler.directory, `Emitter.${WINDOWS ? 'exe' : 'elf'}`), [...Publication.Outputs, Published]);
                        if (!/^source emission status=Published mode=optimized functions=[1-9][0-9]* code-bytes=[1-9][0-9]* module-bytes=[1-9][0-9]*\r?\n$/u.test(Admission.Output)) {
                            throw new Error('Publication emitter did not report canonical admission.');
                        }
                        const PublishedBytes = await Readˉordinary(Published, 16_777_216);
                        if (Publication.Minor !== null && (PublishedBytes.length < 8 ||
                            PublishedBytes.readUInt16LE(4) !== 1 || PublishedBytes.readUInt16LE(6) !== Publication.Minor)) {
                            throw new Error('Published WVB version differs from the source contract.');
                        }
                        if (!Admission.Output.endsWith(`module-bytes=${PublishedBytes.length}\n`) &&
                            !Admission.Output.endsWith(`module-bytes=${PublishedBytes.length}\r\n`)) {
                            throw new Error('Published WVB length differs from emitter admission.');
                        }
                        if (Product.Name === 'generic-collection-publication') {
                            // Decision 0787 retains source/IR/emission evidence for
                            // these Seed collection shapes, outside the native subset.
                            const Refusal = await Command(`${Product.Name}-published-native-subset-refusal`, process.execPath,
                                [join(NATIVE, 'Verify-Wvb.mjs'), '--current', Published], 1,
                                'wvb status=Invalid phase=semantic step=structure\n');
                            assert.equal(Refusal.Output, '', 'Native subset refusal must not publish success.');
                            process.stdout.write('INFO generic collection publication scope=source-ir-emission ' +
                                'native-collection-execution=unsupported\n');
                        } else {
                            await Command(`${Product.Name}-published-wvb-verify${Publication.Suffix}`, process.execPath,
                                [join(NATIVE, 'Verify-Wvb.mjs'), '--current', Published]);
                        }
                        for (const [Index, Output] of Publication.Outputs.entries()) {
                            if (!Values[Index].equals(await Readˉordinary(Output, 4_194_304))) {
                                throw new Error('Publication input changed during admission.');
                            }
                        }
                        await Requireˉpublicationˉcompiler();
                    }
                }
                if (Product.Name === 'generic-type-catalog') CatalogBytes = Bytes.length;
                process.stdout.write(`PASS front-end development product=${Product.Name} cases=${Product.Cases} ` +
                    `execution=${PrepareOnly ? 'skipped-preparation' : 'fresh'} ` +
                    `wvb-sha256=${createHash('sha256').update(Bytes).digest('hex')} ` +
                    `elapsed-ms=${Date.now() - Start}\n`);
            },
        };
        if (PrepareOnly) await Prepareˉplan(Plan, Operations);
        else await Executeˉplan(Plan, Operations);
    } finally {
        if (dirname(Work) !== Temporary || !basename(Work).startsWith('windvale-front-end-development-')) {
            throw new Error('Unexpected development temporary directory.');
        }
        await rm(Work, { recursive: true, force: false, maxRetries: 3, retryDelay: 100 });
    }
    if (Date.now() > Deadline) throw Object.assign(
        new Error('Front-end development deadline exceeded during cleanup.'), { exitCode: 124 });
    if (PrepareOnly) process.stdout.write('native language 1 front door preparation status=Prepared ' +
        `selection=${Selection} products=${Plan.Products.length} behavior-execution=skipped ` +
        `maximum-seconds=${MaximumSeconds} elapsed-ms=${Date.now() - Started}\n`);
    else if (Selection === 'all') process.stdout.write(
        `native language 1 front door development status=Passed cases=${Plan.Cases} frozen-inputs=251 source-fixtures=72 ` +
        'descriptor-cases=33 value-front-end-cases=39 generic-front-end-cases=4 generic-resolution-cases=1 ' +
        `generic-type-catalog-cases=1 generic-type-catalog-wvb-bytes=${CatalogBytes} bytes-source-cases=66 byte-result-admission-cases=40 ` +
        'bytes-source-argument-guard-groups=1 compiler-analysis-semantic-groups=41 compiler-analysis-argument-guards=3\n');
    else process.stdout.write(`native language 1 front door development status=Passed cases=${Plan.Cases} ` +
        `selection=${Selection} qualification=false elapsed-ms=${Date.now() - Started}\n`);
}

async function Checkˉrunner() {
    assert.equal((await Readˉplan()).Cases, 481);
    assert.equal((await Readˉplan('generic-declarations')).Cases, 254);
    assert.equal((await Readˉplan('bytes-source')).Cases, 358);
    assert.equal((await Readˉplan('bytes-source')).Products[0].RequiresCurrentCompiler, true);
    assert.equal((await Readˉplan('compiler-analysis')).Cases, 296);
    assert.equal((await Readˉplan('bytes-source+compiler-analysis')).Cases, 403);
    assert.deepEqual(Selectˉproducts('compiler-analysis').map(Product => Product.Name), COMPILER_ANALYSIS_PRODUCTS);
    const SourceAnalysis = Selectˉproducts('source-analysis')[0];
    assert.equal(SourceAnalysis.ExpectedExit, 0);
    assert.equal(SourceAnalysis.PackageProfile, 8);
    assert.equal(SourceAnalysis.Mode, 'publication');
    assert.equal(SourceAnalysis.ArgumentGuardCases, 1);
    assert.equal(SourceAnalysis.SemanticCases, 15);
    assert.equal(SourceAnalysis.Validatorˉproject, 'Projects/Tests/Language-1.0-Source-Analysis-Validation-Self-Test.wvproj');
    assert.deepEqual(Producedˉoutputs(SourceAnalysis, ['source', 'manifest', 'bindings', 'wir']), ['source', 'manifest', 'bindings', 'wir']);
    assert.deepEqual(SourceAnalysis.Publicationˉminors, []);
    assert.equal(Selectˉproducts('generic-wir')[0].ExpectedExit, 42);
    assert.equal(Selectˉproducts('generic-wir')[0].PackageProfile, 8);
    assert.equal(Selectˉproducts('generic-wir')[0].SemanticCases, 13);
    assert.deepEqual(Selectˉproducts('generic-wir')[0].Publicationˉminors, []);
    for (const [Name, Base, Count] of [['generic-wir', 'Generic-Wir', 24],
        ['generic-analysis-publication', 'Generic-Analysis', 12]]) {
        const Product = Selectˉproducts(Name)[0];
        assert.equal(Product.Validatorˉproject, `Projects/Tests/Windvale-Native-Test-Language-1-${Base}-Validation.wvproj`);
        assert.equal(Producedˉoutputs(Product, ['source', 'manifest', 'bindings', 'wir']).length, Count);
        assert((await Readˉplan(Name)).Products[0].Inputs.includes(Product.Validatorˉproject));
    }
    for (const Name of ['bytes-source', 'generic-wir', 'generic-analysis-publication', 'generic-collection-publication']) {
        const Product = Selectˉproducts(Name)[0];
        assert.equal(Product.Mode, 'publication'); assert.equal(Product.PackageProfile, 8);
        assert.equal(Product.ExpectedExit, 42); assert.equal(Product.ArgumentGuardCases, 1);
    }
    const Publicationˉpaths = ['source', 'manifest', 'bindings', 'wir'];
    const Byteˉproduct = Selectˉproducts('bytes-source')[0];
    assert.deepEqual(Publicationˉsets(Byteˉproduct, Publicationˉpaths), [44, null, null, null].map((Minor, Index) => ({
        Minor, Suffix: Index === 0 ? '' : `-${Index}`,
        Outputs: Publicationˉpaths.map(Path => Index === 0 ? Path : `${Path}-${Index}`),
    })));
    const Byteˉplan = await Readˉplan('bytes-source');
    await assert.rejects(Requireˉplan({ ...Byteˉplan,
        Products: [{ ...Byteˉplan.Products[0], Publicationˉminors: [44] }] }));
    await assert.rejects(Requireˉplan({ ...Byteˉplan,
        Products: [{ ...Byteˉplan.Products[0], Validatorˉproject: null }] }));
    assert.equal(Producedˉoutputs(Byteˉproduct, Publicationˉpaths).length, 88);
    assert.equal(new Set(Producedˉoutputs(Byteˉproduct, Publicationˉpaths)).size, 88);
    assert(Byteˉplan.Products[0].Inputs.includes(Byteˉproduct.Validatorˉproject));
    assert(Byteˉplan.Products[0].Inputs.includes('Tests/Fixtures/Language-1.0/Reserved-Byte-Validation-Self-Test.wv'));
    // Exhaustive bounded oracle: every nonempty subset, independent of input order.
    for (var Mask = 1; Mask < 2048; Mask++) {
        const Expected = PRODUCTS.filter((Product, Index) => (Mask & (1 << Index)) !== 0);
        const Selection = [...Expected].reverse().map(Product => Product.Name).join('+');
        assert.deepEqual(Selectˉproducts(Selection), Expected);
    }
    for (const Invalid of ['', 'all+descriptor', 'descriptor+descriptor', '../descriptor', 'compiler-analysis+source-analysis', 'x'.repeat(513)]) {
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
    Requireˉexecution({ Code: 0, Output: '', Error: '' }, 'native', 0);
    Requireˉexecution({ Code: 64, Output: '', Error: '' }, 'publication', 64);
    assert.throws(() => Requireˉexecution({ Code: 42, Output: '', Error: '' }, 'native', 0));
    assert.throws(() => Requireˉexecution({ Code: 0, Output: '', Error: '' }, 'publication', 64));
    const Source = Buffer.alloc(25); Source.write('WVSS'); Source.writeUInt16LE(1, 4);
    for (const [Offset, Value] of [[8, 1], [12, 8], [16, 24], [20, 1]]) Source.writeUInt32LE(Value, Offset);
    const Bindings = Buffer.alloc(32); Bindings.write('WVLB'); Bindings.writeUInt16LE(1, 4);
    const Wir = Buffer.alloc(48); Wir.write('WVIR'); Wir.writeUInt16LE(1, 4);
    const Manifest = Buffer.alloc(104); Manifest.write('WVCA'); Manifest.writeUInt16LE(1, 4);
    for (const [Offset, Value] of [[8, 104], [12, 25], [16, 32], [20, 48], [24, 1]]) Manifest.writeUInt32LE(Value, Offset);
    const Publication = [Source, Manifest, Bindings, Wir];
    Requireˉpublication(Publication);
    assert.throws(() => Requireˉpublication(Publication.slice(1)));
    for (const [Index, Offset] of [[0, 16], [1, 12], [1, 100], [2, 0], [3, 0], [3, 8]]) {
        const Invalid = Publication.map(Value => Buffer.from(Value)); Invalid[Index][Offset] ^= 1;
        assert.throws(() => Requireˉpublication(Invalid));
    }
    assert.throws(() => Requireˉpublication([Source, Manifest.subarray(0, 103), Bindings, Wir]));
    assert.throws(() => Requireˉpublication([Buffer.alloc(4_194_305), Manifest, Bindings, Wir]));
    for (const Version of [1, 2]) for (const Modules of [1, 5, 64]) {
        const Entryˉbytes = Version === 2 ? 20 : 8, Payload = 16 + Modules * Entryˉbytes;
        const Graph = Buffer.alloc(Payload + Modules);
        Graph.write('WVSS'); Graph.writeUInt16LE(Version, 4);
        Graph.writeUInt32LE(Modules, 8); Graph.writeUInt32LE(Modules * Entryˉbytes, 12);
        for (var Module = 0; Module < Modules; Module++) {
            const Entry = 16 + Module * Entryˉbytes;
            Graph.writeUInt32LE(Payload + Module, Entry); Graph.writeUInt32LE(1, Entry + 4);
            if (Version === 2) for (const [Offset, Value] of [[8, 1], [12, 1], [16, 12]]) {
                Graph.writeUInt32LE(Value, Entry + Offset);
            }
        }
        const Graphˉmanifest = Buffer.from(Manifest);
        Graphˉmanifest.writeUInt32LE(Graph.length, 12); Graphˉmanifest.writeUInt32LE(Modules, 24);
        const Graphˉpublication = [Graph, Graphˉmanifest, Bindings, Wir];
        Requireˉpublication(Graphˉpublication);
        const Changes = [[8, 0], [8, 65], [8, 0xffffffff], [12, 0xffffffff],
            [16, Payload + 1], [20, 0], [20, 0xffffffff]];
        if (Version === 2) Changes.push([24, 2], [28, 0], [32, 0], [32, 130]);
        for (const [Offset, Value] of Changes) {
            const Invalid = Buffer.from(Graph); Invalid.writeUInt32LE(Value, Offset);
            assert.throws(() => Requireˉpublication([Invalid, ...Graphˉpublication.slice(1)]));
        }
        for (const Length of [0, 1, 15, Payload - 1, Graph.length - 1]) {
            assert.throws(() => Requireˉpublication([Graph.subarray(0, Length), ...Graphˉpublication.slice(1)]));
        }
        assert.throws(() => Requireˉpublication([Buffer.concat([Graph, Buffer.alloc(1)]), ...Graphˉpublication.slice(1)]));
        const Wrongˉcount = Buffer.from(Graphˉmanifest); Wrongˉcount.writeUInt32LE(Modules + 1, 24);
        assert.throws(() => Requireˉpublication([Graph, Wrongˉcount, Bindings, Wir]));
    }
    const Plan = await Readˉplan();
    var Runs = 0;
    const Operations = { Frozen: async () => {}, Product: async () => { Runs++; } };
    await Executeˉplan(Plan, Operations);
    await Executeˉplan(Plan, Operations);
    assert.equal(Runs, 22, 'Product reuse must not skip behavior execution.');
    const Prepared = [];
    await Prepareˉplan(Plan, { Compiler: async () => { Prepared.push('compiler'); },
        Frozen: async () => { assert.fail('Preparation must not verify frozen behavior.'); },
        Product: async Product => { Prepared.push(Product.Name); } });
    assert.deepEqual(Prepared, ['compiler', ...Plan.Products.map(Product => Product.Name)]);
    await assert.rejects(() => Prepareˉplan(Plan, { Compiler: async () => {
        throw new Error('Seeded preparation failure'); }, Product: async () => {
        assert.fail('Missing compiler must stop preparation'); } }));
    const Forged = { ...Plan, Products: Plan.Products.map((Product, Index) => Index === 0
        ? { ...Product, RequiresCurrentCompiler: !Product.RequiresCurrentCompiler } : Product) };
    await assert.rejects(() => Executeˉplan(Forged, Operations), /compiler requirement or input inventory differs/u);
    await assert.rejects(() => Prepareˉplan(Forged, { Compiler: async () => {
        assert.fail('Forged compiler requirement must reject before preparation'); },
    Product: async () => { assert.fail('Forged plan must not construct products'); } }),
    /compiler requirement or input inventory differs/u);
    assert.deepEqual(Parseˉarguments(['--prepare-only', '--maximum-seconds', '1800', '--target', 'bytes-source']),
        { Selection: 'bytes-source', PrepareOnly: true, PreparedOnly: false, MaximumSeconds: 1800 });
    assert.deepEqual(Parseˉarguments(['--target', 'bytes-source', '--prepared-products-only']),
        { Selection: 'bytes-source', PrepareOnly: false, PreparedOnly: true, MaximumSeconds: 600 });
    for (const Arguments of [['--prepare-only'], ['--prepare-only', '--maximum-seconds', '5401'],
        ['--maximum-seconds', '601'], ['--maximum-seconds', '1'], ['--maximum-seconds', '1e3'],
        ['--prepare-only', '--maximum-seconds', '1800', '--prepared-products-only'],
        ['--target', 'bytes-source', '--target', 'descriptor']]) assert.throws(() => Parseˉarguments(Arguments));
    for (const Field of ['ExpectedExit', 'PackageProfile', 'SemanticCases', 'ArgumentGuardCases']) {
        const Changed = { ...Plan, Products: Plan.Products.map((Product, Index) => Index === 7 ?
            { ...Product, [Field]: Product[Field] + 1 } : Product) };
        await assert.rejects(() => Executeˉplan(Changed, Operations), /Incomplete/u);
    }
    const WrongInputs = { ...Plan, Products: Plan.Products.map((Product, Index) => Index === 7 ?
        { ...Product, Inputs: Product.Inputs.slice(1) } : Product) };
    await assert.rejects(() => Executeˉplan(WrongInputs, Operations), /input inventory differs/u);
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
    process.stdout.write('front-end development runner checks status=Passed products=11 subsets=2047\n');
}

if (process.argv[1] && resolve(process.argv[1]) === SCRIPT_PATH) {
    try {
        const Arguments = process.argv.slice(2);
        if (Arguments.length === 1 && Arguments[0] === '--plan') {
            process.stdout.write(`${JSON.stringify(await Readˉplan())}\n`);
        } else if (Arguments.length === 1 && Arguments[0] === '--check-runner') {
            await Checkˉrunner();
        } else await Run(Parseˉarguments(Arguments));
    } catch (Error) {
        process.stderr.write(`${Error.message.slice(0, MAXIMUM_OUTPUT_BYTES)}\n`);
        process.exitCode = Error.exitCode ?? 1;
    }
}
