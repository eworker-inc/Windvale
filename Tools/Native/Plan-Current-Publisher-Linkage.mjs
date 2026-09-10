import { createHash } from 'node:crypto';
import { lstat, readFile, realpath, writeFile } from 'node:fs/promises';
import path from 'node:path';

const WINDOWS = process.platform === 'win32';
const HOST_FAMILY = WINDOWS ? 'windows-x64' : 'linux-x64';
const BINDING_FORMAT = 'windvale-current-source-wvb-publisher-binding 1';
const LINKAGE_FORMAT = 'windvale-current-source-wvb-publisher-linkage 1';
const MAXIMUM_BINDING_BYTES = 131_072;
const MAXIMUM_MANIFEST_BYTES = 6_244;
const MAXIMUM_CHUNK_BYTES = 4_194_304;
const MAXIMUM_IMAGE_BYTES = 67_108_864;
const MAXIMUM_CURRENT_OBJECT_BYTES = 65_536;
const IMAGE_BASE_ADDRESS = 8_192;
const STARTUP_BASE_ADDRESS = 4_096;
const IMPORT_SECTION_SENTINEL = 0xffff_ffff;

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

const NATIVE_OBJECT_BY_ROLE = new Map(NATIVE_OBJECTS.map(object => [object.role, object]));

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
        entry: 'Windows_wvb_publisher_startup',
        exports: Object.freeze([FunctionSymbol('Windows_wvb_publisher_startup')]),
        imports: Object.freeze([FunctionSymbol('Windows_wvb_publisher_run')]),
        sections: Object.freeze(['.text']),
    }],
    [2, {
        entry: 'Linux_wvb_publisher_startup',
        exports: Object.freeze([FunctionSymbol('Linux_wvb_publisher_startup')]),
        imports: Object.freeze([FunctionSymbol('Linux_wvb_publisher_run')]),
        sections: Object.freeze(['.text']),
    }],
    [3, {
        entry: 'Windows_wvb_publisher_run',
        exports: Object.freeze([FunctionSymbol('Windows_wvb_publisher_run')]),
        imports: Object.freeze([...COMMON_ADAPTER_IMPORTS, ...WINDOWS_IAT_IMPORTS]),
        sections: Object.freeze(['.text']),
    }],
    [4, {
        entry: 'Linux_wvb_publisher_run',
        exports: Object.freeze([FunctionSymbol('Linux_wvb_publisher_run')]),
        imports: COMMON_ADAPTER_IMPORTS,
        sections: Object.freeze(['.text']),
    }],
    [5, {
        entry: 'X64_wvb_publication_sha256_hex',
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
        entry: 'Native_publication_begin',
        exports: Object.freeze([
            FunctionSymbol('Native_publication_apply'),
            FunctionSymbol('Native_publication_begin'),
        ]),
        imports: Object.freeze([]),
        sections: Object.freeze(['.text']),
    }],
]);

const TARGETS = Object.freeze([
    {
        name: 'windows-x64',
        startupRole: 1,
        adapterRole: 3,
        startupEntry: 'Windows_wvb_publisher_startup',
    },
    {
        name: 'linux-x64',
        startupRole: 2,
        adapterRole: 4,
        startupEntry: 'Linux_wvb_publisher_startup',
    },
]);

const NATIVE_RESOLVED_IMPORTS = new Set([
    'Native_main',
    'Native_publication_apply',
    'Native_publication_begin',
    'X64_wvb_publication_report_newline',
    'X64_wvb_publication_report_prefix',
    'X64_wvb_publication_report_separator',
    'X64_wvb_publication_sha256_hex',
    'X64_wvb_publication_u32_hex8',
    'Windows_wvb_publisher_run',
    'Linux_wvb_publisher_run',
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
    await RequireCanonicalDirectory(path.dirname(absolute), 'publisher linkage output parent');
    for (const input of inputPaths) {
        if (SamePath(absolute, input)) {
            Reject('The publisher linkage output must be distinct from every input.', 64);
        }
    }
    const information = await lstat(absolute).catch(error => {
        if (error.code === 'ENOENT') return null;
        throw error;
    });
    if (information !== null) {
        Reject('The publisher linkage output already exists.', 64);
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

function ParseBindingRecord(file) {
    RequireAsciiLf(file.bytes, 'publisher binding');
    const text = file.bytes.toString('ascii');
    const lines = text.split('\n');
    if (lines[lines.length - 1] !== '' || lines.length < 4 ||
        lines[0] !== BINDING_FORMAT) {
        Reject('The publisher binding record header differs.');
    }
    const fields = new Map();
    const nativeObjects = new Map();
    for (let index = 1; index < lines.length - 1; index += 1) {
        const line = lines[index];
        if (line.length === 0) {
            Reject('The publisher binding record contains an interior blank line.');
        }
        const native = /^native-object-([1-6]) ([A-Za-z0-9_.-]+) bytes ([0-9]+) sha256 ([0-9a-f]{64})$/.exec(line);
        if (native !== null) {
            const role = Number(native[1]);
            if (nativeObjects.has(role)) {
                Reject(`The publisher binding record duplicates native object ${role}.`);
            }
            nativeObjects.set(role, {
                role,
                leaf: native[2],
                bytes: ParseUnsignedDecimal(native[3], `native object ${role} byte count`),
                sha256: native[4],
            });
            continue;
        }
        const field = /^([A-Za-z0-9-]+) ([A-Za-z0-9_.-]+)$/.exec(line);
        if (field === null) {
            Reject(`The publisher binding record field is malformed: ${line}`);
        }
        SetOnce(fields, field[1], field[2], 'publisher binding record');
    }
    const bindingSha256 = fields.get('binding-sha256');
    if (bindingSha256 === undefined || !IsHexSha256(bindingSha256)) {
        Reject('The publisher binding record self hash is invalid.');
    }
    const unsignedLines = [lines[0], lines[1], ...lines.slice(3)];
    const unsignedBytes = Buffer.from(unsignedLines.join('\n'), 'ascii');
    if (Sha256(unsignedBytes) !== bindingSha256) {
        Reject('The publisher binding record self hash differs.');
    }
    for (const { role, leaf } of NATIVE_OBJECTS) {
        const object = nativeObjects.get(role);
        if (object === undefined || object.leaf !== leaf) {
            Reject(`The publisher binding record native object ${role} differs.`);
        }
    }
    return { fields, nativeObjects, sha256: bindingSha256 };
}

function RequiredField(binding, key) {
    const value = binding.fields.get(key);
    if (value === undefined) {
        Reject(`The publisher binding record is missing ${key}.`);
    }
    return value;
}

function RequiredNumber(binding, key, maximum = Number.MAX_SAFE_INTEGER) {
    return ParseUnsignedDecimal(RequiredField(binding, key), key, maximum);
}

function RequiredSha(binding, key) {
    const value = RequiredField(binding, key);
    if (!IsHexSha256(value)) {
        Reject(`The publisher binding record field ${key} is not a SHA-256 value.`);
    }
    return value;
}

function RequireManifestHeader(bytes, magic, label) {
    if (bytes.length < (magic === 'WVOP' ? 24 : 28) ||
        bytes.subarray(0, 4).toString('ascii') !== magic ||
        bytes.readUInt16LE(4) !== 1 ||
        bytes.readUInt16LE(6) !== 0 ||
        bytes.readUInt32LE(8) !== bytes.length) {
        Reject(`The ${label} manifest header differs.`);
    }
}

function ReadSegmentedManifest(bytes, magic, label) {
    RequireManifestHeader(bytes, magic, label);
    const isObject = magic === 'WVOP';
    const payloadBytes = bytes.readUInt32LE(12);
    const entryOffset = isObject ? 0 : bytes.readUInt32LE(16);
    const countOffset = isObject ? 16 : 20;
    const limitOffset = isObject ? 20 : 24;
    const entryStart = isObject ? 24 : 28;
    const chunks = bytes.readUInt32LE(countOffset);
    if (payloadBytes < 1 || payloadBytes > MAXIMUM_IMAGE_BYTES ||
        chunks < 1 || chunks > 518 ||
        bytes.readUInt32LE(limitOffset) !== MAXIMUM_CHUNK_BYTES ||
        bytes.length !== entryStart + chunks * 12 ||
        (!isObject && entryOffset >= payloadBytes)) {
        Reject(`The ${label} manifest bounds differ.`);
    }
    const entries = [];
    let position = 0;
    for (let index = 0; index < chunks; index += 1) {
        const offset = entryStart + index * 12;
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
    return { bytes: payloadBytes, chunks, entries, entryOffset };
}

async function ReadChunkSet(prefix, manifest, binding, label) {
    const absolutePrefix = path.resolve(prefix);
    const parent = await RequireCanonicalDirectory(
        path.dirname(absolutePrefix),
        `${label} chunk parent`,
    );
    if (!SamePath(path.dirname(absolutePrefix), parent)) {
        Reject(`The ${label} chunk prefix must be canonical.`, 64);
    }
    const chunks = [];
    for (let index = 0; index < manifest.chunks; index += 1) {
        const chunk = await ReadOrdinaryFile(
            `${absolutePrefix}.chunk-${index}`,
            `${label} chunk ${index}`,
            MAXIMUM_CHUNK_BYTES,
        );
        if (chunk.size !== manifest.entries[index].bytes) {
            Reject(`The ${label} chunk ${index} byte length differs.`);
        }
        if (RequiredNumber(binding, `image-chunk-${index}-bytes`, MAXIMUM_CHUNK_BYTES) !== chunk.size ||
            RequiredSha(binding, `image-chunk-${index}-sha256`) !== chunk.sha256) {
            Reject(`The ${label} chunk ${index} binding differs.`);
        }
        chunks.push({ bytes: chunk.size, sha256: chunk.sha256, path: chunk.absolute });
    }
    const trailing = await lstat(`${absolutePrefix}.chunk-${manifest.chunks}`)
        .catch(error => {
            if (error.code === 'ENOENT') return null;
            throw error;
        });
    if (trailing !== null) {
        Reject(`The ${label} chunk set has a trailing chunk.`);
    }
    return chunks;
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
    if (!/^[A-Za-z_.][A-Za-z0-9_.-]*$/.test(name) || !Buffer.from(name, 'ascii').equals(bytes.subarray(offset, offset + length))) {
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

function SectionByName(object, name) {
    return object.sections.find(section => section.name === name);
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
        Reject(`The current publisher role ${role} has no linkage specification.`);
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
    const imports = object.symbols.filter(symbol => symbol.binding === SYMBOL_BINDING_IMPORT);
    const exports = object.symbols.filter(symbol => symbol.binding === SYMBOL_BINDING_EXPORT);
    RequireSymbolSet(imports, spec.imports, 'import', `current publisher role ${role}`);
    RequireSymbolSet(exports, spec.exports, 'export', `current publisher role ${role}`);
    for (const relocation of object.relocations) {
        const section = object.sections[relocation.sectionIndex];
        if (relocation.kind !== RELOCATION_KIND_RELATIVE_I32 ||
            relocation.addend !== -4 ||
            section.name !== '.text') {
            Reject(`The current publisher role ${role} relocation ${relocation.index} differs.`);
        }
    }
    if (!object.symbolsByName.has(spec.entry)) {
        Reject(`The current publisher role ${role} entry symbol is missing.`);
    }
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

function SymbolAddress(symbol, placements) {
    const sectionAddress = placements.sectionAddresses.get(symbol.sectionIndex);
    if (sectionAddress === undefined) {
        Reject(`The symbol ${symbol.name} has no section placement.`);
    }
    return sectionAddress + symbol.offset;
}

function CheckRelativeI32(sourceAddress, targetAddress, addend, label) {
    const displacement = BigInt(targetAddress) - BigInt(sourceAddress) + BigInt(addend);
    if (displacement < -2_147_483_648n || displacement > 2_147_483_647n) {
        Reject(`The ${label} relative-i32 displacement is out of range.`);
    }
}

function BuildTargetPlan(target, objectsByRole, image) {
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

    const objects = new Map([
        [target.startupRole, { object: startup, placements: startupPlacements }],
        [target.adapterRole, { object: adapter, placements: adapterPlacements }],
        [5, { object: sha, placements: shaPlacements }],
        [6, { object: transaction, placements: transactionPlacements }],
    ]);
    const resolvedImports = new Map([
        ['Native_main', { kind: SYMBOL_KIND_FUNCTION, address: IMAGE_BASE_ADDRESS + image.entryOffset }],
    ]);
    for (const role of [target.adapterRole, 5, 6]) {
        const entry = objects.get(role);
        for (const symbol of entry.object.symbols) {
            if (symbol.binding === SYMBOL_BINDING_EXPORT) {
                resolvedImports.set(symbol.name, {
                    kind: symbol.kind,
                    address: SymbolAddress(symbol, entry.placements),
                });
            }
        }
    }

    const importSymbols = [
        ...startup.symbols.filter(symbol => symbol.binding === SYMBOL_BINDING_IMPORT),
        ...adapter.symbols.filter(symbol => symbol.binding === SYMBOL_BINDING_IMPORT),
    ];
    const resolvedImportNames = new Set();
    const deferredImports = [];
    for (const symbol of importSymbols) {
        const resolved = resolvedImports.get(symbol.name);
        if (resolved !== undefined) {
            if (resolved.kind !== symbol.kind || !NATIVE_RESOLVED_IMPORTS.has(symbol.name)) {
                Reject(`The target ${target.name} resolved import ${symbol.name} differs.`);
            }
            resolvedImportNames.add(symbol.name);
        } else {
            if (symbol.name === `${target.name.startsWith('windows') ? 'Linux' : 'Windows'}_wvb_publisher_run`) {
                Reject(`The target ${target.name} imports the wrong startup adapter.`);
            }
            deferredImports.push({ name: symbol.name, kind: symbol.kind });
        }
    }

    let checkedRelocations = 0;
    let deferredRelocations = 0;
    for (const [role, entry] of objects) {
        for (const relocation of entry.object.relocations) {
            const sourceSectionAddress = entry.placements.sectionAddresses.get(relocation.sectionIndex);
            const sourceAddress = sourceSectionAddress + relocation.offset;
            const targetSymbol = entry.object.symbols[relocation.symbolIndex];
            if (targetSymbol.binding === SYMBOL_BINDING_IMPORT) {
                const resolved = resolvedImports.get(targetSymbol.name);
                if (resolved === undefined) {
                    const deferred = deferredImports.some(symbol => symbol.name === targetSymbol.name);
                    if (!deferred) {
                        Reject(`The target ${target.name} relocation imports ${targetSymbol.name} unexpectedly.`);
                    }
                    deferredRelocations += 1;
                    continue;
                }
                CheckRelativeI32(
                    sourceAddress,
                    resolved.address,
                    relocation.addend,
                    `target ${target.name} role ${role} relocation ${relocation.index}`,
                );
            } else {
                CheckRelativeI32(
                    sourceAddress,
                    SymbolAddress(targetSymbol, entry.placements),
                    relocation.addend,
                    `target ${target.name} role ${role} relocation ${relocation.index}`,
                );
            }
            checkedRelocations += 1;
        }
    }

    return {
        name: target.name,
        startupRole: target.startupRole,
        adapterRole: target.adapterRole,
        startupEntry: target.startupEntry,
        startupEntryAddress: SymbolAddress(startup.symbolsByName.get(target.startupEntry), startupPlacements),
        imageAddress: IMAGE_BASE_ADDRESS,
        adapterAddress: adapterPlacements.sectionAddresses.get(0),
        shaTextAddress: shaPlacements.sectionAddresses.get(SectionByName(sha, '.text').index),
        shaReadOnlyAddress: shaPlacements.sectionAddresses.get(SectionByName(sha, '.rodata').index),
        transactionAddress: transactionPlacements.sectionAddresses.get(0),
        endAddress: cursor,
        nativeMainAddress: IMAGE_BASE_ADDRESS + image.entryOffset,
        importsResolved: resolvedImportNames.size,
        importsDeferred: deferredImports.length,
        relocationsChecked: checkedRelocations,
        relocationsDeferred: deferredRelocations,
        deferredImports,
    };
}

function LinkageLines(values, linkageSha256 = null) {
    const lines = [
        LINKAGE_FORMAT,
        `host ${HOST_FAMILY}`,
    ];
    if (linkageSha256 !== null) {
        lines.push(`linkage-sha256 ${linkageSha256}`);
    }
    lines.push(
        `binding-sha256 ${values.binding.sha256}`,
        `binding-host ${RequiredField(values.binding, 'host')}`,
        `image-manifest-bytes ${values.imageManifest.size}`,
        `image-manifest-sha256 ${values.imageManifest.sha256}`,
        `image-bytes ${values.image.bytes}`,
        `image-entry-offset ${values.image.entryOffset}`,
        `image-chunks ${values.image.chunks}`,
    );
    for (const chunk of values.imageChunks.entries()) {
        const [index, value] = chunk;
        lines.push(`image-chunk-${index}-bytes ${value.bytes}`);
        lines.push(`image-chunk-${index}-sha256 ${value.sha256}`);
    }
    for (const object of values.nativeObjects) {
        lines.push(
            `native-object-${object.role} ${object.leaf} ` +
            `bytes ${object.size} sha256 ${object.sha256}`,
        );
    }
    for (const target of values.targets) {
        lines.push(
            `target ${target.name} startup-role ${target.startupRole} adapter-role ${target.adapterRole}`,
            `target ${target.name} startup-entry ${target.startupEntry} address ${target.startupEntryAddress}`,
            `target ${target.name} current-image-address ${target.imageAddress}`,
            `target ${target.name} native-main-address ${target.nativeMainAddress}`,
            `target ${target.name} adapter-address ${target.adapterAddress}`,
            `target ${target.name} sha-text-address ${target.shaTextAddress}`,
            `target ${target.name} sha-read-only-address ${target.shaReadOnlyAddress}`,
            `target ${target.name} transaction-address ${target.transactionAddress}`,
            `target ${target.name} end-address ${target.endAddress}`,
            `target ${target.name} imports-resolved ${target.importsResolved}`,
            `target ${target.name} imports-deferred ${target.importsDeferred}`,
            `target ${target.name} relocations-checked ${target.relocationsChecked}`,
            `target ${target.name} relocations-deferred ${target.relocationsDeferred}`,
        );
        for (const deferred of target.deferredImports) {
            const kind = deferred.kind === SYMBOL_KIND_FUNCTION ? 'function' : 'data';
            lines.push(`target ${target.name} deferred-import ${deferred.name} kind ${kind}`);
        }
    }
    lines.push('');
    return lines;
}

async function Main() {
    if (process.argv.length !== 7) {
        Reject(
            'Usage: node Tools/Native/Plan-Current-Publisher-Linkage.mjs ' +
            '<current-publisher-binding.wvcp> <image-chunk-prefix> <publisher.wvli> ' +
            '<reference-object-directory> <output.wvcl>',
            64,
        );
    }
    const bindingFile = await ReadOrdinaryFile(
        process.argv[2],
        'current publisher binding record',
        MAXIMUM_BINDING_BYTES,
    );
    const binding = ParseBindingRecord(bindingFile);
    const imageManifest = await ReadOrdinaryFile(
        process.argv[4],
        'current publisher WVLI manifest',
        MAXIMUM_MANIFEST_BYTES,
    );
    if (RequiredNumber(binding, 'image-manifest-bytes', MAXIMUM_MANIFEST_BYTES) !== imageManifest.size ||
        RequiredSha(binding, 'image-manifest-sha256') !== imageManifest.sha256) {
        Reject('The current publisher image manifest differs from the binding record.');
    }
    const image = ReadSegmentedManifest(
        imageManifest.bytes,
        'WVLI',
        'current publisher image',
    );
    if (RequiredNumber(binding, 'image-bytes', MAXIMUM_IMAGE_BYTES) !== image.bytes ||
        RequiredNumber(binding, 'image-entry-offset', MAXIMUM_IMAGE_BYTES) !== image.entryOffset ||
        RequiredNumber(binding, 'image-chunks', 518) !== image.chunks) {
        Reject('The current publisher image manifest fields differ from the binding record.');
    }
    const imageChunks = await ReadChunkSet(
        process.argv[3],
        image,
        binding,
        'current publisher image',
    );
    const objectDirectory = await RequireCanonicalDirectory(
        process.argv[5],
        'current publisher reference object directory',
    );
    const nativeObjects = [];
    const objectsByRole = new Map();
    for (const { role, leaf } of NATIVE_OBJECTS) {
        const object = await ReadOrdinaryFile(
            path.join(objectDirectory, leaf),
            `current publisher native object role ${role}`,
            MAXIMUM_CURRENT_OBJECT_BYTES,
        );
        const bound = binding.nativeObjects.get(role);
        if (bound.bytes !== object.size || bound.sha256 !== object.sha256) {
            Reject(`The current publisher native object role ${role} differs from the binding record.`);
        }
        const parsed = ParseWvo(object.bytes, `current publisher native object role ${role}`);
        ValidateRoleObject(role, parsed);
        nativeObjects.push({
            role,
            leaf,
            size: object.size,
            sha256: object.sha256,
            path: object.absolute,
        });
        objectsByRole.set(role, parsed);
    }
    const inputPaths = [
        bindingFile.absolute,
        imageManifest.absolute,
        ...imageChunks.map(chunk => chunk.path),
        ...nativeObjects.map(object => object.path),
    ];
    const output = await RequireOutputPath(process.argv[6], inputPaths);
    const targets = TARGETS.map(target => BuildTargetPlan(target, objectsByRole, image));
    const values = {
        binding,
        imageManifest,
        image,
        imageChunks,
        nativeObjects,
        targets,
    };
    const unsignedRecord = Buffer.from(LinkageLines(values).join('\n'), 'ascii');
    const linkageSha256 = Sha256(unsignedRecord);
    const record = Buffer.from(LinkageLines(values, linkageSha256).join('\n'), 'ascii');
    await writeFile(output, record, { flag: 'wx' });
    process.stdout.write('current publisher linkage status=Valid format=1 targets=2\n');
}

try {
    await Main();
} catch (error) {
    process.stderr.write(`${error.message}\n`);
    process.exitCode = error.exitCode ?? 1;
}
