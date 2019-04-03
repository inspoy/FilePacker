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
            var fileList = LoadIpm(filePath);
            Rc4 rc4 = null;
            if (!string.IsNullOrEmpty(key))
            {
                rc4 = new Rc4();
            }
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                foreach (var item in fileList)
                {
                    fs.Seek((long)item.Value.Offset + 8, SeekOrigin.Begin);
                    var content = new byte[item.Value.Length];
                    fs.Read(content, 0, (int)item.Value.Length);
                    var itemPath = Path.Combine(targetFolder, item.Key);
                    var itemFolder = Path.GetDirectoryName(itemPath);
                    if (!string.IsNullOrEmpty(itemFolder) && !Directory.Exists(itemFolder))
                    {
                        Directory.CreateDirectory(itemFolder);
                    }
                    Console.WriteLine("Outputting: " + itemPath);
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

        public static byte[] UnpackSingle(string filePath, string fileName, string key = null)
        {

            var fileList = LoadIpm(filePath);
            if (!fileList.ContainsKey(fileName))
            {
                throw new KeyNotFoundException($"{fileName} does not exist in {filePath}");
            }
            var ipmItem = fileList[fileName];
            using (var fs = new FileStream(filePath, FileMode.Open))
            {
                fs.Seek((long)ipmItem.Offset + 8, SeekOrigin.Begin);
                var content = new byte[ipmItem.Length];
                fs.Read(content, 0, (int)ipmItem.Length);
                if (!string.IsNullOrEmpty(key))
                {
                    var rc4 = new Rc4();
                    rc4.SetKeyAndInit(PackTool.GetCryptKey(fileName, key));
                    content = rc4.Encrypt(content);
                }
                return content;
            }
        }

        public static Dictionary<string, FileItemMeta> LoadIpm(string ipkPath)
        {
            if (!File.Exists(ipkPath))
            {
                throw new InvalidOperationException("Ipk file does not exist.");
            }
            var fileList = new Dictionary<string, FileItemMeta>();
            using (var fs = new FileStream(ipkPath, FileMode.Open, FileAccess.Read))
            {
                var length = 0LU;
                using (var br = new BinaryReader(fs, Encoding.UTF8, true))
                {
                    length = br.ReadUInt64();
                }
                fs.Seek((long)length + 8, SeekOrigin.Begin);
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
                item = new FileItemMeta
                {
                    Offset = br.ReadUInt64(),
                    Length = br.ReadUInt64()
                };
            }
            catch (EndOfStreamException e)
            {
                throw new BadImageFormatException("Reading ipm failed.", e);
            }
        }
    }
}