using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class SettingTitleUserControl : UserControl
    {

        public bool lineColorIsImport { get; set; } = true;



        public SolidColorBrush lineColorBrush
        {
            get
            {
                if (lineColorIsImport)
                {
                    return new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, 0xFF, 0xA5, 0x00));
                }
                return new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, 0x43, 0xCD, 0x80));
            }
        }





        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(SettingTitleUserControl), new PropertyMetadata(""));




        public SettingTitleUserControl()
        {
            this.InitializeComponent();
        }
    }
}
