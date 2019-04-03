using System;
using System.Collections.Generic;
using System.IO;
using Instech.FilePacker;

namespace TestRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var cwd = Directory.GetCurrentDirectory();
            if (!cwd.Replace('\\', '/').Trim('/').EndsWith("FilePacker"))
            {
                cwd = cwd.Substring(0, cwd.IndexOf("TestRunner", StringComparison.Ordinal));
            }
            var testFolder = Path.Combine(cwd, "TestFolder");
            var outputFolder = testFolder + "/output";
            if (Directory.Exists(outputFolder))
            {
                Directory.Delete(testFolder + "/output", true);
            }
            var ipkPath = testFolder + "/output.ipk";
            Console.WriteLine("TestFolder: " + testFolder);
            var rawFolder = testFolder + "/raw";
            var fileList = new List<string>();
            var count = 0;
            foreach (var item in Directory.GetFiles(rawFolder, "*", SearchOption.AllDirectories))
            {
                var reletivePath = item.Replace(rawFolder, "").Replace("\\", "/").TrimStart('/');
                fileList.Add(reletivePath);
                count += 1;
            }
            Console.WriteLine($"File count: {count}");
            try
            {
                Console.WriteLine("=====Pack=====");
                FilePacker.PackToFile(rawFolder, fileList, ipkPath, "Secret!!!");
                Console.WriteLine("=====END======");
                Console.WriteLine("====Unpack====");
                FilePacker.UnpackFile(ipkPath, outputFolder, "Secret!!!");
                Console.WriteLine("=====END======");
                Console.WriteLine("==ReadSingle==");
                var content = FilePacker.ReadFileContent(ipkPath, "test.txt", "Secret!!!");
                File.WriteAllBytes(testFolder + "/test.txt", content);
                Console.WriteLine("=====END======");
            }
            catch (Exception e)
            {
                Console.WriteLine("RunTest Error: " + e);
            }
        }
    }
}
