using kernel;
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
    public sealed partial class ContentDialogNewScript : ContentDialog
    {
        private bool canChangeType;
        public ContentDialogNewScript(bool canChangeType)
        {
            this.canChangeType = canChangeType;
            this.InitializeComponent();
        }



        public string name
        {
            get { return (string)GetValue(nameProperty); }
            set { SetValue(nameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for name.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty nameProperty =
            DependencyProperty.Register("name", typeof(string), typeof(ContentDialogNewScript), new PropertyMetadata(""));




        public string language
        {
            get { return (string )GetValue(languageProperty); }
            set { SetValue(languageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for language.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty languageProperty =
            DependencyProperty.Register("language", typeof(string ), typeof(ContentDialogNewScript), new PropertyMetadata(""));




        public string type
        {
            get { return (string)GetValue(typeProperty); }
            set { SetValue(typeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for type.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty typeProperty =
            DependencyProperty.Register("type", typeof(string), typeof(ContentDialogNewScript), new PropertyMetadata(""));




        public string description
        {
            get { return (string)GetValue(descriptionProperty); }
            set { SetValue(descriptionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for description.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty descriptionProperty =
            DependencyProperty.Register("description", typeof(string), typeof(ContentDialogNewScript), new PropertyMetadata(""));



        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            language = Combobox_GetValue(Combobox_Language);
            type = Combobox_GetValue(Combobox_Type);
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            Combobox_Language.IsEnabled = canChangeType;
            Combobox_Type.IsEnabled = false;
            InitUI_Combo_WithValue(Combobox_Language,
                 new List<string> { "Python", "Lua" },
                 language,
                 "Python",
                 null
                 );

            InitUI_Combo_WithValue(Combobox_Type,
                new List<string>() { "Data Type" },
                type,
                Grammar.script_type_datatype,
                new OptionDictionaryScriptType()
                );

            TextBox_Name.SelectAll();
        }

        public void InitUI_Combo_WithValue(ComboBox combobox,
            List<String> options,
            string value,
            string defalut_value,
            OptionDictionary optionDictionary = null
            )
        {
            if (String.IsNullOrEmpty(value))
            {
                value = defalut_value;
            }
            if (optionDictionary != null)
            {
                value = optionDictionary.Value2Ui(value);
            }
            combobox.Tag = optionDictionary;
            combobox.Items.Clear();
            options.ForEach(item => combobox.Items.Add(new ComboBoxItem() { Content = item }));
            SetComboboxSelected(combobox, value);
        }

        public void SetComboboxSelected(ComboBox combobox, String selected)
        {
            ComboBoxItem selectedItem = combobox.Items.Select(item => (ComboBoxItem)item)
                                                        .Where(item => ((item.Content as String) == selected))
                                                        .FirstOrDefault();
            if (selectedItem != null)
            {
                combobox.SelectedItem = selectedItem;
            }
            else
            {
                ComboBoxItem item = new ComboBoxItem() { Content = selected };
                combobox.Items.Add(item);
                combobox.SelectedItem = item;
            }
        }

        public string ComboboxSelectedText(ComboBox combobox)
        {
            String text = combobox.SelectedItem as String;
            ComboBoxItem item = combobox.SelectedItem as ComboBoxItem;
            string value = "";
            if (text != null)
            {
                value = text;
            }
            else if (item != null)
            {
                value = item.Content as String;
            }
            return value ?? "";
        }

        public string Combobox_GetValue(ComboBox combobox)
        {
            string value = ComboboxSelectedText(combobox);            
            OptionDictionary optionDictionary = combobox.Tag as OptionDictionary;
            if (optionDictionary != null)
            {
                value = optionDictionary.Ui2Value(value);
            }
            return value;
        }

    }
}
