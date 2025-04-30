using System.Collections.Generic;
using UnityEngine;

namespace Board
{
    public enum NodeColor
    {
        Blue,
        Red,
        Green
    }
    
    public class Node : MonoBehaviour
    {
       private NodeColor _color;
       
       public NodeColor Color => _color;

       private static readonly Dictionary<NodeColor, Color> ColorLookup = new()
       {
           { NodeColor.Blue,  UnityEngine.Color.blue  },
           { NodeColor.Red,   UnityEngine.Color.red   },
           { NodeColor.Green, UnityEngine.Color.green }
       };

       public void Init(NodeColor chosenColor)
       {
           _color = chosenColor;
           if (TryGetComponent<MeshRenderer>(out var meshRenderer))
           {
               meshRenderer.sharedMaterial = new Material(meshRenderer.sharedMaterial)
               {
                   color = ColorLookup[_color]
               };
           }
       }
    }
}