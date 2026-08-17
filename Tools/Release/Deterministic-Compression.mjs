import { constants as ZLIB_CONSTANTS, deflateRawSync } from "node:zlib";

const CRC32_TABLE = (() => {
    const Table = new Uint32Array(256);
    for (let Value = 0; Value < 256; Value++) {
        let Current = Value;
        for (let Bit = 0; Bit < 8; Bit++) {
            Current = (Current & 1) !== 0 ?
                (0xedb88320 ^ (Current >>> 1)) : (Current >>> 1);
        }
        Table[Value] = Current >>> 0;
    }
    return Table;
})();

export function Crc32(Value) {
    let Crc = 0xffffffff;
    for (const Byte of Value) {
        Crc = CRC32_TABLE[(Crc ^ Byte) & 0xff] ^ (Crc >>> 8);
    }
    return (Crc ^ 0xffffffff) >>> 0;
}

export function Deflateˉbytes(Value) {
    return deflateRawSync(Value, {
        level: 6,
        memLevel: 8,
        strategy: ZLIB_CONSTANTS.Z_DEFAULT_STRATEGY,
        windowBits: 15,
    });
}

export function Gzipˉdeflate(Value) {
    const Header = Buffer.from([0x1f, 0x8b, 0x08, 0x00, 0, 0, 0, 0, 0x00, 0xff]);
    const Trailer = Buffer.alloc(8);
    Trailer.writeUInt32LE(Crc32(Value), 0);
    Trailer.writeUInt32LE(Value.length >>> 0, 4);
    return Buffer.concat([Header, Deflateˉbytes(Value), Trailer]);
}
