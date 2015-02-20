using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;

namespace March {

	#region["Object Definitions"]

	class ChatMessage {
		public string Username { get; set; }
		public string Location { get; set; }
		public string Command { get; set; }
		public string Channel { get; set; }
		public string Message { get; set; }
	} // End class ChatMessage


	class CommandEntry {
		public string CommandName { get; set; }
		public string Usage { get; set; }
		public string Example { get; set; }
		public string HelpMessage { get; set; }
		public string CommandClass { get; set; }
		public string CommandMethod { get; set; }

		[System.ComponentModel.DefaultValue(false)]
		public bool PrivateCommand { get; set; }

		[System.ComponentModel.DefaultValue(false)]
		public bool Disabled { get; set; }

	} // End class CommandEntry
	#endregion

	class MessageParser {

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
				CommandClass = "March.MessageParser",
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
				CommandClass = "March.MessageParser",
				CommandMethod = "GetBotInformation"
			},
			new CommandEntry
			{
				CommandName = "commands",
				Usage = "!commands",
				Example = "!commands",
				HelpMessage = "A command that lists commands",
				CommandClass = "March.MessageParser",
				CommandMethod = "ReturnCommandList"
			},
			new CommandEntry
			{
				CommandName = "aq",
				Usage = "!aq <username> <quote text>, !aq <username>",
				Example = "!aq ScotchDrinker Some no doubt witty comment, !aq ScotchDrinker",
				HelpMessage = "[NYI] Adds a given quote to a given username, adds the last line said by the given username",
				CommandClass = "March.ChatQuote",
				CommandMethod = "AddQuote",
				Disabled = true
			},
			new CommandEntry
			{
				CommandName = "q",
				Usage = "!q <username> <quoteID>, !q <username>, !q",
				Example = "!q ScotchDrinker 1, !q ScotchDrinker, !q",
				HelpMessage = "[NYI] Pulls a given quoteID from a given username, pulls a random quote from given username, pulls any quote",
				CommandClass = "March.ChatQuote",
				CommandMethod = "FetchQuote",
				Disabled = true
			},
			new CommandEntry
			{
				CommandName = "link",
				Usage = "!link <url>",
				Example = "!link http://stackoverflow.com/questions/1732348/regex-match-open-tags-except-xhtml-self-contained-tags",
				HelpMessage = "[NYI] Saves a given link. Mysterious",
				CommandClass = "March.ChatQuote",
				CommandMethod = "SaveLink",
				Disabled = true
			},
			new CommandEntry{
				CommandName = "quit",
				PrivateCommand = true,
				CommandClass = "March.Program",
				CommandMethod = "QuitIRCServer"
			}
		};
		#endregion

		private static StreamReader IrcReader { get; set; }
		private static StreamWriter IrcWriter { get; set; }
		private static string IrcServer { get; set; }


		public MessageParser(StreamReader reader, StreamWriter writer, string server) {
			//TODO: remove the need for these, writer and server have already been removed
			IrcReader = reader;
			IrcWriter = writer;
			IrcServer = server;
		} // End constructor


		/// <summary>
		/// 'Main' loop of the bot, listens to the reader and parses new lines received
		/// </summary>
		public void ListenForMessages() {

			string line;

			while(( line = IrcReader.ReadLine() ) != null) {
				ParseReceivedLine(line);
			} // End while
		} // End ListenForMessages


		/// <summary>
		/// Parses a given line received from the IRC Server
		/// </summary>
		/// <param name="receivedLine"></param>
		private void ParseReceivedLine(string receivedLine) {

			//Console.WriteLine(receivedLine);
			// Need to respond to PING to prevent being timed out
			if(receivedLine.Contains("PING :")) {
				Program.PingIRCServer();
			} else {
				ParseChatMessage(ExtractChatMessageDetails(receivedLine));
			} // End ifelse
		} // End ParseReceivedLine


		/// <summary>
		/// Formats a line into a message object for better access later
		/// </summary>
		/// <param name="chatline"></param>
		/// <returns></returns>
		private ChatMessage ExtractChatMessageDetails(string chatline) {

			Regex lineRegex = new Regex(":(.*)!(.*) ([A-Z]+) (#.*) :(.*)");
			Match lineMatch = lineRegex.Match(chatline);

			ChatMessage chatMessage = new ChatMessage {
				Username = lineMatch.Groups[1].ToString(),
				Location = lineMatch.Groups[2].ToString(),
				Channel = lineMatch.Groups[4].ToString(),
				Command = lineMatch.Groups[3].ToString(),
				Message = lineMatch.Groups[5].ToString()
			};

			return chatMessage;
		} // End ExtractChatMessageDetails


		/// <summary>
		/// Checks the chat message for instances of commands, then uses
		/// reflection to call the appropriate class/method
		/// </summary>
		/// <param name="message"></param>
		private void ParseChatMessage(ChatMessage message) {

			Regex commandRegex = new Regex("!([a-zA-Z]+)(?: )?(.*)$");
			Match commandMatch = commandRegex.Match(message.Message);

			bool commandExists = CommandList.Any(s => s.CommandName == commandMatch.Groups[1].ToString());

			if(commandExists) {

				CommandEntry commandEntry = CommandList.First(s => s.CommandName == commandMatch.Groups[1].ToString());

				if (commandEntry.Disabled){

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
		} // End ParseChatMessage


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
				string.Format(
					"PRIVMSG {0} :" + (char)2 + "Command List:" + (char)2 + " {1}",
					message.Channel,
					commandList
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

				helpMessage = string.Format(
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

					helpMessage = string.Format(
						"PRIVMSG {0} :\x02!{1} usage:\x02 {2}. {3}. Example: {4}",
						message.Channel,
						commandEntry.CommandName,
						commandEntry.Usage,
						commandEntry.HelpMessage,
						commandEntry.Example
					);

				} else {

					helpMessage = string.Format(
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
			Program.SendIRCMessage("PRIVMSG " + message.Channel + " :MarchBot v0.1 written by Scotch in C#. \"Have you tried using jQuery?\"");
		}
	} // End class MessageParser


} // End namespace March
