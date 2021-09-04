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
    public sealed partial class ElementEditGrammarRef : ElementEditUserControl
    {
        public ElementEditGrammarRef()
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

            string file_value = elementBase.GetValue(ElementKey.filename);
            DropDownButton_File.Content = String.IsNullOrEmpty(file_value) ? "<None>" : file_value;


            InitTextBox(Textbox_UTI, ElementKey.uti);
            InitTextBox(Textbox_Extension, ElementKey.fileextension);
            
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

            String file_value = DropDownButton_File.Content as String;
            if (String.IsNullOrEmpty(file_value) == false)
            {
                if (file_value.ToLower() == "<none>")
                {
                    elementBase.RemoveAttribute(ElementKey.filename);
                }
                else
                {
                    elementBase.SetAttribute(ElementKey.filename, file_value);
                }
            }
            SaveUI_TextBox(Textbox_UTI, ElementKey.uti);
            SaveUI_TextBox(Textbox_Extension, ElementKey.fileextension);

            elementBase.description = Textbox_Description.Text;
        }

        private MenuFlyout CreateCurrentUsingGrammarMenu()
        {
            string file_value = elementBase.GetValue(ElementKey.filename);
            MenuFlyout menuFlyout = new MenuFlyout();
            // None
            {
                MenuFlyoutItem menuItem = new MenuFlyoutItem();
                menuItem.Text = $"<None>";
                if (String.IsNullOrEmpty(file_value))
                {
                    menuItem.Icon = new SymbolIcon() { Symbol = Symbol.Accept };
                }
                menuItem.Click += Context_menu_Current_Grammar_Record_Selected;
                menuFlyout.Items.Add(menuItem);
            }

            // custom
            {
                List<GrammarRecord> customList = appData.customGrammarList.ToList();
                customList.Sort((a, b) => a.fileName.CompareTo(b.fileName));
                foreach (GrammarRecord record in customList)
                {
                    MenuFlyoutItem menuItem = new MenuFlyoutItem();
                    menuItem.Text = record.FullName();
                    menuItem.Tag = record;
                    if (file_value == record.fileName)
                    {
                        menuItem.Icon = new SymbolIcon() { Symbol = Symbol.Accept };
                    }
                    menuItem.Click += Context_menu_Current_Grammar_Record_Selected;
                    menuFlyout.Items.Add(menuItem);
                }
            }

            // build in
            {
                List<GrammarRecord> buildInList = appData.buildinGrammarList.ToList();

                MenuFlyoutSubItem menuSubItem = new MenuFlyoutSubItem();
                menuSubItem.Text = "Build In";
                if (buildInList.Select(item => item.fileName).Contains(file_value))
                {
                    menuSubItem.Icon = new SymbolIcon() { Symbol = Symbol.Accept };
                }
                menuFlyout.Items.Add(menuSubItem);


                buildInList.Sort((a, b) => a.fileName.CompareTo(b.fileName));
                foreach (GrammarRecord record in buildInList)
                {
                    MenuFlyoutItem menuItem = new MenuFlyoutItem();
                    menuItem.Text = record.FullName();
                    menuItem.Tag = record;
                    if (file_value == record.fileName)
                    {
                        menuItem.Icon = new SymbolIcon() { Symbol = Symbol.Accept };
                    }
                    menuItem.Click += Context_menu_Current_Grammar_Record_Selected;
                    menuSubItem.Items.Add(menuItem);
                }
            }
            return menuFlyout;
        }

        private void Context_menu_Current_Grammar_Record_Selected(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem menuItem = sender as MenuFlyoutItem;
            if (menuItem == null)
            {
                return;
            }
            GrammarRecord selectedRecord = menuItem.Tag as GrammarRecord;
            DropDownButton_File.Content = (selectedRecord == null) ? "<None>" : selectedRecord.fileName;

        }
        private void DropDownButton_File_Click(object sender, RoutedEventArgs e)
        {
            var menu = CreateCurrentUsingGrammarMenu();
            menu.ShowAt(DropDownButton_File, new FlyoutShowOptions() { Placement = FlyoutPlacementMode.BottomEdgeAlignedLeft });
        }
    }
}
