import { lstat } from 'node:fs/promises';
import {
    dirname,
    join,
    parse,
    relative,
    resolve,
    sep,
} from 'node:path';

export async function Requireˉordinaryˉdirectoryˉpath(Path) {
    const Resolved = resolve(Path);
    const Root = parse(Resolved).root;
    const Components = relative(Root, Resolved)
        .split(sep)
        .filter(Component => Component.length !== 0);
    let Current = Root;
    for (const Component of Components) {
        Current = join(Current, Component);
        const Metadata = await lstat(Current);
        if (!Metadata.isDirectory() || Metadata.isSymbolicLink()) {
            throw new Error(
                `Owner path must not traverse a link or non-directory: ${Current}`
            );
        }
    }
    return Resolved;
}

export async function Requireˉordinaryˉnewˉpath(Path) {
    const Resolved = resolve(Path);
    await Requireˉordinaryˉdirectoryˉpath(dirname(Resolved));
    try {
        await lstat(Resolved);
    } catch (Errorˉvalue) {
        if (Errorˉvalue?.code === 'ENOENT') {
            return Resolved;
        }
        throw Errorˉvalue;
    }
    throw new Error(`Owner log already exists: ${Resolved}`);
}
