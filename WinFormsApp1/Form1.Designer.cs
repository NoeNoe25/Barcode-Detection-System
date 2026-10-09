namespace WinFormsApp1
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.PictureBox pictureBoxOriginal;
        private System.Windows.Forms.PictureBox pictureBoxResult;

        private System.Windows.Forms.Button btnUpload;
        private System.Windows.Forms.Button btnOCR;
        private System.Windows.Forms.Button btnCV;

        private System.Windows.Forms.TextBox txtOutput;

        private System.Windows.Forms.TextBox txtUPC;
        private System.Windows.Forms.TextBox txtSerial;
        private System.Windows.Forms.TextBox txtPart;

        private System.Windows.Forms.Label lblUPC;
        private System.Windows.Forms.Label lblSerial;
        private System.Windows.Forms.Label lblPart;

        private System.Windows.Forms.Label lblTime;

        private void InitializeComponent()
        {
            pictureBoxOriginal = new PictureBox();
            pictureBoxResult = new PictureBox();
            btnUpload = new Button();
            btnOCR = new Button();
            btnCV = new Button();
            txtOutput = new TextBox();
            txtUPC = new TextBox();
            txtSerial = new TextBox();
            txtPart = new TextBox();
            lblUPC = new Label();
            lblSerial = new Label();
            lblPart = new Label();
            lblTime = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBoxOriginal).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxResult).BeginInit();
            SuspendLayout();
            // 
            // pictureBoxOriginal
            // 
            pictureBoxOriginal.Location = new Point(20, 20);
            pictureBoxOriginal.Name = "pictureBoxOriginal";
            pictureBoxOriginal.Size = new Size(400, 300);
            pictureBoxOriginal.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxOriginal.TabIndex = 0;
            pictureBoxOriginal.TabStop = false;
            // 
            // pictureBoxResult
            // 
            pictureBoxResult.Location = new Point(450, 20);
            pictureBoxResult.Name = "pictureBoxResult";
            pictureBoxResult.Size = new Size(400, 300);
            pictureBoxResult.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxResult.TabIndex = 1;
            pictureBoxResult.TabStop = false;
            // 
            // btnUpload
            // 
            btnUpload.Location = new Point(20, 340);
            btnUpload.Name = "btnUpload";
            btnUpload.Size = new Size(120, 40);
            btnUpload.TabIndex = 2;
            btnUpload.Text = "Upload";
            btnUpload.Click += btnUpload_Click;
            // 
            // btnOCR
            // 
            btnOCR.Location = new Point(160, 340);
            btnOCR.Name = "btnOCR";
            btnOCR.Size = new Size(120, 40);
            btnOCR.TabIndex = 3;
            btnOCR.Text = "Direct OCR";
            btnOCR.Click += btnOCR_Click;
            // 
            // btnCV
            // 
            btnCV.Location = new Point(300, 340);
            btnCV.Name = "btnCV";
            btnCV.Size = new Size(120, 40);
            btnCV.TabIndex = 4;
            btnCV.Text = "CV + OCR";
            btnCV.Click += btnCV_Click;
            // 
            // txtOutput
            // 
            txtOutput.Location = new Point(20, 400);
            txtOutput.Multiline = true;
            txtOutput.Name = "txtOutput";
            txtOutput.ScrollBars = ScrollBars.Vertical;
            txtOutput.Size = new Size(830, 120);
            txtOutput.TabIndex = 5;
            // 
            // txtUPC
            // 
            txtUPC.Location = new Point(100, 540);
            txtUPC.Name = "txtUPC";
            txtUPC.Size = new Size(200, 27);
            txtUPC.TabIndex = 7;
            // 
            // txtSerial
            // 
            txtSerial.Location = new Point(430, 540);
            txtSerial.Name = "txtSerial";
            txtSerial.Size = new Size(200, 27);
            txtSerial.TabIndex = 9;
            // 
            // txtPart
            // 
            txtPart.Location = new Point(120, 580);
            txtPart.Name = "txtPart";
            txtPart.Size = new Size(250, 27);
            txtPart.TabIndex = 11;
            // 
            // lblUPC
            // 
            lblUPC.Location = new Point(20, 540);
            lblUPC.Name = "lblUPC";
            lblUPC.Size = new Size(52, 23);
            lblUPC.TabIndex = 6;
            lblUPC.Text = "UPC";
            // 
            // lblSerial
            // 
            lblSerial.Location = new Point(320, 540);
            lblSerial.Name = "lblSerial";
            lblSerial.Size = new Size(100, 23);
            lblSerial.TabIndex = 8;
            lblSerial.Text = "Serial Number";
            // 
            // lblPart
            // 
            lblPart.Location = new Point(20, 580);
            lblPart.Name = "lblPart";
            lblPart.Size = new Size(100, 23);
            lblPart.TabIndex = 10;
            lblPart.Text = "Part Number";
            // 
            // lblTime
            // 
            lblTime.Location = new Point(20, 620);
            lblTime.Name = "lblTime";
            lblTime.Size = new Size(300, 30);
            lblTime.TabIndex = 12;
            lblTime.Text = "Processing Time:";
            // 
            // Form1
            // 
            BackColor = Color.LightCyan;
            ClientSize = new Size(880, 630);
            Controls.Add(pictureBoxOriginal);
            Controls.Add(pictureBoxResult);
            Controls.Add(btnUpload);
            Controls.Add(btnOCR);
            Controls.Add(btnCV);
            Controls.Add(txtOutput);
            Controls.Add(lblUPC);
            Controls.Add(txtUPC);
            Controls.Add(lblSerial);
            Controls.Add(txtSerial);
            Controls.Add(lblPart);
            Controls.Add(txtPart);
            Controls.Add(lblTime);
            Name = "Form1";
            Text = "OCR Detector";
            ((System.ComponentModel.ISupportInitialize)pictureBoxOriginal).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxResult).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}