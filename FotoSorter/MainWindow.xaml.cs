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

namespace FotoSorter
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<MyFile> files = new ObservableCollection<MyFile>();
        private string InputFolder = ConfigurationManager.AppSettings["sourceFolder"];
        private string _outputFolder = ConfigurationManager.AppSettings["destinationFolder"];

        public string OutputFolder { get => _outputFolder; set => _outputFolder = value; }

        public MainWindow()
        {
            InitializeComponent();
            updateInuputFolder(InputFolder);            
        }


        private void updateInuputFolder(string newFolder)
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
                        updateInuputFolder(dialog.SelectedPath);

                    }
                    if (source.Name == "btnDest")
                    {
                        OutputFolder = dialog.SelectedPath;
                        lblOutFolder.Text = dialog.SelectedPath;
                    }
                }
            }
        }

        private void MoveFiles(object sender, RoutedEventArgs e)
        {

        }
    }

