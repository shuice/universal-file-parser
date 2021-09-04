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
using kernel;
using Windows.UI;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace file_structure
{
    public sealed partial class ElementEditOffset : ElementEditUserControl
    {
        public ElementEditOffset()
        {
            this.InitializeComponent();
        }

        private void InitUI_Combo_Structure()
        {
            // root structures
            Combobox_Structure.Items.Clear();
            ComboBoxItem comboboxItemDefault = new ComboBoxItem()
            {
                Content = "<none>",
                Tag = null,
            };
            Combobox_Structure.Items.Add(comboboxItemDefault);
            object selectedItem = comboboxItemDefault;
            string structure_id = elementBase.GetValue(ElementKey.references);
            foreach(ElementStructure structure in elementBase.grammar.rootStructures(true))
            {
                ComboBoxItem comboboxItem = new ComboBoxItem()
                {
                    Content = structure.name,
                    Tag = structure,
                };
                Combobox_Structure.Items.Add(comboboxItem);
                if (structure.IdWithPrefix() == structure_id)
                {
                    selectedItem = comboboxItem;
                }
            }
            Combobox_Structure.SelectedItem = selectedItem;
        }

        private void InitUI_Combo_RelativeTo()
        {
            Combobox_RelativeTo.Items.Clear();
            ComboBoxItem comboboxItemDefault = new ComboBoxItem()
            {
                Content = "File Start",
                Tag = null,
            };
            Combobox_RelativeTo.Items.Add(comboboxItemDefault);

            object selectedItem = comboboxItemDefault;
            string element_id = elementBase.GetValue(ElementKey.relative_to);
            ElementBase from = elementBase;
            for (ElementStructure currentStruct = elementBase.parentStructure;
                currentStruct != null;
                from = currentStruct, currentStruct = currentStruct.parentStructure)
            {
                var elements = currentStruct.elements(true).ToList();
                int take_count = elements.IndexOf(from) + 1;
                foreach(ElementBase option in elements.Take(take_count).Reverse<ElementBase>())
                {
                    ComboBoxItem comboboxItem = new ComboBoxItem()
                    {
                        Content = option.name,
                        Tag = option,
                    };
                    Combobox_RelativeTo.Items.Add(comboboxItem);
                    if (option.IdWithPrefix() == element_id)
                    {
                        selectedItem = comboboxItem;
                    }
                }
            }
            Combobox_RelativeTo.SelectedItem = selectedItem;

        }

        public void InitUI_Combo_RefSize()
        {
            // "" -> Size inside referenced structure
            // "id:1784" -> target
            ComboBoxItem defaultComboboxItem = new ComboBoxItem()
            {
                Content = "Size inside referenced structure",
                Tag = null,
            };

            // Sibling is the value of number
            Combobox_RefSize.Items.Clear();
            Combobox_RefSize.Items.Add(defaultComboboxItem);
            object selectedItem = defaultComboboxItem;
            string element_id = elementBase.GetValue(ElementKey.referenced_size);
            foreach(ElementBase option in elementBase.parentStructure.elements(true).Where(item => item is ElementNumber))
            {
                ComboBoxItem comboboxItem = new ComboBoxItem()
                {
                    Content = option.name,
                    Tag = option,
                };
                Combobox_RefSize.Items.Add(comboboxItem);
                if (option.IdWithPrefix() == element_id)
                {
                    selectedItem = comboboxItem;
                }
            }
            Combobox_RefSize.SelectedItem = selectedItem;
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

            InitUI_Combo_Check_endianness_signed_encoding_debug(Combobox_Endianness,
                new List<string> { "Big", "Little", "Dynamic" },
                ElementKey.endian,
                "big",
                Checkbox_Endianness_Derived,
                new OptionDictionaryEndianness()
                );

            // Combobox_Structure
            InitUI_Combo_Structure();

            InitUI_Combo_RelativeTo();

            // Combobox_RelativeTo

            InitUI_Textbox_Check(Textbox_Additional,
                ElementKey.additional,
                Checkbox_Addtional_Derived,
                "0x0"
                );

            InitUI_Combo_Check(Combobox_Display,
                new List<string> { "Decimal", "Hexadecimal", "Octal" },
                ElementKey.display,
                "hex",
                Checkbox_Display_Derived,
                new OptionDictionaryDisplay()
                );

            InitUI_Combo_RefSize();



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

            Checkbox_FollowNullReference.IsChecked = followNullReference;
        }

        public bool followNullReference
        {
            get
            {
                return elementBase.GetValue(ElementKey.follownullreference) == "yes";
            }
            set
            {
                elementBase.SetAttribute(ElementKey.follownullreference, value ? "yes" : "no");
            }
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


            SaveUI_Combobox_Combobox_CheckBox(Combobox_Length,
                Combobox_Length_Unit,
                Checkbox_Length_Derived,
                ElementKey.length,
                ElementKey.lengthunit);


            SaveUI_Combobox_CheckBox_endianess_signed_encoding_debug(Combobox_Endianness,
                Checkbox_Endianness_Derived,
                ElementKey.endian);

            
            SaveCombobox_ElementID(Combobox_Structure, ElementKey.references);
            SaveCombobox_ElementID(Combobox_RelativeTo, ElementKey.relative_to);            

            SaveUI_Textbox_Check(Textbox_Additional,
                Checkbox_Addtional_Derived,
                ElementKey.additional);

            SaveUI_Combobox_CheckBox(Combobox_Display,
                Checkbox_Display_Derived,
                ElementKey.display);
            SaveCombobox_ElementID(Combobox_RefSize, ElementKey.referenced_size);            

            elementBase.description = Textbox_Description.Text;

        }

    }

}
