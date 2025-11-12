using MahApps.Metro.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Text.Json;
using System.IO;
using Path = System.IO.Path;

namespace AudioPlay
{
    /// <summary>
    /// Interaction logic for settingwindow.xaml
    /// </summary>
    public partial class settingwindow : MetroWindow
    {

        
        public settingwindow()
        {
            InitializeComponent();
            audiopathbox.Text = MainWindow.defaultpath;
        }

        private void browse_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog openfolder = new OpenFolderDialog();
            //openfolder.ShowDialog();
            bool? result = openfolder.ShowDialog();
            if (result == true)
            {
                MainWindow.defaultpath = openfolder.FolderName;
                audiopathbox.Text = openfolder.FolderName;
            }

        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            
            try
            {
                string txtpath = "data.txt";
                File.WriteAllText(txtpath, audiopathbox.Text);

            }
            catch (Exception) { }

            this.Close();
        }
    }
}
