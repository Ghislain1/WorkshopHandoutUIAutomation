// // Description:Definition of ${File.FileName} class
// //
// // Copyright © 2025 - 2025, Alcon.All rights reserved.

using System.Configuration;
using System.Data;
using System.Windows;
using CustomerApp.Services;
using CustomerApp.ViewModels;

namespace CustomerApp;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application {
    protected override void OnStartup(StartupEventArgs e) {
        base.OnStartup(e);

        var viewModel = new MainViewModel(
            new InMemoryCustomerRepository(),
            new MessageBoxDialogService());

        new MainWindow { DataContext = viewModel }.Show();
    }
}

