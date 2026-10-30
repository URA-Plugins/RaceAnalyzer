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
act workflow_dispatch --artifact-server-path "$env:TEMP/ura-act-artifacts"
```

`tests/RaceAnalyzerSmoke` 默认检查本仓发布边界、真实 Host 的闲置生命周期及比赛文件输出。测试工具源码由 `tests/Support/` 持有。

## 验证与发布

在 Windows 仓库根执行 `act workflow_dispatch --artifact-server-path "$env:TEMP/ura-act-artifacts"`。本地与 GitHub 使用同一份 workflow；版本 tag 触发 GitHub Release 发布。环境要求、共用 workflow 本地映射和发布规则见 [URA plugin workflows](https://github.com/URA-Plugins/.github/blob/v1/README.md)。
