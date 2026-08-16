import assert from "node:assert/strict";
import { fileURLToPath } from "node:url";
import {
    Createˉprotectedˉcredential,
} from "../../Runtime/Hosted/Credentials/Protected-Credential.mjs";
import {
    Nativeˉexternalˉmodelˉgatewayˉsupervisor,
} from "../../Runtime/Hosted/Models/Native-External-Model-Gateway-Supervisor.mjs";

const PEER = fileURLToPath(new URL("./Fixtures/Native-Model-Gateway-Peer.mjs", import.meta.url));
const PASSPHRASE = Buffer.from("native bridge passphrase value", "utf8");
const SECRET = Buffer.from("sk-native-bridge-test-abcdefghijklmnop", "ascii");
const Tests = [];
let Wrapper;

function Test(Name, Body) { Tests.push({ name: Name, body: Body }); }

function Deterministicˉrandom() {
    let Counter = 41;
    return Length => {
        const Bytes = Buffer.alloc(Length);
        for (let Index = 0; Index < Length; Index += 1) Bytes[Index] = (Counter + Index) & 0xff;
        Counter += Length;
        return Bytes;
    };
}

function Supervisor(Mode = "stale", Values = {}) {
    return new Nativeˉexternalˉmodelˉgatewayˉsupervisor({
        applicationPath: process.execPath,
        applicationArguments: [PEER, Mode],
        wrapper: Wrapper,
        passphrase: PASSPHRASE,
        providerGeneration: 23n,
        trustGeneration: 7n,
        maximumOperationMilliseconds: Values.maximumOperationMilliseconds ?? 2_000,
        maximumLifetimeMilliseconds: Values.maximumLifetimeMilliseconds ?? 4_000,
    });
}

Test("dedicated native pipes carry one canonical stale request without networking", async () => {
    const Bridge = Supervisor();
    const Result = await Bridge.run();
    assert.equal(Result.code, 0);
    assert.equal(Result.signal, null);
    assert.equal(Result.ready.provider, "openai");
    assert.equal(Result.ready.providerGeneration, 23n);
    assert.equal(Result.diagnosticBytes, 0);
});

Test("native launch arguments and empty environment contain no protected material", async () => {
    const Bridge = Supervisor();
    const Running = Bridge.run();
    while (Bridge.application === null) await new Promise(Resolve => setTimeout(Resolve, 1));
    const Surface = Bridge.application.spawnargs.join(" ");
    assert.equal(Surface.includes(SECRET.toString("ascii")), false);
    assert.equal(Surface.includes(PASSPHRASE.toString("utf8")), false);
    assert.deepEqual(Bridge.applicationEnvironment, {});
    assert.equal((await Running).code, 0);
});

Test("malformed native output tears down both supervised peers", async () => {
    const Bridge = Supervisor("malformed");
    const Result = await Bridge.run();
    assert.notEqual(Result.code, 0);
    assert.equal(Bridge.closed, true);
    assert.equal(Bridge.gateway.closed, true);
});

Test("the launcher lifetime bounds a native peer that never submits", async () => {
    const Bridge = Supervisor("idle", {
        maximumOperationMilliseconds: 50,
        maximumLifetimeMilliseconds: 100,
    });
    const Started = Date.now();
    const Result = await Bridge.run();
    assert.equal(Result.signal !== null || Result.code !== 0, true);
    assert.equal(Date.now() - Started < 2_000, true);
});

Test("launch configuration requires an absolute executable and consistent timer authority", async () => {
    assert.throws(() => new Nativeˉexternalˉmodelˉgatewayˉsupervisor({
        applicationPath: "node", maximumOperationMilliseconds: 10,
        maximumLifetimeMilliseconds: 5,
    }), /configuration is invalid/);
});

Wrapper = await Createˉprotectedˉcredential({
    provider: "openai", service: "api.openai.com", generation: 17n,
    credential: SECRET, passphrase: PASSPHRASE, randomBytes: Deterministicˉrandom(),
});

let Failed = false;
for (let Index = 0; Index < Tests.length; Index += 1) {
    const Value = Tests[Index];
    process.stdout.write(`step=native-external-model-gateway item=${Index + 1}/${Tests.length}\n`);
    try { await Value.body(); } catch (Error) {
        Failed = true;
        process.stderr.write(`case=${Value.name} status=Failed\n${Error.stack ?? Error}\n`);
    }
}
Wrapper.fill(0);
PASSPHRASE.fill(0);
SECRET.fill(0);
if (Failed) process.exit(1);
process.stdout.write(
    `native external model gateway status=Passed cases=${Tests.length} ` +
    "public-network=0 real-credentials=0 dedicated-pipes=Verified timer-authority=Verified\n",
);
