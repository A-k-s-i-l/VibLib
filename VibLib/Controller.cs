using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace VibLib
{
	/// <summary>
	/// Main controller class for managing vibration strength based on simple Ellens states. <para/>
	/// </summary>
	public class Controller
	{
		#region Singleton
		/// <summary>
		/// Use this to access the singleton instance of the <see cref="Controller"/> all over the game.
		/// </summary>
		public static Controller Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new Controller();
				}
				return _instance;
			}
		}
		private static Controller _instance = null;

		private Controller()
		{

			_udpSender = new UdpSender(IP, Port);
			FadingEnabled = true;

			ReloadSettings();

			_congigChange = File.GetLastWriteTime("VibLib_Settings.cfg");

			_instance = this;
		}


		#endregion

		/// <summary>
		/// Current strength of the controller vibration. 1 is full strength and 0 is no vibrations <para/>
		/// Its value is clamped between 0 and <see cref="MaxStrength"/>. <para/>
		/// If Ellen is <see cref="Overwhelmed"/>, increases in strength are amplified by <see cref="OverwhelmMultiplier"/> and <see cref="OverwhelmTimer"/> is extended by <see cref="OverwhemlmAddTime"/>.
		/// </summary>
		public float Strength
		{
			get => _strength;
			set
			{


				if (Overwhelmed && Clamp(value, 0, MaxStrength) > _strength)
				{
					OverwhelmTimer += OverwhemlmAddTime;
					value *= OverwhelmMultiplier;
				}


				_strength = Clamp(value, 0, MaxStrength);

			}
		}
		/// <summary>
		/// If true, the strength will fade over time at a rate of <see cref="StrengthFalloff"/> per second. If Ellen is <see cref="Overwhelmed"/>, the strength will fade at a rate of <see cref="OverwhelmFalloff"/> per second.
		/// </summary>
		public bool FadingEnabled { get; set; }
		/// <summary>
		/// Determines multiplayers. <para/>
		/// </summary>
		public bool Overwhelmed => OverwhelmTimer > 0;
		/// <summary>
		/// Time remaining for which Ellen is considered <see cref="Overwhelmed"/>. <para/>
		/// </summary>
		public float OverwhelmTimer
		{
			get => _overwhelmTimer;
			set
			{
				_overwhelmTimer = value;
				if (_overwhelmTimer < 0) _overwhelmTimer = 0;
			}
		}

		private float _overwhelmTimer;
		private float _strength;
		private UdpSender _udpSender;
		private DateTime _congigChange;

		/// <summary>
		/// Call this method every frame to update the controller state and send the current strength via UDP.
		/// </summary>
		public void Update()
		{
			if(AutoReloadSettings)
				WatchConfigFile();

			if (OverwhelmTimer > 0)
				OverwhelmTimer -= Time.deltaTime;

			if (Strength >= OverwhelmThreshold && !Overwhelmed)
			{
				Overwhelm();
			}


			if (FadingEnabled)
			{
				Strength -= (Overwhelmed ? OverwhelmFalloff : StrengthFalloff) * Time.deltaTime;
			}


			_udpSender.SendMessage(Strength);
		}

		/// <summary>
		/// Makes Ellen overwhelmed for at least <see cref="OverwhelmDuration"/> seconds.
		/// </summary>
		public void Overwhelm()
		{
			if (OverwhelmTimer < OverwhelmDuration)
				OverwhelmTimer += OverwhelmDuration;

		}

		/// <summary>
		/// Call this method on each thrust to increase <see cref="Strength"/> by <see cref="ThrustStrength"/>. <para/>
		/// /// But this is just a example implementation. You can always do <see cref="Strength"/> += something, but this is unified.<para/>
		/// </summary>
		public void Thrust()
		{
			Strength += ThrustStrength;
		}

		/// <summary>
		/// Call when something is done with Ellen, to increase <see cref="Strength"/> by <see cref="CumshotStrenghth"/> and <see cref="OverwhelmTimer"/> by <see cref="CumshotOverwhelm"/>. <para/>
		/// But this is just a example implementation.<para/>
		/// </summary>
		public void Cumshot()
		{
			Strength += CumshotStrenghth;
			OverwhelmTimer += CumshotOverwhelm;
		}

		/// <summary>
		/// On Ellens orgasm (idk its up to you when), apply <see cref="Overwhelm"/> and add <see cref="EllenOrgasmStrength"/> to <see cref="Strength"/>. <para/>
		/// But this is just a example implementation.<para/>
		/// </summary>
		public void Orgasm()
		{
			Strength += EllenOrgasmStrength;
			Overwhelm();
		}


		private void WatchConfigFile() 
		{
			if (!File.Exists("VibLib_Settings.cfg"))
			{
				SaveValues();

				_congigChange = File.GetLastWriteTime("VibLib_Settings.cfg");
			}

			if (File.GetLastWriteTime("VibLib_Settings.cfg") == _congigChange)
				return;

			ReloadSettings();
			_congigChange = File.GetLastWriteTime("VibLib_Settings.cfg");
		}


		#region Settings

		private static int _settingsVersion = 1;

		// Modifyable unified settings, can be changed in runtime for specific use cases.

		/// <summary>
		/// If you want to cap <see cref="Strength"/>. <para/>
		/// </summary>
		public static float MaxStrength = float.MaxValue;
		/// <summary>
		/// How much should <see cref="Strength"/> should be decreased per second. <para/>
		/// </summary>
		public static float StrengthFalloff = 0.4f;

		/// <summary>
		/// How much should be instantly added to <see cref="Strength"/> on cumshot in <see cref="Cumshot"/>. <para/>
		/// </summary>
		public static float CumshotStrenghth = 0.55f;
		/// <summary>
		///  How much should be instantly added to <see cref="OverwhelmTimer"/> on cumshot in <see cref="Cumshot"/>. <para/>
		/// </summary>
		public static float CumshotOverwhelm = 1.25f;

		/// <summary>
		///  How much should be instantly added to <see cref="Strength"/> on orgasm in <see cref="Orgasm"/>. <para/>
		/// </summary>
		public static float EllenOrgasmStrength = 0.7f;

		/// <summary>
		///  How much should be instantly added to <see cref="Strength"/> on thrust in <see cref="Thrust"/>. <para/>
		/// </summary>
		public static float ThrustStrength = 0.155f;

		/// <summary>
		/// On how much <see cref="Strength"/> is Ellen considered overwhelmded. <para/>
		/// </summary>
		public static float OverwhelmThreshold = 0.8f;
		/// <summary>
		/// How much is added on <see cref="Overwhelm"/>
		/// </summary>
		public static float OverwhelmDuration = 4.5f;
		/// <summary>
		/// How much is <see cref="Strength"/> multiplied on change, that is greater then previous value. <para/>
		/// </summary>
		public static float OverwhelmMultiplier = .8f;
		/// <summary>
		///  How much should <see cref="Strength"/> should be decrased per second when overwhelmed. <para/>
		/// </summary>
		public static float OverwhelmFalloff = 0.1f;
		/// <summary>
		/// How much time is added to <see cref="OverwhelmTimer"/> on increase of <see cref="Strength"/> when <see cref="Overwhelmed"/>. <para/>
		/// </summary>
		public static float OverwhemlmAddTime = 0.25f;

		public static string IP = "127.0.0.1";
		public static int Port = 54321;

		public static bool AutoReloadSettings = true;

		
		#endregion

		private static float Clamp(float value, float min, float max) => Math.Max(Math.Min(value, max), min);

		private static void LoadValues()
		{
			bool updateSettings = false;
			bool versionChecked = false;

			var fields = typeof(Controller).FindMembers(System.Reflection.MemberTypes.Field, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static, null, null)
			.OfType<FieldInfo>();


			var lines = File.ReadAllLines("VibLib_Settings.cfg");
			foreach (var line in lines)
			{
				if (line.StartsWith("#")) continue;

				var parts = line.Split('=');
				if (parts.Length != 2) continue;
				var fieldName = parts[0].Trim();
				var fieldValue = parts[1].Trim();

				var field = fields.FirstOrDefault(f => f.Name == fieldName);

				if (field == null)
					continue;

				if(field.Name == "_settingsVersion")
				{
					int fileVersion = 0;
					try
					{
						fileVersion = int.Parse(fieldValue);
					}
					catch
					{
						Debug.LogWarning("Failed to parse settings version from config file.");
					}

					versionChecked = true;

					if (fileVersion != _settingsVersion)
					{
						updateSettings = true;
						Debug.LogWarning($"Settings version mismatch. File version: {fileVersion}, Expected version: {_settingsVersion}. Updating your config file.");
					}
					continue;
				}

				try
				{
					if (field.FieldType == typeof(float))
					{
						field.SetValue(null, float.Parse(fieldValue));
					}
					else if (field.FieldType == typeof(int))
					{
						field.SetValue(null, int.Parse(fieldValue));
					}
					else if (field.FieldType == typeof(string))
					{
						field.SetValue(null, fieldValue);
					}
					else if (field.FieldType == typeof(bool))
					{
						field.SetValue(null, bool.Parse(fieldValue));
					}
				}
				catch (Exception ex)
				{
					Debug.LogWarning($"Failed to parse value for field {fieldName}: {ex.Message}");
				}
			}

			if (updateSettings||!versionChecked)
			{
				SaveValues();
			}
		}

		private static void SaveValues()
		{
			var fields = typeof(Controller).FindMembers(System.Reflection.MemberTypes.Field, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static, null, null)
			.OfType<FieldInfo>();

			using (var writer = new StreamWriter("VibLib_Settings.cfg"))
			{
				writer.WriteLine("# VibLib Settings Configuration File");
				foreach (var field in fields)
				{
					var value = field.GetValue(null);
					writer.WriteLine($"{field.Name}={value}");
				}
			}
		}

		public static void ReloadSettings()
		{

			if (File.Exists("VibLib_Settings.cfg"))
			{
				LoadValues();
			}
			else
			{
				SaveValues();
			}
		}
	}
}
