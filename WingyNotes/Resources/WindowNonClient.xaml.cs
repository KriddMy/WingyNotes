using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Controls;


namespace WingyNotes
{
    public partial class WindowNoClientResources : ResourceDictionary
    {
        public const int CaptionHight = 32;
        public WindowNoClientResources()
        {
            InitializeComponent();
        }
    }
}
