using FlaUI.Core;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using System;


namespace CustomerApp.UiTests.Infrastructure;

public abstract class WpfAppTestBase : IDisposable {
    protected Application App { get; private set; } = null!;
    protected UIA3Automation Automation { get; private set; } = null!;

    [TestInitialize]
    public void StartApplication() {
        Automation = new UIA3Automation();

        var exePath = Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "src", "CustomerApp", "bin", "Debug",
            "net10.0-windows", "CustomerApp.exe");

        App = Application.Launch(exePath);

        Retry.WhileNull(
            () => App.GetMainWindow(Automation),
            timeout: TimeSpan.FromSeconds(10));
    }

    [TestCleanup]
    public void StopApplication() {
        App?.Close();
        App?.Dispose();
        Automation?.Dispose();
    }

    public void Dispose() {
        StopApplication();
    }
}