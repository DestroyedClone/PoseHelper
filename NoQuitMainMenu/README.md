# NoQuitMainMenu

Removes the quit button temporarily from the main menu while the window is unfocused, as I and others have experienced quitting the game by accident.

Due to limitations with OnApplicationFocus, these are the times when the button is hidden
1. Starting the game: Quit button hidden
2. Switching to the window and back: Quit button hidden
The second time you switch to the window, and for the rest of the time the game is launched, the quit button will return.

Adds a single config option to disable the whole menu instead of just the quit button (like when you accidentally click the logbook). Although it follows the same limitations as the quit button only. I guess even more of a limitation since you're stuck with just switching back and forth.

Thanks Gnome for the monobehaviour

![](https://i.imgur.com/EfFFFMj.png)

# Changelog
v1.0.1 - Quit button is only hidden once.
v1.0.0 - Just removes the quit button entirely