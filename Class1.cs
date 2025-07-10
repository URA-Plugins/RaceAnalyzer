using Gallop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using System.IO.Compression;
using UmamusumeResponseAnalyzer;
using UmamusumeResponseAnalyzer.Plugin;

namespace RaceAnalyzer
{
    public class RaceAnalyzer : IPlugin
    {
        [PluginDescription("保存比赛相关数据")]
        public string Name => "RaceAnalyzer";
        public Version Version => new(1, 0, 0, 0);
        public string Author => "离披";
        public string[] Targets => [];

        public async Task UpdatePlugin(ProgressContext ctx)
        {
            var progress = ctx.AddTask($"[{Name}] 更新");

            using var client = new HttpClient();
            using var resp = await client.GetAsync($"https://api.github.com/repos/URA-Plugins/{Name}/releases/latest");
            var json = await resp.Content.ReadAsStringAsync();
            var jo = JObject.Parse(json);

            var isLatest = ("v" + Version.ToString()).Equals("v" + jo["tag_name"]?.ToString());
            if (isLatest)
            {
                progress.Increment(progress.MaxValue);
                progress.StopTask();
                return;
            }
            progress.Increment(25);

            var downloadUrl = jo["assets"][0]["browser_download_url"].ToString();
            if (Config.Updater.IsGithubBlocked && !Config.Updater.ForceUseGithubToUpdate)
            {
                downloadUrl = downloadUrl.Replace("https://", "https://gh.shuise.dev/");
            }
            using var msg = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
            using var stream = await msg.Content.ReadAsStreamAsync();
            var buffer = new byte[8192];
            while (true)
            {
                var read = await stream.ReadAsync(buffer);
                if (read == 0)
                    break;
                progress.Increment(read / msg.Content.Headers.ContentLength ?? 1 * 0.5);
            }
            using var archive = new ZipArchive(stream);
            archive.ExtractToDirectory(Path.Combine("Plugins", Name), true);
            progress.Increment(25);

            progress.StopTask();
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
