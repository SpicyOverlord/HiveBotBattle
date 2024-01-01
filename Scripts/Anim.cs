using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Godot;
using Godot.Collections;

public partial class Anim : Sprite2D
{
    [Export]
    public Array<Texture2D> sprites;
    [Export]
    public float fps = 4;

    private float frameInterval;
    private float _nextSpriteChange;

    private int _index;

    public Anim()
    {
        _index = 0;
        frameInterval = 1 / fps * 1000;
    }

    public override void _Process(double delta)
    {
        if (!(_nextSpriteChange < Time.GetTicksMsec())) return;

        _nextSpriteChange = Time.GetTicksMsec() + frameInterval;
        Texture = sprites[_index];
        _index++;

        if (_index >= sprites.Count) QueueFree();
    }
}