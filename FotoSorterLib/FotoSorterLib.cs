﻿using System;
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
    public static class FotoSorterLib
    {
        static public DateTime GetPhotoDate(string filename)
        {
            var dateTime = DateTime.MinValue.ToString("yyyy:MM:dd HH:mm:ss");
            // open image
            IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(filename);

            var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            if (subIfdDirectory != null)
            {
                dateTime = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagDateTimeOriginal);

                // found pictures with date = "0000:00:00 00:00:00", this is before DateTime.MinValu, so I'm setting date to min value in this case
                if (dateTime.Equals("0000:00:00 00:00:00"))
                {
                    dateTime = DateTime.MinValue.ToString("yyyy:MM:dd HH:mm:ss");
                }
            }
            foreach (var directory in directories)
                foreach (var tag in directory.Tags)
                    Trace.WriteLine($"[{directory.Name}] - {tag.Name} = {tag.Description}");

            Trace.WriteLine("\n\nTime is: " + dateTime + "\n\n");
            return DateTime.ParseExact(dateTime, "yyyy:MM:dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        }

        static public string GetOutFilename(string fullPath)
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

        static public ObservableCollection<MyFile> PrepareFiles(string inFolder)
        {
            var files = new ObservableCollection<MyFile>();
            var dirInfo = new DirectoryInfo(inFolder);

            string supportedExtensions = "*.jpg,*.jpe,*.jpeg,*.wmf,*.avi,*.mov";
            var info = dirInfo.EnumerateFiles("*.*", SearchOption.AllDirectories).
                Where(s => !String.IsNullOrEmpty(Path.GetExtension(s.Name)) && supportedExtensions.Contains(Path.GetExtension(s.Name).ToLower()));
            foreach (var item in info)
            {
                var filenameIn = item.FullName;
                var date = GetPhotoDate(filenameIn);
                var captureDate = GetPhotoDate(filenameIn);
                var fileOutName = Path.GetFileNameWithoutExtension(filenameIn);
                var extension = Path.GetExtension(item.Name);

                files.Add(new MyFile() { FilenameIn = item.FullName, FileOutName = fileOutName, FileOutExtension = extension, CaptureDate = captureDate });
            }
            return files;
        }

        static public string GetTimeFolder(DateTime? date)
        {
            if (date == null)
            {
                return "desconhecido";
            }
            return date?.Year.ToString() + "\\" + date?.Month.ToString().PadLeft(2, '0');
        }

        /// <summary>
        /// This function will get a list of files representing photos ad it will produce files in teh form:
        /// <destFolderBase>/<year>/<month>/<date in dateformat>_<original name or fileName if given>_<counter if destination file exists>.<extension>
        /// </summary>
        /// <param name="files">List of files found</param>
        /// <param name="destFolderBase"></param>
        /// <param name="dateFormat"></param>
        /// <param name="fileName">Base filename to write</param>
        /// <returns></returns>
        static public ObservableCollection<Tuple<string,string>> CopyFiles(ObservableCollection<MyFile> files, string destFolderBase, string dateFormat, string fileName)
        {
            var processed = new ObservableCollection<Tuple<string, string>>();
            // loop over the files
            foreach (var item in files)
            {
                var destFolder = Path.Combine(destFolderBase, GetTimeFolder(item.CaptureDate));
                // calculate the outfilename
                var outFilename = item.CaptureDate?.ToString(dateFormat) + "_"
                    + (String.IsNullOrEmpty(fileName) ? item.FileOutName : fileName)
                    ;
                string result = SimpleFileCopy(item.FilenameIn, outFilename, destFolder);
                processed.Add(Tuple.Create(item.FilenameIn, result));
            }
            return processed;
        }

        /// <summary>
        /// code mix of https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/file-system/how-to-copy-delete-and-move-files-and-folders
        ///  and https://stackoverflow.com/questions/13049732/automatically-rename-a-file-if-it-already-exists-in-windows-way#
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="targetFileName"></param>
        /// <param name="targetPath"></param>
        /// <returns></returns>
        static public string SimpleFileCopy(string sourceFile, string targetFileName, string targetPath)
        {
            string destFile = System.IO.Path.Combine(targetPath, targetFileName);

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
                    return "O mesmo ficheiro já existe.";
                }

                string tempFileName = string.Format("{0}_{1}", fileNameOnly, count++);
                destFile = System.IO.Path.Combine(targetPath, tempFileName + extension);
            }
            try
            {
                System.IO.File.Copy(sourceFile, destFile);
            } catch (System.Exception e)
            {
                return "Erro: " + e.Message;
            }
            return "OK";
        }



        /// <summary>
        /// From https://stackoverflow.com/questions/1358510/how-to-compare-2-files-fast-using-net
        /// </summary>
        const int BYTES_TO_READ = sizeof(Int64);
        static public bool FilesAreEqual(string firstFileName, string secondFileName)
        {

            FileInfo first = new FileInfo(firstFileName);
            FileInfo second = new FileInfo(secondFileName);

            if (first.Length != second.Length)
                return false;

            if (first.FullName == second.FullName)
                return true;

            int iterations = (int)Math.Ceiling((double)first.Length / BYTES_TO_READ);

            using (FileStream fs1 = first.OpenRead())
            using (FileStream fs2 = second.OpenRead())
            {
                byte[] one = new byte[BYTES_TO_READ];
                byte[] two = new byte[BYTES_TO_READ];

                for (int i = 0; i < iterations; i++)
                {
                    fs1.Read(one, 0, BYTES_TO_READ);
                    fs2.Read(two, 0, BYTES_TO_READ);

                    if (BitConverter.ToInt64(one, 0) != BitConverter.ToInt64(two, 0))
                        return false;
                }
            }
            return true;
        }


    }
    public class MyFile
    {
        public string FilenameIn { get; set; }
        public string FileOutName { get; set; }
        public string FileOutExtension { get; set; }
        public DateTime? CaptureDate { get; set; }
    }
}
