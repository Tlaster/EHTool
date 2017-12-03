using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace iHentai.Paging.Animations
{
    public class PushPageAnimation : IPageAnimation
    {
        public PushPageAnimation()
        {
            Duration = TimeSpan.FromSeconds(0.15);
        }

        public TimeSpan Duration { get; set; }

        public PageInsertionMode PageInsertionMode => PageInsertionMode.ConcurrentAbove;

        public Task AnimateForwardNavigatingFromAsync(FrameworkElement previousPage, FrameworkElement nextPage)
        {
            var width = previousPage.ActualWidth;
            return AnimateAsync(previousPage, nextPage, 0, -(width / 2.0), width, width / 2.0);
        }

        public Task AnimateForwardNavigatedToAsync(FrameworkElement previousPage, FrameworkElement nextPage)
        {
            var width = previousPage.ActualWidth;
            return AnimateAsync(previousPage, nextPage, -(width / 2.0), -width, width / 2, 0);
        }

        public Task AnimateBackwardNavigatingFromAsync(FrameworkElement previousPage, FrameworkElement nextPage)
        {
            var width = previousPage.ActualWidth;
            return AnimateAsync(previousPage, nextPage, 1, width / 2.0, -width, -(width / 2.0));
        }

        public Task AnimateBackwardNavigatedToAsync(FrameworkElement previousPage, FrameworkElement nextPage)
        {
            var width = previousPage.ActualWidth;
            return AnimateAsync(previousPage, nextPage, width / 2.0, width, -(width / 2.0), 0);
        }

        private Task AnimateAsync(FrameworkElement page, FrameworkElement otherPage, double fromOffsetPreviousPage,
            double toOffsetPreviousPage, double fromOffsetNextPage, double toOffsetNextPage)
        {
            if (page == null)
                return Task.FromResult<object>(null);

            page.Projection = new PlaneProjection();
            otherPage.Projection = new PlaneProjection();

            var story = new Storyboard();

            var animation1 = new DoubleAnimation
            {
                Duration = Duration,
                From = fromOffsetPreviousPage,
                To = toOffsetPreviousPage
            };
            Storyboard.SetTargetProperty(animation1, "(UIElement.Projection).(PlaneProjection.GlobalOffsetX)");
            Storyboard.SetTarget(animation1, page);

            var animation2 = new DoubleAnimation
            {
                Duration = Duration,
                From = fromOffsetNextPage,
                To = toOffsetNextPage
            };
            Storyboard.SetTargetProperty(animation2, "(UIElement.Projection).(PlaneProjection.GlobalOffsetX)");
            Storyboard.SetTarget(animation2, otherPage);

            story.Children.Add(animation1);
            story.Children.Add(animation2);

            var completion = new TaskCompletionSource<object>();
            story.Completed += delegate { completion.SetResult(null); };
            story.Begin();
            return completion.Task;
        }
    }
}