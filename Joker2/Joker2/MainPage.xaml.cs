using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Joker2.Resources;
using Windows.Phone.Speech.Recognition;
using Windows.Phone.Speech.Synthesis;
using Windows.Foundation;
using Windows.Phone.Speech.VoiceCommands;

namespace Joker2
{
    public partial class MainPage : PhoneApplicationPage
    {
        public const string VoiceCommandFile = @"JokerCommands.xml";
        List<string> jokeCollection;
        List<string> tongueTwisterCollection;
        bool isRecoEnabled = false;
        IAsyncOperation<SpeechRecognitionResult> recoOperation;
        SpeechRecognizer recognizer;
        int indexForJoke = 0;
        int indexForTwister = 0;
        int NUM_OF_JOKES = 3;
        int NUM_OF_TWISTERS = 7;

        public MainPage()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeVoiceCommands();
        }

        async void InitializeVoiceCommands()
        {
            if (VoiceCommandService.InstalledCommandSets.Any(s => s.Key == "Joker"))
                return;

            try
            {
                await VoiceCommandService.InstallCommandSetsFromFileAsync(new Uri(string.Format("ms-appx:///{0}", VoiceCommandFile)));
            }
            catch (Exception ex)
            {
                return;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (jokeCollection == null)
            {
                jokeCollection = new List<string>();
                jokeCollection.Add("What do you do if a blond throws a grenade at you? Pull the pin and throw it back.");
                jokeCollection.Add("How many body builders does it take to change a light bulb? Nine. One to screw in the bulb while the other 8 hold up the mirrors.");
                jokeCollection.Add("It was in train. Boy's mum has talking and talking with his child. And boy has said: 'Mum, please, advertising'");
                jokeCollection.Add("Teacher: Why are you late? Little Johnny: Because of the sign. Teacher: What sign? Little Johnny: The one that says, 'School Ahead, Go Slow. 'That's what I did.");
                jokeCollection.Add("Little Johnny watched, fascinated, as his mother smoothed cold cream on her face. 'Why do you do that, Mommy?' 'To make myself beautiful,' said his mother, who then began removing the cream with a tissue. 'What's the matter?' asked Little Johnny. 'Giving up?'");
                jokeCollection.Add("John: 'I'm glad you named me John.' Mother: 'Why?' John: 'Because that's what all the kids at school call me.'");
                NUM_OF_JOKES = jokeCollection.Count();

            }
            if (tongueTwisterCollection == null)
            {
                tongueTwisterCollection = new List<string>();
                tongueTwisterCollection.Add("Socks on Knox And Knox in box. Fox in socks On box on Knox.");
                tongueTwisterCollection.Add("Look, sir. Look, sir. Mr. Knox, sir. Let’s do tricks with Bricks and blocks, sir. Let’s do tricks with Chicks and clocks, sir. ");
                tongueTwisterCollection.Add("Chicks with bricks comee. Chicks with blocks come. Chicks with bricks and Blocks and clocks come. ");
                tongueTwisterCollection.Add("First, I’ll make a Quick trick brick stack. Then I’ll make a Quick trick block stack. ");
                tongueTwisterCollection.Add("You can make a Quick trick chick stack. You can make a Quick trick clock stack. And here’s a New trick, Mr. Knox… Socks on chicks And chicks on fox. Fox on clocks On bricks and blocks. Bricks and blocks On Knox on box. ");
                tongueTwisterCollection.Add("Please, sir. I don’t Like this trick, sir. My tongue isn’t Quick or slick, sir. I get all those Ticks and clocks, sir, Mixed up with the Chicks and tocks, sir. I can’t do it, Mr. Fox, sir. I’m so sorry, Mr. Knox, sir. ");
                NUM_OF_TWISTERS = tongueTwisterCollection.Count();
            }
            tongueTwisterCollection.Add("Who sees who sew Whose new socks, sir? You see Sue sew Sue’s new socks, sir.");
            if (recognizer == null)
            {
                recognizer = new SpeechRecognizer();
                recognizer.Grammars.AddGrammarFromUri("CommandsGrammar", new Uri("ms-appx:///SRGSGrammar1.xml", UriKind.Absolute));
            }

            string voiceCommand;
            if (this.NavigationContext.QueryString != null && this.NavigationContext.QueryString.ContainsKey("voiceCommandName"))
            {
                if (NavigationContext.QueryString.TryGetValue("voiceCommandName", out voiceCommand))
                {
                    string templatePth = string.Empty;
                    string arg = string.Empty;
                    switch (voiceCommand)
                    {
                        case "NextJoke":
                            templatePth = "JokeTemplate.xml";
                            arg = jokeCollection.ElementAt(indexForJoke);
                            indexForJoke++;
                            indexForJoke %= NUM_OF_JOKES;
                            break;
                        case "NextTongueTwister":
                            templatePth = "TongueTwisterTemplate.xml";
                            arg = tongueTwisterCollection.ElementAt(indexForTwister);
                            indexForTwister++;
                            indexForTwister %= NUM_OF_TWISTERS;
                            break;
                        default:
                            templatePth = string.Empty;
                            break;
                    }
                    ReproduceSpeech(templatePth, arg);
                }
            }
        }

        private async void ReproduceSpeech(string templatePth, object currentText)
        {
            if (templatePth == string.Empty)
                return;
            string ssml;

            using (var f = App.GetResourceStream(new Uri(templatePth, UriKind.Relative)).Stream)
            using (var reader = new StreamReader(f))
            {
                string template = await reader.ReadToEndAsync();
                ssml = string.Format(template, currentText);
                f.Close();
            }
            using (var synthesizer = new SpeechSynthesizer())
            {
                await synthesizer.SpeakSsmlAsync(ssml);
            }
        }

        private void btnStartRecognition_Click_1(object sender, RoutedEventArgs e)
        {
            RecognizeCommand();
        }

        private async void RecognizeCommand()
        {
            if (this.isRecoEnabled)
            {
                isRecoEnabled = false;
                btnStartRecognition.Content = "I wanna more fun!";
                if (recoOperation != null && recoOperation.Status == AsyncStatus.Started)
                {
                    recoOperation.Cancel();
                }
                return;
            }
            else
            {
                isRecoEnabled = true;
                btnStartRecognition.Content = "Stop joking!";
            }


            using (SpeechRecognizerUI recognizerUI = new SpeechRecognizerUI())
            {
                while (this.isRecoEnabled)
                {
                    try
                    {
                        recoOperation = recognizer.RecognizeAsync();
                        var recoResult = await this.recoOperation;

                        if (recoResult.TextConfidence < SpeechRecognitionConfidence.Medium)
                        {
                            using (var synthesizer = new SpeechSynthesizer())
                            {
                                await synthesizer.SpeakTextAsync("Ha-ha-ha. Say it louder!");
                            }
                        }
                        else
                        {
                            SemanticProperty genre;

                            if (recoResult.Semantics.TryGetValue("genre", out genre))
                            {
                                string filePath = string.Empty;
                                object arg = null;
                                string displayFormat = string.Empty;

                                switch (genre.Value.ToString())
                                {
                                    case "joke":
                                        filePath = "JokeTemplate.xml";
                                        arg = jokeCollection.ElementAt(indexForJoke);
                                        indexForJoke++;
                                        indexForJoke %= NUM_OF_JOKES;
                                        break;
                                    case "tongue-twister":
                                        filePath = "TongueTwisterTemplate.xml";
                                        arg = tongueTwisterCollection.ElementAt(indexForTwister);
                                        indexForTwister++;
                                        indexForTwister %= NUM_OF_TWISTERS;
                                        break;
                                    default:
                                        break;
                                }
                                ReproduceSpeech(filePath, arg);
                            }

                        }
                    }
                    catch (System.Threading.Tasks.TaskCanceledException) { }
                    catch (Exception err)
                    {
                        // Handle the speech privacy policy error.
                        const int privacyPolicyHResult = unchecked((int)0x80045509);
                        if (err.HResult == privacyPolicyHResult)
                        {
                            MessageBox.Show("To run the app, you must first accept the speech privacy policy. To do so, navigate to Settings -> speech on your phone and check 'Enable Speech Recognition Service' ");
                            isRecoEnabled = false;
                            btnStartRecognition.Content = "Start speech recognition";
                        }
                        else
                        {
                            //textOfJoke.Text = "Error: " + err.Message;
                        }
                    }
                }
            }
        }
    }
}