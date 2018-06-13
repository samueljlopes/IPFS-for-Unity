using Ipfs.Engine;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;
using System.Threading;

public static class IpfsIO
{

    public static async Task<string> AddObject(List<Object> obs, IpfsGenesis gen, string addhash)
    {
        string hashpath = Application.persistentDataPath + "/hash.txt";
        string hashdatapath = Application.persistentDataPath + "/data.txt";

        if (File.Exists(hashdatapath))
        {
            File.Delete(hashdatapath);
        }

        StreamWriter txtobjectwriter = new StreamWriter(hashdatapath);
        txtobjectwriter.WriteLine("Hash:" + addhash);

        foreach (Object ob in obs)
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(memoryStream, ob);
            string str = System.Convert.ToBase64String(memoryStream.ToArray());

            txtobjectwriter.WriteLine(str);
        }
        txtobjectwriter.Close();

        Ipfs.Cid taskA = await AddObjectSaveHash(hashdatapath);
        string hash = taskA.Hash.ToString();
        Debug.Log(hash);

        StreamWriter txtwriter = new StreamWriter(hashpath);
        txtwriter.Write(hash);
        txtwriter.Close();

        return hash;
    }

    public static async void ReadObject(string hash, IpfsGenerated returnto)
    {
        Stream basestream = await ReturnText(hash);
        
        List<Object> reconstructed = new List<Object>();
        using (StreamReader sr = new StreamReader(basestream))
        {
            while (!sr.EndOfStream)
            {
                string objectStream = sr.ReadLine();
                if (!objectStream.Contains("Hash:"))
                {
                    //Convert the Base64 string into byte array
                    byte[] memorydata = Convert.FromBase64String(objectStream);
                    MemoryStream rs = new MemoryStream(memorydata);
                    BinaryFormatter sf = new BinaryFormatter();
                    //Create object using BinaryFormatter
                    Object objResult = (Object)sf.Deserialize(rs);
                    reconstructed.Add(objResult);
                }
                else
                {
                    objectStream = objectStream.Replace("Hash:", "");
                    Debug.Log("Found next hash");
                    returnto.nexthash = objectStream;
                }
            }
        }
        returnto.objects = reconstructed;
        Debug.Log("Sucess! It's not IPFS");
    }

    async static Task<Ipfs.Cid> AddObjectSaveHash(string hash)
    {
        Batteries.Init();

        using (var ie = new IpfsEngine("".ToCharArray()))
        {
            var newdata = await ie.FileSystem.AddFileAsync(hash)
                           .ConfigureAwait(false);
            return newdata.Id;
        }
    }

    async static Task<Stream> ReturnText(string hash)
    {
        Batteries.Init();

        var ipfs = new IpfsEngine("".ToCharArray());
        var data = await ipfs.FileSystem.ReadFileAsync(hash);
        return data;
    }
}

