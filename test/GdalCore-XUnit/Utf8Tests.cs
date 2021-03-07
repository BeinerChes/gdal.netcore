using System;
using System.IO;
using System.Linq;
using MaxRev.Gdal.Core;
using OSGeo.GDAL;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace GdalCore_XUnit
{

    public class Utf8Tests
    {
        private string dataDirectoryPath;
        private string inputEngDirectoryPath;
        private string inputCyrDirectoryPath;
        private string englishInputFilePath;
        private string cyrillicInputFilePath;

        public Utf8Tests()
        {
            GdalBase.ConfigureAll();

            dataDirectoryPath = Extensions.GetTestDataFolder("utf8-data");
            inputEngDirectoryPath = Path.Combine(dataDirectoryPath, "input-eng");
            inputCyrDirectoryPath = Path.Combine(dataDirectoryPath, "input-cyr");
            englishInputFilePath = Path.Combine(inputEngDirectoryPath, "input.tif");
            cyrillicInputFilePath = Path.Combine(inputCyrDirectoryPath, "тест.tif");
        }

        [Fact]
        public void Test1()
        {

            // Check the default state of "GDAL_FILENAME_IS_UTF8" config option.
            Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "YES");
            string currentState = Gdal.GetConfigOption("GDAL_FILENAME_IS_UTF8", "YES");

            #region Test 1

            // Test 1 - Passing. Paths doesn't contain any cyrillic symbols

            Console.WriteLine($"Test 1 - GDAL_FILENAME_IS_UTF8 is set to {currentState} by default");

            var outputFilePath = Path.Combine(dataDirectoryPath, "test1.vrt");
            string testResult = RunTest(englishInputFilePath, inputEngDirectoryPath, outputFilePath) ? "passed" : "failed";
            Assert.Equal("passed", testResult);

            #endregion 
        }

        [Fact(Skip = "not fixed yet")]
        public void Test2()
        {
            #region Test 2

            // Test 2 - Gdal.Open pass, BuildVrt fails (doesn't throw errors/exceptions, but no output file), writes "warning" in console

            string currentState = Gdal.GetConfigOption("GDAL_FILENAME_IS_UTF8", "YES");
            Console.WriteLine($"Test 2 - GDAL_FILENAME_IS_UTF8 is set to {currentState} before the test");

            var outputFilePath = Path.Combine(dataDirectoryPath, "test2.vrt");
            string testResult = RunTest(cyrillicInputFilePath, inputCyrDirectoryPath, outputFilePath) ? "passed" : "failed";
            Assert.Equal("passed", testResult);

            #endregion
        }

        [Fact]
        public void Test3()
        {
            // Change "GDAL_FILENAME_IS_UTF8" value to "NO" and check, if it was changed correctly.
            Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "NO");
            var currentState = Gdal.GetConfigOption("GDAL_FILENAME_IS_UTF8", "YES");

            #region Test 3

            // Test 3 - Gdal.Open pass, BuildVrt fails (doesn't throw errors/exceptions, but no output file), writes "warning" in console

            Console.WriteLine($"Test 3 - GDAL_FILENAME_IS_UTF8 is set to {currentState} before the test");

            var outputFilePath = Path.Combine(dataDirectoryPath, "test3.vrt");
            var testResult = RunTest(englishInputFilePath, inputEngDirectoryPath, outputFilePath) ? "passed" : "failed";
            Assert.Equal("passed", testResult);

            #endregion
        }

        [Fact(Skip = "not fixed yet")]
        public void Test4()
        {
            // Change "GDAL_FILENAME_IS_UTF8" value to "NO" and check, if it was changed correctly.
            Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "NO");
            var currentState = Gdal.GetConfigOption("GDAL_FILENAME_IS_UTF8", "YES");

            #region Test 4

            // Test 4 - all fails, Gdal.Open throws exception, BuildVrt writes "" warning

            Console.WriteLine($"Test 4 - GDAL_FILENAME_IS_UTF8 is set to {currentState} before the test");

            var outputFilePath = Path.Combine(dataDirectoryPath, "test4.vrt");
            var testResult = RunTest(cyrillicInputFilePath, inputCyrDirectoryPath, outputFilePath) ? "passed" : "failed";
            Assert.Equal("passed", testResult);

            #endregion
        }

        private static bool GdalBuildVrt(string[] inputFilesPaths, string outputFilePath, string[] options, Gdal.GDALProgressFuncDelegate callback)
        {
            try
            {
                using Dataset result = Gdal.wrapper_GDALBuildVRT_names(outputFilePath, inputFilesPaths, new GDALBuildVRTOptions(options), callback, null);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);

                return false;
            }

            return true;
        }

        private static bool OpenDataset(string inputFilePath)
        {
            try
            {
                using Dataset inputDataset = Gdal.Open(inputFilePath, Access.GA_ReadOnly);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);

                return false;
            }

            return true;
        }

        private static bool RunTest(string inputFilePath, string inputDirectoryPath, string outputFilePath)
        {
            bool isTestSuccessful = OpenDataset(inputFilePath);

            string[] inputFilesPaths = new DirectoryInfo(inputDirectoryPath)
                                      .EnumerateFiles().Select(fileInfo => fileInfo.FullName).ToArray();
            if (!GdalBuildVrt(inputFilesPaths, outputFilePath, null, null))
                isTestSuccessful = false;

            // Check if .vrt file was created, because GdalBuildVrt doesn't throw exceptions in that case
            if (!new FileInfo(outputFilePath).Exists)
                isTestSuccessful = false;

            return isTestSuccessful;
        }
    }
}