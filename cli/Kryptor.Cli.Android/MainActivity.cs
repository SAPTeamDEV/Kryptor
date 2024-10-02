
using System.CommandLine;

using Android.Text.Method;

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

            EditText input = FindViewById<EditText>(Resource.Id.editText1);
            Button btn = FindViewById<Button>(Resource.Id.button1);
            TextView output = FindViewById<TextView>(Resource.Id.textView1);
            output.MovementMethod = new ScrollingMovementMethod();

            RootCommand root = Program.GetRootCommand();
            LiveWriter lw = new LiveWriter(output);
            VirtualConsole vc = new VirtualConsole(lw);
            Console.SetOut(lw);
            Console.SetError(lw);

            btn.Click += async (s, e) =>
            {
                input.Enabled = false;
                btn.Enabled = false;
                int exit = 0;

                output.Text += $"===Execution started at {DateTime.Now}===\n";
                await Task.Run(() => HandleButton(input, root, lw, vc, out exit));

                input.Enabled = true;
                btn.Enabled = true;

                lw.Flush();

                output.Text += $"===Execution finished at {DateTime.Now}, Exit code: {exit}===\n\n";
            };

            AlertDialog.Builder warn = new AlertDialog.Builder(this);
            warn.SetTitle($"Warning");
            warn.SetMessage("The Android build in development and may not work properly");
            warn.SetPositiveButton("OK", delegate { });
            warn.Create().Show();
        }

        private static void HandleButton(EditText input, RootCommand root, LiveWriter lw, VirtualConsole vc, out int exit)
        {
            string tx = input.Text;

            try
            {
                exit = root.Invoke(tx.Split(' '), vc);
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
            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle($"{e.ExceptionObject.GetType().Name}");
            alert.SetMessage(e.ToString());
            alert.SetPositiveButton("OK", delegate { });
            alert.Create().Show();
        }
    }
}