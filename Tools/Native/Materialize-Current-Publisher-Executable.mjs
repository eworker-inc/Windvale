import { createHash } from 'node:crypto';
import { spawn, spawnSync } from 'node:child_process';
import {
    copyFile,
    lstat,
    mkdtemp,
    readFile,
    realpath,
    rm,
    writeFile,
} from 'node:fs/promises';
import { tmpdir } from 'node:os';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const WINDOWS = process.platform === 'win32';
const REPOSITORY = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..', '..');
const LINKAGE_FORMAT = 'windvale-current-source-wvb-publisher-linkage 1';
const HOST_IMPORTS_FORMAT = 'windvale-current-source-wvb-publisher-host-imports 1';
const MAXIMUM_RECORD_BYTES = 131_072;
const MAXIMUM_MANIFEST_BYTES = 6_244;
const MAXIMUM_CHUNK_BYTES = 4_194_304;
const MAXIMUM_IMAGE_BYTES = 67_108_864;
const MAXIMUM_CURRENT_OBJECT_BYTES = 65_536;
const IMAGE_BASE_ADDRESS = 8_192;
const STARTUP_BASE_ADDRESS = 4_096;
const IMPORT_SECTION_SENTINEL = 0xffff_ffff;
const PACKAGE_PROFILE = '2';
const MATERIALIZATION_DEADLINE_MS = 300_000;
const WINDOWS_IMPORT_PAGE_BYTES = 4_096;
const WINDOWS_PUBLISHER_IMPORT_DIRECTORY_BYTES = 80;
const WINDOWS_PUBLISHER_IAT_DIRECTORY_OFFSET = 240;
const WINDOWS_PUBLISHER_IAT_DIRECTORY_BYTES = 160;

const SECTION_KIND_CODE = 1;
const SECTION_KIND_READ_ONLY = 2;
const SYMBOL_BINDING_LOCAL = 1;
const SYMBOL_BINDING_EXPORT = 2;
const SYMBOL_BINDING_IMPORT = 3;
const SYMBOL_KIND_FUNCTION = 1;
const SYMBOL_KIND_DATA = 2;
const RELOCATION_KIND_RELATIVE_I32 = 2;

const NATIVE_OBJECTS = Object.freeze([
    { role: 1, leaf: 'Windows-X64-Wvb-Publisher.wvo' },
    { role: 2, leaf: 'Linux-X64-Wvb-Publisher.wvo' },
    { role: 3, leaf: 'Windows-X64-Wvb-Publication-Adapter.wvo' },
    { role: 4, leaf: 'Linux-X64-Wvb-Publication-Adapter.wvo' },
    { role: 5, leaf: 'X64-Wvb-Publication-Sha256.wvo' },
    { role: 6, leaf: 'X64-Publication-Transaction-State.wvo' },
]);

function FunctionSymbol(name) {
    return { name, kind: SYMBOL_KIND_FUNCTION };
}

function DataSymbol(name) {
    return { name, kind: SYMBOL_KIND_DATA };
}

const COMMON_ADAPTER_IMPORTS = Object.freeze([
    DataSymbol('Argument_bytes'),
    DataSymbol('Argument_table'),
    DataSymbol('Data_arena'),
    DataSymbol('Execution_context'),
    DataSymbol('File_input_scratch'),
    DataSymbol('File_input_table'),
    DataSymbol('Name_arena'),
    FunctionSymbol('Native_main'),
    FunctionSymbol('Native_publication_apply'),
    FunctionSymbol('Native_publication_begin'),
    DataSymbol('Output_table'),
    DataSymbol('Record_arena'),
    FunctionSymbol('Service_console_write'),
    FunctionSymbol('Service_diagnostic_write'),
    FunctionSymbol('Service_file_read'),
    FunctionSymbol('Service_process_argument'),
    FunctionSymbol('Service_process_argument_count'),
    DataSymbol('Service_table'),
    FunctionSymbol('Service_utf8'),
    DataSymbol('Snapshot_table'),
    DataSymbol('Text_arena'),
    DataSymbol('X64_wvb_publication_report_newline'),
    DataSymbol('X64_wvb_publication_report_prefix'),
    DataSymbol('X64_wvb_publication_report_separator'),
    FunctionSymbol('X64_wvb_publication_sha256_hex'),
    FunctionSymbol('X64_wvb_publication_u32_hex8'),
]);

const WINDOWS_IAT_IMPORTS = Object.freeze([
    DataSymbol('Windows_close_handle_iat'),
    DataSymbol('Windows_command_line_to_argv_iat'),
    DataSymbol('Windows_create_file_iat'),
    DataSymbol('Windows_flush_file_buffers_iat'),
    DataSymbol('Windows_get_command_line_iat'),
    DataSymbol('Windows_get_file_information_iat'),
    DataSymbol('Windows_get_file_size_iat'),
    DataSymbol('Windows_get_last_error_iat'),
    DataSymbol('Windows_get_std_handle_iat'),
    DataSymbol('Windows_local_free_iat'),
    DataSymbol('Windows_multi_byte_to_wide_char_iat'),
    DataSymbol('Windows_nt_set_file_information_iat'),
    DataSymbol('Windows_read_file_iat'),
    DataSymbol('Windows_set_file_information_iat'),
    DataSymbol('Windows_set_file_pointer_iat'),
    DataSymbol('Windows_wide_char_to_multi_byte_iat'),
    DataSymbol('Windows_write_file_iat'),
]);

const ROLE_SPECS = new Map([
    [1, {
        exports: Object.freeze([FunctionSymbol('Windows_wvb_publisher_startup')]),
        imports: Object.freeze([FunctionSymbol('Windows_wvb_publisher_run')]),
        sections: Object.freeze(['.text']),
    }],
    [2, {
        exports: Object.freeze([FunctionSymbol('Linux_wvb_publisher_startup')]),
        imports: Object.freeze([FunctionSymbol('Linux_wvb_publisher_run')]),
        sections: Object.freeze(['.text']),
    }],
    [3, {
        exports: Object.freeze([FunctionSymbol('Windows_wvb_publisher_run')]),
        imports: Object.freeze([...COMMON_ADAPTER_IMPORTS, ...WINDOWS_IAT_IMPORTS]),
        sections: Object.freeze(['.text']),
    }],
    [4, {
        exports: Object.freeze([FunctionSymbol('Linux_wvb_publisher_run')]),
        imports: COMMON_ADAPTER_IMPORTS,
        sections: Object.freeze(['.text']),
    }],
    [5, {
        exports: Object.freeze([
            DataSymbol('X64_wvb_publication_report_newline'),
            DataSymbol('X64_wvb_publication_report_prefix'),
            DataSymbol('X64_wvb_publication_report_separator'),
            FunctionSymbol('X64_wvb_publication_sha256_hex'),
            FunctionSymbol('X64_wvb_publication_u32_hex8'),
        ]),
        imports: Object.freeze([]),
        sections: Object.freeze(['.text', '.rodata']),
    }],
    [6, {
        exports: Object.freeze([
            FunctionSymbol('Native_publication_apply'),
            FunctionSymbol('Native_publication_begin'),
        ]),
        imports: Object.freeze([]),
        sections: Object.freeze(['.text']),
    }],
]);

const TARGETS = new Map([
    ['windows-x64', { name: 'windows-x64', packageTarget: 'windows', startupRole: 1, adapterRole: 3, outputExtension: '.exe' }],
    ['linux-x64', { name: 'linux-x64', packageTarget: 'linux', startupRole: 2, adapterRole: 4, outputExtension: '.elf' }],
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

async function RequireOutputPath(candidate, inputPaths, label, extension = null) {
    const absolute = path.resolve(candidate);
    if (extension !== null && path.extname(absolute).toLowerCase() !== extension) {
        Reject(`The ${label} extension does not match the target.`, 64);
    }
    await RequireCanonicalDirectory(path.dirname(absolute), `${label} parent`);
    for (const input of inputPaths) {
        if (SamePath(absolute, input)) {
            Reject(`The ${label} must be distinct from every input.`, 64);
        }
    }
    const information = await lstat(absolute).catch(error => {
        if (error.code === 'ENOENT') return null;
        throw error;
    });
    if (information !== null) {
        Reject(`The ${label} already exists.`, 64);
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

function TargetRecord(targets, name) {
    let target = targets.get(name);
    if (target === undefined) {
        target = { fields: new Map(), deferredImports: [] };
        targets.set(name, target);
    }
    return target;
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
    const nativeObjects = new Map();
    for (let index = 1; index < lines.length - 1; index += 1) {
        const line = lines[index];
        if (line.length === 0) {
            Reject('The current publisher linkage record contains an interior blank line.');
        }
        const deferred = /^target (windows-x64|linux-x64) deferred-import ([A-Za-z0-9_.-]+) kind (function|data)$/.exec(line);
        if (deferred !== null) {
            TargetRecord(targets, deferred[1]).deferredImports.push({ name: deferred[2], kind: deferred[3] });
            continue;
        }
        const targetField = /^target (windows-x64|linux-x64) ([A-Za-z0-9-]+) (.+)$/.exec(line);
        if (targetField !== null) {
            SetOnce(TargetRecord(targets, targetField[1]).fields, targetField[2], targetField[3], `target ${targetField[1]} linkage`);
            continue;
        }
        const nativeObject = /^native-object-([1-6]) ([A-Za-z0-9_.-]+) bytes (0|[1-9][0-9]*) sha256 ([0-9a-f]{64})$/.exec(line);
        if (nativeObject !== null) {
            const role = Number(nativeObject[1]);
            if (nativeObjects.has(role)) {
                Reject(`The current publisher linkage duplicates native object ${role}.`);
            }
            nativeObjects.set(role, {
                role,
                leaf: nativeObject[2],
                bytes: ParseUnsignedDecimal(nativeObject[3], `current publisher native-object-${role} bytes`, MAXIMUM_CURRENT_OBJECT_BYTES),
                sha256: nativeObject[4],
            });
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
    if (Sha256(Buffer.from(unsignedLines.join('\n'), 'ascii')) !== linkageSha256) {
        Reject('The current publisher linkage record self hash differs.');
    }
    return { fields, targets, nativeObjects, sha256: linkageSha256 };
}

function ParseHostImportsRecord(file) {
    RequireAsciiLf(file.bytes, 'current publisher host-import');
    const text = file.bytes.toString('ascii');
    const lines = text.split('\n');
    if (lines[lines.length - 1] !== '' || lines.length < 4 ||
        lines[0] !== HOST_IMPORTS_FORMAT) {
        Reject('The current publisher host-import record header differs.');
    }
    const fields = new Map();
    const boundImports = new Map();
    for (let index = 1; index < lines.length - 1; index += 1) {
        const line = lines[index];
        const bound = /^bound-import ([A-Za-z0-9_.-]+) kind (function|data) address (0|[1-9][0-9]*)$/.exec(line);
        if (bound !== null) {
            if (boundImports.has(bound[1])) {
                Reject(`The current publisher host-import record duplicates ${bound[1]}.`);
            }
            boundImports.set(bound[1], {
                name: bound[1],
                kind: bound[2],
                address: ParseUnsignedDecimal(bound[3], `bound import ${bound[1]} address`, 0xffff_ffff),
            });
            continue;
        }
        const field = /^([A-Za-z0-9-]+) ([A-Za-z0-9_.-]+)$/.exec(line);
        if (field === null) {
            Reject(`The current publisher host-import record field is malformed: ${line}`);
        }
        SetOnce(fields, field[1], field[2], 'current publisher host-import record');
    }
    const hostImportsSha256 = fields.get('host-imports-sha256');
    if (hostImportsSha256 === undefined || !IsHexSha256(hostImportsSha256)) {
        Reject('The current publisher host-import record self hash is invalid.');
    }
    const unsignedLines = [lines[0], lines[1], ...lines.slice(3)];
    if (Sha256(Buffer.from(unsignedLines.join('\n'), 'ascii')) !== hostImportsSha256) {
        Reject('The current publisher host-import record self hash differs.');
    }
    return { fields, boundImports, sha256: hostImportsSha256 };
}

function RequiredField(fields, key, label) {
    const value = fields.get(key);
    if (value === undefined) {
        Reject(`The ${label} is missing ${key}.`);
    }
    return value;
}

function RequiredNumber(fields, key, label, maximum = Number.MAX_SAFE_INTEGER) {
    return ParseUnsignedDecimal(RequiredField(fields, key, label), `${label} ${key}`, maximum);
}

function RequiredSha256(fields, key, label) {
    const value = RequiredField(fields, key, label);
    if (!IsHexSha256(value)) {
        Reject(`The ${label} ${key} is not a lowercase SHA-256.`);
    }
    return value;
}

function RequireAvailable(bytes, offset, length, label) {
    if (offset < 0 || length < 0 || offset > bytes.length ||
        length > bytes.length - offset) {
        Reject(`The ${label} record exceeds the object.`);
    }
}

function ReadMachineName(bytes, offset, length, label) {
    RequireAvailable(bytes, offset, length, label);
    if (length < 1 || length > 255) {
        Reject(`The ${label} name length differs.`);
    }
    const name = bytes.subarray(offset, offset + length).toString('ascii');
    if (!/^[A-Za-z_.][A-Za-z0-9_.-]*$/.test(name) ||
        !Buffer.from(name, 'ascii').equals(bytes.subarray(offset, offset + length))) {
        Reject(`The ${label} name is not a bounded ASCII machine name.`);
    }
    return name;
}

function IsPowerOfTwo(value) {
    return value >= 1 && value <= 4096 && (value & (value - 1)) === 0;
}

function ParseWvo(bytes, label) {
    if (bytes.length < 24 || bytes.length > MAXIMUM_CURRENT_OBJECT_BYTES ||
        bytes.subarray(0, 4).toString('ascii') !== 'WVO1' ||
        bytes.readUInt16LE(4) !== 1 ||
        bytes.readUInt16LE(6) !== 0 ||
        bytes[8] !== 1 ||
        bytes[9] !== 0 ||
        bytes.readUInt16LE(10) !== 0) {
        Reject(`The ${label} WVO header differs.`);
    }
    const sectionCount = bytes.readUInt32LE(12);
    const symbolCount = bytes.readUInt32LE(16);
    const relocationCount = bytes.readUInt32LE(20);
    if (sectionCount < 1 || sectionCount > 64 ||
        symbolCount > 64 || relocationCount > 256) {
        Reject(`The ${label} WVO counts exceed the current publisher bounds.`);
    }
    let cursor = 24;
    const sections = [];
    const sectionNames = new Set();
    for (let index = 0; index < sectionCount; index += 1) {
        RequireAvailable(bytes, cursor, 20, `${label} section ${index}`);
        const kind = bytes[cursor];
        const reserved = bytes[cursor + 1] | bytes.readUInt16LE(cursor + 2);
        const alignment = bytes.readUInt32LE(cursor + 4);
        const memoryBytes = bytes.readUInt32LE(cursor + 8);
        const fileBytes = bytes.readUInt32LE(cursor + 12);
        const nameBytes = bytes.readUInt32LE(cursor + 16);
        if (reserved !== 0 || kind < 1 || kind > 4 || !IsPowerOfTwo(alignment) ||
            fileBytes > memoryBytes) {
            Reject(`The ${label} section ${index} record differs.`);
        }
        const name = ReadMachineName(bytes, cursor + 20, nameBytes, `${label} section ${index}`);
        if (sectionNames.has(name)) {
            Reject(`The ${label} duplicates section ${name}.`);
        }
        sectionNames.add(name);
        const dataStart = cursor + 20 + nameBytes;
        const dataEnd = dataStart + fileBytes;
        RequireAvailable(bytes, dataStart, fileBytes, `${label} section ${index} contents`);
        sections.push({
            index,
            kind,
            alignment,
            memoryBytes,
            fileBytes,
            name,
            dataStart,
            dataEnd,
        });
        cursor = dataEnd;
    }
    const symbols = [];
    const symbolsByName = new Map();
    for (let index = 0; index < symbolCount; index += 1) {
        RequireAvailable(bytes, cursor, 20, `${label} symbol ${index}`);
        const binding = bytes[cursor];
        const kind = bytes[cursor + 1];
        const reserved = bytes[cursor + 2] | bytes[cursor + 3];
        const sectionIndex = bytes.readUInt32LE(cursor + 4);
        const offset = bytes.readUInt32LE(cursor + 8);
        const size = bytes.readUInt32LE(cursor + 12);
        const nameBytes = bytes.readUInt32LE(cursor + 16);
        if (reserved !== 0 || binding < 1 || binding > 3 ||
            kind < 1 || kind > 2) {
            Reject(`The ${label} symbol ${index} record differs.`);
        }
        const name = ReadMachineName(bytes, cursor + 20, nameBytes, `${label} symbol ${index}`);
        if (symbolsByName.has(name)) {
            Reject(`The ${label} duplicates symbol ${name}.`);
        }
        if (binding === SYMBOL_BINDING_IMPORT) {
            if (sectionIndex !== IMPORT_SECTION_SENTINEL || offset !== 0 || size !== 0) {
                Reject(`The ${label} import symbol ${name} has definition fields.`);
            }
        } else {
            if (sectionIndex >= sections.length || size < 1) {
                Reject(`The ${label} defined symbol ${name} has invalid bounds.`);
            }
            const section = sections[sectionIndex];
            if (offset > section.memoryBytes || size > section.memoryBytes - offset) {
                Reject(`The ${label} defined symbol ${name} exceeds its section.`);
            }
            if ((kind === SYMBOL_KIND_FUNCTION && section.kind !== SECTION_KIND_CODE) ||
                (kind === SYMBOL_KIND_DATA && section.kind === SECTION_KIND_CODE)) {
                Reject(`The ${label} defined symbol ${name} has the wrong section kind.`);
            }
        }
        const symbol = { index, binding, kind, sectionIndex, offset, size, name };
        symbols.push(symbol);
        symbolsByName.set(name, symbol);
        cursor += 20 + nameBytes;
    }
    const relocations = [];
    for (let index = 0; index < relocationCount; index += 1) {
        RequireAvailable(bytes, cursor, 20, `${label} relocation ${index}`);
        const kind = bytes[cursor];
        const reserved = bytes[cursor + 1] | bytes.readUInt16LE(cursor + 2);
        const sectionIndex = bytes.readUInt32LE(cursor + 4);
        const offset = bytes.readUInt32LE(cursor + 8);
        const symbolIndex = bytes.readUInt32LE(cursor + 12);
        const addend = bytes.readInt32LE(cursor + 16);
        if (reserved !== 0 || kind < 1 || kind > 2 ||
            sectionIndex >= sections.length || symbolIndex >= symbols.length) {
            Reject(`The ${label} relocation ${index} record differs.`);
        }
        const section = sections[sectionIndex];
        if (offset > section.fileBytes || 4 > section.fileBytes - offset) {
            Reject(`The ${label} relocation ${index} patch exceeds its section.`);
        }
        const patch = section.dataStart + offset;
        if (bytes[patch] !== 0 || bytes[patch + 1] !== 0 ||
            bytes[patch + 2] !== 0 || bytes[patch + 3] !== 0) {
            Reject(`The ${label} relocation ${index} patch is not zero-filled.`);
        }
        relocations.push({ index, kind, sectionIndex, offset, symbolIndex, addend });
        cursor += 20;
    }
    if (cursor !== bytes.length) {
        Reject(`The ${label} WVO has trailing bytes.`);
    }
    return { label, bytes, sections, symbols, symbolsByName, relocations };
}

function RequireSymbolSet(actualSymbols, expectedSymbols, binding, label) {
    const actual = new Map(actualSymbols.map(symbol => [symbol.name, symbol]));
    if (actual.size !== expectedSymbols.length) {
        Reject(`The ${label} ${binding} set has the wrong size.`);
    }
    for (const expected of expectedSymbols) {
        const symbol = actual.get(expected.name);
        if (symbol === undefined || symbol.kind !== expected.kind) {
            Reject(`The ${label} ${binding} set differs at ${expected.name}.`);
        }
    }
}

function ValidateRoleObject(role, object) {
    const spec = ROLE_SPECS.get(role);
    if (spec === undefined) {
        Reject(`The current publisher role ${role} has no materialization specification.`);
    }
    if (object.sections.length !== spec.sections.length) {
        Reject(`The current publisher role ${role} section count differs.`);
    }
    for (let index = 0; index < spec.sections.length; index += 1) {
        const section = object.sections[index];
        if (section.name !== spec.sections[index]) {
            Reject(`The current publisher role ${role} section order differs.`);
        }
        if (section.fileBytes < 1 || section.memoryBytes !== section.fileBytes) {
            Reject(`The current publisher role ${role} section ${section.name} bounds differ.`);
        }
        if (section.name === '.text') {
            if (section.kind !== SECTION_KIND_CODE || section.alignment !== 16) {
                Reject(`The current publisher role ${role} .text section differs.`);
            }
        } else if (section.name === '.rodata') {
            if (section.kind !== SECTION_KIND_READ_ONLY || section.alignment !== 4) {
                Reject(`The current publisher role ${role} .rodata section differs.`);
            }
        } else {
            Reject(`The current publisher role ${role} has an unsupported section.`);
        }
    }
    RequireSymbolSet(
        object.symbols.filter(symbol => symbol.binding === SYMBOL_BINDING_IMPORT),
        spec.imports,
        'import',
        `current publisher role ${role}`,
    );
    RequireSymbolSet(
        object.symbols.filter(symbol => symbol.binding === SYMBOL_BINDING_EXPORT),
        spec.exports,
        'export',
        `current publisher role ${role}`,
    );
    for (const relocation of object.relocations) {
        const section = object.sections[relocation.sectionIndex];
        if (relocation.kind !== RELOCATION_KIND_RELATIVE_I32 ||
            relocation.addend !== -4 ||
            section.name !== '.text') {
            Reject(`The current publisher role ${role} relocation ${relocation.index} differs.`);
        }
    }
}

function RequireManifestHeader(bytes, magic, label) {
    if (bytes.length < 28 ||
        bytes.subarray(0, 4).toString('ascii') !== magic ||
        bytes.readUInt16LE(4) !== 1 ||
        bytes.readUInt16LE(6) !== 0 ||
        bytes.readUInt32LE(8) !== bytes.length) {
        Reject(`The ${label} manifest header differs.`);
    }
}

function ReadImageManifest(bytes, label) {
    RequireManifestHeader(bytes, 'WVLI', label);
    const payloadBytes = bytes.readUInt32LE(12);
    const entryOffset = bytes.readUInt32LE(16);
    const chunks = bytes.readUInt32LE(20);
    const chunkLimit = bytes.readUInt32LE(24);
    if (payloadBytes < 1 || payloadBytes > MAXIMUM_IMAGE_BYTES ||
        entryOffset >= payloadBytes ||
        chunks < 1 || chunks > 518 ||
        chunkLimit !== MAXIMUM_CHUNK_BYTES ||
        bytes.length !== 28 + chunks * 12) {
        Reject(`The ${label} manifest bounds differ.`);
    }
    const entries = [];
    let position = 0;
    for (let index = 0; index < chunks; index += 1) {
        const offset = 28 + index * 12;
        const chunkBytes = bytes.readUInt32LE(offset + 8);
        if (bytes.readUInt32LE(offset) !== index ||
            bytes.readUInt32LE(offset + 4) !== position ||
            chunkBytes < 1 || chunkBytes > MAXIMUM_CHUNK_BYTES ||
            chunkBytes > payloadBytes - position) {
            Reject(`The ${label} manifest chunk ${index} differs.`);
        }
        entries.push({ bytes: chunkBytes, position });
        position += chunkBytes;
    }
    if (position !== payloadBytes) {
        Reject(`The ${label} manifest extent differs.`);
    }
    return { bytes: payloadBytes, entryOffset, chunks, entries };
}

async function ReadCurrentImage(prefix, manifestFile, linkage) {
    const image = ReadImageManifest(manifestFile.bytes, 'current publisher image');
    if (RequiredNumber(linkage.fields, 'image-manifest-bytes', 'current publisher linkage', MAXIMUM_MANIFEST_BYTES) !== manifestFile.size ||
        RequiredSha256(linkage.fields, 'image-manifest-sha256', 'current publisher linkage') !== manifestFile.sha256 ||
        RequiredNumber(linkage.fields, 'image-bytes', 'current publisher linkage', MAXIMUM_IMAGE_BYTES) !== image.bytes ||
        RequiredNumber(linkage.fields, 'image-entry-offset', 'current publisher linkage', MAXIMUM_IMAGE_BYTES) !== image.entryOffset ||
        RequiredNumber(linkage.fields, 'image-chunks', 'current publisher linkage', 518) !== image.chunks) {
        Reject('The current publisher image manifest differs from the linkage record.');
    }
    const chunks = [];
    for (let index = 0; index < image.chunks; index += 1) {
        const chunk = await ReadOrdinaryFile(
            `${path.resolve(prefix)}.chunk-${index}`,
            `current publisher image chunk ${index}`,
            MAXIMUM_CHUNK_BYTES,
        );
        if (chunk.size !== image.entries[index].bytes ||
            RequiredNumber(linkage.fields, `image-chunk-${index}-bytes`, 'current publisher linkage', MAXIMUM_CHUNK_BYTES) !== chunk.size ||
            RequiredSha256(linkage.fields, `image-chunk-${index}-sha256`, 'current publisher linkage') !== chunk.sha256) {
            Reject(`The current publisher image chunk ${index} differs.`);
        }
        chunks.push(chunk.bytes);
    }
    const trailing = await lstat(`${path.resolve(prefix)}.chunk-${image.chunks}`)
        .catch(error => {
            if (error.code === 'ENOENT') return null;
            throw error;
        });
    if (trailing !== null) {
        Reject('The current publisher image chunk set has a trailing chunk.');
    }
    return { ...image, bytesBuffer: Buffer.concat(chunks) };
}

function Align(value, alignment) {
    const remainder = value % alignment;
    return remainder === 0 ? value : value + alignment - remainder;
}

function PlaceObjectSections(object, startAddress) {
    let cursor = startAddress;
    const sectionAddresses = new Map();
    for (const section of object.sections) {
        cursor = Align(cursor, section.alignment);
        sectionAddresses.set(section.index, cursor);
        cursor += section.memoryBytes;
    }
    return { sectionAddresses, endAddress: cursor };
}

function SectionByName(object, name) {
    return object.sections.find(section => section.name === name);
}

function SymbolAddress(symbol, placements) {
    const sectionAddress = placements.sectionAddresses.get(symbol.sectionIndex);
    if (sectionAddress === undefined) {
        Reject(`The symbol ${symbol.name} has no section placement.`);
    }
    return sectionAddress + symbol.offset;
}

function KindName(kind) {
    if (kind === SYMBOL_KIND_FUNCTION) return 'function';
    if (kind === SYMBOL_KIND_DATA) return 'data';
    Reject('The symbol kind is unsupported.');
}

function CheckRelativeI32(sourceAddress, targetAddress, addend, label) {
    const displacement = BigInt(targetAddress) - BigInt(sourceAddress) + BigInt(addend);
    if (displacement < -2_147_483_648n || displacement > 2_147_483_647n) {
        Reject(`The ${label} relative-i32 displacement is out of range.`);
    }
    return Number(displacement);
}

function BuildLayout(target, objectsByRole, image, linkageTarget) {
    const startup = objectsByRole.get(target.startupRole);
    const adapter = objectsByRole.get(target.adapterRole);
    const sha = objectsByRole.get(5);
    const transaction = objectsByRole.get(6);
    const startupPlacements = PlaceObjectSections(startup, STARTUP_BASE_ADDRESS);
    let cursor = Align(IMAGE_BASE_ADDRESS + image.bytes, 16);
    const adapterPlacements = PlaceObjectSections(adapter, cursor);
    cursor = adapterPlacements.endAddress;
    const shaPlacements = PlaceObjectSections(sha, cursor);
    cursor = shaPlacements.endAddress;
    const transactionPlacements = PlaceObjectSections(transaction, cursor);
    cursor = transactionPlacements.endAddress;

    const entries = new Map([
        [target.startupRole, { role: target.startupRole, object: startup, placements: startupPlacements }],
        [target.adapterRole, { role: target.adapterRole, object: adapter, placements: adapterPlacements }],
        [5, { role: 5, object: sha, placements: shaPlacements }],
        [6, { role: 6, object: transaction, placements: transactionPlacements }],
    ]);
    const resolved = new Map([
        ['Native_main', { kind: SYMBOL_KIND_FUNCTION, address: IMAGE_BASE_ADDRESS + image.entryOffset }],
    ]);
    for (const role of [target.adapterRole, 5, 6]) {
        const entry = entries.get(role);
        for (const symbol of entry.object.symbols) {
            if (symbol.binding === SYMBOL_BINDING_EXPORT) {
                resolved.set(symbol.name, {
                    kind: symbol.kind,
                    address: SymbolAddress(symbol, entry.placements),
                });
            }
        }
    }
    const imageEnd = cursor;
    const expected = new Map([
        ['current-image-address', IMAGE_BASE_ADDRESS],
        ['native-main-address', IMAGE_BASE_ADDRESS + image.entryOffset],
        ['adapter-address', adapterPlacements.sectionAddresses.get(0)],
        ['sha-text-address', shaPlacements.sectionAddresses.get(SectionByName(sha, '.text').index)],
        ['sha-read-only-address', shaPlacements.sectionAddresses.get(SectionByName(sha, '.rodata').index)],
        ['transaction-address', transactionPlacements.sectionAddresses.get(0)],
        ['end-address', imageEnd],
    ]);
    for (const [field, value] of expected) {
        if (RequiredNumber(linkageTarget.fields, field, `target ${target.name} linkage`) !== value) {
            Reject(`The target ${target.name} linkage field ${field} differs from materialization.`);
        }
    }
    return {
        target,
        entries,
        resolved,
        imageBytes: image.bytes,
        imageEntry: image.entryOffset,
        nativeBytes: imageEnd - IMAGE_BASE_ADDRESS,
    };
}

function CopyObjectSections(output, outputBaseAddress, entry) {
    for (const section of entry.object.sections) {
        const sectionAddress = entry.placements.sectionAddresses.get(section.index);
        const outputOffset = sectionAddress - outputBaseAddress;
        if (outputOffset < 0 || outputOffset + section.fileBytes > output.length) {
            Reject(`The current publisher role ${entry.role} section ${section.name} exceeds output.`);
        }
        entry.object.bytes.copy(output, outputOffset, section.dataStart, section.dataEnd);
    }
}

function ResolveRelocation(entry, symbol, resolved, hostImports, allowDeferred, label) {
    if (symbol.binding === SYMBOL_BINDING_IMPORT) {
        const native = resolved.get(symbol.name);
        if (native !== undefined) {
            if (native.kind !== symbol.kind) {
                Reject(`The ${label} import ${symbol.name} kind differs.`);
            }
            return native.address;
        }
        const hosted = hostImports?.boundImports.get(symbol.name);
        if (hosted !== undefined) {
            if (hosted.kind !== KindName(symbol.kind)) {
                Reject(`The ${label} host import ${symbol.name} kind differs.`);
            }
            return hosted.address;
        }
        if (allowDeferred) {
            return null;
        }
        Reject(`The ${label} import ${symbol.name} is unresolved.`);
    }
    return SymbolAddress(symbol, entry.placements);
}

function PatchRelocations(output, outputBaseAddress, entry, resolved, hostImports, allowDeferred) {
    for (const relocation of entry.object.relocations) {
        const sectionAddress = entry.placements.sectionAddresses.get(relocation.sectionIndex);
        const sourceAddress = sectionAddress + relocation.offset;
        const patchOffset = sourceAddress - outputBaseAddress;
        if (patchOffset < 0 || patchOffset + 4 > output.length) {
            Reject(`The current publisher role ${entry.role} relocation ${relocation.index} exceeds output.`);
        }
        const symbol = entry.object.symbols[relocation.symbolIndex];
        const targetAddress = ResolveRelocation(
            entry,
            symbol,
            resolved,
            hostImports,
            allowDeferred,
            `current publisher role ${entry.role} relocation ${relocation.index}`,
        );
        if (targetAddress === null) continue;
        const displacement = CheckRelativeI32(
            sourceAddress,
            targetAddress,
            relocation.addend,
            `current publisher role ${entry.role} relocation ${relocation.index}`,
        );
        output.writeInt32LE(displacement, patchOffset);
    }
}

function MaterializeNativeImage(layout, image, hostImports = null) {
    const output = Buffer.alloc(layout.nativeBytes);
    image.bytesBuffer.copy(output, 0);
    for (const role of [layout.target.adapterRole, 5, 6]) {
        CopyObjectSections(output, IMAGE_BASE_ADDRESS, layout.entries.get(role));
    }
    for (const role of [layout.target.adapterRole, 5, 6]) {
        PatchRelocations(
            output,
            IMAGE_BASE_ADDRESS,
            layout.entries.get(role),
            layout.resolved,
            hostImports,
            hostImports === null,
        );
    }
    return output;
}

function MaterializeStartup(layout, startupBytes) {
    const startup = Buffer.alloc(startupBytes);
    const entry = layout.entries.get(layout.target.startupRole);
    CopyObjectSections(startup, STARTUP_BASE_ADDRESS, entry);
    PatchRelocations(startup, STARTUP_BASE_ADDRESS, entry, layout.resolved, null, false);
    return startup;
}

function ReadU32(bytes, offset, label) {
    if (offset < 0 || offset + 4 > bytes.length) {
        Reject(`The ${label} u32 field exceeds the file.`);
    }
    return bytes.readUInt32LE(offset);
}

function WriteU32(bytes, offset, value, label) {
    if (offset < 0 || offset + 4 > bytes.length ||
        value < 0 || value > 0xffff_ffff || !Number.isInteger(value)) {
        Reject(`The ${label} u32 patch exceeds the file.`);
    }
    bytes.writeUInt32LE(value, offset);
}

function WriteU64Address(bytes, offset, value, label) {
    WriteU32(bytes, offset, value, label);
    WriteU32(bytes, offset + 4, 0, label);
}

function WriteAscii(bytes, offset, value, label) {
    const text = Buffer.from(value, 'ascii');
    if (text.length !== value.length || offset < 0 ||
        offset + text.length > bytes.length) {
        Reject(`The ${label} ASCII patch exceeds the file.`);
    }
    text.copy(bytes, offset);
}

function ParsePlanFile(file, target) {
    const bytes = file.bytes;
    if (bytes.length < 128 ||
        ReadU32(bytes, 0, 'hosted-container plan') !== 1_145_263_703 ||
        ReadU32(bytes, 4, 'hosted-container plan') !== 1 ||
        ReadU32(bytes, 8, 'hosted-container plan') !== bytes.length ||
        ReadU32(bytes, 12, 'hosted-container plan') !== 0) {
        Reject('The hosted-container plan header differs.');
    }
    const targetId = ReadU32(bytes, 20, 'hosted-container plan');
    if ((target.name === 'windows-x64' && targetId !== 1) ||
        (target.name === 'linux-x64' && targetId !== 2)) {
        Reject('The hosted-container plan target differs.');
    }
    return {
        targetId,
        profile: ReadU32(bytes, 24, 'hosted-container plan'),
        applicationBytes: ReadU32(bytes, 28, 'hosted-container plan'),
        headerBytes: ReadU32(bytes, 36, 'hosted-container plan'),
        startupOffset: ReadU32(bytes, 40, 'hosted-container plan'),
        startupBytes: ReadU32(bytes, 44, 'hosted-container plan'),
        importOffset: ReadU32(bytes, 56, 'hosted-container plan'),
        importBytes: ReadU32(bytes, 60, 'hosted-container plan'),
        textAddress: ReadU32(bytes, 80, 'hosted-container plan'),
        runtimeAddress: ReadU32(bytes, 88, 'hosted-container plan'),
        imageVirtualBytes: ReadU32(bytes, 92, 'hosted-container plan'),
        relocationAddress: ReadU32(bytes, 104, 'hosted-container plan'),
        importAddress: ReadU32(bytes, 108, 'hosted-container plan'),
    };
}

function PatchWindowsPublisherDescriptor(page, offset, lookup, name, iat) {
    WriteU32(page, offset, lookup, 'Windows publisher import descriptor');
    WriteU32(page, offset + 12, name, 'Windows publisher import descriptor');
    WriteU32(page, offset + 16, iat, 'Windows publisher import descriptor');
}

function PatchWindowsPublisherImport(page, importAddress, lookupOffset, iatOffset, index, nameOffset, name) {
    const nameAddress = importAddress + nameOffset;
    WriteU64Address(
        page,
        lookupOffset + index * 8,
        nameAddress,
        `Windows publisher import lookup ${name}`,
    );
    WriteU64Address(
        page,
        iatOffset + index * 8,
        nameAddress,
        `Windows publisher import address ${name}`,
    );
    WriteAscii(page, nameOffset + 2, name, `Windows publisher import name ${name}`);
}

function BuildWindowsPublisherImportPage(importAddress) {
    const page = Buffer.alloc(WINDOWS_IMPORT_PAGE_BYTES);
    PatchWindowsPublisherDescriptor(page, 0, importAddress + 80, importAddress + 724, importAddress + 240);
    PatchWindowsPublisherDescriptor(page, 20, importAddress + 208, importAddress + 738, importAddress + 368);
    PatchWindowsPublisherDescriptor(page, 40, importAddress + 224, importAddress + 748, importAddress + 384);
    PatchWindowsPublisherImport(page, importAddress, 80, 240, 0, 400, 'CloseHandle');
    PatchWindowsPublisherImport(page, importAddress, 80, 240, 1, 414, 'CreateFileW');
    PatchWindowsPublisherImport(page, importAddress, 80, 240, 2, 428, 'FlushFileBuffers');
    PatchWindowsPublisherImport(page, importAddress, 80, 240, 3, 448, 'GetCommandLineW');
    PatchWindowsPublisherImport(page, importAddress, 80, 240, 4, 466, 'GetFileInformationByHandle');
    PatchWindowsPublisherImport(page, importAddress, 80, 240, 5, 496, 'GetFileSizeEx');
    PatchWindowsPublisherImport(page, importAddress, 80, 240, 6, 512, 'GetLastError');
    PatchWindowsPublisherImport(page, importAddress, 80, 240, 7, 528, 'GetStdHandle');
    PatchWindowsPublisherImport(page, importAddress, 80, 240, 8, 544, 'LocalFree');
    PatchWindowsPublisherImport(page, importAddress, 80, 240, 9, 556, 'MultiByteToWideChar');
    PatchWindowsPublisherImport(page, importAddress, 80, 240, 10, 578, 'ReadFile');
    PatchWindowsPublisherImport(page, importAddress, 80, 240, 11, 590, 'SetFileInformationByHandle');
    PatchWindowsPublisherImport(page, importAddress, 80, 240, 12, 620, 'SetFilePointerEx');
    PatchWindowsPublisherImport(page, importAddress, 80, 240, 13, 640, 'WideCharToMultiByte');
    PatchWindowsPublisherImport(page, importAddress, 80, 240, 14, 662, 'WriteFile');
    PatchWindowsPublisherImport(page, importAddress, 208, 368, 0, 674, 'NtSetInformationFile');
    PatchWindowsPublisherImport(page, importAddress, 224, 384, 0, 702, 'CommandLineToArgvW');
    WriteAscii(page, 724, 'KERNEL32.dll', 'Windows publisher kernel DLL name');
    WriteAscii(page, 738, 'ntdll.dll', 'Windows publisher NT DLL name');
    WriteAscii(page, 748, 'SHELL32.dll', 'Windows publisher shell DLL name');
    return page;
}

function PatchWindowsPublisherImports(application, plan) {
    if (plan.headerBytes !== 512 ||
        plan.importBytes !== WINDOWS_IMPORT_PAGE_BYTES ||
        plan.importOffset < plan.headerBytes ||
        plan.importOffset + WINDOWS_IMPORT_PAGE_BYTES > application.length ||
        ReadU32(application, 272, 'Windows PE import directory') !== plan.importAddress ||
        ReadU32(application, 276, 'Windows PE import directory') !== 60 ||
        ReadU32(application, 360, 'Windows PE IAT directory') !== plan.importAddress + 192 ||
        ReadU32(application, 364, 'Windows PE IAT directory') !== 128 ||
        ReadU32(application, 444, 'Windows PE data section') !== plan.importAddress ||
        ReadU32(application, 452, 'Windows PE data section') !== plan.importOffset ||
        ReadU32(application, 484, 'Windows PE relocation section') !== plan.relocationAddress) {
        Reject('The current publisher Windows executable import geometry differs.');
    }
    WriteU32(application, 272, plan.importAddress, 'Windows PE import directory address');
    WriteU32(application, 276, WINDOWS_PUBLISHER_IMPORT_DIRECTORY_BYTES, 'Windows PE import directory bytes');
    WriteU32(
        application,
        360,
        plan.importAddress + WINDOWS_PUBLISHER_IAT_DIRECTORY_OFFSET,
        'Windows PE IAT directory address',
    );
    WriteU32(application, 364, WINDOWS_PUBLISHER_IAT_DIRECTORY_BYTES, 'Windows PE IAT directory bytes');
    BuildWindowsPublisherImportPage(plan.importAddress).copy(application, plan.importOffset);
}

function ValidateExecutableMagic(application, target) {
    if (target.name === 'windows-x64') {
        if (application.length < 2 || application[0] !== 0x4d || application[1] !== 0x5a) {
            Reject('The current publisher Windows executable header differs.');
        }
        return;
    }
    if (application.length < 4 ||
        application[0] !== 0x7f ||
        application[1] !== 0x45 ||
        application[2] !== 0x4c ||
        application[3] !== 0x46) {
        Reject('The current publisher Linux executable header differs.');
    }
}

async function WriteChunkSet(prefix, bytes) {
    if (bytes.length < 1 || bytes.length > MAXIMUM_IMAGE_BYTES) {
        Reject('The materialized current publisher native image size is invalid.');
    }
    const chunks = [];
    for (let offset = 0; offset < bytes.length; offset += MAXIMUM_CHUNK_BYTES) {
        const chunk = bytes.subarray(offset, Math.min(offset + MAXIMUM_CHUNK_BYTES, bytes.length));
        chunks.push(chunk);
        await writeFile(`${prefix}.chunk-${chunks.length - 1}`, chunk);
    }
    if (chunks.length > 16) {
        Reject('The materialized current publisher native image exceeds the hosted package chunk limit.');
    }
    return chunks.length;
}

function PackageScript() {
    return path.join(REPOSITORY, 'Tools', 'Native', WINDOWS ? 'Package-Hosted-Wvb.cmd' : 'Package-Hosted-Wvb.sh');
}

function CommandFor(tool, args) {
    if (WINDOWS && tool.endsWith('.cmd')) {
        if ([tool, ...args].some(value => /[\r\n&|<>^%!"]/u.test(value))) {
            Reject('The package command arguments are outside the safe Windows command subset.');
        }
        return {
            command: process.env.ComSpec ?? 'cmd.exe',
            args: ['/d', '/v:off', '/s', '/c', `"${[tool, ...args].map(value => `"${value}"`).join(' ')}"`],
            options: { windowsHide: true, windowsVerbatimArguments: true },
        };
    }
    return {
        command: tool,
        args,
        options: { windowsHide: true },
    };
}

async function RunCommand(tool, args, label) {
    const deadline = Date.now() + MATERIALIZATION_DEADLINE_MS;
    const remaining = deadline - Date.now();
    const invocation = CommandFor(tool, args);
    return new Promise((resolve, reject) => {
        const child = spawn(invocation.command, invocation.args, {
            cwd: REPOSITORY,
            stdio: ['ignore', 'pipe', 'pipe'],
            ...invocation.options,
        });
        const output = [];
        const errors = [];
        let outputBytes = 0;
        const timer = setTimeout(() => {
            if (child.pid !== undefined) {
                if (WINDOWS) {
                    spawnSync('taskkill.exe', ['/pid', String(child.pid), '/t', '/f'], {
                        windowsHide: true,
                        timeout: 2_000,
                        stdio: 'ignore',
                    });
                }
                child.kill('SIGKILL');
            }
            reject(Object.assign(new Error(`${label} timed out.`), { exitCode: 124 }));
        }, remaining);
        for (const [stream, chunks] of [[child.stdout, output], [child.stderr, errors]]) {
            stream.on('data', chunk => {
                outputBytes += chunk.length;
                if (outputBytes > 1_048_576) {
                    child.kill('SIGKILL');
                    reject(Object.assign(new Error(`${label} exceeded its diagnostic limit.`), { exitCode: 2 }));
                } else {
                    chunks.push(chunk);
                }
            });
        }
        child.once('error', error => {
            clearTimeout(timer);
            reject(error);
        });
        child.once('close', code => {
            clearTimeout(timer);
            const stdout = Buffer.concat(output).toString('utf8');
            const stderr = Buffer.concat(errors).toString('utf8');
            if (code !== 0 || stderr !== '') {
                reject(Object.assign(
                    new Error(`${label} failed (${code}): ${stderr || stdout}`),
                    { exitCode: code ?? 1 },
                ));
                return;
            }
            resolve(stdout);
        });
    });
}

async function RunPackagePlan(target, sourceWvb, chunkPrefix, chunkCount, entryOffset, runtimePath, planPath) {
    await RunCommand(PackageScript(), [
        'plan',
        PACKAGE_PROFILE,
        sourceWvb,
        chunkPrefix,
        String(chunkCount),
        String(entryOffset),
        runtimePath,
        planPath,
        target.packageTarget,
    ], `current publisher ${target.name} hosted plan`);
}

async function RunPackageImage(target, sourceWvb, chunkPrefix, chunkCount, entryOffset, outputPath) {
    await RunCommand(PackageScript(), [
        'image',
        PACKAGE_PROFILE,
        sourceWvb,
        chunkPrefix,
        String(chunkCount),
        String(entryOffset),
        outputPath,
        target.packageTarget,
    ], `current publisher ${target.name} executable package`);
}

async function RunHostImportBinding(linkagePath, target, runtimePath, planPath, outputPath) {
    await RunCommand(process.execPath, [
        path.join(REPOSITORY, 'Tools', 'Native', 'Bind-Current-Publisher-Host-Imports.mjs'),
        linkagePath,
        target.name,
        runtimePath,
        planPath,
        outputPath,
    ], `current publisher ${target.name} host-import binding`);
}

async function ReadObjects(objectDirectory, linkage) {
    const objectsByRole = new Map();
    const inputPaths = [];
    for (const { role, leaf } of NATIVE_OBJECTS) {
        const object = await ReadOrdinaryFile(
            path.join(objectDirectory, leaf),
            `current publisher native object role ${role}`,
            MAXIMUM_CURRENT_OBJECT_BYTES,
        );
        const linked = linkage.nativeObjects.get(role);
        if (linked === undefined || linked.leaf !== leaf ||
            linked.bytes !== object.size || linked.sha256 !== object.sha256) {
            Reject(`The current publisher native object role ${role} differs from the linkage record.`);
        }
        const parsed = ParseWvo(object.bytes, `current publisher native object role ${role}`);
        ValidateRoleObject(role, parsed);
        objectsByRole.set(role, parsed);
        inputPaths.push(object.absolute);
    }
    return { objectsByRole, inputPaths };
}

function ValidateHostImports(hostImports, linkage, target, finalRuntime, finalPlan, layout) {
    if (RequiredField(hostImports.fields, 'target', 'current publisher host-import record') !== target.name ||
        RequiredSha256(hostImports.fields, 'linkage-sha256', 'current publisher host-import record') !== linkage.sha256 ||
        RequiredSha256(hostImports.fields, 'runtime-sha256', 'current publisher host-import record') !== finalRuntime.sha256 ||
        RequiredSha256(hostImports.fields, 'plan-sha256', 'current publisher host-import record') !== finalPlan.sha256 ||
        RequiredNumber(hostImports.fields, 'current-image-bytes', 'current publisher host-import record', MAXIMUM_IMAGE_BYTES) !== layout.imageBytes ||
        RequiredNumber(hostImports.fields, 'native-image-bytes', 'current publisher host-import record', MAXIMUM_IMAGE_BYTES) !== layout.nativeBytes ||
        RequiredNumber(hostImports.fields, 'native-entry-offset', 'current publisher host-import record', MAXIMUM_IMAGE_BYTES) !== layout.imageEntry) {
        Reject('The current publisher final host-import record does not match materialization.');
    }
}

async function PatchExecutableStartup(genericApplication, planFile, target, startup) {
    const plan = ParsePlanFile(planFile, target);
    if (plan.profile !== Number(PACKAGE_PROFILE) ||
        plan.startupBytes < 1 ||
        startup.length !== plan.startupBytes ||
        plan.startupOffset > plan.applicationBytes ||
        startup.length > plan.applicationBytes - plan.startupOffset) {
        Reject('The current publisher final plan startup geometry differs.');
    }
    const application = await readFile(genericApplication);
    const fileOffset = plan.startupOffset;
    if (fileOffset < 0 || fileOffset + startup.length > application.length) {
        Reject('The current publisher startup patch exceeds the executable.');
    }
    startup.copy(application, fileOffset);
    if (target.name === 'windows-x64') {
        PatchWindowsPublisherImports(application, plan);
    }
    ValidateExecutableMagic(application, target);
    return application;
}

async function Main() {
    if (process.argv.length !== 10) {
        Reject(
            'Usage: node Tools/Native/Materialize-Current-Publisher-Executable.mjs ' +
            '<current-publisher-linkage.wvcl> <windows-x64|linux-x64> ' +
            '<image-chunk-prefix> <publisher.wvli> <reference-object-directory> ' +
            '<publisher.wvb> <output.exe|output.elf> <host-imports.wvci>',
            64,
        );
    }
    const target = TARGETS.get(process.argv[3]);
    if (target === undefined) {
        Reject('The current publisher executable target is unsupported.', 64);
    }
    const linkageFile = await ReadOrdinaryFile(process.argv[2], 'current publisher linkage record', MAXIMUM_RECORD_BYTES);
    const imageManifestFile = await ReadOrdinaryFile(process.argv[5], 'current publisher WVLI manifest', MAXIMUM_MANIFEST_BYTES);
    const objectDirectory = await RequireCanonicalDirectory(process.argv[6], 'current publisher reference object directory');
    const sourceWvb = await ReadOrdinaryFile(process.argv[7], 'current publisher WVB', 16_777_216);
    const linkage = ParseLinkageRecord(linkageFile);
    const linkageTarget = linkage.targets.get(target.name);
    if (linkageTarget === undefined) {
        Reject(`The current publisher linkage record is missing target ${target.name}.`);
    }
    const image = await ReadCurrentImage(process.argv[4], imageManifestFile, linkage);
    const objects = await ReadObjects(objectDirectory, linkage);
    const inputPaths = [
        linkageFile.absolute,
        imageManifestFile.absolute,
        sourceWvb.absolute,
        ...objects.inputPaths,
    ];
    const outputPath = await RequireOutputPath(
        process.argv[8],
        inputPaths,
        'current publisher executable output',
        target.outputExtension,
    );
    const hostImportsOutput = await RequireOutputPath(
        process.argv[9],
        [...inputPaths, outputPath],
        'current publisher host-import output',
        '.wvci',
    );

    const layout = BuildLayout(target, objects.objectsByRole, image, linkageTarget);
    const temporaryDirectory = await mkdtemp(path.join(tmpdir(), 'windvale-current-publisher-materialize-'));
    try {
        const provisionalImage = MaterializeNativeImage(layout, image, null);
        const provisionalPrefix = path.join(temporaryDirectory, 'Provisional-Image');
        const provisionalChunks = await WriteChunkSet(provisionalPrefix, provisionalImage);
        const provisionalRuntime = path.join(temporaryDirectory, 'Provisional.wvhr');
        const provisionalPlan = path.join(temporaryDirectory, 'Provisional.wvcd');
        await RunPackagePlan(target, sourceWvb.absolute, provisionalPrefix, provisionalChunks, image.entryOffset, provisionalRuntime, provisionalPlan);
        const provisionalHostImportsPath = path.join(temporaryDirectory, 'Provisional.wvci');
        await RunHostImportBinding(linkageFile.absolute, target, provisionalRuntime, provisionalPlan, provisionalHostImportsPath);
        const provisionalHostImports = ParseHostImportsRecord(
            await ReadOrdinaryFile(provisionalHostImportsPath, 'provisional host-import record', MAXIMUM_RECORD_BYTES),
        );

        const finalImage = MaterializeNativeImage(layout, image, provisionalHostImports);
        const finalPrefix = path.join(temporaryDirectory, 'Final-Image');
        const finalChunks = await WriteChunkSet(finalPrefix, finalImage);
        const finalRuntimePath = path.join(temporaryDirectory, 'Final.wvhr');
        const finalPlanPath = path.join(temporaryDirectory, 'Final.wvcd');
        await RunPackagePlan(target, sourceWvb.absolute, finalPrefix, finalChunks, image.entryOffset, finalRuntimePath, finalPlanPath);
        const finalHostImportsPath = path.join(temporaryDirectory, 'Final.wvci');
        await RunHostImportBinding(linkageFile.absolute, target, finalRuntimePath, finalPlanPath, finalHostImportsPath);
        const finalRuntime = await ReadOrdinaryFile(finalRuntimePath, 'final runtime header', 4_096);
        const finalPlan = await ReadOrdinaryFile(finalPlanPath, 'final hosted-container plan', 4_096);
        const finalHostImportsFile = await ReadOrdinaryFile(finalHostImportsPath, 'final host-import record', MAXIMUM_RECORD_BYTES);
        const finalHostImports = ParseHostImportsRecord(finalHostImportsFile);
        ValidateHostImports(finalHostImports, linkage, target, finalRuntime, finalPlan, layout);
        const finalCheckImage = MaterializeNativeImage(layout, image, finalHostImports);
        if (!finalCheckImage.equals(finalImage)) {
            Reject('The current publisher final host-import addresses changed after final runtime planning.');
        }

        const genericApplicationPath = path.join(
            temporaryDirectory,
            target.name === 'windows-x64' ? 'Generic.exe' : 'Generic.elf',
        );
        await RunPackageImage(target, sourceWvb.absolute, finalPrefix, finalChunks, image.entryOffset, genericApplicationPath);
        const finalPlanParsed = ParsePlanFile(finalPlan, target);
        const startup = MaterializeStartup(layout, finalPlanParsed.startupBytes);
        const application = await PatchExecutableStartup(genericApplicationPath, finalPlan, target, startup);
        await writeFile(outputPath, application, { flag: 'wx' });
        await copyFile(finalHostImportsPath, hostImportsOutput);
        process.stdout.write(
            `current publisher executable materialization status=Valid target=${target.name} ` +
            `bytes=${application.length} sha256=${Sha256(application)} ` +
            `host-imports-sha256=${finalHostImports.sha256}\n`,
        );
    } finally {
        await rm(temporaryDirectory, { recursive: true, force: true });
    }
}

try {
    await Main();
} catch (error) {
    process.stderr.write(`${error.message}\n`);
    process.exitCode = error.exitCode ?? 1;
}
