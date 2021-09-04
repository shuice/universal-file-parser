using System;
using System.Collections.Generic;
using System.Diagnostics;
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

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace file_structure
{
    public sealed partial class NewGrammarContentDialog : ContentDialog
    {


        public string errMsg
        {
            get { return (string)GetValue(errMsgProperty); }
            set { SetValue(errMsgProperty, value); }
        }

        // Using a DependencyProperty as the backing store for errMsg.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty errMsgProperty =
            DependencyProperty.Register("errMsg", typeof(string), typeof(NewGrammarContentDialog), new PropertyMetadata(""));




        public string fileName
        {
            get { return (string)GetValue(fileNameProperty); }
            set { SetValue(fileNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for fileName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty fileNameProperty =
            DependencyProperty.Register("fileName", typeof(string), typeof(NewGrammarContentDialog), new PropertyMetadata(""));





        private Func<string, string> funcFileNameVerify;
        public NewGrammarContentDialog(string fileName, Func<string, string> funcFileNameVerify)
        {
            this.fileName = fileName;
            this.funcFileNameVerify = funcFileNameVerify;
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            string error = funcFileNameVerify(fileName);
            if (error.Length > 0)
            {
                errMsg = error;
                args.Cancel = true;
                return;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string error = funcFileNameVerify(((TextBox)sender).Text);
            errMsg = error;
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox_FileName.SelectAll();
        }
    }
}
