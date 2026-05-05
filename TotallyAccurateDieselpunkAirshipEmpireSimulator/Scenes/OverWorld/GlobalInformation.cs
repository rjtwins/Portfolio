using Godot;
using System;

public partial class GlobalInformation : Control
{
	[Export] Label Funds {get; set;}
	[Export] Label Manpower {get; set;}
	[Export] Label Metals {get; set;}
	[Export] Label Volatiles {get; set;}
	[Export] Label Munitions {get; set;}
	[Export] Label Time {get; set;}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Funds.Text = $"{globals.Funds} ፬";
		Manpower.Text = $"{globals.Manpower} ቷ";
		Metals.Text = $"{globals.Metals} ቷ";
		Volatiles.Text = $"{globals.Metals} ቷ";
		Munitions.Text = $"{globals.Metals} ቷ";
		
		Time.Text = $"{globals.HourOfDay}:{globals.MinOfHour}:{globals.SecOfMin}";
	}
}
