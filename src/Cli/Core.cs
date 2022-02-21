using FS;
using DataTypes;
using Cli.Templates;
using Cli.Animatables;

namespace Cli
{
    public class Core
    {
        public static Dictionary<string, Argument> arguments = ParseArgs();
        public static PlatformID pid = Environment.OSVersion.Platform;

        // parse the command line arguments and convert them to a dictionary
        private static Dictionary<string, Argument> ParseArgs(){
            List<string> args = Environment.GetCommandLineArgs().Skip(1).ToList();
            var parsedArguments = new Dictionary<string, Argument>();
            
            args.ForEach(arg => {
                var parsed = new Argument(arg);
                parsedArguments[parsed.name] = parsed;
            });

            return parsedArguments;
        }

        public struct AuthResult
        {
            public Account account;
            public string loginMethod;
        }

        public static async Task<AuthResult> Auth(){
            // display prompt
            string loginMethod = FileSystem.AccountFileExists()
                ? new SelectionPrompt("Login method:", "From previous session", "Bearer Token", "Microsoft Account", "Mojang Account").result
                : new SelectionPrompt("Login method:", "Bearer Token", "Microsoft Account", "Mojang Account").result;
 
            // obtain login info based on login method choice
            Account account = FileSystem.AccountFileExists() ? FileSystem.GetAccount() : new Account();
            if (loginMethod == "Bearer Token") account = await HandleBearer(account, true);
            else if (loginMethod == "Mojang Account") account = await HandleMojang(account, true);
            else if (loginMethod == "Microsoft Account") account = await HandleMicrosoft(account, true);
            else
            {
                var handleFromFileResult = await HandleFromFile();
                account = handleFromFileResult.Account;
                if (!String.IsNullOrEmpty(loginMethod)) loginMethod = handleFromFileResult.Choice;
            }

            if (account.Prename) Output.Inform("No name history detected, will perform prename snipe and send 6 packets instead of 3");

            // save account
            FileSystem.SaveAccount(account);

            return new AuthResult { account = account, loginMethod = loginMethod };
        }

        private static async Task<Account> HandleMicrosoft(Account account, bool newLogin=false){
            // warn about 2fa
            Output.Warn("Make sure 2 Factor Authentication (2FA) is turned off in your Microsoft account settings");

            // get new credentials
            if (newLogin) {
                account.MicrosoftEmail = Input.Request<string>(Requests.MicrosoftEmail, validator: Validators.Credentials.Email);
                account.MicrosoftPassword = Input.Request<string>(Requests.MicrosoftPassword, hidden: true);
            }
            
            // get bearer with microsoft credentials
            var authResult = await Snipe.Auth.AuthMicrosoft(account.MicrosoftEmail, account.MicrosoftPassword);

            // if bearer not returned, exit
            if (String.IsNullOrEmpty(authResult.bearer)) Output.ExitError("Failed to authenticate Microsoft account");

            account.Bearer = authResult.bearer;
            account.Prename = authResult.prename;
            Output.Success($"Successfully authenticated & updated bearer");

            return account;
        }
        private static async Task<Account> HandleMojang(Account account, bool newLogin = false) {
            if (newLogin) {
                account.MojangEmail = Input.Request<string>(Requests.MojangEmail, validator: Validators.Credentials.Email);
                account.MojangPassword = Input.Request<string>(Requests.MojangPassword, hidden: true);
            }

            // todo, actual async stuff here
            Output.Error($"Not authenticated (Mojang login not implemented)");
            return account;
        }
        private static async Task<Account> HandleBearer(Account account, bool newBearer=false){
            // prompt for bearer token
            if (newBearer) account.Bearer = Input.Request<string>(Requests.Bearer);

            // exit if invalid bearer
            if(!await Snipe.Auth.AuthWithBearer(account.Bearer)) Output.ExitError("Failed to authenticate using bearer");

            // validate the token
            Output.Warn("Bearer tokens reset every 24 hours & on login, sniping will fail if the bearer has expired at snipe time!");
            Output.Success($"Successfully authenticated");
        
            return account;
        }
        private static async Task<HandleFromFileResult> HandleFromFile() {
            var account = FileSystem.GetAccount();

            List<string> availableMethods = new List<string>();
            if (!String.IsNullOrEmpty(account.Bearer)) availableMethods.Add("Bearer Token");
            if (!String.IsNullOrEmpty(account.MicrosoftPassword) && !String.IsNullOrEmpty(account.MicrosoftEmail)) availableMethods.Add("Microsoft Account");
            if (!String.IsNullOrEmpty(account.MojangPassword) && !String.IsNullOrEmpty(account.MojangEmail)) availableMethods.Add("Mojang Account");

            string choice = "";
            if (availableMethods.Count > 1) choice = new SelectionPrompt("More than one login method previously used, choose one:", availableMethods.ToArray()).result;
            else choice = availableMethods[0];

            // authenticate the chosen method
            if (choice == "Bearer Token") account = await HandleBearer(account);
            if (choice == "Mojang Account") account = await HandleMojang(account);
            if (choice == "Microsoft Account") account = await HandleMicrosoft(account);

            return new HandleFromFileResult { Account = account, Choice = choice };
        }
        private struct HandleFromFileResult
        {
            public Account Account { get; set; }
            public string Choice { get; set; }
        }
    }
}