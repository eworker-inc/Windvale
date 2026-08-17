import path from 'node:path';
import {
    Getˉhostedˉapplicationˉcacheˉkey,
    Prepareˉhostedˉapplicationˉcontext
} from './Native-Hosted-Application-Cache-Core.mjs';

function Reject(message) {
    throw new Error(message);
}

async function Main() {
    if (process.argv.length !== 10) {
        Reject(
            'Usage: node Tools/Native/Get-Native-Hosted-Application-Cache-Key.mjs ' +
            '<namespace> <windows|linux> <profile> <input.wvb> <chunk-prefix> ' +
            '<fragment-count> <entry> <packager>'
        );
    }
    const context = await Prepareˉhostedˉapplicationˉcontext(
        process.argv[3],
        path.resolve(process.argv[9])
    );
    const key = await Getˉhostedˉapplicationˉcacheˉkey(context, {
        namespace: process.argv[2],
        profile: process.argv[4],
        inputPath: process.argv[5],
        chunkPrefix: process.argv[6],
        fragmentCountText: process.argv[7],
        entry: process.argv[8]
    });
    process.stdout.write(`${key}\n`);
}

try {
    await Main();
} catch (error) {
    process.stderr.write(`${error.message}\n`);
    process.exit(1);
}
