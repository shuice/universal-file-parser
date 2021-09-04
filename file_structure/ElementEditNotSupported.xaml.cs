using Common;
using kernel;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace file_structure
{
    public sealed partial class ElementEditNotSupported : ElementEditUserControl
    {
        
        public ElementEditNotSupported()
        {
            this.InitializeComponent();
        }


        

        protected override void LoadDataToUI()
        {
            TextBlock_UnsupportedTip.Text = $"Unsupported Type:{elementBase.GetElementTypeUI()}";
        }


 

        private void Textbox_Name_GotFocus(object sender, RoutedEventArgs e)
        {
        }

        protected override void SaveUIToData()
        {
           
        }
    }
}
