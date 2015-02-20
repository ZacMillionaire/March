using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;

namespace March {
	class YouTubeSearch {

		public YouTubeSearch() {} // End constructor


		public YouTubeSearch(ChatMessage message, GroupCollection commandArgs){

			string searchTerm = commandArgs[2].ToString().Trim();

			if (searchTerm == ""){
				Program.SendIRCMessage(
					string.Format(
						"PRIVMSG {0} :" + ( (char)3 + "00,04" ) + " Youtube " + (char)3 + " Search term cannot be empty.",
						message.Channel
					)
				);
				return;
			} // End if

			//TODO: clean this up
			Program.SendIRCMessage(
				string.Format(
					"PRIVMSG {0} :" + ((char)3 + "00,04") + " Youtube " + (char)3 +" Searching for \x02{1}\x02",
					message.Channel,
					searchTerm
				)
			);
			Dictionary<string, string> searchResults = SearchYoutube(searchTerm).Result;
			ReturnYoutubeResult(message.Channel, searchTerm, searchResults);

		} // End constructor overload


		/// <summary>
		/// Performs an async search for the given string using the youtubeAPI
		/// </summary>
		/// <param name="searchTerm"></param>
		/// <returns>Returns a Dictionary containing the title and videoID if found, null otherwise</returns>
		private async Task<Dictionary<string, string>> SearchYoutube(string searchTerm) {

			var youtubeService = new YouTubeService(new BaseClientService.Initializer() {
				ApiKey = Program.YoutubeAPIKey,
				ApplicationName = GetType().ToString()
			});

			var searchListRequest = youtubeService.Search.List("snippet");
			searchListRequest.Type = "video";
			searchListRequest.Q = searchTerm;
			searchListRequest.MaxResults = 1;

			var searchListResponse = await searchListRequest.ExecuteAsync();

			if(searchListResponse.Items.Count != 0) {

				return new Dictionary<string, string>
				{
					{"title", searchListResponse.Items[0].Snippet.Title},
					{"videoID", searchListResponse.Items[0].Id.VideoId}
				};
			} // End if

			return null;

		} // End async SearchYoutube


		/// <summary>
		/// Sends a formatted message containing the results of the search to the server
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="searchTerm"></param>
		/// <param name="searchResult"></param>
		private static void ReturnYoutubeResult(string channel, string searchTerm, Dictionary<string, string> searchResult) {

			string chatMessage;

			if(searchResult != null) {

				chatMessage = string.Format(
						"PRIVMSG {0} :" + ((char)3 + "00,04") + " Youtube " + (char)3 +" \x02{1}\x02 - https://www.youtube.com/watch?v={2}",
						channel,
						searchResult["title"],
						searchResult["videoID"]
				);
				Program.SendIRCMessage(chatMessage);

			} else {

				chatMessage = string.Format(
					"PRIVMSG {0} :" + ( (char)3 + "00,04" ) + " Youtube " + (char)3 + " No video found for: \x02{1}\x02",
					channel,
					searchTerm
				);
				Program.SendIRCMessage(chatMessage);

			} // End if
		} // End ReturnYoutubeResult


	} // End class YouTubeSearch


} // End namespace March
