﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using iHentai.Paging;
using iHentai.Shared.Helpers;
using NavigatedEventHandler = Windows.UI.Xaml.Navigation.NavigatedEventHandler;

namespace iHentai.Mvvm
{
    public interface IMvvmView<T> where T : ViewModel
    {
        T ViewModel { get; set; }
    }

    public static class NavigationService
    {

        static NavigationService()
        {
            KnownViews = Application.Current.GetType().GetTypeInfo().Assembly.DefinedTypes
                .Select(item =>
                    item.IsClass &&
                    ReflectionHelper.ImplementsGenericDefinition(item, typeof(IMvvmView<>), out var res)
                        ? new { ViewType = item, GenericType = res.GetGenericArguments()[0] }
                        : null).Where(item => item != null)
                .ToDictionary(item => item.GenericType, item => item.ViewType);
        }

        public static Dictionary<Type, TypeInfo> KnownViews { get; }

        private static HentaiFrame _frame;

        public static HentaiFrame Frame
        {
            get
            {
                if (_frame == null)
                {
                    _frame = Window.Current.Content as HentaiFrame;
                    RegisterFrameEvents();
                }

                return _frame;
            }

            set
            {
                UnregisterFrameEvents();
                _frame = value;
                RegisterFrameEvents();
            }
        }

        public static bool CanGoBack => Frame.CanGoBack;

        public static bool CanGoForward => Frame.CanGoForward;
        public static event EventHandler<HentaiNavigationEventArgs> Navigated;

        //public static event NavigationFailedEventHandler NavigationFailed;

        public static void GoBack()
        {
            Frame.GoBackAsync();
        }

        public static void GoForward()
        {
            Frame.GoForwardAsync();
        }

        public static async Task<bool> Navigate(Type pageType, object parameter = null,
            NavigationTransitionInfo infoOverride = null)
        {
            // Don't open the same page multiple times
            return Frame.Content?.GetType() != pageType && await Frame.NavigateAsync(pageType, parameter);
        }

        public static Task<bool> Navigate<T>(object parameter = null, NavigationTransitionInfo infoOverride = null)
            where T : Page
        {
            return Navigate(typeof(T), parameter, infoOverride);
        }

        public static Task<bool> NavigateViewModel<T>(params object[] args)
            where T : class
        {
            return NavigateViewModel(typeof(T), args);
        }

        public static Task<bool> NavigateViewModel(Type vmType, params object[] args)
        {
            var pInfo = vmType.GetTypeInfo();
            var uwpPage = typeof(Page).GetTypeInfo();
            if (pInfo.IsSubclassOf(typeof(ViewModel)) && KnownViews.TryGetValue(vmType, out var pageInfo))
            {
                var vm = Activator.CreateInstance(vmType, args) as ViewModel;
                return Navigate(pageInfo, vm);
            }
            if (pInfo.IsAssignableFrom(uwpPage) || pInfo.IsSubclassOf(typeof(Page)))
                return Navigate(vmType);
            throw new ArgumentException("Page Type must be based on Xamarin.Forms.Page");
        }

        private static void RegisterFrameEvents()
        {
            if (_frame != null)
            {
                _frame.Navigated += Frame_Navigated;
                //_frame.NavigationFailed += Frame_NavigationFailed;
            }
        }

        private static void UnregisterFrameEvents()
        {
            if (_frame != null)
            {
                _frame.Navigated -= Frame_Navigated;
                //_frame.NavigationFailed -= Frame_NavigationFailed;
            }
        }

        //private static void Frame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        //{
        //    NavigationFailed?.Invoke(sender, e);
        //}

        private static void Frame_Navigated(object sender, HentaiNavigationEventArgs e)
        {
            Navigated?.Invoke(sender, e);
        }
    }
}