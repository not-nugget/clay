using Clay;

ClayContext? context = null;

try
{
    context = ClayContext.Create(new ClayDimensions
    {
        Width  = 100,
        Height = 100,
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
        using 
        (
            layout.Element(l, r)
        )
        {
            layout.Element("IdentifiedElement").End();
            //layout.Text("Text!");
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
}

context?.Dispose();

_ = "";

GC.Collect();

_ = "";