﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using AngleSharp.Common;
using iHentai.Common.Tab;
using iHentai.Services.EHentai.Model;
using iHentai.ViewModels.EHentai;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI;
using Microsoft.Toolkit.Uwp.UI.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace iHentai.Activities.EHentai
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    internal partial class GalleryDetailActivity
    {
        public GalleryDetailActivity()
        {
            InitializeComponent();
        }

        public override ITabViewModel TabViewModel => ViewModel;
        private GalleryDetailViewModel ViewModel { get; set; }

        protected override void OnCreate(object parameter)
        {
            base.OnCreate(parameter);
            if (parameter is EHGallery gallery)
            {
                ViewModel = new GalleryDetailViewModel(gallery);
            }
        }

        protected override void OnUsingConnectedAnimation(ConnectedAnimationService service)
        {
            service.GetAnimation("image")?.TryStart(DetailImage);
        }

        protected override void OnPrepareConnectedAnimation(ConnectedAnimationService service)
        {
            service.PrepareToAnimate("image", DetailImage).Configuration = new DirectConnectedAnimationConfiguration();
        }
    }


}