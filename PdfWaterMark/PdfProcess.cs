using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace PdfWaterMark
{
    class PdfProcess
    {
        #region  私有变量

        private List<string> sourceFilePathList = new List<string>();
        private List<string> destinationFilePathList = new List<string>();
        private List<int> pageList = new List<int>();

        private int previewPageNum;
        private int previewPageQty;

        private BitmapImage previewImage;

        #endregion
    }
}
