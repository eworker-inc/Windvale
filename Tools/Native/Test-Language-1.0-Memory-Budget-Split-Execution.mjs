import { spawnSync } from 'node:child_process';
import { createHash } from 'node:crypto';
import {
    lstatSync,
    mkdtempSync,
    readFileSync,
    realpathSync,
    rmSync,
    writeFileSync,
} from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const MAXIMUM_DIAGNOSTIC_BYTES = 1_048_576;
const MAXIMUM_WVB_BYTES = 16_777_216;
const TOOL_TIMEOUT_MILLISECONDS = 600_000;
const SOURCE_LOCK_SHA256 =
    '9e2ca572552ed52ed496142d18539f2f55fed2bbdfb1ec602f283b5d72386f3e';
const BOOTSTRAP_ANALYZER_SHA256 =
    '26ea9bccfe8c2763fb887a5a14c2f0a086a27265523c3df84187b361616f9120';
const BOOTSTRAP_EMITTER_SHA256 =
    'ea8ade4774236a84208242a6e17d271077b9a4a94fb40c47ec487d43a97b2b94';
const EXPECTED_SUCCESS_SHA256 =
    '5678409a9b9bba47dd37a6f3d26f0666a7c27d2e86d6ff320a78b8fdcbec8f53';

if (process.argv.length !== 2) {
    process.stderr.write(
        'Usage: node Tools/Native/Test-Language-1.0-Memory-Budget-Split-Execution.mjs\n',
    );
    process.exit(64);
}
if (process.platform !== 'win32' && process.platform !== 'linux') {
    Reject(`Unsupported test host: ${process.platform}.`);
}

const Scriptˉdirectory = path.dirname(fileURLToPath(import.meta.url));
const Repositoryˉroot = realpathSync(path.resolve(Scriptˉdirectory, '..', '..'));
const Profileˉroot = path.join(
    Repositoryˉroot,
    'Documents', 'Project', 'Language-1.0-Localization-Workloads',
    '01-Source-Profile-Admission', 'Reference-Artifacts',
);
const Sourceˉlock = path.join(Profileˉroot, 'Source-Inputs.wvlock');
const Sourceˉprofile = path.join(Profileˉroot, 'En-Source-Profile.wvsp');
const Bootstrapˉanalyzerˉwvb = path.join(
    Repositoryˉroot, 'Artifacts', 'Language-1.0-Target-Aware-Emission-Bootstrap',
    'Wvb', 'wvanalyze.wvb',
);
const Bootstrapˉemitterˉwvb = path.join(
    Repositoryˉroot, 'Artifacts', 'Language-1.0-Target-Aware-Emission-Bootstrap',
    'Wvb', 'wvemit.wvb',
);
const Work = mkdtempSync(path.join(
    os.tmpdir(), 'windvale-memory-budget-split-execution-',
));
let Step = 0;

try {
    Requireˉordinaryˉfile(Sourceˉlock, 4_194_304, 'source lock');
    Requireˉordinaryˉfile(Sourceˉprofile, 4_194_304, 'source profile');
    Requireˉexactˉfile(
        Bootstrapˉanalyzerˉwvb, 992_412, BOOTSTRAP_ANALYZER_SHA256,
        'bootstrap analyzer',
    );
    Requireˉexactˉfile(
        Bootstrapˉemitterˉwvb, 895_787, BOOTSTRAP_EMITTER_SHA256,
        'bootstrap emitter',
    );

    const Executableˉsuffix = process.platform === 'win32' ? '.exe' : '.elf';
    const Target = process.platform === 'win32' ? 'windows' : 'linux';
    const Bootstrapˉanalyzer = path.join(Work, `Bootstrap-Analyzer${Executableˉsuffix}`);
    const Bootstrapˉemitter = path.join(Work, `Bootstrap-Emitter${Executableˉsuffix}`);
    const Bootstrapˉanalyzerˉidentity = path.join(Work, 'Bootstrap-Analyzer.identity');
    const Bootstrapˉemitterˉidentity = path.join(Work, 'Bootstrap-Emitter.identity');
    const Admitterˉwvb = path.join(Work, 'Admitter.wvb');
    const Analyzerˉwvb = path.join(Work, 'Analyzer.wvb');
    const Emitterˉwvb = path.join(Work, 'Emitter.wvb');
    const Admitter = path.join(Work, `Admitter${Executableˉsuffix}`);
    const Analyzer = path.join(Work, `Analyzer${Executableˉsuffix}`);
    const Emitter = path.join(Work, `Emitter${Executableˉsuffix}`);
    const Analyzerˉidentity = path.join(Work, 'Analyzer.identity');
    const Emitterˉidentity = path.join(Work, 'Emitter.identity');

    Runˉnative('bootstrap-analyzer-package', 'Package-Segmented-Compiler-Wvb', [
        '7', Bootstrapˉanalyzerˉwvb, Bootstrapˉanalyzer, '--development-cache',
    ]);
    Runˉnative('bootstrap-emitter-package', 'Package-Segmented-Compiler-Wvb', [
        '7', Bootstrapˉemitterˉwvb, Bootstrapˉemitter, '--development-cache',
    ]);
    Runˉnode('bootstrap-analyzer-identity', 'Write-Split-Compiler-Producer-Identity.mjs', [
        'analyzer', Bootstrapˉanalyzer, Bootstrapˉanalyzerˉidentity,
    ]);
    Runˉnode('bootstrap-emitter-identity', 'Write-Split-Compiler-Producer-Identity.mjs', [
        'emitter', Bootstrapˉemitter, Bootstrapˉemitterˉidentity,
    ]);
    Runˉnode('current-admitter-build', 'Build-Cached-Split-Project-Wvb.mjs', [
        Project('Windvale-Compiler-Admission-Driver.wvproj'), Admitterˉwvb,
        Bootstrapˉanalyzer, Bootstrapˉanalyzerˉidentity,
        Bootstrapˉemitter, Bootstrapˉemitterˉidentity,
    ]);
    Runˉnode('current-analyzer-build', 'Build-Cached-Split-Project-Wvb.mjs', [
        Project('Windvale-Compiler-Analysis-Driver.wvproj'), Analyzerˉwvb,
        Bootstrapˉanalyzer, Bootstrapˉanalyzerˉidentity,
        Bootstrapˉemitter, Bootstrapˉemitterˉidentity,
    ]);
    Runˉnative('current-admitter-package', 'Package-Segmented-Compiler-Wvb', [
        '2', Admitterˉwvb, Admitter, '--development-cache',
    ]);
    Runˉnative('current-analyzer-package', 'Package-Segmented-Compiler-Wvb', [
        '7', Analyzerˉwvb, Analyzer, '--development-cache',
    ]);
    Runˉnode('current-analyzer-identity', 'Write-Split-Compiler-Producer-Identity.mjs', [
        'analyzer', Analyzer, Analyzerˉidentity,
    ]);
    Runˉnode('current-emitter-build', 'Build-Cached-Split-Project-Wvb.mjs', [
        Project('Windvale-Compiler-Emission-Driver.wvproj'), Emitterˉwvb,
        Analyzer, Analyzerˉidentity,
        Bootstrapˉemitter, Bootstrapˉemitterˉidentity,
    ]);
    Runˉnative('current-emitter-package', 'Package-Segmented-Compiler-Wvb', [
        '7', Emitterˉwvb, Emitter, '--development-cache',
    ]);
    Runˉnode('current-emitter-identity', 'Write-Split-Compiler-Producer-Identity.mjs', [
        'emitter', Emitter, Emitterˉidentity,
    ]);

    const Successˉa = path.join(Work, 'Success-A.wvb');
    const Successˉb = path.join(Work, 'Success-B.wvb');
    const Failure = path.join(Work, 'Failure.wvb');
    Compile('success-a-compile', Admitter, Analyzer, Emitter,
        'Memory-Budget-Split-Executable.wv', Successˉa);
    Compile('success-b-compile', Admitter, Analyzer, Emitter,
        'Memory-Budget-Split-Executable.wv', Successˉb);
    Compile('failure-compile', Admitter, Analyzer, Emitter,
        'Memory-Budget-Split-Failure-Executable.wv', Failure);
    const Successˉbytes = readFileSync(Successˉa);
    const Successˉbˉbytes = readFileSync(Successˉb);
    if (!Successˉbytes.equals(Successˉbˉbytes)) {
        Reject('The executable Split fixture is not deterministic.');
    }
    const Layout = Inspectˉexactˉmodule(Successˉbytes);
    const Successˉsha256 = Digest(Successˉbytes);
    if (Successˉsha256 !== EXPECTED_SUCCESS_SHA256) {
        Reject(`The executable Split fixture digest differs: ${Successˉsha256}.`);
    }
    Inspectˉexactˉmodule(readFileSync(Failure), false);

    const Verifierˉwvb = path.join(Work, 'Verifier.wvb');
    const Verifier = path.join(Work, `Verifier${Executableˉsuffix}`);
    Runˉnative('verifier-build', 'Build-Wvb', [
        Project('Windvale-Compiler-Wvb-Verifier.wvproj'), Verifierˉwvb,
    ]);
    Runˉnative('verifier-package', 'Package-Hosted-Wvb', [
        '2', Verifierˉwvb, Verifier, Target,
    ]);
    Requireˉvalid(Verifier, Successˉa, 'successful Split module');
    Requireˉvalid(Verifier, Failure, 'refused Split module');

    const Malformedˉcases = [
        ['version-downgrade', Bytes => Bytes.writeUInt16LE(22, 6)],
        ['unknown-split-opcode', Bytes => { Bytes[Layout.opcode] = 207; }],
        ['entry-budget-parent', Bytes => Bytes.writeUInt32LE(0, Layout.opcode + 1)],
        ['non-budget-parent', Bytes => Bytes.writeUInt32LE(2, Layout.opcode + 1)],
        ['missing-result-type', Bytes => Bytes.writeUInt32LE(3, Layout.opcode + 5)],
        ['record-result-type', Bytes => Bytes.writeUInt32LE(0, Layout.opcode + 5)],
        ['primitive-valid-payload', Bytes => {
            Bytes[Layout.validPayloadShape] = 1;
        }],
        ['budget-in-failure-record', Bytes => {
            Bytes[Layout.requestedBytesShape] = 25;
        }],
        ['wrong-allocation-field', Bytes => {
            Bytes[Layout.availableBytesShape] = 5;
        }],
    ];
    for (const [Name, Mutate] of Malformedˉcases) {
        const Candidate = Buffer.from(Successˉbytes);
        Mutate(Candidate);
        const Candidateˉpath = path.join(Work, `${Name}.wvb`);
        writeFileSync(Candidateˉpath, Candidate, { flag: 'wx' });
        Requireˉinvalid(Verifier, Candidateˉpath, Name);
    }

    const Runnerˉwvb = path.join(Work, 'Runner.wvb');
    const Runner = path.join(Work, `Runner${Executableˉsuffix}`);
    Runˉnode('runner-build', 'Build-Cached-Split-Project-Wvb.mjs', [
        Project('Windvale-Wvb-Runner.wvproj'), Runnerˉwvb,
        Analyzer, Analyzerˉidentity, Emitter, Emitterˉidentity,
    ]);
    Runˉnative('runner-package', 'Package-Hosted-Wvb', [
        '5', Runnerˉwvb, Runner, Target,
    ]);
    Requireˉresultˉ42(Runner, Successˉa, 'successful Split execution');
    Requireˉresultˉ42(Runner, Failure, 'refused Split execution');

    process.stdout.write(
        'native language 1 memory budget split execution status=Passed ' +
        `cases=15 valid=2 malformed=${Malformedˉcases.length} ` +
        `result=42 wvb-bytes=${Successˉbytes.length} sha256=${Successˉsha256}\n`,
    );
} finally {
    const Resolved = path.resolve(Work);
    const Temporaryˉroot = path.resolve(os.tmpdir());
    if (path.dirname(Resolved) !== Temporaryˉroot ||
        !path.basename(Resolved).startsWith('windvale-memory-budget-split-execution-')) {
        Reject(`Refusing to remove unexpected test directory: ${Resolved}.`);
    }
    rmSync(Resolved, { recursive: true, force: true, maxRetries: 2 });
}

function Project(Name) {
    return path.join(Repositoryˉroot, 'Projects', 'Tools', Name);
}

function Compile(Label, Admitter, Analyzer, Emitter, Fixture, Output) {
    Runˉnode(Label, 'Run-Split-Compiler.mjs', [
        Admitter, Analyzer, Emitter,
        '--source-input-lock', Sourceˉlock, SOURCE_LOCK_SHA256,
        '--source-profile', Sourceˉprofile,
        path.join(Repositoryˉroot, 'Tests', 'Fixtures', 'Language-1.0', Fixture),
        path.join(Repositoryˉroot, 'Libraries', 'Foundation', 'Memory', 'Memory.wv'),
        path.join(Repositoryˉroot, 'Libraries', 'Foundation', 'Values', 'Result.wv'),
        Output,
    ]);
}

function Runˉnative(Label, Name, Arguments) {
    const Extension = process.platform === 'win32' ? '.cmd' : '.sh';
    const Script = path.join(Scriptˉdirectory, `${Name}${Extension}`);
    Requireˉordinaryˉfile(Script, 4_194_304, `${Name} script`);
    if (process.platform === 'win32') {
        Run(Label, process.env.ComSpec ?? 'cmd.exe', [
            '/d', '/c', 'call', Script, ...Arguments,
        ]);
    } else {
        Run(Label, 'bash', [Script, ...Arguments]);
    }
}

function Runˉnode(Label, Name, Arguments) {
    Run(Label, process.execPath, [path.join(Scriptˉdirectory, Name), ...Arguments]);
}

function Run(Label, Command, Arguments) {
    Step += 1;
    process.stdout.write(
        `START language 1 memory budget split execution step=${Step} phase=${Label}\n`,
    );
    const Result = spawnSync(Command, Arguments, {
        encoding: 'utf8',
        windowsHide: true,
        maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
        timeout: TOOL_TIMEOUT_MILLISECONDS,
    });
    if (Result.error !== undefined || Result.status !== 0 ||
        Result.stderr.length !== 0) {
        Reject(
            `${Label} failed: status=${Result.status} error=${Result.error?.message ?? ''}\n` +
            `stdout=${Result.stdout}\nstderr=${Result.stderr}`,
        );
    }
    process.stdout.write(
        `PASS  language 1 memory budget split execution step=${Step} phase=${Label}\n`,
    );
    return Result.stdout;
}

function Requireˉvalid(Verifier, Candidate, Label) {
    const Result = spawnSync(Verifier, [Candidate], {
        encoding: 'utf8', windowsHide: true,
        maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
    });
    if (Result.error !== undefined || Result.status !== 0 ||
        Normalize(Result.stdout) !== 'wvb status=Valid profile=compiler-aligned\n' ||
        Result.stderr.length !== 0) {
        Reject(`The verifier rejected the ${Label}.`);
    }
}

function Requireˉinvalid(Verifier, Candidate, Label) {
    const Result = spawnSync(Verifier, [Candidate], {
        encoding: 'utf8', windowsHide: true,
        maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
    });
    if (Result.error !== undefined || Result.status === 0 ||
        Result.stdout.includes('wvb status=Valid')) {
        Reject(`The verifier accepted malformed case ${Label}.`);
    }
}

function Requireˉresultˉ42(Runner, Candidate, Label) {
    const Result = spawnSync(Runner, [Candidate], {
        encoding: 'utf8', windowsHide: true,
        maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
    });
    if (Result.error !== undefined || Result.status !== 0 ||
        Normalize(Result.stdout) !== 'Result: 42\n' || Result.stderr.length !== 0) {
        Reject(`The ${Label} differed: status=${Result.status}.`);
    }
}

function Inspectˉexactˉmodule(Bytes, Requireˉsuccessˉsize = true) {
    if ((Requireˉsuccessˉsize && Bytes.length !== 752) ||
        Bytes.subarray(0, 4).toString('ascii') !== 'WVB1' ||
        Bytes.readUInt16LE(4) !== 1 || Bytes.readUInt16LE(6) !== 23 ||
        Bytes.readUInt32LE(8) !== 7) {
        Reject('The executable Split fixture is not the exact WVB 1.23 module.');
    }
    const Sections = Parseˉsections(Bytes);
    const Function = Parseˉmain(Bytes, Sections[4]);
    const Types = Parseˉtypes(Bytes, Sections[7]);
    if (Function.parameterCount !== 1 || Function.parameterShape !== 25 ||
        Function.returnShape !== 1 || Function.localShapes[0] !== 25 ||
        Types.length !== 3 || Types[0].kind !== 1 || Types[1].kind !== 7 ||
        Types[2].kind !== 3 || Types[2].cases.length !== 2 ||
        Types[2].cases[0].fields.length !== 1 ||
        Types[2].cases[0].fields[0].shape !== 25 ||
        Types[2].cases[1].fields.length !== 1 ||
        Types[2].cases[1].fields[0].shape !== 7 ||
        Types[2].cases[1].fields[0].typeIndex !== 0 ||
        Types[0].fields.length !== 3 || Types[0].fields[1].shape !== 10) {
        Reject('The executable Split fixture nominal layout differs.');
    }
    const Codeˉstart = Sections[5].payload + Function.codeOffset;
    const Codeˉend = Codeˉstart + Function.codeLength;
    const Matches = [];
    for (let Cursor = Codeˉstart; Cursor < Codeˉend; Cursor += 1) {
        if (Bytes[Cursor] === 206) Matches.push(Cursor);
    }
    if (Matches.length !== 1 || Bytes.readUInt32LE(Matches[0] + 1) !== 1 ||
        Bytes.readUInt32LE(Matches[0] + 5) !== 2) {
        Reject('The executable Split fixture opcode differs.');
    }
    return {
        opcode: Matches[0],
        validPayloadShape: Types[2].cases[0].fields[0].shapeOffset,
        requestedBytesShape: Types[0].fields[1].shapeOffset,
        availableBytesShape: Types[0].fields[2].shapeOffset,
    };
}

function Parseˉsections(Bytes) {
    const Result = [];
    let Cursor = 12;
    for (let Expected = 1; Expected <= 7; Expected += 1) {
        if (Cursor + 8 > Bytes.length || Bytes[Cursor] !== Expected ||
            Bytes[Cursor + 1] !== 0 || Bytes.readUInt16LE(Cursor + 2) !== 0) {
            Reject(`The module has no canonical section ${Expected}.`);
        }
        const Length = Bytes.readUInt32LE(Cursor + 4);
        const Payload = Cursor + 8;
        if (Payload + Length > Bytes.length) Reject('A WVB section is truncated.');
        Result[Expected] = { header: Cursor, payload: Payload, length: Length };
        Cursor = Payload + Length;
    }
    if (Cursor !== Bytes.length) Reject('The WVB module has trailing bytes.');
    return Result;
}

function Parseˉmain(Bytes, Section) {
    const Count = Bytes.readUInt32LE(Section.payload);
    let Cursor = Section.payload + 4;
    for (let Index = 0; Index < Count; Index += 1) {
        const Name = Readˉstring(Bytes, Cursor);
        Cursor = Name.end;
        const Parameterˉcount = Bytes.readUInt32LE(Cursor);
        Cursor += 4;
        const Parameters = [];
        for (let Parameter = 0; Parameter < Parameterˉcount; Parameter += 1) {
            const Parsed = Readˉshape(Bytes, Cursor);
            Parameters.push(Parsed.shape);
            Cursor = Parsed.end;
        }
        const Return = Readˉshape(Bytes, Cursor);
        Cursor = Return.end;
        const Localˉcount = Bytes.readUInt32LE(Cursor);
        Cursor += 4;
        const Locals = [];
        for (let Local = 0; Local < Localˉcount; Local += 1) {
            const Parsed = Readˉshape(Bytes, Cursor);
            Locals.push(Parsed.shape);
            Cursor = Parsed.end;
        }
        const Codeˉoffset = Bytes.readUInt32LE(Cursor);
        const Codeˉlength = Bytes.readUInt32LE(Cursor + 4);
        Cursor += 12;
        if (Name.value === 'Main') {
            return {
                parameterCount: Parameterˉcount,
                parameterShape: Parameters[0],
                returnShape: Return.shape,
                localShapes: Locals,
                codeOffset: Codeˉoffset,
                codeLength: Codeˉlength,
            };
        }
    }
    Reject('The module has no Main function.');
}

function Parseˉtypes(Bytes, Section) {
    const Count = Bytes.readUInt32LE(Section.payload);
    const Result = [];
    let Cursor = Section.payload + 4;
    for (let Index = 0; Index < Count; Index += 1) {
        const Kind = Bytes[Cursor++];
        const Name = Readˉstring(Bytes, Cursor);
        Cursor = Name.end;
        const Entry = { kind: Kind, fields: [], cases: [] };
        if (Kind === 1) {
            const Fieldˉcount = Bytes.readUInt32LE(Cursor);
            Cursor += 4;
            for (let Field = 0; Field < Fieldˉcount; Field += 1) {
                const Fieldˉname = Readˉstring(Bytes, Cursor);
                Cursor = Fieldˉname.end;
                const Shape = Readˉshape(Bytes, Cursor);
                Entry.fields.push(Shape);
                Cursor = Shape.end;
            }
        } else if (Kind === 7) {
            Cursor += 1;
            const Memberˉcount = Bytes.readUInt32LE(Cursor);
            Cursor += 4;
            for (let Member = 0; Member < Memberˉcount; Member += 1) {
                const Memberˉname = Readˉstring(Bytes, Cursor);
                Cursor = Memberˉname.end + 1;
            }
        } else if (Kind === 3) {
            const Caseˉcount = Bytes.readUInt32LE(Cursor);
            Cursor += 4;
            for (let Case = 0; Case < Caseˉcount; Case += 1) {
                const Caseˉname = Readˉstring(Bytes, Cursor);
                Cursor = Caseˉname.end;
                const Encoding = Bytes[Cursor++];
                const Fields = [];
                const Fieldˉcount = Encoding === 0 ? 0 :
                    Encoding === 1 ? 1 : Bytes.readUInt32LE(Cursor);
                if (Encoding === 2) Cursor += 4;
                for (let Field = 0; Field < Fieldˉcount; Field += 1) {
                    const Fieldˉname = Readˉstring(Bytes, Cursor);
                    Cursor = Fieldˉname.end;
                    const Shape = Readˉshape(Bytes, Cursor);
                    Fields.push(Shape);
                    Cursor = Shape.end;
                }
                Entry.cases.push({ fields: Fields });
            }
        } else {
            Reject(`Unexpected exact fixture type kind ${Kind}.`);
        }
        Result.push(Entry);
    }
    if (Cursor !== Section.payload + Section.length) {
        Reject('The exact Types directory length differs.');
    }
    return Result;
}

function Readˉstring(Bytes, Offset) {
    const Length = Bytes.readUInt32LE(Offset);
    const Start = Offset + 4;
    const End = Start + Length;
    if (End > Bytes.length) Reject('A WVB string is truncated.');
    return { value: Bytes.subarray(Start, End).toString('utf8'), end: End };
}

function Readˉshape(Bytes, Offset) {
    const Shape = Bytes[Offset];
    const Nominal = [7, 8, 11, 22, 23, 24].includes(Shape);
    return {
        shape: Shape,
        shapeOffset: Offset,
        typeIndex: Nominal ? Bytes.readUInt32LE(Offset + 1) : null,
        end: Offset + (Nominal ? 5 : 1),
    };
}

function Requireˉexactˉfile(Candidate, Size, Sha256, Label) {
    Requireˉordinaryˉfile(Candidate, MAXIMUM_WVB_BYTES, Label);
    const Bytes = readFileSync(Candidate);
    if (Bytes.length !== Size || Digest(Bytes) !== Sha256) {
        Reject(`The ${Label} identity differs.`);
    }
}

function Requireˉordinaryˉfile(Candidate, Maximum, Label) {
    const Information = lstatSync(Candidate);
    if (!Information.isFile() || Information.isSymbolicLink() ||
        Information.size < 1 || Information.size > Maximum ||
        !Sameˉpath(realpathSync(Candidate), Candidate)) {
        Reject(`The ${Label} is not a bounded ordinary file: ${Candidate}.`);
    }
}

function Digest(Bytes) {
    return createHash('sha256').update(Bytes).digest('hex');
}

function Sameˉpath(Left, Right) {
    return process.platform === 'win32'
        ? Left.toLowerCase() === path.resolve(Right).toLowerCase()
        : Left === path.resolve(Right);
}

function Normalize(Value) {
    return Value.replaceAll('\r\n', '\n');
}

function Reject(Message) {
    throw new Error(Message);
}
