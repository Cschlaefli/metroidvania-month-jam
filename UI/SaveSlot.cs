using Godot;
using Godot.Collections;
using System;

public class SaveSlot : Control
{
	[Export]
	PackedScene StartScene;

    Dictionary info;

    [Export]
    string SaveFileName = "debug";
    bool Empty = false;

    Button DeleteButton;
    Button LoadButton;

    Label NewGame;

    public override void _Ready()
    {
        base._Ready();
        var name = GetNode<Label>("Name");
        name.Text = SaveFileName;
        DeleteButton = GetNode<Button>("Delete");
        LoadButton = GetNode<Button>("Button");
        NewGame = GetNode<Label>("NewGame");
        UpdateSaves();
    }

    public void UpdateSaves()
    {
        info = Globals.Load(SaveFileName);

        Empty = (info == null || info.Count == 0);
        NewGame.Visible = Empty;
        DeleteButton.Visible = !Empty;
        DeleteButton.Disabled = Empty;
    }

    public void OnClick()
    {
        if (Empty)
        {
            StartGame(StartScene);
        }
        else
        {
            Globals.SetCurrentSave(SaveFileName);
            Globals.SetLoadBuffer(SaveFileName);
            var startAreaPath = Globals.LoadBuffer["CurrentArea"] as string;
            var area = GD.Load<PackedScene>(startAreaPath);
            StartGame(area);
        }
        GetTree().Paused = false;
        this.QueueFree();
    }

    public void StartGame(PackedScene scene)
    {
        Globals.SetCurrentSave(SaveFileName);
        Globals.CurrentArea?.QueueFree();
        GetTree().ChangeSceneTo(scene);
    }

    public void Delete()
    {

        Globals.Delete(SaveFileName);
        UpdateSaves();
    }
}
