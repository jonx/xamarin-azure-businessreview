using System;
using System.Windows.Input;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Xamarin.Forms;
using Reviewer.SharedModels;
using System.Collections.Generic;

namespace Reviewer.Core
{
    public class AccountViewModel : BaseViewModel
    {
        public ICommand SignInCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand SignOutCommand { get; }

        public event EventHandler SuccessfulSignIn;
        public event EventHandler UnsuccessfulSignIn;

        List<Review> reviews;
        public List<Review> Reviews { get => reviews; set => SetProperty(ref reviews, value); }

        bool loggedIn = false;
        public bool LoggedIn
        {
            get => loggedIn;
            set
            {
                SetProperty(ref loggedIn, value);
                NotLoggedIn = !LoggedIn;
            }
        }

        bool notLoggedIn = true;
        public bool NotLoggedIn { get => notLoggedIn; set => SetProperty(ref notLoggedIn, value); }

        string info;
        public string Info { get => info; set => SetProperty(ref info, value); }

        string notLoggedInInfo = "Sign in to unlock the wonderful world of reviews!";
        string loggedInInfo = "Hiya {user}! Here are your reviews so far!";

        User user = null;

        IMicrosoftAuthService identityService;

        public AccountViewModel()
        {
            Reviews = new List<Review>();

            SignInCommand = new Command(async () => await ExecuteSignInCommand());
            RefreshCommand = new Command(async () => await ExecuteRefreshCommand());
            SignOutCommand = new Command(() => ExecuteSignOutCommand());

            Info = notLoggedInInfo;
            identityService = DependencyService.Get<IMicrosoftAuthService>();

            Task.Run(async () => await CheckLoginStatus());
        }

        void ExecuteSignOutCommand()
        {
            if (IsBusy)
                return;

            if (NotLoggedIn)
                return;

            try
            {
                IsBusy = true;

                identityService.OnSignOutAsync();

                LoggedIn = false;
                Info = notLoggedInInfo;
            }
            finally
            {
                IsBusy = false;
            }
        }

        async Task ExecuteSignInCommand()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                user = await identityService.OnSignInAsync();
            }
            finally
            {
                IsBusy = false;
            }

            if (user == null)
            {
                LoggedIn = false;
                Info = notLoggedInInfo;
                UnsuccessfulSignIn?.Invoke(this, new EventArgs());
            }
            else
            {
                LoggedIn = true;
                Info = loggedInInfo.Replace("{user}", user.DisplayName);

                await ExecuteRefreshCommand();

                SuccessfulSignIn?.Invoke(this, new EventArgs());
            }
        }

        async Task ExecuteRefreshCommand()
        {
            if (IsBusy)
                return;

            if (NotLoggedIn)
                return;

            try
            {
                IsBusy = true;

                var apiService = DependencyService.Get<IAPIService>();
                Reviews = await apiService.GetReviewsForAuthor(user.Id, user.Token);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task CheckLoginStatus()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                var user = await identityService.OnSignInAsync();

                if (user != null)
                {
                    LoggedIn = true;

                    Title = user.DisplayName;
                    Info = loggedInInfo.Replace("{user}", Title);
                }
                else
                {
                    Title = "Account";
                    LoggedIn = false;
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
