# Jetson Nano Box Barcode Detection System

A computer vision-based Windows Forms application for detecting and reading barcodes (UPC, Serial Number, and Part Number) from NVIDIA Jetson Nano packaging boxes.

The system is designed to handle different conditions such as:
- Zoomed-in and zoomed-out images
- Partial visibility of barcodes
- Detection failures due to lighting or blur
- Multiple barcode types on a single package

---

##  Features

- Real-time barcode detection using computer vision
- Extraction of:
  - UPC code
  - Serial number
  - Part number
- Works with camera input or static images
- Handles zoom variations and partial detection cases
- Displays detection confidence and bounding boxes
- Windows Forms UI for user interaction

---

##  Computer Vision Approach

- Image preprocessing (grayscale, blur reduction, thresholding)
- Barcode region detection
- Feature extraction for decoding
- Handling edge cases:
  - low-resolution input
  - zoomed-out objects
  - partially visible barcodes

---

##  Technologies Used

- C#
- Windows Forms (.NET Framework)
- OpenCV (or EmguCV if used)
- Barcode decoding library (custom model)
- Computer Vision techniques

---

##  Results

###  Successful Detection
![Detected](screenshots/detected-box.png)

###  Zoomed-out Case
![Zoom Out](screenshots/zoom-out-case.png)

###  Failure / Undetected Case
![Failure](screenshots/failure-case.png)

---

##  System Workflow

1. Capture image from camera / input file  
2. Preprocess image (filtering, enhancement)  
3. Detect barcode regions  
4. Decode barcode data  
5. Classify output (UPC / Serial / Part Number)  
6. Display results in UI  

---

##  Limitations

- Very small barcodes may not be detected
- Lighting conditions affect accuracy

---

##  Future Improvements

- Deep learning-based barcode detection
- Real-time edge deployment optimization
- Multi-camera support
- Integration with inventory tracking system

---

## Author

Hsu Myat Noe  
Robotics & AI Engineering Student  
King Mongkut’s Institute of Technology Ladkrabang

---

##  License

MIT License