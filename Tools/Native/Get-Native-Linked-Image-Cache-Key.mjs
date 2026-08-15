import { createHash } from 'node:crypto';
import { lstat, readFile, realpath, stat } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const MAXIMUM_INPUT_BYTES = 67_108_864;
const SCRIPT_PATH = fileURLToPath(import.meta.url);
const REPOSITORY_ROOT = path.resolve(path.dirname(SCRIPT_PATH), '..', '..');
const WINDOWS = process.platform === 'win32';

function Fail(message) {
    process.stderr.write(`${message}\n`);
    process.exit(1);
}

function Isˉsameˉpath(left, right) {
    return WINDOWS ? left.toLowerCase() === right.toLowerCase() : left === right;
}

async function Readˉboundedˉordinaryˉfile(candidate, label, allowWindowsAlias = false) {
    let linkInformation;
    let information;
    try {
        linkInformation = await lstat(candidate);
        information = await stat(candidate);
    } catch {
        Fail(`Missing ${label}: ${candidate}`);
    }
    if (linkInformation.isSymbolicLink() || !information.isFile() ||
        information.size < 1 || information.size > MAXIMUM_INPUT_BYTES) {
        Fail(`The ${label} is not a bounded ordinary file: ${candidate}`);
    }
    const canonical = await realpath(candidate).catch(() => '');
    if (!Isˉsameˉpath(canonical, candidate) && !(WINDOWS && allowWindowsAlias)) {
        Fail(`The ${label} must use its canonical non-link path: ${candidate}`);
    }
    return readFile(WINDOWS && allowWindowsAlias ? canonical : candidate);
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

if (process.argv.length !== 8) {
    Fail('Usage: node Tools/Native/Get-Native-Linked-Image-Cache-Key.mjs <namespace> <base-address> <entry> <input.wvo> <link-front-door> <linker>');
}

const namespace = process.argv[2];
const baseAddress = process.argv[3];
const entry = process.argv[4];
const inputPath = path.resolve(process.argv[5]);
const frontDoorPath = path.resolve(process.argv[6]);
const linkerPath = path.resolve(process.argv[7]);

if (!/^[a-z0-9][a-z0-9-]{0,63}$/.test(namespace)) {
    Fail('The linked-image cache-key namespace is invalid.');
}
if (!/^(0|[1-9][0-9]{0,19})$/.test(baseAddress) ||
    BigInt(baseAddress) > 0xffff_ffff_ffff_ffffn) {
    Fail('The linked-image base address is not canonical u64 decimal.');
}
if (!/^[A-Za-z_][A-Za-z0-9_]{0,127}$/.test(entry)) {
    Fail('The linked-image entry symbol is invalid.');
}
if (path.extname(inputPath).toLowerCase() !== '.wvo') {
    Fail('The linked-image cache input must be a WVO.');
}

const expectedFrontDoor = path.join(
    REPOSITORY_ROOT, 'Tools', 'Native', `Link-Wvo.${WINDOWS ? 'cmd' : 'sh'}`);
const expectedLinker = path.join(
    REPOSITORY_ROOT, 'Artifacts', 'Native-Wv-Linker-Candidate',
    `Wv-Linker.${WINDOWS ? 'exe' : 'elf'}`);
if (!Isˉsameˉpath(frontDoorPath, expectedFrontDoor) ||
    !Isˉsameˉpath(linkerPath, expectedLinker)) {
    Fail('The linked-image producer paths are not the current-host front door and linker.');
}

const repositoryReal = await realpath(REPOSITORY_ROOT);
if (!Isˉsameˉpath(repositoryReal, REPOSITORY_ROOT)) {
    Fail('The repository root must use its canonical path.');
}

const hostFamily = WINDOWS ? 'windows-x64' : 'linux-x64';
const hash = createHash('sha256');
Addˉfield(hash, 'format', Buffer.from('windvale-native-linked-image-cache-key 1\n', 'ascii'));
Addˉfield(hash, 'namespace', Buffer.from(namespace, 'ascii'));
Addˉfield(hash, 'host', Buffer.from(hostFamily, 'ascii'));
Addˉfield(hash, 'base-address', Buffer.from(baseAddress, 'ascii'));
Addˉfield(hash, 'entry', Buffer.from(entry, 'ascii'));
Addˉfield(hash, 'input-wvo', await Readˉboundedˉordinaryˉfile(
    inputPath, 'linked-image input', true));
Addˉfield(hash, 'producer:Link-Wvo', await Readˉboundedˉordinaryˉfile(
    frontDoorPath, 'linked-image front door'));
Addˉfield(hash, 'producer:Wv-Linker', await Readˉboundedˉordinaryˉfile(
    linkerPath, 'linked-image linker'));

process.stdout.write(`${hash.digest('hex')}\n`);
