using Gallop;
using Gallop.Endpoints;
using System.Text.Json;
using UmamusumeResponseAnalyzer.Plugin;

namespace RaceAnalyzer
{
    public class RaceAnalyzer : IPlugin
    {
        static readonly JsonSerializerOptions JsonOptions = new()
        {
            IncludeFields = true
        };

        public void Initialize(IPluginContext context) { }

        [ResponseAnalyzer<GameApi.RoomMatch.RaceStart>]
        public ValueTask AnalyzeRoomMatch(RoomMatchRaceStartResponse response)
        {
            var data = response.data;
            WriteRace("RoomMatch", data.race_scenario, data.race_horse_data_array, data.trained_chara_array);
            return ValueTask.CompletedTask;
        }

        [ResponseAnalyzer<GameApi.PracticeRace.RaceStart>]
        public ValueTask AnalyzePracticeRace(PracticeRaceRaceStartResponse response)
        {
            var data = response.data;
            WriteRace("PracticeRace", data.race_result_info.race_scenario, data.race_result_info.race_horse_data_array, data.trained_chara_array);
            return ValueTask.CompletedTask;
        }

        [ResponseAnalyzer<GameApi.Champions.RaceStart>]
        public ValueTask AnalyzeChampionsRace(ChampionsRaceStartResponse response)
        {
            var data = response.data;
            WriteRace("Champions", data.room_info.race_scenario, data.race_horse_data_array, data.trained_chara_array);
            return ValueTask.CompletedTask;
        }

        [ResponseAnalyzer<GameApi.Champions.FinalRaceStart>]
        public ValueTask AnalyzeChampionsFinalRace(ChampionsFinalRaceStartResponse response)
        {
            var data = response.data;
            WriteRace("ChampionsFinal", data.room_info.race_scenario, data.race_horse_data_array, data.trained_chara_array);
            return ValueTask.CompletedTask;
        }

        static void WriteRace(string source, string raceScenario, RaceHorseData[] raceHorseData, TrainedChara[] trainedCharacters)
        {
            Directory.CreateDirectory("races");
            var lines = new List<string>
            {
                "Race Scenario:",
                raceScenario,
                string.Empty,
                "Race Horse Data Array",
                JsonSerializer.Serialize(raceHorseData, JsonOptions),
                string.Empty,
                "Trained Characters:"
            };

            foreach (var character in trainedCharacters)
            {
                lines.Add(JsonSerializer.Serialize(character, JsonOptions));
                lines.Add(string.Empty);
            }

            File.WriteAllLines(@$"./races/{DateTime.Now:yy-MM-dd HH-mm-ss-fff} {source}.txt", lines);
        }
    }
}
