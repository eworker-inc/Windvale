import { Execute } from "../js/windvale-wasm-host.js";
import {
    ARTIFACT_BASE64,
    ARTIFACT_SHA256,
    ARTIFACT_SIZE,
    EXHAUSTION_BUDGET,
    EXHAUSTION_STATUS,
    EXPECTED_ABI,
    EXPECTED_OUTPUT_KIND,
    SUCCESS_BUDGET,
    SUCCESS_STATUS,
} from "./windvale-artifact.js";

const Runˉbutton = document.getElementById("run");
const Input = document.getElementById("memory-input");
const Output = document.getElementById("output");
const Stateˉbadge = document.getElementById("state-badge");

Runˉbutton.addEventListener("click", Runˉartifact);

async function Runˉartifact() {
    Setˉstate("running", "Running");
    Runˉbutton.disabled = true;
    Input.disabled = true;
    Output.textContent = "Decoding and verifying the pinned artifact…";

    try {
        const Bytes = Decodeˉbase64(ARTIFACT_BASE64);
        if (Bytes.byteLength !== ARTIFACT_SIZE) {
            throw new Error("The embedded artifact length is incorrect.");
        }

        const Actualˉsha256 = await Sha256(Bytes);
        if (Actualˉsha256 !== ARTIFACT_SHA256) {
            throw new Error("The embedded artifact identity is incorrect.");
        }

        const Inputˉbytes = new TextEncoder().encode(Input.value);
        const Canonicalˉinput = new TextDecoder("utf-8", { fatal: true })
            .decode(Inputˉbytes);
        if (Canonicalˉinput !== Input.value) {
            throw new Error("The input contains an unpaired Unicode surrogate.");
        }
        const Success = await Execute(Bytes, 2000, SUCCESS_BUDGET, Inputˉbytes);
        if (!Success.Succeeded) {
            throw new Error(Success.Error ?? "The browser worker rejected the module.");
        }
        if (
            Success.ExecutionAbi !== EXPECTED_ABI ||
            Success.Status !== SUCCESS_STATUS ||
            Success.Result !== null ||
            Success.OutputKind !== EXPECTED_OUTPUT_KIND ||
            !(Success.Output instanceof ArrayBuffer) ||
            Success.ExecutedInstructions !== SUCCESS_BUDGET
        ) {
            throw new Error("The exact-budget run does not match its qualified evidence.");
        }
        const Outputˉbytes = new Uint8Array(Success.Output);
        const Outputˉtext = new TextDecoder("utf-8", { fatal: true })
            .decode(Outputˉbytes);
        if (Outputˉtext !== Canonicalˉinput) {
            throw new Error("The ABI-3 output text differs from the input text.");
        }

        const Exhausted = await Execute(
            Bytes,
            2000,
            EXHAUSTION_BUDGET,
            Inputˉbytes,
        );
        if (!Exhausted.Succeeded) {
            throw new Error(Exhausted.Error ?? "The browser worker rejected the module.");
        }
        if (
            Exhausted.ExecutionAbi !== EXPECTED_ABI ||
            Exhausted.Status !== EXHAUSTION_STATUS ||
            Exhausted.Result !== null ||
            Exhausted.OutputKind !== EXPECTED_OUTPUT_KIND ||
            !(Exhausted.Output instanceof ArrayBuffer) ||
            Exhausted.Output.byteLength !== 0 ||
            Exhausted.ExecutedInstructions !== EXHAUSTION_BUDGET
        ) {
            throw new Error("The exhausted-budget run does not match WVR3011 evidence.");
        }

        const Frameworkˉrequests = Countˉframeworkˉrequests();
        if (Frameworkˉrequests !== 0) {
            throw new Error("Unexpected .NET or Blazor framework assets were requested.");
        }

        Output.textContent = [
            `SHA-256     ${Actualˉsha256}`,
            `ABI          ${Success.ExecutionAbi}`,
            `Memory       4 MiB input · 4 MiB output · kind UTF-8 text`,
            `Budget ${SUCCESS_BUDGET}     status ${Success.Status} · ${Outputˉbytes.byteLength} UTF-8 bytes · ${Success.ExecutedInstructions} instructions`,
            `Budget ${EXHAUSTION_BUDGET}   status ${Exhausted.Status} (WVR3011) · ${Exhausted.ExecutedInstructions} instructions`,
            `Output       ${Previewˉtext(Outputˉtext)}`,
            `Framework    ${Frameworkˉrequests} .NET/Blazor requests`,
        ].join("\n");
        Setˉstate("passed", "Passed");
    }
    catch (Error) {
        Output.textContent = `Execution failed: ${Error instanceof Error ? Error.message : "Unknown failure"}`;
        Setˉstate("failed", "Failed");
    }
    finally {
        Runˉbutton.disabled = false;
        Input.disabled = false;
    }
}

function Previewˉtext(Value) {
    const Maximum = 120;
    const Preview = Value.length <= Maximum
        ? Value
        : `${Value.slice(0, Maximum)}…`;
    return JSON.stringify(Preview);
}

function Decodeˉbase64(Value) {
    const Binary = atob(Value);
    const Bytes = new Uint8Array(Binary.length);
    for (let Index = 0; Index < Binary.length; Index++) {
        Bytes[Index] = Binary.charCodeAt(Index);
    }
    return Bytes;
}

async function Sha256(Bytes) {
    const Digest = await crypto.subtle.digest("SHA-256", Bytes);
    return Array.from(new Uint8Array(Digest))
        .map(Byte => Byte.toString(16).padStart(2, "0"))
        .join("");
}

function Countˉframeworkˉrequests() {
    return performance
        .getEntriesByType("resource")
        .filter(Entry => {
            const Path = new URL(Entry.name).pathname.toLowerCase();
            return Path.includes("/_framework/") ||
                Path.includes("blazor.webassembly") ||
                Path.includes("dotnet.native");
        })
        .length;
}

function Setˉstate(Classˉname, Label) {
    Stateˉbadge.className = `state-badge ${Classˉname}`;
    Stateˉbadge.textContent = Label;
}
