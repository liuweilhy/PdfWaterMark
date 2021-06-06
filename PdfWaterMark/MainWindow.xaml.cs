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
    public partial class MainWindow : Window
    {
        #region 属性
        /// <summary>
        /// PDF文件处理类
        /// </summary>
        public PdfFileProcess PdfFileProcess { get; set; }


        #endregion

        /// <summary>
        /// 数据绑定事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            // 创建PDF处理对象
            PdfFileProcess = new PdfFileProcess();
            // 解决TextBox小数点输入问题
            FrameworkCompatibilityPreferences.KeepTextBoxDisplaySynchronizedWithTextProperty = false;
            // 初始化控件
            InitializeComponent();

            #region 数据绑定
            DataContext = PdfFileProcess;

            // 绑定的操作由xmal完成
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
                // 禁用界面控件
                groupbox1.IsEnabled = false;
                groupbox2.IsEnabled = false;
                groupbox3.IsEnabled = false;
                viewbox1.IsEnabled = false;
                grid1.IsEnabled = false;
                // 启动处理
                PdfFileProcess.ImprintMark();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // 启用界面控件
                groupbox1.IsEnabled = true;
                groupbox2.IsEnabled = true;
                groupbox3.IsEnabled = true;
                viewbox1.IsEnabled = true;
                grid1.IsEnabled = true;
            }
        }

        private void ButtonAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }

        private void ButtonPrev_Click(object sender, RoutedEventArgs e)
        {
            if (PdfFileProcess.PreviewPageNum > 1)
                PdfFileProcess.PreviewPageNum--;
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            if (PdfFileProcess.PreviewPageNum < PdfFileProcess.PreviewPageQty)
                PdfFileProcess.PreviewPageNum++;
        }

        private void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog()
            {
                AddToMostRecentlyUsedList = true,
                Multiselect = false
            };

            if (PdfFileProcess.IsSingleFile)
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
                PdfFileProcess.SourcePath = dialog.FileName;
            }
        }

        private void ButtonSaveAs_Click(object sender, RoutedEventArgs e)
        {
            if (PdfFileProcess.IsSingleFile)
            {
                CommonSaveFileDialog dialog = new CommonSaveFileDialog()
                {
                    Title = "另存为",
                    AddToMostRecentlyUsedList = true
                };
                dialog.Filters.Add(new CommonFileDialogFilter("PDF文件", "*.pdf"));
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string path = dialog.FileName;
                    // 如果没有pdf扩展名，则加上扩展名
                    if (!path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                        path += ".pdf";
                    PdfFileProcess.TargetPath = path;
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
                    PdfFileProcess.TargetPath = dialog.FileName;
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
            dialog.Filters.Add(new CommonFileDialogFilter("图像文件", "*.png;*.jpg;*.bmp"));
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                PdfFileProcess.MarkFilePath = dialog.FileName;
            }
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == radioButtonCenter)
            {
                PdfFileProcess.MarkHorizontalAlignment = HorizontalAlignment.Center;
                PdfFileProcess.MarkVerticalAlignment = VerticalAlignment.Center;
                textBoxLeftOffset.IsEnabled = false;
                textBoxTopOffset.IsEnabled = false;
                textBoxRightOffset.IsEnabled = false;
                textBoxBottomOffset.IsEnabled = false;
            }
            else if (sender == radioButtonTopLeft)
            {
                PdfFileProcess.MarkHorizontalAlignment = HorizontalAlignment.Left;
                PdfFileProcess.MarkVerticalAlignment = VerticalAlignment.Top;
                textBoxLeftOffset.IsEnabled = true;
                textBoxTopOffset.IsEnabled = true;
                textBoxRightOffset.IsEnabled = false;
                textBoxBottomOffset.IsEnabled = false;
            }
            else if (sender == radioButtonTopRight)
            {
                PdfFileProcess.MarkHorizontalAlignment = HorizontalAlignment.Right;
                PdfFileProcess.MarkVerticalAlignment = VerticalAlignment.Top;
                textBoxLeftOffset.IsEnabled = false;
                textBoxTopOffset.IsEnabled = true;
                textBoxRightOffset.IsEnabled = true;
                textBoxBottomOffset.IsEnabled = false;
            }
            else if (sender == radioButtonBottomLeft)
            {
                PdfFileProcess.MarkHorizontalAlignment = HorizontalAlignment.Left;
                PdfFileProcess.MarkVerticalAlignment = VerticalAlignment.Bottom;
                textBoxLeftOffset.IsEnabled = true;
                textBoxTopOffset.IsEnabled = false;
                textBoxRightOffset.IsEnabled = false;
                textBoxBottomOffset.IsEnabled = true;
            }
            else if (sender == radioButtonBottomRight)
            {
                PdfFileProcess.MarkHorizontalAlignment = HorizontalAlignment.Right;
                PdfFileProcess.MarkVerticalAlignment = VerticalAlignment.Bottom;
                textBoxLeftOffset.IsEnabled = false;
                textBoxTopOffset.IsEnabled = false;
                textBoxRightOffset.IsEnabled = true;
                textBoxBottomOffset.IsEnabled = true;
            }
            else if (sender == radioButtonTopCenter)
            {
                PdfFileProcess.MarkHorizontalAlignment = HorizontalAlignment.Center;
                PdfFileProcess.MarkVerticalAlignment = VerticalAlignment.Top;
                textBoxLeftOffset.IsEnabled = false;
                textBoxTopOffset.IsEnabled = true;
                textBoxRightOffset.IsEnabled = false;
                textBoxBottomOffset.IsEnabled = false;
            }
            else if (sender == radioButtonRightCenter)
            {
                PdfFileProcess.MarkHorizontalAlignment = HorizontalAlignment.Right;
                PdfFileProcess.MarkVerticalAlignment = VerticalAlignment.Center;
                textBoxLeftOffset.IsEnabled = false;
                textBoxTopOffset.IsEnabled = false;
                textBoxRightOffset.IsEnabled = true;
                textBoxBottomOffset.IsEnabled = false;
            }
            else if (sender == radioButtonBottomCenter)
            {
                PdfFileProcess.MarkHorizontalAlignment = HorizontalAlignment.Center;
                PdfFileProcess.MarkVerticalAlignment = VerticalAlignment.Bottom;
                textBoxLeftOffset.IsEnabled = false;
                textBoxTopOffset.IsEnabled = false;
                textBoxRightOffset.IsEnabled = false;
                textBoxBottomOffset.IsEnabled = true;
            }
            else if (sender == radioButtonLeftCenter)
            {
                PdfFileProcess.MarkHorizontalAlignment = HorizontalAlignment.Left;
                PdfFileProcess.MarkVerticalAlignment = VerticalAlignment.Center;
                textBoxLeftOffset.IsEnabled = true;
                textBoxTopOffset.IsEnabled = false;
                textBoxRightOffset.IsEnabled = false;
                textBoxBottomOffset.IsEnabled = false;
            }
        }

    }
}
