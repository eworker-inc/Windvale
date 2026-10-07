const MAXIMUM_WVB_BYTES = 4_194_304;
const MAXIMUM_OBJECT_BYTES = 67_108_864;
const MAXIMUM_CHUNKS = 518;
const MAXIMUM_CHUNK_BYTES = 4_194_304;
const MAXIMUM_FUNCTIONS = 2048;
const MAXIMUM_TYPES = 256;
const MAXIMUM_RECORD_CELLS = 64;
const ENTRY_NAMES = ['Publicationˉopen', 'Publicationˉbegin', 'Publicationˉnext',
    'Publicationˉrelease', 'Publicationˉstepˉrelease'];
const NOMINAL_SHAPES = new Set([7, 8, 11, 22, 23, 24, 26, 27, 28, 29, 30, 35]);
const UTF8 = new TextDecoder('utf-8', { fatal: true });

function Require(Condition, Message) {
    if (!Condition) throw new Error('Native publication binding: ' + Message);
}

class Byteˉreader {
    constructor(Input, Start = 0, End = Input.length) {
        this.Input = Input;
        this.Position = Start;
        this.End = End;
    }
    Range(Length) {
        Require(Number.isSafeInteger(Length) && Length >= 0 &&
            Length <= this.End - this.Position, 'truncated record');
    }
    Byte() { this.Range(1); return this.Input[this.Position++]; }
    Word() {
        this.Range(4);
        const Value = this.Input.readUInt32LE(this.Position);
        this.Position += 4;
        return Value;
    }
    Skip(Length) { this.Range(Length); this.Position += Length; }
    Name() {
        const Length = this.Word();
        Require(Length > 0 && Length <= 255, 'name length');
        this.Range(Length);
        const Value = UTF8.decode(this.Input.subarray(this.Position, this.Position + Length));
        this.Position += Length;
        return Value;
    }
    Shape(Depth = 0) {
        Require(Depth <= 1, 'nested borrow wrapper');
        const Tag = this.Byte();
        if (Tag === 37 || Tag === 41) return { Tag, Inner: this.Shape(Depth + 1) };
        if (NOMINAL_SHAPES.has(Tag)) return { Tag, Identity: this.Word() };
        if (Tag === 40) {
            Require(this.Byte() === 4, 'builder element shape');
            return { Tag };
        }
        Require((Tag >= 1 && Tag <= 10) || [20, 25, 36, 39].includes(Tag), 'unsupported shape');
        return { Tag };
    }
    Complete() { Require(this.Position === this.End, 'trailing record bytes'); }
}

function Readˉsections(Input) {
    Require(Buffer.isBuffer(Input) && Input.length >= 12 && Input.length <= MAXIMUM_WVB_BYTES &&
        Input.toString('ascii', 0, 4) === 'WVB1' && Input.readUInt16LE(4) === 1 &&
        Input.readUInt16LE(6) === 45 && Input.readUInt32LE(8) === 7, 'WVB 1.45 envelope');
    const Reader = new Byteˉreader(Input, 12), Sections = [];
    for (let Kind = 1; Kind <= 7; Kind++) {
        Require(Reader.Word() === Kind, 'section order or reserved flags');
        const Length = Reader.Word(), Start = Reader.Position;
        Reader.Skip(Length);
        Sections[Kind] = { Start, End: Reader.Position };
    }
    Reader.Complete();
    return Sections;
}

function Readˉtypes(Input, Section) {
    const Reader = new Byteˉreader(Input, Section.Start, Section.End);
    const Count = Reader.Word();
    Require(Count > 0 && Count <= MAXIMUM_TYPES, 'type count');
    const Types = [];
    function Fields(Count) {
        Require(Count > 0 && Count <= 64, 'field count');
        const Result = [];
        for (let Index = 0; Index < Count; Index++) Result.push({ Name: Reader.Name(), Shape: Reader.Shape() });
        Require(new Set(Result.map(Field => Field.Name)).size === Count, 'duplicate field');
        return Result;
    }
    for (let Index = 0; Index < Count; Index++) {
        const Kind = Reader.Byte();
        if (Kind === 8) {
            Reader.Byte(); Reader.Shape();
            const Parameters = Reader.Word();
            Require(Parameters <= 64, 'callable parameters');
            for (let Parameter = 0; Parameter < Parameters; Parameter++) Reader.Shape();
            Types.push({ Kind, Fields: [] });
            continue;
        }
        Require([1, 2, 3, 5, 7].includes(Kind), 'type kind=' + Kind + ' index=' + Index + ' offset=' + (Reader.Position - 1));
        const Name = Reader.Name();
        if (Kind === 5) {
            Reader.Byte(); Types.push({ Kind, Name, Fields: [] }); continue;
        }
        if (Kind === 7) Require(Reader.Byte() === 6, 'text enum underlying shape');
        const Items = Reader.Word();
        Require(Items > 0 && Items <= (Kind === 1 ? 64 : 256), 'type item count');
        if (Kind === 1) {
            Types.push({ Kind, Name, Fields: Fields(Items) }); continue;
        }
        const Values = new Map();
        for (let Item = 0; Item < Items; Item++) {
            const Itemˉname = Reader.Name();
            if (Kind === 2 || Kind === 7) {
                Require(!Values.has(Itemˉname), 'duplicate enum member');
                Values.set(Itemˉname, Kind === 2 ? Reader.Word() : Reader.Byte());
            }
            else {
                const Payload = Reader.Byte();
                Require(Payload <= 2, 'variant payload marker');
                if (Payload === 1) Fields(1);
                if (Payload === 2) Fields(Reader.Word());
            }
        }
        Types.push({ Kind, Name, Fields: [], Values });
    }
    Reader.Complete();
    return Types;
}

function Readˉfunctions(Input, Section, Code) {
    const Reader = new Byteˉreader(Input, Section.Start, Section.End);
    const Count = Reader.Word();
    Require(Count > 0 && Count <= MAXIMUM_FUNCTIONS, 'function count');
    const Entries = new Map(), Names = new Set();
    let Codeˉposition = 0;
    for (let Index = 0; Index < Count; Index++) {
        const Name = Reader.Name(), Parameters = Reader.Word();
        Require(!Names.has(Name) && Parameters <= 64, 'duplicate function or parameter count');
        Names.add(Name);
        const Shapes = [];
        for (let Parameter = 0; Parameter < Parameters; Parameter++) Shapes.push(Reader.Shape());
        const Return = Reader.Shape(), Locals = Reader.Word();
        Require(Locals <= 65_536, 'local count');
        for (let Local = 0; Local < Locals; Local++) Reader.Shape();
        const Offset = Reader.Word(), Length = Reader.Word(), Stack = Reader.Word();
        Require(Offset === Codeˉposition && Length > 0 && Length <= Code.End - Code.Start - Offset &&
            Stack <= 65_536, 'function code geometry');
        Codeˉposition += Length;
        if (ENTRY_NAMES.includes(Name) || Name === 'Main') Entries.set(Name, { Index, Shapes, Return });
    }
    Reader.Complete();
    Require(Codeˉposition === Code.End - Code.Start &&
        ENTRY_NAMES.every(Name => Entries.has(Name)) && Entries.has('Main'), 'missing publication entry');
    return { Entries, Count };
}

function Recordˉlayout(Types, Shape) {
    const Fields = new Map();
    let Visits = 0, Cells = 0;
    function Walk(Current, Path, Depth, Active) {
        Require(++Visits <= 1024 && Depth <= 16, 'record traversal bound');
        const Cell = Cells;
        if (Current.Tag === 7) {
            const Type = Types[Current.Identity];
            Require(Type?.Kind === 1 && !Active.has(Current.Identity), 'invalid or recursive publication record');
            const Next = new Set(Active); Next.add(Current.Identity);
            // Native records reserve all direct cells first, including zero
            // cells for nested fields, then append each nested backing.
            Cells += Type.Fields.length;
            Require(Cells <= MAXIMUM_RECORD_CELLS, 'record width');
            for (const [Index, Field] of Type.Fields.entries()) {
                const Fieldˉpath = Path ? Path + '.' + Field.Name : Field.Name;
                if (Field.Shape.Tag === 7) Walk(Field.Shape, Fieldˉpath, Depth + 1, Next);
                else {
                    Require(++Visits <= 1024 && [1, 2, 3, 4, 5, 6, 8, 10].includes(Field.Shape.Tag),
                        'owner or unsupported publication field');
                    if (Field.Shape.Tag === 8) Require([2, 7].includes(Types[Field.Shape.Identity]?.Kind), 'enum identity');
                    Fields.set(Fieldˉpath, { Offset: (Cell + Index) * 16, Cells: 1, Tag: Field.Shape.Tag });
                }
            }
        }
        else Require(false, 'publication backing must be a record');
        if (Path) Fields.set(Path, { Offset: Cell * 16, Cells: Cells - Cell, Tag: Current.Tag });
    }
    Walk(Shape, '', 0, new Set());
    return { Cells, Fields };
}

function Checkˉsignatures(Types, Entries) {
    const Open = Entries.get(ENTRY_NAMES[0]), Begin = Entries.get(ENTRY_NAMES[1]),
        Next = Entries.get(ENTRY_NAMES[2]), Release = Entries.get(ENTRY_NAMES[3]),
        Stepˉrelease = Entries.get(ENTRY_NAMES[4]);
    const Record = Shape => Shape?.Tag === 7 && Types[Shape.Identity]?.Kind === 1;
    const Borrow = (Shape, Identity) => Shape?.Tag === 37 && Shape.Inner?.Tag === 7 && Shape.Inner.Identity === Identity;
    Require(Open.Shapes.length === 3 && Open.Shapes[0].Tag === 37 && Open.Shapes[0].Inner?.Tag === 6 &&
        Open.Shapes[1].Tag === 41 && Open.Shapes[1].Inner?.Tag === 25 && Open.Shapes[2].Tag === 5 &&
        Record(Open.Return), 'open signature');
    const Session = Open.Return.Identity;
    Require(Begin.Shapes.length === 1 && Borrow(Begin.Shapes[0], Session) && Record(Begin.Return), 'begin signature');
    const Cursor = Begin.Return.Identity;
    Require(Next.Shapes.length === 2 && Borrow(Next.Shapes[0], Session) && Next.Shapes[1].Tag === 7 &&
        Next.Shapes[1].Identity === Cursor && Record(Next.Return), 'next signature');
    const Step = Next.Return.Identity;
    Require(Release.Shapes.length === 1 && Release.Shapes[0].Tag === 7 && Release.Shapes[0].Identity === Session &&
        Release.Return.Tag === 1 && Stepˉrelease.Shapes.length === 1 && Stepˉrelease.Shapes[0].Tag === 7 &&
        Stepˉrelease.Shapes[0].Identity === Step && Stepˉrelease.Return.Tag === 1 &&
        new Set([Session, Cursor, Step]).size === 3, 'release signature');
    const Layouts = { Session: Recordˉlayout(Types, Open.Return), Cursor: Recordˉlayout(Types, Begin.Return),
        Step: Recordˉlayout(Types, Next.Return) };
    for (const [Name, Cells] of [['Session', 51], ['Cursor', 4], ['Step', 8]])
        Require(Layouts[Name].Cells === Cells, Name + ' width');
    for (const [Name, Field, Tag] of [['Session', 'Status', 8], ['Session', 'Input', 6],
        ['Session', 'Plan.Abi', 5], ['Session', 'Plan.Functionˉcount', 5],
        ['Session', 'Regions.Valid', 2], ['Session', 'Regions.Objectˉbytes', 5],
        ['Cursor', 'Valid', 2], ['Cursor', 'Kind', 8], ['Cursor', 'Nextˉfunction', 5], ['Cursor', 'Position', 5],
        ['Step', 'Status', 8], ['Step', 'Kind', 8], ['Step', 'Position', 5], ['Step', 'Value', 6],
        ['Step', 'Nextˉvalid', 2], ['Step', 'Nextˉkind', 8], ['Step', 'Nextˉfunction', 5], ['Step', 'Nextˉposition', 5]])
        Require(Layouts[Name].Fields.get(Field)?.Tag === Tag && Layouts[Name].Fields.get(Field).Cells === 1,
            Name + '.' + Field + ' shape');
    for (const [Name, Names] of [['Cursor', ['Valid', 'Kind', 'Nextˉfunction', 'Position']],
        ['Step', ['Status', 'Kind', 'Position', 'Value', 'Nextˉvalid', 'Nextˉkind', 'Nextˉfunction', 'Nextˉposition']]])
        Require(Names.every((Field, Index) => Layouts[Name].Fields.get(Field).Offset === Index * 16),
            Name + ' native field order');
    function Enum(Identity, Expected) {
        const Values = Types[Identity]?.Values;
        Require(Values instanceof Map && Object.entries(Expected).every(([Name, Value]) => Values.get(Name) === Value),
            'publication enum values');
    }
    const Sessionˉfields = Types[Session].Fields, Cursorˉfields = Types[Cursor].Fields, Stepˉfields = Types[Step].Fields;
    Enum(Sessionˉfields.find(Field => Field.Name === 'Status').Shape.Identity,
        { Valid: 0, Invalidˉwvb: 1, Unsupportedˉprofile: 2, Unsupportedˉmodule: 3,
            Unsupportedˉfunction: 4, Unsupportedˉcode: 5, Outputˉlimit: 6 });
    Enum(Cursorˉfields.find(Field => Field.Name === 'Kind').Shape.Identity,
        { Prefix: 0, Code: 1, Padding: 2, Readˉonlyˉheader: 3, Readˉonlyˉdata: 4, Symbols: 5, Relocations: 6, Complete: 7 });
    Enum(Stepˉfields.find(Field => Field.Name === 'Status').Shape.Identity,
        { Valid: 0, Complete: 1, Invalidˉplan: 2, Invalidˉcursor: 3, Loweringˉfailure: 4 });
    return Layouts;
}

class Stagedˉreader {
    constructor(Chunks) {
        Require(Array.isArray(Chunks) && Chunks.length > 0 && Chunks.length <= MAXIMUM_CHUNKS, 'chunk count');
        this.Chunks = Chunks; this.Starts = []; this.Position = 0; this.End = 0;
        for (const Chunk of Chunks) {
            Require(Buffer.isBuffer(Chunk) && Chunk.length > 0 && Chunk.length <= MAXIMUM_CHUNK_BYTES, 'chunk size');
            this.Starts.push(this.End); this.End += Chunk.length;
            Require(this.End <= MAXIMUM_OBJECT_BYTES, 'object size');
        }
    }
    Range(Length) { Byteˉreader.prototype.Range.call(this, Length); }
    Skip(Length) { this.Range(Length); this.Position += Length; }
    Bytes(Length) {
        this.Range(Length); Require(Length <= 255, 'buffered record bound');
        const Result = Buffer.alloc(Length);
        let Copied = 0;
        while (Copied < Length) {
            let Low = 0, High = this.Starts.length;
            while (Low + 1 < High) { const Middle = (Low + High) >>> 1;
                if (this.Starts[Middle] <= this.Position) Low = Middle; else High = Middle; }
            const Offset = this.Position - this.Starts[Low], Take = Math.min(Length - Copied, this.Chunks[Low].length - Offset);
            Require(Take > 0, 'chunk coverage');
            this.Chunks[Low].copy(Result, Copied, Offset, Offset + Take);
            Copied += Take; this.Position += Take;
        }
        return Result;
    }
    Byte() { return this.Bytes(1)[0]; }
    Word() { return this.Bytes(4).readUInt32LE(0); }
    Name() {
        const Length = this.Word(); Require(Length > 0 && Length <= 255, 'machine name size');
        const Value = this.Bytes(Length).toString('latin1');
        Require(/^[A-Za-z_.$][A-Za-z0-9_.$]*$/u.test(Value), 'machine name grammar');
        return Value;
    }
    Complete() { Byteˉreader.prototype.Complete.call(this); }
}

// WVB and the object have already passed the current native verifier and
// independent staged linker. This binds their private call sites; it does not
// replace either admission boundary or authorize arbitrary object addresses.
export function Readˉpublicationˉsourceˉentries(Bytecode) {
    const Sections = Readˉsections(Bytecode), Types = Readˉtypes(Bytecode, Sections[7]);
    const { Entries, Count } = Readˉfunctions(Bytecode, Sections[4], Sections[5]);
    const Mainˉentry = Entries.get('Main');
    Require(Mainˉentry.Shapes.length === 2 && Mainˉentry.Shapes[0].Tag === 37 &&
        Mainˉentry.Shapes[0].Inner?.Tag === 6 && Mainˉentry.Shapes[1].Tag === 25 &&
        Mainˉentry.Return.Tag === 6, 'compiler Main signature');
    return { Entries, Count, Layouts: Checkˉsignatures(Types, Entries) };
}

export function Bindˉpublicationˉentries(Bytecode, Chunks, Configuration) {
    Require(Buffer.isBuffer(Configuration) && Configuration.length === 64 &&
        Configuration.toString('ascii', 0, 4) === 'WVSC' && Configuration.readUInt32LE(4) === 1 &&
        Configuration.readUInt32LE(8) === 64 && Configuration.readUInt32LE(12) <= MAXIMUM_OBJECT_BYTES &&
        Configuration.readUInt32LE(60) === 0, 'admitted configuration');
    const { Entries, Count, Layouts } = Readˉpublicationˉsourceˉentries(Bytecode);
    const Reader = new Stagedˉreader(Chunks);
    Require(Reader.Word() === 0x314f5657 && Reader.Word() === 1 && Reader.Word() === 1 && Reader.Word() === 2,
        'ABI25 object envelope');
    const Symbols = Reader.Word(), Relocations = Reader.Word();
    Require(Symbols > Count && Symbols <= Count + 528 && Relocations <= 65_536, 'object record counts');
    const Sectionˉsizes = [];
    for (let Index = 0; Index < 2; Index++) {
        Require(Reader.Word() === Index + 1, 'object section kind or reserved flags');
        const Alignment = Reader.Word(), Memory = Reader.Word(), Length = Reader.Word(), Name = Reader.Name();
        Require(Alignment === 16 && Memory === Length && Name === (Index === 0 ? '.text' : '.rodata'), 'object section layout');
        Reader.Skip(Length); Sectionˉsizes.push(Memory);
    }
    const Wanted = new Map(ENTRY_NAMES.map(Name => ['$function_' + String(Entries.get(Name).Index).padStart(4, '0'), Name]));
    const Bound = {}, Names = new Set();
    let Main = null;
    for (let Index = 0; Index < Symbols; Index++) {
        const Binding = Reader.Byte(), Kind = Reader.Byte();
        Require(Reader.Byte() === 0 && Reader.Byte() === 0 && [1, 2, 3].includes(Binding) && [1, 2].includes(Kind),
            'symbol flags or kind');
        const Section = Reader.Word(), Offset = Reader.Word(), Bytes = Reader.Word(), Name = Reader.Name();
        Require(!Names.has(Name), 'duplicate symbol'); Names.add(Name);
        if (Binding === 3) Require(Section === 0xffff_ffff && Offset === 0 && Bytes === 0, 'import geometry');
        else Require(Section < 2 && Offset <= Sectionˉsizes[Section] && (Kind !== 1 || Bytes > 0) &&
            Bytes <= Sectionˉsizes[Section] - Offset && (Kind !== 1 || Section === 0), 'symbol range');
        if (Name === 'Main') {
            Require(Binding === 2 && Kind === 1 && Section === 0, 'Main symbol');
            Main = { Offset, Bytes };
        }
        if (Wanted.has(Name)) {
            Require(Binding === 1 && Kind === 1 && Section === 0, 'private publication symbol');
            Bound[Wanted.get(Name)] = Object.freeze({ Offset, Bytes, Index: Entries.get(Wanted.get(Name)).Index });
        }
    }
    Reader.Skip(Relocations * 20); Reader.Complete();
    Require(Main && Main.Offset === Configuration.readUInt32LE(16) && Main.Bytes === Configuration.readUInt32LE(20) &&
        ENTRY_NAMES.every(Name => Bound[Name]) && Sectionˉsizes[0] + Sectionˉsizes[1] +
        ((16 - Sectionˉsizes[0] % 16) % 16) === Configuration.readUInt32LE(12), 'module binding geometry');
    Require(new Set(ENTRY_NAMES.map(Name => Bound[Name].Offset)).size === ENTRY_NAMES.length,
        'aliased publication entries');
    const Packet = Buffer.alloc(64); Packet.write('WVPC');
    Packet.writeUInt32LE(1, 4); Packet.writeUInt32LE(64, 8);
    Packet.writeUInt32LE(Configuration.readUInt32LE(12), 12);
    ENTRY_NAMES.forEach((Name, Index) => Packet.writeUInt32LE(Bound[Name].Offset, 16 + Index * 4));
    ['Status', 'Plan.Abi', 'Plan.Functionˉcount', 'Plan.Machineˉcodeˉbytes', 'Plan.Helperˉbytes',
        'Regions.Valid', 'Regions.Objectˉbytes'].forEach((Name, Index) =>
        Packet.writeUInt32LE(Layouts.Session.Fields.get(Name)?.Offset ?? 0, 36 + Index * 4));
    Require(['Plan.Machineˉcodeˉbytes', 'Plan.Helperˉbytes'].every(Name =>
        Layouts.Session.Fields.get(Name)?.Tag === 5 && Layouts.Session.Fields.get(Name)?.Cells === 1),
        'session machine extent fields');
    const Assembly = 'windvale-assembly 1\n\nsymbol export data Windvale_shared_compiler_publication in .rodata\n' +
        'section rodata .rodata align 16\ndefine Windvale_shared_compiler_publication\nbytes ' +
        [...Packet].join(' ') + '\nend define\nend section\n';
    return Object.freeze({ Entries: Object.freeze(Bound), Layouts, Functionˉcount: Count, Assembly });
}
