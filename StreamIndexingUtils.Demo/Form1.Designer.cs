namespace StreamIndexingUtils.Demo
{
    public partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.createButton = new System.Windows.Forms.Button();
            this.selectButton = new System.Windows.Forms.Button();
            this.closeFileButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.activeFileTextBox = new System.Windows.Forms.TextBox();
            this.fileContentListBox = new System.Windows.Forms.ListBox();
            this.extractButton = new System.Windows.Forms.Button();
            this.removeButton = new System.Windows.Forms.Button();
            this.selectFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.createFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.addButton = new System.Windows.Forms.Button();
            this.processingLabel = new System.Windows.Forms.Label();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.SuspendLayout();
            // 
            // createButton
            // 
            this.createButton.Location = new System.Drawing.Point(12, 12);
            this.createButton.Name = "createButton";
            this.createButton.Size = new System.Drawing.Size(111, 23);
            this.createButton.TabIndex = 0;
            this.createButton.Text = "Create file";
            this.createButton.UseVisualStyleBackColor = true;
            this.createButton.Click += new System.EventHandler(this.CreateButtonClick);
            // 
            // selectButton
            // 
            this.selectButton.Location = new System.Drawing.Point(12, 41);
            this.selectButton.Name = "selectButton";
            this.selectButton.Size = new System.Drawing.Size(111, 23);
            this.selectButton.TabIndex = 1;
            this.selectButton.Text = "Select file";
            this.selectButton.UseVisualStyleBackColor = true;
            this.selectButton.Click += new System.EventHandler(this.SelectButtonClick);
            // 
            // closeFileButton
            // 
            this.closeFileButton.Location = new System.Drawing.Point(12, 70);
            this.closeFileButton.Name = "closeFileButton";
            this.closeFileButton.Size = new System.Drawing.Size(111, 23);
            this.closeFileButton.TabIndex = 2;
            this.closeFileButton.Text = "Close file";
            this.closeFileButton.UseVisualStyleBackColor = true;
            this.closeFileButton.Click += new System.EventHandler(this.CloseFileButtonClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(142, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Active file:";
            // 
            // activeFileTextBox
            // 
            this.activeFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.activeFileTextBox.Location = new System.Drawing.Point(204, 15);
            this.activeFileTextBox.Name = "activeFileTextBox";
            this.activeFileTextBox.Size = new System.Drawing.Size(100, 20);
            this.activeFileTextBox.TabIndex = 4;
            // 
            // fileContentListBox
            // 
            this.fileContentListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fileContentListBox.FormattingEnabled = true;
            this.fileContentListBox.Location = new System.Drawing.Point(145, 46);
            this.fileContentListBox.Name = "fileContentListBox";
            this.fileContentListBox.Size = new System.Drawing.Size(159, 134);
            this.fileContentListBox.TabIndex = 5;
            // 
            // extractButton
            // 
            this.extractButton.Location = new System.Drawing.Point(12, 128);
            this.extractButton.Name = "extractButton";
            this.extractButton.Size = new System.Drawing.Size(111, 23);
            this.extractButton.TabIndex = 6;
            this.extractButton.Text = "Extract selected item";
            this.extractButton.UseVisualStyleBackColor = true;
            this.extractButton.Click += new System.EventHandler(this.ExtractButtonClickAsync);
            // 
            // removeButton
            // 
            this.removeButton.Location = new System.Drawing.Point(12, 157);
            this.removeButton.Name = "removeButton";
            this.removeButton.Size = new System.Drawing.Size(111, 23);
            this.removeButton.TabIndex = 7;
            this.removeButton.Text = "Remove selected item";
            this.removeButton.UseVisualStyleBackColor = true;
            this.removeButton.Click += new System.EventHandler(this.RemoveButtonClick);
            // 
            // selectFileDialog
            // 
            this.selectFileDialog.FileName = "selectFileDialog";
            this.selectFileDialog.RestoreDirectory = true;
            // 
            // createFileDialog
            // 
            this.createFileDialog.DefaultExt = "dat";
            this.createFileDialog.FileName = "file";
            this.createFileDialog.RestoreDirectory = true;
            // 
            // addButton
            // 
            this.addButton.Location = new System.Drawing.Point(12, 99);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(111, 23);
            this.addButton.TabIndex = 8;
            this.addButton.Text = "Add item";
            this.addButton.UseVisualStyleBackColor = true;
            this.addButton.Click += new System.EventHandler(this.AddButtonClickAsync);
            // 
            // processingLabel
            // 
            this.processingLabel.AutoSize = true;
            this.processingLabel.Location = new System.Drawing.Point(12, 193);
            this.processingLabel.Name = "processingLabel";
            this.processingLabel.Size = new System.Drawing.Size(0, 13);
            this.processingLabel.TabIndex = 9;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(316, 218);
            this.Controls.Add(this.processingLabel);
            this.Controls.Add(this.addButton);
            this.Controls.Add(this.removeButton);
            this.Controls.Add(this.extractButton);
            this.Controls.Add(this.fileContentListBox);
            this.Controls.Add(this.activeFileTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.closeFileButton);
            this.Controls.Add(this.selectButton);
            this.Controls.Add(this.createButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button createButton;
        private System.Windows.Forms.Button selectButton;
        private System.Windows.Forms.Button closeFileButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox activeFileTextBox;
        private System.Windows.Forms.ListBox fileContentListBox;
        private System.Windows.Forms.Button extractButton;
        private System.Windows.Forms.Button removeButton;
        private System.Windows.Forms.OpenFileDialog selectFileDialog;
        private System.Windows.Forms.SaveFileDialog createFileDialog;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.Label processingLabel;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
    }
}

