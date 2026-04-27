using System.Text.Json.Serialization;
using Godot;

public class ComponentBluePrint
{
    public string SceneFilePath { get; set; }
    [JsonIgnore]
    public Vector3 LocalPosition { get; set; }
    
    public float[] LocalPositionArray 
    {
        get 
        {
            return new float[] { LocalPosition.X, LocalPosition.Y, LocalPosition.Z };
        }
        set
        {
            LocalPosition = new Vector3(value[0], value[1], value[2]);
        }
    }
    
    [JsonIgnore]
    public Vector3 LocalRotation { get; set; }
    
    public float[] LocalRotationArray 
    {
        get 
        {
            return new float[] { LocalRotation.X, LocalRotation.Y, LocalRotation.Z };
        }
        set
        {
            LocalRotation = new Vector3(value[0], value[1], value[2]);
        }
    }
}