using Common;
using kernel;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace file_structure
{

    public abstract class ElementEditUserControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected ElementBase elementBase;
        public event EventHandler<ElementBase> eventNameChanged;
        public event EventHandler<ElementBase> eventExtendChanged;
        public event EventHandler<ElementBase> eventEnabledChanged;
        public bool isLoadingData { private set; get; } = false;

        public AppData appData => AppData.appData;

        public void LoadElement(ElementBase elementBase)
        {
            this.elementBase = elementBase;
            isLoadingData = true;
            LoadDataToUI();
            isLoadingData = false;
            LoadDataToUIFinished();
            // bind update
            this.enabled = this.enabled;
            this.structure_debug = this.structure_debug;
            this.structure_derived = this.structure_derived;
        }

        public void Save()
        {
            SaveUIToData();
        }
        protected abstract void LoadDataToUI();
        protected virtual void LoadDataToUIFinished() { }
        protected abstract void SaveUIToData();


        protected void InitTextBox_WithValue(TextBox textBox, String value)
        {
            textBox.Text = value;
        }

        protected void InitTextBox(TextBox textBox, String elementKey)
        {
            textBox.Text = elementBase.GetValue(elementKey);
        }

        protected void SaveUI_TextBox(TextBox textBox, String elementKey)
        {
            elementBase.SetAttribute(elementKey, textBox.Text);
        }



        public List<string> OptionsNumberLength(LENGTH_UNIT lengthUnit)
        {
            if (lengthUnit == LENGTH_UNIT.LENGTH_BYTE)
            {
                return new List<string> { "1", "2", "3", "4", "8" };
            }
            return new List<string> { "8", "16", "24", "32", "64" };
        }
        protected void InitCombobox(ComboBox combobox, List<String> options, String selected)
        {
            combobox.Items.Clear();
            ComboBoxItem selectedItem = null;
            foreach (String option in options)
            {
                ComboBoxItem item = new ComboBoxItem() { Content = option };
                if (option == selected)
                {
                    selectedItem = item;
                }
                combobox.Items.Add(item);
            }

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



        public void OnExtendChanged()
        {
            eventExtendChanged?.Invoke(this, elementBase);
        }


        public void InitFixedValues(DataGrid dataGrid,
                                    Button buttonAdd,
                                    Button buttonRemove,
                                    CheckBox checkBoxDerived,
                                    CheckBox checkMustMatch)
        {
            appData.editingElementFixedValues.Clear();
            foreach (FixedValue fixedValue in elementBase.fixedValues)
            {
                appData.editingElementFixedValues.Add(new Common.NameValuePair() { name = fixedValue.name, value = fixedValue.value });
            }

            checkBoxDerived.IsEnabled = elementBase.is_derived;
            checkBoxDerived.IsChecked = elementBase.is_derived && elementBase.fixedValueDerivid;
            checkMustMatch.IsChecked = elementBase.mustMatch;

            bool canEdit = (elementBase.is_derived == false) || (elementBase.fixedValueDerivid == false);

            buttonAdd.IsEnabled = canEdit;
            buttonRemove.IsEnabled = false; // wait for select
            checkMustMatch.IsEnabled = canEdit;
            dataGrid.IsEnabled = canEdit;

            if (dataGrid.Tag == null)
            {
                dataGrid.Tag = new object();
                buttonAdd.Click += (sender, arg) =>
                {                    
                    NameValuePair nameValuePair = new NameValuePair()
                    {
                        name = "<name>",
                        value = "<value>",
                    };
                    appData.editingElementFixedValues.Add(nameValuePair);
                    elementBase.AddFixedValue(new FixedValue() { name = "", value = "" });
                    elementBase.fixedValueDerivid = false;
                    dataGrid.SelectedItem = nameValuePair;

                    Debug.Assert(appData.editingElementFixedValues.Count() == elementBase.fixedValues.Count());
                };
                buttonRemove.Click += (sender, arg) =>
                {
                    int selectedIndex = dataGrid.SelectedIndex;
                    if (selectedIndex != -1)
                    {
                        appData.editingElementFixedValues.RemoveAt(selectedIndex);
                        elementBase.RemoveFixedValueAt(selectedIndex);
                    }
                    Debug.Assert(appData.editingElementFixedValues.Count() == elementBase.fixedValues.Count());
                };

                dataGrid.SelectionChanged += (sender, arg) =>
                {
                    buttonRemove.IsEnabled = (dataGrid.SelectedItem != null);
                };

                checkBoxDerived.Checked += (sender, arg) =>
                {
                    if (isLoadingData)
                    {
                        return;
                    }

                    dataGrid.IsEnabled = false;
                    buttonAdd.IsEnabled = false;
                    buttonRemove.IsEnabled = false;
                    checkMustMatch.IsEnabled = false;

                    elementBase.fixedValueDerivid = true;
                    elementBase.ClearFixedValues();
                    appData.editingElementFixedValues.Clear();
                    foreach (FixedValue fixedValue in elementBase.fixedValues)
                    {
                        appData.editingElementFixedValues.Add(new Common.NameValuePair() { name = fixedValue.name, value = fixedValue.value });
                    }
                };
                checkBoxDerived.Unchecked += (sender, arg) =>
                {
                    if (isLoadingData)
                    {
                        return;
                    }
                    dataGrid.IsEnabled = true;
                    buttonAdd.IsEnabled = true;
                    buttonRemove.IsEnabled = (dataGrid.SelectedItem != null);
                    checkMustMatch.IsEnabled = true;

                    elementBase.fixedValueDerivid = false;
                    appData.editingElementFixedValues.Clear();
                    foreach (FixedValue fixedValue in elementBase.fixedValues)
                    {
                        appData.editingElementFixedValues.Add(new Common.NameValuePair() { name = fixedValue.name, value = fixedValue.value });
                    }
                };
            }
        }

        public void InitMasks(DataGrid dataGrid,
                                    Button buttonAdd,
                                    Button buttonRemove,
                                    Button buttonEidt,
                                    CheckBox checkBoxDerived)
        {
            appData.editingElementMasks.Clear();
            foreach (Mask mask in elementBase.masks)
            {
                appData.editingElementMasks.Add(new Common.NameValuePair() { name = mask.name, value = mask.value });
            }

            checkBoxDerived.IsEnabled = elementBase.is_derived;
            checkBoxDerived.IsChecked = elementBase.is_derived && elementBase.maskValueDerived;

            bool canEdit = (elementBase.is_derived == false) || (elementBase.maskValueDerived == false);

            buttonAdd.IsEnabled = canEdit;
            buttonRemove.IsEnabled = false;
            dataGrid.IsEnabled = canEdit;
            buttonEidt.IsEnabled = canEdit;

            if (dataGrid.Tag == null)
            {
                dataGrid.Tag = new object();
                buttonAdd.Click += async (sender, arg) =>
                {
                    ElementEditNumberMask elementEditNumberMask = new ElementEditNumberMask(new Mask());
                    ContentDialogResult contentDialogResult = await elementEditNumberMask.ShowAsync();
                    if (contentDialogResult == ContentDialogResult.Secondary)
                    {
                        Mask result = elementEditNumberMask.result;
                        NameValuePair nameValuePair = new NameValuePair()
                        {
                            name = result.name,
                            value = result.value,
                        };
                        elementBase.AddMaskValue(result);
                        elementBase.maskValueDerived = false;
                        checkBoxDerived.IsChecked = false;
                        appData.editingElementMasks.Add(nameValuePair);
                        dataGrid.SelectedItem = nameValuePair;
                    }
                };
                buttonRemove.Click += (sender, arg) =>
                {
                    int selectedIndex = dataGrid.SelectedIndex;
                    if (selectedIndex >= 0)
                    {
                        elementBase.RemoveMaskAtIndex(selectedIndex);
                        appData.editingElementMasks.RemoveAt(selectedIndex);
                    }
                };

                buttonEidt.Click += async (sender, arg) =>
                {
                    int selectedIndex = dataGrid.SelectedIndex;
                    if (selectedIndex >= 0)
                    {
                        Mask editingMask = elementBase.masks[selectedIndex];
                        ElementEditNumberMask elementEditNumberMask = new ElementEditNumberMask(editingMask);
                        ContentDialogResult contentDialogResult = await elementEditNumberMask.ShowAsync();
                        if (contentDialogResult == ContentDialogResult.Secondary)
                        {
                            Mask result = elementEditNumberMask.result;
                            elementBase.ReplaceMaskValue(selectedIndex, result);
                            NameValuePair nameValuePair = appData.editingElementMasks[selectedIndex];
                            nameValuePair.name = result.name;
                            nameValuePair.value = result.value;
                        }
                    }
                };

                dataGrid.SelectionChanged += (sender, arg) =>
                {
                    buttonRemove.IsEnabled = (dataGrid.SelectedItem != null);
                };

                checkBoxDerived.Checked += (sender, arg) =>
                {
                    if (isLoadingData)
                    {
                        return;
                    }
                    dataGrid.IsEnabled = false;
                    buttonAdd.IsEnabled = false;
                    buttonRemove.IsEnabled = false;
                    buttonEidt.IsEnabled = false;

                    elementBase.maskValueDerived = true;
                    elementBase.ClearMaskValues();

                    appData.editingElementMasks.Clear();
                    foreach (Mask mask in elementBase.masks)
                    {
                        appData.editingElementMasks.Add(new Common.NameValuePair() { name = mask.name, value = mask.value });
                    }
                };
                checkBoxDerived.Unchecked += (sender, arg) =>
                {
                    if (isLoadingData)
                    {
                        return;
                    }
                    dataGrid.IsEnabled = true;
                    buttonAdd.IsEnabled = true;
                    buttonRemove.IsEnabled = (dataGrid.SelectedItem != null);
                    buttonEidt.IsEnabled = true;

                    elementBase.maskValueDerived = false;
                    appData.editingElementMasks.Clear();
                    foreach (Mask mask in elementBase.masks)
                    {
                        appData.editingElementMasks.Add(new Common.NameValuePair() { name = mask.name, value = mask.value });
                    }
                };
            }
        }


        public Color ConvertColorStringToColor(string colorStringRRGGBB, Color defaultColor)
        {
            colorStringRRGGBB = colorStringRRGGBB.ToUpper();
            if (colorStringRRGGBB.Length == 6)
            {
                string r = colorStringRRGGBB.Substring(0, 2);
                string g = colorStringRRGGBB.Substring(2, 2);
                string b = colorStringRRGGBB.Substring(4, 2);
                Byte iR;
                Byte iG;
                Byte iB;
                if (Byte.TryParse(r, NumberStyles.HexNumber, null, out iR)
                    && Byte.TryParse(g, NumberStyles.HexNumber, null, out iG)
                    && Byte.TryParse(b, NumberStyles.HexNumber, null, out iB))
                {
                    Color color = Color.FromArgb(0xFF, iR, iG, iB);
                    return color;
                }
            }
            return defaultColor;
        }

        public string ConvertColorToColorString(Color color)
        {
            return $"{color.R.ToString("X2")}{color.G.ToString("X2")}{color.B.ToString("X2")}";
        }

        public void InitColorControlPair(Border borderColor,
                                                Button btnColor,
                                                Button btnColorReset,
                                                CheckBox checkboxDerived,
                                                String elementKey,
                                                Color defaultColor)
        {
            Color color = ConvertColorStringToColor(elementBase.GetValueDerivedWithDefault(elementKey), defaultColor);
            borderColor.Background = new SolidColorBrush(color);

            bool hasAttribute = elementBase.HasAttribute(elementKey);
            bool canEdit = (elementBase.is_derived == false) || (hasAttribute);
            checkboxDerived.IsEnabled = elementBase.is_derived;
            checkboxDerived.IsChecked = (elementBase.is_derived) && (hasAttribute == false);
            btnColor.Visibility = Visibility.Collapsed;
            btnColor.IsEnabled = canEdit;
            borderColor.IsTapEnabled = canEdit;
            btnColorReset.IsEnabled = canEdit;


            if (borderColor.Tag == null)
            {
                borderColor.Tag = new object();
                borderColor.Tapped += async (sender, arg) =>                
                {
                    Color before_color = ConvertColorStringToColor(elementBase.GetValueDerivedWithDefault(elementKey), defaultColor);
                    ColorSelectorContentDialog colorDialog = new ColorSelectorContentDialog(before_color);
                    if (await colorDialog.ShowAsync() == ContentDialogResult.Secondary)
                    {
                        string colorString = ConvertColorToColorString(colorDialog.selectedColor);
                        elementBase.SetAttribute(elementKey, colorString);

                        Color new_color = ConvertColorStringToColor(elementBase.GetValueDerivedWithDefault(elementKey), defaultColor);
                        borderColor.Background = new SolidColorBrush(new_color);
                    }
                };

                btnColorReset.Click += (sender, arg) =>
                {
                    elementBase.RemoveAttribute(elementKey);

                    Color new_color = ConvertColorStringToColor(elementBase.GetValueDerivedWithDefault(elementKey), defaultColor);
                    borderColor.Background = new SolidColorBrush(new_color);
                };

                checkboxDerived.Checked += (sender, arg) =>
                {
                    if (isLoadingData)
                    {
                        return;
                    }

                    btnColor.IsEnabled = false;
                    borderColor.IsTapEnabled = false;
                    btnColorReset.IsEnabled = false;

                    elementBase.RemoveAttribute(elementKey);

                    Color new_color = ConvertColorStringToColor(elementBase.GetValueDerivedWithDefault(elementKey), defaultColor);
                    borderColor.Background = new SolidColorBrush(new_color);
                };

                checkboxDerived.Unchecked += (sender, arg) =>
                {
                    if (isLoadingData)
                    {
                        return;
                    }
                    btnColor.IsEnabled = true;
                    borderColor.IsTapEnabled = true;
                    btnColorReset.IsEnabled = true;
                };
            }
        }


        public void InitUI_Textbox_Check(TextBox textBox,
            string elementKey,
            CheckBox checkBox,
            string defalut_value
            )
        {
            textBox.Text = elementBase.GetValueDerivedWithDefault(elementKey, defalut_value);

            bool hasAttribute = elementBase.HasAttribute(elementKey);
            checkBox.IsEnabled = elementBase.is_derived;
            checkBox.IsChecked = (elementBase.is_derived) && (hasAttribute == false);

            textBox.IsEnabled = !((hasAttribute == false) && (elementBase.is_derived));

            if (textBox.Tag == null)
            {
                textBox.Tag = new object();
                checkBox.Checked += (sender, arg) =>
                {
                    textBox.IsEnabled = false;
                    elementBase.RemoveAttribute(elementKey);
                    string value = elementBase.GetValueDerivedWithDefault(elementKey, defalut_value);
                    textBox.Text = value;
                };
                checkBox.Unchecked += (sender, arg) =>
                {
                    if (isLoadingData)
                    {
                        return;
                    }
                    textBox.IsEnabled = true;
                };
            }
        }


        public void InitUI_Combo_Check(ComboBox combobox,
            List<String> options,
            string elementKey,
            string defalut_value,
            CheckBox checkBox,
            OptionDictionary optionDictionary = null
            )
        {
            combobox.Items.Clear();
            options.ForEach(item => combobox.Items.Add(new ComboBoxItem() { Content = item }));
            string selected = elementBase.GetValueDerivedWithDefault(elementKey, defalut_value);
            if (optionDictionary != null)
            {
                selected = optionDictionary.Value2Ui(selected);
            }
            SetComboboxSelected(combobox, selected);

            bool hasAttribute = elementBase.HasAttribute(elementKey);
            checkBox.IsEnabled = elementBase.is_derived;
            checkBox.IsChecked = (elementBase.is_derived) && (hasAttribute == false);
            combobox.IsEnabled = !((hasAttribute == false) && (elementBase.is_derived));

            if (combobox.Tag == null)
            {
                combobox.Tag = optionDictionary ?? new object();
                checkBox.Checked += (sender, arg) =>
                {
                    if (isLoadingData)
                    {
                        return;
                    }
                    combobox.IsEnabled = false;
                    elementBase.RemoveAttribute(elementKey);
                    string selected = elementBase.GetValueDerivedWithDefault(elementKey, defalut_value);
                    if (optionDictionary != null)
                    {
                        selected = optionDictionary.Value2Ui(selected);
                    }
                    SetComboboxSelected(combobox, selected);
                };
                checkBox.Unchecked += (sender, arg) =>
                {
                    if (isLoadingData)
                    {
                        return;
                    }
                    combobox.IsEnabled = true;
                };
            }
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

        public void InitUI_Combo(ComboBox combobox,
            List<String> options,
            string elementKey,
            string defalut_value
            )
        {
            combobox.Items.Clear();
            options.ForEach(item => combobox.Items.Add(new ComboBoxItem() { Content = item }));
            string selected = elementBase.GetValueDerivedWithDefault(elementKey, defalut_value);
            SetComboboxSelected(combobox, selected);
        }

        public void InitUI_Combo_Check_endianness_signed_encoding_debug(ComboBox combobox,
           List<String> options,
           string elementKey,
           string defalut_value,
           CheckBox checkBox,
           OptionDictionary optionDictionary = null
           )
        {
            combobox.Items.Clear();
            options.ForEach(item => combobox.Items.Add(new ComboBoxItem() { Content = item }));
            string selected = elementBase.GetValueDerivedWithDefault(elementKey, defalut_value);
            if (optionDictionary != null)
            {
                selected = optionDictionary.Value2Ui(selected);
            }
            SetComboboxSelected(combobox, selected);

            bool hasAttribute = elementBase.HasAttribute(elementKey);
            bool isRootStructure = (elementBase.parentStructure == null);
            bool canGetValueDerived = ((isRootStructure == false) || elementBase.is_derived);
            checkBox.IsEnabled = canGetValueDerived;
            combobox.IsEnabled = (canGetValueDerived == false) || hasAttribute;
            checkBox.IsChecked = canGetValueDerived && (hasAttribute == false);


            if (combobox.Tag == null)
            {
                combobox.Tag = optionDictionary ?? new object();
                checkBox.Checked += (sender, arg) =>
                {
                    if (isLoadingData)
                    {
                        return;
                    }
                    combobox.IsEnabled = false;
                    elementBase.RemoveAttribute(elementKey);
                    string selected = elementBase.GetValueDerivedWithDefault(elementKey, defalut_value);
                    if (optionDictionary != null)
                    {
                        selected = optionDictionary.Value2Ui(selected);
                    }
                    SetComboboxSelected(combobox, selected);
                };
                checkBox.Unchecked += (sender, arg) =>
                {
                    if (isLoadingData)
                    {
                        return;
                    }
                    combobox.IsEnabled = true;
                };
            }
        }


        public void InitUI_Combo_Combo_Check(ComboBox combobox1,
            List<String> options1,
            string elementKey1,
            string defalut_value1,            

            ComboBox combobox2,
            List<String> options2,
            string elementKey2,
            string defalut_value2,

            CheckBox checkBox,

            OptionDictionary optionDictionary1 = null,
            OptionDictionary optionDictionary2 = null
            )
        {
            combobox1.Items.Clear();
            options1.ForEach(item => combobox1.Items.Add(new ComboBoxItem() { Content = item }));
            string selected1 = elementBase.GetValueDerivedWithDefault(elementKey1, defalut_value1);
            if (optionDictionary1 != null)
            {
                selected1 = optionDictionary1.Value2Ui(selected1);
            }
            SetComboboxSelected(combobox1, selected1);

            combobox2.Items.Clear();
            options2.ForEach(item => combobox2.Items.Add(new ComboBoxItem() { Content = item }));
            string selected2 = elementBase.GetValueDerivedWithDefault(elementKey2, defalut_value2);
            if (optionDictionary2 != null)
            {
                selected2 = optionDictionary2.Value2Ui(selected2);
            }
            SetComboboxSelected(combobox2, selected2);

            bool hasAttribute = (elementBase.HasAttribute(elementKey1) || elementBase.HasAttribute(elementKey2));
            checkBox.IsEnabled = elementBase.is_derived;
            checkBox.IsChecked = (elementBase.is_derived) && (hasAttribute == false);
            combobox1.IsEnabled = !((hasAttribute == false) && (elementBase.is_derived));
            combobox2.IsEnabled = !((hasAttribute == false) && (elementBase.is_derived));

            if (combobox1.Tag == null)
            {
                combobox1.Tag = optionDictionary1 ?? new object();
                combobox2.Tag = optionDictionary2 ?? new object();
                checkBox.Checked += (sender, arg) =>
                {
                    if (isLoadingData)
                    {
                        return;
                    }
                    combobox1.IsEnabled = false;
                    elementBase.RemoveAttribute(elementKey1);
                    string selected1 = elementBase.GetValueDerivedWithDefault(elementKey1, defalut_value1);
                    if (optionDictionary1 != null)
                    {
                        selected1 = optionDictionary1.Value2Ui(selected1);
                    }
                    SetComboboxSelected(combobox1, selected1);

                    combobox2.IsEnabled = false;
                    elementBase.RemoveAttribute(elementKey2);
                    string selected2 = elementBase.GetValueDerivedWithDefault(elementKey2, defalut_value2);
                    if (optionDictionary2 != null)
                    {
                        selected2 = optionDictionary2.Value2Ui(selected2);
                    }
                    SetComboboxSelected(combobox2, selected2);
                };
                checkBox.Unchecked += (sender, arg) =>
                {
                    if (isLoadingData)
                    {
                        return;
                    }
                    combobox1.IsEnabled = true;
                    combobox2.IsEnabled = true;
                };
            }
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

        public bool enabled
        {
            get => elementBase.enabled;
            set
            {
                elementBase.enabled = value;
                OnPropertyChanged();
                eventEnabledChanged?.Invoke(this, elementBase);
            }
        }

        public bool structure_debug
        {
            get => elementBase.debug;
            set
            {
                elementBase.debug = value;
                OnPropertyChanged();
            }
        }

        public bool structure_derived
        {
            get => elementBase.enabled;
            set
            {
                elementBase.enabled = value;
                OnPropertyChanged();
            }
        }

        public List<string> StringEncodingList()
        {
            var keys = StringEncodingWrapper.dicEncodingName.Keys.ToList();
            keys.Sort();
            return keys;            
        }

        public void ParentContentControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ContentControl contentControl = sender as ContentControl;
            Grid gridEdit = contentControl.Content as Grid;
            Debug.Assert((contentControl != null) && (gridEdit != null));
            if ((contentControl != null) && (gridEdit != null))
            {
                Thickness thickness = gridEdit.Margin;
                gridEdit.Width = contentControl.ActualWidth - (thickness.Left + thickness.Right);
            }
        }


        public string ComboboxSelectedTextValue(ComboBox combobox)
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
            Debug.Assert(value != null);
            OptionDictionary optionDictionary = combobox.Tag as OptionDictionary;
            if (optionDictionary != null)
            {
                value = optionDictionary.Ui2Value(value);
            }
            return value ?? "";
        }

        public void SaveUI_Combobox_CheckBox(ComboBox combobox, CheckBox checkbox, string elementKey)
        {
            if (checkbox.IsChecked.GetValueOrDefault(false))
            {
                Debug.Assert(elementBase.is_derived);
                elementBase.RemoveAttribute(elementKey);
            }
            else
            {
                string value = ComboboxSelectedTextValue(combobox);               
                elementBase.SetAttribute(elementKey, value);
            }
        }

        public void SaveUI_Combobox_Combobox_CheckBox(ComboBox combobox1,
            ComboBox combobox2,
            CheckBox checkbox,
            string elementKey1,
            string elementKey2)
        {
            SaveUI_Combobox_CheckBox(combobox1, checkbox, elementKey1);
            SaveUI_Combobox_CheckBox(combobox2, checkbox, elementKey2);
        }

        public void SaveUI_Combobox_CheckBox_endianess_signed_encoding_debug(ComboBox combobox, CheckBox checkbox, string elementKey)
        {
            if (checkbox.IsChecked.GetValueOrDefault(false))
            {
                Debug.Assert(elementBase.is_derived || elementBase.parentStructure != null);
                elementBase.RemoveAttribute(elementKey);
            }
            else
            {
                string value = ComboboxSelectedTextValue(combobox);                
                elementBase.SetAttribute(elementKey, value);
            }
        }

        public void SaveUI_Textbox_Check(TextBox textBox,
            CheckBox checkBox,
            string elementKey
            )
        {
            if (checkBox.IsChecked.GetValueOrDefault(false))
            {
                Debug.Assert(elementBase.is_derived);
                elementBase.RemoveAttribute(elementKey);
            }
            else
            {
                elementBase.SetAttribute(elementKey, textBox.Text);
            }
        }

        public void SaveCombobox_ElementID(ComboBox combobox,
            String elementKey
            )
        {
            ComboBoxItem comboboxItem = combobox.SelectedItem as ComboBoxItem;
            if (comboboxItem == null)
            {
                elementBase.RemoveAttribute(elementKey);
            }
            else
            {
                ElementBase element = comboboxItem.Tag as ElementBase;
                if (element == null)
                {
                    elementBase.RemoveAttribute(elementKey);
                }
                else
                {
                    elementBase.SetAttribute(elementKey, element.IdWithPrefix());
                }
            }
        }

        protected void TextBox_Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string oldName = elementBase.name;
            string newName = textBox.Text;
            if (oldName != newName)
            {
                elementBase.setName(newName);
                eventNameChanged?.Invoke(this, elementBase);
            }
        }

        protected void SaveFixedValues(CheckBox checkBoxMustMatch,
            CheckBox checkBoxDerived)
        {
            if (checkBoxDerived.IsChecked.GetValueOrDefault(false))
            {
                elementBase.fixedValueDerivid = true;
                elementBase.ClearFixedValues();
                elementBase.RemoveAttribute(ElementKey.mustmatch);
            }
            else
            {
                elementBase.fixedValueDerivid = false;
                for (int index = 0; index < appData.editingElementFixedValues.Count(); index++)
                {
                    NameValuePair nameValuePair = appData.editingElementFixedValues[index];
                    elementBase.SetFixedValueAtIndex(index, nameValuePair.name, nameValuePair.value);
                }
                elementBase.mustMatch = checkBoxMustMatch.IsChecked.GetValueOrDefault(false);
            }
        }        
    }
}
