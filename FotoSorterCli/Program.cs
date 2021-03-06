﻿using MetadataExtractor;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FotoSorterCli
{
    class Program
    {

        public static DateTime GetPhotoDate(string filename)
        {
            // open image
            IEnumerable<Directory> directories = ImageMetadataReader.ReadMetadata(filename);
            foreach (var directory in directories)
                foreach (var tag in directory.Tags)
                    Trace.WriteLine($"{directory.Name} - {tag.Name} = {tag.Description}");

            return DateTime.MinValue;
        }


        static void Main(string[] args)
        {
            string filename = "C:\\Users\\pgp\\Documents\\Visual Studio 2017\\Projects\\FotoSorter\\FotoSorterLibTests\\TestPhotos\\NormalDate-2007.10.09.jpg";
            GetPhotoDate(filename);
        }
    }
}
