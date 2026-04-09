using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class MeshDecimator : EditorWindow
{
    [MenuItem("Tools/Decimate All FBX Models")]
    static void DecimateAll()
    {
        var paths = new string[]
        {
            "Assets/_Project/Models/Truck/HeavyTruck.fbx",
            "Assets/_Project/Models/Scaffolding/Scaffolding.fbx",
            "Assets/_Project/Models/Eladi/Eladi.fbx",
            "Assets/_Project/Models/EnergyDrink/EnergyDrink.fbx",
            "Assets/_Project/Models/WorkGloves/WorkGloves.fbx"
        };

        foreach (var path in paths)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null) continue;

            var mf = go.GetComponentInChildren<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) continue;

            var original = mf.sharedMesh;
            int targetVerts = 8000;
            float quality = (float)targetVerts / original.vertexCount;
            quality = Mathf.Clamp(quality, 0.001f, 1f);

            var decimated = DecimateMesh(original, quality);
            
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            string savePath = System.IO.Path.GetDirectoryName(path) + "/" + name + "_LOD.asset";
            
            AssetDatabase.CreateAsset(decimated, savePath);
            Debug.Log($"[Decimator] {name}: {original.vertexCount:N0} -> {decimated.vertexCount:N0} verts (saved to {savePath})");
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[Decimator] All models decimated!");
    }

    static Mesh DecimateMesh(Mesh source, float quality)
    {
        var vertices = source.vertices;
        var triangles = source.triangles;
        var normals = source.normals;
        var uvs = source.uv;

        // Vertex clustering decimation
        // Compute bounding box
        var bounds = source.bounds;
        
        // Grid resolution based on target quality
        int gridRes = Mathf.Max(30, Mathf.RoundToInt(Mathf.Pow(vertices.Length * quality, 1f/3f) * 1.5f));
        
        var cellSize = new Vector3(
            bounds.size.x / gridRes,
            bounds.size.y / gridRes,
            bounds.size.z / gridRes
        );

        // Prevent zero-size cells
        cellSize.x = Mathf.Max(cellSize.x, 0.0001f);
        cellSize.y = Mathf.Max(cellSize.y, 0.0001f);
        cellSize.z = Mathf.Max(cellSize.z, 0.0001f);

        // Map each vertex to a grid cell
        var cellMap = new Dictionary<long, List<int>>();
        var vertexToCell = new int[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            var v = vertices[i] - bounds.min;
            int cx = Mathf.Min((int)(v.x / cellSize.x), gridRes - 1);
            int cy = Mathf.Min((int)(v.y / cellSize.y), gridRes - 1);
            int cz = Mathf.Min((int)(v.z / cellSize.z), gridRes - 1);
            long key = (long)cx * gridRes * gridRes + (long)cy * gridRes + cz;

            if (!cellMap.ContainsKey(key))
                cellMap[key] = new List<int>();
            cellMap[key].Add(i);
        }

        // Create new vertices (average of each cell)
        var newVerts = new List<Vector3>();
        var newNormals = new List<Vector3>();
        var newUVs = new List<Vector2>();
        var oldToNew = new int[vertices.Length];

        int newIndex = 0;
        foreach (var kvp in cellMap)
        {
            var indices = kvp.Value;
            var avgPos = Vector3.zero;
            var avgNorm = Vector3.zero;
            var avgUV = Vector2.zero;

            foreach (int idx in indices)
            {
                avgPos += vertices[idx];
                if (normals.Length > idx) avgNorm += normals[idx];
                if (uvs.Length > idx) avgUV += uvs[idx];
                oldToNew[idx] = newIndex;
            }

            float count = indices.Count;
            newVerts.Add(avgPos / count);
            newNormals.Add((avgNorm / count).normalized);
            newUVs.Add(avgUV / count);
            newIndex++;
        }

        // Remap triangles, removing degenerate ones
        var newTris = new List<int>();
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int a = oldToNew[triangles[i]];
            int b = oldToNew[triangles[i + 1]];
            int c = oldToNew[triangles[i + 2]];
            if (a != b && b != c && a != c)
            {
                newTris.Add(a);
                newTris.Add(b);
                newTris.Add(c);
            }
        }

        var mesh = new Mesh();
        mesh.name = source.name + "_decimated";
        mesh.SetVertices(newVerts);
        mesh.SetNormals(newNormals);
        mesh.SetUVs(0, newUVs);
        mesh.SetTriangles(newTris, 0);
        mesh.RecalculateBounds();

        return mesh;
    }
}