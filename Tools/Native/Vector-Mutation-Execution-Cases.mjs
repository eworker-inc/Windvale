import { createHash } from 'node:crypto';
import { readFileSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';
import { Buildˉvectorˉmutationˉrejections } from './Native-Owned-Helper-Cases.mjs';

// The source owner and the cross-host runtime selection use identical products
// and malformed cases. Neither selection reconstructs a compiler implicitly.
export async function Verifyˉvectorˉmutationˉproducts(Context, Paths) {
    const { Work, Verifier, Runner, Requireˉfile, Run, Runˉprocess,
        Parseˉsections, Parseˉfunction, Parseˉentries, Instructionˉwidth } = Context;
    for (const Path of [Verifier, Runner]) Requireˉfile(Path, 134_217_728, 'Vector mutation tool');
    if (Paths.length !== 3) throw new Error('Expected helper, scalar and unreachable mutation products.');
    const Products = Paths.map(Path => {
        Requireˉfile(Path, 1_048_576, 'Vector mutation product');
        return readFileSync(Path);
    });
    function Operations(Bytes, Name) {
        const Sections = Parseˉsections(Bytes), Function = Parseˉfunction(Bytes, Sections[4], Name);
        const Start = Sections[5].payload + Function.codeOffset, End = Start + Function.codeLength;
        const Result = [];
        for (let At = Start; At < End;) {
            const Width = Instructionˉwidth(Bytes, At);
            if (At + Width > End) throw new Error('Mutation instruction exceeds its function.');
            Result.push({ Opcode: Bytes[At], Offset: At });
            At += Width;
        }
        return { Function, Operations: Result };
    }
    const [Helpers, Scalars, Unreachable] = Products;
    if (Helpers.readUInt16LE(6) !== 43 || Scalars.readUInt16LE(6) !== 43 ||
        Unreachable.readUInt16LE(6) === 43 ||
        Parseˉentries(Unreachable, Parseˉsections(Unreachable)[4]).map(Item => Item.name).join(',') !== 'Main') {
        throw new Error('Mutation bytecode version selection differs.');
    }
    const Main = Operations(Helpers, 'Main');
    const Iterations = Main.Operations.filter(Item => Item.Opcode === 1 &&
        Helpers.readInt32LE(Item.Offset + 1) === 1000);
    if (Iterations.length !== 1) throw new Error('Mutation iteration control differs.');
    const Single = Buffer.from(Helpers);
    Single.writeInt32LE(1, Iterations[0].Offset + 1);
    for (const [Name, Shape] of [['I32', 1], ['U8', 4], ['U32', 5], ['Bool', 2], ['I64', 9], ['U64', 10]]) {
        const Mutation = Operations(Scalars, 'Mutateˉ' + Name);
        if (Mutation.Function.returnShape !== Shape ||
            Scalars[Mutation.Function.parameterShapeOffsets[0]] !== 27 ||
            Mutation.Operations.filter(Item => Item.Opcode === 228).length !== 1 ||
            Mutation.Operations.filter(Item => Item.Opcode === 208).length !== 1) {
            throw new Error(`Scalar mutation controls differ: ${Name}.`);
        }
    }
    function Normalize(Text) { return Text.replace(/\r\n/gu, '\n'); }
    async function Valid(Label, Bytes, Trap = false) {
        const Input = join(Work, 'Vector-mutation-' + Label + '.wvb');
        writeFileSync(Input, Bytes, { flag: 'wx' });
        if (Normalize(await Run('mutation-verify-' + Label, Verifier, [Input])) !==
            'wvb status=Valid profile=compiler-aligned\n') throw new Error('Mutation verification differs: ' + Label);
        if (Trap) {
            const Result = await Runˉprocess(Runner, [Input]);
            if (Result.Code !== 1 || Result.Output !== '' ||
                !/^wvb run status=Failed code=3008 instructions=[1-9][0-9]*\n$/u.test(Normalize(Result.Error))) {
                throw new Error(`Mutation bounds did not terminate: ${Label}\n${Result.Output}${Result.Error}`);
            }
        } else if (Normalize(await Run('mutation-execute-' + Label, Runner, [Input])) !== 'Result: 42\n') {
            throw new Error('Mutation result differs: ' + Label);
        }
        process.stdout.write(`PASS Vector mutation case=${Label} kind=${Trap ? 'bounds' : 'execution'}\n`);
    }
    for (const [Label, Bytes] of [['nested-helper', Single], ['six-scalars', Scalars], ['unreachable-helper', Unreachable]]) {
        await Valid(Label, Bytes);
    }
    const Rejections = Buildˉvectorˉmutationˉrejections(Helpers);
    // A read-only helper with the same live scalar loan must remain valid.
    // This control makes the conflicting mutation's rejection causal.
    const Loanˉchange = Rejections.find(([Label]) => Label === 'replacement-invalidates-held-loan')[1];
    const Loanˉcontrol = Loanˉchange(Buffer.from(Single));
    const Loan = Operations(Loanˉcontrol, 'Transfer');
    const Mutation = Loan.Operations.find(Item => Item.Opcode === 228);
    const Loanˉsections = Parseˉsections(Loanˉcontrol);
    const Loanˉstart = Loanˉsections[5].payload + Loan.Function.codeOffset;
    const Loanˉend = Loanˉstart + Loan.Function.codeLength;
    if (Mutation === undefined || Loanˉend !== Loanˉsections[5].payload + Loanˉsections[5].length ||
        Loanˉcontrol[Mutation.Offset - 14] !== 129 || Loanˉcontrol[Mutation.Offset - 5] !== 1) {
        throw new Error('Held-loan mutation control layout differs.');
    }
    Loanˉcontrol.writeUInt32LE(Loan.Function.codeLength - 18, Loan.Function.metadataOffset + 4);
    Loanˉcontrol.writeUInt32LE(2, Loan.Function.metadataOffset + 8);
    Loanˉcontrol.writeUInt32LE(Loanˉsections[5].length - 18, Loanˉsections[5].header + 4);
    await Valid('held-loan-read-control', Buffer.concat([
        Loanˉcontrol.subarray(0, Mutation.Offset - 14),
        Loanˉcontrol.subarray(Mutation.Offset - 5, Mutation.Offset),
        Loanˉcontrol.subarray(Mutation.Offset + 9),
    ]));
    const Consume = Operations(Single, 'Consume');
    const Index = Consume.Operations.find(Item => Item.Opcode === 129);
    if (Index === undefined || Single.readBigUInt64LE(Index.Offset + 1) !== 1n) {
        throw new Error('Replacement bounds control differs.');
    }
    const Bounds = [['length', 2n], ['high-half-only', 4294967296n]];
    for (const [Label, Indexˉvalue] of Bounds) {
        const Broken = Buffer.from(Single);
        Broken.writeBigUInt64LE(Indexˉvalue, Index.Offset + 1);
        await Valid(Label, Broken, true);
    }
    for (const [Label, Change] of Rejections) {
        let Broken = Buffer.from(Helpers);
        const Changed = Change(Broken);
        if (Buffer.isBuffer(Changed)) Broken = Changed;
        if (Broken.equals(Helpers)) throw new Error('Mutation changed no bytes: ' + Label);
        const Input = join(Work, 'Vector-mutation-malformed-' + Label + '.wvb');
        writeFileSync(Input, Broken, { flag: 'wx' });
        const Invalid = Label === 'replacement-invalidates-held-loan'
            ? /^wvb status=Invalid phase=typed-execution\n$/u
            : /^wvb status=Invalid phase=(?:semantic step=[a-z-]+|typed-execution|control-reachability)\n$/u;
        for (const [Tool, Pattern] of [[Verifier, Invalid],
            [Runner, /^wvb run status=Unsupported profile=portable-main-i32 phase=envelope\n$/u]]) {
            const Result = await Runˉprocess(Tool, [Input]);
            if (Result.Code !== 1 || Result.Output !== '' || !Pattern.test(Normalize(Result.Error))) {
                throw new Error(`Malformed mutation did not reject: ${Label}\n${Result.Output}${Result.Error}`);
            }
        }
        process.stdout.write(`PASS Vector mutation malformed case=${Label}\n`);
    }
    for (const [Index, Bytes] of Products.entries()) process.stdout.write(
        `Vector mutation input=${Paths[Index]} bytes=${Bytes.length} sha256=${createHash('sha256').update(Bytes).digest('hex')}\n`);
    const Result = { Cases: 4 + Bounds.length + Rejections.length, Executions: 4,
        Bounds: Bounds.length, Malformed: Rejections.length };
    process.stdout.write(`native Vector mutation products status=Passed cases=${Result.Cases} executions=${Result.Executions} ` +
        `bounds=${Result.Bounds} malformed=${Result.Malformed} host=${process.platform} qualification=false\n`);
    return Result;
}
