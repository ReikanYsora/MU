using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;
using Windows.Storage;

namespace MU.Managers
{
    public class SpeechRecognitionManager : MUManager, IDisposable
    {
        #region ATTRIBUTES
        private const string CORE_NAME = "SpeechRecognitionManager";
        private const string CORE_VERSION = "1.0";
        private const string ICON_NAME = "MU_MANAGER_SR.png";
        private const string LANG = "fr-FR";

        private SpeechRecognizer _speechRecon;

        #region EVENTS
        public event SpeechDetected OnSpeechDetected;
        public delegate void SpeechDetected(object sender, string tagDatas, string textDatas);
        #endregion
        #endregion

        #region CONSTRUCTOR
        public SpeechRecognitionManager()
        {
            Name = CORE_NAME;
            Version = CORE_VERSION;
            IconUri = ICON_NAME;
        }

        public async override Task InitializeAsync()
        {
            try
            {
                _speechRecon = new SpeechRecognizer(new Language(LANG));
                _speechRecon.StateChanged += CBSpeechRecon_StateChanged;
                _speechRecon.ContinuousRecognitionSession.ResultGenerated += CBContinuousRecognitionSession_ResultGenerated;
                
                string fileName = String.Format("Resources\\Grammar\\{0}\\Grammar.xml", LANG);
                StorageFile grammarContentFile = await Package.Current.InstalledLocation.GetFileAsync(fileName);
                
                SpeechRecognitionGrammarFileConstraint grammarConstraint = new SpeechRecognitionGrammarFileConstraint(grammarContentFile);
                _speechRecon.Constraints.Add(grammarConstraint);

                SpeechRecognitionCompilationResult result = await _speechRecon.CompileConstraintsAsync();

                if (result.Status == SpeechRecognitionResultStatus.Success)
                {
                    Status = true;
                    try
                    {
                        await _speechRecon.ContinuousRecognitionSession.StartAsync();
                    }
                    catch (Exception ex)
                    {
                        RaiseOnExceptionOccured(ex);
                    }
                }
                else
                {
                    RaiseOnExceptionOccured(new Exception("Error during SpeechRecognition initialization"));
                }

                await Task.Delay(100);

                RaiseOnFunctionInitialized();
            }
            catch (Exception e)
            {
                RaiseOnExceptionOccured(e);
            }
        }

        public void Dispose()
        {
            if (_speechRecon != null)
            {
                _speechRecon.Dispose();
                _speechRecon = null;
            }
        }
        #endregion
        #region CALLBACKS
        private void CBContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            try
            {
                var results = args.Result.RulePath;
                string resultRules = string.Empty;

                for(int i = 0; i < results.Count; i++)
                {
                    string temp = results[i];
                    if (i == 0)
                    {
                        resultRules = temp;
                    }
                    else
                    {
                        resultRules += "|" + temp;
                    }
                }
                OnSpeechDetected?.Invoke(this, resultRules, args.Result.Text);
            }
            catch (Exception e)
            {
                RaiseOnExceptionOccured(e);
            }
        }

        private void CBSpeechRecon_StateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            try
            {

            }
            catch (Exception e)
            {
                RaiseOnExceptionOccured(e);
            }
        }
        #endregion
    }
}
