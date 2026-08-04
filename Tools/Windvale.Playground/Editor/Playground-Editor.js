import * as Monaco from "monaco-editor/editor/editor.api.js";
import Editorˉworker from "monaco-editor/editor/editor.worker.js?worker";
import "monaco-editor/editor/contrib/bracketMatching/browser/bracketMatching.js";
import "monaco-editor/editor/contrib/clipboard/browser/clipboard.js";
import "monaco-editor/editor/contrib/comment/browser/comment.js";
import "monaco-editor/editor/contrib/contextmenu/browser/contextmenu.js";
import "monaco-editor/editor/contrib/find/browser/findController.js";
import "monaco-editor/editor/contrib/folding/browser/folding.js";
import "monaco-editor/editor/contrib/fontZoom/browser/fontZoom.js";
import "monaco-editor/editor/contrib/gotoError/browser/gotoError.js";
import "monaco-editor/editor/contrib/hover/browser/hoverContribution.js";
import "monaco-editor/editor/contrib/indentation/browser/indentation.js";
import "monaco-editor/editor/contrib/linesOperations/browser/linesOperations.js";
import "monaco-editor/editor/contrib/multicursor/browser/multicursor.js";
import "monaco-editor/editor/contrib/snippet/browser/snippetController2.js";
import "monaco-editor/editor/contrib/suggest/browser/suggestController.js";
import "monaco-editor/editor/contrib/wordHighlighter/browser/wordHighlighter.js";
import "monaco-editor/editor/contrib/wordOperations/browser/wordOperations.js";
import "monaco-editor/editor/contrib/wordPartOperations/browser/wordPartOperations.js";

const LAYOUT_STORAGE_KEY = "windvale-playground-layout";
const THEME_STORAGE_KEY = "windvale-theme";
const DEFAULT_SIDEBAR_WIDTH = 284;
const DEFAULT_RESULTS_HEIGHT = 250;
const MINIMUM_EDITOR_WIDTH = 320;
const MINIMUM_EDITOR_HEIGHT = 220;
const MINIMUM_SIDEBAR_WIDTH = 220;
const MAXIMUM_SIDEBAR_WIDTH = 480;
const MINIMUM_RESULTS_HEIGHT = 150;

let Editorˉinstance;
let Modelˉinstance;
let DotNetˉreference;
let Layoutˉelement;
let Sidebarˉwidth = DEFAULT_SIDEBAR_WIDTH;
let Resultsˉheight = DEFAULT_RESULTS_HEIGHT;
let Languageˉregistered = false;
const Disposables = [];

self.MonacoEnvironment = {
    getWorker() {
        return new Editorˉworker();
    },
};

const WINDVALE_COMPLETIONS = [
    { label: "module", group: "declaration", detail: "Declare this module" },
    { label: "profile", group: "declaration", detail: "Select the module capability profile" },
    { label: "import", group: "declaration", detail: "Import another module" },
    { label: "capability", group: "declaration", detail: "Declare a required hosted capability" },
    { label: "data", group: "declaration", detail: "Declare immutable module data" },
    { label: "record", group: "declaration", detail: "Declare a record type" },
    { label: "enum", group: "declaration", detail: "Declare an enumeration type" },
    { label: "export", group: "declaration", detail: "Export a declaration" },
    { label: "fn", group: "declaration", detail: "Declare a function" },
    { label: "if", group: "control", detail: "Conditional branch" },
    { label: "else", group: "control", detail: "Alternative conditional branch" },
    { label: "while", group: "control", detail: "Repeat while a condition is true" },
    { label: "return", group: "control", detail: "Return from the current function" },
    { label: "let", group: "storage", detail: "Declare an immutable local value" },
    { label: "var", group: "storage", detail: "Declare a mutable local value" },
    { label: "portable", group: "profile", detail: "Portable module profile" },
    { label: "hosted", group: "profile", detail: "Hosted module profile" },
    { label: "system", group: "profile", detail: "System module profile (not executable in this playground)" },
    { label: "i32", group: "type", detail: "Signed 32-bit integer type" },
    { label: "u8", group: "type", detail: "Unsigned 8-bit integer type" },
    { label: "u32", group: "type", detail: "Unsigned 32-bit integer type" },
    { label: "bool", group: "type", detail: "Boolean type" },
    { label: "text", group: "type", detail: "UTF-8 text value type" },
    { label: "bytes", group: "type", detail: "Byte sequence type" },
    { label: "void", group: "type", detail: "No return value" },
    { label: "true", group: "literal", detail: "Boolean true literal" },
    { label: "false", group: "literal", detail: "Boolean false literal" },
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
];

const COMPLETION_GROUP_ORDER = new Map([
    ["declaration", "0"],
    ["control", "1"],
    ["storage", "2"],
    ["profile", "3"],
    ["type", "4"],
    ["literal", "5"],
    ["built-in", "6"],
    ["capability", "7"],
]);

function Clamp(value, minimum, maximum) {
    return Math.min(Math.max(value, minimum), Math.max(minimum, maximum));
}

function Readˉsavedˉlayout() {
    try {
        const Savedˉlayout = JSON.parse(localStorage.getItem(LAYOUT_STORAGE_KEY) ?? "null");
        if (Number.isFinite(Savedˉlayout?.sidebarWidth)) {
            Sidebarˉwidth = Clamp(Savedˉlayout.sidebarWidth, MINIMUM_SIDEBAR_WIDTH, MAXIMUM_SIDEBAR_WIDTH);
        }
        if (Number.isFinite(Savedˉlayout?.resultsHeight)) {
            Resultsˉheight = Math.max(MINIMUM_RESULTS_HEIGHT, Savedˉlayout.resultsHeight);
        }
    } catch {
        // Storage can be unavailable or contain data from an older playground version.
    }
}

function Saveˉlayout() {
    try {
        localStorage.setItem(LAYOUT_STORAGE_KEY, JSON.stringify({
            sidebarWidth: Math.round(Sidebarˉwidth),
            resultsHeight: Math.round(Resultsˉheight),
        }));
    } catch {
        // The layout remains usable for the current visit when storage is unavailable.
    }
}

function Applyˉlayout() {
    if (!Layoutˉelement) {
        return;
    }

    const Workspaceˉbounds = Layoutˉelement.getBoundingClientRect();
    const Maximumˉsidebar = Math.min(
        MAXIMUM_SIDEBAR_WIDTH,
        Math.max(MINIMUM_SIDEBAR_WIDTH, Workspaceˉbounds.width - MINIMUM_EDITOR_WIDTH),
    );
    const Maximumˉresults = Math.max(
        MINIMUM_RESULTS_HEIGHT,
        Workspaceˉbounds.height - MINIMUM_EDITOR_HEIGHT,
    );

    Sidebarˉwidth = Clamp(Sidebarˉwidth, MINIMUM_SIDEBAR_WIDTH, Maximumˉsidebar);
    Resultsˉheight = Clamp(Resultsˉheight, MINIMUM_RESULTS_HEIGHT, Maximumˉresults);
    Layoutˉelement.style.setProperty("--sidebar-width", `${Sidebarˉwidth}px`);
    Layoutˉelement.style.setProperty("--results-height", `${Resultsˉheight}px`);
    Editorˉinstance?.layout();
}

function Resetˉlayout(direction) {
    if (direction === "vertical") {
        Sidebarˉwidth = DEFAULT_SIDEBAR_WIDTH;
    } else {
        Resultsˉheight = DEFAULT_RESULTS_HEIGHT;
    }
    Applyˉlayout();
    Saveˉlayout();
}

function Resizeˉfromˉpointer(direction, event) {
    if (direction === "vertical") {
        const Topˉarea = Layoutˉelement.querySelector(".ide-top").getBoundingClientRect();
        Sidebarˉwidth = Topˉarea.right - event.clientX;
    } else {
        const Workspaceˉbounds = Layoutˉelement.getBoundingClientRect();
        Resultsˉheight = Workspaceˉbounds.bottom - event.clientY;
    }
    Applyˉlayout();
}

function Bindˉseparator(separator) {
    const Direction = separator.dataset.resize;
    const Pointerˉmove = event => Resizeˉfromˉpointer(Direction, event);
    const Pointerˉup = event => {
        separator.releasePointerCapture?.(event.pointerId);
        separator.classList.remove("dragging");
        window.removeEventListener("pointermove", Pointerˉmove);
        window.removeEventListener("pointerup", Pointerˉup);
        Saveˉlayout();
    };

    separator.addEventListener("pointerdown", event => {
        event.preventDefault();
        separator.setPointerCapture?.(event.pointerId);
        separator.classList.add("dragging");
        window.addEventListener("pointermove", Pointerˉmove);
        window.addEventListener("pointerup", Pointerˉup);
    });
    separator.addEventListener("dblclick", () => Resetˉlayout(Direction));
    separator.addEventListener("keydown", event => {
        const Isˉvertical = Direction === "vertical";
        const Step = event.shiftKey ? 40 : 10;

        if (Isˉvertical && (event.key === "ArrowLeft" || event.key === "ArrowRight")) {
            Sidebarˉwidth += event.key === "ArrowLeft" ? Step : -Step;
        } else if (!Isˉvertical && (event.key === "ArrowUp" || event.key === "ArrowDown")) {
            Resultsˉheight += event.key === "ArrowUp" ? Step : -Step;
        } else if (event.key === "Home") {
            Resetˉlayout(Direction);
            event.preventDefault();
            return;
        } else {
            return;
        }

        event.preventDefault();
        Applyˉlayout();
        Saveˉlayout();
    });
}

function Registerˉwindvaleˉlanguage() {
    if (Languageˉregistered) {
        return;
    }
    Languageˉregistered = true;

    Monaco.languages.register({ id: "windvale", extensions: [".wv"] });
    Monaco.languages.setLanguageConfiguration("windvale", {
        comments: { lineComment: "//" },
        brackets: [["{", "}"], ["[", "]"], ["(", ")"]],
        autoClosingPairs: [
            { open: "{", close: "}" },
            { open: "[", close: "]" },
            { open: "(", close: ")" },
            { open: "\"", close: "\"", notIn: ["string", "comment"] },
        ],
        surroundingPairs: [["{", "}"], ["[", "]"], ["(", ")"], ["\"", "\""]],
    });
    Monaco.languages.setMonarchTokensProvider("windvale", {
        defaultToken: "",
        tokenizer: {
            root: [
                [/\/\/.*$/, "comment"],
                [/"/, { token: "string.quote", bracket: "@open", next: "@string" }],
                [/\b(?:module|profile|import|capability|data|record|enum|export|fn)\b/, "keyword.declaration"],
                [/\b(?:if|else|while|return)\b/, "keyword.control"],
                [/\b(?:let|var)\b/, "keyword.storage"],
                [/\b(?:portable|hosted|system)\b/, "keyword.profile"],
                [/\b(?:i32|u8|u32|bool|text|bytes|void)\b/, "type"],
                [/\b(?:true|false)\b/, "constant.language"],
                [/\blength\b/, "support.function"],
                [/[a-z_][a-z0-9_]*(?:\.[a-z_][a-z0-9_]*)+(?=\s*\()/, "function.capability"],
                [/[A-Za-z_][A-Za-z0-9_ˉ]*(?=\s*\()/, "function"],
                [/\d+(?:u8|u32)?\b/, "number"],
                [/->|==|!=|<=|>=|[+\-*!<>=]/, "operator"],
                [/[{}()[\]]/, "@brackets"],
                [/[;:,]/, "delimiter"],
                [/[A-Za-z_][A-Za-z0-9_ˉ]*/, "identifier"],
            ],
            string: [
                [/[^\\"]+/, "string"],
                [/\\(?:["\\nrt]|u[0-9A-Fa-f]{4})/, "string.escape"],
                [/\\./, "string.escape.invalid"],
                [/"/, { token: "string.quote", bracket: "@close", next: "@pop" }],
            ],
        },
    });
    Monaco.languages.registerCompletionItemProvider("windvale", {
        provideCompletionItems(model, position) {
            const Lineˉprefix = model.getLineContent(position.lineNumber).slice(0, position.column - 1);
            const Token = Lineˉprefix.match(/[A-Za-z_][A-Za-z0-9_ˉ.]*$/u)?.[0] ?? "";
            const Contextˉprefix = Lineˉprefix.slice(0, Lineˉprefix.length - Token.length);
            const Range = {
                startLineNumber: position.lineNumber,
                endLineNumber: position.lineNumber,
                startColumn: position.column - Token.length,
                endColumn: position.column,
            };

            let Acceptedˉgroups;
            if (/\bprofile\s*$/u.test(Contextˉprefix)) {
                Acceptedˉgroups = new Set(["profile"]);
            } else if (/\bcapability\s*$/u.test(Contextˉprefix)) {
                Acceptedˉgroups = new Set(["capability"]);
            } else if (/(?::|->)\s*$/u.test(Contextˉprefix)) {
                Acceptedˉgroups = new Set(["type"]);
            }

            const Matchingˉcompletions = WINDVALE_COMPLETIONS.filter(Completion =>
                (!Acceptedˉgroups || Acceptedˉgroups.has(Completion.group))
                && Completion.label.startsWith(Token),
            );

            return {
                suggestions: Matchingˉcompletions.map(Completion => ({
                    label: Completion.label,
                    kind: Completion.group === "capability" || Completion.group === "built-in"
                        ? Monaco.languages.CompletionItemKind.Function
                        : Monaco.languages.CompletionItemKind.Keyword,
                    insertText: Completion.label,
                    filterText: Completion.label,
                    sortText: `${COMPLETION_GROUP_ORDER.get(Completion.group)}_${Completion.label}`,
                    detail: `Windvale · ${Completion.detail}`,
                    documentation: Completion.detail,
                    range: Range,
                })),
            };
        },
    });
}

function Registerˉthemes() {
    Monaco.editor.defineTheme("windvale-light", {
        base: "vs",
        inherit: true,
        rules: [
            { token: "comment", foreground: "718096", fontStyle: "italic" },
            { token: "keyword.declaration", foreground: "175CD3", fontStyle: "bold" },
            { token: "keyword.control", foreground: "7C3AED", fontStyle: "bold" },
            { token: "keyword.storage", foreground: "7C3AED" },
            { token: "keyword.profile", foreground: "B54708", fontStyle: "bold" },
            { token: "type", foreground: "0E7490" },
            { token: "constant.language", foreground: "B42318" },
            { token: "function", foreground: "146C43" },
            { token: "function.capability", foreground: "0E7490", fontStyle: "bold" },
            { token: "support.function", foreground: "146C43" },
            { token: "string", foreground: "9A3412" },
            { token: "number", foreground: "B42318" },
        ],
        colors: {
            "editor.background": "#FFFFFF",
            "editor.foreground": "#172033",
            "editorLineNumber.foreground": "#98A2B3",
            "editorLineNumber.activeForeground": "#175CD3",
            "editorCursor.foreground": "#175CD3",
            "editor.selectionBackground": "#D9E8FF",
            "editor.inactiveSelectionBackground": "#EAF2FF",
            "editor.lineHighlightBackground": "#F7F9FC",
            "editorIndentGuide.background1": "#E4E7EC",
            "editorIndentGuide.activeBackground1": "#98A2B3",
            "editorGutter.background": "#FFFFFF",
            "editorError.foreground": "#D92D20",
            "editorOverviewRuler.border": "#E4E7EC",
        },
    });
    Monaco.editor.defineTheme("windvale-dark", {
        base: "vs-dark",
        inherit: true,
        rules: [
            { token: "comment", foreground: "7D91A8", fontStyle: "italic" },
            { token: "keyword.declaration", foreground: "71A7FF", fontStyle: "bold" },
            { token: "keyword.control", foreground: "C59BFF", fontStyle: "bold" },
            { token: "keyword.storage", foreground: "C59BFF" },
            { token: "keyword.profile", foreground: "FFB86B", fontStyle: "bold" },
            { token: "type", foreground: "66D9D0" },
            { token: "constant.language", foreground: "FF8D85" },
            { token: "function", foreground: "8ED9A7" },
            { token: "function.capability", foreground: "66D9D0", fontStyle: "bold" },
            { token: "support.function", foreground: "8ED9A7" },
            { token: "string", foreground: "F5B97D" },
            { token: "number", foreground: "FF8D85" },
        ],
        colors: {
            "editor.background": "#0D1520",
            "editor.foreground": "#D8E1ED",
            "editorLineNumber.foreground": "#52657A",
            "editorLineNumber.activeForeground": "#8EB8FF",
            "editorCursor.foreground": "#8EB8FF",
            "editor.selectionBackground": "#1F4778",
            "editor.inactiveSelectionBackground": "#183556",
            "editor.lineHighlightBackground": "#111D2A",
            "editorIndentGuide.background1": "#223245",
            "editorIndentGuide.activeBackground1": "#52657A",
            "editorGutter.background": "#0D1520",
            "editorError.foreground": "#FF7066",
            "editorOverviewRuler.border": "#223245",
        },
    });
}

function Updateˉsourceˉstatus() {
    const Characterˉstatus = Layoutˉelement?.querySelector("[data-status-characters]");
    if (Characterˉstatus && Modelˉinstance) {
        Characterˉstatus.textContent = `${Modelˉinstance.getValueLength().toLocaleString()} chars`;
    }
}

function Updateˉcursorˉstatus(position) {
    const Cursorˉstatus = Layoutˉelement?.querySelector("[data-status-cursor]");
    if (Cursorˉstatus) {
        Cursorˉstatus.textContent = `Ln ${position.lineNumber}, Col ${position.column}`;
    }
}

function Setˉtheme(theme) {
    const Theme = theme === "dark" ? "dark" : "light";
    document.documentElement.dataset.theme = Theme;
    document.documentElement.style.colorScheme = Theme;
    Monaco.editor.setTheme(`windvale-${Theme}`);

    const Themeˉmeta = document.querySelector('meta[name="theme-color"]');
    Themeˉmeta?.setAttribute("content", Theme === "dark" ? "#08111c" : "#f5f7fb");
    try {
        localStorage.setItem(THEME_STORAGE_KEY, Theme);
    } catch {
        // The chosen theme still applies for the current visit.
    }
    return Theme;
}

export function Initialize(editorElement, layoutElement, initialSource, dotNetReference) {
    Dispose();
    DotNetˉreference = dotNetReference;
    Layoutˉelement = layoutElement;
    Readˉsavedˉlayout();
    Registerˉwindvaleˉlanguage();
    Registerˉthemes();

    const Theme = document.documentElement.dataset.theme === "dark" ? "dark" : "light";
    Modelˉinstance = Monaco.editor.createModel(
        initialSource,
        "windvale",
        Monaco.Uri.parse("inmemory://windvale/playground.wv"),
    );
    Editorˉinstance = Monaco.editor.create(editorElement, {
        model: Modelˉinstance,
        theme: `windvale-${Theme}`,
        automaticLayout: true,
        ariaLabel: "Windvale source editor",
        fontFamily: '"Cascadia Code", "SFMono-Regular", Consolas, monospace',
        fontSize: 13,
        lineHeight: 21,
        tabSize: 4,
        insertSpaces: true,
        minimap: { enabled: false },
        glyphMargin: true,
        folding: true,
        lineNumbersMinChars: 3,
        padding: { top: 12, bottom: 12 },
        renderWhitespace: "selection",
        scrollBeyondLastLine: false,
        smoothScrolling: true,
        wordWrap: "off",
        wordBasedSuggestions: "off",
        quickSuggestions: { other: true, comments: false, strings: false },
        acceptSuggestionOnEnter: "smart",
        bracketPairColorization: { enabled: true },
        guides: { bracketPairs: true, indentation: true },
        overviewRulerBorder: false,
        fixedOverflowWidgets: true,
    });

    Editorˉinstance.addAction({
        id: "windvale.compile-and-run",
        label: "Compile and Run",
        keybindings: [Monaco.KeyMod.CtrlCmd | Monaco.KeyCode.Enter],
        run: () => DotNetˉreference?.invokeMethodAsync("RunWindvaleProgram"),
    });
    Editorˉinstance.addAction({
        id: "windvale.insert-macron-separator",
        label: "Insert Windvale Macron Separator (ˉ)",
        keybindings: [
            Monaco.KeyMod.CtrlCmd | Monaco.KeyCode.KeyM,
            Monaco.KeyMod.CtrlCmd | Monaco.KeyCode.Semicolon,
        ],
        run: Editor => Editor.trigger("windvale", "type", { text: "ˉ" }),
    });
    Disposables.push(Editorˉinstance.onDidChangeModelContent(Updateˉsourceˉstatus));
    Disposables.push(Editorˉinstance.onDidChangeCursorPosition(event => Updateˉcursorˉstatus(event.position)));

    Layoutˉelement.querySelectorAll("[data-resize]").forEach(Bindˉseparator);
    const Windowˉresize = () => Applyˉlayout();
    window.addEventListener("resize", Windowˉresize);
    Disposables.push({ dispose: () => window.removeEventListener("resize", Windowˉresize) });

    Applyˉlayout();
    Updateˉsourceˉstatus();
    Updateˉcursorˉstatus(Editorˉinstance.getPosition());
    Layoutˉelement.classList.add("editor-ready");
    editorElement.querySelector("textarea")?.setAttribute("data-gramm", "false");
    Editorˉinstance.focus();
    return Theme;
}

export function GetValue() {
    return Modelˉinstance?.getValue() ?? "";
}

export function SetValue(source) {
    if (!Modelˉinstance) {
        return;
    }
    Modelˉinstance.setValue(source);
    Monaco.editor.setModelMarkers(Modelˉinstance, "windvale", []);
    Editorˉinstance?.setPosition({ lineNumber: 1, column: 1 });
    Editorˉinstance?.revealPosition({ lineNumber: 1, column: 1 });
    Editorˉinstance?.focus();
}

export function SetDiagnostics(diagnostics) {
    if (!Modelˉinstance) {
        return;
    }

    const Maximumˉline = Modelˉinstance.getLineCount();
    const Markers = (diagnostics ?? []).map(Diagnostic => {
        const Line = Clamp(Diagnostic.startLineNumber ?? 1, 1, Maximumˉline);
        const Maximumˉcolumn = Modelˉinstance.getLineMaxColumn(Line);
        const Column = Clamp(Diagnostic.startColumn ?? 1, 1, Maximumˉcolumn);
        return {
            severity: Monaco.MarkerSeverity.Error,
            startLineNumber: Line,
            startColumn: Column,
            endLineNumber: Line,
            endColumn: Math.min(Column + 1, Maximumˉcolumn),
            message: Diagnostic.message,
            code: Diagnostic.code,
            source: Diagnostic.phase ? `Windvale ${Diagnostic.phase}` : "Windvale",
        };
    });
    Monaco.editor.setModelMarkers(Modelˉinstance, "windvale", Markers);
}

export function RevealPosition(line, column) {
    if (!Editorˉinstance || !Modelˉinstance) {
        return;
    }
    const Line = Clamp(line ?? 1, 1, Modelˉinstance.getLineCount());
    const Column = Clamp(column ?? 1, 1, Modelˉinstance.getLineMaxColumn(Line));
    Editorˉinstance.setPosition({ lineNumber: Line, column: Column });
    Editorˉinstance.revealPositionInCenter({ lineNumber: Line, column: Column });
    Editorˉinstance.focus();
}

export function ToggleTheme() {
    return Setˉtheme(document.documentElement.dataset.theme === "dark" ? "light" : "dark");
}

export function Dispose() {
    for (const Disposable of Disposables.splice(0)) {
        Disposable.dispose();
    }
    Editorˉinstance?.dispose();
    Modelˉinstance?.dispose();
    Editorˉinstance = undefined;
    Modelˉinstance = undefined;
    DotNetˉreference = undefined;
    Layoutˉelement = undefined;
}
