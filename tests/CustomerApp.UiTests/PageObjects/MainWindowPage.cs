using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerApp.UiTests.PageObjects;
    internal class MainWindowPage {
    private readonly Window _window;

    public MainWindowPage(Window window) {
        _window = window;
    }

    private Button AddButton => _window
        .FindFirstDescendant(cf => cf.ByAutomationId("AddCustomerButton"))
        .AsButton();

    private Button DeleteButton => _window
        .FindFirstDescendant(cf => cf.ByAutomationId("DeleteCustomerButton"))
        .AsButton();

    private DataGrid CustomerGrid => _window
        .FindFirstDescendant(cf => cf.ByAutomationId("CustomerDataGrid"))
        .AsGrid();

    public int RowCount => CustomerGrid.Rows.Length;

    public void AddCustomer() {
        AddButton.Invoke();
    }

    public void DeleteSelectedCustomer() {
        DeleteButton.Invoke();
    }

    public void SelectRow(int rowIndex) {
        var row = CustomerGrid.Rows[rowIndex];
        row.Select();
    }
    public string GetCellValue(int rowIndex, string columnHeader) {
        var row = CustomerGrid.Rows[rowIndex];

        var cell = Retry.WhileNull(
            () => row.FindFirstDescendant(
                cf => cf.ByName(columnHeader)),
            timeout: TimeSpan.FromSeconds(2)).Result;

        return cell.AsTextBox().Text;
    }

    public void EditCell(int rowIndex, string columnHeader, string newValue) {
        var row = CustomerGrid.Rows[rowIndex];

        var cell = row.FindFirstDescendant(
            cf => cf.ByName(columnHeader));

        var textBox = cell.AsTextBox();
        textBox.Text = newValue;
    }

    public void WaitUntilRowCountIs(int expectedCount) {
        Retry.WhileFalse(
            () => RowCount == expectedCount,
            timeout: TimeSpan.FromSeconds(5));
    }

}
