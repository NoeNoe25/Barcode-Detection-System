using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Tesseract;
using System.Diagnostics;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        Mat image;

        Stopwatch timer = new Stopwatch();

        public Form1()
        {
            InitializeComponent();
        }

        // Upload Image
        private void btnUpload_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                image = Cv2.ImRead(ofd.FileName);
                pictureBoxOriginal.Image = BitmapConverter.ToBitmap(image);
            }
        }

        // DIRECT OCR (NO CV)
        private void btnOCR_Click(object sender, EventArgs e)
        {
            if (image == null) return;

            timer.Restart();

            string tessPath = Path.Combine(Application.StartupPath, "tessdata");

            Bitmap bmp = BitmapConverter.ToBitmap(image);

            using (var engine = new TesseractEngine(tessPath, "hmn2", EngineMode.Default))
            using (var ms = new MemoryStream())
            {
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

                using (var pix = Pix.LoadFromMemory(ms.ToArray()))
                using (var page = engine.Process(pix))
                {
                    string text = page.GetText();

                    txtOutput.Text = text;

                    ExtractFields(text);
                }
            }

            pictureBoxResult.Image = bmp;

            timer.Stop();
            lblTime.Text = "Processing Time: " + timer.ElapsedMilliseconds + " ms";
        }

        // CV + OCR
        private void btnCV_Click(object sender, EventArgs e)
        {
            if (image == null) return;

            timer.Restart();

            Mat deskewed = Deskew(image);

            Mat gray = new Mat();
            Cv2.CvtColor(deskewed, gray, ColorConversionCodes.BGR2GRAY);

            Mat blur = new Mat();
            Cv2.GaussianBlur(gray, blur, new OpenCvSharp.Size(5, 5), 0);

            Mat thresh = new Mat();
            Cv2.AdaptiveThreshold(
                blur,
                thresh,
                255,
                AdaptiveThresholdTypes.GaussianC,
                ThresholdTypes.BinaryInv,
                11,
                2);

            Mat kernel = Cv2.GetStructuringElement(
                MorphShapes.Rect,
                new OpenCvSharp.Size(25, 5));

            Mat morph = new Mat();

            Cv2.MorphologyEx(
                thresh,
                morph,
                MorphTypes.Close,
                kernel,
                iterations: 2);

            OpenCvSharp.Point[][] contours;
            HierarchyIndex[] hierarchy;

            Cv2.FindContours(
                morph,
                out contours,
                out hierarchy,
                RetrievalModes.External,
                ContourApproximationModes.ApproxSimple);

            Mat contourVis = deskewed.Clone();

            string resultText = "";

            string tessPath = Path.Combine(Application.StartupPath, "tessdata");

            using (var engine = new TesseractEngine(tessPath, "hmn2", EngineMode.Default))
            {
                foreach (var c in contours)
                {
                    OpenCvSharp.Rect rect = Cv2.BoundingRect(c);

                    if (rect.Width > 40 && rect.Height > 15)
                    {
                        Cv2.Rectangle(
                            contourVis,
                            new OpenCvSharp.Point(rect.X, rect.Y),
                            new OpenCvSharp.Point(rect.X + rect.Width, rect.Y + rect.Height),
                            Scalar.Lime,
                            2);

                        Mat roi = new Mat(deskewed, rect);

                        Bitmap bmp = BitmapConverter.ToBitmap(roi);

                        using (var ms = new MemoryStream())
                        {
                            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

                            using (var pix = Pix.LoadFromMemory(ms.ToArray()))
                            using (var page = engine.Process(pix))
                            {
                                string text = page.GetText();
                                resultText += text + Environment.NewLine;
                            }
                        }
                    }
                }
            }

            txtOutput.Text = resultText;

            ExtractFields(resultText);

            pictureBoxResult.Image = BitmapConverter.ToBitmap(contourVis);

            timer.Stop();
            lblTime.Text = "Processing Time: " + timer.ElapsedMilliseconds + " ms";
        }

        // FIELD FILTERING
        void ExtractFields(string text)
        {
            txtUPC.Text = "";
            txtSerial.Text = "";
            txtPart.Text = "";

            Match serial = Regex.Match(text, @"\d{13,15}");
            if (serial.Success)
                txtSerial.Text = serial.Value;

            Match part = Regex.Match(text, @"\d{3,4}-\d{4,6}-\d{4}-\d{3}");
            if (part.Success)
                txtPart.Text = part.Value;

            if (text.ToLower().Contains("upc"))
                txtUPC.Text = "UPC";
        }

        // DESKEW IMAGE
        Mat Deskew(Mat img)
        {
            Mat gray = new Mat();
            Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);

            Mat edges = new Mat();
            Cv2.Canny(gray, edges, 50, 150);

            LineSegmentPolar[] lines = Cv2.HoughLines(edges, 1, Math.PI / 180, 200);

            if (lines.Length == 0)
                return img;

            double angle = (lines[0].Theta * 180 / Math.PI) - 90;

            Mat M = Cv2.GetRotationMatrix2D(
                new OpenCvSharp.Point2f(img.Width / 2, img.Height / 2),
                angle,
                1);

            Mat rotated = new Mat();

            Cv2.WarpAffine(
                img,
                rotated,
                M,
                new OpenCvSharp.Size(img.Width, img.Height),
                InterpolationFlags.Cubic,
                BorderTypes.Replicate);

            return rotated;
        }
    }
}