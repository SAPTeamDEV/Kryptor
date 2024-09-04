
using System.Text;

namespace SAPTeam.Kryptor.Cli
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            var input = FindViewById<EditText>(Resource.Id.editText1);
            var btn = FindViewById<Button>(Resource.Id.button1);
            var output = FindViewById<TextView>(Resource.Id.textView1);

            btn.Click += (s, e) =>
            {
                var tx = input.Text;
                var mem = new MemoryStream();
                var wr = new StreamWriter(mem);
                Console.SetOut(wr);

                int exit;
                string oText;
                try
                {
                    exit = Program.Main(tx.Split(' '), new VirtualConsole(wr));
                    wr.Flush();
                    oText = Encoding.UTF8.GetString(mem.ToArray());
                }
                catch (Exception ex)
                {
                    exit = -1;
                    oText = ex.ToString();
                }

                //output.SetText(oText.ToCharArray(), 0, oText.Length);

                var alert = new AlertDialog.Builder(this);
                alert.SetTitle($"Result ({exit})");
                alert.SetMessage(oText);
                alert.SetPositiveButton("OK", delegate { });
                alert.Create().Show();
            };

            var warn = new AlertDialog.Builder(this);
            warn.SetTitle($"Warning");
            warn.SetMessage("The Android build in development and may not work properly");
            warn.SetPositiveButton("OK", delegate { });
            warn.Create().Show();
        }
    }
}