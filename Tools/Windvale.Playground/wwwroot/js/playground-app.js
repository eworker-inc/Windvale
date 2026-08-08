import * as Editor from "../editor/playground-editor.js";
import { Compileˉandˉrun } from "./windvale-compiler-host.js";
import { EXAMPLES, SCRATCH_SOURCE } from "./playground-examples.js";

const MAXIMUM_SOURCE_TABS = 12;
const COMPILER_TIMEOUT_MILLISECONDS = 300_000;

const Shell = document.getElementById("playground-shell");
const Editorˉelement = document.getElementById("monaco-editor");
const Fallbackˉeditor = document.getElementById("fallback-source-editor");
const Sourceˉtabsˉelement = document.getElementById("source-tabs");
const Exampleˉpicker = document.getElementById("example-picker");
const Exampleˉdescription = document.getElementById("example-description");
const Runˉbutton = document.getElementById("run-program");
const Budgetˉpicker = document.getElementById("instruction-budget");
const Resultˉstatus = document.getElementById("result-status");
const Diagnosticˉcount = document.getElementById("diagnostic-count");
const Statusˉdiagnostics = document.getElementById("status-diagnostics");
const Runˉstate = document.getElementById("run-state");
const Errorˉui = document.getElementById("playground-error-ui");
const Resultˉviews = new Map(Array.from(
    document.querySelectorAll("[data-result-view]"),
    Element => [Element.dataset.resultView, Element],
));

let Sourceˉtabs = [{
    Id: 1,
    Fileˉname: EXAMPLES[0].Fileˉname,
    Description: EXAMPLES[0].Description,
    Exampleˉid: EXAMPLES[0].Id,
    Source: EXAMPLES[0].Source,
}];
let Activeˉsourceˉtabˉid = 1;
let Nextˉsourceˉtabˉid = 2;
let Nextˉscratchˉnumber = 1;
let Activeˉresultˉtab = "output";
let Editorˉready = false;
let Isˉrunning = false;

Initialize();

function Initialize() {
    for (const Example of EXAMPLES) {
        const Option = document.createElement("option");
        Option.value = Example.Id;
        Option.textContent = Example.Title;
        Exampleˉpicker.append(Option);
    }
    Bindˉevents();
    Renderˉsourceˉtabs();
    Loadˉactiveˉsource();
    try {
        Editor.Initialize(Editorˉelement, Shell, Activeˉsourceˉtab().Source, Runˉprogram);
        Editorˉready = true;
    }
    catch (Failure) {
        Showˉunexpectedˉfailure(Failure);
    }
}

function Bindˉevents() {
    Runˉbutton.addEventListener("click", Runˉprogram);
    Exampleˉpicker.addEventListener("change", () => Openˉexample(Exampleˉpicker.value));
    Budgetˉpicker.addEventListener("change", () => {
        const Value = Number(Budgetˉpicker.value);
        if (!Number.isInteger(Value)) {
            Budgetˉpicker.value = "1000000";
        }
    });
    document.getElementById("theme-toggle").addEventListener("click", () => Editor.ToggleTheme());
    document.querySelectorAll("[data-result-tab]").forEach(Button =>
        Button.addEventListener("click", () => Selectˉresultˉtab(Button.dataset.resultTab)));
    Errorˉui.querySelector(".dismiss").addEventListener("click", () => {
        Errorˉui.hidden = true;
    });
    Fallbackˉeditor.addEventListener("input", () => {
        Activeˉsourceˉtab().Source = Fallbackˉeditor.value;
        const Status = Shell.querySelector("[data-status-characters]");
        Status.textContent = `${Fallbackˉeditor.value.length.toLocaleString()} chars`;
    });
    window.addEventListener("beforeunload", () => Editor.Dispose());
    window.addEventListener("unhandledrejection", Event => Showˉunexpectedˉfailure(Event.reason));
}

function Activeˉsourceˉtab() {
    return Sourceˉtabs.find(Tab => Tab.Id === Activeˉsourceˉtabˉid);
}

function Saveˉactiveˉsource() {
    const Tab = Activeˉsourceˉtab();
    if (Tab !== undefined) {
        Tab.Source = Editorˉready ? Editor.GetValue() : Fallbackˉeditor.value;
    }
}

function Loadˉactiveˉsource() {
    const Tab = Activeˉsourceˉtab();
    Fallbackˉeditor.value = Tab.Source;
    if (Editorˉready) {
        Editor.SetValue(Tab.Source);
    }
    Exampleˉdescription.textContent = Tab.Description;
    Exampleˉpicker.value = Tab.Exampleˉid ?? "";
    document.getElementById("source-editor-panel")
        .setAttribute("aria-label", `Source for ${Tab.Fileˉname}`);
}

function Renderˉsourceˉtabs() {
    Sourceˉtabsˉelement.replaceChildren();
    for (const Tab of Sourceˉtabs) {
        const Container = document.createElement("div");
        Container.className = `editor-tab${Tab.Id === Activeˉsourceˉtabˉid ? " active" : ""}`;

        const Select = document.createElement("button");
        Select.className = "editor-tab-select";
        Select.type = "button";
        Select.role = "tab";
        Select.ariaSelected = String(Tab.Id === Activeˉsourceˉtabˉid);
        Select.disabled = Isˉrunning;
        Select.title = `Open ${Tab.Fileˉname}`;
        const Badge = document.createElement("b");
        Badge.ariaHidden = "true";
        Badge.textContent = "WV";
        const Label = document.createElement("span");
        Label.textContent = Tab.Fileˉname;
        Select.append(Badge, Label);
        Select.addEventListener("click", () => Selectˉsourceˉtab(Tab.Id));
        Container.append(Select);

        if (Sourceˉtabs.length > 1) {
            const Close = document.createElement("button");
            Close.className = "editor-tab-close";
            Close.type = "button";
            Close.disabled = Isˉrunning;
            Close.ariaLabel = `Close ${Tab.Fileˉname}`;
            Close.title = `Close ${Tab.Fileˉname}`;
            Close.innerHTML = '<svg viewBox="0 0 16 16" aria-hidden="true"><path d="m4.5 4.5 7 7m0-7-7 7" /></svg>';
            Close.addEventListener("click", () => Closeˉsourceˉtab(Tab.Id));
            Container.append(Close);
        }
        Sourceˉtabsˉelement.append(Container);
    }

    const Add = document.createElement("button");
    Add.className = "editor-tab-add";
    Add.type = "button";
    const Maximumˉtabsˉopen = Sourceˉtabs.length >= MAXIMUM_SOURCE_TABS;
    Add.disabled = Isˉrunning || Maximumˉtabsˉopen;
    Add.ariaLabel = Maximumˉtabsˉopen
        ? "Maximum source tabs open"
        : Isˉrunning
            ? "New scratch tab unavailable while running"
            : "New scratch tab";
    Add.title = Add.ariaLabel;
    Add.innerHTML = '<svg viewBox="0 0 16 16" aria-hidden="true"><path d="M8 3v10M3 8h10" /></svg>';
    Add.addEventListener("click", Addˉsourceˉtab);
    Sourceˉtabsˉelement.append(Add);
}

function Selectˉsourceˉtab(Id) {
    if (Isˉrunning || Id === Activeˉsourceˉtabˉid) {
        return;
    }
    Saveˉactiveˉsource();
    Activeˉsourceˉtabˉid = Id;
    Renderˉsourceˉtabs();
    Loadˉactiveˉsource();
}

function Closeˉsourceˉtab(Id) {
    if (Isˉrunning || Sourceˉtabs.length === 1) {
        return;
    }
    Saveˉactiveˉsource();
    const Index = Sourceˉtabs.findIndex(Tab => Tab.Id === Id);
    Sourceˉtabs.splice(Index, 1);
    if (Id === Activeˉsourceˉtabˉid) {
        Activeˉsourceˉtabˉid = Sourceˉtabs[Math.min(Index, Sourceˉtabs.length - 1)].Id;
    }
    Renderˉsourceˉtabs();
    Loadˉactiveˉsource();
}

function Addˉsourceˉtab() {
    if (Isˉrunning || Sourceˉtabs.length >= MAXIMUM_SOURCE_TABS) {
        return;
    }
    Saveˉactiveˉsource();
    const Number = Nextˉscratchˉnumber++;
    const Tab = {
        Id: Nextˉsourceˉtabˉid++,
        Fileˉname: `Scratch-${Number}.wv`,
        Description: "An editable single-module source. The browser-native language profile remains experimental while its surface expands.",
        Exampleˉid: null,
        Source: SCRATCH_SOURCE.replace("module Scratch", `module Scratchˉ${Number}`),
    };
    Sourceˉtabs.push(Tab);
    Activeˉsourceˉtabˉid = Tab.Id;
    Renderˉsourceˉtabs();
    Loadˉactiveˉsource();
}

function Openˉexample(Id) {
    if (Isˉrunning) {
        return;
    }
    const Example = EXAMPLES.find(Item => Item.Id === Id);
    if (Example === undefined) {
        return;
    }
    const Existing = Sourceˉtabs.find(Tab => Tab.Exampleˉid === Example.Id);
    if (Existing !== undefined) {
        Selectˉsourceˉtab(Existing.Id);
        return;
    }
    if (Sourceˉtabs.length >= MAXIMUM_SOURCE_TABS) {
        return;
    }
    Saveˉactiveˉsource();
    const Tab = {
        Id: Nextˉsourceˉtabˉid++,
        Fileˉname: Example.Fileˉname,
        Description: Example.Description,
        Exampleˉid: Example.Id,
        Source: Example.Source,
    };
    Sourceˉtabs.push(Tab);
    Activeˉsourceˉtabˉid = Tab.Id;
    Renderˉsourceˉtabs();
    Loadˉactiveˉsource();
}

async function Runˉprogram() {
    if (Isˉrunning) {
        return;
    }
    Saveˉactiveˉsource();
    const Source = Activeˉsourceˉtab().Source;
    const Instructionˉbudget = Number(Budgetˉpicker.value);
    Isˉrunning = true;
    Setˉbusy(true);
    Setˉrunˉstate("running", "Compiling");
    Resultˉstatus.hidden = true;
    Editor.SetDiagnostics([]);
    Setˉdiagnosticˉcount(0);
    Selectˉresultˉtab("output");
    const Started = performance.now();
    const Updateˉprogress = () => {
        const Seconds = Math.floor((performance.now() - Started) / 1_000);
        Resultˉviews.get("output").innerHTML = `
            <div class="running-state">
                <span class="activity-ring" aria-hidden="true"></span>
                <p><strong>Compiling natively in WebAssembly…</strong><span>${Seconds.toLocaleString()} seconds elapsed</span></p>
            </div>`;
    };
    Updateˉprogress();
    const Progressˉtimer = setInterval(Updateˉprogress, 1_000);

    try {
        const Result = await Compileˉandˉrun(
            Source,
            COMPILER_TIMEOUT_MILLISECONDS,
            Instructionˉbudget,
        );
        const Elapsedˉmilliseconds = performance.now() - Started;
        const Frameworkˉrequests = Countˉframeworkˉrequests();
        if (!Result.Succeeded) {
            Renderˉfailure(Result.Error ?? "The Windvale compiler worker failed.", Elapsedˉmilliseconds, Frameworkˉrequests);
        } else if (!(Result.Wvb instanceof ArrayBuffer)) {
            Renderˉfailure("The compiler worker did not return canonical WVB bytes.", Elapsedˉmilliseconds, Frameworkˉrequests);
        } else {
            Renderˉpipelineˉresult(Result, Instructionˉbudget, Elapsedˉmilliseconds, Frameworkˉrequests);
        }
    }
    catch (Failure) {
        Renderˉfailure(
            Failure instanceof Error ? Failure.message : "The Windvale pipeline failed.",
            performance.now() - Started,
            Countˉframeworkˉrequests(),
        );
    }
    finally {
        clearInterval(Progressˉtimer);
        Isˉrunning = false;
        Setˉbusy(false);
    }
}

function Renderˉpipelineˉresult(Result, Instructionˉbudget, Elapsedˉmilliseconds, Frameworkˉrequests) {
    const Passed = Result.ExecutionStatus === 0 && Frameworkˉrequests === 0;
    const Statusˉlabel = Passed ? "Passed" : "Execution failed";
    const Statusˉclass = Passed ? "success" : "failure";
    const Diagnostic = Frameworkˉrequests === 0
        ? (Passed ? null : `Verified WVB execution returned WVR${Result.ExecutionStatus}.`)
        : `The static playground unexpectedly requested ${Frameworkˉrequests} .NET/Blazor framework asset(s).`;
    Setˉrunˉstate(Statusˉclass, Statusˉlabel);
    Setˉresultˉstatus(Statusˉclass, Statusˉlabel);
    Setˉdiagnosticˉcount(Diagnostic === null ? 0 : 1);

    const Resultˉlabel = Result.ExecutionResult === null ? "—" : Result.ExecutionResult.toLocaleString();
    Resultˉviews.get("output").innerHTML = `
        <div class="channel-heading"><span>Program result</span><span>i32</span></div>
        <pre class="console-output">${Passed ? `Main returned ${Escapeˉhtml(Resultˉlabel)}` : "[execution did not complete successfully]"}</pre>`;
    Renderˉdiagnostics(Diagnostic);
    Resultˉviews.get("bytecode").innerHTML = `
        <div class="channel-heading"><span>Canonical WVB bytes</span><span>${Result.Wvb.byteLength.toLocaleString()} bytes</span></div>
        <pre class="bytecode-output">${Hexˉdump(new Uint8Array(Result.Wvb))}</pre>`;
    Resultˉviews.get("execution").innerHTML = `
        <div class="execution-layout native-execution-layout">
            <dl class="execution-grid">
                ${Evidenceˉitem("Pipeline", Statusˉlabel)}
                ${Evidenceˉitem("Module profile", "portable")}
                ${Evidenceˉitem("Main result", Resultˉlabel)}
                ${Evidenceˉitem("WVB size", `${Result.Wvb.byteLength.toLocaleString()} bytes`)}
                ${Evidenceˉitem("Execution budget", Instructionˉbudget.toLocaleString())}
                ${Evidenceˉitem("Elapsed", `${(Elapsedˉmilliseconds / 1_000).toFixed(1)} seconds`)}
                ${Evidenceˉitem("Compiler", Formatˉcount(Result.CompilerInstructions))}
                ${Evidenceˉitem("Execution guest", Formatˉcount(Result.ExecutionGuestInstructions))}
                ${Evidenceˉitem("Execution outer", Formatˉcount(Result.ExecutionOuterInstructions))}
            </dl>
            <div class="digest">
                <span>WVB SHA-256</span>
                <code>${Escapeˉhtml(Result.WvbSha256 ?? "—")}</code>
                <span>Runtime boundary</span>
                <code>compile → verify → execute</code>
                <span>Framework requests</span>
                <code>${Frameworkˉrequests} .NET / Blazor</code>
                <p class="wasm-note">The direct compiler package is identity-checked before use. Its returned WVB is treated as untrusted input and admitted again before execution.</p>
            </div>
        </div>`;
}

function Renderˉfailure(Message, Elapsedˉmilliseconds, Frameworkˉrequests) {
    const Safeˉmessage = String(Message);
    Setˉrunˉstate("failure", "Failed");
    Setˉresultˉstatus("failure", "Failed");
    Setˉdiagnosticˉcount(1);
    Renderˉdiagnostics(Safeˉmessage);
    Resultˉviews.get("output").innerHTML = `
        <div class="channel-heading diagnostic"><span>Pipeline failure</span><span>${(Elapsedˉmilliseconds / 1_000).toFixed(1)} seconds</span></div>
        <pre class="console-output diagnostic">${Escapeˉhtml(Safeˉmessage)}</pre>`;
    Resultˉviews.get("bytecode").innerHTML = '<div class="empty-state"><span aria-hidden="true">—</span><p>No WVB was produced.</p></div>';
    Resultˉviews.get("execution").innerHTML = `
        <div class="execution-layout native-execution-layout">
            <dl class="execution-grid">
                ${Evidenceˉitem("Pipeline", "Failed")}
                ${Evidenceˉitem("Elapsed", `${(Elapsedˉmilliseconds / 1_000).toFixed(1)} seconds`)}
                ${Evidenceˉitem("Framework requests", `${Frameworkˉrequests}`)}
            </dl>
            <div class="digest"><span>Boundary</span><code>No WVB was executed.</code></div>
        </div>`;
}

function Renderˉdiagnostics(Message) {
    const View = Resultˉviews.get("diagnostics");
    if (Message === null) {
        View.innerHTML = '<div class="empty-state success"><span aria-hidden="true">✓</span><p>No diagnostics. The module passed compilation, WVB admission, and execution.</p></div>';
        return;
    }
    const Code = Message.match(/\bWVR\d{4}\b/u)?.[0] ?? "WVWEB";
    View.innerHTML = `
        <ol class="diagnostic-list">
            <li><button type="button" disabled>
                <span><code>${Escapeˉhtml(Code)}</code><small>Browser-native pipeline</small></span>
                <strong>${Escapeˉhtml(Message)}</strong>
            </button></li>
        </ol>`;
}

function Selectˉresultˉtab(Name) {
    if (!Resultˉviews.has(Name)) {
        return;
    }
    Activeˉresultˉtab = Name;
    document.querySelectorAll("[data-result-tab]").forEach(Button => {
        const Active = Button.dataset.resultTab === Activeˉresultˉtab;
        Button.classList.toggle("active", Active);
        Button.setAttribute("aria-pressed", String(Active));
    });
    for (const [Viewˉname, View] of Resultˉviews) {
        View.hidden = Viewˉname !== Activeˉresultˉtab;
    }
}

function Setˉbusy(Busy) {
    Runˉbutton.disabled = Busy;
    Runˉbutton.querySelector("span").textContent = Busy ? "Running…" : "Compile + Run";
    Exampleˉpicker.disabled = Busy;
    Budgetˉpicker.disabled = Busy;
    Renderˉsourceˉtabs();
}

function Setˉrunˉstate(Classˉname, Label) {
    Runˉstate.className = `status-state ${Classˉname}`;
    Runˉstate.querySelector("span").textContent = Label;
}

function Setˉresultˉstatus(Classˉname, Label) {
    Resultˉstatus.hidden = false;
    Resultˉstatus.className = `result-status ${Classˉname}`;
    Resultˉstatus.textContent = Label;
}

function Setˉdiagnosticˉcount(Count) {
    Diagnosticˉcount.hidden = Count === 0;
    Diagnosticˉcount.textContent = Count.toLocaleString();
    Statusˉdiagnostics.textContent = Count.toLocaleString();
}

function Countˉframeworkˉrequests() {
    return performance.getEntriesByType("resource").filter(Entry => {
        const Path = new URL(Entry.name).pathname.toLowerCase();
        return Path.includes("/_framework/") ||
            Path.includes("blazor.webassembly") ||
            Path.includes("dotnet.native");
    }).length;
}

function Hexˉdump(Bytes) {
    const Lines = [];
    for (let Offset = 0; Offset < Bytes.byteLength; Offset += 16) {
        const Slice = Bytes.slice(Offset, Math.min(Offset + 16, Bytes.byteLength));
        const Hex = Array.from(Slice, Byte => Byte.toString(16).padStart(2, "0")).join(" ");
        const Text = Array.from(Slice, Byte => Byte >= 32 && Byte <= 126 ? String.fromCharCode(Byte) : ".").join("");
        Lines.push(`${Offset.toString(16).padStart(8, "0")}  ${Hex.padEnd(47, " ")}  |${Text}|`);
    }
    return Lines.join("\n");
}

function Evidenceˉitem(Name, Value) {
    return `<div><dt>${Escapeˉhtml(Name)}</dt><dd>${Escapeˉhtml(Value)}</dd></div>`;
}

function Formatˉcount(Value) {
    return Number.isInteger(Value) ? Value.toLocaleString() : "—";
}

function Escapeˉhtml(Value) {
    return String(Value)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#39;");
}

function Showˉunexpectedˉfailure(Failure) {
    Errorˉui.hidden = false;
    Errorˉui.firstChild.textContent = Failure instanceof Error
        ? `The playground encountered an unexpected browser error: ${Failure.message} `
        : "The playground encountered an unexpected browser error. ";
}
