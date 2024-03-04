using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinningBar : MonoBehaviour
{
    public string Side;
    public GameObject ScoreText;

    private float _initialBarY;
    private float _initialScoreTextY;

    public void Start()
    {
        _initialBarY = transform.position.y;
        _initialScoreTextY = ScoreText.transform.position.y;
    }

    public void GrowBar(int score)
    {
        if (score > 0)
        {
            var scoreFloat = ((float)score) / 3000;

            transform.position = new Vector3(transform.position.x, _initialBarY + scoreFloat, transform.position.z);
            ScoreText.transform.position = new Vector3(ScoreText.transform.position.x, _initialScoreTextY + scoreFloat, ScoreText.transform.position.z);
        } else
        {
            transform.position = new Vector3(transform.position.x, _initialBarY, transform.position.z);
            ScoreText.transform.position = new Vector3(ScoreText.transform.position.x, _initialScoreTextY, ScoreText.transform.position.z);
        }
    }

}
