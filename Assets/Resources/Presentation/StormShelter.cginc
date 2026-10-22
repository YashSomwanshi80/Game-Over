#ifndef GAMEOVER_STORM_SHELTER
#define GAMEOVER_STORM_SHELTER
// Keep roof volumes consistent with StormWeatherRules.Sheltered.
float StormBox(float3 p,float3 a,float3 b)
{float3 v=step(a,p)*step(p,b);return v.x*v.y*v.z;}
float StormSheltered(float3 p)
{
    float garden=StormBox(p,float3(-16.2,-4,-5.2),float3(-10,3.3,5.2));
    float h=StormBox(p,float3(-10.2,-4,-7.2),float3(10.2,6.9,7.2));
    h=max(h,StormBox(p,float3(10.2,-4,-7.2),float3(16.2,3.4,7.2)));
    h=max(h,StormBox(p,float3(-2.2,-4,-9.2),float3(2.2,3.2,-7.2)));
    h=max(h,StormBox(p,float3(-12.2,-4,-4.2),float3(-10.2,3.4,4.2)));
    h=max(h,StormBox(p,float3(-12.2,-4,-4.2),float3(-9.8,6.6,4.2)));
    h=max(h,StormBox(p,float3(-1.6,-4,-9.2),float3(1.6,6.6,-6.8)));
    return max(max(h,garden),1-step(-.12,p.y));
}
#endif
