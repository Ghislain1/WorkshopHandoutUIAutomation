// // Description:Definition of ${File.FileName} class
// //
// // Copyright © 2025 - 2025, Alcon.All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using CustomerApp.Models;

namespace CustomerApp.Services;

public class InMemoryCustomerRepository : ICustomerRepository {
    private readonly List<Customer> _customers =
    [
        new() {
            Id = 1,
            Name = "Anna Weber",
            Email = "anna@example.com",
            Phone = "030123456",
            IsActive = true,
            Status = CustomerStatus.Active,
            Website = new Uri("https://anna-weber.example.com"),
            BirthDate = new DateTime(1985, 3, 14)
        },
        new() {
            Id = 2,
            Name = "Ben Schmidt",
            Email = "ben@example.com",
            Phone = "040654321",
            IsActive = false,
            Status = CustomerStatus.Inactive,
            Website = new Uri("https://ben-schmidt.example.com"),
            BirthDate = new DateTime(1990, 7, 22)
        },
        new() {
            Id = 3,
            Name = "Ghislain Zeus",
            Email = "ghislain@moreread.com",
            Phone = "041154321",
            IsActive = true,
            Status = CustomerStatus.Blocked,
            Website = new Uri("https://moreread.com"),
            BirthDate = new DateTime(1978, 11, 5)
        }
    ];

    public IReadOnlyList<Customer> GetAll() => _customers;

    public void Add(Customer customer) => _customers.Add(customer);

    public void Remove(int id) {
        var customer = _customers.FirstOrDefault(c => c.Id == id);
        if (customer is not null) {
            _customers.Remove(customer);
        }
    }

    public void Save() {
        // In einem echten Projekt: Persistenz, API oder Datenbank.
    }
}
