using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Microsoft.WindowsAPICodePack.Dialogs;
using Patagames.Pdf;
using Patagames.Pdf.Net;
using Patagames.Pdf.Enums;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;

namespace PdfWaterMark
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region  私有变量

        private int previewPageNum;
        private int previewPageQty;

        private BitmapImage previewImage;

        #endregion

        #region 属性
        /// <summary>
        /// 仅处理单文件，否则处理文件夹下所有pdf文件
        /// </summary>
        public bool IsSigleFile { get; set; } = false;

        /// <summary>
        /// 覆盖保存，否则另存到别处
        /// </summary>
        public bool IsSaveReplace { get; set; } = false;

        /// <summary>
        /// 仅打印首页
        /// </summary>
        public bool IsOnlyFirtPage { get; set; } = true;

        /// <summary>
        /// 打印所有页面
        /// </summary>
        public bool IsAllPages { get; set; } = false;

        /// <summary>
        /// 水印是否缩放
        /// </summary>
        public bool IsScale { get; set; } = false;

        /// <summary>
        /// 水印是否铺满
        /// </summary>
        public bool IsFitFull { get; set; } = false;

        /// <summary>
        /// 源文件（夹）路径
        /// </summary>
        public string SourceFilePath { get; set; }

        /// <summary>
        /// 目标文件（夹）路径
        /// </summary>
        public string DestinationFilePath { get; set; }

        /// <summary>
        /// 水印文件路径
        /// </summary>
        public string MarkFilePath { get; set; }

        /// <summary>
        /// 水印宽度
        /// </summary>
        public float MarkWidth { get; set; } = 10;

        /// <summary>
        /// 水印高度
        /// </summary>
        public float MarkHeight { get; set; } = 10;

        /// <summary>
        /// 水印左边距
        /// </summary>
        public float MarkMarginLeft { get; set; } = 10;

        /// <summary>
        /// 水印上边距
        /// </summary>
        public float MarkMarginTop { get; set; } = 10;

        /// <summary>
        /// 水印右边距
        /// </summary>
        public float MarkMarginRight { get; set; } = 10;

        /// <summary>
        /// 水印下边距
        /// </summary>
        public float MarkMarginBottom { get; set; } = 10;


        /// <summary>
        /// 要处理的工作页
        /// </summary>
        public string Pages { get; set; }

        /// <summary>
        /// 当前预览页
        /// </summary>
        public int PreviewPageNum
        {
            get => previewPageNum;
            set
            {
                if (Equals(value, previewPageNum))
                    return;
                previewPageNum = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PreviewPageNum)));
            }
        }

        /// <summary>
        /// 当前文档总页数预览页
        /// </summary>
        public int PreviewPageQty
        {
            get => previewPageQty;
            set
            {
                if (Equals(value, previewPageQty))
                    return;
                previewPageQty = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PreviewPageQty)));
            }
        }

        public BitmapImage PreviewImage
        {
            get => previewImage;
            set
            {
                previewImage = value;
                imagePreview.Source = previewImage;
            }
        }

        #endregion

        /// <summary>
        /// 数据绑定事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();

            #region 数据绑定
            DataContext = this;

            // PDF初始化
            PdfCommon.Initialize();

            // 考虑前后端分离，绑定的操作做好由前端完成
            //radioButtonFile.SetBinding(RadioButton.IsCheckedProperty, new Binding("IsSingleFile"));
            //radioButtonSaveReplace.SetBinding(RadioButton.IsCheckedProperty, new Binding("IsSaveReplace"));
            //radioButtonPage1.SetBinding(RadioButton.IsCheckedProperty, new Binding("IsOnlyFirtPage"));
            //radioButtonPageAll.SetBinding(RadioButton.IsCheckedProperty, new Binding("IsAllPages"));
            //radioButtonScale.SetBinding(RadioButton.IsCheckedProperty, new Binding("IsScale"));
            //radioButtonFull.SetBinding(RadioButton.IsCheckedProperty, new Binding("IsFitFull"));

            //textBoxPath.SetBinding(TipTextBox.TextProperty, new Binding("SourceFilePath")
            //{ UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.TwoWay });

            //textBoxNewPath.SetBinding(TipTextBox.TextProperty, new Binding("DestinationFilePath")
            //{ UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.TwoWay });

            //textBoxMarkPath.SetBinding(TipTextBox.TextProperty, new Binding("MarkFilePath")
            //{ UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.TwoWay });

            //textBoxMarkWidth.SetBinding(TipTextBox.TextProperty, new Binding("MarkWidth")
            //{ UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.TwoWay });
            //textBoxMarkHeight.SetBinding(TipTextBox.TextProperty, new Binding("MarkHeight")
            //{ UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.TwoWay });

            //buttonSaveAs.SetBinding(Button.IsEnabledProperty, new Binding("IsChecked") { Source = radioButtonSaveAs });
            //textBoxNewPath.SetBinding(TipTextBox.IsEnabledProperty, new Binding("IsChecked") { Source = radioButtonSaveAs });

            //textBoxPages.SetBinding(TipTextBox.IsEnabledProperty, new Binding("IsChecked") { Source = radioButtonPageSet });

            #endregion
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var doc = PdfDocument.Load(@"C:\Users\Sniper\Documents\A3.pdf"))
                {
                    var page = doc.Pages[0];

                    // 渐进式加载页面
                    page.StartProgressiveLoad();
                    while (page.ContinueProgressiveLoad() == ProgressiveStatus.ToBeContinued)
                    {
                        Console.WriteLine($"Parsing...");
                    }

                    //PDF unit size is
                    float pdfDpi = 72.0f;
                    if (page.Dictionary.ContainsKey("UserUnit"))
                        pdfDpi = page.Dictionary["UserUnit"].As<Patagames.Pdf.Net.BasicTypes.PdfTypeNumber>().FloatValue / 72;

                    //The actual width and height will be
                    int width = (int)page.Width;
                    int height = (int)page.Height;

                    // 添加图章
                    using (Bitmap waterMark = Bitmap.FromFile(@"C:\Users\Sniper\Documents\wm.png") as Bitmap)
                    {
                        PdfBitmap bitmap = new PdfBitmap(waterMark.Width, waterMark.Height, true);
                        using (var g = Graphics.FromImage(bitmap.Image))
                        {
                            g.DrawImage(waterMark, 0, 0, waterMark.Width / 2, waterMark.Height / 2);
                            g.DrawImage(waterMark, waterMark.Width / 2, waterMark.Height / 2, waterMark.Width / 2, waterMark.Height / 2);
                        }
                        PdfImageObject imageObject = PdfImageObject.Create(doc, bitmap, 0, 0);
                        imageObject.Matrix = new FS_MATRIX(waterMark.Width, 0, 0, waterMark.Height, 0, 0);
                        page.PageObjects.Add(imageObject);
                    }

                    //生成页面内容
                    page.GenerateContent();

                    using (var bitmap = new PdfBitmap(width, height, true))
                    {
                        bitmap.FillRect(0, 0, width, height, FS_COLOR.White);

                        Console.WriteLine($"Start progressive render");
                        ProgressiveStatus status = page.StartProgressiveRender(bitmap, 0, 0, width, height, PageRotate.Normal, RenderFlags.FPDF_ANNOT, null);
                        while (status == ProgressiveStatus.ToBeContinued)
                        {
                            Console.WriteLine($"Render in progress...");
                            status = page.ContinueProgressiveLoad();
                        }


                        PreviewImage = BitmapToBitmapImage(new Bitmap(bitmap.Image));

                        //bitmap.Image.Save(@"C:\Users\Sniper\Documents\sample.png", ImageFormat.Png);

                    }

                    doc.Save(@"C:\Users\Sniper\Documents\2.pdf", SaveFlags.NoIncremental);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("ButtonAbout_Click");
        }

        private void ButtonPrev_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog()
            {
                AddToMostRecentlyUsedList = true,
                Multiselect = false
            };

            if (IsSigleFile)
            {
                dialog.Title = "打开文件";
                dialog.IsFolderPicker = false;
                dialog.Filters.Add(new CommonFileDialogFilter("PDF文件", "*.pdf"));
            }
            else
            {
                dialog.Title = "打开文件夹";
                dialog.IsFolderPicker = true;
            }

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                textBoxPath.Text = dialog.FileName;
            }
        }

        private void ButtonSaveAs_Click(object sender, RoutedEventArgs e)
        {
            if (IsSigleFile)
            {
                CommonSaveFileDialog dialog = new CommonSaveFileDialog()
                {
                    Title = "另存为",
                    AddToMostRecentlyUsedList = true
                };
                dialog.Filters.Add(new CommonFileDialogFilter("PDF文件", "*.pdf"));
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    textBoxNewPath.Text = dialog.FileName;
                }
            }
            else
            {
                CommonOpenFileDialog dialog = new CommonOpenFileDialog()
                {
                    Title = "另存为",
                    AddToMostRecentlyUsedList = true,
                    IsFolderPicker = true
                };
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    textBoxNewPath.Text = dialog.FileName;
                }
            }
        }

        private void ButtonMarkOpen_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog()
            {
                Title = "打开印章文件",
                AddToMostRecentlyUsedList = true,
            };
            dialog.Filters.Add(new CommonFileDialogFilter("图像文件", "*.png"));
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                textBoxMarkPath.Text = dialog.FileName;
            }
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == radioButtonCenter)
            {
                textBoxLeftOffset.IsEnabled = false;
                textBoxTopOffset.IsEnabled = false;
                textBoxRightOffset.IsEnabled = false;
                textBoxBottomOffset.IsEnabled = false;
            }
            else if (sender == radioButtonTopLeft)
            {
                textBoxLeftOffset.IsEnabled = true;
                textBoxTopOffset.IsEnabled = true;
                textBoxRightOffset.IsEnabled = false;
                textBoxBottomOffset.IsEnabled = false;
            }
            else if (sender == radioButtonTopRight)
            {
                textBoxLeftOffset.IsEnabled = false;
                textBoxTopOffset.IsEnabled = true;
                textBoxRightOffset.IsEnabled = true;
                textBoxBottomOffset.IsEnabled = false;
            }
            else if (sender == radioButtonBottomLeft)
            {
                textBoxLeftOffset.IsEnabled = true;
                textBoxTopOffset.IsEnabled = false;
                textBoxRightOffset.IsEnabled = false;
                textBoxBottomOffset.IsEnabled = true;
            }
            else if (sender == radioButtonBottomRight)
            {
                textBoxLeftOffset.IsEnabled = false;
                textBoxTopOffset.IsEnabled = false;
                textBoxRightOffset.IsEnabled = true;
                textBoxBottomOffset.IsEnabled = true;
            }
        }

        private void TextBoxCurrentPage_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        /// <summary>
        /// Bitmap --> BitmapImage
        /// </summary>
        /// <param name="bitmap">Bitmap</param>
        /// <returns></returns>
        public static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png); // 坑点：格式选Bmp时，不带透明度

                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                // According to MSDN, "The default OnDemand cache option retains access to the stream until the image is needed."
                // Force the bitmap to load right now so we can dispose the stream.
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }


        /// <summary>
        /// BitmapImage --> Bitmap
        /// </summary>
        /// <param name="bitmapImage">BitmapImage</param>
        /// <returns></returns>
        public static Bitmap BitmapImageToBitmap(BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }
    }
}
