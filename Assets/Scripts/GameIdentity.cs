using System;
using System.IO;
using UnityEngine;

public static class GameIdentity
{
    public const string Title = "Against Fate: Saving Constantinople";
    public const string ShortTitle = "Saving Constantinople";
    public const string MenuTitle = "AGAINST FATE\n<size=68%>SAVING CONSTANTINOPLE</size>";
    public const string LegacyProduct = "Besieged in Constantinopole";

    // The display name changes; existing campaign formats and preference keys do not.
    public static string LegacyDataPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low",
        Application.companyName, LegacyProduct);

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    [System.Runtime.InteropServices.DllImport("advapi32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    static extern int RegOpenKeyEx(IntPtr key, string path, uint options, int access, out IntPtr result);
    [System.Runtime.InteropServices.DllImport("advapi32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    static extern int RegEnumValue(IntPtr key, uint index, System.Text.StringBuilder name, ref uint nameLength,
        IntPtr reserved, out uint type, byte[] data, ref uint dataLength);
    [System.Runtime.InteropServices.DllImport("advapi32.dll")]
    static extern int RegCloseKey(IntPtr key);
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ImportPreferences()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        if (PlayerPrefs.GetInt("SavingConstantinople.IdentityImported", 0) >= 2) return;
        try
        {
            IntPtr old;
            int result = RegOpenKeyEx(new IntPtr(unchecked((int)0x80000001)),
                "Software\\" + Application.companyName + "\\" + LegacyProduct, 0, 0x20019, out old);
            if (result != 0 && result != 2) throw new IOException("Registry read failed: " + result);
            if (result == 0) try
            {
                for (uint index = 0; ; index++)
                {
                    var name = new System.Text.StringBuilder(1024);
                    uint nameLength = 1024, type, dataLength = 8192;
                    byte[] data = new byte[dataLength];
                    result = RegEnumValue(old, index, name, ref nameLength, IntPtr.Zero, out type, data, ref dataLength);
                    if (result == 259) break;
                    if (result != 0) throw new IOException("Registry value read failed: " + result);
                    string storedName = name.ToString();
                    int hash = storedName.LastIndexOf("_h", StringComparison.Ordinal);
                    string key = hash > 0 ? storedName.Substring(0, hash) : storedName;
                    if (!key.StartsWith("Besieged", StringComparison.Ordinal) || PlayerPrefs.HasKey(key)) continue;
                    if (key == "Besieged.MusicVolume" && dataLength == 4)
                        PlayerPrefs.SetFloat(key, BitConverter.ToSingle(data, 0));
                    else if (key == "Besieged.MusicVolume" && dataLength == 8)
                        PlayerPrefs.SetFloat(key, (float)BitConverter.ToDouble(data, 0));
                    else if (type == 4 && dataLength == 4) PlayerPrefs.SetInt(key, BitConverter.ToInt32(data, 0));
                    else if (type == 1) PlayerPrefs.SetString(key, System.Text.Encoding.Unicode.GetString(data, 0, (int)dataLength).TrimEnd('\0'));
                    else if (type == 3) PlayerPrefs.SetString(key, System.Text.Encoding.UTF8.GetString(data, 0, (int)dataLength).TrimEnd('\0'));
                }
            }
            finally { RegCloseKey(old); }
            PlayerPrefs.SetInt("SavingConstantinople.IdentityImported", 2);
            PlayerPrefs.Save();
        }
        catch (Exception e) { Debug.LogWarning("Previous game preferences could not be imported: " + e.Message); }
#endif
    }
}
