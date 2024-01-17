using Newtonsoft.Json;

namespace DyeLab.Configuration;

public class Config
{
    public ConfigOption<string> TerrariaInstallationPath { get; } = new(Default.TerrariaInstallationPathSteam);
    public ConfigOption<string> OutputDirectory { get; } = new(Default.OutputDirectory);
    public ConfigOption<string> FxFilePath { get; } = new(Default.FxFilePath, x => x.EndsWith(".fx"));
    public ConfigOption<string?> CompiledFileName { get; } = new(string.Empty);

    private static class Default
    {
        public const string TerrariaInstallationPathSteam = @"C:/Program Files (x86)/Steam/steamapps/common/Terraria";
        public const string InputDirectory = "input";
        public const string OutputDirectory = "output";
        public const string FxFilePath = InputDirectory + "/myEffect.fx";
    }
}