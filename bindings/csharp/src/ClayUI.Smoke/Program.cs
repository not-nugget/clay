
using Clay;

ClayContext? context = null;

try
{
    context = ClayContext.Create(new ClayDimensions
    {
        Width = 100,
        Height = 100,
    });

    var layout = context.Layout();
    using (layout)
    {
        using (layout.Element())
        {
            layout.Element("IdentifiedElement").End();
            layout.Text("Text!");
            layout.Element().End();
            layout.Text("Text!");
            layout.Element().End();
            
            using (layout.Element())
            {
                layout.Text("Text!");
                layout.Element().End();
                layout.Element().End();
                layout.Element().End();
                layout.Text("Text!");
            }
            
            layout.Element().End();
        }
        
        layout.Text("Text!");
        layout.Element().End();
        layout.Element().End();
    }
    
    var renderArray = layout.RenderCommands;
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

context?.Dispose();