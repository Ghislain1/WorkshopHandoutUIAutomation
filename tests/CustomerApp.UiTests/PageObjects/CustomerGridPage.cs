using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;

namespace CustomerApp.UiTests.PageObjects;

public sealed class CustomerGridPage {
    private readonly Grid _grid;

    public CustomerGridPage(Window window) {
        ArgumentNullException.ThrowIfNull(window);

        _grid = window
            .FindFirstDescendant(cf => cf.ByAutomationId("CustomerDataGrid"))
            ?.AsGrid()
            ?? throw new InvalidOperationException("CustomerDataGrid nicht gefunden.");
    }

    public int RowCount => _grid.Rows.Length;

    public GridRow GetRow(int index) => _grid.Rows[index];

    private AutomationElement GetCell(int rowIndex, string columnAutomationId) {
        var row = GetRow(rowIndex);

        return Retry.WhileNull(
            () => row.FindFirstDescendant(
                cf => cf.ByAutomationId(columnAutomationId)),
            timeout: TimeSpan.FromSeconds(3)).Result
            ?? throw new InvalidOperationException(
                $"Zelle '{columnAutomationId}' nicht gefunden.");
    }

    // DataGridTextColumn
    public string GetText(int rowIndex) =>
        GetCell(rowIndex, "NameCell").AsTextBox().Text;

    public void SetText(int rowIndex, string value) {
        var cell = GetCell(rowIndex, "NameCell");
        cell.DoubleClick();
        var textBox = Retry.WhileNull(
            () => cell.FindFirstDescendant(cf => cf.ByControlType(ControlType.Edit)),
            timeout: TimeSpan.FromSeconds(2)).Result?.AsTextBox()
            ?? throw new InvalidOperationException("Edit-Control nicht gefunden.");

        textBox.Text = value;
        Keyboard.Press(VirtualKeyShort.ENTER);
    }

    // DataGridCheckBoxColumn
    public bool GetIsActive(int rowIndex) =>
        GetCell(rowIndex, "IsActiveCell").AsCheckBox().IsToggled ?? false;

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
            timeout: TimeSpan.FromSeconds(2)).Result
            ?? throw new InvalidOperationException("BirthDatePicker nicht gefunden.");

        datePicker.Patterns.Value.Pattern.SetValue(date.ToString("d"));
    }

    // DataGridTemplateColumn: Button
    public void OpenDetails(int rowIndex) {
        var detailsButton = GetCell(rowIndex, "ActionsCell")
            .FindFirstDescendant(cf => cf.ByAutomationId("CustomerDetailsButton"))?.AsButton()
            ?? throw new InvalidOperationException("CustomerDetailsButton nicht gefunden.");

        detailsButton.Invoke();
    }
}