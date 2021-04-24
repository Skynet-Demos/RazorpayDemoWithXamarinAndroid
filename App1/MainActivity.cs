using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using AndroidX.AppCompat.App;
using Com.Razorpay;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace App1
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, IPaymentResultWithDataListener
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            var button = FindViewById<Button>(Resource.Id.btnRegister);
            button.Click += Button_Click;
        }

        private void Button_Click(object sender, System.EventArgs e)
        {
            string name = FindViewById<EditText>(Resource.Id.txtName).Text;
            string mobile = FindViewById<EditText>(Resource.Id.txtMobile).Text;
            string email = FindViewById<EditText>(Resource.Id.txtEmail).Text;
            double amount = Convert.ToDouble(FindViewById<TextView>(Resource.Id.txtAmount).Text);

            Register(name, mobile, email, amount);
        }

        private async Task Register(string name, string mobile, string email, double amount)
        {
            string orderId = await CreateOrder(amount);
            if (!string.IsNullOrEmpty(orderId))
                MakePayment(name, mobile, email, amount, orderId);
        }

        private async Task<string> CreateOrder(double amount)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");

            var plaintextBytes = Encoding.UTF8.GetBytes("rzp_test_MPvIGssxnpqT5c:q63Un1dC5oeadUNcARG8RrB4");
            string val = Convert.ToBase64String(plaintextBytes);
            httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + val);

            Dictionary<string, object> options = new Dictionary<string, object>();
            options.Add("amount", amount * 100);        // Order amount should be in sub units -> amount * 100
            options.Add("currency", "INR");
            options.Add("payment_capture", 1);

            Dictionary<string, string> notes = new Dictionary<string, string>()
            {
                { "note 1", "first note from Xamarin Android" }, { "note 2", "2nd note" }
            };

            options.Add("notes", notes);

            try
            {
                string url = "https://api.razorpay.com/v1/orders";
                HttpContent content = new StringContent(JsonConvert.SerializeObject(options), Encoding.UTF8, "application/json");
                HttpResponseMessage responseMessage = await httpClient.PostAsync(url, content);
                if (responseMessage.IsSuccessStatusCode)
                {
                    string responseObject = await responseMessage.Content.ReadAsStringAsync();
                    var parsedObject = JObject.Parse(responseObject);
                    string orderId = parsedObject["id"].ToString();
                    Toast.MakeText(this, "Order created", ToastLength.Short);
                    return orderId;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private void MakePayment(string name, string mobile, string email, double amount, string orderId)
        {
            Checkout checkout = new Checkout();
            checkout.SetKeyID("rzp_test_MPvIGssxnpqT5c");
            Activity activity = this;

            try
            {
                JSONObject options = new JSONObject();
                options.Put("description", "My sample payment activity");
                options.Put("order_id", orderId);
                options.Put("currency", "INR");
                options.Put("amount", amount * 100);
                options.Put("name", "Skynet");
                options.Put("description", "a sample payment demo");
                options.Put("prefill.name", name);
                options.Put("prefill.contact", mobile);
                options.Put("prefill.email", email);
                options.Put("theme.color", "#006400");

                JSONObject retryObj = new JSONObject();
                retryObj.Put("enabled", true);
                retryObj.Put("max_count", 4);
                options.Put("retry", retryObj);

                checkout.Open(activity, options);
            }
            catch (Exception ex)
            {

            }
        }

        private void ClearData()
        {
            Checkout.ClearUserData(this);   // this is important to clear existing customer details
            FindViewById<EditText>(Resource.Id.txtName).Text = string.Empty;
            FindViewById<EditText>(Resource.Id.txtMobile).Text = string.Empty;
            FindViewById<EditText>(Resource.Id.txtEmail).Text = string.Empty;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public void OnPaymentError(int p0, string p1, PaymentData p2)
        {
            DisplayAlert("Error", "Oops! Something went wrong in payment");
        }

        public void OnPaymentSuccess(string p0, PaymentData p1)
        {
            ClearData();
            DisplayAlert("Success", "Payment and Registration completed!");
        }

        private void DisplayAlert(string title, string message)
        {
            Android.App.AlertDialog.Builder dialog = new Android.App.AlertDialog.Builder(this);
            Android.App.AlertDialog alert = dialog.Create();
            alert.SetTitle(title);
            alert.SetMessage(message);
            //alert.SetIcon(Resource.Drawable.abc_btn_check_material);
            alert.SetButton("OK", (c, ev) =>
            {
                // Ok button click task  
            });
            alert.Show();
        }
    }
}