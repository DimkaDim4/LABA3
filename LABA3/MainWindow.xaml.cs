using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LABA3
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataGrid1.ItemsSource = Points;
        }

        private ObservableCollection<Point> points = new ObservableCollection<Point>();
        public ObservableCollection<Point> Points
        {
            get { return points; }
            set
            {
                points = value;
            }
        }

        double xMin, xMax, yMin, yMax;
        double zoomX = 1, zoomY = 1, xsign_off, ysign_off;

        private void MouseWheel(object sender, MouseWheelEventArgs e)
        {
            zoomX += e.Delta / 120 * 0.1;
            zoomY += e.Delta / 120 * 0.1;

            zoomX = Math.Max(0.1, zoomX);
            zoomY = Math.Max(0.1, zoomY);

            Draw();
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            zoomX = 1;
            zoomY = 1;

            Draw();
        }

        private void AddPoint(double x, double y)
        {
            Point point = new Point();
            point.PropertyChanged += Point_PropertyChanged;
            point.X = x;
            point.Y = y;
            points.Add(point);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AddPoint(0, 0);
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt";
            if (openFileDialog.ShowDialog() == true)
            {
                points.Clear();
                var s = File.ReadAllLines(openFileDialog.FileName);
                foreach (var str in s)
                {
                    var coords = str.Split('\t');
                    AddPoint(double.Parse(coords[0]), double.Parse(coords[1]));
                }
            }
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text files (*.txt)|*.txt";
            if (saveFileDialog.ShowDialog() == true)
            {
                var file = new StreamWriter(saveFileDialog.FileName);
                foreach (Point point in points)
                {
                    file.WriteLine($"{point.X}\t{point.Y}");
                }
                file.Close();
            }
        }

        private void Point_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {   
            this.Draw();
        }

        private void Draw()
        {
            if (img1.Children.Count > 0) img1.Children.Clear();
            if (img2.Children.Count > 0) img2.Children.Clear();
            if (img3.Children.Count > 0) img3.Children.Clear();

            if (points.Count != 0)
            {
                xMin = points[0].X;
                yMin = points[0].Y;

                xMax = xMin;
                yMax = yMin;

                foreach(Point point in points)
                {
                    xMin = Math.Min(xMin, point.X);
                    yMin = Math.Min(yMin, point.Y);

                    xMax = Math.Max(xMax, point.X);
                    yMax = Math.Max(yMax, point.Y);
                }

                double h_x = (xMax - xMin);
                double h_y = (yMax - yMin);
                double x_c = h_x * 0.5 + xMin;
                double y_c = h_y * 0.5 + yMin;

                xMin = x_c - 0.5 * h_x * zoomX;
                xMax = x_c + 0.5 * h_x * zoomX;

                yMin = y_c - 0.5 * h_y * zoomY;
                yMax = y_c + 0.5 * h_y * zoomY;

                List<Point> sortedPoints = points.ToList<Point>();
                sortedPoints.Sort((a, b) => a.X.CompareTo(b.X));
                Polyline line = new Polyline();

                foreach (Point point in sortedPoints)
                {
                    System.Windows.Point point1 = new System.Windows.Point((point.X - xMin) / (xMax - xMin) * img1.Width, img1.Height - (point.Y - yMin) / (yMax - yMin) * img1.Height);
                    line.Points.Add(point1);
                }

                line.Stroke = new SolidColorBrush(Colors.Black);
                line.StrokeThickness = 3;
                img1.Children.Add(line);

                // подписи осей по х
                double div = (xMax - xMin) / 9.0;

                double axeW = img3.Width;
                double axeH = img3.Height;

                double w10 = axeW / 9;

                for (int i = 1; i < 9; ++i)
                {
                    double x_cur = xMin + i * div;

                    string text = x_cur.ToString();

                    if (text.Length > 5)
                        text = text.Substring(0, 5);

                    Text(img3, i * w10, 25.0, text, Color.FromRgb(0, 0, 0));
                }

                // подписи осей по х
                div = (yMax - yMin) / 9.0;

                axeW = img2.Width;
                axeH = img2.Height;

                double h10 = axeH / 9;

                for (int i = 1; i < 9; ++i)
                {
                    double y_cur = yMax - i * div;

                    string text = y_cur.ToString();

                    if (text.Length > 5)
                        text = text.Substring(0, 5);

                    Text(img2, 25.0, i * h10, text, Color.FromRgb(0, 0, 0));
                }

            }
        }

        private void Text(Canvas canvasObj, double x, double y, string text, Color color)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.Foreground = new SolidColorBrush(color);
            textBlock.Measure(new Size(9999, 9999));
            Canvas.SetLeft(textBlock, x - textBlock.DesiredSize.Width / 2.0);
            Canvas.SetTop(textBlock, y - textBlock.DesiredSize.Height / 2.0);
            canvasObj.Children.Add(textBlock);
        }
    }

    public class Point : INotifyPropertyChanged, IComparable<Point>
    {
        double x, y;
        public double X
        {
            get { return x; } 
            set {
                x = value;
                OnPropertyChanged();
                //MessageBox.Show($"X = {X}, Y = {Y}");
            }
        }
        public double Y
        { 
            get { return y; } 
            set { 
                y = value;
                OnPropertyChanged();
                //MessageBox.Show($"X = {X}, Y = {Y}");
            } 
        }

        private void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public int CompareTo(Point other)
        {
            return X.CompareTo(other.X);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
