using System;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.IO.Ports;

namespace MocapReceiver {
	class Program {

		struct Vector3 {
			float x, y, z;
			public const int FLOAT_SCALAR = 1;

			public static Vector3 Zero { get { return new Vector3(0, 0, 0); } }

			public Vector3(int x, int y, int z) {
				this.x = x;
				this.y = y;
				this.z = z;
			}

			public Vector3(float x, float y, float z) {
				this.x = (int)(x * FLOAT_SCALAR);
				this.y = (int)(y * FLOAT_SCALAR);
				this.z = (int)(z * FLOAT_SCALAR);
			}

			public override string ToString() {
				return $"({x}, {y}, {z})";
			}
		}

		private static SerialPort port;
		private const int BAUD_RATE = 115200;
		private static bool running = true;
		private static Dictionary<string, Vector3> vectorDict;

		static void Main(string[] args) {
			Initialize();
			while (running)
				Loop();
		}

		private static void Initialize() {
			SetUpVectors();
			port = GetPortFromCommand();
			Console.CancelKeyPress += SafeExit;
			Console.WriteLine("Please press any key to start the recording...");
			Console.ReadKey(true);
		}

		private static void SetUpVectors() {
			vectorDict = new Dictionary<string, Vector3>();
			string[] names = { "MinPos", "MaxPos", "MinRot", "MaxRot" };
			
			for (int i = 0; i < names.Length; i++)
				vectorDict.Add(names[i], Vector3.Zero);
		
		}

		private static void SafeExit(object sender, ConsoleCancelEventArgs e) {
			Console.WriteLine("Terminating application...");
			running = false;
			port?.Close();
			Console.WriteLine("Resources disposed...");
			Environment.Exit(0);
		}

		private static SerialPort GetPortFromCommand() {
			try {
				Console.WriteLine("Select the port through which we'll comminicate with the MCU:");
				string[] availablePorts = SerialPort.GetPortNames();
				for (int i = 0; i < availablePorts.Length; i++) {
					Console.WriteLine($"{(char)(65 + i)}) {availablePorts[i]}");
				}
				char selection = Console.ReadKey(true).KeyChar;
				selection = char.ToUpper(selection);
				Console.WriteLine("Preparing Arduino interface...");
				var res = new SerialPort(availablePorts[selection - 65]) {
					BaudRate = BAUD_RATE
				};
				res.Open();
				Thread.Sleep(1000);
				return res;
			}
			catch(Exception err) {
				Console.WriteLine($"There was an error selecting the COM port, {err.Message}");
				Console.WriteLine("Press any key to try again...");
				Console.ReadKey(true);
				Console.Clear();
				return GetPortFromCommand();
			}
		}

		private static void Loop() {
			//Let's do a raw reading first
			if (port.BytesToRead > 0) {
				try {
					string[] lines = port.ReadLine().Split("\t");
					string[] dNames = { "Rotation", "Position" };
					for (int i = 0; i < lines.Length; i++) {
						string[] raw = lines[i].Split(" ");
						Vector3 data = new Vector3(float.Parse(raw[0]), float.Parse(raw[1]), float.Parse(raw[2]));
						Console.WriteLine(dNames[i] + ": " + data);
					}
				}
				catch(Exception err) {
					Console.WriteLine($"\tError Log: {err.Message}");
				}
			}
		}

		private static void UpdateMaxAndMinValues() {
		}

	}
}
