using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using VoiceCommander.Resources;

using System.Windows.Media;
using Windows.Foundation;
using Windows.Phone.Speech.Recognition;
using Windows.Phone.Speech.Synthesis;

namespace VoiceCommander
{
    public partial class MainPage : PhoneApplicationPage
    {
        SpeechSynthesizer speaker;
        SpeechRecognizer recognizer;
        IAsyncOperation<SpeechRecognitionResult> recoOperation;

        bool isRecoEnabled = false;
        Dictionary<string, string> jokeCollection; 

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
             if (jokeCollection == null)
             {
                 jokeCollection = new Dictionary<string, string>();
                 jokeCollection.Add("first", "text number 1");
                 jokeCollection.Add("second", "text number 2");
                 jokeCollection.Add("third", "text number 3");
                 jokeCollection.Add("fourth", "text number 4");
                 jokeCollection.Add("fiveth", "text number 5");
                 jokeCollection.Add("sixth", "text number 6");
             }

            try
            {
                // Create the speech recognizer and speech synthesizer objects. 
                if (speaker == null)
                {
                    speaker = new SpeechSynthesizer();
                }
                if (recognizer == null)
                {
                    recognizer = new SpeechRecognizer();

                    // Set up a list of colors to recognize.
                    recognizer.Grammars.AddGrammarFromList("JokeNumbers", jokeCollection.Keys);
                }
                
            }
            catch (Exception err)
            {
                //txtResult.Text = err.ToString();
            }

            base.OnNavigatedTo(e);

        }

        private async void btnStartRecognition_Click(object sender, RoutedEventArgs e)
        {
            if (this.isRecoEnabled)
            {
                // Update the UI to the initial state
                isRecoEnabled = false;
                btnStartRecognition.Content = "I wanna fun!";
                nameOfJoke.Text = String.Empty;
                textOfJoke.Visibility = System.Windows.Visibility.Collapsed;

                // Cancel the outstanding recognition operation, if one exists
                if (recoOperation != null && recoOperation.Status == AsyncStatus.Started)
                {
                    recoOperation.Cancel();
                }
                return;
            }
            else
            {
                // Set the flag to say that we are in recognition mode
                isRecoEnabled = true;

                // Update the UI
                btnStartRecognition.Content = "Speak please!";
                //txtInstructions.Visibility = System.Windows.Visibility.Visible;
            }

            while (this.isRecoEnabled)
            {
                try
                {
                    // Perform speech recognition.  
                    recoOperation = recognizer.RecognizeAsync();
                    var recoResult = await this.recoOperation;

                    // Check the confidence level of the speech recognition attempt.
                    if (recoResult.TextConfidence < SpeechRecognitionConfidence.Medium)
                    {
                        // If the confidence level of the speech recognition attempt is low, 
                        // ask the user to try again.
                        nameOfJoke.Text = "Stop muttering!";
                        await speaker.SpeakTextAsync("Ha-ha-ha. Say it louder!");
                    }
                    else
                    {
                        // Output that the color of the rectangle is changing by updating
                        // the TextBox control and by using text-to-speech (TTS). 
                        nameOfJoke.Text = "Searching the joke ..." + recoResult.Text;
                        await speaker.SpeakTextAsync("The " + recoResult.Text + "joke");

                        // Set the fill color of the rectangle to the recognized color. 

                        textOfJoke.Text = "this is very cool joke"; //jokeCollection.First;// recoResult.Text.ToLower);

                    }
                }
                catch (System.Threading.Tasks.TaskCanceledException)
                {
                    // Ignore the cancellation exception of the recoOperation.
                    // When recoOperation.Cancel() is called to cancel the asynchronous speech recognition operation
                    // initiated by RecognizeAsync(),  a TaskCanceledException is thrown to signify early exit.
                }
                catch (Exception err)
                {
                    // Handle the speech privacy policy error.
                    const int privacyPolicyHResult = unchecked((int)0x80045509);

                    if (err.HResult == privacyPolicyHResult)
                    {
                        MessageBox.Show("To run this sample, you must first accept the speech privacy policy. To do so, navigate to Settings -> speech on your phone and check 'Enable Speech Recognition Service' ");
                        isRecoEnabled = false;
                        btnStartRecognition.Content = "Start speech recognition";
                    }
                    else
                    {
                        textOfJoke.Text = "Error: " + err.Message;
                    }
                }
            }
        }
        
        /*private async void btnStartRecognition_Click(object sender, RoutedEvent e)
        {
            if (this.isRecoEnabled)
            {
                // Update the UI to the initial state
                isRecoEnabled = false;
                btnStartRecognition.Content = "I wanna fun!";
                nameOfJoke.Text = String.Empty;
                textOfJoke.Visibility = System.Windows.Visibility.Collapsed;

                // Cancel the outstanding recognition operation, if one exists
                if (recoOperation != null && recoOperation.Status == AsyncStatus.Started)
                {
                    recoOperation.Cancel();
                }
                return;
            }
            else
            {
                // Set the flag to say that we are in recognition mode
                isRecoEnabled = true;

                // Update the UI
                btnStartRecognition.Content = "Speak please!";
                //txtInstructions.Visibility = System.Windows.Visibility.Visible;
            }

            while (this.isRecoEnabled)
            {
                try
                {
                    // Perform speech recognition.  
                    recoOperation = recognizer.RecognizeAsync();
                    var recoResult = await this.recoOperation;

                    // Check the confidence level of the speech recognition attempt.
                    if (recoResult.TextConfidence < SpeechRecognitionConfidence.Medium)
                    {
                        // If the confidence level of the speech recognition attempt is low, 
                        // ask the user to try again.
                        nameOfJoke.Text = "Stop muttering!";
                        await speaker.SpeakTextAsync("Ha-ha-ha. Say it louder!");
                    }
                    else
                    {
                        // Output that the color of the rectangle is changing by updating
                        // the TextBox control and by using text-to-speech (TTS). 
                        nameOfJoke.Text = "Searching the joke ..." + recoResult.Text;
                        await speaker.SpeakTextAsync("The " + recoResult.Text + "joke");

                        // Set the fill color of the rectangle to the recognized color. 

                        textOfJoke.Text = "this is very cool joke"; //jokeCollection.First;// recoResult.Text.ToLower);

                    }
                }
                catch (System.Threading.Tasks.TaskCanceledException)
                {
                    // Ignore the cancellation exception of the recoOperation.
                    // When recoOperation.Cancel() is called to cancel the asynchronous speech recognition operation
                    // initiated by RecognizeAsync(),  a TaskCanceledException is thrown to signify early exit.
                }
                catch (Exception err)
                {
                    // Handle the speech privacy policy error.
                    const int privacyPolicyHResult = unchecked((int)0x80045509);

                    if (err.HResult == privacyPolicyHResult)
                    {
                        MessageBox.Show("To run this sample, you must first accept the speech privacy policy. To do so, navigate to Settings -> speech on your phone and check 'Enable Speech Recognition Service' ");
                        isRecoEnabled = false;
                        btnStartRecognition.Content = "Start speech recognition";
                    }
                    else
                    {
                        textOfJoke.Text = "Error: " + err.Message;
                    }
                }
            }
        }*/
    }
}