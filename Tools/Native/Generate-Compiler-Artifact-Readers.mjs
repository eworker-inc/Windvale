import fs from 'node:fs';
import path from 'node:path';
import process from 'node:process';
import { fileURLToPath } from 'node:url';

const Scriptˉdirectory = path.dirname(fileURLToPath(import.meta.url));
const Repositoryˉroot = path.resolve(Scriptˉdirectory, '..', '..');
const Checkˉonly = process.argv.includes('--check');
const Emitterˉproject =
    'Projects/Tools/Windvale-Compiler-Emission-Driver.wvproj';

const Readers = [
    {
        source: 'Compiler/Windvale/Source-Bindings-Core.wv',
        output: 'Compiler/Windvale/Source-Bindings-Artifact-Core.wv',
        prefix: 'Compilerˉsourceˉbindingsˉ',
        roots: [
            'Compilerˉsourceˉbindingsˉshape',
            'Compilerˉsourceˉbindingsˉcapabilityˉarity',
            'Compilerˉsourceˉbindingsˉrangeˉdeclaration',
            'Compilerˉsourceˉbindingsˉrangesˉcount',
            'Compilerˉsourceˉbindingsˉentriesˉoffset',
            'Compilerˉsourceˉbindingsˉrangeˉoffset',
            'Compilerˉsourceˉbindingsˉidentifierˉisˉvalid',
            'Compilerˉsourceˉbindingsˉphaseˉempty',
            'Compilerˉsourceˉbindingsˉfindˉlocal',
            'Compilerˉsourceˉbindingsˉpriorˉlocal',
            'Compilerˉsourceˉbindingsˉphaseˉbuild',
            'Compilerˉsourceˉbindingsˉphaseˉbuildˉvalue',
            'Compilerˉsourceˉbindingsˉappendˉparameter',
            'Compilerˉsourceˉbindingsˉappendˉcapture'
        ]
    },
    {
        source: 'Compiler/Windvale/Source-Bindings-Closures-Core.wv',
        output: 'Compiler/Windvale/Source-Bindings-Closures-Artifact-Core.wv',
        prefix: 'Compilerˉsourceˉbindingsˉclosuresˉ',
        roots: ['Compilerˉsourceˉbindingsˉclosuresˉdeclaration']
    },
    {
        source: 'Compiler/Windvale/Source-Closure-Captures-Core.wv',
        output: 'Compiler/Windvale/Source-Closure-Captures-Artifact-Core.wv',
        prefix: 'Compilerˉsourceˉclosureˉcapture',
        transform: 'artifact-closure-validation',
        roots: [
            'Compilerˉsourceˉclosureˉcapturesˉvalidateˉrangeˉwithˉeffects',
            'Compilerˉsourceˉclosureˉcaptureˉempty',
            'Compilerˉsourceˉclosureˉcaptureˉfailure',
            'Compilerˉsourceˉclosureˉcaptureˉmodeˉat',
            'Compilerˉsourceˉclosureˉcaptureˉslotˉat'
        ]
    },
    {
        source: 'Compiler/Windvale/Source-Wvb-Core.wv',
        output: 'Compiler/Windvale/Source-Wvb-Artifact-Core.wv',
        prefix: 'Compilerˉ',
        project: Emitterˉproject,
        roots: []
    },
    {
        source: 'Compiler/Windvale/Source-Effects-Core.wv',
        output: 'Compiler/Windvale/Source-Effects-Artifact-Core.wv',
        prefix: 'Compilerˉsourceˉeffect',
        project: Emitterˉproject,
        roots: []
    },
    {
        source: 'Compiler/Windvale/Source-Function-Type-Lowering-Core.wv',
        output: 'Compiler/Windvale/Source-Function-Type-Lowering-Artifact-Core.wv',
        prefix: 'Compilerˉsourceˉfunctionˉtypeˉ',
        project: Emitterˉproject,
        roots: []
    },
    {
        source: 'Compiler/Windvale/Source-Symbols-Core.wv',
        output: 'Compiler/Windvale/Source-Symbols-Artifact-Core.wv',
        prefix: 'Compilerˉ',
        project: Emitterˉproject,
        roots: []
    },
    {
        source: 'Compiler/Windvale/Source-Generic-Lowering-Core.wv',
        output: 'Compiler/Windvale/Source-Generic-Lowering-Artifact-Core.wv',
        prefix: 'Compilerˉsourceˉgenericˉ',
        project: Emitterˉproject,
        roots: []
    },
    {
        source: 'Compiler/Windvale/Source-Declaration-Parser.wv',
        output: 'Compiler/Windvale/Source-Declaration-Parser-Artifact.wv',
        prefix: 'Compilerˉ',
        project: Emitterˉproject,
        roots: []
    },
    {
        source: 'Compiler/Windvale/Source-Closure-Lowering-Core.wv',
        output: 'Compiler/Windvale/Source-Closure-Lowering-Artifact-Core.wv',
        prefix: 'Compilerˉsourceˉclosureˉ',
        project: Emitterˉproject,
        roots: []
    }
];

for (const [Source, Output] of [
    ['Compiler/Windvale/Source-Body-Parser.wv',
        'Compiler/Windvale/Source-Body-Parser-Artifact.wv'],
    ['Compiler/Windvale/Source-Graph-Core.wv',
        'Compiler/Windvale/Source-Graph-Artifact-Core.wv'],
    ['Compiler/Windvale/Source-Wvb-Temporary-Slots.wv',
        'Compiler/Windvale/Source-Wvb-Temporary-Slots-Artifact.wv'],
    ['Compiler/Windvale/Source-Wir-Consumer-Core.wv',
        'Compiler/Windvale/Source-Wir-Consumer-Artifact-Core.wv'],
    ['Compiler/Windvale/Source-Lexer-Core.wv',
        'Compiler/Windvale/Source-Lexer-Artifact-Core.wv'],
    ['Compiler/Windvale/Source-Generic-Type-Layout-Core.wv',
        'Compiler/Windvale/Source-Generic-Type-Layout-Artifact-Core.wv'],
    ['Compiler/Windvale/Source-Generic-Type-Lowering-Core.wv',
        'Compiler/Windvale/Source-Generic-Type-Lowering-Artifact-Core.wv']
]) {
    Readers.push({
        source: Source,
        output: Output,
        prefix: 'Compilerˉ',
        project: Emitterˉproject,
        roots: []
    });
}

function Countˉbraces(Line) {
    let Open = 0;
    let Close = 0;
    let Quoted = false;
    let Escaped = false;
    for (let Index = 0; Index < Line.length; Index += 1) {
        const Character = Line[Index];
        if (!Quoted && Character === '/' && Line[Index + 1] === '/') {
            break;
        }
        if (Quoted) {
            if (Escaped) {
                Escaped = false;
            } else if (Character === '\\') {
                Escaped = true;
            } else if (Character === '"') {
                Quoted = false;
            }
            continue;
        }
        if (Character === '"') {
            Quoted = true;
        } else if (Character === '{') {
            Open += 1;
        } else if (Character === '}') {
            Close += 1;
        }
    }
    return { Open, Close };
}

function Parseˉfunctions(Source) {
    const Lines = Source.split('\n');
    const Functions = new Map();
    let Firstˉfunction = Lines.length;
    for (let Index = 0; Index < Lines.length; Index += 1) {
        const Match = Lines[Index].match(
            /^(?:export )?fn ([\p{L}\p{N}ˉ_]+)\(/u
        );
        if (Match === null) {
            continue;
        }
        Firstˉfunction = Math.min(Firstˉfunction, Index);
        const Start = Index;
        let Depth = 0;
        let Bodyˉstarted = false;
        for (; Index < Lines.length; Index += 1) {
            const Braces = Countˉbraces(Lines[Index]);
            if (Braces.Open > 0) {
                Bodyˉstarted = true;
            }
            Depth += Braces.Open - Braces.Close;
            if (Bodyˉstarted && Depth === 0) {
                break;
            }
        }
        if (!Bodyˉstarted || Depth !== 0) {
            throw new Error(`Unterminated function ${Match[1]}`);
        }
        Functions.set(Match[1].trim(), {
            start: Start,
            end: Index,
            text: Lines.slice(Start, Index + 1).join('\n')
        });
    }
    return { Lines, Functions, Firstˉfunction };
}

function Selectˉfunctions(Functions, Roots) {
    const Selected = new Set();
    const Queue = [...Roots];
    // A referenced function may be a callback value rather than a direct call.
    const Reference = /[\p{L}\p{N}ˉ_]+/gu;
    while (Queue.length > 0) {
        const Name = Queue.shift();
        if (Selected.has(Name)) {
            continue;
        }
        const Function = Functions.get(Name);
        if (Function === undefined) {
            throw new Error(`Required function is absent: ${Name}`);
        }
        Selected.add(Name);
        for (const Match of Maskˉsource(Function.text).matchAll(Reference)) {
            if (Functions.has(Match[0]) && !Selected.has(Match[0])) {
                Queue.push(Match[0]);
            }
        }
    }
    return [...Selected]
        .map((Name) => Functions.get(Name))
        .sort((Left, Right) => Left.start - Right.start);
}

function Projectˉpaths(Project) {
    const Projectˉpath = path.join(Repositoryˉroot, Project);
    const Text = fs.readFileSync(Projectˉpath, 'utf8').replaceAll('\r\n', '\n');
    const Paths = [];
    for (const Match of Text.matchAll(/^(?:root|source) "([^"]+)"$/gmu)) {
        Paths.push(Match[1]);
    }
    if (Paths.length === 0) {
        throw new Error(`Project has no source paths: ${Project}`);
    }
    return Paths;
}

function Maskˉsource(Source) {
    return Source.replace(
        /\/\*[\s\S]*?\*\/|\/\/[^\n]*|"(?:\\.|[^"\\])*"/gu,
        (Value) => Value.replace(/[^\n]/gu, ' ')
    );
}

const Projectˉclosures = new Map();

function Projectˉclosure(Project) {
    if (Projectˉclosures.has(Project)) {
        return Projectˉclosures.get(Project);
    }
    const Modules = new Map();
    const Functions = new Map();
    let Rootˉmodule;
    for (const Relativeˉpath of Projectˉpaths(Project)) {
        const Reader = Readers.find((Candidate) =>
            Candidate.output === Relativeˉpath || Candidate.source === Relativeˉpath
        );
        const Sourceˉpath = path.join(
            Repositoryˉroot, Reader?.source ?? Relativeˉpath
        );
        if (!fs.existsSync(Sourceˉpath)) {
            throw new Error(`Project source is absent: ${Relativeˉpath}`);
        }
        const Source = fs.readFileSync(Sourceˉpath, 'utf8').replaceAll('\r\n', '\n');
        const Clean = Maskˉsource(Source);
        const Module = Clean.match(/^module ([^\s;]+);$/mu)?.[1];
        if (Module === undefined || Modules.has(Module)) {
            throw new Error(`Project module is absent or duplicated: ${Relativeˉpath}`);
        }
        Rootˉmodule ??= Module;
        const Imports = new Map([...Clean.matchAll(
            /^import ([^\s;]+) as ([^\s;]+);$/gmu
        )].map((Match) => [Match[2], Match[1]]));
        const Parsed = Parseˉfunctions(Source);
        if (Reader?.transform === 'artifact-closure-validation') {
            Transformˉartifactˉclosureˉvalidation(Parsed.Functions);
        }
        const Declarations = [];
        let Cursor = 0;
        for (const Function of Parsed.Functions.values()) {
            Declarations.push(...Parsed.Lines.slice(Cursor, Function.start));
            Cursor = Function.end + 1;
        }
        Declarations.push(...Parsed.Lines.slice(Cursor));
        Modules.set(Module, {
            Imports,
            Functions: Parsed.Functions,
            Declarations: Declarations.join('\n')
        });
        for (const [Name, Function] of Parsed.Functions) {
            Functions.set(`${Module}.${Name}`, { Module, Function });
        }
    }
    const Root = `${Rootˉmodule}.Main`;
    if (!Functions.has(Root)) {
        throw new Error(`Project root Main is absent: ${Project}`);
    }
    const Selected = new Set();
    const Pending = [Root];
    const Reference = /(?<![\p{L}\p{N}ˉ_])([\p{L}\p{N}ˉ_]+)(?:\s*\.\s*([\p{L}\p{N}ˉ_]+))?/gu;
    // Every declaration is retained, including any function-valued initializer.
    for (const [Module, Entry] of Modules) {
        for (const Match of Maskˉsource(Entry.Declarations).matchAll(Reference)) {
            const Owner = Match[2] === undefined
                ? Module : Entry.Imports.get(Match[1]);
            const Target = `${Owner}.${Match[2] ?? Match[1]}`;
            if (Functions.has(Target)) {
                Pending.push(Target);
            }
        }
    }
    while (Pending.length > 0) {
        const Key = Pending.pop();
        if (Selected.has(Key)) {
            continue;
        }
        Selected.add(Key);
        const Entry = Functions.get(Key);
        const Imports = Modules.get(Entry.Module).Imports;
        for (const Match of Maskˉsource(Entry.Function.text).matchAll(Reference)) {
            const Module = Match[2] === undefined
                ? Entry.Module : Imports.get(Match[1]);
            const Target = `${Module}.${Match[2] ?? Match[1]}`;
            if (Functions.has(Target) && !Selected.has(Target)) {
                Pending.push(Target);
            }
        }
    }
    Projectˉclosures.set(Project, Selected);
    return Selected;
}

function Projectˉroots(Reader, Functions, Module) {
    if (Reader.project === undefined) {
        return [];
    }
    const Selected = Projectˉclosure(Reader.project);
    return [...Functions.keys()].filter((Name) => Selected.has(`${Module}.${Name}`));
}

function Transformˉartifactˉclosureˉvalidation(Functions) {
    const Name =
        'Compilerˉsourceˉclosureˉcapturesˉvalidateˉrangeˉwithˉeffects';
    const Function = Functions.get(Name);
    if (Function === undefined) {
        throw new Error(`Required function is absent: ${Name}`);
    }
    const Bodyˉstart = Function.text.indexOf('    let Bodyˉposition:');
    const Effectˉstart = Function.text.indexOf(
        '    if Effectˉstatus !=',
        Bodyˉstart
    );
    if (Bodyˉstart < 0 || Effectˉstart < 0) {
        throw new Error(`Artifact closure validation markers changed: ${Name}`);
    }
    Function.text = Function.text.slice(0, Bodyˉstart) +
        '    // The producer already bound the closure body. The artifact target\n' +
        '    // reconstructs its capture and parameter front door here; typed-WIR\n' +
        '    // validation independently proves every emitted body operation.\n' +
        Function.text.slice(Effectˉstart);
}

function Compactˉgeneratedˉsource(Source) {
    return Source.split('\n')
        .filter((Line) => {
            const Trimmed = Line.trim();
            return Trimmed.length > 0 && !Trimmed.startsWith('//');
        })
        .map((Line) => Line.trim())
        .join('\n');
}

function Generateˉreader(Reader) {
    const Sourceˉpath = path.join(Repositoryˉroot, Reader.source);
    const Source = fs.readFileSync(Sourceˉpath, 'utf8').replaceAll('\r\n', '\n');
    const Parsed = Parseˉfunctions(Source);
    const Moduleˉmatch = Source.match(/^module ([^\s;]+)(?: profile (?:portable|hosted|system))?;$/mu);
    if (Moduleˉmatch === null) {
        throw new Error(`Module declaration is absent: ${Reader.source}`);
    }
    if (Reader.transform === 'artifact-closure-validation') {
        Transformˉartifactˉclosureˉvalidation(Parsed.Functions);
    }
    const Roots = [
        ...Reader.roots,
        ...Projectˉroots(Reader, Parsed.Functions, Moduleˉmatch[1])
    ];
    if (Roots.length === 0) {
        throw new Error(`Artifact reader has no callable roots: ${Reader.source}`);
    }
    const Selected = Selectˉfunctions(Parsed.Functions, Roots);
    const Header = Compactˉgeneratedˉsource(
        Parsed.Lines.slice(0, Parsed.Firstˉfunction).join('\n')
    );
    // Preserve declarations between functions as well as the initial header.
    const Selectedˉstarts = new Set(Selected.map((Function) => Function.start));
    const Body = [];
    let Cursor = Parsed.Firstˉfunction;
    for (const Function of Parsed.Functions.values()) {
        Body.push(...Parsed.Lines.slice(Cursor, Function.start));
        if (Selectedˉstarts.has(Function.start)) {
            Body.push(Function.text);
        }
        Cursor = Function.end + 1;
    }
    Body.push(...Parsed.Lines.slice(Cursor));
    const Functions = Compactˉgeneratedˉsource(Body.join('\n'));
    return `${Header}\n` +
        '// Generated by Tools/Native/Generate-Compiler-Artifact-Readers.mjs.\n' +
        '// This compact target-specific implementation retains only artifact contracts and validators.\n' +
        `${Functions}\n`;
}

let Mismatches = 0;
for (const Reader of Readers) {
    const Outputˉpath = path.join(Repositoryˉroot, Reader.output);
    const Expected = Generateˉreader(Reader);
    if (Checkˉonly) {
        const Actual = fs.existsSync(Outputˉpath)
            ? fs.readFileSync(Outputˉpath, 'utf8').replaceAll('\r\n', '\n')
            : '';
        if (Actual !== Expected) {
            console.error(`artifact-reader mismatch path=${Reader.output}`);
            Mismatches += 1;
        }
    } else {
        fs.writeFileSync(Outputˉpath, Expected, 'utf8');
        console.log(`artifact-reader generated path=${Reader.output}`);
    }
}

if (Mismatches > 0) {
    process.exitCode = 1;
} else if (Checkˉonly) {
    console.log(`artifact-reader status=Passed files=${Readers.length}`);
}
