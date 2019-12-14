/**
 * == Inspoy Technology ==
 * Assembly: TestRunner
 * FileName: Program.cs
 * Created on 2019/12/15 by inspo
 * All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            foreach (var item in Directory.GetFiles(rawFolder, "*", SearchOption.AllDirectories))
            {
                var reletivePath = item.Replace(rawFolder, "").Replace("\\", "/").TrimStart('/');
                fileList.Add(reletivePath);
            }
            try
            {
                var sw = new Stopwatch();
                Console.WriteLine("=====Pack=====");
                sw.Restart();
                FilePacker.PackToFile(rawFolder, fileList, ipkPath, "Secret!!!", true);
                Console.WriteLine($"Packing cost: {sw.ElapsedMilliseconds / 1000f:F2} s");
                Console.WriteLine($"Packed {fileList.Count} files");
                Console.WriteLine("=====END======");
                Console.WriteLine("====Unpack====");
                sw.Restart();
                var count = FilePacker.UnpackFile(ipkPath, outputFolder, "Secret!!!");
                Console.WriteLine($"Unpacking cost: {sw.ElapsedMilliseconds / 1000f:F2} s");
                Console.WriteLine($"Unpacked {count} files");
                Console.WriteLine("=====END======");
                Console.WriteLine("==ReadSingle==");
                sw.Restart();
                var content = FilePacker.ReadFileContent(ipkPath, "test.txt", "Secret!!!");
                File.WriteAllBytes(testFolder + "/test.txt", content);
                Console.WriteLine($"Unpacking single cost: {sw.ElapsedMilliseconds / 1000f:F2} s");
                Console.WriteLine("=====END======");
            }
            catch (Exception e)
            {
                Console.WriteLine("RunTest Error: " + e);
            }
        }
    }
}
