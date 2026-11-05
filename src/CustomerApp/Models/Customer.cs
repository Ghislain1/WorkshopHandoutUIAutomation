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
    private Uri? _website;

    private CustomerStatus _status = CustomerStatus.Active;
    private DateTime _birthDate = DateTime.Today;
    private bool _isActive  ;
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

    public DateTime BirthDate {
        get => _birthDate;
        set => Set(ref _birthDate, value);
    }
    public CustomerStatus Status {
        get => _status;
        set => Set(ref _status, value);
    }
    public Uri? Website {
        get => _website;
        set => Set(ref _website, value);
    }
    public string Error => string.Empty;

    public string this[string columnName] => columnName switch {
        nameof(Name) when string.IsNullOrWhiteSpace(Name)
            => "Name ist erforderlich.",
        nameof(Email) when !Email.Contains('@')
            => "Ungültige E-Mail-Adresse.",
        nameof(Phone) when Phone.Length < 6
            => "Telefonnummer muss mindestens 6 Zeichen enthalten.",
        nameof(BirthDate) when BirthDate > DateTime.Today
           => "Geburtsdatum darf nicht in der Zukunft liegen.",
        _ => string.Empty
    };
    public bool IsActive {
        get => _isActive;
        set => Set(ref _isActive, value);
    }
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
