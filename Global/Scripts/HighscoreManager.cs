using System;

public static class HighscoreManager
{
    public static int CalculateScore(float actualTime, int collectedTreats, int collectedSpecialTreats, float expectedTime = 180f, int exptectedTreats = 150)
    {
        float safeTime = actualTime <= 0f ? 1f : actualTime;
        float timeScore = MathF.Max(0, (expectedTime / safeTime) * 5000f);
        float treatScore = exptectedTreats <= 0 ? 0f : MathF.Max(0, ((float)collectedTreats / exptectedTreats) * 5000f);
        float specialTreats = (collectedSpecialTreats / 3f) * 2000f;
        float allSpecialTreatBonus = collectedSpecialTreats == 3 ? 3000f : 0f;

        float totalScore = timeScore + treatScore + specialTreats + allSpecialTreatBonus;
        return (int)MathF.Round(totalScore);
    }
}
