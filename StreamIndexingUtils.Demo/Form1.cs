using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using StreamIndexingUtils.Models;

namespace StreamIndexingUtils.Demo
{
    public partial class Form1 : Form
    {
        private IndexedStreamReaderWriter idxStream;

        public Form1()
        {
            InitializeComponent();
        }

        private async void AddButtonClickAsync(object sender, EventArgs e)
        {
            await UseLoadingIndicator(() => ErrorMessageTryCatch(async () =>
            {
                if (idxStream == null)
                {
                    MessageBox.Show("Please select file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    if (selectFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        var fileInfo = new FileInfo(selectFileDialog.FileName);
                        using (var file = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                        {
                            await idxStream.WriteAsync(file, fileInfo.Name);
                            await idxStream.SaveContentIndexAsync();
                            UpdateList();
                        }
                    }
                }
            }));
        }

        private void CloseFileButtonClick(object sender, EventArgs e)
        {
            DisposeStream();
            activeFileTextBox.Text = string.Empty;
            UpdateList();
        }

        private async void CreateButtonClick(object sender, EventArgs e)
        {
            await UseLoadingIndicator(() => ErrorMessageTryCatch(async () =>
            {
                if (createFileDialog.ShowDialog() == DialogResult.OK)
                {
                    DisposeStream();
                    activeFileTextBox.Text = createFileDialog.FileName;
                    idxStream = new IndexedStreamReaderWriter(
                        new FileStream(activeFileTextBox.Text, FileMode.Create, FileAccess.ReadWrite, FileShare.None),
                        new ContentIndex());

                    await idxStream.SaveContentIndexAsync();
                    UpdateList();
                }
            }));
        }

        private async void ExtractButtonClickAsync(object sender, EventArgs e)
        {
            await UseLoadingIndicator(() => ErrorMessageTryCatch(async () =>
            {
                if (fileContentListBox.SelectedItem == null)
                {
                    MessageBox.Show("Item not selected", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    var fullPath = Path.Combine(
                        folderBrowserDialog.SelectedPath,
                        fileContentListBox.SelectedItem.ToString());
                    int count = 1;

                    string fileNameOnly = Path.GetFileNameWithoutExtension(fullPath);
                    string extension = Path.GetExtension(fullPath);
                    string path = Path.GetDirectoryName(fullPath);
                    string newFullPath = fullPath;

                    while (File.Exists(newFullPath))
                    {
                        string tempFileName = $"{fileNameOnly} ({count++})";
                        newFullPath = Path.Combine(path, tempFileName + extension);
                    }

                    using (var file = new FileStream(newFullPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                    {
                        await idxStream.ReadAsync(file, fileContentListBox.SelectedItem.ToString());
                    }
                }
            }));
        }

        private async void RemoveButtonClick(object sender, EventArgs e)
        {
            await UseLoadingIndicator(() => ErrorMessageTryCatch(async () =>
            {
                await idxStream.RemoveAsync(fileContentListBox.SelectedItem.ToString());
                await idxStream.SaveContentIndexAsync();
                UpdateList();
            }));
        }

        private async void SelectButtonClick(object sender, EventArgs e)
        {
            await UseLoadingIndicator(() => ErrorMessageTryCatch(async () =>
            {
                if (selectFileDialog.ShowDialog() == DialogResult.OK)
                {
                    DisposeStream();
                    activeFileTextBox.Text = selectFileDialog.FileName;
                    idxStream = new IndexedStreamReaderWriter(
                         new FileStream(activeFileTextBox.Text, FileMode.Open, FileAccess.ReadWrite, FileShare.None));
                    await idxStream.LoadContentIndexAsync();
                    UpdateList();
                }
            }));
        }

        private void DisposeStream()
        {
            if (idxStream != null)
            {
                idxStream.Dispose();
                idxStream = null;
            }
        }

        private void UpdateList()
        {
            if (idxStream != null)
            {
                fileContentListBox.DataSource = idxStream.CurrentContentIndex.Keys.ToList();
            }
            else
            {
                fileContentListBox.DataSource = null;
            }
        }

        private async Task ErrorMessageTryCatch(Func<Task> func)
        {
            try
            {
                await func();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task UseLoadingIndicator(Func<Task> func)
        {
            try
            {
                processingLabel.Text = "Processing request. Please wait...";
                await func();
            }
            finally
            {
                processingLabel.Text = "Done";
            }
        }
    }
}