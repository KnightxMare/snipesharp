﻿using FS;
using Cli;
using Utils;
using DataTypes;
using DataTypes.Auth;
using Cli.Templates;
using Cli.Animatables;
using DataTypes.SetText;
using Cli.Names;

// get and log snipesharp version
string currentVersion = new Snipesharp().GetAssemblyVersion();

// prepare everything and welcome the user
await Initialize(currentVersion);

// handle args
await HandleArgs(currentVersion);

// let the user authenticate
AuthResult authResult = await Core.Auth();

if(!Account.v.prename && authResult.loginMethod != "Bearer Token") if (!await Stats.CanChangeName(Account.v.Bearer)) Cli.Output.ExitError($"{Account.v.MicrosoftEmail} cannot change username yet.");
string? username = await Utils.Stats.GetUsername(Account.v.Bearer);

// handle prename account and change config (runtime only)
if (Account.v.prename) {
    var maxPackets2 = !Convert.ToBoolean(
    new SelectionPrompt("Sniping using a prename account, switch to 2 max packets sent?", 
        new string[] { "Yes [suggested]", "No" }).answerIndex);
    Config.v.SendPacketsCount = maxPackets2 ? 2 : Config.v.SendPacketsCount;
    Output.Inform(TAuth.AuthInforms.NoNameHistory);
    Console.Title = $"snipesharp {currentVersion} - Logged in with a prename account";
}
else if (!String.IsNullOrEmpty(username)) { 
    Console.Title = $"snipesharp {currentVersion} - Logged in as {username}";
    if (Config.v.ShowUsernameDRPC) Utils.DiscordRPC.SetDescription($"Logged in as {username}");
}

// fetch names list now to see if they are empty or not
// will be used later if needed
List<string> namesList = FileSystem.GetNames();

// first time setup
if (DataTypes.Config.v.firstTime) Cli.Output.Inform(Cli.Templates.TFileSystem.FSInforms.Names);

// prompt the user for name choices
var nameOption = new SelectionPrompt("What name(s) would you like to snipe?",
    new string[] {
        TNames.LetMePick,
        TNames.UseNamesJson,
        TNames.ThreeCharNames,
    },
    new string[] {
        namesList.Count == 0 ? TNames.UseNamesJson : "",
    }
).result;

// handle each option individualy
if(nameOption == TNames.LetMePick) await Names.handleSingleName(authResult);
if(nameOption == TNames.UseNamesJson) await Names.handleNamesList(authResult, namesList);
if(nameOption == TNames.ThreeCharNames) await Names.handleThreeLetter(authResult);

// don't exit automatically
Output.Inform("Finished sniping, press any key to exit");
Console.ReadKey();

static async Task Initialize(string currentVersion) {

    // delete snipesharp.old
    try { File.Delete("snipesharp.old"); } catch (Exception e) { FS.FileSystem.Log(e.ToString()); }

    // set console window title
    Console.Title = $"snipesharp {currentVersion}";

    // delete latest log file
    if (File.Exists(FileSystem.latestLogFile)) File.Delete(FileSystem.latestLogFile);

    // log verison
    FS.FileSystem.Log($"Running version: {currentVersion}");

    // attempt to fix windows cmd colors
    if (Core.pid != PlatformID.Unix)
    Fix.Windows.FixCmd();

    // disable quick edit
    if (Core.pid != PlatformID.Unix)
    Fix.Windows.DisableQuickEdit();

    // attempt to fix cursor not showing after close
    Fix.TerminateHandler.FixCursor();

    // dispose discord rpc on close
    Fix.TerminateHandler.FixRpc();

    // clear the console before execution
    Console.Clear();
    SetText.DisplayCursor(true);

    // welcome the user
    Output.PrintLogo();
    if (Config.v.firstTime) Cli.Output.Inform(TFileSystem.FSInforms.ConfigSetup);

    // create, load and log config
    FileSystem.PrepareConfig();
    FS.FileSystem.Log("Using config: " + System.Text.Json.JsonSerializer.Serialize(Config.v, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

    // install snipesharp
    if (Core.arguments.ContainsKey("--install")) Snipesharp.Install();

    // execute auto update
    if (!Core.arguments.ContainsKey("--disable-auto-update")) FS.FileSystem.Log(await Utils.AutoUpdate.Update(currentVersion));

    // load account config
    FileSystem.PrepareAccount();
    
    // override old packetspreadms default value
    if (Config.v.PacketSpreadMs == 31) Config.v.PacketSpreadMs = 390;
    FileSystem.UpdateConfig();
    
    // create and load name list
    if (!FileSystem.NamesFileExists()) FileSystem.SaveNames(new List<string>());

    // create example names file
    FileSystem.SaveNames(new List<string> { "example1", "example2", "example3" }, "example.names.json");

    // disable-discordrpc arg
    if (Core.arguments.ContainsKey("--disable-discordrpc")) {
        Config.v.EnableDiscordRPC = false;
        Cli.Output.Inform("EnableDiscordRPC set to false");
    }

    // enable-discordrpc arg
    if (Core.arguments.ContainsKey("--enable-discordrpc")) {
        Config.v.EnableDiscordRPC = true;
        Cli.Output.Inform("EnableDiscordRPC set to true");
    }

    // start discord rpc
    if (Config.v.EnableDiscordRPC) Utils.DiscordRPC.Initialize();
}

static async Task HandleArgs(string currentVersion) {
    string argName = "";
    
    // --offset handled in Names.cs
    // --disable-auto-update, --disable-discordrpc, --enable-discordrpc & --install handled in Initialize()

    if (Core.arguments.ContainsKey("--help")) {
        Snipesharp.PrintHelp();
        Environment.Exit(0);
    }
    if (Core.arguments.ContainsKey("--packet-spread-ms")) { 
        if (int.TryParse(Core.arguments["--packet-spread-ms"].data!, out int packetSpreadMs)) {
            Config.v.PacketSpreadMs = int.Parse(Core.arguments["--packet-spread-ms"].data!);
            Cli.Output.Inform($"PacketSpreadMs set to {Core.arguments["--packet-spread-ms"].data!}");
        }
        else Cli.Output.Error($"{Core.arguments["--packet-spread-ms"].data!} is not a valid PacketSpreadMs value");
    }
    if (Core.arguments.ContainsKey("--username")) { 
        Config.v.DiscordWebhookUsername = Core.arguments["--username"].data!;
        Cli.Output.Inform($"DiscordWebhookUsername set to {Core.arguments["--username"].data!}");
    }
    if (Core.arguments.ContainsKey("--asc")) {
        Config.v.AutoSkinChange = true;
        Cli.Output.Inform("AutoSkinChange set to true");
    }
    if (Core.arguments.ContainsKey("--name")) argName = Core.arguments["--name"].data!;
    if (Core.arguments.ContainsKey("--email") && Core.arguments.ContainsKey("--password")){
        // set account credentials
        Account.v.MicrosoftEmail = Core.arguments["--email"].data!;
        Account.v.MicrosoftPassword = Core.arguments["--password"].data!;
        Account.v.Bearer = Snipe.Auth.AuthMicrosoft(Account.v.MicrosoftEmail, Account.v.MicrosoftPassword).Result.bearer;

        // verify the credentials work
        if(!Core.arguments.ContainsKey("--dont-verify")) {
            if (string.IsNullOrEmpty(Snipe.Auth.AuthMicrosoft(Account.v.MicrosoftEmail, Account.v.MicrosoftPassword).Result.bearer)) {
                Output.Error(TAuth.AuthInforms.FailedMicrosoft);
                return;
            }
            Cli.Output.Success(TAuth.AuthInforms.SuccessAuthMicrosoft);
        }
        else Output.Warn("Not verifying credentials validity because --dont-verify was used");

        // update config and account files
        FileSystem.UpdateConfig();
        FileSystem.UpdateAccount();

        string? username = await Utils.Stats.GetUsername(Account.v.Bearer);
        if (!String.IsNullOrEmpty(username)) { 
            Console.Title = $"snipesharp - Logged in as {username}";
            if (Config.v.EnableDiscordRPC && Config.v.ShowUsernameDRPC) Utils.DiscordRPC.SetDescription($"Logged in as {username}");
        }

        var temp = new AuthResult {
            loginMethod = TAuth.AuthOptions.Microsoft
        };
        if (string.IsNullOrEmpty(argName)) await Names.handleThreeLetter(temp);
        if (argName == "l" || argName == "list") await Names.handleNamesList(temp, FileSystem.GetNames());
        if (argName == "3" || argName == "3char") await Names.handleThreeLetter(temp);
        if (argName != "3" && argName != "l" && argName != "3char" && argName != "list") await Names.handleSingleName(temp, argName);
        Console.ReadKey();
        return;
    }
    if (Core.arguments.ContainsKey("--bearer")){
        // set account bearer
        Account.v.Bearer = Core.arguments["--bearer"].data!;

        // verify the credentials work
        if(!Core.arguments.ContainsKey("--dont-verify")) {
            if (!await Snipe.Auth.AuthWithBearer(Account.v.Bearer)) {
                Output.Error(TAuth.AuthInforms.FailedBearer);
                return;
            }
            Cli.Output.Success(TAuth.AuthInforms.SuccessAuth);
        }
        else Output.Warn("Not verifying bearer validity because --dont-verify was used");

        // update config and account files
        FileSystem.UpdateConfig();
        FileSystem.UpdateAccount();

        string? username = await Utils.Stats.GetUsername(Account.v.Bearer);
        if (!String.IsNullOrEmpty(username)) {
            Console.Title = $"snipesharp {currentVersion} - Logged in as {username}";
            if (Config.v.EnableDiscordRPC && Config.v.ShowUsernameDRPC) Utils.DiscordRPC.SetDescription($"Logged in as {username}");
        }

        var temp = new AuthResult {
            loginMethod = TAuth.AuthOptions.BearerToken
        };
        if (string.IsNullOrEmpty(argName)) await Names.handleThreeLetter(temp);
        if (argName == "l") await Names.handleNamesList(temp, FileSystem.GetNames());
        if (argName == "3") await Names.handleThreeLetter(temp);
        if (argName != "3" && argName != "l") await Names.handleSingleName(temp, argName);
        Console.ReadKey();
        return;
    }
}