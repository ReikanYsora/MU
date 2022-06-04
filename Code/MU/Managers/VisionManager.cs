using MU.Global.Tools;
using MU.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.FaceAnalysis;
using Windows.Media.MediaProperties;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace MU.Managers
{
    public class VisionManager : MUManager, IDisposable
    {
        #region ATTRIBUTES
        public const string CORE_NAME = "VisionManager";
        public const string CORE_VERSION = "1.0";
        public const string ICON_NAME = "MU_MANAGER_VISION.png";

        private readonly SolidColorBrush _lineBrush = new SolidColorBrush(Colors.OrangeRed);
        private readonly SolidColorBrush _fillBrush = new SolidColorBrush(Colors.Transparent);
        private readonly double _lineThickness = 2.0;
        private const BitmapPixelFormat INPUT_PIXEL_FORMAT = BitmapPixelFormat.Nv12;
        private CaptureElement _previewFrame;
        private Canvas _cnvVision;

        private MediaCapture _mediaCapture;
        private VideoEncodingProperties _videoProperties;
        private FaceTracker _faceTracker;
        private SemaphoreSlim _frameProcessingSemaphore = new SemaphoreSlim(1);
        private ThreadPoolTimer _frameProcessingTimer;

        #region EVENTS
        public event FaceDetected OnFaceDetected;
        public delegate void FaceDetected(object sender, IList<DetectedFace> faces, FaceOffset mainFacePosition);
        #endregion
        #endregion

        #region CONSTRUCTOR
        public VisionManager()
        {
            Name = CORE_NAME;
            Version = CORE_VERSION;
            IconUri = ICON_NAME;
        }
        #endregion

        #region METHODS
        public async override Task InitializeAsync()
        {
            try
            {
                if (_faceTracker == null)
                {
                    _faceTracker = await FaceTracker.CreateAsync();
                }
                
                _mediaCapture = new MediaCapture();

                MediaCaptureInitializationSettings settings = new MediaCaptureInitializationSettings();
                settings.MediaCategory = MediaCategory.Communications;
                
                settings.StreamingCaptureMode = StreamingCaptureMode.Video;
                await _mediaCapture.InitializeAsync(settings);

                var deviceController = _mediaCapture.VideoDeviceController;
                _videoProperties = deviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;
                
                RaiseOnFunctionInitialized();
            }
            catch (Exception e)
            {
                RaiseOnExceptionOccured(e);
            }
        }

        public async Task StartPreviewAsync(Canvas cnvVision, CaptureElement previewFrame)
        {
            if ((previewFrame != null) && (_mediaCapture != null))
            {
                if (cnvVision != null)
                {
                    _cnvVision = cnvVision;
                }

                _previewFrame = previewFrame;
                previewFrame.Source = _mediaCapture;
                await _mediaCapture.StartPreviewAsync();
                
                TimeSpan timerInterval = TimeSpan.FromMilliseconds(50);
                _frameProcessingTimer = ThreadPoolTimer.CreatePeriodicTimer(new TimerElapsedHandler(ProcessCurrentVideoFrame), timerInterval);
            }
        }

        private async void ProcessCurrentVideoFrame(ThreadPoolTimer timer)
        {
            if (!_frameProcessingSemaphore.Wait(0))
            {
                return;
            }

            try
            {   
                using (VideoFrame previewFrame = new VideoFrame(INPUT_PIXEL_FORMAT, (int) _videoProperties.Width, (int) _videoProperties.Height))
                {
                    await _mediaCapture.GetPreviewFrameAsync(previewFrame);
                    IList<DetectedFace> faces = null;

                    if (FaceDetector.IsBitmapPixelFormatSupported(previewFrame.SoftwareBitmap.BitmapPixelFormat))
                    {
                        faces = await _faceTracker.ProcessNextFrameAsync(previewFrame);
                    }
                    
                    var _previewFrameSize = new Size(previewFrame.SoftwareBitmap.PixelWidth, previewFrame.SoftwareBitmap.PixelHeight);
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                    () =>
                    {
                        DisplayDetectedFaces(_previewFrameSize, faces);
                    });
                }
            }
            catch (Exception e)
            {
                RaiseOnExceptionOccured(e);
            }
            finally
            {
                _frameProcessingSemaphore.Release();
            }
        }

        private void DisplayDetectedFaces(Size framePizelSize, IList<DetectedFace> foundFaces)
        {
            _cnvVision.Children.Clear();

            double actualWidth = _cnvVision.ActualWidth;
            double actualHeight = _cnvVision.ActualHeight;

            if (foundFaces != null)
            {
                if ((actualWidth != 0) && (actualHeight != 0))
                {
                    double widthScale = framePizelSize.Width / actualWidth;
                    double heightScale = framePizelSize.Height / actualHeight;

                    foreach (DetectedFace face in foundFaces)
                    {
                        Rectangle box = new Rectangle();
                        box.Width = (uint)(face.FaceBox.Width / widthScale);
                        box.Height = (uint)(face.FaceBox.Height / heightScale);
                        box.Fill = _fillBrush;
                        box.Stroke = _lineBrush;
                        box.StrokeThickness = _lineThickness;
                        box.Margin = new Thickness((uint)(face.FaceBox.X / widthScale), (uint)(face.FaceBox.Y / heightScale), 0, 0);
                        _cnvVision.Children.Add(box);
                    }
                }
                else
                {
                    DetectedFace mainFace = foundFaces.OrderByDescending(x => x.FaceBox.Width).ThenByDescending(x => x.FaceBox.Height).FirstOrDefault();

                    FaceOffset facePosition = new FaceOffset();
                    if (mainFace != null)
                    {
                        double pointFaceCenterX = mainFace.FaceBox.X + (mainFace.FaceBox.Width / 2.0);
                        double pointFaceCenterY = mainFace.FaceBox.Y + (mainFace.FaceBox.Height / 2.0);
                        double pointCenterX = framePizelSize.Width / 2.0;
                        double pointCenterY = framePizelSize.Height / 2.0;

                        facePosition.X = (pointFaceCenterX - pointCenterX) / pointCenterX;
                        facePosition.Y = (pointFaceCenterY - pointCenterY) / pointCenterY;
                    }

                    OnFaceDetected?.Invoke(this, foundFaces, facePosition);
                }
            }
        }

        public void Dispose()
        {
            try
            {
                if (Status)
                {
                    Status = false;

                    if (_frameProcessingTimer != null)
                    {
                        _frameProcessingTimer.Cancel();
                    }

                    if (_mediaCapture != null)
                    {
                        _mediaCapture.Dispose();
                    }

                    _frameProcessingTimer = null;
                    _previewFrame.Source = null;
                    _mediaCapture = null;
                }
            }
            catch (Exception e)
            {
                RaiseOnExceptionOccured(e);
            }
        }
        #endregion
    }

    public class FaceOffset
    {
        public double X { set; get; }
        public double Y { set; get; }

        public override string ToString()
        {
            return "X : " + X + ", Y : " + Y; 
        }
    }
}

