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

namespace PdfWaterMark
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region  私有变量

        private List<string> sourceFilePathList = new List<string>();

        private List<string> destinationFilePathList = new List<string>();

        private List<int> pageList = new List<int>();

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
        public int MarkWidth { get; set; } = 10;

        /// <summary>
        /// 水印高度
        /// </summary>
        public int MarkHeight { get; set; } = 10;

        /// <summary>
        /// 水印左边距
        /// </summary>
        public int MarkMarginLeft { get; set; } = 10;

        /// <summary>
        /// 水印上边距
        /// </summary>
        public int MarkMarginTop { get; set; } = 10;

        /// <summary>
        /// 水印右边距
        /// </summary>
        public int MarkMarginRight { get; set; } = 10;

        /// <summary>
        /// 水印下边距
        /// </summary>
        public int MarkMarginBottom { get; set; } = 10;


        /// <summary>
        /// 要处理的工作页
        /// </summary>
        public string Pages { get; set; }

        /// <summary>
        /// 当前预览页
        /// </summary>
        public int PreviewPageNum { get; set; } = 0;

        /// <summary>
        /// 当前文档总页数预览页
        /// </summary>
        public int PreviewPageQty { get; set; } = 0;

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
            dialog.Filters.Add(new CommonFileDialogFilter("图像文件", "*.jpg,*.png,*.bmp"));
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                textBoxMarkPath.Text = dialog.FileName;
            }
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == toggleButtonBottomCenter)
            {

            }
        }

        private void TextBoxCurrentPage_TextChanged(object sender, TextChangedEventArgs e)
        {

        }


    }
}
