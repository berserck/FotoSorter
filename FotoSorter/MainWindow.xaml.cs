﻿using FotoSorterLib;
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


    }
}

