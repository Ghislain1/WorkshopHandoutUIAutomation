using CustomerApp.UiTests.Infrastructure;
using CustomerApp.UiTests.PageObjects;
using FlaUI.Core.Tools;

namespace CustomerApp.UiTests;

[TestClass]
public sealed class CustomerManagementE2eTests : WpfAppTestBase {
    private MainWindowPage CreatePage() {
        var mainWindow = Retry.WhileNull(
            () => App.GetMainWindow(Automation),
            timeout: TimeSpan.FromSeconds(10)).Result
            ?? throw new InvalidOperationException("Hauptfenster nicht gefunden.");

        return new MainWindowPage(mainWindow);
    }

    [TestMethod]
    public void AddCustomer_ShowsNewRow() {
        var page = CreatePage();
        var initialCount = page.RowCount;

        page.AddCustomer();

        page.WaitUntilRowCountIs(initialCount + 1);

        Assert.AreEqual(initialCount + 1, page.RowCount);
    }

    [TestMethod]
    public void EditCustomerName_UpdatesGrid() {
        var page = CreatePage();

        page.SelectRow(0);
        page.EditCell(0, "Name", "Aktualisierter Kunde");

        Assert.AreEqual(
            "Aktualisierter Kunde",
            page.GetCellValue(0, "Name"));
    }

    [TestMethod]
    public void DeleteSelectedCustomer_RemovesRow() {
        var page = CreatePage();
        var initialCount = page.RowCount;

        page.SelectRow(0);
        page.DeleteSelectedCustomer();

        page.WaitUntilRowCountIs(initialCount - 1);

        Assert.AreEqual(initialCount - 1, page.RowCount);
    }

    [TestMethod]
    public void FullWorkflow_AddEditAndDeleteCustomer() {
        var page = CreatePage();
        var initialCount = page.RowCount;

        page.AddCustomer();
        page.WaitUntilRowCountIs(initialCount + 1);

        page.SelectRow(initialCount);
        page.EditCell(initialCount, "Name", "E2E Kunde");

        Assert.AreEqual(
            "E2E Kunde",
            page.GetCellValue(initialCount, "Name"));

        page.DeleteSelectedCustomer();
        page.WaitUntilRowCountIs(initialCount);
    }
}