using System;
using System.Linq;
using MsAppToolkit;
using Xunit;
using Xunit.Abstractions;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets;

public class TempFormDiag
{
    private readonly ITestOutputHelper _output;
    public TempFormDiag(ITestOutputHelper output) => _output = output;

    [Fact]
    public void DumpForm1Yaml()
    {
        var msappPath = System.IO.Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..",
            "Rnwood.Dataverse.Data.PowerShell", "TestData", "AccountAppReference.msapp");
        var doc = MsAppDocument.Load(msappPath);
        var screens = doc.GetScreens();
        var screenName = screens.Select(s => s.Name).FirstOrDefault(s => s.Contains("Form")) ?? screens.First().Name;
        _output.WriteLine($"Screen: {screenName}");
        var yaml = doc.ExportScreenYaml(screenName, includeHeader: false, includeDefaults: false);
        var lines = yaml.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains("Form1:"))
            {
                for (int j = i; j < Math.Min(i + 60, lines.Length); j++)
                    _output.WriteLine(lines[j]);
                return;
            }
        }
        _output.WriteLine("Form1 not found. First 10 lines:");
        for (int k = 0; k < Math.Min(10, lines.Length); k++) _output.WriteLine(lines[k]);
    }
}
