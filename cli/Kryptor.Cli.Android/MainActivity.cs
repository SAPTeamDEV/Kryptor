
using System.Security.Permissions;
using System.Text;

using Android.Text.Method;
using Android.Widget;

using Java.Security;

using SAPTeam.Kryptor.Helpers;

namespace SAPTeam.Kryptor.Cli
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            //CheckPermissions();
            Directory.SetCurrentDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

            var input = FindViewById<EditText>(Resource.Id.editText1);
            var btn = FindViewById<Button>(Resource.Id.button1);
            var output = FindViewById<TextView>(Resource.Id.textView1);
            output.MovementMethod = new ScrollingMovementMethod();

            var lw = new LiveWriter(output);
            var vc = new VirtualConsole(lw);
            Console.SetOut(lw);
            Console.SetError(lw);

            btn.Click += async (s, e) =>
            {
                input.Enabled = false;
                btn.Enabled = false;

                await Task.Run(() => HandleButton(input, lw, vc));

                input.Enabled = true;
                btn.Enabled = true;

                lw.Flush();
            };

            var warn = new AlertDialog.Builder(this);
            warn.SetTitle($"Warning");
            warn.SetMessage("The Android build in development and may not work properly");
            warn.SetPositiveButton("OK", delegate { });
            warn.Create().Show();
        }

        private static void HandleButton(EditText input, LiveWriter lw, VirtualConsole vc)
        {
            var tx = input.Text;
            int exit;
            try
            {
                exit = Program.Main(tx.Split(' '), vc);
            }
            catch (Exception ex)
            {
                exit = -1;
                lw.Write(ex.ToString());
            }
        }

        /*
        async Task CheckPermissions()
        {
            await checkPermissions(Android.Manifest.Permission.ManageExternalStorage);
        }

        async Task checkPermissions(string permission)
        {
            var res = CheckCallingOrSelfPermission(permission);
            RequestPermissions(new string[] { permission }, 1);
            
        }
        */

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var alert = new AlertDialog.Builder(this);
            alert.SetTitle($"{e.ExceptionObject.GetType().Name}");
            alert.SetMessage(e.ToString());
            alert.SetPositiveButton("OK", delegate { });
            alert.Create().Show();
        }
    }
}