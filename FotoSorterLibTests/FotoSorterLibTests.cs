using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace FotoSorterLib.Tests
{
    [TestClass()]
    public class FotoSorterLibTests
    {
        [TestMethod()]
        public void GetPhotoDateTest_Get_correct_date()
        {
            string filename = "..\\..\\TestPhotos\\NormalDate-2007.10.09.jpg";
            DateTime expected = new DateTime(2007, 10, 9, 9, 11, 7);

            DateTime actual = FotoSorterLib.GetPhotoDate(filename);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetPhotoDateTest_file_without_exif_data_returns_minDate()
        {
            string filename = "..\\..\\TestPhotos\\NoDate.jpg";
            DateTime expected = DateTime.MinValue;

            DateTime actual = FotoSorterLib.GetPhotoDate(filename);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetPhotoDateTest_file_without_date_returns_minDate()
        {
            string filename = "..\\..\\TestPhotos\\zeroDate.jpg";
            DateTime expected = DateTime.MinValue;

            DateTime actual = FotoSorterLib.GetPhotoDate(filename);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void PrepareFilesTest_selects_the_three_files()
        {
            string folder = "..\\..\\TestPhotos\\";
            var expected = 3;
            var actual = FotoSorterLib.PrepareFiles(folder);
            Assert.AreEqual(expected, actual.Count);

        }

        [TestMethod()]
        public void GetTimeFolderTest_nulldate_gets_unkonwn()
        {
            var expected = "desconhecido";
            var actual = FotoSorterLib.GetTimeFolder(null);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetTimeFolderTest_date_gets_year_month()
        {
            var expected = "2017\\10";
            var date = new DateTime(2017, 10, 13, 3, 57, 32, 11);
            var actual = FotoSorterLib.GetTimeFolder(date);
            Assert.AreEqual(expected, actual);
        }


        [TestMethod()]
        public void GetTimeFolderTest_month_is_0_padded()
        {
            var expected = "2017\\01";
            var date = new DateTime(2017, 1, 13, 3, 57, 32, 11);
            var actual = FotoSorterLib.GetTimeFolder(date);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void FilesAreEqualTest_same_file_returns_true()
        {
            string file1 = "..\\..\\TestFiles\\File1.bmp";
            Assert.IsTrue(FotoSorterLib.FilesAreEqual(file1, file1));
        }

        [TestMethod()]
        public void FilesAreEqualTest_different_files_returns_false()
        {
            string file1 = "..\\..\\TestFiles\\File1.bmp";
            string file2 = "..\\..\\TestFiles\\File2.bmp";
            Assert.IsFalse(FotoSorterLib.FilesAreEqual(file1, file2));
        }

        [TestMethod()]
        public void FilesAreEqualTest_equal_files_returns_true()
        {
            string file1 = "..\\..\\TestFiles\\File1.bmp";
            string file2 = "..\\..\\TestFiles\\CopyFile1.bmp";
            Assert.IsTrue(FotoSorterLib.FilesAreEqual(file1, file2));
        }

        [TestMethod()]
        public void GetPathTest_return_Path_if_file_is_not_present()
        {
            string expected = System.IO.Path.GetTempPath();
            expected = expected.Remove(expected.LastIndexOf(System.IO.Path.DirectorySeparatorChar), 1);
            string actual = FotoSorterLib.GetPath(expected);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetPathTest_return_Path_if_file_is_present()
        {
            string filename = System.IO.Path.GetTempFileName();
            string expected = System.IO.Path.GetTempPath();
            expected = expected.Remove(expected.LastIndexOf(System.IO.Path.DirectorySeparatorChar), 1);
            string actual = FotoSorterLib.GetPath(filename);
            Assert.AreEqual(expected, actual);
        }

    }
}