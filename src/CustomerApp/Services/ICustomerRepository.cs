// // Description:Definition of ${File.FileName} class
// //
// // Copyright © 2025 - 2025, Alcon.All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using CustomerApp.Models;

namespace CustomerApp.Services;

public interface ICustomerRepository {
    IReadOnlyList<Customer> GetAll();
    void Add(Customer customer);
    void Remove(int id);
    void Save();
}
