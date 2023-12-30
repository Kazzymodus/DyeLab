using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace DyeLab.Configuration;

public class ConfigManager
{
    private const string ConfigPath = "config.json";

    private readonly JsonSerializerSettings _serializerSettings = new()
    {
        Formatting = Formatting.Indented
    };

    public Config? Config { get; private set; }

    [MemberNotNull(nameof(Config))]
    public void Load()
    {
        if (!File.Exists(ConfigPath))
        {
            CreateNewConfigFile();
            return;
        }

        var configText = File.ReadAllText(ConfigPath);
        var config = JsonConvert.DeserializeObject<Config>(configText, _serializerSettings);
        if (config != null)
        {
            Config = config;
            return;
        }
        
        CreateNewConfigFile();
    }

    public void Save()
    {
        if (Config == null)
            throw new InvalidOperationException("No config has been loaded yet.");
        
        var configText = JsonConvert.SerializeObject(Config, _serializerSettings);
        File.WriteAllText(ConfigPath, configText);
    }

    [MemberNotNull(nameof(Config))]
    private void CreateNewConfigFile()
    {
        Config = new Config();
        Save();
    }
}