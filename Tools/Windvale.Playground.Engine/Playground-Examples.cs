using System.Collections.Immutable;
using Windvale.Bytecode;

namespace Windvale.Playground;

public static class Playgroundˉexamples
{
    public static ImmutableArray<Playgroundˉexample> All { get; } =
    [
        new(
            "hello",
            "Hello, Windvale",
            "Compile a hosted module and write through one explicitly authorized capability.",
            """
            module Helloˉwindvale profile hosted;

            capability console.write_line;

            data Greeting: text = "Hello from Windvale";

            export fn Main() -> i32 {
                console.write_line(Greeting);
                return 0;
            }
            """,
            Capabilities(Capabilityˉcatalog.CONSOLE_WRITE_LINE)),
        new(
            "webassembly-worker",
            "WebAssembly worker",
            "Lower canonical WVB with the Windvale-authored backend, execute it in a disposable Web Worker, and compare both runtimes.",
            """
            module WebAssemblyˉstraightˉi32 profile portable;

            export fn Main() -> i32 {
                20 + 22;
                return (20 + 22) * 2 - 42;
            }
            """,
            Capabilities()),
        new(
            "sum-data",
            "Sum data",
            "Run a portable module with loops, functions, and immutable module data.",
            """
            module Sumˉdata profile portable;

            data Values: [i32] = [3, 5, 8, 13];

            fn Add(Left: i32, Right: i32) -> i32 {
                return Left + Right;
            }

            export fn Main() -> i32 {
                var Index: i32 = 0;
                var Total: i32 = 0;

                while Index < length(Values) {
                    Total = Add(Total, Values[Index]);
                    Index = Index + 1;
                }

                return Total;
            }
            """,
            Capabilities()),
        new(
            "two-channels",
            "Two output channels",
            "Keep ordinary program output separate from explicitly authorized diagnostic evidence.",
            """
            module Twoˉchannels profile hosted;

            capability console.write_line;
            capability diagnostic.write_line;

            export fn Main() -> i32 {
                console.write_line("Build complete");
                diagnostic.write_line("verified: canonical WVB");
                return 0;
            }
            """,
            Capabilities(
                Capabilityˉcatalog.CONSOLE_WRITE_LINE,
                Capabilityˉcatalog.DIAGNOSTIC_WRITE_LINE)),
        new(
            "records-enums",
            "Records and enums",
            "Construct nominal values and return a field selected through an enum comparison.",
            """
            module Recordsˉandˉenums profile portable;

            enum Deliveryˉstate {
                Waiting = 0;
                Ready = 1;
            }

            record Deliveryˉresult {
                Value: i32;
                State: Deliveryˉstate;
            }

            fn Makeˉresult() -> Deliveryˉresult {
                return Deliveryˉresult(42, Deliveryˉstate.Ready);
            }

            export fn Main() -> i32 {
                let Result: Deliveryˉresult = Makeˉresult();
                if Result.State == Deliveryˉstate.Ready {
                    return Result.Value;
                }
                return 0;
            }
            """,
            Capabilities()),
        new(
            "text-formatting",
            "Text and formatting",
            "Build readable text from enum names and numbers, then write it through an authorized capability.",
            """
            module Textˉandˉformatting profile hosted;

            capability console.write_line;

            enum Buildˉstate {
                Waiting = 0;
                Running = 1;
                Complete = 2;
            }

            fn Describeˉbuild(State: Buildˉstate, Number: u32) -> text {
                let Stateˉname: text = Enumˉname(State);
                let Buildˉnumber: text = U32ˉformat(Number);
                let Prefix: text = Textˉconcat("Windvale: ", Stateˉname);
                return Textˉconcat(Prefix, Textˉconcat(", build ", Buildˉnumber));
            }

            export fn Main() -> i32 {
                console.write_line(Describeˉbuild(Buildˉstate.Running, 42u32));
                return 0;
            }
            """,
            Capabilities(Capabilityˉcatalog.CONSOLE_WRITE_LINE)),
        new(
            "unicode-round-trip",
            "Unicode round trip",
            "Encode Unicode text as strict UTF-8 bytes, validate it, decode it, and report its byte length.",
            """
            module Unicodeˉroundˉtrip profile hosted;

            capability console.write_line;

            data Greeting: text = "Hello, 世界";

            export fn Main() -> i32 {
                let Encoded: bytes = Textˉtoˉutf8(Greeting);
                if !Textˉutf8ˉisˉvalid(Encoded) {
                    return 1;
                }

                let Recovered: text = Textˉfromˉutf8(Encoded);
                let Byteˉcount: text = U32ˉformat(Bytesˉlength(Encoded));
                console.write_line(Recovered);
                console.write_line(Textˉconcat("UTF-8 bytes: ", Byteˉcount));
                return 0;
            }
            """,
            Capabilities(Capabilityˉcatalog.CONSOLE_WRITE_LINE)),
        new(
            "inspect-bytes",
            "Inspect binary data",
            "Slice immutable bytes, read little-endian fields, and calculate a stable SHA-256 digest.",
            """
            module Inspectˉbinaryˉdata profile hosted;

            capability console.write_line;

            data Header: bytes = [87, 86, 66, 49, 1, 0, 7, 0, 0, 0];

            export fn Main() -> i32 {
                let Magic: bytes = Bytesˉslice(Header, 0u32, 4u32);
                let Version: u32 = Bytesˉreadˉu16ˉlittle(Header, 4u32);
                let Sectionˉcount: u32 = Bytesˉreadˉu32ˉlittle(Header, 6u32);
                let Summary: text = Textˉconcat(
                    Textˉconcat("version=", U32ˉformat(Version)),
                    Textˉconcat(", sections=", U32ˉformat(Sectionˉcount)));

                console.write_line(Summary);
                console.write_line(Textˉconcat("magic sha256=", Bytesˉsha256ˉhex(Magic)));

                if Bytesˉlength(Header) == 10u32 {
                    return 0;
                }
                return 1;
            }
            """,
            Capabilities(Capabilityˉcatalog.CONSOLE_WRITE_LINE)),
        new(
            "instruction-budget",
            "Instruction budget",
            "See the runtime stop a valid program when it exhausts the playground instruction budget.",
            """
            module Instructionˉbudget profile portable;

            export fn Main() -> i32 {
                var Counter: i32 = 0;
                while Counter >= 0 {
                    Counter = Counter + 1;
                }
                return Counter;
            }
            """,
            Capabilities()),
    ];

    private static ImmutableHashSet<string> Capabilities(params string[] names) =>
        names.ToImmutableHashSet(StringComparer.Ordinal);
}
