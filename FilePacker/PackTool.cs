using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Instech.CryptHelper;

namespace Instech.FilePacker
{
    internal struct FileItemMeta
    {
        public ulong Offset;
        public ulong Length;
    }

    internal class PackTool
    {
        private string _ipkPath;
        private uint _currenctPosition;
        private string _key;
        private Dictionary<string, FileItemMeta> _fileList;
        public PackTool(string ipkPath)
        {
            _ipkPath = ipkPath;
        }

        public void Create(string key)
        {
            File.WriteAllBytes(_ipkPath, Array.Empty<byte>());
            _currenctPosition = 0;
            _fileList = new Dictionary<string, FileItemMeta>();
            _key = key;
        }

        public void AddContent(string key, byte[] content)
        {
            if (!string.IsNullOrEmpty(_key))
            {
                // 使用RC4算法加密
                var rc4 = new Rc4();
                rc4.SetKeyAndInit(_key);
            }
            using (var fs = new FileStream(_ipkPath, FileMode.Append, FileAccess.Write))
            {
                using (var bw = new BinaryWriter(fs))
                {
                    bw.Write(content);
                }
            }
            _fileList.Add(key, new FileItemMeta
            {
                Offset = _currenctPosition,
                Length = (ulong)content.Length
            });
            _currenctPosition += (uint)content.Length;
        }

        public void Save()
        {
            var dirName = Path.GetDirectoryName(_ipkPath);
            var ipkName = Path.GetFileNameWithoutExtension(_ipkPath);
            var ipmPath = Path.Combine(dirName, ipkName + ".ipm");
            using (var fs = new FileStream(ipmPath, FileMode.Create, FileAccess.Write))
            {
                using (var bw = new BinaryWriter(fs))
                {
                    bw.Write(_fileList.Count);
                    foreach (var item in _fileList)
                    {
                        bw.Write(item.Key);
                        bw.Write(item.Value.Offset);
                        bw.Write(item.Value.Length);
                    }
                }
            }
        }
    }
}