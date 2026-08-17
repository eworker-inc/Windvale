import { createHash } from 'node:crypto';
import { createReadStream } from 'node:fs';
import { readFile, realpath, stat } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { TextDecoder } from 'node:util';

const MAXIMUM_INPUT_BYTES = 67_108_864;
const MAXIMUM_PRODUCER_BYTES = 134_217_728;
const MAXIMUM_PROJECT_BYTES = 268_435_456;
const MAXIMUM_PRODUCERS = 16;
const MAXIMUM_PROJECT_INPUTS = 1_024;
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

function Addˉfieldˉheader(hash, label, byteCount) {
    const labelBytes = Buffer.from(label, 'utf8');
    const frame = Buffer.alloc(16);
    frame.writeBigUInt64LE(BigInt(labelBytes.length), 0);
    frame.writeBigUInt64LE(BigInt(byteCount), 8);
    hash.update(frame);
    hash.update(labelBytes);
}

function Addˉfield(hash, label, bytes) {
    Addˉfieldˉheader(hash, label, bytes.length);
    hash.update(bytes);
}

async function Addˉfileˉfield(
    hash,
    label,
    candidate,
    inputLabel,
    remainingAggregateBytes
) {
    let information;
    try {
        information = await stat(candidate);
    } catch {
        Reject(`Missing ${inputLabel}: ${candidate}`);
    }
    if (!information.isFile() || information.size < 1 ||
        information.size > MAXIMUM_INPUT_BYTES) {
        Reject(`The ${inputLabel} is not a bounded ordinary file: ${candidate}`);
    }
    if (information.size > remainingAggregateBytes) {
        Reject('The cache-key producer set exceeds 128 MiB.');
    }
    Addˉfieldˉheader(hash, label, information.size);
    const digest = createHash('sha256');
    let measuredBytes = 0;
    for await (const chunk of createReadStream(candidate, {
        highWaterMark: 1_048_576
    })) {
        measuredBytes += chunk.length;
        if (measuredBytes > information.size) {
            Reject(`The ${inputLabel} grew while it was hashed: ${candidate}`);
        }
        hash.update(chunk);
        digest.update(chunk);
    }
    if (measuredBytes !== information.size) {
        Reject(`The ${inputLabel} size changed while it was hashed: ${candidate}`);
    }
    return {
        bytes: measuredBytes,
        path: candidate,
        sha256: digest.digest('hex')
    };
}

function Evidence(candidate, bytes) {
    return {
        bytes: bytes.length,
        path: candidate,
        sha256: createHash('sha256').update(bytes).digest('hex')
    };
}

async function Requireˉevidenceˉunchanged(evidence, label) {
    const bytes = await Readˉboundedˉordinaryˉfile(evidence.path, label);
    if (bytes.length !== evidence.bytes ||
        createHash('sha256').update(bytes).digest('hex') !== evidence.sha256) {
        Reject(`The ${label} changed during project checkpoint publication.`);
    }
}

export async function Prepareˉnativeˉprojectˉcacheˉcontext(
    namespace,
    producerInputs
) {
    if (!/^[a-z0-9][a-z0-9-]{0,63}$/.test(namespace)) {
        Reject('The cache-key namespace is invalid.');
    }
    if (!Array.isArray(producerInputs) || producerInputs.length < 1 ||
        producerInputs.length > MAXIMUM_PRODUCERS) {
        Reject(`The cache key requires one through ${MAXIMUM_PRODUCERS} producers.`);
    }

    const repositoryReal = await realpath(REPOSITORY_ROOT);
    if (!Isˉsameˉpath(repositoryReal, REPOSITORY_ROOT)) {
        Reject('The repository root must use its canonical path.');
    }

    const hash = createHash('sha256');
    Addˉfield(
        hash,
        'format',
        Buffer.from('windvale-native-project-cache-key 2\n', 'ascii')
    );
    Addˉfield(hash, 'namespace', Buffer.from(namespace, 'ascii'));
    const workspacePath = path.join(REPOSITORY_ROOT, 'Windvale.wvws');
    const workspaceBytes = await Readˉboundedˉordinaryˉfile(
        workspacePath,
        'workspace marker'
    );
    Addˉfield(hash, 'workspace', workspaceBytes);
    Addˉfield(
        hash,
        'producer-count',
        Buffer.from(String(producerInputs.length), 'ascii')
    );

    const producerEvidence = [];
    let producerBytes = 0;
    for (let index = 0; index < producerInputs.length; index += 1) {
        const producerPath = path.resolve(producerInputs[index]);
        const producerReal = await realpath(producerPath).catch(() => '');
        if (!Isˉsameˉpath(producerReal, producerPath)) {
            Reject(
                `The cache-key producer must use its canonical non-link path: ${producerPath}`
            );
        }
        const evidence = await Addˉfileˉfield(
            hash,
            `producer:${index}`,
            producerPath,
            'producer',
            MAXIMUM_PRODUCER_BYTES - producerBytes
        );
        producerBytes += evidence.bytes;
        producerEvidence.push(evidence);
    }
    return {
        hash,
        namespace,
        producerEvidence,
        workspaceEvidence: Evidence(workspacePath, workspaceBytes)
    };
}

export async function Getˉnativeˉprojectˉcacheˉrequest(
    context,
    projectInput
) {
    if (context === null || typeof context !== 'object' ||
        context.hash === undefined ||
        !Array.isArray(context.producerEvidence)) {
        Reject('The native project cache context is invalid.');
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
        if (declaredPaths.length > MAXIMUM_PROJECT_INPUTS) {
            Reject(
                `The cache-key project exceeds ${MAXIMUM_PROJECT_INPUTS} source inputs.`
            );
        }
    }
    if (rootCount !== 1 || declaredPaths.length < 1) {
        Reject(
            'The cache-key project must declare exactly one root and at least one source input.'
        );
    }

    const hash = context.hash.copy();
    Addˉfield(
        hash,
        `project:${path.relative(REPOSITORY_ROOT, projectPath).replaceAll('\\', '/')}`,
        projectBytes
    );

    const inputEvidence = [Evidence(projectPath, projectBytes)];
    let inputBytes = projectBytes.length;
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
        const sourceBytes = await Readˉboundedˉordinaryˉfile(
            sourcePath,
            'project source'
        );
        inputBytes += sourceBytes.length;
        if (inputBytes > MAXIMUM_PROJECT_BYTES) {
            Reject('The cache-key project input set exceeds 256 MiB.');
        }
        Addˉfield(hash, `source:${declared}`, sourceBytes);
        inputEvidence.push(Evidence(sourcePath, sourceBytes));
    }

    return {
        context,
        inputEvidence,
        key: hash.digest('hex'),
        projectPath
    };
}

export async function Requireˉnativeˉprojectˉcacheˉrequestˉunchanged(request) {
    await Requireˉevidenceˉunchanged(
        request.context.workspaceEvidence,
        'workspace marker'
    );
    for (const evidence of request.context.producerEvidence) {
        await Requireˉevidenceˉunchanged(evidence, 'producer');
    }
    for (const evidence of request.inputEvidence) {
        await Requireˉevidenceˉunchanged(evidence, 'project input');
    }
}

export async function Getˉnativeˉprojectˉcacheˉkey(
    namespace,
    projectInput,
    producerInputs
) {
    const context = await Prepareˉnativeˉprojectˉcacheˉcontext(
        namespace,
        producerInputs
    );
    return (await Getˉnativeˉprojectˉcacheˉrequest(
        context,
        projectInput
    )).key;
}
