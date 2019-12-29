/**
 * == Inspoy Technology ==
 * Assembly: Instech.FilePacker
 * FileName: FilePacker.cs
 * Created on 2019/12/15 by inspo
 * All rights reserved.
 */

using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Instech.FilePacker
{
    public static class FilePacker
    {
        public static void PackToFile(string basePath, List<string> filePaths, string targetPath, string key = null, bool compress = true)
        {
            var packTool = new PackTool(targetPath);
            packTool.Create(key, compress);
            foreach (var fileName in filePaths)
            {
                var fullPath = Path.Combine(basePath, fileName);
                var content = File.ReadAllBytes(fullPath);
                packTool.AddContent(fileName, content);
            }
            packTool.Save();
        }

        public static int UnpackFile(string filePath, string targetFolder, string key = null)
        {
            return UnpackTool.UnpackAll(filePath, targetFolder, key);
        }

        public static byte[] ReadFileContent(string filePath, string fileName, string key = null)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File does not exist", filePath);
            }
            return UnpackTool.UnpackSingle(filePath, fileName, key);
        }

        public static async Task<byte[]> ReadFileContentAsync(string filePath, string fileName, string key = null)
        {
            var content = await Task.Run(() => ReadFileContent(filePath, fileName, key));
            return content;
        }
    }
}
