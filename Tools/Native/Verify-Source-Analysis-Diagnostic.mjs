import { lstatSync, readFileSync } from 'node:fs';
import { TextDecoder } from 'node:util';

const MAXIMUM_DIAGNOSTIC_BYTES = 4096;
const MAXIMUM_U32 = 4_294_967_295n;

if (process.argv.length !== 5) {
    process.stderr.write(
        'Usage: node Verify-Source-Analysis-Diagnostic.mjs ' +
        '<diagnostic-file> <wir|symbols> <status>\n',
    );
    process.exit(64);
}

const Path = process.argv[2];
const Phase = process.argv[3];
const Status = process.argv[4];
if (Phase !== 'wir' && Phase !== 'symbols') {
    Reject('The expected source-analysis phase is invalid.');
}
if (!/^[A-Za-z][A-Za-z0-9]*(?:(?:-|ˉ)[A-Za-z][A-Za-z0-9]*)*$/u.test(Status)) {
    Reject('The expected source-analysis status is invalid.');
}
const Canonicalˉstatus = Status.replaceAll('-', 'ˉ');

const Information = lstatSync(Path);
if (!Information.isFile() || Information.isSymbolicLink() ||
    Information.size < 1 || Information.size > MAXIMUM_DIAGNOSTIC_BYTES) {
    Reject('The diagnostic is not a bounded ordinary file.');
}

const Diagnostic = new TextDecoder('utf-8', { fatal: true }).decode(
    readFileSync(Path),
);
const Prefix = Phase === 'wir'
    ? 'source analysis status=Sourceˉwir symbol-status=Valid ' +
        `binding-status=Valid wir-status=${Canonicalˉstatus}`
    : 'source analysis status=Sourceˉsymbols ' +
        `symbol-status=${Canonicalˉstatus} ` +
        'binding-status=Sourceˉsymbols wir-status=Sourceˉbindings ' +
        'graph-status=Valid';
const Locationˉpattern = Phase === 'wir'
    ? ' failure-module=(0|[1-9][0-9]*)' +
        ' related-module=(0|[1-9][0-9]*)' +
        ' function=(0|[1-9][0-9]*)' +
        ' offset=(0|[1-9][0-9]*)' +
        ' line=(0|[1-9][0-9]*)' +
        ' column=(0|[1-9][0-9]*)'
    : ' failure-module=(0|[1-9][0-9]*)' +
        ' related-module=(0|[1-9][0-9]*)' +
        ' declaration-kind=([A-Za-z][A-Za-z0-9ˉ]*)' +
        ' offset=(0|[1-9][0-9]*)' +
        ' line=(0|[1-9][0-9]*)' +
        ' column=(0|[1-9][0-9]*)';
const Match = new RegExp(
    '^' + Escapeˉregularˉexpression(Prefix) + Locationˉpattern +
    '\\r?\\n$', 'u',
).exec(Diagnostic);
if (Match === null) {
    Reject('The source-analysis diagnostic differs.');
}
const Numericˉvalues = Phase === 'wir'
    ? Match.slice(1)
    : [Match[1], Match[2], Match[4], Match[5], Match[6]];
for (const Value of Numericˉvalues) {
    if (BigInt(Value) > MAXIMUM_U32) {
        Reject('A source-analysis diagnostic location exceeds u32.');
    }
}

function Escapeˉregularˉexpression(Value) {
    return Value.replace(/[.*+?^${}()|[\]\\]/gu, '\\$&');
}

function Reject(Message) {
    throw new Error(Message);
}
