//#define MCOMMONS
#if MCOMMONS
using MCommons;
#endif
using System;
using System.Reflection;
using Godot;
namespace MSettings;
/// <summary>
/// Drop this in to control child UI Nodes.
/// Set FieldTarget to the exact field name in the class definition you want to control.
/// Remember that the label for the setting's name need to be named "FieldName" for this script to pick it up
/// Supports: LineEdit, Slider, CheckBox, OptionButton
/// </summary>
[GlobalClass]
public partial class UISettingsEntry : Control
{
    [Export(hintString: "This should be the parent of all the nodes we look for")]
    private Control elementParent;
    [Export] private string settingsName;
    [Export] private string fieldTarget = "UNSET";
    #region  The references to first layer children by type. The fieldTarget Type determines which of them get used;
    private LineEdit lineEdit;
    private Slider slider;
    private CheckButton toggleButton;
    private CheckBox toggleBox;
    private OptionButton dropdown;
    private Label labelFieldName;
    private Label labelValue;
    private ColorPickerButton btnColourPicker;

    private TextureButton btnReset;
    private Button btnKeyBind;
    private Button btnKeyBindAlt;
    #endregion
    //private string fullSettingsName;
    /*
    [Export] private string _keyBindText;
    [Export] private string _keyBindAltText;
    [Export] private InputAction _rebindAction;
    */
    private Object Config => Settings.GetCachedSettings(settingsName);

    public override void _Ready()
    {
        // Listen for config changes
        Settings.OnSettingsChange += WhenSettingsChange;
        // Listen for keybinds
        UIKeybindPopup.OnKeyBindUpdated += WhenNewKeyBindIsMade;
        // GrabChildren
        foreach (Control child in elementParent.GetChildren())
        {
            if (lineEdit is null && child is LineEdit) { lineEdit = child as LineEdit; }
            if (slider is null && child is Slider) { slider = child as Slider; }
            if (toggleButton is null && child is CheckButton) { toggleButton = child as CheckButton; }
            if (toggleBox is null && child is CheckBox) { toggleBox = child as CheckBox; }
            if (dropdown is null && child is OptionButton) { dropdown = child as OptionButton; }
            if (child is Label && child.Name == "FieldName") { labelFieldName = child as Label; }
            if (child is Label && child.Name == "Value") { labelValue = child as Label; }
            if (child is ColorPickerButton) { btnColourPicker = child as ColorPickerButton; }

            if (child is Button && child.Name == "KeyBind") { btnKeyBind = child as Button; }
            if (child is Button && child.Name == "KeyBindAlt") { btnKeyBindAlt = child as Button; }

            if (child is TextureButton) { btnReset = child as TextureButton; }
        }
        // Listen to changes in UI elements
        //if (slider is not null) { slider.ValueChanged += WhenSliderValueChanged; } // This might be bugged so disconnect this when changing slider
        if (slider is not null) { slider.DragEnded += WhenSliderDragEnded; }
        //if (lineEdit is not null) { lineEdit.TextChanged += WhenLineEditChanged; }
        if (lineEdit is not null) { lineEdit.TextSubmitted += WhenLineEditChanged; }
        if (lineEdit is not null) { lineEdit.FocusExited += () => { WhenLineEditChanged(lineEdit.Text); }; }
        if (toggleButton is not null) { toggleButton.Toggled += WhenToggleToggled; }
        if (toggleBox is not null) { toggleBox.Toggled += WhenToggleToggled; }
        if (btnColourPicker is not null) { btnColourPicker.PopupClosed += WhenBtnColPickPopUpClosed; }
        if (btnReset is not null) { btnReset.Pressed += WhenbtnResetPressed; }
        dropdown.ItemSelected += WhenItemSelected;

        VisibilityChanged += WhenVisibilityChanged;


    }



    private void WhenVisibilityChanged()
    {
        if (!Visible) { return; }
        if (Core.CurrentProfile is null) { return; }
        // Get Field, verify it is valid and set it up
        FieldInfo fieldInfo = Config.GetType().GetField(fieldTarget);

        if (fieldInfo is null)
        {
            PropertyInfo pInfo = Config.GetType().GetProperty(fieldTarget);
            if (pInfo is null)
            {
                GD.PushError($"UISettingsEntry::_Ready() fieldInfo[{fieldTarget}] returned as NULL! Neither Field or Property found. From node[{GetPath()}]");
                return;
            }
            SetupProperty(settingsName, pInfo);
            return;
        }
        UpdateAllElements(settingsName, fieldInfo);
    }

    private void WhenSettingsChange(object sender, object e)
    {
        if (e.GetType().Name == settingsName)
        {
            FieldInfo fieldInfo = Config.GetType().GetField(fieldTarget);

            if (fieldInfo is null)
            {

                PropertyInfo pInfo = Config.GetType().GetProperty(fieldTarget);
                if (pInfo is null)
                {
                    GD.PushError($"UISettingsEntry::WhenSettingsChange() fieldInfo[{fieldTarget}] returned as NULL! Neither Field or Property found. From node[{GetPath()}]");
                    return;
                }
                SetupProperty(settingsName, pInfo);
                return;
            }
            UpdateAllElements(settingsName, fieldInfo);
        }
    }

    private void WhenBtnColPickPopUpClosed()
    {
        Color c = btnColourPicker.Color;
        Settings.SetFieldValue(settingsName, fieldTarget, c, "");
    }


    private void UpdateAllElements(string settingsName, FieldInfo field)
    {
        if (field.FieldType == typeof(float)) { FillFloat(settingsName, field); }
        if (field.FieldType == typeof(int)) { FillInt(settingsName, field); }
        if (field.FieldType == typeof(bool)) { FillBool(settingsName, field); }
        if (field.FieldType == typeof(string)) { FillString(settingsName, field); }
        //if (field.FieldType == typeof(char)) { FillString(settingsName, field); }
        if (field.FieldType == typeof(PlayerKeyBind)) { FillKeyBind(settingsName, field); }

        //if (field.FieldType == typeof(Color)) { FillColor(settingsName, field); }
        //if (field.FieldType.IsEnum) { FillEnum(_settingsName, field); }
    }
    internal void SetupProperty(string settingsName, PropertyInfo property)
    {

        if (property.PropertyType == typeof(Vector4)) { FillColor(settingsName, property); return; }
        if (property.PropertyType == typeof(Color)) { FillColor(settingsName, property); return; }
        if (property.PropertyType == typeof(String)) { FillString(settingsName, property); return; }
        //if (field.PropertyType == typeof(Enum))
        if (property.PropertyType != typeof(Type))
        {
            MethodInfo methodInfo = typeof(UISettingsEntry).GetMethod("FillPropertyEnum");
            MethodInfo genericMethodInfo = methodInfo.MakeGenericMethod(property.PropertyType);
            genericMethodInfo.Invoke(this, new object[] { settingsName, property });
            return;
        }
        GD.PrintErr($"UISettingsEntry::SetupProperty() Property Type[{property.PropertyType.GetType()}] failed to get caught");

    }


    private void FillKeyBind(string settingsTypeName, FieldInfo field)
    {
        PlayerKeyBind value = (PlayerKeyBind)Settings.GetFieldValue(settingsTypeName, field.Name);

        if (labelFieldName is not null)
        {
            if (field.GetCustomAttribute<MenuLabel>() is not null)
            {
                labelFieldName.Text = field.GetCustomAttribute<MenuLabel>().Text;
            }
            else
            {
                labelFieldName.Text = field.Name;
            }
            if (field.GetCustomAttribute<Tooltip>() is not null)
            {
                labelFieldName.TooltipText = field.GetCustomAttribute<Tooltip>().Text;
                if (labelFieldName.MouseFilter == MouseFilterEnum.Ignore) { labelFieldName.MouseFilter = MouseFilterEnum.Pass; }
            }
        }
        if (field.GetCustomAttribute<IsKeyBind>() is not null)
        {

            if (btnKeyBind is not null)
            {
                btnKeyBind.Text = value.Key == Key.None ? "M" + value.MouseButton.ToString() : value.Key.ToString();
                if (btnKeyBind.Text == "MNone") { btnKeyBind.Text = "-"; }
                if (!btnKeyBind.IsConnected(Button.SignalName.Pressed, startKB))
                {
                    btnKeyBind.Connect(Button.SignalName.Pressed, startKB);
                }
            }
            if (btnKeyBindAlt is not null)
            {
                btnKeyBindAlt.Text = value.KeyAlt == Key.None ? "M" + value.MouseButtonAlt.ToString() : value.KeyAlt.ToString();
                if (btnKeyBindAlt.Text == "MNone") { btnKeyBindAlt.Text = "-"; }
                if (!btnKeyBindAlt.IsConnected(Button.SignalName.Pressed, startKBAlt))
                {
                    btnKeyBindAlt.Connect(Button.SignalName.Pressed, startKBAlt);
                }
            }
        }
        if (lineEdit is not null) { lineEdit.Hide(); }
        if (labelValue is not null) { labelValue.Hide(); }
        if (toggleBox is not null) { toggleBox.Hide(); }
        if (toggleButton is not null) { toggleButton.Hide(); }
        if (btnColourPicker is not null) { btnColourPicker.Hide(); }
        if (slider is not null) { slider.Hide(); }
    }
    private Callable startKB => Callable.From(StartKeyBind);
    private void StartKeyBind()
    {
        UIKeybindPopup.StartKeyBind(settingsName, fieldTarget, false);
    }
    private Callable startKBAlt => Callable.From(StartKeyBindAlt);
    private void StartKeyBindAlt()
    {
        UIKeybindPopup.StartKeyBind(settingsName, fieldTarget, true);
    }

    private void WhenNewKeyBindIsMade(object sender, string[] e)
    {
        if (e[0] == settingsName && e[1] == fieldTarget)
        {
            object Config = Settings.GetCachedSettings(settingsName);
            FieldInfo fieldInfo = Config.GetType().GetField(fieldTarget);
            PlayerKeyBind value = (PlayerKeyBind)Settings.GetFieldValue(settingsName, fieldTarget);

            if (fieldInfo.GetCustomAttribute<IsKeyBind>() is not null)
            {
                if (btnKeyBind is not null)
                {
                    btnKeyBind.Text = value.Key == Key.None ? "M" + value.MouseButton.ToString() : value.Key.ToString();
                }
                if (btnKeyBindAlt is not null)
                {
                    btnKeyBindAlt.Text = value.KeyAlt == Key.None ? "M" + value.MouseButtonAlt.ToString() : value.KeyAlt.ToString();
                }
            }

        }
    }

    private void FillColor(string settingsTypeName, PropertyInfo field)
    {
        Object obj = Settings.GetPropertyValue(settingsTypeName, field.Name);
        Color value = Colors.White;
        if (obj is Vector4)
        {
            Vector4 valueTest = (Vector4)obj;
            value = new Color(valueTest.X, valueTest.Y, valueTest.Z, valueTest.W);
        }
        if (obj is Color)
        {
            value = (Color)obj;
        }
        if (labelFieldName is not null)
        {
            labelFieldName.Text = field.Name;
            if (field.GetCustomAttribute<MenuLabel>() is not null)
            {
                labelFieldName.Text = field.GetCustomAttribute<MenuLabel>().Text;
            }
            if (field.GetCustomAttribute<Tooltip>() is not null)
            {
                labelFieldName.TooltipText = field.GetCustomAttribute<Tooltip>().Text;
                if (labelFieldName.MouseFilter == MouseFilterEnum.Ignore) { labelFieldName.MouseFilter = MouseFilterEnum.Pass; }
            }
        }
        if (btnColourPicker is not null)
        {
            btnColourPicker.Color = value;
            btnColourPicker.Show();
        }
        if (btnKeyBind is not null) { btnKeyBind.Hide(); }
        if (btnKeyBindAlt is not null) { btnKeyBindAlt.Hide(); }
        if (labelValue is not null) { labelValue.Hide(); }
        if (lineEdit is not null) { lineEdit.Hide(); }
        if (slider is not null) { slider.Hide(); }
        if (toggleBox is not null) { toggleBox.Hide(); }
        if (toggleButton is not null) { toggleButton.Hide(); }
    }


    /// <summary>
    /// Works as of Nov 26 '24
    /// </summary>
    /// <param name="settingsTypeName"></param>
    /// <param name="field"></param>
    private void FillFloat(string settingsTypeName, MemberInfo field)
    {
        float value = (float)Settings.GetFieldValue(settingsTypeName, field.Name);
        if (slider is not null)
        {
            if (field.GetCustomAttribute<NormalizedVolumeAttribute>() is not null)
            {
                slider.MinValue = 0.0f;
                slider.MaxValue = 1.0f;
                slider.Step = 0.01f;
            }
            else
            {
                slider.MinValue = field.GetCustomAttribute<RangeAttribute>().Min;
                slider.MaxValue = field.GetCustomAttribute<RangeAttribute>().Max;
            }
            slider.SetValueNoSignal(value);
        }

        if (labelFieldName is not null)
        {
            labelFieldName.Text = field.Name;
            if (field.GetCustomAttribute<MenuLabel>() is not null)
            {
                labelFieldName.Text = field.GetCustomAttribute<MenuLabel>().Text;
            }
            if (field.GetCustomAttribute<Tooltip>() is not null)
            {
                labelFieldName.TooltipText = field.GetCustomAttribute<Tooltip>().Text;
                if (labelFieldName.MouseFilter == MouseFilterEnum.Ignore) { labelFieldName.MouseFilter = MouseFilterEnum.Pass; }
            }
        }

        if (field.GetCustomAttribute<EditableValue>() is null || !field.GetCustomAttribute<EditableValue>().isEditable)
        {
            if (labelValue is not null) { labelValue.Text = value.ToString("0.00"); }
            if (lineEdit is not null) { lineEdit.Hide(); }
        }
        else
        {
            if (lineEdit is not null)
            {
                lineEdit.Text = value.ToString("0.00");
            }
            if (labelValue is not null) { labelValue.Hide(); }
        }



        if (btnKeyBind is not null) { btnKeyBind.Hide(); }
        if (btnKeyBindAlt is not null) { btnKeyBindAlt.Hide(); }
        if (toggleBox is not null) { toggleBox.Hide(); }
        if (toggleButton is not null) { toggleButton.Hide(); }
        if (btnColourPicker is not null) { btnColourPicker.Hide(); }
        if (dropdown is not null) { dropdown.Hide(); }
    }
    /// <summary>
    /// Works as of Nov 26 '24
    /// </summary>
    /// <param name="settingsTypeName"></param>
    /// <param name="field"></param>
    private void FillInt(string settingsTypeName, MemberInfo field)
    {
        int value = (int)Settings.GetFieldValue(settingsTypeName, field.Name);
        if (slider is not null)
        {
            slider.MaxValue = field.GetCustomAttribute<RangeAttribute>().Max;
            slider.MinValue = field.GetCustomAttribute<RangeAttribute>().Min;
            slider.SetValueNoSignal(value);
        }
        if (labelFieldName is not null)
        {
            labelFieldName.Text = field.Name;
            if (field.GetCustomAttribute<MenuLabel>() is not null)
            {
                labelFieldName.Text = field.GetCustomAttribute<MenuLabel>().Text;
            }
            if (field.GetCustomAttribute<Tooltip>() is not null)
            {
                labelFieldName.TooltipText = field.GetCustomAttribute<Tooltip>().Text;
                if (labelFieldName.MouseFilter == MouseFilterEnum.Ignore) { labelFieldName.MouseFilter = MouseFilterEnum.Pass; }
            }
        }

        if (field.GetCustomAttribute<EditableValue>() is null || !field.GetCustomAttribute<EditableValue>().isEditable)
        {
            if (labelValue is not null) { labelValue.Text = value.ToString(); }
            if (lineEdit is not null) { lineEdit.Hide(); }
        }
        else
        {
            if (lineEdit is not null)
            {
                lineEdit.Text = value.ToString();
                //lineEdit.Size = lineEdit.GetParent<Control>().Size;
            }
            if (labelValue is not null) { labelValue.Hide(); }
        }


        if (btnKeyBind is not null) { btnKeyBind.Hide(); }
        if (btnKeyBindAlt is not null) { btnKeyBindAlt.Hide(); }
        if (toggleBox is not null) { toggleBox.Hide(); }
        if (toggleButton is not null) { toggleButton.Hide(); }
        if (btnColourPicker is not null) { btnColourPicker.Hide(); }
        if (dropdown is not null) { dropdown.Hide(); }
    }
    /// <summary>
    /// Works as of Nov 26 '24
    /// </summary>
    /// <param name="settingsTypeName"></param>
    /// <param name="field"></param>
    private void FillBool(string settingsTypeName, FieldInfo field)
    {
        bool value = (bool)Settings.GetFieldValue(settingsTypeName, field.Name);
        if (toggleBox is not null && field.GetCustomAttribute<IsCheckBox>() is not null)
        { toggleBox.SetPressedNoSignal(value); }
        else { toggleBox.Hide(); }
        if (toggleButton is not null && field.GetCustomAttribute<IsCheckBox>() is null)
        { toggleButton.SetPressedNoSignal(value); }
        else { toggleButton.Hide(); }

        if (labelFieldName is not null)
        {
            labelFieldName.Text = field.Name;
            if (field.GetCustomAttribute<MenuLabel>() is not null)
            {
                labelFieldName.Text = field.GetCustomAttribute<MenuLabel>().Text;
            }
            if (field.GetCustomAttribute<Tooltip>() is not null)
            {
                labelFieldName.TooltipText = field.GetCustomAttribute<Tooltip>().Text;
                if (labelFieldName.MouseFilter == MouseFilterEnum.Ignore) { labelFieldName.MouseFilter = MouseFilterEnum.Pass; }
            }
        }
        if (btnKeyBind is not null) { btnKeyBind.Hide(); }
        if (btnKeyBindAlt is not null) { btnKeyBindAlt.Hide(); }
        if (slider is not null) { slider.Hide(); }
        if (labelValue is not null) { labelValue.Hide(); }
        if (lineEdit is not null) { lineEdit.Hide(); }
        if (btnColourPicker is not null) { btnColourPicker.Hide(); }
        if (dropdown is not null) { dropdown.Hide(); }
    }


    private void FillString(string settingsTypeName, PropertyInfo pInfo)
    {
        string value = string.Empty;

        if (pInfo.PropertyType == typeof(char))
        {
            value = value + (char)Settings.GetPropertyValue(settingsTypeName, pInfo.Name);
        }
        else
        {
            value = (string)Settings.GetPropertyValue(settingsTypeName, pInfo.Name);
        }
        if (slider is not null) { slider.Hide(); }
        if (lineEdit is not null)
        {
            lineEdit.SizeFlagsHorizontal = SizeFlags.ExpandFill;

            if (pInfo.GetCustomAttribute<Encrypt>() is not null)
            {
#if MCOMMONS
                lineEdit.Text = Cipher.Decrypt(value.ToString());
#else
                lineEdit.Text = value.ToString();
                GD.PushWarning("MenuSystem.UISettingsEntry::FillString() MCommons not defined so no decryption done");
#endif

            }
            else
            {
                lineEdit.Text = value.ToString();
            }

            if (pInfo.GetCustomAttribute<SecretAttribute>() is not null)
            {
                lineEdit.Secret = true;
            }
            if (pInfo.GetCustomAttribute<Tooltip>() is not null) { lineEdit.TooltipText = pInfo.GetCustomAttribute<Tooltip>().Text; }
        }
        if (labelFieldName is not null)
        {
            labelFieldName.Text = pInfo.Name;
            if (pInfo.GetCustomAttribute<MenuLabel>() is not null)
            {
                labelFieldName.Text = pInfo.GetCustomAttribute<MenuLabel>().Text;
            }
            labelFieldName.Size = Vector2.Right * Size.X * 0.5f;
            if (pInfo.GetCustomAttribute<Tooltip>() is not null)
            {
                labelFieldName.TooltipText = pInfo.GetCustomAttribute<Tooltip>().Text;
                if (labelFieldName.MouseFilter == MouseFilterEnum.Ignore) { labelFieldName.MouseFilter = MouseFilterEnum.Pass; }
            }
        }
        if (btnKeyBind is not null) { btnKeyBind.Hide(); }
        if (btnKeyBindAlt is not null) { btnKeyBindAlt.Hide(); }
        if (labelValue is not null) { labelValue.Hide(); }
        if (toggleBox is not null) { toggleBox.Hide(); }
        if (toggleButton is not null) { toggleButton.Hide(); }
        if (btnColourPicker is not null) { btnColourPicker.Hide(); }
        if (dropdown is not null) { dropdown.Hide(); }
    }

    private void FillString(string settingsTypeName, FieldInfo field)
    {
        string value = string.Empty;

        if (field.FieldType == typeof(char))
        {
            value = value + (char)Settings.GetFieldValue(settingsTypeName, field.Name);
        }
        else
        {
            value = (string)Settings.GetFieldValue(settingsTypeName, field.Name);
        }

        if (slider is not null) { slider.Hide(); }
        if (lineEdit is not null)
        {
            lineEdit.SizeFlagsHorizontal = SizeFlags.ExpandFill;

            if (field.GetCustomAttribute<Encrypt>() is not null)
            {
#if MCOMMONS
                lineEdit.Text = Cipher.Decrypt(value.ToString());
#else
                lineEdit.Text = value.ToString();
                GD.PushWarning("MenuSystem.UISettingsEntry::FillString() MCommons not defined so no decryption done");
#endif

            }
            else
            {
                lineEdit.Text = value.ToString();
            }

            if (field.GetCustomAttribute<SecretAttribute>() is not null)
            {
                lineEdit.Secret = true;
            }
            if (field.GetCustomAttribute<Tooltip>() is not null) { lineEdit.TooltipText = field.GetCustomAttribute<Tooltip>().Text; }
        }
        if (labelFieldName is not null)
        {
            labelFieldName.Text = field.Name;
            if (field.GetCustomAttribute<MenuLabel>() is not null)
            {
                labelFieldName.Text = field.GetCustomAttribute<MenuLabel>().Text;
            }
            labelFieldName.Size = Vector2.Right * Size.X * 0.5f;
            if (field.GetCustomAttribute<Tooltip>() is not null)
            {
                labelFieldName.TooltipText = field.GetCustomAttribute<Tooltip>().Text;
                if (labelFieldName.MouseFilter == MouseFilterEnum.Ignore) { labelFieldName.MouseFilter = MouseFilterEnum.Pass; }
            }
        }
        if (btnKeyBind is not null) { btnKeyBind.Hide(); }
        if (btnKeyBindAlt is not null) { btnKeyBindAlt.Hide(); }
        if (labelValue is not null) { labelValue.Hide(); }
        if (toggleBox is not null) { toggleBox.Hide(); }
        if (toggleButton is not null) { toggleButton.Hide(); }
        if (btnColourPicker is not null) { btnColourPicker.Hide(); }
        if (dropdown is not null) { dropdown.Hide(); }
    }
    private void FillEnum(string settingsTypeName, FieldInfo field)
    {
        Enum value = (Enum)Settings.GetFieldValue(settingsTypeName, field.Name);

        dropdown.Clear();
        //List<string> options = new List<string>();
        // Add current picked first
        //options.Add(value.ToString());
        foreach (int enumValue in Enum.GetValues(field.FieldType))
        {
            // Exclude current pick
            if (value.ToString() == Enum.GetName(field.FieldType, enumValue)) { continue; }
            dropdown.AddItem(Enum.GetName(field.FieldType, enumValue));
        }
    }
    public void FillPropertyEnum<T>(string settingsTypeName, PropertyInfo pInfo) where T : Enum
    {
        T value = (T)Settings.GetPropertyValue(settingsTypeName, pInfo.Name);
        if (labelFieldName is not null)
        {
            labelFieldName.Text = pInfo.Name;
            if (pInfo.GetCustomAttribute<MenuLabel>() is not null)
            {
                labelFieldName.Text = pInfo.GetCustomAttribute<MenuLabel>().Text;
            }
            labelFieldName.Size = Vector2.Right * Size.X * 0.5f;
            if (pInfo.GetCustomAttribute<Tooltip>() is not null)
            {
                labelFieldName.TooltipText = pInfo.GetCustomAttribute<Tooltip>().Text;
                if (labelFieldName.MouseFilter == MouseFilterEnum.Ignore) { labelFieldName.MouseFilter = MouseFilterEnum.Pass; }
            }
        }
        dropdown.Clear();
        //List<string> options = new List<string>();
        // Add current picked first
        //options.Add(value.ToString());

        foreach (long enumValue in Enum.GetValues(typeof(T)))
        {
            // Exclude current pick
            //if (value.ToString() == Enum.GetName(pInfo.PropertyType, enumValue)) { continue; }
            dropdown.AddItem(Enum.GetName(pInfo.PropertyType, enumValue));
        }
        if (dropdown is not null) { dropdown.Show(); }
        dropdown.Selected = -1;

        if (slider is not null) { slider.Hide(); }
        if (lineEdit is not null) { lineEdit.Hide(); }
        if (btnKeyBind is not null) { btnKeyBind.Hide(); }
        if (btnKeyBindAlt is not null) { btnKeyBindAlt.Hide(); }
        if (labelValue is not null) { labelValue.Hide(); }
        if (toggleBox is not null) { toggleBox.Hide(); }
        if (toggleButton is not null) { toggleButton.Hide(); }
        if (btnColourPicker is not null) { btnColourPicker.Hide(); }
    }

    private void WhenItemSelected(long index)
    {
        Settings.SetPropertyEnumValue(settingsName, fieldTarget, (int)index, "");
        MethodInfo methodInfo = typeof(UISettingsEntry).GetMethod("FillPropertyEnum");
        PropertyInfo pInfo = Config.GetType().GetProperty(fieldTarget);
        MethodInfo genericMethodInfo = methodInfo.MakeGenericMethod(pInfo.PropertyType);
        genericMethodInfo.Invoke(this, new object[] { settingsName, pInfo });
    }


    #region Listeners
    private void WhenbtnResetPressed()
    {
        Settings.ResetField(settingsName, fieldTarget, "");
        FieldInfo fieldInfo = Config.GetType().GetField(fieldTarget);
        if (fieldInfo is null)
        {
            PropertyInfo pInfo = Config.GetType().GetProperty(fieldTarget);
            if (pInfo is null)
            {
                GD.PushError($"UISettingsEntry::WhenbtnResetPressed() fieldInfo[{fieldTarget}] returned as NULL! Neither Field or Property found");
                return;
            }
            SetupProperty(settingsName, pInfo);
            return;
        }
        UpdateAllElements(settingsName, fieldInfo);
    }
    private void WhenLineEditChanged(string newText)
    {
        FieldInfo fieldInfo = Config.GetType().GetField(fieldTarget);
        if (fieldInfo is null) { WhenLineEditChangedProperty(newText); return; }
        if (fieldInfo.FieldType == typeof(float))
        {
            if (float.TryParse(newText, out float value))
            {
                Settings.SetFieldValue(settingsName, fieldTarget, (float)value, "");
                FillFloat(settingsName, fieldInfo);
            }
        }
        if (fieldInfo.FieldType == typeof(int))
        {
            if (int.TryParse(newText, out int value))
            {
                Settings.SetFieldValue(settingsName, fieldTarget, (int)value, "");
                FillInt(settingsName, fieldInfo);
            }
        }
        if (fieldInfo.FieldType == typeof(string))
        {
            if (fieldInfo.GetCustomAttribute<Encrypt>() is not null)
            {

#if MCOMMONS
                Settings.SetFieldValue(settingsName, fieldTarget, Cipher.Encrypt(newText), "");
#else
                Settings.SetFieldValue(settingsName, fieldTarget, newText, "");
                GD.PushWarning("MenuSystem.UISettingsEntry::WhenLineEditChanged() MCommons not defined so no encryption done");
#endif

            }
            else
            {
                Settings.SetFieldValue(settingsName, fieldTarget, newText, "");
            }
            FillString(settingsName, fieldInfo);
        }
        if (fieldInfo.FieldType == typeof(char))
        {
            if (newText.Length > 0)
            {
                Settings.SetFieldValue(settingsName, fieldTarget, newText[0], "");
                FillString(settingsName, fieldInfo);
            }
        }
    }

    private void WhenLineEditChangedProperty(string newText)
    {
        PropertyInfo pInfo = Config.GetType().GetProperty(fieldTarget);
        if (pInfo.PropertyType == typeof(float))
        {
            if (float.TryParse(newText, out float value))
            {
                Settings.SetFieldValue(settingsName, fieldTarget, (float)value, "");
                FillFloat(settingsName, pInfo);
            }
        }
        if (pInfo.PropertyType == typeof(int))
        {
            if (int.TryParse(newText, out int value))
            {
                Settings.SetFieldValue(settingsName, fieldTarget, (int)value, "");
                FillInt(settingsName, pInfo);
            }
        }
        if (pInfo.PropertyType == typeof(string))
        {
            if (pInfo.GetCustomAttribute<Encrypt>() is not null)
            {

#if MCOMMONS
                Settings.SetFieldValue(settingsName, fieldTarget, Cipher.Encrypt(newText), "");
#else
                Settings.SetFieldValue(settingsName, fieldTarget, newText, "");
                GD.PushWarning("MenuSystem.UISettingsEntry::WhenLineEditChanged() MCommons not defined so no encryption done");
#endif

            }
            else
            {
                Settings.SetFieldValue(settingsName, fieldTarget, newText, "");
            }
            FillString(settingsName, pInfo);
        }
        if (pInfo.PropertyType == typeof(char))
        {
            if (newText.Length > 0)
            {
                Settings.SetFieldValue(settingsName, fieldTarget, newText[0], "");
                FillString(settingsName, pInfo);
            }
        }
    }


    private void WhenSliderDragEnded(bool valueChanged)
    {
        if (valueChanged)
        {
            WhenSliderValueChanged(slider.Value);
        }
    }
    private void WhenSliderValueChanged(double value)
    {
        FieldInfo fieldInfo = Config.GetType().GetField(fieldTarget);
        if (fieldInfo.FieldType == typeof(float))
        {
            Settings.SetFieldValue(settingsName, fieldTarget, (float)value, "");
            FillFloat(settingsName, fieldInfo);
        }
        if (fieldInfo.FieldType == typeof(int))
        {
            Settings.SetFieldValue(settingsName, fieldTarget, (int)value, "");
            FillInt(settingsName, fieldInfo);
        }
    }
    private void WhenToggleToggled(bool toggleState)
    {
        Settings.SetFieldValue(settingsName, fieldTarget, toggleState, "");
        FieldInfo fieldInfo = Config.GetType().GetField(fieldTarget);
        if (fieldInfo.FieldType == typeof(bool)) { FillBool(settingsName, fieldInfo); }
    }
    #endregion
}// EOF CLASS