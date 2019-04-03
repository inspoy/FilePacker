using System.IO;
using System.Collections.Generic;

namespace Instech.FilePacker
{
    public static class FilePacker
    {
        public static void PackToFile(string basePath, List<string> filePaths, string targetPath, string key = null)
        {
            var packTool = new PackTool(targetPath);
            packTool.Create(key);
            foreach (var fileName in filePaths)
            {
                var fullPath = Path.Combine(basePath, fileName);
                var content = File.ReadAllBytes(fullPath);
                packTool.AddContent(fileName, content);
            }
            packTool.Save();
        }

        public static void UnpackFile(string filePath, string targetFolder, string key = null)
        {
            UnpackTool.UnpackAll(filePath, targetFolder, key);
        }

        public static byte[] ReadFileContent(string filePath, string fileName, string key = null)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File does not exist", filePath);
            }
            return UnpackTool.UnpackSingle(filePath, fileName, key);
        }
    }
}
