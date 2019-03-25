using System;
using System.IO;
using System.Text;

namespace Instech.FilePacker
{
    internal class UnpackTool
    {
        public static void UnpackAll(string filePath, string targetFolder)
        {
            if (!File.Exists(filePath))
            {
                throw new InvalidOperationException("Source file does not exist.");
            }
            if (Directory.Exists(targetFolder))
            {
                throw new InvalidOperationException("Target folder exists.");
            }
            Directory.CreateDirectory(targetFolder);
            var ipmPath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + ".ipm");
            if (!File.Exists(ipmPath))
            {
                throw new InvalidOperationException("Ipm file does not exist.");
            }
            using (var fs = new FileStream(ipmPath, FileMode.Open))
            {
                using (var br = new BinaryReader(fs))
                {
                    var count = br.ReadInt32();
                }
            }
        }
    }
}