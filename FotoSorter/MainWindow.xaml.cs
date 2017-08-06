using FotoSorterLib;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Configuration;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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
        }


        private void UpdateInuputFolder(string newFolder)
        {
            files.Clear();
            try
            {
                files = FotoSorterLib.FotoSorterLib.PrepareFiles(newFolder);
                lbInFiles.ItemsSource = files;
            }
            catch (System.UnauthorizedAccessException e)
            {
                System.Windows.MessageBox.Show("Erro: " + e.Message, "Error a procurar as fotos");
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
            btnDoSort.IsEnabled = false;
            string message = String.Format(
                @"Processo concluido.
{0} Fotos copiadas.
{1} fotos repetidas.", result.Count(t => t.Item2 == "OK"), result.Count(t => t.Item2 == "O mesmo ficheiro já existe."));
            System.Windows.MessageBox.Show(message);
        }

        private void SetExecuteButtonStatus()
        {
            btnDoSort.IsEnabled = (files.Count > 0) && !String.IsNullOrEmpty(lblOutFolder.Text);
        }


    }
}

