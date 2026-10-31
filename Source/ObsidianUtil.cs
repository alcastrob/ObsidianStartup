using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Obsidian_Startup
{
	public static class ObsidianUtil
	{
		public static readonly string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "obsidian", "obsidian.json");
		public static readonly string execPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Obsidian", "Obsidian.exe");
		public static ObsidianConfig? LoadVaults()
		{
			try { return File.Exists(configPath) ? JsonSerializer.Deserialize<ObsidianConfig>(File.ReadAllText(configPath)) : null; }
			catch (Exception e) when (e is IOException || e is JsonException) { return null; }
		}
		/// <summary>Shows a folder picker and returns the selected folder, or null if cancelled.</summary>
		public static string? PickFolder(string? initial)
		{
			using var dialog = new System.Windows.Forms.FolderBrowserDialog() { ShowNewFolderButton = true };
			if (!string.IsNullOrEmpty(initial) && Directory.Exists(initial))
				dialog.SelectedPath = initial;
			return dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK ? dialog.SelectedPath : null;
		}
		/// <summary>Opens a folder as a vault. Uses the vault id if registered, otherwise its path. Works even if Obsidian is already running.</summary>
		public static void OpenVaultFolder(string folder, ObsidianConfig? config)
		{
			string full = Path.GetFullPath(folder).TrimEnd('\\', '/');
			string? id = config?.vaults.FirstOrDefault(it => string.Equals(Path.GetFullPath(it.Value.path).TrimEnd('\\', '/'), full, StringComparison.OrdinalIgnoreCase)).Key;
			string uri = id != null
				? "obsidian://open?vault=" + Uri.EscapeDataString(id)
				: "obsidian://open?path=" + Uri.EscapeDataString(full);
			Process.Start(new ProcessStartInfo() { UseShellExecute = true, FileName = uri });
		}
		public static void SaveVaults(ObsidianConfig vaults) => File.WriteAllText(configPath, JsonSerializer.Serialize(vaults));
		public static void Launch() => Process.Start(new ProcessStartInfo() { UseShellExecute = true, FileName = execPath });
		public static void OpenVault(string name) => Process.Start(new ProcessStartInfo() { UseShellExecute = true, FileName = "obsidian://open-vault?vault=" + Uri.EscapeDataString(Path.GetFileName(name)) });
	}
	public class ObsidianConfig
	{
		public Dictionary<string, ObsidianVault> vaults { get; set; } = new();
	}
	public class ObsidianVault
	{
		public string path { get; set; } = "";
		public ulong ts { get; set; }
		public bool open { get; set; }
	}
}
