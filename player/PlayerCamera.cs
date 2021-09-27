using Godot;
using System;

public class ScreenLimits : Godot.Object
{
    public ScreenLimits() { }
	public ScreenLimits(Vector2 position, Vector2 sizeLimits)
    {
		Top =   (int)position.y;
        Bottom = (int)(position.y + sizeLimits.y);
		Left = (int)position.x;
        Right = (int)(position.x + sizeLimits.x);
    }
	public int Top { get; set; }
	public int Bottom { get; set; }
	public int Left { get; set; }
	public int Right { get; set; }
    public ScreenLimits Combine(ScreenLimits sl)
    {
        return new ScreenLimits()
        {
            Top = Math.Min(sl.Top, Top),
            Left = Math.Min(sl.Left, Left),
            Bottom = Math.Max(sl.Bottom, Bottom),
            Right = Math.Max(sl.Right, Right)
        };      
    }
    public bool IsOutsideLimits(Vector2 v)
    {
        return (v.x < Left || v.x > Right || v.y < Top || v.y > Bottom);
    }
    public override string ToString()
    {
        return $"Top: {Top}, Bottom {Bottom}, Left {Left}, Right {Right}";
    }
}



public class PlayerCamera : Camera2D
{
	bool IsTransitioning = false;
	Vector2 TransPostion;
	Vector2 TransZoom;
	[Signal]
	public delegate void Transitioning();
	[Signal]
	public delegate void EndTransition();
    public ScreenLimits screenLimits;
    Timer SoftLockPreventer;

    public override void _Ready()
    {
        base._Ready();
        screenLimits = new ScreenLimits() { Top = LimitTop, Bottom = LimitBottom, Left = LimitLeft, Right = LimitRight };
        SoftLockPreventer = GetNode<Timer>("SoftLockPreventer");
    }

    public void Transition(Vector2 position, ScreenLimits newLimits, int newZoom = 5)
    {
        SoftLockPreventer.Start();
        EmitSignal(nameof(Transitioning));
        IsTransitioning = true;
        TransPostion = position;
        TransZoom = newZoom * Vector2.One;
        screenLimits =  screenLimits.Combine(newLimits);

        SetLimits(screenLimits);
        screenLimits = newLimits;
        SetDragMargin(0);
        SmoothingEnabled = false;
        GetTree().Paused = true;
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        if (IsTransitioning)
        {
            var pos = GlobalPosition;
            if (pos.Round() != TransPostion.Round() || Zoom.Round() != TransZoom.Round() || SoftLockPreventer.IsStopped())
            {
                Offset = Offset.LinearInterpolate(Vector2.Zero, delta * 10);
                GlobalPosition = GlobalPosition.LinearInterpolate(TransPostion, delta * 20);
                Zoom.LinearInterpolate(TransZoom, delta * 5);
            }
            else
            {
                StopTransitioning();
            }
        }
    }

    private void StopTransitioning()
    {
        Zoom = TransZoom;
        IsTransitioning = false;
        SmoothingEnabled = true;
        SetLimits(screenLimits);
        SetDragMargin(0.01f);
        EmitSignal(nameof(EndTransition));
        GetTree().Paused = false;
    }

    private void SetDragMargin(float v)
    {
        DragMarginBottom = v;
        DragMarginTop = v;
        DragMarginLeft = v;
        DragMarginRight = v;
    }

    private void SetLimits(ScreenLimits sl)
    {
        LimitTop = sl.Top;
        LimitBottom = sl.Bottom;
        LimitLeft = sl.Left;
        LimitRight = sl.Right;
    }

}
