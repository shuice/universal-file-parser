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
using Windows.UI;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace file_structure
{
    public sealed partial class ElementEditScript : ElementEditUserControl
    {
        public ElementEditScript()
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

            InitUI_Combo_WithValue(Combobox_Language,
                new List<string> { "Python", "Lua" },
                elementBase.scriptLanguage,
                "Python",
                null
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

            
            InitTextBox_WithValue(Textbox_ScriptContent, elementBase.scriptContent);
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
            elementBase.scriptLanguage = ComboboxSelectedTextValue(Combobox_Language);
            elementBase.scriptContent = Textbox_ScriptContent.Text;            
            elementBase.description = Textbox_Description.Text;
        }

    }
}
