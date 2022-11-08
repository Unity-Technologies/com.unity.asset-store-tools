using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomColorChanger : MonoBehaviour
{
    // Start is called before the first frame update
    private MeshRenderer mr;
    void Start()
    {
        mr = GetComponent<MeshRenderer>();
        StartCoroutine(Randomizer());
    }

    private IEnumerator Randomizer()
    {
        while (true)
        {
            var color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            var mat = mr.material;
            mat.color = color;
            mr.material = mat;
            yield return new WaitForSeconds(1f);
        }
    }
}
