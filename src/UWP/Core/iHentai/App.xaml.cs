﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Conet.Apis.Core;
using Conet.Pages;
using Conet.ViewModels;
using iHentai.Apis.Core;
using iHentai.Basic.Extensions;
using iHentai.Core.Pages;
using iHentai.Core.ViewModels;
using iHentai.Mvvm;
using iHentai.Services;
using iHentai.ViewModels;
using iHentai.Views;

namespace iHentai
{
    public sealed partial class App : Application, IMvvmApplication, IApiApplication, IMultiContentApplication
    {
        private readonly Lazy<ActivationService> _activationService;
        private readonly bool _enableHentaiMode = "enableHentaiMode".Read(false);

        public App()
        {
            InitializeComponent();
            // Deferred execution until used. Check https://msdn.microsoft.com/library/dd642331(v=vs.110).aspx for further info on Lazy<T> class.
            _activationService = new Lazy<ActivationService>(CreateActivationService);
        }

        private ActivationService ActivationService => _activationService.Value;

        public IEnumerable<Assembly> GetApiAssemblies()
        {
            if (_enableHentaiMode)
                yield return typeof(IHentaiApi).GetTypeInfo().Assembly;
            yield return typeof(IConetApi).GetTypeInfo().Assembly;
        }

        public IEnumerable<(Assembly ApiAssembly, Assembly ViewAssembly)> GetEntries()
        {
            if (_enableHentaiMode)
                yield return (typeof(IHentaiApi).GetTypeInfo().Assembly, typeof(GalleryViewModel).GetTypeInfo().Assembly);
            yield return (typeof(IConetApi).GetTypeInfo().Assembly, typeof(TimelineViewModel).GetTypeInfo().Assembly);
        }

        public IEnumerable<Assembly> GetContentViewAssemblies()
        {
            if (_enableHentaiMode)
                yield return typeof(IHentaiApi).GetTypeInfo().Assembly;
            yield return typeof(IConetApi).GetTypeInfo().Assembly;
        }

        public IEnumerable<Assembly> MvvmViewAssemblies()
        {
            if (_enableHentaiMode)
                yield return typeof(GalleryViewModel).GetTypeInfo().Assembly;
            yield return typeof(TimelineViewModel).GetTypeInfo().Assembly;
            yield return typeof(SettingsViewModel).GetTypeInfo().Assembly;
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (!args.PrelaunchActivated)
                await ActivationService.ActivateAsync(args);
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            await ActivationService.ActivateAsync(args);
        }

        private ActivationService CreateActivationService()
        {
            return new ActivationService(this, typeof(ServiceSelectionPage));
        }

        protected override async void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            await ActivationService.ActivateAsync(args);
        }
    }
}