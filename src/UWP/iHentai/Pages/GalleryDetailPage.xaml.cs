﻿using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using iHentai.Mvvm;
using iHentai.Paging;
using iHentai.ViewModels;
using iHentai.Views;
using Microsoft.Toolkit.Uwp.UI.Extensions;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace iHentai.Pages
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GalleryDetailPage : IMvvmView<GalleryDetailViewModel>
    {
        public GalleryDetailPage()
        {
            InitializeComponent();
        }

        public new GalleryDetailViewModel ViewModel
        {
            get => (GalleryDetailViewModel) base.ViewModel;
            set => base.ViewModel = value;
        }

        protected override void OnStart()
        {
            base.OnStart();
            if (ViewModel.Model.ThumbHeight > 0d && ViewModel.Model.ThumbWidth > 0d)
            {
                var reqHeight = ViewModel.Model.ThumbHeight / ViewModel.Model.ThumbWidth * 300d;
                var reqWidth = ViewModel.Model.ThumbWidth / ViewModel.Model.ThumbHeight * 300d;
                var itemHeight = Math.Min(reqHeight, 300d);
                var itemWidth = Math.Min(reqWidth, 300d);
                ThumbImage.Width = itemWidth;
                ThumbImage.Height = itemHeight;
            }
            ConnectedAnimationService.GetForCurrentView().GetAnimation("detail_image")?.TryStart(ThumbImage, new []{ GalleryInfoContainer });
            ConnectedAnimationService.GetForCurrentView().GetAnimation("detail_title")?.TryStart(TitleTextBlock);
        }

        protected override void OnClose()
        {
            base.OnClose();
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("detail_image", ThumbImage);
        }
        
    }
}