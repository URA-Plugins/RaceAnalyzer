using Gallop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using UmamusumeResponseAnalyzer.Plugin;

namespace RaceAnalyzer
{
    public class RaceAnalyzer : IPlugin
    {
        public string Name => "RaceAnalyzer";
        public Version Version => new(1, 0, 0, 0);
        public string Author => "离披";

        public async Task UpdatePlugin(ProgressContext ctx)
        {
            throw new NotImplementedException();
        }

        [Analyzer]
        public void Analyze(JObject jo)
        {
            var dyn = (dynamic)jo;
            AnalyzeRoomMatch(dyn);
            AnalyzePracticeRace(dyn);
            AnalyzeChampionsRace(dyn);
        }

        public void AnalyzeRoomMatch(dynamic dyn)
        {
            if (dyn.data == null || dyn.data.race_scenario == null || dyn.data.random_seed == null || dyn.data.race_horse_data_array == null || dyn.data.trained_chara_array == null || dyn.data.season == null || dyn.data.weather == null || dyn.data.ground_condition == null)
                return;
            var data = ((RoomMatchRaceStartResponse)dyn.ToObject<RoomMatchRaceStartResponse>()).data;
            Directory.CreateDirectory("races");
            var lines = new List<string>
            {
                $"Race Scenario:",
                data.race_scenario,
                string.Empty,
                $"Race Horse Data Array",
                JsonConvert.SerializeObject(data.race_horse_data_array),
                string.Empty,
                $"Trained Characters:"
                };
            foreach (var i in data.trained_chara_array)
            {
                lines.Add(JsonConvert.SerializeObject(i, Formatting.None));
                lines.Add(string.Empty);
            }
            File.WriteAllLines(@$"./races/{DateTime.Now:yy-MM-dd HH-mm-ss-fff} RoomMatch.txt", lines);
        }
        public void AnalyzePracticeRace(dynamic dyn)
        {
            if (dyn.data == null || dyn.data.trained_chara_array == null || dyn.data.race_result_info == null || dyn.data.entry_info_array == null || dyn.data.practice_race_id == null || dyn.data.state == null || dyn.data.practice_partner_owner_info_array == null)
                return;
            var data = ((PracticeRaceRaceStartResponse)dyn.ToObject<PracticeRaceRaceStartResponse>()).data;
            Directory.CreateDirectory("races");
            var lines = new List<string>
            {
                $"Race Scenario:",
                data.race_result_info.race_scenario,
                string.Empty,
                $"Race Horse Data Array",
                JsonConvert.SerializeObject(data.race_result_info.race_horse_data_array),
                string.Empty,
                $"Trained Characters:"
                };
            foreach (var i in data.trained_chara_array)
            {
                lines.Add(JsonConvert.SerializeObject(i, Formatting.None));
                lines.Add(string.Empty);
            }
            File.WriteAllLines(@$"./races/{DateTime.Now:yy-MM-dd HH-mm-ss-fff} PracticeRace.txt", lines);
        }
        public void AnalyzeChampionsRace(dynamic dyn)
        {
            if (dyn.data == null || dyn.data.room_info == null || dyn.data.room_user_array == null || dyn.data.race_horse_data_array == null || dyn.data.trained_chara_array == null)
            {
                return;
            }
            var data = ((ChampionsFinalRaceStartResponse)dyn.ToObject<ChampionsFinalRaceStartResponse>()).data;
            Directory.CreateDirectory("races");
            var lines = new List<string>
                {
                    $"Race Scenario:",
                    data.room_info.race_scenario,
                    string.Empty,
                    $"Race Horse Data Array",
                    JsonConvert.SerializeObject(data.race_horse_data_array),
                    string.Empty,
                    $"Trained Characters:"
                };
            foreach (var i in data.trained_chara_array)
            {
                lines.Add(JsonConvert.SerializeObject(i, Formatting.None));
                lines.Add(string.Empty);
            }
            File.WriteAllLines(@$"./races/{DateTime.Now:yy-MM-dd HH-mm-ss-fff} Champions.txt", lines);
        }
    }
}
