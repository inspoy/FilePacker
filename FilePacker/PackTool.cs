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
        private readonly string _ipkPath;
        private ulong _currenctPosition;
        private string _key;
        private Rc4 _rc4;
        private Dictionary<string, FileItemMeta> _fileList;
        public PackTool(string ipkPath)
        {
            _ipkPath = ipkPath;
        }

        public void Create(string key)
        {
            File.WriteAllBytes(_ipkPath, new byte[8]);
            _currenctPosition = 0;
            _fileList = new Dictionary<string, FileItemMeta>();
            _key = key;
            if (!string.IsNullOrEmpty(_key))
            {
                _rc4 = new Rc4();
            }
        }

        internal static byte[] GetCryptKey(string fileKey, string ipkKey)
        {
            var b1 = Encoding.UTF8.GetBytes(fileKey);
            var b2 = Encoding.UTF8.GetBytes(ipkKey);
            var ret = new byte[Math.Max(b1.Length, b2.Length)];
            for (var i = 0; i < ret.Length; ++i)
            {
                if (i < b1.Length && i < b2.Length)
                {
                    ret[i] = (byte)(b1[i] ^ b2[i]);
                }
                else if (i < b1.Length)
                {
                    ret[i] = b1[i];
                }
                else if (i < b2.Length)
                {
                    ret[i] = b2[i];
                }
            }
            return ret;
        }

        public void AddContent(string fileKey, byte[] content)
        {
            if (_rc4 != null)
            {
                // 使用RC4算法加密
                _rc4.SetKeyAndInit(GetCryptKey(fileKey, _key));
                content = _rc4.Encrypt(content);
            }
            using (var fs = new FileStream(_ipkPath, FileMode.Append, FileAccess.Write))
            {
                using (var bw = new BinaryWriter(fs))
                {
                    bw.Write(content);
                }
            }
            _fileList.Add(fileKey, new FileItemMeta
            {
                Offset = _currenctPosition,
                Length = (ulong)content.Length
            });
            _currenctPosition += (ulong)content.Length;
        }

        public void Save()
        {
            using (var fs = new FileStream(_ipkPath, FileMode.Open, FileAccess.Write))
            {
                using (var bw = new BinaryWriter(fs))
                {
                    bw.Write(_currenctPosition);
                }
            }
            using (var fs = new FileStream(_ipkPath, FileMode.Append, FileAccess.Write))
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