using FotoSorterLib;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace FotoSorterLib.Tests
{
    [TestClass()]
    public class UnitTest1
    {
        [TestMethod()]
        public void GetPhotoDateTest_Get_correct_date()
        {
            string filename = "..\\..\\TestPhotos\\NormalDate-2007.10.09.jpg";
            DateTime expected = new DateTime(2007, 10, 9, 9, 11, 7);

            DateTime actual = FotoAnalyser.GetPhotoDate(filename);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetPhotoDateTest_file_without_exif_data_returns_minDate()
        {
            string filename = "..\\..\\TestPhotos\\NoDate.jpg";
            DateTime expected = DateTime.MinValue;

            DateTime actual = FotoAnalyser.GetPhotoDate(filename);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetPhotoDateTest_file_without_date_returns_minDate()
        {
            string filename = "..\\..\\TestPhotos\\zeroDate.jpg";
            DateTime expected = DateTime.MinValue;

            DateTime actual = FotoAnalyser.GetPhotoDate(filename);
            Assert.AreEqual(expected, actual);
        }

    }
}


