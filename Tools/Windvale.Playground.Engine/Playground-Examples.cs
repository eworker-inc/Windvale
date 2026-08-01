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
