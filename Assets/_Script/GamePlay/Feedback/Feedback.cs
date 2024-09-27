using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Feedback : MonoBehaviour
{
    public abstract void CreateFeedback();

    //this function will basically reset any value when the previous effect is still playing be we need to play new effect
    public abstract void CompletePreviousFeedback();
}
