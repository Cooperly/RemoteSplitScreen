using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace RemoteSplitScreen.Helpers;

public static class LabelEx {
	// The field name seems to differ on different platforms:
	//     - Rider reports "internal int label".
	//     - Avalonia (with mscorlib from RustDedicated) reports "internal readonly int m_label".
	//     - Production reports "internal int label".
	//
	// As such, we'll just iterate over every single declared field until we find the one that we want.
	// It's not pretty, but then again, when has reflection ever been pretty? :D
	private static readonly FieldInfo Field = typeof(Label)
		.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
		.First(it => it.FieldType == typeof(int) && it.Name is "m_label" or "label");

	/// <summary>
	///     Creates a new label with the specified inner value.
	/// </summary>
	/// <param name="value">The inner value for the label.</param>
	/// <returns>A new label.</returns>
	public static Label CreateLabel(int value) {
		var constructor = AccessTools.Constructor(typeof(Label), [typeof(int)]);
		return (Label) constructor.Invoke([value]);
	}

	extension(Label label) {
		public int Value {
			get => (int) Field.GetValue(label);
			set => Field.SetValue(label, value);
		}
	}
}
