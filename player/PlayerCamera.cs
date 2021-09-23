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
    public void Combine(ScreenLimits sl)
    {
        Top = Math.Min(sl.Top, Top);
        Left = Math.Min(sl.Left, Left);
        Bottom = Math.Max(sl.Bottom, Bottom);
        Right = Math.Max(sl.Right, Right);        
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
        screenLimits.Combine(newLimits);

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



    /*

    var trans := false
    var trans_pos : Vector2
    var trans_zoom  : Vector2
    signal transitioning
    signal end_trans

    onready var current_limits = { 'top' : limit_top, 'left' : limit_left, 'right' : limit_right, 'bottom' : limit_bottom }

    func transition(pos, new_limits, new_zoom := 5):
        $SoftLockPreventer.start()
        emit_signal("transitioning")
        trans = true
        trans_pos = pos
        trans_zoom = new_zoom * Vector2.ONE
        current_limits.top = min(new_limits.top, current_limits.top)
        current_limits.right = max(new_limits.right, current_limits.right)
        current_limits.bottom = max(new_limits.bottom, current_limits.bottom)
        current_limits.left = min(new_limits.left, current_limits.left)

        _set_limits(current_limits)
        current_limits = new_limits
        _set_drag_margins(0.0)
        smoothing_enabled = false
        get_tree().paused = true

    func end_transition():
        zoom = trans_zoom
        trans = false
        smoothing_enabled = true
        _set_limits(current_limits)
        _set_drag_margins(0.01)
        emit_signal("end_trans")
        get_tree().paused = false

    func _physics_process(delta):
        if trans :
            var pos = global_position
            if (pos.round() != trans_pos.round() or zoom.round() != trans_zoom.round()) and not $SoftLockPreventer.is_stopped()  :
                offset = lerp(offset, Vector2.ZERO, delta* 10)
                global_position = lerp(pos, trans_pos, delta* 20)
                zoom = lerp(zoom, trans_zoom, delta* 5)
            else :
                end_transition()


    func _set_drag_margins(v : float):
        drag_margin_bottom = v
        drag_margin_top = v
        drag_margin_right = v
        drag_margin_left = v

    func _set_limits(margins):
        limit_top = margins.top
        limit_bottom = margins.bottom
        limit_left = margins.left
        limit_right = margins.right
    */
}
