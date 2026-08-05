using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Runtime;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const string WVB_PUBLICATION_TRANSACTION_SHA256 =
        "a9c356ba0bcbd61fd6bac7afd40c10e752f3eedad729077d5abdc5518ae188a4";

    private static readonly string WVB_PUBLICATION_TRANSACTION_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Wvb-Publication-Transaction.wv");
    private static readonly string WVB_PUBLICATION_NATIVE_BRIDGE_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Wvb-Publication-Native-Bridge.wv");
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
                    new Sourceˉmoduleˉinput(
                        "Wvb-Publication-Native-Bridge.wv",
                        WVB_PUBLICATION_NATIVE_BRIDGE_SOURCE),
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
        Equal(13_617, First.Moduleˉbytes.Length);

        var Verified = Moduleˉcodec.Readˉandˉverify(First.Moduleˉbytes.AsSpan());
        Equal("Wvbˉpublicationˉtransactionˉadapter", Verified.Module.Name);
        Equal(Moduleˉprofile.Portable, Verified.Module.Profile);
        Equal(4, Verified.Module.Types.Length);
        Equal(18, Verified.Module.Functions.Length);
        Sequenceˉequal(["Main"], Verified.Module.Exports.Select(Item => Item.Name));

        var Result = new Referenceˉruntime(
            Verified,
            new Referenceˉcapabilityˉhost(new StringWriter()),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        Equal(0, Result.Exitˉcode);
    }
}
