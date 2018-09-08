using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Nfc;
using Android.OS;
using Prism;
using Prism.Ioc;

namespace NFCReader.Droid
{
    [Activity(Label = "NFCReader", Icon = "@mipmap/ic_launcher", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public NfcAdapter _NfcAdapter;
        public NfcScannerService _nfcScannerService;
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            NfcManager NfcManager = (NfcManager)Android.App.Application.Context.GetSystemService(Context.NfcService);
            _NfcAdapter = NfcManager.DefaultAdapter;
            _nfcScannerService = (NfcScannerService) ((NFCReader.App)Xamarin.Forms.Application.Current).Container.Resolve(typeof(INfcScannerService));

            LoadApplication(new App(new AndroidInitializer()));
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (_NfcAdapter != null)
            {
                var intent = new Intent(this, GetType()).AddFlags(ActivityFlags.SingleTop);
                _NfcAdapter.EnableForegroundDispatch
                (
                    this,
                    PendingIntent.GetActivity(this, 0, intent, 0),
                    new[] { new IntentFilter(NfcAdapter.ActionTechDiscovered) },
                    new String[][] {new string[] {
                            NFCTechs.Ndef,
                        },
                        new string[] {
                            NFCTechs.MifareClassic,
                        },
                    }
                );
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            _NfcAdapter.DisableForegroundDispatch(this);
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            _nfcScannerService.OnNewIntent(this, intent);
        }

    }

    public class AndroidInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IContainerRegistry container)
        {
            // Register any platform specific implementations
            container.Register<INfcScannerService, NfcScannerService>();
        }
    }
}

