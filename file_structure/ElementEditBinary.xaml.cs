using kernel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public sealed partial class ElementEditBinary : ElementEditUserControl
    {
        public ElementEditBinary()
        {
            this.InitializeComponent();
        }

        protected override void LoadDataToUI()
        {
            InitTextBox(TextBox_Name, ElementKey.name);

            InitUI_Combo_Check(Combobox_RepeatMin,
                new List<string> { "0", "1" },
                ElementKey.repeatmin,
                "1",
                Checkbox_RepeatMin_Derived
                );

            InitUI_Combo_Check(Combobox_RepeatMax,
                new List<string> { "1", "Unlimited" },
                ElementKey.repeatmax,
                "1",
                Checkbox_RepeatMax_Derived,
                new OptionDictionaryRepeatMax()
                );

            // 1,2,3,4,8
            // 8,16,24,32,64
            LENGTH_UNIT lengthUnit = elementBase.lengthUnit;
            InitUI_Combo_Combo_Check(Combobox_Length,
                new List<string>(),
                ElementKey.length,
                "remaining",

                Combobox_Length_Unit,
                new List<string> { "Bytes", "Bits" },
                ElementKey.lengthunit,
                "byte",
                Checkbox_Length_Derived,

                new OptionDictionaryLength(),
                new OptionDictionaryLengthUnit()
                );

            InitFixedValues(DataGrid_FixedValues,
                Button_FixedValues_Add,
                Button_FixedValues_Remove,
                Checkbox_FixedValues_Derived,
                Checkbox_FixedValues_MustMatch
                );

            InitColorControlPair(Border_StrokeColor,
                Button_StrokeColor,
                Button_StrokeColor_Reset,
                Checkbox_StrokeColor_Derived,
                ElementKey.strokecolor,
                Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF)
                );
            InitColorControlPair(Border_FillColor,
                Button_FillColor,
                Button_FillColor_Reset,
                Checkbox_FillColor_Derived,
                ElementKey.fillcolor,
                Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));

            InitTextBox_WithValue(Textbox_Description, elementBase.description);
        }

       
        protected override void SaveUIToData()
        {
            // repeat min
            SaveUI_Combobox_CheckBox(Combobox_RepeatMin,
                Checkbox_RepeatMin_Derived,
                ElementKey.repeatmin
                );

            SaveUI_Combobox_CheckBox(Combobox_RepeatMax,
                Checkbox_RepeatMax_Derived,
                ElementKey.repeatmax
                );

            SaveUI_Combobox_Combobox_CheckBox(Combobox_Length,
                Combobox_Length_Unit,
                Checkbox_Length_Derived,
                ElementKey.length,
                ElementKey.lengthunit);

            SaveFixedValues(Checkbox_FixedValues_MustMatch, 
                Checkbox_FixedValues_Derived);

            elementBase.description = Textbox_Description.Text;
        }

        private void Combobox_Length_LostFocus(object sender, RoutedEventArgs e)
        {

        }
    }
}
