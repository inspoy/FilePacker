/**
 * == Inspoy Technology ==
 * Assembly: Instech.FilePacker
 * FileName: UnpackTool.cs
 * Created on 2019/12/15 by inspo
 * All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Instech.CryptHelper;

namespace Instech.FilePacker
{
    internal class UnpackTool
    {
        public static int UnpackAll(string filePath, string targetFolder, string key = null)
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
            var fileList = LoadIpm(filePath, out var compress);
            Rc4 rc4 = null;
            if (!string.IsNullOrEmpty(key))
            {
                rc4 = new Rc4();
            }
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                foreach (var item in fileList)
                {
                    fs.Seek((long)item.Value.Offset + 8 + 1, SeekOrigin.Begin);
                    var content = new byte[item.Value.Length];
                    fs.Read(content, 0, (int)item.Value.Length);
                    var itemPath = Path.Combine(targetFolder, item.Key);
                    var itemFolder = Path.GetDirectoryName(itemPath);
                    if (!string.IsNullOrEmpty(itemFolder) && !Directory.Exists(itemFolder))
                    {
                        Directory.CreateDirectory(itemFolder);
                    }
                    if (rc4 != null)
                    {
                        var cryptKey = PackTool.GetCryptKey(item.Key, key);
                        rc4.SetKeyAndInit(cryptKey);
                        content = rc4.Encrypt(content);
                    }
                    if (compress)
                    {
                        // 先解密再解压
                        content = K4os.Compression.LZ4.LZ4Codec.Unpickle(content);
                    }
                    File.WriteAllBytes(itemPath, content);
                }
            }
            return fileList.Count;
        }

        public static byte[] UnpackSingle(string filePath, string fileName, string key = null)
        {

            var fileList = LoadIpm(filePath, out var compress);
            if (!fileList.ContainsKey(fileName))
            {
                throw new KeyNotFoundException($"{fileName} does not exist in {filePath}");
            }
            var ipmItem = fileList[fileName];
            using (var fs = new FileStream(filePath, FileMode.Open))
            {
                fs.Seek((long)ipmItem.Offset + 8 + 1, SeekOrigin.Begin);
                var content = new byte[ipmItem.Length];
                fs.Read(content, 0, (int)ipmItem.Length);
                if (!string.IsNullOrEmpty(key))
                {
                    var rc4 = new Rc4();
                    rc4.SetKeyAndInit(PackTool.GetCryptKey(fileName, key));
                    content = rc4.Encrypt(content);
                }
                if (compress)
                {
                    // 先解密再解压
                    content = K4os.Compression.LZ4.LZ4Codec.Unpickle(content);
                }
                return content;
            }
        }

        private static Dictionary<string, (bool, Dictionary<string, FileItemMeta>)> _cachedIpm = new Dictionary<string, (bool, Dictionary<string, FileItemMeta>)>();

        public static Dictionary<string, FileItemMeta> LoadIpm(string ipkPath, out bool compress)
        {
            if (_cachedIpm.TryGetValue(ipkPath, out var cachedItem))
            {
                compress = cachedItem.Item1;
                return cachedItem.Item2;
            }

            compress = false;
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
                    compress = br.ReadByte() == 1;
                }
                fs.Seek((long)length + 8 + 1, SeekOrigin.Begin);
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