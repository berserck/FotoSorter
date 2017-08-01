using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System.Diagnostics;

namespace FotoSorterLib
{
    /// <summary>
    /// Class responsible for reading a foto
    /// </summary>
    public static class FotoAnalyser
    {
        static public DateTime GetPhotoDate(string filename)
        {
            var dateTime = DateTime.MinValue.ToString("yyyy:MM:dd HH:mm:ss");
            // open image
            IEnumerable <Directory> directories = ImageMetadataReader.ReadMetadata(filename);

            var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            if (subIfdDirectory != null)
            {
                dateTime = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagDateTimeOriginal);

                // found pictures with date = "0000:00:00 00:00:00", this is before DateTime.MinValu, so I'm setting date to min value in this case
                if(dateTime.Equals("0000:00:00 00:00:00"))
                {
                    dateTime = DateTime.MinValue.ToString("yyyy:MM:dd HH:mm:ss");
                }
            }
            foreach (var directory in directories)
                foreach (var tag in directory.Tags)
                    Trace.WriteLine($"[{directory.Name}] - {tag.Name} = {tag.Description}");

            Trace.WriteLine("\n\nTime is: " + dateTime +"\n\n");
            return DateTime.ParseExact(dateTime, "yyyy:MM:dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        }

        static public List<string> GetFilesInFolder(string folder)
        {
            //Directory.GetFiles
            return new List<string>() { folder };
        }
    }
}
