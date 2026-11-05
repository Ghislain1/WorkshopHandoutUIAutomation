// // Description:Definition of ${File.FileName} class
// //
// // Copyright © 2025 - 2025, Alcon.All rights reserved.

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CustomerApp.Models;
using CustomerApp.Services;

namespace CustomerApp.ViewModels;

public partial class MainViewModel : ObservableObject {
    private readonly ICustomerRepository _repository;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCustomerCommand))]
    private Customer? _selectedCustomer;

    public ObservableCollection<Customer> Customers { get; }

    public MainViewModel(ICustomerRepository repository) {
        _repository = repository;
        Customers = new ObservableCollection<Customer>(repository.GetAll());
    }

    public bool HasSelection => SelectedCustomer is not null;

    [RelayCommand]
    private void AddCustomer() {
        var nextId = Customers.Count == 0
            ? 1
            : Customers.Max(c => c.Id) + 1;

        var customer = new Customer {
            Id = nextId,
            Name = $"Neuer Kunde {nextId}",
            Email = $"kunde{nextId}@example.com"
        };

        Customers.Add(customer);
        _repository.Add(customer);
        SelectedCustomer = customer;
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void DeleteCustomer() {
        if (SelectedCustomer is null) {
            return;
        }

        _repository.Remove(SelectedCustomer.Id);
        Customers.Remove(SelectedCustomer);
        SelectedCustomer = null;
    }

    [RelayCommand]
    private void SaveCustomers() {
        _repository.Save();
    }
}
