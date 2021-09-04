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
    public sealed partial class ElementEditNumber : ElementEditUserControl
    {
        
        public ElementEditNumber()
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

            InitUI_Combo_Check(Combobox_Type,                
                new List<string> { "Integer", "Floating Point" },
                ElementKey.type,
                "integer",
                Checkbox_Type_Derived,
                new OptionDictionaryNumberType()
                );

            // 1,2,3,4,8
            // 8,16,24,32,64
            LENGTH_UNIT lengthUnit = elementBase.lengthUnit;
            InitUI_Combo_Combo_Check(Combobox_Length,                
                OptionsNumberLength(elementBase.lengthUnit),
                ElementKey.length,
                "4",

                Combobox_Length_Unit,
                new List<string> { "Bytes", "Bits" },
                ElementKey.lengthunit,
                "byte",
                Checkbox_Length_Derived,

                null,
                new OptionDictionaryLengthUnit()
                );

            InitUI_Combo_Check_endianness_signed_encoding_debug(
                Combobox_Endianness,
                new List<string> { "Big", "Little", "Dynamic" },
                ElementKey.endian,
                "big",
                Checkbox_Endianness_Derived,
                new OptionDictionaryEndianness()
                );

            InitUI_Combo_Check_endianness_signed_encoding_debug(
                Combobox_Signed,
                new List<string> { "No", "Yes"},
                ElementKey.signed,
                "no",
                Checkbox_Signed_Derived,
                new OptionDictionaryYesNo()
                );

            InitUI_Combo_Check(Combobox_Display,
                new List<string> { "Decimal", "Hexadecimal", "Octal", "Binary" },
                ElementKey.display,
                "decimal",
                Checkbox_Display_Derived,
                new OptionDictionaryDisplay()
                );

            InitUI_Textbox_Check(Textbox_MinValue,
                ElementKey.minval,
                Checkbox_MinValue_Derived,
                "");

            InitUI_Textbox_Check(Textbox_MaxValue,
                ElementKey.maxval,
                Checkbox_MaxValue_Derived,
                "");

            InitUI_Textbox_Check(Textbox_Value,
                ElementKey.valueexpression,
                Checkbox_Value_Derived,
                "");

            InitFixedValues(DataGrid_FixedValues,
                Button_FixedValues_Add,
                Button_FixedValues_Remove,
                Checkbox_FixedValues_Derived,
                Checkbox_FixedValues_MustMatch
                );

            InitMasks(DataGrid_Masks,
                Button_Masks_Add,
                Button_Masks_Remove,
                Button_Masks_Edit,
                Checkbox_Masks_Derived
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


 

        private void Textbox_Name_GotFocus(object sender, RoutedEventArgs e)
        {
        }

        protected override void SaveUIToData()
        {
            SaveUI_Combobox_CheckBox(Combobox_RepeatMin,
                                    Checkbox_RepeatMin_Derived,
                                    ElementKey.repeatmin
                                    );

            SaveUI_Combobox_CheckBox(Combobox_RepeatMax,
                                    Checkbox_RepeatMax_Derived,
                                    ElementKey.repeatmax
                                    );

            SaveUI_Combobox_CheckBox(Combobox_Type,
                                    Checkbox_Type_Derived,
                                    ElementKey.type);

            SaveUI_Combobox_Combobox_CheckBox(Combobox_Length,
                Combobox_Length_Unit,
                Checkbox_Length_Derived,
                ElementKey.length,
                ElementKey.lengthunit);


            SaveUI_Combobox_CheckBox_endianess_signed_encoding_debug(Combobox_Endianness,
                Checkbox_Endianness_Derived,
                ElementKey.endian);
            SaveUI_Combobox_CheckBox_endianess_signed_encoding_debug(Combobox_Signed,
                Checkbox_Signed_Derived,
                ElementKey.signed);

            SaveUI_Combobox_CheckBox(Combobox_Display,
                Checkbox_Display_Derived,
                ElementKey.display);

            SaveUI_Textbox_Check(Textbox_MinValue,
                Checkbox_MinValue_Derived,
                ElementKey.minval);

            SaveUI_Textbox_Check(Textbox_MaxValue,
                Checkbox_MaxValue_Derived,
                ElementKey.maxval);

            SaveUI_Textbox_Check(Textbox_Value,
                Checkbox_Value_Derived,
                ElementKey.valueexpression);

            SaveFixedValues(Checkbox_FixedValues_MustMatch,
                Checkbox_FixedValues_Derived);

            elementBase.description = Textbox_Description.Text;
        }
    }
}
