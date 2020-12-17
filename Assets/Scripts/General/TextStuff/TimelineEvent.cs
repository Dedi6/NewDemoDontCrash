using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

public class TimelineEvent : MonoBehaviour
{

    public UnityEvent eventT;
    public PlayableDirector director;

    private void OnEnable()
    {
        eventT.Invoke();
        director.playableGraph.GetRootPlayable(0).SetSpeed(0);
        PrefabManager.instance.currentDirector = director;
    }

}
