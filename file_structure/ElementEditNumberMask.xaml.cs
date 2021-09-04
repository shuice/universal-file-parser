using Common;
using kernel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public sealed partial class ElementEditNumberMask : ContentDialog, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<NameValuePair> fixedValues = new ObservableCollection<NameValuePair>();
        private string _name;
        private string _hex;
        private string _binary;

        private void SetHexNumber(UInt64 number, bool toHexEditBox, bool toBinaryEditBox)
        {
            if (toHexEditBox)
            {
                this.hex = number.ToString("X");
            }
            if (toBinaryEditBox)
            {
                this.binary = Convert.ToString((long)number, 2);
            }            
        }

        

        public string name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }


        public string hex
        {
            get => _hex;
            set
            {
                _hex = value;
                OnPropertyChanged();
            }
        }

        public string binary
        {
            get => _binary;
            set
            {
                _binary = value;
                OnPropertyChanged();
            }
        }

        private Mask inputMask;
        public ElementEditNumberMask(Mask inputMask)
        {
            this.inputMask = inputMask.Clone() as Mask;
            this.InitializeComponent();

            this.name = this.inputMask.name;
            UInt64 hexValue = this.inputMask.value.ToUInt64(ToolsExtention.EnumStringFormatType.hex).GetValueOrDefault(0);            
            SetHexNumber(hexValue, true, true);
            foreach(FixedValue fixedValue in this.inputMask.fixedValues)
            {
                fixedValues.Add(new NameValuePair() { name = fixedValue.name, value = fixedValue.value });
            }
        }

        public Mask result => inputMask;
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.inputMask.name = this.name;
            this.inputMask.value = this.hex;
            this.inputMask.fixedValues.Clear();
            foreach (NameValuePair nameValuePair in fixedValues)
            {
                this.inputMask.fixedValues.Add(new FixedValue() { name = nameValuePair.name, value = nameValuePair.value });
            }
        }

        private void TextBoxHex_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = TextBoxHex.Text;

            UInt64 hexValue = text.ToUInt64(ToolsExtention.EnumStringFormatType.hex).GetValueOrDefault(0);            
            SetHexNumber(hexValue, false, true);
        }

        private void TextBoxBinary_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = TextBoxBinary.Text;
            UInt64 hexValue = 0;
            try
            {
                hexValue = Convert.ToUInt64(text, 2);
            }
            catch(Exception)
            {

            }
            
            SetHexNumber(hexValue, true, false);
        }



       


        private void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            fixedValues.Add(new NameValuePair() { name = "<name>", value = "<value>" });
        }

        private void Button_Remove_Click(object sender, RoutedEventArgs e)
        {
            NameValuePair selected = DataGrid_FixedValues.SelectedItem as NameValuePair;
            if (selected != null)
            {
                fixedValues.Remove(selected);
            }
        }

        private void DataGrid_FixedValues_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Button_Remove.IsEnabled = (DataGrid_FixedValues.SelectedItem != null);
        }


        private bool IsVirualKey_0_9(Windows.System.VirtualKey key)
        {
            if ((key == Windows.System.VirtualKey.Number0)
                || (key == Windows.System.VirtualKey.Number1)
                || (key == Windows.System.VirtualKey.Number2)
                || (key == Windows.System.VirtualKey.Number3)
                || (key == Windows.System.VirtualKey.Number4)
                || (key == Windows.System.VirtualKey.Number5)
                || (key == Windows.System.VirtualKey.Number6)
                || (key == Windows.System.VirtualKey.Number7)
                || (key == Windows.System.VirtualKey.Number8)
                || (key == Windows.System.VirtualKey.Number9))
            {
                return true;
            }
            return false;
        }

        private bool IsVirualKey_0_9_numpad(Windows.System.VirtualKey key)
        {
            if ((key == Windows.System.VirtualKey.NumberPad0)
                || (key == Windows.System.VirtualKey.NumberPad1)
                || (key == Windows.System.VirtualKey.NumberPad2)
                || (key == Windows.System.VirtualKey.NumberPad3)
                || (key == Windows.System.VirtualKey.NumberPad4)
                || (key == Windows.System.VirtualKey.NumberPad5)
                || (key == Windows.System.VirtualKey.NumberPad6)
                || (key == Windows.System.VirtualKey.NumberPad7)
                || (key == Windows.System.VirtualKey.NumberPad8)
                || (key == Windows.System.VirtualKey.NumberPad9))
            {
                return true;
            }
            return false;
        }

        private bool IsVirualKey_A_F(Windows.System.VirtualKey key)
        {
            if ((key == Windows.System.VirtualKey.A)
                || (key == Windows.System.VirtualKey.B)
                || (key == Windows.System.VirtualKey.C)
                || (key == Windows.System.VirtualKey.D)
                || (key == Windows.System.VirtualKey.E)
                || (key == Windows.System.VirtualKey.F))
            {
                return true;
            }
            return false;
        }

        private bool IsBinaryNumberInput(Windows.System.VirtualKey key)
        {
            if ((key == Windows.System.VirtualKey.Number0)
                || (key == Windows.System.VirtualKey.NumberPad1)
                || (key == Windows.System.VirtualKey.Number0)
                || (key == Windows.System.VirtualKey.Number1))
            {
                return true;
            }
            return false;
        }

        private bool IsHexNumberInput(Windows.System.VirtualKey key)
        {
            if (IsVirualKey_0_9_numpad(key)
                || IsVirualKey_0_9(key)
                || IsVirualKey_A_F(key))
            {
                return true;
            }
            return false;
        }

        

        // 0-9 a-f A-F del backspace
        private void TextBoxHex_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            Windows.System.VirtualKey key = e.Key;
            if ((IsHexNumberInput(key) == false)
                && (key != Windows.System.VirtualKey.Delete)
                && (key != Windows.System.VirtualKey.Back)
                && (key != Windows.System.VirtualKey.Left)
                && (key != Windows.System.VirtualKey.Right)
                && (key != Windows.System.VirtualKey.Home)
                && (key != Windows.System.VirtualKey.End)
                )
            {
                e.Handled = true;
            }
        }

        private void TextBoxBinary_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            Windows.System.VirtualKey key = e.Key;
            if ((IsBinaryNumberInput(key) == false)
                && (key != Windows.System.VirtualKey.Delete)
                && (key != Windows.System.VirtualKey.Back)
                && (key != Windows.System.VirtualKey.Left)
                && (key != Windows.System.VirtualKey.Right)
                && (key != Windows.System.VirtualKey.Home)
                && (key != Windows.System.VirtualKey.End)
                )
            {
                e.Handled = true;
            }
        }
    }
}


