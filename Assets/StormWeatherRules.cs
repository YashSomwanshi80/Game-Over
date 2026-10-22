using System;

// Pure schedule shared by either LAN role. Weather never alters mains or AI state.
public static class StormWeatherRules
{
    public static double Seed(int index,int salt)
    {unchecked{uint n=(uint)(index*374761393+salt*668265263);n=(n^(n>>13))*1274126177;return (n^(n>>16))/(double)uint.MaxValue;}}
    public static double StrikeTime(int index)=>8+index*22+Seed(index,1)*7;
    public static double Distance(int index)=>280+Seed(index,2)*850;
    public static double ThunderTime(int index)=>StrikeTime(index)+Distance(index)/343;
    public static bool ShouldThunder(int index,double time,int lastIndex)
    {return index>=0&&index>lastIndex&&time>=ThunderTime(index)&&time-ThunderTime(index)<.75;}
    public static int LatestStrike(double time)
    {if(time<8)return -1;int n=(int)((time-8)/22);return time>=StrikeTime(n)?n:n-1;}
    public static float Flash(double age,bool reduced)
    {
        if(age<0||age>1)return 0;
        if(reduced)return (float)(.10*Math.Exp(-age*4));
        double first=Math.Exp(-age*22),second=age<.18?0:.62*Math.Exp(-(age-.18)*18);
        return (float)Math.Min(1,first+second);
    }
    public static bool Sheltered(float x,float y,float z)
    {
        if(y<-.12f)return true;
        if(x>=-12.2f&&x<=-9.8f&&Math.Abs(z)<=4.2f&&y<6.6f)return true;
        if(Math.Abs(x)<=1.6f&&z>=-9.2f&&z<=-6.8f&&y<6.6f)return true;
        if(x>=-16.2f&&x<=-10&&z>=-5.2f&&z<=5.2f&&y<3.3f)return true;
        if(x>=-10.2f&&x<=10.2f&&z>=-7.2f&&z<=7.2f&&y<6.9f)return true;
        if(x>=10.2f&&x<=16.2f&&z>=-7.2f&&z<=7.2f&&y<3.4f)return true;
        if(Math.Abs(x)<2.2f&&z>=-9.2f&&z< -7.2f&&y<3.2f)return true;
        if(x>=-12.2f&&x< -10.2f&&Math.Abs(z)<4.2f&&y<3.4f)return true;
        return false;
    }
    public static float Exposure(float x,float y,float z,bool hidden)
    {
        if(y< -1)return hidden?.025f:.045f;
        if(!Sheltered(x,y+1.5f,z))return hidden?.5f:1;
        float edge=Math.Min(Math.Abs(z-7),Math.Abs(z+7));
        // The walls remain acoustically closed; proximity only adds a subtle window wash.
        float value=.19f+Math.Max(0,1-edge/2)*.22f;
        return hidden?value*.50f:value;
    }
}
