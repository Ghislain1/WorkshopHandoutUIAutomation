using CustomerApp.UiTests.Infrastructure;
using CustomerApp.UiTests.PageObjects;
using FlaUI.Core.Tools;

namespace CustomerApp.UiTests;

[TestClass]
[DoNotParallelize]
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
    public void SearchCustomer_WithoutMatches_ShowsEmptyGrid() {
        var page = CreatePage();
        Assert.IsTrue(page.RowCount > 0);

        page.SearchCustomer("kein-treffer-123456");

        page.WaitUntilRowCountIs(0);

        Assert.AreEqual(0, page.RowCount);
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


    [TestMethod]
    public void Ghis_SearchSelectCustomer_ChangeEmail_Save_RestartApp_CheckChanges() {
        var page = CreatePage();
        WaitForConvience(2);
        var initialCount = page.RowCount;

        // Suche → Kunde 
        page.SearchCustomer("Ghislain");
        page.WaitUntilRowCountIs(initialCount - 2);
        WaitForConvience(2);

        // Kunde auswählen 
        page.SelectRow(initialCount-3);
        WaitForConvience();

        // E-Mail ändern
        page.EditCell(initialCount-3, "E-Mail", "virginie@dotnet.com");
        WaitForConvience();
        
        // Speichern
        page.SaveChanges();

        // Anwendung neu starten
        RestartApplication();
        page = CreatePage();
        Assert.AreEqual(3, page.RowCount);
        WaitForConvience();


    }
}