# Hands-on-Workshop: WPF DataGrid, MVVM, FlaUI, MSTest & E2E-Tests

Dieses Workshop-Paket richtet sich an Senior-WPF-Developer und baut in etwa 6–8 Stunden eine testbare Desktop-Anwendung mit editierbarem `DataGrid`, sauberem MVVM, Unit-Tests, UI-Automation und End-to-End-Tests auf. WPFs `DataGrid` bindet Zeilen an Objekte und aktualisiert die UI automatisch, wenn die Quelle `INotifyCollectionChanged` implementiert; die Zeilenobjekte sollten `INotifyPropertyChanged` implementieren. [learn.microsoft](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/controls/datagrid)

## Lernziele

Nach dem Workshop können die Teilnehmer:

- Ein WPF-`DataGrid` vollständig MVVM-konform an `ObservableCollection<T>` binden.
- CRUD-Aktionen, Validierung, Auswahl und Commands ohne Code-Behind-Logik umsetzen.
- ViewModels mit MSTest unit-testen.
- Eine echte WPF-Anwendung mit FlaUI über Windows UI Automation steuern.
- Robuste, wartbare E2E-Tests mit Page Objects, explicit waits und stabilen AutomationIds schreiben.
- UI-Tests in CI unter Windows ausführen.

MVVM trennt View, ViewModel und Model; die View kommuniziert über Data Binding, Commands und Change Notification mit dem ViewModel. [prismlibrary.github](https://prismlibrary.github.io/docs/wpf/legacy/Implementing-MVVM.html)

## Agenda

| Zeit | Modul | Ergebnis |
|---|---|---|
| 30 min | Setup & Architektur | Lösung, Projekte, NuGet-Pakete |
| 60 min | DataGrid + MVVM | Editierbare Kundentabelle |
| 45 min | Commands & Validierung | Hinzufügen, Löschen, Speichern |
| 45 min | MSTest Unit-Tests | ViewModel-Logik getestet |
| 60 min | FlaUI-Grundlagen | App starten, Fenster/Controls finden |
| 60 min | DataGrid-Automation | Zeilen lesen, editieren, hinzufügen |
| 60 min | E2E-Test-Suite | Page Objects, Happy Path, Fehlerfälle |
| 30 min | Stabilität & CI | Selektoren, Waits, Testausführung |

## Projektstruktur

```text
WpfWorkshop.sln
├─ src
│  ├─ CustomerApp                 # WPF-Anwendung
│  │  ├─ App.xaml
│  │  ├─ Views
│  │  │  └─ MainWindow.xaml
│  │  ├─ ViewModels
│  │  │  ├─ MainViewModel.cs
│  │  │  └─ CustomerViewModel.cs
│  │  ├─ Models
│  │  │  └─ Customer.cs
│  │  └─ Services
│  │     └─ ICustomerRepository.cs
│  └─ CustomerApp.Core            # Optional: Domain/Services
└─ tests
   ├─ CustomerApp.Tests           # MSTest Unit-Tests
   └─ CustomerApp.UiTests         # FlaUI + MSTest E2E-Tests
```

Empfohlene NuGet-Pakete:

```bash
dotnet add src/CustomerApp package CommunityToolkit.Mvvm
dotnet add tests/CustomerApp.Tests package MSTest
dotnet add tests/CustomerApp.UiTests package MSTest
dotnet add tests/CustomerApp.UiTests package FlaUI.UIA3
dotnet add tests/CustomerApp.UiTests package FlaUI.Core
```

FlaUI ist eine .NET-UI-Automation-Bibliothek; die aktuelle Hauptversion ist FlaUI 5.0.0.  Microsoft dokumentiert außerdem ein MSTest-UI-Automation-Paket, das WPF unterstützt und sich mit FlaUI kombinieren lässt, wenn weitergehende Elementinteraktion benötigt wird. [github](https://github.com/FlaUI/FlaUI/releases)

## Modul 1: Domain-Modell

Erstellen Sie `Customer.cs`:

```csharp
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CustomerApp.Models;

public class Customer : INotifyPropertyChanged, IDataErrorInfo
{
    private string _name = string.Empty;
    private string _email = string.Empty;

    public int Id { get; init; }

    public string Name
    {
        get => _name;
        set => Set(ref _name, value);
    }

    public string Email
    {
        get => _email;
        set => Set(ref _email, value);
    }

    public string Error => string.Empty;

    public string this[string columnName] => columnName switch
    {
        nameof(Name) when string.IsNullOrWhiteSpace(Name)
            => "Name ist erforderlich.",
        nameof(Email) when !Email.Contains('@')
            => "Ungültige E-Mail-Adresse.",
        _ => string.Empty
    };

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Set<T>(ref T field, T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

**Übung:** Ergänzen Sie `Phone` und eine Regel, dass die Telefonnummer mindestens 6 Zeichen enthält.

## Modul 2: Repository-Schnittstelle

Für Unit-Tests sollte die Datenquelle austauschbar sein:

```csharp
using CustomerApp.Models;

namespace CustomerApp.Services;

public interface ICustomerRepository
{
    IReadOnlyList<Customer> GetAll();
    void Add(Customer customer);
    void Remove(int id);
    void Save();
}
```

Eine In-Memory-Implementierung:

```csharp
using CustomerApp.Models;

namespace CustomerApp.Services;

public class InMemoryCustomerRepository : ICustomerRepository
{
    private readonly List<Customer> _customers =
    [
        new() { Id = 1, Name = "Anna Weber", Email = "anna@example.com" },
        new() { Id = 2, Name = "Ben Schmidt", Email = "ben@example.com" }
    ];

    public IReadOnlyList<Customer> GetAll() => _customers;

    public void Add(Customer customer) => _customers.Add(customer);

    public void Remove(int id)
    {
        var customer = _customers.FirstOrDefault(c => c.Id == id);
        if (customer is not null)
        {
            _customers.Remove(customer);
        }
    }

    public void Save()
    {
        // In einem echten Projekt: Persistenz, API oder Datenbank.
    }
}
```

## Modul 3: MVVM-ViewModel

Das ViewModel kapselt Präsentationslogik und Zustand; die View bindet daran.  `ObservableCollection<T>` sorgt dafür, dass Hinzufügen und Entfernen von Zeilen automatisch im `DataGrid` erscheinen. [learn.microsoft](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/controls/datagrid)

```csharp
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CustomerApp.Models;
using CustomerApp.Services;

namespace CustomerApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ICustomerRepository _repository;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCustomerCommand))]
    private Customer? _selectedCustomer;

    public ObservableCollection<Customer> Customers { get; }

    public MainViewModel(ICustomerRepository repository)
    {
        _repository = repository;
        Customers = new ObservableCollection<Customer>(repository.GetAll());
    }

    public bool HasSelection => SelectedCustomer is not null;

    [RelayCommand]
    private void AddCustomer()
    {
        var nextId = Customers.Count == 0
            ? 1
            : Customers.Max(c => c.Id) + 1;

        var customer = new Customer
        {
            Id = nextId,
            Name = $"Neuer Kunde {nextId}",
            Email = $"kunde{nextId}@example.com"
        };

        Customers.Add(customer);
        _repository.Add(customer);
        SelectedCustomer = customer;
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void DeleteCustomer()
    {
        if (SelectedCustomer is null)
        {
            return;
        }

        _repository.Remove(SelectedCustomer.Id);
        Customers.Remove(SelectedCustomer);
        SelectedCustomer = null;
    }

    [RelayCommand]
    private void SaveCustomers()
    {
        _repository.Save();
    }
}
```

**Senior-Diskussion:** Warum sollte `DeleteCustomerCommand` nicht direkt `MessageBox.Show()` aufrufen? Wie würde man einen Bestätigungsdialog testbar machen?

## Modul 4: DataGrid-View

Wichtig für FlaUI: Vergeben Sie von Anfang an stabile `AutomationProperties.AutomationId`-Werte. Verlassen Sie sich nicht auf visuelle Positionen oder generierte Control-Namen.

```xml
<Window x:Class="CustomerApp.Views.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="clr-namespace:CustomerApp.ViewModels"
        Title="Kundenverwaltung"
        Height="600"
        Width="900"
        AutomationProperties.AutomationId="MainWindow">

    <Window.DataContext>
        <vm:MainViewModel />
    </Window.DataContext>

    <DockPanel Margin="16">

        <StackPanel DockPanel.Dock="Top"
                    Orientation="Horizontal"
                    Margin="0,0,0,16">

            <Button Content="Kunde hinzufügen"
                    AutomationProperties.AutomationId="AddCustomerButton"
                    Command="{Binding AddCustomerCommand}"
                    Margin="0,0,8,0" />

            <Button Content="Kunde löschen"
                    AutomationProperties.AutomationId="DeleteCustomerButton"
                    Command="{Binding DeleteCustomerCommand}"
                    Margin="0,0,8,0" />

            <Button Content="Speichern"
                    AutomationProperties.AutomationId="SaveCustomersButton"
                    Command="{Binding SaveCustomersCommand}" />
        </StackPanel>

        <DataGrid AutomationProperties.AutomationId="CustomerDataGrid"
                  ItemsSource="{Binding Customers}"
                  SelectedItem="{Binding SelectedCustomer,
                                 Mode=TwoWay,
                                 UpdateSourceTrigger=PropertyChanged}"
                  AutoGenerateColumns="False"
                  CanUserAddRows="False"
                  CanUserDeleteRows="False"
                  SelectionMode="Single">

            <DataGrid.Columns>
                <DataGridTextColumn Header="ID"
                                    Binding="{Binding Id}"
                                    IsReadOnly="True"
                                    Width="60" />

                <DataGridTextColumn Header="Name"
                                    Binding="{Binding Name,
                                              UpdateSourceTrigger=PropertyChanged}"
                                    Width="*" />

                <DataGridTextColumn Header="E-Mail"
                                    Binding="{Binding Email,
                                              UpdateSourceTrigger=PropertyChanged}"
                                    Width="*" />
            </DataGrid.Columns>
        </DataGrid>
    </DockPanel>
</Window>
```

`ItemsSource` bindet das Grid an eine `IEnumerable`-Implementierung; jede Spalte bindet an eine Eigenschaft des Zeilenobjekts. [learn.microsoft](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/controls/datagrid)

**Übung:** Binden Sie `DeleteCustomerButton.IsEnabled` nicht manuell, sondern prüfen Sie, ob `CanExecute` über `HasSelection` korrekt funktioniert.

## Modul 5: MSTest-Unit-Tests

Installieren bzw. verwenden Sie MSTest im Testprojekt. Testen Sie hier die ViewModel-Logik, nicht die WPF-Visualisierung.

```csharp
using CustomerApp.Models;
using CustomerApp.Services;
using CustomerApp.ViewModels;

namespace CustomerApp.Tests;

[TestClass]
public sealed class MainViewModelTests
{
    private static MainViewModel CreateSut()
    {
        return new MainViewModel(new InMemoryCustomerRepository());
    }

    [TestMethod]
    public void Constructor_LoadsCustomersFromRepository()
    {
        var sut = CreateSut();

        Assert.HasCount(2, sut.Customers);
        Assert.AreEqual("Anna Weber", sut.Customers[0].Name);
    }

    [TestMethod]
    public void AddCustomer_AddsRowAndSelectsIt()
    {
        var sut = CreateSut();

        sut.AddCustomerCommand.Execute(null);

        Assert.HasCount(3, sut.Customers);
        Assert.IsNotNull(sut.SelectedCustomer);
        Assert.AreEqual(3, sut.SelectedCustomer!.Id);
    }

    [TestMethod]
    public void DeleteCustomer_RemovesSelectedCustomer()
    {
        var sut = CreateSut();
        sut.SelectedCustomer = sut.Customers[0];

        sut.DeleteCustomerCommand.Execute(null);

        Assert.HasCount(1, sut.Customers);
        Assert.IsNull(sut.SelectedCustomer);
    }

    [TestMethod]
    public void DeleteCustomerCommand_CannotExecuteWithoutSelection()
    {
        var sut = CreateSut();

        Assert.IsFalse(sut.DeleteCustomerCommand.CanExecute(null));
    }

    [TestMethod]
    public void Customer_Validation_RejectsMissingName()
    {
        var customer = new Customer { Name = "", Email = "test@example.com" };

        Assert.AreEqual("Name ist erforderlich.", customer["Name"]);
    }
}
```

## Modul 6: FlaUI-Grundlagen

FlaUI automatisiert Windows-Apps über UI Automation. Für WPF ist in der Regel UIA3 die passende Variante. [github](https://github.com/FlaUI/FlaUI/releases)

Erstellen Sie eine Basis-Klasse für E2E-Tests:

```csharp
using FlaUI.Core;
using FlaUI.Core.Tools;
using FlaUI.UIA3;

namespace CustomerApp.UiTests.Infrastructure;

public abstract class WpfAppTestBase : IDisposable
{
    protected Application App { get; private set; } = null!;
    protected UIA3Automation Automation { get; private set; } = null!;

    [TestInitialize]
    public void StartApplication()
    {
        Automation = new UIA3Automation();

        var exePath = Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "src", "CustomerApp", "bin", "Debug",
            "net8.0-windows", "CustomerApp.exe");

        App = Application.Launch(exePath);

        Retry.WhileNull(
            () => App.GetMainWindow(Automation),
            timeout: TimeSpan.FromSeconds(10));
    }

    [TestCleanup]
    public void StopApplication()
    {
        App?.Close();
        App?.Dispose();
        Automation?.Dispose();
    }

    public void Dispose()
    {
        StopApplication();
    }
}
```

**Best Practice:** Bauen Sie den Pfad zur EXE nicht hart, sondern über eine Konfigurationsvariable, `AssemblyLocation` oder ein Build-Target. Für CI sollte die App vor den UI-Tests gebaut werden.

## Modul 7: Page Object für das Hauptfenster

Page Objects kapseln UI-Zugriff und machen Tests lesbar und wartbar.

```csharp
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Tools;

namespace CustomerApp.UiTests.PageObjects;

public sealed class MainWindowPage
{
    private readonly Window _window;

    public MainWindowPage(Window window)
    {
        _window = window;
    }

    private Button AddButton => _window
        .FindFirstDescendant(cf => cf.ByAutomationId("AddCustomerButton"))
        .AsButton();

    private Button DeleteButton => _window
        .FindFirstDescendant(cf => cf.ByAutomationId("DeleteCustomerButton"))
        .AsButton();

    private DataGrid CustomerGrid => _window
        .FindFirstDescendant(cf => cf.ByAutomationId("CustomerDataGrid"))
        .AsGrid();

    public int RowCount => CustomerGrid.Rows.Length;

    public void AddCustomer()
    {
        AddButton.Invoke();
    }

    public void DeleteSelectedCustomer()
    {
        DeleteButton.Invoke();
    }

    public void SelectRow(int rowIndex)
    {
        var row = CustomerGrid.Rows[rowIndex];
        row.Select();
    }

    public string GetCellValue(int rowIndex, string columnHeader)
    {
        var row = CustomerGrid.Rows[rowIndex];

        var cell = Retry.WhileNull(
            () => row.FindFirstDescendant(
                cf => cf.ByName(columnHeader)),
            timeout: TimeSpan.FromSeconds(2)).Result;

        return cell.AsTextBox().Text;
    }

    public void EditCell(int rowIndex, string columnHeader, string newValue)
    {
        var row = CustomerGrid.Rows[rowIndex];

        var cell = row.FindFirstDescendant(
            cf => cf.ByName(columnHeader));

        var textBox = cell.AsTextBox();
        textBox.Text = newValue;
    }

    public void WaitUntilRowCountIs(int expectedCount)
    {
        Retry.WhileFalse(
            () => RowCount == expectedCount,
            timeout: TimeSpan.FromSeconds(5));
    }
}
```

**Wichtiger Hinweis:** WPF-`DataGrid`-Zellen sind zur Laufzeit virtualisiert und können abhängig von Spalten-Templates, Bearbeitungsmodus und Scroll-Position unterschiedliche Automation-Strukturen erzeugen. Prüfen Sie mit **FlaUInspect**, welche `AutomationId`, `Name`, `ControlType` und Patterns Ihre konkreten Zellen tatsächlich bereitstellen.

## Modul 8: E2E-Tests mit MSTest

```csharp
using CustomerApp.UiTests.Infrastructure;
using CustomerApp.UiTests.PageObjects;
using FlaUI.Core.Tools;

namespace CustomerApp.UiTests;

[TestClass]
public sealed class CustomerManagementE2eTests : WpfAppTestBase
{
    private MainWindowPage CreatePage()
    {
        var mainWindow = Retry.WhileNull(
            () => App.GetMainWindow(Automation),
            timeout: TimeSpan.FromSeconds(10)).Result;

        return new MainWindowPage(mainWindow);
    }

    [TestMethod]
    public void AddCustomer_ShowsNewRow()
    {
        var page = CreatePage();
        var initialCount = page.RowCount;

        page.AddCustomer();

        page.WaitUntilRowCountIs(initialCount + 1);

        Assert.AreEqual(initialCount + 1, page.RowCount);
    }

    [TestMethod]
    public void EditCustomerName_UpdatesGrid()
    {
        var page = CreatePage();

        page.SelectRow(0);
        page.EditCell(0, "Name", "Aktualisierter Kunde");

        Assert.AreEqual(
            "Aktualisierter Kunde",
            page.GetCellValue(0, "Name"));
    }

    [TestMethod]
    public void DeleteSelectedCustomer_RemovesRow()
    {
        var page = CreatePage();
        var initialCount = page.RowCount;

        page.SelectRow(0);
        page.DeleteSelectedCustomer();

        page.WaitUntilRowCountIs(initialCount - 1);

        Assert.AreEqual(initialCount - 1, page.RowCount);
    }

    [TestMethod]
    public void FullWorkflow_AddEditAndDeleteCustomer()
    {
        var page = CreatePage();
        var initialCount = page.RowCount;

        page.AddCustomer();
        page.WaitUntilRowCountIs(initialCount + 1);

        page.SelectRow(initialCount);
        page.EditCell(initialCount, "Name", "E2E Kunde");

        Assert.AreEqual(
            "E2E Kunde",
            page.GetCellValue(initialCount, "Name"));

        page.DeleteSelectedCustomer();
        page.WaitUntilRowCountIs(initialCount);
    }
}
```

## Teststrategie

| Ebene | Ziel | Beispiele |
|---|---|---|
| Unit-Test | ViewModel-, Command- und Validierungslogik | Kunde hinzufügen, Löschen nur mit Auswahl |
| Komponententest | Repository / Service isoliert | `InMemoryCustomerRepository` |
| UI-Automation | Echte WPF-Controls und Bindings | DataGrid-Zeilen, Buttons, Editieren |
| E2E-Test | User Journey über mehrere Interaktionen | Hinzufügen → Bearbeiten → Löschen |

UI-Tests sollten nicht jede ViewModel-Regel duplizieren. Testen Sie dort vor allem Integration: Binding, Commands, Rendering, Selektion, Tastatur-/Maus-Interaktion und anwendungsspezifische User Journeys.

## Stabilität: Die wichtigsten Regeln

- Vergeben Sie **eindeutige `AutomationProperties.AutomationId`** für alle interaktiven Elemente.
- Nutzen Sie `Retry.WhileNull` / `Retry.WhileFalse` statt `Thread.Sleep()`.
- Halten Sie Tests unabhängig: Jeder Test startet mit definiertem Ausgangszustand.
- Kapseln Sie Selektoren ausschließlich in Page Objects.
- Testen Sie Verhalten, nicht Implementierungsdetails wie interne Grid-Strukturen.
- Führen Sie UI-Tests nur auf Windows-Agents aus; UIA benötigt eine interaktive Desktop-Sitzung. Microsoft weist ausdrücklich darauf hin, dass das MSTest-UI-Automation-Paket WPF in einer interaktiven Windows-Sitzung unterstützt. [learn.microsoft](https://learn.microsoft.com/de-de/dotnet/core/testing/unit-testing-mstest-ui-automation)
- Deaktivieren Sie Animationen und visuelle Effekte in Test-Umgebungen, wenn möglich.

## CI-Beispiel: GitHub Actions

```yaml
name: WPF Tests

on:
  push:
  pull_request:

jobs:
  test:
    runs-on: windows-latest

    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Restore
        run: dotnet restore WpfWorkshop.sln

      - name: Build
        run: dotnet build WpfWorkshop.sln -c Debug --no-restore

      - name: Run unit tests
        run: dotnet test tests/CustomerApp.Tests -c Debug --no-build

      - name: Run UI tests
        run: dotnet test tests/CustomerApp.UiTests -c Debug --no-build
```

## Abschlussübung

Bauen Sie folgende Features vollständig MVVM- und testgetrieben:

1. Ein Suchfeld, das die `DataGrid`-Zeilen filtert.
2. Einen „Ungültige Daten speichern“-Fall: Speichern darf bei Validierungsfehlern nicht erfolgen.
3. Einen Bestätigungsdialog vor dem Löschen, ersetzt durch ein testbares `IDialogService`.
4. Einen E2E-Test: Suche → Kunde auswählen → E-Mail ändern → speichern → Anwendung neu starten → Änderung sichtbar.
5. Einen Test für leere DataGrid-Ergebnisse nach Filterung.

Der Kern des Workshops: **ViewModel-Logik gehört in schnelle MSTests; FlaUI-E2E-Tests beweisen, dass View, Bindings, Commands und DataGrid zusammen tatsächlich funktionieren.**