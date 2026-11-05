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

        public bool Confirm(string title, string message) {
            WasCalled = true;
            return Result;
        }
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

}
