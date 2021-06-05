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
using System.Runtime.CompilerServices;

namespace PdfWaterMark
{
    public class PdfFileProcess: INotifyPropertyChanged
    {
        #region  私有字段
        // 处理文件夹下所有pdf文件
        private bool isSingleFile = false;
        // 不覆盖保存
        private bool isSaveReplace = false;
        // 仅处理首页
        private bool isOnlyFirtPage = true;
        // 不处理所有页面
        private bool isAllPages = false;
        // 印章是否等比例缩放
        private bool isProportional = false;
        // 源文件（夹）路径
        private string sourcePath = string.Empty;
        // 目标文件（夹）路径
        private string targetPath = string.Empty;
        // 印章文件路径
        private string markFilePath = string.Empty;
        // 要处理的工作页
        private string pages = string.Empty;
        // 源文件（夹）所有文件路径
        private List<string> sourceFilePathList = new List<string>();
        // 目标文件（夹）所有文件路径
        private List<string> targetFilePathList = new List<string>();
        // 印章水平对齐方式
        private HorizontalAlignment markHorizontalAlignment = HorizontalAlignment.Right;
        // 印章竖直对齐方式
        private VerticalAlignment markVerticalAlignment = VerticalAlignment.Bottom;
        // 水印宽度(mm)
        private double markWidth = 80.0;
        // 水印高度(mm)
        private double markHeight = 25.0;
        // 水印边距
        private Thickness margin = new Thickness(45.0, 20.0, 45.0, 20.0);

        private double pdfDpi = 72.0;
        private const double inch2mm = 25.4;
        private int previewPageNum;
        private int previewPageQty;

        private BitmapImage previewImage;

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
                NotifyPropertyChanged();
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
                targetFilePathList = SetTargetFilePathList(value, isSingleFile);
                targetPath = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// 目标文件（夹）所有文件路径
        /// </summary>
        public List<string> TargetFilePathList { get => targetFilePathList; }

        /// <summary>
        /// 印章文件路径
        /// </summary>
        public string MarkFilePath
        {
            get => markFilePath;
            set
            {
                if (Object.Equals(markFilePath, value))
                    return;
                markFilePath = value;
                SetPdfBitmap(value, MarkWidth, MarkHeight, IsProportional);
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// 仅处理单文件，否则处理文件夹下所有pdf文件
        /// </summary>
        public bool IsSingleFile
        {
            get => isSingleFile;
            set
            {
                if (Object.Equals(isSingleFile, value))
                    return;
                isSingleFile = value;
                NotifyPropertyChanged();
                TargetPath = "";
            }
        }

        /// <summary>
        /// 覆盖保存，否则另存到别处
        /// </summary>
        public bool IsSaveReplace
        {
            get => isSaveReplace;
            set { isSaveReplace = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// 仅处理首页
        /// </summary>
        public bool IsOnlyFirtPage
        {
            get => isOnlyFirtPage;
            set { isOnlyFirtPage = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// 处理所有页面
        /// </summary>
        public bool IsAllPages
        {
            get => isAllPages;
            set { isAllPages = value; NotifyPropertyChanged(); }
        }

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
                pages = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// 印章水平对齐方式
        /// </summary>
        public HorizontalAlignment MarkHorizontalAlignment
        {
            get => markHorizontalAlignment;
            set { markHorizontalAlignment = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// 印章竖直对齐方式
        /// </summary>
        public VerticalAlignment MarkVerticalAlignment
        {
            get => markVerticalAlignment;
            set { markVerticalAlignment = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// 水印宽度(mm)
        /// </summary>
        public double MarkWidth
        {
            get => markWidth;
            set { markWidth = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// 水印高度(mm)
        /// </summary>
        public double MarkHeight
        {
            get => markHeight;
            set { markHeight = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// 印章边距
        /// </summary>
        public Thickness Margin {
            get => margin;
            set { margin = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// 印章左边距
        /// </summary>
        public double MarginLeft
        {
            get => margin.Left;
            set { margin.Left = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// 印章上边距
        /// </summary>
        public double MarginTop
        {
            get => margin.Top;
            set { margin.Top = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// 印章右边距
        /// </summary>
        public double MarginRight
        {
            get => margin.Right;
            set { margin.Right = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// 印章下边距
        /// </summary>
        public double MarginBottom
        {
            get => margin.Bottom;
            set { margin.Bottom = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// 印章是否等比例缩放
        /// </summary>
        public bool IsProportional
        {
            get => isProportional;
            set { isProportional = value; NotifyPropertyChanged(); }
        }

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
                NotifyPropertyChanged();
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
                NotifyPropertyChanged();
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
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region 数据绑定

        /// <summary>
        /// 数据绑定事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// This method is called by the Set accessor of each property. The CallerMemberName attribute that is applied to the optional propertyName parameter causes the property name of the caller to be substituted as an argument.  
        /// </summary>
        /// <param name="propertyName"></param>
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region 公共方法
        public PdfFileProcess()
        {
            // PDF初始化
            PdfCommon.Initialize();
        }

        /// <summary>
        /// 对文件夹、文件添加印章
        /// </summary>
        /// <param name="sourcePath">源文件（夹）路径</param>
        /// <param name="targetPath">目标文件（夹）路径，为null则覆盖保存</param>
        /// <param name="pages">需要处理的页码，为null则处理全部</param>
        /// <param name="markFilePath">印章文件路径</param>
        /// <param name="markWidth">印象宽度（毫米）</param>
        /// <param name="markHeight">印象高度（毫米）</param>
        /// <param name="hAlign">水平布局</param>
        /// <param name="vAlign">竖直布局</param>
        /// <param name="margin">边距</param>
        /// <param name="isProportional">等比例缩放</param>
        /// <returns>已处理文件个数</returns>
        public int ImprintMark(string sourcePath, string targetPath,
                           string pages, string markFilePath,
                           double markWidth, double markHeight,
                           HorizontalAlignment hAlign, VerticalAlignment vAlign,
                           Thickness margin, bool isProportional = false)
        {
            // 需处理的文档总数
            int n = 0;
            // 处理过的文档总数
            int count = 0;

            // 源文件列表
            sourceFilePathList = GetSourceFilePathList(sourcePath);
            n = sourceFilePathList.Count();

            // 目标文档列表
            bool isSigleFile = File.Exists(sourcePath);
            if (targetPath != null)
                targetFilePathList = SetTargetFilePathList(targetPath, isSigleFile);

            // 指定页面列表
            List<int> pageList = new List<int>();
            bool isAllPages = false;
            if (pages is null)
                isAllPages = true;
            else
                pageList = IntArray(pages);

            // 设置印章
            using (PdfBitmap pdfBitmap = SetPdfBitmap(markFilePath, markWidth, markHeight, isProportional))
            {
                // 依次处理各个文件
                for (int d = 0; d < sourceFilePathList.Count; d++)
                {
                    using (var doc = PdfDocument.Load(sourceFilePathList[d]))
                    {
                        // 总页数
                        previewPageQty = doc.Pages.Count;
                        for (int i = 0; i < previewPageQty; i++)
                        {
                            if (!isAllPages && !pageList.Contains(i+1))
                                continue;
                            ImprintMarkOnPage(doc, doc.Pages[i], pdfBitmap, hAlign, vAlign, margin);
                        }

                        // 覆盖保存
                        if (targetPath is null)
                            doc.Save(SaveFlags.NoIncremental);
                        else // 换位置保存
                            doc.Save(targetFilePathList[d], SaveFlags.NoIncremental);
                    }
                }
            }

            // 返回处理的文档数
            return count;
        }

        /// <summary>
        /// 对文件夹、文件添加印章
        /// </summary>
        /// <param name="sourcePath">源文件（夹）路径</param>
        /// <param name="targetPath">目标文件（夹）路径，为null则覆盖保存</param>
        /// <param name="pages">需要处理的页码，为null则处理全部</param>
        /// <returns>已处理文件个数</returns>
        public int ImprintMark(string sourcePath, string targetPath = null, string pages = null)
        {
            return ImprintMark(sourcePath, targetPath, pages, markFilePath, markWidth, markHeight,
                               markHorizontalAlignment, markVerticalAlignment, margin, isProportional);        
        }


        /// <summary>
        /// 对文件夹、文件添加印章
        /// </summary>
        /// <returns>已处理文件个数</returns>
        public int ImprintMark()
        {
            string targetPath = this.targetPath;
            string pages = this.pages;
            if (isSaveReplace)
                targetPath = null;
            if (isOnlyFirtPage)
                pages = "1";
            else if (isAllPages)
                pages = null;
            return ImprintMark(sourcePath, targetPath, pages);
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

            try
            {
                // 添加文件
                // 如果是PDF文件
                if (File.Exists(sourcePath) && Path.GetExtension(sourcePath).ToLower() == ".pdf")
                {
                    sourceFilePathList.Add(Path.GetFullPath(sourcePath));
                }
                // 如果是文件夹
                else if (Directory.Exists(sourcePath))
                {
                    sourceFilePathList.AddRange(Directory.GetFiles(sourcePath, "*.pdf", SearchOption.AllDirectories));
                }
            }
            catch { }
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

            try
            {
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
            }
            catch { }
            return targetFilePathList;
        }

        /// <summary>
        /// 设置印章
        /// </summary>
        /// <param name="markFilePath">印章文件路径</param>
        /// <param name="markWidthMM">印象宽度（毫米）</param>
        /// <param name="markHeightMM">印象高度（毫米）</param>
        /// <param name="isProportional">等比例缩放</param>
        protected PdfBitmap SetPdfBitmap(string markFilePath, double markWidthMM, double markHeightMM, bool isProportional = false)
        {
            // 打开印章图片
            Bitmap bitmap = Bitmap.FromFile(markFilePath) as Bitmap;
            if (bitmap == null)
                throw new Exception("打开印章文件失败！");

            // 检查印章宽度和高度
            if (markWidthMM <= 0 || markHeightMM <= 0)
                throw new Exception("印章宽度和高度必须大于零!");

            // 目标PdfBitmap大小（像素）
            int markWidth = (int)(markWidthMM * pdfDpi / inch2mm);
            int markHeight = (int)(markHeightMM * pdfDpi / inch2mm);

            // 初始化PdfBitmap
            PdfBitmap pdfBitmap = new PdfBitmap(markWidth, markHeight, true);

            // 画印章
            using (var g = Graphics.FromImage(pdfBitmap.Image))
            {
                // 原图大小（像素）
                int bitmapWidth = bitmap.Width;
                int bitmapHeight = bitmap.Height;

                // 目标坐标和大小
                int posX = 0;
                int posY = 0;
                int width = markWidth;
                int height = markHeight;

                // 将左下角第一点定义为背景色
                g.Clear(bitmap.GetPixel(0,0));

                // 等比例缩放
                if (isProportional)
                {
                    double markWH = (double)markWidth / (double)markHeight;
                    double bitmapWH = (double)bitmapWidth / (double)bitmapHeight;
                    if (markWH > bitmapWH)
                    {
                        width = bitmapWidth * markHeight / bitmapHeight;
                        posX = (markWidth - width) / 2;
                    }
                    else
                    {
                        height = bitmapHeight * markWidth / bitmapWidth;
                        posY = (markHeight - height) / 2;
                    }
                }

                g.DrawImage(bitmap, posX, posY, width, height);
            }

            return pdfBitmap;
        }

        /// <summary>
        /// 工作页中添加印章
        /// </summary>
        /// <param name="doc">PdfDocument</param>
        /// <param name="page">PdfPage</param>
        /// <param name="pdfBitmap">PdfBitmap</param>
        /// <param name="hAlign">水平对齐方式</param>
        /// <param name="vAlign">垂直对齐方式</param>
        /// <param name="margin">边距</param>
        protected void ImprintMarkOnPage(PdfDocument doc, PdfPage page, PdfBitmap pdfBitmap, HorizontalAlignment hAlign, VerticalAlignment vAlign, Thickness margin)
        {
            try
            {
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

                // 页面像素宽度
                int width = (int)page.Width;
                int height = (int)page.Height;

                // 印章像素宽度
                int markWidth = pdfBitmap.Width;
                int markHeight = pdfBitmap.Height;

                // 边距（像素）
                int left = (int)(margin.Left * pdfDpi / inch2mm);
                int right = (int)(margin.Right * pdfDpi / inch2mm);
                int top = (int)(margin.Top * pdfDpi / inch2mm);
                int bottom = (int)(margin.Bottom * pdfDpi / inch2mm);

                // 偏移
                int offsetX = 0;
                int offsetY = 0;

                // X向偏移
                if (hAlign == HorizontalAlignment.Left)
                    offsetX = left;
                else if (hAlign == HorizontalAlignment.Right)
                    offsetX = width - markWidth - right;
                else
                    offsetX = (width - markWidth) / 2;
                // Y向偏移
                if (vAlign == VerticalAlignment.Top)
                    offsetY = height - markHeight - top;
                else if (vAlign == VerticalAlignment.Bottom)
                    offsetY = bottom;
                else
                    offsetY = (height - markHeight) / 2;

                // 页面添加图像
                PdfImageObject imageObject = PdfImageObject.Create(doc, pdfBitmap, 0, 0);
                imageObject.Matrix = new FS_MATRIX(markWidth, 0, 0, markHeight, offsetX, offsetY);
                page.PageObjects.Add(imageObject);

                // 生成页面内容
                page.GenerateContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 将以空格、逗号、分号隔开的数字分解成数组
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>数字数组</returns>
        protected static List<int> IntArray(string str)
        {
            List<int> ints = new List<int>();
            string[] strArray = str.Split(' ', ',', ';');
            foreach (string s in strArray)
            {
                if (int.TryParse(s, out int i) && i > 0)
                {
                    ints.Add(i);
                }
            }
            return ints;
        }
        #endregion
    }
}
