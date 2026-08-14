import { createHash } from 'node:crypto';
import { readFile, realpath, stat } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { TextDecoder } from 'node:util';

const MAXIMUM_INPUT_BYTES = 67_108_864;
const SCRIPT_PATH = fileURLToPath(import.meta.url);
const REPOSITORY_ROOT = path.resolve(path.dirname(SCRIPT_PATH), '..', '..');
const UTF8 = new TextDecoder('utf-8', { fatal: true });
const WINDOWS = process.platform === 'win32';

function Fail(message) {
    process.stderr.write(`${message}\n`);
    process.exit(1);
}

function Isˉwithinˉrepository(candidate) {
    const relative = path.relative(REPOSITORY_ROOT, candidate);
    return relative !== '' && relative !== '..' &&
        !relative.startsWith(`..${path.sep}`) && !path.isAbsolute(relative);
}

function Isˉsameˉpath(left, right) {
    return WINDOWS ? left.toLowerCase() === right.toLowerCase() : left === right;
}

async function Readˉboundedˉordinaryˉfile(candidate, label) {
    let information;
    try {
        information = await stat(candidate);
    } catch {
        Fail(`Missing ${label}: ${candidate}`);
    }
    if (!information.isFile() || information.size < 1 ||
        information.size > MAXIMUM_INPUT_BYTES) {
        Fail(`The ${label} is not a bounded ordinary file: ${candidate}`);
    }
    return readFile(candidate);
}

function Addˉfield(hash, label, bytes) {
    const labelBytes = Buffer.from(label, 'utf8');
    const frame = Buffer.alloc(16);
    frame.writeBigUInt64LE(BigInt(labelBytes.length), 0);
    frame.writeBigUInt64LE(BigInt(bytes.length), 8);
    hash.update(frame);
    hash.update(labelBytes);
    hash.update(bytes);
}

if (process.argv.length < 5) {
    Fail('Usage: node Tools/Native/Get-Native-Project-Cache-Key.mjs <namespace> <project.wvproj> <producer>...');
}

const namespace = process.argv[2];
if (!/^[a-z0-9][a-z0-9-]{0,63}$/.test(namespace)) {
    Fail('The cache-key namespace is invalid.');
}

const repositoryReal = await realpath(REPOSITORY_ROOT);
if (!Isˉsameˉpath(repositoryReal, REPOSITORY_ROOT)) {
    Fail('The repository root must use its canonical path.');
}

const projectPath = path.resolve(process.argv[3]);
if (path.extname(projectPath).toLowerCase() !== '.wvproj' ||
    !Isˉwithinˉrepository(projectPath)) {
    Fail('The cache-key project must be a repository-owned .wvproj file.');
}
const projectReal = await realpath(projectPath).catch(() => '');
if (!Isˉsameˉpath(projectReal, projectPath)) {
    Fail('The cache-key project must use its canonical non-link path.');
}

const projectBytes = await Readˉboundedˉordinaryˉfile(projectPath, 'project manifest');
let projectText;
try {
    projectText = UTF8.decode(projectBytes);
} catch {
    Fail('The cache-key project manifest is not valid UTF-8.');
}

const declaredPaths = [];
let rootCount = 0;
for (const line of projectText.split(/\r?\n/u)) {
    if (!line.startsWith('root ') && !line.startsWith('source ')) {
        continue;
    }
    const match = /^(root|source) "([^"\r\n]+)"$/u.exec(line);
    if (match === null) {
        Fail('The cache-key project contains a malformed source declaration.');
    }
    if (match[1] === 'root') {
        rootCount += 1;
    }
    const declared = match[2];
    if (declared.includes('\\') || path.posix.isAbsolute(declared) ||
        declared.split('/').some(part => part === '' || part === '.' || part === '..')) {
        Fail(`The cache-key project source path is not canonical: ${declared}`);
    }
    declaredPaths.push(declared);
}
if (rootCount !== 1 || declaredPaths.length < 1) {
    Fail('The cache-key project must declare exactly one root and at least one source input.');
}

const hash = createHash('sha256');
Addˉfield(hash, 'format', Buffer.from('windvale-native-project-cache-key 1\n', 'ascii'));
Addˉfield(hash, 'namespace', Buffer.from(namespace, 'ascii'));
Addˉfield(hash, 'workspace', await Readˉboundedˉordinaryˉfile(
    path.join(REPOSITORY_ROOT, 'Windvale.wvws'), 'workspace marker'));
Addˉfield(hash, `project:${path.relative(REPOSITORY_ROOT, projectPath).replaceAll('\\', '/')}`,
    projectBytes);

for (const declared of declaredPaths) {
    const sourcePath = path.resolve(REPOSITORY_ROOT, ...declared.split('/'));
    if (!Isˉwithinˉrepository(sourcePath)) {
        Fail(`The cache-key source escapes the repository: ${declared}`);
    }
    const sourceReal = await realpath(sourcePath).catch(() => '');
    if (!Isˉsameˉpath(sourceReal, sourcePath)) {
        Fail(`The cache-key source must use its canonical non-link path: ${declared}`);
    }
    Addˉfield(hash, `source:${declared}`,
        await Readˉboundedˉordinaryˉfile(sourcePath, 'project source'));
}

for (let index = 4; index < process.argv.length; index += 1) {
    const producerPath = path.resolve(process.argv[index]);
    const producerReal = await realpath(producerPath).catch(() => '');
    if (!Isˉsameˉpath(producerReal, producerPath)) {
        Fail(`The cache-key producer must use its canonical non-link path: ${producerPath}`);
    }
    Addˉfield(hash, `producer:${index - 4}`,
        await Readˉboundedˉordinaryˉfile(producerPath, 'producer'));
}

process.stdout.write(`${hash.digest('hex')}\n`);
