# CS2 Turnbinds

## Overview

The CS2 Turnbinds program is a customizable tool designed to assist gamers in executing precise turning movements in games. It allows users to configure specific yaw speeds, mouse sensitivity, and key bindings to automate mouse movements for turning. This is functionality that used to be in the game in CounterStrike Source and is needed to complete many surf maps.

## Configuration

The program reads its configuration from a `config.json` file, which it will create with default settings if it does not already exist. You can customize this file to suit your preferences. Here are the configurable options:

- `Yaw`: A list of integers representing the yaw speeds you can toggle between.
- `Sensitivity`: Your mouse sensitivity. (Match to your in game value)
- `M_Yaw`: The yaw factor that determines how much the view angles change per mouse movement. (Match to your in game value)
- `Left`: The key used to move the mouse left.
- `Right`: The key used to move the mouse right.
- `Toggle`: The key used to toggle between different yaw speeds.
- `Pause`: The key used to pause and unpause the program.
- `Inc`: The key used to increase the current yaw speed.
- `Dec`: The key used to decrease the current yaw speed.
- `Exit`: The key used to exit the program.

## Usage

1. Run the program once, it will make `config.json` with default keybinds.
1. Ensure the `config.json` file is set up with your preferred settings.
2. Restart the program for your changes to `config.json` to take effect.
3. Use the configured keys to control the program during your game. You can toggle yaw speeds, pause the program, adjust yaw speeds on-the-fly, and more.

## Key Bindings

When configuring keys in `config.json`, use the key names from the [Virtual-Key Codes](https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes) documentation without the `VK_` prefix. For example, use `LBUTTON` for the left mouse button, `RBUTTON` for the right mouse button, `A` for the "A" key, etc.


## Credits

 - [t5mat's CS:GO Turnbinds](https://github.com/t5mat/turnbinds) - This program is based on t5mat's original CS:GO Turnbinds program.
 - Spaz - Figuring out how to get mouse movement in CS2
 - [oce.surf Community](https://oce.surf) - Testing
