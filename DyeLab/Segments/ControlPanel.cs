﻿using DyeLab.Effects;
using DyeLab.Effects.Constants;
using DyeLab.Segments.DataStructures;
using DyeLab.UI;
using DyeLab.UI.InputField;
using DyeLab.UI.ScrollableList;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ArmorShaderParam = DyeLab.Effects.Constants.TerrariaShaderParameters.Armor;

namespace DyeLab.Segments;

public static class ControlPanel
{
    private const int LabelHeight = 20;

    private const int SliderWidth = 100;
    private const int SliderHeight = 10;
    private const int SliderPadding = 20;

    private const int InputFieldWidth = 40;
    private const int InputFieldHeight = 20;

    private const int CheckboxSize = 12;

    public static Panel Build(Point position, SpriteFont font,
        IDictionary<string, EffectParameterWrapper> effectParameters, Action<float> timeScalarSetter,
        PassSliderData passSliderData, ImageControlData imageControlData)
    {
        var controlPanel = Panel.New().SetBounds(position.X, position.Y, 0, 0).Build();

        CreateColorSliders(controlPanel, 0, 0,
            (Float3EffectParameterWrapper)effectParameters[ArmorShaderParam.Color],
            new LabelData(font, "Color"),
            new InputFieldData(font)
        );

        CreateColorSliders(controlPanel, 180, 0,
            (Float3EffectParameterWrapper)effectParameters[ArmorShaderParam.SecondaryColor],
            new LabelData(font, "Secondary color"),
            new InputFieldData(font)
        );

        CreateLabeledSliderWithInputField(controlPanel, 0, 100, timeScalarSetter,
            new LabelData(font, "Time"),
            new SliderData(0f, 8f, 1f),
            new InputFieldData(font)
        );

        CreateLabeledSliderWithInputField(controlPanel, 180, 100,
            ((FloatEffectParameterWrapper)effectParameters[ArmorShaderParam.Opacity]).Set,
            new LabelData(font, "Opacity"),
            new SliderData(0f, 1f, 1f),
            new InputFieldData(font)
        );

        CreateLabeledSliderWithInputField(controlPanel, 0, 160,
            ((FloatEffectParameterWrapper)effectParameters[ArmorShaderParam.Saturation]).Set,
            new LabelData(font, "Saturation"),
            new SliderData(0f, 1f, 1f),
            new InputFieldData(font)
        );

        CreateLabeledSliderWithInputField(controlPanel, 180, 160,
            ((FloatEffectParameterWrapper)effectParameters[ArmorShaderParam.Rotation]).Set,
            new LabelData(font, "Rotation"),
            new SliderData(-2 * MathF.PI, 2 * MathF.PI, 0f),
            new InputFieldData(font)
        );

        CreatePassSlider(controlPanel, 0, 220, font, passSliderData);
        CreateImageControl(controlPanel, 0, 300, 1, font, imageControlData);

        controlPanel.SizeToContents();
        return controlPanel;
    }

    private static void CreateColorSliders(UIElement parent, int x, int y, Float3EffectParameterWrapper parameter,
        LabelData labelData, InputFieldData inputFieldData)
    {
        CreateLabel(parent, x, y, labelData);

        CreateColorSlider(y + SliderPadding * 0, parameter.SetX, Color.Red);
        CreateColorSlider(y + SliderPadding * 1, parameter.SetY, Color.Green);
        CreateColorSlider(y + SliderPadding * 2, parameter.SetZ, Color.Blue);

        void CreateColorSlider(int yOffset, Action<float> action, Color color)
        {
            CreateSliderWithInputField(parent, x, y + LabelHeight + yOffset, action,
                new SliderData(0f, 2f, 1f, color), inputFieldData);
        }
    }

    private static void CreatePassSlider(UIElement parent, int x, int y, SpriteFont font, PassSliderData passSliderData)
    {
        const int width = 350;
        const int belowSliderY = 48;

        var parentPosition = parent.Position;

        CreateLabel(parent, x, y, new LabelData(font, "Pass"));

        CreateLabel(parent, x + 20, y + belowSliderY - CheckboxSize / 6, new LabelData(font, "Vanilla"));
        var vanillaCheckbox = Checkbox.New()
            .SetBounds(parent.Position.X + x, parent.Position.Y + y + belowSliderY, CheckboxSize, CheckboxSize)
            .Build();
        vanillaCheckbox.ValueChanged += b =>
            passSliderData.SetActiveEffectDelegate(b ? EffectType.Vanilla : EffectType.Editor);

        parent.AddChild(vanillaCheckbox);

        var passLabel = CreateLabel(parent, x + (int)(width * 0.5f) + SliderPadding,
            y + LabelHeight + SliderHeight + 16,
            new LabelData(font, passSliderData.ActiveEffect.CurrentPass.Name, new Vector2(200, 20)));
        var passSlider = Slider.New()
            .SetMinMaxValues(0, passSliderData.ActiveEffect.Passes.Count - 1)
            .SetBounds(parentPosition.X + x, parentPosition.Y + y + LabelHeight + SliderHeight / 2, width, SliderHeight)
            .Build();
        passSlider.ValueChanged +=
            (_, args) => passSliderData.ActiveEffect.SetPassIndex((int)Math.Round(args.NewValue));
        passSlider.ValueChanged += (_, args) =>
            passLabel.SetText(passSliderData.ActiveEffect.Passes[(int)Math.Round(args.NewValue)].Name);
        parent.AddChild(passSlider);

        passSliderData.ActiveEffect.EffectChanged += (_, args) =>
        {
            passSlider.SetMinMaxValue(0, args.NewEffect.CurrentTechnique.Passes.Count - 1);
        };
    }

    private static void CreateImageControl(UIElement parent, int x, int y, int imageIndex, SpriteFont font,
        ImageControlData imageControlData)
    {
        const int texturePreviewSize = 160;
        const int scrollListWidth = 180;
        const int scrollListHeight = 160;
        const int buttonWidth = 40;
        const int buttonHeight = 20;
        
        const int sizeLabelPadding = 22;
        const int scrollListPadding = 20;

        if (imageIndex <= 0)
            throw new ArgumentOutOfRangeException(nameof(imageIndex), imageIndex, "Index must be higher than 0.");

        var label = CreateLabel(parent, x, y, new LabelData(font, "Image"));

        var parentPosition = parent.Position;
        var texturePreview = TexturePreview.New()
            .SetFont(font)
            .SetBounds(parentPosition.X + x, parentPosition.Y + y + label.Height, texturePreviewSize,
                texturePreviewSize)
            .Build();
        var sizeLabel = CreateLabel(parent, x, y + texturePreviewSize + sizeLabelPadding,
            new LabelData(font, "-", new Vector2(texturePreviewSize, LabelHeight)));

        var internalImageScrollList = ScrollableList<int>.New()
            .SetListItems(ImagesToEntries(imageControlData.AssetManager.Images))
            .SetItemHeight(20)
            .SetFont(font)
            .SetBounds(parentPosition.X + x + texturePreviewSize + scrollListPadding,
                parentPosition.Y + y + label.Height,
                scrollListWidth, scrollListHeight)
            .Build();
        internalImageScrollList.ValueChanged += (_, args) =>
        {
            var index = args.NewValue;
            var images = imageControlData.AssetManager.Images;
            var image = index > 0 ? images[index - 1] : null;
            texturePreview.SetTexture(image);
            imageControlData.GraphicsDevice.Textures[imageIndex] = image;
            sizeLabel.SetText(image != null ? $"{image.Width}X{image.Height}" : "-");
        };
        imageControlData.AssetManager.ImagesUpdated += images =>
        {
            internalImageScrollList.SetEntries(ImagesToEntries(images));
        };

        parent.AddChild(texturePreview);
        parent.AddChild(internalImageScrollList);

        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            var openImagesDirectoryButton = Button.New()
                .SetFont(font)
                .SetLabel("Open")
                .SetBounds(parent.Position.X + x + texturePreviewSize + scrollListPadding,
                    parent.Position.Y + y + label.Height + scrollListHeight, buttonWidth, buttonHeight)
                .Build();
            openImagesDirectoryButton.Clicked += () =>
            {
                var folderPath = "Content" + Path.DirectorySeparatorChar + "Images";
                Directory.CreateDirectory(folderPath);
                OS.Open(folderPath + Path.DirectorySeparatorChar);
            };
            parent.AddChild(openImagesDirectoryButton);
        }

        IEnumerable<ScrollableListItem<int>> ImagesToEntries(ICollection<Texture2D> images)
        {
            var entries = new ScrollableListItem<int>[images.Count + 1];
            entries[0] = new ScrollableListItem<int>("None", 0);
            var index = 1;
            foreach (var image in images)
            {
                entries[index] = new ScrollableListItem<int>(image.Name, index);
                index++;
            }

            return entries;
        }
    }

    private static void CreateLabeledSliderWithInputField(UIElement parent, int x, int y,
        Action<float> valueChangedDelegate, LabelData labelData, SliderData sliderData, InputFieldData inputFieldData)
    {
        CreateLabel(parent, x, y, labelData);
        CreateSliderWithInputField(parent, x, y + LabelHeight, valueChangedDelegate, sliderData,
            inputFieldData);
    }

    private static void CreateSliderWithInputField(UIElement parent, int x, int y, Action<float> valueChangedDelegate,
        SliderData sliderData, InputFieldData inputFieldData)
    {
        var parentPosition = parent.Position;
        var slider = Slider.New()
            .SetMinMaxValues(sliderData.MinValue, sliderData.MaxValue)
            .SetBackgroundColor(sliderData.BarColor ?? Color.White)
            .SetBounds(parentPosition.X + x, parentPosition.Y + y + SliderHeight / 2, SliderWidth, SliderHeight)
            .Build();

        var inputField = FloatInputField.New()
            .SetFont(inputFieldData.Font)
            .SetBounds(parentPosition.X + x + SliderWidth + SliderPadding, parentPosition.Y + y, InputFieldWidth,
                InputFieldHeight)
            .Build();

        slider.ValueChanged += (_, args) => valueChangedDelegate(args.NewValue);
        slider.ValueChanged += (_, args) => inputField.SetValue(args.NewValue);

        inputField.Commit += (_, args) => valueChangedDelegate(args.NewValue);
        inputField.Commit += (_, args) => slider.SetValue(args.NewValue);

        slider.SetValue(sliderData.StartValue);

        parent.AddChild(slider);
        parent.AddChild(inputField);
    }

    private static Label CreateLabel(UIElement parent, int x, int y, LabelData labelData)
    {
        var parentPosition = parent.Position;
        var size = labelData.Size ?? labelData.Font.MeasureString(labelData.Text);

        var label = Label.New()
            .SetFont(labelData.Font)
            .SetText(labelData.Text)
            .SetBounds(parentPosition.X + x, parentPosition.Y + y, (int)size.X, (int)size.Y)
            .Build();

        parent.AddChild(
            label
        );

        return label;
    }
}