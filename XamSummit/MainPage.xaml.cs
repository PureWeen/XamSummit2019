using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Xamarin.Forms;

namespace XamSummit
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            ObservableMessages.ItemsSource = Helpers.Messages;

            // Wire up buttons
            LoginClickObs = Observable.FromEventPattern(
                x => Login.Clicked += x,
                x => Login.Clicked -= x)
                .Select(_ => Unit.Default)
                .Do(Helpers.ClearMessages)
                .Publish().RefCount()
                .LogMessage("Login Clicked");

            LogoutClickObs = Observable.FromEventPattern(
                x => Logout.Clicked += x,
                x => Logout.Clicked -= x)
                .Select(_ => false)
                .Publish().RefCount()
                .LogMessage("Logout Clicked");

            CancelClickObs = Observable.FromEventPattern(
                x => Cancel.Clicked += x,
                x => Cancel.Clicked -= x)
                .Select(_ => Unit.Default)
                .Publish().RefCount()
                .LogMessage("Cancel Clicked");

            UserNameObs = Observable.FromEventPattern<TextChangedEventArgs>(
                x => UserName.TextChanged += x,
                x => UserName.TextChanged -= x)
                .Select(x => x.EventArgs.NewTextValue)
                .Publish().RefCount();

            PasswordObs = Observable.FromEventPattern<TextChangedEventArgs>(
                x => Password.TextChanged += x,
                x => Password.TextChanged -= x)
                .Select(x => x.EventArgs.NewTextValue)
                .Publish().RefCount();

            // Fake Web Service Call
            WebRequest = () =>
            {
                var request = Observable.Timer(TimeSpan.FromSeconds(2))
                    .Select(_ =>
                    {
                        if (new Random().Next(0, 3) == 2)
                            throw new Exception("ERROR");

                        return true;
                    });

                return Observable.Defer(() =>
                    {
                        Helpers.AddMessage("Start Web Request");
                        return request;
                    })
                    .LogMessage("WebRequest")
                    .Retry()
                    .TakeUntil(CancelClickObs);
            };


            // !!! Cross the streams !!!
            AuthenticatedObs =
                // Login button clicked
                LoginClickObs
                    // Trigger web request
                    .SelectMany(_ => WebRequest())
                    .Merge(LogoutClickObs)
                    .StartWith(false)
                    .LogMessage("AuthenticatedObs");

            ValidateCreds =
                UserNameObs
                    .CombineLatest(PasswordObs.StartWith(""),
                        (u,p)=>
                        {
                            return u == "Monkey" && p == "Monkey";
                        }
                    )
                    .Throttle(TimeSpan.FromMilliseconds(800))
                    .LogMessage("Validating")
                    .StartWith(false);

            AuthenticatedObs
                .CombineLatest(ValidateCreds, (auth, valid) => new { auth, valid } )
                .Subscribe(result =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        bool isAuthenticated = result.auth;
                        bool isValid = result.valid;

                        Logout.IsVisible = isAuthenticated;
                        Login.IsVisible = !isAuthenticated;
                        Cancel.IsVisible = !isAuthenticated;
                        UserName.IsVisible = !isAuthenticated;
                        Password.IsVisible = !isAuthenticated;

                        Login.IsEnabled = isValid;
                    });
                });
        }


        IObservable<string> UserNameObs { get; set; }
        IObservable<string> PasswordObs { get; set; }

        IObservable<bool> AuthenticatedObs { get; set; }
        IObservable<bool> ValidateCreds { get; set; }
        IObservable<Unit> LoginClickObs { get; set; }
        IObservable<bool> LogoutClickObs { get; set; }
        IObservable<Unit> CancelClickObs { get; set; }
        Func<IObservable<bool>> WebRequest { get; set; }

    }

    
}
