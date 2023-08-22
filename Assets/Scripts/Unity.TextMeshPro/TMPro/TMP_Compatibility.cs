namespace TMPro
{
	public static class TMP_Compatibility
	{
		public enum AnchorPositions
		{
			TopLeft = 0,
			Top = 1,
			TopRight = 2,
			Left = 3,
			Center = 4,
			Right = 5,
			BottomLeft = 6,
			Bottom = 7,
			BottomRight = 8,
			BaseLine = 9,
			None = 10
		}

		public static TextAlignmentOptions ConvertTextAlignmentEnumValues(TextAlignmentOptions oldValue)
		{
			switch ((int)oldValue)
			{
			case 0:
				return TextAlignmentOptions.TopLeft;
			case 1:
				return TextAlignmentOptions.Top;
			case 2:
				return TextAlignmentOptions.TopRight;
			case 3:
				return TextAlignmentOptions.TopJustified;
			case 4:
				return TextAlignmentOptions.Left;
			case 5:
				return TextAlignmentOptions.Center;
			case 6:
				return TextAlignmentOptions.Right;
			case 7:
				return TextAlignmentOptions.Justified;
			case 8:
				return TextAlignmentOptions.BottomLeft;
			case 9:
				return TextAlignmentOptions.Bottom;
			case 10:
				return TextAlignmentOptions.BottomRight;
			case 11:
				return TextAlignmentOptions.BottomJustified;
			case 12:
				return TextAlignmentOptions.BaselineLeft;
			case 13:
				return TextAlignmentOptions.Baseline;
			case 14:
				return TextAlignmentOptions.BaselineRight;
			case 15:
				return TextAlignmentOptions.BaselineJustified;
			case 16:
				return TextAlignmentOptions.MidlineLeft;
			case 17:
				return TextAlignmentOptions.Midline;
			case 18:
				return TextAlignmentOptions.MidlineRight;
			case 19:
				return TextAlignmentOptions.MidlineJustified;
			case 20:
				return TextAlignmentOptions.CaplineLeft;
			case 21:
				return TextAlignmentOptions.Capline;
			case 22:
				return TextAlignmentOptions.CaplineRight;
			case 23:
				return TextAlignmentOptions.CaplineJustified;
			default:
				return TextAlignmentOptions.TopLeft;
			}
		}
	}
}
