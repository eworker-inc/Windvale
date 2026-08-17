import { createHash, randomBytes } from 'node:crypto';
import {
    chmod,
    copyFile,
    lstat,
    mkdir,
    readFile,
    realpath,
    readdir,
    stat,
    unlink,
    writeFile
} from 'node:fs/promises';
import net from 'node:net';
import os from 'node:os';
import path from 'node:path';
import {
    Materializeˉprojectˉobjectˉhit,
    Prepareˉprojectˉobjectˉcache
} from './Build-Cached-Project-Object.mjs';
import {
    Getˉhostedˉapplicationˉcacheˉkey,
    HOSTED_REPOSITORY_ROOT,
    Isˉsameˉhostedˉpath,
    MAXIMUM_HOSTED_INPUT_BYTES,
    Prepareˉhostedˉapplicationˉcontext,
    Readˉboundedˉhostedˉfile
} from './Native-Hosted-Application-Cache-Core.mjs';

const MAXIMUM_PROTOCOL_BYTES = 32_768;
const WINDOWS = process.platform === 'win32';
const HOST_FAMILY = WINDOWS ? 'windows-x64' : 'linux-x64';
const RECORD_ENDING = WINDOWS ? '\r\n' : '\n';
const PACKAGER = path.join(
    HOSTED_REPOSITORY_ROOT,
    'Tools',
    'Native',
    `Package-Hosted-Wvb.${WINDOWS ? 'cmd' : 'sh'}`
);

function Reject(message, exitCode = 1) {
    const error = new Error(message);
    error.exitCode = exitCode;
    throw error;
}

function Delay(milliseconds) {
    return new Promise(resolve => setTimeout(resolve, milliseconds));
}

async function Requireˉcanonicalˉdirectory(candidate, label) {
    const absolute = path.resolve(candidate);
    const information = await lstat(absolute).catch(() => null);
    if (information === null || !information.isDirectory() ||
        information.isSymbolicLink()) {
        Reject(`The ${label} is not an ordinary directory: ${absolute}`);
    }
    const canonical = await realpath(absolute);
    if (!Isˉsameˉhostedˉpath(canonical, absolute)) {
        Reject(`The ${label} must use its canonical non-link path: ${absolute}`);
    }
    return absolute;
}

async function Ensureˉcanonicalˉdirectory(candidate, label) {
    await mkdir(candidate, { recursive: true });
    return Requireˉcanonicalˉdirectory(candidate, label);
}

async function Requireˉoutputˉpath(candidate, target) {
    const absolute = path.resolve(candidate);
    const extension = target === 'windows' ? '.exe' : '.elf';
    if (path.extname(absolute).toLowerCase() !== extension) {
        Reject(`The hosted application output must use the ${extension} extension.`);
    }
    await Requireˉcanonicalˉdirectory(
        path.dirname(absolute),
        'hosted application output parent'
    );
    const information = await lstat(absolute).catch(error => {
        if (error.code === 'ENOENT') {
            return null;
        }
        throw error;
    });
    if (information !== null && (!information.isFile() ||
        information.isSymbolicLink())) {
        Reject(`The hosted application output is not an ordinary file: ${absolute}`);
    }
    if (information !== null) {
        const canonical = await realpath(absolute);
        if (!Isˉsameˉhostedˉpath(canonical, absolute)) {
            Reject(
                `The hosted application output must use its canonical path: ${absolute}`
            );
        }
    }
    return absolute;
}

async function Measureˉapplication(candidate, label) {
    const bytes = await Readˉboundedˉhostedˉfile(
        candidate,
        label,
        MAXIMUM_HOSTED_INPUT_BYTES,
        false,
        true
    );
    return {
        bytes: bytes.length,
        sha256: createHash('sha256').update(bytes).digest('hex')
    };
}

function Checkpointˉrecord(key, target, application) {
    return Buffer.from([
        'windvale-native-hosted-application-checkpoint 1',
        `key ${key}`,
        `target ${target}`,
        `application-bytes ${application.bytes}`,
        `application-sha256 ${application.sha256}`,
        ''
    ].join(RECORD_ENDING), 'ascii');
}

async function Validateˉcheckpoint(directory, key, target) {
    await Requireˉcanonicalˉdirectory(directory, 'hosted application checkpoint');
    const productLeaf = target === 'windows' ? 'Product.exe' : 'Product.elf';
    const entries = (await readdir(directory)).sort();
    const expectedEntries = ['Checkpoint.txt', productLeaf].sort();
    if (entries.length !== expectedEntries.length ||
        entries.some((entry, index) => entry !== expectedEntries[index])) {
        Reject(`The hosted application checkpoint has unexpected entries: ${directory}`);
    }
    const manifestPath = path.join(directory, 'Checkpoint.txt');
    const productPath = path.join(directory, productLeaf);
    const record = await Readˉboundedˉhostedˉfile(
        manifestPath,
        'hosted application checkpoint record',
        1_024
    );
    const application = await Measureˉapplication(
        productPath,
        'hosted application checkpoint product'
    );
    if (!record.equals(Checkpointˉrecord(key, target, application))) {
        Reject(`The hosted application checkpoint record differs: ${directory}`);
    }
    return { application, productPath };
}

function Readyˉrecord(port, token, target) {
    return Buffer.from([
        'windvale-native-hosted-application-session 1',
        `host ${HOST_FAMILY}`,
        `target ${target}`,
        `port ${port}`,
        `token ${token}`,
        `pid ${process.pid}`,
        ''
    ].join(RECORD_ENDING), 'ascii');
}

function Parseˉreadyˉrecord(bytes) {
    const text = bytes.toString('ascii');
    if (!Buffer.from(text, 'ascii').equals(bytes)) {
        Reject('The hosted application session record is not ASCII.');
    }
    const lines = text.split(RECORD_ENDING);
    if (lines.length !== 7 || lines[0] !==
        'windvale-native-hosted-application-session 1' ||
        lines[1] !== `host ${HOST_FAMILY}` ||
        !/^target (windows|linux)$/.test(lines[2]) ||
        !/^port ([1-9][0-9]{0,4})$/.test(lines[3]) ||
        !/^token ([0-9a-f]{64})$/.test(lines[4]) ||
        !/^pid ([1-9][0-9]*)$/.test(lines[5]) || lines[6] !== '') {
        Reject('The hosted application session record differs.');
    }
    const port = Number(lines[3].slice('port '.length));
    const pid = Number(lines[5].slice('pid '.length));
    if (port > 65_535 || !Number.isSafeInteger(pid)) {
        Reject('The hosted application session record exceeds its bounds.');
    }
    return {
        pid,
        port,
        target: lines[2].slice('target '.length),
        token: lines[4].slice('token '.length)
    };
}

async function Readˉreadyˉrecord(readyInput) {
    const readyPath = path.resolve(readyInput);
    const bytes = await Readˉboundedˉhostedˉfile(
        readyPath,
        'hosted application session record',
        1_024
    );
    return { readyPath, ...Parseˉreadyˉrecord(bytes) };
}

function Isˉprocessˉalive(pid) {
    try {
        process.kill(pid, 0);
        return true;
    } catch (error) {
        if (error.code === 'ESRCH') {
            return false;
        }
        throw error;
    }
}

async function Waitˉforˉready(readyInput) {
    let lastError;
    for (let attempt = 0; attempt < 200; attempt += 1) {
        try {
            const ready = await Readˉreadyˉrecord(readyInput);
            if (!Isˉprocessˉalive(ready.pid)) {
                Reject('The hosted application session exited before readiness.');
            }
            return ready;
        } catch (error) {
            lastError = error;
            await Delay(25);
        }
    }
    Reject(`The hosted application session did not become ready: ${lastError.message}`);
}

async function Removeˉreadyˉrecord(readyPath) {
    const information = await lstat(readyPath).catch(error => {
        if (error.code === 'ENOENT') {
            return null;
        }
        throw error;
    });
    if (information === null) {
        return;
    }
    if (!information.isFile() || information.isSymbolicLink()) {
        Reject(`The hosted application session record is not ordinary: ${readyPath}`);
    }
    const canonical = await realpath(readyPath);
    if (!Isˉsameˉhostedˉpath(canonical, readyPath)) {
        Reject(`The hosted application session record escaped its path: ${readyPath}`);
    }
    await unlink(readyPath);
}

async function Materializeˉhit(state, request) {
    if (request.target !== state.context.target) {
        Reject('The hosted application request target differs from its session.');
    }
    const outputPath = await Requireˉoutputˉpath(
        request.outputPath,
        request.target
    );
    const key = await Getˉhostedˉapplicationˉcacheˉkey(
        state.context,
        {
            namespace: 'hosted-application-v1',
            profile: request.profile,
            inputPath: request.inputPath,
            chunkPrefix: request.chunkPrefix,
            fragmentCountText: request.fragmentCountText,
            entry: request.entry
        }
    );
    const checkpointDirectory = path.join(state.checkpointFamily, key);
    if (await lstat(checkpointDirectory).catch(error => {
        if (error.code === 'ENOENT') {
            return null;
        }
        throw error;
    }) === null) {
        return { status: 'Miss' };
    }
    const checkpoint = await Validateˉcheckpoint(
        checkpointDirectory,
        key,
        request.target
    );
    await copyFile(checkpoint.productPath, outputPath);
    if (!WINDOWS) {
        const mode = (await stat(checkpoint.productPath)).mode & 0o777;
        await chmod(outputPath, mode);
    }
    const materialized = await Measureˉapplication(
        outputPath,
        'materialized hosted application'
    );
    if (materialized.bytes !== checkpoint.application.bytes ||
        materialized.sha256 !== checkpoint.application.sha256) {
        Reject('The materialized hosted application differs from its checkpoint.');
    }
    return {
        status: 'Hit',
        report: `native hosted application cache status=Hit key=${key} ` +
            `target=${request.target}`
    };
}

function Readˉprotocolˉrequest(socket) {
    return new Promise((resolve, reject) => {
        let text = '';
        socket.setEncoding('utf8');
        socket.setTimeout(120_000, () => {
            reject(new Error('The hosted application session request timed out.'));
            socket.destroy();
        });
        socket.on('data', chunk => {
            text += chunk;
            if (Buffer.byteLength(text, 'utf8') > MAXIMUM_PROTOCOL_BYTES) {
                reject(new Error('The hosted application session request is oversized.'));
                socket.destroy();
                return;
            }
            const newline = text.indexOf('\n');
            if (newline < 0) {
                return;
            }
            if (newline + 1 !== text.length) {
                reject(new Error('The hosted application session request is not one line.'));
                socket.destroy();
                return;
            }
            try {
                resolve(JSON.parse(text.slice(0, newline)));
            } catch {
                reject(new Error('The hosted application session request is not JSON.'));
            }
        });
        socket.on('error', reject);
        socket.on('end', () => {
            if (!text.endsWith('\n')) {
                reject(new Error('The hosted application session request is truncated.'));
            }
        });
    });
}

function Writeˉprotocolˉresponse(socket, response) {
    socket.end(`${JSON.stringify(response)}\n`);
}

async function Serve(readyInput, target, buildDriverInput, lowererInput) {
    if (target !== (WINDOWS ? 'windows' : 'linux')) {
        Reject('The hosted application session must use its current-host target.', 64);
    }
    const readyPath = path.resolve(readyInput);
    await Requireˉcanonicalˉdirectory(
        path.dirname(readyPath),
        'hosted application session parent'
    );
    if (await lstat(readyPath).catch(() => null) !== null) {
        Reject(`The hosted application session record already exists: ${readyPath}`);
    }
    const context = await Prepareˉhostedˉapplicationˉcontext(target, PACKAGER);
    if (WINDOWS && process.env.WINDVALE_NATIVE_CACHE_ROOT === undefined &&
        process.env.LOCALAPPDATA === undefined) {
        Reject('The native hosted application cache root is unavailable.');
    }
    const configuredRoot = process.env.WINDVALE_NATIVE_CACHE_ROOT ?? (
        WINDOWS
            ? path.join(process.env.LOCALAPPDATA, 'Windvale', 'Native-Tool-Cache')
            : path.join(
                process.env.XDG_CACHE_HOME ?? path.join(os.homedir(), '.cache'),
                'windvale',
                'native-tool-cache'
            )
    );
    const checkpointRoot = await Ensureˉcanonicalˉdirectory(
        path.resolve(configuredRoot),
        'hosted application checkpoint root'
    );
    const productRoot = await Ensureˉcanonicalˉdirectory(
        path.join(checkpointRoot, 'hosted-application-v1'),
        'hosted application checkpoint product root'
    );
    const checkpointFamily = await Ensureˉcanonicalˉdirectory(
        path.join(productRoot, HOST_FAMILY),
        'hosted application checkpoint family'
    );
    if ((buildDriverInput === undefined) !== (lowererInput === undefined)) {
        Reject('Project-object session producers must be supplied together.', 64);
    }
    const projectContext = buildDriverInput === undefined
        ? null
        : await Prepareˉprojectˉobjectˉcache(buildDriverInput, lowererInput);
    const token = randomBytes(32).toString('hex');
    const state = { checkpointFamily, context, projectContext, token };
    let requestTail = Promise.resolve();
    let shuttingDown = false;
    const server = net.createServer(socket => {
        requestTail = requestTail.then(async () => {
            try {
                const request = await Readˉprotocolˉrequest(socket);
                if (request === null || typeof request !== 'object' ||
                    request.token !== token) {
                    Reject('The hosted application session request token differs.');
                }
                if (request.operation === 'shutdown') {
                    if (shuttingDown) {
                        Reject('The hosted application session is already stopping.');
                    }
                    shuttingDown = true;
                    await Removeˉreadyˉrecord(readyPath);
                    Writeˉprotocolˉresponse(socket, { ok: true });
                    server.close();
                    return;
                }
                if (request.operation === 'project-object') {
                    if (shuttingDown || state.projectContext === null) {
                        Reject('The project-object session operation is unavailable.');
                    }
                    const result = await Materializeˉprojectˉobjectˉhit(
                        state.projectContext,
                        request.projectPath,
                        request.outputWvb,
                        request.outputWvo
                    );
                    Writeˉprotocolˉresponse(socket, result.status === 'Miss'
                        ? { ok: true, status: 'Miss' }
                        : { ok: true, ...result });
                    return;
                }
                if (request.operation !== 'request' || shuttingDown) {
                    Reject('The hosted application session operation is invalid.');
                }
                const result = await Materializeˉhit(state, request);
                Writeˉprotocolˉresponse(socket, { ok: true, ...result });
            } catch (error) {
                Writeˉprotocolˉresponse(socket, {
                    ok: false,
                    error: error.message
                });
            }
        }).catch(error => {
            process.stderr.write(`${error.message}\n`);
            process.exitCode = 1;
            server.close();
        });
    });
    server.on('error', error => {
        process.stderr.write(`${error.message}\n`);
        process.exitCode = 1;
    });
    await new Promise((resolve, reject) => {
        server.once('error', reject);
        server.listen({ host: '127.0.0.1', port: 0, exclusive: true }, resolve);
    });
    const address = server.address();
    await writeFile(
        readyPath,
        Readyˉrecord(address.port, token, target),
        { flag: 'wx' }
    );
    await new Promise(resolve => server.once('close', resolve));
    await Removeˉreadyˉrecord(readyPath);
}

function Sendˉrequest(ready, request) {
    return new Promise((resolve, reject) => {
        const socket = net.createConnection({
            host: '127.0.0.1',
            port: ready.port
        });
        let text = '';
        socket.setEncoding('utf8');
        socket.setTimeout(120_000, () => {
            reject(new Error('The hosted application session response timed out.'));
            socket.destroy();
        });
        socket.on('connect', () => {
            socket.write(`${JSON.stringify({ ...request, token: ready.token })}\n`);
        });
        socket.on('data', chunk => {
            text += chunk;
            if (Buffer.byteLength(text, 'utf8') > MAXIMUM_PROTOCOL_BYTES) {
                reject(new Error('The hosted application session response is oversized.'));
                socket.destroy();
            }
        });
        socket.on('error', reject);
        socket.on('end', () => {
            try {
                if (!text.endsWith('\n') || text.indexOf('\n') !== text.length - 1) {
                    Reject('The hosted application session response is malformed.');
                }
                resolve(JSON.parse(text.slice(0, -1)));
            } catch (error) {
                reject(error);
            }
        });
    });
}

async function Request(readyInput) {
    if (process.argv.length !== 11) {
        Reject(
            'Usage: node Tools/Native/Build-Cached-Hosted-Application-Session.mjs ' +
            'request <ready.txt> <profile> <input.wvb> <chunk-prefix> ' +
            '<fragment-count> <entry> <output.exe|output.elf> <windows|linux>',
            64
        );
    }
    const ready = await Readˉreadyˉrecord(readyInput);
    const target = process.argv[10];
    if (ready.target !== target || !Isˉprocessˉalive(ready.pid)) {
        Reject('The hosted application session is unavailable.');
    }
    const response = await Sendˉrequest(ready, {
        operation: 'request',
        profile: process.argv[4],
        inputPath: path.resolve(process.argv[5]),
        chunkPrefix: path.resolve(process.argv[6]),
        fragmentCountText: process.argv[7],
        entry: process.argv[8],
        outputPath: path.resolve(process.argv[9]),
        target
    });
    if (!response.ok) {
        Reject(response.error);
    }
    if (response.status === 'Miss') {
        process.exit(75);
    }
    if (response.status !== 'Hit' || typeof response.report !== 'string') {
        Reject('The hosted application session response status differs.');
    }
    process.stdout.write(`${response.report}\n`);
}

async function Projectˉrequest(readyInput) {
    if (process.argv.length !== 7) {
        Reject(
            'Usage: node Tools/Native/Build-Cached-Hosted-Application-Session.mjs ' +
            'project-request <ready.txt> <project.wvproj> <output.wvb> <output.wvo>',
            64
        );
    }
    const ready = await Readˉreadyˉrecord(readyInput);
    if (!Isˉprocessˉalive(ready.pid)) {
        Reject('The project-object session is unavailable.');
    }
    const response = await Sendˉrequest(ready, {
        operation: 'project-object',
        outputWvb: path.resolve(process.argv[5]),
        outputWvo: path.resolve(process.argv[6]),
        projectPath: path.resolve(process.argv[4])
    });
    if (!response.ok) {
        Reject(response.error);
    }
    if (response.status === 'Miss') {
        process.exit(75);
    }
    if (response.status !== 'Hit' || typeof response.report !== 'string') {
        Reject('The project-object session response status differs.');
    }
    process.stdout.write(`${response.report}\n`);
}

async function Shutdown(readyInput) {
    const readyPath = path.resolve(readyInput);
    const information = await lstat(readyPath).catch(error => {
        if (error.code === 'ENOENT') {
            return null;
        }
        throw error;
    });
    if (information === null) {
        return;
    }
    const ready = await Readˉreadyˉrecord(readyPath);
    if (Isˉprocessˉalive(ready.pid)) {
        const response = await Sendˉrequest(ready, { operation: 'shutdown' });
        if (!response.ok) {
            Reject(response.error);
        }
    }
    if (WINDOWS) {
        for (let attempt = 0; attempt < 200; attempt += 1) {
            if (!Isˉprocessˉalive(ready.pid)) {
                return;
            }
            await Delay(25);
        }
        Reject('The hosted application session did not stop.');
    }
}

async function Main() {
    const operation = process.argv[2];
    if (operation === 'serve' &&
        (process.argv.length === 5 || process.argv.length === 7)) {
        await Serve(
            process.argv[3],
            process.argv[4],
            process.argv[5],
            process.argv[6]
        );
        return;
    }
    if (operation === 'wait' && process.argv.length === 4) {
        const ready = await Waitˉforˉready(process.argv[3]);
        process.stdout.write(
            `native hosted application session status=Ready target=${ready.target}\n`
        );
        return;
    }
    if (operation === 'request') {
        await Request(process.argv[3]);
        return;
    }
    if (operation === 'project-request') {
        await Projectˉrequest(process.argv[3]);
        return;
    }
    if (operation === 'shutdown' && process.argv.length === 4) {
        await Shutdown(process.argv[3]);
        return;
    }
    Reject(
        'Usage: node Tools/Native/Build-Cached-Hosted-Application-Session.mjs ' +
        '<serve <ready.txt> <windows|linux> [<build-driver> <lowerer>]|' +
        'wait <ready.txt>|request ...|project-request ...|shutdown <ready.txt>>',
        64
    );
}

try {
    await Main();
} catch (error) {
    process.stderr.write(`${error.message}\n`);
    process.exit(error.exitCode ?? 1);
}
