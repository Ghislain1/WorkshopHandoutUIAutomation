# CustomerApp - WPF MVVM, DataGrid and UI Automation

A small WPF customer-management application built as a hands-on workshop for:

- WPF `DataGrid` with MVVM data binding
- Add, edit, delete and save commands
- Live search/filtering
- Model validation with `IDataErrorInfo`
- Unit testing with MSTest
- End-to-end UI testing with FlaUI and Windows UI Automation

The application uses an in-memory repository, so all data is reset when the app restarts.

## Features

- Editable customer grid with ID, name, e-mail and phone columns
- Add new customers
- Delete the selected customer after confirmation
- Save command with validation guard: invalid customer data blocks saving and shows a status message
- Live filtering across name, e-mail and phone
- Validation:
  - Name is required
  - E-mail must contain `@`
  - Phone must contain at least 6 characters
- Stable `AutomationProperties.AutomationId` values for UI tests
- Manual dependency composition in the application startup

## Tech Stack

| Area | Technology |
|---|---|
| Runtime | .NET 10 (`net10.0-windows`) |
| UI | WPF |
| MVVM | CommunityToolkit.Mvvm 8.4.2 |
| Unit tests | MSTest 4.x |
| UI automation | FlaUI 5.0.0 (UIA3) |
| CI | GitHub Actions, `windows-latest` |

## Repository Structure

```text
.
├── .github/
│   └── workflows/
│       └── ci.yml
├── src/
│   ├── WorkshopHandoutUIAutomation.sln
│   ├── CustomerApp/
│   │   ├── App.xaml
│   │   ├── App.xaml.cs              # Composition root
│   │   ├── Models/
│   │   │   └── Customer.cs
│   │   ├── Services/
│   │   │   ├── ICustomerRepository.cs
│   │   │   ├── InMemoryCustomerRepository.cs
│   │   │   ├── IDialogService.cs
│   │   │   └── MessageBoxDialogService.cs
│   │   ├── ViewModels/
│   │   │   ├── MainViewModel.cs
│   │   │   └── CustomerViewModel.cs
│   │   └── Views/
│   │       └── MainWindow.xaml
│   └── CustomerApp.Core/            # Placeholder for shared domain logic
├── tests/
│   ├── CustomerApp.Tests/           # Fast MSTest unit tests
│   └── CustomerApp.UiTests/         # FlaUI end-to-end tests
├── Workshop-UIA-E2E-TEIL 1.md
└── Workshop-UIA-E2E-TEIL 2.md
```

## Prerequisites

- Windows 10 or Windows 11
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022+ or another editor/IDE with WPF support
- An interactive desktop session for running the FlaUI UI tests

## Getting Started

Restore and build the solution:

```powershell
dotnet restore src\WorkshopHandoutUIAutomation.sln
dotnet build src\WorkshopHandoutUIAutomation.sln -c Debug
```

Run the application:

```powershell
dotnet run --project src\CustomerApp\CustomerApp.csproj
```

## Running Tests

### Unit tests

The unit tests cover view-model commands, validation and search behavior. They use a fake `IDialogService`, so no UI is required.

```powershell
dotnet test tests\CustomerApp.Tests\CustomerApp.Tests.csproj -c Debug
```

### UI tests

The UI tests launch the built WPF executable through FlaUI, control the DataGrid and verify the integration of view, bindings and commands. Build the application before running them:

```powershell
dotnet build src\CustomerApp\CustomerApp.csproj -c Debug
dotnet test tests\CustomerApp.UiTests\CustomerApp.UiTests.csproj -c Debug
```

UI automation requires an interactive Windows desktop session and cannot run in a headless environment.

## Architecture

### Model

`Customer` implements `INotifyPropertyChanged` for data binding and `IDataErrorInfo` for validation. The model exposes:

- `Id` (read-only after creation)
- `Name`
- `Email`
- `Phone`

### ViewModel

`MainViewModel` contains the presentation logic and exposes:

- `Customers` - all customers
- `FilteredCustomers` - filtered `ICollectionView` bound to the DataGrid
- `SelectedCustomer`
- `SearchText`
- `AddCustomerCommand`
- `DeleteCustomerCommand`
- `SaveCustomersCommand`

Deleting a customer uses `IDialogService.Confirm(...)` so the view model remains testable and does not depend directly on `MessageBox`.

### Services

- `ICustomerRepository` / `InMemoryCustomerRepository` - data access abstraction and in-memory implementation
- `IDialogService` / `MessageBoxDialogService` - dialog abstraction and WPF `MessageBox` implementation

### View and Composition Root

`App.xaml.cs` creates the view model and its dependencies, then assigns it as the `DataContext` of `MainWindow`:

```csharp
var viewModel = new MainViewModel(
    new InMemoryCustomerRepository(),
    new MessageBoxDialogService());

new MainWindow { DataContext = viewModel }.Show();
```

## Automation IDs

The following IDs are used by the FlaUI tests:

| Element | Automation ID |
|---|---|
| Main window | `MainWindow` |
| Add customer button | `AddCustomerButton` |
| Delete customer button | `DeleteCustomerButton` |
| Save button | `SaveCustomersButton` |
| Search box | `SearchBox` |
| Customer grid | `CustomerDataGrid` |

## Continuous Integration

The GitHub Actions workflow runs on `windows-latest` and performs:

1. Restore
2. Build
3. Unit tests
4. UI tests

See [`.github/workflows/ci.yml`](.github/workflows/ci.yml) for details.

## Workshop Material

The repository includes two accompanying workshop documents:

- [`Workshop-UIA-E2E-TEIL 1.md`](Workshop-UIA-E2E-TEIL%201.md)
- [`Workshop-UIA-E2E-TEIL 2.md`](Workshop-UIA-E2E-TEIL%202.md)

## Notes and Limitations

- Data is stored in memory only; `Save()` does not persist anything yet.
- The application is intentionally small and focuses on MVVM, testability and UI automation.
- UI tests depend on automation IDs and the WPF control structure, so keep those IDs stable when changing the view.
