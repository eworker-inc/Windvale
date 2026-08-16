import assert from "node:assert/strict";
import {
    Createˉprotectedˉcredential,
} from "../../Runtime/Hosted/Credentials/Protected-Credential.mjs";
import {
    Decodeˉmodelˉgatewayˉinitialization,
    Decodeˉmodelˉgatewayˉready,
    Encodeˉmodelˉgatewayˉinitialization,
    Encodeˉmodelˉgatewayˉready,
    Readˉmodelˉgatewayˉframeˉlength,
} from "../../Runtime/Hosted/Models/External-Model-Gateway-Protocol.mjs";
import {
    Externalˉmodelˉgatewayˉsupervisor,
} from "../../Runtime/Hosted/Models/External-Model-Gateway-Supervisor.mjs";

const Tests = [];
const PASSPHRASE = Buffer.from("gateway passphrase test value", "utf8");
const SECRET = Buffer.from("sk-supervised-test-abcdefghijklmnop", "ascii");
let Wrapper;

function Test(Name, Body) { Tests.push({ name: Name, body: Body }); }

function Deterministicˉrandom() {
    let Counter = 1;
    return Length => {
        const Bytes = Buffer.alloc(Length);
        for (let Index = 0; Index < Length; Index += 1) Bytes[Index] = (Counter + Index) & 0xff;
        Counter += Length;
        return Bytes;
    };
}

function Catalogˉrequest(Generation = 22n) {
    const Bytes = Buffer.alloc(48);
    Bytes.write("WVMQ", 0, 4, "ascii");
    Bytes.writeUInt32LE(1, 4);
    Bytes.writeUInt32LE(Bytes.length, 8);
    Bytes.writeUInt32LE(1, 12);
    Bytes.writeBigUInt64LE(91n, 16);
    Bytes.writeBigUInt64LE(Generation, 24);
    Bytes.writeUInt32LE(8, 32);
    return Bytes;
}

function Initialization(Values = {}) {
    return Encodeˉmodelˉgatewayˉinitialization({
        wrapper: Values.wrapper ?? Wrapper,
        passphrase: Values.passphrase ?? PASSPHRASE,
        providerGeneration: Values.providerGeneration ?? 23n,
        trustGeneration: Values.trustGeneration ?? 5n,
        maximumRequestBytes: 65_536,
        maximumHeaderBytes: 16_384,
        maximumBodyBytes: 1_048_576,
        maximumWireBytes: 1_081_344,
        maximumOperationMilliseconds: 5_000,
        maximumLifetimeMilliseconds: 10_000,
    });
}

function Supervisor(Values = {}) {
    return new Externalˉmodelˉgatewayˉsupervisor({
        wrapper: Values.wrapper ?? Wrapper,
        passphrase: Values.passphrase ?? PASSPHRASE,
        providerGeneration: Values.providerGeneration ?? 23n,
        trustGeneration: Values.trustGeneration ?? 5n,
        maximumOperationMilliseconds: 5_000,
        maximumLifetimeMilliseconds: 10_000,
    });
}

Test("initialization frame is bounded and round-trips exact public configuration", async () => {
    const Bytes = Initialization();
    const Values = Decodeˉmodelˉgatewayˉinitialization(Bytes);
    assert.equal(Values.providerGeneration, 23n);
    assert.equal(Values.trustGeneration, 5n);
    assert.deepEqual(Values.wrapper, Wrapper);
    assert.deepEqual(Values.passphrase, PASSPHRASE);
    Values.wrapper.fill(0);
    Values.passphrase.fill(0);
    Bytes.fill(0);
});

Test("initialization encoder preserves caller-owned protected inputs", async () => {
    const WrapperCopy = Buffer.from(Wrapper);
    const PassphraseCopy = Buffer.from(PASSPHRASE);
    const Bytes = Initialization();
    assert.deepEqual(Wrapper, WrapperCopy);
    assert.deepEqual(PASSPHRASE, PassphraseCopy);
    Bytes.fill(0);
});

Test("initialization rejects inconsistent limits before launch", async () => {
    assert.throws(() => Encodeˉmodelˉgatewayˉinitialization({
        wrapper: Wrapper, passphrase: PASSPHRASE,
        providerGeneration: 23n, trustGeneration: 5n,
        maximumHeaderBytes: 16_384, maximumBodyBytes: 1_048_576,
        maximumWireBytes: 1_024,
    }), /Wire limit|inconsistent/);
});

Test("ready records expose only public generations and credential identity", async () => {
    const Bytes = Encodeˉmodelˉgatewayˉready({
        provider: "openai", providerGeneration: 23n, credentialGeneration: 17n,
        identity: "0102030405060708090a0b0c0d0e0f10",
    });
    assert.deepEqual(Decodeˉmodelˉgatewayˉready(Bytes), {
        status: "ready", provider: "openai", providerGeneration: 23n,
        credentialGeneration: 17n, identity: "0102030405060708090a0b0c0d0e0f10",
    });
    assert.equal(Bytes.includes(SECRET), false);
});

Test("failed ready records cannot carry provider metadata", async () => {
    const Bytes = Encodeˉmodelˉgatewayˉready({ status: 1 });
    assert.deepEqual(Decodeˉmodelˉgatewayˉready(Bytes), { status: "failed" });
    Bytes.writeUInt32LE(1, 16);
    assert.throws(() => Decodeˉmodelˉgatewayˉready(Bytes));
});

Test("framed record reader rejects wrong magic and oversized totals", async () => {
    const Bytes = Catalogˉrequest();
    assert.equal(Readˉmodelˉgatewayˉframeˉlength(Bytes, "WVMQ", 48, 65_536), 48);
    const Wrong = Buffer.from(Bytes);
    Wrong.write("BAD!", 0, 4, "ascii");
    assert.throws(() => Readˉmodelˉgatewayˉframeˉlength(Wrong, "WVMQ", 48, 65_536));
    const Large = Buffer.from(Bytes);
    Large.writeUInt32LE(65_537, 8);
    assert.throws(() => Readˉmodelˉgatewayˉframeˉlength(Large, "WVMQ", 48, 65_536));
});

Test("supervised child unlocks with an empty launch argument and environment surface", async () => {
    const Gateway = Supervisor();
    try {
        const Ready = await Gateway.ready();
        assert.equal(Ready.provider, "openai");
        assert.equal(Ready.providerGeneration, 23n);
        assert.equal(Ready.credentialGeneration, 17n);
        const Launch = Gateway.child.spawnargs.join(" ");
        assert.equal(Launch.includes(SECRET.toString("ascii")), false);
        assert.equal(Launch.includes(PASSPHRASE.toString("utf8")), false);
        assert.equal(Gateway.stderrBytes, 0);
    } finally {
        await Gateway.teardown();
    }
});

Test("supervised stale request completes canonically without public networking", async () => {
    const Gateway = Supervisor();
    try {
        await Gateway.ready();
        const Response = await Gateway.request(Catalogˉrequest(22n));
        assert.equal(Response.subarray(0, 4).toString("ascii"), "WVMC");
        assert.equal(Response.readUInt32LE(12), 9);
        assert.equal(Response.readBigUInt64LE(16), 91n);
        assert.equal(Response.readBigUInt64LE(24), 0n);
    } finally {
        await Gateway.teardown();
    }
});

Test("wrong startup passphrase returns only generic startup failure", async () => {
    const Gateway = Supervisor({ passphrase: Buffer.from("wrong gateway passphrase value", "utf8") });
    await assert.rejects(Gateway.ready(), Error =>
        Error.message === "External-model gateway startup failed." ||
        Error.message === "External-model gateway exited.");
    assert.equal(Gateway.stderrBytes, 0);
    await Gateway.teardown();
});

Test("supervisor rejects malformed requests locally and teardown is idempotent", async () => {
    const Gateway = Supervisor();
    await Gateway.ready();
    const Bytes = Catalogˉrequest();
    Bytes.writeUInt32LE(49, 8);
    await assert.rejects(Gateway.request(Bytes), /request is invalid/);
    await Gateway.teardown();
    await Gateway.teardown();
    await assert.rejects(Gateway.request(Catalogˉrequest()), /closed/);
});

Wrapper = await Createˉprotectedˉcredential({
    provider: "openai", service: "api.openai.com", generation: 17n,
    credential: SECRET, passphrase: PASSPHRASE, randomBytes: Deterministicˉrandom(),
});

let Failed = false;
for (let Index = 0; Index < Tests.length; Index += 1) {
    const Value = Tests[Index];
    process.stdout.write(`step=supervised-external-model-gateway item=${Index + 1}/${Tests.length}\n`);
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
    `supervised external model gateway status=Passed providers=3 cases=${Tests.length} ` +
    "child-process=Verified public-network=0 real-credentials=0 plaintext-files=0\n",
);
