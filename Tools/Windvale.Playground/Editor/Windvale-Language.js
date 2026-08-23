const IDENTIFIER_START_SOURCE = String.raw`(?:(?!ˉ)[_\p{XID_Start}])`;
const IDENTIFIER_CONTINUE_SOURCE = String.raw`(?:(?!ˉ)[_\p{XID_Continue}])`;
const IDENTIFIER_SEGMENT_SOURCE = String.raw`${IDENTIFIER_START_SOURCE}${IDENTIFIER_CONTINUE_SOURCE}*`;
const IDENTIFIER_SOURCE = String.raw`${IDENTIFIER_SEGMENT_SOURCE}(?:ˉ${IDENTIFIER_SEGMENT_SOURCE})*`;
const IDENTIFIER_CONTINUATION_SOURCE = String.raw`[_\p{XID_Continue}ˉ]`;
const DECIMAL_DIGITS_SOURCE = String.raw`[0-9](?:_?[0-9])*`;
const HEX_DIGITS_SOURCE = String.raw`[0-9A-Fa-f](?:_?[0-9A-Fa-f])*`;
const BINARY_DIGITS_SOURCE = String.raw`[01](?:_?[01])*`;
const INTEGER_SUFFIX_SOURCE = String.raw`(?:i8|i16|i32|i64|u8|u16|u32|u64)`;
const FLOAT_SUFFIX_SOURCE = String.raw`(?:f32|f64)`;
const DECIMAL_EXPONENT_SOURCE = String.raw`[eE][+-]?${DECIMAL_DIGITS_SOURCE}`;
const BINARY_EXPONENT_SOURCE = String.raw`[pP][+-]?${DECIMAL_DIGITS_SOURCE}`;

export const WINDVALE_DECLARATION_KEYWORDS = Object.freeze([
    "module", "profile", "platform", "authority", "import", "as",
    "requires", "optional", "capability", "version", "data", "const",
    "record", "enum", "variant", "protocol", "implement", "derive",
    "package", "foreign", "export", "fn", "where", "maximum",
]);

export const WINDVALE_CONTROL_KEYWORDS = Object.freeze([
    "if", "else", "while", "for", "in", "match", "case", "try", "await",
    "using", "task", "scope", "policy", "join", "cancel_join", "fail_join",
    "break", "continue", "return",
]);

export const WINDVALE_STORAGE_KEYWORDS = Object.freeze([
    "let", "var", "borrow", "mut", "copy", "move", "base", "unsafe",
    "async", "effects",
    // These remain part of the currently executable source subset.
    "freeze", "push",
]);

export const WINDVALE_MODULE_PROFILE_VALUES = Object.freeze([
    "core", "hosted", "system",
    // The browser compiler still executes the pre-1.0 portable profile.
    "portable",
]);

export const WINDVALE_AUTHORITY_VALUES = Object.freeze([
    "application", "library", "service", "system",
]);

export const WINDVALE_PROFILE_KEYWORDS = Object.freeze([
    ...new Set([...WINDVALE_MODULE_PROFILE_VALUES, ...WINDVALE_AUTHORITY_VALUES]),
]);

export const WINDVALE_TYPE_KEYWORDS = Object.freeze([
    "i8", "i16", "i32", "i64", "u8", "u16", "u32", "u64", "f32", "f64",
    "rune", "bool", "text", "bytes", "unit", "never",
    // Compatibility types accepted by the current compiler surface.
    "sequence", "builder", "void",
]);

export const WINDVALE_LITERAL_KEYWORDS = Object.freeze(["true", "false"]);

export const WINDVALE_LANGUAGE_1_RESERVED_WORDS = Object.freeze([
    "application", "as", "async", "authority", "await", "base", "bool",
    "borrow", "break", "bytes", "cancel_join", "capability", "case", "const",
    "continue", "copy", "core", "data", "derive", "effects", "else", "enum",
    "export", "f32", "f64", "fail_join", "false", "fn", "for", "foreign",
    "hosted", "i8", "i16", "i32", "i64", "if", "implement", "import", "in",
    "join", "let", "library", "match", "module", "move", "mut", "never",
    "optional", "maximum", "package", "platform", "policy", "profile",
    "protocol", "record", "requires", "return", "rune", "scope", "service",
    "system", "task", "text", "true", "try", "u8", "u16", "u32", "u64",
    "unit", "unsafe", "using", "var", "variant", "version", "where",
]);

export const WINDVALE_OPERATORS = Object.freeze([
    "->", "&&", "||", "<<", ">>", "==", "!=", "<=", ">=", "+=", "-=",
    "*=", "/=", "%=", "+", "-", "*", "/", "%", "&", "|", "^", "~",
    "!", "<", ">", "=",
]);

export const WINDVALE_IDENTIFIER_PATTERN = new RegExp(IDENTIFIER_SOURCE, "u");
export const WINDVALE_WORD_PATTERN = new RegExp(
    `(${IDENTIFIER_SOURCE})|(${DECIMAL_DIGITS_SOURCE}(?:${INTEGER_SUFFIX_SOURCE}|${FLOAT_SUFFIX_SOURCE})?)`,
    "u",
);

const WINDVALE_HEX_FLOAT_PATTERN = new RegExp(
    `0x${HEX_DIGITS_SOURCE}(?:\\.${HEX_DIGITS_SOURCE})?${BINARY_EXPONENT_SOURCE}(?:${FLOAT_SUFFIX_SOURCE})?(?!${IDENTIFIER_CONTINUATION_SOURCE})`,
    "u",
);
const WINDVALE_DECIMAL_FLOAT_PATTERN = new RegExp(
    `${DECIMAL_DIGITS_SOURCE}(?:\\.${DECIMAL_DIGITS_SOURCE}(?:${DECIMAL_EXPONENT_SOURCE})?|${DECIMAL_EXPONENT_SOURCE})(?:${FLOAT_SUFFIX_SOURCE})?(?!${IDENTIFIER_CONTINUATION_SOURCE})`,
    "u",
);
const WINDVALE_INTEGER_PATTERN = new RegExp(
    `(?:0x${HEX_DIGITS_SOURCE}|0b${BINARY_DIGITS_SOURCE}|${DECIMAL_DIGITS_SOURCE})(?:${INTEGER_SUFFIX_SOURCE})?(?!${IDENTIFIER_CONTINUATION_SOURCE})`,
    "u",
);

export const WINDVALE_NUMBER_PATTERNS = Object.freeze([
    WINDVALE_HEX_FLOAT_PATTERN,
    WINDVALE_DECIMAL_FLOAT_PATTERN,
    WINDVALE_INTEGER_PATTERN,
]);

const TEXT_ESCAPE_PATTERN = /\\(?:[\\'"nrt0{}]|u\{[0-9A-Fa-f]{1,6}\})/u;
const BYTE_ESCAPE_PATTERN = /\\(?:[\\'"nrt0{}]|x[0-9A-Fa-f]{2})/u;
const CAPABILITY_CALL_PATTERN = /[a-z][a-z0-9_]*(?:\.[a-z][a-z0-9_]*)+(?=\s*\()/u;
const FUNCTION_CALL_PATTERN = new RegExp(`${IDENTIFIER_SOURCE}(?=\\s*\\()`, "u");
const IDENTIFIER_PATTERN = new RegExp(IDENTIFIER_SOURCE, "u");
const LENGTH_PATTERN = new RegExp(`length(?!${IDENTIFIER_CONTINUATION_SOURCE})`, "u");

const COMPLETION_GROUPS = Object.freeze([
    ["declaration", WINDVALE_DECLARATION_KEYWORDS, "Windvale declaration or header keyword"],
    ["control", WINDVALE_CONTROL_KEYWORDS, "Windvale control-flow keyword"],
    ["storage", WINDVALE_STORAGE_KEYWORDS, "Windvale ownership or storage keyword"],
    ["profile", WINDVALE_PROFILE_KEYWORDS, "Windvale profile or authority value"],
    ["type", WINDVALE_TYPE_KEYWORDS, "Windvale type keyword"],
    ["literal", WINDVALE_LITERAL_KEYWORDS, "Windvale Boolean literal"],
]);

const BUILT_IN_COMPLETIONS = Object.freeze([
    { label: "length", group: "built-in", detail: "Get the length of a supported value" },
    { label: "Bytesˉlength", group: "built-in", detail: "Get an immutable byte sequence length" },
    { label: "Bytesˉslice", group: "built-in", detail: "Create an immutable byte slice" },
    { label: "Bytesˉreadˉu8", group: "built-in", detail: "Read one unsigned byte" },
    { label: "Bytesˉreadˉu16ˉlittle", group: "built-in", detail: "Read an unsigned 16-bit little-endian value" },
    { label: "Bytesˉreadˉu32ˉlittle", group: "built-in", detail: "Read an unsigned 32-bit little-endian value" },
    { label: "Bytesˉreadˉi32ˉlittle", group: "built-in", detail: "Read a signed 32-bit little-endian value" },
    { label: "Bytesˉconcat", group: "built-in", detail: "Concatenate two immutable byte sequences" },
    { label: "Bytesˉfromˉu8", group: "built-in", detail: "Encode one unsigned byte" },
    { label: "Bytesˉfromˉu16ˉlittle", group: "built-in", detail: "Encode an unsigned 16-bit little-endian value" },
    { label: "Bytesˉfromˉu32ˉlittle", group: "built-in", detail: "Encode an unsigned 32-bit little-endian value" },
    { label: "Bytesˉfromˉi32ˉlittle", group: "built-in", detail: "Encode a signed 32-bit little-endian value" },
    { label: "Bytesˉsha256ˉhex", group: "built-in", detail: "Hash bytes as lowercase SHA-256 text" },
    { label: "Textˉtoˉutf8", group: "built-in", detail: "Encode text as strict UTF-8 bytes" },
    { label: "Textˉutf8ˉisˉvalid", group: "built-in", detail: "Check whether bytes are strict UTF-8" },
    { label: "Textˉfromˉutf8", group: "built-in", detail: "Decode strict UTF-8 bytes as text" },
    { label: "Textˉconcat", group: "built-in", detail: "Concatenate two text values" },
    { label: "Textˉquote", group: "built-in", detail: "Format text as a quoted Windvale literal" },
    { label: "I32ˉformat", group: "built-in", detail: "Format a signed integer as text" },
    { label: "U8ˉformat", group: "built-in", detail: "Format an unsigned byte as text" },
    { label: "U32ˉformat", group: "built-in", detail: "Format an unsigned integer as text" },
    { label: "U32ˉfromˉu8", group: "built-in", detail: "Widen an unsigned byte to u32" },
    { label: "U64ˉfromˉu32", group: "built-in", detail: "Widen an unsigned u32 to u64" },
    { label: "Enumˉname", group: "built-in", detail: "Get the declared name of an enum value" },
    { label: "console.write", group: "capability", detail: "Write text without a newline" },
    { label: "console.write_line", group: "capability", detail: "Write one line of text" },
    { label: "diagnostic.write_line", group: "capability", detail: "Write one line to the diagnostic channel" },
]);

export const WINDVALE_COMPLETIONS = Object.freeze([
    ...COMPLETION_GROUPS.flatMap(([Group, Labels, Detail]) =>
        Labels.map(Label => ({ label: Label, group: Group, detail: Detail }))),
    ...BUILT_IN_COMPLETIONS,
]);

function Addˉrawˉliteralˉstates(tokenizer) {
    for (let Hashˉcount = 0; Hashˉcount <= 8; Hashˉcount += 1) {
        const Hashes = "#".repeat(Hashˉcount);
        const Closingˉpattern = new RegExp(`"${Hashes}`);
        const Textˉstate = `rawText${Hashˉcount}`;
        const Byteˉstate = `rawBytes${Hashˉcount}`;
        tokenizer.root.push([
            new RegExp(`r${Hashes}"`),
            { token: "string.quote", bracket: "@open", next: `@${Textˉstate}` },
        ]);
        tokenizer.root.push([
            new RegExp(`br${Hashes}"`),
            { token: "string.quote", bracket: "@open", next: `@${Byteˉstate}` },
        ]);
        tokenizer[Textˉstate] = [
            [Closingˉpattern, { token: "string.quote", bracket: "@close", next: "@pop" }],
            [/[^\"]+/, "string.raw"],
            [/\"/, "string.raw"],
        ];
        tokenizer[Byteˉstate] = [
            [Closingˉpattern, { token: "string.quote", bracket: "@close", next: "@pop" }],
            [/[^\"]+/, "string.byte.raw"],
            [/\"/, "string.byte.raw"],
        ];
    }
}

export function Createˉwindvaleˉtokensˉprovider() {
    const Tokenizer = {
        root: [
            [/^#!wv\/1 [A-Za-z][A-Za-z0-9]*(?:-[A-Za-z][A-Za-z0-9]*)*(?:\.[A-Za-z][A-Za-z0-9]*(?:-[A-Za-z][A-Za-z0-9]*)*)*@[1-9][0-9]*$/, "meta.directive"],
            [/\/\/\/.*$/, "comment.doc"],
            [/\/\/.*$/, "comment"],
        ],
        multilineText: [
            [/\"\"\"/, { token: "string.quote", bracket: "@close", next: "@pop" }],
            [TEXT_ESCAPE_PATTERN, "string.escape"],
            [/\\./, "string.escape.invalid"],
            [/[^\\\"]+/, "string"],
            [/\"/, "string"],
        ],
        multilineBytes: [
            [/\"\"\"/, { token: "string.quote", bracket: "@close", next: "@pop" }],
            [BYTE_ESCAPE_PATTERN, "string.escape"],
            [/\\./, "string.escape.invalid"],
            [/[^\\\"]+/, "string.byte"],
            [/\"/, "string.byte"],
        ],
        text: [
            [TEXT_ESCAPE_PATTERN, "string.escape"],
            [/\\./, "string.escape.invalid"],
            [/[^\\\"\r\n]+/, "string"],
            [/\"/, { token: "string.quote", bracket: "@close", next: "@pop" }],
        ],
        bytes: [
            [BYTE_ESCAPE_PATTERN, "string.escape"],
            [/\\./, "string.escape.invalid"],
            [/[^\\\"\r\n]+/, "string.byte"],
            [/\"/, { token: "string.quote", bracket: "@close", next: "@pop" }],
        ],
        rune: [
            [TEXT_ESCAPE_PATTERN, "string.escape"],
            [/\\./, "string.escape.invalid"],
            [/[^\\'\r\n]/u, "string.rune"],
            [/'/, { token: "string.quote", bracket: "@close", next: "@pop" }],
        ],
    };

    Addˉrawˉliteralˉstates(Tokenizer);
    Tokenizer.root.push(
        [/b\"\"\"/, { token: "string.quote", bracket: "@open", next: "@multilineBytes" }],
        [/\"\"\"/, { token: "string.quote", bracket: "@open", next: "@multilineText" }],
        [/b\"/, { token: "string.quote", bracket: "@open", next: "@bytes" }],
        [/\"/, { token: "string.quote", bracket: "@open", next: "@text" }],
        [/'/, { token: "string.quote", bracket: "@open", next: "@rune" }],
        [WINDVALE_HEX_FLOAT_PATTERN, "number.float"],
        [WINDVALE_DECIMAL_FLOAT_PATTERN, "number.float"],
        [WINDVALE_INTEGER_PATTERN, "number"],
        [LENGTH_PATTERN, "support.function"],
        [CAPABILITY_CALL_PATTERN, "function.capability"],
        [FUNCTION_CALL_PATTERN, "function"],
        [IDENTIFIER_PATTERN, {
            cases: {
                "@declarationKeywords": "keyword.declaration",
                "@controlKeywords": "keyword.control",
                "@storageKeywords": "keyword.storage",
                "@profileKeywords": "keyword.profile",
                "@typeKeywords": "type",
                "@literalKeywords": "constant.language",
                "@default": "identifier",
            },
        }],
        [/::|->|&&|\|\||<<|>>|==|!=|<=|>=|\+=|-=|\*=|\/=|%=|[+\-*/%&|^~!<>=]/, "operator"],
        [/[{}()[\]]/, "@brackets"],
        [/[;:,.]/, "delimiter"],
    );

    return {
        defaultToken: "",
        declarationKeywords: [...WINDVALE_DECLARATION_KEYWORDS],
        controlKeywords: [...WINDVALE_CONTROL_KEYWORDS],
        storageKeywords: [...WINDVALE_STORAGE_KEYWORDS],
        profileKeywords: [...WINDVALE_PROFILE_KEYWORDS],
        typeKeywords: [...WINDVALE_TYPE_KEYWORDS],
        literalKeywords: [...WINDVALE_LITERAL_KEYWORDS],
        tokenizer: Tokenizer,
    };
}

export function Isˉwindvaleˉnumber(value) {
    return WINDVALE_NUMBER_PATTERNS.some(Pattern => {
        const Match = Pattern.exec(value);
        return Match?.index === 0 && Match[0].length === value.length;
    });
}
