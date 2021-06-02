using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using Patagames.Pdf;
using Patagames.Pdf.Net;
using Patagames.Pdf.Enums;
using System.Windows;
using System.ComponentModel;

namespace PdfWaterMark
{
    public class PdfFileProcess: INotifyPropertyChanged
    {
        #region  私有变量
        private string sourcePath;
        private string targetPath;
        private string pages;
        private List<string> sourceFilePathList;
        private List<string> targetFilePathList;
        private List<int> pageList;
        private bool isSigleFile;
        private BitmapImage previewImage;
		private PdfBitmap markBitmap;
        private Thickness margin;
        private double pdfDpi = 72.0;
        private const double inch2mm = 25.4;
        private FS_MATRIX markMatrix;
        private int previewPageNum;
        private int previewPageQty;

        #endregion

        #region  属性
        /// <summary>
        /// 源文件（夹）路径
        /// </summary>
        public string SourcePath
        {
            get => sourcePath;
            set
            {
                if (Object.Equals(sourcePath, value))
                    return;
                sourceFilePathList = GetSourceFilePathList(value);
                sourcePath = value;
            }
        }

        /// <summary>
        /// 源文件（夹）所有文件路径
        /// </summary>
        public List<string> SourceFilePathList { get => sourceFilePathList; }

        /// <summary>
        /// 目标文件（夹）路径
        /// </summary>
        public string TargetPath
        {
            get => targetPath;
            set
            {
                if (Object.Equals(targetPath, value))
                    return;
                targetFilePathList = SetTargetFilePathList(value, isSigleFile);
                targetPath = value;
            }
        }

        /// <summary>
        /// 目标文件（夹）所有文件路径
        /// </summary>
        public List<string> TargetFilePathList { get => targetFilePathList; }


        /// <summary>
        /// 仅处理单文件，否则处理文件夹下所有pdf文件
        /// </summary>
        public bool IsSigleFile
        {
            get => isSigleFile;
            set
            {
                if (Object.Equals(isSigleFile, value))
                    return;
                targetFilePathList = SetTargetFilePathList(targetPath, value);
                isSigleFile = value;
            }
        }

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
        /// 要处理的工作页
        /// </summary>
        public string Pages
        {
            get => pages;
            set
            {
                if (Object.Equals(pages, value))
                    return;
                string[] strArray = value.Split(' ', ',', ';');
                pageList.Clear();
                pages = string.Empty;
                foreach(string str in strArray)
                {
                    if (int.TryParse(str, out int i) && i > 0)
                    {
                        pageList.Add(i);
                        pages += i.ToString() + ',';
                    }
                }
                pages.TrimEnd(',');
            }
        }

        /// <summary>
        /// 印章文件路径
        /// </summary>
        public string MarkFilePath { get; set; }

        /// <summary>
        /// 印章水平对齐方式
        /// </summary>
        public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Right;

        /// <summary>
        /// 印章竖直对齐方式
        /// </summary>
        public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Bottom;

        /// <summary>
        /// 水印宽度(mm)
        /// </summary>
        public double MarkWidth { get; set; } = 3.2;

        /// <summary>
        /// 水印高度(mm)
        /// </summary>
        public double MarkHeight { get; set; } = 7.2;

        /// <summary>
        /// 印章边距
        /// </summary>
        public Thickness Margin {
            get => margin;
            set => margin = value;
        }

        /// <summary>
        /// 印章左边距
        /// </summary>
        public double MarginLeft
        {
            get => margin.Left;
            set => margin.Left = value;
        }

        /// <summary>
        /// 印章上边距
        /// </summary>
        public double MarginTop
        {
            get => margin.Top;
            set => margin.Top = value;
        }

        /// <summary>
        /// 印章右边距
        /// </summary>
        public double MarginRight
        {
            get => margin.Right;
            set => margin.Right = value;
        }

        /// <summary>
        /// 印章下边距
        /// </summary>
        public double MarginBottom
        {
            get => margin.Bottom;
            set => margin.Bottom = value;
        }

        /// <summary>
        /// 印章是否等比例缩放
        /// </summary>
        public bool IsProportional { get; set; } = false;

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

        /// <summary>
        /// 预览图
        /// </summary>
        public BitmapImage PreviewImage
        {
            get => previewImage;
            set
            {
                previewImage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PreviewImage)));
            }
        }

        #endregion

        /// <summary>
        /// 数据绑定事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #region 公共方法
        public PdfFileProcess()
        {
            // PDF初始化
            PdfCommon.Initialize();
            // 初始边距
            margin = new Thickness(4.5, 2.0, 4.5, 2.0);
            // 指定页面列表
            pageList = new List<int>();
            IsSigleFile = false;
        }

        /// <summary>
        /// 对文件夹、文件添加印章
        /// </summary>
        /// <param name="sourcePath">源文件（夹）路径</param>
        /// <param name="targetPath">目标文件（夹）路径，为null则覆盖保存</param>
        /// <param name="markPath">印章文件路径</param>
        /// <param name="markWidth">印象宽度（毫米）</param>
        /// <param name="markHeight">印象高度（毫米）</param>
        /// <param name="hAlign">水平布局</param>
        /// <param name="vAlign">竖直布局</param>
        /// <param name="margin">边距</param>
        /// <param name="isProportional">等比例缩放</param>
        /// <returns>已处理文件个数</returns>
        public int ImprintMark(string sourcePath, string targetPath,
                           string markPath, double markWidth, double markHeight,
                           HorizontalAlignment hAlign, VerticalAlignment vAlign,
                           Thickness margin, bool isProportional = false)
        {
            SetMark(markPath, markWidth, markHeight, hAlign, vAlign, margin, isProportional);
            return ImprintMark(sourcePath, targetPath);
        }

        /// <summary>
        /// 对文件夹、文件添加印章
        /// </summary>
        /// <param name="sourcePath">源文件（夹）路径</param>
        /// <param name="targetPath">目标文件（夹）路径，为null则覆盖保存</param>
        /// <returns>已处理文件个数</returns>
        public int ImprintMark(string sourcePath, string targetPath = null)
        {
            int count = 0;

            return count;        
        }

        /// <summary>
        /// Bitmap --> BitmapImage
        /// </summary>
        /// <param name="bitmap">Bitmap</param>
        /// <returns>BitmapImage</returns>
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
        /// <returns>Bitmap</returns>
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
        #endregion

        #region 内部方法
        /// <summary>
        /// 获取PDF文件路径列表
        /// </summary>
        /// <param name="sourcePath">源文件（夹）路径</param>
        /// <returns>PDF文件路径列表</returns>
        protected List<string> GetSourceFilePathList(string sourcePath)
        {
            // 文件列表
            List<string> sourceFilePathList = new List<string>();

            // 添加文件
            // 如果是PDF文件
            if (File.Exists(sourcePath) && Path.GetExtension(sourcePath).ToLower() == "pdf")
            {
                sourceFilePathList.Add(Path.GetFullPath(sourcePath));
            }
            // 如果是文件夹
            else if (Directory.Exists(sourcePath))
            {
                sourceFilePathList.AddRange(Directory.GetFiles(sourcePath, "*.pdf", SearchOption.AllDirectories));
            }
            return sourceFilePathList;
        }

        /// <summary>
        /// 设置新的PDF文件路径列表
        /// </summary>
        /// <param name="targetPath">目标文件（夹）路径</param>
        /// <param name="isFile">目标是文件还是文件夹</param>
        /// <returns>新的PDF文件路径列表</returns>
        protected List<string> SetTargetFilePathList(string targetPath, bool isFile = true)
        {
            // 文件列表
            List<string> targetFilePathList = new List<string>();

            // 添加文件
            // 如果是PDF文件
            if (isFile)
            {
                targetFilePathList.Add(Path.GetFullPath(targetPath));
            }
            // 是文件夹
            else
            {
                targetPath = targetPath.TrimEnd('\\') + '\\';
                foreach (var path in sourceFilePathList)
                {
                    targetFilePathList.Add(targetPath + Path.GetFileName(path));
                }
            }
            return targetFilePathList;
        }

        /// <summary>
        /// 设置印章
        /// </summary>
        /// <param name="markPath">印章文件路径</param>
        /// <param name="markWidth">印象宽度（毫米）</param>
        /// <param name="markHeight">印象高度（毫米）</param>
        /// <param name="hAlign">水平布局</param>
        /// <param name="vAlign">竖直布局</param>
        /// <param name="margin">边距</param>
        /// <param name="isProportional">等比例缩放</param>
        protected void SetMark(string markPath, double markWidth, double markHeight,
                            HorizontalAlignment hAlign, VerticalAlignment vAlign,
                            Thickness margin, bool isProportional = false)
        {
            // 打开印章图片
            Bitmap bitmap = Bitmap.FromFile(markPath) as Bitmap;
            if (bitmap == null)
                throw new Exception("打开印章文件失败！");

            // 检查印章宽度和高度
            if (markWidth <= 0 || markHeight <= 0)
                throw new Exception("印章宽度和高度必须大于零!");

            // 初始化PdfBitmap
            markBitmap?.Dispose();
            int markWidthPixel = (int)(markWidth / inch2mm * pdfDpi);
            int markHeightPixel = (int)(markHeight / inch2mm * pdfDpi);
            markBitmap = new PdfBitmap(markWidthPixel, markHeightPixel, true);

            // 画印章
            using (var g = Graphics.FromImage(markBitmap.Image))
            {
                g.Clear(Color.White);
                g.DrawImage(bitmap, 0, 0, markWidthPixel, markHeightPixel);
            }

            // 设置边距
            this.Margin = margin;
        }

        #endregion
    }
}
