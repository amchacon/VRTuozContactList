using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class GazeClick : MonoBehaviour, IGvrPointer
{ 
	private Transform Parent;
	private Material Source;
	private Color OriginalColor;
	private Material Copy;
	private Mesh TargetMesh;
	private Mesh RingMesh;
	private MeshRenderer Renderer;

	private GameObject Target;
	private EventSystem EventSystem;
	private IGvrPointer GazePointer;

	[Tooltip("How long the circle waits before starting the rotation after the expansion. (in Seconds)")]
	public float DelayBeforeRotation = 0.1f;

	[Tooltip("How long the circle waits before the click happens after finishing the rotation. (in Seconds)")]
	public float DelayBeforeClick = 0.1f;

	[Tooltip("How long the rotation of the circle takes. (in Seconds)")]
	public float RotationDuration = 1.5f;

	[Tooltip("If the original color should be used.")]
	public bool UseOriginalColor = true;
	[Tooltip("The color of the initial front circle during rotation if UseOriginalColor is set to false.")]
	public Color FillColor=Color.white;
	[Tooltip("The color of the background circle during rotation if UseOriginalColor is set to false.")]
	public Color BackgroundColor=new Color(1,1,1,0.5f);

	[Tooltip("The alpha value of the background during the rotation, the smaler the value the less visible.")]
	public float BackgroundAlpha=0.3f;

	[Tooltip("The direction in which the circle rotates.")]
	public RotationDirection RotationDirection= RotationDirection.clockwise;
	[Tooltip("Where the rotation begins.")]
	public RotationStart RotationStart = RotationStart.top;
	private float circleState=0;

	[Tooltip("Precision of movement recognized. The smaller this is the more sensible it is for small shakes.")]
	public float Precision=0.01f;

	[Tooltip("The name of a float in the used shader (only change if the shader changed)")]
	public string InnerCircle = "_InnerDiameter";
	private float innerValue = 0;
	private float maxInnerValue = float.MinValue;
	private float minInnerValue = float.MaxValue;
	private float lastInnerValue = 0;

	[Tooltip("The name of a float in the used shader (only change if the shader changed)")]
	public string OuterCircle = "_OuterDiameter";
	private float outerValue = 0;
	private float maxOuterValue = 0;
	private float minOuterValue = 0;
	private float lastOuterValue = 0;

	[Tooltip("The name of a float in the used shader (only change if the shader changed)")]
	public string Distance = "_DistanceInMeters";
	private float distanceValue = 0;
	private float theSameFor = 0;
	private LoockingState State=LoockingState.normal;

    private bool isInitialized = false;
    private bool hasResetMax = false;
    private bool hasStayed = false;

    void Start ()
    {
		GvrBasePointer ParentObject = GameObject.FindObjectOfType<GvrBasePointer> ();
		if (ParentObject != null)
        {
			this.transform.parent = ParentObject.transform;
		
			if (RotationDirection == RotationDirection.clockwise)
            {
				this.transform.localScale = new Vector3 (1, 1, 1);
			}
            else
            {
				this.transform.localScale = new Vector3 (-1, 1, 1);
			}
			this.transform.localPosition = new Vector3 (0, 0, 0);

			switch (RotationStart)
            {
			    case RotationStart.top:
				    this.transform.localRotation = Quaternion.identity;
				    break;
			    case RotationStart.left:
				    this.transform.localRotation = Quaternion.Euler (new Vector3 (0, 0, 90));
				    break;
			    case RotationStart.bottom:
				    this.transform.localRotation = Quaternion.Euler (new Vector3 (0, 0, 180));
				    break;
			    case RotationStart.right:
				    this.transform.localRotation = Quaternion.Euler (new Vector3 (0, 0, 270));
				    break;
			    default:
				    break;
			}

			EventSystem = GameObject.FindObjectOfType<EventSystem> ();
			Renderer = this.GetComponent<MeshRenderer> ();
			Renderer.enabled = false;
			Parent = this.transform.parent;
			Source = Parent.gameObject.GetComponent<Renderer> ().material;
			Copy = this.gameObject.GetComponent<Renderer> ().material;
			OriginalColor = Source.GetColor ("_Color");

			this.gameObject.AddComponent<MeshFilter> ();
			RingMesh = new Mesh ();
			GetComponent<MeshFilter> ().mesh = RingMesh;
		}
        else
        {
			throw new UnityException ("There is no GvrReticlePointer in the Scene. Please add the GvrReticlePointer under GvrMain/Head/MainCamera.");
		}
	}

	void OnEnable()
    {
		isInitialized = false;
	}

	private void Initialize()
    {
		GazePointer = GvrPointerManager.Pointer;
		GvrPointerManager.Pointer = this;

		TargetMesh = Parent.GetComponent<MeshFilter> ().mesh;
		RingMesh.vertices = TargetMesh.vertices;
		RingMesh.triangles = TargetMesh.triangles;
		RingMesh.RecalculateBounds();
		Color[] Colors = new Color[RingMesh.vertices.Length];
		for (int i = 0; i < Colors.Length; i++)
        {
			if (UseOriginalColor)
            {
				Colors [i] = new Color (OriginalColor.r, OriginalColor.g, OriginalColor.b, 1);
			}
            else
            {
				Colors [i] = new Color (FillColor.r, FillColor.g, FillColor.b, 1);
			}
		}
		RingMesh.colors = Colors;
		isInitialized = true;
	}

	void Update ()
    {
		if (!isInitialized)
        {
			Initialize ();
		}	

		if (!hasResetMax&&IsMin())
        {
			maxOuterValue = 0;
			maxInnerValue = 0;
		}

		innerValue = Source.GetFloat (InnerCircle);
		outerValue = Source.GetFloat (OuterCircle);
		distanceValue = Source.GetFloat (Distance);

		hasStayed = IsSame (innerValue, lastInnerValue) && IsSame (outerValue, lastOuterValue);

		lastInnerValue = innerValue;
		lastOuterValue = outerValue;

		if (hasStayed)
        {
			theSameFor += Time.deltaTime;
		}
        else
        {
			theSameFor = 0;
		}

		switch (State)
        {
		    case LoockingState.normal:
			    HandleNormalState ();
			    break;
		    case LoockingState.spinning:
			    HandleSpinningState ();
			    break;
		    case LoockingState.finished:
			    HandleFinishedState ();
			    break;
		    case LoockingState.clicked:
			    HandleClickedState ();
			    break;
		    default:
			    break;
		}
	}

	private void HandleNormalState()
    {
		if (innerValue > maxInnerValue)
        {
			maxInnerValue = innerValue;
		}
		if (innerValue < minInnerValue)
        {
			minInnerValue = innerValue;
		}
		if (outerValue > maxOuterValue)
        {
			maxOuterValue = outerValue;
		}
		if (outerValue < minOuterValue)
        {
			minOuterValue = outerValue;
		}
		if (theSameFor > DelayBeforeRotation && !IsMin () && Target!=null)
        {
			State = LoockingState.spinning;
		}
	}

	private void HandleSpinningState()
    {
		if (circleState>=1)
        {
			State = LoockingState.finished;
			circleState = 1;
		}
		if (hasStayed && Target!=null)
        {
			SetFillValue (circleState);
			circleState = circleState + Time.deltaTime / RotationDuration;
		}
        else
        {
			Reset ();
		}
	}

	private void HandleFinishedState(){
		
		if (hasStayed)
        {
			if (theSameFor>=DelayBeforeClick)
            {
				Click ();
			}
		}
        else
        {
			Reset ();
		}
	}

	private void HandleClickedState()
    {
		if (!hasStayed)
        {
			Reset ();
		}
	}

	private void Reset()
    {
		for (int i = 0; i < RingMesh.colors.Length; i++)
        {
			if (UseOriginalColor)
            {
				RingMesh.colors [i] = new Color (OriginalColor.r, OriginalColor.g, OriginalColor.b, 1);
			}
            else
            {
				RingMesh.colors [i] = new Color (FillColor.r, FillColor.g, FillColor.b, 1);
			}
		}
		Renderer.enabled = false;
		Source.SetColor ("_Color", OriginalColor);
		State= LoockingState.normal;
		circleState=0;
	}

	private void Click()
    {
		State = LoockingState.clicked;
		PointerEventData pointerData = new PointerEventData (EventSystem);
		pointerData.button = PointerEventData.InputButton.Left;
		if (Target!=null)
        {
			ExecuteEvents.ExecuteHierarchy (Target, pointerData, ExecuteEvents.pointerDownHandler);
			StartCoroutine (ClickNow());
		}		 
	}

	IEnumerator ClickNow()
    {
		yield return new WaitForSecondsRealtime (0.1f);
		PointerEventData pointerData = new PointerEventData (EventSystem);
		pointerData.button = PointerEventData.InputButton.Left;
		if (Target!=null)
        {
			ExecuteEvents.ExecuteHierarchy (Target, pointerData, ExecuteEvents.pointerClickHandler);
			ExecuteEvents.ExecuteHierarchy (Target, pointerData, ExecuteEvents.pointerUpHandler);
		}	
	}


	private void SetFillValue(float amount, float start=0)
    {
		Copy.SetFloat ("_InnerRing",innerValue);
		Copy.SetFloat ("_OuterRing",outerValue);
		Copy.SetFloat ("_Distance",distanceValue);
		Color[] Colors= new Color[RingMesh.vertices.Length];

		int max = Mathf.FloorToInt (((float)RingMesh.vertices.Length)/2 * amount)*2;
		for (int i = 0; i < max; i++)
        {
			Colors [i] = new Color (0, 0, 0, 0);
		}
		if (max<RingMesh.vertices.Length-1)
        {
			float transparentValue = 1.0f-(amount * RingMesh.vertices.Length - max) / 2;
			if (UseOriginalColor)
            {	
				Colors [max] = new Color (OriginalColor.r, OriginalColor.g, OriginalColor.b, transparentValue);
				Colors [max + 1] = new Color (OriginalColor.r, OriginalColor.g, OriginalColor.b, transparentValue);
			}
            else
            {
				Colors [max] = new Color (FillColor.r, FillColor.g, FillColor.b, transparentValue);
				Colors [max + 1] = new Color (FillColor.r, FillColor.g, FillColor.b, transparentValue);
			}
		}
	
		for (int i = max+2; i < RingMesh.vertices.Length; i++)
        {
			if (UseOriginalColor)
            {
				Colors [i] = OriginalColor;
			}
            else
            {
				Colors [i] = FillColor;
			}
		}

		RingMesh.colors=Colors;

		Renderer.enabled = true;
		if (UseOriginalColor)
        {
			Source.SetColor ("_Color", new Color (OriginalColor.r, OriginalColor.g, OriginalColor.b, BackgroundAlpha));
		}
        else
        {
			Source.SetColor ("_Color", BackgroundColor);
		}
	}

	private bool IsSame (float oldValue, float newValue)
    {
		if (oldValue*(1-Precision)>newValue)
        {
			return false;
		}
		if (oldValue*(1+Precision) < newValue)
        {
			return false;
		}

		return true;
	}

	private bool IsMax()
    {
		if (!IsSame(maxInnerValue,innerValue) && !IsSame(maxOuterValue,outerValue))
        {
			return false;
		}
		if (IsSame(minInnerValue,maxInnerValue)||IsSame(minOuterValue,maxOuterValue))
        {
			return false;
		}

		return true;
	}

	private bool IsMin()
    {
		if (!IsSame(minInnerValue,innerValue) && !IsSame(minInnerValue,outerValue))
        {
			return false;
		}
		if (IsSame(minInnerValue,maxInnerValue)||IsSame(minOuterValue,maxOuterValue))
        {
			return false;
		}

		return true;
	}

	#region IGvrPointer implementation


	public void OnInputModuleEnabled ()
	{
		GazePointer.OnInputModuleEnabled ();
	}

	public void OnInputModuleDisabled ()
	{
		GazePointer.OnInputModuleEnabled ();
	}

	public void OnPointerEnter (GameObject targetObject, Vector3 intersectionPosition, Ray intersectionRay, bool isInteractive)
	{
		GazePointer.OnPointerEnter (targetObject,intersectionPosition,intersectionRay,isInteractive);
		if (isInteractive)
        {
			Target = targetObject;
		}
        else
        {
			Target = null;
		}
	}

	public void OnPointerHover (GameObject targetObject, Vector3 intersectionPosition, Ray intersectionRay, bool isInteractive)
	{
		GazePointer.OnPointerHover (targetObject,intersectionPosition,intersectionRay,isInteractive);
	}

	public void OnPointerExit (GameObject targetObject)
	{
		GazePointer.OnPointerExit (targetObject);
		Target = null;
	}

	public void OnPointerClickDown ()
	{
		GazePointer.OnPointerClickDown ();
	}

	public void OnPointerClickUp ()
	{
		GazePointer.OnPointerClickUp ();
	}

	public float GetMaxPointerDistance ()
	{
		return GazePointer.GetMaxPointerDistance ();
	}

	public Transform GetPointerTransform ()
	{
		return GazePointer.GetPointerTransform ();
	}

	public void GetPointerRadius(out float innerRadius, out float outerRadius) {

		GazePointer.GetPointerRadius (out innerRadius, out outerRadius);
	}

	#endregion
}
	

public enum LoockingState
{
	normal,
	spinning,
	finished,
	clicked
}

public enum RotationDirection
{
	clockwise,
	counterclockwise
}

public enum RotationStart
{
	top,
	bottom,
	right,
	left
}