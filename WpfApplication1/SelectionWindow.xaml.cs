using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Threading;


namespace ScreenShooter
{
    /// <summary>
    /// Interaktionslogik für Window1.xaml
    /// </summary>
    ///  
    public partial class SelectionWindow : Window
    {
        public event EventHandler<customEventArgs> RaiseCustomEvent;
        private Point p_Start = new Point(-100,-100);
        private Point p_End = new Point(-100,-100);
        private Rectangle saveRect = null;
        private MoveType move = MoveType.Draw;
        private bool top = false;
        private bool left = false;


        public SelectionWindow()
        {
            InitializeComponent();            
            WindowState = WindowState.Maximized;
        }

        public SelectionWindow(Point a, Point b)
        {
            InitializeComponent();
            WindowState = WindowState.Maximized;

            Rectangle r = new Rectangle();
            r.Stroke = new SolidColorBrush(Colors.Red);
            r.Opacity = 1;
            r.Height = Math.Abs(b.Y-a.Y);
            r.Width = Math.Abs(b.X - a.X);
            r.VerticalAlignment = VerticalAlignment.Top;
            r.HorizontalAlignment = HorizontalAlignment.Left;
            r.Margin = new Thickness(a.X, a.Y, 0, 0);
            this.mainGrid.Children.Add(r);
            saveRect = r;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
            p_Start = e.GetPosition(null);

            double xAct = e.GetPosition(null).X;
            double yAct = e.GetPosition(null).Y;

            if (saveRect != null)
            {
                double rectLeft = saveRect.Margin.Left;
                double rectTop = saveRect.Margin.Top;
                double rectRight = saveRect.Margin.Left + saveRect.Width;
                double rectBottom = saveRect.Margin.Top + saveRect.Height;

                if (between(xAct, rectLeft + 3, rectRight - 3) &&
                    between(yAct, rectTop + 3, rectBottom - 3))
                    move = MoveType.Drag;

                else if (( between(xAct, rectLeft - 3, rectLeft) || between(xAct, rectRight - 3, rectRight) ) &&
                    between(yAct, rectTop, rectBottom))
                {
                    move = MoveType.ResizeWidth;
                    left = between(xAct, rectLeft - 3, rectLeft + 3);
                }
                else if (( between(yAct, rectTop - 3, rectTop) || between(yAct, rectBottom - 3, rectBottom) ) &&
                    between(xAct, rectLeft, rectRight))
                {
                    move = MoveType.ResizeHeight;
                    top = between(yAct, rectTop - 3, rectTop + 3);
                }
                else
                    move = MoveType.Draw;
            }
            else
                move = MoveType.Draw;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (CM.IsVisible)
            {                
                return;
            }

            double xAct = e.GetPosition(null).X;
            double yAct = e.GetPosition(null).Y;

            #region select Cursor
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                if (saveRect != null)
                {
                    double rectLeft = saveRect.Margin.Left;
                    double rectTop = saveRect.Margin.Top;
                    double rectRight = saveRect.Margin.Left + saveRect.Width;
                    double rectBottom = saveRect.Margin.Top + saveRect.Height;

                    if (between(xAct, rectLeft + 3, rectRight - 3) &&
                        between(yAct, rectTop + 3, rectBottom - 3))
                        Cursor = Cursors.Hand;

                    else if (( between(xAct, rectLeft - 3, rectLeft) || between(xAct, rectRight - 3, rectRight) ) &&
                        between(yAct, rectTop, rectBottom))
                        Cursor = Cursors.SizeWE;
                    else if (( between(yAct, rectTop - 3, rectTop) || between(yAct, rectBottom - 3, rectBottom) ) &&
                        between(xAct, rectLeft, rectRight))
                        Cursor = Cursors.SizeNS;

                    else Cursor = Cursors.Pen;
                }
                else
                    Cursor = Cursors.Pen;
                return;
            }
            else
            {
                switch (move)
                {
                    case MoveType.Drag:
                        Cursor = Cursors.Hand;
                        break;
                    case MoveType.Draw:
                        Cursor = Cursors.Pen;
                        break;
                    case MoveType.ResizeHeight:
                        Cursor = Cursors.SizeNS;
                        break;
                    case MoveType.ResizeWidth:
                        Cursor = Cursors.SizeWE;
                        break;
                }
            }
            #endregion

            #region scale, drag, draw the rectangle
            if (mainGrid.Children.Count > 0)
                this.mainGrid.Children.RemoveAt(0);
            Rectangle r = new Rectangle();
            p_End = e.GetPosition(null);
            switch (move)
            {   // Rechteck Zeichnen
                case MoveType.Draw:
                    r.Stroke = new SolidColorBrush(Colors.Red);
                    r.Opacity = 1;
                    r.Height = Math.Abs(p_End.Y - p_Start.Y);
                    r.Width = Math.Abs(p_End.X - p_Start.X);
                    double t_left = p_End.X > p_Start.X ? p_Start.X : p_End.X;
                    double t_top = p_End.Y > p_Start.Y ? p_Start.Y : p_End.Y;
                    r.VerticalAlignment = VerticalAlignment.Top;
                    r.HorizontalAlignment = HorizontalAlignment.Left;
                    r.Margin = new Thickness(t_left, t_top, 0, 0);
                    this.mainGrid.Children.Add(r);
                    saveRect = r;
                    break;
                // Rechteck verschieben
                case MoveType.Drag:
                    r = saveRect;
                    double moveHorizontal = p_Start.X - p_End.X;
                    double moveVertical = p_Start.Y - p_End.Y;
                    if (r.Margin.Left - moveHorizontal < 0)
                        moveHorizontal = r.Margin.Left;
                    if (r.Margin.Top - moveVertical < 0)
                        moveVertical = r.Margin.Top;
                    r.Margin = new Thickness(r.Margin.Left - moveHorizontal, r.Margin.Top - moveVertical, 0, 0);
                    this.mainGrid.Children.Add(r);
                    saveRect = r;
                    p_Start = p_End;
                    break;
                // Rechteckhöhe skalieren
                case MoveType.ResizeHeight:
                    r = saveRect;
                    double resizeY = p_Start.Y - p_End.Y;
                    try
                    {
                        if (top)
                        {
                            if (r.Height - resizeY > 0)
                            {
                                r.Margin = new Thickness(r.Margin.Left, r.Margin.Top - resizeY, 0, 0);
                                r.Height += resizeY;
                            }
                        }
                        else
                        {
                            if (r.Height - resizeY > 0)
                                r.Height -= resizeY;
                        }
                    }
                    catch (Exception) { }
                    this.mainGrid.Children.Add(r);
                    saveRect = r;
                    p_Start = p_End;
                    break;
                // Rechteckbreite skalieren
                case MoveType.ResizeWidth:
                    r = saveRect;
                    double resizeX = p_Start.X - p_End.X;
                    try
                    {
                        if (left)
                        {
                            if (r.Width - resizeX > 0)
                            {
                                r.Margin = new Thickness(r.Margin.Left - resizeX, r.Margin.Top, 0, 0);
                                r.Width += resizeX;
                            }
                        }
                        else
                        {
                            if (r.Width - resizeX > 0)
                                r.Width -= resizeX;
                        }
                    }
                    catch (Exception) { }
                    this.mainGrid.Children.Add(r);
                    saveRect = r;
                    p_Start = p_End;
                    break;
            } 
            #endregion

            // for cpu utilisation reasons
            Thread.Sleep(10);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Return:
                    CM_End_Click(this, new RoutedEventArgs());
                    break;
                case Key.Escape:
                    CM_Abort_Click(this, new RoutedEventArgs());
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Topmost = true;
            Activate();
            Focus();
        }

        /// <summary>
        /// gets the upper left and lower right point of the last drawn selection rectangle
        /// </summary>
        /// <param name="abort">returns an emtpy list of points if true</param>
        /// <returns></returns>
        public List<Point> getPoints(bool abort)
        {
            List<Point> p = new List<Point>();
            if (abort)
                return p;
            if (mainGrid.Children.Count > 0)
            {
                p.Add(new Point(saveRect.Margin.Left, saveRect.Margin.Top));
                p.Add(new Point(saveRect.Width+ saveRect.Margin.Left, saveRect.Height+ saveRect.Margin.Top));
                return p;
            }

            p.Clear();
            p.Add(new Point(-100, -100));
            p.Add(new Point(-100, -100));
            return p;
        }

        /// <summary>
        /// if a value is between the boundaries
        /// </summary>
        /// <param name="actual">the value to be tested</param>
        /// <param name="min">lower boundary</param>
        /// <param name="max">upper boundary</param>
        /// <returns>true if the value is between the boundaries</returns>
        private bool between(double actual, double min, double max)
        {
            if (( actual < min ) || ( actual > max ))
                return false;
            else return true;
        }

        private enum MoveType
        {
            Draw,
            Drag,
            ResizeHeight,
            ResizeWidth
        }

        #region Context Menu
        private void CM_End_Click(object sender, RoutedEventArgs e)
        {
            RaiseCustomEvent(this, new customEventArgs(getPoints(false)));
            this.Close();
        }

        private void CM_Abort_Click(object sender, RoutedEventArgs e)
        {
            RaiseCustomEvent(this, new customEventArgs(getPoints(true)));
            this.Close();
        }

        private void CM_Reset_Click(object sender, RoutedEventArgs e)
        {
            mainGrid.Children.Clear();
        } 
        #endregion

    }

    public class customEventArgs : EventArgs
    {
        private List<Point> pts;
        public customEventArgs(List<Point> Points)
        {
            pts = Points;
        }

        public List<Point> Points
        {
            get { return pts; }
        }
    }
}
