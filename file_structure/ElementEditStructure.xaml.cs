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
    public sealed partial class ElementEditStructure : ElementEditUserControl
    {
        private bool isLoadingExtendCombobox = false;
        public ElementEditStructure()
        {
            this.InitializeComponent();
        }

        private void InitUI_Combobox_Repeat()
        {
            Combobox_Repeat.Items.Clear();

            ComboBoxItem comboboxItemNone = new ComboBoxItem()
            {
                Content = "<none>",
                Tag = null,
            };
            Combobox_Repeat.Items.Add(comboboxItemNone);
            // Resolved at the same level or higher level number
            object selectedItem = comboboxItemNone;
            string element_id = elementBase.GetValue(ElementKey.repeat);
            ElementBase from = elementBase;
            for (ElementStructure currentStruct = elementBase.parentStructure;
                currentStruct != null;
                from = currentStruct, currentStruct = currentStruct.parentStructure)
            {
                var elements = currentStruct.elements(true).ToList();
                int take_count = elements.IndexOf(from) + 1;
                foreach (ElementBase option in elements
                                                .Take(take_count)
                                                .Where(item => item is ElementNumber)
                                                .Reverse<ElementBase>())
                {
                    ComboBoxItem comboboxItem = new ComboBoxItem()
                    {
                        Content = option.name,
                        Tag = option,
                    };
                    Combobox_Repeat.Items.Add(comboboxItem);
                    if (option.IdWithPrefix() == element_id)
                    {
                        selectedItem = comboboxItem;
                    }
                }
            }
            Combobox_Repeat.SelectedItem = selectedItem;
        }

        private void InitUI_Combobox_Extends()
        {
            isLoadingExtendCombobox = true;
            // In the root structure, it doesn’t derive from itself.
            Combobox_Extends.Items.Clear();

            ComboBoxItem comboboxItemNone = new ComboBoxItem()
            {
                Content = "<none>",
                Tag = null,
            };
            Combobox_Extends.Items.Add(comboboxItemNone);

            // Resolved at the same level or higher level number
            object selectedItem = comboboxItemNone;
            string element_id = elementBase.GetValue(ElementKey.extends);

            if (elementBase.parentStructure != null)
            {
                Combobox_Extends.SelectedItem = selectedItem;
                Combobox_Extends.IsEnabled = false;
            }
            else
            {
                foreach (ElementStructure option in elementBase.grammar.rootStructures(true).Where(item => item.ExtendsList().Contains(elementBase) == false))
                {
                    if (option != elementBase)
                    {
                        ComboBoxItem comboboxItem = new ComboBoxItem()
                        {
                            Content = option.name,
                            Tag = option,
                        };
                        Combobox_Extends.Items.Add(comboboxItem);
                        if (option.IdWithPrefix() == element_id)
                        {
                            selectedItem = comboboxItem;
                        }
                    }
                }
                Combobox_Extends.SelectedItem = selectedItem;

                Combobox_Extends.IsEnabled = true;
            }
            isLoadingExtendCombobox = false;
            if (Combobox_Extends.Tag == null)
            {
                Combobox_Extends.Tag = new object();
                Combobox_Extends.SelectionChanged += (sender, arg) =>
                {
                    if (isLoadingExtendCombobox == false)
                    {
                        SaveCombobox_ElementID(Combobox_Extends, ElementKey.extends);
                        OnExtendChanged();
                    }
                };
            }
        }

        private void InitUI_Combobox_ConsistesOf()
        {
            // 
            Combobox_Consists.Items.Clear();
            ComboBoxItem comboboxItemNone = new ComboBoxItem()
            {
                Content = "<none>",
                Tag = null,
            };

            Combobox_Consists.Items.Add(comboboxItemNone);
            // All root nodes
            object selectedItem = comboboxItemNone;
            string element_id = elementBase.GetValue(ElementKey.consists_of);

            foreach (ElementStructure option in elementBase.grammar.rootStructures(true))
            {
                if (option != elementBase)
                {
                    ComboBoxItem comboboxItem = new ComboBoxItem()
                    {
                        Content = option.name,
                        Tag = option,
                    };
                    Combobox_Consists.Items.Add(comboboxItem);
                    if (option.IdWithPrefix() == element_id)
                    {
                        selectedItem = comboboxItem;
                    }
                }
            }
            Combobox_Consists.SelectedItem = selectedItem;
        }

       
        protected override void LoadDataToUI()
        {
            InitTextBox(TextBox_Name, ElementKey.name);

            InitUI_Combobox_Repeat();

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
            InitUI_Combobox_Extends();
            InitUI_Combobox_ConsistesOf();

            InitUI_Combo_Check(Combobox_Length,
                new List<string> { "", "Remaining" },
                ElementKey.length,
                "",
                Checkbox_Length_Derived,
                new OptionDictionaryLength()
                );

            InitUI_Combo_Check(Combobox_Alignment,
                new List<string> {"0", "1", "2", "4", "8", "16" },
                ElementKey.alignment,
                "0",
                Checkbox_Alignement_Derived
                );

            InitUI_Combo_Check(Combobox_ElementOrder,
                new List<string> { "Fixed", "Variable" },
                ElementKey.order,
                "fixed",
                Checkbox_ElementOrder_Derived,
                new OptionDictionaryElementOrder()
                );

            InitUI_Textbox_Check(Textbox_Value,
                ElementKey.valueexpression,
                Checkbox_Value_Derived,
                "");

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
                new List<string> { "Yes", "No" },
                ElementKey.signed,
                "no",
                Checkbox_Signed_Derived,
                new OptionDictionaryYesNo()
                );

            InitUI_Combo_Check_endianness_signed_encoding_debug(Combobox_Encoding,
                StringEncodingList(),
                ElementKey.encoding,
                "UTF-8",
                Checkbox_Encoding_Derived
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

            InitUI_Combo_Check_endianness_signed_encoding_debug(Combobox_Debug,
                new List<string> { "Yes", "No"},
                ElementKey.debug,
                "no",
                Checkbox_Debug_Derived,
                new OptionDictionaryYesNo()
                );

            InitTextBox_WithValue(Textbox_Description, elementBase.description);

        }

        protected override void SaveUIToData()
        {
            SaveCombobox_ElementID(Combobox_Repeat,
                ElementKey.repeat);

            SaveUI_Combobox_CheckBox(Combobox_RepeatMin,
                                    Checkbox_RepeatMin_Derived,
                                    ElementKey.repeatmin
                                    );

            SaveUI_Combobox_CheckBox(Combobox_RepeatMax,
                                    Checkbox_RepeatMax_Derived,
                                    ElementKey.repeatmax
                                    );
            SaveCombobox_ElementID(Combobox_Extends, ElementKey.extends);
            SaveCombobox_ElementID(Combobox_Consists, ElementKey.consists_of);

        
            SaveUI_Combobox_CheckBox(Combobox_Length,
                                    Checkbox_Length_Derived,
                                    ElementKey.length
                                    );
            SaveUI_Combobox_CheckBox(Combobox_Alignment,
                Checkbox_Alignement_Derived,
                ElementKey.alignment);

            SaveUI_Combobox_CheckBox(Combobox_ElementOrder,
                Checkbox_ElementOrder_Derived,
                ElementKey.order);

            SaveUI_Textbox_Check(Textbox_Value,
                Checkbox_Value_Derived,
                ElementKey.valueexpression);

            SaveUI_Combobox_CheckBox_endianess_signed_encoding_debug(Combobox_Endianness,
                Checkbox_Endianness_Derived,
                ElementKey.endian);

            SaveUI_Combobox_CheckBox_endianess_signed_encoding_debug(Combobox_Signed,
                Checkbox_Signed_Derived,
                ElementKey.signed);

            SaveUI_Combobox_CheckBox_endianess_signed_encoding_debug(Combobox_Encoding,
                Checkbox_Encoding_Derived,
                ElementKey.encoding);

            SaveUI_Combobox_CheckBox_endianess_signed_encoding_debug(Combobox_Debug,
                Checkbox_Debug_Derived,
                ElementKey.debug);

            elementBase.description = Textbox_Description.Text;
        }
    }
}
