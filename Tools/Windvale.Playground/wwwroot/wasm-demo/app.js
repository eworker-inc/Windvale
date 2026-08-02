import { Execute } from "../js/windvale-wasm-host.js";
import {
    ARTIFACT_BASE64,
    ARTIFACT_SHA256,
    ARTIFACT_SIZE,
    EXPECTED_ABI,
    EXPECTED_INSTRUCTIONS,
    EXPECTED_RESULT,
    EXPECTED_STATUS,
} from "./windvale-artifact.js";

const Runˉbutton = document.getElementById("run");
const Output = document.getElementById("output");
const Stateˉbadge = document.getElementById("state-badge");

Runˉbutton.addEventListener("click", Runˉartifact);

async function Runˉartifact() {
    Setˉstate("running", "Running");
    Runˉbutton.disabled = true;
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

        const Execution = await Execute(Bytes, 2000);
        if (!Execution.Succeeded) {
            throw new Error(Execution.Error ?? "The browser worker rejected the module.");
        }
        if (
            Execution.ExecutionAbi !== EXPECTED_ABI ||
            Execution.Status !== EXPECTED_STATUS ||
            Execution.Result !== EXPECTED_RESULT ||
            Execution.ExecutedInstructions !== EXPECTED_INSTRUCTIONS
        ) {
            throw new Error("The module result does not match its qualified evidence.");
        }

        const Frameworkˉrequests = Countˉframeworkˉrequests();
        if (Frameworkˉrequests !== 0) {
            throw new Error("Unexpected .NET or Blazor framework assets were requested.");
        }

        Output.textContent = [
            `SHA-256     ${Actualˉsha256}`,
            `ABI          ${Execution.ExecutionAbi}`,
            `Status       ${Execution.Status}`,
            `Result       ${Execution.Result}`,
            `Instructions ${Execution.ExecutedInstructions}`,
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
    }
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
