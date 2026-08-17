using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFootstep : MonoBehaviour
{
    public void PlayFootstep()
    {
        SoundManager.PlaySound(SoundType.FOOTSTEP, 0.5f);
    }
}
