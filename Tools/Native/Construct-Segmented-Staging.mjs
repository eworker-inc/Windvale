import { createHash } from 'node:crypto';
import { lstat, realpath, writeFile } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { Runˉdevelopmentˉcommand } from './Development-Command-Core.mjs';
import { Readˉboundedˉhostedˉfile } from './Native-Hosted-Application-Cache-Core.mjs';

const ROOT = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../..');
const CANDIDATE = path.join(ROOT, 'Artifacts/Native-Segmented-Compiler-Toolset-Candidate');
const WINDOWS = process.platform === 'win32';
const HOST = WINDOWS ? 'windows-x64' : 'linux-x64';
const EXTENSION = WINDOWS ? 'exe' : 'elf';
const WVB_SHA256 = '616602ebae6c77234b8886d66381a1e13c3e3c186fd8531424aa733d636a7436';
const PRODUCT_SHA256 = WINDOWS
    ? '494dd6002be0420da63842f9b3ce256496f8b65e7cc3d34528dc8cf336915b0c'
    : '44885be2f9759f7058263f675e7a8a43c64f40b64f14145f1bfafa336e0575c1';

// Reconstruct the retained staging product from its exact WVB. Source-to-WVB
// construction is separate evidence; this mode must never hide a compiler rebuild.
export async function Constructˉsegmentedˉstaging(Output, Deadline, Verify = false) {
    const Started = Date.now();
    Output = path.resolve(Output);
    const Information = await lstat(Output);
    const Relative = path.relative(CANDIDATE, Output);
    const Canonical = await realpath(Output);
    if (!Information.isDirectory() || Information.isSymbolicLink() ||
        Relative === '' || (!Relative.startsWith('..' + path.sep) && !path.isAbsolute(Relative)) ||
        (WINDOWS ? Canonical.toLowerCase() !== Output.toLowerCase() : Canonical !== Output)) {
        throw new Error('Staging reconstruction requires a separate canonical output directory.');
    }
    const Wvb = path.join(CANDIDATE, 'Wvo-Staging-Producer.wvb');
    const Bytes = await Readˉboundedˉhostedˉfile(Wvb, 'retained staging WVB', 1_349_262);
    if (Bytes.length !== 1_349_262 || Digest(Bytes) !== WVB_SHA256) throw new Error('Staging WVB identity differs.');
    const Privateˉwvb = path.join(Output, 'Wvo-Staging-Producer.wvb');
    await writeFile(Privateˉwvb, Bytes, { flag: 'wx' });
    const Records = [];
    async function Run(Name, Command, Arguments) {
        const Begin = Date.now();
        console.log(`staging reconstruction step=${Name} status=Started`);
        const Result = await Runˉdevelopmentˉcommand(Command, Arguments, Deadline, true, 1_048_576);
        await writeFile(path.join(Output, Name + '.log'), Result.Output + Result.Error);
        Records.push({ step: Name, elapsedMilliseconds: Date.now() - Begin, command: [Command, ...Arguments], exitCode: Result.Code });
        await writeFile(path.join(Output, 'Construction.json'), JSON.stringify({ host: HOST,
            elapsedMilliseconds: Date.now() - Started, records: Records }, null, 2) + '\n');
        if (Result.Code !== 0) throw new Error(`Staging reconstruction failed: ${Name}, exit ${Result.Code}.`);
        return Result.Output;
    }
    async function Wrapper(Name, Arguments) {
        const Script = path.join(ROOT, 'Tools/Native', Name + (WINDOWS ? '.cmd' : '.sh'));
        return Run(Name, WINDOWS ? Script : 'bash', WINDOWS ? Arguments : [Script, ...Arguments]);
    }
    const Object = path.join(Output, 'Object'), Linked = path.join(Output, 'Linked'), Image = path.join(Output, 'Image');
    await Wrapper('Stage-Compiler-Wvb', [Privateˉwvb, Object, Object + '.wvop']);
    await Wrapper('Link-Staged-Compiler-Wvo', [Object, Object + '.wvop', Linked, Linked + '.wvli']);
    await Wrapper('Transport-Compiler-Image', [Linked, Linked + '.wvli', Image, Image + '.wvli']);
    const Manifest = await Readˉboundedˉhostedˉfile(Image + '.wvli', 'transport manifest', 256);
    if (Manifest.length < 28 || Manifest.toString('ascii', 0, 4) !== 'WVLI') throw new Error('Invalid transport manifest.');
    const Count = Manifest.readUInt32LE(20), Entry = Manifest.readUInt32LE(16);
    if (Count < 1 || Count > 16 || Manifest.length !== 28 + Count * 12) throw new Error('Invalid transport count.');
    const Product = path.join(Output, HOST + '-wvstage.' + EXTENSION);
    await Wrapper('Package-Hosted-Wvb', ['image', '8', Privateˉwvb, Image, String(Count), String(Entry), Product, WINDOWS ? 'windows' : 'linux']);
    const Productˉbytes = await Readˉboundedˉhostedˉfile(Product, 'reconstructed staging executable');
    if (Digest(Productˉbytes) !== PRODUCT_SHA256) throw new Error('Reconstructed staging executable differs.');
    if (Verify) {
        const Repeated = path.join(Output, 'Repeated');
        await Run('self-staging', Product, [Privateˉwvb, Repeated, Repeated + '.wvop']);
        const First = await Readˉboundedˉhostedˉfile(Object + '.wvop', 'object manifest', 4096);
        const Second = await Readˉboundedˉhostedˉfile(Repeated + '.wvop', 'repeated object manifest', 4096);
        if (!First.equals(Second)) throw new Error('Self-staging manifest differs.');
        const Chunks = First.readUInt32LE(16);
        if (Chunks > 128) throw new Error('Self-staging exceeds the chunk bound.');
        for (let Index = 0; Index < Chunks; Index += 1) {
            const Left = await Readˉboundedˉhostedˉfile(Object + '.chunk-' + Index, 'object chunk', 4_194_304);
            const Right = await Readˉboundedˉhostedˉfile(Repeated + '.chunk-' + Index, 'repeated chunk', 4_194_304);
            if (!Left.equals(Right)) throw new Error('Self-staging chunk differs.');
        }
        for (const [Name, Expected] of [
            ['Return-42', '53f3218e2a9e19ce8e2d470267a3d73af569a3a918d2949042b6c926564ae5b3'],
            ['Metadata', 'f8264e4b56fea680d3456adc84b4a12b217a6001accf57ac1edb03133b097144'],
        ]) {
            const Prefix = path.join(Output, Name);
            await Run(Name, Product, [path.join(ROOT, 'Artifacts/Native-Wvb-To-Wvo-Candidate', Name + '.wvb'), Prefix, Prefix + '.wvop']);
            const Record = await Readˉboundedˉhostedˉfile(Prefix + '.wvop', 'smoke manifest', 4096);
            if (Record.readUInt32LE(16) !== 3) throw new Error('Smoke chunk count differs.');
            const Parts = [];
            for (let Index = 0; Index < 3; Index += 1) Parts.push(await Readˉboundedˉhostedˉfile(Prefix + '.chunk-' + Index, 'smoke chunk', 4096));
            if (Digest(Buffer.concat(Parts)) !== Expected) throw new Error('Segmented and monolithic WVO differ.');
        }
        const Malformed = path.join(Output, 'Truncated.wvb');
        await writeFile(Malformed, Bytes.subarray(0, 11), { flag: 'wx' });
        const Rejected = await Runˉdevelopmentˉcommand(Product,
            [Malformed, path.join(Output, 'Invalid'), path.join(Output, 'Invalid.wvop')], Deadline, false, 65_536);
        if (Rejected.Code === 0 || Rejected.Code === 124 ||
            await lstat(path.join(Output, 'Invalid.wvop')).then(() => true, Error => {
                if (Error.code === 'ENOENT') return false;
                throw Error;
            })) throw new Error('Truncated WVB was not rejected without publication.');
        console.log('staging reconstruction status=Passed cases=6 source-compilation=separate retained-wvb=verified');
    }
    console.log(`staging reconstruction status=Complete host=${HOST} elapsed-ms=${Date.now() - Started}`);
}

function Digest(Bytes) { return createHash('sha256').update(Bytes).digest('hex'); }

if (process.argv[1] && path.resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
    const Verify = process.argv[2] === '--verify';
    const Arguments = process.argv.slice(Verify ? 3 : 2);
    if (Arguments.length !== 1 || Arguments[0].length === 0) {
        console.error('Usage: node Tools/Native/Construct-Segmented-Staging.mjs [--verify] <existing-separate-output-directory>');
        process.exitCode = 64;
    } else {
        try { await Constructˉsegmentedˉstaging(Arguments[0], Date.now() + 600_000, Verify); }
        catch (Error) { console.error(Error.message); process.exitCode = 1; }
    }
}
