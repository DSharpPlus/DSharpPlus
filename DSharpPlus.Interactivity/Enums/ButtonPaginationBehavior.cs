namespace DSharpPlus.Interactivity.Enums
{
    /// <summary>
    /// Represents options of how to handle pagination timing out.
    /// </summary>
    public enum ButtonPaginationBehavior
    {
        /// <summary>
        /// The buttons should be disabled when pagination times out.
        /// </summary>
        Disable,
        /// <summary>
        /// The buttons should be left as is when pagination times out.
        /// </summary>
        Ignore,
        /// <summary>
        /// The entire message should be deleted when pagination times out.
        /// </summary>
        DeleteMessage,
        /// <summary>
        /// The buttons should be removed entirely when pagination times out.
        /// </summary>
        DeleteButtons,
    }
}
