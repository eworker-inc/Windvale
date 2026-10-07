import assert from 'node:assert/strict';
import { Bindˉpublicationˉentries } from './Native-Compiler-Publication-Bindings.mjs';

const NAMES = ['Main', 'Publicationˉopen', 'Publicationˉbegin', 'Publicationˉnext',
    'Publicationˉrelease', 'Publicationˉstepˉrelease'];
const Word = Value => { const Result = Buffer.alloc(4); Result.writeUInt32LE(Value); return Result; };
const Name = Value => { const Bytes = Buffer.from(Value); return Buffer.concat([Word(Bytes.length), Bytes]); };
const Shape = (Tag, Identity) => Identity === undefined ? Buffer.from([Tag]) : Buffer.concat([Buffer.from([Tag]), Word(Identity)]);
const Field = (Value, Type) => Buffer.concat([Name(Value), Type]);
const Record = (Value, Fields) => Buffer.concat([Buffer.from([1]), Name(Value), Word(Fields.length), ...Fields]);
const Enum = (Value, Items) => Buffer.concat([Buffer.from([2]), Name(Value), Word(Items.length),
    ...Items.flatMap(([Value, Number]) => [Name(Value), Word(Number)])]);

// These synthetic records exercise the private binder, not bytecode or object
// admission. Executed consumers still pass both native admission boundaries.
function Fixture(Order = NAMES) {
    const Types = [
        Record('Plan', [Field('Abi', Shape(5)), Field('Functionˉcount', Shape(5)),
            Field('Machineˉcodeˉbytes', Shape(5)), Field('Helperˉbytes', Shape(5)),
            ...Array.from({ length: 28 }, (_, Index) => Field('Cell' + Index, Shape(5))),
            Field('Borrowˉdirectory', Shape(7, 8))]),
        Record('Regions', [Field('Valid', Shape(2)), Field('Objectˉbytes', Shape(5)),
            ...Array.from({ length: 7 }, (_, Index) => Field('Region' + Index, Shape(6)))]),
        Record('Session', [Field('Status', Shape(8, 5)), Field('Input', Shape(6)),
            Field('Plan', Shape(7, 0)), Field('Regions', Shape(7, 1))]),
        Record('Cursor', [Field('Valid', Shape(2)), Field('Kind', Shape(8, 6)),
            Field('Nextˉfunction', Shape(5)), Field('Position', Shape(5))]),
        Record('Step', [Field('Status', Shape(8, 7)), Field('Kind', Shape(8, 6)),
            Field('Position', Shape(5)), Field('Value', Shape(6)), Field('Nextˉvalid', Shape(2)),
            Field('Nextˉkind', Shape(8, 6)), Field('Nextˉfunction', Shape(5)), Field('Nextˉposition', Shape(5))]),
        Enum('Status', [['Valid', 0], ['Invalidˉwvb', 1], ['Unsupportedˉprofile', 2],
            ['Unsupportedˉmodule', 3], ['Unsupportedˉfunction', 4], ['Unsupportedˉcode', 5], ['Outputˉlimit', 6]]),
        Enum('Kind', [['Prefix', 0], ['Code', 1], ['Padding', 2], ['Readˉonlyˉheader', 3],
            ['Readˉonlyˉdata', 4], ['Symbols', 5], ['Relocations', 6], ['Complete', 7]]),
        Enum('Stepˉstatus', [['Valid', 0], ['Complete', 1], ['Invalidˉplan', 2],
            ['Invalidˉcursor', 3], ['Loweringˉfailure', 4]]),
        Record('Borrowˉdirectory', Array.from({ length: 5 }, (_, Index) => Field('Borrow' + Index, Shape(5)))),
    ];
    const Borrow = Identity => Buffer.concat([Buffer.from([37]), Shape(7, Identity)]);
    const Signatures = new Map([
        ['Main', { Parameters: [Buffer.from([37, 6]), Shape(25)], Return: Shape(6) }],
        ['Publicationˉopen', { Parameters: [Buffer.from([37, 6]), Buffer.from([41, 25]), Shape(5)], Return: Shape(7, 2) }],
        ['Publicationˉbegin', { Parameters: [Borrow(2)], Return: Shape(7, 3) }],
        ['Publicationˉnext', { Parameters: [Borrow(2), Shape(7, 3)], Return: Shape(7, 4) }],
        ['Publicationˉrelease', { Parameters: [Shape(7, 2)], Return: Shape(1) }],
        ['Publicationˉstepˉrelease', { Parameters: [Shape(7, 4)], Return: Shape(1) }],
    ]);
    const Functions = Buffer.concat([Word(Order.length), ...Order.map((Value, Index) => {
        const Signature = Signatures.get(Value);
        return Buffer.concat([Name(Value), Word(Signature.Parameters.length), ...Signature.Parameters,
            Signature.Return, Word(0), Word(Index), Word(1), Word(1)]);
    })]);
    const Wvbˉparts = [Buffer.alloc(0), Buffer.alloc(0), Word(0),
        Functions, Buffer.alloc(Order.length), Buffer.alloc(0), Buffer.concat([Word(Types.length), ...Types])];
    const Header = Buffer.alloc(12); Header.write('WVB1'); Header.writeUInt16LE(1, 4);
    Header.writeUInt16LE(45, 6); Header.writeUInt32LE(7, 8);
    const Bytecode = Buffer.concat([Header, ...Wvbˉparts.flatMap((Part, Index) => [Word(Index + 1), Word(Part.length), Part])]);
    const Symbols = Order.map((Value, Index) => ({ Binding: Value === 'Main' ? 2 : 1, Kind: 1, Section: 0,
        Offset: Value === 'Main' ? 0 : 64 + Index * 8, Bytes: Value === 'Main' ? 49 : 8,
        Name: Value === 'Main' ? Value : '$function_' + String(Index).padStart(4, '0') }));
    Symbols.push({ Binding: 2, Kind: 2, Section: 1, Offset: 0, Bytes: 16, Name: 'Data' });
    Symbols.sort((Left, Right) => Left.Binding - Right.Binding ||
        (Left.Name < Right.Name ? -1 : Left.Name > Right.Name ? 1 : 0));
    const Symbolˉparts = Symbols.map(Item => Buffer.concat([Buffer.from([Item.Binding, Item.Kind, 0, 0]),
        Word(Item.Section), Word(Item.Offset), Word(Item.Bytes), Name(Item.Name)]));
    const Code = Buffer.alloc(128), Data = Buffer.alloc(16);
    const Object = Buffer.concat([Buffer.from('WVO1'), Word(1), Word(1), Word(2), Word(Symbols.length), Word(0),
        Word(1), Word(16), Word(Code.length), Word(Code.length), Name('.text'), Code,
        Word(2), Word(16), Word(Data.length), Word(Data.length), Name('.rodata'), Data, ...Symbolˉparts]);
    const Configuration = Buffer.alloc(64); Configuration.write('WVSC');
    Configuration.writeUInt32LE(1, 4); Configuration.writeUInt32LE(64, 8);
    Configuration.writeUInt32LE(144, 12); Configuration.writeUInt32LE(49, 20);
    Configuration.writeUInt32LE(32, 40); Configuration.writeUInt32LE(17, 44);
    return { Bytecode, Object, Configuration, Symbols };
}

export function Checkˉpublicationˉbindingˉcases() {
    let Cases = 0;
    const Read = Item => Bindˉpublicationˉentries(Item.Bytecode, [Item.Object], Item.Configuration);
    const Valid = Fixture(), Bound = Read(Valid);
    assert.equal(Bound.Layouts.Session.Cells, 51);
    assert.equal(Bound.Layouts.Session.Fields.get('Plan').Offset, 64);
    assert.equal(Bound.Layouts.Session.Fields.get('Plan.Borrowˉdirectory').Offset, 592);
    assert.equal(Bound.Layouts.Session.Fields.get('Regions.Objectˉbytes').Offset, 688);
    assert.equal(Bound.Layouts.Step.Fields.get('Value').Offset, 48);
    assert.equal(Bound.Entries['Publicationˉnext'].Offset, 88); Cases++;
    const Packet = Buffer.from(Bound.Assembly.match(/\nbytes ([0-9 ]+)\n/u)[1].split(' ').map(Number));
    assert.equal(Packet.length, 64); assert.equal(Packet.toString('ascii', 0, 4), 'WVPC');
    assert.equal(Packet.readUInt32LE(16), Bound.Entries['Publicationˉopen'].Offset);
    assert.equal(Packet.readUInt32LE(20), Bound.Entries['Publicationˉbegin'].Offset);
    assert.equal(Packet.readUInt32LE(60), Bound.Layouts.Session.Fields.get('Regions.Objectˉbytes').Offset); Cases++;
    const Reordered = Fixture(['Publicationˉrelease', 'Main', 'Publicationˉnext',
        'Publicationˉstepˉrelease', 'Publicationˉopen', 'Publicationˉbegin']);
    const Newˉbound = Read(Reordered);
    assert.equal(Newˉbound.Entries['Publicationˉnext'].Index, 2);
    assert.equal(Newˉbound.Entries['Publicationˉnext'].Offset, 80); Cases++;
    const Segmented = Bindˉpublicationˉentries(Valid.Bytecode,
        Array.from({ length: Math.ceil(Valid.Object.length / 7) }, (_, Index) => Valid.Object.subarray(Index * 7, Index * 7 + 7)),
        Valid.Configuration);
    assert.deepEqual(Segmented.Entries, Bound.Entries); Cases++;
    function Reject(Label, Mutate) {
        const Value = Fixture(); Mutate(Value);
        assert.throws(() => Read(Value), undefined, Label); Cases++;
    }
    Reject('WVB version', Value => Value.Bytecode.writeUInt16LE(44, 6));
    Reject('WVB oversized section', Value => Value.Bytecode.writeUInt32LE(0xffff_ffff, 16));
    Reject('WVB truncation', Value => { Value.Bytecode = Value.Bytecode.subarray(0, Value.Bytecode.length - 1); });
    Reject('missing entry', Value => { const At = Value.Bytecode.indexOf(Buffer.from('Publicationˉopen')); Value.Bytecode[At] = 88; });
    Reject('open borrowed budget', Value => {
        const At = Value.Bytecode.indexOf(Buffer.from('Publicationˉopen')) + Buffer.byteLength('Publicationˉopen') + 6;
        assert.equal(Value.Bytecode[At], 41); Value.Bytecode[At + 1] = 6;
    });
    Reject('owned release identity', Value => {
        const At = Value.Bytecode.indexOf(Buffer.from('Publicationˉrelease')) + Buffer.byteLength('Publicationˉrelease') + 4;
        assert.equal(Value.Bytecode[At], 7); Value.Bytecode.writeUInt32LE(3, At + 1);
    });
    Reject('recursive record', Value => {
        const First = Value.Bytecode.indexOf(Buffer.from('Plan'));
        const At = Value.Bytecode.indexOf(Buffer.from('Plan'), First + 4);
        const Shapeˉoffset = At + 4;
        assert.equal(Value.Bytecode[Shapeˉoffset], 7);
        Value.Bytecode.writeUInt32LE(2, Shapeˉoffset + 1);
    });
    Reject('nested native backing width', Value => {
        const At = Value.Bytecode.lastIndexOf(Buffer.from('Borrowˉdirectory')) + Buffer.byteLength('Borrowˉdirectory');
        assert.equal(Value.Bytecode.readUInt32LE(At), 5); Value.Bytecode.writeUInt32LE(4, At);
    });
    Reject('session machine extent shape', Value => {
        const At = Value.Bytecode.indexOf(Buffer.from('Machineˉcodeˉbytes')) + Buffer.byteLength('Machineˉcodeˉbytes');
        assert.equal(Value.Bytecode[At], 5); Value.Bytecode[At] = 2;
    });
    Reject('status code mapping', Value => {
        const At = Value.Bytecode.indexOf(Buffer.from('Unsupportedˉcode')) + Buffer.byteLength('Unsupportedˉcode');
        assert.equal(Value.Bytecode.readUInt32LE(At), 5); Value.Bytecode.writeUInt32LE(7, At);
    });
    Reject('object architecture', Value => Value.Object.writeUInt32LE(2, 8));
    Reject('object section flags', Value => Value.Object[25] = 1);
    Reject('object truncation', Value => { Value.Object = Value.Object.subarray(0, Value.Object.length - 1); });
    Reject('object trailing bytes', Value => { Value.Object = Buffer.concat([Value.Object, Buffer.from([0])]); });
    Reject('private symbol binding', Value => {
        const At = Value.Object.indexOf(Buffer.from('$function_0001')); Value.Object[At - 20] = 2;
    });
    Reject('private symbol range', Value => {
        const At = Value.Object.indexOf(Buffer.from('$function_0001')); Value.Object.writeUInt32LE(128, At - 12);
    });
    Reject('aliased publication entries', Value => {
        const First = Value.Object.indexOf(Buffer.from('$function_0001'));
        const Second = Value.Object.indexOf(Buffer.from('$function_0002'));
        Value.Object.writeUInt32LE(Value.Object.readUInt32LE(First - 12), Second - 12);
    });
    Reject('non-ASCII symbol', Value => {
        const At = Value.Object.indexOf(Buffer.from('$function_0001')); Value.Object[At] = 0xa4;
    });
    Reject('missing private symbol', Value => {
        const At = Value.Object.indexOf(Buffer.from('$function_0001')); Value.Object[At + 13] = 57;
    });
    Reject('configuration Main offset', Value => Value.Configuration.writeUInt32LE(1, 16));
    Reject('configuration module size', Value => Value.Configuration.writeUInt32LE(143, 12));
    Reject('configuration reserved field', Value => Value.Configuration.writeUInt32LE(1, 60));
    assert.throws(() => Bindˉpublicationˉentries(Valid.Bytecode, [Buffer.alloc(4_194_305)], Valid.Configuration)); Cases++;
    assert.throws(() => Bindˉpublicationˉentries(Valid.Bytecode, Array.from({ length: 519 }, () => Buffer.alloc(1)), Valid.Configuration)); Cases++;
    assert.throws(() => Bindˉpublicationˉentries(Valid.Bytecode, [Valid.Object], Valid.Configuration.subarray(0, 63))); Cases++;
    return { Cases };
}
