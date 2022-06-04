using Microsoft.ProjectOxford.Common.Contract;
using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using MU.Global.Tools;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;

namespace MU.Analysis
{
    public class VisionAnalysis
    {

        private const string EMOTION_DETECTION_API_KEY = "6aaaec9966de4a6a9d4641c7b5700eb7";
        private EmotionServiceClient _emotionServiceClient;
        private DateTime _lastAPIRequest;

        private const string COMPUTER_VISION_API_KEY = "2daab683e8ab4c4d893811a690557734";
        private VisionServiceClient _computerVision;
        private string _lastAnalysis;
        private const short API_WAITING_DELAY = 2000;

        #region EVENTS
        public event EmotionDetected OnEmotionDetected;
        public delegate void EmotionDetected(object sender, EmotionAnalysis emotionAnalysis);
        public event VisionAnalysed OnVisionAnalysed;
        public delegate void VisionAnalysed(object sender, string description);
        #endregion
        
//                        using (SoftwareBitmap convertedSource = SoftwareBitmap.Convert(previewFrame.SoftwareBitmap, BitmapPixelFormat.Bgra8))
//                        {
//                            displaySource = new WriteableBitmap(convertedSource.PixelWidth, convertedSource.PixelHeight);
//    convertedSource.CopyToBuffer(displaySource.PixelBuffer);
//                        }

//DisplayDetectedFaces(_previewFrameSize, faces);
////SoftwareBitmap sb = previewFrame.SoftwareBitmap;
////SoftwareBitmap softwarebitmapBGRB = SoftwareBitmap.Convert(sb, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
        public VisionAnalysis()
        {
            _emotionServiceClient = new EmotionServiceClient(EMOTION_DETECTION_API_KEY);
            _lastAPIRequest = DateTime.Now;

        }

        private async Task UploadAndAnalyze(SoftwareBitmap softwareBitmap)
        {
            try
            {
                _computerVision = new VisionServiceClient(COMPUTER_VISION_API_KEY, "https://westeurope.api.cognitive.microsoft.com/vision/v1.0");
                var bitmapImageStream = await SoftwareBitmapHelper.GetBitmapStream(softwareBitmap);
                VisualFeature[] visualFeatures = new VisualFeature[] { VisualFeature.Adult, VisualFeature.Categories, VisualFeature.Color, VisualFeature.Description, VisualFeature.Faces, VisualFeature.ImageType, VisualFeature.Tags };
                AnalysisResult analysisResult = await _computerVision.AnalyzeImageAsync(bitmapImageStream, visualFeatures);

                if ((analysisResult != null) && (analysisResult.Description != null) && (analysisResult.Description.Captions != null))
                {
                    if (analysisResult.Description.Captions.Length > 0)
                    {
                        //LastAnalysis = analysisResult.Description.Captions.FirstOrDefault().Text;
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private async Task<Emotion[]> UploadAndDetectEmotions(SoftwareBitmap softwarebitmap)
        {
            try
            {
                Emotion[] emotionResult;
                var bitmapImageStream = await SoftwareBitmapHelper.GetBitmapStream(softwarebitmap);
                emotionResult = await _emotionServiceClient.RecognizeAsync(bitmapImageStream);

                if (emotionResult.Length > 0)
                {
                    Emotion e = emotionResult.FirstOrDefault();

                    if (e != null)
                    {
                        OnEmotionDetected?.Invoke(this, new EmotionAnalysis(e));
                    }
                }
                return emotionResult;
            }
            catch (Exception e)
            {
                return null;
            }
        }

    }
}

public class EmotionAnalysis
{
    public float Anger { private set; get; }
    public float Fear { private set; get; }
    public float Happiness { private set; get; }
    public float Neutral { private set; get; }
    public float Sadness { private set; get; }
    public float Surprise { private set; get; }

    public EmotionAnalysis(Emotion emotion)
    {
        Anger = emotion.Scores.Anger;
        Fear = emotion.Scores.Fear;
        Happiness = emotion.Scores.Happiness;
        Neutral = emotion.Scores.Neutral;
        Sadness = emotion.Scores.Sadness;
        Surprise = emotion.Scores.Surprise;
    }

    public string GetFirstEmotion()
    {
        float result = 0.0f;
        string resultValue = "None";

        if (Anger > result)
        {
            result = Anger;
            resultValue = "Anger";
        }

        if (Fear > result)
        {
            result = Fear;
            resultValue = "Fear";
        }

        if (Happiness > result)
        {
            result = Happiness;
            resultValue = "Happiness";
        }

        if (Neutral > result)
        {
            result = Neutral;
            resultValue = "Neutral";
        }

        if (Sadness > result)
        {
            result = Sadness;
            resultValue = "Sadness";
        }

        if (Surprise > result)
        {
            result = Surprise;
            resultValue = "Surprise";
        }

        return resultValue;
    }
}
