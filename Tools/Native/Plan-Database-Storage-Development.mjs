import { readFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

if (process.argv.length !== 3) {
    process.stderr.write(
        'Usage: node Tools/Native/Plan-Database-Storage-Development.mjs <target-set>\n',
    );
    process.exit(64);
}

const SCRIPT_DIRECTORY = dirname(fileURLToPath(import.meta.url));
const PLAN_PATH = join(
    dirname(dirname(SCRIPT_DIRECTORY)),
    'Tests',
    'Native',
    'Database-Storage-Development-Cases.txt',
);
const QUALIFICATION_PLAN_PATH = join(
    dirname(dirname(SCRIPT_DIRECTORY)),
    'Tests',
    'Native',
    'Database-Storage-Qualification-Steps.txt',
);
const lines = readFileSync(PLAN_PATH, 'utf8').split(/\r?\n/u);
if (lines.at(-1) === '') lines.pop();
if (lines.length !== 54 ||
    lines[0] !== 'windvale-database-storage-development-cases 3') {
    throw new Error('The database development case inventory header or size differs.');
}

const cases = [];
const caseNames = new Set();
const knownTargets = new Set();
for (const line of lines.slice(1)) {
    const fields = line.split('|');
    if ((fields.length !== 3 && fields.length !== 4) ||
        !/^[A-Z][A-Za-z0-9]*$/u.test(fields[0]) ||
        !/^(?:portable|hosted)$/u.test(fields[1])) {
        throw new Error(`Invalid database development case row: ${line}`);
    }
    const selectors = fields[2].split(',');
    if (selectors.length === 0 || selectors.some(selector =>
        !/^[a-z0-9][a-z0-9-]*$/u.test(selector))) {
        throw new Error(`Invalid database development selector row: ${line}`);
    }
    if (new Set(selectors).size !== selectors.length ||
        caseNames.has(fields[0])) {
        throw new Error(`Duplicate database development case data: ${line}`);
    }
    const bundle = fields.length === 4 ? fields[3] : '-';
    if (bundle !== '-' &&
        (!/^[A-Z][A-Za-z0-9]*$/u.test(bundle) || fields[1] !== 'portable')) {
        throw new Error(`Invalid database development bundle: ${line}`);
    }
    caseNames.add(fields[0]);
    for (const selector of selectors) knownTargets.add(selector);
    cases.push({ Name: fields[0], Lane: fields[1], Selectors: selectors, Bundle: bundle });
}

const bundles = new Map();
for (const entry of cases.filter(entry => entry.Bundle !== '-')) {
    const members = bundles.get(entry.Bundle) ?? [];
    members.push(entry.Name);
    bundles.set(entry.Bundle, members);
}
const qualificationLines = readFileSync(QUALIFICATION_PLAN_PATH, 'utf8')
    .split(/\r?\n/u)
    .filter(line => line.length !== 0);
if (qualificationLines.shift() !==
    'windvale-database-storage-qualification-steps 3') {
    throw new Error('The database qualification inventory header differs.');
}
const qualificationBundles = new Map();
for (const line of qualificationLines) {
    const fields = line.split('|');
    if (fields.length >= 6 && bundles.has(fields[0])) {
        if (fields[1] !== 'portable' || fields[2] !== 'case' ||
            !['project', 'segmented-project'].includes(fields[3])) {
            throw new Error(`Invalid database qualification bundle row: ${line}`);
        }
        qualificationBundles.set(fields[0], fields[5].split(','));
    }
}
for (const [bundle, members] of bundles) {
    const qualificationMembers = qualificationBundles.get(bundle);
    if (members.length < 2 || qualificationMembers === undefined ||
        members.join(',') !== qualificationMembers.join(',')) {
        throw new Error(`Database development bundle ${bundle} differs from qualification.`);
    }
}

const requestedText = process.argv[2];
let requested;
if (requestedText === 'all') {
    requested = ['all'];
} else {
    requested = requestedText.split('+');
    if (requested.length === 0 || requested.some(target =>
        !/^[a-z0-9][a-z0-9-]*$/u.test(target) ||
        !knownTargets.has(target)) ||
        new Set(requested).size !== requested.length) {
        process.stderr.write(
            `Unknown or duplicate database development target set: ${requestedText}.\n`,
        );
        process.exit(64);
    }
    requested.sort();
}

const selected = requested[0] === 'all'
    ? cases
    : cases.filter(entry => entry.Selectors.some(selector =>
        requested.includes(selector)));
if (selected.length === 0) {
    throw new Error('The database development target set selected no cases.');
}

const selectedNames = new Set(selected.map(entry => entry.Name));
const selectedBundles = [...bundles]
    .filter(([, members]) => members.every(member => selectedNames.has(member)));
const bundledCases = selectedBundles.flatMap(([, members]) => members);
const selectedExecutions = selected.length - bundledCases.length + selectedBundles.length;

process.stdout.write(
    `windvale-database-storage-development-plan-2|${requested.join('+')}|` +
    `${selected.length}|${selectedExecutions}|` +
    `${selected.map(entry => entry.Name).join(',')}|` +
    `${selectedBundles.length === 0
        ? '-'
        : selectedBundles.map(([bundle]) => bundle).join(',')}|` +
    `${bundledCases.length === 0 ? '-' : bundledCases.join(',')}\n`,
);
