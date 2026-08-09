const HELLO_SOURCE = `module Helloˉwindvale profile hosted;

capability console.write_line;

data Greeting: text = "Hello from Windvale";

export fn Main() -> i32 {
    console.write_line(Greeting);
    return 0;
}
`;
const EXACT_SOURCE = `module WebAssemblyˉcompilerˉsuccess profile portable;

export fn Main() -> i32 {
    return 42;
}
`;
const ARITHMETIC_SOURCE = `module Arithmetic profile portable;

export fn Main() -> i32 {
    return (20 + 22) * 2 - 42;
}
`;
const FACTORIAL_SOURCE = `module Factorial profile portable;

fn Factorial(Value: i32) -> i32 {
    var Current: i32 = 2;
    var Result: i32 = 1;
    while Current <= Value {
        Result = Result * Current;
        Current = Current + 1;
    }
    return Result;
}

export fn Main() -> i32 {
    return Factorial(6);
}
`;
const SUM_DATA_SOURCE = `module Sumˉdata profile portable;

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
`;
const RECORDS_AND_ENUMS_SOURCE = `module Recordsˉandˉenums profile portable;

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
`;

export const SCRATCH_SOURCE = `module Scratch profile portable;

export fn Main() -> i32 {
    return 0;
}
`;

export const EXAMPLES = Object.freeze([
    Object.freeze({
        Id: "hello-windvale",
        Title: "Hello from Windvale",
        Fileˉname: "Hello-Windvale.wv",
        Description: "Write one bounded line through an explicitly authorized browser console capability.",
        Authorizeˉconsoleˉwriteˉline: true,
        Source: HELLO_SOURCE,
    }),
    Object.freeze({
        Id: "windvale-starter",
        Title: "Windvale WebAssembly starter",
        Fileˉname: "WebAssembly-Starter.wv",
        Description: "A small Windvale program compiled to canonical WVB, independently admitted, and run entirely in browser WebAssembly.",
        Authorizeˉconsoleˉwriteˉline: false,
        Source: EXACT_SOURCE,
    }),
    Object.freeze({
        Id: "arithmetic",
        Title: "Integer arithmetic",
        Fileˉname: "Arithmetic.wv",
        Description: "Evaluate a portable integer expression and return 42.",
        Authorizeˉconsoleˉwriteˉline: false,
        Source: ARITHMETIC_SOURCE,
    }),
    Object.freeze({
        Id: "factorial",
        Title: "Functions and loops",
        Fileˉname: "Factorial.wv",
        Description: "Call an internal function, update mutable locals in a loop, and return 720.",
        Authorizeˉconsoleˉwriteˉline: false,
        Source: FACTORIAL_SOURCE,
    }),
    Object.freeze({
        Id: "sum-data",
        Title: "Module data",
        Fileˉname: "Sum-Data.wv",
        Description: "Read immutable module data through indexing and length, sum it through an internal function, and return 29.",
        Authorizeˉconsoleˉwriteˉline: false,
        Source: SUM_DATA_SOURCE,
    }),
    Object.freeze({
        Id: "records-enums",
        Title: "Records and enums",
        Fileˉname: "Records-And-Enums.wv",
        Description: "Construct nominal values, compare an enum member, read a record field, and return 42.",
        Authorizeˉconsoleˉwriteˉline: false,
        Source: RECORDS_AND_ENUMS_SOURCE,
    }),
]);
