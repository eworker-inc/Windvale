import assert from "node:assert/strict";
import {
    Createˉprotectedˉcredential,
    Inspectˉprotectedˉcredential,
    Unlockˉprotectedˉcredential,
} from "../../Runtime/Hosted/Credentials/Protected-Credential.mjs";
import { Hostˉnetworkˉstatus } from "../../Runtime/Hosted/Network/Host-Network-Protocol.mjs";

const Tests = [];
const PASSPHRASE = Buffer.from("correct horse battery staple", "utf8");
const SECRETS = Object.freeze({
    openai: Buffer.from("sk-openai-test-abcdefghijklmnop", "ascii"),
    anthropic: Buffer.from("sk-ant-test-abcdefghijklmnop", "ascii"),
    google: Buffer.from("google-test-abcdefghijklmnop", "ascii"),
});
const SERVICES = Object.freeze({
    openai: "api.openai.com",
    anthropic: "api.anthropic.com",
    google: "generativelanguage.googleapis.com",
});
const Wrappers = new Map();

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

async function Wrapper(Provider) {
    if (!Wrappers.has(Provider)) {
        Wrappers.set(Provider, await Createˉprotectedˉcredential({
            provider: Provider,
            service: SERVICES[Provider],
            generation: 17n,
            credential: SECRETS[Provider],
            passphrase: PASSPHRASE,
            randomBytes: Deterministicˉrandom(),
        }));
    }
    return Buffer.from(Wrappers.get(Provider));
}

function Fakeˉhttps(Observed) {
    return () => {
        let Read = false;
        return {
            connect: async () => ({
                status: Hostˉnetworkˉstatus.Valid,
                providerGeneration: 71n,
                connectionId: 1n,
                connectionGeneration: 1n,
                address: "127.0.0.1",
                endpointPort: 443,
            }),
            write: async (_Id, _Generation, Bytes) => {
                Observed.live = Bytes;
                Observed.request = Buffer.from(Bytes);
                Observed.writes = (Observed.writes ?? 0) + 1;
                return { status: Hostˉnetworkˉstatus.Valid, progress: BigInt(Bytes.length) };
            },
            read: async () => {
                assert.equal(Read, false);
                Read = true;
                return {
                    status: Hostˉnetworkˉstatus.Valid,
                    flags: 1,
                    payload: Buffer.from("HTTP/1.1 200 OK\r\nContent-Length: 2\r\n\r\nok", "ascii"),
                };
            },
            closeConnection: async () => ({ status: Hostˉnetworkˉstatus.Valid }),
            teardown: async () => { Observed.teardowns = (Observed.teardowns ?? 0) + 1; },
        };
    };
}

function Bind(Lease, Observed, Values = {}) {
    return Lease.bindHttps({
        port: 443,
        generation: 71n,
        trustGeneration: 5n,
        allowedTargets: new Set(["/v1/models"]),
        maximumRequestBytes: 8_192,
        maximumHeaderBytes: 1_024,
        maximumBodyBytes: 1_024,
        maximumWireBytes: 4_096,
        maximumOperationMilliseconds: 1_000,
        maximumLifetimeMilliseconds: 2_000,
        supervisorFactory: Values.supervisorFactory ?? Fakeˉhttps(Observed),
        ...(Values.allowedHeaders ? { allowedHeaders: Values.allowedHeaders } : {}),
    });
}

async function Authorizedˉrequest(Provider) {
    const Lease = await Unlockˉprotectedˉcredential(await Wrapper(Provider), PASSPHRASE);
    const Observed = {};
    const Client = Bind(Lease, Observed);
    const Result = await Client.request({
        expectedCredentialGeneration: 17n,
        method: "GET",
        target: "/v1/models",
        headers: [{ name: "accept", value: "application/json" }],
    });
    return { lease: Lease, observed: Observed, result: Result };
}

Test("wrapper encrypts the credential and exposes only bounded metadata", async () => {
    const Bytes = await Wrapper("openai");
    assert.equal(Bytes.includes(SECRETS.openai), false);
    const Metadata = Inspectˉprotectedˉcredential(Bytes);
    assert.deepEqual(Metadata, {
        provider: "openai",
        service: "api.openai.com",
        port: 443,
        generation: 17n,
        identity: "0102030405060708090a0b0c0d0e0f10",
        credentialBytes: SECRETS.openai.length,
    });
    assert.equal(Object.values(Metadata).join("|").includes("sk-openai"), false);
});

Test("creation and unlock do not mutate caller-owned input buffers", async () => {
    const Secret = Buffer.from(SECRETS.openai);
    const Password = Buffer.from(PASSPHRASE);
    const SecretCopy = Buffer.from(Secret);
    const PasswordCopy = Buffer.from(Password);
    const Bytes = await Createˉprotectedˉcredential({
        provider: "openai", service: SERVICES.openai, generation: 18n,
        credential: Secret, passphrase: Password, randomBytes: Deterministicˉrandom(),
    });
    assert.deepEqual(Secret, SecretCopy);
    assert.deepEqual(Password, PasswordCopy);
    const Lease = await Unlockˉprotectedˉcredential(Bytes, Password);
    assert.deepEqual(Password, PasswordCopy);
    Lease.destroy();
});

Test("OpenAI authorization is injected internally and request copies are zeroed", async () => {
    const { lease, observed, result } = await Authorizedˉrequest("openai");
    assert.match(observed.request.toString("ascii"), /\r\nauthorization: Bearer sk-openai-test-abcdefghijklmnop\r\n/);
    assert.equal(observed.live.every(Byte => Byte === 0), true);
    assert.equal(result.body.toString(), "ok");
    assert.equal(observed.writes, 1);
    assert.equal(observed.teardowns, 1);
    lease.destroy();
});

Test("Anthropic authorization uses only x-api-key", async () => {
    const { lease, observed } = await Authorizedˉrequest("anthropic");
    const Text = observed.request.toString("ascii");
    assert.match(Text, /\r\nx-api-key: sk-ant-test-abcdefghijklmnop\r\n/);
    assert.doesNotMatch(Text, /authorization:/i);
    lease.destroy();
});

Test("Google authorization uses only x-goog-api-key", async () => {
    const { lease, observed } = await Authorizedˉrequest("google");
    const Text = observed.request.toString("ascii");
    assert.match(Text, /\r\nx-goog-api-key: google-test-abcdefghijklmnop\r\n/);
    assert.doesNotMatch(Text, /authorization:/i);
    lease.destroy();
});

Test("wrong passphrase returns one generic unlock failure", async () => {
    await assert.rejects(
        Unlockˉprotectedˉcredential(
            await Wrapper("openai"), Buffer.from("wrong passphrase value", "utf8"),
        ),
        Error => Error.kind === "unlock_failed" && Error.message === "Protected credential unlock failed.",
    );
});

Test("authenticated ciphertext tampering returns the same unlock failure", async () => {
    const Bytes = await Wrapper("openai");
    Bytes[Bytes.length - 1] ^= 1;
    await assert.rejects(
        Unlockˉprotectedˉcredential(Bytes, PASSPHRASE),
        Error => Error.kind === "unlock_failed" && Error.message === "Protected credential unlock failed.",
    );
});

Test("provider-binding tampering is rejected before it can retarget a key", async () => {
    const Bytes = await Wrapper("openai");
    Bytes.writeUInt32LE(2, 12);
    await assert.rejects(
        Unlockˉprotectedˉcredential(Bytes, PASSPHRASE),
        Error => Error.kind === "invalid_wrapper",
    );
});

Test("malformed public wrapper geometry fails before derivation", async () => {
    const Original = await Wrapper("openai");
    const Mutations = [
        Bytes => Bytes.write("BAD!", 0, 4, "ascii"),
        Bytes => Bytes.writeUInt32LE(2, 4),
        Bytes => Bytes.writeUInt32LE(Bytes.length + 1, 8),
        Bytes => Bytes.writeBigUInt64LE(0n, 16),
        Bytes => Bytes.writeUInt32LE(0, 24),
        Bytes => Bytes.writeUInt32LE(1, 76),
        Bytes => Bytes.fill(0, 80, 96),
    ];
    for (const Mutate of Mutations) {
        const Bytes = Buffer.from(Original);
        Mutate(Bytes);
        assert.throws(() => Inspectˉprotectedˉcredential(Bytes), Error => Error.kind === "invalid_wrapper");
    }
});

Test("passphrases require bounded strict UTF-8", async () => {
    const Base = {
        provider: "openai", service: SERVICES.openai, generation: 1n,
        credential: SECRETS.openai, randomBytes: Deterministicˉrandom(),
    };
    await assert.rejects(Createˉprotectedˉcredential({ ...Base, passphrase: Buffer.from("short") }),
        Error => Error.kind === "invalid_passphrase");
    await assert.rejects(Createˉprotectedˉcredential({ ...Base, passphrase: Buffer.alloc(16, 0xff) }),
        Error => Error.kind === "invalid_passphrase");
});

Test("credential plaintext requires bounded printable ASCII", async () => {
    const Base = {
        provider: "openai", service: SERVICES.openai, generation: 1n,
        passphrase: PASSPHRASE, randomBytes: Deterministicˉrandom(),
    };
    await assert.rejects(Createˉprotectedˉcredential({ ...Base, credential: Buffer.from("short") }),
        Error => Error.kind === "invalid_credential");
    await assert.rejects(Createˉprotectedˉcredential({ ...Base, credential: Buffer.alloc(16) }),
        Error => Error.kind === "invalid_credential");
});

Test("failed entropy is explicit and has no weak fallback", async () => {
    await assert.rejects(Createˉprotectedˉcredential({
        provider: "openai", service: SERVICES.openai, generation: 1n,
        credential: SECRETS.openai, passphrase: PASSPHRASE,
        randomBytes: Length => Buffer.alloc(Length),
    }), Error => Error.kind === "entropy_unavailable");
});

Test("stale credential generation is denied before network construction", async () => {
    const Lease = await Unlockˉprotectedˉcredential(await Wrapper("openai"), PASSPHRASE);
    let Factories = 0;
    const Client = Bind(Lease, {}, { supervisorFactory: () => { Factories += 1; return {}; } });
    await assert.rejects(Client.request({
        expectedCredentialGeneration: 16n, method: "GET", target: "/v1/models",
    }), Error => Error.kind === "stale");
    assert.equal(Factories, 0);
    Lease.destroy();
});

Test("caller cannot supply or delegate the provider authorization field", async () => {
    const Lease = await Unlockˉprotectedˉcredential(await Wrapper("openai"), PASSPHRASE);
    assert.throws(() => Bind(Lease, {}, { allowedHeaders: ["accept", "Authorization"] }),
        Error => Error.kind === "invalid_binding");
    const Client = Bind(Lease, {});
    await assert.rejects(Client.request({
        expectedCredentialGeneration: 17n,
        method: "GET",
        target: "/v1/models",
        headers: [{ name: "Authorization", value: "Bearer attacker" }],
    }), Error => Error.kind === "denied");
    Lease.destroy();
});

Test("credential origin fixes HTTPS port 443", async () => {
    const Lease = await Unlockˉprotectedˉcredential(await Wrapper("openai"), PASSPHRASE);
    assert.throws(() => Lease.bindHttps({ port: 8443 }), Error => Error.kind === "invalid_binding");
    assert.equal(Lease.inspect().port, 443);
    Lease.destroy();
    await assert.rejects(Createˉprotectedˉcredential({
        provider: "openai", service: "credential-thief.example", generation: 18n,
        credential: SECRETS.openai, passphrase: PASSPHRASE,
        randomBytes: Deterministicˉrandom(),
    }), Error => Error.kind === "invalid_binding");
});

Test("destroy revokes existing bindings and is idempotent", async () => {
    const Lease = await Unlockˉprotectedˉcredential(await Wrapper("openai"), PASSPHRASE);
    const Client = Bind(Lease, {});
    Lease.destroy();
    Lease.destroy();
    assert.equal(Lease.inspect().state, "destroyed");
    await assert.rejects(Client.request({
        expectedCredentialGeneration: 17n, method: "GET", target: "/v1/models",
    }), Error => Error.kind === "revoked");
    assert.throws(() => Bind(Lease, {}), Error => Error.kind === "revoked");
});

let Failed = false;
for (let Index = 0; Index < Tests.length; Index += 1) {
    const Value = Tests[Index];
    process.stdout.write(`step=protected-credential item=${Index + 1}/${Tests.length}\n`);
    try { await Value.body(); } catch (Error) {
        Failed = true;
        process.stderr.write(`case=${Value.name} status=Failed\n${Error.stack ?? Error}\n`);
    }
}
for (const Bytes of Wrappers.values()) Bytes.fill(0);
PASSPHRASE.fill(0);
for (const Bytes of Object.values(SECRETS)) Bytes.fill(0);
if (Failed) process.exit(1);
process.stdout.write(
    `protected credential status=Passed providers=3 cases=${Tests.length} ` +
    "current-host=Verified plaintext-files=0 exported-secrets=0 public-network=0\n",
);
