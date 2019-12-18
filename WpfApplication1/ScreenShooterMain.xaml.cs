using System;
using System.Windows;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Runtime.InteropServices;

namespace ScreenShooter
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class ScreenShooterMain : Window
    {
        int number = 0;
        const double diff = 8; // fest abweichung vom sollwert der Position

        private System.Windows.Point scsh_Start = new System.Windows.Point(-100, -100);
        private System.Windows.Point scsh_Ende = new System.Windows.Point(-100, -100);
        private System.Windows.Point OCR_Start = new System.Windows.Point(-100,-100);
        private System.Windows.Point OCR_Ende = new System.Windows.Point(-100, -100);
        private WindowState prevWs;
        private string folder = @"C:\Users\Public\Pictures\Sample Pictures\ScreenShooter";        

        public ScreenShooterMain()
        {
            InitializeComponent();           
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Hotkeys erstellen
            hWnd = new WindowInteropHelper(this).Handle;
            HookWndProc(this);
            RegisterHotKey(hWnd, 0x0312, (int)Modifiers.Ctrl, (int)Keys.P);
            RegisterHotKey(hWnd, 0x0312, (int)Modifiers.Ctrl, (int)Keys.I);
            RegisterHotKey(hWnd, 0x0312, (int)Modifiers.Ctrl, (int)Keys.O);
        }

        private void B_Folder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowNewFolderButton = true;
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                folder = fbd.SelectedPath;
        }

        #region screenshot
        private void b_Screenshot_Click(object sender, RoutedEventArgs e)
        {
            double prevLeft = Left;
            double prevTop = Top;
            prevWs = this.WindowState;
            Left = -500;
            Top = -500;

            takeScreenShot();

            Left = prevLeft;
            Top = prevTop;
            this.WindowState = prevWs;
        }
        private void takeScreenShot()
        {
            int Width = (int)( Math.Abs(OCR_Start.X - OCR_Ende.X) );
            int Height = (int)( Math.Abs(OCR_Start.Y - OCR_Ende.Y) );
            string fileOCR = string.Empty;
            string text = string.Empty;
            if (Height > 0 && Width > 0)
            {
                using (Bitmap bmpOCR = new Bitmap(Width, Height))
                {
                    using (Graphics g = Graphics.FromImage(bmpOCR))
                    {
                        g.CopyFromScreen((int)OCR_Start.X, (int)OCR_Start.Y, 0, 0, bmpOCR.Size);
                        Directory.CreateDirectory(folder);
                        fileOCR = folder + @"\OCR_Region.JPG";
                        bmpOCR.Save(fileOCR, System.Drawing.Imaging.ImageFormat.Jpeg);
                        text = doOCR(fileOCR);
                    }
                }
            }

            if (text != string.Empty)
            {
                text = text.Replace("\r\n", "").Replace(@"\", "-").Replace("/", "-");
                File.Delete(fileOCR);
            }
            else
            {
                text = number.ToString();                
            }

            Width = (int)( Math.Abs(scsh_Start.X - scsh_Ende.X) );
            Height = (int)( Math.Abs(scsh_Start.Y - scsh_Ende.Y) );

            if (Height>0 && Width>0)
            {                
                using (Bitmap bmpImage = new Bitmap(Width, Height))
                {
                    using (Graphics g = Graphics.FromImage(bmpImage))
                    {
                        g.CopyFromScreen((int)scsh_Start.X, (int)scsh_Start.Y, 0, 0, bmpImage.Size);
                        Directory.CreateDirectory(folder);
                        string filePath = folder + @"\" + text + ".png";
                        int add_num = 1;
                        if (File.Exists(filePath))
                        {
                            if (System.Windows.MessageBox.Show(
                                "A file with the filename '" + filePath.Replace(folder + @"\", "") + "' already Exists. \r\nDo you want to override it?",
                                "Override file", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.No)
                            { 
                                while (File.Exists(filePath))
                                {
                                    if (!filePath.Contains("_"))
                                        filePath = filePath.Remove(filePath.Length - 4);
                                    else
                                        filePath = filePath.Remove(filePath.LastIndexOf("_"));

                                    filePath += "_" + add_num.ToString() + ".png";
                                    add_num++;
                                }
                            }                            
                        }
                        bmpImage.Save(filePath);
                        number++;
                    }
                }
            }
            else
            {
                using (Bitmap bmpImage = new Bitmap(Convert.ToInt32(SystemParameters.VirtualScreenWidth), Convert.ToInt32(SystemParameters.VirtualScreenHeight)))
                {
                    using (Graphics g = Graphics.FromImage(bmpImage))
                    {
                        g.CopyFromScreen(0, 0, 0, 0, bmpImage.Size);
                        Directory.CreateDirectory(folder);
                        string filePath = folder + @"\" + text + ".png";
                        int add_num = 1;
                        if (File.Exists(filePath))
                        {
                            if (System.Windows.MessageBox.Show(
                                "A file with the filename '" + filePath.Replace(folder + @"\", "") + "' already Exists. \r\nDo you want to override it?",
                                "Override file", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.No)
                            {
                                while (File.Exists(filePath))
                                {
                                    if (!filePath.Contains("_"))
                                        filePath = filePath.Remove(filePath.Length - 4);
                                    else
                                        filePath = filePath.Remove(filePath.LastIndexOf("_"));

                                    filePath += "_" + add_num.ToString() + ".png";
                                    add_num++;
                                }
                            }
                        }
                        bmpImage.Save(filePath);
                        number++;
                    }
                }
            }
        }
        /// <summary>
        /// Check for Images
        /// read text from these images.
        /// save text from each image in text file automaticly.
        /// handle problems with images
        /// </summary>
        /// <param name="directoryPath">Set Directory Path to check for Images in it</param>
        public string doOCR(string file)
        {
            try
            {
                //OCR Operations ... 
                MODI.Document md = new MODI.Document();
                md.Create(file);
                md.OCR(MODI.MiLANGUAGES.miLANG_ENGLISH, true, true);
                MODI.Image image = (MODI.Image)md.Images[0];

                return image.Layout.Text;
            }
            catch (Exception exc)
            {
                string msgTxt;
                if (exc.Message == "OCR running error")
                    msgTxt = "Es konnten keine Buchstaben erkannt werden. \r\nDas Bild wird unter einer laufenden Nummer gespeichert.";
                else msgTxt = exc.Message;
                //uncomment the below code to see the expected errors
                System.Windows.MessageBox.Show(msgTxt, "OCR Exception");
                return string.Empty;
            }

        } 
        #endregion

        #region Auswahlfenster
        private void b_SelectRegion_Click(object sender, RoutedEventArgs e)
        {
            prevWs = WindowState;
            this.WindowState = WindowState.Minimized;
            double scale = getDPIScale();
            SelectionWindow winLocationImage = new SelectionWindow();
            if (scsh_Start.X >= 0 || scsh_Start.Y >= 0)
            {
                System.Windows.Point start = new System.Windows.Point(( scsh_Start.X + diff ) / scale, ( scsh_Start.Y + diff ) / scale);
                System.Windows.Point end = new System.Windows.Point(( scsh_Ende.X + diff ) / scale, ( scsh_Ende.Y + diff ) / scale);
                winLocationImage = new SelectionWindow(start, end);
            }
            winLocationImage.RaiseCustomEvent += pointsScreenshot;
            winLocationImage.Closed += selectionWindow_Closed;
            winLocationImage.ShowDialog();
        }
        private void pointsScreenshot(object sender, customEventArgs e)
        {
            if (e == null || e.Points == null || e.Points.Count < 1)
                return;

            double scale = getDPIScale();            
            scsh_Start = new System.Windows.Point(e.Points[0].X * scale - diff, e.Points[0].Y * scale - diff);
            scsh_Ende = new System.Windows.Point(e.Points[1].X * scale - diff, e.Points[1].Y * scale - diff);

        }

        private void B_SelectOCR_Click(object sender, RoutedEventArgs e)
        {
            prevWs = WindowState;
            this.WindowState = WindowState.Minimized;
            double scale = getDPIScale();
            SelectionWindow winLocationOCR = new SelectionWindow(); ;
            if (OCR_Start.X >= 0 || OCR_Start.Y >= 0)            
            {
                System.Windows.Point start = new System.Windows.Point(( OCR_Start.X + diff ) / scale, ( OCR_Start.Y + diff ) / scale);
                System.Windows.Point end = new System.Windows.Point(( OCR_Ende.X + diff ) / scale, ( OCR_Ende.Y + diff ) / scale);
                winLocationOCR = new SelectionWindow(start, end);
            }
            winLocationOCR.RaiseCustomEvent += pointsOCR;
            winLocationOCR.Closed += selectionWindow_Closed;
            winLocationOCR.ShowDialog();
        }
        private void pointsOCR(object sender, customEventArgs e)
        {
            if (e == null || e.Points == null || e.Points.Count < 1)
                return;

            double scale = getDPIScale();             
            OCR_Start = new System.Windows.Point(e.Points[0].X * scale - diff, e.Points[0].Y * scale - diff);
            OCR_Ende = new System.Windows.Point(e.Points[1].X * scale - diff, e.Points[1].Y * scale - diff);
        }

        /// <summary>
        /// gets the device pixels for scaling factor dpi/dip
        /// </summary>
        /// <returns>the scaling factor</returns>
        private double getDPIScale()
        {
            PresentationSource source = PresentationSource.FromVisual(this);

            if (source != null)
                return source.CompositionTarget.TransformToDevice.M11;

            return 0;
        }
        private void selectionWindow_Closed(object sender, EventArgs e)
        {
            this.WindowState = prevWs;
        } 
        #endregion

        #region Hotkeys

        IntPtr hWnd;

        [DllImport("user32.dll")]
        internal static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        //pass your WPF Window object to this method (Window derives from Visual)
        private void HookWndProc(Visual window)
        {
            var source = PresentationSource.FromVisual(window) as HwndSource;
            if (source == null) throw new Exception("Could not create hWnd source from window.");
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x0312)
            {
                var lpInt = (int)lParam;
                Keys key = (Keys)( ( lpInt >> 16 ) & 0xFFFF );
                Modifiers modifier = (Modifiers)( lpInt & 0xFFFF );

                switch (key)
                {
                    case Keys.I:
                        b_SelectRegion_Click(this, new RoutedEventArgs());
                        break;
                    case Keys.P:
                        b_Screenshot_Click(this, new RoutedEventArgs());
                        break;
                    case Keys.O:
                        B_SelectOCR_Click(this, new RoutedEventArgs());
                        break;
                }
            }
            return IntPtr.Zero;
        }

        private enum Modifiers
        {
            NoMod = 0x0000,
            Alt = 0x0001,
            Ctrl = 0x0002,
            Shift = 0x0004,
            Win = 0x0008
        } 
        #endregion

    }

}
