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
import {
    Createˉwindvaleˉtokensˉprovider,
    WINDVALE_AUTHORITY_VALUES,
    WINDVALE_COMPLETIONS,
    WINDVALE_MODULE_PROFILE_VALUES,
    WINDVALE_TYPE_KEYWORDS,
    WINDVALE_WORD_PATTERN,
} from "./Windvale-Language.js";

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
let Runˉhandler;
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
            { open: "'", close: "'", notIn: ["string", "comment"] },
        ],
        surroundingPairs: [["{", "}"], ["[", "]"], ["(", ")"], ["\"", "\""], ["'", "'"]],
        wordPattern: WINDVALE_WORD_PATTERN,
    });
    Monaco.languages.setMonarchTokensProvider("windvale", Createˉwindvaleˉtokensˉprovider());
    Monaco.languages.registerCompletionItemProvider("windvale", {
        provideCompletionItems(model, position) {
            const Lineˉprefix = model.getLineContent(position.lineNumber).slice(0, position.column - 1);
            const Token = Lineˉprefix.match(/[_\p{XID_Continue}ˉ.]+$/u)?.[0] ?? "";
            const Contextˉprefix = Lineˉprefix.slice(0, Lineˉprefix.length - Token.length);
            const Range = {
                startLineNumber: position.lineNumber,
                endLineNumber: position.lineNumber,
                startColumn: position.column - Token.length,
                endColumn: position.column,
            };

            let Acceptedˉgroups;
            let Acceptedˉlabels;
            if (/\bprofile\s*$/u.test(Contextˉprefix)) {
                Acceptedˉlabels = new Set(WINDVALE_MODULE_PROFILE_VALUES);
            } else if (/\bauthority\s*$/u.test(Contextˉprefix)) {
                Acceptedˉlabels = new Set(WINDVALE_AUTHORITY_VALUES);
            } else if (/\bcapability\s*$/u.test(Contextˉprefix)) {
                Acceptedˉgroups = new Set(["capability"]);
            } else if (/(?::|->)\s*$/u.test(Contextˉprefix)) {
                Acceptedˉlabels = new Set([...WINDVALE_TYPE_KEYWORDS, "borrow"]);
            }

            const Matchingˉcompletions = WINDVALE_COMPLETIONS.filter(Completion =>
                (!Acceptedˉgroups || Acceptedˉgroups.has(Completion.group))
                && (!Acceptedˉlabels || Acceptedˉlabels.has(Completion.label))
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

export function Initialize(editorElement, layoutElement, initialSource, runHandler) {
    Dispose();
    Runˉhandler = typeof runHandler === "function"
        ? runHandler
        : () => runHandler?.invokeMethodAsync("RunWindvaleProgram");
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
        run: () => Runˉhandler?.(),
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
    Runˉhandler = undefined;
    Layoutˉelement = undefined;
}
