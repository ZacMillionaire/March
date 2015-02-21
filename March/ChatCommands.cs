using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace March {
	public static class ChatCommands {


		#region["Command List"]


		//TODO: Move this and other command related stuff to a new class
		private static List<CommandEntry> CommandList = new List<CommandEntry>
		{
			new CommandEntry
			{
				CommandName = "help",
				Usage = "!help <command>",
				Example = "!help yt",
				HelpMessage = "Let me help you to !help me so I can help you to help yourself",
				CommandClass = "March.ChatCommands",
				CommandMethod = "GetCommandHelp"
			},
			new CommandEntry
			{
				CommandName = "yt",
				Usage = "!yt <search term>",
				Example = "!yt Let The Night (Dualistic Remix)",
				HelpMessage = "Searches YouTube and returns the first result if found",
				CommandClass = "March.YouTubeSearch"
			},
			new CommandEntry
			{
				CommandName = "about",
				Usage = "!about",
				Example = "!about",
				HelpMessage = "My life story",
				CommandClass = "March.ChatCommands",
				CommandMethod = "GetBotInformation"
			},
			new CommandEntry
			{
				CommandName = "commands",
				Usage = "!commands",
				Example = "!commands",
				HelpMessage = "A command that lists commands",
				CommandClass = "March.ChatCommands",
				CommandMethod = "ReturnCommandList"
			},
			new CommandEntry
			{
				CommandName = "aq",
				Usage = "!aq <username> <quote text>, !aq <username>",
				Example = "!aq ScotchDrinker Some no doubt witty comment, !aq ScotchDrinker",
				HelpMessage = "[NYI] Adds a given quote to a given username, adds the last line said by the given username",
				CommandClass = "March.ChatCommands",
				CommandMethod = "AddQuote",
				Disabled = true
			},
			new CommandEntry
			{
				CommandName = "q",
				Usage = "!q <username> <quoteID>, !q <username>, !q",
				Example = "!q ScotchDrinker 1, !q ScotchDrinker, !q",
				HelpMessage = "[NYI] Pulls a given quoteID from a given username, pulls a random quote from given username, pulls any quote",
				CommandClass = "March.ChatCommands",
				CommandMethod = "FetchQuote",
				Disabled = true
			},
			new CommandEntry
			{
				CommandName = "link",
				Usage = "!link <url>",
				Example = "!link http://stackoverflow.com/questions/1732348/regex-match-open-tags-except-xhtml-self-contained-tags",
				HelpMessage = "[NYI] Saves a given link. Mysterious",
				CommandClass = "March.ChatCommands",
				CommandMethod = "SaveLink",
				Disabled = true
			},
			new CommandEntry
			{
				CommandName = "quit",
				PrivateCommand = true,
				CommandClass = "March.Program",
				CommandMethod = "QuitIRCServer"
			},
			new CommandEntry
			{
				CommandName = "leave",
				PrivateCommand = true,
				CommandClass = "March.Program",
				CommandMethod = "PartIRCChannel"
			},
			new CommandEntry
			{
				CommandName = "wm",
				Usage = "!wm <welcome message>, !wm",
				Example = "To set or change: !wm Welcome to the chat, To clear: !wm",
				HelpMessage = "Sets a welcome message for yourself. Using !wm on it's own will clear your welcome message",
				CommandClass = "March.ChatCommands",
				CommandMethod = "SetWelcomeMessage"
			}
		};
		#endregion


		public static Dictionary<string, string> WelcomeMessageDictionary = new Dictionary<string, string>();
		private static string welcomeFilePath = Directory.GetCurrentDirectory() + "\\welcomeMessages.txt";

		static ChatCommands() {

			bool welcomeFileExists = File.Exists(welcomeFilePath);
			if(!welcomeFileExists) {
				File.Create(welcomeFilePath);
			}

			string[] welcomeFileContents = File.ReadAllLines(welcomeFilePath);
			Regex welcomeMessageRegex = new Regex("(.*) = (.*)");
			foreach(string welcomeFileLine in welcomeFileContents) {
				Match regexMatch = welcomeMessageRegex.Match(welcomeFileLine);
				WelcomeMessageDictionary.Add(regexMatch.Groups[1].ToString(), regexMatch.Groups[2].ToString());
			}

		}


		/// <summary>
		/// Checks the chat message for instances of commands, then uses
		/// reflection to call the appropriate class/method
		/// </summary>
		/// <param name="message"></param>
		public static void ParseChatCommand(ChatMessage message) {

			Regex commandRegex = new Regex("^[!.]([a-zA-Z]+)(?: )?(.*)$");
			Match commandMatch = commandRegex.Match(message.Message);

			bool commandExists = CommandList.Any(s => s.CommandName == commandMatch.Groups[1].ToString());

			if(commandExists) {

				CommandEntry commandEntry = CommandList.First(s => s.CommandName == commandMatch.Groups[1].ToString());

				if(commandEntry.Disabled) {

					//TODO: clean this up
					Program.SendIRCMessage(
						string.Format(
							"PRIVMSG {0} :\x02!{1}\x02 is currently disabled.",
							message.Channel,
							commandEntry.CommandName
						)
					);
					return;
				} // End nested if

				// To prevent an ever growing list of switch cases, use reflection to call the
				// appropriate class/method

				Type commandInstance = Type.GetType(commandEntry.CommandClass, true);

				if(commandEntry.CommandMethod != null) {

					// If the command has a CommandMethod, invoke it and pass the message and command arguments.
					// The method to invoke MUST BE PUBLIC, and its signature MUST BE (ChatMessage, GroupCollection)
					commandInstance.InvokeMember(
						commandEntry.CommandMethod,
						BindingFlags.InvokeMethod,
						null,
						null,
						new object[]{
							message,
							commandMatch.Groups
						}
					);

				} else {

					// If there is no CommandMethod, then the command works by the Class's Constructor method,
					// this is basically the same as calling:
					// new ClassName(arg,[arg])
					// without knowing the ClassName ahead of time.
					// Again, the constructor must have a signature of (ChatMessage, GroupCollection).
					// Shouldn't need to tell you that constructors need to be public.
					Activator.CreateInstance(
						commandInstance,
						new object[]{
							message,
							commandMatch.Groups
						}
					);

				} // End nested ifelse
			} // End if
		} // End ParseChatCommand


		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		public static void ParseChatStatus(ChatMessage message) {
			if(message.Command == "JOIN") {
				bool userInWelcomeList = WelcomeMessageDictionary.ContainsKey(message.Username);
				if(userInWelcomeList) {
					//TODO: clean this up
					Program.SendIRCMessage(
						string.Format(
							"PRIVMSG {0} :{1}",
							message.Channel,
							WelcomeMessageDictionary[message.Username]
						)
					);
				}
			}
		} // End ParseChatStatus


		/// <summary>
		///	Invoked Method.
		/// 
		/// Sets a chat members welcome message.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="commandArgs"></param>
		public static void SetWelcomeMessage(ChatMessage message, GroupCollection commandArgs) {

			bool userInWelcomeList = WelcomeMessageDictionary.ContainsKey(message.Username);
			if(userInWelcomeList) {
				WelcomeMessageDictionary[message.Username] = commandArgs[2].ToString();
			} else {
				WelcomeMessageDictionary.Add(message.Username, commandArgs[2].ToString());
			}

			UpdateWelcomeMessageFile();
		} // End SetWelcomeMessage


		/// <summary>
		/// Updates the welcome message file to reflect changes made.
		/// </summary>
		private static void UpdateWelcomeMessageFile() {

			IEnumerable<string> newLines;

			newLines = WelcomeMessageDictionary.Where(s => s.Value != "").Select(s => s.Key + " = " + s.Value);

			File.WriteAllLines(welcomeFilePath, newLines);

		} // End UpdateWelcomeMessageFile

		/// <summary>
		/// Invoked Method.
		/// 
		/// Sends a message listing all non-private commands.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="commandArgs"></param>
		public static void ReturnCommandList(ChatMessage message, GroupCollection commandArgs) {

			string commandList = string.Join(", ", CommandList.Where(s => s.PrivateCommand == false).Select(s => "!" + s.CommandName));

			// TODO: clean this up
			Program.SendIRCMessage(
				String.Format(
					"PRIVMSG {0} :" + (char)2 + "Command List:" + (char)2 + " {1}",
					message.Channel,
					commandList
				)
			);
			Program.SendIRCMessage(
				String.Format(
					"PRIVMSG {0} :" + (char)2 + "Command List:" + (char)2 + " {1}",
					message.Channel,
					"Commands can also be called using a . instead of a !"
				)
			);
		} // End ReturnCommandList


		/// <summary>
		/// Invoked Method.
		/// 
		/// Sends a message containing the usage of a given non-private command
		/// </summary>
		/// <param name="message"></param>
		/// <param name="commandArgs"></param>
		public static void GetCommandHelp(ChatMessage message, GroupCollection commandArgs) {

			string helpMessage;
			string searchCommand = commandArgs[2].ToString().Trim();

			if(searchCommand == "") {

				helpMessage = String.Format(
					"PRIVMSG {0} :\x02!{1} usage:\x02 {2}. {3}.",
					message.Channel,
					CommandList[0].CommandName,
					CommandList[0].Usage,
					CommandList[0].HelpMessage
				);

			} else {

				bool inCommandList = CommandList.Any(s => s.CommandName == searchCommand && s.PrivateCommand == false);

				if(inCommandList) {

					CommandEntry commandEntry = CommandList.First(s => s.CommandName == searchCommand);

					helpMessage = String.Format(
						"PRIVMSG {0} :\x02!{1} usage:\x02 {2}. {3}. Example: {4}",
						message.Channel,
						commandEntry.CommandName,
						commandEntry.Usage,
						commandEntry.HelpMessage,
						commandEntry.Example
					);

				} else {

					helpMessage = String.Format(
						"PRIVMSG {0} :Unable to find \x02!{1}\x02 in my command list.",
						message.Channel,
						searchCommand
					);
				} // End nested ifelse
			} // End ifelse

			Program.SendIRCMessage(helpMessage);
		} // End GetCommandHelp


		/// <summary>
		/// Invoked Method.
		/// 
		/// Sends details about the bot.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="commandArgs"></param>
		public static void GetBotInformation(ChatMessage message, GroupCollection commandArgs) {
			Program.SendIRCMessage(
				string.Format(
					"PRIVMSG {0} :MarchBot v{1} written by {2} in C#, can be found at {3}. \"Fork it to me\"",
					message.Channel,
					ConfigurationManager.AppSettings["version"],
					ConfigurationManager.AppSettings["author"],
					ConfigurationManager.AppSettings["github"]
				)
			);
		} // End GetBotInformation
	}
}
