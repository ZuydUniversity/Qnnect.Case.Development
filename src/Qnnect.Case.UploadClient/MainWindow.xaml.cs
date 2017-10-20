using System.Configuration;
using System.IO;
using System.Windows;
using Microsoft.Win32;

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
                var uploadManager = UploadManager.Create(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);

                using (var photoFileStream = File.OpenRead(openFileDialog.FileName))
                {
                    await uploadManager.UploadPicture(photoFileStream, Path.GetFileName(openFileDialog.FileName), FirstNameTextBox.Text, LastNameTextBox.Text, EmailAddressTextBox.Text);
                }
            }
        }
    }
}
