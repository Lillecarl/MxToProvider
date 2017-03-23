using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;

using ARSoft.Tools.Net.Dns;
using DnsClient;
using Newtonsoft.Json;

namespace MailSolutionFinder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///

    public class MxMappings
    {
        public List<MxMapping> mappings = new List<MxMapping>();
    }

    public class MxMapping
    {
        public string recordmatch = "";
        public string provider = "";
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var mappings = new MxMappings()
            {
                mappings = new List<MxMapping>()
                {
                    new MxMapping()
                    {
                        recordmatch = "recordmatch", provider = "provider"
                    },
                },
            };

            try
            {
                string contents = "";
                using (var wc = new System.Net.WebClient())
                    contents = wc.DownloadString("https://raw.githubusercontent.com/Lillecarl/MxToProvider/master/mxtoprovider.json");

                mappings = JsonConvert.DeserializeObject<MxMappings>(contents);
            }
            catch
            { }

            Console.WriteLine(JsonConvert.SerializeObject(mappings, Formatting.Indented));

            resulttextbox.Text = "";

            var lookup = new LookupClient(IPAddress.Parse("8.8.8.8"))
            {
                Timeout = new System.TimeSpan(0, 0, 3),
                UseCache = false,
                EnableAuditTrail = true,
            };

            var address = mailaddrbox.Text;

            if (address.Contains("@"))
            {
                int idx = address.IndexOf("@");
                address = address.Substring(idx + 1, address.Length - idx - 1);
            }
            Console.WriteLine("Lookup address: {0}", address);
            var result = lookup.Query(address, QueryType.ANY);

            Console.WriteLine(result.AuditTrail);

            if (result.HasError)
                resulttextbox.Text += result.ErrorMessage + Environment.NewLine;

            foreach (var i in result.Answers.MxRecords())
            {
                string value = i.Exchange.ToString().ToLower();

                resulttextbox.Text += string.Format("{0} MX has value {1} ", address, value);

                foreach (var j in mappings.mappings)
                    if (value.Contains(j.recordmatch.ToLower()))
                        resulttextbox.Text += string.Format("({0}) ", j.provider);

                resulttextbox.Text += Environment.NewLine;
            }

            /*foreach (var i in result.Answers.TxtRecords())
            {
                foreach (var value in i.Text)
                {
                    resulttextbox.Text += string.Format("{0} SPF has value {1} ", address, value);

                    if (value.Contains("spf.protection.outlook.com"))
                        resulttextbox.Text += "(Office365)";

                    if (value.Contains("googlemail.com") || value.Contains("google.com"))
                        resulttextbox.Text += "(Google Apps)";

                    resulttextbox.Text += Environment.NewLine;
                }
            }*/
        }
    }
}
