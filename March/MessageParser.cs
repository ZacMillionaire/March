using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;


namespace March {


	class MessageParser {
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
				Console.WriteLine(line);
				Debug.WriteLine(line);
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
				ChatMessage parsedMessage = ExtractChatMessageDetails(receivedLine);
				if (parsedMessage.Message != null){
					ChatCommands.ParseChatCommand(parsedMessage);
				}
				else{
					ChatCommands.ParseChatStatus(parsedMessage);
				}
			} // End ifelse
		} // End ParseReceivedLine


		/// <summary>
		/// Formats a line into a message object for better access later
		/// </summary>
		/// <param name="chatline"></param>
		/// <returns></returns>
		private ChatMessage ExtractChatMessageDetails(string chatline) {

			ChatMessage chatMessage;

			Regex lineRegex = new Regex(":(.*)!(.*) ([A-Z]+) (#.*) :(.*)");
			Regex systemRegex = new Regex(":(.*)!(.*) ([A-Z]+) (#[a-zA-Z]+)$");

			Match lineMatch = lineRegex.Match(chatline);
			Match systemMatch = systemRegex.Match(chatline);

			if(lineMatch.Groups.Count != 1) {
				chatMessage = new ChatMessage {
					Username = lineMatch.Groups[1].ToString(),
					Location = lineMatch.Groups[2].ToString(),
					Channel = lineMatch.Groups[4].ToString(),
					Command = lineMatch.Groups[3].ToString(),
					Message = lineMatch.Groups[5].ToString()
				};
			} else {
				chatMessage = new ChatMessage {
					Username = systemMatch.Groups[1].ToString(),
					Location = systemMatch.Groups[2].ToString(),
					Channel = systemMatch.Groups[4].ToString(),
					Command = systemMatch.Groups[3].ToString()
				};
			}

			return chatMessage;
		} // End ExtractChatMessageDetails


	} // End class MessageParser


} // End namespace March
