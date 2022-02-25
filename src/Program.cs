﻿using FS;
using Cli;
using Utils;
using DataTypes;
using DataTypes.Auth;
using Cli.Templates;
using Cli.Animatables;
using DataTypes.SetText;
using Cli.Names;
			
// prepare everything and welcome the user
Initialize();

// let the user authenticate
AuthResult authResult = await Core.Auth();
Account account = authResult.account;

if(!await Stats.CanChangeName(account.Bearer)){
    Cli.Output.ExitError($"{account.MicrosoftEmail} cannot change username yet.");
}

// handle prename account and change config (runtime only)
if (account.Prename) {
    var maxPackets2 = !Convert.ToBoolean(
    new SelectionPrompt("Sniping using a prename account, switch to 2 max packets sent?", 
        new string[] { "Yes [suggested]", "No" }).answerIndex);
    Config.v.SendPacketsCount = maxPackets2 ? 2 : Config.v.SendPacketsCount;
    Output.Inform(TAuth.AuthInforms.NoNameHistory);
}

// fetch names list now to see if they are empty or not
// will be used later if needed
List<string> namesList = FileSystem.GetNames();

// prompt the user for name choices
var nameOption = new SelectionPrompt("What name/s would you like to snipe?",
    new string[] {
        TNames.LetMePick,
        TNames.UseNamesJson,
        TNames.ThreeLetterNames,
        TNames.EnglishNames
    },
    new string[] {
        namesList.Count == 0 ? TNames.UseNamesJson : "",
        TNames.EnglishNames
    }
).result;

// handle each option individualy
if(nameOption == TNames.LetMePick) await Names.handleSingleName(authResult, account);
if(nameOption == TNames.UseNamesJson) await Names.handleNamesList(authResult, account, namesList);
if(nameOption == TNames.ThreeLetterNames) await Names.handleThreeLetter(authResult, account);
if(nameOption == TNames.EnglishNames) await Names.handleEnglishNames(authResult, account);

// don't exit automatically
Output.Inform("Finished sniping, press any key to exit");
Console.ReadKey();

static void Initialize() {
    // delete latest log file
    if (File.Exists(FileSystem.latestLogFile)) File.Delete(FileSystem.latestLogFile);

    // attempt to fix windows cmd colors
    if (Core.pid != PlatformID.Unix)
    Fix.Windows.FixCmd();

    // attempt to fix cursor not showing after close
    Fix.TerminateHandler.FixCursor();

    // dispose discord rpc on close
    Fix.TerminateHandler.FixRpc();

    // clear the console before execution
    Console.Clear();
    SetText.DisplayCursor(true);

    // welcome the user
    Output.PrintLogo();

    // create and load config
    FileSystem.PrepareConfig();

    // create and load name list
    if (!FileSystem.NamesFileExists()) FileSystem.SaveNames(new List<string>());

    // create example names file
    FileSystem.SaveNames(new List<string> { "example1", "example2", "example3" }, "names.example.json");

    // start discord rpc
    Utils.DiscordRPC.Initialize();
}