﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;

namespace FotoSorter
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<MyFile> files = new ObservableCollection<MyFile>();
        private string InputFolder = "C:\\Users\\pgp\\Documents\\Visual Studio 2017\\Projects\\FotoSorter\\FotoSorterLibTests\\TestPhotos\\";

        public MainWindow()
        {
            InitializeComponent();
            lbInFiles.ItemsSource = files;
            updateInuputFolder(InputFolder);
            
        }


        private void updateInuputFolder(string newFolder)
        {
            files.Clear();
            DirectoryInfo dirInfo = new DirectoryInfo(newFolder);
            try
            {
                string supportedExtensions = "*.jpg,*.jpe,*.jpeg,*.wmf,*.avi,*.mov";
                var info = dirInfo.EnumerateFiles("*.*", SearchOption.AllDirectories).
                    Where(s => !String.IsNullOrEmpty(Path.GetExtension(s.Name)) && supportedExtensions.Contains(Path.GetExtension(s.Name).ToLower()) );
                foreach (var item in info)
                {
                    files.Add(new MyFile() { FilenameIn = item.FullName });
                }
            }catch (System.UnauthorizedAccessException e)
            {
                System.Windows.MessageBox.Show("Erro: " + e.Message, "Error a procurar as fotos");
            }
            lblInFolder.Text = newFolder;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    updateInuputFolder(dialog.SelectedPath);
                }
            }
        }
    }
    public class MyFile
    {
        public string FilenameIn { get; set; }
        public string FilenameOut { get; set; }
        public DateTime? CaptureDate { get; set; }
    }

}
