void WorldToScreen_float(in float3 WorldSpaceViewDirection, out float2 ScreenCoords)
{
    // float3 v = TransformWorldToHClipDir(WorldSpaceViewDirection, true);
    ScreenCoords = WorldSpaceViewDirection.xy;
}