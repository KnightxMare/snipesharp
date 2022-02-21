<p align="center">
  <a href="#about">
    <img src="https://user-images.githubusercontent.com/93228501/154115422-57cca957-4f1a-4cdf-93f5-18f9dd3cc13b.png">
  </a>
</p>
<p align="center">
  <a href="https://github.com/snipesharp/snipesharp/releases/download/v1.4.0/sha256sums.txt">
    <img src="https://img.shields.io/badge/sha256sums-%231a6eef?style=flat-square"</img>
  </a>
  <a href="https://github.com/snipesharp/snipesharp/releases/download/v1.4.0/snipesharp_linux-x86-64-v1.4.0">
    <img src="https://img.shields.io/badge/linux-v1.4.0-%231a6eef?style=flat-square"</img>
  </a>
  <a href="https://github.com/snipesharp/snipesharp/releases/download/v1.4.0/snipesharp_mac-os-x86-64-v1.4.0">
    <img src="https://img.shields.io/badge/mac_os-v1.4.0-%231a6eef?style=flat-square"</img>
  </a>
  <a href="https://github.com/snipesharp/snipesharp/releases/download/v1.4.0/snipesharp_win-x86-64-v1.4.0.exe">
    <img src="https://img.shields.io/badge/windows-v1.4.0-%231a6eef?style=flat-square"</img>
  </a>
</p>
<p align="center">
  <a href="https://github.com/snipesharp/snipesharp/releases/download/v1.4.0/snipesharp_linux-arm64-v1.4.0">
    <img src="https://img.shields.io/badge/linux_arm64-v1.4.0-%23015fa1?style=flat-square"</img>
  </a>
  <a href="https://github.com/snipesharp/snipesharp/releases/download/v1.4.0/snipesharp_mac-os-arm64-v1.4.0">
    <img src="https://img.shields.io/badge/mac_os_arm64-v1.4.0-%23015fa1?style=flat-square"</img>
  </a>
  <a href="https://github.com/snipesharp/snipesharp/releases/download/v1.4.0/snipesharp_win-arm64-v1.4.0.exe">
    <img src="https://img.shields.io/badge/windows_arm64-v1.4.0-%23015fa1?style=flat-square"</img>
  </a>
</p>
<p align="center">
  <a href="https://github.com/snipesharp/snipesharp/releases/tag/v1.4.0">
    <img src="https://img.shields.io/badge/latest_release-%23015fa1?style=flat-square"</img>
  </a>
</p>
<p align="center">
  <a href="https://discord.gg/ptFvZ8AYuU">
    <img src="https://img.shields.io/discord/943483411597758494?color=567CFF&label=discord&logo=discord&logoColor=ffffff&style=for-the-badge">
  </a>
</p>

# About
Snipesharp is an easy to use Minecraft name Sniper coded in C# thats focused on both speed and user friendliness.

### Created by:

<p align="center">
<a href="https://namemc.com/profile/dement6d.1">
demented
  <img src="https://mc-heads.net/head/a5aee899-2d82-4594-aed1-f547178db6c0/100"></img>
</a>
<a href="https://github.com/StiliyanKushev">
  <img src="https://i.imgur.com/lMWqAlH.png">StiliyanKushev</img>
</a>
</p>

# Features
### Logging in
- Features
  - Completely possible through the Console Interface
  - Re-use of previous credentials
- Methods
  - Microsoft Login
  - Bearer Token Login
  - Using previous session credentials/bearer
- Configuration ([account.json](#configuration-file-locations))
  - All credentials (including Bearer Token) can be edited through account.json
### Sniping
- Features
  - Giftcard (prename) sniping
  - Snipe custom name
  - Snipe from a custom name list
    - Automatically clean name list of names which were already sniped, can be disabled from [config.json](#configuration-file-locations)
    - Supports sniping 24/7 (Bearer gets updated using Microsoft Account credentials)
    - Snipes in top to bottom order
  - Automatic (suggested) offset based on your ping and more math
- Configuration ([config.json](#configuration-file-locations) & In App)
  - Custom offset configured in app
  - Custom name configured in app
  - Custom name list configured in [config.json](#configuration-file-locations)
  - Custom packets sent amount configured in [config.json](#configuration-file-locations)
  - Custom delay between packets sent configured in [config.json](#configuration-file-locations)
### Post sniping
- Discord webhooks
  - Features
    - Webhooks contain your desired username & the name you sniped
    - Webhooks contain Minecraft character head of the account which sniped the name
  - Configuration ([config.json](#configuration-file-locations))
    - Custom username contained in webhook
    - Enable/disable webhook to snipesharp Discord server
    - Enable/disable webhook to custom Discord server
- Automatic skin change
  - Configuration ([config.json](#configuration-file-locations))
    - Custom skin
    - Custom skin type (classic/slim)

# Donate
- To demented
  - Monero: 89Gk3YiZGWnLsgGygzRg8Shqp1UyEuYGMbnrz3dLX9isbiLb5b8e6Zu4rT6NX5K5dsNtMb1WTyScqdYCsjxNfUFaRLcdeBk
- To StiliyanKushev
  - No donation methods yet

# Troubleshooting
If youre having trouble while using snipesharp, check for solutions on the [Wiki](https://github.com/snipesharp/snipesharp/wiki).
If you still can't fix your issue or you just want to hang out, you can join [our Discord server](https://discord.gg/ptFvZ8AYuU) 

<a href="https://discord.gg/ptFvZ8AYuU">
    <img src="https://img.shields.io/discord/943483411597758494?color=567CFF&label=discord&logo=discord&logoColor=ffffff&style=for-the-badge">
</a>

# Configuration file locations
### Windows
- `%appdata%` = WindowsPartition:\\Users\\{user}\\AppData\\Roaming
- `account.json` = %appdata%\\.snipesharp\\account.json
- `config.json` = %appdata%\\.snipesharp\\config.json
### Linux
- `~/` = /home/{user}/
- `account.json` = ~/.snipesharp/account.json
- `config.json` = ~/.snipesharp/config.json
