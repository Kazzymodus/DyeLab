using Newtonsoft.Json;

namespace DyeLab.Configuration;

public class Config
{
    public ConfigOption<string> TerrariaInstallationPath { get; } = new(Default.TerrariaInstallationPathSteam);

    [JsonConverter(typeof(ConfigOptionJsonConverter<string>))]
    public ConfigOption<string> OutputDirectory { get; } = new(Default.OutputDirectory);

    [JsonConverter(typeof(ConfigOptionJsonConverter<string>))]
    public ConfigOption<string> FxFilePath { get; } = new(Default.FxFilePath, x => x.EndsWith(".fx"));

    private static class Default
    {
        public const string TerrariaInstallationPathSteam = @"C:\Program Files (x86)\Steam\steamapps\common\Terraria";
        public const string TerrariaInstallationPathGog = @"C:\GOG Galaxy\Games\Terraria";
        public const string InputDirectory = "input";
        public const string OutputDirectory = "output";
        public const string FxFilePath = InputDirectory + "/myEffect.fx";
    }
}