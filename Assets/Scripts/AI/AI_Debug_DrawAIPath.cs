using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.AI;


/// <summary>
/// I found this script at the below link, but had to modify it from Javascript. I also improved this component with a few new features.
/// https://gamedev.stackexchange.com/questions/67839/is-there-a-way-to-display-navmesh-agent-path-in-unity
/// </summary>
public class AI_Debug_DrawAIPath : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField] private float _LineEndWidth = 0.05f;

    [Range(0f, 1f)]
    [SerializeField] private float _LineStartWidth = 0.05f;

    [SerializeField] private Color32 _LineEndColor = Color.cyan;
    [SerializeField] private Color32 _LineStartColor = Color.cyan;



    private LineRenderer _Line; // The line Renderer
    private Transform _Target; // The transform of the target
    private NavMeshAgent _Agent; // The agent of this gameObject



    void Start()
    {
        _Agent = GetComponent<NavMeshAgent>(); //get the agent

        _Line = gameObject.AddComponent<LineRenderer>();
        _Line.endWidth = _LineEndWidth;
        _Line.startWidth = _LineStartWidth;
        _Line.endColor = _LineEndColor;
        _Line.startColor = _LineStartColor;
        _Line.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));        
    }

    void Update()
    {
        _Line.SetPosition(0, transform.position); //set the line's origin

        //_Agent.SetDestination(_Target.position); //create the path

        if (_Agent.path != null && _Agent.path.corners.Length > 0)
            DrawPath(_Agent.path);

        //_Agent.isStopped = true; // Add this if you don't want to move the agent
    }

    private void DrawPath(NavMeshPath path)
    {
        if (path.corners.Length < 2) // If the path has 1 or no corners, there is no need
            return;


        _Line.endWidth = _LineEndWidth;
        _Line.startWidth = _LineStartWidth;
        _Line.endColor = _LineEndColor;
        _Line.startColor = _LineStartColor;

        _Line.positionCount = path.corners.Length; // Set the array of positions to the amount of corners

        for (var i = 1; i < path.corners.Length; i++)
        {
            _Line.SetPosition(i, path.corners[i]); // Go through each corner and set that to the line renderer's position
        }
    }


    public void SetWidth(float startWidth, float endWidth)
    {
        _LineStartWidth = startWidth;
        _LineEndWidth = endWidth;
    }

    public void SetWidth(float lineWidth)
    {
        SetWidth(lineWidth, lineWidth);
    }

    public void SetColor(Color32 lineStartColor, Color32 lineEndColor)
    {
        _LineStartColor = lineStartColor;
        _LineEndColor = lineEndColor;
    }

    public void SetColor(Color32 lineColor)
    {
        SetColor(lineColor, lineColor);
    }

    public void SetColorAndWidth(float lineStartWidth, float lineEndWidth, Color32 lineStartColor, Color32 lineEndColor)
    {
        SetColor(lineStartColor, lineEndColor);
        SetWidth(lineStartWidth, lineEndWidth);
    }

    public void SetColorAndWidth(Color32 lineColor, float lineWidth)
    {
        SetColor(lineColor);
        SetWidth(lineWidth);
    }

}
