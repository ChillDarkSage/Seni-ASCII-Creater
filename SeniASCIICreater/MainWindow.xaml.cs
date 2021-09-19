using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Drawing;
using System.Globalization;

namespace SeniASCIICreater
{

    public partial class MainWindow : Window
    {

        public string filePath;

        public Painter painter;

        public int maxWidth { set; get; } = 700;
        public int maxHeight { set; get; } = 510;
        public int minWidth { set; get; } = 600;
        public int minHeight { set; get; } = 420;




        public MainWindow()
        {
            InitializeComponent();

            painter = new Painter();

            this.SetBinding();

            var icon = BitmapFrame.Create(new Uri("pack://application:,,,/icon.png"));
            this.Icon = icon;
        }

        private void DragWindow(object sender, MouseButtonEventArgs args)
        {
            this.DragMove();
        }

        private void SetBinding()
        {
            this.painter.SetBinding(Painter.IsColorfulProperty, new Binding("IsChecked") { Source = checkBoxColor });

            this.painter.SetBinding(Painter.IsGaussionBluredProperty, new Binding("IsChecked") { Source = checkBoxGaussion });

            this.painter.SetBinding(Painter.IsSortedProperty, new Binding("IsChecked") { Source = checkBoxBalance });

            this.painter.SetBinding(Painter.IsUserListProperty, new Binding("IsChecked") { Source = checkBoxUserList });

            this.painter.SetBinding(Painter.UserListProperty, new Binding("Text") { Source = textBoxUserList });

        }

        private void Start(object sender, RoutedEventArgs e)
        {
            var fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.Description = "你想把这幅画放在哪儿？";
            fbd.ShowDialog();

            string outPath = fbd.SelectedPath;

            if (!string.IsNullOrWhiteSpace(outPath))
                painter.Dispatcher.Invoke(() => {painter.Draw(filePath, outPath); });
        }

        private void SelectFile(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog()
            {
                DefaultExt = ".png",
                Title = "从这里选择文件~",
                Filter = "所有文件|*.jpeg;*.png;*.jpg|JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg"
            };
            var result = ofd.ShowDialog();
            if (result == true)
            {
                filePath = ofd.FileName;

                double height;
                double width;
                using (Bitmap bitmap = new Bitmap(ofd.FileName))
                {
                    height = bitmap.Height;
                    width = bitmap.Width;
                }
                double n = width / height;
                double max = Math.Max(width, height);

                if (max == width)
                {
                    if (width >= maxWidth)
                    {
                        this.Width = maxWidth;
                        this.Height = maxWidth / n;
                    }
                    else
                    {
                        if (height < this.minHeight)
                        {
                            this.Height = this.minHeight;
                            this.Width = this.Height * n;
                        }

                        this.Width = width;
                        this.Height = height;
                    }
                }
                else
                {
                    if (height >= maxHeight)
                    {
                        this.Height = maxHeight;
                        this.Width = maxHeight * n;
                    }
                    else
                    {
                        if (width < this.minWidth)
                        {
                            this.Width = this.minWidth;
                            this.Height = width / n;
                        }
                        this.Width = width;
                        this.Height = height;
                    }
                }

                Rect rect = new Rect(0, 0, this.Width, this.Height);
                borderRect.Rect = rect;
                image.Source = new BitmapImage(new Uri(filePath));
                this.ResizeMode = ResizeMode.CanMinimize;

                icon.Opacity = 0;
                author.Opacity = 0;
                title.Opacity = 0;
            }
        }

        private void ColorClick(object sender, RoutedEventArgs e)
        {
            if (checkBoxColor.IsChecked == true)
            {
                painter.IsColorful = true;
                painter.IsSorted = false;
                checkBoxBalance.IsChecked = false;
                checkBoxBalance.IsEnabled = false;
                checkBoxGaussion.IsEnabled = true;
            }else if (checkBoxColor.IsChecked == false)
            {
                painter.IsColorful = false;
                painter.IsGaussionBlured = false;
                checkBoxGaussion.IsChecked = false;
                checkBoxGaussion.IsEnabled = false;
                checkBoxBalance.IsEnabled = true;
            }
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }

    public class CheckedToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? b = value as bool?;
            if (b == true)
            {
                return Visibility.Visible;
            }
            else
                return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SourceToImage : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ImageSource s = value as ImageSource;
            if (s == null)
            {
                return Visibility.Hidden;
            }
            else
                return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
