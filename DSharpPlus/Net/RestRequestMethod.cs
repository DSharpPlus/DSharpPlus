namespace DSharpPlus.Net
{
    /// <summary>
    /// Defines the HTTP method to use for an HTTP request.
    /// </summary>
    public enum RestRequestMethod
    {
        /// <summary>
        /// Defines that the request is a GET request.
        /// </summary>
        Get = 0,

        /// <summary>
        /// Defines that the request is a POST request.
        /// </summary>
        Post = 1,

        /// <summary>
        /// Defines that the request is a DELETE request.
        /// </summary>
        Delete = 2,

        /// <summary>
        /// Defines that the request is a PATCH request.
        /// </summary>
        Patch = 3,

        /// <summary>
        /// Defines that the request is a PUT request.
        /// </summary>
        Put = 4,

        /// <summary>
        /// Defines that the request is a HEAD request.
        /// </summary>
        Head = 5
    }
}
