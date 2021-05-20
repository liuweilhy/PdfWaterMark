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
using Microsoft.WindowsAPICodePack.Dialogs;

namespace PdfWaterMark
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region  私有变量
        /// <summary>
        /// 仅处理单文件，否则处理文件夹下所有pdf文件
        /// </summary>
        private bool isSigleFile = false;

        /// <summary>
        /// 覆盖保存，否则另存到别处
        /// </summary>
        private bool isSaveReplace = false;

        /// <summary>
        /// 仅打印首页
        /// </summary>
        private bool isOnlyFirtPage = true;

        /// <summary>
        /// 打印所有页面
        /// </summary>
        private bool isAllPages = false;

        /// <summary>
        /// 水印是否缩放
        /// </summary>
        private bool isScale = false;

        /// <summary>
        /// 水印是否铺满
        /// </summary>
        private bool isFull = false;

        /// <summary>
        /// 源文件（夹）路径
        /// </summary>
        private string sourceFilePath;
        private List<string> sourceFilePathList = new List<string>();

        /// <summary>
        /// 目标文件（夹）路径
        /// </summary>
        private string destinationFilePath;
        private List<string> destinationFilePathList = new List<string>();

        /// <summary>
        /// 要处理的工作页
        /// </summary>
        private string page;
        private List<int> pageList = new List<int>();

        /// <summary>
        /// 当前预览页
        /// </summary>
        private int previewPageNum = 0;

        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            // 更新数据
            if (!UpdateData())
                return;

            ;
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
            UpdateData();
            CommonOpenFileDialog dialog = new CommonOpenFileDialog()
            {
                AddToMostRecentlyUsedList = true,
                Multiselect = false
            };

            if (isSigleFile)
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
            UpdateData();
            if (isSigleFile)
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
            UpdateData();
            CommonOpenFileDialog dialog = new CommonOpenFileDialog()
            {
                Title = "打开印章文件",
                AddToMostRecentlyUsedList = true,
            };
            dialog.Filters.Add(new CommonFileDialogFilter("图像文件", "*.jpg,*.png,*.bmp"));
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                textBoxWaterMarkPath.Text = dialog.FileName;
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

        private void RadioButtonSave_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (radioButtonSaveAs.IsChecked == true)
                {
                    textBoxNewPath.IsEnabled = true;
                    buttonSaveAs.IsEnabled = true;
                }
                else
                {
                    textBoxNewPath.IsEnabled = false;
                    buttonSaveAs.IsEnabled = false;
                }
            }
            catch { }
        }

        private void RadioButtonPage_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (radioButtonPageSet.IsChecked == true)
                    textBoxPages.IsEnabled = true;
                else
                    textBoxPages.IsEnabled = false;
            }
            catch { }
        }

        /// <summary>
        /// 根据显示刷新内容
        /// </summary>
        private bool UpdateData()
        {
            try
            {
                // 处理文件还是文件夹
                if (radioButtonFile.IsChecked == true)
                    isSigleFile = true;
                else
                    isSigleFile = false;

                sourceFilePath = textBoxPath.Text;

                // 另存为还是覆盖保存
                if (radioButtonSaveReplace.IsChecked == true)
                    isSaveReplace = true;
                else
                    isSaveReplace = false;

                destinationFilePath = textBoxNewPath.Text;

                // 要处理的页面
                if (radioButtonPage1.IsChecked == true)
                {
                    isOnlyFirtPage = true;
                    isAllPages = false;
                }
                else if (radioButtonPageAll.IsChecked == true)
                {
                    isOnlyFirtPage = false;
                    isAllPages = true;
                }
                else
                {
                    isOnlyFirtPage = false;
                    isAllPages = false;
                    page = textBoxPages.Text;
                }

                // 水印缩放
                if (radioButtonCenter.IsChecked == true)
                {
                    isScale = false;
                    isFull = false;
                }
                else if (radioButtonScale.IsChecked == true)
                {
                    isScale = true;
                    isFull = false;
                }
                else
                {
                    isScale = false;
                    isFull = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

    }
}
