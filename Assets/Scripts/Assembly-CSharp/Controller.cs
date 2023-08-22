using UnityEngine;

public class Controller
{
	public class TouchState
	{
		public Vector2 DownAt;

		public Vector2 Position;

		public bool WasDown;

		public bool IsDown;

		public bool TouchStart;

		public bool TouchEnd;
	}

	public readonly TouchState[] Touches = new TouchState[2];

	private Collider2D amTouching;

	private int touchId = -1;

	public bool AnyTouch
	{
		get
		{
			return Touches[0].IsDown || Touches[1].IsDown;
		}
	}

	public bool AnyTouchDown
	{
		get
		{
			return Touches[0].TouchStart || Touches[1].TouchStart;
		}
	}

	public bool AnyTouchUp
	{
		get
		{
			return Touches[0].TouchEnd || Touches[1].TouchEnd;
		}
	}

	public bool FirstDown
	{
		get
		{
			return Touches[0].TouchStart;
		}
	}

	public Vector2 DragPosition
	{
		get
		{
			return Touches[touchId].Position;
		}
	}

	public Vector2 DragStartPosition
	{
		get
		{
			return Touches[touchId].DownAt;
		}
	}

	public Controller()
	{
		for (int i = 0; i < Touches.Length; i++)
		{
			Touches[i] = new TouchState();
		}
	}

	public DragState CheckDrag(Collider2D coll, bool firstOnly = false)
	{
		if (!coll)
		{
			return DragState.NoTouch;
		}
		if (touchId > -1)
		{
			if (coll != amTouching)
			{
				return DragState.NoTouch;
			}
			TouchState touchState = Touches[touchId];
			if (touchState.IsDown)
			{
				return DragState.Dragging;
			}
			amTouching = null;
			touchId = -1;
			return DragState.Released;
		}
		if (firstOnly)
		{
			TouchState touchState2 = Touches[0];
			if (touchState2.TouchStart && coll.OverlapPoint(touchState2.Position))
			{
				amTouching = coll;
				touchId = 0;
				return DragState.TouchStart;
			}
		}
		else
		{
			for (int i = 0; i < Touches.Length; i++)
			{
				TouchState touchState3 = Touches[i];
				if (touchState3.TouchStart && coll.OverlapPoint(touchState3.Position))
				{
					amTouching = coll;
					touchId = i;
					return DragState.TouchStart;
				}
			}
		}
		return DragState.NoTouch;
	}

	public void Update()
	{
		TouchState touchState = Touches[0];
		bool mouseButton = Input.GetMouseButton(0);
		touchState.Position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		touchState.TouchStart = !touchState.IsDown && mouseButton;
		if (touchState.TouchStart)
		{
			touchState.DownAt = touchState.Position;
		}
		touchState.TouchEnd = touchState.IsDown && !mouseButton;
		touchState.IsDown = mouseButton;
	}

	public void Reset()
	{
		for (int i = 0; i < Touches.Length; i++)
		{
			Touches[i] = new TouchState();
		}
		touchId = -1;
		amTouching = null;
	}

	public TouchState GetTouch(int i)
	{
		return Touches[i];
	}
}
