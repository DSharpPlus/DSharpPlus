namespace DSharpPlus.Net
{
    /// <summary>
    /// Defines the HTTP method to use for an HTTP request.
    /// </summary>
    public enum RestRequestMethod : int
    {
        /// <summary>
        /// Defines that the request is a GET request.
        /// </summary>
        GET = 0,

        /// <summary>
        /// Defines that the request is a POST request.
        /// </summary>
        POST = 1,

        /// <summary>
        /// Defines that the request is a DELETE request.
        /// </summary>
        DELETE = 2,

        /// <summary>
        /// Defines that the request is a PATCH request.
        /// </summary>
        PATCH = 3,

        /// <summary>
        /// Defines that the request is a PUT request.
        /// </summary>
        PUT = 4,

        /// <summary>
        /// Defines that the request is a HEAD request.
        /// </summary>
        HEAD = 5
    }
}
