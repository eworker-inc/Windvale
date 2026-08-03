using System.Collections.Immutable;

namespace Windvale.Bytecode;

public static class Capabilityˉcatalog
{
    public const string CONSOLE_WRITE = "console.write";
    public const string CONSOLE_WRITE_LINE = "console.write_line";
    public const string DIAGNOSTIC_WRITE_LINE = "diagnostic.write_line";
    public const string FILESYSTEM_DIRECTORY_READ_V1 = "filesystem.directory_read_v1";
    public const string FILE_READ_BYTES = "file.read_bytes";
    public const string FILE_WRITE_BYTES = "file.write_bytes";
    public const string PROCESS_ARGUMENT = "process.argument";
    public const string PROCESS_ARGUMENT_COUNT = "process.argument_count";

    private static readonly ImmutableDictionary<string, Capabilityˉdeclaration> DECLARATIONS =
        new Dictionary<string, Capabilityˉdeclaration>(StringComparer.Ordinal)
        {
            [CONSOLE_WRITE] = new(
                CONSOLE_WRITE,
                [Valueˉtype.Text],
                Valueˉtype.Void),
            [CONSOLE_WRITE_LINE] = new(
                CONSOLE_WRITE_LINE,
                [Valueˉtype.Text],
                Valueˉtype.Void),
            [DIAGNOSTIC_WRITE_LINE] = new(
                DIAGNOSTIC_WRITE_LINE,
                [Valueˉtype.Text],
                Valueˉtype.Void),
            [FILESYSTEM_DIRECTORY_READ_V1] = new(
                FILESYSTEM_DIRECTORY_READ_V1,
                [Valueˉtype.Text, Valueˉtype.U32, Valueˉtype.U32],
                Valueˉtype.Bytes),
            [FILE_READ_BYTES] = new(
                FILE_READ_BYTES,
                [Valueˉtype.Text],
                Valueˉtype.Bytes),
            [FILE_WRITE_BYTES] = new(
                FILE_WRITE_BYTES,
                [Valueˉtype.Text, Valueˉtype.Bytes],
                Valueˉtype.Void),
            [PROCESS_ARGUMENT] = new(
                PROCESS_ARGUMENT,
                [Valueˉtype.U32],
                Valueˉtype.Text),
            [PROCESS_ARGUMENT_COUNT] = new(
                PROCESS_ARGUMENT_COUNT,
                [],
                Valueˉtype.U32),
        }.ToImmutableDictionary(StringComparer.Ordinal);

    public static bool Tryˉget(string name, out Capabilityˉdeclaration declaration)
    {
        return DECLARATIONS.TryGetValue(name, out declaration!);
    }
}
