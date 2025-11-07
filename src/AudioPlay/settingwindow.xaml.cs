using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AudioPlay
{
    /// <summary>
    /// Interaction logic for settingwindow.xaml
    /// </summary>
    public partial class settingwindow : Window
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
            this.Close();
        }
    }
}
