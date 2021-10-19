using System;
using Microsoft.Identity.Client;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Reviewer.Core;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;

[assembly: Dependency(typeof(IMicrosoftAuthService))]
namespace Reviewer.Core
{
    public class MicrosoftAuthService : IMicrosoftAuthService
    {

        static readonly string ClientID = "0db2705e-164e-4d0d-a736-c05e7a1380ad";
        static readonly string RedirectUri = "msal.com.jkn.foodreview://auth";
        static readonly string Authority = "https://login.microsoftonline.com/common";
        static readonly string GraphEndpoint = "https://graph.microsoft.com/";

        //static readonly string Tenant = "b2cbuild.onmicrosoft.com";
        //static readonly string ClientID = "0db2705e-164e-4d0d-a736-c05e7a1380ad";
        //static readonly string SignUpAndInPolicy = "B2C_1_Reviewer_SignUpIn";

        //static readonly string AuthorityBase = $"https://login.microsoftonline.com/tfp/{Tenant}/";
        //static readonly string Authority = $"{AuthorityBase}{SignUpAndInPolicy}";

        //static readonly string[] Scopes = { "https://b2cbuild.onmicrosoft.com/reviewer/rvw_all" };

        static readonly string RedirectUrl = $"msal{ClientID}://auth";

        private readonly string[] Scopes = { "User.Read" };
        private readonly string GraphUrl = "https://graph.microsoft.com/v1.0/me";

        private IPublicClientApplication publicClientApplication;

        public void Initialize()
        {
            this.publicClientApplication = PublicClientApplicationBuilder.Create(ClientID)
                .WithIosKeychainSecurityGroup("com.microsoft.adalcache")
                .WithRedirectUri(RedirectUrl)
                .Build();
        }

        /// <summary>
        /// This object is used to know where to display the authentication activity (for Android) or page.
        /// </summary>
        public static object ParentWindow { get; set; }

        /// <summary>
        /// Signin with your Microsoft account.
        /// </summary>
        public async Task<User> OnSignInAsync()
        {
            User currentUser = null;

            var accounts = await this.publicClientApplication.GetAccountsAsync();
            try
            {
                try
                {
                    var firstAccount = accounts.FirstOrDefault();
                    var authResult = await this.publicClientApplication.AcquireTokenSilent(Scopes, firstAccount).ExecuteAsync();
                    currentUser = await this.RefreshUserDataAsync(authResult?.AccessToken).ConfigureAwait(false);
                }
                catch (MsalUiRequiredException ex)
                {
                    // the user was not already connected.
                    try
                    {
                        var authResult = await this.publicClientApplication.AcquireTokenInteractive(Scopes)
                                                    .WithParentActivityOrWindow(ParentWindow)
                                                    .ExecuteAsync();
                        currentUser = await this.RefreshUserDataAsync(authResult?.AccessToken).ConfigureAwait(false);
                    }
                    catch (Exception ex2)
                    {
                        // Manage the exception with a logger as you need
                        System.Diagnostics.Debug.WriteLine(ex2.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                // Manage the exception with a logger as you need
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }

            return currentUser;
        }

        /// <summary>
        /// Sign out with your Microsoft account.
        /// </summary>
        public async Task OnSignOutAsync()
        {
            var accounts = await this.publicClientApplication.GetAccountsAsync();
            try
            {
                while (accounts.Any())
                {
                    await this.publicClientApplication.RemoveAsync(accounts.FirstOrDefault());
                    accounts = await this.publicClientApplication.GetAccountsAsync();
                }
            }
            catch (Exception ex)
            {
                // Manage the exception with a logger as you need
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Refresh user date from the Graph api.
        /// </summary>
        /// <param name="token">The user access token.</param>
        /// <returns>The current user with his associated informations.</returns>
        private async Task<User> RefreshUserDataAsync(string token)
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, this.GraphUrl);
            message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", token);
            HttpResponseMessage response = await client.SendAsync(message);
            User currentUser = null;

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                currentUser = JsonConvert.DeserializeObject<User>(json);
            }

            return currentUser;
        }

        string Base64UrlDecode(string s)
        {
            s = s.Replace('-', '+').Replace('_', '/');
            s = s.PadRight(s.Length + (4 - s.Length % 4) % 4, '=');
            var byteArray = Convert.FromBase64String(s);
            var decoded = Encoding.UTF8.GetString(byteArray, 0, byteArray.Count());
            return decoded;
        }

        JObject ParseIdToken(string idToken)
        {
            // Get the piece with actual user info
            idToken = idToken.Split('.')[1];
            idToken = Base64UrlDecode(idToken);
            return JObject.Parse(idToken);
        }
    }
}
