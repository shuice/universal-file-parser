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
using kernel;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace file_structure
{
    public sealed partial class ElementEditStructureRef : ElementEditUserControl
    {
        public ElementEditStructureRef()
        {
            this.InitializeComponent();
        }

        private void InitUI_Combobox_Structure()
        {
            Combobox_Structure.Items.Clear();
            ComboBoxItem comboboxItemNone = new ComboBoxItem()
            {
                Content = "<none>",
                Tag = null,
            };

            Combobox_Structure.Items.Add(comboboxItemNone);
            // All root nodes
            object selectedItem = comboboxItemNone;
            string element_id = elementBase.GetValueDerivedWithDefault(ElementKey.structure);

            foreach (ElementStructure option in elementBase.grammar.rootStructures(true))
            {
                if (option != elementBase)
                {
                    ComboBoxItem comboboxItem = new ComboBoxItem()
                    {
                        Content = option.name,
                        Tag = option,
                    };
                    Combobox_Structure.Items.Add(comboboxItem);
                    if (option.IdWithPrefix() == element_id)
                    {
                        selectedItem = comboboxItem;
                    }
                }
            }
            Combobox_Structure.SelectedItem = selectedItem;
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

            InitUI_Combobox_Structure();
            

          
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
            SaveCombobox_ElementID(Combobox_Structure, ElementKey.structure);

            elementBase.description = Textbox_Description.Text;
        }


    }
}
