using FotoSorterLib;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
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
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                var source = sender as System.Windows.Controls.Button;

                var startFolderPath = GetFolderPath(source.Name);

                if (! String.IsNullOrEmpty(startFolderPath))
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
                    }
                }
                SetExecuteButtonStatus();
            }
        }

        private string GetFolderPath(string name)
        {
            if (name == "btnSource")
            {
                return lblInFolder.Text;
            } else
            {
                return lblOutFolder.Text;
            }
        }

        private void MoveFiles(object sender, RoutedEventArgs e)
        {
            var result = FotoSorterLib.FotoSorterLib.CopyFiles(files, lblOutFolder.Text, "yyyy.MM.dd", String.Empty);
            Log.Information("Processed files: {@files}", result);
            btnDoSort.IsEnabled = false;
            string message = String.Format(
                @"Processo concluido.
{0} Fotos copiadas.
{1} fotos repetidas.", result.Count(t => t.Status == CopyResult.Sucess), result.Count(t => t.Status == CopyResult.SameFileFound));
            Log.Information(message);

            gridResult.ItemsSource = result;

            System.Windows.MessageBox.Show(message);
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

    }
}

