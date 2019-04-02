using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Instech.CryptHelper;

namespace Instech.FilePacker
{
    internal class UnpackTool
    {
        public static void UnpackAll(string filePath, string targetFolder, string key = null)
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
            var fileList = LoadIpm(ipmPath);
            Rc4 rc4 = null;
            if (!string.IsNullOrEmpty(key))
            {
                rc4 = new Rc4();
            }
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                foreach (var item in fileList)
                {
                    fs.Seek((long)item.Value.Offset, SeekOrigin.Begin);
                    var content = new byte[item.Value.Length];
                    fs.Read(content, 0, (int)item.Value.Length);
                    var itemPath = Path.Combine(targetFolder, item.Key);
                    var itemFolder = Path.GetDirectoryName(itemPath);
                    if (!Directory.Exists(itemFolder))
                    {
                        Directory.CreateDirectory(itemFolder);
                    }
                    Console.WriteLine("Outputing: " + itemPath);
                    if (rc4 != null)
                    {
                        var cryptKey = PackTool.GetCryptKey(item.Key, key);
                        rc4.SetKeyAndInit(cryptKey);
                        content = rc4.Encrypt(content);
                    }
                    File.WriteAllBytes(itemPath, content);
                }
            }
        }

        public static Dictionary<string, FileItemMeta> LoadIpm(string ipmPath)
        {
            if (!File.Exists(ipmPath))
            {
                throw new InvalidOperationException("Ipm file does not exist.");
            }
            var fileList = new Dictionary<string, FileItemMeta>();
            using (var fs = new FileStream(ipmPath, FileMode.Open, FileAccess.Read))
            {
                using (var br = new BinaryReader(fs))
                {
                    var count = br.ReadInt32();
                    for (var i = 0; i < count; ++i)
                    {
                        ReadIpmItem(br, out var key, out var item);
                        fileList.Add(key, item);
                    }
                }
            }
            return fileList;
        }

        private static void ReadIpmItem(BinaryReader br, out string key, out FileItemMeta item)
        {
            try
            {
                key = br.ReadString();
                item = new FileItemMeta();
                item.Offset = br.ReadUInt64();
                item.Length = br.ReadUInt64();
            }
            catch (EndOfStreamException e)
            {
                throw new BadImageFormatException("Reading ipm file failed.", e);
            }
        }
    }
}