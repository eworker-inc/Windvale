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
            'Compilerˉsourceˉbindingsˉdirectoryˉisˉvalid',
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
        roots: [
            'Compilerˉsourceˉbindingsˉclosuresˉbaseˉfunctions',
            'Compilerˉsourceˉbindingsˉclosuresˉcatalog',
            'Compilerˉsourceˉbindingsˉclosuresˉdirectoryˉisˉvalid',
            'Compilerˉsourceˉbindingsˉclosuresˉentriesˉoffset',
            'Compilerˉsourceˉbindingsˉclosuresˉfunctionˉcatalog',
            'Compilerˉsourceˉbindingsˉclosuresˉheaderˉbytes',
            'Compilerˉsourceˉbindingsˉclosuresˉrangeˉoffset',
            'Compilerˉsourceˉbindingsˉclosuresˉranges',
            'Compilerˉsourceˉbindingsˉclosuresˉtypeˉcatalog'
        ]
    },
    {
        source: 'Compiler/Windvale/Source-Bindings-Generic-Types-Core.wv',
        output: 'Compiler/Windvale/Source-Bindings-Generic-Types-Artifact-Core.wv',
        prefix: 'Compilerˉsourceˉbindingsˉgenericˉtypesˉ',
        roots: [
            'Compilerˉsourceˉbindingsˉgenericˉtypesˉdirectoryˉisˉvalid',
            'Compilerˉsourceˉbindingsˉgenericˉtypesˉfunctionˉcatalog',
            'Compilerˉsourceˉbindingsˉgenericˉtypesˉtypeˉcatalog'
        ]
    },
    {
        source: 'Compiler/Windvale/Source-Closure-Captures-Core.wv',
        output: 'Compiler/Windvale/Source-Closure-Captures-Artifact-Core.wv',
        prefix: 'Compilerˉsourceˉclosureˉcapture',
        transform: 'artifact-closure-validation',
        roots: [
            'Compilerˉsourceˉclosureˉcapturesˉvalidateˉrangeˉwithˉeffects',
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
        'Compiler/Windvale/Source-Wvb-Temporary-Slots-Artifact.wv']
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
            text: Lines.slice(Start, Index + 1).join('\n')
        });
    }
    return { Lines, Functions, Firstˉfunction };
}

function Selectˉfunctions(Functions, Roots) {
    const Selected = new Set();
    const Queue = [...Roots];
    const Reference = /([\p{L}\p{N}ˉ_]+)\s*\(/gu;
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
        for (const Match of Function.text.matchAll(Reference)) {
            if (Functions.has(Match[1]) && !Selected.has(Match[1])) {
                Queue.push(Match[1]);
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

function Escapeˉregularˉexpression(Value) {
    return Value.replace(/[.*+?^${}()|[\]\\]/gu, '\\$&');
}

function Projectˉroots(Reader, Functions, Module) {
    if (Reader.project === undefined) {
        return [];
    }
    const Roots = new Set();
    const Import = new RegExp(
        `^import ${Escapeˉregularˉexpression(Module)} as ([^;]+);$`,
        'mu'
    );
    for (const Relativeˉpath of Projectˉpaths(Reader.project)) {
        if (Relativeˉpath === Reader.source || Relativeˉpath === Reader.output) {
            continue;
        }
        const Sourceˉpath = path.join(Repositoryˉroot, Relativeˉpath);
        if (!fs.existsSync(Sourceˉpath)) {
            throw new Error(`Project source is absent: ${Relativeˉpath}`);
        }
        const Source = fs.readFileSync(Sourceˉpath, 'utf8').replaceAll('\r\n', '\n');
        const Importˉmatch = Source.match(Import);
        if (Importˉmatch === null) {
            continue;
        }
        const Alias = Importˉmatch[1].trim();
        const Reference = new RegExp(
            `${Escapeˉregularˉexpression(Alias)}\\.([\\p{L}\\p{N}ˉ_]+)\\s*\\(`,
            'gu'
        );
        for (const Match of Source.matchAll(Reference)) {
            if (Functions.has(Match[1])) {
                Roots.add(Match[1]);
            }
        }
    }
    return [...Roots];
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
    const Lateˉdeclaration = Parsed.Lines.slice(Parsed.Firstˉfunction + 1)
        .find((Line) => /^(?:const |(?:export )?(?:record|enum) )/u.test(Line));
    if (Lateˉdeclaration !== undefined) {
        throw new Error(
            `Artifact reader has a declaration after its first function: ${Reader.source}`
        );
    }
    const Moduleˉmatch = Source.match(/^module ([^ ]+) profile /mu);
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
    const Functions = Compactˉgeneratedˉsource(
        Selected.map((Function) => Function.text).join('\n')
    );
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
