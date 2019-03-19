using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Concurrency;
using DSharpPlus.Interactivity.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlus.Interactivity.EventHandling
{
    public class PaginationRequest
    {
        internal TaskCompletionSource<bool> _tcs;
        internal CancellationTokenSource _ct;
        internal TimeSpan _timeout;
        internal ConcurrentHashSet<Page> _pages;
        internal PaginationBehaviour _behaviour;
        internal PaginationDeletion _deletion;

        /// <summary>
        /// Creates a new Pagination request
        /// </summary>
        /// <param name="behaviour">Behaviour during pagination</param>
        /// <param name="deletion">Behavior on pagination end</param>
        /// <param name="timeout">Timeout time</param>
        /// <param name="pages">Pagination pages</param>
        public PaginationRequest(PaginationBehaviour behaviour, PaginationDeletion deletion, TimeSpan timeout, params Page[] pages)
        {
            this._tcs = new TaskCompletionSource<bool>();
            this._ct = new CancellationTokenSource(timeout);
            this._ct.Token.Register(() => _tcs.TrySetResult(true));
            this._timeout = timeout;

            this._pages = new ConcurrentHashSet<Page>();
            this._deletion = deletion;
            this._behaviour = behaviour;

            foreach(var p in pages)
            {
                this._pages.Add(p);
            }
        }

        /// <summary>
        /// Returns a page by its index.
        /// STORE THE INDEX PRESENT IN THE CLASS.
        /// INPUT INDEX MAY NOT BE THE SAME AS OUTPUT INDEX.
        /// </summary>
        /// <param name="index">Index to get page for</param>
        /// <returns></returns>
        internal Page GetContents(int index)
        {
            if(index < 0)
            {
                return _pages.First();
            }
            if (_pages.Any(x => x.Index == index))
            {
                return _pages.First(x => x.Index == index);
            }
            var highest = _pages.Max(x => x.Index);
            if(this._behaviour == PaginationBehaviour.WrapAround)
                return GetContents(index - highest);
            return _pages.First(x => x.Index == highest);
        }

        ~PaginationRequest()
        {
            this.Dispose();
        }

        /// <summary>
        /// Disposes this PaginationRequest.
        /// </summary>
        public void Dispose()
        {
            this._ct.Dispose();
            this._tcs = null;
        }
    }

    public class Page
    {
        public string Content { get; private set; }
        public DiscordEmbed Embed { get; private set; }
        public int Index { get; private set; }

        public Page(string content, DiscordEmbedBuilder embed, int index)
        {
            this.Content = content;
            this.Embed = embed.Build();
            this.Index = index;
        }
    }
}
