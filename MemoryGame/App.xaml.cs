using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ViewManagement;

namespace MemoryGame
{
    /// <summary>
    /// Stellt das anwendungsspezifische Verhalten bereit, um das Standardanwendungsverhalten zu ergänzen.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initialisiert das Singleton-Anwendungsobjekt. Dies ist die erste Zeile des erstellten
        /// ausgeführten Codes und daher das logische Äquivalent von main() oder WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            Windows.UI.Xaml.ElementSoundPlayer.State = ElementSoundPlayerState.On;
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Anwendung normal durch den Endbenutzer gestartet wird.
        /// Weitere Einstiegspunkte werden verwendet, wenn die Anwendung gestartet wird, um eine bestimmte Datei zu öffnen,
        /// um Suchergebnisse anzuzeigen usw.
        /// </summary>
        /// <param name="e">Details über Startanforderung und -prozess.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Xbox: Mauszeiger deaktivieren und Gamepad-Navigation erzwingen
            this.RequiresPointerMode = ApplicationRequiresPointerMode.WhenRequested;

            // Xbox: Safe Area verwenden, um TV-Overscan zu vermeiden
            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseVisible);

            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Zustand von zuvor angehaltener Anwendung laden
                }

                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Navigation zu einer bestimmten Seite fehlschlägt.
        /// </summary>
        /// <param name="sender">Der Rahmen, bei dem die Navigation fehlgeschlagen ist.</param>
        /// <param name="e">Details über den Navigationsfehler.</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Anwendungssitzung angehalten wird.
        /// </summary>
        /// <param name="sender">Die Quelle der Anhalteanforderung.</param>
        /// <param name="e">Details zur Anhalteanforderung.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Anwendungszustand speichern und alle Hintergrundaktivitäten beenden
            deferral.Complete();
        }
    }
}
