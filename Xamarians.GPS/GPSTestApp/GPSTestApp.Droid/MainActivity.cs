
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;

namespace GPSTestApp.Droid
{
    [Activity(Label = "GPSTestApp", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironment_UnhandledExceptionRaiser;
            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            Xamarians.GPS.Droid.GPSServiceAndroid.Initialize(this);

            LoadApplication(new App());
        }

        private void AndroidEnvironment_UnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs e)
        {
            e.Handled = true;
            var alertDialog = new AlertDialog.Builder(this);
            alertDialog.SetTitle("Exception");
            alertDialog.SetMessage(e.Exception.Message + "____" + e.Exception.ToString());
            alertDialog.SetNeutralButton("Ok", (s, ee) =>
            {
            });
            alertDialog.Show();
        }
    }
}

