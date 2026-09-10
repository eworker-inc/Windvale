import { createHash } from 'node:crypto';
import { lstat, readFile, realpath, writeFile } from 'node:fs/promises';
import path from 'node:path';

const WINDOWS = process.platform === 'win32';
const HOST_FAMILY = WINDOWS ? 'windows-x64' : 'linux-x64';
const LINKAGE_FORMAT = 'windvale-current-source-wvb-publisher-linkage 1';
const HOST_IMPORTS_FORMAT = 'windvale-current-source-wvb-publisher-host-imports 1';
const MAXIMUM_RECORD_BYTES = 131_072;
const MAXIMUM_PLAN_BYTES = 4_096;
const RUNTIME_BYTES = 4_096;
const IMAGE_BASE_ADDRESS = 8_192;

const SYMBOL_KIND_FUNCTION = 'function';
const SYMBOL_KIND_DATA = 'data';

const TARGET_IDS = new Map([
    ['windows-x64', 1],
    ['linux-x64', 2],
]);

const TARGET_NAMES = new Map([...TARGET_IDS].map(([name, id]) => [id, name]));

const CONTAINER_PROFILE = new Map([
    [3, 1],
    [5, 2],
    [6, 3],
    [7, 4],
    [8, 5],
    [9, 6],
    [10, 7],
    [11, 8],
]);

const PROFILE_CONTAINER = new Map([...CONTAINER_PROFILE].map(([container, profile]) => [profile, container]));

const DATA_RUNTIME_OFFSETS = new Map([
    ['Argument_bytes', 5],
    ['Argument_table', 4],
    ['Data_arena', 11],
    ['Execution_context', 0],
    ['File_input_scratch', 12],
    ['File_input_table', 7],
    ['Name_arena', 10],
    ['Output_table', 6],
    ['Record_arena', 2],
    ['Service_table', 1],
    ['Snapshot_table', 9],
    ['Text_arena', 3],
]);

const SERVICE_INDICES = new Map([
    ['Service_console_write', 0],
    ['Service_process_argument_count', 1],
    ['Service_process_argument', 2],
    ['Service_file_read', 3],
    ['Service_utf8', 4],
    ['Service_diagnostic_write', 5],
]);

const WINDOWS_IAT_OFFSETS = new Map([
    ['Windows_close_handle_iat', 240],
    ['Windows_command_line_to_argv_iat', 384],
    ['Windows_create_file_iat', 248],
    ['Windows_flush_file_buffers_iat', 256],
    ['Windows_get_command_line_iat', 264],
    ['Windows_get_file_information_iat', 272],
    ['Windows_get_file_size_iat', 280],
    ['Windows_get_last_error_iat', 288],
    ['Windows_get_std_handle_iat', 296],
    ['Windows_local_free_iat', 304],
    ['Windows_multi_byte_to_wide_char_iat', 312],
    ['Windows_nt_set_file_information_iat', 368],
    ['Windows_read_file_iat', 320],
    ['Windows_set_file_information_iat', 328],
    ['Windows_set_file_pointer_iat', 336],
    ['Windows_wide_char_to_multi_byte_iat', 344],
    ['Windows_write_file_iat', 352],
]);

function Reject(message, exitCode = 1) {
    const error = new Error(message);
    error.exitCode = exitCode;
    throw error;
}

function SamePath(left, right) {
    return WINDOWS ? left.toLowerCase() === right.toLowerCase() : left === right;
}

function Sha256(bytes) {
    return createHash('sha256').update(bytes).digest('hex');
}

function IsHexSha256(value) {
    return /^[0-9a-f]{64}$/.test(value);
}

function ParseUnsignedDecimal(value, label, maximum = Number.MAX_SAFE_INTEGER) {
    if (!/^(0|[1-9][0-9]*)$/.test(value)) {
        Reject(`The ${label} is not canonical decimal.`);
    }
    const parsed = Number(value);
    if (!Number.isSafeInteger(parsed) || parsed > maximum) {
        Reject(`The ${label} exceeds its supported bound.`);
    }
    return parsed;
}

async function RequireCanonicalDirectory(candidate, label) {
    const absolute = path.resolve(candidate);
    const information = await lstat(absolute).catch(() => null);
    if (information === null || !information.isDirectory() ||
        information.isSymbolicLink()) {
        Reject(`The ${label} is not an ordinary directory: ${absolute}`, 64);
    }
    const canonical = await realpath(absolute);
    if (!SamePath(canonical, absolute)) {
        Reject(`The ${label} must use its canonical non-link path: ${absolute}`, 64);
    }
    return absolute;
}

async function ReadOrdinaryFile(candidate, label, maximumBytes) {
    const absolute = path.resolve(candidate);
    const information = await lstat(absolute).catch(() => null);
    if (information === null || !information.isFile() ||
        information.isSymbolicLink() || information.size < 1 ||
        information.size > maximumBytes) {
        Reject(`The ${label} is not a bounded ordinary file: ${absolute}`, 64);
    }
    const canonical = await realpath(absolute);
    if (!SamePath(canonical, absolute)) {
        Reject(`The ${label} must use its canonical non-link path: ${absolute}`, 64);
    }
    const bytes = await readFile(absolute);
    if (bytes.length !== information.size) {
        Reject(`The ${label} changed while it was read.`);
    }
    return { absolute, bytes, size: information.size, sha256: Sha256(bytes) };
}

async function RequireOutputPath(candidate, inputPaths) {
    const absolute = path.resolve(candidate);
    await RequireCanonicalDirectory(path.dirname(absolute), 'current publisher host-import output parent');
    for (const input of inputPaths) {
        if (SamePath(absolute, input)) {
            Reject('The current publisher host-import output must be distinct from every input.', 64);
        }
    }
    const information = await lstat(absolute).catch(error => {
        if (error.code === 'ENOENT') return null;
        throw error;
    });
    if (information !== null) {
        Reject('The current publisher host-import output already exists.', 64);
    }
    return absolute;
}

function RequireAsciiLf(bytes, label) {
    if (bytes.length < 1 || bytes[bytes.length - 1] !== 0x0a) {
        Reject(`The ${label} record must end with LF.`);
    }
    for (let index = 0; index < bytes.length; index += 1) {
        const value = bytes[index];
        if (value === 0x0a) continue;
        if (value < 0x20 || value > 0x7e) {
            Reject(`The ${label} record is not ASCII/LF.`);
        }
    }
}

function SetOnce(fields, key, value, label) {
    if (fields.has(key)) {
        Reject(`The ${label} duplicates field ${key}.`);
    }
    fields.set(key, value);
}

function ParseLinkageRecord(file) {
    RequireAsciiLf(file.bytes, 'current publisher linkage');
    const text = file.bytes.toString('ascii');
    const lines = text.split('\n');
    if (lines[lines.length - 1] !== '' || lines.length < 4 ||
        lines[0] !== LINKAGE_FORMAT) {
        Reject('The current publisher linkage record header differs.');
    }
    const fields = new Map();
    const targets = new Map();
    for (let index = 1; index < lines.length - 1; index += 1) {
        const line = lines[index];
        if (line.length === 0) {
            Reject('The current publisher linkage record contains an interior blank line.');
        }
        const deferred = /^target (windows-x64|linux-x64) deferred-import ([A-Za-z0-9_.-]+) kind (function|data)$/.exec(line);
        if (deferred !== null) {
            const target = Target(targets, deferred[1]);
            target.deferredImports.push({ name: deferred[2], kind: deferred[3] });
            continue;
        }
        const targetField = /^target (windows-x64|linux-x64) ([A-Za-z0-9-]+) (.+)$/.exec(line);
        if (targetField !== null) {
            SetOnce(Target(targets, targetField[1]).fields, targetField[2], targetField[3], `target ${targetField[1]} linkage`);
            continue;
        }
        const nativeObject = /^native-object-([1-6]) ([A-Za-z0-9_.-]+) bytes (0|[1-9][0-9]*) sha256 ([0-9a-f]{64})$/.exec(line);
        if (nativeObject !== null) {
            ParseUnsignedDecimal(nativeObject[3], `current publisher linkage native-object-${nativeObject[1]} bytes`, MAXIMUM_RECORD_BYTES);
            continue;
        }
        const field = /^([A-Za-z0-9-]+) ([A-Za-z0-9_.-]+)$/.exec(line);
        if (field === null) {
            Reject(`The current publisher linkage record field is malformed: ${line}`);
        }
        SetOnce(fields, field[1], field[2], 'current publisher linkage record');
    }
    const linkageSha256 = fields.get('linkage-sha256');
    if (linkageSha256 === undefined || !IsHexSha256(linkageSha256)) {
        Reject('The current publisher linkage record self hash is invalid.');
    }
    const unsignedLines = [lines[0], lines[1], ...lines.slice(3)];
    const unsignedBytes = Buffer.from(unsignedLines.join('\n'), 'ascii');
    if (Sha256(unsignedBytes) !== linkageSha256) {
        Reject('The current publisher linkage record self hash differs.');
    }
    return { fields, targets, sha256: linkageSha256 };
}

function Target(targets, name) {
    let target = targets.get(name);
    if (target === undefined) {
        target = { fields: new Map(), deferredImports: [] };
        targets.set(name, target);
    }
    return target;
}

function RequiredField(fields, key, label) {
    const value = fields.get(key);
    if (value === undefined) {
        Reject(`The ${label} is missing ${key}.`);
    }
    return value;
}

function RequiredSha256(fields, key, label) {
    const value = RequiredField(fields, key, label);
    if (!IsHexSha256(value)) {
        Reject(`The ${label} ${key} is not a lowercase SHA-256.`);
    }
    return value;
}

function RequiredNumber(fields, key, label, maximum = Number.MAX_SAFE_INTEGER) {
    return ParseUnsignedDecimal(RequiredField(fields, key, label), `${label} ${key}`, maximum);
}

function ReadU32(bytes, offset, label) {
    if (offset < 0 || offset + 4 > bytes.length) {
        Reject(`The ${label} u32 field exceeds the file.`);
    }
    return bytes.readUInt32LE(offset);
}

function RuntimeOffset(index, target, profile) {
    if (index === 0) return 0;
    if (index === 1) return 112;
    if (index === 2) return 73_728;
    if (index === 3) return 2_170_880;
    if (index === 4) return 4_096;
    if (index === 5) return 5_168;
    if (index === 6) return 216;
    if (index === 7) return 264;
    if (index === 8) return 400;
    if (index === 9) return 70_704;
    if (profile === 2 || profile === 6) {
        if (index === 10) return 237_051_904;
        if (index === 11) return 237_576_192;
        if (index === 12) return 506_011_648;
        return target === 1 ? 508_112_896 : 507_064_320;
    }
    if (profile === 7) {
        if (index === 10) return 303_636_480;
        if (index === 11) return 304_160_768;
        if (index === 12) return 572_596_224;
        return target === 1 ? 574_697_472 : 573_648_896;
    }
    if (profile === 8) {
        if (index === 10) return 438_116_352;
        if (index === 11) return 438_378_496;
        if (index === 12) return 572_596_224;
        return target === 1 ? 574_697_472 : 573_648_896;
    }
    if (index === 10) return 136_388_608;
    if (index === 11) return 203_497_472;
    if (index === 12) return 471_932_928;
    return target === 1 ? 474_034_176 : 472_985_600;
}

function ParsePlan(file) {
    const bytes = file.bytes;
    if (bytes.length < 128 ||
        ReadU32(bytes, 0, 'hosted-container plan') !== 1_145_263_703 ||
        ReadU32(bytes, 4, 'hosted-container plan') !== 1 ||
        ReadU32(bytes, 8, 'hosted-container plan') !== bytes.length ||
        ReadU32(bytes, 12, 'hosted-container plan') !== 0) {
        Reject('The hosted-container plan header differs.');
    }
    const target = ReadU32(bytes, 20, 'hosted-container plan');
    const profile = ReadU32(bytes, 24, 'hosted-container plan');
    const targetPayload = ReadU32(bytes, 96, 'hosted-container plan');
    const targetBytes = ReadU32(bytes, 100, 'hosted-container plan');
    if (!TARGET_NAMES.has(target) || profile < 1 || profile > 8 ||
        targetPayload !== 128 || 128 + targetBytes !== bytes.length ||
        (target === 1 && targetBytes !== 240) ||
        (target === 2 && targetBytes !== 128)) {
        Reject('The hosted-container plan target payload differs.');
    }
    return {
        target,
        targetName: TARGET_NAMES.get(target),
        profile,
        applicationBytes: ReadU32(bytes, 28, 'hosted-container plan'),
        headerBytes: ReadU32(bytes, 36, 'hosted-container plan'),
        textAddress: ReadU32(bytes, 80, 'hosted-container plan'),
        runtimeAddress: ReadU32(bytes, 88, 'hosted-container plan'),
        imageVirtualBytes: ReadU32(bytes, 92, 'hosted-container plan'),
        relocationAddress: ReadU32(bytes, 104, 'hosted-container plan'),
        importAddress: ReadU32(bytes, 108, 'hosted-container plan'),
        targetBytes,
    };
}

function ParseRuntime(file, plan, linkage, target) {
    const bytes = file.bytes;
    if (bytes.length !== RUNTIME_BYTES) {
        Reject('The hosted-container runtime header byte length differs.');
    }
    const metadataOffset = 480;
    const targetId = ReadU32(bytes, metadataOffset + 12, 'hosted-container runtime metadata');
    const container = ReadU32(bytes, metadataOffset + 16, 'hosted-container runtime metadata');
    const profile = CONTAINER_PROFILE.get(container) ?? 0;
    const expectedContainer = PROFILE_CONTAINER.get(plan.profile);
    if (targetId !== plan.target || profile !== plan.profile ||
        container !== expectedContainer) {
        Reject('The hosted-container runtime metadata target/profile differs from the plan.');
    }
    const serviceCount = ReadU32(bytes, metadataOffset + 36, 'hosted-container runtime metadata');
    const bundleOffset = ReadU32(bytes, metadataOffset + 56, 'hosted-container runtime metadata');
    const bundleBytes = ReadU32(bytes, metadataOffset + 60, 'hosted-container runtime metadata');
    const nativeBytes = ReadU32(bytes, metadataOffset + 68, 'hosted-container runtime metadata');
    const nativeEntry = ReadU32(bytes, metadataOffset + 72, 'hosted-container runtime metadata');
    const imageBytes = RequiredNumber(linkage.fields, 'image-bytes', 'current publisher linkage', 67_108_864);
    const imageEntry = RequiredNumber(linkage.fields, 'image-entry-offset', 'current publisher linkage', 67_108_864);
    const linkedNativeMain = RequiredNumber(target.fields, 'native-main-address', `target ${plan.targetName} linkage`);
    const endAddress = RequiredNumber(target.fields, 'end-address', `target ${plan.targetName} linkage`);
    const linkedNativeBytes = endAddress - IMAGE_BASE_ADDRESS;
    if (serviceCount !== 10 || bundleOffset !== 4_096 ||
        (nativeBytes !== imageBytes && nativeBytes !== linkedNativeBytes) ||
        nativeEntry !== imageEntry ||
        IMAGE_BASE_ADDRESS + nativeEntry !== linkedNativeMain ||
        nativeEntry >= nativeBytes || nativeBytes > bundleBytes) {
        Reject('The hosted-container runtime metadata does not match the current publisher image.');
    }
    return {
        metadataOffset,
        serviceCount,
        bundleBytes,
        imageBytes,
        nativeBytes,
        nativeEntry,
        nativeMainAddress: IMAGE_BASE_ADDRESS + nativeEntry,
    };
}

function ServiceAddress(runtime, metadata, index) {
    const record = metadata.metadataOffset + 224 + index * 64;
    const identity = ReadU32(runtime.bytes, record, 'hosted-container runtime service');
    const imageOffset = ReadU32(runtime.bytes, record + 16, 'hosted-container runtime service');
    const codeBytes = ReadU32(runtime.bytes, record + 20, 'hosted-container runtime service');
    if (identity !== index + 1 || codeBytes === 0 || imageOffset >= metadata.bundleBytes) {
        Reject(`The hosted-container runtime service ${index} metadata differs.`);
    }
    return IMAGE_BASE_ADDRESS + imageOffset;
}

function BoundAddressFor(symbol, plan, runtime, metadata) {
    const runtimeIndex = DATA_RUNTIME_OFFSETS.get(symbol.name);
    if (runtimeIndex !== undefined) {
        return plan.runtimeAddress + RuntimeOffset(runtimeIndex, plan.target, plan.profile);
    }
    const serviceIndex = SERVICE_INDICES.get(symbol.name);
    if (serviceIndex !== undefined) {
        return ServiceAddress(runtime, metadata, serviceIndex);
    }
    if (plan.target === 1) {
        const iatOffset = WINDOWS_IAT_OFFSETS.get(symbol.name);
        if (iatOffset !== undefined) {
            return plan.importAddress + iatOffset;
        }
    }
    return null;
}

function ExpectedKind(name) {
    if (DATA_RUNTIME_OFFSETS.has(name) || WINDOWS_IAT_OFFSETS.has(name)) {
        return SYMBOL_KIND_DATA;
    }
    if (SERVICE_INDICES.has(name)) {
        return SYMBOL_KIND_FUNCTION;
    }
    return null;
}

function HostImportLines(values, hostImportsSha256 = null) {
    const lines = [
        HOST_IMPORTS_FORMAT,
        `host ${HOST_FAMILY}`,
    ];
    if (hostImportsSha256 !== null) {
        lines.push(`host-imports-sha256 ${hostImportsSha256}`);
    }
    lines.push(
        `linkage-sha256 ${values.linkage.sha256}`,
        `binding-sha256 ${RequiredSha256(values.linkage.fields, 'binding-sha256', 'current publisher linkage')}`,
        `target ${values.plan.targetName}`,
        `target-id ${values.plan.target}`,
        `profile ${values.plan.profile}`,
        `runtime-bytes ${values.runtime.size}`,
        `runtime-sha256 ${values.runtime.sha256}`,
        `plan-bytes ${values.planFile.size}`,
        `plan-sha256 ${values.planFile.sha256}`,
        `application-bytes ${values.plan.applicationBytes}`,
        `image-virtual-bytes ${values.plan.imageVirtualBytes}`,
        `current-image-address ${IMAGE_BASE_ADDRESS}`,
        `current-image-bytes ${values.metadata.imageBytes}`,
        `native-image-bytes ${values.metadata.nativeBytes}`,
        `native-entry-offset ${values.metadata.nativeEntry}`,
        `native-main-address ${values.metadata.nativeMainAddress}`,
        `runtime-address ${values.plan.runtimeAddress}`,
        `import-address ${values.plan.importAddress}`,
        `relocation-address ${values.plan.relocationAddress}`,
        `imports-deferred ${values.deferredCount}`,
        `imports-bound ${values.boundImports.length}`,
    );
    if (values.plan.target === 1) {
        lines.push(
            `windows-publisher-import-directory-address ${values.plan.importAddress}`,
            `windows-publisher-iat-directory-address ${values.plan.importAddress + 240}`,
            'windows-publisher-import-page-bytes 4096',
        );
    }
    for (const bound of values.boundImports) {
        lines.push(`bound-import ${bound.name} kind ${bound.kind} address ${bound.address}`);
    }
    lines.push('');
    return lines;
}

async function Main() {
    if (process.argv.length !== 7) {
        Reject(
            'Usage: node Tools/Native/Bind-Current-Publisher-Host-Imports.mjs ' +
            '<current-publisher-linkage.wvcl> <windows-x64|linux-x64> ' +
            '<runtime.wvhr> <plan.wvcd> <output.wvci>',
            64,
        );
    }
    const requestedTarget = process.argv[3];
    if (!TARGET_IDS.has(requestedTarget)) {
        Reject('The current publisher host-import target is unsupported.', 64);
    }
    const linkageFile = await ReadOrdinaryFile(
        process.argv[2],
        'current publisher linkage record',
        MAXIMUM_RECORD_BYTES,
    );
    const runtimeFile = await ReadOrdinaryFile(
        process.argv[4],
        'hosted-container runtime header',
        RUNTIME_BYTES,
    );
    const planFile = await ReadOrdinaryFile(
        process.argv[5],
        'hosted-container plan',
        MAXIMUM_PLAN_BYTES,
    );
    const output = await RequireOutputPath(
        process.argv[6],
        [linkageFile.absolute, runtimeFile.absolute, planFile.absolute],
    );

    const linkage = ParseLinkageRecord(linkageFile);
    const target = linkage.targets.get(requestedTarget);
    if (target === undefined) {
        Reject(`The current publisher linkage record is missing target ${requestedTarget}.`);
    }
    const plan = ParsePlan(planFile);
    if (plan.targetName !== requestedTarget) {
        Reject('The hosted-container plan target differs from the requested target.');
    }
    if (RequiredNumber(target.fields, 'current-image-address', `target ${requestedTarget} linkage`) !== IMAGE_BASE_ADDRESS) {
        Reject('The current publisher image base address differs.');
    }
    const metadata = ParseRuntime(runtimeFile, plan, linkage, target);
    const deferredCount = RequiredNumber(target.fields, 'imports-deferred', `target ${requestedTarget} linkage`);
    if (deferredCount !== target.deferredImports.length) {
        Reject('The current publisher linkage deferred import count differs.');
    }
    const boundImports = [];
    for (const deferred of target.deferredImports) {
        const expectedKind = ExpectedKind(deferred.name);
        if (expectedKind === null || expectedKind !== deferred.kind) {
            Reject(`The deferred import ${deferred.name} has no host binding contract.`);
        }
        const address = BoundAddressFor(deferred, plan, runtimeFile, metadata);
        if (address === null || address < 1 || address > 0xffff_ffff) {
            Reject(`The deferred import ${deferred.name} did not bind to a valid address.`);
        }
        boundImports.push({ ...deferred, address });
    }
    const values = {
        linkage,
        runtime: runtimeFile,
        planFile,
        plan,
        metadata,
        deferredCount,
        boundImports,
    };
    const unsignedRecord = Buffer.from(HostImportLines(values).join('\n'), 'ascii');
    const hostImportsSha256 = Sha256(unsignedRecord);
    const record = Buffer.from(HostImportLines(values, hostImportsSha256).join('\n'), 'ascii');
    await writeFile(output, record, { flag: 'wx' });
    process.stdout.write(
        `current publisher host imports status=Valid format=1 target=${requestedTarget} bound=${boundImports.length}\n`,
    );
}

try {
    await Main();
} catch (error) {
    process.stderr.write(`${error.message}\n`);
    process.exitCode = error.exitCode ?? 1;
}
