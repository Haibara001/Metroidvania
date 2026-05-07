using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(Water))]
public class WaterEditor : Editor
{
    private Water _water;

    private void OnEnable()
    {
        _water = (Water)target;
    }

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new VisualElement();

        InspectorElement.FillDefaultInspector(root, serializedObject, this);

        root.Add(new VisualElement { style = { height = 10 } });

        Button generateButton = new Button(() =>
        {
            _water.GenerateMesh();
            _water.ResetEdgeCollider();
        })
        {
            text = "Generate Water Mesh"
        };

        root.Add(generateButton);

        Button placeEdgeColliderButton = new Button(() => _water.ResetEdgeCollider())
        {
            text = "Place Edge Collider"
        };
        root.Add(placeEdgeColliderButton);

        return root;
    }

    private void OnSceneGUI()
    {
        Handles.color = _water.GizmoColor;
        Vector3 center = _water.transform.position;
        Vector3 size = new Vector3(_water.Width, _water.Height, 0.1f);
        Handles.DrawWireCube(center, size);

        float handleSize = HandleUtility.GetHandleSize(center) * 0.1f;
        Vector3 snap = Vector3.one * 0.1f;

        Vector3[] corners = new Vector3[4];
        corners[0] = center + new Vector3(-_water.Width / 2f, -_water.Height / 2f, 0);
        corners[1] = center + new Vector3(_water.Width / 2f, -_water.Height / 2f, 0);
        corners[2] = center + new Vector3(-_water.Width / 2f, _water.Height / 2f, 0);
        corners[3] = center + new Vector3(_water.Width / 2f, _water.Height / 2f, 0);

        EditorGUI.BeginChangeCheck();
        Vector3 newBottomLeft = Handles.FreeMoveHandle(corners[0], handleSize, snap, Handles.CubeHandleCap);
        if (EditorGUI.EndChangeCheck())
        {
            ChangeDimensions(ref _water.Width, ref _water.Height,
                corners[3].x - newBottomLeft.x, corners[3].y - newBottomLeft.y);
        }

        EditorGUI.BeginChangeCheck();
        Vector3 newTopRight = Handles.FreeMoveHandle(corners[3], handleSize, snap, Handles.CubeHandleCap);
        if (EditorGUI.EndChangeCheck())
        {
            ChangeDimensions(ref _water.Width, ref _water.Height,
                newTopRight.x - corners[0].x, newTopRight.y - corners[0].y);
        }

        if (GUI.changed)
        {
            _water.GenerateMesh();
        }
    }

    private void ChangeDimensions(ref float width, ref float height, float newWidth, float newHeight)
    {
        width = Mathf.Max(0.1f, newWidth);
        height = Mathf.Max(0.1f, newHeight);
    }
}
