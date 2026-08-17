import { createHash } from 'node:crypto';
import { readFile, realpath, stat } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { TextDecoder } from 'node:util';

const MAXIMUM_INPUT_BYTES = 67_108_864;
const SCRIPT_PATH = fileURLToPath(import.meta.url);
export const REPOSITORY_ROOT = path.resolve(path.dirname(SCRIPT_PATH), '..', '..');
const UTF8 = new TextDecoder('utf-8', { fatal: true });
const WINDOWS = process.platform === 'win32';

function Reject(message) {
    throw new Error(message);
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
        Reject(`Missing ${label}: ${candidate}`);
    }
    if (!information.isFile() || information.size < 1 ||
        information.size > MAXIMUM_INPUT_BYTES) {
        Reject(`The ${label} is not a bounded ordinary file: ${candidate}`);
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

export async function Getˉnativeˉprojectˉcacheˉkey(
    namespace,
    projectInput,
    producerInputs
) {
    if (!/^[a-z0-9][a-z0-9-]{0,63}$/.test(namespace)) {
        Reject('The cache-key namespace is invalid.');
    }
    if (!Array.isArray(producerInputs) || producerInputs.length < 1) {
        Reject('The cache key requires at least one producer.');
    }

    const repositoryReal = await realpath(REPOSITORY_ROOT);
    if (!Isˉsameˉpath(repositoryReal, REPOSITORY_ROOT)) {
        Reject('The repository root must use its canonical path.');
    }

    const projectPath = path.resolve(projectInput);
    if (path.extname(projectPath).toLowerCase() !== '.wvproj' ||
        !Isˉwithinˉrepository(projectPath)) {
        Reject('The cache-key project must be a repository-owned .wvproj file.');
    }
    const projectReal = await realpath(projectPath).catch(() => '');
    if (!Isˉsameˉpath(projectReal, projectPath)) {
        Reject('The cache-key project must use its canonical non-link path.');
    }

    const projectBytes = await Readˉboundedˉordinaryˉfile(
        projectPath,
        'project manifest'
    );
    let projectText;
    try {
        projectText = UTF8.decode(projectBytes);
    } catch {
        Reject('The cache-key project manifest is not valid UTF-8.');
    }

    const declaredPaths = [];
    let rootCount = 0;
    for (const line of projectText.split(/\r?\n/u)) {
        if (!line.startsWith('root ') && !line.startsWith('source ')) {
            continue;
        }
        const match = /^(root|source) "([^"\r\n]+)"$/u.exec(line);
        if (match === null) {
            Reject('The cache-key project contains a malformed source declaration.');
        }
        if (match[1] === 'root') {
            rootCount += 1;
        }
        const declared = match[2];
        if (declared.includes('\\') || path.posix.isAbsolute(declared) ||
            declared.split('/').some(part =>
                part === '' || part === '.' || part === '..')) {
            Reject(`The cache-key project source path is not canonical: ${declared}`);
        }
        declaredPaths.push(declared);
    }
    if (rootCount !== 1 || declaredPaths.length < 1) {
        Reject(
            'The cache-key project must declare exactly one root and at least one source input.'
        );
    }

    const hash = createHash('sha256');
    Addˉfield(
        hash,
        'format',
        Buffer.from('windvale-native-project-cache-key 1\n', 'ascii')
    );
    Addˉfield(hash, 'namespace', Buffer.from(namespace, 'ascii'));
    Addˉfield(
        hash,
        'workspace',
        await Readˉboundedˉordinaryˉfile(
            path.join(REPOSITORY_ROOT, 'Windvale.wvws'),
            'workspace marker'
        )
    );
    Addˉfield(
        hash,
        `project:${path.relative(REPOSITORY_ROOT, projectPath).replaceAll('\\', '/')}`,
        projectBytes
    );

    for (const declared of declaredPaths) {
        const sourcePath = path.resolve(REPOSITORY_ROOT, ...declared.split('/'));
        if (!Isˉwithinˉrepository(sourcePath)) {
            Reject(`The cache-key source escapes the repository: ${declared}`);
        }
        const sourceReal = await realpath(sourcePath).catch(() => '');
        if (!Isˉsameˉpath(sourceReal, sourcePath)) {
            Reject(
                `The cache-key source must use its canonical non-link path: ${declared}`
            );
        }
        Addˉfield(
            hash,
            `source:${declared}`,
            await Readˉboundedˉordinaryˉfile(sourcePath, 'project source')
        );
    }

    for (let index = 0; index < producerInputs.length; index += 1) {
        const producerPath = path.resolve(producerInputs[index]);
        const producerReal = await realpath(producerPath).catch(() => '');
        if (!Isˉsameˉpath(producerReal, producerPath)) {
            Reject(
                `The cache-key producer must use its canonical non-link path: ${producerPath}`
            );
        }
        Addˉfield(
            hash,
            `producer:${index}`,
            await Readˉboundedˉordinaryˉfile(producerPath, 'producer')
        );
    }

    return hash.digest('hex');
}
