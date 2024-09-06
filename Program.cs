using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

class BinaryReaderBE : BinaryReader
{
    public BinaryReaderBE(System.IO.Stream stream) : base(stream) {}

    public override Int16 ReadInt16()
    {
        var data = base.ReadBytes(2);
        Array.Reverse(data);
        return BitConverter.ToInt16(data, 0);
    }
    public override Int32 ReadInt32()
    {
        var data = base.ReadBytes(4);
        Array.Reverse(data);
        return BitConverter.ToInt32(data, 0);
    }

    public override Int64 ReadInt64()
    {
        var data = base.ReadBytes(8);
        Array.Reverse(data);
        return BitConverter.ToInt64(data, 0);
    }

    public override UInt16 ReadUInt16()
    {
        var data = base.ReadBytes(2);
        Array.Reverse(data);
        return BitConverter.ToUInt16(data, 0);
    }

    public override UInt32 ReadUInt32()
    {
        var data = base.ReadBytes(4);
        Array.Reverse(data);
        return BitConverter.ToUInt32(data, 0);
    }

    public override UInt64 ReadUInt64()
    {
        var data = base.ReadBytes(8);
        Array.Reverse(data);
        return BitConverter.ToUInt64(data, 0);
    }

}

namespace World_of_Zoo_Extractor
{
    class Program
    {
        static BinaryReader br;

        static void Main(string[] args)
        {
            FileStream fin = File.OpenRead(args[0]); //come and see him

            br = new BinaryReader(fin);

            if (br.ReadUInt32() == 0)
            {
                Console.WriteLine($"{args[0]}: little endian");
            } else
            {
                Console.WriteLine($"{args[0]}: big endian");
                br = new BinaryReaderBE(fin);
            }

            br.ReadUInt32();
            uint groupCount = br.ReadUInt32();
            uint fileCount = br.ReadUInt32();

            Console.WriteLine($"groups: {groupCount}, files: {fileCount}");

            List<PkgGroup> groups= new();
            for (int i = 0; i < groupCount; i++)
            {
                groups.Add(new());
            }

            List<PkgFile> files = new();
            for (int i = 0; i < fileCount; i++)
            {
                files.Add(new());
            }

            long fileTable = br.BaseStream.Position;
            Console.WriteLine(fileTable);

            foreach (PkgFile file in files)
            {
                br.BaseStream.Position = fileTable + file.start;
                string name = new string(file.name);
                name = name.Substring(0, name.IndexOf('\0'));

                Console.WriteLine($"{name}: isCompressed={file.isCompressed}, compressedSize={file.compressedSize}, uncompressedSize={file.uncompressedSize}");

                Directory.CreateDirectory(Path.GetDirectoryName(name));

                MemoryStream ms = new();
                if (file.isCompressed == 1)
                {
                    br.ReadInt16(); //skip zlib header
                    DeflateStream ds = new DeflateStream(new MemoryStream(br.ReadBytes((int)file.compressedSize)), CompressionMode.Decompress);
                    ds.CopyTo(ms);
                    ds.Close();

                }
                else if (file.isCompressed == 0)
                    ms.Write(br.ReadBytes((int)file.uncompressedSize));
                else
                    throw new System.Exception("Fuck!");

                FileStream fout = File.OpenWrite(name);
                ms.Position = 0;
                ms.CopyTo(fout);

                fout.Close();
                ms.Close();
            }
        }

        class PkgGroup
        {
            public UInt32 number = br.ReadUInt32();
            public char[] name = br.ReadChars(32);
            public UInt32 unknown1 = br.ReadUInt32();
        }

        class PkgFile
        {
            public UInt32 number = br.ReadUInt32();
            public char[] name = br.ReadChars(256);
            public UInt32 start = br.ReadUInt32();
            public UInt32 compressedSize = br.ReadUInt32();
            public UInt32 uncompressedSize = br.ReadUInt32();
            public UInt64 unknown1 = br.ReadUInt64(); //maybe hash
            public UInt32 isCompressed = br.ReadUInt32();
            public UInt32 group = br.ReadUInt32();
        }
    }
}
