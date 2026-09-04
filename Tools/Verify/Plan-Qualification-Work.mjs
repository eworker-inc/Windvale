import { existsSync, readFileSync, statSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const SCRIPT_DIRECTORY = dirname(fileURLToPath(import.meta.url));
const REPOSITORY_ROOT = dirname(dirname(SCRIPT_DIRECTORY));
const OWNER_INVENTORY = join(
    REPOSITORY_ROOT,
    'Tests',
    'Native',
    'Verification-Owners.txt',
);
const PROFILE_INVENTORY = join(
    REPOSITORY_ROOT,
    'Tests',
    'Native',
    'Verification-Duration-Profiles.txt',
);
const PIPELINE_MARKERS = [
    'Build-Current-Wvb',
    'Build-Wvb',
    'Build-Cached-Project-Object',
    'Build-Cached-Hosted-Application',
    'Build-Cached-Split-Project-Wvb',
    'Build-Cached-Segmented-Hosted-Wvb',
    'Stage-Compiler-Wvb',
    'Lower-Wvb-To-Wvo',
    'Check-Wvo',
    'Link-Wvo',
    'Package-Hosted-Wvb',
    'Package-Console',
    'Package-Segmented-Compiler-Wvb',
    'Verify-Wvb',
    'Verify-Wvo',
    'Verify-Source-Analysis-Diagnostic',
    'Run-Wvb',
    'Run-Split-Compiler',
    'Run-Authenticated-Source-Admission',
];

function Fail(Message) {
    process.stderr.write(`Qualification work plan rejected: ${Message}.\n`);
    process.exit(1);
}

function Readˉtext(Path, Description) {
    let Bytes;
    try {
        Bytes = readFileSync(Path);
    } catch (Error) {
        Fail(`${Description} could not be read (${Error.message})`);
    }
    if (Bytes.includes(0) || Bytes.toString('utf8').includes('\uFFFD')) {
        Fail(`${Description} is not strict UTF-8 text`);
    }
    return Bytes.toString('utf8');
}

function Readˉlines(Path, Header, Description) {
    const Lines = Readˉtext(Path, Description)
        .split(/\r?\n/u)
        .filter(Line => Line.length !== 0);
    if (Lines.shift() !== Header) Fail(`${Description} header is invalid`);
    return Lines;
}

function Positiveˉinteger(Value, Description) {
    if (!/^[1-9][0-9]*$/u.test(Value)) Fail(`${Description} is invalid`);
    const Result = Number(Value);
    if (!Number.isSafeInteger(Result)) Fail(`${Description} exceeds the safe range`);
    return Result;
}

function Countˉuses(Text, Marker) {
    let Count = 0;
    let Offset = 0;
    while ((Offset = Text.indexOf(Marker, Offset)) !== -1) {
        Count += 1;
        Offset += Marker.length;
    }
    return Count;
}

function Countˉlines(Text) {
    if (Text.length === 0) return 0;
    return Text.split(/\r?\n/u).length - Number(Text.endsWith('\n'));
}

function Findˉprojects(Text) {
    const Projects = new Set();
    const ProjectPathPattern =
        /Projects[\\/](?:[A-Za-z0-9._-]+[\\/])*[A-Za-z0-9._-]+\.wvproj/gu;
    for (const Match of Text.matchAll(ProjectPathPattern)) {
        Projects.add(Match[0].replaceAll('\\', '/'));
    }
    const RepositoryJoinPattern =
        /join\(\s*REPOSITORY_ROOT\s*,((?:\s*['"][^'"\r\n]+['"]\s*,?)+)\s*\)/gu;
    for (const Match of Text.matchAll(RepositoryJoinPattern)) {
        const Segments = [...Match[1].matchAll(/['"]([^'"\r\n]+)['"]/gu)]
            .map(Segment => Segment[1]);
        if (Segments.length !== 0 &&
            Segments.at(-1).endsWith('.wvproj')) {
            Projects.add(Segments.join('/'));
        }
    }
    return Projects;
}

const Profiles = new Map();
for (const [Index, Line] of Readˉlines(
    PROFILE_INVENTORY,
    'windvale-native-verification-duration-profiles 1',
    'duration-profile inventory',
).entries()) {
    const Fields = Line.split('|');
    if (Fields.length !== 4 || !/^[a-z]+(?:-[a-z]+)*$/u.test(Fields[0]) ||
        Profiles.has(Fields[0])) {
        Fail(`duration-profile row ${Index + 2} is malformed`);
    }
    const ExpectedSeconds = Positiveˉinteger(
        Fields[1],
        `duration-profile row ${Index + 2} expected seconds`,
    );
    const MaximumSeconds = Positiveˉinteger(
        Fields[2],
        `duration-profile row ${Index + 2} maximum seconds`,
    );
    if (MaximumSeconds < ExpectedSeconds || !/^[01]$/u.test(Fields[3])) {
        Fail(`duration-profile row ${Index + 2} has invalid bounds`);
    }
    Profiles.set(Fields[0], {
        Name: Fields[0],
        ExpectedSeconds,
        MaximumSeconds,
        InfrastructureRetries: Number(Fields[3]),
    });
}

const Owners = [];
const OwnerNames = new Set();
const Commands = new Set();
for (const [Index, Line] of Readˉlines(
    OWNER_INVENTORY,
    'windvale-native-verification-owners 2',
    'verification-owner inventory',
).entries()) {
    const RawFields = Line.split('|');
    const Fields = RawFields.length < 6
        ? RawFields
        : [...RawFields.slice(0, 5), RawFields.slice(5).join('|')];
    if (Fields.length !== 6 ||
        !/^[a-z0-9]+(?:[.-][a-z0-9]+)*$/u.test(Fields[0]) ||
        OwnerNames.has(Fields[0]) ||
        !/^[A-Za-z0-9]+(?:[.-][A-Za-z0-9]+)*$/u.test(Fields[1]) ||
        Commands.has(Fields[1]) || !/^[1-4]$/u.test(Fields[3]) ||
        !Profiles.has(Fields[4]) || Fields[5].trim().length === 0) {
        Fail(`verification-owner row ${Index + 2} is malformed`);
    }
    const Cases = Positiveˉinteger(
        Fields[2],
        `verification-owner row ${Index + 2} cases`,
    );
    const Profile = Profiles.get(Fields[4]);
    const HostSources = [];
    for (const Extension of ['cmd', 'sh']) {
        const RelativePath = `Tools/Native/${Fields[1]}.${Extension}`;
        const AbsolutePath = join(REPOSITORY_ROOT, ...RelativePath.split('/'));
        try {
            if (!statSync(AbsolutePath).isFile()) Fail(`${RelativePath} is not a file`);
        } catch (Error) {
            Fail(`${RelativePath} is unavailable (${Error.message})`);
        }
        HostSources.push({ RelativePath, Text: Readˉtext(AbsolutePath, RelativePath) });
    }
    const AnalysisSources = [...HostSources];
    const ModuleRelativePath = `Tools/Native/${Fields[1]}.mjs`;
    const ModuleAbsolutePath = join(
        REPOSITORY_ROOT,
        ...ModuleRelativePath.split('/'),
    );
    if (existsSync(ModuleAbsolutePath)) {
        if (!statSync(ModuleAbsolutePath).isFile()) {
            Fail(`${ModuleRelativePath} is not a file`);
        }
        AnalysisSources.push({
            RelativePath: ModuleRelativePath,
            Text: Readˉtext(ModuleAbsolutePath, ModuleRelativePath),
        });
    }
    OwnerNames.add(Fields[0]);
    Commands.add(Fields[1]);
    Owners.push({
        Name: Fields[0],
        Command: Fields[1],
        Cases,
        Shard: Number(Fields[3]),
        Profile: Profile.Name,
        ExpectedSeconds: Profile.ExpectedSeconds,
        MaximumSeconds: Profile.MaximumSeconds,
        HostSources,
        AnalysisSources,
    });
}

const Shards = [1, 2, 3, 4].map(Shard => {
    const Selected = Owners.filter(Owner => Owner.Shard === Shard);
    return {
        Shard,
        Owners: Selected.length,
        Cases: Selected.reduce((Total, Owner) => Total + Owner.Cases, 0),
        ExpectedSeconds: Selected.reduce(
            (Total, Owner) => Total + Owner.ExpectedSeconds,
            0,
        ),
        MaximumSeconds: Selected.reduce(
            (Total, Owner) => Total + Owner.MaximumSeconds,
            0,
        ),
    };
});

const ProfileTotals = [...Profiles.values()].map(Profile => {
    const Selected = Owners.filter(Owner => Owner.Profile === Profile.Name);
    return {
        Profile: Profile.Name,
        Owners: Selected.length,
        Cases: Selected.reduce((Total, Owner) => Total + Owner.Cases, 0),
        ExpectedSeconds: Selected.length * Profile.ExpectedSeconds,
        MaximumSeconds: Selected.length * Profile.MaximumSeconds,
    };
});

const ProjectOwners = new Map();
const PipelineUses = new Map(PIPELINE_MARKERS.map(Marker => [Marker, {
    Owners: new Set(),
    ScriptCallSites: 0,
}]));
for (const Owner of Owners) {
    const OwnerProjects = new Set();
    const OwnerPipelineUses = new Map();
    let OwnerPipelineCallSites = 0;
    for (const AnalysisSource of Owner.AnalysisSources) {
        for (const Project of Findˉprojects(AnalysisSource.Text)) {
            OwnerProjects.add(Project);
        }
        for (const Marker of PIPELINE_MARKERS) {
            const Uses = Countˉuses(AnalysisSource.Text, Marker);
            if (Uses === 0) continue;
            const Entry = PipelineUses.get(Marker);
            Entry.Owners.add(Owner.Name);
            Entry.ScriptCallSites += Uses;
            OwnerPipelineUses.set(
                Marker,
                (OwnerPipelineUses.get(Marker) ?? 0) + Uses,
            );
            OwnerPipelineCallSites += Uses;
        }
    }
    Owner.AnalysisFiles = Owner.AnalysisSources.length;
    Owner.SourceLines = Owner.AnalysisSources.reduce(
        (Total, Source) => Total + Countˉlines(Source.Text),
        0,
    );
    Owner.UniqueProjects = OwnerProjects.size;
    Owner.Projects = [...OwnerProjects].sort();
    Owner.PipelineCallSites = OwnerPipelineCallSites;
    Owner.PipelineUses = [...OwnerPipelineUses.entries()].map(
        ([Marker, ScriptCallSites]) => ({ Marker, ScriptCallSites }),
    );
    for (const Project of OwnerProjects) {
        const Uses = ProjectOwners.get(Project) ?? new Set();
        Uses.add(Owner.Name);
        ProjectOwners.set(Project, Uses);
    }
}

const RepeatedProjects = [...ProjectOwners.entries()]
    .filter(([, Uses]) => Uses.size > 1)
    .map(([Path, Uses]) => ({ Path, OwnerCount: Uses.size, Owners: [...Uses].sort() }))
    .sort((Left, Right) =>
        Right.OwnerCount - Left.OwnerCount || Left.Path.localeCompare(Right.Path))
    .slice(0, 30);

const NestedOwnerEdges = [];
for (const Owner of Owners) {
    const Combined = Owner.AnalysisSources.map(Source => Source.Text).join('\n');
    for (const Dependency of Owners) {
        if (Dependency === Owner) continue;
        if (Combined.includes(`${Dependency.Command}.cmd`) ||
            Combined.includes(`${Dependency.Command}.sh`) ||
            Combined.includes(`${Dependency.Command}.mjs`)) {
            NestedOwnerEdges.push({ Owner: Owner.Name, Invokes: Dependency.Name });
        }
    }
}
NestedOwnerEdges.sort((Left, Right) =>
    Left.Owner.localeCompare(Right.Owner) || Left.Invokes.localeCompare(Right.Invokes));

const TotalExpectedSeconds = Owners.reduce(
    (Total, Owner) => Total + Owner.ExpectedSeconds,
    0,
);
const TotalMaximumSeconds = Owners.reduce(
    (Total, Owner) => Total + Owner.MaximumSeconds,
    0,
);
const DeclaredCriticalPathExpectedSeconds = Math.max(
    ...Shards.map(Shard => Shard.ExpectedSeconds),
);
const DeclaredCriticalPathMaximumSeconds = Math.max(
    ...Shards.map(Shard => Shard.MaximumSeconds),
);
const MinimumShardExpectedSeconds = Math.min(
    ...Shards.map(Shard => Shard.ExpectedSeconds),
);
const IdealShardExpectedSeconds = Math.ceil(
    TotalExpectedSeconds / Shards.length,
);
const ShardExpectedSpreadSeconds =
    DeclaredCriticalPathExpectedSeconds - MinimumShardExpectedSeconds;
const DeclaredParallelEfficiencyBasisPoints = Math.floor(
    (TotalExpectedSeconds * 10000) /
        (Shards.length * DeclaredCriticalPathExpectedSeconds),
);
const LongOwners = Owners.filter(Owner => Owner.ExpectedSeconds >= 900);
const OwnerAnalysis = Owners.map(Owner => ({
    Name: Owner.Name,
    Command: Owner.Command,
    Shard: Owner.Shard,
    Cases: Owner.Cases,
    Profile: Owner.Profile,
    ExpectedSeconds: Owner.ExpectedSeconds,
    MaximumSeconds: Owner.MaximumSeconds,
    AnalysisFiles: Owner.AnalysisFiles,
    SourceLines: Owner.SourceLines,
    UniqueProjects: Owner.UniqueProjects,
    Projects: Owner.Projects,
    PipelineCallSites: Owner.PipelineCallSites,
    PipelineUses: Owner.PipelineUses,
}));
const Summary = {
    Format: 'windvale-qualification-work-plan-2',
    Owners: Owners.length,
    Cases: Owners.reduce((Total, Owner) => Total + Owner.Cases, 0),
    TotalExpectedSeconds,
    TotalMaximumSeconds,
    DualHostExpectedWorkSeconds: TotalExpectedSeconds * 2,
    DeclaredCriticalPathExpectedSeconds,
    DeclaredCriticalPathMaximumSeconds,
    MinimumShardExpectedSeconds,
    IdealShardExpectedSeconds,
    ShardExpectedSpreadSeconds,
    DeclaredParallelEfficiencyBasisPoints,
    LongOwners: LongOwners.length,
    LongOwnerExpectedSeconds: LongOwners.reduce(
        (Total, Owner) => Total + Owner.ExpectedSeconds,
        0,
    ),
    AnalysisFiles: Owners.reduce(
        (Total, Owner) => Total + Owner.AnalysisFiles,
        0,
    ),
    SourceLines: Owners.reduce(
        (Total, Owner) => Total + Owner.SourceLines,
        0,
    ),
    OwnerProjectReferences: Owners.reduce(
        (Total, Owner) => Total + Owner.UniqueProjects,
        0,
    ),
    Shards,
    Profiles: ProfileTotals,
    OwnerAnalysis,
    TopExpectedOwners: [...OwnerAnalysis]
        .sort((Left, Right) =>
            Right.ExpectedSeconds - Left.ExpectedSeconds ||
            Left.Name.localeCompare(Right.Name))
        .slice(0, 20),
    RepeatedProjects,
    NestedOwnerEdges,
    PipelineUses: [...PipelineUses.entries()].map(([Marker, Entry]) => ({
        Marker,
        Owners: Entry.Owners.size,
        ScriptCallSites: Entry.ScriptCallSites,
    })),
};

if (process.argv.length === 3 && process.argv[2] === '--json') {
    process.stdout.write(`${JSON.stringify(Summary, null, 2)}\n`);
} else if (process.argv.length === 3 && process.argv[2] === '--owners') {
    for (const Owner of Owners) {
        process.stdout.write(
            `${Owner.Name}|${Owner.Shard}|${Owner.Cases}|${Owner.Profile}|` +
            `${Owner.ExpectedSeconds}|${Owner.MaximumSeconds}|${Owner.Command}\n`,
        );
    }
} else if (process.argv.length === 2) {
    process.stdout.write(
        `${Summary.Format} owners=${Summary.Owners} cases=${Summary.Cases} ` +
        `expected-seconds=${Summary.TotalExpectedSeconds} ` +
        `maximum-seconds=${Summary.TotalMaximumSeconds} ` +
        `critical-path-expected-seconds=${Summary.DeclaredCriticalPathExpectedSeconds} ` +
        `critical-path-maximum-seconds=${Summary.DeclaredCriticalPathMaximumSeconds} ` +
        `ideal-shard-expected-seconds=${Summary.IdealShardExpectedSeconds} ` +
        `shard-expected-spread-seconds=${Summary.ShardExpectedSpreadSeconds} ` +
        `parallel-efficiency-basis-points=${Summary.DeclaredParallelEfficiencyBasisPoints} ` +
        `dual-host-expected-work-seconds=${Summary.DualHostExpectedWorkSeconds} ` +
        `long-owners=${Summary.LongOwners} ` +
        `long-owner-expected-seconds=${Summary.LongOwnerExpectedSeconds} ` +
        `analysis-files=${Summary.AnalysisFiles} ` +
        `source-lines=${Summary.SourceLines} ` +
        `owner-project-references=${Summary.OwnerProjectReferences}\n`,
    );
    for (const Shard of Shards) {
        process.stdout.write(
            `qualification shard=${Shard.Shard} owners=${Shard.Owners} ` +
            `cases=${Shard.Cases} expected-seconds=${Shard.ExpectedSeconds} ` +
            `maximum-seconds=${Shard.MaximumSeconds}\n`,
        );
    }
    for (const Owner of Summary.TopExpectedOwners.slice(0, 10)) {
        process.stdout.write(
            `qualification long-owner name=${Owner.Name} shard=${Owner.Shard} ` +
            `cases=${Owner.Cases} expected-seconds=${Owner.ExpectedSeconds} ` +
            `maximum-seconds=${Owner.MaximumSeconds} ` +
            `analysis-files=${Owner.AnalysisFiles} source-lines=${Owner.SourceLines} ` +
            `unique-projects=${Owner.UniqueProjects} ` +
            `pipeline-call-sites=${Owner.PipelineCallSites}\n`,
        );
    }
    process.stdout.write(
        `qualification overlap repeated-projects=${Summary.RepeatedProjects.length} ` +
        `nested-owner-edges=${Summary.NestedOwnerEdges.length}\n`,
    );
    for (const Pipeline of Summary.PipelineUses) {
        process.stdout.write(
            `qualification pipeline marker=${Pipeline.Marker} owners=${Pipeline.Owners} ` +
            `script-call-sites=${Pipeline.ScriptCallSites}\n`,
        );
    }
} else {
    process.stderr.write(
        'Usage: node Tools/Verify/Plan-Qualification-Work.mjs [--json|--owners]\n',
    );
    process.exit(64);
}
