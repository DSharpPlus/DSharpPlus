using DSharpPlus.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlus.Entities
{
    public class PropertyChangedBase : INotifyPropertyChanged
    {
        public static IUIThreadDispatcher Dispatcher { get; set; }

        public static void SetDispatcher<T>() where T : IUIThreadDispatcher, new() => Dispatcher = (T)Activator.CreateInstance(typeof(T));

        public static void SetDispatcher(IUIThreadDispatcher dispatcher) => Dispatcher = dispatcher;

        private ConcurrentDictionary<SynchronizationContext, List<PropertyChangedEventHandler>> _handlers
            = new ConcurrentDictionary<SynchronizationContext, List<PropertyChangedEventHandler>>();

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                var context = SynchronizationContext.Current;
                if (_handlers.TryGetValue(context, out var list))
                {
                    list.Add(value);
                }
                else
                {
                    _handlers[context] = new List<PropertyChangedEventHandler>() { value };
                }

                //Debug.WriteLine($"Added event handler, {this}, {value}");
            }

            remove
            {
                var context = SynchronizationContext.Current;
                if (_handlers.TryGetValue(context, out var list))
                {
                    list.Remove(value);
                }

                //Debug.WriteLine($"Removed event handler, {this}, {value}");
            }
        }

        // Holy hell is the C# Discord great.
        // Y'all should join https://aka.ms/csharp-discord
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void OnPropertySet<T>(ref T oldValue, T newValue, [CallerMemberName] string property = null)
        {
            if (oldValue == null || newValue == null || !newValue.Equals(oldValue))
            {
                oldValue = newValue;
                InvokePropertyChanged(property);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InvokePropertyChanged([CallerMemberName] string property = null)
        {
            var args = new PropertyChangedEventArgs(property);
            foreach (var item in _handlers)
            {
                for (var i = 0; i < item.Value.Count; i++)
                {
                    try
                    {
                        var handler = item.Value[i];
                        item.Key.Post(o => handler.Invoke(this, args), null);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error in binding: {0}", ex);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void UnsafeInvokePropertyChange(string property)
        {
            var args = new PropertyChangedEventArgs(property);
            foreach (var item in _handlers)
            {
                for (var i = 0; i < item.Value.Count; i++)
                {
                    var handler = item.Value[i];
                    handler.Invoke(this, args);
                }
            }
        }
    }
}
