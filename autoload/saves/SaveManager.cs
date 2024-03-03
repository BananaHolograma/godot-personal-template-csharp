namespace GameRoot;

using Godot;
using Godot.Collections;

public partial class SaveManager : Node
{
    [Signal]
    public delegate void RemovedSavedGameEventHandler(string filename);
    [Export] public Dictionary<string, SavedGame> ListOfSavedGames;
    [Export] public SavedGame CurrentSavedGame;

    public override void _Ready()
    {
        ListOfSavedGames = SavedGame.ReadUserSavedGames();
    }

    public void Remove(string filename)
    {
        if (ListOfSavedGames.TryGetValue(filename, out var _))
        {
            ListOfSavedGames.Remove(filename);
            EmitSignal(SignalName.RemovedSavedGame);

        }
        GD.PushError("$Trying to remove a saved game with name {filename} that does not exists in the list of saved games");
    }
}