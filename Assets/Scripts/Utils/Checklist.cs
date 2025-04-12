using System;

public class Checklist
{
    public int requiredSteps { get; private set; }
    public int currentSteps { get; private set; }

    public float percentDone => (float)currentSteps / currentSteps;
    public bool isDone => currentSteps >= requiredSteps;
    public event Action onCompleted;

    public Checklist(int stepsNeeded)
    {
        currentSteps = 0;
        requiredSteps = stepsNeeded;
    }

    public void FinishStep(int stepAmount = 1)
    {
        currentSteps += stepAmount;
        currentSteps = Math.Min(currentSteps, requiredSteps);

        if (isDone) onCompleted?.Invoke();
    }

    public void ResetChecklist() => currentSteps = 0;
}