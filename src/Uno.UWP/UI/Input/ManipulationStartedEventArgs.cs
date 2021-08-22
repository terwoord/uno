using System.Linq;
using Windows.Devices.Input;
using Windows.Foundation;

namespace Windows.UI.Input
{
	public partial class ManipulationStartedEventArgs 
	{
		internal ManipulationStartedEventArgs(
			PointerIdentifier[] pointers,
			Point position,
			ManipulationDelta cumulative,
			uint contactCount)
		{
			global::System.Diagnostics.Debug.Assert(contactCount == pointers.Length, "We should have the same number of pointers for the manip start.");
			global::System.Diagnostics.Debug.Assert(pointers.Length > 0 && pointers.All(p => p.Type == pointers[0].Type));

			Pointers = pointers;
			PointerDeviceType = pointers[0].Type;
			Position = position;
			Cumulative = cumulative;
			ContactCount = contactCount;
		}

		/// <summary>
		/// Gets identifiers of all pointer that has been involved in that manipulation.
		/// </summary>
		/// <remarks>All pointers are expected to have the same <see cref="PointerIdentifier.Type"/>.</remarks>
		internal PointerIdentifier[] Pointers { get; }

		public PointerDeviceType PointerDeviceType { get; }
		public Point Position { get; }
		public ManipulationDelta Cumulative { get; }
		public uint ContactCount { get; }
	}
}
