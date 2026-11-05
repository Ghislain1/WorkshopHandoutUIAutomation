using FlaUI.Core;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using System;


namespace CustomerApp.UiTests.Infrastructure;

public abstract class WpfAppTestBase : IDisposable {
    protected Application App { get; private set; } = null!;
    protected UIA3Automation Automation { get; private set; } = null!;

    private static string ExePath => Path.Combine(
        AppContext.BaseDirectory,
        "..", "..", "..", "..", "..",
        "src", "CustomerApp", "bin", "Debug",
        "net10.0-windows", "CustomerApp.exe");

    [TestInitialize]
    public void StartApplication() {
        LaunchApplication();
    }

    [TestCleanup]
    public void StopApplication() {
        App?.Close();
        App?.Dispose();
        Automation?.Dispose();
    }

    public void RestartApplication() {
        StopApplication();
        LaunchApplication();
    }

    public void WaitForConvience(int timeInSeconde = 5) {
        Thread.Sleep(timeInSeconde*1000);
    }

    private void LaunchApplication() {
        Automation = new UIA3Automation();
        App = Application.Launch(ExePath);

        Retry.WhileNull(
            () => App.GetMainWindow(Automation),
            timeout: TimeSpan.FromSeconds(10));
    }

    public void Dispose() {
        StopApplication();
    }
}