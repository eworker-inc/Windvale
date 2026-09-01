import {
    existsSync,
    mkdirSync,
    mkdtempSync,
    readdirSync,
    rmSync,
    symlinkSync,
    utimesSync,
    writeFileSync,
} from 'node:fs';
import { execFileSync } from 'node:child_process';
import os from 'node:os';
import path from 'node:path';
import {
    Requireˉordinaryˉdirectoryˉpath,
    Requireˉordinaryˉnewˉpath,
} from './Verification-Owner-Stream-Path.mjs';
import {
    Getˉverificationˉresultˉpath,
    Prepareˉverificationˉresultˉcache,
    Probeˉverificationˉresult,
    Publishˉverificationˉresult,
} from './Verification-Owner-Result-Cache.mjs';

if (process.argv.length !== 2) {
    process.stderr.write(
        'Usage: node Tools/Native/Test-Verification-Owner-Stream.mjs\n'
    );
    process.exit(64);
}

const Work = mkdtempSync(path.join(
    os.tmpdir(), 'windvale-verification-owner-stream-',
));

try {
    const Fresh = path.join(Work, 'Fresh.log');
    if (await Requireˉordinaryˉnewˉpath(Fresh) !== path.resolve(Fresh)) {
        Reject('The fresh log path did not retain its resolved identity.');
    }
    Pass('ordinary-new-path');

    writeFileSync(Fresh, 'occupied', { encoding: 'utf8', flag: 'wx' });
    await Requireˉrejection(
        () => Requireˉordinaryˉnewˉpath(Fresh),
        'Owner log already exists:',
    );
    Pass('existing-path');

    const Ordinaryˉdirectory = path.join(Work, 'Ordinary');
    mkdirSync(Ordinaryˉdirectory);
    if (await Requireˉordinaryˉdirectoryˉpath(Ordinaryˉdirectory) !==
        path.resolve(Ordinaryˉdirectory)) {
        Reject('The ordinary directory did not retain its resolved identity.');
    }
    Pass('ordinary-directory');

    const Linkˉtarget = path.join(Work, 'Link-Target');
    const Link = path.join(Work, 'Link');
    mkdirSync(Linkˉtarget);
    symlinkSync(
        Linkˉtarget,
        Link,
        process.platform === 'win32' ? 'junction' : 'dir',
    );
    await Requireˉrejection(
        () => Requireˉordinaryˉnewˉpath(path.join(Link, 'Linked.log')),
        'Owner path must not traverse a link or non-directory:',
    );
    Pass('linked-parent-rejection');

    await Verifyˉresultˉcache(Work);
    Pass('persistent-result-cache');

    process.stdout.write(
        'verification owner stream status=Passed cases=5\n'
    );
} finally {
    rmSync(Work, { recursive: true, force: true });
}

async function Verifyˉresultˉcache(Work) {
    const Repository = path.join(Work, 'Repository');
    const Cache = path.join(Work, 'Result-Cache');
    const Source = path.join(Repository, 'Source.txt');
    const Owner = 'verification-owner-stream';
    const Action = JSON.stringify({
        format: 'windvale-verification-owner-action-1',
        suite: Owner,
        command: 'Tools/Native/Test-Verification-Owner-Stream',
        arguments: [],
        scope: 'development',
    });
    mkdirSync(Repository);
    Runˉgit(Repository, ['init', '--quiet']);
    Runˉgit(Repository, ['config', 'core.autocrlf', 'false']);
    writeFileSync(Source, 'first\n', 'utf8');
    Runˉgit(Repository, ['add', '--', 'Source.txt']);
    Runˉgit(Repository, [
        '-c', 'user.name=Windvale Verification',
        '-c', 'user.email=verification.invalid@example.invalid',
        'commit', '--quiet', '-m', 'first',
    ]);

    const Insideˉrepository = path.join(Repository, 'Result-Cache');
    await Requireˉrejection(
        () => Prepareˉverificationˉresultˉcache(
            Repository,
            Insideˉrepository,
        ),
        'Verification result cache root must remain outside the repository.',
    );
    if (existsSync(Insideˉrepository)) {
        Reject('Rejected result-cache setup left a directory in the repository.');
    }
    const Linkedˉcache = path.join(Work, 'Link', 'Verification-Result-Cache');
    await Requireˉrejection(
        () => Prepareˉverificationˉresultˉcache(Repository, Linkedˉcache),
        'Verification result cache path is linked or not a directory:',
    );
    if (existsSync(path.join(Work, 'Link-Target', 'Verification-Result-Cache'))) {
        Reject('Rejected linked result-cache setup wrote through its link.');
    }

    const First = await Prepareˉverificationˉresultˉcache(Repository, Cache);
    if (await Probeˉverificationˉresult(
        First.root, First.stateKey, Owner, Action
    )) {
        Reject('A verification result existed before publication.');
    }
    if (await Publishˉverificationˉresult(
        Repository,
        First.root,
        First.stateKey,
        First.sourceTree,
        First.sourceSentinel,
        Owner,
        Action,
    ) !== 'Stored' || !(await Probeˉverificationˉresult(
        First.root, First.stateKey, Owner, Action
    ))) {
        Reject('A published verification result was not reusable.');
    }

    Runˉgit(Repository, [
        '-c', 'user.name=Windvale Verification',
        '-c', 'user.email=verification.invalid@example.invalid',
        'commit', '--quiet', '--allow-empty', '-m', 'same-tree',
    ]);
    const Sameˉtree = await Prepareˉverificationˉresultˉcache(
        Repository,
        Cache,
    );
    if (Sameˉtree.stateKey !== First.stateKey ||
        Sameˉtree.sourceTree !== First.sourceTree ||
        Sameˉtree.sourceSentinel !== First.sourceSentinel) {
        Reject('A commit-only change invalidated the exact source-state result.');
    }

    writeFileSync(Source, 'commit-transition\n', 'utf8');
    const Dirtyˉtransition = await Prepareˉverificationˉresultˉcache(
        Repository,
        Cache,
    );
    Runˉgit(Repository, ['add', '--', 'Source.txt']);
    Runˉgit(Repository, [
        '-c', 'user.name=Windvale Verification',
        '-c', 'user.email=verification.invalid@example.invalid',
        'commit', '--quiet', '-m', 'commit-transition',
    ]);
    const Committedˉtransition = await Prepareˉverificationˉresultˉcache(
        Repository,
        Cache,
    );
    if (Dirtyˉtransition.stateKey !== Committedˉtransition.stateKey ||
        Dirtyˉtransition.sourceTree !== Committedˉtransition.sourceTree ||
        Dirtyˉtransition.sourceSentinel ===
            Committedˉtransition.sourceSentinel) {
        Reject('Committing identical working bytes invalidated the result state.');
    }
    writeFileSync(Source, 'first\n', 'utf8');
    Runˉgit(Repository, ['add', '--', 'Source.txt']);
    Runˉgit(Repository, [
        '-c', 'user.name=Windvale Verification',
        '-c', 'user.email=verification.invalid@example.invalid',
        'commit', '--quiet', '-m', 'restore-first',
    ]);
    const Restoredˉcommit = await Prepareˉverificationˉresultˉcache(
        Repository,
        Cache,
    );
    if (Restoredˉcommit.stateKey !== First.stateKey ||
        Restoredˉcommit.sourceSentinel !== First.sourceSentinel) {
        Reject('Restoring committed source did not restore its result state.');
    }

    writeFileSync(Source, 'changed\n', 'utf8');
    const Changed = await Prepareˉverificationˉresultˉcache(Repository, Cache);
    if (Changed.stateKey === First.stateKey ||
        Changed.sourceSentinel === First.sourceSentinel ||
        await Probeˉverificationˉresult(
            Changed.root, Changed.stateKey, Owner, Action
        )) {
        Reject('Changed source reused an earlier verification result.');
    }
    if (await Publishˉverificationˉresult(
        Repository,
        First.root,
        First.stateKey,
        First.sourceTree,
        First.sourceSentinel,
        'verification-owner-cache-changed-state',
        Action,
    ) !== 'StateChanged') {
        Reject('Publication accepted a result after the source state changed.');
    }
    writeFileSync(Source, 'first\n', 'utf8');

    const Untracked = path.join(Repository, 'Untracked.txt');
    writeFileSync(Untracked, 'untracked\n', 'utf8');
    const Withˉuntracked = await Prepareˉverificationˉresultˉcache(
        Repository,
        Cache,
    );
    if (Withˉuntracked.stateKey === First.stateKey ||
        Withˉuntracked.sourceSentinel === First.sourceSentinel) {
        Reject('Untracked source content did not change the result-cache state.');
    }
    rmSync(Untracked);
    const Restored = await Prepareˉverificationˉresultˉcache(Repository, Cache);
    if (Restored.stateKey !== First.stateKey ||
        Restored.sourceSentinel !== First.sourceSentinel) {
        Reject('Removing untracked source did not restore the exact state.');
    }

    const Resultˉpath = Getˉverificationˉresultˉpath(
        First.root,
        First.stateKey,
        Owner,
        Action,
    );
    writeFileSync(Resultˉpath, '{invalid', 'utf8');
    if (await Probeˉverificationˉresult(
        First.root, First.stateKey, Owner, Action
    )) {
        Reject('A corrupt verification result was accepted.');
    }
    mkdirSync(Resultˉpath);
    let Publicationˉfailed = false;
    try {
        await Publishˉverificationˉresult(
            Repository,
            First.root,
            First.stateKey,
            First.sourceTree,
            First.sourceSentinel,
            Owner,
            Action,
        );
    } catch {
        Publicationˉfailed = true;
    }
    const Stateˉdirectory = path.dirname(Resultˉpath);
    if (!Publicationˉfailed ||
        readdirSync(Stateˉdirectory).some(Name => Name.startsWith('.new-'))) {
        Reject('A failed result publication retained temporary debris.');
    }
    rmSync(Resultˉpath, { recursive: true });
    await Promise.all([
        Publishˉverificationˉresult(
            Repository,
            First.root,
            First.stateKey,
            First.sourceTree,
            First.sourceSentinel,
            Owner,
            Action,
        ),
        Publishˉverificationˉresult(
            Repository,
            First.root,
            First.stateKey,
            First.sourceTree,
            First.sourceSentinel,
            Owner,
            Action,
        ),
    ]);
    if (!(await Probeˉverificationˉresult(
        First.root, First.stateKey, Owner, Action
    )) || readdirSync(Stateˉdirectory).some(Name => Name.startsWith('.new-'))) {
        Reject('A publication race damaged the result or retained temporary debris.');
    }

    const Family = path.join(Cache, 'owner-result-v1');
    const Old = new Date(Date.now() - (8 * 24 * 60 * 60 * 1000));
    for (let Index = 0; Index < 20; Index += 1) {
        const Name = Index.toString(16).padStart(64, '0');
        const Directory = path.join(Family, Name);
        mkdirSync(Directory, { recursive: false });
        utimesSync(Directory, Old, Old);
    }
    await Prepareˉverificationˉresultˉcache(Repository, Cache);
    const Retained = readdirSync(Family, { withFileTypes: true })
        .filter(Entry => Entry.isDirectory() && /^[0-9a-f]{64}$/.test(Entry.name));
    if (Retained.length > 16) {
        Reject('Verification result cache retention exceeded its state bound.');
    }
}

function Runˉgit(Repository, Arguments) {
    execFileSync('git', ['-C', Repository, ...Arguments], {
        encoding: 'utf8',
        stdio: ['ignore', 'pipe', 'pipe'],
        windowsHide: true,
    });
}

async function Requireˉrejection(Action, Prefix) {
    try {
        await Action();
    } catch (Errorˉvalue) {
        if (Errorˉvalue instanceof Error &&
            Errorˉvalue.message.startsWith(Prefix)) {
            return;
        }
        throw Errorˉvalue;
    }
    Reject(`Expected rejection beginning with: ${Prefix}`);
}

function Pass(Name) {
    process.stdout.write(`PASS  verification owner stream case=${Name}\n`);
}

function Reject(Message) {
    throw new Error(Message);
}
