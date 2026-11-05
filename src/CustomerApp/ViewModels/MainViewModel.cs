// // Description:Definition of ${File.FileName} class
// //
// // Copyright © 2025 - 2025, Alcon.All rights reserved.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CustomerApp.Models;
using CustomerApp.Services;

namespace CustomerApp.ViewModels;

public partial class MainViewModel : ObservableObject {
    private readonly ICustomerRepository _repository;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCustomerCommand))]
    private Customer? _selectedCustomer;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ObservableCollection<Customer> Customers { get; }

    public ICollectionView FilteredCustomers { get; }

    public MainViewModel(ICustomerRepository repository, IDialogService dialogService) {
        _repository = repository;
        _dialogService = dialogService;
        Customers = new ObservableCollection<Customer>(repository.GetAll());
        FilteredCustomers = CollectionViewSource.GetDefaultView(Customers);
        FilteredCustomers.Filter = MatchesSearch;
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
            Email = $"kunde{nextId}@example.com",
            Phone = $"0151{nextId:D7}"
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

        if (!_dialogService.Confirm(
                "Kunde löschen",
                $"Soll '{SelectedCustomer.Name}' wirklich gelöscht werden?")) {
            return;
        }

        _repository.Remove(SelectedCustomer.Id);
        Customers.Remove(SelectedCustomer);
        SelectedCustomer = null;
    }
 
    [RelayCommand]
    private void ShowDetails(Customer customer) {
        StatusMessage = $"Details: {customer.Name}";
    }

    [RelayCommand]
    private void SaveCustomers() {
        var invalid = Customers
            .Where(c => !string.IsNullOrEmpty(c["Name"]) ||
                        !string.IsNullOrEmpty(c["Email"]))
            .ToList();

        LastAction = invalid.Count == 0
            ? "Gespeichert"
            : $"{invalid.Count} ungültige Kunden";
    }

    private static bool HasValidationError(Customer customer) =>
        !string.IsNullOrEmpty(customer[nameof(Customer.Name)])
        || !string.IsNullOrEmpty(customer[nameof(Customer.Email)])
        || !string.IsNullOrEmpty(customer[nameof(Customer.Phone)]);

    partial void OnSearchTextChanged(string value) {
        FilteredCustomers.Refresh();
    }
    [ObservableProperty]
    private string? _lastAction;

    private bool MatchesSearch(object item) {
        if (item is not Customer customer) {
            return false;
        }

        var search = SearchText.Trim();
        if (search.Length == 0) {
            return true;
        }

        return customer.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
            || customer.Email.Contains(search, StringComparison.OrdinalIgnoreCase)
            || customer.Phone.Contains(search, StringComparison.OrdinalIgnoreCase);
    }
}
