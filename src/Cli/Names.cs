using FS;
using Snipe;
using Utils;
using DataTypes;
using DataTypes.Auth;

namespace Cli.Names
{
    public class Names
    {
        static async Task<long> GetDelay(){
            var suggestedOffset = await Offset.CalcSuggested();
            return Input.Request<long>($"Offset in ms [suggested: {suggestedOffset}ms]: ");
        }

        public static async Task handleSingleName(AuthResult authResult, Account account){
            string name = Input.Request<string>("Name to snipe: ");
            long delay = await GetDelay();
            var dropTime = Math.Max(0, await Droptime.GetMilliseconds(name, true) - delay);
            Sniper.WaitForName(name, dropTime, account, authResult.loginMethod);
            Sniper.Shoot(account, name);
        }

        public static async Task handleNamesJson(AuthResult authResult, Account account, List<string> namesList){
            long delay = await GetDelay();
            for (int i = 0; i < namesList.Count; i++) {
                var dropTime = Math.Max(0, await Droptime.GetMilliseconds(namesList[i], false) - delay);
                if(dropTime > 0){
                    Sniper.WaitForName(namesList[i], dropTime, account, authResult.loginMethod);
                    Sniper.Shoot(account, namesList[i]);
                }

                // remove sniped name from list and update the file
                if (Config.v.NamesListAutoClean) {
                    namesList = FileSystem.GetNames();
                    namesList.Remove(namesList[i--]);
                }
                FileSystem.SaveNames(namesList);
            }
        }

        public static async Task handleThreeLetter(AuthResult authResult, Account account){
            // todo
        }

        public static async Task handleEnglishNames(AuthResult authResult, Account account){
            // todo
        }
    }
}