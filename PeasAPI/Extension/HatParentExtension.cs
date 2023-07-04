using System;
using System.Collections.Generic;

namespace PeasAPI.Extensions;

public static class HatParentExtensions
{
    public static Dictionary<string, HatViewData> HatViewDataRegistry = new();
    public static HatViewData GetViewData(this HatParent parent)
    {
        if (!parent || !parent.Hat) return null;
        return HatViewDataRegistry.TryGetValue(parent.Hat.ProductId, out var asset) ? asset : parent.hatDataAsset?.GetAsset();
    }

    public static bool IsModdedHat(this HatParent parent)
    {
        if (!parent || !parent.Hat) return false;
        return HatViewDataRegistry.ContainsKey(parent.Hat.ProductId);
    }
}