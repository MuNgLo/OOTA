using System;
using Godot;

namespace MGizmosCSharp;
/// <summary>
/// Static class that defines base shapes to use for the gizmos
/// </summary>
public enum GSHAPES { CUSTOM, SQUARE, STOP, DIAMOND, ARROW, CUBE, TRIANGLE, CIRCLE }
public static class GizmoShapes
{
    /// <summary>
    /// Uses the shape field to get the vec3 array that should be used
    /// </summary>
    /// <returns></returns>
    public static Vector3[] GetShape(GSHAPES shape)
    {
        switch (shape)
        {
            case GSHAPES.STOP:
                return Stop;
            case GSHAPES.SQUARE:
                return Square;
            case GSHAPES.DIAMOND:
                return Diamond;
            case GSHAPES.ARROW:
                return Arrow;
            case GSHAPES.CUBE:
                return Cube;
            case GSHAPES.TRIANGLE:
                return Triangle;
            case GSHAPES.CIRCLE:
                return Circle();
        }
        return new Vector3[0];
    }

    public static Vector3[] Circle(int subd = 12)
    {
        if (subd < 3) { subd = 3; }
        Vector3[] arr = new Vector3[subd + 1];

        float step = 2 * Mathf.Pi / subd;
        for (int i = 0; i < subd; i++)
        {
            float x = 0.5f * Mathf.Cos(i * step);
            float z = 0.5f * Mathf.Sin(i * step);
            arr[i] = new Vector3(x, 0.0f, z);
        }
        arr[subd] = arr[0];
        return arr;
    }

    public static Vector3[] Triangle = new Vector3[]{
        new Vector3(0.0f,0.5f,0.0f),
        new Vector3(-0.5f,0.0f,0.0f),
        new Vector3(0.5f,0.0f,0.0f),
        new Vector3(0.0f,0.5f,0.0f)
    };
    public static Vector3[] Arrow = new Vector3[]{
        new Vector3(0.0f,1.0f,0.0f), new Vector3(0.0f,0.0f,0.0f), new Vector3(0.25f,0.25f,0.0f),
        new Vector3(-0.25f,0.25f,0.0f), new Vector3(0.0f,0.0f,0.0f)
    };
    public static Vector3[] Square = new Vector3[]{
        new Vector3(-0.5f,0.0f,0.0f), new Vector3(0.0f,0.0f,0.5f), new Vector3(0.5f,0.0f,0.0f),
        new Vector3(0.0f,0.0f,-0.5f), new Vector3(-0.5f,0.0f,0.0f)
    };
    public static Vector3[] Diamond = new Vector3[]{
        new Vector3(-0.5f,0.0f,0.0f), new Vector3(0.0f,0.0f,0.5f), new Vector3(0.5f,0.0f,0.0f),
        new Vector3(0.0f,0.0f,-0.5f), new Vector3(-0.5f,0.0f,0.0f),new Vector3(0.0f,-0.5f,0.0f),
        new Vector3(0.5f,0.0f,0.0f), new Vector3(0.0f,0.5f,0.0f), new Vector3(0.0f,0.0f,-0.5f),
        new Vector3(0.0f,-0.5f,0.0f),new Vector3(0.0f,0.0f,0.5f),new Vector3(0.0f,0.5f,0.0f),
        new Vector3(-0.5f,0.0f,0.0f)
    };
    public static Vector3[] Stop = new Vector3[]{
        new Vector3(-0.25f,0.0f,-0.5f), new Vector3(0.25f,0.0f,-0.5f), new Vector3(0.5f,0.0f,-0.25f),
        new Vector3(0.5f,0.0f,0.25f), new Vector3(0.25f,0.0f,0.5f), new Vector3(-0.25f,0.0f,0.5f),
        new Vector3(-0.5f,0.0f,0.25f), new Vector3(-0.5f,0.0f,-0.25f), new Vector3(-0.25f,0.0f,-0.5f),
    };
    public static Vector3[] Cube = new Vector3[]{
		// Bottom
		new Vector3(-0.5f,-0.5f,0.5f),
        new Vector3(0.5f,-0.5f,0.5f),
        new Vector3(0.5f,-0.5f,-0.5f),
        new Vector3(-0.5f,-0.5f,-0.5f),
        new Vector3(-0.5f,-0.5f,0.5f),
		// Top
		new Vector3(-0.5f,0.5f,0.5f),
        new Vector3(0.5f,0.5f,0.5f), // 6th
		new Vector3(0.5f,0.5f,-0.5f),
        new Vector3(-0.5f,0.5f,-0.5f),
        new Vector3(-0.5f,0.5f,0.5f),
        new Vector3(-0.5f,-0.5f,-0.5f), // 10th
		new Vector3(-0.5f,0.5f,-0.5f),
        new Vector3(0.5f,-0.5f,-0.5f),
        new Vector3(0.5f,0.5f,-0.5f),
        new Vector3(0.5f,-0.5f,0.5f),
        new Vector3(0.5f,0.5f,0.5f), // 15th
		new Vector3(-0.5f,-0.5f,0.5f)
    };




}// EOF CLASS