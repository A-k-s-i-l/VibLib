# VibLib

A simple library with a few pre-made example methods for Intiface integration for modding Unity games.

## Requirements
- [Intiface](https://intiface.com)
- [AQEBridge](https://github.com/A-k-s-i-l/AQEBridge)
- .NET 3.5 SDK (if you want to modify/compile this library)
- `Assembly-CSharp.dll` from your game (if you want to modify/compile this library)

## How to use

This library on its own does nothing. It acts as a link in the chain between Intiface and your game. You need to integrate this library into your game, which shouldn’t be too difficult. There are a few methods and base logic you can use, or you can modify the library to fit your needs.

Start Intiface, then start [AQEBridge](https://github.com/A-k-s-i-l/AQEBridge). The bridge handles communication with Intiface and expects raw numeric data, which this DLL will send from your game.

## How to implement

In your Visual Studio project, link this library so you can use it. There is only one class for you to interact with: `Controller.cs`.

`Controller.cs` is a singleton, so you can access its methods and properties anywhere using `Controller.Instance`. There are also some configuration `public static` fields. You can change their values in your game, but if you do, I’d recommend disabling auto-update from the config file. You can also add more fields, and they will be automatically added to the config file via reflection.

In some game manager class or another object that is constantly updated, call `Controller.Instance.Update()`. This ensures the game will send data to [AQEBridge](https://github.com/A-k-s-i-l/AQEBridge).

Throughout your game code, you can call specific methods or add your own to change strength or other properties. I recommend looking at the DLL code, as it contains documentation to explain what each method does.

Also, place this DLL somewhere Unity can link it to your game, such as `gameName_data/Managed`.

## Config

This library will create its own config file: `VibLib_Settings.cfg`. This file contains various strengths and falloffs (documented in the code). It is read at startup, and by default, any changes to this file will be applied instantly in-game.

If you add any `public static` fields to `Controller.cs`, they will also be added to the config file. All these fields are considered configurable by the user without recompiling the library. (Adding new fields would still require recompilation.)
