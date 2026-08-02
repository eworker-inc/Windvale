using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Windvale.Assembler;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.ObjectModel;
using Windvale.Playground;
using Windvale.Project;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static class Program
{
    private const string SUM_SHA256 = "6f3a272d37dd8893995c7f85c236414ed2864bf59de2f3775c08afd426013f8c";
    private const string HELLO_SHA256 = "bcf6597a27384661d2796f1dd8ee6e24cce8e6c7cb84def3b7826a564acb7d54";
    private const string FOUNDATION_SHA256 = "72ae31559bb3335b320328c26e70518b6a0f3e617d099d41b328b066bb3784c7";
    private const string WVDUMP_CORE_SHA256 = "f11dcff36bb5e686d5841be69cf03838da240da64de672123ca0a6f9db9c102a";
    private const string WVO_SAMPLE_SHA256 = "006fd80183da7fbc71d3c6d63b65e6f3551765508fe9dba6f38ba80e002eb28a";
    private const string WVO_CORE_SHA256 = "e35939e46ca63f6c284ae457be12de23bb6bc8cb28fac52ce76c833d5fe6bb74";
    private const string WVA_OBJECT_SHA256 = "992c298a4f9b68dec27b7203a2770f2a37ef2016ea45e88d33ee21994060fe85";
    private const string WVA_ASSEMBLER_CORE_SHA256 = "442ad834282d50b5c63d04aafae02a0de4db4b44a1c3c5101623d1e19ce0218e";
    private const string WVLINK_CORE_SHA256 = "091383174f0ca6e535881f31949c65d46542f8b452905f0a82c713707cada1aa";
    private const string LINK_IMAGE_SHA256 = "0e02d447ec379e8bc8be373694d6ca14fdde0125550cbd34ee05b3ecc63ffe9a";
    private const string LINK_MAP_SHA256 = "31bc6a8e90d5f3049ae3e2eb0735a901923186d6a03ed40f22762b557b2ba5f4";
    private const string NATIVE_CONSTANT_CODE_SHA256 = "7c05565142850adab1d63d999479977a23ef50c7264c03ee55ce5b323df26408";
    private const string NATIVE_CONSTANT_WVO_SHA256 = "0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5";
    private const string NATIVE_ARITHMETIC_CODE_SHA256 = "0215fb8a41dfb1f01f670149583371cb512c68bd301e2c2908a28aef47594f7c";
    private const string NATIVE_ARITHMETIC_WVO_SHA256 = "d9ac70a601afdf2fb2efb1bf8b3d958532c2efa8991fb4b9ef3f066fab63331d";
    private const string NATIVE_CONTROL_CODE_SHA256 = "3ba822ee8c1b664bf72501f81b288fc4930db68df1d1f167270d5aa714ed6d62";
    private const string NATIVE_CONTROL_WVO_SHA256 = "4cf925098870f7e1aa9ae3d50f30211d69287ac486a5169dc685ae3fcf18417e";
    private const string NATIVE_LOOP_CODE_SHA256 = "470542f262ebb288c72b306cf73807f1922c9c1cf089ecfc8dbba6c810435fe8";
    private const string NATIVE_LOOP_WVO_SHA256 = "1771bcb36ce897dab2184b28a93a93d3d1116e948997ee551920c94c2a52e9e6";
    private const string NATIVE_STENCIL_CORE_SHA256 = "d40fc83c3288043c7af80a261e351066bf3507913b34371a9839014b51ed4b2f";
    private const string NATIVE_STENCIL_BRIDGE_SHA256 = "5e1c6c360d93ac54c9281adb0f27b53c77937cf78027e80a9d3fc177877ae7e9";
    private const string NATIVE_STENCIL_DEMO_SHA256 = "651d9435c2b11b4f102a086615bdd159eb981096e2a2324027d5f86a29e36a15";
    private const string NATIVE_PUBLICATION_CORE_SHA256 = "b25fa550518caa4ef43c7ae886cce328148777782f70e3faa25ac19821b6d439";
    private const string NATIVE_PUBLICATION_BRIDGE_SHA256 = "750b6134395c46c9e1c703ae2a56449bd1710f517e516397e10a1ccc951c503e";
    private const string NATIVE_PUBLICATION_LIFETIME_CORE_SHA256 = "52b1cb6dd0d7fa9d17c1cba50b527912876e4acf1cd9663846ce915b4c56aed5";
    private const string NATIVE_PUBLICATION_LIFETIME_BRIDGE_SHA256 = "74dfaf40bb6ea83f0fd72757c9c4cb85f5c8dd28a41f3993325871d348e88d32";
    private const string SOURCE_COMPOSITION_SHA256 = "0980b7178943be516cd9b6924f179d5977ca147e11bf105c5063ea078c645b60";
    private const string PROJECT_MANIFEST_CORE_SHA256 = "b609fb7d442bbe1685c1058c71eb011d43b291df505697a97c233ca7063a2044";
    private const string PROJECT_MANIFEST_TOOL_SHA256 = "50ab9aa5048ab844a816d0f7f12fb691cb69f57c4a71f7eb18ebc7fb4aaf0b0c";
    private const string PROJECT_MANIFEST_NATIVE_CODE_SHA256 = "78cc236076bca41a80539653de093a9cfea5dd2e5eb0fd6403675332b9d6d78c";
    private const string PROJECT_MANIFEST_NATIVE_WVO_SHA256 = "d667d2e7657670a1fd27fd2fa08639c020c853f3cfc9bf7d99e3d023e31a55a2";
    private const string MACHINE_CONTRACTS_SHA256 = "9f909a4c47d6f7fb41570b58615a533e79e0219a780c686a64995826b322219a";
    private const string MACHINE_CONTRACTS_DEMO_SHA256 = "b505d3335fa5a4b1dabe2d5e64e4c7a557e0028666cbebe1e2557a0255772f1a";
    private const string BYTE_ORDERING_SHA256 = "194e4b5c4eb7f4641a39098abce3dabb93187af7149e184b56b76f978ed2f4f1";
    private const string BYTE_ORDERING_DEMO_SHA256 = "0b41e8f615630e0734812ba8cd8e7c06e975592b86327c2fe8220f5e29c10cab";
    private const string DECIMAL_PARSING_SHA256 = "39f6c1c3d5a2233d5296e777e798450571c5f4ba837120a25a6487bf8014ee1f";
    private const string DECIMAL_PARSING_DEMO_SHA256 = "16a20ee595eb708095f6e8c38c809a24774989110780dbefbacbc36ee468e695";
    private const string BYTE_CONSTRUCTION_SHA256 = "6f26865069333c02b15ab83d48f2a0cb0e3a05db98bcd841f31e232485b76207";
    private const string BYTE_CONSTRUCTION_DEMO_SHA256 = "a9b577dc08ac6e4a0d786f04d6667eb0347c57a0c1abbd81f3481fb0e0bc6c29";
    private const string SOURCE_LEXER_SHA256 = "ca91d5aa9889540250be552b5563dacba8deba2abb70ea557d0e4f8089ee749f";
    private const string SOURCE_LEXER_DEMO_SHA256 = "2a7a2f8c1276c252fa8ddb53a362c6560dfa06ba8c2a8be0fb56f507e820df87";
    private const string SOURCE_DECLARATION_PARSER_SHA256 = "4bbaaaa6293ab1fb5a4eb92c3e8a52c078943ba88652b27f69fdc3c5ab76fda7";
    private const string SOURCE_DECLARATION_PARSER_DEMO_SHA256 = "ab28936fe0961261a0f243009d5c9b93af52069326618e03e428d1cc024fea11";
    private const string SOURCE_DECLARATION_PARSER_TOOL_SHA256 = "94134e28bef9544b0fbb4b4ae6dfd3deb3aa52598475023d37b01a5de8686d45";
    private const string SOURCE_BODY_PARSER_SHA256 = "3df42c7b6e81343194340b8f6f44e44fb83f3d6f18c249c9d9ed4e58df69ec73";
    private const string SOURCE_BODY_PARSER_DEMO_SHA256 = "afa07f843679e89f84a5a55887af834575d43d4a3ac3f1a76cd4395a103e62b6";
    private const string SOURCE_BODY_PARSER_TOOL_SHA256 = "342fadc0886e5b8b2910cb65c8495730a902364a526fd34df58c574a32a91890";
    private const string SOURCE_SET_SHA256 = "bb671df781acb049c513f9504abf00069a3fff1cdb9affb8706340b9e02fefda";
    private const string SOURCE_SET_DEMO_SHA256 = "5b334b0ead653bc043e244a60e2e36bc32d66aa0211f715329434c0447a539c9";
    private const string SOURCE_SET_TOOL_SHA256 = "3ecb611599ee51799799ead54288259569a1b0a092d24c216caa703b578d55e4";
    private const string SOURCE_GRAPH_SHA256 = "5d266d834c5cde77efa4046dfc9c8a8c0eed7c2df1dd254f98c4e338d76cccda";
    private const string SOURCE_GRAPH_DEMO_SHA256 = "2d2fa7ae2cca012834fb340253a551f9332a764200bb8f6449158b8dad4b30b2";
    private const string SOURCE_GRAPH_TOOL_SHA256 = "4558a7c6ba1c1632bb2d46747d31dfd1be0480e93fc4eec8340d0ea39db702f1";
    private const string SOURCE_SYMBOLS_SHA256 = "7769def20aef89bac982d896a5fa791f7ae3cea744b70fc199583ed97aef40e4";
    private const string SOURCE_SYMBOLS_DEMO_SHA256 = "6ce17cdcd140cd686c0975e30a9be173d09deff4eb8d4dbb8d972ed1b8440158";
    private const string SOURCE_SYMBOLS_TOOL_SHA256 = "0d09379e35df8af7d3239badc4a50a71ecb4638255f28f3a40421491d35a6529";
    private const string SOURCE_BINDINGS_SHA256 = "4e1e5d7f0029d15abaaadc3c2d84d966db6b33bb7b11c8b222bc5336b32cdae6";
    private const string SOURCE_BINDINGS_DEMO_SHA256 = "261d3497883a1920ebc523b252b91b1ee5efbbd4a5ad7a0d81255e3908959014";
    private const string SOURCE_BINDINGS_TOOL_SHA256 = "b5c0663ba414a913f0e64e611a8619ecb0dc95c763dfb2c2d12e02f65919e0b3";
    private const string SOURCE_WIR_SHA256 = "959a9341668215bd748d5a04946ff5a598c443dd788b551b9062fe47a5d7bca8";
    private const string SOURCE_WIR_DEMO_SHA256 = "a32ae736936f459a33e0e9733593926b8d4f345d7f399310adb61b7e136f142d";
    private const string SOURCE_WIR_TOOL_SHA256 = "8da075794db7227c8e89b48885a227d501a3ca03b2de7a186c27c97100060b4f";
    private const string SOURCE_WVB_SHA256 = "9c3f4f6839274766a3633784716147e03e3bce47ec1103dac0eb0d998a1b4b9a";
    private const string SOURCE_WVB_DEMO_SHA256 = "acf1f5cbde6e2ba3d831ed8390dac85f812d13525847619b3c85903bb7a44c8f";
    private const string SOURCE_WVB_TOOL_SHA256 = "9673bf3331763181f443ec67b7a513bc66daa718969f7f6b0d197a4186071066";
    private const string SOURCE_WVB_DATA_AND_TEXT_SHA256 = "5d0779925bee06b8e27afb5ccedd995fc83cbd6aa71954911a644cf078c71704";
    private const string SOURCE_WVB_NOMINAL_TYPES_SHA256 = "1366b543a28a1921aca6198bca9eaaf5eeeb97766405d5efcdeff9d27cfca57a";
    private const string SOURCE_WVB_HOSTED_CAPABILITIES_SHA256 = "1df4503a21abf5f2c0b0307ac2dc79402bc8550ec5e4a016df43fdeb8197d528";
    private const string SOURCE_WVB_COMPOSITION_SHA256 = "7279011a12f3d2becc1e9775fb92bd7c74b8760b2c94f13a282d71c0849f8e6f";
    private const string WEBASSEMBLY_CORE_SHA256 = "18d8f2a32c7ee6ff0a89ac705663595dc611bf7ffd545f76662e1227085bbc34";
    private const string WEBASSEMBLY_TOOL_SHA256 = "b47a6f5b89ac0d58dc6cafd6489b1fb12f1a0b9b161c09e8d2ca5a438993076a";
    private const string WEBASSEMBLY_DEMO_SHA256 = "cb6b5fbf378a4b13387704dda87beb75d6023112afeabfbaa558cf8fa32f5fe1";
    private const string WEBASSEMBLY_CONSTANT_WVB_SHA256 = "da24fd4b2d7a0859d0262f4e79e31d9733bf58092730ee7f69d1992a21e3110f";
    private const string WEBASSEMBLY_CONSTANT_SHA256 = "1b62162dbc97b579c02834e9623e3ac9eccc7bc444e4b48a9e4d6c39b77ea3f1";
    private const string WEBASSEMBLY_CHECKED_ADD_WVB_SHA256 = "54fccbb837dc47dad0f40dca1356d046dd9beb6dab13a3a2574b867791e10466";
    private const string WEBASSEMBLY_CHECKED_ADD_SHA256 = "4057797732dd7250413f44aa71e012222591ae7e219e27a7680f246b2cedeb8a";
    private const string WEBASSEMBLY_CHECKED_ADD_HEX = "0061736D010000000105016000017F030201000610037F0041010B7F0141000B7F0141000B0749040C57696E6476616C652E72756E00000C57696E6476616C652E61626903000F57696E6476616C652E726573756C7403011557696E6476616C652E696E737472756374696F6E7303020A3E013C01017F410024014100240241F8FFFFFF0741076A220041F8FFFFFF077320004107737141004804404107240241BF170F0B20002401410A240241000B";
    private const string WEBASSEMBLY_CHECKED_ADD_OVERFLOW_WVB_SHA256 = "fbba878513eabf1d8c47fdbab887f314117a8ee5184c42a23edc94190926a583";
    private const string WEBASSEMBLY_CHECKED_ADD_OVERFLOW_SHA256 = "984139ccb136981e4d6382e4c547012be13df38af056cd09abebec10cc1a6f52";
    private const string WEBASSEMBLY_STRAIGHT_I32_WVB_SHA256 = "f7d360cf4d717d2cce93eda4f2c814960c39f1dd04bd0f74c44f55066730d655";
    private const string WEBASSEMBLY_STRAIGHT_I32_SHA256 = "15f2d58746ff2b0ae33a0de05e2781949c9d908fab46dd4072bfe3b2fa42b0bb";
    private const string WEBASSEMBLY_SUBTRACT_OVERFLOW_WVB_SHA256 = "d1994cfa17dd4b7ccc133d77c60f39b6d7aa5d7e250c415d511c725e971b4725";
    private const string WEBASSEMBLY_SUBTRACT_OVERFLOW_SHA256 = "757d26c2cf404cabcf5b78d2c998bc7ddc78ec4531e4571630ae2c1b5c8d7925";
    private const string WEBASSEMBLY_MULTIPLY_OVERFLOW_WVB_SHA256 = "bb21f58144ffdefe31bb0bbf8ea5c2d7ca6c9b2321b255d42201d5313a608587";
    private const string WEBASSEMBLY_MULTIPLY_OVERFLOW_SHA256 = "e924c7507a363a7b019935622abfbd4bf4ac8445cd37a0412130ce8e5c83d51a";
    private const string WEBASSEMBLY_NEGATE_OVERFLOW_WVB_SHA256 = "bf617ef07f7c3e43ba33d21c8f18eab07658ea0f40153bf8c3bef80f7db7ec98";
    private const string WEBASSEMBLY_NEGATE_OVERFLOW_SHA256 = "3f098efd63c68d8c62a4f6b373507e12c21808ff01120d165c9dc85a047e99e2";

    private const string COMPLETE_ASSEMBLY_SOURCE = """
        windvale-assembly 1
        symbol local data Bss in .bss
        symbol local data Values in .data
        symbol export function Main in .text
        section code .text align 16
        define Main
        nop
        trap
        move_i32 edi -1
        move_u32 ecx 4294967295
        jump Main
        return
        end define
        end section
        section data .data align 4
        define Values
        bytes 1 255
        u32 2309737967
        i32 -2
        address_u32 Main
        end define
        end section
        section bss .bss align 16
        define Bss
        zero 16
        end define
        end section
        """;

    private const string KERNEL_MECHANICS_ASSEMBLY_SOURCE = """
        windvale-assembly 1
        symbol export function Main in .text
        section code .text align 16
        define Main
        push_i32 -1
        enable_page_protection
        activate_page_table
        syscall
        move_u32 edx 1540
        move_u32 eax 8192
        out_u16
        disable_interrupts
        halt
        jump Main
        end define
        end section
        """;

    private const string SUM_SOURCE = """
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
        """;

    private const string NATIVE_CONSTANT_SOURCE = """
        module Nativeˉconstant profile portable;

        export fn Main() -> i32 {
            return 42;
        }
        """;

    private const string NATIVE_ARITHMETIC_SOURCE = """
        module Nativeˉarithmetic profile portable;

        export fn Main() -> i32 {
            return -(((2 + 2) - (7 * 6)) - 4);
        }
        """;

    private const string NATIVE_CONTROL_SOURCE = """
        module Nativeˉcontrol profile portable;

        export fn Main() -> i32 {
            let Value: i32 = 6 * 7;
            let Equal: bool = Value == 42;
            let Accepted: bool = Equal == true;
            if Accepted != false {
                if Value != 41 {
                    if Value < 43 {
                        if Value <= 42 {
                            if Value > 41 {
                                if Value >= 42 {
                                    if !false { return Value; }
                                }
                            }
                        }
                    }
                }
            }
            return 0;
        }
        """;

    private const string NATIVE_NOMINAL_SOURCE = """
        module Nativeˉnominal profile portable;

        enum Nativeˉstate {
            Ready = 0;
            Complete = 7;
        }

        record Nativeˉresult {
            Value: i32;
            State: Nativeˉstate;
            Accepted: bool;
        }

        fn Make(Value: i32) -> Nativeˉresult {
            return Nativeˉresult(Value, Nativeˉstate.Ready, true);
        }

        fn Advance(Value: Nativeˉresult) -> Nativeˉresult {
            return Nativeˉresult(Value.Value + 1, Nativeˉstate.Complete, Value.Accepted);
        }

        fn Read(Value: Nativeˉresult) -> i32 {
            if Value.State == Nativeˉstate.Complete {
                if Value.State != Nativeˉstate.Ready {
                    if Value.Accepted { return Value.Value; }
                }
            }
            return 0;
        }

        export fn Main() -> i32 {
            let Result: Nativeˉresult = Advance(Make(41));
            return Read(Result);
        }
        """;

    private const string NATIVE_LOOP_SOURCE = """
        module Nativeˉloop profile portable;

        export fn Main() -> i32 {
            var Value: i32 = 0;
            while Value < 6 {
                Value = Value + 1;
            }
            return Value * 7;
        }
        """;

    private const string NATIVE_BYTES_SOURCE = """
        module Nativeˉbytes profile portable;

        data Packet: bytes = [42, 1, 0, 0, 0, 255, 255, 255, 255];

        fn Inspect(Input: bytes, Offset: u32) -> u32 {
            let Tail: bytes = Bytesˉslice(Input, Offset, 4u32);
            let Word: u32 = Bytesˉreadˉu32ˉlittle(Tail, 0u32);
            let First: u32 = U32ˉfromˉu8(Bytesˉreadˉu8(Input, 0u32));
            return Word + First;
        }

        export fn Main() -> i32 {
            let Sum: u32 = Inspect(Packet, 1u32);
            let First: u8 = Bytesˉreadˉu8(Packet, 0u32);
            let Short: u32 = Bytesˉreadˉu16ˉlittle(Packet, 1u32);
            let Negative: i32 = Bytesˉreadˉi32ˉlittle(Packet, 5u32);
            let Answer: u32 = (Sum - 1u32) * 1u32;
            let Encodedˉzero: bytes = Bytesˉfromˉu8(0u8);
            let Encodedˉmaximum: bytes = Bytesˉfromˉu8(255u8);
            let Encodedˉword: bytes = Bytesˉfromˉu32ˉlittle(42u32);
            if Bytesˉlength(Packet) == 9u32 {
                if First == 42u8 {
                    if First != 41u8 {
                        if Short == 1u32 {
                            if Negative == -1 {
                                if Answer != 41u32 {
                                    if Answer < 43u32 {
                                        if Answer <= 42u32 {
                                            if Answer > 41u32 {
                                                if Answer >= 42u32 {
                                                    if Bytesˉlength(Encodedˉzero) != 1u32 { return 0; }
                                                    if Bytesˉreadˉu8(Encodedˉzero, 0u32) != 0u8 { return 0; }
                                                    if Bytesˉlength(Encodedˉmaximum) != 1u32 { return 0; }
                                                    if Bytesˉreadˉu8(Encodedˉmaximum, 0u32) != 255u8 { return 0; }
                                                    if Bytesˉreadˉu32ˉlittle(Encodedˉword, 0u32) != 42u32 { return 0; }
                                                    return 42;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return 0;
        }
        """;

    private const string HELLO_SOURCE = """
        module Helloˉwindvale profile hosted;

        capability console.write_line;

        data Greeting: text = "Hello from Windvale";

        export fn Main() -> i32 {
            console.write_line(Greeting);
            return 0;
        }
        """;

    private const string FOUNDATION_SOURCE = """
        module Readˉwvbˉheader profile portable;

        data Moduleˉheader: bytes = [87, 86, 66, 49, 1, 0, 6, 0, 7, 0, 0, 0];

        fn Headerˉisˉvalid(Input: bytes) -> bool {
            if Bytesˉlength(Input) != 12u32 {
                return false;
            }

            let Magic: bytes = Bytesˉslice(Input, 0u32, 4u32);
            if Bytesˉreadˉu8(Magic, 0u32) != 87u8 {
                return false;
            }
            if Bytesˉreadˉu8(Magic, 1u32) != 86u8 {
                return false;
            }
            if Bytesˉreadˉu8(Magic, 2u32) != 66u8 {
                return false;
            }
            if Bytesˉreadˉu8(Magic, 3u32) != 49u8 {
                return false;
            }

            let Version: u32 = Bytesˉreadˉu16ˉlittle(Input, 4u32);
            let Minorˉversion: u32 = Bytesˉreadˉu16ˉlittle(Input, 6u32);
            let Sectionˉcount: u32 = Bytesˉreadˉu32ˉlittle(Input, 8u32);
            if Version != 1u32 {
                return false;
            }
            if Minorˉversion != 6u32 {
                return false;
            }
            if Sectionˉcount != 7u32 {
                return false;
            }

            let Arithmeticˉcheck: u32 = 3u32 * 4u32 - 8u32;
            if Arithmeticˉcheck <= 3u32 {
                return false;
            }
            if Arithmeticˉcheck > 4u32 {
                return false;
            }
            if Arithmeticˉcheck >= 5u32 {
                return false;
            }

            var Checkedˉbytes: u32 = 0u32;
            while Checkedˉbytes < 4u32 {
                Checkedˉbytes = Checkedˉbytes + 1u32;
            }

            return Checkedˉbytes == 4u32;
        }

        export fn Main() -> i32 {
            if Headerˉisˉvalid(Moduleˉheader) {
                return 1;
            }

            return 0;
        }
        """;

    private const string COMPOSITION_LEAF_SOURCE = """
        module Compositionˉleaf profile portable;

        enum Compositionˉstatus {
            Ready = 1;
        }

        record Compositionˉvalue {
            Value: i32;
            Status: Compositionˉstatus;
        }

        export fn Compositionˉmake(Value: i32) -> Compositionˉvalue {
            return Compositionˉvalue(Value, Compositionˉstatus.Ready);
        }

        export fn Compositionˉincrement(Value: i32) -> i32 {
            return Value + 1;
        }
        """;

    private const string COMPOSITION_MIDDLE_SOURCE = """
        module Compositionˉmiddle profile portable;

        import Compositionˉleaf;

        export fn Compositionˉanswer() -> i32 {
            let Candidate: Compositionˉvalue = Compositionˉmake(41);
            if Candidate.Status != Compositionˉstatus.Ready { return 0; }
            return Compositionˉincrement(Candidate.Value);
        }
        """;

    private const string COMPOSITION_ROOT_SOURCE = """
        module Compositionˉdemo profile portable;

        import Compositionˉmiddle;

        export fn Main() -> i32 {
            return Compositionˉanswer();
        }
        """;

    private static readonly string WVDUMP_CORE_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Wv-Dump-Core.wv");

    private static readonly string WVB_HEADER_INSPECTOR_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Wvb-Header-Inspector.wv");

    private static readonly string WVO_CORE_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Wvo-Object-Core.wv");

    private static readonly string MACHINE_CONTRACTS_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Machine-Contracts.wv");

    private static readonly string MACHINE_CONTRACTS_DEMO_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Machine-Contracts-Demo.wv");

    private static readonly string BYTE_ORDERING_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Byte-Ordering.wv");

    private static readonly string BYTE_ORDERING_DEMO_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Byte-Ordering-Demo.wv");

    private static readonly string DECIMAL_PARSING_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Decimal-Parsing.wv");

    private static readonly string DECIMAL_PARSING_DEMO_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Decimal-Parsing-Demo.wv");

    private static readonly string BYTE_CONSTRUCTION_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Byte-Construction.wv");

    private static readonly string BYTE_CONSTRUCTION_DEMO_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Byte-Construction-Demo.wv");

    private static readonly string PROJECT_MANIFEST_CORE_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Project-Manifest-Core.wv");

    private static readonly string PROJECT_MANIFEST_TOOL_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Project-Manifest-Tool.wv");

    private static readonly string SOURCE_LEXER_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Source-Lexer-Core.wv");

    private static readonly string SOURCE_LEXER_DEMO_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Source-Lexer-Demo.wv");

    private static readonly string SOURCE_DECLARATION_PARSER_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Source-Declaration-Parser.wv");

    private static readonly string SOURCE_DECLARATION_PARSER_DEMO_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Source-Declaration-Parser-Demo.wv");

    private static readonly string SOURCE_DECLARATION_PARSER_TOOL_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Source-Declaration-Parser-Tool.wv");

    private static readonly string SOURCE_BODY_PARSER_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Source-Body-Parser.wv");

    private static readonly string SOURCE_BODY_PARSER_DEMO_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Source-Body-Parser-Demo.wv");

    private static readonly string SOURCE_BODY_PARSER_TOOL_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Source-Body-Parser-Tool.wv");

    private static readonly string SOURCE_SET_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Source-Set-Core.wv");

    private static readonly string SOURCE_SET_DEMO_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Source-Set-Demo.wv");

    private static readonly string SOURCE_SET_TOOL_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Source-Set-Tool.wv");

    private static readonly string SOURCE_GRAPH_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Source-Graph-Core.wv");

    private static readonly string SOURCE_GRAPH_DEMO_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Source-Graph-Demo.wv");

    private static readonly string SOURCE_GRAPH_TOOL_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Source-Graph-Tool.wv");

    private static readonly string SOURCE_SYMBOLS_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Source-Symbols-Core.wv");

    private static readonly string SOURCE_SYMBOLS_DEMO_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Source-Symbols-Demo.wv");

    private static readonly string SOURCE_SYMBOLS_TOOL_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Source-Symbols-Tool.wv");

    private static readonly string SOURCE_BINDINGS_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Source-Bindings-Core.wv");

    private static readonly string SOURCE_BINDINGS_DEMO_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Source-Bindings-Demo.wv");

    private static readonly string SOURCE_BINDINGS_TOOL_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Source-Bindings-Tool.wv");

    private static readonly string SOURCE_WIR_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Source-Wir-Core.wv");

    private static readonly string SOURCE_WIR_DEMO_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Source-Wir-Demo.wv");

    private static readonly string SOURCE_WIR_TOOL_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Source-Wir-Tool.wv");

    private static readonly string SOURCE_WIR_VALID_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Source-Wir-Valid.wv");

    private static readonly string SOURCE_WVB_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Source-Wvb-Core.wv");

    private static readonly string SOURCE_WVB_DEMO_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Source-Wvb-Demo.wv");

    private static readonly string SOURCE_WVB_TOOL_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Source-Wvb-Tool.wv");

    private static readonly string SOURCE_WVB_FUNCTION_ONLY_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Source-Wvb-Function-Only.wv");

    private static readonly string SOURCE_WVB_DATA_AND_TEXT_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Source-Wvb-Data-And-Text.wv");

    private static readonly string SOURCE_WVB_NOMINAL_TYPES_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Source-Wvb-Nominal-Types.wv");

    private static readonly string SOURCE_WVB_HOSTED_CAPABILITIES_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Source-Wvb-Hosted-Capabilities.wv");

    private static readonly string SOURCE_WVB_COMPOSITION_ROOT_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Source-Wvb-Composition-Root.wv");

    private static readonly string SOURCE_WVB_COMPOSITION_LEAF_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Source-Wvb-Composition-Leaf.wv");

    private static readonly string SOURCE_WVB_COMPOSITION_MIDDLE_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Source-Wvb-Composition-Middle.wv");

    private static readonly string WEBASSEMBLY_CORE_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.WebAssembly-Core.wv");

    private static readonly string WEBASSEMBLY_TOOL_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.WebAssembly-Tool.wv");

    private static readonly string WEBASSEMBLY_DEMO_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.WebAssembly-Demo.wv");

    private static readonly string WEBASSEMBLY_CONSTANT_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.WebAssembly-Constant-Main.wv");

    private static readonly string WEBASSEMBLY_CHECKED_ADD_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.WebAssembly-Checked-Add-Main.wv");

    private static readonly string WEBASSEMBLY_CHECKED_ADD_OVERFLOW_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.WebAssembly-Checked-Add-Overflow-Main.wv");

    private static readonly string WEBASSEMBLY_STRAIGHT_I32_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.WebAssembly-Straight-I32-Main.wv");

    private static readonly string WEBASSEMBLY_SUBTRACT_OVERFLOW_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.WebAssembly-Checked-Subtract-Overflow-Main.wv");

    private static readonly string WEBASSEMBLY_MULTIPLY_OVERFLOW_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.WebAssembly-Checked-Multiply-Overflow-Main.wv");

    private static readonly string WEBASSEMBLY_NEGATE_OVERFLOW_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.WebAssembly-Checked-Negate-Overflow-Main.wv");

    private static readonly string HELLO_ASSEMBLY_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Hello-Object.wva");

    private static readonly string WVA_ASSEMBLER_CORE_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Wva-Assembler-Core.wv");

    private static readonly string PROCESS_ARGUMENT_COUNT_STENCIL_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Process-Argument-Count.wva");

    private static readonly string PROCESS_ARGUMENT_STENCIL_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Process-Argument.wva");

    private static readonly string NATIVE_STENCIL_CORE_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Native-Stencil-Core.wv");

    private static readonly string NATIVE_STENCIL_BRIDGE_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Native-Stencil-Bridge.wv");

    private static readonly string NATIVE_PUBLICATION_CORE_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Native-Publication-Core.wv");

    private static readonly string NATIVE_PUBLICATION_BRIDGE_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Native-Publication-Bridge.wv");

    private static readonly string NATIVE_PUBLICATION_LIFETIME_CORE_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Native-Publication-Lifetime-Core.wv");

    private static readonly string NATIVE_PUBLICATION_LIFETIME_BRIDGE_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Native-Publication-Lifetime-Bridge.wv");

    private static readonly string NATIVE_STENCIL_DEMO_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Native-Stencil-Demo.wv");

    private static readonly string CONSOLE_PROVIDER_ASSEMBLY_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Console-Provider.wva");

    private static readonly string WVLINK_CORE_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Wv-Linker-Core.wv");

    private const string TEST_AREA_ASSEMBLER = "assembler";
    private const string TEST_AREA_BYTECODE = "bytecode";
    private const string TEST_AREA_COMPILER = "compiler";
    private const string TEST_AREA_FOUNDATION = "foundation";
    private const string TEST_AREA_GOLDEN = "golden";
    private const string TEST_AREA_LINKER = "linker";
    private const string TEST_AREA_OBJECT_MODEL = "object-model";
    private const string TEST_AREA_RUNTIME = "runtime";
    private const string GOLDEN_TEST_NAME = "golden hashes identify the cross-host contract";

    private static readonly ImmutableHashSet<string> TEST_AREAS = ImmutableHashSet.Create(
        StringComparer.Ordinal,
        TEST_AREA_ASSEMBLER,
        TEST_AREA_BYTECODE,
        TEST_AREA_COMPILER,
        TEST_AREA_FOUNDATION,
        TEST_AREA_GOLDEN,
        TEST_AREA_LINKER,
        TEST_AREA_OBJECT_MODEL,
        TEST_AREA_RUNTIME);

    private static readonly ImmutableArray<string> GOLDEN_PHASE_NAMES =
    [
        "artifact-compilation",
        "baseline-runtime",
        "parser-closures",
        "source-set-closure",
        "source-graph-closure",
        "source-symbols-closure",
        "source-bindings-closure",
        "inspection-tools",
        "assembler-closure",
        "linker-closure",
        "contract-assembly",
    ];

    private static readonly List<Testˉcase> TESTS =
    [
        new("browser playground contains compilation, capabilities, and execution", [TEST_AREA_COMPILER, TEST_AREA_BYTECODE, TEST_AREA_RUNTIME], Browserˉplaygroundˉcontainsˉexecution),
        new("portable source compiles, verifies, and returns the data sum", [TEST_AREA_COMPILER, TEST_AREA_BYTECODE, TEST_AREA_RUNTIME], Portableˉprogramˉruns),
        new("hosted source requires authorization and writes text", [TEST_AREA_COMPILER, TEST_AREA_RUNTIME], Hostedˉprogramˉruns),
        new("hosted resources are explicit, separated, and bounded", [TEST_AREA_RUNTIME], Hostedˉresourcesˉareˉbounded),
        new("compiler output is deterministic and canonical", [TEST_AREA_COMPILER, TEST_AREA_BYTECODE], Compilerˉisˉdeterministic),
        new("shared x86-64 backend agrees across interpreter, JIT, and WVO AOT", [TEST_AREA_COMPILER, TEST_AREA_BYTECODE, TEST_AREA_OBJECT_MODEL, TEST_AREA_LINKER, TEST_AREA_RUNTIME], Nativeˉbackendˉconstantˉagrees),
        new("bounded wide native calls agree across interpreter, JIT, and WVO AOT", [TEST_AREA_COMPILER, TEST_AREA_BYTECODE, TEST_AREA_OBJECT_MODEL, TEST_AREA_LINKER, TEST_AREA_RUNTIME], Nativeˉwideˉcallsˉagree),
        new("native enums and records agree across interpreter, JIT, and WVO AOT", [TEST_AREA_COMPILER, TEST_AREA_BYTECODE, TEST_AREA_OBJECT_MODEL, TEST_AREA_LINKER, TEST_AREA_RUNTIME], Nativeˉnominalˉvaluesˉagree),
        new("native dynamic text, descriptor returns, and void calls agree across runtimes", [TEST_AREA_COMPILER, TEST_AREA_BYTECODE, TEST_AREA_OBJECT_MODEL, TEST_AREA_LINKER, TEST_AREA_RUNTIME], Nativeˉdynamicˉtextˉagrees),
        new("Windvale-written wvdump structural parser runs through JIT and WVO AOT", [TEST_AREA_FOUNDATION, TEST_AREA_COMPILER, TEST_AREA_BYTECODE, TEST_AREA_OBJECT_MODEL, TEST_AREA_LINKER, TEST_AREA_RUNTIME], Nativeˉwvdumpˉstructuralˉparserˉruns),
        new("complete Windvale-written wvdump agrees across interpreter, JIT, and WVO AOT", [TEST_AREA_FOUNDATION, TEST_AREA_COMPILER, TEST_AREA_BYTECODE, TEST_AREA_OBJECT_MODEL, TEST_AREA_LINKER, TEST_AREA_RUNTIME], Nativeˉwvdumpˉcompleteˉruns),
        new("native borrowed bytes and unsigned scalars agree with the reference runtime", [TEST_AREA_COMPILER, TEST_AREA_BYTECODE, TEST_AREA_OBJECT_MODEL, TEST_AREA_LINKER, TEST_AREA_RUNTIME], Nativeˉborrowedˉbytesˉagree),
        new("native runtime service writes static UTF-8 through explicit authorization", [TEST_AREA_COMPILER, TEST_AREA_BYTECODE, TEST_AREA_OBJECT_MODEL, TEST_AREA_LINKER, TEST_AREA_RUNTIME], Nativeˉruntimeˉserviceˉisˉauthorized),
        new("Windvale-assembled native stencils reproduce the argument-service leaves", [TEST_AREA_ASSEMBLER, TEST_AREA_OBJECT_MODEL, TEST_AREA_COMPILER, TEST_AREA_RUNTIME], Windvaleˉnativeˉstencilsˉreproduceˉargumentˉservices),
        new("Windvale validates and patches its native stencils across every runtime", [TEST_AREA_COMPILER, TEST_AREA_BYTECODE, TEST_AREA_OBJECT_MODEL, TEST_AREA_LINKER, TEST_AREA_RUNTIME], Windvaleˉnativeˉstencilˉconsumerˉruns),
        new("Windvale returns and publishes native argument leaves through the descriptor bridge", [TEST_AREA_COMPILER, TEST_AREA_BYTECODE, TEST_AREA_OBJECT_MODEL, TEST_AREA_LINKER, TEST_AREA_RUNTIME], Windvaleˉnativeˉstencilˉbridgeˉruns),
        new("Windvale owns bounded executable-image layout before W^X publication", [TEST_AREA_COMPILER, TEST_AREA_BYTECODE, TEST_AREA_RUNTIME], Windvaleˉnativeˉpublicationˉlayoutˉruns),
        new("Windvale owns executable publication lifetime transitions", [TEST_AREA_COMPILER, TEST_AREA_BYTECODE, TEST_AREA_RUNTIME], Windvaleˉnativeˉpublicationˉlifetimeˉruns),
        new("native hosted input inspects a real WVB through bounded argument and file snapshots", [TEST_AREA_COMPILER, TEST_AREA_BYTECODE, TEST_AREA_RUNTIME], Nativeˉhostedˉinputˉinspectsˉwvb),
        new("native file output publishes bounded bytes and advances compiler preflight", [TEST_AREA_COMPILER, TEST_AREA_BYTECODE, TEST_AREA_OBJECT_MODEL, TEST_AREA_LINKER, TEST_AREA_RUNTIME], Nativeˉfileˉoutputˉpublishes),
        new("Windvale lowers verified WVB profiles to deterministic WebAssembly", [TEST_AREA_COMPILER, TEST_AREA_BYTECODE, TEST_AREA_RUNTIME], Compilerˉwebassemblyˉruns),
        new("bounded source modules compose deterministically before bytecode lowering", [TEST_AREA_COMPILER, TEST_AREA_BYTECODE], Sourceˉmodulesˉcompose),
        new("Windvale projects select bounded deterministic source sets", [TEST_AREA_COMPILER, TEST_AREA_BYTECODE], Projectsˉselectˉsourceˉsets),
        new("Windvale-written project manifests agree with the reference parser", [TEST_AREA_COMPILER, TEST_AREA_RUNTIME], Windvaleˉprojectˉmanifestsˉagree),
        new("Windvale-written project manifests agree across interpreter, JIT, and WVO AOT", [TEST_AREA_COMPILER, TEST_AREA_BYTECODE, TEST_AREA_OBJECT_MODEL, TEST_AREA_LINKER, TEST_AREA_RUNTIME], Nativeˉprojectˉmanifestsˉagree),
        new("Foundation machine contracts are shared, bounded, and portable", [TEST_AREA_FOUNDATION, TEST_AREA_COMPILER, TEST_AREA_RUNTIME], Foundationˉmachineˉcontractsˉrun),
        new("Foundation byte ordering is shared, ordinal, and portable", [TEST_AREA_FOUNDATION, TEST_AREA_COMPILER, TEST_AREA_RUNTIME], Foundationˉbyteˉorderingˉruns),
        new("Foundation decimal parsing shares nominal results and boundaries", [TEST_AREA_FOUNDATION, TEST_AREA_COMPILER, TEST_AREA_RUNTIME], Foundationˉdecimalˉparsingˉruns),
        new("Foundation byte construction is total, bounded, and shared", [TEST_AREA_FOUNDATION, TEST_AREA_COMPILER, TEST_AREA_RUNTIME], Foundationˉbyteˉconstructionˉruns),
        new("Windvale-written source lexer streams the complete Seed token contract", [TEST_AREA_COMPILER], Compilerˉsourceˉlexerˉruns),
        new("Windvale-written declaration parser exposes bounded streaming source views", [TEST_AREA_COMPILER], Compilerˉsourceˉdeclarationˉparserˉruns),
        new("Windvale-written body parser exposes bounded statement and expression views", [TEST_AREA_COMPILER], Compilerˉsourceˉbodyˉparserˉruns),
        new("Windvale compiler source sets are canonical, bounded, and portable", [TEST_AREA_COMPILER], Compilerˉsourceˉsetˉruns),
        new("Windvale compiler import graphs are complete, acyclic, and portable", [TEST_AREA_COMPILER], Compilerˉsourceˉgraphˉruns),
        new("Windvale compiler declaration namespaces and signatures bind portably", [TEST_AREA_COMPILER], Compilerˉsourceˉsymbolsˉrun),
        new("Windvale compiler bodies, locals, and calls bind portably", [TEST_AREA_COMPILER], Compilerˉsourceˉbindingsˉrun),
        new("Windvale compiler lowers typed source into canonical validated WVIR", [TEST_AREA_COMPILER], Compilerˉsourceˉwirˉruns),
        new("Windvale compiler emits canonical executable WVB from validated WVIR", [TEST_AREA_COMPILER, TEST_AREA_BYTECODE, TEST_AREA_RUNTIME], Compilerˉsourceˉwvbˉruns),
        new("module codec round-trips exact canonical bytes", [TEST_AREA_BYTECODE], Moduleˉroundˉtrip),
        new("inspector exposes module metadata and disassembly", [TEST_AREA_BYTECODE], Inspectorˉisˉuseful),
        new("bool, if, text literals, and calls execute", [TEST_AREA_COMPILER, TEST_AREA_RUNTIME], Additionalˉsemanticsˉrun),
        new("macron names and explicit local mutability execute", [TEST_AREA_COMPILER, TEST_AREA_RUNTIME], Namingˉandˉmutabilityˉrun),
        new("Foundation byte values, slices, and little-endian reads execute", [TEST_AREA_FOUNDATION, TEST_AREA_RUNTIME], Foundationˉbytesˉrun),
        new("Foundation signed reads and strict UTF-8 text operations execute", [TEST_AREA_FOUNDATION, TEST_AREA_RUNTIME], Foundationˉtextˉrun),
        new("Foundation constructs deterministic immutable byte values", [TEST_AREA_FOUNDATION, TEST_AREA_RUNTIME], Foundationˉbyteˉconstructionˉrun),
        new("Foundation byte concatenation remains balanced under linker-scale assembly", [TEST_AREA_FOUNDATION, TEST_AREA_RUNTIME], Foundationˉbalancedˉbytesˉrun),
        new("Windvale wvdump decodes bounded payloads and instructions", [TEST_AREA_FOUNDATION, TEST_AREA_BYTECODE, TEST_AREA_RUNTIME], Wvˉdumpˉcoreˉwalksˉsections),
        new("Windvale object codec validates canonical symbols and relocations", [TEST_AREA_OBJECT_MODEL], Objectˉmodelˉroundˉtrip),
        new("Windvale-written object core matches the Stage 0 oracle", [TEST_AREA_OBJECT_MODEL, TEST_AREA_FOUNDATION, TEST_AREA_RUNTIME], Wvoˉobjectˉcoreˉmatchesˉoracle),
        new("WVA assembler emits canonical sections, symbols, and relocations", [TEST_AREA_ASSEMBLER, TEST_AREA_OBJECT_MODEL], Assemblerˉemitsˉcanonicalˉobject),
        new("WVA assembler rejects malformed and inconsistent source", [TEST_AREA_ASSEMBLER], Assemblerˉrejectsˉinvalidˉsource),
        new("Windvale-written WVA assembler enforces source and token boundaries", [TEST_AREA_ASSEMBLER, TEST_AREA_FOUNDATION, TEST_AREA_RUNTIME], Wvaˉassemblerˉcoreˉrecognizesˉsource),
        new("Windvale-written WVA assembler matches Stage 0 semantics and bytes", [TEST_AREA_ASSEMBLER, TEST_AREA_OBJECT_MODEL, TEST_AREA_RUNTIME], Wvaˉassemblerˉmatchesˉoracle),
        new("Windvale linker core scans WVO exactly at the hosted boundary", [TEST_AREA_LINKER, TEST_AREA_OBJECT_MODEL, TEST_AREA_RUNTIME], Wvˉlinkerˉcoreˉscansˉobjects),
        new("Windvale linker emits verified deterministic images and maps", [TEST_AREA_LINKER, TEST_AREA_OBJECT_MODEL, TEST_AREA_RUNTIME], Wvˉlinkerˉresolvesˉandˉlaysˉout),
        new("Stage 0 linker resolves and verifies a canonical flat image", [TEST_AREA_LINKER, TEST_AREA_OBJECT_MODEL], Linkerˉproducesˉcanonicalˉflatˉimage),
        new("Stage 0 linker rejects resolution, layout, and relocation failures", [TEST_AREA_LINKER, TEST_AREA_OBJECT_MODEL], Linkerˉrejectsˉinvalidˉlinks),
        new("Stage 0 linker contains hostile objects and remains deterministic", [TEST_AREA_LINKER, TEST_AREA_OBJECT_MODEL], Linkerˉcontainsˉhostileˉinput),
        new("immutable nominal records cross function boundaries", [TEST_AREA_COMPILER, TEST_AREA_RUNTIME], Immutableˉrecordsˉrun),
        new("nominal enums and bounded formatting execute", [TEST_AREA_COMPILER, TEST_AREA_RUNTIME], Enumsˉandˉformattingˉrun),
        new("Seed arithmetic and comparison operators execute", [TEST_AREA_COMPILER, TEST_AREA_RUNTIME], Operatorsˉrun),
        new("source diagnostics contain stable codes and locations", [TEST_AREA_COMPILER], Sourceˉdiagnosticsˉareˉuseful),
        new("binary reader rejects malformed envelopes and UTF-8", [TEST_AREA_BYTECODE], Malformedˉmodulesˉareˉrejected),
        new("verifier rejects unsafe instruction streams", [TEST_AREA_BYTECODE], Unsafeˉbytecodeˉisˉrejected),
        new("runtime traps overflow and data bounds", [TEST_AREA_RUNTIME], Runtimeˉtrapsˉareˉdeterministic),
        new("runtime enforces instruction and call-depth limits", [TEST_AREA_RUNTIME], Runtimeˉlimitsˉareˉenforced),
        new("bounded random input never escapes diagnostic boundaries", [TEST_AREA_COMPILER, TEST_AREA_BYTECODE, TEST_AREA_OBJECT_MODEL, TEST_AREA_ASSEMBLER], Randomˉinputˉisˉcontained),
        new(GOLDEN_TEST_NAME, [TEST_AREA_GOLDEN], Goldenˉhashesˉmatch),
    ];

    private static Conformanceˉcontract? Contract;
    private static readonly List<Goldenˉphaseˉtimingˉentry> GOLDEN_PHASE_TIMINGS = [];
    private static bool Collectˉgoldenˉphaseˉtimings;

    public static int Main(string[] arguments)
    {
        if (arguments.Length == 3 && arguments[0] == "--compare-reports")
        {
            return Compareˉreports(arguments[1], arguments[2]);
        }

        if (!Tryˉparseˉrunnerˉoptions(arguments, out var Options, out var Optionˉerror))
        {
            Console.Error.WriteLine(Optionˉerror);
            Writeˉrunnerˉusage();
            return 64;
        }

        if (Options.Listˉtests)
        {
            foreach (var Test in TESTS)
            {
                Console.WriteLine(Test.Name);
            }

            return 0;
        }

        if (Options.Listˉareas)
        {
            foreach (var Area in TEST_AREAS.Order(StringComparer.Ordinal))
            {
                Console.WriteLine(Area);
            }

            return 0;
        }

        IEnumerable<Testˉcase> Selection = TESTS;
        if (Options.Filter is not null)
        {
            Selection = Selection.Where(
                Test => Test.Name.Contains(Options.Filter, StringComparison.OrdinalIgnoreCase));
        }
        if (Options.Areas.Count != 0)
        {
            Selection = Selection.Where(Test => Test.Areas.Any(Options.Areas.Contains));
        }

        var Selectedˉtests = Selection.ToList();
        if (Selectedˉtests.Count == 0)
        {
            var Selectionˉdescription = Options.Filter is null
                ? $"areas [{string.Join(", ", Options.Areas.Order(StringComparer.Ordinal))}]"
                : Options.Areas.Count == 0
                    ? $"filter '{Options.Filter}'"
                    : $"filter '{Options.Filter}' and areas " +
                      $"[{string.Join(", ", Options.Areas.Order(StringComparer.Ordinal))}]";
            Console.Error.WriteLine($"No tests match {Selectionˉdescription}.");
            return 64;
        }

        GOLDEN_PHASE_TIMINGS.Clear();
        Collectˉgoldenˉphaseˉtimings = Options.Timingˉreportˉpath is not null;

        var Failures = 0;
        var Timings = new List<Testˉtimingˉentry>(Selectedˉtests.Count);
        var Suiteˉtimer = Stopwatch.StartNew();
        foreach (var Test in Selectedˉtests)
        {
            var Testˉtimer = Stopwatch.StartNew();
            var Outcome = "passed";
            try
            {
                Test.Body();
            }
            catch (Exception Exception)
            {
                Failures++;
                Outcome = "failed";
                Testˉtimer.Stop();
                Console.Error.WriteLine($"FAIL  {Test.Name} ({Testˉtimer.ElapsedMilliseconds} ms)");
                Console.Error.WriteLine($"      {Exception.Message}");
                Timings.Add(new(Test.Name, Outcome, Testˉtimer.ElapsedMilliseconds));
                if (Options.Failˉfast)
                {
                    break;
                }

                continue;
            }

            Testˉtimer.Stop();
            Console.WriteLine($"PASS  {Test.Name} ({Testˉtimer.ElapsedMilliseconds} ms)");
            Timings.Add(new(Test.Name, Outcome, Testˉtimer.ElapsedMilliseconds));
        }

        Suiteˉtimer.Stop();
        Console.WriteLine();
        Console.WriteLine(
            $"Tests: {Selectedˉtests.Count}, Executed: {Timings.Count}, " +
            $"Passed: {Timings.Count - Failures}, Failed: {Failures}, " +
            $"Elapsed: {Suiteˉtimer.Elapsed.TotalSeconds:F3} s");
        if (Options.Timingˉreportˉpath is not null)
        {
            Writeˉtimingˉreport(
                Options.Timingˉreportˉpath,
                Options.Filter,
                Options.Areas,
                Options.Failˉfast,
                Selectedˉtests.Count,
                Suiteˉtimer.ElapsedMilliseconds,
                Timings);
        }

        if (Failures != 0)
        {
            return 1;
        }

        if (Options.Reportˉpath is not null)
        {
            Writeˉreport(Options.Reportˉpath);
        }

        return 0;
    }

    private static bool Tryˉparseˉrunnerˉoptions(
        string[] arguments,
        out Testˉrunnerˉoptions options,
        out string error)
    {
        string? Reportˉpath = null;
        string? Filter = null;
        string? Timingˉreportˉpath = null;
        var Areas = ImmutableHashSet.CreateBuilder<string>(StringComparer.Ordinal);
        var Failˉfast = false;
        var Listˉtests = false;
        var Listˉareas = false;

        for (var Index = 0; Index < arguments.Length; Index++)
        {
            var Argument = arguments[Index];
            if (Argument is "--report" or "--filter" or "--area" or "--timing-report")
            {
                if (Index + 1 >= arguments.Length || string.IsNullOrWhiteSpace(arguments[Index + 1]))
                {
                    options = Testˉrunnerˉoptions.Empty;
                    error = $"{Argument} requires a value.";
                    return false;
                }

                var Value = arguments[++Index];
                if (Argument == "--report")
                {
                    if (Reportˉpath is not null)
                    {
                        options = Testˉrunnerˉoptions.Empty;
                        error = "--report may be specified only once.";
                        return false;
                    }

                    Reportˉpath = Value;
                }
                else if (Argument == "--filter")
                {
                    if (Filter is not null)
                    {
                        options = Testˉrunnerˉoptions.Empty;
                        error = "--filter may be specified only once.";
                        return false;
                    }

                    Filter = Value;
                }
                else if (Argument == "--area")
                {
                    var Canonicalˉarea = TEST_AREAS.FirstOrDefault(
                        Area => string.Equals(Area, Value, StringComparison.OrdinalIgnoreCase));
                    if (Canonicalˉarea is null)
                    {
                        options = Testˉrunnerˉoptions.Empty;
                        error = $"Unknown test area '{Value}'. Expected one of: " +
                            string.Join(", ", TEST_AREAS.Order(StringComparer.Ordinal)) + ".";
                        return false;
                    }

                    Areas.Add(Canonicalˉarea);
                }
                else
                {
                    if (Timingˉreportˉpath is not null)
                    {
                        options = Testˉrunnerˉoptions.Empty;
                        error = "--timing-report may be specified only once.";
                        return false;
                    }

                    Timingˉreportˉpath = Value;
                }

                continue;
            }

            if (Argument == "--fail-fast")
            {
                Failˉfast = true;
                continue;
            }

            if (Argument == "--list-tests")
            {
                Listˉtests = true;
                continue;
            }

            if (Argument == "--list-areas")
            {
                Listˉareas = true;
                continue;
            }

            options = Testˉrunnerˉoptions.Empty;
            error = $"Unknown argument: {Argument}";
            return false;
        }

        if (Reportˉpath is not null && (Filter is not null || Areas.Count != 0))
        {
            options = Testˉrunnerˉoptions.Empty;
            error = "--report requires the complete test suite and cannot be combined with selection options.";
            return false;
        }

        if (Reportˉpath is not null && Failˉfast)
        {
            options = Testˉrunnerˉoptions.Empty;
            error = "--report requires the complete test suite and cannot be combined with --fail-fast.";
            return false;
        }

        if ((Listˉtests || Listˉareas) && (
            Listˉtests == Listˉareas ||
            Reportˉpath is not null ||
            Filter is not null ||
            Areas.Count != 0 ||
            Failˉfast ||
            Timingˉreportˉpath is not null))
        {
            options = Testˉrunnerˉoptions.Empty;
            error = "A list option must be used alone.";
            return false;
        }

        options = new(
            Reportˉpath,
            Filter,
            Areas.ToImmutable(),
            Failˉfast,
            Timingˉreportˉpath,
            Listˉtests,
            Listˉareas);
        error = string.Empty;
        return true;
    }

    private static void Writeˉrunnerˉusage()
    {
        Console.Error.WriteLine(
            "Usage: Windvale.Seed.Tests [--report <path>] [--timing-report <path>]\n" +
            "       Windvale.Seed.Tests [--filter <substring>] [--area <name>]... " +
            "[--fail-fast] [--timing-report <path>]\n" +
            "       Windvale.Seed.Tests --list-tests\n" +
            "       Windvale.Seed.Tests --list-areas\n" +
            "       Windvale.Seed.Tests --compare-reports <first> <second>");
    }

    private static void Browserˉplaygroundˉcontainsˉexecution()
    {
        var Examples = Playgroundˉexamples.All;
        Equal(Examples.Length, Examples.Select(Example => Example.Id).Distinct(StringComparer.Ordinal).Count());

        var Hello = Examples.Single(Example => Example.Id == "hello");
        var Helloˉresult = Playgroundˉrunner.Run(new(
            Hello.Source,
            Hello.Recommendedˉcapabilities));
        Equal(Playgroundˉstatus.Completed, Helloˉresult.Status);
        Equal("Hello from Windvale\n", Helloˉresult.Standardˉoutput);
        Equal(0, Helloˉresult.Exitˉcode);
        Equal(Moduleˉprofile.Hosted, Helloˉresult.Profile);
        True(Helloˉresult.Bytecodeˉbytes.Length > 0, "The playground did not retain compiled WVB.");
        Equal(64, Helloˉresult.Moduleˉsha256!.Length);
        Contains(Helloˉresult.Bytecodeˉreport!, "call.capability");
        True(
            Helloˉresult.Executedˉinstructions > 0,
            "The playground did not report executed instructions.");

        var Unauthorized = Playgroundˉrunner.Run(new(
            Hello.Source,
            ImmutableHashSet.Create<string>(StringComparer.Ordinal)));
        Equal(Playgroundˉstatus.Runtimeˉfailed, Unauthorized.Status);
        Equal("WVR3010", Unauthorized.Diagnostics.Single().Code);

        var Sum = Examples.Single(Example => Example.Id == "sum-data");
        var Sumˉresult = Playgroundˉrunner.Run(new(
            Sum.Source,
            Sum.Recommendedˉcapabilities));
        Equal(Playgroundˉstatus.Completed, Sumˉresult.Status);
        Equal(29, Sumˉresult.Exitˉcode);
        Equal(Moduleˉprofile.Portable, Sumˉresult.Profile);
        Equal(0, Sumˉresult.Requiredˉcapabilities.Length);
        var Unsupportedˉwebassembly = Playgroundˉwebassemblyˉlowerer.Lower(
            Sumˉresult.Bytecodeˉbytes);
        Equal(
            Playgroundˉwebassemblyˉloweringˉstatus.Unsupported,
            Unsupportedˉwebassembly.Status);
        True(
            Unsupportedˉwebassembly.Selectorˉstatus?.StartsWith(
                "Unsupportedˉ",
                StringComparison.Ordinal) == true,
            "The playground did not retain the Windvale backend selector status.");
        Equal(0, Unsupportedˉwebassembly.Webassemblyˉbytes.Length);

        var Webassembly = Examples.Single(Example => Example.Id == "webassembly-worker");
        var Webassemblyˉreference = Playgroundˉrunner.Run(new(
            Webassembly.Source,
            Webassembly.Recommendedˉcapabilities));
        Equal(Playgroundˉstatus.Completed, Webassemblyˉreference.Status);
        Equal(Moduleˉprofile.Portable, Webassemblyˉreference.Profile);
        Equal(42, Webassemblyˉreference.Exitˉcode);
        Equal(30L, Webassemblyˉreference.Executedˉinstructions);
        Equal(WEBASSEMBLY_STRAIGHT_I32_WVB_SHA256, Webassemblyˉreference.Moduleˉsha256);
        var Webassemblyˉlowered = Playgroundˉwebassemblyˉlowerer.Lower(
            Webassemblyˉreference.Bytecodeˉbytes);
        Equal(
            Playgroundˉwebassemblyˉloweringˉstatus.Lowered,
            Webassemblyˉlowered.Status);
        Equal("Valid", Webassemblyˉlowered.Selectorˉstatus);
        Equal(432, Webassemblyˉlowered.Webassemblyˉbytes.Length);
        Equal(WEBASSEMBLY_STRAIGHT_I32_SHA256, Webassemblyˉlowered.Webassemblyˉsha256);
        True(
            Webassemblyˉlowered.Loweringˉinstructions > 0,
            "The playground did not report Windvale backend execution evidence.");
        var Webassemblyˉrepeat = Playgroundˉwebassemblyˉlowerer.Lower(
            Webassemblyˉreference.Bytecodeˉbytes);
        Equal(Webassemblyˉlowered.Status, Webassemblyˉrepeat.Status);
        Equal(Webassemblyˉlowered.Webassemblyˉsha256, Webassemblyˉrepeat.Webassemblyˉsha256);
        Equal(Webassemblyˉlowered.Loweringˉinstructions, Webassemblyˉrepeat.Loweringˉinstructions);
        Sequenceˉequal(
            Webassemblyˉlowered.Webassemblyˉbytes,
            Webassemblyˉrepeat.Webassemblyˉbytes);

        var Twoˉchannels = Examples.Single(Example => Example.Id == "two-channels");
        var Twoˉchannelˉresult = Playgroundˉrunner.Run(new(
            Twoˉchannels.Source,
            Twoˉchannels.Recommendedˉcapabilities));
        Equal(Playgroundˉstatus.Completed, Twoˉchannelˉresult.Status);
        Equal("Build complete\n", Twoˉchannelˉresult.Standardˉoutput);
        Equal("verified: canonical WVB\n", Twoˉchannelˉresult.Diagnosticˉoutput);

        var Nominal = Examples.Single(Example => Example.Id == "records-enums");
        var Nominalˉresult = Playgroundˉrunner.Run(new(
            Nominal.Source,
            Nominal.Recommendedˉcapabilities));
        Equal(Playgroundˉstatus.Completed, Nominalˉresult.Status);
        Equal(42, Nominalˉresult.Exitˉcode);

        var Textˉformatting = Examples.Single(Example => Example.Id == "text-formatting");
        var Textˉformattingˉresult = Playgroundˉrunner.Run(new(
            Textˉformatting.Source,
            Textˉformatting.Recommendedˉcapabilities));
        Equal(Playgroundˉstatus.Completed, Textˉformattingˉresult.Status);
        Equal("Windvale: Running, build 42\n", Textˉformattingˉresult.Standardˉoutput);
        Equal(0, Textˉformattingˉresult.Exitˉcode);

        var Unicode = Examples.Single(Example => Example.Id == "unicode-round-trip");
        var Unicodeˉresult = Playgroundˉrunner.Run(new(
            Unicode.Source,
            Unicode.Recommendedˉcapabilities));
        Equal(Playgroundˉstatus.Completed, Unicodeˉresult.Status);
        Equal("Hello, 世界\nUTF-8 bytes: 13\n", Unicodeˉresult.Standardˉoutput);
        Equal(0, Unicodeˉresult.Exitˉcode);

        var Binary = Examples.Single(Example => Example.Id == "inspect-bytes");
        var Binaryˉresult = Playgroundˉrunner.Run(new(
            Binary.Source,
            Binary.Recommendedˉcapabilities));
        Equal(Playgroundˉstatus.Completed, Binaryˉresult.Status);
        Equal(
            "version=1, sections=7\n" +
            "magic sha256=7b30cf1ebf4a969d835a5236be8488aa57d613a26c30eaa62c20b27059a6bd5f\n",
            Binaryˉresult.Standardˉoutput);
        Equal(0, Binaryˉresult.Exitˉcode);

        var Budget = Examples.Single(Example => Example.Id == "instruction-budget");
        var Budgetˉresult = Playgroundˉrunner.Run(new(
            Budget.Source,
            Budget.Recommendedˉcapabilities,
            50));
        Equal(Playgroundˉstatus.Runtimeˉfailed, Budgetˉresult.Status);
        Equal("WVR3011", Budgetˉresult.Diagnostics.Single().Code);

        var Invalid = Playgroundˉrunner.Run(new(
            "module Invalid profile portable; export fn Main(",
            ImmutableHashSet.Create<string>(StringComparer.Ordinal)));
        Equal(Playgroundˉstatus.Compilationˉfailed, Invalid.Status);
        True(!Invalid.Diagnostics.IsEmpty, "Invalid playground source produced no diagnostic.");
        Equal(0, Invalid.Bytecodeˉbytes.Length);

        var System = Playgroundˉrunner.Run(new(
            "module Browserˉsystem profile system; export fn Main() -> i32 { return 0; }",
            ImmutableHashSet.Create<string>(StringComparer.Ordinal)));
        Equal(Playgroundˉstatus.Rejected, System.Status);
        Equal("WVPG1003", System.Diagnostics.Single().Code);

        var Unsupportedˉauthorization = Playgroundˉrunner.Run(new(
            Sum.Source,
            ImmutableHashSet.Create(StringComparer.Ordinal, Capabilityˉcatalog.FILE_READ_BYTES)));
        Equal(Playgroundˉstatus.Rejected, Unsupportedˉauthorization.Status);
        Equal("WVPG1002", Unsupportedˉauthorization.Diagnostics.Single().Code);

        const string Unsupportedˉmodule = """
            module Unsupportedˉmodule profile hosted;
            capability file.read_bytes;
            export fn Main() -> i32 {
                let Value: bytes = file.read_bytes("input.wvb");
                return 0;
            }
            """;
        var Unsupportedˉmoduleˉresult = Playgroundˉrunner.Run(new(
            Unsupportedˉmodule,
            ImmutableHashSet.Create<string>(StringComparer.Ordinal)));
        Equal(Playgroundˉstatus.Rejected, Unsupportedˉmoduleˉresult.Status);
        Equal("WVPG1004", Unsupportedˉmoduleˉresult.Diagnostics.Single().Code);

        var Outputˉchunk = new string('x', 20 * 1024);
        var Outputˉsource = $$"""
            module Boundedˉoutput profile hosted;
            capability console.write;
            data Chunk: text = "{{Outputˉchunk}}";
            export fn Main() -> i32 {
                console.write(Chunk);
                console.write(Chunk);
                console.write(Chunk);
                console.write(Chunk);
                return 0;
            }
            """;
        var Boundedˉoutput = Playgroundˉrunner.Run(new(
            Outputˉsource,
            ImmutableHashSet.Create(StringComparer.Ordinal, Capabilityˉcatalog.CONSOLE_WRITE)));
        Equal(Playgroundˉstatus.Runtimeˉfailed, Boundedˉoutput.Status);
        Equal("WVR3029", Boundedˉoutput.Diagnostics.Single().Code);
        Equal(60 * 1024, Boundedˉoutput.Standardˉoutput.Length);

        var Oversized = Playgroundˉrunner.Run(new(
            new string(' ', Playgroundˉlimits.MAXIMUM_SOURCE_CHARACTERS + 1),
            ImmutableHashSet.Create<string>(StringComparer.Ordinal)));
        Equal(Playgroundˉstatus.Rejected, Oversized.Status);
        Equal("WVPG1001", Oversized.Diagnostics.Single().Code);

        var Invalidˉbudget = Playgroundˉrunner.Run(new(
            Sum.Source,
            Sum.Recommendedˉcapabilities,
            Playgroundˉlimits.MAXIMUM_INSTRUCTIONS + 1));
        Equal(Playgroundˉstatus.Rejected, Invalidˉbudget.Status);
        Equal("WVPG1005", Invalidˉbudget.Diagnostics.Single().Code);
    }

    private static void Portableˉprogramˉruns()
    {
        var Bytes = Compileˉsuccess(SUM_SOURCE);
        var Module = Moduleˉcodec.Readˉandˉverify(Bytes);
        Equal(Moduleˉprofile.Portable, Module.Module.Profile);
        Equal(0, Module.Module.Capabilities.Length);
        var Output = new StringWriter();
        var Runtime = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(Output),
            Runtimeˉoptions.Portableˉdefaults);
        var Result = Runtime.Runˉmain();
        Equal(29, Result.Exitˉcode);
        Equal(string.Empty, Output.ToString());
        True(Result.Executedˉinstructions > 0, "The runtime did not count executed instructions.");
        Equal(0, Runtime.Readˉfunctionˉsteps().Length);

        var Profiledˉruntime = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new StringWriter()),
            Runtimeˉoptions.Portableˉdefaults with { Collectˉfunctionˉsteps = true });
        Equal(29, Profiledˉruntime.Runˉmain().Exitˉcode);
        var Functionˉsteps = Profiledˉruntime.Readˉfunctionˉsteps();
        Equal(2, Functionˉsteps.Length);
        Equal(1, Functionˉsteps[0].Functionˉindex);
        Equal("Main", Functionˉsteps[0].Functionˉname);
        Equal(163L, Functionˉsteps[0].Executedˉinstructions);
        Equal(0, Functionˉsteps[1].Functionˉindex);
        Equal("Add", Functionˉsteps[1].Functionˉname);
        Equal(40L, Functionˉsteps[1].Executedˉinstructions);
    }

    private static void Hostedˉprogramˉruns()
    {
        var Bytes = Compileˉsuccess(HELLO_SOURCE);
        var Module = Moduleˉcodec.Readˉandˉverify(Bytes);
        Equal(Moduleˉprofile.Hosted, Module.Module.Profile);
        Equal(Capabilityˉcatalog.CONSOLE_WRITE_LINE, Module.Module.Capabilities.Single().Name);

        var Unauthorized = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new StringWriter()),
            Runtimeˉoptions.Portableˉdefaults);
        Throwsˉruntime("WVR3010", () => _ = Unauthorized.Runˉmain());

        var Output = new StringWriter();
        var Authorized = ImmutableHashSet.Create(
            StringComparer.Ordinal,
            Capabilityˉcatalog.CONSOLE_WRITE_LINE);
        var Runtime = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(Output),
            new(Authorized));
        var Result = Runtime.Runˉmain();
        Equal(0, Result.Exitˉcode);
        Equal("Hello from Windvale\n", Output.ToString());
    }

    private static void Hostedˉresourcesˉareˉbounded()
    {
        const string Source = """
            module Hostedˉresources profile hosted;

            capability console.write;
            capability console.write_line;
            capability diagnostic.write_line;
            capability file.read_bytes;
            capability file.write_bytes;
            capability process.argument;
            capability process.argument_count;

            export fn Main() -> i32 {
                if process.argument_count() != 2u32 {
                    return 1;
                }

                let Resourceˉname: text = process.argument(0u32);
                console.write(Resourceˉname);
                console.write_line(Textˉconcat(":", process.argument(1u32)));
                let Input: bytes = file.read_bytes(Resourceˉname);
                let Sameˉinput: bytes = file.read_bytes(Resourceˉname);
                if Bytesˉlength(Sameˉinput) != 3u32 { return 2; }
                if Bytesˉreadˉu8(Sameˉinput, 0u32) != 87u8 { return 3; }
                file.write_bytes(process.argument(1u32), Input);
                console.write_line(Textˉconcat("bytes=", U32ˉformat(Bytesˉlength(Input))));
                diagnostic.write_line("note");
                return 0;
            }
            """;

        var Module = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(Source));
        Sequenceˉequal(
            [
                Capabilityˉcatalog.CONSOLE_WRITE,
                Capabilityˉcatalog.CONSOLE_WRITE_LINE,
                Capabilityˉcatalog.DIAGNOSTIC_WRITE_LINE,
                Capabilityˉcatalog.FILE_READ_BYTES,
                Capabilityˉcatalog.FILE_WRITE_BYTES,
                Capabilityˉcatalog.PROCESS_ARGUMENT,
                Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT,
            ],
            Module.Module.Capabilities.Select(Capability => Capability.Name));
        var Authorized = Module.Module.Capabilities
            .Select(Capability => Capability.Name)
            .ToImmutableHashSet(StringComparer.Ordinal);
        var Output = new StringWriter();
        var Diagnostics = new StringWriter();
        var Files = new Testˉfileˉreader((Resourceˉname, Maximumˉbytes) =>
        {
            Equal("input.wvb", Resourceˉname);
            Equal(Bytecodeˉlimits.MAX_BYTE_DATA_BYTES, Maximumˉbytes);
            return [87, 86, 66];
        });
        var Fileˉwriter = new Capturingˉfileˉwriter();
        var Runtime = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                ["input.wvb", "tail"],
                Output,
                Diagnostics,
                Files,
                Fileˉwriter)),
            new(Authorized));
        Equal(0, Runtime.Runˉmain().Exitˉcode);
        Equal(1, Files.Readˉcount);
        Equal("input.wvb:tail\nbytes=3\n", Output.ToString());
        Equal("note\n", Diagnostics.ToString());
        Equal(1, Fileˉwriter.Writeˉcount);
        Equal("tail", Fileˉwriter.Resourceˉname);
        Sequenceˉequal<byte>([87, 86, 66], Fileˉwriter.Bytes);

        var Unsupported = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new StringWriter()),
            new(Authorized));
        Throwsˉruntime("WVR3001", () => _ = Unsupported.Runˉmain());

        const string Badˉargument = """
            module Badˉargument profile hosted;
            capability process.argument;
            export fn Main() -> i32 {
                process.argument(0u32);
                return 0;
            }
            """;
        var Badˉargumentˉmodule = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(Badˉargument));
        var Badˉargumentˉruntime = new Referenceˉruntime(
            Badˉargumentˉmodule,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                [],
                TextWriter.Null,
                TextWriter.Null)),
            new(ImmutableHashSet.Create(StringComparer.Ordinal, Capabilityˉcatalog.PROCESS_ARGUMENT)));
        Throwsˉruntime("WVR3020", () => _ = Badˉargumentˉruntime.Runˉmain());

        const string Fileˉsource = """
            module Fileˉresource profile hosted;
            capability file.read_bytes;
            export fn Main() -> i32 {
                file.read_bytes("input.wvb");
                return 0;
            }
            """;
        var Fileˉmodule = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(Fileˉsource));
        var Fileˉauthorization = new Runtimeˉoptions(
            ImmutableHashSet.Create(StringComparer.Ordinal, Capabilityˉcatalog.FILE_READ_BYTES));
        var Missingˉruntime = new Referenceˉruntime(
            Fileˉmodule,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                [],
                TextWriter.Null,
                TextWriter.Null,
                new Testˉfileˉreader((_, _) => throw new Hostedˉfileˉexception(
                    Hostedˉfileˉerror.Notˉfound,
                    "The requested test resource was not found.")))),
            Fileˉauthorization);
        Throwsˉruntime("WVR3022", () => _ = Missingˉruntime.Runˉmain());

        var Oversizedˉruntime = new Referenceˉruntime(
            Fileˉmodule,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                [],
                TextWriter.Null,
                TextWriter.Null,
                new Testˉfileˉreader((_, Maximumˉbytes) =>
                    ImmutableArray.Create(new byte[Maximumˉbytes + 1])))),
            Fileˉauthorization);
        Throwsˉruntime("WVR3025", () => _ = Oversizedˉruntime.Runˉmain());

        var Snapshotˉreader = new Testˉfileˉreader((_, _) => [1]);
        var Snapshotˉcontext = new Hostedˉresourceˉcontext(
            [],
            TextWriter.Null,
            TextWriter.Null,
            Snapshotˉreader);
        var Snapshotˉhost = new Referenceˉcapabilityˉhost(Snapshotˉcontext);
        var Readˉcapability = Fileˉmodule.Module.Capabilities.Single();
        for (var Index = 0; Index < Hostedˉresourceˉlimits.MAX_FILE_SNAPSHOTS; Index++)
        {
            _ = Snapshotˉhost.Invoke(
                Readˉcapability,
                [Runtimeˉvalue.Fromˉtext($"input-{Index}.wvo")]);
        }

        Equal(Hostedˉresourceˉlimits.MAX_FILE_SNAPSHOTS, Snapshotˉreader.Readˉcount);
        _ = new Referenceˉcapabilityˉhost(Snapshotˉcontext).Invoke(
            Readˉcapability,
            [Runtimeˉvalue.Fromˉtext("input-0.wvo")]);
        Equal(Hostedˉresourceˉlimits.MAX_FILE_SNAPSHOTS, Snapshotˉreader.Readˉcount);
        Throwsˉruntime(
            "WVR3028",
            () => _ = Snapshotˉhost.Invoke(
                Readˉcapability,
                [Runtimeˉvalue.Fromˉtext("input-over-limit.wvo")]));

        const string Invalidˉresult = """
            module Invalidˉhostˉresult profile hosted;
            capability process.argument_count;
            export fn Main() -> i32 {
                process.argument_count();
                return 0;
            }
            """;
        var Invalidˉresultˉmodule = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(Invalidˉresult));
        var Invalidˉresultˉruntime = new Referenceˉruntime(
            Invalidˉresultˉmodule,
            new Invalidˉresultˉcapabilityˉhost(),
            new(ImmutableHashSet.Create(
                StringComparer.Ordinal,
                Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT)));
        Throwsˉruntime("WVR3013", () => _ = Invalidˉresultˉruntime.Runˉmain());

        Throwsˉruntime(
            "WVR3027",
            () => _ = new Hostedˉresourceˉcontext(
                [.. Enumerable.Repeat("a", Hostedˉresourceˉlimits.MAX_ARGUMENTS + 1)],
                TextWriter.Null,
                TextWriter.Null));
        Throwsˉruntime(
            "WVR3027",
            () => _ = new Hostedˉresourceˉcontext(
                [new string('a', Hostedˉresourceˉlimits.MAX_ARGUMENT_UTF8_BYTES + 1)],
                TextWriter.Null,
                TextWriter.Null));
        Throwsˉruntime(
            "WVR3027",
            () => _ = new Hostedˉresourceˉcontext(
                [.. Enumerable.Repeat(new string('a', 4096), 17)],
                TextWriter.Null,
                TextWriter.Null));
        Throwsˉruntime(
            "WVR3027",
            () => _ = new Hostedˉresourceˉcontext(
                ["\uD800"],
                TextWriter.Null,
                TextWriter.Null));
    }

    private static void Compilerˉisˉdeterministic()
    {
        var First = Compileˉsuccess(SUM_SOURCE);
        var Second = Compileˉsuccess(SUM_SOURCE);
        Sequenceˉequal(First, Second);

        const string Reorderedˉsource = """
            module Canonical profile portable;
            data Zed: text = "z";
            data Alpha: [i32] = [1];
            export fn Main() -> i32 { return Zebra(); }
            fn Zebra() -> i32 { return Alpha[0]; }
            """;
        var Module = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(Reorderedˉsource));
        Sequenceˉequal(["Alpha", "Zed"], Module.Module.Data.Select(Data => Data.Name));
        Sequenceˉequal(["Main", "Zebra"], Module.Module.Functions.Select(Function => Function.Name));
    }

    private static void Nativeˉbackendˉconstantˉagrees()
    {
        var Wvbˉbytes = Compileˉsuccess(NATIVE_CONSTANT_SOURCE);
        var Verified = Moduleˉcodec.Readˉandˉverify(Wvbˉbytes);
        var Interpreted = new Referenceˉruntime(
            Verified,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        Equal(42, Interpreted.Exitˉcode);
        Equal(4L, Interpreted.Executedˉinstructions);

        var First = X64ˉnativeˉbackend.Compile(Verified);
        var Second = X64ˉnativeˉbackend.Compile(Verified);
        Equal(1, First.Module.Functions.Length);
        Equal(1, First.Module.Functions[0].Blocks.Length);
        Equal(7, First.Module.Functions[0].Blocks[0].Operations.Length);
        Equal(
            4,
            First.Module.Functions[0].Blocks[0].Operations.OfType<Nativeˉinstructionˉcharge>().Count());
        True(First.Module.Functions[0].Blocks[0].Operations[1] is Nativeˉi32ˉconstant { Result: 0, Value: 42 },
            "Native machine IR did not retain the constant definition.");
        True(First.Module.Functions[0].Blocks[0].Terminator is Nativeˉreturn { Value: 1 },
            "Native machine IR did not retain the return use.");
        Sequenceˉequal(
            new byte[]
            {
                0x41, 0x57,
                0x49, 0x89, 0xD7,
                0x4D, 0x8B, 0x5F, 0x08,
                0x4D, 0x8B, 0x57, 0x10,
            },
            First.Fragment.Code.Take(13));
        Sequenceˉequal(First.Fragment.Code, Second.Fragment.Code);
        Equal(19, Nativeˉcontract.ABI_VERSION);
        Equal(2_048, Nativeˉcontract.MAXIMUM_FRAME_SLOTS);
        Equal(100_000, Nativeˉcontract.MAXIMUM_VALUE_IDENTIFIERS);
        Equal(32_768, Nativeˉcontract.MAXIMUM_FRAME_BYTES);
        Equal(Nativeˉcontract.X64_BASELINE_TARGET, First.Fragment.Target);
        Equal(Nativeˉcontract.ABI_VERSION, First.Fragment.Abiˉversion);
        Equal(0, First.Fragment.Patches.Length);
        _ = Nativeˉfragmentˉverifier.Verify(First.Fragment);

        var Firstˉobject = Nativeˉobjectˉsink.Writeˉwvo(First.Fragment);
        var Secondˉobject = Nativeˉobjectˉsink.Writeˉwvo(Second.Fragment);
        Sequenceˉequal(Firstˉobject, Secondˉobject);
        Equal(406, First.Fragment.Code.Length);
        Equal(NATIVE_CONSTANT_CODE_SHA256, Objectˉdigest.Calculateˉsha256(First.Fragment.Code.AsSpan()));
        Equal(479, Firstˉobject.Length);
        Equal(NATIVE_CONSTANT_WVO_SHA256, Objectˉdigest.Calculateˉsha256(Firstˉobject.AsSpan()));
        var Verifiedˉobject = Objectˉcodec.Readˉandˉverify(Firstˉobject.AsSpan()).Value;
        Equal(1, Verifiedˉobject.Sections.Length);
        Equal(1, Verifiedˉobject.Symbols.Length);
        True(Verifiedˉobject.Symbols.Any(Symbol => Symbol.Name == "Main"), "Native WVO omitted Main.");
        Equal(0, Verifiedˉobject.Relocations.Length);

        var Linked = Linkˉsuccess(
            [Firstˉobject.ToArray()],
            new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"));
        Equal(Linkˉcontract.DEFAULT_BASE_ADDRESS, Linked.Entryˉaddress);
        Sequenceˉequal(First.Fragment.Code, Linked.Imageˉbytes);

        var Jitˉresult = X64ˉnativeˉexecutor.Executeˉi32(First.Fragment);
        Equal(42, X64ˉnativeˉexecutor.Executeˉi32(
            First.Fragment,
            maximumˉinstructions: Interpreted.Executedˉinstructions));
        Throwsˉnativeˉtrap(
            "WVR3011",
            () => _ = X64ˉnativeˉexecutor.Executeˉi32(
                First.Fragment,
                maximumˉinstructions: Interpreted.Executedˉinstructions - 1));
        var Invalidˉbudgetˉrejected = false;
        try
        {
            _ = X64ˉnativeˉexecutor.Executeˉi32(First.Fragment, maximumˉinstructions: 0);
        }
        catch (ArgumentOutOfRangeException)
        {
            Invalidˉbudgetˉrejected = true;
        }
        True(Invalidˉbudgetˉrejected, "The native executor accepted a non-positive instruction budget.");
        var Aotˉfragment = First.Fragment with { Code = Linked.Imageˉbytes };
        var Aotˉresult = X64ˉnativeˉexecutor.Executeˉi32(Aotˉfragment);
        Equal(Interpreted.Exitˉcode, Jitˉresult);
        Equal(Interpreted.Exitˉcode, Aotˉresult);

        var Arithmeticˉverified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(NATIVE_ARITHMETIC_SOURCE));
        var Arithmeticˉinterpreted = new Referenceˉruntime(
            Arithmeticˉverified,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        Equal(42, Arithmeticˉinterpreted.Exitˉcode);
        var Arithmeticˉfirst = X64ˉnativeˉbackend.Compile(Arithmeticˉverified);
        var Arithmeticˉsecond = X64ˉnativeˉbackend.Compile(Arithmeticˉverified);
        var Arithmeticˉoperations = Arithmeticˉfirst.Module.Functions[0].Blocks
            .SelectMany(Block => Block.Operations)
            .ToImmutableArray();
        True(
            Arithmeticˉoperations.OfType<Nativeˉi32ˉbinary>()
                .Any(Operation => Operation.Kind == Nativeˉi32ˉbinaryˉkind.Add),
            "Native machine IR did not retain checked i32 addition.");
        True(
            Arithmeticˉoperations.OfType<Nativeˉi32ˉbinary>()
                .Any(Operation => Operation.Kind == Nativeˉi32ˉbinaryˉkind.Subtract),
            "Native machine IR did not retain checked i32 subtraction.");
        True(
            Arithmeticˉoperations.OfType<Nativeˉi32ˉbinary>()
                .Any(Operation => Operation.Kind == Nativeˉi32ˉbinaryˉkind.Multiply),
            "Native machine IR did not retain checked i32 multiplication.");
        True(
            Arithmeticˉoperations.Any(Operation => Operation is Nativeˉi32ˉnegate),
            "Native machine IR did not retain checked i32 negation.");
        Sequenceˉequal(Arithmeticˉfirst.Fragment.Code, Arithmeticˉsecond.Fragment.Code);
        Equal(1, Arithmeticˉfirst.Fragment.Symbols.Length);
        Equal("Main", Arithmeticˉfirst.Fragment.Symbols[0].Name);
        _ = Nativeˉfragmentˉverifier.Verify(Arithmeticˉfirst.Fragment);
        var Arithmeticˉfirstˉobject = Nativeˉobjectˉsink.Writeˉwvo(Arithmeticˉfirst.Fragment);
        var Arithmeticˉsecondˉobject = Nativeˉobjectˉsink.Writeˉwvo(Arithmeticˉsecond.Fragment);
        Sequenceˉequal(Arithmeticˉfirstˉobject, Arithmeticˉsecondˉobject);
        Equal(1871, Arithmeticˉfirst.Fragment.Code.Length);
        Equal(
            NATIVE_ARITHMETIC_CODE_SHA256,
            Objectˉdigest.Calculateˉsha256(Arithmeticˉfirst.Fragment.Code.AsSpan()));
        Equal(1944, Arithmeticˉfirstˉobject.Length);
        Equal(
            NATIVE_ARITHMETIC_WVO_SHA256,
            Objectˉdigest.Calculateˉsha256(Arithmeticˉfirstˉobject.AsSpan()));
        var Arithmeticˉlinked = Linkˉsuccess(
            [Arithmeticˉfirstˉobject.ToArray()],
            new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"));
        Sequenceˉequal(Arithmeticˉfirst.Fragment.Code, Arithmeticˉlinked.Imageˉbytes);
        Equal(42, X64ˉnativeˉexecutor.Executeˉi32(Arithmeticˉfirst.Fragment));
        Equal(42, X64ˉnativeˉexecutor.Executeˉi32(
            Arithmeticˉfirst.Fragment with { Code = Arithmeticˉlinked.Imageˉbytes }));

        var Controlˉverified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(NATIVE_CONTROL_SOURCE));
        var Controlˉinterpreted = new Referenceˉruntime(
            Controlˉverified,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        Equal(42, Controlˉinterpreted.Exitˉcode);
        var Controlˉfirst = X64ˉnativeˉbackend.Compile(Controlˉverified);
        var Controlˉsecond = X64ˉnativeˉbackend.Compile(Controlˉverified);
        var Controlˉfunction = Controlˉfirst.Module.Functions[0];
        var Controlˉoperations = Controlˉfunction.Blocks
            .SelectMany(Block => Block.Operations)
            .ToImmutableArray();
        foreach (var Kind in Enum.GetValues<Nativeˉi32ˉcomparisonˉkind>())
        {
            True(
                Controlˉoperations.OfType<Nativeˉi32ˉcomparison>()
                    .Any(Operation => Operation.Kind == Kind),
                $"Native machine IR did not retain i32 comparison '{Kind}'.");
        }
        foreach (var Kind in Enum.GetValues<Nativeˉboolˉcomparisonˉkind>())
        {
            True(
                Controlˉoperations.OfType<Nativeˉboolˉcomparison>()
                    .Any(Operation => Operation.Kind == Kind),
                $"Native machine IR did not retain bool comparison '{Kind}'.");
        }
        True(
            Controlˉoperations.Any(Operation => Operation is Nativeˉboolˉnot),
            "Native machine IR did not retain bool negation.");
        True(
            Controlˉfunction.Blocks.Any(Block => Block.Terminator is Nativeˉbranch),
            "Native machine IR did not retain a conditional branch.");
        True(
            Controlˉfunction.Blocks.Count(Block => Block.Terminator is Nativeˉreturn) >= 2,
            "Native machine IR did not retain structured early returns.");
        True(
            Controlˉfunction.Valueˉslotˉcount < Controlˉfunction.Valueˉtypes.Length,
            "Native control flow did not reuse physical value slots across empty-stack blocks.");
        Equal(Controlˉfunction.Valueˉtypes.Length, Controlˉfunction.Valueˉslotˉindices.Length);
        Equal(
            Controlˉfunction.Valueˉslotˉcount,
            Controlˉfunction.Valueˉslotˉindices.Distinct().Count());
        True(
            Controlˉfunction.Valueˉtypes
                .Select((Type, Value) => (Type, Slot: Controlˉfunction.Valueˉslotˉindices[Value]))
                .GroupBy(Value => Value.Slot)
                .All(Slot => Slot.Select(Value => Value.Type).Distinct().Count() == 1),
            "A compact native value slot was reused across different machine value types.");
        Sequenceˉequal(
            Controlˉfunction.Valueˉslotˉindices,
            Controlˉsecond.Module.Functions[0].Valueˉslotˉindices);
        Sequenceˉequal(Controlˉfirst.Fragment.Code, Controlˉsecond.Fragment.Code);
        _ = Nativeˉfragmentˉverifier.Verify(Controlˉfirst.Fragment);
        var Controlˉfirstˉobject = Nativeˉobjectˉsink.Writeˉwvo(Controlˉfirst.Fragment);
        var Controlˉsecondˉobject = Nativeˉobjectˉsink.Writeˉwvo(Controlˉsecond.Fragment);
        Sequenceˉequal(Controlˉfirstˉobject, Controlˉsecondˉobject);
        Equal(4835, Controlˉfirst.Fragment.Code.Length);
        Equal(
            NATIVE_CONTROL_CODE_SHA256,
            Objectˉdigest.Calculateˉsha256(Controlˉfirst.Fragment.Code.AsSpan()));
        Equal(4908, Controlˉfirstˉobject.Length);
        Equal(
            NATIVE_CONTROL_WVO_SHA256,
            Objectˉdigest.Calculateˉsha256(Controlˉfirstˉobject.AsSpan()));
        var Controlˉlinked = Linkˉsuccess(
            [Controlˉfirstˉobject.ToArray()],
            new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"));
        Sequenceˉequal(Controlˉfirst.Fragment.Code, Controlˉlinked.Imageˉbytes);
        Equal(42, X64ˉnativeˉexecutor.Executeˉi32(Controlˉfirst.Fragment));
        Equal(42, X64ˉnativeˉexecutor.Executeˉi32(
            Controlˉfirst.Fragment with { Code = Controlˉlinked.Imageˉbytes }));

        var Falseˉverified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(
            "module Nativeˉfalseˉpath profile portable; export fn Main() -> i32 { if 41 == 42 { return 0; } return 42; }"));
        var Falseˉinterpreted = new Referenceˉruntime(
            Falseˉverified,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        Equal(42, Falseˉinterpreted.Exitˉcode);
        var Falseˉnative = X64ˉnativeˉbackend.Compile(Falseˉverified);
        Equal(42, X64ˉnativeˉexecutor.Executeˉi32(Falseˉnative.Fragment));
        var Falseˉobject = Nativeˉobjectˉsink.Writeˉwvo(Falseˉnative.Fragment);
        var Falseˉlinked = Linkˉsuccess(
            [Falseˉobject.ToArray()],
            new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"));
        Equal(42, X64ˉnativeˉexecutor.Executeˉi32(
            Falseˉnative.Fragment with { Code = Falseˉlinked.Imageˉbytes }));

        var Corruptedˉbudgetˉsource = Controlˉfirst.Fragment.Code.ToArray();
        Corruptedˉbudgetˉsource[0] = 0x90;
        Throwsˉnative(
            "WVN3030",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                Controlˉfirst.Fragment with { Code = Corruptedˉbudgetˉsource.ToImmutableArray() }));

        var Corruptedˉzero = Controlˉfirst.Fragment.Code.ToArray();
        var Zeroˉinstruction = Corruptedˉzero.AsSpan().IndexOf(new byte[] { 0x31, 0xC0 });
        True(Zeroˉinstruction >= 0, "Native control fragment did not contain frame zeroing.");
        Corruptedˉzero[Zeroˉinstruction] = 0x90;
        Throwsˉnative(
            "WVN3030",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                Controlˉfirst.Fragment with { Code = Corruptedˉzero.ToImmutableArray() }));

        var Corruptedˉcharge = Controlˉfirst.Fragment.Code.ToArray();
        var Chargeˉinstruction = Corruptedˉcharge.AsSpan().IndexOf(
            new byte[] { 0x49, 0x83, 0xEB, 0x01, 0x0F, 0x82 });
        True(Chargeˉinstruction >= 0, "Native control fragment did not contain an instruction charge.");
        Corruptedˉcharge[Chargeˉinstruction + 3] = 0x02;
        Throwsˉnative(
            "WVN3030",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                Controlˉfirst.Fragment with { Code = Corruptedˉcharge.ToImmutableArray() }));

        var Corruptedˉlimitˉtarget = Controlˉfirst.Fragment.Code.ToArray();
        BinaryPrimitives.WriteInt32LittleEndian(
            Corruptedˉlimitˉtarget.AsSpan(Chargeˉinstruction + 6, sizeof(int)),
            0);
        Throwsˉnative(
            "WVN3030",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                Controlˉfirst.Fragment with { Code = Corruptedˉlimitˉtarget.ToImmutableArray() }));

        var Corruptedˉcondition = Controlˉfirst.Fragment.Code.ToArray();
        var Comparisonˉinstruction = Corruptedˉcondition.AsSpan().IndexOf(new byte[] { 0x39, 0xC8, 0x0F });
        True(Comparisonˉinstruction >= 0, "Native control fragment did not contain a comparison instruction.");
        Corruptedˉcondition[Comparisonˉinstruction + 3] = 0x90;
        Throwsˉnative(
            "WVN3030",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                Controlˉfirst.Fragment with { Code = Corruptedˉcondition.ToImmutableArray() }));

        var Corruptedˉtarget = Controlˉfirst.Fragment.Code.ToArray();
        var Conditionalˉbranch = Corruptedˉtarget.AsSpan().IndexOf(new byte[] { 0x0F, 0x85 });
        True(Conditionalˉbranch >= 0, "Native control fragment did not contain a conditional branch.");
        BinaryPrimitives.WriteInt32LittleEndian(
            Corruptedˉtarget.AsSpan(Conditionalˉbranch + 2, sizeof(int)),
            0);
        Throwsˉnative(
            "WVN3030",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                Controlˉfirst.Fragment with { Code = Corruptedˉtarget.ToImmutableArray() }));

        var Corruptedˉnonˉchargeˉtarget = Controlˉfirst.Fragment.Code.ToArray();
        var Frameˉprologue = Corruptedˉnonˉchargeˉtarget.AsSpan().IndexOf(
            new byte[] { 0x48, 0x81, 0xEC });
        True(Frameˉprologue >= 0, "Native control fragment did not contain a frame prologue.");
        var Frameˉbytes = BinaryPrimitives.ReadInt32LittleEndian(
            Corruptedˉnonˉchargeˉtarget.AsSpan(Frameˉprologue + 3, sizeof(int)));
        var Bodyˉoffset = Frameˉprologue + 9 + (Frameˉbytes / sizeof(int) * 7);
        var Falseˉbranchˉdisplacement = Conditionalˉbranch + 7;
        BinaryPrimitives.WriteInt32LittleEndian(
            Corruptedˉnonˉchargeˉtarget.AsSpan(Falseˉbranchˉdisplacement, sizeof(int)),
            Bodyˉoffset + 10 - (Falseˉbranchˉdisplacement + sizeof(int)));
        Throwsˉnative(
            "WVN3030",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                Controlˉfirst.Fragment with { Code = Corruptedˉnonˉchargeˉtarget.ToImmutableArray() }));

        var Minimumˉverified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(
            "module Nativeˉminimum profile portable; export fn Main() -> i32 { return -2147483647 - 1; }"));
        var Minimumˉinterpreted = new Referenceˉruntime(
            Minimumˉverified,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        Equal(int.MinValue, Minimumˉinterpreted.Exitˉcode);
        Equal(
            int.MinValue,
            X64ˉnativeˉexecutor.Executeˉi32(X64ˉnativeˉbackend.Compile(Minimumˉverified).Fragment));

        var Corruptedˉarithmeticˉcode = Arithmeticˉfirst.Fragment.Code.ToArray();
        var Overflowˉbranch = Corruptedˉarithmeticˉcode.AsSpan().IndexOf(new byte[] { 0x0F, 0x80 });
        True(Overflowˉbranch >= 0, "Native checked arithmetic did not contain an overflow branch.");
        BinaryPrimitives.WriteInt32LittleEndian(
            Corruptedˉarithmeticˉcode.AsSpan(Overflowˉbranch + 2, sizeof(int)),
            0);
        Throwsˉnative(
            "WVN3030",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                Arithmeticˉfirst.Fragment with { Code = Corruptedˉarithmeticˉcode.ToImmutableArray() }));

        var Arithmeticˉmain = Arithmeticˉfirst.Fragment.Symbols.Single(Symbol => Symbol.Name == "Main");
        var Arithmeticˉend = checked((int)(Arithmeticˉmain.Offset + Arithmeticˉmain.Size));
        var Propagateˉoffset = Arithmeticˉend - 88;
        var Trapˉoffset = Propagateˉoffset + 11;
        var Arithmeticˉframeˉprologue = Arithmeticˉfirst.Fragment.Code.AsSpan().IndexOf(
            new byte[] { 0x48, 0x81, 0xEC });
        var Structuralˉcorruptions = new Action<byte[]>[]
        {
            Code => Code[0] = 0x90,
            Code => BinaryPrimitives.WriteInt32LittleEndian(
                Code.AsSpan(Arithmeticˉframeˉprologue + 3, sizeof(int)),
                Nativeˉcontract.MAXIMUM_FRAME_BYTES + 16),
            Code => Code[Propagateˉoffset - 1] = 0x90,
            Code => Code[Trapˉoffset + 12] ^= 0x01,
        };
        foreach (var Corrupt in Structuralˉcorruptions)
        {
            var Corruptedˉcode = Arithmeticˉfirst.Fragment.Code.ToArray();
            Corrupt(Corruptedˉcode);
            Throwsˉnative(
                "WVN3030",
                () => _ = Nativeˉfragmentˉverifier.Verify(
                    Arithmeticˉfirst.Fragment with { Code = Corruptedˉcode.ToImmutableArray() }));
        }

        var Instructionˉlimitˉoffset = Trapˉoffset + 21;
        var Corruptedˉlimitˉstatus = Arithmeticˉfirst.Fragment.Code.ToArray();
        Corruptedˉlimitˉstatus[Instructionˉlimitˉoffset + 12] ^= 0x01;
        Throwsˉnative(
            "WVN3030",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                Arithmeticˉfirst.Fragment with { Code = Corruptedˉlimitˉstatus.ToImmutableArray() }));

        var Overflowˉsources = new[]
        {
            "module Nativeˉaddˉoverflow profile portable; export fn Main() -> i32 { return 2147483647 + 1; }",
            "module Nativeˉsubtractˉoverflow profile portable; export fn Main() -> i32 { return -2147483647 - 2; }",
            "module Nativeˉmultiplyˉoverflow profile portable; export fn Main() -> i32 { return 50000 * 50000; }",
            "module Nativeˉnegateˉoverflow profile portable; export fn Main() -> i32 { return -(-2147483647 - 1); }",
        };
        foreach (var Overflowˉsource in Overflowˉsources)
        {
            var Overflowˉverified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(Overflowˉsource));
            Throwsˉruntime(
                "WVR3007",
                () => _ = new Referenceˉruntime(
                    Overflowˉverified,
                    new Referenceˉcapabilityˉhost(TextWriter.Null),
                    Runtimeˉoptions.Portableˉdefaults).Runˉmain());
            var Overflowˉnative = X64ˉnativeˉbackend.Compile(Overflowˉverified);
            Throwsˉnativeˉtrap(
                "WVR3007",
                () => _ = X64ˉnativeˉexecutor.Executeˉi32(Overflowˉnative.Fragment));
        }

        var Aotˉoverflowˉverified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(Overflowˉsources[0]));
        var Aotˉoverflow = X64ˉnativeˉbackend.Compile(Aotˉoverflowˉverified);
        var Aotˉoverflowˉobject = Nativeˉobjectˉsink.Writeˉwvo(Aotˉoverflow.Fragment);
        var Aotˉoverflowˉlinked = Linkˉsuccess(
            [Aotˉoverflowˉobject.ToArray()],
            new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"));
        Throwsˉnativeˉtrap(
            "WVR3007",
            () => _ = X64ˉnativeˉexecutor.Executeˉi32(
                Aotˉoverflow.Fragment with { Code = Aotˉoverflowˉlinked.Imageˉbytes }));

        var Invalidˉfragment = First.Fragment with
        {
            Patches = [new(Nativeˉpatchˉkind.Relativeˉi32, (uint)First.Fragment.Code.Length - 2, "Main", -4)],
        };
        Throwsˉnative("WVN3020", () => _ = Nativeˉfragmentˉverifier.Verify(Invalidˉfragment));

        var Invalidˉbytes = First.Fragment.Code.ToArray();
        Invalidˉbytes[0] = 0x90;
        var Invalidˉcode = First.Fragment with { Code = Invalidˉbytes.ToImmutableArray() };
        Throwsˉnative("WVN3030", () => _ = X64ˉnativeˉexecutor.Executeˉi32(Invalidˉcode));

        var Loopˉverified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(NATIVE_LOOP_SOURCE));
        var Loopˉinterpreted = new Referenceˉruntime(
            Loopˉverified,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        Equal(42, Loopˉinterpreted.Exitˉcode);
        var Loopˉnative = X64ˉnativeˉbackend.Compile(Loopˉverified);
        True(
            Loopˉnative.Module.Functions[0].Blocks.Any(Block =>
                Block.Terminator switch
                {
                    Nativeˉjump Jump => Jump.Targetˉblock <= Block.Id,
                    Nativeˉbranch Branch =>
                        Branch.Trueˉblock <= Block.Id || Branch.Falseˉblock <= Block.Id,
                    _ => false,
                }),
            "Native machine IR did not retain a backward loop edge.");
        Equal(42, X64ˉnativeˉexecutor.Executeˉi32(
            Loopˉnative.Fragment,
            maximumˉinstructions: Loopˉinterpreted.Executedˉinstructions));
        Throwsˉnativeˉtrap(
            "WVR3011",
            () => _ = X64ˉnativeˉexecutor.Executeˉi32(
                Loopˉnative.Fragment,
                maximumˉinstructions: Loopˉinterpreted.Executedˉinstructions - 1));
        Throwsˉruntime(
            "WVR3011",
            () => _ = new Referenceˉruntime(
                Loopˉverified,
                new Referenceˉcapabilityˉhost(TextWriter.Null),
                Runtimeˉoptions.Portableˉdefaults with
                {
                    Maximumˉinstructions = Loopˉinterpreted.Executedˉinstructions - 1,
                }).Runˉmain());
        var Loopˉobject = Nativeˉobjectˉsink.Writeˉwvo(Loopˉnative.Fragment);
        var Loopˉlinked = Linkˉsuccess(
            [Loopˉobject.ToArray()],
            new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"));
        var Loopˉaot = Loopˉnative.Fragment with { Code = Loopˉlinked.Imageˉbytes };
        Equal(42, X64ˉnativeˉexecutor.Executeˉi32(
            Loopˉaot,
            maximumˉinstructions: Loopˉinterpreted.Executedˉinstructions));
        Throwsˉnativeˉtrap(
            "WVR3011",
            () => _ = X64ˉnativeˉexecutor.Executeˉi32(
                Loopˉaot,
                maximumˉinstructions: Loopˉinterpreted.Executedˉinstructions - 1));
        Equal(157L, Loopˉinterpreted.Executedˉinstructions);
        Equal(1665, Loopˉnative.Fragment.Code.Length);
        Equal(
            NATIVE_LOOP_CODE_SHA256,
            Objectˉdigest.Calculateˉsha256(Loopˉnative.Fragment.Code.AsSpan()));
        Equal(1738, Loopˉobject.Length);
        Equal(
            NATIVE_LOOP_WVO_SHA256,
            Objectˉdigest.Calculateˉsha256(Loopˉobject.AsSpan()));

        var Nonterminatingˉverified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(
            "module Nativeˉnonterminating profile portable; export fn Main() -> i32 { var Value: i32 = 0; while Value == 0 { Value = Value; } return 1; }"));
        Throwsˉruntime(
            "WVR3011",
            () => _ = new Referenceˉruntime(
                Nonterminatingˉverified,
                new Referenceˉcapabilityˉhost(TextWriter.Null),
                Runtimeˉoptions.Portableˉdefaults with { Maximumˉinstructions = 50 }).Runˉmain());
        var Nonterminatingˉnative = X64ˉnativeˉbackend.Compile(Nonterminatingˉverified);
        Throwsˉnativeˉtrap(
            "WVR3011",
            () => _ = X64ˉnativeˉexecutor.Executeˉi32(
                Nonterminatingˉnative.Fragment,
                maximumˉinstructions: 50));

        var Callˉverified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(
            "module Nativeˉcalls profile portable; export fn Main() -> i32 { return Answer(); } fn Answer() -> i32 { return 42; }"));
        var Callˉinterpreted = new Referenceˉruntime(
            Callˉverified,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        var Callˉnative = X64ˉnativeˉbackend.Compile(Callˉverified);
        Equal(2, Callˉnative.Module.Functions.Length);
        True(
            Callˉnative.Module.Functions
                .SelectMany(Function => Function.Blocks)
                .SelectMany(Block => Block.Operations)
                .Any(Operation => Operation is Nativeˉcall { Function: 0, Arguments.Length: 0 }),
            "Native machine IR did not retain the zero-argument function call.");
        Equal(42, X64ˉnativeˉexecutor.Executeˉi32(
            Callˉnative.Fragment,
            maximumˉinstructions: Callˉinterpreted.Executedˉinstructions,
            maximumˉcallˉdepth: 2));
        Throwsˉnativeˉtrap(
            "WVR3011",
            () => _ = X64ˉnativeˉexecutor.Executeˉi32(
                Callˉnative.Fragment,
                maximumˉinstructions: Callˉinterpreted.Executedˉinstructions - 1,
                maximumˉcallˉdepth: 2));
        Throwsˉnativeˉtrap(
            "WVR3004",
            () => _ = X64ˉnativeˉexecutor.Executeˉi32(
                Callˉnative.Fragment,
                maximumˉinstructions: Callˉinterpreted.Executedˉinstructions,
                maximumˉcallˉdepth: 1));
        var Invalidˉcallˉdepthˉrejected = false;
        try
        {
            _ = X64ˉnativeˉexecutor.Executeˉi32(Callˉnative.Fragment, maximumˉcallˉdepth: 0);
        }
        catch (ArgumentOutOfRangeException)
        {
            Invalidˉcallˉdepthˉrejected = true;
        }
        True(Invalidˉcallˉdepthˉrejected, "The native executor accepted a non-positive call-depth limit.");

        var Boolˉcallˉverified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(
            "module Nativeˉboolˉcall profile portable; fn Isˉanswer(Value: i32) -> bool { return Value == 42; } export fn Main() -> i32 { if Isˉanswer(42) { return 42; } return 0; }"));
        Equal(
            42,
            X64ˉnativeˉexecutor.Executeˉi32(
                X64ˉnativeˉbackend.Compile(Boolˉcallˉverified).Fragment));

        var Argumentˉcallˉverified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(
            "module Nativeˉarguments profile portable; fn Add(Left: i32, Right: i32) -> i32 { return Left + Right; } export fn Main() -> i32 { return Add(20, 22); }"));
        Equal(
            42,
            X64ˉnativeˉexecutor.Executeˉi32(X64ˉnativeˉbackend.Compile(Argumentˉcallˉverified).Fragment));

        var Nestedˉcallˉverified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(
            "module Nativeˉnestedˉcalls profile portable; fn Add(Left: i32, Right: i32) -> i32 { return Left + Right; } export fn Main() -> i32 { return Add(Add(Add(3, 5), 8), 13); }"));
        var Nestedˉcallˉresult = X64ˉnativeˉexecutor.Executeˉi32(
            X64ˉnativeˉbackend.Compile(Nestedˉcallˉverified).Fragment);
        True(Nestedˉcallˉresult == 29, $"Nested call result was {Nestedˉcallˉresult}; expected 29.");

        var Leftˉargumentˉverified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(
            "module Nativeˉleftˉargument profile portable; fn First(Left: i32, Right: i32) -> i32 { return Left; } export fn Main() -> i32 { return First(16, 3); }"));
        var Leftˉargumentˉresult = X64ˉnativeˉexecutor.Executeˉi32(
            X64ˉnativeˉbackend.Compile(Leftˉargumentˉverified).Fragment);
        True(Leftˉargumentˉresult == 16, $"First call argument was {Leftˉargumentˉresult}; expected 16.");

        var Directˉdataˉverified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(
            "module Nativeˉdirectˉdata profile portable; data Values: [i32] = [3, 5, 8, 13]; export fn Main() -> i32 { return Values[3]; }"));
        Equal(
            13,
            X64ˉnativeˉexecutor.Executeˉi32(X64ˉnativeˉbackend.Compile(Directˉdataˉverified).Fragment));

        var Shiftedˉdataˉverified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(
            "module Nativeˉshiftedˉdata profile portable; data Values: [i32] = [3, 5, 8, 13]; fn Add(Left: i32, Right: i32) -> i32 { return Left + Right; } export fn Main() -> i32 { return Values[3]; }"));
        var Shiftedˉdataˉresult = X64ˉnativeˉexecutor.Executeˉi32(
            X64ˉnativeˉbackend.Compile(Shiftedˉdataˉverified).Fragment);
        True(Shiftedˉdataˉresult == 13, $"Shifted static-data result was {Shiftedˉdataˉresult}; expected 13.");

        var Preservedˉlocalˉverified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(
            "module Nativeˉpreservedˉlocal profile portable; data Values: [i32] = [3, 5, 8, 13]; export fn Main() -> i32 { let Left: i32 = 16; let Right: i32 = Values[3]; return Left; }"));
        var Preservedˉlocalˉresult = X64ˉnativeˉexecutor.Executeˉi32(
            X64ˉnativeˉbackend.Compile(Preservedˉlocalˉverified).Fragment);
        True(Preservedˉlocalˉresult == 16, $"Static-data load changed an unrelated local to {Preservedˉlocalˉresult}.");

        var Dataˉargumentˉverified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(
            "module Nativeˉdataˉargument profile portable; data Values: [i32] = [3, 5, 8, 13]; fn Add(Left: i32, Right: i32) -> i32 { return Left + Right; } export fn Main() -> i32 { return Add(16, Values[3]); }"));
        Equal(
            29,
            X64ˉnativeˉexecutor.Executeˉi32(
                X64ˉnativeˉbackend.Compile(Dataˉargumentˉverified).Fragment));

        var Loopˉdataˉverified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess("""
            module Nativeˉloopˉdata profile portable;
            data Values: [i32] = [3, 5, 8, 13];
            export fn Main() -> i32 {
                var Index: i32 = 0;
                var Total: i32 = 0;
                while Index < length(Values) {
                    Total = Total + Values[Index];
                    Index = Index + 1;
                }
                return Total;
            }
            """));
        Equal(
            29,
            X64ˉnativeˉexecutor.Executeˉi32(X64ˉnativeˉbackend.Compile(Loopˉdataˉverified).Fragment));

        var Unrolledˉverified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess("""
            module Nativeˉunrolled profile portable;
            data Values: [i32] = [3, 5, 8, 13];
            fn Add(Left: i32, Right: i32) -> i32 { return Left + Right; }
            export fn Main() -> i32 { return Add(Add(Add(Values[0], Values[1]), Values[2]), Values[3]); }
            """));
        var Unrolledˉnative = X64ˉnativeˉbackend.Compile(Unrolledˉverified);
        Equal(29, X64ˉnativeˉexecutor.Executeˉi32(Unrolledˉnative.Fragment));

        var Sumˉverified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(SUM_SOURCE));
        var Sumˉinterpreted = new Referenceˉruntime(
            Sumˉverified,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        Equal(29, Sumˉinterpreted.Exitˉcode);
        var Sumˉnative = X64ˉnativeˉbackend.Compile(Sumˉverified);
        Equal(2, Sumˉnative.Module.Functions.Length);
        Equal(1, Sumˉnative.Module.Data.Length);
        True(
            Sumˉnative.Module.Data[0] is Nativeˉi32ˉdata,
            "Native machine IR did not retain immutable i32 data.");
        Sequenceˉequal([3, 5, 8, 13], ((Nativeˉi32ˉdata)Sumˉnative.Module.Data[0]).Values);
        True(
            Sumˉnative.Module.Functions
                .SelectMany(Function => Function.Blocks)
                .SelectMany(Block => Block.Operations)
                .Any(Operation => Operation is Nativeˉcall { Arguments.Length: 2 }),
            "Native machine IR did not retain typed call arguments.");
        True(
            Sumˉnative.Module.Functions
                .SelectMany(Function => Function.Blocks)
                .SelectMany(Block => Block.Operations)
                .Any(Operation => Operation is Nativeˉdataˉloadˉi32),
            "Native machine IR did not retain the bounds-checked static-data load.");
        var Sumˉjitˉresult = X64ˉnativeˉexecutor.Executeˉi32(
            Sumˉnative.Fragment,
            maximumˉinstructions: Sumˉinterpreted.Executedˉinstructions,
            maximumˉcallˉdepth: 2);
        True(Sumˉjitˉresult == 29, $"Looped call/data result was {Sumˉjitˉresult}; expected 29.");
        Throwsˉnativeˉtrap(
            "WVR3011",
            () => _ = X64ˉnativeˉexecutor.Executeˉi32(
                Sumˉnative.Fragment,
                maximumˉinstructions: Sumˉinterpreted.Executedˉinstructions - 1,
                maximumˉcallˉdepth: 2));
        var Sumˉobject = Nativeˉobjectˉsink.Writeˉwvo(Sumˉnative.Fragment);
        var Sumˉverifiedˉobject = Objectˉcodec.Readˉandˉverify(Sumˉobject.AsSpan()).Value;
        Equal(2, Sumˉverifiedˉobject.Sections.Length);
        Equal(Objectˉsectionˉkind.Readˉonlyˉdata, Sumˉverifiedˉobject.Sections[1].Kind);
        Equal(1, Sumˉverifiedˉobject.Relocations.Length);
        Equal(Objectˉrelocationˉkind.Relativeˉi32, Sumˉverifiedˉobject.Relocations[0].Kind);
        var Sumˉlinked = Linkˉsuccess(
            [Sumˉobject.ToArray()],
            new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"));
        Sequenceˉequal(Sumˉnative.Fragment.Code, Sumˉlinked.Imageˉbytes);
        Equal(29, X64ˉnativeˉexecutor.Executeˉi32(
            Sumˉnative.Fragment with { Code = Sumˉlinked.Imageˉbytes },
            maximumˉinstructions: Sumˉinterpreted.Executedˉinstructions,
            maximumˉcallˉdepth: 2));

        var Corruptedˉdataˉpatch = Sumˉnative.Fragment.Code.ToArray();
        var Dataˉpatch = Sumˉnative.Fragment.Patches.Single();
        Corruptedˉdataˉpatch[checked((int)Dataˉpatch.Offset)] ^= 0x01;
        Throwsˉnative(
            "WVN3024",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                Sumˉnative.Fragment with { Code = Corruptedˉdataˉpatch.ToImmutableArray() }));

        var Boundsˉverified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(
            "module Nativeˉbounds profile portable; data Values: [i32] = [42]; export fn Main() -> i32 { return Values[1]; }"));
        Throwsˉruntime(
            "WVR3005",
            () => _ = new Referenceˉruntime(
                Boundsˉverified,
                new Referenceˉcapabilityˉhost(TextWriter.Null),
                Runtimeˉoptions.Portableˉdefaults).Runˉmain());
        Throwsˉnativeˉtrap(
            "WVR3005",
            () => _ = X64ˉnativeˉexecutor.Executeˉi32(
                X64ˉnativeˉbackend.Compile(Boundsˉverified).Fragment));

        var Recursiveˉverified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess("""
            module Nativeˉrecursive profile portable;
            fn Descend(Value: i32) -> i32 {
                if Value == 0 { return 42; }
                return Descend(Value - 1);
            }
            export fn Main() -> i32 { return Descend(3); }
            """));
        var Recursiveˉinterpreted = new Referenceˉruntime(
            Recursiveˉverified,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        var Recursiveˉnative = X64ˉnativeˉbackend.Compile(Recursiveˉverified);
        Equal(42, X64ˉnativeˉexecutor.Executeˉi32(
            Recursiveˉnative.Fragment,
            maximumˉinstructions: Recursiveˉinterpreted.Executedˉinstructions,
            maximumˉcallˉdepth: 5));
        Throwsˉnativeˉtrap(
            "WVR3004",
            () => _ = X64ˉnativeˉexecutor.Executeˉi32(
                Recursiveˉnative.Fragment,
                maximumˉinstructions: Recursiveˉinterpreted.Executedˉinstructions,
                maximumˉcallˉdepth: 4));

        var Fourˉparameterˉverified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(
            "module Nativeˉfourˉparameter profile portable; fn Sum(A: i32, B: i32, C: i32, D: i32) -> i32 { return A + B + C + D; } export fn Main() -> i32 { return Sum(9, 10, 11, 12); }"));
        Equal(
            42,
            X64ˉnativeˉexecutor.Executeˉi32(
                X64ˉnativeˉbackend.Compile(Fourˉparameterˉverified).Fragment));

        var Fiveˉparameterˉverified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(
            "module Nativeˉwideˉcall profile portable; fn Sum(A: i32, B: i32, C: i32, D: i32, E: i32) -> i32 { return A + B + C + D + E; } export fn Main() -> i32 { return Sum(1, 2, 3, 4, 5); }"));
        Equal(
            15,
            X64ˉnativeˉexecutor.Executeˉi32(
                X64ˉnativeˉbackend.Compile(Fiveˉparameterˉverified).Fragment));
    }

    private static void Nativeˉwideˉcallsˉagree()
    {
        var Parameters = string.Join(
            ", ",
            Enumerable.Range(0, Nativeˉcontract.MAXIMUM_CALL_PARAMETERS)
                .Select(Index => $"P{Index:D2}: i32"));
        var Arguments = string.Join(
            ", ",
            Enumerable.Range(1, Nativeˉcontract.MAXIMUM_CALL_PARAMETERS));
        var Wideˉsource = $$"""
            module Nativeˉmaximumˉwideˉcall profile portable;
            fn Select({{Parameters}}) -> i32 { return P00 + P31 + P63; }
            export fn Main() -> i32 { return Select({{Arguments}}); }
            """;
        var Wideˉverified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(Wideˉsource));
        var Wideˉreference = new Referenceˉruntime(
            Wideˉverified,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        Equal(97, Wideˉreference.Exitˉcode);
        var Wideˉfirst = X64ˉnativeˉbackend.Compile(Wideˉverified);
        var Wideˉsecond = X64ˉnativeˉbackend.Compile(Wideˉverified);
        Sequenceˉequal(Wideˉfirst.Fragment.Code, Wideˉsecond.Fragment.Code);
        True(
            Wideˉfirst.Module.Functions
                .SelectMany(Function => Function.Blocks)
                .SelectMany(Block => Block.Operations)
                .Any(Operation => Operation is Nativeˉcall
                    { Arguments.Length: Nativeˉcontract.MAXIMUM_CALL_PARAMETERS }),
            "Native machine IR omitted the bounded maximum-width call.");
        Equal(
            Wideˉreference.Exitˉcode,
            X64ˉnativeˉexecutor.Executeˉi32(
                Wideˉfirst.Fragment,
                maximumˉinstructions: Wideˉreference.Executedˉinstructions));
        var Wideˉobject = Nativeˉobjectˉsink.Writeˉwvo(Wideˉfirst.Fragment);
        var Wideˉlinked = Linkˉsuccess(
            [Wideˉobject.ToArray()],
            new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"));
        Equal(
            Wideˉreference.Exitˉcode,
            X64ˉnativeˉexecutor.Executeˉi32(
                Wideˉfirst.Fragment with { Code = Wideˉlinked.Imageˉbytes },
                maximumˉinstructions: Wideˉreference.Executedˉinstructions));

        var Mainˉsymbol = Wideˉfirst.Fragment.Symbols.Single(
            Symbol => Symbol.Binding == Nativeˉsymbolˉbinding.Export);
        var Mainˉoffset = checked((int)Mainˉsymbol.Offset);
        var Mainˉcode = Wideˉfirst.Fragment.Code.AsSpan(
            Mainˉoffset,
            checked((int)Mainˉsymbol.Size));
        var Stackˉreserve = Mainˉcode.IndexOf(new byte[]
        {
            0x48, 0x81, 0xEC, 0xC0, 0x03, 0x00, 0x00,
        });
        True(Stackˉreserve >= 0, "Native maximum-width call omitted its bounded stack reservation.");
        var Stackˉrelease = Mainˉcode.IndexOf(new byte[]
        {
            0x48, 0x81, 0xC4, 0xC0, 0x03, 0x00, 0x00,
        });
        True(Stackˉrelease >= 0, "Native maximum-width call omitted its bounded stack release.");

        var Corruptedˉreserve = Wideˉfirst.Fragment.Code.ToArray();
        Corruptedˉreserve[Mainˉoffset + Stackˉreserve + 3] ^= 0x10;
        Throwsˉnative(
            "WVN3030",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                Wideˉfirst.Fragment with { Code = Corruptedˉreserve.ToImmutableArray() }));

        var Corruptedˉoutgoingˉslot = Wideˉfirst.Fragment.Code.ToArray();
        Corruptedˉoutgoingˉslot[Mainˉoffset + Stackˉreserve + 17] ^= 0x01;
        Throwsˉnative(
            "WVN3030",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                Wideˉfirst.Fragment with { Code = Corruptedˉoutgoingˉslot.ToImmutableArray() }));

        var Corruptedˉrelease = Wideˉfirst.Fragment.Code.ToArray();
        Corruptedˉrelease[Mainˉoffset + Stackˉrelease + 3] ^= 0x10;
        Throwsˉnative(
            "WVN3030",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                Wideˉfirst.Fragment with { Code = Corruptedˉrelease.ToImmutableArray() }));

        var Descriptorˉverified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess("""
            module Nativeˉwideˉdescriptor profile portable;
            data Expected: bytes = [0, 42, 255];
            fn Fifth(A: i32, B: i32, C: i32, D: i32, Value: bytes) -> bytes {
                return Value;
            }
            export fn Main() -> bytes { return Fifth(1, 2, 3, 4, Expected); }
            """));
        var Descriptorˉnative = X64ˉnativeˉbackend.Compile(Descriptorˉverified);
        Sequenceˉequal(
            new byte[] { 0, 42, 255 },
            X64ˉnativeˉexecutor.Executeˉbytes(Descriptorˉnative.Fragment));
        var Descriptorˉobject = Nativeˉobjectˉsink.Writeˉwvo(Descriptorˉnative.Fragment);
        var Descriptorˉlinked = Linkˉsuccess(
            [Descriptorˉobject.ToArray()],
            new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"));
        Sequenceˉequal(
            new byte[] { 0, 42, 255 },
            X64ˉnativeˉexecutor.Executeˉbytes(
                Descriptorˉnative.Fragment with { Code = Descriptorˉlinked.Imageˉbytes }));

        var Descriptorˉmain = Descriptorˉnative.Fragment.Symbols.Single(
            Symbol => Symbol.Binding == Nativeˉsymbolˉbinding.Export);
        var Descriptorˉmainˉoffset = checked((int)Descriptorˉmain.Offset);
        var Descriptorˉreserve = Descriptorˉnative.Fragment.Code.AsSpan(
                Descriptorˉmainˉoffset,
                checked((int)Descriptorˉmain.Size))
            .IndexOf(new byte[] { 0x48, 0x81, 0xEC, 0x10, 0x00, 0x00, 0x00 });
        True(Descriptorˉreserve >= 0, "Native descriptor call omitted its stack cell.");
        var Corruptedˉdescriptorˉcell = Descriptorˉnative.Fragment.Code.ToArray();
        Corruptedˉdescriptorˉcell[Descriptorˉmainˉoffset + Descriptorˉreserve + 35] ^= 0x01;
        Throwsˉnative(
            "WVN3030",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                Descriptorˉnative.Fragment with
                {
                    Code = Corruptedˉdescriptorˉcell.ToImmutableArray(),
                }));

        var Voidˉverified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess("""
            module Nativeˉwideˉvoid profile portable;
            fn Ignore(A: i32, B: i32, C: i32, D: i32, E: i32) -> void { return; }
            export fn Main() -> i32 { Ignore(1, 2, 3, 4, 5); return 42; }
            """));
        Equal(
            42,
            X64ˉnativeˉexecutor.Executeˉi32(
                X64ˉnativeˉbackend.Compile(Voidˉverified).Fragment));
    }

    private static void Nativeˉnominalˉvaluesˉagree()
    {
        var Verified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(NATIVE_NOMINAL_SOURCE));
        var Interpreted = new Referenceˉruntime(
            Verified,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        Equal(42, Interpreted.Exitˉcode);

        var First = X64ˉnativeˉbackend.Compile(Verified);
        var Second = X64ˉnativeˉbackend.Compile(Verified);
        Sequenceˉequal(First.Fragment.Code, Second.Fragment.Code);
        Equal(2, First.Module.Types.Length);
        True(
            First.Module.Functions.SelectMany(Function => Function.Blocks)
                .SelectMany(Block => Block.Operations)
                .Any(Operation => Operation is Nativeˉenumˉconstant),
            "Native machine IR omitted enum constants.");
        True(
            First.Module.Functions.SelectMany(Function => Function.Blocks)
                .SelectMany(Block => Block.Operations)
                .Any(Operation => Operation is Nativeˉenumˉcomparison),
            "Native machine IR omitted enum comparisons.");
        True(
            First.Module.Functions.SelectMany(Function => Function.Blocks)
                .SelectMany(Block => Block.Operations)
                .Any(Operation => Operation is Nativeˉrecordˉcreate),
            "Native machine IR omitted record construction.");
        True(
            First.Module.Functions.SelectMany(Function => Function.Blocks)
                .SelectMany(Block => Block.Operations)
                .Any(Operation => Operation is Nativeˉrecordˉfield),
            "Native machine IR omitted record field access.");
        _ = Nativeˉfragmentˉverifier.Verify(First.Fragment);
        Equal(Interpreted.Exitˉcode, X64ˉnativeˉexecutor.Executeˉi32(First.Fragment));

        var Object = Nativeˉobjectˉsink.Writeˉwvo(First.Fragment);
        var Linked = Linkˉsuccess(
            [Object.ToArray()],
            new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"));
        Sequenceˉequal(First.Fragment.Code, Linked.Imageˉbytes);
        Equal(
            Interpreted.Exitˉcode,
            X64ˉnativeˉexecutor.Executeˉi32(First.Fragment with { Code = Linked.Imageˉbytes }));

        var Corruptedˉrecord = First.Fragment.Code.ToArray();
        var Recordˉcreate = Corruptedˉrecord.AsSpan().IndexOf(new byte[]
        {
            0x41, 0x8B, 0x47, Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_USED_OFFSET,
            0x89, 0xC1, 0x81, 0xC1,
        });
        True(Recordˉcreate >= 0, "Native nominal fragment omitted the record arena allocation sequence.");
        BinaryPrimitives.WriteInt32LittleEndian(
            Corruptedˉrecord.AsSpan(Recordˉcreate + 8, sizeof(int)),
            0);
        Throwsˉnative(
            "WVN3030",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                First.Fragment with { Code = Corruptedˉrecord.ToImmutableArray() }));

        var Nonzeroˉdefault = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess("""
            module Nativeˉnonzeroˉenum profile portable;
            enum Nativeˉstate { Ready = 1; }
            export fn Main() -> i32 { return 0; }
            """));
        Throwsˉnative("WVN2001", () => _ = X64ˉnativeˉbackend.Compile(Nonzeroˉdefault));

        var Arenaˉexhaustion = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess("""
            module Nativeˉrecordˉarena profile portable;
            record Nativeˉcell { Value: i32; }
            export fn Main() -> i32 {
                var Index: i32 = 0;
                var Cell: Nativeˉcell = Nativeˉcell(0);
                while Index < 65536 {
                    Cell = Nativeˉcell(Index);
                    Index = Index + 1;
                }
                return Cell.Value;
            }
            """));
        Throwsˉnativeˉtrap(
            "WVR3017",
            () => _ = X64ˉnativeˉexecutor.Executeˉi32(
                X64ˉnativeˉbackend.Compile(Arenaˉexhaustion).Fragment,
                maximumˉinstructions: 2_000_000));
    }

    private static void Nativeˉdynamicˉtextˉagrees()
    {
        const string Source = """
            module Nativeˉdynamicˉtext profile hosted;
            capability console.write_line;
            capability diagnostic.write_line;
            record Nativeˉtag { Value: i32; }
            enum Nativeˉstate { Ready = 0; Running = 1; }
            enum Nativeˉedge { Zero = 0; Second = 2; Maximum = 2147483647; }
            data Euro: bytes = [226, 130, 172];
            data Quoteˉedges: bytes = [34, 92, 8, 12, 10, 13, 9, 0, 31, 32, 126, 127, 195, 169, 226, 130, 172, 240, 159, 152, 128, 244, 143, 191, 191];

            fn Compose(State: Nativeˉstate, Delta: i32, Byte: u8, Count: u32) -> text {
                return Textˉconcat(
                    Enumˉname(State),
                    Textˉconcat(
                        ":",
                        Textˉconcat(
                            I32ˉformat(Delta),
                            Textˉconcat(
                                ":",
                                Textˉconcat(U8ˉformat(Byte), Textˉconcat(":", U32ˉformat(Count))))
                        )
                    )
                );
            }

            fn Emit(Value: text) -> void {
                console.write_line(Value);
                diagnostic.write_line(Textˉquote(Textˉconcat(Value, Textˉfromˉutf8(Euro))));
                return;
            }

            export fn Main() -> i32 {
                Emit(Compose(Nativeˉstate.Running, -3, 7u8, 42u32));
                console.write_line(Textˉconcat(
                    I32ˉformat(-2147483647 - 1),
                    Textˉconcat(":", U32ˉformat(4294967295u32))));
                console.write_line(Textˉconcat(I32ˉformat(0), Textˉconcat(":", U32ˉformat(0u32))));
                diagnostic.write_line(Textˉquote(Textˉfromˉutf8(Quoteˉedges)));
                diagnostic.write_line(Textˉconcat(
                    Enumˉname(Nativeˉedge.Second),
                    Textˉconcat(":", Enumˉname(Nativeˉedge.Maximum))));
                return 42;
            }
            """;
        var Verified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(Source));
        var Authorized = ImmutableHashSet.Create(
            StringComparer.Ordinal,
            Capabilityˉcatalog.CONSOLE_WRITE_LINE,
            Capabilityˉcatalog.DIAGNOSTIC_WRITE_LINE);

        var Referenceˉoutput = new StringWriter();
        var Referenceˉdiagnostic = new StringWriter();
        var Referenceˉresources = new Hostedˉresourceˉcontext(
            [],
            Referenceˉoutput,
            Referenceˉdiagnostic);
        var Reference = new Referenceˉruntime(
            Verified,
            new Referenceˉcapabilityˉhost(Referenceˉresources),
            new(Authorized)).Runˉmain();
        Equal(42, Reference.Exitˉcode);
        Equal("Running:-3:7:42\n-2147483648:4294967295\n0:0\n", Referenceˉoutput.ToString());
        Equal(
            "\"Running:-3:7:42\\u20AC\"\n" +
            "\"\\\"\\\\\\b\\f\\n\\r\\t\\u0000\\u001F ~\\u007F\\u00E9\\u20AC\\uD83D\\uDE00\\uDBFF\\uDFFF\"\n" +
            "Second:Maximum\n",
            Referenceˉdiagnostic.ToString());

        var First = X64ˉnativeˉbackend.Compile(Verified);
        var Second = X64ˉnativeˉbackend.Compile(Verified);
        Sequenceˉequal(First.Fragment.Code, Second.Fragment.Code);
        Sequenceˉequal(First.Fragment.Types, Second.Fragment.Types);
        var Operations = First.Module.Functions
            .SelectMany(Function => Function.Blocks)
            .SelectMany(Block => Block.Operations)
            .ToImmutableArray();
        True(Operations.Any(Operation => Operation is Nativeˉenumˉname), "Native IR omitted enum.name.");
        True(Operations.Count(Operation => Operation is Nativeˉintegerˉformat) == 7,
            "Native IR omitted integer formatting.");
        True(Operations.Any(Operation => Operation is Nativeˉtextˉconcat), "Native IR omitted text.concat.");
        True(Operations.Any(Operation => Operation is Nativeˉtextˉfromˉutf8), "Native IR omitted text.from_utf8.");
        True(Operations.Any(Operation => Operation is Nativeˉtextˉquote), "Native IR omitted text.quote.");
        True(Operations.Any(Operation => Operation is Nativeˉvoidˉcall), "Native IR omitted void calls.");
        True(First.Module.Functions.Any(Function => Function.Returnˉtype == Nativeˉvalueˉtype.Borrowedˉtext),
            "Native IR omitted a descriptor-returning function.");
        True(First.Module.Functions.Any(Function => Function.Returnˉtype == Nativeˉvalueˉtype.Void),
            "Native IR omitted a void function.");
        foreach (var Service in new[]
        {
            Nativeˉservice.Enumˉname,
            Nativeˉservice.Textˉconcat,
            Nativeˉservice.Textˉquote,
            Nativeˉservice.I32ˉformat,
            Nativeˉservice.U32ˉformat,
        })
        {
            var Firstˉservice = X64ˉnativeˉtextˉservices.Build(Service, First.Fragment.Types);
            var Secondˉservice = X64ˉnativeˉtextˉservices.Build(Service, First.Fragment.Types);
            Sequenceˉequal(Firstˉservice, Secondˉservice);
            X64ˉnativeˉtextˉservices.Verify(Service, Firstˉservice.AsSpan(), First.Fragment.Types);
            var Corruptedˉservice = Firstˉservice.ToArray();
            Corruptedˉservice[0] ^= 0x01;
            Throwsˉinvalidˉoperation(
                $"Native {Service} service identity",
                () => X64ˉnativeˉtextˉservices.Verify(
                    Service,
                    Corruptedˉservice,
                    First.Fragment.Types));
        }
        Equal(
            323,
            X64ˉnativeˉtextˉservices.ENUM_NAME_CANONICAL_SIZE);
        Equal(
            X64ˉnativeˉtextˉservices.TEXT_CONCAT_CANONICAL_SIZE,
            X64ˉnativeˉtextˉservices.Build(Nativeˉservice.Textˉconcat).Length);
        Equal(
            X64ˉnativeˉtextˉservices.I32_FORMAT_CANONICAL_SIZE,
            X64ˉnativeˉtextˉservices.Build(Nativeˉservice.I32ˉformat).Length);
        Equal(
            X64ˉnativeˉtextˉservices.U32_FORMAT_CANONICAL_SIZE,
            X64ˉnativeˉtextˉservices.Build(Nativeˉservice.U32ˉformat).Length);
        Equal(
            X64ˉnativeˉtextˉservices.TEXT_QUOTE_CANONICAL_SIZE,
            X64ˉnativeˉtextˉservices.Build(Nativeˉservice.Textˉquote).Length);
        var Enumˉbundle = X64ˉnativeˉtextˉservices.Build(
            Nativeˉservice.Enumˉname,
            First.Fragment.Types);
        True(
            Enumˉbundle.Length > X64ˉnativeˉtextˉservices.ENUM_NAME_CANONICAL_SIZE,
            "Native enum-name service omitted its immutable metadata.");
        var Enumˉmetadata = Enumˉbundle.AsSpan()[X64ˉnativeˉtextˉservices.ENUM_NAME_CANONICAL_SIZE..];
        Equal(0x4E455657u, BinaryPrimitives.ReadUInt32LittleEndian(Enumˉmetadata));
        Equal(1u, BinaryPrimitives.ReadUInt32LittleEndian(Enumˉmetadata[4..]));
        Equal((uint)Enumˉmetadata.Length, BinaryPrimitives.ReadUInt32LittleEndian(Enumˉmetadata[8..]));
        Equal((uint)First.Fragment.Types.Length, BinaryPrimitives.ReadUInt32LittleEndian(Enumˉmetadata[12..]));
        Equal(
            (uint)First.Fragment.Types.OfType<Enumˉtypeˉdeclaration>().Sum(Enum => Enum.Members.Length),
            BinaryPrimitives.ReadUInt32LittleEndian(Enumˉmetadata[16..]));
        var Corruptedˉmetadata = Enumˉbundle.ToArray();
        Corruptedˉmetadata[^1] ^= 0x01;
        Throwsˉinvalidˉoperation(
            "Native Enumˉname service identity",
            () => X64ˉnativeˉtextˉservices.Verify(
                Nativeˉservice.Enumˉname,
                Corruptedˉmetadata,
                First.Fragment.Types));
        var Corruptedˉmetadataˉheader = Enumˉbundle.ToArray();
        BinaryPrimitives.WriteUInt32LittleEndian(
            Corruptedˉmetadataˉheader.AsSpan(X64ˉnativeˉtextˉservices.ENUM_NAME_CANONICAL_SIZE + 8),
            uint.MaxValue);
        Throwsˉinvalidˉoperation(
            "Native Enumˉname service identity",
            () => X64ˉnativeˉtextˉservices.Verify(
                Nativeˉservice.Enumˉname,
                Corruptedˉmetadataˉheader,
                First.Fragment.Types));

        void Runˉnative(ImmutableArray<byte> code)
        {
            using var Output = new Nativeˉoutputˉcapture();
            using var Diagnostic = new Nativeˉoutputˉcapture();
            var Resources = new Hostedˉresourceˉcontext([], TextWriter.Null, TextWriter.Null);
            Equal(
                Reference.Exitˉcode,
                X64ˉnativeˉexecutor.Executeˉi32(
                    First.Fragment with { Code = code },
                    maximumˉinstructions: Reference.Executedˉinstructions,
                    hostˉservices: new(Output.Channel, Authorized, Resources, Diagnostic.Channel)));
            Equal(Referenceˉoutput.ToString(), Output.Readˉtext());
            Equal(Referenceˉdiagnostic.ToString(), Diagnostic.Readˉtext());
        }

        Runˉnative(First.Fragment.Code);
        var Object = Nativeˉobjectˉsink.Writeˉwvo(First.Fragment);
        var Linked = Linkˉsuccess([Object.ToArray()], new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"));
        Runˉnative(Linked.Imageˉbytes);

        var Invalidˉutf8 = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess("""
            module Nativeˉinvalidˉutf8 profile portable;
            data Invalid: bytes = [192, 175];
            export fn Main() -> i32 {
                let Value: text = Textˉfromˉutf8(Invalid);
                return 0;
            }
            """));
        Throwsˉruntime(
            "WVR3014",
            () => _ = new Referenceˉruntime(
                Invalidˉutf8,
                new Referenceˉcapabilityˉhost(TextWriter.Null),
                Runtimeˉoptions.Portableˉdefaults).Runˉmain());
        Throwsˉnativeˉtrap(
            "WVR3014",
            () => _ = X64ˉnativeˉexecutor.Executeˉi32(
                X64ˉnativeˉbackend.Compile(Invalidˉutf8).Fragment));

        var Valueˉlimit = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess("""
            module Nativeˉtextˉvalueˉlimit profile portable;
            export fn Main() -> i32 {
                var Value: text = "a";
                var Power: i32 = 0;
                while Power < 20 {
                    Value = Textˉconcat(Value, Value);
                    Power = Power + 1;
                }
                Value = Textˉconcat(Value, "a");
                return 0;
            }
            """));
        Throwsˉruntime(
            "WVR3012",
            () => _ = new Referenceˉruntime(
                Valueˉlimit,
                new Referenceˉcapabilityˉhost(TextWriter.Null),
                Runtimeˉoptions.Portableˉdefaults).Runˉmain());
        Throwsˉnativeˉtrap(
            "WVR3012",
            () => _ = X64ˉnativeˉexecutor.Executeˉi32(
                X64ˉnativeˉbackend.Compile(Valueˉlimit).Fragment,
                maximumˉinstructions: 10_000));

        var Arenaˉexhaustion = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess("""
            module Nativeˉtextˉarena profile portable;
            export fn Main() -> i32 {
                var Value: text = "a";
                var Power: i32 = 0;
                while Power < 19 {
                    Value = Textˉconcat(Value, Value);
                    Power = Power + 1;
                }
                var Copy: text = Value;
                var Count: i32 = 0;
                while Count < 40 {
                    Copy = Textˉconcat(Value, "");
                    Count = Count + 1;
                }
                return 0;
            }
            """));
        Throwsˉnativeˉtrap(
            "WVR3018",
            () => _ = X64ˉnativeˉexecutor.Executeˉi32(
                X64ˉnativeˉbackend.Compile(Arenaˉexhaustion).Fragment,
                maximumˉinstructions: 10_000));

        var Quoteˉvalueˉlimit = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess("""
            module Nativeˉquoteˉvalueˉlimit profile portable;
            data Euro: bytes = [226, 130, 172];
            export fn Main() -> i32 {
                var Value: text = Textˉfromˉutf8(Euro);
                var Power: i32 = 0;
                while Power < 18 {
                    Value = Textˉconcat(Value, Value);
                    Power = Power + 1;
                }
                Value = Textˉquote(Value);
                return 0;
            }
            """));
        Throwsˉruntime(
            "WVR3012",
            () => _ = new Referenceˉruntime(
                Quoteˉvalueˉlimit,
                new Referenceˉcapabilityˉhost(TextWriter.Null),
                Runtimeˉoptions.Portableˉdefaults).Runˉmain());
        Throwsˉnativeˉtrap(
            "WVR3012",
            () => _ = X64ˉnativeˉexecutor.Executeˉi32(
                X64ˉnativeˉbackend.Compile(Quoteˉvalueˉlimit).Fragment,
                maximumˉinstructions: 10_000));

        var Quoteˉarenaˉexhaustion = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess("""
            module Nativeˉquoteˉarena profile portable;
            export fn Main() -> i32 {
                var Value: text = "a";
                var Power: i32 = 0;
                while Power < 18 {
                    Value = Textˉconcat(Value, Value);
                    Power = Power + 1;
                }
                var Quoted: text = "";
                var Count: i32 = 0;
                while Count < 62 {
                    Quoted = Textˉquote(Value);
                    Count = Count + 1;
                }
                return 0;
            }
            """));
        Throwsˉnativeˉtrap(
            "WVR3018",
            () => _ = X64ˉnativeˉexecutor.Executeˉi32(
                X64ˉnativeˉbackend.Compile(Quoteˉarenaˉexhaustion).Fragment,
                maximumˉinstructions: 10_000));

        var Longˉmember = $"N{new string('a', 127)}";
        var Enumˉarenaˉexhaustion = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess($$"""
            module Nativeˉenumˉarena profile portable;
            enum Nativeˉlong { {{Longˉmember}} = 0; }
            export fn Main() -> i32 {
                var Name: text = "";
                var Count: i32 = 0;
                while Count < 131073 {
                    Name = Enumˉname(Nativeˉlong.{{Longˉmember}});
                    Count = Count + 1;
                }
                return 0;
            }
            """));
        Throwsˉnativeˉtrap(
            "WVR3018",
            () => _ = X64ˉnativeˉexecutor.Executeˉi32(
                X64ˉnativeˉbackend.Compile(Enumˉarenaˉexhaustion).Fragment,
                maximumˉinstructions: 10_000_000));

        var Descriptorˉfunction = First.Module.Functions
            .Select((Function, Index) => (Function, Index))
            .First(Item => Item.Function.Returnˉtype == Nativeˉvalueˉtype.Borrowedˉtext);
        var Descriptorˉsymbol = First.Fragment.Symbols.Single(Symbol =>
            StringComparer.Ordinal.Equals(Symbol.Name, $"$function_{Descriptorˉfunction.Index:D4}"));
        var Corruptedˉhiddenˉresult = First.Fragment.Code.ToArray();
        var Descriptorˉstart = checked((int)Descriptorˉsymbol.Offset);
        var Frameˉallocation = Corruptedˉhiddenˉresult.AsSpan(
                Descriptorˉstart,
                checked((int)Descriptorˉsymbol.Size))
            .IndexOf(new byte[] { 0x48, 0x81, 0xEC });
        True(Frameˉallocation >= 0, "Descriptor-returning native function omitted its frame allocation.");
        Corruptedˉhiddenˉresult[Descriptorˉstart + Frameˉallocation + 8] ^= 0x01;
        Throwsˉnative(
            "WVN3030",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                First.Fragment with { Code = Corruptedˉhiddenˉresult.ToImmutableArray() }));

        var Invalidˉtypes = First.Fragment.Types.SetItem(
            0,
            new Enumˉtypeˉdeclaration(
                "",
                [new("Ready", 0), new("Running", 1)]));
        Throwsˉnative(
            "WVN3009",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                First.Fragment with { Types = Invalidˉtypes }));
    }

    private static void Nativeˉwvdumpˉstructuralˉparserˉruns()
    {
        const string Reportˉmarker = "fn Stringˉvalue(Input: bytes, Cursor: u32) -> text";
        var Reportˉoffset = WVDUMP_CORE_SOURCE.IndexOf(Reportˉmarker, StringComparison.Ordinal);
        True(Reportˉoffset > 0, "The Windvale wvdump structural/report boundary is missing.");
        var Structuralˉsource = WVDUMP_CORE_SOURCE[..Reportˉoffset]
            .Replace("profile hosted", "profile portable", StringComparison.Ordinal);
        foreach (var Capability in new[]
        {
            "console.write_line",
            "diagnostic.write_line",
            "file.read_bytes",
            "process.argument",
            "process.argument_count",
        })
        {
            Structuralˉsource = Structuralˉsource
                .Replace($"capability {Capability};\r\n", "", StringComparison.Ordinal)
                .Replace($"capability {Capability};\n", "", StringComparison.Ordinal);
        }
        Structuralˉsource +=
            """
            export fn Main() -> i32 {
                let Envelope: Wvbˉinspection = Inspectˉwvbˉenvelope(Validˉmodule);
                if Envelope.Status != Wvbˉstatus.Valid { return 1; }
                if Envelope.Sectionsˉseen != 7u32 { return 2; }
                if Envelope.Failureˉoffset != 94u32 { return 3; }

                let Payload: Wvbˉpayloadˉinspection = Inspectˉwvbˉpayloads(Validˉmodule);
                if Payload.Status != Wvbˉstatus.Valid { return 4; }
                if Payload.Declarationsˉseen != 1u32 { return 5; }
                if Payload.Instructionsˉseen != 0u32 { return 6; }
                return 42;
            }
            """;

        var Verified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(Structuralˉsource));
        var Interpreted = new Referenceˉruntime(
            Verified,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        Equal(42, Interpreted.Exitˉcode);

        var Native = X64ˉnativeˉbackend.Compile(Verified);
        True(Native.Module.Types.Length == 5, "Native wvdump omitted structural nominal metadata.");
        True(
            Native.Module.Functions.SelectMany(Function => Function.Blocks)
                .SelectMany(Block => Block.Operations)
                .Count(Operation => Operation is Nativeˉrecordˉcreate) >= 4,
            "Native wvdump omitted structural record construction.");
        Equal(
            Interpreted.Exitˉcode,
            X64ˉnativeˉexecutor.Executeˉi32(
                Native.Fragment,
                maximumˉinstructions: Interpreted.Executedˉinstructions));

        var Object = Nativeˉobjectˉsink.Writeˉwvo(Native.Fragment);
        var Linked = Linkˉsuccess(
            [Object.ToArray()],
            new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"));
        Equal(
            Interpreted.Exitˉcode,
            X64ˉnativeˉexecutor.Executeˉi32(
                Native.Fragment with { Code = Linked.Imageˉbytes },
                maximumˉinstructions: Interpreted.Executedˉinstructions));
    }

    private static void Nativeˉwvdumpˉcompleteˉruns()
    {
        var Input = Compileˉsuccess(NATIVE_CONSTANT_SOURCE).ToImmutableArray();
        var Verified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(WVDUMP_CORE_SOURCE));
        var Authorized = ImmutableHashSet.Create(
            StringComparer.Ordinal,
            Capabilityˉcatalog.CONSOLE_WRITE_LINE,
            Capabilityˉcatalog.DIAGNOSTIC_WRITE_LINE,
            Capabilityˉcatalog.FILE_READ_BYTES,
            Capabilityˉcatalog.PROCESS_ARGUMENT,
            Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT);

        Testˉfileˉreader Makeˉreader() => new((Name, Maximum) =>
        {
            Equal("input.wvb", Name);
            True(Input.Length <= Maximum, "The complete wvdump fixture exceeded the hosted reader bound.");
            return Input;
        });

        Hostedˉresourceˉcontext Makeˉresources(
            TextWriter output,
            TextWriter diagnostic,
            Testˉfileˉreader reader,
            string resourceˉname = "input.wvb") =>
            new([resourceˉname], output, diagnostic, reader);

        var Referenceˉoutput = new StringWriter();
        var Referenceˉdiagnostic = new StringWriter();
        var Referenceˉreader = Makeˉreader();
        var Reference = new Referenceˉruntime(
            Verified,
            new Referenceˉcapabilityˉhost(Makeˉresources(
                Referenceˉoutput,
                Referenceˉdiagnostic,
                Referenceˉreader)),
            new(Authorized)).Runˉmain();
        Equal(0, Reference.Exitˉcode);
        Equal(string.Empty, Referenceˉdiagnostic.ToString());
        True(Referenceˉoutput.ToString().StartsWith("wvdump 1\nmodule version=1.6", StringComparison.Ordinal),
            "The complete Windvale wvdump report omitted its module header.");
        True(Referenceˉoutput.ToString().Contains("name=\"Main\"", StringComparison.Ordinal),
            "The complete Windvale wvdump report omitted its function declaration.");
        Equal(1, Referenceˉreader.Readˉcount);

        var First = X64ˉnativeˉbackend.Compile(Verified);
        var Second = X64ˉnativeˉbackend.Compile(Verified);
        Sequenceˉequal(First.Fragment.Code, Second.Fragment.Code);
        Sequenceˉequal(First.Fragment.Types, Second.Fragment.Types);
        Sequenceˉequal(
            Enum.GetValues<Nativeˉservice>()
                .Where(Service => Service != Nativeˉservice.Fileˉwriteˉbytes),
            First.Fragment.Requiredˉservices);
        var Operations = First.Module.Functions
            .SelectMany(Function => Function.Blocks)
            .SelectMany(Block => Block.Operations)
            .ToImmutableArray();
        True(Operations.Count(Operation => Operation is Nativeˉtextˉconcat) >= 100,
            "Complete native wvdump omitted its report composition.");
        True(Operations.Any(Operation => Operation is Nativeˉdiagnosticˉwriteˉline),
            "Complete native wvdump omitted diagnostics.");
        True(Operations.Any(Operation => Operation is Nativeˉvoidˉcall),
            "Complete native wvdump omitted void calls.");
        True(First.Module.Functions.Any(Function => Function.Returnˉtype == Nativeˉvalueˉtype.Borrowedˉtext),
            "Complete native wvdump omitted descriptor-returning helpers.");

        var Nativeˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-wvdump-{Guid.NewGuid():N}.wvb");
        void Runˉnative(ImmutableArray<byte> code)
        {
            using var Output = new Nativeˉoutputˉcapture();
            using var Diagnostic = new Nativeˉoutputˉcapture();
            var Reader = new Testˉfileˉreader((_, _) =>
                throw new InvalidOperationException("Native execution called the Stage 0 file reader."));
            Equal(
                Reference.Exitˉcode,
                X64ˉnativeˉexecutor.Executeˉi32(
                    First.Fragment with { Code = code },
                    maximumˉinstructions: Reference.Executedˉinstructions,
                    hostˉservices: new(
                        Output.Channel,
                        Authorized,
                        Makeˉresources(
                            TextWriter.Null,
                            TextWriter.Null,
                            Reader,
                            Nativeˉpath),
                        Diagnostic.Channel,
                        Nativeˉfileˉinput.Hostˉfileˉsystem())));
            Equal(Referenceˉoutput.ToString(), Output.Readˉtext());
            Equal(Referenceˉdiagnostic.ToString(), Diagnostic.Readˉtext());
            Equal(0, Reader.Readˉcount);
        }

        try
        {
            File.WriteAllBytes(Nativeˉpath, Input.AsSpan());
            Runˉnative(First.Fragment.Code);
            var Object = Nativeˉobjectˉsink.Writeˉwvo(First.Fragment);
            var Linked = Linkˉsuccess(
                [Object.ToArray()],
                new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"));
            Runˉnative(Linked.Imageˉbytes);
        }
        finally
        {
            File.Delete(Nativeˉpath);
        }
    }

    private static void Nativeˉborrowedˉbytesˉagree()
    {
        var Verified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(NATIVE_BYTES_SOURCE));
        var Interpreted = new Referenceˉruntime(
            Verified,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        Equal(42, Interpreted.Exitˉcode);

        var First = X64ˉnativeˉbackend.Compile(Verified);
        var Second = X64ˉnativeˉbackend.Compile(Verified);
        Sequenceˉequal(First.Fragment.Code, Second.Fragment.Code);
        Sequenceˉequal(First.Fragment.Symbols, Second.Fragment.Symbols);
        Sequenceˉequal(First.Fragment.Patches, Second.Fragment.Patches);
        True(
            First.Module.Data.Single() is Nativeˉbytesˉdata,
            "Native machine IR did not retain immutable byte data.");
        Sequenceˉequal(
            new byte[] { 42, 1, 0, 0, 0, 255, 255, 255, 255 },
            ((Nativeˉbytesˉdata)First.Module.Data.Single()).Bytes);
        var Operations = First.Module.Functions
            .SelectMany(Function => Function.Blocks)
            .SelectMany(Block => Block.Operations)
            .ToImmutableArray();
        True(Operations.Any(Operation => Operation is Nativeˉstaticˉbytesˉconstant),
            "Native machine IR omitted the static borrowed-bytes value.");
        True(Operations.Any(Operation => Operation is Nativeˉbytesˉslice),
            "Native machine IR omitted the bounded byte slice.");
        Sequenceˉequal(
            Enum.GetValues<Nativeˉbytesˉreadˉkind>(),
            Operations
                .OfType<Nativeˉbytesˉread>()
                .Select(Operation => Operation.Kind)
                .Distinct()
                .Order());
        True(Operations.Any(Operation => Operation is Nativeˉbytesˉlength),
            "Native machine IR omitted the byte length.");
        Equal(2, Operations.OfType<Nativeˉbytesˉfromˉu8>().Count());
        True(Operations.Any(Operation => Operation is Nativeˉbytesˉfromˉu32ˉlittle),
            "Native machine IR omitted little-endian byte construction.");
        True(Operations.Any(Operation => Operation is Nativeˉu32ˉfromˉu8),
            "Native machine IR omitted the lossless u8-to-u32 conversion.");
        True(Operations.Any(Operation => Operation is Nativeˉu32ˉbinary),
            "Native machine IR omitted checked u32 arithmetic.");
        True(Operations.Any(Operation => Operation is Nativeˉu32ˉcomparison),
            "Native machine IR omitted u32 comparisons.");
        True(Operations.Any(Operation => Operation is Nativeˉu8ˉcomparison),
            "Native machine IR omitted u8 comparisons.");

        _ = Nativeˉfragmentˉverifier.Verify(First.Fragment);

        var Encodedˉu8ˉstart = First.Fragment.Code.AsSpan().IndexOf(new byte[]
        {
            0x41, 0x8B, 0x47, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET,
            0x41, 0x89, 0xC1,
            0x89, 0xC1,
            0x83, 0xC1, 0x01,
            0x0F, 0x82,
        });
        True(Encodedˉu8ˉstart >= 0, "Native bytes.from_u8 omitted its checked arena allocation.");

        var Corruptedˉu8ˉallocation = First.Fragment.Code.ToArray();
        Corruptedˉu8ˉallocation[Encodedˉu8ˉstart + 11] = 0x02;
        Throwsˉnative(
            "WVN3030",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                First.Fragment with { Code = Corruptedˉu8ˉallocation.ToImmutableArray() }));

        var Corruptedˉu8ˉstore = First.Fragment.Code.ToArray();
        Corruptedˉu8ˉstore[Encodedˉu8ˉstart + 69] = 0x89;
        Throwsˉnative(
            "WVN3030",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                First.Fragment with { Code = Corruptedˉu8ˉstore.ToImmutableArray() }));

        var Corruptedˉu8ˉalias = First.Fragment.Code.ToArray();
        Corruptedˉu8ˉalias.AsSpan(Encodedˉu8ˉstart + 46, sizeof(int)).CopyTo(
            Corruptedˉu8ˉalias.AsSpan(Encodedˉu8ˉstart + 65, sizeof(int)));
        Throwsˉnative(
            "WVN3030",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                First.Fragment with { Code = Corruptedˉu8ˉalias.ToImmutableArray() }));

        var Encodedˉu32ˉstart = First.Fragment.Code.AsSpan().IndexOf(new byte[]
        {
            0x41, 0x8B, 0x47, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET,
            0x41, 0x89, 0xC1,
            0x89, 0xC1,
            0x83, 0xC1, 0x04,
            0x0F, 0x82,
        });
        True(Encodedˉu32ˉstart >= 0, "Native bytes.from_u32_little omitted its checked arena allocation.");
        var Corruptedˉu32ˉalias = First.Fragment.Code.ToArray();
        Corruptedˉu32ˉalias.AsSpan(Encodedˉu32ˉstart + 46, sizeof(int)).CopyTo(
            Corruptedˉu32ˉalias.AsSpan(Encodedˉu32ˉstart + 65, sizeof(int)));
        Throwsˉnative(
            "WVN3030",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                First.Fragment with { Code = Corruptedˉu32ˉalias.ToImmutableArray() }));

        Equal(
            42,
            X64ˉnativeˉexecutor.Executeˉi32(
                First.Fragment,
                maximumˉinstructions: Interpreted.Executedˉinstructions,
                maximumˉcallˉdepth: 2));
        var Objectˉbytes = Nativeˉobjectˉsink.Writeˉwvo(First.Fragment);
        Sequenceˉequal(Objectˉbytes, Nativeˉobjectˉsink.Writeˉwvo(Second.Fragment));
        var Linked = Linkˉsuccess(
            [Objectˉbytes.ToArray()],
            new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"));
        Equal(
            42,
            X64ˉnativeˉexecutor.Executeˉi32(
                First.Fragment with { Code = Linked.Imageˉbytes },
                maximumˉinstructions: Interpreted.Executedˉinstructions,
                maximumˉcallˉdepth: 2));

        var Utf8ˉverified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess("""
            module Nativeˉutf8ˉvalidation profile portable;
            data Empty: bytes = [];
            data Ascii: bytes = [0, 65, 127];
            data Twoˉminimum: bytes = [194, 128];
            data Twoˉmaximum: bytes = [223, 191];
            data Threeˉe0: bytes = [224, 160, 128];
            data Threeˉstandard: bytes = [225, 128, 191, 236, 191, 191, 238, 128, 128, 239, 191, 191];
            data Threeˉed: bytes = [237, 159, 191];
            data Fourˉf0: bytes = [240, 144, 128, 128];
            data Fourˉstandard: bytes = [241, 128, 128, 128, 243, 191, 191, 191];
            data Fourˉf4: bytes = [244, 143, 191, 191];
            data Strayˉcontinuation: bytes = [128];
            data Overlongˉtwo: bytes = [192, 175];
            data Truncatedˉtwo: bytes = [194];
            data Badˉtwoˉcontinuation: bytes = [194, 32];
            data Overlongˉthree: bytes = [224, 159, 191];
            data Surrogate: bytes = [237, 160, 128];
            data Truncatedˉthree: bytes = [225, 128];
            data Badˉthreeˉcontinuation: bytes = [225, 128, 32];
            data Overlongˉfour: bytes = [240, 143, 191, 191];
            data Aboveˉunicode: bytes = [244, 144, 128, 128];
            data Highˉlead: bytes = [245, 128, 128, 128];
            data Truncatedˉfour: bytes = [241, 128, 128];
            data Badˉfourˉcontinuation: bytes = [241, 128, 32, 128];
            export fn Main() -> i32 {
                if !Textˉutf8ˉisˉvalid(Empty) { return 1; }
                if !Textˉutf8ˉisˉvalid(Ascii) { return 1; }
                if !Textˉutf8ˉisˉvalid(Twoˉminimum) { return 1; }
                if !Textˉutf8ˉisˉvalid(Twoˉmaximum) { return 1; }
                if !Textˉutf8ˉisˉvalid(Threeˉe0) { return 1; }
                if !Textˉutf8ˉisˉvalid(Threeˉstandard) { return 1; }
                if !Textˉutf8ˉisˉvalid(Threeˉed) { return 1; }
                if !Textˉutf8ˉisˉvalid(Fourˉf0) { return 1; }
                if !Textˉutf8ˉisˉvalid(Fourˉstandard) { return 1; }
                if !Textˉutf8ˉisˉvalid(Fourˉf4) { return 1; }
                if Textˉutf8ˉisˉvalid(Strayˉcontinuation) { return 2; }
                if Textˉutf8ˉisˉvalid(Overlongˉtwo) { return 2; }
                if Textˉutf8ˉisˉvalid(Truncatedˉtwo) { return 2; }
                if Textˉutf8ˉisˉvalid(Badˉtwoˉcontinuation) { return 2; }
                if Textˉutf8ˉisˉvalid(Overlongˉthree) { return 2; }
                if Textˉutf8ˉisˉvalid(Surrogate) { return 2; }
                if Textˉutf8ˉisˉvalid(Truncatedˉthree) { return 2; }
                if Textˉutf8ˉisˉvalid(Badˉthreeˉcontinuation) { return 2; }
                if Textˉutf8ˉisˉvalid(Overlongˉfour) { return 2; }
                if Textˉutf8ˉisˉvalid(Aboveˉunicode) { return 2; }
                if Textˉutf8ˉisˉvalid(Highˉlead) { return 2; }
                if Textˉutf8ˉisˉvalid(Truncatedˉfour) { return 2; }
                if Textˉutf8ˉisˉvalid(Badˉfourˉcontinuation) { return 2; }
                return 42;
            }
            """));
        var Utf8ˉreference = new Referenceˉruntime(
            Utf8ˉverified,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        Equal(42, Utf8ˉreference.Exitˉcode);
        var Utf8ˉnative = X64ˉnativeˉbackend.Compile(Utf8ˉverified);
        Sequenceˉequal([Nativeˉservice.Textˉutf8ˉisˉvalid], Utf8ˉnative.Fragment.Requiredˉservices);
        True(
            Utf8ˉnative.Module.Functions.SelectMany(Function => Function.Blocks)
                .SelectMany(Block => Block.Operations)
                .Count(Operation => Operation is Nativeˉtextˉutf8ˉisˉvalid) == 23,
            "Native machine IR omitted UTF-8 validation.");
        var Utf8ˉservice = X64ˉnativeˉutf8ˉservice.Build();
        Equal(X64ˉnativeˉutf8ˉservice.CANONICAL_SIZE, Utf8ˉservice.Length);
        Equal(
            X64ˉnativeˉutf8ˉservice.CANONICAL_SHA256,
            Convert.ToHexString(SHA256.HashData(Utf8ˉservice.AsSpan())).ToLowerInvariant());
        Sequenceˉequal(Utf8ˉservice, X64ˉnativeˉutf8ˉservice.Build());
        X64ˉnativeˉutf8ˉservice.Verify(Utf8ˉservice.AsSpan());
        var Corruptedˉutf8ˉservice = Utf8ˉservice.ToArray();
        Corruptedˉutf8ˉservice[0] ^= 0x01;
        Throwsˉinvalidˉoperation(
            "Native UTF-8 service identity",
            () => X64ˉnativeˉutf8ˉservice.Verify(Corruptedˉutf8ˉservice));
        Equal(
            Utf8ˉreference.Exitˉcode,
            X64ˉnativeˉexecutor.Executeˉi32(
                Utf8ˉnative.Fragment,
                maximumˉinstructions: Utf8ˉreference.Executedˉinstructions));

        var Dataˉpatch = First.Fragment.Patches[0];
        var Corruptedˉdescriptor = First.Fragment.Code.ToArray();
        Corruptedˉdescriptor[checked((int)Dataˉpatch.Offset - 3)] = 0x90;
        Throwsˉnative(
            "WVN3030",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                First.Fragment with { Code = Corruptedˉdescriptor.ToImmutableArray() }));

        var Corruptedˉlength = First.Fragment.Code.ToArray();
        Corruptedˉlength[checked((int)Dataˉpatch.Offset + 13)] ^= 0x01;
        Throwsˉnative(
            "WVN3030",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                First.Fragment with { Code = Corruptedˉlength.ToImmutableArray() }));

        var Corruptedˉdescriptorˉtype = First.Fragment.Code.ToArray();
        var Descriptorˉslotˉoffset = BinaryPrimitives.ReadInt32LittleEndian(
            Corruptedˉdescriptorˉtype.AsSpan(checked((int)Dataˉpatch.Offset + 8), sizeof(int)));
        var Scalarˉconstant = -1;
        for (var Offset = checked((int)Dataˉpatch.Offset + 20);
            Offset <= Corruptedˉdescriptorˉtype.Length - 12;
            Offset++)
        {
            if (Corruptedˉdescriptorˉtype[Offset] == 0xB8 &&
                Corruptedˉdescriptorˉtype.AsSpan(Offset + 5, 3).SequenceEqual(
                    new byte[] { 0x89, 0x84, 0x24 }))
            {
                Scalarˉconstant = Offset;
                break;
            }
        }
        True(Scalarˉconstant >= 0, "Native borrowed bytes did not emit a later scalar constant.");
        BinaryPrimitives.WriteInt32LittleEndian(
            Corruptedˉdescriptorˉtype.AsSpan(Scalarˉconstant + 8, sizeof(int)),
            Descriptorˉslotˉoffset);
        Throwsˉnative(
            "WVN3030",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                First.Fragment with { Code = Corruptedˉdescriptorˉtype.ToImmutableArray() }));

        var Corruptedˉbyteˉargument = First.Fragment.Code.ToArray();
        var Byteˉargument = Corruptedˉbyteˉargument.AsSpan().IndexOf(
            new byte[] { 0x4C, 0x8D, 0x84, 0x24 });
        True(Byteˉargument >= 0, "Native borrowed bytes did not use the typed first-argument form.");
        Corruptedˉbyteˉargument[Byteˉargument + 1] = 0x8B;
        Throwsˉnative(
            "WVN3030",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                First.Fragment with { Code = Corruptedˉbyteˉargument.ToImmutableArray() }));

        var Corruptedˉbyteˉbounds = First.Fragment.Code.ToArray();
        var Byteˉboundsˉbranch = Corruptedˉbyteˉbounds.AsSpan().IndexOf(
            new byte[] { 0x0F, 0x87 });
        True(Byteˉboundsˉbranch >= 0, "Native borrowed bytes did not emit an unsigned bounds branch.");
        BinaryPrimitives.WriteInt32LittleEndian(
            Corruptedˉbyteˉbounds.AsSpan(Byteˉboundsˉbranch + 2, sizeof(int)),
            0);
        Throwsˉnative(
            "WVN3030",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                First.Fragment with { Code = Corruptedˉbyteˉbounds.ToImmutableArray() }));

        var Boundsˉverified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess("""
            module Nativeˉbyteˉbounds profile portable;
            data Input: bytes = [1, 2, 3, 4];
            export fn Main() -> i32 {
                let Value: u32 = Bytesˉreadˉu32ˉlittle(Input, 1u32);
                if Value == 0u32 { return 0; }
                return 1;
            }
            """));
        Throwsˉruntime(
            "WVR3008",
            () => _ = new Referenceˉruntime(
                Boundsˉverified,
                new Referenceˉcapabilityˉhost(TextWriter.Null),
                Runtimeˉoptions.Portableˉdefaults).Runˉmain());
        Throwsˉnativeˉtrap(
            "WVR3008",
            () => _ = X64ˉnativeˉexecutor.Executeˉi32(
                X64ˉnativeˉbackend.Compile(Boundsˉverified).Fragment));

        var Overflowˉverified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess("""
            module Nativeˉu32ˉoverflow profile portable;
            export fn Main() -> i32 {
                let Value: u32 = 4294967295u32 + 1u32;
                if Value == 0u32 { return 0; }
                return 1;
            }
            """));
        Throwsˉruntime(
            "WVR3007",
            () => _ = new Referenceˉruntime(
                Overflowˉverified,
                new Referenceˉcapabilityˉhost(TextWriter.Null),
                Runtimeˉoptions.Portableˉdefaults).Runˉmain());
        Throwsˉnativeˉtrap(
            "WVR3007",
            () => _ = X64ˉnativeˉexecutor.Executeˉi32(
                X64ˉnativeˉbackend.Compile(Overflowˉverified).Fragment));
    }

    private static void Nativeˉruntimeˉserviceˉisˉauthorized()
    {
        var Verified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(HELLO_SOURCE));
        var Authorizedˉcapabilities = ImmutableHashSet.Create(
            StringComparer.Ordinal,
            Capabilityˉcatalog.CONSOLE_WRITE_LINE);
        var Interpreterˉoutput = new StringWriter();
        var Interpreted = new Referenceˉruntime(
            Verified,
            new Referenceˉcapabilityˉhost(Interpreterˉoutput),
            new(Authorizedˉcapabilities)).Runˉmain();
        Equal(0, Interpreted.Exitˉcode);
        Equal("Hello from Windvale\n", Interpreterˉoutput.ToString());
        True(Interpreted.Executedˉinstructions > 1, "The hosted reference runtime did not charge instructions.");
        Throwsˉruntime(
            "WVR3029",
            () => _ = new Referenceˉruntime(
                Verified,
                new Referenceˉcapabilityˉhost(new Failingˉtextˉwriter()),
                new(Authorizedˉcapabilities)).Runˉmain());

        var First = X64ˉnativeˉbackend.Compile(Verified);
        var Second = X64ˉnativeˉbackend.Compile(Verified);
        Sequenceˉequal(First.Fragment.Code, Second.Fragment.Code);
        Sequenceˉequal(First.Fragment.Symbols, Second.Fragment.Symbols);
        Sequenceˉequal(First.Fragment.Patches, Second.Fragment.Patches);
        Sequenceˉequal(
            [Nativeˉservice.Consoleˉwriteˉline],
            First.Fragment.Requiredˉservices);
        True(
            First.Module.Data.Single() is Nativeˉutf8ˉdata,
            "Native machine IR did not retain immutable UTF-8 text data.");
        Sequenceˉequal(
            System.Text.Encoding.UTF8.GetBytes("Hello from Windvale"),
            ((Nativeˉutf8ˉdata)First.Module.Data.Single()).Bytes);
        True(
            First.Module.Functions
                .SelectMany(Function => Function.Blocks)
                .SelectMany(Block => Block.Operations)
                .Any(Operation => Operation is Nativeˉconsoleˉwriteˉline),
            "Native machine IR did not retain the authorized console service call.");

        foreach (var Platform in Enum.GetValues<Nativeˉoutputˉplatform>())
        {
            foreach (var Service in new[]
            {
                Nativeˉservice.Consoleˉwriteˉline,
                Nativeˉservice.Diagnosticˉwriteˉline,
            })
            {
                var Firstˉleaf = X64ˉnativeˉoutputˉservices.Build(Service, Platform);
                var Secondˉleaf = X64ˉnativeˉoutputˉservices.Build(Service, Platform);
                Sequenceˉequal(Firstˉleaf, Secondˉleaf);
                Equal(
                    X64ˉnativeˉoutputˉservices.Canonicalˉsize(Platform),
                    Firstˉleaf.Length);
                Equal(
                    X64ˉnativeˉoutputˉservices.Canonicalˉsha256(Service, Platform),
                    Convert.ToHexString(SHA256.HashData(Firstˉleaf.AsSpan())).ToLowerInvariant());
                X64ˉnativeˉoutputˉservices.Verify(Service, Platform, Firstˉleaf.AsSpan());
                var Corruptedˉleaf = Firstˉleaf.ToArray();
                Corruptedˉleaf[0] ^= 0x01;
                Throwsˉinvalidˉoperation(
                    $"Native {Platform} {Service} service identity",
                    () => X64ˉnativeˉoutputˉservices.Verify(
                        Service,
                        Platform,
                        Corruptedˉleaf));
            }
        }

        using var Nativeˉoutput = new Nativeˉoutputˉcapture();
        var Host = new Nativeˉhostˉservices(Nativeˉoutput.Channel, Authorizedˉcapabilities);
        Equal(
            0,
            X64ˉnativeˉexecutor.Executeˉi32(
                First.Fragment,
                maximumˉinstructions: Interpreted.Executedˉinstructions,
                hostˉservices: Host));
        Equal("Hello from Windvale\n", Nativeˉoutput.Readˉtext());
        Throwsˉnativeˉtrap(
            "WVR3011",
            () => _ = X64ˉnativeˉexecutor.Executeˉi32(
                First.Fragment,
                maximumˉinstructions: Interpreted.Executedˉinstructions - 1,
                hostˉservices: new(Nativeˉoutput.Channel, Authorizedˉcapabilities)));
        Throwsˉnativeˉtrap(
            "WVR3010",
            () => _ = X64ˉnativeˉexecutor.Executeˉi32(First.Fragment));
        Throwsˉnativeˉtrap(
            "WVR3010",
            () => _ = X64ˉnativeˉexecutor.Executeˉi32(
                First.Fragment,
                hostˉservices: new(Nativeˉoutput.Channel)));
        Throwsˉnativeˉtrap(
            "WVR3001",
            () => _ = X64ˉnativeˉexecutor.Executeˉi32(
                First.Fragment,
                hostˉservices: new(null, Authorizedˉcapabilities)));

        var Failureˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-output-failure-{Guid.NewGuid():N}.tmp");
        File.WriteAllBytes(Failureˉpath, []);
        try
        {
            using var Readˉonly = new FileStream(
                Failureˉpath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete);
            Throwsˉnativeˉtrap(
                "WVR3029",
                () => _ = X64ˉnativeˉexecutor.Executeˉi32(
                    First.Fragment,
                    hostˉservices: new(
                        Nativeˉoutputˉchannel.Fromˉfileˉhandle(Readˉonly.SafeFileHandle),
                        Authorizedˉcapabilities)));
        }
        finally
        {
            File.Delete(Failureˉpath);
        }

        var Dualˉoutputˉverified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess("""
            module Nativeˉoutputˉchannels profile hosted;

            capability console.write_line;
            capability diagnostic.write_line;

            export fn Main() -> i32 {
                console.write_line("console-€-😀");
                diagnostic.write_line("diagnostic-€-😀");
                console.write_line("");
                return 0;
            }
            """));
        var Dualˉoutputˉfragment = X64ˉnativeˉbackend.Compile(Dualˉoutputˉverified).Fragment;
        var Dualˉoutputˉcapabilities = ImmutableHashSet.Create(
            StringComparer.Ordinal,
            Capabilityˉcatalog.CONSOLE_WRITE_LINE,
            Capabilityˉcatalog.DIAGNOSTIC_WRITE_LINE);
        using (var Consoleˉcapture = new Nativeˉoutputˉcapture())
        using (var Diagnosticˉcapture = new Nativeˉoutputˉcapture())
        {
            Equal(
                0,
                X64ˉnativeˉexecutor.Executeˉi32(
                    Dualˉoutputˉfragment,
                    hostˉservices: new(
                        Consoleˉcapture.Channel,
                        Dualˉoutputˉcapabilities,
                        diagnosticˉoutput: Diagnosticˉcapture.Channel)));
            Equal("console-€-😀\n\n", Consoleˉcapture.Readˉtext());
            Equal("diagnostic-€-😀\n", Diagnosticˉcapture.Readˉtext());
        }

        var Objectˉbytes = Nativeˉobjectˉsink.Writeˉwvo(First.Fragment);
        Sequenceˉequal(Objectˉbytes, Nativeˉobjectˉsink.Writeˉwvo(Second.Fragment));
        var Object = Objectˉcodec.Readˉandˉverify(Objectˉbytes.AsSpan()).Value;
        Equal(2, Object.Sections.Length);
        Equal(".rodata", Object.Sections[1].Name);
        Equal(Objectˉsectionˉkind.Readˉonlyˉdata, Object.Sections[1].Kind);
        Sequenceˉequal(
            System.Text.Encoding.UTF8.GetBytes("Hello from Windvale"),
            Object.Sections[1].Data);
        var Linked = Linkˉsuccess(
            [Objectˉbytes.ToArray()],
            new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"));
        Sequenceˉequal(First.Fragment.Code, Linked.Imageˉbytes);
        using var Linkedˉoutput = new Nativeˉoutputˉcapture();
        Equal(
            0,
            X64ˉnativeˉexecutor.Executeˉi32(
                First.Fragment with { Code = Linked.Imageˉbytes },
                maximumˉinstructions: Interpreted.Executedˉinstructions,
                hostˉservices: new(Linkedˉoutput.Channel, Authorizedˉcapabilities)));
        Equal("Hello from Windvale\n", Linkedˉoutput.Readˉtext());

        var Textˉpatch = First.Fragment.Patches.Single();
        var Corruptedˉservice = First.Fragment.Code.ToArray();
        Corruptedˉservice[checked((int)Textˉpatch.Offset - 3)] = 0x90;
        Throwsˉnative(
            "WVN3030",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                First.Fragment with { Code = Corruptedˉservice.ToImmutableArray() }));

        var Corruptedˉpatch = First.Fragment.Code.ToArray();
        Corruptedˉpatch[checked((int)Textˉpatch.Offset)] ^= 0x01;
        Throwsˉnative(
            "WVN3024",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                First.Fragment with { Code = Corruptedˉpatch.ToImmutableArray() }));

        var Textˉsymbol = First.Fragment.Symbols.Single(Symbol =>
            Symbol.Kind == Nativeˉsymbolˉkind.Data);
        var Invalidˉutf8 = First.Fragment.Code.ToArray();
        Invalidˉutf8[checked((int)Textˉsymbol.Offset)] = 0xFF;
        Throwsˉnative(
            "WVN3030",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                First.Fragment with { Code = Invalidˉutf8.ToImmutableArray() }));
        Throwsˉnative(
            "WVN3030",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                First.Fragment with { Requiredˉservices = [] }));
    }

    private static void Windvaleˉnativeˉstencilsˉreproduceˉargumentˉservices()
    {
        const string INVALID_STENCIL =
            "The WVA native stencil does not match the bounded native-service contract.";
        var Assemblerˉmodule = Moduleˉcodec.Readˉandˉverify(Compileˉwithˉtoolˉfoundationˉsuccess(
            WVA_ASSEMBLER_CORE_SOURCE,
            "Wva-Assembler-Core.wv"));
        var Authorized = Assemblerˉmodule.Module.Capabilities
            .Select(Capability => Capability.Name)
            .ToImmutableHashSet(StringComparer.Ordinal);

        ImmutableArray<byte> Assembleˉtwice(
            string source,
            string sourceˉname,
            string outputˉname)
        {
            var Sourceˉbytes = Encoding.UTF8.GetBytes(source).ToImmutableArray();

            Capturingˉfileˉwriter Runˉwindvaleˉassembler()
            {
                var Output = new StringWriter();
                var Diagnostics = new StringWriter();
                var Writer = new Capturingˉfileˉwriter();
                var Result = new Referenceˉruntime(
                    Assemblerˉmodule,
                    new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                        [sourceˉname, outputˉname],
                        Output,
                        Diagnostics,
                        new Testˉfileˉreader((Name, Maximumˉbytes) =>
                        {
                            Equal(sourceˉname, Name);
                            True(
                                Sourceˉbytes.Length <= Maximumˉbytes,
                                "The native-stencil source exceeded the Windvale assembler input bound.");
                            return Sourceˉbytes;
                        }),
                        Writer)),
                    new(Authorized, Maximumˉinstructions: 10_000_000)).Runˉmain();
                Equal(0, Result.Exitˉcode);
                Equal(string.Empty, Diagnostics.ToString());
                Contains(Output.ToString(), "assembly status=valid");
                Equal(1, Writer.Writeˉcount);
                Equal(outputˉname, Writer.Resourceˉname);
                return Writer;
            }

            var First = Runˉwindvaleˉassembler();
            var Second = Runˉwindvaleˉassembler();
            Sequenceˉequal(First.Bytes, Second.Bytes);
            Sequenceˉequal(Assembleˉsuccess(source), First.Bytes);
            return First.Bytes;
        }

        static Verifiedˉobject Corruptˉdata(
            Verifiedˉobject verified,
            int offset,
            byte value)
        {
            var Object = verified.Value;
            var Section = Object.Sections.Single();
            var Data = Section.Data.ToArray();
            Data[offset] = value;
            return Objectˉverifier.Verify(Object with
            {
                Sections = [Section with { Data = Data.ToImmutableArray() }],
            });
        }

        static void Verifyˉembeddedˉobject(
            ImmutableArray<byte> expected,
            string resource,
            int size,
            string sha256)
        {
            using var Embeddedˉstream = typeof(X64ˉnativeˉstencil).Assembly
                .GetManifestResourceStream(resource) ??
                throw new InvalidOperationException($"The native-stencil resource {resource} was not embedded.");
            var Embeddedˉbytes = new byte[checked((int)Embeddedˉstream.Length)];
            Embeddedˉstream.ReadExactly(Embeddedˉbytes);
            Sequenceˉequal(expected, Embeddedˉbytes);
            Equal(size, Embeddedˉbytes.Length);
            Equal(
                sha256,
                Convert.ToHexString(SHA256.HashData(Embeddedˉbytes)).ToLowerInvariant());
        }

        var Countˉbytes = Assembleˉtwice(
            PROCESS_ARGUMENT_COUNT_STENCIL_SOURCE,
            "Process-Argument-Count.wva",
            "Process-Argument-Count.wvo");
        Verifyˉembeddedˉobject(
            Countˉbytes,
            "Windvale.NativeCompiler.Process-Argument-Count.wvo",
            166,
            "e2057943b9c79e10a432ea20a77da5ed0a261e3effdd36511cbb34e77e55c10b");
        var Countˉverified = Objectˉcodec.Readˉandˉverify(Countˉbytes.AsSpan());
        var Countˉstencil = X64ˉnativeˉstencil.Readˉprocessˉargumentˉcount(Countˉverified);
        var Countˉinstantiated = X64ˉnativeˉstencil.Instantiateˉu8(
            Countˉstencil,
            Nativeˉstencilˉpatchˉkind.Executionˉcontextˉu8ˉoffset,
            checked((byte)Nativeˉexecutionˉcontextˉcontract.ARGUMENT_COUNT_OFFSET));
        Sequenceˉequal(
            X64ˉnativeˉargumentˉservices.Build(Nativeˉservice.Processˉargumentˉcount),
            Countˉinstantiated);
        Equal(
            X64ˉnativeˉargumentˉservices.ARGUMENT_COUNT_CANONICAL_SIZE,
            Countˉinstantiated.Length);
        Equal(
            X64ˉnativeˉargumentˉservices.ARGUMENT_COUNT_CANONICAL_SHA256,
            Convert.ToHexString(SHA256.HashData(Countˉinstantiated.AsSpan())).ToLowerInvariant());
        X64ˉnativeˉargumentˉservices.Verify(
            Nativeˉservice.Processˉargumentˉcount,
            Countˉinstantiated.AsSpan());

        Throwsˉinvalidˉoperation(
            INVALID_STENCIL,
            () => _ = X64ˉnativeˉstencil.Readˉprocessˉargumentˉcount(
                Corruptˉdata(Countˉverified, 8, 4)));
        Throwsˉinvalidˉoperation(
            INVALID_STENCIL,
            () => _ = X64ˉnativeˉstencil.Readˉprocessˉargumentˉcount(
                Corruptˉdata(Countˉverified, 12, 2)));
        Throwsˉinvalidˉoperation(
            INVALID_STENCIL,
            () => _ = X64ˉnativeˉstencil.Readˉprocessˉargumentˉcount(
                Corruptˉdata(Countˉverified, 16, 2)));
        Throwsˉinvalidˉoperation(
            INVALID_STENCIL,
            () => _ = X64ˉnativeˉstencil.Readˉprocessˉargumentˉcount(
                Corruptˉdata(Countˉverified, 20, 0x40)));
        Throwsˉinvalidˉoperation(
            INVALID_STENCIL,
            () => _ = X64ˉnativeˉstencil.Instantiateˉu8(
                Countˉstencil,
                Nativeˉstencilˉpatchˉkind.Executionˉcontextˉserviceˉfailureˉdetailˉu8ˉoffset,
                checked((byte)Nativeˉexecutionˉcontextˉcontract.ARGUMENT_COUNT_OFFSET)));
        Throwsˉinvalidˉoperation(
            INVALID_STENCIL,
            () => _ = X64ˉnativeˉstencil.Instantiateˉu8(
                Countˉstencil with { Template = Countˉinstantiated },
                Nativeˉstencilˉpatchˉkind.Executionˉcontextˉu8ˉoffset,
                checked((byte)Nativeˉexecutionˉcontextˉcontract.ARGUMENT_COUNT_OFFSET)));

        var Argumentˉbytes = Assembleˉtwice(
            PROCESS_ARGUMENT_STENCIL_SOURCE,
            "Process-Argument.wva",
            "Process-Argument.wvo");
        Verifyˉembeddedˉobject(
            Argumentˉbytes,
            "Windvale.NativeCompiler.Process-Argument.wvo",
            321,
            "307e61dcb2a156eb0d4b77f7d93676d7b1ac24f9bb6fe1f31217837213352bad");
        var Argumentˉverified = Objectˉcodec.Readˉandˉverify(Argumentˉbytes.AsSpan());
        var Argumentˉstencil = X64ˉnativeˉstencil.Readˉprocessˉargument(Argumentˉverified);
        Equal(2u, Argumentˉstencil.Formatˉversion);
        Equal(8, Argumentˉstencil.Patches.Length);
        Equal(
            2,
            Argumentˉstencil.Patches.Count(Patch =>
                Patch.Kind ==
                    Nativeˉstencilˉpatchˉkind.Executionˉcontextˉserviceˉfailureˉdetailˉu8ˉoffset));
        Equal(
            2,
            Argumentˉstencil.Patches.Count(Patch =>
                Patch.Kind == Nativeˉstencilˉpatchˉkind.Borrowedˉtextˉlengthˉu8ˉoffset));
        var Argumentˉinstantiated = X64ˉnativeˉstencil.Instantiateˉprocessˉargument(
            Argumentˉstencil);
        Sequenceˉequal(
            X64ˉnativeˉargumentˉservices.Build(Nativeˉservice.Processˉargument),
            Argumentˉinstantiated);
        Equal(
            X64ˉnativeˉargumentˉservices.ARGUMENT_CANONICAL_SIZE,
            Argumentˉinstantiated.Length);
        Equal(
            X64ˉnativeˉargumentˉservices.ARGUMENT_CANONICAL_SHA256,
            Convert.ToHexString(SHA256.HashData(Argumentˉinstantiated.AsSpan())).ToLowerInvariant());
        X64ˉnativeˉargumentˉservices.Verify(
            Nativeˉservice.Processˉargument,
            Argumentˉinstantiated.AsSpan());

        Throwsˉinvalidˉoperation(
            INVALID_STENCIL,
            () => _ = X64ˉnativeˉstencil.Readˉprocessˉargument(
                Corruptˉdata(Argumentˉverified, 8, 7)));
        Throwsˉinvalidˉoperation(
            INVALID_STENCIL,
            () => _ = X64ˉnativeˉstencil.Readˉprocessˉargument(
                Corruptˉdata(Argumentˉverified, 12, 69)));
        Throwsˉinvalidˉoperation(
            INVALID_STENCIL,
            () => _ = X64ˉnativeˉstencil.Readˉprocessˉargument(
                Corruptˉdata(Argumentˉverified, 16, 4)));
        Throwsˉinvalidˉoperation(
            INVALID_STENCIL,
            () => _ = X64ˉnativeˉstencil.Readˉprocessˉargument(
                Corruptˉdata(Argumentˉverified, 20, 2)));
        Throwsˉinvalidˉoperation(
            INVALID_STENCIL,
            () => _ = X64ˉnativeˉstencil.Readˉprocessˉargument(
                Corruptˉdata(Argumentˉverified, 24, 3)));
        Throwsˉinvalidˉoperation(
            INVALID_STENCIL,
            () => _ = X64ˉnativeˉstencil.Readˉprocessˉargument(
                Corruptˉdata(Argumentˉverified, 112, 0x40)));
        Throwsˉinvalidˉoperation(
            INVALID_STENCIL,
            () => _ = X64ˉnativeˉstencil.Readˉprocessˉargument(
                Corruptˉdata(Argumentˉverified, 115, 1)));
        Throwsˉinvalidˉoperation(
            INVALID_STENCIL,
            () => _ = X64ˉnativeˉstencil.Instantiateˉu8(
                Argumentˉstencil,
                Nativeˉstencilˉpatchˉkind.Executionˉcontextˉserviceˉfailureˉdetailˉu8ˉoffset,
                checked((byte)Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET)));
        var Duplicateˉpatches = Argumentˉstencil.Patches.ToArray();
        Duplicateˉpatches[1] = Duplicateˉpatches[1] with { Offset = Duplicateˉpatches[0].Offset };
        Throwsˉinvalidˉoperation(
            INVALID_STENCIL,
            () => _ = X64ˉnativeˉstencil.Instantiateˉprocessˉargument(
                Argumentˉstencil with { Patches = Duplicateˉpatches.ToImmutableArray() }));
        Throwsˉinvalidˉoperation(
            INVALID_STENCIL,
            () => _ = X64ˉnativeˉstencil.Instantiateˉprocessˉargument(
                Argumentˉstencil with { Template = Argumentˉinstantiated }));
    }

    private static void Windvaleˉnativeˉstencilˉconsumerˉruns()
    {
        var Coreˉresult = Seedˉcompiler.Compileˉmodules(
            new("Compiler/Windvale/Native-Stencil-Core.wv", NATIVE_STENCIL_CORE_SOURCE),
            []);
        True(
            Coreˉresult.Success,
            "The Windvale native-stencil core did not compile: " +
                string.Join(" | ", Coreˉresult.Diagnostics));
        Equal(
            NATIVE_STENCIL_CORE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Coreˉresult.Moduleˉbytes.AsSpan()));

        var Demoˉresult = Seedˉcompiler.Compileˉmodules(
            new("Examples/Compiler/Native-Stencil-Demo.wv", NATIVE_STENCIL_DEMO_SOURCE),
            [
                new("Compiler/Windvale/Native-Stencil-Core.wv", NATIVE_STENCIL_CORE_SOURCE),
            ]);
        True(
            Demoˉresult.Success,
            "The Windvale native-stencil demo did not compile: " +
                string.Join(" | ", Demoˉresult.Diagnostics));
        Equal(
            NATIVE_STENCIL_DEMO_SHA256,
            Moduleˉdigest.Calculateˉsha256(Demoˉresult.Moduleˉbytes.AsSpan()));
        var Demo = Moduleˉcodec.Readˉandˉverify(Demoˉresult.Moduleˉbytes.AsSpan());

        static ImmutableArray<byte> Readˉnativeˉstencilˉresource(string name)
        {
            using var Stream = typeof(X64ˉnativeˉstencil).Assembly
                .GetManifestResourceStream(name) ??
                throw new InvalidOperationException($"The native-stencil resource {name} was not embedded.");
            var Bytes = new byte[checked((int)Stream.Length)];
            Stream.ReadExactly(Bytes);
            return Bytes.ToImmutableArray();
        }

        var Countˉdata = (Bytesˉdataˉdeclaration)Demo.Module.Data.Single(
            Data => Data.Name == "Argumentˉcountˉobject");
        var Argumentˉdata = (Bytesˉdataˉdeclaration)Demo.Module.Data.Single(
            Data => Data.Name == "Argumentˉobject");
        Sequenceˉequal(
            Readˉnativeˉstencilˉresource(
                "Windvale.NativeCompiler.Process-Argument-Count.wvo"),
            Countˉdata.Values);
        Sequenceˉequal(
            Readˉnativeˉstencilˉresource("Windvale.NativeCompiler.Process-Argument.wvo"),
            Argumentˉdata.Values);

        var Interpreted = new Referenceˉruntime(
            Demo,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults with { Maximumˉinstructions = 20_000_000 })
            .Runˉmain();
        Equal(0, Interpreted.Exitˉcode);

        var First = X64ˉnativeˉbackend.Compile(Demo);
        var Second = X64ˉnativeˉbackend.Compile(Demo);
        Sequenceˉequal(First.Fragment.Code, Second.Fragment.Code);
        Sequenceˉequal(First.Fragment.Symbols, Second.Fragment.Symbols);
        Sequenceˉequal(First.Fragment.Patches, Second.Fragment.Patches);
        Sequenceˉequal(First.Fragment.Types, Second.Fragment.Types);
        var Operations = First.Module.Functions
            .SelectMany(Function => Function.Blocks)
            .SelectMany(Block => Block.Operations)
            .ToImmutableArray();
        True(Operations.Any(Operation => Operation is Nativeˉbytesˉslice),
            "The Windvale native-stencil consumer omitted immutable byte slicing.");
        True(Operations.Any(Operation => Operation is Nativeˉbytesˉconcat),
            "The Windvale native-stencil consumer omitted immutable byte replacement.");
        True(Operations.Any(Operation => Operation is Nativeˉbytesˉfromˉu32ˉlittle),
            "The Windvale native-stencil consumer omitted typed patch-byte construction.");
        True(Operations.Any(Operation => Operation is Nativeˉrecordˉcreate),
            "The Windvale native-stencil consumer omitted typed result construction.");
        True(Operations.Any(Operation => Operation is Nativeˉenumˉcomparison),
            "The Windvale native-stencil consumer omitted typed status checks.");
        _ = Nativeˉfragmentˉverifier.Verify(First.Fragment);
        Equal(
            Interpreted.Exitˉcode,
            X64ˉnativeˉexecutor.Executeˉi32(
                First.Fragment,
                maximumˉinstructions: Interpreted.Executedˉinstructions));

        var Firstˉobject = Nativeˉobjectˉsink.Writeˉwvo(First.Fragment);
        var Secondˉobject = Nativeˉobjectˉsink.Writeˉwvo(Second.Fragment);
        Sequenceˉequal(Firstˉobject, Secondˉobject);
        var Linked = Linkˉsuccess(
            [Firstˉobject.ToArray()],
            new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"));
        Sequenceˉequal(First.Fragment.Code, Linked.Imageˉbytes);
        Equal(
            Interpreted.Exitˉcode,
            X64ˉnativeˉexecutor.Executeˉi32(
                First.Fragment with { Code = Linked.Imageˉbytes },
                maximumˉinstructions: Interpreted.Executedˉinstructions));
    }

    private static void Windvaleˉnativeˉstencilˉbridgeˉruns()
    {
        var Bridgeˉresult = Seedˉcompiler.Compileˉmodules(
            new("Compiler/Windvale/Native-Stencil-Bridge.wv", NATIVE_STENCIL_BRIDGE_SOURCE),
            [
                new("Compiler/Windvale/Native-Stencil-Core.wv", NATIVE_STENCIL_CORE_SOURCE),
            ]);
        True(
            Bridgeˉresult.Success,
            "The Windvale native-stencil bridge did not compile: " +
                string.Join(" | ", Bridgeˉresult.Diagnostics));
        Equal(
            NATIVE_STENCIL_BRIDGE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Bridgeˉresult.Moduleˉbytes.AsSpan()));
        Equal(X64ˉnativeˉargumentˉservices.CONSUMER_CANONICAL_SIZE, Bridgeˉresult.Moduleˉbytes.Length);
        Equal(X64ˉnativeˉargumentˉservices.CONSUMER_CANONICAL_SHA256, NATIVE_STENCIL_BRIDGE_SHA256);

        using (var Stream = typeof(X64ˉnativeˉargumentˉservices).Assembly
            .GetManifestResourceStream("Windvale.Native.Native-Stencil-Bridge.wvb") ??
            throw new InvalidOperationException("The retained native-stencil bridge was not embedded."))
        {
            var Retained = new byte[checked((int)Stream.Length)];
            Stream.ReadExactly(Retained);
            Sequenceˉequal(Bridgeˉresult.Moduleˉbytes, Retained);
        }

        var Bridge = Moduleˉcodec.Readˉandˉverify(Bridgeˉresult.Moduleˉbytes.AsSpan());
        var Countˉdata = (Bytesˉdataˉdeclaration)Bridge.Module.Data.Single(
            Data => Data.Name == "Argumentˉcountˉobject");
        var Argumentˉdata = (Bytesˉdataˉdeclaration)Bridge.Module.Data.Single(
            Data => Data.Name == "Argumentˉobject");
        static ImmutableArray<byte> Readˉobjectˉresource(string name)
        {
            using var Stream = typeof(X64ˉnativeˉstencil).Assembly
                .GetManifestResourceStream(name) ??
                throw new InvalidOperationException($"The native-stencil object {name} was not embedded.");
            var Bytes = new byte[checked((int)Stream.Length)];
            Stream.ReadExactly(Bytes);
            return Bytes.ToImmutableArray();
        }
        Sequenceˉequal(
            Readˉobjectˉresource("Windvale.NativeCompiler.Process-Argument-Count.wvo"),
            Countˉdata.Values);
        Sequenceˉequal(
            Readˉobjectˉresource("Windvale.NativeCompiler.Process-Argument.wvo"),
            Argumentˉdata.Values);

        var Expectedˉbuilder = ImmutableArray.CreateBuilder<byte>(
            X64ˉnativeˉargumentˉservices.ARGUMENT_COUNT_CANONICAL_SIZE +
            X64ˉnativeˉargumentˉservices.ARGUMENT_CANONICAL_SIZE);
        Expectedˉbuilder.AddRange(X64ˉnativeˉstencil.Buildˉprocessˉargumentˉcount());
        Expectedˉbuilder.AddRange(X64ˉnativeˉstencil.Buildˉprocessˉargument());
        var Expected = Expectedˉbuilder.MoveToImmutable();

        var Reference = new Referenceˉruntime(
            Bridge,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults);
        var Interpreted = Reference.Runˉmainˉbytes();
        Sequenceˉequal(Expected, Interpreted.Bytes);
        Throwsˉruntime("WVR3003", () => _ = Reference.Runˉmain());

        var First = X64ˉnativeˉbackend.Compile(Bridge);
        var Second = X64ˉnativeˉbackend.Compile(Bridge);
        Sequenceˉequal(First.Fragment.Code, Second.Fragment.Code);
        Sequenceˉequal(First.Fragment.Symbols, Second.Fragment.Symbols);
        Sequenceˉequal(First.Fragment.Patches, Second.Fragment.Patches);
        Equal(
            Nativeˉentryˉresultˉkind.Descriptor,
            Nativeˉfragmentˉverifier.Verifyˉentryˉresultˉkind(First.Fragment));
        Sequenceˉequal(
            new byte[] { 0x48, 0x89, 0xC8 },
            First.Fragment.Code.AsSpan(30, 3).ToArray());
        Sequenceˉequal(
            Expected,
            X64ˉnativeˉexecutor.Executeˉbytes(
                First.Fragment,
                maximumˉinstructions: Interpreted.Executedˉinstructions));
        Throwsˉnative("WVN4011", () => _ = X64ˉnativeˉexecutor.Executeˉi32(First.Fragment));

        var Corruptedˉcode = First.Fragment.Code.ToArray();
        Corruptedˉcode[32] ^= 0x01;
        Throwsˉnative(
            "WVN3030",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                First.Fragment with { Code = Corruptedˉcode.ToImmutableArray() }));

        var Firstˉobject = Nativeˉobjectˉsink.Writeˉwvo(First.Fragment);
        var Secondˉobject = Nativeˉobjectˉsink.Writeˉwvo(Second.Fragment);
        Sequenceˉequal(Firstˉobject, Secondˉobject);
        var Linked = Linkˉsuccess(
            [Firstˉobject.ToArray()],
            new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"));
        Sequenceˉequal(First.Fragment.Code, Linked.Imageˉbytes);
        Sequenceˉequal(
            Expected,
            X64ˉnativeˉexecutor.Executeˉbytes(
                First.Fragment with { Code = Linked.Imageˉbytes },
                maximumˉinstructions: Interpreted.Executedˉinstructions));

        Sequenceˉequal(
            Expected.AsSpan(0, X64ˉnativeˉargumentˉservices.ARGUMENT_COUNT_CANONICAL_SIZE)
                .ToArray(),
            X64ˉnativeˉargumentˉservices.Build(Nativeˉservice.Processˉargumentˉcount));
        Sequenceˉequal(
            Expected.AsSpan()[X64ˉnativeˉargumentˉservices.ARGUMENT_COUNT_CANONICAL_SIZE..]
                .ToArray(),
            X64ˉnativeˉargumentˉservices.Build(Nativeˉservice.Processˉargument));

        var Scalar = X64ˉnativeˉbackend.Compile(
            Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(NATIVE_CONSTANT_SOURCE)));
        Throwsˉnative("WVN4011", () => _ = X64ˉnativeˉexecutor.Executeˉbytes(Scalar.Fragment));
        Throwsˉruntime(
            "WVR3003",
            () => _ = new Referenceˉruntime(
                Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(NATIVE_CONSTANT_SOURCE)),
                new Referenceˉcapabilityˉhost(TextWriter.Null),
                Runtimeˉoptions.Portableˉdefaults).Runˉmainˉbytes());
        var Staticˉbytes = X64ˉnativeˉbackend.Compile(
            Moduleˉcodec.Readˉandˉverify(Compileˉsuccess("""
                module Nativeˉstaticˉbytes profile portable;
                data Value: bytes = [1, 2, 3, 4];
                export fn Main() -> bytes { return Value; }
                """)));
        Sequenceˉequal(
            new byte[] { 1, 2, 3, 4 },
            X64ˉnativeˉexecutor.Executeˉbytes(Staticˉbytes.Fragment));
        var Hostedˉbytes = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess("""
            module Nativeˉhostedˉbytes profile hosted;
            capability file.read_bytes;
            data Name: text = "input.bin";
            export fn Main() -> bytes { return file.read_bytes(Name); }
            """));
        Throwsˉnative("WVN2002", () => _ = X64ˉnativeˉbackend.Compile(Hostedˉbytes));
    }

    private static void Windvaleˉnativeˉpublicationˉlayoutˉruns()
    {
        var Coreˉresult = Seedˉcompiler.Compileˉmodules(
            new("Compiler/Windvale/Native-Publication-Core.wv", NATIVE_PUBLICATION_CORE_SOURCE),
            []);
        True(
            Coreˉresult.Success,
            "The Windvale native-publication core did not compile: " +
                string.Join(" | ", Coreˉresult.Diagnostics));
        Equal(7_189, Coreˉresult.Moduleˉbytes.Length);
        Equal(
            NATIVE_PUBLICATION_CORE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Coreˉresult.Moduleˉbytes.AsSpan()));

        var Bridgeˉresult = Seedˉcompiler.Compileˉmodules(
            new("Compiler/Windvale/Native-Publication-Bridge.wv", NATIVE_PUBLICATION_BRIDGE_SOURCE),
            [
                new("Compiler/Windvale/Native-Publication-Core.wv", NATIVE_PUBLICATION_CORE_SOURCE),
            ]);
        True(
            Bridgeˉresult.Success,
            "The Windvale native-publication bridge did not compile: " +
                string.Join(" | ", Bridgeˉresult.Diagnostics));
        Equal(X64ˉnativeˉpublicationˉlayout.PLANNER_CANONICAL_SIZE, Bridgeˉresult.Moduleˉbytes.Length);
        Equal(
            NATIVE_PUBLICATION_BRIDGE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Bridgeˉresult.Moduleˉbytes.AsSpan()));
        Equal(
            X64ˉnativeˉpublicationˉlayout.PLANNER_CANONICAL_SHA256,
            NATIVE_PUBLICATION_BRIDGE_SHA256);

        using (var Stream = typeof(X64ˉnativeˉpublicationˉlayout).Assembly
            .GetManifestResourceStream("Windvale.Native.Native-Publication-Bridge.wvb") ??
            throw new InvalidOperationException("The retained native-publication bridge was not embedded."))
        {
            var Retained = new byte[checked((int)Stream.Length)];
            Stream.ReadExactly(Retained);
            Sequenceˉequal(Bridgeˉresult.Moduleˉbytes, Retained);
        }

        var Bridge = Moduleˉcodec.Readˉandˉverify(Bridgeˉresult.Moduleˉbytes.AsSpan());
        Equal(Moduleˉprofile.Hosted, Bridge.Module.Profile);
        Equal(Capabilityˉcatalog.FILE_READ_BYTES, Bridge.Module.Capabilities.Single().Name);
        var Main = Bridge.Module.Exports.Single(Export => Export.Name == "Main");
        Equal(Valueˉtype.Bytes, Bridge.Module.Functions[Main.Targetˉindex].Returnˉtype.Kind);

        var Noˉservices = ImmutableArray<Nativeˉpublicationˉservice>.Empty;
        var Emptyˉplan = X64ˉnativeˉpublicationˉlayout.Plan(5, Noˉservices);
        Equal(5, Emptyˉplan.Fragmentˉbytes);
        Equal(16, Emptyˉplan.Imageˉbytes);
        True(Emptyˉplan.Placements.IsEmpty, "A service-free image received a service placement.");

        var Services = ImmutableArray.Create(
            new Nativeˉpublicationˉservice(Nativeˉservice.Processˉargumentˉcount, 5),
            new Nativeˉpublicationˉservice(Nativeˉservice.Processˉargument, 70));
        var First = X64ˉnativeˉpublicationˉlayout.Plan(5, Services);
        var Second = X64ˉnativeˉpublicationˉlayout.Plan(5, Services);
        Equal(102, First.Imageˉbytes);
        Sequenceˉequal(
            new[]
            {
                new Nativeˉpublicationˉplacement(
                    Nativeˉservice.Processˉargumentˉcount,
                    16,
                    5),
                new Nativeˉpublicationˉplacement(
                    Nativeˉservice.Processˉargument,
                    32,
                    70),
            },
            First.Placements);
        Sequenceˉequal(First.Placements, Second.Placements);

        var Maximumˉfragment = X64ˉnativeˉpublicationˉlayout.Plan(
            Nativeˉcontract.MAXIMUM_CODE_BYTES,
            Noˉservices);
        Equal(Nativeˉcontract.MAXIMUM_CODE_BYTES, Maximumˉfragment.Imageˉbytes);
        var Maximumˉimage = X64ˉnativeˉpublicationˉlayout.Plan(
            1,
            [
                new(
                    Nativeˉservice.Consoleˉwriteˉline,
                    X64ˉnativeˉpublicationˉlayout.MAXIMUM_IMAGE_BYTES - 16),
            ]);
        Equal(X64ˉnativeˉpublicationˉlayout.MAXIMUM_IMAGE_BYTES, Maximumˉimage.Imageˉbytes);
        var Allˉservices = Enumerable.Range(1, X64ˉnativeˉpublicationˉlayout.MAXIMUM_SERVICES)
            .Select(Value => new Nativeˉpublicationˉservice((Nativeˉservice)Value, 1))
            .ToImmutableArray();
        var Allˉplan = X64ˉnativeˉpublicationˉlayout.Plan(1, Allˉservices);
        Equal(193, Allˉplan.Imageˉbytes);
        Equal(16, Allˉplan.Placements[0].Offset);
        Equal(192, Allˉplan.Placements[^1].Offset);

        Throwsˉnative(
            "WVN4013",
            () => _ = X64ˉnativeˉpublicationˉlayout.Buildˉrequest(0, Noˉservices));
        Throwsˉnative(
            "WVN4013",
            () => _ = X64ˉnativeˉpublicationˉlayout.Buildˉrequest(
                Nativeˉcontract.MAXIMUM_CODE_BYTES + 1,
                Noˉservices));
        Throwsˉnative(
            "WVN4013",
            () => _ = X64ˉnativeˉpublicationˉlayout.Buildˉrequest(
                1,
                [new((Nativeˉservice)13, 1)]));
        Throwsˉnative(
            "WVN4013",
            () => _ = X64ˉnativeˉpublicationˉlayout.Buildˉrequest(
                1,
                [new(Nativeˉservice.Consoleˉwriteˉline, 0)]));
        Throwsˉnative(
            "WVN4013",
            () => _ = X64ˉnativeˉpublicationˉlayout.Buildˉrequest(
                1,
                [
                    new(Nativeˉservice.Processˉargument, 1),
                    new(Nativeˉservice.Processˉargumentˉcount, 1),
                ]));
        Throwsˉnative(
            "WVN4013",
            () => _ = X64ˉnativeˉpublicationˉlayout.Plan(
                1,
                [
                    new(
                        Nativeˉservice.Consoleˉwriteˉline,
                        X64ˉnativeˉpublicationˉlayout.MAXIMUM_IMAGE_BYTES),
                ]));

        var Request = X64ˉnativeˉpublicationˉlayout.Buildˉrequest(5, Services);
        var Response = X64ˉnativeˉpublicationˉlayout.Evaluateˉrequest(Request);
        var Repeatedˉresponse = X64ˉnativeˉpublicationˉlayout.Evaluateˉrequest(Request);
        Sequenceˉequal(Response, Repeatedˉresponse);
        Sequenceˉequal(
            First.Placements,
            X64ˉnativeˉpublicationˉlayout.Verifyˉresponse(5, Services, Response).Placements);

        static ImmutableArray<byte> Replaceˉu32(
            ImmutableArray<byte> input,
            int offset,
            uint value)
        {
            var Result = input.ToArray();
            BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(offset), value);
            return Result.ToImmutableArray();
        }

        static void Expectˉrequestˉfailure(
            ImmutableArray<byte> request,
            Nativeˉpublicationˉstatus status,
            uint failureˉoffset)
        {
            var Result = X64ˉnativeˉpublicationˉlayout.Evaluateˉrequest(request);
            Equal(X64ˉnativeˉpublicationˉlayout.RESPONSE_HEADER_BYTES, Result.Length);
            Equal(
                X64ˉnativeˉpublicationˉlayout.RESPONSE_MAGIC,
                BinaryPrimitives.ReadUInt32LittleEndian(Result.AsSpan()));
            Equal(
                (uint)status,
                BinaryPrimitives.ReadUInt32LittleEndian(Result.AsSpan()[12..]));
            Equal(
                failureˉoffset,
                BinaryPrimitives.ReadUInt32LittleEndian(Result.AsSpan()[16..]));
        }

        Expectˉrequestˉfailure([], Nativeˉpublicationˉstatus.Invalidˉsize, 0);
        Expectˉrequestˉfailure(
            Request.AsSpan(0, 23).ToArray().ToImmutableArray(),
            Nativeˉpublicationˉstatus.Invalidˉsize,
            23);
        Expectˉrequestˉfailure(
            Replaceˉu32(Request, 0, 0),
            Nativeˉpublicationˉstatus.Invalidˉmagic,
            0);
        Expectˉrequestˉfailure(
            Replaceˉu32(Request, 4, 2),
            Nativeˉpublicationˉstatus.Invalidˉversion,
            4);
        Expectˉrequestˉfailure(
            Replaceˉu32(Request, 8, (uint)Request.Length + 1),
            Nativeˉpublicationˉstatus.Invalidˉsize,
            8);
        Expectˉrequestˉfailure(
            Replaceˉu32(Request, 12, 0),
            Nativeˉpublicationˉstatus.Invalidˉfragment,
            12);
        Expectˉrequestˉfailure(
            Replaceˉu32(Request, 12, Nativeˉcontract.MAXIMUM_CODE_BYTES + 1u),
            Nativeˉpublicationˉstatus.Invalidˉfragment,
            12);
        Expectˉrequestˉfailure(
            Replaceˉu32(Request, 16, 13),
            Nativeˉpublicationˉstatus.Invalidˉservice,
            16);
        Expectˉrequestˉfailure(
            Replaceˉu32(Request, 20, 1),
            Nativeˉpublicationˉstatus.Invalidˉreserved,
            20);
        Expectˉrequestˉfailure(
            Replaceˉu32(Request, 24, 0),
            Nativeˉpublicationˉstatus.Invalidˉservice,
            24);
        Expectˉrequestˉfailure(
            Replaceˉu32(Request, 28, 0),
            Nativeˉpublicationˉstatus.Invalidˉrange,
            28);
        Expectˉrequestˉfailure(
            Replaceˉu32(Request, 32, 1),
            Nativeˉpublicationˉstatus.Invalidˉreserved,
            32);
        Expectˉrequestˉfailure(
            Replaceˉu32(Request, 36, (uint)Nativeˉservice.Processˉargumentˉcount),
            Nativeˉpublicationˉstatus.Invalidˉorder,
            36);
        Expectˉrequestˉfailure(
            Replaceˉu32(
                Request,
                28,
                X64ˉnativeˉpublicationˉlayout.MAXIMUM_IMAGE_BYTES),
            Nativeˉpublicationˉstatus.Imageˉlimit,
            28);
        Expectˉrequestˉfailure(
            Request.Add(0),
            Nativeˉpublicationˉstatus.Invalidˉsize,
            8);

        Throwsˉnative(
            "WVN4014",
            () => _ = X64ˉnativeˉpublicationˉlayout.Verifyˉresponse(
                5,
                Services,
                Response.AsSpan(0, 31).ToArray().ToImmutableArray()));
        foreach (var (Offset, Value) in new (int Offset, uint Value)[]
        {
            (0, 0),
            (4, 2),
            (8, (uint)Response.Length + 1),
            (12, 99),
            (16, 0),
            (20, 6),
            (24, 103),
            (28, 1),
            (32, (uint)Nativeˉservice.Consoleˉwriteˉline),
            (36, 17),
            (40, 6),
            (36, uint.MaxValue),
            (40, uint.MaxValue),
        })
        {
            Throwsˉnative(
                "WVN4014",
                () => _ = X64ˉnativeˉpublicationˉlayout.Verifyˉresponse(
                    5,
                    Services,
                    Replaceˉu32(Response, Offset, Value)));
        }
        Throwsˉnative(
            "WVN4013",
            () => _ = X64ˉnativeˉpublicationˉlayout.Verifyˉresponse(
                5,
                Services,
                Replaceˉu32(Response, 12, (uint)Nativeˉpublicationˉstatus.Imageˉlimit)));

        var Dataˉfragment = X64ˉnativeˉbackend.Compile(
            Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(
                "module Nativeˉpublicationˉdata profile portable; data Values: [i32] = [3, 5, 8, 13]; export fn Main() -> i32 { return Values[3]; }")))
            .Fragment;
        True(!Dataˉfragment.Patches.IsEmpty, "The publication fixture omitted its relative data patch.");
        foreach (var Patch in Dataˉfragment.Patches)
        {
            var Symbol = Dataˉfragment.Symbols.Single(Item => Item.Name == Patch.Symbol);
            Equal(
                checked((int)Symbol.Offset + Patch.Addend - (int)Patch.Offset),
                BinaryPrimitives.ReadInt32LittleEndian(
                    Dataˉfragment.Code.AsSpan(checked((int)Patch.Offset), sizeof(int))));
        }
        Equal(13, X64ˉnativeˉexecutor.Executeˉi32(Dataˉfragment));
    }

    private static void Windvaleˉnativeˉpublicationˉlifetimeˉruns()
    {
        var Coreˉresult = Seedˉcompiler.Compileˉmodules(
            new(
                "Compiler/Windvale/Native-Publication-Lifetime-Core.wv",
                NATIVE_PUBLICATION_LIFETIME_CORE_SOURCE),
            []);
        True(
            Coreˉresult.Success,
            "The Windvale native publication-lifetime core did not compile: " +
                string.Join(" | ", Coreˉresult.Diagnostics));
        Equal(4_954, Coreˉresult.Moduleˉbytes.Length);
        Equal(
            NATIVE_PUBLICATION_LIFETIME_CORE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Coreˉresult.Moduleˉbytes.AsSpan()));

        var Bridgeˉresult = Seedˉcompiler.Compileˉmodules(
            new(
                "Compiler/Windvale/Native-Publication-Lifetime-Bridge.wv",
                NATIVE_PUBLICATION_LIFETIME_BRIDGE_SOURCE),
            [
                new(
                    "Compiler/Windvale/Native-Publication-Lifetime-Core.wv",
                    NATIVE_PUBLICATION_LIFETIME_CORE_SOURCE),
            ]);
        True(
            Bridgeˉresult.Success,
            "The Windvale native publication-lifetime bridge did not compile: " +
                string.Join(" | ", Bridgeˉresult.Diagnostics));
        Equal(
            X64ˉnativeˉpublicationˉlifetime.PLANNER_CANONICAL_SIZE,
            Bridgeˉresult.Moduleˉbytes.Length);
        Equal(
            NATIVE_PUBLICATION_LIFETIME_BRIDGE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Bridgeˉresult.Moduleˉbytes.AsSpan()));
        Equal(
            X64ˉnativeˉpublicationˉlifetime.PLANNER_CANONICAL_SHA256,
            NATIVE_PUBLICATION_LIFETIME_BRIDGE_SHA256);

        using (var Stream = typeof(X64ˉnativeˉpublicationˉlifetime).Assembly
            .GetManifestResourceStream("Windvale.Native.Native-Publication-Lifetime-Bridge.wvb") ??
            throw new InvalidOperationException(
                "The retained native publication-lifetime bridge was not embedded."))
        {
            var Retained = new byte[checked((int)Stream.Length)];
            Stream.ReadExactly(Retained);
            Sequenceˉequal(Bridgeˉresult.Moduleˉbytes, Retained);
        }

        var Bridge = Moduleˉcodec.Readˉandˉverify(Bridgeˉresult.Moduleˉbytes.AsSpan());
        Equal(Moduleˉprofile.Hosted, Bridge.Module.Profile);
        Equal(Capabilityˉcatalog.FILE_READ_BYTES, Bridge.Module.Capabilities.Single().Name);
        var Main = Bridge.Module.Exports.Single(Export => Export.Name == "Main");
        Equal(Valueˉtype.Bytes, Bridge.Module.Functions[Main.Targetˉindex].Returnˉtype.Kind);

        var Expected = ImmutableArray.Create(
            new Nativeˉpublicationˉtransition(
                Nativeˉpublicationˉstate.Unallocated,
                Nativeˉpublicationˉaction.Allocateˉwritable,
                Nativeˉpublicationˉstate.Writable),
            new Nativeˉpublicationˉtransition(
                Nativeˉpublicationˉstate.Writable,
                Nativeˉpublicationˉaction.Copyˉimage,
                Nativeˉpublicationˉstate.Copied),
            new Nativeˉpublicationˉtransition(
                Nativeˉpublicationˉstate.Writable,
                Nativeˉpublicationˉaction.Release,
                Nativeˉpublicationˉstate.Released),
            new Nativeˉpublicationˉtransition(
                Nativeˉpublicationˉstate.Copied,
                Nativeˉpublicationˉaction.Sealˉexecutable,
                Nativeˉpublicationˉstate.Executable),
            new Nativeˉpublicationˉtransition(
                Nativeˉpublicationˉstate.Copied,
                Nativeˉpublicationˉaction.Release,
                Nativeˉpublicationˉstate.Released),
            new Nativeˉpublicationˉtransition(
                Nativeˉpublicationˉstate.Executable,
                Nativeˉpublicationˉaction.Invoke,
                Nativeˉpublicationˉstate.Invoked),
            new Nativeˉpublicationˉtransition(
                Nativeˉpublicationˉstate.Executable,
                Nativeˉpublicationˉaction.Release,
                Nativeˉpublicationˉstate.Released),
            new Nativeˉpublicationˉtransition(
                Nativeˉpublicationˉstate.Invoked,
                Nativeˉpublicationˉaction.Release,
                Nativeˉpublicationˉstate.Released),
            new Nativeˉpublicationˉtransition(
                Nativeˉpublicationˉstate.Released,
                Nativeˉpublicationˉaction.Complete,
                Nativeˉpublicationˉstate.Released));
        var Plan = X64ˉnativeˉpublicationˉlifetime.Plan(102);
        Equal(102, Plan.Imageˉbytes);
        Sequenceˉequal(Expected, Plan.Transitions);
        Sequenceˉequal(
            Expected,
            X64ˉnativeˉpublicationˉlifetime.Plan(102).Transitions);
        Equal(1, X64ˉnativeˉpublicationˉlifetime.Plan(1).Imageˉbytes);
        Equal(
            X64ˉnativeˉpublicationˉlayout.MAXIMUM_IMAGE_BYTES,
            X64ˉnativeˉpublicationˉlifetime.Plan(
                X64ˉnativeˉpublicationˉlayout.MAXIMUM_IMAGE_BYTES).Imageˉbytes);

        Throwsˉnative(
            "WVN4015",
            () => _ = X64ˉnativeˉpublicationˉlifetime.Buildˉrequest(0));
        Throwsˉnative(
            "WVN4015",
            () => _ = X64ˉnativeˉpublicationˉlifetime.Buildˉrequest(
                X64ˉnativeˉpublicationˉlayout.MAXIMUM_IMAGE_BYTES + 1));

        var Request = X64ˉnativeˉpublicationˉlifetime.Buildˉrequest(102);
        var Response = X64ˉnativeˉpublicationˉlifetime.Evaluateˉrequest(Request);
        Sequenceˉequal(
            Response,
            X64ˉnativeˉpublicationˉlifetime.Evaluateˉrequest(Request));
        Sequenceˉequal(
            Expected,
            X64ˉnativeˉpublicationˉlifetime.Verifyˉresponse(102, Response).Transitions);

        static ImmutableArray<byte> Replaceˉu32(
            ImmutableArray<byte> input,
            int offset,
            uint value)
        {
            var Result = input.ToArray();
            BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(offset), value);
            return Result.ToImmutableArray();
        }

        static void Expectˉrequestˉfailure(
            ImmutableArray<byte> request,
            Nativeˉpublicationˉlifetimeˉstatus status,
            uint failureˉoffset)
        {
            var Result = X64ˉnativeˉpublicationˉlifetime.Evaluateˉrequest(request);
            Equal(X64ˉnativeˉpublicationˉlifetime.RESPONSE_HEADER_BYTES, Result.Length);
            Equal(
                X64ˉnativeˉpublicationˉlifetime.RESPONSE_MAGIC,
                BinaryPrimitives.ReadUInt32LittleEndian(Result.AsSpan()));
            Equal(
                (uint)status,
                BinaryPrimitives.ReadUInt32LittleEndian(Result.AsSpan()[12..]));
            Equal(
                failureˉoffset,
                BinaryPrimitives.ReadUInt32LittleEndian(Result.AsSpan()[16..]));
        }

        Expectˉrequestˉfailure([], Nativeˉpublicationˉlifetimeˉstatus.Invalidˉsize, 0);
        Expectˉrequestˉfailure(
            Request.AsSpan(0, 19).ToArray().ToImmutableArray(),
            Nativeˉpublicationˉlifetimeˉstatus.Invalidˉsize,
            19);
        Expectˉrequestˉfailure(
            Replaceˉu32(Request, 0, 0),
            Nativeˉpublicationˉlifetimeˉstatus.Invalidˉmagic,
            0);
        Expectˉrequestˉfailure(
            Replaceˉu32(Request, 4, 2),
            Nativeˉpublicationˉlifetimeˉstatus.Invalidˉversion,
            4);
        Expectˉrequestˉfailure(
            Replaceˉu32(Request, 8, 19),
            Nativeˉpublicationˉlifetimeˉstatus.Invalidˉsize,
            8);
        Expectˉrequestˉfailure(
            Replaceˉu32(Request, 12, 0),
            Nativeˉpublicationˉlifetimeˉstatus.Invalidˉimage,
            12);
        Expectˉrequestˉfailure(
            Replaceˉu32(
                Request,
                12,
                X64ˉnativeˉpublicationˉlayout.MAXIMUM_IMAGE_BYTES + 1u),
            Nativeˉpublicationˉlifetimeˉstatus.Invalidˉimage,
            12);
        Expectˉrequestˉfailure(
            Replaceˉu32(Request, 16, 1),
            Nativeˉpublicationˉlifetimeˉstatus.Invalidˉreserved,
            16);
        Expectˉrequestˉfailure(
            Request.Add(0),
            Nativeˉpublicationˉlifetimeˉstatus.Invalidˉsize,
            8);

        Throwsˉnative(
            "WVN4016",
            () => _ = X64ˉnativeˉpublicationˉlifetime.Verifyˉresponse(
                102,
                Response.AsSpan(0, 31).ToArray().ToImmutableArray()));
        foreach (var (Offset, Value) in new (int Offset, uint Value)[]
        {
            (0, 0),
            (4, 2),
            (8, (uint)Response.Length + 1),
            (12, 99),
            (16, 0),
            (20, 103),
            (24, 8),
            (28, 1),
            (32, 1),
            (36, 2),
            (40, 2),
            (44, uint.MaxValue),
            (48, uint.MaxValue),
            (52, uint.MaxValue),
        })
        {
            Throwsˉnative(
                "WVN4016",
                () => _ = X64ˉnativeˉpublicationˉlifetime.Verifyˉresponse(
                    102,
                    Replaceˉu32(Response, Offset, Value)));
        }
        Throwsˉnative(
            "WVN4015",
            () => _ = X64ˉnativeˉpublicationˉlifetime.Verifyˉresponse(
                102,
                Replaceˉu32(
                    Response,
                    12,
                    (uint)Nativeˉpublicationˉlifetimeˉstatus.Invalidˉimage)));
        Throwsˉnative(
            "WVN4016",
            () => _ = Nativeˉexecutableˉimage.Allocateˉwritable(
                Plan with { Transitions = Plan.Transitions.RemoveAt(0) }));
        Throwsˉnative(
            "WVN4016",
            () => _ = Nativeˉexecutableˉimage.Allocateˉwritable(
                Plan with
                {
                    Transitions = Plan.Transitions.SetItem(
                        0,
                        Plan.Transitions[0] with
                        {
                            Nextˉstate = Nativeˉpublicationˉstate.Executable,
                        }),
                }));

        var Writable = Nativeˉexecutableˉimage.Allocateˉwritable(
            X64ˉnativeˉpublicationˉlifetime.Plan(1));
        Equal(Nativeˉpublicationˉstate.Writable, Writable.State);
        Throwsˉnative("WVN4017", () => _ = Writable.Executableˉaddress);
        Throwsˉnative("WVN4017", () => Writable.Sealˉexecutable());
        Throwsˉnative("WVN4017", () => Writable.Copyˉimage([0x90, 0xC3]));
        Writable.Dispose();
        Equal(Nativeˉpublicationˉstate.Released, Writable.State);

        var Copied = Nativeˉexecutableˉimage.Allocateˉwritable(
            X64ˉnativeˉpublicationˉlifetime.Plan(1));
        Copied.Copyˉimage([0xC3]);
        Equal(Nativeˉpublicationˉstate.Copied, Copied.State);
        Copied.Dispose();
        Equal(Nativeˉpublicationˉstate.Released, Copied.State);

        var Sealed = Nativeˉexecutableˉimage.Allocateˉwritable(
            X64ˉnativeˉpublicationˉlifetime.Plan(1));
        Sealed.Copyˉimage([0xC3]);
        Sealed.Sealˉexecutable();
        Equal(Nativeˉpublicationˉstate.Executable, Sealed.State);
        Sealed.Dispose();
        Equal(Nativeˉpublicationˉstate.Released, Sealed.State);

        var Executable = Nativeˉexecutableˉimage.Allocateˉwritable(
            X64ˉnativeˉpublicationˉlifetime.Plan(1));
        try
        {
            Executable.Copyˉimage([0xC3]);
            Executable.Sealˉexecutable();
            Equal(Nativeˉpublicationˉstate.Executable, Executable.State);
            Throwsˉnative("WVN4017", () => Executable.Copyˉimage([0xC3]));
            Equal(29, Executable.Invoke(Address =>
            {
                True(Address != IntPtr.Zero, "The executable image exposed a null address.");
                return 29;
            }));
            Equal(Nativeˉpublicationˉstate.Invoked, Executable.State);
            Throwsˉnative("WVN4017", () => _ = Executable.Invoke(_ => 0));
        }
        finally
        {
            Executable.Dispose();
        }
        Equal(Nativeˉpublicationˉstate.Released, Executable.State);
        Executable.Dispose();
    }

    private static void Nativeˉhostedˉinputˉinspectsˉwvb()
    {
        var Source = WVB_HEADER_INSPECTOR_SOURCE;
        var Input = Compileˉsuccess(NATIVE_CONSTANT_SOURCE).ToImmutableArray();
        var Verified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(Source));
        var Authorized = ImmutableHashSet.Create(
            StringComparer.Ordinal,
            Capabilityˉcatalog.CONSOLE_WRITE_LINE,
            Capabilityˉcatalog.FILE_READ_BYTES,
            Capabilityˉcatalog.PROCESS_ARGUMENT,
            Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT);

        Hostedˉresourceˉcontext Makeˉresources(
            TextWriter output,
            Testˉfileˉreader reader,
            string resourceˉname = "input.wvb") =>
            new([resourceˉname], output, TextWriter.Null, reader);
        Testˉfileˉreader Makeˉreader() => new((Name, Maximum) =>
        {
            Equal("input.wvb", Name);
            True(Input.Length <= Maximum, "The WVB fixture exceeded the hosted reader bound.");
            return Input;
        });

        var Referenceˉoutput = new StringWriter();
        var Referenceˉreader = Makeˉreader();
        var Referenceˉresources = Makeˉresources(Referenceˉoutput, Referenceˉreader);
        var Reference = new Referenceˉruntime(
            Verified,
            new Referenceˉcapabilityˉhost(Referenceˉresources),
            new(Authorized)).Runˉmain();
        Equal(0, Reference.Exitˉcode);
        Equal("input.wvb\nwvb-header=pass\n", Referenceˉoutput.ToString());
        Equal(1, Referenceˉreader.Readˉcount);

        var First = X64ˉnativeˉbackend.Compile(Verified);
        var Second = X64ˉnativeˉbackend.Compile(Verified);
        Sequenceˉequal(First.Fragment.Code, Second.Fragment.Code);
        Sequenceˉequal(
            [
                Nativeˉservice.Consoleˉwriteˉline,
                Nativeˉservice.Processˉargumentˉcount,
                Nativeˉservice.Processˉargument,
                Nativeˉservice.Fileˉreadˉbytes,
            ],
            First.Fragment.Requiredˉservices);
        var Operations = First.Module.Functions
            .SelectMany(Function => Function.Blocks)
            .SelectMany(Block => Block.Operations)
            .ToImmutableArray();
        True(Operations.Any(Operation => Operation is Nativeˉprocessˉargumentˉcount),
            "Native machine IR omitted process.argument_count.");
        True(Operations.Any(Operation => Operation is Nativeˉprocessˉargument),
            "Native machine IR omitted process.argument.");
        True(Operations.Count(Operation => Operation is Nativeˉfileˉreadˉbytes) == 2,
            "Native machine IR did not retain both file.read_bytes calls.");
        foreach (var Service in new[]
        {
            Nativeˉservice.Processˉargumentˉcount,
            Nativeˉservice.Processˉargument,
        })
        {
            var Firstˉservice = X64ˉnativeˉargumentˉservices.Build(Service);
            var Secondˉservice = X64ˉnativeˉargumentˉservices.Build(Service);
            Sequenceˉequal(Firstˉservice, Secondˉservice);
            X64ˉnativeˉargumentˉservices.Verify(Service, Firstˉservice.AsSpan());
            var Corruptedˉservice = Firstˉservice.ToArray();
            Corruptedˉservice[0] ^= 0x01;
            Throwsˉinvalidˉoperation(
                $"Native {Service} service identity",
                () => X64ˉnativeˉargumentˉservices.Verify(Service, Corruptedˉservice));
        }
        Equal(
            X64ˉnativeˉargumentˉservices.ARGUMENT_COUNT_CANONICAL_SIZE,
            X64ˉnativeˉargumentˉservices.Build(
                Nativeˉservice.Processˉargumentˉcount).Length);
        Equal(
            X64ˉnativeˉargumentˉservices.ARGUMENT_CANONICAL_SIZE,
            X64ˉnativeˉargumentˉservices.Build(Nativeˉservice.Processˉargument).Length);

        foreach (var Platform in new[]
        {
            Nativeˉfileˉinputˉplatform.Windows,
            Nativeˉfileˉinputˉplatform.Linux,
        })
        {
            var Firstˉservice = X64ˉnativeˉfileˉinputˉservice.Build(Platform);
            var Secondˉservice = X64ˉnativeˉfileˉinputˉservice.Build(Platform);
            Sequenceˉequal(Firstˉservice, Secondˉservice);
            Equal(X64ˉnativeˉfileˉinputˉservice.Canonicalˉsize(Platform), Firstˉservice.Length);
            Equal(
                X64ˉnativeˉfileˉinputˉservice.Canonicalˉsha256(Platform),
                Convert.ToHexString(SHA256.HashData(Firstˉservice.AsSpan())).ToLowerInvariant());
            X64ˉnativeˉfileˉinputˉservice.Verify(Platform, Firstˉservice.AsSpan());
            var Corruptedˉservice = Firstˉservice.ToArray();
            Corruptedˉservice[0] ^= 0x01;
            Throwsˉinvalidˉoperation(
                $"Native {Platform} file-input service identity",
                () => X64ˉnativeˉfileˉinputˉservice.Verify(Platform, Corruptedˉservice));
        }

        using var Nativeˉoutput = new Nativeˉoutputˉcapture();
        var Nativeˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-file-input-{Guid.NewGuid():N}.wvb");
        var Nativeˉreader = new Testˉfileˉreader((_, _) =>
            throw new InvalidOperationException("Native execution called the Stage 0 file reader."));
        try
        {
            File.WriteAllBytes(Nativeˉpath, Input.AsSpan());
            var Nativeˉresources = Makeˉresources(
                TextWriter.Null,
                Nativeˉreader,
                Nativeˉpath);
            Equal(
                0,
                X64ˉnativeˉexecutor.Executeˉi32(
                    First.Fragment,
                    hostˉservices: new(
                        Nativeˉoutput.Channel,
                        Authorized,
                        Nativeˉresources,
                        fileˉinput: Nativeˉfileˉinput.Hostˉfileˉsystem())));
            Equal($"{Nativeˉpath}\nwvb-header=pass\n", Nativeˉoutput.Readˉtext());
            Equal(0, Nativeˉreader.Readˉcount);

            var Objectˉbytes = Nativeˉobjectˉsink.Writeˉwvo(First.Fragment);
            var Linked = Linkˉsuccess(
                [Objectˉbytes.ToArray()],
                new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"));
            Sequenceˉequal(First.Fragment.Code, Linked.Imageˉbytes);
            using var Linkedˉoutput = new Nativeˉoutputˉcapture();
            Equal(
                0,
                X64ˉnativeˉexecutor.Executeˉi32(
                    First.Fragment with { Code = Linked.Imageˉbytes },
                    hostˉservices: new(
                        Linkedˉoutput.Channel,
                        Authorized,
                        Nativeˉresources,
                        fileˉinput: Nativeˉfileˉinput.Hostˉfileˉsystem())));
            Equal($"{Nativeˉpath}\nwvb-header=pass\n", Linkedˉoutput.Readˉtext());
            Equal(0, Nativeˉreader.Readˉcount);
        }
        finally
        {
            File.Delete(Nativeˉpath);
        }

        const string Argumentˉtableˉsource = """
            module Nativeˉargumentˉtable profile hosted;

            capability console.write_line;
            capability process.argument;
            capability process.argument_count;

            export fn Main() -> i32 {
                let Count: u32 = process.argument_count();
                if Count != 67u32 { return 1; }
                var Index: u32 = 0u32;
                while Index < Count {
                    console.write_line(process.argument(Index));
                    Index = Index + 1u32;
                }
                return 0;
            }
            """;
        var Argumentˉtableˉverified = Moduleˉcodec.Readˉandˉverify(
            Compileˉsuccess(Argumentˉtableˉsource));
        var Argumentˉtableˉfragment = X64ˉnativeˉbackend.Compile(
            Argumentˉtableˉverified).Fragment;
        var Maximumˉarguments = Enumerable.Range(0, Hostedˉresourceˉlimits.MAX_ARGUMENTS)
            .Select(Index => Index switch
            {
                0 => "",
                1 => "ascii",
                2 => "euro-€",
                3 => "supplementary-😀",
                _ => $"argument-{Index}",
            })
            .ToImmutableArray();
        var Expectedˉarguments = string.Concat(Maximumˉarguments.Select(Argument => $"{Argument}\n"));
        var Argumentˉtableˉreferenceˉoutput = new StringWriter();
        var Argumentˉtableˉreferenceˉresources = new Hostedˉresourceˉcontext(
            Maximumˉarguments,
            Argumentˉtableˉreferenceˉoutput,
            TextWriter.Null);
        var Argumentˉtableˉreference = new Referenceˉruntime(
            Argumentˉtableˉverified,
            new Referenceˉcapabilityˉhost(Argumentˉtableˉreferenceˉresources),
            new(Authorized)).Runˉmain();
        Equal(0, Argumentˉtableˉreference.Exitˉcode);
        Equal(Expectedˉarguments, Argumentˉtableˉreferenceˉoutput.ToString());
        using var Argumentˉtableˉnativeˉoutput = new Nativeˉoutputˉcapture();
        var Argumentˉtableˉnativeˉresources = new Hostedˉresourceˉcontext(
            Maximumˉarguments,
            TextWriter.Null,
            TextWriter.Null);
        Equal(
            0,
            X64ˉnativeˉexecutor.Executeˉi32(
                Argumentˉtableˉfragment,
                maximumˉinstructions: Argumentˉtableˉreference.Executedˉinstructions,
                hostˉservices: new(
                    Argumentˉtableˉnativeˉoutput.Channel,
                    Authorized,
                    Argumentˉtableˉnativeˉresources)));
        Equal(Expectedˉarguments, Argumentˉtableˉnativeˉoutput.Readˉtext());

        var Emptyˉargumentˉresources = new Hostedˉresourceˉcontext(
            [],
            TextWriter.Null,
            TextWriter.Null);
        Equal(
            1,
            X64ˉnativeˉexecutor.Executeˉi32(
                Argumentˉtableˉfragment,
                hostˉservices: new(
                    Argumentˉtableˉnativeˉoutput.Channel,
                    Authorized,
                    Emptyˉargumentˉresources)));

        foreach (var Pointerˉoffset in new[]
        {
            Nativeˉserviceˉtableˉcontract.CONSOLE_WRITE_LINE_POINTER_OFFSET,
            Nativeˉserviceˉtableˉcontract.PROCESS_ARGUMENT_COUNT_POINTER_OFFSET,
            Nativeˉserviceˉtableˉcontract.PROCESS_ARGUMENT_POINTER_OFFSET,
            Nativeˉserviceˉtableˉcontract.FILE_READ_BYTES_POINTER_OFFSET,
        })
        {
            var Corrupted = First.Fragment.Code.ToArray();
            var Serviceˉload = Corrupted.AsSpan().IndexOf(new byte[]
            {
                0x49, 0x8B, 0x47,
                Nativeˉexecutionˉcontextˉcontract.SERVICE_TABLE_POINTER_OFFSET,
                0x48, 0x8B, 0x40,
                checked((byte)Pointerˉoffset),
            });
            True(Serviceˉload >= 0, $"Native code omitted service-table offset {Pointerˉoffset}.");
            Corrupted[Serviceˉload + 7] ^= 0x01;
            Throwsˉnative(
                "WVN3030",
                () => _ = Nativeˉfragmentˉverifier.Verify(
                    First.Fragment with { Code = Corrupted.ToImmutableArray() }));
        }
        Throwsˉnative(
            "WVN3009",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                First.Fragment with
                {
                    Requiredˉservices =
                    [
                        Nativeˉservice.Processˉargument,
                        Nativeˉservice.Processˉargumentˉcount,
                    ],
                }));

        var Invalidˉargumentˉverified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(
            Source.Replace("process.argument(0u32)", "process.argument(1u32)", StringComparison.Ordinal)));
        var Invalidˉargumentˉfragment = X64ˉnativeˉbackend.Compile(Invalidˉargumentˉverified).Fragment;
        var Missingˉargumentˉresources = Makeˉresources(TextWriter.Null, Makeˉreader());
        Throwsˉnativeˉtrap(
            "WVR3020",
            () => _ = X64ˉnativeˉexecutor.Executeˉi32(
                Invalidˉargumentˉfragment,
                hostˉservices: new(
                    Argumentˉtableˉnativeˉoutput.Channel,
                    Authorized,
                    Missingˉargumentˉresources,
                    fileˉinput: Nativeˉfileˉinput.Hostˉfileˉsystem())));

        var Missingˉfileˉreader = new Testˉfileˉreader((_, _) =>
            throw new InvalidOperationException("Native execution called the Stage 0 file reader."));
        var Missingˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-missing-{Guid.NewGuid():N}.wvb");
        var Missingˉfileˉresources = Makeˉresources(
            TextWriter.Null,
            Missingˉfileˉreader,
            Missingˉpath);
        Throwsˉnativeˉtrap(
            "WVR3022",
            () => _ = X64ˉnativeˉexecutor.Executeˉi32(
                First.Fragment,
                hostˉservices: new(
                    Argumentˉtableˉnativeˉoutput.Channel,
                    Authorized,
                    Missingˉfileˉresources,
                    fileˉinput: Nativeˉfileˉinput.Hostˉfileˉsystem())));
        Equal(0, Missingˉfileˉreader.Readˉcount);

        var Invalidˉnameˉresources = Makeˉresources(
            TextWriter.Null,
            Missingˉfileˉreader,
            "");
        Throwsˉnativeˉtrap(
            "WVR3021",
            () => _ = X64ˉnativeˉexecutor.Executeˉi32(
                First.Fragment,
                hostˉservices: new(
                    Argumentˉtableˉnativeˉoutput.Channel,
                    Authorized,
                    Invalidˉnameˉresources,
                    fileˉinput: Nativeˉfileˉinput.Hostˉfileˉsystem())));

        var Oversizedˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-oversized-{Guid.NewGuid():N}.bin");
        try
        {
            File.WriteAllBytes(
                Oversizedˉpath,
                new byte[Bytecodeˉlimits.MAX_BYTE_DATA_BYTES + 1]);
            var Oversizedˉresources = Makeˉresources(
                TextWriter.Null,
                Missingˉfileˉreader,
                Oversizedˉpath);
            Throwsˉnativeˉtrap(
                "WVR3025",
                () => _ = X64ˉnativeˉexecutor.Executeˉi32(
                    First.Fragment,
                    hostˉservices: new(
                        Argumentˉtableˉnativeˉoutput.Channel,
                        Authorized,
                        Oversizedˉresources,
                        fileˉinput: Nativeˉfileˉinput.Hostˉfileˉsystem())));
        }
        finally
        {
            File.Delete(Oversizedˉpath);
        }

        var Snapshotˉdirectory = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-snapshots-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Snapshotˉdirectory);
        try
        {
            var Snapshotˉpaths = Enumerable
                .Range(0, Hostedˉresourceˉlimits.MAX_FILE_SNAPSHOTS + 1)
                .Select(Index => Path.Combine(Snapshotˉdirectory, $"input-{Index}.bin"))
                .ToImmutableArray();
            foreach (var Pathˉname in Snapshotˉpaths)
            {
                File.WriteAllBytes(Pathˉname, []);
            }
            var Snapshotˉcalls = string.Join(
                Environment.NewLine,
                Enumerable.Range(0, Snapshotˉpaths.Length).Select(Index =>
                    $"    file.read_bytes(process.argument({Index}u32));"));
            var Snapshotˉsource = $$"""
                module Nativeˉfileˉsnapshotˉlimit profile hosted;
                capability file.read_bytes;
                capability process.argument;
                export fn Main() -> i32 {
                {{Snapshotˉcalls}}
                    return 0;
                }
                """;
            var Snapshotˉfragment = X64ˉnativeˉbackend.Compile(
                Moduleˉcodec.Readˉandˉverify(
                    Compileˉsuccess(Snapshotˉsource))).Fragment;
            var Snapshotˉresources = new Hostedˉresourceˉcontext(
                Snapshotˉpaths,
                TextWriter.Null,
                TextWriter.Null,
                Missingˉfileˉreader);
            Throwsˉnativeˉtrap(
                "WVR3028",
                () => _ = X64ˉnativeˉexecutor.Executeˉi32(
                    Snapshotˉfragment,
                    hostˉservices: new(
                        null,
                        Authorized,
                        Snapshotˉresources,
                        fileˉinput: Nativeˉfileˉinput.Hostˉfileˉsystem())));
        }
        finally
        {
            Directory.Delete(Snapshotˉdirectory, recursive: true);
        }
        Equal(0, Missingˉfileˉreader.Readˉcount);
    }

    private static void Nativeˉfileˉoutputˉpublishes()
    {
        const string Source = """
            module Nativeˉfileˉoutput profile hosted;

            capability file.write_bytes;
            capability process.argument;

            data Payload: bytes = [0, 65, 226, 130, 172, 240, 159, 152, 128, 255];
            data Empty: bytes = [];

            export fn Main() -> i32 {
                file.write_bytes(process.argument(0u32), Payload);
                file.write_bytes(process.argument(1u32), Empty);
                return 0;
            }
            """;
        var First = X64ˉnativeˉbackend.Compile(
            Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(Source)));
        var Second = X64ˉnativeˉbackend.Compile(
            Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(Source)));
        Sequenceˉequal(First.Fragment.Code, Second.Fragment.Code);
        Sequenceˉequal(
            [Nativeˉservice.Processˉargument, Nativeˉservice.Fileˉwriteˉbytes],
            First.Fragment.Requiredˉservices);
        Equal(2, First.Module.Functions[0].Blocks
            .SelectMany(Block => Block.Operations)
            .OfType<Nativeˉfileˉwriteˉbytes>()
            .Count());
        _ = Nativeˉfragmentˉverifier.Verify(First.Fragment);
        var Corruptedˉcall = First.Fragment.Code.ToArray();
        var Fileˉwriteˉload = Corruptedˉcall.AsSpan().IndexOf(new byte[]
        {
            0x49, 0x8B, 0x47,
            Nativeˉexecutionˉcontextˉcontract.SERVICE_TABLE_POINTER_OFFSET,
            0x48, 0x8B, 0x40,
            Nativeˉserviceˉtableˉcontract.FILE_WRITE_BYTES_POINTER_OFFSET,
        });
        True(Fileˉwriteˉload >= 0, "Native code omitted the file.write_bytes service-table load.");
        Corruptedˉcall[Fileˉwriteˉload + 7] ^= 0x01;
        Throwsˉnative(
            "WVN3030",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                First.Fragment with { Code = Corruptedˉcall.ToImmutableArray() }));

        foreach (var Platform in Enum.GetValues<Nativeˉfileˉinputˉplatform>())
        {
            var Leaf = X64ˉnativeˉfileˉoutputˉservice.Build(Platform);
            Equal(X64ˉnativeˉfileˉoutputˉservice.Canonicalˉsize(Platform), Leaf.Length);
            Equal(
                X64ˉnativeˉfileˉoutputˉservice.Canonicalˉsha256(Platform),
                Convert.ToHexString(SHA256.HashData(Leaf.AsSpan())).ToLowerInvariant());
            X64ˉnativeˉfileˉoutputˉservice.Verify(Platform, Leaf.AsSpan());
            var Corrupted = Leaf.ToArray();
            Corrupted[^1] ^= 0x01;
            Throwsˉinvalidˉoperation(
                $"Native {Platform} file-output service identity",
                () => X64ˉnativeˉfileˉoutputˉservice.Verify(Platform, Corrupted));
        }

        var Authorized = ImmutableHashSet.Create(
            StringComparer.Ordinal,
            Capabilityˉcatalog.FILE_WRITE_BYTES,
            Capabilityˉcatalog.PROCESS_ARGUMENT);
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-file-output-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        var Payloadˉpath = Path.Combine(Directoryˉpath, "published-ǣ-😀.bin");
        var Emptyˉpath = Path.Combine(Directoryˉpath, "empty.bin");
        var Expected = new byte[] { 0, 65, 226, 130, 172, 240, 159, 152, 128, 255 };
        try
        {
            File.WriteAllBytes(Payloadˉpath, Enumerable.Repeat((byte)0xCC, 128).ToArray());
            var Resources = new Hostedˉresourceˉcontext(
                [Payloadˉpath, Emptyˉpath],
                TextWriter.Null,
                TextWriter.Null);
            var Host = new Nativeˉhostˉservices(
                null,
                Authorized,
                Resources,
                fileˉoutput: Nativeˉfileˉoutput.Hostˉfileˉsystem());

            using (var Corruptedˉtable = new Nativeˉfileˉoutputˉcontext(Host, required: true))
            {
                Marshal.WriteByte(
                    Corruptedˉtable.Address,
                    Nativeˉfileˉoutputˉtableˉcontract.RESERVED_OFFSET,
                    1);
                Throwsˉinvalidˉoperation(
                    "The native file-output table violated its independently verified static layout.",
                    Corruptedˉtable.Verifyˉcompleted);
            }

            Throwsˉnativeˉtrap(
                "WVR3010",
                () => _ = X64ˉnativeˉexecutor.Executeˉi32(
                    First.Fragment,
                    hostˉservices: new(
                        null,
                        [Capabilityˉcatalog.PROCESS_ARGUMENT],
                        Resources,
                        fileˉoutput: Nativeˉfileˉoutput.Hostˉfileˉsystem())));
            Throwsˉnativeˉtrap(
                "WVR3001",
                () => _ = X64ˉnativeˉexecutor.Executeˉi32(
                    First.Fragment,
                    hostˉservices: new(null, Authorized, Resources)));

            Equal(0, X64ˉnativeˉexecutor.Executeˉi32(First.Fragment, hostˉservices: Host));
            Sequenceˉequal(Expected, File.ReadAllBytes(Payloadˉpath));
            Equal(0L, new FileInfo(Emptyˉpath).Length);

            var Object = Nativeˉobjectˉsink.Writeˉwvo(First.Fragment);
            var Linked = Linkˉsuccess(
                [Object.ToArray()],
                new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"));
            File.WriteAllBytes(Payloadˉpath, Enumerable.Repeat((byte)0xDD, 64).ToArray());
            Equal(
                0,
                X64ˉnativeˉexecutor.Executeˉi32(
                    First.Fragment with { Code = Linked.Imageˉbytes },
                    hostˉservices: Host));
            Sequenceˉequal(Expected, File.ReadAllBytes(Payloadˉpath));

            const string Copyˉsource = """
                module Nativeˉfileˉcopy profile hosted;
                capability file.read_bytes;
                capability file.write_bytes;
                capability process.argument;
                export fn Main() -> i32 {
                    file.write_bytes(
                        process.argument(1u32),
                        file.read_bytes(process.argument(0u32))
                    );
                    return 0;
                }
                """;
            var Copyˉfragment = X64ˉnativeˉbackend.Compile(
                Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(Copyˉsource))).Fragment;
            var Maximumˉinputˉpath = Path.Combine(Directoryˉpath, "maximum-input.bin");
            var Maximumˉoutputˉpath = Path.Combine(Directoryˉpath, "maximum-output.bin");
            var Maximumˉbytes = new byte[Bytecodeˉlimits.MAX_BYTE_DATA_BYTES];
            for (var Index = 0; Index < Maximumˉbytes.Length; Index++)
            {
                Maximumˉbytes[Index] = unchecked((byte)(Index * 131));
            }
            File.WriteAllBytes(Maximumˉinputˉpath, Maximumˉbytes);
            var Copyˉresources = new Hostedˉresourceˉcontext(
                [Maximumˉinputˉpath, Maximumˉoutputˉpath],
                TextWriter.Null,
                TextWriter.Null);
            Equal(
                0,
                X64ˉnativeˉexecutor.Executeˉi32(
                    Copyˉfragment,
                    hostˉservices: new(
                        null,
                        [
                            Capabilityˉcatalog.FILE_READ_BYTES,
                            Capabilityˉcatalog.FILE_WRITE_BYTES,
                            Capabilityˉcatalog.PROCESS_ARGUMENT,
                        ],
                        Copyˉresources,
                        fileˉinput: Nativeˉfileˉinput.Hostˉfileˉsystem(),
                        fileˉoutput: Nativeˉfileˉoutput.Hostˉfileˉsystem())));
            Sequenceˉequal(Maximumˉbytes, File.ReadAllBytes(Maximumˉoutputˉpath));

            var Missingˉparent = Path.Combine(Directoryˉpath, "missing", "output.bin");
            var Missingˉresources = new Hostedˉresourceˉcontext(
                [Missingˉparent, Emptyˉpath],
                TextWriter.Null,
                TextWriter.Null);
            Throwsˉnativeˉtrap(
                "WVR3022",
                () => _ = X64ˉnativeˉexecutor.Executeˉi32(
                    First.Fragment,
                    hostˉservices: new(
                        null,
                        Authorized,
                        Missingˉresources,
                        fileˉoutput: Nativeˉfileˉoutput.Hostˉfileˉsystem())));

            var Deniedˉresources = new Hostedˉresourceˉcontext(
                [Directoryˉpath, Emptyˉpath],
                TextWriter.Null,
                TextWriter.Null);
            Throwsˉnativeˉtrap(
                "WVR3023",
                () => _ = X64ˉnativeˉexecutor.Executeˉi32(
                    First.Fragment,
                    hostˉservices: new(
                        null,
                        Authorized,
                        Deniedˉresources,
                        fileˉoutput: Nativeˉfileˉoutput.Hostˉfileˉsystem())));

            var Invalidˉresources = new Hostedˉresourceˉcontext(
                ["", Emptyˉpath],
                TextWriter.Null,
                TextWriter.Null);
            Throwsˉnativeˉtrap(
                "WVR3021",
                () => _ = X64ˉnativeˉexecutor.Executeˉi32(
                    First.Fragment,
                    hostˉservices: new(
                        null,
                        Authorized,
                        Invalidˉresources,
                        fileˉoutput: Nativeˉfileˉoutput.Hostˉfileˉsystem())));
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }

        var Compilerˉtoolˉbytes = Compileˉwithˉsourceˉwvbˉsuccess(
            SOURCE_WVB_TOOL_SOURCE,
            "Source-Wvb-Tool.wv");
        Equal(SOURCE_WVB_TOOL_SHA256, Moduleˉdigest.Calculateˉsha256(Compilerˉtoolˉbytes));
        try
        {
            _ = X64ˉnativeˉbackend.Compile(Moduleˉcodec.Readˉandˉverify(Compilerˉtoolˉbytes));
        }
        catch (Nativeˉbackendˉexception Exception)
        {
            Equal("WVN2003", Exception.Code);
            True(
                Exception.Message.Contains(
                    "Compilerˉcompileˉsourceˉwvb",
                    StringComparison.Ordinal),
                $"Compiler native preflight did not identify the next exact function: {Exception.Message}");
            True(
                Exception.Message.Contains(
                    "Bytesˉfromˉu16ˉlittle",
                    StringComparison.Ordinal),
                $"Compiler native preflight did not identify the next unsupported operation: {Exception.Message}");
            return;
        }
        throw new InvalidOperationException(
            "The compiler native preflight unexpectedly completed; update this evidence to the newly observed execution result.");
    }

    private static void Sourceˉmodulesˉcompose()
    {
        var First = Compileˉcompositionˉsuccess(
            new("middle.wv", COMPOSITION_MIDDLE_SOURCE),
            new("leaf.wv", COMPOSITION_LEAF_SOURCE));
        var Second = Compileˉcompositionˉsuccess(
            new("leaf.wv", COMPOSITION_LEAF_SOURCE),
            new("middle.wv", COMPOSITION_MIDDLE_SOURCE));
        Sequenceˉequal(First, Second);

        var Module = Moduleˉcodec.Readˉandˉverify(First);
        Equal("Compositionˉdemo", Module.Module.Name);
        Equal(Moduleˉprofile.Portable, Module.Module.Profile);
        Sequenceˉequal(
            ["Compositionˉanswer", "Compositionˉincrement", "Compositionˉmake", "Main"],
            Module.Module.Functions.Select(Function => Function.Name));
        Sequenceˉequal(
            ["Compositionˉvalue", "Compositionˉstatus"],
            Module.Module.Types.Select(Type => Type.Name));
        Sequenceˉequal(["Main"], Module.Module.Exports.Select(Export => Export.Name));
        Equal(
            42,
            new Referenceˉruntime(
                Module,
                new Referenceˉcapabilityˉhost(new StringWriter()),
                Runtimeˉoptions.Portableˉdefaults).Runˉmain().Exitˉcode);

        var Missing = Seedˉcompiler.Compileˉmodules(
            new("root.wv", COMPOSITION_ROOT_SOURCE),
            []);
        Equal("WVC0007", Missing.Diagnostics.Single().Code);
        Equal("root.wv", Missing.Diagnostics.Single().Span.Sourceˉname);

        const string Cycleˉroot = """
            module Cycleˉroot profile portable;
            import Cycleˉdependency;
            export fn Main() -> i32 { return Cycleˉvalue(); }
            """;
        const string Cycleˉdependency = """
            module Cycleˉdependency profile portable;
            import Cycleˉroot;
            export fn Cycleˉvalue() -> i32 { return 0; }
            """;
        Hasˉdiagnostic(
            Seedˉcompiler.Compileˉmodules(
                new("cycle-root.wv", Cycleˉroot),
                [new("cycle-dependency.wv", Cycleˉdependency)]),
            "WVC0008");

        const string Hostedˉdependency = """
            module Hostedˉdependency profile hosted;
            export fn Hostedˉvalue() -> i32 { return 0; }
            """;
        const string Hostedˉroot = """
            module Hostedˉroot profile hosted;
            import Hostedˉdependency;
            export fn Main() -> i32 { return Hostedˉvalue(); }
            """;
        Hasˉdiagnostic(
            Seedˉcompiler.Compileˉmodules(
                new("hosted-root.wv", Hostedˉroot),
                [new("hosted-dependency.wv", Hostedˉdependency)]),
            "WVC0010");

        const string Privateˉdependency = """
            module Privateˉdependency profile portable;
            fn Privateˉvalue() -> i32 { return 0; }
            """;
        const string Privateˉroot = """
            module Privateˉroot profile portable;
            import Privateˉdependency;
            export fn Main() -> i32 { return Privateˉvalue(); }
            """;
        Hasˉdiagnostic(
            Seedˉcompiler.Compileˉmodules(
                new("private-root.wv", Privateˉroot),
                [new("private-dependency.wv", Privateˉdependency)]),
            "WVC0012");

        const string Dataˉdependency = """
            module Dataˉdependency profile portable;
            data Value: [i32] = [0];
            export fn Dataˉvalue() -> i32 { return Value[0]; }
            """;
        const string Dataˉroot = """
            module Dataˉroot profile portable;
            import Dataˉdependency;
            export fn Main() -> i32 { return Dataˉvalue(); }
            """;
        Hasˉdiagnostic(
            Seedˉcompiler.Compileˉmodules(
                new("data-root.wv", Dataˉroot),
                [new("data-dependency.wv", Dataˉdependency)]),
            "WVC0011");

        const string Nominalˉsibling = """
            module Nominalˉsibling profile portable;
            record Siblingˉvalue { Value: i32; }
            export fn Siblingˉmake() -> Siblingˉvalue { return Siblingˉvalue(1); }
            """;
        const string Nominalˉleak = """
            module Nominalˉleak profile portable;
            export fn Nominalˉleakingˉvalue(Value: Siblingˉvalue) -> i32 { return Value.Value; }
            """;
        const string Nominalˉleakˉroot = """
            module Nominalˉleakˉroot profile portable;
            import Nominalˉsibling;
            import Nominalˉleak;
            export fn Main() -> i32 { return 0; }
            """;
        Hasˉdiagnostic(
            Seedˉcompiler.Compileˉmodules(
                new("nominal-leak-root.wv", Nominalˉleakˉroot),
                [
                    new("nominal-sibling.wv", Nominalˉsibling),
                    new("nominal-leak.wv", Nominalˉleak),
                ]),
            "WVC2085");

        const string Leakingˉdependency = """
            module Leakingˉdependency profile portable;
            export fn Leakingˉvalue() -> i32 { return Rootˉhelper(); }
            """;
        const string Leakingˉroot = """
            module Leakingˉroot profile portable;
            import Leakingˉdependency;
            fn Rootˉhelper() -> i32 { return 0; }
            export fn Main() -> i32 { return Leakingˉvalue(); }
            """;
        Hasˉdiagnostic(
            Seedˉcompiler.Compileˉmodules(
                new("leaking-root.wv", Leakingˉroot),
                [new("leaking-dependency.wv", Leakingˉdependency)]),
            "WVC2065");

        const string Duplicateˉimport = """
            module Duplicateˉimport profile portable;
            import Compositionˉleaf;
            import Compositionˉleaf;
            export fn Main() -> i32 { return Compositionˉincrement(0); }
            """;
        Hasˉdiagnostic(
            Seedˉcompiler.Compileˉmodules(
                new("duplicate-import.wv", Duplicateˉimport),
                [new("leaf.wv", COMPOSITION_LEAF_SOURCE)]),
            "WVC0006");

        const string Duplicateˉmoduleˉroot = """
            module Duplicateˉmoduleˉroot profile portable;
            import Compositionˉleaf;
            export fn Main() -> i32 { return Compositionˉincrement(0); }
            """;
        Hasˉdiagnostic(
            Seedˉcompiler.Compileˉmodules(
                new("duplicate-module-root.wv", Duplicateˉmoduleˉroot),
                [
                    new("leaf-first.wv", COMPOSITION_LEAF_SOURCE),
                    new("leaf-second.wv", COMPOSITION_LEAF_SOURCE),
                ]),
            "WVC0004");

        Hasˉdiagnostic(
            Seedˉcompiler.Compileˉmodules(
                new("root.wv", COMPOSITION_ROOT_SOURCE),
                [
                    new("middle.wv", COMPOSITION_MIDDLE_SOURCE),
                    new("leaf.wv", COMPOSITION_LEAF_SOURCE),
                    new("unused.wv", "module Unused profile portable; export fn Unusedˉvalue() -> i32 { return 0; }"),
                ]),
            "WVC0009");

        const string Lateˉimport = """
            module Lateˉimport profile portable;
            export fn Main() -> i32 { return 0; }
            import Compositionˉleaf;
            """;
        Hasˉdiagnostic(Seedˉcompiler.Compile(Lateˉimport, "late.wv"), "WVC1107");

        var Excessˉmodules = Enumerable.Range(0, Seedˉcompiler.MAX_SOURCE_MODULES)
            .Select(Index => new Sourceˉmoduleˉinput(
                $"module-{Index}.wv",
                $"module Module_{Index} profile portable; export fn Value_{Index}() -> i32 {{ return 0; }}"))
            .ToArray();
        Hasˉdiagnostic(
            Seedˉcompiler.Compileˉmodules(new("root.wv", COMPOSITION_ROOT_SOURCE), Excessˉmodules),
            "WVC0002");
    }

    private static void Projectsˉselectˉsourceˉsets()
    {
        const string Valid = """
            windvale-project 1
            root "Source/Main.wv"
            source "Source/Leaf.wv"
            source "Source/Middle.wv"
            emit wvb
            """;
        var Parsed = Projectˉparser.Parse(Valid);
        True(Parsed.Success, "The valid project manifest was rejected.");
        Equal("Source/Main.wv", Parsed.Manifest!.Root.Value);
        Sequenceˉequal(
            ["Source/Leaf.wv", "Source/Middle.wv"],
            Parsed.Manifest.Sources.Select(Source => Source.Value));
        Equal(Projectˉemissionˉkind.Wvb, Parsed.Manifest.Emission);

        Projectˉhasˉdiagnostic(string.Empty, "WVP1001");
        Projectˉhasˉdiagnostic("windvale-project 2\nroot \"Main.wv\"\nemit wvb\n", "WVP1001");
        Projectˉhasˉdiagnostic("\uFEFFwindvale-project 1\nroot \"Main.wv\"\nemit wvb\n", "WVP1001");
        Projectˉhasˉdiagnostic("windvale-project 1\n \nroot \"Main.wv\"\nemit wvb\n", "WVP1003");
        Projectˉhasˉdiagnostic("windvale-project 1\r", "WVP1003");
        Projectˉhasˉdiagnostic("windvale-project 1\n# comment\nroot \"Main.wv\"\nemit wvb\n", "WVP1003");
        Projectˉhasˉdiagnostic("windvale-project 1\nroot Main.wv\nemit wvb\n", "WVP1003");
        Projectˉhasˉdiagnostic(
            "windvale-project 1\nroot \"Main.wv\"\nroot \"Other.wv\"\nemit wvb\n",
            "WVP1004");
        Projectˉhasˉdiagnostic(
            "windvale-project 1\nroot \"Main.wv\"\nemit wvb\nemit wvb\n",
            "WVP1004");
        Projectˉhasˉdiagnostic("windvale-project 1\nemit wvb\n", "WVP1004");
        Projectˉhasˉdiagnostic("windvale-project 1\nroot \"Main.wv\"\n", "WVP1004");

        foreach (var Invalidˉpath in new[]
        {
            string.Empty,
            "/Main.wv",
            "../Main.wv",
            "Source/../Main.wv",
            "Source\\Main.wv",
            "C:/Main.wv",
            "Source//Main.wv",
            "Source/./Main.wv",
            "Source/Main.WV",
            "Source/Main File.wv",
            "Source/Maïn.wv",
            "Source/Main?.wv",
        })
        {
            Projectˉhasˉdiagnostic(
                $"windvale-project 1\nroot \"{Invalidˉpath}\"\nemit wvb\n",
                "WVP1006");
        }
        Projectˉhasˉdiagnostic(
            $"windvale-project 1\nroot \"{new string('a', Projectˉlimits.MAX_PATH_BYTES)}.wv\"\nemit wvb\n",
            "WVP1006");
        Projectˉhasˉdiagnostic(
            "windvale-project 1\nroot \"Main.wv\"\nemit wvb\n" +
            new string('\n', Projectˉlimits.MAX_MANIFEST_BYTES),
            "WVP1002");
        Projectˉhasˉdiagnostic(
            "windvale-project 1\nroot \"Main.wv\"\nemit wvb\n\uD800",
            "WVP1002");

        var Maximum = new StringBuilder("windvale-project 1\nroot \"Root.wv\"\n");
        for (var Index = 0; Index < Projectˉlimits.MAX_SOURCE_MODULES - 1; Index++)
        {
            Maximum.AppendLine($"source \"Source/Module-{Index}.wv\"");
        }
        Maximum.AppendLine("emit wvb");
        True(Projectˉparser.Parse(Maximum.ToString()).Success, "The exact project module bound was rejected.");
        Maximum.Insert(Maximum.ToString().LastIndexOf("emit wvb", StringComparison.Ordinal),
            "source \"Source/Excess.wv\"\n");
        Projectˉhasˉdiagnostic(Maximum.ToString(), "WVP1005");

        var Temporaryˉdirectory = Path.Combine(
            Path.GetTempPath(),
            $"windvale-project-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Temporaryˉdirectory);
        try
        {
            var Projectˉpath = Path.Combine(Temporaryˉdirectory, "Example.wvproj");
            File.WriteAllText(
                Projectˉpath,
                "windvale-project 1\nroot \"Source/Main.wv\"\nsource \"Source/Leaf.wv\"\nemit wvb\n",
                new UTF8Encoding(false, true));
            var Read = Projectˉreader.Read(Projectˉpath);
            True(Read.Success, "The valid project file was not resolved.");
            Equal(
                Path.GetFullPath(Path.Combine(Temporaryˉdirectory, "Source", "Main.wv")),
                Read.Plan!.Rootˉpath);
            Sequenceˉequal(
                [Path.GetFullPath(Path.Combine(Temporaryˉdirectory, "Source", "Leaf.wv"))],
                Read.Plan.Sourceˉpaths);

            File.WriteAllText(
                Projectˉpath,
                "windvale-project 1\nroot \"Source/Main.wv\"\nsource \"Source/Main.wv\"\nemit wvb\n",
                new UTF8Encoding(false, true));
            var Duplicate = Projectˉreader.Read(Projectˉpath);
            False(Duplicate.Success, "The project reader accepted a duplicate resolved source path.");
            Equal("WVP1007", Duplicate.Diagnostics.Single().Code);

            File.WriteAllBytes(Projectˉpath, [0xFF]);
            var Invalidˉutf8 = Projectˉreader.Read(Projectˉpath);
            False(Invalidˉutf8.Success, "The project reader accepted malformed UTF-8.");
            Equal("WVP1002", Invalidˉutf8.Diagnostics.Single().Code);
        }
        finally
        {
            Directory.Delete(Temporaryˉdirectory, recursive: true);
        }
    }

    private static void Windvaleˉprojectˉmanifestsˉagree()
    {
        var Coreˉresult = Seedˉcompiler.Compile(
            PROJECT_MANIFEST_CORE_SOURCE,
            "Tools/Windvale.Project/Project-Manifest-Core.wv");
        True(
            Coreˉresult.Success,
            "The Windvale project core did not compile: " +
                string.Join(" | ", Coreˉresult.Diagnostics));
        var Core = Moduleˉcodec.Readˉandˉverify(Coreˉresult.Moduleˉbytes.AsSpan());
        Equal(
            PROJECT_MANIFEST_CORE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Coreˉresult.Moduleˉbytes.AsSpan()));
        Equal("Windvaleˉproject", Core.Module.Name);
        Equal(Moduleˉprofile.Portable, Core.Module.Profile);
        Equal(0, Core.Module.Capabilities.Length);
        True(
            Core.Module.Exports.Any(Export => Export.Name == "Windvaleˉprojectˉscanˉmanifest"),
            "The Windvale project core did not export its manifest scanner.");
        True(
            Core.Module.Exports.Any(Export => Export.Name == "Windvaleˉprojectˉpathˉat"),
            "The Windvale project core did not export its path view.");

        var Toolˉresult = Seedˉcompiler.Compileˉmodules(
            new(
                "Tools/Windvale.Project/Project-Manifest-Tool.wv",
                PROJECT_MANIFEST_TOOL_SOURCE),
            [
                new(
                    "Tools/Windvale.Project/Project-Manifest-Core.wv",
                    PROJECT_MANIFEST_CORE_SOURCE),
            ]);
        True(
            Toolˉresult.Success,
            "The Windvale project tool did not compile: " +
                string.Join(" | ", Toolˉresult.Diagnostics));
        var Tool = Moduleˉcodec.Readˉandˉverify(Toolˉresult.Moduleˉbytes.AsSpan());
        Equal(
            PROJECT_MANIFEST_TOOL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Toolˉresult.Moduleˉbytes.AsSpan()));
        Equal("Windvaleˉprojectˉtool", Tool.Module.Name);
        Equal(Moduleˉprofile.Hosted, Tool.Module.Profile);

        Projectˉparserˉagreesˉwithˉtool(
            Tool,
            "windvale-project 1\n" +
            "root \"Source/Main.wv\"\n" +
            "source \"Source/Leaf.wv\"\n" +
            "source \"Source/Middle.wv\"\n" +
            "emit wvb\n");
        Projectˉparserˉagreesˉwithˉtool(
            Tool,
            "windvale-project 1\r\n" +
            "emit wvb\r\n" +
            "source \"Source/Leaf.wv\"\r\n" +
            "root \"Source/Main.wv\"\r\n");

        foreach (var Invalid in new[]
        {
            string.Empty,
            "windvale-project 2\nroot \"Main.wv\"\nemit wvb\n",
            "\uFEFFwindvale-project 1\nroot \"Main.wv\"\nemit wvb\n",
            "windvale-project 1\n \nroot \"Main.wv\"\nemit wvb\n",
            "windvale-project 1\r",
            "windvale-project 1\nroot Main.wv\nemit wvb\n",
            "windvale-project 1\nroot \"Main.wv\"\nroot \"Other.wv\"\nemit wvb\n",
            "windvale-project 1\nroot \"Main.wv\"\nemit wvb\nemit wvb\n",
            "windvale-project 1\nemit wvb\n",
            "windvale-project 1\nroot \"Main.wv\"\n",
            "windvale-project 1\nroot \"../Main.wv\"\nemit wvb\n",
            "windvale-project 1\nroot \"Source/Main.WV\"\nemit wvb\n",
            "windvale-project 1\nroot \"Source/Maïn.wv\"\nemit wvb\n",
            "windvale-project 1\nroot \"Main.wv\"\nemit wvb\n" +
                new string('\n', Projectˉlimits.MAX_MANIFEST_BYTES),
        })
        {
            Projectˉparserˉagreesˉwithˉtool(Tool, Invalid);
        }

        var Maximum = new StringBuilder("windvale-project 1\nroot \"Root.wv\"\n");
        for (var Index = 0; Index < Projectˉlimits.MAX_SOURCE_MODULES - 1; Index++)
        {
            Maximum.AppendLine($"source \"Source/Module-{Index}.wv\"");
        }
        Maximum.AppendLine("emit wvb");
        Projectˉparserˉagreesˉwithˉtool(Tool, Maximum.ToString());
        Maximum.Insert(
            Maximum.ToString().LastIndexOf("emit wvb", StringComparison.Ordinal),
            "source \"Source/Excess.wv\"\n");
        Projectˉparserˉagreesˉwithˉtool(Tool, Maximum.ToString());

        var Invalidˉutf8 = Runˉprojectˉmanifestˉtool(Tool, [0xFF]);
        Equal(1, Invalidˉutf8.Exitˉcode);
        Equal(string.Empty, Invalidˉutf8.Output);
        Equal("project status=WVP1002 line=1 column=1\n", Invalidˉutf8.Diagnostics);
        Equal(1, Invalidˉutf8.Readˉcount);
    }

    private static void Nativeˉprojectˉmanifestsˉagree()
    {
        var Toolˉresult = Seedˉcompiler.Compileˉmodules(
            new(
                "Tools/Windvale.Project/Project-Manifest-Tool.wv",
                PROJECT_MANIFEST_TOOL_SOURCE),
            [
                new(
                    "Tools/Windvale.Project/Project-Manifest-Core.wv",
                    PROJECT_MANIFEST_CORE_SOURCE),
            ]);
        True(
            Toolˉresult.Success,
            "The Windvale project tool did not compile for native execution: " +
                string.Join(" | ", Toolˉresult.Diagnostics));
        var Tool = Moduleˉcodec.Readˉandˉverify(Toolˉresult.Moduleˉbytes.AsSpan());
        var Authorized = Tool.Module.Capabilities
            .Select(Capability => Capability.Name)
            .ToImmutableHashSet(StringComparer.Ordinal);

        var First = X64ˉnativeˉbackend.Compile(Tool);
        var Second = X64ˉnativeˉbackend.Compile(Tool);
        Sequenceˉequal(First.Fragment.Code, Second.Fragment.Code);
        Sequenceˉequal(First.Fragment.Symbols, Second.Fragment.Symbols);
        Sequenceˉequal(First.Fragment.Patches, Second.Fragment.Patches);
        Sequenceˉequal(First.Fragment.Types, Second.Fragment.Types);
        Sequenceˉequal(
            [
                Nativeˉservice.Consoleˉwriteˉline,
                Nativeˉservice.Processˉargumentˉcount,
                Nativeˉservice.Processˉargument,
                Nativeˉservice.Fileˉreadˉbytes,
                Nativeˉservice.Textˉutf8ˉisˉvalid,
                Nativeˉservice.Diagnosticˉwriteˉline,
                Nativeˉservice.Textˉconcat,
                Nativeˉservice.Textˉquote,
                Nativeˉservice.U32ˉformat,
            ],
            First.Fragment.Requiredˉservices);
        var Operations = First.Module.Functions
            .SelectMany(Function => Function.Blocks)
            .SelectMany(Block => Block.Operations)
            .ToImmutableArray();
        True(Operations.Any(Operation => Operation is Nativeˉbytesˉconcat),
            "The native project tool omitted immutable byte concatenation.");
        True(Operations.Any(Operation => Operation is Nativeˉbytesˉfromˉu32ˉlittle),
            "The native project tool omitted little-endian directory construction.");
        True(Operations.Any(Operation => Operation is Nativeˉtextˉtoˉutf8),
            "The native project tool omitted borrowed text encoding.");
        True(
            Tool.Module.Types
                .OfType<Recordˉtypeˉdeclaration>()
                .Any(Record => Record.Fields.Any(Field => Field.Type.Kind == Valueˉtype.Bytes)),
            "The native project tool omitted its borrowed-byte record boundary.");
        _ = Nativeˉfragmentˉverifier.Verify(First.Fragment);
        Equal(
            PROJECT_MANIFEST_NATIVE_CODE_SHA256,
            Objectˉdigest.Calculateˉsha256(First.Fragment.Code.AsSpan()));

        var Corruptedˉrecordˉtag = First.Fragment.Code.ToArray();
        var Recordˉtag = Enumerable
            .Range(0, Corruptedˉrecordˉtag.Length - 13)
            .Where(Index =>
                Corruptedˉrecordˉtag[Index] == 0x41 &&
                Corruptedˉrecordˉtag[Index + 1] == 0xB8 &&
                Corruptedˉrecordˉtag[Index + 6] == 0x41 &&
                Corruptedˉrecordˉtag[Index + 7] == 0x8B &&
                Corruptedˉrecordˉtag[Index + 8] == 0x47 &&
                Corruptedˉrecordˉtag[Index + 9] ==
                    Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_USED_OFFSET)
            .DefaultIfEmpty(-1)
            .First();
        True(Recordˉtag >= 0, "The native project tool omitted typed record construction.");
        BinaryPrimitives.WriteInt32LittleEndian(
            Corruptedˉrecordˉtag.AsSpan(Recordˉtag + 2, sizeof(int)),
            int.MaxValue);
        Throwsˉnative(
            "WVN3030",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                First.Fragment with { Code = Corruptedˉrecordˉtag.ToImmutableArray() }));

        var Corruptedˉbyteˉlimit = First.Fragment.Code.ToArray();
        var Byteˉlimit = Corruptedˉbyteˉlimit.AsSpan().IndexOf(new byte[]
        {
            0x3D, 0x00, 0x00, 0x40, 0x00, 0x0F, 0x87,
        });
        True(Byteˉlimit >= 0, "The native project tool omitted the 4 MiB byte-value bound.");
        Corruptedˉbyteˉlimit[Byteˉlimit + 1] ^= 0x01;
        Throwsˉnative(
            "WVN3030",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                First.Fragment with { Code = Corruptedˉbyteˉlimit.ToImmutableArray() }));

        var Firstˉobject = Nativeˉobjectˉsink.Writeˉwvo(First.Fragment);
        var Secondˉobject = Nativeˉobjectˉsink.Writeˉwvo(Second.Fragment);
        Sequenceˉequal(Firstˉobject, Secondˉobject);
        Equal(
            PROJECT_MANIFEST_NATIVE_WVO_SHA256,
            Objectˉdigest.Calculateˉsha256(Firstˉobject.AsSpan()));
        var Firstˉlinked = Linkˉsuccess(
            [Firstˉobject.ToArray()],
            new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"));
        var Secondˉlinked = Linkˉsuccess(
            [Secondˉobject.ToArray()],
            new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"));
        Sequenceˉequal(Firstˉlinked.Imageˉbytes, Secondˉlinked.Imageˉbytes);
        Sequenceˉequal(Firstˉlinked.Mapˉbytes, Secondˉlinked.Mapˉbytes);
        Sequenceˉequal(First.Fragment.Code, Firstˉlinked.Imageˉbytes);

        void Runˉcase(string manifest)
        {
            var Manifestˉbytes = Encoding.UTF8.GetBytes(manifest).ToImmutableArray();
            var Reference = Runˉprojectˉmanifestˉtool(Tool, Manifestˉbytes);
            Equal(1, Reference.Readˉcount);
            var Nativeˉpath = Path.Combine(
                Path.GetTempPath(),
                $"windvale-native-project-{Guid.NewGuid():N}.wvproj");

            void Runˉnative(ImmutableArray<byte> code)
            {
                using var Output = new Nativeˉoutputˉcapture();
                using var Diagnostic = new Nativeˉoutputˉcapture();
                var Reader = new Testˉfileˉreader((_, _) =>
                    throw new InvalidOperationException(
                        "Native project execution called the Stage 0 file reader."));
                var Resources = new Hostedˉresourceˉcontext(
                    [Nativeˉpath],
                    TextWriter.Null,
                    TextWriter.Null,
                    Reader);
                Equal(
                    Reference.Exitˉcode,
                    X64ˉnativeˉexecutor.Executeˉi32(
                        First.Fragment with { Code = code },
                        maximumˉinstructions: Reference.Executedˉinstructions,
                        hostˉservices: new(
                            Output.Channel,
                            Authorized,
                            Resources,
                            Diagnostic.Channel,
                            Nativeˉfileˉinput.Hostˉfileˉsystem())));
                Equal(Reference.Output, Output.Readˉtext());
                Equal(Reference.Diagnostics, Diagnostic.Readˉtext());
                Equal(0, Reader.Readˉcount);
            }

            try
            {
                File.WriteAllBytes(Nativeˉpath, Manifestˉbytes.AsSpan());
                Runˉnative(First.Fragment.Code);
                Runˉnative(Firstˉlinked.Imageˉbytes);
            }
            finally
            {
                File.Delete(Nativeˉpath);
            }
        }

        Runˉcase(
            "windvale-project 1\n" +
            "root \"Source/Main.wv\"\n" +
            "source \"Source/Leaf.wv\"\n" +
            "emit wvb\n");
        Runˉcase(
            "windvale-project 1\n" +
            " \n" +
            "root \"Source/Main.wv\"\n" +
            "emit wvb\n");

        var Limitˉverified = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess("""
            module Nativeˉprojectˉbyteˉlimit profile hosted;
            capability file.read_bytes;
            capability process.argument;
            export fn Main() -> i32 {
                let Input: bytes = file.read_bytes(process.argument(0u32));
                let Combined: bytes = Bytesˉconcat(Input, Input);
                return 0;
            }
            """));
        var Limitˉinput = new byte[Bytecodeˉlimits.MAX_BYTE_DATA_BYTES / 2 + 1]
            .ToImmutableArray();
        var Limitˉreader = new Testˉfileˉreader((Name, Maximum) =>
        {
            Equal("input.bin", Name);
            True(Limitˉinput.Length <= Maximum, "The byte-limit fixture exceeded the hosted bound.");
            return Limitˉinput;
        });
        var Limitˉauthorized = ImmutableHashSet.Create(
            StringComparer.Ordinal,
            Capabilityˉcatalog.FILE_READ_BYTES,
            Capabilityˉcatalog.PROCESS_ARGUMENT);
        Throwsˉruntime(
            "WVR3015",
            () => _ = new Referenceˉruntime(
                Limitˉverified,
                new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                    ["input.bin"],
                    TextWriter.Null,
                    TextWriter.Null,
                    Limitˉreader)),
                new(Limitˉauthorized)).Runˉmain());
        Equal(1, Limitˉreader.Readˉcount);

        var Limitˉnative = X64ˉnativeˉbackend.Compile(Limitˉverified);
        var Limitˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-project-limit-{Guid.NewGuid():N}.bin");
        try
        {
            File.WriteAllBytes(Limitˉpath, Limitˉinput.AsSpan());
            var Reader = new Testˉfileˉreader((_, _) =>
                throw new InvalidOperationException(
                    "Native byte-limit execution called the Stage 0 file reader."));
            Throwsˉnativeˉtrap(
                "WVR3015",
                () => _ = X64ˉnativeˉexecutor.Executeˉi32(
                    Limitˉnative.Fragment,
                    hostˉservices: new(
                        null,
                        Limitˉauthorized,
                        new Hostedˉresourceˉcontext(
                            [Limitˉpath],
                            TextWriter.Null,
                            TextWriter.Null,
                            Reader),
                        fileˉinput: Nativeˉfileˉinput.Hostˉfileˉsystem())));
            Equal(0, Reader.Readˉcount);
        }
        finally
        {
            File.Delete(Limitˉpath);
        }
    }

    private static void Foundationˉmachineˉcontractsˉrun()
    {
        var Libraryˉbytes = Compileˉsuccess(MACHINE_CONTRACTS_SOURCE);
        var Library = Moduleˉcodec.Readˉandˉverify(Libraryˉbytes);
        Equal("Foundationˉmachineˉcontracts", Library.Module.Name);
        Equal(Moduleˉprofile.Portable, Library.Module.Profile);
        Sequenceˉequal(
            ["Foundationˉalignmentˉisˉvalid", "Foundationˉmachineˉnameˉisˉvalid"],
            Library.Module.Exports.Select(Export => Export.Name));

        var Demoˉbytes = Compileˉwithˉmachineˉcontractsˉsuccess(
            MACHINE_CONTRACTS_DEMO_SOURCE,
            "Machine-Contracts-Demo.wv");
        var Demo = Moduleˉcodec.Readˉandˉverify(Demoˉbytes);
        Sequenceˉequal(["Main"], Demo.Module.Exports.Select(Export => Export.Name));
        Equal(
            0,
            new Referenceˉruntime(
                Demo,
                new Referenceˉcapabilityˉhost(new StringWriter()),
                Runtimeˉoptions.Portableˉdefaults).Runˉmain().Exitˉcode);
    }

    private static void Foundationˉbyteˉorderingˉruns()
    {
        var Libraryˉbytes = Compileˉsuccess(BYTE_ORDERING_SOURCE);
        var Library = Moduleˉcodec.Readˉandˉverify(Libraryˉbytes);
        Equal("Foundationˉbyteˉordering", Library.Module.Name);
        Equal(Moduleˉprofile.Portable, Library.Module.Profile);
        Sequenceˉequal(
            ["Foundationˉbyteˉspansˉcompare"],
            Library.Module.Exports.Select(Export => Export.Name));

        var Demoˉbytes = Compileˉwithˉbyteˉorderingˉsuccess(
            BYTE_ORDERING_DEMO_SOURCE,
            "Byte-Ordering-Demo.wv");
        var Demo = Moduleˉcodec.Readˉandˉverify(Demoˉbytes);
        Sequenceˉequal(["Main"], Demo.Module.Exports.Select(Export => Export.Name));
        Equal(
            0,
            new Referenceˉruntime(
                Demo,
                new Referenceˉcapabilityˉhost(new StringWriter()),
                Runtimeˉoptions.Portableˉdefaults).Runˉmain().Exitˉcode);
    }

    private static void Foundationˉdecimalˉparsingˉruns()
    {
        var Libraryˉbytes = Compileˉsuccess(DECIMAL_PARSING_SOURCE);
        var Library = Moduleˉcodec.Readˉandˉverify(Libraryˉbytes);
        Equal("Foundationˉdecimalˉparsing", Library.Module.Name);
        Equal(Moduleˉprofile.Portable, Library.Module.Profile);
        Sequenceˉequal(
            ["Foundationˉu32ˉparse"],
            Library.Module.Types.Select(Type => Type.Name));
        Sequenceˉequal(
            ["Foundationˉu32ˉdecimalˉparse"],
            Library.Module.Exports.Select(Export => Export.Name));

        var Demoˉbytes = Compileˉwithˉdecimalˉparsingˉsuccess(
            DECIMAL_PARSING_DEMO_SOURCE,
            "Decimal-Parsing-Demo.wv");
        var Demo = Moduleˉcodec.Readˉandˉverify(Demoˉbytes);
        Sequenceˉequal(["Foundationˉu32ˉparse"], Demo.Module.Types.Select(Type => Type.Name));
        Sequenceˉequal(["Main"], Demo.Module.Exports.Select(Export => Export.Name));
        Equal(
            0,
            new Referenceˉruntime(
                Demo,
                new Referenceˉcapabilityˉhost(new StringWriter()),
                Runtimeˉoptions.Portableˉdefaults).Runˉmain().Exitˉcode);
    }

    private static void Foundationˉbyteˉconstructionˉruns()
    {
        var Libraryˉbytes = Compileˉsuccess(BYTE_CONSTRUCTION_SOURCE);
        var Library = Moduleˉcodec.Readˉandˉverify(Libraryˉbytes);
        Equal("Foundationˉbyteˉconstruction", Library.Module.Name);
        Equal(Moduleˉprofile.Portable, Library.Module.Profile);
        Sequenceˉequal(
            ["Foundationˉbytesˉresult"],
            Library.Module.Types.Select(Type => Type.Name));
        Sequenceˉequal(
            ["Foundationˉbytesˉrepeat", "Foundationˉbytesˉreplace"],
            Library.Module.Exports.Select(Export => Export.Name));

        var Demoˉbytes = Compileˉwithˉbyteˉconstructionˉsuccess(
            BYTE_CONSTRUCTION_DEMO_SOURCE,
            "Byte-Construction-Demo.wv");
        var Demo = Moduleˉcodec.Readˉandˉverify(Demoˉbytes);
        Sequenceˉequal(["Foundationˉbytesˉresult"], Demo.Module.Types.Select(Type => Type.Name));
        Sequenceˉequal(["Main"], Demo.Module.Exports.Select(Export => Export.Name));
        Equal(
            0,
            new Referenceˉruntime(
                Demo,
                new Referenceˉcapabilityˉhost(new StringWriter()),
                Runtimeˉoptions.Portableˉdefaults).Runˉmain().Exitˉcode);
    }

    private static void Compilerˉsourceˉlexerˉruns()
    {
        var Libraryˉbytes = Compileˉwithˉdecimalˉparsingˉsuccess(
            SOURCE_LEXER_SOURCE,
            "Source-Lexer-Core.wv");
        var Library = Moduleˉcodec.Readˉandˉverify(Libraryˉbytes);
        Equal("Compilerˉsourceˉlexer", Library.Module.Name);
        Equal(Moduleˉprofile.Portable, Library.Module.Profile);
        Sequenceˉequal(
            [
                "Compilerˉsourceˉscan",
                "Compilerˉsourceˉtoken",
                "Foundationˉu32ˉparse",
                "Compilerˉlexˉstatus",
                "Compilerˉnumericˉkind",
                "Compilerˉtokenˉkind",
            ],
            Library.Module.Types.Select(Type => Type.Name));
        Sequenceˉequal(
            [
                "Compilerˉlexˉnext",
                "Compilerˉlexˉnextˉafterˉscan",
                "Compilerˉlexˉnextˉvalidated",
                "Compilerˉlexˉsource",
                "Compilerˉlexˉsourceˉbounded",
                "Compilerˉlexˉtokenˉat",
                "Compilerˉsourceˉcolumnˉwidth",
                "Compilerˉsourceˉhexˉdigit",
                "Compilerˉsourceˉidentifierˉpartˉlength",
                "Compilerˉsourceˉidentifierˉpartˉlengthˉafterˉscan",
                "Compilerˉsourceˉidentifierˉstart",
                "Compilerˉsourceˉkeywordˉkind",
                "Compilerˉsourceˉspanˉequalsˉtext",
                "Compilerˉsourceˉtokenˉmake",
                "Compilerˉsourceˉutf8ˉlength",
                "Compilerˉsourceˉwhitespaceˉlength",
                "Compilerˉsourceˉwhitespaceˉlengthˉafterˉscan",
            ],
            Library.Module.Exports.Select(Export => Export.Name));

        var Demoˉbytes = Compileˉwithˉsourceˉlexerˉsuccess(
            SOURCE_LEXER_DEMO_SOURCE,
            "Source-Lexer-Demo.wv");
        var Demo = Moduleˉcodec.Readˉandˉverify(Demoˉbytes);
        Sequenceˉequal(["Main"], Demo.Module.Exports.Select(Export => Export.Name));
        Equal(
            0,
            new Referenceˉruntime(
                Demo,
                new Referenceˉcapabilityˉhost(new StringWriter()),
                new(Runtimeˉoptions.Portableˉdefaults.Authorizedˉcapabilities,
                    Maximumˉinstructions: 10_000_000)).Runˉmain().Exitˉcode);
    }

    private static void Compilerˉsourceˉdeclarationˉparserˉruns()
    {
        var Libraryˉbytes = Compileˉwithˉsourceˉlexerˉsuccess(
            SOURCE_DECLARATION_PARSER_SOURCE,
            "Source-Declaration-Parser.wv");
        var Library = Moduleˉcodec.Readˉandˉverify(Libraryˉbytes);
        Equal("Compilerˉsourceˉdeclarationˉparser", Library.Module.Name);
        Equal(Moduleˉprofile.Portable, Library.Module.Profile);
        Sequenceˉequal(
            [
                "Compilerˉparseˉstep",
                "Compilerˉsourceˉdeclaration",
                "Compilerˉsourceˉheader",
                "Compilerˉsourceˉmoduleˉsummary",
                "Compilerˉsourceˉscan",
                "Compilerˉsourceˉtoken",
                "Foundationˉu32ˉparse",
                "Compilerˉlexˉstatus",
                "Compilerˉnumericˉkind",
                "Compilerˉparseˉstatus",
                "Compilerˉsourceˉdeclarationˉkind",
                "Compilerˉsourceˉprofile",
                "Compilerˉsourceˉtypeˉkind",
                "Compilerˉtokenˉkind",
            ],
            Library.Module.Types.Select(Type => Type.Name));
        Sequenceˉequal(
            [
                "Compilerˉparseˉbytesˉarray",
                "Compilerˉparseˉcapabilityˉdeclaration",
                "Compilerˉparseˉconsume",
                "Compilerˉparseˉdataˉdeclaration",
                "Compilerˉparseˉdeclarationˉat",
                "Compilerˉparseˉenumˉdeclaration",
                "Compilerˉparseˉfunctionˉdeclaration",
                "Compilerˉparseˉheader",
                "Compilerˉparseˉheaderˉvalidated",
                "Compilerˉparseˉi32ˉarray",
                "Compilerˉparseˉimportˉdeclaration",
                "Compilerˉparseˉnextˉdeclaration",
                "Compilerˉparseˉnextˉdeclarationˉvalidated",
                "Compilerˉparseˉqualifiedˉname",
                "Compilerˉparseˉrecordˉdeclaration",
                "Compilerˉparseˉskipˉblock",
                "Compilerˉparseˉskipˉblockˉafterˉscan",
                "Compilerˉparseˉsource",
                "Compilerˉparseˉstepˉfailure",
                "Compilerˉparseˉstepˉfromˉtoken",
                "Compilerˉparseˉstepˉvalid",
                "Compilerˉparseˉtype",
                "Compilerˉsourceˉdeclarationˉfailure",
                "Compilerˉsourceˉdeclarationˉmake",
                "Compilerˉsourceˉsummaryˉfromˉfailure",
            ],
            Library.Module.Exports.Select(Export => Export.Name));

        var Demoˉbytes = Compileˉwithˉsourceˉdeclarationˉparserˉsuccess(
            SOURCE_DECLARATION_PARSER_DEMO_SOURCE,
            "Source-Declaration-Parser-Demo.wv");
        var Demo = Moduleˉcodec.Readˉandˉverify(Demoˉbytes);
        Equal(
            0,
            new Referenceˉruntime(
                Demo,
                new Referenceˉcapabilityˉhost(new StringWriter()),
                new(Runtimeˉoptions.Portableˉdefaults.Authorizedˉcapabilities,
                    Maximumˉinstructions: 20_000_000)).Runˉmain().Exitˉcode);

        var Toolˉbytes = Compileˉwithˉsourceˉdeclarationˉparserˉsuccess(
            SOURCE_DECLARATION_PARSER_TOOL_SOURCE,
            "Source-Declaration-Parser-Tool.wv");
        var Tool = Moduleˉcodec.Readˉandˉverify(Toolˉbytes);
        var Lexerˉresult = Runˉsourceˉdeclarationˉparser(
            Tool,
            "Source-Lexer-Core.wv",
            SOURCE_LEXER_SOURCE,
            30_000_000);
        Equal(0, Lexerˉresult.Exitˉcode);
        Equal(string.Empty, Lexerˉresult.Diagnostics);
        Equal(
            "source declarations status=Valid imports=1 capabilities=0 data=0 records=2 enums=3 functions=17 tokens=5384 offset=45588\n",
            Lexerˉresult.Output);

        var Selfˉresult = Runˉsourceˉdeclarationˉparser(
            Tool,
            "Source-Declaration-Parser.wv",
            SOURCE_DECLARATION_PARSER_SOURCE,
            45_000_000);
        Equal(0, Selfˉresult.Exitˉcode);
        Equal(string.Empty, Selfˉresult.Diagnostics);
        Equal(
            "source declarations status=Valid imports=1 capabilities=0 data=0 records=4 enums=4 functions=25 tokens=9561 offset=70591\n",
            Selfˉresult.Output);
    }

    private static void Compilerˉsourceˉbodyˉparserˉruns()
    {
        var Libraryˉbytes = Compileˉwithˉsourceˉdeclarationˉparserˉsuccess(
            SOURCE_BODY_PARSER_SOURCE,
            "Source-Body-Parser.wv");
        Equal(SOURCE_BODY_PARSER_SHA256, Moduleˉdigest.Calculateˉsha256(Libraryˉbytes));
        var Library = Moduleˉcodec.Readˉandˉverify(Libraryˉbytes);
        Equal("Compilerˉsourceˉbodyˉparser", Library.Module.Name);
        Equal(Moduleˉprofile.Portable, Library.Module.Profile);
        Equal(23, Library.Module.Types.Length);
        Equal(39, Library.Module.Exports.Length);
        foreach (var Typeˉname in new[]
                 {
                     "Compilerˉbodyˉparseˉstatus",
                     "Compilerˉbodyˉparseˉstep",
                     "Compilerˉsourceˉbodyˉsummary",
                     "Compilerˉsourceˉexpression",
                     "Compilerˉsourceˉexpressionˉkind",
                     "Compilerˉsourceˉposition",
                     "Compilerˉsourceˉstatement",
                     "Compilerˉsourceˉstatementˉkind",
                 })
        {
            True(Library.Module.Types.Any(Type => Type.Name == Typeˉname),
                $"Body-parser type '{Typeˉname}' was not emitted.");
        }
        foreach (var Exportˉname in new[]
                 {
                     "Compilerˉbodyˉpositionˉbetween",
                     "Compilerˉparseˉblockˉvalidated",
                     "Compilerˉparseˉbodyˉspan",
                     "Compilerˉparseˉbodyˉspanˉvalidated",
                     "Compilerˉparseˉexpressionˉspan",
                     "Compilerˉparseˉexpressionˉvalidated",
                     "Compilerˉparseˉnextˉstatementˉvalidated",
                     "Compilerˉparseˉsourceˉbodies",
                     "Compilerˉparseˉsourceˉbodiesˉfromˉdeclarations",
                 })
        {
            True(Library.Module.Exports.Any(Export => Export.Name == Exportˉname),
                $"Body-parser export '{Exportˉname}' was not emitted.");
        }

        var Demoˉbytes = Compileˉwithˉsourceˉbodyˉparserˉsuccess(
            SOURCE_BODY_PARSER_DEMO_SOURCE,
            "Source-Body-Parser-Demo.wv");
        Equal(SOURCE_BODY_PARSER_DEMO_SHA256, Moduleˉdigest.Calculateˉsha256(Demoˉbytes));
        Equal(
            0,
            new Referenceˉruntime(
                Moduleˉcodec.Readˉandˉverify(Demoˉbytes),
                new Referenceˉcapabilityˉhost(new StringWriter()),
                new(Runtimeˉoptions.Portableˉdefaults.Authorizedˉcapabilities,
                    Maximumˉinstructions: 30_000_000)).Runˉmain().Exitˉcode);

        var Excessˉarguments = $"Call({string.Join(",", Enumerable.Repeat("0", 65))})";
        var Deepˉexpression = new string('(', 65) + "0" + new string(')', 65);
        var Deepˉblocks = new string('{', 66) + "return;" + new string('}', 66);
        var Excessˉstatements = "{" + string.Concat(Enumerable.Repeat("return;", 4_097)) + "}";
        var Boundaryˉsource = $$"""
            module Compilerˉbodyˉparserˉboundaries profile portable;
            import Compilerˉsourceˉbodyˉparser;
            data Excessˉarguments: text = "{{Excessˉarguments}}";
            data Deepˉexpression: text = "{{Deepˉexpression}}";
            data Deepˉblocks: text = "{{Deepˉblocks}}";
            data Excessˉstatements: text = "{{Excessˉstatements}}";
            export fn Main() -> i32 {
                let Arguments: bytes = Textˉtoˉutf8(Excessˉarguments);
                let Argumentˉresult: Compilerˉsourceˉexpression = Compilerˉparseˉexpressionˉspan(Arguments, 0u32, 1u32, 1u32, Bytesˉlength(Arguments));
                if Argumentˉresult.Status != Compilerˉbodyˉparseˉstatus.Itemˉlimit { return 1; }
                let Expression: bytes = Textˉtoˉutf8(Deepˉexpression);
                let Expressionˉresult: Compilerˉsourceˉexpression = Compilerˉparseˉexpressionˉspan(Expression, 0u32, 1u32, 1u32, Bytesˉlength(Expression));
                if Expressionˉresult.Status != Compilerˉbodyˉparseˉstatus.Nestingˉlimit { return 2; }
                let Blocks: bytes = Textˉtoˉutf8(Deepˉblocks);
                let Blockˉresult: Compilerˉsourceˉbodyˉsummary = Compilerˉparseˉbodyˉspan(Blocks, 0u32, 1u32, 1u32, Bytesˉlength(Blocks));
                if Blockˉresult.Status != Compilerˉbodyˉparseˉstatus.Nestingˉlimit { return 3; }
                let Statements: bytes = Textˉtoˉutf8(Excessˉstatements);
                let Statementˉresult: Compilerˉsourceˉbodyˉsummary = Compilerˉparseˉbodyˉspan(Statements, 0u32, 1u32, 1u32, Bytesˉlength(Statements));
                if Statementˉresult.Status != Compilerˉbodyˉparseˉstatus.Itemˉlimit { return 4; }
                return 0;
            }
            """;
        var Boundaryˉbytes = Compileˉwithˉsourceˉbodyˉparserˉsuccess(
            Boundaryˉsource,
            "Source-Body-Parser-Boundaries.wv");
        Equal(
            0,
            new Referenceˉruntime(
                Moduleˉcodec.Readˉandˉverify(Boundaryˉbytes),
                new Referenceˉcapabilityˉhost(new StringWriter()),
                new(Runtimeˉoptions.Portableˉdefaults.Authorizedˉcapabilities,
                    Maximumˉinstructions: 160_000_000)).Runˉmain().Exitˉcode);

        var Toolˉbytes = Compileˉwithˉsourceˉbodyˉparserˉsuccess(
            SOURCE_BODY_PARSER_TOOL_SOURCE,
            "Source-Body-Parser-Tool.wv");
        Equal(SOURCE_BODY_PARSER_TOOL_SHA256, Moduleˉdigest.Calculateˉsha256(Toolˉbytes));
        var Tool = Moduleˉcodec.Readˉandˉverify(Toolˉbytes);
        var Lexerˉresult = Runˉsourceˉdeclarationˉparser(
            Tool, "Source-Lexer-Core.wv", SOURCE_LEXER_SOURCE, 100_000_000);
        Equal(0, Lexerˉresult.Exitˉcode);
        Equal(string.Empty, Lexerˉresult.Diagnostics);
        Equal(1, Lexerˉresult.Readˉcount);
        Equal(
            "source bodies status=Valid functions=17 top-level=111 statements=602 expression-nodes=1670 statement-depth=17 expression-depth=5 offset=45589\n",
            Lexerˉresult.Output);
        var Declarationˉresult = Runˉsourceˉdeclarationˉparser(
            Tool, "Source-Declaration-Parser.wv", SOURCE_DECLARATION_PARSER_SOURCE, 160_000_000);
        Equal(0, Declarationˉresult.Exitˉcode);
        Equal(string.Empty, Declarationˉresult.Diagnostics);
        Equal(1, Declarationˉresult.Readˉcount);
        Equal(
            "source bodies status=Valid functions=25 top-level=245 statements=615 expression-nodes=2366 statement-depth=12 expression-depth=4 offset=70592\n",
            Declarationˉresult.Output);
        var Selfˉresult = Runˉsourceˉdeclarationˉparser(
            Tool, "Source-Body-Parser.wv", SOURCE_BODY_PARSER_SOURCE, 160_000_000);
        Equal(0, Selfˉresult.Exitˉcode);
        Equal(string.Empty, Selfˉresult.Diagnostics);
        Equal(1, Selfˉresult.Readˉcount);
        Equal(
            "source bodies status=Valid functions=39 top-level=237 statements=523 expression-nodes=2520 statement-depth=5 expression-depth=3 offset=69903\n",
            Selfˉresult.Output);
    }

    private static void Compilerˉsourceˉsetˉruns()
    {
        var Libraryˉbytes = Compileˉwithˉsourceˉsetˉsuccess(
            SOURCE_SET_SOURCE,
            "Source-Set-Core.wv",
            includeˉsourceˉset: false);
        Equal(SOURCE_SET_SHA256, Moduleˉdigest.Calculateˉsha256(Libraryˉbytes));
        var Library = Moduleˉcodec.Readˉandˉverify(Libraryˉbytes);
        Equal("Compilerˉsourceˉset", Library.Module.Name);
        Equal(Moduleˉprofile.Portable, Library.Module.Profile);
        Equal(27, Library.Module.Types.Length);
        Equal(10, Library.Module.Exports.Length);
        foreach (var Typeˉname in new[]
                 {
                     "Compilerˉsourceˉsetˉscan",
                     "Compilerˉsourceˉsetˉstatus",
                     "Compilerˉsourceˉsetˉsummary",
                     "Compilerˉsourceˉsetˉview",
                 })
        {
            True(Library.Module.Types.Any(Type => Type.Name == Typeˉname),
                $"Source-set type '{Typeˉname}' was not emitted.");
        }
        foreach (var Exportˉname in new[]
                 {
                     "Compilerˉscanˉsourceˉset",
                     "Compilerˉsourceˉsetˉmodule",
                     "Compilerˉsourceˉspansˉcompare",
                     "Compilerˉsourceˉspansˉequal",
                     "Compilerˉvalidateˉsourceˉset",
                 })
        {
            True(Library.Module.Exports.Any(Export => Export.Name == Exportˉname),
                $"Source-set export '{Exportˉname}' was not emitted.");
        }

        var Demoˉbytes = Compileˉwithˉsourceˉsetˉsuccess(
            SOURCE_SET_DEMO_SOURCE,
            "Source-Set-Demo.wv");
        Equal(SOURCE_SET_DEMO_SHA256, Moduleˉdigest.Calculateˉsha256(Demoˉbytes));
        Equal(
            0,
            new Referenceˉruntime(
                Moduleˉcodec.Readˉandˉverify(Demoˉbytes),
                new Referenceˉcapabilityˉhost(new StringWriter()),
                new(Runtimeˉoptions.Portableˉdefaults.Authorizedˉcapabilities,
                    Maximumˉinstructions: 200_000_000)).Runˉmain().Exitˉcode);

        var Toolˉbytes = Compileˉwithˉsourceˉsetˉsuccess(
            SOURCE_SET_TOOL_SOURCE,
            "Source-Set-Tool.wv");
        Equal(SOURCE_SET_TOOL_SHA256, Moduleˉdigest.Calculateˉsha256(Toolˉbytes));
        var Tool = Moduleˉcodec.Readˉandˉverify(Toolˉbytes);
        const string Root =
            "module Setˉroot profile portable; import Setˉdependency; export fn Main() -> i32 { return Setˉvalue(); }";
        const string Dependency =
            "module Setˉdependency profile portable; export fn Setˉvalue() -> i32 { return 1; }";
        var Smallˉset = Runˉsourceˉsetˉtool(
            Tool,
            [
                new("root.wv", Root),
                new("dependency.wv", Dependency),
            ],
            50_000_000);
        Equal(0, Smallˉset.Exitˉcode);
        Equal(string.Empty, Smallˉset.Diagnostics);
        Equal(2, Smallˉset.Readˉcount);
        Equal(
            $"source set status=Valid modules=2 source-bytes={System.Text.Encoding.UTF8.GetByteCount(Root) + System.Text.Encoding.UTF8.GetByteCount(Dependency)} imports=1 records=0 enums=0 functions=2\n",
            Smallˉset.Output);

        var Boundaryˉmodules = new List<Sourceˉmoduleˉinput>
        {
            new("root.wv", "module Boundaryˉroot profile portable;"),
        };
        for (var Index = 0; Index < 63; Index++)
        {
            Boundaryˉmodules.Add(new(
                $"dependency-{Index:D2}.wv",
                $"module Boundary_{Index:D2} profile portable;"));
        }
        var Boundary = Runˉsourceˉsetˉtool(Tool, Boundaryˉmodules, 300_000_000);
        Equal(0, Boundary.Exitˉcode);
        Equal(string.Empty, Boundary.Diagnostics);
        Equal(64, Boundary.Readˉcount);
        Contains(Boundary.Output, "source set status=Valid modules=64");
    }

    private static void Compilerˉsourceˉgraphˉruns()
    {
        var Libraryˉbytes = Compileˉwithˉsourceˉgraphˉsuccess(
            SOURCE_GRAPH_SOURCE,
            "Source-Graph-Core.wv",
            includeˉsourceˉgraph: false);
        Equal(SOURCE_GRAPH_SHA256, Moduleˉdigest.Calculateˉsha256(Libraryˉbytes));
        var Library = Moduleˉcodec.Readˉandˉverify(Libraryˉbytes);
        Equal("Compilerˉsourceˉgraph", Library.Module.Name);
        Equal(Moduleˉprofile.Portable, Library.Module.Profile);
        Equal(32, Library.Module.Types.Length);
        Equal(11, Library.Module.Exports.Length);
        foreach (var Typeˉname in new[]
                 {
                     "Compilerˉsourceˉgraphˉmatch",
                     "Compilerˉsourceˉgraphˉstatus",
                     "Compilerˉsourceˉgraphˉsummary",
                     "Compilerˉsourceˉgraphˉwalk",
                 })
        {
            True(Library.Module.Types.Any(Type => Type.Name == Typeˉname),
                $"Source-graph type '{Typeˉname}' was not emitted.");
        }
        True(
            Library.Module.Exports.Any(Export => Export.Name == "Compilerˉvalidateˉsourceˉgraph"),
            "The source-graph validation entry point was not emitted.");

        var Demoˉbytes = Compileˉwithˉsourceˉgraphˉsuccess(
            SOURCE_GRAPH_DEMO_SOURCE,
            "Source-Graph-Demo.wv");
        Equal(SOURCE_GRAPH_DEMO_SHA256, Moduleˉdigest.Calculateˉsha256(Demoˉbytes));
        Equal(
            0,
            new Referenceˉruntime(
                Moduleˉcodec.Readˉandˉverify(Demoˉbytes),
                new Referenceˉcapabilityˉhost(new StringWriter()),
                new(Runtimeˉoptions.Portableˉdefaults.Authorizedˉcapabilities,
                    Maximumˉinstructions: 300_000_000)).Runˉmain().Exitˉcode);

        var Toolˉbytes = Compileˉwithˉsourceˉgraphˉsuccess(
            SOURCE_GRAPH_TOOL_SOURCE,
            "Source-Graph-Tool.wv");
        Equal(SOURCE_GRAPH_TOOL_SHA256, Moduleˉdigest.Calculateˉsha256(Toolˉbytes));
        var Tool = Moduleˉcodec.Readˉandˉverify(Toolˉbytes);
        var Boundaryˉmodules = new List<Sourceˉmoduleˉinput>
        {
            new(
                "root.wv",
                "module Graphˉboundaryˉroot profile portable; import Graph_00;"),
        };
        for (var Index = 0; Index < 63; Index++)
        {
            var Import = Index == 62 ? string.Empty : $" import Graph_{Index + 1:D2};";
            Boundaryˉmodules.Add(new(
                $"dependency-{Index:D2}.wv",
                $"module Graph_{Index:D2} profile portable;{Import}"));
        }
        var Boundary = Runˉsourceˉsetˉtool(Tool, Boundaryˉmodules, 600_000_000);
        Equal(0, Boundary.Exitˉcode);
        Equal(string.Empty, Boundary.Diagnostics);
        Equal(64, Boundary.Readˉcount);
        Equal(
            "source graph status=Valid modules=64 imports=63 reachable=64\n",
            Boundary.Output);
    }

    private static void Compilerˉsourceˉsymbolsˉrun()
    {
        var Libraryˉbytes = Compileˉwithˉsourceˉsymbolsˉsuccess(
            SOURCE_SYMBOLS_SOURCE,
            "Source-Symbols-Core.wv",
            includeˉsourceˉsymbols: false);
        Equal(SOURCE_SYMBOLS_SHA256, Moduleˉdigest.Calculateˉsha256(Libraryˉbytes));
        var Library = Moduleˉcodec.Readˉandˉverify(Libraryˉbytes);
        Equal("Compilerˉsourceˉsymbols", Library.Module.Name);
        Equal(Moduleˉprofile.Portable, Library.Module.Profile);
        Equal(38, Library.Module.Types.Length);
        Equal(36, Library.Module.Exports.Length);
        foreach (var Typeˉname in new[]
                 {
                     "Compilerˉsourceˉsymbolˉmatch",
                     "Compilerˉsourceˉsymbolˉstatus",
                     "Compilerˉsourceˉsymbolˉsummary",
                     "Compilerˉsourceˉtypeˉbinding",
                 })
        {
            True(Library.Module.Types.Any(Type => Type.Name == Typeˉname),
                $"Source-symbol type '{Typeˉname}' was not emitted.");
        }
        True(
            Library.Module.Exports.Any(Export => Export.Name == "Compilerˉvalidateˉsourceˉsymbols"),
            "The source-symbol validation entry point was not emitted.");

        var Demoˉbytes = Compileˉwithˉsourceˉsymbolsˉsuccess(
            SOURCE_SYMBOLS_DEMO_SOURCE,
            "Source-Symbols-Demo.wv");
        Equal(SOURCE_SYMBOLS_DEMO_SHA256, Moduleˉdigest.Calculateˉsha256(Demoˉbytes));
        Equal(
            0,
            new Referenceˉruntime(
                Moduleˉcodec.Readˉandˉverify(Demoˉbytes),
                new Referenceˉcapabilityˉhost(new StringWriter()),
                new(Runtimeˉoptions.Portableˉdefaults.Authorizedˉcapabilities,
                    Maximumˉinstructions: 1_500_000_000)).Runˉmain().Exitˉcode);

        var Toolˉbytes = Compileˉwithˉsourceˉsymbolsˉsuccess(
            SOURCE_SYMBOLS_TOOL_SOURCE,
            "Source-Symbols-Tool.wv");
        Equal(SOURCE_SYMBOLS_TOOL_SHA256, Moduleˉdigest.Calculateˉsha256(Toolˉbytes));
        var Tool = Moduleˉcodec.Readˉandˉverify(Toolˉbytes);
        const string Root = """
            module Symbolsˉtoolˉroot profile portable;
            import Symbolsˉtoolˉdependency;
            record Rootˉbox { State: Dependencyˉstate; }
            export fn Main(Value: Dependencyˉrecord) -> Dependencyˉstate {
                return Dependencyˉstate.Ready;
            }
            """;
        const string Dependency = """
            module Symbolsˉtoolˉdependency profile portable;
            record Dependencyˉrecord { Value: u32; }
            enum Dependencyˉstate { Ready = 0; }
            export fn Dependencyˉuse(Value: Dependencyˉrecord) -> Dependencyˉstate {
                return Dependencyˉstate.Ready;
            }
            """;
        var Small = Runˉsourceˉsetˉtool(
            Tool,
            [new("root.wv", Root), new("dependency.wv", Dependency)],
            500_000_000);
        Equal(0, Small.Exitˉcode);
        Equal(string.Empty, Small.Diagnostics);
        Equal(2, Small.Readˉcount);
        Equal(
            "source symbols status=Valid modules=2 capabilities=0 data=0 records=2 enums=1 functions=2 fields=2 members=1 parameters=2 directory-bytes=136 visibility-bytes=4\n",
            Small.Output);

        const string Unknownˉtype = """
            module Symbolsˉunknown profile portable;
            export fn Main(Value: Missingˉtype) -> i32 { return 0; }
            """;
        var Rejected = Runˉsourceˉsetˉtool(
            Tool,
            [new("unknown.wv", Unknownˉtype)],
            200_000_000);
        Equal(1, Rejected.Exitˉcode);
        Equal(string.Empty, Rejected.Output);
        Contains(Rejected.Diagnostics, "source symbols status=Unknownˉtype");
        Contains(Rejected.Diagnostics, "module=0 related-module=1 kind=Function");
        Equal(1, Rejected.Readˉcount);
    }

    private static void Compilerˉsourceˉbindingsˉrun()
    {
        var Libraryˉbytes = Compileˉwithˉsourceˉbindingsˉsuccess(
            SOURCE_BINDINGS_SOURCE,
            "Source-Bindings-Core.wv",
            includeˉsourceˉbindings: false);
        Equal(SOURCE_BINDINGS_SHA256, Moduleˉdigest.Calculateˉsha256(Libraryˉbytes));
        var Library = Moduleˉcodec.Readˉandˉverify(Libraryˉbytes);
        Equal("Compilerˉsourceˉbindings", Library.Module.Name);
        Equal(Moduleˉprofile.Portable, Library.Module.Profile);
        Equal(47, Library.Module.Types.Length);
        Equal(54, Library.Module.Exports.Length);
        foreach (var Typeˉname in new[]
                 {
                     "Compilerˉsourceˉbindingˉkind",
                     "Compilerˉsourceˉbindingˉstatus",
                     "Compilerˉsourceˉbindingˉsummary",
                     "Compilerˉsourceˉlocalˉmatch",
                 })
        {
            True(Library.Module.Types.Any(Type => Type.Name == Typeˉname),
                $"Source-binding type '{Typeˉname}' was not emitted.");
        }
        True(
            Library.Module.Exports.Any(Export => Export.Name == "Compilerˉvalidateˉsourceˉbindings"),
            "The source-binding validation entry point was not emitted.");

        var Demoˉbytes = Compileˉwithˉsourceˉbindingsˉsuccess(
            SOURCE_BINDINGS_DEMO_SOURCE,
            "Source-Bindings-Demo.wv");
        Equal(SOURCE_BINDINGS_DEMO_SHA256, Moduleˉdigest.Calculateˉsha256(Demoˉbytes));
        Equal(
            0,
            new Referenceˉruntime(
                Moduleˉcodec.Readˉandˉverify(Demoˉbytes),
                new Referenceˉcapabilityˉhost(new StringWriter()),
                new(Runtimeˉoptions.Portableˉdefaults.Authorizedˉcapabilities,
                    Maximumˉinstructions: 2_000_000_000)).Runˉmain().Exitˉcode);

        var Toolˉbytes = Compileˉwithˉsourceˉbindingsˉsuccess(
            SOURCE_BINDINGS_TOOL_SOURCE,
            "Source-Bindings-Tool.wv");
        Equal(SOURCE_BINDINGS_TOOL_SHA256, Moduleˉdigest.Calculateˉsha256(Toolˉbytes));
        var Tool = Moduleˉcodec.Readˉandˉverify(Toolˉbytes);
        const string Root = """
            module Bindingsˉtoolˉroot profile portable;
            import Bindingsˉtoolˉdependency;
            export fn Main(Input: u32) -> u32 {
                let Start: u32 = Dependencyˉvalue(Input);
                var Result: u32 = Start;
                Result = Result + 1u32;
                return Result;
            }
            """;
        const string Dependency = """
            module Bindingsˉtoolˉdependency profile portable;
            export fn Dependencyˉvalue(Value: u32) -> u32 { return Value; }
            """;
        var Small = Runˉsourceˉsetˉtool(
            Tool,
            [new("root.wv", Root), new("dependency.wv", Dependency)],
            700_000_000);
        Equal(0, Small.Exitˉcode);
        Equal(string.Empty, Small.Diagnostics);
        Equal(2, Small.Readˉcount);
        Equal(
            "source bindings status=Valid modules=2 functions=2 parameters=2 locals=2 reads=5 assignments=1 calls=1 directory-bytes=184\n",
            Small.Output);

        const string Unknownˉname = """
            module Bindingsˉunknown profile portable;
            export fn Main() -> u32 { return Missing; }
            """;
        var Rejected = Runˉsourceˉsetˉtool(
            Tool,
            [new("unknown.wv", Unknownˉname)],
            300_000_000);
        Equal(1, Rejected.Exitˉcode);
        Equal(string.Empty, Rejected.Output);
        Contains(Rejected.Diagnostics, "source bindings status=Unknownˉname");
        Contains(Rejected.Diagnostics, "module=0 related-module=1");
        Equal(1, Rejected.Readˉcount);
    }

    private static void Compilerˉsourceˉwirˉruns()
    {
        var Libraryˉbytes = Compileˉwithˉsourceˉwirˉsuccess(
            SOURCE_WIR_SOURCE,
            "Source-Wir-Core.wv",
            includeˉsourceˉwir: false);
        Equal(SOURCE_WIR_SHA256, Moduleˉdigest.Calculateˉsha256(Libraryˉbytes));
        var Library = Moduleˉcodec.Readˉandˉverify(Libraryˉbytes);
        Equal("Compilerˉsourceˉwir", Library.Module.Name);
        Equal(Moduleˉprofile.Portable, Library.Module.Profile);
        Equal(64, Library.Module.Exports.Length);
        foreach (var Typeˉname in new[]
                 {
                     "Compilerˉsourceˉwirˉoperation",
                     "Compilerˉsourceˉwirˉstatus",
                     "Compilerˉsourceˉwirˉsummary",
                 })
        {
            True(Library.Module.Types.Any(Type => Type.Name == Typeˉname),
                $"WVIR type '{Typeˉname}' was not emitted.");
        }
        True(
            Library.Module.Exports.Any(Export => Export.Name == "Compilerˉvalidateˉsourceˉwir"),
            "The source-to-WVIR validation entry point was not emitted.");
        True(
            Library.Module.Exports.Any(Export => Export.Name == "Compilerˉsourceˉwirˉdirectoryˉisˉvalid"),
            "The independent WVIR directory validator was not emitted.");

        var Demoˉbytes = Compileˉwithˉsourceˉwirˉsuccess(
            SOURCE_WIR_DEMO_SOURCE,
            "Source-Wir-Demo.wv");
        Equal(SOURCE_WIR_DEMO_SHA256, Moduleˉdigest.Calculateˉsha256(Demoˉbytes));
        Equal(
            0,
            new Referenceˉruntime(
                Moduleˉcodec.Readˉandˉverify(Demoˉbytes),
                new Referenceˉcapabilityˉhost(new StringWriter()),
                new(Runtimeˉoptions.Portableˉdefaults.Authorizedˉcapabilities,
                    Maximumˉinstructions: 4_000_000_000)).Runˉmain().Exitˉcode);

        var Toolˉbytes = Compileˉwithˉsourceˉwirˉsuccess(
            SOURCE_WIR_TOOL_SOURCE,
            "Source-Wir-Tool.wv");
        Equal(SOURCE_WIR_TOOL_SHA256, Moduleˉdigest.Calculateˉsha256(Toolˉbytes));
        var Tool = Moduleˉcodec.Readˉandˉverify(Toolˉbytes);
        var Valid = Runˉsourceˉsetˉtool(
            Tool,
            [new("valid.wv", SOURCE_WIR_VALID_SOURCE)],
            2_000_000_000);
        Equal(0, Valid.Exitˉcode);
        Equal(string.Empty, Valid.Diagnostics);
        Equal(1, Valid.Readˉcount);
        Equal(
            "source wir status=Valid modules=1 functions=8 blocks=11 operations=44 temporaries=36 operands=29 directory-bytes=3200\n",
            Valid.Output);
    }

    private static void Compilerˉsourceˉwvbˉruns()
    {
        var Libraryˉbytes = Compileˉwithˉsourceˉwvbˉsuccess(
            SOURCE_WVB_SOURCE,
            "Source-Wvb-Core.wv",
            includeˉsourceˉwvb: false);
        Equal(SOURCE_WVB_SHA256, Moduleˉdigest.Calculateˉsha256(Libraryˉbytes));
        var Library = Moduleˉcodec.Readˉandˉverify(Libraryˉbytes);
        Equal("Compilerˉsourceˉwvb", Library.Module.Name);
        Equal(Moduleˉprofile.Portable, Library.Module.Profile);
        True(
            Library.Module.Exports.Any(Export => Export.Name == "Compilerˉcompileˉsourceˉwvb"),
            "The source-to-WVB entry point was not emitted.");
        True(
            Library.Module.Types.Any(Type => Type.Name == "Compilerˉsourceˉwvbˉsummary"),
            "The source-to-WVB summary record was not emitted.");

        var Demoˉbytes = Compileˉwithˉsourceˉwvbˉsuccess(
            SOURCE_WVB_DEMO_SOURCE,
            "Source-Wvb-Demo.wv");
        Equal(SOURCE_WVB_DEMO_SHA256, Moduleˉdigest.Calculateˉsha256(Demoˉbytes));
        Equal(
            0,
            new Referenceˉruntime(
                Moduleˉcodec.Readˉandˉverify(Demoˉbytes),
                new Referenceˉcapabilityˉhost(new StringWriter()),
                new(Runtimeˉoptions.Portableˉdefaults.Authorizedˉcapabilities,
                    Maximumˉinstructions: 4_000_000_000)).Runˉmain().Exitˉcode);

        var Toolˉbytes = Compileˉwithˉsourceˉwvbˉsuccess(
            SOURCE_WVB_TOOL_SOURCE,
            "Source-Wvb-Tool.wv");
        Equal(SOURCE_WVB_TOOL_SHA256, Moduleˉdigest.Calculateˉsha256(Toolˉbytes));
        var Tool = Moduleˉcodec.Readˉandˉverify(Toolˉbytes);
        var Sourceˉbytes = System.Text.Encoding.UTF8.GetBytes(
            SOURCE_WVB_FUNCTION_ONLY_SOURCE).ToImmutableArray();
        var Output = new StringWriter();
        var Diagnostics = new StringWriter();
        var Writer = new Capturingˉfileˉwriter();
        var Reader = new Testˉfileˉreader((Name, Maximumˉbytes) =>
        {
            Equal("function-only.wv", Name);
            True(Sourceˉbytes.Length <= Maximumˉbytes,
                "The source-to-WVB hosted byte limit was too small.");
            return Sourceˉbytes;
        });
        var Authorized = Tool.Module.Capabilities
            .Select(Capability => Capability.Name)
            .ToImmutableHashSet(StringComparer.Ordinal);
        var Toolˉresult = new Referenceˉruntime(
            Tool,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                ["function-only.wv", "function-only.wvb"],
                Output,
                Diagnostics,
                Reader,
                Writer)),
            new(Authorized, Maximumˉinstructions: 4_000_000_000)).Runˉmain();
        Equal(0, Toolˉresult.Exitˉcode);
        Equal(string.Empty, Diagnostics.ToString());
        Equal(
            "source wvb status=Valid functions=4 code-bytes=532 module-bytes=815\n",
            Output.ToString().Replace("\r\n", "\n", StringComparison.Ordinal));
        Equal(1, Reader.Readˉcount);
        Equal(1, Writer.Writeˉcount);
        Equal("function-only.wvb", Writer.Resourceˉname);

        var Stageˉzeroˉbytes = Compileˉsuccess(SOURCE_WVB_FUNCTION_ONLY_SOURCE);
        Sequenceˉequal(Stageˉzeroˉbytes, Writer.Bytes);
        var Generated = Moduleˉcodec.Readˉandˉverify(Writer.Bytes.AsSpan());
        Equal("Sourceˉwvbˉfixture", Generated.Module.Name);
        Equal(
            6,
            new Referenceˉruntime(
                Generated,
                new Referenceˉcapabilityˉhost(new StringWriter()),
                Runtimeˉoptions.Portableˉdefaults).Runˉmain().Exitˉcode);

        var Dataˉsourceˉbytes = System.Text.Encoding.UTF8.GetBytes(
            SOURCE_WVB_DATA_AND_TEXT_SOURCE).ToImmutableArray();
        var Dataˉoutput = new StringWriter();
        var Dataˉdiagnostics = new StringWriter();
        var Dataˉwriter = new Capturingˉfileˉwriter();
        var Dataˉreader = new Testˉfileˉreader((Name, Maximumˉbytes) =>
        {
            Equal("data-and-text.wv", Name);
            True(Dataˉsourceˉbytes.Length <= Maximumˉbytes,
                "The source-to-WVB hosted byte limit was too small for data and text.");
            return Dataˉsourceˉbytes;
        });
        var Dataˉtoolˉresult = new Referenceˉruntime(
            Tool,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                ["data-and-text.wv", "data-and-text.wvb"],
                Dataˉoutput,
                Dataˉdiagnostics,
                Dataˉreader,
                Dataˉwriter)),
            new(Authorized, Maximumˉinstructions: 4_000_000_000)).Runˉmain();
        Equal(0, Dataˉtoolˉresult.Exitˉcode);
        Equal(string.Empty, Dataˉdiagnostics.ToString());
        Equal(
            "source wvb status=Valid functions=3 code-bytes=1210 module-bytes=1651\n",
            Dataˉoutput.ToString().Replace("\r\n", "\n", StringComparison.Ordinal));
        Equal(1, Dataˉreader.Readˉcount);
        Equal(1, Dataˉwriter.Writeˉcount);
        Equal("data-and-text.wvb", Dataˉwriter.Resourceˉname);

        var Dataˉstageˉzeroˉbytes = Compileˉsuccess(SOURCE_WVB_DATA_AND_TEXT_SOURCE);
        Equal(
            SOURCE_WVB_DATA_AND_TEXT_SHA256,
            Moduleˉdigest.Calculateˉsha256(Dataˉstageˉzeroˉbytes));
        Sequenceˉequal(Dataˉstageˉzeroˉbytes, Dataˉwriter.Bytes);
        var Dataˉgenerated = Moduleˉcodec.Readˉandˉverify(Dataˉwriter.Bytes.AsSpan());
        Equal("Sourceˉwvbˉdataˉandˉtext", Dataˉgenerated.Module.Name);
        Equal(5, Dataˉgenerated.Module.Data.Length);
        Equal(
            13,
            new Referenceˉruntime(
                Dataˉgenerated,
                new Referenceˉcapabilityˉhost(new StringWriter()),
                Runtimeˉoptions.Portableˉdefaults).Runˉmain().Exitˉcode);

        var Nominalˉsourceˉbytes = System.Text.Encoding.UTF8.GetBytes(
            SOURCE_WVB_NOMINAL_TYPES_SOURCE).ToImmutableArray();
        var Nominalˉoutput = new StringWriter();
        var Nominalˉdiagnostics = new StringWriter();
        var Nominalˉwriter = new Capturingˉfileˉwriter();
        var Nominalˉreader = new Testˉfileˉreader((Name, Maximumˉbytes) =>
        {
            Equal("nominal-types.wv", Name);
            True(Nominalˉsourceˉbytes.Length <= Maximumˉbytes,
                "The source-to-WVB hosted byte limit was too small for nominal types.");
            return Nominalˉsourceˉbytes;
        });
        var Nominalˉtoolˉresult = new Referenceˉruntime(
            Tool,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                ["nominal-types.wv", "nominal-types.wvb"],
                Nominalˉoutput,
                Nominalˉdiagnostics,
                Nominalˉreader,
                Nominalˉwriter)),
            new(Authorized, Maximumˉinstructions: 4_000_000_000)).Runˉmain();
        Equal(0, Nominalˉtoolˉresult.Exitˉcode);
        Equal(string.Empty, Nominalˉdiagnostics.ToString());
        Equal(
            "source wvb status=Valid functions=3 code-bytes=1097 module-bytes=1781\n",
            Nominalˉoutput.ToString().Replace("\r\n", "\n", StringComparison.Ordinal));
        Equal(1, Nominalˉreader.Readˉcount);
        Equal(1, Nominalˉwriter.Writeˉcount);
        Equal("nominal-types.wvb", Nominalˉwriter.Resourceˉname);

        var Nominalˉstageˉzeroˉbytes = Compileˉsuccess(SOURCE_WVB_NOMINAL_TYPES_SOURCE);
        Equal(
            SOURCE_WVB_NOMINAL_TYPES_SHA256,
            Moduleˉdigest.Calculateˉsha256(Nominalˉstageˉzeroˉbytes));
        Sequenceˉequal(Nominalˉstageˉzeroˉbytes, Nominalˉwriter.Bytes);
        var Nominalˉgenerated = Moduleˉcodec.Readˉandˉverify(Nominalˉwriter.Bytes.AsSpan());
        Equal("Sourceˉwvbˉnominalˉtypes", Nominalˉgenerated.Module.Name);
        Equal(4, Nominalˉgenerated.Module.Types.Length);
        Equal("Envelope", Nominalˉgenerated.Module.Types[0].Name);
        Equal("Reading", Nominalˉgenerated.Module.Types[1].Name);
        Equal("Signal", Nominalˉgenerated.Module.Types[2].Name);
        Equal("Weather", Nominalˉgenerated.Module.Types[3].Name);
        Equal(
            11,
            new Referenceˉruntime(
                Nominalˉgenerated,
                new Referenceˉcapabilityˉhost(new StringWriter()),
                Runtimeˉoptions.Portableˉdefaults).Runˉmain().Exitˉcode);

        var Hostedˉsourceˉbytes = System.Text.Encoding.UTF8.GetBytes(
            SOURCE_WVB_HOSTED_CAPABILITIES_SOURCE).ToImmutableArray();
        var Hostedˉoutput = new StringWriter();
        var Hostedˉdiagnostics = new StringWriter();
        var Hostedˉwriter = new Capturingˉfileˉwriter();
        var Hostedˉreader = new Testˉfileˉreader((Name, Maximumˉbytes) =>
        {
            Equal("hosted-capabilities.wv", Name);
            True(Hostedˉsourceˉbytes.Length <= Maximumˉbytes,
                "The source-to-WVB hosted byte limit was too small for capabilities.");
            return Hostedˉsourceˉbytes;
        });
        var Hostedˉtoolˉresult = new Referenceˉruntime(
            Tool,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                ["hosted-capabilities.wv", "hosted-capabilities.wvb"],
                Hostedˉoutput,
                Hostedˉdiagnostics,
                Hostedˉreader,
                Hostedˉwriter)),
            new(Authorized, Maximumˉinstructions: 4_000_000_000)).Runˉmain();
        Equal(0, Hostedˉtoolˉresult.Exitˉcode);
        Equal(string.Empty, Hostedˉdiagnostics.ToString());
        Equal(
            "source wvb status=Valid functions=7 code-bytes=249 module-bytes=849\n",
            Hostedˉoutput.ToString().Replace("\r\n", "\n", StringComparison.Ordinal));
        Equal(1, Hostedˉreader.Readˉcount);
        Equal(1, Hostedˉwriter.Writeˉcount);
        Equal("hosted-capabilities.wvb", Hostedˉwriter.Resourceˉname);

        var Hostedˉstageˉzeroˉbytes = Compileˉsuccess(SOURCE_WVB_HOSTED_CAPABILITIES_SOURCE);
        Equal(
            SOURCE_WVB_HOSTED_CAPABILITIES_SHA256,
            Moduleˉdigest.Calculateˉsha256(Hostedˉstageˉzeroˉbytes));
        Sequenceˉequal(Hostedˉstageˉzeroˉbytes, Hostedˉwriter.Bytes);
        var Hostedˉgenerated = Moduleˉcodec.Readˉandˉverify(Hostedˉwriter.Bytes.AsSpan());
        Equal("Sourceˉwvbˉhostedˉcapabilities", Hostedˉgenerated.Module.Name);
        Equal(Moduleˉprofile.Hosted, Hostedˉgenerated.Module.Profile);
        Sequenceˉequal(
            new[]
            {
                Capabilityˉcatalog.CONSOLE_WRITE,
                Capabilityˉcatalog.CONSOLE_WRITE_LINE,
                Capabilityˉcatalog.DIAGNOSTIC_WRITE_LINE,
                Capabilityˉcatalog.FILE_READ_BYTES,
                Capabilityˉcatalog.FILE_WRITE_BYTES,
                Capabilityˉcatalog.PROCESS_ARGUMENT,
                Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT,
            },
            Hostedˉgenerated.Module.Capabilities.Select(Capability => Capability.Name));
        var Hostedˉinspection = Moduleˉinspector.Inspect(
            Hostedˉgenerated,
            Hostedˉwriter.Bytes.AsSpan());
        for (var Capabilityˉindex = 0; Capabilityˉindex < 7; Capabilityˉindex++)
        {
            Contains(Hostedˉinspection, $"call.capability capability[{Capabilityˉindex}]");
        }
        var Runtimeˉoutput = new StringWriter();
        var Runtimeˉdiagnostics = new StringWriter();
        var Runtimeˉreader = new Testˉfileˉreader((_, _) => []);
        var Runtimeˉwriter = new Capturingˉfileˉwriter();
        var Runtimeˉauthorized = Hostedˉgenerated.Module.Capabilities
            .Select(Capability => Capability.Name)
            .ToImmutableHashSet(StringComparer.Ordinal);
        Equal(
            0,
            new Referenceˉruntime(
                Hostedˉgenerated,
                new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                    [],
                    Runtimeˉoutput,
                    Runtimeˉdiagnostics,
                    Runtimeˉreader,
                    Runtimeˉwriter)),
                new(Runtimeˉauthorized)).Runˉmain().Exitˉcode);
        Equal(string.Empty, Runtimeˉoutput.ToString());
        Equal(string.Empty, Runtimeˉdiagnostics.ToString());
        Equal(0, Runtimeˉreader.Readˉcount);
        Equal(0, Runtimeˉwriter.Writeˉcount);

        var Compositionˉsources = new Dictionary<string, ImmutableArray<byte>>(
            StringComparer.Ordinal)
        {
            ["composition-root.wv"] = System.Text.Encoding.UTF8.GetBytes(
                SOURCE_WVB_COMPOSITION_ROOT_SOURCE).ToImmutableArray(),
            ["composition-leaf.wv"] = System.Text.Encoding.UTF8.GetBytes(
                SOURCE_WVB_COMPOSITION_LEAF_SOURCE).ToImmutableArray(),
            ["composition-middle.wv"] = System.Text.Encoding.UTF8.GetBytes(
                SOURCE_WVB_COMPOSITION_MIDDLE_SOURCE).ToImmutableArray(),
        };
        var Compositionˉoutput = new StringWriter();
        var Compositionˉdiagnostics = new StringWriter();
        var Compositionˉwriter = new Capturingˉfileˉwriter();
        var Compositionˉreader = new Testˉfileˉreader((Name, Maximumˉbytes) =>
        {
            True(Compositionˉsources.TryGetValue(Name, out var Source),
                $"Unexpected multi-module source resource '{Name}'.");
            True(Source.Length <= Maximumˉbytes,
                "The source-to-WVB hosted byte limit was too small for composition.");
            return Source;
        });
        var Compositionˉtoolˉresult = new Referenceˉruntime(
            Tool,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                [
                    "composition-root.wv",
                    "composition-leaf.wv",
                    "composition-middle.wv",
                    "composition.wvb",
                ],
                Compositionˉoutput,
                Compositionˉdiagnostics,
                Compositionˉreader,
                Compositionˉwriter)),
            new(Authorized, Maximumˉinstructions: 4_000_000_000)).Runˉmain();
        Equal(0, Compositionˉtoolˉresult.Exitˉcode);
        Equal(string.Empty, Compositionˉdiagnostics.ToString());
        Equal(
            "source wvb status=Valid functions=5 code-bytes=451 module-bytes=1030\n",
            Compositionˉoutput.ToString().Replace("\r\n", "\n", StringComparison.Ordinal));
        Equal(3, Compositionˉreader.Readˉcount);
        Equal(1, Compositionˉwriter.Writeˉcount);
        Equal("composition.wvb", Compositionˉwriter.Resourceˉname);

        var Compositionˉstageˉzero = Seedˉcompiler.Compileˉmodules(
            new("composition-root.wv", SOURCE_WVB_COMPOSITION_ROOT_SOURCE),
            [
                new("composition-leaf.wv", SOURCE_WVB_COMPOSITION_LEAF_SOURCE),
                new("composition-middle.wv", SOURCE_WVB_COMPOSITION_MIDDLE_SOURCE),
            ]);
        True(
            Compositionˉstageˉzero.Success,
            "Stage 0 composition failed: " +
                string.Join(" | ", Compositionˉstageˉzero.Diagnostics));
        Equal(
            SOURCE_WVB_COMPOSITION_SHA256,
            Moduleˉdigest.Calculateˉsha256(Compositionˉstageˉzero.Moduleˉbytes.AsSpan()));
        Sequenceˉequal(Compositionˉstageˉzero.Moduleˉbytes, Compositionˉwriter.Bytes);
        var Compositionˉgenerated = Moduleˉcodec.Readˉandˉverify(
            Compositionˉwriter.Bytes.AsSpan());
        Sequenceˉequal(
            [
                "Compositionˉanswer",
                "Compositionˉincrement",
                "Compositionˉlabel",
                "Compositionˉmake",
                "Main",
            ],
            Compositionˉgenerated.Module.Functions.Select(Function => Function.Name));
        Sequenceˉequal(
            ["Compositionˉoffset", "__Text_000000", "__Text_000001"],
            Compositionˉgenerated.Module.Data.Select(Data => Data.Name));
        Sequenceˉequal(
            ["Compositionˉvalue", "Compositionˉstatus"],
            Compositionˉgenerated.Module.Types.Select(Type => Type.Name));
        Sequenceˉequal(
            ["Main"],
            Compositionˉgenerated.Module.Exports.Select(Export => Export.Name));
        Equal(
            42,
            new Referenceˉruntime(
                Compositionˉgenerated,
                new Referenceˉcapabilityˉhost(new StringWriter()),
                Runtimeˉoptions.Portableˉdefaults).Runˉmain().Exitˉcode);

        var Rejectedˉdiagnostics = new StringWriter();
        var Rejectedˉwriter = new Capturingˉfileˉwriter();
        var Rejectedˉreader = new Testˉfileˉreader((Name, _) =>
            Compositionˉsources[Name]);
        var Rejectedˉresult = new Referenceˉruntime(
            Tool,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                [
                    "composition-root.wv",
                    "composition-middle.wv",
                    "composition-leaf.wv",
                    "rejected.wvb",
                ],
                new StringWriter(),
                Rejectedˉdiagnostics,
                Rejectedˉreader,
                Rejectedˉwriter)),
            new(Authorized, Maximumˉinstructions: 4_000_000_000)).Runˉmain();
        Equal(1, Rejectedˉresult.Exitˉcode);
        Contains(Rejectedˉdiagnostics.ToString(), "source wvb status=Sourceˉwir");
        Equal(3, Rejectedˉreader.Readˉcount);
        Equal(0, Rejectedˉwriter.Writeˉcount);
    }

    private static void Compilerˉwebassemblyˉruns()
    {
        var Coreˉbytes = Compileˉwithˉwebassemblyˉsuccess(
            WEBASSEMBLY_CORE_SOURCE,
            "WebAssembly-Core.wv",
            includeˉwebassembly: false);
        Equal(WEBASSEMBLY_CORE_SHA256, Moduleˉdigest.Calculateˉsha256(Coreˉbytes));
        var Core = Moduleˉcodec.Readˉandˉverify(Coreˉbytes);
        Equal("Compilerˉwebassembly", Core.Module.Name);
        Equal(Moduleˉprofile.Portable, Core.Module.Profile);
        True(
            Core.Module.Exports.Any(Export =>
                Export.Name == "Compilerˉlowerˉwvbˉwebassembly"),
            "The Windvale WebAssembly lowering entry point was not emitted.");

        var Toolˉbytes = Compileˉwithˉwebassemblyˉsuccess(
            WEBASSEMBLY_TOOL_SOURCE,
            "WebAssembly-Tool.wv");
        Equal(WEBASSEMBLY_TOOL_SHA256, Moduleˉdigest.Calculateˉsha256(Toolˉbytes));
        var Tool = Moduleˉcodec.Readˉandˉverify(Toolˉbytes);
        var Demoˉbytes = Compileˉwithˉwebassemblyˉsuccess(
            WEBASSEMBLY_DEMO_SOURCE,
            "WebAssembly-Demo.wv");
        Equal(WEBASSEMBLY_DEMO_SHA256, Moduleˉdigest.Calculateˉsha256(Demoˉbytes));
        Equal(
            0,
            new Referenceˉruntime(
                Moduleˉcodec.Readˉandˉverify(Demoˉbytes),
                new Referenceˉcapabilityˉhost(new StringWriter()),
                Runtimeˉoptions.Portableˉdefaults).Runˉmain().Exitˉcode);
        var Constantˉwvb = Compileˉsuccess(WEBASSEMBLY_CONSTANT_SOURCE);
        Equal(
            WEBASSEMBLY_CONSTANT_WVB_SHA256,
            Moduleˉdigest.Calculateˉsha256(Constantˉwvb));
        var Constantˉverified = Moduleˉcodec.Readˉandˉverify(Constantˉwvb);
        Equal(
            42,
            new Referenceˉruntime(
                Constantˉverified,
                new Referenceˉcapabilityˉhost(new StringWriter()),
                Runtimeˉoptions.Portableˉdefaults).Runˉmain().Exitˉcode);

        var First = Runˉwebassemblyˉtool(Tool, Constantˉwvb);
        Equal(0, First.Exitˉcode);
        Equal(string.Empty, First.Diagnostics);
        Equal(1, First.Readˉcount);
        Equal(1, First.Writeˉcount);
        Equal("output.wasm", First.Writtenˉresourceˉname);
        Equal("webassembly status=Valid module-bytes=37 result=42\n", First.Output);
        Sequenceˉequal<byte>(
            [
                0x00, 0x61, 0x73, 0x6D, 0x01, 0x00, 0x00, 0x00,
                0x01, 0x05, 0x01, 0x60, 0x00, 0x01, 0x7F,
                0x03, 0x02, 0x01, 0x00,
                0x07, 0x08, 0x01, 0x04, 0x4D, 0x61, 0x69, 0x6E, 0x00, 0x00,
                0x0A, 0x06, 0x01, 0x04, 0x00, 0x41, 0x2A, 0x0B,
            ],
            First.Writtenˉbytes);
        Equal(
            WEBASSEMBLY_CONSTANT_SHA256,
            Moduleˉdigest.Calculateˉsha256(First.Writtenˉbytes.AsSpan()));
        Equal(42, Executeˉconstantˉwebassembly(First.Writtenˉbytes.AsSpan()));

        var Second = Runˉwebassemblyˉtool(Tool, Constantˉwvb);
        Equal(0, Second.Exitˉcode);
        Sequenceˉequal(First.Writtenˉbytes, Second.Writtenˉbytes);

        foreach (var Expected in new[]
        {
            -134_217_729,
            -65,
            -64,
            63,
            64,
            8_192,
            134_217_728,
            int.MaxValue,
            int.MinValue,
        })
        {
            var Wvb = Constantˉwvb.ToArray();
            var Codeˉpayload = Findˉsectionˉpayload(Wvb, Sectionˉkind.Code);
            BinaryPrimitives.WriteInt32LittleEndian(Wvb.AsSpan(Codeˉpayload + 1, 4), Expected);
            Equal(
                Expected,
                new Referenceˉruntime(
                    Moduleˉcodec.Readˉandˉverify(Wvb),
                    new Referenceˉcapabilityˉhost(new StringWriter()),
                    Runtimeˉoptions.Portableˉdefaults).Runˉmain().Exitˉcode);
            var Lowered = Runˉwebassemblyˉtool(Tool, Wvb);
            Equal(0, Lowered.Exitˉcode);
            Equal(Expected, Executeˉconstantˉwebassembly(Lowered.Writtenˉbytes.AsSpan()));
        }

        var Checkedˉaddˉwvb = Compileˉsuccess(WEBASSEMBLY_CHECKED_ADD_SOURCE);
        Equal(
            WEBASSEMBLY_CHECKED_ADD_WVB_SHA256,
            Moduleˉdigest.Calculateˉsha256(Checkedˉaddˉwvb));
        var Checkedˉreference = Runˉreferenceˉwebassemblyˉi32(Checkedˉaddˉwvb);
        Equal(0, Checkedˉreference.Status);
        Equal(int.MaxValue, Checkedˉreference.Result);
        Equal(10L, Checkedˉreference.Executedˉinstructions);

        var Checkedˉlowered = Runˉwebassemblyˉtool(Tool, Checkedˉaddˉwvb);
        Equal(0, Checkedˉlowered.Exitˉcode);
        Equal(string.Empty, Checkedˉlowered.Diagnostics);
        Equal(1, Checkedˉlowered.Readˉcount);
        Equal(1, Checkedˉlowered.Writeˉcount);
        Equal(
            "webassembly status=Valid module-bytes=176 execution-abi=1\n",
            Checkedˉlowered.Output);
        Equal(
            WEBASSEMBLY_CHECKED_ADD_HEX,
            Convert.ToHexString(Checkedˉlowered.Writtenˉbytes.AsSpan()));
        Equal(
            WEBASSEMBLY_CHECKED_ADD_SHA256,
            Moduleˉdigest.Calculateˉsha256(Checkedˉlowered.Writtenˉbytes.AsSpan()));
        Equal(
            Checkedˉreference,
            Executeˉcheckedˉaddˉwebassembly(Checkedˉlowered.Writtenˉbytes.AsSpan()));

        var Checkedˉoverflowˉwvb = Compileˉsuccess(
            WEBASSEMBLY_CHECKED_ADD_OVERFLOW_SOURCE);
        Equal(
            WEBASSEMBLY_CHECKED_ADD_OVERFLOW_WVB_SHA256,
            Moduleˉdigest.Calculateˉsha256(Checkedˉoverflowˉwvb));
        var Checkedˉoverflowˉreference = Runˉreferenceˉwebassemblyˉi32(
            Checkedˉoverflowˉwvb);
        Equal(new WebAssemblyˉexecutionˉresult(3007, 0, 7), Checkedˉoverflowˉreference);
        var Checkedˉoverflowˉlowered = Runˉwebassemblyˉtool(
            Tool,
            Checkedˉoverflowˉwvb);
        Equal(0, Checkedˉoverflowˉlowered.Exitˉcode);
        Equal(
            WEBASSEMBLY_CHECKED_ADD_OVERFLOW_SHA256,
            Moduleˉdigest.Calculateˉsha256(
                Checkedˉoverflowˉlowered.Writtenˉbytes.AsSpan()));
        Equal(
            Checkedˉoverflowˉreference,
            Executeˉcheckedˉaddˉwebassembly(
                Checkedˉoverflowˉlowered.Writtenˉbytes.AsSpan()));

        var Checkedˉrepeat = Runˉwebassemblyˉtool(Tool, Checkedˉaddˉwvb);
        Equal(0, Checkedˉrepeat.Exitˉcode);
        Sequenceˉequal(Checkedˉlowered.Writtenˉbytes, Checkedˉrepeat.Writtenˉbytes);

        foreach (var Case in new (int Left, int Right, int Status, int Result, long Steps)[]
        {
            (40, 2, 0, 42, 10),
            (int.MaxValue, 0, 0, int.MaxValue, 10),
            (int.MinValue, 0, 0, int.MinValue, 10),
            (-20, -22, 0, -42, 10),
            (int.MaxValue, 1, 3007, 0, 7),
            (int.MinValue, -1, 3007, 0, 7),
            (int.MaxValue, int.MinValue, 0, -1, 10),
        })
        {
            var Wvb = Checkedˉaddˉwvb.ToArray();
            var Codeˉpayload = Findˉsectionˉpayload(Wvb, Sectionˉkind.Code);
            BinaryPrimitives.WriteInt32LittleEndian(
                Wvb.AsSpan(Codeˉpayload + 1, 4),
                Case.Left);
            BinaryPrimitives.WriteInt32LittleEndian(
                Wvb.AsSpan(Codeˉpayload + 11, 4),
                Case.Right);
            var Reference = Runˉreferenceˉwebassemblyˉi32(Wvb);
            Equal(Case.Status, Reference.Status);
            Equal(Case.Result, Reference.Result);
            Equal(Case.Steps, Reference.Executedˉinstructions);
            var Lowered = Runˉwebassemblyˉtool(Tool, Wvb);
            Equal(0, Lowered.Exitˉcode);
            Equal(
                Reference,
                Executeˉcheckedˉaddˉwebassembly(Lowered.Writtenˉbytes.AsSpan()));
        }

        foreach (var Case in new (
            string Source,
            string Wvbˉsha256,
            string Wasmˉsha256,
            int Wasmˉbytes,
            int Status,
            int Result,
            long Steps)[]
        {
            (
                WEBASSEMBLY_STRAIGHT_I32_SOURCE,
                WEBASSEMBLY_STRAIGHT_I32_WVB_SHA256,
                WEBASSEMBLY_STRAIGHT_I32_SHA256,
                432,
                0,
                42,
                30),
            (
                WEBASSEMBLY_SUBTRACT_OVERFLOW_SOURCE,
                WEBASSEMBLY_SUBTRACT_OVERFLOW_WVB_SHA256,
                WEBASSEMBLY_SUBTRACT_OVERFLOW_SHA256,
                268,
                3007,
                0,
                10),
            (
                WEBASSEMBLY_MULTIPLY_OVERFLOW_SOURCE,
                WEBASSEMBLY_MULTIPLY_OVERFLOW_WVB_SHA256,
                WEBASSEMBLY_MULTIPLY_OVERFLOW_SHA256,
                224,
                3007,
                0,
                7),
            (
                WEBASSEMBLY_NEGATE_OVERFLOW_SOURCE,
                WEBASSEMBLY_NEGATE_OVERFLOW_WVB_SHA256,
                WEBASSEMBLY_NEGATE_OVERFLOW_SHA256,
                307,
                3007,
                0,
                13),
        })
        {
            var Wvb = Compileˉsuccess(Case.Source);
            Equal(Case.Wvbˉsha256, Moduleˉdigest.Calculateˉsha256(Wvb));
            var Verified = Moduleˉcodec.Readˉandˉverify(Wvb);
            var Reference = Runˉreferenceˉwebassemblyˉi32(Wvb);
            Equal(
                new WebAssemblyˉexecutionˉresult(
                    Case.Status,
                    Case.Result,
                    Case.Steps),
                Reference);
            var Lowered = Runˉwebassemblyˉtool(Tool, Wvb);
            Equal(0, Lowered.Exitˉcode);
            Equal(
                $"webassembly status=Valid module-bytes={Case.Wasmˉbytes} execution-abi=1\n",
                Lowered.Output);
            Equal(
                Case.Wasmˉsha256,
                Moduleˉdigest.Calculateˉsha256(Lowered.Writtenˉbytes.AsSpan()));
            Equal(
                Reference,
                Executeˉstraightˉi32ˉwebassembly(
                    Lowered.Writtenˉbytes.AsSpan(),
                    Verified));
            var Repeat = Runˉwebassemblyˉtool(Tool, Wvb);
            Equal(0, Repeat.Exitˉcode);
            Sequenceˉequal(Lowered.Writtenˉbytes, Repeat.Writtenˉbytes);
        }

        foreach (var Source in new[]
        {
            "module Wasmˉsubtractˉsuccess profile portable; export fn Main() -> i32 { return -40 - 2; }",
            "module Wasmˉmultiplyˉnegative profile portable; export fn Main() -> i32 { return -50000 * 50000; }",
            "module Wasmˉmultiplyˉminimum profile portable; export fn Main() -> i32 { return (-2147483647 - 1) * -1; }",
            "module Wasmˉnegateˉsuccess profile portable; export fn Main() -> i32 { return -42; }",
        })
        {
            var Wvb = Compileˉsuccess(Source);
            var Verified = Moduleˉcodec.Readˉandˉverify(Wvb);
            var Reference = Runˉreferenceˉwebassemblyˉi32(Wvb);
            var Lowered = Runˉwebassemblyˉtool(Tool, Wvb);
            Equal(0, Lowered.Exitˉcode);
            Equal(
                Reference,
                Executeˉstraightˉi32ˉwebassembly(
                    Lowered.Writtenˉbytes.AsSpan(),
                    Verified));
        }

        var Popˉwvb = Moduleˉcodec.Write(Buildˉmodule(
            [
                .. I32ˉinstruction(20),
                .. I32ˉinstruction(22),
                (byte)Opcode.I32ˉadd,
                (byte)Opcode.Pop,
                .. I32ˉinstruction(42),
                (byte)Opcode.Return,
            ],
            Valueˉtype.I32,
            maximumˉstack: 2)).ToArray();
        var Popˉverified = Moduleˉcodec.Readˉandˉverify(Popˉwvb);
        var Popˉreference = Runˉreferenceˉwebassemblyˉi32(Popˉwvb);
        Equal(new WebAssemblyˉexecutionˉresult(0, 42, 6), Popˉreference);
        var Popˉlowered = Runˉwebassemblyˉtool(Tool, Popˉwvb);
        Equal(0, Popˉlowered.Exitˉcode);
        Equal(
            Popˉreference,
            Executeˉstraightˉi32ˉwebassembly(
                Popˉlowered.Writtenˉbytes.AsSpan(),
                Popˉverified));

        var Outputˉlimitˉcode = ImmutableArray.CreateBuilder<byte>();
        Outputˉlimitˉcode.AddRange(I32ˉinstruction(1));
        for (var Index = 0; Index < 2_047; Index++)
        {
            Outputˉlimitˉcode.AddRange(I32ˉinstruction(1));
            Outputˉlimitˉcode.Add((byte)Opcode.I32ˉmultiply);
        }
        Outputˉlimitˉcode.Add((byte)Opcode.Return);
        var Outputˉlimitˉwvb = Moduleˉcodec.Write(Buildˉmodule(
            Outputˉlimitˉcode.ToImmutable(),
            Valueˉtype.I32,
            maximumˉstack: 2));
        var Outputˉlimitˉresult = Runˉwebassemblyˉtool(Tool, Outputˉlimitˉwvb);
        Equal(1, Outputˉlimitˉresult.Exitˉcode);
        Equal(
            "webassembly status=Outputˉlimit\n",
            Outputˉlimitˉresult.Diagnostics);
        Equal(0, Outputˉlimitˉresult.Writeˉcount);

        var Instructionˉlimitˉcode = ImmutableArray.CreateBuilder<byte>();
        for (var Index = 0; Index < 2_048; Index++)
        {
            Instructionˉlimitˉcode.AddRange(I32ˉinstruction(0));
            Instructionˉlimitˉcode.Add((byte)Opcode.Pop);
        }
        Instructionˉlimitˉcode.AddRange(I32ˉinstruction(0));
        Instructionˉlimitˉcode.Add((byte)Opcode.Return);
        var Instructionˉlimitˉresult = Runˉwebassemblyˉtool(
            Tool,
            Moduleˉcodec.Write(Buildˉmodule(
                Instructionˉlimitˉcode.ToImmutable(),
                Valueˉtype.I32,
                maximumˉstack: 1)));
        Equal(1, Instructionˉlimitˉresult.Exitˉcode);
        Equal(
            "webassembly status=Unsupportedˉcode\n",
            Instructionˉlimitˉresult.Diagnostics);
        Equal(0, Instructionˉlimitˉresult.Writeˉcount);

        var Invalidˉlocal = Compileˉsuccess(WEBASSEMBLY_STRAIGHT_I32_SOURCE);
        var Invalidˉlocalˉverified = Moduleˉcodec.Readˉandˉverify(Invalidˉlocal);
        var Localˉinstruction = Invalidˉlocalˉverified.Module.Code.IndexOf(
            (byte)Opcode.Localˉload);
        True(Localˉinstruction >= 0, "The straight-line fixture has no local load.");
        var Invalidˉlocalˉcode = Findˉsectionˉpayload(
            Invalidˉlocal,
            Sectionˉkind.Code);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Invalidˉlocal.AsSpan(Invalidˉlocalˉcode + Localˉinstruction + 1, 4),
            uint.MaxValue);
        var Invalidˉlocalˉresult = Runˉwebassemblyˉtool(Tool, Invalidˉlocal);
        Equal(1, Invalidˉlocalˉresult.Exitˉcode);
        Equal(
            "webassembly status=Unsupportedˉcode\n",
            Invalidˉlocalˉresult.Diagnostics);
        Equal(0, Invalidˉlocalˉresult.Writeˉcount);

        var Inconsistentˉstack = Moduleˉcodec.Write(Buildˉmodule(
            [
                .. I32ˉinstruction(40),
                .. I32ˉinstruction(2),
                (byte)Opcode.I32ˉadd,
                (byte)Opcode.Return,
            ],
            Valueˉtype.I32,
            maximumˉstack: 2));
        var Inconsistentˉstackˉfunction = Findˉsectionˉpayload(
            Inconsistentˉstack,
            Sectionˉkind.Functions);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Inconsistentˉstack.AsSpan(Inconsistentˉstackˉfunction + 29, 4),
            1);
        var Inconsistentˉstackˉresult = Runˉwebassemblyˉtool(
            Tool,
            Inconsistentˉstack);
        Equal(1, Inconsistentˉstackˉresult.Exitˉcode);
        Equal(
            "webassembly status=Unsupportedˉcode\n",
            Inconsistentˉstackˉresult.Diagnostics);
        Equal(0, Inconsistentˉstackˉresult.Writeˉcount);

        var Oversizedˉcode = ImmutableArray.CreateBuilder<byte>();
        for (var Index = 0; Index < 2_731; Index++)
        {
            Oversizedˉcode.AddRange(I32ˉinstruction(0));
            Oversizedˉcode.Add((byte)Opcode.Pop);
        }
        Oversizedˉcode.AddRange(I32ˉinstruction(0));
        Oversizedˉcode.Add((byte)Opcode.Return);
        var Oversizedˉcodeˉresult = Runˉwebassemblyˉtool(
            Tool,
            Moduleˉcodec.Write(Buildˉmodule(
                Oversizedˉcode.ToImmutable(),
                Valueˉtype.I32,
                maximumˉstack: 1)));
        Equal(1, Oversizedˉcodeˉresult.Exitˉcode);
        Equal(
            "webassembly status=Unsupportedˉfunction\n",
            Oversizedˉcodeˉresult.Diagnostics);
        Equal(0, Oversizedˉcodeˉresult.Writeˉcount);

        var Unsupportedˉarithmetic = Checkedˉaddˉwvb.ToArray();
        var Unsupportedˉarithmeticˉcode = Findˉsectionˉpayload(
            Unsupportedˉarithmetic,
            Sectionˉkind.Code);
        Unsupportedˉarithmetic[Unsupportedˉarithmeticˉcode + 30] =
            (byte)Opcode.I32ˉequal;
        var Unsupportedˉarithmeticˉresult = Runˉwebassemblyˉtool(
            Tool,
            Unsupportedˉarithmetic);
        Equal(1, Unsupportedˉarithmeticˉresult.Exitˉcode);
        Equal(
            "webassembly status=Unsupportedˉcode\n",
            Unsupportedˉarithmeticˉresult.Diagnostics);
        Equal(0, Unsupportedˉarithmeticˉresult.Writeˉcount);

        var Inconsistentˉadd = Checkedˉaddˉwvb.ToArray();
        var Inconsistentˉaddˉfunction = Findˉsectionˉpayload(
            Inconsistentˉadd,
            Sectionˉkind.Functions);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Inconsistentˉadd.AsSpan(Inconsistentˉaddˉfunction + 17, 4),
            2);
        var Inconsistentˉaddˉresult = Runˉwebassemblyˉtool(Tool, Inconsistentˉadd);
        Equal(1, Inconsistentˉaddˉresult.Exitˉcode);
        Equal(
            "webassembly status=Unsupportedˉfunction\n",
            Inconsistentˉaddˉresult.Diagnostics);
        Equal(0, Inconsistentˉaddˉresult.Writeˉcount);

        Throwsˉinvalidˉdata(() => Executeˉcheckedˉaddˉwebassembly(
            Checkedˉlowered.Writtenˉbytes.AsSpan()[..^1]));
        var Mutableˉabi = Checkedˉlowered.Writtenˉbytes.ToArray();
        Mutableˉabi[23] = 1;
        Throwsˉinvalidˉdata(() => Executeˉcheckedˉaddˉwebassembly(Mutableˉabi));

        var Truncated = Constantˉwvb[..^1];
        var Truncatedˉresult = Runˉwebassemblyˉtool(Tool, Truncated);
        Equal(1, Truncatedˉresult.Exitˉcode);
        Equal("webassembly status=Invalidˉwvb\n", Truncatedˉresult.Diagnostics);
        Equal(0, Truncatedˉresult.Writeˉcount);

        var Oversizedˉsection = Constantˉwvb.ToArray();
        BinaryPrimitives.WriteUInt32LittleEndian(Oversizedˉsection.AsSpan(16, 4), uint.MaxValue);
        var Oversizedˉsectionˉresult = Runˉwebassemblyˉtool(Tool, Oversizedˉsection);
        Equal(1, Oversizedˉsectionˉresult.Exitˉcode);
        Equal("webassembly status=Invalidˉwvb\n", Oversizedˉsectionˉresult.Diagnostics);
        Equal(0, Oversizedˉsectionˉresult.Writeˉcount);

        var Hosted = Constantˉwvb.ToArray();
        Hosted[20] = 2;
        var Hostedˉresult = Runˉwebassemblyˉtool(Tool, Hosted);
        Equal(1, Hostedˉresult.Exitˉcode);
        Equal("webassembly status=Unsupportedˉprofile\n", Hostedˉresult.Diagnostics);
        Equal(0, Hostedˉresult.Writeˉcount);

        var Unsupportedˉwvb = Compileˉsuccess("""
            module WebAssemblyˉunsupported profile portable;

            export fn Main() -> i32 {
                if 40 == 2 { return 0; }
                return 42;
            }
            """);
        var Unsupported = Runˉwebassemblyˉtool(Tool, Unsupportedˉwvb);
        Equal(1, Unsupported.Exitˉcode);
        Contains(Unsupported.Diagnostics, "webassembly status=Unsupportedˉ");
        Equal(0, Unsupported.Writeˉcount);

        var Truncatedˉwasm = First.Writtenˉbytes[..^1].ToArray();
        Throwsˉinvalidˉdata(() => Executeˉconstantˉwebassembly(Truncatedˉwasm));
        var Inconsistentˉwasm = First.Writtenˉbytes.ToArray();
        Inconsistentˉwasm[9] = 6;
        Throwsˉinvalidˉdata(() => Executeˉconstantˉwebassembly(Inconsistentˉwasm));
        var Invalidˉopcode = First.Writtenˉbytes.ToArray();
        Invalidˉopcode[^3] = 0x42;
        Throwsˉinvalidˉdata(() => Executeˉconstantˉwebassembly(Invalidˉopcode));
    }

    private static void Moduleˉroundˉtrip()
    {
        var Bytes = Compileˉsuccess(SUM_SOURCE);
        var Parsed = Moduleˉcodec.Read(Bytes);
        var Rewritten = Moduleˉcodec.Write(Parsed);
        Sequenceˉequal(Bytes, Rewritten);
    }

    private static void Inspectorˉisˉuseful()
    {
        var Bytes = Compileˉsuccess(SUM_SOURCE);
        var Inspection = Moduleˉinspector.Inspect(Moduleˉcodec.Readˉandˉverify(Bytes), Bytes);
        Contains(Inspection, "Module: Sumˉdata");
        Contains(Inspection, "Data (1)");
        Contains(Inspection, "data.load.i32");
        Contains(Inspection, "call function[0] (Add)");
        Contains(Inspection, $"SHA-256: {SUM_SHA256}");

        var Unicodeˉsource = $$"""
            module Unicodeˉpreview profile portable;
            data Message: text = "{{new string('a', 79)}}😀";
            export fn Main() -> i32 { return 0; }
            """;
        var Unicodeˉbytes = Compileˉsuccess(Unicodeˉsource);
        var Unicodeˉinspection = Moduleˉinspector.Inspect(
            Moduleˉcodec.Readˉandˉverify(Unicodeˉbytes),
            Unicodeˉbytes);
        Contains(Unicodeˉinspection, "\\uD83D\\uDE00");
        False(
            Unicodeˉinspection.Contains("\\uFFFD", StringComparison.OrdinalIgnoreCase),
            "The inspector split a Unicode scalar while creating its preview.");
    }

    private static void Additionalˉsemanticsˉrun()
    {
        const string Source = """
            module Conditions profile hosted;
            capability console.write_line;
            fn Isˉanswer(Value: i32) -> bool { return !(Value != 42); }
            export fn Main() -> i32 {
                if Isˉanswer(6 * 7) {
                    console.write_line("answer");
                    return 42;
                } else {
                    return 1;
                }
            }
            """;
        var Module = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(Source));
        var Output = new StringWriter();
        var Runtime = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(Output),
            new(ImmutableHashSet.Create(StringComparer.Ordinal, Capabilityˉcatalog.CONSOLE_WRITE_LINE)));
        Equal(42, Runtime.Runˉmain().Exitˉcode);
        Equal("answer\n", Output.ToString());
    }

    private static void Namingˉandˉmutabilityˉrun()
    {
        const string Source = """
            module Namingˉandˉmutability profile portable;
            fn Addˉone(Value: i32) -> i32 { return Value + 1; }
            export fn Main() -> i32 {
                let Baseˉvalue: i32 = 40;
                var Resultˉvalue: i32 = Baseˉvalue;
                Resultˉvalue = Addˉone(Resultˉvalue);
                return Resultˉvalue;
            }
            """;
        Equal(41, Runˉportable(Source));

        const string Immutableˉassignment = """
            module Immutableˉassignment profile portable;
            export fn Main() -> i32 {
                let Value: i32 = 1;
                Value = 2;
                return Value;
            }
            """;
        Hasˉdiagnostic(Immutableˉassignment, "WVC2042");

        const string Parameterˉassignment = """
            module Parameterˉassignment profile portable;
            fn Change(Value: i32) -> i32 {
                Value = 2;
                return Value;
            }
            export fn Main() -> i32 { return Change(1); }
            """;
        Hasˉdiagnostic(Parameterˉassignment, "WVC2042");

        const string Malformedˉseparator = """
            module Badˉˉname profile portable;
            export fn Main() -> i32 { return 0; }
            """;
        Hasˉdiagnostic(Malformedˉseparator, "WVC2004");

        const string Confusableˉseparator = """
            module Bad¯name profile portable;
            export fn Main() -> i32 { return 0; }
            """;
        Hasˉdiagnostic(Confusableˉseparator, "WVC1002");

        const string Unknownˉrecord = """
            module Broken profile portable;
            export fn Main(Value: Missing) -> i32 { return 0; }
            """;
        Hasˉdiagnostic(Unknownˉrecord, "WVC2085");

        const string Duplicateˉrecordˉfield = """
            module Broken profile portable;
            record Pair { Value: i32; Value: u32; }
            export fn Main() -> i32 { return 0; }
            """;
        Hasˉdiagnostic(Duplicateˉrecordˉfield, "WVC2082");

        const string Emptyˉrecord = """
            module Broken profile portable;
            record Empty { }
            export fn Main() -> i32 { return 0; }
            """;
        Hasˉdiagnostic(Emptyˉrecord, "WVC2084");

        const string Nestedˉrecord = """
            module Broken profile portable;
            record Inner { Value: i32; }
            record Outer { Value: Inner; }
            export fn Main() -> i32 { return 0; }
            """;
        Hasˉdiagnostic(Nestedˉrecord, "WVC2083");

        const string Wrongˉconstructorˉtype = """
            module Broken profile portable;
            record Pair { Value: i32; }
            export fn Main() -> i32 { Pair(1u32); return 0; }
            """;
        Hasˉdiagnostic(Wrongˉconstructorˉtype, "WVC2070");

        const string Missingˉfield = """
            module Broken profile portable;
            record Pair { Value: i32; }
            export fn Main() -> i32 {
                let Pairˉvalue: Pair = Pair(1);
                return Pairˉvalue.Missing;
            }
            """;
        Hasˉdiagnostic(Missingˉfield, "WVC2087");

        const string Constructorˉnameˉconflict = """
            module Broken profile portable;
            record Pair { Value: i32; }
            fn Pair(Value: i32) -> i32 { return Value; }
            export fn Main() -> i32 { return 0; }
            """;
        Hasˉdiagnostic(Constructorˉnameˉconflict, "WVC2025");

        const string Nominalˉmismatch = """
            module Broken profile portable;
            record Left { Value: i32; }
            record Right { Value: i32; }
            export fn Main() -> i32 {
                let Value: Left = Right(1);
                return 0;
            }
            """;
        Hasˉdiagnostic(Nominalˉmismatch, "WVC2070");

        const string Duplicateˉenumˉmember = """
            module Broken profile portable;
            enum State { Ready = 0; Ready = 1; }
            export fn Main() -> i32 { return 0; }
            """;
        Hasˉdiagnostic(Duplicateˉenumˉmember, "WVC2093");

        const string Duplicateˉenumˉvalue = """
            module Broken profile portable;
            enum State { Ready = 0; Failed = 0; }
            export fn Main() -> i32 { return 0; }
            """;
        Hasˉdiagnostic(Duplicateˉenumˉvalue, "WVC2094");

        const string Emptyˉenum = """
            module Broken profile portable;
            enum State { }
            export fn Main() -> i32 { return 0; }
            """;
        Hasˉdiagnostic(Emptyˉenum, "WVC2095");

        const string Unsignedˉenumˉvalue = """
            module Broken profile portable;
            enum State { Ready = 0u32; }
            export fn Main() -> i32 { return 0; }
            """;
        Hasˉdiagnostic(Unsignedˉenumˉvalue, "WVC2099");

        const string Missingˉenumˉmember = """
            module Broken profile portable;
            enum State { Ready = 0; }
            export fn Main() -> i32 { State.Missing; return 0; }
            """;
        Hasˉdiagnostic(Missingˉenumˉmember, "WVC2097");

        const string Nameˉnonˉenum = """
            module Broken profile portable;
            export fn Main() -> i32 { Enumˉname(1); return 0; }
            """;
        Hasˉdiagnostic(Nameˉnonˉenum, "WVC2098");

        const string Enumˉnominalˉmismatch = """
            module Broken profile portable;
            enum Left { Value = 0; }
            enum Right { Value = 0; }
            export fn Main() -> i32 {
                let Value: Left = Right.Value;
                return 0;
            }
            """;
        Hasˉdiagnostic(Enumˉnominalˉmismatch, "WVC2070");
    }

    private static void Foundationˉbytesˉrun()
    {
        var Bytes = Compileˉsuccess(FOUNDATION_SOURCE);
        var Module = Moduleˉcodec.Readˉandˉverify(Bytes);
        Equal(Dataˉtype.Bytes, Module.Module.Data.Single().Type);
        var Data = (Bytesˉdataˉdeclaration)Module.Module.Data.Single();
        Sequenceˉequal<byte>([87, 86, 66, 49, 1, 0, 6, 0, 7, 0, 0, 0], Data.Values);
        True(
            Module.Module.Functions.SelectMany(Function => Function.Allˉlocalˉtypes)
                .Contains(Valueˉtype.Bytes),
            "The Foundation module did not preserve its bytes value type.");
        True(
            Module.Module.Functions.SelectMany(Function => Function.Allˉlocalˉtypes)
                .Contains(Valueˉtype.U8),
            "The Foundation module did not preserve its u8 value type.");
        True(
            Module.Module.Functions.SelectMany(Function => Function.Allˉlocalˉtypes)
                .Contains(Valueˉtype.U32),
            "The Foundation module did not preserve its u32 value type.");

        var Rewritten = Moduleˉcodec.Write(Module.Module);
        Sequenceˉequal(Bytes, Rewritten);
        var Inspection = Moduleˉinspector.Inspect(Module, Bytes);
        Contains(Inspection, "bytes.read_u32_little");
        Contains(Inspection, "bytes.slice");
        Equal(FOUNDATION_SHA256, Moduleˉdigest.Calculateˉsha256(Bytes));
        Equal(1, new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new StringWriter()),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain().Exitˉcode);
    }

    private static void Foundationˉtextˉrun()
    {
        const string Source = """
            module Foundationˉtext profile hosted;

            capability console.write_line;

            data Encoded: bytes = [
                87, 105, 110, 100, 118, 97, 108, 101, 32,
                226, 152, 131,
                240, 159, 152, 128
            ];
            data Invalid: bytes = [195, 40];
            data Signed: bytes = [249, 255, 255, 255];
            data Escaped: bytes = [34, 92, 10, 9];

            export fn Main() -> i32 {
                if U32ˉfromˉu8(Bytesˉreadˉu8(Encoded, 0u32)) != 87u32 {
                    return 3;
                }
                if !Textˉutf8ˉisˉvalid(Encoded) {
                    return 1;
                }
                if Textˉutf8ˉisˉvalid(Invalid) {
                    return 2;
                }

                console.write_line(Textˉquote(Textˉfromˉutf8(Encoded)));
                console.write_line(Textˉquote(Textˉfromˉutf8(Escaped)));
                return Bytesˉreadˉi32ˉlittle(Signed, 0u32) + 7;
            }
            """;

        var Bytes = Compileˉsuccess(Source);
        var Module = Moduleˉcodec.Readˉandˉverify(Bytes);
        var Inspection = Moduleˉinspector.Inspect(Module, Bytes);
        Contains(Inspection, "bytes.read_i32_little");
        Contains(Inspection, "text.utf8_is_valid");
        Contains(Inspection, "text.from_utf8");
        Contains(Inspection, "text.quote");
        Contains(Inspection, "u32.from_u8");
        var Output = new StringWriter();
        var Result = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(Output),
            new(ImmutableHashSet.Create(
                StringComparer.Ordinal,
                Capabilityˉcatalog.CONSOLE_WRITE_LINE))).Runˉmain();
        Equal(0, Result.Exitˉcode);
        Equal("\"Windvale \\u2603\\uD83D\\uDE00\"\n\"\\\"\\\\\\n\\t\"\n", Output.ToString());

        const string Invalidˉdecode = """
            module Invalidˉutf8 profile portable;
            data Invalid: bytes = [195, 40];
            export fn Main() -> i32 {
                Textˉfromˉutf8(Invalid);
                return 0;
            }
            """;
        Throwsˉruntime("WVR3014", () => Runˉportable(Invalidˉdecode));
    }

    private static void Foundationˉbyteˉconstructionˉrun()
    {
        const string Source = """
            module Foundationˉbyteˉconstruction profile portable;

            data Expectedˉdigest: bytes = [
                48, 53, 101, 101, 51, 101, 101, 99, 102, 97, 98, 55, 55, 49, 99, 57,
                53, 100, 55, 56, 51, 102, 53, 48, 48, 100, 50, 55, 101, 101, 101, 52,
                101, 53, 52, 99, 100, 53, 49, 56, 57, 100, 99, 52, 54, 57, 56, 101,
                99, 52, 97, 54, 55, 102, 100, 51, 99, 100, 101, 57, 55, 97, 52, 98
            ];

            export fn Main() -> i32 {
                var Encoded: bytes = Bytesˉfromˉu8(171u8);
                Encoded = Bytesˉconcat(Encoded, Bytesˉfromˉu16ˉlittle(4660u32));
                Encoded = Bytesˉconcat(Encoded, Bytesˉfromˉu32ˉlittle(2309737967u32));
                Encoded = Bytesˉconcat(Encoded, Bytesˉfromˉi32ˉlittle(-7));
                Encoded = Bytesˉconcat(Encoded, Textˉtoˉutf8("WVO"));
                if Bytesˉlength(Encoded) != 14u32 { return 1; }
                if Bytesˉreadˉu8(Encoded, 0u32) != 171u8 { return 2; }
                if Bytesˉreadˉu16ˉlittle(Encoded, 1u32) != 4660u32 { return 3; }
                if Bytesˉreadˉu32ˉlittle(Encoded, 3u32) != 2309737967u32 { return 4; }
                if Bytesˉreadˉi32ˉlittle(Encoded, 7u32) != -7 { return 5; }
                if Bytesˉreadˉu8(Encoded, 11u32) != 87u8 { return 6; }
                if Bytesˉreadˉu8(Encoded, 12u32) != 86u8 { return 7; }
                if Bytesˉreadˉu8(Encoded, 13u32) != 79u8 { return 8; }
                let Digest: bytes = Textˉtoˉutf8(Bytesˉsha256ˉhex(Bytesˉslice(Encoded, 11u32, 3u32)));
                if Bytesˉlength(Digest) != Bytesˉlength(Expectedˉdigest) { return 9; }
                var Digestˉoffset: u32 = 0u32;
                while Digestˉoffset < Bytesˉlength(Expectedˉdigest) {
                    if Bytesˉreadˉu8(Digest, Digestˉoffset) != Bytesˉreadˉu8(Expectedˉdigest, Digestˉoffset) { return 10; }
                    Digestˉoffset = Digestˉoffset + 1u32;
                }
                return 0;
            }
            """;

        var Bytes = Compileˉsuccess(Source);
        var Module = Moduleˉcodec.Readˉandˉverify(Bytes);
        var Inspection = Moduleˉinspector.Inspect(Module, Bytes);
        Contains(Inspection, "bytes.concat");
        Contains(Inspection, "bytes.from_u8");
        Contains(Inspection, "bytes.from_u16_little");
        Contains(Inspection, "bytes.from_u32_little");
        Contains(Inspection, "bytes.from_i32_little");
        Contains(Inspection, "bytes.sha256_hex");
        Contains(Inspection, "text.to_utf8");
        Equal(0, new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new StringWriter()),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain().Exitˉcode);

        const string U16ˉoverflow = """
            module U16ˉoverflow profile portable;
            export fn Main() -> i32 {
                Bytesˉfromˉu16ˉlittle(65536u32);
                return 0;
            }
            """;
        Throwsˉruntime("WVR3016", () => Runˉportable(U16ˉoverflow));
    }

    private static void Foundationˉbalancedˉbytesˉrun()
    {
        const string Source = """
            module Foundationˉbalancedˉbytes profile portable;

            data Empty: bytes = [];
            data Unit: bytes = [171];

            export fn Main() -> i32 {
                var Value: bytes = Empty;
                var Index: u32 = 0u32;
                while Index < 65536u32 {
                    Value = Bytesˉconcat(Value, Unit);
                    Index = Index + 1u32;
                }
                if Bytesˉlength(Value) != 65536u32 { return 1; }
                if Bytesˉreadˉu8(Value, 0u32) != 171u8 { return 2; }
                if Bytesˉreadˉu8(Value, 65535u32) != 171u8 { return 3; }

                var Patched: bytes = Bytesˉslice(Value, 0u32, 32767u32);
                Patched = Bytesˉconcat(Patched, Bytesˉfromˉu32ˉlittle(2309737967u32));
                Patched = Bytesˉconcat(Patched, Bytesˉslice(Value, 32771u32, 32765u32));
                if Bytesˉlength(Patched) != 65536u32 { return 4; }
                if Bytesˉreadˉu8(Patched, 32766u32) != 171u8 { return 5; }
                if Bytesˉreadˉu32ˉlittle(Patched, 32767u32) != 2309737967u32 { return 6; }
                if Bytesˉreadˉu8(Patched, 32771u32) != 171u8 { return 7; }
                if Bytesˉreadˉu8(Value, 32767u32) != 171u8 { return 8; }
                return 0;
            }
            """;

        var Module = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(Source));
        var Runtime = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            new(
                ImmutableHashSet.Create<string>(StringComparer.Ordinal),
                Maximumˉinstructions: 20_000_000));
        Equal(0, Runtime.Runˉmain().Exitˉcode);
    }

    private static void Wvˉdumpˉcoreˉwalksˉsections()
    {
        var Bytes = Compileˉsuccess(WVDUMP_CORE_SOURCE);
        var Module = Moduleˉcodec.Readˉandˉverify(Bytes);
        Equal("Wvˉdumpˉcore", Module.Module.Name);
        Equal(Moduleˉprofile.Hosted, Module.Module.Profile);
        Equal(10, Module.Module.Data.OfType<Bytesˉdataˉdeclaration>().Count());
        Sequenceˉequal(
            [
                Capabilityˉcatalog.CONSOLE_WRITE_LINE,
                Capabilityˉcatalog.DIAGNOSTIC_WRITE_LINE,
                Capabilityˉcatalog.FILE_READ_BYTES,
                Capabilityˉcatalog.PROCESS_ARGUMENT,
                Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT,
            ],
            Module.Module.Capabilities.Select(Capability => Capability.Name));
        Equal(5, Module.Module.Types.Length);
        Equal("Wvbˉinspection", Module.Module.Types[0].Name);
        Equal("Wvbˉpayloadˉinspection", Module.Module.Types[1].Name);
        Equal("Wvbˉscan", Module.Module.Types[2].Name);
        Equal("Wvbˉsection", Module.Module.Types[3].Name);
        Equal("Wvbˉstatus", Module.Module.Types[4].Name);
        Equal(3, ((Recordˉtypeˉdeclaration)Module.Module.Types[0]).Fields.Length);
        Equal(4, ((Recordˉtypeˉdeclaration)Module.Module.Types[1]).Fields.Length);
        Equal(4, ((Recordˉtypeˉdeclaration)Module.Module.Types[2]).Fields.Length);
        Equal(6, ((Recordˉtypeˉdeclaration)Module.Module.Types[3]).Fields.Length);
        Equal(
            Valueˉshape.Forˉenum(4),
            ((Recordˉtypeˉdeclaration)Module.Module.Types[0]).Fields[0].Type);
        Equal(19, ((Enumˉtypeˉdeclaration)Module.Module.Types[4]).Members.Length);

        var Inspectˉfunction = Module.Module.Functions.Single(
            Function => Function.Name == "Inspectˉwvbˉenvelope");
        Equal(Valueˉshape.Forˉrecord(0), Inspectˉfunction.Returnˉtype);

        var Validˉdata = (Bytesˉdataˉdeclaration)Module.Module.Data.Single(
            Data => Data.Name == "Validˉmodule");
        var Embeddedˉmodule = Moduleˉcodec.Readˉandˉverify(Validˉdata.Values.AsSpan());
        Equal("A", Embeddedˉmodule.Module.Name);
        Equal(Moduleˉprofile.Portable, Embeddedˉmodule.Module.Profile);
        Equal(0, Embeddedˉmodule.Module.Functions.Length);

        var Hostileˉlength = (Bytesˉdataˉdeclaration)Module.Module.Data.Single(
            Data => Data.Name == "Hostileˉlengthˉmodule");
        Sequenceˉequal<byte>([255, 255, 255, 255], Hostileˉlength.Values.TakeLast(4));

        var Inspection = Moduleˉinspector.Inspect(Module, Bytes);
        Contains(Inspection, "Inspectˉwvbˉenvelope");
        Contains(Inspection, "bytes.read_u32_little");
        Contains(Inspection, "u32.less_equal");
        Contains(Inspection, "Nominal types (5)");
        Contains(Inspection, "record.create");
        Contains(Inspection, "record.field");
        Contains(Inspection, "enum Wvbˉstatus");
        Contains(Inspection, "enum.const");
        Contains(Inspection, "enum.name");
        Contains(Inspection, "u32.format");
        Contains(Inspection, "text.concat");
        Contains(Inspection, "bytes.read_i32_little");
        Contains(Inspection, "text.utf8_is_valid");
        Contains(Inspection, "text.from_utf8");
        Contains(Inspection, "text.quote");
        Contains(Inspection, "u32.from_u8");
        Equal(WVDUMP_CORE_SHA256, Moduleˉdigest.Calculateˉsha256(Bytes));
        var Authorized = Module.Module.Capabilities
            .Select(Capability => Capability.Name)
            .ToImmutableHashSet(StringComparer.Ordinal);
        Equal(0, new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                [],
                TextWriter.Null,
                TextWriter.Null,
                new Testˉfileˉreader((_, _) => throw new InvalidOperationException(
                    "The no-argument WvDump self-test must not read a hosted file.")))),
            new(Authorized)).Runˉmain().Exitˉcode);

        var Hostedˉoutput = new StringWriter();
        var Hostedˉdiagnostics = new StringWriter();
        var Hostedˉrun = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                ["real.wvb"],
                Hostedˉoutput,
                Hostedˉdiagnostics,
                new Testˉfileˉreader((Name, Maximumˉbytes) =>
                {
                    Equal("real.wvb", Name);
                    True(Validˉdata.Values.Length <= Maximumˉbytes, "The hosted byte limit was too small.");
                    return Validˉdata.Values;
                }))),
            new(Authorized)).Runˉmain();
        Equal(0, Hostedˉrun.Exitˉcode);
        Equal(
            """
            wvdump 1
            module version=1.6 profile=portable name="A"
            section name=module offset=20 bytes=6 count=1
            section name=capabilities offset=34 bytes=4 count=0
            section name=data offset=46 bytes=4 count=0
            section name=functions offset=58 bytes=4 count=0
            section name=code offset=70 bytes=0 count=0
            section name=exports offset=78 bytes=4 count=0
            section name=types offset=90 bytes=4 count=0
            """.Replace("\r\n", "\n", StringComparison.Ordinal) + "\n",
            Hostedˉoutput.ToString());
        Equal(string.Empty, Hostedˉdiagnostics.ToString());

        const string Hashˉsource = """
            module Hashˉinspection profile portable;
            data Value: bytes = [1, 2, 3];
            export fn Main() -> i32 {
                Bytesˉsha256ˉhex(Value);
                return 0;
            }
            """;
        var Hashˉmoduleˉbytes = Compileˉsuccess(Hashˉsource);
        var Hashˉoutput = new StringWriter();
        Equal(0, new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                ["hash.wvb"],
                Hashˉoutput,
                TextWriter.Null,
                new Testˉfileˉreader((_, _) => Hashˉmoduleˉbytes.ToImmutableArray()))),
            new(Authorized)).Runˉmain().Exitˉcode);
        Contains(Hashˉoutput.ToString(), "opcode=bytes.sha256_hex");

        var Malformedˉpayload = Validˉdata.Values.ToArray();
        var Dataˉpayload = Findˉsectionˉpayload(Malformedˉpayload, Sectionˉkind.Data);
        BinaryPrimitives.WriteUInt32LittleEndian(Malformedˉpayload.AsSpan(Dataˉpayload), 1u);
        var Malformedˉpayloadˉoutput = new StringWriter();
        var Malformedˉpayloadˉdiagnostics = new StringWriter();
        var Malformedˉpayloadˉrun = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                ["bad-payload.wvb"],
                Malformedˉpayloadˉoutput,
                Malformedˉpayloadˉdiagnostics,
                new Testˉfileˉreader((_, _) => Malformedˉpayload.ToImmutableArray()))),
            new(Authorized)).Runˉmain();
        Equal(2, Malformedˉpayloadˉrun.Exitˉcode);
        Equal(string.Empty, Malformedˉpayloadˉoutput.ToString());
        Equal(
            $"Outˉofˉbounds declarations=1 instructions=0 offset={Dataˉpayload + sizeof(uint)}\n",
            Malformedˉpayloadˉdiagnostics.ToString());

        var Invalidˉutf8 = Validˉdata.Values.ToArray();
        var Moduleˉpayload = Findˉsectionˉpayload(Invalidˉutf8, Sectionˉkind.Module);
        Invalidˉutf8[Moduleˉpayload + 5] = byte.MaxValue;
        var Invalidˉutf8ˉoutput = new StringWriter();
        var Invalidˉutf8ˉdiagnostics = new StringWriter();
        var Invalidˉutf8ˉrun = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                ["bad-utf8.wvb"],
                Invalidˉutf8ˉoutput,
                Invalidˉutf8ˉdiagnostics,
                new Testˉfileˉreader((_, _) => Invalidˉutf8.ToImmutableArray()))),
            new(Authorized)).Runˉmain();
        Equal(2, Invalidˉutf8ˉrun.Exitˉcode);
        Equal(string.Empty, Invalidˉutf8ˉoutput.ToString());
        Equal(
            $"Invalidˉutf8 declarations=0 instructions=0 offset={Moduleˉpayload + 5}\n",
            Invalidˉutf8ˉdiagnostics.ToString());

        var Malformedˉopcode = Compileˉsuccess(SUM_SOURCE);
        var Codeˉpayload = Findˉsectionˉpayload(Malformedˉopcode, Sectionˉkind.Code);
        Malformedˉopcode[Codeˉpayload] = byte.MaxValue;
        var Malformedˉopcodeˉoutput = new StringWriter();
        var Malformedˉopcodeˉdiagnostics = new StringWriter();
        var Malformedˉopcodeˉrun = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                ["bad-opcode.wvb"],
                Malformedˉopcodeˉoutput,
                Malformedˉopcodeˉdiagnostics,
                new Testˉfileˉreader((_, _) => Malformedˉopcode.ToImmutableArray()))),
            new(Authorized)).Runˉmain();
        Equal(2, Malformedˉopcodeˉrun.Exitˉcode);
        Equal(string.Empty, Malformedˉopcodeˉoutput.ToString());
        Equal(
            $"Unknownˉopcode declarations=2 instructions=0 offset={Codeˉpayload}\n",
            Malformedˉopcodeˉdiagnostics.ToString());

        var Invalidˉoutput = new StringWriter();
        var Invalidˉdiagnostics = new StringWriter();
        var Invalidˉrun = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                ["bad.wvb"],
                Invalidˉoutput,
                Invalidˉdiagnostics,
                new Testˉfileˉreader((_, _) => Hostileˉlength.Values))),
            new(Authorized)).Runˉmain();
        Equal(2, Invalidˉrun.Exitˉcode);
        Equal(string.Empty, Invalidˉoutput.ToString());
        Equal("Outˉofˉbounds sections=0 offset=20\n", Invalidˉdiagnostics.ToString());
    }

    private static void Objectˉmodelˉroundˉtrip()
    {
        var Value = Buildˉsampleˉobject();
        var Bytes = Objectˉcodec.Write(Value);
        Equal(189, Bytes.Length);
        Equal(WVO_SAMPLE_SHA256, Objectˉdigest.Calculateˉsha256(Bytes));

        var Verified = Objectˉcodec.Readˉandˉverify(Bytes);
        Sequenceˉequal(Bytes, Objectˉcodec.Write(Verified.Value));
        Equal(Objectˉarchitecture.X86ˉ64, Verified.Value.Architecture);
        Equal(2, Verified.Value.Sections.Length);
        Equal(".text", Verified.Value.Sections[0].Name);
        Equal(Objectˉsectionˉkind.Readˉonlyˉdata, Verified.Value.Sections[1].Kind);
        Equal(3, Verified.Value.Symbols.Length);
        Equal(Objectˉlimits.UNDEFINED_SECTION, Verified.Value.Symbols[2].Sectionˉindex);
        Equal(Objectˉrelocationˉkind.Relativeˉi32, Verified.Value.Relocations.Single().Kind);
        Equal(-4, Verified.Value.Relocations.Single().Addend);
        var Inspection = Objectˉinspector.Inspect(Verified, Bytes);
        Contains(Inspection, "Sections (2)");
        Contains(Inspection, "Console_write binding=Import");
        Contains(Inspection, "kind=Relativeˉi32 section=0 offset=1 symbol=2 addend=-4");

        var Badˉmagic = Bytes.ToArray();
        Badˉmagic[0] = 0;
        Throwsˉobject("WVO1002", () => Objectˉcodec.Readˉandˉverify(Badˉmagic));

        var Badˉversion = Bytes.ToArray();
        Badˉversion[6] = 1;
        Throwsˉobject("WVO1003", () => Objectˉcodec.Readˉandˉverify(Badˉversion));

        var Badˉcount = Bytes.ToArray();
        BinaryPrimitives.WriteUInt32LittleEndian(Badˉcount.AsSpan(12), uint.MaxValue);
        Throwsˉobject("WVO1013", () => Objectˉcodec.Readˉandˉverify(Badˉcount));

        var Badˉsectionˉkind = Bytes.ToArray();
        Badˉsectionˉkind[24] = byte.MaxValue;
        Throwsˉobject("WVO1007", () => Objectˉcodec.Readˉandˉverify(Badˉsectionˉkind));

        var Badˉutf8 = Bytes.ToArray();
        Badˉutf8[44] = byte.MaxValue;
        Throwsˉobject("WVO1014", () => Objectˉcodec.Readˉandˉverify(Badˉutf8));
        Throwsˉobject("WVO1016", () => Objectˉcodec.Readˉandˉverify(Bytes.AsSpan(0, Bytes.Length - 1)));
        Throwsˉobject("WVO1015", () => Objectˉcodec.Readˉandˉverify([.. Bytes, (byte)0]));

        var Noncanonicalˉsections = Value with
        {
            Sections = [Value.Sections[1], Value.Sections[0]],
        };
        Throwsˉobject("WVO2012", () => Objectˉverifier.Verify(Noncanonicalˉsections));

        var Badˉsymbol = Value with
        {
            Symbols =
            [
                Value.Symbols[0] with { Offset = 4 },
                Value.Symbols[1],
                Value.Symbols[2],
            ],
        };
        Throwsˉobject("WVO2025", () => Objectˉverifier.Verify(Badˉsymbol));

        var Badˉplaceholder = Value with
        {
            Sections =
            [
                Value.Sections[0] with { Data = [232, 1, 0, 0, 0, 195] },
                Value.Sections[1],
            ],
        };
        Throwsˉobject("WVO2035", () => Objectˉverifier.Verify(Badˉplaceholder));

        var Overlappingˉrelocations = Value with
        {
            Relocations =
            [
                Value.Relocations[0],
                Value.Relocations[0] with { Offset = 2 },
            ],
        };
        Throwsˉobject("WVO2033", () => Objectˉverifier.Verify(Overlappingˉrelocations));
    }

    private static void Wvoˉobjectˉcoreˉmatchesˉoracle()
    {
        var Moduleˉbytes = Compileˉwithˉbyteˉorderingˉsuccess(
            WVO_CORE_SOURCE,
            "Wvo-Object-Core.wv");
        Equal(WVO_CORE_SHA256, Moduleˉdigest.Calculateˉsha256(Moduleˉbytes));
        var Module = Moduleˉcodec.Readˉandˉverify(Moduleˉbytes);
        Equal("Wvoˉobjectˉcore", Module.Module.Name);
        Sequenceˉequal(
            [
                Capabilityˉcatalog.CONSOLE_WRITE_LINE,
                Capabilityˉcatalog.DIAGNOSTIC_WRITE_LINE,
                Capabilityˉcatalog.FILE_WRITE_BYTES,
                Capabilityˉcatalog.PROCESS_ARGUMENT,
                Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT,
            ],
            Module.Module.Capabilities.Select(Capability => Capability.Name));
        var Moduleˉinspection = Moduleˉinspector.Inspect(Module, Moduleˉbytes);
        Contains(Moduleˉinspection, "bytes.concat");
        Contains(Moduleˉinspection, "bytes.from_u16_little");
        Contains(Moduleˉinspection, "bytes.from_i32_little");
        Contains(Moduleˉinspection, "text.to_utf8");
        Contains(Moduleˉinspection, "call.capability capability[2] (file.write_bytes)");

        var Authorized = Module.Module.Capabilities
            .Select(Capability => Capability.Name)
            .ToImmutableHashSet(StringComparer.Ordinal);
        var Selfˉtestˉwriter = new Capturingˉfileˉwriter();
        var Selfˉtestˉresult = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                [],
                TextWriter.Null,
                TextWriter.Null,
                null,
                Selfˉtestˉwriter)),
            new(Authorized, Maximumˉinstructions: 10_000_000)).Runˉmain();
        Equal(0, Selfˉtestˉresult.Exitˉcode);
        Equal(0, Selfˉtestˉwriter.Writeˉcount);

        var Hostedˉwriter = new Capturingˉfileˉwriter();
        var Hostedˉoutput = new StringWriter();
        var Hostedˉdiagnostics = new StringWriter();
        var Hostedˉresult = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                ["sample.wvo"],
                Hostedˉoutput,
                Hostedˉdiagnostics,
                null,
                Hostedˉwriter)),
            new(Authorized, Maximumˉinstructions: 10_000_000)).Runˉmain();
        Equal(0, Hostedˉresult.Exitˉcode);
        Equal("Wrote WVO 1.0 bytes=189\n", Hostedˉoutput.ToString());
        Equal(string.Empty, Hostedˉdiagnostics.ToString());
        Equal(1, Hostedˉwriter.Writeˉcount);
        Equal("sample.wvo", Hostedˉwriter.Resourceˉname);
        var Oracleˉbytes = Objectˉcodec.Write(Buildˉsampleˉobject());
        Sequenceˉequal(Oracleˉbytes, Hostedˉwriter.Bytes);
        Equal(WVO_SAMPLE_SHA256, Objectˉdigest.Calculateˉsha256(Hostedˉwriter.Bytes.AsSpan()));
        _ = Objectˉcodec.Readˉandˉverify(Hostedˉwriter.Bytes.AsSpan());
    }

    private static void Assemblerˉemitsˉcanonicalˉobject()
    {
        var Bytes = Assembleˉsuccess(HELLO_ASSEMBLY_SOURCE);
        Equal(WVA_OBJECT_SHA256, Objectˉdigest.Calculateˉsha256(Bytes));
        Sequenceˉequal(Bytes, Assembleˉsuccess(HELLO_ASSEMBLY_SOURCE));
        Sequenceˉequal(
            Bytes,
            Assembleˉsuccess(HELLO_ASSEMBLY_SOURCE.Replace("\n", "\r\n", StringComparison.Ordinal)));

        var Object = Objectˉcodec.Readˉandˉverify(Bytes).Value;
        Equal(Objectˉarchitecture.X86ˉ64, Object.Architecture);
        Equal(2, Object.Sections.Length);
        Equal(".text", Object.Sections[0].Name);
        Equal(Objectˉsectionˉkind.Code, Object.Sections[0].Kind);
        Equal(16u, Object.Sections[0].Alignment);
        Sequenceˉequal<byte>(
            [0xB8, 42, 0, 0, 0, 0xE8, 0, 0, 0, 0, 0xC3],
            Object.Sections[0].Data);
        Equal(".rodata", Object.Sections[1].Name);
        Sequenceˉequal<byte>([72, 105, 10, 0, 0, 0, 0], Object.Sections[1].Data);

        Equal(3, Object.Symbols.Length);
        Equal(new Objectˉsymbol("Message", Objectˉsymbolˉbinding.Local, Objectˉsymbolˉkind.Data, 1, 0, 7), Object.Symbols[0]);
        Equal(new Objectˉsymbol("Main", Objectˉsymbolˉbinding.Export, Objectˉsymbolˉkind.Function, 0, 0, 11), Object.Symbols[1]);
        Equal(Objectˉsymbolˉbinding.Import, Object.Symbols[2].Binding);
        Equal("Console_write", Object.Symbols[2].Name);
        Equal(2, Object.Relocations.Length);
        Equal(new Objectˉrelocation(Objectˉrelocationˉkind.Relativeˉi32, 0, 6, 2, -4), Object.Relocations[0]);
        Equal(new Objectˉrelocation(Objectˉrelocationˉkind.Absoluteˉu32, 1, 3, 1, 0), Object.Relocations[1]);
        Sequenceˉequal(Bytes, Objectˉcodec.Write(Object));

        var Complete = Objectˉcodec.Readˉandˉverify(Assembleˉsuccess(COMPLETE_ASSEMBLY_SOURCE)).Value;
        Equal(3, Complete.Sections.Length);
        Equal(18u, Complete.Sections[0].Memoryˉsize);
        Equal(14u, Complete.Sections[1].Memoryˉsize);
        Equal(Objectˉsectionˉkind.Zeroˉfill, Complete.Sections[2].Kind);
        Equal(16u, Complete.Sections[2].Memoryˉsize);
        Equal(0, Complete.Sections[2].Data.Length);
        Equal(2, Complete.Relocations.Length);
        Equal(Objectˉrelocationˉkind.Relativeˉi32, Complete.Relocations[0].Kind);
        Equal(13u, Complete.Relocations[0].Offset);
        Equal(Objectˉrelocationˉkind.Absoluteˉu32, Complete.Relocations[1].Kind);
        Equal(10u, Complete.Relocations[1].Offset);

        var Mechanics = Objectˉcodec.Readˉandˉverify(
            Assembleˉsuccess(KERNEL_MECHANICS_ASSEMBLY_SOURCE)).Value;
        Sequenceˉequal<byte>(
            [0x68, 0xFF, 0xFF, 0xFF, 0xFF,
                0xB9, 0x80, 0x00, 0x00, 0xC0, 0x0F, 0x32, 0x0F, 0xBA, 0xE8, 0x0B,
                0x0F, 0x30, 0x0F, 0x20, 0xC0, 0x48, 0x0F, 0xBA, 0xE8, 0x10,
                0x0F, 0x22, 0xC0, 0x0F, 0x22, 0xD8, 0x0F, 0x20, 0xD8, 0x0F, 0x05,
                0xBA, 0x04, 0x06, 0x00, 0x00, 0xB8, 0x00, 0x20, 0x00, 0x00,
                0x66, 0xEF, 0xFA, 0xF4, 0xE9, 0x00, 0x00, 0x00, 0x00],
            Mechanics.Sections[0].Data);
        Equal(56u, Mechanics.Symbols[0].Size);
        Equal(
            new Objectˉrelocation(Objectˉrelocationˉkind.Relativeˉi32, 0, 52, 0, -4),
            Mechanics.Relocations.Single());
    }

    private static void Assemblerˉrejectsˉinvalidˉsource()
    {
        Hasˉassemblyˉdiagnostic("section code .text align 16", "WVA1001");
        Hasˉassemblyˉdiagnostic("""
            windvale-assembly 1
            section code .text align 16
            end section
            symbol export function Main in .text
            """, "WVA1002");
        Hasˉassemblyˉdiagnostic("""
            windvale-assembly 1
            symbol local
            """, "WVA1003");
        Hasˉassemblyˉdiagnostic("""
            windvale-assembly 1
            symbol local data Bad-name in .data
            """, "WVA1004");
        Hasˉassemblyˉdiagnostic("""
            windvale-assembly 1
            section code .text align 3
            end section
            """, "WVA1005");
        Hasˉassemblyˉdiagnostic("""
            windvale-assembly 1
            symbol export function Main in .text
            symbol local data Data in .data
            """, "WVA1006");
        Hasˉassemblyˉdiagnostic("""
            windvale-assembly 1
            symbol export function Main in .rodata
            section rodata .rodata align 1
            define Main
            bytes 1
            end define
            end section
            """, "WVA1007");
        Hasˉassemblyˉdiagnostic("""
            windvale-assembly 1
            symbol export function Main in .text
            section code .text align 16
            define Main
            bytes 1
            end define
            end section
            """, "WVA1008");
        Hasˉassemblyˉdiagnostic("""
            windvale-assembly 1
            symbol export function Main in .text
            section code .text align 16
            define Main
            call Missing
            end define
            end section
            """, "WVA1009");
        Hasˉassemblyˉdiagnostic("""
            windvale-assembly 1
            symbol export function Main in .text
            section code .text align 16
            define Main
            return
            """, "WVA1010");
        Hasˉassemblyˉdiagnostic(
            new string('a', Assemblyˉlimits.MAX_SOURCE_BYTES + 1),
            "WVA1011");
        Hasˉassemblyˉdiagnostic("""
            windvale-assembly 1
            symbol export function Main in .text
            section code .text align 16
            define Main
            out_u16 eax
            end define
            end section
            """, "WVA1003");
        Hasˉassemblyˉdiagnostic("""
            windvale-assembly 1
            symbol export function Main in .text
            section code .text align 16
            define Main
            push_i32 2147483648
            end define
            end section
            """, "WVA1005");
        Hasˉassemblyˉdiagnostic("""
            windvale-assembly 1
            symbol export function Main in .text
            section code .text align 16
            define Main
            activate_page_table rax
            end define
            end section
            """, "WVA1003");
    }

    private static void Linkerˉproducesˉcanonicalˉflatˉimage()
    {
        var Mainˉobject = Assembleˉsuccess(HELLO_ASSEMBLY_SOURCE);
        var Providerˉobject = Assembleˉsuccess(CONSOLE_PROVIDER_ASSEMBLY_SOURCE);
        var Result = Linkˉsuccess(
            [Mainˉobject, Providerˉobject],
            new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"));
        Equal(Linkˉcontract.FORMAT_VERSION, 1);
        Equal(24, Result.Imageˉbytes.Length);
        Equal(Linkˉcontract.DEFAULT_BASE_ADDRESS, Result.Entryˉaddress);
        Equal(3, Result.Sectionˉcount);
        Equal(3, Result.Definedˉsymbolˉcount);
        Equal(1, Result.Importˉcount);
        Equal(2, Result.Relocationˉcount);
        Sequenceˉequal<byte>(
            [
                0xB8, 0x2A, 0, 0, 0,
                0xE8, 6, 0, 0, 0,
                0xC3, 0, 0, 0, 0, 0,
                0xC3, 72, 105, 10,
                0, 0, 0x10, 0,
            ],
            Result.Imageˉbytes);
        Equal(LINK_IMAGE_SHA256, Objectˉdigest.Calculateˉsha256(Result.Imageˉbytes.AsSpan()));
        Equal(LINK_MAP_SHA256, Objectˉdigest.Calculateˉsha256(Result.Mapˉbytes.AsSpan()));

        var Map = System.Text.Encoding.UTF8.GetString(Result.Mapˉbytes.AsSpan());
        True(Map.EndsWith('\n'), "The canonical link map must end with LF.");
        False(Map.Contains('\r'), "The canonical link map must not contain CR.");
        Contains(Map, "target name=flat-x86-64-v1 architecture=x86-64 base-address=1048576 image-bytes=24\n");
        Contains(Map, "entry name=Main address=1048576\n");
        Contains(Map, "section index=1 input=1 source-index=0 kind=code name=.text.console image-offset=16 address=1048592");
        Contains(Map, "import index=0 input=0 source-index=2 kind=function name=Console_write provider-input=1 provider-source-index=0 address=1048592\n");
        Contains(Map, "relocation index=0 input=0 source-index=0 kind=relative-i32 patch-offset=6 patch-address=1048582 target=Console_write target-input=1 target-source-index=0 target-address=1048592 addend=-4 value=6\n");
        Contains(Map, "relocation index=1 input=0 source-index=1 kind=absolute-u32 patch-offset=20 patch-address=1048596 target=Main target-input=0 target-source-index=1 target-address=1048576 addend=0 value=1048576\n");

        var Repeated = Linkˉsuccess(
            [Mainˉobject, Providerˉobject],
            new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"));
        Sequenceˉequal(Result.Imageˉbytes, Repeated.Imageˉbytes);
        Sequenceˉequal(Result.Mapˉbytes, Repeated.Mapˉbytes);

        var Originalˉculture = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture =
                System.Globalization.CultureInfo.GetCultureInfo("fr-FR");
            var Otherˉlocale = Linkˉsuccess(
                [Mainˉobject, Providerˉobject],
                new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"));
            Sequenceˉequal(Result.Imageˉbytes, Otherˉlocale.Imageˉbytes);
            Sequenceˉequal(Result.Mapˉbytes, Otherˉlocale.Mapˉbytes);
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = Originalˉculture;
        }

        var Reversed = Linkˉsuccess(
            [Providerˉobject, Mainˉobject],
            new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"));
        False(
            Result.Imageˉbytes.SequenceEqual(Reversed.Imageˉbytes),
            "Changing semantic input order should change this image layout.");
        Contains(
            System.Text.Encoding.UTF8.GetString(Reversed.Mapˉbytes.AsSpan()),
            $"input index=0 sha256={Objectˉdigest.Calculateˉsha256(Providerˉobject)}\n");

        var Completeˉresult = Linkˉsuccess(
            [Assembleˉsuccess(COMPLETE_ASSEMBLY_SOURCE)],
            new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"));
        Equal(64, Completeˉresult.Imageˉbytes.Length);
        Equal(-17, BinaryPrimitives.ReadInt32LittleEndian(Completeˉresult.Imageˉbytes.AsSpan().Slice(13)));
        Equal(
            Linkˉcontract.DEFAULT_BASE_ADDRESS,
            BinaryPrimitives.ReadUInt32LittleEndian(Completeˉresult.Imageˉbytes.AsSpan().Slice(30)));
        True(
            Completeˉresult.Imageˉbytes.AsSpan(34, 30).SequenceEqual(new byte[30]),
            "Data-to-BSS padding and BSS bytes must be deterministic zeroes.");
        var Completeˉmap = System.Text.Encoding.UTF8.GetString(Completeˉresult.Mapˉbytes.AsSpan());
        Contains(Completeˉmap, "kind=writable-data name=.data image-offset=20 address=1048596 memory-bytes=14");
        Contains(Completeˉmap, "kind=zero-fill name=.bss image-offset=48 address=1048624 memory-bytes=16 data-bytes=0 alignment=16");

        var Unalignedˉbase = Linkˉsuccess([Providerˉobject], new(1, "Console_write"));
        Equal(16, Unalignedˉbase.Imageˉbytes.Length);
        Equal(16u, Unalignedˉbase.Entryˉaddress);
        True(
            Unalignedˉbase.Imageˉbytes.AsSpan(0, 15).SequenceEqual(new byte[15]),
            "Actual-address alignment must materialize leading zero padding.");
        Equal((byte)0xC3, Unalignedˉbase.Imageˉbytes[15]);
    }

    private static void Wvˉlinkerˉcoreˉscansˉobjects()
    {
        var Moduleˉbytes = Compileˉwithˉtoolˉfoundationˉsuccess(
            WVLINK_CORE_SOURCE,
            "Wv-Linker-Core.wv");
        var Module = Moduleˉcodec.Readˉandˉverify(Moduleˉbytes);
        Equal("Wvˉlinkerˉcore", Module.Module.Name);
        Equal(Moduleˉprofile.Hosted, Module.Module.Profile);
        Sequenceˉequal(
            [
                Capabilityˉcatalog.CONSOLE_WRITE,
                Capabilityˉcatalog.DIAGNOSTIC_WRITE_LINE,
                Capabilityˉcatalog.FILE_READ_BYTES,
                Capabilityˉcatalog.FILE_WRITE_BYTES,
                Capabilityˉcatalog.PROCESS_ARGUMENT,
                Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT,
            ],
            Module.Module.Capabilities.Select(Capability => Capability.Name));
        Equal(WVLINK_CORE_SHA256, Moduleˉdigest.Calculateˉsha256(Moduleˉbytes));

        var Inspection = Moduleˉinspector.Inspect(Module, Moduleˉbytes);
        Contains(Inspection, "Inspectˉobject");
        Contains(Inspection, "Findˉsection");
        Contains(Inspection, "Findˉsymbol");
        Contains(Inspection, "Findˉrelocation");
        Contains(Inspection, "Symbolˉrangesˉareˉdistinct");
        Contains(Inspection, "Validateˉexportˉuniqueness");
        Contains(Inspection, "Validateˉimports");
        Contains(Inspection, "Measureˉlayout");
        Contains(Inspection, "Validateˉdefinitions");
        Contains(Inspection, "Verifierˉplaceˉsection");
        Contains(Inspection, "Verifierˉbuildˉunrelocatedˉimage");
        Contains(Inspection, "Verifierˉapplyˉrelocationsˉreverse");
        Contains(Inspection, "Acceptˉreconstructedˉimage");
        Contains(Inspection, "Appendˉmapˉline");
        Contains(Inspection, "Buildˉcanonicalˉmap");

        var Authorized = Module.Module.Capabilities
            .Select(Capability => Capability.Name)
            .ToImmutableHashSet(StringComparer.Ordinal);
        Equal(0, new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                [],
                TextWriter.Null,
                TextWriter.Null,
                new Testˉfileˉreader((_, _) => throw new InvalidOperationException(
                    "The no-argument linker self-test must not read a hosted file.")),
                new Capturingˉfileˉwriter())),
            new(Authorized, Maximumˉinstructions: 20_000_000)).Runˉmain().Exitˉcode);

        var Sampleˉbytes = Objectˉcodec.Write(Buildˉsampleˉobject()).ToImmutableArray();
        var Valid = Runˉwvˉlinkerˉscan(Module, Sampleˉbytes);
        Equal(0, Valid.Exitˉcode);
        Equal(
            "object status=Valid sections=2 symbols=3 relocations=1 offset=189\n",
            Valid.Output);
        Equal(string.Empty, Valid.Diagnostics);

        var Badˉmagic = Sampleˉbytes.ToArray();
        Badˉmagic[0] = 0;
        var Badˉmagicˉresult = Runˉwvˉlinkerˉscan(Module, Badˉmagic.ToImmutableArray());
        Equal(2, Badˉmagicˉresult.Exitˉcode);
        Equal(string.Empty, Badˉmagicˉresult.Output);
        Contains(Badˉmagicˉresult.Diagnostics, "object status=Badˉmagic");

        var Representativeˉobjects = new[]
        {
            Assembleˉsuccess(HELLO_ASSEMBLY_SOURCE).ToImmutableArray(),
            Assembleˉsuccess(CONSOLE_PROVIDER_ASSEMBLY_SOURCE).ToImmutableArray(),
            Assembleˉsuccess(COMPLETE_ASSEMBLY_SOURCE).ToImmutableArray(),
            Objectˉcodec.Write(new Objectˉfile(
                Objectˉarchitecture.X86ˉ64,
                [new(".text", Objectˉsectionˉkind.Code, 1, 0, [])],
                [],
                [])).ToImmutableArray(),
        };
        foreach (var Objectˉbytes in Representativeˉobjects)
        {
            True(Objectˉisˉvalid(Objectˉbytes), "The Stage 0 oracle rejected a representative WVO fixture.");
            Equal(0, Runˉwvˉlinkerˉscan(Module, Objectˉbytes).Exitˉcode);
        }

        var Random = new Random(0x57_56_4F_31);
        for (var Case = 0; Case < 128; Case++)
        {
            var Mutation = Sampleˉbytes.ToArray();
            var Offset = Random.Next(Mutation.Length);
            Mutation[Offset] = Mutation[Offset] == byte.MaxValue
                ? (byte)0
                : checked((byte)(Mutation[Offset] + 1));
            var Mutationˉbytes = Mutation.ToImmutableArray();
            var Oracleˉaccepted = Objectˉisˉvalid(Mutationˉbytes);
            var Windvaleˉaccepted = Runˉwvˉlinkerˉscan(Module, Mutationˉbytes).Exitˉcode == 0;
            if (Oracleˉaccepted != Windvaleˉaccepted)
            {
                throw new InvalidOperationException(
                    $"WVO differential case {Case} at byte {Offset} disagreed: oracle={Oracleˉaccepted}, Windvale={Windvaleˉaccepted}.");
            }
        }

        for (var Case = 0; Case < 128; Case++)
        {
            var Randomˉbytes = new byte[Random.Next(0, 257)];
            Random.NextBytes(Randomˉbytes);
            var Input = Randomˉbytes.ToImmutableArray();
            var Oracleˉaccepted = Objectˉisˉvalid(Input);
            var Windvaleˉaccepted = Runˉwvˉlinkerˉscan(Module, Input).Exitˉcode == 0;
            if (Oracleˉaccepted != Windvaleˉaccepted)
            {
                throw new InvalidOperationException(
                    $"WVO random differential case {Case} disagreed: oracle={Oracleˉaccepted}, Windvale={Windvaleˉaccepted}.");
            }
        }
    }

    private static void Wvˉlinkerˉresolvesˉandˉlaysˉout()
    {
        var Module = Moduleˉcodec.Readˉandˉverify(Compileˉwithˉtoolˉfoundationˉsuccess(
            WVLINK_CORE_SOURCE,
            "Wv-Linker-Core.wv"));
        var Mainˉobject = Assembleˉsuccess(HELLO_ASSEMBLY_SOURCE).ToImmutableArray();
        var Providerˉobject = Assembleˉsuccess(CONSOLE_PROVIDER_ASSEMBLY_SOURCE).ToImmutableArray();

        var Oracle = Linkˉsuccess(
            [Mainˉobject.ToArray(), Providerˉobject.ToArray()],
            new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"));
        var Canonical = Runˉwvˉlinkerˉanalysis(
            Module,
            Linkˉcontract.DEFAULT_BASE_ADDRESS.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "Main",
            Mainˉobject,
            Providerˉobject);
        Equal(0, Canonical.Exitˉcode);
        Equal(System.Text.Encoding.UTF8.GetString(Oracle.Mapˉbytes.AsSpan()), Canonical.Output);
        Equal(string.Empty, Canonical.Diagnostics);
        Equal(2, Canonical.Readˉcount);
        Equal(1, Canonical.Writeˉcount);
        Equal("output.bin", Canonical.Writtenˉresourceˉname);
        Sequenceˉequal(Oracle.Imageˉbytes, Canonical.Writtenˉbytes);
        Equal(Oracle.Imageˉbytes.Length, 24);
        Equal(Oracle.Entryˉaddress, 1_048_576u);

        var Reversedˉoracle = Linkˉsuccess(
            [Providerˉobject.ToArray(), Mainˉobject.ToArray()],
            new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"));
        var Reversed = Runˉwvˉlinkerˉanalysis(
            Module,
            "1048576",
            "Main",
            Providerˉobject,
            Mainˉobject);
        Equal(0, Reversed.Exitˉcode);
        Equal(
            System.Text.Encoding.UTF8.GetString(Reversedˉoracle.Mapˉbytes.AsSpan()),
            Reversed.Output);
        Equal(2, Reversed.Readˉcount);
        Equal(1, Reversed.Writeˉcount);
        Sequenceˉequal(Reversedˉoracle.Imageˉbytes, Reversed.Writtenˉbytes);

        var Unalignedˉoracle = Linkˉsuccess([Providerˉobject.ToArray()], new(1, "Console_write"));
        var Unaligned = Runˉwvˉlinkerˉanalysis(Module, "1", "Console_write", Providerˉobject);
        Equal(0, Unaligned.Exitˉcode);
        Equal(
            System.Text.Encoding.UTF8.GetString(Unalignedˉoracle.Mapˉbytes.AsSpan()),
            Unaligned.Output);
        Equal(1, Unaligned.Readˉcount);
        Equal(1, Unaligned.Writeˉcount);
        Sequenceˉequal(Unalignedˉoracle.Imageˉbytes, Unaligned.Writtenˉbytes);

        var Completeˉobject = Assembleˉsuccess(COMPLETE_ASSEMBLY_SOURCE).ToImmutableArray();
        var Completeˉoracle = Linkˉsuccess(
            [Completeˉobject.ToArray()],
            new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"));
        var Complete = Runˉwvˉlinkerˉanalysis(
            Module,
            "1048576",
            "Main",
            Completeˉobject);
        Equal(0, Complete.Exitˉcode);
        Equal(
            System.Text.Encoding.UTF8.GetString(Completeˉoracle.Mapˉbytes.AsSpan()),
            Complete.Output);
        Equal(1, Complete.Writeˉcount);
        Sequenceˉequal(Completeˉoracle.Imageˉbytes, Complete.Writtenˉbytes);

        var Maximumˉimageˉobject = Objectˉcodec.Write(new Objectˉfile(
            Objectˉarchitecture.X86ˉ64,
            [
                new(".text", Objectˉsectionˉkind.Code, 1, 1, [0xC3]),
                new(".bss", Objectˉsectionˉkind.Zeroˉfill, 1, Linkˉlimits.MAX_IMAGE_BYTES - 1, []),
            ],
            [new("Main", Objectˉsymbolˉbinding.Export, Objectˉsymbolˉkind.Function, 0, 0, 1)],
            [])).ToImmutableArray();
        var Maximumˉimageˉoracle = Linkˉsuccess(
            [Maximumˉimageˉobject.ToArray()],
            new(0, "Main"));
        var Maximumˉimage = Runˉwvˉlinkerˉanalysisˉwithˉlimit(
            Module,
            "0",
            "Main",
            200_000_000,
            Maximumˉimageˉobject);
        Equal(0, Maximumˉimage.Exitˉcode);
        Equal(
            System.Text.Encoding.UTF8.GetString(Maximumˉimageˉoracle.Mapˉbytes.AsSpan()),
            Maximumˉimage.Output);
        Equal(1, Maximumˉimage.Readˉcount);
        Equal(1, Maximumˉimage.Writeˉcount);
        Equal(
            Objectˉdigest.Calculateˉsha256(Maximumˉimageˉoracle.Imageˉbytes.AsSpan()),
            Objectˉdigest.Calculateˉsha256(Maximumˉimage.Writtenˉbytes.AsSpan()));
        True(
            Maximumˉimage.Executedˉinstructions < 200_000_000,
            "The maximum-image verifier exhausted its explicit instruction budget.");

        var Mapˉlimitˉlocals = Enumerable.Range(0, Objectˉlimits.MAX_SYMBOLS)
            .Select(Index => new Objectˉsymbol(
                $"L{Index:D4}",
                Objectˉsymbolˉbinding.Local,
                Objectˉsymbolˉkind.Function,
                0,
                0,
                0))
            .ToImmutableArray();
        var Mapˉlimitˉlocalˉobject = Objectˉcodec.Write(new Objectˉfile(
            Objectˉarchitecture.X86ˉ64,
            [new(".text", Objectˉsectionˉkind.Code, 1, 0, [])],
            Mapˉlimitˉlocals,
            [])).ToImmutableArray();
        var Mapˉlimitˉentryˉobject = Objectˉcodec.Write(new Objectˉfile(
            Objectˉarchitecture.X86ˉ64,
            [new(".text", Objectˉsectionˉkind.Code, 1, 0, [])],
            [
                .. Mapˉlimitˉlocals.Take(Objectˉlimits.MAX_SYMBOLS - 1),
                new("Main", Objectˉsymbolˉbinding.Export, Objectˉsymbolˉkind.Function, 0, 0, 0),
            ],
            [])).ToImmutableArray();
        var Mapˉlimit = Runˉwvˉlinkerˉanalysisˉwithˉlimit(
            Module,
            "0",
            "Main",
            200_000_000,
            Mapˉlimitˉentryˉobject,
            Mapˉlimitˉlocalˉobject,
            Mapˉlimitˉlocalˉobject,
            Mapˉlimitˉlocalˉobject);
        Equal(2, Mapˉlimit.Exitˉcode);
        Equal(string.Empty, Mapˉlimit.Output);
        Contains(Mapˉlimit.Diagnostics, "link status=WVL1012");
        Contains(Mapˉlimit.Diagnostics, "input=4294967295");
        Equal(4, Mapˉlimit.Readˉcount);
        Equal(0, Mapˉlimit.Writeˉcount);
        True(
            Mapˉlimit.Executedˉinstructions < 200_000_000,
            "The maximum-map rejection exhausted its explicit instruction budget.");

        var Undefined = Runˉwvˉlinkerˉanalysis(Module, "1048576", "Main", Mainˉobject);
        Equal(2, Undefined.Exitˉcode);
        Contains(Undefined.Diagnostics, "link status=WVL1005");
        Contains(Undefined.Diagnostics, "input=0");
        Equal(0, Undefined.Writeˉcount);

        var Duplicate = Runˉwvˉlinkerˉanalysis(
            Module,
            "1048576",
            "Console_write",
            Providerˉobject,
            Providerˉobject);
        Equal(2, Duplicate.Exitˉcode);
        Contains(Duplicate.Diagnostics, "link status=WVL1004");
        Contains(Duplicate.Diagnostics, "input=1");

        var Wrongˉkindˉprovider = Assembleˉsuccess("""
            windvale-assembly 1
            symbol export data Console_write in .data
            section data .data align 1
            define Console_write
            bytes 0
            end define
            end section
            """).ToImmutableArray();
        var Kindˉmismatch = Runˉwvˉlinkerˉanalysis(
            Module,
            "1048576",
            "Main",
            Mainˉobject,
            Wrongˉkindˉprovider);
        Equal(2, Kindˉmismatch.Exitˉcode);
        Contains(Kindˉmismatch.Diagnostics, "link status=WVL1006");
        Contains(Kindˉmismatch.Diagnostics, "input=0");

        var Missingˉentry = Runˉwvˉlinkerˉanalysis(
            Module,
            "1048576",
            "Main",
            Providerˉobject);
        Equal(2, Missingˉentry.Exitˉcode);
        Contains(Missingˉentry.Diagnostics, "link status=WVL1007");
        Contains(Missingˉentry.Diagnostics, "input=4294967295");

        var Layoutˉoverflow = Runˉwvˉlinkerˉanalysis(
            Module,
            uint.MaxValue.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "Console_write",
            Providerˉobject);
        Equal(2, Layoutˉoverflow.Exitˉcode);
        Contains(Layoutˉoverflow.Diagnostics, "link status=WVL1008");
        Contains(Layoutˉoverflow.Diagnostics, "input=0");

        var Absoluteˉoverflowˉobject = Objectˉcodec.Write(new Objectˉfile(
            Objectˉarchitecture.X86ˉ64,
            [new(".text", Objectˉsectionˉkind.Code, 1, 4, [0, 0, 0, 0])],
            [new("Main", Objectˉsymbolˉbinding.Export, Objectˉsymbolˉkind.Function, 0, 0, 4)],
            [new(Objectˉrelocationˉkind.Absoluteˉu32, 0, 0, 0, int.MaxValue)]))
            .ToImmutableArray();
        var Absoluteˉoverflow = Runˉwvˉlinkerˉanalysis(
            Module,
            (uint.MaxValue - 3).ToString(System.Globalization.CultureInfo.InvariantCulture),
            "Main",
            Absoluteˉoverflowˉobject);
        Equal(2, Absoluteˉoverflow.Exitˉcode);
        Contains(Absoluteˉoverflow.Diagnostics, "link status=WVL1009");
        Contains(Absoluteˉoverflow.Diagnostics, "input=0");
        Equal(0, Absoluteˉoverflow.Writeˉcount);

        var Relativeˉoverflowˉobject = Objectˉcodec.Write(new Objectˉfile(
            Objectˉarchitecture.X86ˉ64,
            [new(".text", Objectˉsectionˉkind.Code, 1, 5, [0, 0, 0, 0, 0])],
            [
                new("Target", Objectˉsymbolˉbinding.Local, Objectˉsymbolˉkind.Function, 0, 4, 1),
                new("Main", Objectˉsymbolˉbinding.Export, Objectˉsymbolˉkind.Function, 0, 0, 4),
            ],
            [new(Objectˉrelocationˉkind.Relativeˉi32, 0, 0, 0, int.MaxValue)]))
            .ToImmutableArray();
        var Relativeˉoverflow = Runˉwvˉlinkerˉanalysis(
            Module,
            "0",
            "Main",
            Relativeˉoverflowˉobject);
        Equal(2, Relativeˉoverflow.Exitˉcode);
        Contains(Relativeˉoverflow.Diagnostics, "link status=WVL1010");
        Contains(Relativeˉoverflow.Diagnostics, "input=0");
        Equal(0, Relativeˉoverflow.Writeˉcount);

        var Invalidˉobject = Runˉwvˉlinkerˉanalysis(
            Module,
            "1048576",
            "Main",
            ImmutableArray.Create<byte>(0));
        Equal(2, Invalidˉobject.Exitˉcode);
        Contains(Invalidˉobject.Diagnostics, "link status=WVL1002");
        Contains(Invalidˉobject.Diagnostics, "input=0");

        var Maximumˉsectionˉobject = Objectˉcodec.Write(new Objectˉfile(
            Objectˉarchitecture.X86ˉ64,
            Enumerable.Range(0, Objectˉlimits.MAX_SECTIONS)
                .Select(Index => new Objectˉsection(
                    ".s" + Index.ToString("D2", System.Globalization.CultureInfo.InvariantCulture),
                    Objectˉsectionˉkind.Code,
                    1,
                    0,
                    []))
                .ToImmutableArray(),
            [],
            [])).ToImmutableArray();
        var Aggregateˉinputs = Enumerable.Repeat(Maximumˉsectionˉobject, 5).ToArray();
        var Aggregateˉoracle = Linkˉcompiler.Link(
            Aggregateˉinputs.Select(Bytes => new Linkˉinput(Bytes)).ToImmutableArray(),
            new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"));
        Equal("WVL1003", Aggregateˉoracle.Diagnostics.Single().Code);
        Equal(-1, Aggregateˉoracle.Diagnostics.Single().Inputˉindex);
        var Aggregateˉoverflow = Runˉwvˉlinkerˉanalysis(
            Module,
            "1048576",
            "Main",
            Aggregateˉinputs);
        Equal(2, Aggregateˉoverflow.Exitˉcode);
        Contains(Aggregateˉoverflow.Diagnostics, "link status=WVL1003");
        Contains(Aggregateˉoverflow.Diagnostics, "input=4294967295");
        Equal(5, Aggregateˉoverflow.Readˉcount);

        var Invalidˉbase = Runˉwvˉlinkerˉanalysis(
            Module,
            "4294967296",
            "Main",
            Mainˉobject,
            Providerˉobject);
        Equal(2, Invalidˉbase.Exitˉcode);
        Contains(Invalidˉbase.Diagnostics, "link status=WVL1001 inputs=2");
        Equal(0, Invalidˉbase.Readˉcount);

        var Invalidˉentry = Runˉwvˉlinkerˉanalysis(
            Module,
            "1048576",
            "Bad-name",
            Mainˉobject,
            Providerˉobject);
        Equal(2, Invalidˉentry.Exitˉcode);
        Contains(Invalidˉentry.Diagnostics, "link status=WVL1001 inputs=2");
        Equal(0, Invalidˉentry.Readˉcount);
    }

    private static void Linkerˉrejectsˉinvalidˉlinks()
    {
        var Mainˉobject = Assembleˉsuccess(HELLO_ASSEMBLY_SOURCE);
        var Providerˉobject = Assembleˉsuccess(CONSOLE_PROVIDER_ASSEMBLY_SOURCE);
        Hasˉlinkˉdiagnostic([], new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"), "WVL1001");
        Hasˉlinkˉdiagnostic(
            [Providerˉobject],
            new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Bad-name"),
            "WVL1001");
        Hasˉlinkˉdiagnostic([[0]], new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"), "WVL1002");
        Hasˉlinkˉdiagnostic(
            [new byte[Objectˉlimits.MAX_OBJECT_BYTES + 1]],
            new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"),
            "WVL1002");
        Hasˉlinkˉdiagnostic(
            [Providerˉobject, Providerˉobject],
            new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Console_write"),
            "WVL1004");
        Hasˉlinkˉdiagnostic(
            [Mainˉobject],
            new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"),
            "WVL1005");

        var Wrongˉkindˉprovider = Assembleˉsuccess("""
            windvale-assembly 1
            symbol export data Console_write in .data
            section data .data align 1
            define Console_write
            bytes 0
            end define
            end section
            """);
        Hasˉlinkˉdiagnostic(
            [Mainˉobject, Wrongˉkindˉprovider],
            new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"),
            "WVL1006");
        Hasˉlinkˉdiagnostic(
            [Providerˉobject],
            new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"),
            "WVL1007");
        Hasˉlinkˉdiagnostic(
            [Wrongˉkindˉprovider],
            new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Console_write"),
            "WVL1007");

        var Addressˉoverflow = Objectˉcodec.Write(new(
            Objectˉarchitecture.X86ˉ64,
            [new(".text", Objectˉsectionˉkind.Code, 1, 2, [0, 0])],
            [new("Main", Objectˉsymbolˉbinding.Export, Objectˉsymbolˉkind.Function, 0, 0, 2)],
            []));
        Hasˉlinkˉdiagnostic(
            [Addressˉoverflow],
            new(uint.MaxValue, "Main"),
            "WVL1008");

        var Absoluteˉoverflow = Objectˉcodec.Write(new(
            Objectˉarchitecture.X86ˉ64,
            [new(".text", Objectˉsectionˉkind.Code, 1, 4, [0, 0, 0, 0])],
            [new("Main", Objectˉsymbolˉbinding.Export, Objectˉsymbolˉkind.Function, 0, 0, 4)],
            [new(Objectˉrelocationˉkind.Absoluteˉu32, 0, 0, 0, int.MaxValue)]));
        Hasˉlinkˉdiagnostic(
            [Absoluteˉoverflow],
            new(uint.MaxValue - 3, "Main"),
            "WVL1009");

        var Relativeˉoverflow = Objectˉcodec.Write(new(
            Objectˉarchitecture.X86ˉ64,
            [new(".text", Objectˉsectionˉkind.Code, 1, 5, [0, 0, 0, 0, 0])],
            [
                new("Target", Objectˉsymbolˉbinding.Local, Objectˉsymbolˉkind.Function, 0, 4, 1),
                new("Main", Objectˉsymbolˉbinding.Export, Objectˉsymbolˉkind.Function, 0, 0, 4),
            ],
            [new(Objectˉrelocationˉkind.Relativeˉi32, 0, 0, 0, int.MaxValue)]));
        Hasˉlinkˉdiagnostic([Relativeˉoverflow], new(0, "Main"), "WVL1010");

        var Manyˉsections = Objectˉcodec.Write(new(
            Objectˉarchitecture.X86ˉ64,
            Enumerable.Range(0, Objectˉlimits.MAX_SECTIONS)
                .Select(Index => new Objectˉsection(
                    $".s{Index:D2}",
                    Objectˉsectionˉkind.Code,
                    1,
                    0,
                    []))
                .ToImmutableArray(),
            [new("Main", Objectˉsymbolˉbinding.Export, Objectˉsymbolˉkind.Function, 0, 0, 0)],
            []));
        Hasˉlinkˉdiagnostic(
            [Manyˉsections, Manyˉsections, Manyˉsections, Manyˉsections, Manyˉsections],
            new(0, "Main"),
            "WVL1003");

        var Longˉsuffix = new string('x', 240);
        var Manyˉsymbols = Objectˉcodec.Write(new(
            Objectˉarchitecture.X86ˉ64,
            [new(".text", Objectˉsectionˉkind.Code, 1, 0, [])],
            [
                .. Enumerable.Range(0, Objectˉlimits.MAX_SYMBOLS - 1)
                    .Select(Index => new Objectˉsymbol(
                        $"L{Index:D4}{Longˉsuffix}",
                        Objectˉsymbolˉbinding.Local,
                        Objectˉsymbolˉkind.Function,
                        0,
                        0,
                        0)),
                new("Main", Objectˉsymbolˉbinding.Export, Objectˉsymbolˉkind.Function, 0, 0, 0),
            ],
            []));
        Hasˉlinkˉdiagnostic([Manyˉsymbols], new(0, "Main"), "WVL1012");
    }

    private static void Linkerˉcontainsˉhostileˉinput()
    {
        var Random = new Random(0x57_56_4C);
        for (var Case = 0; Case < 200; Case++)
        {
            var Bytes = new byte[Random.Next(0, 512)];
            Random.NextBytes(Bytes);
            var Result = Linkˉcompiler.Link(
                [new(Bytes.ToImmutableArray())],
                new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"));
            False(Result.Success, $"Hostile object case {Case} unexpectedly linked.");
            Equal("WVL1002", Result.Diagnostics.Single().Code);
            Equal(0, Result.Imageˉbytes.Length);
            Equal(0, Result.Mapˉbytes.Length);
        }
    }

    private static void Wvaˉassemblerˉcoreˉrecognizesˉsource()
    {
        var Moduleˉbytes = Compileˉwithˉtoolˉfoundationˉsuccess(
            WVA_ASSEMBLER_CORE_SOURCE,
            "Wva-Assembler-Core.wv");
        Equal(WVA_ASSEMBLER_CORE_SHA256, Moduleˉdigest.Calculateˉsha256(Moduleˉbytes));
        var Module = Moduleˉcodec.Readˉandˉverify(Moduleˉbytes);
        Equal("Wvaˉassemblerˉcore", Module.Module.Name);
        Equal(Moduleˉprofile.Hosted, Module.Module.Profile);
        Sequenceˉequal(
            [
                Capabilityˉcatalog.CONSOLE_WRITE_LINE,
                Capabilityˉcatalog.DIAGNOSTIC_WRITE_LINE,
                Capabilityˉcatalog.FILE_READ_BYTES,
                Capabilityˉcatalog.FILE_WRITE_BYTES,
                Capabilityˉcatalog.PROCESS_ARGUMENT,
                Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT,
            ],
            Module.Module.Capabilities.Select(Capability => Capability.Name));
        True(
            Module.Module.Types.Any(Type => Type.Name == "Wvaˉsemanticˉinspection"),
            "The WVA semantic inspection record was not serialized.");
        True(
            Module.Module.Types.Any(Type => Type.Name == "Wvaˉsemanticˉstatus"),
            "The WVA semantic status enum was not serialized.");
        True(
            Module.Module.Types.Any(Type => Type.Name == "Wvaˉobjectˉencoding"),
            "The WVA object encoding record was not serialized.");

        var Inspection = Moduleˉinspector.Inspect(Module, Moduleˉbytes);
        Contains(Inspection, "Scanˉwva");
        Contains(Inspection, "Inspectˉwvaˉsemantics");
        Contains(Inspection, "Encodeˉwva");
        Contains(Inspection, "Readˉtoken");
        Contains(Inspection, "bytes.concat");
        Contains(Inspection, "bytes.from_u32_little");
        Contains(Inspection, "text.utf8_is_valid");
        Contains(Inspection, "file.read_bytes");
        Contains(Inspection, "file.write_bytes");

        var Authorized = Module.Module.Capabilities
            .Select(Capability => Capability.Name)
            .ToImmutableHashSet(StringComparer.Ordinal);
        Throwsˉruntime(
            "WVR3010",
            () => _ = new Referenceˉruntime(
                Module,
                new Referenceˉcapabilityˉhost(new StringWriter()),
                Runtimeˉoptions.Portableˉdefaults).Runˉmain());

        var Selfˉtestˉwriter = new Capturingˉfileˉwriter();
        var Selfˉtest = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                [],
                TextWriter.Null,
                TextWriter.Null,
                new Testˉfileˉreader((_, _) => throw new InvalidOperationException(
                    "The WVA assembler self-test must not read a hosted file.")),
                Selfˉtestˉwriter)),
            new(Authorized, Maximumˉinstructions: 10_000_000)).Runˉmain();
        Equal(0, Selfˉtest.Exitˉcode);
        Equal(0, Selfˉtestˉwriter.Writeˉcount);

        (Runtimeˉresult Result, string Output, string Diagnostics, Capturingˉfileˉwriter Writer) Runˉsource(
            ImmutableArray<byte> input,
            string resourceˉname)
        {
            var Output = new StringWriter();
            var Diagnostics = new StringWriter();
            var Writer = new Capturingˉfileˉwriter();
            var Result = new Referenceˉruntime(
                Module,
                new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                    [resourceˉname, resourceˉname + ".wvo"],
                    Output,
                    Diagnostics,
                    new Testˉfileˉreader((Name, Maximumˉbytes) =>
                    {
                        Equal(resourceˉname, Name);
                        True(input.Length <= Maximumˉbytes, "The WVA assembler hosted byte limit was too small.");
                        return input;
                    }),
                    Writer)),
                new(Authorized, Maximumˉinstructions: 10_000_000)).Runˉmain();
            return (Result, Output.ToString(), Diagnostics.ToString(), Writer);
        }

        var Canonicalˉsource = System.Text.Encoding.UTF8.GetBytes(HELLO_ASSEMBLY_SOURCE).ToImmutableArray();
        var Canonical = Runˉsource(Canonicalˉsource, "hello.wva");
        Equal(0, Canonical.Result.Exitˉcode);
        Equal(
            "wvasm 1\n" +
            "assembly status=valid object-bytes=218 sections=2 symbols=3 relocations=2 offset=403 line=22 column=1\n",
            Canonical.Output);
        Equal(string.Empty, Canonical.Diagnostics);
        Equal(1, Canonical.Writer.Writeˉcount);
        Equal("hello.wva.wvo", Canonical.Writer.Resourceˉname);
        Sequenceˉequal(Assembleˉsuccess(HELLO_ASSEMBLY_SOURCE), Canonical.Writer.Bytes);
        _ = Objectˉcodec.Readˉandˉverify(Canonical.Writer.Bytes.AsSpan());

        var Crˉlfˉsource = System.Text.Encoding.UTF8.GetBytes(
            HELLO_ASSEMBLY_SOURCE.Replace("\n", "\r\n", StringComparison.Ordinal)).ToImmutableArray();
        var Crˉlf = Runˉsource(Crˉlfˉsource, "hello-crlf.wva");
        Equal(0, Crˉlf.Result.Exitˉcode);
        Equal(
            "wvasm 1\n" +
            "assembly status=valid object-bytes=218 sections=2 symbols=3 relocations=2 offset=424 line=22 column=1\n",
            Crˉlf.Output);
        Equal(string.Empty, Crˉlf.Diagnostics);
        Sequenceˉequal(Canonical.Writer.Bytes, Crˉlf.Writer.Bytes);

        var Crˉsource = System.Text.Encoding.UTF8.GetBytes(
            HELLO_ASSEMBLY_SOURCE.Replace('\n', '\r')).ToImmutableArray();
        var Cr = Runˉsource(Crˉsource, "hello-cr.wva");
        Equal(0, Cr.Result.Exitˉcode);
        Equal(
            "wvasm 1\n" +
            "assembly status=valid object-bytes=218 sections=2 symbols=3 relocations=2 offset=403 line=22 column=1\n",
            Cr.Output);
        Equal(string.Empty, Cr.Diagnostics);
        Sequenceˉequal(Canonical.Writer.Bytes, Cr.Writer.Bytes);

        var Invalidˉutf8 = Runˉsource([255], "invalid-utf8.wva");
        Equal(2, Invalidˉutf8.Result.Exitˉcode);
        Equal(string.Empty, Invalidˉutf8.Output);
        Equal(
            "assembly status=WVA1001 object-bytes=0 sections=0 symbols=0 relocations=0 offset=0 line=1 column=1\n",
            Invalidˉutf8.Diagnostics);
        Equal(0, Invalidˉutf8.Writer.Writeˉcount);

        var Boundary = Runˉsource(
            ImmutableArray.Create(Enumerable.Repeat((byte)'a', Assemblyˉlimits.MAX_LINE_BYTES).ToArray()),
            "boundary-line.wva");
        Equal(2, Boundary.Result.Exitˉcode);
        Equal(
            "assembly status=WVA1001 object-bytes=0 sections=0 symbols=0 relocations=0 offset=0 line=1 column=1\n",
            Boundary.Diagnostics);
        Equal(0, Boundary.Writer.Writeˉcount);

        var Longˉline = Runˉsource(
            ImmutableArray.Create(Enumerable.Repeat((byte)'a', Assemblyˉlimits.MAX_LINE_BYTES + 1).ToArray()),
            "long-line.wva");
        Equal(2, Longˉline.Result.Exitˉcode);
        Equal(
            "assembly status=WVA1011 object-bytes=0 sections=0 symbols=0 relocations=0 offset=4096 line=1 column=4097\n",
            Longˉline.Diagnostics);
        Equal(0, Longˉline.Writer.Writeˉcount);

        var Oversizedˉsource = Runˉsource(
            ImmutableArray.Create(new byte[Assemblyˉlimits.MAX_SOURCE_BYTES + 1]),
            "oversized.wva");
        Equal(2, Oversizedˉsource.Result.Exitˉcode);
        Equal(
            "assembly status=WVA1011 object-bytes=0 sections=0 symbols=0 relocations=0 offset=1048576 line=1 column=1\n",
            Oversizedˉsource.Diagnostics);
        Equal(0, Oversizedˉsource.Writer.Writeˉcount);
    }

    private static void Wvaˉassemblerˉmatchesˉoracle()
    {
        var Module = Moduleˉcodec.Readˉandˉverify(Compileˉwithˉtoolˉfoundationˉsuccess(
            WVA_ASSEMBLER_CORE_SOURCE,
            "Wva-Assembler-Core.wv"));
        var Authorized = Module.Module.Capabilities
            .Select(Capability => Capability.Name)
            .ToImmutableHashSet(StringComparer.Ordinal);

        (Runtimeˉresult Result, string Output, string Diagnostics, Capturingˉfileˉwriter Writer) Runˉsource(
            string source)
        {
            var Input = System.Text.Encoding.UTF8.GetBytes(source).ToImmutableArray();
            var Output = new StringWriter();
            var Diagnostics = new StringWriter();
            var Writer = new Capturingˉfileˉwriter();
            var Result = new Referenceˉruntime(
                Module,
                new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                    ["semantic.wva", "semantic.wvo"],
                    Output,
                    Diagnostics,
                    new Testˉfileˉreader((Name, Maximumˉbytes) =>
                    {
                        Equal("semantic.wva", Name);
                        True(Input.Length <= Maximumˉbytes, "The semantic inspector input limit was too small.");
                        return Input;
                    }),
                    Writer)),
                new(Authorized, Maximumˉinstructions: 10_000_000)).Runˉmain();
            return (Result, Output.ToString(), Diagnostics.ToString(), Writer);
        }

        var Complete = Runˉsource(COMPLETE_ASSEMBLY_SOURCE);
        Equal(0, Complete.Result.Exitˉcode);
        Equal(string.Empty, Complete.Diagnostics);
        Contains(
            Complete.Output,
            "assembly status=valid object-bytes=243 sections=3 symbols=3 relocations=2");
        Sequenceˉequal(Assembleˉsuccess(COMPLETE_ASSEMBLY_SOURCE), Complete.Writer.Bytes);
        _ = Objectˉcodec.Readˉandˉverify(Complete.Writer.Bytes.AsSpan());

        const string Numericˉboundaries = """
            windvale-assembly 1
            symbol local data Limits in .data
            symbol export function Main in .text
            section code .text align 16
            define Main
            move_i32 eax -2147483648
            move_i32 ecx 2147483647
            move_u32 edx 4294967295
            return
            end define
            end section
            section data .data align 4
            define Limits
            i32 -2147483648
            i32 2147483647
            u32 4294967295
            bytes 0 255
            end define
            end section
            """;
        var Numeric = Runˉsource(Numericˉboundaries);
        Equal(0, Numeric.Result.Exitˉcode);
        Equal(string.Empty, Numeric.Diagnostics);
        Contains(
            Numeric.Output,
            "assembly status=valid object-bytes=154 sections=2 symbols=2 relocations=0");
        Sequenceˉequal(Assembleˉsuccess(Numericˉboundaries), Numeric.Writer.Bytes);
        _ = Objectˉcodec.Readˉandˉverify(Numeric.Writer.Bytes.AsSpan());

        var Mechanics = Runˉsource(KERNEL_MECHANICS_ASSEMBLY_SOURCE);
        Equal(0, Mechanics.Result.Exitˉcode);
        Equal(string.Empty, Mechanics.Diagnostics);
        Sequenceˉequal(Assembleˉsuccess(KERNEL_MECHANICS_ASSEMBLY_SOURCE), Mechanics.Writer.Bytes);
        var Mechanicsˉobject = Objectˉcodec.Readˉandˉverify(Mechanics.Writer.Bytes.AsSpan()).Value;
        Sequenceˉequal<byte>(
            [0x68, 0xFF, 0xFF, 0xFF, 0xFF,
                0xB9, 0x80, 0x00, 0x00, 0xC0, 0x0F, 0x32, 0x0F, 0xBA, 0xE8, 0x0B,
                0x0F, 0x30, 0x0F, 0x20, 0xC0, 0x48, 0x0F, 0xBA, 0xE8, 0x10,
                0x0F, 0x22, 0xC0, 0x0F, 0x22, 0xD8, 0x0F, 0x20, 0xD8, 0x0F, 0x05,
                0xBA, 0x04, 0x06, 0x00, 0x00, 0xB8, 0x00, 0x20, 0x00, 0x00,
                0x66, 0xEF, 0xFA, 0xF4, 0xE9, 0x00, 0x00, 0x00, 0x00],
            Mechanicsˉobject.Sections[0].Data);

        const string Definitionˉrangesˉandˉregisters = """
            windvale-assembly 1
            symbol local function Alpha in .text
            symbol local function Beta in .text
            symbol local data First in .data
            symbol local data Second in .data
            symbol export function Main in .text
            symbol import function External
            section code .text align 16
            define Alpha
            move_u32 eax 0
            move_u32 ecx 1
            move_u32 edx 2
            move_u32 ebx 3
            move_u32 esp 4
            move_u32 ebp 5
            move_u32 esi 6
            move_u32 edi 7
            call External
            return
            end define
            define Beta
            jump Main
            return
            end define
            define Main
            nop
            trap
            move_i32 eax -1
            return
            end define
            end section
            section data .data align 4
            define First
            bytes 0 255
            u32 2309737967
            i32 -2
            end define
            define Second
            address_u32 Main
            end define
            end section
            """;
        var Ranges = Runˉsource(Definitionˉrangesˉandˉregisters);
        Equal(0, Ranges.Result.Exitˉcode);
        Equal(string.Empty, Ranges.Diagnostics);
        Contains(
            Ranges.Output,
            "assembly status=valid object-bytes=360 sections=2 symbols=6 relocations=3");
        Sequenceˉequal(Assembleˉsuccess(Definitionˉrangesˉandˉregisters), Ranges.Writer.Bytes);
        var Rangesˉobject = Objectˉcodec.Readˉandˉverify(Ranges.Writer.Bytes.AsSpan()).Value;
        Equal(0u, Rangesˉobject.Symbols[0].Offset);
        Equal(46u, Rangesˉobject.Symbols[1].Offset);
        Equal(52u, Rangesˉobject.Symbols[4].Offset);
        Equal(41u, Rangesˉobject.Relocations[0].Offset);
        Equal(47u, Rangesˉobject.Relocations[1].Offset);
        Equal(10u, Rangesˉobject.Relocations[2].Offset);

        const string Emptyˉobjectˉsource = """
            windvale-assembly 1
            section code .text align 1
            end section
            """;
        var Emptyˉobject = Runˉsource(Emptyˉobjectˉsource);
        Equal(0, Emptyˉobject.Result.Exitˉcode);
        Contains(
            Emptyˉobject.Output,
            "assembly status=valid object-bytes=49 sections=1 symbols=0 relocations=0");
        Sequenceˉequal(Assembleˉsuccess(Emptyˉobjectˉsource), Emptyˉobject.Writer.Bytes);
        _ = Objectˉcodec.Readˉandˉverify(Emptyˉobject.Writer.Bytes.AsSpan());

        var Cases = new (string Source, string Code)[]
        {
            ("section code .text align 16", "WVA1001"),
            ("""
                windvale-assembly 1
                section code .text align 16
                end section
                symbol export function Main in .text
                """, "WVA1002"),
            ("""
                windvale-assembly 1
                symbol local
                """, "WVA1003"),
            ("""
                windvale-assembly 1
                symbol local data Bad-name in .data
                """, "WVA1004"),
            ("""
                windvale-assembly 1
                section code .text align 3
                end section
                """, "WVA1005"),
            ("""
                windvale-assembly 1
                symbol export function Main in .text
                symbol local data Data in .data
                """, "WVA1006"),
            ("""
                windvale-assembly 1
                symbol local data Same in .data
                symbol export function Same in .text
                section code .text align 16
                end section
                section data .data align 1
                end section
                """, "WVA1006"),
            ("""
                windvale-assembly 1
                section code Same align 16
                end section
                section data Same align 1
                end section
                """, "WVA1006"),
            ("""
                windvale-assembly 1
                symbol export function Main in .rodata
                section rodata .rodata align 1
                define Main
                bytes 1
                end define
                end section
                """, "WVA1007"),
            ("""
                windvale-assembly 1
                symbol import function External
                section code .text align 16
                define External
                return
                end define
                end section
                """, "WVA1007"),
            ("""
                windvale-assembly 1
                symbol local data Value in .data
                section rodata .rodata align 1
                define Value
                bytes 1
                end define
                end section
                section data .data align 1
                end section
                """, "WVA1007"),
            ("""
                windvale-assembly 1
                symbol export function Main in .text
                section code .text align 16
                define Main
                bytes 1
                end define
                end section
                """, "WVA1008"),
            ("""
                windvale-assembly 1
                symbol export function Main in .text
                section code .text align 16
                define Main
                call Missing
                end define
                end section
                """, "WVA1009"),
            ("""
                windvale-assembly 1
                symbol local data Target in .data
                symbol export function Main in .text
                section code .text align 16
                define Main
                call Target
                end define
                end section
                section data .data align 1
                define Target
                bytes 1
                end define
                end section
                """, "WVA1009"),
            ("""
                windvale-assembly 1
                symbol export function Main in .text
                section code .text align 16
                end section
                """, "WVA1009"),
            ("""
                windvale-assembly 1
                section code .text align 16
                define Unknown
                return
                end define
                end section
                """, "WVA1009"),
            ("""
                windvale-assembly 1
                symbol export function Main in .text
                section code .text align 16
                define Main
                return
                """, "WVA1010"),
            ("""
                windvale-assembly 1
                symbol local data Huge in .bss
                section bss .bss align 16
                define Huge
                zero 16777217
                end define
                end section
                """, "WVA1011"),
            ("""
                windvale-assembly 1
                symbol export function Main in .text
                section code .text align 16
                define Main
                move_i32 eax -2147483649
                end define
                end section
                """, "WVA1005"),
            ("""
                windvale-assembly 1
                symbol export function Main in .text
                section code .text align 16
                define Main
                move_u32 eax 4294967296
                end define
                end section
                """, "WVA1005"),
            ("""
                windvale-assembly 1
                symbol export function Main in .text
                section code .text align 16
                define Main
                push_i32
                end define
                end section
                """, "WVA1003"),
            ("""
                windvale-assembly 1
                symbol export function Main in .text
                section code .text align 16
                define Main
                push_i32 -2147483649
                end define
                end section
                """, "WVA1005"),
            ("""
                windvale-assembly 1
                symbol export function Main in .text
                section code .text align 16
                define Main
                enable_page_protection eax
                end define
                end section
                """, "WVA1003"),
            ("""
                windvale-assembly 1
                symbol export function Main in .text
                section code .text align 16
                define Main
                activate_page_table rax
                end define
                end section
                """, "WVA1003"),
        };

        foreach (var (Source, Code) in Cases)
        {
            var Oracle = Assemblyˉcompiler.Assemble(Source);
            False(Oracle.Success, $"The Stage 0 oracle unexpectedly accepted the {Code} fixture.");
            Equal(Code, Oracle.Diagnostics.Single().Code);

            var Windvale = Runˉsource(Source);
            Equal(2, Windvale.Result.Exitˉcode);
            Equal(string.Empty, Windvale.Output);
            Contains(Windvale.Diagnostics, $"assembly status={Code} ");
            Equal(0, Windvale.Writer.Writeˉcount);
        }

        const string Mutationˉalphabet =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789._$- #\t\r\n";
        var Random = new Random(0x57_56_41);
        for (var Case = 0; Case < 200; Case++)
        {
            var Mutated = COMPLETE_ASSEMBLY_SOURCE.ToCharArray();
            var Mutationˉcount = Random.Next(1, 5);
            for (var Mutation = 0; Mutation < Mutationˉcount; Mutation++)
            {
                var Position = Random.Next(Mutated.Length);
                Mutated[Position] = Mutationˉalphabet[Random.Next(Mutationˉalphabet.Length)];
            }
            var Source = new string(Mutated);
            var Oracle = Assemblyˉcompiler.Assemble(Source);
            var Windvale = Runˉsource(Source);
            if (Oracle.Success != (Windvale.Result.Exitˉcode == 0))
            {
                throw new InvalidOperationException(
                    $"WVA semantic acceptance differed for deterministic mutation {Case}.");
            }
            if (Oracle.Success)
            {
                Equal(1, Windvale.Writer.Writeˉcount);
                Sequenceˉequal(Oracle.Objectˉbytes, Windvale.Writer.Bytes);
                _ = Objectˉcodec.Readˉandˉverify(Windvale.Writer.Bytes.AsSpan());
            }
            else
            {
                Equal(0, Windvale.Writer.Writeˉcount);
            }
        }
    }

    private static void Immutableˉrecordsˉrun()
    {
        const string Source = """
            module Recordˉflow profile portable;

            record Pair {
                Left: i32;
                Right: u32;
            }

            fn Make(Left: i32, Right: u32) -> Pair {
                return Pair(Left, Right);
            }

            fn Readˉleft(Value: Pair) -> i32 {
                return Value.Left;
            }

            export fn Main() -> i32 {
                let Value: Pair = Make(42, 9u32);
                if Value.Right != 9u32 {
                    return 0;
                }

                return Readˉleft(Value);
            }
            """;

        var Bytes = Compileˉsuccess(Source);
        var Module = Moduleˉcodec.Readˉandˉverify(Bytes);
        var Pair = (Recordˉtypeˉdeclaration)Module.Module.Types.Single();
        Equal("Pair", Pair.Name);
        Equal("Left", Pair.Fields[0].Name);
        Equal(Valueˉtype.I32, Pair.Fields[0].Type);
        Equal("Right", Pair.Fields[1].Name);
        Equal(Valueˉtype.U32, Pair.Fields[1].Type);
        Equal(Valueˉshape.Forˉrecord(0), Module.Module.Functions.Single(
            Function => Function.Name == "Make").Returnˉtype);
        Equal(Valueˉshape.Forˉrecord(0), Module.Module.Functions.Single(
            Function => Function.Name == "Readˉleft").Parameterˉtypes.Single());
        Sequenceˉequal(Bytes, Moduleˉcodec.Write(Module.Module));

        var Inspection = Moduleˉinspector.Inspect(Module, Bytes);
        Contains(Inspection, "record Pair");
        Contains(Inspection, "[0] Left: i32");
        Contains(Inspection, "[1] Right: u32");
        Contains(Inspection, "record.create type[0] (Pair)");
        Contains(Inspection, "record.field 0");
        Equal(42, new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new StringWriter()),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain().Exitˉcode);
    }

    private static void Enumsˉandˉformattingˉrun()
    {
        const string Source = """
            module Enumˉformat profile hosted;

            capability console.write_line;

            enum Runˉstatus {
                Ready = 7;
                Failed = 9;
            }

            record Runˉresult {
                Status: Runˉstatus;
                Count: u32;
                Delta: i32;
                Byte: u8;
            }

            fn Describe(Value: Runˉresult) -> text {
                return Textˉconcat(
                    Enumˉname(Value.Status),
                    Textˉconcat(
                        " count=",
                        Textˉconcat(
                            U32ˉformat(Value.Count),
                            Textˉconcat(
                                " delta=",
                                Textˉconcat(
                                    I32ˉformat(Value.Delta),
                                    Textˉconcat(" byte=", U8ˉformat(Value.Byte))
                                )
                            )
                        )
                    )
                );
            }

            export fn Main() -> i32 {
                let Value: Runˉresult = Runˉresult(
                    Runˉstatus.Ready,
                    42u32,
                    -7,
                    255u8
                );
                if Value.Status != Runˉstatus.Ready {
                    return 1;
                }
                if Value.Status == Runˉstatus.Failed {
                    return 2;
                }

                console.write_line(Describe(Value));
                return 0;
            }
            """;

        var Bytes = Compileˉsuccess(Source);
        var Module = Moduleˉcodec.Readˉandˉverify(Bytes);
        Equal(2, Module.Module.Types.Length);
        var Record = (Recordˉtypeˉdeclaration)Module.Module.Types[0];
        var Enum = (Enumˉtypeˉdeclaration)Module.Module.Types[1];
        Equal("Runˉresult", Record.Name);
        Equal("Runˉstatus", Enum.Name);
        Equal(Valueˉshape.Forˉenum(1), Record.Fields[0].Type);
        Equal("Ready", Enum.Members[0].Name);
        Equal(7, Enum.Members[0].Value);
        Equal("Failed", Enum.Members[1].Name);
        Equal(9, Enum.Members[1].Value);
        Sequenceˉequal(Bytes, Moduleˉcodec.Write(Module.Module));

        var Inspection = Moduleˉinspector.Inspect(Module, Bytes);
        Contains(Inspection, "enum Runˉstatus");
        Contains(Inspection, "enum.const type[1] (Runˉstatus)");
        Contains(Inspection, "enum.not_equal");
        Contains(Inspection, "enum.equal");
        Contains(Inspection, "enum.name");
        Contains(Inspection, "i32.format");
        Contains(Inspection, "u8.format");
        Contains(Inspection, "u32.format");
        Contains(Inspection, "text.concat");

        var Output = new StringWriter();
        var Result = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(Output),
            Runtimeˉoptions.Portableˉdefaults with
            {
                Authorizedˉcapabilities = ImmutableHashSet.Create(
                    StringComparer.Ordinal,
                    Capabilityˉcatalog.CONSOLE_WRITE_LINE),
            }).Runˉmain();
        Equal(0, Result.Exitˉcode);
        Equal("Ready count=42 delta=-7 byte=255\n", Output.ToString());
    }

    private static void Sourceˉdiagnosticsˉareˉuseful()
    {
        const string Typeˉmismatch = """
            module Broken profile portable;
            export fn Main() -> i32 {
                let Wrong: bool = 1;
                return 0;
            }
            """;
        var Typeˉresult = Seedˉcompiler.Compile(Typeˉmismatch);
        False(Typeˉresult.Success, "Type-invalid source compiled successfully.");
        var Typeˉdiagnostic = Typeˉresult.Diagnostics.Single(Diagnostic => Diagnostic.Code == "WVC2070");
        Equal(3, Typeˉdiagnostic.Span.Line);
        True(Typeˉdiagnostic.Span.Column > 1, "The diagnostic column was not preserved.");

        const string Missingˉcapability = """
            module Broken profile hosted;
            export fn Main() -> i32 {
                console.write_line("no declaration");
                return 0;
            }
            """;
        Hasˉdiagnostic(Missingˉcapability, "WVC2064");

        const string Missingˉreturn = """
            module Broken profile portable;
            export fn Main() -> i32 { let Value: i32 = 1; }
            """;
        Hasˉdiagnostic(Missingˉreturn, "WVC2030");

        const string Badˉescape = """
            module Broken profile portable;
            data Text: text = "\q";
            export fn Main() -> i32 { return 0; }
            """;
        Hasˉdiagnostic(Badˉescape, "WVC1003");

        const string U8ˉoverflow = """
            module Broken profile portable;
            export fn Main() -> i32 { 256u8; return 0; }
            """;
        Hasˉdiagnostic(U8ˉoverflow, "WVC1001");

        const string U32ˉoverflow = """
            module Broken profile portable;
            export fn Main() -> i32 { 4294967296u32; return 0; }
            """;
        Hasˉdiagnostic(U32ˉoverflow, "WVC1001");

        const string Byteˉdataˉoverflow = """
            module Broken profile portable;
            data Values: bytes = [256];
            export fn Main() -> i32 { return 0; }
            """;
        Hasˉdiagnostic(Byteˉdataˉoverflow, "WVC1106");

        const string Intrinsicˉtypeˉmismatch = """
            module Broken profile portable;
            export fn Main() -> i32 { Bytesˉlength(1u32); return 0; }
            """;
        Hasˉdiagnostic(Intrinsicˉtypeˉmismatch, "WVC2070");

        const string Reservedˉintrinsic = """
            module Broken profile portable;
            fn Bytesˉlength(Value: i32) -> i32 { return Value; }
            export fn Main() -> i32 { return 0; }
            """;
        Hasˉdiagnostic(Reservedˉintrinsic, "WVC2024");

        const string Reservedˉenumˉname = """
            module Broken profile portable;
            fn Enumˉname(Value: i32) -> text { return "bad"; }
            export fn Main() -> i32 { return 0; }
            """;
        Hasˉdiagnostic(Reservedˉenumˉname, "WVC2024");

        const string Reservedˉrecordˉconstructor = """
            module Broken profile portable;
            record Bytesˉlength { Value: i32; }
            export fn Main() -> i32 { return 0; }
            """;
        Hasˉdiagnostic(Reservedˉrecordˉconstructor, "WVC2090");
    }

    private static void Operatorsˉrun()
    {
        const string Source = """
            module Operators profile portable;
            export fn Main() -> i32 {
                var Score: i32 = 0;
                let Seven: i32 = 10 - 3;
                if Seven == 7 { Score = Score + 1; }
                if Seven != 8 { Score = Score + 1; }
                if Seven <= 7 { Score = Score + 1; }
                if Seven > 6 { Score = Score + 1; }
                if Seven >= 7 { Score = Score + 1; }
                if -Seven < 0 { Score = Score + 1; }
                if true == true { Score = Score + 1; }
                if true != false { Score = Score + 1; }
                return Score;
            }
            """;
        Equal(8, Runˉportable(Source));
    }

    private static void Malformedˉmodulesˉareˉrejected()
    {
        var Valid = Compileˉsuccess(SUM_SOURCE);

        var Badˉmagic = (byte[])Valid.Clone();
        Badˉmagic[0] ^= 0xFF;
        Throwsˉbytecode("WVB1002", () => Moduleˉcodec.Readˉandˉverify(Badˉmagic));

        var Badˉversion = (byte[])Valid.Clone();
        Badˉversion[4] = 2;
        Throwsˉbytecode("WVB1003", () => Moduleˉcodec.Readˉandˉverify(Badˉversion));

        var Badˉsectionˉcount = (byte[])Valid.Clone();
        BinaryPrimitives.WriteUInt32LittleEndian(Badˉsectionˉcount.AsSpan(8), 5);
        Throwsˉbytecode("WVB1004", () => Moduleˉcodec.Readˉandˉverify(Badˉsectionˉcount));

        var Badˉflags = (byte[])Valid.Clone();
        Badˉflags[13] = 1;
        Throwsˉbytecode("WVB1009", () => Moduleˉcodec.Readˉandˉverify(Badˉflags));

        var Badˉutf8 = (byte[])Valid.Clone();
        var Moduleˉpayload = Findˉsectionˉpayload(Badˉutf8, Sectionˉkind.Module);
        Badˉutf8[Moduleˉpayload + 5] = 0xFF;
        Throwsˉbytecode("WVB1016", () => Moduleˉcodec.Readˉandˉverify(Badˉutf8));

        var Truncated = Valid[..^1];
        Throwsˉbytecode("WVB1018", () => Moduleˉcodec.Readˉandˉverify(Truncated));

        var Trailing = new byte[Valid.Length + 1];
        Valid.CopyTo(Trailing, 0);
        Throwsˉbytecode("WVB1017", () => Moduleˉcodec.Readˉandˉverify(Trailing));

        var Oversized = new byte[Bytecodeˉlimits.MAX_MODULE_BYTES + 1];
        Throwsˉbytecode("WVB1001", () => Moduleˉcodec.Readˉandˉverify(Oversized));

        var Badˉcount = Compileˉsuccess(HELLO_SOURCE);
        var Capabilityˉpayload = Findˉsectionˉpayload(Badˉcount, Sectionˉkind.Capabilities);
        BinaryPrimitives.WriteUInt32LittleEndian(Badˉcount.AsSpan(Capabilityˉpayload), uint.MaxValue);
        Throwsˉbytecode("WVB1011", () => Moduleˉcodec.Readˉandˉverify(Badˉcount));

        var Badˉtypeˉcount = Compileˉsuccess(SUM_SOURCE);
        var Typesˉpayload = Findˉsectionˉpayload(Badˉtypeˉcount, Sectionˉkind.Types);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Badˉtypeˉcount.AsSpan(Typesˉpayload),
            Bytecodeˉlimits.MAX_NOMINAL_TYPES + 1u);
        Throwsˉbytecode("WVB1012", () => Moduleˉcodec.Readˉandˉverify(Badˉtypeˉcount));

        const string Enumˉsource = """
            module Enumˉbinary profile portable;
            enum State { Ready = 0; }
            export fn Main() -> i32 { return 0; }
            """;
        var Badˉtypeˉkind = Compileˉsuccess(Enumˉsource);
        var Badˉtypeˉpayload = Findˉsectionˉpayload(Badˉtypeˉkind, Sectionˉkind.Types);
        Badˉtypeˉkind[Badˉtypeˉpayload + sizeof(uint)] = byte.MaxValue;
        Throwsˉbytecode("WVB1020", () => Moduleˉcodec.Readˉandˉverify(Badˉtypeˉkind));
    }

    private static void Unsafeˉbytecodeˉisˉrejected()
    {
        Throwsˉbytecode(
            "WVB2003",
            () => Moduleˉverifier.Verify(Buildˉmodule([0xFF], Valueˉtype.Void, maximumˉstack: 0)));

        Throwsˉbytecode(
            "WVB2006",
            () => Moduleˉverifier.Verify(Buildˉmodule([(byte)Opcode.I32ˉconst], Valueˉtype.I32, maximumˉstack: 1)));

        Throwsˉbytecode(
            "WVB2231",
            () => Moduleˉverifier.Verify(Buildˉmodule(
                [.. U32ˉinstruction(Opcode.Jump, 999)],
                Valueˉtype.Void,
                maximumˉstack: 0)));

        Throwsˉbytecode(
            "WVB2210",
            () => Moduleˉverifier.Verify(Buildˉmodule(
                [.. U32ˉinstruction(Opcode.Localˉload, 0), (byte)Opcode.Pop, (byte)Opcode.Return],
                Valueˉtype.Void,
                maximumˉstack: 1)));

        Throwsˉbytecode(
            "WVB2201",
            () => Moduleˉverifier.Verify(Buildˉmodule(
                [(byte)Opcode.Return, (byte)Opcode.Return],
                Valueˉtype.Void,
                maximumˉstack: 0)));

        var Mismatchedˉmerge = new List<byte>();
        Mismatchedˉmerge.AddRange(Boolˉinstruction(true));
        Mismatchedˉmerge.AddRange(U32ˉinstruction(Opcode.Branchˉfalse, 17));
        Mismatchedˉmerge.AddRange(I32ˉinstruction(1));
        Mismatchedˉmerge.AddRange(U32ˉinstruction(Opcode.Jump, 17));
        Mismatchedˉmerge.Add((byte)Opcode.Return);
        Throwsˉbytecode(
            "WVB2232",
            () => Moduleˉverifier.Verify(Buildˉmodule(
                [.. Mismatchedˉmerge],
                Valueˉtype.Void,
                maximumˉstack: 1)));

        Throwsˉbytecode(
            "WVB2202",
            () => Moduleˉverifier.Verify(Buildˉmodule(
                [.. I32ˉinstruction(1), (byte)Opcode.Pop, (byte)Opcode.Return],
                Valueˉtype.Void,
                maximumˉstack: 0)));

        Throwsˉbytecode(
            "WVB2220",
            () => Moduleˉverifier.Verify(Buildˉmodule(
                [.. I32ˉinstruction(0), (byte)Opcode.Bytesˉlength, (byte)Opcode.Pop, (byte)Opcode.Return],
                Valueˉtype.Void,
                maximumˉstack: 1)));

        var Invalidˉrecordˉshape = Buildˉmodule(
            [(byte)Opcode.Return],
            Valueˉtype.Void,
            maximumˉstack: 0) with
        {
            Functions = [new(
                "Main",
                [Valueˉshape.Forˉrecord(0)],
                Valueˉtype.Void,
                [],
                0,
                1,
                0)],
        };
        Throwsˉbytecode("WVB2242", () => Moduleˉverifier.Verify(Invalidˉrecordˉshape));

        ImmutableArray<Nominalˉtypeˉdeclaration> Oneˉu32ˉfield =
        [
            new Recordˉtypeˉdeclaration(
                "Pair",
                [new Recordˉfieldˉdeclaration("Value", Valueˉtype.U32)]),
        ];
        var Wrongˉrecordˉfieldˉtype = Buildˉmodule(
            [
                .. I32ˉinstruction(1),
                .. U32ˉinstruction(Opcode.Recordˉcreate, 0),
                (byte)Opcode.Pop,
                (byte)Opcode.Return,
            ],
            Valueˉtype.Void,
            maximumˉstack: 1) with
        {
            Types = Oneˉu32ˉfield,
        };
        Throwsˉbytecode("WVB2220", () => Moduleˉverifier.Verify(Wrongˉrecordˉfieldˉtype));

        var Fieldˉonˉprimitive = Buildˉmodule(
            [
                .. I32ˉinstruction(1),
                .. U32ˉinstruction(Opcode.Recordˉfield, 0),
                (byte)Opcode.Pop,
                (byte)Opcode.Return,
            ],
            Valueˉtype.Void,
            maximumˉstack: 1) with
        {
            Types = Oneˉu32ˉfield,
        };
        Throwsˉbytecode("WVB2222", () => Moduleˉverifier.Verify(Fieldˉonˉprimitive));

        var Invalidˉrecordˉfield = Buildˉmodule(
            [
                .. U32ˉinstruction(Opcode.U32ˉconst, 1),
                .. U32ˉinstruction(Opcode.Recordˉcreate, 0),
                .. U32ˉinstruction(Opcode.Recordˉfield, 1),
                (byte)Opcode.Pop,
                (byte)Opcode.Return,
            ],
            Valueˉtype.Void,
            maximumˉstack: 1) with
        {
            Types = Oneˉu32ˉfield,
        };
        Throwsˉbytecode("WVB2223", () => Moduleˉverifier.Verify(Invalidˉrecordˉfield));

        var Duplicateˉrecordˉmetadata = Buildˉmodule(
            [(byte)Opcode.Return],
            Valueˉtype.Void,
            maximumˉstack: 0) with
        {
            Types = [new Recordˉtypeˉdeclaration(
                "Pair",
                [new("Value", Valueˉtype.I32), new("Value", Valueˉtype.U32)])],
        };
        Throwsˉbytecode("WVB2152", () => Moduleˉverifier.Verify(Duplicateˉrecordˉmetadata));

        ImmutableArray<Nominalˉtypeˉdeclaration> Oneˉenum =
        [
            new Enumˉtypeˉdeclaration(
                "State",
                [new("Ready", 0), new("Failed", 1)]),
        ];
        var Invalidˉenumˉmember = Buildˉmodule(
            [
                .. Twoˉu32ˉinstruction(Opcode.Enumˉconst, 0, 2),
                (byte)Opcode.Pop,
                (byte)Opcode.Return,
            ],
            Valueˉtype.Void,
            maximumˉstack: 1) with
        {
            Types = Oneˉenum,
        };
        Throwsˉbytecode("WVB2225", () => Moduleˉverifier.Verify(Invalidˉenumˉmember));

        var Enumˉconstantˉonˉrecord = Buildˉmodule(
            [
                .. Twoˉu32ˉinstruction(Opcode.Enumˉconst, 0, 0),
                (byte)Opcode.Pop,
                (byte)Opcode.Return,
            ],
            Valueˉtype.Void,
            maximumˉstack: 1) with
        {
            Types = Oneˉu32ˉfield,
        };
        Throwsˉbytecode("WVB2217", () => Moduleˉverifier.Verify(Enumˉconstantˉonˉrecord));

        var Enumˉnameˉonˉprimitive = Buildˉmodule(
            [
                .. I32ˉinstruction(0),
                (byte)Opcode.Enumˉname,
                (byte)Opcode.Pop,
                (byte)Opcode.Return,
            ],
            Valueˉtype.Void,
            maximumˉstack: 1) with
        {
            Types = Oneˉenum,
        };
        Throwsˉbytecode("WVB2226", () => Moduleˉverifier.Verify(Enumˉnameˉonˉprimitive));

        ImmutableArray<Nominalˉtypeˉdeclaration> Twoˉenums =
        [
            new Enumˉtypeˉdeclaration("First", [new("Value", 0)]),
            new Enumˉtypeˉdeclaration("Second", [new("Value", 0)]),
        ];
        var Mismatchedˉenumˉcomparison = Buildˉmodule(
            [
                .. Twoˉu32ˉinstruction(Opcode.Enumˉconst, 0, 0),
                .. Twoˉu32ˉinstruction(Opcode.Enumˉconst, 1, 0),
                (byte)Opcode.Enumˉequal,
                (byte)Opcode.Pop,
                (byte)Opcode.Return,
            ],
            Valueˉtype.Void,
            maximumˉstack: 2) with
        {
            Types = Twoˉenums,
        };
        Throwsˉbytecode("WVB2224", () => Moduleˉverifier.Verify(Mismatchedˉenumˉcomparison));

        var Wrongˉnominalˉkind = Buildˉmodule(
            [(byte)Opcode.Return],
            Valueˉtype.Void,
            maximumˉstack: 0) with
        {
            Types = Oneˉenum,
            Functions = [new(
                "Main",
                [Valueˉshape.Forˉrecord(0)],
                Valueˉtype.Void,
                [],
                0,
                1,
                0)],
        };
        Throwsˉbytecode("WVB2244", () => Moduleˉverifier.Verify(Wrongˉnominalˉkind));

        var Duplicateˉenumˉmetadata = Buildˉmodule(
            [(byte)Opcode.Return],
            Valueˉtype.Void,
            maximumˉstack: 0) with
        {
            Types = [new Enumˉtypeˉdeclaration(
                "State",
                [new("Ready", 0), new("Failed", 0)])],
        };
        Throwsˉbytecode("WVB2156", () => Moduleˉverifier.Verify(Duplicateˉenumˉmetadata));

        var Duplicateˉnominalˉname = Buildˉmodule(
            [(byte)Opcode.Return],
            Valueˉtype.Void,
            maximumˉstack: 0) with
        {
            Types =
            [
                new Recordˉtypeˉdeclaration("Same", [new("Value", Valueˉtype.I32)]),
                new Enumˉtypeˉdeclaration("Same", [new("Value", 0)]),
            ],
        };
        Throwsˉbytecode("WVB2159", () => Moduleˉverifier.Verify(Duplicateˉnominalˉname));

        var Oversizedˉbyteˉdata = Buildˉmodule(
            [(byte)Opcode.Return],
            Valueˉtype.Void,
            maximumˉstack: 0) with
        {
            Data = [new Bytesˉdataˉdeclaration(
                "Oversizedˉbytes",
                ImmutableArray.Create<byte>(new byte[Bytecodeˉlimits.MAX_BYTE_DATA_BYTES + 1]))],
        };
        Throwsˉbytecode("WVB2125", () => Moduleˉverifier.Verify(Oversizedˉbyteˉdata));

        var Invalidˉtext = new Textˉdataˉdeclaration("Text", "\uD800");
        Throwsˉbytecode(
            "WVB2124",
            () => Moduleˉverifier.Verify(new(
                "Invalidˉtext",
                Moduleˉprofile.Portable,
                [],
                [Invalidˉtext],
                [new("Main", [], Valueˉtype.Void, [], 0, 1, 0)],
                [(byte)Opcode.Return],
                [new("Main", Exportˉkind.Function, 0)])));
    }

    private static void Runtimeˉtrapsˉareˉdeterministic()
    {
        const string Overflow = """
            module Overflow profile portable;
            export fn Main() -> i32 { return 2147483647 + 1; }
            """;
        Throwsˉruntime("WVR3007", () => Runˉportable(Overflow));

        const string Bounds = """
            module Bounds profile portable;
            data Values: [i32] = [1];
            export fn Main() -> i32 { return Values[2]; }
            """;
        Throwsˉruntime("WVR3005", () => Runˉportable(Bounds));

        const string Byteˉbounds = """
            module Byteˉbounds profile portable;
            data Values: bytes = [1, 2, 3];
            export fn Main() -> i32 {
                Bytesˉreadˉu32ˉlittle(Values, 0u32);
                return 0;
            }
            """;
        Throwsˉruntime("WVR3008", () => Runˉportable(Byteˉbounds));

        const string Sliceˉbounds = """
            module Sliceˉbounds profile portable;
            data Values: bytes = [1, 2, 3];
            export fn Main() -> i32 {
                Bytesˉslice(Values, 2u32, 2u32);
                return 0;
            }
            """;
        Throwsˉruntime("WVR3008", () => Runˉportable(Sliceˉbounds));

        const string U32ˉoverflow = """
            module U32ˉoverflow profile portable;
            export fn Main() -> i32 {
                4294967295u32 + 1u32;
                return 0;
            }
            """;
        Throwsˉruntime("WVR3007", () => Runˉportable(U32ˉoverflow));

        const string U32ˉunderflow = """
            module U32ˉunderflow profile portable;
            export fn Main() -> i32 {
                0u32 - 1u32;
                return 0;
            }
            """;
        Throwsˉruntime("WVR3007", () => Runˉportable(U32ˉunderflow));

        var Oversizedˉtextˉresult = Buildˉmodule(
            [
                .. U32ˉinstruction(Opcode.Textˉconst, 0),
                .. U32ˉinstruction(Opcode.Textˉconst, 1),
                (byte)Opcode.Textˉconcat,
                (byte)Opcode.Pop,
                .. I32ˉinstruction(0),
                (byte)Opcode.Return,
            ],
            Valueˉtype.I32,
            maximumˉstack: 2) with
        {
            Data =
            [
                new Textˉdataˉdeclaration("Left", new string('a', 600_000)),
                new Textˉdataˉdeclaration("Right", new string('b', 600_000)),
            ],
        };
        var Verifiedˉoversizedˉtext = Moduleˉverifier.Verify(Oversizedˉtextˉresult);
        Throwsˉruntime(
            "WVR3012",
            () => new Referenceˉruntime(
                Verifiedˉoversizedˉtext,
                new Referenceˉcapabilityˉhost(new StringWriter()),
                Runtimeˉoptions.Portableˉdefaults).Runˉmain());

        var Oversizedˉquoteˉresult = Buildˉmodule(
            [
                .. U32ˉinstruction(Opcode.Textˉconst, 0),
                (byte)Opcode.Textˉquote,
                (byte)Opcode.Pop,
                .. I32ˉinstruction(0),
                (byte)Opcode.Return,
            ],
            Valueˉtype.I32,
            maximumˉstack: 1) with
        {
            Data = [new Textˉdataˉdeclaration("Quoted", new string('\u0100', 200_000))],
        };
        var Verifiedˉoversizedˉquote = Moduleˉverifier.Verify(Oversizedˉquoteˉresult);
        Throwsˉruntime(
            "WVR3012",
            () => new Referenceˉruntime(
                Verifiedˉoversizedˉquote,
                new Referenceˉcapabilityˉhost(new StringWriter()),
                Runtimeˉoptions.Portableˉdefaults).Runˉmain());

        var Oversizedˉdecodeˉresult = Buildˉmodule(
            [
                .. U32ˉinstruction(Opcode.Bytesˉconst, 0),
                (byte)Opcode.Textˉfromˉutf8,
                (byte)Opcode.Pop,
                .. I32ˉinstruction(0),
                (byte)Opcode.Return,
            ],
            Valueˉtype.I32,
            maximumˉstack: 1) with
        {
            Data =
            [
                new Bytesˉdataˉdeclaration(
                    "Encoded",
                    ImmutableArray.Create<byte>(new byte[Bytecodeˉlimits.MAX_UTF8_VALUE_BYTES + 1])),
            ],
        };
        var Verifiedˉoversizedˉdecode = Moduleˉverifier.Verify(Oversizedˉdecodeˉresult);
        Throwsˉruntime(
            "WVR3012",
            () => new Referenceˉruntime(
                Verifiedˉoversizedˉdecode,
                new Referenceˉcapabilityˉhost(new StringWriter()),
                Runtimeˉoptions.Portableˉdefaults).Runˉmain());

        var Oversizedˉbytesˉresult = Buildˉmodule(
            [
                .. U32ˉinstruction(Opcode.Bytesˉconst, 0),
                .. U32ˉinstruction(Opcode.Bytesˉconst, 1),
                (byte)Opcode.Bytesˉconcat,
                (byte)Opcode.Pop,
                .. I32ˉinstruction(0),
                (byte)Opcode.Return,
            ],
            Valueˉtype.I32,
            maximumˉstack: 2) with
        {
            Data =
            [
                new Bytesˉdataˉdeclaration("Left", ImmutableArray.Create<byte>(new byte[3_000_000])),
                new Bytesˉdataˉdeclaration("Right", ImmutableArray.Create<byte>(new byte[3_000_000])),
            ],
        };
        var Verifiedˉoversizedˉbytes = Moduleˉverifier.Verify(Oversizedˉbytesˉresult);
        Throwsˉruntime(
            "WVR3015",
            () => new Referenceˉruntime(
                Verifiedˉoversizedˉbytes,
                new Referenceˉcapabilityˉhost(new StringWriter()),
                Runtimeˉoptions.Portableˉdefaults).Runˉmain());
    }

    private static void Runtimeˉlimitsˉareˉenforced()
    {
        var Sumˉmodule = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(SUM_SOURCE));
        var Limited = new Referenceˉruntime(
            Sumˉmodule,
            new Referenceˉcapabilityˉhost(new StringWriter()),
            new(
                Runtimeˉoptions.Portableˉdefaults.Authorizedˉcapabilities,
                Maximumˉinstructions: 5,
                Collectˉfunctionˉsteps: true));
        Throwsˉruntime("WVR3011", () => _ = Limited.Runˉmain());
        Equal(5L, Limited.Readˉfunctionˉsteps().Sum(Function => Function.Executedˉinstructions));

        const string Recursion = """
            module Recursion profile portable;
            fn Recurse(Value: i32) -> i32 { return Recurse(Value + 1); }
            export fn Main() -> i32 { return Recurse(0); }
            """;
        var Recursiveˉmodule = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(Recursion));
        var Depthˉlimited = new Referenceˉruntime(
            Recursiveˉmodule,
            new Referenceˉcapabilityˉhost(new StringWriter()),
            new(Runtimeˉoptions.Portableˉdefaults.Authorizedˉcapabilities, Maximumˉcallˉdepth: 8));
        Throwsˉruntime("WVR3004", () => _ = Depthˉlimited.Runˉmain());
    }

    private static void Goldenˉhashesˉmatch()
    {
        using var Phases = new Goldenˉphaseˉrecorder(
            Collectˉgoldenˉphaseˉtimings ? GOLDEN_PHASE_TIMINGS : null);
        Phases.Start("artifact-compilation");

        var Sumˉbytes = Compileˉsuccess(SUM_SOURCE);
        var Helloˉbytes = Compileˉsuccess(HELLO_SOURCE);
        var Foundationˉbytes = Compileˉsuccess(FOUNDATION_SOURCE);
        var Sourceˉcompositionˉbytes = Compileˉcompositionˉsuccess(
            new("middle.wv", COMPOSITION_MIDDLE_SOURCE),
            new("leaf.wv", COMPOSITION_LEAF_SOURCE));
        var Machineˉcontractsˉbytes = Compileˉsuccess(MACHINE_CONTRACTS_SOURCE);
        var Machineˉcontractsˉdemoˉbytes = Compileˉwithˉmachineˉcontractsˉsuccess(
            MACHINE_CONTRACTS_DEMO_SOURCE,
            "Machine-Contracts-Demo.wv");
        var Byteˉorderingˉbytes = Compileˉsuccess(BYTE_ORDERING_SOURCE);
        var Byteˉorderingˉdemoˉbytes = Compileˉwithˉbyteˉorderingˉsuccess(
            BYTE_ORDERING_DEMO_SOURCE,
            "Byte-Ordering-Demo.wv");
        var Decimalˉparsingˉbytes = Compileˉsuccess(DECIMAL_PARSING_SOURCE);
        var Decimalˉparsingˉdemoˉbytes = Compileˉwithˉdecimalˉparsingˉsuccess(
            DECIMAL_PARSING_DEMO_SOURCE,
            "Decimal-Parsing-Demo.wv");
        var Byteˉconstructionˉbytes = Compileˉsuccess(BYTE_CONSTRUCTION_SOURCE);
        var Byteˉconstructionˉdemoˉbytes = Compileˉwithˉbyteˉconstructionˉsuccess(
            BYTE_CONSTRUCTION_DEMO_SOURCE,
            "Byte-Construction-Demo.wv");
        var Nativeˉstencilˉcoreˉbytes = Compileˉwithˉnativeˉstencilˉsuccess(
            NATIVE_STENCIL_CORE_SOURCE,
            "Compiler/Windvale/Native-Stencil-Core.wv",
            includeˉnativeˉstencil: false);
        var Nativeˉstencilˉdemoˉbytes = Compileˉwithˉnativeˉstencilˉsuccess(
            NATIVE_STENCIL_DEMO_SOURCE,
            "Examples/Compiler/Native-Stencil-Demo.wv");
        var Sourceˉlexerˉbytes = Compileˉwithˉdecimalˉparsingˉsuccess(
            SOURCE_LEXER_SOURCE,
            "Source-Lexer-Core.wv");
        var Sourceˉlexerˉdemoˉbytes = Compileˉwithˉsourceˉlexerˉsuccess(
            SOURCE_LEXER_DEMO_SOURCE,
            "Source-Lexer-Demo.wv");
        var Sourceˉdeclarationˉparserˉbytes = Compileˉwithˉsourceˉlexerˉsuccess(
            SOURCE_DECLARATION_PARSER_SOURCE,
            "Source-Declaration-Parser.wv");
        var Sourceˉdeclarationˉparserˉdemoˉbytes = Compileˉwithˉsourceˉdeclarationˉparserˉsuccess(
            SOURCE_DECLARATION_PARSER_DEMO_SOURCE,
            "Source-Declaration-Parser-Demo.wv");
        var Sourceˉdeclarationˉparserˉtoolˉbytes = Compileˉwithˉsourceˉdeclarationˉparserˉsuccess(
            SOURCE_DECLARATION_PARSER_TOOL_SOURCE,
            "Source-Declaration-Parser-Tool.wv");
        var Sourceˉbodyˉparserˉbytes = Compileˉwithˉsourceˉdeclarationˉparserˉsuccess(
            SOURCE_BODY_PARSER_SOURCE,
            "Source-Body-Parser.wv");
        var Sourceˉbodyˉparserˉdemoˉbytes = Compileˉwithˉsourceˉbodyˉparserˉsuccess(
            SOURCE_BODY_PARSER_DEMO_SOURCE,
            "Source-Body-Parser-Demo.wv");
        var Sourceˉbodyˉparserˉtoolˉbytes = Compileˉwithˉsourceˉbodyˉparserˉsuccess(
            SOURCE_BODY_PARSER_TOOL_SOURCE,
            "Source-Body-Parser-Tool.wv");
        var Sourceˉsetˉbytes = Compileˉwithˉsourceˉsetˉsuccess(
            SOURCE_SET_SOURCE,
            "Source-Set-Core.wv",
            includeˉsourceˉset: false);
        var Sourceˉsetˉdemoˉbytes = Compileˉwithˉsourceˉsetˉsuccess(
            SOURCE_SET_DEMO_SOURCE,
            "Source-Set-Demo.wv");
        var Sourceˉsetˉtoolˉbytes = Compileˉwithˉsourceˉsetˉsuccess(
            SOURCE_SET_TOOL_SOURCE,
            "Source-Set-Tool.wv");
        var Sourceˉgraphˉbytes = Compileˉwithˉsourceˉgraphˉsuccess(
            SOURCE_GRAPH_SOURCE,
            "Source-Graph-Core.wv",
            includeˉsourceˉgraph: false);
        var Sourceˉgraphˉdemoˉbytes = Compileˉwithˉsourceˉgraphˉsuccess(
            SOURCE_GRAPH_DEMO_SOURCE,
            "Source-Graph-Demo.wv");
        var Sourceˉgraphˉtoolˉbytes = Compileˉwithˉsourceˉgraphˉsuccess(
            SOURCE_GRAPH_TOOL_SOURCE,
            "Source-Graph-Tool.wv");
        var Sourceˉsymbolsˉbytes = Compileˉwithˉsourceˉsymbolsˉsuccess(
            SOURCE_SYMBOLS_SOURCE,
            "Source-Symbols-Core.wv",
            includeˉsourceˉsymbols: false);
        var Sourceˉsymbolsˉdemoˉbytes = Compileˉwithˉsourceˉsymbolsˉsuccess(
            SOURCE_SYMBOLS_DEMO_SOURCE,
            "Source-Symbols-Demo.wv");
        var Sourceˉsymbolsˉtoolˉbytes = Compileˉwithˉsourceˉsymbolsˉsuccess(
            SOURCE_SYMBOLS_TOOL_SOURCE,
            "Source-Symbols-Tool.wv");
        var Sourceˉbindingsˉbytes = Compileˉwithˉsourceˉbindingsˉsuccess(
            SOURCE_BINDINGS_SOURCE,
            "Source-Bindings-Core.wv",
            includeˉsourceˉbindings: false);
        var Sourceˉbindingsˉdemoˉbytes = Compileˉwithˉsourceˉbindingsˉsuccess(
            SOURCE_BINDINGS_DEMO_SOURCE,
            "Source-Bindings-Demo.wv");
        var Sourceˉbindingsˉtoolˉbytes = Compileˉwithˉsourceˉbindingsˉsuccess(
            SOURCE_BINDINGS_TOOL_SOURCE,
            "Source-Bindings-Tool.wv");
        var Wvˉdumpˉbytes = Compileˉsuccess(WVDUMP_CORE_SOURCE);
        var Wvoˉcoreˉbytes = Compileˉwithˉbyteˉorderingˉsuccess(
            WVO_CORE_SOURCE,
            "Wvo-Object-Core.wv");
        var Wvaˉassemblerˉbytes = Compileˉwithˉtoolˉfoundationˉsuccess(
            WVA_ASSEMBLER_CORE_SOURCE,
            "Wva-Assembler-Core.wv");
        var Wvˉlinkerˉbytes = Compileˉwithˉtoolˉfoundationˉsuccess(
            WVLINK_CORE_SOURCE,
            "Wv-Linker-Core.wv");
        var Wvoˉsampleˉbytes = Objectˉcodec.Write(Buildˉsampleˉobject());
        var Assemblyˉobjectˉbytes = Assembleˉsuccess(HELLO_ASSEMBLY_SOURCE);
        var Providerˉobjectˉbytes = Assembleˉsuccess(CONSOLE_PROVIDER_ASSEMBLY_SOURCE);
        var Linkˉresult = Linkˉsuccess(
            [Assemblyˉobjectˉbytes, Providerˉobjectˉbytes],
            new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"));
        var Sumˉhash = Moduleˉdigest.Calculateˉsha256(Sumˉbytes);
        var Helloˉhash = Moduleˉdigest.Calculateˉsha256(Helloˉbytes);
        var Foundationˉhash = Moduleˉdigest.Calculateˉsha256(Foundationˉbytes);
        var Sourceˉcompositionˉhash = Moduleˉdigest.Calculateˉsha256(Sourceˉcompositionˉbytes);
        var Machineˉcontractsˉhash = Moduleˉdigest.Calculateˉsha256(Machineˉcontractsˉbytes);
        var Machineˉcontractsˉdemoˉhash = Moduleˉdigest.Calculateˉsha256(Machineˉcontractsˉdemoˉbytes);
        var Byteˉorderingˉhash = Moduleˉdigest.Calculateˉsha256(Byteˉorderingˉbytes);
        var Byteˉorderingˉdemoˉhash = Moduleˉdigest.Calculateˉsha256(Byteˉorderingˉdemoˉbytes);
        var Decimalˉparsingˉhash = Moduleˉdigest.Calculateˉsha256(Decimalˉparsingˉbytes);
        var Decimalˉparsingˉdemoˉhash = Moduleˉdigest.Calculateˉsha256(Decimalˉparsingˉdemoˉbytes);
        var Byteˉconstructionˉhash = Moduleˉdigest.Calculateˉsha256(Byteˉconstructionˉbytes);
        var Byteˉconstructionˉdemoˉhash = Moduleˉdigest.Calculateˉsha256(Byteˉconstructionˉdemoˉbytes);
        var Nativeˉstencilˉcoreˉhash = Moduleˉdigest.Calculateˉsha256(Nativeˉstencilˉcoreˉbytes);
        var Nativeˉstencilˉdemoˉhash = Moduleˉdigest.Calculateˉsha256(Nativeˉstencilˉdemoˉbytes);
        var Sourceˉlexerˉhash = Moduleˉdigest.Calculateˉsha256(Sourceˉlexerˉbytes);
        var Sourceˉlexerˉdemoˉhash = Moduleˉdigest.Calculateˉsha256(Sourceˉlexerˉdemoˉbytes);
        var Sourceˉdeclarationˉparserˉhash = Moduleˉdigest.Calculateˉsha256(
            Sourceˉdeclarationˉparserˉbytes);
        var Sourceˉdeclarationˉparserˉdemoˉhash = Moduleˉdigest.Calculateˉsha256(
            Sourceˉdeclarationˉparserˉdemoˉbytes);
        var Sourceˉdeclarationˉparserˉtoolˉhash = Moduleˉdigest.Calculateˉsha256(
            Sourceˉdeclarationˉparserˉtoolˉbytes);
        var Sourceˉbodyˉparserˉhash = Moduleˉdigest.Calculateˉsha256(
            Sourceˉbodyˉparserˉbytes);
        var Sourceˉbodyˉparserˉdemoˉhash = Moduleˉdigest.Calculateˉsha256(
            Sourceˉbodyˉparserˉdemoˉbytes);
        var Sourceˉbodyˉparserˉtoolˉhash = Moduleˉdigest.Calculateˉsha256(
            Sourceˉbodyˉparserˉtoolˉbytes);
        var Sourceˉsetˉhash = Moduleˉdigest.Calculateˉsha256(Sourceˉsetˉbytes);
        var Sourceˉsetˉdemoˉhash = Moduleˉdigest.Calculateˉsha256(Sourceˉsetˉdemoˉbytes);
        var Sourceˉsetˉtoolˉhash = Moduleˉdigest.Calculateˉsha256(Sourceˉsetˉtoolˉbytes);
        var Sourceˉgraphˉhash = Moduleˉdigest.Calculateˉsha256(Sourceˉgraphˉbytes);
        var Sourceˉgraphˉdemoˉhash = Moduleˉdigest.Calculateˉsha256(Sourceˉgraphˉdemoˉbytes);
        var Sourceˉgraphˉtoolˉhash = Moduleˉdigest.Calculateˉsha256(Sourceˉgraphˉtoolˉbytes);
        var Sourceˉsymbolsˉhash = Moduleˉdigest.Calculateˉsha256(Sourceˉsymbolsˉbytes);
        var Sourceˉsymbolsˉdemoˉhash = Moduleˉdigest.Calculateˉsha256(Sourceˉsymbolsˉdemoˉbytes);
        var Sourceˉsymbolsˉtoolˉhash = Moduleˉdigest.Calculateˉsha256(Sourceˉsymbolsˉtoolˉbytes);
        var Sourceˉbindingsˉhash = Moduleˉdigest.Calculateˉsha256(Sourceˉbindingsˉbytes);
        var Sourceˉbindingsˉdemoˉhash = Moduleˉdigest.Calculateˉsha256(Sourceˉbindingsˉdemoˉbytes);
        var Sourceˉbindingsˉtoolˉhash = Moduleˉdigest.Calculateˉsha256(Sourceˉbindingsˉtoolˉbytes);
        var Wvˉdumpˉhash = Moduleˉdigest.Calculateˉsha256(Wvˉdumpˉbytes);
        var Wvoˉcoreˉhash = Moduleˉdigest.Calculateˉsha256(Wvoˉcoreˉbytes);
        var Wvaˉassemblerˉhash = Moduleˉdigest.Calculateˉsha256(Wvaˉassemblerˉbytes);
        var Wvˉlinkerˉhash = Moduleˉdigest.Calculateˉsha256(Wvˉlinkerˉbytes);
        var Wvoˉsampleˉhash = Objectˉdigest.Calculateˉsha256(Wvoˉsampleˉbytes);
        var Assemblyˉobjectˉhash = Objectˉdigest.Calculateˉsha256(Assemblyˉobjectˉbytes);
        var Linkˉimageˉhash = Objectˉdigest.Calculateˉsha256(Linkˉresult.Imageˉbytes.AsSpan());
        var Linkˉmapˉhash = Objectˉdigest.Calculateˉsha256(Linkˉresult.Mapˉbytes.AsSpan());
        var Linkˉmap = System.Text.Encoding.UTF8.GetString(Linkˉresult.Mapˉbytes.AsSpan());
        Equal(SUM_SHA256, Sumˉhash);
        Equal(HELLO_SHA256, Helloˉhash);
        Equal(FOUNDATION_SHA256, Foundationˉhash);
        Equal(SOURCE_COMPOSITION_SHA256, Sourceˉcompositionˉhash);
        Equal(MACHINE_CONTRACTS_SHA256, Machineˉcontractsˉhash);
        Equal(MACHINE_CONTRACTS_DEMO_SHA256, Machineˉcontractsˉdemoˉhash);
        Equal(BYTE_ORDERING_SHA256, Byteˉorderingˉhash);
        Equal(BYTE_ORDERING_DEMO_SHA256, Byteˉorderingˉdemoˉhash);
        Equal(DECIMAL_PARSING_SHA256, Decimalˉparsingˉhash);
        Equal(DECIMAL_PARSING_DEMO_SHA256, Decimalˉparsingˉdemoˉhash);
        Equal(BYTE_CONSTRUCTION_SHA256, Byteˉconstructionˉhash);
        Equal(BYTE_CONSTRUCTION_DEMO_SHA256, Byteˉconstructionˉdemoˉhash);
        Equal(NATIVE_STENCIL_CORE_SHA256, Nativeˉstencilˉcoreˉhash);
        Equal(NATIVE_STENCIL_DEMO_SHA256, Nativeˉstencilˉdemoˉhash);
        Equal(SOURCE_LEXER_SHA256, Sourceˉlexerˉhash);
        Equal(SOURCE_LEXER_DEMO_SHA256, Sourceˉlexerˉdemoˉhash);
        Equal(SOURCE_DECLARATION_PARSER_SHA256, Sourceˉdeclarationˉparserˉhash);
        Equal(SOURCE_DECLARATION_PARSER_DEMO_SHA256, Sourceˉdeclarationˉparserˉdemoˉhash);
        Equal(SOURCE_DECLARATION_PARSER_TOOL_SHA256, Sourceˉdeclarationˉparserˉtoolˉhash);
        Equal(SOURCE_BODY_PARSER_SHA256, Sourceˉbodyˉparserˉhash);
        Equal(SOURCE_BODY_PARSER_DEMO_SHA256, Sourceˉbodyˉparserˉdemoˉhash);
        Equal(SOURCE_BODY_PARSER_TOOL_SHA256, Sourceˉbodyˉparserˉtoolˉhash);
        Equal(SOURCE_SET_SHA256, Sourceˉsetˉhash);
        Equal(SOURCE_SET_DEMO_SHA256, Sourceˉsetˉdemoˉhash);
        Equal(SOURCE_SET_TOOL_SHA256, Sourceˉsetˉtoolˉhash);
        Equal(SOURCE_GRAPH_SHA256, Sourceˉgraphˉhash);
        Equal(SOURCE_GRAPH_DEMO_SHA256, Sourceˉgraphˉdemoˉhash);
        Equal(SOURCE_GRAPH_TOOL_SHA256, Sourceˉgraphˉtoolˉhash);
        Equal(SOURCE_SYMBOLS_SHA256, Sourceˉsymbolsˉhash);
        Equal(SOURCE_SYMBOLS_DEMO_SHA256, Sourceˉsymbolsˉdemoˉhash);
        Equal(SOURCE_SYMBOLS_TOOL_SHA256, Sourceˉsymbolsˉtoolˉhash);
        Equal(SOURCE_BINDINGS_SHA256, Sourceˉbindingsˉhash);
        Equal(SOURCE_BINDINGS_DEMO_SHA256, Sourceˉbindingsˉdemoˉhash);
        Equal(SOURCE_BINDINGS_TOOL_SHA256, Sourceˉbindingsˉtoolˉhash);
        Equal(WVDUMP_CORE_SHA256, Wvˉdumpˉhash);
        Equal(WVO_CORE_SHA256, Wvoˉcoreˉhash);
        Equal(WVA_ASSEMBLER_CORE_SHA256, Wvaˉassemblerˉhash);
        Equal(WVLINK_CORE_SHA256, Wvˉlinkerˉhash);
        Equal(WVO_SAMPLE_SHA256, Wvoˉsampleˉhash);
        Equal(WVA_OBJECT_SHA256, Assemblyˉobjectˉhash);
        Equal(LINK_IMAGE_SHA256, Linkˉimageˉhash);
        Equal(LINK_MAP_SHA256, Linkˉmapˉhash);
        _ = Objectˉcodec.Readˉandˉverify(Assemblyˉobjectˉbytes);

        Phases.Start("baseline-runtime");
        var Sumˉresult = new Referenceˉruntime(
            Moduleˉcodec.Readˉandˉverify(Sumˉbytes),
            new Referenceˉcapabilityˉhost(new StringWriter()),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        var Helloˉoutput = new StringWriter();
        var Helloˉresult = new Referenceˉruntime(
            Moduleˉcodec.Readˉandˉverify(Helloˉbytes),
            new Referenceˉcapabilityˉhost(Helloˉoutput),
            new(ImmutableHashSet.Create(StringComparer.Ordinal, Capabilityˉcatalog.CONSOLE_WRITE_LINE)))
            .Runˉmain();
        var Normalizedˉhelloˉoutput = Helloˉoutput.ToString()
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
        var Foundationˉresult = new Referenceˉruntime(
            Moduleˉcodec.Readˉandˉverify(Foundationˉbytes),
            new Referenceˉcapabilityˉhost(new StringWriter()),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        var Sourceˉcompositionˉresult = new Referenceˉruntime(
            Moduleˉcodec.Readˉandˉverify(Sourceˉcompositionˉbytes),
            new Referenceˉcapabilityˉhost(new StringWriter()),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        var Machineˉcontractsˉdemoˉresult = new Referenceˉruntime(
            Moduleˉcodec.Readˉandˉverify(Machineˉcontractsˉdemoˉbytes),
            new Referenceˉcapabilityˉhost(new StringWriter()),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        var Byteˉorderingˉdemoˉresult = new Referenceˉruntime(
            Moduleˉcodec.Readˉandˉverify(Byteˉorderingˉdemoˉbytes),
            new Referenceˉcapabilityˉhost(new StringWriter()),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        var Decimalˉparsingˉdemoˉresult = new Referenceˉruntime(
            Moduleˉcodec.Readˉandˉverify(Decimalˉparsingˉdemoˉbytes),
            new Referenceˉcapabilityˉhost(new StringWriter()),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        var Byteˉconstructionˉdemoˉresult = new Referenceˉruntime(
            Moduleˉcodec.Readˉandˉverify(Byteˉconstructionˉdemoˉbytes),
            new Referenceˉcapabilityˉhost(new StringWriter()),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        var Nativeˉstencilˉdemoˉresult = new Referenceˉruntime(
            Moduleˉcodec.Readˉandˉverify(Nativeˉstencilˉdemoˉbytes),
            new Referenceˉcapabilityˉhost(new StringWriter()),
            new(
                Runtimeˉoptions.Portableˉdefaults.Authorizedˉcapabilities,
                Maximumˉinstructions: 20_000_000)).Runˉmain();
        var Sourceˉlexerˉdemoˉresult = new Referenceˉruntime(
            Moduleˉcodec.Readˉandˉverify(Sourceˉlexerˉdemoˉbytes),
            new Referenceˉcapabilityˉhost(new StringWriter()),
            new(Runtimeˉoptions.Portableˉdefaults.Authorizedˉcapabilities,
                Maximumˉinstructions: 10_000_000)).Runˉmain();
        var Sourceˉdeclarationˉparserˉdemoˉresult = new Referenceˉruntime(
            Moduleˉcodec.Readˉandˉverify(Sourceˉdeclarationˉparserˉdemoˉbytes),
            new Referenceˉcapabilityˉhost(new StringWriter()),
            new(Runtimeˉoptions.Portableˉdefaults.Authorizedˉcapabilities,
                Maximumˉinstructions: 20_000_000)).Runˉmain();
        Phases.Addˉexecutedˉinstructions(checked(
            Sumˉresult.Executedˉinstructions +
            Helloˉresult.Executedˉinstructions +
            Foundationˉresult.Executedˉinstructions +
            Sourceˉcompositionˉresult.Executedˉinstructions +
            Machineˉcontractsˉdemoˉresult.Executedˉinstructions +
            Byteˉorderingˉdemoˉresult.Executedˉinstructions +
            Decimalˉparsingˉdemoˉresult.Executedˉinstructions +
            Byteˉconstructionˉdemoˉresult.Executedˉinstructions +
            Nativeˉstencilˉdemoˉresult.Executedˉinstructions +
            Sourceˉlexerˉdemoˉresult.Executedˉinstructions +
            Sourceˉdeclarationˉparserˉdemoˉresult.Executedˉinstructions));

        Phases.Start("parser-closures");
        var Sourceˉdeclarationˉparserˉtool = Moduleˉcodec.Readˉandˉverify(
            Sourceˉdeclarationˉparserˉtoolˉbytes);
        var Sourceˉlexerˉdeclarationˉresult = Runˉsourceˉdeclarationˉparser(
            Sourceˉdeclarationˉparserˉtool,
            "Source-Lexer-Core.wv",
            SOURCE_LEXER_SOURCE,
            30_000_000);
        var Sourceˉparserˉselfˉdeclarationˉresult = Runˉsourceˉdeclarationˉparser(
            Sourceˉdeclarationˉparserˉtool,
            "Source-Declaration-Parser.wv",
            SOURCE_DECLARATION_PARSER_SOURCE,
            45_000_000);
        var Sourceˉbodyˉparserˉdemoˉresult = new Referenceˉruntime(
            Moduleˉcodec.Readˉandˉverify(Sourceˉbodyˉparserˉdemoˉbytes),
            new Referenceˉcapabilityˉhost(new StringWriter()),
            new(Runtimeˉoptions.Portableˉdefaults.Authorizedˉcapabilities,
                Maximumˉinstructions: 30_000_000)).Runˉmain();
        var Sourceˉbodyˉparserˉtool = Moduleˉcodec.Readˉandˉverify(
            Sourceˉbodyˉparserˉtoolˉbytes);
        var Sourceˉlexerˉbodyˉresult = Runˉsourceˉdeclarationˉparser(
            Sourceˉbodyˉparserˉtool,
            "Source-Lexer-Core.wv",
            SOURCE_LEXER_SOURCE,
            100_000_000);
        var Sourceˉdeclarationˉbodyˉresult = Runˉsourceˉdeclarationˉparser(
            Sourceˉbodyˉparserˉtool,
            "Source-Declaration-Parser.wv",
            SOURCE_DECLARATION_PARSER_SOURCE,
            160_000_000);
        var Sourceˉbodyˉselfˉresult = Runˉsourceˉdeclarationˉparser(
            Sourceˉbodyˉparserˉtool,
            "Source-Body-Parser.wv",
            SOURCE_BODY_PARSER_SOURCE,
            160_000_000);
        Phases.Addˉexecutedˉinstructions(checked(
            Sourceˉlexerˉdeclarationˉresult.Executedˉinstructions +
            Sourceˉparserˉselfˉdeclarationˉresult.Executedˉinstructions +
            Sourceˉbodyˉparserˉdemoˉresult.Executedˉinstructions +
            Sourceˉlexerˉbodyˉresult.Executedˉinstructions +
            Sourceˉdeclarationˉbodyˉresult.Executedˉinstructions +
            Sourceˉbodyˉselfˉresult.Executedˉinstructions));

        Phases.Start("source-set-closure");
        var Sourceˉsetˉdemoˉresult = new Referenceˉruntime(
            Moduleˉcodec.Readˉandˉverify(Sourceˉsetˉdemoˉbytes),
            new Referenceˉcapabilityˉhost(new StringWriter()),
            new(Runtimeˉoptions.Portableˉdefaults.Authorizedˉcapabilities,
                Maximumˉinstructions: 200_000_000)).Runˉmain();
        var Sourceˉsetˉtool = Moduleˉcodec.Readˉandˉverify(Sourceˉsetˉtoolˉbytes);
        var Sourceˉsetˉselfˉresult = Runˉsourceˉsetˉtool(
            Sourceˉsetˉtool,
            [
                new("Source-Set-Core.wv", SOURCE_SET_SOURCE),
                new("Source-Body-Parser.wv", SOURCE_BODY_PARSER_SOURCE),
                new("Source-Declaration-Parser.wv", SOURCE_DECLARATION_PARSER_SOURCE),
                new("Source-Lexer-Core.wv", SOURCE_LEXER_SOURCE),
                new("Decimal-Parsing.wv", DECIMAL_PARSING_SOURCE),
            ],
            800_000_000);
        Phases.Addˉexecutedˉinstructions(checked(
            Sourceˉsetˉdemoˉresult.Executedˉinstructions +
            Sourceˉsetˉselfˉresult.Executedˉinstructions));

        Phases.Start("source-graph-closure");
        var Sourceˉgraphˉdemoˉresult = new Referenceˉruntime(
            Moduleˉcodec.Readˉandˉverify(Sourceˉgraphˉdemoˉbytes),
            new Referenceˉcapabilityˉhost(new StringWriter()),
            new(Runtimeˉoptions.Portableˉdefaults.Authorizedˉcapabilities,
                Maximumˉinstructions: 300_000_000)).Runˉmain();
        var Sourceˉgraphˉtool = Moduleˉcodec.Readˉandˉverify(Sourceˉgraphˉtoolˉbytes);
        var Sourceˉgraphˉselfˉresult = Runˉsourceˉsetˉtool(
            Sourceˉgraphˉtool,
            [
                new("Source-Graph-Core.wv", SOURCE_GRAPH_SOURCE),
                new("Source-Body-Parser.wv", SOURCE_BODY_PARSER_SOURCE),
                new("Source-Declaration-Parser.wv", SOURCE_DECLARATION_PARSER_SOURCE),
                new("Source-Lexer-Core.wv", SOURCE_LEXER_SOURCE),
                new("Source-Set-Core.wv", SOURCE_SET_SOURCE),
                new("Byte-Construction.wv", BYTE_CONSTRUCTION_SOURCE),
                new("Decimal-Parsing.wv", DECIMAL_PARSING_SOURCE),
            ],
            1_500_000_000);
        Phases.Addˉexecutedˉinstructions(checked(
            Sourceˉgraphˉdemoˉresult.Executedˉinstructions +
            Sourceˉgraphˉselfˉresult.Executedˉinstructions));

        Phases.Start("source-symbols-closure");
        var Sourceˉsymbolsˉdemoˉresult = new Referenceˉruntime(
            Moduleˉcodec.Readˉandˉverify(Sourceˉsymbolsˉdemoˉbytes),
            new Referenceˉcapabilityˉhost(new StringWriter()),
            new(Runtimeˉoptions.Portableˉdefaults.Authorizedˉcapabilities,
                Maximumˉinstructions: 1_500_000_000)).Runˉmain();
        var Sourceˉsymbolsˉtool = Moduleˉcodec.Readˉandˉverify(Sourceˉsymbolsˉtoolˉbytes);
        var Sourceˉsymbolsˉselfˉresult = Runˉsourceˉsetˉtool(
            Sourceˉsymbolsˉtool,
            [
                new("Source-Symbols-Core.wv", SOURCE_SYMBOLS_SOURCE),
                new("Source-Body-Parser.wv", SOURCE_BODY_PARSER_SOURCE),
                new("Source-Declaration-Parser.wv", SOURCE_DECLARATION_PARSER_SOURCE),
                new("Source-Graph-Core.wv", SOURCE_GRAPH_SOURCE),
                new("Source-Lexer-Core.wv", SOURCE_LEXER_SOURCE),
                new("Source-Set-Core.wv", SOURCE_SET_SOURCE),
                new("Byte-Construction.wv", BYTE_CONSTRUCTION_SOURCE),
                new("Decimal-Parsing.wv", DECIMAL_PARSING_SOURCE),
            ],
            4_000_000_000);
        Phases.Addˉexecutedˉinstructions(checked(
            Sourceˉsymbolsˉdemoˉresult.Executedˉinstructions +
            Sourceˉsymbolsˉselfˉresult.Executedˉinstructions));

        Phases.Start("source-bindings-closure");
        var Sourceˉbindingsˉdemoˉresult = new Referenceˉruntime(
            Moduleˉcodec.Readˉandˉverify(Sourceˉbindingsˉdemoˉbytes),
            new Referenceˉcapabilityˉhost(new StringWriter()),
            new(Runtimeˉoptions.Portableˉdefaults.Authorizedˉcapabilities,
                Maximumˉinstructions: 2_000_000_000)).Runˉmain();
        var Sourceˉbindingsˉtool = Moduleˉcodec.Readˉandˉverify(Sourceˉbindingsˉtoolˉbytes);
        var Sourceˉbindingsˉselfˉresult = Runˉsourceˉsetˉtool(
            Sourceˉbindingsˉtool,
            [
                new("Source-Bindings-Core.wv", SOURCE_BINDINGS_SOURCE),
                new("Source-Body-Parser.wv", SOURCE_BODY_PARSER_SOURCE),
                new("Source-Declaration-Parser.wv", SOURCE_DECLARATION_PARSER_SOURCE),
                new("Source-Graph-Core.wv", SOURCE_GRAPH_SOURCE),
                new("Source-Lexer-Core.wv", SOURCE_LEXER_SOURCE),
                new("Source-Set-Core.wv", SOURCE_SET_SOURCE),
                new("Source-Symbols-Core.wv", SOURCE_SYMBOLS_SOURCE),
                new("Byte-Construction.wv", BYTE_CONSTRUCTION_SOURCE),
                new("Decimal-Parsing.wv", DECIMAL_PARSING_SOURCE),
            ],
            4_000_000_000);
        Phases.Addˉexecutedˉinstructions(checked(
            Sourceˉbindingsˉdemoˉresult.Executedˉinstructions +
            Sourceˉbindingsˉselfˉresult.Executedˉinstructions));

        Phases.Start("inspection-tools");
        var Wvˉdumpˉmodule = Moduleˉcodec.Readˉandˉverify(Wvˉdumpˉbytes);
        var Wvˉdumpˉcapabilities = Wvˉdumpˉmodule.Module.Capabilities
            .Select(Capability => Capability.Name)
            .ToImmutableHashSet(StringComparer.Ordinal);
        var Wvˉdumpˉresult = new Referenceˉruntime(
            Wvˉdumpˉmodule,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                [],
                TextWriter.Null,
                TextWriter.Null,
                new Testˉfileˉreader((_, _) => throw new InvalidOperationException(
                    "The golden WvDump self-test must not read a hosted file.")))),
            new(Wvˉdumpˉcapabilities)).Runˉmain();
        var Wvˉdumpˉhostedˉoutput = new StringWriter();
        var Wvˉdumpˉhostedˉdiagnostics = new StringWriter();
        var Wvˉdumpˉhostedˉresult = new Referenceˉruntime(
            Wvˉdumpˉmodule,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                ["sum.wvb"],
                Wvˉdumpˉhostedˉoutput,
                Wvˉdumpˉhostedˉdiagnostics,
                new Testˉfileˉreader((Name, Maximumˉbytes) =>
                {
                    Equal("sum.wvb", Name);
                    True(Sumˉbytes.Length <= Maximumˉbytes, "The golden WvDump byte limit was too small.");
                    return Sumˉbytes.ToImmutableArray();
                }))),
            new(Wvˉdumpˉcapabilities, Maximumˉinstructions: 10_000_000)).Runˉmain();
        var Normalizedˉwvdumpˉoutput = Wvˉdumpˉhostedˉoutput.ToString()
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
        var Wvoˉmodule = Moduleˉcodec.Readˉandˉverify(Wvoˉcoreˉbytes);
        var Wvoˉcapabilities = Wvoˉmodule.Module.Capabilities
            .Select(Capability => Capability.Name)
            .ToImmutableHashSet(StringComparer.Ordinal);
        var Wvoˉselfˉtestˉwriter = new Capturingˉfileˉwriter();
        var Wvoˉselfˉtestˉresult = new Referenceˉruntime(
            Wvoˉmodule,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                [],
                TextWriter.Null,
                TextWriter.Null,
                null,
                Wvoˉselfˉtestˉwriter)),
            new(Wvoˉcapabilities, Maximumˉinstructions: 10_000_000)).Runˉmain();
        var Wvoˉhostedˉwriter = new Capturingˉfileˉwriter();
        var Wvoˉhostedˉoutput = new StringWriter();
        var Wvoˉhostedˉdiagnostics = new StringWriter();
        var Wvoˉhostedˉresult = new Referenceˉruntime(
            Wvoˉmodule,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                ["sample.wvo"],
                Wvoˉhostedˉoutput,
                Wvoˉhostedˉdiagnostics,
                null,
                Wvoˉhostedˉwriter)),
            new(Wvoˉcapabilities, Maximumˉinstructions: 10_000_000)).Runˉmain();
        var Normalizedˉwvoˉoutput = Wvoˉhostedˉoutput.ToString()
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
        Phases.Addˉexecutedˉinstructions(checked(
            Wvˉdumpˉresult.Executedˉinstructions +
            Wvˉdumpˉhostedˉresult.Executedˉinstructions +
            Wvoˉselfˉtestˉresult.Executedˉinstructions +
            Wvoˉhostedˉresult.Executedˉinstructions));

        Phases.Start("assembler-closure");
        var Wvaˉassemblerˉmodule = Moduleˉcodec.Readˉandˉverify(Wvaˉassemblerˉbytes);
        var Wvaˉassemblerˉcapabilities = Wvaˉassemblerˉmodule.Module.Capabilities
            .Select(Capability => Capability.Name)
            .ToImmutableHashSet(StringComparer.Ordinal);
        var Wvaˉassemblerˉselfˉtestˉwriter = new Capturingˉfileˉwriter();
        var Wvaˉassemblerˉselfˉtestˉresult = new Referenceˉruntime(
            Wvaˉassemblerˉmodule,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                [],
                TextWriter.Null,
                TextWriter.Null,
                new Testˉfileˉreader((_, _) => throw new InvalidOperationException(
                    "The golden WVA assembler self-test must not read a hosted file.")),
                Wvaˉassemblerˉselfˉtestˉwriter)),
            new(Wvaˉassemblerˉcapabilities, Maximumˉinstructions: 10_000_000)).Runˉmain();
        var Wvaˉassemblerˉwriter = new Capturingˉfileˉwriter();
        var Wvaˉassemblerˉhostedˉoutput = new StringWriter();
        var Wvaˉassemblerˉhostedˉdiagnostics = new StringWriter();
        var Wvaˉsourceˉbytes = System.Text.Encoding.UTF8.GetBytes(HELLO_ASSEMBLY_SOURCE);
        var Wvaˉassemblerˉhostedˉresult = new Referenceˉruntime(
            Wvaˉassemblerˉmodule,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                ["hello.wva", "hello.wvo"],
                Wvaˉassemblerˉhostedˉoutput,
                Wvaˉassemblerˉhostedˉdiagnostics,
                new Testˉfileˉreader((Name, Maximumˉbytes) =>
                {
                    Equal("hello.wva", Name);
                    True(Wvaˉsourceˉbytes.Length <= Maximumˉbytes, "The golden WVA source limit was too small.");
                    return Wvaˉsourceˉbytes.ToImmutableArray();
                }),
                Wvaˉassemblerˉwriter)),
            new(Wvaˉassemblerˉcapabilities, Maximumˉinstructions: 10_000_000)).Runˉmain();
        var Normalizedˉwvaˉassemblerˉoutput = Wvaˉassemblerˉhostedˉoutput.ToString()
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
        var Wvaˉassemblerˉobjectˉhash = Objectˉdigest.Calculateˉsha256(
            Wvaˉassemblerˉwriter.Bytes.AsSpan());
        Phases.Addˉexecutedˉinstructions(checked(
            Wvaˉassemblerˉselfˉtestˉresult.Executedˉinstructions +
            Wvaˉassemblerˉhostedˉresult.Executedˉinstructions));

        Phases.Start("linker-closure");
        var Wvˉlinkerˉmodule = Moduleˉcodec.Readˉandˉverify(Wvˉlinkerˉbytes);
        var Wvˉlinkerˉcapabilities = Wvˉlinkerˉmodule.Module.Capabilities
            .Select(Capability => Capability.Name)
            .ToImmutableHashSet(StringComparer.Ordinal);
        var Wvˉlinkerˉselfˉtestˉresult = new Referenceˉruntime(
            Wvˉlinkerˉmodule,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                [],
                TextWriter.Null,
                TextWriter.Null,
                new Testˉfileˉreader((_, _) => throw new InvalidOperationException(
                    "The golden Windvale linker self-test must not read a hosted file.")),
                new Capturingˉfileˉwriter())),
            new(Wvˉlinkerˉcapabilities, Maximumˉinstructions: 20_000_000)).Runˉmain();
        var Wvˉlinkerˉhosted = Runˉwvˉlinkerˉscan(
            Wvˉlinkerˉmodule,
            Assemblyˉobjectˉbytes.ToImmutableArray());
        var Normalizedˉwvˉlinkerˉoutput = Wvˉlinkerˉhosted.Output
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
        var Wvˉlinkerˉanalysis = Runˉwvˉlinkerˉanalysis(
            Wvˉlinkerˉmodule,
            Linkˉcontract.DEFAULT_BASE_ADDRESS.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "Main",
            Assemblyˉobjectˉbytes.ToImmutableArray(),
            Providerˉobjectˉbytes.ToImmutableArray());
        var Normalizedˉwvˉlinkerˉanalysisˉoutput = Wvˉlinkerˉanalysis.Output
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
        Phases.Addˉexecutedˉinstructions(checked(
            Wvˉlinkerˉselfˉtestˉresult.Executedˉinstructions +
            Wvˉlinkerˉhosted.Executedˉinstructions +
            Wvˉlinkerˉanalysis.Executedˉinstructions));

        Phases.Start("contract-assembly");
        Equal(29, Sumˉresult.Exitˉcode);
        Equal("Hello from Windvale\n", Normalizedˉhelloˉoutput);
        Equal(0, Helloˉresult.Exitˉcode);
        Equal(1, Foundationˉresult.Exitˉcode);
        Equal(42, Sourceˉcompositionˉresult.Exitˉcode);
        Equal(0, Machineˉcontractsˉdemoˉresult.Exitˉcode);
        Equal(0, Byteˉorderingˉdemoˉresult.Exitˉcode);
        Equal(0, Nativeˉstencilˉdemoˉresult.Exitˉcode);
        Equal(0, Wvˉdumpˉresult.Exitˉcode);
        Equal(0, Wvˉdumpˉhostedˉresult.Exitˉcode);
        Equal(string.Empty, Wvˉdumpˉhostedˉdiagnostics.ToString());
        Contains(Normalizedˉwvdumpˉoutput, "module version=1.6 profile=portable name=\"Sum\\u02C9data\"");
        Contains(Normalizedˉwvdumpˉoutput, "instruction function=1 offset=141 opcode=call operand=0");
        Contains(Normalizedˉwvdumpˉoutput, "export index=0 name=\"Main\" kind=function target=1");
        Equal(0, Wvoˉselfˉtestˉresult.Exitˉcode);
        Equal(0, Wvoˉselfˉtestˉwriter.Writeˉcount);
        Equal(0, Wvoˉhostedˉresult.Exitˉcode);
        Equal("Wrote WVO 1.0 bytes=189\n", Normalizedˉwvoˉoutput);
        Equal(string.Empty, Wvoˉhostedˉdiagnostics.ToString());
        Sequenceˉequal(Wvoˉsampleˉbytes, Wvoˉhostedˉwriter.Bytes);
        Equal(0, Wvaˉassemblerˉselfˉtestˉresult.Exitˉcode);
        Equal(0, Wvaˉassemblerˉselfˉtestˉwriter.Writeˉcount);
        Equal(0, Wvaˉassemblerˉhostedˉresult.Exitˉcode);
        Equal(
            "wvasm 1\n" +
            "assembly status=valid object-bytes=218 sections=2 symbols=3 relocations=2 offset=403 line=22 column=1\n",
            Normalizedˉwvaˉassemblerˉoutput);
        Equal(string.Empty, Wvaˉassemblerˉhostedˉdiagnostics.ToString());
        Equal(1, Wvaˉassemblerˉwriter.Writeˉcount);
        Equal("hello.wvo", Wvaˉassemblerˉwriter.Resourceˉname);
        Sequenceˉequal(Assemblyˉobjectˉbytes, Wvaˉassemblerˉwriter.Bytes);
        Equal(Assemblyˉobjectˉhash, Wvaˉassemblerˉobjectˉhash);
        _ = Objectˉcodec.Readˉandˉverify(Wvaˉassemblerˉwriter.Bytes.AsSpan());
        Equal(0, Wvˉlinkerˉselfˉtestˉresult.Exitˉcode);
        Equal(0, Wvˉlinkerˉhosted.Exitˉcode);
        Equal(
            "object status=Valid sections=2 symbols=3 relocations=2 offset=218\n",
            Normalizedˉwvˉlinkerˉoutput);
        Equal(string.Empty, Wvˉlinkerˉhosted.Diagnostics);
        Equal(0, Wvˉlinkerˉanalysis.Exitˉcode);
        Equal(Linkˉmap, Normalizedˉwvˉlinkerˉanalysisˉoutput);
        Equal(string.Empty, Wvˉlinkerˉanalysis.Diagnostics);
        Equal(2, Wvˉlinkerˉanalysis.Readˉcount);
        Equal(1, Wvˉlinkerˉanalysis.Writeˉcount);
        Equal("output.bin", Wvˉlinkerˉanalysis.Writtenˉresourceˉname);
        Sequenceˉequal(Linkˉresult.Imageˉbytes, Wvˉlinkerˉanalysis.Writtenˉbytes);
        Equal(0, Sourceˉlexerˉdemoˉresult.Exitˉcode);
        Equal(0, Sourceˉdeclarationˉparserˉdemoˉresult.Exitˉcode);
        Equal(0, Sourceˉlexerˉdeclarationˉresult.Exitˉcode);
        Equal(string.Empty, Sourceˉlexerˉdeclarationˉresult.Diagnostics);
        Equal(1, Sourceˉlexerˉdeclarationˉresult.Readˉcount);
        Equal(0, Sourceˉparserˉselfˉdeclarationˉresult.Exitˉcode);
        Equal(string.Empty, Sourceˉparserˉselfˉdeclarationˉresult.Diagnostics);
        Equal(1, Sourceˉparserˉselfˉdeclarationˉresult.Readˉcount);
        Equal(0, Sourceˉbodyˉparserˉdemoˉresult.Exitˉcode);
        Equal(0, Sourceˉlexerˉbodyˉresult.Exitˉcode);
        Equal(string.Empty, Sourceˉlexerˉbodyˉresult.Diagnostics);
        Equal(1, Sourceˉlexerˉbodyˉresult.Readˉcount);
        Equal(0, Sourceˉdeclarationˉbodyˉresult.Exitˉcode);
        Equal(string.Empty, Sourceˉdeclarationˉbodyˉresult.Diagnostics);
        Equal(1, Sourceˉdeclarationˉbodyˉresult.Readˉcount);
        Equal(0, Sourceˉbodyˉselfˉresult.Exitˉcode);
        Equal(string.Empty, Sourceˉbodyˉselfˉresult.Diagnostics);
        Equal(1, Sourceˉbodyˉselfˉresult.Readˉcount);
        Equal(0, Sourceˉsetˉdemoˉresult.Exitˉcode);
        Equal(0, Sourceˉsetˉselfˉresult.Exitˉcode);
        Equal(string.Empty, Sourceˉsetˉselfˉresult.Diagnostics);
        Equal(5, Sourceˉsetˉselfˉresult.Readˉcount);
        Equal(
            "source set status=Valid modules=5 source-bytes=205658 imports=4 records=16 enums=11 functions=92\n",
            Sourceˉsetˉselfˉresult.Output);
        Equal(0, Sourceˉgraphˉdemoˉresult.Exitˉcode);
        Equal(0, Sourceˉgraphˉselfˉresult.Exitˉcode);
        Equal(string.Empty, Sourceˉgraphˉselfˉresult.Diagnostics);
        Equal(7, Sourceˉgraphˉselfˉresult.Readˉcount);
        Equal(
            "source graph status=Valid modules=7 imports=6 reachable=7\n",
            Sourceˉgraphˉselfˉresult.Output);
        Equal(0, Sourceˉsymbolsˉdemoˉresult.Exitˉcode);
        Equal(0, Sourceˉsymbolsˉselfˉresult.Exitˉcode);
        Equal(string.Empty, Sourceˉsymbolsˉselfˉresult.Diagnostics);
        Equal(8, Sourceˉsymbolsˉselfˉresult.Readˉcount);
        Equal(
            "source symbols status=Valid modules=8 capabilities=0 data=0 records=24 enums=14 functions=141 fields=291 members=181 parameters=619 directory-bytes=4312 visibility-bytes=64\n",
            Sourceˉsymbolsˉselfˉresult.Output);
        Equal(0, Sourceˉbindingsˉdemoˉresult.Exitˉcode);
        Equal(0, Sourceˉbindingsˉselfˉresult.Exitˉcode);
        Equal(string.Empty, Sourceˉbindingsˉselfˉresult.Diagnostics);
        Equal(9, Sourceˉbindingsˉselfˉresult.Readˉcount);
        Equal(
            "source bindings status=Valid modules=9 functions=195 parameters=849 locals=1018 reads=8642 assignments=684 calls=1540 directory-bytes=69172\n",
            Sourceˉbindingsˉselfˉresult.Output);
        Contract = new(
            $"{Moduleˉcodec.MAJOR_VERSION}.{Moduleˉcodec.MINOR_VERSION}",
            $"{Objectˉcodec.MAJOR_VERSION}.{Objectˉcodec.MINOR_VERSION}",
            Assemblyˉcompiler.FORMAT_VERSION.ToString(),
            Assemblyˉobjectˉhash,
            Wvaˉassemblerˉhash,
            Wvaˉassemblerˉselfˉtestˉresult.Exitˉcode,
            Normalizedˉwvaˉassemblerˉoutput,
            Wvaˉassemblerˉobjectˉhash,
            Wvˉlinkerˉhash,
            Wvˉlinkerˉselfˉtestˉresult.Exitˉcode,
            Normalizedˉwvˉlinkerˉoutput,
            Normalizedˉwvˉlinkerˉanalysisˉoutput,
            Linkˉcontract.FORMAT_VERSION.ToString(),
            Linkˉimageˉhash,
            Linkˉmapˉhash,
            Linkˉmap,
            Sumˉhash,
            Sumˉresult.Exitˉcode,
            Helloˉhash,
            Normalizedˉhelloˉoutput,
            Helloˉresult.Exitˉcode,
            Foundationˉhash,
            Foundationˉresult.Exitˉcode,
            Sourceˉcompositionˉhash,
            Sourceˉcompositionˉresult.Exitˉcode,
            Machineˉcontractsˉhash,
            Machineˉcontractsˉdemoˉhash,
            Machineˉcontractsˉdemoˉresult.Exitˉcode,
            Byteˉorderingˉhash,
            Byteˉorderingˉdemoˉhash,
            Byteˉorderingˉdemoˉresult.Exitˉcode,
            Decimalˉparsingˉhash,
            Decimalˉparsingˉdemoˉhash,
            Decimalˉparsingˉdemoˉresult.Exitˉcode,
            Byteˉconstructionˉhash,
            Byteˉconstructionˉdemoˉhash,
            Byteˉconstructionˉdemoˉresult.Exitˉcode,
            Nativeˉstencilˉcoreˉhash,
            Nativeˉstencilˉdemoˉhash,
            Nativeˉstencilˉdemoˉresult.Exitˉcode,
            Sourceˉlexerˉhash,
            Sourceˉlexerˉdemoˉhash,
            Sourceˉlexerˉdemoˉresult.Exitˉcode,
            Sourceˉdeclarationˉparserˉhash,
            Sourceˉdeclarationˉparserˉdemoˉhash,
            Sourceˉdeclarationˉparserˉdemoˉresult.Exitˉcode,
            Sourceˉdeclarationˉparserˉtoolˉhash,
            Sourceˉlexerˉdeclarationˉresult.Output,
            Sourceˉparserˉselfˉdeclarationˉresult.Output,
            Sourceˉbodyˉparserˉhash,
            Sourceˉbodyˉparserˉdemoˉhash,
            Sourceˉbodyˉparserˉdemoˉresult.Exitˉcode,
            Sourceˉbodyˉparserˉtoolˉhash,
            Sourceˉlexerˉbodyˉresult.Output,
            Sourceˉdeclarationˉbodyˉresult.Output,
            Sourceˉbodyˉselfˉresult.Output,
            Sourceˉsetˉhash,
            Sourceˉsetˉdemoˉhash,
            Sourceˉsetˉdemoˉresult.Exitˉcode,
            Sourceˉsetˉtoolˉhash,
            Sourceˉsetˉselfˉresult.Output,
            Sourceˉgraphˉhash,
            Sourceˉgraphˉdemoˉhash,
            Sourceˉgraphˉdemoˉresult.Exitˉcode,
            Sourceˉgraphˉtoolˉhash,
            Sourceˉgraphˉselfˉresult.Output,
            Sourceˉsymbolsˉhash,
            Sourceˉsymbolsˉdemoˉhash,
            Sourceˉsymbolsˉdemoˉresult.Exitˉcode,
            Sourceˉsymbolsˉtoolˉhash,
            Sourceˉsymbolsˉselfˉresult.Output,
            Sourceˉbindingsˉhash,
            Sourceˉbindingsˉdemoˉhash,
            Sourceˉbindingsˉdemoˉresult.Exitˉcode,
            Sourceˉbindingsˉtoolˉhash,
            Sourceˉbindingsˉselfˉresult.Output,
            Wvˉdumpˉhash,
            Wvˉdumpˉresult.Exitˉcode,
            Normalizedˉwvdumpˉoutput,
            Wvoˉsampleˉhash,
            Wvoˉcoreˉhash,
            Wvoˉselfˉtestˉresult.Exitˉcode,
            Normalizedˉwvoˉoutput);
    }

    private static void Randomˉinputˉisˉcontained()
    {
        const string Sourceˉalphabet =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789" +
            "{}[]();:,.+-*!<>=_ˉ \t\r\n\\\"";
        var Random = new Random(0x57_56_42);
        for (var Case = 0; Case < 500; Case++)
        {
            var Length = Random.Next(0, 512);
            var Characters = new char[Length];
            for (var Index = 0; Index < Characters.Length; Index++)
            {
                Characters[Index] = Sourceˉalphabet[Random.Next(Sourceˉalphabet.Length)];
            }

            _ = Seedˉcompiler.Compile(new string(Characters), $"fuzz-{Case}.wv");
            _ = Assemblyˉcompiler.Assemble(new string(Characters));
        }

        for (var Case = 0; Case < 1000; Case++)
        {
            var Bytes = new byte[Random.Next(0, 512)];
            Random.NextBytes(Bytes);
            try
            {
                _ = Moduleˉcodec.Readˉandˉverify(Bytes);
            }
            catch (Bytecodeˉexception)
            {
                // Rejection through the stable bytecode boundary is the expected result.
            }
        }

        for (var Case = 0; Case < 500; Case++)
        {
            var Bytes = new byte[Random.Next(0, 512)];
            Random.NextBytes(Bytes);
            try
            {
                _ = Objectˉcodec.Readˉandˉverify(Bytes);
            }
            catch (Objectˉexception)
            {
                // Rejection through the stable object boundary is the expected result.
            }
        }
    }

    private static void Projectˉparserˉagreesˉwithˉtool(
        Verifiedˉmodule tool,
        string manifest)
    {
        var Reference = Projectˉparser.Parse(manifest);
        var Run = Runˉprojectˉmanifestˉtool(
            tool,
            Encoding.UTF8.GetBytes(manifest).ToImmutableArray());
        Equal(1, Run.Readˉcount);

        if (!Reference.Success)
        {
            var Diagnostic = Reference.Diagnostics.Single();
            Equal(1, Run.Exitˉcode);
            Equal(string.Empty, Run.Output);
            Equal(
                $"project status={Diagnostic.Code} line={Diagnostic.Line} " +
                    $"column={Diagnostic.Column}\n",
                Run.Diagnostics);
            return;
        }

        Equal(0, Run.Exitˉcode);
        Equal(string.Empty, Run.Diagnostics);
        var Manifest = Reference.Manifest!;
        var Paths = new[] { Manifest.Root }.Concat(Manifest.Sources);
        var Expected = new StringBuilder(
            $"project status=Valid modules={Manifest.Sources.Length + 1}\n");
        var Index = 0;
        foreach (var Path in Paths)
        {
            Expected.AppendLine(
                $"project path={Index} line={Path.Line} column={Path.Column} " +
                    $"value=\"{Path.Value}\"");
            Index++;
        }
        Equal(Expected.ToString().Replace("\r\n", "\n", StringComparison.Ordinal), Run.Output);
    }

    private static Compilerˉsourceˉparserˉrunˉresult Runˉprojectˉmanifestˉtool(
        Verifiedˉmodule module,
        ImmutableArray<byte> manifest)
    {
        const string Manifestˉname = "input.wvproj";
        var Output = new StringWriter();
        var Diagnostics = new StringWriter();
        var Reader = new Testˉfileˉreader((Name, Maximumˉbytes) =>
        {
            Equal(Manifestˉname, Name);
            True(
                manifest.Length <= Maximumˉbytes,
                "The hosted project-manifest byte limit was too small.");
            return manifest;
        });
        var Authorized = module.Module.Capabilities
            .Select(Capability => Capability.Name)
            .ToImmutableHashSet(StringComparer.Ordinal);
        var Result = new Referenceˉruntime(
            module,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                [Manifestˉname],
                Output,
                Diagnostics,
                Reader)),
            new(Authorized, Maximumˉinstructions: 20_000_000)).Runˉmain();
        return new(
            Result.Exitˉcode,
            Output.ToString().Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n'),
            Diagnostics.ToString().Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n'),
            Reader.Readˉcount,
            Result.Executedˉinstructions);
    }

    private static Compilerˉsourceˉparserˉrunˉresult Runˉsourceˉdeclarationˉparser(
        Verifiedˉmodule module,
        string sourceˉname,
        string source,
        long maximumˉinstructions)
    {
        var Sourceˉbytes = System.Text.Encoding.UTF8.GetBytes(source).ToImmutableArray();
        var Output = new StringWriter();
        var Diagnostics = new StringWriter();
        var Reader = new Testˉfileˉreader((Name, Maximumˉbytes) =>
        {
            Equal(sourceˉname, Name);
            True(
                Sourceˉbytes.Length <= Maximumˉbytes,
                "The hosted source-parser byte limit was too small.");
            return Sourceˉbytes;
        });
        var Authorized = module.Module.Capabilities
            .Select(Capability => Capability.Name)
            .ToImmutableHashSet(StringComparer.Ordinal);
        var Result = new Referenceˉruntime(
            module,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                [sourceˉname],
                Output,
                Diagnostics,
                Reader)),
            new(Authorized, Maximumˉinstructions: maximumˉinstructions)).Runˉmain();
        return new(
            Result.Exitˉcode,
            Output.ToString().Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n'),
            Diagnostics.ToString().Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n'),
            Reader.Readˉcount,
            Result.Executedˉinstructions);
    }

    private static Compilerˉsourceˉparserˉrunˉresult Runˉsourceˉsetˉtool(
        Verifiedˉmodule module,
        IReadOnlyList<Sourceˉmoduleˉinput> sources,
        long maximumˉinstructions)
    {
        var Sourceˉbytes = sources.ToDictionary(
            Source => Source.Sourceˉname,
            Source => System.Text.Encoding.UTF8.GetBytes(Source.Source).ToImmutableArray(),
            StringComparer.Ordinal);
        var Output = new StringWriter();
        var Diagnostics = new StringWriter();
        var Reader = new Testˉfileˉreader((Name, Maximumˉbytes) =>
        {
            True(Sourceˉbytes.TryGetValue(Name, out var Bytes),
                $"The source-set tool requested unexpected source '{Name}'.");
            True(Bytes.Length <= Maximumˉbytes,
                "The hosted source-set byte limit was too small.");
            return Bytes;
        });
        var Authorized = module.Module.Capabilities
            .Select(Capability => Capability.Name)
            .ToImmutableHashSet(StringComparer.Ordinal);
        var Result = new Referenceˉruntime(
            module,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                [.. sources.Select(Source => Source.Sourceˉname)],
                Output,
                Diagnostics,
                Reader)),
            new(Authorized, Maximumˉinstructions: maximumˉinstructions)).Runˉmain();
        return new(
            Result.Exitˉcode,
            Output.ToString().Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n'),
            Diagnostics.ToString().Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n'),
            Reader.Readˉcount,
            Result.Executedˉinstructions);
    }

    private static Wvˉlinkerˉscanˉresult Runˉwvˉlinkerˉscan(
        Verifiedˉmodule module,
        ImmutableArray<byte> objectˉbytes)
    {
        var Output = new StringWriter();
        var Diagnostics = new StringWriter();
        var Authorized = module.Module.Capabilities
            .Select(Capability => Capability.Name)
            .ToImmutableHashSet(StringComparer.Ordinal);
        var Result = new Referenceˉruntime(
            module,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                ["input.wvo"],
                Output,
                Diagnostics,
                new Testˉfileˉreader((Name, Maximumˉbytes) =>
                {
                    Equal("input.wvo", Name);
                    True(
                        objectˉbytes.Length <= Maximumˉbytes,
                        "The hosted WVO byte limit was too small.");
                    return objectˉbytes;
                }),
                new Capturingˉfileˉwriter())),
            new(Authorized, Maximumˉinstructions: 20_000_000)).Runˉmain();
        return new(
            Result.Exitˉcode,
            Output.ToString(),
            Diagnostics.ToString(),
            Result.Executedˉinstructions);
    }

    private static Wvˉlinkerˉanalysisˉresult Runˉwvˉlinkerˉanalysis(
        Verifiedˉmodule module,
        string baseˉaddress,
        string entry,
        params ImmutableArray<byte>[] objects)
    {
        return Runˉwvˉlinkerˉanalysisˉwithˉlimit(
            module,
            baseˉaddress,
            entry,
            20_000_000,
            objects);
    }

    private static Wvˉlinkerˉanalysisˉresult Runˉwvˉlinkerˉanalysisˉwithˉlimit(
        Verifiedˉmodule module,
        string baseˉaddress,
        string entry,
        int maximumˉinstructions,
        params ImmutableArray<byte>[] objects)
    {
        var Arguments = ImmutableArray.CreateBuilder<string>(objects.Length + 3);
        Arguments.Add(baseˉaddress);
        Arguments.Add(entry);
        Arguments.Add("output.bin");
        var Resources = new Dictionary<string, ImmutableArray<byte>>(StringComparer.Ordinal);
        for (var Index = 0; Index < objects.Length; Index++)
        {
            var Name = $"input-{Index.ToString(System.Globalization.CultureInfo.InvariantCulture)}.wvo";
            Arguments.Add(Name);
            Resources.Add(Name, objects[Index]);
        }

        var Output = new StringWriter();
        var Diagnostics = new StringWriter();
        var Reader = new Testˉfileˉreader((Name, Maximumˉbytes) =>
        {
            True(Resources.TryGetValue(Name, out var Bytes), $"Unknown linker resource '{Name}'.");
            True(Bytes.Length <= Maximumˉbytes, "The hosted linker object limit was too small.");
            return Bytes;
        });
        var Writer = new Capturingˉfileˉwriter();
        var Authorized = module.Module.Capabilities
            .Select(Capability => Capability.Name)
            .ToImmutableHashSet(StringComparer.Ordinal);
        var Result = new Referenceˉruntime(
            module,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                Arguments.ToImmutable(),
                Output,
                Diagnostics,
                Reader,
                Writer)),
            new(Authorized, Maximumˉinstructions: maximumˉinstructions)).Runˉmain();
        return new(
            Result.Exitˉcode,
            Output.ToString(),
            Diagnostics.ToString(),
            Reader.Readˉcount,
            Writer.Writeˉcount,
            Writer.Resourceˉname,
            Writer.Bytes,
            Result.Executedˉinstructions);
    }

    private static bool Objectˉisˉvalid(ImmutableArray<byte> objectˉbytes)
    {
        try
        {
            _ = Objectˉcodec.Readˉandˉverify(objectˉbytes.AsSpan());
            return true;
        }
        catch (Objectˉexception)
        {
            return false;
        }
    }

    private static WebAssemblyˉtoolˉresult Runˉwebassemblyˉtool(
        Verifiedˉmodule module,
        IEnumerable<byte> input)
    {
        var Input = input.ToImmutableArray();
        var Output = new StringWriter();
        var Diagnostics = new StringWriter();
        var Reader = new Testˉfileˉreader((Name, Maximumˉbytes) =>
        {
            Equal("input.wvb", Name);
            True(Input.Length <= Maximumˉbytes, "The WebAssembly input limit was too small.");
            return Input;
        });
        var Writer = new Capturingˉfileˉwriter();
        var Authorized = module.Module.Capabilities
            .Select(Capability => Capability.Name)
            .ToImmutableHashSet(StringComparer.Ordinal);
        var Result = new Referenceˉruntime(
            module,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                ["input.wvb", "output.wasm"],
                Output,
                Diagnostics,
                Reader,
                Writer)),
            new(Authorized, Maximumˉinstructions: 100_000_000)).Runˉmain();
        return new(
            Result.Exitˉcode,
            Output.ToString().Replace("\r\n", "\n", StringComparison.Ordinal),
            Diagnostics.ToString().Replace("\r\n", "\n", StringComparison.Ordinal),
            Reader.Readˉcount,
            Writer.Writeˉcount,
            Writer.Resourceˉname,
            Writer.Bytes,
            Result.Executedˉinstructions);
    }

    private static WebAssemblyˉexecutionˉresult Runˉreferenceˉwebassemblyˉi32(
        IEnumerable<byte> input)
    {
        var Verified = Moduleˉcodec.Readˉandˉverify(input.ToArray());
        var Runtime = new Referenceˉruntime(
            Verified,
            new Referenceˉcapabilityˉhost(new StringWriter()),
            Runtimeˉoptions.Portableˉdefaults with { Collectˉfunctionˉsteps = true });
        try
        {
            var Result = Runtime.Runˉmain();
            Equal(
                Result.Executedˉinstructions,
                Runtime.Readˉfunctionˉsteps().Sum(Item => Item.Executedˉinstructions));
            return new(0, Result.Exitˉcode, Result.Executedˉinstructions);
        }
        catch (Runtimeˉexception Exception) when (Exception.Code == "WVR3007")
        {
            var Steps = Runtime.Readˉfunctionˉsteps()
                .Sum(Item => Item.Executedˉinstructions);
            return new(3007, 0, Steps);
        }
    }

    private static WebAssemblyˉexecutionˉresult Executeˉstraightˉi32ˉwebassembly(
        ReadOnlySpan<byte> module,
        Verifiedˉmodule source)
    {
        Equal(1, source.Functions.Length);
        var Function = source.Functions[0];
        Equal("Main", Function.Declaration.Name);
        Equal(Valueˉtype.I32, Function.Declaration.Returnˉtype.Kind);
        True(
            Function.Declaration.Localˉtypes.All(Type => Type.Kind == Valueˉtype.I32),
            "The straight-i32 fixture contains a non-i32 local.");

        var Reader = new WebAssemblyˉtestˉreader(module);
        Reader.Readˉheader();

        var Typeˉend = Reader.Readˉsection(1);
        Reader.Require(Reader.Readˉuleb32() == 1, "The straight-i32 type count is invalid.");
        Reader.Require(Reader.Readˉbyte() == 0x60, "The straight-i32 function type is invalid.");
        Reader.Require(Reader.Readˉuleb32() == 0, "The straight-i32 parameter count is invalid.");
        Reader.Require(Reader.Readˉuleb32() == 1, "The straight-i32 result count is invalid.");
        Reader.Require(Reader.Readˉbyte() == 0x7F, "The straight-i32 result type is invalid.");
        Reader.Require(Reader.Position == Typeˉend, "The straight-i32 type section has trailing bytes.");

        var Functionˉend = Reader.Readˉsection(3);
        Reader.Require(Reader.Readˉuleb32() == 1, "The straight-i32 function count is invalid.");
        Reader.Require(Reader.Readˉuleb32() == 0, "The straight-i32 type index is invalid.");
        Reader.Require(Reader.Position == Functionˉend, "The straight-i32 function section has trailing bytes.");

        var Globalˉend = Reader.Readˉsection(6);
        Reader.Require(Reader.Readˉuleb32() == 3, "The straight-i32 global count is invalid.");
        Reader.Readˉglobal(0, 1);
        Reader.Readˉglobal(1, 0);
        Reader.Readˉglobal(1, 0);
        Reader.Require(Reader.Position == Globalˉend, "The straight-i32 global section has trailing bytes.");

        var Exportˉend = Reader.Readˉsection(7);
        Reader.Require(Reader.Readˉuleb32() == 4, "The straight-i32 export count is invalid.");
        Reader.Readˉexport("Windvale.run", 0, 0);
        Reader.Readˉexport("Windvale.abi", 3, 0);
        Reader.Readˉexport("Windvale.result", 3, 1);
        Reader.Readˉexport("Windvale.instructions", 3, 2);
        Reader.Require(Reader.Position == Exportˉend, "The straight-i32 export section has trailing bytes.");

        var Codeˉend = Reader.Readˉsection(10);
        Reader.Require(Reader.Readˉuleb32() == 1, "The straight-i32 body count is invalid.");
        var Bodyˉlength = Reader.Readˉuleb32();
        Reader.Require(Bodyˉlength <= int.MaxValue, "The straight-i32 body is oversized.");
        Reader.Require(
            Reader.Position <= Codeˉend - (int)Bodyˉlength,
            "The straight-i32 body is truncated.");
        var Bodyˉend = Reader.Position + (int)Bodyˉlength;

        var Localˉcount = Function.Declaration.Localˉtypes.Length;
        var Scratchˉleft = (uint)Localˉcount;
        var Scratchˉright = Scratchˉleft + 1;
        var Scratchˉresult = Scratchˉleft + 2;
        var Scratchˉwide = Scratchˉleft + 3;
        Reader.Require(Reader.Readˉuleb32() == 2, "The straight-i32 local group count is invalid.");
        Reader.Require(
            Reader.Readˉuleb32() == (uint)Localˉcount + 3,
            "The straight-i32 i32 local count is invalid.");
        Reader.Require(Reader.Readˉbyte() == 0x7F, "The straight-i32 local group type is invalid.");
        Reader.Require(Reader.Readˉuleb32() == 1, "The straight-i32 i64 local count is invalid.");
        Reader.Require(Reader.Readˉbyte() == 0x7E, "The straight-i32 wide local type is invalid.");
        Reader.Require(Reader.Readˉi32ˉconstant() == 0, "The straight-i32 result reset is invalid.");
        Reader.Readˉindexed(0x24, 1);
        Reader.Require(Reader.Readˉi32ˉconstant() == 0, "The straight-i32 instruction reset is invalid.");
        Reader.Readˉindexed(0x24, 2);

        void Readˉoverflowˉreturn()
        {
            Reader.Require(Reader.Readˉbyte() == 0x04, "The overflow if opcode is invalid.");
            Reader.Require(Reader.Readˉbyte() == 0x40, "The overflow block type is invalid.");
            Reader.Require(Reader.Readˉi32ˉconstant() == 3007, "The overflow status is invalid.");
            Reader.Require(Reader.Readˉbyte() == 0x0F, "The overflow return opcode is invalid.");
            Reader.Require(Reader.Readˉbyte() == 0x0B, "The overflow branch is unterminated.");
        }

        void Readˉcheckedˉadd()
        {
            Reader.Readˉindexed(0x21, Scratchˉright);
            Reader.Readˉindexed(0x21, Scratchˉleft);
            Reader.Readˉindexed(0x20, Scratchˉleft);
            Reader.Readˉindexed(0x20, Scratchˉright);
            Reader.Require(Reader.Readˉbyte() == 0x6A, "The checked add opcode is invalid.");
            Reader.Readˉindexed(0x22, Scratchˉresult);
            Reader.Readˉindexed(0x20, Scratchˉleft);
            Reader.Require(Reader.Readˉbyte() == 0x73, "The checked add left xor is invalid.");
            Reader.Readˉindexed(0x20, Scratchˉresult);
            Reader.Readˉindexed(0x20, Scratchˉright);
            Reader.Require(Reader.Readˉbyte() == 0x73, "The checked add right xor is invalid.");
            Reader.Require(Reader.Readˉbyte() == 0x71, "The checked add mask is invalid.");
            Reader.Require(Reader.Readˉi32ˉconstant() == 0, "The checked add sign constant is invalid.");
            Reader.Require(Reader.Readˉbyte() == 0x48, "The checked add sign comparison is invalid.");
            Readˉoverflowˉreturn();
            Reader.Readˉindexed(0x20, Scratchˉresult);
        }

        void Readˉcheckedˉsubtract()
        {
            Reader.Readˉindexed(0x21, Scratchˉright);
            Reader.Readˉindexed(0x21, Scratchˉleft);
            Reader.Readˉindexed(0x20, Scratchˉleft);
            Reader.Readˉindexed(0x20, Scratchˉright);
            Reader.Require(Reader.Readˉbyte() == 0x6B, "The checked subtract opcode is invalid.");
            Reader.Readˉindexed(0x22, Scratchˉresult);
            Reader.Readˉindexed(0x20, Scratchˉleft);
            Reader.Require(Reader.Readˉbyte() == 0x73, "The checked subtract result xor is invalid.");
            Reader.Readˉindexed(0x20, Scratchˉleft);
            Reader.Readˉindexed(0x20, Scratchˉright);
            Reader.Require(Reader.Readˉbyte() == 0x73, "The checked subtract operand xor is invalid.");
            Reader.Require(Reader.Readˉbyte() == 0x71, "The checked subtract mask is invalid.");
            Reader.Require(Reader.Readˉi32ˉconstant() == 0, "The checked subtract sign constant is invalid.");
            Reader.Require(Reader.Readˉbyte() == 0x48, "The checked subtract sign comparison is invalid.");
            Readˉoverflowˉreturn();
            Reader.Readˉindexed(0x20, Scratchˉresult);
        }

        void Readˉcheckedˉmultiply()
        {
            Reader.Readˉindexed(0x21, Scratchˉright);
            Reader.Readˉindexed(0x21, Scratchˉleft);
            Reader.Readˉindexed(0x20, Scratchˉleft);
            Reader.Require(Reader.Readˉbyte() == 0xAC, "The checked multiply left extension is invalid.");
            Reader.Readˉindexed(0x20, Scratchˉright);
            Reader.Require(Reader.Readˉbyte() == 0xAC, "The checked multiply right extension is invalid.");
            Reader.Require(Reader.Readˉbyte() == 0x7E, "The checked multiply opcode is invalid.");
            Reader.Readˉindexed(0x22, Scratchˉwide);
            Reader.Require(Reader.Readˉbyte() == 0xA7, "The checked multiply wrap is invalid.");
            Reader.Readˉindexed(0x22, Scratchˉresult);
            Reader.Require(Reader.Readˉbyte() == 0xAC, "The checked multiply result extension is invalid.");
            Reader.Readˉindexed(0x20, Scratchˉwide);
            Reader.Require(Reader.Readˉbyte() == 0x52, "The checked multiply comparison is invalid.");
            Readˉoverflowˉreturn();
            Reader.Readˉindexed(0x20, Scratchˉresult);
        }

        void Readˉcheckedˉnegate()
        {
            Reader.Readˉindexed(0x21, Scratchˉleft);
            Reader.Readˉindexed(0x20, Scratchˉleft);
            Reader.Require(
                Reader.Readˉi32ˉconstant() == int.MinValue,
                "The checked negate minimum is invalid.");
            Reader.Require(Reader.Readˉbyte() == 0x46, "The checked negate comparison is invalid.");
            Readˉoverflowˉreturn();
            Reader.Require(Reader.Readˉi32ˉconstant() == 0, "The checked negate zero is invalid.");
            Reader.Readˉindexed(0x20, Scratchˉleft);
            Reader.Require(Reader.Readˉbyte() == 0x6B, "The checked negate subtraction is invalid.");
        }

        var Locals = new int[Localˉcount];
        var Stack = new Stack<int>();
        var Trapped = false;
        var Trapˉstep = 0;
        var Result = 0;
        foreach (var (Instruction, Index) in Function.Instructions.Select((Item, Index) => (Item, Index)))
        {
            var Step = Index + 1;
            Reader.Require(
                Reader.Readˉi32ˉconstant() == Step,
                "The emitted WVB instruction charge is invalid.");
            Reader.Readˉindexed(0x24, 2);
            switch (Instruction.Opcode)
            {
                case Opcode.I32ˉconst:
                    Reader.Require(
                        Reader.Readˉi32ˉconstant() == Instruction.Signedˉoperand,
                        "The emitted i32 constant changed value.");
                    if (!Trapped) Stack.Push(Instruction.Signedˉoperand);
                    break;
                case Opcode.Localˉload:
                    Reader.Readˉindexed(0x20, Instruction.Unsignedˉoperand);
                    if (!Trapped) Stack.Push(Locals[(int)Instruction.Unsignedˉoperand]);
                    break;
                case Opcode.Localˉstore:
                    Reader.Readˉindexed(0x21, Instruction.Unsignedˉoperand);
                    if (!Trapped) Locals[(int)Instruction.Unsignedˉoperand] = Stack.Pop();
                    break;
                case Opcode.Pop:
                    Reader.Require(Reader.Readˉbyte() == 0x1A, "The emitted drop opcode is invalid.");
                    if (!Trapped) _ = Stack.Pop();
                    break;
                case Opcode.I32ˉadd:
                    Readˉcheckedˉadd();
                    if (!Trapped)
                    {
                        var Right = Stack.Pop();
                        var Left = Stack.Pop();
                        try { Stack.Push(checked(Left + Right)); }
                        catch (OverflowException) { Trapped = true; Trapˉstep = Step; }
                    }
                    break;
                case Opcode.I32ˉsubtract:
                    Readˉcheckedˉsubtract();
                    if (!Trapped)
                    {
                        var Right = Stack.Pop();
                        var Left = Stack.Pop();
                        try { Stack.Push(checked(Left - Right)); }
                        catch (OverflowException) { Trapped = true; Trapˉstep = Step; }
                    }
                    break;
                case Opcode.I32ˉmultiply:
                    Readˉcheckedˉmultiply();
                    if (!Trapped)
                    {
                        var Right = Stack.Pop();
                        var Left = Stack.Pop();
                        try { Stack.Push(checked(Left * Right)); }
                        catch (OverflowException) { Trapped = true; Trapˉstep = Step; }
                    }
                    break;
                case Opcode.I32ˉnegate:
                    Readˉcheckedˉnegate();
                    if (!Trapped)
                    {
                        var Value = Stack.Pop();
                        try { Stack.Push(checked(-Value)); }
                        catch (OverflowException) { Trapped = true; Trapˉstep = Step; }
                    }
                    break;
                case Opcode.Return:
                    Reader.Readˉindexed(0x24, 1);
                    Reader.Require(Reader.Readˉi32ˉconstant() == 0, "The success status is invalid.");
                    Reader.Require(Reader.Readˉbyte() == 0x0F, "The success return opcode is invalid.");
                    if (!Trapped) Result = Stack.Pop();
                    break;
                default:
                    throw new InvalidDataException(
                        $"Unsupported straight-i32 source opcode {Instruction.Opcode}.");
            }
        }

        Reader.Require(Reader.Readˉbyte() == 0x0B, "The straight-i32 body is unterminated.");
        Reader.Require(Reader.Position == Bodyˉend, "The straight-i32 body has trailing bytes.");
        Reader.Require(Reader.Position == Codeˉend, "The straight-i32 code section has trailing bytes.");
        Reader.Require(Reader.Position == Reader.Length, "The straight-i32 module has trailing bytes.");
        return Trapped
            ? new(3007, 0, Trapˉstep)
            : new(0, Result, Function.Instructions.Length);
    }

    private static WebAssemblyˉexecutionˉresult Executeˉcheckedˉaddˉwebassembly(
        ReadOnlySpan<byte> module)
    {
        var Bytes = module.ToArray();
        var Cursor = 0;

        void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidDataException(message);
            }
        }

        byte Readˉbyte()
        {
            Require(Cursor < Bytes.Length, "The checked-add WebAssembly module is truncated.");
            return Bytes[Cursor++];
        }

        uint Readˉuleb32()
        {
            uint Result = 0;
            for (var Index = 0; Index < 5; Index++)
            {
                var Value = Readˉbyte();
                var Payload = (uint)(Value & 0x7F);
                if (Index == 4)
                {
                    Require(Payload <= 0x0F, "The checked-add u32 LEB128 value exceeds 32 bits.");
                }
                Result |= Payload << (Index * 7);
                if ((Value & 0x80) == 0)
                {
                    return Result;
                }
            }
            throw new InvalidDataException("The checked-add u32 LEB128 value is unterminated.");
        }

        int Readˉsleb32()
        {
            long Result = 0;
            for (var Index = 0; Index < 5; Index++)
            {
                var Value = Readˉbyte();
                var Payload = Value & 0x7F;
                Result |= (long)Payload << (Index * 7);
                if ((Value & 0x80) != 0)
                {
                    continue;
                }

                if (Index == 4)
                {
                    var Negative = (Payload & 0x08) != 0;
                    Require(
                        Negative
                            ? (Payload & 0x70) == 0x70
                            : (Payload & 0x70) == 0,
                        "The checked-add i32 LEB128 value has invalid unused bits.");
                    return unchecked((int)(uint)Result);
                }

                var Shift = (Index + 1) * 7;
                if ((Value & 0x40) != 0)
                {
                    Result |= -1L << Shift;
                }
                Require(Result is >= int.MinValue and <= int.MaxValue,
                    "The checked-add signed LEB128 value exceeds i32.");
                return (int)Result;
            }
            throw new InvalidDataException("The checked-add i32 LEB128 value is unterminated.");
        }

        int Readˉsection(byte expectedˉkind)
        {
            Require(Readˉbyte() == expectedˉkind, "The checked-add section order is invalid.");
            var Length = Readˉuleb32();
            Require(Length <= int.MaxValue, "The checked-add section is oversized.");
            Require(Cursor <= Bytes.Length - (int)Length, "The checked-add section is truncated.");
            return Cursor + (int)Length;
        }

        void Readˉindexed(byte opcode, uint index)
        {
            Require(Readˉbyte() == opcode, "The checked-add indexed opcode is invalid.");
            Require(Readˉuleb32() == index, "The checked-add instruction index is invalid.");
        }

        int Readˉi32ˉconstant()
        {
            Require(Readˉbyte() == 0x41, "The checked-add i32 constant opcode is invalid.");
            return Readˉsleb32();
        }

        void Readˉglobal(byte mutable, int initial)
        {
            Require(Readˉbyte() == 0x7F, "The checked-add global type is invalid.");
            Require(Readˉbyte() == mutable, "The checked-add global mutability is invalid.");
            Require(Readˉi32ˉconstant() == initial, "The checked-add global initializer is invalid.");
            Require(Readˉbyte() == 0x0B, "The checked-add global initializer is unterminated.");
        }

        void Readˉexport(string name, byte kind, uint index)
        {
            Require(Readˉuleb32() == name.Length, "The checked-add export name length is invalid.");
            foreach (var Character in name)
            {
                Require(Readˉbyte() == (byte)Character, "The checked-add export name is invalid.");
            }
            Require(Readˉbyte() == kind, "The checked-add export kind is invalid.");
            Require(Readˉuleb32() == index, "The checked-add export index is invalid.");
        }

        Require(Bytes.Length >= 8, "The checked-add WebAssembly header is truncated.");
        Require(Readˉbyte() == 0x00, "The checked-add WebAssembly magic is invalid.");
        Require(Readˉbyte() == 0x61, "The checked-add WebAssembly magic is invalid.");
        Require(Readˉbyte() == 0x73, "The checked-add WebAssembly magic is invalid.");
        Require(Readˉbyte() == 0x6D, "The checked-add WebAssembly magic is invalid.");
        Require(Readˉbyte() == 0x01, "The checked-add WebAssembly version is invalid.");
        Require(Readˉbyte() == 0x00, "The checked-add WebAssembly version is invalid.");
        Require(Readˉbyte() == 0x00, "The checked-add WebAssembly version is invalid.");
        Require(Readˉbyte() == 0x00, "The checked-add WebAssembly version is invalid.");

        var Typeˉend = Readˉsection(1);
        Require(Readˉuleb32() == 1, "The checked-add type count is invalid.");
        Require(Readˉbyte() == 0x60, "The checked-add function type is invalid.");
        Require(Readˉuleb32() == 0, "The checked-add parameter count is invalid.");
        Require(Readˉuleb32() == 1, "The checked-add result count is invalid.");
        Require(Readˉbyte() == 0x7F, "The checked-add result type is invalid.");
        Require(Cursor == Typeˉend, "The checked-add type section has trailing bytes.");

        var Functionˉend = Readˉsection(3);
        Require(Readˉuleb32() == 1, "The checked-add function count is invalid.");
        Require(Readˉuleb32() == 0, "The checked-add type index is invalid.");
        Require(Cursor == Functionˉend, "The checked-add function section has trailing bytes.");

        var Globalˉend = Readˉsection(6);
        Require(Readˉuleb32() == 3, "The checked-add global count is invalid.");
        Readˉglobal(0, 1);
        Readˉglobal(1, 0);
        Readˉglobal(1, 0);
        Require(Cursor == Globalˉend, "The checked-add global section has trailing bytes.");

        var Exportˉend = Readˉsection(7);
        Require(Readˉuleb32() == 4, "The checked-add export count is invalid.");
        Readˉexport("Windvale.run", 0, 0);
        Readˉexport("Windvale.abi", 3, 0);
        Readˉexport("Windvale.result", 3, 1);
        Readˉexport("Windvale.instructions", 3, 2);
        Require(Cursor == Exportˉend, "The checked-add export section has trailing bytes.");

        var Codeˉend = Readˉsection(10);
        Require(Readˉuleb32() == 1, "The checked-add body count is invalid.");
        var Bodyˉlength = Readˉuleb32();
        Require(Bodyˉlength <= int.MaxValue, "The checked-add body is oversized.");
        Require(Cursor <= Codeˉend - (int)Bodyˉlength, "The checked-add body is truncated.");
        var Bodyˉend = Cursor + (int)Bodyˉlength;
        Require(Readˉuleb32() == 1, "The checked-add local group count is invalid.");
        Require(Readˉuleb32() == 1, "The checked-add local count is invalid.");
        Require(Readˉbyte() == 0x7F, "The checked-add local type is invalid.");

        Require(Readˉi32ˉconstant() == 0, "The checked-add result reset is invalid.");
        Readˉindexed(0x24, 1);
        Require(Readˉi32ˉconstant() == 0, "The checked-add step reset is invalid.");
        Readˉindexed(0x24, 2);

        var Left = Readˉi32ˉconstant();
        var Right = Readˉi32ˉconstant();
        Require(Readˉbyte() == 0x6A, "The checked-add addition opcode is invalid.");
        Readˉindexed(0x22, 0);
        Require(Readˉi32ˉconstant() == Left, "The checked-add left overflow probe changed.");
        Require(Readˉbyte() == 0x73, "The checked-add first xor opcode is invalid.");
        Readˉindexed(0x20, 0);
        Require(Readˉi32ˉconstant() == Right, "The checked-add right overflow probe changed.");
        Require(Readˉbyte() == 0x73, "The checked-add second xor opcode is invalid.");
        Require(Readˉbyte() == 0x71, "The checked-add and opcode is invalid.");
        Require(Readˉi32ˉconstant() == 0, "The checked-add sign comparison constant is invalid.");
        Require(Readˉbyte() == 0x48, "The checked-add signed comparison opcode is invalid.");
        Require(Readˉbyte() == 0x04, "The checked-add if opcode is invalid.");
        Require(Readˉbyte() == 0x40, "The checked-add if block type is invalid.");

        Require(Readˉi32ˉconstant() == 7, "The checked-add overflow step count is invalid.");
        Readˉindexed(0x24, 2);
        Require(Readˉi32ˉconstant() == 3007, "The checked-add overflow status is invalid.");
        Require(Readˉbyte() == 0x0F, "The checked-add overflow return is invalid.");
        Require(Readˉbyte() == 0x0B, "The checked-add overflow branch is unterminated.");

        Readˉindexed(0x20, 0);
        Readˉindexed(0x24, 1);
        Require(Readˉi32ˉconstant() == 10, "The checked-add success step count is invalid.");
        Readˉindexed(0x24, 2);
        Require(Readˉi32ˉconstant() == 0, "The checked-add success status is invalid.");
        Require(Readˉbyte() == 0x0B, "The checked-add body end is invalid.");
        Require(Cursor == Bodyˉend, "The checked-add body has trailing bytes.");
        Require(Cursor == Codeˉend, "The checked-add code section has trailing bytes.");
        Require(Cursor == Bytes.Length, "The checked-add module has trailing bytes.");

        var Sum = unchecked(Left + Right);
        var Overflow = ((Left ^ Sum) & (Right ^ Sum)) < 0;
        return Overflow
            ? new(3007, 0, 7)
            : new(0, Sum, 10);
    }

    private static int Executeˉconstantˉwebassembly(ReadOnlySpan<byte> module)
    {
        var Bytes = module.ToArray();
        var Cursor = 0;

        void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidDataException(message);
            }
        }

        byte Readˉbyte()
        {
            Require(Cursor < Bytes.Length, "The WebAssembly module is truncated.");
            return Bytes[Cursor++];
        }

        uint Readˉuleb32()
        {
            uint Result = 0;
            for (var Index = 0; Index < 5; Index++)
            {
                var Value = Readˉbyte();
                var Payload = (uint)(Value & 0x7F);
                if (Index == 4)
                {
                    Require(Payload <= 0x0F, "The u32 LEB128 value exceeds 32 bits.");
                }
                Result |= Payload << (Index * 7);
                if ((Value & 0x80) == 0)
                {
                    return Result;
                }
            }
            throw new InvalidDataException("The u32 LEB128 value is unterminated.");
        }

        int Readˉsleb32()
        {
            long Result = 0;
            for (var Index = 0; Index < 5; Index++)
            {
                var Value = Readˉbyte();
                var Payload = Value & 0x7F;
                Result |= (long)Payload << (Index * 7);
                if ((Value & 0x80) != 0)
                {
                    continue;
                }

                if (Index == 4)
                {
                    var Negative = (Payload & 0x08) != 0;
                    Require(
                        Negative
                            ? (Payload & 0x70) == 0x70
                            : (Payload & 0x70) == 0,
                        "The i32 LEB128 value has invalid unused bits.");
                    return unchecked((int)(uint)Result);
                }

                var Shift = (Index + 1) * 7;
                if ((Value & 0x40) != 0)
                {
                    Result |= -1L << Shift;
                }
                Require(Result is >= int.MinValue and <= int.MaxValue,
                    "The signed LEB128 value exceeds i32.");
                return (int)Result;
            }
            throw new InvalidDataException("The i32 LEB128 value is unterminated.");
        }

        int Readˉsection(byte expectedˉkind)
        {
            Require(Readˉbyte() == expectedˉkind, "The WebAssembly section order is invalid.");
            var Length = Readˉuleb32();
            Require(Length <= int.MaxValue, "The WebAssembly section is oversized.");
            Require(Cursor <= Bytes.Length - (int)Length, "The WebAssembly section is truncated.");
            return Cursor + (int)Length;
        }

        Require(Bytes.Length >= 8, "The WebAssembly header is truncated.");
        Require(Readˉbyte() == 0x00, "The WebAssembly magic is invalid.");
        Require(Readˉbyte() == 0x61, "The WebAssembly magic is invalid.");
        Require(Readˉbyte() == 0x73, "The WebAssembly magic is invalid.");
        Require(Readˉbyte() == 0x6D, "The WebAssembly magic is invalid.");
        Require(Readˉbyte() == 0x01, "The WebAssembly version is invalid.");
        Require(Readˉbyte() == 0x00, "The WebAssembly version is invalid.");
        Require(Readˉbyte() == 0x00, "The WebAssembly version is invalid.");
        Require(Readˉbyte() == 0x00, "The WebAssembly version is invalid.");

        var Typeˉend = Readˉsection(1);
        Require(Readˉuleb32() == 1, "The WebAssembly type count is invalid.");
        Require(Readˉbyte() == 0x60, "The WebAssembly function type is invalid.");
        Require(Readˉuleb32() == 0, "The WebAssembly parameter count is invalid.");
        Require(Readˉuleb32() == 1, "The WebAssembly result count is invalid.");
        Require(Readˉbyte() == 0x7F, "The WebAssembly result type is invalid.");
        Require(Cursor == Typeˉend, "The WebAssembly type section has trailing bytes.");

        var Functionˉend = Readˉsection(3);
        Require(Readˉuleb32() == 1, "The WebAssembly function count is invalid.");
        Require(Readˉuleb32() == 0, "The WebAssembly type index is invalid.");
        Require(Cursor == Functionˉend, "The WebAssembly function section has trailing bytes.");

        var Exportˉend = Readˉsection(7);
        Require(Readˉuleb32() == 1, "The WebAssembly export count is invalid.");
        Require(Readˉuleb32() == 4, "The WebAssembly export name length is invalid.");
        Require(Readˉbyte() == (byte)'M', "The WebAssembly export name is invalid.");
        Require(Readˉbyte() == (byte)'a', "The WebAssembly export name is invalid.");
        Require(Readˉbyte() == (byte)'i', "The WebAssembly export name is invalid.");
        Require(Readˉbyte() == (byte)'n', "The WebAssembly export name is invalid.");
        Require(Readˉbyte() == 0, "The WebAssembly export kind is invalid.");
        Require(Readˉuleb32() == 0, "The WebAssembly export index is invalid.");
        Require(Cursor == Exportˉend, "The WebAssembly export section has trailing bytes.");

        var Codeˉend = Readˉsection(10);
        Require(Readˉuleb32() == 1, "The WebAssembly body count is invalid.");
        var Bodyˉlength = Readˉuleb32();
        Require(Bodyˉlength <= int.MaxValue, "The WebAssembly body is oversized.");
        Require(Cursor <= Codeˉend - (int)Bodyˉlength, "The WebAssembly body is truncated.");
        var Bodyˉend = Cursor + (int)Bodyˉlength;
        Require(Readˉuleb32() == 0, "The WebAssembly local group count is invalid.");
        Require(Readˉbyte() == 0x41, "The WebAssembly body opcode is invalid.");
        var Result = Readˉsleb32();
        Require(Readˉbyte() == 0x0B, "The WebAssembly body end is invalid.");
        Require(Cursor == Bodyˉend, "The WebAssembly body has trailing bytes.");
        Require(Cursor == Codeˉend, "The WebAssembly code section has trailing bytes.");
        Require(Cursor == Bytes.Length, "The WebAssembly module has trailing bytes.");
        return Result;
    }

    private static int Runˉportable(string source)
    {
        var Module = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(source));
        return new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new StringWriter()),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain().Exitˉcode;
    }

    private static byte[] Compileˉsuccess(string source)
    {
        var Result = Seedˉcompiler.Compile(source);
        if (!Result.Success)
        {
            throw new InvalidOperationException(
                "Compilation failed: " + string.Join(" | ", Result.Diagnostics));
        }

        return Result.Moduleˉbytes.ToArray();
    }

    private static byte[] Compileˉcompositionˉsuccess(params Sourceˉmoduleˉinput[] dependencies)
    {
        var Result = Seedˉcompiler.Compileˉmodules(
            new("composition-root.wv", COMPOSITION_ROOT_SOURCE),
            dependencies);
        if (!Result.Success)
        {
            throw new InvalidOperationException(
                "Source-module compilation failed: " + string.Join(" | ", Result.Diagnostics));
        }

        return Result.Moduleˉbytes.ToArray();
    }

    private static byte[] Compileˉwithˉmachineˉcontractsˉsuccess(string source, string sourceˉname)
    {
        var Result = Seedˉcompiler.Compileˉmodules(
            new(sourceˉname, source),
            [new("Foundation/Machine-Contracts.wv", MACHINE_CONTRACTS_SOURCE)]);
        if (!Result.Success)
        {
            throw new InvalidOperationException(
                "Foundation composition failed: " + string.Join(" | ", Result.Diagnostics));
        }

        return Result.Moduleˉbytes.ToArray();
    }

    private static byte[] Compileˉwithˉbyteˉorderingˉsuccess(string source, string sourceˉname)
    {
        var Result = Seedˉcompiler.Compileˉmodules(
            new(sourceˉname, source),
            [new("Foundation/Byte-Ordering.wv", BYTE_ORDERING_SOURCE)]);
        if (!Result.Success)
        {
            throw new InvalidOperationException(
                "Foundation composition failed: " + string.Join(" | ", Result.Diagnostics));
        }

        return Result.Moduleˉbytes.ToArray();
    }

    private static byte[] Compileˉwithˉdecimalˉparsingˉsuccess(string source, string sourceˉname)
    {
        var Result = Seedˉcompiler.Compileˉmodules(
            new(sourceˉname, source),
            [new("Foundation/Decimal-Parsing.wv", DECIMAL_PARSING_SOURCE)]);
        if (!Result.Success)
        {
            throw new InvalidOperationException(
                "Foundation composition failed: " + string.Join(" | ", Result.Diagnostics));
        }

        return Result.Moduleˉbytes.ToArray();
    }

    private static byte[] Compileˉwithˉbyteˉconstructionˉsuccess(string source, string sourceˉname)
    {
        var Result = Seedˉcompiler.Compileˉmodules(
            new(sourceˉname, source),
            [new("Foundation/Byte-Construction.wv", BYTE_CONSTRUCTION_SOURCE)]);
        if (!Result.Success)
        {
            throw new InvalidOperationException(
                "Foundation composition failed: " + string.Join(" | ", Result.Diagnostics));
        }

        return Result.Moduleˉbytes.ToArray();
    }

    private static byte[] Compileˉwithˉnativeˉstencilˉsuccess(
        string source,
        string sourceˉname,
        bool includeˉnativeˉstencil = true)
    {
        var Dependencies = new List<Sourceˉmoduleˉinput>();
        if (includeˉnativeˉstencil)
        {
            Dependencies.Add(new(
                "Compiler/Windvale/Native-Stencil-Core.wv",
                NATIVE_STENCIL_CORE_SOURCE));
        }
        var Result = Seedˉcompiler.Compileˉmodules(
            new(sourceˉname, source),
            Dependencies);
        if (!Result.Success)
        {
            throw new InvalidOperationException(
                "Native-stencil composition failed: " + string.Join(" | ", Result.Diagnostics));
        }

        return Result.Moduleˉbytes.ToArray();
    }

    private static byte[] Compileˉwithˉsourceˉlexerˉsuccess(string source, string sourceˉname)
    {
        var Result = Seedˉcompiler.Compileˉmodules(
            new(sourceˉname, source),
            [
                new("Compiler/Windvale/Source-Lexer-Core.wv", SOURCE_LEXER_SOURCE),
                new("Foundation/Decimal-Parsing.wv", DECIMAL_PARSING_SOURCE),
            ]);
        if (!Result.Success)
        {
            throw new InvalidOperationException(
                "Compiler bootstrap composition failed: " + string.Join(" | ", Result.Diagnostics));
        }

        return Result.Moduleˉbytes.ToArray();
    }

    private static byte[] Compileˉwithˉsourceˉdeclarationˉparserˉsuccess(
        string source,
        string sourceˉname)
    {
        var Result = Seedˉcompiler.Compileˉmodules(
            new(sourceˉname, source),
            [
                new("Compiler/Windvale/Source-Declaration-Parser.wv", SOURCE_DECLARATION_PARSER_SOURCE),
                new("Compiler/Windvale/Source-Lexer-Core.wv", SOURCE_LEXER_SOURCE),
                new("Foundation/Decimal-Parsing.wv", DECIMAL_PARSING_SOURCE),
            ]);
        if (!Result.Success)
        {
            throw new InvalidOperationException(
                "Compiler parser composition failed: " + string.Join(" | ", Result.Diagnostics));
        }

        return Result.Moduleˉbytes.ToArray();
    }

    private static byte[] Compileˉwithˉsourceˉbodyˉparserˉsuccess(
        string source,
        string sourceˉname)
    {
        var Result = Seedˉcompiler.Compileˉmodules(
            new(sourceˉname, source),
            [
                new("Compiler/Windvale/Source-Body-Parser.wv", SOURCE_BODY_PARSER_SOURCE),
                new("Compiler/Windvale/Source-Declaration-Parser.wv", SOURCE_DECLARATION_PARSER_SOURCE),
                new("Compiler/Windvale/Source-Lexer-Core.wv", SOURCE_LEXER_SOURCE),
                new("Foundation/Decimal-Parsing.wv", DECIMAL_PARSING_SOURCE),
            ]);
        if (!Result.Success)
        {
            throw new InvalidOperationException(
                "Compiler body-parser composition failed: " + string.Join(" | ", Result.Diagnostics));
        }

        return Result.Moduleˉbytes.ToArray();
    }

    private static byte[] Compileˉwithˉsourceˉsetˉsuccess(
        string source,
        string sourceˉname,
        bool includeˉsourceˉset = true)
    {
        var Dependencies = new List<Sourceˉmoduleˉinput>();
        if (includeˉsourceˉset)
        {
            Dependencies.Add(new("Compiler/Windvale/Source-Set-Core.wv", SOURCE_SET_SOURCE));
        }
        Dependencies.Add(new("Compiler/Windvale/Source-Body-Parser.wv", SOURCE_BODY_PARSER_SOURCE));
        Dependencies.Add(new("Compiler/Windvale/Source-Declaration-Parser.wv", SOURCE_DECLARATION_PARSER_SOURCE));
        Dependencies.Add(new("Compiler/Windvale/Source-Lexer-Core.wv", SOURCE_LEXER_SOURCE));
        Dependencies.Add(new("Foundation/Decimal-Parsing.wv", DECIMAL_PARSING_SOURCE));
        var Result = Seedˉcompiler.Compileˉmodules(
            new(sourceˉname, source),
            Dependencies);
        if (!Result.Success)
        {
            throw new InvalidOperationException(
                "Compiler source-set composition failed: " + string.Join(" | ", Result.Diagnostics));
        }

        return Result.Moduleˉbytes.ToArray();
    }

    private static byte[] Compileˉwithˉsourceˉgraphˉsuccess(
        string source,
        string sourceˉname,
        bool includeˉsourceˉgraph = true)
    {
        var Dependencies = new List<Sourceˉmoduleˉinput>();
        if (includeˉsourceˉgraph)
        {
            Dependencies.Add(new("Compiler/Windvale/Source-Graph-Core.wv", SOURCE_GRAPH_SOURCE));
        }
        Dependencies.Add(new("Compiler/Windvale/Source-Body-Parser.wv", SOURCE_BODY_PARSER_SOURCE));
        Dependencies.Add(new("Compiler/Windvale/Source-Declaration-Parser.wv", SOURCE_DECLARATION_PARSER_SOURCE));
        Dependencies.Add(new("Compiler/Windvale/Source-Lexer-Core.wv", SOURCE_LEXER_SOURCE));
        Dependencies.Add(new("Compiler/Windvale/Source-Set-Core.wv", SOURCE_SET_SOURCE));
        Dependencies.Add(new("Foundation/Byte-Construction.wv", BYTE_CONSTRUCTION_SOURCE));
        Dependencies.Add(new("Foundation/Decimal-Parsing.wv", DECIMAL_PARSING_SOURCE));
        var Result = Seedˉcompiler.Compileˉmodules(
            new(sourceˉname, source),
            Dependencies);
        if (!Result.Success)
        {
            throw new InvalidOperationException(
                "Compiler source-graph composition failed: " + string.Join(" | ", Result.Diagnostics));
        }

        return Result.Moduleˉbytes.ToArray();
    }

    private static byte[] Compileˉwithˉsourceˉsymbolsˉsuccess(
        string source,
        string sourceˉname,
        bool includeˉsourceˉsymbols = true)
    {
        var Dependencies = new List<Sourceˉmoduleˉinput>();
        if (includeˉsourceˉsymbols)
        {
            Dependencies.Add(new("Compiler/Windvale/Source-Symbols-Core.wv", SOURCE_SYMBOLS_SOURCE));
        }
        Dependencies.Add(new("Compiler/Windvale/Source-Graph-Core.wv", SOURCE_GRAPH_SOURCE));
        Dependencies.Add(new("Compiler/Windvale/Source-Body-Parser.wv", SOURCE_BODY_PARSER_SOURCE));
        Dependencies.Add(new("Compiler/Windvale/Source-Declaration-Parser.wv", SOURCE_DECLARATION_PARSER_SOURCE));
        Dependencies.Add(new("Compiler/Windvale/Source-Lexer-Core.wv", SOURCE_LEXER_SOURCE));
        Dependencies.Add(new("Compiler/Windvale/Source-Set-Core.wv", SOURCE_SET_SOURCE));
        Dependencies.Add(new("Foundation/Byte-Construction.wv", BYTE_CONSTRUCTION_SOURCE));
        Dependencies.Add(new("Foundation/Decimal-Parsing.wv", DECIMAL_PARSING_SOURCE));
        var Result = Seedˉcompiler.Compileˉmodules(
            new(sourceˉname, source),
            Dependencies);
        if (!Result.Success)
        {
            throw new InvalidOperationException(
                "Compiler source-symbol composition failed: " + string.Join(" | ", Result.Diagnostics));
        }

        return Result.Moduleˉbytes.ToArray();
    }

    private static byte[] Compileˉwithˉsourceˉbindingsˉsuccess(
        string source,
        string sourceˉname,
        bool includeˉsourceˉbindings = true)
    {
        var Dependencies = new List<Sourceˉmoduleˉinput>();
        if (includeˉsourceˉbindings)
        {
            Dependencies.Add(new("Compiler/Windvale/Source-Bindings-Core.wv", SOURCE_BINDINGS_SOURCE));
        }
        Dependencies.Add(new("Compiler/Windvale/Source-Symbols-Core.wv", SOURCE_SYMBOLS_SOURCE));
        Dependencies.Add(new("Compiler/Windvale/Source-Graph-Core.wv", SOURCE_GRAPH_SOURCE));
        Dependencies.Add(new("Compiler/Windvale/Source-Body-Parser.wv", SOURCE_BODY_PARSER_SOURCE));
        Dependencies.Add(new("Compiler/Windvale/Source-Declaration-Parser.wv", SOURCE_DECLARATION_PARSER_SOURCE));
        Dependencies.Add(new("Compiler/Windvale/Source-Lexer-Core.wv", SOURCE_LEXER_SOURCE));
        Dependencies.Add(new("Compiler/Windvale/Source-Set-Core.wv", SOURCE_SET_SOURCE));
        Dependencies.Add(new("Foundation/Byte-Construction.wv", BYTE_CONSTRUCTION_SOURCE));
        Dependencies.Add(new("Foundation/Decimal-Parsing.wv", DECIMAL_PARSING_SOURCE));
        var Result = Seedˉcompiler.Compileˉmodules(
            new(sourceˉname, source),
            Dependencies);
        if (!Result.Success)
        {
            throw new InvalidOperationException(
                "Compiler source-binding composition failed: " + string.Join(" | ", Result.Diagnostics));
        }

        return Result.Moduleˉbytes.ToArray();
    }

    private static byte[] Compileˉwithˉsourceˉwirˉsuccess(
        string source,
        string sourceˉname,
        bool includeˉsourceˉwir = true)
    {
        var Dependencies = new List<Sourceˉmoduleˉinput>();
        if (includeˉsourceˉwir)
        {
            Dependencies.Add(new("Compiler/Windvale/Source-Wir-Core.wv", SOURCE_WIR_SOURCE));
        }
        Dependencies.Add(new("Compiler/Windvale/Source-Bindings-Core.wv", SOURCE_BINDINGS_SOURCE));
        Dependencies.Add(new("Compiler/Windvale/Source-Symbols-Core.wv", SOURCE_SYMBOLS_SOURCE));
        Dependencies.Add(new("Compiler/Windvale/Source-Graph-Core.wv", SOURCE_GRAPH_SOURCE));
        Dependencies.Add(new("Compiler/Windvale/Source-Body-Parser.wv", SOURCE_BODY_PARSER_SOURCE));
        Dependencies.Add(new("Compiler/Windvale/Source-Declaration-Parser.wv", SOURCE_DECLARATION_PARSER_SOURCE));
        Dependencies.Add(new("Compiler/Windvale/Source-Lexer-Core.wv", SOURCE_LEXER_SOURCE));
        Dependencies.Add(new("Compiler/Windvale/Source-Set-Core.wv", SOURCE_SET_SOURCE));
        Dependencies.Add(new("Foundation/Byte-Construction.wv", BYTE_CONSTRUCTION_SOURCE));
        Dependencies.Add(new("Foundation/Decimal-Parsing.wv", DECIMAL_PARSING_SOURCE));
        var Result = Seedˉcompiler.Compileˉmodules(
            new(sourceˉname, source),
            Dependencies);
        if (!Result.Success)
        {
            throw new InvalidOperationException(
                "Compiler WVIR composition failed: " + string.Join(" | ", Result.Diagnostics));
        }

        return Result.Moduleˉbytes.ToArray();
    }

    private static byte[] Compileˉwithˉsourceˉwvbˉsuccess(
        string source,
        string sourceˉname,
        bool includeˉsourceˉwvb = true)
    {
        var Dependencies = new List<Sourceˉmoduleˉinput>();
        if (includeˉsourceˉwvb)
        {
            Dependencies.Add(new("Compiler/Windvale/Source-Wvb-Core.wv", SOURCE_WVB_SOURCE));
        }
        Dependencies.Add(new("Compiler/Windvale/Source-Wir-Core.wv", SOURCE_WIR_SOURCE));
        Dependencies.Add(new("Compiler/Windvale/Source-Bindings-Core.wv", SOURCE_BINDINGS_SOURCE));
        Dependencies.Add(new("Compiler/Windvale/Source-Symbols-Core.wv", SOURCE_SYMBOLS_SOURCE));
        Dependencies.Add(new("Compiler/Windvale/Source-Graph-Core.wv", SOURCE_GRAPH_SOURCE));
        Dependencies.Add(new("Compiler/Windvale/Source-Body-Parser.wv", SOURCE_BODY_PARSER_SOURCE));
        Dependencies.Add(new("Compiler/Windvale/Source-Declaration-Parser.wv", SOURCE_DECLARATION_PARSER_SOURCE));
        Dependencies.Add(new("Compiler/Windvale/Source-Lexer-Core.wv", SOURCE_LEXER_SOURCE));
        Dependencies.Add(new("Compiler/Windvale/Source-Set-Core.wv", SOURCE_SET_SOURCE));
        Dependencies.Add(new("Foundation/Byte-Construction.wv", BYTE_CONSTRUCTION_SOURCE));
        Dependencies.Add(new("Foundation/Decimal-Parsing.wv", DECIMAL_PARSING_SOURCE));
        var Result = Seedˉcompiler.Compileˉmodules(
            new(sourceˉname, source),
            Dependencies);
        if (!Result.Success)
        {
            throw new InvalidOperationException(
                "Compiler WVB composition failed: " + string.Join(" | ", Result.Diagnostics));
        }

        return Result.Moduleˉbytes.ToArray();
    }

    private static byte[] Compileˉwithˉwebassemblyˉsuccess(
        string source,
        string sourceˉname,
        bool includeˉwebassembly = true)
    {
        var Dependencies = new List<Sourceˉmoduleˉinput>();
        if (includeˉwebassembly)
        {
            Dependencies.Add(new(
                "Compiler/Windvale/WebAssembly-Core.wv",
                WEBASSEMBLY_CORE_SOURCE));
        }
        var Result = Seedˉcompiler.Compileˉmodules(
            new(sourceˉname, source),
            Dependencies);
        if (!Result.Success)
        {
            throw new InvalidOperationException(
                "Compiler WebAssembly composition failed: " +
                string.Join(" | ", Result.Diagnostics));
        }

        return Result.Moduleˉbytes.ToArray();
    }

    private static byte[] Compileˉwithˉtoolˉfoundationˉsuccess(string source, string sourceˉname)
    {
        var Result = Seedˉcompiler.Compileˉmodules(
            new(sourceˉname, source),
            [
                new("Foundation/Machine-Contracts.wv", MACHINE_CONTRACTS_SOURCE),
                new("Foundation/Byte-Ordering.wv", BYTE_ORDERING_SOURCE),
                new("Foundation/Decimal-Parsing.wv", DECIMAL_PARSING_SOURCE),
                new("Foundation/Byte-Construction.wv", BYTE_CONSTRUCTION_SOURCE),
            ]);
        if (!Result.Success)
        {
            throw new InvalidOperationException(
                "Tool Foundation composition failed: " + string.Join(" | ", Result.Diagnostics));
        }

        return Result.Moduleˉbytes.ToArray();
    }

    private static byte[] Assembleˉsuccess(string source)
    {
        var Result = Assemblyˉcompiler.Assemble(source);
        if (!Result.Success)
        {
            throw new InvalidOperationException(
                "Assembly failed: " + string.Join(" | ", Result.Diagnostics));
        }

        return Result.Objectˉbytes.ToArray();
    }

    private static Linkˉresult Linkˉsuccess(
        IEnumerable<byte[]> objectˉbytes,
        Linkˉoptions options)
    {
        var Result = Linkˉcompiler.Link(
            objectˉbytes
                .Select(Bytes => new Linkˉinput(Bytes.ToImmutableArray()))
                .ToImmutableArray(),
            options);
        if (!Result.Success)
        {
            throw new InvalidOperationException(
                "Link failed: " + string.Join(" | ", Result.Diagnostics));
        }
        return Result;
    }

    private static void Hasˉlinkˉdiagnostic(
        IEnumerable<byte[]> objectˉbytes,
        Linkˉoptions options,
        string code)
    {
        var Result = Linkˉcompiler.Link(
            objectˉbytes
                .Select(Bytes => new Linkˉinput(Bytes.ToImmutableArray()))
                .ToImmutableArray(),
            options);
        False(Result.Success, $"Link expected to produce {code} succeeded.");
        Equal(code, Result.Diagnostics.Single().Code);
        Equal(0, Result.Imageˉbytes.Length);
        Equal(0, Result.Mapˉbytes.Length);
    }

    private static void Hasˉdiagnostic(string source, string code)
    {
        var Result = Seedˉcompiler.Compile(source);
        Hasˉdiagnostic(Result, code);
    }

    private static void Hasˉdiagnostic(Compilationˉresult result, string code)
    {
        False(result.Success, $"Source expected to produce {code} compiled successfully.");
        True(result.Diagnostics.Any(Diagnostic => Diagnostic.Code == code),
            $"Expected diagnostic {code}; found {string.Join(", ", result.Diagnostics.Select(Item => Item.Code))}.");
    }

    private static void Projectˉhasˉdiagnostic(string text, string code)
    {
        var Result = Projectˉparser.Parse(text);
        False(Result.Success, $"Project text expected to produce {code} was accepted.");
        Equal(code, Result.Diagnostics.Single().Code);
        True(Result.Diagnostics[0].Line > 0, "A project diagnostic line was not one-based.");
        True(Result.Diagnostics[0].Column > 0, "A project diagnostic column was not one-based.");
    }

    private static void Hasˉassemblyˉdiagnostic(string source, string code)
    {
        var Result = Assemblyˉcompiler.Assemble(source);
        False(Result.Success, $"Assembly source expected to produce {code} succeeded.");
        Equal(code, Result.Diagnostics.Single().Code);
        True(Result.Diagnostics[0].Line > 0, "Assembly diagnostic line was not one-based.");
        True(Result.Diagnostics[0].Column > 0, "Assembly diagnostic column was not one-based.");
    }

    private static Objectˉfile Buildˉsampleˉobject()
    {
        return new(
            Objectˉarchitecture.X86ˉ64,
            [
                new(".text", Objectˉsectionˉkind.Code, 16, 6, [232, 0, 0, 0, 0, 195]),
                new(".rodata", Objectˉsectionˉkind.Readˉonlyˉdata, 1, 3, [72, 105, 10]),
            ],
            [
                new("Message", Objectˉsymbolˉbinding.Local, Objectˉsymbolˉkind.Data, 1, 0, 3),
                new("Main", Objectˉsymbolˉbinding.Export, Objectˉsymbolˉkind.Function, 0, 0, 6),
                new(
                    "Console_write",
                    Objectˉsymbolˉbinding.Import,
                    Objectˉsymbolˉkind.Function,
                    Objectˉlimits.UNDEFINED_SECTION,
                    0,
                    0),
            ],
            [new(Objectˉrelocationˉkind.Relativeˉi32, 0, 1, 2, -4)]);
    }

    private static Bytecodeˉmodule Buildˉmodule(
        ImmutableArray<byte> code,
        Valueˉtype returnˉtype,
        int maximumˉstack)
    {
        return new(
            "Verifierˉcase",
            Moduleˉprofile.Portable,
            [],
            [],
            [new("Main", [], returnˉtype, [], 0, code.Length, maximumˉstack)],
            code,
            [new("Main", Exportˉkind.Function, 0)]);
    }

    private static byte[] I32ˉinstruction(int value)
    {
        var Result = new byte[5];
        Result[0] = (byte)Opcode.I32ˉconst;
        BinaryPrimitives.WriteInt32LittleEndian(Result.AsSpan(1), value);
        return Result;
    }

    private static byte[] Boolˉinstruction(bool value)
    {
        return [(byte)Opcode.Boolˉconst, value ? (byte)1 : (byte)0];
    }

    private static byte[] U32ˉinstruction(Opcode opcode, uint value)
    {
        var Result = new byte[5];
        Result[0] = (byte)opcode;
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(1), value);
        return Result;
    }

    private static byte[] Twoˉu32ˉinstruction(Opcode opcode, uint first, uint second)
    {
        var Result = new byte[9];
        Result[0] = (byte)opcode;
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(1), first);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(5), second);
        return Result;
    }

    private static int Findˉsectionˉpayload(byte[] bytes, Sectionˉkind kind)
    {
        var Offset = 12;
        for (var Index = 0; Index < Bytecodeˉlimits.SECTION_COUNT; Index++)
        {
            var Currentˉkind = (Sectionˉkind)bytes[Offset];
            var Length = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(Offset + 4)));
            if (Currentˉkind == kind)
            {
                return Offset + 8;
            }

            Offset = checked(Offset + 8 + Length);
        }

        throw new InvalidOperationException($"Section '{kind}' was not found.");
    }

    private static void Throwsˉbytecode(string expectedˉcode, Action action)
    {
        try
        {
            action();
        }
        catch (Bytecodeˉexception Exception)
        {
            Equal(expectedˉcode, Exception.Code);
            return;
        }

        throw new InvalidOperationException($"Expected bytecode failure {expectedˉcode}.");
    }

    private static void Throwsˉobject(string expectedˉcode, Action action)
    {
        try
        {
            action();
        }
        catch (Objectˉexception Exception)
        {
            Equal(expectedˉcode, Exception.Code);
            return;
        }

        throw new InvalidOperationException($"Expected object failure {expectedˉcode}.");
    }

    private static void Throwsˉruntime(string expectedˉcode, Action action)
    {
        try
        {
            action();
        }
        catch (Runtimeˉexception Exception)
        {
            Equal(expectedˉcode, Exception.Code);
            return;
        }

        throw new InvalidOperationException($"Expected runtime failure {expectedˉcode}.");
    }

    private static void Throwsˉnative(string expectedˉcode, Action action)
    {
        try
        {
            action();
        }
        catch (Nativeˉbackendˉexception Exception)
        {
            Equal(expectedˉcode, Exception.Code);
            return;
        }

        throw new InvalidOperationException($"Expected native backend failure {expectedˉcode}.");
    }

    private static void Throwsˉinvalidˉoperation(string expectedˉmessage, Action action)
    {
        try
        {
            action();
        }
        catch (InvalidOperationException Exception)
        {
            True(
                Exception.Message.Contains(expectedˉmessage, StringComparison.Ordinal),
                $"Invalid-operation message omitted '{expectedˉmessage}': {Exception.Message}");
            return;
        }

        throw new InvalidOperationException(
            $"Expected invalid-operation failure containing '{expectedˉmessage}'.");
    }

    private static void Throwsˉinvalidˉdata(Action action)
    {
        try
        {
            action();
        }
        catch (InvalidDataException)
        {
            return;
        }

        throw new InvalidOperationException("Expected invalid WebAssembly data failure.");
    }

    private static void Throwsˉnativeˉtrap(string expectedˉcode, Action action)
    {
        try
        {
            action();
        }
        catch (Nativeˉtrapˉexception Exception)
        {
            Equal(expectedˉcode, Exception.Code);
            return;
        }

        throw new InvalidOperationException($"Expected native trap {expectedˉcode}.");
    }

    private sealed class Nativeˉoutputˉcapture : IDisposable
    {
        private readonly string Pathˉname;
        private readonly FileStream Stream;
        private bool Isˉdisposed;

        public Nativeˉoutputˉcapture()
        {
            Pathˉname = Path.Combine(
                Path.GetTempPath(),
                $"windvale-native-output-{Guid.NewGuid():N}.tmp");
            Stream = new FileStream(
                Pathˉname,
                FileMode.CreateNew,
                FileAccess.ReadWrite,
                FileShare.ReadWrite | FileShare.Delete);
            Channel = Nativeˉoutputˉchannel.Fromˉfileˉhandle(Stream.SafeFileHandle);
        }

        public Nativeˉoutputˉchannel Channel { get; }

        public string Readˉtext()
        {
            Stream.Flush(flushToDisk: true);
            Stream.Position = 0;
            var Bytes = new byte[checked((int)Stream.Length)];
            Stream.ReadExactly(Bytes);
            Stream.Position = Stream.Length;
            return new System.Text.UTF8Encoding(
                encoderShouldEmitUTF8Identifier: false,
                throwOnInvalidBytes: true).GetString(Bytes);
        }

        public void Dispose()
        {
            if (Isˉdisposed)
            {
                return;
            }
            Isˉdisposed = true;
            Stream.Dispose();
            File.Delete(Pathˉname);
        }
    }

    private sealed class Failingˉtextˉwriter : TextWriter
    {
        public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;

        public override void Write(string? value) =>
            throw new IOException("Deliberate output failure.");
    }

    private static void Writeˉreport(string path)
    {
        var Report = new Conformanceˉreport(
            Contract ?? throw new InvalidOperationException("The golden contract test did not run."),
            new(
                Getˉosˉfamily(),
                RuntimeInformation.OSDescription,
                RuntimeInformation.OSArchitecture.ToString(),
                RuntimeInformation.FrameworkDescription));
        var Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        };
        var Fullˉpath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(Fullˉpath)!);
        File.WriteAllText(Fullˉpath, JsonSerializer.Serialize(Report, Options) + Environment.NewLine);
        Console.WriteLine($"Conformance report: {Fullˉpath}");
    }

    private static void Writeˉtimingˉreport(
        string path,
        string? filter,
        ImmutableHashSet<string> areas,
        bool failˉfast,
        int selected,
        long elapsedˉmilliseconds,
        List<Testˉtimingˉentry> tests)
    {
        Validateˉgoldenˉphaseˉtimings(tests);
        var Report = new Testˉtimingˉreport(
            filter,
            [.. areas.Order(StringComparer.Ordinal)],
            failˉfast,
            selected,
            tests.Count,
            elapsedˉmilliseconds,
            tests,
            [.. GOLDEN_PHASE_TIMINGS]);
        var Options = new JsonSerializerOptions { WriteIndented = true };
        var Fullˉpath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(Fullˉpath)!);
        File.WriteAllText(Fullˉpath, JsonSerializer.Serialize(Report, Options) + Environment.NewLine);
        Console.WriteLine($"Timing report: {Fullˉpath}");
    }

    private static void Validateˉgoldenˉphaseˉtimings(List<Testˉtimingˉentry> tests)
    {
        var Goldenˉtest = tests.SingleOrDefault(
            Test => StringComparer.Ordinal.Equals(Test.Name, GOLDEN_TEST_NAME));
        if (Goldenˉtest is null)
        {
            if (GOLDEN_PHASE_TIMINGS.Count != 0)
            {
                throw new InvalidOperationException(
                    "Golden phase timings were recorded without executing the golden test.");
            }

            return;
        }

        if (
            GOLDEN_PHASE_TIMINGS.Count > GOLDEN_PHASE_NAMES.Length ||
            !GOLDEN_PHASE_TIMINGS
                .Select(Phase => Phase.Name)
                .SequenceEqual(GOLDEN_PHASE_NAMES.Take(GOLDEN_PHASE_TIMINGS.Count), StringComparer.Ordinal)
        )
        {
            throw new InvalidOperationException("Golden phase timings are incomplete or out of canonical order.");
        }

        if (Goldenˉtest.Outcome == "passed" && GOLDEN_PHASE_TIMINGS.Count != GOLDEN_PHASE_NAMES.Length)
        {
            throw new InvalidOperationException("A passing golden test did not record every timing phase.");
        }

        if (GOLDEN_PHASE_TIMINGS.Any(Phase =>
            Phase.Elapsedˉmilliseconds < 0 ||
            Phase.Executedˉinstructions < 0 ||
            Phase.Allocatedˉbytes < 0 ||
            Phase.Generation0ˉcollections < 0 ||
            Phase.Generation1ˉcollections < 0 ||
            Phase.Generation2ˉcollections < 0))
        {
            throw new InvalidOperationException("Golden phase timings contain a negative metric.");
        }
    }

    private static int Compareˉreports(string firstˉpath, string secondˉpath)
    {
        var Options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var First = JsonSerializer.Deserialize<Conformanceˉreport>(File.ReadAllText(firstˉpath), Options)
            ?? throw new InvalidOperationException("The first report is invalid.");
        var Second = JsonSerializer.Deserialize<Conformanceˉreport>(File.ReadAllText(secondˉpath), Options)
            ?? throw new InvalidOperationException("The second report is invalid.");
        if (First.Contract != Second.Contract)
        {
            Console.Error.WriteLine("Cross-host conformance contracts differ.");
            Console.Error.WriteLine($"First:  {JsonSerializer.Serialize(First.Contract)}");
            Console.Error.WriteLine($"Second: {JsonSerializer.Serialize(Second.Contract)}");
            return 1;
        }

        var Hostˉfamilies = new HashSet<string>(StringComparer.Ordinal)
        {
            First.Host.Operatingˉsystemˉfamily,
            Second.Host.Operatingˉsystemˉfamily,
        };
        if (!Hostˉfamilies.SetEquals(["windows", "linux"]))
        {
            Console.Error.WriteLine(
                "Cross-host comparison requires one Windows report and one Linux report.");
            return 1;
        }

        Console.WriteLine("Cross-host conformance contracts match.");
        Console.WriteLine($"First host:  {First.Host.Operatingˉsystem} / {First.Host.Architecture}");
        Console.WriteLine($"Second host: {Second.Host.Operatingˉsystem} / {Second.Host.Architecture}");
        return 0;
    }

    private static string Getˉosˉfamily()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "windows";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return "linux";
        }

        return "other";
    }

    private static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"Expected '{expected}', found '{actual}'.");
        }
    }

    private static void Sequenceˉequal<T>(IEnumerable<T> expected, IEnumerable<T> actual)
    {
        if (!expected.SequenceEqual(actual))
        {
            throw new InvalidOperationException(
                $"Sequences differ. Expected [{string.Join(", ", expected)}], " +
                $"found [{string.Join(", ", actual)}].");
        }
    }

    private static void Contains(string value, string expectedˉfragment)
    {
        if (!value.Contains(expectedˉfragment, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Text does not contain '{expectedˉfragment}'.");
        }
    }

    private static string Readˉembeddedˉsource(string name)
    {
        using var Stream = typeof(Program).Assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"Embedded source '{name}' was not found.");
        using var Reader = new StreamReader(Stream);
        return Reader.ReadToEnd();
    }

    private static void True(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static void False(bool condition, string message)
    {
        True(!condition, message);
    }

    private sealed class Testˉfileˉreader(
        Func<string, int, ImmutableArray<byte>> read) : IHostedˉfileˉreader
    {
        public int Readˉcount { get; private set; }

        public ImmutableArray<byte> Readˉbytes(string resourceˉname, int maximumˉbytes)
        {
            Readˉcount++;
            return read(resourceˉname, maximumˉbytes);
        }
    }

    private sealed class WebAssemblyˉtestˉreader
    {
        private readonly byte[] Bytes;

        public WebAssemblyˉtestˉreader(ReadOnlySpan<byte> bytes)
        {
            Bytes = bytes.ToArray();
        }

        public int Position { get; private set; }

        public int Length => Bytes.Length;

        public void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidDataException(message);
            }
        }

        public byte Readˉbyte()
        {
            Require(Position < Bytes.Length, "The WebAssembly module is truncated.");
            return Bytes[Position++];
        }

        public uint Readˉuleb32()
        {
            uint Result = 0;
            for (var Index = 0; Index < 5; Index++)
            {
                var Value = Readˉbyte();
                var Payload = (uint)(Value & 0x7F);
                if (Index == 4)
                {
                    Require(Payload <= 0x0F, "The u32 LEB128 value exceeds 32 bits.");
                }
                Result |= Payload << (Index * 7);
                if ((Value & 0x80) == 0)
                {
                    return Result;
                }
            }
            throw new InvalidDataException("The u32 LEB128 value is unterminated.");
        }

        public int Readˉsleb32()
        {
            long Result = 0;
            for (var Index = 0; Index < 5; Index++)
            {
                var Value = Readˉbyte();
                var Payload = Value & 0x7F;
                Result |= (long)Payload << (Index * 7);
                if ((Value & 0x80) != 0)
                {
                    continue;
                }

                if (Index == 4)
                {
                    var Negative = (Payload & 0x08) != 0;
                    Require(
                        Negative
                            ? (Payload & 0x70) == 0x70
                            : (Payload & 0x70) == 0,
                        "The i32 LEB128 value has invalid unused bits.");
                    return unchecked((int)(uint)Result);
                }

                var Shift = (Index + 1) * 7;
                if ((Value & 0x40) != 0)
                {
                    Result |= -1L << Shift;
                }
                Require(Result is >= int.MinValue and <= int.MaxValue,
                    "The signed LEB128 value exceeds i32.");
                return (int)Result;
            }
            throw new InvalidDataException("The i32 LEB128 value is unterminated.");
        }

        public int Readˉsection(byte expectedˉkind)
        {
            Require(Readˉbyte() == expectedˉkind, "The WebAssembly section order is invalid.");
            var Sectionˉlength = Readˉuleb32();
            Require(Sectionˉlength <= int.MaxValue, "The WebAssembly section is oversized.");
            Require(
                Position <= Bytes.Length - (int)Sectionˉlength,
                "The WebAssembly section is truncated.");
            return Position + (int)Sectionˉlength;
        }

        public void Readˉheader()
        {
            Require(Bytes.Length >= 8, "The WebAssembly header is truncated.");
            Require(Readˉbyte() == 0x00, "The WebAssembly magic is invalid.");
            Require(Readˉbyte() == 0x61, "The WebAssembly magic is invalid.");
            Require(Readˉbyte() == 0x73, "The WebAssembly magic is invalid.");
            Require(Readˉbyte() == 0x6D, "The WebAssembly magic is invalid.");
            Require(Readˉbyte() == 0x01, "The WebAssembly version is invalid.");
            Require(Readˉbyte() == 0x00, "The WebAssembly version is invalid.");
            Require(Readˉbyte() == 0x00, "The WebAssembly version is invalid.");
            Require(Readˉbyte() == 0x00, "The WebAssembly version is invalid.");
        }

        public void Readˉindexed(byte opcode, uint index)
        {
            Require(Readˉbyte() == opcode, "The WebAssembly indexed opcode is invalid.");
            Require(Readˉuleb32() == index, "The WebAssembly instruction index is invalid.");
        }

        public int Readˉi32ˉconstant()
        {
            Require(Readˉbyte() == 0x41, "The WebAssembly i32 constant opcode is invalid.");
            return Readˉsleb32();
        }

        public void Readˉglobal(byte mutable, int initial)
        {
            Require(Readˉbyte() == 0x7F, "The WebAssembly global type is invalid.");
            Require(Readˉbyte() == mutable, "The WebAssembly global mutability is invalid.");
            Require(Readˉi32ˉconstant() == initial, "The WebAssembly global initializer is invalid.");
            Require(Readˉbyte() == 0x0B, "The WebAssembly global initializer is unterminated.");
        }

        public void Readˉexport(string name, byte kind, uint index)
        {
            Require(Readˉuleb32() == (uint)name.Length, "The WebAssembly export name length is invalid.");
            foreach (var Character in name)
            {
                Require(Readˉbyte() == (byte)Character, "The WebAssembly export name is invalid.");
            }
            Require(Readˉbyte() == kind, "The WebAssembly export kind is invalid.");
            Require(Readˉuleb32() == index, "The WebAssembly export index is invalid.");
        }
    }

    private sealed record Compilerˉsourceˉparserˉrunˉresult(
        int Exitˉcode,
        string Output,
        string Diagnostics,
        int Readˉcount,
        long Executedˉinstructions);

    private sealed record WebAssemblyˉtoolˉresult(
        int Exitˉcode,
        string Output,
        string Diagnostics,
        int Readˉcount,
        int Writeˉcount,
        string Writtenˉresourceˉname,
        ImmutableArray<byte> Writtenˉbytes,
        long Executedˉinstructions);

    private sealed record WebAssemblyˉexecutionˉresult(
        int Status,
        int Result,
        long Executedˉinstructions);

    private sealed record Wvˉlinkerˉscanˉresult(
        int Exitˉcode,
        string Output,
        string Diagnostics,
        long Executedˉinstructions);

    private sealed record Wvˉlinkerˉanalysisˉresult(
        int Exitˉcode,
        string Output,
        string Diagnostics,
        int Readˉcount,
        int Writeˉcount,
        string Writtenˉresourceˉname,
        ImmutableArray<byte> Writtenˉbytes,
        long Executedˉinstructions);

    private sealed class Capturingˉfileˉwriter : IHostedˉfileˉwriter
    {
        public int Writeˉcount { get; private set; }

        public string Resourceˉname { get; private set; } = string.Empty;

        public ImmutableArray<byte> Bytes { get; private set; } = [];

        public void Writeˉbytes(
            string resourceˉname,
            ImmutableArray<byte> bytes,
            int maximumˉbytes)
        {
            if (bytes.IsDefault || bytes.Length > maximumˉbytes)
            {
                throw new InvalidOperationException("The runtime passed invalid bytes to the hosted writer.");
            }
            Writeˉcount++;
            Resourceˉname = resourceˉname;
            Bytes = bytes;
        }
    }

    private sealed class Invalidˉresultˉcapabilityˉhost : ICapabilityˉhost
    {
        public bool Supports(string capabilityˉname) => true;

        public Runtimeˉvalue? Invoke(
            Capabilityˉdeclaration capability,
            ImmutableArray<Runtimeˉvalue> arguments)
        {
            return Runtimeˉvalue.Fromˉi32(1);
        }
    }

    private sealed class Goldenˉphaseˉrecorder(
        List<Goldenˉphaseˉtimingˉentry>? entries) : IDisposable
    {
        private string? Currentˉname;
        private long Startedˉtimestamp;
        private long Startedˉallocatedˉbytes;
        private int Startedˉgeneration0ˉcollections;
        private int Startedˉgeneration1ˉcollections;
        private int Startedˉgeneration2ˉcollections;
        private long Executedˉinstructions;

        public void Start(string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            Finishˉcurrent();
            if (entries is null)
            {
                return;
            }

            Currentˉname = name;
            Startedˉtimestamp = Stopwatch.GetTimestamp();
            Startedˉallocatedˉbytes = GC.GetAllocatedBytesForCurrentThread();
            Startedˉgeneration0ˉcollections = GC.CollectionCount(0);
            Startedˉgeneration1ˉcollections = GC.CollectionCount(1);
            Startedˉgeneration2ˉcollections = GC.CollectionCount(2);
            Executedˉinstructions = 0;
        }

        public void Addˉexecutedˉinstructions(long count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (Currentˉname is not null)
            {
                Executedˉinstructions = checked(Executedˉinstructions + count);
            }
        }

        public void Dispose()
        {
            Finishˉcurrent();
        }

        private void Finishˉcurrent()
        {
            if (entries is null || Currentˉname is null)
            {
                return;
            }

            var Elapsedˉmilliseconds =
                Stopwatch.GetElapsedTime(Startedˉtimestamp).Ticks / TimeSpan.TicksPerMillisecond;
            var Allocatedˉbytes =
                GC.GetAllocatedBytesForCurrentThread() - Startedˉallocatedˉbytes;
            entries.Add(new(
                Currentˉname,
                Elapsedˉmilliseconds,
                Executedˉinstructions,
                Allocatedˉbytes,
                GC.CollectionCount(0) - Startedˉgeneration0ˉcollections,
                GC.CollectionCount(1) - Startedˉgeneration1ˉcollections,
                GC.CollectionCount(2) - Startedˉgeneration2ˉcollections));
            Currentˉname = null;
        }
    }

    private sealed record Testˉcase(
        string Name,
        ImmutableArray<string> Areas,
        Action Body);

    private sealed record Testˉrunnerˉoptions(
        string? Reportˉpath,
        string? Filter,
        ImmutableHashSet<string> Areas,
        bool Failˉfast,
        string? Timingˉreportˉpath,
        bool Listˉtests,
        bool Listˉareas)
    {
        public static Testˉrunnerˉoptions Empty { get; } = new(
            null,
            null,
            ImmutableHashSet<string>.Empty,
            false,
            null,
            false,
            false);
    }

    private sealed record Testˉtimingˉentry(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("outcome")] string Outcome,
        [property: JsonPropertyName("elapsedMilliseconds")] long Elapsedˉmilliseconds);

    private sealed record Goldenˉphaseˉtimingˉentry(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("elapsedMilliseconds")] long Elapsedˉmilliseconds,
        [property: JsonPropertyName("executedInstructions")] long Executedˉinstructions,
        [property: JsonPropertyName("allocatedBytes")] long Allocatedˉbytes,
        [property: JsonPropertyName("generation0Collections")] int Generation0ˉcollections,
        [property: JsonPropertyName("generation1Collections")] int Generation1ˉcollections,
        [property: JsonPropertyName("generation2Collections")] int Generation2ˉcollections);

    private sealed record Testˉtimingˉreport(
        [property: JsonPropertyName("filter")] string? Filter,
        [property: JsonPropertyName("areas")] ImmutableArray<string> Areas,
        [property: JsonPropertyName("failFast")] bool Failˉfast,
        [property: JsonPropertyName("selected")] int Selected,
        [property: JsonPropertyName("executed")] int Executed,
        [property: JsonPropertyName("elapsedMilliseconds")] long Elapsedˉmilliseconds,
        [property: JsonPropertyName("tests")] List<Testˉtimingˉentry> Tests,
        [property: JsonPropertyName("goldenPhases")] ImmutableArray<Goldenˉphaseˉtimingˉentry> Goldenˉphases);

    private sealed record Conformanceˉcontract(
        [property: JsonPropertyName("moduleFormat")] string Moduleˉformat,
        [property: JsonPropertyName("objectFormat")] string Objectˉformat,
        [property: JsonPropertyName("assemblyFormat")] string Assemblyˉformat,
        [property: JsonPropertyName("assemblyObjectSha256")] string Assemblyˉobjectˉsha256,
        [property: JsonPropertyName("wvaAssemblerCoreSha256")] string Wvaˉassemblerˉcoreˉsha256,
        [property: JsonPropertyName("wvaAssemblerCoreResult")] int Wvaˉassemblerˉcoreˉresult,
        [property: JsonPropertyName("wvaAssemblerHostedOutput")] string Wvaˉassemblerˉhostedˉoutput,
        [property: JsonPropertyName("wvaAssemblerObjectSha256")] string Wvaˉassemblerˉobjectˉsha256,
        [property: JsonPropertyName("wvLinkerCoreSha256")] string Wvˉlinkerˉcoreˉsha256,
        [property: JsonPropertyName("wvLinkerCoreResult")] int Wvˉlinkerˉcoreˉresult,
        [property: JsonPropertyName("wvLinkerHostedOutput")] string Wvˉlinkerˉhostedˉoutput,
        [property: JsonPropertyName("wvLinkerMapOutput")] string Wvˉlinkerˉmapˉoutput,
        [property: JsonPropertyName("linkFormat")] string Linkˉformat,
        [property: JsonPropertyName("linkImageSha256")] string Linkˉimageˉsha256,
        [property: JsonPropertyName("linkMapSha256")] string Linkˉmapˉsha256,
        [property: JsonPropertyName("linkMap")] string Linkˉmap,
        [property: JsonPropertyName("sumSha256")] string Sumˉsha256,
        [property: JsonPropertyName("sumResult")] int Sumˉresult,
        [property: JsonPropertyName("helloSha256")] string Helloˉsha256,
        [property: JsonPropertyName("helloOutput")] string Helloˉoutput,
        [property: JsonPropertyName("helloResult")] int Helloˉresult,
        [property: JsonPropertyName("foundationSha256")] string Foundationˉsha256,
        [property: JsonPropertyName("foundationResult")] int Foundationˉresult,
        [property: JsonPropertyName("sourceCompositionSha256")] string Sourceˉcompositionˉsha256,
        [property: JsonPropertyName("sourceCompositionResult")] int Sourceˉcompositionˉresult,
        [property: JsonPropertyName("machineContractsSha256")] string Machineˉcontractsˉsha256,
        [property: JsonPropertyName("machineContractsDemoSha256")] string Machineˉcontractsˉdemoˉsha256,
        [property: JsonPropertyName("machineContractsDemoResult")] int Machineˉcontractsˉdemoˉresult,
        [property: JsonPropertyName("byteOrderingSha256")] string Byteˉorderingˉsha256,
        [property: JsonPropertyName("byteOrderingDemoSha256")] string Byteˉorderingˉdemoˉsha256,
        [property: JsonPropertyName("byteOrderingDemoResult")] int Byteˉorderingˉdemoˉresult,
        [property: JsonPropertyName("decimalParsingSha256")] string Decimalˉparsingˉsha256,
        [property: JsonPropertyName("decimalParsingDemoSha256")] string Decimalˉparsingˉdemoˉsha256,
        [property: JsonPropertyName("decimalParsingDemoResult")] int Decimalˉparsingˉdemoˉresult,
        [property: JsonPropertyName("byteConstructionSha256")] string Byteˉconstructionˉsha256,
        [property: JsonPropertyName("byteConstructionDemoSha256")] string Byteˉconstructionˉdemoˉsha256,
        [property: JsonPropertyName("byteConstructionDemoResult")] int Byteˉconstructionˉdemoˉresult,
        [property: JsonPropertyName("nativeStencilCoreSha256")] string Nativeˉstencilˉcoreˉsha256,
        [property: JsonPropertyName("nativeStencilDemoSha256")] string Nativeˉstencilˉdemoˉsha256,
        [property: JsonPropertyName("nativeStencilDemoResult")] int Nativeˉstencilˉdemoˉresult,
        [property: JsonPropertyName("sourceLexerSha256")] string Sourceˉlexerˉsha256,
        [property: JsonPropertyName("sourceLexerDemoSha256")] string Sourceˉlexerˉdemoˉsha256,
        [property: JsonPropertyName("sourceLexerDemoResult")] int Sourceˉlexerˉdemoˉresult,
        [property: JsonPropertyName("sourceDeclarationParserSha256")] string Sourceˉdeclarationˉparserˉsha256,
        [property: JsonPropertyName("sourceDeclarationParserDemoSha256")] string Sourceˉdeclarationˉparserˉdemoˉsha256,
        [property: JsonPropertyName("sourceDeclarationParserDemoResult")] int Sourceˉdeclarationˉparserˉdemoˉresult,
        [property: JsonPropertyName("sourceDeclarationParserToolSha256")] string Sourceˉdeclarationˉparserˉtoolˉsha256,
        [property: JsonPropertyName("sourceLexerDeclarationOutput")] string Sourceˉlexerˉdeclarationˉoutput,
        [property: JsonPropertyName("sourceParserSelfDeclarationOutput")] string Sourceˉparserˉselfˉdeclarationˉoutput,
        [property: JsonPropertyName("sourceBodyParserSha256")] string Sourceˉbodyˉparserˉsha256,
        [property: JsonPropertyName("sourceBodyParserDemoSha256")] string Sourceˉbodyˉparserˉdemoˉsha256,
        [property: JsonPropertyName("sourceBodyParserDemoResult")] int Sourceˉbodyˉparserˉdemoˉresult,
        [property: JsonPropertyName("sourceBodyParserToolSha256")] string Sourceˉbodyˉparserˉtoolˉsha256,
        [property: JsonPropertyName("sourceLexerBodyOutput")] string Sourceˉlexerˉbodyˉoutput,
        [property: JsonPropertyName("sourceDeclarationBodyOutput")] string Sourceˉdeclarationˉbodyˉoutput,
        [property: JsonPropertyName("sourceBodySelfOutput")] string Sourceˉbodyˉselfˉoutput,
        [property: JsonPropertyName("sourceSetSha256")] string Sourceˉsetˉsha256,
        [property: JsonPropertyName("sourceSetDemoSha256")] string Sourceˉsetˉdemoˉsha256,
        [property: JsonPropertyName("sourceSetDemoResult")] int Sourceˉsetˉdemoˉresult,
        [property: JsonPropertyName("sourceSetToolSha256")] string Sourceˉsetˉtoolˉsha256,
        [property: JsonPropertyName("sourceSetSelfOutput")] string Sourceˉsetˉselfˉoutput,
        [property: JsonPropertyName("sourceGraphSha256")] string Sourceˉgraphˉsha256,
        [property: JsonPropertyName("sourceGraphDemoSha256")] string Sourceˉgraphˉdemoˉsha256,
        [property: JsonPropertyName("sourceGraphDemoResult")] int Sourceˉgraphˉdemoˉresult,
        [property: JsonPropertyName("sourceGraphToolSha256")] string Sourceˉgraphˉtoolˉsha256,
        [property: JsonPropertyName("sourceGraphSelfOutput")] string Sourceˉgraphˉselfˉoutput,
        [property: JsonPropertyName("sourceSymbolsSha256")] string Sourceˉsymbolsˉsha256,
        [property: JsonPropertyName("sourceSymbolsDemoSha256")] string Sourceˉsymbolsˉdemoˉsha256,
        [property: JsonPropertyName("sourceSymbolsDemoResult")] int Sourceˉsymbolsˉdemoˉresult,
        [property: JsonPropertyName("sourceSymbolsToolSha256")] string Sourceˉsymbolsˉtoolˉsha256,
        [property: JsonPropertyName("sourceSymbolsSelfOutput")] string Sourceˉsymbolsˉselfˉoutput,
        [property: JsonPropertyName("sourceBindingsSha256")] string Sourceˉbindingsˉsha256,
        [property: JsonPropertyName("sourceBindingsDemoSha256")] string Sourceˉbindingsˉdemoˉsha256,
        [property: JsonPropertyName("sourceBindingsDemoResult")] int Sourceˉbindingsˉdemoˉresult,
        [property: JsonPropertyName("sourceBindingsToolSha256")] string Sourceˉbindingsˉtoolˉsha256,
        [property: JsonPropertyName("sourceBindingsSelfOutput")] string Sourceˉbindingsˉselfˉoutput,
        [property: JsonPropertyName("wvdumpCoreSha256")] string Wvˉdumpˉcoreˉsha256,
        [property: JsonPropertyName("wvdumpCoreResult")] int Wvˉdumpˉcoreˉresult,
        [property: JsonPropertyName("wvdumpHostedOutput")] string Wvˉdumpˉhostedˉoutput,
        [property: JsonPropertyName("wvoSampleSha256")] string Wvoˉsampleˉsha256,
        [property: JsonPropertyName("wvoCoreSha256")] string Wvoˉcoreˉsha256,
        [property: JsonPropertyName("wvoCoreResult")] int Wvoˉcoreˉresult,
        [property: JsonPropertyName("wvoHostedOutput")] string Wvoˉhostedˉoutput);

    private sealed record Hostˉreport(
        [property: JsonPropertyName("operatingSystemFamily")] string Operatingˉsystemˉfamily,
        [property: JsonPropertyName("operatingSystem")] string Operatingˉsystem,
        [property: JsonPropertyName("architecture")] string Architecture,
        [property: JsonPropertyName("framework")] string Framework);

    private sealed record Conformanceˉreport(
        [property: JsonPropertyName("contract")] Conformanceˉcontract Contract,
        [property: JsonPropertyName("host")] Hostˉreport Host);
}
