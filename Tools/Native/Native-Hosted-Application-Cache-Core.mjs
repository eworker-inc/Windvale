import { createHash } from 'node:crypto';
import { lstat, open, realpath } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { TextDecoder } from 'node:util';

export const MAXIMUM_HOSTED_INPUT_BYTES = 67_108_864;
export const MAXIMUM_HOSTED_FRAGMENT_BYTES = 4_194_304;
const SCRIPT_PATH = fileURLToPath(import.meta.url);
export const HOSTED_REPOSITORY_ROOT = path.resolve(
    path.dirname(SCRIPT_PATH),
    '..',
    '..'
);
const UTF8 = new TextDecoder('utf-8', { fatal: true });
const WINDOWS = process.platform === 'win32';
const PRODUCER_READ_CONCURRENCY = 4;

function Reject(message) {
    throw new Error(message);
}

function Isˉwithinˉrepository(candidate) {
    const relative = path.relative(HOSTED_REPOSITORY_ROOT, candidate);
    return relative !== '' && relative !== '..' &&
        !relative.startsWith(`..${path.sep}`) && !path.isAbsolute(relative);
}

export function Isˉsameˉhostedˉpath(left, right) {
    return WINDOWS ? left.toLowerCase() === right.toLowerCase() : left === right;
}

export async function Readˉboundedˉhostedˉfile(
    candidate,
    label,
    maximumBytes = MAXIMUM_HOSTED_INPUT_BYTES,
    allowWindowsAlias = false,
    requireExecutable = false
) {
    const absolute = path.resolve(candidate);
    let information;
    try {
        information = await lstat(absolute);
    } catch {
        Reject(`Missing ${label}: ${absolute}`);
    }
    if (information.isSymbolicLink() || !information.isFile() ||
        information.size < 1 || information.size > maximumBytes ||
        (requireExecutable && !WINDOWS && (information.mode & 0o111) === 0)) {
        Reject(`The ${label} is not a bounded ordinary file: ${absolute}`);
    }
    const canonical = await realpath(absolute).catch(() => '');
    if (!Isˉsameˉhostedˉpath(canonical, absolute) &&
        !(WINDOWS && allowWindowsAlias)) {
        Reject(`The ${label} must use its canonical non-link path: ${absolute}`);
    }
    const handle = await open(WINDOWS && allowWindowsAlias ? canonical : absolute, 'r');
    try {
        const opened = await handle.stat();
        if (!opened.isFile() || opened.dev !== information.dev ||
            opened.ino !== information.ino || opened.size !== information.size ||
            (requireExecutable && !WINDOWS && (opened.mode & 0o111) === 0)) {
            Reject(`The ${label} changed before it was opened: ${absolute}`);
        }
        // One extra byte detects growth without allowing unbounded readFile allocation.
        const bytes = Buffer.allocUnsafe(opened.size + 1);
        let count = 0;
        while (count < bytes.length) {
            const result = await handle.read(bytes, count, bytes.length - count, count);
            if (result.bytesRead === 0) break;
            count += result.bytesRead;
        }
        if (count !== opened.size) {
            Reject(`The ${label} changed length while being read: ${absolute}`);
        }
        return bytes.subarray(0, count);
    } finally {
        await handle.close();
    }

}

export function Addˉhostedˉkeyˉfield(hash, label, bytes) {
    const labelBytes = Buffer.from(label, 'utf8');
    const frame = Buffer.alloc(16);
    frame.writeBigUInt64LE(BigInt(labelBytes.length), 0);
    frame.writeBigUInt64LE(BigInt(bytes.length), 8);
    hash.update(frame);
    hash.update(labelBytes);
    hash.update(bytes);
}

function Repositoryˉpath(relative) {
    const candidate = path.resolve(
        HOSTED_REPOSITORY_ROOT,
        ...relative.split('/')
    );
    if (!Isˉwithinˉrepository(candidate)) {
        Reject(`The producer path escapes the repository: ${relative}`);
    }
    return candidate;
}

async function Readˉrepositoryˉproducer(relative) {
    return Readˉboundedˉhostedˉfile(
        Repositoryˉpath(relative),
        'hosted application producer'
    );
}

function Producerˉfingerprint(label, bytes, digest) {
    const fingerprint = Buffer.alloc(40);
    fingerprint.writeBigUInt64LE(BigInt(bytes.length), 0);
    (digest ?? createHash('sha256').update(bytes).digest()).copy(fingerprint, 8);
    return { label, bytes: fingerprint };
}

function Addˉproducerˉfield(fields, label, bytes) {
    fields.push(Producerˉfingerprint(label, bytes));
}

export async function Prepareˉhostedˉapplicationˉcontext(
    target,
    packagerInput
) {
    if (target !== 'windows' && target !== 'linux') {
        Reject('The hosted-application cache-key target is invalid.');
    }
    const repositoryReal = await realpath(HOSTED_REPOSITORY_ROOT);
    if (!Isˉsameˉhostedˉpath(repositoryReal, HOSTED_REPOSITORY_ROOT)) {
        Reject('The repository root must use its canonical path.');
    }

    const hostFamily = WINDOWS ? 'windows-x64' : 'linux-x64';
    const packagerPath = path.resolve(packagerInput);
    const expectedPackager = path.join(
        HOSTED_REPOSITORY_ROOT,
        'Tools',
        'Native',
        `Package-Hosted-Wvb.${WINDOWS ? 'cmd' : 'sh'}`
    );
    if (!Isˉsameˉhostedˉpath(packagerPath, expectedPackager)) {
        Reject(
            'The hosted-application cache-key packager is not the current-host front door.'
        );
    }

    const producerFields = [];
    Addˉproducerˉfield(
        producerFields,
        'producer:Package-Hosted-Wvb',
        await Readˉboundedˉhostedˉfile(
            packagerPath,
            'hosted packager'
        )
    );

    const inventoryRelative =
        'Artifacts/Native-Hosted-Container-Toolset-Candidate/SHA256SUMS';
    const inventoryBytes = await Readˉboundedˉhostedˉfile(
        Repositoryˉpath(inventoryRelative),
        'hosted toolset inventory'
    );
    Addˉproducerˉfield(
        producerFields,
        `producer:${inventoryRelative}`,
        inventoryBytes
    );
    let inventoryText;
    try {
        inventoryText = UTF8.decode(inventoryBytes);
    } catch {
        Reject('The hosted toolset inventory is not valid UTF-8.');
    }
    const inventoryEntries = inventoryText.trimEnd().split(/\r?\n/u);
    if (inventoryEntries.length !== 72) {
        Reject('The hosted toolset inventory does not contain exactly 72 entries.');
    }
    const producerEntries = inventoryEntries.map(line => {
        const match = /^([0-9a-f]{64})  ([A-Za-z0-9._/-]+)$/u.exec(line);
        if (match === null || match[2].split('/').some(part =>
            part === '' || part === '.' || part === '..')) {
            Reject('The hosted toolset inventory contains a malformed entry.');
        }
        return { digest: match[1], path: match[2] };
    });
    for (let offset = 0; offset < producerEntries.length; offset += PRODUCER_READ_CONCURRENCY) {
        const batch = await Promise.allSettled(producerEntries
            .slice(offset, offset + PRODUCER_READ_CONCURRENCY)
            .map(async entry => {
                const relative = 'Artifacts/Native-Hosted-Container-Toolset-Candidate/' + entry.path;
                const bytes = await Readˉrepositoryˉproducer(relative);
                const digest = createHash('sha256').update(bytes).digest();
                if (digest.toString('hex') !== entry.digest) {
                    Reject('The hosted toolset artifact differs from its inventory: ' + entry.path);
                }
                return Producerˉfingerprint('producer:' + relative, bytes, digest);
            }));
        // Await the entire bounded batch and keep inventory order in the key.
        for (const result of batch) {
            if (result.status === 'rejected') throw result.reason;
            producerFields.push(result.value);
        }
    }

    for (const relative of [
        'Artifacts/Native-Hosted-Enum-Request-Candidate/Wvb/wvhostenumrequest.wvb',
        'Artifacts/Native-Hosted-Enum-Request-Candidate/windows-x64/wvhostenumrequest.exe',
        'Artifacts/Native-Hosted-Enum-Request-Candidate/linux-x64/wvhostenumrequest.elf'
    ]) {
        Addˉproducerˉfield(
            producerFields,
            `producer:${relative}`,
            await Readˉrepositoryˉproducer(relative)
        );
    }

    const platform = target === 'windows' ? 'Windows' : 'Linux';
    for (const leaf of [
        `Native-X64-${platform}-Console-Output-Service.bin`,
        'Native-X64-Argument-Count-Service.bin',
        'Native-X64-Argument-Service.bin',
        `Native-X64-${platform}-File-Input-Service.bin`,
        'Native-X64-Utf8-Service.bin',
        `Native-X64-${platform}-Diagnostic-Output-Service.bin`,
        'Native-X64-Text-Concat-Service.bin',
        'Native-X64-U32-Format-Service.bin',
        `Native-X64-${platform}-File-Output-Service.bin`
    ]) {
        const relative = `Runtime/Windvale.Native/Consumers/${leaf}`;
        Addˉproducerˉfield(
            producerFields,
            `producer:${relative}`,
            await Readˉrepositoryˉproducer(relative)
        );
    }
    const linkerRelative =
        `Linker/Reference/Consumers/${platform}-X64-Hosted-Compiler.wvo`;
    Addˉproducerˉfield(
        producerFields,
        `producer:${linkerRelative}`,
        await Readˉrepositoryˉproducer(linkerRelative)
    );

    Addˉproducerˉfield(producerFields, 'producer:Native-Hosted-Application-Cache-Core',
        await Readˉboundedˉhostedˉfile(SCRIPT_PATH, 'hosted cache key implementation'));
    Addˉproducerˉfield(producerFields, 'runtime:node', Buffer.from(process.version, 'ascii'));

    return {
        hostFamily,
        packagerPath,
        producerFields,
        target
    };
}

export async function Getˉhostedˉapplicationˉcacheˉkey(
    context,
    request
) {
    const {
        namespace,
        profile,
        inputPath: inputArgument,
        chunkPrefix: prefixArgument,
        fragmentCountText,
        entry
    } = request;
    if (!/^[a-z0-9][a-z0-9-]{0,63}$/.test(namespace)) {
        Reject('The hosted-application cache-key namespace is invalid.');
    }
    if (!/^[1-8]$/.test(profile)) {
        Reject('The hosted-application cache-key profile is invalid.');
    }
    const inputPath = path.resolve(inputArgument);
    const chunkPrefix = path.resolve(prefixArgument);
    if (path.extname(inputPath).toLowerCase() !== '.wvb') {
        Reject('The hosted-application cache-key input must be a WVB.');
    }
    if (!/^(?:[1-9]|1[0-6])$/.test(fragmentCountText)) {
        Reject('The hosted-application cache-key fragment count is invalid.');
    }
    if (entry.length > 20 || !/^(0|[1-9][0-9]*)$/.test(entry) ||
        BigInt(entry) > 0xffff_ffff_ffff_ffffn) {
        Reject('The hosted-application cache-key entry is not canonical u64 decimal.');
    }

    const fragmentCount = Number(fragmentCountText);
    const hash = createHash('sha256');
    Addˉhostedˉkeyˉfield(
        hash,
        'format',
        Buffer.from('windvale-native-hosted-application-cache-key 2\n', 'ascii')
    );
    Addˉhostedˉkeyˉfield(hash, 'namespace', Buffer.from(namespace, 'ascii'));
    Addˉhostedˉkeyˉfield(hash, 'host', Buffer.from(context.hostFamily, 'ascii'));
    Addˉhostedˉkeyˉfield(hash, 'target', Buffer.from(context.target, 'ascii'));
    Addˉhostedˉkeyˉfield(hash, 'profile', Buffer.from(profile, 'ascii'));
    Addˉhostedˉkeyˉfield(
        hash,
        'fragment-count',
        Buffer.from(fragmentCountText, 'ascii')
    );
    Addˉhostedˉkeyˉfield(hash, 'entry', Buffer.from(entry, 'ascii'));
    Addˉhostedˉkeyˉfield(
        hash,
        'input-wvb',
        await Readˉboundedˉhostedˉfile(
            inputPath,
            'hosted input WVB',
            MAXIMUM_HOSTED_INPUT_BYTES,
            true
        )
    );
    for (let index = 0; index < fragmentCount; index += 1) {
        const chunkBytes = await Readˉboundedˉhostedˉfile(
            `${chunkPrefix}.chunk-${index}`,
            'hosted native-image fragment',
            MAXIMUM_HOSTED_FRAGMENT_BYTES,
            true
        );
        if (index + 1 < fragmentCount &&
            chunkBytes.length !== MAXIMUM_HOSTED_FRAGMENT_BYTES) {
            Reject(
                'Every nonfinal hosted native-image fragment must be exactly 4 MiB.'
            );
        }
        Addˉhostedˉkeyˉfield(hash, `native-fragment:${index}`, chunkBytes);
    }
    for (const field of context.producerFields) {
        Addˉhostedˉkeyˉfield(hash, field.label, field.bytes);
    }
    return hash.digest('hex');
}
