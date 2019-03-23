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
            foreach(var fileName in filePaths){
                var fullPath = Path.Combine(basePath,fileName);
            }
        }

        public static void UnpackFile(string filePath, string targetFolder)
        {
            throw new NotImplementedException();
        }

        public static byte[] ReadFileContent(string filePath, string fileName)
        {
            throw new NotImplementedException();
        }
    }
}
