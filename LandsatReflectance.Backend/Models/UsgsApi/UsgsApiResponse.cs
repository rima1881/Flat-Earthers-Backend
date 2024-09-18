namespace LandsatReflectance.Backend.Models.UsgsApi;

// Responses from the API have the base structure defined in 'UsgsApiResponse'.
// The 'Data' property differs depending on the endpoint we're querying.
// The 'IUsgsApiResponseData' interface represents a type that represents some data, generally corresponding to a
// particular endpoint.

public interface IUsgsApiResponseData
{ }

public class UsgsApiResponse<T> where T : class, IUsgsApiResponseData 
{
    public int RequestId { get; set; }
    public string Version { get; set; } = string.Empty;
    public T? Data { get; set; } 
    public int? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}