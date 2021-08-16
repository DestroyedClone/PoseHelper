# Skip To Outro Text
Successor to the now deprecated MoonToOutro, because the name was horrible.

Allows you to view your outro flavortext by sending you to the outro with the defined flavortext. Mostly intended for character creation devs to see how their outro text looks.

You must be in at least the lobby, to activate the command, just doing it on the title screen will yield no result. Because this is done through a command, it means that if you view the outro normally, then you're not affected, probably.
The outro is supposed to go to the credits after a certain amount of time, but by skipping ahead like this it never does and eventually it'll just stop refreshing the frames and you know it when you see it. Nothing to worry about.

### Command
There's two versions of the command: `show_outro`:
* `show_outro {BodyName} {win/fail}`
	* Bodyname must match an existing bodyname that belongs to a survivor (such as `EngiBody`).
	* `win` will show the flavortext from surviving the ending sequence. (Defaults to `win`, if this argument is missing)
	* `fail` will show the flavortext from dying on the moon while your teammates survive the ending sequence.
* `show_outro custom {string}`
	* If you have a bunch of text you want to test, then you can put `custom` instead of the bodyname to have the next argument be any string. Just be sure to enclose it in quotation marks if it has spaces.
		* If your text has any quotation marks (") escape it with a backslash (\) as you would putting the string in normally, otherwise you'll close your quoted string early.

### Examples
* `show_outro EngiBody` : Shows the Engineer's win quote
* `show_outro HuntressBody fail` : Shows the Huntress's fail quote
* `show_outro custom "<color=red>red text</color>"` : Shows the end flavortext as `red text` colored red.
* `show_outro custom "<style=cDeath><sprite name=\"Skull\" tint=1> Choose a new character? <sprite name=\"Skull\" tint=1></style>"`
	* Note the backslashes before the quotation marks.
![](https://cdn.discordapp.com/attachments/471781153607647232/876741824272805928/unknown.png)