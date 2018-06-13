using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using System;

public class IpfsObject : MonoBehaviour {
    // Update is called once per frame
    void Update () {
	}

    public async Task<string> Write(IpfsGenesis gen, string addhash)
    {
        Debug.Log("Hi. Today, the hash I'm referencing is:" + addhash);
        GameObject ob = gameObject;
        if (ob.transform.childCount > 0)
        {
            List<Object> objects = new List<Object>();
            List<MeshFilter> filter = GetChildRecursive(ob);

            foreach (Transform child in ob.transform)
            {
                filter.AddRange(GetChildRecursive(child.gameObject));
            }

            foreach (MeshFilter f in filter)
            {
                objects.Add(new Object(f.mesh, f.gameObject));
            }
            string hash = await IpfsIO.AddObject(objects, gen, addhash);
            return hash;
        }
        else
        {
            Mesh mesh = GetComponent<MeshFilter>().mesh;
            Object onlyobject = new Object(mesh, gameObject);
            Object[] objectarray = new Object[1] { onlyobject };
            string hash = await IpfsIO.AddObject(objectarray.ToList(), gen, addhash);
            return hash;

        }
    }

    private List<MeshFilter> GetChildRecursive(GameObject obj)
    {
        List<MeshFilter> listOfChildren = new List<MeshFilter>();
        if (null == obj)
            return null;

        foreach (Transform child in obj.transform)
        {
            if (null == child)
                continue;
            //child.gameobject contains the current child you can do whatever you want like add it to an array
            if (child.gameObject.GetComponent<MeshFilter>() != null)
            {
                listOfChildren.Add(child.gameObject.GetComponent<MeshFilter>());
            }
            GetChildRecursive(child.gameObject);
        }
        return listOfChildren;
    }
}

[System.Serializable]
public class Object
{
    public float[][] verts;
    public float[][] normals;
    public int[] triangles;

    public float positionx, positiony, positionz;
    public Object(Mesh m, GameObject o)
    {
        
        verts = new float[m.vertexCount][];
        int inc = 0;
        foreach (Vector3 vert in m.vertices)
        {
            verts[inc] = new float[3]; 
            verts[inc][0] = vert.x;
            verts[inc][1] = vert.y;
            verts[inc][2] = vert.z;
            inc++;
        }

        normals = new float[m.vertexCount][];
        inc = 0;
        foreach (Vector3 n in m.normals)
        {
            normals[inc] = new float[3];
            normals[inc][0] = n.x;
            normals[inc][1] = n.y;
            normals[inc][2] = n.z;
            inc++;
        }

        triangles = m.triangles;
        positionx = o.transform.position.x;
        positiony = o.transform.position.y; ;
        positionz = o.transform.position.z; ;
    }

    public GameObject GameObject()
    {
        Mesh newmesh = new Mesh();
        List<Vector3> newverts = new List<Vector3>();
        for (int i = 0; i < verts.GetLength(0); i++)
        {
            Vector3 co = new Vector3(verts[i][0], verts[i][1], verts[i][2]);
            newverts.Add(co);
        }

        List<Vector3> newnormals = new List<Vector3>();
        for (int i = 0; i < normals.GetLength(0); i++)
        {
            Vector3 co = new Vector3(normals[i][0], normals[i][1], normals[i][2]);
            newnormals.Add(co);
        }

        newmesh.vertices = newverts.ToArray();
        newmesh.triangles = triangles;
        newmesh.normals = newnormals.ToArray();

        GameObject newob = new GameObject();
        newob.AddComponent<MeshRenderer>();
        MeshFilter filter = newob.AddComponent<MeshFilter>();
        filter.mesh = newmesh;

        return newob;
    }
}