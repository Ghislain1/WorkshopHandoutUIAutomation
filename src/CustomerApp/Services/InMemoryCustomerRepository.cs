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
        new() { Id = 1, Name = "Anna Weber", Email = "anna@example.com", Phone = "030123456" },
        new() { Id = 2, Name = "Ben Schmidt", Email = "ben@example.com", Phone = "040654321" },
        new() { Id = 3, Name = "Ghislain Zeus", Email = "ghislain@moreread.com", Phone = "041154321" }
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
