using System.Threading.Tasks;

namespace DSharpPlus.Entities
{
    public abstract class BaseDiscordBuilder<ReturnType, SendType>
    {
        public abstract Task<ReturnType> SendAsync(SendType type);

        //public abstract Task<ReturnType> ModifyAsync(ReturnType type);

        public abstract void Clear();

        internal abstract void Validate(bool isModify = false);
    }
}
