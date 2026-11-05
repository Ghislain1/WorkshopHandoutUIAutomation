// // Description:Definition of ${File.FileName} class
// //
// // Copyright © 2025 - 2025, Alcon.All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace CustomerApp.Models;

public class Customer : INotifyPropertyChanged, IDataErrorInfo {


    private string _name = string.Empty;
    private string _phone = string.Empty;
    private string _email = string.Empty;
    public int Id { get; init; }

    public string Name {
        get => _name;
        set => Set(ref _name, value);
    }

    public string Email {
        get => _email;
        set => Set(ref _email, value);
    }
    public string Phone {
        get => _phone;
        set => Set(ref _phone, value);
    }

    public string Error => string.Empty;

    public string this[string columnName] => columnName switch {
        nameof(Name) when string.IsNullOrWhiteSpace(Name)
            => "Name ist erforderlich.",
        nameof(Email) when !Email.Contains('@')
            => "Ungültige E-Mail-Adresse.",
        nameof(Phone) when Phone.Length < 6
            => "Telefonnummer muss mindestens 6 Zeichen enthalten.",
        _ => string.Empty
    };

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Set<T>(ref T field, T value,
        [CallerMemberName] string? propertyName = null) {
        if (EqualityComparer<T>.Default.Equals(field, value)) {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
