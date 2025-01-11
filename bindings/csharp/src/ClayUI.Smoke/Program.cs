using Clay;
using Clay.Types.Element;

ClayContext? context = null;

try
{
    context = ClayContext.Create(
        (100, 100),
        (ref string text, ref TextElementConfig config) => (5, 5),
        errorData =>
        {
            var a = $"Clay observed an error: \"[{errorData.ErrorType}] {errorData.ErrorText}\" (UserDataPtr: {errorData.UserData})";
            Console.Error.WriteLine(a);
            throw new ApplicationException(a);
        });

    var l = new LayoutConfig
    {
        Sizing          = new Sizing { Width = 100, Height = 100 },
        Padding         = new Padding { X    = 10, Y       = 10 },
        ChildGap        = 5,
        LayoutDirection = LayoutDirection.TopToBottom,
    };
    var r = new RectangleElementConfig
    {
        Color        = (255, 0, 0),
        CornerRadius = 10f,
    };
    
    var layout = context.Layout();
    using (layout)
    {
        using(layout.Element(l, r))
        {
            layout.Element("IdentifiedElement").End();
            layout.Text("Text!");
            layout.Element(l, r).End();
            //layout.Text("Text!");
            layout.Element(l, r).End();

            using (layout.Element(l, r))
            {
                //layout.Text("Text!");
                layout.Element(l, r).End();
                layout.Element(l, r).End();
                layout.Element(l, r).End();
                //layout.Text("Text!");
            }

            layout.Element(l, r).End();
        }

        //layout.Text("Text!");
        layout.Element(l, r).End();
        layout.Element(l, r).End();
    }

    var renderArray = layout.RenderCommands;
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    throw;
}

context?.Dispose();

_ = "";