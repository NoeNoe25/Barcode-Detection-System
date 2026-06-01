//using OpenCvSharp;
//using OpenCvSharp.Extensions;
//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.IO;
//using System.Text.RegularExpressions;
//using System.Windows.Forms;
//using Tesseract;

//namespace WinFormsApp1
//{
//    public partial class Form1 : Form
//    {
//        Mat image;

//        public Form1()
//        {
//            InitializeComponent();
//        }

//        // -----------------------------
//        // Upload Image
//        // -----------------------------
//        private void btnUpload_Click(object sender, EventArgs e)
//        {
//            OpenFileDialog ofd = new OpenFileDialog();

//            if (ofd.ShowDialog() == DialogResult.OK)
//            {
//                image = Cv2.ImRead(ofd.FileName);
//                pictureBoxOriginal.Image = BitmapConverter.ToBitmap(image);
//            }
//        }

//        // -----------------------------
//        // Run Full Pipeline
//        // -----------------------------
//        private void btnProcess_Click(object sender, EventArgs e)
//        {
//            if (image == null) return;

//            Mat deskewed = Deskew(image);

//            // STEP 3 : grayscale
//            Mat gray = new Mat();
//            Cv2.CvtColor(deskewed, gray, ColorConversionCodes.BGR2GRAY);

//            // STEP 4 : blur
//            Mat blur = new Mat();
//            Cv2.GaussianBlur(gray, blur, new OpenCvSharp.Size(5, 5), 0);

//            // STEP 5 : threshold
//            Mat thresh = new Mat();
//            Cv2.AdaptiveThreshold(
//                blur,
//                thresh,
//                255,
//                AdaptiveThresholdTypes.GaussianC,
//                ThresholdTypes.BinaryInv,
//                11,
//                2
//            );

//            // STEP 6 : morphology
//            Mat kernel = Cv2.GetStructuringElement(
//                MorphShapes.Rect,
//                new OpenCvSharp.Size(25, 5)
//            );

//            Mat morph = new Mat();

//            Cv2.MorphologyEx(
//                thresh,
//                morph,
//                MorphTypes.Close,
//                kernel,
//                iterations: 2
//            );

//            // STEP 7 : find contours
//            OpenCvSharp.Point[][] contours;
//            HierarchyIndex[] hierarchy;

//            Cv2.FindContours(
//                morph,
//                out contours,
//                out hierarchy,
//                RetrievalModes.External,
//                ContourApproximationModes.ApproxSimple
//            );

//            Mat contourVis = deskewed.Clone();

//            string serial = "";
//            string part = "";
//            string upc = "";

//            string tessPath = Path.Combine(Application.StartupPath, "tessdata");

//            using (var engine = new TesseractEngine(tessPath, "hmn2", EngineMode.Default))
//            {
////                engine.SetVariable("tessedit_char_whitelist",
////"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789:- ");

//                foreach (var c in contours)
//                {
//                    OpenCvSharp.Rect rect = Cv2.BoundingRect(c);

//                    int x = rect.X;
//                    int y = rect.Y;
//                    int w = rect.Width;
//                    int h = rect.Height;

//                    if (w > 40 && h > 15)
//                    {
//                        // draw contour box
//                        Cv2.Rectangle(
//                            contourVis,
//                            new OpenCvSharp.Point(x, y),
//                            new OpenCvSharp.Point(x + w, y + h),
//                            Scalar.Lime,
//                            2
//                        );

//                        // crop region
//                        Mat roi = new Mat(deskewed, rect);

//                        Bitmap bmp = BitmapConverter.ToBitmap(roi);

//                        using (var ms = new MemoryStream())
//                        {
//                            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

//                            using (var pix = Pix.LoadFromMemory(ms.ToArray()))
//                            using (var page = engine.Process(pix))
//                            {
//                                string text = page.GetText();

//                                //var matches = Regex.Matches(text, @"[\d\- ]{6,}");

//                                //foreach (Match m in matches)
//                                //{
//                                //    string clean = Regex.Replace(m.Value, @"[^\d]", "");

//                                //    if (clean.Length >= 12)
//                                //        upc = clean;
//                                //    else if (serial == "")
//                                //        serial = clean;
//                                //    else
//                                //        part = clean;
//                                //}
//                                string lower = text.ToLower();

//                                // Detect SERIAL
//                                if (lower.Contains("Serial") || lower.Contains("s/n"))
//                                {
//                                    Match m = Regex.Match(text, @"\d{13}");
//                                    if (m.Success)
//                                        serial = m.Value;
//                                }

//                                // Detect PART
//                                if (lower.Contains("Part") || lower.Contains("p/n"))
//                                {
//                                    Match m = Regex.Match(text, @"\d+-\d+-\d+-\d+");
//                                    if (m.Success)
//                                        part = m.Value;
//                                }

//                                // Detect UPC
//                                if (lower.Contains("upc"))
//                                {
//                                    upc = "UPC";
//                                }
//                            }
//                        }
//                    }
//                }
//            }

//            // show results
//            txtUPC.Text = upc;
//            txtSerial.Text = serial;
//            txtPart.Text = part;

//            pictureBoxResult.Image = BitmapConverter.ToBitmap(contourVis);
//        }

//        // -----------------------------
//        // Deskew Image (no cropping)
//        // -----------------------------
//        Mat Deskew(Mat image)
//        {
//            Mat gray = new Mat();
//            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);

//            Mat edges = new Mat();
//            Cv2.Canny(gray, edges, 50, 150);

//            LineSegmentPolar[] lines = Cv2.HoughLines(
//                edges,
//                1,
//                Math.PI / 180,
//                200
//            );

//            if (lines.Length == 0)
//                return image;

//            List<double> angles = new List<double>();

//            foreach (var line in lines)
//            {
//                double angle = (line.Theta * 180 / Math.PI) - 90;
//                angles.Add(angle);
//            }

//            angles.Sort();
//            double angleMedian = angles[angles.Count / 2];

//            int w = image.Width;
//            int h = image.Height;

//            double centerX = w / 2.0;
//            double centerY = h / 2.0;

//            Mat M = Cv2.GetRotationMatrix2D(
//                new OpenCvSharp.Point2f((float)centerX, (float)centerY),
//                angleMedian,
//                1.0
//            );

//            double cos = Math.Abs(M.At<double>(0, 0));
//            double sin = Math.Abs(M.At<double>(0, 1));

//            int newW = (int)((h * sin) + (w * cos));
//            int newH = (int)((h * cos) + (w * sin));

//            // adjust translation
//            M.Set(0, 2, M.At<double>(0, 2) + (newW / 2.0 - centerX));
//            M.Set(1, 2, M.At<double>(1, 2) + (newH / 2.0 - centerY));

//            Mat rotated = new Mat();

//            Cv2.WarpAffine(
//                image,
//                rotated,
//                M,
//                new OpenCvSharp.Size(newW, newH),
//                InterpolationFlags.Cubic,
//                BorderTypes.Replicate
//            );

//            return rotated;
//        }
//    }
//}

//using OpenCvSharp;
//using OpenCvSharp.Extensions;
//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.IO;
//using System.Text.RegularExpressions;
//using System.Windows.Forms;
//using Tesseract;
//using ZXing;
//using ZXing.Common;
//using System.Drawing;
//using ZXing.Windows.Compatibility;

//namespace WinFormsApp1
//{
//    public partial class Form1 : Form
//    {
//        Mat image;

//        public Form1()
//        {
//            InitializeComponent();
//        }

//        // Upload Image
//        private void btnUpload_Click(object sender, EventArgs e)
//        {
//            OpenFileDialog ofd = new OpenFileDialog();

//            if (ofd.ShowDialog() == DialogResult.OK)
//            {
//                image = Cv2.ImRead(ofd.FileName);
//                pictureBoxOriginal.Image = BitmapConverter.ToBitmap(image);
//            }
//        }

//        // Main Processing
//        private void btnProcess_Click(object sender, EventArgs e)
//        {
//            if (image == null) return;

//            Mat deskewed = Deskew(image);

//            // -----------------------------
//            // Detect Barcode
//            // -----------------------------
//            OpenCvSharp.Rect barcodeRect = DetectBarcode(deskewed);

//            Mat workingArea = deskewed;

//            if (barcodeRect.Width > 0)
//            {
//                int cropY = barcodeRect.Y + barcodeRect.Height;

//                OpenCvSharp.Rect textRegion =
//                    new OpenCvSharp.Rect(
//                        0,
//                        cropY,
//                        deskewed.Width,
//                        deskewed.Height - cropY
//                    );

//                workingArea = new Mat(deskewed, textRegion);
//            }

//            // -----------------------------
//            // Image preprocessing
//            // -----------------------------
//            Mat gray = new Mat();
//            Cv2.CvtColor(workingArea, gray, ColorConversionCodes.BGR2GRAY);

//            Mat blur = new Mat();
//            Cv2.GaussianBlur(gray, blur, new OpenCvSharp.Size(5, 5), 0);

//            Mat thresh = new Mat();
//            Cv2.AdaptiveThreshold(
//                blur,
//                thresh,
//                255,
//                AdaptiveThresholdTypes.GaussianC,
//                ThresholdTypes.BinaryInv,
//                11,
//                2
//            );

//            Mat kernel = Cv2.GetStructuringElement(
//                MorphShapes.Rect,
//                new OpenCvSharp.Size(25, 5)
//            );

//            Mat morph = new Mat();

//            Cv2.MorphologyEx(
//                thresh,
//                morph,
//                MorphTypes.Close,
//                kernel,
//                iterations: 2
//            );

//            // -----------------------------
//            // Find contours
//            // -----------------------------
//            OpenCvSharp.Point[][] contours;
//            HierarchyIndex[] hierarchy;

//            Cv2.FindContours(
//                morph,
//                out contours,
//                out hierarchy,
//                RetrievalModes.External,
//                ContourApproximationModes.ApproxSimple
//            );

//            Mat contourVis = deskewed.Clone();

//            string serial = "";
//            string part = "";
//            string upc = "";

//            string tessPath = Path.Combine(Application.StartupPath, "tessdata");

//            using (var engine = new TesseractEngine(tessPath, "eng", EngineMode.Default))
//            {
//                engine.SetVariable(
//                "tessedit_char_whitelist",
//                "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789:- ");

//                foreach (var c in contours)
//                {
//                    OpenCvSharp.Rect rect = Cv2.BoundingRect(c);

//                    int x = rect.X;
//                    int y = rect.Y;
//                    int w = rect.Width;
//                    int h = rect.Height;

//                    if (w > 40 && h > 15)
//                    {
//                        Cv2.Rectangle(
//                            contourVis,
//                            new OpenCvSharp.Point(x, y),
//                            new OpenCvSharp.Point(x + w, y + h),
//                            Scalar.Lime,
//                            2
//                        );

//                        Mat roi = new Mat(workingArea, rect);

//                        Bitmap bmp = BitmapConverter.ToBitmap(roi);

//                        using (var ms = new MemoryStream())
//                        {
//                            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

//                            using (var pix = Pix.LoadFromMemory(ms.ToArray()))
//                            using (var page = engine.Process(pix))
//                            {
//                                string text = page.GetText().ToLower();

//                                if (text.Contains("serial"))
//                                {
//                                    Match m = Regex.Match(text, @"\d{13}");
//                                    if (m.Success)
//                                        serial = m.Value;
//                                }

//                                if (text.Contains("part"))
//                                {
//                                    Match m = Regex.Match(text, @"\d+-\d+-\d+-\d+");
//                                    if (m.Success)
//                                        part = m.Value;
//                                }

//                                if (text.Contains("upc"))
//                                {
//                                    upc = "UPC";
//                                }
//                            }
//                        }
//                    }
//                }
//            }

//            txtUPC.Text = upc;
//            txtSerial.Text = serial;
//            txtPart.Text = part;

//            pictureBoxResult.Image = BitmapConverter.ToBitmap(contourVis);
//        }

//        // -----------------------------
//        // Barcode Detection
//        // -----------------------------
//        OpenCvSharp.Rect DetectBarcode(OpenCvSharp.Mat image)
//        {
//            Bitmap bmp = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(image);

//            var reader = new BarcodeReader<Bitmap>(
//    bitmap => new BitmapLuminanceSource(bitmap)
//)
//            {
//                AutoRotate = true,
//                Options = new DecodingOptions
//                {
//                    TryHarder = true
//                }
//            };

//            var result = reader.Decode(bmp);

//            if (result != null && result.ResultPoints != null && result.ResultPoints.Length > 0)
//            {
//                float minX = float.MaxValue;
//                float minY = float.MaxValue;
//                float maxX = 0;
//                float maxY = 0;

//                foreach (var p in result.ResultPoints)
//                {
//                    minX = Math.Min(minX, p.X);
//                    minY = Math.Min(minY, p.Y);
//                    maxX = Math.Max(maxX, p.X);
//                    maxY = Math.Max(maxY, p.Y);
//                }

//                return new OpenCvSharp.Rect(
//                    (int)minX,
//                    (int)minY,
//                    (int)(maxX - minX),
//                    (int)(maxY - minY)
//                );
//            }

//            return new OpenCvSharp.Rect();
//        }
//        // Deskew
//        // -----------------------------
//        Mat Deskew(Mat image)
//        {
//            Mat gray = new Mat();
//            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);

//            Mat edges = new Mat();
//            Cv2.Canny(gray, edges, 50, 150);

//            LineSegmentPolar[] lines = Cv2.HoughLines(
//                edges,
//                1,
//                Math.PI / 180,
//                200
//            );

//            if (lines.Length == 0)
//                return image;

//            List<double> angles = new List<double>();

//            foreach (var line in lines)
//            {
//                double angle = (line.Theta * 180 / Math.PI) - 90;
//                angles.Add(angle);
//            }

//            angles.Sort();
//            double angleMedian = angles[angles.Count / 2];

//            int w = image.Width;
//            int h = image.Height;

//            double centerX = w / 2.0;
//            double centerY = h / 2.0;

//            Mat M = Cv2.GetRotationMatrix2D(
//                new OpenCvSharp.Point2f((float)centerX, (float)centerY),
//                angleMedian,
//                1.0
//            );

//            Mat rotated = new Mat();

//            Cv2.WarpAffine(
//                image,
//                rotated,
//                M,
//                new OpenCvSharp.Size(w, h),
//                InterpolationFlags.Cubic,
//                BorderTypes.Replicate
//            );

//            return rotated;
//        }
//    }
//}

//using OpenCvSharp;
//using OpenCvSharp.Extensions;
//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.IO;
//using System.Net.Http;
//using System.Text;
//using System.Text.Json;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;
//using System.Windows.Forms;
//using Tesseract;

//namespace WinFormsApp1
//{
//    public partial class Form1 : Form
//    {
//        Mat image;
//        private readonly HttpClient httpClient = new HttpClient();

//        public Form1()
//        {
//            InitializeComponent();
//        }

//        // -----------------------------
//        // Upload Image
//        // -----------------------------
//        private void btnUpload_Click(object sender, EventArgs e)
//        {
//            OpenFileDialog ofd = new OpenFileDialog();

//            if (ofd.ShowDialog() == DialogResult.OK)
//            {
//                image = Cv2.ImRead(ofd.FileName);
//                pictureBoxOriginal.Image = BitmapConverter.ToBitmap(image);
//            }
//        }

//        // -----------------------------
//        // Run Full Pipeline with AI OCR
//        // -----------------------------
//        private async void btnProcess_Click(object sender, EventArgs e)
//        {
//            if (image == null) return;

//            Mat deskewed = Deskew(image);

//            // STEP 3 : grayscale
//            Mat gray = new Mat();
//            Cv2.CvtColor(deskewed, gray, ColorConversionCodes.BGR2GRAY);

//            // STEP 4 : blur
//            Mat blur = new Mat();
//            Cv2.GaussianBlur(gray, blur, new OpenCvSharp.Size(5, 5), 0);

//            // STEP 5 : threshold
//            Mat thresh = new Mat();
//            Cv2.AdaptiveThreshold(
//                blur,
//                thresh,
//                255,
//                AdaptiveThresholdTypes.GaussianC,
//                ThresholdTypes.BinaryInv,
//                11,
//                2
//            );

//            // STEP 6 : morphology
//            Mat kernel = Cv2.GetStructuringElement(
//                MorphShapes.Rect,
//                new OpenCvSharp.Size(25, 5)
//            );

//            Mat morph = new Mat();
//            Cv2.MorphologyEx(
//                thresh,
//                morph,
//                MorphTypes.Close,
//                kernel,
//                iterations: 2
//            );

//            // STEP 7 : find contours
//            OpenCvSharp.Point[][] contours;
//            HierarchyIndex[] hierarchy;

//            Cv2.FindContours(
//                morph,
//                out contours,
//                out hierarchy,
//                RetrievalModes.External,
//                ContourApproximationModes.ApproxSimple
//            );

//            Mat contourVis = deskewed.Clone();

//            string serial = "";
//            string part = "";
//            string upc = "";

//            // Collect all ROIs for AI processing
//            List<(OpenCvSharp.Rect rect, Bitmap bitmap)> rois = new List<(OpenCvSharp.Rect, Bitmap)>();

//            foreach (var c in contours)
//            {
//                OpenCvSharp.Rect rect = Cv2.BoundingRect(c);

//                int x = rect.X;
//                int y = rect.Y;
//                int w = rect.Width;
//                int h = rect.Height;

//                if (w > 40 && h > 15)
//                {
//                    // draw contour box
//                    Cv2.Rectangle(
//                        contourVis,
//                        new OpenCvSharp.Point(x, y),
//                        new OpenCvSharp.Point(x + w, y + h),
//                        Scalar.Lime,
//                        2
//                    );

//                    // crop region
//                    Mat roi = new Mat(deskewed, rect);
//                    Bitmap bmp = BitmapConverter.ToBitmap(roi);
//                    rois.Add((rect, bmp));
//                }
//            }

//            // Process all ROIs with AI OCR
//            var aiResults = await ProcessWithAIOCR(rois);

//            // Fallback to Tesseract for any failed regions
//            string tessPath = Path.Combine(Application.StartupPath, "tessdata");
//            using (var engine = new TesseractEngine(tessPath, "hmn_3.168_500_5000", EngineMode.Default))
//            {
//                engine.SetVariable("tessedit_char_whitelist",
//                    "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789:- ");

//                foreach (var result in aiResults)
//                {
//                    string text = result.Text;

//                    // If AI OCR failed or returned empty, try Tesseract
//                    if (string.IsNullOrWhiteSpace(text))
//                    {
//                        using (var ms = new MemoryStream())
//                        {
//                            result.Bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
//                            using (var pix = Pix.LoadFromMemory(ms.ToArray()))
//                            using (var page = engine.Process(pix))
//                            {
//                                text = page.GetText();
//                            }
//                        }
//                    }

//                    // Extract information from the text
//                    ExtractInformation(text, ref serial, ref part, ref upc);
//                }
//            }

//            // show results
//            txtUPC.Text = upc;
//            txtSerial.Text = serial;
//            txtPart.Text = part;

//            pictureBoxResult.Image = BitmapConverter.ToBitmap(contourVis);
//        }

//        private void ExtractInformation(string text, ref string serial, ref string part, ref string upc)
//        {
//            string lower = text.ToLower();

//            // Detect SERIAL
//            if (lower.Contains("serial") || lower.Contains("s/n") || lower.Contains("sn:"))
//            {
//                // Try multiple serial number patterns
//                Match m = Regex.Match(text, @"\d{13}"); // 13-digit
//                if (!m.Success)
//                    m = Regex.Match(text, @"[A-Z0-9]{8,15}"); // Alphanumeric serial
//                if (!m.Success)
//                    m = Regex.Match(text, @"\d{5,10}"); // 5-10 digit

//                if (m.Success && string.IsNullOrEmpty(serial))
//                    serial = m.Value;
//            }

//            // Detect PART
//            if (lower.Contains("part") || lower.Contains("p/n") || lower.Contains("pn:"))
//            {
//                // Try multiple part number patterns
//                Match m = Regex.Match(text, @"\d+-\d+-\d+-\d+"); // 4-part number
//                if (!m.Success)
//                    m = Regex.Match(text, @"[A-Z0-9]+-[A-Z0-9]+"); // Alphanumeric with hyphen
//                if (!m.Success)
//                    m = Regex.Match(text, @"[A-Z]{2,5}\d{5,10}"); // Letters followed by numbers

//                if (m.Success && string.IsNullOrEmpty(part))
//                    part = m.Value;
//            }

//            // Detect UPC
//            if (lower.Contains("upc") || lower.Contains("upc:"))
//            {
//                // Extract UPC code
//                Match m = Regex.Match(text, @"\d{12,13}"); // 12-13 digit UPC
//                if (m.Success)
//                    upc = m.Value;
//                else
//                    upc = "UPC detected"; // Mark as detected but no code found
//            }
//        }

//        private async Task<List<AIResult>> ProcessWithAIOCR(List<(OpenCvSharp.Rect rect, Bitmap bitmap)> rois)
//        {
//            var results = new List<AIResult>();

//            foreach (var roi in rois)
//            {
//                try
//                {
//                    // Convert bitmap to base64
//                    string base64Image = ConvertBitmapToBase64(roi.bitmap);

//                    // Send to AI service (adjust the endpoint and payload based on your AI service)
//                    var aiText = await CallAIService(base64Image);

//                    results.Add(new AIResult
//                    {
//                        Rect = roi.rect,
//                        Bitmap = roi.bitmap,
//                        Text = aiText
//                    });
//                }
//                catch (Exception ex)
//                {
//                    // If AI fails, return empty text to fall back to Tesseract
//                    results.Add(new AIResult
//                    {
//                        Rect = roi.rect,
//                        Bitmap = roi.bitmap,
//                        Text = ""
//                    });

//                    // Log error if needed
//                    Console.WriteLine($"AI OCR failed: {ex.Message}");
//                }
//            }

//            return results;
//        }

//        private string ConvertBitmapToBase64(Bitmap bitmap)
//        {
//            using (MemoryStream ms = new MemoryStream())
//            {
//                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
//                byte[] imageBytes = ms.ToArray();
//                return Convert.ToBase64String(imageBytes);
//            }
//        }

//        private async Task<string> CallAIService(string base64Image)
//        {
//            // Example for Azure Computer Vision
//            // Replace with your actual AI service endpoint and key
//            var payload = new
//            {
//                image = base64Image,
//                language = "en",
//                detectOrientation = true
//            };

//            string jsonPayload = JsonSerializer.Serialize(payload);
//            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

//            // Add your API key and endpoint
//            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "YOUR_API_KEY");

//            var response = await httpClient.PostAsync("YOUR_AI_ENDPOINT", content);

//            if (response.IsSuccessStatusCode)
//            {
//                string responseBody = await response.Content.ReadAsStringAsync();

//                // Parse response based on your AI service format
//                // This is an example for Azure Computer Vision
//                using JsonDocument doc = JsonDocument.Parse(responseBody);
//                if (doc.RootElement.TryGetProperty("text", out JsonElement textElement))
//                {
//                    return textElement.GetString();
//                }
//                else if (doc.RootElement.TryGetProperty("description", out JsonElement descElement))
//                {
//                    return descElement.GetString();
//                }

//                return "";
//            }

//            return "";
//        }

//        // -----------------------------
//        // Deskew Image (no cropping)
//        // -----------------------------
//        Mat Deskew(Mat image)
//        {
//            Mat gray = new Mat();
//            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);

//            Mat edges = new Mat();
//            Cv2.Canny(gray, edges, 50, 150);

//            LineSegmentPolar[] lines = Cv2.HoughLines(
//                edges,
//                1,
//                Math.PI / 180,
//                200
//            );

//            if (lines.Length == 0)
//                return image;

//            List<double> angles = new List<double>();

//            foreach (var line in lines)
//            {
//                double angle = (line.Theta * 180 / Math.PI) - 90;
//                angles.Add(angle);
//            }

//            angles.Sort();
//            double angleMedian = angles[angles.Count / 2];

//            int w = image.Width;
//            int h = image.Height;

//            double centerX = w / 2.0;
//            double centerY = h / 2.0;

//            Mat M = Cv2.GetRotationMatrix2D(
//                new OpenCvSharp.Point2f((float)centerX, (float)centerY),
//                angleMedian,
//                1.0
//            );

//            double cos = Math.Abs(M.At<double>(0, 0));
//            double sin = Math.Abs(M.At<double>(0, 1));

//            int newW = (int)((h * sin) + (w * cos));
//            int newH = (int)((h * cos) + (w * sin));

//            // adjust translation
//            M.Set(0, 2, M.At<double>(0, 2) + (newW / 2.0 - centerX));
//            M.Set(1, 2, M.At<double>(1, 2) + (newH / 2.0 - centerY));

//            Mat rotated = new Mat();

//            Cv2.WarpAffine(
//                image,
//                rotated,
//                M,
//                new OpenCvSharp.Size(newW, newH),
//                InterpolationFlags.Cubic,
//                BorderTypes.Replicate
//            );

//            return rotated;
//        }
//    }

//    // Helper class for AI results
//    public class AIResult
//    {
//        public OpenCvSharp.Rect Rect { get; set; }
//        public Bitmap Bitmap { get; set; }
//        public string Text { get; set; }
//    }
//}

//using System;
//using System.Drawing;
//using System.IO;
//using System.Windows.Forms;
//using Tesseract;

//namespace WinFormsApp1
//{
//    public partial class Form1 : Form
//    {
//        public Form1()
//        {
//            InitializeComponent();
//        }

//        private void btnTest_Click(object sender, EventArgs e)
//        {
//            OpenFileDialog ofd = new OpenFileDialog();
//            ofd.Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp";

//            if (ofd.ShowDialog() == DialogResult.OK)
//            {
//                pictureBox1.Image = Image.FromFile(ofd.FileName);

//                string tessPath = Path.Combine(Application.StartupPath, "tessdata");

//                using (var engine = new TesseractEngine(tessPath, "hmn2", EngineMode.Default))
//                {


//                    using (var img = Pix.LoadFromFile(ofd.FileName))
//                    using (var page = engine.Process(img))
//                    {
//                        string text = page.GetText();

//                        txtResult.Text = text;

//                        MessageBox.Show("Confidence: " + page.GetMeanConfidence());
//                    }
//                }
//            }
//        }
//    }
//}


//using OpenCvSharp;
//using OpenCvSharp.Extensions;
//using System;
//using System.Drawing;
//using System.IO;
//using System.Windows.Forms;
//using Tesseract;

//namespace WinFormsApp1
//{
//    public partial class Form1 : Form
//    {
//        Mat image;

//        public Form1()
//        {
//            InitializeComponent();
//        }

//        // =========================
//        // Upload Image
//        // =========================
//        private void btnUpload_Click(object sender, EventArgs e)
//        {
//            OpenFileDialog ofd = new OpenFileDialog();

//            if (ofd.ShowDialog() == DialogResult.OK)
//            {
//                image = Cv2.ImRead(ofd.FileName);
//                pictureBoxOriginal.Image = BitmapConverter.ToBitmap(image);
//            }
//        }

//        // =========================
//        // Direct OCR (NO CV)
//        // =========================
//        private void btnOCR_Click(object sender, EventArgs e)
//        {
//            if (image == null) return;

//            Bitmap bmp = BitmapConverter.ToBitmap(image);

//            string tessPath = Path.Combine(Application.StartupPath, "tessdata");

//            using (var engine = new TesseractEngine(tessPath, "hmn2", EngineMode.Default))
//            {
//                using (var ms = new MemoryStream())
//                {
//                    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

//                    using (var pix = Pix.LoadFromMemory(ms.ToArray()))
//                    using (var page = engine.Process(pix))
//                    {
//                        string text = page.GetText();

//                        txtOutput.Text = text;
//                    }
//                }
//            }

//            pictureBoxResult.Image = bmp;
//        }

//        // =========================
//        // CV + OCR Pipeline
//        // =========================
//        private void btnCV_Click(object sender, EventArgs e)
//        {
//            if (image == null) return;

//            Mat deskewed = Deskew(image);

//            Mat gray = new Mat();
//            Cv2.CvtColor(deskewed, gray, ColorConversionCodes.BGR2GRAY);

//            Mat blur = new Mat();
//            Cv2.GaussianBlur(gray, blur, new OpenCvSharp.Size(5, 5), 0);

//            Mat thresh = new Mat();
//            Cv2.AdaptiveThreshold(
//                blur,
//                thresh,
//                255,
//                AdaptiveThresholdTypes.GaussianC,
//                ThresholdTypes.BinaryInv,
//                11,
//                2
//            );

//            Mat kernel = Cv2.GetStructuringElement(
//                MorphShapes.Rect,
//                new OpenCvSharp.Size(25, 5)
//            );

//            Mat morph = new Mat();

//            Cv2.MorphologyEx(
//                thresh,
//                morph,
//                MorphTypes.Close,
//                kernel,
//                iterations: 2
//            );

//            OpenCvSharp.Point[][] contours;
//            HierarchyIndex[] hierarchy;

//            Cv2.FindContours(
//                morph,
//                out contours,
//                out hierarchy,
//                RetrievalModes.External,
//                ContourApproximationModes.ApproxSimple
//            );

//            Mat contourVis = deskewed.Clone();

//            string resultText = "";

//            string tessPath = Path.Combine(Application.StartupPath, "tessdata");

//            using (var engine = new TesseractEngine(tessPath, "hmn2", EngineMode.Default))
//            {
//                foreach (var c in contours)
//                {
//                    OpenCvSharp.Rect rect = Cv2.BoundingRect(c);

//                    int x = rect.X;
//                    int y = rect.Y;
//                    int w = rect.Width;
//                    int h = rect.Height;

//                    if (w > 40 && h > 15)
//                    {
//                        Cv2.Rectangle(
//                            contourVis,
//                            new OpenCvSharp.Point(x, y),
//                            new OpenCvSharp.Point(x + w, y + h),
//                            Scalar.Lime,
//                            2
//                        );

//                        Mat roi = new Mat(deskewed, rect);

//                        Bitmap bmp = BitmapConverter.ToBitmap(roi);

//                        using (var ms = new MemoryStream())
//                        {
//                            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

//                            using (var pix = Pix.LoadFromMemory(ms.ToArray()))
//                            using (var page = engine.Process(pix))
//                            {
//                                string text = page.GetText();

//                                resultText += text + Environment.NewLine;
//                            }
//                        }
//                    }
//                }
//            }

//            txtOutput.Text = resultText;

//            pictureBoxResult.Image = BitmapConverter.ToBitmap(contourVis);
//        }

//        // =========================
//        // Deskew
//        // =========================
//        Mat Deskew(Mat image)
//        {
//            Mat gray = new Mat();
//            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);

//            Mat edges = new Mat();
//            Cv2.Canny(gray, edges, 50, 150);

//            LineSegmentPolar[] lines = Cv2.HoughLines(
//                edges,
//                1,
//                Math.PI / 180,
//                200
//            );

//            if (lines.Length == 0)
//                return image;

//            double angle = (lines[0].Theta * 180 / Math.PI) - 90;

//            int w = image.Width;
//            int h = image.Height;

//            Mat M = Cv2.GetRotationMatrix2D(
//                new OpenCvSharp.Point2f(w / 2, h / 2),
//                angle,
//                1.0
//            );

//            Mat rotated = new Mat();

//            Cv2.WarpAffine(
//                image,
//                rotated,
//                M,
//                new OpenCvSharp.Size(w, h),
//                InterpolationFlags.Cubic,
//                BorderTypes.Replicate
//            );

//            return rotated;
//        }
//    }
//}


//using OpenCvSharp;
//using OpenCvSharp.Extensions;
//using System;
//using System.Drawing;
//using System.IO;
//using System.Text.RegularExpressions;
//using System.Windows.Forms;
//using Tesseract;

//namespace WinFormsApp1
//{
//    public partial class Form1 : Form
//    {
//        Mat image;

//        public Form1()
//        {
//            InitializeComponent();
//        }

//        // Upload Image
//        private void btnUpload_Click(object sender, EventArgs e)
//        {
//            OpenFileDialog ofd = new OpenFileDialog();

//            if (ofd.ShowDialog() == DialogResult.OK)
//            {
//                image = Cv2.ImRead(ofd.FileName);
//                pictureBoxOriginal.Image = BitmapConverter.ToBitmap(image);
//            }
//        }

//        // DIRECT OCR (NO CV)
//        private void btnOCR_Click(object sender, EventArgs e)
//        {
//            if (image == null) return;

//            string tessPath = Path.Combine(Application.StartupPath, "tessdata");

//            Bitmap bmp = BitmapConverter.ToBitmap(image);

//            using (var engine = new TesseractEngine(tessPath, "hmn2", EngineMode.Default))
//            using (var ms = new MemoryStream())
//            {
//                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

//                using (var pix = Pix.LoadFromMemory(ms.ToArray()))
//                using (var page = engine.Process(pix))
//                {
//                    string text = page.GetText();

//                    txtOutput.Text = text;

//                    ExtractFields(text);
//                }
//            }

//            pictureBoxResult.Image = bmp;
//        }

//        // CV + OCR
//        private void btnCV_Click(object sender, EventArgs e)
//        {
//            if (image == null) return;

//            Mat deskewed = Deskew(image);

//            Mat gray = new Mat();
//            Cv2.CvtColor(deskewed, gray, ColorConversionCodes.BGR2GRAY);

//            Mat blur = new Mat();
//            Cv2.GaussianBlur(gray, blur, new OpenCvSharp.Size(5, 5), 0);

//            Mat thresh = new Mat();
//            Cv2.AdaptiveThreshold(
//                blur,
//                thresh,
//                255,
//                AdaptiveThresholdTypes.GaussianC,
//                ThresholdTypes.BinaryInv,
//                11,
//                2);

//            Mat kernel = Cv2.GetStructuringElement(
//                MorphShapes.Rect,
//                new OpenCvSharp.Size(25, 5));

//            Mat morph = new Mat();

//            Cv2.MorphologyEx(
//                thresh,
//                morph,
//                MorphTypes.Close,
//                kernel,
//                iterations: 2);

//            OpenCvSharp.Point[][] contours;
//            HierarchyIndex[] hierarchy;

//            Cv2.FindContours(
//                morph,
//                out contours,
//                out hierarchy,
//                RetrievalModes.External,
//                ContourApproximationModes.ApproxSimple);

//            Mat contourVis = deskewed.Clone();

//            string resultText = "";

//            string tessPath = Path.Combine(Application.StartupPath, "tessdata");

//            using (var engine = new TesseractEngine(tessPath, "hmn2", EngineMode.Default))
//            {
//                foreach (var c in contours)
//                {
//                    OpenCvSharp.Rect rect = Cv2.BoundingRect(c);

//                    if (rect.Width > 40 && rect.Height > 15)
//                    {
//                        Cv2.Rectangle(
//                            contourVis,
//                            new OpenCvSharp.Point(rect.X, rect.Y),
//                            new OpenCvSharp.Point(rect.X + rect.Width, rect.Y + rect.Height),
//                            Scalar.Lime,
//                            2);

//                        Mat roi = new Mat(deskewed, rect);

//                        Bitmap bmp = BitmapConverter.ToBitmap(roi);

//                        using (var ms = new MemoryStream())
//                        {
//                            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

//                            using (var pix = Pix.LoadFromMemory(ms.ToArray()))
//                            using (var page = engine.Process(pix))
//                            {
//                                string text = page.GetText();
//                                resultText += text + Environment.NewLine;
//                            }
//                        }
//                    }
//                }
//            }

//            txtOutput.Text = resultText;

//            ExtractFields(resultText);

//            pictureBoxResult.Image = BitmapConverter.ToBitmap(contourVis);
//        }

//        // FIELD FILTERING
//        void ExtractFields(string text)
//        {
//            txtUPC.Text = "";
//            txtSerial.Text = "";
//            txtPart.Text = "";

//            // SERIAL NUMBER (13 digits)
//            Match serial = Regex.Match(text, @"\d{13,15}");
//            if (serial.Success)
//                txtSerial.Text = serial.Value;

//            // PART NUMBER (xxxx-xxxxx-xxxx-xxx)
//            Match part = Regex.Match(text, @"\d{3,4}-\d{4,6}-\d{4}-\d{3}");
//            if (part.Success)
//                txtPart.Text = part.Value;

//            // UPC
//            if (text.ToLower().Contains("upc"))
//                txtUPC.Text = "UPC";
//        }

//        // DESKEW IMAGE
//        Mat Deskew(Mat img)
//        {
//            Mat gray = new Mat();
//            Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);

//            Mat edges = new Mat();
//            Cv2.Canny(gray, edges, 50, 150);

//            LineSegmentPolar[] lines = Cv2.HoughLines(edges, 1, Math.PI / 180, 200);

//            if (lines.Length == 0)
//                return img;

//            double angle = (lines[0].Theta * 180 / Math.PI) - 90;

//            Mat M = Cv2.GetRotationMatrix2D(
//                new OpenCvSharp.Point2f(img.Width / 2, img.Height / 2),
//                angle,
//                1);

//            Mat rotated = new Mat();

//            Cv2.WarpAffine(
//                img,
//                rotated,
//                M,
//                new OpenCvSharp.Size(img.Width, img.Height),
//                InterpolationFlags.Cubic,
//                BorderTypes.Replicate);

//            return rotated;
//        }
//    }
//}

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