using System;
using System.IO;
using System.Collections.Generic;

namespace Instech.FilePacker
{
    public static class FilePacker
    {
        public static void PackToFile(string basePath, IEnumerable<string> filePaths, string targetPath)
        {
            var packTool = new PackTool(targetPath);
            packTool.Create();
            foreach (var fileName in filePaths)
            {
                var fullPath = Path.Combine(basePath, fileName);
                var content = File.ReadAllBytes(fullPath);
                packTool.AddContent(fileName, content);
            }
            packTool.Save();
        }

        public static void UnpackFile(string filePath, string targetFolder)
        {
            UnpackTool.UnpackAll(filePath, targetFolder);
        }

        public static byte[] ReadFileContent(string filePath, string fileName)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File does not exist", filePath);
            }
            var ipmPath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + ".ipm");
            var fileList = UnpackTool.LoadIpm(ipmPath);
            if (!fileList.ContainsKey(fileName))
            {
                throw new KeyNotFoundException($"{fileName} does not exist in {filePath}");
            }
            var ipmItem = fileList[fileName];
            using (var fs = new FileStream(filePath, FileMode.Open))
            {
                fs.Seek((long)ipmItem.Offset, SeekOrigin.Begin);
                var content = new byte[ipmItem.Length];
                fs.Read(content, 0, (int)ipmItem.Length);
                return content;
            }
        }
    }
}
