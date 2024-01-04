using System.Runtime.CompilerServices;
using DyeLab.Effects;
using DyeLab.Effects.Constants;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DyeLab.UI;

public sealed class DrawHelper : IDisposable
{
    private readonly SpriteBatch _spriteBatch;
    private EffectWrapper? _effect;

    public Vector2 DrawOffset { get; set; }

    private readonly Dictionary<ulong, Texture2D> _solidCache = new();
    private readonly Dictionary<ulong, Texture2D> _coloredSolidCache = new();

    private bool _effectApplied;

    private bool _isDisposed;

    public DrawHelper(SpriteBatch spriteBatch)
    {
        _spriteBatch = spriteBatch;
    }

    public void SetEffect(EffectWrapper effect)
    {
        _effect = effect;
    }

    public void DrawSolid(Vector2 offset, int width, int height, Color color, bool withEffect = false)
    {
        DrawSolid(offset, width, height, null, color, withEffect);
    }

    public void DrawSolid(Vector2 offset, int width, int height, Rectangle? sourceRectangle, Color color,
        bool withEffect = false)
    {
        var hash = GetSolidHash(width, height);
        _solidCache.TryGetValue(hash, out var solid);

        if (solid == null)
        {
            solid = CreateSolid(width, height, Color.White);
            _solidCache.Add(hash, solid);
        }

        DrawTexture(solid, offset, sourceRectangle, color, withEffect);
    }

    public void DrawColoredSolid(Vector2 offset, int width, int height, Rectangle? sourceRectangle, Color color,
        bool withEffect = false)
    {
        if (color == Color.White)
        {
            DrawSolid(offset, width, height, sourceRectangle, color, withEffect);
            return;
        }

        ThrowIfLargerThanMaxUShortValue(width);
        ThrowIfLargerThanMaxUShortValue(height);

        var hash = GetColoredSolidHash((ushort)width, (ushort)height, color);
        _coloredSolidCache.TryGetValue(hash, out var solid);

        if (solid == null)
        {
            solid = CreateSolid(width, height, color);
            _coloredSolidCache.Add(hash, solid);
        }

        DrawTexture(solid, offset, sourceRectangle, Color.White, withEffect);

        void ThrowIfLargerThanMaxUShortValue(int property,
            [CallerArgumentExpression("property")] string? propertyName = null)
        {
            if (property > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(propertyName, property,
                    $"{propertyName} must be smaller or equal to {ushort.MaxValue}.");
        }
    }

    private Texture2D CreateSolid(int width, int height, Color color)
    {
        var solid = new Texture2D(_spriteBatch.GraphicsDevice, width, height);
        var textureData = new Color[width * height];
        for (var i = 0; i < textureData.Length; i++)
            textureData[i] = color;
        solid.SetData(textureData);
        return solid;
    }

    public void DrawTexture(Texture2D texture, Vector2 offset, Rectangle? sourceRectangle, Color color,
        bool withEffect = false)
    {
        DrawTexture(texture, offset, sourceRectangle, color, 0f, Vector2.Zero, 1f, withEffect);
    }

    public void DrawTexture(Texture2D texture, Vector2 offset, Rectangle? sourceRectangle, Color color, float rotation,
        Vector2 origin, float scale, bool withEffect = false)
    {
        if (withEffect)
            PrepareEffectParameters(sourceRectangle, texture.Width, texture.Height);

        ApplyEffect(withEffect, withEffect);

        _spriteBatch.Draw(texture, DrawOffset + offset, sourceRectangle, color, rotation, origin, scale,
            SpriteEffects.None, 0f);
    }
    
    public void DrawText(SpriteFont font, string text, Vector2 position, Color color)
    {
        ApplyEffect(false);

        _spriteBatch.DrawString(font, text, DrawOffset + position, color);
    }

    private void ApplyEffect(bool withEffect, bool refresh = false)
    {
        if (!refresh && withEffect == _effectApplied)
            return;

        if (withEffect)
        {
            if (_effect == null)
                throw new InvalidOperationException("Effect has not been set yet.");

            _effect.Apply();
            _effectApplied = true;
            return;
        }

        _spriteBatch.ResetEffect();
        _effectApplied = false;
    }

    private void PrepareEffectParameters(Rectangle? sourceRectangle, int textureWidth, int textureHeight)
    {
        if (_effect == null)
            throw new InvalidOperationException("Effect has not been set yet.");

        if (sourceRectangle.HasValue)
        {
            var rectangle = new Vector4(
                sourceRectangle.Value.X,
                sourceRectangle.Value.Y,
                sourceRectangle.Value.Width,
                sourceRectangle.Value.Height);
            _effect.Parameters[TerrariaShaderParameters.Armor.SourceRectangle].SetValue(rectangle);
            _effect.Parameters[TerrariaShaderParameters.Armor.LegacySourceRectangle].SetValue(rectangle);
        }

        var vector = new Vector2(textureWidth, textureHeight);
        _effect.Parameters[TerrariaShaderParameters.Armor.ImageSize].SetValue(vector);
        _effect.Parameters[TerrariaShaderParameters.Armor.LegacyArmorImageSize].SetValue(vector);
    }

    private static ulong GetSolidHash(int width, int height) =>
        (uint)width | (ulong)height << 32;

    private static ulong GetColoredSolidHash(ushort width, ushort height, Color color) =>
        (ulong)width << 48 | (ulong)height << 32 | color.PackedValue;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if (disposing)
        {
            foreach (var solid in _solidCache.Values)
                solid.Dispose();
        }

        _isDisposed = true;
    }

    ~DrawHelper()
    {
        Dispose(false);
    }
}