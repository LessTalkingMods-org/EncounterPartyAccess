# Party Screen In Encounter

A small Mount & Blade II: Bannerlord mod that re-enables the Party screen during
pre-battle encounters, for both the **P** hotkey and the campaign nav-bar button.

In the base game the Party button greys out the moment you get pulled into an
encounter. This mod lifts only that one restriction. Every other reason the game
blocks party access is left alone: being a prisoner, stuck on a raft, or in a
mission that disallows it all still apply. Works with or without the Naval DLC.
Singleplayer only.

## How it works

Both the **P** hotkey and the nav-bar button read
`PartyNavigationElement.GetPermission().IsAuthorized`. Stock logic returns
unauthorized whenever `MobileParty.MainParty.MapEvent != null` (the pre-battle /
encounter case). A Harmony postfix on `GetPermission` re-checks every other
blocker the original method applies and authorizes only when the active map event
was the sole reason access was denied.

## Requirements

- [Harmony](https://www.nexusmods.com/mountandblade2bannerlord/mods/2006) (`Bannerlord.Harmony`)

## Install

Drop the `EncounterPartyAccess` folder into your Bannerlord `Modules` folder and
enable it in the launcher.

## Building

Targets game version **v1.4.6** via the `Bannerlord.BUTRModule.Sdk` (reference
assemblies come from NuGet, so no local game install is needed to compile):

```
dotnet build EncounterPartyAccess.csproj -c Release -p:DeployToGameAfterBuild=false
```

Omit `-p:DeployToGameAfterBuild=false` to also deploy into the local game's
`Modules` folder via `scripts\bl-deploy.ps1`.

## Publishing

`.github/workflows/release.yml` builds the module, zips the `EncounterPartyAccess/`
layout, and (on a GitHub Release or manual dispatch, once the `NEXUS_FILE_ID` repo
variable is set) uploads it to Nexus Mods using the org `NEXUSMODS_API_KEY` secret.
