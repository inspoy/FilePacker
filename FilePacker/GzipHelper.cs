/**
 * == Inspoy Technology ==
 * Assembly: Instech.FilePacker
 * FileName: GzipHelper.cs
 * Created on 2019/12/15 by inspo
 * All rights reserved.
 */

using System.IO;
using System.IO.Compression;

namespace Instech.FilePacker
{
    internal static class GzipHelper
    {
        public static byte[] Compress(byte[] src)
        {
            byte[] buffer;
            using (var ms = new MemoryStream())
            {
                using (var gs = new GZipStream(ms, CompressionMode.Compress, true))
                {
                    gs.Write(src, 0, src.Length);
                }
                buffer = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(buffer, 0, buffer.Length);
            }
            return buffer;
        }
        public static byte[] Decompress(byte[] src)
        {
            byte[] buffer;
            using (var ms = new MemoryStream(src))
            {
                using (var gs = new GZipStream(ms, CompressionMode.Decompress, true))
                {
                    MemoryStream msreader = new MemoryStream();
                    buffer = new byte[0x1000];
                    while (true)
                    {
                        int reader = gs.Read(buffer, 0, buffer.Length);
                        if (reader <= 0)
                        {
                            break;
                        }
                        msreader.Write(buffer, 0, reader);
                    }

                    msreader.Position = 0;
                    buffer = msreader.ToArray();
                    msreader.Close();
                }
            }
            return buffer;
        }
    }
}