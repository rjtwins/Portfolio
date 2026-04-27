using Godot;

public partial class RightPanelSettlementInfo : HBoxContainer
{
	private static PackedScene scene = GD.Load("uid://ctb60kmqyu3nm") as PackedScene;
	public static RightPanelSettlementInfo CreateNew(Settlement settlement)
	{
		var node = scene.Instantiate<RightPanelSettlementInfo>();
		node.settlement = settlement;
		node.FoldableContainer.Title = settlement.Data.Name;
		return node;
	}
	
	private Settlement settlement;
	[Export] Timer UpdateTimer;
	[Export] Label QueueNr;
	[Export] Label MetalNr;
	[Export] Label OilNr;
	[Export] Label ManNr;
	[Export] Label MunitNr;
	[Export] Label FundsNr;
	[Export] Button GoToSettlementButton;
	[Export] FoldableContainer FoldableContainer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		UpdateTimer.Timeout += () =>
		{		
			Visible = settlement.Data.Owner == Faction.PLAYER;
			
			if(!Visible)
				return;
			
			FundsNr.Text = settlement.Data.FundsProduction.ToString();
			ManNr.Text = settlement.Data.ManpowerProduction.ToString();
			QueueNr.Text = settlement.Data.ShipBuildQueue.Count.ToString();
			MetalNr.Text = settlement.Data.MetalProduction.ToString();
			OilNr.Text = settlement.Data.VolatilesProduction.ToString();
			MunitNr.Text = settlement.Data.MunitionsProduction.ToString();
		};
		
		UpdateTimer.Start();
		
		GoToSettlementButton.Pressed += GoToSettlementButtonPressed;
    }

    private void GoToSettlementButtonPressed()
    {
        var camera = GetViewport()?.GetCamera2D();
        if(camera == null)
			return;
			
		camera.GlobalPosition = settlement.GlobalPosition;
		settlement.Selectable.Select();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
		
	}
}
