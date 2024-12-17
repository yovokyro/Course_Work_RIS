namespace ClientEmulator
{
    partial class Emulator
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.connectionButton = new System.Windows.Forms.Button();
            this.ipTextBox = new System.Windows.Forms.TextBox();
            this.errorText = new System.Windows.Forms.Label();
            this.addFilesButton = new System.Windows.Forms.Button();
            this.fileCountString = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textCount = new System.Windows.Forms.NumericUpDown();
            this.startTest = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.completedText = new System.Windows.Forms.Label();
            this.outputCheckBox = new System.Windows.Forms.CheckBox();
            this.timeText = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.textCount)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Server IP:";
            // 
            // connectionButton
            // 
            this.connectionButton.Location = new System.Drawing.Point(201, 10);
            this.connectionButton.Name = "connectionButton";
            this.connectionButton.Size = new System.Drawing.Size(89, 23);
            this.connectionButton.TabIndex = 1;
            this.connectionButton.Text = "Connection";
            this.connectionButton.UseVisualStyleBackColor = true;
            this.connectionButton.Click += new System.EventHandler(this.connectionButton_Click);
            // 
            // ipTextBox
            // 
            this.ipTextBox.Location = new System.Drawing.Point(73, 10);
            this.ipTextBox.Name = "ipTextBox";
            this.ipTextBox.Size = new System.Drawing.Size(122, 20);
            this.ipTextBox.TabIndex = 2;
            this.ipTextBox.Text = "127.0.0.1";
            // 
            // errorText
            // 
            this.errorText.AutoSize = true;
            this.errorText.ForeColor = System.Drawing.Color.ForestGreen;
            this.errorText.Location = new System.Drawing.Point(296, 15);
            this.errorText.Name = "errorText";
            this.errorText.Size = new System.Drawing.Size(41, 13);
            this.errorText.TabIndex = 3;
            this.errorText.Text = "Успех!";
            // 
            // addFilesButton
            // 
            this.addFilesButton.Enabled = false;
            this.addFilesButton.Location = new System.Drawing.Point(12, 62);
            this.addFilesButton.Name = "addFilesButton";
            this.addFilesButton.Size = new System.Drawing.Size(89, 25);
            this.addFilesButton.TabIndex = 5;
            this.addFilesButton.Text = "AddFiles";
            this.addFilesButton.UseVisualStyleBackColor = true;
            this.addFilesButton.Click += new System.EventHandler(this.addFilesButton_Click);
            // 
            // fileCountString
            // 
            this.fileCountString.AutoSize = true;
            this.fileCountString.Location = new System.Drawing.Point(107, 68);
            this.fileCountString.Name = "fileCountString";
            this.fileCountString.Size = new System.Drawing.Size(190, 13);
            this.fileCountString.TabIndex = 6;
            this.fileCountString.Text = "Количество загруженный файлов: 0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 43);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(119, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Количество отправок:";
            // 
            // textCount
            // 
            this.textCount.Enabled = false;
            this.textCount.Location = new System.Drawing.Point(141, 39);
            this.textCount.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.textCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.textCount.Name = "textCount";
            this.textCount.Size = new System.Drawing.Size(63, 20);
            this.textCount.TabIndex = 8;
            this.textCount.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // startTest
            // 
            this.startTest.Enabled = false;
            this.startTest.Location = new System.Drawing.Point(12, 93);
            this.startTest.Name = "startTest";
            this.startTest.Size = new System.Drawing.Size(89, 27);
            this.startTest.TabIndex = 9;
            this.startTest.Text = "StartTest";
            this.startTest.UseVisualStyleBackColor = true;
            this.startTest.Click += new System.EventHandler(this.startTest_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(110, 93);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(120, 17);
            this.checkBox1.TabIndex = 10;
            this.checkBox1.Text = "Мультипоточность";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // flowLayoutPanel
            // 
            this.flowLayoutPanel.AutoScroll = true;
            this.flowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel.Location = new System.Drawing.Point(19, 163);
            this.flowLayoutPanel.Name = "flowLayoutPanel";
            this.flowLayoutPanel.Size = new System.Drawing.Size(433, 370);
            this.flowLayoutPanel.TabIndex = 12;
            this.flowLayoutPanel.WrapContents = false;
            // 
            // completedText
            // 
            this.completedText.AutoSize = true;
            this.completedText.Location = new System.Drawing.Point(19, 144);
            this.completedText.Name = "completedText";
            this.completedText.Size = new System.Drawing.Size(87, 13);
            this.completedText.TabIndex = 13;
            this.completedText.Text = "Выполнено: 0/0";
            // 
            // outputCheckBox
            // 
            this.outputCheckBox.AutoSize = true;
            this.outputCheckBox.Location = new System.Drawing.Point(110, 116);
            this.outputCheckBox.Name = "outputCheckBox";
            this.outputCheckBox.Size = new System.Drawing.Size(119, 17);
            this.outputCheckBox.TabIndex = 14;
            this.outputCheckBox.Text = "Вывод результата";
            this.outputCheckBox.UseVisualStyleBackColor = true;
            this.outputCheckBox.CheckedChanged += new System.EventHandler(this.outputCheckBox_CheckedChanged);
            // 
            // timeText
            // 
            this.timeText.AutoSize = true;
            this.timeText.Location = new System.Drawing.Point(409, 147);
            this.timeText.Name = "timeText";
            this.timeText.Size = new System.Drawing.Size(43, 13);
            this.timeText.TabIndex = 15;
            this.timeText.Text = "0";
            // 
            // Emulator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(464, 545);
            this.Controls.Add(this.timeText);
            this.Controls.Add(this.outputCheckBox);
            this.Controls.Add(this.completedText);
            this.Controls.Add(this.flowLayoutPanel);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.startTest);
            this.Controls.Add(this.textCount);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.fileCountString);
            this.Controls.Add(this.addFilesButton);
            this.Controls.Add(this.errorText);
            this.Controls.Add(this.ipTextBox);
            this.Controls.Add(this.connectionButton);
            this.Controls.Add(this.label1);
            this.Name = "Emulator";
            this.Text = "ClientEmulator";
            ((System.ComponentModel.ISupportInitialize)(this.textCount)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button connectionButton;
        private System.Windows.Forms.TextBox ipTextBox;
        private System.Windows.Forms.Label errorText;
        private System.Windows.Forms.Button addFilesButton;
        private System.Windows.Forms.Label fileCountString;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown textCount;
        private System.Windows.Forms.Button startTest;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel;
        private System.Windows.Forms.Label completedText;
        private System.Windows.Forms.CheckBox outputCheckBox;
        private System.Windows.Forms.Label timeText;
    }
}

