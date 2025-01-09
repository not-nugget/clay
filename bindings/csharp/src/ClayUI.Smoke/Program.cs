
using Clay;

ClayContext? context = null;

try
{
    context = ClayContext.Create(new ClayDimensions
    {
        Width = 100,
        Height = 100
    });

    var layout = context.Layout();
    
    layout.Element().End();
    
    layout.End();

    var renderArray = layout.RenderCommands;
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

context?.Dispose();