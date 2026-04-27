using Godot;
using System;

public partial class EconInfo : HBoxContainer
{
	[Export]
	public Timer UpdateTimer {get; set;}
	
	[Export] public Label Funds {get; set;}
	[Export] public Label Metals {get; set;}
	[Export] public Label Volatiles {get; set;}
	[Export] public Label Munitions {get; set;}
	[Export] public Label Manpower {get; set;}
	
	private Settlement _settlement;

	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		UpdateTimer.Timeout += Update;
		_settlement = Owner as Settlement;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
	
	public void Update()
	{
		Funds.Text = _settlement.Data.FundsProduction.ToString();
		Manpower.Text = _settlement.Data.ManpowerProduction.ToString();
		Metals.Text = _settlement.Data.MetalProduction.ToString();
		Volatiles.Text = _settlement.Data.VolatilesProduction.ToString();
		Munitions.Text = _settlement.Data.MunitionsProduction.ToString();
	}
}
