import { TextDecoder } from 'node:util';

const UTF8 = new TextDecoder('utf-8', { fatal: true });

function Reject(Message) {
    throw new Error(Message);
}

function Moduleˉidentity(Source, Index) {
    let Text;
    try {
        Text = UTF8.decode(Source);
    } catch {
        Reject(`The split compiler source module ${Index} is not valid UTF-8.`);
    }
    if (Text.startsWith('#!')) {
        const Lineˉend = Text.indexOf('\n');
        if (Lineˉend < 0) {
            Reject(`The split compiler source module ${Index} has no module declaration.`);
        }
        Text = Text.slice(Lineˉend + 1);
    }
    const Match = /^module[ \t]+([^\s;]+)(?:[ \t]|;)/u.exec(Text);
    if (Match === null) {
        Reject(`The split compiler source module ${Index} has no leading module identity.`);
    }
    return Buffer.from(Match[1], 'utf8');
}

export function Orderˉsplitˉprojectˉsourceˉpayloads(Payloads) {
    if (!Array.isArray(Payloads) || Payloads.length < 1 ||
        Payloads.length > 64 || Payloads.some(Payload => !Buffer.isBuffer(Payload))) {
        Reject('The split compiler source payload collection is invalid.');
    }
    const Root = Payloads[0];
    const Dependencies = Payloads.slice(1).map((Payload, Offset) => ({
        identity: Moduleˉidentity(Payload, Offset + 1),
        payload: Payload,
    }));
    Dependencies.sort((Left, Right) =>
        Buffer.compare(Left.identity, Right.identity));
    return [Root, ...Dependencies.map(Dependency => Dependency.payload)];
}
