using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
#if WINDOWS_UWP
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
#elif WINDOWS_WPF
using System.Windows;
using System.Windows.Threading;
#endif

namespace DSharpPlus.Entities
{
    public class PropertyChangedBase : INotifyPropertyChanged
    {
#if WINDOWS_UWP
        public static CoreDispatcher Dispatcher => _dispatcherLazy.Value;
        private static Lazy<CoreDispatcher> _dispatcherLazy = new Lazy<CoreDispatcher>(() => CoreApplication.MainView.CoreWindow.Dispatcher, true);
#elif WINDOWS_WPF
        public static Dispatcher Dispatcher => _dispatcherLazy.Value;
        private static Lazy<Dispatcher> _dispatcherLazy = new Lazy<Dispatcher>(() => Application.Current.Dispatcher, true);
#endif

        private int _handlers;
        private bool _hasHandlers;
        private event PropertyChangedEventHandler _propertyChanged;
        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                _handlers++;
                _hasHandlers = true;
                _propertyChanged += value;
            }

            remove
            {
                _handlers--;
                _hasHandlers = _handlers > 0;
                _propertyChanged -= value;
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
        internal void InvokePropertyChanged([CallerMemberName] string property = null)
        {
            if (_hasHandlers)
            {
#if WINDOWS_UWP
                Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(property))).AsTask().Wait();
#elif WINDOWS_WPF
                Dispatcher.Invoke(() => _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(property)));
#endif
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void InternalInvokePropertyChanged(string property)
        {
            if (_hasHandlers)
            {
                _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
