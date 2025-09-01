using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PlaySoundOnClick : MonoBehaviour
{
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            SoundManager.Instance.PlayTick();
        });
    }
}

