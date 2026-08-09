using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.Runtime.Native;

namespace Windvale.Tool;

internal static class Stage0ˉrecoveryˉaotˉtargets
{
    internal static bool Contains(string target) => target is
        Windowsˉconsoleˉapplicationˉcontract.HOSTED_CONTAINER_SEGMENTER_TARGET_NAME or
        Linuxˉconsoleˉapplicationˉcontract.HOSTED_CONTAINER_SEGMENTER_TARGET_NAME or
        Hostedˉcontainerˉplannerˉapplicationˉcontract.WINDOWS_TARGET_NAME or
        Hostedˉcontainerˉplannerˉapplicationˉcontract.LINUX_TARGET_NAME or
        Hostedˉcontainerˉplatformˉbytesˉapplicationˉcontract.WINDOWS_TARGET_NAME or
        Hostedˉcontainerˉplatformˉbytesˉapplicationˉcontract.LINUX_TARGET_NAME or
        Hostedˉcontainerˉstartupˉapplicationˉcontract.WINDOWS_TARGET_NAME or
        Hostedˉcontainerˉstartupˉapplicationˉcontract.LINUX_TARGET_NAME or
        Hostedˉcontainerˉruntimeˉapplicationˉcontract.WINDOWS_TARGET_NAME or
        Hostedˉcontainerˉruntimeˉapplicationˉcontract.LINUX_TARGET_NAME or
        Hostedˉcontainerˉmetadataˉapplicationˉcontract.WINDOWS_TARGET_NAME or
        Hostedˉcontainerˉmetadataˉapplicationˉcontract.LINUX_TARGET_NAME or
        Hostedˉserviceˉbundleˉapplicationˉcontract.WINDOWS_TARGET_NAME or
        Hostedˉserviceˉbundleˉapplicationˉcontract.LINUX_TARGET_NAME or
        Hostedˉmetadataˉrequestˉapplicationˉcontract.WINDOWS_TARGET_NAME or
        Hostedˉmetadataˉrequestˉapplicationˉcontract.LINUX_TARGET_NAME or
        Hostedˉverifierˉmetadataˉrequestˉapplicationˉcontract.WINDOWS_TARGET_NAME or
        Hostedˉverifierˉmetadataˉrequestˉapplicationˉcontract.LINUX_TARGET_NAME or
        Hostedˉserviceˉbundleˉrequestˉapplicationˉcontract.WINDOWS_TARGET_NAME or
        Hostedˉserviceˉbundleˉrequestˉapplicationˉcontract.LINUX_TARGET_NAME or
        Hostedˉcontainerˉsegmentˉrequestˉapplicationˉcontract.WINDOWS_TARGET_NAME or
        Hostedˉcontainerˉsegmentˉrequestˉapplicationˉcontract.LINUX_TARGET_NAME or
        Hostedˉpublicationˉrequestˉapplicationˉcontract.WINDOWS_TARGET_NAME or
        Hostedˉpublicationˉrequestˉapplicationˉcontract.LINUX_TARGET_NAME or
        Hostedˉsourceˉgeometryˉapplicationˉcontract.WINDOWS_TARGET_NAME or
        Hostedˉsourceˉgeometryˉapplicationˉcontract.LINUX_TARGET_NAME or
        Hostedˉfixedˉservicesˉapplicationˉcontract.WINDOWS_TARGET_NAME or
        Hostedˉfixedˉservicesˉapplicationˉcontract.LINUX_TARGET_NAME or
        Hostedˉorchestrationˉcontrolˉapplicationˉcontract.WINDOWS_TARGET_NAME or
        Hostedˉorchestrationˉcontrolˉapplicationˉcontract.LINUX_TARGET_NAME or
        Hostedˉcontainerˉsourceˉsetˉapplicationˉcontract.WINDOWS_TARGET_NAME or
        Hostedˉcontainerˉsourceˉsetˉapplicationˉcontract.LINUX_TARGET_NAME or
        Hostedˉcontainerˉsegmentˉmanifestˉapplicationˉcontract.WINDOWS_TARGET_NAME or
        Hostedˉcontainerˉsegmentˉmanifestˉapplicationˉcontract.LINUX_TARGET_NAME or
        Hostedˉenumˉrequestˉapplicationˉcontract.WINDOWS_TARGET_NAME or
        Hostedˉenumˉrequestˉapplicationˉcontract.LINUX_TARGET_NAME or
        Hostedˉenumˉserviceˉapplicationˉcontract.WINDOWS_TARGET_NAME or
        Hostedˉenumˉserviceˉapplicationˉcontract.LINUX_TARGET_NAME or
        Wvoˉstagingˉproducerˉapplicationˉcontract.WINDOWS_TARGET_NAME or
        Wvoˉstagingˉproducerˉapplicationˉcontract.LINUX_TARGET_NAME or
        Compilerˉimageˉstagingˉapplicationˉcontract.WINDOWS_TARGET_NAME or
        Compilerˉimageˉstagingˉapplicationˉcontract.LINUX_TARGET_NAME or
        Compilerˉimageˉtransportˉapplicationˉcontract.WINDOWS_TARGET_NAME or
        Compilerˉimageˉtransportˉapplicationˉcontract.LINUX_TARGET_NAME or
        Hostedˉcontainerˉpublisherˉapplicationˉcontract.WINDOWS_TARGET_NAME or
        Hostedˉcontainerˉpublisherˉapplicationˉcontract.LINUX_TARGET_NAME;
}
