const Mode = process.argv[2] ?? "stale";

function Request() {
    const Model = Buffer.from("gpt-test", "utf8");
    const Messages = Buffer.alloc(28);
    Messages.write("WVMM", 0, 4, "ascii");
    Messages.writeUInt32LE(1, 4);
    Messages.writeUInt32LE(Messages.length, 8);
    Messages.writeUInt32LE(1, 12);
    Messages.writeUInt32LE(1, 16);
    Messages.writeUInt32LE(4, 20);
    Messages.write("test", 24, 4, "utf8");
    const Bytes = Buffer.alloc(48 + Model.length + Messages.length);
    Bytes.write("WVMQ", 0, 4, "ascii");
    Bytes.writeUInt32LE(1, 4);
    Bytes.writeUInt32LE(Bytes.length, 8);
    Bytes.writeUInt32LE(2, 12);
    Bytes.writeBigUInt64LE(701n, 16);
    Bytes.writeBigUInt64LE(22n, 24);
    Bytes.writeUInt32LE(64, 32);
    Bytes.writeUInt32LE(Model.length, 36);
    Bytes.writeUInt32LE(Messages.length, 40);
    Model.copy(Bytes, 48);
    Messages.copy(Bytes, 48 + Model.length);
    return Bytes;
}

if (Mode === "malformed") {
    process.stdout.write(Buffer.from("BAD!\x01\x00\x00\x00\x30\x00\x00\x00", "binary"));
} else if (Mode === "idle") {
    setInterval(() => {}, 60_000);
} else {
    const Input = Request();
    let Response = Buffer.alloc(0);
    process.stdin.on("data", Chunk => {
        Response = Buffer.concat([Response, Chunk]);
        if (Response.length < 12) return;
        const Total = Response.readUInt32LE(8);
        if (Response.length < Total) return;
        const Valid = Response.length === Total &&
            Response.subarray(0, 4).toString("ascii") === "WVMG" &&
            Response.readUInt32LE(4) === 1 && Response.readUInt32LE(12) === 9 &&
            Response.readBigUInt64LE(16) === 701n;
        Response.fill(0);
        Input.fill(0);
        process.exit(Valid ? 0 : 7);
    });
    process.stdout.write(Input);
}
