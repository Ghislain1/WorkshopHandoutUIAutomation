// // Description:Definition of ${File.FileName} class
// //
// // Copyright © 2025 - 2025, Alcon.All rights reserved.

using CustomerApp.Models;
using CustomerApp.Services;
using CustomerApp.ViewModels;

namespace CustomerApp.Tests;

[TestClass]
public sealed class MainViewModelTests {
    private sealed class FakeDialogService : IDialogService {
        public bool Result { get; set; }
        public bool WasCalled { get; private set; }
        public string LastTitle { get; private set; } = string.Empty;
        public string LastMessage { get; private set; } = string.Empty;

        public bool Confirm(string title, string message) {
            WasCalled = true;
            LastTitle = title;
            LastMessage = message;
            return Result;
        }
    }

    private sealed class FakeCustomerRepository : ICustomerRepository {
        private readonly List<Customer> _customers = [];

        public bool SaveCalled { get; private set; }

        public IReadOnlyList<Customer> GetAll() => _customers;

        public void Add(Customer customer) => _customers.Add(customer);

        public void Remove(int id) => _customers.RemoveAll(c => c.Id == id);

        public void Save() => SaveCalled = true;
    }

    [TestMethod]
    public void DeleteCustomer_WithConfirmation_RemovesCustomer() {
        var repository = new InMemoryCustomerRepository();
        var dialog = new FakeDialogService { Result = true };
        var sut = new MainViewModel(repository, dialog);
        sut.SelectedCustomer = sut.Customers[0];

        sut.DeleteCustomerCommand.Execute(null);

        Assert.HasCount(1, sut.Customers);
        Assert.HasCount(1, repository.GetAll());
        Assert.IsNull(sut.SelectedCustomer);
        Assert.IsTrue(dialog.WasCalled);
        Assert.AreEqual("Kunde löschen", dialog.LastTitle);
        StringAssert.Contains(dialog.LastMessage, "Anna Weber");
    }

    [TestMethod]
    public void DeleteCustomer_WithoutConfirmation_KeepsCustomer() {
        var repository = new InMemoryCustomerRepository();
        var dialog = new FakeDialogService { Result = false };
        var sut = new MainViewModel(repository, dialog);
        sut.SelectedCustomer = sut.Customers[0];

        sut.DeleteCustomerCommand.Execute(null);

        Assert.HasCount(2, sut.Customers);
        Assert.HasCount(2, repository.GetAll());
        Assert.IsNotNull(sut.SelectedCustomer);
        Assert.IsTrue(dialog.WasCalled);
    }

    [TestMethod]
    public void DeleteCustomerCommand_CannotExecuteWithoutSelection() {
        var sut = new MainViewModel(
            new InMemoryCustomerRepository(),
            new FakeDialogService());

        Assert.IsFalse(sut.DeleteCustomerCommand.CanExecute(null));
    }

    private static MainViewModel CreateSut() {
        var dialog = new FakeDialogService { Result = true };
        return new MainViewModel(new InMemoryCustomerRepository(),dialog);
    }
    [TestMethod]
    public void Constructor_LoadsCustomersFromRepository() {
        var sut = CreateSut();

        Assert.HasCount(2, sut.Customers);
        Assert.AreEqual("Anna Weber", sut.Customers[0].Name);
    }

    [TestMethod]
    public void AddCustomer_AddsRowAndSelectsIt() {
        var sut = CreateSut();

        sut.AddCustomerCommand.Execute(null);

        Assert.HasCount(3, sut.Customers);
        Assert.IsNotNull(sut.SelectedCustomer);
        Assert.AreEqual(3, sut.SelectedCustomer!.Id);
    }

    [TestMethod]
    public void DeleteCustomer_RemovesSelectedCustomer() {
        var sut = CreateSut();
        sut.SelectedCustomer = sut.Customers[0];

        sut.DeleteCustomerCommand.Execute(null);

        Assert.HasCount(1, sut.Customers);
        Assert.IsNull(sut.SelectedCustomer);
    }

    [TestMethod]
    public void DeleteCustomerCommand_CannotExecuteWithoutSelection2() {
        var sut = CreateSut();

        Assert.IsFalse(sut.DeleteCustomerCommand.CanExecute(null));
    }
    [TestMethod]
    public void Customer_Validation_RejectsMissingName() {
        var customer = new Customer { Name = "", Email = "test@example.com" };

        Assert.AreEqual("Name ist erforderlich.", customer["Name"]);
    }

    [TestMethod]
    public void SaveCustomers_WithInvalidCustomer_DoesNotSave() {
        var repository = new FakeCustomerRepository();
        repository.Add(new Customer {
            Id = 1,
            Name = string.Empty,
            Email = "invalid",
            Phone = "123"
        });
        var sut = new MainViewModel(repository, new FakeDialogService());

        sut.SaveCustomersCommand.Execute(null);

        Assert.IsFalse(repository.SaveCalled);
        Assert.AreEqual(
            "Speichern nicht möglich: 1 Kunde enthält ungültige Daten.",
            sut.StatusMessage);
    }

    [TestMethod]
    public void SaveCustomers_WithValidCustomers_Saves() {
        var repository = new FakeCustomerRepository();
        repository.Add(new Customer {
            Id = 1,
            Name = "Anna Weber",
            Email = "anna@example.com",
            Phone = "030123456"
        });
        var sut = new MainViewModel(repository, new FakeDialogService());

        sut.SaveCustomersCommand.Execute(null);

        Assert.IsTrue(repository.SaveCalled);
        Assert.AreEqual("Gespeichert.", sut.StatusMessage);
    }

    [TestMethod]
    public void SearchText_FiltersCustomersByName() {
        var sut = CreateSut();

        sut.SearchText = "Anna";

        var visible = sut.FilteredCustomers.Cast<Customer>().ToList();
        Assert.HasCount(1, visible);
        Assert.AreEqual("Anna Weber", visible[0].Name);
    }

    [TestMethod]
    public void SearchText_FiltersCustomersByEmailAndPhone() {
        var sut = CreateSut();

        sut.SearchText = "ben@example.com";
        Assert.HasCount(1, sut.FilteredCustomers.Cast<Customer>().ToList());

        sut.SearchText = "040654321";
        var visible = sut.FilteredCustomers.Cast<Customer>().ToList();
        Assert.HasCount(1, visible);
        Assert.AreEqual("Ben Schmidt", visible[0].Name);
    }

    [TestMethod]
    public void SearchText_Empty_ShowsAllCustomers() {
        var sut = CreateSut();
        sut.SearchText = "Anna";

        sut.SearchText = string.Empty;

        Assert.HasCount(2, sut.FilteredCustomers.Cast<Customer>().ToList());
    }
}
