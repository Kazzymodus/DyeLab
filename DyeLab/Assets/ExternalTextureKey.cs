namespace DyeLab.Assets;

public class ExternalTextureKey
{
    public string TextureName { get; }
    public int ExternalId { get; }

    public ExternalTextureKey(string textureName, int externalId)
    {
        TextureName = textureName;
        ExternalId = externalId;
    }
}