using System;
using System.Collections.Generic;
using System.Linq;
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
    public static class FotoSorterLib
    {

        public static DateTime GetPhotoDate(string filename)
        {
            var dateTime = DateTime.MinValue.ToString("yyyy:MM:dd HH:mm:ss");
            // open image
            IEnumerable<MetadataExtractor.Directory> directories = null;
            try
            {
                directories = ImageMetadataReader.ReadMetadata(filename);
            }
            catch (ImageProcessingException e)
            {
                // TODO log error
            }
            var subIfdDirectory = directories?.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            if (subIfdDirectory != null)
            {
                dateTime = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagDateTimeOriginal) ?? DateTime.MinValue.ToString("yyyy:MM:dd HH:mm:ss");

                // found pictures with date = "0000:00:00 00:00:00", this is before DateTime.MinValu, so I'm setting date to min value in this case
                if (dateTime.Equals("0000:00:00 00:00:00"))
                {
                    dateTime = DateTime.MinValue.ToString("yyyy:MM:dd HH:mm:ss");
                }
            }

            return DateTime.ParseExact(dateTime, "yyyy:MM:dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        }

        public static string GetOutFilename(string fullPath)
        {
            int count = 1;

            string fileNameOnly = Path.GetFileNameWithoutExtension(fullPath);
            string extension = Path.GetExtension(fullPath);
            string path = Path.GetDirectoryName(fullPath);
            string newFullPath = fullPath;

            while (File.Exists(newFullPath))
            {
                string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
                newFullPath = Path.Combine(path, tempFileName + extension);
            }
            return newFullPath;
        }

        public static ObservableCollection<MyFile> PrepareFiles(string inFolder)
        {
            var files = new ObservableCollection<MyFile>();
            var dirInfo = new DirectoryInfo(inFolder);

            string supportedExtensions = "*.jpg,*.jpe,*.jpeg,*.wmf,*.avi,*.mov";
            var info = dirInfo.EnumerateFiles("*.*", SearchOption.AllDirectories).
                Where(s => !String.IsNullOrEmpty(Path.GetExtension(s.Name)) && supportedExtensions.Contains(Path.GetExtension(s.Name).ToLower()));
            foreach (var item in info)
            {
                Trace.WriteLine($"Processing file {item.FullName}");
                var filenameIn = item.FullName;
                files.Add(new MyFile() { FilenameOrigin = item.FullName});
            }
            return files;
        }

        public static string GetTimeFolder(DateTime? date)
        {
            if (date == null)
            {
                return "desconhecido";
            }
            return date?.Year.ToString() + "\\" + date?.Month.ToString().PadLeft(2, '0');
        }

        /// <summary>
        /// code mix of https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/file-system/how-to-copy-delete-and-move-files-and-folders
        ///  and https://stackoverflow.com/questions/13049732/automatically-rename-a-file-if-it-already-exists-in-windows-way#
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="targetFileName"></param>
        /// <param name="targetPath"></param>
        /// <returns></returns>
        public static Tuple<CopyResult, string> SimpleFileCopy(string sourceFile, string targetFileName, string targetPath)
        {
            string destFile = Path.Combine(targetPath, targetFileName);

            // To copy a folder's contents to a new location:
            // Create a new target folder, if necessary.
            if (!System.IO.Directory.Exists(targetPath))
            {
                System.IO.Directory.CreateDirectory(targetPath);
            }

            string fileNameOnly = Path.GetFileNameWithoutExtension(targetFileName);
            string extension = Path.GetExtension(targetFileName);

            int count = 1;
            while (File.Exists(destFile))
            {
                // test if the file is the same
                if (FilesAreEqual(sourceFile, destFile))
                {
                    return Tuple.Create(CopyResult.SameFileFound, "O mesmo ficheiro já existe.");
                }

                string tempFileName = string.Format("{0}_{1}", fileNameOnly, count++);
                destFile = Path.Combine(targetPath, tempFileName + extension);
            }
            try
            {
                File.Copy(sourceFile, destFile);
            }
            catch (Exception e)
            {
                return Tuple.Create(CopyResult.Error, "Erro: " + e.Message);
            }
            return Tuple.Create(CopyResult.Sucess, "OK");
        }

        /// <summary>
        /// Function that will return the path, from a full path with or without a filename in the end
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public static string GetPath(string fullPath)
        {
            bool isFolder = File.GetAttributes(fullPath).HasFlag(FileAttributes.Directory);
            if (!isFolder)
            {
                return new FileInfo(fullPath).Directory.FullName;

            }
            else
            {
                return fullPath;

            }
        }


        /// <summary>
        /// From https://stackoverflow.com/questions/1358510/how-to-compare-2-files-fast-using-net
        /// </summary>
        const int BytesToRead = sizeof(Int64);
        public static bool FilesAreEqual(string firstFileName, string secondFileName)
        {

            FileInfo first = new FileInfo(firstFileName);
            FileInfo second = new FileInfo(secondFileName);

            if (first.Length != second.Length)
                return false;

            if (first.FullName == second.FullName)
                return true;

            int iterations = (int)Math.Ceiling((double)first.Length / BytesToRead);

            using (FileStream fs1 = first.OpenRead())
            using (FileStream fs2 = second.OpenRead())
            {
                byte[] one = new byte[BytesToRead];
                byte[] two = new byte[BytesToRead];

                for (int i = 0; i < iterations; i++)
                {
                    fs1.Read(one, 0, BytesToRead);
                    fs2.Read(two, 0, BytesToRead);

                    if (BitConverter.ToInt64(one, 0) != BitConverter.ToInt64(two, 0))
                        return false;
                }
            }
            return true;
        }

    }

    public enum CopyResult
    {
        Sucess, SameFileFound, Error
    };

    public class MyFile
    {
        private string _filenameIn;

        public string FilenameOrigin
        {
            get { return _filenameIn; }
            set { _filenameIn = value; }
        }
        public string FileOutName
        { get { return Path.GetFileNameWithoutExtension(_filenameIn); } }

        public string FileOutExtension
        { get { return Path.GetExtension(_filenameIn); } }

        public DateTime? CaptureDate
        {
            get { return FotoSorterLib.GetPhotoDate(_filenameIn); }
        }
        public string DestinationFolder { get; set; }
        public string Message { get; set; }
        public CopyResult Status { get; set; }
    }

    public class CopyResults
    {
        public string FilenameOrigin { get; set; }
        public string DestinationFolder { get; set; }
        public string Message { get; set; }
        public CopyResult Status { get; set; }
    }
}
