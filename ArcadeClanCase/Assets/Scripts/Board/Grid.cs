using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Board
{
    [ExecuteAlways]
    public class Grid : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Vector2Int gridSize = new Vector2Int(4, 4);
        [SerializeField] private Vector2Int gridOrigin = Vector2Int.zero;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private float verticalOffset = -0.7f;

        [Header("Elements")] 
        [SerializeField] private Node nodePrefab;
        
        private bool _needsRebuild; 

        private readonly Dictionary<Vector2Int, Node> _nodesByCoord = new();
        
        private static readonly Vector2Int[] DirectionOffsets =
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
            new Vector2Int( 1,  1), new Vector2Int( 1, -1),
            new Vector2Int(-1,  1), new Vector2Int(-1, -1)
        };
        
        private void OnValidate(){ _needsRebuild = true; }

        private void Update()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && _needsRebuild)
            {
                _needsRebuild = false;             
                EditorApplication.delayCall += () =>
                {
                    if (!this) return;       
                    Rebuild();                     
                };
            }
#endif
        }

        public Node this[Vector2Int coordinate] =>
            _nodesByCoord.GetValueOrDefault(coordinate);
        
        public IEnumerable<Vector2Int> AllCoordinates => _nodesByCoord.Keys;
        
        public Vector3 ToWorld(Vector2Int coordinate) =>
            new Vector3(coordinate.x * cellSize, 0f, coordinate.y * cellSize);
        
        public bool TryFindMatches(out List<List<Node>> matchedGroups)
        {
            matchedGroups = new List<List<Node>>();
            var alreadyVisited = new HashSet<Vector2Int>();

            foreach (var (coordinate, node) in _nodesByCoord)
            {
                if (alreadyVisited.Contains(coordinate)) continue;

                var groupCoordinates = FloodFill(coordinate);
                alreadyVisited.UnionWith(groupCoordinates);

                if (groupCoordinates.Count > 1)
                    matchedGroups.Add(groupCoordinates.Select(c => _nodesByCoord[c]).ToList());
            }
            return matchedGroups.Count > 0;
        }

        private void Rebuild()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(child.gameObject);  
                else
#endif
                    Destroy(child.gameObject);
            }
            _nodesByCoord.Clear();
            
            System.Random prng = new ();
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    Vector2Int coord = new (x, y);
                    Vector3 worldPos = new Vector3(
                        (gridOrigin.x + coord.x) * cellSize,
                        verticalOffset,
                        (gridOrigin.y + coord.y) * cellSize);

                    Node node = Instantiate(nodePrefab, worldPos, Quaternion.identity, transform);
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        node.gameObject.name = $"Node {coord}";
#endif
                    NodeColor randomColor = (NodeColor)prng.Next(Enum.GetValues(typeof(NodeColor)).Length);
                    node.Init(randomColor);

                    _nodesByCoord.Add(coord, node);
                }
            }
        }
        
        private List<Vector2Int> FloodFill(Vector2Int startCoordinate)
        {
            var targetColor   = _nodesByCoord[startCoordinate].Color;
            var connected     = new List<Vector2Int>();
            var openQueue     = new Queue<Vector2Int>();
            var localVisited  = new HashSet<Vector2Int> { startCoordinate };

            openQueue.Enqueue(startCoordinate);

            while (openQueue.Count > 0)
            {
                var currentCoordinate = openQueue.Dequeue();
                connected.Add(currentCoordinate);

                var neighbourCoords = DirectionOffsets
                    .Select(offset => currentCoordinate + offset)
                    .Where(neighbour => _nodesByCoord.TryGetValue(neighbour, out var neighbourNode)
                                        && neighbourNode.Color == targetColor
                                        && !localVisited.Contains(neighbour));

                foreach (var neighbourCoordinate in neighbourCoords)
                {
                    localVisited.Add(neighbourCoordinate);
                    openQueue.Enqueue(neighbourCoordinate);
                }
            }
            return connected;
        }
    }
}
