namespace GameRoot;

using Godot;
using Godot.Collections;
using System;
using System.Linq;
public partial class SavedGame : Resource
{
    public static string DefaultPath = OS.GetUserDataDir();
    [Export] public string Filename;
    [Export] public string VersionControl = (string)ProjectSettings.GetSetting("application/config/version");
    [Export] public string EngineVersion;
    [Export] public string LastDatetime;
    [Export] public double Timestamp;
    [Export] public GameSettings GameSettings;

    public void UpdateLastDatetime()
    {
        Dictionary datetime = Time.GetDatetimeDictFromSystem();
        LastDatetime = $"{datetime["day"].ToString().PadZeros(2)}/" +
                           $"{datetime["month"].ToString().PadZeros(2)}/" +
                           $"{datetime["year"].ToString().PadZeros(2)} " +
                           $"{datetime["hour"].ToString().PadZeros(2)}:" +
                           $"{datetime["minute"].ToString().PadZeros(2)}";

        Timestamp = Time.GetUnixTimeFromSystem();
    }

    public void WriteSavegame(string filename)
    {
        EngineVersion = $"Godot {Engine.GetVersionInfo()}";
        UpdateLastDatetime();
        ResourceSaver.Save(this, GetSavePath(filename.GetBaseName()));
    }


    public void Delete()
    {
        if (SaveExists(Filename))
        {
            Error error = DirAccess.RemoveAbsolute(GetSavePath(Filename));

            if (error != Error.Ok)
                GD.PushError($"An error happened trying to delete the file {Filename} with code {error}");

        }
    }
    public static string GetSavePath(string filename)
    {
        return $"{DefaultPath}/{filename.GetBaseName()}.{GetSaveExtension()}";
    }

    public static string GetSaveExtension()
    {
        return OS.IsDebugBuild() ? "tres" : "res";
    }

    public static bool SaveExists(string filename)
    {
        return ResourceLoader.Exists(GetSavePath(filename));
    }

    public static SavedGame LoadSaveGame(string filename)
    {
        if (SaveExists(filename))
        {
            return ResourceLoader.Load<SavedGame>(GetSavePath(filename), "", ResourceLoader.CacheMode.Ignore);
        }

        return null;
    }

    public static Dictionary<string, SavedGame> ReadUserSavedGames()
    {
        string[] validExtensions = new string[] { GetSaveExtension() };

        Dictionary<string, SavedGame> savedGames = new();
        DirAccess directory = DirAccess.Open(DefaultPath);

        if (directory is not null)
        {
            directory.ListDirBegin();
            string filename = directory.GetNext();

            while (filename.Length > 0)
            {
                if (!directory.CurrentIsDir() && validExtensions.Contains(filename.GetExtension()))
                {
                    SavedGame savedGame = LoadSaveGame(filename.GetBaseName());

                    if (savedGame is not null)
                        savedGames.Add(savedGame.Filename, savedGame);
                }

                filename = directory.GetNext();
            }

            directory.ListDirEnd();
        }

        return savedGames;
    }
}
