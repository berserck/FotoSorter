using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System.Diagnostics;
using System.IO;
using System.Collections.ObjectModel;

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
            IEnumerable <MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(filename);

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

        static public ObservableCollection<MyFile> PrepareFiles(string newFolder) {
            // get the fileinfo
            // loop over the files
            // get the inFilename
            // calculate the outfilenam
            // outfilename has: outpath(folder with outFolder/Year/Month)/filename(perfix(date)-middle.extension)
            // when the file is copied a suffix might be needed in case file with same name exists

            ObservableCollection<MyFile> files = new ObservableCollection<MyFile>();
            DirectoryInfo dirInfo = new DirectoryInfo(newFolder);

            string supportedExtensions = "*.jpg,*.jpe,*.jpeg,*.wmf,*.avi,*.mov";
            var info = dirInfo.EnumerateFiles("*.*", SearchOption.AllDirectories).
                Where(s => !String.IsNullOrEmpty(Path.GetExtension(s.Name)) && supportedExtensions.Contains(Path.GetExtension(s.Name).ToLower()));
            foreach (var item in info)
            {
                files.Add(new MyFile() { FilenameIn = item.FullName });
            }
            return files;
        }

    }
    public class MyFile
    {
        public string FilenameIn { get; set; }
        public string FilenameOut { get; set; }
        public DateTime? CaptureDate { get; set; }
    }
}
