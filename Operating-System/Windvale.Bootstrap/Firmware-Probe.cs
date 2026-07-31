using System.Collections.Immutable;
using System.Text;
using Windvale.Linker;
using Windvale.ObjectModel;

namespace Windvale.Bootstrap;

public static class Firmwareˉprobe
{
    public const int FORMAT_VERSION = 1;
    public const string ENTRY_SYMBOL = "Windvale_boot_probe";
    public const string SERIAL_MARKER = "windvale-os-boot 1\nstatus=pass\n";

    public static ImmutableArray<byte> Buildˉapplication()
    {
        var Code = Buildˉmachineˉcode();
        var Object = new Objectˉfile(
            Objectˉarchitecture.X86ˉ64,
            [new(".text", Objectˉsectionˉkind.Code, 16, (uint)Code.Length, Code)],
            [new(
                ENTRY_SYMBOL,
                Objectˉsymbolˉbinding.Export,
                Objectˉsymbolˉkind.Function,
                0,
                0,
                (uint)Code.Length)],
            []);
        var Objectˉbytes = Objectˉcodec.Write(Object).ToImmutableArray();
        var Link = Linkˉcompiler.Link(
            [new(Objectˉbytes)],
            new(Uefiˉapplicationˉcontract.REQUIRED_LINK_BASE_ADDRESS, ENTRY_SYMBOL));
        if (!Link.Success)
        {
            throw new InvalidOperationException(
                $"The firmware probe did not link: {Link.Diagnostics[0].Code}: {Link.Diagnostics[0].Message}");
        }

        var Application = Uefiˉapplicationˉwriter.Write(Link);
        if (!Application.Success)
        {
            throw new InvalidOperationException(
                $"The firmware probe did not encode: {Application.Diagnostics[0].Code}: {Application.Diagnostics[0].Message}");
        }
        return Application.Imageˉbytes;
    }

    private static ImmutableArray<byte> Buildˉmachineˉcode()
    {
        var Output = new List<byte>();

        Emitˉoutˉbyte(Output, 0x03F9, 0x00);
        Emitˉoutˉbyte(Output, 0x03FB, 0x80);
        Emitˉoutˉbyte(Output, 0x03F8, 0x03);
        Emitˉoutˉbyte(Output, 0x03F9, 0x00);
        Emitˉoutˉbyte(Output, 0x03FB, 0x03);
        Emitˉoutˉbyte(Output, 0x03FA, 0xC7);
        Emitˉoutˉbyte(Output, 0x03FC, 0x0B);

        foreach (var Value in Encoding.ASCII.GetBytes(SERIAL_MARKER))
        {
            Emitˉmoveˉedx(Output, 0x03FD);
            Output.Add(0xEC);
            Output.AddRange([0xA8, 0x20]);
            Output.AddRange([0x74, 0xFB]);
            Emitˉmoveˉedx(Output, 0x03F8);
            Emitˉmoveˉeax(Output, Value);
            Output.Add(0xEE);
        }

        Emitˉmoveˉedx(Output, 0x00F4);
        Output.AddRange([0x31, 0xC0]);
        Output.Add(0xEF);
        Output.AddRange([0x31, 0xC0]);
        Output.Add(0xC3);
        return Output.ToImmutableArray();
    }

    private static void Emitˉoutˉbyte(List<byte> output, uint port, byte value)
    {
        Emitˉmoveˉedx(output, port);
        Emitˉmoveˉeax(output, value);
        output.Add(0xEE);
    }

    private static void Emitˉmoveˉedx(List<byte> output, uint value)
    {
        output.Add(0xBA);
        Emitˉu32(output, value);
    }

    private static void Emitˉmoveˉeax(List<byte> output, uint value)
    {
        output.Add(0xB8);
        Emitˉu32(output, value);
    }

    private static void Emitˉu32(List<byte> output, uint value)
    {
        output.Add((byte)value);
        output.Add((byte)(value >> 8));
        output.Add((byte)(value >> 16));
        output.Add((byte)(value >> 24));
    }
}
