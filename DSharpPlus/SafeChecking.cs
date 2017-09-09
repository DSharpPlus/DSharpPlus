namespace DSharpPlus
{
    public static class SafeChecking
    {
        public static bool IsText(this ChannelType channelType)
        {
            return channelType == ChannelType.Text || channelType == ChannelType.Private || channelType == ChannelType.Group;
        }
    }
}
