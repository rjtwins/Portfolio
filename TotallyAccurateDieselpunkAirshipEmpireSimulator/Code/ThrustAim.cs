using Godot;

public static class ThrustAim
{
    public struct Solution
    {
        public Vector3 ThrustDir;
        public float GimbalAngle;
        public bool Feasible;
        public float Tmin;
        public float T_required;
    }

    public static Solution Compute(
        Vector3 position,
        Vector3 target,
        Vector3 gravity,
        float mass,
        float T_max,
        float gimbalMaxRad,          // ✅ NEW
        float T_chosen = float.NaN
    )
    {
        var dVec = target - position;
        var d = dVec.Normalized();
        var g = gravity;
        float gMag = g.Length();

        // T_min = m * |g x d|
        float sinThetaMag = g.Cross(d).Length();
        float Tmin = mass * sinThetaMag;

        bool feasibleThrust = T_max >= Tmin;

        float T = feasibleThrust
            ? (float.IsNaN(T_chosen) ? Tmin : Mathf.Clamp(T_chosen, Tmin, T_max))
            : T_max;

        // ----- Step 1: Compute unconstrained direction -----
        Vector3 u_req;
        if (feasibleThrust)
        {
            float s = Mathf.Sqrt(Mathf.Max(0f, (T / mass) * (T / mass) - sinThetaMag * sinThetaMag));
            float lambda = g.Dot(d) + s;
            u_req = (lambda * d - g).Normalized();
        }
        else
        {
            Vector3 gPerp = g - d * g.Dot(d);
            u_req = gPerp.Length() > 1e-6f ? (-gPerp).Normalized() : (-g).Normalized();
        }

        // ----- Step 2: Apply gimbal limit -----
        Vector3 up = (-g).Normalized();
        float angle = Mathf.Acos(Mathf.Clamp(u_req.Dot(up), -1f, 1f));

        Vector3 u_final = u_req;

        if (angle > gimbalMaxRad)
        {
            // Clamp onto cone surface
            float t = gimbalMaxRad / angle;
            u_final = up.Slerp(u_req, t).Normalized();
        }

        // ----- Step 3: Compute thrust required for this constrained direction -----
        float finalAngle = Mathf.Acos(Mathf.Clamp(u_final.Dot(up), -1f, 1f));
        float T_required = mass * gMag / Mathf.Cos(finalAngle);

        bool feasible = T_required <= T_max;

        return new Solution
        {
            ThrustDir = u_final,
            GimbalAngle = finalAngle,
            Feasible = feasible,
            Tmin = Tmin,
            T_required = T_required
        };
    }
}