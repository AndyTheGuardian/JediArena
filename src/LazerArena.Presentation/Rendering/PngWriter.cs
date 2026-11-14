using System.Buffers.Binary;
using System.IO.Compression;

namespace LazerArena.Presentation.Rendering;

/// <summary>Minimaler PNG-Schreiber (RGB, ohne Filter), fuer Screenshots und Tests ohne Grafikbibliothek.</summary>
public static class PngWriter
{
    public static void Write(PixelCanvas canvas, string path, int scale = 1)
    {
        using var file = File.Create(path);
        Write(canvas, file, scale);
    }

    public static void Write(PixelCanvas canvas, Stream output, int scale = 1)
    {
        int w = canvas.Width * scale, h = canvas.Height * scale;
        output.Write([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);

        var header = new byte[13];
        BinaryPrimitives.WriteInt32BigEndian(header, w);
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), h);
        header[8] = 8;   // Bit je Kanal
        header[9] = 2;   // RGB
        WriteChunk(output, "IHDR", header);

        using var raw = new MemoryStream();
        using (var z = new ZLibStream(raw, CompressionLevel.Fastest, leaveOpen: true))
        {
            var row = new byte[1 + w * 3];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var p = canvas[x / scale, y / scale];
                    row[1 + x * 3] = (byte)Math.Clamp(p.R, 0, 255);
                    row[2 + x * 3] = (byte)Math.Clamp(p.G, 0, 255);
                    row[3 + x * 3] = (byte)Math.Clamp(p.B, 0, 255);
                }
                z.Write(row);
            }
        }
        WriteChunk(output, "IDAT", raw.ToArray());
        WriteChunk(output, "IEND", []);
    }

    private static void WriteChunk(Stream output, string type, byte[] data)
    {
        Span<byte> buffer = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(buffer, data.Length);
        output.Write(buffer);
        var typeBytes = System.Text.Encoding.ASCII.GetBytes(type);
        output.Write(typeBytes);
        output.Write(data);
        BinaryPrimitives.WriteUInt32BigEndian(buffer, Crc32(typeBytes, data));
        output.Write(buffer);
    }

    private static uint Crc32(byte[] type, byte[] data)
    {
        uint crc = 0xFFFFFFFF;
        foreach (var b in type.Concat(data))
        {
            crc ^= b;
            for (int k = 0; k < 8; k++) crc = (crc & 1) != 0 ? 0xEDB88320 ^ (crc >> 1) : crc >> 1;
        }
        return ~crc;
    }
}
