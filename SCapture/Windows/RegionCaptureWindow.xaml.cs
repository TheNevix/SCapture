using Hardcodet.Wpf.TaskbarNotification;
using SCapture.Classes;
using SCapture.Properties;
using System;
using System.Threading.Tasks;
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

            Start = e.GetPosition(Canvas1);

            Canvas.SetLeft(Rect, Start.X);
            Canvas.SetTop(Rect, Start.Y);
        }

        private async void Grid1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDrawing = false;

            // Making sure the opacity is normal and the rectangle
            // are not in the screenshot
            Rect.Visibility = Visibility.Collapsed;
            this.Opacity = 0;
            await Task.Delay(200);

            System.Windows.Point actualStart = this.PointToScreen(Start);
            System.Windows.Point actualCurrent = this.PointToScreen(Current);

            int captureX = (int)actualStart.X;
            int captureY = (int)actualStart.Y;
            int captureW = (int)Math.Abs(actualCurrent.X - actualStart.X);
            int captureH = (int)Math.Abs(actualCurrent.Y - actualStart.Y);

            BitmapSource bSource = ScreenCapturer.CaptureRegion(captureX, captureY, captureW, captureH);

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
                Current = e.GetPosition(Canvas1);

                // Calculate rectangle cords/size
                X = Math.Min(Current.X, Start.X);
                Y = Math.Min(Current.Y, Start.Y);
                W = Math.Abs(Current.X - Start.X); // Use Abs to ensure positive width
                H = Math.Abs(Current.Y - Start.Y);

                Canvas.SetLeft(Rect, X);
                Canvas.SetTop(Rect, Y);

                // Update rectangle
                Rect.Width = W;
                Rect.Height = H;
                Rect.SetValue(Canvas.LeftProperty, X);
                Rect.SetValue(Canvas.TopProperty, Y);
            }
        }
        #endregion
    }
}
