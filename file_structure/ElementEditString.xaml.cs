using kernel;
using System;
using System.Collections.Generic;
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
    public sealed partial class ElementEditString : ElementEditUserControl
    {
        public ElementEditString()
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
                new List<string> { "Fixed length", "Zero terminated", "Delimiter terminated", "Length prefixed (Pascal)" },
                ElementKey.type,
                "fixed-length",
                Checkbox_Type_Derived,
                new OptionDictionaryStringType()
                );

           

            InitUI_Combo_Check_endianness_signed_encoding_debug(Combobox_Encoding,
                StringEncodingList(),
                ElementKey.encoding,
                "UTF-8",
                Checkbox_Encoding_Derived
                );

            InitFixedValues(data_grid_fixed_values,
                ButtonAdd,
                ButtonRemove,
                Checkbox_FixedValues_Derived,
                Checkbox_MustMatch
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
                                    ElementKey.type
                                    );
            SaveUI_Combobox_CheckBox(Combobox_Length,
                                    Checkbox_Length_Derived,
                                    ElementKey.length
                                    );
            SaveUI_Combobox_CheckBox_endianess_signed_encoding_debug(Combobox_Encoding,
                Checkbox_Encoding_Derived,
                ElementKey.encoding);

            SaveUI_Textbox_Check(Textbox_Delimiter,
                Checkbox_Delimiter_Derived,
                ElementKey.delimiter);

            SaveFixedValues(Checkbox_MustMatch,
                Checkbox_FixedValues_Derived);

            elementBase.description = Textbox_Description.Text;
        }

        protected override void LoadDataToUIFinished() 
        {
            InitTypeRelatedComboboxes();
        }
        private void InitTypeRelatedComboboxes()
        {
            if (isLoadingData)
            {
                return;
            }
            string value = ComboboxSelectedTextValue(Combobox_Type);
            STRING_LENGTH_TYPE type = String2Enum.translateStringToStringLengthType(value);

            if (type == STRING_LENGTH_TYPE.STRING_LENGTH_PASCAL)
            {
                InitUI_Combo_Check(Combobox_Length,
                       new List<string> {"Actual", "Remaining" },
                       ElementKey.length,
                       "actual",
                       Checkbox_Length_Derived,
                       new OptionDictionaryLength()
                       );
            }
            else
            {
                InitUI_Combo_Check(Combobox_Length,
                       new List<string> { "Remaining" },
                       ElementKey.length,
                       "remaining",
                       Checkbox_Length_Derived,
                       new OptionDictionaryLength()
                       );
            }

            InitUI_Textbox_Check(Textbox_Delimiter, 
                ElementKey.delimiter, 
                Checkbox_Delimiter_Derived, 
                "");           
            
            Textbox_Delimiter.IsEnabled = (type == STRING_LENGTH_TYPE.STRING_LENGTH_DELIMITER_TERMINATED);
            Combobox_Length.IsEnabled = ((type == STRING_LENGTH_TYPE.STRING_LENGTH_FIXED) || (type == STRING_LENGTH_TYPE.STRING_LENGTH_PASCAL));
        }

        private void Combobox_Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InitTypeRelatedComboboxes();
        }
    }
}
