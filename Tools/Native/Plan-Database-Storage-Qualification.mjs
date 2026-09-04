import { readFileSync, statSync } from 'node:fs';
import { dirname, join, normalize, relative, resolve, sep } from 'node:path';
import { fileURLToPath } from 'node:url';

const SCRIPT_DIRECTORY = dirname(fileURLToPath(import.meta.url));
const REPOSITORY_ROOT = dirname(dirname(SCRIPT_DIRECTORY));
const INVENTORY = join(
    REPOSITORY_ROOT,
    'Tests',
    'Native',
    'Database-Storage-Qualification-Steps.txt',
);
const EXPECTED_HEADER = 'windvale-database-storage-qualification-steps 3';

function Fail(Message) {
    process.stderr.write(`Database qualification plan rejected: ${Message}.\n`);
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

function Repositoryˉpath(Value, Description) {
    if (!/^[A-Za-z0-9][A-Za-z0-9./-]*$/u.test(Value) ||
        Value.includes('..') || Value.includes('\\') ||
        !Value.endsWith('.wvproj')) {
        Fail(`${Description} has an invalid repository path`);
    }
    const Absolute = resolve(REPOSITORY_ROOT, ...Value.split('/'));
    const Relative = relative(REPOSITORY_ROOT, Absolute);
    if (Relative === '' || Relative === '..' ||
        Relative.startsWith(`..${sep}`) || normalize(Relative) !== Relative) {
        Fail(`${Description} escapes the repository`);
    }
    try {
        if (!statSync(Absolute).isFile()) Fail(`${Description} is not a file`);
    } catch (Error) {
        Fail(`${Description} does not exist (${Error.message})`);
    }
    return Absolute;
}

const Lines = Readˉtext(INVENTORY, 'qualification inventory')
    .split(/\r?\n/u)
    .filter(Line => Line.length !== 0);
if (Lines.shift() !== EXPECTED_HEADER) Fail('qualification inventory header is invalid');

const Labels = new Set();
const CaseNames = new Set();
const Steps = [];
for (const [Index, Line] of Lines.entries()) {
    const Fields = Line.split('|');
    if (Fields.length < 5 || Fields.length > 7) {
        Fail(`inventory row ${Index + 2} has ${Fields.length} fields`);
    }
    const [Label, Lane, Role, Handler, ProjectList, DeclaredCases, DeclaredRequires] = Fields;
    if (!/^[A-Z][A-Za-z0-9]*$/u.test(Label) || Labels.has(Label)) {
        Fail(`inventory row ${Index + 2} has an invalid or duplicate label`);
    }
    if (Lane !== 'portable' && Lane !== 'hosted') {
        Fail(`inventory row ${Index + 2} has an invalid lane`);
    }
    if (Role !== 'case' && Role !== 'prerequisite') {
        Fail(`inventory row ${Index + 2} has an invalid role`);
    }
    if (Role === 'prerequisite' && Lane !== 'portable') {
        Fail(`inventory row ${Index + 2} has a hosted prerequisite`);
    }
    const Cases = Role === 'case'
        ? (DeclaredCases === undefined || DeclaredCases === '-'
            ? [Label]
            : DeclaredCases.split(','))
        : [];
    if (Role === 'prerequisite' &&
        DeclaredCases !== undefined && DeclaredCases !== '-') {
        Fail(`inventory row ${Index + 2} gives cases to a prerequisite`);
    }
    if (Role === 'case' && (Cases.length === 0 || Cases.some(Case =>
        !/^[A-Z][A-Za-z0-9]*$/u.test(Case) || !CaseNames.add(Case)))) {
        Fail(`inventory row ${Index + 2} has an invalid or duplicate case`);
    }
    if (!/^[a-z][a-z0-9-]*$/u.test(Handler)) {
        Fail(`inventory row ${Index + 2} has an invalid handler`);
    }
    const ProjectPaths = ProjectList.split(',');
    if (ProjectPaths.length === 0 || ProjectPaths.length > 3 ||
        new Set(ProjectPaths).size !== ProjectPaths.length) {
        Fail(`inventory row ${Index + 2} has an empty, duplicate, or oversized project list`);
    }
    const Projects = ProjectPaths.map((Path, ProjectIndex) => ({
        Path,
        Absolute: Repositoryˉpath(
            Path,
            `inventory row ${Index + 2} project ${ProjectIndex + 1}`,
        ),
    }));
    const Requires = DeclaredRequires === undefined || DeclaredRequires === '-'
        ? []
        : DeclaredRequires.split(',');
    if (Requires.some(Required => !/^[A-Z][A-Za-z0-9]*$/u.test(Required)) ||
        new Set(Requires).size !== Requires.length || Requires.includes(Label)) {
        Fail(`inventory row ${Index + 2} has an invalid dependency list`);
    }
    Labels.add(Label);
    Steps.push({ Label, Lane, Role, Handler, Projects, Cases, Requires });
}

const StepByLabel = new Map(Steps.map(Step => [Step.Label, Step]));
for (const [Index, Step] of Steps.entries()) {
    for (const Required of Step.Requires) {
        const Dependency = StepByLabel.get(Required);
        const DependencyIndex = Steps.indexOf(Dependency);
        if (Dependency === undefined || Dependency.Lane !== Step.Lane ||
            DependencyIndex >= Index) {
            Fail(`step ${Step.Label} has a missing, cross-lane, or forward dependency`);
        }
    }
}

const Cases = Steps.flatMap(Step => Step.Cases);
const Prerequisites = Steps.filter(Step => Step.Role === 'prerequisite');
if (Cases.length === 0 ||
    !Steps.some(Step => Step.Lane === 'portable') ||
    !Steps.some(Step => Step.Lane === 'hosted')) {
    Fail('inventory must contain cases and both qualification lanes');
}

const ProjectUses = new Map();
const SourceUses = new Map();
const SourceBytes = new Map();
const ProjectProfiles = new Map();
for (const Step of Steps) {
    for (const Project of Step.Projects) {
        ProjectUses.set(Project.Path, (ProjectUses.get(Project.Path) ?? 0) + 1);
        const ProjectLines = Readˉtext(Project.Absolute, `project ${Project.Path}`)
            .split(/\r?\n/u)
            .filter(Line => Line.length !== 0);
        if (ProjectLines[0] !== 'windvale-project 2') {
            Fail(`project ${Project.Path} is not Project 2`);
        }
        const Roots = [];
        const Sources = [];
        for (const Line of ProjectLines.slice(1)) {
            const Match = /^(root|source) "([^"]+)"$/u.exec(Line);
            if (Match === null) continue;
            const SourcePath = Match[2];
            const SourceAbsolute = resolve(REPOSITORY_ROOT, ...SourcePath.split('/'));
            const SourceRelative = relative(REPOSITORY_ROOT, SourceAbsolute);
            if (SourceRelative === '' || SourceRelative === '..' ||
                SourceRelative.startsWith(`..${sep}`)) {
                Fail(`project ${Project.Path} source escapes the repository`);
            }
            let SourceInformation;
            try {
                SourceInformation = statSync(SourceAbsolute);
                if (!SourceInformation.isFile()) {
                    Fail(`project ${Project.Path} source ${SourcePath} is not a file`);
                }
            } catch (Error) {
                Fail(`project ${Project.Path} source ${SourcePath} does not exist (${Error.message})`);
            }
            SourceUses.set(SourcePath, (SourceUses.get(SourcePath) ?? 0) + 1);
            SourceBytes.set(SourcePath, SourceInformation.size);
            if (Match[1] === 'root') Roots.push(SourcePath);
            else Sources.push(SourcePath);
        }
        if (Roots.length + Sources.length === 0) {
            Fail(`project ${Project.Path} has no root or source entries`);
        }
        ProjectProfiles.set(Project.Path, { Roots, Sources });
    }
}

const ProjectReferences = [...ProjectUses.values()].reduce((Left, Right) => Left + Right, 0);
const SourceReferences = [...SourceUses.values()].reduce((Left, Right) => Left + Right, 0);
const Duplication = SourceReferences / SourceUses.size;
const RepeatedSources = [...SourceUses.entries()]
    .sort((Left, Right) => Right[1] - Left[1] || Left[0].localeCompare(Right[0]))
    .slice(0, 10)
    .map(([Path, Uses]) => ({ Path, Uses }));
const PortableSingleConstructionSteps = Steps.filter(Step =>
    Step.Lane === 'portable' &&
    (Step.Handler === 'project' || Step.Handler === 'segmented-project'));
const PortableDuplicateSourceVisitsDelegated = PortableSingleConstructionSteps
    .flatMap(Step => Step.Projects)
    .reduce((Total, Project) => {
        const Profile = ProjectProfiles.get(Project.Path);
        return Total + Profile.Roots.length + Profile.Sources.length;
    }, 0);
const SharedClosureGroups = new Map();
for (const Step of Steps.filter(Step =>
    Step.Lane === 'portable' && Step.Role === 'case' &&
    Step.Handler === 'project' && Step.Projects.length === 1 &&
    Step.Cases.length === 1)) {
    const Project = Step.Projects[0];
    const Profile = ProjectProfiles.get(Project.Path);
    const Key = JSON.stringify(Profile.Sources);
    const Group = SharedClosureGroups.get(Key) ?? {
        SourceCount: Profile.Sources.length,
        Steps: [],
        Cases: [],
        Projects: [],
    };
    Group.Steps.push(Step.Label);
    Group.Cases.push(...Step.Cases);
    Group.Projects.push(Project.Path);
    SharedClosureGroups.set(Key, Group);
}
const SharedClosureCandidates = [...SharedClosureGroups.values()]
    .filter(Group => Group.Steps.length > 1)
    .sort((Left, Right) =>
        Right.Steps.length - Left.Steps.length ||
        Right.SourceCount - Left.SourceCount ||
        Left.Steps[0].localeCompare(Right.Steps[0]));
const OverlapCandidateSteps = Steps.filter(Step =>
    Step.Lane === 'portable' && Step.Role === 'case' &&
    Step.Handler === 'project' && Step.Projects.length === 1 &&
    Step.Cases.length === 1);
const OverlapMergeCandidates = [];
for (let LeftIndex = 0;
    LeftIndex < OverlapCandidateSteps.length;
    LeftIndex += 1) {
    const Left = OverlapCandidateSteps[LeftIndex];
    const LeftProfile = ProjectProfiles.get(Left.Projects[0].Path);
    const LeftPaths = [...LeftProfile.Roots, ...LeftProfile.Sources];
    for (let RightIndex = LeftIndex + 1;
        RightIndex < OverlapCandidateSteps.length;
        RightIndex += 1) {
        const Right = OverlapCandidateSteps[RightIndex];
        const RightProfile = ProjectProfiles.get(Right.Projects[0].Path);
        if (LeftProfile.Sources.join('\n') === RightProfile.Sources.join('\n')) {
            continue;
        }
        const RightPaths = [...RightProfile.Roots, ...RightProfile.Sources];
        const RightPathSet = new Set(RightPaths);
        const SharedPaths = LeftPaths.filter(Path => RightPathSet.has(Path));
        if (SharedPaths.length === 0) continue;
        const UnionPaths = [...new Set([...LeftPaths, ...RightPaths])];
        const SeparateReferences = LeftPaths.length + RightPaths.length;
        OverlapMergeCandidates.push({
            Steps: [Left.Label, Right.Label],
            Cases: [...Left.Cases, ...Right.Cases],
            Projects: [Left.Projects[0].Path, Right.Projects[0].Path],
            SharedSources: SharedPaths.length,
            SharedSourceBytes: SharedPaths.reduce(
                (Total, Path) => Total + SourceBytes.get(Path), 0),
            UnionSources: UnionPaths.length,
            UnionSourceBytes: UnionPaths.reduce(
                (Total, Path) => Total + SourceBytes.get(Path), 0),
            DeclarationVisitReductionBasisPoints: Math.floor(
                (SharedPaths.length * 10_000) / SeparateReferences),
        });
    }
}
OverlapMergeCandidates.sort((Left, Right) =>
    Right.DeclarationVisitReductionBasisPoints -
        Left.DeclarationVisitReductionBasisPoints ||
    Right.SharedSourceBytes - Left.SharedSourceBytes ||
    Left.UnionSourceBytes - Right.UnionSourceBytes ||
    Left.Steps[0].localeCompare(Right.Steps[0]) ||
    Left.Steps[1].localeCompare(Right.Steps[1]));
const Summary = {
    Format: 'windvale-database-storage-qualification-plan-3',
    Steps: Steps.length,
    Cases: Cases.length,
    Prerequisites: Prerequisites.length,
    PortableSteps: Steps.filter(Step => Step.Lane === 'portable').length,
    HostedSteps: Steps.filter(Step => Step.Lane === 'hosted').length,
    PortableCases: Steps
        .filter(Step => Step.Lane === 'portable')
        .reduce((Total, Step) => Total + Step.Cases.length, 0),
    HostedCases: Steps
        .filter(Step => Step.Lane === 'hosted')
        .reduce((Total, Step) => Total + Step.Cases.length, 0),
    ProjectReferences,
    UniqueProjects: ProjectUses.size,
    SourceReferences,
    UniqueSources: SourceUses.size,
    ManifestDuplication: Number(Duplication.toFixed(2)),
    AllPairedSourceVisits: SourceReferences * 2,
    PortableSingleConstructionSteps: PortableSingleConstructionSteps.length,
    PortableDuplicateSourceVisitsDelegated,
    DependencyEdges: Steps.reduce(
        (Total, Step) => Total + Step.Requires.length,
        0,
    ),
    StepsWithDependencies: Steps.filter(Step => Step.Requires.length !== 0).length,
    RepeatedSources,
    SharedClosureCandidates,
    OverlapMergeCandidates: OverlapMergeCandidates.slice(0, 12),
};

function Writeˉrow(Step) {
    const Paths = Step.Projects.map(Project => Project.Path);
    while (Paths.length < 3) Paths.push('-');
    process.stdout.write(
        `${Step.Label}|${Step.Handler}|${Paths.slice(0, 3).join('|')}|` +
        `${Step.Cases.length === 0 ? '-' : Step.Cases.join(',')}\n`,
    );
}

function Closureˉfor(Step) {
    const Selected = new Set();
    function Addˉstep(Current) {
        for (const Required of Current.Requires) {
            Addˉstep(StepByLabel.get(Required));
        }
        Selected.add(Current.Label);
    }
    Addˉstep(Step);
    return Steps.filter(Candidate => Selected.has(Candidate.Label));
}

if (process.argv.length === 3 && process.argv[2] === '--json') {
    process.stdout.write(`${JSON.stringify(Summary, null, 2)}\n`);
} else if (process.argv.length === 3 && process.argv[2] === '--counts') {
    process.stdout.write(
        `windvale-database-storage-qualification-counts-1|${Summary.Steps}|` +
        `${Summary.Cases}|${Summary.Prerequisites}|${Summary.PortableSteps}|` +
        `${Summary.HostedSteps}|${Summary.PortableCases}|${Summary.HostedCases}\n`,
    );
} else if (process.argv.length === 4 && process.argv[2] === '--rows' &&
    (process.argv[3] === 'portable' || process.argv[3] === 'hosted')) {
    for (const Step of Steps.filter(Step => Step.Lane === process.argv[3])) {
        Writeˉrow(Step);
    }
} else if (process.argv.length === 3 && process.argv[2] === '--row-env') {
    const Label = process.env.WINDVALE_DATABASE_QUALIFICATION_STEP ?? '';
    const Step = Steps.find(Candidate => Candidate.Label === Label);
    if (!/^[A-Z][A-Za-z0-9]*$/u.test(Label) || Step === undefined) {
        process.stderr.write(`Unknown database qualification step: ${Label}.\n`);
        process.exit(64);
    }
    Writeˉrow(Step);
} else if (process.argv.length === 3 && process.argv[2] === '--closure-env') {
    const Label = process.env.WINDVALE_DATABASE_QUALIFICATION_STEP ?? '';
    const Step = Steps.find(Candidate => Candidate.Label === Label);
    if (!/^[A-Z][A-Za-z0-9]*$/u.test(Label) || Step === undefined) {
        process.stderr.write(`Unknown database qualification step: ${Label}.\n`);
        process.exit(64);
    }
    for (const Dependency of Closureˉfor(Step)) Writeˉrow(Dependency);
} else if (process.argv.length === 2) {
    process.stdout.write(
        `${Summary.Format} steps=${Summary.Steps} cases=${Summary.Cases} ` +
        `prerequisites=${Summary.Prerequisites} portable-steps=${Summary.PortableSteps} ` +
        `hosted-steps=${Summary.HostedSteps} portable-cases=${Summary.PortableCases} ` +
        `hosted-cases=${Summary.HostedCases} project-references=${Summary.ProjectReferences} ` +
        `unique-projects=${Summary.UniqueProjects} source-references=${Summary.SourceReferences} ` +
        `unique-sources=${Summary.UniqueSources} manifest-duplication=${Summary.ManifestDuplication} ` +
        `all-paired-source-visits=${Summary.AllPairedSourceVisits} ` +
        `portable-single-construction-steps=${Summary.PortableSingleConstructionSteps} ` +
        `portable-duplicate-source-visits-delegated=${Summary.PortableDuplicateSourceVisitsDelegated} ` +
        `dependency-edges=${Summary.DependencyEdges} ` +
        `steps-with-dependencies=${Summary.StepsWithDependencies}\n`,
    );
    for (const Source of RepeatedSources) {
        process.stdout.write(
            `database qualification repeated-source uses=${Source.Uses} path=${Source.Path}\n`,
        );
    }
    for (const Group of SharedClosureCandidates) {
        process.stdout.write(
            `database qualification shared-closure steps=${Group.Steps.length} ` +
            `sources=${Group.SourceCount} cases=${Group.Cases.join(',')}\n`,
        );
    }
    for (const Candidate of Summary.OverlapMergeCandidates) {
        process.stdout.write(
            `database qualification overlap-candidate ` +
            `steps=${Candidate.Steps.join(',')} ` +
            `shared-sources=${Candidate.SharedSources} ` +
            `union-sources=${Candidate.UnionSources} ` +
            `shared-bytes=${Candidate.SharedSourceBytes} ` +
            `union-bytes=${Candidate.UnionSourceBytes} ` +
            `visit-reduction-bps=${Candidate.DeclarationVisitReductionBasisPoints}\n`,
        );
    }
} else {
    process.stderr.write(
        'Usage: node Tools/Native/Plan-Database-Storage-Qualification.mjs ' +
        '[--json|--counts|--rows <portable|hosted>|--row-env|--closure-env]\n',
    );
    process.exit(64);
}
