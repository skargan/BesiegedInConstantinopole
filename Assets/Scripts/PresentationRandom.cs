// Cosmetic sampling must never advance Unity's saved campaign RNG.
public static class PresentationRandom {
 static readonly System.Random random=new System.Random();
 public static float Range(float min,float max)=>min+(max-min)*(float)random.NextDouble();
}
