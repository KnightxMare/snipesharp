using System.Text.Json;
using System.Net.Http.Headers;
using Cli.Animatables;

namespace Snipe
{
    public class Auth
    {
        public static async Task<bool> AuthWithBearer(string bearer) {
            if (!await OwnsMinecraft(bearer)) Cli.Output.ExitError("Account doesn't own Minecraft");
            return true;
        }
        
        /// <returns>MC Bearer using Microsoft credentials</returns>
        public static async Task<MsAuthResult> AuthMicrosoft(string email, string password) {
            var spinner = new Spinner();

            // get post url and PPFT
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:97.0) Gecko/20100101 Firefox/97.0");
            HttpResponseMessage initGet = await client.GetAsync("https://login.live.com/oauth20_authorize.srf?client_id=000000004C12AE6F&redirect_uri=https://login.live.com/oauth20_desktop.srf&scope=service::user.auth.xboxlive.com::MBI_SSL&display=touch&response_type=token&locale=en");
            string GetResult = initGet.Content.ReadAsStringAsync().Result;
            string sFTTag = Validators.Auth.rSFTTagRegex.Matches(GetResult)[0].Value.Replace("value=\"", "").Replace("\"", "");
            string urlPost = Validators.Auth.rUrlPostRegex.Matches(GetResult)[0].Value.Replace("urlPost:'", "").Replace("'", "");

            var requestContent = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string,string>("login",email),
                new KeyValuePair<string,string>("loginfmt",email),
                new KeyValuePair<string, string>("passwd",password),
                new KeyValuePair<string, string>("PPFT",sFTTag)
            });

            // make the post call
            var postHttpResponse = await client.PostAsync(urlPost, requestContent);

            // if it was successful
            if (postHttpResponse.RequestMessage!.RequestUri!.AbsoluteUri.Contains("access_token")){
                if (postHttpResponse.ToString().Contains("Sign in to")) { Cli.Output.ExitError("Wrong credentials, failed to login"); }
                if (postHttpResponse.ToString().Contains("Help us protect your account")) { Cli.Output.ExitError("2FA enabled, failed to login"); }
                
                //  parse url data to dictionary
                Dictionary<string, string> urlDataDictionary = new Dictionary<string, string>();
                var urlData = postHttpResponse.RequestMessage.RequestUri.AbsoluteUri.Split('#')[1].Split('&');
                for (int i = 0; i < urlData.Length; i++) urlDataDictionary.Add(
                    urlData[i].Split('=').First(), urlData[i].Split('=').Last()
                );

                if (urlDataDictionary.TryGetValue("access_token", out string? token)) {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    StringContent xboxPayloadContent = XboxPayload.GetContent(XboxPayload.GenerateXboxPayload(token));
                    var xboxPayloadJsonResponse = JsonSerializer.Deserialize<XboxResponse>
                        (client.PostAsync("https://user.auth.xboxlive.com/user/authenticate", xboxPayloadContent).Result.Content.ReadAsStringAsync().Result);

                    StringContent xstsPayloadContent = XboxPayload.GetContent(XboxPayload.GenerateXstsPayload(xboxPayloadJsonResponse!.Token!));
                     var xstsPayloadHttpResponse =
                        client.PostAsync("https://xsts.auth.xboxlive.com/xsts/authorize", xstsPayloadContent);
                     var xstsPayloadJsonResponse = JsonSerializer.Deserialize<XboxResponse>
                        (xstsPayloadHttpResponse.Result.Content.ReadAsStringAsync().Result);

                    if (xstsPayloadJsonResponse!.DisplayClaims == null){
                        Cli.Output.Error("Microsoft account not linked to an Xbox account");
                        return new MsAuthResult();
                    }

                     // Get MC Bearer
                    StringContent mcPayloadContent = McPayload.GetContent(McPayload.GenerateMcPayload(xstsPayloadJsonResponse!.DisplayClaims.xui![0].uhs!, xstsPayloadJsonResponse!.Token!));
                    HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.minecraftservices.com/authentication/login_with_xbox");
                    requestMessage.Content = mcPayloadContent;
                        
                    var mcApiHttpResponse = client.SendAsync(requestMessage).Result.Content.ReadAsStringAsync();
                    var mcApiJsonResponse = JsonSerializer.Deserialize<McApiResponse>(mcApiHttpResponse.Result);
                    spinner.Cancel();

                    // Check if account owns MC
                     bool ownsMinecraft = await OwnsMinecraft(mcApiJsonResponse.access_token);
                    if (!ownsMinecraft) {
                        Cli.Output.ExitError("Account doesn't own Minecraft");
                    }

                    if (!await HasNameHistory(mcApiJsonResponse.access_token) && ownsMinecraft) return new MsAuthResult{bearer = mcApiJsonResponse.access_token.ToString(), prename = true};
                    if (!String.IsNullOrEmpty(mcApiJsonResponse.access_token.ToString())) return new MsAuthResult{bearer = mcApiJsonResponse.access_token.ToString(), prename = false};
                }
                else Cli.Output.ExitError("Failed to get access_token");
            } 
            else {
                // log the response for later debug
                spinner.Cancel();
                string result = await postHttpResponse.Content.ReadAsStringAsync();
                FS.FileSystem.Log(result);

                // handle error
                string error = "";
                if (result.Contains("That Microsoft account doesn\\'t exist")) error = "That Microsoft account doesn't exist";
                if (result.Contains("incorrect")) error = "Wrong password";
                if (result.Contains("Please enter the password for your Microsoft account")) error = "Password can't be empty";
                if (!result.Contains("Please enter the password for your Microsoft account") && !result.Contains("incorrect") && !result.Contains("That Microsoft account doesn\\'t exist")) error = $"Failed due to Microsoft suspecting suspicious activities. Try following this tutorial to fix this: {DataTypes.SetText.SetText.Cyan}https://github.com/snipesharp/snipesharp/wiki/How-to-fix-failed-Microsoft-login {DataTypes.SetText.SetText.ResetAll}";
                Cli.Output.Error(error);
            }

            return new MsAuthResult();
        }

        // return true if user has name history, false otherwise
        public async static Task<bool> HasNameHistory(string bearer) {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
            var response = await client.GetAsync("https://api.minecraftservices.com/minecraft/profile");
            return (int)response.StatusCode == 200;
        }

        // return true if user owns minecraft, false otherwise
        public async static Task<bool> OwnsMinecraft(string bearer) {
            // prepare http call using the bearer
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:97.0) Gecko/20100101 Firefox/97.0");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearer);

            // get json response
            var mcOwnershipHttpResponse = await client.GetAsync("https://api.minecraftservices.com/entitlements/mcstore");
            if (!mcOwnershipHttpResponse.IsSuccessStatusCode) Cli.Output.ExitError(Cli.Templates.TAuth.AuthInforms.FailedBearer);
            var mcOwnershipJsonResponse = JsonSerializer.Deserialize<McOwnershipResponse>(
                await mcOwnershipHttpResponse.Content.ReadAsStringAsync()
            );

            // If doesn't own minecraft, prompt to redeem a giftcard 
            if (mcOwnershipJsonResponse.items == null || mcOwnershipJsonResponse.items.Length < 1) {
                bool redeemResult;
                while (redeemResult = !await RedeemGiftcard(Cli.Input.Request<string>(Cli.Templates.TRequests.Giftcode), bearer));
                return redeemResult;
            }
            return true;
        }

        // redeen a giftcard from given giftcode. Returns true or false based on success.
        public async static Task<bool> RedeemGiftcard(string giftcode, string bearer) {
            // prepare the http request
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:97.0) Gecko/20100101 Firefox/97.0");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearer);

            // 
            var spinner = new Spinner();
            var response = await client.PutAsync($"https://api.minecraftservices.com/productvoucher/:{giftcode}", null);
            spinner.Cancel();
            if ((int)response.StatusCode!=200) Cli.Output.Error("Failed to redeem giftcard, try again");
            return (int)response.StatusCode==200;
        }
    }

    public class McPayload {
        public string? identityToken { get; set; }
        public bool ensureLegacyEnabled { get; set; }
        public static McPayload GenerateMcPayload(string userHash, string xstsToken) {
            return new McPayload {
                identityToken = $"XBL3.0 x={userHash};{xstsToken}",
                ensureLegacyEnabled = true
            };
        }
        public static StringContent GetContent(McPayload payload) {
            return new StringContent(
                JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented=true}),
                System.Text.Encoding.UTF8, "application/json");
        }
    }
    public class XboxPayload {
        public object? Properties { get; set; }
        public string? RelyingParty { get; set; }
        public string? TokenType { get; set; }
        public static XboxPayload GenerateXboxPayload(string token) {
            return new XboxPayload {
                Properties = new {
                    AuthMethod = "RPS",
                    SiteName = "user.auth.xboxlive.com",
                    RpsTicket = token
                },
                RelyingParty = "http://auth.xboxlive.com",
                TokenType = "JWT"
            };
        }

        public static XboxPayload GenerateXstsPayload(string token) {
            return new XboxPayload {
                Properties = new {
                    SandboxId = "RETAIL",
                    UserTokens = new[] { token }
                },
                RelyingParty = "rp://api.minecraftservices.com/",
                TokenType = "JWT"
            };
        }
        public static StringContent GetContent(XboxPayload xboxPayload) {
            return new StringContent
                (JsonSerializer.Serialize(
                    xboxPayload,
                    new JsonSerializerOptions { WriteIndented = true }
                ), System.Text.Encoding.UTF8, "application/json");
        }
    }

    public struct MsAuthResult {
        public string bearer { get;set; }
        public bool prename { get;set; }
    }
    public class Items {
        public string? name { get; set; }
        public string? signature { get; set; }
    }
    public struct McOwnershipResponse {
        public Items[] items { get; set; }
        public string signature { get; set; }
        public string keyId { get; set; }
    }
    public struct McApiResponse {
        public string username { get; set; }
        public object roles { get; set; }
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
    }

    public class XboxResponse {
        public string? IssueInstant { get; set; }
        public string? NotAfter { get; set; }
        public string? Token { get; set; }
        public DisplayClaimsObject? DisplayClaims { get; set; }
    }

    public class DisplayClaimsObject { public XuiObject[]? xui { get; set; } }
    public class XuiObject { public string? uhs { get; set; } }
}