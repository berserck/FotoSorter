﻿using FotoSorterLib;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;

namespace FotoSorter
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<MyFile> files = new ObservableCollection<MyFile>();
        private string _previousSourceFolder = null;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            lblInFolder.Text = String.Empty;
            lblOutFolder.Text = ConfigurationManager.AppSettings["destinationFolder"];
            Log.Information("Starting finish initializing main window!");
        }


        private void UpdateInuputFolder(string newFolder)
        {
            Log.Information("Starting UpdateInuputFolder");
            files.Clear();
            try
            {
                files = FotoSorterLib.FotoSorterLib.PrepareFiles(newFolder);
                lbInFiles.ItemsSource = files;
                Log.Information("Files Found {@files}", files);
            }
            catch (System.UnauthorizedAccessException e)
            {
                System.Windows.MessageBox.Show("Erro: " + e.Message, "Error a procurar as fotos");
                Log.Error(e.Message);
            }
            lblInFolder.Text = newFolder;
            SetExecuteButtonStatus();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                var source = sender as System.Windows.Controls.Button;

                var startFolderPath = GetFolderPath(source.Name);

                if (!String.IsNullOrEmpty(startFolderPath))
                {
                    dialog.SelectedPath = startFolderPath;
                }
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    if (source.Name == "btnSource")
                    {
                        UpdateInuputFolder(dialog.SelectedPath);
                    }
                    if (source.Name == "btnDest")
                    {
                        lblOutFolder.Text = dialog.SelectedPath;
                        SetExecuteButtonStatus();
                    }
                }
            }
        }

        private string GetFolderPath(string name)
        {
            if (name == "btnSource")
            {
                return lblInFolder.Text;
            }
            else
            {
                return lblOutFolder.Text;
            }
        }



        static public ObservableCollection<CopyResults> CopyFiles(ObservableCollection<MyFile> files, string destFolderBase, string dateFormat, string fileName, string eventName = "")
        {
            var processed = new ObservableCollection<CopyResults>();
            // loop over the files
            int count = 0;
            foreach (var item in files)
            {
                count++;
                if (count % 10 == 0)
                {

                }
                var destFolder = Path.Combine(destFolderBase, FotoSorterLib.FotoSorterLib.GetTimeFolder(item.CaptureDate), eventName);
                // calculate the outfilename
                var outFilename = item.CaptureDate?.ToString(dateFormat) + "_"
                    + (String.IsNullOrEmpty(fileName) ? item.FileOutName : fileName)
                    + item.FileOutExtension;
                var result = FotoSorterLib.FotoSorterLib.SimpleFileCopy(item.FilenameIn, outFilename, destFolder);
                processed.Add(new CopyResults()
                {
                    FilenameOrigin = item.FilenameIn,
                    DestinationFolder = destFolder,
                    Message = result.Item2,
                    Status = result.Item1
                });
                //processed.Add(Tuple.Create(item.FilenameIn, destFolder, result));
            }
            return processed;
        }


        private void SetExecuteButtonStatus()
        {
            btnDoSort.IsEnabled = (files.Count > 0) && !String.IsNullOrEmpty(lblOutFolder.Text);
        }

        #region DragnDrop

        private void lblInFolder_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy; // Okay
                string[] fullPaths = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                string fullPath = fullPaths[0];
                var folder = FotoSorterLib.FotoSorterLib.GetPath(fullPath);
                lblInFolder.Text = folder;
            }
            e.Handled = true;
        }

        private void lblInFolder_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy; // Okay
                string[] fullPaths = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                string fullPath = fullPaths[0];
                var folder = FotoSorterLib.FotoSorterLib.GetPath(fullPath);
                Log.Information("File Drop: {@original} turned into {@folder}", fullPath, folder);
                lblInFolder.Text = folder;
                UpdateInuputFolder(folder);
            }
            e.Handled = true;
        }

        private void lblInFolder_PreviewDragOver(object sender, DragEventArgs e)
        {
            //base.OnPreviewDragOver(e);
            _previousSourceFolder = lblInFolder.Text;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy; // Okay
            }
            else
            {
                e.Effects = DragDropEffects.None; // Unknown data, ignore it
            }
            e.Handled = true;
        }

        private void lblInFolder_PreviewDragLeave(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                lblInFolder.Text = _previousSourceFolder;
            }
            e.Handled = true;
        }

        private void lblInFolder_PreviewDragEnter(object sender, DragEventArgs e)
        {
            _previousSourceFolder = lblInFolder.Text;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy; // Okay
            }
            else
            {
                e.Effects = DragDropEffects.None; // Unknown data, ignore it
            }
            e.Handled = true;
        }
        #endregion

        #region ProgressBars
        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            copyStatus.Value = e.ProgressPercentage;
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var processed = new ObservableCollection<CopyResults>();
            var arg = e.Argument as CopyArguments;
            // loop over the files
            int count = 0;
            int total = files.Count;
            foreach (var item in files)
            {
                count++;
                if (count % 5 == 0)
                {
                    int progress = (int)(((double)count / (double)total) * 100);
                    (sender as BackgroundWorker).ReportProgress(progress);
                }
                var destFolder = Path.Combine(arg.DestFolderBase, FotoSorterLib.FotoSorterLib.GetTimeFolder(item.CaptureDate), arg.EventName);
                // calculate the outfilename
                var outFilename = item.CaptureDate?.ToString(arg.DateFormat) + "_"
                    + (String.IsNullOrEmpty(arg.FileName) ? item.FileOutName : arg.FileName)
                    + item.FileOutExtension;
                var result = FotoSorterLib.FotoSorterLib.SimpleFileCopy(item.FilenameIn, outFilename, destFolder);
                processed.Add(new CopyResults()
                {
                    FilenameOrigin = item.FilenameIn,
                    DestinationFolder = destFolder,
                    Message = result.Item2,
                    Status = result.Item1
                });
                e.Result = processed;
            }

            //for (int i = 0; i < 100; i++)
            //{
            //    (sender as BackgroundWorker).ReportProgress(i);
            //    Thread.Sleep(100);
            //}

        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            copyStatus.Value = 0;
            btnDoSort.IsEnabled = false;
            ObservableCollection<CopyResults> result = e.Result as ObservableCollection<CopyResults>;
            string message = String.Format(
                @"Processo concluido.
            {0} Fotos copiadas.
            {1} fotos repetidas.", result.Count(t => t.Status == CopyResult.Sucess), result.Count(t => t.Status == CopyResult.SameFileFound));
            Log.Information(message);
            gridResult.ItemsSource = result;

            MessageBox.Show(message);
        }

        private void ExecuteButtonClick(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;

            Log.Information("Starting to process files: {@files}", files);

            var arg = new CopyArguments() { Files = files, DestFolderBase = lblOutFolder.Text, DateFormat = "yyyy.MM.dd", FileName = String.Empty, EventName = txtEvent.Text };
            worker.RunWorkerAsync(arg);

            //            var result = CopyFiles(files, lblOutFolder.Text, "yyyy.MM.dd", String.Empty, txtEvent.Text);
            //            Log.Information("Processed files: {@files}", result);
            //            btnDoSort.IsEnabled = false;
            //            string message = String.Format(
            //                @"Processo concluido.
            //{0} Fotos copiadas.
            //{1} fotos repetidas.", result.Count(t => t.Status == CopyResult.Sucess), result.Count(t => t.Status == CopyResult.SameFileFound));
            //            Log.Information(message);

            //            gridResult.ItemsSource = result;

            //            System.Windows.MessageBox.Show(message);
        }

        #endregion

    }

    public class CopyArguments
    {
        public ObservableCollection<MyFile> Files { get; set; }
        public string DestFolderBase { get; set; }
        public string DateFormat { get; set; }
        public string FileName { get; set; }
        public string EventName { get; set; }
    }
}

