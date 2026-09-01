# RaceAnalyzer

RaceAnalyzer 是 UmamusumeResponseAnalyzer 插件，用于保存房间赛、练习赛、冠军赛和冠军赛决赛的比赛数据。

## 行为

- 监听 `RoomMatch.RaceStart`、`PracticeRace.RaceStart`、`Champions.RaceStart` 和 `Champions.FinalRaceStart` 响应。
- 每次响应在宿主当前工作目录的 `races/` 下写入一个 `yy-MM-dd HH-mm-ss-fff <source>.txt` 文件。
- 文件包含比赛场景、比赛马匹数组，以及逐个序列化的育成角色数据。
- 插件没有专属配置，也不处理上述四类响应以外的比赛。

## 构建

仓库通过 `NuGet.Config` 恢复 `UmamusumeResponseAnalyzer` 编译期包。在仓库根执行：

```powershell
dotnet build .\RaceAnalyzer.csproj -c Release -m:1 -p:RuntimeIdentifier=win-x64 -p:SelfContained=false -p:PlatformTarget=AnyCPU -p:DeployUraPluginToLocalAppDataOnBuild=false
dotnet run --project .\tests\RaceAnalyzerSmoke\RaceAnalyzerSmoke.csproj -c Release -p:GenerateUraPluginManifestOnBuild=false -p:PackageUraPluginOnBuild=false -p:DeployUraPluginToLocalAppDataOnBuild=false
```
