import assert from "node:assert/strict";
import { resolve } from "node:path";
import {
    Createˉprotectedˉcredential,
} from "../../Runtime/Hosted/Credentials/Protected-Credential.mjs";
import {
    Nativeˉexternalˉmodelˉgatewayˉsupervisor,
} from "../../Runtime/Hosted/Models/Native-External-Model-Gateway-Supervisor.mjs";

if (process.argv.length !== 3) {
    process.stderr.write("Usage: node Test-Native-External-Model-Gateway-Execution.mjs <native-worker>\n");
    process.exit(64);
}

const Passphrase = Buffer.from("native execution passphrase value", "utf8");
const Secret = Buffer.from("sk-native-execution-test-abcdefghijklmnop", "ascii");
let Counter = 91;
const Wrapper = await Createˉprotectedˉcredential({
    provider: "openai",
    service: "api.openai.com",
    generation: 17n,
    credential: Secret,
    passphrase: Passphrase,
    randomBytes: Length => {
        const Bytes = Buffer.alloc(Length);
        for (let Index = 0; Index < Length; Index += 1) Bytes[Index] = (Counter + Index) & 0xff;
        Counter += Length;
        return Bytes;
    },
});

try {
    const Bridge = new Nativeˉexternalˉmodelˉgatewayˉsupervisor({
        applicationPath: resolve(process.argv[2]),
        wrapper: Wrapper,
        passphrase: Passphrase,
        providerGeneration: 23n,
        trustGeneration: 7n,
        maximumOperationMilliseconds: 5_000,
        maximumLifetimeMilliseconds: 10_000,
    });
    const Result = await Bridge.run();
    assert.equal(Result.code, 0);
    assert.equal(Result.signal, null);
    assert.equal(Result.diagnosticBytes, 0);
    assert.equal(Result.ready.provider, "openai");
    assert.equal(Result.ready.providerGeneration, 23n);
    process.stdout.write(
        "native external model gateway execution status=Passed requests=1 " +
        "result=stale public-network=0 real-credentials=0\n",
    );
} finally {
    Wrapper.fill(0);
    Passphrase.fill(0);
    Secret.fill(0);
}
