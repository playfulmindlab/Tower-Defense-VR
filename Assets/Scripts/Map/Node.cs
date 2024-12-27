using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Node : MonoBehaviour
{
    public Node[] nextNodesToGo;

    public bool isStart = false;
    public bool isEnd = false;

    public Node GetNextNode(List<Node> currPath)
    {
        if (isEnd != true)
        {
            int nodeNum = UnityEngine.Random.Range(0, nextNodesToGo.Length);

            if (currPath.Contains(nextNodesToGo[nodeNum]))//(Array.Exists(currPath, node => nextNodesToGo[nodeNum]))
                return GetOtherNode(nextNodesToGo[nodeNum]);
            else
                return nextNodesToGo[nodeNum];
        }
        return null;
    }

    public bool CanGetNextNode()
    {
        if (isEnd == false && nextNodesToGo.Length > 0)
            return true;

        return false;
    }

    Node GetOtherNode(Node firstNode)
    {
        List<Node> tempNodeList = new List<Node>();

        foreach(Node node in nextNodesToGo)
        {
            if (node != firstNode)
                tempNodeList.Add(node);
        }

        return tempNodeList[0];
    }
}
