using System.Collections.Immutable;
using System.Security;
using Windvale.Runtime;

namespace Windvale.Tool;

internal sealed class Nativeˉhostedˉfileˉwriter : IHostedˉfileˉwriter
{
    public void Writeˉbytes(
        string resourceˉname,
        ImmutableArray<byte> bytes,
        int maximumˉbytes)
    {
        if (maximumˉbytes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumˉbytes));
        }
        if (bytes.IsDefault)
        {
            throw new ArgumentException("Hosted file bytes must be initialized.", nameof(bytes));
        }
        if (bytes.Length > maximumˉbytes)
        {
            throw new Hostedˉfileˉexception(
                Hostedˉfileˉerror.Tooˉlarge,
                $"Hosted file output exceeds the {maximumˉbytes}-byte limit.");
        }
        if (resourceˉname.Length == 0)
        {
            throw new Hostedˉfileˉexception(
                Hostedˉfileˉerror.Invalidˉname,
                "The hosted file resource name is empty.");
        }

        string Fullˉpath;
        try
        {
            Fullˉpath = Path.GetFullPath(resourceˉname);
        }
        catch (Exception Exception) when (
            Exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            throw new Hostedˉfileˉexception(
                Hostedˉfileˉerror.Invalidˉname,
                $"The hosted file resource name '{resourceˉname}' is invalid.");
        }

        try
        {
            using var Output = new FileStream(
                Fullˉpath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                64 * 1024,
                FileOptions.SequentialScan);
            Output.Write(bytes.AsSpan());
            Output.Flush(flushToDisk: true);
        }
        catch (Exception Exception) when (Exception is DirectoryNotFoundException)
        {
            throw new Hostedˉfileˉexception(
                Hostedˉfileˉerror.Notˉfound,
                $"The parent of hosted file resource '{Fullˉpath}' was not found.");
        }
        catch (Exception Exception) when (Exception is UnauthorizedAccessException or SecurityException)
        {
            throw new Hostedˉfileˉexception(
                Hostedˉfileˉerror.Permissionˉdenied,
                $"Hosted file resource '{Fullˉpath}' was denied.");
        }
        catch (PathTooLongException)
        {
            throw new Hostedˉfileˉexception(
                Hostedˉfileˉerror.Invalidˉname,
                $"The hosted file resource name '{resourceˉname}' is invalid.");
        }
        catch (IOException)
        {
            throw new Hostedˉfileˉexception(
                Hostedˉfileˉerror.Unavailable,
                $"Hosted file resource '{Fullˉpath}' is unavailable.");
        }
    }
}
