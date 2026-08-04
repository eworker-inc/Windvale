using System.Collections.Immutable;
using System.Text;

namespace Windvale.Bytecode;

public static class Moduleˉverifier
{
    private static readonly UTF8Encoding STRICT_UTF8 = new(false, true);

    public static Verifiedˉmodule Verify(Bytecodeˉmodule module)
    {
        ArgumentNullException.ThrowIfNull(module);
        Verifyˉmoduleˉmetadata(module);

        var Verifiedˉfunctions = ImmutableArray.CreateBuilder<Verifiedˉfunction>(module.Functions.Length);
        foreach (var Function in module.Functions)
        {
            var Functionˉcode = module.Code.AsSpan(Function.Codeˉoffset, Function.Codeˉlength);
            var Instructions = Instructionˉcodec.Decode(Functionˉcode, Function.Name);
            if (module.Formatˉminorˉversion == Moduleˉcodec.BASE_MINOR_VERSION &&
                Instructions.Any(Instruction =>
                    Moduleˉcodec.Isˉversionˉ1ˉ7ˉopcode(Instruction.Opcode)))
            {
                Fail(
                    "WVB2107",
                    $"Function '{Function.Name}' uses a WVB 1.7 opcode in a WVB 1.6 module.");
            }
            if (module.Formatˉminorˉversion < Moduleˉcodec.VARIANT_MINOR_VERSION &&
                Instructions.Any(Instruction =>
                    Moduleˉcodec.Isˉversionˉ1ˉ9ˉopcode(Instruction.Opcode)))
            {
                Fail(
                    "WVB2107",
                    $"Function '{Function.Name}' uses a WVB 1.9 opcode in a WVB 1.{module.Formatˉminorˉversion} module.");
            }
            if (module.Formatˉminorˉversion < Moduleˉcodec.COLLECTION_MINOR_VERSION &&
                Instructions.Any(Instruction =>
                    Moduleˉcodec.Isˉversionˉ1ˉ10ˉopcode(Instruction.Opcode)))
            {
                Fail(
                    "WVB2107",
                    $"Function '{Function.Name}' uses a WVB 1.10 opcode in a WVB 1.{module.Formatˉminorˉversion} module.");
            }
            if (module.Formatˉminorˉversion < Moduleˉcodec.OPERATOR_MINOR_VERSION &&
                Instructions.Any(Instruction =>
                    Moduleˉcodec.Isˉversionˉ1ˉ11ˉopcode(Instruction.Opcode)))
            {
                Fail(
                    "WVB2107",
                    $"Function '{Function.Name}' uses a WVB 1.11 opcode in a WVB 1.{module.Formatˉminorˉversion} module.");
            }

            Verifyˉfunction(module, Function, Instructions);
            Verifiedˉfunctions.Add(new(Function, Instructions));
        }

        return new(module, Verifiedˉfunctions.ToImmutable());
    }

    private static void Verifyˉmoduleˉmetadata(Bytecodeˉmodule module)
    {
        if (module.Formatˉminorˉversion is not (
            Moduleˉcodec.BASE_MINOR_VERSION or
            Moduleˉcodec.WIDE_MINOR_VERSION or
            Moduleˉcodec.MINOR_VERSION or
            Moduleˉcodec.VARIANT_MINOR_VERSION or
            Moduleˉcodec.COLLECTION_MINOR_VERSION or
            Moduleˉcodec.OPERATOR_MINOR_VERSION))
        {
            Fail(
                "WVB2107",
                $"Module model version 1.{module.Formatˉminorˉversion} is not supported.");
        }

        static bool Isˉwide(Valueˉshape Shape) =>
            Shape.Kind is Valueˉtype.I64 or Valueˉtype.U64;
        if (module.Formatˉminorˉversion == Moduleˉcodec.BASE_MINOR_VERSION &&
            (module.Capabilities.Any(Capability =>
                Capability.Parameterˉtypes.Any(Type => Isˉwide(Type)) ||
                Isˉwide(Capability.Returnˉtype)) ||
            module.Functions.Any(Function =>
                Function.Parameterˉtypes.Any(Isˉwide) ||
                Isˉwide(Function.Returnˉtype) ||
                Function.Localˉtypes.Any(Isˉwide)) ||
            module.Types.OfType<Recordˉtypeˉdeclaration>().Any(Record =>
                Record.Fields.Any(Field => Isˉwide(Field.Type)))))
        {
            Fail("WVB2107", "A WVB 1.6 module contains a WVB 1.7 value type.");
        }

        if (module.Formatˉminorˉversion < Moduleˉcodec.MINOR_VERSION &&
            module.Metadata is not null)
        {
            Fail("WVB2160", "Module metadata requires WVB 1.8.");
        }
        if (module.Formatˉminorˉversion == Moduleˉcodec.MINOR_VERSION &&
            module.Metadata is null)
        {
            Fail("WVB2160", "A WVB 1.8 module must contain module metadata.");
        }
        if (module.Formatˉminorˉversion < Moduleˉcodec.VARIANT_MINOR_VERSION &&
            module.Types.Any(Type => Type is Variantˉtypeˉdeclaration))
        {
            Fail("WVB2161", "Nominal variants require WVB 1.9.");
        }

        if (!Seedˉnames.Isˉidentifier(module.Name))
        {
            Fail("WVB2100", $"Module name '{module.Name}' is not a Seed identifier.");
        }

        if (!Enum.IsDefined(module.Profile))
        {
            Fail("WVB2101", $"Module profile '{module.Profile}' is invalid.");
        }

        if (module.Capabilities.Length > Bytecodeˉlimits.MAX_CAPABILITIES)
        {
            Fail("WVB2102", "The module has too many capabilities.");
        }

        if (module.Data.Length > Bytecodeˉlimits.MAX_DATA_DECLARATIONS)
        {
            Fail("WVB2103", "The module has too many data declarations.");
        }

        if (module.Functions.Length > Bytecodeˉlimits.MAX_FUNCTIONS)
        {
            Fail("WVB2104", "The module has too many functions.");
        }

        if (module.Types.Length > Bytecodeˉlimits.MAX_NOMINAL_TYPES)
        {
            Fail("WVB2106", "The module has too many nominal types.");
        }

        if (module.Code.Length > Bytecodeˉlimits.MAX_MODULE_BYTES)
        {
            Fail("WVB2105", "The code section exceeds the module-size limit.");
        }

        Verifyˉcapabilities(module);
        Verifyˉindependentˉmetadata(module);
        Verifyˉdata(module);
        Verifyˉtypes(module);
        Verifyˉfunctionˉmetadata(module);
        Verifyˉexports(module);
    }

    private static void Verifyˉindependentˉmetadata(Bytecodeˉmodule module)
    {
        if (module.Metadata is not { } Metadata)
        {
            return;
        }

        if (!Enum.IsDefined(Metadata.Authority))
        {
            Fail("WVB2161", $"Module authority '{Metadata.Authority}' is invalid.");
        }
        if (Metadata.Platformˉscopes.IsDefault ||
            Metadata.Requiredˉcapabilities.IsDefault ||
            Metadata.Optionalˉcapabilities.IsDefault)
        {
            Fail("WVB2162", "Module metadata arrays must be initialized.");
        }
        if (Metadata.Platformˉscopes.Length is 0 or > Bytecodeˉlimits.MAX_PLATFORM_SCOPES)
        {
            Fail("WVB2162", "Module metadata has an invalid platform-scope count.");
        }
        if (Metadata.Requiredˉcapabilities.Length > Bytecodeˉlimits.MAX_CAPABILITY_REQUIREMENTS ||
            Metadata.Optionalˉcapabilities.Length > Bytecodeˉlimits.MAX_CAPABILITY_REQUIREMENTS)
        {
            Fail("WVB2163", "Module metadata has too many capability requirements.");
        }

        Verifyˉstrictˉordering(Metadata.Platformˉscopes, "platform scope");
        foreach (var Scope in Metadata.Platformˉscopes)
        {
            if (!Seedˉnames.Isˉplatformˉscope(Scope))
            {
                Fail("WVB2164", $"Platform scope '{Scope}' is invalid.");
            }
        }

        Verifyˉrequirements(Metadata.Requiredˉcapabilities, "required");
        Verifyˉrequirements(Metadata.Optionalˉcapabilities, "optional");
        var Requiredˉnames = Metadata.Requiredˉcapabilities
            .Select(Requirement => Requirement.Name)
            .ToHashSet(StringComparer.Ordinal);
        foreach (var Requirement in Metadata.Optionalˉcapabilities)
        {
            if (Requiredˉnames.Contains(Requirement.Name))
            {
                Fail(
                    "WVB2165",
                    $"Capability '{Requirement.Name}' cannot be both required and optional.");
            }
        }

        var Declaredˉnames = module.Capabilities
            .Select(Capability => Capability.Name)
            .ToArray();
        var Metadataˉnames = Metadata.Requiredˉcapabilities
            .Select(Requirement => Requirement.Name)
            .ToArray();
        if (!Declaredˉnames.SequenceEqual(Metadataˉnames, StringComparer.Ordinal))
        {
            Fail(
                "WVB2166",
                "Required capability metadata must exactly match the executable capability declarations.");
        }

        if ((Metadata.Authority == Moduleˉauthority.System) !=
            (module.Profile == Moduleˉprofile.System))
        {
            Fail("WVB2167", "System authority and the retained system profile must agree.");
        }
        if (module.Profile == Moduleˉprofile.Portable &&
            Metadata.Optionalˉcapabilities.Length != 0)
        {
            Fail("WVB2168", "A portable module cannot declare optional hosted capabilities.");
        }
    }

    private static void Verifyˉrequirements(
        ImmutableArray<Capabilityˉrequirement> requirements,
        string kind)
    {
        Verifyˉstrictˉordering(
            requirements.Select(Requirement => Requirement.Name),
            $"{kind} capability");
        foreach (var Requirement in requirements)
        {
            if (!Seedˉnames.Isˉcapability(Requirement.Name))
            {
                Fail("WVB2169", $"{kind} capability name '{Requirement.Name}' is invalid.");
            }
            if (!Capabilityˉcatalog.Tryˉget(Requirement.Name, out _))
            {
                Fail("WVB2170", $"{kind} capability '{Requirement.Name}' is not defined by Windvale Seed.");
            }
            if (Requirement.Majorˉversion != 1)
            {
                Fail(
                    "WVB2171",
                    $"{kind} capability '{Requirement.Name}' requires unsupported major version {Requirement.Majorˉversion}.");
            }
        }
    }

    private static void Verifyˉcapabilities(Bytecodeˉmodule module)
    {
        if (module.Profile == Moduleˉprofile.Portable && module.Capabilities.Length != 0)
        {
            Fail("WVB2110", "A portable module cannot declare capabilities.");
        }

        Verifyˉstrictˉordering(
            module.Capabilities.Select(Capability => Capability.Name),
            "capability");

        foreach (var Capability in module.Capabilities)
        {
            if (!Seedˉnames.Isˉcapability(Capability.Name))
            {
                Fail("WVB2111", $"Capability name '{Capability.Name}' is invalid.");
            }

            if (!Capabilityˉcatalog.Tryˉget(Capability.Name, out var Canonical))
            {
                Fail("WVB2112", $"Capability '{Capability.Name}' is not defined by Windvale Seed.");
            }

            if (!Capability.Parameterˉtypes.SequenceEqual(Canonical.Parameterˉtypes) ||
                Capability.Returnˉtype != Canonical.Returnˉtype)
            {
                Fail("WVB2113", $"Capability '{Capability.Name}' has a non-canonical signature.");
            }

            foreach (var Parameterˉtype in Capability.Parameterˉtypes)
            {
                Verifyˉvalueˉtype(Parameterˉtype, allowˉvoid: false, "capability parameter");
            }

            Verifyˉvalueˉtype(Capability.Returnˉtype, allowˉvoid: true, "capability return");
        }
    }

    private static void Verifyˉdata(Bytecodeˉmodule module)
    {
        Verifyˉstrictˉordering(module.Data.Select(Data => Data.Name), "data declaration");
        foreach (var Data in module.Data)
        {
            if (!Seedˉnames.Isˉidentifier(Data.Name))
            {
                Fail("WVB2120", $"Data name '{Data.Name}' is not a Seed identifier.");
            }

            switch (Data)
            {
                case Textˉdataˉdeclaration Text when Text.Type == Dataˉtype.Text:
                    int Utf8ˉlength;
                    try
                    {
                        Utf8ˉlength = STRICT_UTF8.GetByteCount(Text.Value);
                    }
                    catch (EncoderFallbackException)
                    {
                        Fail("WVB2124", $"Text data '{Text.Name}' contains an unpaired Unicode surrogate.");
                        break;
                    }

                    if (Utf8ˉlength > Bytecodeˉlimits.MAX_UTF8_VALUE_BYTES)
                    {
                        Fail("WVB2121", $"Text data '{Text.Name}' exceeds the UTF-8 value limit.");
                    }

                    break;
                case I32ˉarrayˉdataˉdeclaration Array when Array.Type == Dataˉtype.I32ˉarray:
                    if (Array.Values.Length > Bytecodeˉlimits.MAX_I32_ARRAY_ELEMENTS)
                    {
                        Fail("WVB2122", $"Array data '{Array.Name}' exceeds the element limit.");
                    }

                    break;
                case Bytesˉdataˉdeclaration Bytes when Bytes.Type == Dataˉtype.Bytes:
                    if (Bytes.Values.Length > Bytecodeˉlimits.MAX_BYTE_DATA_BYTES)
                    {
                        Fail("WVB2125", $"Byte data '{Bytes.Name}' exceeds the byte-data limit.");
                    }

                    break;
                default:
                    Fail("WVB2123", $"Data declaration '{Data.Name}' has an inconsistent representation.");
                    break;
            }
        }
    }

    private static void Verifyˉfunctionˉmetadata(Bytecodeˉmodule module)
    {
        Verifyˉstrictˉordering(module.Functions.Select(Function => Function.Name), "function");
        var Expectedˉcodeˉoffset = 0;

        foreach (var Function in module.Functions)
        {
            if (!Seedˉnames.Isˉidentifier(Function.Name))
            {
                Fail("WVB2130", $"Function name '{Function.Name}' is not a Seed identifier.");
            }

            if (Function.Parameterˉtypes.Length > Bytecodeˉlimits.MAX_PARAMETERS_OR_LOCALS ||
                Function.Localˉtypes.Length > Bytecodeˉlimits.MAX_PARAMETERS_OR_LOCALS ||
                Function.Parameterˉtypes.Length + Function.Localˉtypes.Length >
                    Bytecodeˉlimits.MAX_PARAMETERS_OR_LOCALS)
            {
                Fail("WVB2131", $"Function '{Function.Name}' exceeds the local-slot limit.");
            }

            foreach (var Parameterˉtype in Function.Parameterˉtypes)
            {
                Verifyˉvalueˉshape(module, Parameterˉtype, allowˉvoid: false, "function parameter");
                if (Parameterˉtype.Kind == Valueˉtype.Builder)
                {
                    Fail("WVB2167", $"Function '{Function.Name}' has a builder parameter.");
                }
            }

            foreach (var Localˉtype in Function.Localˉtypes)
            {
                Verifyˉvalueˉshape(module, Localˉtype, allowˉvoid: false, "function local");
            }

            Verifyˉvalueˉshape(module, Function.Returnˉtype, allowˉvoid: true, "function return");
            if (Function.Returnˉtype.Kind == Valueˉtype.Builder)
            {
                Fail("WVB2167", $"Function '{Function.Name}' returns a builder.");
            }

            if (Function.Codeˉoffset != Expectedˉcodeˉoffset)
            {
                Fail(
                    "WVB2132",
                    $"Function '{Function.Name}' code does not begin at the canonical contiguous offset {Expectedˉcodeˉoffset}.");
            }

            if (Function.Codeˉlength <= 0 ||
                Function.Codeˉlength > Bytecodeˉlimits.MAX_CODE_BYTES_PER_FUNCTION)
            {
                Fail("WVB2133", $"Function '{Function.Name}' has an invalid code length.");
            }

            if (Function.Codeˉoffset < 0 ||
                Function.Codeˉlength > module.Code.Length - Function.Codeˉoffset)
            {
                Fail("WVB2134", $"Function '{Function.Name}' code range is outside the Code section.");
            }

            if (Function.Maximumˉstackˉdepth < 0 ||
                Function.Maximumˉstackˉdepth > Bytecodeˉlimits.MAX_OPERAND_STACK)
            {
                Fail("WVB2135", $"Function '{Function.Name}' has an invalid maximum stack depth.");
            }

            Expectedˉcodeˉoffset = checked(Expectedˉcodeˉoffset + Function.Codeˉlength);
        }

        if (Expectedˉcodeˉoffset != module.Code.Length)
        {
            Fail("WVB2136", "Function code ranges do not cover the complete Code section.");
        }
    }

    private static void Verifyˉtypes(Bytecodeˉmodule module)
    {
        Nominalˉtypeˉkind? Previousˉkind = null;
        string? Previousˉname = null;
        var Typeˉnames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var Type in module.Types)
        {
            if (Previousˉkind is not null &&
                (Type.Kind < Previousˉkind.Value ||
                    (Type.Kind == Previousˉkind.Value &&
                        StringComparer.Ordinal.Compare(Previousˉname, Type.Name) >= 0)))
            {
                Fail("WVB2157", "Nominal types must be grouped by kind and strictly sorted by name.");
            }

            Previousˉkind = Type.Kind;
            Previousˉname = Type.Name;
            if (!Seedˉnames.Isˉidentifier(Type.Name))
            {
                Fail("WVB2150", $"Nominal type name '{Type.Name}' is not a Seed identifier.");
            }

            if (!Typeˉnames.Add(Type.Name))
            {
                Fail("WVB2159", $"Nominal type name '{Type.Name}' is declared more than once.");
            }

            switch (Type)
            {
                case Recordˉtypeˉdeclaration Record:
                    Verifyˉrecordˉtype(module, Record);
                    break;
                case Enumˉtypeˉdeclaration Enum:
                    Verifyˉenumˉtype(Enum);
                    break;
                case Variantˉtypeˉdeclaration Variant:
                    Verifyˉvariantˉtype(module, Variant);
                    break;
                default:
                    Fail("WVB2158", $"Nominal type '{Type.Name}' has an inconsistent representation.");
                    break;
            }
        }
    }

    private static void Verifyˉrecordˉtype(
        Bytecodeˉmodule module,
        Recordˉtypeˉdeclaration type)
    {
        if (type.Fields.Length == 0 || type.Fields.Length > Bytecodeˉlimits.MAX_RECORD_FIELDS)
        {
            Fail("WVB2151", $"Record type '{type.Name}' has an invalid field count.");
        }

        var Fieldˉnames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var Field in type.Fields)
        {
            if (!Seedˉnames.Isˉidentifier(Field.Name) || !Fieldˉnames.Add(Field.Name))
            {
                Fail("WVB2152", $"Record type '{type.Name}' has an invalid or duplicate field '{Field.Name}'.");
            }

            Verifyˉvalueˉshape(module, Field.Type, allowˉvoid: false, "record field");
            if (Field.Type.Kind == Valueˉtype.Builder)
            {
                Fail("WVB2167", $"Record field '{type.Name}.{Field.Name}' cannot contain a builder.");
            }
            if (Field.Type.Kind == Valueˉtype.Record)
            {
                Fail("WVB2153", $"Record field '{type.Name}.{Field.Name}' cannot contain a record in Seed.");
            }
        }
    }

    private static void Verifyˉenumˉtype(Enumˉtypeˉdeclaration type)
    {
        if (type.Members.Length == 0 || type.Members.Length > Bytecodeˉlimits.MAX_ENUM_MEMBERS)
        {
            Fail("WVB2154", $"Enum type '{type.Name}' has an invalid member count.");
        }

        var Memberˉnames = new HashSet<string>(StringComparer.Ordinal);
        var Memberˉvalues = new HashSet<int>();
        foreach (var Member in type.Members)
        {
            if (!Seedˉnames.Isˉidentifier(Member.Name) || !Memberˉnames.Add(Member.Name))
            {
                Fail("WVB2155", $"Enum type '{type.Name}' has an invalid or duplicate member '{Member.Name}'.");
            }

            if (!Memberˉvalues.Add(Member.Value))
            {
                Fail("WVB2156", $"Enum type '{type.Name}' repeats value {Member.Value}.");
            }
        }
    }

    private static void Verifyˉvariantˉtype(
        Bytecodeˉmodule module,
        Variantˉtypeˉdeclaration type)
    {
        if (type.Cases.IsDefault ||
            type.Cases.Length == 0 ||
            type.Cases.Length > Bytecodeˉlimits.MAX_VARIANT_CASES)
        {
            Fail("WVB2162", $"Variant type '{type.Name}' has an invalid case count.");
        }

        var Caseˉnames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var Case in type.Cases)
        {
            if (!Seedˉnames.Isˉidentifier(Case.Name) || !Caseˉnames.Add(Case.Name))
            {
                Fail("WVB2163", $"Variant type '{type.Name}' has an invalid or duplicate case '{Case.Name}'.");
            }

            if ((Case.Payloadˉname is null) != (Case.Payloadˉtype is null))
            {
                Fail("WVB2164", $"Variant case '{type.Name}.{Case.Name}' has inconsistent payload metadata.");
            }
            if (Case.Payloadˉtype is not { } Payloadˉtype)
            {
                continue;
            }
            if (!Seedˉnames.Isˉidentifier(Case.Payloadˉname!))
            {
                Fail("WVB2165", $"Variant case '{type.Name}.{Case.Name}' has an invalid payload name.");
            }
            Verifyˉvalueˉshape(module, Payloadˉtype, allowˉvoid: false, "variant payload");
            if (Payloadˉtype.Kind == Valueˉtype.Builder)
            {
                Fail("WVB2167", $"Variant case '{type.Name}.{Case.Name}' cannot contain a builder.");
            }
            if (Payloadˉtype.Kind == Valueˉtype.Variant)
            {
                Fail("WVB2166", $"Variant case '{type.Name}.{Case.Name}' cannot contain a variant in WVB 1.9.");
            }
        }
    }

    private static void Verifyˉexports(Bytecodeˉmodule module)
    {
        Verifyˉstrictˉordering(module.Exports.Select(Export => Export.Name), "export");
        foreach (var Export in module.Exports)
        {
            if (!Seedˉnames.Isˉidentifier(Export.Name))
            {
                Fail("WVB2140", $"Export name '{Export.Name}' is not a Seed identifier.");
            }

            if (Export.Kind != Exportˉkind.Function)
            {
                Fail("WVB2141", $"Export '{Export.Name}' has an unsupported kind.");
            }

            if ((uint)Export.Targetˉindex >= (uint)module.Functions.Length)
            {
                Fail("WVB2142", $"Export '{Export.Name}' references an invalid function index.");
            }

            if (!StringComparer.Ordinal.Equals(Export.Name, module.Functions[Export.Targetˉindex].Name))
            {
                Fail("WVB2143", $"Export '{Export.Name}' does not match its function name.");
            }
        }
    }

    private static void Verifyˉfunction(
        Bytecodeˉmodule module,
        Functionˉdeclaration function,
        ImmutableArray<Decodedˉinstruction> instructions)
    {
        var Instructionsˉbyˉoffset = instructions.ToDictionary(
            Instruction => Instruction.Offset,
            Instruction => Instruction);
        var Entryˉstacks = new Dictionary<int, ImmutableArray<Valueˉshape>>();
        var Pending = new Queue<int>();
        var Maximumˉstack = 0;
        Mergeˉentry(0, [], Entryˉstacks, Pending, function.Name);

        while (Pending.TryDequeue(out var Offset))
        {
            var Instruction = Instructionsˉbyˉoffset[Offset];
            var Stack = Entryˉstacks[Offset].ToList();
            Simulateˉinstruction(module, function, Instruction, Stack);
            Maximumˉstack = Math.Max(Maximumˉstack, Stack.Count);
            if (Maximumˉstack > Bytecodeˉlimits.MAX_OPERAND_STACK)
            {
                Fail("WVB2200", $"Function '{function.Name}' exceeds the operand-stack limit.", Offset);
            }

            var Resultˉstack = Stack.ToImmutableArray();
            var Nextˉoffset = checked(Instruction.Offset + Instruction.Size);
            switch (Instruction.Opcode)
            {
                case Opcode.Jump:
                    Mergeˉbranchˉtarget(
                        Instruction.Unsignedˉoperand,
                        Resultˉstack,
                        Instructionsˉbyˉoffset,
                        Entryˉstacks,
                        Pending,
                        function.Name,
                        Instruction.Offset);
                    break;
                case Opcode.Branchˉfalse:
                    Mergeˉbranchˉtarget(
                        Instruction.Unsignedˉoperand,
                        Resultˉstack,
                        Instructionsˉbyˉoffset,
                        Entryˉstacks,
                        Pending,
                        function.Name,
                        Instruction.Offset);
                    Mergeˉfallthrough(
                        Nextˉoffset,
                        function.Codeˉlength,
                        Resultˉstack,
                        Instructionsˉbyˉoffset,
                        Entryˉstacks,
                        Pending,
                        function.Name,
                        Instruction.Offset);
                    break;
                case Opcode.Return:
                    break;
                default:
                    Mergeˉfallthrough(
                        Nextˉoffset,
                        function.Codeˉlength,
                        Resultˉstack,
                        Instructionsˉbyˉoffset,
                        Entryˉstacks,
                        Pending,
                        function.Name,
                        Instruction.Offset);
                    break;
            }
        }

        if (Entryˉstacks.Count != instructions.Length)
        {
            var Firstˉunreachable = instructions.First(Instruction => !Entryˉstacks.ContainsKey(Instruction.Offset));
            Fail(
                "WVB2201",
                $"Function '{function.Name}' contains unreachable instructions.",
                Firstˉunreachable.Offset);
        }

        if (Maximumˉstack != function.Maximumˉstackˉdepth)
        {
            Fail(
                "WVB2202",
                $"Function '{function.Name}' declares maximum stack {function.Maximumˉstackˉdepth}, but verification computed {Maximumˉstack}.");
        }
    }

    private static void Simulateˉinstruction(
        Bytecodeˉmodule module,
        Functionˉdeclaration function,
        Decodedˉinstruction instruction,
        List<Valueˉshape> stack)
    {
        switch (instruction.Opcode)
        {
            case Opcode.I32ˉconst:
                Push(stack, Valueˉtype.I32);
                break;
            case Opcode.I64ˉconst:
                Push(stack, Valueˉtype.I64);
                break;
            case Opcode.Boolˉconst:
                Push(stack, Valueˉtype.Bool);
                break;
            case Opcode.U8ˉconst:
                Push(stack, Valueˉtype.U8);
                break;
            case Opcode.U32ˉconst:
                Push(stack, Valueˉtype.U32);
                break;
            case Opcode.U64ˉconst:
                Push(stack, Valueˉtype.U64);
                break;
            case Opcode.Textˉconst:
                Requireˉdataˉtype(module, instruction, Dataˉtype.Text, function.Name);
                Push(stack, Valueˉtype.Text);
                break;
            case Opcode.Bytesˉconst:
                Requireˉdataˉtype(module, instruction, Dataˉtype.Bytes, function.Name);
                Push(stack, Valueˉtype.Bytes);
                break;
            case Opcode.Localˉload:
                Push(stack, Getˉlocalˉtype(function, instruction));
                break;
            case Opcode.Localˉstore:
                Pop(stack, Getˉlocalˉtype(function, instruction), function.Name, instruction.Offset);
                break;
            case Opcode.Dataˉlength:
                Requireˉdataˉtype(module, instruction, Dataˉtype.I32ˉarray, function.Name);
                Push(stack, Valueˉtype.I32);
                break;
            case Opcode.Dataˉloadˉi32:
                Requireˉdataˉtype(module, instruction, Dataˉtype.I32ˉarray, function.Name);
                Pop(stack, Valueˉtype.I32, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.I32);
                break;
            case Opcode.Bytesˉlength:
                Pop(stack, Valueˉtype.Bytes, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.U32);
                break;
            case Opcode.Bytesˉslice:
                Pop(stack, Valueˉtype.U32, function.Name, instruction.Offset);
                Pop(stack, Valueˉtype.U32, function.Name, instruction.Offset);
                Pop(stack, Valueˉtype.Bytes, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.Bytes);
                break;
            case Opcode.Bytesˉreadˉu8:
                Pop(stack, Valueˉtype.U32, function.Name, instruction.Offset);
                Pop(stack, Valueˉtype.Bytes, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.U8);
                break;
            case Opcode.Bytesˉreadˉu16ˉlittle:
            case Opcode.Bytesˉreadˉu32ˉlittle:
                Pop(stack, Valueˉtype.U32, function.Name, instruction.Offset);
                Pop(stack, Valueˉtype.Bytes, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.U32);
                break;
            case Opcode.Bytesˉreadˉi32ˉlittle:
                Pop(stack, Valueˉtype.U32, function.Name, instruction.Offset);
                Pop(stack, Valueˉtype.Bytes, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.I32);
                break;
            case Opcode.I32ˉadd:
            case Opcode.I32ˉsubtract:
            case Opcode.I32ˉmultiply:
            case Opcode.I32ˉdivide:
            case Opcode.I32ˉremainder:
                Pop(stack, Valueˉtype.I32, function.Name, instruction.Offset);
                Pop(stack, Valueˉtype.I32, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.I32);
                break;
            case Opcode.I32ˉnegate:
                Pop(stack, Valueˉtype.I32, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.I32);
                break;
            case Opcode.I64ˉadd:
            case Opcode.I64ˉsubtract:
            case Opcode.I64ˉmultiply:
            case Opcode.I64ˉdivide:
            case Opcode.I64ˉremainder:
                Pop(stack, Valueˉtype.I64, function.Name, instruction.Offset);
                Pop(stack, Valueˉtype.I64, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.I64);
                break;
            case Opcode.I64ˉnegate:
                Pop(stack, Valueˉtype.I64, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.I64);
                break;
            case Opcode.U32ˉadd:
            case Opcode.U32ˉsubtract:
            case Opcode.U32ˉmultiply:
            case Opcode.U32ˉdivide:
            case Opcode.U32ˉremainder:
            case Opcode.U32ˉbitwiseˉand:
            case Opcode.U32ˉbitwiseˉor:
            case Opcode.U32ˉbitwiseˉxor:
            case Opcode.U32ˉshiftˉleft:
            case Opcode.U32ˉshiftˉright:
                Pop(stack, Valueˉtype.U32, function.Name, instruction.Offset);
                Pop(stack, Valueˉtype.U32, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.U32);
                break;
            case Opcode.U64ˉadd:
            case Opcode.U64ˉsubtract:
            case Opcode.U64ˉmultiply:
            case Opcode.U64ˉdivide:
            case Opcode.U64ˉremainder:
            case Opcode.U64ˉbitwiseˉand:
            case Opcode.U64ˉbitwiseˉor:
            case Opcode.U64ˉbitwiseˉxor:
                Pop(stack, Valueˉtype.U64, function.Name, instruction.Offset);
                Pop(stack, Valueˉtype.U64, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.U64);
                break;
            case Opcode.U8ˉbitwiseˉand:
            case Opcode.U8ˉbitwiseˉor:
            case Opcode.U8ˉbitwiseˉxor:
                Pop(stack, Valueˉtype.U8, function.Name, instruction.Offset);
                Pop(stack, Valueˉtype.U8, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.U8);
                break;
            case Opcode.U8ˉbitwiseˉnot:
                Pop(stack, Valueˉtype.U8, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.U8);
                break;
            case Opcode.U8ˉshiftˉleft:
            case Opcode.U8ˉshiftˉright:
                Pop(stack, Valueˉtype.U32, function.Name, instruction.Offset);
                Pop(stack, Valueˉtype.U8, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.U8);
                break;
            case Opcode.U32ˉbitwiseˉnot:
                Pop(stack, Valueˉtype.U32, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.U32);
                break;
            case Opcode.U64ˉbitwiseˉnot:
                Pop(stack, Valueˉtype.U64, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.U64);
                break;
            case Opcode.U64ˉshiftˉleft:
            case Opcode.U64ˉshiftˉright:
                Pop(stack, Valueˉtype.U32, function.Name, instruction.Offset);
                Pop(stack, Valueˉtype.U64, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.U64);
                break;
            case Opcode.I32ˉequal:
            case Opcode.I32ˉnotˉequal:
            case Opcode.I32ˉless:
            case Opcode.I32ˉlessˉequal:
            case Opcode.I32ˉgreater:
            case Opcode.I32ˉgreaterˉequal:
                Pop(stack, Valueˉtype.I32, function.Name, instruction.Offset);
                Pop(stack, Valueˉtype.I32, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.Bool);
                break;
            case Opcode.I64ˉequal:
            case Opcode.I64ˉnotˉequal:
            case Opcode.I64ˉless:
            case Opcode.I64ˉlessˉequal:
            case Opcode.I64ˉgreater:
            case Opcode.I64ˉgreaterˉequal:
                Pop(stack, Valueˉtype.I64, function.Name, instruction.Offset);
                Pop(stack, Valueˉtype.I64, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.Bool);
                break;
            case Opcode.Boolˉequal:
            case Opcode.Boolˉnotˉequal:
                Pop(stack, Valueˉtype.Bool, function.Name, instruction.Offset);
                Pop(stack, Valueˉtype.Bool, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.Bool);
                break;
            case Opcode.Boolˉnot:
                Pop(stack, Valueˉtype.Bool, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.Bool);
                break;
            case Opcode.U32ˉequal:
            case Opcode.U32ˉnotˉequal:
            case Opcode.U32ˉless:
            case Opcode.U32ˉlessˉequal:
            case Opcode.U32ˉgreater:
            case Opcode.U32ˉgreaterˉequal:
                Pop(stack, Valueˉtype.U32, function.Name, instruction.Offset);
                Pop(stack, Valueˉtype.U32, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.Bool);
                break;
            case Opcode.U64ˉequal:
            case Opcode.U64ˉnotˉequal:
            case Opcode.U64ˉless:
            case Opcode.U64ˉlessˉequal:
            case Opcode.U64ˉgreater:
            case Opcode.U64ˉgreaterˉequal:
                Pop(stack, Valueˉtype.U64, function.Name, instruction.Offset);
                Pop(stack, Valueˉtype.U64, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.Bool);
                break;
            case Opcode.U8ˉequal:
            case Opcode.U8ˉnotˉequal:
                Pop(stack, Valueˉtype.U8, function.Name, instruction.Offset);
                Pop(stack, Valueˉtype.U8, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.Bool);
                break;
            case Opcode.Textˉequal:
            case Opcode.Textˉnotˉequal:
                Pop(stack, Valueˉtype.Text, function.Name, instruction.Offset);
                Pop(stack, Valueˉtype.Text, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.Bool);
                break;
            case Opcode.Bytesˉequal:
            case Opcode.Bytesˉnotˉequal:
                Pop(stack, Valueˉtype.Bytes, function.Name, instruction.Offset);
                Pop(stack, Valueˉtype.Bytes, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.Bool);
                break;
            case Opcode.Enumˉconst:
                var Enumˉtype = Getˉenumˉtype(module, instruction, function.Name);
                if (instruction.Secondˉunsignedˉoperand >= (uint)Enumˉtype.Members.Length)
                {
                    Fail(
                        "WVB2225",
                        $"Function '{function.Name}' references invalid member {instruction.Secondˉunsignedˉoperand} on enum '{Enumˉtype.Name}'.",
                        instruction.Offset);
                }

                Push(stack, Valueˉshape.Forˉenum((int)instruction.Unsignedˉoperand));
                break;
            case Opcode.Enumˉequal:
            case Opcode.Enumˉnotˉequal:
                var Rightˉenum = Popˉany(stack, function.Name, instruction.Offset);
                var Leftˉenum = Popˉany(stack, function.Name, instruction.Offset);
                if (Leftˉenum.Kind != Valueˉtype.Enum || Leftˉenum != Rightˉenum)
                {
                    Fail("WVB2224", $"Function '{function.Name}' compares incompatible enum values.", instruction.Offset);
                }

                Push(stack, Valueˉtype.Bool);
                break;
            case Opcode.Enumˉname:
                var Namedˉenum = Popˉany(stack, function.Name, instruction.Offset);
                if (Namedˉenum.Kind != Valueˉtype.Enum)
                {
                    Fail("WVB2226", $"Function '{function.Name}' names a non-enum value.", instruction.Offset);
                }

                Push(stack, Valueˉtype.Text);
                break;
            case Opcode.Variantˉcreate:
                var Createdˉvariant = Getˉvariantˉtype(module, instruction, function.Name);
                var Createdˉcase = Getˉvariantˉcase(Createdˉvariant, instruction, function.Name);
                if (Createdˉcase.Payloadˉtype is { } Createdˉpayload)
                {
                    Pop(stack, Createdˉpayload, function.Name, instruction.Offset);
                }
                Push(stack, Valueˉshape.Forˉvariant((int)instruction.Unsignedˉoperand));
                break;
            case Opcode.Variantˉisˉcase:
                var Testedˉvariant = Getˉvariantˉtype(module, instruction, function.Name);
                _ = Getˉvariantˉcase(Testedˉvariant, instruction, function.Name);
                Pop(
                    stack,
                    Valueˉshape.Forˉvariant((int)instruction.Unsignedˉoperand),
                    function.Name,
                    instruction.Offset);
                Push(stack, Valueˉtype.Bool);
                break;
            case Opcode.Variantˉpayload:
                var Payloadˉvariant = Getˉvariantˉtype(module, instruction, function.Name);
                var Payloadˉcase = Getˉvariantˉcase(Payloadˉvariant, instruction, function.Name);
                var Payloadˉshape = Payloadˉcase.Payloadˉtype.GetValueOrDefault();
                if (Payloadˉcase.Payloadˉtype is null)
                {
                    Fail(
                        "WVB2227",
                        $"Function '{function.Name}' reads absent payload from variant case '{Payloadˉvariant.Name}.{Payloadˉcase.Name}'.",
                        instruction.Offset);
                }
                Pop(
                    stack,
                    Valueˉshape.Forˉvariant((int)instruction.Unsignedˉoperand),
                    function.Name,
                    instruction.Offset);
                Push(stack, Payloadˉshape);
                break;
            case Opcode.Builderˉcreate:
                var Builderˉelement = Decodeˉcollectionˉelement(
                    module, instruction.Unsignedˉoperand, function.Name, instruction.Offset);
                if (instruction.Secondˉunsignedˉoperand is 0 or > Bytecodeˉlimits.MAX_SEQUENCE_ELEMENTS)
                {
                    Fail(
                        "WVB2228",
                        $"Function '{function.Name}' constructs a builder with invalid maximum {instruction.Secondˉunsignedˉoperand}.",
                        instruction.Offset);
                }
                Push(
                    stack,
                    Valueˉshape.Forˉbuilder(
                        Builderˉelement, instruction.Secondˉunsignedˉoperand));
                break;
            case Opcode.Builderˉpush:
                var Pushedˉelement = Popˉany(stack, function.Name, instruction.Offset);
                var Pushedˉbuilder = Popˉany(stack, function.Name, instruction.Offset);
                if (Pushedˉbuilder.Kind != Valueˉtype.Builder ||
                    Pushedˉbuilder.Elementˉshape != Pushedˉelement)
                {
                    Fail(
                        "WVB2229",
                        $"Function '{function.Name}' pushes an incompatible builder element.",
                        instruction.Offset);
                }
                Push(stack, Pushedˉbuilder);
                break;
            case Opcode.Builderˉfreeze:
                var Frozenˉbuilder = Popˉany(stack, function.Name, instruction.Offset);
                if (Frozenˉbuilder.Kind != Valueˉtype.Builder)
                {
                    Fail(
                        "WVB2230",
                        $"Function '{function.Name}' freezes a non-builder value.",
                        instruction.Offset);
                }
                Push(
                    stack,
                    Valueˉshape.Forˉsequence(
                        Frozenˉbuilder.Elementˉshape, Frozenˉbuilder.Maximum));
                break;
            case Opcode.Sequenceˉlength:
                var Lengthˉsequence = Popˉany(stack, function.Name, instruction.Offset);
                if (Lengthˉsequence.Kind != Valueˉtype.Sequence)
                {
                    Fail(
                        "WVB2231",
                        $"Function '{function.Name}' reads the length of a non-sequence value.",
                        instruction.Offset);
                }
                Push(stack, Valueˉtype.U32);
                break;
            case Opcode.Sequenceˉelement:
                Pop(stack, Valueˉtype.U32, function.Name, instruction.Offset);
                var Indexedˉsequence = Popˉany(stack, function.Name, instruction.Offset);
                if (Indexedˉsequence.Kind != Valueˉtype.Sequence)
                {
                    Fail(
                        "WVB2232",
                        $"Function '{function.Name}' indexes a non-sequence value.",
                        instruction.Offset);
                }
                Push(stack, Indexedˉsequence.Elementˉshape);
                break;
            case Opcode.I32ˉformat:
                Pop(stack, Valueˉtype.I32, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.Text);
                break;
            case Opcode.I64ˉformat:
                Pop(stack, Valueˉtype.I64, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.Text);
                break;
            case Opcode.U8ˉformat:
                Pop(stack, Valueˉtype.U8, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.Text);
                break;
            case Opcode.U32ˉformat:
                Pop(stack, Valueˉtype.U32, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.Text);
                break;
            case Opcode.U64ˉformat:
                Pop(stack, Valueˉtype.U64, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.Text);
                break;
            case Opcode.U32ˉfromˉu8:
                Pop(stack, Valueˉtype.U8, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.U32);
                break;
            case Opcode.Textˉconcat:
                Pop(stack, Valueˉtype.Text, function.Name, instruction.Offset);
                Pop(stack, Valueˉtype.Text, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.Text);
                break;
            case Opcode.Textˉutf8ˉisˉvalid:
                Pop(stack, Valueˉtype.Bytes, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.Bool);
                break;
            case Opcode.Textˉfromˉutf8:
                Pop(stack, Valueˉtype.Bytes, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.Text);
                break;
            case Opcode.Textˉquote:
                Pop(stack, Valueˉtype.Text, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.Text);
                break;
            case Opcode.Bytesˉconcat:
                Pop(stack, Valueˉtype.Bytes, function.Name, instruction.Offset);
                Pop(stack, Valueˉtype.Bytes, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.Bytes);
                break;
            case Opcode.Bytesˉfromˉu8:
                Pop(stack, Valueˉtype.U8, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.Bytes);
                break;
            case Opcode.Bytesˉfromˉu16ˉlittle:
            case Opcode.Bytesˉfromˉu32ˉlittle:
                Pop(stack, Valueˉtype.U32, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.Bytes);
                break;
            case Opcode.Bytesˉfromˉi32ˉlittle:
                Pop(stack, Valueˉtype.I32, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.Bytes);
                break;
            case Opcode.Bytesˉsha256ˉhex:
                Pop(stack, Valueˉtype.Bytes, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.Text);
                break;
            case Opcode.Textˉtoˉutf8:
                Pop(stack, Valueˉtype.Text, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.Bytes);
                break;
            case Opcode.Recordˉcreate:
                var Recordˉtype = Getˉrecordˉtype(module, instruction, function.Name);
                for (var Fieldˉindex = Recordˉtype.Fields.Length - 1; Fieldˉindex >= 0; Fieldˉindex--)
                {
                    Pop(stack, Recordˉtype.Fields[Fieldˉindex].Type, function.Name, instruction.Offset);
                }

                Push(stack, Valueˉshape.Forˉrecord((int)instruction.Unsignedˉoperand));
                break;
            case Opcode.Recordˉfield:
                var Sourceˉshape = Popˉany(stack, function.Name, instruction.Offset);
                if (Sourceˉshape.Kind != Valueˉtype.Record ||
                    (uint)Sourceˉshape.Nominalˉtypeˉindex >= (uint)module.Types.Length ||
                    module.Types[Sourceˉshape.Nominalˉtypeˉindex] is not Recordˉtypeˉdeclaration)
                {
                    Fail("WVB2222", $"Function '{function.Name}' reads a field from a non-record value.", instruction.Offset);
                }

                var Sourceˉtype = (Recordˉtypeˉdeclaration)module.Types[Sourceˉshape.Nominalˉtypeˉindex];
                if (instruction.Unsignedˉoperand >= (uint)Sourceˉtype.Fields.Length)
                {
                    Fail("WVB2223", $"Function '{function.Name}' references invalid field {instruction.Unsignedˉoperand} on record '{Sourceˉtype.Name}'.", instruction.Offset);
                }

                Push(stack, Sourceˉtype.Fields[(int)instruction.Unsignedˉoperand].Type);
                break;
            case Opcode.Jump:
                break;
            case Opcode.Branchˉfalse:
                Pop(stack, Valueˉtype.Bool, function.Name, instruction.Offset);
                break;
            case Opcode.Call:
                var Calledˉfunction = Getˉfunction(module, instruction, function.Name);
                Popˉparameters(stack, Calledˉfunction.Parameterˉtypes, function.Name, instruction.Offset);
                if (Calledˉfunction.Returnˉtype != Valueˉtype.Void)
                {
                    Push(stack, Calledˉfunction.Returnˉtype);
                }

                break;
            case Opcode.Callˉcapability:
                var Capability = Getˉcapability(module, instruction, function.Name);
                Popˉparameters(stack, Capability.Parameterˉtypes, function.Name, instruction.Offset);
                if (Capability.Returnˉtype != Valueˉtype.Void)
                {
                    Push(stack, Capability.Returnˉtype);
                }

                break;
            case Opcode.Pop:
                Popˉany(stack, function.Name, instruction.Offset);
                break;
            case Opcode.Return:
                if (function.Returnˉtype != Valueˉtype.Void)
                {
                    Pop(stack, function.Returnˉtype, function.Name, instruction.Offset);
                }

                if (stack.Count != 0)
                {
                    Fail(
                        "WVB2203",
                        $"Return in function '{function.Name}' leaves values on the operand stack.",
                        instruction.Offset);
                }

                break;
            default:
                Fail("WVB2204", $"Opcode '{instruction.Opcode}' has no verifier implementation.");
                break;
        }
    }

    private static Valueˉshape Getˉlocalˉtype(
        Functionˉdeclaration function,
        Decodedˉinstruction instruction)
    {
        if (instruction.Unsignedˉoperand >= (uint)function.Allˉlocalˉtypes.Length)
        {
            Fail(
                "WVB2210",
                $"Function '{function.Name}' references invalid local {instruction.Unsignedˉoperand}.",
                instruction.Offset);
        }

        return function.Allˉlocalˉtypes[(int)instruction.Unsignedˉoperand];
    }

    private static void Requireˉdataˉtype(
        Bytecodeˉmodule module,
        Decodedˉinstruction instruction,
        Dataˉtype requiredˉtype,
        string functionˉname)
    {
        if (instruction.Unsignedˉoperand >= (uint)module.Data.Length)
        {
            Fail(
                "WVB2211",
                $"Function '{functionˉname}' references invalid data {instruction.Unsignedˉoperand}.",
                instruction.Offset);
        }

        var Data = module.Data[(int)instruction.Unsignedˉoperand];
        if (Data.Type != requiredˉtype)
        {
            Fail(
                "WVB2212",
                $"Function '{functionˉname}' uses data '{Data.Name}' as {requiredˉtype}, but it is {Data.Type}.",
                instruction.Offset);
        }
    }

    private static Functionˉdeclaration Getˉfunction(
        Bytecodeˉmodule module,
        Decodedˉinstruction instruction,
        string functionˉname)
    {
        if (instruction.Unsignedˉoperand >= (uint)module.Functions.Length)
        {
            Fail(
                "WVB2213",
                $"Function '{functionˉname}' calls invalid function {instruction.Unsignedˉoperand}.",
                instruction.Offset);
        }

        return module.Functions[(int)instruction.Unsignedˉoperand];
    }

    private static Capabilityˉdeclaration Getˉcapability(
        Bytecodeˉmodule module,
        Decodedˉinstruction instruction,
        string functionˉname)
    {
        if (instruction.Unsignedˉoperand >= (uint)module.Capabilities.Length)
        {
            Fail(
                "WVB2214",
                $"Function '{functionˉname}' calls invalid capability {instruction.Unsignedˉoperand}.",
                instruction.Offset);
        }

        return module.Capabilities[(int)instruction.Unsignedˉoperand];
    }

    private static Recordˉtypeˉdeclaration Getˉrecordˉtype(
        Bytecodeˉmodule module,
        Decodedˉinstruction instruction,
        string functionˉname)
    {
        if (instruction.Unsignedˉoperand >= (uint)module.Types.Length)
        {
            Fail(
                "WVB2215",
                $"Function '{functionˉname}' constructs invalid record type {instruction.Unsignedˉoperand}.",
                instruction.Offset);
        }

        if (module.Types[(int)instruction.Unsignedˉoperand] is Recordˉtypeˉdeclaration Record)
        {
            return Record;
        }

        Fail(
            "WVB2216",
            $"Function '{functionˉname}' constructs non-record type {instruction.Unsignedˉoperand}.",
            instruction.Offset);
        return null!;
    }

    private static Enumˉtypeˉdeclaration Getˉenumˉtype(
        Bytecodeˉmodule module,
        Decodedˉinstruction instruction,
        string functionˉname)
    {
        if (instruction.Unsignedˉoperand >= (uint)module.Types.Length)
        {
            Fail(
                "WVB2217",
                $"Function '{functionˉname}' references invalid enum type {instruction.Unsignedˉoperand}.",
                instruction.Offset);
        }

        if (module.Types[(int)instruction.Unsignedˉoperand] is Enumˉtypeˉdeclaration Enum)
        {
            return Enum;
        }

        Fail(
            "WVB2217",
            $"Function '{functionˉname}' references invalid enum type {instruction.Unsignedˉoperand}.",
            instruction.Offset);
        return null!;
    }

    private static Variantˉtypeˉdeclaration Getˉvariantˉtype(
        Bytecodeˉmodule module,
        Decodedˉinstruction instruction,
        string functionˉname)
    {
        if (instruction.Unsignedˉoperand >= (uint)module.Types.Length ||
            module.Types[(int)instruction.Unsignedˉoperand] is not Variantˉtypeˉdeclaration Variant)
        {
            Fail(
                "WVB2228",
                $"Function '{functionˉname}' references invalid variant type {instruction.Unsignedˉoperand}.",
                instruction.Offset);
            return null!;
        }

        return Variant;
    }

    private static Variantˉcaseˉdeclaration Getˉvariantˉcase(
        Variantˉtypeˉdeclaration variant,
        Decodedˉinstruction instruction,
        string functionˉname)
    {
        if (instruction.Secondˉunsignedˉoperand >= (uint)variant.Cases.Length)
        {
            Fail(
                "WVB2229",
                $"Function '{functionˉname}' references invalid case {instruction.Secondˉunsignedˉoperand} on variant '{variant.Name}'.",
                instruction.Offset);
        }

        return variant.Cases[(int)instruction.Secondˉunsignedˉoperand];
    }

    private static void Popˉparameters(
        List<Valueˉshape> stack,
        ImmutableArray<Valueˉshape> parameters,
        string functionˉname,
        int offset)
    {
        for (var Index = parameters.Length - 1; Index >= 0; Index--)
        {
            Pop(stack, parameters[Index], functionˉname, offset);
        }
    }

    private static void Popˉparameters(
        List<Valueˉshape> stack,
        ImmutableArray<Valueˉtype> parameters,
        string functionˉname,
        int offset)
    {
        for (var Index = parameters.Length - 1; Index >= 0; Index--)
        {
            Pop(stack, parameters[Index], functionˉname, offset);
        }
    }

    private static void Push(List<Valueˉshape> stack, Valueˉshape type)
    {
        stack.Add(type);
    }

    private static void Pop(
        List<Valueˉshape> stack,
        Valueˉshape expectedˉtype,
        string functionˉname,
        int offset)
    {
        var Actualˉtype = Popˉany(stack, functionˉname, offset);
        if (Actualˉtype != expectedˉtype)
        {
            Fail(
                "WVB2220",
                $"Function '{functionˉname}' expected {expectedˉtype} on the stack but found {Actualˉtype}.",
                offset);
        }
    }

    private static Valueˉshape Popˉany(List<Valueˉshape> stack, string functionˉname, int offset)
    {
        if (stack.Count == 0)
        {
            Fail("WVB2221", $"Function '{functionˉname}' underflows the operand stack.", offset);
        }

        var Lastˉindex = stack.Count - 1;
        var Result = stack[Lastˉindex];
        stack.RemoveAt(Lastˉindex);
        return Result;
    }

    private static void Mergeˉfallthrough(
        int nextˉoffset,
        int codeˉlength,
        ImmutableArray<Valueˉshape> stack,
        Dictionary<int, Decodedˉinstruction> instructions,
        Dictionary<int, ImmutableArray<Valueˉshape>> entryˉstacks,
        Queue<int> pending,
        string functionˉname,
        int sourceˉoffset)
    {
        if (nextˉoffset >= codeˉlength || !instructions.ContainsKey(nextˉoffset))
        {
            Fail(
                "WVB2230",
                $"Control falls past the end of function '{functionˉname}'.",
                sourceˉoffset);
        }

        Mergeˉentry(nextˉoffset, stack, entryˉstacks, pending, functionˉname);
    }

    private static void Mergeˉbranchˉtarget(
        uint rawˉtarget,
        ImmutableArray<Valueˉshape> stack,
        Dictionary<int, Decodedˉinstruction> instructions,
        Dictionary<int, ImmutableArray<Valueˉshape>> entryˉstacks,
        Queue<int> pending,
        string functionˉname,
        int sourceˉoffset)
    {
        if (rawˉtarget > int.MaxValue || !instructions.ContainsKey((int)rawˉtarget))
        {
            Fail(
                "WVB2231",
                $"Function '{functionˉname}' branches to invalid instruction offset {rawˉtarget}.",
                sourceˉoffset);
        }

        Mergeˉentry((int)rawˉtarget, stack, entryˉstacks, pending, functionˉname);
    }

    private static void Mergeˉentry(
        int offset,
        ImmutableArray<Valueˉshape> stack,
        Dictionary<int, ImmutableArray<Valueˉshape>> entryˉstacks,
        Queue<int> pending,
        string functionˉname)
    {
        if (entryˉstacks.TryGetValue(offset, out var Existing))
        {
            if (!Existing.SequenceEqual(stack))
            {
                Fail(
                    "WVB2232",
                    $"Function '{functionˉname}' has inconsistent operand-stack types at offset {offset}.",
                    offset);
            }

            return;
        }

        entryˉstacks.Add(offset, stack);
        pending.Enqueue(offset);
    }

    private static void Verifyˉstrictˉordering(IEnumerable<string> names, string kind)
    {
        string? Previous = null;
        foreach (var Name in names)
        {
            if (Previous is not null && StringComparer.Ordinal.Compare(Previous, Name) >= 0)
            {
                Fail(
                    "WVB2240",
                    $"{kind} names must be unique and strictly sorted; '{Name}' follows '{Previous}'.");
            }

            Previous = Name;
        }
    }

    private static void Verifyˉvalueˉtype(Valueˉtype type, bool allowˉvoid, string position)
    {
        if (!Enum.IsDefined(type) || (!allowˉvoid && type == Valueˉtype.Void))
        {
            Fail("WVB2241", $"Value type '{type}' is invalid for a {position}.");
        }
    }

    private static void Verifyˉvalueˉshape(
        Bytecodeˉmodule module,
        Valueˉshape shape,
        bool allowˉvoid,
        string position)
    {
        Verifyˉvalueˉtype(shape.Kind, allowˉvoid, position);
        if (module.Formatˉminorˉversion == Moduleˉcodec.BASE_MINOR_VERSION &&
            shape.Kind is Valueˉtype.I64 or Valueˉtype.U64)
        {
            Fail(
                "WVB2107",
                $"Value type '{shape.Kind}' requires WVB 1.{Moduleˉcodec.WIDE_MINOR_VERSION} for a {position}.");
        }

        if (shape.Kind == Valueˉtype.Variant &&
            module.Formatˉminorˉversion < Moduleˉcodec.VARIANT_MINOR_VERSION)
        {
            Fail("WVB2107", $"Value type 'Variant' requires WVB 1.9 for a {position}.");
        }

        if (shape.Kind is Valueˉtype.Sequence or Valueˉtype.Builder)
        {
            if (module.Formatˉminorˉversion < Moduleˉcodec.COLLECTION_MINOR_VERSION)
            {
                Fail("WVB2107", $"Value type '{shape.Kind}' requires WVB 1.10 for a {position}.");
            }
            if (shape.Maximum is 0 or > Bytecodeˉlimits.MAX_SEQUENCE_ELEMENTS)
            {
                Fail("WVB2245", $"Collection maximum {shape.Maximum} is invalid for a {position}.");
            }
            if (shape.Elementˉshape.Kind is Valueˉtype.Void or Valueˉtype.Sequence or Valueˉtype.Builder)
            {
                Fail("WVB2246", $"Collection element type '{shape.Elementˉshape}' is invalid for a {position}.");
            }
            Verifyˉvalueˉshape(module, shape.Elementˉshape, allowˉvoid: false, $"{position} element");
            if (shape.Nominalˉtypeˉindex != -1)
            {
                Fail("WVB2243", $"Collection type '{shape.Kind}' carries a nominal type index in a {position}.");
            }
            return;
        }

        if (shape.Kind is Valueˉtype.Record or Valueˉtype.Enum or Valueˉtype.Variant)
        {
            if ((uint)shape.Nominalˉtypeˉindex >= (uint)module.Types.Length)
            {
                Fail("WVB2242", $"Nominal type index {shape.Nominalˉtypeˉindex} is invalid for a {position}.");
            }

            var Expectedˉkind = shape.Kind switch
            {
                Valueˉtype.Record => Nominalˉtypeˉkind.Record,
                Valueˉtype.Enum => Nominalˉtypeˉkind.Enum,
                _ => Nominalˉtypeˉkind.Variant,
            };
            if (module.Types[shape.Nominalˉtypeˉindex].Kind != Expectedˉkind)
            {
                Fail("WVB2244", $"Nominal type index {shape.Nominalˉtypeˉindex} has the wrong kind for a {position}.");
            }
        }
        else if (shape.Nominalˉtypeˉindex != -1)
        {
            Fail("WVB2243", $"Primitive type '{shape.Kind}' carries a nominal type index in a {position}.");
        }
        if (shape.Elementˉkind != Valueˉtype.Void ||
            shape.Elementˉnominalˉtypeˉindex != -1 ||
            shape.Maximum != 0)
        {
            Fail("WVB2247", $"Non-collection type '{shape.Kind}' carries collection metadata in a {position}.");
        }
    }

    private static Valueˉshape Decodeˉcollectionˉelement(
        Bytecodeˉmodule module,
        uint descriptor,
        string functionˉname,
        int instructionˉoffset)
    {
        var Tag = descriptor >> 30;
        var Payload = descriptor & 0x3FFF_FFFFu;
        Valueˉshape Shape;
        if (Tag == 0)
        {
            if (Payload > byte.MaxValue ||
                !Enum.IsDefined(typeof(Valueˉtype), (byte)Payload))
            {
                Fail("WVB2233", $"Function '{functionˉname}' has invalid collection element descriptor {descriptor}.", instructionˉoffset);
            }
            Shape = (Valueˉtype)Payload;
        }
        else
        {
            if (Payload > int.MaxValue)
            {
                Fail("WVB2233", $"Function '{functionˉname}' has invalid collection element descriptor {descriptor}.", instructionˉoffset);
            }
            Shape = Tag switch
            {
                1 => Valueˉshape.Forˉrecord((int)Payload),
                2 => Valueˉshape.Forˉenum((int)Payload),
                _ => Valueˉshape.Forˉvariant((int)Payload),
            };
        }
        Verifyˉvalueˉshape(module, Shape, allowˉvoid: false, "builder constructor element");
        if (Shape.Kind is Valueˉtype.Void or Valueˉtype.Sequence or Valueˉtype.Builder)
        {
            Fail("WVB2233", $"Function '{functionˉname}' has invalid collection element descriptor {descriptor}.", instructionˉoffset);
        }
        return Shape;
    }

    private static void Fail(string code, string message, int? byteˉoffset = null)
    {
        throw new Moduleˉverificationˉexception(code, message, byteˉoffset);
    }
}
