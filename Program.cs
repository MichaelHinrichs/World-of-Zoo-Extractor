using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace World_of_Zoo_Extractor
{
    class Program
    {
        static BinaryReader br;
        static void Main(string[] args)
        {
            br = new(File.OpenRead(args[0]));
            br.ReadInt64();
            int block1Size = br.ReadInt32();
            int block2Size = br.ReadInt32();

            List<Block1> block1 = new();
            for (int i = 0; i < block1Size; i++)
            {
                block1.Add(new());
            }

            List<Block2> block2 = new();
            for (int i = 0; i < block2Size; i++)
            {
                block2.Add(new());
            }

            int fileTable = (int)br.BaseStream.Position;

            foreach (Block2 file in block2)
            {
                br.BaseStream.Position = file.start + fileTable;
                string firstName = file.name.Substring(0, file.name.LastIndexOf('\0'));
                string secondName = file.name.Substring(firstName.Length + 1);
                Directory.CreateDirectory(Path.GetDirectoryName(args[0]) + "\\" + Path.GetDirectoryName(firstName));

                MemoryStream ms = new();
                if (file.isCompressed == 1)
                {
                    br.ReadInt16();
                    using var ds = new DeflateStream(new MemoryStream(br.ReadBytes(file.size)), CompressionMode.Decompress);
                    ds.CopyTo(ms);
                    ds.Close();
                }
                else if (file.isCompressed == 0)
                    ms.Write(br.ReadBytes(file.size));
                else
                    throw new System.Exception("Fuck!");

                BinaryReader msr = new(ms);
                msr.BaseStream.Position = 0;

                BinaryWriter bw = new BinaryWriter(File.OpenWrite(Path.GetDirectoryName(args[0]) + "\\" + firstName));
                bw.Write(msr.ReadBytes(file.size));
                msr.Close();
                bw.Close();
            }
        }

        class Block1
        {
            int number = br.ReadInt32();
            string name = new (new string (br.ReadChars(32)).TrimEnd((char)0x00));
            int unknown = br.ReadInt32();
        }

        class Block2
        {
            int number = br.ReadInt32();
            public string name = new(new string(br.ReadChars(256)).TrimEnd((char)0x00));
            public int start = br.ReadInt32();
            public int size = br.ReadInt32();
            int sizeMinus0x0BForSomeReason = br.ReadInt32();
            long unknown = br.ReadInt64();
            public int isCompressed = br.ReadInt32();
            int block1 = br.ReadInt32();
        }
    }
}
