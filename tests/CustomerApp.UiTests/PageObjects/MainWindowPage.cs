using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using System;

namespace CustomerApp.UiTests.PageObjects;

internal class MainWindowPage {
    private readonly Window _window;

    public MainWindowPage(Window window) {
        _window = window;
    }

    private Button AddButton => _window
        .FindFirstDescendant(cf => cf.ByAutomationId("AddCustomerButton"))!
        .AsButton();

    private Button DeleteButton => _window
        .FindFirstDescendant(cf => cf.ByAutomationId("DeleteCustomerButton"))!
        .AsButton();

    private Button SaveButton => _window
      .FindFirstDescendant(cf => cf.ByAutomationId("SaveCustomersButton"))!
      .AsButton();

    private Grid CustomerGrid => _window
        .FindFirstDescendant(cf => cf.ByAutomationId("CustomerDataGrid"))!
        .AsGrid();

    private TextBox SearchTextBox => _window
     .FindFirstDescendant(cf => cf.ByAutomationId("SearchBox"))!
     .AsTextBox();

    public int RowCount => CustomerGrid.Rows.Length;

    public void AddCustomer() {
        AddButton.Invoke();
    }

    public void SearchCustomer(string pattern) {
        SearchTextBox.Text = pattern;
    }

    public void SaveChanges() {
        SaveButton.Invoke();
    }

    public void DeleteSelectedCustomer() {
        var invokeTask = Task.Run(() => DeleteButton.Invoke());

        var yesButton = Retry.WhileNull(
            () => _window.FindFirstDescendant(cf => cf.ByAutomationId("6")),
            timeout: TimeSpan.FromSeconds(5)).Result
            ?? throw new InvalidOperationException("Bestätigungsdialog nicht gefunden.");

        yesButton.AsButton().Invoke();
        invokeTask.GetAwaiter().GetResult();
    }

    public void SelectRow(int rowIndex) {
        CustomerGrid.GetRowByIndex(rowIndex)!.Select();
    }

    public string GetCellValue(int rowIndex, string columnHeader) =>
        GetCell(rowIndex, columnHeader).Value ?? string.Empty;

    public void EditCell(int rowIndex, string columnHeader, string newValue) {
        var cell = GetCell(rowIndex, columnHeader);
        cell.DoubleClick();

        var edit = Retry.WhileNull(
            () => cell.FindFirstDescendant(cf => cf.ByControlType(ControlType.Edit)),
            timeout: TimeSpan.FromSeconds(2)).Result
            ?? throw new InvalidOperationException(
                $"Edit-Control für Spalte '{columnHeader}' nicht gefunden.");

        var textBox = edit.AsTextBox();
        textBox.Text = newValue;
        textBox.Focus();
        Keyboard.Press(VirtualKeyShort.ENTER);

        Retry.WhileFalse(
            () => GetCellValue(rowIndex, columnHeader) == newValue,
            timeout: TimeSpan.FromSeconds(2));
    }

    public void WaitUntilRowCountIs(int expectedCount) {
        Retry.WhileFalse(
            () => RowCount == expectedCount,
            timeout: TimeSpan.FromSeconds(5));
    }

    private GridCell GetCell(int rowIndex, string columnHeader) {
        var row = CustomerGrid.GetRowByIndex(rowIndex)!;
        return row.Cells[GetColumnIndex(columnHeader)];
    }
  
    private int GetColumnIndex(string columnHeader) {
        var columns = CustomerGrid.Header!.Columns;

        for (var i = 0; i < columns.Length; i++) {
            if (string.Equals(columns[i].Text, columnHeader, StringComparison.Ordinal)) {
                return i;
            }
        }

        throw new ArgumentException(
            $"Spalte '{columnHeader}' nicht gefunden.",
            nameof(columnHeader));
    }
}
