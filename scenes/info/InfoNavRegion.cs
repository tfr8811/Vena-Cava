using func_godot.FGD;
using Godot;
using System;

[Tool]
public partial class InfoNavRegion : NavigationRegion3D, ICsFGD
{
    private StringName WORLD_GROUP = "world";
    
    public void _func_godot_apply_properties(Godot.Collections.Dictionary<string, Variant> entity_properties)
    {
        GD.Print("Building navmesh");
        GetParent().GetParent().AddToGroup(WORLD_GROUP);
    }

    public void _func_godot_build_complete()
    {
        NavigationMesh = new NavigationMesh();
        NavigationMesh.GeometrySourceGeometryMode = NavigationMesh.SourceGeometryMode.GroupsWithChildren;
        NavigationMesh.GeometrySourceGroupName = WORLD_GROUP;
        NavigationMesh.AgentMaxSlope = 20;
        NavigationMesh.AgentMaxClimb = 0.75f;
        NavigationMesh.AgentRadius = 0.75f;
        BakeNavigationMesh(false);
    }
}
