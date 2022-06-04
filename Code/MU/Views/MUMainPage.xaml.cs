using MU.Managers;
using MU.ViewModel;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace MU.Views
{
    public sealed partial class MUMainPage : Page
    {
        #region PROPERTIES
        private List<MUToastDatas> _toastsToDisplay;
        private MUContext _context;
        private bool _notificationOnProgress;
        #endregion

        public MUMainPage()
        {
            InitializeComponent();

            _toastsToDisplay = new List<MUToastDatas>();
            _notificationOnProgress = false;

             _context = new MUContext(this);
            DataContext = _context;

            InitializeContextAsync();
        }

        private void CheckAndDisplayNotification()
        {
            if ((_toastsToDisplay.Count > 0) && (!_notificationOnProgress))
            {
                _notificationOnProgress = true;
                MUToastDatas tempToast = _toastsToDisplay[0];
                _toastsToDisplay.Remove(tempToast);
                toastTitle.Text = tempToast.Title;
                toastContent.Text = tempToast.Message;
                toastIcon.Source = new BitmapImage(tempToast.IconUri);
                EnterStoryboard.Begin();

                _context.LEDScroll();
            }
        }

        private async void InitializeContextAsync()
        {
            await _context.InitializeContextASync();
            await _context.CameraManager.StartPreviewAsync(cnvVision, previewElement);
        }

        public void ShowToast(string title, string message, Uri iconUri)
        {
            _toastsToDisplay.Add(new MUToastDatas
            {
                Title = title,
                Message = message,
                IconUri = iconUri
            });

            CheckAndDisplayNotification();
        }

        private void CBStoryBoardAnimation_Completed(object sender, object e)
        {
            _context.LEDNormal();
            _notificationOnProgress = false;

            CheckAndDisplayNotification();
        }

        public void DisplayMainPanel()
        {
            grdMainPanel.Visibility = Visibility.Visible;
        }

        public void HideMainPanel()
        {
            grdMainPanel.Visibility = Visibility.Collapsed;
        }

        public void DisplayMainFacePosition(FaceOffset mainFacePosition)
        {
            txtMainFacePosition.Text = mainFacePosition.ToString();
        }
    }
}
