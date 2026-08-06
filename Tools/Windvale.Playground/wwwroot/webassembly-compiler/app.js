import { Compileˉandˉrun } from "../js/windvale-compiler-host.js";

const Runˉbutton = document.getElementById("run");
const Source = document.getElementById("source");
const Output = document.getElementById("output");
const Stateˉbadge = document.getElementById("state-badge");

Runˉbutton.addEventListener("click", Runˉpipeline);

async function Runˉpipeline() {
    Setˉstate("running", "Running");
    Runˉbutton.disabled = true;
    Source.disabled = true;
    const Started = performance.now();
    const Updateˉprogress = () => {
        const Seconds = Math.floor((performance.now() - Started) / 1000);
        Output.textContent =
            "Loading, warming, and compiling entirely inside WebAssembly…\n" +
            `Elapsed      ${Seconds} seconds`;
    };
    Updateˉprogress();
    const Progressˉtimer = setInterval(Updateˉprogress, 1_000);
    try {
        const Result = await Compileˉandˉrun(Source.value, 600_000, 1_000_000);
        if (!Result.Succeeded) {
            throw new Error(Result.Error ?? "The compiler worker failed.");
        }
        if (!(Result.Wvb instanceof ArrayBuffer) ||
            Result.ExecutionStatus !== 0) {
            throw new Error("The compiler worker returned an invalid success response.");
        }
        const Frameworkˉrequests = Countˉframeworkˉrequests();
        if (Frameworkˉrequests !== 0) {
            throw new Error("Unexpected .NET or Blazor framework assets were requested.");
        }
        Output.textContent = [
            `WVB          ${Result.Wvb.byteLength} bytes`,
            `SHA-256      ${Result.WvbSha256}`,
            `Warmup       ${Result.WarmupGuestInstructions.toLocaleString()} guest · ${Result.WarmupOuterInstructions.toLocaleString()} outer instructions`,
            `Compile      ${Result.CompilerGuestInstructions.toLocaleString()} guest · ${Result.CompilerOuterInstructions.toLocaleString()} outer instructions`,
            `Verify + run status ${Result.ExecutionStatus} · result ${Result.ExecutionResult}`,
            `Execution    ${Result.ExecutionGuestInstructions.toLocaleString()} guest · ${Result.ExecutionOuterInstructions.toLocaleString()} outer instructions`,
            `Elapsed      ${((performance.now() - Started) / 1000).toFixed(1)} seconds`,
            `Framework    ${Frameworkˉrequests} .NET/Blazor requests`,
        ].join("\n");
        Setˉstate("passed", "Passed");
    }
    catch (Failure) {
        Output.textContent = `Pipeline failed: ${Failure instanceof Error ? Failure.message : "Unknown failure"}`;
        Setˉstate("failed", "Failed");
    }
    finally {
        clearInterval(Progressˉtimer);
        Runˉbutton.disabled = false;
        Source.disabled = false;
    }
}

function Countˉframeworkˉrequests() {
    return performance.getEntriesByType("resource").filter(Entry => {
        const Path = new URL(Entry.name).pathname.toLowerCase();
        return Path.includes("/_framework/") ||
            Path.includes("blazor.webassembly") ||
            Path.includes("dotnet.native");
    }).length;
}

function Setˉstate(Classˉname, Label) {
    Stateˉbadge.className = `state-badge ${Classˉname}`;
    Stateˉbadge.textContent = Label;
}
