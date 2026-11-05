Ja — das DataGrid sollte alle relevanten WPF-Spaltentypen enthalten und jeder Typ sollte eigene Unit- und FlaUI-E2E-Tests erhalten. WPF stellt standardmäßig `DataGridTextColumn`, `DataGridCheckBoxColumn`, `DataGridComboBoxColumn`, `DataGridHyperlinkColumn` und `DataGridTemplateColumn` bereit. [learn.microsoft](https://learn.microsoft.com/sk-sk/dotnet/desktop/wpf/controls/datagrid?view=netframeworkdesktop-4.8)

## Spaltentypen und Testziele

| Spaltentyp | Daten-/UI-Typ | Zu testendes Verhalten | FlaUI-Zugriff |
|---|---|---|---|
| `DataGridTextColumn` | `string` | Anzeige, Editieren, Validierung | `TextBlock` / `TextBox` |
| `DataGridCheckBoxColumn` | `bool` | Toggle, TwoWay-Binding, `IsThreeState` | `CheckBox` |
| `DataGridComboBoxColumn` | Enum / Lookup | Auswahl, Binding, Command-Reaktion | `ComboBox` |
| `DataGridHyperlinkColumn` | `Uri` | Anzeige, Klick/Navigation | `Hyperlink` |
| `DataGridTemplateColumn` | Beliebig | Custom Control, Buttons, DatePicker | Controls aus Template |
| Optional: DatePicker | `DateTime` | Datum wählen, Validierung | `DatePicker` im Template |

`DataGridTextColumn` zeigt im Nicht-Edit-Modus ein `TextBlock` und im Edit-Modus eine `TextBox`; `DataGridComboBoxColumn` erzeugt im Edit-Modus eine `ComboBox`.  `DataGridTemplateColumn` verwendet eigene `CellTemplate`- und `CellEditingTemplate`-Definitionen. [learn.microsoft](https://learn.microsoft.com/en-us/dotnet/api/system.windows.controls.datagrid?view=netframework-4.5)

## Erweitertes Model

```csharp
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CustomerApp.Models;

public enum CustomerStatus
{
    Active,
    Inactive,
    Blocked
}

public class Customer : INotifyPropertyChanged, IDataErrorInfo
{
    private string _name = string.Empty;
    private string _email = string.Empty;
    private bool _isActive;
    private CustomerStatus _status = CustomerStatus.Active;
    private Uri? _website;
    private DateTime _birthDate = DateTime.Today;

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

    public bool IsActive
    {
        get => _isActive;
        set => Set(ref _isActive, value);
    }

    public CustomerStatus Status
    {
        get => _status;
        set => Set(ref _status, value);
    }

    public Uri? Website
    {
        get => _website;
        set => Set(ref _website, value);
    }

    public DateTime BirthDate
    {
        get => _birthDate;
        set => Set(ref _birthDate, value);
    }

    public string Error => string.Empty;

    public string this[string columnName] => columnName switch
    {
        nameof(Name) when string.IsNullOrWhiteSpace(Name)
            => "Name ist erforderlich.",
        nameof(Email) when !Email.Contains('@')
            => "Ungültige E-Mail-Adresse.",
        nameof(BirthDate) when BirthDate > DateTime.Today
            => "Geburtsdatum darf nicht in der Zukunft liegen.",
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

## DataGrid mit allen Spaltentypen

```xml
<DataGrid AutomationProperties.AutomationId="CustomerDataGrid"
          ItemsSource="{Binding Customers}"
          SelectedItem="{Binding SelectedCustomer,
                         Mode=TwoWay,
                         UpdateSourceTrigger=PropertyChanged}"
          AutoGenerateColumns="False"
          CanUserAddRows="False"
          CanUserDeleteRows="False">

    <DataGrid.Columns>

        <!-- Text -->
        <DataGridTextColumn Header="Name"
                            AutomationProperties.AutomationId="NameColumn"
                            Binding="{Binding Name,
                                      UpdateSourceTrigger=PropertyChanged}"
                            Width="150" />

        <!-- Text / E-Mail -->
        <DataGridTextColumn Header="E-Mail"
                            AutomationProperties.AutomationId="EmailColumn"
                            Binding="{Binding Email,
                                      UpdateSourceTrigger=PropertyChanged}"
                            Width="200" />

        <!-- Boolean -->
        <DataGridCheckBoxColumn Header="Aktiv"
                                AutomationProperties.AutomationId="IsActiveColumn"
                                Binding="{Binding IsActive,
                                          UpdateSourceTrigger=PropertyChanged}"
                                IsThreeState="False" />

        <!-- Enum -->
        <DataGridComboBoxColumn Header="Status"
                                AutomationProperties.AutomationId="StatusColumn"
                                SelectedItemBinding="{Binding Status}"
                                ItemsSource="{Binding Source={StaticResource CustomerStatuses}}" />

        <!-- Uri -->
        <DataGridHyperlinkColumn Header="Website"
                                 AutomationProperties.AutomationId="WebsiteColumn"
                                 Binding="{Binding Website}"
                                 ContentBinding="{Binding Website}"
                                 Width="200" />

        <!-- Custom: DatePicker -->
        <DataGridTemplateColumn Header="Geburtsdatum"
                                AutomationProperties.AutomationId="BirthDateColumn">
            <DataGridTemplateColumn.CellTemplate>
                <DataTemplate>
                    <TextBlock Text="{Binding BirthDate,
                                      StringFormat=d}"
                               VerticalAlignment="Center"
                               Margin="4,0" />
                </DataTemplate>
            </DataGridTemplateColumn.CellTemplate>

            <DataGridTemplateColumn.CellEditingTemplate>
                <DataTemplate>
                    <DatePicker AutomationProperties.AutomationId="BirthDatePicker"
                                SelectedDate="{Binding BirthDate,
                                               UpdateSourceTrigger=PropertyChanged}" />
                </DataTemplate>
            </DataGridTemplateColumn.CellEditingTemplate>
        </DataGridTemplateColumn>

        <!-- Custom: Aktion -->
        <DataGridTemplateColumn Header="Aktion"
                                AutomationProperties.AutomationId="ActionsColumn">
            <DataGridTemplateColumn.CellTemplate>
                <DataTemplate>
                    <Button Content="Details"
                            AutomationProperties.AutomationId="CustomerDetailsButton"
                            Command="{Binding DataContext.ShowDetailsCommand,
                                      RelativeSource={RelativeSource
                                          AncestorType=Window}}"
                            CommandParameter="{Binding}" />
                </DataTemplate>
            </DataGridTemplateColumn.CellTemplate>
        </DataGridTemplateColumn>

    </DataGrid.Columns>
</DataGrid>
```

Für die Enum-Quelle im Window-Resource-Bereich:

```xml
<Window.Resources>
    <ObjectDataProvider x:Key="CustomerStatuses"
                        MethodName="GetValues"
                        ObjectType="{x:Type sys:Enum}">
        <ObjectDataProvider.MethodParameters>
            <x:Type TypeName="models:CustomerStatus" />
        </ObjectDataProvider.MethodParameters>
    </ObjectDataProvider>
</Window.Resources>
```

Dazu die Namespaces:

```xml
xmlns:sys="clr-namespace:System;assembly=mscorlib"
xmlns:models="clr-namespace:CustomerApp.Models"
```

## ViewModel-Erweiterung

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
    private Customer? _selectedCustomer;

    [ObservableProperty]
    private string? _lastAction;

    public ObservableCollection<Customer> Customers { get; }

    public MainViewModel(ICustomerRepository repository)
    {
        _repository = repository;
        Customers = new ObservableCollection<Customer>(repository.GetAll());
    }

    [RelayCommand]
    private void ShowDetails(Customer customer)
    {
        LastAction = $"Details: {customer.Name}";
    }

    [RelayCommand]
    private void SaveCustomers()
    {
        var invalid = Customers
            .Where(c => !string.IsNullOrEmpty(c["Name"]) ||
                        !string.IsNullOrEmpty(c["Email"]))
            .ToList();

        LastAction = invalid.Count == 0
            ? "Gespeichert"
            : $"{invalid.Count} ungültige Kunden";
    }
}
```

## MSTest-Unit-Tests je Spaltentyp

```csharp
using CustomerApp.Models;
using CustomerApp.Services;
using CustomerApp.ViewModels;

namespace CustomerApp.Tests;

[TestClass]
public sealed class CustomerColumnTests
{
    private static Customer CreateCustomer() => new()
    {
        Id = 1,
        Name = "Anna Weber",
        Email = "anna@example.com",
        IsActive = true,
        Status = CustomerStatus.Active,
        Website = new Uri("https://example.com"),
        BirthDate = new DateTime(1990, 1, 1)
    };

    [TestMethod]
    public void TextColumn_Name_CanBeChanged()
    {
        var customer = CreateCustomer();

        customer.Name = "Neuer Name";

        Assert.AreEqual("Neuer Name", customer.Name);
    }

    [TestMethod]
    public void TextColumn_Email_RaisesPropertyChanged()
    {
        var customer = CreateCustomer();
        var changed = false;

        customer.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Customer.Email))
            {
                changed = true;
            }
        };

        customer.Email = "new@example.com";

        Assert.IsTrue(changed);
    }

    [TestMethod]
    public void CheckBoxColumn_IsActive_TogglesValue()
    {
        var customer = CreateCustomer();

        customer.IsActive = false;

        Assert.IsFalse(customer.IsActive);
    }

    [TestMethod]
    public void ComboBoxColumn_Status_AcceptsAllEnumValues()
    {
        var customer = CreateCustomer();

        foreach (CustomerStatus status in Enum.GetValues<CustomerStatus>())
        {
            customer.Status = status;
            Assert.AreEqual(status, customer.Status);
        }
    }

    [TestMethod]
    public void HyperlinkColumn_Website_StoresUri()
    {
        var customer = CreateCustomer();

        customer.Website = new Uri("https://flaui.com");

        Assert.AreEqual("https://flaui.com/", customer.Website!.ToString());
    }

    [TestMethod]
    public void TemplateColumn_DatePicker_RejectsFutureDate()
    {
        var customer = CreateCustomer
        {
            BirthDate = DateTime.Today.AddDays(1)
        };

        Assert.AreEqual(
            "Geburtsdatum darf nicht in der Zukunft liegen.",
            customer[nameof(Customer.BirthDate)]);
    }

    [TestMethod]
    public void ShowDetailsCommand_SetsLastAction()
    {
        var sut = new MainViewModel(new InMemoryCustomerRepository());
        var customer = CreateCustomer();

        sut.ShowDetailsCommand.Execute(customer);

        Assert.AreEqual("Details: Anna Weber", sut.LastAction);
    }
}
```

## FlaUI Page Object für alle Spaltentypen

Wichtig: Eine `DataGridTextColumn` ist im Anzeigemodus meist ein `TextBlock` und erst nach dem Wechsel in den Bearbeitungsmodus eine `TextBox`; FlaUI bietet Konvertierungen zu `CheckBox`, `ComboBox` und Grid-Zellen. [learn.microsoft](http://www.learn.microsoft.com/en-us/dotnet/api/system.windows.controls.datagridtextcolumn?view=windowsdesktop-3.1)

```csharp
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Tools;

namespace CustomerApp.UiTests.PageObjects;

public sealed class CustomerGridPage
{
    private readonly DataGrid _grid;

    public CustomerGridPage(Window window)
    {
        _grid = window
            .FindFirstDescendant(cf => cf.ByAutomationId("CustomerDataGrid"))
            .AsGrid();
    }

    public int RowCount => _grid.Rows.Length;

    public DataGridRow GetRow(int index) => _grid.Rows[index];

    private AutomationElement GetCell(int rowIndex, string columnAutomationId)
    {
        var row = GetRow(rowIndex);

        return Retry.WhileNull(
            () => row.FindFirstDescendant(
                cf => cf.ByAutomationId(columnAutomationId)),
            timeout: TimeSpan.FromSeconds(3)).Result;
    }

    // DataGridTextColumn
    public string GetText(int rowIndex) =>
        GetCell(rowIndex, "NameCell").AsTextBox().Text;

    public void SetText(int rowIndex, string value)
    {
        var cell = GetCell(rowIndex, "NameCell");
        cell.DoubleClick();
        var textBox = Retry.WhileNull(
            () => cell.FindFirstDescendant(cf => cf.ByControlType(ControlType.Edit)),
            timeout: TimeSpan.FromSeconds(2)).Result.AsTextBox();

        textBox.Text = value;
        Keyboard.PressVirtualKeyCode(FlaUI.Core.Input.VirtualKeyShort.ENTER);
    }

    // DataGridCheckBoxColumn
    public bool GetIsActive(int rowIndex) =>
        GetCell(rowIndex, "IsActiveCell").AsCheckBox().IsToggled;

    public void ToggleIsActive(int rowIndex)
    {
        GetCell(rowIndex, "IsActiveCell").AsCheckBox().Toggle();
    }

    // DataGridComboBoxColumn
    public string GetStatus(int rowIndex) =>
        GetCell(rowIndex, "StatusCell").AsComboBox().Value;

    public void SetStatus(int rowIndex, string value)
    {
        var comboBox = GetCell(rowIndex, "StatusCell").AsComboBox();
        comboBox.Select(value);
    }

    // DataGridHyperlinkColumn
    public string GetWebsite(int rowIndex) =>
        GetCell(rowIndex, "WebsiteCell").Name;

    // DataGridTemplateColumn: DatePicker
    public void SetBirthDate(int rowIndex, DateTime date)
    {
        var cell = GetCell(rowIndex, "BirthDateCell");
        cell.DoubleClick();

        var datePicker = Retry.WhileNull(
            () => cell.FindFirstDescendant(
                cf => cf.ByAutomationId("BirthDatePicker")),
            timeout: TimeSpan.FromSeconds(2)).Result;

        datePicker.Patterns.Value.Pattern.SetValue(date.ToString("d"));
    }

    // DataGridTemplateColumn: Button
    public void OpenDetails(int rowIndex)
    {
        GetCell(rowIndex, "ActionsCell")
            .FindFirstDescendant(cf => cf.ByAutomationId("CustomerDetailsButton"))
            .AsButton()
            .Invoke();
    }
}
```

Für diese Variante müssen die Zellen selbst eine `AutomationId` erhalten. Das geht zuverlässig über einen `CellStyle`:

```xml
<Style x:Key="NameCellStyle" TargetType="DataGridCell">
    <Setter Property="AutomationProperties.AutomationId"
            Value="NameCell" />
</Style>
```

Und an der Spalte:

```xml
<DataGridTextColumn Header="Name"
                    Binding="{Binding Name}"
                    CellStyle="{StaticResource NameCellStyle}" />
```

Wiederholen Sie das für `EmailCell`, `IsActiveCell`, `StatusCell`, `WebsiteCell`, `BirthDateCell` und `ActionsCell`.

## E2E-Tests für alle Spaltentypen

```csharp
using CustomerApp.UiTests.Infrastructure;
using CustomerApp.UiTests.PageObjects;
using FlaUI.Core.Tools;

namespace CustomerApp.UiTests;

[TestClass]
public sealed class CustomerDataGridColumnE2eTests : WpfAppTestBase
{
    private CustomerGridPage CreateGridPage()
    {
        var window = Retry.WhileNull(
            () => App.GetMainWindow(Automation),
            timeout: TimeSpan.FromSeconds(10)).Result;

        return new CustomerGridPage(window);
    }

    [TestMethod]
    public void TextColumn_CanEditName()
    {
        var page = CreateGridPage();

        page.SetText(0, "Anna E2E");

        Assert.AreEqual("Anna E2E", page.GetText(0));
    }

    [TestMethod]
    public void CheckBoxColumn_CanToggleIsActive()
    {
        var page = CreateGridPage();
        var before = page.GetIsActive(0);

        page.ToggleIsActive(0);

        Assert.AreNotEqual(before, page.GetIsActive(0));
    }

    [TestMethod]
    public void ComboBoxColumn_CanSelectStatus()
    {
        var page = CreateGridPage();

        page.SetStatus(0, "Blocked");

        Assert.AreEqual("Blocked", page.GetStatus(0));
    }

    [TestMethod]
    public void HyperlinkColumn_DisplaysWebsite()
    {
        var page = CreateGridPage();

        Assert.Contains("example.com", page.GetWebsite(0));
    }

    [TestMethod]
    public void TemplateColumn_DatePicker_CanChangeDate()
    {
        var page = CreateGridPage();
        var newDate = new DateTime(2001, 12, 31);

        page.SetBirthDate(0, newDate);

        // Optional: über eine sichtbare Anzeige oder ein Test-Feld verifizieren.
    }

    [TestMethod]
    public void TemplateColumn_Button_RunsDetailsCommand()
    {
        var page = CreateGridPage();

        page.OpenDetails(0);

        // Beispiel: Prüfen über ein sichtbares Status-/Info-Feld.
        // Assert.AreEqual("Details: Anna Weber", mainWindowPage.LastAction);
    }
}
```

## Praktische Empfehlung

- Testen Sie **Text, CheckBox und ComboBox** bevorzugt über echte UI-Interaktion, weil deren Edit-Templates besonders fehleranfällig sind.
- Testen Sie **Hyperlink** primär als korrekte Anzeige und Bindung; das Öffnen externer Browser macht E2E-Tests unnötig instabil.
- Testen Sie **TemplateColumns** über die konkreten enthaltenen Controls, etwa `DatePicker`, `Button`, `TextBox` oder `ProgressBar`.
- Geben Sie jeder Zelle eine eigene `AutomationId`, nicht nur dem `DataGrid`. Das macht FlaUI-Selektoren robust gegenüber Layout-, Virtualisierungs- und Theme-Änderungen.
- Verwenden Sie `Retry.WhileNull` beim Auffinden von Edit-Controls, weil WPF Edit-Elemente erst beim Aktivieren der Zelle erzeugt. [learn.microsoft](http://www.learn.microsoft.com/en-us/dotnet/api/system.windows.controls.datagridtextcolumn?view=windowsdesktop-3.1)