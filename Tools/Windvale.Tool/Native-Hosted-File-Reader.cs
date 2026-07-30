using System.Collections.Immutable;
using System.Security;
using Windvale.Runtime;

namespace Windvale.Tool;

internal sealed class Nativeˉhostedˉfileˉreader : IHostedˉfileˉreader
{
    private const int COPY_BUFFER_BYTES = 64 * 1024;

    public ImmutableArray<byte> Readˉbytes(string resourceˉname, int maximumˉbytes)
    {
        if (maximumˉbytes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumˉbytes));
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
            using var Input = new FileStream(
                Fullˉpath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                COPY_BUFFER_BYTES,
                FileOptions.SequentialScan);
            if (Input.Length > maximumˉbytes)
            {
                throw Tooˉlarge(Fullˉpath, maximumˉbytes);
            }

            using var Output = new MemoryStream(checked((int)Input.Length));
            var Buffer = new byte[COPY_BUFFER_BYTES];
            while (true)
            {
                var Read = Input.Read(Buffer, 0, Buffer.Length);
                if (Read == 0)
                {
                    break;
                }

                if (Read > maximumˉbytes - Output.Length)
                {
                    throw Tooˉlarge(Fullˉpath, maximumˉbytes);
                }

                Output.Write(Buffer, 0, Read);
            }

            return ImmutableArray.Create(Output.ToArray());
        }
        catch (Hostedˉfileˉexception)
        {
            throw;
        }
        catch (Exception Exception) when (Exception is FileNotFoundException or DirectoryNotFoundException)
        {
            throw new Hostedˉfileˉexception(
                Hostedˉfileˉerror.Notˉfound,
                $"Hosted file resource '{Fullˉpath}' was not found.");
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

    private static Hostedˉfileˉexception Tooˉlarge(string fullˉpath, int maximumˉbytes)
    {
        return new(
            Hostedˉfileˉerror.Tooˉlarge,
            $"Hosted file resource '{fullˉpath}' exceeds the {maximumˉbytes}-byte limit.");
    }
}
