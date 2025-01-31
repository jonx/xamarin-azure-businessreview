﻿using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Reviewer.Services;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Reviewer.Core
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MonkeyCache.FileStore.Barrel.ApplicationId = "buildreviewer";

            DependencyService.Register<IStorageService, StorageService>();
            DependencyService.Register<IDataService, CosmosDataService>();
            DependencyService.Register<IMicrosoftAuthService, MicrosoftAuthService>();
            DependencyService.Register<IAPIService, WebAPIService>();

            // Initialize the Public Client Application for Microssoft Authentication.
            DependencyService.Get<IMicrosoftAuthService>().Initialize();

            var tabbedPage = new TabbedPage();

            tabbedPage.Children.Add(new NavigationPage(new BusinessListPage()) { Title = "Reviews" });
            tabbedPage.Children.Add(new NavigationPage(new AccountPage()) { Title = "Me" });

            MainPage = tabbedPage;
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        protected override void OnSleep()
        {
            base.OnSleep();
        }

        protected override void OnResume()
        {
            base.OnResume();
        }
    }
}
