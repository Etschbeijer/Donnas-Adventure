using System;

public static class HighscoreManager
{
    public static int CalculateScore(float actualTime, int collectedTreats, int collectedSpecialTreats, float expectedTime = 180f, int exptectedTreats = 200)
    {
        float timeScore = MathF.Max(0, (expectedTime / actualTime) * 5000f);
        float treatScore = MathF.Max(0, (exptectedTreats / collectedTreats) * 5000f);
        float specialTreats = (collectedSpecialTreats / 3) * 2000f;
        float allSpecialTreatBonus = collectedSpecialTreats == 3 ? 3000f : 0f;

        float totalScore = timeScore + treatScore + specialTreats + allSpecialTreatBonus;
        return (int)MathF.Round(totalScore);
    }
}
