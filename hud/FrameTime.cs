using Godot;

namespace upgradedspoon.hud;

public partial class FrameTime : RichTextLabel
{
	public override void _Process(double delta)
	{
		// 1. Using the Performance Singleton (Recommended)
		// Performance.Monitor.TimeProcess retrieves total frame process time in seconds
		var frameTimeSec = Performance.GetMonitor(Performance.Monitor.TimeProcess);
		var frameTimeMs = frameTimeSec * 1000.0;

		// 2. Alternative: Using Engine FPS
		var fps = Engine.GetFramesPerSecond();

		// Update display text
		Text = $"Frame Time: {frameTimeMs:F2} ms\nFPS: {fps}";
	}
}
