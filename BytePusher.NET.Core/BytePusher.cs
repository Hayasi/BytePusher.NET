namespace BytePusher.NET.Core;

public class BytePusher
{
    private static readonly Pixel[] palette = new Pixel[256];

    static BytePusher()
    {
        for (int i = 0; i < 216; i++)
        {
            byte r = (byte)(i / 36);
            byte g = (byte)(i / 6 % 6);
            byte b = (byte)(i % 6);
            palette[i] = new Pixel((byte)(r * 0x33), (byte)(g * 0x33), (byte)(b * 0x33));
        }
    }

    public byte[] Screen { get; } = new byte[256 * 256 * 3];
    public Keys KeysState { get; set; } = 0x0;
    public sbyte[] AudioSamples { get; } = new sbyte[256];

    private readonly byte[] memory = new byte[16 * 1024 * 1024 + 8];

    public void Load(string path)
    {
        var data = File.ReadAllBytes(path);
        Array.Clear(memory);
        Array.Copy(data, memory, data.Length);
    }

    public void Clock()
    {
        memory[0] = (byte)((ushort)KeysState >> 8);
        memory[1] = (byte)KeysState;

        int Get3BytesValue(int address) => (memory[address] << 16) | (memory[address + 1] << 8) | (memory[address + 2]);

        int pc = Get3BytesValue(2);

        for (int i = 0; i < 65536; i++)
        {
            memory[Get3BytesValue(pc + 3)] = memory[Get3BytesValue(pc)];
            pc = Get3BytesValue(pc + 6);
        }

        int index = memory[5] << 16;
        int screenIndex = 0;
        for(int i = 0; i < 256 * 256; i++)
        {
            var p = palette[memory[index + i]];
            Screen[screenIndex++] = p.Red;
            Screen[screenIndex++] = p.Green;
            Screen[screenIndex++] = p.Blue;
        }

        int audioIndex = (memory[6] << 16) | (memory[7] << 8);
        for (int i = 0; i < 256; i++)
        {
            AudioSamples[i] = (sbyte)memory[audioIndex + i];
        }
    }
}

public struct Pixel
{
    public byte Red;
    public byte Green;
    public byte Blue;

    public Pixel(byte red, byte green, byte blue)
    {
        Red = red;
        Green = green;
        Blue = blue;
    }
}

[Flags]
public enum Keys : ushort
{
    Key0 = 1 << 0x0,
    Key1 = 1 << 0x1,
    Key2 = 1 << 0x2,
    Key3 = 1 << 0x3,
    Key4 = 1 << 0x4,
    Key5 = 1 << 0x5,
    Key6 = 1 << 0x6,
    Key7 = 1 << 0x7,
    Key8 = 1 << 0x8,
    Key9 = 1 << 0x9,
    A = 1 << 0xA,
    B = 1 << 0xB,
    C = 1 << 0xC,
    D = 1 << 0xD,
    E = 1 << 0xE,
    F = 1 << 0xF
}
