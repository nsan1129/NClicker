﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeverClicker.Interactions {
	public static partial class Keyboard {
		public static void Send(Interactor intr, string key) {
			SendInput(intr, key);
		}

		public static void SendKey(Interactor intr, string key) {
			SendInput(intr, "{ " + key + " }");
		}

		public static void KeyPress(Interactor intr, string key) {
			intr.ExecuteStatement("Send { " + key + " down }");
			intr.Wait(70);
			intr.ExecuteStatement("Send { " + key + " up }");
			intr.Wait(20);
		}

		public static void SendInput(Interactor intr, string key) {
			//intr.Wait(200);
			intr.ExecuteStatement("SendInput " + key);
			intr.Wait(20);
		}

		public static void SendPlay(Interactor intr, string keys) {
			intr.ExecuteStatement("SendPlay " + keys);
			intr.Wait(20);
		}

		public static void SendEvent(Interactor intr, string keys) {
			intr.ExecuteStatement("SendEvent " + keys);
			intr.Wait(20);
		}

		public static void SendTest(Interactor intr, string key) {
			intr.Wait(3000);
			SendInput(intr, key);
		}
	}
}
