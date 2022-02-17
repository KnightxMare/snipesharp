<p align="center">
  <a href="#about">
    <img src="https://user-images.githubusercontent.com/93228501/154115422-57cca957-4f1a-4cdf-93f5-18f9dd3cc13b.png">
  </a>
</p>
<p align="center">
  <a href="https://github.com/snipesharp/snipesharp/releases/download/v1.1.0/sha256sums.txt">
    <img src="https://img.shields.io/badge/sha256sums-%231a6eef?style=flat-square"</img>
  </a>
  <a href="https://github.com/snipesharp/snipesharp/releases/download/v1.1.0/snipesharp_linux-x86-64">
    <img src="https://img.shields.io/badge/linux%20x86-v1.1.0-%231a6eef?style=flat-square"</img>
  </a>
  <a href="https://github.com/snipesharp/snipesharp/releases/download/v1.1.0/snipesharp_linux-arm64">
    <img src="https://img.shields.io/badge/linux%20arm64-v1.1.0-%231a6eef?style=flat-square"</img>
  </a>
  <a href="https://github.com/snipesharp/snipesharp/releases/download/v1.1.0/snipesharp_win-x86-64.exe">
    <img src="https://img.shields.io/badge/windows-v1.1.0-%231a6eef?style=flat-square"</img>
  </a>
</p>
<p align="center">
  <a href="https://discord.com/ptFvZ8AYuU">
    <img src="https://img.shields.io/discord/943483411597758494?color=567CFF&label=discord&logo=discord&logoColor=ffffff&style=for-the-badge">
  </a>
</p>

# About
Snipesharp is an easy to use Minecraft name Sniper coded in C# thats focused on both speed and user friendliness.

## Created by:

<p align="center">
<a href="https://namemc.com/profile/dement6d.1">
demented
  <img src="https://mc-heads.net/head/a5aee899-2d82-4594-aed1-f547178db6c0/100"></img>
</a>
<a href="https://github.com/StiliyanKushev">
  <img src="https://user-images.githubusercontent.com/93228501/154389051-a3852ea7-5d2d-435c-9858-52c883d5d818.png">StiliyanKushev</img>
</a>
</p>

# Features
## Logging in
- Features
  - Completely possible through the Console Interface
  - Re-use of previous credentials
- Methods
  - Microsoft Login
  - Bearer Token Login
  - Using previous session credentials/bearer
- Configuration (account.json)
  - All credentials (including Bearer Token) can be edited through account.json
## Post sniping
- Discord webhooks
  - Features
    - Webhooks contain your desired username & the name you sniped
    - Webhooks contain Minecraft character head of the account which sniped the name
  - Configuration (config.json)
    - Custom username contained in webhook
    - Enable/disable webhook to snipesharp Discord server
    - Enable/disable webhook to custom Discord server
- Automatic skin change
  - Configuration (config.json)
    - Custom skin

# How to use
Start snipersharp and you will be asked to choose a login method.
1. You can choose between a `Bearer Token` and a `Microsoft Account` and, if you've already authenticated before, you will be able to choose `From previous session`
2. Once you've successfully authenticated your account, you choose which `name` you want to snipe
3. After that, choose the `Offset in ms`, this determines how early to start sniping the chosen name in milliseconds
4. Wait for the sniper to obtain the chosen name

# Donate
- To demented
  - XMR: 89Gk3YiZGWnLsgGygzRg8Shqp1UyEuYGMbnrz3dLX9isbiLb5b8e6Zu4rT6NX5K5dsNtMb1WTyScqdYCsjxNfUFaRLcdeBk
- To StiliyanKushev
  - No donation methods implemented

# Configuration file locations
## Windows
- %appdata% = WindowsPartition:\\Users\\{user}\\AppData\\Roaming
- account.json = %appdata%\\.snipesharp\\account.json
- config.json = %appdata%\\.snipesharp\\config.json
## Linux
- ~/ = /home/{user}
- account.json = ~/.snipesharp/account.json
- config.json = ~/.snipesharp/config.json