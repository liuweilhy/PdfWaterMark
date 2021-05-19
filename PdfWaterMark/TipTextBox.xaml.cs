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

namespace PdfWaterMark
{
    /// <summary>
    /// TipTextBox.xaml 的交互逻辑
    /// </summary>
    public partial class TipTextBox : TextBox
    {
        public TipTextBox()
        {
            InitializeComponent();
        }

        public string TipText
        {
            get { return (string)GetValue(TipTextProperty); }
            set { SetValue(TipTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TipText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TipTextProperty =
            DependencyProperty.Register("TipText", typeof(string), typeof(TipTextBox), new PropertyMetadata(""));   
    }
}
