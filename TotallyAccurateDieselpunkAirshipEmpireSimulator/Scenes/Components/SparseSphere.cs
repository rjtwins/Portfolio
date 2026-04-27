using Godot;
using System;
using System.Collections.Generic;

public partial class SparseSphere : MeshInstance3D
{
    // Input points on the sphere
    public List<Vector3> Points = new List<Vector3>();
    
    // Total possible points (to determine patch size)
    public int TotalPoints = 1000;
    
    public float SphereRadius = 200f;
    
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        // var mesh = GenerateSparseSphere(Points, TotalPoints, SphereRadius);
        // Mesh = mesh;
    }
    
    public void Generate(List<Vector3> points, int totalPoints, float radius)
    {
		this.Points = points;
		this.TotalPoints = totalPoints;
		this.SphereRadius = radius;
    
		if (Mesh != null)
			this.Mesh.Dispose();
		
		var mesh = GenerateSparseSphere(Points, TotalPoints, SphereRadius);
        Mesh = mesh;
    }

    private Mesh GenerateSparseSphere(List<Vector3> points, int totalPoints, float radius)
    {
        SurfaceTool st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);

        // Determine patch size based on total points
        float patchRadius = Mathf.Sqrt(4 * Mathf.Pi * radius * radius / totalPoints) * 0.5f;

        int subdivisions = 5; // You can increase for smoother patches

        foreach (Vector3 point in points)
        {
            AddSpherePatch(st, point, patchRadius, subdivisions);
        }

        return st.Commit();
    }
    
	private void AddSpherePatch(SurfaceTool st, Vector3 center, float patchRadius, int subdivisions)
    {
        for (int i = 0; i < subdivisions; i++)
        {
            float theta1 = i * Mathf.Pi / subdivisions;
            float theta2 = (i + 1) * Mathf.Pi / subdivisions;

            for (int j = 0; j < subdivisions * 2; j++)
            {
                float phi1 = j * Mathf.Tau / (subdivisions * 2);
                float phi2 = (j + 1) * Mathf.Tau / (subdivisions * 2);

                Vector3 p1 = SphericalToCartesian(theta1, phi1, patchRadius) + center;
                Vector3 p2 = SphericalToCartesian(theta2, phi1, patchRadius) + center;
                Vector3 p3 = SphericalToCartesian(theta2, phi2, patchRadius) + center;
                Vector3 p4 = SphericalToCartesian(theta1, phi2, patchRadius) + center;

                // Two triangles
                st.AddVertex(p1);
                st.AddVertex(p2);
                st.AddVertex(p3);

                st.AddVertex(p1);
                st.AddVertex(p3);
                st.AddVertex(p4);
            }
        }
    }
    
	private Vector3 SphericalToCartesian(float theta, float phi, float r)
    {
        return new Vector3(
            r * Mathf.Sin(theta) * Mathf.Cos(phi),
            r * Mathf.Cos(theta),
            r * Mathf.Sin(theta) * Mathf.Sin(phi)
        );
    }
}
