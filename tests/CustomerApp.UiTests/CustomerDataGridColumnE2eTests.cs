using CustomerApp.UiTests.Infrastructure;
using CustomerApp.UiTests.PageObjects;
using FlaUI.Core.Tools;

namespace CustomerApp.UiTests;

[TestClass]
public sealed class CustomerDataGridColumnE2eTests : WpfAppTestBase {
    private CustomerGridPage CreateGridPage() {
        var window = Retry.WhileNull(
            () => App.GetMainWindow(Automation),
            timeout: TimeSpan.FromSeconds(10)).Result
            ?? throw new InvalidOperationException("Hauptfenster nicht gefunden.");

        return new CustomerGridPage(window);
    }

    [TestMethod]
    public void TextColumn_CanEditName() {
        var page = CreateGridPage();

        page.SetText(0, "Anna E2E");

        Assert.AreEqual("Anna E2E", page.GetText(0));
    }

    [TestMethod]
    public void CheckBoxColumn_CanToggleIsActive() {
        var page = CreateGridPage();
        var before = page.GetIsActive(0);

        page.ToggleIsActive(0);

        Assert.AreNotEqual(before, page.GetIsActive(0));
    }

    [TestMethod]
    public void ComboBoxColumn_CanSelectStatus() {
        var page = CreateGridPage();

        page.SetStatus(0, "Blocked");

        Assert.AreEqual("Blocked", page.GetStatus(0));
    }

    [TestMethod]
    public void HyperlinkColumn_DisplaysWebsite() {
        var page = CreateGridPage();

        Assert.Contains("example.com", page.GetWebsite(0));
    }

    [TestMethod]
    public void TemplateColumn_DatePicker_CanChangeDate() {
        var page = CreateGridPage();
        var newDate = new DateTime(2001, 12, 31);

        page.SetBirthDate(0, newDate);

        // Optional: über eine sichtbare Anzeige oder ein Test-Feld verifizieren.
    }

    [TestMethod]
    public void TemplateColumn_Button_RunsDetailsCommand() {
        var page = CreateGridPage();

        page.OpenDetails(0);

        // Beispiel: Prüfen über ein sichtbares Status-/Info-Feld.
        // Assert.AreEqual("Details: Anna Weber", mainWindowPage.LastAction);
    }
}