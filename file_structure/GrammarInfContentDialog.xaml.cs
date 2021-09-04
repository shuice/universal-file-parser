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

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace file_structure
{
    public sealed partial class GrammarInfContentDialog : ContentDialog
    {
        public GrammarInfContentDialog()
        {
            this.InitializeComponent();
        }



        public string grammarName
        {
            get { return (string)GetValue(grammarNameProperty); }
            set { SetValue(grammarNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for grammarName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty grammarNameProperty =
            DependencyProperty.Register("grammarName", typeof(string), typeof(GrammarInfContentDialog), new PropertyMetadata(""));




        public string author
        {
            get { return (string)GetValue(authorProperty); }
            set { SetValue(authorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for author.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty authorProperty =
            DependencyProperty.Register("author", typeof(string), typeof(GrammarInfContentDialog), new PropertyMetadata(""));





        public string email
        {
            get { return (string)GetValue(emailProperty); }
            set { SetValue(emailProperty, value); }
        }

        // Using a DependencyProperty as the backing store for email.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty emailProperty =
            DependencyProperty.Register("email", typeof(string), typeof(GrammarInfContentDialog), new PropertyMetadata(""));




        public string extension
        {
            get { return (string)GetValue(extensionProperty); }
            set { SetValue(extensionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for extension.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty extensionProperty =
            DependencyProperty.Register("extension", typeof(string), typeof(GrammarInfContentDialog), new PropertyMetadata(""));




        public string description
        {
            get { return (string)GetValue(descriptionProperty); }
            set { SetValue(descriptionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for description.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty descriptionProperty =
            DependencyProperty.Register("description", typeof(string), typeof(GrammarInfContentDialog), new PropertyMetadata(""));



        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox_GrammarName.SelectAll();
        }
    }
}
