using System.Collections.Immutable;
using Windvale.Bytecode;

namespace Windvale.Compiler;

internal static class Semanticˉcompiler
{
    public static Wirˉmodule Compile(Moduleˉsyntax syntax, Diagnosticˉbag diagnostics)
    {
        var Context = new Moduleˉcontext(syntax, diagnostics);
        return Context.Compile();
    }

    private sealed class Moduleˉcontext(Moduleˉsyntax syntax, Diagnosticˉbag diagnostics)
    {
        private readonly Dictionary<string, Capabilityˉdeclaration> Capabilities =
            new(StringComparer.Ordinal);
        private readonly Dictionary<string, Dataˉdeclaration> Data =
            new(StringComparer.Ordinal);
        private readonly Dictionary<string, Constantˉvalue> Constants =
            new(StringComparer.Ordinal);
        private readonly Dictionary<string, Functionˉsymbol> Functions =
            new(StringComparer.Ordinal);
        private readonly Dictionary<string, Recordˉsymbol> Records =
            new(StringComparer.Ordinal);
        private readonly Dictionary<string, Enumˉsymbol> Enums =
            new(StringComparer.Ordinal);
        private readonly Dictionary<string, Variantˉsymbol> Variants =
            new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> Textˉdataˉbyˉvalue =
            new(StringComparer.Ordinal);
        private int Syntheticˉtextˉcounter;

        public Wirˉmodule Compile()
        {
            Bindˉmoduleˉname();
            var Profile = Bindˉprofile(syntax.Profile);
            Bindˉcapabilities(Profile);
            var Metadata = Bindˉmetadata(Profile);
            Bindˉdata();
            Bindˉnominalˉtypes();
            Bindˉconstants();
            Bindˉfunctionˉsignatures();

            var Wirˉfunctions = ImmutableArray.CreateBuilder<Wirˉfunction>(Functions.Count);
            foreach (var Function in Functions.Values.OrderBy(Function => Function.Name, StringComparer.Ordinal))
            {
                var Builder = new Functionˉbuilder(
                    Function,
                    diagnostics,
                    Data,
                    Constants,
                    Functions,
                    Capabilities,
                    Records,
                    Enums,
                    Variants,
                    Getˉorˉaddˉtextˉdata);
                Wirˉfunctions.Add(Builder.Compile());
            }

            return new(
                syntax.Name.Text,
                Profile,
                Metadata,
                [.. Capabilities.Values.OrderBy(Capability => Capability.Name, StringComparer.Ordinal)],
                [.. Data.Values.OrderBy(Item => Item.Name, StringComparer.Ordinal)],
                [
                    .. Records.Values.OrderBy(Record => Record.Index).Select(Record =>
                        (Nominalˉtypeˉdeclaration)Record.Declaration),
                    .. Enums.Values.OrderBy(Enum => Enum.Index).Select(Enum =>
                        (Nominalˉtypeˉdeclaration)Enum.Declaration),
                    .. Variants.Values.OrderBy(Variant => Variant.Index).Select(Variant =>
                        (Nominalˉtypeˉdeclaration)Variant.Declaration),
                ],
                Wirˉfunctions.ToImmutable());
        }

        private void Bindˉnominalˉtypes()
        {
            var Seenˉnames = new HashSet<string>(StringComparer.Ordinal);
            var Validˉrecords = new List<Recordˉsyntax>();
            foreach (var Record in syntax.Records)
            {
                if (!Seedˉnames.Isˉidentifier(Record.Name.Text))
                {
                    Report("WVC2080", Record.Name.Span, $"Record name '{Record.Name.Text}' is not a valid Windvale identifier.");
                    continue;
                }

                if (Record.Name.Text is "length" or Foundationˉintrinsics.ENUM_NAME ||
                    Foundationˉintrinsics.Tryˉget(Record.Name.Text, out _))
                {
                    Report("WVC2090", Record.Name.Span, $"Record name '{Record.Name.Text}' is reserved by Windvale Seed.");
                    continue;
                }

                if (!Seenˉnames.Add(Record.Name.Text))
                {
                    Report("WVC2081", Record.Name.Span, $"Record '{Record.Name.Text}' is declared more than once.");
                    continue;
                }

                Validˉrecords.Add(Record);
            }

            var Validˉenums = new List<Enumˉsyntax>();
            foreach (var Enum in syntax.Enums)
            {
                if (!Seedˉnames.Isˉidentifier(Enum.Name.Text))
                {
                    Report("WVC2091", Enum.Name.Span, $"Enum name '{Enum.Name.Text}' is not a valid Windvale identifier.");
                    continue;
                }

                if (!Seenˉnames.Add(Enum.Name.Text))
                {
                    Report("WVC2092", Enum.Name.Span, $"Nominal type '{Enum.Name.Text}' is declared more than once.");
                    continue;
                }

                Validˉenums.Add(Enum);
            }

            var Validˉvariants = new List<Variantˉsyntax>();
            foreach (var Variant in syntax.Variants)
            {
                if (!Seedˉnames.Isˉidentifier(Variant.Name.Text))
                {
                    Report("WVC2130", Variant.Name.Span, $"Variant name '{Variant.Name.Text}' is not a valid Windvale identifier.");
                    continue;
                }
                if (!Seenˉnames.Add(Variant.Name.Text))
                {
                    Report("WVC2131", Variant.Name.Span, $"Nominal type '{Variant.Name.Text}' is declared more than once.");
                    continue;
                }
                Validˉvariants.Add(Variant);
            }

            var Orderedˉrecords = Validˉrecords
                .OrderBy(Item => Item.Name.Text, StringComparer.Ordinal)
                .ToArray();
            var Orderedˉenums = Validˉenums
                .OrderBy(Item => Item.Name.Text, StringComparer.Ordinal)
                .ToArray();
            var Orderedˉvariants = Validˉvariants
                .OrderBy(Item => Item.Name.Text, StringComparer.Ordinal)
                .ToArray();
            if (Orderedˉrecords.Length + Orderedˉenums.Length + Orderedˉvariants.Length > Bytecodeˉlimits.MAX_NOMINAL_TYPES)
            {
                Report("WVC2088", syntax.Name.Span, "The module exceeds the Seed nominal-type limit.");
            }

            foreach (var Record in Orderedˉrecords.Take(Bytecodeˉlimits.MAX_NOMINAL_TYPES))
            {
                var Index = Records.Count;
                Records.Add(
                    Record.Name.Text,
                    new(Record.Name.Text, Index, new(Record.Name.Text, [])));
            }

            var Remainingˉtypeˉslots = Bytecodeˉlimits.MAX_NOMINAL_TYPES - Records.Count;
            foreach (var Enum in Orderedˉenums.Take(Remainingˉtypeˉslots))
            {
                var Memberˉnames = new HashSet<string>(StringComparer.Ordinal);
                var Memberˉvalues = new HashSet<int>();
                var Members = ImmutableArray.CreateBuilder<Enumˉmemberˉdeclaration>(Enum.Members.Length);
                foreach (var Member in Enum.Members)
                {
                    if (Members.Count >= Bytecodeˉlimits.MAX_ENUM_MEMBERS)
                    {
                        Report("WVC2096", Member.Span, $"Enum '{Enum.Name.Text}' exceeds the Seed member limit.");
                        break;
                    }

                    var Value = Member.Value.Value is int Integer ? Integer : 0;
                    if (Member.Value.Value is not int)
                    {
                        Report("WVC2099", Member.Value.Span, "Seed enum values must be unsuffixed nonnegative i32 literals.");
                    }
                    if (!Seedˉnames.Isˉidentifier(Member.Name.Text) || !Memberˉnames.Add(Member.Name.Text))
                    {
                        Report("WVC2093", Member.Name.Span, $"Enum '{Enum.Name.Text}' has an invalid or duplicate member '{Member.Name.Text}'.");
                    }

                    if (!Memberˉvalues.Add(Value))
                    {
                        Report("WVC2094", Member.Value.Span, $"Enum '{Enum.Name.Text}' repeats value {Value}.");
                    }

                    Members.Add(new(Member.Name.Text, Value));
                }

                if (Members.Count == 0)
                {
                    Report("WVC2095", Enum.Span, $"Enum '{Enum.Name.Text}' must declare at least one member.");
                    Members.Add(new("Invalid", 0));
                }

                var Index = Records.Count + Enums.Count;
                var Declaration = new Enumˉtypeˉdeclaration(Enum.Name.Text, Members.ToImmutable());
                Enums.Add(Enum.Name.Text, new(Enum.Name.Text, Index, Declaration));
            }

            var Remainingˉvariantˉslots = Bytecodeˉlimits.MAX_NOMINAL_TYPES - Records.Count - Enums.Count;
            var Variantˉnames = Orderedˉvariants.Select(Item => Item.Name.Text).ToHashSet(StringComparer.Ordinal);
            foreach (var Variant in Orderedˉvariants.Take(Remainingˉvariantˉslots))
            {
                var Caseˉnames = new HashSet<string>(StringComparer.Ordinal);
                var Cases = ImmutableArray.CreateBuilder<Variantˉcaseˉdeclaration>(Variant.Cases.Length);
                foreach (var Case in Variant.Cases)
                {
                    if (Cases.Count >= Bytecodeˉlimits.MAX_VARIANT_CASES)
                    {
                        Report("WVC2132", Case.Span, $"Variant '{Variant.Name.Text}' exceeds the case limit.");
                        break;
                    }
                    if (!Seedˉnames.Isˉidentifier(Case.Name.Text) || !Caseˉnames.Add(Case.Name.Text))
                    {
                        Report("WVC2133", Case.Name.Span, $"Variant '{Variant.Name.Text}' has an invalid or duplicate case '{Case.Name.Text}'.");
                    }

                    string? Payloadˉname = null;
                    Valueˉshape? Payloadˉtype = null;
                    if (Case.Payloadˉtype is not null)
                    {
                        Payloadˉname = Case.Payloadˉname?.Text;
                        if (Payloadˉname is null || !Seedˉnames.Isˉidentifier(Payloadˉname))
                        {
                            Report("WVC2134", Case.Span, $"Variant case '{Variant.Name.Text}.{Case.Name.Text}' has an invalid payload name.");
                            Payloadˉname = "Invalid";
                        }
                        if (Case.Payloadˉtype.Kind == Typeˉsyntaxˉkind.Named &&
                            Case.Payloadˉtype.Name is not null &&
                            Variantˉnames.Contains(Case.Payloadˉtype.Name))
                        {
                            Report("WVC2135", Case.Payloadˉtype.Span, "WVB 1.9 variant payloads cannot contain variants.");
                            Payloadˉtype = Valueˉtype.I32;
                        }
                        else
                        {
                            Payloadˉtype = Bindˉvalueˉshape(Case.Payloadˉtype);
                            if (Payloadˉtype.Value.Kind == Valueˉtype.Builder)
                            {
                                Report("WVC2151", Case.Payloadˉtype.Span, "A variant payload cannot contain an affine builder.");
                                Payloadˉtype = Valueˉtype.I32;
                            }
                        }
                    }
                    Cases.Add(new(Case.Name.Text, Payloadˉname, Payloadˉtype));
                }
                if (Cases.Count == 0)
                {
                    Report("WVC2136", Variant.Span, $"Variant '{Variant.Name.Text}' must declare at least one case.");
                    Cases.Add(new("Invalid", null, null));
                }
                var Index = Records.Count + Enums.Count + Variants.Count;
                var Declaration = new Variantˉtypeˉdeclaration(Variant.Name.Text, Cases.ToImmutable());
                Variants.Add(Variant.Name.Text, new(Variant.Name.Text, Index, Declaration));
            }

            foreach (var Record in Orderedˉrecords.Take(Records.Count))
            {
                var Fieldˉnames = new HashSet<string>(StringComparer.Ordinal);
                var Fields = ImmutableArray.CreateBuilder<Recordˉfieldˉdeclaration>(Record.Fields.Length);
                foreach (var Field in Record.Fields)
                {
                    if (Fields.Count >= Bytecodeˉlimits.MAX_RECORD_FIELDS)
                    {
                        Report("WVC2089", Field.Span, $"Record '{Record.Name.Text}' exceeds the Seed field limit.");
                        break;
                    }

                    if (!Seedˉnames.Isˉidentifier(Field.Name.Text) || !Fieldˉnames.Add(Field.Name.Text))
                    {
                        Report("WVC2082", Field.Name.Span, $"Record '{Record.Name.Text}' has an invalid or duplicate field '{Field.Name.Text}'.");
                    }

                    if (Field.Type.Kind is Typeˉsyntaxˉkind.Void or Typeˉsyntaxˉkind.Invalid)
                    {
                        Report("WVC2083", Field.Type.Span, "Seed record fields must use a primitive or enum value type.");
                        Fields.Add(new(Field.Name.Text, Valueˉtype.I32));
                        continue;
                    }

                    if (Field.Type.Kind == Typeˉsyntaxˉkind.Named)
                    {
                        if (Field.Type.Name is not null && Enums.TryGetValue(Field.Type.Name, out var Enum))
                        {
                            Fields.Add(new(Field.Name.Text, Valueˉshape.Forˉenum(Enum.Index)));
                        }
                        else
                        {
                            Report("WVC2083", Field.Type.Span, "Seed record fields cannot contain records or unknown named types.");
                            Fields.Add(new(Field.Name.Text, Valueˉtype.I32));
                        }

                        continue;
                    }

                    if (Field.Type.Kind is Typeˉsyntaxˉkind.Sequence or Typeˉsyntaxˉkind.Builder)
                    {
                        var Fieldˉtype = Bindˉvalueˉshape(Field.Type);
                        if (Fieldˉtype.Kind == Valueˉtype.Builder)
                        {
                            Report("WVC2151", Field.Type.Span, "A record field cannot contain an affine builder.");
                            Fieldˉtype = Valueˉtype.I32;
                        }
                        Fields.Add(new(Field.Name.Text, Fieldˉtype));
                        continue;
                    }

                    Fields.Add(new(Field.Name.Text, Bindˉprimitiveˉtype(Field.Type)));
                }

                if (Fields.Count == 0)
                {
                    Report("WVC2084", Record.Span, $"Record '{Record.Name.Text}' must declare at least one field.");
                    Fields.Add(new("Invalid", Valueˉtype.I32));
                }

                var Declaration = new Recordˉtypeˉdeclaration(Record.Name.Text, Fields.ToImmutable());
                var Existing = Records[Record.Name.Text];
                Records[Record.Name.Text] = Existing with { Declaration = Declaration };
            }
        }

        private void Bindˉmoduleˉname()
        {
            if (!Seedˉnames.Isˉidentifier(syntax.Name.Text))
            {
                Report(
                    "WVC2004",
                    syntax.Name.Span,
                    $"Module name '{syntax.Name.Text}' is not a valid Windvale identifier.");
            }
        }

        private Moduleˉprofile Bindˉprofile(Syntaxˉtoken profile)
        {
            return profile.Kind switch
            {
                Tokenˉkind.Portable => Moduleˉprofile.Portable,
                Tokenˉkind.Hosted => Moduleˉprofile.Hosted,
                Tokenˉkind.System => Moduleˉprofile.System,
                _ => Moduleˉprofile.Portable,
            };
        }

        private void Bindˉcapabilities(Moduleˉprofile profile)
        {
            foreach (var Capabilityˉsyntax in syntax.Capabilities)
            {
                if (Capabilities.ContainsKey(Capabilityˉsyntax.Name))
                {
                    Report(
                        "WVC2000",
                        Capabilityˉsyntax.Span,
                        $"Capability '{Capabilityˉsyntax.Name}' is declared more than once.");
                    continue;
                }

                if (!Seedˉnames.Isˉcapability(Capabilityˉsyntax.Name))
                {
                    Report(
                        "WVC2001",
                        Capabilityˉsyntax.Span,
                        $"Capability name '{Capabilityˉsyntax.Name}' is invalid.");
                    continue;
                }

                if (!Capabilityˉcatalog.Tryˉget(Capabilityˉsyntax.Name, out var Declaration))
                {
                    Report(
                        "WVC2002",
                        Capabilityˉsyntax.Span,
                        $"Capability '{Capabilityˉsyntax.Name}' is not defined by Windvale Seed.");
                    continue;
                }

                if (profile == Moduleˉprofile.Portable)
                {
                    Report(
                        "WVC2003",
                        Capabilityˉsyntax.Span,
                        "A portable module cannot declare hosted capabilities.");
                }

                Capabilities.Add(Capabilityˉsyntax.Name, Declaration);
            }
        }

        private Moduleˉmetadata? Bindˉmetadata(Moduleˉprofile profile)
        {
            if (syntax.Metadata is not { } Metadata)
            {
                return null;
            }

            var Authority = Metadata.Authority.Text switch
            {
                "library" => Moduleˉauthority.Library,
                "application" => Moduleˉauthority.Application,
                "service" => Moduleˉauthority.Service,
                "system" => Moduleˉauthority.System,
                _ => (Moduleˉauthority)0,
            };
            if (!Enum.IsDefined(Authority))
            {
                Report(
                    "WVC2113",
                    Metadata.Authority.Span,
                    $"Module authority '{Metadata.Authority.Text}' is not library, application, service, or system.");
            }
            if ((Authority == Moduleˉauthority.System) != (profile == Moduleˉprofile.System))
            {
                Report(
                    "WVC2114",
                    Metadata.Authority.Span,
                    "System authority and the derived system execution profile must agree.");
            }

            var Platformˉnames = new HashSet<string>(StringComparer.Ordinal);
            foreach (var Scope in Metadata.Platformˉscopes)
            {
                if (!Seedˉnames.Isˉplatformˉscope(Scope.Name))
                {
                    Report("WVC2115", Scope.Span, $"Platform scope '{Scope.Name}' is invalid.");
                }
                if (!Platformˉnames.Add(Scope.Name))
                {
                    Report("WVC2116", Scope.Span, $"Platform scope '{Scope.Name}' is declared more than once.");
                }
            }
            if (Metadata.Platformˉscopes.Length == 0)
            {
                Report("WVC2115", Metadata.Authority.Span, "A metadata-bearing module needs at least one platform scope.");
            }
            if (Metadata.Platformˉscopes.Length > Bytecodeˉlimits.MAX_PLATFORM_SCOPES)
            {
                Report("WVC2117", Metadata.Authority.Span, "The module declares too many platform scopes.");
            }

            var Required = Bindˉrequirements(Metadata.Requiredˉcapabilities, "required");
            var Optional = Bindˉrequirements(Metadata.Optionalˉcapabilities, "optional");
            var Requiredˉnames = Required.Select(Requirement => Requirement.Name).ToHashSet(StringComparer.Ordinal);
            foreach (var Requirement in Optional)
            {
                if (Requiredˉnames.Contains(Requirement.Name))
                {
                    var Source = Metadata.Optionalˉcapabilities.First(Item => Item.Name == Requirement.Name);
                    Report(
                        "WVC2118",
                        Source.Span,
                        $"Capability '{Requirement.Name}' cannot be both required and optional.");
                }
            }

            return new(
                Authority,
                [.. Platformˉnames.OrderBy(Name => Name, StringComparer.Ordinal)],
                [.. Required.OrderBy(Requirement => Requirement.Name, StringComparer.Ordinal)],
                [.. Optional.OrderBy(Requirement => Requirement.Name, StringComparer.Ordinal)]);
        }

        private ImmutableArray<Capabilityˉrequirement> Bindˉrequirements(
            ImmutableArray<Capabilityˉrequirementˉsyntax> requirements,
            string kind)
        {
            if (requirements.Length > Bytecodeˉlimits.MAX_CAPABILITY_REQUIREMENTS)
            {
                Report("WVC2119", syntax.Name.Span, $"The module declares too many {kind} capability requirements.");
            }

            var Result = ImmutableArray.CreateBuilder<Capabilityˉrequirement>();
            var Names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var Requirement in requirements)
            {
                if (!Names.Add(Requirement.Name))
                {
                    Report(
                        "WVC2120",
                        Requirement.Span,
                        $"{char.ToUpperInvariant(kind[0])}{kind[1..]} capability '{Requirement.Name}' is declared more than once.");
                    continue;
                }
                if (!Seedˉnames.Isˉcapability(Requirement.Name))
                {
                    Report("WVC2121", Requirement.Span, $"Capability name '{Requirement.Name}' is invalid.");
                    continue;
                }
                if (!Capabilityˉcatalog.Tryˉget(Requirement.Name, out _))
                {
                    Report(
                        "WVC2122",
                        Requirement.Span,
                        $"{char.ToUpperInvariant(kind[0])}{kind[1..]} capability '{Requirement.Name}' is not defined by Windvale Seed.");
                    continue;
                }
                if (Requirement.Majorˉversion != 1)
                {
                    Report(
                        "WVC2123",
                        Requirement.Span,
                        $"Capability '{Requirement.Name}' major version {Requirement.Majorˉversion} is not supported.");
                    continue;
                }
                Result.Add(new(Requirement.Name, Requirement.Majorˉversion));
            }
            return Result.ToImmutable();
        }

        private void Bindˉdata()
        {
            foreach (var Dataˉsyntax in syntax.Data)
            {
                if (!Seedˉnames.Isˉidentifier(Dataˉsyntax.Name.Text))
                {
                    Report(
                        "WVC2012",
                        Dataˉsyntax.Name.Span,
                        $"Data name '{Dataˉsyntax.Name.Text}' is not a valid Windvale identifier.");
                    continue;
                }

                if (Data.ContainsKey(Dataˉsyntax.Name.Text))
                {
                    Report(
                        "WVC2010",
                        Dataˉsyntax.Name.Span,
                        $"Data '{Dataˉsyntax.Name.Text}' is declared more than once.");
                    continue;
                }

                Dataˉdeclaration? Declaration = (Dataˉsyntax.Type.Kind, Dataˉsyntax.Value) switch
                {
                    (Typeˉsyntaxˉkind.Text, Textˉdataˉvalueˉsyntax Textˉvalue) =>
                        new Textˉdataˉdeclaration(Dataˉsyntax.Name.Text, Textˉvalue.Value),
                    (Typeˉsyntaxˉkind.I32ˉarray, I32ˉarrayˉdataˉvalueˉsyntax Array) =>
                        new I32ˉarrayˉdataˉdeclaration(Dataˉsyntax.Name.Text, Array.Values),
                    (Typeˉsyntaxˉkind.Bytes, Bytesˉdataˉvalueˉsyntax Bytes) =>
                        new Bytesˉdataˉdeclaration(Dataˉsyntax.Name.Text, Bytes.Values),
                    _ => null,
                };

                if (Declaration is null)
                {
                    Report(
                        "WVC2011",
                        Dataˉsyntax.Span,
                        $"Data '{Dataˉsyntax.Name.Text}' has an incompatible initializer.");
                    continue;
                }

                Data.Add(Declaration.Name, Declaration);
                if (Declaration is Textˉdataˉdeclaration Textˉdeclaration &&
                    !Textˉdataˉbyˉvalue.ContainsKey(Textˉdeclaration.Value))
                {
                    Textˉdataˉbyˉvalue.Add(Textˉdeclaration.Value, Textˉdeclaration.Name);
                }
            }
        }

        private void Bindˉconstants()
        {
            var Allˉnames = syntax.Constants
                .Select(Constant => Constant.Name.Text)
                .ToHashSet(StringComparer.Ordinal);
            foreach (var Constant in syntax.Constants)
            {
                if (!Isˉconstantˉname(Constant.Name.Text))
                {
                    Report(
                        "WVC2100",
                        Constant.Name.Span,
                        $"Constant name '{Constant.Name.Text}' must use ALL_CAPS_WITH_UNDERSCORES.");
                    continue;
                }

                if (Constants.ContainsKey(Constant.Name.Text) || Data.ContainsKey(Constant.Name.Text))
                {
                    Report(
                        "WVC2100",
                        Constant.Name.Span,
                        $"Constant '{Constant.Name.Text}' conflicts with an earlier value declaration.");
                    continue;
                }

                var Type = Bindˉvalueˉshape(Constant.Type);
                if (Type.Kind is not (
                    Valueˉtype.I32 or Valueˉtype.I64 or Valueˉtype.U8 or
                    Valueˉtype.U32 or Valueˉtype.U64 or Valueˉtype.Bool or
                    Valueˉtype.Enum))
                {
                    Report(
                        "WVC2101",
                        Constant.Type.Span,
                        "A constant must have an explicit integer, bool, or enum type.");
                    continue;
                }

                var Value = Evaluateˉconstant(Constant.Initializer, Allˉnames);
                if (Value is null)
                {
                    continue;
                }
                if (Value.Type != Type)
                {
                    Report(
                        "WVC2102",
                        Constant.Initializer.Span,
                        $"Constant '{Constant.Name.Text}' has type {Formatˉtype(Value.Type)}; " +
                        $"{Formatˉtype(Type)} is required.");
                    continue;
                }

                Constants.Add(Constant.Name.Text, Value);
            }
        }

        private Constantˉvalue? Evaluateˉconstant(
            Expressionˉsyntax expression,
            IReadOnlySet<string> allˉnames)
        {
            switch (expression)
            {
                case Literalˉexpressionˉsyntax { Value: int Value }:
                    return new(Valueˉtype.I32, Value);
                case Literalˉexpressionˉsyntax { Value: long Value }:
                    return new(Valueˉtype.I64, Value);
                case Literalˉexpressionˉsyntax { Value: byte Value }:
                    return new(Valueˉtype.U8, Value);
                case Literalˉexpressionˉsyntax { Value: uint Value }:
                    return new(Valueˉtype.U32, Value);
                case Literalˉexpressionˉsyntax { Value: ulong Value }:
                    return new(Valueˉtype.U64, Value);
                case Literalˉexpressionˉsyntax { Value: bool Value }:
                    return new(Valueˉtype.Bool, Value);
                case Nameˉexpressionˉsyntax Name:
                    if (Constants.TryGetValue(Name.Name, out var Earlier))
                    {
                        return Earlier;
                    }
                    Report(
                        allˉnames.Contains(Name.Name) ? "WVC2103" : "WVC2104",
                        Name.Span,
                        allˉnames.Contains(Name.Name)
                            ? $"Constant '{Name.Name}' is a forward or cyclic reference."
                            : $"'{Name.Name}' is not an earlier constant.");
                    return null;
                case Fieldˉexpressionˉsyntax Field:
                    return Evaluateˉenumˉconstant(Field);
                case Unaryˉexpressionˉsyntax Unary:
                    return Evaluateˉconstantˉunary(Unary, allˉnames);
                case Binaryˉexpressionˉsyntax Binary:
                    return Evaluateˉconstantˉbinary(Binary, allˉnames);
                default:
                    Report(
                        "WVC2104",
                        expression.Span,
                        "A constant initializer may use only literals, enum members, earlier constants, and checked operators.");
                    return null;
            }
        }

        private Constantˉvalue? Evaluateˉenumˉconstant(Fieldˉexpressionˉsyntax expression)
        {
            if (!Enums.TryGetValue(expression.Target, out var Enum))
            {
                Report("WVC2104", expression.Span, "A constant member expression must name an enum member.");
                return null;
            }
            for (var Index = 0; Index < Enum.Declaration.Members.Length; Index++)
            {
                if (StringComparer.Ordinal.Equals(Enum.Declaration.Members[Index].Name, expression.Field))
                {
                    return new(
                        Valueˉshape.Forˉenum(Enum.Index),
                        new Enumˉconstantˉvalue(Enum.Index, Index));
                }
            }
            Report("WVC2097", expression.Span, $"Enum '{Enum.Name}' has no member '{expression.Field}'.");
            return null;
        }

        private Constantˉvalue? Evaluateˉconstantˉunary(
            Unaryˉexpressionˉsyntax expression,
            IReadOnlySet<string> allˉnames)
        {
            var Operand = Evaluateˉconstant(expression.Operand, allˉnames);
            if (Operand is null)
            {
                return null;
            }
            try
            {
                if (expression.Operator == Tokenˉkind.Minus)
                {
                    return Operand.Value switch
                    {
                        int Value => new(Operand.Type, checked(-Value)),
                        long Value => new(Operand.Type, checked(-Value)),
                        _ => Invalidˉconstantˉoperator(expression.Span),
                    };
                }
                if (expression.Operator == Tokenˉkind.Bang && Operand.Value is bool Boolean)
                {
                    return new(Valueˉtype.Bool, !Boolean);
                }
                if (expression.Operator == Tokenˉkind.Tilde)
                {
                    return Operand.Value switch
                    {
                        byte Value => new(Operand.Type, (byte)~Value),
                        uint Value => new(Operand.Type, ~Value),
                        ulong Value => new(Operand.Type, ~Value),
                        _ => Invalidˉconstantˉoperator(expression.Span),
                    };
                }
            }
            catch (OverflowException)
            {
                Report("WVC2106", expression.Span, "Constant evaluation would trap on checked overflow.");
                return null;
            }
            return Invalidˉconstantˉoperator(expression.Span);
        }

        private Constantˉvalue? Evaluateˉconstantˉbinary(
            Binaryˉexpressionˉsyntax expression,
            IReadOnlySet<string> allˉnames)
        {
            var Left = Evaluateˉconstant(expression.Left, allˉnames);
            if (Left is null)
            {
                return null;
            }

            if (expression.Operator is Tokenˉkind.Andˉand or Tokenˉkind.Orˉor)
            {
                if (Left.Value is not bool Leftˉboolean)
                {
                    return Invalidˉconstantˉoperator(expression.Span);
                }
                if ((expression.Operator == Tokenˉkind.Andˉand && !Leftˉboolean) ||
                    (expression.Operator == Tokenˉkind.Orˉor && Leftˉboolean))
                {
                    return new(Valueˉtype.Bool, Leftˉboolean);
                }

                var Shortˉright = Evaluateˉconstant(expression.Right, allˉnames);
                if (Shortˉright is null)
                {
                    return null;
                }
                return Shortˉright.Type == Valueˉtype.Bool && Shortˉright.Value is bool Rightˉboolean
                    ? new(Valueˉtype.Bool, Rightˉboolean)
                    : Invalidˉconstantˉoperator(expression.Span);
            }

            var Right = Evaluateˉconstant(expression.Right, allˉnames);
            if (Right is null)
            {
                return null;
            }
            if (expression.Operator is Tokenˉkind.Shiftˉleft or Tokenˉkind.Shiftˉright)
            {
                if (Right.Type != Valueˉtype.U32 || Right.Value is not uint Count ||
                    Left.Type.Kind is not (Valueˉtype.U8 or Valueˉtype.U32 or Valueˉtype.U64))
                {
                    return Invalidˉconstantˉoperator(expression.Span);
                }
                var Width = Left.Type.Kind == Valueˉtype.U8 ? 8u : Left.Type.Kind == Valueˉtype.U32 ? 32u : 64u;
                if (Count >= Width)
                {
                    Report("WVC2106", expression.Span, "Constant evaluation would trap on an out-of-range shift count.");
                    return null;
                }
                object Shifted = (Left.Value, expression.Operator) switch
                {
                    (byte L, Tokenˉkind.Shiftˉleft) => (byte)(L << (int)Count),
                    (byte L, _) => (byte)(L >> (int)Count),
                    (uint L, Tokenˉkind.Shiftˉleft) => L << (int)Count,
                    (uint L, _) => L >> (int)Count,
                    (ulong L, Tokenˉkind.Shiftˉleft) => L << (int)Count,
                    (ulong L, _) => L >> (int)Count,
                    _ => throw new InvalidOperationException(),
                };
                return new(Left.Type, Shifted);
            }
            if (Left.Type != Right.Type)
            {
                return Invalidˉconstantˉoperator(expression.Span);
            }

            try
            {
                if (expression.Operator is Tokenˉkind.Plus or Tokenˉkind.Minus or Tokenˉkind.Star)
                {
                    object Value = (Left.Value, expression.Operator) switch
                    {
                        (int L, Tokenˉkind.Plus) => checked(L + (int)Right.Value),
                        (int L, Tokenˉkind.Minus) => checked(L - (int)Right.Value),
                        (int L, Tokenˉkind.Star) => checked(L * (int)Right.Value),
                        (long L, Tokenˉkind.Plus) => checked(L + (long)Right.Value),
                        (long L, Tokenˉkind.Minus) => checked(L - (long)Right.Value),
                        (long L, Tokenˉkind.Star) => checked(L * (long)Right.Value),
                        (uint L, Tokenˉkind.Plus) => checked(L + (uint)Right.Value),
                        (uint L, Tokenˉkind.Minus) => checked(L - (uint)Right.Value),
                        (uint L, Tokenˉkind.Star) => checked(L * (uint)Right.Value),
                        (ulong L, Tokenˉkind.Plus) => checked(L + (ulong)Right.Value),
                        (ulong L, Tokenˉkind.Minus) => checked(L - (ulong)Right.Value),
                        (ulong L, Tokenˉkind.Star) => checked(L * (ulong)Right.Value),
                        _ => throw new InvalidOperationException(),
                    };
                    return new(Left.Type, Value);
                }

                if (expression.Operator is Tokenˉkind.Slash or Tokenˉkind.Percent)
                {
                    object Value = (Left.Value, expression.Operator) switch
                    {
                        (int L, Tokenˉkind.Slash) => checked(L / (int)Right.Value),
                        (int L, _) => checked(L % (int)Right.Value),
                        (long L, Tokenˉkind.Slash) => checked(L / (long)Right.Value),
                        (long L, _) => checked(L % (long)Right.Value),
                        (uint L, Tokenˉkind.Slash) => L / (uint)Right.Value,
                        (uint L, _) => L % (uint)Right.Value,
                        (ulong L, Tokenˉkind.Slash) => L / (ulong)Right.Value,
                        (ulong L, _) => L % (ulong)Right.Value,
                        _ => throw new InvalidOperationException(),
                    };
                    return new(Left.Type, Value);
                }

                if (expression.Operator is Tokenˉkind.Ampersand or Tokenˉkind.Pipe or Tokenˉkind.Caret)
                {
                    object Value = (Left.Value, expression.Operator) switch
                    {
                        (byte L, Tokenˉkind.Ampersand) => (byte)(L & (byte)Right.Value),
                        (byte L, Tokenˉkind.Pipe) => (byte)(L | (byte)Right.Value),
                        (byte L, _) => (byte)(L ^ (byte)Right.Value),
                        (uint L, Tokenˉkind.Ampersand) => L & (uint)Right.Value,
                        (uint L, Tokenˉkind.Pipe) => L | (uint)Right.Value,
                        (uint L, _) => L ^ (uint)Right.Value,
                        (ulong L, Tokenˉkind.Ampersand) => L & (ulong)Right.Value,
                        (ulong L, Tokenˉkind.Pipe) => L | (ulong)Right.Value,
                        (ulong L, _) => L ^ (ulong)Right.Value,
                        _ => throw new InvalidOperationException(),
                    };
                    return new(Left.Type, Value);
                }

                if (expression.Operator is Tokenˉkind.Equalsˉequals or Tokenˉkind.Bangˉequals)
                {
                    var Equal = Equals(Left.Value, Right.Value);
                    return new(
                        Valueˉtype.Bool,
                        expression.Operator == Tokenˉkind.Equalsˉequals ? Equal : !Equal);
                }

                if (expression.Operator is
                    Tokenˉkind.Less or Tokenˉkind.Lessˉequals or
                    Tokenˉkind.Greater or Tokenˉkind.Greaterˉequals)
                {
                    var Comparison = Left.Value switch
                    {
                        int L => L.CompareTo((int)Right.Value),
                        long L => L.CompareTo((long)Right.Value),
                        uint L => L.CompareTo((uint)Right.Value),
                        ulong L => L.CompareTo((ulong)Right.Value),
                        _ => int.MinValue,
                    };
                    if (Comparison == int.MinValue)
                    {
                        return Invalidˉconstantˉoperator(expression.Span);
                    }
                    return new(
                        Valueˉtype.Bool,
                        expression.Operator switch
                        {
                            Tokenˉkind.Less => Comparison < 0,
                            Tokenˉkind.Lessˉequals => Comparison <= 0,
                            Tokenˉkind.Greater => Comparison > 0,
                            _ => Comparison >= 0,
                        });
                }
            }
            catch (OverflowException)
            {
                Report("WVC2106", expression.Span, "Constant evaluation would trap on checked overflow.");
                return null;
            }
            catch (DivideByZeroException)
            {
                Report("WVC2106", expression.Span, "Constant evaluation would trap on division by zero.");
                return null;
            }
            catch (InvalidOperationException)
            {
                return Invalidˉconstantˉoperator(expression.Span);
            }
            return Invalidˉconstantˉoperator(expression.Span);
        }

        private Constantˉvalue? Invalidˉconstantˉoperator(Sourceˉspan span)
        {
            Report("WVC2105", span, "The operator is not defined for these exact constant operand types.");
            return null;
        }

        private string Formatˉtype(Valueˉshape type)
        {
            if (type.Kind == Valueˉtype.Record &&
                Records.Values.FirstOrDefault(Record => Record.Index == type.Nominalˉtypeˉindex) is { } Record)
            {
                return Record.Name;
            }

            if (type.Kind == Valueˉtype.Enum &&
                Enums.Values.FirstOrDefault(Enum => Enum.Index == type.Nominalˉtypeˉindex) is { } Enum)
            {
                return Enum.Name;
            }

            if (type.Kind == Valueˉtype.Variant &&
                Variants.Values.FirstOrDefault(Variant => Variant.Index == type.Nominalˉtypeˉindex) is { } Variant)
            {
                return Variant.Name;
            }

            return type.Kind switch
            {
                Valueˉtype.Void => "void",
                Valueˉtype.I32 => "i32",
                Valueˉtype.I64 => "i64",
                Valueˉtype.U8 => "u8",
                Valueˉtype.U32 => "u32",
                Valueˉtype.U64 => "u64",
                Valueˉtype.Bool => "bool",
                _ => type.ToString(),
            };
        }

        private static bool Isˉconstantˉname(string name)
        {
            if (name.Length == 0 || !(name[0] is >= 'A' and <= 'Z' or '_'))
            {
                return false;
            }
            return name.All(Character =>
                Character is >= 'A' and <= 'Z' or >= '0' and <= '9' or '_');
        }

        private void Bindˉfunctionˉsignatures()
        {
            foreach (var Functionˉsyntax in syntax.Functions)
            {
                if (Functionˉsyntax.Name.Text is "length" or Foundationˉintrinsics.ENUM_NAME ||
                    Foundationˉintrinsics.Tryˉget(Functionˉsyntax.Name.Text, out _))
                {
                    Report(
                        "WVC2024",
                        Functionˉsyntax.Name.Span,
                        $"Function name '{Functionˉsyntax.Name.Text}' is reserved by Windvale Seed.");
                    continue;
                }

                if (!Seedˉnames.Isˉidentifier(Functionˉsyntax.Name.Text))
                {
                    Report(
                        "WVC2022",
                        Functionˉsyntax.Name.Span,
                        $"Function name '{Functionˉsyntax.Name.Text}' is not a valid Windvale identifier.");
                    continue;
                }

                if (Functions.ContainsKey(Functionˉsyntax.Name.Text))
                {
                    Report(
                        "WVC2020",
                        Functionˉsyntax.Name.Span,
                        $"Function '{Functionˉsyntax.Name.Text}' is declared more than once.");
                    continue;
                }

                if (Records.ContainsKey(Functionˉsyntax.Name.Text))
                {
                    Report(
                        "WVC2025",
                        Functionˉsyntax.Name.Span,
                        $"Function name '{Functionˉsyntax.Name.Text}' conflicts with a record constructor.");
                    continue;
                }

                var Parameterˉnames = new HashSet<string>(StringComparer.Ordinal);
                var Parameters = ImmutableArray.CreateBuilder<Parameterˉsymbol>(Functionˉsyntax.Parameters.Length);
                for (var Index = 0; Index < Functionˉsyntax.Parameters.Length; Index++)
                {
                    var Parameter = Functionˉsyntax.Parameters[Index];
                    if (!Seedˉnames.Isˉidentifier(Parameter.Name.Text))
                    {
                        Report(
                            "WVC2023",
                            Parameter.Name.Span,
                            $"Parameter name '{Parameter.Name.Text}' is not a valid Windvale identifier.");
                    }

                    if (!Parameterˉnames.Add(Parameter.Name.Text))
                    {
                        Report(
                            "WVC2021",
                            Parameter.Name.Span,
                            $"Parameter '{Parameter.Name.Text}' is declared more than once.");
                    }

                    var Parameterˉtype = Bindˉvalueˉshape(Parameter.Type);
                    if (Parameterˉtype.Kind == Valueˉtype.Builder)
                    {
                        Report("WVC2151", Parameter.Type.Span, "A function parameter cannot have affine builder type.");
                        Parameterˉtype = Valueˉtype.I32;
                    }
                    Parameters.Add(new(
                        Parameter.Name.Text,
                        Parameterˉtype,
                        Index,
                        Parameter.Name.Span));
                }

                var Returnˉtype = Bindˉvalueˉshape(Functionˉsyntax.Returnˉtype);
                if (Returnˉtype.Kind == Valueˉtype.Builder)
                {
                    Report("WVC2151", Functionˉsyntax.Returnˉtype.Span, "A function cannot return an affine builder.");
                    Returnˉtype = Valueˉtype.I32;
                }

                Functions.Add(
                    Functionˉsyntax.Name.Text,
                    new(
                        Functionˉsyntax.Name.Text,
                        Parameters.ToImmutable(),
                        Returnˉtype,
                        Functionˉsyntax.Isˉexported,
                        Functionˉsyntax));
            }
        }

        private string Getˉorˉaddˉtextˉdata(string value)
        {
            if (Textˉdataˉbyˉvalue.TryGetValue(value, out var Existing))
            {
                return Existing;
            }

            string Name;
            do
            {
                Name = $"__Text_{Syntheticˉtextˉcounter++:D6}";
            }
            while (Data.ContainsKey(Name));

            Data.Add(Name, new Textˉdataˉdeclaration(Name, value));
            Textˉdataˉbyˉvalue.Add(value, Name);
            return Name;
        }

        private Valueˉshape Bindˉvalueˉshape(Typeˉsyntax type)
        {
            if (type.Kind is Typeˉsyntaxˉkind.Sequence or Typeˉsyntaxˉkind.Builder)
            {
                var Element = type.Elementˉtype is null
                    ? (Valueˉshape)Valueˉtype.I32
                    : Bindˉvalueˉshape(type.Elementˉtype);
                if (Element.Kind is Valueˉtype.Void or Valueˉtype.Sequence or Valueˉtype.Builder)
                {
                    Report("WVC2150", type.Span, "A bounded collection requires one non-collection value element type.");
                    Element = Valueˉtype.I32;
                }
                var Maximum = type.Maximum;
                if (Maximum is 0 or > Bytecodeˉlimits.MAX_SEQUENCE_ELEMENTS)
                {
                    Report(
                        "WVC2150",
                        type.Span,
                        $"A bounded collection maximum must be 1 through {Bytecodeˉlimits.MAX_SEQUENCE_ELEMENTS}.");
                    Maximum = 1;
                }
                return type.Kind == Typeˉsyntaxˉkind.Sequence
                    ? Valueˉshape.Forˉsequence(Element, Maximum)
                    : Valueˉshape.Forˉbuilder(Element, Maximum);
            }

            if (type.Kind == Typeˉsyntaxˉkind.Named)
            {
                if (type.Name is not null && Records.TryGetValue(type.Name, out var Record))
                {
                    return Valueˉshape.Forˉrecord(Record.Index);
                }

                if (type.Name is not null && Enums.TryGetValue(type.Name, out var Enum))
                {
                    return Valueˉshape.Forˉenum(Enum.Index);
                }

                if (type.Name is not null && Variants.TryGetValue(type.Name, out var Variant))
                {
                    return Valueˉshape.Forˉvariant(Variant.Index);
                }

                Report("WVC2085", type.Span, $"Named type '{type.Name}' is not declared.");
                return Valueˉtype.I32;
            }

            return Bindˉprimitiveˉtype(type);
        }

        private static Valueˉtype Bindˉprimitiveˉtype(Typeˉsyntax type)
        {
            return type.Kind switch
            {
                Typeˉsyntaxˉkind.Void => Valueˉtype.Void,
                Typeˉsyntaxˉkind.I32 => Valueˉtype.I32,
                Typeˉsyntaxˉkind.I64 => Valueˉtype.I64,
                Typeˉsyntaxˉkind.U8 => Valueˉtype.U8,
                Typeˉsyntaxˉkind.U32 => Valueˉtype.U32,
                Typeˉsyntaxˉkind.U64 => Valueˉtype.U64,
                Typeˉsyntaxˉkind.Bool => Valueˉtype.Bool,
                Typeˉsyntaxˉkind.Text => Valueˉtype.Text,
                Typeˉsyntaxˉkind.Bytes => Valueˉtype.Bytes,
                _ => Valueˉtype.I32,
            };
        }

        private void Report(string code, Sourceˉspan span, string message)
        {
            diagnostics.Report(code, "semantic", span, message);
        }
    }

    private sealed record Parameterˉsymbol(
        string Name,
        Valueˉshape Type,
        int Slot,
        Sourceˉspan Span);

    private sealed record Functionˉsymbol(
        string Name,
        ImmutableArray<Parameterˉsymbol> Parameters,
        Valueˉshape Returnˉtype,
        bool Isˉexported,
        Functionˉsyntax Syntax)
    {
        public ImmutableArray<Valueˉshape> Parameterˉtypes => [.. Parameters.Select(Parameter => Parameter.Type)];
    }

    private sealed record Recordˉsymbol(
        string Name,
        int Index,
        Recordˉtypeˉdeclaration Declaration);

    private sealed record Enumˉsymbol(
        string Name,
        int Index,
        Enumˉtypeˉdeclaration Declaration);

    private sealed record Variantˉsymbol(
        string Name,
        int Index,
        Variantˉtypeˉdeclaration Declaration);

    private sealed record Enumˉconstantˉvalue(int Typeˉindex, int Memberˉindex);

    private sealed record Constantˉvalue(Valueˉshape Type, object Value);

    private sealed record Localˉsymbol(string Name, Valueˉshape Type, int Slot, bool Isˉmutable);

    private readonly record struct Loopˉtargets(int Continueˉblock, int Breakˉblock);

    private readonly record struct Boundˉvalue(Valueˉshape Type, int Temporary)
    {
        public static Boundˉvalue Void => new(Valueˉtype.Void, -1);
    }

    private sealed class Mutableˉblock(int id)
    {
        public int Id { get; } = id;

        public List<Wirˉinstruction> Instructions { get; } = [];

        public Wirˉterminator? Terminator { get; set; }
    }

    private sealed class Functionˉbuilder
    {
        private readonly Functionˉsymbol Function;
        private readonly Diagnosticˉbag Diagnostics;
        private readonly IReadOnlyDictionary<string, Dataˉdeclaration> Data;
        private readonly IReadOnlyDictionary<string, Constantˉvalue> Constants;
        private readonly IReadOnlyDictionary<string, Functionˉsymbol> Functions;
        private readonly IReadOnlyDictionary<string, Capabilityˉdeclaration> Capabilities;
        private readonly IReadOnlyDictionary<string, Recordˉsymbol> Records;
        private readonly IReadOnlyDictionary<string, Enumˉsymbol> Enums;
        private readonly IReadOnlyDictionary<string, Variantˉsymbol> Variants;
        private readonly Func<string, string> Getˉtextˉdata;
        private readonly List<Mutableˉblock> Blocks = [];
        private readonly List<Valueˉshape> Userˉlocalˉtypes = [];
        private readonly List<Valueˉshape> Temporaryˉtypes = [];
        private readonly Stack<Dictionary<string, Localˉsymbol>> Scopes = [];
        private readonly Stack<Loopˉtargets> Loops = [];
        private readonly HashSet<string> Allˉlocalˉnames = new(StringComparer.Ordinal);
        private readonly HashSet<int> Consumedˉbuilderˉslots = [];
        private Mutableˉblock? Currentˉblock;

        public Functionˉbuilder(
            Functionˉsymbol function,
            Diagnosticˉbag diagnostics,
            IReadOnlyDictionary<string, Dataˉdeclaration> data,
            IReadOnlyDictionary<string, Constantˉvalue> constants,
            IReadOnlyDictionary<string, Functionˉsymbol> functions,
            IReadOnlyDictionary<string, Capabilityˉdeclaration> capabilities,
            IReadOnlyDictionary<string, Recordˉsymbol> records,
            IReadOnlyDictionary<string, Enumˉsymbol> enums,
            IReadOnlyDictionary<string, Variantˉsymbol> variants,
            Func<string, string> getˉtextˉdata)
        {
            Function = function;
            Diagnostics = diagnostics;
            Data = data;
            Constants = constants;
            Functions = functions;
            Capabilities = capabilities;
            Records = records;
            Enums = enums;
            Variants = variants;
            Getˉtextˉdata = getˉtextˉdata;
        }

        public Wirˉfunction Compile()
        {
            Enterˉscope();
            foreach (var Parameter in Function.Parameters)
            {
                if (Allˉlocalˉnames.Add(Parameter.Name))
                {
                    Scopes.Peek().Add(Parameter.Name, new(Parameter.Name, Parameter.Type, Parameter.Slot, false));
                }
            }

            Currentˉblock = Createˉblock();
            Compileˉblock(Function.Syntax.Body, createˉscope: false);
            if (Currentˉblock is not null)
            {
                if (Function.Returnˉtype == Valueˉtype.Void)
                {
                    Currentˉblock.Terminator = new Wirˉreturn(null);
                    Currentˉblock = null;
                }
                else
                {
                    Report(
                        "WVC2030",
                        Function.Syntax.Body.Span,
                        $"Function '{Function.Name}' can reach the end without returning {Formatˉtype(Function.Returnˉtype)}.");
                    var Fallbackˉblock = Currentˉblock;
                    var Temporary = Emitˉresult(Wirˉoperation.I32ˉconstant, Valueˉtype.I32, integerˉoperand: 0);
                    Fallbackˉblock.Terminator = new Wirˉreturn(Temporary);
                    Currentˉblock = null;
                }
            }

            Exitˉscope();
            var Frozenˉblocks = Blocks.Select(Block => new Wirˉblock(
                Block.Id,
                [.. Block.Instructions],
                Block.Terminator ?? throw new InvalidOperationException(
                    $"WIR block {Block.Id} in function '{Function.Name}' has no terminator.")));

            return new(
                Function.Name,
                Function.Parameterˉtypes,
                Function.Returnˉtype,
                [.. Userˉlocalˉtypes],
                [.. Temporaryˉtypes],
                [.. Frozenˉblocks],
                Function.Isˉexported);
        }

        private void Compileˉblock(Blockˉstatementˉsyntax block, bool createˉscope = true)
        {
            if (createˉscope)
            {
                Enterˉscope();
            }

            foreach (var Statement in block.Statements)
            {
                if (Currentˉblock is null)
                {
                    Report("WVC2031", Statement.Span, "This statement is unreachable.");
                    continue;
                }

                Compileˉstatement(Statement);
            }

            if (createˉscope)
            {
                Exitˉscope();
            }
        }

        private void Compileˉstatement(Statementˉsyntax statement)
        {
            switch (statement)
            {
                case Blockˉstatementˉsyntax Block:
                    Compileˉblock(Block);
                    break;
                case Localˉdeclarationˉstatementˉsyntax Localˉdeclaration:
                    Compileˉlocalˉdeclaration(Localˉdeclaration);
                    break;
                case Assignmentˉstatementˉsyntax Assignment:
                    Compileˉassignment(Assignment);
                    break;
                case Expressionˉstatementˉsyntax Expression:
                    _ = Compileˉexpression(Expression.Expression);
                    break;
                case Ifˉstatementˉsyntax If:
                    Compileˉif(If);
                    break;
                case Whileˉstatementˉsyntax While:
                    Compileˉwhile(While);
                    break;
                case Pushˉstatementˉsyntax Push:
                    Compileˉpush(Push);
                    break;
                case Forˉstatementˉsyntax For:
                    Compileˉfor(For);
                    break;
                case Returnˉstatementˉsyntax Return:
                    Compileˉreturn(Return);
                    break;
                case Breakˉstatementˉsyntax Break:
                    Compileˉbreak(Break);
                    break;
                case Continueˉstatementˉsyntax Continue:
                    Compileˉcontinue(Continue);
                    break;
                case Matchˉstatementˉsyntax Match:
                    Compileˉmatch(Match);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown statement syntax '{statement.GetType().Name}'.");
            }
        }

        private void Compileˉlocalˉdeclaration(Localˉdeclarationˉstatementˉsyntax statement)
        {
            var Initializer = Compileˉexpression(statement.Initializer);
            Valueˉshape Type;
            if (statement.Type is null)
            {
                if (Initializer.Type == Valueˉtype.Void)
                {
                    Report(
                        "WVC2044",
                        statement.Initializer.Span,
                        "A local type cannot be inferred from a void initializer.");
                    Initializer = Result(Wirˉoperation.I32ˉconstant, Valueˉtype.I32, integerˉoperand: 0);
                }

                Type = Initializer.Type;
            }
            else
            {
                Type = Bindˉvalueˉshape(statement.Type);
                Requireˉtype(Initializer, Type, statement.Initializer.Span, "local initializer");
            }

            if (Type.Kind == Valueˉtype.Builder && !statement.Isˉmutable)
            {
                Report("WVC2152", statement.Span, "An affine builder must be stored in a mutable 'var' local.");
            }

            if (!Seedˉnames.Isˉidentifier(statement.Name.Text))
            {
                Report(
                    "WVC2043",
                    statement.Name.Span,
                    $"Local name '{statement.Name.Text}' is not a valid Windvale identifier.");
                return;
            }

            if (!Allˉlocalˉnames.Add(statement.Name.Text))
            {
                Report(
                    "WVC2040",
                    statement.Name.Span,
                    $"Local or parameter '{statement.Name.Text}' is already declared in this function.");
                return;
            }

            var Slot = Function.Parameters.Length + Userˉlocalˉtypes.Count;
            Userˉlocalˉtypes.Add(Type);
            Scopes.Peek().Add(statement.Name.Text, new(statement.Name.Text, Type, Slot, statement.Isˉmutable));
            Emit(
                new(
                    Wirˉoperation.Storeˉlocal,
                    null,
                    [Initializer.Temporary],
                    Integerˉoperand: Slot));
        }

        private void Compileˉassignment(Assignmentˉstatementˉsyntax statement)
        {
            if (!Tryˉlookupˉlocal(statement.Name.Text, out var Local))
            {
                Report(
                    "WVC2041",
                    statement.Name.Span,
                    $"Local or parameter '{statement.Name.Text}' is not declared in this scope.");
                _ = Compileˉexpression(statement.Value);
                return;
            }

            if (!Local.Isˉmutable)
            {
                Report(
                    "WVC2042",
                    statement.Name.Span,
                    $"Local or parameter '{statement.Name.Text}' is immutable; copy it to a 'var' local before assigning.");
                _ = Compileˉexpression(statement.Value);
                return;
            }

            if (Local.Type.Kind == Valueˉtype.Builder)
            {
                Report(
                    "WVC2153",
                    statement.Span,
                    "An affine builder can be changed only by 'push' and consumed only by 'freeze'.");
                _ = Compileˉexpression(statement.Value);
                return;
            }

            Boundˉvalue Value;
            if (statement.Operator == Tokenˉkind.Equals)
            {
                Value = Compileˉexpression(statement.Value);
            }
            else
            {
                var Underlyingˉoperator = statement.Operator switch
                {
                    Tokenˉkind.Plusˉequals => Tokenˉkind.Plus,
                    Tokenˉkind.Minusˉequals => Tokenˉkind.Minus,
                    Tokenˉkind.Starˉequals => Tokenˉkind.Star,
                    _ => throw new InvalidOperationException(
                        $"Unknown assignment operator '{statement.Operator}'."),
                };
                Value = Compileˉbinary(new(
                    new Nameˉexpressionˉsyntax(statement.Name.Text, statement.Name.Span),
                    Underlyingˉoperator,
                    statement.Value,
                    statement.Span));
            }
            Requireˉtype(Value, Local.Type, statement.Value.Span, "assignment");
            Emit(new(
                Wirˉoperation.Storeˉlocal,
                null,
                [Value.Temporary],
                Integerˉoperand: Local.Slot));
        }

        private void Compileˉif(Ifˉstatementˉsyntax statement)
        {
            var Condition = Compileˉexpression(statement.Condition);
            Requireˉtype(Condition, Valueˉtype.Bool, statement.Condition.Span, "if condition");
            var Branchˉsource = Requireˉcurrentˉblock();
            var Thenˉblock = Createˉblock();
            var Elseˉblock = Createˉblock();
            Branchˉsource.Terminator = new Wirˉbranch(Condition.Temporary, Thenˉblock.Id, Elseˉblock.Id);

            Currentˉblock = Thenˉblock;
            Compileˉblock(statement.Then);
            var Thenˉend = Currentˉblock;

            Currentˉblock = Elseˉblock;
            if (statement.Else is not null)
            {
                Compileˉblock(statement.Else);
            }

            var Elseˉend = Currentˉblock;
            if (Thenˉend is null && Elseˉend is null)
            {
                Currentˉblock = null;
                return;
            }

            var Joinˉblock = Createˉblock();
            if (Thenˉend is not null)
            {
                Thenˉend.Terminator = new Wirˉjump(Joinˉblock.Id);
            }

            if (Elseˉend is not null)
            {
                Elseˉend.Terminator = new Wirˉjump(Joinˉblock.Id);
            }

            Currentˉblock = Joinˉblock;
        }

        private void Compileˉwhile(Whileˉstatementˉsyntax statement)
        {
            var Entry = Requireˉcurrentˉblock();
            var Header = Createˉblock();
            var Body = Createˉblock();
            var After = Createˉblock();
            Entry.Terminator = new Wirˉjump(Header.Id);

            Currentˉblock = Header;
            var Condition = Compileˉexpression(statement.Condition);
            Requireˉtype(Condition, Valueˉtype.Bool, statement.Condition.Span, "while condition");
            Header.Terminator = new Wirˉbranch(Condition.Temporary, Body.Id, After.Id);

            Currentˉblock = Body;
            Loops.Push(new(Header.Id, After.Id));
            Compileˉblock(statement.Body);
            Loops.Pop();
            if (Currentˉblock is not null)
            {
                Currentˉblock.Terminator = new Wirˉjump(Header.Id);
            }

            Currentˉblock = After;
        }

        private void Compileˉpush(Pushˉstatementˉsyntax statement)
        {
            var Value = Compileˉexpression(statement.Value);
            if (!Tryˉlookupˉlocal(statement.Builder.Text, out var Builder))
            {
                Report("WVC2041", statement.Builder.Span, $"Local or parameter '{statement.Builder.Text}' is not declared in this scope.");
                return;
            }
            if (Builder.Type.Kind != Valueˉtype.Builder || !Builder.Isˉmutable)
            {
                Report("WVC2154", statement.Builder.Span, "'push' requires one mutable builder local.");
                return;
            }
            if (Consumedˉbuilderˉslots.Contains(Builder.Slot))
            {
                Report("WVC2155", statement.Builder.Span, $"Builder '{Builder.Name}' was already consumed.");
                return;
            }

            Requireˉtype(Value, Builder.Type.Elementˉshape, statement.Value.Span, "pushed value");
            var Loaded = Result(
                Wirˉoperation.Loadˉlocal,
                Builder.Type,
                integerˉoperand: Builder.Slot);
            var Pushed = Result(
                Wirˉoperation.Builderˉpush,
                Builder.Type,
                [Loaded.Temporary, Value.Temporary]);
            Emit(new(
                Wirˉoperation.Storeˉlocal,
                null,
                [Pushed.Temporary],
                Integerˉoperand: Builder.Slot));
        }

        private void Compileˉfor(Forˉstatementˉsyntax statement)
        {
            var Sequence = Compileˉexpression(statement.Sequence);
            if (Sequence.Type.Kind != Valueˉtype.Sequence)
            {
                Report("WVC2156", statement.Sequence.Span, "A 'for' loop requires one bounded sequence value.");
                Compileˉblock(statement.Body);
                return;
            }

            var Sequenceˉslot = Function.Parameters.Length + Userˉlocalˉtypes.Count;
            Userˉlocalˉtypes.Add(Sequence.Type);
            Emit(new(Wirˉoperation.Storeˉlocal, null, [Sequence.Temporary], Integerˉoperand: Sequenceˉslot));

            var Indexˉslot = Function.Parameters.Length + Userˉlocalˉtypes.Count;
            Userˉlocalˉtypes.Add(Valueˉtype.U32);
            var Zero = Result(Wirˉoperation.U32ˉconstant, Valueˉtype.U32, unsignedˉintegerˉoperand: 0);
            Emit(new(Wirˉoperation.Storeˉlocal, null, [Zero.Temporary], Integerˉoperand: Indexˉslot));

            var Bindingˉslot = Function.Parameters.Length + Userˉlocalˉtypes.Count;
            Userˉlocalˉtypes.Add(Sequence.Type.Elementˉshape);
            Enterˉscope();
            if (!Seedˉnames.Isˉidentifier(statement.Binding.Text))
            {
                Report("WVC2043", statement.Binding.Span, $"Loop binding '{statement.Binding.Text}' is not a valid Windvale identifier.");
            }
            else if (!Allˉlocalˉnames.Add(statement.Binding.Text))
            {
                Report("WVC2040", statement.Binding.Span, $"Local or parameter '{statement.Binding.Text}' is already declared in this function.");
            }
            else
            {
                Scopes.Peek().Add(statement.Binding.Text, new(
                    statement.Binding.Text,
                    Sequence.Type.Elementˉshape,
                    Bindingˉslot,
                    false));
            }

            var Entry = Requireˉcurrentˉblock();
            var Header = Createˉblock();
            var Body = Createˉblock();
            var Increment = Createˉblock();
            var After = Createˉblock();
            Entry.Terminator = new Wirˉjump(Header.Id);

            Currentˉblock = Header;
            var Headerˉindex = Result(Wirˉoperation.Loadˉlocal, Valueˉtype.U32, integerˉoperand: Indexˉslot);
            var Headerˉsequence = Result(Wirˉoperation.Loadˉlocal, Sequence.Type, integerˉoperand: Sequenceˉslot);
            var Length = Result(Wirˉoperation.Sequenceˉlength, Valueˉtype.U32, [Headerˉsequence.Temporary]);
            var Hasˉelement = Result(
                Wirˉoperation.U32ˉless,
                Valueˉtype.Bool,
                [Headerˉindex.Temporary, Length.Temporary]);
            Header.Terminator = new Wirˉbranch(Hasˉelement.Temporary, Body.Id, After.Id);

            Currentˉblock = Body;
            var Bodyˉsequence = Result(Wirˉoperation.Loadˉlocal, Sequence.Type, integerˉoperand: Sequenceˉslot);
            var Bodyˉindex = Result(Wirˉoperation.Loadˉlocal, Valueˉtype.U32, integerˉoperand: Indexˉslot);
            var Element = Result(
                Wirˉoperation.Sequenceˉelement,
                Sequence.Type.Elementˉshape,
                [Bodyˉsequence.Temporary, Bodyˉindex.Temporary]);
            Emit(new(Wirˉoperation.Storeˉlocal, null, [Element.Temporary], Integerˉoperand: Bindingˉslot));
            Loops.Push(new(Increment.Id, After.Id));
            Compileˉblock(statement.Body, createˉscope: false);
            Loops.Pop();
            if (Currentˉblock is not null)
            {
                Currentˉblock.Terminator = new Wirˉjump(Increment.Id);
            }

            Currentˉblock = Increment;
            var Incrementˉindex = Result(Wirˉoperation.Loadˉlocal, Valueˉtype.U32, integerˉoperand: Indexˉslot);
            var One = Result(Wirˉoperation.U32ˉconstant, Valueˉtype.U32, unsignedˉintegerˉoperand: 1);
            var Next = Result(Wirˉoperation.U32ˉadd, Valueˉtype.U32, [Incrementˉindex.Temporary, One.Temporary]);
            Emit(new(Wirˉoperation.Storeˉlocal, null, [Next.Temporary], Integerˉoperand: Indexˉslot));
            Increment.Terminator = new Wirˉjump(Header.Id);

            Exitˉscope();
            Currentˉblock = After;
        }

        private void Compileˉbreak(Breakˉstatementˉsyntax statement)
        {
            if (Loops.Count == 0)
            {
                Report("WVC2111", statement.Span, "'break' is valid only inside a loop.");
                return;
            }

            Requireˉcurrentˉblock().Terminator = new Wirˉjump(Loops.Peek().Breakˉblock);
            Currentˉblock = null;
        }

        private void Compileˉcontinue(Continueˉstatementˉsyntax statement)
        {
            if (Loops.Count == 0)
            {
                Report("WVC2112", statement.Span, "'continue' is valid only inside a loop.");
                return;
            }

            Requireˉcurrentˉblock().Terminator = new Wirˉjump(Loops.Peek().Continueˉblock);
            Currentˉblock = null;
        }

        private void Compileˉmatch(Matchˉstatementˉsyntax statement)
        {
            var Value = Compileˉexpression(statement.Value);
            if (Value.Type.Kind == Valueˉtype.Enum &&
                Enums.Values.FirstOrDefault(Enum => Enum.Index == Value.Type.Nominalˉtypeˉindex) is { } Enum)
            {
                Compileˉenumˉmatch(statement, Value, Enum);
                return;
            }

            if (Value.Type.Kind == Valueˉtype.Variant &&
                Variants.Values.FirstOrDefault(Variant => Variant.Index == Value.Type.Nominalˉtypeˉindex) is { } Variant)
            {
                Compileˉvariantˉmatch(statement, Value, Variant);
                return;
            }

            Report("WVC2124", statement.Value.Span, "A match value must have one nominal enum or variant type.");
            foreach (var Case in statement.Cases)
            {
                if (Currentˉblock is not null)
                {
                    Compileˉblock(Case.Body);
                }
            }
        }

        private void Compileˉenumˉmatch(
            Matchˉstatementˉsyntax statement,
            Boundˉvalue value,
            Enumˉsymbol enumˉsymbol)
        {
            if (statement.Cases.Length == 0)
            {
                Report("WVC2125", statement.Span, $"Match over enum '{enumˉsymbol.Name}' requires one case per member.");
                return;
            }

            var Seen = new bool[enumˉsymbol.Declaration.Members.Length];
            var Memberˉindices = ImmutableArray.CreateBuilder<int>(statement.Cases.Length);
            foreach (var Case in statement.Cases)
            {
                if (!StringComparer.Ordinal.Equals(Case.Nominalˉname, enumˉsymbol.Name))
                {
                    Report(
                        "WVC2126",
                        Case.Span,
                        $"Match case '{Case.Nominalˉname}.{Case.Memberˉname}' does not belong to enum '{enumˉsymbol.Name}'.");
                }
                if (Case.Binding is not null)
                {
                    Report("WVC2137", Case.Binding.Span, "Enum match cases cannot bind a payload.");
                }

                var Memberˉindex = -1;
                for (var Index = 0; Index < enumˉsymbol.Declaration.Members.Length; Index++)
                {
                    if (StringComparer.Ordinal.Equals(
                        enumˉsymbol.Declaration.Members[Index].Name,
                        Case.Memberˉname))
                    {
                        Memberˉindex = Index;
                        break;
                    }
                }
                if (Memberˉindex < 0)
                {
                    Report(
                        "WVC2127",
                        Case.Span,
                        $"Enum '{enumˉsymbol.Name}' has no member '{Case.Memberˉname}'.");
                    Memberˉindex = 0;
                }
                else if (Seen[Memberˉindex])
                {
                    Report(
                        "WVC2128",
                        Case.Span,
                        $"Match case '{enumˉsymbol.Name}.{Case.Memberˉname}' is repeated.");
                }
                else
                {
                    Seen[Memberˉindex] = true;
                }
                Memberˉindices.Add(Memberˉindex);
            }
            for (var Index = 0; Index < Seen.Length; Index++)
            {
                if (!Seen[Index])
                {
                    Report(
                        "WVC2129",
                        statement.Span,
                        $"Match over enum '{enumˉsymbol.Name}' is missing case '{enumˉsymbol.Name}.{enumˉsymbol.Declaration.Members[Index].Name}'.");
                }
            }

            var Testˉblock = Requireˉcurrentˉblock();
            var Fallthroughˉblocks = new List<Mutableˉblock>();
            for (var Index = 0; Index < statement.Cases.Length; Index++)
            {
                var Caseˉblock = Createˉblock();
                if (Index + 1 == statement.Cases.Length)
                {
                    Testˉblock.Terminator = new Wirˉjump(Caseˉblock.Id);
                }
                else
                {
                    Currentˉblock = Testˉblock;
                    var Member = Result(
                        Wirˉoperation.Enumˉconstant,
                        Valueˉshape.Forˉenum(enumˉsymbol.Index),
                        unsignedˉintegerˉoperand: (uint)enumˉsymbol.Index,
                        secondˉunsignedˉintegerˉoperand: (uint)Memberˉindices[Index]);
                    var Equal = Result(
                        Wirˉoperation.Enumˉequal,
                        Valueˉtype.Bool,
                        [value.Temporary, Member.Temporary]);
                    var Nextˉtest = Createˉblock();
                    Testˉblock.Terminator = new Wirˉbranch(
                        Equal.Temporary,
                        Caseˉblock.Id,
                        Nextˉtest.Id);
                    Testˉblock = Nextˉtest;
                }

                Currentˉblock = Caseˉblock;
                Compileˉblock(statement.Cases[Index].Body);
                if (Currentˉblock is not null)
                {
                    Fallthroughˉblocks.Add(Currentˉblock);
                }
            }

            if (Fallthroughˉblocks.Count == 0)
            {
                Currentˉblock = null;
                return;
            }
            var Joinˉblock = Createˉblock();
            foreach (var Block in Fallthroughˉblocks)
            {
                Block.Terminator = new Wirˉjump(Joinˉblock.Id);
            }
            Currentˉblock = Joinˉblock;
        }

        private void Compileˉvariantˉmatch(
            Matchˉstatementˉsyntax statement,
            Boundˉvalue value,
            Variantˉsymbol variant)
        {
            if (statement.Cases.Length == 0)
            {
                Report("WVC2138", statement.Span, $"Match over variant '{variant.Name}' requires one case per variant case.");
                return;
            }

            var Seen = new bool[variant.Declaration.Cases.Length];
            var Caseˉindices = ImmutableArray.CreateBuilder<int>(statement.Cases.Length);
            foreach (var Case in statement.Cases)
            {
                if (!StringComparer.Ordinal.Equals(Case.Nominalˉname, variant.Name))
                {
                    Report(
                        "WVC2139",
                        Case.Span,
                        $"Match case '{Case.Nominalˉname}.{Case.Memberˉname}' does not belong to variant '{variant.Name}'.");
                }

                var Caseˉindex = -1;
                for (var Index = 0; Index < variant.Declaration.Cases.Length; Index++)
                {
                    if (StringComparer.Ordinal.Equals(variant.Declaration.Cases[Index].Name, Case.Memberˉname))
                    {
                        Caseˉindex = Index;
                        break;
                    }
                }

                if (Caseˉindex < 0)
                {
                    Report("WVC2140", Case.Span, $"Variant '{variant.Name}' has no case '{Case.Memberˉname}'.");
                    Caseˉindex = 0;
                }
                else if (Seen[Caseˉindex])
                {
                    Report("WVC2141", Case.Span, $"Match case '{variant.Name}.{Case.Memberˉname}' is repeated.");
                }
                else
                {
                    Seen[Caseˉindex] = true;
                }

                var Declaration = variant.Declaration.Cases[Caseˉindex];
                if (Declaration.Payloadˉtype is null && Case.Binding is not null)
                {
                    Report("WVC2142", Case.Binding.Span, $"Variant case '{variant.Name}.{Declaration.Name}' has no payload to bind.");
                }
                else if (Declaration.Payloadˉtype is not null && Case.Binding is null)
                {
                    Report("WVC2143", Case.Span, $"Variant case '{variant.Name}.{Declaration.Name}' requires one payload binding.");
                }
                else if (Case.Binding is not null && !Seedˉnames.Isˉidentifier(Case.Binding.Text))
                {
                    Report("WVC2144", Case.Binding.Span, $"Payload binding '{Case.Binding.Text}' is not a valid Windvale identifier.");
                }

                Caseˉindices.Add(Caseˉindex);
            }

            for (var Index = 0; Index < Seen.Length; Index++)
            {
                if (!Seen[Index])
                {
                    Report(
                        "WVC2145",
                        statement.Span,
                        $"Match over variant '{variant.Name}' is missing case '{variant.Name}.{variant.Declaration.Cases[Index].Name}'.");
                }
            }

            var Testˉblock = Requireˉcurrentˉblock();
            var Fallthroughˉblocks = new List<Mutableˉblock>();
            for (var Index = 0; Index < statement.Cases.Length; Index++)
            {
                var Caseˉblock = Createˉblock();
                if (Index + 1 == statement.Cases.Length)
                {
                    Testˉblock.Terminator = new Wirˉjump(Caseˉblock.Id);
                }
                else
                {
                    Currentˉblock = Testˉblock;
                    var Isˉcase = Result(
                        Wirˉoperation.Variantˉisˉcase,
                        Valueˉtype.Bool,
                        [value.Temporary],
                        unsignedˉintegerˉoperand: (uint)variant.Index,
                        secondˉunsignedˉintegerˉoperand: (uint)Caseˉindices[Index]);
                    var Nextˉtest = Createˉblock();
                    Testˉblock.Terminator = new Wirˉbranch(Isˉcase.Temporary, Caseˉblock.Id, Nextˉtest.Id);
                    Testˉblock = Nextˉtest;
                }

                Currentˉblock = Caseˉblock;
                Enterˉscope();
                var Syntaxˉcase = statement.Cases[Index];
                var Declaration = variant.Declaration.Cases[Caseˉindices[Index]];
                if (Declaration.Payloadˉtype is { } Payloadˉtype && Syntaxˉcase.Binding is { } Binding)
                {
                    var Payload = Result(
                        Wirˉoperation.Variantˉpayload,
                        Payloadˉtype,
                        [value.Temporary],
                        unsignedˉintegerˉoperand: (uint)variant.Index,
                        secondˉunsignedˉintegerˉoperand: (uint)Caseˉindices[Index]);
                    var Slot = Function.Parameters.Length + Userˉlocalˉtypes.Count;
                    Userˉlocalˉtypes.Add(Payloadˉtype);
                    if (Seedˉnames.Isˉidentifier(Binding.Text))
                    {
                        Scopes.Peek().TryAdd(Binding.Text, new(Binding.Text, Payloadˉtype, Slot, false));
                    }
                    Emit(new(Wirˉoperation.Storeˉlocal, null, [Payload.Temporary], Integerˉoperand: Slot));
                }
                Compileˉblock(Syntaxˉcase.Body, createˉscope: false);
                Exitˉscope();
                if (Currentˉblock is not null)
                {
                    Fallthroughˉblocks.Add(Currentˉblock);
                }
            }

            if (Fallthroughˉblocks.Count == 0)
            {
                Currentˉblock = null;
                return;
            }
            var Joinˉblock = Createˉblock();
            foreach (var Block in Fallthroughˉblocks)
            {
                Block.Terminator = new Wirˉjump(Joinˉblock.Id);
            }
            Currentˉblock = Joinˉblock;
        }

        private void Compileˉreturn(Returnˉstatementˉsyntax statement)
        {
            var Block = Requireˉcurrentˉblock();
            if (Function.Returnˉtype == Valueˉtype.Void)
            {
                if (statement.Value is not null)
                {
                    Report("WVC2050", statement.Value.Span, "A void function cannot return a value.");
                    _ = Compileˉexpression(statement.Value);
                }

                Block.Terminator = new Wirˉreturn(null);
            }
            else if (statement.Value is null)
            {
                Report(
                    "WVC2051",
                    statement.Span,
                    $"Function '{Function.Name}' must return {Formatˉtype(Function.Returnˉtype)}.");
                var Fallback = Emitˉresult(Wirˉoperation.I32ˉconstant, Valueˉtype.I32, integerˉoperand: 0);
                Block.Terminator = new Wirˉreturn(Fallback);
            }
            else
            {
                var Value = Compileˉexpression(statement.Value);
                Requireˉtype(Value, Function.Returnˉtype, statement.Value.Span, "return value");
                Block.Terminator = new Wirˉreturn(Value.Temporary);
            }

            Currentˉblock = null;
        }

        private Boundˉvalue Compileˉexpression(Expressionˉsyntax expression)
        {
            return expression switch
            {
                Literalˉexpressionˉsyntax Literal => Compileˉliteral(Literal),
                Nameˉexpressionˉsyntax Name => Compileˉname(Name),
                Unaryˉexpressionˉsyntax Unary => Compileˉunary(Unary),
                Binaryˉexpressionˉsyntax Binary => Compileˉbinary(Binary),
                Callˉexpressionˉsyntax Call => Compileˉcall(Call),
                Builderˉexpressionˉsyntax Builder => Compileˉbuilder(Builder),
                Recordˉexpressionˉsyntax Record => Compileˉrecord(Record),
                Indexˉexpressionˉsyntax Index => Compileˉindex(Index),
                Fieldˉexpressionˉsyntax Field => Compileˉfield(Field),
                Invalidˉexpressionˉsyntax Invalid => Invalidˉvalue(Invalid.Span),
                _ => throw new InvalidOperationException($"Unknown expression syntax '{expression.GetType().Name}'."),
            };
        }

        private Boundˉvalue Compileˉliteral(Literalˉexpressionˉsyntax expression)
        {
            return expression.Value switch
            {
                int Integer => Result(
                    Wirˉoperation.I32ˉconstant,
                    Valueˉtype.I32,
                    integerˉoperand: Integer),
                long I64 => Result(
                    Wirˉoperation.I64ˉconstant,
                    Valueˉtype.I64,
                    wideˉintegerˉoperand: I64),
                byte U8 => Result(
                    Wirˉoperation.U8ˉconstant,
                    Valueˉtype.U8,
                    unsignedˉintegerˉoperand: U8),
                uint U32 => Result(
                    Wirˉoperation.U32ˉconstant,
                    Valueˉtype.U32,
                    unsignedˉintegerˉoperand: U32),
                ulong U64 => Result(
                    Wirˉoperation.U64ˉconstant,
                    Valueˉtype.U64,
                    unsignedˉwideˉintegerˉoperand: U64),
                bool Boolean => Result(
                    Wirˉoperation.Boolˉconstant,
                    Valueˉtype.Bool,
                    integerˉoperand: Boolean ? 1 : 0),
                string Text => Result(
                    Wirˉoperation.Textˉconstant,
                    Valueˉtype.Text,
                    nameˉoperand: Getˉtextˉdata(Text)),
                _ => Invalidˉvalue(expression.Span),
            };
        }

        private Boundˉvalue Compileˉname(Nameˉexpressionˉsyntax expression)
        {
            if (Tryˉlookupˉlocal(expression.Name, out var Local))
            {
                if (Local.Type.Kind == Valueˉtype.Builder)
                {
                    Report(
                        "WVC2153",
                        expression.Span,
                        "An affine builder cannot be copied or used as an ordinary expression.");
                    return Invalidˉvalue(expression.Span);
                }
                return Result(
                    Wirˉoperation.Loadˉlocal,
                    Local.Type,
                    integerˉoperand: Local.Slot);
            }

            if (Constants.TryGetValue(expression.Name, out var Constant))
            {
                return Compileˉconstant(Constant);
            }

            if (Data.TryGetValue(expression.Name, out var Declaration))
            {
                if (Declaration is Textˉdataˉdeclaration)
                {
                    return Result(
                        Wirˉoperation.Textˉconstant,
                        Valueˉtype.Text,
                        nameˉoperand: Declaration.Name);
                }

                if (Declaration is Bytesˉdataˉdeclaration)
                {
                    return Result(
                        Wirˉoperation.Bytesˉconstant,
                        Valueˉtype.Bytes,
                        nameˉoperand: Declaration.Name);
                }

                Report(
                    "WVC2060",
                    expression.Span,
                    $"Array data '{expression.Name}' must be indexed or passed to length().");
                return Invalidˉvalue(expression.Span);
            }

            Report("WVC2061", expression.Span, $"Name '{expression.Name}' is not declared.");
            return Invalidˉvalue(expression.Span);
        }

        private Boundˉvalue Compileˉconstant(Constantˉvalue constant)
        {
            return constant.Value switch
            {
                int Integer => Result(
                    Wirˉoperation.I32ˉconstant,
                    constant.Type,
                    integerˉoperand: Integer),
                long I64 => Result(
                    Wirˉoperation.I64ˉconstant,
                    constant.Type,
                    wideˉintegerˉoperand: I64),
                byte U8 => Result(
                    Wirˉoperation.U8ˉconstant,
                    constant.Type,
                    unsignedˉintegerˉoperand: U8),
                uint U32 => Result(
                    Wirˉoperation.U32ˉconstant,
                    constant.Type,
                    unsignedˉintegerˉoperand: U32),
                ulong U64 => Result(
                    Wirˉoperation.U64ˉconstant,
                    constant.Type,
                    unsignedˉwideˉintegerˉoperand: U64),
                bool Boolean => Result(
                    Wirˉoperation.Boolˉconstant,
                    constant.Type,
                    integerˉoperand: Boolean ? 1 : 0),
                Enumˉconstantˉvalue Enum => Result(
                    Wirˉoperation.Enumˉconstant,
                    constant.Type,
                    unsignedˉintegerˉoperand: (uint)Enum.Typeˉindex,
                    secondˉunsignedˉintegerˉoperand: (uint)Enum.Memberˉindex),
                _ => throw new InvalidOperationException("Unknown constant value."),
            };
        }

        private Boundˉvalue Compileˉunary(Unaryˉexpressionˉsyntax expression)
        {
            if (expression.Operator == Tokenˉkind.Freeze)
            {
                return Compileˉfreeze(expression);
            }

            var Operand = Compileˉexpression(expression.Operand);
            if (expression.Operator == Tokenˉkind.Minus)
            {
                if (Operand.Type.Kind is not (Valueˉtype.I32 or Valueˉtype.I64))
                {
                    Report(
                        "WVC2070",
                        expression.Operand.Span,
                        $"The unary '-' operand has type {Formatˉtype(Operand.Type)}; i32 or i64 is required.");
                    return Invalidˉvalue(expression.Span);
                }

                return Result(
                    Operand.Type.Kind == Valueˉtype.I32
                        ? Wirˉoperation.I32ˉnegate
                        : Wirˉoperation.I64ˉnegate,
                    Operand.Type,
                    [Operand.Temporary]);
            }

            if (expression.Operator == Tokenˉkind.Tilde)
            {
                if (Operand.Type.Kind is not (Valueˉtype.U8 or Valueˉtype.U32 or Valueˉtype.U64))
                {
                    Report(
                        "WVC2160", expression.Operand.Span,
                        $"The unary '~' operand has type {Formatˉtype(Operand.Type)}; u8, u32, or u64 is required.");
                    return Invalidˉvalue(expression.Span);
                }
                return Result(
                    Operand.Type.Kind switch
                    {
                        Valueˉtype.U8 => Wirˉoperation.U8ˉbitwiseˉnot,
                        Valueˉtype.U32 => Wirˉoperation.U32ˉbitwiseˉnot,
                        _ => Wirˉoperation.U64ˉbitwiseˉnot,
                    },
                    Operand.Type, [Operand.Temporary]);
            }

            Requireˉtype(Operand, Valueˉtype.Bool, expression.Operand.Span, "unary '!' operand");
            return Result(Wirˉoperation.Boolˉnot, Valueˉtype.Bool, [Operand.Temporary]);
        }

        private Boundˉvalue Compileˉfreeze(Unaryˉexpressionˉsyntax expression)
        {
            if (expression.Operand is not Nameˉexpressionˉsyntax Name ||
                !Tryˉlookupˉlocal(Name.Name, out var Builder) ||
                Builder.Type.Kind != Valueˉtype.Builder ||
                !Builder.Isˉmutable)
            {
                Report("WVC2157", expression.Span, "'freeze' requires one mutable builder local.");
                return Invalidˉvalue(expression.Span);
            }
            if (!Consumedˉbuilderˉslots.Add(Builder.Slot))
            {
                Report("WVC2155", expression.Span, $"Builder '{Builder.Name}' was already consumed.");
                return Invalidˉvalue(expression.Span);
            }

            var Loaded = Result(
                Wirˉoperation.Loadˉlocal,
                Builder.Type,
                integerˉoperand: Builder.Slot);
            return Result(
                Wirˉoperation.Builderˉfreeze,
                Valueˉshape.Forˉsequence(Builder.Type.Elementˉshape, Builder.Type.Maximum),
                [Loaded.Temporary]);
        }

        private Boundˉvalue Compileˉbuilder(Builderˉexpressionˉsyntax expression)
        {
            var Type = Bindˉvalueˉshape(expression.Type);
            if (Type.Kind != Valueˉtype.Builder)
            {
                Report("WVC2158", expression.Span, "A builder constructor requires builder<T, N>().");
                return Invalidˉvalue(expression.Span);
            }

            return Result(
                Wirˉoperation.Builderˉcreate,
                Type,
                unsignedˉintegerˉoperand: Encodeˉcollectionˉelement(Type.Elementˉshape),
                secondˉunsignedˉintegerˉoperand: Type.Maximum);
        }

        private Boundˉvalue Compileˉbinary(Binaryˉexpressionˉsyntax expression)
        {
            if (expression.Operator is Tokenˉkind.Andˉand or Tokenˉkind.Orˉor)
            {
                return Compileˉshortˉcircuit(expression);
            }

            var Left = Compileˉexpression(expression.Left);
            var Right = Compileˉexpression(expression.Right);
            var Operands = ImmutableArray.Create(Left.Temporary, Right.Temporary);

            switch (expression.Operator)
            {
                case Tokenˉkind.Plus:
                case Tokenˉkind.Minus:
                case Tokenˉkind.Star:
                    if (Left.Type != Right.Type || Left.Type.Kind is not (
                        Valueˉtype.I32 or Valueˉtype.I64 or Valueˉtype.U32 or Valueˉtype.U64))
                    {
                        Report(
                            "WVC2068",
                            expression.Span,
                            "Arithmetic requires two i32, i64, u32, or u64 values of the same type.");
                        return Invalidˉvalue(expression.Span);
                    }

                    return Result(
                        (Left.Type.Kind, expression.Operator) switch
                        {
                            (Valueˉtype.I32, Tokenˉkind.Plus) => Wirˉoperation.I32ˉadd,
                            (Valueˉtype.I32, Tokenˉkind.Minus) => Wirˉoperation.I32ˉsubtract,
                            (Valueˉtype.I32, _) => Wirˉoperation.I32ˉmultiply,
                            (Valueˉtype.I64, Tokenˉkind.Plus) => Wirˉoperation.I64ˉadd,
                            (Valueˉtype.I64, Tokenˉkind.Minus) => Wirˉoperation.I64ˉsubtract,
                            (Valueˉtype.I64, _) => Wirˉoperation.I64ˉmultiply,
                            (Valueˉtype.U32, Tokenˉkind.Plus) => Wirˉoperation.U32ˉadd,
                            (Valueˉtype.U32, Tokenˉkind.Minus) => Wirˉoperation.U32ˉsubtract,
                            (Valueˉtype.U32, _) => Wirˉoperation.U32ˉmultiply,
                            (Valueˉtype.U64, Tokenˉkind.Plus) => Wirˉoperation.U64ˉadd,
                            (Valueˉtype.U64, Tokenˉkind.Minus) => Wirˉoperation.U64ˉsubtract,
                            _ => Wirˉoperation.U64ˉmultiply,
                        },
                        Left.Type,
                        Operands);
                case Tokenˉkind.Slash:
                case Tokenˉkind.Percent:
                    if (Left.Type != Right.Type || Left.Type.Kind is not (
                        Valueˉtype.I32 or Valueˉtype.I64 or Valueˉtype.U32 or Valueˉtype.U64))
                    {
                        Report(
                            "WVC2161", expression.Span,
                            "Division and remainder require two i32, i64, u32, or u64 values of the same type.");
                        return Invalidˉvalue(expression.Span);
                    }
                    return Result(
                        (Left.Type.Kind, expression.Operator) switch
                        {
                            (Valueˉtype.I32, Tokenˉkind.Slash) => Wirˉoperation.I32ˉdivide,
                            (Valueˉtype.I32, _) => Wirˉoperation.I32ˉremainder,
                            (Valueˉtype.I64, Tokenˉkind.Slash) => Wirˉoperation.I64ˉdivide,
                            (Valueˉtype.I64, _) => Wirˉoperation.I64ˉremainder,
                            (Valueˉtype.U32, Tokenˉkind.Slash) => Wirˉoperation.U32ˉdivide,
                            (Valueˉtype.U32, _) => Wirˉoperation.U32ˉremainder,
                            (Valueˉtype.U64, Tokenˉkind.Slash) => Wirˉoperation.U64ˉdivide,
                            _ => Wirˉoperation.U64ˉremainder,
                        },
                        Left.Type, Operands);
                case Tokenˉkind.Ampersand:
                case Tokenˉkind.Pipe:
                case Tokenˉkind.Caret:
                    if (Left.Type != Right.Type || Left.Type.Kind is not (
                        Valueˉtype.U8 or Valueˉtype.U32 or Valueˉtype.U64))
                    {
                        Report(
                            "WVC2162", expression.Span,
                            "Bitwise operators require two u8, u32, or u64 values of the same type.");
                        return Invalidˉvalue(expression.Span);
                    }
                    return Result(
                        (Left.Type.Kind, expression.Operator) switch
                        {
                            (Valueˉtype.U8, Tokenˉkind.Ampersand) => Wirˉoperation.U8ˉbitwiseˉand,
                            (Valueˉtype.U8, Tokenˉkind.Pipe) => Wirˉoperation.U8ˉbitwiseˉor,
                            (Valueˉtype.U8, _) => Wirˉoperation.U8ˉbitwiseˉxor,
                            (Valueˉtype.U32, Tokenˉkind.Ampersand) => Wirˉoperation.U32ˉbitwiseˉand,
                            (Valueˉtype.U32, Tokenˉkind.Pipe) => Wirˉoperation.U32ˉbitwiseˉor,
                            (Valueˉtype.U32, _) => Wirˉoperation.U32ˉbitwiseˉxor,
                            (Valueˉtype.U64, Tokenˉkind.Ampersand) => Wirˉoperation.U64ˉbitwiseˉand,
                            (Valueˉtype.U64, Tokenˉkind.Pipe) => Wirˉoperation.U64ˉbitwiseˉor,
                            _ => Wirˉoperation.U64ˉbitwiseˉxor,
                        },
                        Left.Type, Operands);
                case Tokenˉkind.Shiftˉleft:
                case Tokenˉkind.Shiftˉright:
                    if (Left.Type.Kind is not (Valueˉtype.U8 or Valueˉtype.U32 or Valueˉtype.U64) ||
                        Right.Type != Valueˉtype.U32)
                    {
                        Report(
                            "WVC2163", expression.Span,
                            "A shift requires a u8, u32, or u64 left operand and a u32 count.");
                        return Invalidˉvalue(expression.Span);
                    }
                    return Result(
                        (Left.Type.Kind, expression.Operator) switch
                        {
                            (Valueˉtype.U8, Tokenˉkind.Shiftˉleft) => Wirˉoperation.U8ˉshiftˉleft,
                            (Valueˉtype.U8, _) => Wirˉoperation.U8ˉshiftˉright,
                            (Valueˉtype.U32, Tokenˉkind.Shiftˉleft) => Wirˉoperation.U32ˉshiftˉleft,
                            (Valueˉtype.U32, _) => Wirˉoperation.U32ˉshiftˉright,
                            (Valueˉtype.U64, Tokenˉkind.Shiftˉleft) => Wirˉoperation.U64ˉshiftˉleft,
                            _ => Wirˉoperation.U64ˉshiftˉright,
                        },
                        Left.Type, Operands);
                case Tokenˉkind.Less:
                case Tokenˉkind.Lessˉequals:
                case Tokenˉkind.Greater:
                case Tokenˉkind.Greaterˉequals:
                    if (Left.Type != Right.Type || Left.Type.Kind is not (
                        Valueˉtype.I32 or Valueˉtype.I64 or Valueˉtype.U32 or Valueˉtype.U64))
                    {
                        Report(
                            "WVC2069",
                            expression.Span,
                            "Ordering requires two i32, i64, u32, or u64 values of the same type.");
                        return Invalidˉvalue(expression.Span);
                    }

                    return Result(
                        (Left.Type.Kind, expression.Operator) switch
                        {
                            (Valueˉtype.I32, Tokenˉkind.Less) => Wirˉoperation.I32ˉless,
                            (Valueˉtype.I32, Tokenˉkind.Lessˉequals) => Wirˉoperation.I32ˉlessˉequal,
                            (Valueˉtype.I32, Tokenˉkind.Greater) => Wirˉoperation.I32ˉgreater,
                            (Valueˉtype.I32, _) => Wirˉoperation.I32ˉgreaterˉequal,
                            (Valueˉtype.I64, Tokenˉkind.Less) => Wirˉoperation.I64ˉless,
                            (Valueˉtype.I64, Tokenˉkind.Lessˉequals) => Wirˉoperation.I64ˉlessˉequal,
                            (Valueˉtype.I64, Tokenˉkind.Greater) => Wirˉoperation.I64ˉgreater,
                            (Valueˉtype.I64, _) => Wirˉoperation.I64ˉgreaterˉequal,
                            (Valueˉtype.U32, Tokenˉkind.Less) => Wirˉoperation.U32ˉless,
                            (Valueˉtype.U32, Tokenˉkind.Lessˉequals) => Wirˉoperation.U32ˉlessˉequal,
                            (Valueˉtype.U32, Tokenˉkind.Greater) => Wirˉoperation.U32ˉgreater,
                            (Valueˉtype.U32, _) => Wirˉoperation.U32ˉgreaterˉequal,
                            (Valueˉtype.U64, Tokenˉkind.Less) => Wirˉoperation.U64ˉless,
                            (Valueˉtype.U64, Tokenˉkind.Lessˉequals) => Wirˉoperation.U64ˉlessˉequal,
                            (Valueˉtype.U64, Tokenˉkind.Greater) => Wirˉoperation.U64ˉgreater,
                            _ => Wirˉoperation.U64ˉgreaterˉequal,
                        },
                        Valueˉtype.Bool,
                        Operands);
                case Tokenˉkind.Equalsˉequals:
                case Tokenˉkind.Bangˉequals:
                    if (Left.Type != Right.Type ||
                        Left.Type.Kind is not (
                            Valueˉtype.I32 or
                            Valueˉtype.I64 or
                            Valueˉtype.U8 or
                            Valueˉtype.U32 or
                            Valueˉtype.U64 or
                            Valueˉtype.Bool or
                            Valueˉtype.Text or
                            Valueˉtype.Bytes or
                            Valueˉtype.Enum))
                    {
                        Report(
                            "WVC2062",
                            expression.Span,
                            "Equality requires two equal-shape scalar, text, bytes, or identical enum values.");
                        return Invalidˉvalue(expression.Span);
                    }

                    return Result(
                        (Left.Type.Kind, expression.Operator) switch
                        {
                            (Valueˉtype.I32, Tokenˉkind.Equalsˉequals) => Wirˉoperation.I32ˉequal,
                            (Valueˉtype.I32, _) => Wirˉoperation.I32ˉnotˉequal,
                            (Valueˉtype.I64, Tokenˉkind.Equalsˉequals) => Wirˉoperation.I64ˉequal,
                            (Valueˉtype.I64, _) => Wirˉoperation.I64ˉnotˉequal,
                            (Valueˉtype.U8, Tokenˉkind.Equalsˉequals) => Wirˉoperation.U8ˉequal,
                            (Valueˉtype.U8, _) => Wirˉoperation.U8ˉnotˉequal,
                            (Valueˉtype.U32, Tokenˉkind.Equalsˉequals) => Wirˉoperation.U32ˉequal,
                            (Valueˉtype.U32, _) => Wirˉoperation.U32ˉnotˉequal,
                            (Valueˉtype.U64, Tokenˉkind.Equalsˉequals) => Wirˉoperation.U64ˉequal,
                            (Valueˉtype.U64, _) => Wirˉoperation.U64ˉnotˉequal,
                            (Valueˉtype.Bool, Tokenˉkind.Equalsˉequals) => Wirˉoperation.Boolˉequal,
                            (Valueˉtype.Bool, _) => Wirˉoperation.Boolˉnotˉequal,
                            (Valueˉtype.Text, Tokenˉkind.Equalsˉequals) => Wirˉoperation.Textˉequal,
                            (Valueˉtype.Text, _) => Wirˉoperation.Textˉnotˉequal,
                            (Valueˉtype.Bytes, Tokenˉkind.Equalsˉequals) => Wirˉoperation.Bytesˉequal,
                            (Valueˉtype.Bytes, _) => Wirˉoperation.Bytesˉnotˉequal,
                            (Valueˉtype.Enum, Tokenˉkind.Equalsˉequals) => Wirˉoperation.Enumˉequal,
                            _ => Wirˉoperation.Enumˉnotˉequal,
                        },
                        Valueˉtype.Bool,
                        Operands);
                default:
                    throw new InvalidOperationException($"Unknown binary operator '{expression.Operator}'.");
            }
        }

        private Boundˉvalue Compileˉshortˉcircuit(Binaryˉexpressionˉsyntax expression)
        {
            var Left = Compileˉexpression(expression.Left);
            Requireˉtype(Left, Valueˉtype.Bool, expression.Left.Span, "short-circuit left operand");

            var Branchˉsource = Requireˉcurrentˉblock();
            var Rightˉblock = Createˉblock();
            var Shortˉblock = Createˉblock();
            var Joinˉblock = Createˉblock();
            Branchˉsource.Terminator = expression.Operator == Tokenˉkind.Andˉand
                ? new Wirˉbranch(Left.Temporary, Rightˉblock.Id, Shortˉblock.Id)
                : new Wirˉbranch(Left.Temporary, Shortˉblock.Id, Rightˉblock.Id);

            Currentˉblock = Shortˉblock;
            var Shortˉvalue = Result(
                Wirˉoperation.Boolˉconstant,
                Valueˉtype.Bool,
                integerˉoperand: expression.Operator == Tokenˉkind.Orˉor ? 1 : 0);
            var Shortˉend = Requireˉcurrentˉblock();
            Shortˉend.Terminator = new Wirˉjump(Joinˉblock.Id);

            Currentˉblock = Rightˉblock;
            var Right = Compileˉexpression(expression.Right);
            Requireˉtype(Right, Valueˉtype.Bool, expression.Right.Span, "short-circuit right operand");
            var Rightˉend = Requireˉcurrentˉblock();
            Rightˉend.Terminator = new Wirˉjump(Joinˉblock.Id);

            Currentˉblock = Joinˉblock;
            return Result(
                Wirˉoperation.Boolˉphi,
                Valueˉtype.Bool,
                [Shortˉvalue.Temporary, Right.Temporary],
                integerˉoperand: Shortˉend.Id,
                unsignedˉintegerˉoperand: checked((uint)Rightˉend.Id));
        }

        private Boundˉvalue Compileˉindex(Indexˉexpressionˉsyntax expression)
        {
            var Index = Compileˉexpression(expression.Index);
            if (Tryˉlookupˉlocal(expression.Name, out var Local) &&
                Local.Type.Kind == Valueˉtype.Sequence)
            {
                Requireˉtype(Index, Valueˉtype.U32, expression.Index.Span, "sequence index");
                var Sequence = Result(
                    Wirˉoperation.Loadˉlocal,
                    Local.Type,
                    integerˉoperand: Local.Slot);
                return Result(
                    Wirˉoperation.Sequenceˉelement,
                    Local.Type.Elementˉshape,
                    [Sequence.Temporary, Index.Temporary]);
            }

            Requireˉtype(Index, Valueˉtype.I32, expression.Index.Span, "data index");
            if (!Data.TryGetValue(expression.Name, out var Declaration) ||
                Declaration is not I32ˉarrayˉdataˉdeclaration)
            {
                Report(
                    "WVC2063",
                    expression.Span,
                    $"'{expression.Name}' is not immutable [i32] data.");
                return Invalidˉvalue(expression.Span);
            }

            return Result(
                Wirˉoperation.Dataˉloadˉi32,
                Valueˉtype.I32,
                [Index.Temporary],
                nameˉoperand: expression.Name);
        }

        private Boundˉvalue Compileˉcall(Callˉexpressionˉsyntax expression)
        {
            if (expression.Name == "length")
            {
                return Compileˉlength(expression);
            }

            if (expression.Name == Foundationˉintrinsics.ENUM_NAME)
            {
                return Compileˉenumˉname(expression);
            }

            if (Foundationˉintrinsics.Tryˉget(expression.Name, out var Intrinsic))
            {
                return Compileˉfoundationˉintrinsic(expression, Intrinsic);
            }

            if (Records.TryGetValue(expression.Name, out var Record))
            {
                var Recordˉarguments = expression.Arguments.Select(Compileˉexpression).ToImmutableArray();
                Checkˉarguments(
                    expression,
                    Recordˉarguments,
                    [.. Record.Declaration.Fields.Select(Field => Field.Type)]);
                return Result(
                    Wirˉoperation.Recordˉcreate,
                    Valueˉshape.Forˉrecord(Record.Index),
                    Recordˉarguments.Select(Argument => Argument.Temporary).ToImmutableArray(),
                    unsignedˉintegerˉoperand: (uint)Record.Index);
            }

            var Caseˉseparator = expression.Name.LastIndexOf('.');
            if (Caseˉseparator > 0 &&
                Variants.TryGetValue(expression.Name[..Caseˉseparator], out var Variant))
            {
                var Caseˉname = expression.Name[(Caseˉseparator + 1)..];
                var Caseˉindex = -1;
                for (var Index = 0; Index < Variant.Declaration.Cases.Length; Index++)
                {
                    if (StringComparer.Ordinal.Equals(Variant.Declaration.Cases[Index].Name, Caseˉname))
                    {
                        Caseˉindex = Index;
                        break;
                    }
                }
                if (Caseˉindex < 0)
                {
                    Report("WVC2140", expression.Span, $"Variant '{Variant.Name}' has no case '{Caseˉname}'.");
                    return Invalidˉvalue(expression.Span);
                }

                var Variantˉarguments = expression.Arguments.Select(Compileˉexpression).ToImmutableArray();
                var Payloadˉtype = Variant.Declaration.Cases[Caseˉindex].Payloadˉtype;
                Checkˉarguments(
                    expression,
                    Variantˉarguments,
                    Payloadˉtype is null ? [] : [Payloadˉtype.Value]);
                return Result(
                    Wirˉoperation.Variantˉcreate,
                    Valueˉshape.Forˉvariant(Variant.Index),
                    Variantˉarguments.Select(Argument => Argument.Temporary).ToImmutableArray(),
                    unsignedˉintegerˉoperand: (uint)Variant.Index,
                    secondˉunsignedˉintegerˉoperand: (uint)Caseˉindex);
            }

            var Arguments = expression.Arguments.Select(Compileˉexpression).ToImmutableArray();
            if (Functions.TryGetValue(expression.Name, out var Calledˉfunction))
            {
                Checkˉarguments(expression, Arguments, Calledˉfunction.Parameterˉtypes);
                return Callˉresult(
                    Wirˉoperation.Callˉfunction,
                    Calledˉfunction.Returnˉtype,
                    Arguments,
                    expression.Name);
            }

            if (Capabilities.TryGetValue(expression.Name, out var Capability))
            {
                Checkˉarguments(expression, Arguments, Capability.Parameterˉtypes);
                return Callˉresult(
                    Wirˉoperation.Callˉcapability,
                    Capability.Returnˉtype,
                    Arguments,
                    expression.Name);
            }

            if (Capabilityˉcatalog.Tryˉget(expression.Name, out _))
            {
                Report(
                    "WVC2064",
                    expression.Span,
                    $"Capability '{expression.Name}' must be declared by the module before it is called.");
            }
            else
            {
                Report("WVC2065", expression.Span, $"Function or capability '{expression.Name}' is not declared.");
            }

            return Invalidˉvalue(expression.Span);
        }

        private Boundˉvalue Compileˉrecord(Recordˉexpressionˉsyntax expression)
        {
            if (!Records.TryGetValue(expression.Name, out var Record))
            {
                foreach (var Field in expression.Fields)
                {
                    _ = Compileˉexpression(Field.Value);
                }
                Report("WVC2107", expression.Span, $"Record '{expression.Name}' is not declared.");
                return Invalidˉvalue(expression.Span);
            }

            var Temporaries = new int[Record.Declaration.Fields.Length];
            var Seen = new bool[Record.Declaration.Fields.Length];
            foreach (var Field in expression.Fields)
            {
                var Value = Compileˉexpression(Field.Value);
                var Fieldˉindex = -1;
                for (var Index = 0; Index < Record.Declaration.Fields.Length; Index++)
                {
                    if (StringComparer.Ordinal.Equals(
                            Record.Declaration.Fields[Index].Name,
                            Field.Name.Text))
                    {
                        Fieldˉindex = Index;
                        break;
                    }
                }

                if (Fieldˉindex < 0)
                {
                    Report(
                        "WVC2108",
                        Field.Name.Span,
                        $"Record '{expression.Name}' has no field '{Field.Name.Text}'.");
                    continue;
                }
                if (Seen[Fieldˉindex])
                {
                    Report(
                        "WVC2109",
                        Field.Name.Span,
                        $"Record field '{Field.Name.Text}' is initialized more than once.");
                    continue;
                }

                Seen[Fieldˉindex] = true;
                Temporaries[Fieldˉindex] = Value.Temporary;
                Requireˉtype(
                    Value,
                    Record.Declaration.Fields[Fieldˉindex].Type,
                    Field.Value.Span,
                    $"record field '{Field.Name.Text}'");
            }

            for (var Index = 0; Index < Seen.Length; Index++)
            {
                if (!Seen[Index])
                {
                    Report(
                        "WVC2110",
                        expression.Span,
                        $"Record field '{Record.Declaration.Fields[Index].Name}' is missing from '{expression.Name}'.");
                }
            }

            return Result(
                Wirˉoperation.Recordˉcreate,
                Valueˉshape.Forˉrecord(Record.Index),
                Temporaries.ToImmutableArray(),
                unsignedˉintegerˉoperand: (uint)Record.Index);
        }

        private Boundˉvalue Compileˉfield(Fieldˉexpressionˉsyntax expression)
        {
            if (!Tryˉlookupˉlocal(expression.Target, out var Local))
            {
                if (!Enums.TryGetValue(expression.Target, out var Enum))
                {
                    Report("WVC2086", expression.Span, $"'{expression.Target}' is not a record value or enum type.");
                    return Invalidˉvalue(expression.Span);
                }

                for (var Index = 0; Index < Enum.Declaration.Members.Length; Index++)
                {
                    if (StringComparer.Ordinal.Equals(Enum.Declaration.Members[Index].Name, expression.Field))
                    {
                        return Result(
                            Wirˉoperation.Enumˉconstant,
                            Valueˉshape.Forˉenum(Enum.Index),
                            unsignedˉintegerˉoperand: (uint)Enum.Index,
                            secondˉunsignedˉintegerˉoperand: (uint)Index);
                    }
                }

                Report("WVC2097", expression.Span, $"Enum '{Enum.Name}' has no member '{expression.Field}'.");
                return Invalidˉvalue(expression.Span);
            }

            var Target = Result(
                Wirˉoperation.Loadˉlocal,
                Local.Type,
                integerˉoperand: Local.Slot);
            if (Target.Type.Kind != Valueˉtype.Record)
            {
                Report("WVC2086", expression.Span, $"'{expression.Target}' is not a record value.");
                return Invalidˉvalue(expression.Span);
            }

            var Record = Records.Values.Single(Item => Item.Index == Target.Type.Nominalˉtypeˉindex);
            var Fieldˉindex = -1;
            for (var Index = 0; Index < Record.Declaration.Fields.Length; Index++)
            {
                if (StringComparer.Ordinal.Equals(Record.Declaration.Fields[Index].Name, expression.Field))
                {
                    Fieldˉindex = Index;
                    break;
                }
            }

            if (Fieldˉindex < 0)
            {
                Report("WVC2087", expression.Span, $"Record '{Record.Name}' has no field '{expression.Field}'.");
                return Invalidˉvalue(expression.Span);
            }

            return Result(
                Wirˉoperation.Recordˉfield,
                Record.Declaration.Fields[Fieldˉindex].Type,
                [Target.Temporary],
                unsignedˉintegerˉoperand: (uint)Fieldˉindex);
        }

        private Boundˉvalue Compileˉenumˉname(Callˉexpressionˉsyntax expression)
        {
            var Arguments = expression.Arguments.Select(Compileˉexpression).ToImmutableArray();
            if (Arguments.Length != 1)
            {
                Report(
                    "WVC2067",
                    expression.Span,
                    $"Call to '{expression.Name}' has {Arguments.Length} arguments; 1 is required.");
                return Invalidˉvalue(expression.Span);
            }

            if (Arguments[0].Type.Kind != Valueˉtype.Enum)
            {
                Report("WVC2098", expression.Arguments[0].Span, "Enumˉname requires an enum value.");
                return Invalidˉvalue(expression.Span);
            }

            return Result(
                Wirˉoperation.Enumˉname,
                Valueˉtype.Text,
                [Arguments[0].Temporary]);
        }

        private Boundˉvalue Compileˉfoundationˉintrinsic(
            Callˉexpressionˉsyntax expression,
            Foundationˉintrinsicˉdeclaration intrinsic)
        {
            var Arguments = expression.Arguments.Select(Compileˉexpression).ToImmutableArray();
            Checkˉarguments(expression, Arguments, intrinsic.Parameterˉtypes);
            return Result(
                intrinsic.Operation,
                intrinsic.Returnˉtype,
                Arguments.Select(Argument => Argument.Temporary).ToImmutableArray());
        }

        private Boundˉvalue Compileˉlength(Callˉexpressionˉsyntax expression)
        {
            if (expression.Arguments.Length == 1 &&
                expression.Arguments[0] is Nameˉexpressionˉsyntax Name &&
                Data.TryGetValue(Name.Name, out var Declaration) &&
                Declaration is I32ˉarrayˉdataˉdeclaration)
            {
                return Result(
                    Wirˉoperation.Dataˉlength,
                    Valueˉtype.I32,
                    nameˉoperand: Name.Name);
            }

            if (expression.Arguments.Length == 1)
            {
                var Sequence = Compileˉexpression(expression.Arguments[0]);
                if (Sequence.Type.Kind == Valueˉtype.Sequence)
                {
                    return Result(
                        Wirˉoperation.Sequenceˉlength,
                        Valueˉtype.U32,
                        [Sequence.Temporary]);
                }
            }

            if (expression.Arguments.Length != 1)
            {
                Report(
                    "WVC2066",
                    expression.Span,
                    "length() requires one immutable [i32] data name or bounded sequence value.");
            }
            else
            {
                Report(
                    "WVC2066",
                    expression.Span,
                    "length() requires one immutable [i32] data name or bounded sequence value.");
            }
            return Invalidˉvalue(expression.Span);
        }

        private Boundˉvalue Callˉresult(
            Wirˉoperation operation,
            Valueˉshape returnˉtype,
            ImmutableArray<Boundˉvalue> arguments,
            string name)
        {
            var Operands = arguments.Select(Argument => Argument.Temporary).ToImmutableArray();
            if (returnˉtype == Valueˉtype.Void)
            {
                Emit(new(operation, null, Operands, Nameˉoperand: name));
                return Boundˉvalue.Void;
            }

            return Result(operation, returnˉtype, Operands, nameˉoperand: name);
        }

        private void Checkˉarguments(
            Callˉexpressionˉsyntax expression,
            ImmutableArray<Boundˉvalue> arguments,
            ImmutableArray<Valueˉshape> parameterˉtypes)
        {
            if (arguments.Length != parameterˉtypes.Length)
            {
                Report(
                    "WVC2067",
                    expression.Span,
                    $"Call to '{expression.Name}' has {arguments.Length} arguments; {parameterˉtypes.Length} are required.");
            }

            var Count = Math.Min(arguments.Length, parameterˉtypes.Length);
            for (var Index = 0; Index < Count; Index++)
            {
                Requireˉtype(
                    arguments[Index],
                    parameterˉtypes[Index],
                    expression.Arguments[Index].Span,
                    $"argument {Index + 1}");
            }
        }

        private void Checkˉarguments(
            Callˉexpressionˉsyntax expression,
            ImmutableArray<Boundˉvalue> arguments,
            ImmutableArray<Valueˉtype> parameterˉtypes)
        {
            Checkˉarguments(
                expression,
                arguments,
                [.. parameterˉtypes.Select(Type => (Valueˉshape)Type)]);
        }

        private Boundˉvalue Invalidˉvalue(Sourceˉspan span)
        {
            _ = span;
            return Result(Wirˉoperation.I32ˉconstant, Valueˉtype.I32, integerˉoperand: 0);
        }

        private Boundˉvalue Result(
            Wirˉoperation operation,
            Valueˉshape type,
            ImmutableArray<int> operands = default,
            int integerˉoperand = 0,
            uint unsignedˉintegerˉoperand = 0,
            uint secondˉunsignedˉintegerˉoperand = 0,
            long wideˉintegerˉoperand = 0,
            ulong unsignedˉwideˉintegerˉoperand = 0,
            string? nameˉoperand = null)
        {
            var Temporary = Emitˉresult(
                operation,
                type,
                operands.IsDefault ? [] : operands,
                integerˉoperand,
                unsignedˉintegerˉoperand,
                secondˉunsignedˉintegerˉoperand,
                wideˉintegerˉoperand,
                unsignedˉwideˉintegerˉoperand,
                nameˉoperand);
            return new(type, Temporary);
        }

        private int Emitˉresult(
            Wirˉoperation operation,
            Valueˉshape type,
            ImmutableArray<int> operands = default,
            int integerˉoperand = 0,
            uint unsignedˉintegerˉoperand = 0,
            uint secondˉunsignedˉintegerˉoperand = 0,
            long wideˉintegerˉoperand = 0,
            ulong unsignedˉwideˉintegerˉoperand = 0,
            string? nameˉoperand = null)
        {
            var Temporary = Temporaryˉtypes.Count;
            Temporaryˉtypes.Add(type);
            Emit(new(
                operation,
                Temporary,
                operands.IsDefault ? [] : operands,
                Integerˉoperand: integerˉoperand,
                Unsignedˉintegerˉoperand: unsignedˉintegerˉoperand,
                Secondˉunsignedˉintegerˉoperand: secondˉunsignedˉintegerˉoperand,
                Wideˉintegerˉoperand: wideˉintegerˉoperand,
                Unsignedˉwideˉintegerˉoperand: unsignedˉwideˉintegerˉoperand,
                Nameˉoperand: nameˉoperand));
            return Temporary;
        }

        private void Emit(Wirˉinstruction instruction)
        {
            Requireˉcurrentˉblock().Instructions.Add(instruction);
        }

        private void Requireˉtype(
            Boundˉvalue value,
            Valueˉshape required,
            Sourceˉspan span,
            string role)
        {
            if (value.Type != required)
            {
                Report(
                    "WVC2070",
                    span,
                    $"The {role} has type {Formatˉtype(value.Type)}; {Formatˉtype(required)} is required.");
            }
        }

        private bool Tryˉlookupˉlocal(string name, out Localˉsymbol local)
        {
            foreach (var Scope in Scopes)
            {
                if (Scope.TryGetValue(name, out local!))
                {
                    return true;
                }
            }

            local = null!;
            return false;
        }

        private void Enterˉscope()
        {
            Scopes.Push(new(StringComparer.Ordinal));
        }

        private void Exitˉscope()
        {
            Scopes.Pop();
        }

        private Mutableˉblock Createˉblock()
        {
            var Block = new Mutableˉblock(Blocks.Count);
            Blocks.Add(Block);
            return Block;
        }

        private Mutableˉblock Requireˉcurrentˉblock()
        {
            return Currentˉblock ?? throw new InvalidOperationException(
                $"Function '{Function.Name}' has no current WIR block.");
        }

        private static uint Encodeˉcollectionˉelement(Valueˉshape element)
        {
            return element.Kind switch
            {
                Valueˉtype.Record => 0x4000_0000u | checked((uint)element.Nominalˉtypeˉindex),
                Valueˉtype.Enum => 0x8000_0000u | checked((uint)element.Nominalˉtypeˉindex),
                Valueˉtype.Variant => 0xC000_0000u | checked((uint)element.Nominalˉtypeˉindex),
                _ => (uint)element.Kind,
            };
        }

        private Valueˉshape Bindˉvalueˉshape(Typeˉsyntax type)
        {
            if (type.Kind is Typeˉsyntaxˉkind.Sequence or Typeˉsyntaxˉkind.Builder)
            {
                var Element = type.Elementˉtype is null
                    ? (Valueˉshape)Valueˉtype.I32
                    : Bindˉvalueˉshape(type.Elementˉtype);
                if (Element.Kind is Valueˉtype.Void or Valueˉtype.Sequence or Valueˉtype.Builder)
                {
                    Report("WVC2150", type.Span, "A bounded collection requires one non-collection value element type.");
                    Element = Valueˉtype.I32;
                }
                var Maximum = type.Maximum;
                if (Maximum is 0 or > Bytecodeˉlimits.MAX_SEQUENCE_ELEMENTS)
                {
                    Report(
                        "WVC2150",
                        type.Span,
                        $"A bounded collection maximum must be 1 through {Bytecodeˉlimits.MAX_SEQUENCE_ELEMENTS}.");
                    Maximum = 1;
                }
                return type.Kind == Typeˉsyntaxˉkind.Sequence
                    ? Valueˉshape.Forˉsequence(Element, Maximum)
                    : Valueˉshape.Forˉbuilder(Element, Maximum);
            }

            if (type.Kind == Typeˉsyntaxˉkind.Named)
            {
                if (type.Name is not null && Records.TryGetValue(type.Name, out var Record))
                {
                    return Valueˉshape.Forˉrecord(Record.Index);
                }

                if (type.Name is not null && Enums.TryGetValue(type.Name, out var Enum))
                {
                    return Valueˉshape.Forˉenum(Enum.Index);
                }

                if (type.Name is not null && Variants.TryGetValue(type.Name, out var Variant))
                {
                    return Valueˉshape.Forˉvariant(Variant.Index);
                }

                Report("WVC2085", type.Span, $"Named type '{type.Name}' is not declared.");
                return Valueˉtype.I32;
            }

            return type.Kind switch
            {
                Typeˉsyntaxˉkind.Void => Valueˉtype.Void,
                Typeˉsyntaxˉkind.I32 => Valueˉtype.I32,
                Typeˉsyntaxˉkind.I64 => Valueˉtype.I64,
                Typeˉsyntaxˉkind.U8 => Valueˉtype.U8,
                Typeˉsyntaxˉkind.U32 => Valueˉtype.U32,
                Typeˉsyntaxˉkind.U64 => Valueˉtype.U64,
                Typeˉsyntaxˉkind.Bool => Valueˉtype.Bool,
                Typeˉsyntaxˉkind.Text => Valueˉtype.Text,
                Typeˉsyntaxˉkind.Bytes => Valueˉtype.Bytes,
                _ => Valueˉtype.I32,
            };
        }

        private void Report(string code, Sourceˉspan span, string message)
        {
            Diagnostics.Report(code, "semantic", span, message);
        }

        private string Formatˉtype(Valueˉshape type)
        {
            if (type.Kind == Valueˉtype.Record &&
                Records.Values.FirstOrDefault(Record => Record.Index == type.Nominalˉtypeˉindex) is { } Record)
            {
                return Record.Name;
            }

            if (type.Kind == Valueˉtype.Enum &&
                Enums.Values.FirstOrDefault(Enum => Enum.Index == type.Nominalˉtypeˉindex) is { } Enum)
            {
                return Enum.Name;
            }

            if (type.Kind == Valueˉtype.Variant &&
                Variants.Values.FirstOrDefault(Variant => Variant.Index == type.Nominalˉtypeˉindex) is { } Variant)
            {
                return Variant.Name;
            }

            return type.Kind switch
            {
                Valueˉtype.Void => "void",
                Valueˉtype.I32 => "i32",
                Valueˉtype.I64 => "i64",
                Valueˉtype.U8 => "u8",
                Valueˉtype.U32 => "u32",
                Valueˉtype.U64 => "u64",
                Valueˉtype.Bool => "bool",
                Valueˉtype.Text => "text",
                Valueˉtype.Bytes => "bytes",
                _ => type.ToString(),
            };
        }
    }
}
