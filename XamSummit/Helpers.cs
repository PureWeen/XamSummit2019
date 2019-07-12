using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Xamarin.Forms;

namespace XamSummit
{
    public static class Helpers
    {
        public static ObservableCollection<string> Messages = new ObservableCollection<string>();
        public static IObservable<T> LogMessage<T>(this IObservable<T> obs, string message)
        {
            return obs.Do(next =>
            {
                AddMessage($"OnNext: {message} - {next}");
            },
            exception =>
            {
                AddMessage($"Exception: {message} - {exception?.Message}");
            },
            () =>
            {
                AddMessage($"Completed: {message}");
            });
        }

        public static void AddMessage(string message)
        {
            Device.BeginInvokeOnMainThread(() => Messages.Insert(0, message));
        }

        public static void ClearMessages(Unit unit)
        {
            Device.BeginInvokeOnMainThread(() => Messages.Clear());
        }

        public static void DisposeWith(this IDisposable disposable, CompositeDisposable cd)
        {
            cd.Add(disposable);
        }


    }
}
