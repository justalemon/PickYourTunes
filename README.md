[![AppVeyor](https://img.shields.io/appveyor/ci/justalemon/PickYourTunes.svg)](https://ci.appveyor.com/project/justalemon/pickyourtunes)
[![Github All Releases](https://img.shields.io/github/downloads/justalemon/PickYourTunes/total.svg)](https://github.com/justalemon/PickYourTunes/releases)
[![5mods](https://img.shields.io/badge/Download-5mods-1b9e42.svg)](https://www.gta5-mods.com/scripts/pickyourtunes)

# PickYourTunes for Grand Theft Auto V
This mod helps you to set default radio stations on vehicles with support for custom files (mp3/wav/ogg).

# Building the Mod
* Clone the repo
* Download Visual Studio 2017 with the .NET Framework 4.7.2 SDK & Targeting Pack
* Open the solution
* On the top bar, select Build > Build Solution or press Ctrl+Shift+B
* Done

# Configurating the Mod

## Getting vehicle hashes
**NOTE**: If you know how to get vehicle hashes, skip this

1. Run the game with the Mod
2. Get in a car and use the cheat "pyt hash"
3. Copy the hash that has appeared over the radar

## Setting Radios and custom files

1. Run the game with the mod at least once
2. If you are going to use a Radio, copy the ID from the following table:
  * Los Santos Rock Radio        | ID   0 | RADIO_01_CLASS_ROCK
  * Non-Stop-Pop FM              | ID   1 | RADIO_02_POP
  * Radio Los Santos             | ID   2 | RADIO_03_HIPHOP_NEW
  * Channel X                    | ID   3 | RADIO_04_PUNK
  * West Coast Talk Radio        | ID   4 | RADIO_05_TALK_01
  * Rebel Radio                  | ID   5 | RADIO_06_COUNTRY
  * Soulwax FM                   | ID   6 | RADIO_07_DANCE_01
  * East Los FM                  | ID   7 | RADIO_08_MEXICAN
  * West Coast Classics          | ID   8 | RADIO_09_HIPHOP_OLD
  * Blue Ark                     | ID   9 | RADIO_12_REGGAE
  * Worldwide FM                 | ID  10 | RADIO_13_JAZZ
  * FlyLo FM                     | ID  11 | RADIO_14_DANCE_02
  * The Lowdown 91.1             | ID  12 | RADIO_15_MOTOWN
  * The Lab                      | ID  13 | RADIO_20_THELAB
  * Radio Mirror Park            | ID  14 | RADIO_16_SILVERLAKE
  * Space 103.2                  | ID  15 | RADIO_17_FUNK
  * Vinewood Boulevard Radio     | ID  16 | RADIO_18_90S_ROCK
  * Blonded Los Santos 97.8 FM   | ID  17 | RADIO_21_DLC_XM17
  * Blaine County Radio          | ID  18 | RADIO_11_TALK_02
  * Los Santos Underground Radio | ID  19 | RADIO_22_DLC_BATTLE_MIX1_RADIO
  * Self Radio                   | ID  20 | RADIO_19_USER
  * Radio Off                    | ID 255 | OFF
3. Paste the hash on the *Radio* or *Audio* section for radios and custom audio files respectively
4. Add the Radio ID or Audio filename on the section
5. For custom audio, put the file in the <GTA V>\scripts\PickYourTones
