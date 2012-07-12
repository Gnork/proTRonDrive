//Christoph Jansen

using UnityEngine;
using System.Collections;

[ExecuteInEditMode()] 
public class RotatableGuiItem : MonoBehaviour
{
	
	/*Texture data
	 *public to be assigned in editor*/
	public Texture2D texture = null;
	public Vector2 size = new Vector2(128, 128);
	public float angle = 0;
	public AlignmentScreenpoint ScreenpointToAlign = AlignmentScreenpoint.TopLeft;
	public Vector2 relativePosition = new Vector2(0, 0);
	
	Vector2 pos = new Vector2(0, 0);
	
	Rect rect;
	
	//Pivot point for rotation
	Vector2 pivot;
	
	/*Enums for screen positions
	 *public to be assigned in editor*/
	public enum AlignmentScreenpoint
	{
	    TopLeft, TopMiddle, TopRight,
	    MiddleLeft, MiddleRight,
	    BottomLeft, BottomMiddle, BottomRight
	}
	
	void Start() 
	{
	    UpdateSettings();		
	}
	
	//Update setting changes made in editor
	void UpdateSettings()
	{
	    Vector2 cornerPos = new Vector2(0, 0);
		
	    switch (ScreenpointToAlign)
	    {
	       case AlignmentScreenpoint.TopLeft:
	         cornerPos =new Vector2(0, 0);
	         break;
	       case AlignmentScreenpoint.TopMiddle:
	         cornerPos =new Vector2(Screen.width/2, 0);
	         break;
	       case AlignmentScreenpoint.TopRight:
	         cornerPos = new Vector2(Screen.width, 0);
	         break;
	       case AlignmentScreenpoint.MiddleLeft:
	         cornerPos = new Vector2(0, Screen.height / 2);
	         break;
	       case AlignmentScreenpoint.MiddleRight:
	         cornerPos = new Vector2(Screen.width, Screen.height / 2);
	         break;
	       case AlignmentScreenpoint.BottomLeft:
	         cornerPos = new Vector2(0, Screen.height);
	         break;
	       case AlignmentScreenpoint.BottomMiddle:
	         cornerPos = new Vector2(Screen.width/2, Screen.height);
	         break;
	       case AlignmentScreenpoint.BottomRight:
	         cornerPos = new Vector2(Screen.width, Screen.height);
	         break;
	       default:
	         break;
	    }
	
	    pos = cornerPos + relativePosition;
	    rect = new Rect(pos.x - size.x * 0.5f, pos.y - size.y * 0.5f, size.x, size.y);
	    pivot = new Vector2(rect.xMin + rect.width * 0.5f, rect.yMin + rect.height * 0.5f);
	}
	
	void OnGUI()
	{
		/*Update setting changes on the fly only if editor is used
		 *not for deployment*/
	    if (Application.isEditor)
	    {
	       UpdateSettings();
	    }
		//Store screen matrix, rotate and draw texture, restore old matrix
	    Matrix4x4 matrixBackup = GUI.matrix;
	    GUIUtility.RotateAroundPivot(angle, pivot);
	    GUI.DrawTexture(rect, texture);
	    GUI.matrix = matrixBackup;
	}
}
