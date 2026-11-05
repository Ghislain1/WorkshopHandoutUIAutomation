using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Tools;

namespace CustomerApp.UiTests.PageObjects;

public sealed class CustomerGridPage {
    private readonly DataGrid _grid;

    public CustomerGridPage(Window window) {
        _grid = window
            .FindFirstDescendant(cf => cf.ByAutomationId("CustomerDataGrid"))
            .AsGrid();
    }

    public int RowCount => _grid.Rows.Length;

    public DataGridRow GetRow(int index) => _grid.Rows[index];

    private AutomationElement GetCell(int rowIndex, string columnAutomationId) {
        var row = GetRow(rowIndex);

        return Retry.WhileNull(
            () => row.FindFirstDescendant(
                cf => cf.ByAutomationId(columnAutomationId)),
            timeout: TimeSpan.FromSeconds(3)).Result;
    }

    // DataGridTextColumn
    public string GetText(int rowIndex) =>
        GetCell(rowIndex, "NameCell").AsTextBox().Text;

    public void SetText(int rowIndex, string value) {
        var cell = GetCell(rowIndex, "NameCell");
        cell.DoubleClick();
        var textBox = Retry.WhileNull(
            () => cell.FindFirstDescendant(cf => cf.ByControlType(ControlType.Edit)),
            timeout: TimeSpan.FromSeconds(2)).Result.AsTextBox();

        textBox.Text = value;
        Keyboard.PressVirtualKeyCode(FlaUI.Core.Input.VirtualKeyShort.ENTER);
    }

    // DataGridCheckBoxColumn
    public bool GetIsActive(int rowIndex) =>
        GetCell(rowIndex, "IsActiveCell").AsCheckBox().IsToggled;

    public void ToggleIsActive(int rowIndex) {
        GetCell(rowIndex, "IsActiveCell").AsCheckBox().Toggle();
    }

    // DataGridComboBoxColumn
    public string GetStatus(int rowIndex) =>
        GetCell(rowIndex, "StatusCell").AsComboBox().Value;

    public void SetStatus(int rowIndex, string value) {
        var comboBox = GetCell(rowIndex, "StatusCell").AsComboBox();
        comboBox.Select(value);
    }

    // DataGridHyperlinkColumn
    public string GetWebsite(int rowIndex) =>
        GetCell(rowIndex, "WebsiteCell").Name;

    // DataGridTemplateColumn: DatePicker
    public void SetBirthDate(int rowIndex, DateTime date) {
        var cell = GetCell(rowIndex, "BirthDateCell");
        cell.DoubleClick();

        var datePicker = Retry.WhileNull(
            () => cell.FindFirstDescendant(
                cf => cf.ByAutomationId("BirthDatePicker")),
            timeout: TimeSpan.FromSeconds(2)).Result;

        datePicker.Patterns.Value.Pattern.SetValue(date.ToString("d"));
    }

    // DataGridTemplateColumn: Button
    public void OpenDetails(int rowIndex) {
        GetCell(rowIndex, "ActionsCell")
            .FindFirstDescendant(cf => cf.ByAutomationId("CustomerDetailsButton"))
            .AsButton()
            .Invoke();
    }
}