namespace LandsatReflectance.Backend.Models.UsgsApi.Types;

// We cannot use the name 'Options' because it's the name of a property for json source generators (stuff that speeds up
// serialization). Naming it 'Options' will cause the code to error during compilation, but won't produce any intellisense
// errors.

public class QueryOptions
{
    public bool Bulk { get; set; }
    public bool Download { get; set; }
    public bool Order { get; set; }
    public bool Secondary { get; set; }
}