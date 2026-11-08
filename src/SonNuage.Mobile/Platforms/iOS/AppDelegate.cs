using AVFoundation;
using Foundation;
using SonNuage.Mobile.Services;
using UIKit;

namespace SonNuage.Mobile;

[Register("AppDelegate")]
public sealed class AppDelegate : MauiUIApplicationDelegate
{
    public override bool FinishedLaunching(UIApplication application, NSDictionary? launchOptions)
    {
        // Avant tout le reste : un échec de démarrage doit laisser une trace.
        CrashLog.Install();
        ActivateAudioSession();
        return base.FinishedLaunching(application, launchOptions);
    }

    protected override MauiApp CreateMauiApp()
    {
        try
        {
            return MauiProgram.CreateMauiApp();
        }
        catch (Exception ex)
        {
            CrashLog.Write("Démarrage", ex);
            return MauiProgram.CreateCrashApp();
        }
    }

    /// <summary>
    /// Catégorie « playback » : la musique continue écran verrouillé ou appli en arrière-plan (avec UIBackgroundModes=audio)
    /// et passe malgré le commutateur silencieux. Les contrôles de l'écran verrouillé viennent de la MediaSession du lecteur web.
    /// </summary>
    private static void ActivateAudioSession()
    {
        try
        {
            // Les échecs Objective-C arrivent en NSError, pas en exception : sans ce test, la lecture écran verrouillé
            // pouvait cesser sans laisser de trace dans le rapport de plantage.
            var session = AVAudioSession.SharedInstance();
            if (!session.SetCategory(AVAudioSession.CategoryPlayback, out var error) || !session.SetActive(true, out error))
                CrashLog.Breadcrumbs.Add($"session audio : {error?.LocalizedDescription ?? "échec sans détail"}");
        }
        catch (Exception ex)
        {
            CrashLog.Breadcrumbs.Add($"session audio : {ex.Message}");
        }
    }

    // iOS peut désactiver la session pendant une interruption (appel) : on la réactive au retour.
    public override void WillEnterForeground(UIApplication application)
    {
        base.WillEnterForeground(application);
        ActivateAudioSession();
    }
}
