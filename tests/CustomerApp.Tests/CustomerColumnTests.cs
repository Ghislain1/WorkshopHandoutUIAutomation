using CustomerApp.Models;
using CustomerApp.Services;
using CustomerApp.ViewModels;

namespace CustomerApp.Tests;

[TestClass]
public sealed class CustomerColumnTests {
    private static Customer CreateCustomer() => new() {
        Id = 1,
        Name = "Anna Weber",
        Email = "anna@example.com",
        IsActive = true,
        Status = CustomerStatus.Active,
        Website = new Uri("https://example.com"),
        BirthDate = new DateTime(1990, 1, 1)
    };

    [TestMethod]
    public void TextColumn_Name_CanBeChanged() {
        var customer = CreateCustomer();

        customer.Name = "Neuer Name";

        Assert.AreEqual("Neuer Name", customer.Name);
    }

    [TestMethod]
    public void TextColumn_Email_RaisesPropertyChanged() {
        var customer = CreateCustomer();
        var changed = false;

        customer.PropertyChanged += (_, e) => {
            if (e.PropertyName == nameof(Customer.Email)) {
                changed = true;
            }
        };

        customer.Email = "new@example.com";

        Assert.IsTrue(changed);
    }

    [TestMethod]
    public void CheckBoxColumn_IsActive_TogglesValue() {
        var customer = CreateCustomer();

        customer.IsActive = false;

        Assert.IsFalse(customer.IsActive);
    }

    [TestMethod]
    public void ComboBoxColumn_Status_AcceptsAllEnumValues() {
        var customer = CreateCustomer();

        foreach (CustomerStatus status in Enum.GetValues<CustomerStatus>()) {
            customer.Status = status;
            Assert.AreEqual(status, customer.Status);
        }
    }

    [TestMethod]
    public void HyperlinkColumn_Website_StoresUri() {
        var customer = CreateCustomer();

        customer.Website = new Uri("https://flaui.com");

        Assert.AreEqual("https://flaui.com/", customer.Website!.ToString());
    }

    [TestMethod]
    public void TemplateColumn_DatePicker_RejectsFutureDate() {
        var customer = CreateCustomer();
        customer.BirthDate = DateTime.Today.AddDays(1);
        

        Assert.AreEqual(
            "Geburtsdatum darf nicht in der Zukunft liegen.",
            customer[nameof(Customer.BirthDate)]);
    }

    [TestMethod]
    public void ShowDetailsCommand_SetsLastAction() {
        var sut = new MainViewModel(new InMemoryCustomerRepository(), new MessageBoxDialogService());
        var customer = CreateCustomer();

        sut.ShowDetailsCommand.Execute(customer);

        Assert.AreEqual("Details: Anna Weber", sut.LastAction);
    }
}