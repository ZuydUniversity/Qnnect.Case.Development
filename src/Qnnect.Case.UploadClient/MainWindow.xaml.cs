using System.Configuration;
using System.IO;
using Microsoft.Win32;
using System.Windows;

namespace Qnnect.Case.UploadClient
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public async void BtnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg)|*.png;*.jpg"
            };

            if (openFileDialog.ShowDialog() == true)
            {

            }
        }
    }
}