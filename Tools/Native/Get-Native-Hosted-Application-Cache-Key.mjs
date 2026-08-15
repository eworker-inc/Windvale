import { createHash } from 'node:crypto';
import { lstat, readFile, realpath, stat } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { TextDecoder } from 'node:util';

const MAXIMUM_INPUT_BYTES = 67_108_864;
const MAXIMUM_FRAGMENT_BYTES = 4_194_304;
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

async function Readˉboundedˉordinaryˉfile(
    candidate,
    label,
    maximumBytes = MAXIMUM_INPUT_BYTES,
    allowWindowsAlias = false
) {
    let linkInformation;
    let information;
    try {
        linkInformation = await lstat(candidate);
        information = await stat(candidate);
    } catch {
        Fail(`Missing ${label}: ${candidate}`);
    }
    if (linkInformation.isSymbolicLink() || !information.isFile() ||
        information.size < 1 || information.size > maximumBytes) {
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

function Repositoryˉpath(relative) {
    const candidate = path.resolve(REPOSITORY_ROOT, ...relative.split('/'));
    if (!Isˉwithinˉrepository(candidate)) {
        Fail(`The producer path escapes the repository: ${relative}`);
    }
    return candidate;
}

async function Addˉrepositoryˉfile(hash, relative, label = `producer:${relative}`) {
    Addˉfield(hash, label, await Readˉboundedˉordinaryˉfile(
        Repositoryˉpath(relative), 'hosted application producer'));
}

if (process.argv.length !== 10) {
    Fail('Usage: node Tools/Native/Get-Native-Hosted-Application-Cache-Key.mjs <namespace> <windows|linux> <profile> <input.wvb> <chunk-prefix> <fragment-count> <entry> <packager>');
}

const namespace = process.argv[2];
const target = process.argv[3];
const profile = process.argv[4];
const inputPath = path.resolve(process.argv[5]);
const chunkPrefix = path.resolve(process.argv[6]);
const fragmentCountText = process.argv[7];
const entry = process.argv[8];
const packagerPath = path.resolve(process.argv[9]);

if (!/^[a-z0-9][a-z0-9-]{0,63}$/.test(namespace)) {
    Fail('The hosted-application cache-key namespace is invalid.');
}
if (target !== 'windows' && target !== 'linux') {
    Fail('The hosted-application cache-key target is invalid.');
}
if (!/^[1-7]$/.test(profile)) {
    Fail('The hosted-application cache-key profile is invalid.');
}
if (path.extname(inputPath).toLowerCase() !== '.wvb') {
    Fail('The hosted-application cache-key input must be a WVB.');
}
if (!/^[1-8]$/.test(fragmentCountText)) {
    Fail('The hosted-application cache-key fragment count is invalid.');
}
if (entry.length > 20 || !/^(0|[1-9][0-9]*)$/.test(entry) ||
    BigInt(entry) > 0xffff_ffff_ffff_ffffn) {
    Fail('The hosted-application cache-key entry is not canonical u64 decimal.');
}

const hostFamily = WINDOWS ? 'windows-x64' : 'linux-x64';
const expectedPackager = path.join(
    REPOSITORY_ROOT, 'Tools', 'Native', `Package-Hosted-Wvb.${WINDOWS ? 'cmd' : 'sh'}`);
if (!Isˉsameˉpath(packagerPath, expectedPackager)) {
    Fail('The hosted-application cache-key packager is not the current-host front door.');
}

const repositoryReal = await realpath(REPOSITORY_ROOT);
if (!Isˉsameˉpath(repositoryReal, REPOSITORY_ROOT)) {
    Fail('The repository root must use its canonical path.');
}

const fragmentCount = Number(fragmentCountText);
const hash = createHash('sha256');
Addˉfield(hash, 'format', Buffer.from('windvale-native-hosted-application-cache-key 1\n', 'ascii'));
Addˉfield(hash, 'namespace', Buffer.from(namespace, 'ascii'));
Addˉfield(hash, 'host', Buffer.from(hostFamily, 'ascii'));
Addˉfield(hash, 'target', Buffer.from(target, 'ascii'));
Addˉfield(hash, 'profile', Buffer.from(profile, 'ascii'));
Addˉfield(hash, 'fragment-count', Buffer.from(fragmentCountText, 'ascii'));
Addˉfield(hash, 'entry', Buffer.from(entry, 'ascii'));
Addˉfield(hash, 'input-wvb', await Readˉboundedˉordinaryˉfile(
    inputPath, 'hosted input WVB', MAXIMUM_INPUT_BYTES, true));

for (let index = 0; index < fragmentCount; index += 1) {
    const chunkPath = `${chunkPrefix}.chunk-${index}`;
    const chunkBytes = await Readˉboundedˉordinaryˉfile(
        chunkPath, 'hosted native-image fragment', MAXIMUM_FRAGMENT_BYTES, true);
    if (index + 1 < fragmentCount && chunkBytes.length !== MAXIMUM_FRAGMENT_BYTES) {
        Fail('Every nonfinal hosted native-image fragment must be exactly 4 MiB.');
    }
    Addˉfield(hash, `native-fragment:${index}`, chunkBytes);
}

Addˉfield(hash, 'producer:Package-Hosted-Wvb',
    await Readˉboundedˉordinaryˉfile(packagerPath, 'hosted packager'));

const inventoryRelative = 'Artifacts/Native-Hosted-Container-Toolset-Candidate/SHA256SUMS';
const inventoryBytes = await Readˉboundedˉordinaryˉfile(
    Repositoryˉpath(inventoryRelative), 'hosted toolset inventory');
Addˉfield(hash, `producer:${inventoryRelative}`, inventoryBytes);
let inventoryText;
try {
    inventoryText = UTF8.decode(inventoryBytes);
} catch {
    Fail('The hosted toolset inventory is not valid UTF-8.');
}
const inventoryEntries = inventoryText.trimEnd().split(/\r?\n/u);
if (inventoryEntries.length !== 72) {
    Fail('The hosted toolset inventory does not contain exactly 72 entries.');
}
for (const line of inventoryEntries) {
    const match = /^([0-9a-f]{64})  ([A-Za-z0-9._/-]+)$/u.exec(line);
    if (match === null || match[2].split('/').some(part => part === '' || part === '.' || part === '..')) {
        Fail('The hosted toolset inventory contains a malformed entry.');
    }
    const relative = `Artifacts/Native-Hosted-Container-Toolset-Candidate/${match[2]}`;
    const bytes = await Readˉboundedˉordinaryˉfile(
        Repositoryˉpath(relative), 'hosted toolset artifact');
    const actual = createHash('sha256').update(bytes).digest('hex');
    if (actual !== match[1]) {
        Fail(`The hosted toolset artifact differs from its inventory: ${match[2]}`);
    }
    Addˉfield(hash, `producer:${relative}`, bytes);
}

for (const relative of [
    'Artifacts/Native-Hosted-Enum-Request-Candidate/Wvb/wvhostenumrequest.wvb',
    'Artifacts/Native-Hosted-Enum-Request-Candidate/windows-x64/wvhostenumrequest.exe',
    'Artifacts/Native-Hosted-Enum-Request-Candidate/linux-x64/wvhostenumrequest.elf'
]) {
    await Addˉrepositoryˉfile(hash, relative);
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
    await Addˉrepositoryˉfile(hash, `Runtime/Windvale.Native/Consumers/${leaf}`);
}
await Addˉrepositoryˉfile(
    hash, `Linker/Reference/Consumers/${platform}-X64-Hosted-Compiler.wvo`);

process.stdout.write(`${hash.digest('hex')}\n`);
