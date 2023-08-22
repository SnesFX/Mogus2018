using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class TextBox : MonoBehaviour, IFocusHolder
{
	public static readonly char[] SymbolChars = new char[16]
	{
		'?', '!', ',', '.', '\'', ':', ';', '(', ')', '/',
		'\\', '%', '^', '&', '-', '='
	};

	public string text = string.Empty;

	public int characterLimit = -1;

	public TextRenderer outputText;

	public SpriteRenderer Background;

	public MeshRenderer Pipe;

	private float pipeBlinkTimer;

	public bool ClearOnFocus;

	public bool ForceUppercase;

	public Button.ButtonClickedEvent OnEnter;

	public Button.ButtonClickedEvent OnChange;

	private TouchScreenKeyboard keyboard;

	private HashSet<char> allowedChars;

	public bool AllowSymbols;

	private Collider2D[] colliders;

	private bool hasFocus;

	private StringBuilder tempTxt = new StringBuilder();

	public void Start()
	{
		allowedChars = new HashSet<char>();
		for (int i = 65; i <= 90; i++)
		{
			allowedChars.Add((char)i);
		}
		for (int j = 97; j <= 122; j++)
		{
			allowedChars.Add((char)j);
		}
		for (int k = 48; k <= 57; k++)
		{
			allowedChars.Add((char)k);
		}
		allowedChars.Add(' ');
		if (AllowSymbols)
		{
			for (int l = 0; l < SymbolChars.Length; l++)
			{
				allowedChars.Add(SymbolChars[l]);
			}
		}
		colliders = GetComponents<Collider2D>();
		DestroyableSingleton<PassiveButtonManager>.Instance.RegisterOne(this);
		if ((bool)Pipe)
		{
			Pipe.enabled = false;
		}
	}

	public void OnDestroy()
	{
		if (DestroyableSingleton<PassiveButtonManager>.InstanceExists)
		{
			DestroyableSingleton<PassiveButtonManager>.Instance.RemoveOne(this);
		}
	}

	public void Clear()
	{
		SetText(string.Empty);
	}

	public void Update()
	{
		if (!hasFocus)
		{
			return;
		}
		string inputString = Input.inputString;
		if (inputString.Length > 0)
		{
			if (text == null || text == "Enter Name")
			{
				text = string.Empty;
			}
			SetText(text + inputString);
		}
		if ((bool)Pipe && hasFocus)
		{
			pipeBlinkTimer += Time.deltaTime * 2f;
			Pipe.enabled = (int)pipeBlinkTimer % 2 == 0;
		}
	}

	public void GiveFocus()
	{
		if (!hasFocus)
		{
			if (ClearOnFocus)
			{
				text = string.Empty;
				outputText.Text = string.Empty;
			}
			hasFocus = true;
			if (TouchScreenKeyboard.isSupported)
			{
				keyboard = TouchScreenKeyboard.Open(text);
			}
			if ((bool)Background)
			{
				Background.color = Color.green;
			}
			pipeBlinkTimer = 0f;
			if ((bool)Pipe)
			{
				Pipe.transform.localPosition = outputText.CursorPos;
			}
		}
	}

	public void LoseFocus()
	{
		hasFocus = false;
		keyboard = null;
		if ((bool)Background)
		{
			Background.color = Color.white;
		}
		if ((bool)Pipe)
		{
			Pipe.enabled = false;
		}
	}

	public bool CheckCollision(Vector2 pt)
	{
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i].OverlapPoint(pt))
			{
				return true;
			}
		}
		return false;
	}

	public void SetText(string input)
	{
		bool flag = false;
		char c = ' ';
		tempTxt.Clear();
		for (int i = 0; i < input.Length; i++)
		{
			char c2 = input[i];
			if (c != ' ' || c2 != ' ')
			{
				if (c2 == '\r' || c2 == '\n')
				{
					flag = true;
				}
				if (c2 == '\b')
				{
					tempTxt.Length = Math.Max(tempTxt.Length - 1, 0);
				}
				if (ForceUppercase)
				{
					c2 = char.ToUpperInvariant(c2);
				}
				if (allowedChars.Contains(c2))
				{
					tempTxt.Append(c2);
					c = c2;
				}
			}
		}
		tempTxt.Length = Math.Min(tempTxt.Length, characterLimit);
		input = tempTxt.ToString();
		if (!input.Equals(text))
		{
			text = input;
			outputText.Text = text;
			outputText.RefreshMesh();
			if (keyboard != null)
			{
				keyboard.text = text;
			}
			OnChange.Invoke();
		}
		if (flag)
		{
			OnEnter.Invoke();
		}
		if ((bool)Pipe)
		{
			Pipe.transform.localPosition = outputText.CursorPos;
		}
	}
}
