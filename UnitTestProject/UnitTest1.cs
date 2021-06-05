using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PdfWaterMark;

// 单元测试
namespace UnitTestProject
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            PdfFileProcess pdf = new PdfFileProcess();
            pdf.SourcePath = @"Y:\IT";
            Console.WriteLine(pdf.SourcePath);
            foreach (var item in pdf.SourceFilePathList)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine("-----------only file---------------------------");
            pdf.TargetPath = @"F:\1";
            pdf.IsSingleFile = true;
            foreach(var item in pdf.TargetFilePathList)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine("-----------folder---------------------------");
            pdf.IsSingleFile = false;
            foreach (var item in pdf.TargetFilePathList)
            {
                Console.WriteLine(item);
            }
        }
    }
}
