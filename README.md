# DiscordBotV5
The 5th (probably) iteration of my Discord bot attempt.

## What?
This repo contains a small "framework" for a bot. It only has 1 proper command, but it has some other nice features I 
personally wanted. In no particular order:

* **Command interpolation**: While you can use the bot normally using a prefix (eg. `h:help`), it is also possible to 
place commands in the middle of your message, name them, and execute multiple at once. An example would be like so:
    ```
    [User]
    There are {h: seconds | calc 365.25*24*3600} seconds in a year (on average), and a random word 
    is {h:rand beep boop imabot}.
    
    [HoLLyBot]
    `seconds` - 31557600
    `cmd1` - boop
    ```

* **Declarative command creation**: Boilerplate code is boring, so I made creating commands as easy as possible. Simply 
create a method like you normally would and add a CommandAttribute with the parameters you like. Behind the scenes all these methods get parsed and converted in commands that can be used by the bot.

  This is how the `rand` command is implemented:
  ```cs
  [Command("rand", "Picks a random word from a list")]
  public static string Random(string[] words) => words[new Random().Next(0, words.Length)];
  ```
  Note that the assembly containing these command definitions has to be named like `*.DiscordBot.Commands`.
  
* **Simple permission definition**: Permissions are defined in a custom format that should be easy to understand. User 
groups are assigned numbers that can be added to a command which will be required to execute it.

  ```yaml
  DEFAULTSERVER {
      DEFAULT: 0
      SERVEROWNER: 100
      USERID[113009395494830080]: 100
  }

  SERVERID[115858304915210245] {
      DEFAULT: 0
      ROLE[Regular]: 20
      ROLE[Trusted]: 40
      ROLE[Moderator]: 60
      SERVEROWNER: 100
  }

  ```
  
