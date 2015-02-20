using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace March {
	class Program {

		//TODO: Move this stuff to a config file
		public const string YoutubeAPIKey = null;
		public const string BotOwner = "ScotchDrinker";

		private const string Server = "irc.freenode.org";
		private const int Port = 6667;
		private const string BotNick = null;
		private const string BotPassword = null;
		private const string IrcChannel = "#qutcode";

		private static TcpClient _irc;
		private static NetworkStream _ircStream;
		private static StreamReader _ircReader;
		private static StreamWriter _ircWriter;

		//TODO: probably delete this
		#region["Control Character Reference"]
		/*
		 * control character reference
		class ControlCharacters {
			//Controls
			public static char NORMAL = (char)15;
			public static char BOLD = (char)2;
			public static char UNDERLINE = (char)31;
			public static char REVERSE = (char)22;
			public static char COLOUR = (char)3;
			//Forground
			public static string WHITE = "00";
			public static string BLACK = "01";
			public static string DARK_BLUE = "02";
			public static string DARK_GREEN = "03";
			public static string RED = "04";
			public static string BROWN = "05";
			public static string PURPLE = "06";
			public static string OLIVE = "07";
			public static string YELLOW = "08";
			public static string GREEN = "09";
			public static string TEAL = "10";
			public static string CYAN = "11";
			public static string BLUE = "12";
			public static string MAGENTA = "13";
			public static string DARK_GRAY = "14";
			public static string LIGHT_GRAY = "15";
			//Background
			public static string BG_WHITE = ",00";
			public static string BG_BLACK = ",01";
			public static string BG_DARK_BLUE = ",02";
			public static string BG_DARK_GREEN = ",03";
			public static string BG_RED = ",04";
			public static string BG_BROWN = ",05";
			public static string BG_PURPLE = ",06";
			public static string BG_OLIVE = ",07";
			public static string BG_YELLOW = ",08";
			public static string BG_GREEN = ",09";
			public static string BG_TEAL = ",10";
			public static string BG_CYAN = ",11";
			public static string BG_BLUE = ",12";
			public static string BG_MAGENTA = ",13";
			public static string BG_DARK_GRAY = ",14";
			public static string BG_LIGHT_GRAY = ",15";
		}
		 */
		#endregion

		static void Main(string[] args) {
			Connect();
		} // End Main


		/// <summary>
		/// Connects to an IRC server using configuration options.
		/// </summary>
		public static void Connect() {

			_irc = new TcpClient(Server, Port);

			_ircStream = _irc.GetStream();
			_ircReader = new StreamReader(_ircStream);
			_ircWriter = new StreamWriter(_ircStream) { NewLine = "\r\n", AutoFlush = true };

			// Send connection details, then jump into the chatReader loop
			SendIRCMessage("NICK " + BotNick);
			SendIRCMessage("USER " + BotNick + " 0 * :March Bot");
			// If a password was given, attempt to identify with the server
			if(BotPassword != null) {
				// Sending a PRIVMSG won't work for identify. Why? Well you see,
				SendIRCMessage("NickServ identify " + BotNick + " " + BotPassword);
			}
			SendIRCMessage("JOIN " + IrcChannel);

			MessageParser chatReader = new MessageParser(_ircReader, _ircWriter, Server);
			chatReader.ListenForMessages();

			_ircStream.Dispose();
			_ircReader.Dispose();
			_ircWriter.Dispose();

		} // End Connect


		/// <summary>
		/// Sends a given string to the connected server
		/// 
		/// TODO: create seperate commands for PRIVMSG, PART etc
		/// </summary>
		/// <param name="message"></param>
		public static void SendIRCMessage(string message) {

			_ircWriter.WriteLine(message);
			_ircWriter.Flush();

		} // End SendIRCMessage


		/// <summary>
		/// Returns PONG to the server to prevent timeout
		/// </summary>
		public static void PingIRCServer() {

			Console.WriteLine("Got PING Event, sending PONG");
			_ircWriter.WriteLine("PONG " + Server);
			_ircWriter.Flush();

		} // End PingIRCServer


		/// <summary>
		/// Invoked Method.
		/// 
		/// Quits the connected IRC Server.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="commandArgs"></param>
		public static void QuitIRCServer(ChatMessage message, GroupCollection commandArgs) {

			if(message.Username == BotOwner) {
				SendIRCMessage("QUIT :http://i.imgur.com/OpFcp.jpg");
			} // End if
		} // End QuitIRCServer


	} // End class Program


} // End namespace March
