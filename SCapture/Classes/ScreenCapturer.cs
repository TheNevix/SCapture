using SCapture.Properties;
using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace SCapture.Classes
{
    class ScreenCapturer
    {
        /// <summary>
        /// Captures the graphical content of the given region
        /// </summary>
        /// <returns>The image captured</returns>
        public static BitmapSource CaptureRegion(int Left, int Top, int Width, int Height)
        {
            IntPtr dc1 = NativeMethods.GetDC(NativeMethods.GetDesktopWindow());
            IntPtr dc2 = NativeMethods.CreateCompatibleDC(dc1);

            // Create Bitmap
            IntPtr hBitmap = NativeMethods.CreateCompatibleBitmap(dc1, Width, Height);

            NativeMethods.SelectObject(dc2, hBitmap);
            NativeMethods.BitBlt(dc2, 0, 0, Width, Height, dc1, Left, Top, 0x00CC0020);

            // Get BitmapSource
            BitmapSource bSource = Imaging.CreateBitmapSourceFromHBitmap(hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            // Release resources
            NativeMethods.DeleteObject(hBitmap);
            NativeMethods.ReleaseDC(IntPtr.Zero, dc1);
            NativeMethods.ReleaseDC(IntPtr.Zero, dc2);

            return bSource;
        }

        /// <summary>
        /// Captures the graphical content of the given window
        /// </summary>
        /// <param name="hWnd">Window handle to capture</param>
        /// <returns>The image captured</returns>
        public static BitmapSource CaptureWindow(IntPtr hWnd)
        {
            // Get window rect
            RECT rc;
            NativeMethods.GetWindowRect(hWnd, out rc);

            // Bring window to the front
            NativeMethods.SetForegroundWindow(hWnd);

            // Small hack to fix black border arround window
            int xOffset = 8;
            return CaptureRegion(
                rc.Left + xOffset,
                rc.Top + xOffset,
                rc.Width - xOffset * 2,
                rc.Height - xOffset * 2);
        }

        /// <summary>
        /// Captures the graphical content of screen
        /// </summary>
        /// <returns>The image captured</returns>
        public static BitmapSource CaptureFullScreen()
        {
            var (scaleX, scaleY) = GetSystemDpiScale();

            // Get the entire screen size with DPI scaling applied
            int screenWidth = (int)(SystemParameters.PrimaryScreenWidth * scaleX);
            int screenHeight = (int)(SystemParameters.PrimaryScreenHeight * scaleY);

            return CaptureRegion(0, 0, screenWidth, screenHeight);
        }

        /// <summary>
        /// Saves the capture to a file.
        /// </summary>
        /// <param name="bSource">The image captured</param>
        /// <returns>Capture saved successfully?</returns>
        public static bool Save(BitmapSource bSource)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string extension = ".jpg";

            switch (Settings.Default.SaveFileFormat)
            {
                case 0:
                    extension = ".bmp";
                    break;
                case 1:
                    extension = ".jpeg";
                    break;
                default:
                    extension = ".png";
                    break;

            }

            string fileName = desktopPath +
                $"/screenshot_{DateTime.Now.ToString("yyyy_dd_MM_HH_mm_ss")}" +
                extension;

            return Save(fileName, bSource);
        }

        /// <summary>
        /// Saves the capture to a file
        /// </summary>
        /// <param name="fileName">What's the file name?</param>
        /// <param name="bSource">The image captured</param>
        /// <returns></returns>
        public static bool Save(string fileName, BitmapSource bSource)
        {
            try
            {
                BitmapEncoder encoder;
                var extension = Path.GetExtension(fileName);
                switch (extension)
                {
                    case ".bmp":
                        encoder = new BmpBitmapEncoder();
                        break;
                    case ".jpeg":
                        encoder = new JpegBitmapEncoder();
                        break;
                    default:
                        encoder = new PngBitmapEncoder();
                        break;
                }

                encoder.Frames.Add(BitmapFrame.Create(bSource));

                using (var stream = File.Create(fileName))
                {
                    encoder.Save(stream);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// Get DPI Scale
        /// </summary>
        /// <returns></returns>
        public static (double scaleX, double scaleY) GetSystemDpiScale()
        {
            using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                double dpiX = graphics.DpiX;
                double dpiY = graphics.DpiY;
                return (dpiX / 96.0, dpiY / 96.0); // 96.0 is the base DPI for 100% scaling
            }
        }
    }
}
