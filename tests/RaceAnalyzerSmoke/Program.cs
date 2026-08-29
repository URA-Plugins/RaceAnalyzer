using Gallop;
using Terminal.Gui.App;
using UmamusumeResponseAnalyzer.Plugin;
using RacePlugin = RaceAnalyzer.RaceAnalyzer;

var originalCwd = Directory.GetCurrentDirectory();
var workspace = Path.Combine(Path.GetTempPath(), "ura-race-smoke-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(workspace);
Directory.SetCurrentDirectory(workspace);

try
{
    var tests = new (string Name, Func<ValueTask> Run)[]
    {
        ("Race analyzers write race files", TestRaceAnalyzersWriteRaceFiles),
    };

    var failures = new List<string>();
    foreach (var test in tests)
    {
        try
        {
            await test.Run();
            Console.WriteLine($"PASS {test.Name}");
        }
        catch (Exception ex)
        {
            failures.Add($"{test.Name}: {ex}");
            Console.Error.WriteLine($"FAIL {test.Name}");
            Console.Error.WriteLine(ex);
        }
    }

    if (failures.Count != 0)
    {
        Console.Error.WriteLine();
        Console.Error.WriteLine($"FAILED RaceAnalyzer smoke tests: {failures.Count}");
        foreach (var failure in failures)
        {
            Console.Error.WriteLine();
            Console.Error.WriteLine(failure);
        }

        Environment.Exit(1);
    }

    Console.WriteLine();
    Console.WriteLine($"PASS RaceAnalyzer smoke tests: {tests.Length}");
}
finally
{
    Directory.SetCurrentDirectory(originalCwd);
    Directory.Delete(workspace, recursive: true);
}

static async ValueTask TestRaceAnalyzersWriteRaceFiles()
{
    var plugin = new RacePlugin();
    plugin.Initialize(new ThrowingPluginContext());

    try
    {
        await AssertWritesRaceFile(
            "RoomMatch",
            "room-match-scenario",
            () => plugin.AnalyzeRoomMatch(new RoomMatchRaceStartResponse
            {
                data = new()
                {
                    race_scenario = "room-match-scenario",
                    race_horse_data_array = [],
                    trained_chara_array = [],
                }
            }));

        await AssertWritesRaceFile(
            "PracticeRace",
            "practice-race-scenario",
            () => plugin.AnalyzePracticeRace(new PracticeRaceRaceStartResponse
            {
                data = new()
                {
                    race_result_info = new()
                    {
                        race_scenario = "practice-race-scenario",
                        race_horse_data_array = [],
                    },
                    trained_chara_array = [],
                }
            }));

        await AssertWritesRaceFile(
            "Champions",
            "champions-scenario",
            () => plugin.AnalyzeChampionsRace(new ChampionsRaceStartResponse
            {
                data = new()
                {
                    room_info = new() { race_scenario = "champions-scenario" },
                    race_horse_data_array = [],
                    trained_chara_array = [],
                }
            }));

        await AssertWritesRaceFile(
            "ChampionsFinal",
            "champions-final-scenario",
            () => plugin.AnalyzeChampionsFinalRace(new ChampionsFinalRaceStartResponse
            {
                data = new()
                {
                    room_info = new() { race_scenario = "champions-final-scenario" },
                    race_horse_data_array = [],
                    trained_chara_array = [],
                }
            }));
    }
    finally
    {
        ((IPlugin)plugin).Dispose();
    }
}

static async ValueTask AssertWritesRaceFile(string source, string scenario, Func<ValueTask> act)
{
    var existing = Directory.Exists("races")
        ? Directory.GetFiles("races", $"* {source}.txt").ToHashSet(StringComparer.OrdinalIgnoreCase)
        : [];

    await act();

    var created = Directory
        .GetFiles("races", $"* {source}.txt")
        .Where(path => !existing.Contains(path))
        .ToArray();
    if (created.Length != 1)
        throw new InvalidOperationException($"{source} analyzer should create exactly one race file, got {created.Length}.");

    var text = File.ReadAllText(created[0]);
    foreach (var expected in new[] { "Race Scenario:", scenario, "Race Horse Data Array", "Trained Characters:" })
    {
        if (!text.Contains(expected, StringComparison.Ordinal))
            throw new InvalidOperationException($"{source} race file does not contain '{expected}'.");
    }
}

sealed class ThrowingPluginContext : IPluginContext
{
    public IApplication Application => throw new InvalidOperationException("RaceAnalyzer must not access the Terminal.Gui application.");
    public IPluginHostEvents Events => throw new InvalidOperationException("RaceAnalyzer must not subscribe to host events.");
    public IPluginAnalyzerRegistry Analyzers => throw new InvalidOperationException("RaceAnalyzer must not register runtime analyzers.");
    public bool IsPluginAvailable(string internalName) => false;
    public void RunBackground(Func<CancellationToken, ValueTask> operation) =>
        throw new InvalidOperationException("RaceAnalyzer must not run background operations.");
}
