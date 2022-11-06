// Verse.Dialog_Slider
using System;
using UnityEngine;
using Verse;

public class Dialog_PrintBars : Window
{
	public Func<int, string> textGetter;

	public int from;

	public int to;

	private bool forbid;

	private bool rear;

	public float roundTo = 1f;

	private Action<int, bool, bool> confirmAction;

	private int curValue;

	private const float BotAreaHeight = 30f;

	private const float TopPadding = 15f;

	public override Vector2 InitialSize => new Vector2(300f, 230f);

	public Dialog_PrintBars(Func<int, string> textGetter, int from, int to, Action<int, bool, bool> confirmAction, int startingValue = int.MinValue, float roundTo = 1f)
	{
		this.textGetter = textGetter;
		this.from = from;
		this.to = to;
		this.confirmAction = confirmAction;
		this.roundTo = roundTo;
		this.forbid = false;
		this.rear = false;
		forcePause = true;
		closeOnClickedOutside = true;
		if (startingValue == int.MinValue)
		{
			curValue = from;
		}
		else
		{
			curValue = startingValue;
		}
	}

	public Dialog_PrintBars(string text, int from, int to, Action<int, bool, bool> confirmAction, int startingValue = int.MinValue, float roundTo = 1f)
		: this((int val) => string.Format(text, val), from, to, confirmAction, startingValue, roundTo)
	{
	}

	//Defines the layout of the printing window
	public override void DoWindowContents(Rect inRect)
	{
		Rect rect = new Rect(inRect.x, inRect.y + 30f, inRect.width, 30f);
		Rect forbidCheckbox = new Rect(inRect.x, inRect.y + 90f, inRect.width, 30f);
		Rect rearCheckbox = new Rect(inRect.x, inRect.y + 120f, inRect.width, 30f);
		curValue = (int)Widgets.HorizontalSlider(rect, curValue, from, to, middleAlignment: true, textGetter(curValue), null, null, roundTo);
		float sepY = inRect.y - 5f;
		float sepY2 = inRect.y + 60f;
		Widgets.ListSeparator(ref sepY, 280, "BarQuantity".Translate());
		Widgets.ListSeparator(ref sepY2, 280, "PrintSettings".Translate());
		Widgets.CheckboxLabeled(forbidCheckbox, "PrintForbidden".Translate(), checkOn: ref forbid);
		Widgets.CheckboxLabeled(rearCheckbox, "PrintRear".Translate(), checkOn: ref rear);
		Text.Font = GameFont.Small;
		if (Widgets.ButtonText(new Rect(inRect.x, inRect.yMax - 32f, inRect.width / 2f, 30f), "CancelButton".Translate()))
		{
			Close();
		}
		if (Widgets.ButtonText(new Rect(inRect.x + inRect.width / 2f, inRect.yMax - 32f, inRect.width / 2f, 30f), "ConfirmPrint".Translate()))
		{
			Close();
			confirmAction(curValue, forbid, rear);
		}
	}
}
