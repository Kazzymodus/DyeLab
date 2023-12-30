using Microsoft.Xna.Framework;

namespace DyeLab.UI;

public class Panel : UIElement
{
    public static Builder New() => new();

    public class Builder : UIElementBuilder<Panel>
    {
        protected override Panel BuildElement()
        {
            return new Panel();
        }
    }
}