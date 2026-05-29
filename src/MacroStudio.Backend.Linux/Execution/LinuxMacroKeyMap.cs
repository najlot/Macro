namespace MacroStudio.Backend.Linux.Execution;

internal static class LinuxMacroKeyMap
{
	private static readonly KeyDefinition[] Definitions =
	[
		new(8, 0xFF08),
		new(9, 0xFF09),
		new(13, 0xFF0D),
		new(16, 0xFFE1, 0xFFE2),
		new(17, 0xFFE3, 0xFFE4),
		new(18, 0xFFE9, 0xFFEA),
		new(19, 0xFF13),
		new(20, 0xFFE5),
		new(27, 0xFF1B),
		new(32, 0x0020),
		new(33, 0xFF55),
		new(34, 0xFF56),
		new(35, 0xFF57),
		new(36, 0xFF50),
		new(37, 0xFF51),
		new(38, 0xFF52),
		new(39, 0xFF53),
		new(40, 0xFF54),
		new(45, 0xFF63),
		new(46, 0xFFFF),
		new(48, 0x0030),
		new(49, 0x0031),
		new(50, 0x0032),
		new(51, 0x0033),
		new(52, 0x0034),
		new(53, 0x0035),
		new(54, 0x0036),
		new(55, 0x0037),
		new(56, 0x0038),
		new(57, 0x0039),
		new(65, 0x0041, 0x0061),
		new(66, 0x0042, 0x0062),
		new(67, 0x0043, 0x0063),
		new(68, 0x0044, 0x0064),
		new(69, 0x0045, 0x0065),
		new(70, 0x0046, 0x0066),
		new(71, 0x0047, 0x0067),
		new(72, 0x0048, 0x0068),
		new(73, 0x0049, 0x0069),
		new(74, 0x004A, 0x006A),
		new(75, 0x004B, 0x006B),
		new(76, 0x004C, 0x006C),
		new(77, 0x004D, 0x006D),
		new(78, 0x004E, 0x006E),
		new(79, 0x004F, 0x006F),
		new(80, 0x0050, 0x0070),
		new(81, 0x0051, 0x0071),
		new(82, 0x0052, 0x0072),
		new(83, 0x0053, 0x0073),
		new(84, 0x0054, 0x0074),
		new(85, 0x0055, 0x0075),
		new(86, 0x0056, 0x0076),
		new(87, 0x0057, 0x0077),
		new(88, 0x0058, 0x0078),
		new(89, 0x0059, 0x0079),
		new(90, 0x005A, 0x007A),
		new(91, 0xFFEB),
		new(92, 0xFFEC),
		new(93, 0xFF67),
		new(112, 0xFFBE),
		new(113, 0xFFBF),
		new(114, 0xFFC0),
		new(115, 0xFFC1),
		new(116, 0xFFC2),
		new(117, 0xFFC3),
		new(118, 0xFFC4),
		new(119, 0xFFC5),
		new(120, 0xFFC6),
		new(121, 0xFFC7),
		new(122, 0xFFC8),
		new(123, 0xFFC9),
		new(186, 0x003B, 0x003A),
		new(187, 0x003D, 0x002B),
		new(188, 0x002C, 0x003C),
		new(189, 0x002D, 0x005F),
		new(190, 0x002E, 0x003E),
		new(191, 0x002F, 0x003F),
		new(192, 0x0060, 0x007E),
		new(219, 0x005B, 0x007B),
		new(220, 0x005C, 0x007C),
		new(221, 0x005D, 0x007D),
		new(222, 0x0027, 0x0022)
	];

	public static int[] SupportedMacroKeys { get; } = Definitions.Select(definition => definition.MacroKey).ToArray();

	public static IReadOnlyDictionary<int, ushort[]> Build(IntPtr display)
	{
		var result = new Dictionary<int, ushort[]>(Definitions.Length);
		foreach (var definition in Definitions)
		{
			var keycodes = definition.KeySyms
				.Select(keySym => (ushort)X11AutomationContext.XKeysymToKeycode(display, keySym))
				.Where(keycode => keycode > 0)
				.Distinct()
				.ToArray();

			if (keycodes.Length > 0)
			{
				result[definition.MacroKey] = keycodes;
			}
		}

		return result;
	}

	private sealed record KeyDefinition(int MacroKey, params nuint[] KeySyms);
}