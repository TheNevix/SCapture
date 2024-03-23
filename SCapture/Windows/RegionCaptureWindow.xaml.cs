using Hardcodet.Wpf.TaskbarNotification;
using SCapture.Classes;
using SCapture.Properties;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace SCapture.Windows
{
    /// <summary>
    /// Interaction logic for RegionCaptureWindow.xaml
    /// </summary>
    public partial class RegionCaptureWindow : Window
    {
        /// <summary>
        /// The start position of the Region Rectangle
        /// </summary>
        private System.Windows.Point Start;

        /// <summary>
        /// The end position of the Region Rectangle
        /// </summary>
        private System.Windows.Point Current;

        /// <summary>
        /// Determines weather the user is drawing region or not
        /// </summary>
        private bool isDrawing = false;

        /// <summary>
        /// Rectangle
        /// </summary>
        private double X, Y, W, H;

        public RegionCaptureWindow()
        {
            InitializeComponent();
        }

        #region Functions
        private void Grid1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDrawing = true;

            Start = Mouse.GetPosition(Canvas1);

            Canvas.SetLeft(Rect, Start.X);
            Canvas.SetTop(Rect, Start.Y);
        }

        private void Grid1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDrawing = false;

            // Get DPI scale
            var (scaleX, scaleY) = GetSystemDpiScale();

            // Apply the DPI scaling
            int dpiAdjustedX = (int)(X * scaleX);
            int dpiAdjustedY = (int)(Y * scaleY);
            int dpiAdjustedW = (int)(W * scaleX);
            int dpiAdjustedH = (int)(H * scaleY);

            // Capture the region with DPI adjustment
            BitmapSource bSource = ScreenCapturer.CaptureRegion(dpiAdjustedX, dpiAdjustedY, dpiAdjustedW, dpiAdjustedH);

            if (Settings.Default.AlwaysCopyToClipboard)
                Clipboard.SetImage(bSource);

            if (Settings.Default.AlwaysOpenToEditor)
            {
                new EditCaptureWindow(bSource).Show();
            }
            else
            {
                if (ScreenCapturer.Save(bSource))
                    MainWindow.notification.ShowBalloonTip(this.Title, "File saved!", BalloonIcon.Info);
                else
                    MessageBox.Show("Oups! We couldn't save this file. Please check permissions.");
            }

            this.Close();
        }

        private void Grid1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing)
            {
                // Get new position
                Current = Mouse.GetPosition(Canvas1);

                // Calculate rectangle cords/size
                X = Math.Min(Current.X, Start.X);
                Y = Math.Min(Current.Y, Start.Y);
                W = Math.Max(Current.X, Start.X) - X;
                H = Math.Max(Current.Y, Start.Y) - Y;

                Canvas.SetLeft(Rect, X);
                Canvas.SetTop(Rect, Y);

                // Update rectangle
                Rect.Width = W;
                Rect.Height = H;
                Rect.SetValue(Canvas.LeftProperty, X);
                Rect.SetValue(Canvas.TopProperty, Y);

                // Toogle visibility
                if (Rect.Visibility != Visibility.Visible)
                    Rect.Visibility = Visibility.Visible;
            }
        }

        //Method to get the DPI scale
        private (double scaleX, double scaleY) GetSystemDpiScale()
        {
            using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                double dpiX = graphics.DpiX;
                double dpiY = graphics.DpiY;
                return (dpiX / 96.0, dpiY / 96.0); // 96 is the system DPI for 100% scaling
            }
        }
        #endregion
    }
}
