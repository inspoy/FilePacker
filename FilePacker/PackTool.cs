using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Instech.CryptHelper;
using K4os.Compression.LZ4;

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
        private FileStream _fs;
        private BinaryWriter _bw;
        private bool _compress;
        public PackTool(string ipkPath)
        {
            _ipkPath = ipkPath;
        }

        public void Create(string key, bool compress)
        {
            File.WriteAllBytes(_ipkPath, new byte[8 + 1]);
            _currenctPosition = 0;
            _fileList = new Dictionary<string, FileItemMeta>();
            _key = key;
            _compress = compress;
            if (!string.IsNullOrEmpty(_key))
            {
                _rc4 = new Rc4();
            }
            _fs = new FileStream(_ipkPath, FileMode.Append, FileAccess.Write);
            _bw = new BinaryWriter(_fs);
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
            if (_compress)
            {
                // 先压缩再加密
                content = LZ4Codec.Pickle(content);
            }
            if (_rc4 != null)
            {
                // 使用RC4算法加密
                _rc4.SetKeyAndInit(GetCryptKey(fileKey, _key));
                content = _rc4.Encrypt(content);
            }
            _bw.Write(content);
            _fileList.Add(fileKey, new FileItemMeta
            {
                Offset = _currenctPosition,
                Length = (ulong)content.Length
            });
            _currenctPosition += (ulong)content.Length;
        }

        public void Save()
        {
            // 文件结构
            // 8字节 - 文件内容总长度
            // 1字节 - 是否压缩
            // N字节 - 文件内容
            // 4字节 - 文件个数
            // M字节 - 元数据
            _bw.Close();
            _bw.Dispose();
            _fs.Close();
            _fs.Dispose();
            using (var fs = new FileStream(_ipkPath, FileMode.Open, FileAccess.Write))
            {
                using (var bw = new BinaryWriter(fs))
                {
                    bw.Write(_currenctPosition);
                    bw.Write((byte)(_compress ? 1 : 0));
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