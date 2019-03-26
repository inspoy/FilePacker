﻿using System;
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
            var testFolder = Path.Combine(Directory.GetCurrentDirectory(), "TestFolder");
            var ipkPath = testFolder + "/output.ipk";
            Console.WriteLine("TestFolder: " + testFolder);
            var rawFolder = testFolder + "/raw";
            var fileList = new List<string>();
            foreach (var item in Directory.GetFiles(rawFolder, "*", SearchOption.AllDirectories))
            {
                var reletivePath = item.Replace(rawFolder, "").Replace("\\", "/").TrimStart('/');
                Console.WriteLine(reletivePath);
                fileList.Add(reletivePath);
            }
            Console.WriteLine("=====Pack=====");
            FilePacker.PackToFile(rawFolder, fileList, ipkPath);
            Console.WriteLine("=====END======");
            Console.WriteLine("====Unpack====");
            FilePacker.UnpackFile(ipkPath, testFolder + "/output");
            Console.WriteLine("=====END======");
            Console.WriteLine("==ReadSingle==");
            var content = FilePacker.ReadFileContent(ipkPath, "/test.txt");
            File.WriteAllBytes(testFolder + "/test.txt", content);
            Console.WriteLine("=====END======");
        }
    }
}
