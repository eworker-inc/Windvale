import fs from 'node:fs';
import path from 'node:path';
import { execFileSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';

const ROOT = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../..');
const REGISTRY = path.join(ROOT, 'Documents/Project/Toolchain-Retirement.json');
const MAXIMUM_FILE_BYTES = 8_388_608;
const MAXIMUM_TOTAL_BYTES = 268_435_456;
const Arguments = process.argv.slice(2);
if (Arguments.length > 1 || (Arguments.length === 1 && !['--update', '--check'].includes(Arguments[0]))) {
    throw new Error('Usage: node Tools/Documentation/Inspect-Toolchain-Retirement.mjs [--check | --update]');
}
function Read(Path) {
    if (fs.statSync(Path).size > MAXIMUM_FILE_BYTES) throw new Error('Retirement input exceeds8MiB: ' + Path);
    return fs.readFileSync(Path, 'utf8');
}
function Resolve(Relative) {
    if (typeof Relative !== 'string' || !/^[A-Za-z0-9][A-Za-z0-9_./-]*$/u.test(Relative) ||
        Relative.split('/').some(Part => Part === '.' || Part === '..')) throw new Error('Invalid retirement path.');
    return path.join(ROOT, Relative);
}
const Registry = JSON.parse(Read(REGISTRY));
if (Registry.Format !== 'windvale-toolchain-retirement-1' || !Array.isArray(Registry.Entries) ||
    Registry.Entries.length < 1 || Registry.Entries.length > 32) throw new Error('Invalid retirement registry.');
const Paths = execFileSync('git', ['-C', ROOT, 'ls-files', '--cached', '--others', '--exclude-standard', '-z'],
    { maxBuffer: MAXIMUM_FILE_BYTES }).toString('utf8').split('\0').filter(Boolean);
const Texts = new Map();
let Total = 0;
for (const Relative of new Set(Paths)) {
    if (!/^(Compiler|Runtime|Foundation|Libraries|Tools|Tests|Projects|Linker|Assembler|Applications|Operating-System)\//u.test(Relative) ||
        !/\.(wv|wva|wvproj|mjs|ps1|cmd|sh|wvws)$/u.test(Relative)) continue;
    const Full = Resolve(Relative);
    if (!fs.existsSync(Full)) continue;
    Total += fs.statSync(Full).size;
    if (Total > MAXIMUM_TOTAL_BYTES) throw new Error('Retirement inventory exceeds256MiB.');
    Texts.set(Relative, Read(Full));
}
const Identifiers = new Set();
let Changed = false;
for (const Entry of Registry.Entries) {
    if (!/^(MEM|BOOT)-[0-9]{3}$/u.test(Entry.Id) || Identifiers.has(Entry.Id) ||
        !['Planned', 'Migrating', 'RequiredBootstrap', 'ReadyToRetire'].includes(Entry.Status) ||
        !Array.isArray(Entry.Owners) || Entry.Owners.length < 1 || Entry.Owners.length > 16) {
        throw new Error('Invalid retirement entry.');
    }
    Identifiers.add(Entry.Id);
    const Owners = new Set(), Modules = new Set();
    for (const Owner of Entry.Owners) {
        Resolve(Owner.Path);
        const Text = Texts.get(Owner.Path);
        if (!Text || typeof Owner.Anchor !== 'string' || !Text.includes(Owner.Anchor)) {
            throw new Error('Retirement owner anchor missing: ' + Entry.Id + ' ' + Owner.Path);
        }
        Owners.add(Owner.Path);
        const Module = /^module ([^ ;\r\n]+)/mu.exec(Text)?.[1];
        if (Module) Modules.add(Module);
    }
    for (const Replacement of Entry.Replacements) {
        if (!fs.existsSync(Resolve(Replacement))) throw new Error('Replacement path missing: ' + Replacement);
    }
    for (const Scope of Entry.ConvertedScopes ?? []) {
        for (const Anchor of Scope.Anchors) {
            if (!Texts.get(Scope.Path)?.includes(Anchor)) throw new Error('Converted scope anchor missing: ' + Scope.Path);
        }
        if (!fs.existsSync(Resolve(Scope.Evidence))) throw new Error('Converted scope evidence missing.');
    }
    const References = [];
    for (const [File, Text] of Texts) {
        if (Owners.has(File)) continue;
        const Reasons = [];
        for (const Module of Modules) {
            if (Text.split(/\r?\n/u).some(Line => Line.startsWith('import ' + Module + ';') ||
                Line.startsWith('import ' + Module + ' as '))) Reasons.push('import ' + Module);
        }
        for (const Owner of Owners) {
            if (Text.includes(Owner) || Text.includes(Owner.replaceAll('/', '\\')) ||
                Text.includes(path.posix.basename(Owner))) Reasons.push('path ' + Owner);
        }
        if (Reasons.length) References.push({ Path: File, Kind: File.endsWith('.wvproj') ? 'Project' :
            File.endsWith('.wv') ? 'Source' : 'HostTool', Status: 'NeedsReview', References: Reasons });
    }
    const Sources = new Set([...Owners, ...References.filter(Reference => Reference.Kind === 'Source').map(Reference => Reference.Path)]);
    for (const [File, Text] of Texts) {
        if (!File.endsWith('.wvproj') || References.some(Reference => Reference.Path === File)) continue;
        const Includes = [...Text.matchAll(/^(?:root|source) "([^"\r\n]+)"$/gmu)].map(Match => Match[1]).filter(Source => Sources.has(Source));
        if (Includes.length) References.push({ Path: File, Kind: 'Project', Status: 'NeedsReview',
            References: Includes.map(Source => 'includes ' + Source) });
    }
    References.sort((Left, Right) => Left.Path < Right.Path ? -1 : Left.Path > Right.Path ? 1 : 0);
    const Modulesˉcurrent = [...Modules];
    const Drift = JSON.stringify(Entry.References) !== JSON.stringify(References) ||
        JSON.stringify(Entry.ReferenceModules) !== JSON.stringify(Modulesˉcurrent);
    Changed ||= Drift;
    console.log(Entry.Id + ' status=' + Entry.Status + ' owners=' + Owners.size +
        ' references=' + References.length + ' inventory=' + (Drift ? 'Changed' : 'Current'));
    if (Arguments[0] === '--update') {
        Entry.References = References;
        Entry.ReferenceModules = Modulesˉcurrent;
    }
}
if (Arguments[0] === '--update') {
    delete Registry.SourceRevision;
    Registry.BaseRevision = execFileSync('git', ['-C', ROOT, 'rev-parse', 'HEAD']).toString().trim();
    Registry.Scope = 'Tracked and non-ignored working source/project/host-tool files in the declared repository areas; module imports, owner paths/names and project inclusion of matching source consumers.';
    fs.writeFileSync(REGISTRY, JSON.stringify(Registry, null, 2) + '\n');
} else if (Arguments[0] === '--check' && Changed) {
    process.exitCode = 1;
}
