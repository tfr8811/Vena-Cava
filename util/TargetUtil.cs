using Godot;
using System;
using System.Collections.Generic;

public static class TargetUtil
{
    public static Node GetNodeByTargetName<T>(Node self, string targetName)
    {
        if (string.IsNullOrEmpty(targetName))
        {
            GD.PushError($"{self.Name} - Empty target provided");
            return null;
        }

        Node targetNode = self.GetParent().GetNode($"entity_{targetName}");

        if (targetNode == null)
        {
            GD.PushError($"{self.Name} - Target node \"{targetName}\" not found");
            return null;
        }

        if (!(targetNode is T))
        {
            GD.PushError($"{self.Name} - Target {targetName} does not have interface {typeof(T).FullName}");
            return null;
        }

        return targetNode;
    }

    public static Node[] GetNodesByTargetName<T>(Node self, string[] targetNames)
    {
        List<Node> validTargets = new List<Node>();
        foreach (string targetName in targetNames)
        {
            Node node = GetNodeByTargetName<T>(self, targetName);
            if (node != null)
            {
                validTargets.Add(node);
            }
        }
        return validTargets.ToArray();
    }
}
