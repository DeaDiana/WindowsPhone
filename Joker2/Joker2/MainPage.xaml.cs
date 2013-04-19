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
                            arg = "What do you do if a blond throws a grenade at you? Pull the pin and throw it back.";
                            break;
                        case "NextTongueTwister":
                            templatePth = "TongueTwisterTemplate.xml";
                            arg = "Socks on Knox And Knox in box. Fox in socks On box on Knox.";
                            break;
                        default:
                            templatePth = string.Empty;
                            break;
                    }
                    ReproduceSpeech(templatePth, arg);
                }
            }
        }

        private async void ReproduceSpeech(string templatePth, object currentJoke)
        {
            if (templatePth == string.Empty)
                return;
            string ssml;

            using (var f = App.GetResourceStream(new Uri(templatePth, UriKind.Relative)).Stream)
            using (var reader = new StreamReader(f))
            {
                string template = reader.ReadToEnd();
                ssml = string.Format(template, currentJoke);
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
            using (SpeechRecognizerUI recognizerUI = new SpeechRecognizerUI())
            {
                recognizerUI.Settings.ListenText = "Listening the command...";
                recognizerUI.Settings.ExampleText = "\"Next\"";
                recognizerUI.Settings.ReadoutEnabled = false;
                recognizerUI.Settings.ShowConfirmation = false;

                recognizerUI.Recognizer.Grammars.AddGrammarFromUri("CommandsGrammar", new Uri("ms-appx:///SRGSGrammar1.xml", UriKind.Absolute));
                SpeechRecognitionUIResult result = await recognizerUI.RecognizeWithUIAsync();
                if (result.RecognitionResult.TextConfidence == SpeechRecognitionConfidence.Rejected)
                {
                    using (var speaker = new SpeechSynthesizer())
                    {
                        await speaker.SpeakTextAsync(result.RecognitionResult.Text);
                    }
                    return;
                }
                SemanticProperty genre;

                if (result.RecognitionResult.Semantics.TryGetValue("genre", out genre))
                {
                    string filePath = string.Empty;
                    object arg = null;
                    string displayFormat = string.Empty;

                    switch (genre.Value.ToString())
                    {
                        case "joke":
                            filePath = "JokeTemplate.xml";
                            arg = "smth from joke-list"; //smth from joke-list
                            break;
                        case "tongue-twister":
                            filePath = "TongueTwisterTemplate.xml";
                            arg = "smth from tng-tw-list"; //smth from tng-tw-list
                            break;
                        default:
                            break;
                    }

                    ReproduceSpeech(filePath, arg);
                }
            }
        }
    }
}