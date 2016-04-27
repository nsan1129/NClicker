﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using NeverClicker.Properties;
//using AForge.Imaging;

namespace NeverClicker.Interactions {
	public static partial class Screen {
		public const string OUTPUT_VAR_X = "OutputVarX";
		public const string OUTPUT_VAR_Y = "OutputVarY";
		public const string ERROR_LEVEL = "ErrorLevel";
		//public const string OPTIONS = "*40";

		//public static Point ImageSearchAndClick(Interactor intr, string imgCode) {
		//	return new Point(0, 0);
		//}	

		/// <summary>
		/// Find an image and return it's upper left coordinate.
		/// </summary>
		/// This needs a crazy amount more error handling/reporting but it's just a huge pain. Most stuff is ignored.
		/// <param name="intr"></param>
		/// <param name="imgCode"></param>
		/// <param name="topLeft"></param>
		/// <param name="botRight"></param>
		/// <returns></returns>
		public static ImageSearchResult ImageSearch(Interactor intr, string imgCode, Point topLeft, Point botRight) {
			//ImageSearch, ImgX, ImgY, 1, 1, 1920, 1080, *40 % image_file %
			string imageFileName;
			//var imageFileName = intr.GameClient.GetSettingOrEmpty(imgCode + "_ImageFile", "SearchRectanglesAnd_ImageFiles");

			if (!intr.GameClient.TryGetSetting(imgCode + "_ImageFile", "SearchRectanglesAnd_ImageFiles", out imageFileName)) {
				//intr.Log("Image code prefix '" + imgCode + "' not found in settings ini file. Creating.", LogEntryType.Debug);
				imageFileName = imgCode + ".png";
				intr.GameClient.SaveSetting(imageFileName, imgCode + "_ImageFile", "SearchRectanglesAnd_ImageFiles");
			}

			var imageFilePath = Settings.Default.ImagesFolderPath + "\\" + imageFileName;

			intr.Log(new LogMessage("ImageSearch(" + imgCode + "): Searching for image: '" + imageFilePath + "'"
				+ " [TopLeft:" + topLeft.ToString()
				+ " BotRight:" + botRight.ToString() + "]",
				LogEntryType.Debug			
			));

			int outX = 0;
			int outY = 0;
			int errorLevel = 0;

			var imgSrcOptions = Settings.Default.ImageShadeVariation.ToString();

			intr.SetVar(OUTPUT_VAR_X, outX.ToString());
			intr.SetVar(OUTPUT_VAR_Y, outY.ToString());

			var statement = string.Format("ImageSearch, {0}, {1}, {2}, {3}, {4}, {5}, {6} {7}",
				 OUTPUT_VAR_X, OUTPUT_VAR_Y, topLeft.X.ToString(), topLeft.Y.ToString(), 
				 botRight.X.ToString(), botRight.Y.ToString(), "*" + imgSrcOptions, imageFilePath);

			//intr.Log(new LogMessage(""ImageSearch(" + imgCode + "): Executing: '" + statement + "'", LogEntryType.Detail));

			intr.Wait(20);
			intr.ExecuteStatement(statement);

			int.TryParse(intr.GetVar(OUTPUT_VAR_X), out outX);
			int.TryParse(intr.GetVar(OUTPUT_VAR_Y), out outY);
			int.TryParse(intr.GetVar(ERROR_LEVEL), out errorLevel);

			intr.Log(new LogMessage(
					"ImageSearch(" + imgCode + "): Results: "
					+ " OutputVarX:" + intr.GetVar(OUTPUT_VAR_X)
					+ " OutputVarY:" + intr.GetVar(OUTPUT_VAR_Y)
					+ " ErrorLevel:" + intr.GetVar(ERROR_LEVEL),
					LogEntryType.Debug					
			));

			//try {

			//	outX = int.Parse(intr.GetVar(OUTPUT_VAR_X));
			//	outY = int.Parse(intr.GetVar(OUTPUT_VAR_Y));
			//	errorLevel = int.Parse(intr.GetVar(ERROR_LEVEL));
			//} catch (Exception){
			//	throw new ProblemConductingImageSearchException("ImageSearch Results: "
			//		+ " OutputVarX:" + intr.GetVar(OUTPUT_VAR_X)
			//		+ " OutPutVarY:" + intr.GetVar(OUTPUT_VAR_Y)
			//		+ " ErrorLevel:" + intr.GetVar(ERROR_LEVEL)
			//	);
			//	//return new FindResult() { Found = false, At = new Point(0, 0) };
			//}

			switch (errorLevel) {
				case 0:
					intr.Log("ImageSearch(" + imgCode + "): Found.", LogEntryType.Debug);
					//return new ImageSearchResult() { Found = true, Point = new Point(outX, outY) };
					return new ImageSearchResult(true, new Point(outX, outY));
				case 1:
					intr.Log("ImageSearch(" + imgCode + "): Not Found.", LogEntryType.Debug);
					//return new ImageSearchResult() { Found = false, Point = new Point(outX, outY) };				
					return new ImageSearchResult(false, new Point(outX, outY));
				case 2:
					intr.Log("ImageSearch(" + imgCode + "): FATAL ERROR. UNABLE TO FIND IMAGE OR BAD OPTION FORMAT.", LogEntryType.Fatal);
					//return new ImageSearchResult() { Found = false, Point = new Point(outX, outY) };
					return new ImageSearchResult(false, new Point(outX, outY));
				default:
					intr.Log("ImageSearch(" + imgCode + "): Not Found.", LogEntryType.Fatal);
					//return new ImageSearchResult() { Found = false, Point = new Point(outX, outY) };
					//throw new ProblemConductingImageSearchException();
					return new ImageSearchResult(false, new Point(outX, outY));
			}
		}

		public static ImageSearchResult ImageSearch(Interactor intr, List<string> imgCodes, Point topLeft, Point botRight) {
			foreach (var imgCode in imgCodes) {
				var res = ImageSearch(intr, imgCode, topLeft, botRight);

				if (res.Found) {
					return res;
				}
			}
			return new ImageSearchResult();
		}

		public static ImageSearchResult ImageSearch(Interactor intr, string imgCode) {
			int scrWidth;
			int scrHeight;
			bool success = int.TryParse(intr.GetVar("A_ScreenWidth"), out scrWidth);
			success &= int.TryParse(intr.GetVar("A_ScreenHeight"), out scrHeight);

			if (success) {
				return ImageSearch(intr, imgCode, new Point(0, 0), new Point(scrWidth, scrHeight));
			} else {
				return new ImageSearchResult();
			}
		}

		public static ImageSearchResult ImageSearch(Interactor intr, List<string> imgCodes) {
			foreach (var imgCode in imgCodes) {
				var res = ImageSearch(intr, imgCode);
				if (res.Found) {
					return res;
				}
			}
			return new ImageSearchResult();
		}
	}

	public class ImageSearchResult {
		public Point Point;
		public bool Found;

		public ImageSearchResult(bool found, Point point) {
			Found = found;
			Point = point;
		}

		public ImageSearchResult() {
			Found = false;
			Point = new Point(0, 0);
		}
	}

	class ProblemConductingImageSearchException : Exception {
		public ProblemConductingImageSearchException() : base("There was a problem that prevented ImageSearch"  
			+ " from conducting the search(such as failure to open the image file or a badly formatted option)") { }
		public ProblemConductingImageSearchException(string message) : base(message) { }
		public ProblemConductingImageSearchException(string message, Exception inner) : base(message, inner) { }
	}
}


// ErrorLevel is set to 0 if the image was found in the specified region, 
// 1 if it was not found, or 2 if there was a problem that prevented the 
// command from conducting the search(such as failure to open the image file 
// or a badly formatted option).