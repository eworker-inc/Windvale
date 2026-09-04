import { lstatSync, readFileSync, realpathSync } from 'node:fs';
import { createHash } from 'node:crypto';
import path from 'node:path';

const MAXIMUM_WVB_BYTES = 16_777_216;

if (process.argv.length !== 4) {
    Reject(
        'Usage: node Tools/Native/Verify-Language-1.0-Foundation-Value-Borrow-Wvb.mjs ' +
        '<candidate-a.wvb> <candidate-b.wvb>',
    );
}

const Candidateˉa = Requireˉordinaryˉfile(process.argv[2], 'candidate A');
const Candidateˉb = Requireˉordinaryˉfile(process.argv[3], 'candidate B');
const Bytesˉa = readFileSync(Candidateˉa);
const Bytesˉb = readFileSync(Candidateˉb);
if (!Bytesˉa.equals(Bytesˉb)) {
    Reject('The Foundation value-borrow WVB is not deterministic.');
}

const Layout = Inspectˉcandidate(Bytesˉa);
const Mutations = [
    ['prior-minor', Candidateˉmutation(Bytesˉa, Bytes => Bytes.writeUInt16LE(38, 6))],
    ['unknown-opcode', Candidateˉmutation(
        Bytesˉa, Bytes => { Bytes[Layout.opcodes[0]] = 226; },
    )],
    ['zero-projection', Candidateˉmutation(
        Bytesˉa, Bytes => Bytes.writeUInt32LE(0, Layout.opcodes[0] + 9),
    )],
    ['unknown-projection', Candidateˉmutation(
        Bytesˉa, Bytes => Bytes.writeUInt32LE(4, Layout.opcodes[0] + 9),
    )],
    ['owner-out-of-range', Candidateˉmutation(
        Bytesˉa, Bytes => Bytes.writeUInt32LE(0xffff_ffff, Layout.opcodes[0] + 1),
    )],
    ['option-type-out-of-range', Candidateˉmutation(
        Bytesˉa, Bytes => Bytes.writeUInt32LE(Layout.typeCount, Layout.opcodes[0] + 5),
    )],
    ['record-parameter-loses-borrow', Removeˉshapeˉwrapper(Bytesˉa, Layout.recordParameter)],
    ['scalar-parameter-loses-borrow', Removeˉshapeˉwrapper(Bytesˉa, Layout.scalarParameter)],
    ['record-parameter-wrong-type', Candidateˉmutation(Bytesˉa, Bytes =>
        Bytes.writeUInt32LE(Layout.typeCount, Layout.recordParameter + 2))],
    ['call-target-out-of-range', Candidateˉmutation(Bytesˉa, Bytes =>
        Bytes.writeUInt32LE(Layout.functionCount, Layout.calls[0] + 1))],
    ['call-operand-out-of-range', Candidateˉmutation(Bytesˉa, Bytes =>
        Bytes.writeUInt32LE(4096, Layout.calls[0] - 4))],
    ['call-operand-loses-borrow', Removeˉshapeˉwrapper(Bytesˉa, Layout.callOperand)],
];
for (const [Label, Bytes] of Mutations) {
    Requireˉrejected(Bytes, Label);
}

process.stdout.write(
    'foundation value borrow WVB status=Passed cases=20 ' +
    `functions=${Layout.functionCount} types=${Layout.typeCount} ` +
    `opcodes=${Layout.opcodes.length} borrowed-option-shapes=${Layout.viewShapes} ` +
    `borrowed-payload-shapes=${Layout.payloadShapes} ` +
    `borrowed-call-cases=8 direct-calls=${Layout.calls.length} ` +
    `wvb-bytes=${Bytesˉa.length} ` +
    `wvb-sha256=${createHash('sha256').update(Bytesˉa).digest('hex')}\n`,
);

function Inspectˉcandidate(Bytes) {
    if (Bytes.length < 12 || Bytes.length > MAXIMUM_WVB_BYTES ||
        Bytes.subarray(0, 4).toString('ascii') !== 'WVB1' ||
        Bytes.readUInt16LE(4) !== 1 || Bytes.readUInt16LE(6) !== 39 ||
        Bytes.readUInt32LE(8) !== 7) {
        Reject('The candidate is not canonical WVB 1.39.');
    }
    const Sections = Parseˉsections(Bytes);
    const Types = Parseˉtypes(Bytes, Sections[7]);
    const Functions = Parseˉfunctions(Bytes, Sections[4]);
    const Opcodes = [];
    const Calls = [];
    let Callˉoperand = null;
    const Borrowˉhelpers = ['Readˉpayload', 'Readˉu32', 'Forwardˉpayload', 'Forwardˉu32'];
    for (const Name of Borrowˉhelpers) {
        const Function = Functions.find(Entry => Entry.name === Name);
        if (!Function || Function.parameters.length !== 1 ||
            Function.parameters[0].kind !== 37 ||
            Function.parameters[0].inner.kind !== (Name.endsWith('payload') ? 7 : 5) ||
            (Name.endsWith('payload') &&
                Types[Function.parameters[0].inner.typeIndex]?.name !== 'Payload') ||
            !Function.locals.some(Shape => Shapeˉequal(Shape, Function.parameters[0]))) {
            Reject(`The ${Name} declaration or forwarded payload identity differs.`);
        }
    }
    const Owned = Functions.find(Entry => Entry.name === 'Readˉowned');
    if (!Owned || Owned.parameters.length !== 1 || Owned.parameters[0].kind !== 5 ||
        Functions.some(Function => Function.result.kind === 37)) {
        Reject('Borrow metadata escaped into an owned parameter or function result.');
    }
    let Viewˉshapes = 0;
    let Payloadˉshapes = 0;
    for (const Function of Functions) {
        const Slots = [...Function.parameters, ...Function.locals];
        Viewˉshapes += Slots.filter(Shape => Shape.kind === 29).length;
        Payloadˉshapes += Slots.filter(Shape => Shape.kind === 37).length;
        const Codeˉstart = Sections[5].payload + Function.codeOffset;
        const Codeˉend = Codeˉstart + Function.codeLength;
        if (Codeˉstart < Sections[5].payload ||
            Codeˉend > Sections[5].payload + Sections[5].length) {
            Reject(`The ${Function.name} code range is invalid.`);
        }
        let Cursor = Codeˉstart;
        while (Cursor < Codeˉend) {
            const Opcode = Bytes[Cursor];
            const Width = Instructionˉwidth(Bytes, Cursor);
            if (Width === 0 || Cursor + Width > Codeˉend) {
                Reject(`The ${Function.name} instruction stream is invalid.`);
            }
            if (Opcode === 225) {
                const Owner = Bytes.readUInt32LE(Cursor + 1);
                const Optionˉtype = Bytes.readUInt32LE(Cursor + 5);
                const Projection = Bytes.readUInt32LE(Cursor + 9);
                if (Owner >= Slots.length || Optionˉtype >= Types.length ||
                    Projection < 1 || Projection > 3 ||
                    Cursor + Width >= Codeˉend || Bytes[Cursor + Width] !== 5) {
                    Reject('A Foundation value-borrow instruction is malformed.');
                }
                const Option = Types[Optionˉtype];
                if (!Isˉoption(Option)) {
                    Reject('A Foundation value-borrow view does not name Option.');
                }
                const Ownerˉshape = Slots[Owner];
                if (Ownerˉshape.kind !== 11 || Ownerˉshape.typeIndex >= Types.length) {
                    Reject('A Foundation value-borrow owner is not an owned variant.');
                }
                const Ownerˉtype = Types[Ownerˉshape.typeIndex];
                if (Projection === 1) {
                    if (!Isˉoption(Ownerˉtype) ||
                        Ownerˉshape.typeIndex !== Optionˉtype) {
                        Reject('An Option borrow does not preserve its exact type.');
                    }
                } else {
                    if (!Isˉresult(Ownerˉtype)) {
                        Reject('A Result borrow does not name an exact Result owner.');
                    }
                    const Resultˉcase = Projection === 2 ? 0 : 1;
                    if (!Shapeˉequal(
                        Option.cases[0].fields[0],
                        Ownerˉtype.cases[Resultˉcase].fields[0],
                    )) {
                        Reject('A Result borrow payload does not match its Option view.');
                    }
                }
                if (!Slots.some(
                    Shape => Shape.kind === 29 && Shape.typeIndex === Optionˉtype,
                )) {
                    Reject('A Foundation value-borrow instruction has no borrowed view slot.');
                }
                Opcodes.push(Cursor);
            }
            if (Opcode === 64) {
                const Target = Bytes.readUInt32LE(Cursor + 1);
                const Callee = Functions[Target];
                if (!Callee || Callee.parameters.length !== 1 || Cursor - 5 < Codeˉstart ||
                    Bytes[Cursor - 5] !== 4) {
                    Reject('A direct payload helper call has invalid geometry.');
                }
                const Argument = Slots[Bytes.readUInt32LE(Cursor - 4)];
                if (!Argument || !Shapeˉequal(Argument, Callee.parameters[0])) {
                    Reject('A direct helper call loses its exact parameter identity.');
                }
                if (Argument.kind === 37) Callˉoperand ??= Argument.start;
                Calls.push(Cursor);
            }
            Cursor += Width;
        }
        if (Cursor !== Codeˉend) {
            Reject(`The ${Function.name} instruction stream has trailing bytes.`);
        }
    }
    const Projections = Opcodes.map(Offset => Bytes.readUInt32LE(Offset + 9));
    if (Opcodes.length !== 3 || Projections.join(',') !== '1,2,3' ||
        Viewˉshapes < 3 || Payloadˉshapes < 3 || Calls.length !== 8 || Callˉoperand === null) {
        Reject(
            'The Foundation value-borrow WVB shape or opcode inventory differs: ' +
            `opcodes=${Opcodes.length} projections=${Projections.join(',')} ` +
            `views=${Viewˉshapes} payloads=${Payloadˉshapes}.`,
        );
    }
    return {
        functionCount: Functions.length,
        typeCount: Types.length,
        opcodes: Opcodes,
        viewShapes: Viewˉshapes,
        payloadShapes: Payloadˉshapes,
        calls: Calls,
        callOperand: Callˉoperand,
        recordParameter: Functions.find(Function => Function.name === 'Readˉpayload').parameters[0].start,
        scalarParameter: Functions.find(Function => Function.name === 'Readˉu32').parameters[0].start,
    };
}

function Parseˉsections(Bytes) {
    const Result = [];
    let Cursor = 12;
    for (let Expected = 1; Expected <= 7; Expected += 1) {
        if (Cursor + 8 > Bytes.length || Bytes[Cursor] !== Expected ||
            Bytes[Cursor + 1] !== 0 || Bytes.readUInt16LE(Cursor + 2) !== 0) {
            Reject(`The candidate has no canonical section ${Expected}.`);
        }
        const Length = Bytes.readUInt32LE(Cursor + 4);
        const Payload = Cursor + 8;
        if (Payload + Length > Bytes.length) Reject('A WVB section is truncated.');
        Result[Expected] = { payload: Payload, length: Length };
        Cursor = Payload + Length;
    }
    if (Cursor !== Bytes.length) Reject('The candidate has trailing bytes.');
    return Result;
}

function Parseˉfunctions(Bytes, Section) {
    const Count = Readˉu32(Bytes, Section.payload, 'function count');
    if (Count < 1 || Count > 65_536) Reject('The function count is invalid.');
    const Result = [];
    let Cursor = Section.payload + 4;
    for (let Index = 0; Index < Count; Index += 1) {
        const Name = Readˉstring(Bytes, Cursor);
        Cursor = Name.end;
        const Parameterˉcount = Readˉu32(Bytes, Cursor, 'parameter count');
        Cursor += 4;
        if (Parameterˉcount > 64) Reject('A function parameter count is invalid.');
        const Parameters = [];
        for (let Parameter = 0; Parameter < Parameterˉcount; Parameter += 1) {
            const Shape = Readˉshape(Bytes, Cursor);
            Parameters.push(Shape);
            Cursor = Shape.end;
        }
        const Return = Readˉshape(Bytes, Cursor);
        Cursor = Return.end;
        const Localˉcount = Readˉu32(Bytes, Cursor, 'local count');
        Cursor += 4;
        if (Localˉcount > 4096) Reject('A function local count is invalid.');
        const Locals = [];
        for (let Local = 0; Local < Localˉcount; Local += 1) {
            const Shape = Readˉshape(Bytes, Cursor);
            Locals.push(Shape);
            Cursor = Shape.end;
        }
        if (Cursor + 12 > Section.payload + Section.length) {
            Reject('A function directory entry is truncated.');
        }
        Result.push({
            name: Name.value,
            parameters: Parameters,
            result: Return,
            locals: Locals,
            codeOffset: Bytes.readUInt32LE(Cursor),
            codeLength: Bytes.readUInt32LE(Cursor + 4),
        });
        Cursor += 12;
    }
    if (Cursor !== Section.payload + Section.length) {
        Reject('The function directory length differs.');
    }
    return Result;
}

function Parseˉtypes(Bytes, Section) {
    const Count = Readˉu32(Bytes, Section.payload, 'type count');
    if (Count > 65_536) Reject('The type count is invalid.');
    const Result = [];
    let Cursor = Section.payload + 4;
    for (let Index = 0; Index < Count; Index += 1) {
        if (Cursor >= Section.payload + Section.length) {
            Reject('The type directory is truncated.');
        }
        const Kind = Bytes[Cursor++];
        const Name = Readˉstring(Bytes, Cursor);
        Cursor = Name.end;
        const Entry = { kind: Kind, name: Name.value, cases: [] };
        if (Kind === 1) {
            const Fields = Readˉu32(Bytes, Cursor, 'record field count');
            Cursor += 4;
            for (let Field = 0; Field < Fields; Field += 1) {
                const Fieldˉname = Readˉstring(Bytes, Cursor);
                Cursor = Readˉshape(Bytes, Fieldˉname.end).end;
            }
        } else if (Kind === 3) {
            const Cases = Readˉu32(Bytes, Cursor, 'variant case count');
            Cursor += 4;
            for (let Case = 0; Case < Cases; Case += 1) {
                const Caseˉname = Readˉstring(Bytes, Cursor);
                Cursor = Caseˉname.end;
                const Encoding = Bytes[Cursor++];
                const Fields = Encoding === 0 ? 0 :
                    Encoding === 1 ? 1 : Readˉu32(Bytes, Cursor, 'variant field count');
                if (Encoding > 2) Reject('A variant field encoding is invalid.');
                if (Encoding === 2) Cursor += 4;
                const Fieldˉshapes = [];
                for (let Field = 0; Field < Fields; Field += 1) {
                    const Fieldˉname = Readˉstring(Bytes, Cursor);
                    const Shape = Readˉshape(Bytes, Fieldˉname.end);
                    Fieldˉshapes.push(Shape);
                    Cursor = Shape.end;
                }
                Entry.cases.push({ name: Caseˉname.value, fields: Fieldˉshapes });
            }
        } else if (Kind === 5) {
            Cursor = Readˉshape(Bytes, Cursor).end;
        } else if (Kind === 7) {
            Cursor += 1;
            const Members = Readˉu32(Bytes, Cursor, 'enum member count');
            Cursor += 4;
            for (let Member = 0; Member < Members; Member += 1) {
                const Memberˉname = Readˉstring(Bytes, Cursor);
                Cursor = Memberˉname.end + 1;
            }
        } else {
            Reject(`The type kind ${Kind} is unsupported by this bounded reader.`);
        }
        Result.push(Entry);
    }
    if (Cursor !== Section.payload + Section.length) {
        Reject('The type directory length differs.');
    }
    return Result;
}

function Readˉshape(Bytes, Offset, Depth = 0) {
    if (Offset >= Bytes.length || Depth > 64) Reject('A WVB shape is truncated or too deep.');
    const Kind = Bytes[Offset];
    const Nominal = [7, 8, 11, 22, 23, 24, 26, 27, 28, 29, 30, 35]
        .includes(Kind);
    if (Nominal) {
        if (Offset + 5 > Bytes.length) Reject('A nominal WVB shape is truncated.');
        return { kind: Kind, start: Offset, typeIndex: Bytes.readUInt32LE(Offset + 1), end: Offset + 5 };
    }
    if (Kind === 37) {
        const Inner = Readˉshape(Bytes, Offset + 1, Depth + 1);
        if (Inner.kind === 37) Reject('A payload shape cannot contain another borrow.');
        return { kind: Kind, start: Offset, inner: Inner, typeIndex: null, end: Inner.end };
    }
    if (Kind === 12 || Kind === 13) {
        const Inner = Readˉshape(Bytes, Offset + 1, Depth + 1);
        if (Inner.end + 4 > Bytes.length) Reject('A collection shape is truncated.');
        return { kind: Kind, start: Offset, inner: Inner, typeIndex: null, end: Inner.end + 4 };
    }
    if (Kind > 36) Reject(`The WVB shape kind ${Kind} is unknown.`);
    return { kind: Kind, start: Offset, typeIndex: null, end: Offset + 1 };
}

function Readˉstring(Bytes, Offset) {
    const Length = Readˉu32(Bytes, Offset, 'string length');
    const Start = Offset + 4;
    const End = Start + Length;
    if (End > Bytes.length) Reject('A WVB string is truncated.');
    return { value: Bytes.subarray(Start, End).toString('utf8'), end: End };
}

function Readˉu32(Bytes, Offset, Label) {
    if (Offset + 4 > Bytes.length) Reject(`The ${Label} is truncated.`);
    return Bytes.readUInt32LE(Offset);
}

function Instructionˉwidth(Bytes, Cursor) {
    const Opcode = Bytes[Cursor];
    if (Opcode === 0 || Opcode > 225) return 0;
    if (Opcode === 192) return Bytes[Cursor + 2] === 0 ? 5 : 3;
    if (Opcode === 193) return Bytes[Cursor + 1] === 0 ? 6 : 2;
    if (Opcode === 194) {
        if (Bytes[Cursor + 2] !== 0) return 3;
        return Bytes[Cursor + 1] === 19 ? 11 : 7;
    }
    if (Opcode === 214 || Opcode === 215 || Opcode === 218) return 9;
    if (Opcode === 216 || Opcode === 217 || Opcode === 220 ||
        Opcode === 222 || Opcode === 223 || Opcode === 224 || Opcode === 225) {
        return 13;
    }
    if (Opcode === 219) return 6;
    if (Opcode === 221) return 9;
    if (Opcode === 1 || (Opcode >= 3 && Opcode <= 7) ||
        Opcode === 9 || Opcode === 10 || Opcode === 48 || Opcode === 49 ||
        Opcode === 64 || Opcode === 65 || Opcode === 104 || Opcode === 105 ||
        Opcode === 197 || Opcode === 199 || Opcode === 200 ||
        (Opcode >= 203 && Opcode <= 205) || Opcode === 210) {
        return 5;
    }
    if (Opcode === 2 || Opcode === 8) return 2;
    if (Opcode === 106 || Opcode === 128 || Opcode === 129 ||
        (Opcode >= 151 && Opcode <= 154) || Opcode === 196 ||
        Opcode === 201 || (Opcode >= 206 && Opcode <= 208)) {
        return 9;
    }
    if (Opcode === 202) return 15;
    if (Opcode === 209) return 13;
    return 1;
}

function Isˉoption(Type) {
    return Type?.kind === 3 && Type.cases.length === 2 &&
        Type.cases[0].name === 'Present' && Type.cases[0].fields.length === 1 &&
        Type.cases[1].name === 'Absent' && Type.cases[1].fields.length === 0;
}

function Isˉresult(Type) {
    return Type?.kind === 3 && Type.cases.length === 2 &&
        Type.cases[0].name === 'Valid' && Type.cases[0].fields.length === 1 &&
        Type.cases[1].name === 'Failure' && Type.cases[1].fields.length === 1;
}

function Shapeˉequal(Left, Right) {
    return Left.kind === Right.kind && Left.typeIndex === Right.typeIndex &&
        ((Left.inner === undefined && Right.inner === undefined) ||
            (Left.inner !== undefined && Right.inner !== undefined &&
                Shapeˉequal(Left.inner, Right.inner)));
}

function Candidateˉmutation(Source, Mutate) {
    const Result = Buffer.from(Source);
    Mutate(Result);
    return Result;
}

function Removeˉshapeˉwrapper(Source, Offset) {
    const Section = Parseˉsections(Source)[4];
    if (Source[Offset] !== 37 || Offset < Section.payload || Offset >= Section.payload + Section.length) {
        Reject('The shape-removal probe must target a borrowed function shape.');
    }
    const Result = Buffer.concat([Source.subarray(0, Offset), Source.subarray(Offset + 1)]);
    Result.writeUInt32LE(Section.length - 1, Section.payload - 4);
    return Result;
}

function Requireˉrejected(Bytes, Label) {
    try {
        Inspectˉcandidate(Bytes);
    } catch {
        return;
    }
    Reject(`The malformed ${Label} candidate was accepted.`);
}

function Requireˉordinaryˉfile(Candidate, Label) {
    const Resolved = path.resolve(Candidate);
    const Information = lstatSync(Resolved);
    if (!Information.isFile() || Information.isSymbolicLink() ||
        Information.size < 1 || Information.size > MAXIMUM_WVB_BYTES ||
        !Sameˉpath(realpathSync(Resolved), Resolved)) {
        Reject(`The ${Label} is not a bounded ordinary file: ${Resolved}.`);
    }
    return Resolved;
}

function Sameˉpath(Left, Right) {
    return process.platform === 'win32'
        ? Left.toLowerCase() === Right.toLowerCase()
        : Left === Right;
}

function Reject(Message) {
    throw new Error(Message);
}
