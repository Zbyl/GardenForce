using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOut : MonoBehaviour
{
    public float fadeOutSeconds;
    private float createTime;

    // Start is called before the first frame update
    void Start()
    {
        createTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (fadeOut())
        {
            Destroy(gameObject);
        }
    }

    /// Logic for fade out.
    bool fadeOut()
    {
        if (fadeOutSeconds == 0)
            return true;

        var fadeFraction = (Time.time - createTime) / fadeOutSeconds;
        if (fadeFraction > 1.0f)
        {
            fadeFraction = 1.0f;
        }
        else
        {
            fadeFraction = Mathf.Sqrt(fadeFraction);
        }

        var model = this.transform.Find("Model");
        foreach (var child in model.GetComponentsInChildren<SpriteRenderer>())
        {
            var color = child.color;
            color.a = Mathf.Min(1.0f - fadeFraction, color.a);
            child.color = color;
        }

        return fadeFraction >= 1.0f;
    }
}
