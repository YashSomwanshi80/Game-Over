using System;
using System.IO;

// Offline sound authoring. PCM WAV assets are generated once, not synthesized during play.
public static class ProceduralSoundDesign
{
    public const int Rate=22050;
    public static readonly string[] Names={"Rain","RoomTone","Hum","Tension","PhoneLine","Static","Click","Connected","PowerOff","PowerOn","Hide","Sensor","Thunder","Capture","Rescue","HeavyRain","StormThunder"};
    public static bool IsLoop(string name)=>name=="Rain"||name=="HeavyRain"||name=="RoomTone"||name=="Hum"||name=="Tension"||name=="PhoneLine"||name=="Static";
    static double Sin(double hz,double t)=>Math.Sin(2*Math.PI*hz*t);
    public static float[] Render(string name)
    {
        bool loop=IsLoop(name);
        double duration=name=="HeavyRain"?16:name=="StormThunder"?9:name=="Rain"?8:name=="RoomTone"?6:name=="Hum"?2:name=="Tension"?8:name=="PhoneLine"?2:name=="Static"?4:name=="Thunder"?5:name=="Rescue"?1.7:name=="Capture"?1.1:name=="Hide"?.5:name=="PowerOff"?.65:name=="PowerOn"?.4:name=="Connected"?.65:.18;
        var data=new float[(int)(Rate*duration)];var rng=new Random(901+Array.IndexOf(Names,name));double low=0,slow=0,impact=0;
        for(int i=0;i<data.Length;i++) {
            double t=(double)i/Rate,n=rng.NextDouble()*2-1;
            low+=.085*(n-low);slow+=.006*(n-slow);double value=0;
            switch(name) {
                case "Rain":value=(low*.72+(n-low)*.10)*(.75+.16*Sin(.375,t)+.08*Sin(.875,t));break;
                case "HeavyRain":
                    impact*=.994;if(rng.NextDouble()<.0018)impact=.25+rng.NextDouble()*.4;
                    double gust=.82+.11*Sin(.0625,t)+.07*Sin(.1875,t);
                    value=(low*.85+(n-low)*.19+slow*.8+impact*n*.4)*gust;break;
                case "StormThunder":
                    double roll=(slow*5.2+.13*Sin(27,t)+.05*Sin(43,t));
                    double swell=.55*Math.Exp(-Math.Pow((t-1.4)/.8,2))+.3*Math.Exp(-Math.Pow((t-3.6)/1.2,2));
                    value=(low*.65*Math.Exp(-t*22)+roll*(Math.Min(t*7,1)*Math.Exp(-t*.42)+swell))*.85;break;
                case "RoomTone":value=.12*Sin(48,t)+.07*Sin(55,t)+slow*.5;break;
                case "Hum":value=.16*Sin(50,t)+.035*Sin(100,t);break;
                case "Tension":double pulse=Math.Pow(Math.Max(0,Sin(1.25,t)),9);value=(Sin(43,t)*.25+Sin(61,t)*.08)*pulse+.025*Sin(31,t);break;
                case "PhoneLine":value=(Sin(350,t)+Sin(440,t))*.12*(.8+.2*Sin(.5,t));break;
                case "Static":value=low*.24;break;
                case "Click":value=(low*.9+Sin(180,t)*.35)*Math.Exp(-t*40);break;
                case "Connected":value=(Sin(t<.2?660:880,t)*.3)*((t<.16||t>.24&&t<.43)?1:0);break;
                case "PowerOff":value=low*.8*Math.Exp(-t*12)+Sin(90-t*70,t)*.2*Math.Exp(-t*8);break;
                case "PowerOn":value=(low*.7+Sin(140,t)*.2)*Math.Exp(-t*19);break;
                case "Hide":value=(slow*2+Sin(105+t*20,t)*.09)*Math.Sin(Math.PI*t/duration);break;
                case "Sensor":value=Sin(1050,t)*.17*Math.Exp(-t*22);break;
                case "Thunder":value=(slow*5+.09*Sin(29,t)+.05*Sin(41,t))*Math.Min(t*3,1)*Math.Exp(-t*.65);break;
                case "Capture":value=(Sin(55-t*22,t)*.32+low*.45)*Math.Exp(-t*3.5);break;
                case "Rescue":value=(Sin(220,t)+Sin(277.1826,t)+Sin(329.6276,t))*.105*Math.Exp(-t*1.8);break;
            }
            // Tiny start/end fades remove clicks; loops get a wrap crossfade below.
            if(!loop)value*=Math.Min(t/.008,1)*Math.Min((duration-t)/.025,1);
            data[i]=(float)Math.Max(-.85,Math.Min(.85,value));
        }
        if(loop) {
            const int fade=512;int length=data.Length-fade;
            var wrapped=new float[length];Array.Copy(data,wrapped,length);
            for(int i=0;i<fade;i++){double w=(double)i/(fade-1);wrapped[i]=(float)(data[length+i]*(1-w)+data[i]*w);}
            return wrapped;
        }
        // Connected tones use short envelopes around both on/off transitions too.
        if(name=="Connected")for(int i=0;i<data.Length;i++) {
            double t=(double)i/Rate,start=t<.2?0:.24,end=t<.2?.16:.43;
            if(t>=start&&t<=end)data[i]*=(float)Math.Max(0,Math.Min(1,Math.Min((t-start)/.008,(end-t)/.008)));
        }
        return data;
    }
    public static void WriteWav(string path,float[] samples)
    {
        using(var writer=new BinaryWriter(File.Create(path))) {
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+samples.Length*2);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
            writer.Write(16);writer.Write((short)1);writer.Write((short)1);writer.Write(Rate);writer.Write(Rate*2);writer.Write((short)2);writer.Write((short)16);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(samples.Length*2);
            foreach(float value in samples)writer.Write((short)Math.Round(Math.Max(-1,Math.Min(1,value))*32767));
        }
    }
    public static void Bake(string folder)
    {Directory.CreateDirectory(folder);foreach(var name in Names)WriteWav(Path.Combine(folder,name+".wav"),Render(name));}
}
