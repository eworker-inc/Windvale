using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Runtime;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const string WVB_PUBLICATION_TRANSACTION_SHA256 =
        "4be7e3948576498963f2858fd95d7273d6b63d467fbbb2b344c86c223a8864ce";

    private static readonly string WVB_PUBLICATION_TRANSACTION_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Wvb-Publication-Transaction.wv");
    private static readonly string WVB_PUBLICATION_TRANSACTION_ADAPTER_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Wvb-Publication-Transaction-Adapter.wv");

    private static void Wvbˉpublicationˉtransactionˉruns()
    {
        Compilationˉresult Compile()
        {
            return Seedˉcompiler.Compileˉmodules(
                new(
                    "Wvb-Publication-Transaction-Adapter.wv",
                    WVB_PUBLICATION_TRANSACTION_ADAPTER_SOURCE),
                [
                    new Sourceˉmoduleˉinput(
                        "Wvb-Publication-Transaction.wv",
                        WVB_PUBLICATION_TRANSACTION_SOURCE),
                ]);
        }

        var First = Compile();
        True(
            First.Success,
            "The WVB publication transaction fixture did not compile: " +
                string.Join(" | ", First.Diagnostics));
        var Second = Compile();
        True(
            Second.Success,
            "The repeated WVB publication transaction fixture did not compile: " +
                string.Join(" | ", Second.Diagnostics));
        Sequenceˉequal(First.Moduleˉbytes, Second.Moduleˉbytes);
        Equal(
            WVB_PUBLICATION_TRANSACTION_SHA256,
            Moduleˉdigest.Calculateˉsha256(First.Moduleˉbytes.AsSpan()));

        var Verified = Moduleˉcodec.Readˉandˉverify(First.Moduleˉbytes.AsSpan());
        Equal("Wvbˉpublicationˉtransactionˉadapter", Verified.Module.Name);
        Equal(Moduleˉprofile.Portable, Verified.Module.Profile);
        Equal(4, Verified.Module.Types.Length);
        Equal(9, Verified.Module.Functions.Length);
        Sequenceˉequal(["Main"], Verified.Module.Exports.Select(Item => Item.Name));

        var Result = new Referenceˉruntime(
            Verified,
            new Referenceˉcapabilityˉhost(new StringWriter()),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        Equal(0, Result.Exitˉcode);
    }
}
