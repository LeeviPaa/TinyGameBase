using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum gameplayTags
{
    player,
    tool,
    climbable,
    other
}

public class GameplayTag : MonoBehaviour {

    [SerializeField]
    private gameplayTags Tag = gameplayTags.other;

    public gameplayTags GetGameplayTag()
    {
        return Tag;
    }
}
