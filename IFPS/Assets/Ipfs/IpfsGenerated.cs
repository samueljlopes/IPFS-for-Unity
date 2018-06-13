using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class IpfsGenerated : MonoBehaviour {

    public string nexthash = "";
    public bool Shifted = false;

    public List<Object> objects;
    private bool generate = false;
    

    private void Update()
    {
        if (objects != null && !generate)
        {
            Debug.Log("Stream received");
            GameObject parent = gameObject;
            foreach (Object ob in objects)
            {
                GameObject newob = ob.GameObject();
                newob.transform.parent = parent.transform;
                gameObject.transform.position = new Vector3(ob.positionx, ob.positiony, ob.positionz);
                newob.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse")); 
            }
            generate = true;
        }

        if (nexthash != "" && !Shifted)
        {
            string hash = PlayerPrefs.GetString("StartHash");
            GameObject newobject = new GameObject();
            IpfsGenerated gen = newobject.AddComponent<IpfsGenerated>();

            Thread readthread = new Thread(() => IpfsIO.ReadObject(nexthash, gen));
            readthread.Start();

            Shifted = true;
        }
    }
}